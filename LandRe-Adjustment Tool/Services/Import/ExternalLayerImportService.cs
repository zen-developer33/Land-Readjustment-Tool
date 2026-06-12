using System.Drawing;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.Services.Import.Readers;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OSGeo.OGR;
using OSGeo.OSR;
using AcadArc = ACadSharp.Entities.Arc;
using AcadCircle = ACadSharp.Entities.Circle;
using AcadEntity = ACadSharp.Entities.Entity;
using AcadInsert = ACadSharp.Entities.Insert;
using AcadLine = ACadSharp.Entities.Line;
using AcadLwPolyline = ACadSharp.Entities.LwPolyline;
using AcadMText = ACadSharp.Entities.MText;
using AcadPoint = ACadSharp.Entities.Point;
using AcadPolyline2D = ACadSharp.Entities.Polyline2D;
using AcadPolyline3D = ACadSharp.Entities.Polyline3D;
using AcadText = ACadSharp.Entities.TextEntity;
using DxfArc = netDxf.Entities.Arc;
using DxfCircle = netDxf.Entities.Circle;
using CadDocument = ACadSharp.CadDocument;
using DxfDocument = netDxf.DxfDocument;
using DxfLine = netDxf.Entities.Line;
using DxfMText = netDxf.Entities.MText;
using DxfPoint = netDxf.Entities.Point;
using DxfPolyline2D = netDxf.Entities.Polyline2D;
using DxfPolyline2DVertex = netDxf.Entities.Polyline2DVertex;
using DxfPolyline3D = netDxf.Entities.Polyline3D;
using DxfText = netDxf.Entities.Text;
using NtsEnvelope = NetTopologySuite.Geometries.Envelope;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace Land_Readjustment_Tool.Services.Import
{
    public interface IExternalLayerImportService
    {
        ExternalLayerFileInfo Inspect(string filePath);

        Task<ExternalLayerImportResult> ImportAsync(
            ProjectSession session,
            string filePath,
            ExternalLayerImportOptions options,
            Func<int, ImportDuplicateGeometryChoice>? resolveDuplicateGeometries = null,
            Func<int, ImportDuplicateGeometryChoice>? resolveProjectBoundaryConflict = null,
            CancellationToken ct = default);
    }

    public sealed class ExternalLayerImportService : IExternalLayerImportService
    {
        private static readonly GeometryFactory CanvasGeometryFactory = new(new PrecisionModel(), 0);

        // Tolerance (in source drawing units) for treating a polyline whose first and last points
        // coincide as a closed ring. Kept in sync with the file inspector so the geometry counts
        // shown in the mapping dialog match what is actually imported.
        private const double ClosedRingTolerance = 0.000001;

        // Tolerance for treating two geometries in the same layer as the same object.
        private const double DuplicateGeometryTolerance = 0.000001;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly CadastralVectorReader _cadReader = new();
        private readonly IProjectRasterCrsResolver _projectCrsResolver;

        public ExternalLayerImportService(IProjectRasterCrsResolver projectCrsResolver)
        {
            _projectCrsResolver = projectCrsResolver
                ?? throw new ArgumentNullException(nameof(projectCrsResolver));
        }

        public ExternalLayerFileInfo Inspect(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension is ".kml" or ".kmz")
                return InspectOgr(filePath, "KML");

            CadastralFileInfo info = _cadReader.Inspect(filePath);
            return new ExternalLayerFileInfo(
                info.FilePath,
                info.FileFormat,
                info.Layers
                    .Select(layer => new ExternalLayerInfo(
                        layer.Name,
                        layer.ObjectCount,
                        layer.ObjectTypes))
                    .ToList(),
                info.DetectedCrsCode,
                info.RequiresCrsFromUser);
        }

        public async Task<ExternalLayerImportResult> ImportAsync(
            ProjectSession session,
            string filePath,
            ExternalLayerImportOptions options,
            Func<int, ImportDuplicateGeometryChoice>? resolveDuplicateGeometries = null,
            Func<int, ImportDuplicateGeometryChoice>? resolveProjectBoundaryConflict = null,
            CancellationToken ct = default)
        {
            IReadOnlyList<ExternalRawObject> rawObjects = Read(filePath, options);
            if (rawObjects.Count == 0)
            {
                return new ExternalLayerImportResult(
                    false,
                    "No importable objects were found in the selected layer(s).",
                    0,
                    0,
                    null,
                    null);
            }

            ProjectRasterCrsContext projectCrs =
                await _projectCrsResolver.ResolveAsync(session, ct);
            List<ExternalRawObject> objects = NeedsTransform(
                    options.SourceCrsCode,
                    projectCrs.TargetSrsDefinition)
                ? Transform(rawObjects, options.SourceCrsCode, projectCrs.TargetSrsDefinition)
                : rawObjects
                    .Select(item => item with { Geometry = item.Geometry.Copy() })
                    .ToList();

            foreach (ExternalRawObject item in objects)
                NormalizeGeometryForCanvasDatabase(item.Geometry);

            // Drop objects whose geometry is incompatible with the application layer they were
            // mapped to: only closed polygons may enter area layers (Blocks, parcels, ...), and
            // only lines may enter line layers (Road Centerline). Skipped objects are reported back.
            List<string> importWarnings = [];
            objects = FilterObjectsForTargetCompatibility(objects, options, importWarnings);
            objects = FilterObjectsForCanvasDatabaseCompatibility(objects, importWarnings);
            if (objects.Count == 0)
            {
                string message = importWarnings.Count > 0
                    ? "No compatible objects were imported.\r\n\r\n" + string.Join("\r\n", importWarnings)
                    : "No importable objects were found in the selected layer(s).";
                return new ExternalLayerImportResult(false, message, 0, 0, null, null, importWarnings);
            }

            Dictionary<string, ExternalLayerStyle> sourceLayerStyles =
                ReadSourceLayerStyles(filePath);
            string? copiedSourceFile = CopySourceFileToProjectFolder(session, filePath);
            AppDbContext context = session.GetDbContext();
            await ProjectDatabaseCompatibility.EnsureAsync(context, ct);
            Color canvasBackgroundColor = await ResolveCanvasBackgroundColorAsync(context, ct);

            Dictionary<string, CanvasLayer> targetLayers =
                await CreateTargetLayersAsync(
                    context,
                    objects,
                    options,
                    sourceLayerStyles,
                    copiedSourceFile ?? filePath,
                    canvasBackgroundColor,
                    ct);

            DateTime now = DateTime.Now;
            List<CanvasObject> canvasObjects = new(objects.Count);
            foreach (ExternalRawObject item in objects)
            {
                CanvasLayer layer = targetLayers[GetTargetLayerKey(item, options)];
                CanvasObject canvasObject = new()
                {
                    CanvasLayerId = layer.Id,
                    CanvasLayer = layer,
                    ObjectType = item.ObjectType,
                    Shape = item.Geometry,
                    GeometryMetadataJson = JsonSerializer.Serialize(
                        new ExternalLayerObjectMetadata(
                            options.ImportKind,
                            Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant(),
                            Path.GetFileName(filePath),
                            copiedSourceFile,
                    item.SourceLayer,
                    item.SourceHandle,
                    item.CurveMetadataJson,
                    item.Attributes.Count == 0 ? null : item.Attributes),
                JsonOptions),
                    LabelText = item.LabelText,
                    ObjectDescription = BuildObjectDescription(item, layer, options),
                    SourceDxfHandle = item.SourceHandle,
                    IsVisible = true,
                    IsLocked = false,
                    CreatedDate = now,
                    LastModifiedDate = now
                };
                if (IsBlockTargetLayer(layer))
                {
                    CanvasGeometryMetricsService.StoreBlockDepthFromGeometry(canvasObject);
                }

                canvasObjects.Add(canvasObject);
            }

            HashSet<int> replacedTargetLayerIds = [];
            int replacedTargetLayerObjectCount = 0;
            if (options.ReplaceExistingTargetLayerObjects)
            {
                replacedTargetLayerIds = targetLayers.Values
                    .Select(layer => layer.Id)
                    .ToHashSet();
                if (replacedTargetLayerIds.Count > 0)
                {
                    List<CanvasObject> existingTargetObjects = await context.CanvasObjects
                        .Where(item => replacedTargetLayerIds.Contains(item.CanvasLayerId))
                        .ToListAsync(ct);
                    replacedTargetLayerObjectCount = existingTargetObjects.Count;
                    if (existingTargetObjects.Count > 0)
                        context.CanvasObjects.RemoveRange(existingTargetObjects);
                }
            }

            ImportProjectBoundaryResolution projectBoundaryResolution =
                await ResolveImportProjectBoundaryAsync(
                    context,
                    canvasObjects,
                    resolveProjectBoundaryConflict,
                    replacedTargetLayerIds,
                    ct);
            if (projectBoundaryResolution.Cancelled)
            {
                return new ExternalLayerImportResult(
                    false,
                    "Import cancelled because a Project Boundary already exists.",
                    0,
                    0,
                    copiedSourceFile,
                    null,
                    importWarnings);
            }

            canvasObjects = projectBoundaryResolution.ObjectsToAdd;
            if (projectBoundaryResolution.SkippedExtraIncomingCount > 0)
            {
                importWarnings.Add(
                    $"Skipped {projectBoundaryResolution.SkippedExtraIncomingCount} extra incoming Project Boundary object(s); a project can have only one Project Boundary.");
            }

            if (projectBoundaryResolution.SkippedForExistingBoundary)
            {
                importWarnings.Add(
                    "Skipped incoming Project Boundary because the existing Project Boundary was kept.");
            }

            if (projectBoundaryResolution.ReplacedExistingCount > 0)
            {
                importWarnings.Add(
                    $"Replaced {projectBoundaryResolution.ReplacedExistingCount} existing Project Boundary object(s).");
                context.CanvasObjects.RemoveRange(projectBoundaryResolution.ExistingObjectsToRemove);
            }

            // A target layer holds at most one object per geometry. Drop in-batch duplicates
            // silently, and for objects that duplicate geometry already present in the layer,
            // ask the caller whether to replace the existing object or skip the incoming one.
            ImportDuplicateResolution duplicateResolution = await ResolveImportDuplicateGeometriesAsync(
                context,
                canvasObjects,
                resolveDuplicateGeometries,
                replacedTargetLayerIds,
                ct);
            if (duplicateResolution.Cancelled)
            {
                return new ExternalLayerImportResult(
                    false,
                    "Import cancelled because of duplicate geometry.",
                    0,
                    0,
                    copiedSourceFile,
                    null,
                    importWarnings);
            }

            canvasObjects = duplicateResolution.ObjectsToAdd;
            if (duplicateResolution.SkippedDuplicateCount > 0)
            {
                importWarnings.Add(
                    $"Skipped {duplicateResolution.SkippedDuplicateCount} object(s) whose geometry already exists in the target layer.");
            }

            if (duplicateResolution.ReplacedExistingCount > 0)
            {
                importWarnings.Add(
                    $"Replaced {duplicateResolution.ReplacedExistingCount} existing object(s) that had the same geometry.");
                context.CanvasObjects.RemoveRange(duplicateResolution.ExistingObjectsToRemove);
            }

            if (replacedTargetLayerObjectCount > 0)
            {
                importWarnings.Add(
                    $"Replaced {replacedTargetLayerObjectCount} existing object(s) in the selected target layer(s).");
            }

            if (canvasObjects.Count == 0 &&
                projectBoundaryResolution.ExistingObjectsToRemove.Count == 0 &&
                duplicateResolution.ExistingObjectsToRemove.Count == 0 &&
                replacedTargetLayerObjectCount == 0)
            {
                return new ExternalLayerImportResult(
                    true,
                    null,
                    0,
                    0,
                    copiedSourceFile,
                    null,
                    importWarnings);
            }

            foreach (CanvasObject canvasObject in canvasObjects)
                NormalizeGeometryForCanvasDatabase(canvasObject.Shape);

            canvasObjects = FilterCanvasObjectsForCanvasDatabaseCompatibility(canvasObjects, importWarnings);

            await context.CanvasObjects.AddRangeAsync(canvasObjects, ct);
            foreach (CanvasLayer layer in targetLayers.Values)
            {
                layer.ImportedDate = now;
                layer.LastModifiedDate = now;
            }

            await context.SaveChangesAsync(ct);

            NtsEnvelope envelope = new();
            foreach (CanvasObject canvasObject in canvasObjects)
            {
                if (canvasObject.Shape != null)
                    envelope.ExpandToInclude(canvasObject.Shape.EnvelopeInternal);
            }

            return new ExternalLayerImportResult(
                true,
                null,
                targetLayers.Count,
                canvasObjects.Count,
                copiedSourceFile,
                envelope.IsNull ? null : envelope,
                importWarnings,
                canvasObjects.Select(canvasObject => canvasObject.Id).ToList());
        }

        private IReadOnlyList<ExternalRawObject> Read(
            string filePath,
            ExternalLayerImportOptions options)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension is ".kml" or ".kmz")
                return ReadOgr(filePath, options);

            if (extension == ".dxf")
                return ReadDxf(filePath, options);

            if (extension == ".dwg")
                return ReadDwg(filePath, options);

            CadastralImportOptions cadOptions = new(
                options.Layers
                    .Select(layer => new CadastralLayerImportOption(
                        layer.LayerName,
                        layer.Include,
                        layer.LayerName,
                        null))
                    .ToList(),
                options.SourceCrsCode,
                AutoAssignParcelRecords: false,
                ShpParcelNumberField: null,
                ShpMapSheetField: null,
                AttributeMapSheetValueMappings: new Dictionary<string, string>(),
                SkipDuplicateGeometries: false);

            return _cadReader.Read(filePath, cadOptions)
                .Select(item => new ExternalRawObject(
                    item.Geometry,
                    item.ObjectType,
                    item.SourceLayer,
                    item.ParcelNo,
                    item.SourceHandle,
                    item.Attributes,
                    null))
                .ToList();
        }

        private static IReadOnlyList<ExternalRawObject> ReadDxf(
            string filePath,
            ExternalLayerImportOptions options)
        {
            DxfDocument dxf = DxfDocument.Load(filePath);
            Dictionary<string, ExternalLayerImportOption> layerOptions = IncludedLayerOptions(options);

            List<ExternalRawObject> objects = [];
            foreach (DxfPolyline2D polyline in dxf.Entities.Polylines2D)
            {
                AddDxfPolyline(
                    objects,
                    polyline.Layer?.Name,
                    polyline.Handle,
                    CreatePolylineImportGeometry(ReadDxfPolylineSegments(polyline), polyline.IsClosed),
                    layerOptions);
            }

            foreach (DxfPolyline3D polyline in dxf.Entities.Polylines3D)
            {
                IReadOnlyList<CurveSegmentMetadata> segments = ReadLineSegments(
                    polyline.Vertexes.Select(vertex => new CurvePoint(vertex.X, vertex.Y)).ToList(),
                    polyline.IsClosed || IsRingClosed(polyline.Vertexes.Select(vertex => new CurvePoint(vertex.X, vertex.Y)).ToList()));
                AddDxfPolyline(
                    objects,
                    polyline.Layer?.Name,
                    polyline.Handle,
                    CreatePolylineImportGeometry(segments, polyline.IsClosed),
                    layerOptions);
            }

            foreach (DxfArc arc in dxf.Entities.Arcs)
            {
                AddDxfObject(
                    objects,
                    arc.Layer?.Name,
                    arc.Handle,
                    CreateArcGeometry(DegreesToRadians(arc.StartAngle), DegreesToRadians(SweepDegrees(arc.StartAngle, arc.EndAngle)), arc.Radius, arc.Center.X, arc.Center.Y),
                    "Arc",
                    null,
                    CreateCurveMetadata("Arc", arc.Center.X, arc.Center.Y, null, null, arc.Radius, DegreesToRadians(arc.StartAngle), DegreesToRadians(SweepDegrees(arc.StartAngle, arc.EndAngle))),
                    layerOptions);
            }

            foreach (DxfCircle circle in dxf.Entities.Circles)
            {
                AddDxfObject(
                    objects,
                    circle.Layer?.Name,
                    circle.Handle,
                    CreateCircleGeometry(circle.Center.X, circle.Center.Y, circle.Radius),
                    "Circle",
                    null,
                    CreateCurveMetadata("Circle", circle.Center.X, circle.Center.Y, circle.Center.X + circle.Radius, circle.Center.Y, circle.Radius, null, null),
                    layerOptions);
            }

            foreach (DxfLine line in dxf.Entities.Lines)
            {
                AddDxfObject(
                    objects,
                    line.Layer?.Name,
                    line.Handle,
                    CanvasGeometryFactory.CreateLineString(
                        [
                            new Coordinate(line.StartPoint.X, line.StartPoint.Y),
                            new Coordinate(line.EndPoint.X, line.EndPoint.Y)
                        ]),
                    "Line",
                    null,
                    null,
                    layerOptions);
            }

            foreach (DxfPoint point in dxf.Entities.Points)
            {
                AddDxfObject(
                    objects,
                    point.Layer?.Name,
                    point.Handle,
                    CanvasGeometryFactory.CreatePoint(new Coordinate(point.Position.X, point.Position.Y)),
                    "Point",
                    null,
                    null,
                    layerOptions);
            }

            foreach (DxfText text in dxf.Entities.Texts)
            {
                AddDxfObject(
                    objects,
                    text.Layer?.Name,
                    text.Handle,
                    CanvasGeometryFactory.CreatePoint(new Coordinate(text.Position.X, text.Position.Y)),
                    "Text",
                    NormalizeText(text.Value),
                    null,
                    layerOptions);
            }

            foreach (DxfMText text in dxf.Entities.MTexts)
            {
                AddDxfObject(
                    objects,
                    text.Layer?.Name,
                    text.Handle,
                    CanvasGeometryFactory.CreatePoint(new Coordinate(text.Position.X, text.Position.Y)),
                    "Text",
                    NormalizeText(text.Value),
                    null,
                    layerOptions);
            }

            return objects;
        }

        private static IReadOnlyList<ExternalRawObject> ReadDwg(
            string filePath,
            ExternalLayerImportOptions options)
        {
            CadDocument document = ACadSharp.IO.DwgReader.Read(filePath);
            Dictionary<string, ExternalLayerImportOption> layerOptions = IncludedLayerOptions(options);

            List<ExternalRawObject> objects = [];
            foreach (AcadEntity entity in EnumerateCadEntities(document.Entities))
            {
                string? layerName = entity.Layer?.Name;
                string handle = entity.Handle.ToString("X");
                switch (entity)
                {
                    case AcadLwPolyline polyline:
                        AddDxfPolyline(
                            objects,
                            layerName,
                            handle,
                            CreatePolylineImportGeometry(ReadAcadPolylineSegments(polyline), polyline.IsClosed),
                            layerOptions);
                        break;
                    case AcadPolyline2D polyline:
                        AddDxfPolyline(
                            objects,
                            layerName,
                            handle,
                            CreatePolylineImportGeometry(ReadAcadPolylineSegments(polyline), polyline.IsClosed),
                            layerOptions);
                        break;
                    case AcadPolyline3D polyline:
                        IReadOnlyList<CurvePoint> vertices = polyline.Vertices
                            .Select(vertex => new CurvePoint(vertex.Location.X, vertex.Location.Y))
                            .ToList();
                        AddDxfPolyline(
                            objects,
                            layerName,
                            handle,
                            CreatePolylineImportGeometry(ReadLineSegments(vertices, polyline.IsClosed || IsRingClosed(vertices)), polyline.IsClosed || IsRingClosed(vertices)),
                            layerOptions);
                        break;
                    case AcadArc arc:
                        double arcSweep = NormalizePositiveRadians(arc.EndAngle - arc.StartAngle);
                        AddDxfObject(
                            objects,
                            layerName,
                            handle,
                            CreateArcGeometry(arc.StartAngle, arcSweep, arc.Radius, arc.Center.X, arc.Center.Y),
                            "Arc",
                            null,
                            CreateCurveMetadata("Arc", arc.Center.X, arc.Center.Y, null, null, arc.Radius, arc.StartAngle, arcSweep),
                            layerOptions);
                        break;
                    case AcadCircle circle:
                        AddDxfObject(
                            objects,
                            layerName,
                            handle,
                            CreateCircleGeometry(circle.Center.X, circle.Center.Y, circle.Radius),
                            "Circle",
                            null,
                            CreateCurveMetadata("Circle", circle.Center.X, circle.Center.Y, circle.Center.X + circle.Radius, circle.Center.Y, circle.Radius, null, null),
                            layerOptions);
                        break;
                    case AcadLine line:
                        AddDxfObject(
                            objects,
                            layerName,
                            handle,
                            CanvasGeometryFactory.CreateLineString(
                                [
                                    new Coordinate(line.StartPoint.X, line.StartPoint.Y),
                                    new Coordinate(line.EndPoint.X, line.EndPoint.Y)
                                ]),
                            "Line",
                            null,
                            null,
                            layerOptions);
                        break;
                    case AcadPoint point:
                        AddDxfObject(
                            objects,
                            layerName,
                            handle,
                            CanvasGeometryFactory.CreatePoint(new Coordinate(point.Location.X, point.Location.Y)),
                            "Point",
                            null,
                            null,
                            layerOptions);
                        break;
                    case AcadText text:
                        AddDxfObject(
                            objects,
                            layerName,
                            handle,
                            CanvasGeometryFactory.CreatePoint(new Coordinate(text.InsertPoint.X, text.InsertPoint.Y)),
                            "Text",
                            NormalizeText(text.Value),
                            null,
                            layerOptions);
                        break;
                    case AcadMText text:
                        AddDxfObject(
                            objects,
                            layerName,
                            handle,
                            CanvasGeometryFactory.CreatePoint(new Coordinate(text.InsertPoint.X, text.InsertPoint.Y)),
                            "Text",
                            NormalizeText(text.PlainText ?? text.Value),
                            null,
                            layerOptions);
                        break;
                }
            }

            return objects;
        }

        private static Dictionary<string, ExternalLayerStyle> ReadSourceLayerStyles(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            try
            {
                return extension switch
                {
                    ".dxf" => ReadDxfLayerStyles(filePath),
                    ".dwg" => ReadDwgLayerStyles(filePath),
                    _ => new Dictionary<string, ExternalLayerStyle>(StringComparer.OrdinalIgnoreCase)
                };
            }
            catch
            {
                return new Dictionary<string, ExternalLayerStyle>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Dictionary<string, ExternalLayerStyle> ReadDxfLayerStyles(string filePath)
        {
            DxfDocument dxf = DxfDocument.Load(filePath);
            Dictionary<string, ExternalLayerStyle> styles = new(StringComparer.OrdinalIgnoreCase);
            foreach (netDxf.Tables.Layer layer in dxf.Layers)
            {
                if (string.IsNullOrWhiteSpace(layer.Name))
                    continue;

                styles[layer.Name] = new ExternalLayerStyle(ToDrawingColor(layer.Color));
            }

            return styles;
        }

        private static Dictionary<string, ExternalLayerStyle> ReadDwgLayerStyles(string filePath)
        {
            ACadSharp.CadDocument document = ACadSharp.IO.DwgReader.Read(filePath);
            Dictionary<string, ExternalLayerStyle> styles = new(StringComparer.OrdinalIgnoreCase);
            foreach (ACadSharp.Tables.Layer layer in document.Layers)
            {
                if (string.IsNullOrWhiteSpace(layer.Name))
                    continue;

                styles[layer.Name] = new ExternalLayerStyle(ToDrawingColor(layer.Color));
            }

            return styles;
        }

        private static Color ToDrawingColor(netDxf.AciColor? color)
        {
            if (color == null || color.IsByBlock || color.IsByLayer)
                return Color.Black;

            return Color.FromArgb(255, color.R, color.G, color.B);
        }

        private static Color ToDrawingColor(ACadSharp.Color color)
        {
            if (color.IsByBlock || color.IsByLayer)
                return Color.Black;

            return Color.FromArgb(255, color.R, color.G, color.B);
        }

        private static ExternalLayerFileInfo InspectOgr(string filePath, string format)
        {
            GdalBootstrapper.ConfigureAll();
            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            List<ExternalLayerInfo> layers = [];
            string? detectedCrs = null;
            for (int index = 0; index < dataSource.GetLayerCount(); index++)
            {
                using Layer layer = dataSource.GetLayerByIndex(index);
                LayerStats stats = CountOgrObjects(layer);
                if (stats.ObjectCount > 0)
                {
                    layers.Add(new ExternalLayerInfo(
                        layer.GetName(),
                        stats.ObjectCount,
                        stats.ObjectTypes));
                }

                detectedCrs ??= GetLayerCrsDefinition(layer);
            }

            detectedCrs ??= "EPSG:4326";
            return new ExternalLayerFileInfo(
                filePath,
                format,
                layers.OrderBy(layer => layer.Name).ToList(),
                detectedCrs,
                RequiresCrsFromUser: false);
        }

        private static IReadOnlyList<ExternalRawObject> ReadOgr(
            string filePath,
            ExternalLayerImportOptions options)
        {
            GdalBootstrapper.ConfigureAll();
            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            Dictionary<string, ExternalLayerImportOption> layerOptions = options.Layers
                .Where(option => option.Include)
                .ToDictionary(option => option.LayerName, StringComparer.OrdinalIgnoreCase);

            WKTReader reader = new();
            List<ExternalRawObject> objects = [];
            for (int index = 0; index < dataSource.GetLayerCount(); index++)
            {
                using Layer layer = dataSource.GetLayerByIndex(index);
                string layerName = layer.GetName();
                if (!layerOptions.ContainsKey(layerName))
                    continue;

                layer.ResetReading();
                Feature? feature;
                while ((feature = layer.GetNextFeature()) != null)
                {
                    using (feature)
                    {
                        OSGeo.OGR.Geometry ogrGeometry = feature.GetGeometryRef();
                        if (ogrGeometry == null)
                            continue;

                        ogrGeometry.ExportToWkt(out string wkt);
                        NtsGeometry geometry = reader.Read(wkt);
                        Dictionary<string, string?> attributes = ReadAttributes(feature);
                        string? label = ResolveLabel(attributes);

                        foreach (NtsGeometry part in FlattenImportableGeometries(geometry))
                        {
                            string objectType = ResolveObjectType(part);
                            objects.Add(new ExternalRawObject(
                                part,
                                objectType,
                                layerName,
                                objectType.Equals("Text", StringComparison.OrdinalIgnoreCase) ? label : null,
                                feature.GetFID().ToString(),
                                attributes,
                                null));
                        }
                    }
                }
            }

            return objects;
        }

        private static Dictionary<string, ExternalLayerImportOption> IncludedLayerOptions(
            ExternalLayerImportOptions options)
        {
            return options.Layers
                .Where(option => option.Include)
                .ToDictionary(option => option.LayerName, StringComparer.OrdinalIgnoreCase);
        }

        private static void AddDxfPolyline(
            List<ExternalRawObject> objects,
            string? layerName,
            string? sourceHandle,
            PolylineImportGeometry importGeometry,
            IReadOnlyDictionary<string, ExternalLayerImportOption> layerOptions)
        {
            string normalizedLayer = string.IsNullOrWhiteSpace(layerName) ? "0" : layerName;
            if (!layerOptions.ContainsKey(normalizedLayer) || importGeometry.Geometry == null)
                return;

            objects.Add(new ExternalRawObject(
                importGeometry.Geometry,
                importGeometry.IsClosed ? "Polygon" : "Polyline",
                normalizedLayer,
                null,
                sourceHandle,
                new Dictionary<string, string?>(),
                importGeometry.CurveMetadataJson));
        }

        private static void AddDxfObject(
            List<ExternalRawObject> objects,
            string? layerName,
            string? sourceHandle,
            NtsGeometry geometry,
            string objectType,
            string? labelText,
            string? curveMetadataJson,
            IReadOnlyDictionary<string, ExternalLayerImportOption> layerOptions)
        {
            string normalizedLayer = string.IsNullOrWhiteSpace(layerName) ? "0" : layerName;
            if (!layerOptions.ContainsKey(normalizedLayer))
                return;

            objects.Add(new ExternalRawObject(
                geometry,
                objectType,
                normalizedLayer,
                labelText,
                sourceHandle,
                new Dictionary<string, string?>(),
                curveMetadataJson));
        }

        private static IReadOnlyList<CurveSegmentMetadata> ReadDxfPolylineSegments(DxfPolyline2D polyline)
        {
            IReadOnlyList<DxfPolyline2DVertex> vertices = polyline.Vertexes;
            if (vertices.Count < 2)
                return [];

            bool isClosed = polyline.IsClosed || IsRingClosed(vertices.Select(vertex => new CurvePoint(vertex.Position.X, vertex.Position.Y)).ToList());
            List<CurveSegmentMetadata> segments = [];
            int segmentCount = isClosed ? vertices.Count : vertices.Count - 1;
            for (int index = 0; index < segmentCount; index++)
            {
                DxfPolyline2DVertex startVertex = vertices[index];
                DxfPolyline2DVertex endVertex = vertices[(index + 1) % vertices.Count];
                CurvePoint start = new(startVertex.Position.X, startVertex.Position.Y);
                CurvePoint end = new(endVertex.Position.X, endVertex.Position.Y);
                CurveSegmentMetadata segment = CreateSegmentFromBulge(start, end, startVertex.Bulge);
                if (segment.Length > 0.000001)
                    segments.Add(segment);
            }

            return segments;
        }

        private static IReadOnlyList<CurveSegmentMetadata> ReadAcadPolylineSegments(AcadLwPolyline polyline)
        {
            IReadOnlyList<AcadLwPolyline.Vertex> vertices = polyline.Vertices;
            if (vertices.Count < 2)
                return [];

            bool isClosed = polyline.IsClosed || IsRingClosed(vertices.Select(vertex => new CurvePoint(vertex.Location.X, vertex.Location.Y)).ToList());
            List<CurveSegmentMetadata> segments = [];
            int segmentCount = isClosed ? vertices.Count : vertices.Count - 1;
            for (int index = 0; index < segmentCount; index++)
            {
                AcadLwPolyline.Vertex startVertex = vertices[index];
                AcadLwPolyline.Vertex endVertex = vertices[(index + 1) % vertices.Count];
                CurvePoint start = new(startVertex.Location.X, startVertex.Location.Y);
                CurvePoint end = new(endVertex.Location.X, endVertex.Location.Y);
                CurveSegmentMetadata segment = CreateSegmentFromBulge(start, end, startVertex.Bulge);
                if (segment.Length > 0.000001)
                    segments.Add(segment);
            }

            return segments;
        }

        private static IReadOnlyList<CurveSegmentMetadata> ReadAcadPolylineSegments(AcadPolyline2D polyline)
        {
            IReadOnlyList<ACadSharp.Entities.Vertex2D> vertices = polyline.Vertices.ToList();
            if (vertices.Count < 2)
                return [];

            bool isClosed = polyline.IsClosed || IsRingClosed(vertices.Select(vertex => new CurvePoint(vertex.Location.X, vertex.Location.Y)).ToList());
            List<CurveSegmentMetadata> segments = [];
            int segmentCount = isClosed ? vertices.Count : vertices.Count - 1;
            for (int index = 0; index < segmentCount; index++)
            {
                ACadSharp.Entities.Vertex2D startVertex = vertices[index];
                ACadSharp.Entities.Vertex2D endVertex = vertices[(index + 1) % vertices.Count];
                CurvePoint start = new(startVertex.Location.X, startVertex.Location.Y);
                CurvePoint end = new(endVertex.Location.X, endVertex.Location.Y);
                CurveSegmentMetadata segment = CreateSegmentFromBulge(start, end, startVertex.Bulge);
                if (segment.Length > 0.000001)
                    segments.Add(segment);
            }

            return segments;
        }

        private static IReadOnlyList<CurveSegmentMetadata> ReadLineSegments(
            IReadOnlyList<CurvePoint> vertices,
            bool isClosed)
        {
            if (vertices.Count < 2)
                return [];

            List<CurveSegmentMetadata> segments = [];
            int segmentCount = isClosed ? vertices.Count : vertices.Count - 1;
            for (int index = 0; index < segmentCount; index++)
            {
                CurvePoint start = vertices[index];
                CurvePoint end = vertices[(index + 1) % vertices.Count];
                if (Distance(start, end) <= 0.000001)
                    continue;

                segments.Add(CurveSegmentMetadata.Line(start.X, start.Y, end.X, end.Y));
            }

            return segments;
        }

        private static CurveSegmentMetadata CreateSegmentFromBulge(
            CurvePoint start,
            CurvePoint end,
            double bulge)
        {
            if (Math.Abs(bulge) <= 0.000000001)
                return CurveSegmentMetadata.Line(start.X, start.Y, end.X, end.Y);

            ArcCurve? arc = CreateArcFromBulge(start, end, bulge);
            return arc == null
                ? CurveSegmentMetadata.Line(start.X, start.Y, end.X, end.Y)
                : CurveSegmentMetadata.Arc(
                    start.X,
                    start.Y,
                    end.X,
                    end.Y,
                    arc.CenterX,
                    arc.CenterY,
                    arc.Radius,
                    arc.StartAngleRadians,
                    arc.SweepAngleRadians);
        }

        private static ArcCurve? CreateArcFromBulge(CurvePoint start, CurvePoint end, double bulge)
        {
            double chord = Distance(start, end);
            if (chord <= 0.000001)
                return null;

            double theta = 4.0 * Math.Atan(bulge);
            double sinHalfTheta = Math.Sin(Math.Abs(theta) / 2.0);
            if (Math.Abs(sinHalfTheta) <= 0.000000001)
                return null;

            double radius = chord / (2.0 * sinHalfTheta);
            double midpointX = (start.X + end.X) / 2.0;
            double midpointY = (start.Y + end.Y) / 2.0;
            double unitX = (end.X - start.X) / chord;
            double unitY = (end.Y - start.Y) / chord;
            double leftNormalX = -unitY;
            double leftNormalY = unitX;
            double centerOffset = chord * (1.0 - bulge * bulge) / (4.0 * bulge);
            double centerX = midpointX + leftNormalX * centerOffset;
            double centerY = midpointY + leftNormalY * centerOffset;
            double startAngle = Math.Atan2(start.Y - centerY, start.X - centerX);
            double endAngle = Math.Atan2(end.Y - centerY, end.X - centerX);
            double sweep = bulge >= 0.0
                ? NormalizePositiveRadians(endAngle - startAngle)
                : -NormalizePositiveRadians(startAngle - endAngle);

            if (Math.Abs(sweep) <= 0.000000001)
                return null;

            return new ArcCurve(centerX, centerY, radius, startAngle, sweep);
        }

        private static PolylineImportGeometry CreatePolylineImportGeometry(
            IReadOnlyList<CurveSegmentMetadata> segments,
            bool isClosed)
        {
            if (segments.Count == 0)
                return new PolylineImportGeometry(null, isClosed, null);

            List<Coordinate> coordinates = [];
            foreach (CurveSegmentMetadata segment in segments)
            {
                if (coordinates.Count == 0)
                    coordinates.Add(new Coordinate(segment.StartX, segment.StartY));

                if (segment.Kind.Equals("Arc", StringComparison.OrdinalIgnoreCase) &&
                    segment.CenterX.HasValue &&
                    segment.CenterY.HasValue &&
                    segment.Radius.HasValue &&
                    segment.StartAngleRadians.HasValue &&
                    segment.SweepAngleRadians.HasValue)
                {
                    foreach (Coordinate coordinate in SampleArc(
                                 segment.CenterX.Value,
                                 segment.CenterY.Value,
                                 segment.Radius.Value,
                                 segment.StartAngleRadians.Value,
                                 segment.SweepAngleRadians.Value,
                                 32).Skip(1))
                    {
                        coordinates.Add(coordinate);
                    }
                }
                else
                {
                    coordinates.Add(new Coordinate(segment.EndX, segment.EndY));
                }
            }

            RemoveConsecutiveDuplicates(coordinates);

            // A polyline is treated as a closed area when it carries the closed flag OR its first
            // and last points coincide (within tolerance). This mirrors the file inspector so the
            // "Objects/Object types" counts shown in the mapping dialog agree with what is imported:
            // a closed ring becomes a Polygon, an open polyline stays a line.
            bool effectiveClosed = isClosed ||
                (coordinates.Count >= 4 &&
                 coordinates[0].Distance(coordinates[^1]) <= ClosedRingTolerance);

            NtsGeometry? geometry = effectiveClosed && coordinates.Count >= 3
                ? CreatePolygon(coordinates)
                : coordinates.Count >= 2
                    ? CanvasGeometryFactory.CreateLineString(coordinates.ToArray())
                    : null;

            // If a "closed" ring degenerates and cannot form a valid polygon, fall back to a line
            // so the object is not silently dropped at this stage.
            if (effectiveClosed && geometry == null && coordinates.Count >= 2)
            {
                geometry = CanvasGeometryFactory.CreateLineString(coordinates.ToArray());
                effectiveClosed = false;
            }

            string? metadataJson = segments.Any(segment => segment.Kind.Equals("Arc", StringComparison.OrdinalIgnoreCase))
                ? JsonSerializer.Serialize(new PolylineCurveMetadata(
                    effectiveClosed ? "Polygon" : "Polyline",
                    effectiveClosed,
                    segments.ToList()),
                    JsonOptions)
                : null;

            return new PolylineImportGeometry(geometry, effectiveClosed, metadataJson);
        }

        private static NtsGeometry CreateArcGeometry(
            double startAngleRadians,
            double sweepAngleRadians,
            double radius,
            double centerX,
            double centerY)
        {
            return CanvasGeometryFactory.CreateLineString(
                SampleArc(centerX, centerY, radius, startAngleRadians, sweepAngleRadians, 64).ToArray());
        }

        private static NtsGeometry CreateCircleGeometry(double centerX, double centerY, double radius)
        {
            List<Coordinate> ring = [];
            const int samples = 96;
            for (int index = 0; index < samples; index++)
            {
                double angle = Math.PI * 2.0 * index / samples;
                ring.Add(new Coordinate(
                    centerX + radius * Math.Cos(angle),
                    centerY + radius * Math.Sin(angle)));
            }

            ring.Add(new Coordinate(ring[0].X, ring[0].Y));
            return CanvasGeometryFactory.CreatePolygon(ring.ToArray());
        }

        private static IEnumerable<Coordinate> SampleArc(
            double centerX,
            double centerY,
            double radius,
            double startAngleRadians,
            double sweepAngleRadians,
            int sampleCount)
        {
            int count = Math.Max(2, sampleCount);
            for (int index = 0; index < count; index++)
            {
                double fraction = (double)index / (count - 1);
                double angle = startAngleRadians + sweepAngleRadians * fraction;
                yield return new Coordinate(
                    centerX + radius * Math.Cos(angle),
                    centerY + radius * Math.Sin(angle));
            }
        }

        private static NtsGeometry? CreatePolygon(List<Coordinate> coordinates)
        {
            Coordinate[] ringCoordinates = BoundaryGeometryReaderHelpers.CloseRing(coordinates);
            if (ringCoordinates.Length < 4)
                return null;

            LinearRing ring = CanvasGeometryFactory.CreateLinearRing(ringCoordinates);
            Polygon polygon = CanvasGeometryFactory.CreatePolygon(ring);
            return BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(polygon);
        }

        private static string CreateCurveMetadata(
            string shapeType,
            double centerX,
            double centerY,
            double? radiusPointX,
            double? radiusPointY,
            double? radius,
            double? startAngleRadians,
            double? sweepAngleRadians)
        {
            return JsonSerializer.Serialize(new CurveMetadata(
                shapeType,
                centerX,
                centerY,
                radiusPointX,
                radiusPointY,
                radius,
                startAngleRadians,
                sweepAngleRadians),
                JsonOptions);
        }

        private static IEnumerable<AcadEntity> EnumerateCadEntities(IEnumerable<AcadEntity> entities)
        {
            foreach (AcadEntity entity in entities)
            {
                if (entity.IsInvisible)
                    continue;

                if (entity is AcadInsert insert)
                {
                    foreach (AcadEntity exploded in EnumerateCadEntities(insert.Explode().OfType<AcadEntity>()))
                        yield return exploded;
                    continue;
                }

                yield return entity;
            }
        }

        private static void RemoveConsecutiveDuplicates(List<Coordinate> coordinates)
        {
            for (int index = coordinates.Count - 1; index > 0; index--)
            {
                if (coordinates[index].Distance(coordinates[index - 1]) <= 0.000001)
                    coordinates.RemoveAt(index);
            }
        }

        private static bool IsRingClosed(IReadOnlyList<CurvePoint> vertices)
        {
            return vertices.Count >= 3 &&
                   Distance(vertices[0], vertices[^1]) <= 0.000001;
        }

        private static double Distance(CurvePoint first, CurvePoint second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double DegreesToRadians(double degrees) =>
            degrees * Math.PI / 180.0;

        private static double SweepDegrees(double startAngleDegrees, double endAngleDegrees)
        {
            double sweep = endAngleDegrees - startAngleDegrees;
            while (sweep < 0.0)
                sweep += 360.0;
            while (sweep > 360.0)
                sweep -= 360.0;
            return sweep;
        }

        private static double NormalizePositiveRadians(double angle)
        {
            double full = Math.PI * 2.0;
            angle %= full;
            return angle < 0.0 ? angle + full : angle;
        }

        private static string? NormalizeText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value
                .Replace("\\P", " ")
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
        }

        private static IEnumerable<NtsGeometry> FlattenImportableGeometries(NtsGeometry geometry)
        {
            if (geometry.IsEmpty)
                yield break;

            if (geometry is GeometryCollection collection)
            {
                for (int index = 0; index < collection.NumGeometries; index++)
                {
                    foreach (NtsGeometry child in FlattenImportableGeometries(collection.GetGeometryN(index)))
                        yield return child;
                }

                yield break;
            }

            if (geometry is Polygon)
            {
                NtsGeometry? valid = BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(geometry);
                if (valid != null)
                    yield return valid;
                yield break;
            }

            if (geometry is LineString line && line.NumPoints >= 2)
            {
                yield return line;
                yield break;
            }

            if (geometry is NtsPoint point)
                yield return point;
        }

        private static LayerStats CountOgrObjects(Layer layer)
        {
            WKTReader reader = new();
            LayerStats stats = new();
            layer.ResetReading();
            Feature? feature;
            while ((feature = layer.GetNextFeature()) != null)
            {
                using (feature)
                {
                    OSGeo.OGR.Geometry ogrGeometry = feature.GetGeometryRef();
                    if (ogrGeometry == null)
                        continue;

                    ogrGeometry.ExportToWkt(out string wkt);
                    foreach (NtsGeometry part in FlattenImportableGeometries(reader.Read(wkt)))
                        stats.Add(ResolveObjectType(part));
                }
            }

            layer.ResetReading();
            return stats;
        }

        private static Dictionary<string, string?> ReadAttributes(Feature feature)
        {
            Dictionary<string, string?> attributes = new(StringComparer.OrdinalIgnoreCase);
            FeatureDefn definition = feature.GetDefnRef();
            for (int index = 0; index < definition.GetFieldCount(); index++)
            {
                string name = definition.GetFieldDefn(index).GetName();
                attributes[name] = feature.IsFieldSet(index)
                    ? feature.GetFieldAsString(index)
                    : null;
            }

            return attributes;
        }

        private static string? ResolveLabel(IReadOnlyDictionary<string, string?> attributes)
        {
            foreach (string key in new[] { "Name", "name", "Label", "label", "Description", "description" })
            {
                if (attributes.TryGetValue(key, out string? value) &&
                    !string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private static async Task<Color> ResolveCanvasBackgroundColorAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            string? canvasBackgroundColor = await context.ProjectSettings
                .AsNoTracking()
                .Select(settings => settings.CanvasBackgroundColor)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(canvasBackgroundColor))
                return Color.White;

            try
            {
                return ColorTranslator.FromHtml(canvasBackgroundColor);
            }
            catch
            {
                return Color.White;
            }
        }

        private static async Task<Dictionary<string, CanvasLayer>> CreateTargetLayersAsync(
            AppDbContext context,
            IReadOnlyList<ExternalRawObject> objects,
            ExternalLayerImportOptions options,
            IReadOnlyDictionary<string, ExternalLayerStyle> sourceLayerStyles,
            string sourceFile,
            Color canvasBackgroundColor,
            CancellationToken ct)
        {
            IReadOnlyList<TargetLayerSpec> specs = objects
                .Select(item => GetTargetLayerSpec(item, objects, options))
                .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderByDescending(item => item.IsApplicationLayer)
                .ThenBy(item => item.SourceLayer)
                .ThenBy(item => item.LayerType)
                .ToList();

            int nextDisplayOrder =
                (await context.CanvasLayers
                    .Select(layer => (int?)layer.DisplayOrder)
                    .MaxAsync(ct) ?? -1) + 1;
            Dictionary<string, CanvasLayer> result = new(StringComparer.OrdinalIgnoreCase);
            int colorIndex = 0;

            foreach (TargetLayerSpec spec in specs)
            {
                CanvasLayer layer;
                if (spec.IsApplicationLayer)
                {
                    layer = await GetOrCreateApplicationTargetLayerAsync(
                        context,
                        spec.Name,
                        nextDisplayOrder,
                        ct);

                    if (context.Entry(layer).State == EntityState.Added)
                        nextDisplayOrder++;
                }
                else
                {
                    layer = CreateExternalLayer(
                        spec.Name,
                        spec.LayerType,
                        sourceFile,
                        nextDisplayOrder++,
                        sourceLayerStyles.TryGetValue(spec.SourceLayer, out ExternalLayerStyle? style)
                            ? style.Color
                            : PickLayerColor(colorIndex++),
                        canvasBackgroundColor);

                    await context.CanvasLayers.AddAsync(layer, ct);
                }

                result[spec.Key] = layer;
            }

            await context.SaveChangesAsync(ct);
            return result;
        }

        private static async Task<CanvasLayer> GetOrCreateApplicationTargetLayerAsync(
            AppDbContext context,
            string layerName,
            int displayOrder,
            CancellationToken ct)
        {
            CanvasLayer? layer = await context.CanvasLayers
                .FirstOrDefaultAsync(
                    item => item.Name == layerName,
                    ct);

            BlockLayoutPlanTargetLayerDefinition definition =
                BlockLayoutPlanImportTargets.Find(layerName)
                ?? new BlockLayoutPlanTargetLayerDefinition(
                    layerName,
                    CanvasLayerTreeService.PolygonLayerType,
                    "#6B7280",
                    null,
                    50,
                    1.2,
                    "Solid");

            if (layer != null)
            {
                ApplyMissingApplicationTargetLayerStyle(layer, definition);
                return layer;
            }

            DateTime now = DateTime.Now;
            layer = new CanvasLayer
            {
                Name = definition.Name,
                LayerType = definition.LayerType,
                IsVisible = true,
                IsLocked = true,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = displayOrder,
                BorderColor = definition.BorderColor,
                LineWeight = definition.LineWeight,
                LineStyle = definition.LineStyle,
                LineTypeScale = 1.0,
                FillColor = definition.FillColor,
                ShowFillTransparency = false,
                FillTransparency = definition.FillTransparency,
                FillStyle = string.IsNullOrWhiteSpace(definition.FillColor) ? "None" : "Solid",
                LabelColor = "#000000",
                LabelFontName = "Nirmala UI",
                LabelFontSize = 1.0,
                LabelScaleWithZoom = true,
                TextAlignment = "Center Middle",
                PointSymbol = "Dot",
                PointSize = 5.0,
                CreatedDate = now,
                LastModifiedDate = now,
                Description = $"Default layer: {definition.Name}"
            };

            await context.CanvasLayers.AddAsync(layer, ct);
            return layer;
        }

        private static void ApplyMissingApplicationTargetLayerStyle(
            CanvasLayer layer,
            BlockLayoutPlanTargetLayerDefinition definition)
        {
            bool changed = false;

            if (!string.Equals(layer.LayerType, definition.LayerType, StringComparison.OrdinalIgnoreCase))
            {
                layer.LayerType = definition.LayerType;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(layer.BorderColor))
            {
                layer.BorderColor = definition.BorderColor;
                changed = true;
            }

            if (layer.LineWeight <= 0.0)
            {
                layer.LineWeight = definition.LineWeight;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(layer.LineStyle))
            {
                layer.LineStyle = definition.LineStyle;
                changed = true;
            }

            if (layer.LineTypeScale <= 0.0)
            {
                layer.LineTypeScale = 1.0;
                changed = true;
            }

            if (!string.IsNullOrWhiteSpace(definition.FillColor))
            {
                bool missingFillStyle = string.IsNullOrWhiteSpace(layer.FillStyle);
                bool missingFillColor = string.IsNullOrWhiteSpace(layer.FillColor);
                if (missingFillStyle || (missingFillColor && string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase)))
                {
                    layer.FillStyle = "Solid";
                    changed = true;
                }

                if (missingFillColor &&
                    !string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase))
                {
                    layer.FillColor = definition.FillColor;
                    changed = true;
                }
            }
            else if (string.IsNullOrWhiteSpace(layer.FillStyle))
            {
                layer.FillStyle = "None";
                changed = true;
            }

            if (layer.FillTransparency < 0 || layer.FillTransparency > 100)
            {
                layer.FillTransparency = definition.FillTransparency;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(layer.LabelFontName))
            {
                layer.LabelFontName = "Nirmala UI";
                changed = true;
            }

            if (layer.LabelFontSize <= 0.0)
            {
                layer.LabelFontSize = 1.0;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(layer.TextAlignment))
            {
                layer.TextAlignment = "Center Middle";
                changed = true;
            }

            if (changed)
            {
                layer.LastModifiedDate = DateTime.Now;
            }
        }

        private static CanvasLayer CreateExternalLayer(
            string name,
            string layerType,
            string sourceFile,
            int displayOrder,
            Color color,
            Color canvasBackgroundColor)
        {
            DateTime now = DateTime.Now;
            bool polygon = string.Equals(layerType, CanvasLayerTreeService.PolygonLayerType, StringComparison.OrdinalIgnoreCase);
            bool point = string.Equals(layerType, CanvasLayerTreeService.PointLayerType, StringComparison.OrdinalIgnoreCase);
            bool annotation = string.Equals(layerType, CanvasLayerTreeService.AnnotationLayerType, StringComparison.OrdinalIgnoreCase);
            string themeColor = ToHtml(CanvasThemeColorService.AdjustColorForCanvasTheme(
                canvasBackgroundColor,
                color));

            return new CanvasLayer
            {
                Name = name,
                LayerType = layerType,
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = displayOrder,
                BorderColor = themeColor,
                LineWeight = annotation ? 0 : 1.2,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                FillColor = polygon ? themeColor : null,
                ShowFillTransparency = false,
                FillTransparency = 55,
                FillStyle = "None",
                LabelColor = themeColor,
                LabelFontName = "Nirmala UI",
                LabelFontSize = annotation ? 10.0 : 1.0,
                TextAlignment = "Center Middle",
                PointSymbol = point ? "Circle" : "Dot",
                PointSize = point ? 5.0 : 1.0,
                SourceFile = sourceFile,
                ImportedDate = now,
                CreatedDate = now,
                LastModifiedDate = now,
                Description = "Imported external layer"
            };
        }

        private static TargetLayerSpec GetTargetLayerSpec(
            ExternalRawObject item,
            IReadOnlyList<ExternalRawObject> allObjects,
            ExternalLayerImportOptions options)
        {
            string? mappedTargetLayer = GetMappedTargetLayerName(options, item.SourceLayer);
            if (!string.IsNullOrWhiteSpace(mappedTargetLayer))
            {
                BlockLayoutPlanTargetLayerDefinition? definition =
                    BlockLayoutPlanImportTargets.Find(mappedTargetLayer);
                string mappedLayerType = definition?.LayerType ?? ResolveCanvasLayerType(item.ObjectType);
                return new TargetLayerSpec(
                    $"app|{mappedTargetLayer}",
                    mappedTargetLayer,
                    item.SourceLayer,
                    mappedLayerType,
                    IsApplicationLayer: true);
            }

            string sourceLayer = SanitizeLayerName(item.SourceLayer) ?? "0";
            string layerType = ResolveCanvasLayerType(item.ObjectType);
            HashSet<string> layerTypesForSource = allObjects
                .Where(candidate => string.Equals(candidate.SourceLayer, item.SourceLayer, StringComparison.OrdinalIgnoreCase))
                .Select(candidate => ResolveCanvasLayerType(candidate.ObjectType))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            string name = sourceLayer;
            if (layerTypesForSource.Count > 1 &&
                !string.Equals(layerType, GetPrimaryLayerType(layerTypesForSource), StringComparison.OrdinalIgnoreCase))
            {
                name = $"{sourceLayer} {GetLayerTypeSuffix(layerType)}";
            }

            return new TargetLayerSpec(
                $"{sourceLayer}|{layerType}",
                name,
                sourceLayer,
                layerType,
                IsApplicationLayer: false);
        }

        private static string GetTargetLayerKey(
            ExternalRawObject item,
            ExternalLayerImportOptions options)
        {
            return GetTargetLayerSpec(item, [item], options).Key;
        }

        /// <summary>
        /// Removes objects whose geometry cannot be represented on the application layer they were
        /// mapped to. Area layers (Blocks, Road Parcel, Project Boundary, parcels, ...) accept only
        /// closed polygons; line layers (Road Centerline) accept only linear geometry. Objects kept
        /// as external/source layers are never filtered. Skipped objects are summarised in
        /// <paramref name="warnings"/>.
        /// </summary>
        private static List<ExternalRawObject> FilterObjectsForTargetCompatibility(
            IReadOnlyList<ExternalRawObject> objects,
            ExternalLayerImportOptions options,
            List<string> warnings)
        {
            if (!options.UseTargetLayerMapping)
                return objects.ToList();

            List<ExternalRawObject> accepted = [];
            Dictionary<(string Source, string Target, bool TargetIsLine), int> skipped = new();

            foreach (ExternalRawObject item in objects)
            {
                string? mappedTarget = GetMappedTargetLayerName(options, item.SourceLayer);
                BlockLayoutPlanTargetLayerDefinition? definition =
                    BlockLayoutPlanImportTargets.Find(mappedTarget);

                // Not mapped to a managed application layer -> kept as an external layer, import as-is.
                if (definition == null)
                {
                    accepted.Add(item);
                    continue;
                }

                bool targetIsLine = IsLineTargetLayerType(definition.LayerType);
                bool compatible = targetIsLine ? IsLinearImportObject(item) : IsAreaImportObject(item);
                if (compatible)
                {
                    accepted.Add(item);
                    continue;
                }

                (string, string, bool) key = (item.SourceLayer, definition.Name, targetIsLine);
                skipped[key] = skipped.GetValueOrDefault(key) + 1;
            }

            foreach (KeyValuePair<(string Source, string Target, bool TargetIsLine), int> entry in skipped)
            {
                string reason = entry.Key.TargetIsLine
                    ? "only line geometry can be imported here"
                    : "only closed polylines/polygons can be imported here";
                warnings.Add(
                    $"'{entry.Key.Source}' → {entry.Key.Target}: skipped {entry.Value} object(s) — {reason}.");
            }

            return accepted;
        }

        private static List<ExternalRawObject> FilterObjectsForCanvasDatabaseCompatibility(
            IReadOnlyList<ExternalRawObject> objects,
            List<string> warnings)
        {
            List<ExternalRawObject> accepted = [];
            Dictionary<string, int> skippedByLayer = new(StringComparer.OrdinalIgnoreCase);

            foreach (ExternalRawObject item in objects)
            {
                NormalizeGeometryForCanvasDatabase(item.Geometry);
                if (IsCanvasDatabaseCompatibleGeometry(item.Geometry))
                {
                    accepted.Add(item);
                    continue;
                }

                skippedByLayer[item.SourceLayer] = skippedByLayer.GetValueOrDefault(item.SourceLayer) + 1;
            }

            foreach (KeyValuePair<string, int> entry in skippedByLayer)
            {
                warnings.Add(
                    $"'{entry.Key}': skipped {entry.Value} object(s) because the geometry cannot be stored on the canvas.");
            }

            return accepted;
        }

        private static List<CanvasObject> FilterCanvasObjectsForCanvasDatabaseCompatibility(
            IReadOnlyList<CanvasObject> objects,
            List<string> warnings)
        {
            List<CanvasObject> accepted = [];
            Dictionary<string, int> skippedByLayer = new(StringComparer.OrdinalIgnoreCase);

            foreach (CanvasObject item in objects)
            {
                NormalizeGeometryForCanvasDatabase(item.Shape);
                if (IsCanvasDatabaseCompatibleGeometry(item.Shape))
                {
                    accepted.Add(item);
                    continue;
                }

                string layerName = item.CanvasLayer?.Name ?? item.CanvasLayerId.ToString();
                skippedByLayer[layerName] = skippedByLayer.GetValueOrDefault(layerName) + 1;
            }

            foreach (KeyValuePair<string, int> entry in skippedByLayer)
            {
                warnings.Add(
                    $"{entry.Key}: skipped {entry.Value} object(s) because the geometry cannot be stored on the canvas.");
            }

            return accepted;
        }

        private static bool IsLineTargetLayerType(string layerType)
        {
            return string.Equals(layerType, "RoadCenterline", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, CanvasLayerTreeService.RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBlockTargetLayer(CanvasLayer layer)
        {
            return CanvasLayerTreeService.IsBlockLayoutLayer(layer);
        }

        private static bool IsAreaImportObject(ExternalRawObject item)
        {
            return item.Geometry is Polygon or MultiPolygon ||
                   item.ObjectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase) ||
                   item.ObjectType.Equals("Circle", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLinearImportObject(ExternalRawObject item)
        {
            return item.Geometry is LineString or MultiLineString ||
                   item.ObjectType.Equals("Polyline", StringComparison.OrdinalIgnoreCase) ||
                   item.ObjectType.Equals("Line", StringComparison.OrdinalIgnoreCase) ||
                   item.ObjectType.Equals("Arc", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Enforces one-object-per-geometry per target layer. In-batch duplicates are dropped
        /// silently; objects that duplicate geometry already stored in the layer are resolved via
        /// <paramref name="resolveDuplicateGeometries"/> (replace existing, skip incoming, or cancel).
        /// </summary>
        private static async Task<ImportDuplicateResolution> ResolveImportDuplicateGeometriesAsync(
            AppDbContext context,
            List<CanvasObject> incomingObjects,
            Func<int, ImportDuplicateGeometryChoice>? resolveDuplicateGeometries,
            IReadOnlySet<int> replacedTargetLayerIds,
            CancellationToken ct)
        {
            // 1) Remove duplicates within the incoming batch (keep the first per layer + geometry).
            Dictionary<int, List<CanvasObject>> acceptedByLayer = new();
            int inBatchSkipped = 0;
            foreach (CanvasObject candidate in incomingObjects)
            {
                if (!acceptedByLayer.TryGetValue(candidate.CanvasLayerId, out List<CanvasObject>? accepted))
                {
                    accepted = [];
                    acceptedByLayer[candidate.CanvasLayerId] = accepted;
                }

                if (accepted.Any(existing => ImportGeometriesEqual(existing.Shape, candidate.Shape)))
                {
                    inBatchSkipped++;
                    continue;
                }

                accepted.Add(candidate);
            }

            // 2) Detect incoming objects that duplicate geometry already present in the layer.
            List<CanvasObject> nonConflicting = [];
            List<(CanvasObject Incoming, CanvasObject Existing)> conflicts = [];
            foreach (KeyValuePair<int, List<CanvasObject>> entry in acceptedByLayer)
            {
                if (replacedTargetLayerIds.Contains(entry.Key))
                {
                    nonConflicting.AddRange(entry.Value);
                    continue;
                }

                List<CanvasObject> existingObjects = await context.CanvasObjects
                    .Where(item => item.CanvasLayerId == entry.Key)
                    .ToListAsync(ct);

                foreach (CanvasObject candidate in entry.Value)
                {
                    if (IsProjectBoundaryImportObject(candidate))
                    {
                        nonConflicting.Add(candidate);
                        continue;
                    }

                    CanvasObject? match = existingObjects
                        .FirstOrDefault(existing => ImportGeometriesEqual(existing.Shape, candidate.Shape));
                    if (match != null)
                        conflicts.Add((candidate, match));
                    else
                        nonConflicting.Add(candidate);
                }
            }

            if (conflicts.Count == 0)
                return new ImportDuplicateResolution(false, nonConflicting, [], inBatchSkipped, 0);

            ImportDuplicateGeometryChoice choice =
                resolveDuplicateGeometries?.Invoke(conflicts.Count) ?? ImportDuplicateGeometryChoice.Skip;

            switch (choice)
            {
                case ImportDuplicateGeometryChoice.Cancel:
                    return new ImportDuplicateResolution(true, [], [], 0, 0);

                case ImportDuplicateGeometryChoice.Replace:
                    List<CanvasObject> existingToRemove = conflicts
                        .Select(conflict => conflict.Existing)
                        .DistinctBy(existing => existing.Id)
                        .ToList();
                    List<CanvasObject> toAdd = nonConflicting
                        .Concat(conflicts.Select(conflict => conflict.Incoming))
                        .ToList();
                    return new ImportDuplicateResolution(false, toAdd, existingToRemove, inBatchSkipped, existingToRemove.Count);

                case ImportDuplicateGeometryChoice.Skip:
                default:
                    return new ImportDuplicateResolution(false, nonConflicting, [], inBatchSkipped + conflicts.Count, 0);
            }
        }

        private static async Task<ImportProjectBoundaryResolution> ResolveImportProjectBoundaryAsync(
            AppDbContext context,
            List<CanvasObject> incomingObjects,
            Func<int, ImportDuplicateGeometryChoice>? resolveProjectBoundaryConflict,
            IReadOnlySet<int> replacedTargetLayerIds,
            CancellationToken ct)
        {
            List<CanvasObject> projectBoundaryObjects = incomingObjects
                .Where(IsProjectBoundaryImportObject)
                .ToList();
            if (projectBoundaryObjects.Count == 0)
            {
                return new ImportProjectBoundaryResolution(false, incomingObjects, [], 0, 0, false);
            }

            CanvasObject keptBoundaryObject = projectBoundaryObjects
                .OrderByDescending(item => item.Shape?.Area ?? 0.0)
                .First();
            int skippedExtraIncomingCount = projectBoundaryObjects.Count - 1;
            List<CanvasObject> objectsToAdd = incomingObjects
                .Where(item => !IsProjectBoundaryImportObject(item) || item.Id == keptBoundaryObject.Id)
                .ToList();

            List<CanvasObject> existingBoundaryObjects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.CanvasLayer != null &&
                    !replacedTargetLayerIds.Contains(item.CanvasLayerId) &&
                    (item.CanvasLayer.Name == "Project Boundary" ||
                     item.CanvasLayer.LayerType == "ProjectBoundary"))
                .ToListAsync(ct);
            if (existingBoundaryObjects.Count == 0)
            {
                return new ImportProjectBoundaryResolution(
                    false,
                    objectsToAdd,
                    [],
                    skippedExtraIncomingCount,
                    0,
                    false);
            }

            ImportDuplicateGeometryChoice choice =
                resolveProjectBoundaryConflict?.Invoke(existingBoundaryObjects.Count)
                ?? ImportDuplicateGeometryChoice.Skip;

            return choice switch
            {
                ImportDuplicateGeometryChoice.Cancel => new ImportProjectBoundaryResolution(true, [], [], 0, 0, false),
                ImportDuplicateGeometryChoice.Replace => new ImportProjectBoundaryResolution(
                    false,
                    objectsToAdd,
                    existingBoundaryObjects,
                    skippedExtraIncomingCount,
                    existingBoundaryObjects.Count,
                    false),
                _ => new ImportProjectBoundaryResolution(
                    false,
                    incomingObjects
                        .Where(item => !IsProjectBoundaryImportObject(item))
                        .ToList(),
                    [],
                    skippedExtraIncomingCount,
                    0,
                    true)
            };
        }

        private static bool IsProjectBoundaryImportObject(CanvasObject item)
        {
            return item.CanvasLayer != null &&
                   CanvasLayerTreeService.IsProjectBoundaryLayer(item.CanvasLayer);
        }

        private static bool ImportGeometriesEqual(NtsGeometry? left, NtsGeometry? right)
        {
            if (left == null || right == null)
                return false;

            if (left.IsEmpty || right.IsEmpty)
                return left.IsEmpty && right.IsEmpty;

            if (left.OgcGeometryType != right.OgcGeometryType)
                return false;

            if (left.EqualsExact(right, DuplicateGeometryTolerance))
                return true;

            NtsGeometry leftCopy = left.Copy();
            NtsGeometry rightCopy = right.Copy();
            leftCopy.Normalize();
            rightCopy.Normalize();
            return leftCopy.EqualsExact(rightCopy, DuplicateGeometryTolerance);
        }

        private static string? GetMappedTargetLayerName(
            ExternalLayerImportOptions options,
            string sourceLayer)
        {
            if (!options.UseTargetLayerMapping)
                return null;

            return options.Layers
                .FirstOrDefault(option =>
                    option.Include &&
                    string.Equals(option.LayerName, sourceLayer, StringComparison.OrdinalIgnoreCase))
                ?.TargetLayerName;
        }

        private static string ResolveCanvasLayerType(string objectType)
        {
            return objectType.Trim().ToLowerInvariant() switch
            {
                "text" => CanvasLayerTreeService.AnnotationLayerType,
                "point" => CanvasLayerTreeService.PointLayerType,
                "line" => CanvasLayerTreeService.PolylineLayerType,
                "polyline" => CanvasLayerTreeService.PolylineLayerType,
                "arc" => CanvasLayerTreeService.PolylineLayerType,
                "circle" => CanvasLayerTreeService.PolylineLayerType,
                "polygon" => CanvasLayerTreeService.PolygonLayerType,
                _ => CanvasLayerTreeService.PolylineLayerType
            };
        }

        private static string ResolveObjectType(NtsGeometry geometry)
        {
            return geometry switch
            {
                NtsPoint => "Point",
                LineString => "Polyline",
                Polygon => "Polygon",
                _ => "Polyline"
            };
        }

        private static string GetPrimaryLayerType(IReadOnlySet<string> layerTypes)
        {
            string[] priority =
            [
                CanvasLayerTreeService.PolygonLayerType,
                CanvasLayerTreeService.PolylineLayerType,
                CanvasLayerTreeService.PointLayerType,
                CanvasLayerTreeService.AnnotationLayerType
            ];

            return priority.First(layerTypes.Contains);
        }

        private static string GetLayerTypeSuffix(string layerType)
        {
            return layerType switch
            {
                CanvasLayerTreeService.AnnotationLayerType => "Annotation",
                CanvasLayerTreeService.PointLayerType => "Points",
                CanvasLayerTreeService.PolylineLayerType => "Lines",
                _ => layerType
            };
        }

        private static Color PickLayerColor(int index)
        {
            Color[] palette =
            [
                Color.FromArgb(141, 211, 199),
                Color.FromArgb(255, 255, 179),
                Color.FromArgb(190, 186, 218),
                Color.FromArgb(251, 128, 114),
                Color.FromArgb(128, 177, 211),
                Color.FromArgb(253, 180, 98),
                Color.FromArgb(179, 222, 105),
                Color.FromArgb(252, 205, 229)
            ];

            return palette[index % palette.Length];
        }

        private static Color Darken(Color color, float factor)
        {
            factor = Math.Clamp(factor, 0.0f, 1.0f);
            return Color.FromArgb(
                255,
                (int)Math.Round(color.R * factor),
                (int)Math.Round(color.G * factor),
                (int)Math.Round(color.B * factor));
        }

        private static string ToHtml(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private static string BuildObjectDescription(
            ExternalRawObject item,
            CanvasLayer layer,
            ExternalLayerImportOptions options)
        {
            if (options.UseTargetLayerMapping &&
                !string.IsNullOrWhiteSpace(GetMappedTargetLayerName(options, item.SourceLayer)))
            {
                return $"Imported block layout {item.ObjectType.ToLowerInvariant()} from layer {item.SourceLayer} to {layer.Name}";
            }

            return $"Imported external {item.ObjectType.ToLowerInvariant()} from layer {item.SourceLayer}";
        }

        private static string? CopySourceFileToProjectFolder(ProjectSession session, string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return null;

            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            string baseName = Path.GetFileNameWithoutExtension(sourcePath);
            string targetFolder = Path.Combine(session.ProjectFolderPath, "Imports", "External");
            Directory.CreateDirectory(targetFolder);

            string targetBaseName = GetAvailableImportBaseName(targetFolder, baseName, extension);
            string relativePath = Path.Combine("Imports", "External", targetBaseName + extension);

            foreach (string path in EnumerateSourceSidecarFiles(sourcePath))
            {
                string sidecarExtension = Path.GetExtension(path);
                File.Copy(path, Path.Combine(targetFolder, targetBaseName + sidecarExtension), overwrite: true);
            }

            return relativePath.Replace(Path.DirectorySeparatorChar, '/');
        }

        private static IEnumerable<string> EnumerateSourceSidecarFiles(string sourcePath)
        {
            string extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension != ".shp")
                return [sourcePath];

            string folder = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(sourcePath);
            string[] sidecarExtensions =
            [
                ".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix",
                ".sbn", ".sbx", ".xml", ".fix"
            ];

            return sidecarExtensions
                .Select(item => Path.Combine(folder, baseName + item))
                .Where(File.Exists)
                .ToList();
        }

        private static string GetAvailableImportBaseName(
            string targetFolder,
            string baseName,
            string primaryExtension)
        {
            string candidate = SanitizeLayerName(baseName) ?? "external-source";
            if (!File.Exists(Path.Combine(targetFolder, candidate + primaryExtension)))
                return candidate;

            return $"{candidate}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        private static string? SanitizeLayerName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            char[] invalid = Path.GetInvalidFileNameChars();
            string cleaned = new(value.Trim().Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        private static bool NeedsTransform(string source, string target)
        {
            return !string.Equals(
                NormalizeDefinition(ProjectCrsWktBuilder.SanitizeForProj(source)),
                NormalizeDefinition(ProjectCrsWktBuilder.SanitizeForProj(target)),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeDefinition(string value)
        {
            return value.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        private static List<ExternalRawObject> Transform(
            IReadOnlyList<ExternalRawObject> sourceObjects,
            string sourceDefinition,
            string targetDefinition)
        {
            GdalBootstrapper.ConfigureAll();
            using SpatialReference sourceSrs = CreateSpatialReference(sourceDefinition);
            using SpatialReference targetSrs = CreateSpatialReference(targetDefinition);
            using CoordinateTransformation transformation = new(sourceSrs, targetSrs);

            List<ExternalRawObject> transformed = [];
            foreach (ExternalRawObject sourceObject in sourceObjects)
            {
                NtsGeometry copy = sourceObject.Geometry.Copy();
                copy.Apply(new CoordinateTransformFilter(transformation));
                copy.GeometryChanged();
                if (!copy.IsEmpty)
                    transformed.Add(sourceObject with { Geometry = copy });
            }

            return transformed;
        }

        private static SpatialReference CreateSpatialReference(string definition)
        {
            definition = ProjectCrsWktBuilder.SanitizeForProj(definition);
            SpatialReference spatialReference = new(string.Empty);
            spatialReference.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            if (spatialReference.SetFromUserInput(definition) != 0)
            {
                string wkt = definition;
                if (spatialReference.ImportFromWkt(ref wkt) != 0)
                {
                    spatialReference.Dispose();
                    throw new InvalidOperationException($"Could not parse CRS definition '{definition}'.");
                }
            }

            spatialReference.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            return spatialReference;
        }

        private static string? GetLayerCrsDefinition(Layer layer)
        {
            using SpatialReference spatialReference = layer.GetSpatialRef();
            if (spatialReference == null)
                return null;

            spatialReference.SetAxisMappingStrategy(AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            spatialReference.AutoIdentifyEPSG();

            string? authorityName =
                spatialReference.GetAuthorityName(null) ??
                spatialReference.GetAuthorityName("PROJCS") ??
                spatialReference.GetAuthorityName("GEOGCS");
            string? authorityCode =
                spatialReference.GetAuthorityCode(null) ??
                spatialReference.GetAuthorityCode("PROJCS") ??
                spatialReference.GetAuthorityCode("GEOGCS");

            if (!string.IsNullOrWhiteSpace(authorityName) &&
                !string.IsNullOrWhiteSpace(authorityCode))
            {
                return $"{authorityName}:{authorityCode}";
            }

            spatialReference.ExportToWkt(out string wkt, []);
            return string.IsNullOrWhiteSpace(wkt) ? null : wkt;
        }

        private static void NormalizeGeometryForCanvasDatabase(NtsGeometry geometry)
        {
            if (geometry == null)
                return;

            geometry.SRID = 0;
            if (geometry is GeometryCollection collection)
            {
                for (int index = 0; index < collection.NumGeometries; index++)
                    NormalizeGeometryForCanvasDatabase(collection.GetGeometryN(index));
            }
        }

        private static bool IsCanvasDatabaseCompatibleGeometry(NtsGeometry? geometry)
        {
            if (geometry == null || geometry.IsEmpty || geometry.SRID != 0)
                return false;

            if (geometry is GeometryCollection collection)
            {
                for (int index = 0; index < collection.NumGeometries; index++)
                {
                    if (!IsCanvasDatabaseCompatibleGeometry(collection.GetGeometryN(index)))
                        return false;
                }
            }

            return geometry switch
            {
                NtsPoint point => !point.IsEmpty,
                MultiPoint multiPoint => multiPoint.NumGeometries > 0,
                LineString line => line.NumPoints >= 2,
                MultiLineString multiLine => multiLine.NumGeometries > 0,
                Polygon polygon => polygon.Area > 0.000001,
                MultiPolygon multiPolygon => multiPolygon.NumGeometries > 0,
                _ => false
            };
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
                    throw new InvalidOperationException("An imported coordinate could not be transformed.");

                seq.SetX(i, point[0]);
                seq.SetY(i, point[1]);
            }
        }

        private sealed class LayerStats
        {
            private int PolygonCount { get; set; }
            private int PolylineCount { get; set; }
            private int PointCount { get; set; }

            public int ObjectCount => PolygonCount + PolylineCount + PointCount;

            public string ObjectTypes
            {
                get
                {
                    List<string> parts = [];
                    if (PolygonCount > 0) parts.Add($"Polygon: {PolygonCount}");
                    if (PolylineCount > 0) parts.Add($"Line: {PolylineCount}");
                    if (PointCount > 0) parts.Add($"Point: {PointCount}");
                    return string.Join(", ", parts);
                }
            }

            public void Add(string objectType)
            {
                if (string.Equals(objectType, "Polygon", StringComparison.OrdinalIgnoreCase))
                    PolygonCount++;
                else if (string.Equals(objectType, "Point", StringComparison.OrdinalIgnoreCase))
                    PointCount++;
                else
                    PolylineCount++;
            }
        }

        private sealed record ImportDuplicateResolution(
            bool Cancelled,
            List<CanvasObject> ObjectsToAdd,
            List<CanvasObject> ExistingObjectsToRemove,
            int SkippedDuplicateCount,
            int ReplacedExistingCount);

        private sealed record ImportProjectBoundaryResolution(
            bool Cancelled,
            List<CanvasObject> ObjectsToAdd,
            List<CanvasObject> ExistingObjectsToRemove,
            int SkippedExtraIncomingCount,
            int ReplacedExistingCount,
            bool SkippedForExistingBoundary);

        private sealed record ExternalRawObject(
            NtsGeometry Geometry,
            string ObjectType,
            string SourceLayer,
            string? LabelText,
            string? SourceHandle,
            Dictionary<string, string?> Attributes,
            string? CurveMetadataJson);

        private sealed record CurvePoint(double X, double Y);

        private sealed record ArcCurve(
            double CenterX,
            double CenterY,
            double Radius,
            double StartAngleRadians,
            double SweepAngleRadians);

        private sealed record PolylineImportGeometry(
            NtsGeometry? Geometry,
            bool IsClosed,
            string? CurveMetadataJson);

        private sealed record CurveMetadata(
            string ShapeType,
            double CenterX,
            double CenterY,
            double? RadiusPointX,
            double? RadiusPointY,
            double? Radius,
            double? StartAngleRadians,
            double? SweepAngleRadians);

        private sealed record PolylineCurveMetadata(
            string ShapeType,
            bool IsClosed,
            List<CurveSegmentMetadata> Segments);

        private sealed record CurveSegmentMetadata(
            string Kind,
            double StartX,
            double StartY,
            double EndX,
            double EndY,
            double? CenterX,
            double? CenterY,
            double? Radius,
            double? StartAngleRadians,
            double? SweepAngleRadians)
        {
            public double Length
            {
                get
                {
                    double dx = EndX - StartX;
                    double dy = EndY - StartY;
                    return Math.Sqrt(dx * dx + dy * dy);
                }
            }

            public static CurveSegmentMetadata Line(
                double startX,
                double startY,
                double endX,
                double endY) =>
                new("Line", startX, startY, endX, endY, null, null, null, null, null);

            public static CurveSegmentMetadata Arc(
                double startX,
                double startY,
                double endX,
                double endY,
                double centerX,
                double centerY,
                double radius,
                double startAngleRadians,
                double sweepAngleRadians) =>
                new(
                    "Arc",
                    startX,
                    startY,
                    endX,
                    endY,
                    centerX,
                    centerY,
                    radius,
                    startAngleRadians,
                    sweepAngleRadians);
        }

        private sealed record ExternalLayerStyle(Color Color);

        private sealed record TargetLayerSpec(
            string Key,
            string Name,
            string SourceLayer,
            string LayerType,
            bool IsApplicationLayer);

        private sealed record ExternalLayerObjectMetadata(
            string Kind,
            string SourceFormat,
            string SourceFileName,
            string? ProjectSourceFile,
            string SourceLayer,
            string? SourceHandle,
            string? CurveMetadataJson,
            Dictionary<string, string?>? Attributes);
    }
}
