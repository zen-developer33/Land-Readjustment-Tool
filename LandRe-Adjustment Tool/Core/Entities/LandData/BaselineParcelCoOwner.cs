using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.LandData
{
    [Table("tblBaselineParcelCoOwners")]
    public class BaselineParcelCoOwner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BaselineParcelId { get; set; }

        [Required]
        public int LandOwnerId { get; set; }

        // null = share unknown / equal split assumed
        public double? OwnershipSharePercent { get; set; }

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public BaselineParcel BaselineParcel { get; set; } = null!;
        public LandOwner LandOwner { get; set; } = null!;
    }
}
