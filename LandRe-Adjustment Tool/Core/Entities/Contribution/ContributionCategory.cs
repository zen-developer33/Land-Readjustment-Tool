using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Land_Readjustment_Tool.Core.Entities.Contribution
{
    [Table("tblContributionCategories")]
    public class ContributionCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;
        // e.g. "Road Contribution"
        //      "Open Space Contribution"
        //      "Corner Plot"
        //      "Infrastructure"

        [Required]
        public string ContributionType { get; set; } = string.Empty;
        // "General" → applies to all parcels
        // "Specific" → applies to selected parcels only

        public bool IsDeduction { get; set; } = false;
        // false → adds to contribution (owner gives more)
        // true  → reduces contribution (owner gives less)
        // e.g. existing road deduction = true

        [Required]
        public string RateType { get; set; } = string.Empty;
        // "Percentage" → rate is a percent of effective area
        // "FixedArea"  → rate is fixed sqm amount
        // "Formula"    → application calculates using
        //                 a specific algorithm

        public double? Rate { get; set; }
        // used when RateType = "Percentage" or "FixedArea"
        // null when RateType = "Formula"

        public string? FormulaType { get; set; }
        // used when RateType = "Formula"
        // "RoadContributionFormula"
        // "CornerContributionFormula"
        // "SlopedLandFormula"
        // application reads this and applies
        // the correct algorithm in code

        public string? ApplicableAreaRule { get; set; }
        // "FullEffectiveArea"  → use full effective area
        // "FrontageLimited"    → use area up to frontage limit
        // "Custom"             → special rule

        public double? ApplicableAreaLimit { get; set; }
        // used when ApplicableAreaRule = "FrontageLimited"
        // e.g. 8.0 for 8m frontage limit on corner plots

        public int DisplayOrder { get; set; } = 0;
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public ICollection<ParcelContribution>
            ParcelContributions
        { get; set; } = new List<ParcelContribution>();
    }
}