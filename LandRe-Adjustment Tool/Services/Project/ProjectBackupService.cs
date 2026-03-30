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
        ///
        /// IMPORTANT — caller must do the following BEFORE calling this:
        ///   1. PRAGMA wal_checkpoint(TRUNCATE)  — empties the WAL in-place
        ///      while the connection is still open (no file-lock conflict).
        ///   2. Session.Dispose()
        ///   3. SqliteConnection.ClearAllPools() — forces pool to release handles.
        ///
        /// After those three steps the -wal and -shm files are empty / released,
        /// so we can safely overwrite the .lpp without touching the sidecar files.
        /// SQLite will see an empty WAL on next open and will not replay anything.
        /// </summary>
        public bool RollbackToLatest(string projectFilePath)
        {
            string latestBackup = GetBackupPath(
                projectFilePath, 1);

            if (!File.Exists(latestBackup))
                return false; // No backup exists yet

            // Restore the .lpp from the last good backup.
            // WAL was already emptied by the checkpoint before session dispose.
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