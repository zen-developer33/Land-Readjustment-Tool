using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Spatial
{
    /// <summary>
    /// Stores datum transformation parameters (Helmert 7-parameter).
    /// Used to convert between local datum (Everest) and WGS84.
    /// 
    /// SEEDED DEFAULTS:
    /// → Nepal Survey Department (official, recommended)
    /// → Nagarkot GPS Campaign 1994
    /// → Kalianpur Datum Parameters
    /// → WGS84 Identity (no transformation)
    /// 
    /// USER CAN ADD:
    /// Custom parameters for specific project areas.
    /// IsSystemDefault = false for user-added entries.
    /// 
    /// USAGE:
    /// ProjectSettings.DatumTransformationId picks
    /// which transformation to use for this project.
    /// CoordinateTransformService reads parameters and
    /// applies forward (local→WGS84) or inverse (WGS84→local).
    /// </summary>
    [Table("tblDatumTransformations")]
    public class DatumTransformation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>Short unique code. e.g. NEPAL_SURV_DEPT</summary>
        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>Full display name shown in UI dropdown.</summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        // ── SOURCE AND TARGET DATUM ──────────────────

        /// <summary>
        /// Source datum name.
        /// e.g. "Everest1830" for Nepal MUTM zones.
        /// "WGS84" for UTM44N/45N zones.
        /// </summary>
        [Required]
        public string SourceDatum { get; set; }
            = string.Empty;

        /// <summary>
        /// Target datum. Always WGS84 for our use.
        /// Stored for clarity and future use.
        /// </summary>
        [Required]
        public string TargetDatum { get; set; } = "WGS84";

        // ── HELMERT 7-PARAMETER TRANSFORM ───────────
        // Parameters shift coordinates from source to target datum.
        // Forward  = source → WGS84 (for KML export)
        // Inverse  = WGS84 → source (for satellite import)
        // Inverse is calculated by negating translation values.

        /// <summary>X translation in meters.</summary>
        public double DeltaX { get; set; } = 0;

        /// <summary>Y translation in meters.</summary>
        public double DeltaY { get; set; } = 0;

        /// <summary>Z translation in meters.</summary>
        public double DeltaZ { get; set; } = 0;

        /// <summary>X rotation in arc-seconds.</summary>
        public double RotationX { get; set; } = 0;

        /// <summary>Y rotation in arc-seconds.</summary>
        public double RotationY { get; set; } = 0;

        /// <summary>Z rotation in arc-seconds.</summary>
        public double RotationZ { get; set; } = 0;

        /// <summary>
        /// Scale difference in ppm (parts per million).
        /// 0 = no scale change.
        /// </summary>
        public double ScalePpm { get; set; } = 0;

        // ── APPLICABLE CRS ───────────────────────────

        /// <summary>
        /// Comma separated CRS codes this transformation applies to.
        /// e.g. "MUTM81,MUTM82,MUTM83"
        /// Used to filter dropdown in settings UI.
        /// Null = applies to all coordinate systems.
        /// </summary>
        public string? ApplicableCrsCodes { get; set; }

        // ── METADATA ─────────────────────────────────

        /// <summary>
        /// Source of these parameters.
        /// e.g. "Survey Department Nepal 2012"
        /// Helps user choose the right transformation.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Geographic area where valid.
        /// e.g. "Nepal", "Central Nepal", "Global"
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// True = seeded record, cannot be deleted.
        /// False = user added, deletable if unused.
        /// </summary>
        public bool IsSystemDefault { get; set; } = false;

        /// <summary>False = soft deleted, hidden from UI.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Order in UI dropdown.</summary>
        public int DisplayOrder { get; set; } = 0;

        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Name) ? Code : Name;
        }
    }
}