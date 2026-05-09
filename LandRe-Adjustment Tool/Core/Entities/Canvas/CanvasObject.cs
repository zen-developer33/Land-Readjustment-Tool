using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Core.Entities.Layout;

namespace Land_Readjustment_Tool.Core.Entities.Canvas
{
    [Table("tblCanvasObjects")]
    public class CanvasObject
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        // Guid because canvas objects are created
        // in memory before database save

        [Required]
        public int CanvasLayerId { get; set; }

        [Required]
        public string ObjectType { get; set; } = string.Empty;
        // "Polygon"
        // "Polyline"
        // "Line"
        // "Arc"
        // "Point"
        // "Text"
        // "Circle"

        // ── GEOMETRY ────────────────────────────────
        [Required]
        public Geometry Shape { get; set; } = null!;
        // NetTopologySuite Geometry
        // Stores actual coordinates in SQLite
        // Polygon, Polyline, Point etc.

        public string? GeometryMetadataJson { get; set; }
        // Stores semantic CAD geometry parameters that NTS/WKT cannot preserve,
        // e.g. circle center/radius or arc angles. Shape remains the linearized
        // geometry used for database queries and interchange.

        // ── DISPLAY OVERRIDES ───────────────────────
        // These override layer defaults per object
        public string? BorderColorOverride { get; set; }
        public string? FillColorOverride { get; set; }
        public int? FillTransparencyOverride { get; set; }
        public double? LineWeightOverride { get; set; }
        public string? LineStyleOverride { get; set; }

        // ── LABEL ───────────────────────────────────
        public string? LabelText { get; set; }
        // Custom label for this specific object

        public string? ObjectDescription { get; set; }
        // For unlinked objects — describe what it is
        // e.g. "Project Boundary"
        //      "Existing Building"
        //      "Drainage Line"
        //      "Survey Control Point"
        //      "Contour 1400m"

        // ── VISIBILITY ──────────────────────────────
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        // ── DATA LINKS ──────────────────────────────
        // All nullable — linked later by user or system or may not be linked at all
        public int? BaselineParcelId { get; set; }
        public int? ReplottedParcelId { get; set; }
        public int? RoadId { get; set; }
        public int? BlockId { get; set; }

        // ── SOURCE ──────────────────────────────────
        public string? SourceDxfHandle { get; set; }
        // Original DXF entity handle
        // Used for re-import matching

        // ── METADATA ────────────────────────────────
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }

        // Navigation properties
        public CanvasLayer CanvasLayer { get; set; } = null!;
        public BaselineParcel? BaselineParcel { get; set; }
        public ReplottedParcel? ReplottedParcel { get; set; }
        public Road? Road { get; set; }
        public Block? Block { get; set; }
    }
}
