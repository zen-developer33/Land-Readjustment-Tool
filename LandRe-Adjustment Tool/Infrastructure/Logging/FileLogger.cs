namespace Land_Readjustment_Tool.Infrastructure.Logging
{
    /// <summary>
    /// Writes log messages to a text file in the
    /// project's Logs/ folder.
    /// 
    /// One log file is created per day:
    /// Logs/log_2026-03-15.txt
    /// Logs/log_2026-03-16.txt
    /// 
    /// This class implements IAppLogger.
    /// All other classes depend on IAppLogger interface
    /// not on this class directly.
    /// 
    /// THREAD SAFETY:
    /// Uses a lock object to prevent two threads from
    /// writing to the file at the same time which would
    /// cause file corruption.
    /// 
    /// FAIL SAFE:
    /// If writing to the log file fails for any reason
    /// (disk full, permissions, etc.) the application
    /// continues normally. Logging must never crash
    /// the application.
    /// </summary>
    public class FileLogger : IAppLogger
    {
        // ── FIELDS ──────────────────────────────────

        /// <summary>
        /// Full path to the Logs/ folder inside
        /// the project folder.
        /// Example: C:\Projects\Ward5\Logs\
        /// </summary>
        private readonly string _logFolderPath;

        /// <summary>
        /// Full path to today's log file.
        /// Example: C:\Projects\Ward5\Logs\log_2026-03-15.txt
        /// New file is created each day automatically.
        /// </summary>
        private readonly string _logFilePath;

        /// <summary>
        /// Lock object for thread safety.
        /// static = shared across all FileLogger instances.
        /// Prevents two threads writing simultaneously
        /// which would corrupt the log file.
        /// </summary>
        private static readonly object _lock = new();

        // ── CONSTRUCTOR ──────────────────────────────

        /// <summary>
        /// Creates a FileLogger for the given project folder.
        /// Automatically creates the Logs/ subfolder if it
        /// does not exist yet.
        /// </summary>
        /// <param name="projectFolderPath">
        /// The root folder of the open project.
        /// Example: C:\Projects\Kathmandu_Ward5\
        /// The Logs/ folder will be created inside this.
        /// </param>
        public FileLogger(string projectFolderPath)
        {
            // Build path to Logs/ folder
            _logFolderPath = Path.Combine(
                projectFolderPath, "Logs");

            // Create Logs/ folder if it does not exist
            // Safe to call even if folder already exists
            Directory.CreateDirectory(_logFolderPath);

            // Build log file name using today's date
            // New file created automatically each day
            string fileName =
                $"log_{DateTime.Now:yyyy-MM-dd}.txt";

            _logFilePath = Path.Combine(
                _logFolderPath, fileName);
        }

        // ── PUBLIC METHODS ───────────────────────────

        /// <summary>
        /// Writes an INFO level message to the log file.
        /// Use for normal successful operations.
        /// </summary>
        public void LogInfo(string message)
        {
            Write("INFO", message, null);
        }

        /// <summary>
        /// Writes a WARN level message to the log file.
        /// Use for unexpected but non-fatal situations.
        /// </summary>
        public void LogWarning(string message)
        {
            Write("WARN", message, null);
        }

        /// <summary>
        /// Writes an ERROR level message to the log file.
        /// Always pass the exception if available.
        /// Exception details are written to the log
        /// to help with debugging.
        /// </summary>
        public void LogError(string message,
            Exception? exception = null)
        {
            Write("ERROR", message, exception);
        }

        // ── PRIVATE METHODS ──────────────────────────

        /// <summary>
        /// Core method that formats and writes a log entry.
        /// 
        /// Log entry format:
        /// [2026-03-15 14:23:05] [INFO] Import started.
        /// [2026-03-15 14:23:06] [ERROR] Failed to save.
        ///   Exception: Object reference not set...
        ///   StackTrace: at LandOwnerService.SaveAsync()...
        /// 
        /// The lock(_lock) block ensures only one thread
        /// can write at a time — thread safe.
        /// 
        /// The outer try/catch ensures the application
        /// never crashes because of a logging failure.
        /// </summary>
        private void Write(
            string level,
            string message,
            Exception? exception)
        {
            try
            {
                // Build the log line
                // Format: [date time] [LEVEL] message
                var line =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]" +
                    $" [{level}] {message}";

                // If an exception was passed
                // append its details on new lines
                if (exception != null)
                {
                    line +=
                        Environment.NewLine +
                        $"  Exception: {exception.Message}" +
                        Environment.NewLine +
                        $"  StackTrace: {exception.StackTrace}";
                }

                // lock = only one thread writes at a time
                // prevents file corruption from
                // simultaneous writes
                lock (_lock)
                {
                    // AppendAllText adds to existing file
                    // or creates new file if not exists
                    File.AppendAllText(
                        _logFilePath,
                        line + Environment.NewLine);
                }
            }
            catch
            {
                // If writing to file fails for any reason:
                // disk full, permissions denied, locked etc.
                // → silently ignore
                // → application continues normally
                // Logging must NEVER crash the application
            }
        }
    }
}