using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.LandData;

namespace Land_Readjustment_Tool.Core.Entities.Replotting
{
    [Table("tblOriginalToReplottedMaps")]
    public class OriginalToReplottedMap
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BaselineParcelId { get; set; }

        [Required]
        public int ReplottedParcelId { get; set; }

        public double ContributedAreaSqm { get; set; }
        // how much area this original parcel
        // contributed to this replotted parcel

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public BaselineParcel BaselineParcel { get; set; } = null!;
        public ReplottedParcel ReplottedParcel { get; set; } = null!;
    }
}