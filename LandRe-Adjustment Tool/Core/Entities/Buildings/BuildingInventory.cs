using Land_Readjustment_Tool.Core.Entities.Canvas;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Buildings
{
    [Table("tblBuildingInventories")]
    public class BuildingInventory
    {
        [Key]
        public int Id { get; set; }

        public Guid? CanvasObjectId { get; set; }

        [MaxLength(80)]
        public string BuildingCode { get; set; } = string.Empty;

        [MaxLength(160)]
        public string? BuildingName { get; set; }

        [MaxLength(120)]
        public string? OwnerName { get; set; }

        [MaxLength(80)]
        public string? BuildingUse { get; set; }

        [MaxLength(80)]
        public string? ConstructionType { get; set; }

        public int? StoreyCount { get; set; }

        public double? PlinthAreaSqm { get; set; }

        [MaxLength(80)]
        public string? BuildingCondition { get; set; }

        public string? Notes { get; set; }

        public DateTime SurveyDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public CanvasObject? CanvasObject { get; set; }

        public ICollection<BuildingPhoto> Photos { get; set; } = new List<BuildingPhoto>();

        public ICollection<BuildingOpening> Openings { get; set; } = new List<BuildingOpening>();
    }
}
