using Land_Readjustment_Tool.Core.Entities.Canvas;
using NetTopologySuite.Geometries;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Provides geometry measurements for canvas objects without leaking
    /// UI-specific formatting into geometry calculations.
    /// </summary>
    public static class CanvasGeometryMetricsService
    {
        public const string BlockDepthFromGeometryMetadataKey = "BlockDepthFromGeometry";

        private static readonly JsonSerializerOptions MetadataJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static double? GetArea(CanvasObject canvasObject)
        {
            if (canvasObject.Shape == null || canvasObject.Shape.IsEmpty)
                return null;

            double area = Math.Abs(canvasObject.Shape.Area);
            return area > 0 ? area : null;
        }

        public static double? GetLength(CanvasObject canvasObject)
        {
            if (canvasObject.Shape == null || canvasObject.Shape.IsEmpty)
                return null;

            double length = Math.Abs(canvasObject.Shape.Length);
            return length > 0 ? length : null;
        }

        public static int? GetVertexCount(CanvasObject canvasObject)
        {
            if (TryGetSemanticVertexCount(canvasObject, out int semanticVertexCount))
            {
                return semanticVertexCount > 0 ? semanticVertexCount : null;
            }

            if (canvasObject.Shape == null || canvasObject.Shape.IsEmpty)
                return null;

            int vertexCount = canvasObject.Shape switch
            {
                Polygon polygon when IsPolygonObject(canvasObject) => CountPolygonVertices(polygon),
                MultiPolygon multiPolygon when IsPolygonObject(canvasObject) => multiPolygon.Geometries
                    .OfType<Polygon>()
                    .Sum(CountPolygonVertices),
                LineString lineString when IsPolygonObject(canvasObject) => CountRingVertices(lineString),
                LineString lineString when IsPolylineObject(canvasObject) => lineString.NumPoints,
                _ => 0
            };

            return vertexCount > 0 ? vertexCount : null;
        }

        public static double? GetBlockDepthFromGeometry(CanvasObject canvasObject)
        {
            if (TryGetBlockDepthFromMetadata(canvasObject.GeometryMetadataJson, out double metadataDepth))
            {
                return metadataDepth;
            }

            return CalculateBlockDepthFromGeometry(canvasObject.Shape);
        }

        public static void StoreBlockDepthFromGeometry(CanvasObject canvasObject)
        {
            double? depth = CalculateBlockDepthFromGeometry(canvasObject.Shape);
            canvasObject.GeometryMetadataJson = UpdateMetadataNumber(
                canvasObject.GeometryMetadataJson,
                BlockDepthFromGeometryMetadataKey,
                depth);
        }

        public static bool IsUsableEnvelope(Envelope? envelope)
        {
            return envelope != null &&
                   !envelope.IsNull &&
                   double.IsFinite(envelope.MinX) &&
                   double.IsFinite(envelope.MinY) &&
                   double.IsFinite(envelope.MaxX) &&
                   double.IsFinite(envelope.MaxY) &&
                   envelope.MaxX >= envelope.MinX &&
                   envelope.MaxY >= envelope.MinY;
        }

        private static int CountPolygonVertices(Polygon polygon)
        {
            int vertexCount = CountRingVertices(polygon.ExteriorRing);
            for (int index = 0; index < polygon.NumInteriorRings; index++)
            {
                vertexCount += CountRingVertices(polygon.GetInteriorRingN(index));
            }

            return vertexCount;
        }

        private static int CountRingVertices(LineString ring)
        {
            int count = ring.NumPoints;
            if (count <= 1)
                return count;

            Coordinate first = ring.GetCoordinateN(0);
            Coordinate last = ring.GetCoordinateN(count - 1);
            return first.Equals2D(last) ? count - 1 : count;
        }

        private static bool TryGetSemanticVertexCount(CanvasObject canvasObject, out int vertexCount)
        {
            vertexCount = 0;

            if (TryGetMetadataVertexCount(canvasObject.GeometryMetadataJson, out vertexCount))
            {
                return true;
            }

            if (canvasObject.ObjectType.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                vertexCount = 0;
                return true;
            }

            if (canvasObject.ObjectType.Equals("Arc", StringComparison.OrdinalIgnoreCase))
            {
                vertexCount = 2;
                return true;
            }

            if (canvasObject.ObjectType.Equals("Line", StringComparison.OrdinalIgnoreCase))
            {
                vertexCount = 2;
                return true;
            }

            return false;
        }

        private static bool TryGetMetadataVertexCount(string? metadataJson, out int vertexCount)
        {
            vertexCount = 0;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadataJson);
                if (!document.RootElement.TryGetProperty("ShapeType", out JsonElement shapeTypeElement))
                {
                    return false;
                }

                string? shapeType = shapeTypeElement.GetString();
                if (string.IsNullOrWhiteSpace(shapeType))
                {
                    return false;
                }

                if (shapeType.Equals("Circle", StringComparison.OrdinalIgnoreCase))
                {
                    vertexCount = 0;
                    return true;
                }

                if (shapeType.Equals("Arc", StringComparison.OrdinalIgnoreCase))
                {
                    vertexCount = 2;
                    return true;
                }

                if (shapeType.Equals("Line", StringComparison.OrdinalIgnoreCase))
                {
                    vertexCount = 2;
                    return true;
                }

                if (shapeType.Equals("Polyline", StringComparison.OrdinalIgnoreCase) ||
                    shapeType.Equals("Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    PolylineCurveMetadata? metadata = document.RootElement.Deserialize<PolylineCurveMetadata>(
                        MetadataJsonOptions);
                    if (metadata?.Segments == null || metadata.Segments.Count == 0)
                    {
                        return false;
                    }

                    vertexCount = CountPolylineMetadataVertices(metadata);
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static int CountPolylineMetadataVertices(PolylineCurveMetadata metadata)
        {
            List<Coordinate> vertices = new();
            foreach (PolylineSegmentMetadata segment in metadata.Segments)
            {
                Coordinate start = new(segment.StartX, segment.StartY);
                Coordinate end = new(segment.EndX, segment.EndY);

                if (vertices.Count == 0 || !SameCoordinate(vertices[^1], start))
                {
                    vertices.Add(start);
                }

                if (!SameCoordinate(vertices[^1], end))
                {
                    vertices.Add(end);
                }
            }

            if (vertices.Count > 1 &&
                metadata.ShapeType.Equals("Polygon", StringComparison.OrdinalIgnoreCase) &&
                SameCoordinate(vertices[0], vertices[^1]))
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            return vertices.Count;
        }

        private static bool TryGetBlockDepthFromMetadata(string? metadataJson, out double depth)
        {
            depth = 0.0;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return false;
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadataJson);
                if (!TryGetJsonPropertyIgnoreCase(
                        document.RootElement,
                        BlockDepthFromGeometryMetadataKey,
                        out JsonElement value))
                {
                    return false;
                }

                if (value.ValueKind == JsonValueKind.Number &&
                    value.TryGetDouble(out depth) &&
                    depth > 0.0)
                {
                    return true;
                }

                if (value.ValueKind == JsonValueKind.String &&
                    double.TryParse(value.GetString(), out depth) &&
                    depth > 0.0)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static string? UpdateMetadataNumber(
            string? metadataJson,
            string propertyName,
            double? value)
        {
            JsonObject root;
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                root = [];
            }
            else
            {
                try
                {
                    root = JsonNode.Parse(metadataJson) as JsonObject ?? [];
                }
                catch
                {
                    root = [];
                }
            }

            RemoveJsonPropertyIgnoreCase(root, propertyName);
            RemoveJsonPropertyIgnoreCase(root, ToCamelCase(propertyName));

            if (value.HasValue && value.Value > 0.0 && double.IsFinite(value.Value))
            {
                root[propertyName] = Math.Round(value.Value, 6);
            }

            return root.Count == 0
                ? null
                : root.ToJsonString(MetadataJsonOptions);
        }

        private static double? CalculateBlockDepthFromGeometry(Geometry? geometry)
        {
            Polygon? polygon = ExtractLargestPolygon(geometry);
            if (polygon == null || polygon.IsEmpty)
            {
                return null;
            }

            Geometry hullGeometry = polygon.ConvexHull();
            Coordinate[] coordinates = hullGeometry switch
            {
                Polygon hullPolygon => hullPolygon.ExteriorRing.Coordinates,
                LineString lineString => lineString.Coordinates,
                _ => []
            };

            List<Coordinate> points = coordinates
                .Where(coordinate => double.IsFinite(coordinate.X) && double.IsFinite(coordinate.Y))
                .ToList();
            if (points.Count > 1 && points[0].Equals2D(points[^1], 1e-7))
            {
                points.RemoveAt(points.Count - 1);
            }

            if (points.Count < 3)
            {
                return null;
            }

            double? bestWidth = null;
            for (int index = 0; index < points.Count; index++)
            {
                Coordinate start = points[index];
                Coordinate end = points[(index + 1) % points.Count];
                double dx = end.X - start.X;
                double dy = end.Y - start.Y;
                double edgeLength = Math.Sqrt(dx * dx + dy * dy);
                if (edgeLength <= 1e-9)
                {
                    continue;
                }

                double normalX = -dy / edgeLength;
                double normalY = dx / edgeLength;
                double minProjection = double.PositiveInfinity;
                double maxProjection = double.NegativeInfinity;
                foreach (Coordinate point in points)
                {
                    double projection = point.X * normalX + point.Y * normalY;
                    minProjection = Math.Min(minProjection, projection);
                    maxProjection = Math.Max(maxProjection, projection);
                }

                double width = maxProjection - minProjection;
                if (width <= 1e-9 || !double.IsFinite(width))
                {
                    continue;
                }

                bestWidth = !bestWidth.HasValue
                    ? width
                    : Math.Min(bestWidth.Value, width);
            }

            return bestWidth.HasValue && bestWidth.Value > 0.0
                ? bestWidth.Value
                : null;
        }

        private static Polygon? ExtractLargestPolygon(Geometry? geometry)
        {
            return geometry switch
            {
                Polygon polygon => polygon,
                MultiPolygon multiPolygon => multiPolygon.Geometries
                    .OfType<Polygon>()
                    .OrderByDescending(polygon => polygon.Area)
                    .FirstOrDefault(),
                GeometryCollection collection => collection.Geometries
                    .Select(ExtractLargestPolygon)
                    .Where(polygon => polygon != null)
                    .OrderByDescending(polygon => polygon!.Area)
                    .FirstOrDefault(),
                _ => null
            };
        }

        private static bool TryGetJsonPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        value = property.Value;
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        private static void RemoveJsonPropertyIgnoreCase(JsonObject root, string propertyName)
        {
            string? existingKey = root
                .Select(property => property.Key)
                .FirstOrDefault(key => key.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (existingKey != null)
            {
                root.Remove(existingKey);
            }
        }

        private static string ToCamelCase(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? value
                : char.ToLowerInvariant(value[0]) + value[1..];
        }

        private static bool IsPolygonObject(CanvasObject canvasObject)
        {
            return canvasObject.ObjectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase) ||
                   canvasObject.ObjectType.Equals("Rectangle", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPolylineObject(CanvasObject canvasObject)
        {
            return canvasObject.ObjectType.Equals("Polyline", StringComparison.OrdinalIgnoreCase);
        }

        private static bool SameCoordinate(Coordinate first, Coordinate second)
        {
            return first.Equals2D(second, 1e-7);
        }

        private sealed record PolylineCurveMetadata(
            string ShapeType,
            bool IsClosed,
            List<PolylineSegmentMetadata> Segments);

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
    }
}
