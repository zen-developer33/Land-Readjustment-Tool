using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OSGeo.OSR;

namespace Land_Readjustment_Tool.Services.Import
{
    public interface IBoundaryImportService
    {
        Task<BoundaryImportResult> ImportProjectBoundaryAsync(
            ProjectSession session,
            string filePath,
            BoundaryImportOptions options,
            CancellationToken ct = default);
    }

    public sealed class BoundaryImportService : IBoundaryImportService
    {
        private const string ProjectBoundaryLayerName = "Project Boundary";
        private const string ProjectBoundaryLayerType = "ProjectBoundary";

        private readonly IBoundaryReaderFactory _readerFactory;
        private readonly IProjectScopedFactory _projectScopedFactory;
        private readonly IProjectRasterCrsResolver _projectCrsResolver;

        public BoundaryImportService(
            IBoundaryReaderFactory readerFactory,
            IProjectScopedFactory projectScopedFactory,
            IProjectRasterCrsResolver projectCrsResolver)
        {
            _readerFactory = readerFactory;
            _projectScopedFactory = projectScopedFactory;
            _projectCrsResolver = projectCrsResolver;
        }

        public async Task<BoundaryImportResult> ImportProjectBoundaryAsync(
            ProjectSession session,
            string filePath,
            BoundaryImportOptions options,
            CancellationToken ct = default)
        {
            IReadOnlyList<Geometry> sourceGeometries =
                _readerFactory.GetReader(filePath).ReadGeometries(filePath, options);

            if (sourceGeometries.Count == 0)
            {
                return new BoundaryImportResult(
                    false,
                    "No closed boundary polylines or polygons were found in the selected layer.",
                    0,
                    null);
            }

            ProjectRasterCrsContext projectCrs =
                await _projectCrsResolver.ResolveAsync(session, ct);

            List<Geometry> geometries = NeedsTransform(
                    options.SourceCrsCode,
                    projectCrs.TargetSrsDefinition)
                ? TransformGeometries(
                    sourceGeometries,
                    options.SourceCrsCode,
                    projectCrs.TargetSrsDefinition)
                : sourceGeometries.Select(geometry => geometry.Copy()).ToList();

            foreach (Geometry geometry in geometries)
                NormalizeGeometryForCanvasDatabase(geometry);

            AppDbContext context = session.GetDbContext();
            await context.Database.MigrateAsync(ct);

            CanvasLayer boundaryLayer = await GetOrCreateProjectBoundaryLayerAsync(
                context,
                ct);
            ApplyProjectBoundaryDefaultStyle(boundaryLayer);

            DateTime now = DateTime.Now;
            List<CanvasObject> objects = geometries
                .Select(geometry => new CanvasObject
                {
                    CanvasLayerId = boundaryLayer.Id,
                    CanvasLayer = boundaryLayer,
                    ObjectType = "Polygon",
                    Shape = geometry,
                    ObjectDescription = $"Project Boundary imported from {Path.GetFileName(filePath)}",
                    IsVisible = true,
                    IsLocked = false,
                    CreatedDate = now,
                    LastModifiedDate = now
                })
                .ToList();

            await context.CanvasObjects.AddRangeAsync(objects, ct);
            boundaryLayer.SourceFile = filePath;
            boundaryLayer.ImportedDate = now;
            boundaryLayer.LastModifiedDate = now;

            try
            {
                await context.SaveChangesAsync(ct);
                context.Entry(boundaryLayer).State = EntityState.Detached;
                foreach (CanvasObject canvasObject in objects)
                    context.Entry(canvasObject).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                session.Logger.LogError("Project boundary import save failed.", ex);
                return new BoundaryImportResult(
                    false,
                    $"Could not save the project boundary: {BuildExceptionMessage(ex)}",
                    0,
                    null);
            }

            Envelope envelope = new();
            foreach (Geometry geometry in geometries)
                envelope.ExpandToInclude(geometry.EnvelopeInternal);

            return new BoundaryImportResult(
                true,
                null,
                objects.Count,
                envelope);
        }

        private static void ApplyProjectBoundaryDefaultStyle(CanvasLayer layer)
        {
            layer.FillStyle = "None";
            layer.FillColor = null;
            layer.FillTransparency = 0;

            if (string.IsNullOrWhiteSpace(layer.BorderColor))
                layer.BorderColor = "#CF7C82";

            if (layer.LineWeight <= 0)
                layer.LineWeight = 2.0;

            if (string.IsNullOrWhiteSpace(layer.LineStyle))
                layer.LineStyle = "Solid";
        }

