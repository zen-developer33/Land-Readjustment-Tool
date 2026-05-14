using Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Adds imported shapes as one undoable command with a source-file description.
    /// </summary>
    /// <remarks>
    /// This behaves like <see cref="BulkAddShapesCommand"/> but keeps the import
    /// source path so the undo tooltip can name the file that introduced the shapes.
    /// </remarks>
    public class ImportShapesCommand : ICommand
    {
        private readonly ShapeManager _shapeManager;
        private readonly List<IShape> _shapes;
        private readonly string _sourceFile;

        /// <summary>
        /// Gets the short text shown in undo and redo UI.
        /// </summary>
        public string Description => $"Import {_shapes.Count} shapes from {Path.GetFileName(_sourceFile)}";

        /// <summary>
        /// Creates a command that imports a batch of shapes from a file.
        /// </summary>
        /// <param name="shapeManager">The manager that owns the imported shapes.</param>
        /// <param name="shapes">The imported shapes to add or remove as a batch.</param>
        /// <param name="sourceFile">The source file path used for UI descriptions.</param>
        public ImportShapesCommand(
            ShapeManager shapeManager,
            IEnumerable<IShape> shapes,
            string sourceFile)
        {
            _shapeManager = shapeManager;
            _shapes = shapes.ToList();
            _sourceFile = sourceFile;
        }

        /// <summary>
        /// Adds all imported shapes and rebuilds the spatial index once.
        /// </summary>
        public void Execute() => _shapeManager.BulkAddShapes(_shapes);

        /// <summary>
        /// Removes all imported shapes and rebuilds the spatial index once.
        /// </summary>
        public void Undo() => _shapeManager.BulkRemoveShapes(_shapes);

        /// <summary>
        /// Re-adds imported shapes after an undo.
        /// </summary>
        public void Redo() => Execute();

        /// <summary>
        /// Import operations remain discrete undo steps and do not merge.
        /// </summary>
        /// <param name="other">The incoming command being considered for merge.</param>
        public bool CanMergeWith(ICommand other) => false;

        /// <summary>
        /// Does nothing because import operations are never merged.
        /// </summary>
        /// <param name="other">The command that would have been merged.</param>
        public void MergeWith(ICommand other)
        {
        }
    }
}
