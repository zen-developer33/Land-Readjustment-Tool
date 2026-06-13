using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Project;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LandReadjustment.Tests;

public class ProjectServiceTests : IDisposable
{
    private readonly string _tempDir;

    public ProjectServiceTests()
    {
        SQLitePCL.Batteries_V2.Init();
        _tempDir = Path.Combine(Path.GetTempPath(), $"RePlotTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task CreateNewProject_ProducesValidLppFile()
    {
        var lppPath = Path.Combine(_tempDir, "test.lpp");
        var service = new ProjectService();

        var projectInfo = await service.CreateNewProjectAsync(lppPath, "Smoke Test Project");

        Assert.True(File.Exists(lppPath), ".lpp file should exist on disk");
        Assert.Equal("Smoke Test Project", projectInfo.ProjectName);

        using var ctx = new AppDbContext(lppPath);
        var info = await ctx.ProjectInfo.FirstOrDefaultAsync();
        var settings = await ctx.ProjectSettings.FirstOrDefaultAsync();
        var crsList = await ctx.CoordinateSystems.ToListAsync();

        Assert.NotNull(info);
        Assert.NotNull(settings);
        Assert.False(settings!.IsConfigured, "New project should start unconfigured");
        Assert.True(crsList.Count > 0, "CRS seed data should be present");
        Assert.True(await TableExistsAsync(lppPath, "tblPolicySets"));
        Assert.True(await TableExistsAsync(lppPath, "tblPolicyClauses"));
        Assert.True(await TableExistsAsync(lppPath, "tblPolicyParameters"));
        Assert.True(await TableExistsAsync(lppPath, "tblPolicyLookupTables"));
        Assert.True(await TableExistsAsync(lppPath, "tblPolicyAttachments"));
    }

    [Fact]
    public async Task OpenProject_RepairsMissingCanvasLayerTextAlignmentColumn()
    {
        var lppPath = Path.Combine(_tempDir, "old-project.lpp");
        var projectService = new ProjectService();
        await projectService.CreateNewProjectAsync(lppPath, "Old Project");

        await using (var ctx = new AppDbContext(lppPath))
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "ALTER TABLE tblCanvasLayers DROP COLUMN TextAlignment;");
        }

        Assert.False(await ColumnExistsAsync(
            lppPath,
            "tblCanvasLayers",
            "TextAlignment"));

        var openService = new ProjectOpenService(
            new ProjectSessionFactory(),
            new ProjectScopedFactory());

        var projectContext = await openService.OpenAsync(lppPath);
        try
        {
            Assert.True(await ColumnExistsAsync(
                lppPath,
                "tblCanvasLayers",
                "TextAlignment"));

            await projectContext.Session
                .GetDbContext()
                .CanvasLayers
                .ToListAsync();
        }
        finally
        {
            projectContext.Close();
        }
    }

    private static async Task<bool> ColumnExistsAsync(
        string dbPath,
        string tableName,
        string columnName)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = dbPath,
            Pooling = false
        };

        await using SqliteConnection connection = new(builder.ToString());
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";

        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (string.Equals(
                reader.GetString(1),
                columnName,
                StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task<bool> TableExistsAsync(
        string dbPath,
        string tableName)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = dbPath,
            Pooling = false
        };

        await using SqliteConnection connection = new(builder.ToString());
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table' AND name = $tableName;
            """;
        command.Parameters.AddWithValue("$tableName", tableName);

        object? result = await command.ExecuteScalarAsync();
        return result is string existingName
            && string.Equals(existingName, tableName, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort cleanup */ }
    }
}
