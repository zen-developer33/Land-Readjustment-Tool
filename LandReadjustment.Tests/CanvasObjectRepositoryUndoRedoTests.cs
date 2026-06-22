using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Land_Readjustment_Tool.Repositories.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class CanvasObjectRepositoryUndoRedoTests : IDisposable
{
    private readonly string _tempDir;

    public CanvasObjectRepositoryUndoRedoTests()
    {
        SQLitePCL.Batteries_V2.Init();
        _tempDir = Path.Combine(Path.GetTempPath(), $"RePlotUndoRedoTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task DeleteRangeThenAddRange_WithTrackedDeletedObject_RestoresSnapshot()
    {
        string projectPath = Path.Combine(_tempDir, "undo-redo.lpp");
        await using AppDbContext context = new(projectPath);
        await context.Database.EnsureCreatedAsync();

        CanvasLayer layer = CreateLayer();
        context.CanvasLayers.Add(layer);
        await context.SaveChangesAsync();

        Guid objectId = Guid.NewGuid();
        CanvasObject original = CreateCanvasObject(objectId, layer.Id);
        context.CanvasObjects.Add(original);
        await context.SaveChangesAsync();

        _ = await context.CanvasObjects.FirstAsync(item => item.Id == objectId);

        using ProjectSession session = new(projectPath, context, new DebugLogger());
        CanvasObjectRepository repository = new(session);
        await repository.DeleteRangeAsync([objectId]);

        CanvasObject snapshot = CreateCanvasObject(objectId, layer.Id);
        await repository.AddRangeAsync([snapshot]);

        CanvasObject? restored = await context.CanvasObjects
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == objectId);

        Assert.NotNull(restored);
        Assert.Equal("Polygon", restored!.ObjectType);
        Assert.Equal(layer.Id, restored.CanvasLayerId);
    }

    private static CanvasLayer CreateLayer()
    {
        DateTime now = DateTime.UtcNow;
        return new CanvasLayer
        {
            Name = "Polygons",
            LayerType = CanvasLayerTreeService.PolygonLayerType,
            BorderColor = "#FF0000",
            FillColor = null,
            FillTransparency = 50,
            LineWeight = 1.0,
            LineStyle = "Solid",
            FillStyle = "Solid",
            CreatedDate = now,
            LastModifiedDate = now
        };
    }

    private static CanvasObject CreateCanvasObject(Guid id, int layerId)
    {
        DateTime now = DateTime.UtcNow;
        GeometryFactory geometryFactory = new(new PrecisionModel(), 0);
        Polygon polygon = geometryFactory.CreatePolygon(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10),
                new Coordinate(0, 0)
            ]);
        polygon.SRID = 0;

        return new CanvasObject
        {
            Id = id,
            CanvasLayerId = layerId,
            ObjectType = "Polygon",
            Shape = polygon,
            IsVisible = true,
            IsLocked = false,
            CreatedDate = now,
            LastModifiedDate = now
        };
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { }
    }
}
