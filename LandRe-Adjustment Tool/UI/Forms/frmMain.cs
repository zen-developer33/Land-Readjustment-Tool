
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Forms;
using Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment;
using Land_Readjustment_Tool.Properties;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Forms;
using Land_Readjustment_Tool.UI.Forms.Project;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using VisualStyles = System.Windows.Forms.VisualStyles;

namespace Land_Readjustment_Tool
{

    public partial class frmMain : Form
    {
        //--- INITIALIZE SERVICES -------
        #region
        private readonly ProjectBackupService _backupService;
        private readonly ProjectSessionFactory _sessionFactory;
        private readonly ProjectService _projectService;
        private readonly IProjectScopedFactory _projectScopedFactory;
        #endregion

        //App Title shown in window Title bar
        private readonly string _appTitle = "RePlot";
        private readonly string? _startupFilePath;
        //Canvas Control for drawing
        private MapCanvasControl? _workspaceCanvas;
        private frmReplotWorkspace? _replotWorkspaceForm;
        private frmAreaConverter? _areaConverterForm;
        private CanvasLayerTreeService? _layerTreeService;
        private bool _suppressLayerTreeEvents;
        private readonly ToolStripStatusLabel _statusSpacer = new()
        {
            Name = "lblStatusSpacer",
            Spring = true,
            Text = string.Empty
        };
        private const string LayerGroupNodeNamePrefix = "LayerGroup_";
        private const int LayerNodeCheckBoxSize = 14;
        private const int LayerNodeCheckBoxGap = 6;
        private const int LayerNodeColorBoxSize = 18;
        private const int LayerNodeColorBoxGap = 4;

        private sealed class LayerTreeNodeState
        {
            public bool IsLayerNode { get; init; }
            public CanvasLayer? Layer { get; set; }
        }

        // Keeps designer/local fallback working without DI container.
        public frmMain(string? startupFilePath = null)
            : this(
                new ProjectBackupService(),
                new ProjectSessionFactory(),
                new ProjectService(),
                new ProjectScopedFactory(),
                startupFilePath)
        {
        }

        public frmMain(
            ProjectBackupService backupService,
            ProjectSessionFactory sessionFactory,
            ProjectService projectService,
            IProjectScopedFactory projectScopedFactory,
            string? startupFilePath = null)
        {
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _projectScopedFactory = projectScopedFactory ?? throw new ArgumentNullException(nameof(projectScopedFactory));

            InitializeComponent();
            _startupFilePath = startupFilePath;
            ConfigureSmoothSplitterLayout();
            mapCanvasControlMain.StatusChanged += MapCanvasControlMain_StatusChanged;
            ConfigureCanvasStatusBarLayout();
            ConfigureLayerTree();
            ConfigureLayerPropertiesPanel();
            MapCanvasControlMain_StatusChanged("E: --    N: --", "Mode: Ready");


            UpdateWindowTitle();
            DisableProjectMenuItems();

            //subscribe to the menu Click events
            this.FormClosing += frmMain_FormClosing!;
            tsmSave.Click += tsmSave_Click!;
            tsmSaveAs.Click += tsmSaveAs_Click!;
            mnuNewProject.Click += tsmNewProject_Click!;
            mnuOpenProject.Click += tsmOpenProject_Click!;
            mnuSaveProject.Click += tsmSave_Click!;
            mnuSaveAsProject.Click += tsmSaveAs_Click!;
            mnuCloseProject.Click += tsmCloseProject_Click!;
            mnuProjectInfo.Click += tsmProjectInformation_Click!;
            mnuProjectSettings.Click += tsmProjectSetting_Click!;
            startReplotWorkspaceToolStripMenuItem.Click += startReplotWorkspaceToolStripMenuItem_Click!;
            mnuPan.CheckOnClick = true;
            mnuZoomIn.Click += mnuZoomIn_Click!;
            mnuZoomOut.Click += mnuZoomOut_Click!;
            mnuZoomExtent.Click += mnuZoomExtent_Click!;
            mnuZoomWindow.Click += mnuZoomWindow_Click!;
            geotiffToolStripMenuItem.Click += geotiffToolStripMenuItem_Click!;

        }

        private void ConfigureSmoothSplitterLayout()
        {
            // Prevent property panel from collapsing into a broken layout.
            mainSplitContainer.Panel1MinSize = 270;

            EnableDoubleBuffering(mainSplitContainer);
            EnableDoubleBuffering(leftSplitContainer);
            EnableDoubleBuffering(splitContainer3);
            EnableDoubleBuffering(grpProperties);

            HookSplitterRedrawHandlers();
        }

        private void HookSplitterRedrawHandlers()
        {
            mainSplitContainer.SplitterMoved += (_, _) => RefreshPropertyPanelLayout();
            leftSplitContainer.SplitterMoved += (_, _) => RefreshPropertyPanelLayout();
            splitContainer3.SplitterMoved += (_, _) => RefreshPropertyPanelLayout();
            leftSplitContainer.Panel2.Resize += (_, _) => RefreshPropertyPanelLayout();
        }

        private void RefreshPropertyPanelLayout()
        {
            if (!IsHandleCreated || IsDisposed)
            {
                return;
            }

            grpProperties.SuspendLayout();
            grpProperties.ResumeLayout(true);
            grpProperties.Invalidate(true);
            grpProperties.Update();
        }

        private static void EnableDoubleBuffering(Control control)
        {
            if (SystemInformation.TerminalServerSession)
            {
                return;
            }

            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(control, true, null);
        }


        private void InitializeProjectWorkspace()
        {
            mainSplitContainer.Visible = true;
            EnableProjectMenuItems();
            _layerTreeService = AppServices.HasContext
                ? _projectScopedFactory.CreateCanvasLayerTreeService(AppServices.Context.Session)
                : null;
        }

        private void UnloadProjectWorkspace()
        {
            CloseReplotWorkspace();
            mainSplitContainer.Visible = false;
            DisableProjectMenuItems();
            _layerTreeService = null;
            ResetLayerTree();
            ClearLayerProperties();
        }

        private void CloseReplotWorkspace()
        {
            if (_replotWorkspaceForm == null || _replotWorkspaceForm.IsDisposed)
            {
                _replotWorkspaceForm = null;
                _workspaceCanvas = null;
                return;
            }

            _replotWorkspaceForm.FormClosed -= ReplotWorkspaceForm_FormClosed;
            _replotWorkspaceForm.Close();
            _replotWorkspaceForm.Dispose();
            _replotWorkspaceForm = null;
            _workspaceCanvas = null;
        }

        private void ReplotWorkspaceForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _replotWorkspaceForm = null;
            _workspaceCanvas = null;
        }

        private void UpdateWindowTitle()
        {
            if (!AppServices.HasContext || AppServices.Context.Info == null)
            {
                this.Text = _appTitle;
                return;
            }

            var name = AppServices.Context.Info.ProjectName;

            this.Text = AppServices.Context.HasUnsavedChanges ? $"{name}* - {_appTitle}" : $"{name} - {_appTitle}";
        }

        private async void frmMain_Load(object sender, EventArgs e)
        {
            BuildRecentProjectsMenu();
            // If launched by double-clicking a .lpp file — open it
            if (!string.IsNullOrEmpty(_startupFilePath) && File.Exists(_startupFilePath)
                && _startupFilePath.EndsWith(".lpp", StringComparison.OrdinalIgnoreCase))
            {
                await OpenProjectInternalAsync(
                    _startupFilePath,
                    checkUnsavedChanges: false);
                return;
            }

        }

