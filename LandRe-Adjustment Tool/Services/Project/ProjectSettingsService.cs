using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Infrastructure.Logging;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Business logic for ProjectSettings.
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
            _repo   = repo;
            _logger = logger;
        }

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

        public async Task SaveAsync(
            ProjectSettings settings,
            CancellationToken ct = default)
        {
            // Business rules
            if (settings.DefaultPrintScale <= 0)
                throw new InvalidOperationException(
                    "Print scale must be greater than 0.");

            if (settings.MinPlotAreaSqm <= 0)
                throw new InvalidOperationException(
                    "Minimum plot area must be greater than 0.");

            if (settings.SnapTolerancePx <= 0)
                throw new InvalidOperationException(
                    "Snap tolerance must be greater than 0.");

            try
            {
                _logger.LogInfo("Saving project settings.");
                await _repo.UpdateAsync(settings, ct);
                _logger.LogInfo("Settings saved.");
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

        public async Task MarkAsConfiguredAsync(
            CancellationToken ct = default)
        {
            try
            {
                await _repo.MarkAsConfiguredAsync(ct);
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
