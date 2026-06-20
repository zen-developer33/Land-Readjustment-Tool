using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Roads;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using NetTopologySuite.Geometries;
using Xunit;

namespace LandReadjustment.Tests;

/// <summary>
/// Integration tests for road parcel rendering through the backend-neutral
/// render surface.
/// </summary>
public sealed class RoadParcelRendererBackendTests
{
    /// <summary>
    /// Verifies that a donut road parcel renders through the backend path and
    /// keeps its interior ring unfilled.
    /// </summary>
    [Fact]
    public void Draw_DonutRoadParcel_RendersVisiblePixelsAndPreservesHole()
    {
        using Bitmap bitmap = new(180, 120);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        RoadParcel road = new()
        {
            RoadName = "Ring Road",
            RoadParcelNumber = "R-1",
            RoadType = RoadParcelType.Roundabout,
            Shape = CreateDonutPolygon()
        };

        RoadParcelRenderer renderer = new();
        renderer.Draw(
            graphics,
            road,
            coordinate => new PointF((float)coordinate.X, (float)coordinate.Y),
            isSelected: false,
            showIslandOutlines: true);

        Assert.True(CountNonWhitePixels(bitmap) > 3_000);
        Assert.Equal(Color.White.ToArgb(), bitmap.GetPixel(90, 60).ToArgb());
    }

    /// <summary>
    /// Creates a simple rectangular road polygon with one rectangular island.
    /// </summary>
    private static Polygon CreateDonutPolygon()
    {
        GeometryFactory factory = GeometryFactory.Default;
        LinearRing shell = factory.CreateLinearRing(
        [
            new Coordinate(20, 20),
            new Coordinate(160, 20),
            new Coordinate(160, 100),
            new Coordinate(20, 100),
            new Coordinate(20, 20)
        ]);
        LinearRing hole = factory.CreateLinearRing(
        [
            new Coordinate(70, 45),
            new Coordinate(110, 45),
            new Coordinate(110, 75),
            new Coordinate(70, 75),
            new Coordinate(70, 45)
        ]);

        return factory.CreatePolygon(shell, [hole]);
    }

    /// <summary>
    /// Counts pixels that differ from the white test background.
    /// </summary>
    private static int CountNonWhitePixels(Bitmap bitmap)
    {
        int white = Color.White.ToArgb();
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != white)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
