using Land_Readjustment_Tool.Core.Entities.Canvas;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Provides geometry measurements for canvas objects without leaking
    /// UI-specific formatting into geometry calculations.
    /// </summary>
    public static class CanvasGeometryMetricsService
    {
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
            if (canvasObject.Shape == null || canvasObject.Shape.IsEmpty)
                return null;

            int vertexCount = canvasObject.Shape switch
            {
                Polygon polygon => CountPolygonVertices(polygon),
                MultiPolygon multiPolygon => multiPolygon.Geometries
                    .OfType<Polygon>()
                    .Sum(CountPolygonVertices),
                _ => canvasObject.Shape.NumPoints
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
    }
}
