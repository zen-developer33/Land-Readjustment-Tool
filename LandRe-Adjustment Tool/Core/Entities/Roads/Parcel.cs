using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Entities.Roads
{
    [Table("tblParcels")]
    public class Parcel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ParcelNumber { get; set; } = string.Empty;

        [Required]
        public string ParcelType { get; set; } = "GENERAL";

        [Required]
        public Geometry Shape { get; set; } = null!;

        [NotMapped]
        public bool IsDonut => (Shape as Polygon)?.NumInteriorRings > 0;

        [NotMapped]
        public int InteriorRingCount => (Shape as Polygon)?.NumInteriorRings ?? 0;
    }
}
