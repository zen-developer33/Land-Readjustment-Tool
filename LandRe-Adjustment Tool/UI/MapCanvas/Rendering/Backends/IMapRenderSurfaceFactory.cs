using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends
{
    /// <summary>
    /// Creates backend-neutral render surfaces for the map canvas.
    /// </summary>
    /// <remarks>
    /// Production renderers should depend on this factory instead of directly
    /// constructing <see cref="Gdi.GdiMapRenderSurface"/> or future SkiaSharp
    /// surfaces. That keeps backend selection at the host boundary.
    /// </remarks>
    public interface IMapRenderSurfaceFactory
    {
        /// <summary>
        /// Returns whether the requested backend can create surfaces in this build.
        /// </summary>
        /// <param name="backend">Rendering backend to query.</param>
        bool IsBackendAvailable(MapRenderBackend backend);

        /// <summary>
        /// Resolves the effective backend after applying availability and fallback rules.
        /// </summary>
        /// <param name="options">Backend selection and initialization options.</param>
        MapRenderBackend ResolveBackend(MapRenderSurfaceOptions? options = null);

        /// <summary>
        /// Creates a render surface for the existing Windows Forms/GDI+ paint target.
        /// </summary>
        /// <param name="graphics">Native WinForms/GDI+ drawing target.</param>
        /// <param name="pixelSize">Optional target size in physical pixels.</param>
        /// <param name="options">Backend selection and quality initialization options.</param>
        /// <param name="ownsGraphics">
        /// Whether the created surface should dispose the supplied graphics target.
        /// </param>
        IMapRenderSurface CreateForGraphics(
            Graphics graphics,
            Size? pixelSize = null,
            MapRenderSurfaceOptions? options = null,
            bool ownsGraphics = false);

        /// <summary>
        /// Creates a backend-owned image wrapper for a native image resource.
        /// </summary>
        /// <param name="image">Native image resource used by the current backend.</param>
        /// <param name="ownsImage">
        /// Whether the returned image wrapper should dispose the native image.
        /// </param>
        IMapImage CreateImage(Image image, bool ownsImage = false);
    }
}
