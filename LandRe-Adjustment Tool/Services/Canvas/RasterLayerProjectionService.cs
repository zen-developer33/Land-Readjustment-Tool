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
                    bool reprojected = await Task.Run(
                        () => _datasetImporter.TryReprojectProjectRasterToProjectCrs(
                            rasterPath,
                            crsContext.TargetSrsDefinition,
                            out _),
                        ct);

                    if (!reprojected)
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
                        $"Failed to update raster layer CRS. Layer={layer.Name}",
                        ex);
                }
            }

            return new RasterLayerProjectionUpdateResult(
                updatedCount,
                skippedCount,
                failedCount);
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
