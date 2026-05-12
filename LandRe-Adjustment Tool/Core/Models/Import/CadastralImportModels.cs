using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Core.Models.Import
{
    public sealed record CadastralFileInfo(
        string FilePath,
        string FileFormat,
        IReadOnlyList<CadastralLayerInfo> Layers,
        IReadOnlyList<string> AttributeFields,
        string? DetectedCrsCode,
        bool RequiresCrsFromUser,
        int TextCount);

    public sealed record CadastralLayerInfo(
        string Name,
        int PolygonCount,
        int PolylineCount,
        int LineCount,
        int PointCount,
        int TextCount,
        bool HasImportableObjects)
    {
        public int ObjectCount => PolygonCount + PolylineCount + LineCount + PointCount + TextCount;

        public string ObjectTypes
        {
            get
            {
                List<string> types = [];
                if (PolygonCount > 0) types.Add($"polygon: {PolygonCount}");
                if (PolylineCount > 0) types.Add($"polyline: {PolylineCount}");
                if (LineCount > 0) types.Add($"line: {LineCount}");
                if (PointCount > 0) types.Add($"point: {PointCount}");
                if (TextCount > 0) types.Add($"text: {TextCount}");
                return types.Count == 0 ? "none" : string.Join(", ", types);
            }
        }
    }

    public sealed record CadastralLayerImportOption(
        string LayerName,
        bool Include,
        string CanvasLayerName,
        string? MapSheetNo);

    public sealed record CadastralImportOptions(
        IReadOnlyList<CadastralLayerImportOption> Layers,
        string SourceCrsCode,
        bool AutoAssignParcelRecords,
        string? ShpParcelNumberField,
        string? ShpMapSheetField);

    public sealed record CadastralImportResult(
        bool Success,
        string? ErrorMessage,
        int ObjectsCreated,
        int ObjectsAssigned,
        int ObjectsUnassigned,
        int TextsMatched,
        Envelope? BoundingBox);

    public sealed class CadastralCanvasMetadata
    {
        public const string MetadataKind = "CadastralParcel";

        public string Kind { get; set; } = MetadataKind;
        public string SourceFormat { get; set; } = string.Empty;
        public string SourceFileName { get; set; } = string.Empty;
        public string? SourceLayer { get; set; }
        public string? MapSheetNo { get; set; }
        public string? ParcelNo { get; set; }
        public double CalculatedAreaSqm { get; set; }
        public string? SourceHandle { get; set; }
        public string? MatchedText { get; set; }
        public string? AttributesJson { get; set; }
        public int? BaselineParcelId { get; set; }
        public string? FullUniqueParcelCode { get; set; }
        public double? RecordAreaSqm { get; set; }
        public string? OwnerName { get; set; }
        public string? LandUse { get; set; }
        public string AssignmentStatus { get; set; } = "Unassigned";
        public DateTime ImportedAt { get; set; } = DateTime.Now;
    }

    public sealed record CadastralAssignmentResult(
        bool Success,
        string? ErrorMessage,
        int AssignedCount,
        int MissingKeyCount,
        int NoRecordMatchCount);

    public sealed record CadastralLayerMapSheetMapping(
        string LayerName,
        string? SourceLayer,
        string MapSheetNo);

    public sealed record CadastralAssignmentCandidate(
        Guid CanvasObjectId,
        string LayerName,
        string ObjectType,
        string? SourceLayer,
        string? MapSheetNo,
        string? ParcelNo,
        int? BaselineParcelId,
        string AssignmentStatus,
        double CalculatedAreaSqm);

    public sealed record CadastralParcelRecordChoice(
        int Id,
        string MapSheetNo,
        string ParcelNo,
        string DisplayText,
        double OriginalAreaSqm,
        Guid? CanvasObjectId);
}
