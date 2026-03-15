using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Import
{
    [Table("tblImportSessions")]
    public class ImportSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SourceFileName { get; set; } = string.Empty;
        public string? SourceFilePath { get; set; }
        public DateTime ImportDate { get; set; }
        public int TotalRowsInFile { get; set; }
        public int TotalRowsImported { get; set; }
        public int TotalRowsInvalid { get; set; }
        public bool IsReplaced { get; set; } = false; // Indicates if this import replaced an existing one
        public int? ReplacedBySessionID { get; set; }
        public string? Notes { get; set; }
        // Navigation properties    
        public ICollection<ImportedRawRecord> ImportedRawRecords { get; set; } = [];
        public ICollection<ValidationError> ValidationErrors { get; set; } = [];
        public ICollection<CitizenshipConflict> CitizenshipConflicts { get; set; } = [];
    }
}
