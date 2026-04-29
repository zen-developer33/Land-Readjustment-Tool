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

            try
            {
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
            catch (Exception ex)
            {
                reason = ex.Message;
                return false;
            }

            return true;
        }

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

        public static bool ShouldSkipProjectFolderCopyFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            return fileName.EndsWith("-wal", StringComparison.OrdinalIgnoreCase) ||
                   fileName.EndsWith("-shm", StringComparison.OrdinalIgnoreCase) ||
                   fileName.Contains(".lpp.bak", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasSQLiteHeader(string projectFilePath)
        {
            Span<byte> header = stackalloc byte[SQLiteHeader.Length];

            using FileStream stream = File.OpenRead(projectFilePath);
            int bytesRead = stream.Read(header);

            return bytesRead == SQLiteHeader.Length &&
                   header.SequenceEqual(SQLiteHeader);
        }

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
