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
        int TextCount,
        bool HasPolygons);

    public sealed record CadastralLayerImportOption(
        string LayerName,
        bool Include,
        string MapSheetNo);

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
        public string AssignmentStatus { get; set; } = "Unassigned";
        public DateTime ImportedAt { get; set; } = DateTime.Now;
    }
}
