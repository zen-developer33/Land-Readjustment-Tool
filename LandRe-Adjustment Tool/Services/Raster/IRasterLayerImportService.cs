namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Coordinates raster import, projection, layer creation, logging, and user-facing result text.
    /// </summary>
    public interface IRasterLayerImportService
    {
        /// <summary>
        /// Resolves the current project CRS details used as the raster import target.
        /// </summary>
        Task<ProjectRasterCrsContext> ResolveProjectCrsAsync(
            ProjectSession session,
            CancellationToken ct = default);

        /// <summary>
        /// Reads raster metadata and project CRS details before the user confirms import.
        /// </summary>
        Task<RasterLayerImportPreview> PrepareImportAsync(
            ProjectSession session,
            string sourcePath,
            CancellationToken ct = default);

        /// <summary>
        /// Imports one raster source into the active project and persists it as a canvas layer.
        /// </summary>
        Task<RasterLayerImportResult> ImportAsync(
            RasterLayerImportRequest request,
            IProgress<RasterImportProgressInfo>? progress = null,
            CancellationToken ct = default);
    }
}
