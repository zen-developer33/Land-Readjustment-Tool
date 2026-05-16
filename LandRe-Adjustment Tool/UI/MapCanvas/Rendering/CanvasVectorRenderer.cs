using System.Collections.Concurrent;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class CanvasVectorRenderer : IDisposable
    {
        private const string DefaultCanvasLabelFontName = "Nirmala UI";
        private const double MaxGdiCoordinate = 1_000_000.0;
        private const float HatchLineWidthPx = 0.65f;
        private const float SelectionLineWidthPx = 2.0f;
        private const float LockedLayerColorAlphaFactor = 0.48f;
        private const float MinLabelFontSizePt = 1.0f;
        private const float MaxFixedLabelFontSizePt = 72.0f;
        private const float MaxScaledLabelFontSizePt = 120.0f;
        private const double MinLabelZoomFactor = 0.1;
        private const double MaxLabelZoomFactor = 12.0;
        private static readonly Color SelectionStrokeColor = Color.FromArgb(0, 122, 204);
        private readonly Font _previewFont = new("Segoe UI", 8.0f, FontStyle.Bold);
        private readonly VectorFeatureSpatialIndex _featureSpatialIndex = new();
        private IReadOnlyList<CanvasFeature> _features = Array.Empty<CanvasFeature>();
        private IReadOnlyDictionary<int, CanvasLayer> _layersById =
            new Dictionary<int, CanvasLayer>();
        private IReadOnlySet<Guid> _excludedShapeIds = new HashSet<Guid>();
        private VectorRenderStats _lastRenderStats = VectorRenderStats.Empty;
        private readonly ConcurrentDictionary<Guid, PointD> _labelAnchorCache = new();
        private static readonly GeometryFactory LabelGeometryFactory = new(new PrecisionModel(), 0);

        public int FeatureCount => _features.Count;

        public int LayerCount => _layersById.Count;

        public VectorRenderStats LastRenderStats => _lastRenderStats;

        public void UpdateFeatures(IEnumerable<CanvasFeature>? features)
        {
            _features = features?.ToArray() ?? [];
            _featureSpatialIndex.Rebuild(_features);
            _labelAnchorCache.Clear();
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

        public void SetExcludedShapeIds(IEnumerable<Guid>? shapeIds)
        {
            _excludedShapeIds = shapeIds?
                .Where(id => id != Guid.Empty)
                .ToHashSet()
                ?? new HashSet<Guid>();
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
            Size? canvasSize = null,
            bool antiAliasingEnabled = true)
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

            using PenCache penCache = new();
            using BrushCache brushCache = new();
            using LabelFontCache labelFontCache = new();
            VectorRenderContext context = new(
                penCache,
                brushCache,
                engine.ZoomScale,
                antiAliasingEnabled);

            Stopwatch queryStopwatch = Stopwatch.StartNew();
            IReadOnlyList<CanvasFeature> queriedFeatures =
                _featureSpatialIndex.Query(visibleWorldBounds);
            queryStopwatch.Stop();

            int renderedCount = 0;
            int hiddenSkippedCount = 0;
            int lodSkippedCount = 0;

            List<CanvasFeature> orderedFeatures = queriedFeatures
                .OrderBy(GetDisplayOrder)
                .ThenBy(f => f.CanvasObject.Id)
                .ToList();

            foreach (CanvasFeature feature in orderedFeatures)
            {
                if (_excludedShapeIds.Contains(feature.Shape.Id))
                {
                    hiddenSkippedCount++;
                    continue;
                }

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

                DrawShape(graphics, engine, feature.Shape, ResolveStyle(feature, layer), context, feature, layer, suppressParcelHighlight: true);
                DrawLabelIfNeeded(graphics, engine, visibleWorldBounds, feature, layer, context, labelFontCache);
                renderedCount++;
            }

            // Second pass: draw cadastral parcel selection highlights on top of all features.
            foreach (CanvasFeature feature in orderedFeatures)
            {
                if (_excludedShapeIds.Contains(feature.Shape.Id)) continue;
                if (!IsSelectedCadastralParcelFeature(feature, feature.Shape)) continue;
                CanvasLayer? layer = ResolveLayer(feature);
                if (!IsRenderable(feature, layer, visibleWorldBounds)) continue;
                DrawCadastralParcelHighlightOnly(graphics, engine, feature.Shape);
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
            CanvasLayer? previewLayer,
            CanvasObject? canvasObject = null,
            bool drawAsPreview = true)
        {
            if (previewShape == null)
            {
                return;
            }

            using PenCache penCache = new();
            using BrushCache brushCache = new();
            VectorRenderContext context = new(
                penCache,
                brushCache,
                engine.ZoomScale,
                antiAliasingEnabled: true,
                isPreview: drawAsPreview);

            DrawShape(
                graphics,
                engine,
                previewShape,
                ResolveStyle(previewShape, previewLayer, canvasObject),
                context,
                layer: previewLayer);

            if (drawAsPreview && previewShape is CircleShape circle)
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
            bool isAnnotationLayer = layer != null && CanvasLayerTreeService.IsAnnotationLayer(layer);
            Color stroke = ParseColor(
                isAnnotationLayer ? layer?.LabelColor : layer?.BorderColor,
                ParseColor(canvasObject?.BorderColorOverride, shape.BorderColor));

            Color fill = ParseColor(
                layer?.FillColor,
                ParseColor(canvasObject?.FillColorOverride, shape.FillColor));

            int transparency = Math.Clamp(
                layer == null
                    ? canvasObject?.FillTransparencyOverride ?? 0
                    : layer.ShowFillTransparency ? layer.FillTransparency : 0,
                0,
                100);
            int alpha = (int)Math.Round(255 * ((100 - transparency) / 100.0));
            fill = Color.FromArgb(alpha, fill.R, fill.G, fill.B);

            if (layer?.IsLocked == true)
            {
                stroke = FadeLockedLayerColor(stroke);
                fill = FadeLockedLayerColor(fill);
            }

            double lineWeight = layer?.LineWeight ?? canvasObject?.LineWeightOverride ?? 1.0;
            string lineStyle = layer?.LineStyle ?? canvasObject?.LineStyleOverride ?? "Solid";
            double lineTypeScale = layer?.LineTypeScale ?? 1.0;
            bool hasStroke = lineWeight > 0 && stroke.A > 0;
            bool isPointStyle = layer != null && CanvasLayerTreeService.IsPointLayer(layer);

            FillMode fillMode = ResolveFillMode(layer, fill);

            return new VectorShapeStyle(
                stroke,
                fill,
                (float)Math.Max(0, lineWeight),
                hasStroke,
                ResolveDashStyle(lineStyle),
                (float)Math.Clamp(lineTypeScale, 0.1, 100.0),
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

        private static Color FadeLockedLayerColor(Color color)
        {
            if (color.A == 0)
                return color;

            int alpha = Math.Clamp(
                (int)Math.Round(color.A * LockedLayerColorAlphaFactor),
                24,
                color.A);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
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
            VectorRenderContext context,
            CanvasFeature? feature = null,
            CanvasLayer? layer = null,
            bool suppressParcelHighlight = false)
        {
            if (style.IsPointStyle && shape is CircleShape pointCircle)
            {
                DrawPointMarker(graphics, engine, pointCircle, style);
                return;
            }

            bool isCadastralParcelSelection = !suppressParcelHighlight && IsSelectedCadastralParcelFeature(feature, shape);
            bool useDefaultSelectionStyle = shape.IsSelected && !isCadastralParcelSelection;
            bool shouldStroke = style.HasStroke || shape.IsSelected;
            float width = useDefaultSelectionStyle
                ? SelectionLineWidthPx
                : Math.Max(0.25f, style.LineWidth);
            Color stroke = useDefaultSelectionStyle ? SelectionStrokeColor : style.StrokeColor;
            DashStyle dashStyle = useDefaultSelectionStyle ? DashStyle.Dash : style.DashStyle;
            Pen? pen = shouldStroke
                ? context.GetPen(
                    stroke,
                    width,
                    dashStyle,
                    useDefaultSelectionStyle ? 1.0f : style.LineTypeScale)
                : null;

            switch (shape)
            {
                case DonutPolygonShape donut:
                    DrawDonutPolygon(graphics, engine, donut, style, context, pen, isCadastralParcelSelection);
                    break;
                case PolylineShape polyline:
                    DrawPolyline(graphics, engine, polyline, style, context, pen, isCadastralParcelSelection);
                    break;
                case LineShape line:
                    DrawLine(graphics, engine, line, pen);
                    break;
                case RectangleShape rectangle:
                    DrawRectangle(graphics, engine, rectangle, style, context, pen, isCadastralParcelSelection);
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
                    DrawText(graphics, engine, text, style, context, layer);
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
            Pen? pen,
            bool drawParcelSelectionHighlight = false)
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

            using GraphicsPath path = polyline.CreateScreenPath(engine.WorldToScreen);
            if (path.PointCount == 0)
            {
                return;
            }

            RectangleF bounds = path.GetBounds();
            if (!IsValidPathBounds(bounds) ||
                Math.Abs(bounds.Left) > MaxGdiCoordinate ||
                Math.Abs(bounds.Top) > MaxGdiCoordinate ||
                Math.Abs(bounds.Right) > MaxGdiCoordinate ||
                Math.Abs(bounds.Bottom) > MaxGdiCoordinate)
            {
                return;
            }

            if (polyline.IsClosed &&
                polyline.Vertices.Count > 2 &&
                style.FillMode != FillMode.None &&
                IsValidRectangle(bounds))
            {
                FillClosedPath(graphics, path, bounds, style, context);
            }

            if (polyline.IsClosed &&
                polyline.Vertices.Count > 2 &&
                drawParcelSelectionHighlight &&
                IsValidRectangle(bounds))
            {
                DrawCadastralParcelSelectionHighlight(graphics, path, bounds);
            }

            if (pen == null)
            {
                return;
            }

            graphics.DrawPath(pen, path);
        }

        private static void DrawDonutPolygon(
            Graphics graphics,
            MapCanvasEngine engine,
            DonutPolygonShape donut,
            VectorShapeStyle style,
            VectorRenderContext context,
            Pen? pen,
            bool drawParcelSelectionHighlight = false)
        {
            if (donut.ExteriorRing.Count < 3)
            {
                return;
            }

            using GraphicsPath path = donut.CreateScreenPath(engine.WorldToScreen);
            if (path.PointCount == 0)
            {
                return;
            }

            RectangleF bounds = path.GetBounds();
            if (!IsValidPathBounds(bounds) ||
                Math.Abs(bounds.Left) > MaxGdiCoordinate ||
                Math.Abs(bounds.Top) > MaxGdiCoordinate ||
                Math.Abs(bounds.Right) > MaxGdiCoordinate ||
                Math.Abs(bounds.Bottom) > MaxGdiCoordinate)
            {
                return;
            }

            if (style.FillMode != FillMode.None && IsValidRectangle(bounds))
            {
                FillClosedPath(graphics, path, bounds, style, context);
            }

            if (drawParcelSelectionHighlight && IsValidRectangle(bounds))
            {
                DrawCadastralParcelSelectionHighlight(graphics, path, bounds);
            }

            if (pen != null)
            {
                graphics.DrawPath(pen, path);
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
            Pen? pen,
            bool drawParcelSelectionHighlight = false)
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

                if (drawParcelSelectionHighlight)
                {
                    DrawCadastralParcelSelectionHighlight(graphics, path, rect);
                }
            }
            else if (drawParcelSelectionHighlight)
            {
                using GraphicsPath path = new();
                path.AddRectangle(rect);
                DrawCadastralParcelSelectionHighlight(graphics, path, rect);
            }

            if (pen != null)
            {
                graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
        }

        private static bool IsSelectedCadastralParcelFeature(CanvasFeature? feature, IShape shape)
        {
            return shape.IsSelected &&
                   feature != null &&
                   string.Equals(feature.CanvasObject.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(feature.CanvasObject.GeometryMetadataJson) &&
                   feature.CanvasObject.GeometryMetadataJson.Contains("CadastralParcel", StringComparison.OrdinalIgnoreCase);
        }

        private static void DrawCadastralParcelSelectionHighlight(
            Graphics graphics,
            GraphicsPath path,
            RectangleF bounds)
        {
            if (!IsValidRectangle(bounds))
            {
                return;
            }

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(path, CombineMode.Intersect);

                using SolidBrush fillBrush = new(Color.FromArgb(42, 255, 193, 7));
                graphics.FillPath(fillBrush, path);

                float bufferWidth = Math.Clamp(
                    Math.Min(bounds.Width, bounds.Height) * 0.18f,
                    7.0f,
                    18.0f);
                using Pen bufferPen = new(Color.FromArgb(140, 255, 193, 7), bufferWidth)
                {
                    Alignment = PenAlignment.Center,
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                graphics.DrawPath(bufferPen, path);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private static void DrawCadastralParcelHighlightOnly(
            Graphics graphics,
            MapCanvasEngine engine,
            IShape shape)
        {
            switch (shape)
            {
                case DonutPolygonShape donut:
                {
                    if (donut.ExteriorRing.Count < 3) return;
                    using GraphicsPath path = donut.CreateScreenPath(engine.WorldToScreen);
                    if (path.PointCount == 0) return;
                    RectangleF bounds = path.GetBounds();
                    if (IsValidPathBounds(bounds) && IsValidRectangle(bounds) &&
                        Math.Abs(bounds.Left) <= MaxGdiCoordinate &&
                        Math.Abs(bounds.Top) <= MaxGdiCoordinate)
                        DrawCadastralParcelSelectionHighlight(graphics, path, bounds);
                    break;
                }
                case PolylineShape polyline:
                {
                    if (!polyline.IsClosed || polyline.Vertices.Count <= 2) return;
                    using GraphicsPath path = polyline.CreateScreenPath(engine.WorldToScreen);
                    if (path.PointCount == 0) return;
                    RectangleF bounds = path.GetBounds();
                    if (IsValidPathBounds(bounds) && IsValidRectangle(bounds) &&
                        Math.Abs(bounds.Left) <= MaxGdiCoordinate &&
                        Math.Abs(bounds.Top) <= MaxGdiCoordinate)
                        DrawCadastralParcelSelectionHighlight(graphics, path, bounds);
                    break;
                }
                case RectangleShape rectangle:
                {
                    PointF start = ToScreenPointF(engine.WorldToScreen(rectangle.Start));
                    PointF end = ToScreenPointF(engine.WorldToScreen(rectangle.End));
                    RectangleF rect = CreateScreenRectangle(start, end);
                    if (!IsValidRectangle(rect)) return;
                    using GraphicsPath path = new();
                    path.AddRectangle(rect);
                    DrawCadastralParcelSelectionHighlight(graphics, path, rect);
                    break;
                }
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

            if (style.FillMode == FillMode.Hatched)
            {
                DrawHatchPattern(graphics, path, style, context);
            }
        }

        private static void DrawHatchPattern(
            Graphics graphics,
            GraphicsPath path,
            VectorShapeStyle style,
            VectorRenderContext context)
        {
            Color foreColor = ResolveHatchColor(style);
            TextureBrush brush = context.GetTextureHatchBrush(
                style.HatchPattern,
                foreColor,
                style.HatchScale);
            brush.ResetTransform();
            graphics.FillPath(brush, path);
        }

        private static HatchStyle ResolveGdipHatchStyle(string hatchPattern) =>
            hatchPattern.Trim().ToUpperInvariant() switch
            {
                "ANSI31" => HatchStyle.ForwardDiagonal,
                "ANSI32" or "DIAGONAL-CROSS" => HatchStyle.DiagonalCross,
                "ANSI33" => HatchStyle.LightUpwardDiagonal,
                "ANSI34" => HatchStyle.LightDownwardDiagonal,
                "HORIZONTAL" => HatchStyle.Horizontal,
                "VERTICAL" => HatchStyle.Vertical,
                "CROSS" => HatchStyle.Cross,
                "DOTS" or "SAND" => HatchStyle.DottedGrid,
                "GRAVEL" or "CONCRETE" => HatchStyle.SmallGrid,
                "BRICK" => HatchStyle.HorizontalBrick,
                "NET" => HatchStyle.DiagonalCross,
                "EARTH" => HatchStyle.Weave,
                "WATER" or "WOOD" or "WAVE" => HatchStyle.Wave,
                _ => HatchStyle.ForwardDiagonal
            };

        private static Color ResolveHatchColor(VectorShapeStyle style)
        {
            Color baseColor = Color.FromArgb(255, style.FillColor.R, style.FillColor.G, style.FillColor.B);
            return Color.FromArgb(220, baseColor.R, baseColor.G, baseColor.B);
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
            VectorShapeStyle style,
            VectorRenderContext context,
            CanvasLayer? layer)
        {
            PointF position = ToScreenPointF(engine.WorldToScreen(text.Position));
            if (!IsValidPoint(position))
            {
                return;
            }

            bool isAnnotationLayer = layer != null && CanvasLayerTreeService.IsAnnotationLayer(layer);
            Color color = isAnnotationLayer
                ? ParseColor(layer!.LabelColor, style.StrokeColor)
                : text.FillColor == Color.Transparent
                ? style.StrokeColor
                : text.FillColor;
            if (layer?.IsLocked == true)
                color = FadeLockedLayerColor(color);

            using Font? layerFont = CreateAnnotationTextFont(layer, engine.ZoomScale);
            using StringFormat format = new()
            {
                Alignment = TextShape.ToStringAlignment(layer?.TextAlignment ?? text.HorizontalAlignment),
                LineAlignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.NoClip
            };

            Font effectiveFont = layerFont ?? text.Font;
            SizeF textSize = graphics.MeasureString(string.IsNullOrEmpty(text.Text) ? " " : text.Text, effectiveFont);
            float left = format.Alignment switch
            {
                StringAlignment.Center => position.X - textSize.Width / 2f,
                StringAlignment.Far => position.X - textSize.Width,
                _ => position.X
            };
            text.SetLastRenderedBounds(new RectangleF(left, position.Y, textSize.Width, textSize.Height));

            graphics.DrawString(text.Text, effectiveFont, context.GetSolidBrush(color), position, format);
        }

        private static Font? CreateAnnotationTextFont(CanvasLayer? layer, double zoomScale)
        {
            if (layer == null || !CanvasLayerTreeService.IsAnnotationLayer(layer))
                return null;

            string fontName = string.IsNullOrWhiteSpace(layer.LabelFontName)
                ? DefaultCanvasLabelFontName
                : layer.LabelFontName.Trim();
            double zoomFactor = double.IsFinite(zoomScale)
                ? Math.Clamp(zoomScale, MinLabelZoomFactor, MaxLabelZoomFactor)
                : 1.0;
            float fontSize = (float)Math.Clamp(
                (layer.LabelFontSize <= 0 ? 6.0 : layer.LabelFontSize) * zoomFactor,
                MinLabelFontSizePt,
                MaxScaledLabelFontSizePt);

            try
            {
                return new Font(fontName, fontSize);
            }
            catch
            {
                return new Font(DefaultCanvasLabelFontName, fontSize);
            }
        }

        private static Font CreateLayerLabelFont(CanvasLayer layer, double zoomScale)
        {
            string fontName = string.IsNullOrWhiteSpace(layer.LabelFontName)
                ? DefaultCanvasLabelFontName
                : layer.LabelFontName.Trim();
            float fontSize = ResolveLayerLabelFontSize(layer, zoomScale);

            try
            {
                return new Font(fontName, fontSize);
            }
            catch
            {
                return new Font(DefaultCanvasLabelFontName, fontSize);
            }
        }

        private static float ResolveLayerLabelFontSize(CanvasLayer layer, double zoomScale)
        {
            double baseSize = layer.LabelFontSize <= 0
                ? layer.LabelScaleWithZoom ? 2.0 : 6.0
                : layer.LabelFontSize;
            double maxSize = layer.LabelScaleWithZoom
                ? MaxScaledLabelFontSizePt
                : MaxFixedLabelFontSizePt;

            if (layer.LabelScaleWithZoom)
            {
                double zoomFactor = double.IsFinite(zoomScale)
                    ? Math.Clamp(zoomScale, MinLabelZoomFactor, MaxLabelZoomFactor)
                    : 1.0;
                baseSize *= zoomFactor;
            }

            return (float)Math.Clamp(baseSize, MinLabelFontSizePt, maxSize);
        }

        private void DrawLabelIfNeeded(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            CanvasFeature feature,
            CanvasLayer? layer,
            VectorRenderContext context,
            LabelFontCache labelFontCache)
        {
            if (layer?.ShowLabels != true ||
                CanvasLayerTreeService.IsAnnotationLayer(layer))
            {
                return;
            }

            string? labelText = ResolveLabelText(feature, layer);
            if (string.IsNullOrWhiteSpace(labelText))
            {
                return;
            }

            if (!TryResolveLabelAnchor(feature, visibleWorldBounds, out PointD labelAnchor))
            {
                return;
            }

            PointF position = ToScreenPointF(engine.WorldToScreen(labelAnchor));

            if (!IsValidPoint(position))
            {
                return;
            }

            Color labelColor = ParseColor(layer.LabelColor, Color.Black);
            if (layer.IsLocked)
                labelColor = FadeLockedLayerColor(labelColor);

            Font labelFont = labelFontCache.Get(layer, context.ZoomScale);
            System.Drawing.Text.TextRenderingHint previousTextRenderingHint = graphics.TextRenderingHint;
            graphics.TextRenderingHint = context.AntiAliasingEnabled
                ? System.Drawing.Text.TextRenderingHint.AntiAliasGridFit
                : System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            try
            {
                SizeF size = graphics.MeasureString(labelText, labelFont);
                graphics.DrawString(
                    labelText,
                    labelFont,
                    context.GetSolidBrush(labelColor),
                    position.X - size.Width / 2.0f,
                    position.Y - size.Height / 2.0f);
            }
            finally
            {
                graphics.TextRenderingHint = previousTextRenderingHint;
            }
        }

        private bool TryResolveLabelAnchor(
            CanvasFeature feature,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            anchor = default;
            Guid cacheKey = feature.CanvasObject.Id;
            RectangleD shapeBounds = feature.Shape.GetBoundingBox();
            bool shapeFullyVisible = ContainsBounds(visibleWorldBounds, shapeBounds);

            Geometry? geometry = feature.CanvasObject.Shape;
            if (!shapeFullyVisible && geometry != null)
            {
                if (TryResolveClippedGeometryLabelAnchor(geometry, visibleWorldBounds, out PointD clippedAnchor))
                {
                    anchor = clippedAnchor;
                    return true;
                }

                return false;
            }

            // Re-use the cached full-geometry interior point only when it is visible.
            if (cacheKey != Guid.Empty &&
                _labelAnchorCache.TryGetValue(cacheKey, out PointD cached) &&
                ContainsPoint(visibleWorldBounds, cached))
            {
                anchor = cached;
                return true;
            }

            if (geometry != null && !geometry.IsEmpty)
            {
                try
                {
                    NetTopologySuite.Geometries.Point point = geometry.PointOnSurface;
                    PointD pointAnchor = new(point.X, point.Y);
                    if (cacheKey != Guid.Empty)
                        _labelAnchorCache.TryAdd(cacheKey, pointAnchor);
                    if (ContainsPoint(visibleWorldBounds, pointAnchor))
                    {
                        anchor = pointAnchor;
                        return true;
                    }
                }
                catch
                {
                    // Fall back to the visible bounding-box centre below.
                }
            }

            anchor = ClampAnchorToViewport(shapeBounds, visibleWorldBounds);
            return ContainsPoint(visibleWorldBounds, anchor);
        }

        private static bool TryResolveClippedGeometryLabelAnchor(
            Geometry geometry,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            anchor = default;
            if (geometry.IsEmpty)
                return false;

            try
            {
                Geometry viewportGeometry = CreateViewportGeometry(visibleWorldBounds);
                Geometry clippedGeometry = geometry.Intersection(viewportGeometry);
                if (clippedGeometry.IsEmpty)
                    return false;

                NetTopologySuite.Geometries.Point centroid = clippedGeometry.Centroid;
                if (IsUsableLabelPoint(centroid, clippedGeometry, visibleWorldBounds))
                {
                    anchor = new PointD(centroid.X, centroid.Y);
                    return true;
                }

                NetTopologySuite.Geometries.Point interiorPoint = clippedGeometry.PointOnSurface;
                if (IsUsableLabelPoint(interiorPoint, clippedGeometry, visibleWorldBounds))
                {
                    anchor = new PointD(interiorPoint.X, interiorPoint.Y);
                    return true;
                }
            }
            catch
            {
                // Invalid source geometry or a topology edge case: use the visible bounds centre fallback.
            }

            return false;
        }

        private static Geometry CreateViewportGeometry(RectangleD visibleWorldBounds)
        {
            return LabelGeometryFactory.ToGeometry(new Envelope(
                MinX(visibleWorldBounds),
                MaxX(visibleWorldBounds),
                MinY(visibleWorldBounds),
                MaxY(visibleWorldBounds)));
        }

        private static bool IsUsableLabelPoint(
            NetTopologySuite.Geometries.Point point,
            Geometry geometry,
            RectangleD visibleWorldBounds)
        {
            if (point.IsEmpty ||
                !double.IsFinite(point.X) ||
                !double.IsFinite(point.Y))
            {
                return false;
            }

            PointD anchor = new(point.X, point.Y);
            if (!ContainsPoint(visibleWorldBounds, anchor))
                return false;

            return geometry.Covers(point) || geometry.Distance(point) <= 1e-7;
        }

        private static PointD ClampAnchorToViewport(RectangleD bounds, RectangleD visibleWorldBounds)
        {
            double left = Math.Max(MinX(bounds), MinX(visibleWorldBounds));
            double right = Math.Min(MaxX(bounds), MaxX(visibleWorldBounds));
            double bottom = Math.Max(MinY(bounds), MinY(visibleWorldBounds));
            double top = Math.Min(MaxY(bounds), MaxY(visibleWorldBounds));

            if (right <= left || top <= bottom)
                return new PointD(MinX(bounds) + Math.Abs(bounds.Width) / 2.0, MinY(bounds) + Math.Abs(bounds.Height) / 2.0);

            return new PointD(left + (right - left) / 2.0, bottom + (top - bottom) / 2.0);
        }

        private static bool ContainsBounds(RectangleD outer, RectangleD inner)
        {
            return MinX(inner) >= MinX(outer) &&
                   MaxX(inner) <= MaxX(outer) &&
                   MinY(inner) >= MinY(outer) &&
                   MaxY(inner) <= MaxY(outer);
        }

        private static bool ContainsPoint(RectangleD bounds, PointD point)
        {
            return point.X >= MinX(bounds) &&
                   point.X <= MaxX(bounds) &&
                   point.Y >= MinY(bounds) &&
                   point.Y <= MaxY(bounds);
        }

        private static double MinX(RectangleD bounds) => Math.Min(bounds.Left, bounds.Right);

        private static double MaxX(RectangleD bounds) => Math.Max(bounds.Left, bounds.Right);

        private static double MinY(RectangleD bounds) => Math.Min(bounds.Top, bounds.Bottom);

        private static double MaxY(RectangleD bounds) => Math.Max(bounds.Top, bounds.Bottom);


        private static string? ResolveLabelText(CanvasFeature feature, CanvasLayer layer)
        {
            string? labelField = layer.LabelField?.Trim();
            if (!string.IsNullOrWhiteSpace(labelField))
            {
                // "static:Some fixed text" — same text for every object on the layer.
                if (labelField.StartsWith("static:", StringComparison.OrdinalIgnoreCase))
                    return labelField["static:".Length..];

                string? presetTemplate = ResolveLabelPresetTemplate(labelField);
                if (!string.IsNullOrWhiteSpace(presetTemplate))
                    return ResolveLabelTemplate(feature, presetTemplate);

                if (labelField.StartsWith("template:", StringComparison.OrdinalIgnoreCase))
                    return ResolveLabelTemplate(feature, labelField["template:".Length..]);

                if (labelField.Contains('{') && labelField.Contains('}'))
                    return ResolveLabelTemplate(feature, labelField);

                return ResolveLabelFieldValue(feature, labelField);
            }

            if (!string.IsNullOrWhiteSpace(feature.CanvasObject.LabelText))
            {
                return feature.CanvasObject.LabelText;
            }

            return TryGetShapeProperty(feature, "LabelText", out object? fallback)
                ? fallback?.ToString()
                : null;
        }

        private static string? ResolveLabelPresetTemplate(string labelField)
        {
            string normalized = NormalizeLabelField(labelField);
            return normalized switch
            {
                "parcelnoareasqm" or "parcelnumberareasqm" =>
                    "{ParcelNo}\n{AreaSqm} sq.m",
                "parcelnoarearapd" or "parcelnumberarearapd" or "parcelnoarealocal" =>
                    "{ParcelNo}\n{AreaRAPD}",
                "mapsheetparcelnoareasqm" or "mapsheetnoparcelnoareasqm" =>
                    "{MapSheetNo}-{ParcelNo}\n{AreaSqm} sq.m",
                "mapsheetparcelnoarearapd" or "mapsheetnoparcelnoarearapd" =>
                    "{MapSheetNo}-{ParcelNo}\n{AreaRAPD}",
                "parcelnoownername" =>
                    "{ParcelNo}\n{OwnerName}",
                "parcelnolanduse" =>
                    "{ParcelNo}\n{LandUse}",
                _ => null
            };
        }

        private static string? ResolveLabelTemplate(CanvasFeature feature, string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                return null;

            string expanded = template.Replace("\\n", "\n", StringComparison.Ordinal);
            expanded = System.Text.RegularExpressions.Regex.Replace(
                expanded,
                @"\{(?<field>[^{}]+)\}",
                match =>
                {
                    string field = match.Groups["field"].Value.Trim();
                    return ResolveLabelFieldValue(feature, field) ?? string.Empty;
                });

            string[] lines = expanded
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            return lines.Length == 0 ? null : string.Join(Environment.NewLine, lines);
        }

        private static string? ResolveLabelFieldValue(CanvasFeature feature, string labelField)
        {
            if (string.IsNullOrWhiteSpace(labelField))
                return null;

            CanvasObject canvasObject = feature.CanvasObject;
            CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
            string normalized = NormalizeLabelField(labelField);

            object? value = normalized switch
            {
                "labeltext" => canvasObject.LabelText,
                "objectdescription" or "description" => canvasObject.ObjectDescription,
                "objecttype" => canvasObject.ObjectType,
                "layername" or "canvaslayer" => feature.Layer?.Name ?? canvasObject.CanvasLayer?.Name ?? feature.Shape.LayerName,
                "id" or "canvasobjectid" => canvasObject.Id,
                "baselineparcelid" => canvasObject.BaselineParcelId ?? metadata?.BaselineParcelId,
                "parcelno" or "parcelnumber" or "plotnumber" or "plotno" => ResolveFirst(
                    metadata?.ParcelNo,
                    GetPropertyValue(canvasObject.BaselineParcel, "ParcelNo"),
                    canvasObject.LabelText),
                "mapsheetno" or "mapsheet" or "sheetno" => ResolveFirst(
                    metadata?.MapSheetNo,
                    GetPropertyValue(canvasObject.BaselineParcel, "MapSheetNo")),
                "mapsheetparcelno" or "fulluniqueparcelcode" or "uniqueparcelcode" => ResolveFirst(
                    metadata?.FullUniqueParcelCode,
                    GetPropertyValue(canvasObject.BaselineParcel, "FullUniqueParcelCode"),
                    CombineMapSheetAndParcel(metadata)),
                "ownername" or "landowner" or "landownersname" => ResolveFirst(
                    metadata?.OwnerName,
                    GetPropertyValue(canvasObject.BaselineParcel?.LandOwner, "FullName")),
                "landuse" => ResolveFirst(
                    metadata?.LandUse,
                    GetPropertyValue(canvasObject.BaselineParcel, "LandUse")),
                "areasqm" or "originalareasqm" or "recordareasqm" => ResolveFirst(
                    metadata?.RecordAreaSqm,
                    GetPropertyValue(canvasObject.BaselineParcel, "OriginalAreaSqm"),
                    GetAttributeValue(metadata, labelField)),
                "arearapd" or "localarea" => FormatTraditionalArea(ResolveFirst(
                    metadata?.RecordAreaSqm,
                    GetPropertyValue(canvasObject.BaselineParcel, "OriginalAreaSqm"),
                    metadata?.CalculatedAreaSqm)),
                "areabkd" => FormatTraditionalArea(ResolveFirst(
                    metadata?.RecordAreaSqm,
                    GetPropertyValue(canvasObject.BaselineParcel, "OriginalAreaSqm"),
                    metadata?.CalculatedAreaSqm), useBkd: true),
                "effectiveareasqm" => GetPropertyValue(canvasObject.BaselineParcel, "EffectiveAreaSqm"),
                "calculatedareasqm" or "geometryareasqm" or "geometryarea" => ResolveFirst(
                    metadata?.CalculatedAreaSqm,
                    Math.Abs(canvasObject.Shape?.Area ?? feature.Shape.GetBoundingBox().Width * feature.Shape.GetBoundingBox().Height)),
                "assignmentstatus" or "status" => metadata?.AssignmentStatus,
                "sourcelayer" => metadata?.SourceLayer,
                "sourcefilename" or "sourcefile" => metadata?.SourceFileName,
                "sourceformat" => metadata?.SourceFormat,
                "matchedtext" => metadata?.MatchedText,
                _ => ResolveFirst(
                    GetAttributeValue(metadata, labelField),
                    GetPropertyValue(canvasObject.BaselineParcel, labelField),
                    GetPropertyValue(canvasObject.BaselineParcel?.LandOwner, labelField),
                    TryGetShapeProperty(feature, labelField, out object? shapeValue) ? shapeValue : null)
            };

            return FormatLabelValue(value);
        }

        private static string? FormatTraditionalArea(object? value, bool useBkd = false)
        {
            if (value == null)
                return null;

            if (!double.TryParse(
                    Convert.ToString(value, CultureInfo.InvariantCulture),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double areaSqm) ||
                areaSqm <= 0)
            {
                return null;
            }

            return useBkd
                ? AreaConverterService.SqmToBKDString(areaSqm)
                : AreaConverterService.SqmToRAPDString(areaSqm);
        }

        private static string? CombineMapSheetAndParcel(CadastralCanvasMetadata? metadata)
        {
            if (metadata == null ||
                string.IsNullOrWhiteSpace(metadata.MapSheetNo) ||
                string.IsNullOrWhiteSpace(metadata.ParcelNo))
            {
                return null;
            }

            return $"{metadata.MapSheetNo}-{metadata.ParcelNo}";
        }

        private static string NormalizeLabelField(string value)
        {
            return value
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace("+", string.Empty)
                .Trim()
                .ToLowerInvariant();
        }

        private static object? ResolveFirst(params object?[] values)
        {
            foreach (object? value in values)
            {
                if (value == null)
                    continue;

                if (value is string text && string.IsNullOrWhiteSpace(text))
                    continue;

                return value;
            }

            return null;
        }

        private static string? FormatLabelValue(object? value)
        {
            return value switch
            {
                null => null,
                string text => string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
                double number => number.ToString("0.##", CultureInfo.InvariantCulture),
                float number => number.ToString("0.##", CultureInfo.InvariantCulture),
                decimal number => number.ToString("0.##", CultureInfo.InvariantCulture),
                int number => number.ToString(CultureInfo.InvariantCulture),
                long number => number.ToString(CultureInfo.InvariantCulture),
                Guid guid => guid.ToString(),
                DateTime date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                JsonElement element => FormatJsonElement(element),
                _ => value.ToString()
            };
        }

        private static string? FormatJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number when element.TryGetDouble(out double number) =>
                    number.ToString("0.##", CultureInfo.InvariantCulture),
                JsonValueKind.True => "True",
                JsonValueKind.False => "False",
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => element.ToString()
            };
        }

        private static object? GetPropertyValue(object? source, string propertyName)
        {
            if (source == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            PropertyInfo? property = source
                .GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            return property?.GetValue(source);
        }

        private static bool TryGetShapeProperty(
            CanvasFeature feature,
            string propertyName,
            out object? value)
        {
            if (feature.Shape.Properties.TryGetValue(propertyName, out value))
                return true;

            foreach (var item in feature.Shape.Properties)
            {
                if (string.Equals(item.Key, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = item.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        private static object? GetAttributeValue(
            CadastralCanvasMetadata? metadata,
            string fieldName)
        {
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.AttributesJson))
                return null;

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadata.AttributesJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return null;

                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    if (string.Equals(property.Name, fieldName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(NormalizeLabelField(property.Name), NormalizeLabelField(fieldName), StringComparison.OrdinalIgnoreCase))
                    {
                        return property.Value.Clone();
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static CadastralCanvasMetadata? ReadCadastralMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                CadastralCanvasMetadata? metadata =
                    JsonSerializer.Deserialize<CadastralCanvasMetadata>(json);
                return string.Equals(metadata?.Kind, CadastralCanvasMetadata.MetadataKind, StringComparison.OrdinalIgnoreCase)
                    ? metadata
                    : null;
            }
            catch
            {
                return null;
            }
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

        private static bool IsValidPathBounds(RectangleF rectangle) =>
            IsValidPoint(rectangle.Location) &&
            float.IsFinite(rectangle.Width) &&
            float.IsFinite(rectangle.Height) &&
            rectangle.Width >= 0 &&
            rectangle.Height >= 0;

        private static float Distance(PointF a, PointF b)
        {
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }

        public void Dispose()
        {
            _previewFont.Dispose();
        }

        private enum FillMode
        {
            None,
            Solid,
            Hatched
        }

        private sealed class LabelFontCache : IDisposable
        {
            private readonly Dictionary<LabelFontKey, Font> _fonts = new();

            public Font Get(CanvasLayer layer, double zoomScale)
            {
                string fontName = string.IsNullOrWhiteSpace(layer.LabelFontName)
                    ? DefaultCanvasLabelFontName
                    : layer.LabelFontName.Trim();
                float fontSize = ResolveLayerLabelFontSize(layer, zoomScale);
                LabelFontKey key = new(fontName, fontSize);

                if (_fonts.TryGetValue(key, out Font? cached))
                    return cached;

                Font font = CreateLayerLabelFont(layer, zoomScale);
                _fonts[key] = font;
                return font;
            }

            public void Dispose()
            {
                foreach (Font font in _fonts.Values)
                    font.Dispose();

                _fonts.Clear();
            }
        }

        private readonly record struct LabelFontKey(string FontName, float FontSize);

        private readonly record struct VectorShapeStyle(
            Color StrokeColor,
            Color FillColor,
            float LineWidth,
            bool HasStroke,
            DashStyle DashStyle,
            float LineTypeScale,
            FillMode FillMode,
            string HatchPattern,
            float HatchScale,
            bool IsPointStyle,
            string PointSymbol,
            float PointSize);
    }
}
