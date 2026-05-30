using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Buildings
{
    [Table("tblBuildingPhotos")]
    public class BuildingPhoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BuildingInventoryId { get; set; }

        [MaxLength(40)]
        public string Direction { get; set; } = string.Empty;

        [MaxLength(260)]
        public string? FileName { get; set; }

        [MaxLength(80)]
        public string? ContentType { get; set; }

        public byte[] ImageData { get; set; } = [];

        public DateTime? CapturedDate { get; set; }

        public string? Notes { get; set; }

        public BuildingInventory BuildingInventory { get; set; } = null!;
    }
}
