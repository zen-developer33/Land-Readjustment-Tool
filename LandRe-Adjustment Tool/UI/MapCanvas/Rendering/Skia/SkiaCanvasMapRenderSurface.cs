using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Threading;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Diagnostics;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using SkiaSharp;

#pragma warning disable CS0618

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// Direct Skia render surface for an already-presented <see cref="SKCanvas"/>.
    /// Unlike the WinForms bridge surfaces, this never reads pixels back to GDI.
    /// </summary>
    public sealed class SkiaCanvasMapRenderSurface : IMapRenderSurface
    {
        private readonly SKCanvas _canvas;
        private readonly Dictionary<StrokePaintKey, SKPaint> _strokePaintCache = new();
        private readonly Dictionary<FillPaintKey, SKPaint> _fillPaintCache = new();
        private readonly Dictionary<TextPaintKey, SKPaint> _textPaintCache = new();
        private readonly Dictionary<TextKey, SKTypeface> _typefaceCache = new();
        private static readonly ConditionalWeakTable<Image, CachedSkiaImage> ImageCache = new();
        // The cap must be large enough to cache the full-canvas vector/raster cache
        // frames — those are the images presented on every frame, so excluding them
        // (the old 512 cap did) meant re-uploading the whole canvas to the GPU each
        // paint. Only images that explicitly opt in (AllowSkiaImageCache) and are
        // immutable per instance reach this check, so a generous cap is safe.
        private const int MaximumCachedImageDimension = 8192;
        private const long MaximumCachedImagePixels = (long)MaximumCachedImageDimension * MaximumCachedImageDimension;
        private static long _imageCacheHits;
        private static long _imageCacheMisses;
        private static long _uncachedImages;
        private RenderQuality _quality = RenderQuality.VectorHighQuality;
        private bool _disposed;
        private const int MaxPaintCacheEntries = 512;

        public SkiaCanvasMapRenderSurface(SKCanvas canvas, Size pixelSize)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            PixelSize = new Size(Math.Max(1, pixelSize.Width), Math.Max(1, pixelSize.Height));
        }

        public Size PixelSize { get; }

        public static SkiaCanvasImageCacheStats SnapshotAndResetImageCacheStats()
        {
            long hits = Interlocked.Exchange(ref _imageCacheHits, 0);
            long misses = Interlocked.Exchange(ref _imageCacheMisses, 0);
            long uncached = Interlocked.Exchange(ref _uncachedImages, 0);
            return new SkiaCanvasImageCacheStats(hits, misses, uncached);
        }

        public void Clear(Color color) => _canvas.Clear(ToSkiaColor(color));

        public IDisposable SaveState() => new SavedSkiaState(_canvas);

        public void SetQuality(RenderQuality quality) => _quality = quality;

        public void ClipPath(IMapPath path)
        {
            using SKPath skiaPath = AsSkiaPath(path);
            _canvas.ClipPath(skiaPath, SKClipOperation.Intersect, IsAntialiasEnabled);
        }

        public IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding) =>
            new SkiaMapPathBuilder(fillRule);

        public void DrawLine(PointF a, PointF b, in StrokeStyle stroke)
        {
            if (!IsValidPoint(a) || !IsValidPoint(b))
            {
                return;
            }

            SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawLine(a.X, a.Y, b.X, b.Y, paint);
        }

        public void DrawPath(IMapPath path, in StrokeStyle stroke)
        {
            using SKPath skiaPath = AsSkiaPath(path);
            if (skiaPath.PointCount == 0)
            {
                return;
            }

            SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawPath(skiaPath, paint);
        }

        public void FillPath(IMapPath path, in FillStyle fill)
        {
            using SKPath skiaPath = AsSkiaPath(path);
            if (skiaPath.PointCount == 0)
            {
                return;
            }

            if (fill.Pattern == FillPatternKind.Solid)
            {
                SKPaint paint = CreateFillPaint(fill.Color);
                _canvas.DrawPath(skiaPath, paint);
                return;
            }

            DrawHatchedFill(skiaPath, fill);
        }

        public void DrawRectangle(RectangleF rect, in StrokeStyle stroke)
        {
            if (!IsValidRectangle(rect))
            {
                return;
            }

            SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawRect(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        public void FillRectangle(RectangleF rect, in FillStyle fill)
        {
            if (!IsValidRectangle(rect))
            {
                return;
            }

            SKPaint paint = CreateFillPaint(fill.Color);
            _canvas.DrawRect(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        public void DrawEllipse(RectangleF rect, in StrokeStyle stroke)
        {
            if (!IsValidRectangle(rect))
            {
                return;
            }

            SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawOval(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        public void FillEllipse(RectangleF rect, in FillStyle fill)
        {
            if (!IsValidRectangle(rect))
            {
                return;
            }

            SKPaint paint = CreateFillPaint(fill.Color);
            _canvas.DrawOval(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        public void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke)
        {
            if (!IsValidRectangle(rect) ||
                !float.IsFinite(startDeg) ||
                !float.IsFinite(sweepDeg) ||
                Math.Abs(sweepDeg) < 0.001f)
            {
                return;
            }

            SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawArc(SkiaMapPathBuilder.ToSkiaRect(rect), startDeg, sweepDeg, false, paint);
        }

        public SizeF MeasureText(string text, in TextStyle style)
        {
            SKPaint paint = CreateTextPaint(style);
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

        public void DrawText(string text, RectangleF layout, in TextStyle style)
        {
            if (!IsValidRectangle(layout))
            {
                return;
            }

            SKPaint paint = CreateTextPaint(style);
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

        public void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style)
        {
            if (!IsValidRectangle(dest))
            {
                return;
            }

            using SkiaImageLease imageLease = AsSkiaImage(image);
            SKRect source = src.HasValue
                ? SkiaMapPathBuilder.ToSkiaRect(src.Value)
                : new SKRect(0, 0, imageLease.Width, imageLease.Height);
            using SKPaint paint = CreateImagePaint(style);
            _canvas.DrawImage(imageLease.Image, source, SkiaMapPathBuilder.ToSkiaRect(dest), paint);
        }

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

            using SkiaImageLease imageLease = AsSkiaImage(image);
            SKRect source = SkiaMapPathBuilder.ToSkiaRect(src);
            using SKPaint paint = CreateImagePaint(style);
            using IDisposable state = SaveState();
            SKMatrix matrix = CreateSourceToParallelogramMatrix(src, upperLeft, upperRight, lowerLeft);
            _canvas.Concat(in matrix);
            _canvas.DrawImage(imageLease.Image, source, source, paint);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DisposePaintCache(_strokePaintCache);
            DisposePaintCache(_fillPaintCache);
            DisposePaintCache(_textPaintCache);

            foreach (SKTypeface typeface in _typefaceCache.Values)
            {
                typeface.Dispose();
            }

            _typefaceCache.Clear();
        }

        private bool IsAntialiasEnabled =>
            _quality != RenderQuality.VectorHighSpeed &&
            _quality != RenderQuality.RasterHighSpeed;

        private SKPaint CreateStrokePaint(in StrokeStyle style)
        {
            float width = Math.Max(0.1f, style.Width);
            float dashScale = Math.Clamp(style.DashScale, 0.1f, 100.0f);
            StrokePaintKey key = new(
                style.Color.ToArgb(),
                width,
                style.DashPattern,
                dashScale,
                style.Cap,
                style.Join,
                IsAntialiasEnabled);

            if (_strokePaintCache.TryGetValue(key, out SKPaint? cached))
            {
                return cached;
            }

            EnsurePaintCacheCapacity(_strokePaintCache, "stroke");
            SKPaint paint = new()
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSkiaColor(style.Color),
                StrokeWidth = width,
                StrokeCap = ToSkiaStrokeCap(style.Cap),
                StrokeJoin = ToSkiaStrokeJoin(style.Join),
                IsAntialias = IsAntialiasEnabled
            };
            paint.PathEffect = CreateDashEffect(style.DashPattern, dashScale, width);
            _strokePaintCache[key] = paint;
            return paint;
        }

        private SKPaint CreateFillPaint(Color color)
        {
            FillPaintKey key = new(color.ToArgb(), IsAntialiasEnabled);
            if (_fillPaintCache.TryGetValue(key, out SKPaint? cached))
            {
                return cached;
            }

            EnsurePaintCacheCapacity(_fillPaintCache, "fill");
            SKPaint paint = new()
            {
                Style = SKPaintStyle.Fill,
                Color = ToSkiaColor(color),
                IsAntialias = IsAntialiasEnabled
            };
            _fillPaintCache[key] = paint;
            return paint;
        }

        private SKPaint CreateTextPaint(in TextStyle style)
        {
            string family = ResolveFontFamily(style.FontFamily);
            float size = Math.Max(1.0f, style.SizePx);
            TextPaintKey key = new(
                family,
                size,
                style.Color.ToArgb(),
                style.Bold,
                style.HorizontalAlign,
                IsAntialiasEnabled);

            if (_textPaintCache.TryGetValue(key, out SKPaint? cached))
            {
                return cached;
            }

            EnsurePaintCacheCapacity(_textPaintCache, "text");
            SKPaint paint = new()
            {
                Style = SKPaintStyle.Fill,
                Color = ToSkiaColor(style.Color),
                IsAntialias = IsAntialiasEnabled,
                TextSize = size,
                Typeface = GetTypeface(family, style.Bold),
                TextAlign = ToSkiaTextAlign(style.HorizontalAlign)
            };
            _textPaintCache[key] = paint;
            return paint;
        }

        private static SKPaint CreateImagePaint(in ImageStyle style) =>
            new()
            {
                IsAntialias = style.Interpolation == ImageInterpolation.HighQuality,
                FilterQuality = style.Interpolation == ImageInterpolation.HighQuality
                    ? SKFilterQuality.High
                    : SKFilterQuality.None,
                Color = SKColors.White.WithAlpha((byte)(Math.Clamp(style.Opacity, 0.0f, 1.0f) * 255.0f))
            };

        private void DrawHatchedFill(SKPath path, in FillStyle fill)
        {
            using IDisposable state = SaveState();
            if (fill.Color.A > 0)
            {
                SKPaint background = CreateFillPaint(fill.Color);
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

        private static SKPath AsSkiaPath(IMapPath path)
        {
            if (path is SkiaMapPath skiaPath)
            {
                return new SKPath(skiaPath.Path);
            }

            if (path is GdiMapPath gdiPath)
            {
                RenderBackendTelemetry.RecordGdiPathFallback();
                return ConvertGdiPath(gdiPath.Path, gdiPath.FillRule);
            }

            throw new ArgumentException("Path type not supported by Skia canvas surface.", nameof(path));
        }

        private static SkiaImageLease AsSkiaImage(IMapImage image)
        {
            if (image is GdiMapImage gdiImage)
            {
                if (CanCacheSkiaImage(gdiImage))
                {
                    CachedSkiaImage cached;
                    if (ImageCache.TryGetValue(gdiImage.Image, out CachedSkiaImage? existing))
                    {
                        Interlocked.Increment(ref _imageCacheHits);
                        cached = existing;
                    }
                    else
                    {
                        Interlocked.Increment(ref _imageCacheMisses);
                        cached = ImageCache.GetValue(
                            gdiImage.Image,
                            static source => new CachedSkiaImage(CopyBitmapToSkia(source)));
                    }

                    return SkiaImageLease.Borrow(cached);
                }

                Interlocked.Increment(ref _uncachedImages);
                return SkiaImageLease.Own(CopyBitmapToSkia(gdiImage.Image));
            }

            throw new ArgumentException("Image type not supported by Skia canvas surface.", nameof(image));
        }

        private static bool CanCacheSkiaImage(GdiMapImage image)
        {
            if (!image.AllowSkiaImageCache ||
                image.PixelSize.Width <= 0 ||
                image.PixelSize.Height <= 0 ||
                image.PixelSize.Width > MaximumCachedImageDimension ||
                image.PixelSize.Height > MaximumCachedImageDimension)
            {
                return false;
            }

            long pixels = (long)image.PixelSize.Width * image.PixelSize.Height;
            return pixels <= MaximumCachedImagePixels;
        }

        private static SKPath ConvertGdiPath(GraphicsPath source, FillRule fillRule)
        {
            SKPath path = new() { FillType = SkiaMapPath.ToSkiaFillType(fillRule) };
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
                    case PathPointType.Bezier3 when i + 2 < points.Length:
                        path.CubicTo(
                            point.X,
                            point.Y,
                            points[i + 1].X,
                            points[i + 1].Y,
                            points[i + 2].X,
                            points[i + 2].Y);
                        close = (types[i + 2] & (byte)PathPointType.CloseSubpath) != 0;
                        i += 2;
                        break;
                }

                if (close)
                {
                    path.Close();
                }
            }

            return path;
        }

        private static SKBitmap CopyBitmapToSkia(Image image)
        {
            using Bitmap bitmap = image is Bitmap existing
                ? new Bitmap(existing)
                : new Bitmap(image);
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);
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
            return new SKMatrix
            {
                ScaleX = scaleX,
                SkewX = skewX,
                TransX = upperLeft.X - scaleX * source.Left - skewX * source.Top,
                SkewY = skewY,
                ScaleY = scaleY,
                TransY = upperLeft.Y - skewY * source.Left - scaleY * source.Top,
                Persp2 = 1.0f
            };
        }

        private SKTypeface GetTypeface(string family, bool bold)
        {
            TextKey key = new(family, bold);
            if (_typefaceCache.TryGetValue(key, out SKTypeface? cached))
            {
                return cached;
            }

            SKTypeface typeface = SKTypeface.FromFamilyName(
                family,
                bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);
            _typefaceCache[key] = typeface;
            return typeface;
        }

        private static SKPathEffect? CreateDashEffect(DashPatternKind dashPattern, float scale, float strokeWidth)
        {
            float effectiveScale = scale * Math.Max(0.1f, strokeWidth);
            float[]? intervals = dashPattern switch
            {
                DashPatternKind.Dashed => [4f * effectiveScale, 2f * effectiveScale],
                DashPatternKind.Dotted => [0.1f * Math.Max(0.1f, strokeWidth), Math.Max(1.5f, 2f * scale) * Math.Max(0.1f, strokeWidth)],
                DashPatternKind.DashDot => [4f * effectiveScale, 2f * effectiveScale, 1f * effectiveScale, 2f * effectiveScale],
                DashPatternKind.DashDoubleDot => [4f * effectiveScale, 2f * effectiveScale, 1f * effectiveScale, 2f * effectiveScale, 1f * effectiveScale, 2f * effectiveScale],
                DashPatternKind.CenterLine => [8f * effectiveScale, 3f * effectiveScale, 2f * effectiveScale, 3f * effectiveScale],
                _ => null
            };

            return intervals == null ? null : SKPathEffect.CreateDash(intervals, 0.0f);
        }

        private void EnsurePaintCacheCapacity<TKey>(Dictionary<TKey, SKPaint> cache, string cacheName)
            where TKey : notnull
        {
            if (cache.Count < MaxPaintCacheEntries)
            {
                return;
            }

            Debug.WriteLine(
                $"Skia canvas {cacheName} paint cache exceeded {MaxPaintCacheEntries} entries; clearing cached paints.");
            DisposePaintCache(cache);
        }

        private static void DisposePaintCache<TKey>(Dictionary<TKey, SKPaint> cache)
            where TKey : notnull
        {
            foreach (SKPaint paint in cache.Values)
            {
                paint.Dispose();
            }

            cache.Clear();
        }

        private static string ResolveFontFamily(string? fontFamily) =>
            string.IsNullOrWhiteSpace(fontFamily) ? "Segoe UI" : fontFamily.Trim();

        private static string[] SplitLines(string text) =>
            string.IsNullOrEmpty(text)
                ? [" "]
                : text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

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

        private static float CalculateTextX(RectangleF layout, TextAlign alignment) =>
            alignment switch
            {
                TextAlign.Center => layout.Left + layout.Width / 2.0f,
                TextAlign.Far => layout.Right,
                _ => layout.Left
            };

        private static SKColor ToSkiaColor(Color color) => new(color.R, color.G, color.B, color.A);

        private static SKStrokeCap ToSkiaStrokeCap(LineCapKind cap) =>
            cap switch
            {
                LineCapKind.Flat => SKStrokeCap.Butt,
                LineCapKind.Square => SKStrokeCap.Square,
                _ => SKStrokeCap.Round
            };

        private static SKStrokeJoin ToSkiaStrokeJoin(LineJoinKind join) =>
            join switch
            {
                LineJoinKind.Miter => SKStrokeJoin.Miter,
                LineJoinKind.Bevel => SKStrokeJoin.Bevel,
                _ => SKStrokeJoin.Round
            };

        private static SKTextAlign ToSkiaTextAlign(TextAlign align) =>
            align switch
            {
                TextAlign.Center => SKTextAlign.Center,
                TextAlign.Far => SKTextAlign.Right,
                _ => SKTextAlign.Left
            };

        private static bool IsValidPoint(PointF point) =>
            float.IsFinite(point.X) && float.IsFinite(point.Y);

        private static bool IsValidRectangle(RectangleF rectangle) =>
            float.IsFinite(rectangle.Left) &&
            float.IsFinite(rectangle.Top) &&
            float.IsFinite(rectangle.Right) &&
            float.IsFinite(rectangle.Bottom) &&
            rectangle.Width > 0.0f &&
            rectangle.Height > 0.0f;

        private sealed class SavedSkiaState : IDisposable
        {
            private readonly SKCanvas _canvas;
            private bool _disposed;

            public SavedSkiaState(SKCanvas canvas)
            {
                _canvas = canvas;
                _canvas.Save();
            }

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

        private readonly record struct TextKey(string Family, bool Bold);

        private readonly record struct StrokePaintKey(
            int ColorArgb,
            float Width,
            DashPatternKind DashPattern,
            float DashScale,
            LineCapKind Cap,
            LineJoinKind Join,
            bool AntiAlias);

        private readonly record struct FillPaintKey(int ColorArgb, bool AntiAlias);

        private readonly record struct TextPaintKey(
            string Family,
            float SizePx,
            int ColorArgb,
            bool Bold,
            TextAlign HorizontalAlign,
            bool AntiAlias);

        private sealed class CachedSkiaImage
        {
            public CachedSkiaImage(SKBitmap bitmap)
            {
                Bitmap = bitmap;
                Image = SKImage.FromBitmap(bitmap);
            }

            public SKBitmap Bitmap { get; }

            public SKImage Image { get; }

            ~CachedSkiaImage()
            {
                Image.Dispose();
                Bitmap.Dispose();
            }
        }

        private readonly struct SkiaImageLease : IDisposable
        {
            private readonly SKBitmap? _ownedBitmap;
            private readonly SKImage? _ownedImage;

            private SkiaImageLease(SKImage image, SKBitmap? ownedBitmap, SKImage? ownedImage)
            {
                Image = image;
                _ownedBitmap = ownedBitmap;
                _ownedImage = ownedImage;
            }

            public SKImage Image { get; }

            public int Width => Image.Width;

            public int Height => Image.Height;

            public static SkiaImageLease Borrow(CachedSkiaImage cached) =>
                new(cached.Image, ownedBitmap: null, ownedImage: null);

            public static SkiaImageLease Own(SKBitmap bitmap)
            {
                SKImage image = SKImage.FromBitmap(bitmap);
                return new SkiaImageLease(image, bitmap, image);
            }

            public void Dispose()
            {
                _ownedImage?.Dispose();
                _ownedBitmap?.Dispose();
            }
        }
    }

    public readonly record struct SkiaCanvasImageCacheStats(
        long Hits,
        long Misses,
        long UncachedImages);
}

#pragma warning restore CS0618
