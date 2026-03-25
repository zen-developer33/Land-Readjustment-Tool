namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Manages backup files for .lpp project files.
    ///
    /// PURPOSE:
    /// .bak files allow the user to restore the project
    /// to any of the last 5 SAVED states.
    /// They also protect against .lpp file corruption.
    ///
    /// WHEN BACKUPS ARE CREATED:
    ///   1. New project created → initial .bak
    ///   2. User presses Ctrl+S → backup rotated
    ///   3. User clicks Backup Project menu
    ///   4. Save As → fresh .bak in new location
    ///
    /// WHEN BACKUPS ARE NOT CREATED:
    ///   → Form edits (these are staged only)
    ///   → Auto-save
    ///   → Close without saving
    ///      (EF Core ChangeTracker.Clear handles this)
    ///
    /// BACKUP NAMING:
    ///   MyProject.lpp.bak   ← most recent save
    ///   MyProject.lpp.bak2  ← one save before
    ///   MyProject.lpp.bak3
    ///   MyProject.lpp.bak4
    ///   MyProject.lpp.bak5  ← oldest saved state
    ///
    /// ROTATION ON SAVE:
    ///   .bak4 → .bak5  (oldest dropped if exists)
    ///   .bak3 → .bak4
    ///   .bak2 → .bak3
    ///   .bak  → .bak2
    ///   current .lpp → .bak  (state before this save)
    /// </summary>
    public class ProjectBackupService
    {
        private const int MaxBackups = 5;

        // ── CREATE BACKUP ────────────────────────────

        /// <summary>
        /// Creates a backup of the current .lpp file.
        /// Rotates existing backups before creating new one.
        ///
        /// Call this AFTER WAL checkpoint and
        /// BEFORE SaveChangesAsync in SaveCurrentProjectAsync.
        /// This means .bak = state just before this save.
        ///
        /// Also call on:
        ///   → new project created
        ///   → save as (in new location)
        ///   → manual backup menu
        /// </summary>
        public void CreateBackup(string projectFilePath)
        {
            // Nothing to back up if .lpp does not exist yet
            if (!File.Exists(projectFilePath)) return;

            RotateBackups(projectFilePath);
        }

        // ── GET BACKUPS ──────────────────────────────

        /// <summary>
        /// Returns all existing backup files for this project.
        /// Ordered from most recent (1) to oldest (5).
        /// Used by frmBackupManager to show restore options.
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

        // ── RESTORE ──────────────────────────────────

        /// <summary>
        /// Restores the project by copying a backup
        /// file over the main .lpp file.
        ///
        /// CALLER MUST:
        ///   1. Call ChangeTracker.Clear() first
        ///   2. Dispose the EF Core session
        ///   3. Call AppServices.ClearContext()
        ///   4. THEN call this method
        ///   5. Reopen project from restored .lpp
        ///
        /// This order is required because SQLite
        /// keeps a file lock on the .lpp while
        /// a session is open.
        /// </summary>
        public void RestoreFromBackup(
            string projectFilePath,
            string backupFilePath)
        {
            if (!File.Exists(backupFilePath))
                throw new FileNotFoundException(
                    "Backup file not found.",
                    backupFilePath);

            // Overwrite .lpp with selected backup
            File.Copy(backupFilePath,
                projectFilePath, overwrite: true);
        }

        // ── PRIVATE ──────────────────────────────────

        /// <summary>
        /// Rotates existing backup files and creates
        /// a new .bak from current .lpp.
        ///
        /// Before rotation:
        ///   .bak  = save N-1
        ///   .bak2 = save N-2
        ///   ...
        ///
        /// After rotation:
        ///   .bak  = save N (just created)
        ///   .bak2 = save N-1
        ///   ...
        ///   .bak5 = save N-4 (oldest kept)
        /// </summary>
        private void RotateBackups(string projectFilePath)
        {
            // Step 1 — delete oldest backup to make room
            string oldest = GetBackupPath(
                projectFilePath, MaxBackups);
            if (File.Exists(oldest))
                File.Delete(oldest);

            // Step 2 — shift all existing backups up by one
            // .bak4 → .bak5, .bak3 → .bak4, etc.
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

            // Step 3 — copy current .lpp → .bak
            // This captures the state just before
            // the upcoming SaveChangesAsync commit
            string latest = GetBackupPath(
                projectFilePath, 1);
            File.Copy(projectFilePath,
                latest, overwrite: true);
        }

        /// <summary>
        /// Builds the backup file path for a given number.
        /// Number 1 = .bak (most recent)
        /// Number 2-5 = .bak2 to .bak5
        /// </summary>
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
    /// Used by frmBackupManager to display the list.
    /// </summary>
    public class BackupEntry
    {
        /// <summary>1 = most recent, 5 = oldest.</summary>
        public int Number { get; set; }

        /// <summary>Full path to the .bak file.</summary>
        public string FilePath { get; set; } = "";

        /// <summary>When this backup was created.</summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>File size in bytes.</summary>
        public long FileSizeBytes { get; set; }

        /// <summary>Human-readable file size.</summary>
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

        /// <summary>
        /// Display label shown in frmBackupManager.
        /// </summary>
        public string Label => Number == 1
            ? "Latest Backup"
            : $"Backup {Number}";
    }
}