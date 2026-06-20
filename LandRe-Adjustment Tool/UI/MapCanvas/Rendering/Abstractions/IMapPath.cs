namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Represents a backend-owned vector path that can be stroked, filled, or
    /// used as a clip region.
    /// </summary>
    /// <remarks>
    /// Paths are intentionally opaque. The vector renderer can hold and pass
    /// them around, but only the backend that created a path should know whether
    /// it wraps a GDI+ <c>GraphicsPath</c>, a Skia <c>SKPath</c>, or another
    /// native representation.
    /// </remarks>
    public interface IMapPath : IDisposable
    {
        /// <summary>
        /// Gets the path bounds in screen/pixel coordinates.
        /// </summary>
        RectangleF Bounds { get; }

        /// <summary>
        /// Gets the fill rule assigned when the path was created.
        /// </summary>
        FillRule FillRule { get; }

        /// <summary>
        /// Gets the number of points in the path, used for quick empty-path checks.
        /// </summary>
        int PointCount { get; }
    }
}
