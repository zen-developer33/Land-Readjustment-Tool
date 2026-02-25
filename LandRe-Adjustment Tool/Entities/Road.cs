using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Entities
{
    [Table("tblRoads")]
    public class Road
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoadName { get; set; } = string.Empty;

        public string? RoadCode { get; set; }

        [Required]
        public string RoadStatus { get; set; } = string.Empty;
        // "Existing"
        // "Proposed"

        public string? SurfaceType { get; set; }
        // "Blacktopped"
        // "Gravel"
        // "Earthen"
        // "Concrete"

        [Required]
        public double RoadWidth { get; set; }
        // in meters — carriageway width

        public double? RightOfWayWidth { get; set; }
        // in meters — total ROW including footpath

        public string? RoadType { get; set; }
        // "Arterial"
        // "Collector"
        // "Local"
        // "Lane"

        // Canvas link
        public Guid? CanvasObjectId { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public CanvasObject? CanvasObject { get; set; }
        public ICollection<ParcelFrontage> ParcelFrontages { get; set; } = [];
    }
}