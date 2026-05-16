using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Opens and validates project databases without placing database workflow in forms.
    /// </summary>
    public sealed class ProjectOpenService
    {
        private readonly ProjectSessionFactory _sessionFactory;
        private readonly IProjectScopedFactory _projectScopedFactory;

        /// <summary>
        /// Creates a project open service from session and project service factories.
        /// </summary>
        public ProjectOpenService(
            ProjectSessionFactory sessionFactory,
            IProjectScopedFactory projectScopedFactory)
        {
            _sessionFactory = sessionFactory
                ?? throw new ArgumentNullException(nameof(sessionFactory));
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
        }

        /// <summary>
        /// Checks whether a selected project file can be opened and reports a valid backup if available.
        /// </summary>
        public ProjectOpenCheck CheckProjectFile(string projectFilePath)
        {
            if (ProjectDatabaseValidator.IsValidProjectDatabase(
                projectFilePath,
                out string invalidReason))
            {
                return ProjectOpenCheck.Valid();
            }

            string? validBackup =
                ProjectDatabaseValidator.FindLatestValidBackup(projectFilePath);
            return ProjectOpenCheck.Invalid(invalidReason, validBackup);
        }

        /// <summary>
        /// Restores the selected project file from a validated backup.
        /// </summary>
        public void RestoreBackup(string backupPath, string projectFilePath)
        {
            File.Copy(backupPath, projectFilePath, overwrite: true);
        }

        /// <summary>
        /// Opens a project session, migrates schema, and returns a ready project context.
        /// </summary>
        public async Task<ProjectContext> OpenAsync(
            string projectFilePath,
            CancellationToken ct = default)
        {
            ProjectSession? session = null;

            try
            {
                session = _sessionFactory.CreateSession(projectFilePath);
                await session.GetDbContext().Database.MigrateAsync(ct);
                await ProjectDatabaseCompatibility.EnsureAsync(
                    session.GetDbContext(),
                    ct);

                var service = _projectScopedFactory.CreateProjectInfoService(session);
                var info = await service.GetAsync();
                if (info == null)
                {
                    throw new InvalidOperationException(
                        "Project file is invalid or missing project information.");
                }

                var context = new ProjectContext(session, projectFilePath);
                context.SetInfo(info);
                return context;
            }
            catch (Exception ex)
            {
                session?.Logger.LogError(
                    $"Project open failed. Path={projectFilePath}",
                    ex);
                session?.Dispose();
                throw;
            }
        }
    }

    /// <summary>
    /// Describes project file validation results before opening a database.
    /// </summary>
    public sealed record ProjectOpenCheck(
        bool CanOpen,
        string Reason,
        string? ValidBackupPath)
    {
        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static ProjectOpenCheck Valid()
        {
            return new ProjectOpenCheck(true, string.Empty, null);
        }

        /// <summary>
        /// Creates a failed validation result with an optional recoverable backup path.
        /// </summary>
        public static ProjectOpenCheck Invalid(string reason, string? validBackupPath)
        {
            return new ProjectOpenCheck(false, reason, validBackupPath);
        }
    }
}
