using Land_Readjustment_Tool.Services;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Import
{
    [Table("tblImportedRawRecords")]
    public class ImportedRawRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ImportSessionId { get; set; }

        // Parcel fields — all nullable
        public string? MapSheetNo { get; set; }
        public string? ParcelNo { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Municipality { get; set; }
        public string? WardNo { get; set; }
        public string? MothNo { get; set; }
        public string? PaanaNo { get; set; }
        public string? LandUse { get; set; }
        public double? AreaSqm { get; set; }

        // Traditional area units — kept for raw audit
        public string? AreaRAPD { get; set; }
        // e.g. "2-3-1-0" or "2 Ropani 3 Aana 1 Paisa 0 Daam"

        public string? AreaBKD { get; set; }
        // e.g. "1-2-3" or "1 Bigha 2 Kattha 3 Dhur"

        // Owner fields — all nullable
        public string? OwnerName { get; set; }
        public string? FatherSpouseName { get; set; }
        public string? Gender { get; set; }
        public string? CitizenshipNumber { get; set; }
        public string? CitizenshipDistrict { get; set; }
        public string? CitizenshipDate { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public bool? IsTenant { get; set; }
        public string? Remarks { get; set; }

        // Import metadata
        public int RowNumber { get; set; }
        public bool IsValid { get; set; } = true;
        public string? RawRowData { get; set; }

        // Deduplication metadata
        public int? DeduplicatedToOwnerId { get; set; }
        public string? DeduplicationMethod { get; set; }
        public double? DeduplicationConfidence { get; set; }
        public bool WasManuallyReviewed { get; set; } = false;
        public DateTime? ManualReviewDate { get; set; }

        // Navigation properties
        public ImportSession ImportSession { get; set; } = null!;
        public ICollection<ValidationError> ValidationErrors { get; set; } = new List<ValidationError>();
    }
}