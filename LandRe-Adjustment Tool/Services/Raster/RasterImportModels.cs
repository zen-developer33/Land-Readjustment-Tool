using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Data;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Carries the context needed to import one raster source into the active project.
    /// </summary>
    public sealed record RasterLayerImportRequest(
        ProjectSession Session,
        string ProjectFolderPath,
        string SourcePath,
        string? LayerName = null,
        string? SourceSrsDefinitionOverride = null,
        RasterSourceExtent? SourceExtent = null);

    /// <summary>
    /// User settings for importing a web XYZ tile source through GDAL's WMS/TMS driver.
    /// </summary>
    public sealed record XyzTileSourceImportRequest(
        string LayerName,
        string UrlTemplate,
        double MinLongitude,
        double MinLatitude,
        double MaxLongitude,
        double MaxLatitude,
        int ZoomLevel,
        string ImageExtension);

    /// <summary>
    /// Project-scoped memory for the XYZ tile import form and the last downloaded map bounds.
    /// </summary>
    public sealed record XyzTileImportOptionsState(
        string? LayerName,
        string? UrlTemplate,
        double? MinLongitude,
        double? MinLatitude,
        double? MaxLongitude,
        double? MaxLatitude,
        int? ZoomLevel,
        string? ImageExtension,
        double? LastDownloadMinLongitude,
        double? LastDownloadMinLatitude,
        double? LastDownloadMaxLongitude,
        double? LastDownloadMaxLatitude);

    /// <summary>
    /// Optional source coordinate window used to crop very large or remote raster sources during import.
    /// </summary>
    public sealed record RasterSourceExtent(
        string SrsDefinition,
        double MinX,
        double MinY,
        double MaxX,
        double MaxY);

    /// <summary>
    /// Local GDAL source definition created for a web XYZ tile source.
    /// </summary>
public sealed record XyzTileSourceDefinition(
    string DefinitionPath,
    RasterSourceExtent SourceExtent,
    long TileCount);

    /// <summary>
    /// Contains metadata and project CRS details used by the raster import review form.
    /// </summary>
    public sealed record RasterLayerImportPreview(
        string SuggestedLayerName,
        RasterDatasetMetadata Metadata,
        ProjectRasterCrsContext ProjectCrs,
        Bitmap? PreviewImage);

    /// <summary>
    /// Reports user-visible raster import progress.
    /// </summary>
    public sealed record RasterImportProgressInfo(
        int Percent,
        string Status);

    /// <summary>
    /// Describes the active project CRS prepared for raster reprojection.
    /// </summary>
    public sealed record ProjectRasterCrsContext(
        CoordinateSystem CoordinateSystem,
        DatumTransformation? DatumTransformation,
        string TargetSrsDefinition);

    /// <summary>
    /// Summarizes the physical raster file imported by GDAL before layer persistence.
    /// </summary>
    public sealed record RasterDatasetImportOutput(
        string AbsolutePath,
        string RelativePath,
        RasterDatasetImportMode ImportMode,
        int SourceWidth,
        int SourceHeight,
        RasterDatasetMetadata SourceMetadata);

    /// <summary>
    /// Identifies how the raster source was handled during import.
    /// </summary>
    public enum RasterDatasetImportMode
    {
        ProjectedToProjectCrs,
        SourceCrsDefinedProjectedToProjectCrs,
        UnknownCrsCopiedWithoutProjection,
        UnreferencedCopiedToLocalCoordinates,
        SourceCrsAssignedWithoutGeoreferencing,
        MbTilesDirectTileSource
    }

    /// <summary>
    /// Result returned after a raster source is imported and saved as a map layer.
    /// </summary>
    public sealed record RasterLayerImportResult(
        CanvasLayer Layer,
        RasterDatasetImportOutput Dataset,
        string Heading,
        string Details,
        string? Warning);
}
