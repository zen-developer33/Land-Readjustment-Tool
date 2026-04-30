
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Forms;
using Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment;
using Land_Readjustment_Tool.Properties;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Forms;
using Land_Readjustment_Tool.UI.Forms.Project;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using System.Drawing;
using System.Reflection;
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
        private readonly CanvasLayerCommandService _layerCommandService;
        private readonly CanvasLayerBoundsService _layerBoundsService;
        private readonly RasterLayerProjectionService _rasterLayerProjectionService;
        private readonly IRasterLayerImportService _rasterLayerImportService;
        private readonly IXyzTileSourceService _xyzTileSourceService;
        private readonly ProjectOpenService _projectOpenService;
        private readonly ProjectSaveAsService _projectSaveAsService;
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
        private readonly ToolStripStatusLabel _canvasCommandStatus = new()
        {
            Name = "lblCanvasCommandStatus",
            AutoSize = false,
            Width = 170,
            Text = "Canvas: Ready",
            TextAlign = ContentAlignment.MiddleLeft
        };
        private readonly ToolStripStatusLabel _activeLayerLabel = new()
        {
            Name = "lblActiveLayer",
            Text = "Active Layer:",
            Visible = false
        };
        private readonly ToolStripComboBox _activeLayerCombo = new()
        {
            Name = "cmbActiveLayer",
            DropDownStyle = ComboBoxStyle.DropDownList,
            AutoSize = false,
            Width = 190,
            Visible = false
        };
        private readonly ToolStripStatusLabel _operationProgressStatus = new()
        {
            Name = "lblOperationProgressStatus",
            AutoSize = false,
            Width = 210,
            Text = string.Empty,
            TextAlign = ContentAlignment.MiddleRight,
            Visible = false
        };
        private readonly StatusProgressBar _operationProgressBar = new()
        {
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            Size = new Size(150, 16)
        };
        private readonly ToolStripControlHost _operationProgressHost;
        private bool _suppressActiveLayerEvents;
        private const string LayerGroupNodeNamePrefix = "LayerGroup_";
        private const string RasterLayerGroupKey = "RasterLayer";
        private const int LayerNodeCheckBoxSize = 14;
        private const int LayerNodeCheckBoxGap = 6;
        private const int LayerNodeColorBoxSize = 18;
        private const int LayerNodeColorBoxGap = 4;
        private readonly ContextMenuStrip _layerContextMenu = new();
        private readonly ToolStripMenuItem _mnuZoomToLayer = new("Zoom To Layer");
        private readonly ToolStripMenuItem _mnuRenameLayer = new("Rename");
        private readonly ToolStripMenuItem _mnuDeleteLayer = new("Delete");
        private readonly ToolStripMenuItem _mnuToggleLayerVisibility = new("Show/Hide");
        private readonly ToolStripMenuItem _mnuToggleLayerLock = new("Lock/Unlock");
        private readonly ToolStripMenuItem _mnuLayerProperties = new("Layer Properties");
        private readonly ToolStripMenuItem _mnuAddRasterMap = new("Add Raster Map...");
        private readonly ToolStripMenuItem _mnuAddXyzTiles = new("Add XYZ Tiles...");
        private readonly ToolStripMenuItem _mnuImportXyzTiles = new("Import XYZ Tiles...");
        private readonly TextBox _layerRenameTextBox = new();
        private TreeNode? _contextLayerNode;
        private TreeNode? _renamingLayerNode;
        private bool _isCompletingLayerRename;

        private sealed class LayerTreeNodeState
        {
            public bool IsLayerNode { get; init; }
            public CanvasLayer? Layer { get; set; }
        }

        private sealed class ActiveLayerStatusItem
        {
            public ActiveLayerStatusItem(int layerId, string name)
            {
                LayerId = layerId;
                Name = name;
            }

            public int LayerId { get; }
            public string Name { get; }

            public override string ToString()
            {
                return Name;
            }
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

        /// <summary>
        /// Builds dependent services for the non-DI constructor without duplicating wiring.
        /// </summary>
        private frmMain(
            ProjectBackupService backupService,
            ProjectSessionFactory sessionFactory,
            ProjectService projectService,
            IProjectScopedFactory projectScopedFactory,
            string? startupFilePath)
            : this(
                backupService,
                sessionFactory,
                projectService,
                projectScopedFactory,
                new CanvasLayerCommandService(projectScopedFactory),
                new CanvasLayerBoundsService(projectScopedFactory),
                new RasterLayerProjectionService(
                    projectScopedFactory,
                    new ProjectRasterCrsResolver(projectScopedFactory),
                    new GdalRasterDatasetImporter()),
                new RasterLayerImportService(
                    projectScopedFactory,
                    new ProjectRasterCrsResolver(projectScopedFactory),
                    new GdalRasterDatasetImporter()),
                new XyzTileSourceService(),
                new ProjectOpenService(sessionFactory, projectScopedFactory),
                new ProjectSaveAsService(backupService, sessionFactory),
                startupFilePath)
        {
        }

        public frmMain(
            ProjectBackupService backupService,
            ProjectSessionFactory sessionFactory,
            ProjectService projectService,
            IProjectScopedFactory projectScopedFactory,
            CanvasLayerCommandService layerCommandService,
            CanvasLayerBoundsService layerBoundsService,
            RasterLayerProjectionService rasterLayerProjectionService,
            IRasterLayerImportService rasterLayerImportService,
            IXyzTileSourceService xyzTileSourceService,
            ProjectOpenService projectOpenService,
            ProjectSaveAsService projectSaveAsService,
            string? startupFilePath = null)
        {
            _backupService = backupService ?? throw new ArgumentNullException(nameof(backupService));
            _sessionFactory = sessionFactory ?? throw new ArgumentNullException(nameof(sessionFactory));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _projectScopedFactory = projectScopedFactory ?? throw new ArgumentNullException(nameof(projectScopedFactory));
            _layerCommandService = layerCommandService ?? throw new ArgumentNullException(nameof(layerCommandService));
            _layerBoundsService = layerBoundsService ?? throw new ArgumentNullException(nameof(layerBoundsService));
            _rasterLayerProjectionService = rasterLayerProjectionService ?? throw new ArgumentNullException(nameof(rasterLayerProjectionService));
            _rasterLayerImportService = rasterLayerImportService ?? throw new ArgumentNullException(nameof(rasterLayerImportService));
            _xyzTileSourceService = xyzTileSourceService ?? throw new ArgumentNullException(nameof(xyzTileSourceService));
            _projectOpenService = projectOpenService ?? throw new ArgumentNullException(nameof(projectOpenService));
            _projectSaveAsService = projectSaveAsService ?? throw new ArgumentNullException(nameof(projectSaveAsService));

            InitializeComponent();
            _operationProgressHost = new ToolStripControlHost(_operationProgressBar)
            {
                Name = "hostOperationProgress",
                AutoSize = false,
                Size = new Size(154, 22),
                Margin = new Padding(4, 2, 8, 2),
                Visible = false
            };
            _activeLayerCombo.SelectedIndexChanged += ActiveLayerCombo_SelectedIndexChanged;
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
            baseMapsToolStripMenuItem.Click += importRasterToolStripMenuItem_Click!;
            _mnuImportXyzTiles.Click += importXyzTilesToolStripMenuItem_Click!;
            importDataToolStripMenuItem1.DropDownItems.Add(_mnuImportXyzTiles);

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

        // -- FORM CLOSING -----------------------------

        // Handles form closing event
        private void frmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!HandleUnsavedChangesOnClose())
            {
                e.Cancel = true;
                return;
            }

            DisposeCurrentProjectSession();
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

        private void DisposeCurrentProjectSession()
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            try
            {
                AppServices.Context.StateChanged -= UpdateWindowTitle;
                AppServices.Context.Session.Dispose();
            }
            finally
            {
                AppServices.ClearContext();
                SqliteConnection.ClearAllPools();
            }
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

        // -- NEW PROJECT ------------------------------

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
                SetOperationProgress(5, "Creating project workspace");

                // Create folder structure
                Directory.CreateDirectory(projectFolder);
                ProjectFolderCreator.CreateFolders(projectFolder);

                SetOperationProgress(25, "Opening project database");

                // Create session and context
                var session = _sessionFactory.CreateSession(projectFilePath);
                var context = new ProjectContext(session, projectFilePath);

                // Register in AppServices
                AppServices.SetContext(context);

                // Subscribe to state changes
                context.StateChanged += UpdateWindowTitle;

                SetOperationProgress(45, "Saving project details");

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
                SetOperationProgress(65, "Preparing map canvas");

                InitializeProjectWorkspace();
                await ApplySettingsAsync(showRefreshProgress: false);
                await RefreshMapCanvasAsync("Opening project canvas", 75);
            }
            catch (Exception ex)
            {
                HideOperationProgress();
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
                "from Project > Project Settings.",
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

            frm.SettingsApplied += async (_, args) =>
            {
                appliedWhileOpen = true;
                AppServices.Context.MarkAsModified();
                await ApplySettingsAsync(args.ProjectCrsChanged);
            };

            if (frm.ShowDialog() == DialogResult.OK && !appliedWhileOpen)
            {
                AppServices.Context.MarkAsModified();
                await ApplySettingsAsync(updateRasterProjection: false);
            }
        }

        private async Task ApplySettingsAsync(
            bool updateRasterProjection = false,
            bool showRefreshProgress = true)
        {
            if (!AppServices.HasContext) return;

            try
            {
                if (showRefreshProgress)
                    SetOperationProgress(8, "Loading project settings");

                var repo = _projectScopedFactory.CreateProjectSettingsRepository(
                    AppServices.Context.Session);
                var settings = await repo
                    .GetProjectSettingsAsync();

                if (settings == null) return;

                if (showRefreshProgress)
                    SetOperationProgress(35, "Applying canvas settings");

                var bgColor = ParseColorOrDefault(
                    settings.CanvasBackgroundColor, Color.White);
                var gridColor = ParseColorOrDefault(
                    settings.CanvasGridColor, Color.LightGray);

                mapCanvasControlMain.ApplyRenderSettings(
                    MapCanvasSettingsService.FromProjectSettings(settings));
                mapCanvasControlMain.ApplySnapEnabled(settings.SnapEnabled);

                if (_workspaceCanvas != null && !_workspaceCanvas.IsDisposed)
                {
                    if (showRefreshProgress)
                        SetOperationProgress(55, "Updating workspace canvas");

                    _workspaceCanvas.ApplyBackgroundColor(bgColor);
                    _workspaceCanvas.ApplyGridColor(gridColor);
                    _workspaceCanvas.ApplyGridVisible(
                        settings.CanvasGridVisible);
                    _workspaceCanvas.ApplySnapEnabled(
                        settings.SnapEnabled);
                }

                if (updateRasterProjection)
                {
                    if (showRefreshProgress)
                        SetOperationProgress(72, "Updating raster projection");

                    await ReprojectRasterLayersForCurrentProjectAsync();
                }

                if (showRefreshProgress)
                {
                    SetOperationProgress(90, "Rendering map canvas");
                    mapCanvasControlMain.RequestRender();
                    SetCanvasCommandStatus("Canvas: Refreshed");
                    SetOperationProgress(100, "Canvas refreshed");
                    await Task.Delay(250);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ApplyCanvasSettings failed: {ex.Message}");
            }
            finally
            {
                if (showRefreshProgress)
                    HideOperationProgress();
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
        // -- CLOSE PROJECT ----------------------------

        private async Task CloseCurrentProjectAsync()
        {
            if (!AppServices.HasContext) return;

            // HandleUnsavedChangesOnClose may already
            // dispose session if user clicked No
            if (!HandleUnsavedChangesOnClose()) return;

            DisposeCurrentProjectSession();

            UnloadProjectWorkspace();
            DisableProjectMenuItems();
            UpdateWindowTitle();
        }



        // -- PROJECT DETAILS --------------------------

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

        // -- EXIT -------------------------------------

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

        // -- STUB HANDLERS ----------------------------
        // Implemented later one by one
        private void tsmSave_Click(object sender, EventArgs e)
        {
            _ = SaveCurrentProjectAsync(showMessage: false);
        }
        // -- SAVE AS ----------------------------------
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

            string currentFilePath = AppServices.Context.ProjectFilePath;
            string currentFolder = Path.GetFullPath(
                Path.GetDirectoryName(currentFilePath)!);
            ProjectSaveAsTarget target;

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

                target = _projectSaveAsService.CreateTarget(sfd.FileName);

                if (string.Equals(target.ProjectFilePath, currentFilePath,
                    StringComparison.OrdinalIgnoreCase))
                {
                    await SaveCurrentProjectAsync(showMessage: false);
                    return;
                }

                if (_projectSaveAsService.IsInsideCurrentProject(
                    target.ProjectFolderPath,
                    currentFolder))
                {
                    MessageBox.Show(
                        "Cannot save inside the current project folder.\n\n" +
                        "Please choose a different location.",
                        "Invalid Location",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    continue;
                }

                if (Directory.Exists(target.ProjectFolderPath))
                {
                    DialogResult replaceResult = MessageBox.Show(
                        "A project folder with this name already exists.\n\n" +
                        "Replace that folder with the saved copy?",
                        "Replace Existing Project Folder",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);

                    if (replaceResult != DialogResult.Yes)
                    {
                        continue;
                    }
                }

                break;
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                ProjectContext newContext =
                    await _projectSaveAsService.SaveAsAsync(
                        currentFilePath,
                        target);

                DisposeCurrentProjectSession();
                AppServices.SetContext(newContext);
                newContext.StateChanged += UpdateWindowTitle;
                _layerTreeService = _projectScopedFactory.CreateCanvasLayerTreeService(
                    newContext.Session);

                EnableProjectMenuItems();
                UpdateWindowTitle();
                await RefreshMapCanvasAsync("Refreshing saved project canvas");

                MessageBox.Show(
                    $"Project saved as:\n{target.ProjectFilePath}",
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
        /// Reprojects persisted raster layers after the active project CRS or datum changes.
        /// </summary>
        private async Task ReprojectRasterLayersForCurrentProjectAsync()
        {
            if (!AppServices.HasContext)
                return;

            mapCanvasControlMain.ClearRasterLayers();

            RasterLayerProjectionUpdateResult result =
                await _rasterLayerProjectionService
                    .ReprojectRasterLayersToProjectCrsAsync(
                        AppServices.Context.Session,
                        AppServices.Context.ProjectFolderPath);

            await RefreshLayerTreeAsync();
            mapCanvasControlMain.RequestRender();

            if (result.FailedCount > 0)
            {
                MessageBox.Show(
                    $"{result.FailedCount} raster layer(s) could not be updated to the new project CRS. " +
                    "The details were written to the project log.",
                    "Raster CRS Update",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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

        private async void importRasterToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            await ImportRasterFileAsync(
                "Import Raster",
                GetGeneralRasterImportFilter());
        }

        private async void importXyzTilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            await ImportXyzTilesAsync();
        }

        /// <summary>
        /// Imports a local raster file through metadata preview, source CRS definition, and project CRS projection.
        /// </summary>
        private async Task ImportRasterFileAsync(string dialogTitle, string dialogFilter)
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
                Title = dialogTitle,
                Filter = dialogFilter,
                Multiselect = false,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                SetOperationProgress(2, "Reading raster details");

                RasterLayerImportPreview importPreview =
                    await _rasterLayerImportService.PrepareImportAsync(
                        AppServices.Context.Session,
                        dialog.FileName);

                using frmRasterImportReview reviewForm =
                    new(importPreview);

                if (reviewForm.ShowDialog(this) != DialogResult.OK)
                    return;

                CanvasLayer? existingRasterLayer =
                    await FindExistingRasterLayerAsync(reviewForm.LayerName);
                if (existingRasterLayer != null)
                {
                    DialogResult replaceResult = MessageBox.Show(
                        this,
                        $"A raster layer named '{existingRasterLayer.Name}' already exists. Replace it?",
                        "Raster Import",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);

                    if (replaceResult != DialogResult.Yes)
                    {
                        return;
                    }

                    await ReplaceRasterLayerAsync(existingRasterLayer);
                }

                SetOperationProgress(2, "Starting raster import");

                Progress<RasterImportProgressInfo> progress = new(
                    update => SetOperationProgress(update.Percent, update.Status));

                RasterLayerImportResult importResult =
                    await _rasterLayerImportService.ImportAsync(
                        new RasterLayerImportRequest(
                            AppServices.Context.Session,
                            AppServices.Context.ProjectFolderPath,
                            dialog.FileName,
                            reviewForm.LayerName,
                            reviewForm.SourceSrsDefinitionOverride),
                        progress);

                SetOperationProgress(94, "Refreshing raster layer list");

                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();

                await RefreshLayerTreeAsync();
                SelectLayerNodeById(importResult.Layer.Id);
                mapCanvasControlMain.ZoomExtents();

                SetCanvasCommandStatus(importResult.Heading);
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    ex.Message,
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                LogProjectError("Raster import failed.", ex);

                MessageBox.Show(
                    $"Failed to import raster: {GetMostUsefulExceptionMessage(ex)}",
                    "Raster Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        /// <summary>
        /// Imports an online XYZ tile source as a project raster layer.
        /// </summary>
        private async Task ImportXyzTilesAsync()
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

            using frmXyzTileImportOptions optionsForm =
                new(AppServices.Context.ProjectFolderPath);

            if (optionsForm.ShowDialog(this) != DialogResult.OK)
                return;

            XyzTileSourceImportRequest request = optionsForm.ImportRequest;

            try
            {
                CanvasLayer? existingRasterLayer =
                    await FindExistingRasterLayerAsync(request.LayerName);
                if (existingRasterLayer != null)
                {
                    DialogResult replaceResult = MessageBox.Show(
                        this,
                        $"A raster layer named '{existingRasterLayer.Name}' already exists. Replace it?",
                        "XYZ Tile Import",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);

                    if (replaceResult != DialogResult.Yes)
                    {
                        return;
                    }

                    await ReplaceRasterLayerAsync(existingRasterLayer);
                }

                SetOperationProgress(2, "Preparing XYZ tile source");

                XyzTileSourceDefinition sourceDefinition =
                    _xyzTileSourceService.CreateSourceDefinition(
                        AppServices.Context.ProjectFolderPath,
                        request);

                Progress<RasterImportProgressInfo> progress = new(
                    update => SetOperationProgress(update.Percent, update.Status));

                RasterLayerImportResult importResult =
                    await _rasterLayerImportService.ImportAsync(
                        new RasterLayerImportRequest(
                            AppServices.Context.Session,
                            AppServices.Context.ProjectFolderPath,
                            sourceDefinition.DefinitionPath,
                            request.LayerName,
                            sourceDefinition.SourceExtent.SrsDefinition,
                            sourceDefinition.SourceExtent),
                        progress);

                SetOperationProgress(94, "Refreshing raster layer list");

                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();

                await RefreshLayerTreeAsync();
                SelectLayerNodeById(importResult.Layer.Id);
                mapCanvasControlMain.ZoomExtents();

                SetCanvasCommandStatus(
                    $"Imported XYZ tiles: {importResult.Layer.Name}");
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    ex.Message,
                    "XYZ Tile Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                LogProjectError("XYZ tile import failed.", ex);

                MessageBox.Show(
                    $"Failed to import XYZ tiles: {GetMostUsefulExceptionMessage(ex)}",
                    "XYZ Tile Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private async Task<CanvasLayer?> FindExistingRasterLayerAsync(string layerName)
        {
            if (!AppServices.HasContext || _layerTreeService == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(layerName))
            {
                return null;
            }

            IReadOnlyList<CanvasLayer> rasterLayers =
                await _layerTreeService.GetRasterLayersAsync();

            return rasterLayers.FirstOrDefault(layer =>
                string.Equals(layer.Name, layerName, StringComparison.OrdinalIgnoreCase));
        }

        private async Task ReplaceRasterLayerAsync(CanvasLayer existingLayer)
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            await _layerCommandService.DeleteAsync(
                AppServices.Context.Session,
                existingLayer);

            await DeleteRasterLayerFileAsync(
                existingLayer,
                AppServices.Context.ProjectFolderPath);
        }

        private static async Task DeleteRasterLayerFileAsync(
            CanvasLayer existingLayer,
            string? projectFolderPath)
        {
            if (string.IsNullOrWhiteSpace(existingLayer.SourceFile))
            {
                return;
            }

            string fullPath = ResolveLayerSourceFilePath(
                existingLayer.SourceFile,
                projectFolderPath);

            if (!File.Exists(fullPath))
            {
                return;
            }

            string projectRoot = string.IsNullOrWhiteSpace(projectFolderPath)
                ? string.Empty
                : Path.GetFullPath(projectFolderPath);
            string resolvedPath = Path.GetFullPath(fullPath);

            if (!string.IsNullOrWhiteSpace(projectRoot) &&
                !resolvedPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            await Task.Run(() => File.Delete(resolvedPath));
        }

        private static string ResolveLayerSourceFilePath(
            string storedPath,
            string? projectFolderPath)
        {
            if (Path.IsPathRooted(storedPath))
            {
                return Path.GetFullPath(storedPath);
            }

            if (string.IsNullOrWhiteSpace(projectFolderPath))
            {
                return Path.GetFullPath(storedPath);
            }

            return Path.GetFullPath(Path.Combine(projectFolderPath, storedPath));
        }

        /// <summary>
        /// Builds the shared file filter for local GDAL-readable raster imports.
        /// </summary>
        private static string GetGeneralRasterImportFilter()
        {
            return
                "Raster files (*.tif;*.tiff;*.vrt;*.img;*.jpg;*.jpeg;*.png;*.bmp;*.mbtiles;*.jp2;*.grd;*.asc;*.xyz;*.dem;*.hgt)|*.tif;*.tiff;*.vrt;*.img;*.jpg;*.jpeg;*.png;*.bmp;*.mbtiles;*.jp2;*.grd;*.asc;*.xyz;*.dem;*.hgt|" +
                "GeoTIFF / TIFF (*.tif;*.tiff)|*.tif;*.tiff|" +
                "MBTiles (*.mbtiles)|*.mbtiles|" +
                "Image rasters (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|" +
                "Elevation rasters (*.dem;*.hgt;*.asc;*.xyz;*.grd)|*.dem;*.hgt;*.asc;*.xyz;*.grd|" +
                "All files (*.*)|*.*";
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

            try
            {
                SetOperationProgress(5, "Opening project");

                if (!EnsureProjectFileCanBeOpened(projectFilePath))
                {
                    HideOperationProgress();
                    return;
                }

                SetOperationProgress(15, "Closing current workspace");
                DisposeCurrentProjectSession();
                UnloadProjectWorkspace();

                SetOperationProgress(35, "Loading project database");
                ProjectContext context =
                    await _projectOpenService.OpenAsync(projectFilePath);

                SetOperationProgress(55, "Preparing project workspace");
                RecentProjectsManager.AddRecentProject(projectFilePath); //ADDS TO THE RECENTLY OPENED PROJECTS IN SETTINGS
                BuildRecentProjectsMenu(); // Refresh menu to show the newly added recent project immediately
                context.StateChanged += UpdateWindowTitle;
                AppServices.SetContext(context);

                EnableProjectMenuItems();
                UpdateWindowTitle();
                SetOperationProgress(70, "Preparing map canvas");
                InitializeProjectWorkspace();
                await ApplySettingsAsync(showRefreshProgress: false);
                await RefreshMapCanvasAsync("Opening project canvas", 75);
            }
            catch (Exception ex)
            {
                HideOperationProgress();
                MessageBox.Show(
                    $"Failed to open project: {GetMostUsefulExceptionMessage(ex)}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool EnsureProjectFileCanBeOpened(string projectFilePath)
        {
            ProjectOpenCheck openCheck =
                _projectOpenService.CheckProjectFile(projectFilePath);

            if (openCheck.CanOpen)
            {
                return true;
            }

            if (openCheck.ValidBackupPath != null)
            {
                DialogResult restoreResult = MessageBox.Show(
                    "The selected project file is not a valid RePlot database.\n\n" +
                    $"Reason: {openCheck.Reason}\n\n" +
                    "A valid backup was found. Restore the latest valid backup and open it?",
                    "Project File Recovery",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                if (restoreResult == DialogResult.Yes)
                {
                    _projectOpenService.RestoreBackup(
                        openCheck.ValidBackupPath,
                        projectFilePath);
                    return true;
                }
            }

            MessageBox.Show(
                "The selected file cannot be opened as a RePlot project.\n\n" +
                $"Reason: {openCheck.Reason}\n\n" +
                "Please choose the main .lpp file from the project folder, not a backup, WAL/SHM sidecar, shortcut, or exported data file.",
                "Invalid Project File",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            return false;
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
                // Save = WAL checkpoint (merge WAL ? .lpp) + rotate backups.

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
                DisposeCurrentProjectSession();

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
        ///      ? Yes   = SaveCurrentProjectAsync then open new
        ///      ? No    = ChangeTracker.Clear() then open new
        ///      ? Cancel = abort, stay on current project
        /// </summary>
        private async void RecentProjectItem_Click(
            object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem item) return;
            if (item.Tag is not string path) return;

            // -- Step 1: File still exists on disk? -------------------
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

            // -- Step 2: Is this project already open? ----------------
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

            // -- Step 3: Confirm switching to selected recent project -
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

            // -- Step 4: Delegate to OpenProjectInternalAsync -
            // checkUnsavedChanges: true means it will internally call
            // HandleUnsavedChangesOnClose() before doing anything.
            // That gives the user Yes / No / Cancel for unsaved changes.
            // If user clicks Cancel — nothing happens, current project stays.
            await OpenProjectInternalAsync(
                path,
                checkUnsavedChanges: true);
        }
        // -- RECENT PROJECTS MENU -----------------------------------------

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
        // -- PROJECT FOLDER CREATOR -------------------

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
            treeViewLayers.LabelEdit = false;

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

            ConfigureLayerContextMenu();
            ConfigureLayerRenameTextBox();

            ResetLayerTree();
        }

        private void ConfigureLayerRenameTextBox()
        {
            _layerRenameTextBox.BorderStyle = BorderStyle.FixedSingle;
            _layerRenameTextBox.Font = treeViewLayers.Font;
            _layerRenameTextBox.Visible = false;
            _layerRenameTextBox.Margin = Padding.Empty;
            _layerRenameTextBox.KeyDown += LayerRenameTextBox_KeyDown;
            _layerRenameTextBox.Leave += LayerRenameTextBox_Leave;
            treeViewLayers.Controls.Add(_layerRenameTextBox);
        }

        private void ConfigureLayerContextMenu()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Opening += LayerContextMenu_Opening;
            _mnuAddRasterMap.Click += async (_, _) => await ImportRasterFileAsync(
                "Add Raster Map",
                GetGeneralRasterImportFilter());
            _mnuAddXyzTiles.Click += async (_, _) => await ImportXyzTilesAsync();
            _mnuZoomToLayer.Click += async (_, _) => await ZoomToLayerAsync(_contextLayerNode);
            _mnuRenameLayer.Click += (_, _) => BeginLayerRename(_contextLayerNode);
            _mnuDeleteLayer.Click += async (_, _) => await DeleteLayerAsync(_contextLayerNode);
            _mnuToggleLayerVisibility.Click += async (_, _) => await ToggleLayerNodeVisibilityAsync(_contextLayerNode);
            _mnuToggleLayerLock.Click += async (_, _) => await ToggleLayerLockAsync(_contextLayerNode);
            _mnuLayerProperties.Click += async (_, _) => await OpenLayerPropertyManagerAsync(_contextLayerNode);
            treeViewLayers.ContextMenuStrip = _layerContextMenu;
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

                if (e.Node == _renamingLayerNode)
                {
                    return;
                }

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
                GetLayerNodeTextRect(e.Node, x),
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

            if (e.Button == MouseButtons.Right)
            {
                _contextLayerNode = IsLayerNode(e.Node)
                    ? e.Node
                    : null;
                return;
            }

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
            {
                await OpenLayerPropertyManagerAsync(e.Node);
            }
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

            BeginLayerRename(e.Node);
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
            CanvasLayer? layer = GetLayerFromNode(e.Node);
            if (layer == null)
            {
                SetCanvasCommandStatus(
                    e.Node == null
                        ? "Canvas: Ready"
                        : $"Canvas Group: {e.Node.Text}");
                return;
            }

            UpdateActiveLayerComboFromTree(layer.Id);
            SetCanvasCommandStatus($"Active Layer: {layer.Name}");
        }

        private void ActiveLayerCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressActiveLayerEvents ||
                _activeLayerCombo.SelectedItem is not ActiveLayerStatusItem item)
            {
                return;
            }

            SelectLayerNodeById(item.LayerId);
            SetCanvasCommandStatus($"Active Layer: {item.Name}");
        }

        private void LayerContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Point clientPoint = treeViewLayers.PointToClient(Cursor.Position);
            TreeNode? node = treeViewLayers.GetNodeAt(clientPoint);
            TreeNode? targetNode = node ?? treeViewLayers.SelectedNode;

            if (IsRasterLayerGroupNode(targetNode))
            {
                _contextLayerNode = null;
                treeViewLayers.SelectedNode = targetNode;
                ConfigureRasterGroupContextMenuItems();
                return;
            }

            _contextLayerNode = IsLayerNode(targetNode) ? targetNode : null;

            if (!IsLayerNode(_contextLayerNode))
            {
                e.Cancel = true;
                return;
            }

            treeViewLayers.SelectedNode = _contextLayerNode;
            ConfigureLayerContextMenuItems();

            CanvasLayer layer = GetLayerFromNode(_contextLayerNode)!;

            _mnuToggleLayerVisibility.Text = layer.IsVisible
                ? "Hide"
                : "Show";
            _mnuToggleLayerLock.Text = layer.IsLocked
                ? "Unlock"
                : "Lock";

            _mnuZoomToLayer.Enabled = true;
            _mnuRenameLayer.Enabled = true;
            _mnuDeleteLayer.Enabled = true;
            _mnuToggleLayerVisibility.Enabled = true;
            _mnuToggleLayerLock.Enabled = true;
            _mnuLayerProperties.Enabled = true;
        }

        private void ConfigureRasterGroupContextMenuItems()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.AddRange(
            [
                _mnuAddRasterMap,
                _mnuAddXyzTiles
            ]);
        }

        private void ConfigureLayerContextMenuItems()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.AddRange(
            [
                _mnuZoomToLayer,
                new ToolStripSeparator(),
                _mnuRenameLayer,
                _mnuDeleteLayer,
                new ToolStripSeparator(),
                _mnuToggleLayerVisibility,
                _mnuToggleLayerLock,
                new ToolStripSeparator(),
                _mnuLayerProperties
            ]);
        }

        private void BeginLayerRename(TreeNode? node)
        {
            if (!IsLayerNode(node))
            {
                return;
            }

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer?.IsLocked == true)
            {
                ShowLayerLockedMessage(layer, "renamed");
                return;
            }

            treeViewLayers.SelectedNode = node;
            _renamingLayerNode = node;
            _layerRenameTextBox.Bounds = GetLayerRenameTextBoxRect(node!);
            _layerRenameTextBox.Text = node!.Text;
            _layerRenameTextBox.Visible = true;
            _layerRenameTextBox.BringToFront();
            _layerRenameTextBox.Focus();
            _layerRenameTextBox.SelectAll();
            treeViewLayers.Invalidate();
        }

        private async void LayerRenameTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await CompleteLayerRenameAsync(commit: true);
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                await CompleteLayerRenameAsync(commit: false);
            }
        }

        private async void LayerRenameTextBox_Leave(object? sender, EventArgs e)
        {
            await CompleteLayerRenameAsync(commit: true);
        }

        private async Task CompleteLayerRenameAsync(bool commit)
        {
            if (_isCompletingLayerRename ||
                !_layerRenameTextBox.Visible ||
                _renamingLayerNode == null)
            {
                return;
            }

            _isCompletingLayerRename = true;
            TreeNode node = _renamingLayerNode;
            string newName = _layerRenameTextBox.Text.Trim();

            try
            {
                _layerRenameTextBox.Visible = false;
                _renamingLayerNode = null;
                treeViewLayers.Focus();
                treeViewLayers.Invalidate();

                if (commit && !string.IsNullOrWhiteSpace(newName))
                {
                    await RenameLayerAsync(node, newName);
                }
            }
            finally
            {
                _isCompletingLayerRename = false;
            }
        }

        private async Task ToggleLayerNodeVisibilityAsync(TreeNode? node)
        {
            if (node == null ||
                node.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsLayerNode ||
                nodeState.Layer == null)
            {
                return;
            }

            try
            {
                CanvasLayer? updatedLayer =
                    await _layerCommandService.SetVisibilityAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        nodeState.Layer,
                        !nodeState.Layer.IsVisible);

                if (updatedLayer == null)
                {
                    return;
                }

                UpdateLayerNode(node, updatedLayer, updateRasterStack: false);
                MarkProjectModifiedIfOpen();
                if (IsRasterLayer(updatedLayer))
                {
                    bool visibilityUpdated =
                        mapCanvasControlMain.SetRasterLayerVisibility(
                            updatedLayer.Id,
                            updatedLayer.IsVisible);

                    if (!visibilityUpdated)
                        UpdateRasterCanvasLayersFromTree();
                }
                else
                {
                    mapCanvasControlMain.RequestRender();
                }

                SetCanvasCommandStatus(updatedLayer.IsVisible
                    ? $"Layer shown: {updatedLayer.Name}"
                    : $"Layer hidden: {updatedLayer.Name}");
                SelectLayerNodeById(updatedLayer.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update layer visibility: {ex.Message}",
                    "Layer Visibility",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task RenameLayerAsync(TreeNode node, string newName)
        {
            if (node.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsLayerNode ||
                nodeState.Layer == null)
            {
                return;
            }

            if (nodeState.Layer.IsLocked)
            {
                ShowLayerLockedMessage(nodeState.Layer, "renamed");
                return;
            }

            try
            {
                CanvasLayer? renamedLayer =
                    await _layerCommandService.RenameAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        nodeState.Layer,
                        newName);

                if (renamedLayer == null)
                {
                    return;
                }

                UpdateLayerNode(node, renamedLayer);
                MarkProjectModifiedIfOpen();
                await RefreshMapCanvasAsync("Refreshing layer list");
                SelectLayerNodeById(renamedLayer.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to rename the layer: {ex.Message}",
                    "Rename Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task ToggleLayerLockAsync(TreeNode? node)
        {
            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            try
            {
                CanvasLayer? updatedLayer =
                    await _layerCommandService.ToggleLockAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        layer);

                if (updatedLayer == null)
                {
                    return;
                }

                UpdateLayerNode(node!, updatedLayer, updateRasterStack: false);
                MarkProjectModifiedIfOpen();
                SetCanvasCommandStatus(updatedLayer.IsLocked
                    ? $"Layer locked: {updatedLayer.Name}"
                    : $"Layer unlocked: {updatedLayer.Name}");
                SelectLayerNodeById(updatedLayer.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update layer lock state: {ex.Message}",
                    "Layer Lock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task DeleteLayerAsync(TreeNode? node)
        {
            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            if (layer.IsLocked)
            {
                ShowLayerLockedMessage(layer, "deleted");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Delete layer '{layer.Name}'?\n\nThis will remove the layer and its objects from the project.",
                "Delete Layer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (!AppServices.HasContext || layer.Id <= 0)
            {
                node!.Remove();
                ClearLayerProperties();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                return;
            }

            try
            {
                await _layerCommandService.DeleteAsync(
                    AppServices.HasContext ? AppServices.Context.Session : null,
                    layer);
                MarkProjectModifiedIfOpen();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                ClearLayerProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete the layer: {ex.Message}",
                    "Delete Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task ZoomToLayerAsync(TreeNode? node)
        {
            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            try
            {
                RectangleD? bounds =
                    await _layerBoundsService.GetWorldBoundsAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        layer,
                        AppServices.HasContext
                            ? AppServices.Context.ProjectFolderPath
                            : null);

                if (!bounds.HasValue)
                {
                    MessageBox.Show(
                        $"Layer '{layer.Name}' does not have drawable bounds yet.",
                        "Zoom To Layer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                mapCanvasControlMain.SetPanToolActive(false);
                mnuPan.Checked = false;
                mapCanvasControlMain.ZoomToWorldBounds(bounds.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to zoom to the layer: {ex.Message}",
                    "Zoom To Layer",
                    MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private void UpdateLayerNode(
            TreeNode node,
            CanvasLayer layer,
            bool updateRasterStack = true)
        {
            if (node.Tag is LayerTreeNodeState nodeState)
            {
                nodeState.Layer = layer;
            }

            node.Text = layer.Name;
            treeViewLayers.Invalidate();
            UpdateActiveLayerComboFromTree(layer.Id);

            if (updateRasterStack)
                UpdateRasterCanvasLayersFromTree();
        }

        /// <summary>
        /// Marks the active project modified after a successful layer command.
        /// </summary>
        private void MarkProjectModifiedIfOpen()
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            AppServices.Context.MarkAsModified();
            UpdateWindowTitle();
        }

        /// <summary>
        /// Explains why a protected layer cannot be changed.
        /// </summary>
        private void ShowLayerLockedMessage(CanvasLayer layer, string action)
        {
            MessageBox.Show(
                $"Layer '{layer.Name}' is locked and cannot be {action}.\n\nUnlock the layer first if you want to change it.",
                "Layer Locked",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            SetCanvasCommandStatus($"Layer locked: {layer.Name}");
        }

        /// <summary>
        /// Refreshes the layer tree and requests a map render while reporting shared canvas progress.
        /// </summary>
        private async Task RefreshMapCanvasAsync(string status, int startPercent = 20)
        {
            try
            {
                int layerPercent = Math.Clamp(startPercent + 25, 0, 95);
                int renderPercent = Math.Clamp(startPercent + 55, 0, 98);

                SetOperationProgress(startPercent, status);
                SetOperationProgress(layerPercent, "Loading map layers");
                await RefreshLayerTreeAsync();

                SetOperationProgress(renderPercent, "Rendering map canvas");
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Canvas: Refreshed");

                SetOperationProgress(100, "Canvas refreshed");
                await Task.Delay(250);
            }
            finally
            {
                HideOperationProgress();
            }
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
            UpdateActiveLayerComboFromTree();
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
            UpdateActiveLayerComboFromTree();
        }

        private void PopulateLayerProperties(CanvasLayer layer)
        {
        }

        private void ClearLayerProperties()
        {
        }

        private void SetCanvasCommandStatus(string status)
        {
            _canvasCommandStatus.Text = string.IsNullOrWhiteSpace(status)
                ? "Canvas: Ready"
                : status;
        }

        private void UpdateActiveLayerComboFromTree(int? preferredLayerId = null)
        {
            int? selectedLayerId =
                preferredLayerId ??
                (_activeLayerCombo.SelectedItem as ActiveLayerStatusItem)?.LayerId ??
                GetLayerFromNode(treeViewLayers.SelectedNode)?.Id;

            List<ActiveLayerStatusItem> layerItems = [];
            foreach (TreeNode groupNode in treeViewLayers.Nodes)
            {
                foreach (TreeNode layerNode in groupNode.Nodes)
                {
                    CanvasLayer? layer = GetLayerFromNode(layerNode);
                    if (layer != null)
                    {
                        layerItems.Add(new ActiveLayerStatusItem(layer.Id, layer.Name));
                    }
                }
            }

            _suppressActiveLayerEvents = true;
            try
            {
                _activeLayerCombo.Items.Clear();
                foreach (ActiveLayerStatusItem item in layerItems)
                    _activeLayerCombo.Items.Add(item);

                bool hasLayers = layerItems.Count > 0;
                _activeLayerLabel.Visible = hasLayers;
                _activeLayerCombo.Visible = hasLayers;

                if (!hasLayers)
                    return;

                ActiveLayerStatusItem selectedItem =
                    layerItems.FirstOrDefault(item => item.LayerId == selectedLayerId) ??
                    layerItems[0];

                _activeLayerCombo.SelectedItem = selectedItem;
            }
            finally
            {
                _suppressActiveLayerEvents = false;
            }
        }

        private static bool IsLayerNode(TreeNode? node)
        {
            return node?.Tag is LayerTreeNodeState state &&
                   state.IsLayerNode &&
                   state.Layer != null;
        }

        private static bool IsRasterLayerGroupNode(TreeNode? node)
        {
            return string.Equals(
                node?.Name,
                $"{LayerGroupNodeNamePrefix}{RasterLayerGroupKey}",
                StringComparison.OrdinalIgnoreCase);
        }

        private static CanvasLayer? GetLayerFromNode(TreeNode? node)
        {
            return node?.Tag is LayerTreeNodeState state &&
                   state.IsLayerNode
                ? state.Layer
                : null;
        }

        private static bool IsRasterLayer(CanvasLayer? layer)
        {
            return CanvasLayerBoundsService.IsRasterLayer(layer);
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

        private Rectangle GetLayerNodeTextRect(TreeNode node, int textStartX)
        {
            return new Rectangle(
                textStartX,
                node.Bounds.Y,
                Math.Max(1, treeViewLayers.ClientSize.Width - textStartX - 4),
                node.Bounds.Height);
        }

        private Rectangle GetLayerRenameTextBoxRect(TreeNode node)
        {
            Rectangle checkBoxRect = GetLayerNodeCheckBoxRect(node);
            int x = checkBoxRect.Right + LayerNodeCheckBoxGap;
            int width = Math.Max(80, treeViewLayers.ClientSize.Width - x - 4);

            int height = Math.Min(
                treeViewLayers.ItemHeight - 2,
                _layerRenameTextBox.PreferredHeight);
            int y = node.Bounds.Y + Math.Max(0, (treeViewLayers.ItemHeight - height) / 2);

            return new Rectangle(x, y, width, height);
        }

        private async Task OpenLayerPropertyManagerAsync(TreeNode? node)
        {
            if (node == null ||
                node.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsLayerNode ||
                nodeState.Layer == null)
            {
                return;
            }

            CanvasLayer editableLayer =
                _layerCommandService.CreateEditableCopy(nodeState.Layer);

            if (nodeState.Layer.IsLocked)
            {
                ShowLayerLockedMessage(nodeState.Layer, "edited");
                return;
            }

            using var frm = new frmLayerPropertyManager(editableLayer);
            PositionLayerPropertyManager(frm);

            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                CanvasLayer updatedLayer =
                    await _layerCommandService.UpdatePropertiesAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        editableLayer);

                bool isRaster = IsRasterLayer(updatedLayer);
                UpdateLayerNode(node, updatedLayer, updateRasterStack: !isRaster);
                MarkProjectModifiedIfOpen();

                if (isRaster)
                {
                    if (!mapCanvasControlMain.SetRasterLayerRenderState(
                        updatedLayer.Id,
                        updatedLayer.IsVisible,
                        updatedLayer.FillTransparency))
                    {
                        UpdateRasterCanvasLayersFromTree();
                    }

                    SetCanvasCommandStatus($"Raster layer updated: {updatedLayer.Name}");
                }
                else
                {
                    mapCanvasControlMain.RequestRender();
                    SetCanvasCommandStatus($"Layer properties updated: {updatedLayer.Name}");
                }

                SelectLayerNodeById(updatedLayer.Id);
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

        private static string GetMostUsefulExceptionMessage(Exception exception)
        {
            Exception current = exception;

            while (current.InnerException != null)
                current = current.InnerException;

            return current.Message;
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

        /// <summary>
        /// Shows operation progress in the map canvas status bar.
        /// </summary>
        private void SetOperationProgress(int percent, string status)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetOperationProgress(percent, status)));
                return;
            }

            int clampedPercent = Math.Clamp(percent, 0, 100);
            _operationProgressStatus.Text = status;
            _operationProgressStatus.Visible = true;
            _operationProgressBar.Value = clampedPercent;
            _operationProgressBar.Invalidate();
            _operationProgressHost.Visible = true;
            statusCanvas.Refresh();
        }

        /// <summary>
        /// Hides operation progress after a project or canvas operation finishes, fails, or is cancelled.
        /// </summary>
        private void HideOperationProgress()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideOperationProgress));
                return;
            }

            _operationProgressStatus.Text = string.Empty;
            _operationProgressStatus.Visible = false;
            _operationProgressBar.Value = 0;
            _operationProgressBar.Invalidate();
            _operationProgressHost.Visible = false;
            statusCanvas.Refresh();
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

                _canvasCommandStatus.ForeColor = SystemColors.ControlText;
                _canvasCommandStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
                _canvasCommandStatus.BorderStyle = Border3DStyle.Flat;

                _activeLayerLabel.ForeColor = SystemColors.ControlText;
                _activeLayerCombo.DropDownWidth = 260;

                lblCanvasCoordinates.Alignment = ToolStripItemAlignment.Right;
                lblCanvasCoordinates.Spring = false;
                lblCanvasCoordinates.AutoSize = true;
                lblCanvasCoordinates.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                lblCanvasCoordinates.ForeColor = SystemColors.ControlText;
                lblCanvasCoordinates.BorderSides = ToolStripStatusLabelBorderSides.Left;
                lblCanvasCoordinates.BorderStyle = Border3DStyle.RaisedOuter;
                lblCanvasCoordinates.Margin = new Padding(0, 3, 6, 2);

                _operationProgressStatus.ForeColor = SystemColors.ControlText;
                _operationProgressStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
                _operationProgressStatus.BorderStyle = Border3DStyle.Flat;
                _operationProgressHost.Visible = false;

                // Rebuild order deterministically.
                statusCanvas.Items.Clear();
                statusCanvas.Items.Add(lblCanvasMode);
                statusCanvas.Items.Add(_canvasCommandStatus);
                statusCanvas.Items.Add(_statusSpacer);
                statusCanvas.Items.Add(_activeLayerLabel);
                statusCanvas.Items.Add(_activeLayerCombo);
                statusCanvas.Items.Add(_operationProgressStatus);
                statusCanvas.Items.Add(_operationProgressHost);
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
            SetCanvasCommandStatus(
                mode.Replace("Mode:", "Canvas:", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Small status-strip progress bar that paints the current percentage inside the bar.
        /// </summary>
        private sealed class StatusProgressBar : ProgressBar
        {
            public StatusProgressBar()
            {
                SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Rectangle bounds = ClientRectangle;
                e.Graphics.Clear(SystemColors.Control);

                Rectangle track = new(
                    bounds.X,
                    bounds.Y,
                    Math.Max(1, bounds.Width - 1),
                    Math.Max(1, bounds.Height - 1));

                using Pen borderPen = new(SystemColors.ControlDark);
                e.Graphics.DrawRectangle(borderPen, track);

                double range = Math.Max(1, Maximum - Minimum);
                double progress = Math.Clamp((Value - Minimum) / range, 0.0, 1.0);
                Rectangle fill = new(
                    bounds.X + 1,
                    bounds.Y + 1,
                    Math.Max(0, (int)Math.Round((bounds.Width - 2) * progress)),
                    Math.Max(0, bounds.Height - 2));

                if (fill.Width > 0)
                {
                    using Brush fillBrush = new SolidBrush(Color.FromArgb(72, 136, 201));
                    e.Graphics.FillRectangle(fillBrush, fill);
                }

                TextRenderer.DrawText(
                    e.Graphics,
                    $"{Value}%",
                    Font,
                    bounds,
                    SystemColors.ControlText,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine);
            }

        }
    }
}

