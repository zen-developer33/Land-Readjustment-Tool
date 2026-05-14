using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Entities.Roads
{
    [Table("tblRoadIslands")]
    public class RoadIsland
    {
        [Key]
        public int Id { get; set; }

        public int RoadParcelId { get; set; }
        public int HoleIndex { get; set; }
        public string? LinkedParcelNumber { get; set; }

        [Required]
        public Polygon IslandShape { get; set; } = null!;

        public string? IslandDescription { get; set; }

        public RoadParcel RoadParcel { get; set; } = null!;
    }
}
