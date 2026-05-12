
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Assignment;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Forms;
using Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Assignment;
using Land_Readjustment_Tool.Services.Canvas;
using Land_Readjustment_Tool.Services.Import;
using Land_Readjustment_Tool.Services.Project;
using Land_Readjustment_Tool.Services.Raster;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Dialogs;
using Land_Readjustment_Tool.UI.Forms;
using Land_Readjustment_Tool.UI.Forms.Project;
using Land_Readjustment_Tool.UI.Helpers;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OSGeo.OSR;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using NtsEnvelope = NetTopologySuite.Geometries.Envelope;
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
        private readonly IProjectRasterCrsResolver _projectRasterCrsResolver;
        private readonly IRasterLayerImportService _rasterLayerImportService;
        private readonly RasterImportFileManagementService _rasterImportFileManagementService;
        private readonly IXyzTileSourceService _xyzTileSourceService;
        private readonly IBoundaryReaderFactory _boundaryReaderFactory;
        private readonly IBoundaryImportService _boundaryImportService;
        private readonly ICadastralImportService _cadastralImportService;
        private readonly IProjectBoundaryAssignmentService _projectBoundaryAssignmentService;
        private readonly ICadastralRecordAssignmentService _cadastralRecordAssignmentService;
        private readonly XyzTilePreDownloadService _xyzTilePreDownloadService;
        private readonly IHatchPatternService _hatchPatternService;
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
        private string? _currentProjectRasterSrsDefinition;
        private bool _suppressLayerTreeEvents;
        private frmOperationProgress? _operationProgressForm;
        private bool _operationProgressActive;
        private const string LayerGroupNodeNamePrefix = "LayerGroup_";
        private const string RePlotRootNodeKey = "RePlotRoot";
        private const string OriginalDataGroupKey = CanvasLayerTreeService.OriginalDataGroupKey;
        private const string CadastralMapGroupKey = "CadastralMap";
        private const string BlockLayoutGroupKey = CanvasLayerTreeService.BlockLayoutGroupKey;
        private const string RoadsGroupKey = CanvasLayerTreeService.RoadsGroupKey;
        private const string ReplottedParcelsGroupKey = CanvasLayerTreeService.ReplottedParcelsGroupKey;
        private const string DrawingMarkupGroupKey = CanvasLayerTreeService.DrawingMarkupGroupKey;
        private const string RasterLayerGroupKey = CanvasLayerTreeService.RasterGroupKey;
        private const int LayerNodeCheckBoxSize = 14;
        private const int LayerNodeCheckBoxGap = 10;
        private const int LayerNodeColorBoxSize = 18;
        private const int LayerNodeColorBoxGap = 4;
        private readonly ContextMenuStrip _layerContextMenu = new();

        private readonly ToolStripMenuItem _mnuZoomToLayer = new("Zoom To Layer");
        private readonly ToolStripMenuItem _mnuRenameLayer = new("Rename");
        private readonly ToolStripMenuItem _mnuDeleteLayer = new("Delete");
        private readonly ToolStripMenuItem _mnuMoveLayerUp = new("Shift Layer Up");
        private readonly ToolStripMenuItem _mnuMoveLayerDown = new("Shift Layer Down");
        private readonly ToolStripMenuItem _mnuToggleLayerVisibility = new("Hidden");
        private readonly ToolStripMenuItem _mnuToggleLayerLock = new("Locked");
        private readonly ToolStripMenuItem _mnuLayerProperties = new("Layer Properties");
        private readonly ToolStripMenuItem _mnuAddDrawingLayer = new("Add Drawing Layer...");
        private readonly ToolStripMenuItem _mnuSetActiveLayer = new("Set Active Layer");
        private readonly ToolStripMenuItem _mnuAddRasterMap = new("Add Raster Map...");
        private readonly ToolStripMenuItem _mnuAddXyzTiles = new("Add XYZ Tiles...");
        private readonly ToolStripMenuItem _mnuImportXyzTiles = new("Import XYZ Tiles...");
        private readonly TextBox _layerRenameTextBox = new();
        private TreeNode? _contextLayerNode;
        private TreeNode? _contextLayerGroupNode;
        private TreeNode? _renamingLayerNode;
        private bool _isCompletingLayerRename;
        private frmXyzTileImportOptions? _xyzTileImportOptionsForm;
        private readonly ToolStripStatusLabel _liveTileFetchStatus = new();
        private readonly System.Windows.Forms.Timer _liveTileFetchTimer = new();
        private readonly List<Image> _liveTileFetchFrames = new List<Image>();
        private Image? _liveTileStaticGlobe;
        private Image? _liveTileDisconnectedGlobe;
        private int _liveTileFetchFrameIndex;
        private bool _suppressCurrentDrawingLayerSelectionChanged;
        private CanvasLayer? _currentDrawingLayer;
        private MapCanvasTool _currentCanvasTool = MapCanvasTool.Select;

        private sealed class LayerTreeNodeState
        {
            public bool IsLayerNode { get; init; }
            public bool IsCheckableGroup { get; init; }
            public bool IsGroupCheckedWhenEmpty { get; set; }
            public string? GroupKey { get; init; }
            public CanvasLayer? Layer { get; set; }
            public bool IsOnlineBasemap { get; set; }
        }

        private sealed class DrawingLayerComboItem(CanvasLayer layer)
        {
            public CanvasLayer Layer { get; } = layer;

            public override string ToString()
            {
                return Layer.Name;
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
                new ProjectRasterCrsResolver(projectScopedFactory),
                new RasterLayerProjectionService(
                    projectScopedFactory,
                    new ProjectRasterCrsResolver(projectScopedFactory),
                    new GdalRasterDatasetImporter()),
                new RasterLayerImportService(
                    projectScopedFactory,
                    new ProjectRasterCrsResolver(projectScopedFactory),
                    new GdalRasterDatasetImporter()),
                new RasterImportFileManagementService(projectScopedFactory),
                new XyzTileSourceService(),
                new BoundaryReaderFactory(
                    new Services.Import.Readers.DxfBoundaryReader(),
                    new Services.Import.Readers.ShpBoundaryReader(),
                    new Services.Import.Readers.KmlBoundaryReader()),
                new BoundaryImportService(
                    new BoundaryReaderFactory(
                        new Services.Import.Readers.DxfBoundaryReader(),
                        new Services.Import.Readers.ShpBoundaryReader(),
                        new Services.Import.Readers.KmlBoundaryReader()),
                    projectScopedFactory,
                    new ProjectRasterCrsResolver(projectScopedFactory)),
                new CadastralImportService(new ProjectRasterCrsResolver(projectScopedFactory)),
                new ProjectBoundaryAssignmentService(projectScopedFactory),
                new CadastralRecordAssignmentService(),
                new HatchPatternService(),
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
            IProjectRasterCrsResolver projectRasterCrsResolver,
            RasterLayerProjectionService rasterLayerProjectionService,
            IRasterLayerImportService rasterLayerImportService,
            RasterImportFileManagementService rasterImportFileManagementService,
            IXyzTileSourceService xyzTileSourceService,
            IBoundaryReaderFactory boundaryReaderFactory,
            IBoundaryImportService boundaryImportService,
            ICadastralImportService cadastralImportService,
            IProjectBoundaryAssignmentService projectBoundaryAssignmentService,
            ICadastralRecordAssignmentService cadastralRecordAssignmentService,
            IHatchPatternService hatchPatternService,
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
            _projectRasterCrsResolver = projectRasterCrsResolver ?? throw new ArgumentNullException(nameof(projectRasterCrsResolver));
            _rasterLayerProjectionService = rasterLayerProjectionService ?? throw new ArgumentNullException(nameof(rasterLayerProjectionService));
            _rasterLayerImportService = rasterLayerImportService ?? throw new ArgumentNullException(nameof(rasterLayerImportService));
            _rasterImportFileManagementService = rasterImportFileManagementService ?? throw new ArgumentNullException(nameof(rasterImportFileManagementService));
            _xyzTileSourceService = xyzTileSourceService ?? throw new ArgumentNullException(nameof(xyzTileSourceService));
            _boundaryReaderFactory = boundaryReaderFactory ?? throw new ArgumentNullException(nameof(boundaryReaderFactory));
            _boundaryImportService = boundaryImportService ?? throw new ArgumentNullException(nameof(boundaryImportService));
            _cadastralImportService = cadastralImportService ?? throw new ArgumentNullException(nameof(cadastralImportService));
            _projectBoundaryAssignmentService = projectBoundaryAssignmentService ?? throw new ArgumentNullException(nameof(projectBoundaryAssignmentService));
            _cadastralRecordAssignmentService = cadastralRecordAssignmentService ?? throw new ArgumentNullException(nameof(cadastralRecordAssignmentService));
            _xyzTilePreDownloadService = new XyzTilePreDownloadService();
            _hatchPatternService = hatchPatternService ?? throw new ArgumentNullException(nameof(hatchPatternService));
            _projectOpenService = projectOpenService ?? throw new ArgumentNullException(nameof(projectOpenService));
            _projectSaveAsService = projectSaveAsService ?? throw new ArgumentNullException(nameof(projectSaveAsService));

            InitializeComponent();
            hostOperationProgress = hostProgressBarHost;
            NumericUpDownSelectAllBehavior.AttachTo(this);
            _startupFilePath = startupFilePath;
            ConfigureSmoothSplitterLayout();
            ConfigureStatusStripSizing();
            ConfigureLiveTileFetchStatusIndicator();
            XyzLiveTileRenderLayer.FetchStatusChanged += XyzLiveTileRenderLayer_FetchStatusChanged;
            FormClosed += frmMain_FormClosed;
            mapCanvasControlMain.StatusChanged += MapCanvasControlMain_StatusChanged;
            mapCanvasControlMain.CommandService.PromptChanged += prompt =>
            {
                if (!IsDisposed && !Disposing)
                    lblStatusMessage.Text = prompt;
            };
            mapCanvasControlMain.ShapeCompleted += MapCanvasControlMain_ShapeCompleted;
            mapCanvasControlMain.SelectToolRequested += () => ActivateCanvasTool(MapCanvasTool.Select);
            mapCanvasControlMain.SelectedObjectsDeleteRequested += MapCanvasControlMain_SelectedObjectsDeleteRequested;
            mnuCanvasDebugOverlay.Checked = mapCanvasControlMain.ShowDebugOverlay;
            mnuOSnapToggle.Checked = mapCanvasControlMain.SnapEnabled;
            mnuOrthoToggle.Checked = mapCanvasControlMain.OrthoModeEnabled;
            ConfigureLayerTree();
            ConfigureLayerPropertiesPanel();
            MapCanvasControlMain_StatusChanged("E: --    N: --", "Ready", 0);


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
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Click += ImportProjectBoundaryToolStripMenuItem_Click!;
            projectBoundaryAssignmentToolStripMenuItem.Click += ProjectBoundaryAssignmentToolStripMenuItem_Click!;
            _mnuImportXyzTiles.Click += importXyzTilesToolStripMenuItem_Click!;
            importDataToolStripMenuItem1.DropDownItems.Add(_mnuImportXyzTiles);

        }

        private void ConfigureStatusStripSizing()
        {
            lblScale.AutoSize = true;
            lblCanvasCoordinates.AutoSize = true;
            lblScale.TextAlign = ContentAlignment.MiddleRight;
            lblCanvasCoordinates.TextAlign = ContentAlignment.MiddleRight;
        }

        private void ConfigureLiveTileFetchStatusIndicator()
        {
            _liveTileStaticGlobe =
                LoadStatusIconImage("globe.gif") ??
                CreateEarthSpinnerFrame(0, 12);
            _liveTileDisconnectedGlobe =
                LoadStatusIconImage("globe_disconnected.gif") ??
                _liveTileStaticGlobe;
            _liveTileFetchFrames.AddRange(
                LoadEarthSpinnerFramesFromGif() ?? CreateEarthSpinnerFrames());

            _liveTileFetchStatus.Name = "liveTileFetchStatus";
            _liveTileFetchStatus.Alignment = ToolStripItemAlignment.Right;
            _liveTileFetchStatus.AutoSize = false;
            _liveTileFetchStatus.Size = new Size(24, 21);
            _liveTileFetchStatus.BorderSides = ToolStripStatusLabelBorderSides.None;
            _liveTileFetchStatus.DisplayStyle = ToolStripItemDisplayStyle.Image;
            _liveTileFetchStatus.ImageAlign = ContentAlignment.MiddleCenter;
            _liveTileFetchStatus.Margin = new Padding(4, 3, 2, 2);
            _liveTileFetchStatus.Text = string.Empty;
            _liveTileFetchStatus.ToolTipText = string.Empty;
            _liveTileFetchStatus.Visible = true;
            _liveTileFetchStatus.Image = _liveTileStaticGlobe;

            PlaceLiveTileFetchStatusLeftOfCoordinates();

            _liveTileFetchTimer.Interval = 110;
            _liveTileFetchTimer.Tick += (_, _) =>
            {
                if (_liveTileFetchFrames.Count == 0)
                {
                    return;
                }

                _liveTileFetchFrameIndex =
                    (_liveTileFetchFrameIndex + 1) % _liveTileFetchFrames.Count;
                _liveTileFetchStatus.Image =
                    _liveTileFetchFrames[_liveTileFetchFrameIndex];
                _liveTileFetchStatus.Invalidate();
            };

        }

        // Designer-referenced event handlers (minimal implementations)
        private async void mnuDrawArc_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Arc);
        }

        private void cboCurrentDrawingLayer_Click(object sender, EventArgs e)
        {
            try
            {
                cboCurrentDrawingLayer.DroppedDown = true;
            }
            catch
            {
                // ignore UI errors
            }
        }

        private void PlaceLiveTileFetchStatusLeftOfCoordinates()
        {
            if (statusCanvas.Items.Contains(_liveTileFetchStatus))
            {
                statusCanvas.Items.Remove(_liveTileFetchStatus);
            }

            int coordinateIndex = statusCanvas.Items.IndexOf(lblCanvasCoordinates);
            if (coordinateIndex >= 0)
            {
                // Right-aligned StatusStrip items are arranged right-to-left.
                // Inserting after the coordinate item renders the globe to its left.
                statusCanvas.Items.Insert(coordinateIndex + 1, _liveTileFetchStatus);
                return;
            }

            statusCanvas.Items.Add(_liveTileFetchStatus);
        }

        private static List<Image> CreateEarthSpinnerFrames()
        {
            const int frameCount = 12;
            List<Image> frames = new(frameCount);
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames.Add(CreateEarthSpinnerFrame(frame, frameCount));
            }

            return frames;
        }

        private static Bitmap CreateEarthSpinnerFrame(int frame, int frameCount)
        {
            const int size = 18;
            Bitmap bitmap = new(size, size, PixelFormat.Format32bppPArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            RectangleF globe = new(2.2f, 2.2f, 13.6f, 13.6f);
            using Brush oceanBrush = new SolidBrush(Color.FromArgb(255, 43, 129, 207));
            using Pen outlinePen = new(Color.FromArgb(255, 25, 88, 150), 1.2f);
            using Pen gridPen = new(Color.FromArgb(175, 228, 244, 255), 1f);
            using Pen landPen = new(Color.FromArgb(235, 79, 157, 104), 2f);
            using GraphicsPath globePath = new();

            graphics.FillEllipse(oceanBrush, globe);
            globePath.AddEllipse(globe);
            graphics.SetClip(globePath);

            float phase = frame / (float)frameCount;
            float offset = phase * globe.Width;
            for (int i = -2; i <= 2; i++)
            {
                float x = globe.Left + ((i * globe.Width / 2f + offset) % globe.Width);
                if (x < globe.Left)
                {
                    x += globe.Width;
                }

                float distanceFromCenter = Math.Abs(x - (globe.Left + globe.Width / 2f));
                float arcWidth = Math.Max(1.5f, globe.Width - distanceFromCenter * 2f);
                RectangleF meridian = new(
                    x - arcWidth / 2f,
                    globe.Top,
                    arcWidth,
                    globe.Height);
                graphics.DrawArc(gridPen, meridian, 90, 180);
            }

            graphics.DrawLine(
                gridPen,
                globe.Left + 1.5f,
                globe.Top + globe.Height / 2f,
                globe.Right - 1.5f,
                globe.Top + globe.Height / 2f);

            float landOffset = (phase * 8f) - 4f;
            graphics.DrawArc(
                landPen,
                new RectangleF(globe.Left + 2f + landOffset, globe.Top + 3f, 8f, 5f),
                190,
                170);
            graphics.DrawArc(
                landPen,
                new RectangleF(globe.Left + 6f + landOffset, globe.Top + 8f, 7f, 4f),
                20,
                145);

            graphics.ResetClip();
            graphics.DrawEllipse(outlinePen, globe);
            return bitmap;
        }

        private static List<Image>? LoadEarthSpinnerFramesFromGif()
        {
            string? gifPath = FindStatusResourcePath(
                Path.Combine("Status", "spinning-globe.gif"));
            if (gifPath == null)
            {
                return null;
            }

            try
            {
                using Image gif = Image.FromFile(gifPath);
                FrameDimension dimension = new(gif.FrameDimensionsList[0]);
                int frameCount = gif.GetFrameCount(dimension);
                if (frameCount <= 0)
                {
                    return null;
                }

                List<Image> frames = new(frameCount);
                for (int frame = 0; frame < frameCount; frame++)
                {
                    gif.SelectActiveFrame(dimension, frame);
                    frames.Add(CreateStatusIconFrame(gif));
                }

                return frames;
            }
            catch
            {
                return null;
            }
        }

        private static Image? LoadStatusIconImage(string fileName)
        {
            string? imagePath = FindStatusResourcePath(
                Path.Combine("Status", fileName));
            if (imagePath == null)
            {
                return null;
            }

            try
            {
                using Image image = Image.FromFile(imagePath);
                return CreateStatusIconFrame(image);
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap CreateStatusIconFrame(Image source)
        {
            const int size = 18;
            Bitmap frame = new(size, size, PixelFormat.Format32bppPArgb);
            using Graphics graphics = Graphics.FromImage(frame);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.Clear(Color.Transparent);
            graphics.DrawImage(
                source,
                new Rectangle(0, 0, size, size),
                new Rectangle(0, 0, source.Width, source.Height),
                GraphicsUnit.Pixel);
            return frame;
        }

        private static string? FindStatusResourcePath(string relativeResourcePath)
        {
            string outputPath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources",
                relativeResourcePath);
            if (File.Exists(outputPath))
            {
                return outputPath;
            }

            DirectoryInfo? current = new(AppContext.BaseDirectory);
            while (current != null)
            {
                string sourcePath = Path.Combine(
                    current.FullName,
                    "Resources",
                    relativeResourcePath);
                if (File.Exists(sourcePath))
                {
                    return sourcePath;
                }

                current = current.Parent;
            }

            return null;
        }

        private void ConfigureSmoothSplitterLayout()
        {
            // Prevent property panel from collapsing into a broken layout.
            mainSplitContainer.Panel1MinSize = 250;

            EnableDoubleBuffering(mainSplitContainer);
            EnableDoubleBuffering(leftSplitContainer);
            EnableDoubleBuffering(splitContainer3);


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

            tabProperties.SuspendLayout();
            tabProperties.ResumeLayout(true);
            tabProperties.Invalidate(true);
            tabProperties.Update();
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
                UpdateProjectNameStatus();
                return;
            }

            var name = AppServices.Context.Info.ProjectName;
            this.Text = AppServices.Context.HasUnsavedChanges ? $"{name}* - {_appTitle}" : $"{name} - {_appTitle}";
            UpdateProjectNameStatus();
        }

        private void UpdateProjectNameStatus()
        {
            if (!AppServices.HasContext || AppServices.Context.Info == null)
            {
                lblProjectName.Text = "● No Project";
                lblProjectName.ForeColor = SystemColors.GrayText;
                return;
            }

            string name = AppServices.Context.Info.ProjectName;
            bool unsaved = AppServices.Context.HasUnsavedChanges;
            lblProjectName.Text = unsaved ? $"● {name} *" : $"● {name}";
            lblProjectName.ForeColor = unsaved ? Color.DarkOrange : Color.SeaGreen;
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

        private void frmMain_FormClosed(object? sender, FormClosedEventArgs e)
        {
            XyzLiveTileRenderLayer.FetchStatusChanged -= XyzLiveTileRenderLayer_FetchStatusChanged;
            _liveTileFetchTimer.Stop();
            _liveTileFetchTimer.Dispose();

            foreach (Image frame in _liveTileFetchFrames)
            {
                frame.Dispose();
            }

            _liveTileFetchFrames.Clear();
            Image? disconnectedGlobe = _liveTileDisconnectedGlobe;
            if (!ReferenceEquals(disconnectedGlobe, _liveTileStaticGlobe))
            {
                disconnectedGlobe?.Dispose();
            }

            _liveTileDisconnectedGlobe = null;
            _liveTileStaticGlobe?.Dispose();
            _liveTileStaticGlobe = null;
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
                ProjectContext context = AppServices.Context;
                string filePath = context.ProjectFilePath;
                ReleaseProjectRenderingResources();

                // Step 1 — Checkpoint BEFORE closing the connection.
                //   TRUNCATE empties the WAL file in-place while we still
                //   hold the connection, so there is no file-lock conflict.
                //   An empty WAL cannot be replayed on next open.
                ProjectWalCheckpoint.Execute(filePath);

                // Step 2 — Dispose session and clear the connection pool.
                //   ClearAllPools() forces Microsoft.Data.Sqlite to close
                //   every pooled connection immediately, releasing the OS
                //   file handles on .lpp, .lpp-wal and .lpp-shm.
                context.Session.Dispose();
                SqliteConnection.ClearAllPools();

                // Step 3 — Restore .lpp from the last backup.
                if (!_backupService.RollbackToLatest(filePath))
                    MessageBox.Show(
                        "Could not restore the project to its last saved state — " +
                        "no backup file was found.",
                        "Restore Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                else
                    _rasterImportFileManagementService.CleanupUnsavedImports(context);

                AppServices.ClearContext();

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
                // Release renderer-owned SQLite/GDAL handles before closing
                // the project DB session and clearing pooled SQLite connections.
                ReleaseProjectRenderingResources();
                AppServices.Context.StateChanged -= UpdateWindowTitle;
                AppServices.Context.Session.Dispose();
            }
            finally
            {
                AppServices.ClearContext();
                SqliteConnection.ClearAllPools();
            }
        }

        private void ReleaseProjectRenderingResources()
        {
            try
            {
                mapCanvasControlMain.ClearRasterLayers();
            }
            catch (Exception ex)
            {
                LogProjectError(
                    "Failed to release raster rendering resources while closing project.",
                    ex);
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
            SetDrawingToolButtonsEnabled(true);
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
            SetDrawingToolButtonsEnabled(false);
            toolStripComboBox1.Enabled = false;
        }

        private void SetDrawingToolButtonsEnabled(bool enabled)
        {
            mnuSelectTool.Enabled = enabled;
            mnuDrawPoint.Enabled = enabled;
            mnuDrawLine.Enabled = enabled;
            mnuDrawPolyline.Enabled = enabled;
            mnuDrawPolygon.Enabled = enabled;
            mnuDrawRectangle.Enabled = enabled;
            mnuDrawCircle.Enabled = enabled;
            mnuDrawArc.Enabled = enabled;
            lblCurrentDrawingLayer.Enabled = enabled;
            cboCurrentDrawingLayer.Enabled = enabled;

            if (enabled)
            {
                UpdateDrawingToolAvailability();
            }
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
                Title = "Create New Project",
                RestoreDirectory = true
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

                SetOperationProgress(45, "Creating new project.....");

                // Create project in database
                var info = await _projectService.CreateNewProjectAsync(projectFilePath, projectFileName);

                // Set info on context
                context.SetInfo(info);

                // Enable menu items
                EnableProjectMenuItems();
                UpdateWindowTitle();

                // Open project details form
                HideOperationProgress();
                OpenProjectDetails(info);
                SetOperationProgress(58, "Configuring project settings");
                await PromptProjectSettingsAsync(showPrompt: false);
                SetOperationProgress(65, "Loading project workspace");

                InitializeProjectWorkspace();
                await ApplySettingsAsync(showRefreshProgress: false);
                await RefreshMapCanvasAsync("Opening project canvas", 75);
            }
            catch (Exception ex)
            {
                HideOperationProgress();
                AppServices.ClearContext();
                LogProjectError("Project creation failed.", ex);
                MessageBox.Show(
                    $"Failed to create project: {GetMostUsefulExceptionMessage(ex)}",
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
                SetOperationProgress(8, "Saving project view");
                PersistProjectUiStateAsync().GetAwaiter().GetResult();
                SetOperationProgress(18, "Saving project");
                _rasterImportFileManagementService
                    .RepairRasterLayerReferencesAsync(AppServices.Context)
                    .GetAwaiter()
                    .GetResult();
                SetOperationProgress(38, "Cleaning project workspace");
                _rasterImportFileManagementService
                    .CleanupUnreferencedProjectRastersAsync(AppServices.Context)
                    .GetAwaiter()
                    .GetResult();

                string filePath = AppServices.Context.ProjectFilePath;

                // Repositories write immediately — no pending changes to flush.
                // Save = WAL checkpoint + backup rotation.

                // 1. WAL checkpoint — merge WAL journal into the .lpp file
                SetOperationProgress(64, "Writing project database");
                ProjectWalCheckpoint.Execute(filePath);

                // 2. Rotate backups
                SetOperationProgress(80, "Updating project backup");
                _backupService.CreateBackup(filePath);

                _rasterImportFileManagementService.CommitPendingDeletes(
                    AppServices.Context);

                // 3. Mark clean
                AppServices.Context.MarkAsSaved();
                SetOperationProgress(100, "Project saved");
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
            finally
            {
                HideOperationProgress();
            }
        }
        private async Task PromptProjectSettingsAsync(bool showPrompt = true)
        {
            if (showPrompt)
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
            }
            else
            {
                OpenProjectSettings();
            }


            // Mark as configured regardless of choice —
            // prevents this prompt from showing again on next open.
            SetOperationProgress(60, "Saving project settings");
            var repo = _projectScopedFactory.CreateProjectSettingsRepository(
                AppServices.Context.Session);
            await repo.MarkAsConfiguredAsync();

            SetOperationProgress(62, "Applying workspace settings");
            await ApplySettingsAsync(showRefreshProgress: false);
            await EnsureDefaultGoogleSatelliteLayerAsync(isInitiallyVisible: false);

            // Create the very first project backup.
            // This happens AFTER project info + settings are saved,
            // so the .bak captures the correct initial project state.
            // All future discards will restore to this checkpoint.
            if (AppServices.HasContext)
            {
                var filePath = AppServices.Context.ProjectFilePath;

                // WAL checkpoint — flush all writes into the .lpp file
                SetOperationProgress(68, "Writing initial project file");
                await ProjectWalCheckpoint.ExecuteAsync(filePath);
                SetOperationProgress(72, "Creating initial backup");

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

            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog(this) == DialogResult.OK && !appliedWhileOpen)
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

                await RefreshCurrentProjectRasterSrsDefinitionAsync();

                if (showRefreshProgress)
                    SetOperationProgress(35, "Applying canvas settings");

                var bgColor = ParseColorOrDefault(
                    settings.CanvasBackgroundColor, Color.White);
                var gridColor = ParseColorOrDefault(
                    settings.CanvasGridColor, Color.LightGray);

                mapCanvasControlMain.ApplyRenderSettings(
                    MapCanvasSettingsService.FromProjectSettings(settings));
                mapCanvasControlMain.ApplySnapSettings(
                    settings.SnapEnabled,
                    settings.SnapTolerancePx,
                    settings.SnapGlyphSizePx);
                mnuOSnapToggle.Checked = settings.SnapEnabled;
                mapCanvasControlMain.OrthoModeEnabled = settings.OrthoEnabled;
                mnuOrthoToggle.Checked = settings.OrthoEnabled;

                // Update layer colors to be theme-aware
                if (showRefreshProgress)
                    SetOperationProgress(45, "Updating layer colors for theme");
                await UpdateLayerColorsForCanvasThemeAsync(bgColor);

                if (_workspaceCanvas != null && !_workspaceCanvas.IsDisposed)
                {
                    if (showRefreshProgress)
                        SetOperationProgress(55, "Updating workspace canvas");

                    _workspaceCanvas.ApplyBackgroundColor(bgColor);
                    _workspaceCanvas.ApplyGridColor(gridColor);
                    _workspaceCanvas.ApplyGridVisible(
                        settings.CanvasGridVisible);
                    _workspaceCanvas.ApplySnapSettings(
                        settings.SnapEnabled,
                        settings.SnapTolerancePx,
                        settings.SnapGlyphSizePx);
                    _workspaceCanvas.OrthoModeEnabled = settings.OrthoEnabled;
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

            UnloadProjectWorkspace();
            DisposeCurrentProjectSession();
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
        private void OpenProjectDetails(ProjectInfo? initialProjectInfo = null)
        {
            var context = AppServices.Context;
            var service = _projectScopedFactory.CreateProjectInfoService(context.Session);

            using var frm = new frm_ProjectDetails(service, initialProjectInfo);
            frm.StartPosition = FormStartPosition.CenterParent;

            if (frm.ShowDialog(this) == DialogResult.OK)
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
                    FileName = Path.GetFileNameWithoutExtension(currentFilePath),
                    RestoreDirectory = true
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

                SetOperationProgress(8, "Saving project view");
                await PersistProjectUiStateAsync();
                SetOperationProgress(16, "Preparing Save As");
                await _rasterImportFileManagementService
                    .RepairRasterLayerReferencesAsync(AppServices.Context);
                SetOperationProgress(28, "Cleaning project workspace");
                await _rasterImportFileManagementService
                    .CleanupUnreferencedProjectRastersAsync(AppServices.Context);

                SetOperationProgress(44, "Copying project files");
                ProjectContext newContext =
                    await _projectSaveAsService.SaveAsAsync(
                        currentFilePath,
                        target);

                SetOperationProgress(72, "Opening saved project");
                DisposeCurrentProjectSession();
                AppServices.SetContext(newContext);
                newContext.StateChanged += UpdateWindowTitle;
                _layerTreeService = _projectScopedFactory.CreateCanvasLayerTreeService(
                    newContext.Session);

                EnableProjectMenuItems();
                UpdateWindowTitle();
                SetOperationProgress(86, "Refreshing saved project canvas");
                await RefreshMapCanvasAsync("Refreshing saved project canvas");
                await RestoreCanvasViewportStateAsync();

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
                HideOperationProgress();
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

            await RefreshLayerTreeAsync(rebuildRasterLayersAfterCrsChange: true);

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

        private async Task RefreshLiveTileLayersForCurrentProjectAsync()
        {
            if (!AppServices.HasContext)
                return;

            RasterLayerProjectionUpdateResult result =
                await _rasterLayerProjectionService
                    .RefreshLiveTileLayersToProjectCrsAsync(
                        AppServices.Context.Session,
                        AppServices.Context.ProjectFolderPath);

            if (result.UpdatedCount > 0)
            {
                await RefreshLayerTreeAsync();
                mapCanvasControlMain.RequestRender();
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
            await ShowXyzTileImportOptionsFormAsync();
        }

        private async void ImportProjectBoundaryToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            await ShowImportProjectBoundaryWorkflowAsync();
        }

        private async void importCadastralDataDXFDWGShapefileToolStripMenuItem_Click(
            object? sender,
            EventArgs e)
        {
            await ShowImportCadastralMapWorkflowAsync();
        }

        private async Task ShowImportCadastralMapWorkflowAsync()
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
                Title = "Import Cadastral Map",
                Filter = "Cadastral map files (*.dxf;*.dwg;*.shp)|*.dxf;*.dwg;*.shp|DXF files (*.dxf)|*.dxf|DWG files (*.dwg)|*.dwg|Shapefiles (*.shp)|*.shp|All files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                using frmCadastralImport importForm = new(
                    AppServices.Context.Session,
                    dialog.FileName,
                    _cadastralImportService,
                    _projectScopedFactory,
                    _projectRasterCrsResolver);

                if (importForm.ShowDialog(this) != DialogResult.OK)
                    return;

                MarkProjectModifiedIfOpen();
                await RefreshLayerTreeAsync();

                CadastralImportResult? result = importForm.ImportResult;
                if (result?.BoundingBox != null && !result.BoundingBox.IsNull)
                    mapCanvasControlMain.ZoomToWorldBounds(ToRectangleD(result.BoundingBox));

                SetCanvasCommandStatus(
                    result == null
                        ? "Cadastral map imported."
                        : $"Imported {result.ObjectsCreated} cadastral parcel object(s).");
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    ex.Message,
                    "Cadastral Map Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                LogProjectError("Cadastral map import failed.", ex);
                MessageBox.Show(
                    $"Failed to import cadastral map: {GetMostUsefulExceptionMessage(ex)}",
                    "Cadastral Map Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task ShowImportProjectBoundaryWorkflowAsync()
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
                Title = "Import Project Boundary",
                Filter = "Boundary files (*.dxf;*.shp;*.kml;*.kmz)|*.dxf;*.shp;*.kml;*.kmz|DXF files (*.dxf)|*.dxf|Shapefiles (*.shp)|*.shp|KML/KMZ files (*.kml;*.kmz)|*.kml;*.kmz|All files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                if (_layerTreeService != null)
                    await _layerTreeService.GetLayerTreeAsync();

                using frmBoundaryImport importForm = new(
                    AppServices.Context.Session,
                    dialog.FileName,
                    _boundaryReaderFactory,
                    _boundaryImportService,
                    _projectScopedFactory,
                    _projectRasterCrsResolver);

                if (importForm.ShowDialog(this) != DialogResult.OK)
                    return;

                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();
                await RefreshLayerTreeAsync();

                CanvasLayer? boundaryLayer = await AppServices.Context.Session
                    .GetDbContext()
                    .CanvasLayers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        layer => layer.Name == "Project Boundary" ||
                                 layer.LayerType == "ProjectBoundary");

                if (boundaryLayer != null)
                {
                    SelectLayerNodeById(boundaryLayer.Id);
                    await TryZoomToLayerAsync(boundaryLayer, showErrorDialog: false);
                }

                BoundaryImportResult? result = importForm.ImportResult;
                SetCanvasCommandStatus(
                    result == null
                        ? "Project boundary imported."
                        : $"Imported {result.ObjectsCreated} project boundary object(s).");
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    ex.Message,
                    "Boundary Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                LogProjectError("Boundary import failed.", ex);
                MessageBox.Show(
                    $"Failed to import project boundary: {GetMostUsefulExceptionMessage(ex)}",
                    "Boundary Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void ProjectBoundaryAssignmentToolStripMenuItem_Click(object? sender, EventArgs e)
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

            try
            {
                await RefreshVectorCanvasFeaturesAsync();

                IReadOnlyList<ProjectBoundaryAssignmentCandidate> candidates =
                    await _projectBoundaryAssignmentService.GetCandidatesAsync(
                        AppServices.Context.Session);

                using frmProjectBoundaryAssignment form = new(candidates);
                form.CandidatePreviewRequested += PreviewAssignmentCandidateOnCanvas;

                DialogResult dialogResult = form.ShowDialog(this);
                form.CandidatePreviewRequested -= PreviewAssignmentCandidateOnCanvas;
                mapCanvasControlMain.ClearPreviewSelection();

                if (dialogResult != DialogResult.OK)
                    return;

                if (form.ImportProjectBoundaryRequested)
                {
                    await ShowImportProjectBoundaryWorkflowAsync();
                    return;
                }

                ProjectBoundaryAssignmentResult result;
                if (form.RemoveProjectBoundaryRequested)
                {
                    result = await _projectBoundaryAssignmentService
                        .RemoveProjectBoundaryAsync(AppServices.Context.Session);
                }
                else if (form.SelectedCandidateId.HasValue)
                {
                    result = await _projectBoundaryAssignmentService
                        .AssignProjectBoundaryAsync(
                            AppServices.Context.Session,
                            form.SelectedCandidateId.Value,
                            form.DeleteExistingBoundary);
                }
                else
                {
                    return;
                }

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.ErrorMessage ?? "Project Boundary assignment failed.",
                        "Project Boundary Assignment",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MarkProjectModifiedIfOpen();
                await RefreshLayerTreeAsync();

                CanvasLayer? boundaryLayer = await AppServices.Context.Session
                    .GetDbContext()
                    .CanvasLayers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        layer => layer.Name == "Project Boundary" ||
                                 layer.LayerType == "ProjectBoundary");

                if (boundaryLayer != null)
                    SelectLayerNodeById(boundaryLayer.Id);

                if (result.BoundingBox != null && !result.BoundingBox.IsNull)
                    mapCanvasControlMain.ZoomToWorldBounds(ToRectangleD(result.BoundingBox));

                SetCanvasCommandStatus(
                    form.RemoveProjectBoundaryRequested
                        ? $"Removed {result.ObjectsRemoved} Project Boundary object(s)."
                        : $"Assigned Project Boundary from drawing object. Removed {result.ObjectsRemoved} old object(s).");
            }
            catch (Exception ex)
            {
                LogProjectError("Project boundary assignment failed.", ex);
                MessageBox.Show(
                    $"Failed to assign Project Boundary: {GetMostUsefulExceptionMessage(ex)}",
                    "Project Boundary Assignment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void CadastralRecordsAssignmentToolStripMenuItem_Click(object? sender, EventArgs e)
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

            using frmCadastralRecordAssignment form = new(
                AppServices.Context.Session,
                _cadastralRecordAssignmentService);
            form.ShowDialog(this);

            if (!form.AssignmentChanged)
                return;

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            mapCanvasControlMain.RequestRender();
            SetCanvasCommandStatus("Cadastral records assigned.");
        }

        private void PreviewAssignmentCandidateOnCanvas(Guid? canvasObjectId, bool zoomToObject)
        {
            if (!canvasObjectId.HasValue)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            mapCanvasControlMain.PreviewSelectCanvasObject(
                canvasObjectId.Value,
                zoomToObject);
        }

        private static RectangleD ToRectangleD(NtsEnvelope envelope)
        {
            const double minimumExtent = 1.0;
            double minX = envelope.MinX;
            double minY = envelope.MinY;
            double width = envelope.Width;
            double height = envelope.Height;

            if (width <= 0)
            {
                minX -= minimumExtent / 2.0;
                width = minimumExtent;
            }

            if (height <= 0)
            {
                minY -= minimumExtent / 2.0;
                height = minimumExtent;
            }

            return new RectangleD(minX, minY, width, height);
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
                SetOperationProgress(2, "Reading raster details", showProgressForm: false);

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

                SetOperationProgress(2, "Starting raster import", showProgressForm: false);

                Progress<RasterImportProgressInfo> progress = new(
                    update => SetOperationProgress(update.Percent, update.Status, showProgressForm: false));

                RasterLayerImportResult importResult =
                    await _rasterLayerImportService.ImportAsync(
                        new RasterLayerImportRequest(
                            AppServices.Context.Session,
                            AppServices.Context.ProjectFolderPath,
                            dialog.FileName,
                            reviewForm.LayerName,
                            reviewForm.SourceSrsDefinitionOverride),
                        progress);

                SetOperationProgress(94, "Refreshing raster layer list", showProgressForm: false);

                _rasterImportFileManagementService.RegisterImportedRaster(
                    AppServices.Context,
                    importResult);
                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();

                await RefreshLayerTreeAsync();
                SelectLayerNodeById(importResult.Layer.Id);
                if (!await TryZoomToLayerAsync(importResult.Layer, showErrorDialog: false))
                {
                    mapCanvasControlMain.ZoomExtents();
                }

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

        private async Task ShowXyzTileImportOptionsFormAsync()
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

            if (_xyzTileImportOptionsForm != null &&
                !_xyzTileImportOptionsForm.IsDisposed)
            {
                _xyzTileImportOptionsForm.CurrentViewportBoundsProvider =
                    TryGetCurrentViewportGeographicBounds;

                if (_xyzTileImportOptionsForm.WindowState ==
                    FormWindowState.Minimized)
                {
                    _xyzTileImportOptionsForm.WindowState =
                        FormWindowState.Normal;
                }

                _xyzTileImportOptionsForm.BringToFront();
                _xyzTileImportOptionsForm.Focus();
                return;
            }

            _xyzTileImportOptionsForm =
                new frmXyzTileImportOptions(
                    AppServices.Context.ProjectFolderPath,
                    await LoadXyzTileOptionsStateAsync());
            _xyzTileImportOptionsForm.CurrentViewportBoundsProvider =
                TryGetCurrentViewportGeographicBounds;
            _xyzTileImportOptionsForm.ImportRequested +=
                async (_, args) => await ImportXyzTilesRequestAsync(args.Request);
            _xyzTileImportOptionsForm.OptionsStateChanged +=
                async (_, args) =>
                    await TryPersistXyzTileOptionsStateAsync(args.State);
            _xyzTileImportOptionsForm.FormClosed += (_, _) =>
            {
                _xyzTileImportOptionsForm = null;
            };
            _xyzTileImportOptionsForm.Show(this);
        }

        /// <summary>
        /// Imports an online XYZ tile source as a project raster layer.
        /// </summary>
        private async Task ImportXyzTilesRequestAsync(
            XyzTileSourceImportRequest request)
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

            _xyzTileImportOptionsForm?.BeginImportExecution(
                "Importing downloaded XYZ tiles...");

            try
            {
                if (request.IsLiveTiles)
                {
                    IReadOnlyList<CanvasLayer> existingBasemaps =
                        await FindExistingOnlineBasemapLayersAsync();
                    foreach (CanvasLayer existingBasemap in existingBasemaps)
                    {
                        await ReplaceRasterLayerAsync(existingBasemap);
                    }
                }

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
                        _xyzTileImportOptionsForm?.CompleteImportExecution(
                            "Import canceled. Layer replacement was not approved.");
                        return;
                    }

                    await ReplaceRasterLayerAsync(existingRasterLayer);
                }

                SetOperationProgress(2, "Preparing XYZ tile source", showProgressForm: false);

                Progress<RasterImportProgressInfo> progress = new(
                    update => SetOperationProgress(update.Percent, update.Status, showProgressForm: false));

                string sourcePath;
                string? sourceSrsOverride = null;
                RasterSourceExtent? sourceExtent = null;

                if (request.IsLiveTiles)
                {
                    XyzTileSourceDefinition sourceDefinition =
                        _xyzTileSourceService.CreateSourceDefinition(
                            AppServices.Context.ProjectFolderPath,
                            request);
                    sourcePath = sourceDefinition.DefinitionPath;
                    sourceSrsOverride = sourceDefinition.SourceExtent.SrsDefinition;
                    sourceExtent = sourceDefinition.SourceExtent;
                }
                else
                {
                    // Step 1 — Download tiles to local cache folder
                    SetOperationProgress(5, "Downloading XYZ tiles", showProgressForm: false);

                    XyzTileDownloadResult downloadResult =
                        await _xyzTilePreDownloadService.DownloadTilesAsync(
                            AppServices.Context.ProjectFolderPath,
                            request,
                            new Progress<XyzTileDownloadProgress>(update =>
                                SetOperationProgress(
                                    Math.Clamp(5 + update.Percent / 3, 5, 38),
                                    update.Status,
                                    showProgressForm: false)));

                    SetOperationProgress(40, "Packaging tiles into MBTiles", showProgressForm: false);

                    string rasterFolder = Path.Combine(
                        AppServices.Context.ProjectFolderPath, "RasterLayers");
                    Directory.CreateDirectory(rasterFolder);

                    string mbTilesFileName = $"{SanitizeFileName(request.LayerName)}_xyz.mbtiles";
                    string mbTilesPath = Path.Combine(rasterFolder, mbTilesFileName);

                    // Overwrite if already exists from a previous import of same name
                    if (File.Exists(mbTilesPath))
                        File.Delete(mbTilesPath);

                    await Task.Run(() =>
                        XyzTilePreDownloadService.AssembleDownloadedTilesIntoMbTiles(
                            downloadResult,
                            request,
                            mbTilesPath,
                            new Progress<XyzTileDownloadProgress>(update =>
                                SetOperationProgress(
                                    Math.Clamp(40 + update.Percent / 5, 40, 58),
                                    update.Status,
                                    showProgressForm: false))));

                    sourcePath = mbTilesPath;
                    sourceSrsOverride = null;
                    sourceExtent = null;
                }

                RasterLayerImportResult importResult =
                    await _rasterLayerImportService.ImportAsync(
                        new RasterLayerImportRequest(
                            AppServices.Context.Session,
                            AppServices.Context.ProjectFolderPath,
                            sourcePath,
                            request.LayerName,
                            sourceSrsOverride,
                            sourceExtent),
                        progress);

                // Clean up the temporary assembled GeoTIFF — the import service
                // has already warped and saved its own copy into the project raster folder.
                // The temp file in RasterLayers/ is no longer needed.
                if (!request.IsLiveTiles && File.Exists(sourcePath))
                {
                    try { File.Delete(sourcePath); }
                    catch { /* best-effort cleanup — never block import success */ }
                }

                SetOperationProgress(94, "Refreshing raster layer list", showProgressForm: false);

                if (_xyzTileImportOptionsForm != null &&
                    !_xyzTileImportOptionsForm.IsDisposed)
                {
                    await PersistXyzTileOptionsStateAsync(
                        _xyzTileImportOptionsForm.GetCurrentOptionsState());
                }

                _rasterImportFileManagementService.RegisterImportedRaster(
                    AppServices.Context,
                    importResult);
                AppServices.Context.MarkAsModified();
                UpdateWindowTitle();

                await RefreshLayerTreeAsync();
                SelectLayerNodeById(importResult.Layer.Id);
                if (!request.IsLiveTiles &&
                    !await TryZoomToLayerAsync(importResult.Layer, showErrorDialog: false))
                {
                    mapCanvasControlMain.ZoomExtents();
                }

                SetCanvasCommandStatus(
                    $"Imported XYZ tiles: {importResult.Layer.Name}");
                _xyzTileImportOptionsForm?.CompleteImportAndClose(
                    $"Import complete: {importResult.Layer.Name}");
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                _xyzTileImportOptionsForm?.FailImportExecution(
                    "Import failed: project coordinate system is not configured.");
                MessageBox.Show(
                    ex.Message,
                    "XYZ Tile Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                string errorMessage = XyzTileErrorMessageBuilder.AddUserGuidance(
                    GetMostUsefulExceptionMessage(ex));
                _xyzTileImportOptionsForm?.FailImportExecution(
                    $"Import failed: {errorMessage}");
                LogProjectError("XYZ tile import failed.", ex);

                MessageBox.Show(
                    $"Failed to import XYZ tiles: {errorMessage}",
                    "XYZ Tile Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private async Task EnsureDefaultGoogleSatelliteLayerAsync(
            bool isInitiallyVisible)
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            const string sourceName = "Google Satellite";
            const string layerName = "World Imagery (Google Satellite)";

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);
            CanvasLayer? existingLayer =
                await layerRepository.GetByNameAsync(layerName);
            if (existingLayer != null)
            {
                return;
            }

            try
            {
                XyzTileSourceCatalogItem? source =
                    XyzTileSourceCatalogService
                        .Load(AppServices.Context.ProjectFolderPath)
                        .FirstOrDefault(item =>
                            string.Equals(
                                item.Name,
                                sourceName,
                                StringComparison.OrdinalIgnoreCase));

                if (source == null)
                {
                    return;
                }

                XyzTileSourceImportRequest request = new(
                    layerName,
                    source.UrlTemplate,
                    -180,
                    -85.05112878,
                    180,
                    85.05112878,
                    source.MaxZoom,
                    source.ImageExtension,
                    IsLiveTiles: true);

                XyzTileSourceDefinition sourceDefinition =
                    _xyzTileSourceService.CreateSourceDefinition(
                        AppServices.Context.ProjectFolderPath,
                        request);

                RasterLayerImportResult importResult =
                    await _rasterLayerImportService.ImportAsync(
                        new RasterLayerImportRequest(
                            AppServices.Context.Session,
                            AppServices.Context.ProjectFolderPath,
                            sourceDefinition.DefinitionPath,
                            layerName,
                            sourceDefinition.SourceExtent.SrsDefinition,
                            sourceDefinition.SourceExtent,
                            IsInitiallyVisible: isInitiallyVisible));

                _rasterImportFileManagementService.RegisterImportedRaster(
                    AppServices.Context,
                    importResult);
            }
            catch (Exception ex)
            {
                LogProjectError(
                    "Failed to add the default Google Satellite online layer.",
                    ex);
            }
        }

        private (double West, double South, double East, double North)?
            TryGetCurrentViewportGeographicBounds()
        {
            if (string.IsNullOrWhiteSpace(_currentProjectRasterSrsDefinition))
            {
                return null;
            }

            try
            {
                GdalBootstrapper.ConfigureAll();
                if (!GdalConfiguration.Usable)
                {
                    return null;
                }

                RectangleD visibleBounds =
                    mapCanvasControlMain.GetVisibleWorldBounds();
                using SpatialReference sourceSrs =
                    CreateSpatialReference(_currentProjectRasterSrsDefinition);
                using SpatialReference geographicSrs =
                    CreateSpatialReference("EPSG:4326");
                using CoordinateTransformation transformation =
                    new(sourceSrs, geographicSrs);

                double minLon = double.MaxValue;
                double maxLon = double.MinValue;
                double minLat = double.MaxValue;
                double maxLat = double.MinValue;
                int validCount = 0;

                const int gridSize = 3;
                for (int row = 0; row < gridSize; row++)
                {
                    double y = visibleBounds.Y +
                        visibleBounds.Height * row / (gridSize - 1.0);
                    for (int col = 0; col < gridSize; col++)
                    {
                        double x = visibleBounds.X +
                            visibleBounds.Width * col / (gridSize - 1.0);

                        if (!TryTransformPoint(
                                transformation,
                                x,
                                y,
                                out double lon,
                                out double lat))
                        {
                            continue;
                        }

                        minLon = Math.Min(minLon, lon);
                        maxLon = Math.Max(maxLon, lon);
                        minLat = Math.Min(minLat, lat);
                        maxLat = Math.Max(maxLat, lat);
                        validCount++;
                    }
                }

                if (validCount == 0 ||
                    minLon >= maxLon ||
                    minLat >= maxLat)
                {
                    return null;
                }

                return (
                    Math.Clamp(minLon, -180.0, 180.0),
                    Math.Clamp(minLat, -85.05112878, 85.05112878),
                    Math.Clamp(maxLon, -180.0, 180.0),
                    Math.Clamp(maxLat, -85.05112878, 85.05112878));
            }
            catch (Exception ex)
            {
                LogProjectError(
                    "Failed to derive XYZ import bounds from current viewport.",
                    ex);
                return null;
            }
        }

        private static SpatialReference CreateSpatialReference(
            string definition)
        {
            SpatialReference srs = new(string.Empty);
            srs.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            if (srs.SetFromUserInput(definition) != 0)
            {
                string wkt = definition;
                if (srs.ImportFromWkt(ref wkt) != 0)
                {
                    srs.Dispose();
                    throw new InvalidOperationException(
                        $"Could not parse CRS definition '{definition}'.");
                }
            }

            srs.SetAxisMappingStrategy(
                AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            return srs;
        }

        private static bool TryTransformPoint(
            CoordinateTransformation transformation,
            double x,
            double y,
            out double transformedX,
            out double transformedY)
        {
            transformedX = 0.0;
            transformedY = 0.0;

            try
            {
                double[] point = [x, y, 0.0];
                transformation.TransformPoint(point);
                if (!double.IsFinite(point[0]) ||
                    !double.IsFinite(point[1]))
                {
                    return false;
                }

                transformedX = point[0];
                transformedY = point[1];
                return true;
            }
            catch
            {
                return false;
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

        private async Task<IReadOnlyList<CanvasLayer>> FindExistingOnlineBasemapLayersAsync()
        {
            if (!AppServices.HasContext || _layerTreeService == null)
            {
                return [];
            }

            IReadOnlyList<CanvasLayer> rasterLayers =
                await _layerTreeService.GetRasterLayersAsync();

            return rasterLayers
                .Where(IsOnlineBasemapLayer)
                .ToList();
        }

        private async Task ReplaceRasterLayerAsync(CanvasLayer existingLayer)
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            var layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);
            await layerRepository.DeleteAsync(existingLayer.Id);

            _rasterImportFileManagementService.HandleLayerDeleted(
                AppServices.Context,
                existingLayer);
            AppServices.Context.MarkAsModified();
            UpdateWindowTitle();
        }

        private static string SanitizeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            HashSet<char> invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
            return new string(input.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
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
                Title = "Open Project",
                RestoreDirectory = true
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
                UnloadProjectWorkspace();
                DisposeCurrentProjectSession();

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
                await _rasterImportFileManagementService
                    .RepairRasterLayerReferencesAsync(context);
                await _rasterImportFileManagementService
                    .CleanupUnreferencedProjectRastersAsync(context);
                await ApplySettingsAsync(showRefreshProgress: false);
                await RefreshMapCanvasAsync("Opening project canvas", 75);
                await RestoreCanvasViewportStateAsync();
            }
            catch (Exception ex)
            {
                HideOperationProgress();
                LogProjectError(
                    $"Project open failed. Path={projectFilePath}",
                    ex);
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

            try
            {
                SetOperationProgress(8, "Saving project view");
                await PersistProjectUiStateAsync();
                SetOperationProgress(18, "Saving project");
                await _rasterImportFileManagementService
                    .RepairRasterLayerReferencesAsync(AppServices.Context);
                SetOperationProgress(38, "Cleaning project workspace");
                await _rasterImportFileManagementService
                    .CleanupUnreferencedProjectRastersAsync(AppServices.Context);

                string filePath = AppServices.Context.ProjectFilePath;

                // Repositories write to the DB immediately on every edit,
                // so there are no pending EF Core changes to flush here.
                // Save = WAL checkpoint (merge WAL ? .lpp) + rotate backups.

                // 1. WAL checkpoint — merge WAL journal into the .lpp file
                SetOperationProgress(64, "Writing project database");
                await ProjectWalCheckpoint.ExecuteAsync(filePath);

                // 2. Rotate backups — .bak = state just before this save
                SetOperationProgress(80, "Updating project backup");
                _backupService.CreateBackup(filePath);

                _rasterImportFileManagementService.CommitPendingDeletes(
                    AppServices.Context);

                // 3. Mark clean
                AppServices.Context.MarkAsSaved();
                SetOperationProgress(100, "Project saved");

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
            finally
            {
                HideOperationProgress();
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
            if (mnuPan.Checked)
            {
                mnuSelectTool.Checked = false;
                mnuDrawLine.Checked = false;
                mnuDrawPolyline.Checked = false;
                mnuDrawPolygon.Checked = false;
                mnuDrawRectangle.Checked = false;
                mnuDrawCircle.Checked = false;
            }
            mapCanvasControlMain.SetPanToolActive(mnuPan.Checked);
        }

        private void mnuZoomIn_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            ActivateCanvasTool(MapCanvasTool.Select);
            mapCanvasControlMain.SetPanToolActive(false);
            mapCanvasControlMain.ZoomIn();
        }

        private void mnuZoomOut_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            ActivateCanvasTool(MapCanvasTool.Select);
            mapCanvasControlMain.SetPanToolActive(false);
            mapCanvasControlMain.ZoomOut();
        }

        private void mnuZoomExtent_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            ActivateCanvasTool(MapCanvasTool.Select);
            mapCanvasControlMain.SetPanToolActive(false);
            mapCanvasControlMain.ZoomExtents();
        }

        private void mnuZoomWindow_Click(object sender, EventArgs e)
        {
            mnuPan.Checked = false;
            ActivateCanvasTool(MapCanvasTool.Select);
            mapCanvasControlMain.BeginZoomWindow();
        }

        private void mnuSelectTool_Click(object sender, EventArgs e)
        {
            ActivateCanvasTool(MapCanvasTool.Select);
        }

        private async void mnuDrawLine_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Line);
        }

        private async void mnuDrawPoint_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Point);
        }

        private async void mnuDrawPolyline_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Polyline);
        }

        private async void mnuDrawPolygon_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Polygon);
        }

        private async void mnuDrawRectangle_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Rectangle);
        }

        private async void mnuDrawCircle_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Circle);
        }

        private void mnuCanvasDebugOverlay_Click(object sender, EventArgs e)
        {
            mapCanvasControlMain.ShowDebugOverlay = mnuCanvasDebugOverlay.Checked;
            SetCanvasCommandStatus(mnuCanvasDebugOverlay.Checked
                ? "Canvas debug overlay enabled"
                : "Canvas debug overlay disabled");
        }

        private void mnuOSnapToggle_Click(object sender, EventArgs e)
        {
            SetObjectSnapMode(mnuOSnapToggle.Checked);
        }

        private void mnuOrthoToggle_Click(object sender, EventArgs e)
        {
            SetOrthoMode(mnuOrthoToggle.Checked);
        }

        private void SetObjectSnapMode(bool enabled)
        {
            mnuOSnapToggle.Checked = enabled;
            mapCanvasControlMain.ApplySnapEnabled(enabled);
            SetCanvasCommandStatus(enabled
                ? "Object snap enabled"
                : "Object snap disabled");
        }

        private void SetOrthoMode(bool enabled)
        {
            mnuOrthoToggle.Checked = enabled;
            mapCanvasControlMain.OrthoModeEnabled = enabled;
            SetCanvasCommandStatus(enabled
                ? "Ortho mode enabled"
                : "Ortho mode disabled");
        }

        private void SetGridVisibility(bool visible)
        {
            mapCanvasControlMain.ApplyGridVisible(visible);
            SetCanvasCommandStatus(visible
                ? "Grid display enabled"
                : "Grid display disabled");
        }

        private void SetNorthMarkerVisibility(bool visible)
        {
            mapCanvasControlMain.ApplyNorthMarkerVisible(visible);
            SetCanvasCommandStatus(visible
                ? "North marker enabled"
                : "North marker disabled");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            if (keyCode == Keys.F3)
            {
                SetObjectSnapMode(!mnuOSnapToggle.Checked);
                return true;
            }

            if (keyCode == Keys.F8)
            {
                SetOrthoMode(!mnuOrthoToggle.Checked);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ActivateCanvasTool(MapCanvasTool tool)
        {
            if (tool != MapCanvasTool.Select)
            {
                _ = ActivateCanvasDrawingToolAsync(tool);
                return;
            }

            ApplyCanvasToolSelection(tool);
            _currentCanvasTool = tool;
            mapCanvasControlMain.SetActiveTool(tool, GetSelectedCurrentDrawingLayer());
            UpdateActiveTool("Select");
        }

        private async Task ActivateCanvasDrawingToolAsync(MapCanvasTool tool)
        {
            try
            {
                CanvasLayer? layer = await ResolveCurrentDrawingLayerForToolAsync(tool);
                ApplyCanvasToolSelection(tool);
                _currentCanvasTool = tool;
                mapCanvasControlMain.SetActiveTool(tool, layer);
                UpdateActiveTool($"Draw {tool}");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Layer Type Mismatch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ActivateCanvasTool(MapCanvasTool.Select);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to activate the drawing tool:\n{ex.Message}",
                    "Drawing Tool",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                ActivateCanvasTool(MapCanvasTool.Select);
            }
        }

        private void ApplyCanvasToolSelection(MapCanvasTool tool)
        {
            mnuPan.Checked = false;
            mnuSelectTool.Checked = tool == MapCanvasTool.Select;
            mnuDrawPoint.Checked = tool == MapCanvasTool.Point;
            mnuDrawLine.Checked = tool == MapCanvasTool.Line;
            mnuDrawPolyline.Checked = tool == MapCanvasTool.Polyline;
            mnuDrawPolygon.Checked = tool == MapCanvasTool.Polygon;
            mnuDrawRectangle.Checked = tool == MapCanvasTool.Rectangle;
            mnuDrawCircle.Checked = tool == MapCanvasTool.Circle;
            mnuDrawArc.Checked = tool == MapCanvasTool.Arc;
        }

        private async Task<CanvasLayer?> ResolveCurrentDrawingLayerForToolAsync(
            MapCanvasTool tool)
        {
            CanvasLayer? selectedLayer = GetSelectedCurrentDrawingLayer();
            if (selectedLayer != null && IsEditableDrawingLayer(selectedLayer))
            {
                if (!IsDrawingLayerCompatibleWithTool(selectedLayer, tool))
                {
                    throw new InvalidOperationException(
                        $"The current layer '{selectedLayer.Name}' is a {GetLayerTypeDisplayName(selectedLayer.LayerType)} layer. " +
                        $"Select or create a {GetLayerTypeDisplayName(ResolveDrawingLayerTypeForTool(tool))} layer before using the {tool} tool.");
                }

                _currentDrawingLayer = selectedLayer;
                return selectedLayer;
            }

            if (GetDrawingMarkupLayersFromTree().Any(IsEditableDrawingLayer))
            {
                throw new InvalidOperationException(
                    $"Select a {GetLayerTypeDisplayName(ResolveDrawingLayerTypeForTool(tool))} drawing layer before using the {tool} tool.");
            }

            return await CreateDrawingLayerForToolAsync(tool);
        }

        private async Task<CanvasLayer> CreateDrawingLayerForToolAsync(MapCanvasTool tool)
        {
            if (!AppServices.HasContext)
            {
                throw new InvalidOperationException("Open a project before drawing.");
            }

            ICanvasLayerRepository layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(AppServices.Context.Session);
            IReadOnlyList<CanvasLayer> layers = await layerRepository.GetAllOrderedAsync();
            string layerType = ResolveDrawingLayerTypeForTool(tool);
            string layerName = BuildUniqueLayerName(
                GetDefaultDrawingLayerName(tool),
                layers);

            string drawingColor = GetCanvasContrastDrawingColorHex();
            CanvasLayer newLayer = new()
            {
                Name = layerName,
                LayerType = layerType,
                IsVisible = true,
                IsLocked = false,
                IsSelectable = true,
                IsPrintable = true,
                DisplayOrder = layers.Count == 0
                    ? 0
                    : layers.Max(layer => layer.DisplayOrder) + 1,
                BorderColor = drawingColor,
                LineWeight = 1.3,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                FillColor = null,
                FillTransparency = 100,
                FillStyle = "None",
                LabelColor = "#000000",
                PointSymbol = "Dot",
                PointSize = 5.0,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                Description = $"Drawing/markup layer: {layerName}"
            };

            CanvasLayer savedLayer = await layerRepository.AddAsync(newLayer);
            await RefreshLayerTreeAsync();
            SelectCurrentDrawingLayerById(savedLayer.Id);
            MarkProjectModifiedIfOpen();
            return GetSelectedCurrentDrawingLayer() ?? savedLayer;
        }

        private async Task<bool> EnsureAutomaticDrawingLayerContrastAsync(
            IReadOnlyList<CanvasLayerTreeGroup> layerGroups)
        {
            if (!AppServices.HasContext)
            {
                return false;
            }

            string drawingColor = GetCanvasContrastDrawingColorHex();
            ICanvasLayerRepository layerRepository =
                _projectScopedFactory.CreateCanvasLayerRepository(AppServices.Context.Session);
            bool changed = false;

            foreach (CanvasLayer layer in layerGroups
                         .Where(group => string.Equals(group.Key, DrawingMarkupGroupKey, StringComparison.OrdinalIgnoreCase))
                         .SelectMany(group => group.Layers))
            {
                if (!IsAutomaticDrawingLayer(layer) ||
                    string.Equals(layer.BorderColor, drawingColor, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                layer.BorderColor = drawingColor;
                layer.LastModifiedDate = DateTime.Now;
                await layerRepository.UpdateAsync(layer);
                changed = true;
            }

            return changed;
        }

        private static bool IsAutomaticDrawingLayer(CanvasLayer layer)
        {
            return string.Equals(
                       layer.Description,
                       $"Default layer: {layer.Name}",
                       StringComparison.OrdinalIgnoreCase) ||
                   (layer.Description?.StartsWith(
                       "Drawing/markup layer:",
                       StringComparison.OrdinalIgnoreCase) == true);
        }

        private string GetCanvasContrastDrawingColorHex()
        {
            return IsDarkCanvasColor(mapCanvasControlMain.BackColor)
                ? "#FFFFFF"
                : "#000000";
        }

        private static string GetRandomDrawingMarkupColorHex()
        {
            string[] palette =
            {
                "#D32F2F",    // Medium red
                "#1976D2",    // Medium blue
                "#388E3C",    // Medium green
                "#F57C00",    // Medium orange
                "#7B1FA2",    // Medium purple
                "#0097A7",    // Medium cyan
                "#C2185B",    // Medium pink
                "#5D4037",    // Dark brown
                "#F0F0F0",    // Very light gray (will adjust to black on light canvas)
                "#0A0A0A"     // Very dark (will adjust to white on dark canvas)
            };

            return palette[Random.Shared.Next(palette.Length)];
        }

        private static bool IsDarkCanvasColor(Color color)
        {
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return luminance < 0.45;
        }

        private static string GetDefaultDrawingLayerName(MapCanvasTool tool)
        {
            return tool switch
            {
                MapCanvasTool.Point => "Point Markup",
                MapCanvasTool.Line => "Line Markup",
                MapCanvasTool.Polyline => "Polyline Markup",
                MapCanvasTool.Arc => "Arc Markup",
                MapCanvasTool.Rectangle => "Rectangle Markup",
                MapCanvasTool.Polygon => "Polygon Markup",
                MapCanvasTool.Circle => "Circle Markup",
                _ => "Markup"
            };
        }

        private static string ResolveDrawingLayerTypeForTool(MapCanvasTool tool)
        {
            return tool switch
            {
                MapCanvasTool.Point => CanvasLayerTreeService.PointLayerType,
                MapCanvasTool.Line or MapCanvasTool.Polyline or MapCanvasTool.Arc => CanvasLayerTreeService.PolylineLayerType,
                MapCanvasTool.Rectangle or MapCanvasTool.Polygon or MapCanvasTool.Circle => CanvasLayerTreeService.PolygonLayerType,
                _ => CanvasLayerTreeService.PolylineLayerType
            };
        }

        private void UpdateDrawingToolAvailability()
        {
            bool enabled = cboCurrentDrawingLayer.Enabled;
            CanvasLayer? selectedLayer = GetSelectedCurrentDrawingLayer();
            bool allowAllForAutoLayerCreation = selectedLayer == null;

            mnuDrawPoint.Enabled = enabled &&
                                   (allowAllForAutoLayerCreation ||
                                    IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Point));
            mnuDrawLine.Enabled = enabled &&
                                  (allowAllForAutoLayerCreation ||
                                   IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Line));
            mnuDrawPolyline.Enabled = enabled &&
                                      (allowAllForAutoLayerCreation ||
                                       IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Polyline));
            mnuDrawArc.Enabled = enabled &&
                                (allowAllForAutoLayerCreation ||
                                 IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Arc));
            mnuDrawPolygon.Enabled = enabled &&
                                     (allowAllForAutoLayerCreation ||
                                      IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Polygon));
            mnuDrawRectangle.Enabled = enabled &&
                                       (allowAllForAutoLayerCreation ||
                                        IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Rectangle));
            mnuDrawCircle.Enabled = enabled &&
                                    (allowAllForAutoLayerCreation ||
                                     IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Circle));

            if (_currentCanvasTool != MapCanvasTool.Select &&
                selectedLayer != null &&
                !IsDrawingLayerCompatibleWithTool(selectedLayer, _currentCanvasTool))
            {
                ActivateCanvasTool(MapCanvasTool.Select);
            }
        }

        private static bool IsDrawingLayerCompatibleWithTool(CanvasLayer? layer, MapCanvasTool tool)
        {
            if (layer == null)
                return false;

            string expectedType = ResolveDrawingLayerTypeForTool(tool);
            if (string.Equals(layer.LayerType, expectedType, StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(expectedType, CanvasLayerTreeService.PolylineLayerType, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(layer.LayerType, CanvasLayerTreeService.LineLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetLayerTypeDisplayName(string? layerType)
        {
            return layerType?.Trim().ToLowerInvariant() switch
            {
                "point" => "Point",
                "line" => "Polyline",
                "polyline" => "Polyline",
                "polygon" => "Polygon",
                "annotation" => "Annotation",
                _ => string.IsNullOrWhiteSpace(layerType) ? "matching" : layerType.Trim()
            };
        }

        private static string BuildUniqueLayerName(
            string preferredName,
            IReadOnlyList<CanvasLayer> existingLayers)
        {
            if (!existingLayers.Any(layer => string.Equals(layer.Name, preferredName, StringComparison.OrdinalIgnoreCase)))
            {
                return preferredName;
            }

            int suffix = 2;
            string candidate;
            do
            {
                candidate = $"{preferredName} {suffix++}";
            }
            while (existingLayers.Any(layer => string.Equals(layer.Name, candidate, StringComparison.OrdinalIgnoreCase)));

            return candidate;
        }

        private static bool IsEditableDrawingLayer(CanvasLayer? layer)
        {
            return layer != null &&
                   !layer.IsLocked &&
                   !IsRasterLayer(layer);
        }

        private static int GetPreferredDrawingLayerRank(CanvasLayer layer)
        {
            return 0;
        }

        private CanvasLayer? GetSelectedCurrentDrawingLayer()
        {
            return cboCurrentDrawingLayer.SelectedItem is DrawingLayerComboItem item
                ? item.Layer
                : _currentDrawingLayer;
        }

        private void SelectCurrentDrawingLayerById(int layerId)
        {
            for (int index = 0; index < cboCurrentDrawingLayer.Items.Count; index++)
            {
                if (cboCurrentDrawingLayer.Items[index] is DrawingLayerComboItem item &&
                    item.Layer.Id == layerId)
                {
                    bool previousSuppression = _suppressCurrentDrawingLayerSelectionChanged;
                    _suppressCurrentDrawingLayerSelectionChanged = true;
                    try
                    {
                        cboCurrentDrawingLayer.SelectedIndex = index;
                        _currentDrawingLayer = item.Layer;
                    }
                    finally
                    {
                        _suppressCurrentDrawingLayerSelectionChanged = previousSuppression;
                    }

                    UpdateDrawingToolAvailability();
                    return;
                }
            }
        }

        private async void cboCurrentDrawingLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressCurrentDrawingLayerSelectionChanged)
            {
                return;
            }

            _currentDrawingLayer = GetSelectedCurrentDrawingLayer();
            UpdateDrawingToolAvailability();
            if (_currentCanvasTool == MapCanvasTool.Select)
            {
                mapCanvasControlMain.SetActiveTool(_currentCanvasTool, _currentDrawingLayer);
                return;
            }

            if (!IsDrawingLayerCompatibleWithTool(_currentDrawingLayer, _currentCanvasTool))
            {
                string expectedType = ResolveDrawingLayerTypeForTool(_currentCanvasTool);
                MessageBox.Show(
                    $"The selected layer is not valid for the {_currentCanvasTool} tool.\n\nSelect a {GetLayerTypeDisplayName(expectedType)} layer, or create one, before drawing.",
                    "Layer Type Mismatch",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ActivateCanvasTool(MapCanvasTool.Select);
                return;
            }

            await ActivateCanvasDrawingToolAsync(_currentCanvasTool);
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
            _mnuToggleLayerVisibility.CheckOnClick = false;
            _mnuToggleLayerLock.CheckOnClick = false;
            _mnuAddRasterMap.Click += async (_, _) => await ImportRasterFileAsync(
                "Add Raster Map",
                GetGeneralRasterImportFilter());
            _mnuAddXyzTiles.Click += async (_, _) =>
                await ShowXyzTileImportOptionsFormAsync();
            _mnuZoomToLayer.Click += async (_, _) => await ZoomToLayerAsync(_contextLayerNode);
            _mnuRenameLayer.Click += (_, _) => BeginLayerRename(_contextLayerNode);
            _mnuDeleteLayer.Click += async (_, _) => await DeleteLayerAsync(_contextLayerNode);
            _mnuAddDrawingLayer.Click += async (_, _) => await AddDrawingMarkupLayerAsync(_contextLayerGroupNode);
            _mnuSetActiveLayer.Click += (_, _) =>
            {
                CanvasLayer? layer = GetLayerFromNode(_contextLayerNode);
                if (layer != null)
                    SetCurrentDrawingLayer(layer);
            };
            _mnuMoveLayerUp.Click += async (_, _) => await MoveRasterLayerInDisplayOrderAsync(_contextLayerNode, -1);
            _mnuMoveLayerDown.Click += async (_, _) => await MoveRasterLayerInDisplayOrderAsync(_contextLayerNode, 1);
            _mnuToggleLayerVisibility.Click += async (_, _) => await ToggleLayerNodeVisibilityAsync(_contextLayerNode);
            _mnuToggleLayerLock.Click += async (_, _) => await ToggleLayerLockAsync(_contextLayerNode);
            _mnuLayerProperties.Click += async (_, _) => await OpenLayerPropertyManagerAsync(_contextLayerNode);
            treeViewLayers.ContextMenuStrip = _layerContextMenu;
        }

        private void ConfigureLayerPropertiesPanel()
        {
            // tabProperties (TabControl) replaced the old grpProperties GroupBox.
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
            bool isCheckableGroup = layerState?.IsCheckableGroup == true;

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
                bool isLockedLayer = layer.IsLocked;

                if (isLockedLayer)
                {
                    textColor = selected
                        ? Color.FromArgb(224, 224, 224)
                        : BlendColor(treeViewLayers.ForeColor, treeViewLayers.BackColor, 0.48f);
                }

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

                Rectangle colorRect = GetLayerNodeColorRect(e.Node, layer);

                if (layerState?.IsOnlineBasemap == true)
                {
                    DrawOnlineBasemapIcon(g, colorRect);
                }
                else if (IsRasterLayer(layer))
                {
                    // Raster layers have no single border colour -- draw a small
                    // checkerboard icon that universally signals imagery / raster data.
                    DrawRasterLayerIcon(g, colorRect);
                }
                else if (CanvasLayerTreeService.IsAnnotationLayer(layer))
                {
                    DrawAnnotationLayerSwatch(g, colorRect, layer, treeViewLayers.BackColor);
                }
                else if (CanvasLayerTreeService.IsPointLayer(layer))
                {
                    DrawPointLayerSwatch(g, colorRect, layer, treeViewLayers.BackColor);
                }
                else if (CanvasLayerTreeService.IsLineLayer(layer))
                {
                    DrawLineLayerSwatch(g, colorRect, layer, treeViewLayers.BackColor);
                }
                else
                {
                    DrawVectorLayerSwatch(
                        g,
                        colorRect,
                        layer,
                        treeViewLayers.BackColor);
                }

                x = colorRect.Right + LayerNodeColorBoxGap;
            }
            else if (isCheckableGroup)
            {
                Rectangle chkRect = GetLayerNodeCheckBoxRect(e.Node);
                CheckBoxRenderer.DrawCheckBox(
                    g,
                    chkRect.Location,
                    GetGroupCheckBoxState(e.Node));

                x = chkRect.Right + LayerNodeCheckBoxGap;

                if (CanvasLayerTreeService.IsRoadsGroupKey(layerState?.GroupKey))
                {
                    Rectangle colorRect = GetLayerNodeColorRect(e.Node);
                    DrawRoadsGroupSwatch(g, colorRect, e.Node, treeViewLayers.BackColor);
                    x = colorRect.Right + LayerNodeColorBoxGap;
                }
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
                    IsLayerNode = false,
                    IsCheckableGroup = IsRePlotDataGroupKey(key) ||
                                       string.Equals(key, CadastralMapGroupKey, StringComparison.OrdinalIgnoreCase),
                    IsGroupCheckedWhenEmpty = string.Equals(key, CadastralMapGroupKey, StringComparison.OrdinalIgnoreCase),
                    GroupKey = key
                }
            };
        }

        private static TreeNode CreateRePlotRootNode()
        {
            return new TreeNode("RePlot")
            {
                Name = $"{LayerGroupNodeNamePrefix}{RePlotRootNodeKey}",
                Tag = new LayerTreeNodeState
                {
                    IsLayerNode = false,
                    GroupKey = RePlotRootNodeKey
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
                    Layer = layer,
                    IsOnlineBasemap = IsOnlineBasemapLayer(layer)
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
                _contextLayerGroupNode = IsLayerGroupNode(e.Node)
                    ? e.Node
                    : null;
                return;
            }

            if (e.Node.Tag is not LayerTreeNodeState state)
                return;

            if (!state.IsLayerNode)
            {
                if (state.IsCheckableGroup &&
                    GetLayerNodeCheckBoxRect(e.Node).Contains(e.Location))
                {
                    await ToggleLayerGroupVisibilityAsync(e.Node);
                }

                return;
            }

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
            if (selectedNode.Tag is LayerTreeNodeState state &&
                !state.IsLayerNode &&
                state.IsCheckableGroup)
            {
                await ToggleLayerGroupVisibilityAsync(selectedNode);
                return;
            }

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

            SetCanvasCommandStatus("Layer selected");
        }

        private void LayerContextMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            Point clientPoint = treeViewLayers.PointToClient(Cursor.Position);
            TreeNode? node = treeViewLayers.GetNodeAt(clientPoint);
            TreeNode? targetNode = node ?? treeViewLayers.SelectedNode;

            if (IsRasterLayerGroupNode(targetNode))
            {
                _contextLayerNode = null;
                _contextLayerGroupNode = targetNode;
                treeViewLayers.SelectedNode = targetNode;
                ConfigureRasterGroupContextMenuItems();
                return;
            }

            if (IsDrawingMarkupGroupNode(targetNode))
            {
                _contextLayerNode = null;
                _contextLayerGroupNode = targetNode;
                treeViewLayers.SelectedNode = targetNode;
                ConfigureDrawingMarkupGroupContextMenuItems();
                return;
            }

            _contextLayerNode = IsLayerNode(targetNode) ? targetNode : null;
            _contextLayerGroupNode = null;

            if (!IsLayerNode(_contextLayerNode))
            {
                e.Cancel = true;
                return;
            }

            treeViewLayers.SelectedNode = _contextLayerNode;
            ConfigureLayerContextMenuItems();

            CanvasLayer layer = GetLayerFromNode(_contextLayerNode)!;

            if (CanvasLayerTreeService.IsDrawingMarkupLayer(layer))
            {
                bool isAlreadyActive = _currentDrawingLayer?.Id == layer.Id;
                _mnuSetActiveLayer.Text = isAlreadyActive ? "Set Active Layer ✓" : "Set Active Layer";
                _mnuSetActiveLayer.Enabled = !isAlreadyActive;
                _layerContextMenu.Items.Insert(0, _mnuSetActiveLayer);
                _layerContextMenu.Items.Insert(1, new ToolStripSeparator());
            }

            _mnuToggleLayerVisibility.Text = "Hidden";
            _mnuToggleLayerLock.Text = "Locked";
            _mnuToggleLayerVisibility.Checked = !layer.IsVisible;
            _mnuToggleLayerLock.Checked = layer.IsLocked;

            bool isProtectedDefaultLayer =
                CanvasLayerTreeService.IsProtectedDefaultLayer(layer);
            _mnuZoomToLayer.Enabled = !IsOnlineBasemapLayer(_contextLayerNode);
            _mnuRenameLayer.Enabled = !isProtectedDefaultLayer;
            _mnuDeleteLayer.Enabled = !isProtectedDefaultLayer;
            bool canReorderRaster =
                IsRasterLayer(layer) &&
                !IsOnlineBasemapLayer(_contextLayerNode);
            _mnuMoveLayerUp.Enabled =
                canReorderRaster &&
                CanMoveRasterLayerInDisplayOrder(_contextLayerNode, -1);
            _mnuMoveLayerDown.Enabled =
                canReorderRaster &&
                CanMoveRasterLayerInDisplayOrder(_contextLayerNode, 1);
            _mnuToggleLayerVisibility.Enabled = true;
            _mnuToggleLayerLock.Enabled = true;
            _mnuLayerProperties.Enabled = true;
        }

        private static Color BlendColor(Color source, Color target, float targetWeight)
        {
            float clampedWeight = Math.Clamp(targetWeight, 0f, 1f);
            float sourceWeight = 1f - clampedWeight;

            int red = (int)Math.Round((source.R * sourceWeight) + (target.R * clampedWeight));
            int green = (int)Math.Round((source.G * sourceWeight) + (target.G * clampedWeight));
            int blue = (int)Math.Round((source.B * sourceWeight) + (target.B * clampedWeight));

            return Color.FromArgb(red, green, blue);
        }

        /// <summary>
        /// Draws a small 2x2 checkerboard icon inside <paramref name=rect/> to
        /// indicate a raster / imagery layer.  The pattern mirrors the transparency
        /// icon used in image editors and is instantly recognisable as raster data.
        /// </summary>
        private static void DrawRasterLayerIcon(Graphics g, Rectangle rect)
        {
            int halfW = Math.Max(1, rect.Width / 2);
            int halfH = Math.Max(1, rect.Height / 2);

            // Cell colours: two tones of slate-gray that read well on any background.
            Color dark = Color.FromArgb(148, 148, 155);
            Color light = Color.FromArgb(210, 210, 215);

            using SolidBrush darkBrush = new(dark);
            using SolidBrush lightBrush = new(light);

            // Top-left and bottom-right = dark; top-right and bottom-left = light.
            g.FillRectangle(darkBrush, rect.X, rect.Y, halfW, halfH);
            g.FillRectangle(lightBrush, rect.X + halfW, rect.Y, halfW, halfH);
            g.FillRectangle(lightBrush, rect.X, rect.Y + halfH, halfW, halfH);
            g.FillRectangle(darkBrush, rect.X + halfW, rect.Y + halfH, halfW, halfH);

            // Thin border so the icon has the same framed look as a vector swatch.
            using Pen borderPen = new(Color.FromArgb(110, 110, 120));
            g.DrawRectangle(borderPen, rect);
        }

        private static void DrawOnlineBasemapIcon(Graphics g, Rectangle rect)
        {
            using SolidBrush fillBrush = new(Color.FromArgb(42, 132, 218));
            using Pen outlinePen = new(Color.FromArgb(18, 82, 145));
            using Pen linePen = new(Color.White, 1.4f);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle globeRect = Rectangle.Inflate(rect, -1, -1);
            g.FillEllipse(fillBrush, globeRect);
            g.DrawEllipse(outlinePen, globeRect);

            int centerX = globeRect.Left + globeRect.Width / 2;
            int centerY = globeRect.Top + globeRect.Height / 2;
            g.DrawLine(linePen, centerX, globeRect.Top + 3, centerX, globeRect.Bottom - 3);
            g.DrawArc(linePen, globeRect.Left + 3, globeRect.Top + 2, globeRect.Width - 6, globeRect.Height - 4, 90, 180);
            g.DrawArc(linePen, globeRect.Left + 3, globeRect.Top + 2, globeRect.Width - 6, globeRect.Height - 4, 270, 180);
            g.DrawLine(linePen, globeRect.Left + 3, centerY, globeRect.Right - 3, centerY);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        }

        private void DrawVectorLayerSwatch(
            Graphics g,
            Rectangle rect,
            CanvasLayer layer,
            Color backgroundColor)
        {
            Color outlineColor = ParseColorOrDefault(layer.BorderColor, Color.Black);
            bool hasFill =
                !string.IsNullOrWhiteSpace(layer.FillColor) &&
                !string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase) &&
                layer.FillTransparency < 100;

            Color rawFillColor = hasFill
                ? ParseColorOrDefault(layer.FillColor, Color.White)
                : backgroundColor;

            Color fillColor = hasFill
                ? BlendColor(rawFillColor, backgroundColor, layer.FillTransparency / 100f)
                : backgroundColor;

            Rectangle symbolRect = new(
                rect.X,
                rect.Y,
                Math.Max(1, rect.Width - 1),
                Math.Max(1, rect.Height - 1));

            if (string.Equals(layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase))
            {
                _hatchPatternService.DrawPreview(
                    g,
                    symbolRect,
                    layer.HatchPattern,
                    rawFillColor,
                    backgroundColor,
                    layer.FillTransparency,
                    layer.HatchScale,
                    backgroundColor);
            }
            else
            {
                using SolidBrush fillBrush = new(fillColor);
                g.FillRectangle(fillBrush, symbolRect);
            }

            float outlineWidth = (float)Math.Clamp(
                layer.LineWeight,
                0.0,
                Math.Max(1.0, Math.Min(symbolRect.Width, symbolRect.Height) / 2.0));

            if (outlineWidth <= 0)
                return;

            using Pen outlinePen = new(outlineColor, outlineWidth)
            {
                Alignment = System.Drawing.Drawing2D.PenAlignment.Inset
            };
            ApplyPenLineStyle(outlinePen, layer.LineStyle, (float)layer.LineTypeScale);
            g.DrawRectangle(outlinePen, symbolRect);
        }

        private static void DrawLineLayerSwatch(
            Graphics g,
            Rectangle rect,
            CanvasLayer layer,
            Color backgroundColor)
        {
            Color lineColor = ParseColorOrDefault(layer.BorderColor, Color.Black);
            DrawCenteredLineSymbol(
                g,
                rect,
                lineColor,
                (float)layer.LineWeight,
                layer.LineStyle,
                (float)layer.LineTypeScale);
        }

        private static void DrawPointLayerSwatch(
            Graphics g,
            Rectangle rect,
            CanvasLayer layer,
            Color backgroundColor)
        {
            using SolidBrush backgroundBrush = new(backgroundColor);
            g.FillRectangle(backgroundBrush, rect);

            Color markerColor = ParseColorOrDefault(layer.BorderColor, Color.Black);
            RectangleF markerRect = RectangleF.Inflate(rect, -3, -3);
            PointMarkerRenderer.Draw(
                g,
                markerRect,
                layer.PointSymbol,
                markerColor,
                Math.Max(1.0f, (float)layer.LineWeight));
        }

        private static void DrawAnnotationLayerSwatch(
            Graphics g,
            Rectangle rect,
            CanvasLayer layer,
            Color backgroundColor)
        {
            using SolidBrush backgroundBrush = new(backgroundColor);
            g.FillRectangle(backgroundBrush, rect);

            Color textColor = ParseColorOrDefault(
                layer.LabelColor,
                ParseColorOrDefault(layer.BorderColor, Color.Black));
            using Font font = new(
                "Segoe UI",
                Math.Max(7.0f, rect.Height - 7.0f),
                FontStyle.Bold,
                GraphicsUnit.Pixel);
            TextRenderer.DrawText(
                g,
                "T",
                font,
                rect,
                textColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding);
        }

        private void DrawRoadsGroupSwatch(
            Graphics g,
            Rectangle rect,
            TreeNode roadsNode,
            Color backgroundColor)
        {
            CanvasLayer? roadParcel = EnumerateLayerNodes(roadsNode)
                .Select(GetLayerFromNode)
                .FirstOrDefault(layer =>
                    layer != null &&
                    !CanvasLayerTreeService.IsLineLayer(layer));

            CanvasLayer? centerline = EnumerateLayerNodes(roadsNode)
                .Select(GetLayerFromNode)
                .FirstOrDefault(layer =>
                    layer != null &&
                    CanvasLayerTreeService.IsLineLayer(layer));

            if (roadParcel != null)
            {
                DrawVectorLayerSwatch(g, rect, roadParcel, backgroundColor);
            }
            else
            {
                using SolidBrush fillBrush = new(backgroundColor);
                g.FillRectangle(fillBrush, rect);
            }

            if (centerline != null)
            {
                Color lineColor = ParseColorOrDefault(centerline.BorderColor, Color.Black);
                Rectangle centerlineRect = new(
                    rect.X + 2,
                    rect.Y,
                    Math.Max(1, rect.Width - 5),
                    Math.Max(1, rect.Height - 1));

                DrawCenteredLineSymbol(
                    g,
                    centerlineRect,
                    lineColor,
                    (float)centerline.LineWeight,
                    centerline.LineStyle,
                    (float)centerline.LineTypeScale);
            }
        }

        private static void DrawCenteredLineSymbol(
            Graphics g,
            Rectangle rect,
            Color lineColor,
            float lineWeight,
            string? lineStyle,
            float lineTypeScale)
        {
            float y = rect.Top + (rect.Height - 1) / 2f;
            if (lineWeight <= 0)
                return;

            float width = Math.Clamp(lineWeight, 1.0f, Math.Max(1.0f, rect.Height / 2.0f));
            using Pen pen = new(lineColor, width)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Flat,
                EndCap = System.Drawing.Drawing2D.LineCap.Flat
            };

            ApplyPenLineStyle(pen, lineStyle, lineTypeScale);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawLine(pen, rect.Left, y, rect.Right, y);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        }

        private static void DrawLineStylePreview(
            Graphics g,
            Rectangle rect,
            Color lineColor,
            float lineWeight,
            string? lineStyle)
        {
            int y = rect.Top + rect.Height / 2;
            float width = Math.Clamp(lineWeight, 1.0f, Math.Max(1.0f, rect.Height / 2.0f));
            using Pen pen = new(lineColor, width)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Flat,
                EndCap = System.Drawing.Drawing2D.LineCap.Flat
            };

            ApplyPenLineStyle(pen, lineStyle, 1.0f);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.DrawLine(pen, rect.Left, y, rect.Right, y);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
        }

        private static void ApplyPenLineStyle(Pen pen, string? lineStyle, float lineTypeScale)
        {
            float scale = Math.Clamp(lineTypeScale, 0.1f, 100f);
            switch (NormalizeLineStyleKey(lineStyle))
            {
                case "DASHED":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [4f * scale, 2f * scale];
                    break;
                case "DOTTED":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [1f * scale, 2f * scale];
                    break;
                case "DASHDOT":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case "CENTERLINE":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [8f * scale, 3f * scale, 2f * scale, 3f * scale];
                    break;
                default:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    break;
            }
        }

        private static string NormalizeLineStyleKey(string? lineStyle)
        {
            return (lineStyle ?? string.Empty)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant() switch
            {
                "DASH" => "DASHED",
                "DOT" => "DOTTED",
                _ => (lineStyle ?? string.Empty)
                    .Replace("-", string.Empty, StringComparison.Ordinal)
                    .Replace(" ", string.Empty, StringComparison.Ordinal)
                    .Trim()
                    .ToUpperInvariant()
            };
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

        private void ConfigureDrawingMarkupGroupContextMenuItems()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.Add(_mnuAddDrawingLayer);
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
                _mnuMoveLayerUp,
                _mnuMoveLayerDown,
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
            if (IsProtectedRePlotDefaultLayerNameNode(node, layer))
            {
                MessageBox.Show(
                    "Layer Name cannot be changed",
                    "Rename Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

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

        private static bool IsProtectedRePlotDefaultLayerNameNode(
            TreeNode? node,
            CanvasLayer? layer)
        {
            if (node == null ||
                layer == null ||
                !CanvasLayerTreeService.IsProtectedDefaultLayer(layer))
            {
                return false;
            }

            for (TreeNode? current = node; current != null; current = current.Parent)
            {
                if (string.Equals(
                        current.Name,
                        $"{LayerGroupNodeNamePrefix}{RePlotRootNodeKey}",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
                    mapCanvasControlMain.UpdateVectorLayer(updatedLayer);
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

        private async Task ToggleLayerGroupVisibilityAsync(TreeNode? groupNode)
        {
            if (groupNode == null ||
                groupNode.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsCheckableGroup)
            {
                return;
            }

            List<TreeNode> layerNodes = EnumerateLayerNodes(groupNode).ToList();
            if (layerNodes.Count == 0)
            {
                nodeState.IsGroupCheckedWhenEmpty = !nodeState.IsGroupCheckedWhenEmpty;
                treeViewLayers.Invalidate();
                SetCanvasCommandStatus(nodeState.IsGroupCheckedWhenEmpty
                    ? $"Layer group checked: {groupNode.Text}"
                    : $"Layer group unchecked: {groupNode.Text}");
                return;
            }

            bool newVisibility = layerNodes
                .Select(GetLayerFromNode)
                .Where(layer => layer != null)
                .Any(layer => layer!.IsVisible == false);

            try
            {
                bool rasterStackDirty = false;
                bool vectorStackDirty = false;

                foreach (TreeNode layerNode in layerNodes)
                {
                    CanvasLayer? layer = GetLayerFromNode(layerNode);
                    if (layer == null || layer.IsVisible == newVisibility)
                        continue;

                    CanvasLayer? updatedLayer =
                        await _layerCommandService.SetVisibilityAsync(
                            AppServices.HasContext ? AppServices.Context.Session : null,
                            layer,
                            newVisibility);

                    if (updatedLayer == null)
                        continue;

                    UpdateLayerNode(layerNode, updatedLayer, updateRasterStack: false);
                    if (IsRasterLayer(updatedLayer))
                    {
                        rasterStackDirty = true;
                    }
                    else
                    {
                        vectorStackDirty = true;
                    }
                }

                nodeState.IsGroupCheckedWhenEmpty = newVisibility;
                MarkProjectModifiedIfOpen();
                treeViewLayers.Invalidate();

                if (rasterStackDirty)
                    UpdateRasterCanvasLayersFromTree();

                if (vectorStackDirty)
                    UpdateVectorCanvasLayersFromTree();

                SetCanvasCommandStatus(newVisibility
                    ? $"Layer group shown: {groupNode.Text}"
                    : $"Layer group hidden: {groupNode.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update layer group visibility: {ex.Message}",
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

            if (IsProtectedRePlotDefaultLayerNameNode(node, nodeState.Layer))
            {
                MessageBox.Show(
                    "Layer Name cannot be changed",
                    "Rename Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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

        private bool CanMoveRasterLayerInDisplayOrder(
            TreeNode? node,
            int visualDirection)
        {
            if (node?.Parent == null ||
                !IsRasterLayer(GetLayerFromNode(node)) ||
                IsOnlineBasemapLayer(node))
            {
                return false;
            }

            int targetIndex = node.Index + Math.Sign(visualDirection);
            if (targetIndex < 0 || targetIndex >= node.Parent.Nodes.Count)
            {
                return false;
            }

            TreeNode targetNode = node.Parent.Nodes[targetIndex];
            return IsRasterLayer(GetLayerFromNode(targetNode)) &&
                   !IsOnlineBasemapLayer(targetNode);
        }

        private async Task MoveRasterLayerInDisplayOrderAsync(
            TreeNode? node,
            int visualDirection)
        {
            if (!CanMoveRasterLayerInDisplayOrder(node, visualDirection) ||
                node?.Parent == null)
            {
                return;
            }

            TreeNode parent = node.Parent;
            CanvasLayer? currentLayer = GetLayerFromNode(node);
            List<CanvasLayer> visualLayers = parent.Nodes
                .Cast<TreeNode>()
                .Select(GetLayerFromNode)
                .Where(layer => IsRasterLayer(layer))
                .Select(layer => layer!)
                .ToList();

            int currentIndex = visualLayers.FindIndex(layer =>
                layer.Id == currentLayer?.Id);
            int targetIndex = currentIndex + Math.Sign(visualDirection);
            if (currentIndex < 0 ||
                targetIndex < 0 ||
                targetIndex >= visualLayers.Count ||
                IsOnlineBasemapLayer(visualLayers[currentIndex]) ||
                IsOnlineBasemapLayer(visualLayers[targetIndex]))
            {
                return;
            }

            (visualLayers[currentIndex], visualLayers[targetIndex]) =
                (visualLayers[targetIndex], visualLayers[currentIndex]);

            CanvasLayer movedLayer = visualLayers[targetIndex];
            await PersistRasterVisualOrderAsync(visualLayers);
            MarkProjectModifiedIfOpen();
            await RefreshLayerTreeAsync();
            SelectLayerNodeById(movedLayer.Id);
            SetCanvasCommandStatus($"Layer order updated: {movedLayer.Name}");
        }

        private async Task PersistRasterVisualOrderAsync(
            IReadOnlyList<CanvasLayer> visualLayers)
        {
            List<CanvasLayer> basemapLayers = visualLayers
                .Where(IsOnlineBasemapLayer)
                .ToList();
            List<CanvasLayer> overlayLayers = visualLayers
                .Where(layer => !IsOnlineBasemapLayer(layer))
                .ToList();

            int displayOrder = 0;
            foreach (CanvasLayer basemapLayer in basemapLayers)
            {
                basemapLayer.DisplayOrder = displayOrder++;
                basemapLayer.LastModifiedDate = DateTime.Now;
            }

            foreach (CanvasLayer overlayLayer in overlayLayers.AsEnumerable().Reverse())
            {
                overlayLayer.DisplayOrder = displayOrder++;
                overlayLayer.LastModifiedDate = DateTime.Now;
            }

            if (!AppServices.HasContext)
            {
                UpdateRasterCanvasLayersFromTree();
                return;
            }

            var repository =
                _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);
            foreach (CanvasLayer layer in visualLayers)
            {
                if (layer.Id > 0)
                {
                    await repository.UpdateAsync(layer);
                }
            }
        }

        private async Task DeleteLayerAsync(TreeNode? node)
        {
            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            if (CanvasLayerTreeService.IsProtectedDefaultLayer(layer) &&
                CanvasLayerTreeService.IsDrawingMarkupLayer(layer))
            {
                MessageBox.Show(
                    $"Layer '{layer.Name}' is the default drawing layer and cannot be deleted.",
                    "Delete Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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
                _rasterImportFileManagementService.HandleLayerDeleted(
                    AppServices.Context,
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

        private async Task AddDrawingMarkupLayerAsync(TreeNode? groupNode)
        {
            if (!IsDrawingMarkupGroupNode(groupNode))
                return;

            if (!AppServices.HasContext)
            {
                MessageBox.Show(
                    "Open or create a project before adding drawing layers.",
                    "Add Drawing Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            try
            {
                var repository = _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);
                List<CanvasLayer> existingLayers = await repository.GetAllOrderedAsync();
                string layerName = GetUniqueLayerName(existingLayers, "Drawing Layer");
                int nextDisplayOrder = existingLayers.Count == 0
                    ? 0
                    : existingLayers.Max(layer => layer.DisplayOrder) + 1;
                string drawingColor = GetRandomDrawingMarkupColorHex();

                CanvasLayer newLayer = new()
                {
                    Name = layerName,
                    LayerType = CanvasLayerTreeService.PointLayerType,
                    IsVisible = true,
                    IsLocked = false,
                    IsSelectable = true,
                    IsPrintable = true,
                    DisplayOrder = nextDisplayOrder,
                    BorderColor = drawingColor,
                    LineWeight = 1.3,
                    LineStyle = "Solid",
                    LineTypeScale = 1.0,
                    FillColor = null,
                    FillTransparency = 0,
                    FillStyle = "None",
                    HatchScale = 1.0,
                    PointSymbol = "Dot",
                    PointSize = 5.0,
                    LabelColor = drawingColor,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    Description = $"Drawing/markup layer: {layerName}"
                };

                using var frm = new frmLayerPropertyManager(
                    newLayer,
                    _hatchPatternService,
                    allowRename: true);
                PositionLayerPropertyManager(frm);

                if (frm.ShowDialog(this) != DialogResult.OK)
                    return;

                newLayer.Name = GetUniqueLayerName(existingLayers, newLayer.Name);
                newLayer.Description = $"Drawing/markup layer: {newLayer.Name}";
                CanvasLayer createdLayer = await repository.AddAsync(newLayer);
                MarkProjectModifiedIfOpen();
                await RefreshLayerTreeAsync();
                SetCurrentDrawingLayer(createdLayer);
                SelectLayerNodeById(createdLayer.Id);
                SetCanvasCommandStatus($"Drawing layer added: {createdLayer.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to add the drawing layer: {ex.Message}",
                    "Add Drawing Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates all layer colors to be theme-aware based on the canvas background color.
        /// When canvas theme changes, layer colors are adjusted for visibility and contrast.
        /// </summary>
        private async Task UpdateLayerColorsForCanvasThemeAsync(Color canvasBackgroundColor)
        {
            if (!AppServices.HasContext) return;

            try
            {
                var repository = _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);
                var layers = await repository.GetAllOrderedAsync();

                if (layers.Count == 0) return;

                bool colorsUpdated = false;

                // Update each layer's colors based on canvas theme
                foreach (var layer in layers)
                {
                    string adjustedBorderColor = CanvasThemeColorService.AdjustHexColorForCanvasTheme(
                        canvasBackgroundColor, layer.BorderColor ?? "#000000");
                    string adjustedFillColor = CanvasThemeColorService.AdjustHexColorForCanvasTheme(
                        canvasBackgroundColor, layer.FillColor ?? "#FFFFFF");
                    string adjustedLabelColor = CanvasThemeColorService.AdjustHexColorForCanvasTheme(
                        canvasBackgroundColor, layer.LabelColor ?? "#000000");

                    // Only update if colors changed
                    if (adjustedBorderColor != layer.BorderColor ||
                        adjustedFillColor != layer.FillColor ||
                        adjustedLabelColor != layer.LabelColor)
                    {
                        layer.BorderColor = adjustedBorderColor;
                        layer.FillColor = adjustedFillColor;
                        layer.LabelColor = adjustedLabelColor;
                        layer.LastModifiedDate = DateTime.Now;

                        await repository.UpdateAsync(layer);
                        colorsUpdated = true;
                    }
                }

                // If any colors were updated, refresh everything
                if (colorsUpdated)
                {
                    // Reload layers and refresh tree view (which shows swatches and properties)
                    var updatedLayers = await repository.GetAllOrderedAsync();
                    await RefreshLayerTreeAsync();

                    // Refresh canvas with updated colors
                    mapCanvasControlMain.SetVectorLayers(updatedLayers);
                    mapCanvasControlMain.RequestRender();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"UpdateLayerColorsForCanvasTheme failed: {ex.Message}");
            }
        }

        private static string GetUniqueLayerName(
            IEnumerable<CanvasLayer> existingLayers,
            string? requestedName)
        {
            string baseName = string.IsNullOrWhiteSpace(requestedName)
                ? "Drawing Layer"
                : requestedName.Trim();

            HashSet<string> names = existingLayers
                .Select(layer => layer.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!names.Contains(baseName))
                return baseName;

            int suffix = 2;
            string candidate;
            do
            {
                candidate = $"{baseName} {suffix++}";
            }
            while (names.Contains(candidate));

            return candidate;
        }

        private void SetCurrentDrawingLayer(CanvasLayer layer)
        {
            _currentDrawingLayer = layer;
            RefreshCurrentDrawingLayerCombo();
            SelectCurrentDrawingLayerById(layer.Id);
            mapCanvasControlMain.SetActiveTool(_currentCanvasTool, layer);
        }

        private async Task ZoomToLayerAsync(TreeNode? node)
        {
            if (IsOnlineBasemapLayer(node))
            {
                return;
            }

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            await TryZoomToLayerAsync(layer, showErrorDialog: true);
        }

        private async Task<bool> TryZoomToLayerAsync(
            CanvasLayer layer,
            bool showErrorDialog)
        {
            try
            {
                if (string.Equals(
                        layer.LayerType,
                        CanvasLayerTreeService.RasterLayerType,
                        StringComparison.OrdinalIgnoreCase) &&
                    mapCanvasControlMain.ZoomToRasterLayer(layer.Id))
                {
                    mapCanvasControlMain.SetPanToolActive(false);
                    mnuPan.Checked = false;
                    return true;
                }

                RectangleD? bounds =
                    await _layerBoundsService.GetWorldBoundsAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        layer,
                        AppServices.HasContext
                            ? AppServices.Context.ProjectFolderPath
                            : null);

                if (!bounds.HasValue)
                {
                    if (showErrorDialog)
                    {
                        MessageBox.Show(
                            $"Layer '{layer.Name}' does not have drawable bounds yet.",
                            "Zoom To Layer",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    return false;
                }

                mapCanvasControlMain.SetPanToolActive(false);
                mnuPan.Checked = false;
                mapCanvasControlMain.ZoomToWorldBounds(bounds.Value);
                return true;
            }
            catch (Exception ex)
            {
                if (showErrorDialog)
                {
                    MessageBox.Show(
                        $"Failed to zoom to the layer: {ex.Message}",
                        "Zoom To Layer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    LogProjectError(
                        $"Automatic zoom-to-layer failed for '{layer.Name}'.",
                        ex);
                }

                return false;
            }
            finally
            {
                HideOperationProgress();
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
                nodeState.IsOnlineBasemap = IsOnlineBasemapLayer(layer);
            }

            node.Text = layer.Name;
            treeViewLayers.Invalidate();

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

                SetOperationProgress(startPercent, status, showProgressForm: false);
                SetOperationProgress(layerPercent, "Loading map layers", showProgressForm: false);
                await RefreshLayerTreeAsync();

                SetOperationProgress(renderPercent, "Rendering map canvas", showProgressForm: false);
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Canvas: Refreshed");

                SetOperationProgress(100, "Canvas refreshed", showProgressForm: false);
                await Task.Delay(250);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private async Task RefreshVectorCanvasFeaturesAsync()
        {
            if (!AppServices.HasContext)
            {
                mapCanvasControlMain.SetVectorFeatures([]);
                return;
            }

            CanvasFeatureService featureService =
                _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
            IReadOnlyList<CanvasFeature> features =
                await featureService.GetAllAsync();
            mapCanvasControlMain.SetVectorFeatures(features);
        }

        private async void MapCanvasControlMain_ShapeCompleted(IShape shape)
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            try
            {
                CanvasLayer? drawingLayer = GetSelectedCurrentDrawingLayer() ?? _currentDrawingLayer;
                if (drawingLayer != null)
                {
                    shape.LayerName = drawingLayer.Name;
                    shape.Properties[CanvasFeatureService.CanvasLayerIdPropertyKey] = drawingLayer.Id;
                }

                CanvasFeatureService featureService =
                    _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
                CanvasFeature feature = await featureService.SaveShapeAsync(
                    shape,
                    shape.LayerName);
                MarkProjectModifiedIfOpen();
                await RefreshVectorCanvasFeaturesAsync();
                SetCanvasCommandStatus($"Created {feature.CanvasObject.ObjectType}: {feature.Layer?.Name ?? shape.LayerName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save the drawn feature: {ex.Message}",
                    "Drawing Tools",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async void MapCanvasControlMain_SelectedObjectsDeleteRequested(IReadOnlyList<Guid> shapeIds)
        {
            if (!AppServices.HasContext || shapeIds.Count == 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                shapeIds.Count == 1
                    ? "Delete the selected drawing object?"
                    : $"Delete {shapeIds.Count} selected drawing objects?",
                "Delete Drawing Object",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                CanvasFeatureService featureService =
                    _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);

                foreach (Guid shapeId in shapeIds.Distinct())
                {
                    await featureService.DeleteShapeAsync(shapeId);
                }

                mapCanvasControlMain.ClearSelectionAfterDelete();
                MarkProjectModifiedIfOpen();
                await RefreshVectorCanvasFeaturesAsync();
                SetCanvasCommandStatus(shapeIds.Count == 1
                    ? "Deleted selected drawing object"
                    : $"Deleted {shapeIds.Count} selected drawing objects");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete the selected drawing objects: {ex.Message}",
                    "Delete Drawing Object",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task RefreshLayerTreeAsync(
            bool rebuildRasterLayersAfterCrsChange = false)
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

                PopulateLayerTree(
                    layerGroups,
                    rebuildRasterLayersAfterCrsChange);
                await RefreshVectorCanvasFeaturesAsync();
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
            IReadOnlyList<CanvasLayerTreeGroup> layerGroups,
            bool rebuildRasterLayersAfterCrsChange = false)
        {
            _suppressLayerTreeEvents = true;

            try
            {
                treeViewLayers.BeginUpdate();
                treeViewLayers.Nodes.Clear();

                TreeNode? rePlotRootNode = null;
                TreeNode? blockLayoutNode = null;
                foreach (CanvasLayerTreeGroup group in layerGroups)
                {
                    TreeNode groupNode =
                        CreateLayerGroupNode(group.Key, group.Name);

                    PopulateGroupNodeLayers(groupNode, group);

                    if (IsRePlotDataGroupKey(group.Key))
                    {
                        rePlotRootNode ??= CreateRePlotRootNode();
                        if (string.Equals(group.Key, RoadsGroupKey, StringComparison.OrdinalIgnoreCase))
                        {
                            blockLayoutNode ??= FindGroupNode(rePlotRootNode, BlockLayoutGroupKey);
                            if (blockLayoutNode == null)
                                rePlotRootNode.Nodes.Add(groupNode);
                            else
                                blockLayoutNode.Nodes.Add(groupNode);
                        }
                        else
                        {
                            rePlotRootNode.Nodes.Add(groupNode);
                            if (string.Equals(group.Key, BlockLayoutGroupKey, StringComparison.OrdinalIgnoreCase))
                                blockLayoutNode = groupNode;
                        }
                    }
                    else
                    {
                        treeViewLayers.Nodes.Add(groupNode);
                    }

                    groupNode.Expand();
                }

                if (rePlotRootNode != null)
                {
                    treeViewLayers.Nodes.Insert(0, rePlotRootNode);
                    rePlotRootNode.Expand();
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

            UpdateRasterCanvasLayersFromTree(rebuildRasterLayersAfterCrsChange);
            UpdateVectorCanvasLayersFromTree();
        }

        private void PopulateGroupNodeLayers(
            TreeNode groupNode,
            CanvasLayerTreeGroup group)
        {
            if (!string.Equals(group.Key, OriginalDataGroupKey, StringComparison.OrdinalIgnoreCase))
            {
                foreach (CanvasLayer layer in OrderLayerGroupForDisplay(group.Key, group.Layers))
                    groupNode.Nodes.Add(CreateLayerNode(layer));

                return;
            }

            TreeNode cadastralMapNode = CreateLayerGroupNode(CadastralMapGroupKey, "Cadastral Map");
            foreach (CanvasLayer layer in OrderLayerGroupForDisplay(group.Key, group.Layers))
            {
                if (IsCadastralCanvasLayer(layer))
                    cadastralMapNode.Nodes.Add(CreateLayerNode(layer));
                else
                    groupNode.Nodes.Add(CreateLayerNode(layer));
            }

            groupNode.Nodes.Add(cadastralMapNode);
            cadastralMapNode.Expand();
        }

        private static bool IsCadastralCanvasLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, "BaselineParcel", StringComparison.OrdinalIgnoreCase) ||
                   layer.Description?.StartsWith(
                       "Imported cadastral map layer",
                       StringComparison.OrdinalIgnoreCase) == true;
        }

        private void ResetLayerTree()
        {
            _suppressLayerTreeEvents = true;

            try
            {
                treeViewLayers.BeginUpdate();
                treeViewLayers.Nodes.Clear();

                TreeNode? rePlotRootNode = null;
                TreeNode? blockLayoutNode = null;
                foreach (CanvasLayerTreeGroup group
                    in CanvasLayerTreeService.GetDefaultLayerTree())
                {
                    TreeNode groupNode =
                        CreateLayerGroupNode(group.Key, group.Name);

                    PopulateGroupNodeLayers(groupNode, group);

                    if (IsRePlotDataGroupKey(group.Key))
                    {
                        rePlotRootNode ??= CreateRePlotRootNode();
                        if (string.Equals(group.Key, RoadsGroupKey, StringComparison.OrdinalIgnoreCase))
                        {
                            blockLayoutNode ??= FindGroupNode(rePlotRootNode, BlockLayoutGroupKey);
                            if (blockLayoutNode == null)
                                rePlotRootNode.Nodes.Add(groupNode);
                            else
                                blockLayoutNode.Nodes.Add(groupNode);
                        }
                        else
                        {
                            rePlotRootNode.Nodes.Add(groupNode);
                            if (string.Equals(group.Key, BlockLayoutGroupKey, StringComparison.OrdinalIgnoreCase))
                                blockLayoutNode = groupNode;
                        }
                    }
                    else
                    {
                        treeViewLayers.Nodes.Add(groupNode);
                    }

                    groupNode.Expand();
                }

                if (rePlotRootNode != null)
                {
                    treeViewLayers.Nodes.Insert(0, rePlotRootNode);
                    rePlotRootNode.Expand();
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
            UpdateVectorCanvasLayersFromTree();
            mapCanvasControlMain.SetVectorFeatures([]);
        }

        private void PopulateLayerProperties(CanvasLayer layer)
        {
        }

        private void ClearLayerProperties()
        {
        }

        private void SetCanvasCommandStatus(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
                lblStatusMessage.Text = status;
        }

        private async Task PersistProjectUiStateAsync()
        {
            await PersistCanvasViewportStateAsync();
            await PersistOpenXyzTileOptionsStateAsync();
        }

        private async Task PersistCanvasViewportStateAsync()
        {
            if (!AppServices.HasContext)
                return;

            ProjectSettings? settings = await LoadProjectSettingsForUiStateAsync();
            if (settings == null)
                return;

            MapCanvasViewportState viewportState =
                mapCanvasControlMain.GetViewportState();
            settings.CanvasViewportCenterX = viewportState.CenterX;
            settings.CanvasViewportCenterY = viewportState.CenterY;
            settings.CanvasViewportZoomScale = viewportState.ZoomScale;
            settings.CanvasViewportVisibleWidth = viewportState.VisibleWidth;
            settings.CanvasViewportVisibleHeight = viewportState.VisibleHeight;

            await SaveProjectSettingsForUiStateAsync(settings);
        }

        private async Task RestoreCanvasViewportStateAsync()
        {
            if (!AppServices.HasContext)
                return;

            ProjectSettings? settings = await LoadProjectSettingsForUiStateAsync();
            if (settings == null ||
                !settings.CanvasViewportCenterX.HasValue ||
                !settings.CanvasViewportCenterY.HasValue ||
                !settings.CanvasViewportZoomScale.HasValue)
            {
                return;
            }

            mapCanvasControlMain.TryApplyViewportState(
                new MapCanvasViewportState(
                    settings.CanvasViewportCenterX.Value,
                    settings.CanvasViewportCenterY.Value,
                    settings.CanvasViewportZoomScale.Value,
                    settings.CanvasViewportVisibleWidth ?? 0,
                    settings.CanvasViewportVisibleHeight ?? 0));
        }

        private async Task PersistOpenXyzTileOptionsStateAsync()
        {
            if (_xyzTileImportOptionsForm == null ||
                _xyzTileImportOptionsForm.IsDisposed)
            {
                return;
            }

            await PersistXyzTileOptionsStateAsync(
                _xyzTileImportOptionsForm.GetCurrentOptionsState());
        }

        private async Task PersistXyzTileOptionsStateAsync(
            XyzTileImportOptionsState state)
        {
            if (!AppServices.HasContext)
                return;

            ProjectSettings? settings = await LoadProjectSettingsForUiStateAsync();
            if (settings == null)
                return;

            ApplyXyzTileOptionsState(settings, state);
            await SaveProjectSettingsForUiStateAsync(settings);
        }

        private async Task TryPersistXyzTileOptionsStateAsync(
            XyzTileImportOptionsState state)
        {
            try
            {
                await PersistXyzTileOptionsStateAsync(state);
            }
            catch (Exception ex)
            {
                LogProjectError(
                    "Failed to persist XYZ tile import form state.",
                    ex);
            }
        }

        private async Task<XyzTileImportOptionsState?> LoadXyzTileOptionsStateAsync()
        {
            if (!AppServices.HasContext)
                return null;

            ProjectSettings? settings = await LoadProjectSettingsForUiStateAsync();
            if (settings == null)
                return null;

            return new XyzTileImportOptionsState(
                settings.LastXyzLayerName,
                settings.LastXyzTileSourceUrlTemplate,
                settings.LastXyzMinLongitude,
                settings.LastXyzMinLatitude,
                settings.LastXyzMaxLongitude,
                settings.LastXyzMaxLatitude,
                settings.LastXyzZoomLevel,
                settings.LastXyzImageExtension,
                settings.LastXyzDownloadMinLongitude,
                settings.LastXyzDownloadMinLatitude,
                settings.LastXyzDownloadMaxLongitude,
                settings.LastXyzDownloadMaxLatitude);
        }

        private static void ApplyXyzTileOptionsState(
            ProjectSettings settings,
            XyzTileImportOptionsState state)
        {
            settings.LastXyzLayerName = state.LayerName;
            settings.LastXyzTileSourceUrlTemplate = state.UrlTemplate;
            settings.LastXyzMinLongitude = state.MinLongitude;
            settings.LastXyzMinLatitude = state.MinLatitude;
            settings.LastXyzMaxLongitude = state.MaxLongitude;
            settings.LastXyzMaxLatitude = state.MaxLatitude;
            settings.LastXyzZoomLevel = state.ZoomLevel;
            settings.LastXyzImageExtension = state.ImageExtension;
            settings.LastXyzDownloadMinLongitude = state.LastDownloadMinLongitude;
            settings.LastXyzDownloadMinLatitude = state.LastDownloadMinLatitude;
            settings.LastXyzDownloadMaxLongitude = state.LastDownloadMaxLongitude;
            settings.LastXyzDownloadMaxLatitude = state.LastDownloadMaxLatitude;
        }

        private async Task<ProjectSettings?> LoadProjectSettingsForUiStateAsync()
        {
            var service = _projectScopedFactory.CreateProjectSettingsService(
                AppServices.Context.Session);
            return await service.GetAsync();
        }

        private async Task SaveProjectSettingsForUiStateAsync(
            ProjectSettings settings)
        {
            var service = _projectScopedFactory.CreateProjectSettingsService(
                AppServices.Context.Session);
            await service.SaveAsync(settings);
        }

        private static bool IsLayerNode(TreeNode? node)
        {
            return node?.Tag is LayerTreeNodeState state &&
                   state.IsLayerNode &&
                   state.Layer != null;
        }

        private static bool IsLayerGroupNode(TreeNode? node)
        {
            return node?.Tag is LayerTreeNodeState state &&
                   !state.IsLayerNode &&
                   !string.IsNullOrWhiteSpace(state.GroupKey);
        }

        private static bool IsRePlotDataGroupKey(string? groupKey)
        {
            return CanvasLayerTreeService.IsRePlotDataGroupKey(groupKey);
        }

        private static TreeNode? FindGroupNode(TreeNode rootNode, string groupKey)
        {
            foreach (TreeNode node in rootNode.Nodes)
            {
                if (node.Tag is LayerTreeNodeState state &&
                    string.Equals(state.GroupKey, groupKey, StringComparison.OrdinalIgnoreCase))
                {
                    return node;
                }

                TreeNode? childMatch = FindGroupNode(node, groupKey);
                if (childMatch != null)
                    return childMatch;
            }

            return null;
        }

        private static bool IsRasterLayerGroupNode(TreeNode? node)
        {
            return string.Equals(
                node?.Name,
                $"{LayerGroupNodeNamePrefix}{RasterLayerGroupKey}",
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDrawingMarkupGroupNode(TreeNode? node)
        {
            return string.Equals(
                node?.Name,
                $"{LayerGroupNodeNamePrefix}{DrawingMarkupGroupKey}",
                StringComparison.OrdinalIgnoreCase);
        }

        private static CanvasLayer? GetLayerFromNode(TreeNode? node)
        {
            return node?.Tag is LayerTreeNodeState state &&
                   state.IsLayerNode
                ? state.Layer
                : null;
        }

        private static IEnumerable<TreeNode> EnumerateLayerNodes(TreeNode parentNode)
        {
            foreach (TreeNode childNode in parentNode.Nodes)
            {
                if (IsLayerNode(childNode))
                {
                    yield return childNode;
                }

                foreach (TreeNode descendantNode in EnumerateLayerNodes(childNode))
                    yield return descendantNode;
            }
        }

        private static VisualStyles.CheckBoxState GetGroupCheckBoxState(TreeNode groupNode)
        {
            List<CanvasLayer> layers = EnumerateLayerNodes(groupNode)
                .Select(GetLayerFromNode)
                .Where(layer => layer != null)
                .Select(layer => layer!)
                .ToList();

            if (layers.Count == 0)
            {
                return groupNode.Tag is LayerTreeNodeState state &&
                       state.IsGroupCheckedWhenEmpty
                    ? VisualStyles.CheckBoxState.CheckedNormal
                    : VisualStyles.CheckBoxState.UncheckedNormal;
            }

            if (layers.All(layer => layer.IsVisible))
                return VisualStyles.CheckBoxState.CheckedNormal;

            if (layers.All(layer => !layer.IsVisible))
                return VisualStyles.CheckBoxState.UncheckedNormal;

            return VisualStyles.CheckBoxState.MixedNormal;
        }

        private static bool IsRasterLayer(CanvasLayer? layer)
        {
            return CanvasLayerBoundsService.IsRasterLayer(layer);
        }

        private bool IsOnlineBasemapLayer(TreeNode? node)
        {
            if (node?.Tag is not LayerTreeNodeState state)
            {
                return false;
            }

            return state.IsOnlineBasemap ||
                   IsOnlineBasemapLayer(state.Layer);
        }

        private bool IsOnlineBasemapLayer(CanvasLayer? layer)
        {
            if (!IsRasterLayer(layer) ||
                string.IsNullOrWhiteSpace(layer?.SourceFile))
            {
                return false;
            }

            string sourcePath = ResolveLayerSourcePathForUi(layer.SourceFile);
            if (File.Exists(sourcePath) &&
                XyzLiveTileRenderLayer.IsLiveTileVrtPath(sourcePath))
            {
                return true;
            }

            return layer.SourceFile.EndsWith(
                       ".vrt",
                       StringComparison.OrdinalIgnoreCase) &&
                   layer.Description != null &&
                   (layer.Description.Contains(
                        "internet",
                        StringComparison.OrdinalIgnoreCase) ||
                    layer.Description.Contains(
                        "lazy VRT",
                        StringComparison.OrdinalIgnoreCase) ||
                    layer.Description.Contains(
                        "GDAL_WMS",
                        StringComparison.OrdinalIgnoreCase));
        }

        private string ResolveLayerSourcePathForUi(string sourceFile)
        {
            if (Path.IsPathRooted(sourceFile))
                return Path.GetFullPath(sourceFile);

            return AppServices.HasContext
                ? Path.GetFullPath(Path.Combine(AppServices.Context.ProjectFolderPath, sourceFile))
                : Path.GetFullPath(sourceFile);
        }

        private IEnumerable<CanvasLayer> OrderLayerGroupForDisplay(
            string groupKey,
            IReadOnlyList<CanvasLayer> layers)
        {
            if (!string.Equals(
                    groupKey,
                    RasterLayerGroupKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                return layers;
            }

            return layers
                .OrderBy(layer => IsOnlineBasemapLayer(layer) ? 1 : 0)
                .ThenByDescending(layer => layer.DisplayOrder)
                .ThenBy(layer => layer.Name);
        }

        private Rectangle GetLayerNodeCheckBoxRect(TreeNode node)
        {
            return new Rectangle(
                node.Bounds.X,
                node.Bounds.Y + Math.Max(0, (treeViewLayers.ItemHeight - LayerNodeCheckBoxSize) / 2),
                LayerNodeCheckBoxSize,
                LayerNodeCheckBoxSize);
        }

        private Rectangle GetLayerNodeColorRect(TreeNode node, CanvasLayer? layer = null)
        {
            Rectangle checkBoxRect = GetLayerNodeCheckBoxRect(node);
            int symbolWidth = layer != null && CanvasLayerTreeService.IsLineLayer(layer)
                ? 28
                : LayerNodeColorBoxSize;

            return new Rectangle(
                checkBoxRect.Right + LayerNodeCheckBoxGap,
                node.Bounds.Y + Math.Max(0, (treeViewLayers.ItemHeight - LayerNodeColorBoxSize) / 2),
                symbolWidth,
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

            using var frm = new frmLayerPropertyManager(editableLayer, _hatchPatternService);
            PositionLayerPropertyManager(frm);

            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            if (nodeState.Layer.IsLocked && editableLayer.IsLocked)
            {
                SetCanvasCommandStatus($"Layer locked: {nodeState.Layer.Name}");
                return;
            }

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
                    mapCanvasControlMain.UpdateVectorLayer(updatedLayer);
                    // Sync the drawing-layer combo so the updated layer object
                    // (with the new color/style) replaces the stale cached item.
                    // Without this, re-activating a drawing tool passes the old
                    // layer to SetActiveTool and reverts the preview back to the
                    // previous color.
                    RefreshCurrentDrawingLayerCombo();
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
            foreach (TreeNode rootNode in treeViewLayers.Nodes)
            {
                foreach (TreeNode layerNode in EnumerateLayerNodes(rootNode))
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

        private void UpdateRasterCanvasLayersFromTree(
            bool rebuildRasterLayersAfterCrsChange = false)
        {
            string? projectFolderPath = AppServices.HasContext
                ? AppServices.Context.ProjectFolderPath
                : null;

            List<CanvasLayer> rasterLayers = new List<CanvasLayer>();

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

            List<CanvasLayer> orderedRasterLayers = rasterLayers
                .OrderBy(layer => IsOnlineBasemapLayer(layer) ? 0 : 1)
                .ThenBy(layer => layer.DisplayOrder)
                .ThenBy(layer => layer.Name)
                .ToList();

            if (rebuildRasterLayersAfterCrsChange)
            {
                mapCanvasControlMain.RebuildRasterLayersAfterCrsChange(
                    orderedRasterLayers,
                    projectFolderPath,
                    _currentProjectRasterSrsDefinition);
                return;
            }

            mapCanvasControlMain.SetRasterLayers(
                orderedRasterLayers,
                projectFolderPath,
                _currentProjectRasterSrsDefinition);
        }

        private void UpdateVectorCanvasLayersFromTree()
        {
            List<CanvasLayer> vectorLayers = new List<CanvasLayer>();

            foreach (TreeNode rootNode in treeViewLayers.Nodes)
            {
                foreach (TreeNode layerNode in EnumerateLayerNodes(rootNode))
                {
                    CanvasLayer? layer = GetLayerFromNode(layerNode);
                    if (layer != null && !IsRasterLayer(layer))
                    {
                        vectorLayers.Add(layer);
                    }
                }
            }

            mapCanvasControlMain.SetVectorLayers(
                vectorLayers
                    .OrderBy(layer => layer.DisplayOrder)
                    .ThenBy(layer => layer.Name)
                    .ToList());

            RefreshCurrentDrawingLayerCombo();
        }

        private void RefreshCurrentDrawingLayerCombo()
        {
            int? previousLayerId = GetSelectedCurrentDrawingLayer()?.Id ??
                                   _currentDrawingLayer?.Id;
            List<CanvasLayer> drawingLayers = GetDrawingMarkupLayersFromTree()
                .Where(layer => !IsRasterLayer(layer))
                .OrderBy(layer => layer.DisplayOrder)
                .ThenBy(layer => layer.Name)
                .ToList();

            _suppressCurrentDrawingLayerSelectionChanged = true;
            try
            {
                cboCurrentDrawingLayer.Items.Clear();
                foreach (CanvasLayer layer in drawingLayers)
                {
                    cboCurrentDrawingLayer.Items.Add(new DrawingLayerComboItem(layer));
                }

                int selectedIndex = -1;
                if (previousLayerId.HasValue)
                {
                    selectedIndex = FindCurrentDrawingLayerComboIndex(previousLayerId.Value);
                }

                if (selectedIndex < 0 &&
                    drawingLayers.Count > 0)
                {
                    selectedIndex = 0;
                }

                cboCurrentDrawingLayer.SelectedIndex = selectedIndex;
                _currentDrawingLayer = selectedIndex >= 0 &&
                                       cboCurrentDrawingLayer.Items[selectedIndex] is DrawingLayerComboItem selectedItem
                    ? selectedItem.Layer
                    : null;
            }
            finally
            {
                _suppressCurrentDrawingLayerSelectionChanged = false;
            }

            UpdateDrawingToolAvailability();
        }

        private int FindCurrentDrawingLayerComboIndex(int layerId)
        {
            for (int index = 0; index < cboCurrentDrawingLayer.Items.Count; index++)
            {
                if (cboCurrentDrawingLayer.Items[index] is DrawingLayerComboItem item &&
                    item.Layer.Id == layerId)
                {
                    return index;
                }
            }

            return -1;
        }

        private List<CanvasLayer> GetDrawingMarkupLayersFromTree()
        {
            List<CanvasLayer> layers = new List<CanvasLayer>();
            foreach (TreeNode rootNode in treeViewLayers.Nodes)
            {
                foreach (TreeNode groupNode in EnumerateGroupNodes(rootNode, DrawingMarkupGroupKey))
                {
                    foreach (TreeNode layerNode in EnumerateLayerNodes(groupNode))
                    {
                        CanvasLayer? layer = GetLayerFromNode(layerNode);
                        if (layer != null)
                        {
                            layers.Add(layer);
                        }
                    }
                }
            }

            return layers;
        }

        private static IEnumerable<TreeNode> EnumerateGroupNodes(
            TreeNode node,
            string groupKey)
        {
            if (node.Tag is LayerTreeNodeState nodeState &&
                string.Equals(nodeState.GroupKey, groupKey, StringComparison.OrdinalIgnoreCase))
            {
                yield return node;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                foreach (TreeNode descendantNode in EnumerateGroupNodes(childNode, groupKey))
                {
                    yield return descendantNode;
                }
            }
        }

        private async Task RefreshCurrentProjectRasterSrsDefinitionAsync()
        {
            _currentProjectRasterSrsDefinition = null;

            if (!AppServices.HasContext)
            {
                return;
            }

            try
            {
                ProjectRasterCrsResolver resolver = new(_projectScopedFactory);
                ProjectRasterCrsContext crsContext =
                    await resolver.ResolveAsync(AppServices.Context.Session);
                _currentProjectRasterSrsDefinition =
                    crsContext.TargetSrsDefinition;
            }
            catch (Exception ex)
            {
                LogProjectError(
                    "Failed to resolve current project CRS for live tile rendering.",
                    ex);
            }
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
        private void SetOperationProgress(int percent, string status, bool showProgressForm = true)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetOperationProgress(percent, status, showProgressForm)));
                return;
            }

            int clampedPercent = Math.Clamp(percent, 0, 100);

            // Progress callbacks can arrive slightly after an awaited import/render
            // has completed. Do not let a late 94-100% callback resurrect the bar
            // after HideOperationProgress has already returned the strip to idle.
            if (!_operationProgressActive && clampedPercent >= 90)
                return;

            _operationProgressActive = true;

            lblOperationProgressStatus.Text = status;
            lblOperationProgressStatus.Visible = true;
            lblOperationProgressStatus.AutoSize = false;
            lblOperationProgressStatus.Width = CalculateOperationStatusWidth();
            lblOperationProgressStatus.TextAlign = ContentAlignment.MiddleRight;

            hostOperationProgress.Value = clampedPercent;
            hostOperationProgress.Visible = true;
            hostOperationProgress.Invalidate();
            hostProgressBarHost.Size = new Size(154, 26);
            hostProgressBarHost.Visible = true;
            if (showProgressForm)
            {
                ShowOperationProgressForm(
                    GetOperationProgressTitle(status),
                    status,
                    clampedPercent);
            }
            else
            {
                HideOperationProgressForm();
            }
            statusCanvas.PerformLayout();
            statusCanvas.Refresh();
        }

        private int CalculateOperationStatusWidth()
        {
            int reservedWidth =
                lblStatusMessage.Width +
                lblCanvasCoordinates.Width +
                260;

            int availableWidth = Math.Max(220, statusCanvas.ClientSize.Width - reservedWidth);
            return Math.Clamp(availableWidth, 260, 460);
        }

        private void ShowOperationProgressForm(
            string title,
            string status,
            int percent)
        {
            if (IsDisposed || Disposing) return;

            _operationProgressForm ??= new frmOperationProgress();

            if (_operationProgressForm.IsDisposed)
                _operationProgressForm = new frmOperationProgress();

            _operationProgressForm.Owner = this;
            _operationProgressForm.UpdateProgress(title, status, percent);

            if (!_operationProgressForm.Visible)
                _operationProgressForm.Show(this);

            PositionOperationProgressForm(_operationProgressForm);
        }

        private void PositionOperationProgressForm(Form form)
        {
            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            int x = Math.Max(
                workingArea.Left,
                this.Left + (this.Width - form.Width) / 2);
            int y = Math.Max(
                workingArea.Top,
                this.Top + (this.Height - form.Height) / 2);

            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(
                Math.Min(x, workingArea.Right - form.Width),
                Math.Min(y, workingArea.Bottom - form.Height));
        }

        private void HideOperationProgressForm()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideOperationProgressForm));
                return;
            }

            _operationProgressForm?.Close();
            _operationProgressForm = null;
        }

        private static string GetOperationProgressTitle(string status)
        {
            if (status.StartsWith("Creating", StringComparison.OrdinalIgnoreCase))
                return "Creating Project";

            if (status.StartsWith("Opening", StringComparison.OrdinalIgnoreCase) ||
                status.StartsWith("Loading", StringComparison.OrdinalIgnoreCase))
                return "Opening Project";

            if (status.StartsWith("Saving", StringComparison.OrdinalIgnoreCase) ||
                status.StartsWith("Writing", StringComparison.OrdinalIgnoreCase))
                return "Saving Project";

            if (status.StartsWith("Configuring", StringComparison.OrdinalIgnoreCase) ||
                status.StartsWith("Applying", StringComparison.OrdinalIgnoreCase) ||
                status.StartsWith("Preparing", StringComparison.OrdinalIgnoreCase))
                return "Setting Up Workspace";

            if (status.Contains("raster", StringComparison.OrdinalIgnoreCase) ||
                status.Contains("XYZ", StringComparison.OrdinalIgnoreCase))
                return "Importing Map Data";

            return "Project Operation";
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

            _operationProgressActive = false;
            lblOperationProgressStatus.Visible = true;
            lblOperationProgressStatus.AutoSize = false;
            lblOperationProgressStatus.Width = CalculateOperationStatusWidth();
            lblOperationProgressStatus.TextAlign = ContentAlignment.MiddleRight;
            hostOperationProgress.Value = 0;
            hostOperationProgress.Visible = true;
            hostOperationProgress.Invalidate();
            hostProgressBarHost.Size = new Size(154, 26);
            hostProgressBarHost.Visible = true;
            _operationProgressForm?.Close();
            _operationProgressForm = null;
            statusCanvas.PerformLayout();
            statusCanvas.Refresh();
        }

        private void MapCanvasControlMain_StatusChanged(string coordinates, string mode, double zoomScale)
        {
            lblCanvasCoordinates.Text = coordinates;
            UpdateActiveTool(mode);
            UpdateScaleLabel(zoomScale);
        }

        private void XyzLiveTileRenderLayer_FetchStatusChanged(
            object? sender,
            LiveTileFetchStatusChangedEventArgs e)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(
                        new Action(
                            () => XyzLiveTileRenderLayer_FetchStatusChanged(
                                sender,
                                e)));
                }
                catch (InvalidOperationException)
                {
                    // The form handle can disappear while background tile work finishes.
                }

                return;
            }

            _liveTileFetchStatus.Text = string.Empty;
            _liveTileFetchStatus.Visible = true;
            PlaceLiveTileFetchStatusLeftOfCoordinates();

            if (e.IsDisconnected)
            {
                _liveTileFetchTimer.Stop();
                _liveTileFetchFrameIndex = 0;
                _liveTileFetchStatus.Image =
                    _liveTileDisconnectedGlobe ?? _liveTileStaticGlobe;
            }
            else if (e.IsFetching)
            {
                if (_liveTileFetchFrames.Count > 0)
                {
                    if (!_liveTileFetchTimer.Enabled)
                    {
                        _liveTileFetchFrameIndex = 0;
                        _liveTileFetchStatus.Image = _liveTileFetchFrames[0];
                        _liveTileFetchTimer.Start();
                    }
                }
                else
                {
                    _liveTileFetchStatus.Image = _liveTileStaticGlobe;
                }
            }
            else
            {
                _liveTileFetchTimer.Stop();
                _liveTileFetchFrameIndex = 0;
                _liveTileFetchStatus.Image = _liveTileStaticGlobe;
            }

            statusCanvas.PerformLayout();
            statusCanvas.Refresh();
        }

        private void UpdateActiveTool(string mode)
        {
            string toolName = mode switch
            {
                _ when mode.Contains("Pan", StringComparison.OrdinalIgnoreCase) => "Pan",
                _ when mode.Contains("Zoom Window", StringComparison.OrdinalIgnoreCase) => "Zoom Window",
                _ when mode.Contains("Zoom", StringComparison.OrdinalIgnoreCase) => "Zoom",
                _ when mode.Contains("Draw", StringComparison.OrdinalIgnoreCase) =>
                    mode
                        .Replace("Mode:", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Trim(),
                _ => "Select"
            };
            lblActiveTool.Text = $"Active Tool: {toolName}";
        }

        private void UpdateScaleLabel(double zoomScale)
        {
            if (zoomScale <= 0)
            {
                lblScale.Text = "Scale: 1:—";
                return;
            }
            // 96 DPI screen: pixels per metre = 96 / 0.0254
            const double screenPixelsPerMetre = 96.0 / 0.0254;
            double denominator = screenPixelsPerMetre / zoomScale;
            string denominatorText = denominator >= 1.0
                ? $"{(long)Math.Round(denominator):N0}"
                : denominator.ToString("0.###");

            lblScale.Text = $"Scale: 1:{denominatorText}";
        }

        /// <summary>
        /// ToolStrip-compatible host for the custom-painted status-strip progress bar.
        /// Inheriting from ToolStripControlHost (a ToolStripItem) lets the WinForms
        /// designer place this inside a StatusStrip without regenerating a type error.
        /// </summary>
        private sealed class StatusProgressBar : ToolStripControlHost
        {
            private readonly InnerBar _inner;

            public StatusProgressBar() : this(new InnerBar()) { }
            private StatusProgressBar(InnerBar inner) : base(inner) { _inner = inner; }

            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Value { get => _inner.Value; set => _inner.Value = value; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Minimum { get => _inner.Minimum; set => _inner.Minimum = value; }
            [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
            public int Maximum { get => _inner.Maximum; set => _inner.Maximum = value; }
            public new void Invalidate() => _inner.Invalidate();

            private sealed class InnerBar : ProgressBar
            {
                public InnerBar()
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

                    if (Value > Minimum)
                    {
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

        private void statusCanvas_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void hostProgressBarHost_Click(object sender, EventArgs e)
        {

        }
    }
}

