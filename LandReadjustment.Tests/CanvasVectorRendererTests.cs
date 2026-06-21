using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
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

    [Fact]
    public void RenderSelectionDecoration_ProjectBoundaryNonGdi_UsesLayerLineTypeScale()
    {
        using Bitmap bitmap = new(200, 200);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 100.0);

        RectangleShape shape = new(new PointD(10.0, 10.0), new PointD(90.0, 90.0))
        {
            IsSelected = true
        };
        CanvasLayer layer = new()
        {
            Id = 1,
            Name = "Project Boundary",
            LayerType = "ProjectBoundary",
            BorderColor = "#FF0000",
            LineWeight = 2.0,
            LineStyle = "DashDoubleDot",
            LineTypeScale = 2.5,
            FillStyle = "None"
        };
        CapturingMapRenderSurface surface = new(bitmap.Size);

        using CanvasVectorRenderer renderer = new();
        renderer.RenderSelectionDecoration(
            surface,
            graphics,
            engine,
            shape,
            layer,
            feature: null,
            antiAliasingEnabled: true);

        StrokeStyle selectionStroke = Assert.Single(surface.PathStrokes, IsCenterSelectionStroke);
        Assert.Equal(DashPatternKind.DashDoubleDot, selectionStroke.DashPattern);
        Assert.Equal(2.5f, selectionStroke.DashScale);
    }

    [Fact]
    public void RenderSelectionDecoration_RoadCenterlineNonGdi_UsesLayerLineTypeScale()
    {
        using Bitmap bitmap = new(200, 200);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 100.0);

        LineShape shape = new(new PointD(10.0, 50.0), new PointD(90.0, 50.0))
        {
            IsSelected = true
        };
        CanvasLayer layer = new()
        {
            Id = 2,
            Name = "Road Centerline",
            LayerType = "RoadCenterline",
            BorderColor = "#C76E78",
            LineWeight = 1.4,
            LineStyle = "Centerline",
            LineTypeScale = 3.0,
            FillStyle = "None"
        };
        CapturingMapRenderSurface surface = new(bitmap.Size);

        using CanvasVectorRenderer renderer = new();
        renderer.RenderSelectionDecoration(
            surface,
            graphics,
            engine,
            shape,
            layer,
            feature: null,
            antiAliasingEnabled: true);

        StrokeStyle selectionStroke = Assert.Single(surface.PathStrokes, IsCenterSelectionStroke);
        Assert.Equal(DashPatternKind.CenterLine, selectionStroke.DashPattern);
        Assert.Equal(3.0f, selectionStroke.DashScale);
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

    private static bool IsCenterSelectionStroke(StrokeStyle stroke) =>
        stroke.Color.ToArgb() == Color.FromArgb(0, 120, 212).ToArgb();

    private sealed class CapturingMapRenderSurface(Size pixelSize) : IMapRenderSurface
    {
        public List<StrokeStyle> PathStrokes { get; } = [];

        public Size PixelSize { get; } = pixelSize;

        public void Clear(Color color)
        {
        }

        public IDisposable SaveState() => NoopDisposable.Instance;

        public void SetQuality(RenderQuality quality)
        {
        }

        public void ClipPath(IMapPath path)
        {
        }

        public IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding) =>
            throw new NotSupportedException();

        public void DrawLine(PointF a, PointF b, in StrokeStyle stroke)
        {
        }

        public void DrawPath(IMapPath path, in StrokeStyle stroke) =>
            PathStrokes.Add(stroke);

        public void FillPath(IMapPath path, in FillStyle fill)
        {
        }

        public void DrawRectangle(RectangleF rect, in StrokeStyle stroke)
        {
        }

        public void FillRectangle(RectangleF rect, in FillStyle fill)
        {
        }

        public void DrawEllipse(RectangleF rect, in StrokeStyle stroke)
        {
        }

        public void FillEllipse(RectangleF rect, in FillStyle fill)
        {
        }

        public void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke)
        {
        }

        public SizeF MeasureText(string text, in TextStyle style) =>
            new(Math.Max(1, text.Length * 8), 12);

        public void DrawText(string text, RectangleF layout, in TextStyle style)
        {
        }

        public void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style)
        {
        }

        public void DrawImage(IMapImage image, ReadOnlySpan<PointF> destPoints, RectangleF src, in ImageStyle style)
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}
