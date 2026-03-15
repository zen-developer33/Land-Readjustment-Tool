using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.LandData;

namespace Land_Readjustment_Tool.Core.Entities.Replotting
{
    [Table("tblReplottedParcelOwners")]
    public class ReplottedParcelOwner
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReplottedParcelId { get; set; }

        [Required]
        public int LandOwnerId { get; set; }

        // Ownership share — calculated from
        // OriginalToReplottedMap.ContributedAreaSqm
        // stored here as snapshot for documents
        public double OwnershipSharePercent { get; set; }
        // Single owner  → 100.0
        // Joint owners  → proportional percentage
        // e.g. 60.0 and 40.0

        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public ReplottedParcel ReplottedParcel { get; set; } = null!;
        public LandOwner LandOwner { get; set; } = null!;
    }
}