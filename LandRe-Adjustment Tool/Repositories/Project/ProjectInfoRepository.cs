using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;


namespace Land_Readjustment_Tool.Repositories.Project
{
    /// <summary>
    /// Handles all database operations for ProjectInfo.
    /// Inherits common operations from BaseRepository.
    /// Adds ProjectInfo specific method GetProjectInfoAsync.
    /// </summary>
    public class ProjectInfoRepository: BaseRepository<ProjectInfo>, IProjectInfoRepository
    {
        /// <summary>
        /// Receives ProjectSession via constructor.
        /// Passes it to BaseRepository using : base(session).
        /// </summary>
        public ProjectInfoRepository(ProjectSession session): base(session) { }

        /// <summary>
        /// Gets the single ProjectInfo record.
        /// AsNoTracking = read only, faster query.
        /// </summary>
        public async Task<ProjectInfo?> GetProjectInfoAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    "GetProjectInfoAsync failed.", ex);
                throw;
            }
        }
    }
}