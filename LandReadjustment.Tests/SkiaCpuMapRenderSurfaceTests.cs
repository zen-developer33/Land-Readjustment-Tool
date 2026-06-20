using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace LandReadjustment.Tests;

/// <summary>
/// Smoke tests for the SkiaSharp CPU implementation of the backend-neutral
/// rendering surface.
/// </summary>
/// <remarks>
/// These tests draw through the WinForms bridge into in-memory bitmaps. They
/// verify that the CPU adapter produces visible output without requiring a GPU
/// context or a live window handle.
/// </remarks>
public sealed class SkiaCpuMapRenderSurfaceTests
{
    private static readonly BindingFlags PrivateInstance =
        BindingFlags.Instance | BindingFlags.NonPublic;

    /// <summary>
    /// Verifies that primitive drawing methods produce visible output after the
    /// Skia surface is flushed back to the wrapped GDI+ target.
    /// </summary>
    [Fact]
    public void DrawPrimitives_RendersVisiblePixels()
    {
        using Bitmap bitmap = new(240, 180);
        using Graphics graphics = Graphics.FromImage(bitmap);

        using (SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size))
        {
            surface.Clear(Color.Black);
            surface.SetQuality(RenderQuality.VectorHighSpeed);

            StrokeStyle redStroke = new(Color.Red, 3.0f);
            StrokeStyle yellowStroke = new(Color.Yellow, 2.0f, DashPatternKind.Dashed);
            FillStyle greenFill = new(Color.Lime);
            FillStyle blueFill = new(Color.Blue);

            surface.DrawLine(new PointF(10, 10), new PointF(90, 10), redStroke);
            surface.DrawRectangle(new RectangleF(10, 25, 50, 30), yellowStroke);
            surface.FillRectangle(new RectangleF(70, 25, 40, 30), greenFill);
            surface.DrawEllipse(new RectangleF(125, 20, 45, 35), redStroke);
            surface.FillEllipse(new RectangleF(180, 20, 35, 35), blueFill);
            surface.DrawArc(new RectangleF(20, 80, 60, 45), 0.0f, 210.0f, redStroke);

            using IMapPath path = CreateTrianglePath(surface);
            surface.FillPath(path, new FillStyle(Color.Orange));
            surface.DrawPath(path, new StrokeStyle(Color.White, 2.0f));
        }

