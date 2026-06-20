using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Xunit;

namespace LandReadjustment.Tests;

/// <summary>
/// Tests deferred cache renderers after their bitmap clear/blit work moved
/// through backend render surfaces.
/// </summary>
public sealed class DeferredRendererBackendTests
{
    /// <summary>
    /// Verifies that the vector deferred renderer can build a cache, create a
    /// pan buffer, and expose the pan frame after backend-surface cache blits.
    /// </summary>
    [Fact]
    public void VectorDeferredRenderer_BeginPan_ProducesPanFrame()
    {
        using VectorDeferredRenderer deferredRenderer = new();
        using CanvasVectorRenderer vectorRenderer = new();
        MapCanvasEngine engine = CreateEngine(100, 80);

        bool rendered = deferredRenderer.RenderNow(
            new Size(100, 80),
            vectorRenderer,
            engine,
            antiAliasingEnabled: true);
        bool panStarted = deferredRenderer.BeginPan(
            new Size(100, 80),
            graphics =>
            {
                using SolidBrush brush = new(Color.Red);
                graphics.FillRectangle(brush, 8, 8, 20, 20);
            });

        Assert.True(rendered);
        Assert.True(panStarted);
        Assert.True(deferredRenderer.TryGetPanFrame(new PointF(3, 4), out RasterRenderFrame frame));
        using (frame)
        {
            Assert.Equal(new RectangleF(3, 4, 100, 80), frame.Destination);
            Assert.True(CountPixels(frame.Bitmap, Color.Red) > 300);
        }
    }

    /// <summary>
    /// Verifies that raster layer composition and pan-buffer creation preserve
    /// rendered layer pixels through backend image drawing.
    /// </summary>
    [Fact]
    public void RasterDeferredRenderer_RenderAndBeginPan_PreservesLayerPixels()
    {
        using RasterDeferredRenderer deferredRenderer = new();
        SolidRasterLayer layer = new(1, Color.Blue);
        MapCanvasEngine engine = CreateEngine(120, 90);

        bool rendered = deferredRenderer.RenderNow(
            new Size(120, 90),
            [layer],
            engine);
        bool panStarted = deferredRenderer.BeginPan(
            new Size(120, 90),
            [layer],
            engine);

        Assert.True(rendered);
        Assert.True(panStarted);
        Assert.True(deferredRenderer.TryGetPanFrame(new PointF(5, 6), out RasterRenderFrame frame));
        using (frame)
        {
            Assert.Equal(new RectangleF(5, 6, 120, 90), frame.Destination);
            Assert.True(CountPixels(frame.Bitmap, Color.Blue) > 1_000);
        }
    }

    /// <summary>
    /// Creates a simple map engine with a matching world/screen view.
    /// </summary>
    private static MapCanvasEngine CreateEngine(int width, int height)
    {
        MapCanvasEngine engine = new(new Size(width, height));
        engine.SetView(width / 2.0, height / 2.0, width, height);
        return engine;
    }

    /// <summary>
    /// Counts exact-color pixels in a bitmap.
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

    /// <summary>
    /// Minimal raster layer used to exercise deferred raster composition.
    /// </summary>
    private sealed class SolidRasterLayer(int layerId, Color color) : IRasterRenderLayer
    {
        public int LayerId { get; } = layerId;

        public string Name => "Solid test raster";

        public string FilePath => string.Empty;

        public RectangleD WorldBounds => new(0, 0, 120, 90);

        public int Transparency => 0;

        public bool IsVisible { get; private set; } = true;

        public bool CanRenderFromMemoryCacheDuringInteraction => true;

        public bool RenderVisible(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            MapRenderBackend renderBackend = MapRenderBackend.GdiPlus,
            CancellationToken cancellationToken = default)
        {
            using SolidBrush brush = new(color);
            graphics.FillRectangle(brush, 10, 10, 50, 30);
            return true;
        }

        public bool RenderVisible(
            IMapRenderSurface surface,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            CancellationToken cancellationToken = default)
        {
            surface.FillRectangle(
                new RectangleF(10, 10, 50, 30),
                new FillStyle(color));
            return true;
        }

        public void UpdateRenderState(bool isVisible, int transparency)
        {
            IsVisible = isVisible;
        }

        public void InvalidateCache()
        {
        }

        public void Dispose()
        {
        }
    }
}
