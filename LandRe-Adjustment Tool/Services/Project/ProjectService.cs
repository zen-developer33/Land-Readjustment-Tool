using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Entities.Replotting;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Project;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Manages project lifecycle: creating and opening projects.
    ///
    /// IMPORTANT — CreateNewProjectAsync uses its OWN AppDbContext
    /// (not the session context). It commits its own data (schema,
    /// seed data, initial ProjectInfo + ProjectSettings) internally.
    ///
    /// After this returns, frmMain opens the session context
    /// and calls CommitNewProjectAsync after the setup forms close.
    /// That second commit writes any user edits from the forms.
    ///
    /// BACKUP:
    /// The initial .bak is created by frmMain.CommitNewProjectAsync
    /// AFTER the session context commits. NOT here.
    /// This avoids a double-backup scenario.
    /// </summary>
    public class ProjectService
    {
        /// <summary>
        /// Creates a new .lpp project file.
        /// Applies migrations, seeds default data,
        /// creates initial ProjectInfo and ProjectSettings.
        ///
        /// Uses its OWN AppDbContext — independent of the session.
        /// Commits its own SaveChangesAsync internally.
        /// WAL checkpoint run after commit.
        ///
        /// Returns the created ProjectInfo.
        /// </summary>
        public async Task<ProjectInfo> CreateNewProjectAsync(
            string projectFilePath,
            string projectName)
        {
            using var context = new AppDbContext(projectFilePath);

            // Apply migrations — creates all tables
            await context.Database.MigrateAsync();

            // Seed default lookup data
            await SeedPlotTypesAsync(context);

            // Create initial ProjectInfo
            var projectInfo = new ProjectInfo
            {
                ProjectName = projectName
            };

            // Create default ProjectSettings
            var settings = new ProjectSettings
            {
                // Area
                TraditionalAreaUnit = "RAPD",

                // Coordinate system — null until user sets it
                CoordinateSystemId = null,

                // Canvas
                CanvasBackgroundColor = "#1E2933",
                CanvasGridColor = "#2A3A47",
                CanvasGridVisible = true,
                SnapEnabled = true,
                SnapTolerancePx = 8.0,

                // Parcel numbering
                ParcelNumberFormat = "Sequential",
                ParcelNumberPrefix = null,
                ParcelNumberPadding = 3,

                // Replotting — Nepal government standard
                MinPlotAreaSqm = 79.49,

                // Document
                DocumentLanguage = "English",
                DateFormat = "AD",

                // Print
                DefaultPaperSize = "A3",
                DefaultPrintScale = 500,

                // IsConfigured = false — triggers settings
                // prompt on first open
                IsConfigured = false
            };

            context.ProjectInfo.Add(projectInfo);
            context.ProjectSettings.Add(settings);

            // Commit schema + seed + initial records
            await context.SaveChangesAsync();

            // WAL checkpoint — flush to main .lpp file
            // Required before session context opens the file
            await context.Database
                .ExecuteSqlRawAsync(
                    "PRAGMA wal_checkpoint(FULL);");

            // NOTE: Backup is created by frmMain.CommitNewProjectAsync
            // after user fills in project details and settings.
            // Do NOT create backup here — that would create a backup
            // of the empty project before the user has entered anything.

            return projectInfo;
        }

        /// <summary>
        /// Opens existing project and applies pending migrations.
        /// Returns ProjectInfo or null if file is invalid.
        /// </summary>
        public async Task<ProjectInfo?> OpenProjectAsync(
            string projectFilePath)
        {
            using var context = new AppDbContext(projectFilePath);
            await context.Database.MigrateAsync();
            return await context.ProjectInfo.FirstOrDefaultAsync();
        }

        // ── SEED DATA ────────────────────────────────

        /// <summary>
        /// Seeds default PlotType master data.
        /// Safe to call multiple times — skips if already seeded.
        /// </summary>
        private async Task SeedPlotTypesAsync(AppDbContext context)
        {
            if (await context.PlotTypes.AnyAsync()) return;

            var defaults = new List<PlotType>
            {
                new() { TypeName = "Private",
                    TypeCode = "PRV", IsSystemDefault = true,
                    DisplayOrder = 1,
                    Description = "Private ownership plot" },

                new() { TypeName = "Sales Plot",
                    TypeCode = "SAL", IsSystemDefault = true,
                    DisplayOrder = 2,
                    Description = "Plot for sale to recover project costs" },

                new() { TypeName = "Open Space",
                    TypeCode = "OPS", IsSystemDefault = true,
                    DisplayOrder = 4,
                    Description = "Parks and public open spaces" },

                new() { TypeName = "Community Space",
                    TypeCode = "COM", IsSystemDefault = true,
                    DisplayOrder = 5,
                    Description = "Community use plot" },

                new() { TypeName = "Road",
                    TypeCode = "ROD", IsSystemDefault = true,
                    DisplayOrder = 6,
                    Description = "Road right of way" }
            };

            context.PlotTypes.AddRange(defaults);
            await context.SaveChangesAsync();
        }
    }
}