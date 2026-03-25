using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for ProjectionParameters.
    ///
    /// STAGING PATTERN:
    /// Add / Update stage changes in EF Core memory.
    /// SaveChangesAsync is called ONLY by frmMain.
    /// </summary>
    public class ProjectionParametersRepository
        : BaseRepository<ProjectionParameters>
        , IProjectionParametersRepository
    {
        public ProjectionParametersRepository(
            ProjectSession session)
            : base(session) { }

        // ── READ ─────────────────────────────────────

        /// <summary>
        /// Gets projection parameters for a given CRS.
        /// Returns null if not found.
        /// </summary>
        public async Task<ProjectionParameters?>
            GetByCoordinateSystemIdAsync(
            int coordinateSystemId,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        p => p.CoordinateSystemId
                            == coordinateSystemId, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    "GetByCoordinateSystemIdAsync failed.", ex);
                throw;
            }
        }

        // ── WRITE — STAGING ONLY ─────────────────────
        // Inherited from BaseRepository.
        // frmMain commits at the right time.
    }
}