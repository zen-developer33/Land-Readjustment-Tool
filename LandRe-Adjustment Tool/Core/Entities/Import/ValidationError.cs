using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Mathematics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Import
{
    [Table("tblValidationErrors")]
    public class ValidationError
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ImportSessionId { get; set; }

        [Required]
        public int ImportedRawRecordId { get; set; }

        [Required]
        public string FieldName { get; set; } = string.Empty;

        [Required]
        public string ErrorType { get; set; } = string.Empty;
        // "Missing"
        // "Duplicate"
        // "InvalidFormat"
        // "AreaMismatch"

        [Required]
        public string ErrorMessage { get; set; } = string.Empty;

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedDate { get; set; }

        // Navigation properties
        public ImportSession ImportSession { get; set; } = null!;
        public ImportedRawRecord ImportedRawRecord { get; set; } = null!;
    }
}