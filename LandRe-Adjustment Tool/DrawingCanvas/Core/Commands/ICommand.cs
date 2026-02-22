namespace Land_Readjustment_Tool.DrawingCanvas.Core.Commands
{
    /// <summary>
    /// Command pattern for undo/redo functionality.
    /// 
    /// WHY COMMAND PATTERN:
    /// - Industry standard for undo/redo (used in all major CAD software)
    /// - Encapsulates operations as objects
    /// - Allows complex multi-step operations
    /// - Supports transaction-like grouping
    /// 
    /// BETTER THAN:
    /// - Simple state snapshots (memory inefficient for large drawings)
    /// - Direct manipulation (can't undo)
    /// 
    /// EXAMPLES:
    /// - AddShapeCommand
    /// - DeleteShapeCommand
    /// - MoveShapeCommand
    /// - ModifyShapeCommand
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Execute the command
        /// </summary>
        void Execute();

        /// <summary>
        /// Undo the command (reverse the operation)
        /// </summary>
        void Undo();

        /// <summary>
        /// Redo the command (re-execute after undo)
        /// Usually same as Execute, but can be optimized
        /// </summary>
        void Redo();

        /// <summary>
        /// Description for UI display (optional)
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Can this command be merged with another?
        /// WHY: Multiple small moves can be merged into one undo step
        /// Example: Dragging a shape creates 100 move commands,
        /// but user expects 1 undo to cancel entire drag
        /// </summary>
        bool CanMergeWith(ICommand other);

        /// <summary>
        /// Merge this command with another similar command
        /// </summary>
        void MergeWith(ICommand other);
    }
}
