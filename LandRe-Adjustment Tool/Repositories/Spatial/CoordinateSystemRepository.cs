using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for CoordinateSystem.
    /// </summary>
    public class CoordinateSystemRepository: BaseRepository<CoordinateSystem>, ICoordinateSystemRepository
    {
        public CoordinateSystemRepository(
            ProjectSession session)
            : base(session) { }

        /// <summary>Gets all active CRS records.</summary>
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
        /// Needed when transforming coordinates.
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
                    $"GetWithParametersAsync failed. Id={id}",
                    ex);
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
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.Code == code, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"GetByCodeAsync failed. Code={code}",
                    ex);
                throw;
            }
        }
    }
}
