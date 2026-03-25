using Land_Readjustment_Tool.Core.Entities.Project;

namespace Land_Readjustment_Tool.Data
{
    /// <summary>
    /// Holds all state for one open project.
    /// Regular class — not static.
    /// Supports multiple windows in future.
    /// Registered in AppServices when project opens.
    /// </summary>
    public class ProjectContext
    {
        // ── PROPERTIES ───────────────────────────────

        /// <summary>EF Core session for this project.</summary>
        public ProjectSession Session { get; }

        /// <summary>
        /// Project data loaded from database.
        /// Null until SetInfo() is called.
        /// </summary>
        public Core.Entities.Project.ProjectInfo?
            Info
        { get; private set; }

        /// <summary>
        /// Full path to the open .lpp file.
        /// Example: C:\Projects\Ward5\Ward5.lpp
        /// </summary>
        public string ProjectFilePath { get; }

        /// <summary>
        /// Root folder of the project.
        /// Derived from ProjectFilePath.
        /// Example: C:\Projects\Ward5\
        /// </summary>
        public string ProjectFolderPath =>
            Path.GetDirectoryName(ProjectFilePath)!;

        /// <summary>True if project data is loaded.</summary>
        public bool IsOpen => Info != null;

        /// <summary>True if unsaved changes exist.</summary>
        public bool HasUnsavedChanges { get; private set; }

        // ── EVENTS ───────────────────────────────────

        /// <summary>
        /// Fired when project state changes.
        /// frmMain subscribes to update window title.
        /// </summary>
        public event Action? StateChanged;

        // ── CONSTRUCTOR ──────────────────────────────

        /// <summary>
        /// Creates a ProjectContext for the given session.
        /// </summary>
        public ProjectContext(
            ProjectSession session,
            string projectFilePath)
        {
            Session = session;
            ProjectFilePath = projectFilePath;
        }

        // ── METHODS ──────────────────────────────────

        /// <summary>
        /// Sets project info after loading from database.
        /// Called by ProjectService after create or open.
        /// </summary>
        public void SetInfo(
            Core.Entities.Project.ProjectInfo info)
        {
            Info = info;
            HasUnsavedChanges = false;
            StateChanged?.Invoke();
        }

        /// <summary>
        /// Updates info after user edits project details.
        /// </summary>
        public void UpdateInfo(ProjectInfo info)
        {
            Info = info;
            StateChanged?.Invoke();
        }

        public void MarkAsModified()
        {
            HasUnsavedChanges = true;
            StateChanged?.Invoke();
        }

        public void MarkAsSaved()
        {
            HasUnsavedChanges = false;
            StateChanged?.Invoke();
        }

        /// <summary>
        /// Closes project and releases database connection.
        /// Called by AppServices.ClearContext().
        /// </summary>
        public void Close()
        {
            Session.Dispose();
            Info = null;
            HasUnsavedChanges = false;
            StateChanged?.Invoke();
        }
    }
}