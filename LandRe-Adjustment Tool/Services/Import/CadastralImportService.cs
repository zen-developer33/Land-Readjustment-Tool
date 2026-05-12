using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.Import.Readers;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using OSGeo.OSR;

namespace Land_Readjustment_Tool.Services.Import
{
    public interface ICadastralImportService
    {
        CadastralFileInfo Inspect(string filePath);

        Task<CadastralImportResult> ImportAsync(
            ProjectSession session,
            string filePath,
            CadastralImportOptions options,
            CancellationToken ct = default);
    }

    public sealed class CadastralImportService : ICadastralImportService
    {
        private readonly CadastralVectorReader _reader = new();
        private readonly IProjectRasterCrsResolver _projectCrsResolver;

        public CadastralImportService(IProjectRasterCrsResolver projectCrsResolver)
        {
            _projectCrsResolver = projectCrsResolver;
        }

        public CadastralFileInfo Inspect(string filePath)
        {
            return _reader.Inspect(filePath);
        }

        public async Task<CadastralImportResult> ImportAsync(
            ProjectSession session,
            string filePath,
            CadastralImportOptions options,
            CancellationToken ct = default)
        {
            List<CadastralRawParcel> rawParcels = _reader.Read(filePath, options);
            if (rawParcels.Count == 0)
            {
                return new CadastralImportResult(
                    false,
                    "No importable objects were found in the selected layer(s).",
                    0,
                    0,
                    0,
                    0,
                    null);
            }

            ProjectRasterCrsContext projectCrs =
                await _projectCrsResolver.ResolveAsync(session, ct);

            List<CadastralRawParcel> parcels = NeedsTransform(
                    options.SourceCrsCode,
                    projectCrs.TargetSrsDefinition)
                ? TransformParcels(rawParcels, options.SourceCrsCode, projectCrs.TargetSrsDefinition)
                : rawParcels
                    .Select(parcel => parcel with { Geometry = parcel.Geometry.Copy() })
                    .ToList();

            foreach (CadastralRawParcel parcel in parcels)
                NormalizeGeometryForCanvasDatabase(parcel.Geometry);

            AppDbContext context = session.GetDbContext();
            await context.Database.MigrateAsync(ct);

            Dictionary<string, CanvasLayer> targetLayers = await GetOrCreateTargetLayersAsync(
                context,
                parcels,
                ct);

            Dictionary<string, BaselineParcel> baselineLookup = options.AutoAssignParcelRecords
                ? await LoadBaselineParcelLookupAsync(context, ct)
                : new Dictionary<string, BaselineParcel>(StringComparer.OrdinalIgnoreCase);

            DateTime now = DateTime.Now;
            List<CanvasObject> objects = [];
            foreach (CadastralRawParcel parcel in parcels)
            {
                string layerKey = GetTargetLayerKey(parcel);
                CanvasLayer layer = targetLayers[layerKey];
                double area = Math.Abs(parcel.Geometry.Area);
                bool isParcelPolygon = IsParcelPolygon(parcel);
                BaselineParcel? assignedParcel = isParcelPolygon
                    ? TryFindBaselineParcel(
                        baselineLookup,
                        parcel.MapSheetNo,
                        parcel.ParcelNo)
                    : null;

                CadastralCanvasMetadata metadata = new()
                {
                    SourceFormat = Path.GetExtension(filePath).TrimStart('.').ToUpperInvariant(),
                    SourceFileName = Path.GetFileName(filePath),
                    SourceLayer = parcel.SourceLayer,
                    MapSheetNo = parcel.MapSheetNo,
                    ParcelNo = parcel.ParcelNo,
                    CalculatedAreaSqm = area,
                    SourceHandle = parcel.SourceHandle,
                    MatchedText = parcel.MatchedText,
                    AttributesJson = parcel.Attributes.Count == 0
                        ? null
                        : JsonSerializer.Serialize(parcel.Attributes),
                    BaselineParcelId = assignedParcel?.Id,
                    FullUniqueParcelCode = assignedParcel?.FullUniqueParcelCode,
                    RecordAreaSqm = assignedParcel?.OriginalAreaSqm,
                    OwnerName = assignedParcel?.LandOwner?.FullName,
                    LandUse = assignedParcel?.LandUse,
                    AssignmentStatus = assignedParcel == null
                        ? "Unassigned"
                        : "AutoAssigned",
                    ImportedAt = now
                };

                CanvasObject canvasObject = new()
                {
                    CanvasLayerId = layer.Id,
                    CanvasLayer = layer,
                    ObjectType = parcel.ObjectType,
                    Shape = parcel.Geometry,
                    GeometryMetadataJson = JsonSerializer.Serialize(metadata),
                    LabelText = ResolveObjectLabelText(parcel),
                    BaselineParcelId = assignedParcel?.Id,
                    BorderColorOverride = IsAnnotationObject(parcel)
                        ? layer.LabelColor
                        : null,
                    ObjectDescription = BuildDescription(parcel),
                    IsVisible = true,
                    IsLocked = false,
                    SourceDxfHandle = parcel.SourceHandle,
                    CreatedDate = now,
                    LastModifiedDate = now
                };

                objects.Add(canvasObject);
                if (assignedParcel != null && assignedParcel.CanvasObjectId == null)
                    assignedParcel.CanvasObjectId = canvasObject.Id;
            }

            await context.CanvasObjects.AddRangeAsync(objects, ct);
            foreach (CanvasLayer layer in targetLayers.Values)
            {
                layer.SourceFile = filePath;
                layer.ImportedDate = now;
                layer.LastModifiedDate = now;
            }

            try
            {
                await context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                session.Logger.LogError("Cadastral map import save failed.", ex);
                return new CadastralImportResult(
                    false,
                    $"Could not save cadastral map: {BuildExceptionMessage(ex)}",
                    0,
                    0,
                    0,
                    0,
                    null);
            }

            Envelope envelope = new();
            foreach (Geometry geometry in parcels.Select(parcel => parcel.Geometry))
                envelope.ExpandToInclude(geometry.EnvelopeInternal);

            int assigned = objects.Count(item => item.BaselineParcelId.HasValue);
            return new CadastralImportResult(
                true,
                null,
                objects.Count,
                assigned,
                objects.Count - assigned,
                parcels.Count(item => !string.IsNullOrWhiteSpace(item.MatchedText)),
                envelope);
        }

