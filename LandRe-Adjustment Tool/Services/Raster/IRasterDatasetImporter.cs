namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Imports and reprojects raster datasets using a format-specific implementation.
    /// </summary>
    public interface IRasterDatasetImporter
    {
        /// <summary>
        /// Reads source raster metadata without importing or changing the project.
        /// </summary>
        RasterDatasetMetadata ReadMetadata(string sourcePath);

        /// <summary>
        /// Imports a source raster into the project raster folder and normalizes it to the project CRS when possible.
        /// </summary>
        RasterDatasetImportOutput ImportToProjectCrs(
            string sourcePath,
            string projectFolderPath,
            string layerName,
            string targetSrsDefinition,
            string? sourceSrsDefinitionOverride = null,
            IProgress<RasterImportProgressInfo>? progress = null);

        /// <summary>
        /// Reprojects a project-owned raster file to the supplied project CRS when the source has valid CRS metadata.
        /// </summary>
        bool TryReprojectProjectRasterToProjectCrs(
            string rasterPath,
            string targetSrsDefinition,
            out string skipReason);
    }
}
