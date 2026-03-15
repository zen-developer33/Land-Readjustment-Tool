using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Core.Entities.Layout;

namespace Land_Readjustment_Tool.Core.Entities.LandData
{
    [Table("tblParcelFrontages")]
    public class ParcelFrontage
    {
        [Key]
        public int Id { get; set; }

        // Either BaselineParcelId OR ReplottedParcelId
        // must have a value — not both null
        public int? BaselineParcelId { get; set; }
        public int? ReplottedParcelId { get; set; }

        [Required]
        public int RoadId { get; set; }

        [Required]
        public string FacingDirection { get; set; } = string.Empty;
        // "N", "S", "E", "W"
        // "NE", "NW", "SE", "SW"

        public double? FrontageLength { get; set; }
        // in meters

        // Navigation properties
        public BaselineParcel? BaselineParcel { get; set; }
        public ReplottedParcel? ReplottedParcel { get; set; }
        public Road Road { get; set; } = null!;
    }
}