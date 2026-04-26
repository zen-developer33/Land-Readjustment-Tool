using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Project
{
    /// <summary>
    /// One record per project. Stores all project-wide settings.
    /// Always Id = 1. Created when project is created.
    /// IsConfigured = false triggers settings window on first open.
    /// </summary>
    [Table("tblProjectSettings")]
    public class ProjectSettings
    {
        [Key]
        public int Id { get; set; }

        // ── TRADITIONAL AREA UNIT ───────────────────
        // Controls display only — all calculations use Sqm internally
        [Required]
        public string TraditionalAreaUnit { get; set; } = "RAPD";
        // "RAPD" → Ropani-Aana-Paisa-Daam (hilly areas)
        // "BKD"  → Bigha-Kattha-Dhur (terai areas)

        // ── COORDINATE SYSTEM ───────────────────────
        // FK to tblCoordinateSystems
        // Null until user sets it in settings window
        public int? CoordinateSystemId { get; set; }

        // Navigation — loads full CRS details
        public Spatial.CoordinateSystem?
            CoordinateSystem
        { get; set; }

        // ── DATUM TRANSFORMATION ─────────────────────
        // FK to tblDatumTransformations
        // Null until user sets it in settings window
        // Required for MUTM zones — not needed for UTM/WGS84
        public int? DatumTransformationId { get; set; }

        // Navigation — loads full transformation parameters
        public Spatial.DatumTransformation?
            DatumTransformation
        { get; set; }

        // ── CANVAS SETTINGS ─────────────────────────
        // Canvas background color in hex
        [Required]
        public string CanvasBackgroundColor { get; set; }
            = "#FFFFFF";

        // Grid line color in hex
        [Required]
        public string CanvasGridColor { get; set; }
            = "#2A3A47";

        // Show grid lines on canvas
        public bool CanvasGridVisible { get; set; } = true;

        // Enable snap to geometry points
        public bool SnapEnabled { get; set; } = true;

        // Snap detection radius in pixels
        public double SnapTolerancePx { get; set; } = 8.0;

        // ── PARCEL NUMBERING ────────────────────────
        // Format for replotted parcel numbers
        [Required]
        public string ParcelNumberFormat { get; set; }
            = "Sequential";
        // "Sequential" → 001, 002, 003
        // "BlockBased" → A-001, B-001
        // "Custom"     → user defined prefix + number

        // Optional prefix e.g. "RP-", "NP-"
        public string? ParcelNumberPrefix { get; set; }

        // Number of digits: 3 = "001", 4 = "0001"
        public int ParcelNumberPadding { get; set; } = 3;

        // ── REPLOTTING RULES ────────────────────────
        // Minimum replotted plot area in Sqm
        // Nepal govt standard = 79.49 Sqm (1 Dhur)
        public double MinPlotAreaSqm { get; set; } = 79.49;

        // ── DOCUMENT SETTINGS ───────────────────────
        // Language for generated documents
        [Required]
        public string DocumentLanguage { get; set; }
            = "English";
        // "English", "Nepali", "Both"

        // Date format used in documents
        [Required]
        public string DateFormat { get; set; } = "AD";
        // "AD"   → 2024-05-12
        // "BS"   → 2081 Bhadra 12
        // "Both" → show both formats

        // ── PRINT SETTINGS ──────────────────────────
        // Default paper size for printing and export
        [Required]
        public string DefaultPaperSize { get; set; } = "A3";
        // "A4", "A3", "A2", "A1"

        // Default print scale e.g. 500 = 1:500
        public int DefaultPrintScale { get; set; } = 500;

        // ── STATUS ──────────────────────────────────
        // false → settings window shown on first project open
        // true  → user confirmed settings, do not auto-open
        public bool IsConfigured { get; set; } = false;

        public DateTime LastModifiedDate { get; set; }
    }
}