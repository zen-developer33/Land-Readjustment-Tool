namespace Land_Readjustment_Tool.Core.Models.Import
{
    public sealed record BlockLayoutPlanTargetLayerDefinition(
        string Name,
        string LayerType,
        string BorderColor,
        string? FillColor,
        int FillTransparency,
        double LineWeight,
        string LineStyle);

    public static class BlockLayoutPlanImportTargets
    {
        public const string KeepSourceLayerTarget = "Other External Layers (keep source layer)";

        public static readonly IReadOnlyList<BlockLayoutPlanTargetLayerDefinition> TargetLayers =
        [
            new("Project Boundary", "ProjectBoundary", "#FF0000", null, 50, 2.0, "DashDoubleDot"),
            new("Blocks", "Block", "#D99A5A", "#F6C766", 35, 1.5, "Solid"),
            new("Road Parcel", "RoadParcel", "#D99A5A", "#F6C766", 20, 1.5, "Solid"),
            new("Road Centerline", "RoadCenterline", "#C76E78", null, 50, 1.4, "Centerline"),
            new("Building Footprint", "BuildingFootprint", "#6B7280", "#C7D2FE", 40, 1.2, "Solid"),
            new("Open Spaces/Parks", "OpenSpace", "#6FAF72", "#A8E7AA", 35, 1.2, "Solid"),
            new("Public/Facilities/Community Spaces", "PublicFacility", "#8FCDE4", "#B7DDF0", 35, 1.2, "Solid"),
            new("Service/Sales Plot", "ServiceSalesPlot", "#E09A5B", "#F6C766", 35, 1.2, "Solid"),
            new("Private", "PrivateReplotParcel", "#D99BCA", "#F0B2D1", 35, 1.2, "Solid")
        ];

        public static BlockLayoutPlanTargetLayerDefinition? Find(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return TargetLayers.FirstOrDefault(layer =>
                string.Equals(layer.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
