using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Infrastructure.Logging;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Handles business logic for ProjectInfo.
    /// Validates rules before saving.
    /// Forms call this — never the repository directly.
    /// </summary>
    public class ProjectInfoService : IProjectInfoService
    {
        private readonly IProjectInfoRepository _repo;
        private readonly IAppLogger _logger;

        /// <summary>
        /// Receives dependencies via constructor injection.
        /// Never creates its own dependencies.
        /// </summary>
        public ProjectInfoService(
            IProjectInfoRepository repo,
            IAppLogger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        /// <summary>
        /// Loads ProjectInfo from database.
        /// Returns null if not found.
        /// </summary>
        public async Task<ProjectInfo?> GetAsync(
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInfo(
                    "Loading project info.");

                return await _repo
                    .GetProjectInfoAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to load project info.", ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and saves ProjectInfo.
        /// Throws InvalidOperationException for rule violations.
        /// Form catches this and shows message to user.
        /// </summary>
        public async Task SaveAsync(
            ProjectInfo projectInfo,
            CancellationToken ct = default)
        {
            try
            {
                // Rule 1 — project name is required
                if (string.IsNullOrWhiteSpace(
                    projectInfo.ProjectName))
                    throw new InvalidOperationException(
                        "Project name is required.");

                // Rule 2 — end date cannot be before start date
                if (projectInfo.ProjectStartDate.HasValue &&
                    projectInfo.ProjectEndDate.HasValue &&
                    projectInfo.ProjectEndDate 
                    projectInfo.ProjectStartDate)
                    throw new InvalidOperationException(
                        "Project end date cannot be " +
                        "before start date.");

                _logger.LogInfo(
                    $"Saving project info: " +
                    $"{projectInfo.ProjectName}");

                await _repo.UpdateAsync(
                    projectInfo, ct);

                _logger.LogInfo(
                    "Project info saved successfully.");
            }
            catch (InvalidOperationException)
            {
                // Business rule violation
                // Do not log — just re-throw to form
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Failed to save project info.", ex);
                throw;
            }
        }
    }
}