        private static async Task<Dictionary<string, CanvasLayer>> GetOrCreateTargetLayersAsync(
            AppDbContext context,
            IReadOnlyList<CadastralRawParcel> parcels,
            CancellationToken ct)
        {
            Dictionary<string, CanvasLayer> layers = new(StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<TargetLayerSpec> specs = parcels
                .Select(parcel => GetTargetLayerSpec(parcel, parcels))
                .GroupBy(spec => spec.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(spec => spec.Name)
                .ThenBy(spec => spec.LayerType)
                .ToList();

            int nextDisplayOrder =
                (await context.CanvasLayers
                    .Select(layer => (int?)layer.DisplayOrder)
                    .MaxAsync(ct) ?? -1) + 1;

            foreach (TargetLayerSpec spec in specs)
            {
                CanvasLayer? layer = await context.CanvasLayers
                    .FirstOrDefaultAsync(
                        item => item.Name == spec.Name &&
                                item.LayerType == spec.LayerType,
                        ct);

                if (layer == null)
                {
                    layer = CreateImportedCadastralLayer(spec.Name, spec.LayerType, nextDisplayOrder++);
                    await context.CanvasLayers.AddAsync(layer, ct);
                    await context.SaveChangesAsync(ct);
                }
                else
                {
                    ApplyImportedCadastralLayerDefaults(layer);
                }

                layers[spec.Key] = layer;
            }

            return layers;
        }

        private static CanvasLayer CreateImportedCadastralLayer(
            string name,
            string layerType,
            int displayOrder)
        {
            DateTime now = DateTime.Now;
            CanvasLayer layer = new()
            {
                Name = name,
                LayerType = layerType,
                IsVisible = true,
                IsLocked = true,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = displayOrder,
                CreatedDate = now,
                LastModifiedDate = now,
                Description = "Imported cadastral map layer"
            };

            ApplyImportedCadastralLayerDefaults(layer);
            return layer;
        }

        private static void ApplyImportedCadastralLayerDefaults(CanvasLayer layer)
        {
            layer.IsLocked = true;
            layer.IsSelectable = true;

            if (CanvasLayerTreeService.IsAnnotationLayer(layer))
            {
                layer.BorderColor = string.IsNullOrWhiteSpace(layer.BorderColor)
                    ? "#111111"
                    : layer.BorderColor;
                layer.FillColor = null;
                layer.FillStyle = "None";
                layer.FillTransparency = 100;
                layer.LineWeight = 0;
                layer.LabelColor = string.IsNullOrWhiteSpace(layer.LabelColor)
                    ? layer.BorderColor
                    : layer.LabelColor;
                layer.LabelFontName = string.IsNullOrWhiteSpace(layer.LabelFontName)
                    ? "Segoe UI"
                    : layer.LabelFontName;
                layer.LabelFontSize = layer.LabelFontSize <= 0 ? 9.0 : layer.LabelFontSize;
                layer.PointSymbol = string.IsNullOrWhiteSpace(layer.PointSymbol)
                    ? "Dot"
                    : layer.PointSymbol;
                layer.PointSize = layer.PointSize <= 0 ? 1.0 : layer.PointSize;
                return;
            }

            layer.BorderColor = string.IsNullOrWhiteSpace(layer.BorderColor)
                ? ResolveDefaultBorderColor(layer.LayerType)
                : layer.BorderColor;
            layer.FillColor ??= CanvasLayerTreeService.IsPolygonLayer(layer)
                ? "#C8E8F4"
                : null;
            layer.FillTransparency = CanvasLayerTreeService.IsPolygonLayer(layer)
                ? layer.FillTransparency <= 0 ? 55 : layer.FillTransparency
                : 100;
            layer.LineWeight = layer.LineWeight <= 0 ? 1.4 : layer.LineWeight;
            layer.LineStyle = string.IsNullOrWhiteSpace(layer.LineStyle)
                ? "Solid"
                : layer.LineStyle;
            layer.LineTypeScale = layer.LineTypeScale <= 0 ? 1.0 : layer.LineTypeScale;
            layer.FillStyle = string.IsNullOrWhiteSpace(layer.FillStyle)
                ? CanvasLayerTreeService.IsPolygonLayer(layer) ? "Solid" : "None"
                : layer.FillStyle;
            layer.LabelColor = string.IsNullOrWhiteSpace(layer.LabelColor)
                ? "#000000"
                : layer.LabelColor;
            layer.PointSymbol = string.IsNullOrWhiteSpace(layer.PointSymbol)
                ? "Dot"
                : layer.PointSymbol;
            layer.PointSize = layer.PointSize <= 0 ? 5.0 : layer.PointSize;
        }

        private static string GetTargetLayerKey(CadastralRawParcel parcel)
        {
            return GetTargetLayerSpec(parcel, [parcel]).Key;
        }

        private static TargetLayerSpec GetTargetLayerSpec(
            CadastralRawParcel parcel,
            IReadOnlyList<CadastralRawParcel> allParcels)
        {
            string baseName = SanitizeLayerName(parcel.CanvasLayerName)
                              ?? SanitizeLayerName(parcel.SourceLayer)
                              ?? "Unknown";
            string layerType = ResolveCanvasLayerType(parcel.ObjectType);
            IReadOnlySet<string> layerTypesForName = allParcels
                .Where(item =>
                    string.Equals(
                        SanitizeLayerName(item.CanvasLayerName) ??
                        SanitizeLayerName(item.SourceLayer) ??
                        "Unknown",
                        baseName,
                        StringComparison.OrdinalIgnoreCase))
                .Select(item => ResolveCanvasLayerType(item.ObjectType))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            string layerName = baseName;
            if (layerTypesForName.Count > 1 &&
                !string.Equals(layerType, GetPrimaryLayerType(layerTypesForName), StringComparison.OrdinalIgnoreCase))
            {
                layerName = $"{baseName} {GetLayerTypeSuffix(layerType)}";
            }

            return new TargetLayerSpec(
                $"{layerType}|{baseName}",
                layerName,
                layerType);
        }

        private static string ResolveCanvasLayerType(string objectType)
        {
            return objectType.Trim().ToLowerInvariant() switch
            {
                "text" => CanvasLayerTreeService.AnnotationLayerType,
                "point" => CanvasLayerTreeService.PointLayerType,
                "line" => CanvasLayerTreeService.PolylineLayerType,
                "polyline" => CanvasLayerTreeService.PolylineLayerType,
                "polygon" => CanvasLayerTreeService.PolygonLayerType,
                _ => CanvasLayerTreeService.PolylineLayerType
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

            return priority.First(type => layerTypes.Contains(type));
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

        private static string ResolveDefaultBorderColor(string layerType)
        {
            return layerType switch
            {
                CanvasLayerTreeService.PointLayerType => "#1B5E20",
                CanvasLayerTreeService.PolylineLayerType => "#1976D2",
                CanvasLayerTreeService.AnnotationLayerType => "#111111",
                _ => "#8FCDE4"
            };
        }

        private static bool IsParcelPolygon(CadastralRawParcel parcel)
        {
            return string.Equals(parcel.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAnnotationObject(CadastralRawParcel parcel)
        {
            return string.Equals(parcel.ObjectType, "Text", StringComparison.OrdinalIgnoreCase);
        }

        private static string? ResolveObjectLabelText(CadastralRawParcel parcel)
        {
            return IsAnnotationObject(parcel)
                ? parcel.ParcelNo
                : parcel.ParcelNo;
        }

        private static async Task<Dictionary<string, BaselineParcel>> LoadBaselineParcelLookupAsync(
            AppDbContext context,
            CancellationToken ct)
        {
            List<BaselineParcel> parcels = await context.BaselineParcels
                .Include(parcel => parcel.LandOwner)
                .ToListAsync(ct);
            Dictionary<string, BaselineParcel> lookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (BaselineParcel parcel in parcels)
            {
                if (!string.IsNullOrWhiteSpace(parcel.FullUniqueParcelCode))
                    lookup[parcel.FullUniqueParcelCode.Trim()] = parcel;

                lookup[BuildParcelCode(parcel.MapSheetNo, parcel.ParcelNo)] = parcel;
            }

            return lookup;
        }

        private static BaselineParcel? TryFindBaselineParcel(
            IReadOnlyDictionary<string, BaselineParcel> lookup,
            string? mapSheetNo,
            string? parcelNo)
        {
            if (string.IsNullOrWhiteSpace(mapSheetNo) ||
                string.IsNullOrWhiteSpace(parcelNo))
            {
                return null;
            }

            return lookup.TryGetValue(BuildParcelCode(mapSheetNo, parcelNo), out BaselineParcel? parcel)
                ? parcel
                : null;
        }

        private static string BuildParcelCode(string? mapSheetNo, string? parcelNo)
        {
            return $"{(mapSheetNo ?? string.Empty).Trim().ToUpperInvariant()}::{(parcelNo ?? string.Empty).Trim().ToUpperInvariant()}";
        }

        private static string? SanitizeLayerName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            char[] invalid = Path.GetInvalidFileNameChars();
            string cleaned = new(
                value.Trim()
                    .Select(ch => invalid.Contains(ch) ? '_' : ch)
                    .ToArray());

            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        private static string BuildDescription(CadastralRawParcel parcel)
        {
            if (IsAnnotationObject(parcel))
            {
                string text = string.IsNullOrWhiteSpace(parcel.ParcelNo)
                    ? "empty text"
                    : $"text '{parcel.ParcelNo}'";
                return $"Imported cadastral annotation, {text}";
            }

            string sheet = string.IsNullOrWhiteSpace(parcel.MapSheetNo)
                ? "unknown sheet"
                : parcel.MapSheetNo;
            string parcelNo = string.IsNullOrWhiteSpace(parcel.ParcelNo)
                ? "unassigned parcel"
                : $"parcel {parcel.ParcelNo}";
            return $"Original cadastral {parcelNo}, map sheet {sheet}";
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

        private static List<CadastralRawParcel> TransformParcels(
            IReadOnlyList<CadastralRawParcel> sourceParcels,
            string sourceDefinition,
            string targetDefinition)
        {
            GdalBootstrapper.ConfigureAll();
            using SpatialReference sourceSrs = CreateSpatialReference(sourceDefinition);
            using SpatialReference targetSrs = CreateSpatialReference(targetDefinition);
            using CoordinateTransformation transformation = new(sourceSrs, targetSrs);

            List<CadastralRawParcel> transformed = [];
            foreach (CadastralRawParcel sourceParcel in sourceParcels)
            {
                Geometry copy = sourceParcel.Geometry.Copy();
                copy.Apply(new CoordinateTransformFilter(transformation));
                copy.GeometryChanged();
                Geometry? valid = ValidateImportedGeometry(sourceParcel.ObjectType, copy);
                if (valid != null)
                    transformed.Add(sourceParcel with { Geometry = valid });
            }

            return transformed;
        }

        private static Geometry? ValidateImportedGeometry(string objectType, Geometry geometry)
        {
            if (string.Equals(objectType, "Polygon", StringComparison.OrdinalIgnoreCase))
                return BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(geometry);

            return geometry.IsEmpty ? null : geometry;
        }

        private static SpatialReference CreateSpatialReference(string definition)
        {
            definition = ProjectCrsWktBuilder.SanitizeForProj(definition);
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
                    throw new InvalidOperationException("A cadastral coordinate could not be transformed.");

                seq.SetX(i, point[0]);
                seq.SetY(i, point[1]);
            }
        }

        private sealed record TargetLayerSpec(
            string Key,
            string Name,
            string LayerType);
    }
}
