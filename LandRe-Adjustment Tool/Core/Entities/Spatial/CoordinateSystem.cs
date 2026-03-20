using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Land_Readjustment_Tool.Core.Entities.Spatial
{
    /// <summary>
    /// Master data table for coordinate reference systems.
    /// Stores CRS identity only — not projection math.
    /// Projection parameters stored in tblProjectionParameters.
    /// Datum shift parameters stored in tblDatumTransformations.
    /// 
    /// NEPAL DEFAULTS (seeded via migration):
    /// UTM44N, UTM45N — standard WGS84 based
    /// MUTM81, MUTM82, MUTM83 — Nepal Survey Dept
    /// WGS84 — geographic lat/long
    /// 
    /// FUTURE EXPANSION:
    /// User adds new CRS + ProjectionParameters row.
    /// IsSystemDefault = false for user-added entries.
    /// </summary>
    [Table("tblCoordinateSystems")]
    public class CoordinateSystem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Short unique code used in code and filters.
        /// e.g. UTM44N, UTM45N, MUTM81, MUTM82, MUTM83, WGS84
        /// </summary>
        [Required]
        public string Code { get; set; } = string.Empty;

        /// <summary>Full display name shown in UI.</summary>
        [Required]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Standard EPSG code.
        /// UTM44N = 32644, UTM45N = 32645, WGS84 = 4326.
        /// Null for Nepal MUTM — no standard EPSG exists.
        /// ProjNET uses this for automatic transformation.
        /// </summary>
        public int? EpsgCode { get; set; }

        /// <summary>
        /// Projection type name.
        /// "TransverseMercator" for UTM and MUTM.
        /// "Geographic" for WGS84 lat/long.
        /// Used by CoordinateTransformService to select algorithm.
        /// </summary>
        public string? ProjectionType { get; set; }

        /// <summary>
        /// Country or region this CRS is used in.
        /// e.g. "Nepal", "Global"
        /// Used for filtering in UI dropdown.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// True = seeded record, cannot be deleted.
        /// False = user added, deletable if unused.
        /// </summary>
        public bool IsSystemDefault { get; set; } = false;

        /// <summary>False = soft deleted, hidden from UI.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Order in UI dropdown list.</summary>
        public int DisplayOrder { get; set; } = 0;

        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }

        // ── NAVIGATION ───────────────────────────────

        /// <summary>
        /// Projection parameters for this CRS.
        /// Null for standard EPSG systems —
        /// ProjNET handles them automatically.
        /// Required for MUTM zones.
        /// </summary>
        public ProjectionParameters? ProjectionParameters
        { get; set; }
    }
}