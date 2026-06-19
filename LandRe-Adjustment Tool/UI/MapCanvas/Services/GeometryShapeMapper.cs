using System.Drawing;
using System.Reflection;
using System.Text.Json;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using NetTopologySuite.Geometries;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Maps between runtime canvas shapes and persisted NetTopologySuite geometries.
    /// </summary>
    public static class GeometryShapeMapper
    {
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);
        private static readonly JsonSerializerOptions MetadataJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static CanvasObject ToCanvasObject(
            IShape shape,
            int layerId,
            CanvasObject? target = null)
        {
            ArgumentNullException.ThrowIfNull(shape);

            CanvasObject canvasObject = target ?? new CanvasObject();

            canvasObject.Id = shape.Id;
            canvasObject.CanvasLayerId = layerId;
            canvasObject.ObjectType = ResolveObjectType(shape);
            canvasObject.Shape = ToGeometry(shape);
            canvasObject.GeometryMetadataJson = CreateGeometryMetadataJson(shape);
            canvasObject.IsVisible = shape.IsVisible;
            canvasObject.BorderColorOverride = ResolveColorOverride(shape, "BorderColorOverride", target?.BorderColorOverride);
            canvasObject.FillColorOverride = ResolveColorOverride(shape, "FillColorOverride", target?.FillColorOverride);
            canvasObject.LabelText = ResolveLabelText(shape);
            canvasObject.ObjectDescription = GetPropertyAsString(shape, "Description");

            if (shape.Properties.TryGetValue("LineWeight", out object? lineWeightObj) &&
                TryToDouble(lineWeightObj, out double lineWeight))
            {
                canvasObject.LineWeightOverride = lineWeight;
            }

            if (shape.Properties.TryGetValue("LineStyle", out object? lineStyleObj))
            {
                canvasObject.LineStyleOverride = lineStyleObj?.ToString();
            }

            if (shape.Properties.TryGetValue("FillTransparency", out object? fillTransparencyObj) &&
                TryToInt(fillTransparencyObj, out int fillTransparency))
            {
                canvasObject.FillTransparencyOverride = fillTransparency;
            }

            if (shape.Properties.TryGetValue("BaselineParcelId", out object? baselineIdObj) &&
                TryToInt(baselineIdObj, out int baselineParcelId))
            {
                canvasObject.BaselineParcelId = baselineParcelId;
            }

            if (shape.Properties.TryGetValue("ReplottedParcelId", out object? replottedIdObj) &&
                TryToInt(replottedIdObj, out int replottedParcelId))
            {
                canvasObject.ReplottedParcelId = replottedParcelId;
            }

            if (shape.Properties.TryGetValue("RoadId", out object? roadIdObj) &&
                TryToInt(roadIdObj, out int roadId))
            {
                canvasObject.RoadId = roadId;
            }

            if (shape.Properties.TryGetValue("BlockId", out object? blockIdObj) &&
                TryToInt(blockIdObj, out int blockId))
            {
                canvasObject.BlockId = blockId;
            }

            return canvasObject;
        }

        public static IShape ToShape(CanvasObject canvasObject)
        {
            ArgumentNullException.ThrowIfNull(canvasObject);
            ArgumentNullException.ThrowIfNull(canvasObject.Shape);

            IShape shape = CreateShapeFromGeometry(
                canvasObject.ObjectType,
                canvasObject.Shape,
                canvasObject.LabelText,
                canvasObject.GeometryMetadataJson,
                canvasObject.CanvasLayer?.LayerType);

            SetShapeId(shape, canvasObject.Id);
            shape.LayerName = canvasObject.CanvasLayer?.Name ?? shape.LayerName;
            shape.IsVisible = canvasObject.IsVisible;

            if (!string.IsNullOrWhiteSpace(canvasObject.BorderColorOverride))
            {
                shape.BorderColor = ParseColorOrDefault(canvasObject.BorderColorOverride, shape.BorderColor);
                shape.Properties["BorderColorOverride"] = canvasObject.BorderColorOverride;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.FillColorOverride))
            {
                shape.FillColor = ParseColorOrDefault(canvasObject.FillColorOverride, shape.FillColor);
                shape.Properties["FillColorOverride"] = canvasObject.FillColorOverride;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.LabelText))
            {
                shape.Properties["LabelText"] = canvasObject.LabelText;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.ObjectDescription))
            {
                shape.Properties["Description"] = canvasObject.ObjectDescription;
            }

            if (canvasObject.LineWeightOverride.HasValue)
            {
                shape.Properties["LineWeight"] = canvasObject.LineWeightOverride.Value;
            }

            if (!string.IsNullOrWhiteSpace(canvasObject.LineStyleOverride))
            {
                shape.Properties["LineStyle"] = canvasObject.LineStyleOverride;
            }

            if (canvasObject.FillTransparencyOverride.HasValue)
            {
                shape.Properties["FillTransparency"] = canvasObject.FillTransparencyOverride.Value;
            }

            if (canvasObject.BaselineParcelId.HasValue)
            {
                shape.Properties["BaselineParcelId"] = canvasObject.BaselineParcelId.Value;
            }

            if (canvasObject.ReplottedParcelId.HasValue)
            {
                shape.Properties["ReplottedParcelId"] = canvasObject.ReplottedParcelId.Value;
            }

            if (canvasObject.RoadId.HasValue)
            {
                shape.Properties["RoadId"] = canvasObject.RoadId.Value;
            }

            if (canvasObject.BlockId.HasValue)
            {
                shape.Properties["BlockId"] = canvasObject.BlockId.Value;
            }

            return shape;
        }

        private static Geometry ToGeometry(IShape shape)
        {
            return shape switch
            {
                LineShape line => GeometryFactory.CreateLineString(
                    [
                        new Coordinate(line.Start.X, line.Start.Y),
                        new Coordinate(line.End.X, line.End.Y)
                    ]),
                DonutPolygonShape donut => CreatePolygonFromDonut(donut),
                PolylineShape polyline => CreateGeometryFromPolyline(polyline),
                RectangleShape rectangle => CreatePolygonFromRectangle(rectangle),
                CircleShape circle => CreatePolygonFromCircle(circle),
                ArcShape arc => CreateLineStringFromArc(arc),
                EllipseShape ellipse => CreatePolygonFromEllipse(ellipse),
                TextShape text => GeometryFactory.CreatePoint(
                    new Coordinate(text.Position.X, text.Position.Y)),
                _ => CreatePolygonFromBoundingBox(shape.GetBoundingBox())
            };
        }

        private static Geometry CreateGeometryFromPolyline(PolylineShape polyline)
        {
            if (polyline.Vertices.Count == 0)
            {
                return GeometryFactory.CreateGeometryCollection();
            }

            if (polyline.Vertices.Count == 1)
            {
                PointD v = polyline.Vertices[0];
                return GeometryFactory.CreatePoint(new Coordinate(v.X, v.Y));
            }

            if (polyline.Segments.Count > 0)
            {
                List<Coordinate> sampledCoordinates = polyline.GetGeometryPoints(24)
                    .Select(v => new Coordinate(v.X, v.Y))
                    .ToList();

                if (polyline.IsClosed && sampledCoordinates.Count >= 3)
                {
                    return CreatePolygonFromRing(sampledCoordinates);
                }

                return GeometryFactory.CreateLineString(sampledCoordinates.ToArray());
            }

            List<Coordinate> coordinates = polyline.Vertices
                .Select(v => new Coordinate(v.X, v.Y))
                .ToList();

            if (polyline.IsClosed && coordinates.Count >= 3)
            {
                return CreatePolygonFromRing(coordinates);
            }

            return GeometryFactory.CreateLineString(coordinates.ToArray());
        }

        private static Polygon CreatePolygonFromRectangle(RectangleShape rectangle)
        {
            RectangleD bounds = rectangle.GetBoundingBox();
            List<Coordinate> ring =
            [
                new Coordinate(bounds.Left, bounds.Top),
                new Coordinate(bounds.Right, bounds.Top),
                new Coordinate(bounds.Right, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Top)
            ];

            return CreatePolygonFromRing(ring);
        }

        private static Polygon CreatePolygonFromDonut(DonutPolygonShape donut)
        {
            LinearRing shell = GeometryFactory.CreateLinearRing(
                NormalizeClosedRing(donut.ExteriorRing.Select(point => new Coordinate(point.X, point.Y))));
            LinearRing[] holes = donut.InteriorRings
                .Select(ring => NormalizeClosedRing(ring.Select(point => new Coordinate(point.X, point.Y))))
                .Where(ring => ring.Length >= 4)
                .Select(GeometryFactory.CreateLinearRing)
                .ToArray();

            return GeometryFactory.CreatePolygon(shell, holes);
        }

        private static Polygon CreatePolygonFromCircle(CircleShape circle)
        {
            double radius = circle.GetRadius();
            Coordinate center = new(circle.Center.X, circle.Center.Y);
            return (Polygon)GeometryFactory.CreatePoint(center).Buffer(radius, quadrantSegments: 24);
        }

        private static LineString CreateLineStringFromArc(ArcShape arc)
        {
            Coordinate[] coordinates = arc.SamplePoints(96)
                .Select(point => new Coordinate(point.X, point.Y))
                .ToArray();
            return GeometryFactory.CreateLineString(coordinates);
        }

        private static Polygon CreatePolygonFromEllipse(EllipseShape ellipse)
        {
            RectangleD bounds = ellipse.GetBoundingBox();
            double centerX = bounds.Left + (bounds.Width / 2.0);
            double centerY = bounds.Top + (bounds.Height / 2.0);
            double radiusX = bounds.Width / 2.0;
            double radiusY = bounds.Height / 2.0;

            const int segments = 72;
            List<Coordinate> ring = new(segments + 1);

            for (int i = 0; i < segments; i++)
            {
                double theta = 2.0 * Math.PI * i / segments;
                double x = centerX + radiusX * Math.Cos(theta);
                double y = centerY + radiusY * Math.Sin(theta);
                ring.Add(new Coordinate(x, y));
            }

            ring.Add(new Coordinate(ring[0].X, ring[0].Y));
            return CreatePolygonFromRing(ring);
        }

        private static Polygon CreatePolygonFromBoundingBox(RectangleD bounds)
        {
            List<Coordinate> ring =
            [
                new Coordinate(bounds.Left, bounds.Top),
                new Coordinate(bounds.Right, bounds.Top),
                new Coordinate(bounds.Right, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Bottom),
                new Coordinate(bounds.Left, bounds.Top)
            ];

            return CreatePolygonFromRing(ring);
        }

        private static IShape CreateShapeFromGeometry(
            string objectType,
            Geometry geometry,
            string? labelText,
            string? metadataJson,
            string? layerType)
        {
            string? curveMetadataJson = ExtractCurveMetadataJson(metadataJson) ?? metadataJson;

            if (!string.IsNullOrWhiteSpace(curveMetadataJson) &&
                TryCreatePolylineFromMetadata(curveMetadataJson, out PolylineShape? polylineFromMetadata))
            {
                EnsureClosedForPolygonObjectType(objectType, polylineFromMetadata!);
                return polylineFromMetadata;
            }

            if (objectType.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                if (TryCreateCircleFromMetadata(curveMetadataJson, out CircleShape? circle))
                {
                    return circle;
                }

                if (TryCreateCircleFromPolygon(geometry, out circle))
                {
                    return circle;
                }
            }

            if (objectType.Equals("Arc", StringComparison.OrdinalIgnoreCase) &&
                (TryCreateArcFromMetadata(curveMetadataJson, out ArcShape? arc) ||
                 TryCreateArcFromApproximatedGeometry(geometry, out arc)))
            {
                return arc;
            }

            if ((objectType.Equals("Polyline", StringComparison.OrdinalIgnoreCase) ||
                 objectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase)) &&
                TryCreatePolylineFromMetadata(curveMetadataJson, out PolylineShape? polyline))
            {
                EnsureClosedForPolygonObjectType(objectType, polyline!);
                return polyline;
            }

            Geometry simplified = ReduceGeometry(geometry);

            if (ShouldInferArcSegments(objectType, layerType) &&
                TryCreatePolylineFromApproximatedGeometry(objectType, simplified, out PolylineShape? inferredPolyline))
            {
                return inferredPolyline;
            }

            if (objectType.Equals("Text", StringComparison.OrdinalIgnoreCase) &&
                simplified is NtsPoint textPoint)
            {
                string textAlignment = ReadTextAlignment(metadataJson);
                return new TextShape(
                    new PointD(textPoint.X, textPoint.Y),
                    labelText ?? string.Empty,
                    horizontalAlignment: textAlignment);
            }

            return simplified switch
            {
                NtsPoint point => CreatePointShape(point),
                LineString line => CreateLineShape(objectType, line),
                Polygon polygon => CreatePolygonShape(polygon),
                _ => CreatePolygonFromEnvelopeShape(simplified.EnvelopeInternal)
            };
        }

        /// <summary>
        /// A canvas object whose persisted <c>ObjectType</c> is <c>Polygon</c> represents a closed
        /// area feature (block, parcel, road parcel, ...). When such an object is rebuilt from curve
        /// metadata that was copied from an open source polyline, the metadata's <c>IsClosed</c> flag
        /// can still be <c>false</c> — producing an open <see cref="PolylineShape"/> with zero area.
        /// That shape renders without a fill and only its boundary is hit-testable, so the feature is
        /// not area-selectable like other block/parcel objects. The persisted object type is the
        /// authoritative marker, so force the reconstructed shape closed to match it.
        /// </summary>
        private static void EnsureClosedForPolygonObjectType(string objectType, PolylineShape polyline)
        {
            if (!polyline.IsClosed &&
                polyline.Vertices.Count >= 3 &&
                objectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase))
            {
                polyline.IsClosed = true;
            }
        }

        private static IShape CreatePointShape(NtsPoint point)
        {
            PolylineShape shape = new([new PointD(point.X, point.Y)], isClosed: false);
            shape.Properties["ObjectType"] = "Point";
            return shape;
        }

        private static IShape CreateLineShape(string objectType, LineString line)
        {
            Coordinate[] coordinates = line.Coordinates;

            if (coordinates.Length < 2)
            {
                return CreatePointShape(GeometryFactory.CreatePoint(coordinates.FirstOrDefault() ?? new Coordinate(0, 0)));
            }

            if (objectType.Equals("Line", StringComparison.OrdinalIgnoreCase) && coordinates.Length == 2)
            {
                return new LineShape(
                    new PointD(coordinates[0].X, coordinates[0].Y),
                    new PointD(coordinates[1].X, coordinates[1].Y));
            }

            List<PointD> vertices = coordinates
                .Select(c => new PointD(c.X, c.Y))
                .ToList();

            return new PolylineShape(vertices, isClosed: false);
        }

        private static IShape CreatePolygonShape(Polygon polygon)
        {
            List<PointD> vertices = ToOpenPointRing(polygon.ExteriorRing.Coordinates);

            if (polygon.NumInteriorRings == 0)
            {
                return new PolylineShape(vertices, isClosed: true);
            }

            List<List<PointD>> holes = new();
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                holes.Add(ToOpenPointRing(polygon.GetInteriorRingN(i).Coordinates));
            }

            return new DonutPolygonShape(vertices, holes);
        }

        private static List<PointD> ToOpenPointRing(Coordinate[] coordinates)
        {
            List<PointD> vertices = coordinates
                .Select(c => new PointD(c.X, c.Y))
                .ToList();

            if (vertices.Count > 1 &&
                NearlyEqual(vertices[0].X, vertices[^1].X) &&
                NearlyEqual(vertices[0].Y, vertices[^1].Y))
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            return vertices;
        }

        private static IShape CreatePolygonFromEnvelopeShape(Envelope envelope)
        {
            return new RectangleShape(
                new PointD(envelope.MinX, envelope.MinY),
                new PointD(envelope.MaxX, envelope.MaxY));
        }

        private static Geometry ReduceGeometry(Geometry geometry)
        {
            return geometry switch
            {
                MultiPolygon multiPolygon when multiPolygon.NumGeometries > 0 => multiPolygon
                    .Geometries
                    .OfType<Polygon>()
                    .OrderByDescending(poly => poly.Area)
                    .First(),
                MultiLineString multiLineString when multiLineString.NumGeometries > 0 => (LineString)multiLineString.GetGeometryN(0),
                GeometryCollection collection when collection.NumGeometries > 0 => ReduceGeometry(collection.GetGeometryN(0)),
                _ => geometry
            };
        }

        private static void CloseRing(List<Coordinate> coordinates)
        {
            Coordinate first = coordinates[0];
            Coordinate last = coordinates[^1];
            if (first.Equals2D(last))
            {
                return;
            }

            if (NearlyEqual(first.X, last.X) && NearlyEqual(first.Y, last.Y))
            {
                coordinates[^1] = new Coordinate(first.X, first.Y);
            }
            else
            {
                coordinates.Add(new Coordinate(first.X, first.Y));
            }
        }

        private static Polygon CreatePolygonFromRing(IEnumerable<Coordinate> coordinates)
        {
            Coordinate[] ring = NormalizeClosedRing(coordinates);
            return GeometryFactory.CreatePolygon(GeometryFactory.CreateLinearRing(ring));
        }

        private static Coordinate[] NormalizeClosedRing(IEnumerable<Coordinate> coordinates)
        {
            List<Coordinate> ring = new();
            foreach (Coordinate coordinate in coordinates)
            {
                if (!IsFinite(coordinate.X) || !IsFinite(coordinate.Y))
                {
                    continue;
                }

                Coordinate copy = new(coordinate.X, coordinate.Y);
                if (ring.Count == 0 || !CoordinatesNearlyEqual(ring[^1], copy))
                {
                    ring.Add(copy);
                }
            }

            if (ring.Count < 3)
            {
                return [];
            }

            Coordinate first = ring[0];
            Coordinate last = ring[^1];
            if (first.Equals2D(last))
            {
                return ring.Count >= 4 ? ring.ToArray() : [];
            }

            if (CoordinatesNearlyEqual(first, last))
            {
                ring[^1] = new Coordinate(first.X, first.Y);
            }
            else
            {
                ring.Add(new Coordinate(first.X, first.Y));
            }

            return ring.Count >= 4 ? ring.ToArray() : [];
        }

        private static bool CoordinatesNearlyEqual(Coordinate first, Coordinate second)
        {
            return NearlyEqual(first.X, second.X) &&
                   NearlyEqual(first.Y, second.Y);
        }

        private static string ResolveObjectType(IShape shape)
        {
            if (shape.Properties.TryGetValue("ObjectType", out object? objectType) &&
                !string.IsNullOrWhiteSpace(objectType?.ToString()))
            {
                return objectType.ToString()!.Trim();
            }

            return shape switch
            {
                DonutPolygonShape => "Polygon",
                PolylineShape polyline when polyline.IsClosed => "Polygon",
                PolylineShape => "Polyline",
                LineShape => "Line",
                CircleShape => "Circle",
                RectangleShape => "Polygon",
                ArcShape => "Arc",
                EllipseShape => "Polygon",
                TextShape => "Text",
                _ => "Polyline"
            };
        }

        private static string? ResolveLabelText(IShape shape)
        {
            if (shape is TextShape textShape)
            {
                return textShape.Text;
            }

            if (shape.Properties.TryGetValue("LabelText", out object? value))
            {
                return value?.ToString();
            }

            return null;
        }

        private static string? GetPropertyAsString(IShape shape, string key)
        {
            if (shape.Properties.TryGetValue(key, out object? value))
            {
                return value?.ToString();
            }

            return null;
        }

        private static string? ColorToHtml(Color color)
        {
            return ColorTranslator.ToHtml(Color.FromArgb(255, color.R, color.G, color.B));
        }

        private static string? CreateGeometryMetadataJson(IShape shape)
        {
            try
            {
                return shape switch
                {
                    CircleShape circle => JsonSerializer.Serialize(new CurveMetadata(
                        "Circle",
                        circle.Center.X,
                        circle.Center.Y,
                        circle.RadiusPoint.X,
                        circle.RadiusPoint.Y,
                        circle.GetRadius(),
                        null,
                        null)),
                    ArcShape arc => JsonSerializer.Serialize(new CurveMetadata(
                        "Arc",
                        arc.Center.X,
                        arc.Center.Y,
                        null,
                        null,
                        arc.Radius,
                        arc.StartAngleRadians,
                        arc.SweepAngleRadians)),
                    PolylineShape polyline => CreatePolylineMetadataJson(polyline),
                    TextShape text => JsonSerializer.Serialize(new TextMetadata(
                        "Text",
                        TextShape.NormalizeHorizontalAlignment(
                            text.Properties.TryGetValue("TextAlignment", out object? alignment)
                                ? alignment?.ToString()
                                : text.HorizontalAlignment))),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        private static string? CreatePolylineMetadataJson(PolylineShape polyline)
        {
            if (polyline.Segments.Count == 0)
            {
                return null;
            }

            List<PolylineSegmentMetadata> segments = new();
            foreach (PolylineShape.PolylineSegment segment in polyline.Segments)
            {
                if (segment.Kind == PolylineShape.PolylineSegmentKind.Arc && segment.Arc != null)
                {
                    if (!IsFinite(segment.Arc.Center.X) ||
                        !IsFinite(segment.Arc.Center.Y) ||
                        !IsFinite(segment.Arc.Radius) ||
                        !IsFinite(segment.Arc.StartAngleRadians) ||
                        !IsFinite(segment.Arc.SweepAngleRadians) ||
                        segment.Arc.Radius <= 0.0)
                    {
                        segments.Add(new PolylineSegmentMetadata(
                            "Line",
                            segment.Start.X,
                            segment.Start.Y,
                            segment.End.X,
                            segment.End.Y,
                            null,
                            null,
                            null,
                            null,
                            null));
                        continue;
                    }

                    segments.Add(new PolylineSegmentMetadata(
                        "Arc",
                        segment.Start.X,
                        segment.Start.Y,
                        segment.End.X,
                        segment.End.Y,
                        segment.Arc.Center.X,
                        segment.Arc.Center.Y,
                        segment.Arc.Radius,
                        segment.Arc.StartAngleRadians,
                        segment.Arc.SweepAngleRadians));
                    continue;
                }

                segments.Add(new PolylineSegmentMetadata(
                    "Line",
                    segment.Start.X,
                    segment.Start.Y,
                    segment.End.X,
                    segment.End.Y,
                    null,
                    null,
                    null,
                    null,
                    null));
            }

            if (segments.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(new PolylineCurveMetadata(
                polyline.IsClosed ? "Polygon" : "Polyline",
                polyline.IsClosed,
                segments));
        }

        private static bool TryCreateCircleFromMetadata(
            string? metadataJson,
            out CircleShape? circle)
        {
            circle = null;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                CurveMetadata? metadata = JsonSerializer.Deserialize<CurveMetadata>(metadataJson);
                if (metadata == null ||
                    !string.Equals(metadata.ShapeType, "Circle", StringComparison.OrdinalIgnoreCase) ||
                    !metadata.Radius.HasValue ||
                    metadata.Radius.Value <= 0.0)
                {
                    return false;
                }

                PointD center = new(metadata.CenterX, metadata.CenterY);
                PointD radiusPoint = metadata.RadiusPointX.HasValue && metadata.RadiusPointY.HasValue
                    ? new PointD(metadata.RadiusPointX.Value, metadata.RadiusPointY.Value)
                    : new PointD(metadata.CenterX + metadata.Radius.Value, metadata.CenterY);
                circle = new CircleShape(center, radiusPoint);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string? ExtractCurveMetadataJson(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return null;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadataJson);
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return null;
                }

                if (TryGetJsonPropertyIgnoreCase(root, "curveMetadataJson", out JsonElement curveProperty) &&
                    curveProperty.ValueKind == JsonValueKind.String)
                {
                    string? curveJson = curveProperty.GetString();
                    return string.IsNullOrWhiteSpace(curveJson) ? null : curveJson;
                }

                if (TryGetJsonPropertyIgnoreCase(root, "curve", out JsonElement curveObject) &&
                    curveObject.ValueKind == JsonValueKind.Object)
                {
                    return curveObject.GetRawText();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static bool TryGetJsonPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static bool TryCreateArcFromMetadata(
            string? metadataJson,
            out ArcShape? arc)
        {
            arc = null;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                CurveMetadata? metadata = JsonSerializer.Deserialize<CurveMetadata>(metadataJson);
                if (metadata == null ||
                    !string.Equals(metadata.ShapeType, "Arc", StringComparison.OrdinalIgnoreCase) ||
                    !metadata.Radius.HasValue ||
                    !metadata.StartAngleRadians.HasValue ||
                    !metadata.SweepAngleRadians.HasValue ||
                    metadata.Radius.Value <= 0.0)
                {
                    return false;
                }

                arc = new ArcShape(
                    new PointD(metadata.CenterX, metadata.CenterY),
                    metadata.Radius.Value,
                    metadata.StartAngleRadians.Value,
                    metadata.SweepAngleRadians.Value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryCreateArcFromApproximatedGeometry(
            Geometry geometry,
            out ArcShape? arc)
        {
            arc = null;
            Geometry simplified = ReduceGeometry(geometry);
            if (simplified is not LineString line ||
                line.Coordinates.Length < 3)
            {
                return false;
            }

            Coordinate[] coordinates = line.Coordinates;
            Coordinate start = coordinates[0];
            Coordinate through = coordinates[coordinates.Length / 2];
            Coordinate end = coordinates[^1];

            arc = ArcShape.FromThreePoints(
                new PointD(start.X, start.Y),
                new PointD(through.X, through.Y),
                new PointD(end.X, end.Y));
            return arc != null;
        }

        private static bool TryCreatePolylineFromMetadata(
            string? metadataJson,
            out PolylineShape? polyline)
        {
            polyline = null;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                PolylineCurveMetadata? metadata = JsonSerializer.Deserialize<PolylineCurveMetadata>(
                    metadataJson,
                    MetadataJsonOptions);
                if (metadata == null ||
                    metadata.Segments == null ||
                    metadata.Segments.Count == 0 ||
                    (!string.Equals(metadata.ShapeType, "Polyline", StringComparison.OrdinalIgnoreCase) &&
                     !string.Equals(metadata.ShapeType, "Polygon", StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                List<PointD> vertices = new();
                List<PolylineShape.PolylineSegment> segments = new();

                foreach (PolylineSegmentMetadata segment in metadata.Segments)
                {
                    PointD start = new(segment.StartX, segment.StartY);
                    PointD end = new(segment.EndX, segment.EndY);

                    if (vertices.Count == 0 || !SameWorldPoint(vertices[^1], start))
                    {
                        vertices.Add(start);
                    }

                    PolylineShape.PolylineSegmentKind kind =
                        string.Equals(segment.Kind, "Arc", StringComparison.OrdinalIgnoreCase)
                            ? PolylineShape.PolylineSegmentKind.Arc
                            : PolylineShape.PolylineSegmentKind.Line;

                    ArcShape? arc = null;
                    if (kind == PolylineShape.PolylineSegmentKind.Arc &&
                        segment.CenterX.HasValue &&
                        segment.CenterY.HasValue &&
                        segment.Radius.HasValue &&
                        segment.StartAngleRadians.HasValue &&
                        segment.SweepAngleRadians.HasValue &&
                        segment.Radius.Value > 0.0)
                    {
                        arc = new ArcShape(
                            new PointD(segment.CenterX.Value, segment.CenterY.Value),
                            segment.Radius.Value,
                            segment.StartAngleRadians.Value,
                            segment.SweepAngleRadians.Value);
                    }

                    if (kind == PolylineShape.PolylineSegmentKind.Arc && arc == null)
                    {
                        kind = PolylineShape.PolylineSegmentKind.Line;
                    }

                    segments.Add(new PolylineShape.PolylineSegment(kind, start, end, arc));

                    if (!SameWorldPoint(vertices[^1], end))
                    {
                        vertices.Add(end);
                    }
                }

                bool isClosed = metadata.IsClosed ||
                                string.Equals(metadata.ShapeType, "Polygon", StringComparison.OrdinalIgnoreCase);
                polyline = new PolylineShape(vertices, segments, isClosed);
                return polyline.Vertices.Count >= 2;
            }
            catch
            {
                return false;
            }
        }

        private static bool ShouldInferArcSegments(string objectType, string? layerType)
        {
            if (objectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(layerType, "ProposedRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, "ExistingRoad", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, "Road", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layerType, CanvasLayerTreeService.RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryCreatePolylineFromApproximatedGeometry(
            string objectType,
            Geometry geometry,
            out PolylineShape? polyline)
        {
            polyline = null;
            bool isClosed;
            List<PointD> sourcePoints;

            switch (geometry)
            {
                case LineString lineString when
                    objectType.Equals("Polyline", StringComparison.OrdinalIgnoreCase) ||
                    objectType.Equals("Line", StringComparison.OrdinalIgnoreCase):
                    sourcePoints = lineString.Coordinates
                        .Select(coordinate => new PointD(coordinate.X, coordinate.Y))
                        .ToList();
                    isClosed = false;
                    break;

                case Polygon polygon when objectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase):
                    sourcePoints = ToOpenPointRing(polygon.ExteriorRing.Coordinates);
                    isClosed = true;
                    break;

                default:
                    return false;
            }

            RemoveDuplicatePoints(sourcePoints);
            if (sourcePoints.Count < 4)
            {
                return false;
            }

            RectangleD bounds = GetBounds(sourcePoints);
            double diagonal = Math.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height);
            double tolerance = Math.Max(0.02, diagonal * 0.00001);
            if (!TryInferPolylineSegments(sourcePoints, isClosed, tolerance, out List<PolylineShape.PolylineSegment> segments, out bool hasArc))
            {
                return false;
            }

            if (!hasArc)
            {
                return false;
            }

            List<PointD> logicalVertices = BuildLogicalVertices(segments);
            polyline = new PolylineShape(logicalVertices, segments, isClosed);
            return polyline.Vertices.Count >= 2;
        }

        private static bool TryInferPolylineSegments(
            IReadOnlyList<PointD> sourcePoints,
            bool isClosed,
            double tolerance,
            out List<PolylineShape.PolylineSegment> segments,
            out bool hasArc)
        {
            segments = [];
            hasArc = false;

            int lastLinearIndex = sourcePoints.Count - 1;
            int index = 0;
            while (index < lastLinearIndex)
            {
                if (TryFindArcRun(sourcePoints, index, tolerance, out int endIndex, out ArcShape? arc) &&
                    arc != null)
                {
                    PointD start = sourcePoints[index];
                    PointD end = sourcePoints[endIndex];
                    segments.Add(new PolylineShape.PolylineSegment(
                        PolylineShape.PolylineSegmentKind.Arc,
                        start,
                        end,
                        arc));
                    hasArc = true;
                    index = endIndex;
                    continue;
                }

                segments.Add(new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    sourcePoints[index],
                    sourcePoints[index + 1]));
                index++;
            }

            if (isClosed && sourcePoints.Count > 2 && !SameWorldPoint(sourcePoints[^1], sourcePoints[0]))
            {
                segments.Add(new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    sourcePoints[^1],
                    sourcePoints[0]));
            }

            return segments.Count > 0;
        }

        private static bool TryFindArcRun(
            IReadOnlyList<PointD> points,
            int startIndex,
            double tolerance,
            out int endIndex,
            out ArcShape? arc)
        {
            const int MinimumArcPointCount = 4;
            const int MaximumArcPointCount = 96;

            endIndex = -1;
            arc = null;

            int minEnd = startIndex + MinimumArcPointCount - 1;
            if (minEnd >= points.Count)
            {
                return false;
            }

            int maxEnd = Math.Min(points.Count - 1, startIndex + MaximumArcPointCount - 1);
            for (int candidateEnd = minEnd; candidateEnd <= maxEnd; candidateEnd++)
            {
                int middleIndex = startIndex + ((candidateEnd - startIndex) / 2);
                ArcShape? candidate = ArcShape.FromThreePoints(
                    points[startIndex],
                    points[middleIndex],
                    points[candidateEnd]);
                if (candidate == null ||
                    !IsValidInferredArc(candidate, points, startIndex, candidateEnd, tolerance))
                {
                    continue;
                }

                endIndex = candidateEnd;
                arc = candidate;
            }

            return arc != null && endIndex > startIndex;
        }

        private static bool IsValidInferredArc(
            ArcShape arc,
            IReadOnlyList<PointD> points,
            int startIndex,
            int endIndex,
            double tolerance)
        {
            if (!IsFinite(arc.Center.X) ||
                !IsFinite(arc.Center.Y) ||
                !IsFinite(arc.Radius) ||
                arc.Radius <= tolerance ||
                Math.Abs(arc.SweepAngleRadians) < DegreesToRadians(5.0))
            {
                return false;
            }

            double chordLength = Distance(points[startIndex], points[endIndex]);
            if (chordLength <= tolerance)
            {
                return false;
            }

            double sagitta = MaxDistanceFromLine(points, startIndex, endIndex);
            if (sagitta < Math.Max(tolerance * 2.0, chordLength * 0.001))
            {
                return false;
            }

            double previousFraction = -0.01;
            for (int index = startIndex; index <= endIndex; index++)
            {
                PointD point = points[index];
                double radialError = Math.Abs(Distance(point, arc.Center) - arc.Radius);
                if (radialError > tolerance)
                {
                    return false;
                }

                double angle = Math.Atan2(point.Y - arc.Center.Y, point.X - arc.Center.X);
                if (!ArcShape.AngleLiesOnSweepPublic(angle, arc.StartAngleRadians, arc.SweepAngleRadians))
                {
                    return false;
                }

                double fraction = ArcSweepFraction(angle, arc.StartAngleRadians, arc.SweepAngleRadians);
                if (fraction < previousFraction - 0.02)
                {
                    return false;
                }

                previousFraction = fraction;
            }

            return true;
        }

        private static double ArcSweepFraction(
            double angle,
            double startAngle,
            double sweepAngle)
        {
            double delta = sweepAngle >= 0.0
                ? NormalizePositiveRadians(angle - startAngle)
                : -NormalizePositiveRadians(startAngle - angle);
            return Math.Abs(sweepAngle) <= 1e-12
                ? 0.0
                : delta / sweepAngle;
        }

        private static List<PointD> BuildLogicalVertices(IReadOnlyList<PolylineShape.PolylineSegment> segments)
        {
            List<PointD> vertices = [];
            foreach (PolylineShape.PolylineSegment segment in segments)
            {
                if (vertices.Count == 0 || !SameWorldPoint(vertices[^1], segment.Start))
                {
                    vertices.Add(segment.Start);
                }

                if (!SameWorldPoint(vertices[^1], segment.End))
                {
                    vertices.Add(segment.End);
                }
            }

            if (vertices.Count > 1 && SameWorldPoint(vertices[0], vertices[^1]))
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            return vertices;
        }

        private static void RemoveDuplicatePoints(List<PointD> points)
        {
            for (int index = points.Count - 1; index > 0; index--)
            {
                if (SameWorldPoint(points[index], points[index - 1]))
                {
                    points.RemoveAt(index);
                }
            }
        }

        private static RectangleD GetBounds(IReadOnlyList<PointD> points)
        {
            double minX = points[0].X;
            double minY = points[0].Y;
            double maxX = points[0].X;
            double maxY = points[0].Y;

            foreach (PointD point in points.Skip(1))
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
        }

        private static double MaxDistanceFromLine(
            IReadOnlyList<PointD> points,
            int startIndex,
            int endIndex)
        {
            double maxDistance = 0.0;
            PointD start = points[startIndex];
            PointD end = points[endIndex];
            for (int index = startIndex + 1; index < endIndex; index++)
            {
                maxDistance = Math.Max(maxDistance, PointToLineDistance(points[index], start, end));
            }

            return maxDistance;
        }

        private static double PointToLineDistance(PointD point, PointD lineStart, PointD lineEnd)
        {
            double dx = lineEnd.X - lineStart.X;
            double dy = lineEnd.Y - lineStart.Y;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared <= 1e-12)
            {
                return Distance(point, lineStart);
            }

            double numerator = Math.Abs(dy * point.X - dx * point.Y + lineEnd.X * lineStart.Y - lineEnd.Y * lineStart.X);
            return numerator / Math.Sqrt(lengthSquared);
        }

        private static bool TryCreateCircleFromPolygon(
            Geometry geometry,
            out CircleShape? circle)
        {
            circle = null;
            Geometry simplified = ReduceGeometry(geometry);
            if (simplified is not Polygon polygon)
            {
                return false;
            }

            Envelope envelope = polygon.EnvelopeInternal;
            double width = envelope.Width;
            double height = envelope.Height;
            if (width <= 0.0 ||
                height <= 0.0 ||
                Math.Abs(width - height) > Math.Max(width, height) * 0.02)
            {
                return false;
            }

            double radius = (width + height) / 4.0;
            PointD center = new(
                (envelope.MinX + envelope.MaxX) / 2.0,
                (envelope.MinY + envelope.MaxY) / 2.0);
            circle = new CircleShape(center, new PointD(center.X + radius, center.Y));
            return true;
        }

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
            List<PolylineSegmentMetadata> Segments);

        private sealed record TextMetadata(
            string ShapeType,
            string Alignment);

        private sealed record PolylineSegmentMetadata(
            string Kind,
            double StartX,
            double StartY,
            double EndX,
            double EndY,
            double? CenterX,
            double? CenterY,
            double? Radius,
            double? StartAngleRadians,
            double? SweepAngleRadians);

        private static string? ResolveColorOverride(
            IShape shape,
            string propertyKey,
            string? existingOverride)
        {
            if (!shape.Properties.TryGetValue(propertyKey, out object? value))
            {
                return existingOverride;
            }

            if (value is Color color)
            {
                return ColorToHtml(color);
            }

            string? html = value?.ToString();
            return string.IsNullOrWhiteSpace(html)
                ? null
                : html;
        }

        private static Color ParseColorOrDefault(string html, Color fallback)
        {
            try
            {
                return ColorTranslator.FromHtml(html);
            }
            catch
            {
                return fallback;
            }
        }

        private static void SetShapeId(IShape shape, Guid id)
        {
            PropertyInfo? idProperty = shape
                .GetType()
                .GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (idProperty == null)
            {
                return;
            }

            MethodInfo? setter = idProperty.GetSetMethod(nonPublic: true);
            setter?.Invoke(shape, [id]);
        }

        private static bool TryToDouble(object? value, out double result)
        {
            result = 0;
            return value != null && double.TryParse(value.ToString(), out result);
        }

        private static bool TryToInt(object? value, out int result)
        {
            result = 0;
            return value != null && int.TryParse(value.ToString(), out result);
        }

        private static string ReadTextAlignment(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return "Left";
            }

            try
            {
                TextMetadata? metadata = JsonSerializer.Deserialize<TextMetadata>(
                    metadataJson,
                    MetadataJsonOptions);
                if (metadata == null ||
                    !string.Equals(metadata.ShapeType, "Text", StringComparison.OrdinalIgnoreCase))
                {
                    return "Left";
                }

                return TextShape.NormalizeHorizontalAlignment(metadata.Alignment);
            }
            catch
            {
                return "Left";
            }
        }

        private static bool NearlyEqual(double a, double b)
        {
            return Math.Abs(a - b) <= 0.0000001;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static bool SameWorldPoint(PointD first, PointD second)
        {
            return NearlyEqual(first.X, second.X) &&
                   NearlyEqual(first.Y, second.Y);
        }

        private static double Distance(PointD first, PointD second)
        {
            double dx = second.X - first.X;
            double dy = second.Y - first.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double NormalizePositiveRadians(double angle)
        {
            double fullCircle = Math.PI * 2.0;
            angle %= fullCircle;
            return angle < 0.0 ? angle + fullCircle : angle;
        }
    }
}
