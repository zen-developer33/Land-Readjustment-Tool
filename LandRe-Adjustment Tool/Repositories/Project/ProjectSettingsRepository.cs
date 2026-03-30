using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Project
{
    /// <summary>
    /// Handles all database operations for ProjectSettings.
    /// Inherits common operations from BaseRepository.
    /// </summary>
    public class ProjectSettingsRepository: BaseRepository<ProjectSettings>, IProjectSettingsRepository
    {
        /// <summary>
        /// Receives ProjectSession via constructor.
        /// Passes it to BaseRepository using : base(session).
        /// </summary>
        public ProjectSettingsRepository(ProjectSession session): base(session) { }

        /// <summary>
        /// Gets the single ProjectSettings record.
        /// AsNoTracking = read only, faster query.
        /// </summary>
        public async Task<ProjectSettings?> GetProjectSettingsAsync(CancellationToken ct = default)
        {
            try
            {
                return await DbSet.AsNoTracking().FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError("GetProjectSettingsAsync failed.",ex);
                throw;
            }
        }

        /// <summary>
        /// Marks settings as configured.
        /// IsConfigured = true prevents auto-opening
        /// settings window on next project open.
        /// </summary>
        public async Task MarkAsConfiguredAsync(
            CancellationToken ct = default)
        {
            try
            {
                var settings = await DbSet.FirstOrDefaultAsync(ct);

                if (settings == null)
                {
                    Logger.LogWarning("MarkAsConfiguredAsync: " + "no settings record found.");
                    return;
                }

                settings.IsConfigured = true;
                await Context.SaveChangesAsync(ct);
                // Detach immediately — keeps the ChangeTracker clean.
                Context.Entry(settings).State = EntityState.Detached;
                Logger.LogInfo("Settings marked as configured and saved.");
            }
            catch (Exception ex)
            {
                Logger.LogError( "MarkAsConfiguredAsync failed.",
                    ex);
                throw;
            }
        }
    }
}