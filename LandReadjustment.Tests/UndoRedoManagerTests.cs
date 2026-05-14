using Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core;
using Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Xunit;

namespace LandReadjustment.Tests;

public sealed class UndoRedoManagerTests
{
    [Fact]
    public void ExecuteUndoRedo_WithAddShapeCommand_RestoresShapeCollection()
    {
        ShapeManager shapeManager = CreateShapeManager();
        UndoRedoManager undoManager = new();
        LineShape line = new(new PointD(0, 0), new PointD(10, 0));

        undoManager.ExecuteCommand(new AddShapeCommand(shapeManager, line));
        undoManager.Undo();
        undoManager.Redo();

        Assert.True(undoManager.CanUndo);
        Assert.False(undoManager.CanRedo);
        Assert.Same(line, Assert.Single(shapeManager.GetAllShapes()));
    }

    [Fact]
    public void ExecuteCommand_AfterUndo_ClearsRedoStack()
    {
        ShapeManager shapeManager = CreateShapeManager();
        UndoRedoManager undoManager = new();
        LineShape first = new(new PointD(0, 0), new PointD(10, 0));
        LineShape second = new(new PointD(0, 1), new PointD(10, 1));

        undoManager.ExecuteCommand(new AddShapeCommand(shapeManager, first));
        undoManager.Undo();
        undoManager.ExecuteCommand(new AddShapeCommand(shapeManager, second));

        Assert.False(undoManager.CanRedo);
        Assert.Equal(1, undoManager.UndoCount);
        Assert.Same(second, Assert.Single(shapeManager.GetAllShapes()));
    }

    [Fact]
    public void MoveShapeCommands_ForSameShape_MergeIntoSingleUndoStep()
    {
        ShapeManager shapeManager = CreateShapeManager();
        UndoRedoManager undoManager = new();
        LineShape line = new(new PointD(0, 0), new PointD(10, 0));
        shapeManager.AddShape(line);

        undoManager.ExecuteCommand(new MoveShapeCommand(shapeManager, line, new PointD(3, 4)));
        undoManager.ExecuteCommand(new MoveShapeCommand(shapeManager, line, new PointD(2, -1)));
        undoManager.Undo();

        Assert.Equal(1, undoManager.RedoCount);
        Assert.Equal(0, line.Start.X);
        Assert.Equal(0, line.Start.Y);
        Assert.Equal(10, line.End.X);
        Assert.Equal(0, line.End.Y);
    }

    [Fact]
    public void MoveMultipleShapesCommands_WithSameShapeSet_MergeIntoSingleUndoStep()
    {
        ShapeManager shapeManager = CreateShapeManager();
        UndoRedoManager undoManager = new();
        LineShape first = new(new PointD(0, 0), new PointD(10, 0));
        LineShape second = new(new PointD(0, 10), new PointD(10, 10));
        shapeManager.BulkAddShapes([first, second]);

        undoManager.ExecuteCommand(new MoveMultipleShapesCommand(shapeManager, [first, second], new PointD(5, 0)));
        undoManager.ExecuteCommand(new MoveMultipleShapesCommand(shapeManager, [second, first], new PointD(0, 5)));
        undoManager.Undo();

        Assert.Equal(1, undoManager.RedoCount);
        Assert.Equal(0, first.Start.X);
        Assert.Equal(0, first.Start.Y);
        Assert.Equal(0, second.Start.X);
        Assert.Equal(10, second.Start.Y);
    }

    [Fact]
    public void CompositeCommand_Undo_ReversesAllSubCommands()
    {
        ShapeManager shapeManager = CreateShapeManager();
        UndoRedoManager undoManager = new();
        LineShape original = new(new PointD(0, 0), new PointD(10, 0));
        LineShape replacement = new(new PointD(0, 1), new PointD(10, 1));
        shapeManager.AddShape(original);

        CompositeCommand composite = new(
            "Replace Line",
            new DeleteShapesCommand(shapeManager, [original]),
            new AddShapeCommand(shapeManager, replacement));

        undoManager.ExecuteCommand(composite);
        undoManager.Undo();

        Assert.Same(original, Assert.Single(shapeManager.GetAllShapes()));
    }

    private static ShapeManager CreateShapeManager()
    {
        return new ShapeManager(new RectangleD(-1000, -1000, 2000, 2000));
    }
}
