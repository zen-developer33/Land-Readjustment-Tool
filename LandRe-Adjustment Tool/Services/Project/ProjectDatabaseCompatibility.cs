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
            await EnsureBlockColumnsAsync(context, ct);
            await EnsureBuildingInventoryTablesAsync(context, ct);
            // Policy tables (tblPolicy*) are now ensured by
            // Land_Pooling_Policy_Manager.Data.PolicyDbContext.EnsureSchemaAsync,
            // called from PolicyManagerLauncher when the policy manager opens.
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

            if (!columns.Contains("ApplicationEditLocked"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblProjectSettings ADD COLUMN ApplicationEditLocked INTEGER NOT NULL DEFAULT 0;",
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
                    "ALTER TABLE tblCanvasLayers ADD COLUMN TextAlignment TEXT NOT NULL DEFAULT 'Center Middle';",
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

        private static async Task EnsureBlockColumnsAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            HashSet<string> columns = await ReadTableColumnsAsync(
                context,
                "tblBlocks",
                ct);

            if (columns.Count == 0)
                return;

            if (!columns.Contains("BlockLength"))
            {
                await context.Database.ExecuteSqlRawAsync(
                    "ALTER TABLE tblBlocks ADD COLUMN BlockLength REAL NOT NULL DEFAULT 0;",
                    ct);
            }
        }

        private static async Task EnsureBuildingInventoryTablesAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblBuildingInventories (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblBuildingInventories PRIMARY KEY AUTOINCREMENT,
                    CanvasObjectId TEXT NULL,
                    BuildingCode TEXT NOT NULL DEFAULT '',
                    BuildingName TEXT NULL,
                    OwnerName TEXT NULL,
                    BuildingUse TEXT NULL,
                    ConstructionType TEXT NULL,
                    StoreyCount INTEGER NULL,
                    PlinthAreaSqm REAL NULL,
                    BuildingCondition TEXT NULL,
                    Notes TEXT NULL,
                    SurveyDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    LastModifiedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT FK_tblBuildingInventories_tblCanvasObjects_CanvasObjectId
                        FOREIGN KEY (CanvasObjectId) REFERENCES tblCanvasObjects (Id) ON DELETE SET NULL
                );
                """,
                ct);

            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblBuildingPhotos (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblBuildingPhotos PRIMARY KEY AUTOINCREMENT,
                    BuildingInventoryId INTEGER NOT NULL,
                    Direction TEXT NOT NULL DEFAULT '',
                    FileName TEXT NULL,
                    ContentType TEXT NULL,
                    ImageData BLOB NOT NULL DEFAULT X'',
                    CapturedDate TEXT NULL,
                    Notes TEXT NULL,
                    CONSTRAINT FK_tblBuildingPhotos_tblBuildingInventories_BuildingInventoryId
                        FOREIGN KEY (BuildingInventoryId) REFERENCES tblBuildingInventories (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await context.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS tblBuildingOpenings (
                    Id INTEGER NOT NULL CONSTRAINT PK_tblBuildingOpenings PRIMARY KEY AUTOINCREMENT,
                    BuildingInventoryId INTEGER NOT NULL,
                    Side TEXT NOT NULL DEFAULT '',
                    OpeningType TEXT NOT NULL DEFAULT '',
                    Label TEXT NULL,
                    OffsetFromLeftM REAL NULL,
                    SillHeightM REAL NULL,
                    WidthM REAL NULL,
                    HeightM REAL NULL,
                    Notes TEXT NULL,
                    CONSTRAINT FK_tblBuildingOpenings_tblBuildingInventories_BuildingInventoryId
                        FOREIGN KEY (BuildingInventoryId) REFERENCES tblBuildingInventories (Id) ON DELETE CASCADE
                );
                """,
                ct);

            await context.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_tblBuildingInventories_BuildingCode ON tblBuildingInventories (BuildingCode) WHERE BuildingCode IS NOT NULL AND BuildingCode <> '';",
                ct);
            await context.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IF NOT EXISTS IX_tblBuildingInventories_CanvasObjectId ON tblBuildingInventories (CanvasObjectId) WHERE CanvasObjectId IS NOT NULL;",
                ct);
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IF NOT EXISTS IX_tblBuildingPhotos_BuildingInventoryId_Direction ON tblBuildingPhotos (BuildingInventoryId, Direction);",
                ct);
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IF NOT EXISTS IX_tblBuildingOpenings_BuildingInventoryId_Side_OpeningType ON tblBuildingOpenings (BuildingInventoryId, Side, OpeningType);",
                ct);
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
