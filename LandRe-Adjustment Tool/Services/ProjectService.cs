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
    /// HOW SEEDING WORKS:
    /// Default CRS (MUTM81/82/83, UTM44N/45N) and
    /// DatumTransformations (Nagarkot, Kalianpur, SurveyDept)
    /// are seeded by EF Core MIGRATIONS — not here.
    /// MigrateAsync() applies them automatically on every
    /// new project creation.
    ///
    /// PlotTypes are seeded here because they are not in
    /// migrations (runtime data, not schema data).
    ///
    /// NEW PROJECT FLOW:
    ///   1. CreateNewProjectAsync — own DbContext, own commit
    ///      → applies migrations (creates tables + seeds CRS/Datum)
    ///      → seeds PlotTypes
    ///      → creates ProjectInfo + ProjectSettings
    ///      → commits and checkpoints
    ///
    ///   2. frmMain opens session context
    ///      → OpenProjectDetails (user fills project info)
    ///      → PromptProjectSettingsAsync (user configures or skips)
    ///
    ///   3. CommitNewProjectAsync — session context commit
    ///      → commits all staged changes from step 2
    ///      → creates initial .bak
    /// </summary>
    public class ProjectService
    {
        /// <summary>
        /// Creates a new .lpp project file.
        ///
        /// Uses its OWN AppDbContext — completely independent
        /// of the session context that frmMain opens later.
        /// Commits its own data internally.
        ///
        /// After this returns:
        ///   → All tables exist (migrations applied)
        ///   → Default CRS and Datum data is seeded (migrations)
        ///   → PlotTypes seeded
        ///   → ProjectInfo record created with project name
        ///   → ProjectSettings record created with defaults
        ///   → IsConfigured = false (triggers settings prompt)
        ///   → WAL checkpointed (safe for session to open file)
        ///
        /// Returns created ProjectInfo.
        /// </summary>
        public async Task<ProjectInfo> CreateNewProjectAsync(
            string projectFilePath,
            string projectName)
        {
            using var context = new AppDbContext(projectFilePath);

            // Step 1 — Apply migrations
            // Creates ALL tables and seeds:
            //   tblCoordinateSystems (MUTM81/82/83, UTM44N/45N)
            //   tblDatumTransformations (Nagarkot, Kalianpur, SurveyDept)
            //   tblPlotTypes (via migration seed if present)
            await context.Database.MigrateAsync();

            // Step 2 — Seed PlotTypes if not already seeded
            // (PlotTypes may not be in migrations on older versions)
            await SeedPlotTypesAsync(context);

            // Step 3 — Create initial ProjectInfo
            var projectInfo = new ProjectInfo
            {
                ProjectName = projectName
                // All other fields null — user fills via
                // frm_ProjectDetails after project opens
            };

            // Step 4 — Create default ProjectSettings
            // IsConfigured = false triggers the prompt:
            // "Would you like to configure project settings now?"
            // User can configure now or skip — either way
            // PromptProjectSettingsAsync stages IsConfigured = true
            var settings = new ProjectSettings
            {
                // Coordinate system — null until user sets it
                CoordinateSystemId = 1,
                DatumTransformationId = null,

                // Area
                TraditionalAreaUnit = "RAPD",

                // Canvas defaults
                CanvasBackgroundColor = "#FFFFFF",
                CanvasGridColor = "#CCCCCC",
                CanvasGridVisible = false,
                CanvasAxisMarkerVisible = false,
                SnapEnabled = true,
                SnapTolerancePx = 8.0,

                // Parcel numbering
                ParcelNumberFormat = "Sequential",
                ParcelNumberPrefix = null,
                ParcelNumberPadding = 3,

                // Replotting — Nepal government standard minimum
                MinPlotAreaSqm = 79.49,

                // Document
                DocumentLanguage = "English",
                DateFormat = "AD",

                // Print
                DefaultPaperSize = "A3",
                DefaultPrintScale = 500,

                // false = settings prompt shown on first open
                // true  = user has configured or dismissed
                IsConfigured = false
            };

            context.ProjectInfo.Add(projectInfo);
            context.ProjectSettings.Add(settings);

            // Step 5 — Commit ProjectInfo + ProjectSettings
            await context.SaveChangesAsync();

            // Step 6 — WAL checkpoint
            // Flush WAL into main .lpp file so the session
            // context that frmMain opens can read the data
            await ProjectWalCheckpoint.ExecuteAsync(projectFilePath);

            // NOTE:
            // Initial .bak is created by frmMain.CommitNewProjectAsync
            // AFTER user fills in project details and settings.
            // That is the correct first saved state to back up —
            // not this empty skeleton.

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

        // ── SEED ─────────────────────────────────────

        /// <summary>
        /// Seeds default PlotType master data.
        /// Safe to call multiple times — skips if already seeded.
        /// </summary>
        private static async Task SeedPlotTypesAsync(
            AppDbContext context)
        {
            if (await context.PlotTypes.AnyAsync()) return;

            context.PlotTypes.AddRange(
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
            );

            await context.SaveChangesAsync();
        }
    }
}
