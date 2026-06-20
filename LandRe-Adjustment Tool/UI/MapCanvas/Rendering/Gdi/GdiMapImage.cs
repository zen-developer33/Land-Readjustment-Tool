using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi
{
    /// <summary>
    /// GDI+ implementation of <see cref="IMapImage"/> that wraps a
    /// <see cref="Image"/> instance.
    /// </summary>
    public sealed class GdiMapImage : IMapImage
    {
        private readonly bool _ownsImage;
        private bool _disposed;

        /// <summary>
        /// Creates an image handle for the GDI+ backend.
        /// </summary>
        /// <param name="image">Native GDI+ image to draw.</param>
        /// <param name="ownsImage">
        /// Whether this wrapper should dispose the native image when the wrapper
        /// is disposed.
        /// </param>
        public GdiMapImage(Image image, bool ownsImage = false)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
            _ownsImage = ownsImage;
        }

        /// <summary>
        /// Gets the native GDI+ image used by <see cref="GdiMapRenderSurface"/>.
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// Gets the native image size in pixels.
        /// </summary>
        public Size PixelSize => Image.Size;

        /// <summary>
        /// Releases the wrapped image if this wrapper owns it.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (_ownsImage)
            {
                Image.Dispose();
            }
        }
    }
}
