using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia;
using System.Drawing.Imaging;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends
{
    /// <summary>
    /// Default render-surface factory for the map canvas.
    /// </summary>
    /// <remarks>
    /// GDI+, Skia CPU, and Skia GPU are represented here so callers can resolve
    /// the backend selected by the project settings.
    /// </remarks>
    public sealed class MapRenderSurfaceFactory : IMapRenderSurfaceFactory
    {
        /// <summary>
        /// Gets a reusable factory instance for callers that do not use dependency injection.
        /// </summary>
        public static MapRenderSurfaceFactory Default { get; } = new();

        /// <summary>
        /// Returns whether the requested backend can create a surface in this build.
        /// </summary>
        public bool IsBackendAvailable(MapRenderBackend backend) =>
            backend switch
            {
                MapRenderBackend.GdiPlus => true,
                MapRenderBackend.SkiaCpu => true,
                MapRenderBackend.SkiaGpu => SkiaGlContext.IsAvailable,
                _ => false
            };

        /// <summary>
        /// Resolves the backend that should actually be used for a frame.
        /// </summary>
        /// <param name="options">Backend selection and fallback options.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown when the requested backend is unavailable and fallback is disabled.
        /// </exception>
        public MapRenderBackend ResolveBackend(MapRenderSurfaceOptions? options = null)
        {
            MapRenderSurfaceOptions resolvedOptions = options ?? MapRenderSurfaceOptions.Default;
            if (IsBackendAvailable(resolvedOptions.RequestedBackend))
            {
                return resolvedOptions.RequestedBackend;
            }

            if (resolvedOptions.FallbackToGdiPlusWhenUnavailable &&
                IsBackendAvailable(MapRenderBackend.GdiPlus))
            {
                return MapRenderBackend.GdiPlus;
            }

            throw new NotSupportedException(
                $"The requested map render backend '{resolvedOptions.RequestedBackend}' is not available in this build.");
        }

        /// <summary>
        /// Creates a backend-neutral surface for an existing WinForms/GDI+ graphics target.
        /// </summary>
        /// <remarks>
        /// This method is the bridge used by the current renderer. A future Skia
        /// host should add a Skia-specific creation method instead of forcing
        /// SkiaSharp through <see cref="Graphics"/>.
        /// </remarks>
        public IMapRenderSurface CreateForGraphics(
            Graphics graphics,
            Size? pixelSize = null,
            MapRenderSurfaceOptions? options = null,
            bool ownsGraphics = false)
        {
            ArgumentNullException.ThrowIfNull(graphics);

            MapRenderSurfaceOptions resolvedOptions = options ?? MapRenderSurfaceOptions.Default;
            MapRenderBackend backend = ResolveBackend(resolvedOptions);
            Size resolvedPixelSize = pixelSize ?? Size.Round(graphics.VisibleClipBounds.Size);
            if (backend == MapRenderBackend.SkiaGpu &&
                !ownsGraphics &&
                SkiaGpuFrameRenderScope.TryCreateLease(
                    graphics,
                    resolvedPixelSize,
                    resolvedOptions,
                    out IMapRenderSurface lease))
            {
                return lease;
            }

            IMapRenderSurface surface = backend switch
            {
                MapRenderBackend.GdiPlus => new GdiMapRenderSurface(graphics, resolvedPixelSize, ownsGraphics),
                MapRenderBackend.SkiaCpu => new SkiaCpuMapRenderSurface(graphics, resolvedPixelSize, ownsGraphics),
                MapRenderBackend.SkiaGpu => new SkiaGpuMapRenderSurface(graphics, resolvedPixelSize, ownsGraphics),
                _ => throw new NotSupportedException(
                    $"The backend '{backend}' cannot create a surface from a GDI+ Graphics target.")
            };

            if (resolvedOptions.ApplyInitialQuality)
            {
                surface.SetQuality(resolvedOptions.InitialQuality);
            }

            return surface;
        }

        /// <summary>
        /// Creates a backend-owned image wrapper for the current production backend.
        /// </summary>
        /// <remarks>
        /// The WinForms host supplies GDI+ images today, so the default factory
        /// wraps them as <see cref="GdiMapImage"/>. A future Skia host should add
        /// a Skia-specific image creation path instead of leaking native image
        /// types into renderer code.
        /// </remarks>
        public IMapImage CreateImage(Image image, bool ownsImage = false)
        {
            ArgumentNullException.ThrowIfNull(image);
            return new GdiMapImage(image, ownsImage);
        }
    }
}
