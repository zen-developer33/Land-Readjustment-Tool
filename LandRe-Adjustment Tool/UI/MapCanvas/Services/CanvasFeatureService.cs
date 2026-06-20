using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.UI.Helpers;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using System.Drawing;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Orchestrates load/save of persisted canvas objects and maps them to runtime shapes.
    /// </summary>
    public sealed class CanvasFeatureService
    {
        public const string CanvasLayerIdPropertyKey = "CanvasLayerId";
        private readonly ICanvasObjectRepository _canvasObjectRepository;
        private readonly ICanvasLayerRepository _canvasLayerRepository;

        public CanvasFeatureService(
            ICanvasObjectRepository canvasObjectRepository,
            ICanvasLayerRepository canvasLayerRepository)
        {
            _canvasObjectRepository = canvasObjectRepository;
            _canvasLayerRepository = canvasLayerRepository;
        }

        public async Task<IReadOnlyList<CanvasFeature>> GetAllVisibleAsync(
            CancellationToken ct = default)
        {
            List<CanvasObject> objects = await _canvasObjectRepository.GetAllVisibleAsync(ct);
            return objects.Select(MapFeature).ToList();
        }

        public async Task<IReadOnlyList<CanvasFeature>> GetAllAsync(
            CancellationToken ct = default)
        {
            List<CanvasObject> objects = await _canvasObjectRepository.GetAllAsync(ct);
            return objects.Select(MapFeature).ToList();
        }

        public async Task<IReadOnlyList<CanvasFeature>> QueryVisibleByViewportAsync(
            RectangleD viewportWorldBounds,
            CancellationToken ct = default)
        {
            List<CanvasObject> objects = await _canvasObjectRepository.QueryByViewportAsync(
                viewportWorldBounds,
                ct);

            return objects.Select(MapFeature).ToList();
        }

        public async Task<CanvasFeature> SaveShapeAsync(
            IShape shape,
            string layerName,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(shape);

            CanvasLayer layer = await ResolveTargetLayerAsync(shape, layerName, ct);
            CanvasObject? existing = await _canvasObjectRepository.GetByIdAsync(shape.Id, ct);
            CanvasObject entity;
            try
            {
                entity = GeometryShapeMapper.ToCanvasObject(shape, layer.Id, existing);
            }
            catch (Exception ex)
            {
                string shapeType = shape.GetType().Name;
                string objectType = shape.Properties.TryGetValue("ObjectType", out object? value) &&
                                    !string.IsNullOrWhiteSpace(value?.ToString())
                    ? value.ToString()!
                    : shapeType;
                throw new InvalidOperationException(
                    $"Could not prepare {objectType} ({shapeType}, {shape.Id}) on layer '{layer.Name}' for saving: {ex.Message}",
                    ex);
            }

            if (CanvasLayerTreeService.IsBlockLayoutLayer(layer))
            {
                CanvasGeometryMetricsService.StoreBlockDepthFromGeometry(entity);
            }

            if (existing == null)
            {
                await _canvasObjectRepository.AddAsync(entity, ct);
            }
            else
            {
                await _canvasObjectRepository.UpdateAsync(entity, ct);
            }

            entity.CanvasLayer = layer;
            IShape mappedShape = GeometryShapeMapper.ToShape(entity);
            mappedShape.LayerName = layer.Name;
            return new CanvasFeature(entity, mappedShape, layer);
        }

        public async Task<IReadOnlyList<CanvasFeature>> SaveNewShapesAsync(
            IReadOnlyList<IShape> shapes,
            string fallbackLayerName,
            CancellationToken ct = default)
        {
            if (shapes.Count == 0)
            {
                return [];
            }

            Dictionary<int, CanvasLayer> layersById = (await _canvasLayerRepository.GetAllOrderedAsync(ct))
                .GroupBy(layer => layer.Id)
                .ToDictionary(group => group.Key, group => group.First());
            Dictionary<string, CanvasLayer> layersByName = layersById.Values
                .GroupBy(layer => layer.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            List<CanvasObject> entities = new(shapes.Count);
            Dictionary<Guid, CanvasLayer> layersByShapeId = new(shapes.Count);

            foreach (IShape shape in shapes)
            {
                CanvasLayer layer = await ResolveTargetLayerFromCacheAsync(
                    shape,
                    string.IsNullOrWhiteSpace(shape.LayerName) ? fallbackLayerName : shape.LayerName,
                    layersById,
                    layersByName,
                    ct);

                CanvasObject entity = GeometryShapeMapper.ToCanvasObject(shape, layer.Id);
                if (CanvasLayerTreeService.IsBlockLayoutLayer(layer))
                {
                    CanvasGeometryMetricsService.StoreBlockDepthFromGeometry(entity);
                }

                entities.Add(entity);
                layersByShapeId[shape.Id] = layer;
            }

            await _canvasObjectRepository.AddRangeAsync(entities, ct);

            List<CanvasFeature> features = new(entities.Count);
            foreach (CanvasObject entity in entities)
            {
                CanvasLayer layer = layersByShapeId[entity.Id];
                entity.CanvasLayer = layer;
                IShape mappedShape = GeometryShapeMapper.ToShape(entity);
                mappedShape.LayerName = layer.Name;
                features.Add(new CanvasFeature(entity, mappedShape, layer));
            }

            return features;
        }

        public async Task<IReadOnlyList<CanvasFeature>> SaveExistingShapesAsync(
            IReadOnlyList<(IShape Shape, CanvasObject ExistingObject)> items,
            CancellationToken ct = default)
        {
            if (items.Count == 0)
            {
                return [];
            }

            List<CanvasObject> entities = new(items.Count);
            List<(CanvasObject Entity, CanvasLayer? Layer)> entityLayers = new(items.Count);

            foreach ((IShape shape, CanvasObject existingObject) in items)
            {
                CanvasLayer? layer = existingObject.CanvasLayer;
                CanvasObject entity = GeometryShapeMapper.ToCanvasObject(
                    shape,
                    existingObject.CanvasLayerId,
                    existingObject);

                if (layer != null && CanvasLayerTreeService.IsBlockLayoutLayer(layer))
                {
                    CanvasGeometryMetricsService.StoreBlockDepthFromGeometry(entity);
                }

                entities.Add(entity);
                entityLayers.Add((entity, layer));
            }

            await _canvasObjectRepository.UpdateRangeAsync(entities, ct);

            List<CanvasFeature> features = new(entities.Count);
            foreach ((CanvasObject entity, CanvasLayer? layer) in entityLayers)
            {
                if (layer != null)
                {
                    entity.CanvasLayer = layer;
                }

                IShape mappedShape = GeometryShapeMapper.ToShape(entity);
                if (layer != null)
                {
                    mappedShape.LayerName = layer.Name;
                }

                features.Add(new CanvasFeature(entity, mappedShape, layer));
            }

            return features;
        }

        public async Task DeleteShapeAsync(
            Guid shapeId,
            CancellationToken ct = default)
        {
            await _canvasObjectRepository.DeleteAsync(shapeId, ct);
        }

        public async Task DeleteShapesAsync(
            IReadOnlyList<Guid> shapeIds,
            CancellationToken ct = default)
        {
            await _canvasObjectRepository.DeleteRangeAsync(shapeIds, ct);
        }

        private async Task<CanvasLayer> ResolveTargetLayerAsync(
            IShape shape,
            string layerName,
            CancellationToken ct)
        {
            if (shape.Properties.TryGetValue(CanvasLayerIdPropertyKey, out object? layerIdValue) &&
                TryToInt(layerIdValue, out int layerId))
            {
                CanvasLayer? layerById = await _canvasLayerRepository.GetByIDAsync(layerId, ct);
                if (layerById != null)
                {
                    return layerById;
                }
            }

            return await EnsureLayerAsync(shape, layerName, ct);
        }

        private async Task<CanvasLayer> ResolveTargetLayerFromCacheAsync(
            IShape shape,
            string layerName,
            Dictionary<int, CanvasLayer> layersById,
            Dictionary<string, CanvasLayer> layersByName,
            CancellationToken ct)
        {
            if (shape.Properties.TryGetValue(CanvasLayerIdPropertyKey, out object? layerIdValue) &&
                TryToInt(layerIdValue, out int layerId) &&
                layersById.TryGetValue(layerId, out CanvasLayer? layerById))
            {
                return layerById;
            }

            string normalizedName = string.IsNullOrWhiteSpace(layerName)
                ? "Default"
                : layerName.Trim();

            if (layersByName.TryGetValue(normalizedName, out CanvasLayer? layerByName))
            {
                return layerByName;
            }

            CanvasLayer created = await EnsureLayerAsync(shape, normalizedName, ct);
            layersById[created.Id] = created;
            layersByName[created.Name] = created;
            return created;
        }

        private async Task<CanvasLayer> EnsureLayerAsync(
            IShape shape,
            string layerName,
            CancellationToken ct)
        {
            string normalizedName = string.IsNullOrWhiteSpace(layerName)
                ? "Default"
                : layerName.Trim();

            CanvasLayer? existingLayer = await _canvasLayerRepository.GetByNameAsync(
                normalizedName,
                ct);

            if (existingLayer != null)
            {
                return existingLayer;
            }

            List<CanvasLayer> layers = await _canvasLayerRepository.GetAllOrderedAsync(ct);
            int nextDisplayOrder = layers.Count == 0
                ? 0
                : layers.Max(layer => layer.DisplayOrder) + 1;

            Color paletteColor = GetRandomPaletteColor();
            bool isTextLayer = shape is TextShape;
            CanvasLayer newLayer = new()
            {
                Name = normalizedName,
                LayerType = ResolveLayerType(shape),
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = nextDisplayOrder,
                BorderColor = ToHtml(Darken(paletteColor, 0.58f)),
                FillColor = ToHtml(paletteColor),
                LineWeight = isTextLayer ? 0.0 : 1.0,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                LabelColor = isTextLayer ? ToHtml(Darken(paletteColor, 0.58f)) : "#000000",
                LabelFontName = "Nirmala UI",
                LabelFontSize = isTextLayer ? 10.0 : 1.0,
                LabelScaleWithZoom = !isTextLayer,
                TextAlignment = isTextLayer
                    ? TextShape.NormalizeHorizontalAlignment(shape.Properties.TryGetValue("TextAlignment", out object? alignment)
                        ? alignment?.ToString()
                        : null)
                    : "Center Middle",
                FillStyle = "Solid",
                ShowFillTransparency = false,
                FillTransparency = 50,
                PointSymbol = "Dot",
                PointSize = 5.0
            };

            return await _canvasLayerRepository.AddAsync(newLayer, ct);
        }

        private static Color GetRandomPaletteColor()
        {
            Color[] palette = ColorDialogCustomColorsStore.GetLayerPaletteColors();
            if (palette.Length == 0)
                return Color.FromArgb(Random.Shared.Next(96, 232), Random.Shared.Next(96, 232), Random.Shared.Next(96, 232));

            return palette[Random.Shared.Next(palette.Length)];
        }

        private static Color Darken(Color color, float factor)
        {
            factor = Math.Clamp(factor, 0.0f, 1.0f);
            return Color.FromArgb(
                255,
                (int)Math.Round(color.R * factor),
                (int)Math.Round(color.G * factor),
                (int)Math.Round(color.B * factor));
        }

        private static string ToHtml(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private static string ResolveLayerType(IShape shape)
        {
            if (shape.Properties.TryGetValue("ObjectType", out object? objectType) &&
                string.Equals(objectType?.ToString(), "Point", StringComparison.OrdinalIgnoreCase))
            {
                return CanvasLayerTreeService.PointLayerType;
            }

            return shape switch
            {
                TextShape => CanvasLayerTreeService.AnnotationLayerType,
                LineShape or PolylineShape { IsClosed: false } or ArcShape => CanvasLayerTreeService.PolylineLayerType,
                _ => CanvasLayerTreeService.PolygonLayerType
            };
        }

        private static bool TryToInt(object? value, out int result)
        {
            result = 0;
            return value != null && int.TryParse(value.ToString(), out result);
        }

        private static CanvasFeature MapFeature(CanvasObject canvasObject)
        {
            IShape shape = GeometryShapeMapper.ToShape(canvasObject);
            shape.LayerName = canvasObject.CanvasLayer?.Name ?? shape.LayerName;
            return new CanvasFeature(canvasObject, shape, canvasObject.CanvasLayer);
        }
    }
}
