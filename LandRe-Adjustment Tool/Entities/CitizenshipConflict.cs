using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Entities
{
    [Table("tblCitizenshipConflicts")]
    public class CitizenshipConflict
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ImportSessionId { get; set; }

        [Required]
        public string CitizenshipNumber { get; set; } = string.Empty;
        // The conflicting citizenship number

        [Required]
        public string ConflictType { get; set; } = string.Empty;
        // "SameNumberDifferentName"
        // "SameNumberSameNameDifferentFather"

        public string? Resolution { get; set; }
        // "OneRecordCorrect"
        // "AllSamePerson"
        // "ManuallyEdited"

        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedDate { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public ImportSession ImportSession { get; set; } = null!;
        public ICollection<CitizenshipConflictRecord> ConflictingRecords { get; set; } = [];
    }
}