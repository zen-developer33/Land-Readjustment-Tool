using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.Import;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Contribution;
using Land_Readjustment_Tool.Core.Entities.Replotting;

namespace Land_Readjustment_Tool.Core.Entities.LandData
{
    [Table("tblBaselineParcels")]
    public class BaselineParcel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ImportSessionId { get; set; }

        [Required]
        public int LandOwnerId { get; set; }

        public int? MalpotReferenceId { get; set; }

        // Identification
        [Required]
        public string MapSheetNo { get; set; } = string.Empty;

        [Required]
        public string ParcelNo { get; set; } = string.Empty;

        [Required]
        public string FullUniqueParcelCode { get; set; } = string.Empty;
        // MapSheetNo + ParcelNo combined — must be unique

        // Location
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Municipality { get; set; }
        public string? WardNo { get; set; }

        // Area
        [Required]
        public double OriginalAreaSqm { get; set; }

        public double? EffectiveAreaSqm { get; set; }
        // null until calculated

        public bool IsEffectiveAreaManual { get; set; } = false;
        // true if user overrode auto calculation

        // Land info
        public string? LandUse { get; set; }
        public bool HasTenant { get; set; } = false;
        public string? TenantName { get; set; }
        public string? Remarks { get; set; }

        // Canvas link
        public Guid? CanvasObjectId { get; set; }

        // Metadata
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public ImportSession ImportSession { get; set; } = null!;
        public LandOwner LandOwner { get; set; } = null!;
        public MalpotReference? MalpotReference { get; set; }
        public CanvasObject? CanvasObject { get; set; }
        public ICollection<ParcelFrontage> ParcelFrontages { get; set; } = [];
        public ICollection<ParcelContribution> ParcelContributions { get; set; } = [];
        public ParcelContributionSummary? ParcelContributionSummary { get; set; }
        public ICollection<OriginalToReplottedMap> OriginalToReplottedMaps { get; set; } = [];
    }
}