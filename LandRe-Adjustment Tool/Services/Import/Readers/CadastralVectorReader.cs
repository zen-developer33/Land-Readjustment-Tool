using System.Text.RegularExpressions;
using Land_Readjustment_Tool.Core.Models.Import;
using netDxf;
using netDxf.Entities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using OSGeo.OGR;
using OSGeo.OSR;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;

namespace Land_Readjustment_Tool.Services.Import.Readers
{
    internal sealed class CadastralVectorReader
    {
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);
        private static readonly Regex ParcelTextRegex = new(@"\b[\p{L}\d\-\/]+\b", RegexOptions.Compiled);

        public CadastralFileInfo Inspect(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".dxf" => InspectDxf(filePath),
                ".dwg" => InspectOgr(filePath, "DWG"),
                ".shp" => InspectOgr(filePath, "SHP"),
                _ => throw new NotSupportedException($"Cadastral map format not supported: {extension}")
            };
        }

        public List<CadastralRawParcel> Read(string filePath, CadastralImportOptions options)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            List<CadastralRawParcel> parcels = extension switch
            {
                ".dxf" => ReadDxf(filePath, options),
                ".dwg" => ReadOgr(filePath, options, "DWG"),
                ".shp" => ReadOgr(filePath, options, "SHP"),
                _ => throw new NotSupportedException($"Cadastral map format not supported: {extension}")
            };

            return parcels;
        }

        private static CadastralFileInfo InspectDxf(string filePath)
        {
            DxfDocument dxf = DxfDocument.Load(filePath);
            Dictionary<string, LayerStats> stats = new(StringComparer.OrdinalIgnoreCase);

            foreach (Polyline2D polyline in dxf.Entities.Polylines2D)
            {
                LayerStats layerStats = GetStats(stats, polyline.Layer?.Name);
                if (IsClosedPolyline(polyline))
                    layerStats.PolygonCount++;
                else
                    layerStats.PolylineCount++;
            }

            foreach (Polyline3D polyline in dxf.Entities.Polylines3D)
            {
                LayerStats layerStats = GetStats(stats, polyline.Layer?.Name);
                if (IsClosedPolyline(polyline))
                    layerStats.PolygonCount++;
                else
                    layerStats.PolylineCount++;
            }

            foreach (Line line in dxf.Entities.Lines)
            {
                GetStats(stats, line.Layer?.Name).LineCount++;
            }

            foreach (netDxf.Entities.Point point in dxf.Entities.Points)
            {
                GetStats(stats, point.Layer?.Name).PointCount++;
            }

            int textCount = 0;
            foreach (Text text in dxf.Entities.Texts)
            {
                textCount++;
                GetStats(stats, text.Layer?.Name).TextCount++;
            }

            foreach (MText text in dxf.Entities.MTexts)
            {
                textCount++;
                GetStats(stats, text.Layer?.Name).TextCount++;
            }

            IReadOnlyList<CadastralLayerInfo> layers = stats
                .Where(item => item.Value.ObjectCount > 0)
                .OrderBy(item => item.Key)
                .Select(item => new CadastralLayerInfo(
                    item.Key,
                    item.Value.PolygonCount,
                    item.Value.PolylineCount,
                    item.Value.LineCount,
                    item.Value.PointCount,
                    item.Value.TextCount,
                    HasImportableObjects: item.Value.ObjectCount > 0))
                .ToList();

            return new CadastralFileInfo(
                filePath,
                "DXF",
                layers,
                [],
                null,
                RequiresCrsFromUser: true,
                textCount);
        }

        private static List<CadastralRawParcel> ReadDxf(
            string filePath,
            CadastralImportOptions options)
        {
            DxfDocument dxf = DxfDocument.Load(filePath);
            Dictionary<string, CadastralLayerImportOption> layerOptions = options.Layers
                .Where(option => option.Include)
                .ToDictionary(option => option.LayerName, StringComparer.OrdinalIgnoreCase);

            List<CadastralRawParcel> parcels = [];
            foreach (Polyline2D polyline in dxf.Entities.Polylines2D)
            {
                AddDxfPolyline(
                    parcels,
                    polyline.Layer?.Name,
                    polyline.Handle,
                    ReadCoordinates(polyline),
                    IsClosedPolyline(polyline),
                    layerOptions);
            }

            foreach (Polyline3D polyline in dxf.Entities.Polylines3D)
            {
                AddDxfPolyline(
                    parcels,
                    polyline.Layer?.Name,
                    polyline.Handle,
                    ReadCoordinates(polyline),
                    IsClosedPolyline(polyline),
                    layerOptions);
            }

            foreach (Line line in dxf.Entities.Lines)
            {
                AddDxfObject(
                    parcels,
                    line.Layer?.Name,
                    line.Handle,
                    GeometryFactory.CreateLineString(
                        [
                            new Coordinate(line.StartPoint.X, line.StartPoint.Y),
                            new Coordinate(line.EndPoint.X, line.EndPoint.Y)
                        ]),
                    "Line",
                    null,
                    layerOptions);
            }

            foreach (netDxf.Entities.Point point in dxf.Entities.Points)
            {
                AddDxfObject(
                    parcels,
                    point.Layer?.Name,
                    point.Handle,
                    GeometryFactory.CreatePoint(
                        new Coordinate(point.Position.X, point.Position.Y)),
                    "Point",
                    null,
                    layerOptions);
            }

            if (options.AutoAssignParcelRecords)
            {
                List<CadastralTextFeature> textFeatures = [];
                textFeatures.AddRange(dxf.Entities.Texts.Select(text => new CadastralTextFeature(
                    ExtractParcelText(text.Value),
                    text.Position.X,
                    text.Position.Y,
                    text.Layer?.Name ?? "0")));
                textFeatures.AddRange(dxf.Entities.MTexts.Select(text => new CadastralTextFeature(
                    ExtractParcelText(text.Value),
                    text.Position.X,
                    text.Position.Y,
                    text.Layer?.Name ?? "0")));
                AssignTextToParcels(parcels, textFeatures);
            }

            foreach (Text text in dxf.Entities.Texts)
            {
                AddDxfObject(
                    parcels,
                    text.Layer?.Name,
                    text.Handle,
                    GeometryFactory.CreatePoint(
                        new Coordinate(text.Position.X, text.Position.Y)),
                    "Text",
                    NormalizeText(text.Value),
                    layerOptions);
            }

            foreach (MText text in dxf.Entities.MTexts)
            {
                AddDxfObject(
                    parcels,
                    text.Layer?.Name,
                    text.Handle,
                    GeometryFactory.CreatePoint(
                        new Coordinate(text.Position.X, text.Position.Y)),
                    "Text",
                    NormalizeText(text.Value),
                    layerOptions);
            }

            return parcels;
        }

        private static CadastralFileInfo InspectOgr(string filePath, string format)
        {
            GdalBootstrapper.ConfigureAll();
            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            List<CadastralLayerInfo> layers = [];
            List<string> attributeFields = [];
            string? detectedCrs = null;

            for (int index = 0; index < dataSource.GetLayerCount(); index++)
            {
                using Layer layer = dataSource.GetLayerByIndex(index);
                string layerName = format == "SHP"
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : layer.GetName();

                int count = CountPolygonalFeatures(layer);
                layers.Add(new CadastralLayerInfo(layerName, count, 0, 0, 0, 0, count > 0));

                if (attributeFields.Count == 0)
                    attributeFields.AddRange(GetAttributeFields(layer));

                detectedCrs ??= GetLayerCrsDefinition(layer);
            }

            return new CadastralFileInfo(
                filePath,
                format,
                layers.Where(layer => layer.HasImportableObjects).ToList(),
                attributeFields,
                detectedCrs,
                RequiresCrsFromUser: string.IsNullOrWhiteSpace(detectedCrs),
                TextCount: 0);
        }

        private static List<CadastralRawParcel> ReadOgr(
            string filePath,
            CadastralImportOptions options,
            string format)
        {
            GdalBootstrapper.ConfigureAll();
            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            Dictionary<string, CadastralLayerImportOption> layerOptions = options.Layers
                .Where(option => option.Include)
                .ToDictionary(option => option.LayerName, StringComparer.OrdinalIgnoreCase);

            WKTReader reader = new();
            List<CadastralRawParcel> parcels = [];
            for (int index = 0; index < dataSource.GetLayerCount(); index++)
            {
                using Layer layer = dataSource.GetLayerByIndex(index);
                string layerName = format == "SHP"
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : layer.GetName();

                if (!layerOptions.TryGetValue(layerName, out CadastralLayerImportOption? layerOption))
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
                        NtsGeometry? valid = BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(reader.Read(wkt));
                        if (valid == null)
                            continue;

                        Dictionary<string, string?> attributes = ReadAttributes(feature);
                        string? parcelNo = TryGetAttribute(attributes, options.ShpParcelNumberField);
                        string? mapSheet = TryGetAttribute(attributes, options.ShpMapSheetField);

                        parcels.Add(new CadastralRawParcel(
                            valid,
                            "Polygon",
                            layerName,
                            layerOption.CanvasLayerName,
                            string.IsNullOrWhiteSpace(mapSheet) ? layerOption.MapSheetNo : mapSheet,
                            parcelNo,
                            null,
                            null,
                            attributes));
                    }
                }
            }

            return parcels;
        }

        private static void AddDxfPolyline(
            List<CadastralRawParcel> parcels,
            string? layerName,
            string? sourceHandle,
            List<Coordinate> coordinates,
            bool isClosed,
            IReadOnlyDictionary<string, CadastralLayerImportOption> layerOptions)
        {
            string normalizedLayer = string.IsNullOrWhiteSpace(layerName) ? "0" : layerName;
            if (!layerOptions.TryGetValue(normalizedLayer, out CadastralLayerImportOption? layerOption))
                return;

            NtsGeometry? geometry = isClosed
                ? CreatePolygon(coordinates)
                : CreateLineString(coordinates);
            if (geometry == null)
                return;

            parcels.Add(new CadastralRawParcel(
                geometry,
                isClosed ? "Polygon" : "Polyline",
                normalizedLayer,
                layerOption.CanvasLayerName,
                layerOption.MapSheetNo,
                null,
                null,
                sourceHandle,
                new Dictionary<string, string?>()));
        }

        private static void AddDxfObject(
            List<CadastralRawParcel> parcels,
            string? layerName,
            string? sourceHandle,
            NtsGeometry geometry,
            string objectType,
            string? labelText,
            IReadOnlyDictionary<string, CadastralLayerImportOption> layerOptions)
        {
            string normalizedLayer = string.IsNullOrWhiteSpace(layerName) ? "0" : layerName;
            if (!layerOptions.TryGetValue(normalizedLayer, out CadastralLayerImportOption? layerOption))
                return;

            parcels.Add(new CadastralRawParcel(
                geometry,
                objectType,
                normalizedLayer,
                layerOption.CanvasLayerName,
                layerOption.MapSheetNo,
                labelText,
                null,
                sourceHandle,
                new Dictionary<string, string?>()));
        }

        private static void AssignTextToParcels(
            List<CadastralRawParcel> parcels,
            IReadOnlyList<CadastralTextFeature> textFeatures)
        {
            if (parcels.Count == 0 || textFeatures.Count == 0)
                return;

            STRtree<int> index = new();
            for (int i = 0; i < parcels.Count; i++)
                index.Insert(parcels[i].Geometry.EnvelopeInternal, i);

            index.Build();
            foreach (CadastralTextFeature text in textFeatures)
            {
                if (string.IsNullOrWhiteSpace(text.Value))
                    continue;

                NetTopologySuite.Geometries.Point point =
                    GeometryFactory.CreatePoint(new Coordinate(text.X, text.Y));
                foreach (int parcelIndex in index.Query(point.EnvelopeInternal))
                {
                    CadastralRawParcel parcel = parcels[parcelIndex];
                    if (!string.Equals(parcel.ObjectType, "Polygon", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!string.IsNullOrWhiteSpace(parcel.ParcelNo))
                        continue;

                    if (parcel.Geometry.Covers(point) || parcel.Geometry.Buffer(0.01).Covers(point))
                    {
                        parcels[parcelIndex] = parcel with
                        {
                            ParcelNo = text.Value,
                            MatchedText = text.Value
                        };
                        break;
                    }
                }
            }
        }

        private static int CountPolygonalFeatures(Layer layer)
        {
            WKTReader reader = new();
            int count = 0;
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
                    if (BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(reader.Read(wkt)) != null)
                        count++;
                }
            }

            layer.ResetReading();
            return count;
        }

        private static List<string> GetAttributeFields(Layer layer)
        {
            List<string> fields = [];
            FeatureDefn definition = layer.GetLayerDefn();
            for (int index = 0; index < definition.GetFieldCount(); index++)
                fields.Add(definition.GetFieldDefn(index).GetName());

            return fields;
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

        private static string? TryGetAttribute(
            IReadOnlyDictionary<string, string?> attributes,
            string? fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return null;

            return attributes.TryGetValue(fieldName, out string? value)
                ? NormalizeText(value)
                : null;
        }

        private static string? ExtractParcelText(string? value)
        {
            value = NormalizeText(value);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            Match match = ParcelTextRegex.Match(value);
            return match.Success ? match.Value.Trim() : value;
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

        private static LayerStats GetStats(
            Dictionary<string, LayerStats> stats,
            string? layerName)
        {
            string key = string.IsNullOrWhiteSpace(layerName) ? "0" : layerName;
            if (!stats.TryGetValue(key, out LayerStats? layerStats))
            {
                layerStats = new LayerStats();
                stats[key] = layerStats;
            }

            return layerStats;
        }

        private static bool IsClosedPolyline(Polyline2D polyline)
        {
            return polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline));
        }

        private static bool IsClosedPolyline(Polyline3D polyline)
        {
            return polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline));
        }

        private static List<Coordinate> ReadCoordinates(Polyline2D polyline)
        {
            return polyline.PolygonalVertexes(24)
                .Select(vertex => new Coordinate(vertex.X, vertex.Y))
                .ToList();
        }

        private static List<Coordinate> ReadCoordinates(Polyline3D polyline)
        {
            return polyline.PolygonalVertexes(24)
                .Select(vertex => new Coordinate(vertex.X, vertex.Y))
                .ToList();
        }

        private static bool IsRingClosed(IReadOnlyList<Coordinate> coordinates)
        {
            return coordinates.Count >= 4 &&
                   coordinates[0].Distance(coordinates[^1]) <= 0.000001;
        }

        private static NtsGeometry? CreatePolygon(List<Coordinate> coordinates)
        {
            RemoveConsecutiveDuplicates(coordinates);
            if (coordinates.Count < 3)
                return null;

            LinearRing ring = GeometryFactory.CreateLinearRing(
                BoundaryGeometryReaderHelpers.CloseRing(coordinates));
            Polygon polygon = GeometryFactory.CreatePolygon(ring);
            return BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(polygon);
        }

        private static NtsGeometry? CreateLineString(List<Coordinate> coordinates)
        {
            RemoveConsecutiveDuplicates(coordinates);
            return coordinates.Count < 2
                ? null
                : GeometryFactory.CreateLineString(coordinates.ToArray());
        }

        private static void RemoveConsecutiveDuplicates(List<Coordinate> coordinates)
        {
            for (int index = coordinates.Count - 1; index > 0; index--)
            {
                if (coordinates[index].Distance(coordinates[index - 1]) <= 0.000001)
                    coordinates.RemoveAt(index);
            }
        }

        private static string? GetLayerCrsDefinition(Layer layer)
        {
            using SpatialReference spatialReference = layer.GetSpatialRef();
            if (spatialReference == null)
                return null;

            spatialReference.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
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

        private sealed class LayerStats
        {
            public int PolygonCount { get; set; }
            public int PolylineCount { get; set; }
            public int LineCount { get; set; }
            public int PointCount { get; set; }
            public int TextCount { get; set; }
            public int ObjectCount => PolygonCount + PolylineCount + LineCount + PointCount + TextCount;
        }

        private sealed record CadastralTextFeature(
            string? Value,
            double X,
            double Y,
            string LayerName);
    }

    public sealed record CadastralRawParcel(
        NtsGeometry Geometry,
        string ObjectType,
        string SourceLayer,
        string CanvasLayerName,
        string? MapSheetNo,
        string? ParcelNo,
        string? MatchedText,
        string? SourceHandle,
        Dictionary<string, string?> Attributes);
}
