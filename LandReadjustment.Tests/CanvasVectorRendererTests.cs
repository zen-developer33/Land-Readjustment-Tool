using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class CanvasVectorRendererTests
{
    [Fact]
    public void RenderPreview_DrawsHorizontalPolylinePath()
    {
        using Bitmap bitmap = new(120, 80);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Black);

        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 80.0);

        using CanvasVectorRenderer renderer = new();
        renderer.RenderPreview(
            graphics,
            engine,
            new PolylineShape(
                [new PointD(10.0, 50.0), new PointD(90.0, 50.0)],
                [new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    new PointD(10.0, 50.0),
                    new PointD(90.0, 50.0))],
                isClosed: false),
            previewLayer: null);

        Assert.True(CountNonBlackPixels(bitmap) > 0);
    }

    [Fact]
    public void RenderPreview_DrawsVerticalPolygonRubberBandPath()
    {
        using Bitmap bitmap = new(120, 80);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Black);

        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 80.0);

        using CanvasVectorRenderer renderer = new();
        renderer.RenderPreview(
            graphics,
            engine,
            new PolylineShape(
                [new PointD(50.0, 10.0), new PointD(50.0, 70.0)],
                [new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    new PointD(50.0, 10.0),
                    new PointD(50.0, 70.0))],
                isClosed: true),
            previewLayer: null);

        Assert.True(CountNonBlackPixels(bitmap) > 0);
    }

    private static int CountNonBlackPixels(Bitmap bitmap)
    {
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != Color.Black.ToArgb())
                {
                    count++;
                }
            }
        }

        return count;
    }
}
