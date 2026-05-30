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
        bool Include);

    public sealed record ExternalLayerImportOptions(
        IReadOnlyList<ExternalLayerImportOption> Layers,
        string SourceCrsCode);

    public sealed record ExternalLayerImportResult(
        bool Success,
        string? ErrorMessage,
        int LayersCreated,
        int ObjectsCreated,
        string? ProjectSourceFile,
        Envelope? BoundingBox);
}
