using Land_Readjustment_Tool.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Entities
{
    [Table("tblBlocks")]
    public class Block
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string BlockName { get; set; } = string.Empty;

        public string? BlockCode { get; set; }

        [Required]
        public float BlockDepth { get; set; } 
        
        public string? BlockLandUse { get; set; }//commercial, residential, mixed-use, etc.

        public double BlockArea { get; set; }
        // in sqm
        // calculated from canvas geometry

        public string? Description { get; set; }

        // Canvas link
        public Guid? CanvasObjectId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public CanvasObject? CanvasObject { get; set; }
        public ICollection<ReplottedParcel> ReplottedParcels { get; set; } = [];
    }
}