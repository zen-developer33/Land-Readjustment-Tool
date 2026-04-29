using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Canvas
{
    /// <summary>
    /// Updates persisted raster layer files when the project CRS or datum transformation changes.
    /// </summary>
    public sealed class RasterLayerProjectionService
    {
        private readonly IProjectScopedFactory _projectScopedFactory;

        /// <summary>
        /// Creates a raster projection updater using project-scoped repositories.
        /// </summary>
        public RasterLayerProjectionService(IProjectScopedFactory projectScopedFactory)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
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

            var settingsRepository =
                _projectScopedFactory.CreateProjectSettingsRepository(session);
            var coordinateSystemRepository =
                _projectScopedFactory.CreateCoordinateSystemRepository(session);
            var datumTransformationRepository =
                _projectScopedFactory.CreateDatumTransformationRepository(session);
            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(session);

            var settings = await settingsRepository.GetProjectSettingsAsync(ct);
            if (settings?.CoordinateSystemId == null)
                return RasterLayerProjectionUpdateResult.Empty;

            CoordinateSystem? projectCoordinateSystem =
                await coordinateSystemRepository.GetWithParametersAsync(
                    settings.CoordinateSystemId.Value,
                    ct);

            if (projectCoordinateSystem == null)
                throw new InvalidOperationException(
                    "The configured project coordinate system could not be loaded.");

            DatumTransformation? datumTransformation = null;
            if (settings.DatumTransformationId.HasValue)
            {
                datumTransformation =
                    await datumTransformationRepository.GetByIDAsync(
                        settings.DatumTransformationId.Value,
                        ct);
            }

            string targetSrsDefinition =
                ProjectCrsWktBuilder.BuildTargetSrsDefinition(
                    projectCoordinateSystem,
                    datumTransformation);

            List<CanvasLayer> rasterLayers =
                await layerRepository.GetAllByLayerTypeOrderedAsync(
                    CanvasLayerTreeService.RasterLayerType,
                    ct);

            RasterImportService importService = new();
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
                        () => importService.TryReprojectProjectRasterToProjectCrs(
                            rasterPath,
                            targetSrsDefinition,
                            out _),
                        ct);

                    if (!reprojected)
                    {
                        skippedCount++;
                        continue;
                    }

                    layer.Description = AppendRefreshNote(
                        layer.Description,
                        projectCoordinateSystem.Code);
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
