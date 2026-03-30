using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Project;
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
        public async Task<ProjectInfo> CreateNewProjectAsync(
    string projectFilePath,
    string projectName)
        {
            using var context = new AppDbContext(projectFilePath);

            // Apply migrations — creates all tables
            await context.Database.MigrateAsync();

            // Seed default plot types
            await SeedPlotTypesAsync(context);

            // Create project info and default settings together
            var projectInfo = new ProjectInfo
            {
                ProjectName = projectName
            };

            var settings = new ProjectSettings
            {
                // ── AREA ────────────────────────────────
                // Default traditional unit is RAPD
                // All calculations always use Sqm
                TraditionalAreaUnit = "RAPD",

                // ── COORDINATE SYSTEM ───────────────────
                // NEW — FK replaces old string fields
                CoordinateSystemId = null,
                // null until user sets it in settings window

                // ── CANVAS ──────────────────────────────
                CanvasBackgroundColor = "#1E2933",
                CanvasGridColor = "#2A3A47",
                CanvasGridVisible = true,
                SnapEnabled = true,
                SnapTolerancePx = 8.0,

                // ── PARCEL NUMBERING ────────────────────
                ParcelNumberFormat = "Sequential",
                ParcelNumberPrefix = null,
                ParcelNumberPadding = 3,

                // ── REPLOTTING ──────────────────────────
                // Nepal government standard minimum
                MinPlotAreaSqm = 79.49,

                // ── DOCUMENT ────────────────────────────
                DocumentLanguage = "English",
                DateFormat = "AD",

                // ── PRINT ───────────────────────────────
                DefaultPaperSize = "A3",
                DefaultPrintScale = 500,

                // ── STATUS ──────────────────────────────
                // false = settings window shown on first open
                // true after user confirms settings
                IsConfigured = false
            };
            // IsConfigured = false by default
            // Settings window shown on first open

            context.ProjectInfo.Add(projectInfo);
            context.ProjectSettings.Add(settings);

            // Save both in ONE transaction
            await context.SaveChangesAsync();

            // WAL checkpoint — flush to main.lpp file
            await context.Database
                .ExecuteSqlRawAsync(
                    "PRAGMA wal_checkpoint(TRUNCATE);");

            // NOTE: Initial .bak is created in frmMain.PromptProjectSettings,
            // AFTER the user has filled in project info and settings.
            // That captures the correct "first saved state".
            return projectInfo;
        }
        /// <summary>
        /// Opens an existing .lpp project file.
        /// Applies any pending migrations.
        /// Returns the ProjectInfo record.
        /// </summary>
        public async Task<ProjectInfo?> OpenProjectAsync(
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