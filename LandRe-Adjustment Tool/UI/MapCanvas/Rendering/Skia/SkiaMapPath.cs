using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using SkiaSharp;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// SkiaSharp implementation of <see cref="IMapPath"/> backed by an
    /// <see cref="SKPath"/>.
    /// </summary>
    public sealed class SkiaMapPath : IMapPath
    {
        private bool _disposed;

        /// <summary>
        /// Creates a backend-owned Skia path wrapper.
        /// </summary>
        public SkiaMapPath(SKPath path, FillRule fillRule)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            FillRule = fillRule;
            Path.FillType = ToSkiaFillType(fillRule);
        }

        /// <summary>
        /// Gets the native Skia path.
        /// </summary>
        public SKPath Path { get; }

        /// <summary>
        /// Gets the path bounds in screen coordinates.
        /// </summary>
        public RectangleF Bounds
        {
            get
            {
                SKRect bounds = Path.Bounds;
                return new RectangleF(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
            }
        }

        /// <summary>
        /// Gets the fill rule assigned when the path was created.
        /// </summary>
        public FillRule FillRule { get; }

        /// <summary>
        /// Gets the number of points in the Skia path.
        /// </summary>
        public int PointCount => Path.PointCount;

        /// <summary>
        /// Releases the native Skia path.
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

        /// <summary>
        /// Converts the neutral fill rule into Skia fill semantics.
        /// </summary>
        internal static SKPathFillType ToSkiaFillType(FillRule fillRule) =>
            fillRule == FillRule.Alternate ? SKPathFillType.EvenOdd : SKPathFillType.Winding;
    }
}
