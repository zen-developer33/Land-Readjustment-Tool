using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
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
    public void RenderSelectionDecoration_ProjectBoundaryNonGdi_UsesLayerLineTypeAndScale()
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
        AssertSelectionStrokeMetrics(layer, selectionStroke);
    }

    [Fact]
    public void RenderSelectionDecoration_ProjectBoundaryGdi_DoesNotRedrawLayerStrokeUnderSelection()
    {
        using Bitmap bitmap = new(200, 200);
        using Graphics graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Black);
        using GdiMapRenderSurface surface = new(graphics, bitmap.Size);
        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 100.0);

        RectangleShape shape = new(new PointD(10.0, 10.0), new PointD(90.0, 90.0))
        {
            IsSelected = true
        };
        CanvasLayer layer = new()
        {
            Id = 11,
            Name = "Project Boundary",
            LayerType = "ProjectBoundary",
            BorderColor = "#FF0000",
            LineWeight = 2.0,
            LineStyle = "DashDoubleDot",
            LineTypeScale = 2.5,
            FillStyle = "None"
        };

        using CanvasVectorRenderer renderer = new();
        renderer.RenderSelectionDecoration(
            surface,
            graphics,
            engine,
            shape,
            layer,
            feature: null,
            antiAliasingEnabled: true);

        Assert.True(CountPixels(bitmap, IsSelectionBluePixel) > 0);
        Assert.Equal(0, CountPixels(bitmap, IsProjectBoundaryRedPixel));
    }

    [Fact]
    public void RenderSelectionDecoration_RoadCenterlineNonGdi_UsesLayerLineTypeAndScale()
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
        AssertSelectionStrokeMetrics(layer, selectionStroke);
    }

    [Fact]
    public void RenderSelectionDecoration_DraftingLayerNonGdi_UsesLayerLineTypeAndScale()
    {
        using Bitmap bitmap = new(200, 200);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 100.0);

        LineShape shape = new(new PointD(10.0, 50.0), new PointD(90.0, 30.0))
        {
            IsSelected = true
        };
        CanvasLayer layer = new()
        {
            Id = 3,
            Name = "Drafting",
            LayerType = "DrawingMarkup",
            BorderColor = "#0080FF",
            LineWeight = 1.8,
            LineStyle = "Dashed",
            LineTypeScale = 1.6,
            FillStyle = "None",
            Description = "Default layer: Drawing"
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
        Assert.Equal(DashPatternKind.Dashed, selectionStroke.DashPattern);
        AssertSelectionStrokeMetrics(layer, selectionStroke);
    }

    [Fact]
    public void RenderSelectionDecoration_ExternalLayerNonGdi_UsesLayerLineTypeAndScale()
    {
        using Bitmap bitmap = new(200, 200);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 100.0);

        LineShape shape = new(new PointD(10.0, 30.0), new PointD(90.0, 70.0))
        {
            IsSelected = true
        };
        CanvasLayer layer = new()
        {
            Id = 4,
            Name = "Imported Utilities",
            LayerType = "Polyline",
            BorderColor = "#00A060",
            LineWeight = 2.4,
            LineStyle = "DashDot",
            LineTypeScale = 2.2,
            FillStyle = "None",
            Description = "Imported external layer: Utilities"
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
        Assert.Equal(DashPatternKind.DashDot, selectionStroke.DashPattern);
        AssertSelectionStrokeMetrics(layer, selectionStroke);
    }

    [Theory]
    [MemberData(nameof(SelectionOutlineShapeCases))]
    public void RenderSelectionDecoration_AllStrokeableShapes_LocksSelectionStyleRule(string shapeName, IShape shape)
    {
        using Bitmap bitmap = new(200, 200);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapCanvasEngine engine = new(bitmap.Size);
        engine.SetView(50.0, 50.0, 100.0, 100.0);

        CanvasLayer layer = new()
        {
            Id = 10,
            Name = $"Selection Rule {shapeName}",
            LayerType = "DrawingMarkup",
            BorderColor = "#404040",
            LineWeight = 1.7,
            LineStyle = "DashDoubleDot",
            LineTypeScale = 2.75,
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
        AssertSelectionStrokeMetrics(layer, selectionStroke);
    }

    public static TheoryData<string, IShape> SelectionOutlineShapeCases => new()
    {
        { "Line", Select(new LineShape(new PointD(10.0, 20.0), new PointD(90.0, 80.0))) },
        { "Rectangle", Select(new RectangleShape(new PointD(15.0, 15.0), new PointD(85.0, 70.0))) },
        { "Circle", Select(new CircleShape(new PointD(50.0, 50.0), new PointD(75.0, 50.0))) },
        { "Ellipse", Select(new EllipseShape(new PointD(20.0, 25.0), new PointD(80.0, 70.0))) },
        { "Arc", Select(new ArcShape(new PointD(50.0, 50.0), 30.0, 0.0, Math.PI)) },
        { "Polyline", Select(new PolylineShape(
            [new PointD(15.0, 20.0), new PointD(45.0, 80.0), new PointD(90.0, 35.0)],
            isClosed: false)) },
        { "Polygon", Select(new PolylineShape(
            [new PointD(20.0, 20.0), new PointD(80.0, 25.0), new PointD(70.0, 80.0), new PointD(25.0, 70.0)],
            isClosed: true)) },
        { "DonutPolygon", Select(new DonutPolygonShape(
            [new PointD(15.0, 15.0), new PointD(85.0, 15.0), new PointD(85.0, 85.0), new PointD(15.0, 85.0)],
            [[new PointD(40.0, 40.0), new PointD(60.0, 40.0), new PointD(60.0, 60.0), new PointD(40.0, 60.0)]])) }
    };

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

    private static int CountPixels(Bitmap bitmap, Func<Color, bool> predicate)
    {
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (predicate(bitmap.GetPixel(x, y)))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private static bool IsSelectionBluePixel(Color color) =>
        color.B > 150 && color.G > 80 && color.R < 80;

    private static bool IsProjectBoundaryRedPixel(Color color) =>
        color.R > 150 && color.G < 90 && color.B < 90;

    private static bool IsCenterSelectionStroke(StrokeStyle stroke) =>
        stroke.Color.ToArgb() == Color.FromArgb(0, 120, 212).ToArgb();

    private static void AssertSelectionStrokeMetrics(CanvasLayer layer, StrokeStyle selectionStroke)
    {
        // Regression lock: selection must use the selected object's layer
        // linetype while staying exactly 1px wider than that layer stroke.
        Assert.Equal(layer.LineWeight + 1.0, selectionStroke.Width, precision: 5);
        Assert.Equal(
            layer.LineTypeScale * layer.LineWeight,
            selectionStroke.DashScale * selectionStroke.Width,
            precision: 5);
    }

    private static IShape Select(IShape shape)
    {
        shape.IsSelected = true;
        return shape;
    }

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
            new GdiMapPathBuilder(fillRule);

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
