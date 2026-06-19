using System.Reflection;
using Land_Readjustment_Tool;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using NetTopologySuite.Geometries;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class CreateFeaturesFromPolylineTests
{
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 0);

    [Fact]
    public void PolygonTarget_AcceptsClosedPolylineAndCreatesPolygonGeometry()
    {
        CanvasObject source = new()
        {
            ObjectType = "Polyline",
            Shape = GeometryFactory.CreateLineString(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10),
                new Coordinate(0, 10),
                new Coordinate(0, 0)
            ])
        };
        CanvasLayer targetLayer = new()
        {
            Id = 7,
            Name = "Blocks",
            LayerType = "Block"
        };

        Assert.True(InvokeIsCompatible(source, targetLayer));

        CanvasObject transferred = InvokeCreateTransferredObject(source, targetLayer);
        Assert.Equal("Polygon", transferred.ObjectType);
        Assert.Equal(OgcGeometryType.Polygon, transferred.Shape.OgcGeometryType);
    }

    [Fact]
    public void PolygonTarget_RejectsOpenPolyline()
    {
        CanvasObject source = new()
        {
            ObjectType = "Polyline",
            Shape = GeometryFactory.CreateLineString(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10)
            ])
        };
        CanvasLayer targetLayer = new()
        {
            Id = 7,
            Name = "Blocks",
            LayerType = "Block"
        };

        Assert.False(InvokeIsCompatible(source, targetLayer));
    }

    [Fact]
    public void PolygonTarget_TreatsNearlyClosedPolylineAsClosed()
    {
        CanvasObject source = new()
        {
            ObjectType = "Polyline",
            Shape = GeometryFactory.CreateLineString(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10),
                new Coordinate(0.20, 0.15)
            ])
        };
        CanvasLayer targetLayer = new()
        {
            Id = 7,
            Name = "Blocks",
            LayerType = "Block"
        };

        Assert.True(InvokeIsCompatible(source, targetLayer));

        CanvasObject transferred = InvokeCreateTransferredObject(source, targetLayer);
        Assert.Equal(OgcGeometryType.Polygon, transferred.Shape.OgcGeometryType);
        Polygon polygon = Assert.IsType<Polygon>(transferred.Shape);
        Assert.True(polygon.ExteriorRing.IsClosed);
        Assert.True(polygon.ExteriorRing.GetCoordinateN(0).Equals2D(
            polygon.ExteriorRing.GetCoordinateN(polygon.ExteriorRing.NumPoints - 1)));
    }

    [Fact]
    public void BlockTarget_PreservesClosedPolylineArcMetadataForRendering()
    {
        const string curveMetadataJson = """
        {
          "shapeType": "Polygon",
          "isClosed": true,
          "segments": [
            {
              "kind": "Arc",
              "startX": 0.0,
              "startY": 0.0,
              "endX": 10.0,
              "endY": 0.0,
              "centerX": 5.0,
              "centerY": 0.0,
              "radius": 5.0,
              "startAngleRadians": 3.141592653589793,
              "sweepAngleRadians": 3.141592653589793
            },
            {
              "kind": "Line",
              "startX": 10.0,
              "startY": 0.0,
              "endX": 0.0,
              "endY": 0.0,
              "centerX": null,
              "centerY": null,
              "radius": null,
              "startAngleRadians": null,
              "sweepAngleRadians": null
            }
          ]
        }
        """;

        CanvasObject source = new()
        {
            ObjectType = "Polygon",
            Shape = GeometryFactory.CreatePolygon(
            [
                new Coordinate(0, 0),
                new Coordinate(5, 5),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            ]),
            GeometryMetadataJson = curveMetadataJson
        };
        CanvasLayer targetLayer = new()
        {
            Id = 7,
            Name = "Blocks",
            LayerType = "Block"
        };

        CanvasObject transferred = InvokeCreateTransferredObject(source, targetLayer);

        Assert.Equal(curveMetadataJson, transferred.GeometryMetadataJson);
        PolylineShape shape = Assert.IsType<PolylineShape>(
            GeometryShapeMapper.ToShape(transferred));
        Assert.True(shape.IsClosed);
        Assert.Contains(
            shape.Segments,
            segment => segment.Kind == PolylineShape.PolylineSegmentKind.Arc &&
                       segment.Arc != null);
    }

    [Fact]
    public void GeometryShapeMapper_ExactlyClosesSampledCurvedPolygon()
    {
        PointD start = new(0, 0);
        PointD end = new(10, 0);
        ArcShape arc = new(
            new PointD(5, 0),
            5,
            0,
            Math.PI);
        PolylineShape shape = new(
            [start, end],
            [
                new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Line,
                    start,
                    end),
                new PolylineShape.PolylineSegment(
                    PolylineShape.PolylineSegmentKind.Arc,
                    end,
                    start,
                    arc)
            ],
            isClosed: true);

        CanvasObject canvasObject = GeometryShapeMapper.ToCanvasObject(shape, layerId: 7);

        Polygon polygon = Assert.IsType<Polygon>(canvasObject.Shape);
        Coordinate first = polygon.ExteriorRing.GetCoordinateN(0);
        Coordinate last = polygon.ExteriorRing.GetCoordinateN(polygon.ExteriorRing.NumPoints - 1);
        Assert.True(first.Equals2D(last));
    }

    [Fact]
    public void BlockTarget_ReconstructsClosedShapeWhenSourceMetadataMarkedOpen()
    {
        // Reproduces the reported bug: a drawing polyline whose curve metadata still says
        // "Polyline" / isClosed=false is transferred to the Blocks layer. The stored geometry
        // becomes a polygon, but the carried metadata previously rebuilt an OPEN polyline (zero
        // area), so the block could not be area-selected like other block objects.
        const string openPolylineMetadataJson = """
        {
          "shapeType": "Polyline",
          "isClosed": false,
          "segments": [
            { "kind": "Line", "startX": 0.0, "startY": 0.0, "endX": 10.0, "endY": 0.0 },
            { "kind": "Line", "startX": 10.0, "startY": 0.0, "endX": 10.0, "endY": 10.0 },
            { "kind": "Line", "startX": 10.0, "startY": 10.0, "endX": 0.0, "endY": 10.0 }
          ]
        }
        """;

        CanvasObject source = new()
        {
            ObjectType = "Polyline",
            Shape = GeometryFactory.CreateLineString(
            [
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10),
                new Coordinate(0, 10),
                new Coordinate(0, 0)
            ]),
            GeometryMetadataJson = openPolylineMetadataJson
        };
        CanvasLayer targetLayer = new()
        {
            Id = 7,
            Name = "Blocks",
            LayerType = "Block"
        };

        CanvasObject transferred = InvokeCreateTransferredObject(source, targetLayer);
        Assert.Equal("Polygon", transferred.ObjectType);

        IShape shape = GeometryShapeMapper.ToShape(transferred);
        PolylineShape polyline = Assert.IsType<PolylineShape>(shape);
        Assert.True(polyline.IsClosed, "Polygon-typed block must reconstruct as a closed, area-selectable shape.");
    }

    [Fact]
    public void BlockTarget_RecognizesBlocksLayerNameEvenWhenStoredLayerTypeIsPolygon()
    {
        CanvasObject source = new()
        {
            ObjectType = "Polygon",
            Shape = GeometryFactory.CreatePolygon(
            [
                new Coordinate(0, 0),
                new Coordinate(20, 0),
                new Coordinate(20, 8),
                new Coordinate(0, 8),
                new Coordinate(0, 0)
            ])
        };
        CanvasLayer targetLayer = new()
        {
            Id = 7,
            Name = "Blocks",
            LayerType = "Polygon"
        };

        CanvasObject transferred = InvokeCreateTransferredObject(source, targetLayer);

        Assert.Same(targetLayer, transferred.CanvasLayer);
        Assert.True(InvokeIsBlockObject(transferred));
        Assert.Contains(
            CanvasGeometryMetricsService.BlockDepthFromGeometryMetadataKey,
            transferred.GeometryMetadataJson ?? string.Empty);
    }

    private static bool InvokeIsCompatible(CanvasObject source, CanvasLayer targetLayer)
    {
        MethodInfo method = typeof(frmMain).GetMethod(
            "IsCanvasObjectCompatibleWithFeatureTarget",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, [source, targetLayer])!;
    }

    private static CanvasObject InvokeCreateTransferredObject(CanvasObject source, CanvasLayer targetLayer)
    {
        MethodInfo method = typeof(frmMain).GetMethod(
            "CreateTransferredCanvasObjectForLayer",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return (CanvasObject)method.Invoke(null, [source, targetLayer, DateTime.Now])!;
    }

    private static bool InvokeIsBlockObject(CanvasObject source)
    {
        MethodInfo method = typeof(frmMain).GetMethod(
            "IsBlockObject",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return (bool)method.Invoke(null, [source])!;
    }
}
