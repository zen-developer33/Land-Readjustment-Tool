using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using SkiaSharp;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// Builds SkiaSharp paths through the backend-neutral
    /// <see cref="IMapPathBuilder"/> contract.
    /// </summary>
    public sealed class SkiaMapPathBuilder : IMapPathBuilder
    {
        private SKPath? _path;
        private bool _hasCurrentPoint;

        /// <summary>
        /// Creates a Skia path builder with the requested fill rule.
        /// </summary>
        public SkiaMapPathBuilder(FillRule fillRule)
        {
            FillRule = fillRule;
            _path = new SKPath
            {
                FillType = SkiaMapPath.ToSkiaFillType(fillRule)
            };
        }

        /// <summary>
        /// Gets the fill rule assigned to the completed path.
        /// </summary>
        public FillRule FillRule { get; }

        /// <summary>
        /// Starts a new figure at the supplied point.
        /// </summary>
        public void MoveTo(PointF point)
        {
            SKPath path = GetPath();
            path.MoveTo(point.X, point.Y);
            _hasCurrentPoint = true;
        }

        /// <summary>
        /// Adds a line from the current point to the supplied point.
        /// </summary>
        public void LineTo(PointF point)
        {
            if (!_hasCurrentPoint)
            {
                MoveTo(point);
                return;
            }

            GetPath().LineTo(point.X, point.Y);
        }

        /// <summary>
        /// Adds a standalone line figure.
        /// </summary>
        public void AddLine(PointF start, PointF end)
        {
            MoveTo(start);
            LineTo(end);
        }

        /// <summary>
        /// Adds one closed polygon figure.
        /// </summary>
        public void AddPolygon(ReadOnlySpan<PointF> points)
        {
            if (points.Length < 3)
            {
                return;
            }

            MoveTo(points[0]);
            for (int i = 1; i < points.Length; i++)
            {
                LineTo(points[i]);
            }

            CloseFigure();
        }

        /// <summary>
        /// Adds a rectangle figure.
        /// </summary>
        public void AddRectangle(RectangleF rect)
        {
            if (IsValidRectangle(rect))
            {
                GetPath().AddRect(ToSkiaRect(rect));
                _hasCurrentPoint = false;
            }
        }

        /// <summary>
        /// Adds an ellipse figure.
        /// </summary>
        public void AddEllipse(RectangleF rect)
        {
            if (IsValidRectangle(rect))
            {
                GetPath().AddOval(ToSkiaRect(rect));
                _hasCurrentPoint = false;
            }
        }

        /// <summary>
        /// Adds an arc segment using screen-space bounds and degree angles.
        /// </summary>
        public void AddArc(RectangleF bounds, float startDeg, float sweepDeg)
        {
            if (!IsValidRectangle(bounds) ||
                !float.IsFinite(startDeg) ||
                !float.IsFinite(sweepDeg) ||
                Math.Abs(sweepDeg) < 0.001f)
            {
                return;
            }

            GetPath().ArcTo(ToSkiaRect(bounds), startDeg, sweepDeg, forceMoveTo: !_hasCurrentPoint);
            _hasCurrentPoint = true;
        }

        /// <summary>
        /// Closes the current figure.
        /// </summary>
        public void CloseFigure()
        {
            GetPath().Close();
            _hasCurrentPoint = false;
        }

        /// <summary>
        /// Transfers ownership of the completed path to a
        /// <see cref="SkiaMapPath"/>.
        /// </summary>
        public IMapPath Build()
        {
            SKPath path = GetPath();
            _path = null;
            return new SkiaMapPath(path, FillRule);
        }

        /// <summary>
        /// Returns the mutable native path while guarding against use after
        /// <see cref="Build"/>.
        /// </summary>
        private SKPath GetPath() =>
            _path ?? throw new InvalidOperationException("The path has already been built.");

        /// <summary>
        /// Converts a drawing rectangle to a Skia rectangle.
        /// </summary>
        internal static SKRect ToSkiaRect(RectangleF rect) =>
            new(rect.Left, rect.Top, rect.Right, rect.Bottom);

        /// <summary>
        /// Checks whether a rectangle is finite and drawable.
        /// </summary>
        private static bool IsValidRectangle(RectangleF rectangle) =>
            float.IsFinite(rectangle.Left) &&
            float.IsFinite(rectangle.Top) &&
            float.IsFinite(rectangle.Right) &&
            float.IsFinite(rectangle.Bottom) &&
            rectangle.Width > 0.0f &&
            rectangle.Height > 0.0f;
    }
}
