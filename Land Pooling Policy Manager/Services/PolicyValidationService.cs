using System.Globalization;
using Land_Pooling_Policy_Manager.Entities.Policy;

namespace Land_Pooling_Policy_Manager.Services
{
    public sealed class PolicyValidationService
    {
        public List<string> ValidateDraft(PolicySet policy)
        {
            List<string> issues = [];

            if (string.IsNullOrWhiteSpace(policy.PolicyName))
                issues.Add("Policy name is required.");

            if (policy.Clauses.Count == 0)
                issues.Add("At least one clause is recommended before approval.");

            AddDuplicateWarnings(
                issues,
                policy.Clauses.Select(c => c.ClauseCode),
                "clause code");

            AddDuplicateWarnings(
                issues,
                policy.Parameters.Select(p => p.ParameterKey),
                "parameter key");

            foreach (PolicyParameter parameter in policy.Parameters)
                ValidateParameterBounds(parameter, issues);

            foreach (PolicyLookupTable table in policy.LookupTables)
            {
                if (table.Columns.Count == 0 || table.Rows.Count == 0)
                    issues.Add($"Lookup table '{table.Title}' should have at least one column and one row.");
            }

            return issues;
        }

        public List<string> ValidateForApproval(PolicySet policy)
        {
            List<string> issues = ValidateDraft(policy);

            if (string.IsNullOrWhiteSpace(policy.PolicyCode))
                issues.Add("Policy code is required before approval.");

            if (policy.Clauses.Any(c => string.IsNullOrWhiteSpace(c.ClauseCode)))
                issues.Add("Every clause must have a clause code before approval.");

            if (policy.Parameters.Any(p => string.IsNullOrWhiteSpace(p.ParameterKey)))
                issues.Add("Every parameter must have a parameter key before approval.");

            foreach (PolicyParameter parameter in policy.Parameters)
            {
                bool projectValue = string.Equals(
                    parameter.ValueType,
                    "ProjectValue",
                    StringComparison.OrdinalIgnoreCase);

                if (!projectValue && string.IsNullOrWhiteSpace(parameter.ValueText))
                    issues.Add($"Parameter '{parameter.Label}' requires a value before approval.");
            }

            return issues.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static bool IsEditable(PolicySet policy)
        {
            return !policy.IsLocked &&
                   !string.Equals(policy.Status, PolicyStatuses.Approved, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddDuplicateWarnings(
            List<string> issues,
            IEnumerable<string?> values,
            string label)
        {
            string[] duplicates = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .GroupBy(v => v, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

            foreach (string duplicate in duplicates)
                issues.Add($"Duplicate {label}: {duplicate}");
        }

        private static void ValidateParameterBounds(
            PolicyParameter parameter,
            List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(parameter.ValueText) ||
                string.Equals(parameter.ValueType, "Text", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameter.ValueType, "Bool", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parameter.ValueType, "ProjectValue", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!double.TryParse(parameter.ValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                issues.Add($"Parameter '{parameter.Label}' must be numeric.");
                return;
            }

            if (double.TryParse(parameter.MinValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double min) &&
                value < min)
            {
                issues.Add($"Parameter '{parameter.Label}' is below minimum {parameter.MinValueText}.");
            }

            if (double.TryParse(parameter.MaxValueText, NumberStyles.Float, CultureInfo.InvariantCulture, out double max) &&
                value > max)
            {
                issues.Add($"Parameter '{parameter.Label}' is above maximum {parameter.MaxValueText}.");
            }
        }
    }
}
