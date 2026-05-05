using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Canvas
{
    /// <summary>
    /// Updates persisted raster layer files when the project CRS or datum transformation changes.
    /// </summary>
    public sealed class RasterLayerProjectionService
    {
        private readonly IProjectScopedFactory _projectScopedFactory;
        private readonly IProjectRasterCrsResolver _crsResolver;
        private readonly IRasterDatasetImporter _datasetImporter;

        /// <summary>
        /// Creates a raster projection updater using project-scoped repositories.
        /// </summary>
        public RasterLayerProjectionService(
            IProjectScopedFactory projectScopedFactory,
            IProjectRasterCrsResolver crsResolver,
            IRasterDatasetImporter datasetImporter)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
            _crsResolver = crsResolver
                ?? throw new ArgumentNullException(nameof(crsResolver));
            _datasetImporter = datasetImporter
                ?? throw new ArgumentNullException(nameof(datasetImporter));
        }

        /// <summary>
        /// Reprojects all project raster layers that already have georeferencing and a stored CRS.
        /// </summary>
        public async Task<RasterLayerProjectionUpdateResult> RefreshLiveTileLayersToProjectCrsAsync(
            ProjectSession session,
            string projectFolderPath,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (string.IsNullOrWhiteSpace(projectFolderPath) ||
                !Directory.Exists(projectFolderPath))
            {
                throw new DirectoryNotFoundException(
                    "Project folder was not found.");
            }

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(session);

            ProjectRasterCrsContext crsContext =
                await _crsResolver.ResolveAsync(session, ct);

            List<CanvasLayer> rasterLayers =
                await layerRepository.GetAllByLayerTypeOrderedAsync(
                    CanvasLayerTreeService.RasterLayerType,
                    ct);

            int updatedCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            foreach (CanvasLayer layer in rasterLayers)
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(layer.SourceFile))
                {
                    skippedCount++;
                    continue;
                }

                string rasterPath = ResolveLayerFilePath(
                    projectFolderPath,
                    layer.SourceFile);

                if (!IsLiveTileVrtPath(rasterPath))
                {
                    skippedCount++;
                    continue;
                }

                try
                {
                    RasterDatasetImportOutput? freshImport = await TryReimportFromOriginalSourceAsync(
                        layer,
                        rasterPath,
                        projectFolderPath,
                        crsContext.TargetSrsDefinition,
                        ct);

                    if (freshImport != null)
                    {
                        layer.SourceFile = freshImport.RelativePath;
                        layer.Description = UpdateRefreshDescription(
                            layer.Description,
                            freshImport.RelativePath,
                            crsContext.CoordinateSystem.Code);
                        layer.LastModifiedDate = DateTime.Now;
                        await layerRepository.UpdateAsync(layer, ct);
                        updatedCount++;
                        continue;
                    }

                    RasterProjectReprojectionResult reprojectionResult = await Task.Run(
                        () => _datasetImporter.TryReprojectProjectRasterToProjectCrs(
                            rasterPath,
                            crsContext.TargetSrsDefinition),
                        ct);

                    if (!reprojectionResult.Reprojected)
                    {
                        skippedCount++;
                        continue;
                    }

                    layer.Description = AppendRefreshNote(
                        layer.Description,
                        crsContext.CoordinateSystem.Code);
                    layer.LastModifiedDate = DateTime.Now;
                    await layerRepository.UpdateAsync(layer, ct);
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    session.Logger.LogError(
                        $"Failed to refresh live XYZ layer CRS. Layer={layer.Name}",
                        ex);
                }
            }

            return new RasterLayerProjectionUpdateResult(
                updatedCount,
                skippedCount,
                failedCount);
        }

        /// <summary>
        /// Reprojects all project raster layers that already have georeferencing and a stored CRS.
        /// </summary>
        public async Task<RasterLayerProjectionUpdateResult> ReprojectRasterLayersToProjectCrsAsync(
            ProjectSession session,
            string projectFolderPath,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (string.IsNullOrWhiteSpace(projectFolderPath) ||
                !Directory.Exists(projectFolderPath))
            {
                throw new DirectoryNotFoundException(
                    "Project folder was not found.");
            }

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(session);

            ProjectRasterCrsContext crsContext =
                await _crsResolver.ResolveAsync(session, ct);

            List<CanvasLayer> rasterLayers =
                await layerRepository.GetAllByLayerTypeOrderedAsync(
                    CanvasLayerTreeService.RasterLayerType,
                    ct);

            int updatedCount = 0;
            int skippedCount = 0;
            int failedCount = 0;

            foreach (CanvasLayer layer in rasterLayers)
            {
                ct.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(layer.SourceFile))
                {
                    skippedCount++;
                    continue;
                }

                string rasterPath = ResolveLayerFilePath(
                    projectFolderPath,
                    layer.SourceFile);

                try
                {
                    RasterDatasetImportOutput? freshImport = await TryReimportFromOriginalSourceAsync(
                        layer,
                        rasterPath,
                        projectFolderPath,
                        crsContext.TargetSrsDefinition,
                        ct);

                    if (freshImport != null)
                    {
                        layer.SourceFile = freshImport.RelativePath;
                        layer.Description = UpdateRefreshDescription(
                            layer.Description,
                            freshImport.RelativePath,
                            crsContext.CoordinateSystem.Code);
                        layer.LastModifiedDate = DateTime.Now;
                        await layerRepository.UpdateAsync(layer, ct);
                        updatedCount++;
                        continue;
                    }

                    RasterProjectReprojectionResult reprojectionResult = await Task.Run(
                        () => _datasetImporter.TryReprojectProjectRasterToProjectCrs(
                            rasterPath,
                            crsContext.TargetSrsDefinition),
                        ct);

                    if (!reprojectionResult.Reprojected)
                    {
                        skippedCount++;
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(
                            reprojectionResult.UpdatedRasterPath))
                    {
                        layer.SourceFile = ToProjectRelativePath(
                            projectFolderPath,
                            reprojectionResult.UpdatedRasterPath);
                    }

                    layer.Description = AppendRefreshNote(
                        layer.Description,
                        crsContext.CoordinateSystem.Code);
                    layer.LastModifiedDate = DateTime.Now;
                    await layerRepository.UpdateAsync(layer, ct);
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    session.Logger.LogError(
                        $"Failed to update raster layer CRS. Layer={layer.Name}",
                        ex);
                }
            }

            return new RasterLayerProjectionUpdateResult(
                updatedCount,
                skippedCount,
                failedCount);
        }

        private static bool IsLiveTileVrtPath(string path)
        {
            if (!string.Equals(
                    Path.GetExtension(path),
                    ".vrt",
                    StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(path))
            {
                return false;
            }

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

        private async Task<RasterDatasetImportOutput?> TryReimportFromOriginalSourceAsync(
            CanvasLayer layer,
            string currentRasterPath,
            string projectFolderPath,
            string targetSrsDefinition,
            CancellationToken ct)
        {
            if (!TryGetOriginalSourcePath(
                    layer.Description,
                    projectFolderPath,
                    out string? originalSourcePath) ||
                !File.Exists(originalSourcePath))
            {
                return null;
            }

            string fullCurrentPath = Path.GetFullPath(currentRasterPath);
            string fullOriginalPath = Path.GetFullPath(originalSourcePath);
            if (string.Equals(fullCurrentPath, fullOriginalPath, StringComparison.OrdinalIgnoreCase))
                return null;

            RasterSourceExtent? sourceExtent = null;
            string? sourceSrsOverride = null;

            if (IsLiveTileSourceDescriptor(fullOriginalPath))
            {
                sourceSrsOverride = "EPSG:3857";
                sourceExtent = new RasterSourceExtent(
                    "EPSG:3857",
                    -20037508.342789244,
                    -20037508.342789244,
                    20037508.342789244,
                    20037508.342789244);
            }

            return await Task.Run(
                () => _datasetImporter.ImportToProjectCrs(
                    fullOriginalPath,
                    projectFolderPath,
                    layer.Name,
                    targetSrsDefinition,
                    sourceSrsOverride,
                    sourceExtent),
                ct);
        }

        private static bool TryGetOriginalSourcePath(
            string? description,
            string projectFolderPath,
            out string? sourcePath)
        {
            sourcePath = null;
            if (string.IsNullOrWhiteSpace(description))
                return false;

            foreach (string rawLine in description.Split(
                         ['\r', '\n'],
                         StringSplitOptions.RemoveEmptyEntries))
            {
                string line = rawLine.Trim();
                if (!line.StartsWith("Source:", StringComparison.OrdinalIgnoreCase))
                    continue;

                string value = line["Source:".Length..].Trim();
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                sourcePath = Path.IsPathRooted(value)
                    ? value
                    : Path.Combine(projectFolderPath, value);
                return true;
            }

            return false;
        }

        private static bool IsLiveTileSourceDescriptor(string path)
        {
            if (!string.Equals(
                    Path.GetExtension(path),
                    ".xml",
                    StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(path))
            {
                return false;
            }

            try
            {
                string content = File.ReadAllText(path);
                return content.Contains("GDAL_WMS", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Resolves project-relative raster paths to absolute file paths.
        /// </summary>
        private static string ResolveLayerFilePath(
            string projectFolderPath,
            string sourceFile)
        {
            return Path.IsPathRooted(sourceFile)
                ? sourceFile
                : Path.Combine(projectFolderPath, sourceFile);
        }

        private static string ToProjectRelativePath(
            string projectFolderPath,
            string rasterPath)
        {
            string fullProjectPath = Path.GetFullPath(projectFolderPath);
            string fullRasterPath = Path.GetFullPath(rasterPath);

            try
            {
                string relativePath = Path.GetRelativePath(
                    fullProjectPath,
                    fullRasterPath);
                return relativePath.StartsWith("..", StringComparison.Ordinal)
                    ? fullRasterPath
                    : relativePath;
            }
            catch
            {
                return fullRasterPath;
            }
        }

        /// <summary>
        /// Adds a short audit note to the layer description after CRS refresh.
        /// </summary>
        private static string AppendRefreshNote(
            string? description,
            string projectCrsCode)
        {
            string note =
                $"Project CRS refreshed to {projectCrsCode} on {DateTime.Now:yyyy-MM-dd HH:mm}.";

            return string.IsNullOrWhiteSpace(description)
                ? note
                : $"{description.TrimEnd()}{Environment.NewLine}{note}";
        }

        private static string UpdateRefreshDescription(
            string? description,
            string refreshedRelativePath,
            string projectCrsCode)
        {
            string refreshed = ReplaceDescriptionLine(
                description,
                "Saved raster:",
                $"Saved raster: {refreshedRelativePath}");

            return AppendRefreshNote(refreshed, projectCrsCode);
        }

        private static string ReplaceDescriptionLine(
            string? description,
            string prefix,
            string replacement)
        {
            if (string.IsNullOrWhiteSpace(description))
                return replacement;

            string[] lines = description.Split(
                [Environment.NewLine],
                StringSplitOptions.None);
            bool replaced = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].TrimStart().StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                lines[i] = replacement;
                replaced = true;
                break;
            }

            return replaced
                ? string.Join(Environment.NewLine, lines)
                : $"{description.TrimEnd()}{Environment.NewLine}{replacement}";
        }
    }

    /// <summary>
    /// Summarizes how many raster layers were updated, skipped, or failed during CRS refresh.
    /// </summary>
    public sealed record RasterLayerProjectionUpdateResult(
        int UpdatedCount,
        int SkippedCount,
        int FailedCount)
    {
        public static RasterLayerProjectionUpdateResult Empty { get; } = new(0, 0, 0);
        public bool HasWork => UpdatedCount > 0 || SkippedCount > 0 || FailedCount > 0;
    }
}
