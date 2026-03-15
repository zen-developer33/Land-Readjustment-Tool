using System;
using System.Collections.Generic;
using System.Text;

namespace Land_Readjustment_Tool.Infrastructure.Logging
{
    /// <summary>
    /// Interface for application logging.
    /// 
    /// WHY AN INTERFACE:
    /// Any class that needs logging depends on this
    /// interface — not on FileLogger directly.
    /// This means tomorrow if you want to write logs
    /// to a database or a cloud service instead of
    /// a file — you just create a new class that
    /// implements this interface. Nothing else changes.
    /// 
    /// USAGE:
    /// Never use MessageBox.Show() in services or
    /// repositories for errors. Always use IAppLogger.
    /// Never use Debug.WriteLine() for errors.
    /// Always use IAppLogger.
    /// 
    /// MessageBox.Show() is only for UI forms.
    /// IAppLogger is for everything else.
    /// </summary>
    public interface IAppLogger  // It is a common interface for logging in the application, allowing for different implementations (e.g., console, file, database)
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? exeption = null);
    }
}
