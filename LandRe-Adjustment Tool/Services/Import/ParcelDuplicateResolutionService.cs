using Land_Readjustment_Tool.Models;

namespace Land_Readjustment_Tool.Services.Import
{
    public static class ParcelDuplicateResolutionService
    {
        public sealed class ParcelDuplicateGroup
        {
            public string Key { get; init; } = string.Empty;
            public string MapSheetNo { get; init; } = string.Empty;
            public string ParcelNo { get; init; } = string.Empty;
            public List<BaselineLandParcelRecord> Records { get; init; } = new();

            public string OwnersSummary => string.Join(", ",
                Records.Select(r => r.LandOwnersName).Where(name => !string.IsNullOrWhiteSpace(name)));
        }

        public static List<ParcelDuplicateGroup> FindDuplicateGroups(IEnumerable<BaselineLandParcelRecord> records)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            return records
                .Where(r => !r.IsJointCoOwnerRow)
                .Where(r => !string.IsNullOrWhiteSpace(r.ParcelNo) && !string.IsNullOrWhiteSpace(r.MapSheetNo))
                .GroupBy(r => BuildKey(r.MapSheetNo, r.ParcelNo), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g =>
                {
                    var first = g.First();
                    return new ParcelDuplicateGroup
                    {
                        Key = g.Key,
                        MapSheetNo = first.MapSheetNo?.Trim() ?? string.Empty,
                        ParcelNo = first.ParcelNo?.Trim() ?? string.Empty,
                        Records = g.ToList()
                    };
                })
                .OrderBy(g => g.MapSheetNo, StringComparer.OrdinalIgnoreCase)
                .ThenBy(g => g.ParcelNo, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public static void ApplyJointOwnership(ParcelDuplicateGroup group, int primaryIndex)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));
            ApplyJointOwnership(group.Records, primaryIndex);
        }

        public static void ApplyJointOwnership(IReadOnlyList<BaselineLandParcelRecord> records, int primaryIndex)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));
            if (records.Count < 2)
                return;
            if (primaryIndex < 0 || primaryIndex >= records.Count)
                throw new ArgumentOutOfRangeException(nameof(primaryIndex));

            var primary = records[primaryIndex];
            primary.IsJointCoOwnerRow = false;
            primary.LandOwnershipType = "Private (Joint)";

            for (int i = 0; i < records.Count; i++)
            {
                if (i == primaryIndex)
                    continue;

                var source = records[i];
                var coOwner = CreateCoOwner(source);

                if (!IsSameOwner(primary, coOwner) && !HasMatchingCoOwner(primary, coOwner))
                {
                    primary.JointCoOwners.Add(coOwner);
                }

                source.Remarks = StripShareTag(source.Remarks);
                source.IsJointCoOwnerRow = true;
            }
        }

        private static CoOwnerRecord CreateCoOwner(BaselineLandParcelRecord source)
        {
            return new CoOwnerRecord
            {
                OwnerName = Normalize(source.LandOwnersName),
                FatherSpouse = Normalize(source.FatherSpouse),
                Gender = Normalize(source.Gender),
                CitizenshipNumber = Normalize(source.CitizenshipNumber),
                CitizenshipIssuedDistrict = Normalize(source.CitizenshipIssuedDistrict),
                CitizenshipIssuedDate = Normalize(source.CitizenshipIssuedDate),
                PermanentAddress = Normalize(source.PermanentAddress),
                TemporaryAddress = Normalize(source.TemporaryAddress),
                ContactNumber = Normalize(source.ContactNumber),
                EmailID = Normalize(source.EmailID),
                OwnershipSharePercent = ExtractShare(source.Remarks)
            };
        }

        private static bool HasMatchingCoOwner(BaselineLandParcelRecord primary, CoOwnerRecord candidate)
        {
            var candidateKey = BuildOwnerIdentity(candidate.OwnerName, candidate.FatherSpouse, candidate.CitizenshipNumber);
            return primary.JointCoOwners.Any(existing =>
                string.Equals(
                    BuildOwnerIdentity(existing.OwnerName, existing.FatherSpouse, existing.CitizenshipNumber),
                    candidateKey,
                    StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Reverses a previous <see cref="ApplyJointOwnership"/> call for the given group:
        /// removes the co-owner entries that were added to the primary record and resets all
        /// group records to independent (non-co-owner) rows.
        /// </summary>
        public static void UndoJointOwnership(ParcelDuplicateGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            if (group.Records.Count < 2) return;

            // The primary is whichever record was NOT flagged as a co-owner row after merge.
            var primary = group.Records.FirstOrDefault(r => !r.IsJointCoOwnerRow);
            if (primary == null)
                return; // nothing to undo

            // Remove from the primary's JointCoOwners the entries that came from other group records.
            foreach (var coRecord in group.Records.Where(r => r != primary))
            {
                var identity = BuildOwnerIdentity(coRecord.LandOwnersName, coRecord.FatherSpouse, coRecord.CitizenshipNumber);
                var match = primary.JointCoOwners.FirstOrDefault(co =>
                    string.Equals(
                        BuildOwnerIdentity(co.OwnerName, co.FatherSpouse, co.CitizenshipNumber),
                        identity,
                        StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    primary.JointCoOwners.Remove(match);
            }

            // Revert the ownership type when no joint co-owners remain.
            if (primary.JointCoOwners.Count == 0 &&
                string.Equals(primary.LandOwnershipType, "Private (Joint)", StringComparison.OrdinalIgnoreCase))
            {
                primary.LandOwnershipType = "Private";
            }

            // Un-flag every group record so they are all treated as independent rows again.
            foreach (var record in group.Records)
                record.IsJointCoOwnerRow = false;
        }

        private static bool IsSameOwner(BaselineLandParcelRecord primary, CoOwnerRecord candidate)
        {
            return string.Equals(
                BuildOwnerIdentity(primary.LandOwnersName, primary.FatherSpouse, primary.CitizenshipNumber),
                BuildOwnerIdentity(candidate.OwnerName, candidate.FatherSpouse, candidate.CitizenshipNumber),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildOwnerIdentity(string? ownerName, string? fatherSpouse, string? citizenshipNumber)
        {
            var citizen = OwnerDeduplicationService.NormalizeString(citizenshipNumber ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(citizen))
            {
                return $"C::{citizen}";
            }

            return $"NF::{OwnerDeduplicationService.NormalizeString(ownerName ?? string.Empty)}::{OwnerDeduplicationService.NormalizeString(fatherSpouse ?? string.Empty)}";
        }

        private static string BuildKey(string? mapSheetNo, string? parcelNo)
        {
            return $"{(mapSheetNo ?? string.Empty).Trim().ToUpperInvariant()}::{(parcelNo ?? string.Empty).Trim().ToUpperInvariant()}";
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static double? ExtractShare(string? remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks)) return null;
            const string tag = "__SHARE__";
            int pos = remarks.LastIndexOf(tag, StringComparison.Ordinal);
            if (pos < 0) return null;
            var rest = remarks[(pos + tag.Length)..].Trim().Split(' ')[0];
            return double.TryParse(rest, out var value) ? value : null;
        }

        private static string StripShareTag(string? remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks)) return string.Empty;
            const string tag = "__SHARE__";
            int pos = remarks.LastIndexOf(tag, StringComparison.Ordinal);
            return pos < 0 ? remarks : remarks[..pos].TrimEnd();
        }
    }
}
