using System.Drawing.Drawing2D;
using System.Globalization;
using System.Diagnostics;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class CanvasVectorRenderer : IDisposable
    {
        private const double MaxGdiCoordinate = 1_000_000.0;
        private const float HatchLineWidthPx = 0.65f;
        private const float SelectionLineWidthPx = 2.0f;
        private static readonly Color SelectionStrokeColor = Color.FromArgb(0, 122, 204);
        private readonly PenCache _penCache = new();
        private readonly BrushCache _brushCache = new();
        private readonly Font _labelFont = new("Segoe UI", 8.0f, FontStyle.Regular);
        private readonly Font _previewFont = new("Segoe UI", 8.0f, FontStyle.Bold);
        private readonly VectorFeatureSpatialIndex _featureSpatialIndex = new();
        private IReadOnlyList<CanvasFeature> _features = [];
        private IReadOnlyDictionary<int, CanvasLayer> _layersById =
            new Dictionary<int, CanvasLayer>();
        private VectorRenderStats _lastRenderStats = VectorRenderStats.Empty;

        public int FeatureCount => _features.Count;

        public int LayerCount => _layersById.Count;

        public VectorRenderStats LastRenderStats => _lastRenderStats;

        public void UpdateFeatures(IEnumerable<CanvasFeature>? features)
        {
            _features = features?.ToArray() ?? [];
            _featureSpatialIndex.Rebuild(_features);
            _lastRenderStats = VectorRenderStats.Empty;
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
                _lastRenderStats = new VectorRenderStats
                {
                    TotalFeatureCount = 0,
                    SpatialIndexEntryCount = 0,
                    VisibleWorldBounds = visibleWorldBounds
                };
                return;
            }

            Stopwatch renderStopwatch = Stopwatch.StartNew();
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

            Stopwatch queryStopwatch = Stopwatch.StartNew();
            IReadOnlyList<CanvasFeature> queriedFeatures =
                _featureSpatialIndex.Query(visibleWorldBounds);
            queryStopwatch.Stop();

            int renderedCount = 0;
            int hiddenSkippedCount = 0;
            int lodSkippedCount = 0;

            foreach (CanvasFeature feature in queriedFeatures
                .OrderBy(GetDisplayOrder)
                .ThenBy(feature => feature.CanvasObject.Id))
            {
                CanvasLayer? layer = ResolveLayer(feature);
                if (!IsRenderable(feature, layer, visibleWorldBounds))
                {
                    hiddenSkippedCount++;
                    continue;
                }

                if (minimumVisibleWorldSize > 0.0 &&
                    IsBelowLevelOfDetail(feature.Shape.GetBoundingBox(), minimumVisibleWorldSize))
                {
                    lodSkippedCount++;
                    continue;
                }

                DrawShape(graphics, engine, feature.Shape, ResolveStyle(feature, layer), context);
                DrawLabelIfNeeded(graphics, engine, feature, layer, context);
                renderedCount++;
            }

            renderStopwatch.Stop();
            _lastRenderStats = new VectorRenderStats
            {
                TotalFeatureCount = _features.Count,
                SpatialIndexEntryCount = _featureSpatialIndex.EntryCount,
                QueryCandidateCount = queriedFeatures.Count,
                RenderedFeatureCount = renderedCount,
                HiddenSkippedCount = hiddenSkippedCount,
                LodSkippedCount = lodSkippedCount,
                LevelOfDetailEnabled = useLevelOfDetail,
                MinimumVisibleWorldSize = minimumVisibleWorldSize,
                QueryElapsedMs = queryStopwatch.Elapsed.TotalMilliseconds,
                RenderElapsedMs = renderStopwatch.Elapsed.TotalMilliseconds,
                VisibleWorldBounds = visibleWorldBounds
            };
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
                engine.ZoomScale);

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
            if (circle.Properties.TryGetValue("SuppressPreviewHelpers", out object? suppressHelpersObj) &&
                suppressHelpersObj is bool suppressHelpers &&
                suppressHelpers)
            {
                return;
            }

            PointF center = ToScreenPointF(engine.WorldToScreen(circle.Center));

            if (circle.Properties.TryGetValue("DiameterEndpoints", out object? diameterEndpointsObj) &&
                diameterEndpointsObj is PointD[] diameterEndpoints &&
                diameterEndpoints.Length == 2)
            {
                DrawDiameterPreview(
                    graphics,
                    context,
                    diameterEndpoints[0],
                    diameterEndpoints[1],
                    center,
                    engine);
                return;
            }

            // If preview shape has a CenterDiameterEndpoint property, draw a
            // diameter preview (center -> endpoint) and display the diameter
            // value. Otherwise draw the usual radius preview.
            if (circle.Properties.TryGetValue("CenterDiameterEndpoint", out object? endpointObj) && endpointObj is PointD endpointWorld)
            {
                DrawDiameterPreview(
                    graphics,
                    context,
                    circle.Center,
                    endpointWorld,
                    center,
                    engine);
                return;
            }

            // Default radius preview
            PointF edgeDefault = ToScreenPointF(engine.WorldToScreen(circle.RadiusPoint));
            if (!IsValidPoint(center) || !IsValidPoint(edgeDefault) || Distance(center, edgeDefault) < 2.0f)
            {
                return;
            }

            using GraphicsPath pathDefault = new();
            pathDefault.AddLine(center, edgeDefault);
            graphics.DrawPath(
                context.GetPen(Color.FromArgb(232, 224, 172, 36), 1.1f, DashStyle.Dash),
                pathDefault);

            string radiusText = circle.GetRadius().ToString("0.###", CultureInfo.CurrentCulture);
            SizeF textSizeDefault = graphics.MeasureString(radiusText, _previewFont);
            PointF midpointDefault = new(
                (center.X + edgeDefault.X) / 2.0f,
                (center.Y + edgeDefault.Y) / 2.0f);
            RectangleF labelBoundsDefault = new(
                midpointDefault.X - textSizeDefault.Width / 2.0f - 4.0f,
                midpointDefault.Y - textSizeDefault.Height / 2.0f - 2.0f,
                textSizeDefault.Width + 8.0f,
                textSizeDefault.Height + 4.0f);

            graphics.FillRectangle(context.GetSolidBrush(Color.White), labelBoundsDefault);
            graphics.DrawRectangle(
                context.GetPen(Color.FromArgb(90, 90, 90), 1.0f),
                labelBoundsDefault.X,
                labelBoundsDefault.Y,
                labelBoundsDefault.Width,
                labelBoundsDefault.Height);
            graphics.DrawString(
                radiusText,
                _previewFont,
                context.GetSolidBrush(Color.FromArgb(32, 32, 32)),
                labelBoundsDefault.X + 4.0f,
                labelBoundsDefault.Y + 2.0f);
        }

        private void DrawDiameterPreview(
            Graphics graphics,
            VectorRenderContext context,
            PointD startWorld,
            PointD endWorld,
            PointF? center,
            MapCanvasEngine engine)
        {
            PointF start = ToScreenPointF(engine.WorldToScreen(startWorld));
            PointF end = ToScreenPointF(engine.WorldToScreen(endWorld));
            if (!IsValidPoint(start) || !IsValidPoint(end) || Distance(start, end) < 2.0f)
            {
                return;
            }

            using GraphicsPath path = new();
            path.AddLine(start, end);
            graphics.DrawPath(
                context.GetPen(Color.FromArgb(232, 224, 172, 36), 1.1f, DashStyle.Dash),
                path);

            double dx = startWorld.X - endWorld.X;
            double dy = startWorld.Y - endWorld.Y;
            double diameter = Math.Sqrt(dx * dx + dy * dy);

            string diameterText = diameter.ToString("0.###", CultureInfo.CurrentCulture);
            SizeF textSize = graphics.MeasureString(diameterText, _previewFont);
            PointF midpoint = new(
                (start.X + end.X) / 2.0f,
                (start.Y + end.Y) / 2.0f);
            RectangleF labelBounds = new(
                midpoint.X - textSize.Width / 2.0f - 4.0f,
                midpoint.Y - textSize.Height / 2.0f - 2.0f,
                textSize.Width + 8.0f,
                textSize.Height + 4.0f);

            graphics.FillRectangle(context.GetSolidBrush(Color.White), labelBounds);
            graphics.DrawRectangle(
                context.GetPen(Color.FromArgb(90, 90, 90), 1.0f),
                labelBounds.X,
                labelBounds.Y,
                labelBounds.Width,
                labelBounds.Height);
            graphics.DrawString(
                diameterText,
                _previewFont,
                context.GetSolidBrush(Color.FromArgb(32, 32, 32)),
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
            bool hasStroke = lineWeight > 0 && stroke.A > 0;
            bool isPointStyle = layer != null && CanvasLayerTreeService.IsPointLayer(layer);

            FillMode fillMode = ResolveFillMode(layer, fill);

            return new VectorShapeStyle(
                stroke,
                fill,
                (float)Math.Max(0, lineWeight),
                hasStroke,
                ResolveDashStyle(lineStyle),
                fillMode,
                string.IsNullOrWhiteSpace(layer?.HatchPattern) ? "ANSI31" : layer.HatchPattern.Trim(),
                (float)Math.Clamp(layer?.HatchScale ?? 1.0, 0.15, 25.0),
                isPointStyle,
                PointMarkerRenderer.Normalize(layer?.PointSymbol),
                (float)Math.Clamp(layer?.PointSize ?? 5.0, 3.0, 48.0));
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

        private static void DrawShape(
            Graphics graphics,
            MapCanvasEngine engine,
            IShape shape,
            VectorShapeStyle style,
            VectorRenderContext context)
        {
            if (style.IsPointStyle && shape is CircleShape pointCircle)
            {
                DrawPointMarker(graphics, engine, pointCircle, style);
                return;
            }

            bool shouldStroke = style.HasStroke || shape.IsSelected;
            float width = shape.IsSelected
                ? SelectionLineWidthPx
                : Math.Max(0.25f, style.LineWidth);
            Color stroke = shape.IsSelected ? SelectionStrokeColor : style.StrokeColor;
            DashStyle dashStyle = shape.IsSelected ? DashStyle.Dash : style.DashStyle;
            Pen? pen = shouldStroke ? context.GetPen(stroke, width, dashStyle) : null;

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
                case ArcShape arc:
                    DrawArc(graphics, engine, arc, pen);
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

        private static void DrawArc(
            Graphics graphics,
            MapCanvasEngine engine,
            ArcShape arc,
            Pen? pen)
        {
            if (pen == null ||
                arc.Radius <= 0.0 ||
                !double.IsFinite(arc.Radius))
            {
                return;
            }

            PointF center = ToScreenPointF(engine.WorldToScreen(arc.Center));
            PointF radiusPoint = ToScreenPointF(engine.WorldToScreen(
                new PointD(arc.Center.X + arc.Radius, arc.Center.Y)));
            float radius = Distance(center, radiusPoint);
            if (!IsValidPoint(center) ||
                radius <= 0.5f ||
                !float.IsFinite(radius))
            {
                return;
            }

            RectangleF bounds = new(
                center.X - radius,
                center.Y - radius,
                radius * 2.0f,
                radius * 2.0f);
            if (!IsValidRectangle(bounds))
            {
                return;
            }

            float startAngleDegrees = (float)(-arc.StartAngleRadians * 180.0 / Math.PI);
            float sweepAngleDegrees = (float)(-arc.SweepAngleRadians * 180.0 / Math.PI);
            if (!float.IsFinite(startAngleDegrees) ||
                !float.IsFinite(sweepAngleDegrees) ||
                Math.Abs(sweepAngleDegrees) < 0.001f)
            {
                return;
            }

            GraphicsState state = graphics.Save();
            try
            {
                graphics.DrawArc(pen, bounds, startAngleDegrees, sweepAngleDegrees);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private static void DrawPolyline(
            Graphics graphics,
            MapCanvasEngine engine,
            PolylineShape polyline,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen? pen)
        {
            if (polyline.Vertices.Count == 1)
            {
                DrawPointMarker(graphics, engine, polyline.Vertices[0], style);
                return;
            }

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
                using GraphicsPath path = new();
                path.AddPolygon(points);
                FillClosedPath(graphics, path, path.GetBounds(), style, context);
            }

            if (pen == null)
            {
                return;
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
            Pen? pen)
        {
            if (pen == null)
                return;

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
            Pen? pen)
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
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                FillClosedPath(graphics, path, rect, style, context);
            }

            if (pen != null)
            {
                graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        private static void DrawCircle(
            Graphics graphics,
            MapCanvasEngine engine,
            CircleShape circle,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen? pen)
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
                    using GraphicsPath path = new();
                    path.AddEllipse(rect);
                    FillClosedPath(graphics, path, rect, style, context);
                }

                if (pen != null)
                {
                    graphics.DrawEllipse(pen, rect);
                }
            }
        }

        private static void DrawEllipse(
            Graphics graphics,
            MapCanvasEngine engine,
            EllipseShape ellipse,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen? pen)
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
                using GraphicsPath path = new();
                path.AddEllipse(rect);
                FillClosedPath(graphics, path, rect, style, context);
            }

            if (pen != null)
            {
                graphics.DrawEllipse(pen, rect);
            }
        }

        private static void FillClosedPath(
            Graphics graphics,
            GraphicsPath path,
            RectangleF bounds,
            VectorShapeStyle style,
            VectorRenderContext context)
        {
            if (style.FillMode == FillMode.None ||
                !IsValidRectangle(bounds))
            {
                return;
            }

            if (context.IsPreview)
            {
                return;
            }

            if (style.FillColor.A > 0 && style.FillMode != FillMode.Hatched)
            {
                graphics.FillPath(context.GetSolidBrush(style.FillColor), path);
            }

            if (style.FillMode == FillMode.Hatched && !context.IsPreview)
            {
                DrawHatchPattern(graphics, path, bounds, style);
            }
        }

        private static void DrawHatchPattern(
            Graphics graphics,
            GraphicsPath clipPath,
            RectangleF bounds,
            VectorShapeStyle style)
        {
            float spacing = Math.Max(3.0f, ResolveHatchBaseSpacing(style.HatchPattern) * style.HatchScale);
            RectangleF expandedBounds = RectangleF.Inflate(bounds, spacing * 2.0f, spacing * 2.0f);
            Color hatchColor = ResolveHatchColor(style);

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(clipPath, CombineMode.Intersect);

                using Pen hatchPen = new(hatchColor, HatchLineWidthPx)
                {
                    Alignment = PenAlignment.Center,
                    StartCap = LineCap.Flat,
                    EndCap = LineCap.Flat
                };

                foreach (HatchStroke stroke in ResolveHatchStrokes(style.HatchPattern))
                {
                    DrawHatchStrokeFamily(graphics, hatchPen, expandedBounds, spacing, stroke);
                }
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private static Color ResolveHatchColor(VectorShapeStyle style)
        {
            // Use fill color for hatch pattern, with high opacity for visibility
            Color baseColor = Color.FromArgb(255, style.FillColor.R, style.FillColor.G, style.FillColor.B);
            return Color.FromArgb(220, baseColor.R, baseColor.G, baseColor.B);
        }

        private static float ResolveHatchBaseSpacing(string hatchPattern)
        {
            return hatchPattern.Trim().ToUpperInvariant() switch
            {
                "ANSI33" => 6.0f,
                "ANSI34" => 14.0f,
                "DOTS" or "SAND" => 8.0f,
                "EARTH" or "GRAVEL" or "CONCRETE" => 10.0f,
                "BRICK" or "WOOD" => 12.0f,
                _ => 9.0f
            };
        }

        private static IEnumerable<HatchStroke> ResolveHatchStrokes(string hatchPattern)
        {
            return hatchPattern.Trim().ToUpperInvariant() switch
            {
                "ANSI32" or "DIAGONAL-CROSS" => [HatchStroke.ForwardDiagonal, HatchStroke.BackwardDiagonal],
                "HORIZONTAL" => [HatchStroke.Horizontal],
                "VERTICAL" => [HatchStroke.Vertical],
                "CROSS" => [HatchStroke.Horizontal, HatchStroke.Vertical],
                "DOTS" or "SAND" or "GRAVEL" or "CONCRETE" => [HatchStroke.Dots],
                "BRICK" => [HatchStroke.Horizontal, HatchStroke.BrickVertical],
                "NET" => [HatchStroke.ForwardDiagonal, HatchStroke.BackwardDiagonal],
                "WATER" or "WOOD" => [HatchStroke.Wave],
                _ => [HatchStroke.ForwardDiagonal]
            };
        }

        private static void DrawHatchStrokeFamily(
            Graphics graphics,
            Pen pen,
            RectangleF bounds,
            float spacing,
            HatchStroke stroke)
        {
            switch (stroke)
            {
                case HatchStroke.Horizontal:
                    for (float y = bounds.Top; y <= bounds.Bottom; y += spacing)
                    {
                        graphics.DrawLine(pen, bounds.Left, y, bounds.Right, y);
                    }
                    break;

                case HatchStroke.Vertical:
                    for (float x = bounds.Left; x <= bounds.Right; x += spacing)
                    {
                        graphics.DrawLine(pen, x, bounds.Top, x, bounds.Bottom);
                    }
                    break;

                case HatchStroke.ForwardDiagonal:
                    DrawDiagonalHatches(graphics, pen, bounds, spacing, forward: true);
                    break;

                case HatchStroke.BackwardDiagonal:
                    DrawDiagonalHatches(graphics, pen, bounds, spacing, forward: false);
                    break;

                case HatchStroke.Dots:
                    DrawDotHatches(graphics, pen.Color, bounds, spacing);
                    break;

                case HatchStroke.BrickVertical:
                    DrawBrickVerticals(graphics, pen, bounds, spacing);
                    break;

                case HatchStroke.Wave:
                    DrawWaveHatches(graphics, pen, bounds, spacing);
                    break;
            }
        }

        private static void DrawDiagonalHatches(
            Graphics graphics,
            Pen pen,
            RectangleF bounds,
            float spacing,
            bool forward)
        {
            float diagonal = bounds.Width + bounds.Height;
            for (float offset = -bounds.Height; offset <= bounds.Width + bounds.Height; offset += spacing)
            {
                PointF start = forward
                    ? new PointF(bounds.Left + offset, bounds.Bottom)
                    : new PointF(bounds.Left + offset, bounds.Top);
                PointF end = forward
                    ? new PointF(bounds.Left + offset + diagonal, bounds.Bottom - diagonal)
                    : new PointF(bounds.Left + offset + diagonal, bounds.Top + diagonal);
                graphics.DrawLine(pen, start, end);
            }
        }

        private static void DrawDotHatches(
            Graphics graphics,
            Color color,
            RectangleF bounds,
            float spacing)
        {
            float radius = 1.2f;
            using SolidBrush dotBrush = new(color);
            for (float y = bounds.Top; y <= bounds.Bottom; y += spacing)
            {
                for (float x = bounds.Left; x <= bounds.Right; x += spacing)
                {
                    graphics.FillEllipse(dotBrush, x - radius, y - radius, radius * 2.0f, radius * 2.0f);
                }
            }
        }

        private static void DrawBrickVerticals(
            Graphics graphics,
            Pen pen,
            RectangleF bounds,
            float spacing)
        {
            float rowHeight = spacing;
            float brickWidth = spacing * 2.0f;
            int row = 0;
            for (float y = bounds.Top; y <= bounds.Bottom; y += rowHeight)
            {
                float offset = row % 2 == 0 ? 0 : brickWidth / 2.0f;
                for (float x = bounds.Left - offset; x <= bounds.Right; x += brickWidth)
                {
                    graphics.DrawLine(pen, x, y, x, y + rowHeight);
                }

                row++;
            }
        }

        private static void DrawWaveHatches(
            Graphics graphics,
            Pen pen,
            RectangleF bounds,
            float spacing)
        {
            for (float y = bounds.Top; y <= bounds.Bottom; y += spacing)
            {
                using GraphicsPath wavePath = new();
                wavePath.StartFigure();
                float x = bounds.Left;
                wavePath.AddLine(x, y, x, y);
                while (x < bounds.Right)
                {
                    wavePath.AddBezier(
                        x,
                        y,
                        x + spacing * 0.25f,
                        y - spacing * 0.25f,
                        x + spacing * 0.75f,
                        y + spacing * 0.25f,
                        x + spacing,
                        y);
                    x += spacing;
                }

                graphics.DrawPath(pen, wavePath);
            }
        }

        private static void DrawPointMarker(
            Graphics graphics,
            MapCanvasEngine engine,
            CircleShape circle,
            VectorShapeStyle style)
        {
            DrawPointMarker(graphics, engine, circle.Center, style, circle.IsSelected);
        }

        private static void DrawPointMarker(
            Graphics graphics,
            MapCanvasEngine engine,
            PointD worldPoint,
            VectorShapeStyle style,
            bool isSelected = false)
        {
            PointF center = ToScreenPointF(engine.WorldToScreen(worldPoint));
            if (!IsValidPoint(center))
                return;

            float size = style.PointSize * 2.0f;
            RectangleF markerRect = new(
                center.X - size / 2.0f,
                center.Y - size / 2.0f,
                size,
                size);

            if (isSelected)
            {
                using Pen selectionPen = new(Color.FromArgb(0, 122, 204), Math.Max(1.5f, style.LineWidth + 0.75f))
                {
                    Alignment = PenAlignment.Center
                };
                graphics.DrawEllipse(selectionPen, RectangleF.Inflate(markerRect, 4.0f, 4.0f));
            }

            PointMarkerRenderer.Draw(
                graphics,
                markerRect,
                style.PointSymbol,
                isSelected ? Color.FromArgb(0, 170, 255) : style.StrokeColor,
                Math.Max(1.0f, style.LineWidth));
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

        private enum HatchStroke
        {
            Horizontal,
            Vertical,
            ForwardDiagonal,
            BackwardDiagonal,
            Dots,
            BrickVertical,
            Wave
        }

        private readonly record struct VectorShapeStyle(
            Color StrokeColor,
            Color FillColor,
            float LineWidth,
            bool HasStroke,
            DashStyle DashStyle,
            FillMode FillMode,
            string HatchPattern,
            float HatchScale,
            bool IsPointStyle,
            string PointSymbol,
            float PointSize);
    }
}
