using System.Text.RegularExpressions;
using ACadSharp;
using ACadSharp.IO;
using Land_Readjustment_Tool.Core.Models.Import;
using netDxf;
using netDxf.Entities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using OSGeo.OGR;
using OSGeo.OSR;
using CSMath;
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
                ".dwg" => InspectDwg(filePath),
                ".shp" => InspectOgr(filePath, "SHP"),
                ".kml" => InspectOgr(filePath, "KML"),
                ".kmz" => InspectOgr(filePath, "KML"),
                _ => throw new NotSupportedException($"Cadastral map format not supported: {extension}")
            };
        }

        public List<CadastralRawParcel> Read(string filePath, CadastralImportOptions options)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            List<CadastralRawParcel> parcels = extension switch
            {
                ".dxf" => ReadDxf(filePath, options),
                ".dwg" => ReadDwg(filePath, options),
                ".shp" => ReadOgr(filePath, options, "SHP"),
                ".kml" => ReadOgr(filePath, options, "KML"),
                ".kmz" => ReadOgr(filePath, options, "KML"),
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

            foreach (Arc arc in dxf.Entities.Arcs)
            {
                GetStats(stats, arc.Layer?.Name).PolylineCount++;
            }

            foreach (Circle circle in dxf.Entities.Circles)
            {
                GetStats(stats, circle.Layer?.Name).PolylineCount++;
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
                new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase),
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

        private static CadastralFileInfo InspectDwg(string filePath)
        {
            Exception? managedReaderException = null;
            try
            {
                CadDocument document = DwgReader.Read(filePath, OnCadReaderNotification);
                Dictionary<string, LayerStats> stats = BuildCadLayerStats(document);

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
                    "DWG",
                    layers,
                    [],
                    new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase),
                    null,
                    RequiresCrsFromUser: true,
                    stats.Values.Sum(item => item.TextCount));
            }
            catch (Exception ex)
            {
                managedReaderException = ex;
            }

            try
            {
                return InspectOgr(filePath, "DWG");
            }
            catch (Exception ogrException)
            {
                throw CreateDwgReadException(filePath, managedReaderException, ogrException);
            }
        }

        private static List<CadastralRawParcel> ReadDwg(
            string filePath,
            CadastralImportOptions options)
        {
            Exception? managedReaderException = null;
            try
            {
                CadDocument document = DwgReader.Read(filePath, OnCadReaderNotification);
                return ReadCadDocument(document, options);
            }
            catch (Exception ex)
            {
                managedReaderException = ex;
            }

            try
            {
                return ReadOgr(filePath, options, "DWG");
            }
            catch (Exception ogrException)
            {
                throw CreateDwgReadException(filePath, managedReaderException, ogrException);
            }
        }

        private static InvalidOperationException CreateDwgReadException(
            string filePath,
            Exception? managedReaderException,
            Exception ogrException)
        {
            string message =
                $"Could not read AutoCAD DWG file '{Path.GetFileName(filePath)}'. " +
                "The importer supports common DWG versions from AutoCAD R14/R2000 through DWG 2018/2021/2024 format families when the file is not encrypted or proxy-only. " +
                "If this drawing still fails, open it in AutoCAD/BricsCAD/DWG TrueView and save a clean DXF or DWG copy, then import that copy.";

            return new InvalidOperationException(
                $"{message}{Environment.NewLine}{Environment.NewLine}" +
                $"Managed DWG reader: {managedReaderException?.Message ?? "not attempted"}{Environment.NewLine}" +
                $"GDAL DWG reader: {ogrException.Message}",
                ogrException);
        }

        private static Dictionary<string, LayerStats> BuildCadLayerStats(CadDocument document)
        {
            Dictionary<string, LayerStats> stats = new(StringComparer.OrdinalIgnoreCase);
            foreach (AcadEntity entity in EnumerateCadEntities(document.Entities))
            {
                LayerStats layerStats = GetStats(stats, entity.Layer?.Name);
                switch (entity)
                {
                    case AcadLwPolyline polyline:
                        if (polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline)))
                            layerStats.PolygonCount++;
                        else
                            layerStats.PolylineCount++;
                        break;
                    case AcadPolyline2D polyline:
                        if (polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline)))
                            layerStats.PolygonCount++;
                        else
                            layerStats.PolylineCount++;
                        break;
                    case AcadPolyline3D polyline:
                        if (polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline)))
                            layerStats.PolygonCount++;
                        else
                            layerStats.PolylineCount++;
                        break;
                    case AcadArc:
                    case AcadCircle:
                        layerStats.PolylineCount++;
                        break;
                    case AcadLine:
                        layerStats.LineCount++;
                        break;
                    case AcadPoint:
                        layerStats.PointCount++;
                        break;
                    case AcadText:
                    case AcadMText:
                        layerStats.TextCount++;
                        break;
                }
            }

            return stats;
        }

        private static List<CadastralRawParcel> ReadCadDocument(
            CadDocument document,
            CadastralImportOptions options)
        {
            Dictionary<string, CadastralLayerImportOption> layerOptions = options.Layers
                .Where(option => option.Include)
                .ToDictionary(option => option.LayerName, StringComparer.OrdinalIgnoreCase);

            List<CadastralRawParcel> parcels = [];
            List<CadastralTextFeature> textFeatures = [];
            foreach (AcadEntity entity in EnumerateCadEntities(document.Entities))
            {
                string? layerName = entity.Layer?.Name;
                string handle = entity.Handle.ToString("X");

                switch (entity)
                {
                    case AcadLwPolyline polyline:
                        AddDxfPolyline(
                            parcels,
                            layerName,
                            handle,
                            ReadCoordinates(polyline),
                            polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline)),
                            layerOptions);
                        break;
                    case AcadPolyline2D polyline:
                        AddDxfPolyline(
                            parcels,
                            layerName,
                            handle,
                            ReadCoordinates(polyline),
                            polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline)),
                            layerOptions);
                        break;
                    case AcadPolyline3D polyline:
                        AddDxfPolyline(
                            parcels,
                            layerName,
                            handle,
                            ReadCoordinates(polyline),
                            polyline.IsClosed || IsRingClosed(ReadCoordinates(polyline)),
                            layerOptions);
                        break;
                    case AcadCircle circle:
                        AddDxfPolyline(
                            parcels,
                            layerName,
                            handle,
                            ReadCoordinates(circle),
                            isClosed: true,
                            layerOptions);
                        break;
                    case AcadLine line:
                        AddDxfObject(
                            parcels,
                            layerName,
                            handle,
                            GeometryFactory.CreateLineString(
                                [
                                    new Coordinate(line.StartPoint.X, line.StartPoint.Y),
                                    new Coordinate(line.EndPoint.X, line.EndPoint.Y)
                                ]),
                            "Line",
                            null,
                            layerOptions);
                        break;
                    case AcadPoint point:
                        AddDxfObject(
                            parcels,
                            layerName,
                            handle,
                            GeometryFactory.CreatePoint(
                                new Coordinate(point.Location.X, point.Location.Y)),
                            "Point",
                            null,
                            layerOptions);
                        break;
                    case AcadText text:
                        string? textValue = NormalizeText(text.Value);
                        AddCadTextFeature(textFeatures, textValue, text.InsertPoint.X, text.InsertPoint.Y, layerName);
                        AddDxfObject(
                            parcels,
                            layerName,
                            handle,
                            GeometryFactory.CreatePoint(
                                new Coordinate(text.InsertPoint.X, text.InsertPoint.Y)),
                            "Text",
                            textValue,
                            layerOptions);
                        break;
                    case AcadMText text:
                        string? mTextValue = NormalizeText(text.PlainText ?? text.Value);
                        AddCadTextFeature(textFeatures, mTextValue, text.InsertPoint.X, text.InsertPoint.Y, layerName);
                        AddDxfObject(
                            parcels,
                            layerName,
                            handle,
                            GeometryFactory.CreatePoint(
                                new Coordinate(text.InsertPoint.X, text.InsertPoint.Y)),
                            "Text",
                            mTextValue,
                            layerOptions);
                        break;
                }
            }

            if (options.AutoAssignParcelRecords)
                AssignTextToParcels(parcels, textFeatures);

            return parcels;
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

        private static void AddCadTextFeature(
            List<CadastralTextFeature> textFeatures,
            string? value,
            double x,
            double y,
            string? layerName)
        {
            textFeatures.Add(new CadastralTextFeature(
                ExtractParcelText(value),
                x,
                y,
                string.IsNullOrWhiteSpace(layerName) ? "0" : layerName));
        }

        private static void OnCadReaderNotification(object sender, NotificationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"CAD reader: {e.Message}");
        }

        private static CadastralFileInfo InspectOgr(string filePath, string format)
        {
            GdalBootstrapper.ConfigureAll();
            using DataSource dataSource = Ogr.Open(filePath, 0)
                ?? throw new InvalidOperationException($"Could not open vector file: {filePath}");

            List<CadastralLayerInfo> layers = [];
            List<string> attributeFields = [];
            Dictionary<string, SortedSet<string>> attributeValues = new(StringComparer.OrdinalIgnoreCase);
            string? detectedCrs = null;

            for (int index = 0; index < dataSource.GetLayerCount(); index++)
            {
                using Layer layer = dataSource.GetLayerByIndex(index);
                string layerName = format == "SHP"
                    ? Path.GetFileNameWithoutExtension(filePath)
                    : layer.GetName();

                int count = CountPolygonalFeatures(layer);
                layers.Add(new CadastralLayerInfo(layerName, count, 0, 0, 0, 0, count > 0));

                List<string> layerFields = GetAttributeFields(layer);
                if (attributeFields.Count == 0)
                {
                    attributeFields.AddRange(layerFields);
                    foreach (string field in attributeFields)
                        attributeValues.TryAdd(field, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
                }
                else
                {
                    foreach (string field in layerFields.Where(field =>
                                 !attributeFields.Contains(field, StringComparer.OrdinalIgnoreCase)))
                    {
                        attributeFields.Add(field);
                        attributeValues.TryAdd(field, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
                    }
                }

                CollectAttributeValues(layer, attributeValues);

                detectedCrs ??= GetLayerCrsDefinition(layer);
            }

            Dictionary<string, IReadOnlyList<string>> uniqueValues = attributeValues
                .ToDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyList<string>)pair.Value.ToList(),
                    StringComparer.OrdinalIgnoreCase);

            return new CadastralFileInfo(
                filePath,
                format,
                layers.Where(layer => layer.HasImportableObjects).ToList(),
                attributeFields,
                uniqueValues,
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
                        string? sourceMapSheet = TryGetAttribute(attributes, options.ShpMapSheetField);
                        string? targetMapSheet = ResolveMappedMapSheet(sourceMapSheet, options.AttributeMapSheetValueMappings)
                                                 ?? layerOption.MapSheetNo;

                        parcels.Add(new CadastralRawParcel(
                            valid,
                            "Polygon",
                            layerName,
                            layerOption.CanvasLayerName,
                            targetMapSheet,
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

        private static void CollectAttributeValues(
            Layer layer,
            Dictionary<string, SortedSet<string>> attributeValues)
        {
            if (attributeValues.Count == 0)
                return;

            layer.ResetReading();
            Feature? feature;
            while ((feature = layer.GetNextFeature()) != null)
            {
                using (feature)
                {
                    FeatureDefn definition = feature.GetDefnRef();
                    for (int index = 0; index < definition.GetFieldCount(); index++)
                    {
                        string name = definition.GetFieldDefn(index).GetName();
                        if (!attributeValues.TryGetValue(name, out SortedSet<string>? values) ||
                            !feature.IsFieldSet(index))
                        {
                            continue;
                        }

                        string? value = NormalizeText(feature.GetFieldAsString(index));
                        if (!string.IsNullOrWhiteSpace(value))
                            values.Add(value);
                    }
                }
            }

            layer.ResetReading();
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

        private static string? ResolveMappedMapSheet(
            string? sourceMapSheet,
            IReadOnlyDictionary<string, string> mappings)
        {
            if (string.IsNullOrWhiteSpace(sourceMapSheet))
                return null;

            string normalized = NormalizeMapSheetValue(sourceMapSheet);
            if (mappings.TryGetValue(normalized, out string? target))
                return target;

            return mappings.TryGetValue(sourceMapSheet.Trim(), out target)
                ? target
                : null;
        }

        private static string NormalizeMapSheetValue(string value)
        {
            return Regex.Replace(value.Trim(), @"\s+", string.Empty).ToUpperInvariant();
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

        private static List<Coordinate> ReadCoordinates(AcadLwPolyline polyline)
        {
            return polyline.Vertices
                .Select(vertex => new Coordinate(vertex.Location.X, vertex.Location.Y))
                .ToList();
        }

        private static List<Coordinate> ReadCoordinates(AcadPolyline2D polyline)
        {
            return polyline.Vertices
                .Select(vertex => ToCoordinate(vertex.Location))
                .ToList();
        }

        private static List<Coordinate> ReadCoordinates(AcadPolyline3D polyline)
        {
            return polyline.Vertices
                .Select(vertex => ToCoordinate(vertex.Location))
                .ToList();
        }

        private static List<Coordinate> ReadCoordinates(AcadCircle circle)
        {
            return circle.PolygonalVertexes(72)
                .Select(vertex => new Coordinate(vertex.X, vertex.Y))
                .ToList();
        }

        private static Coordinate ToCoordinate(IVector vector)
        {
            return new Coordinate(vector[0], vector[1]);
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
