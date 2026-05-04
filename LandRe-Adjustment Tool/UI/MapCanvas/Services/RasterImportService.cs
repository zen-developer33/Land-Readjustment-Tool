using OSGeo.GDAL;
using OSGeo.OSR;
using ProjNet;
using System.Drawing;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    internal sealed class RasterImportService
    {
        private const string RasterFolderName = "RasterLayers";
        private static readonly int[] RasterOverviewLevels = [2, 4, 8, 16, 32, 64, 128, 256];
        private static readonly Encoding Utf8NoBom =
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public RasterImportResult ImportToProjectCrs(
            string sourcePath,
            string projectFolderPath,
            string layerName,
            string targetSrsDefinition,
            string? sourceSrsDefinitionOverride = null,
            RasterImportSourceExtent? sourceExtent = null,
            IProgress<RasterImportProgress>? progress = null)
        {
            progress?.Report(new RasterImportProgress(5, "Checking raster file"));

            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Raster file was not found.", sourcePath);

            bool isNetworkServiceDescriptor = IsNetworkServiceDescriptor(sourcePath);
            if (isNetworkServiceDescriptor &&
                !CanImportNetworkServiceDescriptor(
                    sourceSrsDefinitionOverride,
                    sourceExtent))
            {
                throw new NotSupportedException(
                    $"'{Path.GetFileName(sourcePath)}' is a GDAL network-service descriptor " +
                    $"(WMS/WMTS/TMS XML). Live tile service files cannot be imported as " +
                    $"raster layers. Add them as an XYZ/WMS layer instead.");
            }

            if (string.IsNullOrWhiteSpace(projectFolderPath) ||
                !Directory.Exists(projectFolderPath))
            {
                throw new DirectoryNotFoundException(
                    "Project folder was not found.");
            }

            if (string.IsNullOrWhiteSpace(targetSrsDefinition))
                throw new InvalidOperationException(
                    "Project coordinate reference system is not configured.");

            progress?.Report(new RasterImportProgress(12, "Preparing raster engine"));


            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
                throw new InvalidOperationException(
                    "GDAL is not configured correctly. Raster import cannot continue.");
            ApplyGdalNetworkOptions();
            if (isNetworkServiceDescriptor)
                NormalizeNetworkServiceDescriptorForGdal(sourcePath);

            progress?.Report(new RasterImportProgress(20, "Opening raster dataset"));

            Dataset sourceDataset;
            try
            {
                sourceDataset = Gdal.Open(sourcePath, Access.GA_ReadOnly)
                    ?? throw new InvalidOperationException(
                        $"GDAL could not open '{Path.GetFileName(sourcePath)}'. " +
                        "The file format is not supported for raster import.");
            }
            catch (ApplicationException ex)
                when (ex.Message.Contains("not recognized as being in a supported file format",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (isNetworkServiceDescriptor)
                {
                    throw new NotSupportedException(
                        $"GDAL could not open the XYZ tile service descriptor " +
                        $"'{Path.GetFileName(sourcePath)}'. Verify that the GDAL WMS driver " +
                        "is available and that the tile URL is reachable.",
                        ex);
                }

                throw new NotSupportedException(
                    $"'{Path.GetFileName(sourcePath)}' is not a supported raster format. " +
                    "Supported formats include GeoTIFF, PNG, JPEG, and MBTiles. " +
                    "If this is a WMS/WMTS/TMS XML descriptor, add it as a tile service layer instead.",
                    ex);
            }

            using (sourceDataset)
            {
                if (sourceDataset.RasterCount <= 0)
                    throw new InvalidOperationException(
                        "The selected file does not contain raster bands.");

                progress?.Report(new RasterImportProgress(32, "Reading raster metadata"));

                RasterImportMetadata sourceMetadata =
                    ReadMetadata(sourcePath, sourceDataset);
                bool sourceIsMbTiles = IsMbTilesSource(sourcePath, sourceMetadata);

                string sourceProjection = ResolveSourceProjection(
                    sourceMetadata,
                    sourceSrsDefinitionOverride,
                    sourceIsMbTiles);
                RasterImportMode importMode;

                string rasterFolder = Path.Combine(projectFolderPath, RasterFolderName);
                Directory.CreateDirectory(rasterFolder);

                if (sourceIsMbTiles &&
                    sourceExtent == null &&
                    CanUseDirectMbTiles(sourcePath, sourceMetadata))
                {
                    progress?.Report(new RasterImportProgress(45, "Copying MBTiles tile package"));

                    string mbTilesOutputName = $"{SanitizeFileName(layerName)}.mbtiles";
                    string mbTilesOutputPath = GetUniquePath(Path.Combine(rasterFolder, mbTilesOutputName));
                    File.Copy(sourcePath, mbTilesOutputPath);
                    OptimizeMbTilesForRendering(mbTilesOutputPath);
                    MbTilesLayerMetadataStore.Write(
                        mbTilesOutputPath,
                        MbTilesLayerMetadata.Create(
                            "EPSG:3857",
                            targetSrsDefinition,
                            sourcePath));

                    progress?.Report(new RasterImportProgress(88, "Finalizing MBTiles layer"));

                    string mbTilesRelativePath = Path.Combine(
                        RasterFolderName,
                        Path.GetFileName(mbTilesOutputPath));
                    return new RasterImportResult(
                        mbTilesOutputPath,
                        mbTilesRelativePath,
                        RasterImportMode.MbTilesDirectTileSource,
                        sourceDataset.RasterXSize,
                        sourceDataset.RasterYSize,
                        sourceMetadata);
                }

                // ── Live tile WMS/TMS XML shortcut ───────────────────────────────
                // A GDAL WMS/TMS XML represents a global virtual dataset — warping it
                // to a GeoTIFF would require materializing billions of pixels.
                // Instead, create a thin VRT (lazy warp descriptor): GDAL fetches tiles
                // on demand from the internet when the canvas reads visible pixels.
                if (isNetworkServiceDescriptor)
                {
                    progress?.Report(new RasterImportProgress(45, "Creating live tile VRT descriptor"));

                    string vrtFileName = $"{SanitizeFileName(layerName)}.vrt";
                    string vrtPath = GetUniquePath(Path.Combine(rasterFolder, vrtFileName));

                    string sourceSrs =
                        sourceExtent?.SrsDefinition ??
                        (!string.IsNullOrWhiteSpace(sourceMetadata.ProjectionWkt)
                            ? sourceMetadata.ProjectionWkt
                            : "EPSG:3857");

                    CreateLiveTileVrt(sourceDataset, vrtPath, sourceSrs, targetSrsDefinition);

                    progress?.Report(new RasterImportProgress(88, "Finalizing live tile layer"));

                    string vrtRelativePath = Path.Combine(
                        RasterFolderName,
                        Path.GetFileName(vrtPath));

                    return new RasterImportResult(
                        vrtPath,
                        vrtRelativePath,
                        RasterImportMode.XyzLiveTileSource,
                        sourceDataset.RasterXSize,
                        sourceDataset.RasterYSize,
                        sourceMetadata);
                }

                string outputFileName = $"{SanitizeFileName(layerName)}.tif";
                string outputPath = GetUniquePath(Path.Combine(rasterFolder, outputFileName));

                bool usesDefinedSourceProjection =
                    !sourceMetadata.HasProjection &&
                    !string.IsNullOrWhiteSpace(sourceSrsDefinitionOverride);

                if (sourceMetadata.HasGeoreferencing &&
                    !string.IsNullOrWhiteSpace(sourceProjection))
                {
                    progress?.Report(new RasterImportProgress(45, "Transforming raster to project CRS"));

                    WarpToProjectCrs(
                        sourceDataset,
                        outputPath,
                        sourceProjection,
                        targetSrsDefinition,
                        sourceExtent,
                        preferNearestNeighbor: sourceIsMbTiles);
                    importMode = usesDefinedSourceProjection
                        ? RasterImportMode.SourceCrsDefinedProjectedToProjectCrs
                        : RasterImportMode.ProjectedToProjectCrs;
                }
                else
                {
                    progress?.Report(new RasterImportProgress(45, "Copying raster into project"));

                    CopyToProjectRaster(
                        sourceDataset,
                        outputPath,
                        sourceMetadata,
                        sourceProjection);
                    importMode = sourceMetadata.HasGeoreferencing
                        ? RasterImportMode.UnknownCrsCopiedWithoutProjection
                        : !string.IsNullOrWhiteSpace(sourceProjection)
                            ? RasterImportMode.SourceCrsAssignedWithoutGeoreferencing
                            : RasterImportMode.UnreferencedCopiedToLocalCoordinates;
                }

                progress?.Report(new RasterImportProgress(88, "Finalizing raster layer"));

                string relativePath = Path.Combine(RasterFolderName, Path.GetFileName(outputPath));
                return new RasterImportResult(
                    outputPath,
                    relativePath,
                    importMode,
                    sourceDataset.RasterXSize,
                    sourceDataset.RasterYSize,
                    sourceMetadata);
            }
        }

        /// <summary>
        /// Reads source raster metadata without importing it into the project.
        /// </summary>
        public RasterImportMetadata ReadSourceMetadata(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Raster file was not found.", sourcePath);

            if (IsNetworkServiceDescriptor(sourcePath))
                throw new NotSupportedException(
                    $"'{Path.GetFileName(sourcePath)}' is a GDAL network-service descriptor " +
                    "and does not contain raster metadata that can be read as a file.");



            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
                throw new InvalidOperationException(
                    "GDAL is not configured correctly. Raster metadata cannot be read.");

            using Dataset sourceDataset = Gdal.Open(sourcePath, Access.GA_ReadOnly)
                ?? throw new InvalidOperationException(
                    "GDAL could not open the selected raster.");

            if (sourceDataset.RasterCount <= 0)
                throw new InvalidOperationException(
                    "The selected file does not contain raster bands.");

            return ReadMetadata(sourcePath, sourceDataset);
        }

        /// <summary>
        /// Creates a low-resolution raster preview bitmap without importing the file.
        /// </summary>
        public Bitmap? CreatePreviewImage(string sourcePath, int maxPreviewPixels)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                    return null;

                if (IsNetworkServiceDescriptor(sourcePath))
                    return null;

                GdalConfiguration.ConfigureGdal();
                if (!GdalConfiguration.Usable)
                    return null;

                using Dataset dataset = Gdal.Open(sourcePath, Access.GA_ReadOnly);
                if (dataset == null || dataset.RasterCount <= 0)
                    return null;

                Size previewSize = CalculatePreviewSize(
                    dataset.RasterXSize,
                    dataset.RasterYSize,
                    Math.Max(64, maxPreviewPixels));

                if (previewSize.Width <= 0 || previewSize.Height <= 0)
                    return null;

                byte[] red = ReadPreviewBand(dataset, 1, previewSize);
                byte[] green = dataset.RasterCount >= 3
                    ? ReadPreviewBand(dataset, 2, previewSize)
                    : red;
                byte[] blue = dataset.RasterCount >= 3
                    ? ReadPreviewBand(dataset, 3, previewSize)
                    : red;

                Bitmap preview = new(previewSize.Width, previewSize.Height);
                for (int y = 0; y < previewSize.Height; y++)
                {
                    for (int x = 0; x < previewSize.Width; x++)
                    {
                        int index = y * previewSize.Width + x;
                        preview.SetPixel(
                            x,
                            y,
                            Color.FromArgb(red[index], green[index], blue[index]));
                    }
                }

                return preview;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reprojects an existing project raster file to the supplied project CRS definition.
        /// </summary>
        public bool TryReprojectProjectRasterToProjectCrs(
            string rasterPath,
            string targetSrsDefinition,
            out string skipReason)
        {
            skipReason = string.Empty;

            if (string.IsNullOrWhiteSpace(rasterPath) || !File.Exists(rasterPath))
                throw new FileNotFoundException("Raster file was not found.", rasterPath);

            if (string.IsNullOrWhiteSpace(targetSrsDefinition))
                throw new InvalidOperationException(
                    "Project coordinate reference system is not configured.");

            if (IsMbTilesPath(rasterPath))
            {
                MbTilesLayerMetadata existingMetadata =
                    MbTilesLayerMetadataStore.TryRead(rasterPath) ??
                    MbTilesLayerMetadata.Create(
                        "EPSG:3857",
                        targetSrsDefinition,
                        rasterPath);

                MbTilesLayerMetadataStore.Write(
                    rasterPath,
                    MbTilesLayerMetadata.Create(
                        string.IsNullOrWhiteSpace(existingMetadata.SourceSrsDefinition)
                            ? "EPSG:3857"
                            : existingMetadata.SourceSrsDefinition,
                        targetSrsDefinition,
                        existingMetadata.OriginalSourcePath ?? rasterPath));

                skipReason = "MBTiles tile source metadata refreshed for the project CRS.";
                return true;
            }

            GdalConfiguration.ConfigureGdal();

            // Live tile VRT — recreate the warp descriptor for the new project CRS.
            if (IsLiveTileVrtPath(rasterPath))
            {
                if (!GdalConfiguration.Usable)
                    throw new InvalidOperationException(
                        "GDAL is not configured correctly. Live tile VRT cannot be recreated.");

                string? wmsXmlPath = FindLiveTileVrtSource(rasterPath);
                if (wmsXmlPath == null || !File.Exists(wmsXmlPath))
                {
                    skipReason = "Live tile VRT source XML was not found; layer must be re-imported.";
                    return false;
                }

                ApplyGdalNetworkOptions();
                NormalizeNetworkServiceDescriptorForGdal(wmsXmlPath);

                using Dataset sourceDataset = Gdal.Open(wmsXmlPath, Access.GA_ReadOnly)
                    ?? throw new InvalidOperationException(
                        "GDAL could not open the live tile source XML.");

                string sourceSrsWkt = sourceDataset.GetProjectionRef();
                string sourceSrs = string.IsNullOrWhiteSpace(sourceSrsWkt)
                    ? "EPSG:3857"
                    : sourceSrsWkt;

                string tempVrtPath = rasterPath + ".recreating.vrt";
                CreateLiveTileVrt(sourceDataset, tempVrtPath, sourceSrs, targetSrsDefinition);
                File.Copy(tempVrtPath, rasterPath, overwrite: true);
                File.Delete(tempVrtPath);

                skipReason = "Live tile VRT descriptor recreated for the new project CRS.";
                return true;
            }

            if (!GdalConfiguration.Usable)
                throw new InvalidOperationException(
                    "GDAL is not configured correctly. Raster reprojection cannot continue.");

            string directory = Path.GetDirectoryName(rasterPath)
                ?? throw new InvalidOperationException("Invalid raster path.");
            string tempPath = Path.Combine(
                directory,
                $"{Path.GetFileNameWithoutExtension(rasterPath)}.reprojecting.{Guid.NewGuid():N}.tif");

            try
            {
                using (Dataset sourceDataset = Gdal.Open(rasterPath, Access.GA_ReadOnly)
                    ?? throw new InvalidOperationException(
                        "GDAL could not open the project raster."))
                {
                    if (sourceDataset.RasterCount <= 0)
                        throw new InvalidOperationException(
                            "The project raster does not contain raster bands.");

                    RasterImportMetadata sourceMetadata =
                        ReadMetadata(rasterPath, sourceDataset);

                    if (!sourceMetadata.HasGeoreferencing)
                    {
                        skipReason = "Raster has no georeferencing.";
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(sourceMetadata.ProjectionWkt))
                    {
                        skipReason = "Raster has no stored CRS.";
                        return false;
                    }

                    WarpToProjectCrs(
                        sourceDataset,
                        tempPath,
                        sourceMetadata.ProjectionWkt,
                        targetSrsDefinition);
                }

                File.Copy(tempPath, rasterPath, overwrite: true);
                return true;
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        private static void WarpToProjectCrs(
            Dataset sourceDataset,
            string outputPath,
            string sourceProjection,
            string targetSrsDefinition,
            RasterImportSourceExtent? sourceExtent = null,
            bool preferNearestNeighbor = false)
        {
            List<string> warpArgs =
            [
                "-overwrite",
                "-of",
                "GTiff",
                "-r",
                "near",
                "-multi",
                "-wo",
                "NUM_THREADS=ALL_CPUS",
                "-co",
                "TILED=YES",
                "-co",
                "BLOCKXSIZE=256",
                "-co",
                "BLOCKYSIZE=256",
                "-co",
                "COMPRESS=LZW",
                "-co",
                "NUM_THREADS=ALL_CPUS",
                "-co",
                "BIGTIFF=IF_SAFER",
                "-s_srs",
                sourceProjection,
                "-t_srs",
                targetSrsDefinition
            ];

            if (sourceExtent != null)
            {
                warpArgs.AddRange(
                [
                    "-te_srs",
                    sourceExtent.SrsDefinition,
                    "-te",
                    FormatDouble(sourceExtent.MinX),
                    FormatDouble(sourceExtent.MinY),
                    FormatDouble(sourceExtent.MaxX),
                    FormatDouble(sourceExtent.MaxY)
                ]);
            }

            using GDALWarpAppOptions warpOptions = new([.. warpArgs]);
            using Dataset outputDataset = Gdal.Warp(
                outputPath,
                [sourceDataset],
                warpOptions,
                null,
                null)
                ?? throw new InvalidOperationException(
                    "GDAL could not transform the raster to the project CRS.");

            BuildRasterOverviews(outputDataset, "NEAREST");
            outputDataset.FlushCache();
        }

        /// <summary>
        /// Creates a lightweight VRT that lazily warps a WMS/TMS virtual dataset
        /// to the project CRS. No pixels are materialized — GDAL fetches tiles
        /// on demand from the internet when the canvas reads visible pixel windows.
        /// </summary>
        private static void CreateLiveTileVrt(
            Dataset sourceDataset,
            string vrtPath,
            string sourceSrsDefinition,
            string targetSrsDefinition)
        {
            string[] warpArgs =
            [
                "-overwrite",
                "-of", "VRT",
                "-r", "near",
                "-multi",
                "-wo", "NUM_THREADS=ALL_CPUS",
                "-s_srs", sourceSrsDefinition,
                "-t_srs", targetSrsDefinition
            ];

            using GDALWarpAppOptions warpOptions = new([.. warpArgs]);
            using Dataset warpedDataset = Gdal.Warp(
                vrtPath,
                [sourceDataset],
                warpOptions,
                null,
                null)
                ?? throw new InvalidOperationException(
                    "GDAL could not create a live tile VRT descriptor.");

            warpedDataset.FlushCache();
        }

        /// <summary>
        /// Formats GDAL command values with invariant decimal separators.
        /// </summary>
        private static string FormatDouble(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Calculates a preview size that fits inside a square while preserving raster aspect ratio.
        /// </summary>
        private static Size CalculatePreviewSize(
            int sourceWidth,
            int sourceHeight,
            int maxPreviewPixels)
        {
            if (sourceWidth <= 0 || sourceHeight <= 0)
                return Size.Empty;

            double scale = Math.Min(
                maxPreviewPixels / (double)sourceWidth,
                maxPreviewPixels / (double)sourceHeight);

            scale = Math.Min(1.0, scale);

            return new Size(
                Math.Max(1, (int)Math.Round(sourceWidth * scale)),
                Math.Max(1, (int)Math.Round(sourceHeight * scale)));
        }

        /// <summary>
        /// Reads one raster band into preview-sized byte pixels.
        /// </summary>
        private static byte[] ReadPreviewBand(
            Dataset dataset,
            int bandIndex,
            Size previewSize)
        {
            byte[] buffer = new byte[previewSize.Width * previewSize.Height];
            using Band band = dataset.GetRasterBand(bandIndex);
            band.ReadRaster(
                0,
                0,
                dataset.RasterXSize,
                dataset.RasterYSize,
                buffer,
                previewSize.Width,
                previewSize.Height,
                0,
                0);

            return buffer;
        }

        private static void CopyToProjectRaster(
            Dataset sourceDataset,
            string outputPath,
            RasterImportMetadata sourceMetadata,
            string? assignedProjection = null)
        {
            Driver driver = Gdal.GetDriverByName("GTiff")
                ?? throw new InvalidOperationException(
                    "GDAL GeoTIFF driver is not available.");

            string[] copyOptions =
            [
                "TILED=YES",
                "BLOCKXSIZE=256",
                "BLOCKYSIZE=256",
                "COMPRESS=LZW",
                "NUM_THREADS=ALL_CPUS",
                "BIGTIFF=IF_SAFER"
            ];

            using Dataset outputDataset = driver.CreateCopy(
                outputPath,
                sourceDataset,
                0,
                copyOptions,
                null,
                null)
                ?? throw new InvalidOperationException(
                    "GDAL could not copy the raster into the project.");

            if (!sourceMetadata.HasGeoTransform)
            {
                double[] localImageTransform =
                [
                    0.0,
                    1.0,
                    0.0,
                    sourceMetadata.Height,
                    0.0,
                    -1.0
                ];

                outputDataset.SetGeoTransform(localImageTransform);
            }

            if (!string.IsNullOrWhiteSpace(assignedProjection) &&
                string.IsNullOrWhiteSpace(outputDataset.GetProjectionRef()))
            {
                outputDataset.SetProjection(NormalizeSrsToWkt(assignedProjection));
            }

            BuildRasterOverviews(outputDataset, "NEAREST");
            outputDataset.FlushCache();
        }

        private static string ResolveSourceProjection(
            RasterImportMetadata sourceMetadata,
            string? sourceSrsDefinitionOverride,
            bool sourceIsMbTiles)
        {
            if (!string.IsNullOrWhiteSpace(sourceMetadata.ProjectionWkt))
                return sourceMetadata.ProjectionWkt;

            if (sourceIsMbTiles)
                return "EPSG:3857";

            if (!string.IsNullOrWhiteSpace(sourceSrsDefinitionOverride))
                return sourceSrsDefinitionOverride;

            return "EPSG:4326";
        }

        private static string NormalizeSrsToWkt(string srsDefinition)
        {
            using SpatialReference spatialReference = new(string.Empty);
            int result = spatialReference.SetFromUserInput(srsDefinition);
            if (result != 0)
                return srsDefinition;

            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            spatialReference.ExportToWkt(out string wkt, []);
            return string.IsNullOrWhiteSpace(wkt) ? srsDefinition : wkt;
        }

        private static void BuildRasterOverviews(
            Dataset dataset,
            string resampling)
        {
            int maxDimension = Math.Max(dataset.RasterXSize, dataset.RasterYSize);
            int[] overviewLevels = RasterOverviewLevels
                .Where(level => maxDimension / level >= 128)
                .ToArray();

            if (overviewLevels.Length == 0)
            {
                return;
            }

            try
            {
                int result = dataset.BuildOverviews(resampling, overviewLevels);
                if (result != 0)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"GDAL overview build returned code {result}.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Raster overview build skipped: {ex.Message}");
            }
        }

        private static RasterImportMetadata ReadMetadata(
            string sourcePath,
            Dataset dataset)
        {
            FileInfo fileInfo = new(sourcePath);
            Driver? driver = dataset.GetDriver();

            double[] geoTransform = new double[6];
            dataset.GetGeoTransform(geoTransform);
            bool hasGeoTransform = HasUsableGeoTransform(geoTransform);
            int groundControlPointCount = dataset.GetGCPCount();

            string projectionWkt = dataset.GetProjectionRef();
            string projectionSource = string.IsNullOrWhiteSpace(projectionWkt)
                ? "Not available"
                : "Dataset projection";

            if (string.IsNullOrWhiteSpace(projectionWkt) &&
                groundControlPointCount > 0)
            {
                projectionWkt = dataset.GetGCPProjection();
                projectionSource = "GCP projection";
            }

            RasterCrsInfo crsInfo = RasterCrsInfo.FromWkt(projectionWkt);

            List<RasterBandMetadata> bands = [];
            for (int bandIndex = 1; bandIndex <= dataset.RasterCount; bandIndex++)
            {
                using Band band = dataset.GetRasterBand(bandIndex);
                band.GetNoDataValue(
                    out double noDataValue,
                    out int hasNoData);

                bands.Add(new RasterBandMetadata(
                    bandIndex,
                    band.DataType.ToString(),
                    band.GetRasterColorInterpretation().ToString(),
                    hasNoData != 0 ? noDataValue : null));
            }

            return new RasterImportMetadata(
                sourcePath,
                fileInfo.Length,
                driver?.ShortName ?? "Unknown",
                driver?.LongName ?? "Unknown",
                dataset.RasterXSize,
                dataset.RasterYSize,
                dataset.RasterCount,
                hasGeoTransform,
                geoTransform,
                groundControlPointCount,
                projectionWkt ?? string.Empty,
                projectionSource,
                crsInfo,
                bands);
        }

        internal static bool HasUsableGeoTransform(double[] geoTransform)
        {
            if (geoTransform.Length < 6)
                return false;

            bool isDefaultIdentity =
                Math.Abs(geoTransform[0]) < 1e-12 &&
                Math.Abs(geoTransform[1] - 1.0) < 1e-12 &&
                Math.Abs(geoTransform[2]) < 1e-12 &&
                Math.Abs(geoTransform[3]) < 1e-12 &&
                Math.Abs(geoTransform[4]) < 1e-12 &&
                Math.Abs(geoTransform[5] - 1.0) < 1e-12;

            return !isDefaultIdentity &&
                   geoTransform.All(value =>
                       !double.IsNaN(value) && !double.IsInfinity(value));
        }

        private static bool IsMbTilesSource(
            string sourcePath,
            RasterImportMetadata metadata)
        {
            return IsMbTilesPath(sourcePath) ||
                   string.Equals(
                       metadata.DriverShortName,
                       "MBTiles",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMbTilesPath(string sourcePath)
        {
            return string.Equals(
                Path.GetExtension(sourcePath),
                ".mbtiles",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true when the path is a VRT file that contains a GDAL WMS/TMS descriptor
        /// reference, indicating it was created as a live tile layer.
        /// </summary>
        private static bool IsLiveTileVrtPath(string path)
        {
            if (!string.Equals(Path.GetExtension(path), ".vrt", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!File.Exists(path))
                return false;

            try
            {
                string content = File.ReadAllText(path);
                return content.Contains(".gdal-wms", StringComparison.OrdinalIgnoreCase) ||
                       content.Contains("GDAL_WMS", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads the VRT file to find the path of the WMS/TMS XML source it references.
        /// Returns null if the source cannot be determined.
        /// </summary>
        private static string? FindLiveTileVrtSource(string vrtPath)
        {
            try
            {
                string vrtContent = File.ReadAllText(vrtPath);
                string? vrtDirectory = Path.GetDirectoryName(vrtPath);

                // Look for <SourceFilename ...>path</SourceFilename>
                int startTag = vrtContent.IndexOf("<SourceFilename", StringComparison.OrdinalIgnoreCase);
                if (startTag < 0)
                    return null;

                int contentStart = vrtContent.IndexOf('>', startTag) + 1;
                int endTag = vrtContent.IndexOf("</SourceFilename>", contentStart, StringComparison.OrdinalIgnoreCase);
                if (contentStart <= 0 || endTag < 0)
                    return null;

                string rawPath = vrtContent[contentStart..endTag].Trim();
                if (string.IsNullOrWhiteSpace(rawPath))
                    return null;

                if (Path.IsPathRooted(rawPath))
                    return rawPath;

                if (string.IsNullOrWhiteSpace(vrtDirectory))
                    return rawPath;

                return Path.GetFullPath(Path.Combine(vrtDirectory, rawPath));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true when the file is a GDAL network-service XML descriptor
        /// (WMS, WMTS, TMS) that cannot be imported as a project raster file.
        /// </summary>
        private static bool IsNetworkServiceDescriptor(string sourcePath)
        {
            string ext = Path.GetExtension(sourcePath);

            if (string.Equals(ext, ".xml", StringComparison.OrdinalIgnoreCase))
                return true;

            string fileName = Path.GetFileName(sourcePath);
            return fileName.Contains(".gdal-wms", StringComparison.OrdinalIgnoreCase) ||
                   fileName.Contains(".gdal-wmts", StringComparison.OrdinalIgnoreCase) ||
                   fileName.Contains(".gdal-tms", StringComparison.OrdinalIgnoreCase);
        }

        private static bool CanImportNetworkServiceDescriptor(
            string? sourceSrsDefinitionOverride,
            RasterImportSourceExtent? sourceExtent)
        {
            return !string.IsNullOrWhiteSpace(sourceSrsDefinitionOverride) &&
                   sourceExtent != null;
        }

        private static void ApplyGdalNetworkOptions()
        {
            Gdal.SetConfigOption("GDAL_HTTP_UNSAFESSL", "YES");
            Gdal.SetConfigOption("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR");
            Gdal.SetConfigOption("CPL_VSIL_CURL_CACHE_SIZE", "128000000");
            Gdal.SetConfigOption("GDAL_HTTP_MAX_RETRY", "3");
            Gdal.SetConfigOption("GDAL_HTTP_RETRY_DELAY", "1");
        }

        private static void NormalizeNetworkServiceDescriptorForGdal(string sourcePath)
        {
            byte[] utf8Preamble = Encoding.UTF8.GetPreamble();
            if (utf8Preamble.Length == 0)
                return;

            using FileStream input = File.OpenRead(sourcePath);
            if (input.Length < utf8Preamble.Length)
                return;

            byte[] header = new byte[utf8Preamble.Length];
            int bytesRead = input.Read(header, 0, header.Length);
            if (bytesRead != utf8Preamble.Length ||
                !header.SequenceEqual(utf8Preamble))
            {
                return;
            }

            string xml = File.ReadAllText(sourcePath, Encoding.UTF8);
            File.WriteAllText(sourcePath, xml, Utf8NoBom);
        }

        private static bool CanUseDirectMbTiles(
            string sourcePath,
            RasterImportMetadata metadata)
        {
            return IsWebMercatorOrUnspecified(metadata) &&
                   IsSupportedDirectMbTilesFormat(sourcePath);
        }

        private static bool IsWebMercatorOrUnspecified(
            RasterImportMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata.ProjectionWkt))
            {
                return true;
            }

            string authority = metadata.CrsInfo.Authority;
            if (authority.Equals("EPSG:3857", StringComparison.OrdinalIgnoreCase) ||
                authority.Equals("EPSG:900913", StringComparison.OrdinalIgnoreCase) ||
                authority.Equals("EPSG:3785", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return metadata.CrsInfo.Name.Contains(
                       "Pseudo-Mercator",
                       StringComparison.OrdinalIgnoreCase) ||
                   metadata.CrsInfo.Name.Contains(
                       "Web Mercator",
                       StringComparison.OrdinalIgnoreCase) ||
                   metadata.ProjectionWkt.Contains(
                       "Pseudo-Mercator",
                       StringComparison.OrdinalIgnoreCase) ||
                   metadata.ProjectionWkt.Contains(
                       "Web Mercator",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSupportedDirectMbTilesFormat(string sourcePath)
        {
            try
            {
                Dictionary<string, string> metadata = ReadMbTilesMetadata(sourcePath);
                if (!metadata.TryGetValue("format", out string? formatText) ||
                    string.IsNullOrWhiteSpace(formatText))
                {
                    return true;
                }

                string format = formatText.Trim().TrimStart('.').ToLowerInvariant();
                return format is "png" or "jpg" or "jpeg";
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, string> ReadMbTilesMetadata(string mbTilesPath)
        {
            Dictionary<string, string> metadata = new(StringComparer.OrdinalIgnoreCase);
            SqliteConnectionStringBuilder builder = new()
            {
                DataSource = mbTilesPath,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
                Pooling = false
            };

            using SqliteConnection connection = new(builder.ToString());
            connection.Open();
            try
            {
                using SqliteCommand command = connection.CreateCommand();
                command.CommandText = "SELECT name, value FROM metadata";
                using SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    string value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    metadata[name] = value;
                }
            }
            catch
            {
                // MBTiles metadata is optional. Absence should not force GDAL fallback.
            }

            return metadata;
        }

        private static void OptimizeMbTilesForRendering(string mbTilesPath)
        {
            try
            {
                SqliteConnectionStringBuilder builder = new()
                {
                    DataSource = mbTilesPath,
                    Mode = SqliteOpenMode.ReadWrite,
                    Cache = SqliteCacheMode.Shared,
                    Pooling = false
                };

                using SqliteConnection connection = new(builder.ToString());
                connection.Open();

                TryExecuteSql(connection, "PRAGMA temp_store=MEMORY");
                TryExecuteSql(connection, "PRAGMA cache_size=-65536");
                TryExecuteSql(
                    connection,
                    "CREATE INDEX IF NOT EXISTS idx_replot_tiles_zxy ON tiles(zoom_level, tile_column, tile_row)");
                TryExecuteSql(
                    connection,
                    "CREATE INDEX IF NOT EXISTS idx_replot_map_zxy ON map(zoom_level, tile_column, tile_row)");
                TryExecuteSql(
                    connection,
                    "CREATE INDEX IF NOT EXISTS idx_replot_images_tile_id ON images(tile_id)");
                TryExecuteSql(connection, "ANALYZE");
                TryExecuteSql(connection, "PRAGMA optimize");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"MBTiles optimization skipped for '{mbTilesPath}': {ex.Message}");
            }
        }

        private static void ExecuteSql(
            SqliteConnection connection,
            string commandText)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }

        private static void TryExecuteSql(
            SqliteConnection connection,
            string commandText)
        {
            try
            {
                ExecuteSql(connection, commandText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Optional MBTiles SQL skipped: {commandText}. {ex.Message}");
            }
        }

        private static string GetUniquePath(string desiredPath)
        {
            if (!File.Exists(desiredPath))
                return desiredPath;

            string directory = Path.GetDirectoryName(desiredPath)
                ?? throw new InvalidOperationException("Invalid output path.");
            string name = Path.GetFileNameWithoutExtension(desiredPath);
            string extension = Path.GetExtension(desiredPath);

            for (int counter = 1; counter < 10000; counter++)
            {
                string candidate = Path.Combine(
                    directory,
                    $"{name}_{counter}{extension}");

                if (!File.Exists(candidate))
                    return candidate;
            }

            throw new IOException("Could not create a unique raster output path.");
        }

        private static string SanitizeFileName(string value)
        {
            string cleaned = string.IsNullOrWhiteSpace(value)
                ? "Raster"
                : value.Trim();

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
                cleaned = cleaned.Replace(invalidChar, '_');

            return cleaned;
        }
    }

    internal sealed record RasterImportResult(
        string AbsolutePath,
        string RelativePath,
        RasterImportMode ImportMode,
        int SourceWidth,
        int SourceHeight,
        RasterImportMetadata SourceMetadata);

    internal sealed record RasterImportProgress(
        int Percent,
        string Status);

    internal sealed record RasterImportSourceExtent(
        string SrsDefinition,
        double MinX,
        double MinY,
        double MaxX,
        double MaxY);

    internal enum RasterImportMode
    {
        ProjectedToProjectCrs,
        SourceCrsDefinedProjectedToProjectCrs,
        UnknownCrsCopiedWithoutProjection,
        UnreferencedCopiedToLocalCoordinates,
        SourceCrsAssignedWithoutGeoreferencing,
        MbTilesDirectTileSource,
        XyzLiveTileSource
    }

    internal sealed record RasterBandMetadata(
        int Index,
        string DataType,
        string ColorInterpretation,
        double? NoDataValue);

    internal sealed record RasterCorner(
        string Name,
        double X,
        double Y);

    internal sealed record RasterCrsInfo(
        bool HasCoordinateSystem,
        string CoordinateSystemType,
        string Name,
        string Authority)
    {
        public static RasterCrsInfo FromWkt(string? projectionWkt)
        {
            if (string.IsNullOrWhiteSpace(projectionWkt))
            {
                return new RasterCrsInfo(
                    false,
                    "Unknown",
                    "Not stored in raster",
                    "Not available");
            }

            try
            {
                using SpatialReference spatialReference = new(projectionWkt);
                spatialReference.SetAxisMappingStrategy(
                    AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
                spatialReference.AutoIdentifyEPSG();

                string projectedName =
                    spatialReference.GetAttrValue("PROJCS", 0) ?? string.Empty;
                string geographicName =
                    spatialReference.GetAttrValue("GEOGCS", 0) ?? string.Empty;
                bool isProjected = !string.IsNullOrWhiteSpace(projectedName);

                string authority =
                    GetAuthority(spatialReference, null) ??
                    GetAuthority(spatialReference, "PROJCS") ??
                    GetAuthority(spatialReference, "GEOGCS") ??
                    "Not available";

                return new RasterCrsInfo(
                    true,
                    isProjected ? "Projected" : "Geographic",
                    isProjected ? projectedName : geographicName,
                    authority);
            }
            catch
            {
                return new RasterCrsInfo(
                    true,
                    "Unknown",
                    "Stored in raster but could not be parsed",
                    "Not available");
            }
        }

        private static string? GetAuthority(
            SpatialReference spatialReference,
            string? targetNode)
        {
            string? authorityName = spatialReference.GetAuthorityName(targetNode);
            string? authorityCode = spatialReference.GetAuthorityCode(targetNode);

            if (string.IsNullOrWhiteSpace(authorityName) ||
                string.IsNullOrWhiteSpace(authorityCode))
            {
                return null;
            }

            return $"{authorityName}:{authorityCode}";
        }
    }

    internal sealed class RasterImportMetadata
    {
        public RasterImportMetadata(
            string sourcePath,
            long fileSizeBytes,
            string driverShortName,
            string driverLongName,
            int width,
            int height,
            int bandCount,
            bool hasGeoTransform,
            double[] geoTransform,
            int groundControlPointCount,
            string projectionWkt,
            string projectionSource,
            RasterCrsInfo crsInfo,
            IReadOnlyList<RasterBandMetadata> bands)
        {
            SourcePath = sourcePath;
            FileSizeBytes = fileSizeBytes;
            DriverShortName = driverShortName;
            DriverLongName = driverLongName;
            Width = width;
            Height = height;
            BandCount = bandCount;
            HasGeoTransform = hasGeoTransform;
            GeoTransform = [.. geoTransform];
            GroundControlPointCount = groundControlPointCount;
            ProjectionWkt = projectionWkt;
            ProjectionSource = projectionSource;
            CrsInfo = crsInfo;
            Bands = bands;
            Corners = hasGeoTransform
                ? BuildCorners(GeoTransform, width, height)
                : [];
        }

        public string SourcePath { get; }
        public long FileSizeBytes { get; }
        public string DriverShortName { get; }
        public string DriverLongName { get; }
        public int Width { get; }
        public int Height { get; }
        public int BandCount { get; }
        public bool HasGeoTransform { get; }
        public double[] GeoTransform { get; }
        public int GroundControlPointCount { get; }
        public string ProjectionWkt { get; }
        public string ProjectionSource { get; }
        public RasterCrsInfo CrsInfo { get; }
        public IReadOnlyList<RasterBandMetadata> Bands { get; }
        public IReadOnlyList<RasterCorner> Corners { get; }
        public bool HasProjection => !string.IsNullOrWhiteSpace(ProjectionWkt);
        public bool HasGeoreferencing =>
            HasGeoTransform || GroundControlPointCount > 0;

        public string ToDisplayText(
            string layerName,
            string outputRelativePath,
            string projectCrsCode,
            RasterImportMode importMode)
        {
            StringBuilder builder = new();
            AppendCommonSummary(
                builder,
                layerName,
                outputRelativePath,
                projectCrsCode,
                importMode);

            builder.AppendLine();
            builder.AppendLine("Bands");
            foreach (RasterBandMetadata band in Bands.Take(8))
            {
                builder.AppendLine(
                    $"  {band.Index}: {band.DataType}, {band.ColorInterpretation}" +
                    (band.NoDataValue.HasValue
                        ? $", NoData={FormatDouble(band.NoDataValue.Value)}"
                        : string.Empty));
            }

            if (Bands.Count > 8)
                builder.AppendLine($"  ... {Bands.Count - 8} more bands");

            if (HasGeoTransform)
            {
                builder.AppendLine();
                builder.AppendLine("GeoTransform");
                builder.AppendLine(
                    $"  Origin: {FormatDouble(GeoTransform[0])}, {FormatDouble(GeoTransform[3])}");
                builder.AppendLine(
                    $"  Pixel size: {FormatDouble(GeoTransform[1])}, {FormatDouble(GeoTransform[5])}");
                builder.AppendLine(
                    $"  Rotation: {FormatDouble(GeoTransform[2])}, {FormatDouble(GeoTransform[4])}");

                builder.AppendLine();
                builder.AppendLine("Extent");
                foreach (RasterCorner corner in Corners)
                {
                    builder.AppendLine(
                        $"  {corner.Name}: {FormatDouble(corner.X)}, {FormatDouble(corner.Y)}");
                }
            }
            else if (importMode == RasterImportMode.UnreferencedCopiedToLocalCoordinates)
            {
                builder.AppendLine();
                builder.AppendLine("Temporary display extent");
                builder.AppendLine(
                    $"  X: 0 to {Width}, Y: 0 to {Height} image units");
                builder.AppendLine(
                    "  This is not a GIS location. Georeference the raster to align it with map data.");
            }

            return builder.ToString();
        }

        public string ToLayerDescription(
            string layerName,
            string outputRelativePath,
            string projectCrsCode,
            RasterImportMode importMode)
        {
            StringBuilder builder = new();
            AppendCommonSummary(
                builder,
                layerName,
                outputRelativePath,
                projectCrsCode,
                importMode);

            if (HasGeoTransform)
            {
                builder.AppendLine(
                    $"GeoTransform: {string.Join(", ", GeoTransform.Select(FormatDouble))}");
            }
            else if (importMode == RasterImportMode.UnreferencedCopiedToLocalCoordinates)
            {
                builder.AppendLine(
                    $"Temporary display extent: X 0 to {Width}, Y 0 to {Height} image units.");
            }

            return builder.ToString();
        }

        private void AppendCommonSummary(
            StringBuilder builder,
            string layerName,
            string outputRelativePath,
            string projectCrsCode,
            RasterImportMode importMode)
        {
            builder.AppendLine($"Layer: {layerName}");
            builder.AppendLine($"Source: {SourcePath}");
            builder.AppendLine($"Saved raster: {outputRelativePath}");
            builder.AppendLine($"Driver: {DriverShortName} - {DriverLongName}");
            builder.AppendLine(
                $"Size: {Width} x {Height} pixels, {BandCount} band(s)");
            builder.AppendLine($"File size: {FormatFileSize(FileSizeBytes)}");
            builder.AppendLine(
                $"Georeferenced: {(HasGeoreferencing ? "Yes" : "No")}");
            builder.AppendLine(
                $"GeoTransform: {(HasGeoTransform ? "Available" : "Not available")}");
            builder.AppendLine(
                $"Ground control points: {GroundControlPointCount}");
            builder.AppendLine(
                $"Raster CRS stored: {(HasProjection ? "Yes" : "No")}");
            builder.AppendLine($"CRS source: {ProjectionSource}");
            builder.AppendLine($"CRS type: {CrsInfo.CoordinateSystemType}");
            builder.AppendLine($"CRS name: {CrsInfo.Name}");
            builder.AppendLine($"CRS authority: {CrsInfo.Authority}");
            builder.AppendLine($"Project CRS: {projectCrsCode}");
            builder.AppendLine($"Import CRS action: {GetImportAction(importMode)}");
            builder.AppendLine($"Display placement: {GetDisplayPlacement(importMode)}");
        }

        private static string GetImportAction(RasterImportMode importMode)
        {
            return importMode switch
            {
                RasterImportMode.ProjectedToProjectCrs =>
                    "Raster CRS was found; raster was warped to the project CRS.",
                RasterImportMode.SourceCrsDefinedProjectedToProjectCrs =>
                    "Raster CRS was not stored; source CRS was defined during import and warped to the project CRS.",
                RasterImportMode.UnknownCrsCopiedWithoutProjection =>
                    "Raster has map coordinates but no CRS; copied without projection because the source CRS is unknown.",
                RasterImportMode.UnreferencedCopiedToLocalCoordinates =>
                    "Raster has no map coordinates; copied without projection for temporary display only.",
                RasterImportMode.MbTilesDirectTileSource =>
                    "MBTiles tile package was preserved for direct tile rendering in the project CRS.",
                _ => "Unknown"
            };
        }

        private static string GetDisplayPlacement(RasterImportMode importMode)
        {
            return importMode switch
            {
                RasterImportMode.ProjectedToProjectCrs =>
                    "Project CRS coordinates.",
                RasterImportMode.SourceCrsDefinedProjectedToProjectCrs =>
                    "Project CRS coordinates using the source CRS defined during import.",
                RasterImportMode.UnknownCrsCopiedWithoutProjection =>
                    "Original stored raster coordinates; alignment depends on the user later defining the correct CRS.",
                RasterImportMode.UnreferencedCopiedToLocalCoordinates =>
                    "Local image coordinates from pixel size only; not georeferenced and not spatially aligned.",
                RasterImportMode.MbTilesDirectTileSource =>
                    "Direct MBTiles tile rendering with tile footprints transformed to the project CRS.",
                _ => "Unknown"
            };
        }

        private static IReadOnlyList<RasterCorner> BuildCorners(
            double[] transform,
            int width,
            int height)
        {
            return
            [
                PixelToWorld("Upper left", transform, 0, 0),
                PixelToWorld("Upper right", transform, width, 0),
                PixelToWorld("Lower right", transform, width, height),
                PixelToWorld("Lower left", transform, 0, height)
            ];
        }

        private static RasterCorner PixelToWorld(
            string name,
            double[] transform,
            double pixel,
            double line)
        {
            return new RasterCorner(
                name,
                transform[0] + pixel * transform[1] + line * transform[2],
                transform[3] + pixel * transform[4] + line * transform[5]);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            double value = bytes / 1024.0;
            string[] units = ["KB", "MB", "GB", "TB"];
            int unitIndex = 0;

            while (value >= 1024.0 && unitIndex < units.Length - 1)
            {
                value /= 1024.0;
                unitIndex++;
            }

            return $"{value:0.##} {units[unitIndex]}";
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }
    }
}
