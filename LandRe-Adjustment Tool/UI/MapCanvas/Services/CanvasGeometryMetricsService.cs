using Land_Readjustment_Tool.Core.Entities.Canvas;
using NetTopologySuite.Geometries;
using System.Text.Json;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Provides geometry measurements for canvas objects without leaking
    /// UI-specific formatting into geometry calculations.
    /// </summary>
    public static class CanvasGeometryMetricsService
    {
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
