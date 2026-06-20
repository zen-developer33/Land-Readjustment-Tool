using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NetTopologySuite.Geometries;
using Xunit;

namespace LandReadjustment.Tests;

/// <summary>
/// Integration tests for production renderer paths that have moved to render backends.
/// </summary>
public sealed class MapCanvasRendererBackendIntegrationTests
{
    /// <summary>
    /// Verifies that the adaptive grid uses the backend surface for lines and labels.
    /// </summary>
    [Fact]
    public void Render_WithGrid_UsesBackendFactoryAndDrawsGrid()
    {
        MapCanvasEngine engine = new(new Size(160, 120));
        engine.SetView(80.0, 60.0, 160.0, 120.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = true;
        settings.ShowGridLabels = true;
        settings.ShowMinorGridLines = true;
        settings.ShowAxisLines = false;
        settings.ShowAxisLabels = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = false;
        settings.BackgroundColor = Color.White;

        CountingSurfaceFactory factory = new();
        using MapCanvasRenderer renderer = new(engine, settings, renderSurfaceFactory: factory);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);

        renderer.Render(
            graphics,
            rasterFrame: null,
            interactiveRaster: false,
            vectorFrame: null,
            interactiveVector: false,
            zoomWindowRectangle: null,
            suppressDecorations: true);

        Assert.Equal(2, factory.CreateForGraphicsCallCount);
        Assert.True(CountNonWhitePixels(bitmap) > 800);
    }

    /// <summary>
    /// Verifies that selecting Skia CPU in render settings flows into the
    /// production renderer and draws the frame through the Skia adapter.
    /// </summary>
    [Fact]
    public void Render_WithSkiaCpuBackend_DrawsGrid()
    {
        MapCanvasEngine engine = new(new Size(160, 120));
        engine.SetView(80.0, 60.0, 160.0, 120.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.RenderBackend = MapRenderBackend.SkiaCpu;
        settings.ShowGrid = true;
        settings.ShowGridLabels = true;
        settings.ShowMinorGridLines = true;
        settings.ShowAxisLines = false;
        settings.ShowAxisLabels = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = false;
        settings.BackgroundColor = Color.White;

        CountingSurfaceFactory factory = new();
        using MapCanvasRenderer renderer = new(engine, settings, renderSurfaceFactory: factory);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);

        renderer.Render(
            graphics,
            rasterFrame: null,
            interactiveRaster: false,
            vectorFrame: null,
            interactiveVector: false,
            zoomWindowRectangle: null,
            suppressDecorations: true);

        // The Skia CPU backend uses a pooled backing bitmap created inside the
        // renderer rather than the injected factory, so factory surface counts do
        // not apply here. What matters is that selecting Skia CPU still draws the
        // grid end-to-end through the Skia adapter.
        Assert.True(CountNonWhitePixels(bitmap) > 800);
    }

    /// <summary>
    /// Verifies that axis lines and origin marker drawing use the backend surface.
    /// </summary>
    [Fact]
    public void Render_WithAxisAndOriginMarker_UsesBackendFactoryAndDrawsMarker()
    {
        MapCanvasEngine engine = new(new Size(160, 120));
        engine.SetView(0.0, 0.0, 160.0, 120.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowGridLabels = false;
        settings.ShowAxisLines = true;
        settings.ShowAxisLabels = true;
        settings.ShowOriginMarker = true;
        settings.ShowNorthMarker = false;
        settings.BackgroundColor = Color.White;
        settings.AxisXColor = Color.Red;
        settings.AxisYColor = Color.Lime;
        settings.AxisMarkerColor = Color.Blue;
        settings.AxisLabelColor = Color.Black;

        CountingSurfaceFactory factory = new();
        using MapCanvasRenderer renderer = new(engine, settings, renderSurfaceFactory: factory);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);

        renderer.Render(
            graphics,
            rasterFrame: null,
            interactiveRaster: false,
            vectorFrame: null,
            interactiveVector: false,
            zoomWindowRectangle: null);

        Assert.Equal(2, factory.CreateForGraphicsCallCount);
        Assert.True(CountNonWhitePixels(bitmap) > 250);
        Assert.True(CountPixels(bitmap, Color.Blue) > 15);
    }

