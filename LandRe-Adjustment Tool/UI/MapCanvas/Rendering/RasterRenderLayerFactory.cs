using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using OSGeo.GDAL;
using OSGeo.OSR;
using System.Security.Cryptography;
using System.Text;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    internal static class RasterRenderLayerFactory
    {
        private const string WebMercatorSrsDefinition = "EPSG:3857";
        private const string ProjectRasterFolderName = "RasterLayers";

        public static IRasterRenderLayer FromCanvasLayer(
            CanvasLayer layer,
            string? projectFolderPath,
            Action? tileReadyCallback = null,
            string? projectSrsDefinition = null)
        {
            ArgumentNullException.ThrowIfNull(layer);

            if (string.IsNullOrWhiteSpace(layer.SourceFile))
            {
                throw new InvalidOperationException(
                    $"Raster layer '{layer.Name}' does not have a source file.");
            }

            string filePath = ResolveLayerFilePath(
                layer.SourceFile,
                projectFolderPath);

            if (XyzLiveTileRenderLayer.IsLiveTileVrtPath(filePath))
            {
                return XyzLiveTileRenderLayer.FromCanvasLayer(
                    layer,
                    filePath,
                    tileReadyCallback,
                    projectSrsDefinition);
            }

            if (MbTilesRenderLayer.IsMbTilesPath(filePath))
            {
                MbTilesLayerMetadata metadata =
                    MbTilesLayerMetadataStore.TryRead(filePath) ??
                    MbTilesLayerMetadata.Create(
                        WebMercatorSrsDefinition,
                        WebMercatorSrsDefinition,
                        filePath);

                if (!IsWebMercatorDefinition(metadata.TargetSrsDefinition))
                {
                    string warpedRasterPath = CreateOrUpdateWarpedMbTilesVrt(
                        filePath,
                        metadata);
                    CanvasLayer warpedLayer = CreateRasterLayerForSource(
                        layer,
                        warpedRasterPath);
                    return RasterRenderLayer.FromCanvasLayer(warpedLayer, projectFolderPath);
                }

                try
                {
                    return MbTilesRenderLayer.FromCanvasLayer(layer, filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Direct MBTiles renderer failed for '{layer.Name}'. Falling back to GDAL raster renderer. {ex.Message}");
                }
            }

            return RasterRenderLayer.FromCanvasLayer(layer, projectFolderPath);
        }

        private static CanvasLayer CreateRasterLayerForSource(
            CanvasLayer sourceLayer,
            string sourceFile)
        {
            return new CanvasLayer
            {
                Id = sourceLayer.Id,
                Name = sourceLayer.Name,
                LayerType = sourceLayer.LayerType,
                SourceFile = sourceFile,
                IsVisible = sourceLayer.IsVisible,
                FillTransparency = 0,
                DisplayOrder = sourceLayer.DisplayOrder
            };
        }

        private static string CreateOrUpdateWarpedMbTilesVrt(
            string mbTilesPath,
            MbTilesLayerMetadata metadata)
        {
            string? rasterFolder = Path.GetDirectoryName(mbTilesPath);
            if (string.IsNullOrWhiteSpace(rasterFolder))
            {
                rasterFolder = Environment.CurrentDirectory;
            }

            string cacheFolder = Path.Combine(
                rasterFolder,
                ".replot-warped-mbtiles");
            Directory.CreateDirectory(cacheFolder);

            FileInfo mbTilesInfo = new(mbTilesPath);
            string cacheKey = CreateCacheKey(
                mbTilesInfo.FullName,
                mbTilesInfo.Length,
                mbTilesInfo.LastWriteTimeUtc.Ticks,
                metadata.SourceSrsDefinition,
                metadata.TargetSrsDefinition);
            string warpedRasterPath = Path.Combine(
                cacheFolder,
                $"{Path.GetFileNameWithoutExtension(mbTilesPath)}-{cacheKey}.vrt");

            if (File.Exists(warpedRasterPath) &&
                File.GetLastWriteTimeUtc(warpedRasterPath) >= mbTilesInfo.LastWriteTimeUtc)
            {
                return warpedRasterPath;
            }

            GdalConfiguration.ConfigureGdal();
            if (!GdalConfiguration.Usable)
            {
                throw new InvalidOperationException(
                    "GDAL is not configured correctly. MBTiles reprojection cannot continue.");
            }

            using Dataset sourceDataset = Gdal.Open(mbTilesPath, Access.GA_ReadOnly)
                ?? throw new InvalidOperationException(
                    $"GDAL could not open MBTiles layer '{mbTilesPath}'.");

            string[] warpArgs =
            [
                "-overwrite",
                "-of",
                "VRT",
                "-r",
                "near",
                "-multi",
                "-wo",
                "NUM_THREADS=ALL_CPUS",
                "-s_srs",
                metadata.SourceSrsDefinition,
                "-t_srs",
                metadata.TargetSrsDefinition
            ];

            using GDALWarpAppOptions warpOptions = new(warpArgs);
            using Dataset warpedDataset = Gdal.Warp(
                warpedRasterPath,
                [sourceDataset],
                warpOptions,
                null,
                null)
                ?? throw new InvalidOperationException(
                    $"GDAL could not create a warped view for MBTiles layer '{mbTilesPath}'.");

            warpedDataset.FlushCache();
            return warpedRasterPath;
        }

        private static string CreateCacheKey(params object[] values)
        {
            string text = string.Join("|", values);
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
        }

        private static bool IsWebMercatorDefinition(string srsDefinition)
        {
            if (string.IsNullOrWhiteSpace(srsDefinition))
            {
                return false;
            }

            if (srsDefinition.Contains(
                    WebMercatorSrsDefinition,
                    StringComparison.OrdinalIgnoreCase) ||
                srsDefinition.Contains(
                    "EPSG:900913",
                    StringComparison.OrdinalIgnoreCase) ||
                srsDefinition.Contains(
                    "EPSG:3785",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                using SpatialReference spatialReference = new(string.Empty);
                spatialReference.SetAxisMappingStrategy(
                    AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
                if (spatialReference.SetFromUserInput(srsDefinition) != 0)
                {
                    string wkt = srsDefinition;
                    if (spatialReference.ImportFromWkt(ref wkt) != 0)
                    {
                        return false;
                    }
                }

                spatialReference.AutoIdentifyEPSG();
                string? authorityCode =
                    spatialReference.GetAuthorityCode(null) ??
                    spatialReference.GetAuthorityCode("PROJCS");
                if (authorityCode is "3857" or "900913" or "3785")
                {
                    return true;
                }

                string? projectedName =
                    spatialReference.GetAttrValue("PROJCS", 0);
                return projectedName != null &&
                       (projectedName.Contains(
                            "Pseudo-Mercator",
                            StringComparison.OrdinalIgnoreCase) ||
                        projectedName.Contains(
                            "Web Mercator",
                            StringComparison.OrdinalIgnoreCase) ||
                        projectedName.Contains(
                            "Popular Visualisation",
                            StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        internal static string ResolveLayerFilePath(
            string storedPath,
            string? projectFolderPath)
        {
            string resolvedPath;

            if (Path.IsPathRooted(storedPath))
            {
                resolvedPath = Path.GetFullPath(storedPath);
            }
            else if (string.IsNullOrWhiteSpace(projectFolderPath))
            {
                resolvedPath = Path.GetFullPath(storedPath);
            }
            else
            {
                resolvedPath = Path.GetFullPath(Path.Combine(projectFolderPath, storedPath));
            }

            if (File.Exists(resolvedPath) ||
                string.IsNullOrWhiteSpace(projectFolderPath))
            {
                return resolvedPath;
            }

            string fileName = Path.GetFileName(resolvedPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return resolvedPath;
            }

            string rasterFolderPath = Path.GetFullPath(
                Path.Combine(projectFolderPath, ProjectRasterFolderName));
            if (!Directory.Exists(rasterFolderPath))
            {
                return resolvedPath;
            }

            string? fallbackPath = Directory
                .EnumerateFiles(rasterFolderPath, "*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path =>
                    string.Equals(
                        Path.GetFileName(path),
                        fileName,
                        StringComparison.OrdinalIgnoreCase));

            return fallbackPath != null
                ? Path.GetFullPath(fallbackPath)
                : resolvedPath;
        }
    }
}
