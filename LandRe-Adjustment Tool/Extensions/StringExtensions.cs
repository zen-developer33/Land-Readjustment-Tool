namespace Land_Readjustment_Tool.Extensions
{
    /// <summary>
    /// Extension methods for string type.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Returns null if string is null, empty or whitespace.
        /// Otherwise returns the original string.
        /// Used to store null instead of empty string in database.
        /// </summary>
        public static string? NullIfEmpty(this string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }
    }
}