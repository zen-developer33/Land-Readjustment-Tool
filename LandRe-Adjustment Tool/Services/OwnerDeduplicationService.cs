using Land_Readjustment_Tool.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Service for extracting unique landowners from imported records
    /// Handles fuzzy matching, anonymous owner creation, and deduplication
    /// </summary>
    public class OwnerDeduplicationService
    {
        // Threshold for citizenship fuzzy matching (95% character match after normalization)
        private const double CitizenshipMatchThreshold = 0.95;
        // Threshold for name+father fuzzy matching (85% character match - higher for better accuracy)
        private const double NameFatherMatchThreshold = 0.85;

        /// <summary>
        /// Result of the deduplication process
        /// </summary>
        public class DeduplicationResult
        {
            public List<UniqueOwner> UniqueOwners { get; set; } = new();
            public List<DuplicateGroup> DuplicatesNeedingReview { get; set; } = new();
            public List<DuplicateGroup> AutoMergedGroups { get; set; } = new();
            public int TotalOriginalRecords { get; set; }
            public int AnonymousOwnersCreated { get; set; }
            public int AutoMergedCount { get; set; }
        }

        /// <summary>
        /// Represents a unique owner with all their parcel indices
        /// Contains only owner-specific fields for deduplication
        /// </summary>
        public class UniqueOwner
        {
            public string LandOwnersName { get; set; } = string.Empty;
            public string? FatherSpouse { get; set; }
            public string? Gender { get; set; }
            public string? CitizenshipNumber { get; set; }
            public string? PermanentAddress { get; set; }
            public List<int> ParcelIndices { get; set; } = new(); // Indices in original list
            public bool IsAnonymous { get; set; }
            public int CompletenessScore => GetCompletenessScore(this);
        }

        /// <summary>
        /// Group of potential duplicates that need user review
        /// </summary>
        public class DuplicateGroup
        {
            // A duplicate group may contain more than two owners that look like duplicates
            public List<UniqueOwner> Owners { get; set; } = new();

            // Representative similarity score for the group (e.g., 0.80 for name+father matches)
            public double Similarity { get; set; }

            // Confidence components
            public double CitizenshipConfidence { get; set; }
            public double NameFatherConfidence { get; set; }

            // When automatically merged via citizenship, keep a reference so user can undo
            public UniqueOwner? AutoMergedOwner { get; set; }
            
            // Whether this was auto-merged based on citizenship
            public bool IsAutoMerged { get; set; }
            
            // Match type description
            public string MatchType { get; set; } = string.Empty;
            
            // User's decision for this group (null if not decided yet)
            public UserDecisionType? UserDecision { get; set; }
        }
        
        /// <summary>
        /// User's decision for a duplicate group
        /// </summary>
        public enum UserDecisionType
        {
            Merge,
            KeepSeparate
        }

        /// <summary>
        /// Main method: Extract unique owners from imported records
        /// Strategy:
        /// 1. Find citizenship-based duplicates (auto-merge if citizenship matches >= 95%)
        /// 2. For remaining owners, find name+father duplicates
        /// 3. If name+father matches AND citizenship also matches (or one is missing), auto-merge
        /// 4. If only name+father matches, flag for review
        /// </summary>
        public static DeduplicationResult ExtractUniqueOwners(
            List<OriginalLandParcelWithLandOwner> records)
        {
            var result = new DeduplicationResult
            {
                TotalOriginalRecords = records.Count
            };
            
            // Step 1: Handle anonymous owners and create initial owner list
            var potentialOwners = CreateInitialOwnerList(records, result);

            var uniqueOwners = new List<UniqueOwner>();
            var processedIndices = new HashSet<int>();

            // Step 2: Find citizenship-based duplicates (fuzzy match with 95% threshold)
            // Only auto-merge if citizenship matches well
            var citizenshipGroups = FindCitizenshipDuplicates(potentialOwners);
            
            foreach (var group in citizenshipGroups)
            {
                if (group.Count > 1)
                {
                    // Multiple owners with similar citizenship - auto-merge
                    var merged = MergeOwners(group);
                    uniqueOwners.Add(merged);
                    
                    foreach (var owner in group)
                    {
                        foreach (var idx in owner.ParcelIndices)
                            processedIndices.Add(idx);
                    }

                    // Add to auto-merged list for user visibility (can undo)
                    var dupGroup = new DuplicateGroup
                    {
                        Owners = group,
                        Similarity = 0.95,
                        CitizenshipConfidence = 0.95,
                        NameFatherConfidence = 0.0,
                        AutoMergedOwner = merged,
                        IsAutoMerged = true,
                        MatchType = "Citizenship Match (Auto-merged)"
                    };
                    result.DuplicatesNeedingReview.Add(dupGroup);
                    result.AutoMergedGroups.Add(dupGroup);
                    result.AutoMergedCount++;
                }
                // Note: Single-owner groups are NOT marked as processed here
                // They will go through name+father matching in Step 3
            }

            // Step 3: For ALL owners not yet processed, find name+father duplicates
            // This includes owners with citizenship that didn't match anyone
            var remainingOwners = potentialOwners
                .Where(o => !o.ParcelIndices.Any(idx => processedIndices.Contains(idx)))
                .ToList();

            var nameFatherGroups = FindNameFatherDuplicatesWithCitizenshipCheck(remainingOwners);
            
            foreach (var group in nameFatherGroups)
            {
                if (group.Owners.Count == 1)
                {
                    // No duplicates found - add as unique
                    uniqueOwners.Add(group.Owners[0]);
                }
                else if (group.IsAutoMerged)
                {
                    // Name+Father matches AND citizenship confirms (or missing)
                    var merged = MergeOwners(group.Owners);
                    uniqueOwners.Add(merged);
                    group.AutoMergedOwner = merged;
                    result.DuplicatesNeedingReview.Add(group);
                    result.AutoMergedGroups.Add(group);
                    result.AutoMergedCount++;
                }
                else
                {
                    // Name+Father matches but citizenship doesn't confirm - needs review
                    uniqueOwners.Add(group.Owners[0]);
                    result.DuplicatesNeedingReview.Add(group);
                }
            }

            result.UniqueOwners = uniqueOwners;
            return result;
        }

        /// <summary>
        /// Find groups of owners with similar citizenship numbers (>= 95% character match)
        /// Anonymous owners are excluded from this grouping - they are always kept separate
        /// </summary>
        private static List<List<UniqueOwner>> FindCitizenshipDuplicates(List<UniqueOwner> owners)
        {
            var groups = new List<List<UniqueOwner>>();
            var processed = new HashSet<int>();
            
            // Exclude anonymous owners from citizenship-based matching
            var ownersWithCitizenship = owners
                .Select((o, idx) => (Owner: o, Index: idx))
                .Where(x => !string.IsNullOrWhiteSpace(x.Owner.CitizenshipNumber) && !x.Owner.IsAnonymous)
                .ToList();

            for (int i = 0; i < ownersWithCitizenship.Count; i++)
            {
                if (processed.Contains(i)) continue;
                
                var current = ownersWithCitizenship[i];
                var group = new List<UniqueOwner> { current.Owner };
                processed.Add(i);
                
                string normalizedCurrent = NormalizeCitizenship(current.Owner.CitizenshipNumber!);
                
                for (int j = i + 1; j < ownersWithCitizenship.Count; j++)
                {
                    if (processed.Contains(j)) continue;
                    
                    var other = ownersWithCitizenship[j];
                    string normalizedOther = NormalizeCitizenship(other.Owner.CitizenshipNumber!);
                    
                    double similarity = CalculateCharacterSimilarity(normalizedCurrent, normalizedOther);
                    
                    if (similarity >= CitizenshipMatchThreshold)
                    {
                        group.Add(other.Owner);
                        processed.Add(j);
                    }
                }
                
                groups.Add(group);
            }
            
            // NOTE: Owners without citizenship are NOT added here
            // They will be processed by FindNameFatherDuplicates instead
            
            return groups;
        }

        /// <summary>
        /// Find groups of owners with similar name+father combination (>= 50% character match)
        /// </summary>
        private static List<DuplicateGroup> FindNameFatherDuplicates(List<UniqueOwner> owners)
        {
            var groups = new List<DuplicateGroup>();
            var processed = new HashSet<int>();
            
            for (int i = 0; i < owners.Count; i++)
            {
                if (processed.Contains(i)) continue;
                
                var current = owners[i];
                var group = new List<UniqueOwner> { current };
                processed.Add(i);
                
                string normalizedCurrent = GetNormalizedNameFatherKey(current);
                
                for (int j = i + 1; j < owners.Count; j++)
                {
                    if (processed.Contains(j)) continue;
                    
                    var other = owners[j];
                    string normalizedOther = GetNormalizedNameFatherKey(other);
                    
                    double similarity = CalculateCharacterSimilarity(normalizedCurrent, normalizedOther);
                    
                    if (similarity >= NameFatherMatchThreshold)
                    {
                        group.Add(other);
                        processed.Add(j);
                    }
                }
                
                groups.Add(new DuplicateGroup
                {
                    Owners = group,
                    Similarity = group.Count > 1 ? CalculateGroupSimilarity(group) : 1.0,
                    NameFatherConfidence = group.Count > 1 ? CalculateGroupSimilarity(group) : 1.0,
                    CitizenshipConfidence = 0.0
                });
            }
            
            return groups;
        }

        /// <summary>
        /// Find groups of owners with similar name+father combination,
        /// then check if citizenship confirms or conflicts.
        /// - If name+father matches AND citizenship matches (or one is missing): auto-merge
        /// - If name+father matches but citizenship is different: needs review
        /// </summary>
        private static List<DuplicateGroup> FindNameFatherDuplicatesWithCitizenshipCheck(List<UniqueOwner> owners)
        {
            var groups = new List<DuplicateGroup>();
            var processed = new HashSet<int>();
            
            for (int i = 0; i < owners.Count; i++)
            {
                if (processed.Contains(i)) continue;
                
                var current = owners[i];
                var group = new List<UniqueOwner> { current };
                processed.Add(i);
                
                string normalizedCurrentNameFather = GetNormalizedNameFatherKey(current);
                string normalizedCurrentCitizenship = NormalizeCitizenship(current.CitizenshipNumber ?? "");
                
                for (int j = i + 1; j < owners.Count; j++)
                {
                    if (processed.Contains(j)) continue;
                    
                    var other = owners[j];
                    string normalizedOtherNameFather = GetNormalizedNameFatherKey(other);
                    
                    double nameFatherSimilarity = CalculateCharacterSimilarity(normalizedCurrentNameFather, normalizedOtherNameFather);
                    
                    if (nameFatherSimilarity >= NameFatherMatchThreshold)
                    {
                        group.Add(other);
                        processed.Add(j);
                    }
                }
                
                // Now determine if this group should be auto-merged or needs review
                // IMPORTANT: Never auto-merge anonymous owners - they should always be kept separate
                bool shouldAutoMerge = false;
                double citizenshipConfidence = 0.0;
                double nameFatherConfidence = group.Count > 1 ? CalculateGroupSimilarity(group) : 1.0;
                
                // Check if any owner in the group is anonymous
                bool hasAnonymousOwner = group.Any(o => o.IsAnonymous);
                
                if (group.Count > 1 && !hasAnonymousOwner)
                {
                    // Check citizenship compatibility
                    var ownersWithCitizenship = group
                        .Where(o => !string.IsNullOrWhiteSpace(o.CitizenshipNumber))
                        .ToList();
                    
                    if (ownersWithCitizenship.Count == 0)
                    {
                        // No owners have citizenship - cannot confirm based on citizenship
                        shouldAutoMerge = true; // Can auto-merge based on name+father only
                        citizenshipConfidence = 0.0; // No citizenship data to compare
                    }
                    else if (ownersWithCitizenship.Count == 1)
                    {
                        // Only one owner has citizenship - cannot confirm but safe to merge
                        shouldAutoMerge = true;
                        citizenshipConfidence = 0.0; // Only one citizenship, nothing to compare
                    }
                    else
                    {
                        // Multiple owners have citizenship - check if they match
                        var firstCitizenship = NormalizeCitizenship(ownersWithCitizenship[0].CitizenshipNumber!);
                        bool allMatch = true;
                        double totalSimilarity = 0;
                        int comparisons = 0;
                        
                        for (int k = 1; k < ownersWithCitizenship.Count; k++)
                        {
                            var otherCitizenship = NormalizeCitizenship(ownersWithCitizenship[k].CitizenshipNumber!);
                            double similarity = CalculateCharacterSimilarity(firstCitizenship, otherCitizenship);
                            totalSimilarity += similarity;
                            comparisons++;
                            
                            if (similarity < CitizenshipMatchThreshold)
                            {
                                allMatch = false;
                            }
                        }
                        
                        citizenshipConfidence = comparisons > 0 ? totalSimilarity / comparisons : 0.0;
                        shouldAutoMerge = allMatch;
                    }
                }
                
                string matchType;
                if (group.Count == 1)
                {
                    matchType = "Unique";
                }
                else if (hasAnonymousOwner)
                {
                    matchType = "Contains Anonymous Owner (Keep Separate)";
                }
                else if (shouldAutoMerge)
                {
                    matchType = "Name + Father Match + Citizenship Confirmed (Auto-merged)";
                }
                else
                {
                    matchType = "Name + Father Match - Citizenship Differs (Review Required)";
                }
                
                groups.Add(new DuplicateGroup
                {
                    Owners = group,
                    Similarity = nameFatherConfidence,
                    NameFatherConfidence = nameFatherConfidence,
                    CitizenshipConfidence = citizenshipConfidence,
                    IsAutoMerged = shouldAutoMerge && group.Count > 1,
                    MatchType = matchType
                });
            }
            
            return groups;
        }

        /// <summary>
        /// Calculate character-level similarity between two strings (0.0 to 1.0)
        /// </summary>
        public static double CalculateCharacterSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 1.0;
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;
            
            // Use Levenshtein distance for character-level comparison
            int distance = LevenshteinDistance(s1, s2);
            int maxLength = Math.Max(s1.Length, s2.Length);
            
            return 1.0 - ((double)distance / maxLength);
        }

        /// <summary>
        /// Calculate Levenshtein distance between two strings
        /// </summary>
        private static int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        /// <summary>
        /// Get normalized combined key of name + father/spouse for comparison
        /// </summary>
        private static string GetNormalizedNameFatherKey(UniqueOwner owner)
        {
            string name = NormalizeString(owner.LandOwnersName ?? string.Empty);
            string father = NormalizeString(owner.FatherSpouse ?? string.Empty);
            return $"{name} {father}".Trim().ToUpper();
        }

        /// <summary>
        /// Calculate average similarity for a group of owners
        /// </summary>
        private static double CalculateGroupSimilarity(List<UniqueOwner> group)
        {
            if (group.Count <= 1) return 1.0;
            
            double totalSimilarity = 0;
            int comparisons = 0;
            
            for (int i = 0; i < group.Count; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    string key1 = GetNormalizedNameFatherKey(group[i]);
                    string key2 = GetNormalizedNameFatherKey(group[j]);
                    totalSimilarity += CalculateCharacterSimilarity(key1, key2);
                    comparisons++;
                }
            }
            
            return comparisons > 0 ? totalSimilarity / comparisons : 1.0;
        }

        /// <summary>
        /// Step 1: Create initial owner list, handling anonymous cases
        /// </summary>
        private static List<UniqueOwner> CreateInitialOwnerList(
            List<OriginalLandParcelWithLandOwner> records,
            DeduplicationResult result)
        {
            var owners = new List<UniqueOwner>();
            int anonymousCounter = 1;

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                bool isAnonymous = false;

                // Handle empty/null owner names
                if (string.IsNullOrWhiteSpace(record.LandOwnersName))
                {
                    record.LandOwnersName = $"Anonymous Owner {anonymousCounter}";
                    isAnonymous = true;
                    anonymousCounter++;
                    result.AnonymousOwnersCreated++;
                }

                var owner = new UniqueOwner
                {
                    LandOwnersName = record.LandOwnersName.Trim(),
                    FatherSpouse = record.FatherSpouse?.Trim(),
                    Gender = record.Gender?.Trim(),
                    CitizenshipNumber = record.CitizenshipNumber?.Trim(),
                    PermanentAddress = record.PermanentAddress?.Trim(),
                    ParcelIndices = new List<int> { i },
                    IsAnonymous = isAnonymous
                };

                owners.Add(owner);
            }

            return owners;
        }

        /// <summary>
        /// Merge multiple owner records into a single representative owner
        /// </summary>
        private static UniqueOwner MergeOwners(List<UniqueOwner> owners)
        {
            if (owners.Count == 1)
                return owners[0];

            // Strategy: Use the most complete record as base
            var baseOwner = owners
                .OrderByDescending(o => GetCompletenessScore(o))
                .First();

            // Merge all parcel indices
            var allIndices = owners.SelectMany(o => o.ParcelIndices).Distinct().ToList();

            return new UniqueOwner
            {
                LandOwnersName = baseOwner.LandOwnersName,
                FatherSpouse = baseOwner.FatherSpouse ?? owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.FatherSpouse))?.FatherSpouse,
                Gender = baseOwner.Gender ?? owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Gender))?.Gender,
                CitizenshipNumber = baseOwner.CitizenshipNumber ?? owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.CitizenshipNumber))?.CitizenshipNumber,
                PermanentAddress = baseOwner.PermanentAddress ?? owners.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.PermanentAddress))?.PermanentAddress,
                ParcelIndices = allIndices,
                IsAnonymous = baseOwner.IsAnonymous
            };
        }

        public static UniqueOwner MergeOwnerRecords(
    UniqueOwner owner1,
    UniqueOwner owner2,
    List<int> owner1Indices,
    List<int> owner2Indices)
        {
            var ownersToMerge = new List<UniqueOwner>
    {
        new UniqueOwner
        {
            LandOwnersName = owner1.LandOwnersName,
            FatherSpouse = owner1.FatherSpouse,
            Gender = owner1.Gender,
            CitizenshipNumber = owner1.CitizenshipNumber,
            PermanentAddress = owner1.PermanentAddress,
            ParcelIndices = owner1Indices.ToList(),
            IsAnonymous = owner1.IsAnonymous
        },
        new UniqueOwner
        {
            LandOwnersName = owner2.LandOwnersName,
            FatherSpouse = owner2.FatherSpouse,
            Gender = owner2.Gender,
            CitizenshipNumber = owner2.CitizenshipNumber,
            PermanentAddress = owner2.PermanentAddress,
            ParcelIndices = owner2Indices.ToList(),
            IsAnonymous = owner2.IsAnonymous
        }
    };

            return MergeOwners(ownersToMerge);
        }


        /// <summary>
        /// Calculate completeness score for an owner record
        /// Higher score = more complete data
        /// </summary>
        public static int GetCompletenessScore(UniqueOwner owner)
        {
            int score = 0;
            if (!string.IsNullOrWhiteSpace(owner.LandOwnersName) && !owner.IsAnonymous) score += 10;
            if (!string.IsNullOrWhiteSpace(owner.FatherSpouse)) score += 5;
            if (!string.IsNullOrWhiteSpace(owner.CitizenshipNumber)) score += 8;
            if (!string.IsNullOrWhiteSpace(owner.Gender)) score += 2;
            if (!string.IsNullOrWhiteSpace(owner.PermanentAddress)) score += 3;
            return score;
        }

        /// <summary>
        /// Normalize citizenship number for comparison
        /// - Converts Devanagari digits (०१२३४५६७८९) to Arabic digits (0123456789)
        /// - Removes /, -, and other special characters
        /// </summary>
        private static string NormalizeCitizenship(string citizenship)
        {
            if (string.IsNullOrWhiteSpace(citizenship))
                return string.Empty;

            // Convert Devanagari digits to Arabic digits
            var converted = ConvertDevanagariToArabicDigits(citizenship);
            
            // Remove all non-alphanumeric characters (/, -, spaces, etc.)
            return new string(converted.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        /// <summary>
        /// Converts Devanagari numerals to Arabic numerals
        /// ० -> 0, १ -> 1, २ -> 2, ... ९ -> 9
        /// </summary>
        private static string ConvertDevanagariToArabicDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            var result = new System.Text.StringBuilder(input.Length);
            
            foreach (char c in input)
            {
                // Devanagari digits are in Unicode range U+0966 to U+096F
                if (c >= '\u0966' && c <= '\u096F')
                {
                    // Convert to Arabic digit (0-9)
                    result.Append((char)('0' + (c - '\u0966')));
                }
                else
                {
                    result.Append(c);
                }
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Normalize a name or father/spouse string for comparison.
        /// Handles Devanagari (Nepali/Hindi) script properly.
        /// - Normalizes Unicode to composed form (NFC)
        /// - Removes punctuation and special characters
        /// - Collapses whitespace
        /// - Converts to uppercase for Latin characters
        /// </summary>
        public static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            
            // Normalize Unicode to composed form (important for Devanagari)
            string normalized = input.Normalize(System.Text.NormalizationForm.FormC);
            
            // Keep letters (including Devanagari), digits, and spaces
            var cleaned = new string(normalized.Where(c => 
                char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
            
            // Collapse multiple spaces
            var parts = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Join and convert to uppercase (Devanagari has no case, so it stays unchanged)
            return string.Join(" ", parts).ToUpperInvariant();
        }

        /// <summary>
        /// Public wrapper to merge multiple UniqueOwner records into one representative owner
        /// </summary>
        public static UniqueOwner MergeOwnersList(List<UniqueOwner> owners) => MergeOwners(owners);

        /// <summary>
        /// Apply the deduplication result back to the original records
        /// Updates all records with normalized owner information
        /// </summary>
        public static void ApplyDeduplicationToRecords(
            List<OriginalLandParcelWithLandOwner> records,
            DeduplicationResult deduplicationResult)
        {
            foreach (var uniqueOwner in deduplicationResult.UniqueOwners)
            {
                // Update all parcels belonging to this owner
                foreach (int index in uniqueOwner.ParcelIndices)
                {
                    if (index >= 0 && index < records.Count)
                    {
                        var record = records[index];
                        
                        // Normalize owner information across all parcels
                        // Note: ParcelLocation stays with the parcel record, not the owner
                        record.LandOwnersName = uniqueOwner.LandOwnersName;
                        record.FatherSpouse = uniqueOwner.FatherSpouse;
                        record.Gender = uniqueOwner.Gender;
                        record.CitizenshipNumber = uniqueOwner.CitizenshipNumber;
                        record.PermanentAddress = uniqueOwner.PermanentAddress;
                    }
                }
            }
        }
    }
}
