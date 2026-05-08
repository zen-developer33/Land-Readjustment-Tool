using System.Drawing.Drawing2D;
using System.Globalization;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class CanvasVectorRenderer : IDisposable
    {
        private const double MaxGdiCoordinate = 1_000_000.0;
        private readonly PenCache _penCache = new();
        private readonly BrushCache _brushCache = new();
        private readonly Font _labelFont = new("Segoe UI", 8.0f, FontStyle.Regular);
        private readonly Font _previewFont = new("Segoe UI", 8.0f, FontStyle.Bold);
        private readonly VectorFeatureSpatialIndex _featureSpatialIndex = new();
        private IReadOnlyList<CanvasFeature> _features = [];
        private IReadOnlyDictionary<int, CanvasLayer> _layersById =
            new Dictionary<int, CanvasLayer>();

        public int FeatureCount => _features.Count;

        public void UpdateFeatures(IEnumerable<CanvasFeature>? features)
        {
            _features = features?.ToArray() ?? [];
            _featureSpatialIndex.Rebuild(_features);
        }

        public void UpdateLayers(IEnumerable<CanvasLayer>? layers)
        {
            _layersById = layers?
                .GroupBy(layer => layer.Id)
                .ToDictionary(group => group.Key, group => group.First())
                ?? new Dictionary<int, CanvasLayer>();
        }

        public void UpdateLayer(CanvasLayer layer)
        {
            Dictionary<int, CanvasLayer> copy = new(_layersById)
            {
                [layer.Id] = layer
            };
            _layersById = copy;
        }

        public RectangleD? GetFeatureBounds()
        {
            bool hasBounds = false;
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (CanvasFeature feature in _features)
            {
                RectangleD bounds = feature.Shape.GetBoundingBox();
                if (bounds.Width <= 0 && bounds.Height <= 0)
                {
                    continue;
                }

                hasBounds = true;
                minX = Math.Min(minX, bounds.Left);
                minY = Math.Min(minY, bounds.Top);
                maxX = Math.Max(maxX, bounds.Right);
                maxY = Math.Max(maxY, bounds.Bottom);
            }

            return hasBounds
                ? new RectangleD(minX, minY, maxX - minX, maxY - minY)
                : null;
        }

        public void Render(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool useLevelOfDetail = false,
            Size? canvasSize = null)
        {
            if (_features.Count == 0)
            {
                return;
            }

            double minimumVisibleWorldSize = 0.0;
            if (useLevelOfDetail &&
                canvasSize.HasValue &&
                canvasSize.Value.Width > 0)
            {
                minimumVisibleWorldSize =
                    visibleWorldBounds.Width / canvasSize.Value.Width * 2.0;
            }

            VectorRenderContext context = new(
                _penCache,
                _brushCache,
                engine.ZoomScale);

            foreach (CanvasFeature feature in _featureSpatialIndex.Query(visibleWorldBounds)
                .OrderBy(GetDisplayOrder)
                .ThenBy(feature => feature.CanvasObject.Id))
            {
                CanvasLayer? layer = ResolveLayer(feature);
                if (!IsRenderable(feature, layer, visibleWorldBounds))
                {
                    continue;
                }

                if (minimumVisibleWorldSize > 0.0 &&
                    IsBelowLevelOfDetail(feature.Shape.GetBoundingBox(), minimumVisibleWorldSize))
                {
                    continue;
                }

                DrawShape(graphics, engine, feature.Shape, ResolveStyle(feature, layer), context);
                DrawLabelIfNeeded(graphics, engine, feature, layer, context);
            }
        }

        public void RenderPreview(
            Graphics graphics,
            MapCanvasEngine engine,
            IShape? previewShape,
            CanvasLayer? previewLayer)
        {
            if (previewShape == null)
            {
                return;
            }

            VectorRenderContext context = new(
                _penCache,
                _brushCache,
                engine.ZoomScale,
                isPreview: true);

            DrawShape(graphics, engine, previewShape, ResolveStyle(previewShape, previewLayer), context);

            if (previewShape is CircleShape circle)
            {
                DrawCircleRadiusPreview(graphics, engine, circle, context);
            }
        }

        private void DrawCircleRadiusPreview(
            Graphics graphics,
            MapCanvasEngine engine,
            CircleShape circle,
            VectorRenderContext context)
        {
            PointF center = ToScreenPointF(engine.WorldToScreen(circle.Center));
            PointF edge = ToScreenPointF(engine.WorldToScreen(circle.RadiusPoint));
            if (!IsValidPoint(center) ||
                !IsValidPoint(edge) ||
                Distance(center, edge) < 2.0f)
            {
                return;
            }

            using GraphicsPath path = new();
            path.AddLine(center, edge);
            graphics.DrawPath(
                context.GetPen(Color.FromArgb(232, 224, 172, 36), 1.1f, DashStyle.Dash),
                path);

            string radiusText = circle.GetRadius().ToString("0.###", CultureInfo.CurrentCulture);
            SizeF textSize = graphics.MeasureString(radiusText, _previewFont);
            PointF midpoint = new(
                (center.X + edge.X) / 2.0f,
                (center.Y + edge.Y) / 2.0f);
            RectangleF labelBounds = new(
                midpoint.X - textSize.Width / 2.0f - 4.0f,
                midpoint.Y - textSize.Height / 2.0f - 2.0f,
                textSize.Width + 8.0f,
                textSize.Height + 4.0f);

            graphics.FillRectangle(
                context.GetSolidBrush(Color.FromArgb(0, 122, 204)),
                labelBounds);
            graphics.DrawRectangle(
                context.GetPen(Color.FromArgb(20, 20, 20), 1.0f),
                labelBounds.X,
                labelBounds.Y,
                labelBounds.Width,
                labelBounds.Height);
            graphics.DrawString(
                radiusText,
                _previewFont,
                context.GetSolidBrush(Color.White),
                labelBounds.X + 4.0f,
                labelBounds.Y + 2.0f);
        }

        private static bool IsRenderable(
            CanvasFeature feature,
            CanvasLayer? layer,
            RectangleD visibleWorldBounds)
        {
            if (!feature.Shape.IsVisible ||
                !feature.CanvasObject.IsVisible ||
                layer?.IsVisible == false)
            {
                return false;
            }

            return feature.Shape.GetBoundingBox().IntersectsWith(visibleWorldBounds);
        }

        private static bool IsBelowLevelOfDetail(
            RectangleD bounds,
            double minimumVisibleWorldSize)
        {
            return Math.Max(bounds.Width, bounds.Height) < minimumVisibleWorldSize;
        }

        private int GetDisplayOrder(CanvasFeature feature) =>
            ResolveLayer(feature)?.DisplayOrder ?? int.MaxValue;

        private CanvasLayer? ResolveLayer(CanvasFeature feature)
        {
            if (_layersById.TryGetValue(feature.CanvasObject.CanvasLayerId, out CanvasLayer? layer))
            {
                return layer;
            }

            return feature.Layer;
        }

        private static VectorShapeStyle ResolveStyle(
            CanvasFeature feature,
            CanvasLayer? layer)
        {
            return ResolveStyle(feature.Shape, layer, feature.CanvasObject);
        }

        private static VectorShapeStyle ResolveStyle(
            IShape shape,
            CanvasLayer? layer,
            CanvasObject? canvasObject = null)
        {
            Color stroke = ParseColor(
                layer?.BorderColor,
                ParseColor(canvasObject?.BorderColorOverride, shape.BorderColor));

            Color fill = ParseColor(
                layer?.FillColor,
                ParseColor(canvasObject?.FillColorOverride, shape.FillColor));

            int transparency = Math.Clamp(
                layer?.FillTransparency ?? canvasObject?.FillTransparencyOverride ?? 100,
                0,
                100);
            int alpha = (int)Math.Round(255 * ((100 - transparency) / 100.0));
            fill = Color.FromArgb(alpha, fill.R, fill.G, fill.B);

            double lineWeight = layer?.LineWeight ?? canvasObject?.LineWeightOverride ?? 1.0;
            string lineStyle = layer?.LineStyle ?? canvasObject?.LineStyleOverride ?? "Solid";

            FillMode fillMode = ResolveFillMode(layer, fill);
            HatchStyle hatchStyle = ResolveHatchStyle(layer?.HatchPattern);

            return new VectorShapeStyle(
                stroke,
                fill,
                (float)Math.Max(0.25, lineWeight),
                ResolveDashStyle(lineStyle),
                fillMode,
                hatchStyle);
        }

        private static FillMode ResolveFillMode(CanvasLayer? layer, Color fill)
        {
            if (fill.A == 0 ||
                layer == null ||
                string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(layer.FillColor))
            {
                return FillMode.None;
            }

            return string.Equals(layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase)
                ? FillMode.Hatched
                : FillMode.Solid;
        }

        private static Color ParseColor(string? html, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return fallback;
            }

            try
            {
                return ColorTranslator.FromHtml(html);
            }
            catch
            {
                return fallback;
            }
        }

        private static DashStyle ResolveDashStyle(string? lineStyle)
        {
            return lineStyle?.Trim().ToLowerInvariant() switch
            {
                "dashed" => DashStyle.Dash,
                "dotted" => DashStyle.Dot,
                "dashdot" => DashStyle.DashDot,
                "centerline" => DashStyle.DashDotDot,
                _ => DashStyle.Solid
            };
        }

        private static HatchStyle ResolveHatchStyle(string? hatchPattern)
        {
            return hatchPattern?.Trim().ToUpperInvariant() switch
            {
                "ANSI32" => HatchStyle.BackwardDiagonal,
                "ANSI33" => HatchStyle.Cross,
                "ANSI34" => HatchStyle.DiagonalCross,
                "DOTS" => HatchStyle.Percent20,
                "EARTH" => HatchStyle.LargeConfetti,
                _ => HatchStyle.ForwardDiagonal
            };
        }

        private static void DrawShape(
            Graphics graphics,
            MapCanvasEngine engine,
            IShape shape,
            VectorShapeStyle style,
            VectorRenderContext context)
        {
            float width = shape.IsSelected
                ? Math.Max(2.0f, style.LineWidth)
                : Math.Max(0.25f, style.LineWidth);
            Color stroke = shape.IsSelected ? Color.Yellow : style.StrokeColor;
            DashStyle dashStyle = style.DashStyle;
            Pen pen = context.GetPen(stroke, width, dashStyle);

            switch (shape)
            {
                case PolylineShape polyline:
                    DrawPolyline(graphics, engine, polyline, style, context, pen);
                    break;
                case LineShape line:
                    DrawLine(graphics, engine, line, pen);
                    break;
                case RectangleShape rectangle:
                    DrawRectangle(graphics, engine, rectangle, style, context, pen);
                    break;
                case CircleShape circle:
                    DrawCircle(graphics, engine, circle, style, context, pen);
                    break;
                case EllipseShape ellipse:
                    DrawEllipse(graphics, engine, ellipse, style, context, pen);
                    break;
                case TextShape text:
                    DrawText(graphics, engine, text, context);
                    break;
                default:
                    shape.Draw(graphics, engine.WorldToScreen, context.IsPreview);
                    break;
            }
        }

        private static void DrawPolyline(
            Graphics graphics,
            MapCanvasEngine engine,
            PolylineShape polyline,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen pen)
        {
            if (polyline.Vertices.Count < 2)
            {
                return;
            }

            PointF[] points = polyline.Vertices
                .Select(point => ToScreenPointF(engine.WorldToScreen(point)))
                .ToArray();

            if (!points.All(IsValidPoint))
            {
                return;
            }

            if (polyline.IsClosed && points.Length >= 3 && style.FillMode != FillMode.None)
            {
                Brush fillBrush = style.FillMode == FillMode.Hatched
                    ? context.GetHatchBrush(style.HatchStyle, style.StrokeColor, style.FillColor)
                    : context.GetSolidBrush(style.FillColor);
                graphics.FillPolygon(fillBrush, points);
            }

            graphics.DrawLines(pen, points);

            if (polyline.IsClosed && points.Length > 2)
            {
                graphics.DrawLine(pen, points[^1], points[0]);
            }
        }

        private static void DrawLine(
            Graphics graphics,
            MapCanvasEngine engine,
            LineShape line,
            Pen pen)
        {
            PointF start = ToScreenPointF(engine.WorldToScreen(line.Start));
            PointF end = ToScreenPointF(engine.WorldToScreen(line.End));
            if (IsValidPoint(start) && IsValidPoint(end))
            {
                graphics.DrawLine(pen, start, end);
            }
        }

        private static void DrawRectangle(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleShape rectangle,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen pen)
        {
            PointF start = ToScreenPointF(engine.WorldToScreen(rectangle.Start));
            PointF end = ToScreenPointF(engine.WorldToScreen(rectangle.End));
            RectangleF rect = CreateScreenRectangle(start, end);
            if (!IsValidRectangle(rect))
            {
                return;
            }

            if (style.FillMode != FillMode.None)
            {
                Brush fillBrush = style.FillMode == FillMode.Hatched
                    ? context.GetHatchBrush(style.HatchStyle, style.StrokeColor, style.FillColor)
                    : context.GetSolidBrush(style.FillColor);
                graphics.FillRectangle(fillBrush, rect);
            }

            graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }

        private static void DrawCircle(
            Graphics graphics,
            MapCanvasEngine engine,
            CircleShape circle,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen pen)
        {
            PointF center = ToScreenPointF(engine.WorldToScreen(circle.Center));
            PointF edge = ToScreenPointF(engine.WorldToScreen(circle.RadiusPoint));
            float radius = Distance(center, edge);
            RectangleF rect = new(
                center.X - radius,
                center.Y - radius,
                radius * 2.0f,
                radius * 2.0f);

            if (IsValidRectangle(rect))
            {
                if (style.FillMode != FillMode.None)
                {
                    Brush fillBrush = style.FillMode == FillMode.Hatched
                        ? context.GetHatchBrush(style.HatchStyle, style.StrokeColor, style.FillColor)
                        : context.GetSolidBrush(style.FillColor);
                    graphics.FillEllipse(fillBrush, rect);
                }

                graphics.DrawEllipse(pen, rect);
            }
        }

        private static void DrawEllipse(
            Graphics graphics,
            MapCanvasEngine engine,
            EllipseShape ellipse,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen pen)
        {
            PointF start = ToScreenPointF(engine.WorldToScreen(ellipse.Start));
            PointF end = ToScreenPointF(engine.WorldToScreen(ellipse.End));
            RectangleF rect = CreateScreenRectangle(start, end);
            if (!IsValidRectangle(rect))
            {
                return;
            }

            if (style.FillMode != FillMode.None)
            {
                graphics.FillEllipse(context.GetSolidBrush(style.FillColor), rect);
            }

            graphics.DrawEllipse(pen, rect);
        }

        private static void DrawText(
            Graphics graphics,
            MapCanvasEngine engine,
            TextShape text,
            VectorRenderContext context)
        {
            PointF position = ToScreenPointF(engine.WorldToScreen(text.Position));
            if (!IsValidPoint(position))
            {
                return;
            }

            Color color = text.FillColor == Color.Transparent
                ? text.BorderColor
                : text.FillColor;
            graphics.DrawString(text.Text, text.Font, context.GetSolidBrush(color), position);
        }

        private void DrawLabelIfNeeded(
            Graphics graphics,
            MapCanvasEngine engine,
            CanvasFeature feature,
            CanvasLayer? layer,
            VectorRenderContext context)
        {
            if (layer?.ShowLabels != true)
            {
                return;
            }

            string? labelText = ResolveLabelText(feature, layer);
            if (string.IsNullOrWhiteSpace(labelText))
            {
                return;
            }

            RectangleD bounds = feature.Shape.GetBoundingBox();
            PointF position = ToScreenPointF(engine.WorldToScreen(new PointD(
                bounds.X + bounds.Width / 2.0,
                bounds.Y + bounds.Height / 2.0)));

            if (!IsValidPoint(position))
            {
                return;
            }

            Color labelColor = ParseColor(layer.LabelColor, Color.Black);
            SizeF size = graphics.MeasureString(labelText, _labelFont);
            graphics.DrawString(
                labelText,
                _labelFont,
                context.GetSolidBrush(labelColor),
                position.X - size.Width / 2.0f,
                position.Y - size.Height / 2.0f);
        }

        private static string? ResolveLabelText(CanvasFeature feature, CanvasLayer layer)
        {
            if (!string.IsNullOrWhiteSpace(feature.CanvasObject.LabelText))
            {
                return feature.CanvasObject.LabelText;
            }

            if (!string.IsNullOrWhiteSpace(layer.LabelField) &&
                feature.Shape.Properties.TryGetValue(layer.LabelField, out object? value))
            {
                return value?.ToString();
            }

            return feature.Shape.Properties.TryGetValue("LabelText", out object? fallback)
                ? fallback?.ToString()
                : null;
        }

        private static RectangleF CreateScreenRectangle(PointF a, PointF b)
        {
            float left = Math.Min(a.X, b.X);
            float top = Math.Min(a.Y, b.Y);
            float right = Math.Max(a.X, b.X);
            float bottom = Math.Max(a.Y, b.Y);
            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        private static PointF ToScreenPointF(PointD screenPoint)
        {
            return new PointF(
                ClampToGdi(Math.Round(screenPoint.X)),
                ClampToGdi(Math.Round(screenPoint.Y)));
        }

        private static float ClampToGdi(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return float.NaN;
            }

            return value > MaxGdiCoordinate
                ? (float)MaxGdiCoordinate
                : value < -MaxGdiCoordinate
                    ? (float)-MaxGdiCoordinate
                    : (float)value;
        }

        private static bool IsValidPoint(PointF point) =>
            float.IsFinite(point.X) &&
            float.IsFinite(point.Y);

        private static bool IsValidRectangle(RectangleF rectangle) =>
            IsValidPoint(rectangle.Location) &&
            float.IsFinite(rectangle.Width) &&
            float.IsFinite(rectangle.Height) &&
            rectangle.Width > 0 &&
            rectangle.Height > 0;

        private static float Distance(PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public void Dispose()
        {
            _penCache.Dispose();
            _brushCache.Dispose();
            _labelFont.Dispose();
            _previewFont.Dispose();
        }

        private enum FillMode
        {
            None,
            Solid,
            Hatched
        }

        private readonly record struct VectorShapeStyle(
            Color StrokeColor,
            Color FillColor,
            float LineWidth,
            DashStyle DashStyle,
            FillMode FillMode,
            HatchStyle HatchStyle);
    }
}
