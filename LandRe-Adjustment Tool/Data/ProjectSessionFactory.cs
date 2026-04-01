using Land_Readjustment_Tool.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Land_Readjustment_Tool.Data
{
    /// <summary>
    /// Factory that knows HOW to build a ProjectSession.
    /// 
    /// WHY A FACTORY:
    /// ProjectSession now receives its dependencies
    /// from outside (proper DI).
    /// Someone has to create those dependencies.
    /// That someone is this factory.
    /// 
    /// This is the ONLY place in the application
    /// where FileLogger, ConsoleLogger, AppDbContext
    /// are created with 'new'.
    /// 
    /// This is called the "Composition Root" —
    /// the one place where all dependencies are wired.
    /// 
    /// BENEFITS:
    /// → Want to switch to DatabaseLogger?
    ///   Change only this factory.
    /// → Want to test with fake logger?
    ///   Create ProjectSession directly in tests
    ///   passing a MockLogger.
    /// → Want two project windows?
    ///   Call CreateSession() twice.
    ///   Each call creates independent session.
    /// </summary>
    public class ProjectSessionFactory
    {
        /// <summary>
        /// Creates a fully wired ProjectSession
        /// for the given .lpp file.
        /// 
        /// This is the ONLY place new FileLogger,
        /// new ConsoleLogger, new AppDbContext
        /// are created in the application.
        /// </summary>
        public ProjectSession CreateSession(
            string projectFilePath)
        {
            // Derive project folder
            string projectFolder =
                Path.GetDirectoryName(projectFilePath)
                ?? throw new ArgumentException(
                    "Invalid path.",
                    nameof(projectFilePath));

            // Build logger
            // CompositeLogger writes to both
            // file and VS Output window during development
            // For production — remove DebugLogger
            var logger = new CompositeLogger(
                new FileLogger(projectFolder),
                new DebugLogger());

            // Build EF Core context
            var context = new AppDbContext(
                projectFilePath);

            // Wire everything together
            // and return the ready session
            return new ProjectSession(
                projectFilePath,
                context,
                logger);
        }
    }
}