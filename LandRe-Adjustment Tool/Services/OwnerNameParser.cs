namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Parses a raw owner-name field that may contain multiple names joined by
    /// common Nepali and English delimiters.  Used in two places:
    ///   1. PostProcessImportedRecords — single-row joint-name splitting.
    ///   2. Ownership-type auto-detection in the import pipeline.
    /// </summary>
    public static class OwnerNameParser
    {
        // Order matters: longer/multi-char delimiters must come before sub-strings.
        private static readonly string[] Delimiters =
        [
            " एवम् ", " तथा ", " and ", " र ", ",", "/", "&"
        ];

        /// <summary>
        /// Splits a raw owner-name value into individual names.
        /// Returns a list with at least one element (the original string when no
        /// delimiter is found).  Each returned name is trimmed.
        /// </summary>
        public static List<string> SplitOwnerNames(string? rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                return new List<string>();

            var parts = new List<string> { rawName.Trim() };

            foreach (var delimiter in Delimiters)
            {
                var expanded = new List<string>();
                foreach (var part in parts)
                {
                    var splits = part.Split(
                        delimiter,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    expanded.AddRange(splits.Length > 1 ? splits : new[] { part });
                }
                parts = expanded;
            }

            // Keep only non-trivial entries (at least 2 characters).
            return parts.Where(p => p.Length >= 2).ToList();
        }

        /// <summary>
        /// Returns true when the raw name field contains more than one distinct
        /// person name (i.e. the parcel has joint ownership indicated by the name).
        /// </summary>
        public static bool HasMultipleOwnerNames(string? rawName)
            => SplitOwnerNames(rawName).Count >= 2;
    }
}
