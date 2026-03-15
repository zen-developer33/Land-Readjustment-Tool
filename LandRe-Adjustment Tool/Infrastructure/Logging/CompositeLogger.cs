namespace Land_Readjustment_Tool.Infrastructure.Logging
{
    /// <summary>
    /// Combines multiple loggers into one.
    /// Writes every log message to ALL loggers
    /// simultaneously.
    /// 
    /// WHY THIS EXISTS:
    /// During development you want logs in both
    /// the console (instant feedback) AND the file
    /// (permanent record).
    /// 
    /// Instead of calling two loggers separately
    /// everywhere — use CompositeLogger once and it
    /// handles both automatically.
    /// 
    /// USAGE EXAMPLE:
    /// var logger = new CompositeLogger(
    ///     new FileLogger(projectFolder),
    ///     new ConsoleLogger());
    /// 
    /// logger.LogInfo("Saved.");
    /// ← writes to file AND console simultaneously
    /// 
    /// EXTENSIBLE:
    /// Add as many loggers as you want.
    /// Tomorrow add DatabaseLogger or CloudLogger
    /// without changing any existing code.
    /// 
    /// FAIL SAFE:
    /// If one logger fails — others continue.
    /// A console failure never stops file logging.
    /// A file failure never stops console logging.
    /// </summary>
    public class CompositeLogger : IAppLogger
    {
        // ── FIELDS ───────────────────────────────────

        /// <summary>
        /// The collection of loggers to write to.
        /// Readonly — set once in constructor,
        /// never changed after that.
        /// </summary>
        private readonly IReadOnlyList<IAppLogger> _loggers;

        // ── CONSTRUCTOR ──────────────────────────────

        /// <summary>
        /// Creates a CompositeLogger from any number
        /// of IAppLogger implementations.
        /// 
        /// Pass loggers as params — any number:
        /// new CompositeLogger(fileLogger)
        /// new CompositeLogger(fileLogger, consoleLogger)
        /// new CompositeLogger(fileLogger, consoleLogger,
        ///     databaseLogger)
        /// </summary>
        /// <param name="loggers">
        /// One or more IAppLogger implementations.
        /// All receive every log message.
        /// </param>
        public CompositeLogger(
            params IAppLogger[] loggers)
        {
            _loggers = loggers.ToList().AsReadOnly();
        }

        // ── PUBLIC METHODS ───────────────────────────

        /// <summary>
        /// Writes INFO message to all loggers.
        /// </summary>
        public void LogInfo(string message)
        {
            foreach (var logger in _loggers)
                TryLog(() => logger.LogInfo(message));
        }

        /// <summary>
        /// Writes WARN message to all loggers.
        /// </summary>
        public void LogWarning(string message)
        {
            foreach (var logger in _loggers)
                TryLog(() => logger.LogWarning(message));
        }

        /// <summary>
        /// Writes ERROR message to all loggers.
        /// </summary>
        public void LogError(string message,
            Exception? exception = null)
        {
            foreach (var logger in _loggers)
                TryLog(() =>
                    logger.LogError(message, exception));
        }

        // ── PRIVATE METHODS ──────────────────────────

        /// <summary>
        /// Calls the given log action safely.
        /// If one logger throws — it is silently ignored
        /// so other loggers still receive the message.
        /// 
        /// Example:
        /// FileLogger fails (disk full)
        /// → ConsoleLogger still works
        /// → Application continues normally
        /// </summary>
        private static void TryLog(Action logAction)
        {
            try
            {
                logAction();
            }
            catch
            {
                // One logger failing must never stop other loggers from working and must never crash the application
            }
        }
    }
}