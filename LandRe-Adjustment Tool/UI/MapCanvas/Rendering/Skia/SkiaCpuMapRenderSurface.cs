using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Diagnostics;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using SkiaSharp;

#pragma warning disable CS0618 // SkiaSharp keeps these paint/text APIs available across supported target frameworks.

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// CPU raster SkiaSharp implementation of <see cref="IMapRenderSurface"/>.
    /// </summary>
    /// <remarks>
    /// The current WinForms host supplies a <see cref="Graphics"/> target. This
    /// adapter draws into a locked 32-bit bitmap using Skia CPU rasterization,
    /// then composites that bitmap back to the supplied target when disposed.
    /// It intentionally does not create or use any GPU context.
    /// </remarks>
    public sealed class SkiaCpuMapRenderSurface : IMapRenderSurface
    {
        private readonly Graphics? _targetGraphics;
        private readonly bool _ownsGraphics;
        private readonly bool _ownsBitmap;
        private readonly Bitmap _targetBitmap;
        private readonly BitmapData _bitmapData;
        private readonly SKSurface _surface;
        private readonly SKCanvas _canvas;
        private readonly Dictionary<TextKey, SKTypeface> _typefaceCache = new();
        private bool _disposed;
        private RenderQuality _quality = RenderQuality.VectorHighQuality;
        private readonly double _createMs;

        /// <summary>
        /// Creates a CPU Skia surface for an existing WinForms graphics target,
        /// allocating its own backing bitmap.
        /// </summary>
        public SkiaCpuMapRenderSurface(Graphics graphics, Size? pixelSize = null, bool ownsGraphics = false)
        {
            long t0 = Stopwatch.GetTimestamp();
            _targetGraphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _ownsGraphics = ownsGraphics;
            _ownsBitmap = true;
            PixelSize = ClampPixelSize(pixelSize ?? ResolvePixelSize(graphics));
            _targetBitmap = new Bitmap(PixelSize.Width, PixelSize.Height, PixelFormat.Format32bppPArgb);
            Rectangle lockRect = new(0, 0, PixelSize.Width, PixelSize.Height);
            _bitmapData = _targetBitmap.LockBits(lockRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            SKImageInfo info = new(PixelSize.Width, PixelSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info, _bitmapData.Scan0, _bitmapData.Stride)
                ?? throw new InvalidOperationException("Unable to create the Skia CPU surface.");
            _canvas = _surface.Canvas;
            _createMs = ElapsedMs(t0);
        }

        /// <summary>
        /// Creates a CPU Skia surface that wraps a caller-owned backing bitmap.
        /// The bitmap must be <see cref="PixelFormat.Format32bppPArgb"/> and exactly
        /// the size that should be drawn into.  The bitmap is cleared to transparent
        /// on construction so stale content from a prior pass does not bleed through.
        /// </summary>
        public SkiaCpuMapRenderSurface(Bitmap backingBitmap, Graphics targetGraphics, bool ownsGraphics = false)
        {
            ArgumentNullException.ThrowIfNull(backingBitmap);
            _targetGraphics = targetGraphics ?? throw new ArgumentNullException(nameof(targetGraphics));
            _ownsGraphics = ownsGraphics;
            _ownsBitmap = false;
            _targetBitmap = backingBitmap;
            PixelSize = ClampPixelSize(new Size(backingBitmap.Width, backingBitmap.Height));
            Rectangle lockRect = new(0, 0, PixelSize.Width, PixelSize.Height);
            _bitmapData = _targetBitmap.LockBits(lockRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            SKImageInfo info = new(PixelSize.Width, PixelSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info, _bitmapData.Scan0, _bitmapData.Stride)
                ?? throw new InvalidOperationException("Unable to create the Skia CPU surface.");
            _canvas = _surface.Canvas;
            _canvas.Clear(SKColors.Transparent);
        }

        /// <summary>
        /// Creates a CPU Skia surface that rasterizes directly into the supplied
        /// bitmap's pixels with NO final composite blit. Use this for offscreen
        /// cache bitmaps that are themselves the final target — it avoids the
        /// GPU read-back and per-thread GL context that the GPU surface would
        /// incur when rendering a cache on a background thread.
        /// </summary>
        public static SkiaCpuMapRenderSurface CreateInPlace(Bitmap targetBitmap)
        {
            ArgumentNullException.ThrowIfNull(targetBitmap);
            return new SkiaCpuMapRenderSurface(targetBitmap);
        }

        private SkiaCpuMapRenderSurface(Bitmap targetBitmap)
        {
            _targetGraphics = null;
            _ownsGraphics = false;
            _ownsBitmap = false;
            _targetBitmap = targetBitmap;
            PixelSize = ClampPixelSize(new Size(targetBitmap.Width, targetBitmap.Height));
            Rectangle lockRect = new(0, 0, PixelSize.Width, PixelSize.Height);
            _bitmapData = _targetBitmap.LockBits(lockRect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            SKImageInfo info = new(PixelSize.Width, PixelSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info, _bitmapData.Scan0, _bitmapData.Stride)
                ?? throw new InvalidOperationException("Unable to create the Skia CPU surface.");
            _canvas = _surface.Canvas;
            _canvas.Clear(SKColors.Transparent);
        }

        /// <summary>
        /// Gets the target surface size in physical pixels.
        /// </summary>
        public Size PixelSize { get; }

        /// <summary>
        /// Clears the Skia raster target.
        /// </summary>
        public void Clear(Color color) => _canvas.Clear(ToSkiaColor(color));

        /// <summary>
        /// Saves the current Skia canvas state.
        /// </summary>
        public IDisposable SaveState() => new SavedSkiaState(_canvas);

        /// <summary>
        /// Applies a drawing quality preset used by subsequent Skia paint
        /// creation.
        /// </summary>
        public void SetQuality(RenderQuality quality)
        {
            _quality = quality;
        }

        /// <summary>
        /// Intersects the current clip with the supplied path.
        /// </summary>
        public void ClipPath(IMapPath path)
        {
            using SKPath skiaPath = AsSkiaPath(path);
            _canvas.ClipPath(skiaPath, SKClipOperation.Intersect, IsAntialiasEnabled);
        }

        /// <summary>
        /// Creates a Skia path builder.
        /// </summary>
        public IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding) =>
            new SkiaMapPathBuilder(fillRule);

        /// <summary>
        /// Draws a single screen-space line.
        /// </summary>
        public void DrawLine(PointF a, PointF b, in StrokeStyle stroke)
        {
            if (!IsValidPoint(a) || !IsValidPoint(b))
            {
                return;
            }

            using SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawLine(a.X, a.Y, b.X, b.Y, paint);
        }

        /// <summary>
        /// Strokes a path.
        /// </summary>
        public void DrawPath(IMapPath path, in StrokeStyle stroke)
        {
            using SKPath skiaPath = AsSkiaPath(path);
            if (skiaPath.PointCount == 0)
            {
                return;
            }

            using SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawPath(skiaPath, paint);
        }

        /// <summary>
        /// Fills a path.
        /// </summary>
        public void FillPath(IMapPath path, in FillStyle fill)
        {
            using SKPath skiaPath = AsSkiaPath(path);
            if (skiaPath.PointCount == 0)
            {
                return;
            }

            if (fill.Pattern == FillPatternKind.Solid)
            {
                using SKPaint paint = CreateFillPaint(fill.Color);
                _canvas.DrawPath(skiaPath, paint);
                return;
            }

            DrawHatchedFill(skiaPath, fill);
        }

        /// <summary>
        /// Draws a rectangle outline.
        /// </summary>
        public void DrawRectangle(RectangleF rect, in StrokeStyle stroke)
        {
            if (IsValidRectangle(rect))
            {
                using SKPaint paint = CreateStrokePaint(stroke);
                _canvas.DrawRect(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
            }
        }

        /// <summary>
        /// Fills a rectangle.
        /// </summary>
        public void FillRectangle(RectangleF rect, in FillStyle fill)
        {
            if (IsValidRectangle(rect))
            {
                using SKPaint paint = CreateFillPaint(fill.Color);
                _canvas.DrawRect(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
            }
        }

        /// <summary>
        /// Draws an ellipse outline.
        /// </summary>
        public void DrawEllipse(RectangleF rect, in StrokeStyle stroke)
        {
            if (IsValidRectangle(rect))
            {
                using SKPaint paint = CreateStrokePaint(stroke);
                _canvas.DrawOval(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
            }
        }

        /// <summary>
        /// Fills an ellipse.
        /// </summary>
        public void FillEllipse(RectangleF rect, in FillStyle fill)
        {
            if (IsValidRectangle(rect))
            {
                using SKPaint paint = CreateFillPaint(fill.Color);
                _canvas.DrawOval(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
            }
        }

        /// <summary>
        /// Draws an arc using degree angles.
        /// </summary>
        public void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke)
        {
            if (IsValidRectangle(rect) &&
                float.IsFinite(startDeg) &&
                float.IsFinite(sweepDeg) &&
                Math.Abs(sweepDeg) >= 0.001f)
            {
                using SKPaint paint = CreateStrokePaint(stroke);
                _canvas.DrawArc(SkiaMapPathBuilder.ToSkiaRect(rect), startDeg, sweepDeg, false, paint);
            }
        }

        /// <summary>
        /// Measures text using Skia font metrics.
        /// </summary>
        public SizeF MeasureText(string text, in TextStyle style)
        {
            using SKPaint paint = CreateTextPaint(style);
            string[] lines = SplitLines(text);
            float width = 0.0f;
            foreach (string line in lines)
            {
                width = Math.Max(width, paint.MeasureText(string.IsNullOrEmpty(line) ? " " : line));
            }

            SKFontMetrics metrics = paint.FontMetrics;
            float lineHeight = Math.Max(1.0f, metrics.Descent - metrics.Ascent + metrics.Leading);
            return new SizeF(Math.Max(1.0f, width), lineHeight * lines.Length);
        }

        /// <summary>
        /// Draws text inside a layout rectangle, optionally rotating around a
        /// requested origin.
        /// </summary>
        public void DrawText(string text, RectangleF layout, in TextStyle style)
        {
            if (!IsValidRectangle(layout))
            {
                return;
            }

            using SKPaint paint = CreateTextPaint(style);
            string[] lines = SplitLines(text);
            SKFontMetrics metrics = paint.FontMetrics;
            float lineHeight = Math.Max(1.0f, metrics.Descent - metrics.Ascent + metrics.Leading);
            float totalHeight = lineHeight * lines.Length;
            float startBaseline = CalculateFirstBaseline(layout, style, metrics, totalHeight, lineHeight);
            float x = CalculateTextX(layout, style.HorizontalAlign);

            using IDisposable state = SaveState();
            if (Math.Abs(style.RotationDegrees) >= 0.001f)
            {
                PointF origin = style.RotationOrigin
                    ?? new PointF(layout.X + layout.Width / 2.0f, layout.Y + layout.Height / 2.0f);
                _canvas.Translate(origin.X, origin.Y);
                _canvas.RotateDegrees(style.RotationDegrees);
                _canvas.Translate(-origin.X, -origin.Y);
            }

            for (int i = 0; i < lines.Length; i++)
            {
                _canvas.DrawText(lines[i], x, startBaseline + i * lineHeight, paint);
            }
        }

        /// <summary>
        /// Draws an image using optional source clipping, interpolation, and
        /// opacity.
        /// </summary>
        public void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style)
        {
            if (!IsValidRectangle(dest))
            {
                return;
            }

            using SKBitmap bitmap = AsSkiaBitmap(image);
            SKRect source = src.HasValue
                ? SkiaMapPathBuilder.ToSkiaRect(src.Value)
                : new SKRect(0, 0, bitmap.Width, bitmap.Height);
            SKRect destination = SkiaMapPathBuilder.ToSkiaRect(dest);
            using SKPaint paint = new()
            {
                IsAntialias = style.Interpolation == ImageInterpolation.HighQuality,
                FilterQuality = style.Interpolation == ImageInterpolation.HighQuality
                    ? SKFilterQuality.High
                    : SKFilterQuality.None,
                Color = SKColors.White.WithAlpha((byte)(Math.Clamp(style.Opacity, 0.0f, 1.0f) * 255.0f))
            };
            _canvas.DrawBitmap(bitmap, source, destination, paint);
        }

        /// <summary>
        /// Draws an image into a destination parallelogram using an affine Skia
        /// transform.
        /// </summary>
        public void DrawImage(IMapImage image, ReadOnlySpan<PointF> destPoints, RectangleF src, in ImageStyle style)
        {
            if (destPoints.Length < 3 || !IsValidRectangle(src))
            {
                return;
            }

            PointF upperLeft = destPoints[0];
            PointF upperRight = destPoints[1];
            PointF lowerLeft = destPoints[2];
            if (!IsValidPoint(upperLeft) ||
                !IsValidPoint(upperRight) ||
                !IsValidPoint(lowerLeft))
            {
                return;
            }

            using SKBitmap bitmap = AsSkiaBitmap(image);
            SKRect source = SkiaMapPathBuilder.ToSkiaRect(src);
            using SKPaint paint = CreateImagePaint(style);
            using IDisposable state = SaveState();

            SKMatrix matrix = CreateSourceToParallelogramMatrix(
                src,
                upperLeft,
                upperRight,
                lowerLeft);
            _canvas.Concat(in matrix);
            _canvas.DrawBitmap(bitmap, source, source, paint);
        }

        /// <summary>
        /// Flushes the CPU Skia surface and copies its bitmap into the wrapped
        /// WinForms graphics target.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _canvas.Flush();
            _surface.Dispose();
            _targetBitmap.UnlockBits(_bitmapData);

            // In-place mode (no target graphics): the bitmap IS the final target,
            // so there is nothing to composite — just unlock above.
            double blitMs = 0.0;
            if (_targetGraphics != null)
            {
                long tBlit = Stopwatch.GetTimestamp();
                _targetGraphics.DrawImageUnscaled(_targetBitmap, 0, 0);
                blitMs = ElapsedMs(tBlit);
            }

            if (_ownsBitmap)
            {
                _targetBitmap.Dispose();
            }

            foreach (SKTypeface typeface in _typefaceCache.Values)
            {
                typeface.Dispose();
            }

            _typefaceCache.Clear();
            if (_ownsGraphics && _targetGraphics != null)
            {
                _targetGraphics.Dispose();
            }

            // CPU surfaces draw straight into the backing bitmap, so there is no
            // GPU read-back step — readback is reported as zero.
            RenderBackendTelemetry.Record(MapRenderBackend.SkiaCpu, _createMs, readbackMs: 0.0, blitMs);
        }

        private static double ElapsedMs(long startTimestamp) =>
            (Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / Stopwatch.Frequency;

        /// <summary>
        /// Gets whether antialiasing should be enabled for the current quality.
        /// </summary>
        private bool IsAntialiasEnabled => _quality != RenderQuality.VectorHighSpeed &&
                                           _quality != RenderQuality.RasterHighSpeed;

        /// <summary>
        /// Creates a Skia stroke paint from a neutral stroke style.
        /// </summary>
        private SKPaint CreateStrokePaint(in StrokeStyle style)
        {
            SKPaint paint = new()
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSkiaColor(style.Color),
                StrokeWidth = Math.Max(0.1f, style.Width),
                StrokeCap = ToSkiaStrokeCap(style.Cap),
                StrokeJoin = ToSkiaStrokeJoin(style.Join),
                IsAntialias = IsAntialiasEnabled
            };
            paint.PathEffect = CreateDashEffect(style);
            return paint;
        }

        /// <summary>
        /// Creates a Skia fill paint for a solid color.
        /// </summary>
        private SKPaint CreateFillPaint(Color color) =>
            new()
            {
                Style = SKPaintStyle.Fill,
                Color = ToSkiaColor(color),
                IsAntialias = IsAntialiasEnabled
            };

        /// <summary>
        /// Creates a Skia text paint for the supplied text style.
        /// </summary>
        private SKPaint CreateTextPaint(in TextStyle style)
        {
            SKPaint paint = new()
            {
                Style = SKPaintStyle.Fill,
                Color = ToSkiaColor(style.Color),
                IsAntialias = IsAntialiasEnabled,
                TextSize = Math.Max(1.0f, style.SizePx),
                Typeface = GetTypeface(style),
                TextAlign = ToSkiaTextAlign(style.HorizontalAlign)
            };
            return paint;
        }

        /// <summary>
        /// Creates image paint for opacity and interpolation choices.
        /// </summary>
        private static SKPaint CreateImagePaint(in ImageStyle style) =>
            new()
            {
                IsAntialias = style.Interpolation == ImageInterpolation.HighQuality,
                FilterQuality = style.Interpolation == ImageInterpolation.HighQuality
                    ? SKFilterQuality.High
                    : SKFilterQuality.None,
                Color = SKColors.White.WithAlpha((byte)(Math.Clamp(style.Opacity, 0.0f, 1.0f) * 255.0f))
            };

        /// <summary>
        /// Draws a simple clipped hatch approximation for non-solid fills.
        /// </summary>
        private void DrawHatchedFill(SKPath path, in FillStyle fill)
        {
            using IDisposable state = SaveState();
            if (fill.Color.A > 0)
            {
                using SKPaint background = CreateFillPaint(fill.Color);
                _canvas.DrawPath(path, background);
            }

            _canvas.ClipPath(path, SKClipOperation.Intersect, IsAntialiasEnabled);
            SKRect bounds = path.Bounds;
            SKColor hatchColor = ToSkiaColor(fill.PatternColor.IsEmpty ? Color.Black : fill.PatternColor);
            float spacing = (float)Math.Clamp(8.0 * Math.Max(0.25, fill.PatternScale), 4.0, 64.0);
            using SKPaint hatch = new()
            {
                Style = SKPaintStyle.Stroke,
                Color = hatchColor,
                StrokeWidth = 1.0f,
                IsAntialias = IsAntialiasEnabled
            };

            float start = bounds.Left - bounds.Height - spacing;
            float end = bounds.Right + bounds.Height + spacing;
            for (float x = start; x <= end; x += spacing)
            {
                _canvas.DrawLine(x, bounds.Bottom + spacing, x + bounds.Height + spacing, bounds.Top - spacing, hatch);
            }
        }

        /// <summary>
        /// Converts a backend path, including legacy GDI paths, into a Skia path
        /// clone owned by the caller.
        /// </summary>
        private static SKPath AsSkiaPath(IMapPath path)
        {
            if (path is SkiaMapPath skiaPath)
            {
                return new SKPath(skiaPath.Path);
            }

            if (path is GdiMapPath gdiPath)
            {
                return ConvertGdiPath(gdiPath.Path, gdiPath.FillRule);
            }

            throw new ArgumentException("The supplied path cannot be drawn by the Skia CPU render surface.", nameof(path));
        }

        /// <summary>
        /// Builds an affine matrix that maps the source rectangle to a
        /// destination parallelogram.
        /// </summary>
        private static SKMatrix CreateSourceToParallelogramMatrix(
            RectangleF source,
            PointF upperLeft,
            PointF upperRight,
            PointF lowerLeft)
        {
            float scaleX = (upperRight.X - upperLeft.X) / source.Width;
            float skewY = (upperRight.Y - upperLeft.Y) / source.Width;
            float skewX = (lowerLeft.X - upperLeft.X) / source.Height;
            float scaleY = (lowerLeft.Y - upperLeft.Y) / source.Height;
            float translateX = upperLeft.X - scaleX * source.Left - skewX * source.Top;
            float translateY = upperLeft.Y - skewY * source.Left - scaleY * source.Top;

            return new SKMatrix
            {
                ScaleX = scaleX,
                SkewX = skewX,
                TransX = translateX,
                SkewY = skewY,
                ScaleY = scaleY,
                TransY = translateY,
                Persp0 = 0.0f,
                Persp1 = 0.0f,
                Persp2 = 1.0f
            };
        }

        /// <summary>
        /// Converts a backend image, including current GDI images, into an
        /// <see cref="SKBitmap"/> owned by the caller.
        /// </summary>
        private static SKBitmap AsSkiaBitmap(IMapImage image)
        {
            if (image is GdiMapImage gdiImage)
            {
                return CopyBitmapToSkia(gdiImage.Image);
            }

            throw new ArgumentException("The supplied image cannot be drawn by the Skia CPU render surface.", nameof(image));
        }

        /// <summary>
        /// Converts a GDI+ path into a Skia path.
        /// </summary>
        private static SKPath ConvertGdiPath(GraphicsPath source, FillRule fillRule)
        {
            SKPath path = new()
            {
                FillType = SkiaMapPath.ToSkiaFillType(fillRule)
            };
            PointF[] points = source.PathPoints;
            byte[] types = source.PathTypes;
            for (int i = 0; i < points.Length; i++)
            {
                PathPointType pointType = (PathPointType)(types[i] & (byte)PathPointType.PathTypeMask);
                bool close = (types[i] & (byte)PathPointType.CloseSubpath) != 0;
                PointF point = points[i];
                switch (pointType)
                {
                    case PathPointType.Start:
                        path.MoveTo(point.X, point.Y);
                        break;
                    case PathPointType.Line:
                        path.LineTo(point.X, point.Y);
                        break;
                    case PathPointType.Bezier3:
                        if (i + 2 < points.Length)
                        {
                            PointF c1 = points[i];
                            PointF c2 = points[i + 1];
                            PointF end = points[i + 2];
                            path.CubicTo(c1.X, c1.Y, c2.X, c2.Y, end.X, end.Y);
                            close = (types[i + 2] & (byte)PathPointType.CloseSubpath) != 0;
                            i += 2;
                        }
                        break;
                }

                if (close)
                {
                    path.Close();
                }
            }

            return path;
        }

        /// <summary>
        /// Copies a GDI+ image into an Skia bitmap.
        /// </summary>
        private static SKBitmap CopyBitmapToSkia(Image image)
        {
            using Bitmap bitmap = image is Bitmap existing
                ? new Bitmap(existing)
                : new Bitmap(image);
            Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            try
            {
                SKImageInfo info = new(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                SKBitmap skiaBitmap = new(info);
                skiaBitmap.InstallPixels(info, data.Scan0, data.Stride);
                SKBitmap copy = new(info);
                skiaBitmap.CopyTo(copy);
                return copy;
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// Gets or creates a Skia typeface for the supplied text style.
        /// </summary>
        private SKTypeface GetTypeface(in TextStyle style)
        {
            string family = string.IsNullOrWhiteSpace(style.FontFamily) ? "Segoe UI" : style.FontFamily;
            TextKey key = new(family, style.Bold);
            if (_typefaceCache.TryGetValue(key, out SKTypeface? cached))
            {
                return cached;
            }

            SKTypeface typeface = SKTypeface.FromFamilyName(
                family,
                style.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);
            _typefaceCache[key] = typeface;
            return typeface;
        }

        /// <summary>
        /// Creates a dash effect for a stroke style, or null for solid strokes.
        /// </summary>
        private static SKPathEffect? CreateDashEffect(in StrokeStyle style)
        {
            float scale = Math.Clamp(style.DashScale, 0.1f, 100.0f);
            float[]? intervals = style.DashPattern switch
            {
                DashPatternKind.Dashed => [4f * scale, 2f * scale],
                DashPatternKind.Dotted => [0.1f, Math.Max(1.5f, 2f * scale)],
                DashPatternKind.DashDot => [4f * scale, 2f * scale, 1f * scale, 2f * scale],
                DashPatternKind.DashDoubleDot => [4f * scale, 2f * scale, 1f * scale, 2f * scale, 1f * scale, 2f * scale],
                DashPatternKind.CenterLine => [8f * scale, 3f * scale, 2f * scale, 3f * scale],
                _ => null
            };

            return intervals == null ? null : SKPathEffect.CreateDash(intervals, 0.0f);
        }

        /// <summary>
        /// Splits text into drawable lines while preserving blank lines.
        /// </summary>
        private static string[] SplitLines(string text) =>
            string.IsNullOrEmpty(text)
                ? [" "]
                : text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        /// <summary>
        /// Calculates the first baseline for a potentially multiline text block.
        /// </summary>
        private static float CalculateFirstBaseline(
            RectangleF layout,
            in TextStyle style,
            in SKFontMetrics metrics,
            float totalHeight,
            float lineHeight)
        {
            float top = style.VerticalAlign switch
            {
                TextAlign.Center => layout.Top + (layout.Height - totalHeight) / 2.0f,
                TextAlign.Far => layout.Bottom - totalHeight,
                _ => layout.Top
            };

            return top - metrics.Ascent + Math.Max(0.0f, (lineHeight - (metrics.Descent - metrics.Ascent)) / 2.0f);
        }

        /// <summary>
        /// Calculates the Skia text x coordinate from neutral alignment.
        /// </summary>
        private static float CalculateTextX(RectangleF layout, TextAlign alignment) =>
            alignment switch
            {
                TextAlign.Center => layout.Left + layout.Width / 2.0f,
                TextAlign.Far => layout.Right,
                _ => layout.Left
            };

        /// <summary>
        /// Converts a drawing color into an Skia color.
        /// </summary>
        private static SKColor ToSkiaColor(Color color) =>
            new(color.R, color.G, color.B, color.A);

        /// <summary>
        /// Converts neutral line cap into Skia stroke cap.
        /// </summary>
        private static SKStrokeCap ToSkiaStrokeCap(LineCapKind cap) =>
            cap switch
            {
                LineCapKind.Flat => SKStrokeCap.Butt,
                LineCapKind.Square => SKStrokeCap.Square,
                _ => SKStrokeCap.Round
            };

        /// <summary>
        /// Converts neutral line join into Skia stroke join.
        /// </summary>
        private static SKStrokeJoin ToSkiaStrokeJoin(LineJoinKind join) =>
            join switch
            {
                LineJoinKind.Miter => SKStrokeJoin.Miter,
                LineJoinKind.Bevel => SKStrokeJoin.Bevel,
                _ => SKStrokeJoin.Round
            };

        /// <summary>
        /// Converts neutral text alignment into Skia text alignment.
        /// </summary>
        private static SKTextAlign ToSkiaTextAlign(TextAlign align) =>
            align switch
            {
                TextAlign.Center => SKTextAlign.Center,
                TextAlign.Far => SKTextAlign.Right,
                _ => SKTextAlign.Left
            };

        /// <summary>
        /// Resolves a best-effort pixel size for the target graphics.
        /// </summary>
        private static Size ResolvePixelSize(Graphics graphics)
        {
            RectangleF bounds = graphics.VisibleClipBounds;
            return ClampPixelSize(new Size(
                (int)Math.Ceiling(bounds.Width),
                (int)Math.Ceiling(bounds.Height)));
        }

        /// <summary>
        /// Ensures a pixel size is valid for bitmap allocation.
        /// </summary>
        private static Size ClampPixelSize(Size size) =>
            new(Math.Max(1, size.Width), Math.Max(1, size.Height));

        /// <summary>
        /// Checks whether a point can be drawn.
        /// </summary>
        private static bool IsValidPoint(PointF point) =>
            float.IsFinite(point.X) && float.IsFinite(point.Y);

        /// <summary>
        /// Checks whether a rectangle can be drawn.
        /// </summary>
        private static bool IsValidRectangle(RectangleF rectangle) =>
            float.IsFinite(rectangle.Left) &&
            float.IsFinite(rectangle.Top) &&
            float.IsFinite(rectangle.Right) &&
            float.IsFinite(rectangle.Bottom) &&
            rectangle.Width > 0.0f &&
            rectangle.Height > 0.0f;

        /// <summary>
        /// Disposable Skia save/restore state token.
        /// </summary>
        private sealed class SavedSkiaState : IDisposable
        {
            private readonly SKCanvas _canvas;
            private bool _disposed;

            /// <summary>
            /// Saves the current canvas state.
            /// </summary>
            public SavedSkiaState(SKCanvas canvas)
            {
                _canvas = canvas;
                _canvas.Save();
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
                _canvas.Restore();
            }
        }

        /// <summary>
        /// Cache key for Skia typefaces.
        /// </summary>
        private readonly record struct TextKey(string Family, bool Bold);
    }
}

#pragma warning restore CS0618
