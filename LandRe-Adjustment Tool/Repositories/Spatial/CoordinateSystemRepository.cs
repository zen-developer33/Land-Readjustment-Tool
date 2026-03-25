using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for CoordinateSystem.
    ///
    /// STAGING PATTERN:
    /// Add / Update / Delete stage changes in EF Core memory.
    /// SaveChangesAsync is called ONLY by frmMain:
    ///   → CommitNewProjectAsync (new project creation)
    ///   → SaveCurrentProjectAsync (Ctrl+S / Save menu)
    ///
    /// LoadAsync in manage forms reads both committed DB records
    /// AND staged local cache so new/edited records appear
    /// immediately in the list without requiring a save first.
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
        /// AsNoTracking — reads committed DB records only.
        /// Manage forms merge this with EF Core Local cache
        /// to show staged records too.
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
        /// Gets CRS with projection parameters included.
        /// Used when transforming coordinates.
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
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.Id == id, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"GetWithParametersAsync failed. Id={id}", ex);
                throw;
            }
        }

        /// <summary>Gets CRS by short code e.g. "MUTM82".</summary>
        public async Task<CoordinateSystem?> GetByCodeAsync(
            string code,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
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

        // ── WRITE — STAGING ONLY ─────────────────────
        // Inherited from BaseRepository.
        // AddAsync / UpdateAsync / DeleteAsync
        // stage changes in EF Core ChangeTracker only.
        // frmMain commits at the right time.
    }
}