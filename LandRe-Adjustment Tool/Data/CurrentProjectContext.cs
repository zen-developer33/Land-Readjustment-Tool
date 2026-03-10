using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Data
{
    /// <summary>
    /// Holds the AppDbContext for the currently open project.
    /// One context per open project — shared across all repositories.
    /// 
    /// USAGE:
    /// When project opens  → CurrentProjectContext.Initialize(path)
    /// In repositories     → CurrentProjectContext.GetContext()
    /// When project closes → CurrentProjectContext.Close()
    /// </summary>
    public static class CurrentProjectContext
    {
        private static AppDbContext? _context;

        /// <summary>
        /// Returns the current AppDbContext.
        /// Throws if no project is open.
        /// </summary>
        public static AppDbContext GetContext()
        {
            if (_context == null)
                throw new InvalidOperationException(
                    "No project is open. " +
                    "Call Initialize() first.");
            return _context;
        }

        /// <summary>
        /// Creates a new AppDbContext for the given project file.
        /// Call this when user creates or opens a project.
        /// </summary>
        public static void Initialize(string projectFilePath)
        {
            // Dispose existing context if any
            _context?.Dispose();
            _context = new AppDbContext(projectFilePath);
        }

        /// <summary>
        /// Returns true if a project is currently open.
        /// </summary>
        public static bool IsInitialized => _context != null;

        /// <summary>
        /// Disposes context and clears it.
        /// Call this when user closes a project.
        /// </summary>
        public static void Close()
        {
            _context?.Dispose();
            _context = null;
        }
    }
}