using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Utilities;
using ProjNet.CoordinateSystems;
using System.Diagnostics.Metrics;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Manages project lifecycle:
    /// - Creating new projects
    /// - Opening existing projects
    /// - Seeding default data
    /// 
    /// This is the ONLY place AppDbContext
    /// is created for project operations.
    /// </summary>
    public class ProjectService
    {
        /// <summary>
        /// Creates a new .lpp project file at the given path.
        /// Applies migrations to create all tables.
        /// Seeds default PlotType data.
        /// Creates initial ProjectInfo record.
        /// </summary>
        public async Task<Entities.ProjectInfo> CreateNewProjectAsync(
    string projectFilePath,
    string projectName)
        {
            // Initialize shared context for this project
            CurrentProjectContext.Initialize(projectFilePath);
            var context = CurrentProjectContext.GetContext();

            // Apply migrations — creates all tables
            await context.Database.MigrateAsync();

            // Seed default plot types
            await SeedPlotTypesAsync(context);

            // Create project info and default settings together
            var projectInfo = new Entities.ProjectInfo
            {
                ProjectName = projectName
            };

            var settings = new ProjectSettings();
            // IsConfigured = false by default
            // Settings window shown on first open

            context.ProjectInfo.Add(projectInfo);
            context.ProjectSettings.Add(settings);

            // Save both in ONE transaction
            await context.SaveChangesAsync();

            return projectInfo;
        }
        /// <summary>
        /// Opens an existing .lpp project file.
        /// Applies any pending migrations.
        /// Returns the ProjectInfo record.
        /// </summary>
        public async Task<Entities.ProjectInfo?> OpenProjectAsync(
            string projectFilePath)
        {
            using var context = new AppDbContext(projectFilePath);

            // Apply any pending migrations
            // handles version upgrades automatically
            await context.Database.MigrateAsync();

            // Load project info
            return await context.ProjectInfo
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Seeds default PlotType master data.
        /// Only inserts if table is empty.
        /// Safe to call multiple times.
        /// </summary>
        private async Task SeedPlotTypesAsync(AppDbContext context)
        {
            // Check if already seeded
            if (await context.PlotTypes.AnyAsync())
                return;

            var defaultPlotTypes = new List<PlotType>
            {
                new PlotType
                {
                    TypeName = "Private",
                    TypeCode = "PRV",
                    IsSystemDefault = true,
                    DisplayOrder = 1,
                    Description = "Private ownership plot"
                },
                new PlotType
                {
                    TypeName = "Sales Plot",
                    TypeCode = "SAL",
                    IsSystemDefault = true,
                    DisplayOrder = 2,
                    Description = "Plot for sale to recover project costs"
                },
                new PlotType
                {
                    TypeName = "Open Space",
                    TypeCode = "OPS",
                    IsSystemDefault = true,
                    DisplayOrder = 4,
                    Description = "Parks and public open spaces"
                },
                new PlotType
                {
                    TypeName = "Community Space",
                    TypeCode = "COM",
                    IsSystemDefault = true,
                    DisplayOrder = 5,
                    Description = "Community use plot"
                },
                new PlotType
                {
                    TypeName = "Road",
                    TypeCode = "ROD",
                    IsSystemDefault = true,
                    DisplayOrder = 6,
                    Description = "Road right of way"
                }
            };

            context.PlotTypes.AddRange(defaultPlotTypes);
            await context.SaveChangesAsync();
        }
    }
}