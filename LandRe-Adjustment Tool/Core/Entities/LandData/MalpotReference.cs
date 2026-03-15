using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.LandData
{
    [Table("tblMalpotReferences")]
    public class MalpotReference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LandOwnerId { get; set; }

        [Required]
        public string MothNo { get; set; } = string.Empty;

        [Required]
        public string PaanaNo { get; set; } = string.Empty;

        // Navigation properties
        public LandOwner LandOwner { get; set; } = null!;
        public ICollection<BaselineParcel> BaselineParcels { get; set; } = [];
    }
}
