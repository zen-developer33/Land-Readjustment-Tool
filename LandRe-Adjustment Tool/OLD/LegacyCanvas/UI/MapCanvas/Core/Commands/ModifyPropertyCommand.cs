using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Stores a reversible single-property change for a shape.
    /// </summary>
    /// <typeparam name="T">The type of the property being changed.</typeparam>
    /// <remarks>
    /// The caller supplies the setter delegate, keeping this command generic and
    /// allowing any required side effects, such as cache invalidation, to live at
    /// the call site that understands the property.
    /// </remarks>
    public class ModifyPropertyCommand<T> : ICommand
    {
        private readonly IShape _shape;
        private readonly string _propertyName;
        private readonly T _oldValue;
        private readonly T _newValue;
        private readonly Action<IShape, T> _setter;

        /// <summary>
        /// Gets the short text shown in undo and redo UI.
        /// </summary>
        public string Description => $"Change {_propertyName}";

        /// <summary>
        /// Creates a command that can set a property to a new value and restore the old value.
        /// </summary>
        /// <param name="shape">The shape whose property changes.</param>
        /// <param name="propertyName">A display name for the changed property.</param>
        /// <param name="oldValue">The value to restore during undo.</param>
        /// <param name="newValue">The value to apply during execute and redo.</param>
        /// <param name="setter">The assignment logic for the target property.</param>
        public ModifyPropertyCommand(
            IShape shape,
            string propertyName,
            T oldValue,
            T newValue,
            Action<IShape, T> setter)
        {
            _shape = shape;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
            _setter = setter;
        }

        /// <summary>
        /// Applies the new property value.
        /// </summary>
        public void Execute() => _setter(_shape, _newValue);

        /// <summary>
        /// Restores the previous property value.
        /// </summary>
        public void Undo() => _setter(_shape, _oldValue);

        /// <summary>
        /// Reapplies the new property value after an undo.
        /// </summary>
        public void Redo() => Execute();

        /// <summary>
        /// Property changes remain discrete undo steps and do not merge.
        /// </summary>
        /// <param name="other">The incoming command being considered for merge.</param>
        public bool CanMergeWith(ICommand other) => false;

        /// <summary>
        /// Does nothing because property changes are never merged.
        /// </summary>
        /// <param name="other">The command that would have been merged.</param>
        public void MergeWith(ICommand other)
        {
        }
    }
}
