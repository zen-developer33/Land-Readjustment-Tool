using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Entities.Roads
{
    [Table("tblRoadParcels")]
    public class RoadParcel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoadParcelNumber { get; set; } = string.Empty;

        [Required]
        public string RoadName { get; set; } = string.Empty;

        public RoadParcelType RoadType { get; set; } = RoadParcelType.Unknown;

        [Required]
        public Polygon Shape { get; set; } = null!;

        public ImportSource ImportedFrom { get; set; }
        public DateTime ImportedAt { get; set; }
        public DonutValidationStatus ValidationStatus { get; set; }
        public string? ValidationMessage { get; set; }

        public List<RoadIsland> Islands { get; set; } = new();

        [NotMapped]
        public bool IsDonut => Shape?.NumInteriorRings > 0;

        [NotMapped]
        public int IslandCount => Shape?.NumInteriorRings ?? 0;

        [NotMapped]
        public double RoadArea => Shape?.Area ?? 0.0;
    }
}
