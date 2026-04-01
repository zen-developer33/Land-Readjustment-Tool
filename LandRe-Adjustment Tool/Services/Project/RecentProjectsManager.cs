namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Manages the recent projects list stored in Properties.Settings.
    /// Stored as pipe-separated paths: "C:\a.lpp|C:\b.lpp|C:\c.lpp"
    /// 
    /// Rules:
    /// - Max 10 entries (Recent_MaxCount from settings)
    /// - Most recently opened always goes to top
    /// - Duplicate paths are not stored twice
    /// - Paths that no longer exist on disk are removed on load
    /// </summary>
    public static class RecentProjectsManager
    {
        private const char SEPARATOR = '|';

        // ── READ ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the current recent projects list.
        /// Automatically removes paths that no longer exist on disk.
        /// </summary>
        public static List<string> GetRecentProjects()
        {
            string raw = Properties.Settings.Default
                .Recent_ProjectPaths ?? string.Empty;

            return raw
                .Split(new[] { SEPARATOR },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(p => File.Exists(p))   // remove dead paths
                .Distinct()
                .ToList();
        }

        // ── WRITE ────────────────────────────────────────────────

        /// <summary>
        /// Adds a path to the top of the recent list.
        /// Removes duplicate if it already exists.
        /// Trims to max count.
        /// Saves to Settings immediately.
        /// </summary>
        public static void AddRecentProject(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            if (!File.Exists(filePath)) return;

            var list = GetRecentProjects();

            // Remove if already in list (will re-add at top)
            list.RemoveAll(p =>
                string.Equals(p, filePath,
                    StringComparison.OrdinalIgnoreCase));

            // Insert at top
            list.Insert(0, filePath);

            // Trim to max
            int max = Properties.Settings.Default.Recent_MaxCount;
            if (list.Count > max)
                list = list.Take(max).ToList();

            // Save
            Properties.Settings.Default.Recent_ProjectPaths =
                string.Join(SEPARATOR.ToString(), list);

            Properties.Settings.Default.Recent_LastProjectPath =
                filePath;

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Removes a single path from the list.
        /// Called when a file is deleted or moved.
        /// </summary>
        public static void RemoveRecentProject(string filePath)
        {
            var list = GetRecentProjects();

            list.RemoveAll(p =>
                string.Equals(p, filePath,
                    StringComparison.OrdinalIgnoreCase));

            Properties.Settings.Default.Recent_ProjectPaths =
                string.Join(SEPARATOR.ToString(), list);

            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Clears the entire recent projects list.
        /// </summary>
        public static void ClearRecentProjects()
        {
            Properties.Settings.Default.Recent_ProjectPaths
                = string.Empty;
            Properties.Settings.Default.Recent_LastProjectPath
                = string.Empty;
            Properties.Settings.Default.Save();
        }
    }
}