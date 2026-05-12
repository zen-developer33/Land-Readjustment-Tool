using Land_Readjustment_Tool.Core.Interfaces.Import;
using Land_Readjustment_Tool.Core.Models.Import;
using netDxf;
using netDxf.Entities;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Services.Import.Readers
{
    public sealed class DxfBoundaryReader : IBoundaryFileReader
    {
        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);

        public VectorFileInfo Inspect(string filePath)
        {
            DxfDocument dxf = DxfDocument.Load(filePath);
            Dictionary<string, int> counts = new(StringComparer.OrdinalIgnoreCase);

            foreach (Polyline2D polyline in dxf.Entities.Polylines2D)
            {
                if (!IsClosedPolyline(polyline))
                    continue;

                string layerName = polyline.Layer?.Name ?? "0";
                counts[layerName] = counts.TryGetValue(layerName, out int count)
                    ? count + 1
                    : 1;
            }

            foreach (Polyline3D polyline in dxf.Entities.Polylines3D)
            {
                if (!IsClosedPolyline(polyline))
                    continue;

                string layerName = polyline.Layer?.Name ?? "0";
                counts[layerName] = counts.TryGetValue(layerName, out int count)
                    ? count + 1
                    : 1;
            }

            IReadOnlyList<VectorLayerInfo> layers = counts
                .OrderBy(item => item.Key)
                .Select(item => new VectorLayerInfo(item.Key, item.Value, item.Value > 0))
                .ToList();

            return new VectorFileInfo(
                filePath,
                "DXF",
                layers,
                null,
                RequiresCrsFromUser: true);
        }

        public IReadOnlyList<Geometry> ReadGeometries(
            string filePath,
            BoundaryImportOptions options)
        {
            DxfDocument dxf = DxfDocument.Load(filePath);
            List<Geometry> geometries = [];

            foreach (Polyline2D polyline in dxf.Entities.Polylines2D)
            {
                if (!IsClosedPolyline(polyline) ||
                    !string.Equals(
                        polyline.Layer?.Name ?? "0",
                        options.SelectedLayerName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Geometry? valid = CreatePolygon(ReadCoordinates(polyline));
                if (valid != null)
                    geometries.Add(valid);
            }

            foreach (Polyline3D polyline in dxf.Entities.Polylines3D)
            {
                if (!IsClosedPolyline(polyline) ||
                    !string.Equals(
                        polyline.Layer?.Name ?? "0",
                        options.SelectedLayerName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Geometry? valid = CreatePolygon(ReadCoordinates(polyline));
                if (valid != null)
                    geometries.Add(valid);
            }

            return geometries;
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

        private static Geometry? CreatePolygon(List<Coordinate> coordinates)
        {
            RemoveConsecutiveDuplicates(coordinates);
            if (coordinates.Count < 3)
                return null;

            LinearRing ring = GeometryFactory.CreateLinearRing(
                BoundaryGeometryReaderHelpers.CloseRing(coordinates));
            Polygon polygon = GeometryFactory.CreatePolygon(ring);
            return BoundaryGeometryReaderHelpers.ValidatePolygonalGeometry(polygon);
        }

        private static void RemoveConsecutiveDuplicates(List<Coordinate> coordinates)
        {
            for (int index = coordinates.Count - 1; index > 0; index--)
            {
                if (coordinates[index].Distance(coordinates[index - 1]) <= 0.000001)
                    coordinates.RemoveAt(index);
            }
        }
    }
}
