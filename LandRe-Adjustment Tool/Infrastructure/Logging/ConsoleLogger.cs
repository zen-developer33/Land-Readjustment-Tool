using System.Drawing;

namespace Land_Readjustment_Tool.Infrastructure.Logging
{
    /// <summary>
    /// Writes log messages to the system console
    /// with color coding by log level.
    /// 
    /// WHY THIS EXISTS:
    /// During development you want to see log messages
    /// instantly in the Output window or console
    /// without opening a log file.
    /// 
    /// COLOR CODING:
    /// INFO    → Gray    (normal, not important)
    /// WARN    → Yellow  (needs attention)
    /// ERROR   → Red     (something failed)
    /// 
    /// WHEN TO USE:
    /// Development and debugging only.
    /// In production use FileLogger or
    /// CompositeLogger (both together).
    /// 
    /// IMPLEMENTS IAppLogger:
    /// Same interface as FileLogger.
    /// Swap between them without changing
    /// any other code.
    /// </summary>
    public class ConsoleLogger : IAppLogger
    {
        // ── CONSTRUCTOR ──────────────────────────────

        /// <summary>
        /// Creates a ConsoleLogger.
        /// No parameters needed — always writes
        /// to system console output.
        /// </summary>
        public ConsoleLogger() { }

        // ── PUBLIC METHODS ───────────────────────────

        /// <summary>
        /// Writes an INFO message to console in Gray.
        /// Use for normal successful operations.
        /// </summary>
        public void LogInfo(string message)
        {
            Write("INFO", message, null,
                ConsoleColor.Gray);
        }

        /// <summary>
        /// Writes a WARN message to console in Yellow.
        /// Use for unexpected but non-fatal situations.
        /// </summary>
        public void LogWarning(string message)
        {
            Write("WARN", message, null,
                ConsoleColor.Yellow);
        }

        /// <summary>
        /// Writes an ERROR message to console in Red.
        /// Pass the exception if available — its message
        /// will be shown in the console output.
        /// </summary>
        public void LogError(string message,
            Exception? exception = null)
        {
            Write("ERROR", message, exception,
                ConsoleColor.Red);
        }

        // ── PRIVATE METHODS ──────────────────────────

        /// <summary>
        /// Core method that formats and writes
        /// a colored log entry to the console.
        /// 
        /// Output format:
        /// [14:23:05] [INFO]  Import started.
        /// [14:23:06] [WARN]  CitizenshipNo missing.
        /// [14:23:07] [ERROR] Failed to save.
        ///            Exception: Object ref not set...
        /// 
        /// Console color is changed for the level tag
        /// only — the rest of the message stays in the
        /// default console color for readability.
        /// 
        /// Console.ResetColor() is always called in
        /// the finally block to ensure the console
        /// color is never left in a changed state
        /// even if an exception occurs during writing.
        /// </summary>
        private void Write(
            string level,
            string message,
            Exception? exception,
            ConsoleColor color)
        {
            try
            {
                // Write timestamp in default color
                Console.Write(
                    $"[{DateTime.Now:HH:mm:ss}] ");

                // Write level tag in color
                Console.ForegroundColor = color;
                Console.Write($"[{level}]");

                // Reset color for message text
                Console.ResetColor();

                // Write the message
                Console.WriteLine($" {message}");

                // Write exception details if provided
                // Indented for visual clarity
                if (exception != null)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine(
                        $"           " +
                        $"Exception: {exception.Message}");
                    Console.ResetColor();
                }
            }
            finally
            {
                // ALWAYS reset color
                // Even if something went wrong above
                // Prevents console being stuck in red
                Console.ResetColor();
            }
        }
    }
}