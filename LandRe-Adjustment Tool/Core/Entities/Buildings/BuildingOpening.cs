using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Buildings
{
    [Table("tblBuildingOpenings")]
    public class BuildingOpening
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuildingInventoryId { get; set; }

        [MaxLength(40)]
        public string Side { get; set; } = string.Empty;

        [MaxLength(40)]
        public string OpeningType { get; set; } = string.Empty;

        [MaxLength(80)]
        public string? Label { get; set; }

        public double? OffsetFromLeftM { get; set; }

        public double? SillHeightM { get; set; }

        public double? WidthM { get; set; }

        public double? HeightM { get; set; }

        public string? Notes { get; set; }

        public BuildingInventory BuildingInventory { get; set; } = null!;
    }
}
