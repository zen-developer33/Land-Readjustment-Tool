using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Provides layer-tree-friendly queries and updates without pushing
    /// persistence details into WinForms code.
    /// </summary>
    public sealed class CanvasLayerTreeService
    {
        public const string RasterLayerType = "Raster";
        private const string OriginalDataGroupKey = "OriginalDataLayer";
        private const string ProposedDataGroupKey = "ProposedDataLayer";
        private const string RasterGroupKey = "RasterLayer";
        private const string ExternalGroupKey = "OtherExternalLayers";

        private readonly ICanvasLayerRepository _canvasLayerRepository;

        private static readonly IReadOnlyList<LayerGroupDefinition> LayerGroups =
        [
            new(OriginalDataGroupKey, "Original Data Layer"),
            new(ProposedDataGroupKey, "Proposed Data Layer"),
            new(ExternalGroupKey, "Other External Layers"),
            new(RasterGroupKey, "RasterLayer")
            
        ];

        private static readonly IReadOnlyList<DefaultLayerDefinition> DefaultLayers =
        [
            // Colours follow the ArcMap-style palette used elsewhere in the app.
            new(OriginalDataGroupKey, "Boundary", "ProjectBoundary", "#CF7C82", "#F6B3B6", 75, 2.0),
            new(OriginalDataGroupKey, "Original Parcels", "BaselineParcel", "#B7C9EF", "#B7DDF0", 85, 1.4),
            new(ProposedDataGroupKey, "Proposed Roads", "ProposedRoad", "#D99A5A", "#F6C766", 55, 2.0),
            new(ProposedDataGroupKey, "Replotted Parcels", "ReplottedParcel", "#D9A9F0", "#F1A9D8", 80, 1.5)
        ];

        public CanvasLayerTreeService(ICanvasLayerRepository canvasLayerRepository)
        {
            _canvasLayerRepository = canvasLayerRepository
                ?? throw new ArgumentNullException(nameof(canvasLayerRepository));
        }

        public static IReadOnlyList<CanvasLayerTreeGroup> GetDefaultLayerTree()
        {
            return LayerGroups
                .Select(group => new CanvasLayerTreeGroup(group.Key, group.Name, []))
                .ToList();
        }

        public async Task<IReadOnlyList<CanvasLayerTreeGroup>> GetLayerTreeAsync(
            CancellationToken ct = default)
        {
            await EnsureDefaultLayersAsync(ct);
            IReadOnlyList<CanvasLayer> layers = await _canvasLayerRepository.GetAllOrderedAsync(ct);

            return LayerGroups
                .Select(group => new CanvasLayerTreeGroup(
                    group.Key,
                    group.Name,
                    layers
                        .Where(layer => GetGroupKey(layer) == group.Key)
                        .OrderBy(layer => layer.DisplayOrder)
                        .ThenBy(layer => layer.Name)
                        .ToList()))
                .ToList();
        }

        public async Task<IReadOnlyList<CanvasLayer>> GetRasterLayersAsync(
            CancellationToken ct = default)
        {
            return await _canvasLayerRepository.GetAllByLayerTypeOrderedAsync(
                RasterLayerType,
                ct);
        }

        public async Task<IReadOnlyList<CanvasLayer>> GetAllLayersAsync(
            CancellationToken ct = default)
        {
            return await _canvasLayerRepository.GetAllOrderedAsync(ct);
        }

        public async Task<CanvasLayer?> SetVisibilityAsync(
            int layerId,
            bool isVisible,
            CancellationToken ct = default)
        {
            CanvasLayer? layer = await _canvasLayerRepository.GetByIDAsync(layerId, ct);
            if (layer == null)
            {
                return null;
            }

            if (layer.IsVisible == isVisible)
            {
                return layer;
            }

            layer.IsVisible = isVisible;
            layer.LastModifiedDate = DateTime.Now;
            await _canvasLayerRepository.UpdateAsync(layer, ct);
            return layer;
        }

        private static string GetGroupKey(CanvasLayer layer)
        {
            DefaultLayerDefinition? matchedDefault = DefaultLayers
                .FirstOrDefault(definition =>
                    string.Equals(
                        definition.Name,
                        layer.Name,
                        StringComparison.OrdinalIgnoreCase));

            if (matchedDefault != null)
            {
                return matchedDefault.GroupKey;
            }

            return layer.LayerType switch
            {
                "Raster" => RasterGroupKey,
                "BaselineParcel" => OriginalDataGroupKey,
                "ExistingRoad" => OriginalDataGroupKey,
                "ProjectBoundary" => OriginalDataGroupKey,
                "ProposedRoad" => ProposedDataGroupKey,
                "ReplottedParcel" => ProposedDataGroupKey,
                "Block" => ProposedDataGroupKey,
                _ => ExternalGroupKey
            };
        }

        private async Task EnsureDefaultLayersAsync(CancellationToken ct)
        {
            IReadOnlyList<CanvasLayer> existingLayers = await _canvasLayerRepository.GetAllOrderedAsync(ct);
            int nextDisplayOrder = existingLayers.Count == 0
                ? 0
                : existingLayers.Max(layer => layer.DisplayOrder) + 1;

            foreach (DefaultLayerDefinition definition in DefaultLayers)
            {
                CanvasLayer? existingDefaultLayer = existingLayers
                    .FirstOrDefault(layer =>
                        string.Equals(layer.Name, definition.Name, StringComparison.OrdinalIgnoreCase));

                if (existingDefaultLayer != null)
                {
                    if (ApplyDefaultLayerStyle(existingDefaultLayer, definition))
                    {
                        await _canvasLayerRepository.UpdateAsync(existingDefaultLayer, ct);
                    }

                    continue;
                }

                CanvasLayer newLayer = new()
                {
                    Name = definition.Name,
                    LayerType = definition.LayerType,
                    IsVisible = true,
                    IsLocked = false,
                    IsSelectable = true,
                    IsPrintable = true,
                    DisplayOrder = nextDisplayOrder++,
                    BorderColor = definition.BorderColor,
                    LineWeight = definition.LineWeight,
                    LineStyle = "Solid",
                    FillColor = definition.FillColor,
                    FillTransparency = definition.FillTransparency,
                    FillStyle = "Solid",
                    LabelColor = "#000000",
                    PointSymbol = "Circle",
                    PointSize = 5.0,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Description = $"Default layer: {definition.Name}"
                };

                await _canvasLayerRepository.AddAsync(newLayer, ct);
            }
        }

        private static bool ApplyDefaultLayerStyle(
            CanvasLayer layer,
            DefaultLayerDefinition definition)
        {
            if (!string.Equals(
                    layer.Description,
                    $"Default layer: {definition.Name}",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            bool changed = false;

            if (!string.Equals(layer.BorderColor, definition.BorderColor, StringComparison.OrdinalIgnoreCase))
            {
                layer.BorderColor = definition.BorderColor;
                changed = true;
            }

            if (!string.Equals(layer.FillColor, definition.FillColor, StringComparison.OrdinalIgnoreCase))
            {
                layer.FillColor = definition.FillColor;
                changed = true;
            }

            if (!string.Equals(layer.FillStyle, "Solid", StringComparison.OrdinalIgnoreCase))
            {
                layer.FillStyle = "Solid";
                changed = true;
            }

            if (layer.FillTransparency != definition.FillTransparency)
            {
                layer.FillTransparency = definition.FillTransparency;
                changed = true;
            }

            if (Math.Abs(layer.LineWeight - definition.LineWeight) >= 0.001)
            {
                layer.LineWeight = definition.LineWeight;
                changed = true;
            }

            if (changed)
                layer.LastModifiedDate = DateTime.Now;

            return changed;
        }

        private sealed record LayerGroupDefinition(string Key, string Name);
        private sealed record DefaultLayerDefinition(
            string GroupKey,
            string Name,
            string LayerType,
            string BorderColor,
            string FillColor,
            int FillTransparency,
            double LineWeight);
    }

    public sealed record CanvasLayerTreeGroup(
        string Key,
        string Name,
        IReadOnlyList<CanvasLayer> Layers);
}
