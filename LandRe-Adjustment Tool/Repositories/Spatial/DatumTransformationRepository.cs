using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for DatumTransformation.
    ///
    /// READ STRATEGY — GetAllActiveAsync:
    /// Uses a TRACKED query (no AsNoTracking).
    ///
    /// WHY:
    /// When a new datum is staged via AddAsync, it enters
    /// EF Core Local cache (EntityState = Added, Id = 0).
    ///
    /// If GetAllActiveAsync uses AsNoTracking:
    ///   → DB records load as detached objects
    ///   → Local cache has staged entity separately
    ///   → Merge sees both → duplicate in list
    ///
    /// If GetAllActiveAsync uses tracked query:
    ///   → DB records load INTO Local cache
    ///   → Staged entity is also in Local cache
    ///   → EF Core Identity Map deduplicates automatically
    ///   → Local cache has exactly ONE instance per entity
    ///   → No merge needed — Local is the single source of truth
    ///
    /// WRITE — staging only. frmMain commits.
    /// </summary>
    public class DatumTransformationRepository
        : BaseRepository<DatumTransformation>
        , IDatumTransformationRepository
    {
        public DatumTransformationRepository(
            ProjectSession session)
            : base(session) { }

        // ── READ ─────────────────────────────────────

        /// <summary>
        /// Gets all active datum transformations.
        /// TRACKED query — loads records into Local cache.
        /// EF Core deduplicates — no double entries.
        /// </summary>
        public async Task<List<DatumTransformation>>
            GetAllActiveAsync(
            CancellationToken ct = default)
        {
            try
            {
                // TRACKED — no AsNoTracking
                // Records load into Local cache.
                // Staged entities (Added/Modified) are
                // already in Local — Identity Map ensures
                // no duplicates.
                return await DbSet
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DisplayOrder)
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
        /// Gets transformations applicable to a CRS code.
        /// Lookup only — AsNoTracking fine here.
        /// </summary>
        public async Task<List<DatumTransformation>>
            GetForCoordinateSystemAsync(
            string crsCode,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .Where(d => d.IsActive &&
                        (d.ApplicableCrsCodes == null ||
                         d.ApplicableCrsCodes.Contains(crsCode)))
                    .OrderBy(d => d.DisplayOrder)
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"GetForCoordinateSystemAsync failed." +
                    $" CRS={crsCode}", ex);
                throw;
            }
        }

        /// <summary>Gets transformation by code.</summary>
        public async Task<DatumTransformation?> GetByCodeAsync(
            string code,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        d => d.Code == code, ct);
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