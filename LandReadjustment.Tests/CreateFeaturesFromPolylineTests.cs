using System.Reflection;
using Land_Readjustment_Tool;
using Land_Readjustment_Tool.Core.Entities.Canvas;
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
}