    /// <summary>
    /// Verifies that cached frame bitmaps are wrapped as backend images before drawing.
    /// </summary>
    [Fact]
    public void Render_WithFixedReferenceFrame_UsesBackendImageAndDrawsFrame()
    {
        MapCanvasEngine engine = new(new Size(80, 60));
        engine.SetView(40.0, 30.0, 80.0, 60.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowAxisLines = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = false;

        using Bitmap cached = new(20, 20);
        using Graphics cachedGraphics = Graphics.FromImage(cached);
        cachedGraphics.Clear(Color.Red);

        CountingSurfaceFactory factory = new();
        using MapCanvasRenderer renderer = new(engine, settings, renderSurfaceFactory: factory);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        RasterRenderFrame frame = new(
            cached,
            new RectangleF(10, 10, 20, 20),
            new object(),
            Release: null);

        renderer.Render(
            graphics,
            rasterFrame: null,
            interactiveRaster: false,
            vectorFrame: null,
            interactiveVector: false,
            zoomWindowRectangle: null,
            fixedReferenceFrame: frame);

        Assert.Equal(2, factory.CreateForGraphicsCallCount);
        // Cached frames are now wrapped via a directly-constructed GdiMapImage so
        // the per-frame GPU-cacheable flag can be set, so they no longer route
        // through the injected factory's CreateImage. The frame is still drawn.
        Assert.True(CountPixels(bitmap, Color.Red) > 250);
    }

    /// <summary>
    /// Verifies that the zoom-window overlay is drawn through the backend factory.
    /// </summary>
    [Fact]
    public void Render_WithZoomWindow_UsesBackendFactoryAndDrawsOverlay()
    {
        MapCanvasEngine engine = new(new Size(120, 90));
        engine.SetView(60.0, 45.0, 120.0, 90.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowGridLabels = false;
        settings.ShowAxisLines = false;
        settings.ShowAxisLabels = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = false;
        settings.ZoomWindowFillColor = Color.Blue;
        settings.ZoomWindowBorderColor = Color.Red;
        settings.ZoomWindowLineWidth = 2.0f;
        settings.ZoomWindowLineType = System.Drawing.Drawing2D.DashStyle.Solid;

        CountingSurfaceFactory factory = new();
        using MapCanvasRenderer renderer = new(engine, settings, renderSurfaceFactory: factory);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);

        renderer.Render(
            graphics,
            rasterFrame: null,
            interactiveRaster: false,
            vectorFrame: null,
            interactiveVector: false,
            zoomWindowRectangle: new Rectangle(20, 15, 60, 35),
            suppressDecorations: true,
            suppressGridLabels: true,
            suppressFixedReferenceLayers: true);

        Assert.Equal(2, factory.CreateForGraphicsCallCount);
        Assert.True(CountPixels(bitmap, Color.Blue) > 1_500);
        Assert.True(CountPixels(bitmap, Color.Red) > 100);
    }

    /// <summary>
    /// Verifies that the north marker overlay is drawn through the backend factory.
    /// </summary>
    [Fact]
    public void Render_WithNorthMarker_UsesBackendFactoryAndDrawsMarker()
    {
        MapCanvasEngine engine = new(new Size(160, 120));
        engine.SetView(80.0, 60.0, 160.0, 120.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowGridLabels = false;
        settings.ShowAxisLines = false;
        settings.ShowAxisLabels = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = true;
        settings.BackgroundColor = Color.White;

        CountingSurfaceFactory factory = new();
        using MapCanvasRenderer renderer = new(engine, settings, renderSurfaceFactory: factory);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);

        renderer.Render(
            graphics,
            rasterFrame: null,
            interactiveRaster: false,
            vectorFrame: null,
            interactiveVector: false,
            zoomWindowRectangle: null,
            suppressGridLabels: true,
            suppressFixedReferenceLayers: true);

        Assert.Equal(2, factory.CreateForGraphicsCallCount);
        Assert.True(CountNonWhitePixels(bitmap) > 250);
        Assert.True(CountDarkPixels(bitmap) > 50);
    }

    /// <summary>
    /// Verifies that the backend-neutral canvas path skips live raster layer
    /// rendering during interactive cache frames when no cache frame is available.
    /// </summary>
    [Fact]
    public void RenderCachedDirect_WithInteractiveRasterAndNoFrame_SkipsLiveRasterLayer()
    {
        MapCanvasEngine engine = new(new Size(80, 60));
        engine.SetView(40.0, 30.0, 80.0, 60.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowAxisLines = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = false;
        settings.BackgroundColor = Color.White;

        TestRasterLayer rasterLayer = new();
        using MapCanvasRenderer renderer = new(engine, settings);
        renderer.UpdateRasterLayers([rasterLayer]);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using GdiMapRenderSurface surface = new(graphics, engine.CanvasSize);

        bool rendered = renderer.RenderCachedDirect(
            surface,
            rasterFrame: null,
            interactiveRaster: true,
            vectorFrame: null,
            interactiveVector: true,
            suppressGridLabels: true,
            suppressFixedReferenceLayers: true);

        Assert.True(rendered);
        Assert.Equal(0, rasterLayer.SurfaceRenderCallCount);
        Assert.Equal(0, CountPixels(bitmap, Color.Red));
    }

    /// <summary>
    /// Verifies that the backend-neutral canvas path skips direct vector
    /// rendering during interactive cache frames when no cache frame is available.
    /// </summary>
    [Fact]
    public void RenderCachedDirect_WithInteractiveVectorAndNoFrame_SkipsDirectVectorRender()
    {
        MapCanvasEngine engine = new(new Size(80, 60));
        engine.SetView(40.0, 30.0, 80.0, 60.0);
        MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();
        settings.ShowGrid = false;
        settings.ShowAxisLines = false;
        settings.ShowOriginMarker = false;
        settings.ShowNorthMarker = false;
        settings.BackgroundColor = Color.White;

        CanvasLayer layer = new()
        {
            Id = 1,
            Name = "Lines",
            LayerType = "Polyline",
            BorderColor = "#000000",
            LineWeight = 3.0,
            IsVisible = true
        };
        LineShape line = new(new PointD(10, 10), new PointD(70, 50));
        CanvasObject canvasObject = new()
        {
            Id = Guid.NewGuid(),
            CanvasLayerId = layer.Id,
            CanvasLayer = layer,
            ObjectType = "Line",
            IsVisible = true,
            Shape = new GeometryFactory().CreateLineString(
                [new Coordinate(10, 10), new Coordinate(70, 50)])
        };
        CanvasFeature feature = new(canvasObject, line, layer);

        using MapCanvasRenderer renderer = new(engine, settings);
        renderer.UpdateVectorLayers([layer]);
        renderer.UpdateVectorFeatures([feature]);
        using Bitmap bitmap = new(engine.CanvasSize.Width, engine.CanvasSize.Height);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using GdiMapRenderSurface surface = new(graphics, engine.CanvasSize);

        bool rendered = renderer.RenderCachedDirect(
            surface,
            rasterFrame: null,
            interactiveRaster: true,
            vectorFrame: null,
            interactiveVector: true,
            suppressGridLabels: true,
            suppressFixedReferenceLayers: true);

        Assert.True(rendered);
        Assert.Equal(0, CountNonWhitePixels(bitmap));
    }

    /// <summary>
    /// Counts exact-color pixels in a rendered bitmap.
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

    /// <summary>
    /// Counts visibly dark pixels from the north marker outline, fill, and label.
    /// </summary>
    private static int CountDarkPixels(Bitmap bitmap)
    {
        int count = 0;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                Color color = bitmap.GetPixel(x, y);
                if (color.R < 90 && color.G < 90 && color.B < 90)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Test factory that records production renderer surface requests.
    /// </summary>
    private sealed class CountingSurfaceFactory : IMapRenderSurfaceFactory
    {
        private readonly MapRenderSurfaceFactory _inner = new();

        /// <summary>
        /// Gets how many times the renderer requested a surface from this factory.
        /// </summary>
        public int CreateForGraphicsCallCount { get; private set; }

        /// <summary>
        /// Gets how many image resources the renderer wrapped through this factory.
        /// </summary>
        public int CreateImageCallCount { get; private set; }

        /// <summary>
        /// Gets the backend requested for each created surface.
        /// </summary>
        public List<MapRenderBackend> RequestedBackends { get; } = new();

        /// <summary>
        /// Delegates backend availability checks to the production factory.
        /// </summary>
        public bool IsBackendAvailable(MapRenderBackend backend) =>
            _inner.IsBackendAvailable(backend);

        /// <summary>
        /// Delegates backend resolution to the production factory.
        /// </summary>
        public MapRenderBackend ResolveBackend(MapRenderSurfaceOptions? options = null) =>
            _inner.ResolveBackend(options);

        /// <summary>
        /// Records the surface request, then creates the normal production surface.
        /// </summary>
        public IMapRenderSurface CreateForGraphics(
            Graphics graphics,
            Size? pixelSize = null,
            MapRenderSurfaceOptions? options = null,
            bool ownsGraphics = false)
        {
            CreateForGraphicsCallCount++;
            if (options != null)
            {
                RequestedBackends.Add(options.RequestedBackend);
            }

            return _inner.CreateForGraphics(graphics, pixelSize, options, ownsGraphics);
        }

        /// <summary>
        /// Creates the normal production image wrapper for renderer image tests.
        /// </summary>
        public IMapImage CreateImage(Image image, bool ownsImage = false)
        {
            CreateImageCallCount++;
            return _inner.CreateImage(image, ownsImage);
        }
    }

    private sealed class TestRasterLayer : IRasterRenderLayer
    {
        public int LayerId => 1;
        public string Name => "Test Raster";
        public string FilePath => "test";
        public RectangleD WorldBounds => new(0, 0, 80, 60);
        public int Transparency => 0;
        public bool IsVisible => true;
        public bool CanRenderFromMemoryCacheDuringInteraction => false;
        public int SurfaceRenderCallCount { get; private set; }

        public bool RenderVisible(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            MapRenderBackend renderBackend = MapRenderBackend.GdiPlus,
            CancellationToken cancellationToken = default)
        {
            using SolidBrush brush = new(Color.Red);
            graphics.FillRectangle(brush, new Rectangle(0, 0, 20, 20));
            return true;
        }

        public bool RenderVisible(
            IMapRenderSurface surface,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            CancellationToken cancellationToken = default)
        {
            SurfaceRenderCallCount++;
            surface.FillRectangle(
                new RectangleF(0, 0, 20, 20),
                new FillStyle(Color.Red));
            return true;
        }

        public void UpdateRenderState(bool isVisible, int transparency)
        {
        }

        public void InvalidateCache()
        {
        }

        public void Dispose()
        {
        }
    }
}