        private static void NormalizeGeometryForCanvasDatabase(Geometry geometry)
        {
            geometry.SRID = 0;
            for (int index = 0; index < geometry.NumGeometries; index++)
                geometry.GetGeometryN(index).SRID = 0;
        }

        private static string BuildExceptionMessage(Exception ex)
        {
            List<string> messages = [];
            Exception? current = ex;
            while (current != null)
            {
                if (!string.IsNullOrWhiteSpace(current.Message) &&
                    !messages.Contains(current.Message))
                {
                    messages.Add(current.Message);
                }

                current = current.InnerException;
            }

            return string.Join(" ", messages);
        }

        private static async Task<CanvasLayer> GetOrCreateProjectBoundaryLayerAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            CanvasLayer? boundaryLayer = await context.CanvasLayers
                .FirstOrDefaultAsync(
                    layer => layer.Name == ProjectBoundaryLayerName ||
                             layer.LayerType == ProjectBoundaryLayerType,
                    ct);

            if (boundaryLayer != null)
                return boundaryLayer;

            DateTime now = DateTime.Now;
            int displayOrder =
                (await context.CanvasLayers
                    .Select(layer => (int?)layer.DisplayOrder)
                    .MaxAsync(ct) ?? -1) + 1;

            boundaryLayer = new CanvasLayer
            {
                Name = ProjectBoundaryLayerName,
                LayerType = ProjectBoundaryLayerType,
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = displayOrder,
                BorderColor = "#CF7C82",
                LineWeight = 2.0,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                FillColor = null,
                FillTransparency = 0,
                FillStyle = "None",
                LabelColor = "#000000",
                PointSymbol = "Dot",
                PointSize = 5.0,
                CreatedDate = now,
                LastModifiedDate = now,
                Description = $"Default layer: {ProjectBoundaryLayerName}"
            };

            await context.CanvasLayers.AddAsync(boundaryLayer, ct);
            return boundaryLayer;
        }

        private static bool NeedsTransform(string source, string target)
        {
            return !string.Equals(
                NormalizeDefinition(SanitizeCrsDefinition(source)),
                NormalizeDefinition(SanitizeCrsDefinition(target)),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDefinition(string value)
        {
            return value.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        private static List<Geometry> TransformGeometries(
            IReadOnlyList<Geometry> sourceGeometries,
            string sourceDefinition,
            string targetDefinition)
        {
            GdalBootstrapper.ConfigureAll();
            using SpatialReference sourceSrs = CreateSpatialReference(sourceDefinition);
            using SpatialReference targetSrs = CreateSpatialReference(targetDefinition);
            using CoordinateTransformation transformation = new(sourceSrs, targetSrs);

            List<Geometry> transformed = [];
            foreach (Geometry sourceGeometry in sourceGeometries)
            {
                Geometry copy = sourceGeometry.Copy();
                copy.Apply(new CoordinateTransformFilter(transformation));
                copy.GeometryChanged();
                Geometry? valid = Readers.BoundaryGeometryReaderHelpers
                    .ValidatePolygonalGeometry(copy);
                if (valid != null)
                    transformed.Add(valid);
            }

            return transformed;
        }

        private static SpatialReference CreateSpatialReference(string definition)
        {
            definition = SanitizeCrsDefinition(definition);

            SpatialReference spatialReference = new(string.Empty);
            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            if (spatialReference.SetFromUserInput(definition) != 0)
            {
                string wkt = definition;
                if (spatialReference.ImportFromWkt(ref wkt) != 0)
                {
                    spatialReference.Dispose();
                    throw new InvalidOperationException(
                        $"Could not parse CRS definition '{definition}'.");
                }
            }

            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            return spatialReference;
        }

        private static string SanitizeCrsDefinition(string definition)
        {
            return ProjectCrsWktBuilder.SanitizeForProj(definition);
        }

        private sealed class CoordinateTransformFilter : ICoordinateSequenceFilter
        {
            private readonly CoordinateTransformation _transformation;

            public CoordinateTransformFilter(CoordinateTransformation transformation)
            {
                _transformation = transformation;
            }

            public bool Done => false;
            public bool GeometryChanged => true;

            public void Filter(CoordinateSequence seq, int i)
            {
                double[] point = [seq.GetX(i), seq.GetY(i), 0.0];
                _transformation.TransformPoint(point);

                if (!double.IsFinite(point[0]) || !double.IsFinite(point[1]))
                    throw new InvalidOperationException("A boundary coordinate could not be transformed.");

                seq.SetX(i, point[0]);
                seq.SetY(i, point[1]);
            }
        }
    }
}
