using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia;
using Xunit;

namespace LandReadjustment.Tests;

/// <summary>
/// Tests backend selection and surface creation for the map render factory.
/// </summary>
public sealed class MapRenderSurfaceFactoryTests
{
    /// <summary>
    /// Verifies that the default factory creates the current production GDI+ surface.
    /// </summary>
    [Fact]
    public void CreateForGraphics_DefaultOptions_ReturnsGdiSurface()
    {
        using Bitmap bitmap = new(32, 24);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapRenderSurfaceFactory factory = new();

        using IMapRenderSurface surface = factory.CreateForGraphics(graphics, bitmap.Size);

        GdiMapRenderSurface gdiSurface = Assert.IsType<GdiMapRenderSurface>(surface);
        Assert.Equal(bitmap.Size, gdiSurface.PixelSize);
        Assert.True(factory.IsBackendAvailable(MapRenderBackend.GdiPlus));
        Assert.True(factory.IsBackendAvailable(MapRenderBackend.SkiaCpu));
        Assert.True(factory.IsBackendAvailable(MapRenderBackend.SkiaSharp));
    }

    /// <summary>
    /// Verifies that Skia CPU requests create the Skia CPU surface.
    /// </summary>
    [Fact]
    public void CreateForGraphics_SkiaCpuRequested_ReturnsSkiaCpuSurface()
    {
        using Bitmap bitmap = new(32, 24);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapRenderSurfaceFactory factory = new();
        MapRenderSurfaceOptions options = new()
        {
            RequestedBackend = MapRenderBackend.SkiaCpu,
            FallbackToGdiPlusWhenUnavailable = false
        };

        using IMapRenderSurface surface = factory.CreateForGraphics(graphics, bitmap.Size, options);

        SkiaCpuMapRenderSurface skiaSurface = Assert.IsType<SkiaCpuMapRenderSurface>(surface);
        Assert.Equal(bitmap.Size, skiaSurface.PixelSize);
        Assert.Equal(MapRenderBackend.SkiaCpu, factory.ResolveBackend(options));
    }

    /// <summary>
    /// Verifies Skia GPU resolution: when the GPU backend is available it is used,
    /// otherwise the request falls back to GDI+. Availability is probed at runtime.
    /// </summary>
    [Fact]
    public void CreateForGraphics_SkiaGpuRequestedWithFallback_ResolvesByAvailability()
    {
        using Bitmap bitmap = new(32, 24);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapRenderSurfaceFactory factory = new();
        MapRenderSurfaceOptions options = new()
        {
            RequestedBackend = MapRenderBackend.SkiaGpu,
            FallbackToGdiPlusWhenUnavailable = true
        };

        using IMapRenderSurface surface = factory.CreateForGraphics(graphics, bitmap.Size, options);

        if (factory.IsBackendAvailable(MapRenderBackend.SkiaGpu))
        {
            Assert.IsType<SkiaGpuMapRenderSurface>(surface);
            Assert.Equal(MapRenderBackend.SkiaGpu, factory.ResolveBackend(options));
        }
        else
        {
            Assert.IsType<GdiMapRenderSurface>(surface);
            Assert.Equal(MapRenderBackend.GdiPlus, factory.ResolveBackend(options));
        }
    }

    /// <summary>
    /// Verifies strict backend selection: Skia GPU resolves when available, and
    /// throws when unavailable and fallback is disabled.
    /// </summary>
    [Fact]
    public void ResolveBackend_SkiaGpuRequestedWithoutFallback_ResolvesOrThrows()
    {
        MapRenderSurfaceFactory factory = new();
        MapRenderSurfaceOptions options = new()
        {
            RequestedBackend = MapRenderBackend.SkiaGpu,
            FallbackToGdiPlusWhenUnavailable = false
        };

        if (factory.IsBackendAvailable(MapRenderBackend.SkiaGpu))
        {
            Assert.Equal(MapRenderBackend.SkiaGpu, factory.ResolveBackend(options));
        }
        else
        {
            Assert.Throws<NotSupportedException>(() => factory.ResolveBackend(options));
        }
    }

    /// <summary>
    /// Verifies that the factory applies the requested initial quality preset.
    /// </summary>
    [Fact]
    public void CreateForGraphics_AppliesInitialQuality()
    {
        using Bitmap bitmap = new(32, 24);
        using Graphics graphics = Graphics.FromImage(bitmap);
        MapRenderSurfaceFactory factory = new();
        MapRenderSurfaceOptions options = new()
        {
            InitialQuality = RenderQuality.VectorHighSpeed
        };

        using IMapRenderSurface surface = factory.CreateForGraphics(graphics, bitmap.Size, options);

        Assert.IsType<GdiMapRenderSurface>(surface);
        Assert.Equal(SmoothingMode.None, graphics.SmoothingMode);
        Assert.Equal(InterpolationMode.NearestNeighbor, graphics.InterpolationMode);
    }
}
