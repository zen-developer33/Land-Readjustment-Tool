using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Roads;
using Land_Readjustment_Tool.Services.Roads;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NetTopologySuite.Geometries;
using Xunit;

namespace LandReadjustment.Tests
{
    public class DonutRoadParcelGeometryTests
    {
        [Fact]
        public void ImportFromWkt_CreatesDonutRoadAndHoleIsNotRoadSurface()
        {
            RoadParcelImportService service = new();

            RoadParcel road = service.ImportFromWkt(
                "POLYGON ((0 0, 20 0, 20 20, 0 20, 0 0), (8 8, 12 8, 12 12, 8 12, 8 8))",
                "R-1",
                "Roundabout Road");

            Assert.True(road.IsDonut);
            Assert.Equal(1, road.Shape.NumInteriorRings);
            Assert.Single(road.Islands);
            Assert.Equal(DonutValidationStatus.Valid, road.ValidationStatus);
            Assert.False(road.Shape.Contains(new GeometryFactory().CreatePoint(new Coordinate(10, 10))));
            Assert.True(road.Shape.Contains(new GeometryFactory().CreatePoint(new Coordinate(4, 4))));
        }

        [Fact]
        public void GeometryShapeMapper_PreservesInteriorRingsForCanvas()
        {
            RoadParcelImportService service = new();
            RoadParcel road = service.ImportFromWkt(
                "POLYGON ((0 0, 30 0, 30 30, 0 30, 0 0), (10 10, 20 10, 20 20, 10 20, 10 10))",
                "R-2",
                "Median Road");

            CanvasObject canvasObject = new()
            {
                ObjectType = "Polygon",
                Shape = road.Shape
            };

            IShape shape = GeometryShapeMapper.ToShape(canvasObject);
            DonutPolygonShape donut = Assert.IsType<DonutPolygonShape>(shape);

            Assert.Single(donut.InteriorRings);
            Assert.False(donut.ContainsPoint(new PointD(15, 15), 0.01f));
            Assert.True(donut.ContainsPoint(new PointD(5, 5), 0.01f));
            Assert.Equal(1, donut.ToGeometry().NumInteriorRings);
        }
    }
}
