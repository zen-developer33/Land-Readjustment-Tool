using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Spatial
{
    /// <summary>
    /// Handles database operations for ProjectionParameters.
    /// </summary>
    public class ProjectionParametersRepository
        : BaseRepository<ProjectionParameters>
        , IProjectionParametersRepository
    {
        public ProjectionParametersRepository(
            ProjectSession session)
            : base(session) { }

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
                    "GetByCoordinateSystemIdAsync failed.",
                    ex);
                throw;
            }
        }
    }
}
