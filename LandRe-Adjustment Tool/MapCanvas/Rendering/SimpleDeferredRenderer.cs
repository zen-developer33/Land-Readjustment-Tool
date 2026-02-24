using System;
using System.Drawing;
using Land_Readjustment_Tool.DrawingCanvas.Core;
using Land_Readjustment_Tool.DrawingCanvas.Models.Shapes;
using System.Collections.Generic;

namespace Land_Readjustment_Tool.DrawingCanvas.Rendering
{
    /// <summary>
    /// Simple deferred renderer: caches shapes layer for fast redraws (panning, zooming).
    /// Only shapes are cached; grid, axis, preview, and UI are rendered in real-time.
    /// </summary>
    public class SimpleDeferredRenderer : IDisposable
    {
        private Bitmap _shapesCache;
        private Size _canvasSize;
        private DrawingEngine _engine;
        private ShapeManager _shapeManager;
        private bool _cacheValid = false;

        public SimpleDeferredRenderer(Size canvasSize, DrawingEngine engine, ShapeManager shapeManager)
        {
            _canvasSize = canvasSize;
            _engine = engine;
            _shapeManager = shapeManager;
            _shapesCache = new Bitmap(_canvasSize.Width, _canvasSize.Height);
            RenderNow();
        }

        public void RenderNow()
        {
            if (_shapesCache == null || _canvasSize.Width <= 0 || _canvasSize.Height <= 0)
                return;

            var shapes = _shapeManager.QueryShapesInBound(_engine.GetViewportBounds());

            // CRITICAL PERFORMANCE: Skip cache for large datasets (contours, etc.)
            if (shapes.Count > 1000)
            {
                _cacheValid = false;
                return;
            }

            using (var g = Graphics.FromImage(_shapesCache))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                foreach (var shape in shapes)
                {
                    shape.Draw(g, _engine.WorldToScreen, isPreview: false);
                }
            }
            _cacheValid = true;
        }

        public void BeginPan()
        {
            // No bitmap shift, just a placeholder for API compatibility
        }

        public void UpdatePan(float deltaX, float deltaY)
        {
            // No bitmap shift, just a placeholder for API compatibility
        }

        public void EndPan()
        {
            // No bitmap shift, just a placeholder for API compatibility
        }

        public void DrawShapesCache(Graphics g)
        {
            if (_shapesCache != null && _cacheValid)
            {
                g.DrawImage(_shapesCache, 0, 0);
            }
            else
            {
                // Cache invalid (too many shapes) - draw directly with performance mode
                var shapes = _shapeManager.QueryShapesInBound(_engine.GetViewportBounds());

                // Performance warning for huge datasets
                if (shapes.Count > 5000)
                {
                    using (var font = new Font("Arial", 12, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.FromArgb(255, 255, 100, 100)))
                    using (var bgBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                    {
                        string warning = $"⚠ Performance Mode: {shapes.Count} shapes visible";
                        var size = g.MeasureString(warning, font);
                        float x = _canvasSize.Width - size.Width - 15;
                        float y = 45;
                        g.FillRectangle(bgBrush, x - 5, y - 5, size.Width + 10, size.Height + 10);
                        g.DrawString(warning, font, brush, x, y);
                    }
                }

                // Draw directly (no cache)
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                foreach (var shape in shapes)
                {
                    shape.Draw(g, _engine.WorldToScreen, isPreview: false);
                }
            }
        }

        public void Resize(Size newSize)
        {
            if (newSize != _canvasSize)
            {
                _canvasSize = newSize;
                _shapesCache?.Dispose();
                _shapesCache = new Bitmap(_canvasSize.Width, _canvasSize.Height);
                _cacheValid = false;
                RenderNow();
            }
        }

        public void Dispose()
        {
            _shapesCache?.Dispose();
        }
    }
}