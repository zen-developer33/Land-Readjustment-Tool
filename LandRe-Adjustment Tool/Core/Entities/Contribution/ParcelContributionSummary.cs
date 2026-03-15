using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Replotting;

namespace Land_Readjustment_Tool.Core.Entities.Contribution
{
    [Table("tblParcelContributionSummaries")]
    public class ParcelContributionSummary
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BaselineParcelId { get; set; }
        public int? ReplottedParcelId { get; set; }

        public double OriginalAreaSqm { get; set; }
        public double EffectiveAreaSqm { get; set; }
        public double TotalGeneralContributionSqm { get; set; }
        public double TotalSpecificContributionSqm { get; set; }
        public double TotalDeductionSqm { get; set; }
        public double TotalContributionSqm { get; set; }
        public double TotalContributionPercent { get; set; }
        public double NetReturnableAreaSqm { get; set; }

        public double? ReplottedAreaAssignedSqm { get; set; }
        // null until replotting is done

        public double? AreaDifferenceSqm { get; set; }
        public double? CashCompensationAmount { get; set; }

        public bool IsFinalized { get; set; } = false;
        public DateTime? FinalizedDate { get; set; }
        public DateTime LastCalculatedDate { get; set; }

        // Navigation
        public BaselineParcel BaselineParcel { get; set; } = null!;
        public ReplottedParcel? ReplottedParcel { get; set; }
    }
}