using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Models.Import
{
    public sealed record VectorFileInfo(
        string FilePath,
        string FileFormat,
        IReadOnlyList<VectorLayerInfo> Layers,
        string? DetectedCrsCode,
        bool RequiresCrsFromUser);

    public sealed record VectorLayerInfo(
        string Name,
        int FeatureCount,
        bool HasClosedPolygons);

    public sealed record BoundaryImportOptions(
        string SelectedLayerName,
        string SourceCrsCode,
        bool DeleteExistingBoundaryObjects = false);

    public sealed record BoundaryImportResult(
        bool Success,
        string? ErrorMessage,
        int ObjectsCreated,
        Envelope? BoundingBox);
}
