using Microsoft.VisualBasic.ApplicationServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Replotting
{
    [Table("tblPlotTypes")]
    public class PlotType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TypeName { get; set; } = string.Empty;
        // "Private"
        // "Sales Plot"
        // "Government"
        // "Open Space"
        // "Community"
        // "Road"

        [Required]
        public string TypeCode { get; set; } = string.Empty;
        // "PRV"
        // "SAL"
        // "GOV"
        // "OPS"
        // "COM"
        // "ROD"
        // short code used in reports and canvas labels

        public bool IsSystemDefault { get; set; } = false;
        // true  = came from master list
        //         cannot be deleted
        // false = user added custom type
        //         can be deleted if not in use

        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
        // false = soft deleted
        // hidden from UI but kept for
        // existing records that reference it

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public ICollection<ReplottedParcel> ReplottedParcels { get; set; } = new List<ReplottedParcel>();
    }
}