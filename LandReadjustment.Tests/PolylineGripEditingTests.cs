using System.Reflection;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class PolylineGripEditingTests
{
    private static readonly Type MapCanvasControlType = typeof(MapCanvasControl);

    [Fact]
    public void VertexGripMoveOntoPreviousVertex_DoesNotDeleteVertexOrDropFollowingArc()
    {
        PointD a = new(0.0, 0.0);
        PointD b = new(10.0, 0.0);
        PointD c = new(10.0, 10.0);
        ArcShape arc = ArcShape.FromThreePoints(b, new PointD(15.0, 5.0), c)!;
        PolylineShape original = new(
            [a, b, c],
            [
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, a, b),
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Arc, b, c, arc)
            ],
            isClosed: false);
        PolylineShape edited = (PolylineShape)original.Clone();

        InvokeApplyPolylineGrip(edited, original, CreateVertexGrip(edited, vertexIndex: 1), a);

        Assert.Equal(3, edited.Vertices.Count);
        Assert.Equal(2, edited.Segments.Count);
        AssertPoint(a, edited.Vertices[1]);

        Assert.Equal(PolylineShape.PolylineSegmentKind.Line, edited.Segments[0].Kind);
        AssertPoint(a, edited.Segments[0].Start);
        AssertPoint(a, edited.Segments[0].End);

        Assert.Equal(PolylineShape.PolylineSegmentKind.Arc, edited.Segments[1].Kind);
        Assert.NotNull(edited.Segments[1].Arc);
        AssertPoint(a, edited.Segments[1].Start);
        AssertPoint(c, edited.Segments[1].End);
    }

    [Fact]
    public void VertexGripMoveOntoNextVertex_KeepsPolylineTopology()
    {
        PointD a = new(0.0, 0.0);
        PointD b = new(10.0, 0.0);
        PointD c = new(20.0, 0.0);
        PolylineShape original = new(
            [a, b, c],
            [
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, a, b),
                new PolylineShape.PolylineSegment(PolylineShape.PolylineSegmentKind.Line, b, c)
            ],
            isClosed: false);
        PolylineShape edited = (PolylineShape)original.Clone();

        InvokeApplyPolylineGrip(edited, original, CreateVertexGrip(edited, vertexIndex: 1), c);

        Assert.Equal(3, edited.Vertices.Count);
        Assert.Equal(2, edited.Segments.Count);
        AssertPoint(c, edited.Vertices[1]);
        AssertPoint(a, edited.Segments[0].Start);
        AssertPoint(c, edited.Segments[0].End);
        AssertPoint(c, edited.Segments[1].Start);
        AssertPoint(c, edited.Segments[1].End);
    }

    [Fact]
    public void ActiveLineEndpointSnapPoint_IsSuppressedOnlyWhenItIsNotCoincidentWithAnotherSnapPoint()
    {
        LineShape line = new(new PointD(0.0, 0.0), new PointD(10.0, 0.0));
        object grip = CreateVertexGrip(line, line.Start, vertexIndex: 0);

        Assert.True(InvokeIsDraggedHandleSnapPoint(
            new SnapPoint(SnapType.Endpoint, line.Start, line),
            line,
            grip));

        line.Start = line.End;

        Assert.False(InvokeIsDraggedHandleSnapPoint(
            new SnapPoint(SnapType.Endpoint, line.End, line),
            line,
            grip));
    }

    [Fact]
    public void ActiveArcEndpointSnapPoint_RemainsAvailableWhenMovedOntoOtherEndpoint()
    {
        ArcShape arc = new(
            new PointD(0.0, 0.0),
            10.0,
            0.1,
            1.0);
        object grip = CreateVertexGrip(arc, arc.StartPoint, vertexIndex: 0);

        Assert.True(InvokeIsDraggedHandleSnapPoint(
            new SnapPoint(SnapType.Endpoint, arc.StartPoint, arc),
            arc,
            grip));

        ArcShape collapsed = new(
            new PointD(0.0, 0.0),
            10.0,
            0.0,
            Math.PI * 2.0);
        object collapsedGrip = CreateVertexGrip(collapsed, collapsed.StartPoint, vertexIndex: 0);

        Assert.False(InvokeIsDraggedHandleSnapPoint(
            new SnapPoint(SnapType.Endpoint, collapsed.EndPoint, collapsed),
            collapsed,
            collapsedGrip));
    }

    private static object CreateVertexGrip(PolylineShape shape, int vertexIndex)
        => CreateVertexGrip(shape, shape.Vertices[vertexIndex], vertexIndex);

    private static object CreateVertexGrip(IShape shape, PointD position, int vertexIndex)
    {
        Type gripType = MapCanvasControlType.GetNestedType("SelectionGrip", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("SelectionGrip type was not found.");
        Type kindType = MapCanvasControlType.GetNestedType("SelectionGripKind", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("SelectionGripKind type was not found.");
        Type glyphType = MapCanvasControlType.GetNestedType("SelectionGripGlyph", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("SelectionGripGlyph type was not found.");

        object grip = Activator.CreateInstance(gripType)
            ?? throw new InvalidOperationException("SelectionGrip could not be created.");
        SetProperty(grip, "Shape", shape);
        SetProperty(grip, "Kind", Enum.Parse(kindType, "Vertex"));
        SetProperty(grip, "Glyph", Enum.Parse(glyphType, "Square"));
        SetProperty(grip, "Position", position);
        SetProperty(grip, "VertexIndex", vertexIndex);
        return grip;
    }

    private static bool InvokeIsDraggedHandleSnapPoint(
        SnapPoint snapPoint,
        IShape previewShape,
        object grip)
    {
        MethodInfo method = MapCanvasControlType.GetMethod(
                "IsDraggedHandleSnapPoint",
                BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("IsDraggedHandleSnapPoint method was not found.");

        return (bool)method.Invoke(null, [snapPoint, previewShape, grip])!;
    }

    private static void InvokeApplyPolylineGrip(
        PolylineShape polyline,
        PolylineShape originalPolyline,
        object grip,
        PointD target)
    {
        MethodInfo method = MapCanvasControlType.GetMethod(
                "ApplyPolylineGrip",
                BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException("ApplyPolylineGrip method was not found.");

        method.Invoke(null, [polyline, originalPolyline, grip, target]);
    }

    private static void SetProperty(object target, string name, object? value)
    {
        target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(target, value);
    }

    private static void AssertPoint(PointD expected, PointD actual)
    {
        Assert.Equal(expected.X, actual.X, precision: 9);
        Assert.Equal(expected.Y, actual.Y, precision: 9);
    }
}
