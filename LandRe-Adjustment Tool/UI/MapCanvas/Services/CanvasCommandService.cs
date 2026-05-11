namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Tracks the current drawing command prompt and a short log of recent commands.
    /// Integrates with the status bar to guide the user through each drawing step,
    /// similar to the AutoCAD command-line prompt system (but intentionally simpler).
    ///
    /// Usage pattern:
    ///   - MapCanvasControl calls SetPrompt() whenever drawing state changes.
    ///   - MapCanvasControl calls LogCommand() when a shape is completed or a major action occurs.
    ///   - frmMain subscribes to PromptChanged and CommandLogged to update the status bar.
    ///
    /// This service is intentionally stateless regarding geometry — it only manages text.
    /// </summary>
    public sealed class CanvasCommandService
    {
        private const int MaxLogEntries = 10;

        private readonly Queue<string> _log = new();
        private string _prompt = "Ready";
        private string _lastCommand = string.Empty;

        /// <summary>Current instruction shown in the status bar command area.</summary>
        public string Prompt => _prompt;

        /// <summary>Most recently logged command entry.</summary>
        public string LastCommand => _lastCommand;

        /// <summary>Recent command log (oldest first, max 10 entries).</summary>
        public IReadOnlyList<string> Log => _log.ToArray();

        /// <summary>Fires when the prompt text changes.</summary>
        public event Action<string>? PromptChanged;

        /// <summary>Fires when a new command entry is added to the log.</summary>
        public event Action<string>? CommandLogged;

        /// <summary>
        /// Sets the current user instruction prompt. Fires PromptChanged only if the text changed.
        /// </summary>
        public void SetPrompt(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                prompt = "Ready";

            if (_prompt == prompt)
                return;

            _prompt = prompt;
            PromptChanged?.Invoke(prompt);
        }

        /// <summary>
        /// Records a completed action (e.g., "Created Line", "Placed 3 vertices").
        /// Fires CommandLogged and trims the log to the last 10 entries.
        /// </summary>
        public void LogCommand(string entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
                return;

            _lastCommand = entry;
            _log.Enqueue(entry);
            while (_log.Count > MaxLogEntries)
                _log.Dequeue();

            CommandLogged?.Invoke(entry);
        }

        /// <summary>Resets prompt to "Ready" and optionally clears the log.</summary>
        public void Reset(bool clearLog = false)
        {
            SetPrompt("Ready");
            if (clearLog)
                _log.Clear();
        }
    }
}
