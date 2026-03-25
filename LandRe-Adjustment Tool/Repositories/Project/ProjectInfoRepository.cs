using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Project
{
    /// <summary>
    /// Handles database operations for ProjectInfo.
    ///
    /// READ STRATEGY — TRACKED (no AsNoTracking):
    /// EF Core Identity Map returns the in-memory staged
    /// version if already loaded and modified.
    ///
    ///   1. Form opens → reads disk → tracked (Unchanged)
    ///   2. User edits → CollectFormData modifies entity
    ///   3. UpdateAsync stages it (Modified in ChangeTracker)
    ///   4. Form re-opens → GetProjectInfoAsync returns SAME
    ///      tracked entity with staged edits intact ✅
    ///   5. Ctrl+S → SaveChangesAsync commits to disk ✅
    ///   6. Close without save → ChangeTracker.Clear() ✅
    /// </summary>
    public class ProjectInfoRepository
        : BaseRepository<ProjectInfo>
        , IProjectInfoRepository
    {
        public ProjectInfoRepository(
            ProjectSession session) : base(session) { }

        /// <summary>
        /// Gets the single ProjectInfo record.
        /// TRACKED — no AsNoTracking.
        /// </summary>
        public async Task<ProjectInfo?> GetProjectInfoAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet.FirstOrDefaultAsync(ct);
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