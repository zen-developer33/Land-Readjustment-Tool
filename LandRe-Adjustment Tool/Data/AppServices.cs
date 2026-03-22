
namespace Land_Readjustment_Tool.Data
{
    /// <summary>
    /// Lightweight service locator for the application.
    /// Holds the current ProjectContext.
    /// Accessible from any form without constructor parameters.
    /// One context per open project.
    /// </summary>
    public static class AppServices
    {
        private static ProjectContext? _context;

        /// <summary>
        /// Returns the current ProjectContext.
        /// Throws if no project is open.
        /// Always check HasContext before calling this.
        /// </summary>
        public static ProjectContext Context
        {
            get
            {
                if (_context == null)
                    throw new InvalidOperationException("No project is open.");
                return _context;
            }
        }

        /// <summary>
        /// True if a project is currently open.
        /// Always check this before accessing Context.
        /// </summary>
        public static bool HasContext
            => _context != null;

        /// <summary>
        /// Sets the context when project opens.
        /// Closes existing context if one is open.
        /// Called by frmMain when creating or opening project.
        /// </summary>
        public static void SetContext(
            ProjectContext context)
        {
            // Close existing if open
            _context?.Close();
            _context = context;
        }

        /// <summary>
        /// Clears context when project closes.
        /// Disposes database connection.
        /// Called by frmMain when closing project.
        /// </summary>
        public static void ClearContext()
        {
            _context?.Close();
            _context = null;
        }
    }
}