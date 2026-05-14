using System.Drawing.Drawing2D;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public static class ParcelPathBuilder
    {
        public static GraphicsPath ToPath(
            Polygon polygon,
            Func<Coordinate, PointF> worldToScreen)
        {
            ArgumentNullException.ThrowIfNull(polygon);
            ArgumentNullException.ThrowIfNull(worldToScreen);

            GraphicsPath path = new() { FillMode = System.Drawing.Drawing2D.FillMode.Alternate };
            AddRing(path, polygon.ExteriorRing, worldToScreen);

            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                AddRing(path, polygon.GetInteriorRingN(i), worldToScreen);
            }

            return path;
        }

        private static void AddRing(
            GraphicsPath path,
            LineString ring,
            Func<Coordinate, PointF> worldToScreen)
        {
            PointF[] points = ring.Coordinates
                .Select(worldToScreen)
                .Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y))
                .ToArray();

            if (points.Length >= 3)
            {
                path.AddPolygon(points);
            }
        }
    }
}
