using Land_Readjustment_Tool.Models;
using System.Text;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Owner deduplication with strict identity rules.
    /// Design goals:
    /// - Never auto-merge institutional owners with person owners.
    /// - Prefer deterministic keys (citizenship, exact normalized name/father) over fuzzy merges.
    /// - Keep merged data safe: institution owners do not carry person-only attributes.
    /// </summary>
    public class OwnerDeduplicationService
    {
        private const double HighNameThresholdForCitizenshipRule = 0.80;
        private const double HighNameFatherCombinedThreshold = 0.95;
        private const double MinimumReviewThreshold = 0.75;

        private static readonly string[] AnonymousKeywords =
        [
            "anonymous", "unknown", "अज्ञात", "अनाम"
        ];

        private static readonly string[] InstitutionKeywords =
        [
            "नेपाल सरकार", "सरकार", "government", "govt", "sarkar",
            "ministry", "department", "कार्यालय", "मन्त्रालय", "विभाग",
            "नगरपालिका", "गाउँपालिका", "गाउपालिका", "गा.पा", "न.पा",
            "वडा कार्यालय", "सार्वजनिक", "public", "committee", "समिति",
            "trust", "गुठी", "school", "विद्यालय", "bank", "कम्पनी", "company", "ltd", "pvt"
        ];

        private enum OwnerCategory
        {
            Unknown = 0,
            Person = 1,
            Institution = 2
        }

        public class DeduplicationResult
        {
            public List<UniqueOwner> UniqueOwners { get; set; } = new();
            public List<DuplicateGroup> DuplicatesNeedingReview { get; set; } = new();
            public List<DuplicateGroup> AutoMergedGroups { get; set; } = new();
            public int TotalOriginalRecords { get; set; }
            public int AnonymousOwnersCreated { get; set; }
            public int AutoMergedCount { get; set; }
        }

        public class UniqueOwner
        {
            public string LandOwnersName { get; set; } = string.Empty;
            public string? FatherSpouse { get; set; }
            public string? Gender { get; set; }
            public string? CitizenshipNumber { get; set; }
            public string? CitizenshipIssuedDistrict { get; set; }
            public string? CitizenshipIssuedDate { get; set; }
            public string? PermanentAddress { get; set; }
            public string? TemporaryAddress { get; set; }
            public string? ContactNumber { get; set; }
            public string? EmailID { get; set; }
            public List<int> ParcelIndices { get; set; } = new();
            public bool IsAnonymous { get; set; }
            public int CompletenessScore => GetCompletenessScore(this);
        }

        public class DuplicateGroup
        {
            public List<UniqueOwner> Owners { get; set; } = new();
            public double Similarity { get; set; }
            public double CitizenshipConfidence { get; set; }
            public double NameFatherConfidence { get; set; }
            public UniqueOwner? AutoMergedOwner { get; set; }
            public bool IsAutoMerged { get; set; }
            public string MatchType { get; set; } = string.Empty;
            public UserDecisionType? UserDecision { get; set; }
        }

        public enum UserDecisionType
        {
            Merge,
            KeepSeparate
        }

        private enum ConfidenceBand
        {
            None = 0,
            Medium = 1,
            High = 2
        }

        private sealed class OwnerToken
        {
            public required int Index { get; init; }
            public required UniqueOwner Owner { get; init; }
            public required string NormalizedName { get; init; }
            public required string NormalizedFather { get; init; }
            public required string NormalizedCitizenship { get; init; }
        }

        private sealed class PairAssessment
        {
            public required int LeftIndex { get; init; }
            public required int RightIndex { get; init; }
            public required ConfidenceBand Band { get; init; }
            public required double Similarity { get; init; }
            public required double CitizenshipConfidence { get; init; }
            public required double NameFatherConfidence { get; init; }
            public required bool UsesCitizenship { get; init; }
            public required string MatchType { get; init; }
        }

        public static DeduplicationResult ExtractUniqueOwners(
            List<BaselineLandParceRecord> records,
            bool excludeAnonymous = false)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            var result = new DeduplicationResult
            {
                TotalOriginalRecords = records.Count
            };

            var seedOwners = CreateInitialOwnerList(records, result, excludeAnonymous);
            var anonymousOwners = seedOwners.Where(o => o.IsAnonymous).ToList();
            var institutionOwners = seedOwners
                .Where(o => !o.IsAnonymous && DetermineOwnerCategory(o) == OwnerCategory.Institution)
                .ToList();
            var personOwners = seedOwners
                .Where(o => !o.IsAnonymous && DetermineOwnerCategory(o) == OwnerCategory.Person)
                .ToList();

            // Anonymous owners are intentionally isolated.
            result.UniqueOwners.AddRange(anonymousOwners.Select(CloneOwner));

            // Institutions: only exact normalized-name grouping.
            var institutionGroups = institutionOwners
                .GroupBy(o => NormalizeString(o.LandOwnersName), StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var institutionGroup in institutionGroups)
            {
                var members = institutionGroup.ToList();
                if (members.Count == 1)
                {
                    result.UniqueOwners.Add(CloneOwner(members[0]));
                    continue;
                }

                var mergedOwner = MergeOwners(members);
                result.UniqueOwners.Add(mergedOwner);

                var group = new DuplicateGroup
                {
                    Owners = members,
                    Similarity = 1.0,
                    CitizenshipConfidence = 0.0,
                    NameFatherConfidence = 1.0,
                    AutoMergedOwner = mergedOwner,
                    IsAutoMerged = true,
                    MatchType = "High Confidence (Institution Name Exact Match)"
                };

                result.AutoMergedGroups.Add(group);
                result.DuplicatesNeedingReview.Add(group);
                result.AutoMergedCount++;
            }

            var personTokens = BuildOwnerTokens(personOwners);
            var highPairs = FindPairAssessments(personTokens, ConfidenceBand.High);
            var highComponents = BuildConnectedComponents(personTokens.Count, highPairs);

            var processedPersonIndexes = new HashSet<int>();
            foreach (var component in highComponents)
            {
                if (component.Count == 1)
                {
                    var single = personTokens[component[0]].Owner;
                    result.UniqueOwners.Add(CloneOwner(single));
                    _ = processedPersonIndexes.Add(component[0]);
                    continue;
                }

                var ownersToMerge = component.Select(index => personTokens[index].Owner).ToList();
                var mergedOwner = MergeOwners(ownersToMerge);
                result.UniqueOwners.Add(mergedOwner);

                var componentPairs = highPairs
                    .Where(p => component.Contains(p.LeftIndex) && component.Contains(p.RightIndex))
                    .ToList();

                var group = new DuplicateGroup
                {
                    Owners = ownersToMerge,
                    Similarity = componentPairs.Count > 0 ? componentPairs.Max(p => p.Similarity) : 1.0,
                    CitizenshipConfidence = componentPairs.Count > 0 ? componentPairs.Max(p => p.CitizenshipConfidence) : 0.0,
                    NameFatherConfidence = componentPairs.Count > 0 ? componentPairs.Max(p => p.NameFatherConfidence) : 1.0,
                    AutoMergedOwner = mergedOwner,
                    IsAutoMerged = true,
                    MatchType = "High Confidence (Citizenship + Name/Father)"
                };

                result.AutoMergedGroups.Add(group);
                result.DuplicatesNeedingReview.Add(group);
                result.AutoMergedCount++;

                foreach (var index in component)
                {
                    _ = processedPersonIndexes.Add(index);
                }
            }

            for (int i = 0; i < personTokens.Count; i++)
            {
                if (processedPersonIndexes.Contains(i))
                {
                    continue;
                }

                result.UniqueOwners.Add(CloneOwner(personTokens[i].Owner));
            }

            // Medium confidence groups are always sent to manual review.
            var mediumReviewCandidates = result.UniqueOwners
                .Where(o => IsManualReviewCandidate(o))
                .ToList();

            var reviewSuggestions = BuildManualReviewSuggestions(mediumReviewCandidates);
            result.DuplicatesNeedingReview.AddRange(reviewSuggestions);
            foreach (var auto in reviewSuggestions.Where(g => g.IsAutoMerged))
            {
                result.AutoMergedGroups.Add(auto);
                result.AutoMergedCount++;
            }

            return result;
        }

        private static List<DuplicateGroup> BuildManualReviewSuggestions(List<UniqueOwner> uniqueOwners)
        {
            var suggestions = new List<DuplicateGroup>();
            var tokens = BuildOwnerTokens(uniqueOwners);
            var mediumPairs = FindPairAssessments(tokens, ConfidenceBand.Medium);
            if (mediumPairs.Count == 0)
            {
                return suggestions;
            }

            var mediumComponents = BuildConnectedComponents(tokens.Count, mediumPairs);
            foreach (var component in mediumComponents.Where(c => c.Count > 1))
            {
                var owners = component.Select(index => tokens[index].Owner).ToList();
                var componentPairs = mediumPairs
                    .Where(p => component.Contains(p.LeftIndex) && component.Contains(p.RightIndex))
                    .ToList();

                var exactNameFatherMatch = IsExactNameFatherGroup(owners);
                var autoMergedOwner = exactNameFatherMatch ? MergeOwners(owners) : null;

                suggestions.Add(new DuplicateGroup
                {
                    Owners = owners,
                    Similarity = componentPairs.Max(p => p.Similarity),
                    CitizenshipConfidence = componentPairs.Max(p => p.CitizenshipConfidence),
                    NameFatherConfidence = componentPairs.Max(p => p.NameFatherConfidence),
                    IsAutoMerged = exactNameFatherMatch,
                    AutoMergedOwner = autoMergedOwner,
                    MatchType = exactNameFatherMatch
                        ? "High Confidence (100% Name + Father/Spouse Exact)"
                        : componentPairs.Any(p => p.UsesCitizenship)
                            ? "Medium Confidence (Review: Citizenship + Name/Father)"
                            : "Medium Confidence (Review: Name + Father/Spouse)"
                });
            }

            return suggestions;
        }

        private static bool IsExactNameFatherGroup(List<UniqueOwner> owners)
        {
            if (owners == null || owners.Count < 2)
                return false;

            var normalizedNames = owners
                .Select(o => NormalizeString(o.LandOwnersName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var normalizedFathers = owners
                .Select(o => NormalizeString(o.FatherSpouse ?? string.Empty))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return normalizedNames.Count == 1 && normalizedFathers.Count == 1;
        }

        private static List<OwnerToken> BuildOwnerTokens(List<UniqueOwner> owners)
        {
            var tokens = new List<OwnerToken>(owners.Count);
            for (int i = 0; i < owners.Count; i++)
            {
                var owner = owners[i];
                tokens.Add(new OwnerToken
                {
                    Index = i,
                    Owner = owner,
                    NormalizedName = NormalizeString(owner.LandOwnersName),
                    NormalizedFather = NormalizeString(owner.FatherSpouse ?? string.Empty),
                    NormalizedCitizenship = NormalizeCitizenship(owner.CitizenshipNumber ?? string.Empty)
                });
            }

            return tokens;
        }

        private static bool IsManualReviewCandidate(UniqueOwner owner)
        {
            return !owner.IsAnonymous &&
                   DetermineOwnerCategory(owner) == OwnerCategory.Person;
        }

        private static List<PairAssessment> FindPairAssessments(List<OwnerToken> tokens, ConfidenceBand targetBand)
        {
            var assessments = new List<PairAssessment>();
            if (tokens.Count <= 1)
            {
                return assessments;
            }

            var buckets = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < tokens.Count; i++)
            {
                foreach (var key in BuildComparisonBucketKeys(tokens[i]))
                {
                    if (!buckets.TryGetValue(key, out var bucket))
                    {
                        bucket = new List<int>();
                        buckets[key] = bucket;
                    }

                    bucket.Add(i);
                }
            }

            var pairSeen = new HashSet<long>();
            foreach (var bucket in buckets.Values)
            {
                if (bucket.Count <= 1)
                {
                    continue;
                }

                for (int i = 0; i < bucket.Count; i++)
                {
                    for (int j = i + 1; j < bucket.Count; j++)
                    {
                        int leftIndex = Math.Min(bucket[i], bucket[j]);
                        int rightIndex = Math.Max(bucket[i], bucket[j]);
                        long pairKey = ((long)leftIndex << 32) | (uint)rightIndex;
                        if (!pairSeen.Add(pairKey))
                        {
                            continue;
                        }

                        var assessment = AssessPersonDuplicate(tokens[leftIndex], tokens[rightIndex]);
                        if (assessment.Band == targetBand)
                        {
                            assessments.Add(assessment);
                        }
                    }
                }
            }

            return assessments;
        }

        private static IEnumerable<string> BuildComparisonBucketKeys(OwnerToken token)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            keys.Add(BuildSuggestionBucketKey(token.NormalizedName, token.NormalizedFather));

            if (!string.IsNullOrWhiteSpace(token.NormalizedCitizenship))
            {
                string prefix4 = token.NormalizedCitizenship.Length > 4
                    ? token.NormalizedCitizenship[..4]
                    : token.NormalizedCitizenship;
                keys.Add($"CIT::{prefix4}::{token.NormalizedCitizenship.Length}");
            }

            return keys;
        }

        private static string BuildSuggestionBucketKey(string normalizedName, string normalizedFather)
        {
            var name = normalizedName;
            var father = normalizedFather;

            var namePrefix = name.Length >= 3 ? name[..3] : name;
            var fatherPrefix = father.Length >= 2 ? father[..2] : father;
            var lengthBucket = name.Length / 3;

            return $"NF::{namePrefix}::{fatherPrefix}::{lengthBucket}";
        }

        private static PairAssessment AssessPersonDuplicate(OwnerToken left, OwnerToken right)
        {
            bool hasCitizenshipLeft = !string.IsNullOrWhiteSpace(left.NormalizedCitizenship);
            bool hasCitizenshipRight = !string.IsNullOrWhiteSpace(right.NormalizedCitizenship);
            bool hasBothCitizenship = hasCitizenshipLeft && hasCitizenshipRight;

            bool hasFatherLeft = !string.IsNullOrWhiteSpace(left.NormalizedFather);
            bool hasFatherRight = !string.IsNullOrWhiteSpace(right.NormalizedFather);
            bool hasBothFather = hasFatherLeft && hasFatherRight;

            double nameSimilarity = CalculateCharacterSimilarity(left.NormalizedName, right.NormalizedName);
            double fatherSimilarity = hasBothFather
                ? CalculateCharacterSimilarity(left.NormalizedFather, right.NormalizedFather)
                : 0.0;

            double nameFatherConfidence = hasBothFather
                ? (nameSimilarity + fatherSimilarity) / 2.0
                : nameSimilarity;

            double citizenshipConfidence = hasBothCitizenship
                ? CalculateCharacterSimilarity(left.NormalizedCitizenship, right.NormalizedCitizenship)
                : 0.0;

            // RULE 1: 100% citizenship match + >80% owner-name match => high confidence auto-merge.
            if (hasBothCitizenship)
            {
                if (citizenshipConfidence == 1.0 && nameSimilarity > HighNameThresholdForCitizenshipRule)
                {
                    return CreatePairAssessment(ConfidenceBand.High, true,
                        "High Confidence (100% Citizenship + >80% Name)", left.Index, right.Index,
                        citizenshipConfidence, nameFatherConfidence);
                }
            }

            // RULE 2: (owner-name + father/spouse) combined >95% => high confidence auto-merge.
            // Also auto-merge exact 100% normalized name-father score (as shown in UI),
            // even when father/spouse is missing on one side.
            if (nameFatherConfidence > HighNameFatherCombinedThreshold &&
                (hasBothFather || nameSimilarity == 1.0))
            {
                var highType = hasBothFather
                    ? "High Confidence (>95% Name + Father/Spouse)"
                    : "High Confidence (100% Name Match)";

                return CreatePairAssessment(ConfidenceBand.High, hasBothCitizenship,
                    highType, left.Index, right.Index,
                    citizenshipConfidence, nameFatherConfidence);
            }

            // RULE 3: if any available identity signal drops below 75%, keep separate.
            if (nameSimilarity < MinimumReviewThreshold)
            {
                return CreatePairAssessment(
                    ConfidenceBand.None,
                    hasBothCitizenship,
                    "Keep Separate (<75% Name Match)",
                    left.Index,
                    right.Index,
                    citizenshipConfidence,
                    nameFatherConfidence);
            }

            if (hasBothFather && fatherSimilarity < MinimumReviewThreshold)
            {
                return CreatePairAssessment(
                    ConfidenceBand.None,
                    hasBothCitizenship,
                    "Keep Separate (<75% Father/Spouse Match)",
                    left.Index,
                    right.Index,
                    citizenshipConfidence,
                    nameFatherConfidence);
            }

            if (hasBothCitizenship && citizenshipConfidence < MinimumReviewThreshold)
            {
                return CreatePairAssessment(
                    ConfidenceBand.None,
                    true,
                    "Keep Separate (<75% Citizenship Match)",
                    left.Index,
                    right.Index,
                    citizenshipConfidence,
                    nameFatherConfidence);
            }

            // RULE 4: all remaining records in 75-100 confidence range => manual review.
            bool qualifiesForReview;
            if (hasBothCitizenship && hasBothFather)
            {
                qualifiesForReview =
                    citizenshipConfidence >= MinimumReviewThreshold &&
                    nameSimilarity >= MinimumReviewThreshold &&
                    fatherSimilarity >= MinimumReviewThreshold;
            }
            else if (hasBothCitizenship)
            {
                qualifiesForReview =
                    citizenshipConfidence >= MinimumReviewThreshold &&
                    nameSimilarity >= MinimumReviewThreshold;
            }
            else if (hasBothFather)
            {
                qualifiesForReview =
                    nameSimilarity >= MinimumReviewThreshold &&
                    fatherSimilarity >= MinimumReviewThreshold;
            }
            else
            {
                qualifiesForReview = nameSimilarity >= MinimumReviewThreshold;
            }

            if (qualifiesForReview)
            {
                var reviewType = hasBothCitizenship
                    ? "Review Required (75-100% Citizenship + Name/Father)"
                    : hasBothFather
                        ? "Review Required (75-100% Name + Father/Spouse)"
                        : "Review Required (75-100% Name)";

                return CreatePairAssessment(ConfidenceBand.Medium, hasBothCitizenship,
                    reviewType, left.Index, right.Index,
                    citizenshipConfidence, nameFatherConfidence);
            }

            return CreatePairAssessment(
                ConfidenceBand.None,
                hasBothCitizenship,
                "Keep Separate",
                left.Index,
                right.Index,
                citizenshipConfidence,
                nameFatherConfidence);
        }

        private static PairAssessment CreatePairAssessment(
            ConfidenceBand band,
            bool usesCitizenship,
            string matchType,
            int leftIndex,
            int rightIndex,
            double citizenshipConfidence,
            double nameFatherConfidence)
        {
            return new PairAssessment
            {
                LeftIndex = leftIndex,
                RightIndex = rightIndex,
                Band = band,
                UsesCitizenship = usesCitizenship,
                MatchType = matchType,
                CitizenshipConfidence = citizenshipConfidence,
                NameFatherConfidence = nameFatherConfidence,
                Similarity = Math.Max(citizenshipConfidence, nameFatherConfidence)
            };
        }

        private static List<List<int>> BuildConnectedComponents(int nodeCount, List<PairAssessment> edges)
        {
            var adjacency = new List<int>[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                adjacency[i] = new List<int>();
            }

            foreach (var edge in edges)
            {
                adjacency[edge.LeftIndex].Add(edge.RightIndex);
                adjacency[edge.RightIndex].Add(edge.LeftIndex);
            }

            var components = new List<List<int>>();
            var visited = new bool[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                if (visited[i])
                {
                    continue;
                }

                var component = new List<int>();
                var stack = new Stack<int>();
                stack.Push(i);
                visited[i] = true;

                while (stack.Count > 0)
                {
                    int node = stack.Pop();
                    component.Add(node);

                    foreach (var neighbor in adjacency[node])
                    {
                        if (visited[neighbor])
                        {
                            continue;
                        }

                        visited[neighbor] = true;
                        stack.Push(neighbor);
                    }
                }

                components.Add(component);
            }

            return components;
        }

        private static List<UniqueOwner> CreateInitialOwnerList(
            List<BaselineLandParceRecord> records,
            DeduplicationResult result,
            bool excludeAnonymous)
        {
            var owners = new List<UniqueOwner>(records.Count);
            var anonymousCounter = 1;

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var rawName = NormalizeNullable(record.LandOwnersName);

                var isAnonymous = string.IsNullOrWhiteSpace(rawName) || ContainsAnonymousKeyword(rawName!);
                var name = isAnonymous ? $"Anonymous Owner {anonymousCounter++}" : rawName!;

                if (isAnonymous)
                    result.AnonymousOwnersCreated++;

                if (excludeAnonymous && isAnonymous)
                    continue;

                var owner = new UniqueOwner
                {
                    LandOwnersName = name,
                    FatherSpouse = NormalizeNullable(record.FatherSpouse),
                    Gender = NormalizeNullable(record.Gender),
                    CitizenshipNumber = NormalizeNullable(record.CitizenshipNumber),
                    CitizenshipIssuedDistrict = NormalizeNullable(record.CitizenshipIssuedDistrict),
                    CitizenshipIssuedDate = NormalizeNullable(record.citizenshipIssuedDate),
                    PermanentAddress = NormalizeNullable(record.PermanentAddress),
                    TemporaryAddress = NormalizeNullable(record.TempoaryAddress),
                    ContactNumber = NormalizeNullable(record.ContactNumber),
                    EmailID = NormalizeNullable(record.EmailID),
                    ParcelIndices = new List<int> { i },
                    IsAnonymous = isAnonymous
                };

                SanitizeOwnerByCategory(owner);
                owners.Add(owner);
            }

            return owners;
        }

        private static string BuildIdentityKey(UniqueOwner owner)
        {
            var category = DetermineOwnerCategory(owner);
            var normalizedName = NormalizeString(owner.LandOwnersName);
            var normalizedFather = NormalizeString(owner.FatherSpouse ?? string.Empty);
            var normalizedCitizenship = NormalizeCitizenship(owner.CitizenshipNumber ?? string.Empty);

            if (owner.IsAnonymous)
            {
                // Keep anonymous owners isolated by their row linkage.
                return $"A::{normalizedName}::{string.Join(",", owner.ParcelIndices.OrderBy(x => x))}";
            }

            if (category == OwnerCategory.Institution)
            {
                return $"I::{normalizedName}";
            }

            if (!string.IsNullOrWhiteSpace(normalizedCitizenship))
            {
                return $"P::C::{normalizedCitizenship}";
            }

            if (!string.IsNullOrWhiteSpace(normalizedFather))
            {
                return $"P::NF::{normalizedName}::{normalizedFather}";
            }

            return $"P::N::{normalizedName}";
        }

        private static OwnerCategory DetermineOwnerCategory(UniqueOwner owner)
        {
            if (IsLikelyInstitution(owner.LandOwnersName))
                return OwnerCategory.Institution;

            if (!string.IsNullOrWhiteSpace(owner.CitizenshipNumber) ||
                !string.IsNullOrWhiteSpace(owner.FatherSpouse) ||
                !string.IsNullOrWhiteSpace(owner.Gender))
            {
                return OwnerCategory.Person;
            }

            return string.IsNullOrWhiteSpace(owner.LandOwnersName)
                ? OwnerCategory.Unknown
                : OwnerCategory.Person;
        }

        private static bool IsLikelyInstitution(string? ownerName)
        {
            if (string.IsNullOrWhiteSpace(ownerName))
                return false;

            var normalized = NormalizeString(ownerName);
            return InstitutionKeywords.Any(k =>
                normalized.Contains(NormalizeString(k), StringComparison.OrdinalIgnoreCase));
        }

        private static bool ContainsAnonymousKeyword(string normalizedName)
        {
            var name = NormalizeString(normalizedName);
            return AnonymousKeywords.Any(k =>
                name.Contains(NormalizeString(k), StringComparison.OrdinalIgnoreCase));
        }

        private static void SanitizeOwnerByCategory(UniqueOwner owner)
        {
            if (DetermineOwnerCategory(owner) != OwnerCategory.Institution)
                return;

            // Institution/government owners should not carry personal identity fields.
            owner.FatherSpouse = null;
            owner.Gender = null;
            owner.CitizenshipNumber = null;
            owner.CitizenshipIssuedDistrict = null;
            owner.CitizenshipIssuedDate = null;
            owner.PermanentAddress = null;
            owner.TemporaryAddress = null;
            owner.ContactNumber = null;
            owner.EmailID = null;
        }

        private static UniqueOwner MergeOwners(List<UniqueOwner> owners)
        {
            if (owners == null || owners.Count == 0)
                throw new ArgumentException("Owner list cannot be empty.", nameof(owners));

            if (owners.Count == 1)
                return CloneOwner(owners[0]);

            var best = owners
                .OrderByDescending(GetCompletenessScore)
                .First();

            var merged = CloneOwner(best);
            merged.ParcelIndices = owners.SelectMany(o => o.ParcelIndices).Distinct().OrderBy(x => x).ToList();

            if (DetermineOwnerCategory(merged) == OwnerCategory.Institution)
            {
                SanitizeOwnerByCategory(merged);
                return merged;
            }

            merged.FatherSpouse ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.FatherSpouse))?.FatherSpouse;
            merged.Gender ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Gender))?.Gender;
            merged.CitizenshipNumber ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.CitizenshipNumber))?.CitizenshipNumber;
            merged.CitizenshipIssuedDistrict ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.CitizenshipIssuedDistrict))?.CitizenshipIssuedDistrict;
            merged.CitizenshipIssuedDate ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.CitizenshipIssuedDate))?.CitizenshipIssuedDate;
            merged.PermanentAddress ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.PermanentAddress))?.PermanentAddress;
            merged.TemporaryAddress ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.TemporaryAddress))?.TemporaryAddress;
            merged.ContactNumber ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.ContactNumber))?.ContactNumber;
            merged.EmailID ??= owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.EmailID))?.EmailID;

            return merged;
        }

        public static UniqueOwner MergeOwnerRecords(
            UniqueOwner owner1,
            UniqueOwner owner2,
            List<int> owner1Indices,
            List<int> owner2Indices)
        {
            var left = CloneOwner(owner1);
            left.ParcelIndices = owner1Indices.ToList();
            var right = CloneOwner(owner2);
            right.ParcelIndices = owner2Indices.ToList();

            var merged = MergeOwners([left, right]);
            return merged;
        }

        public static UniqueOwner MergeOwnersList(List<UniqueOwner> owners) => MergeOwners(owners);

        public static void ApplyDeduplicationToRecords(
            List<BaselineLandParceRecord> records,
            DeduplicationResult deduplicationResult)
        {
            foreach (var owner in deduplicationResult.UniqueOwners)
            {
                var category = DetermineOwnerCategory(owner);
                foreach (var index in owner.ParcelIndices)
                {
                    if (index < 0 || index >= records.Count)
                        continue;

                    var record = records[index];
                    record.LandOwnersName = owner.LandOwnersName;

                    if (category == OwnerCategory.Institution)
                    {
                        record.FatherSpouse = null;
                        record.Gender = null;
                        record.CitizenshipNumber = null;
                        record.CitizenshipIssuedDistrict = null;
                        record.citizenshipIssuedDate = null;
                        record.PermanentAddress = null;
                        record.TempoaryAddress = null;
                        record.ContactNumber = null;
                        record.EmailID = null;
                    }
                    else
                    {
                        record.FatherSpouse = owner.FatherSpouse;
                        record.Gender = owner.Gender;
                        record.CitizenshipNumber = owner.CitizenshipNumber;
                        record.CitizenshipIssuedDistrict = owner.CitizenshipIssuedDistrict;
                        record.citizenshipIssuedDate = owner.CitizenshipIssuedDate;
                        record.PermanentAddress = owner.PermanentAddress;
                        record.TempoaryAddress = owner.TemporaryAddress;
                        record.ContactNumber = owner.ContactNumber;
                        record.EmailID = owner.EmailID;
                    }
                }
            }
        }

        public static int GetCompletenessScore(UniqueOwner owner)
        {
            if (DetermineOwnerCategory(owner) == OwnerCategory.Institution)
            {
                return !string.IsNullOrWhiteSpace(owner.LandOwnersName) ? 10 : 0;
            }

            var score = 0;
            if (!string.IsNullOrWhiteSpace(owner.LandOwnersName) && !owner.IsAnonymous) score += 10;
            if (!string.IsNullOrWhiteSpace(owner.FatherSpouse)) score += 5;
            if (!string.IsNullOrWhiteSpace(owner.CitizenshipNumber)) score += 8;
            if (!string.IsNullOrWhiteSpace(owner.Gender)) score += 2;
            if (!string.IsNullOrWhiteSpace(owner.PermanentAddress)) score += 3;
            if (!string.IsNullOrWhiteSpace(owner.TemporaryAddress)) score += 2;
            if (!string.IsNullOrWhiteSpace(owner.ContactNumber)) score += 2;
            if (!string.IsNullOrWhiteSpace(owner.EmailID)) score += 2;
            return score;
        }

        public static double CalculateCharacterSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                return 1.0;

            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0.0;

            var distance = LevenshteinDistance(s1, s2);
            var maxLen = Math.Max(s1.Length, s2.Length);
            return 1.0 - ((double)distance / maxLen);
        }

        public static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var normalized = input.Normalize(NormalizationForm.FormC);
            var cleaned = new string(normalized.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            var parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts).ToUpperInvariant();
        }

        private static string NormalizeCitizenship(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var converted = ConvertDevanagariToArabicDigits(input);
            return new string(converted.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        private static string ConvertDevanagariToArabicDigits(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var builder = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (c >= '\u0966' && c <= '\u096F')
                {
                    builder.Append((char)('0' + (c - '\u0966')));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        private static int LevenshteinDistance(string s1, string s2)
        {
            var d = new int[s1.Length + 1, s2.Length + 1];

            for (var i = 0; i <= s1.Length; i++) d[i, 0] = i;
            for (var j = 0; j <= s2.Length; j++) d[0, j] = j;

            for (var i = 1; i <= s1.Length; i++)
            {
                for (var j = 1; j <= s2.Length; j++)
                {
                    var cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        private static double CalculateGroupCitizenshipConfidence(List<UniqueOwner> owners)
        {
            var withCitizenship = owners
                .Select(o => NormalizeCitizenship(o.CitizenshipNumber ?? string.Empty))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (withCitizenship.Count <= 1)
                return withCitizenship.Count == 1 ? 1.0 : 0.0;

            return 0.0;
        }

        private static double CalculateGroupNameFatherConfidence(List<UniqueOwner> owners)
        {
            if (owners.Count <= 1)
                return 1.0;

            var comparisons = 0;
            var total = 0.0;
            for (var i = 0; i < owners.Count; i++)
            {
                for (var j = i + 1; j < owners.Count; j++)
                {
                    total += CalculateCharacterSimilarity(
                        GetNormalizedNameFatherKey(owners[i]),
                        GetNormalizedNameFatherKey(owners[j]));
                    comparisons++;
                }
            }

            return comparisons == 0 ? 1.0 : total / comparisons;
        }

        private static string GetNormalizedNameFatherKey(UniqueOwner owner)
        {
            var name = NormalizeString(owner.LandOwnersName);
            var father = NormalizeString(owner.FatherSpouse ?? string.Empty);
            return $"{name} {father}".Trim();
        }

        private static string? NormalizeNullable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static UniqueOwner CloneOwner(UniqueOwner owner)
        {
            return new UniqueOwner
            {
                LandOwnersName = owner.LandOwnersName,
                FatherSpouse = owner.FatherSpouse,
                Gender = owner.Gender,
                CitizenshipNumber = owner.CitizenshipNumber,
                CitizenshipIssuedDistrict = owner.CitizenshipIssuedDistrict,
                CitizenshipIssuedDate = owner.CitizenshipIssuedDate,
                PermanentAddress = owner.PermanentAddress,
                TemporaryAddress = owner.TemporaryAddress,
                ContactNumber = owner.ContactNumber,
                EmailID = owner.EmailID,
                ParcelIndices = owner.ParcelIndices.ToList(),
                IsAnonymous = owner.IsAnonymous
            };
        }
    }
}
