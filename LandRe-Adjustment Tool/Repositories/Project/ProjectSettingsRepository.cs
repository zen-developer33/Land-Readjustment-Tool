using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Project
{
    /// <summary>
    /// Handles all database operations for ProjectSettings.
    /// Inherits common CRUD from BaseRepository.
    ///
    /// STAGING PATTERN:
    /// All write operations stage changes in EF Core memory.
    /// SaveChangesAsync is called only by frmMain (Ctrl+S).
    /// </summary>
    public class ProjectSettingsRepository
        : BaseRepository<ProjectSettings>
        , IProjectSettingsRepository
    {
        public ProjectSettingsRepository(
            ProjectSession session) : base(session) { }

        /// <summary>
        /// Gets the single ProjectSettings record.
        /// AsNoTracking = read-only, faster query.
        /// Does not stage anything in ChangeTracker.
        /// </summary>
        public async Task<ProjectSettings?>
            GetProjectSettingsAsync(
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
                    "GetProjectSettingsAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Stages IsConfigured = true in EF Core memory.
        /// Does NOT write to database.
        ///
        /// WHY:
        /// This is called after user dismisses the
        /// "Configure settings?" prompt on new project.
        /// The change is staged and written to disk
        /// only when user saves (Ctrl+S).
        ///
        /// This prevents the settings from being
        /// marked as configured in the .lpp file
        /// until the user explicitly saves.
        /// </summary>
        public async Task MarkAsConfiguredAsync(
            CancellationToken ct = default)
        {
            try
            {
                // Must load WITHOUT AsNoTracking
                // so EF Core tracks this entity
                // and picks up the change on SaveChangesAsync
                var settings = await DbSet
                    .FirstOrDefaultAsync(ct);

                if (settings == null)
                {
                    Logger.LogWarning(
                        "MarkAsConfiguredAsync: " +
                        "no settings record found.");
                    return;
                }

                // Stage the change in ChangeTracker
                // EntityState will be Modified
                settings.IsConfigured = true;

                // NOT calling SaveChangesAsync here
                // frmMain.SaveCurrentProjectAsync
                // commits this with all other changes
                Logger.LogInfo(
                    "IsConfigured staged as true.");
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    "MarkAsConfiguredAsync failed.", ex);
                throw;
            }
        }
    }
}