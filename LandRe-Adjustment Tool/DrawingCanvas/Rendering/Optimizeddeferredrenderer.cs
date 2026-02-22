using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.DrawingCanvas.Core;
using Land_Readjustment_Tool.DrawingCanvas.Models.Shapes;
using System.Collections.Generic;

namespace Land_Readjustment_Tool.DrawingCanvas.Rendering
{
    /// <summary>
    /// Deferred renderer responsible ONLY for the shapes layer.
    /// 
    /// RESPONSIBILITIES:
    ///   - Cache rendered shapes into a bitmap (_shapesCache)
    ///   - During pan: blit the cached bitmap shifted by screen-pixel delta
    ///   - After pan: re-render cache at new viewport position
    ///   - LOD (Level of Detail) for large datasets
    /// 
    /// NOT RESPONSIBLE FOR:
    ///   - Grid, axis markers, UI overlays  (CanvasRenderer handles those)
    ///   - Pan state decisions              (Calling form tracks _isPanning)
    /// 
    /// PERFORMANCE TIERS:
    ///   < 1 000 shapes  → AntiAlias, bitmap cache
    ///   1 000–5 000     → HighSpeed, bitmap cache
    ///   > 5 000         → LOD (skip sub-pixel shapes), direct draw, no cache
    /// </summary>
    public class OptimizedDeferredRenderer : IDisposable
    {
        // ── Bitmaps ──────────────────────────────────────────────────────────
        private Bitmap _shapesCache;   // Fully rendered shapes at current viewport
        private Bitmap _panBuffer;     // Snapshot taken at BeginPan(); shifted during drag

        // ── State ────────────────────────────────────────────────────────────
        private Size _canvasSize;
        private DrawingEngine _engine;
        private ShapeManager _shapeManager;
        private bool _cacheValid = false;
        private RectangleD _lastViewBounds;

        // ── Performance thresholds ───────────────────────────────────────────
        private const int CACHE_THRESHOLD = 1000;   // Above this: skip bitmap cache
        private const int LOD_THRESHOLD = 20000;   // Above this: skip sub-pixel shapes
        // ─────────────────────────────────────────────────────────────────────
        public OptimizedDeferredRenderer(Size canvasSize, DrawingEngine engine, ShapeManager shapeManager)
        {
            _canvasSize = canvasSize;
            _engine = engine;
            _shapeManager = shapeManager;

            _shapesCache = new Bitmap(Math.Max(1, _canvasSize.Width), Math.Max(1, _canvasSize.Height));
            _panBuffer = new Bitmap(Math.Max(1, _canvasSize.Width), Math.Max(1, _canvasSize.Height));
            _lastViewBounds = _engine.GetViewportBounds();

            RenderNow();
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Full re-render of shapes into the cache bitmap.
        /// Call after: zoom, add/remove shape, EndPan(), Resize().
        /// </summary>
        public void RenderNow()
        {
            if (_shapesCache == null || _canvasSize.Width <= 0 || _canvasSize.Height <= 0)
                return;

            var viewBounds = _engine.GetViewportBounds();
            var shapes = _shapeManager.QueryShapesInBound(viewBounds);

            // Too many shapes for a cache – fall back to direct draw each frame
            if (shapes.Count > CACHE_THRESHOLD)
            {
                _cacheValid = false;
                _lastViewBounds = viewBounds;
                return;
            }

            using (var g = Graphics.FromImage(_shapesCache))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                foreach (var shape in shapes)
                    shape.Draw(g, _engine.WorldToScreen, isPreview: false);
            }

            _cacheValid = true;
            _lastViewBounds = viewBounds;
        }

        /// <summary>
        /// Call on MouseDown when panning starts.
        /// Snapshots the current shapes cache into the pan buffer.
        /// </summary>
        public void BeginPan()
        {
            // If cache is valid, copy it; otherwise draw shapes directly into pan buffer
            using (var g = Graphics.FromImage(_panBuffer))
            {
                g.Clear(Color.Transparent);

                if (_cacheValid && _shapesCache != null)
                {
                    // Fast path: cache is ready – one blit
                    g.DrawImage(_shapesCache, 0, 0);
                }
                else
                {
                    // Slow path: render directly into pan buffer (happens when shape count > threshold)
                    var shapes = _shapeManager.QueryShapesInBound(_engine.GetViewportBounds());
                    g.SmoothingMode = SmoothingMode.HighSpeed;
                    foreach (var shape in shapes)
                        shape.Draw(g, _engine.WorldToScreen, isPreview: false);
                }
            }
        }

