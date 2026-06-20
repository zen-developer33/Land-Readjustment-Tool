namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Builds an <see cref="IMapPath"/> without exposing backend-specific path
    /// classes to shape or renderer code.
    /// </summary>
    /// <remarks>
    /// Shapes should use this builder instead of returning GDI+ or Skia path
    /// objects. This is the main escape route from the current
    /// <c>GraphicsPath</c> coupling.
    /// </remarks>
    public interface IMapPathBuilder
    {
        /// <summary>
        /// Gets the fill rule that will be assigned to the completed path.
        /// </summary>
        FillRule FillRule { get; }

        /// <summary>
        /// Starts a new figure at the given screen point.
        /// </summary>
        void MoveTo(PointF point);

        /// <summary>
        /// Adds a line from the current point to the given screen point.
        /// </summary>
        void LineTo(PointF point);

        /// <summary>
        /// Adds one independent line figure.
        /// </summary>
        void AddLine(PointF start, PointF end);

        /// <summary>
        /// Adds a closed polygon figure.
        /// </summary>
        void AddPolygon(ReadOnlySpan<PointF> points);

        /// <summary>
        /// Adds a rectangle figure.
        /// </summary>
        void AddRectangle(RectangleF rect);

        /// <summary>
        /// Adds an ellipse figure inside the supplied bounds.
        /// </summary>
        void AddEllipse(RectangleF rect);

        /// <summary>
        /// Adds an arc segment using screen-space bounds and degree angles.
        /// </summary>
        void AddArc(RectangleF bounds, float startDeg, float sweepDeg);

        /// <summary>
        /// Closes the current figure.
        /// </summary>
        void CloseFigure();

        /// <summary>
        /// Completes and returns the backend-owned path.
        /// </summary>
        IMapPath Build();
    }
}
