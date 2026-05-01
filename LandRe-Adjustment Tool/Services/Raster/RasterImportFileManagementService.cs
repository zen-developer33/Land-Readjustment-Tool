using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Keeps project-owned raster files aligned with raster layer and save state.
    /// </summary>
    public sealed class RasterImportFileManagementService
    {
        private const string RasterFolderName = "RasterLayers";
        private readonly IProjectScopedFactory _projectScopedFactory;

        public RasterImportFileManagementService(
            IProjectScopedFactory projectScopedFactory)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
        }

        public void RegisterImportedRaster(
            ProjectContext context,
            RasterLayerImportResult importResult)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(importResult);

            if (!TryResolveManagedRasterPath(
                    importResult.Dataset.AbsolutePath,
                    context.ProjectFolderPath,
                    out string managedPath))
            {
                context.Session.Logger.LogWarning(
                    $"Raster import file was not tracked because it is outside the project raster folder. Path={importResult.Dataset.AbsolutePath}");
                return;
            }

            context.TrackImportedRasterFile(managedPath);
            context.Session.Logger.LogInfo(
                $"Raster import file tracked until project save. Layer={importResult.Layer.Name}, Path={managedPath}");
        }

        public void HandleLayerDeleted(
            ProjectContext context,
            CanvasLayer layer)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(layer);

            if (!IsRasterLayer(layer) ||
                string.IsNullOrWhiteSpace(layer.SourceFile))
            {
                return;
            }

            if (!TryResolveManagedRasterPath(
                    layer.SourceFile,
                    context.ProjectFolderPath,
                    out string managedPath))
            {
                context.Session.Logger.LogWarning(
                    $"Raster layer file was not deleted because it is outside the project raster folder. Layer={layer.Name}, SourceFile={layer.SourceFile}");
                return;
            }

            if (context.IsPendingImportedRasterFile(managedPath))
            {
                DeleteRasterAndCompanionFiles(
                    managedPath,
                    context.Session.Logger,
                    $"Deleted unsaved imported raster file for removed layer '{layer.Name}'.");
                context.UntrackImportedRasterFile(managedPath);
                return;
            }

            context.TrackDeletedRasterFile(managedPath);
            context.Session.Logger.LogInfo(
                $"Raster file cleanup staged until project save. Layer={layer.Name}, Path={managedPath}");
        }

        public void CleanupUnsavedImports(ProjectContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            int deletedCount = 0;
            foreach (string path in context.PendingImportedRasterFiles.ToList())
            {
                deletedCount += DeleteRasterAndCompanionFiles(
                    path,
                    context.Session.Logger,
                    "Deleted unsaved raster import because project changes were discarded.");
            }

            context.Session.Logger.LogInfo(
                $"Unsaved raster import cleanup completed. DeletedCount={deletedCount}, TrackedCount={context.PendingImportedRasterFiles.Count}");
            context.ClearPendingRasterImportChanges();
        }

        public void CommitPendingDeletes(ProjectContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            int deletedCount = 0;
            foreach (string path in context.PendingDeletedRasterFiles.ToList())
            {
                deletedCount += DeleteRasterAndCompanionFiles(
                    path,
                    context.Session.Logger,
                    "Deleted raster file for a removed layer after project save.");
            }

            context.Session.Logger.LogInfo(
                $"Raster import save cleanup completed. DeletedCount={deletedCount}, PendingDeleteCount={context.PendingDeletedRasterFiles.Count}, SavedImportCount={context.PendingImportedRasterFiles.Count}");
        }

        public async Task CleanupUnreferencedProjectRastersAsync(
            ProjectContext context,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            string rasterFolder = Path.Combine(
                context.ProjectFolderPath,
                RasterFolderName);

            if (!Directory.Exists(rasterFolder))
            {
                context.Session.Logger.LogInfo(
                    "Raster orphan cleanup skipped because the project raster folder does not exist.");
                return;
            }

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(context.Session);
            List<CanvasLayer> rasterLayers =
                await layerRepository.GetAllByLayerTypeOrderedAsync(
                    CanvasLayerTreeService.RasterLayerType,
                    ct);

            HashSet<string> referencedFiles = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> protectedFiles = new(StringComparer.OrdinalIgnoreCase);
            foreach (CanvasLayer layer in rasterLayers)
            {
                if (!string.IsNullOrWhiteSpace(layer.SourceFile) &&
                    TryResolveManagedRasterPath(
                        layer.SourceFile,
                        context.ProjectFolderPath,
                        out string path))
                {
                    referencedFiles.Add(path);
                    protectedFiles.Add(path);
                    foreach (string companionPath in EnumerateKnownCompanionPaths(path))
                    {
                        protectedFiles.Add(companionPath);
                    }
                }
            }

            int deletedCount = 0;
            foreach (string file in Directory.GetFiles(
                         rasterFolder,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                string fullPath = Path.GetFullPath(file);
                if (protectedFiles.Contains(fullPath))
                    continue;

                if (DeleteFileIfExists(
                    fullPath,
                    context.Session.Logger,
                    "Deleted unreferenced project raster file."))
                {
                    deletedCount++;
                }
            }

            context.Session.Logger.LogInfo(
                $"Raster orphan cleanup completed. DeletedCount={deletedCount}, ReferencedCount={referencedFiles.Count}");
        }

        public async Task<RasterLayerReferenceRepairResult>
            RepairRasterLayerReferencesAsync(
                ProjectContext context,
                bool removeMissingManagedLayers = true,
                CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(context.Session);
            List<CanvasLayer> rasterLayers =
                await layerRepository.GetAllByLayerTypeOrderedAsync(
                        CanvasLayerTreeService.RasterLayerType,
                        ct)
                    .ConfigureAwait(false);

            int updatedCount = 0;
            int deletedCount = 0;

            foreach (CanvasLayer layer in rasterLayers)
            {
                if (string.IsNullOrWhiteSpace(layer.SourceFile))
                {
                    if (!removeMissingManagedLayers)
                        continue;

                    await layerRepository.DeleteAsync(layer.Id, ct)
                        .ConfigureAwait(false);
                    deletedCount++;
                    context.Session.Logger.LogWarning(
                        $"Removed raster layer without source path. Layer={layer.Name}, Id={layer.Id}");
                    continue;
                }

                if (TryResolveExistingRasterPath(
                    layer.SourceFile,
                    context.ProjectFolderPath,
                    out string resolvedAbsolutePath))
                {
                    if (TryConvertToProjectRelativePath(
                        resolvedAbsolutePath,
                        context.ProjectFolderPath,
                        out string relativePath) &&
                        !string.Equals(
                            layer.SourceFile,
                            relativePath,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        layer.SourceFile = relativePath;
                        layer.LastModifiedDate = DateTime.Now;
                        await layerRepository.UpdateAsync(layer, ct)
                            .ConfigureAwait(false);
                        updatedCount++;
                        context.Session.Logger.LogInfo(
                            $"Repaired raster source path. Layer={layer.Name}, NewSource={relativePath}");
                    }

                    continue;
                }

                if (!removeMissingManagedLayers ||
                    !IsLikelyManagedRasterPath(
                        layer.SourceFile,
                        context.ProjectFolderPath))
                {
                    continue;
                }

                await layerRepository.DeleteAsync(layer.Id, ct)
                    .ConfigureAwait(false);
                deletedCount++;
                context.Session.Logger.LogWarning(
                    $"Removed raster layer whose managed source file is missing. Layer={layer.Name}, Source={layer.SourceFile}");
            }

            if (updatedCount > 0 || deletedCount > 0)
            {
                context.Session.Logger.LogInfo(
                    $"Raster layer reference repair completed. Updated={updatedCount}, Deleted={deletedCount}");
            }

            return new RasterLayerReferenceRepairResult(updatedCount, deletedCount);
        }

        private static bool IsRasterLayer(CanvasLayer layer)
        {
            return string.Equals(
                layer.LayerType,
                CanvasLayerTreeService.RasterLayerType,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryResolveManagedRasterPath(
            string storedPath,
            string projectFolderPath,
            out string managedPath)
        {
            managedPath = string.Empty;

            if (string.IsNullOrWhiteSpace(storedPath) ||
                string.IsNullOrWhiteSpace(projectFolderPath))
            {
                return false;
            }

            string candidate = Path.IsPathRooted(storedPath)
                ? Path.GetFullPath(storedPath)
                : Path.GetFullPath(Path.Combine(projectFolderPath, storedPath));

            string rasterFolder = EnsureTrailingDirectorySeparator(
                Path.GetFullPath(Path.Combine(projectFolderPath, RasterFolderName)));

            if (!candidate.StartsWith(rasterFolder, StringComparison.OrdinalIgnoreCase))
                return false;

            managedPath = candidate;
            return true;
        }

        private static bool TryResolveExistingRasterPath(
            string storedPath,
            string projectFolderPath,
            out string absolutePath)
        {
            absolutePath = string.Empty;
            if (string.IsNullOrWhiteSpace(storedPath))
                return false;

            string primaryPath = Path.IsPathRooted(storedPath)
                ? Path.GetFullPath(storedPath)
                : Path.GetFullPath(Path.Combine(projectFolderPath, storedPath));
            if (File.Exists(primaryPath))
            {
                absolutePath = primaryPath;
                return true;
            }

            string fileName = Path.GetFileName(storedPath);
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            string rasterFolder = Path.Combine(projectFolderPath, RasterFolderName);
            if (!Directory.Exists(rasterFolder))
                return false;

            string? fallbackPath = Directory
                .EnumerateFiles(rasterFolder, "*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault(path =>
                    string.Equals(
                        Path.GetFileName(path),
                        fileName,
                        StringComparison.OrdinalIgnoreCase));

            if (fallbackPath == null)
                return false;

            absolutePath = Path.GetFullPath(fallbackPath);
            return true;
        }

        private static bool TryConvertToProjectRelativePath(
            string absolutePath,
            string projectFolderPath,
            out string relativePath)
        {
            relativePath = string.Empty;

            if (string.IsNullOrWhiteSpace(absolutePath) ||
                string.IsNullOrWhiteSpace(projectFolderPath))
            {
                return false;
            }

            string fullProjectFolder = EnsureTrailingDirectorySeparator(
                Path.GetFullPath(projectFolderPath));
            string fullPath = Path.GetFullPath(absolutePath);

            if (!fullPath.StartsWith(
                fullProjectFolder,
                StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            relativePath = Path.GetRelativePath(projectFolderPath, fullPath);
            return true;
        }

        private static bool IsLikelyManagedRasterPath(
            string storedPath,
            string projectFolderPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath) ||
                string.IsNullOrWhiteSpace(projectFolderPath))
            {
                return false;
            }

            if (!Path.IsPathRooted(storedPath))
            {
                return true;
            }

            string fullPath = Path.GetFullPath(storedPath);
            string rasterFolder = EnsureTrailingDirectorySeparator(
                Path.GetFullPath(Path.Combine(projectFolderPath, RasterFolderName)));

            return fullPath.StartsWith(rasterFolder, StringComparison.OrdinalIgnoreCase);
        }

        private static bool DeleteFileIfExists(
            string path,
            IAppLogger logger,
            string reason)
        {
            try
            {
                if (!File.Exists(path))
                {
                    logger.LogInfo(
                        $"Raster file cleanup skipped because the file does not exist. Path={path}");
                    return false;
                }

                File.Delete(path);
                logger.LogInfo($"{reason} Path={path}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    $"Raster file cleanup failed. Path={path}",
                    ex);
                return false;
            }
        }

        private static int DeleteRasterAndCompanionFiles(
            string rasterPath,
            IAppLogger logger,
            string reason)
        {
            int deletedCount = 0;
            if (DeleteFileIfExists(rasterPath, logger, reason))
            {
                deletedCount++;
            }

            foreach (string companionPath in EnumerateKnownCompanionPaths(rasterPath))
            {
                if (DeleteFileIfExists(
                    companionPath,
                    logger,
                    $"{reason} (companion metadata)"))
                {
                    deletedCount++;
                }
            }

            return deletedCount;
        }

        private static IEnumerable<string> EnumerateKnownCompanionPaths(
            string rasterPath)
        {
            if (string.IsNullOrWhiteSpace(rasterPath))
            {
                yield break;
            }

            if (IsMbTilesPath(rasterPath))
            {
                yield return MbTilesLayerMetadataStore.GetSidecarPath(rasterPath);
            }
        }

        private static bool IsMbTilesPath(string path) =>
            string.Equals(
                Path.GetExtension(path),
                ".mbtiles",
                StringComparison.OrdinalIgnoreCase);

        private static string EnsureTrailingDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar) ||
                   path.EndsWith(Path.AltDirectorySeparatorChar)
                ? path
                : $"{path}{Path.DirectorySeparatorChar}";
        }
    }

    public sealed record RasterLayerReferenceRepairResult(
        int UpdatedCount,
        int DeletedCount);
}
