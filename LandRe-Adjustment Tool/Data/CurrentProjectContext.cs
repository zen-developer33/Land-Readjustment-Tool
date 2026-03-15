using Land_Readjustment_Tool.Infrastructure.Logging;

namespace Land_Readjustment_Tool.Data
{
    /// <summary>
    /// Backwards-compatible wrapper.
    /// Uses ProjectSessionFactory to create sessions.
    /// 
    /// New code should use ProjectSession directly
    /// via constructor injection.
    /// This class exists only for transition period.
    /// </summary>
    public static class CurrentProjectContext
    {
        private static ProjectSession? _session;

        // Factory is the one place that
        // knows how to build a session
        private static readonly ProjectSessionFactory
            _factory = new ProjectSessionFactory();

        public static AppDbContext GetContext()
        {
            if (_session == null)
                throw new InvalidOperationException(
                    "No project is open.");
            return _session.GetContext();
        }

        public static IAppLogger GetLogger()
        {
            if (_session == null)
                throw new InvalidOperationException(
                    "No project is open.");
            return _session.Logger;
        }

        public static ProjectSession GetSession()
        {
            if (_session == null)
                throw new InvalidOperationException(
                    "No project is open.");
            return _session;
        }

        public static bool IsInitialized
            => _session != null;

        public static void Initialize(
            string projectFilePath)
        {
            // Dispose existing session
            _session?.Dispose();

            // Use factory to create properly
            // wired session
            _session = _factory
                .CreateSession(projectFilePath);
        }

        public static void Close()
        {
            _session?.Dispose();
            _session = null;
        }
    }
}