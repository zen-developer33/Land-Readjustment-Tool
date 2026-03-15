using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.Replotting;

namespace Land_Readjustment_Tool.Core.Entities.LandData
{
    [Table("tblLandOwners")]
    public class LandOwner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;
        public string? FatherOrSpouseName { get; set; }
        public string? Gender { get; set; }
        public string? CitizenshipNumber { get; set; }
        public string? CitizenshipIssueDistrict { get; set; }
        public string? CitizenshipIssueDate { get; set; }
        public string? PermanentAddress { get; set; }
        public string? TemporaryAddress { get; set; }
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? PhotoPath { get; set; }
        public string? DocumentsFolderPath { get; set; }

        //Deduplication metadata
        [Required]
        public string IdentificationMethod { get; set; } = "CitizenshipNumber"; // Default to "Citizenship"
        public double? MatchConfidenceScore { get; set; }
        public bool NeedsManualReview { get; set; } = false;

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        //Navigation properties
        public ICollection<MalpotReference> MalpotReferences { get; set; } = [];
        public ICollection<BaselineParcel> BaselineParcels { get; set; } = [];
        public ICollection<ReplottedParcelOwner> ReplottedParcelOwnerships { get; set; } = [];
    }
}