        Assert.True(CountNonBlackPixels(bitmap) > 3_000);
        Assert.True(CountPixels(bitmap, Color.Lime) > 900);
        Assert.True(CountPixels(bitmap, Color.Blue) > 400);
    }

    /// <summary>
    /// Verifies that Skia can draw legacy GDI paths produced by still-migrating
    /// renderer code.
    /// </summary>
    [Fact]
    public void DrawPath_WithGdiPath_RendersVisiblePixels()
    {
        using Bitmap bitmap = new(120, 90);
        using Graphics graphics = Graphics.FromImage(bitmap);

        using (SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size))
        {
            surface.Clear(Color.Black);
            surface.SetQuality(RenderQuality.VectorHighSpeed);
            GraphicsPath nativePath = new();
            nativePath.AddPolygon(
            [
                new PointF(15, 15),
                new PointF(95, 30),
                new PointF(40, 75)
            ]);

            using GdiMapPath path = new(nativePath, FillRule.Winding);
            surface.FillPath(path, new FillStyle(Color.Lime));
        }

        Assert.True(CountPixels(bitmap, Color.Lime) > 1_500);
    }

    /// <summary>
    /// Verifies that clipping is scoped by the save/restore state token.
    /// </summary>
    [Fact]
    public void ClipPath_WithSaveState_RestoresPreviousClip()
    {
        using Bitmap bitmap = new(100, 60);
        using Graphics graphics = Graphics.FromImage(bitmap);

        using (SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size))
        {
            surface.Clear(Color.Black);
            surface.SetQuality(RenderQuality.VectorHighSpeed);

            using IMapPath leftHalfClip = CreateRectanglePath(surface, new RectangleF(0, 0, 50, 60));
            using (surface.SaveState())
            {
                surface.ClipPath(leftHalfClip);
                surface.FillRectangle(new RectangleF(0, 0, 100, 60), new FillStyle(Color.Blue));
            }

            surface.FillRectangle(new RectangleF(65, 15, 20, 20), new FillStyle(Color.Yellow));
        }

        Assert.Equal(Color.Blue.ToArgb(), bitmap.GetPixel(25, 30).ToArgb());
        Assert.Equal(Color.Black.ToArgb(), bitmap.GetPixel(55, 30).ToArgb());
        Assert.Equal(Color.Yellow.ToArgb(), bitmap.GetPixel(75, 25).ToArgb());
    }

    /// <summary>
    /// Verifies text measurement, text drawing, and GDI image drawing through
    /// the Skia adapter.
    /// </summary>
    [Fact]
    public void DrawTextAndImage_RendersVisiblePixels()
    {
        SizeF measured;
        using Bitmap bitmap = new(160, 90);
        using Graphics graphics = Graphics.FromImage(bitmap);

        using (SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size))
        {
            surface.Clear(Color.Black);
            surface.SetQuality(RenderQuality.VectorHighSpeed);

            TextStyle textStyle = new(
                "Segoe UI",
                18.0f,
                Color.White,
                Bold: true,
                HorizontalAlign: TextAlign.Near,
                VerticalAlign: TextAlign.Near);

            measured = surface.MeasureText("Parcel 42", textStyle);
            surface.DrawText("Parcel 42", new RectangleF(8, 8, 120, 30), textStyle);

            using Bitmap image = CreateSourceImage();
            using GdiMapImage mapImage = new(image);
            surface.DrawImage(
                mapImage,
                new RectangleF(20, 50, 50, 25),
                new RectangleF(0, 0, 10, 10),
                new ImageStyle(1.0f, ImageInterpolation.NearestNeighbor));
        }

        Assert.True(measured.Width > 0.0f);
        Assert.True(measured.Height > 0.0f);
        Assert.True(CountNonBlackPixels(bitmap) > 1_000);
        Assert.True(CountPixels(bitmap, Color.Red) > 400);
    }

    /// <summary>
    /// Verifies that projected-tile parallelogram image drawing works through
    /// the Skia CPU adapter.
    /// </summary>
    [Fact]
    public void DrawImage_WithDestinationParallelogram_RendersVisiblePixels()
    {
        using Bitmap bitmap = new(120, 90);
        using Graphics graphics = Graphics.FromImage(bitmap);

        using (SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size))
        {
            surface.Clear(Color.Black);
            surface.SetQuality(RenderQuality.RasterHighSpeed);

            using Bitmap image = CreateSourceImage();
            using GdiMapImage mapImage = new(image);
            surface.DrawImage(
                mapImage,
                [
                    new PointF(20, 15),
                    new PointF(95, 25),
                    new PointF(12, 70)
                ],
                new RectangleF(0, 0, image.Width, image.Height),
                new ImageStyle(1.0f, ImageInterpolation.NearestNeighbor, TileFlipXY: true));
        }

        Assert.True(CountPixels(bitmap, Color.Red) > 2_000);
    }

    /// <summary>
    /// Verifies that repeated stroke style resolution reuses the cached paint
    /// instead of allocating one Skia paint per draw call.
    /// </summary>
    [Fact]
    public void CreateStrokePaint_WithSameStyle_ReusesCachedPaint()
    {
        using Bitmap bitmap = new(32, 24);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size);
        surface.SetQuality(RenderQuality.VectorHighQuality);

        StrokeStyle style = new(Color.Red, 2.0f, DashPatternKind.Dashed);

        SKPaint first = InvokeCreateStrokePaint(surface, style);
        SKPaint second = InvokeCreateStrokePaint(surface, style);

        Assert.Same(first, second);
    }

    /// <summary>
    /// Verifies that antialiasing participates in the cache key, so high-speed
    /// and high-quality render passes never share a paint with stale quality.
    /// </summary>
    [Fact]
    public void CreateStrokePaint_WithDifferentQuality_UsesDifferentCachedPaint()
    {
        using Bitmap bitmap = new(32, 24);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using SkiaCpuMapRenderSurface surface = new(graphics, bitmap.Size);

        StrokeStyle style = new(Color.Red, 2.0f);

        surface.SetQuality(RenderQuality.VectorHighQuality);
        SKPaint highQuality = InvokeCreateStrokePaint(surface, style);

        surface.SetQuality(RenderQuality.VectorHighSpeed);
        SKPaint highSpeed = InvokeCreateStrokePaint(surface, style);

        Assert.NotSame(highQuality, highSpeed);
        Assert.True(highQuality.IsAntialias);
        Assert.False(highSpeed.IsAntialias);
    }

    /// <summary>
    /// Verifies that the direct Skia canvas surface used by SKControl/SKGLControl
    /// shares the same paint-cache behavior as the bitmap-backed CPU bridge.
    /// </summary>
    [Fact]
    public void DirectCanvasCreateStrokePaint_WithSameStyle_ReusesCachedPaint()
    {
        using SKSurface skSurface = SKSurface.Create(new SKImageInfo(32, 24));
        using SkiaCanvasMapRenderSurface surface = new(skSurface.Canvas, new Size(32, 24));
        surface.SetQuality(RenderQuality.VectorHighQuality);

        StrokeStyle style = new(Color.Red, 2.0f, DashPatternKind.Dashed);

        SKPaint first = InvokeCreateStrokePaint(surface, style);
        SKPaint second = InvokeCreateStrokePaint(surface, style);

        Assert.Same(first, second);
    }

    /// <summary>
    /// Verifies that the direct Skia canvas surface keeps quality in its cache
    /// key, preventing stale antialias state when users switch render presets.
    /// </summary>
    [Fact]
    public void DirectCanvasCreateStrokePaint_WithDifferentQuality_UsesDifferentCachedPaint()
    {
        using SKSurface skSurface = SKSurface.Create(new SKImageInfo(32, 24));
        using SkiaCanvasMapRenderSurface surface = new(skSurface.Canvas, new Size(32, 24));
        StrokeStyle style = new(Color.Red, 2.0f);

        surface.SetQuality(RenderQuality.VectorHighQuality);
        SKPaint highQuality = InvokeCreateStrokePaint(surface, style);

        surface.SetQuality(RenderQuality.VectorHighSpeed);
        SKPaint highSpeed = InvokeCreateStrokePaint(surface, style);

        Assert.NotSame(highQuality, highSpeed);
        Assert.True(highQuality.IsAntialias);
        Assert.False(highSpeed.IsAntialias);
    }

    /// <summary>
    /// Creates a triangle path through the same surface interface production
    /// code uses.
    /// </summary>
    private static IMapPath CreateTrianglePath(SkiaCpuMapRenderSurface surface)
    {
        IMapPathBuilder builder = surface.CreatePath();
        builder.AddPolygon(
        [
            new PointF(120, 90),
            new PointF(190, 130),
            new PointF(105, 150)
        ]);
        return builder.Build();
    }

    /// <summary>
    /// Creates a rectangle path for clipping tests.
    /// </summary>
    private static IMapPath CreateRectanglePath(SkiaCpuMapRenderSurface surface, RectangleF rectangle)
    {
        IMapPathBuilder builder = surface.CreatePath();
        builder.AddRectangle(rectangle);
        return builder.Build();
    }

    private static SKPaint InvokeCreateStrokePaint(SkiaCpuMapRenderSurface surface, StrokeStyle style)
    {
        MethodInfo method = typeof(SkiaCpuMapRenderSurface).GetMethod(
            "CreateStrokePaint",
            PrivateInstance)
            ?? throw new MissingMethodException(nameof(SkiaCpuMapRenderSurface), "CreateStrokePaint");

        object?[] parameters = [style];
        return Assert.IsType<SKPaint>(method.Invoke(surface, parameters));
    }

    private static SKPaint InvokeCreateStrokePaint(SkiaCanvasMapRenderSurface surface, StrokeStyle style)
    {
        MethodInfo method = typeof(SkiaCanvasMapRenderSurface).GetMethod(
            "CreateStrokePaint",
            PrivateInstance)
            ?? throw new MissingMethodException(nameof(SkiaCanvasMapRenderSurface), "CreateStrokePaint");

        object?[] parameters = [style];
        return Assert.IsType<SKPaint>(method.Invoke(surface, parameters));
    }

    /// <summary>
    /// Creates a tiny source bitmap with a predictable solid fill.
    /// </summary>
    private static Bitmap CreateSourceImage()
    {
        Bitmap image = new(10, 10);
        using Graphics graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Red);
        return image;
    }

    /// <summary>
    /// Counts pixels that differ from the black test background.
    /// </summary>
    private static int CountNonBlackPixels(Bitmap bitmap)
    {
        int black = Color.Black.ToArgb();
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y).ToArgb() != black)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Counts exact-color pixels for high-speed rendering operations.
    /// </summary>
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
