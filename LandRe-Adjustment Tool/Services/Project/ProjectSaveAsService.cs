using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Performs Save As file copying and new project-session creation.
    /// </summary>
    public sealed class ProjectSaveAsService
    {
        private readonly ProjectBackupService _backupService;
        private readonly ProjectSessionFactory _sessionFactory;

        /// <summary>
        /// Creates a Save As service using project backup and session factories.
        /// </summary>
        public ProjectSaveAsService(
            ProjectBackupService backupService,
            ProjectSessionFactory sessionFactory)
        {
            _backupService = backupService
                ?? throw new ArgumentNullException(nameof(backupService));
            _sessionFactory = sessionFactory
                ?? throw new ArgumentNullException(nameof(sessionFactory));
        }

        /// <summary>
        /// Builds the real project folder and project file path from the Save File dialog path.
        /// </summary>
        public ProjectSaveAsTarget CreateTarget(string pickedFilePath)
        {
            string pickedFolder = Path.GetFullPath(
                Path.GetDirectoryName(pickedFilePath)!);
            string pickedFileName = Path.GetFileNameWithoutExtension(pickedFilePath);
            string destinationFolder = Path.Combine(pickedFolder, pickedFileName);
            string destinationFile = Path.Combine(
                destinationFolder,
                Path.GetFileName(pickedFilePath));

            return new ProjectSaveAsTarget(
                destinationFolder,
                destinationFile,
                Path.GetFileNameWithoutExtension(destinationFile));
        }

        /// <summary>
        /// Returns whether the target project folder is inside the current project folder.
        /// </summary>
        public bool IsInsideCurrentProject(string destinationFolder, string currentFolder)
        {
            return IsSameOrChildPath(destinationFolder, currentFolder);
        }

        /// <summary>
        /// Copies the current project folder, renames the copied database, and opens a new context.
        /// </summary>
        public async Task<ProjectContext> SaveAsAsync(
            string currentFilePath,
            ProjectSaveAsTarget target,
            CancellationToken ct = default)
        {
            string currentFolder = Path.GetFullPath(
                Path.GetDirectoryName(currentFilePath)!);

            if (IsSameOrChildPath(target.ProjectFolderPath, currentFolder))
            {
                throw new InvalidOperationException(
                    "Cannot save a project inside its current project folder.");
            }

            await ProjectWalCheckpoint.ExecuteAsync(currentFilePath, ct);

            if (Directory.Exists(target.ProjectFolderPath))
            {
                Directory.Delete(target.ProjectFolderPath, recursive: true);
            }

            CopyProjectFolder(currentFolder, target.ProjectFolderPath);
            MoveCopiedProjectFile(currentFilePath, target.ProjectFilePath);
            ValidateCopiedDatabase(target.ProjectFilePath);

            ProjectSession? session = null;
            try
            {
                session = _sessionFactory.CreateSession(target.ProjectFilePath);
                var context = new ProjectContext(session, target.ProjectFilePath);

                var info = await session.GetDbContext()
                    .ProjectInfo
                    .FirstOrDefaultAsync(ct);

                if (info != null)
                {
                    info.ProjectName = target.ProjectName;
                    await session.GetDbContext().SaveChangesAsync(ct);
                    context.SetInfo(info);
                }

                await ProjectWalCheckpoint.ExecuteAsync(target.ProjectFilePath, ct);
                _backupService.CreateBackup(target.ProjectFilePath);
                context.MarkAsSaved();
                return context;
            }
            catch
            {
                session?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Copies all project files except transient SQLite sidecars and rotated backups.
        /// </summary>
        private static void CopyProjectFolder(string source, string destination)
        {
            string fullSource = Path.GetFullPath(source);
            string fullDestination = Path.GetFullPath(destination);

            if (IsSameOrChildPath(fullDestination, fullSource))
            {
                throw new InvalidOperationException(
                    "Cannot copy a project folder into itself.");
            }

            Directory.CreateDirectory(fullDestination);

            foreach (string directory in Directory.GetDirectories(
                fullSource,
                "*",
                SearchOption.AllDirectories))
            {
                string relativeDirectory = Path.GetRelativePath(fullSource, directory);
                Directory.CreateDirectory(
                    Path.Combine(fullDestination, relativeDirectory));
            }

            foreach (string file in Directory.GetFiles(
                fullSource,
                "*",
                SearchOption.AllDirectories))
            {
                if (ProjectDatabaseValidator.ShouldSkipProjectFolderCopyFile(file))
                {
                    continue;
                }

                string relativePath = Path.GetRelativePath(fullSource, file);
                string destinationFile = Path.Combine(fullDestination, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
                File.Copy(file, destinationFile, overwrite: true);
            }
        }

        /// <summary>
        /// Renames the copied database to match the user-selected Save As file name.
        /// </summary>
        private static void MoveCopiedProjectFile(
            string currentFilePath,
            string destinationFilePath)
        {
            string copiedFile = Path.Combine(
                Path.GetDirectoryName(destinationFilePath)!,
                Path.GetFileName(currentFilePath));

            if (File.Exists(copiedFile) &&
                !string.Equals(
                    copiedFile,
                    destinationFilePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                File.Move(copiedFile, destinationFilePath, overwrite: true);
            }
        }

        /// <summary>
        /// Verifies that the copied project file is still a valid RePlot database.
        /// </summary>
        private static void ValidateCopiedDatabase(string projectFilePath)
        {
            if (!ProjectDatabaseValidator.IsValidProjectDatabase(
                projectFilePath,
                out string validationReason))
            {
                throw new InvalidOperationException(
                    $"The copied project database is invalid: {validationReason}");
            }
        }

        /// <summary>
        /// Returns whether one path is the same as, or a child of, another path.
        /// </summary>
        private static bool IsSameOrChildPath(string candidatePath, string parentPath)
        {
            string candidate = EnsureTrailingDirectorySeparator(
                Path.GetFullPath(candidatePath));
            string parent = EnsureTrailingDirectorySeparator(
                Path.GetFullPath(parentPath));

            return candidate.StartsWith(parent, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Adds a trailing directory separator so path-prefix checks stay precise.
        /// </summary>
        private static string EnsureTrailingDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar) ||
                   path.EndsWith(Path.AltDirectorySeparatorChar)
                ? path
                : $"{path}{Path.DirectorySeparatorChar}";
        }
    }

    /// <summary>
    /// Holds the resolved destination folder, database path, and project name for Save As.
    /// </summary>
    public sealed record ProjectSaveAsTarget(
        string ProjectFolderPath,
        string ProjectFilePath,
        string ProjectName);
}
