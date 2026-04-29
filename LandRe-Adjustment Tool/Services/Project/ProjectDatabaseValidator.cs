using System.Text;
using Microsoft.Data.Sqlite;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Performs lightweight validation before a user-selected .lpp file is
    /// opened as SQLite. This keeps random/corrupt files from reaching EF
    /// migrations and producing low-level SQLite errors.
    /// </summary>
    public static class ProjectDatabaseValidator
    {
        private static readonly byte[] SQLiteHeader =
            Encoding.ASCII.GetBytes("SQLite format 3\0");

        /// <summary>
        /// Checks whether a project file is a readable RePlot SQLite database.
        /// </summary>
        /// <param name="projectFilePath">Full path to the selected project file.</param>
        /// <param name="reason">Human-readable reason when validation fails.</param>
        /// <returns><see langword="true"/> when the file can be opened as a RePlot project.</returns>
        public static bool IsValidProjectDatabase(
            string projectFilePath,
            out string reason)
        {
            reason = string.Empty;

            if (string.IsNullOrWhiteSpace(projectFilePath))
            {
                reason = "No project file path was provided.";
                return false;
            }

            if (!File.Exists(projectFilePath))
            {
                reason = "The selected project file does not exist.";
                return false;
            }

            try
            {
                FileInfo fileInfo = new(projectFilePath);
                if (fileInfo.Length < SQLiteHeader.Length)
                {
                    reason = "The selected file is empty or incomplete.";
                    return false;
                }

                if (!HasSQLiteHeader(projectFilePath))
                {
                    reason = "The selected file is not a SQLite/RePlot project database.";
                    return false;
                }

                using SqliteConnection connection = new(BuildReadOnlyConnectionString(projectFilePath));
                connection.Open();

                using SqliteCommand quickCheckCommand = connection.CreateCommand();
                quickCheckCommand.CommandText = "PRAGMA quick_check(1);";
                object? quickCheck = quickCheckCommand.ExecuteScalar();

                if (!string.Equals(
                    Convert.ToString(quickCheck),
                    "ok",
                    StringComparison.OrdinalIgnoreCase))
                {
                    reason = "SQLite integrity check failed.";
                    return false;
                }

                using SqliteCommand projectInfoCommand = connection.CreateCommand();
                projectInfoCommand.CommandText =
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'tblProjectInfo';";
                long projectInfoTableCount = Convert.ToInt64(projectInfoCommand.ExecuteScalar());

                if (projectInfoTableCount == 0)
                {
                    reason = "The database is SQLite, but it is not a RePlot project file.";
                    return false;
                }
            }
            catch (IOException ex)
            {
                reason = BuildFileAccessFailureReason(ex);
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                reason = BuildFileAccessFailureReason(ex);
                return false;
            }
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds the newest valid backup beside a project file.
        /// </summary>
        /// <param name="projectFilePath">Full path to the main project file.</param>
        /// <returns>The backup path when a valid backup exists; otherwise <see langword="null"/>.</returns>
        public static string? FindLatestValidBackup(string projectFilePath)
        {
            for (int backupNumber = 1; backupNumber <= 3; backupNumber++)
            {
                string backupPath = backupNumber == 1
                    ? $"{projectFilePath}.bak"
                    : $"{projectFilePath}.bak{backupNumber}";

                if (IsValidProjectDatabase(backupPath, out _))
                {
                    return backupPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks whether a file should be skipped when copying a project folder.
        /// </summary>
        /// <param name="filePath">Full path of a project-folder file.</param>
        /// <returns><see langword="true"/> when the file is a SQLite sidecar or backup file.</returns>
        public static bool ShouldSkipProjectFolderCopyFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            return fileName.EndsWith("-wal", StringComparison.OrdinalIgnoreCase) ||
                   fileName.EndsWith("-shm", StringComparison.OrdinalIgnoreCase) ||
                   fileName.Contains(".lpp.bak", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads the SQLite file signature without taking an exclusive file handle.
        /// </summary>
        /// <param name="projectFilePath">Full path to the selected project file.</param>
        /// <returns><see langword="true"/> when the expected SQLite signature is present.</returns>
        private static bool HasSQLiteHeader(string projectFilePath)
        {
            Span<byte> header = stackalloc byte[SQLiteHeader.Length];

            using FileStream stream = new(
                projectFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                SQLiteHeader.Length,
                FileOptions.SequentialScan);
            int bytesRead = stream.Read(header);

            return bytesRead == SQLiteHeader.Length &&
                   header.SequenceEqual(SQLiteHeader);
        }

        /// <summary>
        /// Creates a clear validation message for file access failures.
        /// </summary>
        /// <param name="ex">The file access exception raised while validating the project file.</param>
        /// <returns>A user-facing validation failure reason.</returns>
        private static string BuildFileAccessFailureReason(Exception ex)
        {
            return "The selected project file is currently in use or cannot be accessed. " +
                   "Close any other window or process using this project, then try again. " +
                   $"Details: {ex.Message}";
        }

        /// <summary>
        /// Builds a read-only SQLite connection string for validation checks.
        /// </summary>
        /// <param name="projectFilePath">Full path to the selected project file.</param>
        /// <returns>A SQLite connection string that does not use connection pooling.</returns>
        private static string BuildReadOnlyConnectionString(string projectFilePath)
        {
            SqliteConnectionStringBuilder builder = new()
            {
                DataSource = projectFilePath,
                Mode = SqliteOpenMode.ReadOnly,
                Pooling = false
            };

            return builder.ToString();
        }
    }
}
