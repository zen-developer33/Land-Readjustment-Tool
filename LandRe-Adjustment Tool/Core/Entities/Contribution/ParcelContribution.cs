using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.LandData;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Land_Readjustment_Tool.Core.Entities.Contribution
{
    [Table("tblParcelContributions")]
    public class ParcelContribution
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BaselineParcelId { get; set; }

        [Required]
        public int ContributionCategoryId { get; set; }

        // ── CALCULATION INPUTS ──────────────────────
        public double ApplicableAreaSqm { get; set; }
        // area used for this specific calculation
        // may differ from effective area
        // e.g. corner plot uses only 8m frontage area

        public double RateApplied { get; set; }
        // snapshot of rate at time of calculation
        // stored here because rate may change later
        // but this calculation must remain unchanged

        // ── CALCULATION RESULT ──────────────────────
        public double ContributionAmountSqm { get; set; }
        // final calculated contribution in sqm
        // positive = contribution (owner gives land)
        // negative = deduction (owner gets reduction)

        // ── MANUAL OVERRIDE ─────────────────────────
        public bool IsManualOverride { get; set; } = false;
        public double? ManualOverrideValueSqm { get; set; }
        public string? ManualOverrideReason { get; set; }

        // ── METADATA ────────────────────────────────
        public DateTime CalculatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public BaselineParcel BaselineParcel { get; set; } = null!;
        public ContributionCategory
            ContributionCategory
        { get; set; } = null!;
    }
}