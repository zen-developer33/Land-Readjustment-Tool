using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class ViewportClipRenderingTests
{
    [Fact]
    public void PolylineScreenPath_PartiallyVisibleLongLineAtHighZoom_RendersClippedSegment()
    {
        MapCanvasEngine engine = new(new Size(1000, 800));
        engine.SetViewport(new PointD(0.0, 0.0), MapCanvasEngine.MaxZoom);
        RectangleD clip = engine.GetClipWorldBounds(64.0 / engine.ZoomScale);
        PolylineShape polyline = new(
        [
            new PointD(-1000.0, 0.0),
            new PointD(1000.0, 0.0)
        ]);

        using GraphicsPath path = polyline.CreateScreenPath(engine.WorldToScreen, clip);

        Assert.True(path.PointCount > 0);
        RectangleF bounds = path.GetBounds();
        Assert.True(float.IsFinite(bounds.Left));
        Assert.True(float.IsFinite(bounds.Right));
        Assert.InRange(bounds.Left, -65.0f, 1001.0f);
        Assert.InRange(bounds.Right, -1.0f, 1065.0f);
    }

    [Fact]
    public void PolylineScreenPath_ClosedSegmentedPolygonWithClip_RemainsFillable()
    {
        MapCanvasEngine engine = new(new Size(120, 120));
        engine.SetView(50.0, 50.0, 100.0, 100.0);
        RectangleD clip = engine.GetClipWorldBounds(64.0 / engine.ZoomScale);

        PointD a = new(20.0, 20.0);
        PointD b = new(80.0, 20.0);
        PointD c = new(80.0, 80.0);
        PointD d = new(20.0, 80.0);
        PolylineShape polyline = new(
            [a, b, c, d, a],
            [
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, a, b),
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, b, c),
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, c, d),
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, d, a)
            ],
            isClosed: true);

        using GraphicsPath path = polyline.CreateScreenPath(engine.WorldToScreen, clip);
        using Bitmap bitmap = new(120, 120);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.White);

        graphics.FillPath(Brushes.Orange, path);

        Assert.True(CountPixels(bitmap, Color.Orange) > 2500);
    }

    [Fact]
    public void Render_HighZoomWithDistantOriginAxisOverlay_DoesNotOverflow()
    {
        MapCanvasEngine engine = new(new Size(1260, 852));
        engine.SetViewport(
            new PointD(525203.8507, 3097615.2834),
            MapCanvasEngine.MaxZoom);

        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowGridLabels = false;
        settings.ShowAxisLines = true;
        settings.ShowOriginMarker = true;

        using MapCanvasRenderer renderer = new(engine, settings);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);

        Exception? exception = Record.Exception(
            () => renderer.Render(graphics, null, false, null, false, null));

        Assert.Null(exception);
    }

    private static int CountPixels(Bitmap bitmap, Color color)
    {
        int expected = color.ToArgb();
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() == expected)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
