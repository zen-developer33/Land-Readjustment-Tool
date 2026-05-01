using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// GDAL-backed raster importer for GeoTIFF, TIFF, VRT, IMG, MBTiles, plain images, and other GDAL-readable rasters.
    /// </summary>
    public sealed class GdalRasterDatasetImporter : IRasterDatasetImporter
    {
        /// <inheritdoc />
        public RasterDatasetMetadata ReadMetadata(string sourcePath)
        {
            RasterImportService importService = new();
            return MapMetadata(importService.ReadSourceMetadata(sourcePath));
        }

        /// <inheritdoc />
        public Bitmap? CreatePreviewImage(string sourcePath, int maxPreviewPixels)
        {
            RasterImportService importService = new();
            return importService.CreatePreviewImage(sourcePath, maxPreviewPixels);
        }

        /// <inheritdoc />
        public RasterDatasetImportOutput ImportToProjectCrs(
            string sourcePath,
            string projectFolderPath,
            string layerName,
            string targetSrsDefinition,
            string? sourceSrsDefinitionOverride = null,
            RasterSourceExtent? sourceExtent = null,
            IProgress<RasterImportProgressInfo>? progress = null)
        {
            RasterImportService importService = new();
            Progress<RasterImportProgress>? innerProgress = progress == null
                ? null
                : new Progress<RasterImportProgress>(
                    update => progress.Report(
                        new RasterImportProgressInfo(update.Percent, update.Status)));

            RasterImportResult result =
                importService.ImportToProjectCrs(
                    sourcePath,
                    projectFolderPath,
                    layerName,
                    targetSrsDefinition,
                    sourceSrsDefinitionOverride,
                    MapExtent(sourceExtent),
                    innerProgress);

            return new RasterDatasetImportOutput(
                result.AbsolutePath,
                result.RelativePath,
                MapMode(result.ImportMode),
                result.SourceWidth,
                result.SourceHeight,
                MapMetadata(result.SourceMetadata));
        }

        /// <inheritdoc />
        public bool TryReprojectProjectRasterToProjectCrs(
            string rasterPath,
            string targetSrsDefinition,
            out string skipReason)
        {
            RasterImportService importService = new();
            return importService.TryReprojectProjectRasterToProjectCrs(
                rasterPath,
                targetSrsDefinition,
                out skipReason);
        }

        /// <summary>
        /// Converts a public source extent into the low-level GDAL import source window.
        /// </summary>
        private static RasterImportSourceExtent? MapExtent(
            RasterSourceExtent? sourceExtent)
        {
            return sourceExtent == null
                ? null
                : new RasterImportSourceExtent(
                    sourceExtent.SrsDefinition,
                    sourceExtent.MinX,
                    sourceExtent.MinY,
                    sourceExtent.MaxX,
                    sourceExtent.MaxY);
        }

        /// <summary>
        /// Converts the low-level GDAL import mode into the public raster service mode.
        /// </summary>
        private static RasterDatasetImportMode MapMode(RasterImportMode mode)
        {
            return mode switch
            {
                RasterImportMode.ProjectedToProjectCrs =>
                    RasterDatasetImportMode.ProjectedToProjectCrs,
                RasterImportMode.SourceCrsDefinedProjectedToProjectCrs =>
                    RasterDatasetImportMode.SourceCrsDefinedProjectedToProjectCrs,
                RasterImportMode.UnknownCrsCopiedWithoutProjection =>
                    RasterDatasetImportMode.UnknownCrsCopiedWithoutProjection,
                RasterImportMode.UnreferencedCopiedToLocalCoordinates =>
                    RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates,
                RasterImportMode.SourceCrsAssignedWithoutGeoreferencing =>
                    RasterDatasetImportMode.SourceCrsAssignedWithoutGeoreferencing,
                RasterImportMode.MbTilesDirectTileSource =>
                    RasterDatasetImportMode.MbTilesDirectTileSource,
                _ => RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates
            };
        }

        /// <summary>
        /// Converts GDAL metadata into a service DTO that does not expose UI-layer implementation types.
        /// </summary>
        private static RasterDatasetMetadata MapMetadata(
            RasterImportMetadata metadata)
        {
            return new RasterDatasetMetadata(
                metadata.SourcePath,
                metadata.FileSizeBytes,
                metadata.DriverShortName,
                metadata.DriverLongName,
                metadata.Width,
                metadata.Height,
                metadata.BandCount,
                metadata.HasGeoTransform,
                metadata.GeoTransform,
                metadata.GroundControlPointCount,
                metadata.ProjectionWkt,
                metadata.ProjectionSource,
                metadata.CrsInfo.HasCoordinateSystem,
                metadata.CrsInfo.CoordinateSystemType,
                metadata.CrsInfo.Name,
                metadata.CrsInfo.Authority);
        }
    }
}
