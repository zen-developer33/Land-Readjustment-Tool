using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Infrastructure.Logging;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Handles business logic for ProjectSettings.
    /// Forms call this — never the repository directly.
    /// </summary>
    public class ProjectSettingsService
        : IProjectSettingsService
    {
        private readonly IProjectSettingsRepository _repo;
        private readonly IAppLogger _logger;

        public ProjectSettingsService(
            IProjectSettingsRepository repo,
            IAppLogger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        /// <summary>
        /// Gets project settings from database.
        /// </summary>
        public async Task<ProjectSettings?> GetAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInfo("Loading project settings.");
                return await _repo
                    .GetProjectSettingsAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to load settings.", ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and saves project settings.
        /// Throws InvalidOperationException for violations.
        /// </summary>
        public async Task SaveAsync(
            ProjectSettings settings,
            CancellationToken ct = default)
        {
            try
            {
                // Rule 1 — print scale must be positive
                if (settings.DefaultPrintScale <= 0)
                    throw new InvalidOperationException(
                        "Print scale must be greater than 0.");

                // Rule 2 — min plot area must be positive
                if (settings.MinPlotAreaSqm <= 0)
                    throw new InvalidOperationException(
                        "Minimum plot area must be greater than 0.");

                // Rule 3 — snap tolerance must be positive
                if (settings.SnapTolerancePx <= 0)
                    throw new InvalidOperationException(
                        "Snap tolerance must be greater than 0.");

                _logger.LogInfo("Saving project settings.");
                await _repo.UpdateAsync(settings, ct);
                _logger.LogInfo(
                    "Project settings saved successfully.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to save settings.", ex);
                throw;
            }
        }

        /// <summary>
        /// Marks settings as configured.
        /// Prevents auto-opening on next project open.
        /// </summary>
        public async Task MarkAsConfiguredAsync(
            CancellationToken ct = default)
        {
            try
            {
                await _repo.MarkAsConfiguredAsync(ct);
                _logger.LogInfo(
                    "Settings marked as configured.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "MarkAsConfiguredAsync failed.", ex);
                throw;
            }
        }
    }
}