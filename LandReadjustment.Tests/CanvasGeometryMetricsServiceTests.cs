using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NetTopologySuite.Geometries;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class CanvasGeometryMetricsServiceTests
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);

    [Fact]
    public void GetVertexCount_ArcObject_IgnoresSampledTopologyPoints()
    {
        CanvasObject canvasObject = new()
        {
            ObjectType = "Arc",
            Shape = GeometryFactory.CreateLineString(
            [
                new Coordinate(10, 0),
                new Coordinate(9, 4),
                new Coordinate(6, 8),
                new Coordinate(0, 10)
            ])
        };

        Assert.Equal(2, CanvasGeometryMetricsService.GetVertexCount(canvasObject));
    }

    [Fact]
    public void GetVertexCount_PolylineArcMetadata_CountsOnlySegmentEndpoints()
    {
        CanvasObject canvasObject = new()
        {
            ObjectType = "Polyline",
            GeometryMetadataJson = """
            {
              "ShapeType": "Polyline",
              "IsClosed": false,
              "Segments": [
                { "Kind": "Line", "StartX": 0, "StartY": 0, "EndX": 10, "EndY": 0 },
                { "Kind": "Arc", "StartX": 10, "StartY": 0, "EndX": 10, "EndY": 10,
                  "CenterX": 5, "CenterY": 5, "Radius": 7.0710678118654755,
                  "StartAngleRadians": -0.7853981633974483, "SweepAngleRadians": 1.5707963267948966 },
                { "Kind": "Line", "StartX": 10, "StartY": 10, "EndX": 20, "EndY": 10 }
              ]
            }
            """,
            Shape = GeometryFactory.CreateLineString(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(12, 2),
                new Coordinate(12, 8),
                new Coordinate(10, 10),
                new Coordinate(20, 10)
            ])
        };

        Assert.Equal(4, CanvasGeometryMetricsService.GetVertexCount(canvasObject));
    }

    [Fact]
    public void GetVertexCount_Circle_ReturnsNullBecauseCircleHasNoVertices()
    {
        CanvasObject canvasObject = new()
        {
            ObjectType = "Circle",
            GeometryMetadataJson = """{ "ShapeType": "Circle", "CenterX": 0, "CenterY": 0, "Radius": 10 }""",
            Shape = GeometryFactory.CreatePoint(new Coordinate(0, 0)).Buffer(10, quadrantSegments: 24)
        };

        Assert.Null(CanvasGeometryMetricsService.GetVertexCount(canvasObject));
    }

    [Fact]
    public void GetVertexCount_Polygon_DoesNotCountClosingCoordinateTwice()
    {
        CanvasObject canvasObject = new()
        {
            ObjectType = "Polygon",
            Shape = GeometryFactory.CreatePolygon(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10),
                new Coordinate(0, 10),
                new Coordinate(0, 0)
            ])
        };

        Assert.Equal(4, CanvasGeometryMetricsService.GetVertexCount(canvasObject));
    }
}
