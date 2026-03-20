using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Spatial
{
    /// <summary>
    /// Stores mathematical projection parameters for a CRS.
    /// Separated from CoordinateSystem for single responsibility.
    /// 
    /// One row per CoordinateSystem that needs custom parameters.
    /// Standard EPSG systems (UTM44N, UTM45N, WGS84) do not need
    /// this — ProjNET looks them up from EPSG code automatically.
    /// 
    /// Required for Nepal MUTM zones (no EPSG code exists).
    /// WktDefinition overrides all individual parameters if provided.
    /// </summary>
    [Table("tblProjectionParameters")]
    public class ProjectionParameters
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// FK to tblCoordinateSystems.
        /// One set of parameters per coordinate system.
        /// </summary>
        [Required]
        public int CoordinateSystemId { get; set; }

        // ── TRANSVERSE MERCATOR PARAMETERS ──────────
        // Used for UTM and MUTM projections

        /// <summary>
        /// Central meridian in decimal degrees.
        /// MUTM81 = 81.0, MUTM82 = 84.0, MUTM83 = 87.0
        /// </summary>
        public double? CentralMeridian { get; set; }

        /// <summary>
        /// Latitude of natural origin in decimal degrees.
        /// Usually 0 for UTM and MUTM.
        /// </summary>
        public double? LatitudeOfOrigin { get; set; }

        /// <summary>
        /// Scale factor at central meridian.
        /// Standard UTM = 0.9996
        /// Nepal MUTM   = 0.9999 (different from standard)
        /// </summary>
        public double? ScaleFactor { get; set; }

        /// <summary>
        /// False easting offset in meters.
        /// Prevents negative X coordinates.
        /// UTM and MUTM = 500000.0
        /// </summary>
        public double? FalseEasting { get; set; }

        /// <summary>
        /// False northing offset in meters.
        /// Northern hemisphere = 0
        /// Southern hemisphere = 10000000
        /// </summary>
        public double? FalseNorthing { get; set; }

        // ── ELLIPSOID PARAMETERS ─────────────────────
        // Stored here when no EPSG code available
        // For standard EPSG systems ProjNET handles ellipsoid

        /// <summary>
        /// Ellipsoid name.
        /// "WGS84" for standard UTM zones.
        /// "Everest1830" for Nepal MUTM zones.
        /// </summary>
        public string? Ellipsoid { get; set; }

        /// <summary>
        /// Semi-major axis of ellipsoid in meters.
        /// WGS84    = 6378137.0
        /// Everest  = 6377276.345
        /// Null = ProjNET uses ellipsoid from EpsgCode.
        /// </summary>
        public double? SemiMajorAxis { get; set; }

        /// <summary>
        /// Inverse flattening of ellipsoid.
        /// WGS84    = 298.257223563
        /// Everest  = 300.8017
        /// Null = ProjNET uses ellipsoid from EpsgCode.
        /// </summary>
        public double? InverseFlattening { get; set; }

        // ── WKT OVERRIDE ────────────────────────────

        /// <summary>
        /// Complete WKT (Well Known Text) definition.
        /// If provided — used directly by ProjNET.
        /// Overrides all individual parameters above.
        /// Most complete and accurate option when available.
        /// </summary>
        public string? WktDefinition { get; set; }

        // ── NAVIGATION ───────────────────────────────

        /// <summary>Parent coordinate system.</summary>
        public CoordinateSystem CoordinateSystem { get; set; }
            = null!;
    }
}