using Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Moves a group of shapes by a shared world-coordinate delta.
    /// </summary>
    /// <remarks>
    /// The spatial index is rebuilt once after all shapes move. Consecutive moves
    /// merge only when they target the exact same set of shape identifiers.
    /// </remarks>
    public class MoveMultipleShapesCommand : ICommand
    {
        private readonly ShapeManager _shapeManager;
        private readonly List<IShape> _shapes;
        private PointD _totalDelta;

        /// <summary>
        /// Gets the short text shown in undo and redo UI.
        /// </summary>
        public string Description => $"Move {_shapes.Count} shapes";

        /// <summary>
        /// Creates a command that moves all supplied shapes by the same delta.
        /// </summary>
        /// <param name="shapeManager">The manager whose spatial index must be rebuilt after movement.</param>
        /// <param name="shapes">The shapes to move.</param>
        /// <param name="delta">The world-coordinate movement to apply.</param>
        public MoveMultipleShapesCommand(
            ShapeManager shapeManager,
            IEnumerable<IShape> shapes,
            PointD delta)
        {
            _shapeManager = shapeManager;
            _shapes = shapes.ToList();
            _totalDelta = delta;
        }

        /// <summary>
        /// Applies the accumulated movement to every shape, then rebuilds the index once.
        /// </summary>
        public void Execute()
        {
            foreach (IShape shape in _shapes)
            {
                shape.Translate(_totalDelta);
            }

            _shapeManager.RebuildSpatialIndex();
        }

        /// <summary>
        /// Reverses the accumulated movement for every shape, then rebuilds the index once.
        /// </summary>
        public void Undo()
        {
            PointD reverse = new(-_totalDelta.X, -_totalDelta.Y);
            foreach (IShape shape in _shapes)
            {
                shape.Translate(reverse);
            }

            _shapeManager.RebuildSpatialIndex();
        }

        /// <summary>
        /// Reapplies the movement after an undo.
        /// </summary>
        public void Redo() => Execute();

        /// <summary>
        /// Returns true only when another command moves the exact same shape set.
        /// </summary>
        /// <param name="other">The incoming command being considered for merge.</param>
        public bool CanMergeWith(ICommand other)
        {
            if (other is not MoveMultipleShapesCommand move ||
                move._shapes.Count != _shapes.Count)
            {
                return false;
            }

            HashSet<Guid> thisIds = _shapes.Select(shape => shape.Id).ToHashSet();
            HashSet<Guid> otherIds = move._shapes.Select(shape => shape.Id).ToHashSet();
            return thisIds.SetEquals(otherIds);
        }

        /// <summary>
        /// Adds another movement delta into this command's accumulated displacement.
        /// </summary>
        /// <param name="other">The same-selection move command to merge.</param>
        public void MergeWith(ICommand other)
        {
            if (other is not MoveMultipleShapesCommand move)
                return;

            _totalDelta = new PointD(
                _totalDelta.X + move._totalDelta.X,
                _totalDelta.Y + move._totalDelta.Y);
        }
    }
}
