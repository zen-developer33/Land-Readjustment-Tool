using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

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
            CanvasObject entity = GeometryShapeMapper.ToCanvasObject(shape, layer.Id, existing);

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

        public async Task DeleteShapeAsync(
            Guid shapeId,
            CancellationToken ct = default)
        {
            await _canvasObjectRepository.DeleteAsync(shapeId, ct);
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

            CanvasLayer newLayer = new()
            {
                Name = normalizedName,
                LayerType = ResolveLayerType(shape),
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = nextDisplayOrder,
                BorderColor = "#000000",
                FillColor = "#FFFFFF",
                FillTransparency = 100,
                LineWeight = 1.0,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                LabelColor = "#000000",
                FillStyle = "Solid",
                PointSymbol = "Dot",
                PointSize = 5.0
            };

            return await _canvasLayerRepository.AddAsync(newLayer, ct);
        }

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
