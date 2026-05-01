using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services;
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
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort cleanup */ }
    }
}
