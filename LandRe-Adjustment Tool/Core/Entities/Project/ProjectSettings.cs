using LandRe_AdjustmentTool.Properties;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Project
{
    [Table("tblProjectSettings")]
    public class ProjectSettings
    {
        [Key]
        public int Id { get; set; }
        // Always Id = 1
        // One settings record per project

        // ── TRADITIONAL AREA UNIT ───────────────────
        [Required]
        public string TraditionalAreaUnit { get; set; } = "RAPD";
        // "RAPD" → Ropani-Aana-Paisa-Daam (hilly)
        // "BKD"  → Bigha-Kattha-Dhur (terai)
        // NOTE: All calculations always use Sqm
        //       This setting controls display only

        // ── COORDINATE SYSTEM ───────────────────────
        public string? CoordinateSystem { get; set; }
        // "UTM44N" → EPSG 32644 western Nepal
        // "UTM45N" → EPSG 32645 eastern Nepal
        // "MUTM"   → Modified UTM Nepal
        // "WGS84"  → Geographic lat/long
        // null until user sets it during project setup

        public int? EpsgCode { get; set; }
        // 32644 for UTM44N
        // 32645 for UTM45N
        // null until user sets it

        public string MapUnit { get; set; } = "Meters";
        // "Meters" or "Feet"

        // ── CANVAS SETTINGS ─────────────────────────
        [Required]
        public string CanvasBackgroundColor { get; set; } = "#1E1E1E";
        // dark background default

        [Required]
        public string CanvasGridColor { get; set; } = "#333333";

        public bool CanvasGridVisible { get; set; } = true;

        public bool SnapEnabled { get; set; } = true;

        public double SnapTolerancePx { get; set; } = 8.0;

        // ── PARCEL NUMBERING ────────────────────────
        [Required]
        public string ParcelNumberFormat { get; set; } = "Sequential";
        // "Sequential"  → 001, 002, 003
        // "BlockBased"  → A-001, B-001
        // "Custom"      → user defined

        public string? ParcelNumberPrefix { get; set; }
        // e.g. "RP-", "NP-", ""

        public int ParcelNumberPadding { get; set; } = 3;
        // 3 → "001"
        // 4 → "0001"

        // ── REPLOTTING RULES ────────────────────────
        public double MinPlotAreaSqm { get; set; } = 79.49;
        // minimum allowed replotted plot area
        // government standard minimum

        // ── DOCUMENT SETTINGS ───────────────────────
        [Required]
        public string DocumentLanguage { get; set; } = "English";
        // "English"
        // "Nepali"
        // "Both"

        [Required]
        public string DateFormat { get; set; } = "AD";
        // "AD" → 2024-05-12
        // "BS" → 2081 Bhadra 12
        // "Both" → show both in documents

        // ── PRINT SETTINGS ──────────────────────────
        [Required]
        public string DefaultPaperSize { get; set; } = "A3";
        // "A4", "A3", "A2", "A1"

        public int DefaultPrintScale { get; set; } = 500;
        // 500 means 1:500

        // ── METADATA ────────────────────────────────
        public bool IsConfigured { get; set; } = false;
        // false → settings window shown on first open
        // true  → user has confirmed settings

        public DateTime LastModifiedDate { get; set; }
    }
}