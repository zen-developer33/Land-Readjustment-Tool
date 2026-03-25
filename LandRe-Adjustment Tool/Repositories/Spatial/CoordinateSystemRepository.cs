using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for CoordinateSystem.
    ///
    /// READ STRATEGY — GetAllActiveAsync:
    /// Uses a TRACKED query (no AsNoTracking).
    /// Same reason as DatumTransformationRepository:
    /// Tracked query loads records into Local cache.
    /// EF Core Identity Map deduplicates automatically —
    /// no double entries when staged records are merged.
    ///
    /// WRITE — staging only. frmMain commits.
    /// </summary>
    public class CoordinateSystemRepository
        : BaseRepository<CoordinateSystem>
        , ICoordinateSystemRepository
    {
        public CoordinateSystemRepository(
            ProjectSession session)
            : base(session) { }

        // ── READ ─────────────────────────────────────

        /// <summary>
        /// Gets all active CRS ordered by display order.
        /// TRACKED — loads into Local cache.
        /// EF Core Identity Map prevents duplicate entries
        /// when staged (Added) records exist in Local cache.
        /// </summary>
        public async Task<List<CoordinateSystem>>
            GetAllActiveAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    "GetAllActiveAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets CRS with projection parameters.
        /// Tracked read.
        /// </summary>
        public async Task<CoordinateSystem?>
            GetWithParametersAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .Include(c => c.ProjectionParameters)
                    .FirstOrDefaultAsync(c => c.Id == id, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"GetWithParametersAsync failed. Id={id}", ex);
                throw;
            }
        }

        /// <summary>Gets CRS by short code.</summary>
        public async Task<CoordinateSystem?> GetByCodeAsync(
            string code,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .FirstOrDefaultAsync(
                        c => c.Code == code, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"GetByCodeAsync failed. Code={code}", ex);
                throw;
            }
        }

        // Staging only — inherited from BaseRepository
    }
}