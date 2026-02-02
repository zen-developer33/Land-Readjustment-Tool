using System;
using System.Collections.Generic;
using System.Linq;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Provides fuzzy string matching capabilities for detecting similar text
    /// Uses Levenshtein distance and normalized scoring
    /// </summary>
    public class FuzzyMatchingService
    {
        /// <summary>
        /// Calculates similarity between two strings (0.0 to 1.0)
        /// 1.0 = identical, 0.0 = completely different
        /// </summary>
        public static double CalculateSimilarity(string str1, string str2)
        {
            if (string.IsNullOrWhiteSpace(str1) && string.IsNullOrWhiteSpace(str2))
                return 1.0; // Both empty = identical

            if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(str2))
                return 0.0; // One empty = different

            str1 = NormalizeString(str1);
            str2 = NormalizeString(str2);

            if (str1 == str2)
                return 1.0; // Exact match after normalization

            int distance = LevenshteinDistance(str1, str2);
            int maxLength = Math.Max(str1.Length, str2.Length);

            return 1.0 - ((double)distance / maxLength);
        }

        /// <summary>
        /// Normalizes string for comparison (lowercase, trim, remove extra spaces)
        /// </summary>
        private static string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert to lowercase
            input = input.ToLower().Trim();

            // Replace multiple spaces with single space
            input = System.Text.RegularExpressions.Regex.Replace(input, @"\s+", " ");

            // Remove common punctuation and special characters
            input = input.Replace(".", "").Replace(",", "").Replace("-", " ").Replace("_", " ");

            // Normalize common Nepali name variations
            input = input.Replace("bdr", "bahadur")
                         .Replace("bd", "bahadur")
                         .Replace("b ", "bahadur ")
                         .Replace("smt", "srimati")
                         .Replace("shri", "sri")
                         .Replace("mr", "")
                         .Replace("mrs", "")
                         .Replace("ms", "");

            return input.Trim();
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings
        /// Returns the minimum number of edits needed to transform one string into another
        /// </summary>
        private static int LevenshteinDistance(string s1, string s2)
        {
            int[,] distance = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                distance[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[s1.Length, s2.Length];
        }

        /// <summary>
        /// Checks if citizenship numbers match (handles null/empty)
        /// </summary>
        public static bool CitizenshipMatches(string cit1, string cit2)
        {
            // If either is empty, can't determine match
            if (string.IsNullOrWhiteSpace(cit1) || string.IsNullOrWhiteSpace(cit2))
                return false;

            // Remove all non-alphanumeric characters for comparison
            string normalized1 = new string(cit1.Where(char.IsLetterOrDigit).ToArray()).ToLower();
            string normalized2 = new string(cit2.Where(char.IsLetterOrDigit).ToArray()).ToLower();

            return normalized1 == normalized2;
        }

        /// <summary>
        /// Calculates composite similarity score for owner matching
        /// Returns 0.0 to 1.0 based on Name, Father/Spouse, and Citizenship
        /// </summary>
        public static double CalculateOwnerSimilarity(
            string name1, string father1, string citizenship1,
            string name2, string father2, string citizenship2)
        {
            // If citizenship numbers match exactly, it's very likely the same person
            if (CitizenshipMatches(citizenship1, citizenship2))
                return 0.95; // High confidence match

            // Calculate individual similarities
            double nameSimilarity = CalculateSimilarity(name1, name2);
            double fatherSimilarity = CalculateSimilarity(father1, father2);

            // Weighted composite score
            // Name is most important (70%), Father/Spouse second (30%)
            double compositeScore = (nameSimilarity * 0.7) + (fatherSimilarity * 0.3);

            return compositeScore;
        }

        /// <summary>
        /// Gets match category based on similarity score
        /// </summary>
        public static MatchCategory GetMatchCategory(double similarity)
        {
            if (similarity >= 0.90)
                return MatchCategory.HighConfidence;
            else if (similarity >= 0.70)
                return MatchCategory.MediumConfidence;
            else
                return MatchCategory.Different;
        }
    }

    /// <summary>
    /// Categories for match confidence levels
    /// </summary>
    public enum MatchCategory
    {
        HighConfidence,    // >= 0.90 - Auto-merge
        MediumConfidence,  // 0.70-0.89 - User review
        Different          // < 0.70 - Keep separate
    }
}
