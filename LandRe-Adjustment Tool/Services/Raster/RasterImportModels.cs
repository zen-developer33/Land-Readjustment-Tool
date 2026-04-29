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
        string? SourceSrsDefinitionOverride = null);

    /// <summary>
    /// Contains metadata and project CRS details used by the raster import review form.
    /// </summary>
    public sealed record RasterLayerImportPreview(
        string SuggestedLayerName,
        RasterDatasetMetadata Metadata,
        ProjectRasterCrsContext ProjectCrs);

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
        UnreferencedCopiedToLocalCoordinates
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
