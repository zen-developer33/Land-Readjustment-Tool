using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Forms.Design.Behavior;

namespace Land_Readjustment_Tool.Core.Entities.Canvas
{
    [Table("tblCanvasLayers")]
    public class CanvasLayer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string LayerType { get; set; } = string.Empty;
        // "BaselineParcel"
        // "ProposedRoad"
        // "ExistingRoad"
        // "Block"
        // "ProjectBoundary"
        // "ReplottedParcel"
        // "Annotation"
        // "Reference"

        // ── VISIBILITY & BEHAVIOUR ──────────────────
        public bool IsVisible { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public bool IsSelectable { get; set; } = true;
        public bool IsPrintable { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        // Lower = bottom, Higher = top

        // ── BORDER / LINE STYLE ─────────────────────
        public string BorderColor { get; set; } = "#000000";
        // Hex color e.g. "#FF0000"

        public double LineWeight { get; set; } = 1.0;
        // In pixels

        public string LineStyle { get; set; } = "Solid";
        // "Solid", "Dashed", "Dotted", "DashDot"

        // ── FILL STYLE ──────────────────────────────
        public string? FillColor { get; set; }
        // Hex color, null = no fill

        public int FillTransparency { get; set; } = 0;
        // 0 = fully opaque, 100 = fully transparent

        public string FillStyle { get; set; } = "Solid";
        // "Solid", "Hatched", "None"

        public string? HatchPattern { get; set; }
        // e.g. "ANSI31", "ANSI32" — only used when
        // FillStyle = "Hatched"

        // ── TEXT / LABEL STYLE ──────────────────────
        public bool ShowLabels { get; set; } = false;
        public string? LabelFontName { get; set; }
        public double LabelFontSize { get; set; } = 10.0;
        public string LabelColor { get; set; } = "#000000";
        public string? LabelField { get; set; }
        // Which field to show as label
        // e.g. "ParcelNo", "OwnerName", "AreaSqm"

        // ── POINT / NODE STYLE ──────────────────────
        public string PointSymbol { get; set; } = "Circle";
        // "Circle", "Square", "Cross", "Triangle"
        public double PointSize { get; set; } = 5.0;

        // ── SOURCE ──────────────────────────────────
        public string? SourceFile { get; set; }
        // DXF/DWG/Shapefile path if imported
        public DateTime? ImportedDate { get; set; }

        // ── METADATA ────────────────────────────────
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        public ICollection<CanvasObject> CanvasObjects { get; set; } = [];
    }
}