namespace Land_Readjustment_Tool.OLD.LegacyCanvas.UI.MapCanvas.Core.Commands
{
    /// <summary>
    /// Groups multiple commands into one atomic undo/redo step.
    /// </summary>
    /// <remarks>
    /// Composite commands are used for domain operations such as parcel split or
    /// merge where several lower-level canvas changes must always travel together.
    /// Undo is performed in reverse execution order to restore dependent state safely.
    /// </remarks>
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> _commands;

        /// <summary>
        /// Gets the short text shown in undo and redo UI.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a composite command from a params array of sub-commands.
        /// </summary>
        /// <param name="description">The display description for the full operation.</param>
        /// <param name="commands">The commands to execute as one operation.</param>
        public CompositeCommand(string description, params ICommand[] commands)
            : this(description, (IEnumerable<ICommand>)commands)
        {
        }

        /// <summary>
        /// Creates a composite command from a sequence of sub-commands.
        /// </summary>
        /// <param name="description">The display description for the full operation.</param>
        /// <param name="commands">The commands to execute as one operation.</param>
        public CompositeCommand(string description, IEnumerable<ICommand> commands)
        {
            Description = description;
            _commands = commands.ToList();
        }

        /// <summary>
        /// Executes every sub-command in the order provided.
        /// </summary>
        public void Execute()
        {
            foreach (ICommand command in _commands)
            {
                command.Execute();
            }
        }

        /// <summary>
        /// Undoes every sub-command in reverse order.
        /// </summary>
        public void Undo()
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                _commands[i].Undo();
            }
        }

        /// <summary>
        /// Re-executes the composite after an undo.
        /// </summary>
        public void Redo() => Execute();

        /// <summary>
        /// Composite operations remain discrete undo steps and do not merge.
        /// </summary>
        /// <param name="other">The incoming command being considered for merge.</param>
        public bool CanMergeWith(ICommand other) => false;

        /// <summary>
        /// Does nothing because composite operations are never merged.
        /// </summary>
        /// <param name="other">The command that would have been merged.</param>
        public void MergeWith(ICommand other)
        {
        }
    }
}
