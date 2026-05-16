using Land_Readjustment_Tool.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Repairs known schema gaps in project files created by older builds.
    /// </summary>
    public static class ProjectDatabaseCompatibility
    {
        /// <summary>
        /// Ensures columns expected by the current model exist even when an
        /// older project database has incomplete migration history.
        /// </summary>
        public static async Task EnsureAsync(
            AppDbContext context,
            CancellationToken ct = default)
        {
            await EnsureProjectSettingsColumnsAsync(context, ct);
            await EnsureCanvasLayerColumnsAsync(context, ct);
            await EnsureCanvasObjectColumnsAsync(context, ct);
        }

        private static async Task EnsureProjectSettingsColumnsAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            HashSet<string> columns = await ReadTableColumnsAsync(
                context,
                "tblProjectSettings",
                ct);

            if (columns.Count == 0)
                return;

            if (!columns.Contains("CanvasZoomBehavior"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblProjectSettings ADD COLUMN CanvasZoomBehavior TEXT NOT NULL DEFAULT 'StandardScaleSteps';",
                    ct);
            }

            if (!columns.Contains("CanvasGridMode"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblProjectSettings ADD COLUMN CanvasGridMode TEXT NOT NULL DEFAULT 'MajorOnly';",
                    ct);
            }
        }

        private static async Task EnsureCanvasLayerColumnsAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            HashSet<string> columns = await ReadTableColumnsAsync(
                context,
                "tblCanvasLayers",
                ct);

            if (columns.Count == 0)
                return;

            if (!columns.Contains("TextAlignment"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblCanvasLayers ADD COLUMN TextAlignment TEXT NOT NULL DEFAULT 'Left';",
                    ct);
            }

            if (!columns.Contains("LineTypeScale"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblCanvasLayers ADD COLUMN LineTypeScale REAL NOT NULL DEFAULT 1.0;",
                    ct);
            }

            if (!columns.Contains("HatchScale"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblCanvasLayers ADD COLUMN HatchScale REAL NOT NULL DEFAULT 1.0;",
                    ct);
            }

            if (!columns.Contains("LabelScaleWithZoom"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblCanvasLayers ADD COLUMN LabelScaleWithZoom INTEGER NOT NULL DEFAULT 1;",
                    ct);
            }

            if (!columns.Contains("ShowFillTransparency"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblCanvasLayers ADD COLUMN ShowFillTransparency INTEGER NOT NULL DEFAULT 0;",
                    ct);
            }
        }

        private static async Task EnsureCanvasObjectColumnsAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            HashSet<string> columns = await ReadTableColumnsAsync(
                context,
                "tblCanvasObjects",
                ct);

            if (columns.Count == 0)
                return;

            if (!columns.Contains("GeometryMetadataJson"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblCanvasObjects ADD COLUMN GeometryMetadataJson TEXT NULL;",
                    ct);
            }
        }

        private static async Task<HashSet<string>> ReadTableColumnsAsync(
            AppDbContext context,
            string tableName,
            CancellationToken ct)
        {
            HashSet<string> columns = new(StringComparer.OrdinalIgnoreCase);
            var connection = context.Database.GetDbConnection();
            bool shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await context.Database.OpenConnectionAsync(ct);

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info('{tableName}');";

                await using var reader = await command.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                    columns.Add(reader.GetString(1));
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            return columns;
        }
    }
}
