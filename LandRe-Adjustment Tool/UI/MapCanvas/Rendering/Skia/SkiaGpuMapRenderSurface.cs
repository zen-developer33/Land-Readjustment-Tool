using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Backends;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Diagnostics;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;
using SkiaSharp;

#pragma warning disable CS0618 // SkiaSharp keeps these paint/text APIs available across supported target frameworks.

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    /// <summary>
    /// GPU-accelerated SkiaSharp render surface. All geometry is rasterized on
    /// the GPU into an offscreen texture via the system OpenGL driver. On
    /// dispose the texture is read back to a GDI+ Bitmap and composited into
    /// the WinForms target using <see cref="Graphics.DrawImageUnscaled"/>.
    /// </summary>
    /// <remarks>
    /// The OpenGL context is managed per-thread by <see cref="SkiaGlContext"/>.
    /// Readback (GPU → CPU) happens once per surface lifetime, so the cost is
    /// dominated by driver rasterization time rather than bus bandwidth.
    /// </remarks>
    public sealed class SkiaGpuMapRenderSurface : IMapRenderSurface
    {
        private readonly Graphics _targetGraphics;
        private readonly bool _ownsGraphics;
        private readonly GRContext _grContext;
        private readonly SKSurface _surface;
        private readonly SKCanvas _canvas;
        private readonly Dictionary<TextKey, SKTypeface> _typefaceCache = new();
        private bool _disposed;
        private RenderQuality _quality = RenderQuality.VectorHighQuality;
        private readonly double _createMs;

        /// <summary>
        /// Creates a GPU render surface that targets the supplied WinForms graphics.
        /// </summary>
        public SkiaGpuMapRenderSurface(
            Graphics graphics,
            Size? pixelSize = null,
            bool ownsGraphics = false)
        {
            _targetGraphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _ownsGraphics = ownsGraphics;
            PixelSize = ClampPixelSize(pixelSize ?? ResolvePixelSize(graphics));

            long t0 = Stopwatch.GetTimestamp();

            SkiaGlContext glCtx = SkiaGlContext.GetOrCreateForCurrentThread();
            _grContext = glCtx.GrContext;

            // Use Rgba8888 on the GPU; pixels are read back as Bgra8888 (= Format32bppPArgb).
            // TopLeft origin means row 0 is the top row — correct for GDI+.
            SKImageInfo info = new(PixelSize.Width, PixelSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            _surface =
                SKSurface.Create(_grContext, budgeted: true, info, 0, GRSurfaceOrigin.TopLeft)
                ?? SKSurface.Create(_grContext, budgeted: true, info)
                ?? throw new InvalidOperationException("Failed to create GPU SKSurface.");

            _canvas = _surface.Canvas;
            _createMs = ElapsedMs(t0);
        }

        /// <inheritdoc/>
        public Size PixelSize { get; }

        /// <inheritdoc/>
        public void Clear(Color color) => _canvas.Clear(ToSkiaColor(color));

        /// <inheritdoc/>
        public IDisposable SaveState() => new SavedSkiaState(_canvas);

        /// <inheritdoc/>
        public void SetQuality(RenderQuality quality) => _quality = quality;

        /// <inheritdoc/>
        public void ClipPath(IMapPath path)
        {
            using SKPath skPath = AsSkiaPath(path);
            _canvas.ClipPath(skPath, SKClipOperation.Intersect, IsAntialiasEnabled);
        }

        /// <inheritdoc/>
        public IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding) =>
            new SkiaMapPathBuilder(fillRule);

        /// <inheritdoc/>
        public void DrawLine(PointF a, PointF b, in StrokeStyle stroke)
        {
            if (!IsValidPoint(a) || !IsValidPoint(b))
                return;

            using SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawLine(a.X, a.Y, b.X, b.Y, paint);
        }

        /// <inheritdoc/>
        public void DrawPath(IMapPath path, in StrokeStyle stroke)
        {
            using SKPath skPath = AsSkiaPath(path);
            if (skPath.PointCount == 0)
                return;

            using SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawPath(skPath, paint);
        }

        /// <inheritdoc/>
        public void FillPath(IMapPath path, in FillStyle fill)
        {
            using SKPath skPath = AsSkiaPath(path);
            if (skPath.PointCount == 0)
                return;

            if (fill.Pattern == FillPatternKind.Solid)
            {
                using SKPaint paint = CreateFillPaint(fill.Color);
                _canvas.DrawPath(skPath, paint);
                return;
            }

            DrawHatchedFill(skPath, fill);
        }

        /// <inheritdoc/>
        public void DrawRectangle(RectangleF rect, in StrokeStyle stroke)
        {
            if (!IsValidRectangle(rect))
                return;

            using SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawRect(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        /// <inheritdoc/>
        public void FillRectangle(RectangleF rect, in FillStyle fill)
        {
            if (!IsValidRectangle(rect))
                return;

            using SKPaint paint = CreateFillPaint(fill.Color);
            _canvas.DrawRect(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        /// <inheritdoc/>
        public void DrawEllipse(RectangleF rect, in StrokeStyle stroke)
        {
            if (!IsValidRectangle(rect))
                return;

            using SKPaint paint = CreateStrokePaint(stroke);
            _canvas.DrawOval(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        /// <inheritdoc/>
        public void FillEllipse(RectangleF rect, in FillStyle fill)
        {
            if (!IsValidRectangle(rect))
                return;

            using SKPaint paint = CreateFillPaint(fill.Color);
            _canvas.DrawOval(SkiaMapPathBuilder.ToSkiaRect(rect), paint);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public SizeF MeasureText(string text, in TextStyle style)
        {
            using SKPaint paint = CreateTextPaint(style);
            string[] lines = SplitLines(text);
            float width = 0.0f;
            foreach (string line in lines)
                width = Math.Max(width, paint.MeasureText(string.IsNullOrEmpty(line) ? " " : line));

            SKFontMetrics metrics = paint.FontMetrics;
            float lineHeight = Math.Max(1.0f, metrics.Descent - metrics.Ascent + metrics.Leading);
            return new SizeF(Math.Max(1.0f, width), lineHeight * lines.Length);
        }

        /// <inheritdoc/>
        public void DrawText(string text, RectangleF layout, in TextStyle style)
        {
            if (!IsValidRectangle(layout))
                return;

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
                _canvas.DrawText(lines[i], x, startBaseline + i * lineHeight, paint);
        }

        /// <inheritdoc/>
        public void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style)
        {
            if (!IsValidRectangle(dest))
                return;

            using SKBitmap bitmap = AsSkiaBitmap(image);
            SKRect source = src.HasValue
                ? SkiaMapPathBuilder.ToSkiaRect(src.Value)
                : new SKRect(0, 0, bitmap.Width, bitmap.Height);
            using SKPaint paint = CreateImagePaint(style);
            _canvas.DrawBitmap(bitmap, source, SkiaMapPathBuilder.ToSkiaRect(dest), paint);
        }

        /// <inheritdoc/>
        public void DrawImage(IMapImage image, ReadOnlySpan<PointF> destPoints, RectangleF src, in ImageStyle style)
        {
            if (destPoints.Length < 3 || !IsValidRectangle(src))
                return;

            PointF ul = destPoints[0], ur = destPoints[1], ll = destPoints[2];
            if (!IsValidPoint(ul) || !IsValidPoint(ur) || !IsValidPoint(ll))
                return;

            using SKBitmap bitmap = AsSkiaBitmap(image);
            SKRect source = SkiaMapPathBuilder.ToSkiaRect(src);
            using SKPaint paint = CreateImagePaint(style);
            using IDisposable state = SaveState();
            SKMatrix matrix = CreateSourceToParallelogramMatrix(src, ul, ur, ll);
            _canvas.Concat(in matrix);
            _canvas.DrawBitmap(bitmap, source, source, paint);
        }

        /// <summary>
        /// Flushes GPU work, reads back pixels, and blits the result to the
        /// WinForms target.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            // Submit all pending GPU draw calls, then copy GPU pixels back to the CPU.
            // This flush + read-back is the dominant cost of the GPU backend: it
            // stalls the pipeline until the GPU finishes and transfers the whole
            // framebuffer over the bus.
            long tReadback = Stopwatch.GetTimestamp();
            _canvas.Flush();
            _grContext.Flush();

            // Read GPU pixels directly into a locked GDI bitmap (Bgra8888 = Format32bppPArgb).
            using Bitmap readback = new(PixelSize.Width, PixelSize.Height, PixelFormat.Format32bppPArgb);
            Rectangle lockRect = new(0, 0, PixelSize.Width, PixelSize.Height);
            BitmapData data = readback.LockBits(lockRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            SKImageInfo readInfo = new(PixelSize.Width, PixelSize.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface.ReadPixels(readInfo, data.Scan0, data.Stride, 0, 0);
            readback.UnlockBits(data);
            double readbackMs = ElapsedMs(tReadback);

            long tBlit = Stopwatch.GetTimestamp();
            _targetGraphics.DrawImageUnscaled(readback, 0, 0);
            double blitMs = ElapsedMs(tBlit);

            _surface.Dispose();

            foreach (SKTypeface tf in _typefaceCache.Values)
                tf.Dispose();
            _typefaceCache.Clear();

            if (_ownsGraphics)
                _targetGraphics.Dispose();

            RenderBackendTelemetry.Record(MapRenderBackend.SkiaGpu, _createMs, readbackMs, blitMs);
        }

        internal IMapRenderSurface CreateFrameLease(MapRenderSurfaceOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            return new FrameLease(this, options);
        }

        internal bool CanLease(Graphics graphics, Size pixelSize) =>
            ReferenceEquals(_targetGraphics, graphics) &&
            PixelSize == ClampPixelSize(pixelSize) &&
            !_disposed;

        private static double ElapsedMs(long startTimestamp) =>
            (Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / Stopwatch.Frequency;

        // ── Private helpers ──────────────────────────────────────────────────────

        private bool IsAntialiasEnabled =>
            _quality != RenderQuality.VectorHighSpeed &&
            _quality != RenderQuality.RasterHighSpeed;

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

        private SKPaint CreateFillPaint(Color color) =>
            new()
            {
                Style = SKPaintStyle.Fill,
                Color = ToSkiaColor(color),
                IsAntialias = IsAntialiasEnabled
            };

        private SKPaint CreateTextPaint(in TextStyle style) =>
            new()
            {
                Style = SKPaintStyle.Fill,
                Color = ToSkiaColor(style.Color),
                IsAntialias = IsAntialiasEnabled,
                TextSize = Math.Max(1.0f, style.SizePx),
                Typeface = GetTypeface(style),
                TextAlign = ToSkiaTextAlign(style.HorizontalAlign)
            };

        private static SKPaint CreateImagePaint(in ImageStyle style) =>
            new()
            {
                IsAntialias = style.Interpolation == ImageInterpolation.HighQuality,
                FilterQuality = style.Interpolation == ImageInterpolation.HighQuality
                    ? SKFilterQuality.High : SKFilterQuality.None,
                Color = SKColors.White.WithAlpha(
                    (byte)(Math.Clamp(style.Opacity, 0.0f, 1.0f) * 255.0f))
            };

        private void DrawHatchedFill(SKPath path, in FillStyle fill)
        {
            using IDisposable state = SaveState();
            if (fill.Color.A > 0)
            {
                using SKPaint bg = CreateFillPaint(fill.Color);
                _canvas.DrawPath(path, bg);
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
                _canvas.DrawLine(x, bounds.Bottom + spacing, x + bounds.Height + spacing, bounds.Top - spacing, hatch);
        }

        private static SKPath AsSkiaPath(IMapPath path)
        {
            if (path is SkiaMapPath sp)
                return new SKPath(sp.Path);
            if (path is GdiMapPath gp)
                return ConvertGdiPath(gp.Path, gp.FillRule);
            throw new ArgumentException("Path type not supported by SkiaGpuMapRenderSurface.", nameof(path));
        }

        private static SKMatrix CreateSourceToParallelogramMatrix(
            RectangleF src, PointF ul, PointF ur, PointF ll)
        {
            float scaleX = (ur.X - ul.X) / src.Width,  skewY  = (ur.Y - ul.Y) / src.Width;
            float skewX  = (ll.X - ul.X) / src.Height, scaleY = (ll.Y - ul.Y) / src.Height;
            return new SKMatrix
            {
                ScaleX = scaleX, SkewX = skewX,
                TransX = ul.X - scaleX * src.Left - skewX  * src.Top,
                SkewY  = skewY,  ScaleY = scaleY,
                TransY = ul.Y - skewY  * src.Left - scaleY * src.Top,
                Persp2 = 1.0f
            };
        }

        private static SKBitmap AsSkiaBitmap(IMapImage image)
        {
            if (image is GdiMapImage gi)
                return CopyBitmapToSkia(gi.Image);
            throw new ArgumentException("Image type not supported by SkiaGpuMapRenderSurface.", nameof(image));
        }

        private static SKPath ConvertGdiPath(GraphicsPath src, FillRule fillRule)
        {
            SKPath path = new() { FillType = SkiaMapPath.ToSkiaFillType(fillRule) };
            PointF[] pts = src.PathPoints;
            byte[] types = src.PathTypes;
            for (int i = 0; i < pts.Length; i++)
            {
                PathPointType t = (PathPointType)(types[i] & (byte)PathPointType.PathTypeMask);
                bool close = (types[i] & (byte)PathPointType.CloseSubpath) != 0;
                switch (t)
                {
                    case PathPointType.Start: path.MoveTo(pts[i].X, pts[i].Y); break;
                    case PathPointType.Line:  path.LineTo(pts[i].X, pts[i].Y); break;
                    case PathPointType.Bezier3 when i + 2 < pts.Length:
                        path.CubicTo(pts[i].X, pts[i].Y, pts[i+1].X, pts[i+1].Y, pts[i+2].X, pts[i+2].Y);
                        close = (types[i + 2] & (byte)PathPointType.CloseSubpath) != 0;
                        i += 2;
                        break;
                }
                if (close) path.Close();
            }
            return path;
        }

        private static SKBitmap CopyBitmapToSkia(Image image)
        {
            using Bitmap bmp = image is Bitmap existing ? new Bitmap(existing) : new Bitmap(image);
            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            try
            {
                SKImageInfo info = new(bmp.Width, bmp.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                SKBitmap sk = new(info);
                sk.InstallPixels(info, data.Scan0, data.Stride);
                SKBitmap copy = new(info);
                sk.CopyTo(copy);
                return copy;
            }
            finally { bmp.UnlockBits(data); }
        }

        private SKTypeface GetTypeface(in TextStyle style)
        {
            string family = string.IsNullOrWhiteSpace(style.FontFamily) ? "Segoe UI" : style.FontFamily;
            TextKey key = new(family, style.Bold);
            if (_typefaceCache.TryGetValue(key, out SKTypeface? cached))
                return cached;
            SKTypeface tf = SKTypeface.FromFamilyName(
                family,
                style.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);
            _typefaceCache[key] = tf;
            return tf;
        }

        private static SKPathEffect? CreateDashEffect(in StrokeStyle style)
        {
            float s = Math.Clamp(style.DashScale, 0.1f, 100.0f);
            float[]? iv = style.DashPattern switch
            {
                DashPatternKind.Dashed      => [4f * s, 2f * s],
                DashPatternKind.Dotted      => [0.1f, Math.Max(1.5f, 2f * s)],
                DashPatternKind.DashDot     => [4f * s, 2f * s, 1f * s, 2f * s],
                DashPatternKind.DashDoubleDot => [4f * s, 2f * s, 1f * s, 2f * s, 1f * s, 2f * s],
                DashPatternKind.CenterLine  => [8f * s, 3f * s, 2f * s, 3f * s],
                _ => null
            };
            return iv == null ? null : SKPathEffect.CreateDash(iv, 0.0f);
        }

        private static string[] SplitLines(string text) =>
            string.IsNullOrEmpty(text)
                ? [" "]
                : text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        private static float CalculateFirstBaseline(
            RectangleF layout, in TextStyle style, in SKFontMetrics m,
            float totalHeight, float lineHeight)
        {
            float top = style.VerticalAlign switch
            {
                TextAlign.Center => layout.Top + (layout.Height - totalHeight) / 2.0f,
                TextAlign.Far    => layout.Bottom - totalHeight,
                _                => layout.Top
            };
            return top - m.Ascent + Math.Max(0.0f, (lineHeight - (m.Descent - m.Ascent)) / 2.0f);
        }

        private static float CalculateTextX(RectangleF layout, TextAlign alignment) =>
            alignment switch
            {
                TextAlign.Center => layout.Left + layout.Width / 2.0f,
                TextAlign.Far    => layout.Right,
                _                => layout.Left
            };

        private static SKColor ToSkiaColor(Color c) => new(c.R, c.G, c.B, c.A);

        private static SKStrokeCap ToSkiaStrokeCap(LineCapKind cap) =>
            cap switch
            {
                LineCapKind.Flat   => SKStrokeCap.Butt,
                LineCapKind.Square => SKStrokeCap.Square,
                _ => SKStrokeCap.Round
            };

        private static SKStrokeJoin ToSkiaStrokeJoin(LineJoinKind j) =>
            j switch
            {
                LineJoinKind.Miter => SKStrokeJoin.Miter,
                LineJoinKind.Bevel => SKStrokeJoin.Bevel,
                _ => SKStrokeJoin.Round
            };

        private static SKTextAlign ToSkiaTextAlign(TextAlign a) =>
            a switch
            {
                TextAlign.Center => SKTextAlign.Center,
                TextAlign.Far    => SKTextAlign.Right,
                _                => SKTextAlign.Left
            };

        private static Size ResolvePixelSize(Graphics g)
        {
            RectangleF b = g.VisibleClipBounds;
            return ClampPixelSize(new Size((int)Math.Ceiling(b.Width), (int)Math.Ceiling(b.Height)));
        }

        private static Size ClampPixelSize(Size s) => new(Math.Max(1, s.Width), Math.Max(1, s.Height));
        private static bool IsValidPoint(PointF p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
        private static bool IsValidRectangle(RectangleF r) =>
            float.IsFinite(r.Left) && float.IsFinite(r.Top) &&
            float.IsFinite(r.Right) && float.IsFinite(r.Bottom) &&
            r.Width > 0 && r.Height > 0;

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
                if (_disposed) return;
                _disposed = true;
                _canvas.Restore();
            }
        }

        private sealed class FrameLease : IMapRenderSurface
        {
            private readonly SkiaGpuMapRenderSurface _owner;
            private readonly RenderQuality _previousQuality;
            private bool _disposed;

            public FrameLease(SkiaGpuMapRenderSurface owner, MapRenderSurfaceOptions options)
            {
                _owner = owner;
                _previousQuality = owner._quality;
                owner._canvas.Save();
                if (options.ApplyInitialQuality)
                {
                    owner.SetQuality(options.InitialQuality);
                }
            }

            public Size PixelSize => _owner.PixelSize;

            public void Clear(Color color) => _owner.Clear(color);

            public IDisposable SaveState() => _owner.SaveState();

            public void SetQuality(RenderQuality quality) => _owner.SetQuality(quality);

            public void ClipPath(IMapPath path) => _owner.ClipPath(path);

            public IMapPathBuilder CreatePath(FillRule fillRule = FillRule.Winding) =>
                _owner.CreatePath(fillRule);

            public void DrawLine(PointF a, PointF b, in StrokeStyle stroke) =>
                _owner.DrawLine(a, b, stroke);

            public void DrawPath(IMapPath path, in StrokeStyle stroke) =>
                _owner.DrawPath(path, stroke);

            public void FillPath(IMapPath path, in FillStyle fill) =>
                _owner.FillPath(path, fill);

            public void DrawRectangle(RectangleF rect, in StrokeStyle stroke) =>
                _owner.DrawRectangle(rect, stroke);

            public void FillRectangle(RectangleF rect, in FillStyle fill) =>
                _owner.FillRectangle(rect, fill);

            public void DrawEllipse(RectangleF rect, in StrokeStyle stroke) =>
                _owner.DrawEllipse(rect, stroke);

            public void FillEllipse(RectangleF rect, in FillStyle fill) =>
                _owner.FillEllipse(rect, fill);

            public void DrawArc(RectangleF rect, float startDeg, float sweepDeg, in StrokeStyle stroke) =>
                _owner.DrawArc(rect, startDeg, sweepDeg, stroke);

            public SizeF MeasureText(string text, in TextStyle style) =>
                _owner.MeasureText(text, style);

            public void DrawText(string text, RectangleF layout, in TextStyle style) =>
                _owner.DrawText(text, layout, style);

            public void DrawImage(IMapImage image, RectangleF dest, RectangleF? src, in ImageStyle style) =>
                _owner.DrawImage(image, dest, src, style);

            public void DrawImage(IMapImage image, ReadOnlySpan<PointF> destPoints, RectangleF src, in ImageStyle style) =>
                _owner.DrawImage(image, destPoints, src, style);

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _owner._quality = _previousQuality;
                _owner._canvas.Restore();
            }
        }

        private readonly record struct TextKey(string Family, bool Bold);
    }
}

#pragma warning restore CS0618