        private static Cursor LoadPanCursor()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Resources", "Cursors", "hand_pan.cur"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", "Cursors", "Pan_hand.cur"))
            };

            foreach (var cursorPath in candidates)
            {
                if (File.Exists(cursorPath))
                {
                    return new Cursor(cursorPath);
                }
            }

            return Cursors.Hand;
        }

        // ── FORM CLOSING ─────────────────────────────

        // Handles form closing event
        private void frmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!HandleUnsavedChangesOnClose())
                e.Cancel = true;
        }

        // Returns true if safe to close
        // Returns false if user clicked Cancel
        private bool HandleUnsavedChangesOnClose()
        {
            if (!AppServices.HasContext ||
                !AppServices.Context.HasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "You have unsaved changes.\n\n" +
                "Save before closing?",
                "Unsaved Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                return SaveCurrentProject();

            if (result == DialogResult.No)
            {
                string filePath = AppServices.Context.ProjectFilePath;

                // Step 1 — Checkpoint BEFORE closing the connection.
                //   TRUNCATE empties the WAL file in-place while we still
                //   hold the connection, so there is no file-lock conflict.
                //   An empty WAL cannot be replayed on next open.
                ProjectWalCheckpoint.Execute(filePath);

                // Step 2 — Dispose session and clear the connection pool.
                //   ClearAllPools() forces Microsoft.Data.Sqlite to close
                //   every pooled connection immediately, releasing the OS
                //   file handles on .lpp, .lpp-wal and .lpp-shm.
                AppServices.Context.Session.Dispose();
                AppServices.ClearContext();
                SqliteConnection.ClearAllPools();

                // Step 3 — Restore .lpp from the last backup.
                if (!_backupService.RollbackToLatest(filePath))
                    MessageBox.Show(
                        "Could not restore the project to its last saved state — " +
                        "no backup file was found.",
                        "Restore Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                return true;
            }

            // Cancel — do not close
            return false;
        }



        /// <summary>
        /// Enables project-scoped commands once a project context has been created or opened.
        /// </summary>
        private void EnableProjectMenuItems()
        {
            dataToolStripMenuItem.Enabled = true;
            contributionToolStripMenuItem.Enabled = false;
            replottingToolStripMenuItem.Enabled = true;
            validationToolStripMenuItem.Enabled = false;
            reportsToolStripMenuItem.Enabled = false;

            tsmSave.Enabled = true;
            mnuSaveProject.Enabled = true;
            tsmSaveAs.Enabled = true;
            mnuSaveAsProject.Enabled = true;
            tsmProjectInformation.Enabled = true;
            mnuProjectInfo.Enabled = true;
            tsmProjectSetting.Enabled = true;
            mnuProjectSettings.Enabled = true;
            tsmCloseProject.Enabled = true;
            mnuCloseProject.Enabled = true;
            tsmBackupProject.Enabled = true;
            mnuBackup.Enabled = true;
            tsmRestoreBackup.Enabled = true;
            mnuRestoreBackup.Enabled = true;
            ImportParcelOwnerShipRecords.Enabled = true;
            viewEditRecordToolStripMenuItem.Enabled = true;
            landOwnerDataToolStripMenuItem.Enabled = true;
            startReplotWorkspaceToolStripMenuItem.Enabled = true;
            mnuUndo.Enabled = false;
            mnuRedo.Enabled = false;
            mnuPan.Enabled = true;
            mnuZoomIn.Enabled = true;
            mnuZoomOut.Enabled = true;
            mnuZoomExtent.Enabled = true;
            mnuZoomWindow.Enabled = true;
            toolStripComboBox1.Enabled = true;
        }

        /// <summary>
        /// Disables project-scoped commands when no project is active, preventing forms from opening without a project database.
        /// </summary>
        private void DisableProjectMenuItems()
        {
            dataToolStripMenuItem.Enabled = false;
            contributionToolStripMenuItem.Enabled = false;
            replottingToolStripMenuItem.Enabled = false;
            validationToolStripMenuItem.Enabled = false;
            reportsToolStripMenuItem.Enabled = false;

            tsmSave.Enabled = false;
            mnuSaveProject.Enabled = false;
            tsmSaveAs.Enabled = false;
            mnuSaveAsProject.Enabled = false;
            tsmProjectInformation.Enabled = false;
            mnuProjectInfo.Enabled = false;
            tsmProjectSetting.Enabled = false;
            mnuProjectSettings.Enabled = false;
            tsmCloseProject.Enabled = false;
            mnuCloseProject.Enabled = false;
            tsmBackupProject.Enabled = false;
            mnuBackup.Enabled = false;
            tsmRestoreBackup.Enabled = false;
            mnuRestoreBackup.Enabled = false;
            ImportParcelOwnerShipRecords.Enabled = false;
            viewEditRecordToolStripMenuItem.Enabled = false;
            landOwnerDataToolStripMenuItem.Enabled = false;
            startReplotWorkspaceToolStripMenuItem.Enabled = false;
            mnuUndo.Enabled = false;
            mnuRedo.Enabled = false;
            mnuPan.Enabled = false;
            mnuZoomIn.Enabled = false;
            mnuZoomOut.Enabled = false;
            mnuZoomExtent.Enabled = false;
            mnuZoomWindow.Enabled = false;
            toolStripComboBox1.Enabled = false;
        }

        // ── NEW PROJECT ──────────────────────────────

        private async void tsmNewProject_Click(object sender, EventArgs e)
        {
            if (AppServices.HasContext && AppServices.Context.HasUnsavedChanges)
            // If there is any project open and it has unsaved changes then ....
            {
                var result = MessageBox.Show(
                    "Do you want to save this project before creating new one?",
                    "Save Current Project",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await SaveCurrentProjectAsync(showMessage: false);
                    //await CloseCurrentProjectAsync();
                }
            }

            using SaveFileDialog sfd = new()
            {
                Filter =
                    "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Create New Project"
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            await CloseCurrentProjectAsync();
            string filePathFromDialog = sfd.FileName;
            string projectFileName = Path.GetFileNameWithoutExtension(filePathFromDialog);
            string projectFolder = Path.Combine(Path.GetDirectoryName(filePathFromDialog)!, projectFileName);
            string projectFilePath = Path.Combine(projectFolder, Path.GetFileName(filePathFromDialog));

            try
            {
                // Create folder structure
                Directory.CreateDirectory(projectFolder);
                ProjectFolderCreator.CreateFolders(projectFolder);

                // Create session and context
                var session = _sessionFactory.CreateSession(projectFilePath);
                var context = new ProjectContext(session, projectFilePath);

                // Register in AppServices
                AppServices.SetContext(context);

                // Subscribe to state changes
                context.StateChanged += UpdateWindowTitle;

                // Create project in database
                var info = await _projectService.CreateNewProjectAsync(projectFilePath, projectFileName);

                // Set info on context
                context.SetInfo(info);

                // Enable menu items
                EnableProjectMenuItems();
                UpdateWindowTitle();

                // Open project details form
                OpenProjectDetails();
                PromptProjectSettings();
                InitializeProjectWorkspace();
                await ApplySettingsAsync();
                await RefreshLayerTreeAsync();
            }
            catch (Exception ex)
            {
                AppServices.ClearContext();
                MessageBox.Show(
                    $"Failed to create project: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool SaveCurrentProject()
        {
            if (!AppServices.HasContext) return true;

            try
            {
                string filePath = AppServices.Context.ProjectFilePath;

                // Repositories write immediately — no pending changes to flush.
                // Save = WAL checkpoint + backup rotation.

                // 1. WAL checkpoint — merge WAL journal into the .lpp file
                ProjectWalCheckpoint.Execute(filePath);

                // 2. Rotate backups
                _backupService.CreateBackup(filePath);

                // 3. Mark clean
                AppServices.Context.MarkAsSaved();
                return true;
            }
            catch (Exception ex)
            {
                LogProjectError("Project save failed.", ex);

                MessageBox.Show(
                    $"Save failed: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
        private async void PromptProjectSettings()
        {
            var result = MessageBox.Show(
                "Would you like to configure " +
                "project settings now?\n\n" +
                "You can always change settings later " +
                "from Project → Project Settings.",
                "Project Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.Yes)
            {
                OpenProjectSettings();
            }


            // Mark as configured regardless of choice —
            // prevents this prompt from showing again on next open.
            var repo = _projectScopedFactory.CreateProjectSettingsRepository(
                AppServices.Context.Session);
            await repo.MarkAsConfiguredAsync();

            await ApplySettingsAsync();

            // Create the very first project backup.
            // This happens AFTER project info + settings are saved,
            // so the .bak captures the correct initial project state.
            // All future discards will restore to this checkpoint.
            if (AppServices.HasContext)
            {
                var filePath = AppServices.Context.ProjectFilePath;

                // WAL checkpoint — flush all writes into the .lpp file
                await ProjectWalCheckpoint.ExecuteAsync(filePath);

                // Create .lpp.bak — this IS the initial saved state
                _backupService.CreateBackup(filePath);

                // Mark as saved — no asterisk on a fresh project
                AppServices.Context.MarkAsSaved();
            }
        }

        private async void OpenProjectSettings()
        {
            if (!AppServices.HasContext) return; // Return if there is no Project Open. ((Has context --> the project is open))

            var context = AppServices.Context;
            var repo = _projectScopedFactory.CreateProjectSettingsRepository(context.Session);
            var crsRepo = _projectScopedFactory.CreateCoordinateSystemRepository(context.Session);
            var datumRepo = _projectScopedFactory.CreateDatumTransformationRepository(context.Session);
            var service = _projectScopedFactory.CreateProjectSettingsService(context.Session);

            using var frm = new frmProjectSettings(service, crsRepo, datumRepo);
            var appliedWhileOpen = false;

            frm.SettingsApplied += (_, _) =>
            {
                appliedWhileOpen = true;
                AppServices.Context.MarkAsModified();
                _ = ApplySettingsAsync();
            };

            if (frm.ShowDialog() == DialogResult.OK && !appliedWhileOpen)
            {
                AppServices.Context.MarkAsModified();
                await ApplySettingsAsync();
            }
        }

        private async Task ApplySettingsAsync()
        {
            if (!AppServices.HasContext) return;

            try
            {
                var repo = _projectScopedFactory.CreateProjectSettingsRepository(
                    AppServices.Context.Session);
                var settings = await repo
                    .GetProjectSettingsAsync();

                if (settings == null) return;

                var bgColor = ParseColorOrDefault(
                    settings.CanvasBackgroundColor, Color.White);
                var gridColor = ParseColorOrDefault(
                    settings.CanvasGridColor, Color.LightGray);

                mapCanvasControlMain.ApplyRenderSettings(
                    MapCanvasSettingsService.FromProjectSettings(settings));
                mapCanvasControlMain.ApplySnapEnabled(settings.SnapEnabled);

                if (_workspaceCanvas != null && !_workspaceCanvas.IsDisposed)
                {
                    _workspaceCanvas.ApplyBackgroundColor(bgColor);
                    _workspaceCanvas.ApplyGridColor(gridColor);
                    _workspaceCanvas.ApplyGridVisible(
                        settings.CanvasGridVisible);
                    _workspaceCanvas.ApplySnapEnabled(
                        settings.SnapEnabled);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ApplyCanvasSettings failed: {ex.Message}");
            }
        }

        private static Color ParseColorOrDefault(string? htmlColor, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(htmlColor))
            {
                return fallback;
            }

            try
            {
                return ColorTranslator.FromHtml(htmlColor);
            }
            catch
            {
                return fallback;
            }
        }
        // ── CLOSE PROJECT ────────────────────────────

        private async Task CloseCurrentProjectAsync()
        {
            if (!AppServices.HasContext) return;

            // HandleUnsavedChangesOnClose may already
            // dispose session if user clicked No
            if (!HandleUnsavedChangesOnClose()) return;

            if (AppServices.HasContext)
            {
                AppServices.Context.StateChanged
                    -= UpdateWindowTitle;
                AppServices.ClearContext();
            }

            UnloadProjectWorkspace();
            DisableProjectMenuItems();
            UpdateWindowTitle();
        }



        // ── PROJECT DETAILS ──────────────────────────

        private void tsmProjectInformation_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;
            OpenProjectDetails();
        }

        // Opens project details form
        // Wires service via DI
        private void OpenProjectDetails()
        {
            var context = AppServices.Context;
            var service = _projectScopedFactory.CreateProjectInfoService(context.Session);

            using var frm = new frm_ProjectDetails(service);
            if (frm.ShowDialog() == DialogResult.OK)
                AppServices.Context.MarkAsModified();

            // Refresh title after user edits
            UpdateWindowTitle();
        }

        // ── EXIT ─────────────────────────────────────

        private void ExitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to exit?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
                == DialogResult.No)
                return;

            if (!HandleUnsavedChangesOnClose()) return;

            Close();
        }

        // ── STUB HANDLERS ────────────────────────────
        // Implemented later one by one
        private void tsmSave_Click(object sender, EventArgs e)
        {
            _ = SaveCurrentProjectAsync(showMessage: false);
        }
        // ── SAVE AS ──────────────────────────────────
        // Saves current project to a new location.
        // Copies entire project folder to destination.
        // Switches session to new location.

        //    private async void tsmSaveAs_Click(
        //object sender, EventArgs e)
        //    {
        //        if(!AppServices.HasContext) return;
        //        string currentFilePath = AppServices.Context.ProjectFilePath;
        //        string currentFolderPath = Path.GetFullPath(Path.GetDirectoryName(currentFilePath)!);
        //        var sfd = new SaveFileDialog()
        //        {
        //            Filter =
        //                "Land Pooling Project File (*.lpp)|*.lpp",
        //            Title = "Save Project As",
        //            FileName = Path.GetFileName(currentFilePath)
        //        };

        //        if(sfd.ShowDialog() != DialogResult.OK)
        //            return;

        //        string fullDestFilePath = sfd.FileName;
        //        string fullDestFolderPath = Path.GetFullPath(Path.GetDirectoryName(fullDestFilePath)!);

        //        if(fullDestFolderPath.Equals(Path.GetDirectoryName(currentFolderPath), StringComparison.OrdinalIgnoreCase)|| fullDestFolderPath.Equals()
        //        {
        //            MessageBox.Show(
        //                "Selected file is the same as current project file.",
        //                "Save As",
        //                MessageBoxButtons.OK,
        //                MessageBoxIcon.Warning);
        //            return;
        //        }

        //    }

        private async void tsmSaveAs_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            var currentFilePath = AppServices.Context.ProjectFilePath; //C:\Projects\MyProject\Myproject.lpp
            var currentFileName = Path.GetFileNameWithoutExtension(currentFilePath); // MyProject
            var currentFolder = Path.GetFullPath(
                                        Path.GetDirectoryName(currentFilePath)!);
            // currentFolder = C:\Projects\MyProject\

            // ── STEP 1 — Open SaveFileDialog ─────────

            string pickedFilePath = string.Empty;

            string pickedFolder = string.Empty;
            string pickedFileName = string.Empty;
            string destFolder = string.Empty;
            string destFile = string.Empty;
            string destFileName = string.Empty;
            while (true)
            {
                using var sfd = new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(currentFolder),
                    Filter = "Land Pooling Project File (*.lpp)|*.lpp",
                    Title = "Save Project As",
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath)
                };
                if (sfd.ShowDialog() != DialogResult.OK) return;

                pickedFilePath = sfd.FileName; //C:\Projects\MyProject2.lpp
                                               // pickedFilePath = wherever user chose

                pickedFolder = Path.GetFullPath(
                                            Path.GetDirectoryName(pickedFilePath)!); //C:\Projects\
                pickedFileName = Path.GetFileNameWithoutExtension(pickedFilePath); // MyProject2
                destFolder = Path.Combine(pickedFolder, pickedFileName); //C:\Projects\MyProject2
                destFile = Path.Combine(destFolder, Path.GetFileName(pickedFilePath)); //C:\Projects\MyProject2\MyProject2.lpp
                                                                                       // pickedFolder = folder user browsed to
                destFileName = Path.GetFileNameWithoutExtension(destFile); // MyProject
                // ── STEP 2 — Block saving inside current project folder ──
                // currentFolder = C:\Projects\MyProject\
                // If user picked something inside it → block
                if (string.Equals(destFile, currentFilePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await SaveCurrentProjectAsync(showMessage: false);
                    return;
                }
                else if (destFile.StartsWith(currentFileName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Cannot save inside the current project folder.\n\n" +
                        "Please choose a different location.",
                        "Invalid Location",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    continue;
                }
                else break;
            }
            // ── STEP 3 — Same parent directory → save normally ───
            // If user is browsing the same parent folder
            // where current project folder lives → just save
            // Example:
            // currentFolder = C:\Projects\MyProject\
            // parent        = C:\Projects\
            // pickedFolder  = C:\Projects\  → save normally


            // ── STEP 4 — Different folder → Save As ──────────────

            try
            {
                Cursor = Cursors.WaitCursor;


                // 4a.1 — Checkpoint WAL into main .lpp file
                // SQLite WAL mode keeps pending writes in a
                // separate -wal file. Checkpoint merges them
                // into the main database file before we copy.
                await ProjectWalCheckpoint.ExecuteAsync(currentFilePath);

                // 4b — Delete destination if already exists
                if (Directory.Exists(destFolder))
                    Directory.Delete(destFolder, recursive: true);

                // 4c — Copy entire project folder to new location
                CopyProjectFolder(currentFolder, destFolder);

                // 4d — Rename copied .lpp file inside new folder
                //      to match new project name
                string copiedFile = Path.Combine(
                    destFolder,
                    Path.GetFileName(currentFilePath));

                if (File.Exists(copiedFile) &&
                    !string.Equals(copiedFile, destFile,
                        StringComparison.OrdinalIgnoreCase))
                {
                    File.Move(copiedFile, destFile,
                        overwrite: true);
                }

                // 4e — Close current session
                AppServices.Context.Session.Dispose();
                AppServices.ClearContext();

                // 4f — Open new session at new location
                var session = _sessionFactory.CreateSession(destFile);
                var context = new ProjectContext(
                    session, destFile);

                AppServices.SetContext(context);
                context.StateChanged += UpdateWindowTitle;

                // 4g — Update ProjectName in new database
                var info = await session.GetDbContext()
                    .ProjectInfo.FirstOrDefaultAsync();
                if (info != null)
                {
                    info.ProjectName = destFileName;
                    await session.GetDbContext().SaveChangesAsync();
                    context.SetInfo(info);
                }

                // 4h — WAL checkpoint on new file
                await ProjectWalCheckpoint.ExecuteAsync(destFile);

                // 4i — Create fresh backup in new location
                // No old backups copied — clean history
                _backupService.CreateBackup(destFile);

                // 4j — Update UI
                EnableProjectMenuItems();
                UpdateWindowTitle();
                context.MarkAsSaved();

                MessageBox.Show(
                    $"Project saved as:\n{destFile}",
                    "Save As Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogProjectError("Save As failed.", ex);

                MessageBox.Show(
                    $"Save As failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Copies all files from source to dest.
        /// Skips .bak files — new project starts
        /// with clean backup history.
        /// </summary>
        private static void CopyProjectFolder(
            string source, string dest)
        {
            string fullSource = Path.GetFullPath(source);
            string fullDest = Path.GetFullPath(dest);

            Directory.CreateDirectory(fullDest);

            // Copy directory structure first (includes empty folders)
            foreach (string dir in Directory.GetDirectories(
                fullSource, "*", SearchOption.AllDirectories))
            {
                string relativeDir = Path.GetRelativePath(
                    fullSource, dir);
                string destDir = Path.Combine(fullDest, relativeDir);
                Directory.CreateDirectory(destDir);
            }

            foreach (string file in Directory.GetFiles(
                fullSource, "*",
                SearchOption.AllDirectories))
            {
                string fullFile = Path.GetFullPath(file);

                // Skip backup files
                if (fullFile.EndsWith(".bak",
                    StringComparison.OrdinalIgnoreCase))
                    continue;

                string relativePath = Path.GetRelativePath(
                    fullSource, fullFile);
                string destFile = Path.Combine(
                    fullDest, relativePath);

                Directory.CreateDirectory(
                    Path.GetDirectoryName(destFile)!);

                File.Copy(fullFile, destFile, overwrite: true);
            }
        }
        /// <summary>
        /// Copies all files from source to dest.
        /// Skips .bak files — new project starts
        /// with clean backup history.
        /// </summary>

        /// <summary>
        /// Copies all files from source to dest.
        /// Skips .bak files — new project starts
        /// with clean backup history.
        /// Also skips files inside dest to prevent
        /// infinite copy if dest is inside source.
        /// </summary>

        /// <summary>
        /// Updates ProjectName in the copied database
        /// to match the new file name chosen by user.
        /// </summary>
        private static async Task UpdateProjectNameAsync(
            ProjectSession session, string newName)
        {
            try
            {
                var info = await session
                    .GetDbContext()
                    .ProjectInfo
                    .FirstOrDefaultAsync();

                if (info == null) return;

                info.ProjectName = newName;
                await session.GetDbContext().SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"UpdateProjectNameAsync failed: {ex.Message}");
            }
        }
        private void projectSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;
            OpenProjectSettings();
        }

        private void backupProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement backup
        }

        private void ImportParcelOwnerShipRecords_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(
                    "Please open or create a project first.",
                    "No Project Open",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var persistenceService = _projectScopedFactory.CreateImportPersistenceService(
                AppServices.Context.Session);
            using var frm = new frmImportParcelOwnershipRecords(
                AppServices.Context.ProjectFilePath,
                persistenceService);

            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();
            }
        }

        private void landOwnerDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(
                    "Please open or create a project first.",
                    "No Project Open",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var landRecordsService = _projectScopedFactory.CreateLandRecordsService(
                AppServices.Context.Session,
                AppServices.Context.ProjectFilePath);
            using var frm = new frmLandOwnersRecord(
                landRecordsService,
                AppServices.Context.ProjectFilePath);
            frm.ShowDialog(this);
        }

        private async void geotiffToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(
                    "Please open or create a project first.",
                    "No Project Open",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using OpenFileDialog dialog = new()
            {
                Title = "Import Raster Image",
                Filter =
                    "Georeferenced rasters (*.tif;*.tiff;*.vrt;*.img;*.jpg;*.jpeg;*.png;*.bmp)|*.tif;*.tiff;*.vrt;*.img;*.jpg;*.jpeg;*.png;*.bmp|" +
                    "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff|" +
                    "All files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                UseWaitCursor = true;

                var session = AppServices.Context.Session;
                var settingsRepository =
                    _projectScopedFactory.CreateProjectSettingsRepository(session);
                var coordinateSystemRepository =
                    _projectScopedFactory.CreateCoordinateSystemRepository(session);
                var datumTransformationRepository =
                    _projectScopedFactory.CreateDatumTransformationRepository(session);
                var layerRepository =
                    _projectScopedFactory.CreateCanvasLayerRepository(session);

                var settings = await settingsRepository.GetProjectSettingsAsync();
                if (settings?.CoordinateSystemId == null)
                {
                    MessageBox.Show(
                        "Please configure the project coordinate system before importing raster data.",
                        "Raster Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    OpenProjectSettings();
                    return;
                }

                var projectCoordinateSystem =
                    await coordinateSystemRepository.GetWithParametersAsync(
                        settings.CoordinateSystemId.Value);

                if (projectCoordinateSystem == null)
                    throw new InvalidOperationException(
                        "The configured project coordinate system could not be loaded.");

                Land_Readjustment_Tool.Core.Entities.Spatial.DatumTransformation?
                    datumTransformation = null;

                if (settings.DatumTransformationId.HasValue)
                {
                    datumTransformation =
                        await datumTransformationRepository.GetByIDAsync(
                            settings.DatumTransformationId.Value);
                }

                string targetSrsDefinition =
                    ProjectCrsWktBuilder.BuildTargetSrsDefinition(
                        projectCoordinateSystem,
                        datumTransformation);

                List<CanvasLayer> existingLayers =
                    await layerRepository.GetAllOrderedAsync();

                string layerName = BuildUniqueLayerName(
                    Path.GetFileNameWithoutExtension(dialog.FileName),
                    existingLayers);

                RasterImportService importService = new();
                RasterImportResult importResult = await Task.Run(() =>
                    importService.ImportToProjectCrs(
                        dialog.FileName,
                        AppServices.Context.ProjectFolderPath,
                        layerName,
                        targetSrsDefinition));

                int nextDisplayOrder = existingLayers.Count == 0
                    ? 0
                    : existingLayers.Max(layer => layer.DisplayOrder) + 1;

                CanvasLayer rasterLayer = new()
                {
                    Name = layerName,
                    LayerType = CanvasLayerTreeService.RasterLayerType,
                    IsVisible = true,
                    IsLocked = false,
                    IsSelectable = true,
                    IsPrintable = true,
                    DisplayOrder = nextDisplayOrder,
                    BorderColor = "#4B8BBE",
                    LineWeight = 1.0,
                    LineStyle = "Solid",
                    FillTransparency = 0,
                    FillStyle = "None",
                    LabelColor = "#000000",
                    PointSymbol = "Circle",
                    PointSize = 5.0,
                    SourceFile = importResult.RelativePath,
                    ImportedDate = DateTime.Now,
                    Description = importResult.SourceMetadata.ToLayerDescription(
                        layerName,
                        importResult.RelativePath,
                        projectCoordinateSystem.Code,
                        importResult.ImportMode)
                };

                CanvasLayer savedLayer = await layerRepository.AddAsync(rasterLayer);

                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();

                await RefreshLayerTreeAsync();
                SelectLayerNodeById(savedLayer.Id);
                mapCanvasControlMain.ZoomExtents();

                string importDetails = importResult.SourceMetadata.ToDisplayText(
                    layerName,
                    importResult.RelativePath,
                    projectCoordinateSystem.Code,
                    importResult.ImportMode);

                AppServices.Context.Session.Logger.LogInfo(
                    $"Raster import metadata:{Environment.NewLine}{importDetails}");

                string importHeading = importResult.ImportMode switch
                {
                    RasterImportMode.ProjectedToProjectCrs =>
                        $"Imported '{layerName}' successfully.",
                    RasterImportMode.UnknownCrsCopiedWithoutProjection =>
                        $"Imported '{layerName}', but the raster CRS is unknown.",
                    RasterImportMode.UnreferencedCopiedToLocalCoordinates =>
                        $"Imported '{layerName}' for temporary display only.",
                    _ => $"Imported '{layerName}'."
                };

                string? importWarning = GetRasterImportWarning(importResult.ImportMode);
                MessageBoxIcon importIcon =
                    importResult.ImportMode == RasterImportMode.ProjectedToProjectCrs
                        ? MessageBoxIcon.Information
                        : MessageBoxIcon.Warning;
                string messageBody = importWarning == null
                    ? importDetails
                    : $"{importWarning}{Environment.NewLine}{Environment.NewLine}" +
                      "The raster was still imported so you can inspect it, but it will not align correctly until it is georeferenced or assigned the correct CRS." +
                      $"{Environment.NewLine}{Environment.NewLine}{importDetails}";

                MessageBox.Show(
                    $"{importHeading}{Environment.NewLine}{Environment.NewLine}" +
                    messageBody,
                    importWarning == null
                        ? "Raster Import Details"
                        : "Raster Georeferencing Warning",
                    MessageBoxButtons.OK,
                    importIcon);
            }
            catch (Exception ex)
            {
                string exceptionDetails = FormatExceptionDetails(ex);
                System.Diagnostics.Debug.WriteLine(exceptionDetails);
                AppServices.Context.Session.Logger.LogError(
                    $"Raster import failed.{Environment.NewLine}{exceptionDetails}",
                    ex);

                MessageBox.Show(
                    $"Failed to import raster: {GetMostUsefulExceptionMessage(ex)}",
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private void startReplotWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(
                    "Please open or create a project first.",
                    "No Project Open",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (_replotWorkspaceForm == null || _replotWorkspaceForm.IsDisposed)
            {
                _replotWorkspaceForm = new frmReplotWorkspace();
                _replotWorkspaceForm.FormClosed += ReplotWorkspaceForm_FormClosed;
                _replotWorkspaceForm.Show(this);
            }
            else
            {
                if (_replotWorkspaceForm.WindowState == FormWindowState.Minimized)
                {
                    _replotWorkspaceForm.WindowState = FormWindowState.Normal;
                }

                _replotWorkspaceForm.BringToFront();
                _replotWorkspaceForm.Activate();
            }

            _workspaceCanvas = _replotWorkspaceForm.CanvasControl;
            _ = ApplySettingsAsync();
        }

        private void AreaConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: implement area converter
        }

        private async void tsmOpenProject_Click(object sender, EventArgs e)
        {
            if (AppServices.HasContext && AppServices.Context.HasUnsavedChanges)
            {
                {
                    var result = MessageBox.Show(
                        "Do you want to save current project before opening another one?",
                        "Save Current Project",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        await SaveCurrentProjectAsync(showMessage: false);
                    }
                }
            }

            using OpenFileDialog ofd = new()
            {
                Filter =
                    "Land Pooling Project File (*.lpp)|*.lpp",
                Title = "Open Project"
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string projectFilePath = ofd.FileName;

            if (!File.Exists(projectFilePath))
            {
                MessageBox.Show(
                    "Selected project file was not found.",
                    "Open Project",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }
            await CloseCurrentProjectAsync();

            await OpenProjectInternalAsync(projectFilePath, checkUnsavedChanges: true);
        }


        private async Task OpenProjectInternalAsync(
            string projectFilePath,
            bool checkUnsavedChanges)
        {
            if (checkUnsavedChanges &&
                AppServices.HasContext &&
                !HandleUnsavedChangesOnClose())
                return;

            ProjectSession? session = null;
            try
            {
                if (AppServices.HasContext)
                    AppServices.Context.StateChanged -= UpdateWindowTitle;

                session = _sessionFactory.CreateSession(projectFilePath);

                // Ensure database schema is up to date
                await session.GetDbContext().Database.MigrateAsync();

                var context = new ProjectContext(
                    session, projectFilePath);

                var service = _projectScopedFactory.CreateProjectInfoService(session);

                var info = await service.GetAsync();
                if (info == null)
                    throw new InvalidOperationException(
                        "Project file is invalid or missing project information.");

                context.SetInfo(info);
                RecentProjectsManager.AddRecentProject(projectFilePath); //ADDS TO THE RECENTLY OPENED PROJECTS IN SETTINGS
                BuildRecentProjectsMenu(); // Refresh menu to show the newly added recent project immediately
                context.StateChanged += UpdateWindowTitle;
                AppServices.SetContext(context);

                EnableProjectMenuItems();
                UpdateWindowTitle();
                InitializeProjectWorkspace();
                await ApplySettingsAsync();
                await RefreshLayerTreeAsync();
            }
            catch (Exception ex)
            {
                session?.Dispose();
                MessageBox.Show(
                    $"Failed to open project: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task<bool> SaveCurrentProjectAsync(bool showMessage)
        {
            if (!AppServices.HasContext) return true;

            if (!AppServices.Context.HasUnsavedChanges)
            {
                if (showMessage)
                    MessageBox.Show(
                        "No changes to save.",
                        "Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                return true;
            }

            try
            {
                string filePath = AppServices.Context.ProjectFilePath;

                // Repositories write to the DB immediately on every edit,
                // so there are no pending EF Core changes to flush here.
                // Save = WAL checkpoint (merge WAL → .lpp) + rotate backups.

                // 1. WAL checkpoint — merge WAL journal into the .lpp file
                await ProjectWalCheckpoint.ExecuteAsync(filePath);

                // 2. Rotate backups — .bak = state just before this save
                _backupService.CreateBackup(filePath);

                // 3. Mark clean
                AppServices.Context.MarkAsSaved();

                if (showMessage)
                    MessageBox.Show(
                        "Project saved successfully.",
                        "Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                return true;
            }
            catch (Exception ex)
            {
                LogProjectError("Project save failed.", ex);

                MessageBox.Show(
                    $"Save failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(
                    destinationDir,
                    Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(
                    destinationDir,
                    Path.GetFileName(directory));
                CopyDirectory(directory, destSubDir);
            }
        }

        private void tsmProjectSetting_Click(object sender, EventArgs e)
        {
            OpenProjectSettings();
        }

        private void lblTransparency_Click(object sender, EventArgs e)
        {

        }

        private async void tsmBackupProject_Click(
    object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            try
            {
                // Save first then backup
                var saved = await SaveCurrentProjectAsync(
                    showMessage: false);
                if (!saved) return;

                MessageBox.Show(
                    "Project backed up successfully.",
                    "Backup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Backup failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void tsmRestoreBackup_Click(
    object sender, EventArgs e)
        {
            if (!AppServices.HasContext) return;

            string filePath = AppServices.Context
                .ProjectFilePath;

            var backups = _backupService.GetBackups(filePath);

            if (backups.Count == 0)
            {
                MessageBox.Show(
                    "No backup files found for this project.",
                    "Restore",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using var frm = new frmBackupManager(
                filePath, backups);

            if (frm.ShowDialog() != DialogResult.OK) return;

            try
            {
                // Close current session before restore
                AppServices.Context.Session.Dispose();
                AppServices.ClearContext();

                // Restore selected backup
                _backupService.RestoreFromBackup(
                    filePath, frm.SelectedBackupPath!);

                // Reopen project from restored file
                await OpenProjectInternalAsync(
                    filePath, checkUnsavedChanges: false);

                MessageBox.Show(
                    "Project restored successfully.",
                    "Restore Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Restore failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void tsmCloseProject_Click(object sender, EventArgs e)
        {
            await CloseCurrentProjectAsync();
        }

        // 1. When File menu opens — rebuild the list
        private void tsmFile_DropDownOpening(object? sender, EventArgs e)
        {
            BuildRecentProjectsMenu();
        }
        /// <summary>
        /// Fires when user clicks a recent project menu item.
        ///
        /// Flow:
        ///   1. Validate the file still exists on disk
        ///   2. Check if the same project is already open — skip if so
        ///   3. Pass to OpenProjectInternalAsync(checkUnsavedChanges: true)
        ///      which internally calls HandleUnsavedChangesOnClose()
        ///      → Yes   = SaveCurrentProjectAsync then open new
        ///      → No    = ChangeTracker.Clear() then open new
        ///      → Cancel = abort, stay on current project
        /// </summary>
        private async void RecentProjectItem_Click(
            object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem item) return;
            if (item.Tag is not string path) return;

            // ── Step 1: File still exists on disk? ───────────────────
            if (!File.Exists(path))
            {
                MessageBox.Show(
                    $"Project file not found:\n\n{path}\n\n" +
                    "It may have been moved or deleted.\n" +
                    "It will be removed from the recent list.",
                    "File Not Found",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                RecentProjectsManager.RemoveRecentProject(path);
                BuildRecentProjectsMenu();
                return;
            }

            // ── Step 2: Is this project already open? ────────────────
            // No point closing and reopening the same file
            if (AppServices.HasContext &&
                string.Equals(
                    AppServices.Context.ProjectFilePath,
                    path,
                    StringComparison.OrdinalIgnoreCase))
            {
                // Already open — just bring focus to main window
                this.Activate();
                return;
            }

            // ── Step 3: Confirm switching to selected recent project ─
            if (AppServices.HasContext)
            {
                var confirm = MessageBox.Show(
                    "Are you sure want to close the current project and open the selected project?\n" +
                    path,
                    "Open Recent Project",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;
            }

            // ── Step 4: Delegate to OpenProjectInternalAsync ─
            // checkUnsavedChanges: true means it will internally call
            // HandleUnsavedChangesOnClose() before doing anything.
            // That gives the user Yes / No / Cancel for unsaved changes.
            // If user clicks Cancel — nothing happens, current project stays.
            await OpenProjectInternalAsync(
                path,
                checkUnsavedChanges: true);
        }
        // ── RECENT PROJECTS MENU ─────────────────────────────────────────

        /// <summary>
        /// Call this every time the File menu opens,
        /// or after any project open/close.
        /// Rebuilds the Recent Projects submenu from Settings.
        /// </summary>
        private void BuildRecentProjectsMenu()
        {
            tsmRecentProjects.DropDownItems.Clear();

            var paths = RecentProjectsManager.GetRecentProjects();

            if (paths.Count == 0)
            {
                // Show a disabled placeholder when list is empty
                tsmRecentProjects.DropDownItems.Add(
                    new ToolStripMenuItem("(No recent projects)")
                    {
                        Enabled = false
                    });
            }
            else
            {
                // Add one menu item per recent file
                for (int i = 0; i < paths.Count; i++)
                {
                    string path = paths[i];
                    string name = Path.GetFileNameWithoutExtension(path);
                    string folder = Path.GetDirectoryName(path) ?? string.Empty;

                    // Format: "1.  Ward5  —  C:\Projects\Ward5\"
                    string label =
                        $"{i + 1}.  {name}  —  {folder}";

                    var item = new ToolStripMenuItem(label)
                    {
                        Tag = path,
                        ToolTipText = path   // show full path on hover
                    };

                    item.Click += RecentProjectItem_Click;
                    tsmRecentProjects.DropDownItems.Add(item);
                }

                // Separator then Clear option
                tsmRecentProjects.DropDownItems.Add(
                    new ToolStripSeparator());

                var clearItem = new ToolStripMenuItem("Clear Recent Projects");
                clearItem.Click += (s, e) =>
                {
                    RecentProjectsManager.ClearRecentProjects();
                    BuildRecentProjectsMenu();
                };
                tsmRecentProjects.DropDownItems.Add(clearItem);
            }
        }

        private Image? LoadToolbarImage(string fileName)
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Resources", "For RePlot Application", fileName),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Resources", "For RePlot Application", fileName)),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", "For RePlot Application", fileName))
            };

            foreach (var imgPath in candidates)
            {
                if (File.Exists(imgPath))
                {
                    return Image.FromFile(imgPath);
                }
            }

            return null;
        }

        private void tsmExpandCollapseLeftPanel_Click(object sender, EventArgs e)
        {
            if (tsmExpandCollapseLeftPanel.Checked)
            {
                mainSplitContainer.Panel1Collapsed = false;
                tsmExpandCollapseLeftPanel.Text = "Collapse Left Panel";
                var img = LoadToolbarImage("icons8-close-left-pane-50.png");
                if (img != null) tsmExpandCollapseLeftPanel.Image = img;
            }
            else
            {
                mainSplitContainer.Panel1Collapsed = true;
                tsmExpandCollapseLeftPanel.Text = "Expand Left Panel";
                var img = LoadToolbarImage("icons8-open-left-pane-50.png");
                if (img != null) tsmExpandCollapseLeftPanel.Image = img;
            }
        }
        // ── PROJECT FOLDER CREATOR ───────────────────

        // Creates standard folder structure
        // for a new project
        public static class ProjectFolderCreator
        {
            public static void CreateFolders(string root)
            {
                string[] folders =
                {
            "Maps",
            "GIS",
            "Documents",
            "Reports",
            "Exports/Excel",
            "Images/LandOwners Certificate",
            "Images/Cadastral Sheets",
            "Images/Land Owners Photos",
            "Temp",
            "Logs"
        };

                foreach (var folder in folders)
                    Directory.CreateDirectory(Path.Combine(root, folder));
            }
        }

        private void tsmExpandCollapseRightPanel_Click(object sender, EventArgs e)
        {
            if (tsmExpandCollapseRightPanel.Checked)
            {
                splitContainer3.Panel2Collapsed = false;
                tsmExpandCollapseRightPanel.Text = "Collapse Right Panel";
                var img = LoadToolbarImage("icons8-close-right-pane-50.png");
                if (img != null) tsmExpandCollapseRightPanel.Image = img;
            }
            else
            {
                splitContainer3.Panel2Collapsed = true;
                tsmExpandCollapseRightPanel.Text = "Expand Right Panel";
                var img = LoadToolbarImage("icons8-open-right-pane-50.png");
                if (img != null) tsmExpandCollapseRightPanel.Image = img;
            }
        }

        /// <summary>
        /// Opens the original parcel ownership records form for the active project.
        /// </summary>
        private void viewEditRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AppServices.HasContext)
            {
                MessageBox.Show(
                    "Please open or create a project first.",
                    "No Project Open",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var landRecordsService = _projectScopedFactory.CreateLandRecordsService(
                AppServices.Context.Session,
                AppServices.Context.ProjectFilePath);
            using var frm = new frmLandParcelOwnersRecord(
                landRecordsService,
                AppServices.Context.ProjectFilePath);
            frm.ShowDialog(this);
        }

        private void mnuSaveAsProject_Click(object sender, EventArgs e)
        {

        }

        private void tsmRecentProjects_Click(object sender, EventArgs e)
        {

        }

        private void mnuPan_Click(object sender, EventArgs e)
        {
            mapCanvasControlMain.SetPanToolActive(mnuPan.Checked);
        }

        private void mnuZoomIn_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            mapCanvasControlMain.SetPanToolActive(false);
            mapCanvasControlMain.ZoomIn();
        }

        private void mnuZoomOut_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            mapCanvasControlMain.SetPanToolActive(false);
            mapCanvasControlMain.ZoomOut();
        }

        private void mnuZoomExtent_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            mapCanvasControlMain.SetPanToolActive(false);
            mapCanvasControlMain.ZoomExtents();
        }

        private void mnuZoomWindow_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            mapCanvasControlMain.BeginZoomWindow();
        }

        private void mnuAreaConverterTool_Click(object sender, EventArgs e)
        {
            try
            {


                if (_areaConverterForm == null || _areaConverterForm.IsDisposed)
                {
                    _areaConverterForm = new frmAreaConverter();
                    _areaConverterForm.FormClosed += (_, _) => _areaConverterForm = null;
                    _areaConverterForm.Show();
                }
                else
                {
                    _areaConverterForm.BringToFront();
                    _areaConverterForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open Area Converter:\n{ex.Message}",
                    "Area Converter",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void mapCanvasControlMain_Load(object sender, EventArgs e)
        {

        }

        private void ConfigureLayerTree()
        {
            treeViewLayers.CheckBoxes = false;
            treeViewLayers.DrawMode = TreeViewDrawMode.OwnerDrawText;

            treeViewLayers.HideSelection = false;
            treeViewLayers.ShowLines = true;
            treeViewLayers.ShowRootLines = true;
            treeViewLayers.ShowPlusMinus = true;
            treeViewLayers.FullRowSelect = false;
            treeViewLayers.HotTracking = false;

            treeViewLayers.ImageList = null;
            treeViewLayers.StateImageList = null;
            treeViewLayers.ItemHeight = 24;
            treeViewLayers.Font = new Font("Segoe UI", 9F);

            treeViewLayers.DrawNode += treeViewLayers_DrawNode;
            treeViewLayers.NodeMouseClick += treeView1_NodeMouseClick;
            treeViewLayers.NodeMouseDoubleClick += treeViewLayers_NodeMouseDoubleClick;
            treeViewLayers.KeyDown += treeView1_KeyDown;
            treeViewLayers.AfterSelect += treeView1_AfterSelect;

            ResetLayerTree();
        }

        private void ConfigureLayerPropertiesPanel()
        {
            grpProperties.Text = "Properties";
        }

        private void treeViewLayers_DrawNode(object? sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node == null) return;

            bool selected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;

            LayerTreeNodeState? layerState = e.Node.Tag as LayerTreeNodeState;
            CanvasLayer? activeLayer = layerState?.IsLayerNode == true
                ? layerState.Layer
                : null;
            bool isLayer = activeLayer != null;

            Graphics g = e.Graphics;
            g.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Color backColor = selected
                ? SystemColors.Highlight
                : treeViewLayers.BackColor;

            Color textColor = selected
                ? SystemColors.HighlightText
                : treeViewLayers.ForeColor;

            Rectangle rowRect = new Rectangle(
                e.Bounds.X,
                e.Bounds.Y,
                treeViewLayers.ClientSize.Width - e.Bounds.X,
                e.Bounds.Height);

            using (SolidBrush backBrush = new(backColor))
                g.FillRectangle(backBrush, rowRect);

            int x = e.Bounds.X;

            if (isLayer)
            {
                CanvasLayer layer = activeLayer!;

                Rectangle chkRect = GetLayerNodeCheckBoxRect(e.Node);

                CheckBoxRenderer.DrawCheckBox(
                    g,
                    chkRect.Location,
                    layer.IsVisible
                        ? VisualStyles.CheckBoxState.CheckedNormal
                        : VisualStyles.CheckBoxState.UncheckedNormal);

                x = chkRect.Right + LayerNodeCheckBoxGap;

                Color layerColor = ParseColorOrDefault(
                    layer.BorderColor,
                    Color.Black);

                Rectangle colorRect = GetLayerNodeColorRect(e.Node);

                using (SolidBrush colorBrush = new(layerColor))
                    g.FillRectangle(colorBrush, colorRect);

                using (Pen borderPen = new(Color.FromArgb(60, 60, 60)))
                    g.DrawRectangle(borderPen, colorRect);

                x = colorRect.Right + LayerNodeColorBoxGap;
            }

            TextRenderer.DrawText(
                g,
                e.Node.Text,
                treeViewLayers.Font,
                new Rectangle(
                    x,
                    e.Bounds.Y,
                    treeViewLayers.ClientSize.Width - x,
                    e.Bounds.Height),
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix);
        }

        private TreeNode CreateLayerGroupNode(string key, string text)
        {
            return new TreeNode(text)
            {
                Name = $"{LayerGroupNodeNamePrefix}{key}",
                Tag = new LayerTreeNodeState
                {
                    IsLayerNode = false
                }
            };
        }

        private TreeNode CreateLayerNode(CanvasLayer layer)
        {
            return new TreeNode(layer.Name)
            {
                Name = $"Layer_{layer.Id}",
                Tag = new LayerTreeNodeState
                {
                    IsLayerNode = true,
                    Layer = layer
                }
            };
        }

        private async void treeView1_NodeMouseClick(
            object? sender,
            TreeNodeMouseClickEventArgs e)
        {
            if (_suppressLayerTreeEvents || e.Node == null)
                return;

            treeViewLayers.SelectedNode = e.Node;

            if (e.Node.Tag is not LayerTreeNodeState state ||
                !state.IsLayerNode)
                return;

            Rectangle checkBoxRect = GetLayerNodeCheckBoxRect(e.Node);
            Rectangle colorRect = GetLayerNodeColorRect(e.Node);

            if (checkBoxRect.Contains(e.Location))
            {
                await ToggleLayerNodeVisibilityAsync(e.Node);
                return;
            }

            if (colorRect.Contains(e.Location))
                await OpenLayerPropertyManagerAsync(e.Node);
        }

        private async void treeViewLayers_NodeMouseDoubleClick(
            object? sender,
            TreeNodeMouseClickEventArgs e)
        {
            if (_suppressLayerTreeEvents || e.Node == null)
                return;

            if (e.Node.Tag is not LayerTreeNodeState state ||
                !state.IsLayerNode)
            {
                return;
            }

            if (GetLayerNodeCheckBoxRect(e.Node).Contains(e.Location))
                return;

            await OpenLayerPropertyManagerAsync(e.Node);
        }

        private async void treeView1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_suppressLayerTreeEvents || e.KeyCode != Keys.Space)
                return;

            TreeNode? selectedNode = treeViewLayers.SelectedNode;

            if (selectedNode == null)
                return;

            e.Handled = true;
            await ToggleLayerNodeVisibilityAsync(selectedNode);
        }

        private void treeView1_AfterSelect(object? sender, TreeViewEventArgs e)
        {
        }

        private async Task ToggleLayerNodeVisibilityAsync(TreeNode node)
        {
            if (_layerTreeService == null ||
                node.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsLayerNode ||
                nodeState.Layer == null)
            {
                return;
            }

            bool newVisibility = !nodeState.Layer.IsVisible;

            CanvasLayer? updatedLayer =
                await _layerTreeService.SetVisibilityAsync(
                    nodeState.Layer.Id,
                    newVisibility);

            if (updatedLayer == null)
                return;

            nodeState.Layer = updatedLayer;
            node.Text = updatedLayer.Name;

            treeViewLayers.Invalidate();
            UpdateRasterCanvasLayersFromTree();

            if (AppServices.HasContext)
            {
                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();
            }

            mapCanvasControlMain.RequestRender();
        }

        private async Task RefreshLayerTreeAsync()
        {
            if (!AppServices.HasContext || _layerTreeService == null)
            {
                ResetLayerTree();
                return;
            }

            try
            {
                IReadOnlyList<CanvasLayerTreeGroup> layerGroups =
                    await _layerTreeService.GetLayerTreeAsync();

                PopulateLayerTree(layerGroups);
            }
            catch (Exception ex)
            {
                ResetLayerTree();

                MessageBox.Show(
                    $"Failed to load layers: {ex.Message}",
                    "Layer Manager",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void PopulateLayerTree(
            IReadOnlyList<CanvasLayerTreeGroup> layerGroups)
        {
            _suppressLayerTreeEvents = true;

            try
            {
                treeViewLayers.BeginUpdate();
                treeViewLayers.Nodes.Clear();

                foreach (CanvasLayerTreeGroup group in layerGroups)
                {
                    TreeNode groupNode =
                        CreateLayerGroupNode(group.Key, group.Name);

                    foreach (CanvasLayer layer in group.Layers)
                        groupNode.Nodes.Add(CreateLayerNode(layer));

                    treeViewLayers.Nodes.Add(groupNode);
                    groupNode.Expand();
                }

                treeViewLayers.SelectedNode =
                    treeViewLayers.Nodes.Count > 0
                        ? treeViewLayers.Nodes[0]
                        : null;
            }
            finally
            {
                treeViewLayers.EndUpdate();
                _suppressLayerTreeEvents = false;
            }

            UpdateRasterCanvasLayersFromTree();
        }

        private void ResetLayerTree()
        {
            _suppressLayerTreeEvents = true;

            try
            {
                treeViewLayers.BeginUpdate();
                treeViewLayers.Nodes.Clear();

                foreach (CanvasLayerTreeGroup group
                    in CanvasLayerTreeService.GetDefaultLayerTree())
                {
                    TreeNode groupNode =
                        CreateLayerGroupNode(group.Key, group.Name);

                    foreach (CanvasLayer layer in group.Layers)
                        groupNode.Nodes.Add(CreateLayerNode(layer));

                    treeViewLayers.Nodes.Add(groupNode);
                    groupNode.Expand();
                }

                treeViewLayers.SelectedNode =
                    treeViewLayers.Nodes.Count > 0
                        ? treeViewLayers.Nodes[0]
                        : null;
            }
            finally
            {
                treeViewLayers.EndUpdate();
                _suppressLayerTreeEvents = false;
            }

            UpdateRasterCanvasLayersFromTree();
        }

        private void PopulateLayerProperties(CanvasLayer layer)
        {
        }

        private void ClearLayerProperties()
        {
        }

        private Rectangle GetLayerNodeCheckBoxRect(TreeNode node)
        {
            return new Rectangle(
                node.Bounds.X,
                node.Bounds.Y + Math.Max(0, (treeViewLayers.ItemHeight - LayerNodeCheckBoxSize) / 2),
                LayerNodeCheckBoxSize,
                LayerNodeCheckBoxSize);
        }

        private Rectangle GetLayerNodeColorRect(TreeNode node)
        {
            Rectangle checkBoxRect = GetLayerNodeCheckBoxRect(node);

            return new Rectangle(
                checkBoxRect.Right + LayerNodeCheckBoxGap,
                node.Bounds.Y + Math.Max(0, (treeViewLayers.ItemHeight - LayerNodeColorBoxSize) / 2),
                LayerNodeColorBoxSize,
                LayerNodeColorBoxSize);
        }

        private async Task OpenLayerPropertyManagerAsync(TreeNode node)
        {
            if (node.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsLayerNode ||
                nodeState.Layer == null)
            {
                return;
            }

            CanvasLayer editableLayer = CloneLayer(nodeState.Layer);

            using var frm = new frmLayerPropertyManager(editableLayer);
            PositionLayerPropertyManager(frm);

            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            if (!AppServices.HasContext)
            {
                nodeState.Layer = editableLayer;
                node.Text = editableLayer.Name;
                treeViewLayers.Invalidate();
                UpdateRasterCanvasLayersFromTree();
                mapCanvasControlMain.RequestRender();
                return;
            }

            try
            {
                var repository = _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);

                editableLayer.LastModifiedDate = DateTime.Now;
                await repository.UpdateAsync(editableLayer);

                nodeState.Layer = editableLayer;

                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();

                await RefreshLayerTreeAsync();
                SelectLayerNodeById(editableLayer.Id);
                mapCanvasControlMain.RequestRender();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update the layer properties: {ex.Message}",
                    "Layer Properties",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void PositionLayerPropertyManager(Form form)
        {
            const int gap = 4;

            Point targetLocation = grpLayer.PointToScreen(
                new Point(grpLayer.Width + gap, 0));

            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            int x = Math.Min(targetLocation.X, workingArea.Right - form.Width);
            int y = Math.Min(targetLocation.Y, workingArea.Bottom - form.Height);

            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(
                Math.Max(workingArea.Left, x),
                Math.Max(workingArea.Top, y));
        }

        private void SelectLayerNodeById(int layerId)
        {
            foreach (TreeNode groupNode in treeViewLayers.Nodes)
            {
                foreach (TreeNode layerNode in groupNode.Nodes)
                {
                    if (layerNode.Tag is LayerTreeNodeState state &&
                        state.Layer?.Id == layerId)
                    {
                        treeViewLayers.SelectedNode = layerNode;
                        layerNode.EnsureVisible();
                        return;
                    }
                }
            }
        }

        private void UpdateRasterCanvasLayersFromTree()
        {
            string? projectFolderPath = AppServices.HasContext
                ? AppServices.Context.ProjectFolderPath
                : null;

            List<CanvasLayer> rasterLayers = [];

            foreach (TreeNode groupNode in treeViewLayers.Nodes)
            {
                foreach (TreeNode layerNode in groupNode.Nodes)
                {
                    if (layerNode.Tag is LayerTreeNodeState state &&
                        state.Layer != null &&
                        string.Equals(
                            state.Layer.LayerType,
                            CanvasLayerTreeService.RasterLayerType,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        rasterLayers.Add(state.Layer);
                    }
                }
            }

            mapCanvasControlMain.SetRasterLayers(
                rasterLayers,
                projectFolderPath);
        }

        private static string BuildUniqueLayerName(
            string? desiredName,
            IEnumerable<CanvasLayer> existingLayers)
        {
            string baseName = string.IsNullOrWhiteSpace(desiredName)
                ? "Raster"
                : desiredName.Trim();

            HashSet<string> existingNames = existingLayers
                .Select(layer => layer.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existingNames.Contains(baseName))
                return baseName;

            for (int counter = 1; counter < 10000; counter++)
            {
                string candidate = $"{baseName}_{counter}";
                if (!existingNames.Contains(candidate))
                    return candidate;
            }

            return $"{baseName}_{DateTime.Now:yyyyMMddHHmmss}";
        }

        private static string GetMostUsefulExceptionMessage(Exception exception)
        {
            Exception current = exception;

            while (current.InnerException != null)
                current = current.InnerException;

            return current.Message;
        }

        private static string? GetRasterImportWarning(RasterImportMode importMode)
        {
            return importMode switch
            {
                RasterImportMode.UnknownCrsCopiedWithoutProjection =>
                    "Warning: this raster has map coordinates, but no coordinate reference system was found.",
                RasterImportMode.UnreferencedCopiedToLocalCoordinates =>
                    "Warning: this raster is not georeferenced. The map placement is temporary image coordinates only, so the map will not be spatially correct.",
                _ => null
            };
        }

        private static string FormatExceptionDetails(Exception exception)
        {
            List<string> lines =
            [
                $"Exception: {exception.GetType().FullName}",
                $"Message: {exception.Message}"
            ];

            Exception? current = exception.InnerException;
            int depth = 1;

            while (current != null)
            {
                lines.Add(
                    $"Inner {depth}: {current.GetType().FullName}: {current.Message}");
                current = current.InnerException;
                depth++;
            }

            lines.Add($"StackTrace: {exception.StackTrace}");
            return string.Join(Environment.NewLine, lines);
        }

        private static void LogProjectError(string message, Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(FormatExceptionDetails(exception));

            if (AppServices.HasContext)
            {
                AppServices.Context.Session.Logger.LogError(
                    $"{message}{Environment.NewLine}{FormatExceptionDetails(exception)}",
                    exception);
            }
        }

        private static CanvasLayer CloneLayer(CanvasLayer source)
        {
            return new CanvasLayer
            {
                Id = source.Id,
                Name = source.Name,
                LayerType = source.LayerType,
                IsVisible = source.IsVisible,
                IsLocked = source.IsLocked,
                IsSelectable = source.IsSelectable,
                IsPrintable = source.IsPrintable,
                DisplayOrder = source.DisplayOrder,
                BorderColor = source.BorderColor,
                LineWeight = source.LineWeight,
                LineStyle = source.LineStyle,
                FillColor = source.FillColor,
                FillTransparency = source.FillTransparency,
                FillStyle = source.FillStyle,
                HatchPattern = source.HatchPattern,
                ShowLabels = source.ShowLabels,
                LabelFontName = source.LabelFontName,
                LabelFontSize = source.LabelFontSize,
                LabelColor = source.LabelColor,
                LabelField = source.LabelField,
                PointSymbol = source.PointSymbol,
                PointSize = source.PointSize,
                SourceFile = source.SourceFile,
                ImportedDate = source.ImportedDate,
                CreatedDate = source.CreatedDate,
                LastModifiedDate = source.LastModifiedDate,
                Description = source.Description
            };
        }

        private void ConfigureCanvasStatusBarLayout()
        {
            // Force a stable, readable layout regardless of designer changes:
            // mode on left, coordinates pinned on right.
            statusCanvas.SuspendLayout();
            try
            {
                statusCanvas.Visible = true;
                statusCanvas.Dock = DockStyle.Bottom;
                statusCanvas.RightToLeft = RightToLeft.No;
                statusCanvas.BackColor = SystemColors.ControlLightLight;
                statusCanvas.ForeColor = SystemColors.ControlText;

                lblCanvasMode.Alignment = ToolStripItemAlignment.Left;
                lblCanvasMode.Spring = false;
                lblCanvasMode.AutoSize = true;
                lblCanvasMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                lblCanvasMode.ForeColor = SystemColors.ControlText;
                lblCanvasMode.BorderSides = ToolStripStatusLabelBorderSides.None;
                lblCanvasMode.BorderStyle = Border3DStyle.Flat;

                lblCanvasCoordinates.Alignment = ToolStripItemAlignment.Right;
                lblCanvasCoordinates.Spring = false;
                lblCanvasCoordinates.AutoSize = true;
                lblCanvasCoordinates.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                lblCanvasCoordinates.ForeColor = SystemColors.ControlText;
                lblCanvasCoordinates.BorderSides = ToolStripStatusLabelBorderSides.Left;
                lblCanvasCoordinates.BorderStyle = Border3DStyle.RaisedOuter;
                lblCanvasCoordinates.Margin = new Padding(0, 3, 6, 2);

                // Rebuild order deterministically.
                statusCanvas.Items.Clear();
                statusCanvas.Items.Add(lblCanvasMode);
                statusCanvas.Items.Add(_statusSpacer);
                statusCanvas.Items.Add(lblCanvasCoordinates);
            }
            finally
            {
                statusCanvas.ResumeLayout(performLayout: true);
            }
        }

        private void MapCanvasControlMain_StatusChanged(string coordinates, string mode)
        {
            lblCanvasCoordinates.Text = coordinates;
            lblCanvasMode.Text = mode;
        }
    }
}

