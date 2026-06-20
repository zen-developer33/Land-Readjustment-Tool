using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi
{
    /// <summary>
    /// Builds GDI+ paths behind the backend-neutral <see cref="IMapPathBuilder"/>
    /// contract.
    /// </summary>
    /// <remarks>
    /// Shape code should use this builder through the interface so it can later
    /// build Skia paths with the same geometry-writing logic.
    /// </remarks>
    public sealed class GdiMapPathBuilder : IMapPathBuilder
    {
        private GraphicsPath? _path;

        /// <summary>
        /// Creates a GDI+ path builder with the requested fill rule.
        /// </summary>
        public GdiMapPathBuilder(FillRule fillRule)
        {
            FillRule = fillRule;
            _path = new GraphicsPath { FillMode = ToGdiFillMode(fillRule) };
        }

        /// <summary>
        /// Gets the fill rule that will be applied to the completed path.
        /// </summary>
        public FillRule FillRule { get; }

        /// <summary>
        /// Starts a new figure at the supplied point.
        /// </summary>
        public void MoveTo(PointF point)
        {
            GraphicsPath path = GetPath();
            path.StartFigure();
            path.AddLine(point, point);
        }

        /// <summary>
        /// Adds a line from the current path point to the supplied point.
        /// </summary>
        public void LineTo(PointF point)
        {
            GraphicsPath path = GetPath();
            PointF start = path.PointCount > 0
                ? path.PathPoints[^1]
                : point;
            path.AddLine(start, point);
        }

        /// <summary>
        /// Adds a standalone line figure.
        /// </summary>
        public void AddLine(PointF start, PointF end)
        {
            GraphicsPath path = GetPath();
            path.StartFigure();
            path.AddLine(start, end);
        }

        /// <summary>
        /// Adds a closed polygon figure when at least three points are supplied.
        /// </summary>
        public void AddPolygon(ReadOnlySpan<PointF> points)
        {
            if (points.Length < 3)
            {
                return;
            }

            GetPath().AddPolygon(points.ToArray());
        }

        /// <summary>
        /// Adds a rectangle figure if the rectangle has finite positive size.
        /// </summary>
        public void AddRectangle(RectangleF rect)
        {
            if (IsValidRectangle(rect))
            {
                GetPath().AddRectangle(rect);
            }
        }

        /// <summary>
        /// Adds an ellipse figure if the bounds have finite positive size.
        /// </summary>
        public void AddEllipse(RectangleF rect)
        {
            if (IsValidRectangle(rect))
            {
                GetPath().AddEllipse(rect);
            }
        }

        /// <summary>
        /// Adds an arc segment if the bounds and angles are drawable by GDI+.
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

            GetPath().AddArc(bounds, startDeg, sweepDeg);
        }

        /// <summary>
        /// Closes the current path figure.
        /// </summary>
        public void CloseFigure()
        {
            GetPath().CloseFigure();
        }

        /// <summary>
        /// Transfers ownership of the native path into a <see cref="GdiMapPath"/>.
        /// </summary>
        public IMapPath Build()
        {
            GraphicsPath path = GetPath();
            _path = null;
            return new GdiMapPath(path, FillRule);
        }

        /// <summary>
        /// Returns the mutable native path while guarding against use after build.
        /// </summary>
        private GraphicsPath GetPath() =>
            _path ?? throw new InvalidOperationException("The path has already been built.");

        /// <summary>
        /// Converts the neutral fill rule into the GDI+ fill mode.
        /// </summary>
        private static FillMode ToGdiFillMode(FillRule fillRule) =>
            fillRule == FillRule.Alternate ? FillMode.Alternate : FillMode.Winding;

        /// <summary>
        /// Checks whether a rectangle is safe to pass to GDI+ drawing APIs.
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
