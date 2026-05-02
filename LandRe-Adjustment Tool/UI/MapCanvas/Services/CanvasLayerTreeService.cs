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
            // Colours follow the muted, context-aware palette used by ArcMap / ArcGIS Pro.
            // ProjectBoundary — dark brick-red: authoritative, thick perimeter line.
            new(OriginalDataGroupKey, "Boundary", "ProjectBoundary", "#9B3A2A", 2.0),
            // BaselineParcel — muted steel-teal: classic cadastral / parcel layer tone.
            new(OriginalDataGroupKey, "Original Parcels", "BaselineParcel", "#2F6B8F", 1.4),
            // ProposedRoad — warm copper-amber: standard GIS road-layer colour.
            new(ProposedDataGroupKey, "Proposed Roads", "ProposedRoad", "#C07B30", 2.0),
            // ReplottedParcel — muted amethyst: clearly distinguishes proposed from existing.
            new(ProposedDataGroupKey, "Replotted Parcels", "ReplottedParcel", "#5B4F8A", 1.5)
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
            HashSet<string> existingNames = existingLayers
                .Select(layer => layer.Name.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            int nextDisplayOrder = existingLayers.Count == 0
                ? 0
                : existingLayers.Max(layer => layer.DisplayOrder) + 1;

            foreach (DefaultLayerDefinition definition in DefaultLayers)
            {
                if (existingNames.Contains(definition.Name))
                {
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
                    FillColor = null,
                    FillTransparency = 100,
                    FillStyle = "None",
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

        private sealed record LayerGroupDefinition(string Key, string Name);
        private sealed record DefaultLayerDefinition(
            string GroupKey,
            string Name,
            string LayerType,
            string BorderColor,
            double LineWeight);
    }

    public sealed record CanvasLayerTreeGroup(
        string Key,
        string Name,
        IReadOnlyList<CanvasLayer> Layers);
}
