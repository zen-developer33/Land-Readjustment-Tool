using Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Moves a single shape by a world-coordinate delta and supports drag-step merging.
    /// </summary>
    /// <remarks>
    /// Consecutive commands for the same shape accumulate into one undo step, so a
    /// drag operation can generate many mouse-move deltas while Ctrl+Z still returns
    /// the shape to its original position in one operation.
    /// </remarks>
    public class MoveShapeCommand : ICommand
    {
        private readonly ShapeManager _shapeManager;
        private readonly IShape _shape;
        private PointD _totalDelta;

        /// <summary>
        /// Gets the short text shown in undo and redo UI.
        /// </summary>
        public string Description => $"Move {_shape.GetType().Name}";

        /// <summary>
        /// Creates a command that moves one shape by the supplied world delta.
        /// </summary>
        /// <param name="shapeManager">The manager whose spatial index must be rebuilt after movement.</param>
        /// <param name="shape">The shape to move.</param>
        /// <param name="delta">The world-coordinate movement to apply.</param>
        public MoveShapeCommand(ShapeManager shapeManager, IShape shape, PointD delta)
        {
            _shapeManager = shapeManager;
            _shape = shape;
            _totalDelta = delta;
        }

        /// <summary>
        /// Applies the accumulated movement and refreshes the spatial index.
        /// </summary>
        public void Execute()
        {
            _shape.Translate(_totalDelta);
            _shapeManager.RebuildSpatialIndex();
        }

        /// <summary>
        /// Reverses the accumulated movement and refreshes the spatial index.
        /// </summary>
        public void Undo()
        {
            _shape.Translate(new PointD(-_totalDelta.X, -_totalDelta.Y));
            _shapeManager.RebuildSpatialIndex();
        }

        /// <summary>
        /// Reapplies the movement after an undo.
        /// </summary>
        public void Redo() => Execute();

        /// <summary>
        /// Returns true when another move targets the same shape.
        /// </summary>
        /// <param name="other">The incoming command being considered for merge.</param>
        public bool CanMergeWith(ICommand other)
        {
            return other is MoveShapeCommand move && move._shape.Id == _shape.Id;
        }

        /// <summary>
        /// Adds another movement delta into this command's accumulated displacement.
        /// </summary>
        /// <param name="other">The same-shape move command to merge.</param>
        public void MergeWith(ICommand other)
        {
            if (other is not MoveShapeCommand move)
                return;

            _totalDelta = new PointD(
                _totalDelta.X + move._totalDelta.X,
                _totalDelta.Y + move._totalDelta.Y);
        }
    }
}
