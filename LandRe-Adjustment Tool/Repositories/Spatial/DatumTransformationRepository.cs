using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for DatumTransformation.
    ///
    /// STAGING PATTERN:
    /// Add / Update / Delete stage changes in EF Core memory.
    /// SaveChangesAsync is called ONLY by frmMain:
    ///   → CommitNewProjectAsync (new project creation)
    ///   → SaveCurrentProjectAsync (Ctrl+S / Save menu)
    /// </summary>
    public class DatumTransformationRepository
        : BaseRepository<DatumTransformation>
        , IDatumTransformationRepository
    {
        public DatumTransformationRepository(
            ProjectSession session)
            : base(session) { }

        // ── READ ─────────────────────────────────────

        /// <summary>Gets all active datum transformations.</summary>
        public async Task<List<DatumTransformation>>
            GetAllActiveAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DisplayOrder)
                    .AsNoTracking()
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
        /// Also returns transformations with null codes (global).
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

        // ── WRITE — STAGING ONLY ─────────────────────
        // Inherited from BaseRepository.
        // frmMain commits at the right time.
    }
}