using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi
{
    /// <summary>
    /// GDI+ implementation of <see cref="IMapPath"/> that wraps a
    /// <see cref="GraphicsPath"/>.
    /// </summary>
    public sealed class GdiMapPath : IMapPath
    {
        private bool _disposed;

        /// <summary>
        /// Creates a path handle for the GDI+ backend.
        /// </summary>
        /// <param name="path">Native GDI+ path built by <see cref="GdiMapPathBuilder"/>.</param>
        /// <param name="fillRule">Backend-neutral fill rule assigned to this path.</param>
        public GdiMapPath(GraphicsPath path, FillRule fillRule)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            FillRule = fillRule;
        }

        /// <summary>
        /// Gets the native GDI+ path used by <see cref="GdiMapRenderSurface"/>.
        /// </summary>
        public GraphicsPath Path { get; }

        /// <summary>
        /// Gets the screen-space bounds of the path.
        /// </summary>
        public RectangleF Bounds => Path.GetBounds();

        /// <summary>
        /// Gets the fill rule assigned when the path was built.
        /// </summary>
        public FillRule FillRule { get; }

        /// <summary>
        /// Gets the number of native path points.
        /// </summary>
        public int PointCount => Path.PointCount;

        /// <summary>
        /// Releases the wrapped <see cref="GraphicsPath"/>.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Path.Dispose();
        }
    }
}
