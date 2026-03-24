namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Handles backup rotation and restore
    /// for .lpp project files.
    ///
    /// Backup naming:
    ///   MyProject.lpp.bak   ? most recent
    ///   MyProject.lpp.bak2  ? one before
    ///   MyProject.lpp.bak3
    ///   MyProject.lpp.bak4
    ///   MyProject.lpp.bak5  ? oldest
    ///
    /// On every save:
    ///   .bak4 ? .bak5  (oldest dropped)
    ///   .bak3 ? .bak4
    ///   .bak2 ? .bak3
    ///   .bak  ? .bak2
    ///   current .lpp ? .bak
    ///
    /// Note:
    /// SQLite WAL mode may keep committed data in
    /// sidecar files (-wal / -shm). Backup rotation
    /// and restore also handle these files.
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

            // Restore WAL sidecars if available.
            // If backup has no sidecar, delete current sidecar
            // so restored main file is not overridden.
            CopyOrDeleteSidecar(
                GetWalPath(backupFilePath),
                GetWalPath(projectFilePath));
            CopyOrDeleteSidecar(
                GetShmPath(backupFilePath),
                GetShmPath(projectFilePath));
        }

        // -- PRIVATE ----------------------------------

        private void RotateBackups(string projectFilePath)
        {
            // Delete oldest backup (main + sidecars)
            DeleteBackupWithSidecars(GetBackupPath(
                projectFilePath, MaxBackups));

            // Shift: .bak(n-1) ? .bak(n)
            for (int i = MaxBackups - 1; i >= 1; i--)
            {
                string current = GetBackupPath(
                    projectFilePath, i);
                string next = GetBackupPath(
                    projectFilePath, i + 1);

                MoveBackupWithSidecars(current, next);
            }

            // Copy current .lpp ? .bak
            string latest = GetBackupPath(
                projectFilePath, 1);
            File.Copy(projectFilePath,
                latest, overwrite: true);

            // Copy SQLite WAL sidecars if they exist
            CopyIfExists(
                GetWalPath(projectFilePath),
                GetWalPath(latest));
            CopyIfExists(
                GetShmPath(projectFilePath),
                GetShmPath(latest));
        }

        /// <summary>
        /// Rolls back .lpp to most recent backup.
        /// Call this when user discards unsaved changes.
        /// Caller must close DB session first.
        /// </summary>
        public bool RollbackToLatest(string projectFilePath)
        {
            string latestBackup = GetBackupPath(
                projectFilePath, 1);

            if (!File.Exists(latestBackup))
                return false; // No backup exists yet

            RestoreFromBackup(
                projectFilePath, latestBackup);

            return true;
        }

        private static string GetBackupPath(
            string projectFilePath, int number)
        {
            return number == 1
                ? $"{projectFilePath}.bak"
                : $"{projectFilePath}.bak{number}";
        }

        private static string GetWalPath(string dbPath)
            => $"{dbPath}-wal";

        private static string GetShmPath(string dbPath)
            => $"{dbPath}-shm";

        private static void CopyIfExists(
            string source, string destination)
        {
            if (File.Exists(source))
                File.Copy(source, destination,
                    overwrite: true);
        }

        private static void CopyOrDeleteSidecar(
            string backupSidecar,
            string targetSidecar)
        {
            if (File.Exists(backupSidecar))
            {
                File.Copy(backupSidecar,
                    targetSidecar, overwrite: true);
                return;
            }

            if (File.Exists(targetSidecar))
                File.Delete(targetSidecar);
        }

        private static void DeleteBackupWithSidecars(
            string backupPath)
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);

            string wal = GetWalPath(backupPath);
            if (File.Exists(wal))
                File.Delete(wal);

            string shm = GetShmPath(backupPath);
            if (File.Exists(shm))
                File.Delete(shm);
        }

        private static void MoveBackupWithSidecars(
            string currentBackupPath,
            string nextBackupPath)
        {
            if (File.Exists(currentBackupPath))
                File.Move(currentBackupPath,
                    nextBackupPath, overwrite: true);

            MoveIfExists(
                GetWalPath(currentBackupPath),
                GetWalPath(nextBackupPath));
            MoveIfExists(
                GetShmPath(currentBackupPath),
                GetShmPath(nextBackupPath));
        }

        private static void MoveIfExists(
            string source,
            string destination)
        {
            if (!File.Exists(source)) return;
            File.Move(source, destination,
                overwrite: true);
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
