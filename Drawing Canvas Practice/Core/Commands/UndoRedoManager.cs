using System.Collections.Generic;

namespace Drawing_Canvas_Practice.Core.Commands
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
        private Stack<ICommand> _undoStack;
        private Stack<ICommand> _redoStack;
        private int _maxUndoLevels;

        public UndoRedoManager(int maxUndoLevels = 100)
        {
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
            _maxUndoLevels = maxUndoLevels;
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
            return CanUndo ? _undoStack.Peek().Description : "";
        }

        /// <summary>
        /// Get description of next redo action
        /// </summary>
        public string GetRedoDescription()
        {
            return CanRedo ? _redoStack.Peek().Description : "";
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
                ICommand previous = _undoStack.Peek();
                if (previous.CanMergeWith(command))
                {
                    previous.MergeWith(command);
                    return; // Don't add to stack, merged into previous
                }
            }

            // Add to undo stack
            _undoStack.Push(command);

            // Clear redo stack (new action invalidates redo)
            _redoStack.Clear();

            // Enforce max undo levels
            if (_undoStack.Count > _maxUndoLevels)
            {
                // Remove oldest command
                var commands = _undoStack.ToArray();
                _undoStack.Clear();
                
                // Re-add all but the oldest
                for (int i = commands.Length - 2; i >= 0; i--)
                {
                    _undoStack.Push(commands[i]);
                }
            }
        }

        /// <summary>
        /// Undo the last command
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) return;

            ICommand command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }

        /// <summary>
        /// Redo the last undone command
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) return;

            ICommand command = _redoStack.Pop();
            command.Redo();
            _undoStack.Push(command);
        }

        /// <summary>
        /// Clear all undo/redo history
        /// WHY: Called when opening new file or clearing drawing
        /// </summary>
        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        /// <summary>
        /// Get undo stack count (for debugging)
        /// </summary>
        public int UndoCount => _undoStack.Count;

        /// <summary>
        /// Get redo stack count (for debugging)
        /// </summary>
        public int RedoCount => _redoStack.Count;
    }
}
