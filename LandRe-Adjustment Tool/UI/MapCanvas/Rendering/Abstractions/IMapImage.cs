namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Represents a backend-owned image or bitmap that can be drawn by an
    /// <see cref="IMapRenderSurface"/>.
    /// </summary>
    /// <remarks>
    /// Implementations wrap native image types such as GDI+ <see cref="Image"/>
    /// or future Skia image objects. Consumers should not unwrap backend-native
    /// objects directly.
    /// </remarks>
    public interface IMapImage : IDisposable
    {
        /// <summary>
        /// Gets the image size in physical pixels.
        /// </summary>
        Size PixelSize { get; }
    }
}
