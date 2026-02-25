
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Entities
{
    [Table("tblReplottedParcels")]
    public class ReplottedParcel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BlockId { get; set; }

        [Required]
        public int PlotTypeId { get; set; }

        // ── PARCEL NUMBERS ──────────────────────────
        public string? SystemGeneratedNumber { get; set; }
        // brand new system generated number

        public string? DerivedNumber { get; set; }
        // derived from original parcel number

        public string? BlockSequenceNumber { get; set; }
        // block name + sequence number
        // e.g. "A-001", "B-012"

        [Required]
        public string ActiveNumberType { get; set; } = "SystemGenerated";
        // "SystemGenerated"
        // "Derived"
        // "BlockSequence"
        // controls which number shows on canvas
        // and which is used in documents

        // ── AREA ────────────────────────────────────
        public double PlotAreaSqm { get; set; }
        // calculated from canvas geometry

        // ── CANVAS LINK ─────────────────────────────
        public Guid? CanvasObjectId { get; set; }

        // ── METADATA ────────────────────────────────
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public Block Block { get; set; } = null!;
        public PlotType PlotType { get; set; } = null!;
        public CanvasObject? CanvasObject { get; set; }
        public ICollection<ReplottedParcelOwner>
            ReplottedParcelOwners
        { get; set; } = [];
        public ICollection<OriginalToReplottedMap>
            OriginalToReplottedMaps
        { get; set; } = [];
        public ICollection<ParcelFrontage>
            ParcelFrontages
        { get; set; } = [];
        public ICollection<ParcelContributionSummary>
            ParcelContributionSummaries
        { get; set; } = [];
    }
}