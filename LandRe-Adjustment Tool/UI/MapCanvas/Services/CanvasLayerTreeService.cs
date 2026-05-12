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
        public const string OriginalDataGroupKey = "OriginalDataLayer";
        public const string BlockLayoutGroupKey = "BlockLayoutPlan";
        public const string RoadsGroupKey = "Roads";
        public const string ReplottedParcelsGroupKey = "ReplottedParcels";
        public const string DrawingMarkupGroupKey = "DrawingMarkupLayers";
        public const string RasterGroupKey = "RasterLayer";
        public const string ExternalGroupKey = "OtherExternalLayers";
        public const string RoadCenterlineLayerType = "RoadCenterline";
        public const string LineLayerType = "Line";
        public const string PointLayerType = "Point";
        public const string PolylineLayerType = "Polyline";
        public const string PolygonLayerType = "Polygon";
        public const string DrawingMarkupLayerType = "DrawingMarkup";

        private readonly ICanvasLayerRepository _canvasLayerRepository;

        private static readonly IReadOnlyList<LayerGroupDefinition> LayerGroups =
        [
            new(OriginalDataGroupKey, "Original Data Layer"),
            new(BlockLayoutGroupKey, "Block Layout Plan"),
            new(RoadsGroupKey, "Roads"),
            new(ReplottedParcelsGroupKey, "Replotted Parcels"),
            new(DrawingMarkupGroupKey, "Drawing/Mark Up Layers"),
            new(ExternalGroupKey, "Other External Layers"),
            new(RasterGroupKey, "Raster Layers")
        ];

        private static readonly IReadOnlyList<DefaultLayerDefinition> DefaultLayers =
        [
            // Colours follow the ArcMap-style palette used elsewhere in the app.
            new(OriginalDataGroupKey, "Project Boundary", "ProjectBoundary", "#CF7C82", null, 0, 2.0, "Solid", "Boundary"),
            new(OriginalDataGroupKey, "Original Parcels", "BaselineParcel", "#8FCDE4", "#C8E8F4", 55, 1.4, "Solid", null),
            new(BlockLayoutGroupKey, "Blocks", "Block", "#D99A5A", "#F6C766", 35, 1.5, "Solid", null),
            new(RoadsGroupKey, "Road Parcel", "RoadParcel", "#D99A5A", "#F6C766", 20, 1.5, "Solid", "Proposed Roads"),
            new(RoadsGroupKey, "Road Centerline", RoadCenterlineLayerType, "#C76E78", null, 100, 1.4, "Centerline", null),
            new(ReplottedParcelsGroupKey, "Private", "PrivateReplotParcel", "#D99BCA", "#F0B2D1", 35, 1.2, "Solid", "Replotted Parcels"),
            new(ReplottedParcelsGroupKey, "Public/Facilities/Community Spaces", "PublicFacility", "#8FCDE4", "#B7DDF0", 35, 1.2, "Solid", null),
            new(ReplottedParcelsGroupKey, "Open Spaces/Parks", "OpenSpace", "#6FAF72", "#A8E7AA", 35, 1.2, "Solid", null),
            new(ReplottedParcelsGroupKey, "Service/Sales Plot", "ServiceSalesPlot", "#E09A5B", "#F6C766", 35, 1.2, "Solid", null)
        ];

        private static readonly IReadOnlyDictionary<string, string> DefaultLayerNameToGroup =
            DefaultLayers.ToDictionary(
                definition => definition.Name,
                definition => definition.GroupKey,
                StringComparer.OrdinalIgnoreCase);

        private static readonly IReadOnlyDictionary<string, string> LegacyDefaultLayerNameToNewName =
            DefaultLayers
                .Where(definition => !string.IsNullOrWhiteSpace(definition.LegacyName))
                .ToDictionary(
                    definition => definition.LegacyName!,
                    definition => definition.Name,
                    StringComparer.OrdinalIgnoreCase);

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
            if (DefaultLayerNameToGroup.TryGetValue(layer.Name, out string? defaultGroupKey))
                return defaultGroupKey;

            return layer.LayerType switch
            {
                "Raster" => RasterGroupKey,
                "BaselineParcel" => OriginalDataGroupKey,
                "ExistingRoad" => OriginalDataGroupKey,
                "ProjectBoundary" => OriginalDataGroupKey,
                "Block" => BlockLayoutGroupKey,
                "RoadParcel" => RoadsGroupKey,
                "ProposedRoad" => RoadsGroupKey,
                RoadCenterlineLayerType => RoadsGroupKey,
                "ReplottedParcel" => ReplottedParcelsGroupKey,
                "PrivateReplotParcel" => ReplottedParcelsGroupKey,
                "PublicFacility" => ReplottedParcelsGroupKey,
                "OpenSpace" => ReplottedParcelsGroupKey,
                "ServiceSalesPlot" => ReplottedParcelsGroupKey,
                "Annotation" => DrawingMarkupGroupKey,
                DrawingMarkupLayerType => DrawingMarkupGroupKey,
                PointLayerType => DrawingMarkupGroupKey,
                PolylineLayerType => DrawingMarkupGroupKey,
                LineLayerType => DrawingMarkupGroupKey,
                PolygonLayerType => DrawingMarkupGroupKey,
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
                        IsDefinitionMatch(layer, definition));

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
                    LineStyle = definition.LineStyle,
                    LineTypeScale = 1.0,
                    FillColor = definition.FillColor,
                    FillTransparency = definition.FillTransparency,
                    FillStyle = string.IsNullOrWhiteSpace(definition.FillColor)
                        ? "None"
                        : "Solid",
                    LabelColor = "#000000",
                    PointSymbol = "Dot",
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
            bool isManagedDefault =
                string.Equals(
                    layer.Description,
                    $"Default layer: {definition.Name}",
                    StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(definition.LegacyName) &&
                 string.Equals(
                     layer.Description,
                     $"Default layer: {definition.LegacyName}",
                     StringComparison.OrdinalIgnoreCase));

            if (!isManagedDefault)
            {
                return false;
            }

            if (string.Equals(definition.GroupKey, DrawingMarkupGroupKey, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            bool changed = false;

            if (!string.Equals(layer.Name, definition.Name, StringComparison.OrdinalIgnoreCase))
            {
                layer.Name = definition.Name;
                changed = true;
            }

            if (!string.Equals(layer.LayerType, definition.LayerType, StringComparison.OrdinalIgnoreCase))
            {
                layer.LayerType = definition.LayerType;
                changed = true;
            }

            if (!string.Equals(layer.Description, $"Default layer: {definition.Name}", StringComparison.OrdinalIgnoreCase))
            {
                layer.Description = $"Default layer: {definition.Name}";
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(layer.LineStyle))
                layer.LineStyle = definition.LineStyle;

            if (layer.LineTypeScale <= 0)
                layer.LineTypeScale = 1.0;

            if (string.Equals(definition.LayerType, "ProjectBoundary", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase))
                {
                    layer.FillStyle = "None";
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(layer.FillColor))
                {
                    layer.FillColor = null;
                    changed = true;
                }

                if (layer.FillTransparency != 0)
                {
                    layer.FillTransparency = 0;
                    changed = true;
                }
            }

            if (changed)
                layer.LastModifiedDate = DateTime.Now;

            return changed;
        }

        public static bool IsProtectedDefaultLayer(CanvasLayer layer)
        {
            return DefaultLayers.Any(definition =>
                string.Equals(
                    layer.Description,
                    $"Default layer: {definition.Name}",
                    StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(definition.LegacyName) &&
                 string.Equals(
                     layer.Description,
                     $"Default layer: {definition.LegacyName}",
                     StringComparison.OrdinalIgnoreCase)));
        }

        public static bool IsLineLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, LineLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, DrawingMarkupLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, PolylineLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPointLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, PointLayerType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPolygonLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, PolygonLayerType, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDrawingMarkupLayer(CanvasLayer layer)
        {
            return string.Equals(GetGroupKey(layer), DrawingMarkupGroupKey, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRoadsGroupKey(string? groupKey)
        {
            return string.Equals(groupKey, RoadsGroupKey, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRePlotDataGroupKey(string? groupKey)
        {
            return string.Equals(groupKey, OriginalDataGroupKey, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(groupKey, BlockLayoutGroupKey, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(groupKey, RoadsGroupKey, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(groupKey, ReplottedParcelsGroupKey, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDefinitionMatch(
            CanvasLayer layer,
            DefaultLayerDefinition definition)
        {
            if (string.Equals(layer.Name, definition.Name, StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(definition.LegacyName) &&
                string.Equals(layer.Name, definition.LegacyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private sealed record LayerGroupDefinition(string Key, string Name);
        private sealed record DefaultLayerDefinition(
            string GroupKey,
            string Name,
            string LayerType,
            string BorderColor,
            string? FillColor,
            int FillTransparency,
            double LineWeight,
            string LineStyle,
            string? LegacyName);
    }

    public sealed record CanvasLayerTreeGroup(
        string Key,
        string Name,
        IReadOnlyList<CanvasLayer> Layers);
}
