using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace Land_Readjustment_Tool.Core.Entities.Import
{
    [Table("tblCitizenshipConflictRecords")]
    public class CitizenshipConflictRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CitizenshipConflictId { get; set; }

        [Required]
        public int ImportedRawRecordId { get; set; }

        public bool IsMarkedCorrect { get; set; } = false;
        // User marks which records are correct
        // during conflict resolution

        // Navigation properties
        public CitizenshipConflict CitizenshipConflict { get; set; } = null!;
        public ImportedRawRecord ImportedRawRecord { get; set; } = null!;
    }
}