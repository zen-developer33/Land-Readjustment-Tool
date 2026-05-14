# RePlot — Undo/Redo System: Practical Implementation Guide

**Scope:** Document canvas operations only — shape add, delete, move, property change,
import, clear, and composite domain operations (parcel split/merge).
Viewport navigation (zoom, pan) is **out of scope** and handled separately.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Scope Boundary](#2-scope-boundary)
3. [Pre-Implementation Issues Checklist](#3-pre-implementation-issues-checklist)
4. [Step 1 — Fix UndoRedoManager](#4-step-1--fix-undoredomanager)
5. [Step 2 — Add Translate to All Shape Classes](#5-step-2--add-translate-to-all-shape-classes)
6. [Step 3 — MoveShapeCommand](#6-step-3--moveshapecommand)
7. [Step 4 — MoveMultipleShapesCommand](#7-step-4--movemultipleshapescommand)
8. [Step 5 — ModifyPropertyCommand](#8-step-5--modifypropertycommandt)
9. [Step 6 — CompositeCommand](#9-step-6--compositecommand)
10. [Step 7 — ImportShapesCommand](#10-step-7--importshapescommand)
11. [Wiring in DrawingCanvasControl](#11-wiring-in-drawingcanvascontrol)
12. [Persistence Strategy](#12-persistence-strategy)
13. [Hard Rules for Codex](#13-hard-rules-for-codex)
14. [File Map](#14-file-map)
15. [Verification Checklist](#15-verification-checklist)

---

## 1. Architecture Overview

The undo/redo system uses the **Command Pattern**. Every canvas operation that changes
the drawing is wrapped in a command object. That object knows how to execute itself,
undo itself, and redo itself. The `UndoRedoManager` owns two stacks and orchestrates
execution, undo, redo, and merge.

```
User action
    │
    ▼
DrawingCanvasControl
    │  creates command object
    ▼
UndoRedoManager.ExecuteCommand(cmd)
    │  checks merge with previous
    │  pushes to undo stack
    │  clears redo stack
    ▼
cmd.Execute()
    │
    ▼
ShapeManager  ←→  IShape (geometry mutated)
    │
    ▼
RebuildSpatialIndex()
    │
    ▼
Canvas Invalidate / Redraw
```

**On Ctrl+Z:**

```
UndoRedoManager.Undo()
    │  pops from undo stack
    │  pushes to redo stack
    ▼
cmd.Undo()
    │
    ▼
ShapeManager  ←→  IShape (geometry reversed)
    │
    ▼
RebuildSpatialIndex()
    │
    ▼
Canvas Invalidate / Redraw
```

### Two things that are completely separate

| System | Controls | Keyboard |
|---|---|---|
| Document undo stack | Shape add, delete, move, property, import, clear, split, merge | Ctrl+Z / Ctrl+Y |
| View navigation *(future, separate)* | Zoom, pan, zoom extents | To be decided separately |

**Never mix these two systems.** A Ctrl+Z that undoes a zoom is the single biggest
usability failure in CAD software.

---

## 2. Scope Boundary

### In scope — implement now

| Operation | Command Class | Status |
|---|---|---|
| Draw any shape (line, polyline, rect, circle, etc.) | `AddShapeCommand` | ✅ Exists |
| Bulk add shapes | `BulkAddShapesCommand` | ✅ Exists |
| Delete one or many shapes | `DeleteShapesCommand` | ✅ Exists |
| Clear all shapes | `ClearAllCommand` | ✅ Exists |
| Import DXF / external file | `ImportShapesCommand` | 🔴 Create |
| Move a single shape (drag) | `MoveShapeCommand` | 🔴 Create |
| Move multiple shapes (group drag) | `MoveMultipleShapesCommand` | 🔴 Create |
| Change any shape property | `ModifyPropertyCommand<T>` | 🔴 Create |
| Parcel split / merge (future) | `CompositeCommand` | 🔴 Create |

### Out of scope — do not touch

- Zoom in / Zoom out
- Pan
- Zoom to extents
- Zoom to selection
- Previous view / Next view
- Any `DrawingEngine` viewport method

These operations do **not** call `_undoManager.ExecuteCommand()` under any circumstances.

---

## 3. Pre-Implementation Issues Checklist

Read and understand every issue before writing code. Each one is a real bug
or design trap you will hit if you skip it.

---

### Issue 1 — Stack trim is O(n) — must fix first

**Current bug in `UndoRedoManager`:**

```csharp
// CURRENT — broken: O(n) rebuild on every operation past the limit
if (_undoStack.Count > _maxUndoLevels)
{
    var commands = _undoStack.ToArray();
    _undoStack.Clear();
    for (int i = commands.Length - 2; i >= 0; i--)
        _undoStack.Push(commands[i]);
}
```

This converts the entire stack to an array, clears it, and re-pushes every entry
except the oldest. If the limit is 100, this runs on every operation after the 100th.

**Fix:** Replace `Stack<ICommand>` with `LinkedList<ICommand>`.
`RemoveFirst()` is O(1) and never touches the rest of the stack.

---

### Issue 2 — No StateChanged event — toolbar is always static

The Undo and Redo toolbar buttons never enable or disable because
`UndoRedoManager` fires no events. The UI has no way to know when the stack changes.

**Fix:** Add `public event EventHandler? StateChanged;` and invoke it after
every `ExecuteCommand`, `Undo`, `Redo`, and `Clear`. Wire it in
`DrawingCanvasControl` once.

---

### Issue 3 — Drag-move floods the undo stack

When the user drags a shape, `MouseMove` fires 50–200 times per drag gesture.
If each `MouseMove` calls `ExecuteCommand(new MoveShapeCommand(...))`,
the user needs to press Ctrl+Z 200 times to undo a single drag.

**Fix:** Implement `CanMergeWith` on `MoveShapeCommand`. Two `MoveShapeCommand`
instances on the **same shape** (same `Id`) must merge by accumulating their deltas.
`UndoRedoManager.ExecuteCommand` checks merge before pushing.

```
MouseMove 1  →  push MoveShapeCommand(delta=1px)
MouseMove 2  →  CanMergeWith? YES (same shape) → merge delta → stack unchanged
MouseMove 3  →  merge again
...
MouseUp      →  stack has 1 command with total delta
Ctrl+Z       →  shape returns to original position — 1 step
```

---

### Issue 4 — Multi-shape move merge requires set identity check

For `MoveMultipleShapesCommand`, merge should only happen if the incoming command
moves the **exact same set of shapes**. Compare the `Id` sets before merging.
Two different multi-selections must not merge into one command.

---

### Issue 5 — Spatial index rebuild cost in loops

`RebuildSpatialIndex()` rebuilds the entire R-Tree. It is not free.

- For `MoveShapeCommand`: one call after the translate. Correct.
- For `MoveMultipleShapesCommand`: translate ALL shapes in the loop first,
  then call `RebuildSpatialIndex()` ONCE after the loop. Never inside the loop.

```csharp
// WRONG — rebuilds N times
foreach (var s in _shapes) { s.Translate(_totalDelta); _shapeManager.RebuildSpatialIndex(); }

// CORRECT — rebuilds once
foreach (var s in _shapes) s.Translate(_totalDelta);
_shapeManager.RebuildSpatialIndex();
```

---

### Issue 6 — Translate must update the bounding box

After `Translate` shifts the shape's coordinates, the bounding box **must also update**.
The bounding box is what `ShapeManager.QueryShapesInBound` uses for spatial queries.
If the bounding box is stale, the shape disappears from viewport queries after a move.

**Recommended pattern:** call a `RecalculateBounds()` method at the end of every
`Translate` override. Each shape knows how to derive its own bounding box from
its current coordinates.

---

### Issue 7 — CompositeCommand undo order is critical

When undoing a composite, sub-commands must undo in **reverse order** of execution.

Example — parcel split (execute order):
1. `DeleteShapesCommand` (removes original)
2. `AddShapeCommand` (adds left parcel)
3. `AddShapeCommand` (adds right parcel)

Undo order **must be**:
1. Undo `AddShapeCommand` (removes right parcel)
2. Undo `AddShapeCommand` (removes left parcel)
3. Undo `DeleteShapesCommand` (restores original)

Getting this backwards leaves the canvas in a corrupted state.

---

### Issue 8 — ClearAllCommand stores references, not deep copies

`ClearAllCommand._previousShapes` is a `List<IShape>` of the original shape
references. When `Undo()` calls `BulkAddShapes(_previousShapes)`, it re-adds
those exact objects. This is correct.

**Verify:** `BulkAddShapes` must not clone or wrap shapes. It must re-register
the original objects in the shape collection and spatial index.

---

### Issue 9 — Redo stack must clear on new command

When `ExecuteCommand` is called after a series of undos, the redo stack clears.
This is already in your current code. **Do not remove it in the refactor.**

```
State:  [A] [B] [C]  ← undo stack
                     ← redo stack empty

Ctrl+Z twice:
[A]         ← undo stack
[C] [B]     ← redo stack

Now user draws D:
[A] [D]     ← undo stack (new action)
             ← redo stack CLEARED — B and C are gone
```

---

### Issue 10 — ModifyPropertyCommand setter captures by reference

The `Action<IShape, T> setter` lambda is created at command construction time.
It holds a direct reference to the shape. When `Undo()` calls
`_setter(_shape, _oldValue)`, no lookup is performed — it directly mutates
the captured shape. This is correct and efficient.

**Think about side effects:** If changing a property (e.g., `LayerName`) requires
a cache invalidation or a renderer notification, trigger it **inside the setter lambda**,
not in the command class itself. The command stays generic; the caller decides
the side effects.

---

### Issue 11 — Stack must clear on project open

When the user opens a new project or creates a new drawing, call
`_undoManager.Clear()`. References in the undo stack point to shapes from the
previous session. Those shapes no longer exist. Calling `Undo` on a stale
command will crash.

---

## 4. Step 1 — Fix UndoRedoManager

**File:** `Core/Commands/UndoRedoManager.cs`

### Change 1 — Replace Stack with LinkedList

```csharp
// BEFORE
private Stack<ICommand> _undoStack;
private Stack<ICommand> _redoStack;

// AFTER
private readonly LinkedList<ICommand> _undoStack = new();
private readonly LinkedList<ICommand> _redoStack = new();
```

### Change 2 — Rewrite ExecuteCommand

```csharp
public void ExecuteCommand(ICommand command)
{
    command.Execute();

    // Merge with previous command if possible (e.g., consecutive drag moves)
    if (_undoStack.Count > 0 && _undoStack.Last!.Value.CanMergeWith(command))
    {
        _undoStack.Last.Value.MergeWith(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
        return;
    }

    _undoStack.AddLast(command);
    _redoStack.Clear();

    // Trim oldest — O(1), no array rebuild
    while (_undoStack.Count > _maxUndoLevels)
        _undoStack.RemoveFirst();

    StateChanged?.Invoke(this, EventArgs.Empty);
}
```

### Change 3 — Rewrite Undo and Redo

```csharp
public void Undo()
{
    if (_undoStack.Count == 0) return;

    ICommand command = _undoStack.Last!.Value;
    _undoStack.RemoveLast();
    command.Undo();
    _redoStack.AddLast(command);

    StateChanged?.Invoke(this, EventArgs.Empty);
}

public void Redo()
{
    if (_redoStack.Count == 0) return;

    ICommand command = _redoStack.Last!.Value;
    _redoStack.RemoveLast();
    command.Redo();
    _undoStack.AddLast(command);

    StateChanged?.Invoke(this, EventArgs.Empty);
}
```

### Change 4 — Add StateChanged event and Clear update

```csharp
public event EventHandler? StateChanged;

public void Clear()
{
    _undoStack.Clear();
    _redoStack.Clear();
    StateChanged?.Invoke(this, EventArgs.Empty);
}
```

### Change 5 — Update property accessors for LinkedList

```csharp
public bool CanUndo => _undoStack.Count > 0;
public bool CanRedo => _redoStack.Count > 0;

public string GetUndoDescription()
    => CanUndo ? _undoStack.Last!.Value.Description : string.Empty;

public string GetRedoDescription()
    => CanRedo ? _redoStack.Last!.Value.Description : string.Empty;

public int UndoCount => _undoStack.Count;
public int RedoCount => _redoStack.Count;
```

**Test gate:** Draw 3 shapes. Ctrl+Z three times (canvas empty).
Ctrl+Y three times (all shapes back). Toolbar buttons toggle correctly.

---

## 5. Step 2 — Add Translate to All Shape Classes

### 5.1 Add to IShape interface

**File:** `Models/Shapes/IShape.cs`

```csharp
/// <summary>
/// Moves the shape by the given delta in world coordinates.
/// Implementations must also update the bounding box.
/// </summary>
void Translate(PointD delta);
```

### 5.2 Implement in Shape base class

**File:** `Models/Shapes/Shape.cs`

```csharp
public virtual void Translate(PointD delta)
{
    // Subclasses shift their geometry, then call base or RecalculateBounds directly.
    // Base shifts the cached bounding box if maintained at this level.
    RecalculateBounds();
}

protected virtual void RecalculateBounds()
{
    // Each subclass overrides to derive bounds from current coordinates.
}
```

### 5.3 LineShape

**File:** `Models/Shapes/LineShape.cs`

```csharp
public override void Translate(PointD delta)
{
    _startPoint = new PointD(_startPoint.X + delta.X, _startPoint.Y + delta.Y);
    _endPoint   = new PointD(_endPoint.X   + delta.X, _endPoint.Y   + delta.Y);
    RecalculateBounds();
}
```

### 5.4 PolylineShape

**File:** `Models/Shapes/PolylineShape.cs`

```csharp
public override void Translate(PointD delta)
{
    for (int i = 0; i < _vertices.Count; i++)
        _vertices[i] = new PointD(_vertices[i].X + delta.X, _vertices[i].Y + delta.Y);
    RecalculateBounds();
}
```

### 5.5 RectangleShape

**File:** `Models/Shapes/RectangleShape.cs`

```csharp
public override void Translate(PointD delta)
{
    _origin = new PointD(_origin.X + delta.X, _origin.Y + delta.Y);
    RecalculateBounds();
}
```

### 5.6 CircleShape

**File:** `Models/Shapes/CircleShape.cs`

```csharp
public override void Translate(PointD delta)
{
    _center = new PointD(_center.X + delta.X, _center.Y + delta.Y);
    RecalculateBounds();
}
```

### 5.7 EllipseShape

**File:** `Models/Shapes/EllipseShape.cs`

```csharp
public override void Translate(PointD delta)
{
    _center = new PointD(_center.X + delta.X, _center.Y + delta.Y);
    RecalculateBounds();
}
```

### 5.8 TextShape

**File:** `Models/Shapes/TextShape.cs`

```csharp
public override void Translate(PointD delta)
{
    _position = new PointD(_position.X + delta.X, _position.Y + delta.Y);
    RecalculateBounds();
}
```

**Test gate:** Manually call `shape.Translate(new PointD(100, 100))` on one
instance of each shape type. Verify the shape renders 100 world-units right and
100 world-units up. Verify bounding box shifts correctly. Verify
`QueryShapesInBound` still returns the shape after the move.

---

## 6. Step 3 — MoveShapeCommand

**File:** `Core/Commands/MoveShapeCommand.cs`  
**Prerequisite:** `IShape.Translate` must be implemented first (Step 2).

```csharp
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Moves a single shape by a world-coordinate delta.
    ///
    /// MERGE BEHAVIOUR:
    /// Consecutive moves on the same shape during a single drag gesture
    /// merge into one command. The total displacement accumulates so that
    /// one Ctrl+Z returns the shape to its original pre-drag position,
    /// regardless of how many MouseMove events fired during the drag.
    /// </summary>
    public class MoveShapeCommand : ICommand
    {
        private readonly ShapeManager _shapeManager;
        private readonly IShape _shape;
        private PointD _totalDelta;

        public string Description => $"Move {_shape.GetType().Name}";

        public MoveShapeCommand(ShapeManager shapeManager, IShape shape, PointD delta)
        {
            _shapeManager = shapeManager;
            _shape        = shape;
            _totalDelta   = delta;
        }

        public void Execute()
        {
            _shape.Translate(_totalDelta);
            _shapeManager.RebuildSpatialIndex();
        }

        public void Undo()
        {
            _shape.Translate(new PointD(-_totalDelta.X, -_totalDelta.Y));
            _shapeManager.RebuildSpatialIndex();
        }

        public void Redo() => Execute();

        public bool CanMergeWith(ICommand other)
            => other is MoveShapeCommand m && m._shape.Id == _shape.Id;

        public void MergeWith(ICommand other)
        {
            if (other is MoveShapeCommand m)
                _totalDelta = new PointD(
                    _totalDelta.X + m._totalDelta.X,
                    _totalDelta.Y + m._totalDelta.Y);
        }
    }
}
```

### How to call this from DrawingCanvasControl

During a drag operation, call `ExecuteCommand` on every `MouseMove`
(not just `MouseUp`). The merge logic in `UndoRedoManager` collapses
all mid-drag moves into one stack entry automatically.

```csharp
// In MouseMove handler (when drag is active):
var delta = new PointD(worldPos.X - _lastDragPos.X, worldPos.Y - _lastDragPos.Y);
_undoManager.ExecuteCommand(new MoveShapeCommand(_shapeManager, _selectedShape, delta));
_lastDragPos = worldPos;
panelCanvas.Invalidate();
```

---

## 7. Step 4 — MoveMultipleShapesCommand

**File:** `Core/Commands/MoveMultipleShapesCommand.cs`

```csharp
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Moves a group of selected shapes by a common world-coordinate delta.
    ///
    /// PERFORMANCE: All shapes translate before RebuildSpatialIndex is called.
    /// Never rebuild inside the loop.
    ///
    /// MERGE: Only merges with another MoveMultipleShapesCommand that moves
    /// the exact same set of shapes (verified by Id set equality).
    /// </summary>
    public class MoveMultipleShapesCommand : ICommand
    {
        private readonly ShapeManager _shapeManager;
        private readonly List<IShape> _shapes;
        private PointD _totalDelta;

        public string Description => $"Move {_shapes.Count} shapes";

        public MoveMultipleShapesCommand(
            ShapeManager shapeManager, IEnumerable<IShape> shapes, PointD delta)
        {
            _shapeManager = shapeManager;
            _shapes       = shapes.ToList();
            _totalDelta   = delta;
        }

        public void Execute()
        {
            foreach (var s in _shapes)
                s.Translate(_totalDelta);          // translate all first
            _shapeManager.RebuildSpatialIndex();   // rebuild once
        }

        public void Undo()
        {
            var reverse = new PointD(-_totalDelta.X, -_totalDelta.Y);
            foreach (var s in _shapes)
                s.Translate(reverse);
            _shapeManager.RebuildSpatialIndex();
        }

        public void Redo() => Execute();

        public bool CanMergeWith(ICommand other)
        {
            if (other is not MoveMultipleShapesCommand m) return false;
            if (m._shapes.Count != _shapes.Count) return false;

            // Must be the exact same set of shapes
            var thisIds  = _shapes.Select(s => s.Id).ToHashSet();
            var otherIds = m._shapes.Select(s => s.Id).ToHashSet();
            return thisIds.SetEquals(otherIds);
        }

        public void MergeWith(ICommand other)
        {
            if (other is MoveMultipleShapesCommand m)
                _totalDelta = new PointD(
                    _totalDelta.X + m._totalDelta.X,
                    _totalDelta.Y + m._totalDelta.Y);
        }
    }
}
```

---

## 8. Step 5 — ModifyPropertyCommand\<T\>

**File:** `Core/Commands/ModifyPropertyCommand.cs`

```csharp
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Undoes any single property change on any shape.
    ///
    /// The setter delegate is provided by the caller and captures the exact
    /// property assignment. No reflection, no string-based lookup at undo time.
    ///
    /// DOES NOT MERGE: every property change is a discrete undo step.
    ///
    /// SIDE EFFECTS: If the property change requires a cache invalidation or
    /// renderer notification, trigger it inside the setter lambda — not here.
    /// The command stays generic; the caller owns the side effects.
    /// </summary>
    public class ModifyPropertyCommand<T> : ICommand
    {
        private readonly IShape _shape;
        private readonly T _oldValue;
        private readonly T _newValue;
        private readonly Action<IShape, T> _setter;
        private readonly string _propertyName;

        public string Description => $"Change {_propertyName}";

        public ModifyPropertyCommand(
            IShape shape,
            string propertyName,
            T oldValue,
            T newValue,
            Action<IShape, T> setter)
        {
            _shape        = shape;
            _propertyName = propertyName;
            _oldValue     = oldValue;
            _newValue     = newValue;
            _setter       = setter;
        }

        public void Execute() => _setter(_shape, _newValue);
        public void Undo()    => _setter(_shape, _oldValue);
        public void Redo()    => Execute();

        public bool CanMergeWith(ICommand other) => false;
        public void MergeWith(ICommand other) { }
    }
}
```

### Usage examples

**Change fill color:**
```csharp
var cmd = new ModifyPropertyCommand<Color>(
    shape, "FillColor",
    shape.FillColor, newColor,
    (s, c) => s.FillColor = c);
_undoManager.ExecuteCommand(cmd);
```

**Change layer:**
```csharp
var cmd = new ModifyPropertyCommand<string>(
    shape, "LayerName",
    shape.LayerName, newLayerName,
    (s, v) => { s.LayerName = v; _deferredRenderer.InvalidateCache(); });
_undoManager.ExecuteCommand(cmd);
```

**Change line weight:**
```csharp
var cmd = new ModifyPropertyCommand<float>(
    shape, "LineWeight",
    shape.LineWeight, newWeight,
    (s, v) => s.LineWeight = v);
_undoManager.ExecuteCommand(cmd);
```

**Change visibility:**
```csharp
var cmd = new ModifyPropertyCommand<bool>(
    shape, "IsVisible",
    shape.IsVisible, newVisibility,
    (s, v) => s.IsVisible = v);
_undoManager.ExecuteCommand(cmd);
```

---

## 9. Step 6 — CompositeCommand

**File:** `Core/Commands/CompositeCommand.cs`

```csharp
namespace Land_Readjustment_Tool.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Groups multiple commands into one atomic undo step.
    ///
    /// Used for domain operations that involve several sub-operations that
    /// must always undo together. Examples:
    ///   - Parcel split:  delete original + add two new parcels
    ///   - Parcel merge:  delete two parcels + add one merged parcel
    ///   - Road creation: add road shape + add two boundary shapes
    ///
    /// UNDO ORDER: Sub-commands undo in REVERSE execution order. This is
    /// not optional — reversing the reverse produces a corrupted canvas state.
    ///
    /// DOES NOT MERGE: composite operations are always discrete undo steps.
    /// </summary>
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> _commands;
        public string Description { get; }

        public CompositeCommand(string description, params ICommand[] commands)
        {
            Description = description;
            _commands   = new List<ICommand>(commands);
        }

        public CompositeCommand(string description, IEnumerable<ICommand> commands)
        {
            Description = description;
            _commands   = commands.ToList();
        }

        public void Execute()
        {
            foreach (var cmd in _commands)
                cmd.Execute();
        }

        public void Undo()
        {
            // Critical: reverse order
            for (int i = _commands.Count - 1; i >= 0; i--)
                _commands[i].Undo();
        }

        public void Redo() => Execute();

        public bool CanMergeWith(ICommand other) => false;
        public void MergeWith(ICommand other) { }
    }
}
```

### Parcel split usage (future implementation)

```csharp
// When the split geometry is ready:
var splitCmd = new CompositeCommand(
    "Split Parcel",
    new DeleteShapesCommand(_shapeManager, new List<IShape> { originalParcel }),
    new AddShapeCommand(_shapeManager, leftParcel),
    new AddShapeCommand(_shapeManager, rightParcel));

_undoManager.ExecuteCommand(splitCmd);

// Ctrl+Z: right parcel removed, left parcel removed, original restored — one step.
```

### Parcel merge usage (future implementation)

```csharp
var mergeCmd = new CompositeCommand(
    "Merge Parcels",
    new DeleteShapesCommand(_shapeManager, new List<IShape> { parcelA, parcelB }),
    new AddShapeCommand(_shapeManager, mergedParcel));

_undoManager.ExecuteCommand(mergeCmd);
```

---

## 10. Step 7 — ImportShapesCommand

**File:** `Core/Commands/ImportShapesCommand.cs`

```csharp
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Wraps a DXF or external file import as an undoable command.
    ///
    /// Functionally identical to BulkAddShapesCommand but carries the source
    /// file path so the undo description is meaningful:
    ///   "Import 247 shapes from Parcel_Survey_BlockA.dxf"
    /// instead of the generic "Add shapes".
    ///
    /// CALL SITE: Replace new BulkAddShapesCommand(...) at the DXF import
    /// call site with new ImportShapesCommand(...). Keep BulkAddShapesCommand
    /// for programmatic internal use.
    /// </summary>
    public class ImportShapesCommand : ICommand
    {
        private readonly ShapeManager _shapeManager;
        private readonly List<IShape> _shapes;
        private readonly string _sourceFile;

        public string Description =>
            $"Import {_shapes.Count} shapes from {Path.GetFileName(_sourceFile)}";

        public ImportShapesCommand(
            ShapeManager shapeManager,
            IEnumerable<IShape> shapes,
            string sourceFile)
        {
            _shapeManager = shapeManager;
            _shapes       = shapes.ToList();
            _sourceFile   = sourceFile;
        }

        public void Execute() => _shapeManager.BulkAddShapes(_shapes);
        public void Undo()    => _shapeManager.BulkRemoveShapes(_shapes);
        public void Redo()    => Execute();

        public bool CanMergeWith(ICommand other) => false;
        public void MergeWith(ICommand other) { }
    }
}
```

### DXF import call site change

```csharp
// BEFORE
_undoManager.ExecuteCommand(new BulkAddShapesCommand(_shapeManager, importedShapes));

// AFTER
_undoManager.ExecuteCommand(
    new ImportShapesCommand(_shapeManager, importedShapes, openFileDialog.FileName));
```

---

## 11. Wiring in DrawingCanvasControl

**File:** `UI/CustomControls/DrawingCanvasControl.cs`

### 11.1 Wire StateChanged on initialization

In the constructor or `InitializeCanvas` method, after `_undoManager` is created:

```csharp
_undoManager.StateChanged += OnUndoRedoStateChanged;
```

The handler:

```csharp
private void OnUndoRedoStateChanged(object? sender, EventArgs e)
{
    // Replace toolStripButton6/7 with your actual Undo/Redo button names
    toolStripButton6.Enabled     = _undoManager.CanUndo;
    toolStripButton7.Enabled     = _undoManager.CanRedo;
    toolStripButton6.ToolTipText = $"Undo {_undoManager.GetUndoDescription()}";
    toolStripButton7.ToolTipText = $"Redo {_undoManager.GetRedoDescription()}";
}
```

### 11.2 Keyboard shortcuts — ProcessCmdKey

```csharp
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    switch (keyData)
    {
        case Keys.Control | Keys.Z:
            if (_undoManager.CanUndo)
            {
                _undoManager.Undo();
                _deferredRenderer.InvalidateCache();
                panelCanvas.Invalidate();
            }
            return true;

        case Keys.Control | Keys.Y:
            if (_undoManager.CanRedo)
            {
                _undoManager.Redo();
                _deferredRenderer.InvalidateCache();
                panelCanvas.Invalidate();
            }
            return true;
    }

    return base.ProcessCmdKey(ref msg, keyData);
}
```

### 11.3 Clear undo history on new project / open project

```csharp
// Call this whenever a new project is created or an existing one is opened
private void ResetCanvasState()
{
    _shapeManager.Clear();
    _undoManager.Clear();      // Critical: stale commands must not survive
    _deferredRenderer.InvalidateCache();
    panelCanvas.Invalidate();
}
```

### 11.4 Example — property change from toolbar

```csharp
// When user changes fill color of selected shape from a color picker:
private void OnFillColorChanged(Color newColor)
{
    if (_selectedShape == null) return;

    _undoManager.ExecuteCommand(new ModifyPropertyCommand<Color>(
        _selectedShape, "FillColor",
        _selectedShape.FillColor, newColor,
        (s, c) => s.FillColor = c));

    _deferredRenderer.InvalidateCache();
    panelCanvas.Invalidate();
}
```

---

## 12. Persistence Strategy

### Session undo vs. persistent revision — two different things

| Concern | Mechanism | Survives app close? |
|---|---|---|
| Ctrl+Z during working session | `UndoRedoManager` in memory | No |
| Audit trail and post-session recovery | `CanvasObjectRevision` in database | Yes |

These are independent systems. Commands operate in memory only.
The service layer writes revision records for legally significant operations.

### Which operations need a revision record

| Operation | Revision required? | Reason |
|---|---|---|
| Draw shape | No | Routine drafting |
| Delete shape | No | Routine drafting |
| Move shape | No | Routine drafting |
| Change property | No | Routine drafting |
| Import DXF | Optional | Depends on project policy |
| Parcel split | **Yes** | Geometry change with legal consequence |
| Parcel merge | **Yes** | Geometry change with legal consequence |
| Boundary adjustment | **Yes** | Geometry change with legal consequence |
| Clear all | No | Would be a user error, not a tracked operation |

### Where revision writing belongs

```csharp
// In the service layer (NOT inside the command class):
public async Task ExecuteParcelSplitAsync(IShape original, IShape left, IShape right)
{
    // 1. Write revision BEFORE the operation (captures before-state)
    await _revisionRepository.AddAsync(new CanvasObjectRevision
    {
        CanvasObjectId  = original.Id,
        OperationType   = "Split",
        GeometryBefore  = original.ToWkt(),
        Timestamp       = DateTime.UtcNow
    });

    // 2. Execute via undo system (in-memory)
    var cmd = new CompositeCommand(
        "Split Parcel",
        new DeleteShapesCommand(_shapeManager, new List<IShape> { original }),
        new AddShapeCommand(_shapeManager, left),
        new AddShapeCommand(_shapeManager, right));

    _undoManager.ExecuteCommand(cmd);

    // 3. Persist new geometry AFTER the operation
    await _canvasObjectRepository.UpdateAsync(/* left and right as CanvasObjects */);
}
```

The command class stays lean and generic. Persistence is the service layer's job.

---

## 13. Hard Rules for Codex

Read these as invariants. Breaking any one of them creates bugs that are
difficult to trace.

| # | Rule |
|---|---|
| 1 | Zoom, pan, and viewport operations never call `_undoManager.ExecuteCommand()`. |
| 2 | Never deep-clone shapes inside commands. Store direct references only. |
| 3 | `RebuildSpatialIndex()` is called once per command execution — never inside a loop over shapes. |
| 4 | `CompositeCommand.Undo()` iterates sub-commands in reverse order. This is mandatory. |
| 5 | `StateChanged` fires after every `ExecuteCommand`, `Undo`, `Redo`, and `Clear`. Missing one call breaks the toolbar. |
| 6 | `_redoStack` clears whenever `ExecuteCommand` is called. New work invalidates forward history. |
| 7 | `Translate` on every shape must update the bounding box in the same call. |
| 8 | `_undoManager.Clear()` is called when the user opens a new project or file. |
| 9 | Revision records (persistence) are written in the service layer, never inside command classes. |
| 10 | Each command class stays under ~60 lines. If it grows beyond this, a responsibility is leaking. |

---

## 14. File Map

### Create (new files)

| File | Status |
|---|---|
| `Core/Commands/MoveShapeCommand.cs` | 🔴 Create |
| `Core/Commands/MoveMultipleShapesCommand.cs` | 🔴 Create |
| `Core/Commands/ModifyPropertyCommand.cs` | 🔴 Create |
| `Core/Commands/CompositeCommand.cs` | 🔴 Create |
| `Core/Commands/ImportShapesCommand.cs` | 🔴 Create |

### Modify (targeted changes)

| File | What changes |
|---|---|
| `Core/Commands/UndoRedoManager.cs` | Stack → LinkedList, add StateChanged event |
| `Models/Shapes/IShape.cs` | Add `void Translate(PointD delta)` |
| `Models/Shapes/Shape.cs` | Implement Translate in base class |
| `Models/Shapes/LineShape.cs` | Override Translate |
| `Models/Shapes/PolylineShape.cs` | Override Translate |
| `Models/Shapes/RectangleShape.cs` | Override Translate |
| `Models/Shapes/CircleShape.cs` | Override Translate |
| `Models/Shapes/EllipseShape.cs` | Override Translate |
| `Models/Shapes/TextShape.cs` | Override Translate |
| `UI/CustomControls/DrawingCanvasControl.cs` | StateChanged handler, ProcessCmdKey, ResetCanvasState |

### Do not touch

| File | Reason |
|---|---|
| `Core/Commands/ICommand.cs` | Already complete and correct |
| `Core/Commands/ShapeCommands.cs` | All four existing commands work — leave them |
| All `Forms/` files | No changes needed |
| All `Services/` files | No changes needed |
| All `Data/` and `Repositories/` files | No changes needed |
| `DrawingEngine.cs` | No viewport methods needed for this scope |

---

## 15. Verification Checklist

After completing all implementation steps, every item below must pass
before the feature is considered done.

### Undo / Redo core

- [ ] Draw 5 shapes → Ctrl+Z × 5 → canvas is empty → Ctrl+Y × 5 → all 5 shapes restored
- [ ] Undo button is disabled when nothing to undo; enabled when there is something
- [ ] Redo button is disabled when nothing to redo; enabled when there is something
- [ ] Undo tooltip reads `"Undo Add PolylineShape"` (not just `"Undo"`)
- [ ] Redo tooltip reads `"Redo Add PolylineShape"` (not just `"Redo"`)
- [ ] Undo then draw a new shape → Redo button becomes disabled (redo stack cleared)

### Move

- [ ] Drag a single shape 200px → Ctrl+Z once → shape returns to original position in one step
- [ ] Drag a group of shapes → Ctrl+Z once → all shapes return to original positions in one step
- [ ] `RebuildSpatialIndex` is not called per-shape inside any loop

### Property change

- [ ] Change fill color of a shape → Ctrl+Z → original color restored
- [ ] Change layer of a shape → Ctrl+Z → original layer restored
- [ ] Change visibility → Ctrl+Z → original visibility restored
- [ ] Each property change is a separate Ctrl+Z step (no merging)

### Import

- [ ] Import DXF → Ctrl+Z → all imported shapes removed
- [ ] Undo tooltip reads `"Import 247 shapes from Survey.dxf"` with actual filename

### Clear all

- [ ] Clear all → Ctrl+Z → all shapes restored in one step
- [ ] Shapes restored are the original objects (not clones)

### Composite (ready for future parcel operations)

- [ ] Manually create a CompositeCommand with 3 sub-commands → Execute → Undo → all 3 reversed in correct order
- [ ] Manually create a CompositeCommand with 3 sub-commands → Execute → Undo → Redo → all 3 re-executed

### Session boundary

- [ ] Open new project / clear canvas → Ctrl+Z → nothing happens (stack was cleared)
- [ ] No crash when Ctrl+Z is pressed on an empty stack
- [ ] No crash when Ctrl+Y is pressed on an empty redo stack

### Spatial index integrity

- [ ] After Undo of a move, shape is returned by `QueryShapesInBound` at its original position
- [ ] After Undo of an add, shape is not returned by any spatial query
- [ ] After Undo of a delete, shape is returned by spatial queries again

---

*End of implementation guide.*
*Viewport view history (zoom/pan navigation) is a separate system to be designed independently.*
