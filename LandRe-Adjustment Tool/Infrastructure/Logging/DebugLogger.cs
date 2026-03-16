using System.Diagnostics;

namespace Land_Readjustment_Tool.Infrastructure.Logging
{
    public class DebugLogger : IAppLogger
    {
        public DebugLogger() { }

        public void LogInfo(string message)
        {
            Write("INFO", message, null);
        }

        public void LogWarning(string message)
        {
            Write("WARN", message, null);
        }

        public void LogError(string message,
            Exception? exception = null)
        {
            Write("ERROR", message, exception);
        }

        private static void Write(
            string level,
            string message,
            Exception? exception)
        {
            // Debug.WriteLine works in both DEBUG
            // and RELEASE builds unlike Debug.WriteLine
            // which is stripped in RELEASE builds
            // Both appear in VS Output window
            var line =
                $"[{DateTime.Now:HH:mm:ss}]" + $" [{level,-5}] {message}";

            Debug.WriteLine(line);

            if (exception != null)
            {
                Debug.WriteLine(
                    $"           " +
                    $"Exception : {exception.Message}");

                if (exception.InnerException != null)
                {
                    Debug.WriteLine(
                        $"           " +
                        $"Inner     : " +
                        $"{exception.InnerException.Message}");
                }

                Debug.WriteLine(
                    $"           " +
                    $"StackTrace: {exception.StackTrace}");
            }
        }
    }
}