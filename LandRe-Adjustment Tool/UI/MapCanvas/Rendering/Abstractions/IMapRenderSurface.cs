namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Backend-neutral drawing surface used by map renderers.
    /// </summary>
    /// <remarks>
    /// This is the central contract that lets renderers draw through GDI+,
    /// SkiaSharp, or future 2D backends without knowing the native drawing API.
    /// Renderer code should pass styles and geometry into this surface, while
    /// each backend translates them into its own native pens, paints, paths, and
    /// images.
    /// </remarks>
    public interface IMapRenderSurface : IDisposable
    {
        /// <summary>
        /// Gets the target surface size in physical pixels.
        /// </summary>
        Size PixelSize { get; }

        /// <summary>
        /// Clears the entire target surface with the supplied color.
        /// </summary>
        void Clear(Color color);

        /// <summary>
        /// Saves the current backend state and returns a disposable token that
        /// restores it.
        /// </summary>
        IDisposable SaveState();

        /// <summary>
        /// Applies a render-stage quality preset to the native drawing surface.
        /// </summary>
        void SetQuality(RenderQuality quality);

        /// <summary>
        /// Intersects the current clip region with the supplied path.
        /// </summary>
        void ClipPath(IMapPath path);

        /// <summary>
        /// Creates a path builder owned by this backend.
        /// </summary>
        IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding);

        /// <summary>
        /// Draws a single screen-space line segment.
        /// </summary>
        void DrawLine(PointF a, PointF b, in StrokeStyle stroke);

        /// <summary>
        /// Strokes a backend-owned path.
        /// </summary>
        void DrawPath(IMapPath path, in StrokeStyle stroke);

        /// <summary>
        /// Fills a backend-owned path.
        /// </summary>
        void FillPath(IMapPath path, in FillStyle fill);

        /// <summary>
        /// Strokes a screen-space rectangle.
        /// </summary>
        void DrawRectangle(RectangleF rect, in StrokeStyle stroke);

        /// <summary>
        /// Fills a screen-space rectangle.
        /// </summary>
        void FillRectangle(RectangleF rect, in FillStyle fill);

        /// <summary>
        /// Strokes a screen-space ellipse.
        /// </summary>
        void DrawEllipse(RectangleF rect, in StrokeStyle stroke);

        /// <summary>
        /// Fills a screen-space ellipse.
        /// </summary>
        void FillEllipse(RectangleF rect, in FillStyle fill);

        /// <summary>
        /// Draws an arc using screen-space bounds and degree angles.
        /// </summary>
        void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke);

        /// <summary>
        /// Measures text using backend-specific text metrics for the supplied style.
        /// </summary>
        SizeF MeasureText(string text, in TextStyle style);

        /// <summary>
        /// Draws text inside the supplied screen-space layout rectangle.
        /// </summary>
        void DrawText(string text, RectangleF layout, in TextStyle style);

        /// <summary>
        /// Draws a backend-owned image to the destination rectangle.
        /// </summary>
        void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style);

        /// <summary>
        /// Draws a backend-owned image into a screen-space parallelogram.
        /// </summary>
        /// <remarks>
        /// The destination points follow the GDI+ convention: upper-left,
        /// upper-right, and lower-left. The fourth corner is implied by the
        /// parallelogram. This supports projected tile mesh rendering without
        /// leaking a native drawing API into raster layers.
        /// </remarks>
        void DrawImage(IMapImage image, ReadOnlySpan<PointF> destPoints, RectangleF src, in ImageStyle style);
    }
}
