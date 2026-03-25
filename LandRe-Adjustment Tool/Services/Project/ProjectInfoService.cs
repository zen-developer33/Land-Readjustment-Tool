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
                _logger.LogInfo("Loading project info.");
                return await _repo.GetProjectInfoAsync(ct);
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
                    projectInfo.ProjectEndDate.Value < projectInfo.ProjectStartDate.Value)
                    throw new InvalidOperationException(
                        "Project end date cannot be " +
                        "before start date.");

                // Rule 3 — gazette date cannot be in future
                if (projectInfo.GazetteDate.HasValue &&
                    projectInfo.GazetteDate.Value > DateTime.Now)
                    throw new InvalidOperationException(
                        "Gazette date cannot be " +
                        "in the future.");

                _logger.LogInfo(
                    $"Staging project info: " +
                    $"{projectInfo.ProjectName}");

                var tracked = await _repo
                    .GetProjectInfoAsync(ct);

                if (tracked == null)
                    throw new InvalidOperationException(
                        "Project info record not found.");

                tracked.Province = projectInfo.Province;
                tracked.District = projectInfo.District;
                tracked.Municipality = projectInfo.Municipality;
                tracked.WardNo = projectInfo.WardNo;
                tracked.ProjectSite = projectInfo.ProjectSite;
                tracked.ImplementingAgency = projectInfo.ImplementingAgency;
                tracked.ConsultingAgency = projectInfo.ConsultingAgency;
                tracked.GazetteDate = projectInfo.GazetteDate;
                tracked.ProjectStartDate = projectInfo.ProjectStartDate;
                tracked.ProjectEndDate = projectInfo.ProjectEndDate;
                tracked.ProjectNotes = projectInfo.ProjectNotes;

                _logger.LogInfo(
                    "Project info changes staged. " +
                    "Will be persisted on project save.");
            }
            catch (InvalidOperationException)
            {
                // Business rule violation
                // Re-throw to form — no logging needed
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