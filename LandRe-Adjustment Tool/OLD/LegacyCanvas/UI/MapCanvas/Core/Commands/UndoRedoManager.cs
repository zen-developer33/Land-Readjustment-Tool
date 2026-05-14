using System.Collections.Generic;

namespace Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Manages undo/redo command stacks.
    /// 
    /// ADVANTAGES OVER OLD APPROACH:
    /// Old: Stack<List<Shape>> (entire drawing cloned)
    /// - Memory: O(n × m) where n=shapes, m=undo levels
    /// - For 10,000 shapes × 50 undo levels = 500,000 shape copies!
    /// 
    /// New: Command pattern (only changes stored)
    /// - Memory: O(k) where k=number of operations
    /// - For 50 operations = 50 command objects
    /// 
    /// 10,000x memory improvement!
    /// </summary>
    public class UndoRedoManager
    {
        private readonly LinkedList<ICommand> _undoStack = new();
        private readonly LinkedList<ICommand> _redoStack = new();
        private readonly int _maxUndoLevels;

        /// <summary>
        /// Raised whenever the undo or redo stack changes.
        /// </summary>
        public event EventHandler? StateChanged;

        public UndoRedoManager(int maxUndoLevels = 100)
        {
            _maxUndoLevels = Math.Max(1, maxUndoLevels);
        }

        /// <summary>
        /// Can we undo?
        /// </summary>
        public bool CanUndo => _undoStack.Count > 0;

        /// <summary>
        /// Can we redo?
        /// </summary>
        public bool CanRedo => _redoStack.Count > 0;

        /// <summary>
        /// Get description of next undo action
        /// WHY: Shows in UI "Undo Add Rectangle"
        /// </summary>
        public string GetUndoDescription()
        {
            return CanUndo ? _undoStack.Last!.Value.Description : string.Empty;
        }

        /// <summary>
        /// Get description of next redo action
        /// </summary>
        public string GetRedoDescription()
        {
            return CanRedo ? _redoStack.Last!.Value.Description : string.Empty;
        }

        /// <summary>
        /// Execute a command and add to undo stack.
        /// 
        /// IMPORTANT: This is the main entry point for all drawing operations!
        /// Instead of: shapes.Add(newShape);
        /// Use: undoManager.ExecuteCommand(new AddShapeCommand(newShape));
        /// </summary>
        public void ExecuteCommand(ICommand command)
        {
            // Execute the command
            command.Execute();

            // Try to merge with previous command (e.g., consecutive moves)
            if (_undoStack.Count > 0)
            {
                ICommand previous = _undoStack.Last!.Value;
                if (previous.CanMergeWith(command))
                {
                    previous.MergeWith(command);
                    _redoStack.Clear();
                    OnStateChanged();
                    return; // Don't add to stack, merged into previous
                }
            }

            // Add to undo stack
            _undoStack.AddLast(command);

            // Clear redo stack (new action invalidates redo)
            _redoStack.Clear();

            // Enforce max undo levels
            while (_undoStack.Count > _maxUndoLevels)
            {
                _undoStack.RemoveFirst();
            }

            OnStateChanged();
        }

        /// <summary>
        /// Undo the last command
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) return;

            ICommand command = _undoStack.Last!.Value;
            _undoStack.RemoveLast();
            command.Undo();
            _redoStack.AddLast(command);
            OnStateChanged();
        }

        /// <summary>
        /// Redo the last undone command
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) return;

            ICommand command = _redoStack.Last!.Value;
            _redoStack.RemoveLast();
            command.Redo();
            _undoStack.AddLast(command);
            OnStateChanged();
        }

        /// <summary>
        /// Clear all undo/redo history
        /// WHY: Called when opening new file or clearing drawing
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnStateChanged();
        }

        /// <summary>
        /// Get undo stack count (for debugging)
        /// </summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>
        /// Get redo stack count (for debugging)
        /// </summary>
        public int RedoCount => _redoStack.Count;

        /// <summary>
        /// Notifies listeners that undo/redo availability or descriptions changed.
        /// </summary>
        private void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
