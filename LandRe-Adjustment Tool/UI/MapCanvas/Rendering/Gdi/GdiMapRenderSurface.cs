using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi
{
    /// <summary>
    /// GDI+ implementation of <see cref="IMapRenderSurface"/>.
    /// </summary>
    /// <remarks>
    /// This class is the compatibility bridge between the future backend-neutral
    /// renderer and the current Windows Forms <see cref="Graphics"/> pipeline.
    /// It translates RePlot style records into cached GDI+ pens, brushes, fonts,
    /// paths, and image calls.
    /// </remarks>
    public sealed class GdiMapRenderSurface : IMapRenderSurface
    {
        private readonly bool _ownsGraphics;
        private readonly BrushCache _brushCache = new();
        private readonly Dictionary<PenKey, Pen> _penCache = new();
        private readonly Dictionary<TextKey, Font> _fontCache = new();
        private bool _disposed;

        /// <summary>
        /// Creates a rendering surface around an existing GDI+ graphics object.
        /// </summary>
        /// <param name="graphics">Native GDI+ drawing surface.</param>
        /// <param name="pixelSize">
        /// Optional target size in pixels. When omitted, the visible clip bounds
        /// are used as a best-effort size.
        /// </param>
        /// <param name="ownsGraphics">
        /// Whether this wrapper should dispose the supplied graphics object.
        /// </param>
        public GdiMapRenderSurface(Graphics graphics, Size? pixelSize = null, bool ownsGraphics = false)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            PixelSize = pixelSize ?? ResolvePixelSize(graphics);
            _ownsGraphics = ownsGraphics;
        }

        /// <summary>
        /// Gets the native GDI+ graphics object used for all draw calls.
        /// </summary>
        public Graphics Graphics { get; }

        /// <summary>
        /// Gets the target surface size in pixels.
        /// </summary>
        public Size PixelSize { get; }

        /// <summary>
        /// Clears the native graphics surface.
        /// </summary>
        public void Clear(Color color) => Graphics.Clear(color);

        /// <summary>
        /// Saves the current GDI+ graphics state and returns a disposable restore token.
        /// </summary>
        public IDisposable SaveState() => new SavedGraphicsState(Graphics, Graphics.Save());

        /// <summary>
        /// Applies a named quality preset to the wrapped <see cref="Graphics"/>.
        /// </summary>
        public void SetQuality(RenderQuality quality)
        {
            switch (quality)
            {
                case RenderQuality.VectorHighQuality:
                    Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    Graphics.PixelOffsetMode = PixelOffsetMode.None;
                    Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    break;
                case RenderQuality.VectorHighSpeed:
                    Graphics.SmoothingMode = SmoothingMode.None;
                    Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    Graphics.PixelOffsetMode = PixelOffsetMode.None;
                    Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                    break;
                case RenderQuality.RasterHighSpeed:
                    Graphics.SmoothingMode = SmoothingMode.None;
                    Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                    Graphics.PixelOffsetMode = PixelOffsetMode.None;
                    Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    Graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;
                    break;
            }
        }

        /// <summary>
        /// Intersects the current clip region with a GDI-owned path.
        /// </summary>
        public void ClipPath(IMapPath path)
        {
            Graphics.SetClip(AsGdiPath(path).Path, CombineMode.Intersect);
        }

        /// <summary>
        /// Creates a GDI+ path builder for screen-space geometry.
        /// </summary>
        public IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding) =>
            new GdiMapPathBuilder(fillRule);

        /// <summary>
        /// Draws one finite screen-space line segment.
        /// </summary>
        public void DrawLine(PointF a, PointF b, in StrokeStyle stroke)
        {
            if (!IsValidPoint(a) || !IsValidPoint(b))
            {
                return;
            }

            Graphics.DrawLine(GetPen(stroke), a, b);
        }

        /// <summary>
        /// Strokes a GDI-owned path with a cached pen derived from the style.
        /// </summary>
        public void DrawPath(IMapPath path, in StrokeStyle stroke)
        {
            GdiMapPath gdiPath = AsGdiPath(path);
            if (gdiPath.PointCount > 0)
            {
                Graphics.DrawPath(GetPen(stroke), gdiPath.Path);
            }
        }

        /// <summary>
        /// Fills a GDI-owned path with a brush derived from the fill style.
        /// </summary>
        public void FillPath(IMapPath path, in FillStyle fill)
        {
            GdiMapPath gdiPath = AsGdiPath(path);
            if (gdiPath.PointCount > 0)
            {
                Graphics.FillPath(GetBrush(fill), gdiPath.Path);
            }
        }

        /// <summary>
        /// Draws a rectangle outline when the rectangle is valid for GDI+.
        /// </summary>
        public void DrawRectangle(RectangleF rect, in StrokeStyle stroke)
        {
            if (IsValidRectangle(rect))
            {
                Graphics.DrawRectangle(GetPen(stroke), rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        /// <summary>
        /// Fills a rectangle when the rectangle is valid for GDI+.
        /// </summary>
        public void FillRectangle(RectangleF rect, in FillStyle fill)
        {
            if (IsValidRectangle(rect))
            {
                Graphics.FillRectangle(GetBrush(fill), rect);
            }
        }

        /// <summary>
        /// Draws an ellipse outline inside the supplied screen-space bounds.
        /// </summary>
        public void DrawEllipse(RectangleF rect, in StrokeStyle stroke)
        {
            if (IsValidRectangle(rect))
            {
                Graphics.DrawEllipse(GetPen(stroke), rect);
            }
        }

        /// <summary>
        /// Fills an ellipse inside the supplied screen-space bounds.
        /// </summary>
        public void FillEllipse(RectangleF rect, in FillStyle fill)
        {
            if (IsValidRectangle(rect))
            {
                Graphics.FillEllipse(GetBrush(fill), rect);
            }
        }

        /// <summary>
        /// Draws an arc using GDI+ degree-angle semantics.
        /// </summary>
        public void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke)
        {
            if (IsValidRectangle(rect) &&
                float.IsFinite(startDeg) &&
                float.IsFinite(sweepDeg) &&
                Math.Abs(sweepDeg) >= 0.001f)
            {
                Graphics.DrawArc(GetPen(stroke), rect, startDeg, sweepDeg);
            }
        }

        /// <summary>
        /// Measures text with the cached GDI+ font and matching string format.
        /// </summary>
        public SizeF MeasureText(string text, in TextStyle style)
        {
            using StringFormat format = CreateStringFormat(style);
            return Graphics.MeasureString(string.IsNullOrEmpty(text) ? " " : text, GetFont(style), SizeF.Empty, format);
        }

        /// <summary>
        /// Draws text in a layout rectangle, optionally rotating around a requested origin.
        /// </summary>
        public void DrawText(string text, RectangleF layout, in TextStyle style)
        {
            if (!IsValidRectangle(layout))
            {
                return;
            }

            Font font = GetFont(style);
            Brush brush = _brushCache.GetSolid(style.Color);
            using StringFormat format = CreateStringFormat(style);

            if (Math.Abs(style.RotationDegrees) < 0.001f)
            {
                Graphics.DrawString(text, font, brush, layout, format);
                return;
            }

            using IDisposable state = SaveState();
            PointF origin = style.RotationOrigin
                ?? new PointF(layout.X + layout.Width / 2.0f, layout.Y + layout.Height / 2.0f);
            Graphics.TranslateTransform(origin.X, origin.Y);
            Graphics.RotateTransform(style.RotationDegrees);
            RectangleF local = new(
                layout.X - origin.X,
                layout.Y - origin.Y,
                layout.Width,
                layout.Height);
            Graphics.DrawString(text, font, brush, local, format);
        }

        /// <summary>
        /// Draws a GDI image using optional source clipping, interpolation, and opacity.
        /// </summary>
        public void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style)
        {
            GdiMapImage gdiImage = AsGdiImage(image);
            if (!IsValidRectangle(dest))
            {
                return;
            }

            using IDisposable state = SaveState();
            Graphics.InterpolationMode = style.Interpolation == ImageInterpolation.HighQuality
                ? InterpolationMode.HighQualityBicubic
                : InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.None;
            Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            RectangleF source = src ?? new RectangleF(0, 0, gdiImage.Image.Width, gdiImage.Image.Height);
            float opacity = Math.Clamp(style.Opacity, 0.0f, 1.0f);
            if (opacity >= 0.999f && !style.TileFlipXY)
            {
                Graphics.DrawImage(
                    gdiImage.Image,
                    dest,
                    source,
                    GraphicsUnit.Pixel);
                return;
            }

            using ImageAttributes attributes = CreateImageAttributes(opacity, style.TileFlipXY);
            Graphics.DrawImage(
                gdiImage.Image,
                Rectangle.Round(dest),
                source.X,
                source.Y,
                source.Width,
                source.Height,
                GraphicsUnit.Pixel,
                attributes);
        }

        /// <summary>
        /// Draws a GDI image into a destination parallelogram.
        /// </summary>
        public void DrawImage(IMapImage image, ReadOnlySpan<PointF> destPoints, RectangleF src, in ImageStyle style)
        {
            GdiMapImage gdiImage = AsGdiImage(image);
            if (destPoints.Length < 3 || !IsValidRectangle(src))
            {
                return;
            }

            PointF[] destination =
            [
                destPoints[0],
                destPoints[1],
                destPoints[2]
            ];

            if (!IsValidPoint(destination[0]) ||
                !IsValidPoint(destination[1]) ||
                !IsValidPoint(destination[2]))
            {
                return;
            }

            using IDisposable state = SaveState();
            Graphics.InterpolationMode = style.Interpolation == ImageInterpolation.HighQuality
                ? InterpolationMode.HighQualityBicubic
                : InterpolationMode.NearestNeighbor;
            Graphics.PixelOffsetMode = PixelOffsetMode.None;
            Graphics.CompositingQuality = CompositingQuality.HighSpeed;

            float opacity = Math.Clamp(style.Opacity, 0.0f, 1.0f);
            if (opacity >= 0.999f && !style.TileFlipXY)
            {
                Graphics.DrawImage(
                    gdiImage.Image,
                    destination,
                    src,
                    GraphicsUnit.Pixel);
                return;
            }

            using ImageAttributes attributes = CreateImageAttributes(opacity, style.TileFlipXY);
            Graphics.DrawImage(
                gdiImage.Image,
                destination,
                src,
                GraphicsUnit.Pixel,
                attributes);
        }

        /// <summary>
        /// Releases all cached GDI+ resources owned by this surface.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (Pen pen in _penCache.Values)
            {
                pen.Dispose();
            }

            foreach (Font font in _fontCache.Values)
            {
                font.Dispose();
            }

            _penCache.Clear();
            _fontCache.Clear();
            _brushCache.Dispose();

            if (_ownsGraphics)
            {
                Graphics.Dispose();
            }

            // GDI+ draws directly into the target graphics: no allocation, no
            // read-back, no composite blit. Recorded only so the surface count
            // can be compared against the Skia backends.
            Diagnostics.RenderBackendTelemetry.Record(
                Abstractions.MapRenderBackend.GdiPlus,
                createMs: 0.0,
                readbackMs: 0.0,
                blitMs: 0.0);
        }

        /// <summary>
        /// Gets or creates a cached GDI+ pen for the supplied stroke style.
        /// </summary>
        private Pen GetPen(in StrokeStyle style)
        {
            float width = Math.Max(0.1f, style.Width);
            float dashScale = Math.Clamp(style.DashScale, 0.1f, 100.0f);
            PenKey key = new(style.Color.ToArgb(), width, style.DashPattern, dashScale, style.Cap, style.Join);

            if (_penCache.TryGetValue(key, out Pen? cached))
            {
                return cached;
            }

            Pen pen = new(style.Color, width)
            {
                StartCap = ToGdiLineCap(style.Cap),
                EndCap = ToGdiLineCap(style.Cap),
                LineJoin = ToGdiLineJoin(style.Join)
            };
            ApplyDashPattern(pen, style.DashPattern, dashScale);
            _penCache[key] = pen;
            return pen;
        }

        /// <summary>
        /// Resolves the fill style into a cached GDI+ brush.
        /// </summary>
        private Brush GetBrush(in FillStyle style)
        {
            return style.Pattern switch
            {
                FillPatternKind.Hatch => _brushCache.GetHatch(
                    ResolveHatchStyle(style.PatternKey),
                    ResolvePatternColor(style),
                    style.Color),
                FillPatternKind.TextureHatch => _brushCache.GetTextureHatch(
                    style.PatternKey,
                    ResolvePatternColor(style),
                    style.PatternScale),
                _ => _brushCache.GetSolid(style.Color)
            };
        }

        /// <summary>
        /// Gets or creates a cached GDI+ font for the supplied text style.
        /// </summary>
        private Font GetFont(in TextStyle style)
        {
            string family = string.IsNullOrWhiteSpace(style.FontFamily) ? "Segoe UI" : style.FontFamily;
            float size = Math.Max(1.0f, style.SizePx);
            TextKey key = new(family, size, style.Bold);

            if (_fontCache.TryGetValue(key, out Font? cached))
            {
                return cached;
            }

            Font font = new(family, size, style.Bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);
            _fontCache[key] = font;
            return font;
        }

        /// <summary>
        /// Ensures that a path was created by the GDI backend before unwrapping it.
        /// </summary>
        private static GdiMapPath AsGdiPath(IMapPath path) =>
            path as GdiMapPath
            ?? throw new ArgumentException("The supplied path was not created by the GDI+ render surface.", nameof(path));

        /// <summary>
        /// Ensures that an image was created by the GDI backend before unwrapping it.
        /// </summary>
        private static GdiMapImage AsGdiImage(IMapImage image) =>
            image as GdiMapImage
            ?? throw new ArgumentException("The supplied image was not created by the GDI+ render surface.", nameof(image));

        /// <summary>
        /// Applies the backend-neutral dash pattern to a native GDI+ pen.
        /// </summary>
        private static void ApplyDashPattern(Pen pen, DashPatternKind dashPattern, float scale)
        {
            switch (dashPattern)
            {
                case DashPatternKind.Dashed:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale];
                    break;
                case DashPatternKind.Dotted:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.DashPattern = [0.1f, Math.Max(1.5f, 2f * scale)];
                    break;
                case DashPatternKind.DashDot:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case DashPatternKind.DashDoubleDot:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case DashPatternKind.CenterLine:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [8f * scale, 3f * scale, 2f * scale, 3f * scale];
                    break;
                default:
                    pen.DashStyle = DashStyle.Solid;
                    break;
            }
        }

        /// <summary>
        /// Converts a backend-neutral line cap into a GDI+ line cap.
        /// </summary>
        private static LineCap ToGdiLineCap(LineCapKind cap) =>
            cap switch
            {
                LineCapKind.Flat => LineCap.Flat,
                LineCapKind.Square => LineCap.Square,
                _ => LineCap.Round
            };

        /// <summary>
        /// Converts a backend-neutral line join into a GDI+ line join.
        /// </summary>
        private static LineJoin ToGdiLineJoin(LineJoinKind join) =>
            join switch
            {
                LineJoinKind.Miter => LineJoin.Miter,
                LineJoinKind.Bevel => LineJoin.Bevel,
                _ => LineJoin.Round
            };

        /// <summary>
        /// Maps common RePlot hatch keys to built-in GDI+ hatch styles.
        /// </summary>
        private static HatchStyle ResolveHatchStyle(string? patternKey)
        {
            string key = (patternKey ?? string.Empty)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant();

            return key switch
            {
                "CROSS" or "DIAGONALCROSS" => HatchStyle.DiagonalCross,
                "HORIZONTAL" => HatchStyle.Horizontal,
                "VERTICAL" => HatchStyle.Vertical,
                "FORWARDDIAGONAL" or "ANSI31" => HatchStyle.ForwardDiagonal,
                "BACKWARDDIAGONAL" => HatchStyle.BackwardDiagonal,
                "PERCENT20" => HatchStyle.Percent20,
                "PERCENT40" => HatchStyle.Percent40,
                "PERCENT50" => HatchStyle.Percent50,
                _ => HatchStyle.ForwardDiagonal
            };
        }

        /// <summary>
        /// Resolves the visible hatch foreground color, falling back to black.
        /// </summary>
        private static Color ResolvePatternColor(in FillStyle style) =>
            style.PatternColor.IsEmpty ? Color.Black : style.PatternColor;

        /// <summary>
        /// Creates a GDI+ string format matching the neutral text alignment.
        /// </summary>
        private static StringFormat CreateStringFormat(in TextStyle style)
        {
            return new StringFormat
            {
                Alignment = ToStringAlignment(style.HorizontalAlign),
                LineAlignment = ToStringAlignment(style.VerticalAlign),
                FormatFlags = StringFormatFlags.NoClip
            };
        }

        /// <summary>
        /// Converts neutral text alignment into GDI+ string alignment.
        /// </summary>
        private static StringAlignment ToStringAlignment(TextAlign align) =>
            align switch
            {
                TextAlign.Center => StringAlignment.Center,
                TextAlign.Far => StringAlignment.Far,
                _ => StringAlignment.Near
            };

        /// <summary>
        /// Creates image attributes that apply alpha opacity and optional tile
        /// edge wrapping during DrawImage.
        /// </summary>
        private static ImageAttributes CreateImageAttributes(float opacity, bool tileFlipXY)
        {
            ColorMatrix matrix = new()
            {
                Matrix33 = opacity
            };
            ImageAttributes attributes = new();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            if (tileFlipXY)
            {
                attributes.SetWrapMode(WrapMode.TileFlipXY);
            }

            return attributes;
        }

        /// <summary>
        /// Determines a best-effort pixel size for an externally supplied graphics object.
        /// </summary>
        private static Size ResolvePixelSize(Graphics graphics)
        {
            RectangleF bounds = graphics.VisibleClipBounds;
            int width = Math.Max(1, (int)Math.Ceiling(bounds.Width));
            int height = Math.Max(1, (int)Math.Ceiling(bounds.Height));
            return new Size(width, height);
        }

        /// <summary>
        /// Checks whether a point can be safely passed to GDI+ drawing APIs.
        /// </summary>
        private static bool IsValidPoint(PointF point) =>
            float.IsFinite(point.X) && float.IsFinite(point.Y);

        /// <summary>
        /// Checks whether a rectangle can be safely passed to GDI+ drawing APIs.
        /// </summary>
        private static bool IsValidRectangle(RectangleF rectangle) =>
            float.IsFinite(rectangle.Left) &&
            float.IsFinite(rectangle.Top) &&
            float.IsFinite(rectangle.Right) &&
            float.IsFinite(rectangle.Bottom) &&
            rectangle.Width > 0.0f &&
            rectangle.Height > 0.0f;

        /// <summary>
        /// Disposable token that restores a previously saved GDI+ graphics state.
        /// </summary>
        private sealed class SavedGraphicsState : IDisposable
        {
            private readonly Graphics _graphics;
            private readonly GraphicsState _state;
            private bool _disposed;

            /// <summary>
            /// Creates a restore token for a saved graphics state.
            /// </summary>
            public SavedGraphicsState(Graphics graphics, GraphicsState state)
            {
                _graphics = graphics;
                _state = state;
            }

            /// <summary>
            /// Restores the saved state once.
            /// </summary>
            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _graphics.Restore(_state);
            }
        }

        /// <summary>
        /// Cache key for native GDI+ pens.
        /// </summary>
        private readonly record struct PenKey(
            int ColorArgb,
            float Width,
            DashPatternKind DashPattern,
            float DashScale,
            LineCapKind Cap,
            LineJoinKind Join);

        /// <summary>
        /// Cache key for native GDI+ fonts.
        /// </summary>
        private readonly record struct TextKey(
            string FontFamily,
            float SizePx,
            bool Bold);
    }
}
