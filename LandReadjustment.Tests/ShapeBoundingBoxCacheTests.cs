using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class ShapeBoundingBoxCacheTests
{
    [Fact]
    public void Translate_InvalidatesCachedBounds()
    {
        LineShape line = new(new PointD(0.0, 0.0), new PointD(10.0, 5.0));
        _ = line.GetBoundingBox();

        line.Translate(new PointD(100.0, 50.0));

        RectangleD bounds = line.GetBoundingBox();
        Assert.Equal(100.0, bounds.Left, precision: 9);
        Assert.Equal(50.0, bounds.Top, precision: 9);
        Assert.Equal(110.0, bounds.Right, precision: 9);
        Assert.Equal(55.0, bounds.Bottom, precision: 9);
    }

    [Fact]
    public void ExplicitInvalidation_RefreshesInPlacePolylineVertexEdits()
    {
        PolylineShape polyline = new(
            [new PointD(0.0, 0.0), new PointD(10.0, 0.0)],
            isClosed: false);
        _ = polyline.GetBoundingBox();

        polyline.Vertices[1] = new PointD(25.0, 15.0);
        polyline.InvalidateBounds();

        RectangleD bounds = polyline.GetBoundingBox();
        Assert.Equal(25.0, bounds.Right, precision: 9);
        Assert.Equal(15.0, bounds.Bottom, precision: 9);
    }
}
