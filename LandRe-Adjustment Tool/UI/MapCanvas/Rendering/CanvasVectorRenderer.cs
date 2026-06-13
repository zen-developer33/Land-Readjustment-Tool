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
        private const double ClipMarginScreenPixels = 64.0;
        private const double MinClipWorldMargin = 0.001;
        private const float HatchLineWidthPx = 0.65f;
        private const float LockedLayerColorAlphaFactor = 0.48f;
        private const float MinLabelFontSizePt = 1.0f;
        private const float MaxFixedLabelFontSizePt = 72.0f;
        private const float MaxScaledLabelFontSizePt = 120.0f;
        private const double MinLabelZoomFactor = 0.1;
        private const double MaxLabelZoomFactor = 12.0;
        private const double DeepZoomLabelViewportWorldSpan = 0.5;
        private static readonly Color SelectionStrokeColor = Color.FromArgb(0, 72, 255);
        private readonly Font _previewFont = new("Segoe UI", 8.0f, FontStyle.Bold);
        private readonly VectorFeatureSpatialIndex _featureSpatialIndex = new();
        private IReadOnlyList<CanvasFeature> _features = Array.Empty<CanvasFeature>();
        private IReadOnlyDictionary<int, CanvasLayer> _layersById =
            new Dictionary<int, CanvasLayer>();
        private IReadOnlySet<Guid> _excludedShapeIds = new HashSet<Guid>();
        private VectorRenderStats _lastRenderStats = VectorRenderStats.Empty;
        private readonly ConcurrentDictionary<Guid, PointD> _labelAnchorCache = new();
        private static readonly GeometryFactory LabelGeometryFactory = new(new PrecisionModel(), 0);
        private int _sqmPrecision = 3;
        private int _traditionalPrecision = 2;

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

        public void UpdateAreaPrecisionSettings(int sqmPrecision, int traditionalPrecision)
        {
            _sqmPrecision = sqmPrecision;
            _traditionalPrecision = traditionalPrecision;
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

        /// <summary>
        /// Returns the features whose bounding box intersects the given world
        /// bounds using the spatial index (O(log n + k)). Used for per-mouse-move
        /// queries such as snapping instead of a linear scan over every feature.
        /// </summary>
        public IReadOnlyList<CanvasFeature> QueryFeatures(RectangleD worldBounds) =>
            _featureSpatialIndex.Query(worldBounds);

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
                antiAliasingEnabled,
                clipWorldBounds: CreateClipWorldBounds(engine));

            Stopwatch queryStopwatch = Stopwatch.StartNew();
            IReadOnlyList<CanvasFeature> queriedFeatures =
                _featureSpatialIndex.Query(visibleWorldBounds);
            queryStopwatch.Stop();

            int renderedCount = 0;
            int hiddenSkippedCount = 0;
            int lodSkippedCount = 0;

            List<CanvasFeature> orderedFeatures = queriedFeatures
                .OrderBy(GetDrawingMarkupRenderPass)
                .ThenBy(GetCadastralParcelRenderPass)
                .ThenBy(GetProjectBoundaryRenderPass)
                .ThenBy(GetOpenSpaceRenderPass)
                .ThenBy(GetDisplayOrder)
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

                RectangleD featureBounds = feature.Shape.GetBoundingBox();
                if (minimumVisibleWorldSize > 0.0 &&
                    (featureBounds.Width > 0 || featureBounds.Height > 0) &&
                    IsBelowLevelOfDetail(featureBounds, minimumVisibleWorldSize))
                {
                    lodSkippedCount++;
                    continue;
                }

                // The cache is intentionally selection-free; selection
                // decoration is drawn later as a small interaction overlay.
                DrawShape(
                    graphics,
                    engine,
                    feature.Shape,
                    ResolveStyle(feature, layer),
                    context,
                    feature,
                    layer,
                    suppressParcelHighlight: true,
                    forceUnselected: true);
                DrawLabelIfNeeded(graphics, engine, visibleWorldBounds, feature, layer, context, labelFontCache);
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
            CanvasLayer? previewLayer,
            CanvasObject? canvasObject = null,
            bool drawAsPreview = true,
            bool forceUnselected = false)
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
                isPreview: drawAsPreview,
                clipWorldBounds: CreateClipWorldBounds(engine));

            DrawShape(
                graphics,
                engine,
                previewShape,
                ResolveStyle(previewShape, previewLayer, canvasObject),
                context,
                layer: previewLayer,
                forceUnselected: forceUnselected);

            if (drawAsPreview && previewShape is CircleShape circle)
            {
                DrawCircleRadiusPreview(graphics, engine, circle, context);
            }
        }

        /// <summary>
        /// Live-renders a batch of transient shapes (e.g. the shapes being moved)
        /// using a SINGLE shared pen/brush cache and render context, so per-frame
        /// allocation stays flat regardless of how many shapes are moving.
        /// </summary>
        public void RenderTransientShapes(
            Graphics graphics,
            MapCanvasEngine engine,
            IReadOnlyList<(IShape Shape, CanvasLayer? Layer, CanvasObject? CanvasObject)> shapes,
            bool forceUnselected = false)
        {
            if (shapes == null || shapes.Count == 0)
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
                isPreview: false,
                clipWorldBounds: CreateClipWorldBounds(engine));

            foreach ((IShape shape, CanvasLayer? layer, CanvasObject? canvasObject) in shapes)
            {
                if (shape == null)
                {
                    continue;
                }

                DrawShape(
                    graphics,
                    engine,
                    shape,
                    ResolveStyle(shape, layer, canvasObject),
                    context,
                    layer: layer,
                    forceUnselected: forceUnselected);
            }
        }

        /// <summary>
        /// Draws only the selection decoration (highlight glow for data-layer
        /// polygons, or the selection stroke for markup shapes) for a single
        /// selected feature, on top of the cached vector frame. Interior fills
        /// and hatch are intentionally skipped so the overlay never doubles the
        /// cached frame's fill opacity. This lets selection feedback appear
        /// instantly without rebuilding the whole vector bitmap cache.
        /// </summary>
        public void RenderSelectionDecoration(
            Graphics graphics,
            MapCanvasEngine engine,
            IShape? shape,
            CanvasLayer? layer,
            CanvasFeature? feature)
        {
            if (shape == null || !shape.IsSelected)
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
                isPreview: false,
                selectionDecorationOnly: true,
                clipWorldBounds: CreateClipWorldBounds(engine));

            DrawShape(
                graphics,
                engine,
                shape,
                ResolveStyle(shape, layer, feature?.CanvasObject),
                context,
                feature,
                layer,
                suppressParcelHighlight: false);
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

        private int GetDrawingMarkupRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveLayer(feature);
            return layer != null && CanvasLayerTreeService.IsDrawingMarkupLayer(layer) ? 1 : 0;
        }

        private int GetProjectBoundaryRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveLayer(feature);
            return layer != null && CanvasLayerTreeService.IsProjectBoundaryLayer(layer) ? 1 : 0;
        }

        private int GetOpenSpaceRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveLayer(feature);
            return IsOpenSpaceLayer(layer) ? 1 : 0;
        }

        private int GetCadastralParcelRenderPass(CanvasFeature feature)
        {
            CanvasLayer? layer = ResolveLayer(feature);
            return IsImportedCadastralParcelFeature(feature, layer) ? 0 : 1;
        }

        private int GetDisplayOrder(CanvasFeature feature) =>
            ResolveLayer(feature)?.DisplayOrder ?? int.MaxValue;

        private static bool IsImportedCadastralParcelFeature(CanvasFeature feature, CanvasLayer? layer)
        {
            return string.Equals(feature.CanvasObject.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   ((layer?.Description?.StartsWith(
                         "Imported cadastral map layer",
                         StringComparison.OrdinalIgnoreCase) == true) ||
                    (!string.IsNullOrWhiteSpace(feature.CanvasObject.GeometryMetadataJson) &&
                     feature.CanvasObject.GeometryMetadataJson.Contains("CadastralParcel", StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsOpenSpaceLayer(CanvasLayer? layer)
        {
            return layer != null &&
                   (string.Equals(layer.LayerType, "OpenSpace", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(layer.Name, "Open Spaces/Parks", StringComparison.OrdinalIgnoreCase));
        }

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
            bool suppressParcelHighlight = false,
            bool forceUnselected = false)
        {
            // When forceUnselected is set the scene cache renders selection-free
            // so selection can be drawn as a live overlay without rebuilding the
            // whole bitmap. Selection decoration is handled separately.
            bool effectiveSelected = shape.IsSelected && !forceUnselected;

            if (style.IsPointStyle && shape is CircleShape pointCircle)
            {
                DrawPointMarker(graphics, engine, pointCircle.Center, style, effectiveSelected);
                return;
            }

            bool isDataLayerPolygon = !forceUnselected && IsDataLayerPolygonFeature(feature, shape, layer);
            bool isCadastralParcelSelection = !suppressParcelHighlight && isDataLayerPolygon;

            // Drawing/markup, external and road-centerline shapes keep their
            // normal outline (same color, weight and line type) and add a soft
            // glow offset to BOTH sides of the outline. Cadastral data-layer
            // polygons keep their interior highlight instead.
            bool drawSelectionGlow = effectiveSelected && !isDataLayerPolygon;

            bool shouldStroke = style.HasStroke;
            float width = Math.Max(0.25f, style.LineWidth);
            Pen? pen = shouldStroke
                ? context.GetPen(
                    style.StrokeColor,
                    width,
                    style.DashStyle,
                    style.LineTypeScale)
                : null;

            switch (shape)
            {
                case DonutPolygonShape donut:
                    DrawDonutPolygon(graphics, engine, donut, style, context, pen, isCadastralParcelSelection);
                    break;
                case PolylineShape polyline:
                    DrawPolyline(graphics, engine, polyline, style, context, pen, isCadastralParcelSelection, effectiveSelected);
                    break;
                case LineShape line:
                    DrawLine(graphics, engine, line, context, pen);
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
                    DrawText(graphics, engine, text, style, context, layer, effectiveSelected);
                    break;
                default:
                    shape.Draw(graphics, engine.WorldToScreen, context.IsPreview);
                    break;
            }

            // Draw the selection glow ON TOP of the shape (AutoCAD-style), as a
            // translucent halo that covers the outline and extends beyond it on
            // both sides. Sized from the object's own stroke width so the total
            // glow span is the line weight + ~2px on each side, staying visible
            // regardless of how thick the line weight is.
            if (drawSelectionGlow)
            {
                DrawSelectionGlow(graphics, engine, shape, context, style, width);
            }
        }

        // Selection halo color — same pure blue as the selection stroke.
        private static readonly Color SelectionGlowColor = SelectionStrokeColor;

        private static readonly (float ExtraWidth, int Alpha)[] SelectionOutlineGlowBands =
        {
            (1.25f, 55),
            (0.5f, 110)
        };

        private static void DrawSelectionGlow(
            Graphics graphics,
            MapCanvasEngine engine,
            IShape shape,
            VectorRenderContext context,
            VectorShapeStyle style,
            float strokeWidth)
        {
            using GraphicsPath? path = TryBuildSelectionOutlinePath(
                shape,
                engine,
                context.ClipWorldBounds);
            if (path == null || path.PointCount == 0)
            {
                return;
            }

            RectangleF bounds = path.GetBounds();
            if (!IsValidPathBounds(bounds))
            {
                return;
            }

            DrawSelectionOutlineGlow(graphics, path, context, style, strokeWidth);
        }

        private static void DrawSelectionOutlineGlow(
            Graphics graphics,
            GraphicsPath path,
            VectorRenderContext context,
            VectorShapeStyle style,
            float strokeWidth)
        {
            GraphicsState state = graphics.Save();
            try
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                float effectiveStroke = Math.Max(0.25f, strokeWidth);
                if (!IsLinearSelectionOutline(path))
                {
                    foreach ((float extraWidth, int alpha) in SelectionOutlineGlowBands)
                    {
                        float selectionWidth = effectiveStroke + extraWidth;
                        Pen glowPen = context.GetPen(
                            Color.FromArgb(alpha, SelectionGlowColor),
                            selectionWidth,
                            style.DashStyle,
                            GetSelectionDashScale(style.LineTypeScale, effectiveStroke, selectionWidth));
                        graphics.DrawPath(glowPen, path);
                    }
                }

                Pen centerPen = context.GetPen(
                    SelectionStrokeColor,
                    effectiveStroke,
                    style.DashStyle,
                    style.LineTypeScale);
                graphics.DrawPath(centerPen, path);
            }
            catch (OutOfMemoryException)
            {
                // Ignore the glow pass for degenerate paths.
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private static bool IsLinearSelectionOutline(GraphicsPath path)
        {
            PointF[] points = path.PathPoints;
            if (points.Length < 2)
            {
                return false;
            }

            byte[] types = path.PathTypes;
            return (types[^1] & (byte)PathPointType.CloseSubpath) == 0;
        }

        private static float GetSelectionDashScale(
            float objectLineTypeScale,
            float objectStrokeWidth,
            float selectionStrokeWidth)
        {
            if (selectionStrokeWidth <= 0.0f)
            {
                return objectLineTypeScale;
            }

            // GDI+ dash pattern values are multiplied by pen width. The selection
            // glow pen is wider than the object pen, so compensate to preserve
            // the object's effective linetype scale on screen.
            return objectLineTypeScale * (objectStrokeWidth / selectionStrokeWidth);
        }

        /// <summary>
        /// Builds a screen-space outline path for the supplied shape, used to
        /// stroke the selection glow. Returns null for shapes that do not have a
        /// strokeable outline (e.g. text).
        /// </summary>
        private static GraphicsPath? TryBuildSelectionOutlinePath(
            IShape shape,
            MapCanvasEngine engine,
            RectangleD? clipWorldBounds)
        {
            switch (shape)
            {
                case PolylineShape polyline:
                {
                    if (polyline.Vertices.Count < 2)
                    {
                        return null;
                    }

                    GraphicsPath path = polyline.CreateScreenPath(engine.WorldToScreen, clipWorldBounds);
                    return path;
                }
                case DonutPolygonShape donut:
                {
                    if (donut.ExteriorRing.Count < 3)
                    {
                        return null;
                    }

                    return donut.CreateScreenPath(engine.WorldToScreen, clipWorldBounds);
                }
                case RectangleShape rectangle:
                {
                    return CreateRectanglePath(engine, rectangle, clipWorldBounds);
                }
                case LineShape line:
                {
                    PointD startWorld = line.Start;
                    PointD endWorld = line.End;
                    if (clipWorldBounds.HasValue &&
                        !ViewportClip.ClipSegment(startWorld, endWorld, clipWorldBounds.Value, out startWorld, out endWorld))
                    {
                        return null;
                    }

                    PointF ls = ToScreenPointF(engine.WorldToScreen(startWorld));
                    PointF le = ToScreenPointF(engine.WorldToScreen(endWorld));
                    if (!IsValidPoint(ls) || !IsValidPoint(le) || Distance(ls, le) < 0.5f)
                    {
                        return null;
                    }

                    GraphicsPath path = new();
                    path.AddLine(ls, le);
                    return path;
                }
                case CircleShape circle:
                {
                    PointF center = ToScreenPointF(engine.WorldToScreen(circle.Center));
                    PointF edge = ToScreenPointF(engine.WorldToScreen(circle.RadiusPoint));
                    float radius = Distance(center, edge);
                    RectangleF rect = new(center.X - radius, center.Y - radius, radius * 2.0f, radius * 2.0f);
                    if (!IsValidRectangle(rect))
                    {
                        return null;
                    }

                    GraphicsPath path = new();
                    path.AddEllipse(rect);
                    return path;
                }
                case EllipseShape ellipse:
                {
                    PointF es = ToScreenPointF(engine.WorldToScreen(ellipse.Start));
                    PointF ee = ToScreenPointF(engine.WorldToScreen(ellipse.End));
                    RectangleF rect = CreateScreenRectangle(es, ee);
                    if (!IsValidRectangle(rect))
                    {
                        return null;
                    }

                    GraphicsPath path = new();
                    path.AddEllipse(rect);
                    return path;
                }
                case ArcShape arc:
                {
                    if (arc.Radius <= 0.0 || !double.IsFinite(arc.Radius))
                    {
                        return null;
                    }

                    PointF center = ToScreenPointF(engine.WorldToScreen(arc.Center));
                    PointF radiusPoint = ToScreenPointF(engine.WorldToScreen(
                        new PointD(arc.Center.X + arc.Radius, arc.Center.Y)));
                    float radius = Distance(center, radiusPoint);
                    if (!IsValidPoint(center) || radius <= 0.5f || !float.IsFinite(radius))
                    {
                        return null;
                    }

                    RectangleF bounds = new(center.X - radius, center.Y - radius, radius * 2.0f, radius * 2.0f);
                    if (!IsValidRectangle(bounds))
                    {
                        return null;
                    }

                    float startAngle = (float)(-arc.StartAngleRadians * 180.0 / Math.PI);
                    float sweepAngle = (float)(-arc.SweepAngleRadians * 180.0 / Math.PI);
                    if (!float.IsFinite(startAngle) || !float.IsFinite(sweepAngle) || Math.Abs(sweepAngle) < 0.001f)
                    {
                        return null;
                    }

                    GraphicsPath path = new();
                    path.AddArc(bounds, startAngle, sweepAngle);
                    return path;
                }
                default:
                    return null;
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
            bool drawParcelSelectionHighlight = false,
            bool effectiveSelected = false)
        {
            if (polyline.Vertices.Count == 1)
            {
                DrawPointMarker(graphics, engine, polyline.Vertices[0], style, effectiveSelected);
                return;
            }

            if (polyline.Vertices.Count < 2)
            {
                return;
            }

            using GraphicsPath path = polyline.CreateScreenPath(
                engine.WorldToScreen,
                context.ClipWorldBounds);
            if (path.PointCount == 0)
            {
                return;
            }

            RectangleF bounds = path.GetBounds();
            if (!IsValidPathBounds(bounds))
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

            using GraphicsPath path = donut.CreateScreenPath(
                engine.WorldToScreen,
                context.ClipWorldBounds);
            if (path.PointCount == 0)
            {
                return;
            }

            RectangleF bounds = path.GetBounds();
            if (!IsValidPathBounds(bounds))
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
            VectorRenderContext context,
            Pen? pen)
        {
            if (pen == null)
                return;

            PointD startWorld = line.Start;
            PointD endWorld = line.End;
            if (context.ClipWorldBounds.HasValue &&
                !ViewportClip.ClipSegment(startWorld, endWorld, context.ClipWorldBounds.Value, out startWorld, out endWorld))
            {
                return;
            }

            PointF start = ToScreenPointF(engine.WorldToScreen(startWorld));
            PointF end = ToScreenPointF(engine.WorldToScreen(endWorld));
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
            using GraphicsPath? path = CreateRectanglePath(
                engine,
                rectangle,
                context.ClipWorldBounds);
            if (path == null || path.PointCount == 0)
            {
                return;
            }

            RectangleF rect = path.GetBounds();
            if (!IsValidPathBounds(rect) || !IsValidRectangle(rect))
            {
                return;
            }

            if (style.FillMode != FillMode.None)
            {
                FillClosedPath(graphics, path, rect, style, context);

                if (drawParcelSelectionHighlight)
                {
                    DrawCadastralParcelSelectionHighlight(graphics, path, rect);
                }
            }
            else if (drawParcelSelectionHighlight)
            {
                DrawCadastralParcelSelectionHighlight(graphics, path, rect);
            }

            if (pen != null)
            {
                graphics.DrawPath(pen, path);
            }
        }

        private static bool IsDataLayerPolygonFeature(CanvasFeature? feature, IShape shape, CanvasLayer? layer = null)
        {
            if (layer != null && CanvasLayerTreeService.IsProjectBoundaryLayer(layer))
            {
                return false;
            }

            return shape.IsSelected &&
                   feature != null &&
                   string.Equals(feature.CanvasObject.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase) &&
                   (layer == null || (!CanvasLayerTreeService.IsAnnotationLayer(layer) &&
                                      !CanvasLayerTreeService.IsDrawingMarkupLayer(layer) &&
                                      !CanvasLayerTreeService.IsExternalImportedLayer(layer)));
        }

        // High-contrast selection blue, tuned to stay visible over pastel map fills.
        private static readonly Color SelectionHighlightColor = Color.FromArgb(0, 96, 255);

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
                // Transparent fill — slightly more visible than before.
                using SolidBrush fillBrush = new(Color.FromArgb(105, SelectionHighlightColor));
                graphics.FillPath(fillBrush, path);

                // Inner glow — half the previous width, clipped so nothing bleeds outside.
                float glowWidth = Math.Clamp(
                    Math.Min(bounds.Width, bounds.Height) * 0.065f,
                    3.0f,
                    7.0f);

                graphics.SetClip(path, System.Drawing.Drawing2D.CombineMode.Intersect);

                // Outer zone (25% opacity): covers full glowWidth inward from the border.
                using Pen outerPen = new(Color.FromArgb(135, SelectionHighlightColor), glowWidth * 2.4f)
                {
                    Alignment = PenAlignment.Center,
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                graphics.DrawPath(outerPen, path);

                // Near-border zone (75% opacity): tight band right at the border edge.
                using Pen innerPen = new(Color.FromArgb(255, SelectionHighlightColor), glowWidth)
                {
                    Alignment = PenAlignment.Center,
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                graphics.DrawPath(innerPen, path);
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
                    RectangleD clipWorldBounds = CreateClipWorldBounds(engine);
                    using GraphicsPath path = donut.CreateScreenPath(engine.WorldToScreen, clipWorldBounds);
                    if (path.PointCount == 0) return;
                    RectangleF bounds = path.GetBounds();
                    if (IsValidPathBounds(bounds) && IsValidRectangle(bounds))
                        DrawCadastralParcelSelectionHighlight(graphics, path, bounds);
                    break;
                }
                case PolylineShape polyline:
                {
                    if (!polyline.IsClosed || polyline.Vertices.Count <= 2) return;
                    RectangleD clipWorldBounds = CreateClipWorldBounds(engine);
                    using GraphicsPath path = polyline.CreateScreenPath(engine.WorldToScreen, clipWorldBounds);
                    if (path.PointCount == 0) return;
                    RectangleF bounds = path.GetBounds();
                    if (IsValidPathBounds(bounds) && IsValidRectangle(bounds))
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

            // Selection-decoration overlays paint on top of a frame that is
            // already filled, so skip interior fills/hatch to avoid doubling.
            if (context.SelectionDecorationOnly)
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
            CanvasLayer? layer,
            bool effectiveSelected = false)
        {
            if (text.IsBeingEdited)
                return;

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
            if (effectiveSelected)
                color = SelectionHighlightColor;

            using Font? layerFont = CreateAnnotationTextFont(layer, engine.ZoomScale);

            string combinedAlignment = layer?.TextAlignment ?? text.HorizontalAlignment;
            (StringAlignment hAlign, StringAlignment vAlign) = ParseCombinedTextAlignment(combinedAlignment);
            using StringFormat format = new()
            {
                Alignment = hAlign,
                LineAlignment = vAlign,
                FormatFlags = StringFormatFlags.NoClip
            };

            Font effectiveFont = layerFont ?? text.Font;
            SizeF textSize = graphics.MeasureString(string.IsNullOrEmpty(text.Text) ? " " : text.Text, effectiveFont);
            float left = hAlign switch
            {
                StringAlignment.Center => position.X - textSize.Width / 2f,
                StringAlignment.Far => position.X - textSize.Width,
                _ => position.X
            };
            float top = vAlign switch
            {
                StringAlignment.Center => position.Y - textSize.Height / 2f,
                StringAlignment.Far => position.Y - textSize.Height,
                _ => position.Y
            };
            text.SetLastRenderedBounds(new RectangleF(left, top, textSize.Width, textSize.Height));

            graphics.DrawString(text.Text, effectiveFont, context.GetSolidBrush(color), position, format);
        }

        private static (StringAlignment Horizontal, StringAlignment Vertical) ParseCombinedTextAlignment(string? alignment)
        {
            if (string.IsNullOrWhiteSpace(alignment))
                return (StringAlignment.Near, StringAlignment.Near);

            string[] parts = alignment.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            StringAlignment h = (parts.Length > 0 ? parts[0] : "").ToLowerInvariant() switch
            {
                "center" or "centre" => StringAlignment.Center,
                "right" => StringAlignment.Far,
                _ => StringAlignment.Near
            };
            StringAlignment v = (parts.Length > 1 ? parts[1] : "").ToLowerInvariant() switch
            {
                "middle" or "center" or "centre" => StringAlignment.Center,
                "bottom" => StringAlignment.Far,
                _ => StringAlignment.Near
            };
            return (h, v);
        }

        private static Font? CreateAnnotationTextFont(CanvasLayer? layer, double zoomScale)
        {
            if (layer == null || !CanvasLayerTreeService.IsAnnotationLayer(layer))
                return null;

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
                ? GetDefaultLayerLabelFontSize(layer)
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

        private static double GetDefaultLayerLabelFontSize(CanvasLayer layer)
        {
            if (CanvasLayerTreeService.IsAnnotationLayer(layer))
                return 10.0;

            return layer.LabelScaleWithZoom ? 2.0 : 6.0;
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

            string? labelText = ResolveLabelText(feature, layer, _sqmPrecision, _traditionalPrecision);
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
                string alignStr = string.IsNullOrWhiteSpace(layer.TextAlignment)
                    ? "Center Middle"
                    : layer.TextAlignment;
                (StringAlignment hAlign, StringAlignment vAlign) = ParseCombinedTextAlignment(alignStr);

                using StringFormat sf = new()
                {
                    Alignment     = hAlign,
                    LineAlignment = vAlign,
                    FormatFlags   = StringFormatFlags.NoClip
                };

                SizeF size = graphics.MeasureString(labelText, labelFont);
                // Use a layout rectangle wide enough that the chosen horizontal alignment
                // independently aligns each line of text around the anchor point.
                float layoutWidth = Math.Max(size.Width * 2f, 400f);
                float layoutX = hAlign switch
                {
                    StringAlignment.Center => position.X - layoutWidth / 2f,
                    StringAlignment.Far    => position.X - layoutWidth,
                    _                      => position.X,
                };
                float layoutY = vAlign switch
                {
                    StringAlignment.Center => position.Y - size.Height / 2f,
                    StringAlignment.Far    => position.Y - size.Height,
                    _                      => position.Y,
                };
                RectangleF layoutRect = new(layoutX, layoutY, layoutWidth, size.Height);
                if (TryResolveLinearLabelAngle(feature, labelAnchor, engine, out float labelAngleDegrees))
                {
                    DrawRotatedLabel(
                        graphics,
                        labelText,
                        labelFont,
                        context.GetSolidBrush(labelColor),
                        position,
                        layoutRect,
                        sf,
                        labelAngleDegrees);
                }
                else
                {
                    graphics.DrawString(labelText, labelFont, context.GetSolidBrush(labelColor), layoutRect, sf);
                }
            }
            finally
            {
                graphics.TextRenderingHint = previousTextRenderingHint;
            }
        }

        private static void DrawRotatedLabel(
            Graphics graphics,
            string text,
            Font font,
            Brush brush,
            PointF position,
            RectangleF layoutRect,
            StringFormat format,
            float angleDegrees)
        {
            GraphicsState state = graphics.Save();
            try
            {
                graphics.TranslateTransform(position.X, position.Y);
                graphics.RotateTransform(angleDegrees);
                RectangleF localRect = new(
                    layoutRect.X - position.X,
                    layoutRect.Y - position.Y,
                    layoutRect.Width,
                    layoutRect.Height);
                graphics.DrawString(text, font, brush, localRect, format);
            }
            finally
            {
                graphics.Restore(state);
            }
        }

        private static bool TryResolveLinearLabelAngle(
            CanvasFeature feature,
            PointD labelAnchor,
            MapCanvasEngine engine,
            out float angleDegrees)
        {
            angleDegrees = 0.0f;
            return feature.Shape switch
            {
                LineShape line => TryResolveReadableScreenAngle(line.Start, line.End, engine, out angleDegrees),
                ArcShape arc => TryResolveArcTangentScreenAngle(arc, labelAnchor, engine, out angleDegrees),
                PolylineShape { IsClosed: false } polyline =>
                    TryResolvePolylineLabelAngle(polyline, labelAnchor, engine, out angleDegrees),
                _ => TryResolveLineGeometryAngle(feature.CanvasObject.Shape, labelAnchor, engine, out angleDegrees)
            };
        }

        private static bool TryResolvePolylineLabelAngle(
            PolylineShape polyline,
            PointD labelAnchor,
            MapCanvasEngine engine,
            out float angleDegrees)
        {
            angleDegrees = 0.0f;
            if (polyline.Vertices.Count < 2)
                return false;

            PolylineShape.PolylineSegment? bestSegment = null;
            double bestScore = double.PositiveInfinity;

            foreach (PolylineShape.PolylineSegment segment in EnumeratePolylineLabelSegments(polyline))
            {
                double score = segment.Kind == PolylineShape.PolylineSegmentKind.Arc && segment.Arc != null
                    ? DistanceToArcSquared(labelAnchor, segment.Arc)
                    : DistanceToSegmentSquared(labelAnchor, segment.Start, segment.End);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestSegment = segment;
                }
            }

            if (bestSegment == null)
                return false;

            if (bestSegment.Kind == PolylineShape.PolylineSegmentKind.Arc && bestSegment.Arc != null)
            {
                return TryResolveArcTangentScreenAngle(bestSegment.Arc, labelAnchor, engine, out angleDegrees);
            }

            return TryResolveReadableScreenAngle(bestSegment.Start, bestSegment.End, engine, out angleDegrees);
        }

        private static IEnumerable<PolylineShape.PolylineSegment> EnumeratePolylineLabelSegments(PolylineShape polyline)
        {
            if (polyline.Segments.Count > 0)
            {
                foreach (PolylineShape.PolylineSegment segment in polyline.Segments)
                    yield return segment;

                yield break;
            }

            for (int i = 1; i < polyline.Vertices.Count; i++)
            {
                yield return new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    polyline.Vertices[i - 1],
                    polyline.Vertices[i]);
            }
        }

        private static bool TryResolveLineGeometryAngle(
            Geometry? geometry,
            PointD labelAnchor,
            MapCanvasEngine engine,
            out float angleDegrees)
        {
            angleDegrees = 0.0f;
            if (geometry == null || geometry.IsEmpty)
                return false;

            LineString? bestLine = null;
            int bestSegmentIndex = -1;
            double bestScore = double.PositiveInfinity;

            foreach (LineString line in EnumerateLineStrings(geometry))
            {
                Coordinate[] coordinates = line.Coordinates;
                for (int i = 1; i < coordinates.Length; i++)
                {
                    PointD start = new(coordinates[i - 1].X, coordinates[i - 1].Y);
                    PointD end = new(coordinates[i].X, coordinates[i].Y);
                    double score = DistanceToSegmentSquared(labelAnchor, start, end);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestLine = line;
                        bestSegmentIndex = i;
                    }
                }
            }

            if (bestLine == null || bestSegmentIndex <= 0)
                return false;

            Coordinate a = bestLine.Coordinates[bestSegmentIndex - 1];
            Coordinate b = bestLine.Coordinates[bestSegmentIndex];
            return TryResolveReadableScreenAngle(
                new PointD(a.X, a.Y),
                new PointD(b.X, b.Y),
                engine,
                out angleDegrees);
        }

        private static bool TryResolveArcTangentScreenAngle(
            ArcShape arc,
            PointD labelAnchor,
            MapCanvasEngine engine,
            out float angleDegrees)
        {
            angleDegrees = 0.0f;
            if (arc.Radius <= 0.0 ||
                !double.IsFinite(arc.Radius) ||
                Math.Abs(arc.SweepAngleRadians) <= 1e-9)
            {
                return false;
            }

            double anchorAngle = Math.Atan2(labelAnchor.Y - arc.Center.Y, labelAnchor.X - arc.Center.X);
            double fraction = ResolveArcFraction(anchorAngle, arc.StartAngleRadians, arc.SweepAngleRadians);
            fraction = Math.Clamp(fraction, 0.0, 1.0);
            double angle = arc.StartAngleRadians + arc.SweepAngleRadians * fraction;
            double direction = arc.SweepAngleRadians >= 0.0 ? 1.0 : -1.0;
            double tangentLength = Math.Max(arc.Radius * 0.01, 1.0);
            PointD point = arc.PointAt(fraction);
            PointD tangentPoint = new(
                point.X + -Math.Sin(angle) * direction * tangentLength,
                point.Y + Math.Cos(angle) * direction * tangentLength);

            return TryResolveReadableScreenAngle(point, tangentPoint, engine, out angleDegrees);
        }

        private static double ResolveArcFraction(double angle, double startAngle, double sweepAngle)
        {
            if (sweepAngle >= 0.0)
                return NormalizePositiveAngle(angle - startAngle) / sweepAngle;

            return NormalizePositiveAngle(startAngle - angle) / -sweepAngle;
        }

        private static double DistanceToArcSquared(PointD point, ArcShape arc)
        {
            double angle = Math.Atan2(point.Y - arc.Center.Y, point.X - arc.Center.X);
            if (ArcShape.AngleLiesOnSweepPublic(angle, arc.StartAngleRadians, arc.SweepAngleRadians))
            {
                double radiusDelta = Distance(point, arc.Center) - arc.Radius;
                return radiusDelta * radiusDelta;
            }

            return Math.Min(
                DistanceSquared(point, arc.StartPoint),
                DistanceSquared(point, arc.EndPoint));
        }

        private static bool TryResolveReadableScreenAngle(
            PointD startWorld,
            PointD endWorld,
            MapCanvasEngine engine,
            out float angleDegrees)
        {
            angleDegrees = 0.0f;
            PointD startScreen = engine.WorldToScreen(startWorld);
            PointD endScreen = engine.WorldToScreen(endWorld);
            double dx = endScreen.X - startScreen.X;
            double dy = endScreen.Y - startScreen.Y;
            if (!double.IsFinite(dx) ||
                !double.IsFinite(dy) ||
                dx * dx + dy * dy <= 1e-6)
            {
                return false;
            }

            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            angle = NormalizeReadableTextAngle(angle);
            if (!double.IsFinite(angle))
                return false;

            angleDegrees = (float)angle;
            return true;
        }

        private static double NormalizeReadableTextAngle(double angleDegrees)
        {
            while (angleDegrees <= -180.0)
                angleDegrees += 360.0;
            while (angleDegrees > 180.0)
                angleDegrees -= 360.0;

            if (angleDegrees > 90.0)
                angleDegrees -= 180.0;
            else if (angleDegrees < -90.0)
                angleDegrees += 180.0;

            return angleDegrees;
        }

        private static double NormalizePositiveAngle(double angle)
        {
            double full = Math.PI * 2.0;
            angle %= full;
            return angle < 0.0 ? angle + full : angle;
        }

        private static double DistanceToSegmentSquared(PointD point, PointD start, PointD end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared <= 0.0)
                return DistanceSquared(point, start);

            double t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared;
            t = Math.Clamp(t, 0.0, 1.0);
            PointD projection = new(start.X + dx * t, start.Y + dy * t);
            return DistanceSquared(point, projection);
        }

        private static double DistanceSquared(PointD first, PointD second)
        {
            double dx = first.X - second.X;
            double dy = first.Y - second.Y;
            return dx * dx + dy * dy;
        }

        private static double Distance(PointD first, PointD second)
        {
            return Math.Sqrt(DistanceSquared(first, second));
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
            bool deepZoomViewport = IsDeepZoomLabelViewport(visibleWorldBounds);

            if (!shapeFullyVisible &&
                deepZoomViewport &&
                TryResolveFastVisibleShapeLabelAnchor(feature.Shape, visibleWorldBounds, out PointD fastAnchor))
            {
                anchor = fastAnchor;
                return true;
            }

            Geometry? geometry = feature.CanvasObject.Shape;
            if (!shapeFullyVisible && geometry != null && !deepZoomViewport)
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
                    if (TryResolveLineGeometryLabelAnchor(geometry, out PointD lineAnchor))
                    {
                        if (cacheKey != Guid.Empty)
                            _labelAnchorCache.TryAdd(cacheKey, lineAnchor);
                        if (ContainsPoint(visibleWorldBounds, lineAnchor))
                        {
                            anchor = lineAnchor;
                            return true;
                        }
                    }

                    if (TryResolvePolygonGeometryLabelAnchor(geometry, visibleWorldBounds, out PointD polygonAnchor))
                    {
                        if (cacheKey != Guid.Empty)
                            _labelAnchorCache.TryAdd(cacheKey, polygonAnchor);
                        anchor = polygonAnchor;
                        return true;
                    }

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

        private static bool IsDeepZoomLabelViewport(RectangleD visibleWorldBounds)
        {
            double width = Math.Abs(MaxX(visibleWorldBounds) - MinX(visibleWorldBounds));
            double height = Math.Abs(MaxY(visibleWorldBounds) - MinY(visibleWorldBounds));
            return double.IsFinite(width) &&
                   double.IsFinite(height) &&
                   Math.Max(width, height) <= DeepZoomLabelViewportWorldSpan;
        }

        private static bool TryResolveFastVisibleShapeLabelAnchor(
            IShape shape,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            anchor = default;

            switch (shape)
            {
                case LineShape line:
                    return TryResolveClippedSegmentAnchor(
                        line.Start,
                        line.End,
                        visibleWorldBounds,
                        out anchor);

                case PolylineShape polyline:
                    return TryResolvePolylineVisibleAnchor(
                        polyline,
                        visibleWorldBounds,
                        out anchor);

                case ArcShape arc:
                    return TryResolveClippedSegmentAnchor(
                        arc.StartPoint,
                        arc.EndPoint,
                        visibleWorldBounds,
                        out anchor);

                default:
                    RectangleD bounds = shape.GetBoundingBox();
                    if (!bounds.IntersectsWith(visibleWorldBounds))
                    {
                        return false;
                    }

                    anchor = ClampAnchorToViewport(bounds, visibleWorldBounds);
                    return ContainsPoint(visibleWorldBounds, anchor);
            }
        }

        private static bool TryResolvePolylineVisibleAnchor(
            PolylineShape polyline,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            anchor = default;
            if (polyline.Vertices.Count == 0)
            {
                return false;
            }

            double bestLengthSquared = double.NegativeInfinity;
            PointD bestAnchor = default;

            foreach (PolylineShape.PolylineSegment segment in EnumeratePolylineLabelSegments(polyline))
            {
                if (!TryResolveClippedSegmentAnchor(
                        segment.Start,
                        segment.End,
                        visibleWorldBounds,
                        out PointD segmentAnchor,
                        out double lengthSquared))
                {
                    continue;
                }

                if (lengthSquared > bestLengthSquared)
                {
                    bestLengthSquared = lengthSquared;
                    bestAnchor = segmentAnchor;
                }
            }

            if (polyline.IsClosed && polyline.Vertices.Count > 2)
            {
                PointD start = polyline.Vertices[^1];
                PointD end = polyline.Vertices[0];
                if (TryResolveClippedSegmentAnchor(
                        start,
                        end,
                        visibleWorldBounds,
                        out PointD segmentAnchor,
                        out double lengthSquared) &&
                    lengthSquared > bestLengthSquared)
                {
                    bestLengthSquared = lengthSquared;
                    bestAnchor = segmentAnchor;
                }
            }

            if (bestLengthSquared >= 0.0)
            {
                anchor = bestAnchor;
                return true;
            }

            RectangleD bounds = polyline.GetBoundingBox();
            if (!bounds.IntersectsWith(visibleWorldBounds))
            {
                return false;
            }

            anchor = ClampAnchorToViewport(bounds, visibleWorldBounds);
            return ContainsPoint(visibleWorldBounds, anchor);
        }

        private static bool TryResolveClippedSegmentAnchor(
            PointD start,
            PointD end,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            return TryResolveClippedSegmentAnchor(
                start,
                end,
                visibleWorldBounds,
                out anchor,
                out _);
        }

        private static bool TryResolveClippedSegmentAnchor(
            PointD start,
            PointD end,
            RectangleD visibleWorldBounds,
            out PointD anchor,
            out double lengthSquared)
        {
            anchor = default;
            lengthSquared = 0.0;

            if (!ViewportClip.ClipSegment(start, end, visibleWorldBounds, out PointD clippedStart, out PointD clippedEnd))
            {
                return false;
            }

            double dx = clippedEnd.X - clippedStart.X;
            double dy = clippedEnd.Y - clippedStart.Y;
            lengthSquared = dx * dx + dy * dy;
            if (!double.IsFinite(lengthSquared))
            {
                return false;
            }

            anchor = new PointD(
                (clippedStart.X + clippedEnd.X) / 2.0,
                (clippedStart.Y + clippedEnd.Y) / 2.0);
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

                if (TryResolveLineGeometryLabelAnchor(clippedGeometry, out PointD lineAnchor))
                {
                    if (ContainsPoint(visibleWorldBounds, lineAnchor))
                    {
                        anchor = lineAnchor;
                        return true;
                    }
                }

                if (TryResolvePolygonGeometryLabelAnchor(clippedGeometry, visibleWorldBounds, out PointD polygonAnchor))
                {
                    anchor = polygonAnchor;
                    return true;
                }

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

        private static bool TryResolvePolygonGeometryLabelAnchor(
            Geometry geometry,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            anchor = default;
            if (!ContainsPolygonalGeometry(geometry))
                return false;

            NetTopologySuite.Geometries.Point centroid = geometry.Centroid;
            if (centroid.IsEmpty ||
                !double.IsFinite(centroid.X) ||
                !double.IsFinite(centroid.Y))
            {
                return false;
            }

            if (geometry.Covers(centroid))
            {
                anchor = new PointD(centroid.X, centroid.Y);
                return ContainsPoint(visibleWorldBounds, anchor);
            }

            if (TryResolvePolygonVisualCenter(geometry, centroid, visibleWorldBounds, out anchor))
                return true;

            NetTopologySuite.Geometries.Point interiorPoint = geometry.PointOnSurface;
            if (IsUsableLabelPoint(interiorPoint, geometry, visibleWorldBounds))
            {
                anchor = new PointD(interiorPoint.X, interiorPoint.Y);
                return true;
            }

            return false;
        }

        private static bool TryResolvePolygonVisualCenter(
            Geometry geometry,
            NetTopologySuite.Geometries.Point centroid,
            RectangleD visibleWorldBounds,
            out PointD anchor)
        {
            anchor = default;

            Envelope envelope = geometry.EnvelopeInternal;
            if (envelope == null ||
                envelope.IsNull ||
                envelope.Width <= 0 ||
                envelope.Height <= 0)
            {
                return false;
            }

            try
            {
                Geometry boundary = geometry.Boundary;
                const int gridSize = 20;
                const int refinementPasses = 3;

                double minX = envelope.MinX;
                double maxX = envelope.MaxX;
                double minY = envelope.MinY;
                double maxY = envelope.MaxY;
                double bestScore = double.NegativeInfinity;
                double bestCentroidDistance = double.PositiveInfinity;
                Coordinate? bestCoordinate = null;

                for (int pass = 0; pass < refinementPasses; pass++)
                {
                    double width = maxX - minX;
                    double height = maxY - minY;
                    if (width <= 0 || height <= 0)
                        break;

                    double stepX = width / gridSize;
                    double stepY = height / gridSize;
                    if (stepX <= 0 || stepY <= 0)
                        break;

                    for (int ix = 0; ix <= gridSize; ix++)
                    {
                        double x = minX + stepX * ix;
                        for (int iy = 0; iy <= gridSize; iy++)
                        {
                            double y = minY + stepY * iy;
                            if (!double.IsFinite(x) || !double.IsFinite(y))
                                continue;

                            NetTopologySuite.Geometries.Point candidate =
                                LabelGeometryFactory.CreatePoint(new Coordinate(x, y));
                            if (!geometry.Covers(candidate))
                                continue;

                            double edgeDistance = boundary.Distance(candidate);
                            double centroidDistance = candidate.Distance(centroid);
                            if (edgeDistance > bestScore ||
                                (Math.Abs(edgeDistance - bestScore) < 1e-9 &&
                                 centroidDistance < bestCentroidDistance))
                            {
                                bestScore = edgeDistance;
                                bestCentroidDistance = centroidDistance;
                                bestCoordinate = candidate.Coordinate;
                            }
                        }
                    }

                    if (bestCoordinate == null)
                        break;

                    double refineWidth = Math.Max(stepX * 2.0, width / gridSize);
                    double refineHeight = Math.Max(stepY * 2.0, height / gridSize);
                    minX = bestCoordinate.X - refineWidth;
                    maxX = bestCoordinate.X + refineWidth;
                    minY = bestCoordinate.Y - refineHeight;
                    maxY = bestCoordinate.Y + refineHeight;
                }

                if (bestCoordinate == null)
                    return false;

                PointD bestAnchor = new(bestCoordinate.X, bestCoordinate.Y);
                if (!ContainsPoint(visibleWorldBounds, bestAnchor))
                    return false;

                anchor = bestAnchor;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ContainsPolygonalGeometry(Geometry geometry)
        {
            switch (geometry)
            {
                case Polygon:
                case MultiPolygon:
                    return true;
                case GeometryCollection collection:
                    for (int i = 0; i < collection.NumGeometries; i++)
                    {
                        if (ContainsPolygonalGeometry(collection.GetGeometryN(i)))
                            return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        private static bool TryResolveLineGeometryLabelAnchor(
            Geometry geometry,
            out PointD anchor)
        {
            anchor = default;
            LineString? line = EnumerateLineStrings(geometry)
                .Where(item => item.NumPoints >= 2 && item.Length > 0)
                .OrderByDescending(item => item.Length)
                .FirstOrDefault();

            if (line == null)
                return false;

            double targetLength = line.Length / 2.0;
            double traversed = 0.0;
            Coordinate[] coordinates = line.Coordinates;

            for (int i = 1; i < coordinates.Length; i++)
            {
                Coordinate a = coordinates[i - 1];
                Coordinate b = coordinates[i];
                double segmentLength = a.Distance(b);
                if (segmentLength <= 0)
                    continue;

                if (traversed + segmentLength >= targetLength)
                {
                    double t = (targetLength - traversed) / segmentLength;
                    double x = a.X + (b.X - a.X) * t;
                    double y = a.Y + (b.Y - a.Y) * t;
                    if (!double.IsFinite(x) || !double.IsFinite(y))
                        return false;

                    anchor = new PointD(x, y);
                    return true;
                }

                traversed += segmentLength;
            }

            Coordinate last = coordinates[^1];
            if (!double.IsFinite(last.X) || !double.IsFinite(last.Y))
                return false;

            anchor = new PointD(last.X, last.Y);
            return true;
        }

        private static IEnumerable<LineString> EnumerateLineStrings(Geometry geometry)
        {
            switch (geometry)
            {
                case LineString line:
                    yield return line;
                    break;
                case MultiLineString multiLine:
                    for (int i = 0; i < multiLine.NumGeometries; i++)
                    {
                        if (multiLine.GetGeometryN(i) is LineString childLine)
                            yield return childLine;
                    }
                    break;
                case GeometryCollection collection:
                    for (int i = 0; i < collection.NumGeometries; i++)
                    {
                        foreach (LineString childLine in EnumerateLineStrings(collection.GetGeometryN(i)))
                            yield return childLine;
                    }
                    break;
            }
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


        private static string? ResolveLabelText(CanvasFeature feature, CanvasLayer layer, int sqmPrecision = 3, int traditionalPrecision = 2)
        {
            string? labelField = layer.LabelField?.Trim();
            if (!string.IsNullOrWhiteSpace(labelField))
            {
                // "static:Some fixed text" — same text for every object on the layer.
                if (labelField.StartsWith("static:", StringComparison.OrdinalIgnoreCase))
                    return labelField["static:".Length..];

                string? presetTemplate = ResolveLabelPresetTemplate(labelField);
                if (!string.IsNullOrWhiteSpace(presetTemplate))
                    return ResolveLabelTemplate(feature, presetTemplate, sqmPrecision, traditionalPrecision);

                if (labelField.StartsWith("template:", StringComparison.OrdinalIgnoreCase))
                    return ResolveLabelTemplate(feature, labelField["template:".Length..], sqmPrecision, traditionalPrecision);

                if (labelField.Contains('{') && labelField.Contains('}'))
                    return ResolveLabelTemplate(feature, labelField, sqmPrecision, traditionalPrecision);

                return ResolveLabelFieldValue(feature, labelField, sqmPrecision, traditionalPrecision);
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

        private static string? ResolveLabelTemplate(CanvasFeature feature, string template, int sqmPrecision = 3, int traditionalPrecision = 2)
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
                    return ResolveLabelFieldValue(feature, field, sqmPrecision, traditionalPrecision) ?? string.Empty;
                });

            string[] lines = expanded
                .Split('\n')
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            return lines.Length == 0 ? null : string.Join(Environment.NewLine, lines);
        }

        private static string? ResolveLabelFieldValue(CanvasFeature feature, string labelField, int sqmPrecision = 3, int traditionalPrecision = 2)
        {
            if (string.IsNullOrWhiteSpace(labelField))
                return null;

            CanvasObject canvasObject = feature.CanvasObject;
            CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
            string normalized = NormalizeLabelField(labelField);

            object? value = normalized switch
            {
                // ── Canvas / layer fields ──────────────────────────────────────────
                "labeltext"                                                         => canvasObject.LabelText,
                "objectdescription" or "description"                               => canvasObject.ObjectDescription,
                "objecttype"                                                        => canvasObject.ObjectType,
                "layername" or "canvaslayer"                                        => feature.Layer?.Name ?? canvasObject.CanvasLayer?.Name ?? feature.Shape.LayerName,
                "id" or "canvasobjectid"                                            => canvasObject.Id,
                "baselineparcelid"                                                  => canvasObject.BaselineParcelId ?? metadata?.BaselineParcelId,

                // ── Geometry metrics ───────────────────────────────────────────────
                "perimeter"                                                         => canvasObject.Shape?.Length,
                "length"                                                            => canvasObject.Shape?.Length,
                "x"                                                                 => canvasObject.Shape?.Centroid?.X,
                "y"                                                                 => canvasObject.Shape?.Centroid?.Y,

                // ── BaselineParcel — identification ────────────────────────────────
                "parcelno" or "parcelnumber" or "plotnumber" or "plotno"            => ResolveFirst(
                    metadata?.ParcelNo,
                    GetPropertyValue(canvasObject.BaselineParcel, "ParcelNo"),
                    canvasObject.LabelText),
                "mapsheetno" or "mapsheet" or "sheetno"                             => ResolveFirst(
                    metadata?.MapSheetNo,
                    GetPropertyValue(canvasObject.BaselineParcel, "MapSheetNo")),
                "mapsheetparcelno" or "fulluniqueparcelcode" or "uniqueparcelcode"  => ResolveFirst(
                    metadata?.FullUniqueParcelCode,
                    GetPropertyValue(canvasObject.BaselineParcel, "FullUniqueParcelCode"),
                    CombineMapSheetAndParcel(metadata)),

                // ── BaselineParcel — owner ─────────────────────────────────────────
                "ownername" or "landowner" or "landownersname"                      => ResolveFirst(
                    metadata?.OwnerName,
                    GetPropertyValue(canvasObject.BaselineParcel?.LandOwner, "FullName")),
                "ownerfatherspouse" or "fatherspousename" or "fathername"           => GetPropertyValue(canvasObject.BaselineParcel?.LandOwner, "FatherOrSpouseName"),
                "ownershiptype" or "landownershiptype"                              => GetPropertyValue(canvasObject.BaselineParcel, "LandOwnershipType"),
                "hastenant"                                                         => canvasObject.BaselineParcel != null
                    ? (canvasObject.BaselineParcel.HasTenant ? "Yes" : "No")
                    : (object?)null,
                "tenantname"                                                        => GetPropertyValue(canvasObject.BaselineParcel, "TenantName"),

                // ── BaselineParcel — area (from records) ───────────────────────────
                "areasqm" or "originalareasqm" or "recordareasqm"                   => FormatSqmArea(ResolveFirst(
                    metadata?.RecordAreaSqm,
                    GetPropertyValue(canvasObject.BaselineParcel, "OriginalAreaSqm"),
                    GetAttributeValue(metadata, labelField)), sqmPrecision),
                "arearapd" or "localarea"                                           => FormatTraditionalArea(ResolveFirst(
                    metadata?.RecordAreaSqm,
                    GetPropertyValue(canvasObject.BaselineParcel, "OriginalAreaSqm"),
                    metadata?.CalculatedAreaSqm), traditionalPrecision: traditionalPrecision),
                "areabkd"                                                           => FormatTraditionalArea(ResolveFirst(
                    metadata?.RecordAreaSqm,
                    GetPropertyValue(canvasObject.BaselineParcel, "OriginalAreaSqm"),
                    metadata?.CalculatedAreaSqm), useBkd: true, traditionalPrecision: traditionalPrecision),
                "fieldmeasuredareasqm"                                              => FormatSqmArea(
                    GetPropertyValue(canvasObject.BaselineParcel, "FieldMeasuredAreaSqm"), sqmPrecision),
                "effectiveareasqm"                                                  => FormatSqmArea(
                    GetPropertyValue(canvasObject.BaselineParcel, "EffectiveAreaSqm"), sqmPrecision),

                // ── BaselineParcel — area (from map / geometry) ────────────────────
                "calculatedareasqm" or "geometryareasqm" or "geometryarea"         => FormatSqmArea(ResolveFirst(
                    CanvasGeometryMetricsService.GetArea(canvasObject),
                    metadata?.CalculatedAreaSqm), sqmPrecision),

                // ── BaselineParcel — location ──────────────────────────────────────
                "province"                                                          => GetPropertyValue(canvasObject.BaselineParcel, "Province"),
                "district"                                                          => GetPropertyValue(canvasObject.BaselineParcel, "District"),
                "municipality"                                                      => GetPropertyValue(canvasObject.BaselineParcel, "Municipality"),
                "wardno" or "ward"                                                  => GetPropertyValue(canvasObject.BaselineParcel, "WardNo"),
                "landuse"                                                           => ResolveFirst(
                    metadata?.LandUse,
                    GetPropertyValue(canvasObject.BaselineParcel, "LandUse")),

                // ── BaselineParcel — land record (Malpot) ──────────────────────────
                "mothno" or "moth"                                                  => GetPropertyValue(canvasObject.BaselineParcel?.MalpotReference, "MothNo"),
                "paanano" or "paana"                                                => GetPropertyValue(canvasObject.BaselineParcel?.MalpotReference, "PaanaNo"),

                // ── BaselineParcel — status ────────────────────────────────────────
                "assignmentstatus" or "status"                                      => metadata?.AssignmentStatus,

                // ── Road fields ────────────────────────────────────────────────────
                "roadname"                                                          => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "RoadName"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Name"),
                    canvasObject.LabelText,
                    metadata?.MatchedText),
                "roadcode"                                                          => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "RoadCode"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Code")),
                "roadstatus"                                                        => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "RoadStatus"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Status")),
                "roadtype"                                                          => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "RoadType"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Type")),
                "surfacetype"                                                       => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "SurfaceType"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Surface")),
                "roadwidth"                                                         => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "RoadWidth"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Width")),
                "rightofwaywidth" or "rowwidth"                                     => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "RightOfWayWidth"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "ROWWidth"),
                    GetAttributeValue(metadata, "ROW Width")),
                "roaddescription"                                                   => ResolveFirst(
                    GetPropertyValue(canvasObject.Road, "Description"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Description")),

                // ── Block fields ───────────────────────────────────────────────────
                "blockname"                                                         => ResolveFirst(
                    GetPropertyValue(canvasObject.Block, "BlockName"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Name"),
                    canvasObject.LabelText,
                    metadata?.MatchedText),
                "blockcode"                                                         => ResolveFirst(
                    GetPropertyValue(canvasObject.Block, "BlockCode"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Code")),
                "blocklanduse"                                                      => ResolveFirst(
                    GetPropertyValue(canvasObject.Block, "BlockLandUse"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "LandUse"),
                    GetAttributeValue(metadata, "Type")),
                "blockdepth"                                                        => ResolveFirst(
                    GetPropertyValue(canvasObject.Block, "BlockDepth"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Depth")),
                "blockdepthgeometry" or "blockdepthfromgeometry"                    => CanvasGeometryMetricsService.GetBlockDepthFromGeometry(canvasObject),
                "blockareasqm" or "blockarea"                                       => FormatSqmArea(ResolveFirst(
                    CanvasGeometryMetricsService.GetArea(canvasObject),
                    GetPropertyValue(canvasObject.Block, "BlockArea"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Area")), sqmPrecision),
                "blockarearapd"                                                     => FormatTraditionalArea(ResolveFirst(
                    CanvasGeometryMetricsService.GetArea(canvasObject),
                    GetPropertyValue(canvasObject.Block, "BlockArea"),
                    GetAttributeValue(metadata, "Area")), traditionalPrecision: traditionalPrecision),
                "blockareabkd"                                                      => FormatTraditionalArea(ResolveFirst(
                    CanvasGeometryMetricsService.GetArea(canvasObject),
                    GetPropertyValue(canvasObject.Block, "BlockArea"),
                    GetAttributeValue(metadata, "Area")), useBkd: true, traditionalPrecision: traditionalPrecision),
                "blockdescription"                                                  => ResolveFirst(
                    GetPropertyValue(canvasObject.Block, "Description"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Description")),

                // ── ReplottedParcel fields ─────────────────────────────────────────
                "replottedparcelno" or "activeplotnumber"                           => ResolveFirst(
                    GetActivePlotNumber(canvasObject.ReplottedParcel),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "PlotNo"),
                    GetAttributeValue(metadata, "ParcelNo"),
                    canvasObject.LabelText),
                "systemgeneratednumber"                                             => ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "SystemGeneratedNumber"),
                    GetAttributeValue(metadata, labelField)),
                "derivednumber"                                                     => ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "DerivedNumber"),
                    GetAttributeValue(metadata, labelField)),
                "blocksequencenumber"                                               => ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "BlockSequenceNumber"),
                    GetAttributeValue(metadata, labelField)),
                "plottypename" or "plottype"                                        => ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel?.PlotType, "TypeName"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Type")),
                "plotblockname" or "plotblock"                                      => ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel?.Block, "BlockName"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "BlockName")),
                "plotareasqm"                                                       => FormatSqmArea(ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "PlotAreaSqm"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Area")), sqmPrecision),
                "plotarearapd"                                                      => FormatTraditionalArea(ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "PlotAreaSqm"),
                    GetAttributeValue(metadata, "Area")), traditionalPrecision: traditionalPrecision),
                "plotareabkd"                                                       => FormatTraditionalArea(ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "PlotAreaSqm"),
                    GetAttributeValue(metadata, "Area")), useBkd: true, traditionalPrecision: traditionalPrecision),
                "plotnotes"                                                         => ResolveFirst(
                    GetPropertyValue(canvasObject.ReplottedParcel, "Notes"),
                    GetAttributeValue(metadata, labelField),
                    GetAttributeValue(metadata, "Notes")),

                // ── Import / tracing ───────────────────────────────────────────────
                "sourcelayer"                                                       => metadata?.SourceLayer,
                "sourcefilename" or "sourcefile"                                    => metadata?.SourceFileName,
                "sourceformat"                                                      => metadata?.SourceFormat,
                "matchedtext"                                                       => metadata?.MatchedText,

                // ── Fallback — try all linked entities via reflection ───────────────
                _ => ResolveFirst(
                    GetAttributeValue(metadata, labelField),
                    GetPropertyValue(canvasObject.BaselineParcel, labelField),
                    GetPropertyValue(canvasObject.BaselineParcel?.LandOwner, labelField),
                    GetPropertyValue(canvasObject.Road, labelField),
                    GetPropertyValue(canvasObject.Block, labelField),
                    GetPropertyValue(canvasObject.ReplottedParcel, labelField),
                    TryGetShapeProperty(feature, labelField, out object? shapeValue) ? shapeValue : null)
            };

            return FormatLabelValue(value);
        }

        private static string? FormatTraditionalArea(object? value, bool useBkd = false, int traditionalPrecision = 2)
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
                ? AreaConverterService.SqmToBKDString(areaSqm, traditionalPrecision)
                : AreaConverterService.SqmToRAPDString(areaSqm, traditionalPrecision);
        }

        private static string? FormatSqmArea(object? value, int sqmPrecision = 3)
        {
            if (value == null)
                return null;

            if (!double.TryParse(
                    Convert.ToString(value, CultureInfo.InvariantCulture),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double areaSqm))
            {
                return null;
            }

            return areaSqm.ToString($"F{sqmPrecision}", CultureInfo.InvariantCulture);
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

        private static string? GetActivePlotNumber(Land_Readjustment_Tool.Core.Entities.Replotting.ReplottedParcel? rp) =>
            rp?.ActiveNumberType switch
            {
                "Derived"       => rp.DerivedNumber,
                "BlockSequence" => rp.BlockSequenceNumber,
                _               => rp?.SystemGeneratedNumber,
            };

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

        private static RectangleD CreateClipWorldBounds(MapCanvasEngine engine)
        {
            double zoomScale = engine.ZoomScale;
            double margin = double.IsFinite(zoomScale) && zoomScale > 0.0
                ? Math.Max(MinClipWorldMargin, ClipMarginScreenPixels / zoomScale)
                : MinClipWorldMargin;

            return engine.GetClipWorldBounds(margin);
        }

        private static GraphicsPath? CreateRectanglePath(
            MapCanvasEngine engine,
            RectangleShape rectangle,
            RectangleD? clipWorldBounds)
        {
            double left = Math.Min(rectangle.Start.X, rectangle.End.X);
            double right = Math.Max(rectangle.Start.X, rectangle.End.X);
            double bottom = Math.Min(rectangle.Start.Y, rectangle.End.Y);
            double top = Math.Max(rectangle.Start.Y, rectangle.End.Y);
            PointD[] ring =
            [
                new PointD(left, bottom),
                new PointD(right, bottom),
                new PointD(right, top),
                new PointD(left, top)
            ];

            IReadOnlyList<PointD> worldPoints = clipWorldBounds.HasValue
                ? ViewportClip.ClipPolygon(ring, clipWorldBounds.Value)
                : ring;
            if (worldPoints.Count < 3)
            {
                return null;
            }

            PointF[] points = worldPoints
                .Select(point => ToScreenPointF(engine.WorldToScreen(point)))
                .Where(IsValidPoint)
                .ToArray();
            if (points.Length < 3)
            {
                return null;
            }

            GraphicsPath path = new();
            path.AddPolygon(points);
            return path;
        }

        private static PointF ToScreenPointF(PointD screenPoint)
        {
            return new PointF(
                ToFiniteFloat(Math.Round(screenPoint.X)),
                ToFiniteFloat(Math.Round(screenPoint.Y)));
        }

        private static float ToFiniteFloat(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return float.NaN;
            }

            return (float)value;
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