        /// <summary>
        /// Call on MouseMove while panning.
        /// Blits the pan buffer shifted by the TOTAL accumulated screen delta since BeginPan().
        /// The gap at canvas edges is left transparent – background colour shows through.
        /// </summary>
        public void DrawShapesDuringPan(Graphics g, double totalDeltaX, double totalDeltaY)
        {
            if (_panBuffer == null) return;

            // InterpolationMode.NearestNeighbor avoids blur from sub-pixel bicubic resampling
            // when doing a pure integer-pixel translation.
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(_panBuffer, (float)totalDeltaX, (float)totalDeltaY);
        }

        /// <summary>
        /// Call on MouseUp when panning ends.
        /// Forces a full re-render at the new viewport position.
        /// </summary>
        public void EndPan()
        {
            // Always re-render – the viewport has moved
            RenderNow();
        }

        /// <summary>
        /// Normal (non-pan) draw path.
        /// Uses bitmap cache when valid; falls back to direct LOD draw otherwise.
        /// </summary>
        public void DrawShapesCache(Graphics g)
        {
            // Strategy 1 – single bitmap blit (fastest)
            if (_cacheValid && _shapesCache != null)
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(_shapesCache, 0, 0);
                return;
            }

            // Strategy 2 – direct draw with LOD optimisation
            var viewBounds = _engine.GetViewportBounds();
            var shapes = _shapeManager.QueryShapesInBound(viewBounds);
            DrawDirect(g, shapes);
        }

        // ── Resize ───────────────────────────────────────────────────────────

        public void Resize(Size newSize)
        {
            if (newSize.Width <= 0 || newSize.Height <= 0) return;
            if (newSize == _canvasSize) return;

            _canvasSize = newSize;

            _shapesCache?.Dispose();
            _panBuffer?.Dispose();

            _shapesCache = new Bitmap(_canvasSize.Width, _canvasSize.Height);
            _panBuffer = new Bitmap(_canvasSize.Width, _canvasSize.Height);

            _cacheValid = false;
            RenderNow();
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Direct draw with Level-of-Detail optimisation.
        /// Skips shapes whose bounding box is smaller than one screen pixel.
        /// </summary>
        private void DrawDirect(Graphics g, List<IShape> shapes)
        {
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;

            bool useLOD = shapes.Count > LOD_THRESHOLD;

            // Minimum visible world size = 2 screen pixels
            double worldPerPixel = _engine.GetViewportBounds().Width / Math.Max(_canvasSize.Width, 1);
            double minVisibleSize = worldPerPixel * 2.0;

            int drawn = 0, skipped = 0;

            foreach (var shape in shapes)
            {
                if (useLOD)
                {
                    var bbox = shape.GetBoundingBox();
                    if (Math.Max(bbox.Width, bbox.Height) < minVisibleSize)
                    {
                        skipped++;
                        continue;
                    }
                }

                shape.Draw(g, _engine.WorldToScreen, isPreview: false);
                drawn++;
            }

            if (shapes.Count > LOD_THRESHOLD)
                DrawStatusBadge(g, drawn, skipped);
        }

        /// <summary>
        /// Small badge in the top-right corner reporting shape counts when LOD is active.
        /// </summary>
        private void DrawStatusBadge(Graphics g, int drawn, int skipped)
        {
            using (var font = new Font("Consolas", 9))
            using (var fg = new SolidBrush(Color.FromArgb(200, 180, 220, 255)))
            using (var bg = new SolidBrush(Color.FromArgb(180, 20, 20, 20)))
            {
                string text = skipped > 0
                    ? $"LOD  drawn {drawn:N0}  skipped {skipped:N0}"
                    : $"Direct  {drawn:N0} shapes";

                var sz = g.MeasureString(text, font);
                float x = _canvasSize.Width - sz.Width - 28;
                float y = _canvasSize.Height - sz.Height - 28;
                g.FillRectangle(bg, x - 6, y - 4, sz.Width + 12, sz.Height + 8);
                g.DrawString(text, font, fg, x, y);
            }
        }

        // ── IDisposable ──────────────────────────────────────────────────────

        public void Dispose()
        {
            _shapesCache?.Dispose();
            _panBuffer?.Dispose();
        }
    }
}