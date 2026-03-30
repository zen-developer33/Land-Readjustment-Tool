namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Handles backup rotation and restore
    /// for .lpp project files.
    ///
    /// Backup naming:
    ///   MyProject.lpp.bak   ← most recent
    ///   MyProject.lpp.bak2  ← one before
    ///   MyProject.lpp.bak3
    ///   MyProject.lpp.bak4
    ///   MyProject.lpp.bak5  ← oldest
    ///
    /// On every save:
    ///   .bak4 → .bak5  (oldest dropped)
    ///   .bak3 → .bak4
    ///   .bak2 → .bak3
    ///   .bak  → .bak2
    ///   current .lpp → .bak
    /// </summary>
    public class ProjectBackupService
    {
        private const int MaxBackups = 5;

        /// <summary>
        /// Creates a backup of the project file
        /// before saving. Rotates existing backups.
        /// Call this BEFORE SaveChangesAsync.
        /// </summary>
        public void CreateBackup(string projectFilePath)
        {
            if (!File.Exists(projectFilePath)) return;

            RotateBackups(projectFilePath);
        }

        /// <summary>
        /// Returns all existing backup files
        /// ordered from most recent to oldest.
        /// </summary>
        public List<BackupEntry> GetBackups(
            string projectFilePath)
        {
            var backups = new List<BackupEntry>();

            for (int i = 1; i <= MaxBackups; i++)
            {
                string path = GetBackupPath(
                    projectFilePath, i);

                if (!File.Exists(path)) continue;

                var info = new FileInfo(path);
                backups.Add(new BackupEntry
                {
                    Number = i,
                    FilePath = path,
                    ModifiedDate = info.LastWriteTime,
                    FileSizeBytes = info.Length
                });
            }

            return backups;
        }

        /// <summary>
        /// Restores project from a specific backup.
        /// Caller must close DB session first.
        /// </summary>
        public void RestoreFromBackup(
            string projectFilePath,
            string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                throw new FileNotFoundException(
                    "Backup file not found.",
                    backupFilePath);

            File.Copy(backupFilePath,
                projectFilePath, overwrite: true);
        }

        // ── PRIVATE ──────────────────────────────────

        private void RotateBackups(string projectFilePath)
        {
            // Delete oldest backup
            string oldest = GetBackupPath(
                projectFilePath, MaxBackups);
            if (File.Exists(oldest))
                File.Delete(oldest);

            // Shift: .bak(n-1) → .bak(n)
            for (int i = MaxBackups - 1; i >= 1; i--)
            {
                string current = GetBackupPath(
                    projectFilePath, i);
                string next = GetBackupPath(
                    projectFilePath, i + 1);

                if (File.Exists(current))
                    File.Move(current, next,
                        overwrite: true);
            }

            // Copy current .lpp → .bak
            string latest = GetBackupPath(
                projectFilePath, 1);
            File.Copy(projectFilePath,
                latest, overwrite: true);
        }

        /// <summary>
        /// Rolls back .lpp to most recent backup.
        /// Call this when user discards unsaved changes.
        /// Caller must close the DB session first.
        ///
        /// WHY we delete -wal and -shm:
        ///   SQLite uses WAL (Write-Ahead Log) mode.  Every SaveChangesAsync
        ///   writes to a .lpp-wal sidecar file first — the main .lpp is only
        ///   updated during a checkpoint (Ctrl+S).  If we only copy .bak → .lpp
        ///   and leave the -wal file in place, SQLite will replay those WAL
        ///   entries the next time the project is opened, making the "discarded"
        ///   changes reappear.  Deleting -wal and -shm ensures the restored
        ///   .lpp is the authoritative source of truth.
        /// </summary>
        public bool RollbackToLatest(string projectFilePath)
        {
            string latestBackup = GetBackupPath(
                projectFilePath, 1);

            if (!File.Exists(latestBackup))
                return false; // No backup exists yet

            // 1. Remove WAL and SHM sidecar files BEFORE restoring the .lpp.
            //    These files contain the uncommitted changes the user wants
            //    to discard.  Leaving them would cause SQLite to replay them
            //    on next open, defeating the entire rollback.
            string walFile = projectFilePath + "-wal";
            string shmFile = projectFilePath + "-shm";

            if (File.Exists(walFile)) File.Delete(walFile);
            if (File.Exists(shmFile)) File.Delete(shmFile);

            // 2. Restore the .lpp from the last good backup.
            File.Copy(latestBackup,
                projectFilePath, overwrite: true);

            return true;
        }
        private static string GetBackupPath(
            string projectFilePath, int number)
        {
            return number == 1
                ? $"{projectFilePath}.bak"
                : $"{projectFilePath}.bak{number}";
        }
    }

    /// <summary>
    /// Represents a single backup file entry.
    /// Used by frmBackupManager to display list.
    /// </summary>
    public class BackupEntry
    {
        public int Number { get; set; }
        public string FilePath { get; set; } = "";
        public DateTime ModifiedDate { get; set; }
        public long FileSizeBytes { get; set; }

        public string FormattedSize
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = FileSizeBytes;
                int order = 0;
                while (len >= 1024 &&
                       order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        public string Label =>
            Number == 1
                ? "Latest Backup"
                : $"Backup {Number}";
    }
}