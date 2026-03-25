using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Project
{
    /// <summary>
    /// Handles database operations for ProjectSettings.
    ///
    /// READ STRATEGY — TRACKED (no AsNoTracking):
    /// EF Core Identity Map returns in-memory staged version.
    ///
    ///   1. Form opens → reads disk → tracked (Unchanged)
    ///   2. User changes snap to 7 → entity Modified
    ///   3. btnSave → UpdateAsync (already Modified, no-op)
    ///   4. Form re-opens → returns same tracked entity (7) ✅
    ///   5. Ctrl+S → commits 7 to disk ✅
    ///   6. Close without save → ChangeTracker.Clear() → 8 ✅
    ///
    /// WRITE STRATEGY — STAGING ONLY:
    /// frmMain commits via SaveChangesAsync.
    /// </summary>
    public class ProjectSettingsRepository
        : BaseRepository<ProjectSettings>
        , IProjectSettingsRepository
    {
        public ProjectSettingsRepository(
            ProjectSession session) : base(session) { }

        /// <summary>
        /// Gets the single ProjectSettings record.
        /// TRACKED — no AsNoTracking.
        /// </summary>
        public async Task<ProjectSettings?> GetProjectSettingsAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet.FirstOrDefaultAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    "GetProjectSettingsAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Stages IsConfigured = true.
        /// Property assignment on tracked entity auto-sets Modified.
        /// Committed by frmMain.
        /// </summary>
        public async Task MarkAsConfiguredAsync(
            CancellationToken ct = default)
        {
            try
            {
                var settings = await DbSet
                    .FirstOrDefaultAsync(ct);

                if (settings == null)
                {
                    Logger.LogWarning(
                        "MarkAsConfiguredAsync: no settings found.");
                    return;
                }

                settings.IsConfigured = true;
                Logger.LogInfo("IsConfigured staged.");
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