using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Models.Import
{
    public sealed record ExternalLayerFileInfo(
        string FilePath,
        string FileFormat,
        IReadOnlyList<ExternalLayerInfo> Layers,
        string? DetectedCrsCode,
        bool RequiresCrsFromUser);

    public sealed record ExternalLayerInfo(
        string Name,
        int ObjectCount,
        string ObjectTypes);

    public sealed record ExternalLayerImportOption(
        string LayerName,
        bool Include,
        string? TargetLayerName = null);

    public sealed record ExternalLayerImportOptions(
        IReadOnlyList<ExternalLayerImportOption> Layers,
        string SourceCrsCode,
        bool UseTargetLayerMapping = false,
        string ImportKind = "ExternalLayer",
        string? BlockLabelLayerName = null,
        bool ReplaceExistingTargetLayerObjects = false);

    /// <summary>
    /// How the import should resolve incoming objects whose geometry already exists in the
    /// target layer (a layer holds at most one object per geometry).
    /// </summary>
    public enum ImportDuplicateGeometryChoice
    {
        Replace,
        Skip,
        Cancel
    }

    public sealed record ExternalLayerImportResult(
        bool Success,
        string? ErrorMessage,
        int LayersCreated,
        int ObjectsCreated,
        string? ProjectSourceFile,
        Envelope? BoundingBox,
        IReadOnlyList<string>? Warnings = null,
        IReadOnlyList<Guid>? ImportedObjectIds = null);
}
