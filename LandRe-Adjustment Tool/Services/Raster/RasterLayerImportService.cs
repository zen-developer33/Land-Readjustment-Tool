using System.Globalization;
using System.Text;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Orchestrates raster import from source file through project CRS normalization and layer persistence.
    /// </summary>
    public sealed class RasterLayerImportService : IRasterLayerImportService
    {
        private readonly IProjectScopedFactory _projectScopedFactory;
        private readonly IProjectRasterCrsResolver _crsResolver;
        private readonly IRasterDatasetImporter _datasetImporter;

        /// <summary>
        /// Creates the raster layer import service.
        /// </summary>
        public RasterLayerImportService(
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

        /// <inheritdoc />
        public async Task<RasterLayerImportPreview> PrepareImportAsync(
            ProjectSession session,
            string sourcePath,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException(
                    "Raster source path is required.",
                    nameof(sourcePath));

            ProjectRasterCrsContext crsContext =
                await _crsResolver.ResolveAsync(session, ct);

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(session);
            List<CanvasLayer> existingLayers =
                await layerRepository.GetAllOrderedAsync(ct);

            RasterDatasetMetadata metadata = await Task.Run(
                () => _datasetImporter.ReadMetadata(sourcePath),
                ct);

            string layerName = BuildUniqueLayerName(
                Path.GetFileNameWithoutExtension(sourcePath),
                existingLayers);

            return new RasterLayerImportPreview(
                layerName,
                metadata,
                crsContext);
        }

        /// <inheritdoc />
        public async Task<RasterLayerImportResult> ImportAsync(
            RasterLayerImportRequest request,
            IProgress<RasterImportProgressInfo>? progress = null,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Session);

            try
            {
                progress?.Report(new RasterImportProgressInfo(4, "Loading project CRS settings"));

                ProjectRasterCrsContext crsContext =
                    await _crsResolver.ResolveAsync(request.Session, ct);

                progress?.Report(new RasterImportProgressInfo(12, "Checking existing map layers"));

                var layerRepository =
                    _projectScopedFactory.CreateCanvasLayerRepository(request.Session);
                List<CanvasLayer> existingLayers =
                    await layerRepository.GetAllOrderedAsync(ct);

                string layerName = string.IsNullOrWhiteSpace(request.LayerName)
                    ? BuildUniqueLayerName(
                        Path.GetFileNameWithoutExtension(request.SourcePath),
                        existingLayers)
                    : request.LayerName.Trim();

                RasterDatasetImportOutput datasetOutput = await Task.Run(
                    () => _datasetImporter.ImportToProjectCrs(
                        request.SourcePath,
                        request.ProjectFolderPath,
                        layerName,
                        crsContext.TargetSrsDefinition,
                        request.SourceSrsDefinitionOverride,
                        progress),
                    ct);

                progress?.Report(new RasterImportProgressInfo(90, "Creating raster map layer"));

                CanvasLayer rasterLayer = CreateRasterLayer(
                    layerName,
                    datasetOutput,
                    crsContext.CoordinateSystem.Code,
                    existingLayers);

                CanvasLayer savedLayer = await layerRepository.AddAsync(rasterLayer, ct);

                string details = BuildDisplayText(
                    layerName,
                    datasetOutput,
                    crsContext.CoordinateSystem.Code);
                string heading = BuildHeading(layerName, datasetOutput.ImportMode);
                string? warning = GetImportWarning(datasetOutput.ImportMode);

                request.Session.Logger.LogInfo(
                    $"Raster import completed. Layer={layerName}, Mode={datasetOutput.ImportMode}");
                request.Session.Logger.LogInfo(
                    $"Raster import metadata:{Environment.NewLine}{details}");

                progress?.Report(new RasterImportProgressInfo(100, "Raster added to map canvas"));

                return new RasterLayerImportResult(
                    savedLayer,
                    datasetOutput,
                    heading,
                    details,
                    warning);
            }
            catch (Exception ex)
            {
                request.Session.Logger.LogError(
                    $"Raster import failed. Source={request.SourcePath}",
                    ex);
                throw;
            }
        }

        /// <summary>
        /// Builds the persistent canvas layer record for an imported raster source.
        /// </summary>
        private static CanvasLayer CreateRasterLayer(
            string layerName,
            RasterDatasetImportOutput datasetOutput,
            string projectCrsCode,
            IReadOnlyCollection<CanvasLayer> existingLayers)
        {
            int nextDisplayOrder = existingLayers.Count == 0
                ? 0
                : existingLayers.Max(layer => layer.DisplayOrder) + 1;

            return new CanvasLayer
            {
                Name = layerName,
                LayerType = CanvasLayerTreeService.RasterLayerType,
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = nextDisplayOrder,
                BorderColor = "#4B8BBE",
                LineWeight = 1.0,
                LineStyle = "Solid",
                FillTransparency = 0,
                FillStyle = "None",
                LabelColor = "#000000",
                PointSymbol = "Circle",
                PointSize = 5.0,
                SourceFile = datasetOutput.RelativePath,
                ImportedDate = DateTime.Now,
                Description = BuildLayerDescription(
                    layerName,
                    datasetOutput,
                    projectCrsCode)
            };
        }

        /// <summary>
        /// Creates a unique layer name based on the source file name.
        /// </summary>
        private static string BuildUniqueLayerName(
            string? desiredName,
            IEnumerable<CanvasLayer> existingLayers)
        {
            string baseName = string.IsNullOrWhiteSpace(desiredName)
                ? "Raster"
                : desiredName.Trim();

            HashSet<string> existingNames = existingLayers
                .Select(layer => layer.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existingNames.Contains(baseName))
                return baseName;

            for (int counter = 1; counter < 10000; counter++)
            {
                string candidate = $"{baseName}_{counter}";
                if (!existingNames.Contains(candidate))
                    return candidate;
            }

            throw new InvalidOperationException(
                "Could not create a unique raster layer name.");
        }

        /// <summary>
        /// Builds the short message shown after raster import completes.
        /// </summary>
        private static string BuildHeading(
            string layerName,
            RasterDatasetImportMode importMode)
        {
            return importMode switch
            {
                RasterDatasetImportMode.ProjectedToProjectCrs =>
                    $"Imported '{layerName}' successfully.",
                RasterDatasetImportMode.SourceCrsDefinedProjectedToProjectCrs =>
                    $"Imported '{layerName}' using the defined source CRS.",
                RasterDatasetImportMode.UnknownCrsCopiedWithoutProjection =>
                    $"Imported '{layerName}', but the raster CRS is unknown.",
                RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates =>
                    $"Imported '{layerName}' for temporary display only.",
                _ => $"Imported '{layerName}'."
            };
        }

        /// <summary>
        /// Returns a warning when the raster was imported without reliable spatial placement.
        /// </summary>
        private static string? GetImportWarning(RasterDatasetImportMode importMode)
        {
            return importMode switch
            {
                RasterDatasetImportMode.UnknownCrsCopiedWithoutProjection =>
                    "Warning: this raster has map coordinates, but no coordinate reference system was found.",
                RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates =>
                    "Warning: this raster is not georeferenced. The map placement is temporary image coordinates only, so the map will not be spatially correct.",
                _ => null
            };
        }

        /// <summary>
        /// Builds the database layer description used for audit and later inspection.
        /// </summary>
        private static string BuildLayerDescription(
            string layerName,
            RasterDatasetImportOutput datasetOutput,
            string projectCrsCode)
        {
            RasterDatasetMetadata metadata = datasetOutput.SourceMetadata;
            StringBuilder builder = new();
            AppendCommonSummary(
                builder,
                layerName,
                datasetOutput,
                projectCrsCode);

            if (metadata.HasGeoTransform)
            {
                builder.AppendLine(
                    $"GeoTransform: {string.Join(", ", metadata.GeoTransform.Select(FormatDouble))}");
            }
            else if (datasetOutput.ImportMode ==
                     RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates)
            {
                builder.AppendLine(
                    $"Temporary display extent: X 0 to {metadata.Width}, Y 0 to {metadata.Height} image units.");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Builds detailed import text for the final user message and project log.
        /// </summary>
        private static string BuildDisplayText(
            string layerName,
            RasterDatasetImportOutput datasetOutput,
            string projectCrsCode)
        {
            RasterDatasetMetadata metadata = datasetOutput.SourceMetadata;
            StringBuilder builder = new();
            AppendCommonSummary(
                builder,
                layerName,
                datasetOutput,
                projectCrsCode);

            if (metadata.HasGeoTransform)
            {
                builder.AppendLine();
                builder.AppendLine("GeoTransform");
                builder.AppendLine(
                    $"  Origin: {FormatDouble(metadata.GeoTransform[0])}, {FormatDouble(metadata.GeoTransform[3])}");
                builder.AppendLine(
                    $"  Pixel size: {FormatDouble(metadata.GeoTransform[1])}, {FormatDouble(metadata.GeoTransform[5])}");
                builder.AppendLine(
                    $"  Rotation: {FormatDouble(metadata.GeoTransform[2])}, {FormatDouble(metadata.GeoTransform[4])}");
            }
            else if (datasetOutput.ImportMode ==
                     RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates)
            {
                builder.AppendLine();
                builder.AppendLine("Temporary display extent");
                builder.AppendLine(
                    $"  X: 0 to {metadata.Width}, Y: 0 to {metadata.Height} image units");
                builder.AppendLine(
                    "  This is not a GIS location. Georeference the raster to align it with map data.");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Appends metadata fields shared by user-facing and database descriptions.
        /// </summary>
        private static void AppendCommonSummary(
            StringBuilder builder,
            string layerName,
            RasterDatasetImportOutput datasetOutput,
            string projectCrsCode)
        {
            RasterDatasetMetadata metadata = datasetOutput.SourceMetadata;
            builder.AppendLine($"Layer: {layerName}");
            builder.AppendLine($"Source: {metadata.SourcePath}");
            builder.AppendLine($"Saved raster: {datasetOutput.RelativePath}");
            builder.AppendLine($"Driver: {metadata.DriverShortName} - {metadata.DriverLongName}");
            builder.AppendLine(
                $"Size: {metadata.Width} x {metadata.Height} pixels, {metadata.BandCount} band(s)");
            builder.AppendLine($"File size: {FormatFileSize(metadata.FileSizeBytes)}");
            builder.AppendLine(
                $"Georeferenced: {(metadata.HasGeoreferencing ? "Yes" : "No")}");
            builder.AppendLine(
                $"GeoTransform: {(metadata.HasGeoTransform ? "Available" : "Not available")}");
            builder.AppendLine(
                $"Ground control points: {metadata.GroundControlPointCount}");
            builder.AppendLine(
                $"Raster CRS stored: {(metadata.HasProjection ? "Yes" : "No")}");
            builder.AppendLine($"CRS source: {metadata.ProjectionSource}");
            builder.AppendLine($"CRS type: {metadata.CoordinateSystemType}");
            builder.AppendLine($"CRS name: {metadata.CoordinateSystemName}");
            builder.AppendLine($"CRS authority: {metadata.CoordinateSystemAuthority}");
            builder.AppendLine($"Project CRS: {projectCrsCode}");
            builder.AppendLine($"Import CRS action: {GetImportAction(datasetOutput.ImportMode)}");
            builder.AppendLine($"Display placement: {GetDisplayPlacement(datasetOutput.ImportMode)}");
        }

        /// <summary>
        /// Describes how the source CRS was handled during import.
        /// </summary>
        private static string GetImportAction(RasterDatasetImportMode importMode)
        {
            return importMode switch
            {
                RasterDatasetImportMode.ProjectedToProjectCrs =>
                    "Raster CRS was found; raster was warped to the project CRS.",
                RasterDatasetImportMode.SourceCrsDefinedProjectedToProjectCrs =>
                    "Raster CRS was not stored; source CRS was defined during import and raster was warped to the project CRS.",
                RasterDatasetImportMode.UnknownCrsCopiedWithoutProjection =>
                    "Raster has map coordinates but no CRS; copied without projection because the source CRS is unknown.",
                RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates =>
                    "Raster has no map coordinates; copied without projection for temporary display only.",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Describes where the imported raster will appear on the map canvas.
        /// </summary>
        private static string GetDisplayPlacement(RasterDatasetImportMode importMode)
        {
            return importMode switch
            {
                RasterDatasetImportMode.ProjectedToProjectCrs =>
                    "Project CRS coordinates.",
                RasterDatasetImportMode.SourceCrsDefinedProjectedToProjectCrs =>
                    "Project CRS coordinates using the source CRS defined during import.",
                RasterDatasetImportMode.UnknownCrsCopiedWithoutProjection =>
                    "Original stored raster coordinates; alignment depends on the user later defining the correct CRS.",
                RasterDatasetImportMode.UnreferencedCopiedToLocalCoordinates =>
                    "Local image coordinates from pixel size only; not georeferenced and not spatially aligned.",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Formats source file size for readable import details.
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            double value = bytes / 1024d;
            string[] units = ["KB", "MB", "GB", "TB"];

            foreach (string unit in units)
            {
                if (value < 1024d)
                    return $"{value:0.##} {unit}";

                value /= 1024d;
            }

            return $"{value:0.##} PB";
        }

        /// <summary>
        /// Formats coordinate and transform values using culture-invariant decimal text.
        /// </summary>
        private static string FormatDouble(double value)
        {
            return value.ToString("0.########", CultureInfo.InvariantCulture);
        }
    }
}
