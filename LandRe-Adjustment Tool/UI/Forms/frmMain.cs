
using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Entities.LandData;
using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Entities.Spatial;
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
using Land_Readjustment_Tool.Services.Roads;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.UI.Dialogs;
using Land_Readjustment_Tool.UI.Forms;
using Land_Readjustment_Tool.UI.Forms.Definitions;
using Land_Readjustment_Tool.UI.Forms.Project;
using Land_Readjustment_Tool.UI.Helpers;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OSGeo.OSR;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Globalization;
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
        private readonly IExternalLayerImportService _externalLayerImportService;
        private readonly IProjectBoundaryAssignmentService _projectBoundaryAssignmentService;
        private readonly ICadastralRecordAssignmentService _cadastralRecordAssignmentService;
        private readonly IRoadCenterlineAssignmentService _roadCenterlineAssignmentService;
        private readonly IBlockAssignmentService _blockAssignmentService;
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
        private frmCanvasPerformanceOverlay? _canvasPerformanceOverlay;
        // Policy manager is launched as an independent top-level modeless window.
        private frmAreaConverter? _areaConverterForm;
        private readonly List<ToolStripItem> _projectScopedProfessionalMenuItems = new();
        private CanvasLayerTreeService? _layerTreeService;
        private string? _currentProjectRasterSrsDefinition;
        private bool _suppressLayerTreeEvents;
        private const int WmSetRedraw = 0x000B;
        private const int TvFirst = 0x1100;
        private const int TvmSetExtendedStyle = TvFirst + 44;
        private const int TvsExDoubleBuffer = 0x0004;
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
        private const string ExternalLayerGroupKey = CanvasLayerTreeService.ExternalGroupKey;
        private const string RasterLayerGroupKey = CanvasLayerTreeService.RasterGroupKey;
        private const int LayerNodeCheckBoxSize = 14;
        private const int LayerNodeCheckBoxGap = 10;
        private const int LayerNodeColorBoxSize = 18;
        private const int LayerNodeColorBoxGap = 4;
        private const int LayerNodeLockIconGap = 6;
        private const int CurrentDrawingLayerComboItemHeight = 24;
        private const int CurrentDrawingLayerComboPadding = 4;
        private const double ClosedPolylineTransferTolerance = 0.50;
        private static readonly JsonSerializerOptions CadastralMetadataJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
        private static readonly string PropertyFieldProfilesFilePath =
            Path.Combine(Application.UserAppDataPath, "property_field_profiles.json");
        private static readonly string[] DefaultObjectPropertyFieldKeys =
        [
            "selection.selectedObjects",
            "selection.objectType",
            "selection.layer",
            "selection.assignment"
        ];

        private static readonly string[] DefaultGeometryPropertyFieldKeys =
        [
            "geometry.type",
            "geometry.length",
            "geometry.area",
            "geometry.vertexCount",
            "geometry.bounds"
        ];

        private static readonly Dictionary<string, List<string>> DefaultPropertyFieldProfiles = new()
        {
            ["originalParcel"] =
            [
                .. DefaultObjectPropertyFieldKeys,
                "parcel.parcelNo", "parcel.mapSheetNo", "parcel.uniqueCode", "parcel.ownershipType",
                "owner.primaryOwner", "owner.coOwners",
                "area.originalRecord", "area.fieldMeasured",
                .. DefaultGeometryPropertyFieldKeys,
                "tenancy.hasTenant", "tenancy.tenantName"
            ],
            ["road"] =
            [
                .. DefaultObjectPropertyFieldKeys,
                "road.name", "road.code", "road.type", "road.width", "road.rightOfWay",
                .. DefaultGeometryPropertyFieldKeys
            ],
            ["block"] =
            [
                .. DefaultObjectPropertyFieldKeys,
                "block.name", "block.code", "block.landUse", "block.area", "block.depth", "block.depthGeometry", "block.length",
                .. DefaultGeometryPropertyFieldKeys
            ],
            ["replottedParcel"] =
            [
                .. DefaultObjectPropertyFieldKeys,
                "replotted.systemNumber", "replotted.derivedNumber", "replotted.blockSequence",
                "replotted.activeNumberType", "replotted.plotType", "replotted.block", "replotted.plotArea",
                .. DefaultGeometryPropertyFieldKeys
            ],
            ["text"] =
            [
                .. DefaultObjectPropertyFieldKeys,
                "text.value", "text.insertionPoint", "text.alignment"
            ],
            ["externalReference"] =
            [
                "selection.selectedObjects",
                "selection.objectType",
                "selection.layer",
                .. DefaultGeometryPropertyFieldKeys,
                "source.format",
                "source.file",
                "source.layer",
                "source.handle"
            ],
            ["general"] =
            [
                .. DefaultObjectPropertyFieldKeys,
                .. DefaultGeometryPropertyFieldKeys
            ]
        };

        private Dictionary<string, List<string>> _propertyFieldProfiles =
            DefaultPropertyFieldProfiles.ToDictionary(kv => kv.Key, kv => new List<string>(kv.Value));
        private IReadOnlyList<Guid> _currentSelectedCanvasObjectIds = Array.Empty<Guid>();
        private IReadOnlyList<CanvasObject> _currentPropertyGridObjects = Array.Empty<CanvasObject>();
        private int _currentPropertyGridSelectedCount;
        private bool _suppressSelectedPropertyObjectChanged;
        private readonly PersistedCanvasUndoRedoManager _canvasUndoManager = new();
        private readonly SemaphoreSlim _shapeEditSaveLock = new(1, 1);
        private bool _canvasUndoRedoOperationInProgress;
        private const string AllSelectedObjectsComboText = "All Selected objects";
        private const string VariesPropertyValue = "*VARIES*";
        private const int MaxPropertySelectionDetails = 20;
        private readonly ContextMenuStrip _layerContextMenu = new();

        private readonly ToolStripMenuItem _mnuZoomToLayer = new("Zoom To Layer");
        private readonly ToolStripMenuItem _mnuRenameLayer = new("Rename");
        private readonly ToolStripMenuItem _mnuDeleteLayer = new("Delete");
        private readonly ToolStripMenuItem _mnuMoveLayerUp = new("Shift Layer Up");
        private readonly ToolStripMenuItem _mnuMoveLayerDown = new("Shift Layer Down");
        private readonly ToolStripMenuItem _mnuToggleLayerVisibility = new("Hidden");
        private readonly ToolStripMenuItem _mnuToggleLayerLock = new("Locked");
        private readonly ToolStripMenuItem _mnuToggleLayerLabels = new("Show Labels");
        private readonly ToolStripMenuItem _mnuToggleFillTransparency = new("Show Transparency");
        private readonly ToolStripMenuItem _mnuLayerProperties = new("Layer Properties...");
        private readonly ToolStripMenuItem _mnuCreateFeaturesFromLayer = new("Create Features from Layer...");
        private readonly ToolStripMenuItem _mnuGenerateRoadParcel = new("Generate Road Parcel...");
        private readonly ToolStripMenuItem _mnuToggleLayerGroupLock = new("Locked");
        private readonly ToolStripMenuItem _mnuToggleLayerGroupSelection = new("Allow Selection");
        private readonly ToolStripMenuItem _mnuDeleteLayerGroup = new("Delete");
        private readonly ToolStripMenuItem _mnuDeleteExternalLayerGroup = new("Delete");
        private readonly ToolStripMenuItem _mnuAddDrawingLayer = new("Add Drawing Layer...");
        private readonly ToolStripMenuItem _mnuSetActiveLayer = new("Set Active Layer");
        private readonly ToolStripMenuItem _mnuAddRasterMap = new("Add Raster Map...");
        private readonly ToolStripMenuItem _mnuAddXyzTiles = new("Add XYZ Tiles...");
        private readonly ToolStripMenuItem _mnuAddExternalLayers = new("Add External Layers...");
        private readonly TextBox _layerRenameTextBox = new();
        private TreeNode? _contextLayerNode;
        private TreeNode? _contextLayerGroupNode;
        private TreeNode? _renamingLayerNode;
        private bool _isCompletingLayerRename;
        private frmXyzTileImportOptions? _xyzTileImportOptionsForm;
        private readonly ToolStripStatusLabel _projectCrsStatus = new();
        private readonly ToolStripStatusLabel _liveTileFetchStatus = new();
        private readonly System.Windows.Forms.Timer _liveTileFetchTimer = new();
        private readonly List<Image> _liveTileFetchFrames = new List<Image>();
        private Image? _liveTileStaticGlobe;
        private Image? _liveTileDisconnectedGlobe;
        private int _liveTileFetchFrameIndex;
        private bool _suppressCurrentDrawingLayerSelectionChanged;
        private bool _suppressCadastralAssignmentCanvasSelectionChanged;
        private frmCadastralRecordAssignment? _cadastralRecordAssignmentForm;
        private bool _suppressRoadAssignmentCanvasSelectionChanged;
        private frmRoadCenterlineAssignment? _roadCenterlineAssignmentForm;
        private bool _suppressBlockAssignmentCanvasSelectionChanged;
        private frmBlockAssignment? _blockAssignmentForm;
        private frmObjectTypeSelector? _blockSelectionForm;
        private frmObjectTypeSelector? _roadSelectionForm;
        private frmSelectByAttributes? _selectByAttributesForm;
        private CanvasLayer? _currentDrawingLayer;
        private MapCanvasTool _currentCanvasTool = MapCanvasTool.Select;
        private SelectionToolbarMethod _activeSelectionToolbarMethod = SelectionToolbarMethod.PointerWindow;
        private bool _isApplicationEditLocked;
        private const string ApplicationEditLockName = "Edit Lock";
        private static readonly string ProjectEditLockIconPath =
    @"C:\Users\CYBORG\source\repos\zen-developer33\Land-Readjustment-Tool\LandRe-Adjustment Tool\Resources\For RePlot Application\icons8-lock-26.png";

        private static readonly string ProjectEditUnlockIconPath =
            @"C:\Users\CYBORG\source\repos\zen-developer33\Land-Readjustment-Tool\LandRe-Adjustment Tool\Resources\For RePlot Application\icons8-padlock-26.png";

        private readonly Image _projectEditLockIcon = Image.FromFile(ProjectEditLockIconPath);
        private readonly Image _projectEditUnlockIcon = Image.FromFile(ProjectEditUnlockIconPath);
        private const float MapScreenshotExportScale = 2.0f;
        private const long MapScreenshotJpegQuality = 100L;

        private enum SelectionToolbarMethod
        {
            PointerWindow,
            Polygon,
            IntersectingPoly,
            IntersectingLine
        }

        private const string SelectionToolIconFolderName = "Selection Tools";
        private const string PointerWindowSelectionIconFileName = "pointer-window.png";
        private const string PolygonSelectionIconFileName = "selection-polygon.png";
        private const string IntersectingPolySelectionIconFileName = "selection-intersecting-poly.png";
        private const string IntersectingLineSelectionIconFileName = "selection-intersecting-line.png";

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

        private sealed record FeatureTransferTarget(CanvasLayer Layer, string GroupName);

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
                new ExternalLayerImportService(new ProjectRasterCrsResolver(projectScopedFactory)),
                new ProjectBoundaryAssignmentService(projectScopedFactory),
                new CadastralRecordAssignmentService(),
                new RoadCenterlineAssignmentService(),
                new BlockAssignmentService(),
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
            IExternalLayerImportService externalLayerImportService,
            IProjectBoundaryAssignmentService projectBoundaryAssignmentService,
            ICadastralRecordAssignmentService cadastralRecordAssignmentService,
            IRoadCenterlineAssignmentService roadCenterlineAssignmentService,
            IBlockAssignmentService blockAssignmentService,
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
            _externalLayerImportService = externalLayerImportService ?? throw new ArgumentNullException(nameof(externalLayerImportService));
            _projectBoundaryAssignmentService = projectBoundaryAssignmentService ?? throw new ArgumentNullException(nameof(projectBoundaryAssignmentService));
            _cadastralRecordAssignmentService = cadastralRecordAssignmentService ?? throw new ArgumentNullException(nameof(cadastralRecordAssignmentService));
            _roadCenterlineAssignmentService = roadCenterlineAssignmentService ?? throw new ArgumentNullException(nameof(roadCenterlineAssignmentService));
            _blockAssignmentService = blockAssignmentService ?? throw new ArgumentNullException(nameof(blockAssignmentService));
            _xyzTilePreDownloadService = new XyzTilePreDownloadService();
            _hatchPatternService = hatchPatternService ?? throw new ArgumentNullException(nameof(hatchPatternService));
            _projectOpenService = projectOpenService ?? throw new ArgumentNullException(nameof(projectOpenService));
            _projectSaveAsService = projectSaveAsService ?? throw new ArgumentNullException(nameof(projectSaveAsService));

            InitializeComponent();
            hostOperationProgress = hostProgressBarHost;
            NumericUpDownSelectAllBehavior.AttachTo(this);
            _startupFilePath = startupFilePath;
            LoadPropertyFieldProfiles();
            ConfigureSmoothSplitterLayout();
            ConfigureStatusStripSizing();
            ConfigureProjectCrsStatusIndicator();
            ConfigureLiveTileFetchStatusIndicator();
            XyzLiveTileRenderLayer.FetchStatusChanged += XyzLiveTileRenderLayer_FetchStatusChanged;
            FormClosed += frmMain_FormClosed;
            mapCanvasControlMain.StatusChanged += MapCanvasControlMain_StatusChanged;
            mapCanvasControlMain.CommandService.PromptChanged += prompt =>
            {
                if (!IsDisposed && !Disposing)
                    lblStatusMessage.Text = prompt;
            };
            mapCanvasControlMain.LongOperationProgressChanged += MapCanvasControlMain_LongOperationProgressChanged;
            mapCanvasControlMain.LongOperationProgressCompleted += MapCanvasControlMain_LongOperationProgressCompleted;
            mapCanvasControlMain.ShapeCompleted += MapCanvasControlMain_ShapeCompleted;
            mapCanvasControlMain.ShapesCompleted += MapCanvasControlMain_ShapesCompleted;
            mapCanvasControlMain.ShapesEdited += MapCanvasControlMain_ShapesEdited;
            mapCanvasControlMain.SelectToolRequested += ResetSelectionToolbarMethodToPointerWindow;
            mapCanvasControlMain.SelectionMethodRequested += MapCanvasControlMain_SelectionMethodRequested;
            mapCanvasControlMain.SelectedObjectsDeleteRequested += MapCanvasControlMain_SelectedObjectsDeleteRequested;
            mapCanvasControlMain.SelectedObjectsAssignDataRequested += MapCanvasControlMain_SelectedObjectsAssignDataRequested;
            mapCanvasControlMain.SelectedObjectsViewEditDataRequested += MapCanvasControlMain_SelectedObjectsViewEditDataRequested;
            mapCanvasControlMain.SelectedCanvasObjectsChanged += MapCanvasControlMain_SelectedCanvasObjectsChanged;
            mapCanvasControlMain.SelectedObjectsCreateFeaturesMenuOpening += MapCanvasControlMain_SelectedObjectsCreateFeaturesMenuOpening;
            mnuCanvasDebugOverlay.Checked = mapCanvasControlMain.ShowDebugOverlay;
            mnuOSnapToggle.Checked = mapCanvasControlMain.SnapEnabled;
            mnuOrthoToggle.Checked = mapCanvasControlMain.OrthoModeEnabled;
            ConfigureLayerTree();
            ConfigureLayerPropertiesPanel();
            MapCanvasControlMain_StatusChanged("E: --    N: --", "Ready", 0);


            UpdateWindowTitle();
            ConfigureProfessionalMenus();
            ConfigureApplicationEditLockUi();
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
            _canvasUndoManager.StateChanged += (_, _) => UpdateCanvasUndoRedoToolbar();
            mnuUndo.Click += async (_, _) => await UndoCanvasCommandAsync();
            mnuRedo.Click += async (_, _) => await RedoCanvasCommandAsync();
            mnuPan.CheckOnClick = true;
            mnuZoomIn.Click += mnuZoomIn_Click!;
            mnuZoomOut.Click += mnuZoomOut_Click!;
            mnuZoomExtent.Click += mnuZoomExtent_Click!;
            mnuZoomWindow.Click += mnuZoomWindow_Click!;
            baseMapsToolStripMenuItem.Click += importRasterToolStripMenuItem_Click!;
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Click += ImportProjectBoundaryToolStripMenuItem_Click!;
            projectBoundaryAssignmentToolStripMenuItem.Click += ProjectBoundaryAssignmentToolStripMenuItem_Click!;
            roadDataToolStripMenuItem.Click += RoadDataToolStripMenuItem_Click!;
            toolStripMenuItem2.Click += RoadAssignmentToolStripMenuItem_Click!;
            blockDataToolStripMenuItem.Click += BlockDataToolStripMenuItem_Click!;
            toolStripMenuItem1.Click += BlockAssignmentToolStripMenuItem_Click;
            mnuImportXyzTiles.Click += importXyzTilesToolStripMenuItem_Click!;
            mnuImportBlockLayoutPlan.Click += async (_, _) => await ShowImportBlockLayoutPlanWorkflowAsync();
            mnuImportExternalLayers.Click += async (_, _) => await ShowImportExternalLayersWorkflowAsync();
            originalScenarioSummaryToolStripMenuItem.Click += OriginalScenarioSummaryToolStripMenuItem_Click;
            btnOriginalScenarioSummary.Click += OriginalScenarioSummaryToolStripMenuItem_Click;
            mapRefreshLayersToolStripMenuItem.Click -= PlannedFeatureToolStripMenuItem_Click;
            mapRefreshLayersToolStripMenuItem.Click += mapRefreshLayersToolStripMenuItem_Click;
            mapLayerPropertiesToolStripMenuItem.Click -= PlannedFeatureToolStripMenuItem_Click;
            mapLayerPropertiesToolStripMenuItem.Click += mapLayerPropertiesToolStripMenuItem_Click;
            mapSelectPointerWindowToolStripMenuItem.Click += mapSelectPointerWindowToolStripMenuItem_Click;
            mapSelectPolygonToolStripMenuItem.Click += mapSelectPolygonToolStripMenuItem_Click;
            mapSelectIntersectPolyToolStripMenuItem.Click += mapSelectIntersectPolyToolStripMenuItem_Click;
            mapSelectIntersectLineToolStripMenuItem.Click += mapSelectIntersectLineToolStripMenuItem_Click;
            mnuSelectPointerWindow.Click += mapSelectPointerWindowToolStripMenuItem_Click;
            mnuSelectPolygon.Click += mapSelectPolygonToolStripMenuItem_Click;
            mnuSelectIntersectingPoly.Click += mapSelectIntersectPolyToolStripMenuItem_Click;
            mnuSelectIntersectingLine.Click += mapSelectIntersectLineToolStripMenuItem_Click;
            ConfigureSelectionToolImages();
            SetActiveSelectionToolbarMethod(SelectionToolbarMethod.PointerWindow);
            mapSelectProjectBoundaryToolStripMenuItem.Click += mapSelectProjectBoundaryToolStripMenuItem_Click;
            mapSelectBlocksToolStripMenuItem.Click += mapSelectBlocksToolStripMenuItem_Click;
            mapSelectRoadsToolStripMenuItem.Click += mapSelectRoadsToolStripMenuItem_Click;
            mapSelectByAttributesToolStripMenuItem.Click += mapSelectByAttributesToolStripMenuItem_Click;
            mapSelectByRecordsToolStripMenuItem.Click += btnSelectFromRecords_Click;
            contributionSettingsToolStripMenuItem.Click -= PolicyManagerToolStripMenuItem_Click;

            contributionSettingsToolStripMenuItem.Click += PolicyManagerToolStripMenuItem_Click;


        }
        private void ConfigureProfessionalMenus()
        {
            _projectScopedProfessionalMenuItems.Clear();

            _projectScopedProfessionalMenuItems.AddRange(new ToolStripItem[]
            {
                mnuProjectHealthCheck,
                mnuProjectLog,
                dataQualityToolStripMenuItem,
                ownerDeduplicationReviewToolStripMenuItem,
                parcelLinkMatchingReviewToolStripMenuItem,
                missingGeometryReviewToolStripMenuItem,
                mapSelectToolStripMenuItem,
                mapPanToolStripMenuItem,
                mapDrawToolStripMenuItem,
                mapDrawPointToolStripMenuItem,
                mapDrawLineToolStripMenuItem,
                mapDrawPolylineToolStripMenuItem,
                mapDrawArcToolStripMenuItem,
                mapDrawRectangleToolStripMenuItem,
                mapDrawPolygonToolStripMenuItem,
                mapDrawCircleToolStripMenuItem,
                mapDrawTextToolStripMenuItem,
                mapZoomInToolStripMenuItem,
                mapZoomOutToolStripMenuItem,
                mapZoomExtentsToolStripMenuItem,
                mapZoomWindowToolStripMenuItem,
                mapCaptureScreenshotToolStripMenuItem,
                mapRefreshLayersToolStripMenuItem,
                mapLayerPropertiesToolStripMenuItem,
                contributionSettingsToolStripMenuItem,

                parcelContributionInputsToolStripMenuItem,
                deriveInputsFromMapToolStripMenuItem,
                calculateContributionToolStripMenuItem,
                contributionReviewToolStripMenuItem,
                contributionOverridesAuditTrailToolStripMenuItem,
                freezeApproveResultsToolStripMenuItem,
                replotBlockManagerToolStripMenuItem,
                replotRoadLayoutToolStripMenuItem,
                ownerAllocationToolStripMenuItem,
                returnableAreaBalanceToolStripMenuItem,
                jointReturnSalesPlotCasesToolStripMenuItem,
                finalizeReplotDesignToolStripMenuItem,
                validationDashboardToolStripMenuItem,
                validateOwnershipRecordsToolStripMenuItem,
                validateSpatialDataToolStripMenuItem,
                topologyCheckToolStripMenuItem,
                contributionRuleCheckToolStripMenuItem,
                returnPolicyCheckToolStripMenuItem,
                areaAllocationDifferenceReviewToolStripMenuItem,
                validationIssuesToolStripMenuItem,
                ownerParcelReportToolStripMenuItem,
                originalParcelRegisterToolStripMenuItem,
                contributionSummaryReportToolStripMenuItem,
                contributionReportToolStripMenuItem,
                returnableAreaReportToolStripMenuItem,
                replotAllocationReportToolStripMenuItem,
                projectOverviewMapToolStripMenuItem,
                blockReplotMapToolStripMenuItem,
                contributionHeatmapToolStripMenuItem,
                exportExcelToolStripMenuItem,
                exportPdfToolStripMenuItem,
                exportDataToolStripMenuItem,
                openReplottingWorkspaceWindowToolStripMenuItem,
                resetWorkspaceLayoutToolStripMenuItem
            });
        }

        private void ConfigureApplicationEditLockUi()
        {
            btnApplicationEditLock.CheckOnClick = false;
            btnApplicationEditLock.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnApplicationEditLock.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            btnApplicationEditLock.Text = string.Empty;
            btnApplicationEditLock.AccessibleName = ApplicationEditLockName;

            mnuApplicationEditLock.CheckOnClick = false;
            mnuApplicationEditLock.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            mnuApplicationEditLock.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            mnuApplicationEditLock.Text = "Lock Edit";
            mnuApplicationEditLock.AccessibleName = ApplicationEditLockName;

            UpdateApplicationEditLockUi(forceNoProject: true);
        }

        private async void ApplicationEditLock_Click(object? sender, EventArgs e)
        {
            await ToggleApplicationEditLockAsync();
        }

        private void mapSelectToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ActivateCurrentSelectionToolbarMethod();
        }

        private void mapSelectPointerWindowToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ActivateSelectionToolbarMethod(SelectionToolbarMethod.PointerWindow);
        }

        private void mapSelectPolygonToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ActivateSelectionToolbarMethod(SelectionToolbarMethod.Polygon);
        }

        private void mapSelectIntersectPolyToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ActivateSelectionToolbarMethod(SelectionToolbarMethod.IntersectingPoly);
        }

        private void mapSelectIntersectLineToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ActivateSelectionToolbarMethod(SelectionToolbarMethod.IntersectingLine);
        }

        private async void mapSelectProjectBoundaryToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!EnsureProjectOpenForSelection("Select Project Boundary"))
                return;

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            Guid? boundaryId = await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.IsVisible &&
                    item.CanvasLayer.IsVisible &&
                    item.CanvasLayer.IsSelectable &&
                    (item.CanvasLayer.Name == "Project Boundary" ||
                     item.CanvasLayer.LayerType == "ProjectBoundary" ||
                     item.ObjectDescription == "Project Boundary"))
                .OrderBy(item => item.CreatedDate)
                .Select(item => (Guid?)item.Id)
                .FirstOrDefaultAsync();

            if (!boundaryId.HasValue)
            {
                MessageBox.Show(
                    this,
                    "No selectable Project Boundary object was found.",
                    "Select Project Boundary",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            mapCanvasControlMain.ApplyCanvasSelection([boundaryId.Value], CanvasSelectionApplyMode.Create, zoomToSelection: true);
            SetCanvasCommandStatus("Selected Project Boundary.");
        }

        private async void mapSelectBlocksToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!EnsureProjectOpenForSelection("Select Blocks"))
                return;

            if (_blockSelectionForm is { IsDisposed: false })
            {
                _blockSelectionForm.BringToFront();
                _blockSelectionForm.Focus();
                return;
            }

            frmObjectTypeSelector form = new("Select Blocks", await LoadBlockSelectorItemsAsync());
            _blockSelectionForm = form;
            form.SelectionRequested += ApplyObjectTypeSelection;
            form.FormClosed += (_, _) => _blockSelectionForm = null;
            PositionAssignmentFormAtCanvasTopLeft(form);
            form.Show(this);
        }

        private async void mapSelectRoadsToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!EnsureProjectOpenForSelection("Select Roads"))
                return;

            if (_roadSelectionForm is { IsDisposed: false })
            {
                _roadSelectionForm.BringToFront();
                _roadSelectionForm.Focus();
                return;
            }

            frmObjectTypeSelector form = new("Select Roads", await LoadRoadSelectorItemsAsync());
            _roadSelectionForm = form;
            form.SelectionRequested += ApplyObjectTypeSelection;
            form.FormClosed += (_, _) => _roadSelectionForm = null;
            PositionAssignmentFormAtCanvasTopLeft(form);
            form.Show(this);
        }

        private async void mapSelectByAttributesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!EnsureProjectOpenForSelection("Select By Attributes"))
                return;

            if (_selectByAttributesForm is { IsDisposed: false })
            {
                _selectByAttributesForm.BringToFront();
                _selectByAttributesForm.Focus();
                return;
            }

            frmSelectByAttributes form = new(await LoadSelectionAttributeLayersAsync());
            _selectByAttributesForm = form;
            form.SelectionRequested += (ids, mode, zoomToSelection) =>
            {
                int selectedCount = mapCanvasControlMain.ApplyCanvasSelection(ids, mode, zoomToSelection);
                SetCanvasCommandStatus($"Select By Attributes: {ids.Count:N0} matched, {selectedCount:N0} selected.");
                return selectedCount;
            };
            form.FormClosed += (_, _) => _selectByAttributesForm = null;
            PositionAssignmentFormAtCanvasTopLeft(form);
            form.Show(this);
        }

        private void mapSelectAllSelectableToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            mapCanvasControlMain.SelectAllSelectableObjects(zoomToSelection: false);
            SetCanvasCommandStatus("Selected all selectable visible objects.");
        }

        private void mapPanToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            mnuPan.PerformClick();
        }

        private async void mapRefreshLayersToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!AppServices.HasContext)
                return;

            await RefreshMapCanvasAsync("Refreshing layer list");
        }

        private async void mapLayerPropertiesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!AppServices.HasContext)
                return;

            TreeNode? layerNode = IsLayerNode(treeViewLayers.SelectedNode)
                ? treeViewLayers.SelectedNode
                : null;

            if (layerNode == null)
            {
                MessageBox.Show(
                    this,
                    "Select a layer in the Layers panel before opening layer properties.",
                    "Layer Properties",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            await OpenLayerPropertyManagerAsync(layerNode);
        }

        private void toggleLayerPanelToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            tsmExpandCollapseLeftPanel.PerformClick();
        }

        private void togglePropertiesPanelToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            tsmExpandCollapseRightPanel.PerformClick();
        }

        private static Bitmap CreateProjectEditLockIcon(bool locked)
        {
            Bitmap bitmap = new(48, 48, PixelFormat.Format32bppArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            Color strokeColor = locked
                ? Color.FromArgb(43, 92, 155)
                : Color.FromArgb(55, 126, 77);
            Color bodyColor = locked
                ? Color.FromArgb(248, 184, 62)
                : Color.FromArgb(122, 207, 142);
            Color shackleColor = Color.FromArgb(82, 95, 113);

            using Pen shacklePen = new(shackleColor, 5.0f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            if (locked)
            {
                graphics.DrawArc(shacklePen, 13, 7, 22, 28, 180, 180);
                graphics.DrawLine(shacklePen, 13, 21, 13, 27);
                graphics.DrawLine(shacklePen, 35, 21, 35, 27);
            }
            else
            {
                graphics.DrawArc(shacklePen, 11, 6, 24, 28, 205, 230);
                graphics.DrawLine(shacklePen, 33, 18, 42, 13);
                graphics.DrawLine(shacklePen, 13, 21, 13, 27);
            }

            RectangleF bodyRect = new(9, 22, 30, 21);
            using GraphicsPath bodyPath = CreateRoundedRectanglePath(bodyRect, 5.0f);
            using SolidBrush bodyBrush = new(bodyColor);
            using Pen bodyPen = new(strokeColor, 2.0f);
            graphics.FillPath(bodyBrush, bodyPath);
            graphics.DrawPath(bodyPen, bodyPath);

            using SolidBrush keyBrush = new(Color.FromArgb(43, 54, 68));
            graphics.FillEllipse(keyBrush, 21, 29, 6, 6);
            graphics.FillRectangle(keyBrush, 23, 34, 2, 5);

            return bitmap;
        }

        private static GraphicsPath CreateRoundedRectanglePath(RectangleF rectangle, float radius)
        {
            float diameter = radius * 2.0f;
            GraphicsPath path = new();
            path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void SetProjectScopedProfessionalMenuItems(bool enabled)
        {
            foreach (ToolStripItem item in _projectScopedProfessionalMenuItems)
            {
                item.Enabled = enabled;
            }
        }

        private async Task ToggleApplicationEditLockAsync()
        {
            if (!AppServices.HasContext)
            {
                return;
            }

            bool targetLocked = !_isApplicationEditLocked;
            if (!targetLocked)
            {
                DialogResult result = MessageBox.Show(
                    this,
                    "Unlock canvas and layer editing?\n\nCanvas objects, canvas layers, layer properties, and feature creation can be modified again.",
                    ApplicationEditLockName,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    UpdateApplicationEditLockUi();
                    return;
                }
            }

            await ApplyApplicationEditLockStateAsync(
                targetLocked,
                persist: true,
                showStatus: true);
        }

        private async Task ApplyApplicationEditLockStateAsync(
            bool locked,
            bool persist,
            bool showStatus)
        {
            _isApplicationEditLocked = locked;
            ApplyApplicationEditLockStateToUi();

            if (persist && AppServices.HasContext)
            {
                ProjectSettings? settings = await LoadProjectSettingsForUiStateAsync();
                if (settings != null && settings.ApplicationEditLocked != locked)
                {
                    settings.ApplicationEditLocked = locked;
                    settings.LastModifiedDate = DateTime.Now;
                    await SaveProjectSettingsForUiStateAsync(settings);
                    MarkProjectModifiedIfOpen();
                }
            }

            if (showStatus)
            {
                SetCanvasCommandStatus(locked
                    ? "Project Edit Lock enabled: canvas objects, canvas layers, layer properties, and feature creation are protected"
                    : "Project Edit Lock disabled: canvas and layer editing is enabled");
            }
        }

        private void ApplyApplicationEditLockStateToUi()
        {
            mapCanvasControlMain.ApplicationEditLocked = _isApplicationEditLocked;
            if (_workspaceCanvas != null && !_workspaceCanvas.IsDisposed)
                _workspaceCanvas.ApplicationEditLocked = _isApplicationEditLocked;
            if (_isApplicationEditLocked)
            {
                ActivateCanvasTool(MapCanvasTool.Select);
                mnuPan.Checked = false;
                CloseOpenEditingWorkflowForms();
            }

            UpdateApplicationEditLockUi();
            UpdateCanvasUndoRedoToolbar();
            PopulatePropertyGridForSelectedComboItem();
            SyncOpenPolicyManagerLockState();
        }

        private void CloseOpenEditingWorkflowForms()
        {
            CloseModelessForm(_cadastralRecordAssignmentForm);
            _cadastralRecordAssignmentForm = null;
            CloseModelessForm(_roadCenterlineAssignmentForm);
            _roadCenterlineAssignmentForm = null;
            CloseModelessForm(_blockAssignmentForm);
            _blockAssignmentForm = null;
        }

        private static void CloseModelessForm(Form? form)
        {
            if (form == null || form.IsDisposed)
                return;

            form.Close();
        }

        private void UpdateApplicationEditLockUi(bool forceNoProject = false)
        {
            bool hasProject = !forceNoProject && AppServices.HasContext;
            btnApplicationEditLock.Enabled = hasProject;
            mnuApplicationEditLock.Enabled = hasProject;
            btnApplicationEditLock.Checked = false;
            mnuApplicationEditLock.Checked = false;
            Image icon = _isApplicationEditLocked ? _projectEditLockIcon : _projectEditUnlockIcon;
            btnApplicationEditLock.Image = icon;
            mnuApplicationEditLock.Image = icon;
            btnApplicationEditLock.Text = string.Empty;
            mnuApplicationEditLock.Text = _isApplicationEditLocked
                ? "Unlock Edit"
                : "Lock Edit";
            btnApplicationEditLock.ToolTipText = _isApplicationEditLocked
                ? "Unlock Edit"
                : "Lock Edit";
            mnuApplicationEditLock.ToolTipText = btnApplicationEditLock.ToolTipText;

            bool canEdit = hasProject && !_isApplicationEditLocked;
            importDataToolStripMenuItem1.Enabled = hasProject;
            assignToolStripMenuItem.Enabled = hasProject;
            assignmentToolStripMenuItem.Enabled = hasProject;
            viewEditRecordToolStripMenuItem.Enabled = hasProject;
            landOwnerDataToolStripMenuItem.Enabled = hasProject;
            dataQualityToolStripMenuItem.Enabled = hasProject;
            ownerDeduplicationReviewToolStripMenuItem.Enabled = hasProject;
            parcelLinkMatchingReviewToolStripMenuItem.Enabled = hasProject;
            missingGeometryReviewToolStripMenuItem.Enabled = hasProject;
            SetDrawingToolButtonsEnabled(hasProject);
            SetCanvasEditToolButtonsEnabled(canEdit);
            cboCurrentDrawingLayer.Enabled = canEdit;
        }

        private void SetCanvasEditToolButtonsEnabled(bool enabled)
        {
            mnuDrawPoint.Enabled = enabled;
            mnuDrawLine.Enabled = enabled;
            mnuDrawPolyline.Enabled = enabled;
            mnuDrawPolygon.Enabled = enabled;
            mnuDrawRectangle.Enabled = enabled;
            mnuDrawCircle.Enabled = enabled;
            mnuDrawArc.Enabled = enabled;
            mnuDrawText.Enabled = enabled;
            lblCurrentDrawingLayer.Enabled = enabled;
            mapDrawToolStripMenuItem.Enabled = enabled;
            mapDrawPointToolStripMenuItem.Enabled = enabled;
            mapDrawLineToolStripMenuItem.Enabled = enabled;
            mapDrawPolylineToolStripMenuItem.Enabled = enabled;
            mapDrawPolygonToolStripMenuItem.Enabled = enabled;
            mapDrawRectangleToolStripMenuItem.Enabled = enabled;
            mapDrawCircleToolStripMenuItem.Enabled = enabled;
            mapDrawArcToolStripMenuItem.Enabled = enabled;
            mapDrawTextToolStripMenuItem.Enabled = enabled;
        }

        private void PlannedFeatureToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            string featureName = sender is ToolStripMenuItem item
                ? (item.Text ?? "This feature").Replace("&", string.Empty)
                : "This feature";

            MessageBox.Show(
                $"{featureName} is part of the planned land contribution and replotting workflow, but its screen has not been implemented yet.",
                "Feature Planned",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void PolicyManagerToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            OpenPolicyManager();
        }

        private void OpenPolicyManager()
        {
            if (!AppServices.HasContext)
                return;

            // The policy manager lives in the Land Pooling Policy Manager
            // project. It is intentionally launched modeless and without owner
            // coupling so minimizing/maximizing it does not affect RePlot.
            string lppPath = AppServices.Context.Session.ProjectFilePath;
            // valueOnlyEditMode = false so an unlocked project can fully draft,
            // edit, add, and delete policy sections, clauses, parameters, and
            // lookup rows. The readOnly flag below still locks everything when
            // the main RePlot edit lock is enabled.
            Land_Pooling_Policy_Manager.PolicyManagerLauncher.Show(
                owner: null,
                sqliteDbPath: lppPath,
                readOnly: _isApplicationEditLocked,
                valueOnlyEditMode: false);
        }

        private void SyncOpenPolicyManagerLockState()
        {
            if (!AppServices.HasContext)
                return;

            Land_Pooling_Policy_Manager.PolicyManagerLauncher.SetReadOnlyMode(
                AppServices.Context.Session.ProjectFilePath,
                _isApplicationEditLocked);
        }

        private bool EnsureApplicationUnlockedForEditing(string action)
        {
            if (!_isApplicationEditLocked)
                return true;

            SetCanvasCommandStatus($"Project Edit Lock active: unlock canvas/layer editing before {action}");
            return false;
        }

        private void ConfigureStatusStripSizing()
        {
            lblProjectName.AutoSize = false;
            lblProjectName.Width = 270;
            lblActiveTool.AutoSize = false;
            lblActiveTool.Width = 185;
            lblStatusMessage.AutoSize = false;
            lblStatusMessage.Width = 720;
            lblStatusMessage.Spring = true;
            lblStatusMessage.Overflow = ToolStripItemOverflow.Never;
            lblScale.AutoSize = false;
            lblScale.Width = 132;
            lblCanvasCoordinates.AutoSize = false;
            lblCanvasCoordinates.Width = 285;
            lblScale.TextAlign = ContentAlignment.MiddleRight;
            lblCanvasCoordinates.TextAlign = ContentAlignment.MiddleRight;
            lblOperationProgressStatus.Visible = false;
            hostProgressBarHost.Visible = false;
        }

        private void ConfigureProjectCrsStatusIndicator()
        {
            _projectCrsStatus.Name = "projectCrsStatus";
            _projectCrsStatus.Alignment = ToolStripItemAlignment.Right;
            _projectCrsStatus.AutoSize = false;
            _projectCrsStatus.Size = new Size(88, 33);
            _projectCrsStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _projectCrsStatus.BorderStyle = Border3DStyle.RaisedOuter;
            _projectCrsStatus.ForeColor = SystemColors.ControlText;
            _projectCrsStatus.Margin = new Padding(0, 3, 2, 2);
            _projectCrsStatus.Text = "--";
            _projectCrsStatus.TextAlign = ContentAlignment.MiddleCenter;
            _projectCrsStatus.ToolTipText = "Project coordinate system";

            PlaceCoordinateStatusItemsLeftOfCoordinates();
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

            PlaceCoordinateStatusItemsLeftOfCoordinates();

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

        private async void mnuDrawText_Click(object sender, EventArgs e)
        {
            await ActivateCanvasDrawingToolAsync(MapCanvasTool.Text);
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

        private void PlaceCoordinateStatusItemsLeftOfCoordinates()
        {
            if (statusCanvas.Items.Contains(_liveTileFetchStatus))
            {
                statusCanvas.Items.Remove(_liveTileFetchStatus);
            }

            if (statusCanvas.Items.Contains(_projectCrsStatus))
            {
                statusCanvas.Items.Remove(_projectCrsStatus);
            }

            int coordinateIndex = statusCanvas.Items.IndexOf(lblCanvasCoordinates);
            if (coordinateIndex >= 0)
            {
                // Right-aligned StatusStrip items are arranged right-to-left.
                // Items inserted after the coordinate item render to its left.
                statusCanvas.Items.Insert(coordinateIndex + 1, _projectCrsStatus);
                statusCanvas.Items.Insert(coordinateIndex + 2, _liveTileFetchStatus);
                return;
            }

            statusCanvas.Items.Add(_projectCrsStatus);
            statusCanvas.Items.Add(_liveTileFetchStatus);
        }

        private async Task UpdateProjectCrsStatusAsync(ProjectSettings settings)
        {
            if (!settings.CoordinateSystemId.HasValue)
            {
                SetProjectCrsStatusCode(null);
                return;
            }

            string? code = settings.CoordinateSystem?.Code;
            if (string.IsNullOrWhiteSpace(code) && AppServices.HasContext)
            {
                try
                {
                    var crsRepository = _projectScopedFactory
                        .CreateCoordinateSystemRepository(AppServices.Context.Session);
                    CoordinateSystem? crs = await crsRepository
                        .GetWithParametersAsync(settings.CoordinateSystemId.Value);
                    code = crs?.Code;
                }
                catch (Exception ex)
                {
                    LogProjectError(
                        $"Failed to load project coordinate system. Id={settings.CoordinateSystemId.Value}",
                        ex);
                }
            }

            SetProjectCrsStatusCode(code);
        }

        private void SetProjectCrsStatusCode(string? code)
        {
            string display = string.IsNullOrWhiteSpace(code)
                ? "--"
                : code.Trim();

            _projectCrsStatus.Text = display;
            _projectCrsStatus.ToolTipText = display == "--"
                ? "No project coordinate system"
                : $"Project coordinate system: {display}";
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
            EnableDoubleBuffering(grpLayer);
            EnableDoubleBuffering(treeViewLayers);
            EnableDoubleBuffering(grpParcelObjProp);
            EnableDoubleBuffering(dgvParcelObjProperty);
            EnableNativeTreeViewDoubleBuffering(treeViewLayers);


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

        private static void EnableNativeTreeViewDoubleBuffering(TreeView treeView)
        {
            if (SystemInformation.TerminalServerSession)
            {
                return;
            }

            if (!treeView.IsHandleCreated)
            {
                treeView.HandleCreated += (_, _) => EnableNativeTreeViewDoubleBuffering(treeView);
                return;
            }

            SendMessage(
                treeView.Handle,
                TvmSetExtendedStyle,
                new IntPtr(TvsExDoubleBuffer),
                new IntPtr(TvsExDoubleBuffer));
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam);

        private ControlRedrawScope SuspendRedraw(params Control?[] controls)
        {
            return new ControlRedrawScope(controls);
        }

        private sealed class ControlRedrawScope : IDisposable
        {
            private readonly List<Control> _controls;
            private bool _disposed;

            public ControlRedrawScope(IEnumerable<Control?> controls)
            {
                _controls = controls
                    .Where(control => control != null &&
                                      !control.IsDisposed &&
                                      control.IsHandleCreated)
                    .Cast<Control>()
                    .Distinct()
                    .ToList();

                foreach (Control control in _controls)
                {
                    control.SuspendLayout();
                    SendMessage(control.Handle, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                for (int i = _controls.Count - 1; i >= 0; i--)
                {
                    Control control = _controls[i];
                    if (control.IsDisposed || !control.IsHandleCreated)
                        continue;

                    SendMessage(control.Handle, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                    control.ResumeLayout(false);
                    control.Invalidate(true);
                }
            }
        }


        private void InitializeProjectWorkspace(bool showWorkspace = true)
        {
            mainSplitContainer.Visible = showWorkspace;
            ClearCanvasUndoRedoHistory();
            EnableProjectMenuItems();
            _layerTreeService = AppServices.HasContext
                ? _projectScopedFactory.CreateCanvasLayerTreeService(AppServices.Context.Session)
                : null;
        }

        private void ShowProjectWorkspace()
        {
            if (!mainSplitContainer.Visible)
            {
                mainSplitContainer.Visible = true;
            }

            // Reveal canvas content only now that the saved viewport and layer caches are ready,
            // so the default coordinate grid is never flashed during project loading.
            mapCanvasControlMain.ShowCanvasContent();
        }

        private void UnloadProjectWorkspace()
        {
            CloseReplotWorkspace();
            mainSplitContainer.Visible = false;
            // Re-arm the load gate so the next project that opens does not flash the default grid
            // before its saved canvas state is restored.
            mapCanvasControlMain.SuppressCanvasContentUntilReady();
            ClearCanvasUndoRedoHistory();
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
                SetProjectCrsStatusCode(null);
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
                Land_Pooling_Policy_Manager.PolicyManagerLauncher.Close(
                    AppServices.Context.Session.ProjectFilePath);

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
            mapToolStripMenuItem.Enabled = true;
            contributionToolStripMenuItem.Enabled = true;
            replottingToolStripMenuItem.Enabled = true;
            validationToolStripMenuItem.Enabled = true;
            reportsToolStripMenuItem.Enabled = true;
            SetProjectScopedProfessionalMenuItems(true);

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
            originalScenarioSummaryToolStripMenuItem.Enabled = true;
            btnOriginalScenarioSummary.Enabled = true;
            startReplotWorkspaceToolStripMenuItem.Enabled = true;
            mnuUndo.Enabled = false;
            mnuRedo.Enabled = false;
            mnuPan.Enabled = true;
            mnuZoomIn.Enabled = true;
            mnuZoomOut.Enabled = true;
            mnuZoomExtent.Enabled = true;
            mnuZoomWindow.Enabled = true;
            SetDrawingToolButtonsEnabled(true);
            cboCurrentDrawingLayer.Enabled = true;
            UpdateCanvasUndoRedoToolbar();
            UpdateApplicationEditLockUi();
        }

        /// <summary>
        /// Disables project-scoped commands when no project is active, preventing forms from opening without a project database.
        /// </summary>
        private void DisableProjectMenuItems()
        {
            _isApplicationEditLocked = false;
            mapCanvasControlMain.ApplicationEditLocked = false;
            if (_workspaceCanvas != null && !_workspaceCanvas.IsDisposed)
                _workspaceCanvas.ApplicationEditLocked = false;

            dataToolStripMenuItem.Enabled = false;
            mapToolStripMenuItem.Enabled = false;
            contributionToolStripMenuItem.Enabled = false;
            replottingToolStripMenuItem.Enabled = false;
            validationToolStripMenuItem.Enabled = false;
            reportsToolStripMenuItem.Enabled = false;
            SetProjectScopedProfessionalMenuItems(false);

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
            originalScenarioSummaryToolStripMenuItem.Enabled = false;
            btnOriginalScenarioSummary.Enabled = false;
            startReplotWorkspaceToolStripMenuItem.Enabled = false;
            mnuUndo.Enabled = false;
            mnuRedo.Enabled = false;
            mnuPan.Enabled = false;
            mnuZoomIn.Enabled = false;
            mnuZoomOut.Enabled = false;
            mnuZoomExtent.Enabled = false;
            mnuZoomWindow.Enabled = false;
            SetDrawingToolButtonsEnabled(false);
            cboCurrentDrawingLayer.Enabled = false;
            UpdateCanvasUndoRedoToolbar();
            UpdateApplicationEditLockUi();
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
            mnuDrawText.Enabled = enabled;
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

                using (mapCanvasControlMain.DeferCacheRefreshes())
                {
                    await ApplySettingsAsync(showRefreshProgress: false);
                    await RefreshMapCanvasAsync("Opening project canvas", 75);
                }
                // Reveal the canvas only after the initial refresh, so the empty grid is not
                // shown while the new project workspace is still being built.
                mapCanvasControlMain.ShowCanvasContent();
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

                if (settings == null)
                {
                    SetProjectCrsStatusCode(null);
                    return;
                }

                await UpdateProjectCrsStatusAsync(settings);

                await ApplyApplicationEditLockStateAsync(
                    settings.ApplicationEditLocked,
                    persist: false,
                    showStatus: false);

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
                mapCanvasControlMain.UpdateAreaPrecisionSettings(
                    settings.AreaSqmDecimalPlaces,
                    settings.TraditionalAreaLowestUnitDecimalPlaces);

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
                    _workspaceCanvas.UpdateAreaPrecisionSettings(
                        settings.AreaSqmDecimalPlaces,
                        settings.TraditionalAreaLowestUnitDecimalPlaces);
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

        private async void ImportParcelOwnerShipRecords_Click(object sender, EventArgs e)
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

            if (!EnsureApplicationUnlockedForEditing("importing parcel ownership records"))
                return;

            var persistenceService = _projectScopedFactory.CreateImportPersistenceService(
                AppServices.Context.Session);
            using var frm = new frmImportParcelOwnershipRecords(
                AppServices.Context.ProjectFilePath,
                persistenceService);

            if (frm.ShowDialog(this) == DialogResult.OK)
            {
                AppServices.Context.MarkAsModified();
                await RefreshVectorCanvasFeaturesAsync();
                await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
                mapCanvasControlMain.RequestRender();
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
                AppServices.Context.ProjectFilePath,
                readOnlyMode: _isApplicationEditLocked);
            frm.ShowDialog(this);
        }

        private void OwnerRegisterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EnsureProjectOpenForReport("Owner Register"))
                return;

            var landRecordsService = _projectScopedFactory.CreateLandRecordsService(
                AppServices.Context.Session,
                AppServices.Context.ProjectFilePath);

            using var frm = new frmLandOwnersRecord(
                landRecordsService,
                AppServices.Context.ProjectFilePath,
                readOnlyMode: true);
            frm.ConfigureAsRegister("Owner Register");
            frm.ShowDialog(this);
        }

        private void OriginalParcelRegisterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EnsureProjectOpenForReport("Original Parcel Register"))
                return;

            var landRecordsService = _projectScopedFactory.CreateLandRecordsService(
                AppServices.Context.Session,
                AppServices.Context.ProjectFilePath);

            using var frm = new frmLandParcelOwnersRecord(
                landRecordsService,
                AppServices.Context.ProjectFilePath,
                readOnlyMode: true);
            frm.ConfigureAsRegister("Original Parcel Register");
            frm.ShowDialog(this);
        }

        private static bool EnsureProjectOpenForReport(string title)
        {
            if (AppServices.HasContext)
                return true;

            MessageBox.Show(
                "Please open or create a project before opening this register.",
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private void OriginalScenarioSummaryToolStripMenuItem_Click(object? sender, EventArgs e)
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

            using var frm = new frmOriginalScenarioSummary(
                AppServices.Context.Session,
                AppServices.Context.ProjectFilePath);
            frm.ShowDialog(this);
        }

        private async void OwnerDeduplicationReviewToolStripMenuItem_Click(object? sender, EventArgs e)
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
                var service = new DataQualityReviewService(AppServices.Context.Session);
                DataQualityReviewService.OwnerReviewRecordSet recordSet =
                    await service.LoadOwnerReviewRecordsAsync();
                if (recordSet.Records.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "No original parcel owner records are available to review.",
                        "Owner Deduplication Review",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                OwnerDeduplicationService.DeduplicationResult result =
                    OwnerDeduplicationService.ExtractUniqueOwners(
                        recordSet.Records.ToList(),
                        excludeAnonymous: true);
                if (result.DuplicatesNeedingReview.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "No duplicate owner groups were found in the saved original parcel records.",
                        "Owner Deduplication Review",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                using var form = new frmReviewDuplicates(
                    result,
                    recordSet.Records,
                    readOnlyMode: _isApplicationEditLocked);
                if (form.ShowDialog(this) != DialogResult.OK || !form.ChangesWereMade)
                    return;

                DataQualityReviewService.OwnerReviewPersistenceResult saveResult =
                    await service.SaveOwnerReviewRecordsAsync(recordSet);
                MarkProjectModifiedIfOpen();
                await RefreshVectorCanvasFeaturesAsync();
                await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Owner deduplication review applied.");

                MessageBox.Show(
                    this,
                    $"Owner review applied.\n\nParcels updated: {saveResult.ParcelsUpdated:N0}\nOwners created: {saveResult.OwnersCreated:N0}\nOwners updated: {saveResult.OwnersUpdated:N0}\nUnused duplicate owners removed: {saveResult.OrphanOwnersRemoved:N0}",
                    "Owner Deduplication Review",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogProjectError("Owner deduplication review failed.", ex);
                MessageBox.Show(
                    this,
                    $"Owner deduplication review failed: {GetMostUsefulExceptionMessage(ex)}",
                    "Owner Deduplication Review",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ParcelLinkMatchingReviewToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            CadastralRecordsAssignmentToolStripMenuItem_Click(sender, e);
        }

        private async void MissingGeometryReviewToolStripMenuItem_Click(object? sender, EventArgs e)
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

            bool openParcelLinkReview = false;
            bool geometryLinksChanged = false;
            using var form = new frmMissingGeometryReview(
                new DataQualityReviewService(AppServices.Context.Session),
                readOnlyMode: _isApplicationEditLocked);
            form.OpenParcelLinkReviewRequested += () =>
            {
                openParcelLinkReview = true;
                form.Close();
            };
            form.GeometryLinksChanged += () => geometryLinksChanged = true;
            form.ShowDialog(this);

            if (geometryLinksChanged)
            {
                MarkProjectModifiedIfOpen();
                await RefreshVectorCanvasFeaturesAsync();
                await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Missing geometry review updated parcel links.");
            }

            if (openParcelLinkReview)
                CadastralRecordsAssignmentToolStripMenuItem_Click(sender, e);
        }

        private async void importRasterToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!EnsureApplicationUnlockedForEditing("importing raster layers"))
                return;

            await ImportRasterFileAsync(
                "Import Raster",
                GetGeneralRasterImportFilter());
        }

        private async void importXyzTilesToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!EnsureApplicationUnlockedForEditing("importing XYZ tile layers"))
                return;

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

        private async Task ShowImportBlockLayoutPlanWorkflowAsync()
        {
            if (!EnsureApplicationUnlockedForEditing("importing block layout plans"))
                return;

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
                Title = "Import Block Layout Plan",
                Filter = "Block layout plan files (*.dxf;*.dwg)|*.dxf;*.dwg|DXF files (*.dxf)|*.dxf|DWG files (*.dwg)|*.dwg|All files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                SetOperationProgress(5, "Inspecting block layout plan...", showProgressForm: false);

                if (_layerTreeService != null)
                    await _layerTreeService.GetLayerTreeAsync();

                ProjectRasterCrsContext projectCrs =
                    await _projectRasterCrsResolver.ResolveAsync(AppServices.Context.Session);
                ExternalLayerFileInfo fileInfo =
                    _externalLayerImportService.Inspect(dialog.FileName);

                if (fileInfo.Layers.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "No importable layers were found in the selected block layout plan.",
                        "Block Layout Plan Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string sourceCrs = string.IsNullOrWhiteSpace(fileInfo.DetectedCrsCode)
                    ? projectCrs.TargetSrsDefinition
                    : fileInfo.DetectedCrsCode;
                string projectCrsLabel =
                    $"{projectCrs.CoordinateSystem.Code} - {projectCrs.CoordinateSystem.Name}";
                IReadOnlyList<frmBlockLayoutPlanImport.CrsChoice> sourceCrsChoices =
                    fileInfo.RequiresCrsFromUser
                        ? await BuildBlockLayoutSourceCrsChoicesAsync(projectCrs)
                        : [];

                using frmBlockLayoutPlanImport importForm = new(
                    fileInfo,
                    sourceCrs,
                    projectCrsLabel,
                    sourceCrsChoices);
                if (importForm.ShowDialog(this) != DialogResult.OK)
                    return;

                ExternalLayerImportOptions importOptions = importForm.ImportOptions;
                bool? replaceExistingTargetObjects =
                    await ResolveBlockLayoutTargetReplacementAsync(importOptions);
                if (!replaceExistingTargetObjects.HasValue)
                    return;

                importOptions = importOptions with
                {
                    ReplaceExistingTargetLayerObjects = replaceExistingTargetObjects.Value
                };

                DialogResult autoDefineChoice = MessageBox.Show(
                    this,
                    "Auto-define roads and blocks from imported layers?",
                    "Block Layout Plan Import",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                if (autoDefineChoice == DialogResult.Cancel)
                    return;

                bool autoDefineLayoutData = autoDefineChoice == DialogResult.Yes;
                bool replaceMatchingRoadDefinitions = false;
                if (autoDefineLayoutData)
                {
                    replaceMatchingRoadDefinitions = MessageBox.Show(
                        this,
                        "Replace matching existing road definitions?",
                        "Road Definitions",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes;
                }

                if (replaceExistingTargetObjects.Value)
                {
                    SetOperationProgress(25, "Replacing existing block layout objects...", showProgressForm: false);
                    await ClearExistingBlockLayoutTargetObjectsAsync(importOptions);
                }

                SetOperationProgress(30, "Importing block layout plan...", showProgressForm: false);
                ExternalLayerImportResult result =
                    await _externalLayerImportService.ImportAsync(
                        AppServices.Context.Session,
                        dialog.FileName,
                        importOptions,
                        PromptImportDuplicateGeometryChoice,
                        PromptImportProjectBoundaryChoice);

                if (!result.Success)
                {
                    MessageBox.Show(
                        this,
                        result.ErrorMessage ?? "Block layout plan import failed.",
                        "Block Layout Plan Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                List<string> importWarnings = result.Warnings?.ToList() ?? [];
                BlockLayoutPlanAutoDefinitionResult? autoDefinitionResult = null;
                if (autoDefineLayoutData)
                {
                    SetOperationProgress(76, "Defining roads and blocks...", showProgressForm: false);
                    autoDefinitionResult = await new BlockLayoutPlanAutoDefinitionService().ApplyAsync(
                        AppServices.Context.Session,
                        result.ImportedObjectIds ?? [],
                        importOptions.BlockLabelLayerName,
                        replaceMatchingRoadDefinitions);
                }

                DialogResult generateRoadParcelChoice = MessageBox.Show(
                    this,
                    "Generate Road Parcel now?",
                    "Block Layout Plan Import",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (generateRoadParcelChoice == DialogResult.Yes)
                {
                    try
                    {
                        SetOperationProgress(84, "Generating road parcel...", showProgressForm: false);
                        BlockRoadParcelRefreshResult roadParcelRefresh =
                            await RefreshGeneratedRoadParcelAsync(AppServices.Context.Session.GetDbContext());
                        if (!roadParcelRefresh.Created &&
                            !string.IsNullOrWhiteSpace(roadParcelRefresh.SkippedReason))
                        {
                            importWarnings.Add(
                                $"Road Parcel was not generated: {roadParcelRefresh.SkippedReason}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogProjectError("Road parcel generation failed after block layout import.", ex);
                        importWarnings.Add(
                            $"Road Parcel was not generated: {GetMostUsefulExceptionMessage(ex)}");
                    }
                }

                MarkProjectModifiedIfOpen();
                await ApplyCurrentCanvasThemeToLayerColorsAsync();
                SetOperationProgress(92, "Refreshing block layout layers...", showProgressForm: false);
                await RefreshLayerTreeAsync();

                if (result.BoundingBox != null && !result.BoundingBox.IsNull)
                    mapCanvasControlMain.ZoomToWorldBounds(ToRectangleD(result.BoundingBox));

                string autoStatus = autoDefinitionResult == null
                    ? string.Empty
                    : $" Auto-defined {autoDefinitionResult.RoadsDefined} road(s), {autoDefinitionResult.BlocksDefined} block(s); assigned {autoDefinitionResult.RoadsAssigned} road object(s), {autoDefinitionResult.BlocksAssigned} block object(s).";
                SetCanvasCommandStatus(
                    $"Imported {result.ObjectsCreated} block layout object(s) into {result.LayersCreated} layer(s).{autoStatus}");

                if (importWarnings.Count > 0)
                {
                    MessageBox.Show(
                        this,
                        $"Imported {result.ObjectsCreated} object(s) into {result.LayersCreated} layer(s).\r\n\r\n" +
                        "Some import decisions need your attention:\r\n\r\n" +
                        string.Join("\r\n", importWarnings),
                        "Block Layout Plan Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    ex.Message,
                    "Block Layout Plan Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                LogProjectError("Block layout plan import failed.", ex);
                MessageBox.Show(
                    $"Failed to import block layout plan: {GetMostUsefulExceptionMessage(ex)}",
                    "Block Layout Plan Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private async Task<IReadOnlyList<frmBlockLayoutPlanImport.CrsChoice>> BuildBlockLayoutSourceCrsChoicesAsync(
            ProjectRasterCrsContext projectCrs)
        {
            List<frmBlockLayoutPlanImport.CrsChoice> choices =
            [
                new($"Project CRS ({projectCrs.CoordinateSystem.Code})", projectCrs.TargetSrsDefinition),
                new("WGS 84 / UTM 44N (EPSG:32644)", "EPSG:32644"),
                new("WGS 84 / UTM 45N (EPSG:32645)", "EPSG:32645"),
                new("WGS 84 (EPSG:4326)", "EPSG:4326")
            ];

            var crsRepository =
                _projectScopedFactory.CreateCoordinateSystemRepository(AppServices.Context.Session);
            IReadOnlyList<CoordinateSystem> activeCrs =
                await crsRepository.GetAllActiveAsync();

            foreach (CoordinateSystem crs in activeCrs.OrderBy(item => item.DisplayOrder).ThenBy(item => item.Name))
            {
                if (choices.Any(item => item.Label.Contains(crs.Code, StringComparison.OrdinalIgnoreCase)))
                    continue;

                string? definition = await TryBuildBlockLayoutSourceCrsDefinitionAsync(crs, projectCrs);
                if (string.IsNullOrWhiteSpace(definition))
                    continue;

                choices.Add(new frmBlockLayoutPlanImport.CrsChoice(
                    $"{crs.Code} - {crs.Name}",
                    definition));
            }

            return choices;
        }

        private async Task<bool?> ResolveBlockLayoutTargetReplacementAsync(
            ExternalLayerImportOptions importOptions)
        {
            if (!AppServices.HasContext)
                return false;

            string[] targetLayerNames = importOptions.Layers
                .Where(option => option.Include &&
                                 !string.IsNullOrWhiteSpace(option.TargetLayerName))
                .Select(option => option.TargetLayerName!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (targetLayerNames.Length == 0)
                return false;

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<CanvasLayer> targetLayers = (await context.CanvasLayers
                    .AsNoTracking()
                    .ToListAsync())
                .Where(layer => targetLayerNames.Contains(layer.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();
            if (targetLayers.Count == 0)
                return false;

            int[] targetLayerIds = targetLayers
                .Select(layer => layer.Id)
                .ToArray();
            int existingObjectCount = await context.CanvasObjects
                .AsNoTracking()
                .CountAsync(item => targetLayerIds.Contains(item.CanvasLayerId));
            if (existingObjectCount == 0)
                return false;

            string layerList = string.Join(
                ", ",
                targetLayers
                    .OrderBy(layer => layer.Name)
                    .Select(layer => layer.Name)
                    .Take(5));
            if (targetLayers.Count > 5)
                layerList += $", +{targetLayers.Count - 5:N0} more";

            DialogResult choice = MessageBox.Show(
                this,
                $"Selected target layer(s) already contain {existingObjectCount:N0} object(s): {layerList}.\r\n\r\n" +
                "Replace existing objects and import again?",
                "Replace Existing Block Layout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            return choice == DialogResult.Yes ? true : null;
        }

        private async Task<int> ClearExistingBlockLayoutTargetObjectsAsync(
            ExternalLayerImportOptions importOptions)
        {
            if (!AppServices.HasContext)
                return 0;

            string[] targetLayerNames = importOptions.Layers
                .Where(option => option.Include &&
                                 !string.IsNullOrWhiteSpace(option.TargetLayerName))
                .Select(option => option.TargetLayerName!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (targetLayerNames.Length == 0)
                return 0;

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<int> targetLayerIds = (await context.CanvasLayers
                    .AsNoTracking()
                    .ToListAsync())
                .Where(layer => targetLayerNames.Contains(layer.Name, StringComparer.OrdinalIgnoreCase))
                .Select(layer => layer.Id)
                .ToList();
            if (targetLayerIds.Count == 0)
                return 0;

            List<CanvasObject> existingObjects = await context.CanvasObjects
                .Where(item => targetLayerIds.Contains(item.CanvasLayerId))
                .ToListAsync();
            if (existingObjects.Count == 0)
                return 0;

            context.CanvasObjects.RemoveRange(existingObjects);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            return existingObjects.Count;
        }

        private async Task<string?> TryBuildBlockLayoutSourceCrsDefinitionAsync(
            CoordinateSystem crs,
            ProjectRasterCrsContext projectCrs)
        {
            if (crs.EpsgCode.HasValue)
                return $"EPSG:{crs.EpsgCode.Value}";

            try
            {
                var crsRepository =
                    _projectScopedFactory.CreateCoordinateSystemRepository(AppServices.Context.Session);
                CoordinateSystem? withParameters =
                    await crsRepository.GetWithParametersAsync(crs.Id);

                if (withParameters?.ProjectionParameters == null)
                    return null;

                return ProjectCrsWktBuilder.BuildTargetSrsDefinition(
                    withParameters,
                    projectCrs.DatumTransformation);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Asks the user how to resolve incoming import objects whose geometry already exists in
        /// their target layer. A layer keeps at most one object per geometry, so the choice is to
        /// replace the existing object(s), skip the incoming duplicate(s), or cancel the import.
        /// </summary>
        private ImportDuplicateGeometryChoice PromptImportDuplicateGeometryChoice(int duplicateCount)
        {
            if (InvokeRequired)
            {
                return (ImportDuplicateGeometryChoice)Invoke(
                    new Func<int, ImportDuplicateGeometryChoice>(PromptImportDuplicateGeometryChoice),
                    duplicateCount);
            }

            DialogResult result = MessageBox.Show(
                this,
                $"{duplicateCount:N0} imported object(s) have the same geometry as object(s) that already exist " +
                "in their target layer.\r\n\r\n" +
                "Choose Yes to replace the existing object(s).\r\n" +
                "Choose No to skip the duplicate incoming object(s).\r\n" +
                "Choose Cancel to abort the import.",
                "Duplicate Geometry",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            return result switch
            {
                DialogResult.Yes => ImportDuplicateGeometryChoice.Replace,
                DialogResult.No => ImportDuplicateGeometryChoice.Skip,
                _ => ImportDuplicateGeometryChoice.Cancel
            };
        }

        /// <summary>
        /// Asks whether an incoming Project Boundary should replace the one already stored in the
        /// default Project Boundary layer. A project may keep only one Project Boundary object.
        /// </summary>
        private ImportDuplicateGeometryChoice PromptImportProjectBoundaryChoice(int existingBoundaryCount)
        {
            if (InvokeRequired)
            {
                return (ImportDuplicateGeometryChoice)Invoke(
                    new Func<int, ImportDuplicateGeometryChoice>(PromptImportProjectBoundaryChoice),
                    existingBoundaryCount);
            }

            DialogResult result = MessageBox.Show(
                this,
                $"A Project Boundary already exists ({existingBoundaryCount:N0} object(s)).\r\n\r\n" +
                "Choose Yes to replace the existing Project Boundary.\r\n" +
                "Choose No to keep the existing Project Boundary and skip the incoming one.\r\n" +
                "Choose Cancel to abort the import.",
                "Replace Project Boundary",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            return result switch
            {
                DialogResult.Yes => ImportDuplicateGeometryChoice.Replace,
                DialogResult.No => ImportDuplicateGeometryChoice.Skip,
                _ => ImportDuplicateGeometryChoice.Cancel
            };
        }

        private async Task ShowImportExternalLayersWorkflowAsync()
        {
            if (!EnsureApplicationUnlockedForEditing("importing external layers"))
                return;

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
                Title = "Import External Layers",
                Filter = "External vector files (*.dxf;*.dwg;*.kml;*.kmz)|*.dxf;*.dwg;*.kml;*.kmz|DXF files (*.dxf)|*.dxf|DWG files (*.dwg)|*.dwg|KML/KMZ files (*.kml;*.kmz)|*.kml;*.kmz|All files (*.*)|*.*",
                Multiselect = false,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                SetOperationProgress(5, "Inspecting external vector file...", showProgressForm: false);

                ProjectRasterCrsContext projectCrs =
                    await _projectRasterCrsResolver.ResolveAsync(AppServices.Context.Session);
                ExternalLayerFileInfo fileInfo =
                    _externalLayerImportService.Inspect(dialog.FileName);

                if (fileInfo.Layers.Count == 0)
                {
                    MessageBox.Show(
                        this,
                        "No importable layers were found in the selected file.",
                        "External Layer Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                string sourceCrs = string.IsNullOrWhiteSpace(fileInfo.DetectedCrsCode)
                    ? projectCrs.TargetSrsDefinition
                    : fileInfo.DetectedCrsCode;

                using frmExternalLayerImport importForm = new(fileInfo, sourceCrs);
                if (importForm.ShowDialog(this) != DialogResult.OK)
                    return;

                SetOperationProgress(30, "Importing external layers...", showProgressForm: false);
                ExternalLayerImportResult result =
                    await _externalLayerImportService.ImportAsync(
                        AppServices.Context.Session,
                        dialog.FileName,
                        importForm.ImportOptions,
                        PromptImportDuplicateGeometryChoice,
                        PromptImportProjectBoundaryChoice);

                if (!result.Success)
                {
                    MessageBox.Show(
                        this,
                        result.ErrorMessage ?? "External layer import failed.",
                        "External Layer Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MarkProjectModifiedIfOpen();
                await ApplyCurrentCanvasThemeToLayerColorsAsync();
                SetOperationProgress(92, "Refreshing external layer tree...", showProgressForm: false);
                await RefreshLayerTreeAsync();

                if (result.BoundingBox != null && !result.BoundingBox.IsNull)
                    mapCanvasControlMain.ZoomToWorldBounds(ToRectangleD(result.BoundingBox));

                SetCanvasCommandStatus(
                    $"Imported {result.ObjectsCreated} object(s) into {result.LayersCreated} external layer(s).");

                if (result.Warnings is { Count: > 0 })
                {
                    MessageBox.Show(
                        this,
                        $"Imported {result.ObjectsCreated} object(s) into {result.LayersCreated} external layer(s).\r\n\r\n" +
                        "Some import decisions need your attention:\r\n\r\n" +
                        string.Join("\r\n", result.Warnings),
                        "External Layer Import",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains(
                    "project coordinate system",
                    StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    ex.Message,
                    "External Layer Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                OpenProjectSettings();
            }
            catch (Exception ex)
            {
                LogProjectError("External layer import failed.", ex);
                MessageBox.Show(
                    $"Failed to import external layers: {GetMostUsefulExceptionMessage(ex)}",
                    "External Layer Import",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private async Task ShowImportCadastralMapWorkflowAsync()
        {
            if (!EnsureApplicationUnlockedForEditing("importing cadastral maps"))
                return;

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
                Filter = "Cadastral map files (*.dxf;*.dwg;*.shp;*.kml;*.kmz)|*.dxf;*.dwg;*.shp;*.kml;*.kmz|DXF files (*.dxf)|*.dxf|DWG files (*.dwg)|*.dwg|Shapefiles (*.shp)|*.shp|KML/KMZ files (*.kml;*.kmz)|*.kml;*.kmz|All files (*.*)|*.*",
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
                importForm.ImportProgressChanged += progress =>
                    SetOperationProgress(progress.Percent, progress.Status, showProgressForm: false);

                try
                {
                    if (importForm.ShowDialog(this) != DialogResult.OK)
                        return;

                    MarkProjectModifiedIfOpen();
                    await ApplyCurrentCanvasThemeToLayerColorsAsync();
                    SetOperationProgress(100, "Refreshing cadastral map layers...", showProgressForm: false);
                    await RefreshLayerTreeAsync();

                    CadastralImportResult? result = importForm.ImportResult;
                    if (result?.BoundingBox != null && !result.BoundingBox.IsNull)
                        mapCanvasControlMain.ZoomToWorldBounds(ToRectangleD(result.BoundingBox));

                    SetCanvasCommandStatus(
                        result == null
                            ? "Cadastral map imported."
                            : $"Imported {result.ObjectsCreated} cadastral parcel object(s).");

                    if (result?.DuplicateObjectsSkipped > 0)
                    {
                        MessageBox.Show(
                            this,
                            $"Imported unique cadastral objects and skipped {result.DuplicateObjectsSkipped} duplicate shape(s) with the same geometry.",
                            "Cadastral Map Import",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }

                    ShowCadastralRecordAssignmentForm(openAutoAssignment: true);
                }
                finally
                {
                    HideOperationProgress();
                }
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
            if (!EnsureApplicationUnlockedForEditing("importing project boundaries"))
                return;

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
                await ApplyCurrentCanvasThemeToLayerColorsAsync();
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

                using frmProjectBoundaryAssignment form = new(
                    candidates,
                    readOnlyMode: _isApplicationEditLocked);
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

        private void CadastralRecordsAssignmentToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ShowCadastralRecordAssignmentForm(openAutoAssignment: false);
        }

        private void ShowCadastralRecordAssignmentForm(
            bool openAutoAssignment,
            Guid? preferredCanvasObjectId = null)
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

            if (_cadastralRecordAssignmentForm is { IsDisposed: false })
            {
                PositionAssignmentFormAtCanvasTopLeft(_cadastralRecordAssignmentForm);
                _cadastralRecordAssignmentForm.Show();
                _cadastralRecordAssignmentForm.BringToFront();
                _cadastralRecordAssignmentForm.Focus();
                if (openAutoAssignment)
                    _cadastralRecordAssignmentForm.OpenAutoAssignmentDialog();
                else if (preferredCanvasObjectId.HasValue)
                    _cadastralRecordAssignmentForm.OpenManualAssignmentMode();
                if (preferredCanvasObjectId.HasValue)
                    _cadastralRecordAssignmentForm.SelectCanvasObjectFromCanvas(
                        preferredCanvasObjectId.Value,
                        previewOnCanvas: true);
                ActivateCanvasTool(MapCanvasTool.Select);
                return;
            }

            frmCadastralRecordAssignment form = new(
                AppServices.Context.Session,
                _cadastralRecordAssignmentService,
                openAutoAssignment,
                preferredCanvasObjectId,
                readOnlyMode: _isApplicationEditLocked);
            _cadastralRecordAssignmentForm = form;
            form.SelectedCanvasObjectChanged += PreviewAssignmentCandidateOnCanvas;
            form.AssignmentCommitted += CadastralRecordAssignmentForm_AssignmentCommitted;
            form.FormClosed += CadastralRecordAssignmentForm_FormClosed;
            PositionAssignmentFormAtCanvasTopLeft(form);
            form.Show(this);
            ActivateCanvasTool(MapCanvasTool.Select);
        }

        private void RoadDataToolStripMenuItem_Click(object? sender, EventArgs e)
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

            using frmDefineRoads form = new(readOnlyMode: _isApplicationEditLocked);
            form.ShowDialog(this);
        }

        private void RoadAssignmentToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ShowRoadCenterlineAssignmentForm();
        }

        private void BlockAssignmentToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ShowBlockAssignmentForm();
        }

        private void ShowRoadCenterlineAssignmentForm(Guid? preferredCanvasObjectId = null)
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

            if (_roadCenterlineAssignmentForm is { IsDisposed: false })
            {
                PositionAssignmentFormAtCanvasTopLeft(_roadCenterlineAssignmentForm);
                _roadCenterlineAssignmentForm.Show();
                _roadCenterlineAssignmentForm.BringToFront();
                _roadCenterlineAssignmentForm.Focus();
                if (preferredCanvasObjectId.HasValue)
                    _roadCenterlineAssignmentForm.SelectCanvasObjectFromCanvas(
                        preferredCanvasObjectId.Value,
                        previewOnCanvas: true);
                ActivateCanvasTool(MapCanvasTool.Select);
                return;
            }

            frmRoadCenterlineAssignment form = new(
                AppServices.Context.Session,
                _roadCenterlineAssignmentService,
                preferredCanvasObjectId,
                readOnlyMode: _isApplicationEditLocked);
            _roadCenterlineAssignmentForm = form;
            form.SelectedCanvasObjectChanged += PreviewRoadAssignmentCandidateOnCanvas;
            form.AssignmentCommitted += RoadCenterlineAssignmentForm_AssignmentCommitted;
            form.FormClosed += RoadCenterlineAssignmentForm_FormClosed;
            PositionAssignmentFormAtCanvasTopLeft(form);
            form.Show(this);
            ActivateCanvasTool(MapCanvasTool.Select);
        }

        private void ShowBlockAssignmentForm(Guid? preferredCanvasObjectId = null)
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

            if (_blockAssignmentForm is { IsDisposed: false })
            {
                PositionAssignmentFormAtCanvasTopLeft(_blockAssignmentForm);
                _blockAssignmentForm.Show();
                _blockAssignmentForm.BringToFront();
                _blockAssignmentForm.Focus();
                if (preferredCanvasObjectId.HasValue)
                    _blockAssignmentForm.SelectCanvasObjectFromCanvas(
                        preferredCanvasObjectId.Value,
                        previewOnCanvas: true);
                ActivateCanvasTool(MapCanvasTool.Select);
                return;
            }

            frmBlockAssignment form = new(
                AppServices.Context.Session,
                _blockAssignmentService,
                preferredCanvasObjectId,
                readOnlyMode: _isApplicationEditLocked);
            _blockAssignmentForm = form;
            form.SelectedCanvasObjectChanged += PreviewBlockAssignmentCandidateOnCanvas;
            form.AssignmentCommitted += BlockAssignmentForm_AssignmentCommitted;
            form.FormClosed += BlockAssignmentForm_FormClosed;
            PositionAssignmentFormAtCanvasTopLeft(form);
            form.Show(this);
            ActivateCanvasTool(MapCanvasTool.Select);
        }

        private void PositionAssignmentFormAtCanvasTopLeft(Form form)
        {
            Point canvasTopLeft = mapCanvasControlMain.PointToScreen(Point.Empty);
            Rectangle workingArea = Screen.FromControl(mapCanvasControlMain).WorkingArea;
            int x = Math.Clamp(canvasTopLeft.X + 8, workingArea.Left, Math.Max(workingArea.Left, workingArea.Right - form.Width));
            int y = Math.Clamp(canvasTopLeft.Y + 8, workingArea.Top, Math.Max(workingArea.Top, workingArea.Bottom - form.Height));
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(x, y);
        }

        private void BlockDataToolStripMenuItem_Click(object? sender, EventArgs e)
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

            using frmDefineBlocks form = new(readOnlyMode: _isApplicationEditLocked);
            form.ShowDialog(this);
        }

        private async void CadastralRecordAssignmentForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not frmCadastralRecordAssignment form)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            form.SelectedCanvasObjectChanged -= PreviewAssignmentCandidateOnCanvas;
            form.AssignmentCommitted -= CadastralRecordAssignmentForm_AssignmentCommitted;
            form.FormClosed -= CadastralRecordAssignmentForm_FormClosed;
            if (ReferenceEquals(_cadastralRecordAssignmentForm, form))
                _cadastralRecordAssignmentForm = null;

            mapCanvasControlMain.ClearPreviewSelection();

            if (form.AssignmentChanged && !_isApplicationEditLocked)
            {
                MarkProjectModifiedIfOpen();
                await RefreshVectorCanvasFeaturesAsync();
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Cadastral records assigned.");
            }
        }

        private async void CadastralRecordAssignmentForm_AssignmentCommitted()
        {
            if (!EnsureApplicationUnlockedForEditing("assigning cadastral records"))
                return;

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            mapCanvasControlMain.RequestRender();
            SetCanvasCommandStatus("Cadastral record assignment updated.");
        }

        private async void RoadCenterlineAssignmentForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not frmRoadCenterlineAssignment form)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            form.SelectedCanvasObjectChanged -= PreviewRoadAssignmentCandidateOnCanvas;
            form.AssignmentCommitted -= RoadCenterlineAssignmentForm_AssignmentCommitted;
            form.FormClosed -= RoadCenterlineAssignmentForm_FormClosed;
            if (ReferenceEquals(_roadCenterlineAssignmentForm, form))
                _roadCenterlineAssignmentForm = null;

            mapCanvasControlMain.ClearPreviewSelection();

            if (form.AssignmentChanged && !_isApplicationEditLocked)
            {
                MarkProjectModifiedIfOpen();
                await RefreshVectorCanvasFeaturesAsync();
                await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Road data assigned.");
            }
        }

        private async void RoadCenterlineAssignmentForm_AssignmentCommitted()
        {
            if (!EnsureApplicationUnlockedForEditing("assigning road data"))
                return;

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
            mapCanvasControlMain.RequestRender();
            SetCanvasCommandStatus("Road data assignment updated.");
        }

        private async void BlockAssignmentForm_FormClosed(object? sender, FormClosedEventArgs e)
        {
            if (sender is not frmBlockAssignment form)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            form.SelectedCanvasObjectChanged -= PreviewBlockAssignmentCandidateOnCanvas;
            form.AssignmentCommitted -= BlockAssignmentForm_AssignmentCommitted;
            form.FormClosed -= BlockAssignmentForm_FormClosed;
            if (ReferenceEquals(_blockAssignmentForm, form))
                _blockAssignmentForm = null;

            mapCanvasControlMain.ClearPreviewSelection();

            if (form.AssignmentChanged && !_isApplicationEditLocked)
            {
                MarkProjectModifiedIfOpen();
                await RefreshGeneratedRoadParcelAsync(AppServices.Context.Session.GetDbContext());
                await RefreshVectorCanvasFeaturesAsync();
                await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
                mapCanvasControlMain.RequestRender();
                SetCanvasCommandStatus("Block data assigned.");
            }
        }

        private async void BlockAssignmentForm_AssignmentCommitted()
        {
            if (!EnsureApplicationUnlockedForEditing("assigning block data"))
                return;

            MarkProjectModifiedIfOpen();
            await RefreshGeneratedRoadParcelAsync(AppServices.Context.Session.GetDbContext());
            await RefreshVectorCanvasFeaturesAsync();
            await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
            mapCanvasControlMain.RequestRender();
            SetCanvasCommandStatus("Block data assignment updated.");
        }

        private void PreviewAssignmentCandidateOnCanvas(Guid? canvasObjectId, bool zoomToObject)
        {
            if (!canvasObjectId.HasValue)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            _suppressCadastralAssignmentCanvasSelectionChanged = true;
            try
            {
                mapCanvasControlMain.PreviewSelectCanvasObject(
                    canvasObjectId.Value,
                    zoomToObject: false);
                if (zoomToObject)
                    mapCanvasControlMain.ZoomToCanvasObjects([canvasObjectId.Value]);
            }
            finally
            {
                _suppressCadastralAssignmentCanvasSelectionChanged = false;
            }
        }

        private void PreviewRoadAssignmentCandidateOnCanvas(Guid? canvasObjectId, bool zoomToObject)
        {
            if (!canvasObjectId.HasValue)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            _suppressRoadAssignmentCanvasSelectionChanged = true;
            try
            {
                mapCanvasControlMain.PreviewSelectCanvasObject(
                    canvasObjectId.Value,
                    zoomToObject: false);
                if (zoomToObject)
                    mapCanvasControlMain.ZoomToCanvasObjects([canvasObjectId.Value]);
            }
            finally
            {
                _suppressRoadAssignmentCanvasSelectionChanged = false;
            }
        }

        private void PreviewBlockAssignmentCandidateOnCanvas(Guid? canvasObjectId, bool zoomToObject)
        {
            if (!canvasObjectId.HasValue)
            {
                mapCanvasControlMain.ClearPreviewSelection();
                return;
            }

            _suppressBlockAssignmentCanvasSelectionChanged = true;
            try
            {
                mapCanvasControlMain.PreviewSelectCanvasObject(
                    canvasObjectId.Value,
                    zoomToObject: false);
                if (zoomToObject)
                    mapCanvasControlMain.ZoomToCanvasObjects([canvasObjectId.Value]);
            }
            finally
            {
                _suppressBlockAssignmentCanvasSelectionChanged = false;
            }
        }

        private async void MapCanvasControlMain_SelectedCanvasObjectsChanged(IReadOnlyList<Guid> selectedObjectIds)
        {
            _currentSelectedCanvasObjectIds = selectedObjectIds.ToArray();
            int uniqueSelectionCount = selectedObjectIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .Count();

            if (uniqueSelectionCount > MaxPropertySelectionDetails)
            {
                ShowLargeSelectionPropertyMessage(uniqueSelectionCount);
                return;
            }

            await LoadSelectedParcelPropertiesAsync(selectedObjectIds);

            if (_suppressCadastralAssignmentCanvasSelectionChanged ||
                _cadastralRecordAssignmentForm is not { IsDisposed: false } form ||
                selectedObjectIds.Count == 0)
            {
                if (!_suppressRoadAssignmentCanvasSelectionChanged &&
                    _roadCenterlineAssignmentForm is { IsDisposed: false } roadForm &&
                    selectedObjectIds.Count > 0)
                {
                    roadForm.SelectCanvasObjectFromCanvas(selectedObjectIds[0]);
                }

                if (!_suppressBlockAssignmentCanvasSelectionChanged &&
                    _blockAssignmentForm is { IsDisposed: false } blockForm &&
                    selectedObjectIds.Count > 0)
                {
                    blockForm.SelectCanvasObjectFromCanvas(selectedObjectIds[0]);
                }

                return;
            }

            form.SelectCanvasObjectFromCanvas(selectedObjectIds[0]);

            if (!_suppressRoadAssignmentCanvasSelectionChanged &&
                _roadCenterlineAssignmentForm is { IsDisposed: false } activeRoadForm)
            {
                activeRoadForm.SelectCanvasObjectFromCanvas(selectedObjectIds[0]);
            }

            if (!_suppressBlockAssignmentCanvasSelectionChanged &&
                _blockAssignmentForm is { IsDisposed: false } activeBlockForm)
            {
                activeBlockForm.SelectCanvasObjectFromCanvas(selectedObjectIds[0]);
            }
        }

        private async Task LoadSelectedParcelPropertiesAsync(IReadOnlyList<Guid> selectedObjectIds)
        {
            if (IsDisposed || Disposing)
                return;

            if (selectedObjectIds.Count == 0)
            {
                ClearCurrentPropertyGridSelection();
                ShowNoParcelSelection();
                return;
            }

            if (!AppServices.HasContext)
            {
                ClearCurrentPropertyGridSelection();
                ShowPropertyGridMessage("Open a project to inspect object properties.");
                return;
            }

            try
            {
                List<Guid> selectedIds = selectedObjectIds
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (selectedIds.Count == 0)
                {
                    ClearCurrentPropertyGridSelection();
                    ShowNoParcelSelection();
                    return;
                }

                if (selectedIds.Count > MaxPropertySelectionDetails)
                {
                    ShowLargeSelectionPropertyMessage(selectedIds.Count);
                    return;
                }

                AppDbContext context = AppServices.Context.Session.GetDbContext();
                await ProjectDatabaseCompatibility.EnsureAsync(context);

                List<CanvasObject> selectedObjects = await context.CanvasObjects
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.LandOwner)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.CoOwners)
                            .ThenInclude(coOwner => coOwner.LandOwner)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.MalpotReference)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.ParcelFrontages)
                            .ThenInclude(frontage => frontage.Road)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.ParcelContributionSummary)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.ParcelContributions)
                            .ThenInclude(contribution => contribution.ContributionCategory)
                    .Include(item => item.Road)
                    .Include(item => item.Block)
                    .Include(item => item.ReplottedParcel)
                        .ThenInclude(parcel => parcel!.Block)
                    .Include(item => item.ReplottedParcel)
                        .ThenInclude(parcel => parcel!.PlotType)
                    .Where(item => selectedIds.Contains(item.Id))
                    .ToListAsync();

                await LoadFallbackBaselineParcelsForSelectionAsync(context, selectedObjects);
                await LoadFallbackRoadsForSelectionAsync(context, selectedObjects);
                await LoadFallbackBlocksForSelectionAsync(context, selectedObjects);

                List<CanvasObject> orderedSelectedObjects = selectedIds
                    .Select(id => selectedObjects.FirstOrDefault(item => item.Id == id))
                    .Where(item => item != null)
                    .Cast<CanvasObject>()
                    .ToList();

                if (orderedSelectedObjects.Count == 0)
                {
                    ClearCurrentPropertyGridSelection();
                    ShowPropertyGridMessage("Selected object was not found in the project database.");
                    return;
                }

                _currentPropertyGridObjects = orderedSelectedObjects;
                _currentPropertyGridSelectedCount = selectedIds.Count;
                PopulateSelectedPropertyObjectCombo(orderedSelectedObjects);
                PopulatePropertyGridForSelectedComboItem();
            }
            catch (Exception ex)
            {
                ClearCurrentPropertyGridSelection();
                ShowPropertyGridMessage($"Could not load selected object properties. {ex.Message}");
            }
        }

        private async Task RefreshCurrentSelectedCanvasObjectPropertiesAsync(Guid? preferredObjectId = null)
        {
            if (_currentSelectedCanvasObjectIds.Count == 0)
                return;

            Guid? selectedComboObjectId = preferredObjectId;
            bool keepAllSelected = false;
            if (cboSelectedPropertyObject.SelectedItem is SelectedPropertyObjectComboItem selectedItem)
            {
                selectedComboObjectId ??= selectedItem.CanvasObjectId;
                keepAllSelected = selectedItem.IsAll && !selectedComboObjectId.HasValue;
            }

            await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);

            if (selectedComboObjectId.HasValue &&
                TrySelectPropertyComboObject(selectedComboObjectId.Value))
            {
                return;
            }

            if (keepAllSelected && cboSelectedPropertyObject.Items.Count > 0)
            {
                _suppressSelectedPropertyObjectChanged = true;
                try
                {
                    cboSelectedPropertyObject.SelectedIndex = 0;
                }
                finally
                {
                    _suppressSelectedPropertyObjectChanged = false;
                }

                PopulatePropertyGridForSelectedComboItem();
                UpdateSelectedPropertyCycleButtons();
            }
        }

        private bool TrySelectPropertyComboObject(Guid canvasObjectId)
        {
            for (int index = 0; index < cboSelectedPropertyObject.Items.Count; index++)
            {
                if (cboSelectedPropertyObject.Items[index] is SelectedPropertyObjectComboItem item &&
                    item.CanvasObjectId == canvasObjectId)
                {
                    _suppressSelectedPropertyObjectChanged = true;
                    try
                    {
                        cboSelectedPropertyObject.SelectedIndex = index;
                    }
                    finally
                    {
                        _suppressSelectedPropertyObjectChanged = false;
                    }

                    PopulatePropertyGridForSelectedComboItem();
                    UpdateSelectedPropertyCycleButtons();
                    return true;
                }
            }

            return false;
        }

        private static async Task LoadFallbackBaselineParcelsForSelectionAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> selectedObjects)
        {
            List<CanvasObject> objectsNeedingParcel = selectedObjects
                .Where(item => item.BaselineParcel == null)
                .ToList();
            if (objectsNeedingParcel.Count == 0)
                return;

            HashSet<int> parcelIds = objectsNeedingParcel
                .Select(item =>
                {
                    CadastralCanvasMetadata? metadata = ReadCadastralMetadata(item.GeometryMetadataJson);
                    return item.BaselineParcelId ?? GetAssignedCadastralMetadata(metadata)?.BaselineParcelId;
                })
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            Dictionary<int, BaselineParcel> parcelsById = parcelIds.Count == 0
                ? []
                : await IncludeParcelPropertyDetails(context.BaselineParcels.AsNoTracking())
                    .Where(parcel => parcelIds.Contains(parcel.Id))
                    .ToDictionaryAsync(parcel => parcel.Id);

            HashSet<string> lookupCodes = objectsNeedingParcel
                .Select(item => ReadCadastralMetadata(item.GeometryMetadataJson))
                .Select(GetAssignedCadastralMetadata)
                .Where(metadata => metadata != null &&
                                   !string.IsNullOrWhiteSpace(metadata.MapSheetNo) &&
                                   !string.IsNullOrWhiteSpace(metadata.ParcelNo))
                .Select(metadata => BuildParcelLookupCode(metadata!.MapSheetNo, metadata.ParcelNo))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Dictionary<string, BaselineParcel> parcelsByCode = lookupCodes.Count == 0
                ? []
                : await IncludeParcelPropertyDetails(context.BaselineParcels.AsNoTracking())
                    .Where(parcel => lookupCodes.Contains(parcel.FullUniqueParcelCode))
                    .ToDictionaryAsync(parcel => parcel.FullUniqueParcelCode, StringComparer.OrdinalIgnoreCase);

            foreach (CanvasObject canvasObject in objectsNeedingParcel)
            {
                CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
                int? parcelId = canvasObject.BaselineParcelId ?? GetAssignedCadastralMetadata(metadata)?.BaselineParcelId;
                if (parcelId.HasValue && parcelsById.TryGetValue(parcelId.Value, out BaselineParcel? parcelById))
                {
                    canvasObject.BaselineParcel = parcelById;
                    canvasObject.BaselineParcelId ??= parcelById.Id;
                    continue;
                }

                CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);
                if (assignedMetadata == null ||
                    string.IsNullOrWhiteSpace(assignedMetadata.MapSheetNo) ||
                    string.IsNullOrWhiteSpace(assignedMetadata.ParcelNo))
                {
                    continue;
                }

                string code = BuildParcelLookupCode(assignedMetadata.MapSheetNo, assignedMetadata.ParcelNo);
                if (parcelsByCode.TryGetValue(code, out BaselineParcel? parcelByCode))
                {
                    canvasObject.BaselineParcel = parcelByCode;
                    canvasObject.BaselineParcelId ??= parcelByCode.Id;
                }
            }
        }

        private static async Task LoadFallbackRoadsForSelectionAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> selectedObjects)
        {
            List<int> roadIds = selectedObjects
                .Where(item => item.Road == null && item.RoadId.HasValue)
                .Select(item => item.RoadId!.Value)
                .Distinct()
                .ToList();
            if (roadIds.Count == 0)
                return;

            Dictionary<int, Core.Entities.Layout.Road> roadsById = await context.Roads
                .AsNoTracking()
                .Where(road => roadIds.Contains(road.Id))
                .ToDictionaryAsync(road => road.Id);

            foreach (CanvasObject canvasObject in selectedObjects)
            {
                if (canvasObject.Road == null &&
                    canvasObject.RoadId.HasValue &&
                    roadsById.TryGetValue(canvasObject.RoadId.Value, out Core.Entities.Layout.Road? road))
                {
                    canvasObject.Road = road;
                }
            }
        }

        private static async Task LoadFallbackBlocksForSelectionAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> selectedObjects)
        {
            List<int> blockIds = selectedObjects
                .Where(item => item.Block == null && item.BlockId.HasValue)
                .Select(item => item.BlockId!.Value)
                .Distinct()
                .ToList();
            if (blockIds.Count == 0)
                return;

            Dictionary<int, Core.Entities.Layout.Block> blocksById = await context.Blocks
                .AsNoTracking()
                .Where(block => blockIds.Contains(block.Id))
                .ToDictionaryAsync(block => block.Id);

            foreach (CanvasObject canvasObject in selectedObjects)
            {
                if (canvasObject.Block == null &&
                    canvasObject.BlockId.HasValue &&
                    blocksById.TryGetValue(canvasObject.BlockId.Value, out Core.Entities.Layout.Block? block))
                {
                    canvasObject.Block = block;
                }
            }
        }

        private static async Task LoadFallbackReplottedParcelsForSelectionAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> selectedObjects)
        {
            List<int> replottedParcelIds = selectedObjects
                .Where(item => item.ReplottedParcel == null && item.ReplottedParcelId.HasValue)
                .Select(item => item.ReplottedParcelId!.Value)
                .Distinct()
                .ToList();
            if (replottedParcelIds.Count == 0)
                return;

            Dictionary<int, Core.Entities.Replotting.ReplottedParcel> parcelsById = await context.ReplottedParcels
                .AsNoTracking()
                .Include(parcel => parcel.Block)
                .Include(parcel => parcel.PlotType)
                .Where(parcel => replottedParcelIds.Contains(parcel.Id))
                .ToDictionaryAsync(parcel => parcel.Id);

            foreach (CanvasObject canvasObject in selectedObjects)
            {
                if (canvasObject.ReplottedParcel == null &&
                    canvasObject.ReplottedParcelId.HasValue &&
                    parcelsById.TryGetValue(
                        canvasObject.ReplottedParcelId.Value,
                        out Core.Entities.Replotting.ReplottedParcel? parcel))
                {
                    canvasObject.ReplottedParcel = parcel;
                }
            }
        }

        private static IQueryable<BaselineParcel> IncludeParcelPropertyDetails(
            IQueryable<BaselineParcel> query)
        {
            return query
                .Include(parcel => parcel.LandOwner)
                .Include(parcel => parcel.CoOwners)
                    .ThenInclude(coOwner => coOwner.LandOwner)
                .Include(parcel => parcel.MalpotReference)
                .Include(parcel => parcel.ParcelFrontages)
                    .ThenInclude(frontage => frontage.Road)
                .Include(parcel => parcel.ParcelContributionSummary)
                .Include(parcel => parcel.ParcelContributions)
                    .ThenInclude(contribution => contribution.ContributionCategory);
        }

        private static string BuildParcelLookupCode(string? mapSheetNo, string? parcelNo)
        {
            return $"{(mapSheetNo ?? string.Empty).Trim().ToUpperInvariant()}::{(parcelNo ?? string.Empty).Trim().ToUpperInvariant()}";
        }

        private static string BuildSelectedObjectDisplayText(CanvasObject canvasObject)
        {
            CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
            CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);
            string? mapSheetNo = FirstNonEmpty(canvasObject.BaselineParcel?.MapSheetNo, assignedMetadata?.MapSheetNo);
            string? parcelNo = FirstNonEmpty(canvasObject.BaselineParcel?.ParcelNo, assignedMetadata?.ParcelNo);
            string? uniqueCode = FirstNonEmpty(canvasObject.BaselineParcel?.FullUniqueParcelCode, assignedMetadata?.FullUniqueParcelCode);

            if (!string.IsNullOrWhiteSpace(mapSheetNo) && !string.IsNullOrWhiteSpace(parcelNo))
                return $"Parcel {mapSheetNo} - {parcelNo}";

            if (!string.IsNullOrWhiteSpace(uniqueCode))
                return $"Parcel {uniqueCode}";

            if (canvasObject.ReplottedParcel != null)
            {
                string? number = FirstNonEmpty(
                    canvasObject.ReplottedParcel.BlockSequenceNumber,
                    canvasObject.ReplottedParcel.DerivedNumber,
                    canvasObject.ReplottedParcel.SystemGeneratedNumber);
                return string.IsNullOrWhiteSpace(number)
                    ? $"Replotted Parcel #{canvasObject.ReplottedParcel.Id}"
                    : $"Replotted Parcel {number}";
            }

            if (canvasObject.Road != null)
                return string.IsNullOrWhiteSpace(canvasObject.Road.RoadName)
                    ? $"Road #{canvasObject.Road.Id}"
                    : $"Road {canvasObject.Road.RoadName}";

            if (IsBlockObject(canvasObject))
            {
                string? blockName = FirstNonEmpty(
                    canvasObject.Block?.BlockName,
                    canvasObject.ObjectDescription,
                    canvasObject.CanvasLayer?.Name);
                return string.IsNullOrWhiteSpace(blockName)
                    ? $"Block {canvasObject.BlockId?.ToString() ?? canvasObject.Id.ToString()}"
                    : $"Block {blockName}";
            }

            if (IsTextObject(canvasObject))
            {
                string? text = FirstNonEmpty(canvasObject.LabelText, canvasObject.ObjectDescription);
                return string.IsNullOrWhiteSpace(text)
                    ? "Text"
                    : $"Text - {TrimForDisplay(text, 40)}";
            }

            string layerName = canvasObject.CanvasLayer?.Name ?? "No layer";
            return $"{GetDisplayObjectType(canvasObject)} - {layerName}";
        }

        private void PopulateSelectedObjectPropertyGrid(IReadOnlyList<CanvasObject> selectedObjects, int selectedCount)
        {
            if (selectedObjects.Count > 1)
            {
                PopulateMultipleObjectPropertyGrid(selectedObjects, selectedCount);
                return;
            }

            PopulateSingleObjectPropertyGrid(selectedObjects[0], selectedCount);
        }

        private void PopulateSelectedPropertyObjectCombo(IReadOnlyList<CanvasObject> selectedObjects)
        {
            _suppressSelectedPropertyObjectChanged = true;
            try
            {
                cboSelectedPropertyObject.Items.Clear();
                cboSelectedPropertyObject.Enabled = selectedObjects.Count > 0;
                UpdateSelectedPropertyCycleButtons();
                if (selectedObjects.Count == 0)
                    return;

                cboSelectedPropertyObject.Items.Add(
                    new SelectedPropertyObjectComboItem(AllSelectedObjectsComboText, null));

                foreach (CanvasObject canvasObject in selectedObjects)
                {
                    cboSelectedPropertyObject.Items.Add(
                        new SelectedPropertyObjectComboItem(
                            BuildSelectedObjectDisplayText(canvasObject),
                            canvasObject.Id));
                }

                cboSelectedPropertyObject.SelectedIndex = 0;
                UpdateSelectedPropertyCycleButtons();
            }
            finally
            {
                _suppressSelectedPropertyObjectChanged = false;
            }
        }

        private void PopulatePropertyGridForSelectedComboItem()
        {
            if (_currentPropertyGridObjects.Count == 0)
            {
                ShowNoParcelSelection();
                return;
            }

            if (cboSelectedPropertyObject.SelectedItem is SelectedPropertyObjectComboItem { IsAll: false } item &&
                item.CanvasObjectId.HasValue)
            {
                CanvasObject? selectedObject = _currentPropertyGridObjects
                    .FirstOrDefault(canvasObject => canvasObject.Id == item.CanvasObjectId.Value);
                if (selectedObject != null)
                {
                    PopulateSingleObjectPropertyGrid(selectedObject, 1);
                    return;
                }
            }

            PopulateSelectedObjectPropertyGrid(_currentPropertyGridObjects, _currentPropertyGridSelectedCount);
        }

        private CanvasObject? GetCurrentSinglePropertyGridObject()
        {
            if (_currentPropertyGridObjects.Count == 0)
                return null;

            if (cboSelectedPropertyObject.SelectedItem is SelectedPropertyObjectComboItem { IsAll: false } item &&
                item.CanvasObjectId.HasValue)
            {
                return _currentPropertyGridObjects
                    .FirstOrDefault(canvasObject => canvasObject.Id == item.CanvasObjectId.Value);
            }

            return _currentPropertyGridObjects.Count == 1
                ? _currentPropertyGridObjects[0]
                : null;
        }

        private void cboSelectedPropertyObject_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressSelectedPropertyObjectChanged)
                return;

            PopulatePropertyGridForSelectedComboItem();
            ZoomToSelectedPropertyComboItem();
            UpdateSelectedPropertyCycleButtons();
        }

        private void cboSelectedPropertyObject_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (_suppressSelectedPropertyObjectChanged)
                return;

            ZoomToSelectedPropertyComboItem();
            UpdateSelectedPropertyCycleButtons();
        }

        private void cboSelectedPropertyObject_DropDownClosed(object? sender, EventArgs e)
        {
            if (_suppressSelectedPropertyObjectChanged)
                return;

            ZoomToSelectedPropertyComboItem();
            UpdateSelectedPropertyCycleButtons();
        }

        private void btnPreviousSelectedPropertyObject_Click(object? sender, EventArgs e)
        {
            CycleSelectedPropertyObject(direction: -1);
        }

        private void btnNextSelectedPropertyObject_Click(object? sender, EventArgs e)
        {
            CycleSelectedPropertyObject(direction: 1);
        }

        private void CycleSelectedPropertyObject(int direction)
        {
            List<int> objectIndexes = GetSelectedPropertyObjectComboIndexes();
            if (objectIndexes.Count == 0)
                return;

            int currentComboIndex = cboSelectedPropertyObject.SelectedIndex;
            int currentObjectIndex = objectIndexes.IndexOf(currentComboIndex);
            int nextObjectIndex;

            if (currentObjectIndex < 0)
            {
                nextObjectIndex = direction < 0
                    ? objectIndexes.Count - 1
                    : 0;
            }
            else
            {
                nextObjectIndex = (currentObjectIndex + direction + objectIndexes.Count) % objectIndexes.Count;
            }

            int nextComboIndex = objectIndexes[nextObjectIndex];
            if (cboSelectedPropertyObject.SelectedIndex == nextComboIndex)
            {
                PopulatePropertyGridForSelectedComboItem();
                ZoomToSelectedPropertyComboItem();
                return;
            }

            cboSelectedPropertyObject.SelectedIndex = nextComboIndex;
        }

        private List<int> GetSelectedPropertyObjectComboIndexes()
        {
            List<int> indexes = new();
            for (int index = 0; index < cboSelectedPropertyObject.Items.Count; index++)
            {
                if (cboSelectedPropertyObject.Items[index] is SelectedPropertyObjectComboItem { IsAll: false })
                {
                    indexes.Add(index);
                }
            }

            return indexes;
        }

        private void UpdateSelectedPropertyCycleButtons()
        {
            bool enabled = GetSelectedPropertyObjectComboIndexes().Count > 0;
            btnPreviousSelectedPropertyObject.Enabled = enabled;
            btnNextSelectedPropertyObject.Enabled = enabled;
        }

        private void ZoomToSelectedPropertyComboItem()
        {
            if (cboSelectedPropertyObject.SelectedItem is not SelectedPropertyObjectComboItem item)
                return;

            if (item.IsAll)
            {
                ZoomToPropertyObjects(_currentPropertyGridObjects);
                return;
            }

            if (item.CanvasObjectId.HasValue)
            {
                ZoomToPropertyObjects(_currentPropertyGridObjects
                    .Where(canvasObject => canvasObject.Id == item.CanvasObjectId.Value));
            }
        }

        private void ZoomToPropertyObjects(IEnumerable<CanvasObject> canvasObjects)
        {
            List<CanvasObject> objects = canvasObjects
                .Where(canvasObject => canvasObject.Shape != null && !canvasObject.Shape.IsEmpty)
                .ToList();
            if (objects.Count == 0)
                return;

            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            bool hasBounds = false;

            foreach (CanvasObject canvasObject in objects)
            {
                NtsEnvelope envelope = canvasObject.Shape!.EnvelopeInternal;
                if (envelope.IsNull ||
                    !double.IsFinite(envelope.MinX) ||
                    !double.IsFinite(envelope.MinY) ||
                    !double.IsFinite(envelope.MaxX) ||
                    !double.IsFinite(envelope.MaxY))
                {
                    continue;
                }

                minX = Math.Min(minX, envelope.MinX);
                minY = Math.Min(minY, envelope.MinY);
                maxX = Math.Max(maxX, envelope.MaxX);
                maxY = Math.Max(maxY, envelope.MaxY);
                hasBounds = true;
            }

            if (!hasBounds || maxX <= minX || maxY <= minY)
                return;

            mapCanvasControlMain.ZoomToWorldBounds(new RectangleD(minX, minY, maxX - minX, maxY - minY));
        }

        private void PopulateMultipleObjectPropertyGrid(IReadOnlyList<CanvasObject> selectedObjects, int selectedCount)
        {
            PopulateObjectPropertySections(BuildMultipleObjectPropertySections(selectedObjects, selectedCount));
        }

        private void PopulateSingleObjectPropertyGrid(CanvasObject canvasObject, int selectedCount)
        {
            PopulateObjectPropertySections(BuildSingleObjectPropertySections(canvasObject, selectedCount));
        }

        private void PopulateObjectPropertySections(IReadOnlyList<ObjectPropertySection> sections)
        {
            string profileKey = GetObjectTypeProfileKey(_currentPropertyGridObjects);
            List<string> profileKeys = GetOrCreateProfile(profileKey);
            var selectedKeys = new HashSet<string>(profileKeys, StringComparer.OrdinalIgnoreCase);

            dgvParcelObjProperty.SuspendLayout();
            try
            {
                dgvParcelObjProperty.Rows.Clear();
                SetPropertiesPanelTitle();

                foreach (ObjectPropertySection section in sections)
                {
                    int categoryIndex = dgvParcelObjProperty.Rows.Count;
                    AddPropertyCategory(section.Title);

                    foreach (ObjectPropertyRow row in section.Rows.Where(row => selectedKeys.Contains(row.Key)))
                    {
                        // Always show user-selected fields, even when the value is empty.
                        AddPropertyRow(
                            row.Key,
                            row.Label,
                            row.Value,
                            includeWhenEmpty: true,
                            IsPropertyRowEditable(row));
                    }

                    if (dgvParcelObjProperty.Rows.Count == categoryIndex + 1)
                    {
                        dgvParcelObjProperty.Rows.RemoveAt(categoryIndex);
                    }
                }

                if (dgvParcelObjProperty.Rows.Count == 0)
                {
                    ShowPropertyGridMessage("No displayable properties were found for this selection.");
                }
                else
                {
                    dgvParcelObjProperty.ClearSelection();
                }
            }
            finally
            {
                dgvParcelObjProperty.ResumeLayout();
            }
        }

        private IReadOnlyList<ObjectPropertySection> BuildMultipleObjectPropertySections(
            IReadOnlyList<CanvasObject> selectedObjects,
            int selectedCount)
        {
            return BuildObjectPropertySectionsFromFields(selectedObjects, selectedCount);
        }

        private IReadOnlyList<ObjectPropertySection> BuildObjectPropertySectionsFromFields(
            IReadOnlyList<CanvasObject> selectedObjects,
            int selectedCount)
        {
            List<ObjectPropertySection> sections = new();
            HashSet<string> relevantKeys = GetRelevantFieldKeys(selectedObjects);
            List<ParcelPropertyField> fields = GetParcelPropertyFields()
                .Where(field => relevantKeys.Contains(field.Key))
                .ToList();

            foreach (IGrouping<string, ParcelPropertyField> categoryGroup in fields.GroupBy(field => field.Category))
            {
                List<ObjectPropertyRow> rows = new();
                foreach (ParcelPropertyField field in categoryGroup)
                {
                    IReadOnlyList<CanvasObject> applicableObjects = field.Key.StartsWith("selection.", StringComparison.OrdinalIgnoreCase) ||
                                                                    field.Key.StartsWith("object.", StringComparison.OrdinalIgnoreCase) ||
                                                                    field.Key.StartsWith("canvas.", StringComparison.OrdinalIgnoreCase)
                        ? selectedObjects
                        : selectedObjects
                            .Where(canvasObject => IsFieldRelevantToObject(field.Key, canvasObject))
                            .ToList();

                    if (applicableObjects.Count == 0)
                        continue;

                    string? value = GetAggregatedFieldValue(applicableObjects, field, applicableObjects.Count);
                    rows.Add(new ObjectPropertyRow(
                        field.Key,
                        field.Label,
                        value,
                        field.IncludeWhenEmpty || field.Category == "Selection"));
                }

                AddOptionalSection(sections, new ObjectPropertySection(
                    NormalizePropertySectionTitle(categoryGroup.Key),
                    rows));
            }

            return sections;
        }

        private IReadOnlyList<ObjectPropertySection> BuildSingleObjectPropertySections(CanvasObject canvasObject, int selectedCount)
        {
            return BuildObjectPropertySectionsFromFields([canvasObject], selectedCount);
        }

        private static ObjectPropertySection BuildObjectSection(
            CanvasObject canvasObject,
            CadastralCanvasMetadata? metadata,
            int selectedCount)
        {
            return new ObjectPropertySection("Object",
            [
                new("selection.selectedObjects", "Selected Objects", selectedCount.ToString("N0"), true),
                new("selection.objectType", "Type", GetDisplayObjectType(canvasObject), true),
                new("selection.layer", "Layer", canvasObject.CanvasLayer?.Name, true),
                new("object.layerType", "Layer Type", canvasObject.CanvasLayer?.LayerType, false),
                new("object.description", "Description", canvasObject.ObjectDescription, false),
                new("selection.assignment", "Assignment", GetAssignmentValue(canvasObject, metadata), true),
                new("canvas.visible", "Visible", FormatBoolean(canvasObject.IsVisible), true),
                new("canvas.locked", "Locked", FormatBoolean(canvasObject.IsLocked || canvasObject.CanvasLayer?.IsLocked == true), true),
                new("object.dataLink", "Data Link", GetDataLinkDisplay(canvasObject, metadata), false),
                new("canvas.objectId", "Object ID", canvasObject.Id.ToString(), false),
                new("canvas.created", "Created", FormatDate(canvasObject.CreatedDate), false),
                new("canvas.modified", "Last Modified", FormatDate(canvasObject.LastModifiedDate), false)
            ]);
        }

        private static ObjectPropertySection BuildTextSection(CanvasObject canvasObject)
        {
            return new ObjectPropertySection("Text",
            [
                new("text.value", "Text", canvasObject.LabelText, true),
                new("text.insertionPoint", "Insertion Point", FormatPoint(canvasObject.Shape?.Coordinate), false),
                new("text.alignment", "Alignment", ReadMetadataString(canvasObject.GeometryMetadataJson, "TextAlignment"), false),
                new("text.source", "Label Source", "Manual text", true)
            ]);
        }

        private static ObjectPropertySection? BuildLabelSection(CanvasObject canvasObject)
        {
            CanvasLayer? layer = canvasObject.CanvasLayer;
            if (layer == null || !layer.ShowLabels)
                return null;

            string? labelField = layer.LabelField;
            string labelValue;
            string labelSource;

            if (!string.IsNullOrEmpty(labelField) &&
                labelField.StartsWith("static:", StringComparison.OrdinalIgnoreCase))
            {
                labelValue = labelField["static:".Length..];
                labelSource = "Fixed text";
            }
            else if (!string.IsNullOrEmpty(canvasObject.LabelText))
            {
                labelValue = canvasObject.LabelText;
                labelSource = "Manual text";
            }
            else if (!string.IsNullOrEmpty(labelField))
            {
                // Auto label from object data field — show as <FieldName>
                string display = labelField.StartsWith("template:", StringComparison.OrdinalIgnoreCase)
                    ? labelField
                    : $"<{labelField}>";
                labelValue = display;
                labelSource = "Object data";
            }
            else
            {
                return null;
            }

            return new ObjectPropertySection("Label",
            [
                new("label.value", "Label", labelValue, true),
                new("label.source", "Source", labelSource, false)
            ]);
        }

        private static ObjectPropertySection BuildGeometrySection(CanvasObject canvasObject)
        {
            List<ObjectPropertyRow> rows =
            [
                new("geometry.type", "Geometry Type", canvasObject.Shape?.GeometryType ?? canvasObject.ObjectType, true),
                new("geometry.bounds", "Bounds", FormatGeometryBounds(canvasObject.Shape?.EnvelopeInternal), false)
            ];

            if (ShouldShowVertexCount(canvasObject))
            {
                rows.Insert(1, new ObjectPropertyRow(
                    "geometry.vertexCount",
                    "Points / Vertices",
                    CanvasGeometryMetricsService.GetVertexCount(canvasObject)?.ToString("N0"),
                    false));
            }

            if (IsPointGeometry(canvasObject))
            {
                rows.Insert(1, new ObjectPropertyRow("geometry.coordinate", "Coordinate", FormatPoint(canvasObject.Shape?.Coordinate), true));
            }
            else
            {
                rows.Insert(1, new ObjectPropertyRow("geometry.length", "Length / Perimeter", FormatLength(CanvasGeometryMetricsService.GetLength(canvasObject)), false));
                if (ShouldShowArea(canvasObject))
                {
                    rows.Insert(2, new ObjectPropertyRow("geometry.area", "Area", FormatArea(CanvasGeometryMetricsService.GetArea(canvasObject)), false));
                }
            }

            return new ObjectPropertySection("Geometry", rows);
        }

        private static ObjectPropertySection BuildOriginalParcelSection(CanvasObject canvasObject, CadastralCanvasMetadata? metadata)
        {
            CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);

            return new ObjectPropertySection("Original Parcel",
            [
                new("parcel.parcelNo", "Parcel No.", FirstNonEmpty(canvasObject.BaselineParcel?.ParcelNo, assignedMetadata?.ParcelNo), true),
                new("parcel.mapSheetNo", "Map Sheet No.", FirstNonEmpty(canvasObject.BaselineParcel?.MapSheetNo, assignedMetadata?.MapSheetNo), true),
                new("parcel.uniqueCode", "Unique Code", FirstNonEmpty(canvasObject.BaselineParcel?.FullUniqueParcelCode, assignedMetadata?.FullUniqueParcelCode), true),
                new("parcel.ownershipType", "Ownership Type", canvasObject.BaselineParcel?.LandOwnershipType, false),
                new("parcel.landUse", "Land Use", FirstNonEmpty(canvasObject.BaselineParcel?.LandUse, assignedMetadata?.LandUse), false),
                new("parcel.remarks", "Remarks", canvasObject.BaselineParcel?.Remarks, false),
                new("parcel.recordId", "Parcel Record ID", canvasObject.BaselineParcel?.Id.ToString(), false)
            ]);
        }

        private static ObjectPropertySection BuildOwnerSection(CanvasObject canvasObject, CadastralCanvasMetadata? metadata)
        {
            CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);

            return new ObjectPropertySection("Owner",
            [
                new("owner.primaryOwner", "Primary Owner", FirstNonEmpty(canvasObject.BaselineParcel?.LandOwner?.FullName, assignedMetadata?.OwnerName), true),
                new("owner.coOwners", "Co-Owners", FormatCoOwners(canvasObject.BaselineParcel), true),
                new("owner.fatherSpouse", "Father/Spouse", canvasObject.BaselineParcel?.LandOwner?.FatherOrSpouseName, false),
                new("owner.gender", "Gender", canvasObject.BaselineParcel?.LandOwner?.Gender, false),
                new("owner.citizenshipNo", "Citizenship No.", canvasObject.BaselineParcel?.LandOwner?.CitizenshipNumber, false),
                new("owner.contact", "Contact", canvasObject.BaselineParcel?.LandOwner?.ContactNumber, false),
                new("owner.email", "Email", canvasObject.BaselineParcel?.LandOwner?.Email, false)
            ]);
        }

        private static ObjectPropertySection BuildParcelAreaSection(CanvasObject canvasObject, CadastralCanvasMetadata? metadata)
        {
            CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);

            return new ObjectPropertySection("Parcel Area",
            [
                new("area.originalRecord", "Original Area (Land Records)", FormatArea(canvasObject.BaselineParcel?.OriginalAreaSqm ?? assignedMetadata?.RecordAreaSqm), true),
                new("area.fieldMeasured", "Area (Measured in Field)", FormatArea(canvasObject.BaselineParcel?.FieldMeasuredAreaSqm), false),
                new("area.effective", "Effective Area", FormatArea(canvasObject.BaselineParcel?.EffectiveAreaSqm), false),
                new("area.effectiveMode", "Effective Area Mode", canvasObject.BaselineParcel == null
                    ? null
                    : canvasObject.BaselineParcel.IsEffectiveAreaManual ? "Manual" : "Calculated", false),
                new("area.imported", "Imported Area", FormatArea(metadata?.CalculatedAreaSqm), false),
                new("geometry.area", "Canvas Geometry Area", FormatArea(CanvasGeometryMetricsService.GetArea(canvasObject)), false)
            ]);
        }

        private static ObjectPropertySection BuildTenancySection(CanvasObject canvasObject)
        {
            return new ObjectPropertySection("Tenancy",
            [
                new("tenancy.hasTenant", "Has Tenant", canvasObject.BaselineParcel == null ? null : FormatBoolean(canvasObject.BaselineParcel.HasTenant), false),
                new("tenancy.tenantName", "Tenant Name", canvasObject.BaselineParcel?.TenantName, false)
            ]);
        }

        private static ObjectPropertySection BuildLandRecordSection(CanvasObject canvasObject)
        {
            return new ObjectPropertySection("Land Record",
            [
                new("landRecord.mothNo", "Moth No.", canvasObject.BaselineParcel?.MalpotReference?.MothNo, true),
                new("landRecord.paanaNo", "Paana No.", canvasObject.BaselineParcel?.MalpotReference?.PaanaNo, true)
            ]);
        }

        private static ObjectPropertySection BuildParcelFrontageSection(CanvasObject canvasObject)
        {
            return new ObjectPropertySection("Frontage",
            [
                new("frontage.roads", "Road Frontages", FormatFrontages(canvasObject.BaselineParcel), false)
            ]);
        }

        private static ObjectPropertySection BuildContributionSection(CanvasObject canvasObject)
        {
            var summary = canvasObject.BaselineParcel?.ParcelContributionSummary;
            return new ObjectPropertySection("Contribution",
            [
                new("contribution.general", "General Contribution", FormatArea(summary?.TotalGeneralContributionSqm), false),
                new("contribution.specific", "Specific Contribution", FormatArea(summary?.TotalSpecificContributionSqm), false),
                new("contribution.total", "Total Contribution", FormatArea(summary?.TotalContributionSqm), false),
                new("contribution.percent", "Contribution Percent", FormatPercent(summary?.TotalContributionPercent), false),
                new("contribution.netReturnable", "Net Returnable Area", FormatArea(summary?.NetReturnableAreaSqm), false),
                new("contribution.replottedAssigned", "Replotted Area Assigned", FormatArea(summary?.ReplottedAreaAssignedSqm), false)
            ]);
        }

        private static ObjectPropertySection BuildReplottedParcelSection(CanvasObject canvasObject)
        {
            var parcel = canvasObject.ReplottedParcel;
            return new ObjectPropertySection("Replotted Parcel",
            [
                new("replotted.systemNumber", "System Number", parcel?.SystemGeneratedNumber, false),
                new("replotted.derivedNumber", "Derived Number", parcel?.DerivedNumber, false),
                new("replotted.blockSequence", "Block Sequence", parcel?.BlockSequenceNumber, false),
                new("replotted.activeNumberType", "Active Number Type", parcel?.ActiveNumberType, false),
                new("replotted.plotType", "Plot Type", parcel?.PlotType?.TypeName, false),
                new("replotted.block", "Block", parcel?.Block?.BlockName, false),
                new("replotted.plotArea", "Plot Area", FormatArea(parcel?.PlotAreaSqm), false),
                new("replotted.notes", "Notes", parcel?.Notes, false),
                new("replotted.recordId", "Record ID", parcel?.Id.ToString() ?? canvasObject.ReplottedParcelId?.ToString(), false)
            ]);
        }

        private static ObjectPropertySection BuildRoadSection(CanvasObject canvasObject)
        {
            var road = canvasObject.Road;
            return new ObjectPropertySection("Road",
            [
                new("road.name", "Name", road?.RoadName, true),
                new("road.code", "Code", road?.RoadCode, false),
                new("road.type", "Type", road?.RoadType, false),
                new("road.surface", "Surface", road?.SurfaceType, false),
                new("road.width", "Road Width", FormatLength(road?.RoadWidth), false),
                new("road.rightOfWay", "Right of Way", FormatLength(road?.RightOfWayWidth), false),
                new("road.description", "Description", road?.Description, false),
                new("road.recordId", "Record ID", road?.Id.ToString() ?? canvasObject.RoadId?.ToString(), false)
            ]);
        }

        private static ObjectPropertySection BuildBlockSection(CanvasObject canvasObject)
        {
            var block = canvasObject.Block;
            return new ObjectPropertySection("Block",
            [
                new("block.name", "Name", block?.BlockName, true),
                new("block.code", "Code", block?.BlockCode, false),
                new("block.depth", "Depth", FormatLength(block?.BlockDepth), false),
                new("block.depthGeometry", "Block Depth (from Geometry)", FormatLength(CanvasGeometryMetricsService.GetBlockDepthFromGeometry(canvasObject)), false),
                new("block.length", "Length", FormatLength(block?.BlockLength), false),
                new("block.landUse", "Land Use", block?.BlockLandUse, false),
                new("block.area", "Block Area", FormatArea(GetBlockDisplayArea(canvasObject)), false),
                new("block.description", "Description", block?.Description, false),
                new("block.recordId", "Record ID", block?.Id.ToString() ?? canvasObject.BlockId?.ToString(), false)
            ]);
        }

        private static double? GetBlockDisplayArea(CanvasObject canvasObject)
        {
            double? geometryArea = CanvasGeometryMetricsService.GetArea(canvasObject);
            if (geometryArea.HasValue)
                return geometryArea;

            if (canvasObject.Block != null && canvasObject.Block.BlockArea > 0)
                return canvasObject.Block.BlockArea;

            return null;
        }

        private static ObjectPropertySection BuildDrawingStyleSection(CanvasObject canvasObject)
        {
            return new ObjectPropertySection("Style",
            [
                new("style.borderColor", "Border Color", FirstNonEmpty(canvasObject.BorderColorOverride, canvasObject.CanvasLayer?.BorderColor), false),
                new("style.lineWeight", "Line Weight", FormatNumber(canvasObject.LineWeightOverride ?? canvasObject.CanvasLayer?.LineWeight), false),
                new("style.lineStyle", "Line Style", FirstNonEmpty(canvasObject.LineStyleOverride, canvasObject.CanvasLayer?.LineStyle), false),
                new("style.fillColor", "Fill Color", FirstNonEmpty(canvasObject.FillColorOverride, canvasObject.CanvasLayer?.FillColor), false),
                new("style.fillTransparency", "Fill Transparency", (canvasObject.FillTransparencyOverride ?? canvasObject.CanvasLayer?.FillTransparency)?.ToString(), false)
            ]);
        }

        private static ObjectPropertySection BuildSourceSection(CanvasObject canvasObject, CadastralCanvasMetadata? metadata)
        {
            return new ObjectPropertySection("Source",
            [
                new("source.format", "Source Format", metadata?.SourceFormat, false),
                new("source.file", "Source File", metadata?.SourceFileName, false),
                new("source.layer", "Source Layer", metadata?.SourceLayer, false),
                new("source.matchedText", "Matched Text", metadata?.MatchedText, false),
                new("source.handle", "Source Handle", FirstNonEmpty(metadata?.SourceHandle, canvasObject.SourceDxfHandle), false),
                new("source.importedAt", "Imported At", FormatDate(metadata?.ImportedAt), false)
            ]);
        }

        private static void AddOptionalSection(List<ObjectPropertySection> sections, ObjectPropertySection? section)
        {
            if (section?.Rows == null)
                return;

            List<ObjectPropertyRow> rows = section.Rows
                .Where(row => row != null)
                .ToList();
            if (rows.Count == 0)
                return;

            if (rows.Any(row => row.IncludeWhenEmpty || !string.IsNullOrWhiteSpace(row.Value)))
            {
                sections.Add(section with { Rows = rows });
            }
        }

        private static string NormalizePropertySectionTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "Properties";

            // Replace delimiters with spaces and convert to title case for display
            string cleaned = title.Replace('.', ' ').Replace('_', ' ').Trim();
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
        }

        private async void btnConfigureParcelProperties_Click(object? sender, EventArgs e)
        {
            IReadOnlyList<CanvasObject> contextObjects = GetPropertyFieldContextObjects();
            string profileKey = GetObjectTypeProfileKey(contextObjects);
            List<string> currentProfile = GetOrCreateProfile(profileKey);

            HashSet<string> relevantFieldKeys = GetRelevantFieldKeys(contextObjects);
            var fieldOptions = GetParcelPropertyFields()
                .Where(field => relevantFieldKeys.Contains(field.Key))
                .Select(field => new ParcelPropertyFieldSelectorItem(field.Key, field.Category, field.Label))
                .ToList();

            var validKeys = fieldOptions.Select(f => f.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var filteredVisible = currentProfile.Where(k => validKeys.Contains(k)).ToList();

            using var form = new frmParcelPropertyFieldSelector(fieldOptions, filteredVisible);
            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            _propertyFieldProfiles[profileKey] = [.. form.SelectedFieldKeys];
            SavePropertyFieldProfiles();

            if (_currentPropertyGridObjects.Count > 0)
            {
                PopulatePropertyGridForSelectedComboItem();
                return;
            }

            await LoadSelectedParcelPropertiesAsync(_currentSelectedCanvasObjectIds);
        }

        private static string GetObjectTypeProfileKey(IReadOnlyList<CanvasObject> objects)
        {
            if (objects.Count == 0)
                return "general";

            bool hasParcel = objects.Any(IsOriginalParcelObject);
            bool hasRoad = objects.Any(o => o.Road != null || o.RoadId.HasValue);
            bool hasBlock = objects.Any(IsBlockObject);
            bool hasReplotted = objects.Any(o => o.ReplottedParcel != null || o.ReplottedParcelId.HasValue);
            bool hasText = objects.Any(IsTextObject);
            bool hasExternalReference = objects.Any(IsExternalReferenceObject);
            bool hasUnclassified = objects.Any(o =>
                !IsOriginalParcelObject(o) &&
                !(o.Road != null || o.RoadId.HasValue) &&
                !IsBlockObject(o) &&
                !(o.ReplottedParcel != null || o.ReplottedParcelId.HasValue) &&
                !IsTextObject(o) &&
                !IsExternalReferenceObject(o));

            int distinctTypes = (hasParcel ? 1 : 0) + (hasRoad ? 1 : 0) + (hasBlock ? 1 : 0)
                              + (hasReplotted ? 1 : 0) + (hasText ? 1 : 0) + (hasExternalReference ? 1 : 0)
                              + (hasUnclassified ? 1 : 0);
            if (distinctTypes != 1)
                return "general";

            if (hasParcel) return "originalParcel";
            if (hasRoad) return "road";
            if (hasBlock) return "block";
            if (hasReplotted) return "replottedParcel";
            if (hasText) return "text";
            if (hasExternalReference) return "externalReference";
            return "general";
        }

        private List<string> GetOrCreateProfile(string profileKey)
        {
            if (!_propertyFieldProfiles.TryGetValue(profileKey, out List<string>? profile))
            {
                profile = DefaultPropertyFieldProfiles.TryGetValue(profileKey, out List<string>? defaults)
                    ? new List<string>(defaults)
                    : new List<string>(DefaultPropertyFieldProfiles["general"]);
                _propertyFieldProfiles[profileKey] = profile;
            }

            return profile;
        }

        private void NormalizeCommonPropertyFieldProfiles()
        {
            foreach (string profileKey in DefaultPropertyFieldProfiles.Keys)
            {
                List<string> profile = GetOrCreateProfile(profileKey);
                if (string.Equals(profileKey, "externalReference", StringComparison.OrdinalIgnoreCase))
                {
                    profile.RemoveAll(key => string.Equals(key, "selection.assignment", StringComparison.OrdinalIgnoreCase));
                    AddMissingPropertyFieldKeys(profile,
                    [
                        "selection.selectedObjects",
                        "selection.objectType",
                        "selection.layer"
                    ]);
                }
                else
                {
                    AddMissingPropertyFieldKeys(profile, DefaultObjectPropertyFieldKeys);
                }

                AddMissingPropertyFieldKeys(profile, DefaultGeometryPropertyFieldKeys);
            }
        }

        private static void AddMissingPropertyFieldKeys(
            List<string> profile,
            IEnumerable<string> fieldKeys)
        {
            HashSet<string> existing = new(profile, StringComparer.OrdinalIgnoreCase);
            foreach (string fieldKey in fieldKeys)
            {
                if (existing.Add(fieldKey))
                {
                    profile.Add(fieldKey);
                }
            }
        }

        private void LoadPropertyFieldProfiles()
        {
            try
            {
                if (!File.Exists(PropertyFieldProfilesFilePath))
                    return;

                string json = File.ReadAllText(PropertyFieldProfilesFilePath);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
                if (loaded == null)
                    return;

                foreach (var (key, value) in loaded)
                {
                    if (value != null)
                        _propertyFieldProfiles[key] = value;
                }
            }
            catch
            {
                // Use defaults if saved profiles cannot be loaded.
            }

            NormalizeCommonPropertyFieldProfiles();
        }

        private void SavePropertyFieldProfiles()
        {
            try
            {
                string? dir = Path.GetDirectoryName(PropertyFieldProfilesFilePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                string json = JsonSerializer.Serialize(
                    _propertyFieldProfiles,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(PropertyFieldProfilesFilePath, json);
            }
            catch
            {
                // Profile saving is non-critical; ignore errors.
            }
        }

        private async void btnSelectFromRecords_Click(object? sender, EventArgs e)
        {
            if (!EnsureProjectOpenForSelection("Select From Records"))
                return;

            try
            {
                List<ObjectRecordSelectorItem> originalParcels = await LoadOriginalParcelRecordSelectorItemsAsync();
                List<ObjectRecordSelectorItem> replottedParcels = await LoadReplottedParcelRecordSelectorItemsAsync();
                List<ObjectRecordSelectorItem> blocks = await LoadBlockRecordSelectorItemsAsync();
                List<ObjectRecordSelectorItem> roads = await LoadRoadRecordSelectorItemsAsync();
                using var form = new frmObjectRecordSelector(
                    originalParcels,
                    replottedParcels,
                    blocks,
                    roads,
                    _currentSelectedCanvasObjectIds);
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                mapCanvasControlMain.ApplyCanvasSelection(
                    form.SelectedCanvasObjectIds,
                    CanvasSelectionApplyMode.Create,
                    form.ZoomToSelection && form.SelectedCanvasObjectIds.Count > 0);
                SetCanvasCommandStatus(form.SelectedCanvasObjectIds.Count == 0
                    ? "Cleared record-based selection"
                    : $"Selected {form.SelectedCanvasObjectIds.Count:N0} object(s) from records");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not load record selector: {ex.Message}",
                    "Select From Records",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static async Task<List<ObjectRecordSelectorItem>> LoadOriginalParcelRecordSelectorItemsAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<BaselineParcel> parcels = await context.BaselineParcels
                .AsNoTracking()
                .Include(parcel => parcel.LandOwner)
                .OrderBy(parcel => parcel.MapSheetNo)
                .ThenBy(parcel => parcel.ParcelNo)
                .ToListAsync();

            List<int> parcelIds = parcels
                .Select(parcel => parcel.Id)
                .ToList();
            List<Guid> linkedCanvasObjectIds = parcels
                .Where(parcel => parcel.CanvasObjectId.HasValue)
                .Select(parcel => parcel.CanvasObjectId!.Value)
                .ToList();

            List<CanvasObject> linkedObjects = await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    (item.BaselineParcelId.HasValue && parcelIds.Contains(item.BaselineParcelId.Value)) ||
                    linkedCanvasObjectIds.Contains(item.Id))
                .ToListAsync();

            Dictionary<int, CanvasObject> objectsByParcelId = linkedObjects
                .Where(item => item.BaselineParcelId.HasValue)
                .GroupBy(item => item.BaselineParcelId!.Value)
                .ToDictionary(group => group.Key, group => group.First());
            Dictionary<int, Guid[]> objectIdsByParcelId = linkedObjects
                .Where(item => item.BaselineParcelId.HasValue)
                .GroupBy(item => item.BaselineParcelId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.Id).Distinct().ToArray());
            Dictionary<Guid, CanvasObject> objectsById = linkedObjects
                .GroupBy(item => item.Id)
                .ToDictionary(group => group.Key, group => group.First());

            return parcels
                .Select(parcel =>
                {
                    CanvasObject? canvasObject = null;
                    if (parcel.CanvasObjectId.HasValue)
                        objectsById.TryGetValue(parcel.CanvasObjectId.Value, out canvasObject);
                    canvasObject ??= objectsByParcelId.GetValueOrDefault(parcel.Id);
                    IReadOnlyList<Guid> canvasObjectIds = ResolveLinkedObjectIds(
                        parcel.CanvasObjectId,
                        objectIdsByParcelId.GetValueOrDefault(parcel.Id));

                    return new ObjectRecordSelectorItem(
                        parcel.Id,
                        parcel.MapSheetNo,
                        parcel.ParcelNo,
                        string.IsNullOrWhiteSpace(parcel.FullUniqueParcelCode)
                            ? BuildParcelLookupCode(parcel.MapSheetNo, parcel.ParcelNo)
                            : parcel.FullUniqueParcelCode,
                        parcel.LandOwner?.FullName ?? string.Empty,
                        parcel.OriginalAreaSqm,
                        canvasObjectIds,
                        canvasObject?.CanvasLayer?.Name,
                        GetAreaPrecisionSettingsStatic().SqmPrecision);
                })
                .ToList();
        }

        private bool EnsureProjectOpenForSelection(string title)
        {
            if (AppServices.HasContext)
                return true;

            MessageBox.Show(
                this,
                "Please open or create a project first.",
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return false;
        }

        private void ApplyObjectTypeSelection(IReadOnlyList<Guid> ids, CanvasSelectionApplyMode mode)
        {
            mapCanvasControlMain.ApplyCanvasSelection(ids, mode, zoomToSelection: mode == CanvasSelectionApplyMode.Create);
            SetCanvasCommandStatus(mode == CanvasSelectionApplyMode.Remove
                ? $"Deselected {ids.Count:N0} object(s)."
                : $"Selected {ids.Count:N0} object(s).");
        }

        private static async Task<List<ObjectTypeSelectorItem>> LoadBlockSelectorItemsAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<Core.Entities.Layout.Block> blocks = await context.Blocks
                .AsNoTracking()
                .OrderBy(block => block.BlockName)
                .ToListAsync();
            Dictionary<int, Guid[]> objectIdsByBlockId = await LoadCanvasObjectIdsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.Block);

            return blocks
                .Select(block => new ObjectTypeSelectorItem(
                    block.Id,
                    string.IsNullOrWhiteSpace(block.BlockName) ? $"Block {block.Id}" : block.BlockName,
                    ResolveLinkedObjectIds(block.CanvasObjectId, objectIdsByBlockId.GetValueOrDefault(block.Id))))
                .ToList();
        }

        private static async Task<List<ObjectTypeSelectorItem>> LoadRoadSelectorItemsAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<Core.Entities.Layout.Road> roads = await context.Roads
                .AsNoTracking()
                .OrderBy(road => road.RoadName)
                .ToListAsync();
            Dictionary<int, Guid[]> objectIdsByRoadId = await LoadCanvasObjectIdsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.Road);

            return roads
                .Select(road => new ObjectTypeSelectorItem(
                    road.Id,
                    string.IsNullOrWhiteSpace(road.RoadName) ? $"Road {road.Id}" : road.RoadName,
                    ResolveLinkedObjectIds(road.CanvasObjectId, objectIdsByRoadId.GetValueOrDefault(road.Id))))
                .ToList();
        }

        private static async Task<List<ObjectRecordSelectorItem>> LoadReplottedParcelRecordSelectorItemsAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<Core.Entities.Replotting.ReplottedParcel> parcels = await context.ReplottedParcels
                .AsNoTracking()
                .Include(parcel => parcel.Block)
                .Include(parcel => parcel.PlotType)
                .OrderBy(parcel => parcel.BlockSequenceNumber)
                .ThenBy(parcel => parcel.SystemGeneratedNumber)
                .ToListAsync();
            Dictionary<int, CanvasObject> objectsByRecordId = await LoadCanvasObjectsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.ReplottedParcel);
            Dictionary<int, Guid[]> objectIdsByRecordId = await LoadCanvasObjectIdsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.ReplottedParcel);
            Dictionary<Guid, CanvasObject> objectsById = await LoadCanvasObjectsByIdsAsync(
                context,
                parcels.Where(parcel => parcel.CanvasObjectId.HasValue).Select(parcel => parcel.CanvasObjectId!.Value));

            return parcels
                .Select(parcel =>
                {
                    CanvasObject? canvasObject = null;
                    if (parcel.CanvasObjectId.HasValue)
                        objectsById.TryGetValue(parcel.CanvasObjectId.Value, out canvasObject);
                    canvasObject ??= objectsByRecordId.GetValueOrDefault(parcel.Id);
                    IReadOnlyList<Guid> canvasObjectIds = ResolveLinkedObjectIds(
                        parcel.CanvasObjectId,
                        objectIdsByRecordId.GetValueOrDefault(parcel.Id));

                    string plotNo = FirstNonEmpty(
                        parcel.BlockSequenceNumber,
                        parcel.SystemGeneratedNumber,
                        parcel.DerivedNumber,
                        parcel.Id.ToString()) ?? parcel.Id.ToString();
                    return new ObjectRecordSelectorItem(
                        ObjectRecordSelectorCategory.ReplottedParcel,
                        parcel.Id,
                        plotNo,
                        parcel.Block?.BlockName ?? "--",
                        parcel.PlotType?.TypeName ?? "--",
                        parcel.Id.ToString(),
                        parcel.PlotAreaSqm,
                        canvasObjectIds,
                        canvasObject?.CanvasLayer?.Name,
                        GetAreaPrecisionSettingsStatic().SqmPrecision);
                })
                .ToList();
        }

        private static async Task<List<ObjectRecordSelectorItem>> LoadBlockRecordSelectorItemsAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<Core.Entities.Layout.Block> blocks = await context.Blocks
                .AsNoTracking()
                .OrderBy(block => block.BlockName)
                .ToListAsync();
            Dictionary<int, CanvasObject> objectsByRecordId = await LoadCanvasObjectsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.Block);
            Dictionary<int, Guid[]> objectIdsByRecordId = await LoadCanvasObjectIdsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.Block);
            Dictionary<Guid, CanvasObject> objectsById = await LoadCanvasObjectsByIdsAsync(
                context,
                blocks.Where(block => block.CanvasObjectId.HasValue).Select(block => block.CanvasObjectId!.Value));

            return blocks
                .Select(block =>
                {
                    CanvasObject? canvasObject = null;
                    if (block.CanvasObjectId.HasValue)
                        objectsById.TryGetValue(block.CanvasObjectId.Value, out canvasObject);
                    canvasObject ??= objectsByRecordId.GetValueOrDefault(block.Id);
                    IReadOnlyList<Guid> canvasObjectIds = ResolveLinkedObjectIds(
                        block.CanvasObjectId,
                        objectIdsByRecordId.GetValueOrDefault(block.Id));

                    return new ObjectRecordSelectorItem(
                        ObjectRecordSelectorCategory.Block,
                        block.Id,
                        block.BlockName,
                        block.BlockCode ?? "--",
                        block.BlockLandUse ?? "--",
                        block.Id.ToString(),
                        block.BlockArea,
                        canvasObjectIds,
                        canvasObject?.CanvasLayer?.Name,
                        GetAreaPrecisionSettingsStatic().SqmPrecision);
                })
                .ToList();
        }

        private static async Task<List<ObjectRecordSelectorItem>> LoadRoadRecordSelectorItemsAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<Core.Entities.Layout.Road> roads = await context.Roads
                .AsNoTracking()
                .OrderBy(road => road.RoadName)
                .ToListAsync();
            Dictionary<int, CanvasObject> objectsByRecordId = await LoadCanvasObjectsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.Road);
            Dictionary<int, Guid[]> objectIdsByRecordId = await LoadCanvasObjectIdsByRecordIdAsync(
                context,
                ObjectRecordSelectorCategory.Road);
            Dictionary<Guid, CanvasObject> objectsById = await LoadCanvasObjectsByIdsAsync(
                context,
                roads.Where(road => road.CanvasObjectId.HasValue).Select(road => road.CanvasObjectId!.Value));

            return roads
                .Select(road =>
                {
                    CanvasObject? canvasObject = null;
                    if (road.CanvasObjectId.HasValue)
                        objectsById.TryGetValue(road.CanvasObjectId.Value, out canvasObject);
                    canvasObject ??= objectsByRecordId.GetValueOrDefault(road.Id);
                    IReadOnlyList<Guid> canvasObjectIds = ResolveLinkedObjectIds(
                        road.CanvasObjectId,
                        objectIdsByRecordId.GetValueOrDefault(road.Id));

                    return new ObjectRecordSelectorItem(
                        ObjectRecordSelectorCategory.Road,
                        road.Id,
                        road.RoadName,
                        road.RoadCode ?? "--",
                        FirstNonEmpty(road.RoadType, road.RoadStatus, "--") ?? "--",
                        road.Id.ToString(),
                        road.RoadWidth,
                        canvasObjectIds,
                        canvasObject?.CanvasLayer?.Name,
                        GetAreaPrecisionSettingsStatic().SqmPrecision);
                })
                .ToList();
        }

        private static async Task<List<SelectionAttributeLayer>> LoadSelectionAttributeLayersAsync()
        {
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            List<CanvasObject> objects = await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .Include(item => item.BaselineParcel)
                    .ThenInclude(parcel => parcel!.LandOwner)
                .Include(item => item.BaselineParcel)
                    .ThenInclude(parcel => parcel!.MalpotReference)
                .Include(item => item.BaselineParcel)
                    .ThenInclude(parcel => parcel!.ParcelContributionSummary)
                .Include(item => item.Road)
                .Include(item => item.Block)
                .Include(item => item.ReplottedParcel)
                    .ThenInclude(parcel => parcel!.Block)
                .Include(item => item.ReplottedParcel)
                    .ThenInclude(parcel => parcel!.PlotType)
                .Where(item => item.CanvasLayer != null && item.CanvasLayer.IsVisible && item.IsVisible)
                .OrderBy(item => item.CanvasLayer.Name)
                .ThenBy(item => item.ObjectType)
                .ToListAsync();

            await LoadFallbackBaselineParcelsForSelectionAsync(context, objects);
            await LoadFallbackRoadsForSelectionAsync(context, objects);
            await LoadFallbackBlocksForSelectionAsync(context, objects);
            await LoadFallbackReplottedParcelsForSelectionAsync(context, objects);

            return objects
                .Where(item => item.CanvasLayer != null)
                .GroupBy(GetSelectionAttributeLayerKey)
                .Select(group => new SelectionAttributeLayer(
                    group.Key.LayerId,
                    group.Key.Name,
                    group.Any(item => item.CanvasLayer?.IsSelectable == true),
                    group.Select(BuildSelectionAttributeRow)))
                .ToList();
        }

        private static SelectionAttributeLayerKey GetSelectionAttributeLayerKey(CanvasObject canvasObject)
        {
            CanvasLayer layer = canvasObject.CanvasLayer!;
            return IsCadastralCanvasLayer(layer)
                ? new SelectionAttributeLayerKey(null, "Cadastral Map")
                : new SelectionAttributeLayerKey(layer.Id, layer.Name);
        }

        private readonly record struct SelectionAttributeLayerKey(int? LayerId, string Name);

        private static SelectionAttributeRow BuildSelectionAttributeRow(CanvasObject canvasObject)
        {
            CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
            CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);
            Dictionary<string, object?> values = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> labels = new(StringComparer.OrdinalIgnoreCase);

            AddStringAttribute(values, labels, "object.id", "Object ID", canvasObject.Id.ToString());
            AddStringAttribute(values, labels, "object.type", "Object Type", canvasObject.ObjectType);
            AddStringAttribute(values, labels, "object.description", "Description", canvasObject.ObjectDescription);
            AddStringAttribute(values, labels, "layer.name", "Layer", canvasObject.CanvasLayer?.Name);
            AddStringAttribute(values, labels, "layer.type", "Layer Type", canvasObject.CanvasLayer?.LayerType);
            AddBooleanAttribute(values, labels, "canvas.visible", "Visible", canvasObject.IsVisible);
            AddBooleanAttribute(values, labels, "canvas.locked", "Locked", canvasObject.IsLocked || canvasObject.CanvasLayer?.IsLocked == true);
            AddNumberAttribute(values, labels, "geometry.areaSqm", "Geometry Area (sq.m)", CanvasGeometryMetricsService.GetArea(canvasObject));
            AddNumberAttribute(values, labels, "geometry.length", "Geometry Length / Perimeter", CanvasGeometryMetricsService.GetLength(canvasObject));
            AddNumberAttribute(values, labels, "geometry.vertexCount", "Geometry Vertex Count", CanvasGeometryMetricsService.GetVertexCount(canvasObject));

            AddOriginalParcelAttributes(values, labels, canvasObject, assignedMetadata);
            AddReplottedParcelAttributes(values, labels, canvasObject);
            AddRoadAttributes(values, labels, canvasObject);
            AddBlockAttributes(values, labels, canvasObject);

            return new SelectionAttributeRow(canvasObject.Id, values, labels);
        }

        private static void AddOriginalParcelAttributes(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            CanvasObject canvasObject,
            CadastralCanvasMetadata? assignedMetadata)
        {
            BaselineParcel? parcel = canvasObject.BaselineParcel;
            if (parcel == null && assignedMetadata == null && !canvasObject.BaselineParcelId.HasValue)
                return;

            AddNumberAttribute(values, labels, "parcel.recordId", "Original Parcel Record ID", parcel?.Id ?? canvasObject.BaselineParcelId);
            AddStringAttribute(values, labels, "parcel.parcelNo", "Parcel No.", FirstNonEmpty(parcel?.ParcelNo, assignedMetadata?.ParcelNo));
            AddStringAttribute(values, labels, "parcel.mapSheetNo", "Map Sheet No.", FirstNonEmpty(parcel?.MapSheetNo, assignedMetadata?.MapSheetNo));
            AddStringAttribute(values, labels, "parcel.uniqueCode", "Unique Parcel Code", FirstNonEmpty(parcel?.FullUniqueParcelCode, assignedMetadata?.FullUniqueParcelCode));
            AddStringAttribute(values, labels, "parcel.landUse", "Land Use", FirstNonEmpty(parcel?.LandUse, assignedMetadata?.LandUse));
            AddStringAttribute(values, labels, "parcel.ownershipType", "Ownership Type", parcel?.LandOwnershipType);
            AddStringAttribute(values, labels, "parcel.remarks", "Remarks", parcel?.Remarks);
            AddStringAttribute(values, labels, "owner.name", "Owner Name", FirstNonEmpty(parcel?.LandOwner?.FullName, assignedMetadata?.OwnerName));
            AddStringAttribute(values, labels, "owner.fatherSpouse", "Father / Spouse", parcel?.LandOwner?.FatherOrSpouseName);
            AddStringAttribute(values, labels, "owner.gender", "Gender", parcel?.LandOwner?.Gender);
            AddStringAttribute(values, labels, "owner.citizenshipNo", "Citizenship No.", parcel?.LandOwner?.CitizenshipNumber);
            AddStringAttribute(values, labels, "location.province", "Province", parcel?.Province);
            AddStringAttribute(values, labels, "location.district", "District", parcel?.District);
            AddStringAttribute(values, labels, "location.municipality", "Municipality", parcel?.Municipality);
            AddStringAttribute(values, labels, "location.wardNo", "Ward No.", parcel?.WardNo);
            AddStringAttribute(values, labels, "landRecord.mothNo", "Moth No.", parcel?.MalpotReference?.MothNo);
            AddStringAttribute(values, labels, "landRecord.paanaNo", "Paana No.", parcel?.MalpotReference?.PaanaNo);
            AddBooleanAttribute(values, labels, "tenancy.hasTenant", "Has Tenant", parcel?.HasTenant);
            AddStringAttribute(values, labels, "tenancy.tenantName", "Tenant Name", parcel?.TenantName);

            double? originalAreaSqm = parcel?.OriginalAreaSqm ?? assignedMetadata?.RecordAreaSqm;
            AddAreaAttributes(values, labels, "parcel.originalArea", "Original Area", originalAreaSqm);
            AddAreaAttributes(values, labels, "parcel.fieldMeasuredArea", "Field Measured Area", parcel?.FieldMeasuredAreaSqm);
            AddAreaAttributes(values, labels, "parcel.effectiveArea", "Effective Area", parcel?.EffectiveAreaSqm);
            AddBooleanAttribute(values, labels, "parcel.effectiveAreaManual", "Effective Area Manual", parcel?.IsEffectiveAreaManual);

            Core.Entities.Contribution.ParcelContributionSummary? summary = parcel?.ParcelContributionSummary;
            AddNumberAttribute(values, labels, "contribution.generalSqm", "General Contribution (sq.m)", summary?.TotalGeneralContributionSqm);
            AddNumberAttribute(values, labels, "contribution.specificSqm", "Specific Contribution (sq.m)", summary?.TotalSpecificContributionSqm);
            AddNumberAttribute(values, labels, "contribution.totalSqm", "Total Contribution (sq.m)", summary?.TotalContributionSqm);
            AddNumberAttribute(values, labels, "contribution.percent", "Contribution Percent", summary?.TotalContributionPercent);
            AddNumberAttribute(values, labels, "contribution.netReturnableSqm", "Net Returnable Area (sq.m)", summary?.NetReturnableAreaSqm);
            AddNumberAttribute(values, labels, "contribution.replottedAssignedSqm", "Replotted Area Assigned (sq.m)", summary?.ReplottedAreaAssignedSqm);
        }

        private static void AddReplottedParcelAttributes(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            CanvasObject canvasObject)
        {
            Core.Entities.Replotting.ReplottedParcel? parcel = canvasObject.ReplottedParcel;
            if (parcel == null && !canvasObject.ReplottedParcelId.HasValue)
                return;

            AddNumberAttribute(values, labels, "replotted.recordId", "Replotted Parcel Record ID", parcel?.Id ?? canvasObject.ReplottedParcelId);
            AddStringAttribute(values, labels, "replotted.systemNumber", "System Number", parcel?.SystemGeneratedNumber);
            AddStringAttribute(values, labels, "replotted.derivedNumber", "Derived Number", parcel?.DerivedNumber);
            AddStringAttribute(values, labels, "replotted.blockSequence", "Block Sequence Number", parcel?.BlockSequenceNumber);
            AddStringAttribute(values, labels, "replotted.activeNumberType", "Active Number Type", parcel?.ActiveNumberType);
            AddStringAttribute(values, labels, "replotted.plotType", "Plot Type", parcel?.PlotType?.TypeName);
            AddStringAttribute(values, labels, "replotted.plotTypeCode", "Plot Type Code", parcel?.PlotType?.TypeCode);
            AddStringAttribute(values, labels, "replotted.block", "Block", parcel?.Block?.BlockName);
            AddAreaAttributes(values, labels, "replotted.plotArea", "Plot Area", parcel?.PlotAreaSqm);
            AddStringAttribute(values, labels, "replotted.notes", "Notes", parcel?.Notes);
        }

        private static void AddRoadAttributes(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            CanvasObject canvasObject)
        {
            Core.Entities.Layout.Road? road = canvasObject.Road;
            if (road == null && !canvasObject.RoadId.HasValue)
                return;

            AddNumberAttribute(values, labels, "road.recordId", "Road Record ID", road?.Id ?? canvasObject.RoadId);
            AddStringAttribute(values, labels, "road.name", "Road Name", road?.RoadName);
            AddStringAttribute(values, labels, "road.code", "Road Code", road?.RoadCode);
            AddStringAttribute(values, labels, "road.status", "Road Status", road?.RoadStatus);
            AddStringAttribute(values, labels, "road.type", "Road Type", road?.RoadType);
            AddStringAttribute(values, labels, "road.surface", "Surface Type", road?.SurfaceType);
            AddNumberAttribute(values, labels, "road.width", "Road Width", road?.RoadWidth);
            AddNumberAttribute(values, labels, "road.rightOfWayWidth", "Right Of Way Width", road?.RightOfWayWidth);
            AddStringAttribute(values, labels, "road.description", "Road Description", road?.Description);
        }

        private static void AddBlockAttributes(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            CanvasObject canvasObject)
        {
            Core.Entities.Layout.Block? block = canvasObject.Block;
            if (block == null && !canvasObject.BlockId.HasValue)
                return;

            AddNumberAttribute(values, labels, "block.recordId", "Block Record ID", block?.Id ?? canvasObject.BlockId);
            AddStringAttribute(values, labels, "block.name", "Block Name", block?.BlockName);
            AddStringAttribute(values, labels, "block.code", "Block Code", block?.BlockCode);
            AddStringAttribute(values, labels, "block.landUse", "Block Land Use", block?.BlockLandUse);
            AddNumberAttribute(values, labels, "block.depth", "Block Depth", block?.BlockDepth);
            AddNumberAttribute(values, labels, "block.length", "Block Length", block?.BlockLength);
            AddAreaAttributes(values, labels, "block.area", "Block Area", block?.BlockArea);
            AddStringAttribute(values, labels, "block.description", "Block Description", block?.Description);
        }

        private static void AddAreaAttributes(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            string keyPrefix,
            string labelPrefix,
            double? areaSqm)
        {
            AddNumberAttribute(values, labels, $"{keyPrefix}Sqm", $"{labelPrefix} (sq.m)", areaSqm);
            if (!areaSqm.HasValue)
                return;

            AddNumberAttribute(values, labels, $"{keyPrefix}Ropani", $"{labelPrefix} (Ropani)", AreaConverterService.SqmToRopani(areaSqm.Value, 6));
            AddNumberAttribute(values, labels, $"{keyPrefix}Aana", $"{labelPrefix} (Aana)", AreaConverterService.SqmToAana(areaSqm.Value, 6));
            AddNumberAttribute(values, labels, $"{keyPrefix}Paisa", $"{labelPrefix} (Paisa)", AreaConverterService.SqmToPaisa(areaSqm.Value, 6));
            AddNumberAttribute(values, labels, $"{keyPrefix}Dam", $"{labelPrefix} (Dam)", AreaConverterService.SqmToDam(areaSqm.Value, 6));
            AddNumberAttribute(values, labels, $"{keyPrefix}Bigha", $"{labelPrefix} (Bigha)", AreaConverterService.SqmToBigha(areaSqm.Value, 6));
            AddNumberAttribute(values, labels, $"{keyPrefix}Kattha", $"{labelPrefix} (Kattha)", AreaConverterService.SqmToKattha(areaSqm.Value, 6));
            AddNumberAttribute(values, labels, $"{keyPrefix}Dhur", $"{labelPrefix} (Dhur)", AreaConverterService.SqmToDhur(areaSqm.Value, 6));
        }

        private static void AddStringAttribute(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            string key,
            string label,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            values[key] = value.Trim();
            labels[key] = label;
        }

        private static void AddBooleanAttribute(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            string key,
            string label,
            bool? value)
        {
            if (!value.HasValue)
                return;

            values[key] = value.Value;
            labels[key] = label;
        }

        private static void AddNumberAttribute(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            string key,
            string label,
            double? value)
        {
            if (!value.HasValue || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
                return;

            values[key] = value.Value;
            labels[key] = label;
        }

        private static void AddNumberAttribute(
            IDictionary<string, object?> values,
            IDictionary<string, string> labels,
            string key,
            string label,
            int? value)
        {
            if (!value.HasValue)
                return;

            values[key] = value.Value;
            labels[key] = label;
        }

        private static async Task<Dictionary<int, CanvasObject>> LoadCanvasObjectsByRecordIdAsync(
            AppDbContext context,
            ObjectRecordSelectorCategory category)
        {
            IQueryable<CanvasObject> query = context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer);

            query = category switch
            {
                ObjectRecordSelectorCategory.ReplottedParcel => query.Where(item => item.ReplottedParcelId.HasValue),
                ObjectRecordSelectorCategory.Block => query.Where(item => item.BlockId.HasValue),
                ObjectRecordSelectorCategory.Road => query.Where(item => item.RoadId.HasValue),
                _ => query.Where(item => item.BaselineParcelId.HasValue)
            };

            List<CanvasObject> objects = await query.ToListAsync();
            return objects
                .GroupBy(item => category switch
                {
                    ObjectRecordSelectorCategory.ReplottedParcel => item.ReplottedParcelId!.Value,
                    ObjectRecordSelectorCategory.Block => item.BlockId!.Value,
                    ObjectRecordSelectorCategory.Road => item.RoadId!.Value,
                    _ => item.BaselineParcelId!.Value
                })
                .ToDictionary(group => group.Key, group => group.First());
        }

        private static async Task<Dictionary<int, Guid[]>> LoadCanvasObjectIdsByRecordIdAsync(
            AppDbContext context,
            ObjectRecordSelectorCategory category)
        {
            IQueryable<CanvasObject> query = context.CanvasObjects.AsNoTracking();
            query = category switch
            {
                ObjectRecordSelectorCategory.ReplottedParcel => query.Where(item => item.ReplottedParcelId.HasValue),
                ObjectRecordSelectorCategory.Block => query.Where(item => item.BlockId.HasValue),
                ObjectRecordSelectorCategory.Road => query.Where(item => item.RoadId.HasValue),
                _ => query.Where(item => item.BaselineParcelId.HasValue)
            };

            List<CanvasObject> objects = await query.ToListAsync();
            return objects
                .GroupBy(item => category switch
                {
                    ObjectRecordSelectorCategory.ReplottedParcel => item.ReplottedParcelId!.Value,
                    ObjectRecordSelectorCategory.Block => item.BlockId!.Value,
                    ObjectRecordSelectorCategory.Road => item.RoadId!.Value,
                    _ => item.BaselineParcelId!.Value
                })
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(item => item.Id).Distinct().ToArray());
        }

        private static async Task<Dictionary<Guid, CanvasObject>> LoadCanvasObjectsByIdsAsync(
            AppDbContext context,
            IEnumerable<Guid> ids)
        {
            Guid[] idArray = ids.Where(id => id != Guid.Empty).Distinct().ToArray();
            if (idArray.Length == 0)
                return [];

            return await context.CanvasObjects
                .AsNoTracking()
                .Include(item => item.CanvasLayer)
                .Where(item => idArray.Contains(item.Id))
                .ToDictionaryAsync(item => item.Id);
        }

        private static IReadOnlyList<Guid> ResolveLinkedObjectIds(Guid? directCanvasObjectId, IReadOnlyList<Guid>? fallbackIds)
        {
            return (directCanvasObjectId.HasValue
                    ? new[] { directCanvasObjectId.Value }
                    : [])
                .Concat(fallbackIds ?? [])
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();
        }

        private void ClearCurrentPropertyGridSelection()
        {
            _currentPropertyGridObjects = Array.Empty<CanvasObject>();
            _currentPropertyGridSelectedCount = 0;
            _suppressSelectedPropertyObjectChanged = true;
            try
            {
                cboSelectedPropertyObject.Items.Clear();
                cboSelectedPropertyObject.Enabled = false;
                UpdateSelectedPropertyCycleButtons();
            }
            finally
            {
                _suppressSelectedPropertyObjectChanged = false;
            }
        }

        private IReadOnlyList<CanvasObject> GetPropertyFieldContextObjects()
        {
            CanvasObject? singleObject = GetCurrentSinglePropertyGridObject();
            if (singleObject != null)
                return [singleObject];

            return _currentPropertyGridObjects;
        }

        private static HashSet<string> GetRelevantFieldKeys(IReadOnlyList<CanvasObject> objects)
        {
            IReadOnlyList<ParcelPropertyField> fields = GetParcelPropertyFields();
            if (objects.Count == 0)
            {
                return fields
                    .Select(field => field.Key)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase)
            {
                "selection.selectedObjects",
                "selection.objectType",
                "selection.layer",
                "selection.assignment"
            };

            foreach (CanvasObject canvasObject in objects)
            {
                foreach (ParcelPropertyField field in fields)
                {
                    if (IsFieldRelevantToObject(field.Key, canvasObject))
                    {
                        keys.Add(field.Key);
                    }
                }
            }

            return keys;
        }

        private static bool IsFieldRelevantToObject(string fieldKey, CanvasObject canvasObject)
        {
            if (fieldKey.StartsWith("selection.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("object.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("canvas.", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (fieldKey.StartsWith("text.", StringComparison.OrdinalIgnoreCase))
                return IsTextObject(canvasObject);

            if (fieldKey.StartsWith("road.", StringComparison.OrdinalIgnoreCase))
                return canvasObject.Road != null || canvasObject.RoadId.HasValue;

            if (fieldKey.StartsWith("block.", StringComparison.OrdinalIgnoreCase))
                return IsBlockObject(canvasObject);

            if (fieldKey.StartsWith("replotted.", StringComparison.OrdinalIgnoreCase))
                return canvasObject.ReplottedParcel != null || canvasObject.ReplottedParcelId.HasValue;

            if (fieldKey.StartsWith("parcel.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("owner.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("area.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("tenancy.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("location.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("landRecord.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("frontage.", StringComparison.OrdinalIgnoreCase) ||
                fieldKey.StartsWith("contribution.", StringComparison.OrdinalIgnoreCase))
            {
                return IsOriginalParcelObject(canvasObject);
            }

            if (fieldKey.StartsWith("geometry.", StringComparison.OrdinalIgnoreCase))
            {
                if (!ShouldShowGeometrySection(canvasObject))
                    return false;

                return fieldKey switch
                {
                    "geometry.coordinate" => IsPointGeometry(canvasObject),
                    "geometry.area" => ShouldShowArea(canvasObject),
                    "geometry.length" => !IsPointGeometry(canvasObject),
                    "geometry.vertexCount" => ShouldShowVertexCount(canvasObject),
                    _ => true
                };
            }

            if (fieldKey.StartsWith("style.", StringComparison.OrdinalIgnoreCase))
                return !IsTextObject(canvasObject);

            if (fieldKey.StartsWith("label.", StringComparison.OrdinalIgnoreCase))
                return !IsTextObject(canvasObject);

            if (fieldKey.StartsWith("source.", StringComparison.OrdinalIgnoreCase))
                return HasSourceProperties(canvasObject);

            return false;
        }
        private static IReadOnlyList<ParcelPropertyField> GetParcelPropertyFields()
        {
            return
            [
                new("selection.selectedObjects", "Selection", "Selected Objects", true,
                    (_, _, selectedCount) => selectedCount.ToString()),
                new("selection.objectType", "Selection", "Object Type", true,
                    (canvasObject, _, _) => canvasObject.ObjectType),
                new("selection.layer", "Selection", "Layer", true,
                    (canvasObject, _, _) => canvasObject.CanvasLayer?.Name),
                new("selection.assignment", "Selection", "Assignment", true,
                    (canvasObject, metadata, _) => GetAssignmentValue(canvasObject, metadata)),
                new("object.layerType", "Object", "Layer Type", false,
                    (canvasObject, _, _) => canvasObject.CanvasLayer?.LayerType),
                new("object.description", "Object", "Description", false,
                    (canvasObject, _, _) => canvasObject.ObjectDescription),
                new("object.dataLink", "Object", "Data Link", false,
                    (canvasObject, metadata, _) => GetDataLinkDisplay(canvasObject, metadata)),

                new("parcel.parcelNo", "Original Parcel", "Parcel No.", true,
                    (canvasObject, metadata, _) => FirstNonEmpty(canvasObject.BaselineParcel?.ParcelNo, GetAssignedCadastralMetadata(metadata)?.ParcelNo)),
                new("parcel.mapSheetNo", "Original Parcel", "Map Sheet No.", true,
                    (canvasObject, metadata, _) => FirstNonEmpty(canvasObject.BaselineParcel?.MapSheetNo, GetAssignedCadastralMetadata(metadata)?.MapSheetNo)),
                new("parcel.uniqueCode", "Original Parcel", "Unique Code", true,
                    (canvasObject, metadata, _) => FirstNonEmpty(canvasObject.BaselineParcel?.FullUniqueParcelCode, GetAssignedCadastralMetadata(metadata)?.FullUniqueParcelCode)),
                new("parcel.ownershipType", "Original Parcel", "Ownership Type", true,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.LandOwnershipType),
                new("parcel.recordId", "Original Parcel", "Parcel Record ID", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.Id.ToString()),
                new("parcel.landUse", "Original Parcel", "Land Use", false,
                    (canvasObject, metadata, _) => FirstNonEmpty(canvasObject.BaselineParcel?.LandUse, GetAssignedCadastralMetadata(metadata)?.LandUse)),
                new("parcel.remarks", "Original Parcel", "Remarks", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.Remarks),

                new("owner.primaryOwner", "Owner", "Primary Owner", true,
                    (canvasObject, metadata, _) => FirstNonEmpty(canvasObject.BaselineParcel?.LandOwner?.FullName, GetAssignedCadastralMetadata(metadata)?.OwnerName)),
                new("owner.coOwners", "Owner", "Co-Owners", true,
                    (canvasObject, _, _) => FormatCoOwners(canvasObject.BaselineParcel)),
                new("owner.fatherSpouse", "Owner", "Father/Spouse", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.LandOwner?.FatherOrSpouseName),
                new("owner.gender", "Owner", "Gender", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.LandOwner?.Gender),
                new("owner.citizenshipNo", "Owner", "Citizenship No.", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.LandOwner?.CitizenshipNumber),
                new("owner.contact", "Owner", "Contact", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.LandOwner?.ContactNumber),
                new("owner.email", "Owner", "Email", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.LandOwner?.Email),

                new("area.originalRecord", "Area", "Original Area (Land Records)", true,
                    (canvasObject, metadata, _) => FormatArea(canvasObject.BaselineParcel?.OriginalAreaSqm ?? GetAssignedCadastralMetadata(metadata)?.RecordAreaSqm)),
                new("area.fieldMeasured", "Area", "Area (Measured in Field)", true,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.FieldMeasuredAreaSqm)),
                new("area.effective", "Area", "Effective Area", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.EffectiveAreaSqm)),
                new("area.effectiveMode", "Area", "Effective Area Mode", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel == null
                        ? null
                        : canvasObject.BaselineParcel.IsEffectiveAreaManual ? "Manual" : "Calculated"),
                new("area.imported", "Area", "Imported Area", false,
                    (_, metadata, _) => FormatArea(metadata?.CalculatedAreaSqm)),

                new("geometry.type", "Geometry", "Geometry Type", false,
                    (canvasObject, _, _) => canvasObject.Shape?.GeometryType ?? canvasObject.ObjectType),
                new("geometry.length", "Geometry", "Length / Perimeter", true,
                    (canvasObject, _, _) => FormatLength(CanvasGeometryMetricsService.GetLength(canvasObject))),
                new("geometry.area", "Geometry", "Area", true,
                    (canvasObject, _, _) => FormatArea(CanvasGeometryMetricsService.GetArea(canvasObject))),
                new("geometry.vertexCount", "Geometry", "Points / Vertices", true,
                    (canvasObject, _, _) => ShouldShowVertexCount(canvasObject)
                        ? CanvasGeometryMetricsService.GetVertexCount(canvasObject)?.ToString("N0")
                        : null),
                new("geometry.bounds", "Geometry", "Bounds", true,
                    (canvasObject, _, _) => FormatGeometryBounds(canvasObject.Shape?.EnvelopeInternal)),
                new("geometry.coordinate", "Geometry", "Coordinate", false,
                    (canvasObject, _, _) => FormatPoint(canvasObject.Shape?.Coordinate)),

                new("tenancy.hasTenant", "Tenancy", "Has Tenant", true,
                    (canvasObject, _, _) => canvasObject.BaselineParcel == null ? null : FormatBoolean(canvasObject.BaselineParcel.HasTenant)),
                new("tenancy.tenantName", "Tenancy", "Name of Tenant", true,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.TenantName),

                new("location.province", "Location", "Province", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.Province),
                new("location.district", "Location", "District", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.District),
                new("location.municipality", "Location", "Municipality", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.Municipality),
                new("location.ward", "Location", "Ward No.", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.WardNo),

                new("landRecord.mothNo", "Land Record", "Moth No.", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.MalpotReference?.MothNo),
                new("landRecord.paanaNo", "Land Record", "Paana No.", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcel?.MalpotReference?.PaanaNo),

                new("frontage.roads", "Frontage", "Road Frontages", false,
                    (canvasObject, _, _) => FormatFrontages(canvasObject.BaselineParcel)),

                new("contribution.general", "Contribution", "General Contribution", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalGeneralContributionSqm)),
                new("contribution.specific", "Contribution", "Specific Contribution", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalSpecificContributionSqm)),
                new("contribution.total", "Contribution", "Total Contribution", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalContributionSqm)),
                new("contribution.percent", "Contribution", "Contribution Percent", false,
                    (canvasObject, _, _) => FormatPercent(canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalContributionPercent)),
                new("contribution.netReturnable", "Contribution", "Net Returnable Area", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.ParcelContributionSummary?.NetReturnableAreaSqm)),
                new("contribution.replottedAssigned", "Contribution", "Replotted Area Assigned", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.BaselineParcel?.ParcelContributionSummary?.ReplottedAreaAssignedSqm)),

                new("text.count", "Text", "Text Objects", false,
                    (_, _, selectedCount) => selectedCount.ToString("N0")),
                new("text.value", "Text", "Text", false,
                    (canvasObject, _, _) => canvasObject.LabelText),
                new("text.insertionPoint", "Text", "Insertion Point", false,
                    (canvasObject, _, _) => FormatPoint(canvasObject.Shape?.Coordinate)),
                new("text.alignment", "Text", "Alignment", false,
                    (canvasObject, _, _) => ReadMetadataString(canvasObject.GeometryMetadataJson, "TextAlignment")),
                new("text.source", "Text", "Label Source", false,
                    (_, _, _) => "Manual text"),

                new("road.count", "Road", "Road Objects", false,
                    (_, _, selectedCount) => selectedCount.ToString("N0")),
                new("road.name", "Road", "Name", false,
                    (canvasObject, _, _) => canvasObject.Road?.RoadName),
                new("road.code", "Road", "Code", false,
                    (canvasObject, _, _) => canvasObject.Road?.RoadCode),
                new("road.type", "Road", "Type", false,
                    (canvasObject, _, _) => canvasObject.Road?.RoadType),
                new("road.surface", "Road", "Surface", false,
                    (canvasObject, _, _) => canvasObject.Road?.SurfaceType),
                new("road.width", "Road", "Road Width", false,
                    (canvasObject, _, _) => FormatLength(canvasObject.Road?.RoadWidth)),
                new("road.rightOfWay", "Road", "Right of Way", false,
                    (canvasObject, _, _) => FormatLength(canvasObject.Road?.RightOfWayWidth)),
                new("road.description", "Road", "Description", false,
                    (canvasObject, _, _) => canvasObject.Road?.Description),
                new("road.recordId", "Road", "Record ID", false,
                    (canvasObject, _, _) => canvasObject.Road?.Id.ToString() ?? canvasObject.RoadId?.ToString()),

                new("block.count", "Block", "Block Objects", false,
                    (_, _, selectedCount) => selectedCount.ToString("N0")),
                new("block.name", "Block", "Name", false,
                    (canvasObject, _, _) => canvasObject.Block?.BlockName),
                new("block.code", "Block", "Code", false,
                    (canvasObject, _, _) => canvasObject.Block?.BlockCode),
                new("block.depth", "Block", "Depth", false,
                    (canvasObject, _, _) => FormatLength(canvasObject.Block?.BlockDepth)),
                new("block.depthGeometry", "Block", "Block Depth (from Geometry)", false,
                    (canvasObject, _, _) => FormatLength(CanvasGeometryMetricsService.GetBlockDepthFromGeometry(canvasObject))),
                new("block.length", "Block", "Length", false,
                    (canvasObject, _, _) => FormatLength(canvasObject.Block?.BlockLength)),
                new("block.landUse", "Block", "Land Use", false,
                    (canvasObject, _, _) => canvasObject.Block?.BlockLandUse),
                new("block.area", "Block", "Block Area", false,
                    (canvasObject, _, _) => FormatArea(GetBlockDisplayArea(canvasObject))),
                new("block.description", "Block", "Description", false,
                    (canvasObject, _, _) => canvasObject.Block?.Description),
                new("block.recordId", "Block", "Record ID", false,
                    (canvasObject, _, _) => canvasObject.Block?.Id.ToString() ?? canvasObject.BlockId?.ToString()),

                new("replotted.systemNumber", "Replotted Parcel", "System Number", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.SystemGeneratedNumber),
                new("replotted.derivedNumber", "Replotted Parcel", "Derived Number", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.DerivedNumber),
                new("replotted.blockSequence", "Replotted Parcel", "Block Sequence", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.BlockSequenceNumber),
                new("replotted.activeNumberType", "Replotted Parcel", "Active Number Type", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.ActiveNumberType),
                new("replotted.plotType", "Replotted Parcel", "Plot Type", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.PlotType?.TypeName),
                new("replotted.block", "Replotted Parcel", "Block", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.Block?.BlockName),
                new("replotted.plotArea", "Replotted Parcel", "Plot Area", false,
                    (canvasObject, _, _) => FormatArea(canvasObject.ReplottedParcel?.PlotAreaSqm)),
                new("replotted.notes", "Replotted Parcel", "Notes", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.Notes),
                new("replotted.recordId", "Replotted Parcel", "Record ID", false,
                    (canvasObject, _, _) => canvasObject.ReplottedParcel?.Id.ToString() ?? canvasObject.ReplottedParcelId?.ToString()),

                new("source.format", "Source", "Source Format", false,
                    (canvasObject, metadata, _) => FirstNonEmpty(
                        metadata?.SourceFormat,
                        ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceFormat"))),
                new("source.file", "Source", "Source File", false,
                    (canvasObject, metadata, _) => FirstNonEmpty(
                        metadata?.SourceFileName,
                        ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceFileName"))),
                new("source.layer", "Source", "Source Layer", false,
                    (canvasObject, metadata, _) => FirstNonEmpty(
                        metadata?.SourceLayer,
                        ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceLayer"))),
                new("source.matchedText", "Source", "Matched Text", false,
                    (_, metadata, _) => metadata?.MatchedText),
                new("source.handle", "Source", "Source Handle", false,
                    (canvasObject, metadata, _) => FirstNonEmpty(
                        metadata?.SourceHandle,
                        canvasObject.SourceDxfHandle,
                        ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceHandle"))),
                new("source.importedAt", "Source", "Imported At", false,
                    (_, metadata, _) => FormatDate(metadata?.ImportedAt)),

                new("canvas.objectId", "Canvas", "Object ID", false,
                    (canvasObject, _, _) => canvasObject.Id.ToString()),
                new("canvas.baselineParcelId", "Canvas", "Baseline Parcel ID", false,
                    (canvasObject, _, _) => canvasObject.BaselineParcelId?.ToString()),
                new("canvas.visible", "Canvas", "Visible", false,
                    (canvasObject, _, _) => FormatBoolean(canvasObject.IsVisible)),
                new("canvas.locked", "Canvas", "Locked", false,
                    (canvasObject, _, _) => FormatBoolean(canvasObject.IsLocked || canvasObject.CanvasLayer?.IsLocked == true)),
                new("canvas.created", "Canvas", "Created", false,
                    (canvasObject, _, _) => FormatDate(canvasObject.CreatedDate)),
                new("canvas.modified", "Canvas", "Last Modified", false,
                    (canvasObject, _, _) => FormatDate(canvasObject.LastModifiedDate)),

                new("style.borderColor", "Style", "Border Color", false,
                    (canvasObject, _, _) => FirstNonEmpty(canvasObject.BorderColorOverride, canvasObject.CanvasLayer?.BorderColor)),
                new("style.lineWeight", "Style", "Line Weight", false,
                    (canvasObject, _, _) => FormatNumber(canvasObject.LineWeightOverride ?? canvasObject.CanvasLayer?.LineWeight)),
                new("style.lineStyle", "Style", "Line Style", false,
                    (canvasObject, _, _) => FirstNonEmpty(canvasObject.LineStyleOverride, canvasObject.CanvasLayer?.LineStyle)),
                new("style.fillColor", "Style", "Fill Color", false,
                    (canvasObject, _, _) => FirstNonEmpty(canvasObject.FillColorOverride, canvasObject.CanvasLayer?.FillColor)),
                new("style.fillTransparency", "Style", "Fill Transparency", false,
                    (canvasObject, _, _) => (canvasObject.FillTransparencyOverride ?? canvasObject.CanvasLayer?.FillTransparency)?.ToString())
            ];
        }

        private static string GetSharedFieldValue(
            IReadOnlyList<CanvasObject> selectedObjects,
            ParcelPropertyField field,
            int selectedCount)
        {
            var values = selectedObjects
                .Select(canvasObject =>
                {
                    CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
                    return field.GetValue(canvasObject, metadata, selectedCount);
                })
                .Select(value => string.IsNullOrWhiteSpace(value) ? "--" : value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return values.Count == 1 ? values[0] : "*VARIES*";
        }

        private static string? GetAggregatedFieldValue(
            IReadOnlyList<CanvasObject> selectedObjects,
            ParcelPropertyField field,
            int selectedCount)
        {
            if (field.Key == "selection.selectedObjects")
                return selectedCount.ToString("N0");

            if (TryAggregateAreaField(selectedObjects, field.Key, out string? areaValue))
                return areaValue;

            if (TryAggregateLengthField(selectedObjects, field.Key, out string? lengthValue))
                return lengthValue;

            if (TryAggregatePointCountField(selectedObjects, field.Key, out string? pointCountValue))
                return pointCountValue;

            if (TryAggregateBoundsField(selectedObjects, field.Key, out string? boundsValue))
                return boundsValue;

            if (field.Key == "parcel.uniqueCode")
                return FormatSelectedUniqueParcelCodes(selectedObjects);

            IReadOnlyList<string> values = selectedObjects
                .Select(canvasObject =>
                {
                    CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
                    return field.GetValue(canvasObject, metadata, selectedCount);
                })
                .Select(value => string.IsNullOrWhiteSpace(value) ? "--" : value.Trim())
                .ToList();

            List<string> distinctValues = values
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctValues.Count == 0 ||
                distinctValues.All(value => value == "--"))
            {
                return field.IncludeWhenEmpty ? "--" : null;
            }

            return distinctValues.Count == 1 ? distinctValues[0] : VariesPropertyValue;
        }

        private static string FormatSelectedUniqueParcelCodes(IReadOnlyList<CanvasObject> selectedObjects)
        {
            List<string> uniqueCodes = selectedObjects
                .Select(canvasObject =>
                {
                    CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
                    CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);
                    string? code = FirstNonEmpty(
                        canvasObject.BaselineParcel?.FullUniqueParcelCode,
                        assignedMetadata?.FullUniqueParcelCode);
                    if (!string.IsNullOrWhiteSpace(code))
                        return code;

                    string? mapSheetNo = FirstNonEmpty(
                        canvasObject.BaselineParcel?.MapSheetNo,
                        assignedMetadata?.MapSheetNo);
                    string? parcelNo = FirstNonEmpty(
                        canvasObject.BaselineParcel?.ParcelNo,
                        assignedMetadata?.ParcelNo);

                    return string.IsNullOrWhiteSpace(mapSheetNo) || string.IsNullOrWhiteSpace(parcelNo)
                        ? null
                        : $"{mapSheetNo.Trim()}::{parcelNo.Trim()}";
                })
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return uniqueCodes.Count == 0 ? "--" : string.Join(", ", uniqueCodes);
        }

        private static bool TryAggregateAreaField(
            IReadOnlyList<CanvasObject> selectedObjects,
            string fieldKey,
            out string? value)
        {
            Func<CanvasObject, CadastralCanvasMetadata?, double?>? selector = fieldKey switch
            {
                "area.originalRecord" => (canvasObject, metadata) =>
                    canvasObject.BaselineParcel?.OriginalAreaSqm ?? GetAssignedCadastralMetadata(metadata)?.RecordAreaSqm,
                "geometry.area" => (canvasObject, _) =>
                    CanvasGeometryMetricsService.GetArea(canvasObject),
                "area.fieldMeasured" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.FieldMeasuredAreaSqm,
                "area.effective" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.EffectiveAreaSqm,
                "area.imported" => (_, metadata) =>
                    metadata?.CalculatedAreaSqm,
                "contribution.general" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalGeneralContributionSqm,
                "contribution.specific" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalSpecificContributionSqm,
                "contribution.total" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.ParcelContributionSummary?.TotalContributionSqm,
                "contribution.netReturnable" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.ParcelContributionSummary?.NetReturnableAreaSqm,
                "contribution.replottedAssigned" => (canvasObject, _) =>
                    canvasObject.BaselineParcel?.ParcelContributionSummary?.ReplottedAreaAssignedSqm,
                "block.area" => (canvasObject, _) =>
                    GetBlockDisplayArea(canvasObject),
                "replotted.plotArea" => (canvasObject, _) =>
                    canvasObject.ReplottedParcel?.PlotAreaSqm,
                _ => null
            };

            if (selector == null)
            {
                value = null;
                return false;
            }

            double total = 0.0;
            int count = 0;
            foreach (CanvasObject canvasObject in selectedObjects)
            {
                CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
                double? area = selector(canvasObject, metadata);
                if (!area.HasValue)
                    continue;

                total += Math.Abs(area.Value);
                count++;
            }

            value = count == 0
                ? null
                : FormatArea(total);
            return true;
        }

        private static bool TryAggregateLengthField(
            IReadOnlyList<CanvasObject> selectedObjects,
            string fieldKey,
            out string? value)
        {
            if (fieldKey != "geometry.length")
            {
                value = null;
                return false;
            }

            double total = selectedObjects
                .Select(CanvasGeometryMetricsService.GetLength)
                .Where(length => length.HasValue)
                .Sum(length => Math.Abs(length!.Value));

            value = total > 0 ? FormatLength(total) : null;
            return true;
        }

        private static bool TryAggregatePointCountField(
            IReadOnlyList<CanvasObject> selectedObjects,
            string fieldKey,
            out string? value)
        {
            if (fieldKey != "geometry.vertexCount")
            {
                value = null;
                return false;
            }

            int?[] counts = selectedObjects
                .Where(ShouldShowVertexCount)
                .Select(CanvasGeometryMetricsService.GetVertexCount)
                .Where(count => count.HasValue)
                .ToArray();

            int total = counts.Sum(count => count!.Value);
            value = counts.Length > 0 ? total.ToString("N0") : null;
            return true;
        }

        private static bool TryAggregateBoundsField(
            IReadOnlyList<CanvasObject> selectedObjects,
            string fieldKey,
            out string? value)
        {
            if (fieldKey != "geometry.bounds")
            {
                value = null;
                return false;
            }

            NtsEnvelope bounds = new();
            bool hasBounds = false;
            foreach (CanvasObject canvasObject in selectedObjects)
            {
                NtsEnvelope? envelope = canvasObject.Shape?.EnvelopeInternal;
                if (!CanvasGeometryMetricsService.IsUsableEnvelope(envelope))
                    continue;

                bounds.ExpandToInclude(envelope);
                hasBounds = true;
            }

            value = hasBounds ? FormatGeometryBounds(bounds) : null;
            return true;
        }

        private static bool ShouldListAggregate(ParcelPropertyField field)
        {
            return field.Key is
                "parcel.parcelNo" or
                "parcel.mapSheetNo" or
                "parcel.uniqueCode" or
                "owner.primaryOwner" or
                "owner.coOwners" or
                "owner.fatherSpouse" or
                "owner.citizenshipNo" or
                "owner.contact" or
                "owner.email" or
                "tenancy.tenantName" or
                "source.file" or
                "source.layer" or
                "source.matchedText" or
                "source.handle" or
                "canvas.objectId" or
                "canvas.baselineParcelId";
        }

        private static bool ShouldCountAggregate(ParcelPropertyField field)
        {
            return field.Key is
                "selection.objectType" or
                "selection.layer" or
                "selection.assignment" or
                "parcel.ownershipType" or
                "parcel.landUse" or
                "tenancy.hasTenant" or
                "area.effectiveMode" or
                "location.province" or
                "location.district" or
                "location.municipality" or
                "location.ward" or
                "source.format" or
                "canvas.visible" or
                "canvas.locked";
        }

        private static string JoinDistinctValues(IEnumerable<string?> values)
        {
            List<string> distinct = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return distinct.Count == 0 ? "--" : string.Join(", ", distinct);
        }

        private static string FormatValueCounts(IReadOnlyList<string> values)
        {
            return string.Join(", ", values
                .GroupBy(value => string.IsNullOrWhiteSpace(value) ? "--" : value.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => $"{group.Key}: {group.Count():N0}"));
        }

        private static string GetAssignmentValue(CanvasObject canvasObject, CadastralCanvasMetadata? metadata)
        {
            if (!string.IsNullOrWhiteSpace(metadata?.AssignmentStatus))
                return metadata.AssignmentStatus;

            return canvasObject.BaselineParcelId.HasValue ||
                   canvasObject.BaselineParcel != null ||
                   canvasObject.RoadId.HasValue ||
                   canvasObject.Road != null ||
                   canvasObject.BlockId.HasValue ||
                   canvasObject.Block != null ||
                   canvasObject.ReplottedParcelId.HasValue ||
                   canvasObject.ReplottedParcel != null
                ? "Assigned"
                : "Unassigned";
        }

        private void ShowNoParcelSelection()
        {
            ShowPropertyGridMessage("Select an object on the map to view its properties.");
        }

        private void ShowLargeSelectionPropertyMessage(int selectedCount)
        {
            ClearCurrentPropertyGridSelection();
            ShowPropertyGridMessage(
                $"{selectedCount:N0} objects selected. Property details are disabled for selections over {MaxPropertySelectionDetails:N0} objects.");
        }

        private void ShowPropertyGridMessage(string message)
        {
            if (dgvParcelObjProperty.Columns.Count == 0)
            {
                ConfigureLayerPropertiesPanel();
            }

            SetPropertiesPanelTitle();
            dgvParcelObjProperty.SuspendLayout();
            try
            {
                dgvParcelObjProperty.Rows.Clear();
                int index = dgvParcelObjProperty.Rows.Add(string.Empty, message);
                DataGridViewRow row = dgvParcelObjProperty.Rows[index];
                row.DefaultCellStyle.ForeColor = Color.FromArgb(85, 96, 110);
                row.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
                dgvParcelObjProperty.ClearSelection();
            }
            finally
            {
                dgvParcelObjProperty.ResumeLayout();
            }
        }

        private void AddPropertyCategory(string categoryName)
        {
            int index = dgvParcelObjProperty.Rows.Add(categoryName, string.Empty);
            DataGridViewRow row = dgvParcelObjProperty.Rows[index];
            row.ReadOnly = true;
            row.Height = 28;
            row.DefaultCellStyle.BackColor = Color.FromArgb(242, 242, 242);
            row.DefaultCellStyle.ForeColor = Color.Black;
            row.DefaultCellStyle.Font = grpParcelObjProp.Font;
            row.Cells[1].Style.BackColor = row.DefaultCellStyle.BackColor;
            row.Cells[1].Style.SelectionBackColor = row.DefaultCellStyle.BackColor;
            row.Cells[0].Style.SelectionBackColor = row.DefaultCellStyle.BackColor;
            row.Cells[0].Style.SelectionForeColor = row.DefaultCellStyle.ForeColor;
        }

        private bool IsPropertyRowEditable(ObjectPropertyRow row)
        {
            CanvasObject? canvasObject = GetCurrentSinglePropertyGridObject();
            if (canvasObject == null)
                return false;

            if (string.Equals(row.Key, "text.value", StringComparison.OrdinalIgnoreCase))
                return IsTextObject(canvasObject);

            return IsEditableBlockPropertyKey(row.Key) &&
                   IsBlockObject(canvasObject) &&
                   (canvasObject.Block != null || canvasObject.BlockId.HasValue);
        }

        private void AddPropertyRow(
            string key,
            string field,
            string? value,
            bool includeWhenEmpty = false,
            bool editable = false)
        {
            string displayValue = string.IsNullOrWhiteSpace(value) ? "--" : value.Trim();
            if (!includeWhenEmpty && displayValue == "--")
                return;

            int index = dgvParcelObjProperty.Rows.Add(field, displayValue);
            DataGridViewRow row = dgvParcelObjProperty.Rows[index];
            row.Tag = key;
            row.ReadOnly = false;
            row.Cells[0].ReadOnly = true;
            row.Cells[1].ReadOnly = !editable || _isApplicationEditLocked;
            row.DefaultCellStyle.BackColor = Color.White;
            if (editable && !_isApplicationEditLocked)
            {
                row.Cells[1].Style.BackColor = Color.FromArgb(255, 252, 232);
            }
        }

        private static bool IsOriginalParcelObject(CanvasObject canvasObject)
        {
            if (canvasObject.BaselineParcel != null || canvasObject.BaselineParcelId.HasValue)
                return true;

            if (canvasObject.CanvasLayer != null &&
                CanvasLayerTreeService.IsDrawingMarkupLayer(canvasObject.CanvasLayer))
            {
                return false;
            }

            return IsCadastralMetadataAssigned(ReadCadastralMetadata(canvasObject.GeometryMetadataJson));
        }

        private static bool IsTextObject(CanvasObject canvasObject)
        {
            return string.Equals(canvasObject.ObjectType, "Text", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(canvasObject.CanvasLayer?.LayerType, "Annotation", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsExternalReferenceObject(CanvasObject canvasObject)
        {
            return canvasObject.CanvasLayer != null &&
                   CanvasLayerTreeService.IsExternalImportedLayer(canvasObject.CanvasLayer);
        }

        private static bool IsBlockObject(CanvasObject canvasObject)
        {
            return canvasObject.Block != null ||
                   canvasObject.BlockId.HasValue ||
                   (canvasObject.CanvasLayer != null &&
                    CanvasLayerTreeService.IsBlockLayoutLayer(canvasObject.CanvasLayer));
        }

        private static bool ShouldShowGeometrySection(CanvasObject canvasObject)
        {
            return !IsTextObject(canvasObject) &&
                   canvasObject.Shape != null &&
                   !canvasObject.Shape.IsEmpty;
        }

        private static bool ShouldShowArea(CanvasObject canvasObject)
        {
            string objectType = canvasObject.ObjectType ?? string.Empty;
            string? geometryType = canvasObject.Shape?.GeometryType;
            return IsOriginalParcelObject(canvasObject) ||
                   canvasObject.ReplottedParcel != null ||
                   IsBlockObject(canvasObject) ||
                   objectType.Equals("Polygon", StringComparison.OrdinalIgnoreCase) ||
                   objectType.Equals("Circle", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(geometryType, "Polygon", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(geometryType, "MultiPolygon", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ShouldShowVertexCount(CanvasObject canvasObject)
        {
            CanvasLayer? layer = canvasObject.CanvasLayer;
            string? metadataShapeType = ReadMetadataString(canvasObject.GeometryMetadataJson, "ShapeType");
            if (layer == null ||
                !CanvasLayerTreeService.IsDrawingMarkupLayer(layer) ||
                !ShouldShowGeometrySection(canvasObject) ||
                IsCircleVertexGeometry(canvasObject.ObjectType) ||
                IsCircleVertexGeometry(layer.LayerType) ||
                IsCircleVertexGeometry(metadataShapeType))
            {
                return false;
            }

            return IsDrawingLineworkVertexGeometry(canvasObject.ObjectType) ||
                   IsDrawingLineworkVertexGeometry(layer.LayerType) ||
                   IsDrawingLineworkVertexGeometry(metadataShapeType) ||
                   IsDrawingLineworkVertexGeometry(canvasObject.Shape?.GeometryType);
        }

        private static bool IsCircleVertexGeometry(string? value)
        {
            return value != null &&
                   value.Equals("Circle", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDrawingLineworkVertexGeometry(string? value)
        {
            return value != null &&
                   (value.Equals("Polygon", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("MultiPolygon", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Polyline", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Line", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("LineString", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("MultiLineString", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Arc", StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsPointGeometry(CanvasObject canvasObject)
        {
            return string.Equals(canvasObject.ObjectType, "Point", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(canvasObject.Shape?.GeometryType, "Point", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDisplayObjectType(CanvasObject canvasObject)
        {
            if (IsTextObject(canvasObject))
                return "Text";

            if (canvasObject.ReplottedParcel != null || canvasObject.ReplottedParcelId.HasValue)
                return "Replotted Parcel";

            if (canvasObject.Road != null || canvasObject.RoadId.HasValue)
                return "Road";

            if (IsBlockObject(canvasObject))
                return "Block";

            if (IsOriginalParcelObject(canvasObject))
                return "Original Parcel";

            if (IsExternalReferenceObject(canvasObject))
                return $"External {FirstNonEmpty(canvasObject.ObjectType, canvasObject.Shape?.GeometryType, "Object")}";

            return FirstNonEmpty(canvasObject.ObjectType, canvasObject.Shape?.GeometryType, "Object") ?? "Object";
        }

        private static string? GetDataLinkDisplay(CanvasObject canvasObject, CadastralCanvasMetadata? metadata)
        {
            if (canvasObject.ReplottedParcel != null || canvasObject.ReplottedParcelId.HasValue)
                return $"Replotted parcel: {canvasObject.ReplottedParcel?.Id.ToString() ?? canvasObject.ReplottedParcelId?.ToString()}";

            if (canvasObject.Road != null || canvasObject.RoadId.HasValue)
                return $"Road: {FirstNonEmpty(canvasObject.Road?.RoadName, canvasObject.RoadId?.ToString())}";

            if (IsBlockObject(canvasObject))
                return $"Block: {FirstNonEmpty(canvasObject.Block?.BlockName, canvasObject.BlockId?.ToString(), canvasObject.ObjectDescription, canvasObject.CanvasLayer?.Name)}";

            if (IsOriginalParcelObject(canvasObject))
            {
                CadastralCanvasMetadata? assignedMetadata = GetAssignedCadastralMetadata(metadata);
                string? code = FirstNonEmpty(
                    canvasObject.BaselineParcel?.FullUniqueParcelCode,
                    assignedMetadata?.FullUniqueParcelCode);
                return string.IsNullOrWhiteSpace(code) ? "Original parcel" : $"Original parcel: {code}";
            }

            return null;
        }

        private static bool HasSourceProperties(CanvasObject canvasObject)
        {
            if (!string.IsNullOrWhiteSpace(canvasObject.SourceDxfHandle))
                return true;

            CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
            return (metadata != null &&
                    (!string.IsNullOrWhiteSpace(metadata.SourceFormat) ||
                     !string.IsNullOrWhiteSpace(metadata.SourceFileName) ||
                     !string.IsNullOrWhiteSpace(metadata.SourceLayer) ||
                     !string.IsNullOrWhiteSpace(metadata.MatchedText) ||
                     !string.IsNullOrWhiteSpace(metadata.SourceHandle))) ||
                   !string.IsNullOrWhiteSpace(ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceFormat")) ||
                   !string.IsNullOrWhiteSpace(ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceFileName")) ||
                   !string.IsNullOrWhiteSpace(ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceLayer")) ||
                   !string.IsNullOrWhiteSpace(ReadMetadataString(canvasObject.GeometryMetadataJson, "SourceHandle"));
        }

        private static string? FormatObjectTypeCounts(IReadOnlyList<CanvasObject> canvasObjects)
        {
            return string.Join(", ", canvasObjects
                .GroupBy(GetDisplayObjectType, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Select(group => $"{group.Key}: {group.Count():N0}"));
        }

        private static string? FormatTotalLength(IReadOnlyList<CanvasObject> canvasObjects)
        {
            double total = canvasObjects
                .Select(CanvasGeometryMetricsService.GetLength)
                .Where(value => value.HasValue)
                .Sum(value => Math.Abs(value!.Value));

            return total > 0 ? FormatLength(total) : null;
        }

        private static string? FormatTotalArea(IReadOnlyList<CanvasObject> canvasObjects)
        {
            double total = canvasObjects
                .Where(ShouldShowArea)
                .Select(CanvasGeometryMetricsService.GetArea)
                .Where(value => value.HasValue)
                .Sum(value => Math.Abs(value!.Value));

            return total > 0 ? FormatArea(total) : null;
        }

        private static string? FormatTotalVertexCount(IReadOnlyList<CanvasObject> canvasObjects)
        {
            int total = canvasObjects
                .Where(ShouldShowVertexCount)
                .Select(CanvasGeometryMetricsService.GetVertexCount)
                .Where(value => value.HasValue)
                .Sum(value => value!.Value);

            return total > 0 ? total.ToString("N0") : null;
        }

        private static string? FormatCombinedBounds(IReadOnlyList<CanvasObject> canvasObjects)
        {
            NtsEnvelope bounds = new();
            bool hasBounds = false;
            foreach (CanvasObject canvasObject in canvasObjects)
            {
                NtsEnvelope? envelope = canvasObject.Shape?.EnvelopeInternal;
                if (!CanvasGeometryMetricsService.IsUsableEnvelope(envelope))
                    continue;

                bounds.ExpandToInclude(envelope);
                hasBounds = true;
            }

            return hasBounds ? FormatGeometryBounds(bounds) : null;
        }

        private static string? FormatPoint(NetTopologySuite.Geometries.Coordinate? coordinate)
        {
            if (coordinate == null ||
                !double.IsFinite(coordinate.X) ||
                !double.IsFinite(coordinate.Y))
            {
                return null;
            }

            return $"X {coordinate.X:N3}, Y {coordinate.Y:N3}";
        }

        private static string? ReadMetadataString(string? json, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                using JsonDocument document = JsonDocument.Parse(json);
                return TryGetJsonPropertyIgnoreCase(document.RootElement, propertyName, out JsonElement element)
                    ? element.GetString()
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static string? GetOwnerDisplayName(CanvasObject canvasObject)
        {
            CadastralCanvasMetadata? metadata = ReadCadastralMetadata(canvasObject.GeometryMetadataJson);
            return FirstNonEmpty(
                canvasObject.BaselineParcel?.LandOwner?.FullName,
                GetAssignedCadastralMetadata(metadata)?.OwnerName);
        }

        private static string TrimForDisplay(string value, int maxLength)
        {
            string trimmed = value.Trim().ReplaceLineEndings(" ");
            return trimmed.Length <= maxLength
                ? trimmed
                : $"{trimmed[..Math.Max(0, maxLength - 3)]}...";
        }

        private static CadastralCanvasMetadata? ReadCadastralMetadata(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                CadastralCanvasMetadata? metadata =
                    JsonSerializer.Deserialize<CadastralCanvasMetadata>(json, CadastralMetadataJsonOptions);
                return string.Equals(metadata?.Kind, CadastralCanvasMetadata.MetadataKind, StringComparison.OrdinalIgnoreCase)
                    ? metadata
                    : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsCadastralMetadataAssigned(CadastralCanvasMetadata? metadata)
        {
            if (metadata == null)
                return false;

            if (string.Equals(metadata.AssignmentStatus, "Unassigned", StringComparison.OrdinalIgnoreCase))
                return false;

            return metadata.BaselineParcelId.HasValue ||
                   !string.IsNullOrWhiteSpace(metadata.FullUniqueParcelCode) ||
                   string.Equals(metadata.AssignmentStatus, "AutoAssigned", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(metadata.AssignmentStatus, "ManualAssigned", StringComparison.OrdinalIgnoreCase);
        }

        private static CadastralCanvasMetadata? GetAssignedCadastralMetadata(CadastralCanvasMetadata? metadata)
        {
            return IsCadastralMetadataAssigned(metadata) ? metadata : null;
        }

        private static string? FormatCoOwners(Core.Entities.LandData.BaselineParcel? parcel)
        {
            if (parcel?.CoOwners == null || parcel.CoOwners.Count == 0)
                return null;

            return string.Join("; ", parcel.CoOwners
                .OrderBy(coOwner => coOwner.LandOwner.FullName)
                .Select(coOwner =>
                {
                    string ownerName = string.IsNullOrWhiteSpace(coOwner.LandOwner.FullName)
                        ? $"Owner #{coOwner.LandOwnerId}"
                        : coOwner.LandOwner.FullName.Trim();
                    return coOwner.OwnershipSharePercent.HasValue
                        ? $"{ownerName} ({coOwner.OwnershipSharePercent.Value:0.##}%)"
                        : ownerName;
                }));
        }

        private static string? FormatFrontages(Core.Entities.LandData.BaselineParcel? parcel)
        {
            if (parcel?.ParcelFrontages == null || parcel.ParcelFrontages.Count == 0)
                return null;

            return string.Join("; ", parcel.ParcelFrontages
                .OrderBy(frontage => frontage.FacingDirection)
                .Select(frontage =>
                {
                    string roadName = string.IsNullOrWhiteSpace(frontage.Road?.RoadName)
                        ? "Road"
                        : frontage.Road.RoadName.Trim();
                    string length = frontage.FrontageLength.HasValue
                        ? $"{frontage.FrontageLength.Value:0.##} m"
                        : "length not set";
                    return $"{roadName} ({frontage.FacingDirection}, {length})";
                }));
        }

        private static string? FormatGeometryBounds(NtsEnvelope? envelope)
        {
            if (!CanvasGeometryMetricsService.IsUsableEnvelope(envelope))
                return null;

            double width = envelope!.Width;
            double height = envelope.Height;
            return $"W {width:N2} m, H {height:N2} m";
        }

        private static string? FormatArea(double? areaSqm)
        {
            if (!areaSqm.HasValue)
                return null;

            double sqm = areaSqm.Value;
            string traditionalUnit = GetTraditionalAreaUnit();
            var (sqmPrec, tradPrec) = GetAreaPrecisionSettingsStatic();
            string traditionalArea = string.Equals(traditionalUnit, "BKD", StringComparison.OrdinalIgnoreCase)
                ? AreaConverterService.SqmToBKDString(sqm, tradPrec)
                : AreaConverterService.SqmToRAPDString(sqm, tradPrec);

            return $"{sqm.ToString($"F{sqmPrec}")} sq.m ({traditionalUnit}: {traditionalArea})";
        }

        private static (int SqmPrecision, int TraditionalPrecision) GetAreaPrecisionSettingsStatic()
        {
            try
            {
                if (!AppServices.HasContext)
                    return (3, 2);

                var s = AppServices.Context.Session.GetDbContext()
                    .ProjectSettings
                    .AsNoTracking()
                    .Select(ps => new { ps.AreaSqmDecimalPlaces, ps.TraditionalAreaLowestUnitDecimalPlaces })
                    .FirstOrDefault();
                return s == null ? (3, 2) : (s.AreaSqmDecimalPlaces, s.TraditionalAreaLowestUnitDecimalPlaces);
            }
            catch
            {
                return (3, 2);
            }
        }

        private static string? FormatLength(double? lengthMeters)
        {
            if (!lengthMeters.HasValue)
                return null;

            return $"{lengthMeters.Value:N2} m";
        }

        private static string GetTraditionalAreaUnit()
        {
            try
            {
                if (!AppServices.HasContext)
                    return "RAPD";

                return AppServices.Context.Session.GetDbContext()
                    .ProjectSettings
                    .Select(settings => settings.TraditionalAreaUnit)
                    .FirstOrDefault() is string unit &&
                       string.Equals(unit, "BKD", StringComparison.OrdinalIgnoreCase)
                    ? "BKD"
                    : "RAPD";
            }
            catch
            {
                return "RAPD";
            }
        }

        private static string? FormatPercent(double? value)
        {
            return value.HasValue ? $"{value.Value:0.##}%" : null;
        }

        private static string? FormatNumber(double? value)
        {
            return value.HasValue ? value.Value.ToString("N2") : null;
        }

        private static string? FormatDate(DateTime? value)
        {
            if (!value.HasValue || value.Value == default)
                return null;

            return value.Value.ToString("yyyy-MM-dd HH:mm");
        }

        private static string FormatBoolean(bool value)
        {
            return value ? "Yes" : "No";
        }

        private static string? FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
        }

        private sealed class SelectedPropertyObjectComboItem
        {
            public SelectedPropertyObjectComboItem(string text, Guid? canvasObjectId)
            {
                Text = text;
                CanvasObjectId = canvasObjectId;
            }

            public string Text { get; }
            public Guid? CanvasObjectId { get; }
            public bool IsAll => !CanvasObjectId.HasValue;

            public override string ToString()
            {
                return Text;
            }
        }

        private sealed record ObjectPropertySection(string Title, IReadOnlyList<ObjectPropertyRow> Rows);

        private sealed record ObjectPropertyRow(string Key, string Label, string? Value, bool IncludeWhenEmpty);

        private sealed class ParcelPropertyField
        {
            public ParcelPropertyField(
                string key,
                string category,
                string label,
                bool includeWhenEmpty,
                Func<CanvasObject, CadastralCanvasMetadata?, int, string?> valueFactory)
            {
                Key = key;
                Category = category;
                Label = label;
                IncludeWhenEmpty = includeWhenEmpty;
                _valueFactory = valueFactory;
            }

            private readonly Func<CanvasObject, CadastralCanvasMetadata?, int, string?> _valueFactory;

            public string Key { get; }
            public string Category { get; }
            public string Label { get; }
            public bool IncludeWhenEmpty { get; }

            public string? GetValue(CanvasObject canvasObject, CadastralCanvasMetadata? metadata, int selectedCount)
            {
                return _valueFactory(canvasObject, metadata, selectedCount);
            }
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
            if (!EnsureApplicationUnlockedForEditing("importing raster layers"))
                return;

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

                await ApplyCurrentCanvasThemeToLayerColorsAsync();
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
            if (!EnsureApplicationUnlockedForEditing("importing XYZ tile layers"))
                return;

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

                await ApplyCurrentCanvasThemeToLayerColorsAsync();
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
            _workspaceCanvas.ApplicationEditLocked = _isApplicationEditLocked;
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
                InitializeProjectWorkspace(showWorkspace: false);
                mapCanvasControlMain.SuppressCanvasContentUntilReady();
                await _rasterImportFileManagementService
                    .RepairRasterLayerReferencesAsync(context);

                using (mapCanvasControlMain.DeferCacheRefreshes())
                {
                    await ApplySettingsAsync(showRefreshProgress: false);
                    await RestoreCanvasViewportStateAsync();
                    SetOperationProgress(75, "Loading map layers");
                    await RefreshLayerTreeAsync(refreshVectorFeatures: false);
                }

                ShowProjectWorkspace();
                await RefreshInitialProjectCanvasAsync();
                SchedulePostOpenRasterCleanup(context);
                HideOperationProgress();
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

        private void SchedulePostOpenRasterCleanup(ProjectContext context)
        {
            if (IsDisposed || Disposing || !IsHandleCreated)
                return;

            BeginInvoke((System.Windows.Forms.MethodInvoker)(async () =>
            {
                if (IsDisposed ||
                    Disposing ||
                    !AppServices.HasContext ||
                    !ReferenceEquals(AppServices.Context, context))
                {
                    return;
                }

                try
                {
                    await _rasterImportFileManagementService
                        .CleanupUnreferencedProjectRastersAsync(context);
                }
                catch (Exception ex)
                {
                    LogProjectError(
                        $"Deferred raster cleanup failed. Path={context.ProjectFilePath}",
                        ex);
                }
            }));
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
                AppServices.Context.ProjectFilePath,
                readOnlyMode: _isApplicationEditLocked);
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
                SetSelectSplitButtonActive(false);
                mnuDrawLine.Checked = false;
                mnuDrawPolyline.Checked = false;
                mnuDrawPolygon.Checked = false;
                mnuDrawRectangle.Checked = false;
                mnuDrawCircle.Checked = false;
                mnuDrawArc.Checked = false;
                mnuDrawText.Checked = false;
            }
            mapCanvasControlMain.SetPanToolActive(mnuPan.Checked);
        }

        /// <summary>
        /// Drops the persistent Pan / Zoom Window navigation modes back to the
        /// Select tool when Escape is pressed. (Zoom In/Out/Extents are momentary
        /// and already revert to Select on their own.) Returns true when a mode
        /// was active and handled, so the key is consumed.
        /// </summary>
        private bool TryDeactivateCanvasNavigationMode()
        {
            if (!mnuPan.Checked && !mapCanvasControlMain.IsZoomWindowActive)
            {
                return false;
            }

            mnuPan.Checked = false;
            ActivateCanvasTool(MapCanvasTool.Select);
            mapCanvasControlMain.SetPanToolActive(false);
            SetCanvasCommandStatus("Select: Pointer/Window");
            return true;
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
            ActivateCurrentSelectionToolbarMethod();
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

        private void mapCaptureScreenshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mapCanvasControlMain == null || mapCanvasControlMain.IsDisposed)
            {
                MessageBox.Show(
                    "The map canvas is not available.",
                    "Capture Map Screenshot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            using SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = "png",
                FileName = $"RePlot_Map_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                Filter = "PNG image (*.png)|*.png|JPEG image (*.jpg)|*.jpg|Bitmap image (*.bmp)|*.bmp",
                FilterIndex = 1,
                InitialDirectory = AppServices.HasContext
                    ? AppServices.Context.ProjectFolderPath
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                OverwritePrompt = true,
                RestoreDirectory = true,
                Title = "Capture Map Screenshot"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                using Bitmap screenshot = mapCanvasControlMain.CaptureCurrentImage(MapScreenshotExportScale);
                SaveMapScreenshot(screenshot, dialog.FileName);
                SetCanvasCommandStatus(
                    $"Map screenshot saved: {Path.GetFileName(dialog.FileName)} ({screenshot.Width} x {screenshot.Height})");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to capture the map screenshot:\n{ex.Message}",
                    "Capture Map Screenshot",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void SaveMapScreenshot(Bitmap screenshot, string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (extension is ".jpg" or ".jpeg")
            {
                SaveJpegMapScreenshot(screenshot, filePath, MapScreenshotJpegQuality);
                return;
            }

            if (extension == ".bmp")
            {
                screenshot.Save(filePath, ImageFormat.Bmp);
                return;
            }

            screenshot.Save(filePath, ImageFormat.Png);
        }

        private static void SaveJpegMapScreenshot(Bitmap screenshot, string filePath, long quality)
        {
            ImageCodecInfo? jpegCodec = ImageCodecInfo
                .GetImageEncoders()
                .FirstOrDefault(codec => string.Equals(
                    codec.MimeType,
                    "image/jpeg",
                    StringComparison.OrdinalIgnoreCase));

            if (jpegCodec == null)
            {
                screenshot.Save(filePath, ImageFormat.Jpeg);
                return;
            }

            using EncoderParameters parameters = new(1);
            parameters.Param[0] = new EncoderParameter(
                System.Drawing.Imaging.Encoder.Quality,
                quality);

            screenshot.Save(filePath, jpegCodec, parameters);
        }

        private void mnuCanvasDebugOverlay_Click(object sender, EventArgs e)
        {
            mapCanvasControlMain.ShowDebugOverlay = mnuCanvasDebugOverlay.Checked;
            SetCanvasCommandStatus(mnuCanvasDebugOverlay.Checked
                ? "Canvas debug overlay enabled"
                : "Canvas debug overlay disabled");
        }

        private void mnuCanvasPerformanceOverlay_Click(object sender, EventArgs e)
        {
            if (!mnuCanvasPerformanceOverlay.Checked)
            {
                _canvasPerformanceOverlay?.Close();
                SetCanvasCommandStatus("Canvas performance overlay closed");
                return;
            }

            if (_canvasPerformanceOverlay == null || _canvasPerformanceOverlay.IsDisposed)
            {
                _canvasPerformanceOverlay = new frmCanvasPerformanceOverlay(mapCanvasControlMain);
                _canvasPerformanceOverlay.FormClosed += (_, _) =>
                {
                    _canvasPerformanceOverlay = null;
                    mnuCanvasPerformanceOverlay.Checked = false;
                };
            }

            PositionCanvasPerformanceOverlay();
            _canvasPerformanceOverlay.Show(this);
            _canvasPerformanceOverlay.RefreshPerformanceText();
            _canvasPerformanceOverlay.BringToFront();
            SetCanvasCommandStatus("Canvas performance overlay opened");
        }

        private void PositionCanvasPerformanceOverlay()
        {
            if (_canvasPerformanceOverlay == null || _canvasPerformanceOverlay.IsDisposed)
            {
                return;
            }

            Point canvasPoint = mapCanvasControlMain.PointToScreen(new Point(12, 12));
            Rectangle screenBounds = Screen.FromControl(mapCanvasControlMain).WorkingArea;
            int left = Math.Min(
                Math.Max(canvasPoint.X, screenBounds.Left),
                screenBounds.Right - _canvasPerformanceOverlay.Width);
            int top = Math.Min(
                Math.Max(canvasPoint.Y, screenBounds.Top),
                screenBounds.Bottom - _canvasPerformanceOverlay.Height);
            _canvasPerformanceOverlay.Location = new Point(left, top);
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
            if (keyData == (Keys.Control | Keys.Z))
            {
                if (_isApplicationEditLocked)
                {
                    EnsureApplicationUnlockedForEditing("using undo");
                    return true;
                }

                _ = UndoCanvasCommandAsync();
                return true;
            }

            if (keyData == (Keys.Control | Keys.Y))
            {
                if (_isApplicationEditLocked)
                {
                    EnsureApplicationUnlockedForEditing("using redo");
                    return true;
                }

                _ = RedoCanvasCommandAsync();
                return true;
            }

            if (keyData == (Keys.Control | Keys.C))
            {
                return (mapCanvasControlMain.ContainsFocus &&
                        mapCanvasControlMain.CopySelectedObjectsToBuffer()) ||
                       base.ProcessCmdKey(ref msg, keyData);
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                return (mapCanvasControlMain.ContainsFocus &&
                        mapCanvasControlMain.BeginPasteCopiedObjects()) ||
                       base.ProcessCmdKey(ref msg, keyData);
            }

            Keys keyCode = keyData & Keys.KeyCode;
            if (keyCode == Keys.Escape && TryDeactivateCanvasNavigationMode())
            {
                return true;
            }

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

            if (TryHandleCanvasToolShortcut(keyData))
            {
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool TryHandleCanvasToolShortcut(Keys keyData)
        {
            if (!IsCanvasToolShortcutContext(keyData))
            {
                return false;
            }

            Keys keyCode = keyData & Keys.KeyCode;
            switch (keyCode)
            {
                case Keys.S:
                    ActivateSelectionToolbarMethod(SelectionToolbarMethod.PointerWindow);
                    return true;

                case Keys.F13:
                    mnuPan.Checked = !mnuPan.Checked;
                    mnuPan_Click(this, EventArgs.Empty);
                    return true;

                case Keys.D:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Point);
                    return true;

                case Keys.L:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Line);
                    return true;

                case Keys.P:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Polyline);
                    return true;

                case Keys.O:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Polygon);
                    return true;

                case Keys.A:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Arc);
                    return true;

                case Keys.C:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Circle);
                    return true;

                case Keys.R:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Rectangle);
                    return true;

                case Keys.T:
                    _ = ActivateCanvasDrawingToolAsync(MapCanvasTool.Text);
                    return true;

                case Keys.Z:
                    mnuZoomWindow_Click(this, EventArgs.Empty);
                    return true;

                case Keys.E:
                    mnuZoomExtent_Click(this, EventArgs.Empty);
                    return true;

                default:
                    return false;
            }
        }

        private bool IsCanvasToolShortcutContext(Keys keyData)
        {
            Keys modifiers = keyData & Keys.Modifiers;
            if (modifiers != Keys.None && modifiers != Keys.Shift)
            {
                return false;
            }

            if (!AppServices.HasContext ||
                mapCanvasControlMain == null ||
                mapCanvasControlMain.IsTextInputActive ||
                IsTextEntryControlFocused())
            {
                return false;
            }

            return true;
        }

        private bool IsTextEntryControlFocused()
        {
            Control? focusedControl = GetFocusedControl(this);
            if (focusedControl == null)
                return false;

            if (focusedControl is TextBoxBase ||
                focusedControl is ComboBox ||
                focusedControl is NumericUpDown ||
                focusedControl is DomainUpDown ||
                focusedControl is DataGridView ||
                focusedControl is DateTimePicker ||
                focusedControl is MaskedTextBox)
            {
                return true;
            }

            return focusedControl is ToolStrip toolStrip &&
                   toolStrip.Focused &&
                   toolStrip != tsCanvasTools &&
                   toolStrip != tsProjectMenu;
        }

        private void ActivateCurrentSelectionToolbarMethod()
        {
            ActivateSelectionToolbarMethod(_activeSelectionToolbarMethod);
        }

        private void ActivateSelectionToolbarMethod(SelectionToolbarMethod method)
        {
            SetActiveSelectionToolbarMethod(method);
            mapCanvasControlMain.SetPanToolActive(false);

            switch (method)
            {
                case SelectionToolbarMethod.PointerWindow:
                    ActivateCanvasTool(MapCanvasTool.Select);
                    SetCanvasCommandStatus("Select: Pointer/Window");
                    break;

                case SelectionToolbarMethod.Polygon:
                    ActivateSelectionSketchTool(
                        MapCanvasTool.SelectionPolygon,
                        "Select Polygon",
                        "Select Polygon: click points, then press Enter or right-click Finish.");
                    break;

                case SelectionToolbarMethod.IntersectingPoly:
                    ActivateSelectionSketchTool(
                        MapCanvasTool.SelectionIntersectingPolygon,
                        "Select Intersecting Poly",
                        "Intersecting Poly: click points, then press Enter or right-click Finish.");
                    break;

                case SelectionToolbarMethod.IntersectingLine:
                    ActivateSelectionSketchTool(
                        MapCanvasTool.SelectionIntersectingLine,
                        "Select Intersecting Line",
                        "Intersecting Line: click points, then press Enter or right-click Finish.");
                    break;
            }
        }

        private void ActivateSelectionSketchTool(
            MapCanvasTool tool,
            string activeToolText,
            string commandStatus)
        {
            ApplyCanvasToolSelection(tool);
            _currentCanvasTool = tool;

            switch (tool)
            {
                case MapCanvasTool.SelectionPolygon:
                    mapCanvasControlMain.BeginContainedPolygonSelection();
                    break;
                case MapCanvasTool.SelectionIntersectingPolygon:
                    mapCanvasControlMain.BeginIntersectingPolygonSelection();
                    break;
                case MapCanvasTool.SelectionIntersectingLine:
                    mapCanvasControlMain.BeginIntersectingLineSelection();
                    break;
            }

            UpdateActiveTool(activeToolText);
            SetCanvasCommandStatus(commandStatus);
        }

        private void ResetSelectionToolbarMethodToPointerWindow()
        {
            ActivateSelectionToolbarMethod(SelectionToolbarMethod.PointerWindow);
        }

        private void MapCanvasControlMain_SelectionMethodRequested(MapCanvasTool tool)
        {
            switch (tool)
            {
                case MapCanvasTool.Select:
                    ActivateSelectionToolbarMethod(SelectionToolbarMethod.PointerWindow);
                    break;

                case MapCanvasTool.SelectionPolygon:
                    ActivateSelectionToolbarMethod(SelectionToolbarMethod.Polygon);
                    break;

                case MapCanvasTool.SelectionIntersectingPolygon:
                    ActivateSelectionToolbarMethod(SelectionToolbarMethod.IntersectingPoly);
                    break;

                case MapCanvasTool.SelectionIntersectingLine:
                    ActivateSelectionToolbarMethod(SelectionToolbarMethod.IntersectingLine);
                    break;
            }
        }

        private void SetActiveSelectionToolbarMethod(SelectionToolbarMethod method)
        {
            _activeSelectionToolbarMethod = method;

            mnuSelectPointerWindow.Checked = method == SelectionToolbarMethod.PointerWindow;
            mnuSelectPolygon.Checked = method == SelectionToolbarMethod.Polygon;
            mnuSelectIntersectingPoly.Checked = method == SelectionToolbarMethod.IntersectingPoly;
            mnuSelectIntersectingLine.Checked = method == SelectionToolbarMethod.IntersectingLine;

            ToolStripMenuItem selectedItem = method switch
            {
                SelectionToolbarMethod.Polygon => mnuSelectPolygon,
                SelectionToolbarMethod.IntersectingPoly => mnuSelectIntersectingPoly,
                SelectionToolbarMethod.IntersectingLine => mnuSelectIntersectingLine,
                _ => mnuSelectPointerWindow
            };

            mnuSelectTool.Image = selectedItem.Image ?? Properties.Resources.selection_Tool;
            mnuSelectTool.Text = selectedItem.Text;
            mnuSelectTool.ToolTipText = selectedItem.ToolTipText;
        }

        private void ConfigureSelectionToolImages()
        {
            ApplySelectionToolImage(
                PointerWindowSelectionIconFileName,
                mnuSelectPointerWindow,
                mapSelectPointerWindowToolStripMenuItem);
            ApplySelectionToolImage(
                PolygonSelectionIconFileName,
                mnuSelectPolygon,
                mapSelectPolygonToolStripMenuItem);
            ApplySelectionToolImage(
                IntersectingPolySelectionIconFileName,
                mnuSelectIntersectingPoly,
                mapSelectIntersectPolyToolStripMenuItem);
            ApplySelectionToolImage(
                IntersectingLineSelectionIconFileName,
                mnuSelectIntersectingLine,
                mapSelectIntersectLineToolStripMenuItem);
        }

        private static void ApplySelectionToolImage(
            string fileName,
            params ToolStripItem[] targets)
        {
            Image? image = TryLoadSelectionToolImage(fileName);
            if (image == null)
                return;

            foreach (ToolStripItem target in targets)
            {
                target.Image = image;
                target.ImageTransparentColor = Color.Magenta;
            }
        }

        private static Image? TryLoadSelectionToolImage(string fileName)
        {
            foreach (string path in GetSelectionToolIconCandidatePaths(fileName))
            {
                if (!File.Exists(path))
                    continue;

                try
                {
                    using Image source = Image.FromFile(path);
                    return new Bitmap(source);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static IEnumerable<string> GetSelectionToolIconCandidatePaths(string fileName)
        {
            yield return Path.Combine(
                AppContext.BaseDirectory,
                "Resources",
                SelectionToolIconFolderName,
                fileName);

            yield return Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "Resources",
                SelectionToolIconFolderName,
                fileName));
        }

        private static bool IsSelectionToolbarCanvasTool(MapCanvasTool tool)
        {
            return tool is MapCanvasTool.Select
                or MapCanvasTool.SelectionPolygon
                or MapCanvasTool.SelectionIntersectingPolygon
                or MapCanvasTool.SelectionIntersectingLine;
        }

        private static Control? GetFocusedControl(Control parent)
        {
            if (parent.Focused)
                return parent;

            foreach (Control child in parent.Controls)
            {
                if (!child.ContainsFocus)
                    continue;

                Control? focusedChild = GetFocusedControl(child);
                return focusedChild ?? child;
            }

            return null;
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
            if (!EnsureApplicationUnlockedForEditing("drawing"))
            {
                ActivateCanvasTool(MapCanvasTool.Select);
                return;
            }

            // If a specific drawing layer is selected but it is locked and/or
            // hidden, tell the user exactly that instead of the misleading
            // "select a <type> drawing layer" (type-mismatch) message.
            CanvasLayer? selectedDrawingLayer = GetSelectedCurrentDrawingLayer();
            if (selectedDrawingLayer != null &&
                (selectedDrawingLayer.IsLocked || !selectedDrawingLayer.IsVisible))
            {
                string state =
                    selectedDrawingLayer.IsLocked && !selectedDrawingLayer.IsVisible ? "locked and hidden" :
                    selectedDrawingLayer.IsLocked ? "locked" :
                    "hidden";
                MessageBox.Show(
                    $"The layer '{selectedDrawingLayer.Name}' is {state}. " +
                    "Unlock and show the layer before drawing on it.",
                    "Drawing Not Allowed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                ActivateCanvasTool(MapCanvasTool.Select);
                return;
            }

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
            SetSelectSplitButtonActive(IsSelectionToolbarCanvasTool(tool));
            mnuDrawPoint.Checked = tool == MapCanvasTool.Point;
            mnuDrawLine.Checked = tool == MapCanvasTool.Line;
            mnuDrawPolyline.Checked = tool == MapCanvasTool.Polyline;
            mnuDrawPolygon.Checked = tool == MapCanvasTool.Polygon;
            mnuDrawRectangle.Checked = tool == MapCanvasTool.Rectangle;
            mnuDrawCircle.Checked = tool == MapCanvasTool.Circle;
            mnuDrawArc.Checked = tool == MapCanvasTool.Arc;
            mnuDrawText.Checked = tool == MapCanvasTool.Text;
        }

        private void SetSelectSplitButtonActive(bool active)
        {
            mnuSelectTool.BackColor = active
                ? SystemColors.ControlLight
                : SystemColors.Control;
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
            if (!EnsureApplicationUnlockedForEditing("creating drawing layers"))
            {
                throw new InvalidOperationException("Project Edit Lock is active.");
            }

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
            bool isTextLayer = tool == MapCanvasTool.Text;
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
                LineWeight = isTextLayer ? 0.0 : 1.3,
                LineStyle = "Solid",
                LineTypeScale = 1.0,
                FillColor = null,
                ShowFillTransparency = false,
                FillTransparency = 50,
                FillStyle = "None",
                LabelColor = isTextLayer ? drawingColor : "#000000",
                LabelFontName = "Nirmala UI",
                LabelFontSize = isTextLayer ? 10.0 : 1.0,
                LabelScaleWithZoom = !isTextLayer,
                TextAlignment = "Center Middle",
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
                MapCanvasTool.Point => "Points",
                MapCanvasTool.Line or MapCanvasTool.Polyline or MapCanvasTool.Arc => "Polylines",
                MapCanvasTool.Rectangle or MapCanvasTool.Polygon or MapCanvasTool.Circle => "Polygons",
                MapCanvasTool.Text => "Annotation",
                _ => "Polylines"
            };
        }

        private static string GetDefaultDrawingLayerNameForLayerType(string? layerType)
        {
            string normalizedType = layerType?.Trim().ToLowerInvariant() ?? string.Empty;
            return normalizedType switch
            {
                "point" => "Points",
                "polygon" => "Polygons",
                "annotation" => "Annotation",
                "drawingmarkup" or "line" or "polyline" => "Polylines",
                _ => "Polylines"
            };
        }

        private static string ResolveDrawingLayerTypeForTool(MapCanvasTool tool)
        {
            return tool switch
            {
                MapCanvasTool.Point => CanvasLayerTreeService.PointLayerType,
                MapCanvasTool.Line or MapCanvasTool.Polyline or MapCanvasTool.Arc => CanvasLayerTreeService.PolylineLayerType,
                MapCanvasTool.Rectangle or MapCanvasTool.Polygon or MapCanvasTool.Circle => CanvasLayerTreeService.PolygonLayerType,
                MapCanvasTool.Text => CanvasLayerTreeService.AnnotationLayerType,
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
            mnuDrawText.Enabled = enabled &&
                                  (allowAllForAutoLayerCreation ||
                                   IsDrawingLayerCompatibleWithTool(selectedLayer, MapCanvasTool.Text));

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
            CanvasLayer? selected = cboCurrentDrawingLayer.SelectedItem is DrawingLayerComboItem item
                ? item.Layer
                : _currentDrawingLayer;

            if (selected == null)
            {
                return null;
            }

            // The combo item holds a snapshot captured when the combo was last
            // populated; if the layer's visibility/lock was toggled afterward
            // (via the layer tree) that snapshot is stale and wrongly reports
            // the layer as hidden/locked. Re-resolve the live layer from the
            // tree so callers always see the current IsVisible/IsLocked state.
            return GetDrawingMarkupLayersFromTree()
                       .FirstOrDefault(layer => layer.Id == selected.Id)
                   ?? selected;
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

        private void CurrentDrawingLayerCombo_FocusChanged(object? sender, EventArgs e)
        {
            if (sender is ComboBox combo)
            {
                combo.Invalidate();
            }
        }

        private void CurrentDrawingLayerCombo_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ComboBox combo)
                return;

            // The closed display (edit portion) is drawn separately from the dropdown
            // rows. It must NOT use the full-blue "selected" fill: that high-contrast
            // repaint on focus is what flickered. Focus is shown as an accent border
            // instead (drawn last). Only dropdown rows use the highlight fill.
            bool isEditPortion =
                (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
            bool selected = !isEditPortion &&
                            (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool disabled = (e.State & DrawItemState.Disabled) == DrawItemState.Disabled || !combo.Enabled;
            Color backgroundColor = selected ? SystemColors.Highlight : combo.BackColor;
            Color textColor = disabled
                ? SystemColors.GrayText
                : selected
                    ? SystemColors.HighlightText
                    : combo.ForeColor;

            using (SolidBrush backgroundBrush = new(backgroundColor))
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);

            if (e.Index >= 0 && e.Index < combo.Items.Count)
            {
                object? item = combo.Items[e.Index];
                if (item is DrawingLayerComboItem layerItem)
                {
                    CanvasLayer layer = layerItem.Layer;
                    Color itemTextColor = textColor;
                    if (layer.IsLocked && !selected && !disabled)
                    {
                        itemTextColor = BlendColor(combo.ForeColor, combo.BackColor, 0.48f);
                    }

                    int swatchSize = Math.Min(LayerNodeColorBoxSize, Math.Max(1, e.Bounds.Height - 6));
                    Rectangle swatchRect = new(
                        e.Bounds.Left + CurrentDrawingLayerComboPadding,
                        e.Bounds.Top + Math.Max(0, (e.Bounds.Height - swatchSize) / 2),
                        swatchSize,
                        swatchSize);
                    DrawCurrentDrawingLayerComboSwatch(e.Graphics, swatchRect, layer, combo.BackColor);

                    int textLeft = swatchRect.Right + LayerNodeColorBoxGap;
                    Rectangle textRect = new(
                        textLeft,
                        e.Bounds.Top,
                        Math.Max(0, e.Bounds.Right - textLeft - CurrentDrawingLayerComboPadding),
                        e.Bounds.Height);

                    TextRenderer.DrawText(
                        e.Graphics,
                        layer.Name,
                        combo.Font,
                        textRect,
                        itemTextColor,
                        TextFormatFlags.Left |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPrefix);
                }
                else
                {
                    TextRenderer.DrawText(
                        e.Graphics,
                        item?.ToString() ?? string.Empty,
                        combo.Font,
                        e.Bounds,
                        textColor,
                        TextFormatFlags.Left |
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis |
                        TextFormatFlags.NoPrefix);
                }
            }

            if (isEditPortion)
            {
                // Show focus as a crisp accent border around the closed display.
                if (combo.Focused && !disabled)
                {
                    Rectangle borderRect = e.Bounds;
                    borderRect.Width -= 1;
                    borderRect.Height -= 1;
                    using Pen focusPen = new(SystemColors.Highlight, 1f);
                    e.Graphics.DrawRectangle(focusPen, borderRect);
                }
            }
            else
            {
                e.DrawFocusRectangle();
            }
        }

        private void DrawCurrentDrawingLayerComboSwatch(
            Graphics g,
            Rectangle rect,
            CanvasLayer layer,
            Color backgroundColor)
        {
            if (CanvasLayerTreeService.IsAnnotationLayer(layer))
            {
                DrawAnnotationLayerSwatch(g, rect, layer, backgroundColor);
            }
            else if (CanvasLayerTreeService.IsPointLayer(layer))
            {
                DrawPointLayerSwatch(g, rect, layer, backgroundColor);
            }
            else if (CanvasLayerTreeService.IsLineLayer(layer))
            {
                DrawLineLayerSwatch(g, rect, layer, backgroundColor);
            }
            else
            {
                DrawVectorLayerSwatch(g, rect, layer, backgroundColor);
            }
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
            _layerContextMenu.Renderer = ElegantMenuRenderer.Instance;
            _layerContextMenu.Opening += LayerContextMenu_Opening;
            _mnuToggleLayerVisibility.CheckOnClick = false;
            _mnuToggleLayerLock.CheckOnClick = false;
            _mnuToggleLayerGroupLock.CheckOnClick = false;
            _mnuToggleLayerGroupSelection.CheckOnClick = false;
            _mnuToggleLayerLabels.CheckOnClick = false;
            _mnuToggleFillTransparency.CheckOnClick = false;
            _mnuAddRasterMap.Click += async (_, _) => await ImportRasterFileAsync(
                "Add Raster Map",
                GetGeneralRasterImportFilter());
            _mnuAddXyzTiles.Click += async (_, _) =>
                await ShowXyzTileImportOptionsFormAsync();
            _mnuAddExternalLayers.Click += async (_, _) =>
                await ShowImportExternalLayersWorkflowAsync();
            _mnuZoomToLayer.Click += async (_, _) => await ZoomToCurrentLayerTreeContextAsync();
            _mnuRenameLayer.Click += (_, _) => BeginLayerRename(_contextLayerNode);
            _mnuDeleteLayer.Click += async (_, _) => await DeleteLayerAsync(_contextLayerNode);
            _mnuDeleteLayerGroup.Click += async (_, _) => await DeleteLayerGroupAsync(_contextLayerGroupNode);
            _mnuDeleteExternalLayerGroup.Click += async (_, _) => await DeleteExternalLayerGroupAsync(_contextLayerGroupNode);
            _mnuAddDrawingLayer.Click += async (_, _) => await AddDrawingMarkupLayerAsync(_contextLayerGroupNode);
            _mnuSetActiveLayer.Click += (_, _) =>
            {
                CanvasLayer? layer = GetLayerFromNode(_contextLayerNode);
                if (layer != null)
                    SetCurrentDrawingLayer(layer);
            };
            _mnuMoveLayerUp.Click += async (_, _) => await MoveLayerInDisplayOrderAsync(_contextLayerNode, -1);
            _mnuMoveLayerDown.Click += async (_, _) => await MoveLayerInDisplayOrderAsync(_contextLayerNode, 1);
            _mnuToggleLayerVisibility.Click += async (_, _) => await ToggleLayerNodeVisibilityAsync(_contextLayerNode);
            _mnuToggleLayerLock.Click += async (_, _) => await ToggleLayerLockAsync(_contextLayerNode);
            _mnuToggleLayerGroupLock.Click += async (_, _) => await ToggleLayerGroupLockAsync(_contextLayerGroupNode);
            _mnuToggleLayerGroupSelection.Click += async (_, _) => await ToggleLayerGroupSelectionAsync(_contextLayerGroupNode);
            _mnuToggleLayerLabels.Click += async (_, _) => await ToggleLayerLabelsAsync(_contextLayerNode);
            _mnuToggleFillTransparency.Click += async (_, _) => await ToggleLayerFillTransparencyAsync(_contextLayerNode);
            _mnuLayerProperties.Click += async (_, _) => await OpenLayerPropertyManagerAsync(_contextLayerNode);
            _mnuGenerateRoadParcel.Click += async (_, _) => await GenerateRoadParcelFromLayerContextAsync();
            treeViewLayers.ContextMenuStrip = _layerContextMenu;
        }

        private void ConfigureLayerPropertiesPanel()
        {
            grpParcelObjProp.ForeColor = Color.FromArgb(39, 55, 77);
            grpParcelObjProp.Padding = new Padding(8);
            dgvParcelObjProperty.ReadOnly = false;
            dgvParcelObjProperty.EditMode = DataGridViewEditMode.EditOnEnter;
            if (dgvParcelObjProperty.Columns.Count > 1)
            {
                dgvParcelObjProperty.Columns[0].ReadOnly = true;
                dgvParcelObjProperty.Columns[1].ReadOnly = false;
            }

            dgvParcelObjProperty.CellBeginEdit -= dgvParcelObjProperty_CellBeginEdit;
            dgvParcelObjProperty.CellBeginEdit += dgvParcelObjProperty_CellBeginEdit;
            dgvParcelObjProperty.CellEndEdit -= dgvParcelObjProperty_CellEndEdit;
            dgvParcelObjProperty.CellEndEdit += dgvParcelObjProperty_CellEndEdit;
            SetPropertiesPanelTitle();
            ShowNoParcelSelection();
        }

        private void dgvParcelObjProperty_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1)
            {
                e.Cancel = true;
                return;
            }

            DataGridViewRow row = dgvParcelObjProperty.Rows[e.RowIndex];
            e.Cancel = row.Tag is not string key ||
                       _isApplicationEditLocked ||
                       !IsEditablePropertyGridValueKey(key) ||
                       row.Cells[1].ReadOnly;
        }

        private void dgvParcelObjProperty_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 1)
                return;

            DataGridViewRow row = dgvParcelObjProperty.Rows[e.RowIndex];
            if (row.Tag is not string key ||
                !IsEditablePropertyGridValueKey(key))
            {
                return;
            }

            string newValue = row.Cells[1].Value?.ToString() ?? string.Empty;
            BeginInvoke(new Action(() => CommitPropertyGridValueEditAsync(key, newValue)));
        }

        private async void CommitPropertyGridValueEditAsync(string key, string newValue)
        {
            try
            {
                await CommitPropertyGridValueEditCoreAsync(key, newValue);
            }
            catch (Exception ex)
            {
                SetCanvasCommandStatus($"Property update failed: {ex.Message}");
            }
        }

        private async Task CommitPropertyGridValueEditCoreAsync(string key, string newValue)
        {
            if (!EnsureApplicationUnlockedForEditing("editing selected object properties"))
            {
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync();
                return;
            }

            if (string.Equals(key, "text.value", StringComparison.OrdinalIgnoreCase))
            {
                await UpdateSelectedTextObjectValueAsync(newValue);
                return;
            }

            if (IsEditableBlockPropertyKey(key))
                await UpdateSelectedBlockPropertyAsync(key, newValue);
        }

        private static bool IsEditablePropertyGridValueKey(string key) =>
            string.Equals(key, "text.value", StringComparison.OrdinalIgnoreCase) ||
            IsEditableBlockPropertyKey(key);

        private static bool IsEditableBlockPropertyKey(string key) =>
            string.Equals(key, "block.name", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "block.depth", StringComparison.OrdinalIgnoreCase);

        private async Task UpdateSelectedTextObjectValueAsync(string newText)
        {
            CanvasObject? selectedObject = GetCurrentSinglePropertyGridObject();
            if (selectedObject == null || !IsTextObject(selectedObject) || !AppServices.HasContext)
                return;

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            CanvasObject? entity = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .FirstOrDefaultAsync(item => item.Id == selectedObject.Id);
            if (entity == null)
                return;

            if (entity.CanvasLayer?.IsLocked == true || entity.IsLocked)
            {
                SetCanvasCommandStatus("Text edit ignored because the object or layer is locked");
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                return;
            }

            string text = newText.Trim();
            if (string.Equals(entity.LabelText ?? string.Empty, text, StringComparison.Ordinal))
                return;

            CanvasObject beforeSnapshot = CloneCanvasObjectSnapshot(entity);
            entity.LabelText = text;
            entity.LastModifiedDate = DateTime.Now;
            await context.SaveChangesAsync();

            CanvasObject afterSnapshot = CloneCanvasObjectSnapshot(entity);
            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            await RefreshCurrentSelectedCanvasObjectPropertiesAsync(entity.Id);
            RegisterCanvasUndoCommand(new ModifyCanvasObjectCommand(beforeSnapshot, afterSnapshot));
            SetCanvasCommandStatus("Updated text object");
        }

        private async Task UpdateSelectedBlockPropertyAsync(string key, string newValue)
        {
            CanvasObject? selectedObject = GetCurrentSinglePropertyGridObject();
            if (selectedObject == null || !IsBlockObject(selectedObject) || !AppServices.HasContext)
                return;

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            CanvasObject? canvasObject = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Include(item => item.Block)
                .FirstOrDefaultAsync(item => item.Id == selectedObject.Id);
            if (canvasObject == null)
                return;

            if (canvasObject.CanvasLayer?.IsLocked == true || canvasObject.IsLocked)
            {
                SetCanvasCommandStatus("Block edit ignored because the object or layer is locked");
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                return;
            }

            int? blockId = canvasObject.BlockId ?? canvasObject.Block?.Id;
            if (!blockId.HasValue)
            {
                SetCanvasCommandStatus("Block edit ignored because this object has no assigned block data");
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                return;
            }

            Core.Entities.Layout.Block? block = await context.Blocks
                .FirstOrDefaultAsync(item => item.Id == blockId.Value);
            if (block == null)
            {
                SetCanvasCommandStatus("Block edit ignored because the assigned block data was not found");
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                return;
            }

            bool changed;
            if (string.Equals(key, "block.name", StringComparison.OrdinalIgnoreCase))
            {
                string blockName = newValue.Trim();
                if (string.IsNullOrWhiteSpace(blockName))
                {
                    SetCanvasCommandStatus("Block name cannot be empty");
                    await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                    return;
                }

                changed = !string.Equals(block.BlockName ?? string.Empty, blockName, StringComparison.Ordinal);
                if (changed)
                    block.BlockName = blockName;
            }
            else if (string.Equals(key, "block.depth", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseLengthMeters(newValue, out double depth) || depth < 0)
                {
                    SetCanvasCommandStatus("Block depth must be a valid non-negative number");
                    await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                    return;
                }

                float depthValue = Convert.ToSingle(depth);
                changed = Math.Abs(block.BlockDepth - depthValue) > 0.0001f;
                if (changed)
                    block.BlockDepth = depthValue;
            }
            else
            {
                return;
            }

            if (!changed)
            {
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
                return;
            }

            block.LastModifiedDate = DateTime.Now;
            await context.SaveChangesAsync();

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
            SetCanvasCommandStatus("Updated block data");
        }

        private static bool TryParseLengthMeters(string? text, out double value)
        {
            value = 0;
            string normalized = (text ?? string.Empty)
                .Trim()
                .Replace("m", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(",", string.Empty);

            return double.TryParse(
                normalized,
                NumberStyles.Float,
                CultureInfo.CurrentCulture,
                out value) ||
                double.TryParse(
                    normalized,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value);
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

            Rectangle rowRect = new Rectangle(
                e.Bounds.X,
                e.Bounds.Y,
                treeViewLayers.ClientSize.Width - e.Bounds.X,
                e.Bounds.Height);

            using (SolidBrush backBrush = new(treeViewLayers.BackColor))
                g.FillRectangle(backBrush, rowRect);

            Color textColor = selected
                ? Color.White
                : treeViewLayers.ForeColor;

            int x = e.Bounds.X;

            if (isLayer)
            {
                CanvasLayer layer = activeLayer!;
                bool isLockedLayer = layer.IsLocked;

                if (isLockedLayer)
                {
                    textColor = selected
                        ? Color.White
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

            Rectangle textRect = GetLayerNodeTextRect(e.Node, x);
            if (selected)
            {
                Rectangle highlightRect = GetLayerNodeTextHighlightRect(g, e.Node.Text, textRect);
                using SolidBrush highlightBrush = new(Color.FromArgb(0, 120, 215));
                g.FillRectangle(highlightBrush, highlightRect);
            }

            TextRenderer.DrawText(
                g,
                e.Node.Text,
                treeViewLayers.Font,
                textRect,
                textColor,
                TextFormatFlags.Left |
                TextFormatFlags.SingleLine |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix);

            // A locked layer gets a small padlock glyph trailing its name so the
            // lock state is visible at a glance; unlocking simply omits the glyph.
            if (isLayer && activeLayer!.IsLocked)
            {
                Size textSize = TextRenderer.MeasureText(
                    g,
                    e.Node.Text,
                    treeViewLayers.Font,
                    new Size(int.MaxValue, textRect.Height),
                    TextFormatFlags.Left |
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.NoPrefix);

                int lockHeight = 10;
                int lockWidth = 8;
                int lockX = textRect.X + Math.Min(textSize.Width, textRect.Width) + LayerNodeLockIconGap;
                int lockY = textRect.Y + Math.Max(0, (textRect.Height - lockHeight) / 2);

                Rectangle lockRect = new Rectangle(lockX, lockY, lockWidth, lockHeight);

                if (lockRect.Right <= treeViewLayers.ClientSize.Width - 2)
                {
                    // The lock sits past the selection highlight (which only spans the
                    // text), so keep it dark on the tree background in every state.
                    Color lockColor = Color.FromArgb(70, 70, 70); // simple dark padlock
                    DrawLockedLayerIcon(g, lockRect, lockColor, treeViewLayers.BackColor);
                }
            }
        }

        // Draws a padlock built from GDI+ primitives: a rounded body with a
        // keyhole punched out, topped by a U-shaped shackle arc.
        private static void DrawLockedLayerIcon(
            Graphics g,
            Rectangle rect,
            Color color,
            Color backColor)
        {
            SmoothingMode previousSmoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                int bodyHeight = Math.Max(5, (int)Math.Round(rect.Height * 0.6));
                Rectangle body = new Rectangle(
                    rect.X,
                    rect.Bottom - bodyHeight,
                    rect.Width,
                    bodyHeight);

                int shackleWidth = Math.Max(4, (int)Math.Round(rect.Width * 0.62));
                int shackleTop = rect.Y;
                int shackleHeight = body.Top - shackleTop;
                Rectangle shackleArc = new Rectangle(
                    rect.X + (rect.Width - shackleWidth) / 2,
                    shackleTop,
                    shackleWidth,
                    Math.Max(4, shackleHeight * 2));

                using (Pen shacklePen = new Pen(color, 1.1f))
                {
                    // Top half of an ellipse forms the U-shaped shackle.
                    g.DrawArc(shacklePen, shackleArc, 180f, 180f);
                }

                using (GraphicsPath bodyPath = CreateRoundedRectanglePath(body, 1.6f))
                using (SolidBrush bodyBrush = new SolidBrush(color))
                {
                    g.FillPath(bodyBrush, bodyPath);
                }

                // Keyhole: a small dot punched in the body using the background colour.
                int holeSize = Math.Max(2, body.Width / 3);
                Rectangle hole = new Rectangle(
                    body.X + (body.Width - holeSize) / 2,
                    body.Y + (body.Height - holeSize) / 2,
                    holeSize,
                    holeSize);
                using (SolidBrush holeBrush = new SolidBrush(backColor))
                {
                    g.FillEllipse(holeBrush, hole);
                }
            }
            finally
            {
                g.SmoothingMode = previousSmoothing;
            }
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
                                       string.Equals(key, ExternalLayerGroupKey, StringComparison.OrdinalIgnoreCase) ||
                                       string.Equals(key, CadastralMapGroupKey, StringComparison.OrdinalIgnoreCase),
                    IsGroupCheckedWhenEmpty = string.Equals(key, CadastralMapGroupKey, StringComparison.OrdinalIgnoreCase),
                    GroupKey = key
                }
            };
        }

        private static TreeNode CreateExternalFileGroupNode(string sourceFile, int index)
        {
            return new TreeNode(GetExternalLayerSourceDisplayName(sourceFile))
            {
                Name = $"{LayerGroupNodeNamePrefix}{ExternalLayerGroupKey}_File_{index}",
                Tag = new LayerTreeNodeState
                {
                    IsLayerNode = false,
                    IsCheckableGroup = true,
                    GroupKey = ExternalLayerGroupKey
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

        private void SetPropertiesPanelTitle()
        {
            grpParcelObjProp.Text = "Properties";
            if (grpParcelObjProp.Font.Style != FontStyle.Bold)
            {
                grpParcelObjProp.Font = new Font(
                    grpParcelObjProp.Font,
                    grpParcelObjProp.Font.Style | FontStyle.Bold);
            }
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

            if (IsExternalFileGroupNode(targetNode))
            {
                _contextLayerNode = null;
                _contextLayerGroupNode = targetNode;
                treeViewLayers.SelectedNode = targetNode;
                ConfigureExternalFileGroupContextMenuItems();
                _mnuZoomToLayer.Enabled = EnumerateLayerNodes(targetNode!)
                    .Select(GetLayerFromNode)
                    .Any(layer => layer != null && !IsOnlineBasemapLayer(layer));
                return;
            }

            if (IsExternalLayerGroupNode(targetNode))
            {
                _contextLayerNode = null;
                _contextLayerGroupNode = targetNode;
                treeViewLayers.SelectedNode = targetNode;
                ConfigureExternalGroupContextMenuItems();
                _mnuZoomToLayer.Enabled = EnumerateLayerNodes(targetNode!)
                    .Select(GetLayerFromNode)
                    .Any(layer => layer != null && !IsOnlineBasemapLayer(layer));
                return;
            }

            if (IsRePlotDataLayerGroupNode(targetNode))
            {
                _contextLayerNode = null;
                _contextLayerGroupNode = targetNode;
                treeViewLayers.SelectedNode = targetNode;
                ConfigureRePlotDataGroupContextMenuItems(targetNode);
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
            _mnuToggleLayerLabels.Text = "Show Labels";
            _mnuToggleFillTransparency.Text = "Show Transparency";
            _mnuToggleLayerVisibility.Checked = !layer.IsVisible;
            _mnuToggleLayerLock.Checked = layer.IsLocked;
            _mnuToggleLayerLabels.Checked = layer.ShowLabels;
            _mnuToggleFillTransparency.Checked = layer.ShowFillTransparency;

            bool isProtectedDefaultLayer =
                CanvasLayerTreeService.IsProtectedDefaultLayer(layer);
            _mnuZoomToLayer.Enabled = !IsOnlineBasemapLayer(_contextLayerNode);
            bool canEditLayerDefinition = !_isApplicationEditLocked;
            _mnuRenameLayer.Enabled = canEditLayerDefinition && CanBeginLayerRename(_contextLayerNode, layer);
            _mnuDeleteLayer.Enabled = canEditLayerDefinition && !IsOnlineBasemapLayer(_contextLayerNode);
            bool canReorderRaster =
                IsRasterLayer(layer) &&
                !IsOnlineBasemapLayer(_contextLayerNode);
            bool canReorderDrawing = CanvasLayerTreeService.IsDrawingMarkupLayer(layer);
            _mnuMoveLayerUp.Enabled =
                canEditLayerDefinition &&
                ((canReorderRaster && CanMoveRasterLayerInDisplayOrder(_contextLayerNode, -1)) ||
                 (canReorderDrawing && CanMoveDrawingLayerInDisplayOrder(_contextLayerNode, -1)));
            _mnuMoveLayerDown.Enabled =
                canEditLayerDefinition &&
                ((canReorderRaster && CanMoveRasterLayerInDisplayOrder(_contextLayerNode, 1)) ||
                 (canReorderDrawing && CanMoveDrawingLayerInDisplayOrder(_contextLayerNode, 1)));
            _mnuToggleLayerVisibility.Enabled = true;
            _mnuToggleLayerLock.Enabled = canEditLayerDefinition;
            _mnuToggleLayerLabels.Enabled = canEditLayerDefinition && CanLayerDisplayLabels(layer);
            _mnuToggleFillTransparency.Enabled = canEditLayerDefinition && CanLayerUseFillTransparency(layer);
            _mnuLayerProperties.Enabled = canEditLayerDefinition;
            ConfigureCreateFeaturesFromLayerMenu(_contextLayerNode, layer);

            if (BlockRoadParcelRefreshService.IsRoadParcelLayer(layer))
            {
                _mnuGenerateRoadParcel.Enabled = canEditLayerDefinition;
                _layerContextMenu.Items.Insert(2, _mnuGenerateRoadParcel);
                _layerContextMenu.Items.Insert(3, new ToolStripSeparator());
            }
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

        private static Color FadeLockedLayerColorForTree(Color color, Color backgroundColor)
        {
            return BlendColor(color, backgroundColor, 0.48f);
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
                !string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase);
            int effectiveTransparency = layer.ShowFillTransparency
                ? Math.Clamp(layer.FillTransparency, 0, 100)
                : 0;

            Color rawFillColor = hasFill
                ? ParseColorOrDefault(layer.FillColor, Color.White)
                : backgroundColor;

            if (layer.IsLocked)
            {
                outlineColor = FadeLockedLayerColorForTree(outlineColor, backgroundColor);
                rawFillColor = FadeLockedLayerColorForTree(rawFillColor, backgroundColor);
            }

            Color fillColor = hasFill
                ? BlendColor(rawFillColor, backgroundColor, effectiveTransparency / 100f)
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
                    effectiveTransparency,
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
            if (layer.IsLocked)
                lineColor = FadeLockedLayerColorForTree(lineColor, backgroundColor);

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
            if (layer.IsLocked)
                markerColor = FadeLockedLayerColorForTree(markerColor, backgroundColor);

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
            if (layer.IsLocked)
                textColor = FadeLockedLayerColorForTree(textColor, backgroundColor);

            using Font font = new(
                "Segoe UI",
                Math.Max(7.0f, rect.Height - 8.0f),
                FontStyle.Bold,
                GraphicsUnit.Pixel);
            TextRenderer.DrawText(
                g,
                "Aa",
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
                if (centerline.IsLocked)
                    lineColor = FadeLockedLayerColorForTree(lineColor, backgroundColor);

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
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale];
                    break;
                case "DOTTED":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                    pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    pen.DashPattern = [0.1f, Math.Max(1.5f, 2f * scale)];
                    break;
                case "DASHDOT":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case "DASHDOUBLEDOT":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case "CENTERLINE":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashCap = System.Drawing.Drawing2D.DashCap.Flat;
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
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant() switch
            {
                "DASH" => "DASHED",
                "DOT" => "DOTTED",
                "DASHDOTDOT" => "DASHDOUBLEDOT",
                _ => (lineStyle ?? string.Empty)
                    .Replace("-", string.Empty, StringComparison.Ordinal)
                    .Replace("_", string.Empty, StringComparison.Ordinal)
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
            _mnuAddRasterMap.Enabled = !_isApplicationEditLocked;
            _mnuAddXyzTiles.Enabled = !_isApplicationEditLocked;
        }

        private void ConfigureDrawingMarkupGroupContextMenuItems()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.AddRange(
            [
                _mnuAddDrawingLayer,
                new ToolStripSeparator(),
                _mnuToggleLayerGroupSelection
            ]);

            _mnuAddDrawingLayer.Enabled = !_isApplicationEditLocked;
            ConfigureLayerGroupSelectionMenuItem(_contextLayerGroupNode);
        }

        private void ConfigureExternalGroupContextMenuItems()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.AddRange(
            [
                _mnuAddExternalLayers,
                new ToolStripSeparator(),
                _mnuZoomToLayer,
                new ToolStripSeparator(),
                _mnuToggleLayerGroupSelection
            ]);

            _mnuAddExternalLayers.Enabled = !_isApplicationEditLocked;
            ConfigureLayerGroupSelectionMenuItem(_contextLayerGroupNode);
        }

        private void ConfigureExternalFileGroupContextMenuItems()
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.AddRange(
            [
                _mnuZoomToLayer,
                new ToolStripSeparator(),
                _mnuDeleteExternalLayerGroup,
                new ToolStripSeparator(),
                _mnuToggleLayerGroupSelection
            ]);

            _mnuDeleteExternalLayerGroup.Enabled = !_isApplicationEditLocked;
            ConfigureLayerGroupSelectionMenuItem(_contextLayerGroupNode);
        }

        private void ConfigureRePlotDataGroupContextMenuItems(TreeNode? groupNode)
        {
            _layerContextMenu.Items.Clear();
            _layerContextMenu.Items.AddRange(
            [
                _mnuZoomToLayer,
                new ToolStripSeparator(),
                _mnuDeleteLayerGroup,
                new ToolStripSeparator(),
                _mnuToggleLayerGroupLock,
                _mnuToggleLayerGroupSelection
            ]);

            List<CanvasLayer> layers = GetLayersFromGroupNode(groupNode);
            _mnuZoomToLayer.Enabled = layers.Any(layer => !IsOnlineBasemapLayer(layer));
            _mnuDeleteLayerGroup.Enabled = !_isApplicationEditLocked && layers.Count > 0;
            _mnuToggleLayerGroupLock.Enabled = !_isApplicationEditLocked && layers.Count > 0;
            _mnuToggleLayerGroupLock.Checked = layers.Count > 0 && layers.All(layer => layer.IsLocked);
            // Keep a constant "Locked" label; lock state is conveyed by the
            // menu item's checkbox (checked = locked), matching the single-layer
            // lock menu item.
            _mnuToggleLayerGroupLock.Text = "Locked";

            ConfigureLayerGroupSelectionMenuItem(groupNode);
        }

        private void ConfigureLayerGroupSelectionMenuItem(TreeNode? groupNode)
        {
            List<CanvasLayer> vectorLayers = GetSelectableVectorLayersFromGroupNode(groupNode);
            bool allSelectable = vectorLayers.Count > 0 && vectorLayers.All(layer => layer.IsSelectable);
            bool anySelectable = vectorLayers.Any(layer => layer.IsSelectable);

            _mnuToggleLayerGroupSelection.Enabled = vectorLayers.Count > 0;
            if (_isApplicationEditLocked)
                _mnuToggleLayerGroupSelection.Enabled = false;
            _mnuToggleLayerGroupSelection.Checked = allSelectable;
            _mnuToggleLayerGroupSelection.Text = !anySelectable
                ? "Allow Selection"
                : allSelectable
                    ? "Allow Selection"
                    : "Allow Selection (Mixed)";
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
                _mnuCreateFeaturesFromLayer,
                new ToolStripSeparator(),
                _mnuMoveLayerUp,
                _mnuMoveLayerDown,
                new ToolStripSeparator(),
                _mnuToggleLayerVisibility,
                _mnuToggleLayerLock,
                _mnuToggleLayerLabels,
                _mnuToggleFillTransparency,
                new ToolStripSeparator(),
                _mnuLayerProperties
            ]);
        }

        private void ConfigureCreateFeaturesFromLayerMenu(
            TreeNode? sourceNode,
            CanvasLayer sourceLayer)
        {
            _mnuCreateFeaturesFromLayer.DropDownItems.Clear();
            if (_isApplicationEditLocked)
            {
                _mnuCreateFeaturesFromLayer.DropDownItems.Add(new ToolStripMenuItem("Project Edit Lock is active")
                {
                    Enabled = false
                });
                _mnuCreateFeaturesFromLayer.Enabled = false;
                return;
            }

            bool canTransfer = CanTransferObjectsFromLayer(sourceNode, sourceLayer);
            if (canTransfer)
            {
                AddFeatureTargetMenuItems(
                    _mnuCreateFeaturesFromLayer.DropDownItems,
                    GetDefaultTransferTargetLayers(sourceNode, sourceLayer),
                    targetLayer => CreateFeaturesFromLayerAsync(sourceLayer, targetLayer));
            }

            if (canTransfer && _mnuCreateFeaturesFromLayer.DropDownItems.Count == 0)
            {
                _mnuCreateFeaturesFromLayer.DropDownItems.Add(new ToolStripMenuItem("No compatible feature layers")
                {
                    Enabled = false
                });
            }

            bool hasTargets = _mnuCreateFeaturesFromLayer.DropDownItems
                .Cast<ToolStripItem>()
                .Any(item => item.Enabled);
            _mnuCreateFeaturesFromLayer.Enabled = canTransfer && hasTargets;
        }

        private bool CanTransferObjectsFromLayer(TreeNode? sourceNode, CanvasLayer sourceLayer)
        {
            if (!AppServices.HasContext ||
                sourceNode == null ||
                sourceLayer.IsLocked ||
                IsRasterLayer(sourceLayer) ||
                CanvasLayerTreeService.IsProtectedDefaultLayer(sourceLayer) ||
                IsCadastralCanvasLayer(sourceLayer))
            {
                return false;
            }

            string? groupKey = GetLayerGroupKeyForNode(sourceNode);
            return string.Equals(groupKey, DrawingMarkupGroupKey, StringComparison.OrdinalIgnoreCase) ||
                   (string.Equals(groupKey, ExternalLayerGroupKey, StringComparison.OrdinalIgnoreCase) &&
                    CanvasLayerTreeService.IsExternalImportedLayer(sourceLayer));
        }

        private void AddFeatureTargetMenuItems(
            ToolStripItemCollection menuItems,
            IEnumerable<FeatureTransferTarget> targets,
            Func<CanvasLayer, Task> onTargetClick)
        {
            string? currentGroup = null;
            foreach (FeatureTransferTarget target in targets)
            {
                if (!string.Equals(currentGroup, target.GroupName, StringComparison.OrdinalIgnoreCase))
                {
                    if (menuItems.Count > 0)
                        menuItems.Add(new ToolStripSeparator());

                    ToolStripMenuItem groupHeader = new(target.GroupName)
                    {
                        Enabled = false,
                        Font = new Font(_layerContextMenu.Font, FontStyle.Bold)
                    };
                    menuItems.Add(groupHeader);
                    currentGroup = target.GroupName;
                }

                ToolStripMenuItem targetItem = new(target.Layer.Name)
                {
                    Tag = target.Layer
                };
                targetItem.Click += async (_, _) => await onTargetClick(target.Layer);
                menuItems.Add(targetItem);
            }
        }

        private IEnumerable<FeatureTransferTarget> GetDefaultTransferTargetLayers(
            TreeNode? sourceNode,
            CanvasLayer sourceLayer)
        {
            if (sourceNode == null)
                yield break;

            HashSet<int> yieldedLayerIds = [];
            foreach (FeatureTransferTarget target in GetDefaultFeatureTargetLayers())
            {
                if (target.Layer.Id != sourceLayer.Id &&
                    AreLayerTypesTransferCompatible(sourceLayer, target.Layer) &&
                    yieldedLayerIds.Add(target.Layer.Id))
                {
                    yield return target;
                }
            }

            string? sourceGroupKey = GetLayerGroupKeyForNode(sourceNode);
            bool sourceIsExternal =
                string.Equals(sourceGroupKey, ExternalLayerGroupKey, StringComparison.OrdinalIgnoreCase) &&
                CanvasLayerTreeService.IsExternalImportedLayer(sourceLayer);
            if (!sourceIsExternal)
                yield break;

            foreach (FeatureTransferTarget target in GetDrawingMarkupLayerTransferTargets())
            {
                if (target.Layer.Id != sourceLayer.Id &&
                    !target.Layer.IsLocked &&
                    AreLayerTypesTransferCompatible(sourceLayer, target.Layer) &&
                    yieldedLayerIds.Add(target.Layer.Id))
                {
                    yield return target;
                }
            }
        }

        private IEnumerable<FeatureTransferTarget> GetDefaultFeatureTargetLayers()
        {
            foreach (TreeNode root in treeViewLayers.Nodes.Cast<TreeNode>())
            {
                foreach (TreeNode layerNode in EnumerateLayerNodes(root))
                {
                    CanvasLayer? targetLayer = GetLayerFromNode(layerNode);
                    if (targetLayer == null ||
                        IsRasterLayer(targetLayer) ||
                        !IsTransferTargetLayer(layerNode, targetLayer))
                    {
                        continue;
                    }

                    yield return new FeatureTransferTarget(
                        targetLayer,
                        GetFeatureTargetGroupName(layerNode));
                }
            }
        }

        private IEnumerable<FeatureTransferTarget> GetDrawingMarkupLayerTransferTargets()
        {
            foreach (TreeNode root in treeViewLayers.Nodes.Cast<TreeNode>())
            {
                foreach (TreeNode groupNode in EnumerateGroupNodes(root, DrawingMarkupGroupKey))
                {
                    foreach (TreeNode layerNode in EnumerateLayerNodes(groupNode))
                    {
                        CanvasLayer? targetLayer = GetLayerFromNode(layerNode);
                        if (targetLayer == null ||
                            targetLayer.IsLocked ||
                            IsRasterLayer(targetLayer) ||
                            !CanvasLayerTreeService.IsDrawingMarkupLayer(targetLayer))
                        {
                            continue;
                        }

                        yield return new FeatureTransferTarget(
                            targetLayer,
                            GetFeatureTargetGroupName(layerNode));
                    }
                }
            }
        }

        private static string GetFeatureTargetGroupName(TreeNode layerNode)
        {
            for (TreeNode? current = layerNode.Parent; current != null; current = current.Parent)
            {
                if (current.Tag is not LayerTreeNodeState state ||
                    state.IsLayerNode ||
                    string.IsNullOrWhiteSpace(state.GroupKey) ||
                    string.Equals(state.GroupKey, RePlotRootNodeKey, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return current.Text;
            }

            return "Layers";
        }

        private static bool IsTransferTargetLayer(TreeNode layerNode, CanvasLayer targetLayer)
        {
            string? groupKey = GetLayerGroupKeyForNode(layerNode);
            return CanvasLayerTreeService.IsProtectedDefaultLayer(targetLayer) ||
                   IsCadastralCanvasLayer(targetLayer) ||
                   (CanvasLayerTreeService.IsRePlotDataGroupKey(groupKey) &&
                    !string.Equals(groupKey, DrawingMarkupGroupKey, StringComparison.OrdinalIgnoreCase));
        }

        private enum FeatureTransferGeometryKind
        {
            None,
            Point,
            LinearOrPolygon,
            Annotation
        }

        private static bool AreLayerTypesTransferCompatible(CanvasLayer sourceLayer, CanvasLayer targetLayer)
        {
            return AreTransferGeometryKindsCompatible(
                GetTransferGeometryKind(sourceLayer),
                GetTransferGeometryKind(targetLayer));
        }

        private static bool AreTransferGeometryKindsCompatible(
            FeatureTransferGeometryKind sourceKind,
            FeatureTransferGeometryKind targetKind)
        {
            return sourceKind != FeatureTransferGeometryKind.None &&
                   sourceKind == targetKind;
        }

        private static FeatureTransferGeometryKind GetTransferGeometryKind(CanvasLayer layer)
        {
            if (IsRasterLayer(layer))
                return FeatureTransferGeometryKind.None;

            if (CanvasLayerTreeService.IsAnnotationLayer(layer))
                return FeatureTransferGeometryKind.Annotation;

            if (CanvasLayerTreeService.IsPointLayer(layer))
                return FeatureTransferGeometryKind.Point;

            if (IsPolylineTransferTargetLayer(layer) ||
                IsPolygonTransferTargetLayer(layer))
            {
                return FeatureTransferGeometryKind.LinearOrPolygon;
            }

            return FeatureTransferGeometryKind.None;
        }

        private static FeatureTransferGeometryKind GetTransferGeometryKind(CanvasObject canvasObject)
        {
            if (string.Equals(canvasObject.ObjectType, "Text", StringComparison.OrdinalIgnoreCase))
                return FeatureTransferGeometryKind.Annotation;

            return canvasObject.Shape.OgcGeometryType switch
            {
                NetTopologySuite.Geometries.OgcGeometryType.Point or
                NetTopologySuite.Geometries.OgcGeometryType.MultiPoint => FeatureTransferGeometryKind.Point,
                NetTopologySuite.Geometries.OgcGeometryType.LineString or
                NetTopologySuite.Geometries.OgcGeometryType.MultiLineString or
                NetTopologySuite.Geometries.OgcGeometryType.Polygon or
                NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon => FeatureTransferGeometryKind.LinearOrPolygon,
                _ => FeatureTransferGeometryKind.None
            };
        }

        private async Task CreateFeaturesFromLayerAsync(
            CanvasLayer sourceLayer,
            CanvasLayer targetLayer)
        {
            if (!EnsureApplicationUnlockedForEditing("creating features from layers"))
                return;

            if (!AppServices.HasContext)
                return;

            try
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<CanvasObject> sourceObjects = await context.CanvasObjects
                    .Include(item => item.CanvasLayer)
                    .Where(item => item.CanvasLayerId == sourceLayer.Id)
                    .ToListAsync();

                if (sourceObjects.Count == 0)
                {
                    MessageBox.Show(
                        $"Layer '{sourceLayer.Name}' does not contain any drawing objects to convert.",
                        "Create Features",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                await CreateFeaturesFromCanvasObjectsAsync(
                    context,
                    sourceObjects,
                    targetLayer,
                    $"layer '{sourceLayer.Name}'");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create features from layer objects: {ex.Message}",
                    "Create Features",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task CreateFeaturesFromSelectedObjectsAsync(
            IReadOnlyList<Guid> selectedObjectIds,
            CanvasLayer targetLayer)
        {
            if (!EnsureApplicationUnlockedForEditing("creating features from objects"))
                return;

            if (!AppServices.HasContext || selectedObjectIds.Count == 0)
                return;

            try
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<Guid> ids = selectedObjectIds
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();
                List<CanvasObject> selectedObjects = await context.CanvasObjects
                    .Include(item => item.CanvasLayer)
                    .Where(item => ids.Contains(item.Id))
                    .ToListAsync();

                List<CanvasObject> sourceObjects = selectedObjects
                    .Where(item => item.CanvasLayer != null &&
                                   ((item.CanvasLayer.IsLocked != true &&
                                     CanvasLayerTreeService.IsDrawingMarkupLayer(item.CanvasLayer)) ||
                                    CanvasLayerTreeService.IsExternalImportedLayer(item.CanvasLayer)))
                    .ToList();

                if (sourceObjects.Count == 0)
                {
                    MessageBox.Show(
                        "The current selection does not contain drawing/markup or external reference objects.",
                        "Create Features",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                await CreateFeaturesFromCanvasObjectsAsync(
                    context,
                    sourceObjects,
                    targetLayer,
                    sourceObjects.Count == 1
                        ? "the selected source object"
                        : $"{sourceObjects.Count} selected source objects");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to create features from the selected objects: {ex.Message}",
                    "Create Features",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task CreateFeaturesFromCanvasObjectsAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> sourceObjects,
            CanvasLayer targetLayer,
            string sourceDescription)
        {
            List<CanvasObject> compatibleSourceObjects = sourceObjects
                .Where(sourceObject => IsCanvasObjectCompatibleWithFeatureTarget(sourceObject, targetLayer))
                .ToList();
            if (compatibleSourceObjects.Count == 0)
            {
                string message = CreateFeatureCompatibilityMessage(
                    sourceObjects,
                    compatibleSourceObjects,
                    targetLayer,
                    noCompatibleObjects: true);
                MessageBox.Show(
                    message,
                    "Create Features",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (compatibleSourceObjects.Count < sourceObjects.Count)
            {
                string message = CreateFeatureCompatibilityMessage(
                    sourceObjects,
                    compatibleSourceObjects,
                    targetLayer,
                    noCompatibleObjects: false);
                MessageBox.Show(
                    message,
                    "Create Features",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            DateTime now = DateTime.Now;
            List<(CanvasObject Source, CanvasObject Created)> transferPairs = compatibleSourceObjects
                .Select(sourceObject => (
                    Source: sourceObject,
                    Created: CreateTransferredCanvasObjectForLayer(
                        sourceObject,
                        targetLayer,
                        now)))
                .ToList();
            Dictionary<Guid, CanvasObject> sourceByCreatedObjectId = transferPairs
                .ToDictionary(pair => pair.Created.Id, pair => pair.Source);
            List<CanvasObject> createdObjects = transferPairs
                .Select(pair => pair.Created)
                .ToList();

            int duplicateSourceGeometryCount = RemoveDuplicateCreatedGeometries(createdObjects);
            int skippedExtraProjectBoundaryCount = 0;
            List<CanvasObject> projectBoundaryTargetSnapshots = [];
            if (CanvasLayerTreeService.IsProjectBoundaryLayer(targetLayer))
            {
                skippedExtraProjectBoundaryCount = KeepSingleProjectBoundaryCandidate(createdObjects);

                ProjectBoundaryFeatureResolution? projectBoundaryResolution =
                    await ResolveProjectBoundaryFeatureConflictAsync(context, createdObjects, targetLayer);
                if (projectBoundaryResolution == null)
                    return;

                if (projectBoundaryResolution.SkippedForExistingBoundary)
                {
                    SetCanvasCommandStatus(
                        "Skipped Project Boundary creation; the existing Project Boundary was kept.");
                    return;
                }

                createdObjects = projectBoundaryResolution.ObjectsToCreate;
                projectBoundaryTargetSnapshots = projectBoundaryResolution.ObjectsToReplace
                    .Select(CloneCanvasObjectSnapshot)
                    .ToList();
            }

            List<CanvasObject> duplicateTargetSnapshots = [];
            if (ShouldPreventDuplicateDefaultLayerGeometries(targetLayer) &&
                !CanvasLayerTreeService.IsProjectBoundaryLayer(targetLayer))
            {
                DuplicateFeatureGeometryResolution? duplicateResolution =
                    await ResolveDuplicateFeatureGeometriesAsync(context, createdObjects, targetLayer);
                if (duplicateResolution == null)
                    return;

                createdObjects = duplicateResolution.ObjectsToCreate;
                duplicateTargetSnapshots = duplicateResolution.ObjectsToReplace
                    .Select(CloneCanvasObjectSnapshot)
                    .ToList();
            }

            if (createdObjects.Count == 0)
            {
                MessageBox.Show(
                    duplicateSourceGeometryCount > 0
                        ? "No new feature geometry was created because every converted object duplicated another object."
                        : $"No new feature geometry was created because the matching geometry already exists in '{targetLayer.Name}'.",
                    "Create Features",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            bool? keepSourceObjects = PromptKeepDrawingObjectsAfterFeatureCreation(
                sourceDescription,
                targetLayer.Name,
                createdObjects.Count);
            if (!keepSourceObjects.HasValue)
                return;

            List<CanvasObject> sourceObjectsToRemove = keepSourceObjects.Value
                ? []
                : createdObjects
                    .Select(createdObject => sourceByCreatedObjectId[createdObject.Id])
                    .DistinctBy(sourceObject => sourceObject.Id)
                    .ToList();
            List<CanvasObject> deletedObjectSnapshots = keepSourceObjects.Value
                ? projectBoundaryTargetSnapshots
                    .Concat(duplicateTargetSnapshots)
                    .ToList()
                : sourceObjectsToRemove
                    .Select(CloneCanvasObjectSnapshot)
                    .Concat(projectBoundaryTargetSnapshots)
                    .Concat(duplicateTargetSnapshots)
                    .ToList();

            CanvasLayer trackedTargetLayer = EnsureTrackedCanvasLayer(context, targetLayer);
            foreach (CanvasObject createdObject in createdObjects)
            {
                createdObject.CanvasLayer = trackedTargetLayer;
            }

            await context.CanvasObjects.AddRangeAsync(createdObjects);
            List<CanvasObject> targetObjectsToReplace = projectBoundaryTargetSnapshots
                .Concat(duplicateTargetSnapshots)
                .ToList();
            if (targetObjectsToReplace.Count > 0)
            {
                Guid[] duplicateIds = targetObjectsToReplace.Select(item => item.Id).ToArray();
                List<CanvasObject> duplicateTargets = await context.CanvasObjects
                    .Where(item => duplicateIds.Contains(item.Id))
                    .ToListAsync();
                context.CanvasObjects.RemoveRange(duplicateTargets);
            }

            if (!keepSourceObjects.Value)
            {
                context.CanvasObjects.RemoveRange(sourceObjectsToRemove);
            }

            await context.SaveChangesAsync();
            if (ShouldRefreshGeneratedRoadParcel(createdObjects) ||
                ShouldRefreshGeneratedRoadParcel(deletedObjectSnapshots))
            {
                await RefreshGeneratedRoadParcelAsync(context);
            }

            MarkProjectModifiedIfOpen();
            if (!keepSourceObjects.Value)
            {
                mapCanvasControlMain.ClearSelectionAfterDelete();
            }

            await RefreshMapCanvasAsync("Creating features");
            await RefreshCurrentSelectedCanvasObjectPropertiesAsync();

            List<CanvasObject> createdSnapshots = createdObjects
                .Select(CloneCanvasObjectSnapshot)
                .ToList();
            IPersistedCanvasUndoCommand undoCommand = deletedObjectSnapshots.Count == 0
                ? new AddCanvasObjectsCommand(createdSnapshots)
                : new CreateFeaturesFromCanvasObjectsCommand(createdSnapshots, deletedObjectSnapshots);
            RegisterCanvasUndoCommand(undoCommand);

            string duplicateSuffix = string.Empty;
            if (duplicateTargetSnapshots.Count > 0)
            {
                duplicateSuffix = $"; replaced {duplicateTargetSnapshots.Count} duplicate feature(s)";
            }
            else if (projectBoundaryTargetSnapshots.Count > 0)
            {
                duplicateSuffix = $"; replaced {projectBoundaryTargetSnapshots.Count} existing Project Boundary object(s)";
            }
            else if (skippedExtraProjectBoundaryCount > 0)
            {
                duplicateSuffix = $"; skipped {skippedExtraProjectBoundaryCount} extra Project Boundary source object(s)";
            }
            else if (duplicateSourceGeometryCount > 0)
            {
                duplicateSuffix = $"; skipped {duplicateSourceGeometryCount} duplicate source geometry item(s)";
            }

            SetCanvasCommandStatus(keepSourceObjects.Value
                ? $"Created {createdObjects.Count} feature(s) in {targetLayer.Name}; kept drawing object(s){duplicateSuffix}"
                : $"Created {createdObjects.Count} feature(s) in {targetLayer.Name}; removed drawing object(s){duplicateSuffix}");
        }

        private static CanvasLayer EnsureTrackedCanvasLayer(
            AppDbContext context,
            CanvasLayer layer)
        {
            CanvasLayer? trackedLayer = context.CanvasLayers.Local
                .FirstOrDefault(item => item.Id == layer.Id);
            if (trackedLayer != null)
                return trackedLayer;

            context.Attach(layer);
            return layer;
        }

        private async Task<BlockRoadParcelRefreshResult> RefreshGeneratedRoadParcelAsync(
            AppDbContext context)
        {
            return await new BlockRoadParcelRefreshService().RefreshAsync(context);
        }

        private async Task GenerateRoadParcelFromLayerContextAsync()
        {
            if (!EnsureApplicationUnlockedForEditing("generating road parcel"))
                return;

            CanvasLayer? layer = GetLayerFromNode(_contextLayerNode);
            if (layer == null || !BlockRoadParcelRefreshService.IsRoadParcelLayer(layer))
                return;

            DialogResult choice = MessageBox.Show(
                this,
                "Generate Road Parcel from Project Boundary and layout area layers?",
                "Generate Road Parcel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (choice != DialogResult.Yes)
                return;

            try
            {
                SetOperationProgress(40, "Generating road parcel...", showProgressForm: false);
                BlockRoadParcelRefreshResult result =
                    await RefreshGeneratedRoadParcelAsync(AppServices.Context.Session.GetDbContext());

                if (!result.Created)
                {
                    MessageBox.Show(
                        this,
                        result.SkippedReason ?? "Road Parcel could not be generated.",
                        "Generate Road Parcel",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                MarkProjectModifiedIfOpen();
                await RefreshLayerTreeAsync();
                await RefreshVectorCanvasFeaturesAsync();
                SetCanvasCommandStatus(
                    $"Generated {result.CreatedObjects:N0} road parcel object(s).");
            }
            catch (Exception ex)
            {
                LogProjectError("Road parcel generation failed.", ex);
                MessageBox.Show(
                    this,
                    $"Failed to generate Road Parcel: {GetMostUsefulExceptionMessage(ex)}",
                    "Generate Road Parcel",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private static bool ShouldRefreshGeneratedRoadParcel(
            IEnumerable<CanvasObject> canvasObjects)
        {
            return canvasObjects.Any(BlockRoadParcelRefreshService.AffectsGeneratedRoadParcel);
        }

        private static bool IsGeneratedRoadParcelDependencyLayer(CanvasLayer layer)
        {
            return BlockRoadParcelRefreshService.IsRoadParcelDependencyLayer(layer);
        }

        private async Task<bool> CanvasObjectSnapshotsAffectGeneratedRoadParcelAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> snapshots)
        {
            if (snapshots.Count == 0)
                return false;

            int[] layerIds = snapshots
                .Select(item => item.CanvasLayerId)
                .Distinct()
                .ToArray();
            if (layerIds.Length == 0)
                return false;

            List<CanvasLayer> layers = await context.CanvasLayers
                .Where(item => layerIds.Contains(item.Id))
                .ToListAsync();
            return layers.Any(IsGeneratedRoadParcelDependencyLayer);
        }

        private async Task<bool> CanvasObjectIdsAffectGeneratedRoadParcelAsync(
            AppDbContext context,
            IReadOnlyList<Guid> canvasObjectIds)
        {
            if (canvasObjectIds.Count == 0)
                return false;

            List<CanvasObject> canvasObjects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(item => canvasObjectIds.Contains(item.Id))
                .ToListAsync();

            return canvasObjects.Any(BlockRoadParcelRefreshService.AffectsGeneratedRoadParcel);
        }

        private static int KeepSingleProjectBoundaryCandidate(List<CanvasObject> createdObjects)
        {
            if (createdObjects.Count <= 1)
                return 0;

            CanvasObject objectToKeep = createdObjects
                .OrderByDescending(item => item.Shape?.Area ?? 0.0)
                .First();
            int skippedCount = createdObjects.Count - 1;
            createdObjects.RemoveAll(item => item.Id != objectToKeep.Id);
            return skippedCount;
        }

        private async Task<ProjectBoundaryFeatureResolution?> ResolveProjectBoundaryFeatureConflictAsync(
            AppDbContext context,
            List<CanvasObject> createdObjects,
            CanvasLayer targetLayer)
        {
            if (createdObjects.Count == 0)
            {
                return new ProjectBoundaryFeatureResolution(createdObjects, [], false);
            }

            List<CanvasObject> existingBoundaryObjects = await context.CanvasObjects
                .Include(item => item.CanvasLayer)
                .Where(item =>
                    item.CanvasLayerId == targetLayer.Id ||
                    (item.CanvasLayer != null &&
                     (item.CanvasLayer.Name == "Project Boundary" ||
                      item.CanvasLayer.LayerType == "ProjectBoundary")))
                .ToListAsync();
            if (existingBoundaryObjects.Count == 0)
            {
                return new ProjectBoundaryFeatureResolution(createdObjects, [], false);
            }

            DuplicateFeatureGeometryChoice choice =
                PromptProjectBoundaryFeatureConflictChoice(existingBoundaryObjects.Count);
            return choice switch
            {
                DuplicateFeatureGeometryChoice.Replace => new ProjectBoundaryFeatureResolution(
                    createdObjects,
                    existingBoundaryObjects,
                    false),
                DuplicateFeatureGeometryChoice.Skip => new ProjectBoundaryFeatureResolution([], [], true),
                _ => null
            };
        }

        private static DuplicateFeatureGeometryChoice PromptProjectBoundaryFeatureConflictChoice(
            int existingBoundaryCount)
        {
            DialogResult result = MessageBox.Show(
                $"A Project Boundary already exists ({existingBoundaryCount:N0} object(s)).\n\n" +
                "Choose Yes to replace the existing Project Boundary.\n" +
                "Choose No to keep the existing Project Boundary and skip the incoming one.",
                "Replace Project Boundary",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            return result switch
            {
                DialogResult.Yes => DuplicateFeatureGeometryChoice.Replace,
                DialogResult.No => DuplicateFeatureGeometryChoice.Skip,
                _ => DuplicateFeatureGeometryChoice.Cancel
            };
        }

        private static int RemoveDuplicateCreatedGeometries(List<CanvasObject> createdObjects)
        {
            int removedCount = 0;
            for (int i = createdObjects.Count - 1; i >= 0; i--)
            {
                CanvasObject current = createdObjects[i];
                bool duplicate = createdObjects
                    .Take(i)
                    .Any(existing => AreGeometriesExactlyEqual(existing.Shape, current.Shape));
                if (!duplicate)
                    continue;

                createdObjects.RemoveAt(i);
                removedCount++;
            }

            return removedCount;
        }

        private async Task<DuplicateFeatureGeometryResolution?> ResolveDuplicateFeatureGeometriesAsync(
            AppDbContext context,
            List<CanvasObject> createdObjects,
            CanvasLayer targetLayer)
        {
            List<CanvasObject> existingTargetObjects = await context.CanvasObjects
                .Where(item => item.CanvasLayerId == targetLayer.Id)
                .ToListAsync();
            if (existingTargetObjects.Count == 0)
                return new DuplicateFeatureGeometryResolution(createdObjects, []);

            List<CanvasObject> objectsToCreate = [];
            List<CanvasObject> objectsWithExistingDuplicates = [];
            Dictionary<Guid, CanvasObject> duplicateTargetsById = [];

            foreach (CanvasObject createdObject in createdObjects)
            {
                List<CanvasObject> matches = existingTargetObjects
                    .Where(existing => AreGeometriesExactlyEqual(existing.Shape, createdObject.Shape))
                    .ToList();
                if (matches.Count == 0)
                {
                    objectsToCreate.Add(createdObject);
                    continue;
                }

                objectsWithExistingDuplicates.Add(createdObject);
                foreach (CanvasObject match in matches)
                    duplicateTargetsById.TryAdd(match.Id, match);
            }

            if (objectsWithExistingDuplicates.Count == 0)
                return new DuplicateFeatureGeometryResolution(objectsToCreate, []);

            DuplicateFeatureGeometryChoice choice = PromptDuplicateFeatureGeometryChoice(
                targetLayer.Name,
                objectsWithExistingDuplicates.Count,
                duplicateTargetsById.Count);
            return choice switch
            {
                DuplicateFeatureGeometryChoice.Replace => new DuplicateFeatureGeometryResolution(
                    objectsToCreate.Concat(objectsWithExistingDuplicates).ToList(),
                    duplicateTargetsById.Values.ToList()),
                DuplicateFeatureGeometryChoice.Skip => new DuplicateFeatureGeometryResolution(objectsToCreate, []),
                _ => null
            };
        }

        private static DuplicateFeatureGeometryChoice PromptDuplicateFeatureGeometryChoice(
            string targetLayerName,
            int incomingDuplicateCount,
            int existingDuplicateCount)
        {
            DialogResult result = MessageBox.Show(
                $"{incomingDuplicateCount:N0} incoming feature geometry item(s) already exist in '{targetLayerName}'.\n\n" +
                $"Replace the existing {existingDuplicateCount:N0} duplicate object(s)?\n\n" +
                "Choose Yes to replace existing geometry.\n" +
                "Choose No to skip duplicate incoming geometry.",
                "Duplicate Geometry",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            return result switch
            {
                DialogResult.Yes => DuplicateFeatureGeometryChoice.Replace,
                DialogResult.No => DuplicateFeatureGeometryChoice.Skip,
                _ => DuplicateFeatureGeometryChoice.Cancel
            };
        }

        private static bool ShouldPreventDuplicateDefaultLayerGeometries(CanvasLayer targetLayer)
        {
            return CanvasLayerTreeService.IsProtectedDefaultLayer(targetLayer) ||
                   CanvasLayerTreeService.IsRePlotDataLayer(targetLayer) ||
                   IsCadastralCanvasLayer(targetLayer);
        }

        private static bool AreGeometriesExactlyEqual(
            NetTopologySuite.Geometries.Geometry? left,
            NetTopologySuite.Geometries.Geometry? right)
        {
            if (left == null || right == null)
                return false;

            if (left.EqualsExact(right, 0.0))
                return true;

            NetTopologySuite.Geometries.Geometry leftCopy = left.Copy();
            NetTopologySuite.Geometries.Geometry rightCopy = right.Copy();
            leftCopy.Normalize();
            rightCopy.Normalize();
            return leftCopy.EqualsExact(rightCopy, 0.0);
        }

        private static bool? PromptKeepDrawingObjectsAfterFeatureCreation(
            string sourceDescription,
            string targetLayerName,
            int objectCount)
        {
            DialogResult result = MessageBox.Show(
                $"Create {objectCount:N0} feature(s) in '{targetLayerName}' from {sourceDescription}?\n\n" +
                "Choose Yes to keep the original drawing/markup object(s).\n" +
                "Choose No to delete them after the features are created.",
                "Create Features",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1);

            return result switch
            {
                DialogResult.Yes => true,
                DialogResult.No => false,
                _ => null
            };
        }

        private static bool IsCanvasObjectCompatibleWithFeatureTarget(
            CanvasObject sourceObject,
            CanvasLayer targetLayer)
        {
            NetTopologySuite.Geometries.OgcGeometryType geometryType =
                sourceObject.Shape.OgcGeometryType;

            if (IsPolygonTransferTargetLayer(targetLayer))
            {
                return geometryType is NetTopologySuite.Geometries.OgcGeometryType.Polygon
                    or NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon ||
                    IsClosedPolylineCanvasObject(sourceObject);
            }

            if (IsPolylineTransferTargetLayer(targetLayer))
            {
                return geometryType is NetTopologySuite.Geometries.OgcGeometryType.LineString
                    or NetTopologySuite.Geometries.OgcGeometryType.MultiLineString
                    or NetTopologySuite.Geometries.OgcGeometryType.Polygon
                    or NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon;
            }

            if (CanvasLayerTreeService.IsPointLayer(targetLayer))
            {
                return geometryType is NetTopologySuite.Geometries.OgcGeometryType.Point
                    or NetTopologySuite.Geometries.OgcGeometryType.MultiPoint;
            }

            if (CanvasLayerTreeService.IsAnnotationLayer(targetLayer))
            {
                return string.Equals(sourceObject.ObjectType, "Text", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static string CreateFeatureCompatibilityMessage(
            IReadOnlyList<CanvasObject> sourceObjects,
            IReadOnlyList<CanvasObject> compatibleSourceObjects,
            CanvasLayer targetLayer,
            bool noCompatibleObjects)
        {
            int skippedCount = sourceObjects.Count - compatibleSourceObjects.Count;
            if (IsPolygonTransferTargetLayer(targetLayer))
            {
                int openPolylineCount = sourceObjects.Count(sourceObject =>
                    IsPolylineLikeTransferSource(sourceObject) &&
                    !IsClosedPolylineCanvasObject(sourceObject));

                if (openPolylineCount > 0)
                {
                    return noCompatibleObjects
                        ? $"No closed polyline or polygon geometry was found for '{targetLayer.Name}'. Open polyline object(s) cannot be converted to polygon features."
                        : $"{skippedCount:N0} drawing object(s) will be skipped. Only closed polylines are converted to polygon features for '{targetLayer.Name}'.";
                }
            }

            return noCompatibleObjects
                ? $"No compatible drawing object geometry was found for '{targetLayer.Name}'."
                : $"{skippedCount:N0} drawing object(s) do not match '{targetLayer.Name}' and will be skipped.";
        }

        private static bool IsPolylineLikeTransferSource(CanvasObject sourceObject)
        {
            return string.Equals(sourceObject.ObjectType, "Polyline", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(sourceObject.ObjectType, "Line", StringComparison.OrdinalIgnoreCase) ||
                   sourceObject.Shape.OgcGeometryType is NetTopologySuite.Geometries.OgcGeometryType.LineString
                       or NetTopologySuite.Geometries.OgcGeometryType.MultiLineString;
        }

        private static bool IsClosedPolylineCanvasObject(CanvasObject sourceObject)
        {
            if (!IsPolylineLikeTransferSource(sourceObject))
                return false;

            return sourceObject.Shape switch
            {
                NetTopologySuite.Geometries.LineString lineString => CanConvertLineStringToPolygon(sourceObject, lineString),
                NetTopologySuite.Geometries.MultiLineString multiLineString => multiLineString.Geometries
                    .OfType<NetTopologySuite.Geometries.LineString>()
                    .Any(IsClosedLineString),
                _ => false
            };
        }

        private static bool TryReadPolylineMetadataClosed(string? metadataJson, out bool isClosed)
        {
            isClosed = false;
            if (string.IsNullOrWhiteSpace(metadataJson))
                return false;

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadataJson);
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return false;
                }

                // CAD/import metadata is produced by several code paths and uses inconsistent
                // casing (e.g. camelCase "shapeType"/"isClosed" vs PascalCase). Read both.
                string? shapeType = TryGetJsonPropertyIgnoreCase(root, "ShapeType", out JsonElement shapeTypeElement)
                    ? shapeTypeElement.GetString()
                    : null;
                if (!string.Equals(shapeType, "Polyline", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(shapeType, "Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (string.Equals(shapeType, "Polygon", StringComparison.OrdinalIgnoreCase))
                {
                    isClosed = true;
                    return true;
                }

                if (TryGetJsonPropertyIgnoreCase(root, "IsClosed", out JsonElement isClosedElement) &&
                    (isClosedElement.ValueKind == JsonValueKind.True ||
                     isClosedElement.ValueKind == JsonValueKind.False))
                {
                    isClosed = isClosedElement.GetBoolean();
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool IsClosedLineString(NetTopologySuite.Geometries.LineString lineString)
        {
            return lineString.NumPoints >= 4 &&
                   lineString.StartPoint != null &&
                   lineString.EndPoint != null &&
                   lineString.StartPoint.Coordinate.Distance(lineString.EndPoint.Coordinate) <= ClosedPolylineTransferTolerance;
        }

        private static NetTopologySuite.Geometries.Geometry CreateTransferredTargetGeometry(
            CanvasObject source,
            CanvasLayer targetLayer)
        {
            if (IsPolygonTransferTargetLayer(targetLayer) &&
                TryCreatePolygonGeometryFromClosedPolyline(source, out NetTopologySuite.Geometries.Geometry? polygonGeometry))
            {
                return polygonGeometry!;
            }

            if (IsPolylineTransferTargetLayer(targetLayer) &&
                TryCreatePolylineGeometryFromPolygon(source, out NetTopologySuite.Geometries.Geometry? polylineGeometry))
            {
                return polylineGeometry!;
            }

            return source.Shape.Copy();
        }

        private static bool TryCreatePolylineGeometryFromPolygon(
            CanvasObject source,
            out NetTopologySuite.Geometries.Geometry? polylineGeometry)
        {
            polylineGeometry = null;
            if (source.Shape.OgcGeometryType is not NetTopologySuite.Geometries.OgcGeometryType.Polygon &&
                source.Shape.OgcGeometryType is not NetTopologySuite.Geometries.OgcGeometryType.MultiPolygon)
            {
                return false;
            }

            NetTopologySuite.Geometries.Geometry boundary = source.Shape.Boundary;
            if (boundary.IsEmpty)
                return false;

            polylineGeometry = boundary.Copy();
            return true;
        }

        private static bool TryCreatePolygonGeometryFromClosedPolyline(
            CanvasObject source,
            out NetTopologySuite.Geometries.Geometry? polygonGeometry)
        {
            polygonGeometry = null;
            switch (source.Shape)
            {
                case NetTopologySuite.Geometries.LineString lineString
                    when CanConvertLineStringToPolygon(source, lineString):
                    polygonGeometry = lineString.Factory.CreatePolygon(
                        CreateClosedRingCoordinates(lineString));
                    return true;

                case NetTopologySuite.Geometries.MultiLineString multiLineString:
                    NetTopologySuite.Geometries.Polygon[] polygons = multiLineString.Geometries
                        .OfType<NetTopologySuite.Geometries.LineString>()
                        .Where(IsClosedLineString)
                        .Select(line => line.Factory.CreatePolygon(
                            CreateClosedRingCoordinates(line)))
                        .ToArray();
                    if (polygons.Length == 0)
                        return false;

                    polygonGeometry = multiLineString.Factory.CreateMultiPolygon(polygons);
                    return true;

                default:
                    return false;
            }
        }

        private static bool CanConvertLineStringToPolygon(
            CanvasObject source,
            NetTopologySuite.Geometries.LineString lineString)
        {
            if (IsClosedLineString(lineString))
                return true;

            return lineString.NumPoints >= 3 &&
                   TryReadPolylineMetadataClosed(source.GeometryMetadataJson, out bool metadataClosed) &&
                   metadataClosed;
        }

        private static NetTopologySuite.Geometries.Coordinate[] CreateClosedRingCoordinates(
            NetTopologySuite.Geometries.LineString lineString)
        {
            List<NetTopologySuite.Geometries.Coordinate> coordinates = lineString.Coordinates
                .Select(coordinate => new NetTopologySuite.Geometries.Coordinate(coordinate.X, coordinate.Y))
                .ToList();

            if (coordinates.Count == 0)
                return [];

            NetTopologySuite.Geometries.Coordinate first = coordinates[0];
            NetTopologySuite.Geometries.Coordinate last = coordinates[^1];
            if (first.Distance(last) <= ClosedPolylineTransferTolerance)
            {
                coordinates[^1] = new NetTopologySuite.Geometries.Coordinate(first.X, first.Y);
            }
            else
            {
                coordinates.Add(new NetTopologySuite.Geometries.Coordinate(first.X, first.Y));
            }

            return coordinates.ToArray();
        }

        private static CanvasObject CreateTransferredCanvasObjectForLayer(
            CanvasObject source,
            CanvasLayer targetLayer,
            DateTime timestamp)
        {
            NetTopologySuite.Geometries.Geometry targetGeometry = CreateTransferredTargetGeometry(
                source,
                targetLayer);
            string targetObjectType = ResolveTransferredObjectType(source, targetLayer);
            CanvasObject canvasObject = new()
            {
                Id = Guid.NewGuid(),
                CanvasLayerId = targetLayer.Id,
                CanvasLayer = targetLayer,
                ObjectType = targetObjectType,
                Shape = targetGeometry,
                GeometryMetadataJson = CreateTransferredGeometryMetadataJson(
                    source,
                    targetLayer,
                    targetGeometry,
                    timestamp),
                BorderColorOverride = null,
                FillColorOverride = null,
                FillTransparencyOverride = null,
                LineWeightOverride = null,
                LineStyleOverride = null,
                LabelText = null,
                ObjectDescription = CreateTransferredObjectDescription(source, targetLayer),
                IsVisible = source.IsVisible,
                IsLocked = false,
                BaselineParcelId = null,
                ReplottedParcelId = null,
                RoadId = null,
                BlockId = null,
                SourceDxfHandle = null,
                CreatedDate = timestamp,
                LastModifiedDate = timestamp
            };
            if (CanvasLayerTreeService.IsBlockLayoutLayer(targetLayer))
            {
                CanvasGeometryMetricsService.StoreBlockDepthFromGeometry(canvasObject);
            }

            return canvasObject;
        }

        private static string ResolveTransferredObjectType(CanvasObject source, CanvasLayer targetLayer)
        {
            if (IsPolygonTransferTargetLayer(targetLayer))
            {
                return "Polygon";
            }

            if (IsPolylineTransferTargetLayer(targetLayer))
            {
                return "Polyline";
            }

            if (CanvasLayerTreeService.IsPointLayer(targetLayer))
            {
                return "Point";
            }

            if (CanvasLayerTreeService.IsAnnotationLayer(targetLayer))
            {
                return "Text";
            }

            return string.IsNullOrWhiteSpace(source.ObjectType)
                ? source.Shape.OgcGeometryType.ToString()
                : source.ObjectType;
        }

        private static bool IsPolygonTransferTargetLayer(CanvasLayer layer)
        {
            if (string.Equals(layer.LayerType, "ProjectBoundary", StringComparison.OrdinalIgnoreCase) ||
                IsCadastralCanvasLayer(layer) ||
                CanvasLayerTreeService.IsPolygonLayer(layer))
            {
                return true;
            }

            return CanvasLayerTreeService.IsBlockLayoutLayer(layer) ||
                   string.Equals(layer.LayerType, "BuildingFootprint", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "RoadParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "ReplottedParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "PrivateReplotParcel", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "PublicFacility", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "OpenSpace", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, "ServiceSalesPlot", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsPolylineTransferTargetLayer(CanvasLayer layer)
        {
            return string.Equals(layer.LayerType, CanvasLayerTreeService.LineLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, CanvasLayerTreeService.PolylineLayerType, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(layer.LayerType, CanvasLayerTreeService.RoadCenterlineLayerType, StringComparison.OrdinalIgnoreCase);
        }

        private static string? CreateTransferredGeometryMetadataJson(
            CanvasObject source,
            CanvasLayer targetLayer,
            NetTopologySuite.Geometries.Geometry targetGeometry,
            DateTime timestamp)
        {
            if (!IsCadastralCanvasLayer(targetLayer))
            {
                return ExtractTransferableCurveMetadataJson(source.GeometryMetadataJson);
            }

            CadastralCanvasMetadata metadata = new()
            {
                SourceFormat = "CanvasTransfer",
                SourceFileName = string.Empty,
                SourceLayer = targetLayer.Name,
                CalculatedAreaSqm = targetGeometry.Area,
                SourceHandle = source.SourceDxfHandle,
                AssignmentStatus = "Unassigned",
                ImportedAt = timestamp
            };

            return JsonSerializer.Serialize(metadata, CadastralMetadataJsonOptions);
        }

        private static string? ExtractTransferableCurveMetadataJson(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
                return null;

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadataJson);
                JsonElement root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                    return null;

                if (TryGetJsonPropertyIgnoreCase(root, "curveMetadataJson", out JsonElement curveJsonProperty) &&
                    curveJsonProperty.ValueKind == JsonValueKind.String)
                {
                    string? curveJson = curveJsonProperty.GetString();
                    return IsCurveMetadataJson(curveJson) ? curveJson : null;
                }

                if (TryGetJsonPropertyIgnoreCase(root, "curve", out JsonElement curveObject) &&
                    curveObject.ValueKind == JsonValueKind.Object)
                {
                    string curveJson = curveObject.GetRawText();
                    return IsCurveMetadataJson(curveJson) ? curveJson : null;
                }

                return IsCurveMetadataRoot(root) ? metadataJson : null;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsCurveMetadataJson(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
                return false;

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadataJson);
                return document.RootElement.ValueKind == JsonValueKind.Object &&
                       IsCurveMetadataRoot(document.RootElement);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsCurveMetadataRoot(JsonElement root)
        {
            if (!TryGetJsonPropertyIgnoreCase(root, "shapeType", out JsonElement shapeTypeElement) ||
                shapeTypeElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            string? shapeType = shapeTypeElement.GetString();
            if (string.IsNullOrWhiteSpace(shapeType))
                return false;

            if (shapeType.Equals("Arc", StringComparison.OrdinalIgnoreCase) ||
                shapeType.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return (shapeType.Equals("Polyline", StringComparison.OrdinalIgnoreCase) ||
                    shapeType.Equals("Polygon", StringComparison.OrdinalIgnoreCase)) &&
                   TryGetJsonPropertyIgnoreCase(root, "segments", out JsonElement segmentsElement) &&
                   segmentsElement.ValueKind == JsonValueKind.Array &&
                   segmentsElement.GetArrayLength() > 0;
        }

        private static bool TryGetJsonPropertyIgnoreCase(
            JsonElement element,
            string propertyName,
            out JsonElement value)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        private static string CreateTransferredObjectDescription(
            CanvasObject source,
            CanvasLayer targetLayer)
        {
            if (string.Equals(targetLayer.LayerType, "ProjectBoundary", StringComparison.OrdinalIgnoreCase))
            {
                return "Project Boundary";
            }

            if (IsCadastralCanvasLayer(targetLayer))
            {
                return $"Cadastral parcel created from {source.ObjectDescription ?? source.ObjectType}";
            }

            return $"{targetLayer.Name} object";
        }

        private void BeginLayerRename(TreeNode? node)
        {
            if (!EnsureApplicationUnlockedForEditing("renaming layers"))
                return;

            if (!IsLayerNode(node))
            {
                return;
            }

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            if (layer.IsLocked)
            {
                ShowLayerLockedMessage(layer, "renamed");
                return;
            }

            if (!CanEditLayerName(node, layer))
            {
                ShowLayerNameProtectedMessage();
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

        private static bool CanBeginLayerRename(TreeNode? node, CanvasLayer? layer)
        {
            return layer != null &&
                   !layer.IsLocked &&
                   CanEditLayerName(node, layer);
        }

        private static bool CanEditLayerName(TreeNode? node, CanvasLayer? layer)
        {
            if (layer == null)
                return false;

            if (CanvasLayerTreeService.IsDrawingMarkupLayer(layer))
                return true;

            return !IsProtectedRePlotDefaultLayerNameNode(node, layer);
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
                CanvasLayer beforeSnapshot = CloneCanvasLayerSnapshot(nodeState.Layer);
                CanvasLayer? updatedLayer =
                    await _layerCommandService.SetVisibilityAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        nodeState.Layer,
                        !nodeState.Layer.IsVisible);

                if (updatedLayer == null)
                {
                    return;
                }

                RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                    [beforeSnapshot],
                    [CloneCanvasLayerSnapshot(updatedLayer)]));
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

        private async Task ToggleLayerLabelsAsync(TreeNode? node)
        {
            if (!EnsureApplicationUnlockedForEditing("changing layer labels"))
                return;

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null || !CanLayerDisplayLabels(layer))
            {
                return;
            }

            try
            {
                CanvasLayer beforeSnapshot = CloneCanvasLayerSnapshot(layer);
                CanvasLayer editableLayer = _layerCommandService.CreateEditableCopy(layer);
                editableLayer.ShowLabels = !layer.ShowLabels;
                if (editableLayer.LabelFontSize <= 0)
                    editableLayer.LabelFontSize = editableLayer.LabelScaleWithZoom ? 1.0 : 6.0;

                CanvasLayer updatedLayer =
                    await _layerCommandService.UpdatePropertiesAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        editableLayer);

                RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                    [beforeSnapshot],
                    [CloneCanvasLayerSnapshot(updatedLayer)]));
                UpdateLayerNode(node!, updatedLayer, updateRasterStack: false);
                mapCanvasControlMain.UpdateVectorLayer(updatedLayer);
                MarkProjectModifiedIfOpen();
                SetCanvasCommandStatus(updatedLayer.ShowLabels
                    ? $"Labels shown: {updatedLayer.Name}"
                    : $"Labels hidden: {updatedLayer.Name}");
                SelectLayerNodeById(updatedLayer.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update layer labels: {ex.Message}",
                    "Layer Labels",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task ToggleLayerFillTransparencyAsync(TreeNode? node)
        {
            if (!EnsureApplicationUnlockedForEditing("changing layer transparency"))
                return;

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null || !CanLayerUseFillTransparency(layer))
            {
                return;
            }

            try
            {
                CanvasLayer beforeSnapshot = CloneCanvasLayerSnapshot(layer);
                CanvasLayer editableLayer = _layerCommandService.CreateEditableCopy(layer);
                editableLayer.ShowFillTransparency = !layer.ShowFillTransparency;
                editableLayer.FillTransparency = Math.Clamp(
                    editableLayer.FillTransparency <= 0 ? 50 : editableLayer.FillTransparency,
                    0,
                    100);

                CanvasLayer updatedLayer =
                    await _layerCommandService.UpdatePropertiesAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        editableLayer);

                RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                    [beforeSnapshot],
                    [CloneCanvasLayerSnapshot(updatedLayer)]));
                UpdateLayerNode(node!, updatedLayer, updateRasterStack: false);
                mapCanvasControlMain.UpdateVectorLayer(updatedLayer);
                MarkProjectModifiedIfOpen();
                SetCanvasCommandStatus(updatedLayer.ShowFillTransparency
                    ? $"Fill transparency shown: {updatedLayer.Name}"
                    : $"Fill transparency hidden: {updatedLayer.Name}");
                SelectLayerNodeById(updatedLayer.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update fill transparency: {ex.Message}",
                    "Fill Transparency",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool CanLayerDisplayLabels(CanvasLayer layer)
        {
            return !IsRasterLayer(layer) &&
                   !CanvasLayerTreeService.IsAnnotationLayer(layer) &&
                   !CanvasLayerTreeService.IsExternalImportedLayer(layer);
        }

        private static bool CanLayerUseFillTransparency(CanvasLayer layer)
        {
            return !IsRasterLayer(layer) &&
                   !CanvasLayerTreeService.IsLineLayer(layer) &&
                   !CanvasLayerTreeService.IsPointLayer(layer) &&
                   !CanvasLayerTreeService.IsAnnotationLayer(layer) &&
                   !string.Equals(layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(layer.FillColor);
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
                List<CanvasLayer> beforeSnapshots = new();
                List<CanvasLayer> afterSnapshots = new();
                List<(TreeNode Node, CanvasLayer Layer)> updatedNodeLayers = new();

                foreach (TreeNode layerNode in layerNodes)
                {
                    CanvasLayer? layer = GetLayerFromNode(layerNode);
                    if (layer == null || layer.IsVisible == newVisibility)
                        continue;

                    beforeSnapshots.Add(CloneCanvasLayerSnapshot(layer));
                    CanvasLayer? updatedLayer =
                        await _layerCommandService.SetVisibilityAsync(
                            AppServices.HasContext ? AppServices.Context.Session : null,
                            layer,
                            newVisibility);

                    if (updatedLayer == null)
                        continue;

                    afterSnapshots.Add(CloneCanvasLayerSnapshot(updatedLayer));
                    updatedNodeLayers.Add((layerNode, updatedLayer));
                    if (IsRasterLayer(updatedLayer))
                    {
                        rasterStackDirty = true;
                    }
                    else
                    {
                        vectorStackDirty = true;
                    }
                }

                if (beforeSnapshots.Count > 0 && beforeSnapshots.Count == afterSnapshots.Count)
                    RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(beforeSnapshots, afterSnapshots));

                using (SuspendRedraw(leftSplitContainer.Panel1, grpLayer, treeViewLayers))
                {
                    treeViewLayers.BeginUpdate();
                    try
                    {
                        foreach ((TreeNode layerNode, CanvasLayer updatedLayer) in updatedNodeLayers)
                        {
                            UpdateLayerNode(
                                layerNode,
                                updatedLayer,
                                updateRasterStack: false,
                                invalidateNode: false);
                        }

                        nodeState.IsGroupCheckedWhenEmpty = newVisibility;
                    }
                    finally
                    {
                        treeViewLayers.EndUpdate();
                    }
                }

                MarkProjectModifiedIfOpen();

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
            if (!EnsureApplicationUnlockedForEditing("renaming layers"))
                return;

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

            if (!CanEditLayerName(node, nodeState.Layer))
            {
                ShowLayerNameProtectedMessage();
                return;
            }

            try
            {
                CanvasLayer beforeSnapshot = CloneCanvasLayerSnapshot(nodeState.Layer);
                CanvasLayer? renamedLayer =
                    await _layerCommandService.RenameAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        nodeState.Layer,
                        newName);

                if (renamedLayer == null)
                {
                    return;
                }

                RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                    [beforeSnapshot],
                    [CloneCanvasLayerSnapshot(renamedLayer)]));
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
            if (!EnsureApplicationUnlockedForEditing("changing layer locks"))
                return;

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer == null)
            {
                return;
            }

            try
            {
                CanvasLayer beforeSnapshot = CloneCanvasLayerSnapshot(layer);
                CanvasLayer? updatedLayer =
                    await _layerCommandService.ToggleLockAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        layer);

                if (updatedLayer == null)
                {
                    return;
                }

                RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                    [beforeSnapshot],
                    [CloneCanvasLayerSnapshot(updatedLayer)]));
                UpdateLayerNode(node!, updatedLayer, updateRasterStack: false);
                if (IsRasterLayer(updatedLayer))
                    UpdateRasterCanvasLayersFromTree();
                else
                    mapCanvasControlMain.UpdateVectorLayer(updatedLayer);

                RefreshCurrentDrawingLayerCombo();
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

        private async Task ToggleLayerGroupLockAsync(TreeNode? groupNode)
        {
            if (!EnsureApplicationUnlockedForEditing("changing layer group locks"))
                return;

            if (!IsRePlotDataLayerGroupNode(groupNode))
                return;

            List<TreeNode> layerNodes = EnumerateLayerNodes(groupNode!).ToList();
            if (layerNodes.Count == 0)
                return;

            bool lockLayers = layerNodes
                .Select(GetLayerFromNode)
                .Where(layer => layer != null)
                .Any(layer => layer!.IsLocked == false);

            try
            {
                List<CanvasLayer> beforeSnapshots = [];
                List<CanvasLayer> updatedLayers = [];
                List<(TreeNode Node, CanvasLayer Layer)> updatedNodeLayers = [];
                bool rasterStackDirty = false;
                bool vectorStackDirty = false;

                foreach (TreeNode layerNode in layerNodes)
                {
                    CanvasLayer? layer = GetLayerFromNode(layerNode);
                    if (layer == null || layer.IsLocked == lockLayers)
                        continue;

                    beforeSnapshots.Add(CloneCanvasLayerSnapshot(layer));
                    CanvasLayer editableLayer = _layerCommandService.CreateEditableCopy(layer);
                    editableLayer.IsLocked = lockLayers;
                    CanvasLayer updatedLayer =
                        await _layerCommandService.UpdatePropertiesAsync(
                            AppServices.HasContext ? AppServices.Context.Session : null,
                            editableLayer);
                    updatedLayers.Add(updatedLayer);
                    updatedNodeLayers.Add((layerNode, updatedLayer));

                    if (IsRasterLayer(updatedLayer))
                        rasterStackDirty = true;
                    else
                        vectorStackDirty = true;
                }

                using (SuspendRedraw(leftSplitContainer.Panel1, grpLayer, treeViewLayers))
                {
                    treeViewLayers.BeginUpdate();
                    try
                    {
                        foreach ((TreeNode layerNode, CanvasLayer updatedLayer) in updatedNodeLayers)
                        {
                            UpdateLayerNode(
                                layerNode,
                                updatedLayer,
                                updateRasterStack: false,
                                invalidateNode: false);
                        }
                    }
                    finally
                    {
                        treeViewLayers.EndUpdate();
                    }
                }

                if (beforeSnapshots.Count > 0 && beforeSnapshots.Count == updatedLayers.Count)
                {
                    RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                        beforeSnapshots,
                        updatedLayers.Select(CloneCanvasLayerSnapshot).ToList()));
                }

                MarkProjectModifiedIfOpen();
                if (rasterStackDirty)
                    UpdateRasterCanvasLayersFromTree();

                if (vectorStackDirty)
                    UpdateVectorCanvasLayersFromTree();

                SetCanvasCommandStatus(lockLayers
                    ? $"Layer group locked: {groupNode!.Text}"
                    : $"Layer group unlocked: {groupNode!.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update layer group lock state: {ex.Message}",
                    "Layer Lock",
                    MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
        }

        private async Task ToggleLayerGroupSelectionAsync(TreeNode? groupNode)
        {
            if (!EnsureApplicationUnlockedForEditing("changing layer selection"))
                return;

            if (groupNode == null)
                return;

            List<TreeNode> layerNodes = EnumerateLayerNodes(groupNode)
                .Where(node => GetLayerFromNode(node) is CanvasLayer layer && !IsRasterLayer(layer))
                .ToList();
            if (layerNodes.Count == 0)
                return;

            bool allowSelection = layerNodes
                .Select(GetLayerFromNode)
                .Where(layer => layer != null)
                .Any(layer => layer!.IsSelectable == false);

            try
            {
                List<CanvasLayer> beforeSnapshots = [];
                List<CanvasLayer> updatedLayers = [];
                List<(TreeNode Node, CanvasLayer Layer)> updatedNodeLayers = [];

                foreach (TreeNode layerNode in layerNodes)
                {
                    CanvasLayer? layer = GetLayerFromNode(layerNode);
                    if (layer == null || layer.IsSelectable == allowSelection)
                        continue;

                    beforeSnapshots.Add(CloneCanvasLayerSnapshot(layer));
                    CanvasLayer editableLayer = _layerCommandService.CreateEditableCopy(layer);
                    editableLayer.IsSelectable = allowSelection;
                    CanvasLayer updatedLayer =
                        await _layerCommandService.UpdatePropertiesAsync(
                            AppServices.HasContext ? AppServices.Context.Session : null,
                            editableLayer);
                    updatedLayers.Add(updatedLayer);
                    updatedNodeLayers.Add((layerNode, updatedLayer));
                }

                using (SuspendRedraw(leftSplitContainer.Panel1, grpLayer, treeViewLayers))
                {
                    treeViewLayers.BeginUpdate();
                    try
                    {
                        foreach ((TreeNode layerNode, CanvasLayer updatedLayer) in updatedNodeLayers)
                        {
                            UpdateLayerNode(
                                layerNode,
                                updatedLayer,
                                updateRasterStack: false,
                                invalidateNode: false);
                        }
                    }
                    finally
                    {
                        treeViewLayers.EndUpdate();
                    }
                }

                if (beforeSnapshots.Count > 0 && beforeSnapshots.Count == updatedLayers.Count)
                {
                    RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                        beforeSnapshots,
                        updatedLayers.Select(CloneCanvasLayerSnapshot).ToList()));
                }

                MarkProjectModifiedIfOpen();
                UpdateVectorCanvasLayersFromTree();
                SetCanvasCommandStatus(allowSelection
                    ? $"Layer group selection enabled: {groupNode!.Text}"
                    : $"Layer group selection disabled: {groupNode!.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to update layer group selection state: {ex.Message}",
                    "Layer Selection",
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

        private async Task MoveLayerInDisplayOrderAsync(TreeNode? node, int visualDirection)
        {
            if (!EnsureApplicationUnlockedForEditing("reordering layers"))
                return;

            CanvasLayer? layer = GetLayerFromNode(node);
            if (layer != null && CanvasLayerTreeService.IsDrawingMarkupLayer(layer))
                await MoveDrawingLayerInDisplayOrderAsync(node, visualDirection);
            else
                await MoveRasterLayerInDisplayOrderAsync(node, visualDirection);
        }

        private bool CanMoveDrawingLayerInDisplayOrder(TreeNode? node, int visualDirection)
        {
            if (node?.Parent == null ||
                !CanvasLayerTreeService.IsDrawingMarkupLayer(GetLayerFromNode(node)))
            {
                return false;
            }

            int targetIndex = node.Index + Math.Sign(visualDirection);
            if (targetIndex < 0 || targetIndex >= node.Parent.Nodes.Count)
                return false;

            return CanvasLayerTreeService.IsDrawingMarkupLayer(
                GetLayerFromNode(node.Parent.Nodes[targetIndex]));
        }

        private async Task MoveDrawingLayerInDisplayOrderAsync(TreeNode? node, int visualDirection)
        {
            if (!CanMoveDrawingLayerInDisplayOrder(node, visualDirection) || node?.Parent == null)
                return;

            List<CanvasLayer> siblings = node.Parent.Nodes
                .Cast<TreeNode>()
                .Select(GetLayerFromNode)
                .Where(l => l != null && CanvasLayerTreeService.IsDrawingMarkupLayer(l))
                .Select(l => l!)
                .ToList();

            CanvasLayer? currentLayer = GetLayerFromNode(node);
            int currentIndex = siblings.FindIndex(l => l.Id == currentLayer?.Id);
            int targetIndex = currentIndex + Math.Sign(visualDirection);
            if (currentIndex < 0 || targetIndex < 0 || targetIndex >= siblings.Count)
                return;

            (siblings[currentIndex].DisplayOrder, siblings[targetIndex].DisplayOrder) =
                (siblings[targetIndex].DisplayOrder, siblings[currentIndex].DisplayOrder);

            CanvasLayer movedLayer = siblings[targetIndex];

            if (AppServices.HasContext)
            {
                var repository = _projectScopedFactory.CreateCanvasLayerRepository(
                    AppServices.Context.Session);
                foreach (CanvasLayer sibling in siblings)
                {
                    if (sibling.Id > 0)
                    {
                        sibling.LastModifiedDate = DateTime.Now;
                        await repository.UpdateAsync(sibling);
                    }
                }
            }

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
            if (!EnsureApplicationUnlockedForEditing("deleting layers"))
                return;

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

            if (ShouldClearLayerObjectsInsteadOfDeletingLayer(layer))
            {
                await DeleteObjectsFromDefaultLayerAsync(layer);
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

        private async Task DeleteLayerGroupAsync(TreeNode? groupNode)
        {
            if (!EnsureApplicationUnlockedForEditing("deleting layer groups"))
                return;

            if (!IsRePlotDataLayerGroupNode(groupNode))
                return;

            List<CanvasLayer> layers = GetLayersFromGroupNode(groupNode);
            if (layers.Count == 0)
                return;

            List<CanvasLayer> lockedLayers = layers
                .Where(layer => layer.IsLocked)
                .ToList();
            if (lockedLayers.Count > 0)
            {
                MessageBox.Show(
                    $"Layer group '{groupNode!.Text}' contains locked layer(s).\n\nUnlock the group first if you want to delete it.",
                    "Layer Locked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            string? groupKey = GetLayerGroupKeyForGroupNode(groupNode);
            bool isCadastralGroup = string.Equals(
                groupKey,
                CadastralMapGroupKey,
                StringComparison.OrdinalIgnoreCase);

            if (!isCadastralGroup)
            {
                await DeleteObjectsFromDefaultLayerGroupAsync(groupNode!, layers);
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Delete all cadastral map child layers under '{groupNode!.Text}'?\n\nThis will remove all cadastral child layer nodes and their objects.",
                "Delete Layer Group",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            if (!AppServices.HasContext)
            {
                groupNode.Remove();
                ClearLayerProperties();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                return;
            }

            try
            {
                foreach (CanvasLayer layer in layers.OrderByDescending(item => item.DisplayOrder))
                {
                    await _layerCommandService.DeleteAsync(
                        AppServices.Context.Session,
                        layer);
                    _rasterImportFileManagementService.HandleLayerDeleted(
                        AppServices.Context,
                        layer);
                }

                MarkProjectModifiedIfOpen();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                ClearLayerProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete the layer group: {ex.Message}",
                    "Delete Layer Group",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task DeleteExternalLayerGroupAsync(TreeNode? groupNode)
        {
            if (!EnsureApplicationUnlockedForEditing("deleting external layers"))
                return;

            if (!IsExternalFileGroupNode(groupNode))
                return;

            List<CanvasLayer> layers = EnumerateLayerNodes(groupNode!)
                .Select(GetLayerFromNode)
                .Where(layer => layer != null)
                .Select(layer => layer!)
                .ToList();

            if (layers.Count == 0)
                return;

            DialogResult result = MessageBox.Show(
                $"Delete '{groupNode!.Text}'?\n\nThis will remove all imported layer nodes and objects under this file.",
                "Delete External Layers",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            if (!AppServices.HasContext)
            {
                groupNode.Remove();
                ClearLayerProperties();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                return;
            }

            try
            {
                foreach (CanvasLayer layer in layers.OrderByDescending(item => item.DisplayOrder))
                {
                    await _layerCommandService.DeleteAsync(
                        AppServices.Context.Session,
                        layer);
                }

                MarkProjectModifiedIfOpen();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                ClearLayerProperties();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete external layers: {ex.Message}",
                    "Delete External Layers",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task AddDrawingMarkupLayerAsync(TreeNode? groupNode)
        {
            if (!EnsureApplicationUnlockedForEditing("adding drawing layers"))
                return;

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
                const string initialLayerType = CanvasLayerTreeService.PointLayerType;
                string layerName = GetUniqueLayerName(
                    existingLayers,
                    GetDefaultDrawingLayerNameForLayerType(initialLayerType));
                int nextDisplayOrder = existingLayers.Count == 0
                    ? 0
                    : existingLayers.Max(layer => layer.DisplayOrder) + 1;
                var settingsRepository = _projectScopedFactory.CreateProjectSettingsRepository(
                    AppServices.Context.Session);
                var settings = await settingsRepository.GetProjectSettingsAsync();
                Color canvasBackgroundColor = ParseColorOrDefault(
                    settings?.CanvasBackgroundColor,
                    Color.White);
                string drawingColor = GetRandomDrawingMarkupColorHex();
                drawingColor = CanvasThemeColorService.AdjustHexColorForCanvasTheme(
                    canvasBackgroundColor,
                    drawingColor);

                CanvasLayer newLayer = new()
                {
                    Name = layerName,
                    LayerType = initialLayerType,
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
                    ShowFillTransparency = false,
                    FillTransparency = 50,
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
                    allowRename: true,
                    allowLayerKindChange: true,
                    layerNameSuggestionProvider: layerType => GetUniqueLayerName(
                        existingLayers,
                        GetDefaultDrawingLayerNameForLayerType(layerType)));
                PositionLayerPropertyManager(frm);
                frm.Shown += (_, _) => frm.FocusLayerNameTextBox();

                if (frm.ShowDialog(this) != DialogResult.OK)
                    return;

                newLayer.Name = GetUniqueLayerName(existingLayers, newLayer.Name);
                newLayer.Description = $"Drawing/markup layer: {newLayer.Name}";
                ApplyCanvasThemeToLayerColors(newLayer, canvasBackgroundColor);
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
        /// Keeps layer colors user-authored. Theme contrast adjustment is applied only
        /// when creating new layers, never by rewriting existing saved layer properties.
        /// </summary>
        private async Task UpdateLayerColorsForCanvasThemeAsync(Color canvasBackgroundColor)
        {
            _ = canvasBackgroundColor;
            await Task.CompletedTask;
        }

        private async Task ApplyCurrentCanvasThemeToLayerColorsAsync()
        {
            if (!AppServices.HasContext) return;

            var settingsRepository = _projectScopedFactory.CreateProjectSettingsRepository(
                AppServices.Context.Session);
            var settings = await settingsRepository.GetProjectSettingsAsync();
            Color canvasBackgroundColor = ParseColorOrDefault(
                settings?.CanvasBackgroundColor,
                Color.White);

            await UpdateLayerColorsForCanvasThemeAsync(canvasBackgroundColor);
        }

        private static void ApplyCanvasThemeToLayerColors(
            CanvasLayer layer,
            Color canvasBackgroundColor)
        {
            layer.BorderColor = CanvasThemeColorService.AdjustHexColorForCanvasTheme(
                canvasBackgroundColor,
                layer.BorderColor ?? "#000000");
            layer.FillColor = CanvasThemeColorService.AdjustNullableHexColorForCanvasTheme(
                canvasBackgroundColor,
                layer.FillColor);
            layer.LabelColor = CanvasThemeColorService.AdjustHexColorForCanvasTheme(
                canvasBackgroundColor,
                layer.LabelColor ?? "#000000");
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

        private async Task ZoomToCurrentLayerTreeContextAsync()
        {
            if (IsLayerGroupNode(_contextLayerGroupNode))
            {
                await ZoomToLayerGroupAsync(_contextLayerGroupNode);
                return;
            }

            await ZoomToLayerAsync(_contextLayerNode);
        }

        private async Task ZoomToLayerGroupAsync(TreeNode? groupNode)
        {
            if (!IsLayerGroupNode(groupNode))
                return;

            List<CanvasLayer> layers = EnumerateLayerNodes(groupNode!)
                .Select(GetLayerFromNode)
                .Where(layer => layer != null && !IsOnlineBasemapLayer(layer))
                .Select(layer => layer!)
                .ToList();

            if (layers.Count == 0)
                return;

            try
            {
                RectangleD? combinedBounds = null;
                foreach (CanvasLayer layer in layers)
                {
                    RectangleD? layerBounds =
                        await _layerBoundsService.GetWorldBoundsAsync(
                            AppServices.HasContext ? AppServices.Context.Session : null,
                            layer,
                            AppServices.HasContext
                                ? AppServices.Context.ProjectFolderPath
                                : null);

                    if (!layerBounds.HasValue)
                        continue;

                    combinedBounds = combinedBounds.HasValue
                        ? CombineWorldBounds(combinedBounds.Value, layerBounds.Value)
                        : layerBounds.Value;
                }

                if (!combinedBounds.HasValue)
                {
                    MessageBox.Show(
                        $"Layer group '{groupNode!.Text}' does not have drawable bounds yet.",
                        "Zoom To Layer",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                mapCanvasControlMain.SetPanToolActive(false);
                mnuPan.Checked = false;
                mapCanvasControlMain.ZoomToWorldBounds(combinedBounds.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to zoom to the layer group: {ex.Message}",
                    "Zoom To Layer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private static RectangleD CombineWorldBounds(RectangleD first, RectangleD second)
        {
            double minX = Math.Min(Math.Min(first.Left, first.Right), Math.Min(second.Left, second.Right));
            double maxX = Math.Max(Math.Max(first.Left, first.Right), Math.Max(second.Left, second.Right));
            double minY = Math.Min(Math.Min(first.Top, first.Bottom), Math.Min(second.Top, second.Bottom));
            double maxY = Math.Max(Math.Max(first.Top, first.Bottom), Math.Max(second.Top, second.Bottom));
            return new RectangleD(minX, minY, maxX - minX, maxY - minY);
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
            bool updateRasterStack = true,
            bool invalidateNode = true)
        {
            if (node.Tag is LayerTreeNodeState nodeState)
            {
                nodeState.Layer = layer;
                nodeState.IsOnlineBasemap = IsOnlineBasemapLayer(layer);
            }

            node.Text = layer.Name;
            if (invalidateNode)
                InvalidateLayerTreeNode(node);

            if (updateRasterStack)
                UpdateRasterCanvasLayersFromTree();
        }

        private void InvalidateLayerTreeNode(TreeNode node)
        {
            Rectangle bounds = node.Bounds;
            if (bounds.IsEmpty)
            {
                treeViewLayers.Invalidate();
                return;
            }

            treeViewLayers.Invalidate(new Rectangle(
                0,
                bounds.Top,
                treeViewLayers.ClientSize.Width,
                Math.Max(treeViewLayers.ItemHeight, bounds.Height)));
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

        private static void ShowLayerNameProtectedMessage()
        {
            MessageBox.Show(
                "Layer Name cannot be changed",
                "Rename Layer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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

                using (mapCanvasControlMain.DeferRenderUpdates())
                {
                    SetOperationProgress(layerPercent, "Loading map layers", showProgressForm: false);
                    await RefreshLayerTreeAsync();

                    SetOperationProgress(renderPercent, "Rendering map canvas", showProgressForm: false);
                    mapCanvasControlMain.RequestRender();
                }

                SetCanvasCommandStatus("Canvas: Refreshed");

                SetOperationProgress(100, "Canvas refreshed", showProgressForm: false);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private async Task RefreshInitialProjectCanvasAsync()
        {
            SetOperationProgress(84, "Loading canvas objects");
            CanvasFeatureService featureService =
                _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
            IReadOnlyList<CanvasFeature> features =
                await featureService.GetAllAsync();

            SetOperationProgress(92, "Preparing canvas render");
            await mapCanvasControlMain.SetVectorFeaturesAndWaitForCacheAsync(
                features);

            SetOperationProgress(98, "Rendering map canvas");
            using CancellationTokenSource firstPaintTimeout =
                new(TimeSpan.FromSeconds(5));
            try
            {
                await mapCanvasControlMain.ShowCanvasContentWhenReadyAsync(
                    firstPaintTimeout.Token);
            }
            catch (OperationCanceledException)
            {
                mapCanvasControlMain.ShowCanvasContent();
                mapCanvasControlMain.RequestRender();
            }

            SetCanvasCommandStatus("Canvas: Refreshed");
            SetOperationProgress(100, "Canvas refreshed");
        }

        private async Task RefreshVectorCanvasFeaturesAsync()
        {
            if (!AppServices.HasContext)
            {
                Stopwatch emptyCanvasStopwatch = Stopwatch.StartNew();
                mapCanvasControlMain.SetVectorFeatures([]);
                emptyCanvasStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Reload canvas objects",
                    "no project database is open",
                    0,
                    emptyCanvasStopwatch.Elapsed.TotalMilliseconds,
                    emptyCanvasStopwatch.Elapsed.TotalMilliseconds);
                return;
            }

            CanvasFeatureService featureService =
                _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
            Stopwatch databaseStopwatch = Stopwatch.StartNew();
            IReadOnlyList<CanvasFeature> features =
                await featureService.GetAllAsync();
            databaseStopwatch.Stop();
            Stopwatch canvasStopwatch = Stopwatch.StartNew();
            mapCanvasControlMain.SetVectorFeatures(features);
            canvasStopwatch.Stop();
            CanvasPerformanceTelemetry.RecordOperation(
                "Reload canvas objects",
                $"{features.Count:n0} objects loaded",
                databaseStopwatch.Elapsed.TotalMilliseconds,
                canvasStopwatch.Elapsed.TotalMilliseconds,
                databaseStopwatch.Elapsed.TotalMilliseconds + canvasStopwatch.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Adds a completed canvas edit command to the active project undo history.
        /// </summary>
        /// <param name="command">The command that can undo and redo a persisted canvas edit.</param>
        private void RegisterCanvasUndoCommand(IPersistedCanvasUndoCommand command)
        {
            _canvasUndoManager.Register(command);
        }

        /// <summary>
        /// Clears canvas edit undo history, usually when switching projects.
        /// </summary>
        private void ClearCanvasUndoRedoHistory()
        {
            _canvasUndoManager.Clear();
        }

        /// <summary>
        /// Refreshes the main toolbar undo and redo button state and tooltips.
        /// </summary>
        private void UpdateCanvasUndoRedoToolbar()
        {
            bool canUseHistory = AppServices.HasContext &&
                                 !_isApplicationEditLocked &&
                                 !_canvasUndoRedoOperationInProgress;

            mnuUndo.Enabled = canUseHistory && _canvasUndoManager.CanUndo;
            mnuRedo.Enabled = canUseHistory && _canvasUndoManager.CanRedo;
            mnuUndo.ToolTipText = mnuUndo.Enabled
                ? $"Undo {_canvasUndoManager.GetUndoDescription()}"
                : "Undo";
            mnuRedo.ToolTipText = mnuRedo.Enabled
                ? $"Redo {_canvasUndoManager.GetRedoDescription()}"
                : "Redo";
        }

        /// <summary>
        /// Runs the next undo command from the main canvas edit history.
        /// </summary>
        private async Task UndoCanvasCommandAsync()
        {
            if (_canvasUndoRedoOperationInProgress ||
                _isApplicationEditLocked ||
                !AppServices.HasContext ||
                !_canvasUndoManager.CanUndo)
            {
                return;
            }

            string description = _canvasUndoManager.GetUndoDescription();
            _canvasUndoRedoOperationInProgress = true;
            UpdateCanvasUndoRedoToolbar();

            try
            {
                SetOperationProgress(5, $"Undo {description}", showProgressForm: false);
                await _canvasUndoManager.UndoAsync(this);
                SetOperationProgress(100, $"Undo {description} complete", showProgressForm: false);
                SetCanvasCommandStatus($"Undo {description}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to undo '{description}': {ex.Message}",
                    "Undo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _canvasUndoRedoOperationInProgress = false;
                UpdateCanvasUndoRedoToolbar();
                HideOperationProgress();
            }
        }

        /// <summary>
        /// Runs the next redo command from the main canvas edit history.
        /// </summary>
        private async Task RedoCanvasCommandAsync()
        {
            if (_canvasUndoRedoOperationInProgress ||
                _isApplicationEditLocked ||
                !AppServices.HasContext ||
                !_canvasUndoManager.CanRedo)
            {
                return;
            }

            string description = _canvasUndoManager.GetRedoDescription();
            _canvasUndoRedoOperationInProgress = true;
            UpdateCanvasUndoRedoToolbar();

            try
            {
                SetOperationProgress(5, $"Redo {description}", showProgressForm: false);
                await _canvasUndoManager.RedoAsync(this);
                SetOperationProgress(100, $"Redo {description} complete", showProgressForm: false);
                SetCanvasCommandStatus($"Redo {description}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to redo '{description}': {ex.Message}",
                    "Redo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _canvasUndoRedoOperationInProgress = false;
                UpdateCanvasUndoRedoToolbar();
                HideOperationProgress();
            }
        }

        /// <summary>
        /// Recreates persisted canvas objects from stored snapshots and restores parcel/object backlinks.
        /// </summary>
        /// <param name="snapshots">The canvas object snapshots to restore.</param>
        private async Task RestoreCanvasObjectSnapshotsAsync(IReadOnlyList<CanvasObject> snapshots)
        {
            if (!AppServices.HasContext || snapshots.Count == 0)
            {
                return;
            }

            Stopwatch totalStopwatch = Stopwatch.StartNew();
            Stopwatch databaseStopwatch = Stopwatch.StartNew();
            SetOperationProgress(
                18,
                snapshots.Count == 1
                    ? "Preparing object restore"
                    : $"Preparing restore for {snapshots.Count:N0} objects",
                showProgressForm: false);
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            var objectRepository =
                _projectScopedFactory.CreateCanvasObjectRepository(AppServices.Context.Session);
            HashSet<Guid> existingIds = await LoadExistingCanvasObjectIdsAsync(
                context,
                snapshots.Select(snapshot => snapshot.Id));
            List<CanvasObject> entitiesToAdd = new();
            List<CanvasObject> entitiesToUpdate = new();

            foreach (CanvasObject snapshot in snapshots)
            {
                CanvasObject entity = CloneCanvasObjectSnapshot(snapshot);
                if (existingIds.Contains(entity.Id))
                {
                    entitiesToUpdate.Add(entity);
                }
                else
                {
                    entitiesToAdd.Add(entity);
                }
            }

            SetOperationProgress(
                46,
                snapshots.Count == 1
                    ? "Writing restored object to database"
                    : $"Writing {snapshots.Count:N0} restored objects to database",
                showProgressForm: false);
            await objectRepository.UpdateRangeAsync(entitiesToUpdate);
            await objectRepository.AddRangeAsync(entitiesToAdd);
            SetOperationProgress(66, "Restoring object links", showProgressForm: false);
            await RestoreCanvasBackLinksAsync(context, snapshots);
            if (await CanvasObjectSnapshotsAffectGeneratedRoadParcelAsync(context, snapshots))
            {
                SetOperationProgress(76, "Refreshing generated road parcel", showProgressForm: false);
                await RefreshGeneratedRoadParcelAsync(context);
            }
            databaseStopwatch.Stop();

            MarkProjectModifiedIfOpen();
            Stopwatch canvasStopwatch = Stopwatch.StartNew();
            SetOperationProgress(86, "Refreshing canvas after restore", showProgressForm: false);
            await RefreshVectorCanvasFeaturesAsync();
            canvasStopwatch.Stop();
            totalStopwatch.Stop();
            CanvasPerformanceTelemetry.RecordOperation(
                "Restore objects",
                $"{snapshots.Count:n0} objects restored",
                databaseStopwatch.Elapsed.TotalMilliseconds,
                canvasStopwatch.Elapsed.TotalMilliseconds,
                totalStopwatch.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Deletes persisted canvas objects by id and refreshes the active canvas.
        /// </summary>
        /// <param name="canvasObjectIds">The canvas object identifiers to delete.</param>
        private async Task DeleteCanvasObjectsForUndoRedoAsync(IReadOnlyList<Guid> canvasObjectIds)
        {
            if (!AppServices.HasContext || canvasObjectIds.Count == 0)
            {
                return;
            }

            Stopwatch totalStopwatch = Stopwatch.StartNew();
            SetOperationProgress(
                18,
                canvasObjectIds.Count == 1
                    ? "Preparing object delete"
                    : $"Preparing delete for {canvasObjectIds.Count:N0} objects",
                showProgressForm: false);
            CanvasFeatureService featureService =
                _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
            AppDbContext context = AppServices.Context.Session.GetDbContext();
            Stopwatch databaseStopwatch = Stopwatch.StartNew();
            bool shouldRefreshRoadParcel =
                await CanvasObjectIdsAffectGeneratedRoadParcelAsync(context, canvasObjectIds);
            Guid[] distinctIds = canvasObjectIds.Distinct().ToArray();

            SetOperationProgress(
                48,
                distinctIds.Length == 1
                    ? "Deleting object from database"
                    : $"Deleting {distinctIds.Length:N0} objects from database",
                showProgressForm: false);
            await featureService.DeleteShapesAsync(distinctIds);

            if (shouldRefreshRoadParcel)
            {
                SetOperationProgress(72, "Refreshing generated road parcel", showProgressForm: false);
                await RefreshGeneratedRoadParcelAsync(context);
                databaseStopwatch.Stop();
                Stopwatch refreshCanvasStopwatch = Stopwatch.StartNew();
                SetOperationProgress(86, "Refreshing canvas after delete", showProgressForm: false);
                await RefreshVectorCanvasFeaturesAsync();
                refreshCanvasStopwatch.Stop();
                totalStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Undo/redo delete objects",
                    $"{distinctIds.Length:n0} objects deleted",
                    databaseStopwatch.Elapsed.TotalMilliseconds,
                    refreshCanvasStopwatch.Elapsed.TotalMilliseconds,
                    totalStopwatch.Elapsed.TotalMilliseconds);
            }
            else
            {
                databaseStopwatch.Stop();
                Stopwatch canvasStopwatch = Stopwatch.StartNew();
                SetOperationProgress(86, "Updating canvas after delete", showProgressForm: false);
                mapCanvasControlMain.RemoveCanvasObjectsImmediatelyAfterDelete(distinctIds);
                canvasStopwatch.Stop();
                totalStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Undo/redo delete objects",
                    $"{distinctIds.Length:n0} objects deleted",
                    databaseStopwatch.Elapsed.TotalMilliseconds,
                    canvasStopwatch.Elapsed.TotalMilliseconds,
                    totalStopwatch.Elapsed.TotalMilliseconds);
            }

            mapCanvasControlMain.ClearSelectionAfterDelete();
            MarkProjectModifiedIfOpen();
        }

        /// <summary>
        /// Restores one-to-one navigation links from parcel, road, and block tables back to canvas objects.
        /// </summary>
        /// <param name="context">The active project database context.</param>
        /// <param name="snapshots">The restored canvas object snapshots.</param>
        private static async Task RestoreCanvasBackLinksAsync(
            AppDbContext context,
            IReadOnlyList<CanvasObject> snapshots)
        {
            foreach (CanvasObject snapshot in snapshots)
            {
                if (snapshot.BaselineParcelId.HasValue)
                {
                    BaselineParcel? parcel = await context.BaselineParcels.FindAsync(snapshot.BaselineParcelId.Value);
                    if (parcel != null)
                    {
                        parcel.CanvasObjectId = snapshot.Id;
                    }
                }

                if (snapshot.ReplottedParcelId.HasValue)
                {
                    Core.Entities.Replotting.ReplottedParcel? parcel =
                        await context.ReplottedParcels.FindAsync(snapshot.ReplottedParcelId.Value);
                    if (parcel != null)
                    {
                        parcel.CanvasObjectId = snapshot.Id;
                    }
                }

                if (snapshot.RoadId.HasValue)
                {
                    Core.Entities.Layout.Road? road = await context.Roads.FindAsync(snapshot.RoadId.Value);
                    if (road != null)
                    {
                        road.CanvasObjectId = snapshot.Id;
                    }
                }

                if (snapshot.BlockId.HasValue)
                {
                    Core.Entities.Layout.Block? block = await context.Blocks.FindAsync(snapshot.BlockId.Value);
                    if (block != null)
                    {
                        block.CanvasObjectId = snapshot.Id;
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a detached scalar snapshot of a canvas object for undo/redo persistence.
        /// </summary>
        /// <param name="source">The canvas object to snapshot.</param>
        private static CanvasObject CloneCanvasObjectSnapshot(CanvasObject source)
        {
            return new CanvasObject
            {
                Id = source.Id,
                CanvasLayerId = source.CanvasLayerId,
                ObjectType = source.ObjectType,
                Shape = source.Shape.Copy(),
                GeometryMetadataJson = source.GeometryMetadataJson,
                BorderColorOverride = source.BorderColorOverride,
                FillColorOverride = source.FillColorOverride,
                FillTransparencyOverride = source.FillTransparencyOverride,
                LineWeightOverride = source.LineWeightOverride,
                LineStyleOverride = source.LineStyleOverride,
                LabelText = source.LabelText,
                ObjectDescription = source.ObjectDescription,
                IsVisible = source.IsVisible,
                IsLocked = source.IsLocked,
                BaselineParcelId = source.BaselineParcelId,
                ReplottedParcelId = source.ReplottedParcelId,
                RoadId = source.RoadId,
                BlockId = source.BlockId,
                SourceDxfHandle = source.SourceDxfHandle,
                CreatedDate = source.CreatedDate,
                LastModifiedDate = source.LastModifiedDate
            };
        }

        private static async Task<Dictionary<Guid, CanvasObject>> LoadCanvasObjectsForEditAsync(
            AppDbContext context,
            IEnumerable<Guid> canvasObjectIds)
        {
            const int idBatchSize = 500;
            Guid[] distinctIds = canvasObjectIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();

            Dictionary<Guid, CanvasObject> result = new(distinctIds.Length);
            for (int index = 0; index < distinctIds.Length; index += idBatchSize)
            {
                Guid[] batch = distinctIds
                    .Skip(index)
                    .Take(idBatchSize)
                    .ToArray();

                List<CanvasObject> objects = await context.CanvasObjects
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Where(item => batch.Contains(item.Id))
                    .ToListAsync();

                foreach (CanvasObject canvasObject in objects)
                {
                    result[canvasObject.Id] = canvasObject;
                }
            }

            return result;
        }

        private static async Task<HashSet<Guid>> LoadExistingCanvasObjectIdsAsync(
            AppDbContext context,
            IEnumerable<Guid> canvasObjectIds)
        {
            const int idBatchSize = 500;
            Guid[] distinctIds = canvasObjectIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();

            HashSet<Guid> result = new();
            for (int index = 0; index < distinctIds.Length; index += idBatchSize)
            {
                Guid[] batch = distinctIds
                    .Skip(index)
                    .Take(idBatchSize)
                    .ToArray();

                List<Guid> existingIds = await context.CanvasObjects
                    .AsNoTracking()
                    .Where(item => batch.Contains(item.Id))
                    .Select(item => item.Id)
                    .ToListAsync();

                foreach (Guid id in existingIds)
                {
                    result.Add(id);
                }
            }

            return result;
        }

        /// <summary>
        /// Restores persisted canvas layer scalar state and refreshes the layer UI/canvas bindings.
        /// </summary>
        /// <param name="snapshots">The layer snapshots to restore.</param>
        private async Task RestoreCanvasLayerSnapshotsAsync(IReadOnlyList<CanvasLayer> snapshots)
        {
            if (!AppServices.HasContext || snapshots.Count == 0)
            {
                return;
            }

            AppDbContext context = AppServices.Context.Session.GetDbContext();

            foreach (CanvasLayer snapshot in snapshots)
            {
                CanvasLayer entity = CloneCanvasLayerSnapshot(snapshot);
                Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CanvasLayer>? localTracked =
                    context.ChangeTracker
                        .Entries<CanvasLayer>()
                        .FirstOrDefault(entry => entry.Entity.Id == entity.Id);

                if (localTracked != null)
                {
                    localTracked.State = EntityState.Detached;
                }

                bool exists = await context.CanvasLayers
                    .AsNoTracking()
                    .AnyAsync(item => item.Id == entity.Id);

                if (exists)
                {
                    context.CanvasLayers.Update(entity);
                }
                else
                {
                    await context.CanvasLayers.AddAsync(entity);
                }
            }

            await context.SaveChangesAsync();
            MarkProjectModifiedIfOpen();
            await RefreshLayerTreeAsync();
            RefreshCurrentDrawingLayerCombo();

            if (snapshots.Count == 1)
                SelectLayerNodeById(snapshots[0].Id);
        }

        /// <summary>
        /// Creates a detached scalar snapshot of a canvas layer for undo/redo persistence.
        /// </summary>
        /// <param name="source">The layer to snapshot.</param>
        private static CanvasLayer CloneCanvasLayerSnapshot(CanvasLayer source)
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
                LineTypeScale = source.LineTypeScale,
                FillColor = source.FillColor,
                ShowFillTransparency = source.ShowFillTransparency,
                FillTransparency = source.FillTransparency,
                FillStyle = source.FillStyle,
                HatchPattern = source.HatchPattern,
                HatchScale = source.HatchScale,
                ShowLabels = source.ShowLabels,
                LabelFontName = source.LabelFontName,
                LabelFontSize = source.LabelFontSize,
                LabelColor = source.LabelColor,
                LabelField = source.LabelField,
                LabelScaleWithZoom = source.LabelScaleWithZoom,
                TextAlignment = source.TextAlignment,
                PointSymbol = source.PointSymbol,
                PointSize = source.PointSize,
                SourceFile = source.SourceFile,
                ImportedDate = source.ImportedDate,
                CreatedDate = source.CreatedDate,
                LastModifiedDate = source.LastModifiedDate,
                Description = source.Description
            };
        }

        private async void MapCanvasControlMain_ShapeCompleted(IShape shape)
        {
            if (!EnsureApplicationUnlockedForEditing("creating canvas objects"))
            {
                await RefreshVectorCanvasFeaturesAsync();
                return;
            }

            if (!AppServices.HasContext)
            {
                return;
            }

            Stopwatch totalStopwatch = Stopwatch.StartNew();
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
                Stopwatch databaseStopwatch = Stopwatch.StartNew();
                CanvasFeature feature = await featureService.SaveShapeAsync(
                    shape,
                    shape.LayerName);
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                bool affectsRoadParcel =
                    BlockRoadParcelRefreshService.AffectsGeneratedRoadParcel(feature.CanvasObject);
                if (affectsRoadParcel)
                {
                    await RefreshGeneratedRoadParcelAsync(context);
                }
                databaseStopwatch.Stop();

                MarkProjectModifiedIfOpen();
                Stopwatch canvasStopwatch = Stopwatch.StartNew();
                if (affectsRoadParcel)
                {
                    await RefreshVectorCanvasFeaturesAsync();
                }
                else
                {
                    mapCanvasControlMain.UpsertVectorFeatures([feature]);
                }
                canvasStopwatch.Stop();
                totalStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Draw object",
                    $"{feature.CanvasObject.ObjectType} saved",
                    databaseStopwatch.Elapsed.TotalMilliseconds,
                    canvasStopwatch.Elapsed.TotalMilliseconds,
                    totalStopwatch.Elapsed.TotalMilliseconds);

                RegisterCanvasUndoCommand(
                    new AddCanvasObjectsCommand([CloneCanvasObjectSnapshot(feature.CanvasObject)]));
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

        private async void MapCanvasControlMain_ShapesCompleted(IReadOnlyList<IShape> shapes)
        {
            if (shapes == null || shapes.Count == 0)
            {
                return;
            }

            if (!EnsureApplicationUnlockedForEditing("creating canvas objects"))
            {
                await RefreshVectorCanvasFeaturesAsync();
                return;
            }

            if (!AppServices.HasContext)
            {
                return;
            }

            try
            {
                SetOperationProgress(
                    24,
                    shapes.Count == 1
                        ? "Saving copied object"
                        : $"Saving {shapes.Count:N0} copied objects",
                    showProgressForm: false);
                CanvasFeatureService featureService =
                    _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<CanvasObject> createdSnapshots = new();
                List<Guid> createdObjectIds = new();
                bool anyAffectsRoadParcel = false;

                Stopwatch totalStopwatch = Stopwatch.StartNew();
                Stopwatch databaseStopwatch = Stopwatch.StartNew();
                IReadOnlyList<CanvasFeature> features = await featureService.SaveNewShapesAsync(
                    shapes,
                    GetSelectedCurrentDrawingLayer()?.Name ?? _currentDrawingLayer?.Name ?? "Features");
                SetOperationProgress(
                    58,
                    features.Count == 1
                        ? "Preparing copied object for canvas"
                        : $"Preparing {features.Count:N0} copied objects for canvas",
                    showProgressForm: false);

                foreach (CanvasFeature feature in features)
                {
                    createdSnapshots.Add(CloneCanvasObjectSnapshot(feature.CanvasObject));
                    createdObjectIds.Add(feature.CanvasObject.Id);
                    anyAffectsRoadParcel |=
                        BlockRoadParcelRefreshService.AffectsGeneratedRoadParcel(feature.CanvasObject);
                }

                if (anyAffectsRoadParcel)
                {
                    SetOperationProgress(72, "Refreshing generated road parcel", showProgressForm: false);
                    await RefreshGeneratedRoadParcelAsync(context);
                }
                databaseStopwatch.Stop();

                MarkProjectModifiedIfOpen();
                Stopwatch canvasStopwatch = Stopwatch.StartNew();
                if (anyAffectsRoadParcel)
                {
                    SetOperationProgress(84, "Reloading canvas after copy", showProgressForm: false);
                    await RefreshVectorCanvasFeaturesAsync();
                    SetOperationProgress(92, "Selecting copied objects", showProgressForm: false);
                    mapCanvasControlMain.SelectCanvasObjects(createdObjectIds, zoomToSelection: false);
                }
                else
                {
                    SetOperationProgress(84, "Updating canvas with copied objects", showProgressForm: false);
                    mapCanvasControlMain.UpsertVectorFeatures(
                        features,
                        selectUpsertedFeatures: true);
                }
                canvasStopwatch.Stop();
                totalStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Create objects",
                    $"{createdObjectIds.Count:n0} objects saved",
                    databaseStopwatch.Elapsed.TotalMilliseconds,
                    canvasStopwatch.Elapsed.TotalMilliseconds,
                    totalStopwatch.Elapsed.TotalMilliseconds);

                RegisterCanvasUndoCommand(new AddCanvasObjectsCommand(createdSnapshots));
                SetOperationProgress(100, "Copy complete", showProgressForm: false);
                SetCanvasCommandStatus(createdObjectIds.Count == 1
                    ? "Copied object"
                    : $"Copied {createdObjectIds.Count} objects");
            }
            catch (Exception ex)
            {
                await RefreshVectorCanvasFeaturesAsync();
                MessageBox.Show(
                    $"Failed to save copied objects: {ex.Message}",
                    "Copy Objects",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private void MapCanvasControlMain_LongOperationProgressChanged(int percent, string status)
        {
            SetOperationProgress(percent, status, showProgressForm: false);
        }

        private void MapCanvasControlMain_LongOperationProgressCompleted()
        {
            HideOperationProgress();
        }

        private async void MapCanvasControlMain_ShapesEdited(IReadOnlyList<IShape> shapes)
        {
            if (shapes == null || shapes.Count == 0)
            {
                return;
            }

            if (!EnsureApplicationUnlockedForEditing("editing canvas objects"))
            {
                await RefreshVectorCanvasFeaturesAsync();
                return;
            }

            if (!AppServices.HasContext)
            {
                return;
            }

            await _shapeEditSaveLock.WaitAsync();
            Stopwatch totalStopwatch = Stopwatch.StartNew();
            try
            {
                SetOperationProgress(
                    8,
                    shapes.Count == 1
                        ? "Saving moved object"
                        : $"Saving {shapes.Count:N0} moved objects",
                    showProgressForm: false);
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                CanvasFeatureService featureService =
                    _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);

                List<CanvasObject> beforeSnapshots = new();
                List<CanvasObject> afterSnapshots = new();
                bool anyAffectsRoadParcel = false;
                Guid? firstSavedShapeId = null;
                string? lastSavedLabel = null;
                Stopwatch databaseStopwatch = Stopwatch.StartNew();
                Dictionary<Guid, CanvasObject> existingObjects =
                    await LoadCanvasObjectsForEditAsync(context, shapes.Select(shape => shape.Id));
                databaseStopwatch.Stop();
                SetOperationProgress(
                    28,
                    existingObjects.Count == 1
                        ? "Preparing object update"
                        : $"Preparing {existingObjects.Count:N0} object updates",
                    showProgressForm: false);
                List<(IShape Shape, CanvasObject ExistingObject)> saveItems = new();

                // Persist every edited shape, but defer the (expensive) feature
                // reload / cache rebuild / property reload to a SINGLE call after
                // the whole batch — moving N shapes must not reload the project N
                // times.
                foreach (IShape shape in shapes)
                {
                    if (!existingObjects.TryGetValue(shape.Id, out CanvasObject? existingObject))
                    {
                        continue;
                    }

                    if (existingObject.CanvasLayer?.IsLocked == true ||
                        (existingObject.CanvasLayer != null &&
                         !CanvasLayerTreeService.IsDrawingMarkupLayer(existingObject.CanvasLayer) &&
                         !CanvasLayerTreeService.IsProjectBoundaryLayer(existingObject.CanvasLayer) &&
                         !BlockRoadParcelRefreshService.IsBlockLayer(existingObject.CanvasLayer)))
                    {
                        // Skip shapes whose layer is not editable.
                        continue;
                    }

                    CanvasObject beforeSnapshot = CloneCanvasObjectSnapshot(existingObject);
                    if (existingObject.CanvasLayer != null)
                    {
                        shape.LayerName = existingObject.CanvasLayer.Name;
                        shape.Properties[CanvasFeatureService.CanvasLayerIdPropertyKey] = existingObject.CanvasLayer.Id;
                    }

                    beforeSnapshots.Add(beforeSnapshot);
                    saveItems.Add((shape, existingObject));
                    firstSavedShapeId ??= shape.Id;
                    lastSavedLabel = $"{existingObject.ObjectType}: {existingObject.CanvasLayer?.Name ?? shape.LayerName}";

                    if (BlockRoadParcelRefreshService.AffectsGeneratedRoadParcel(existingObject))
                    {
                        anyAffectsRoadParcel = true;
                    }
                }

                if (beforeSnapshots.Count == 0)
                {
                    SetCanvasCommandStatus("Edit ignored because the object layer is not editable");
                    await RefreshVectorCanvasFeaturesAsync();
                    return;
                }

                SetOperationProgress(
                    48,
                    saveItems.Count == 1
                        ? "Writing moved object to database"
                        : $"Writing {saveItems.Count:N0} moved objects to database",
                    showProgressForm: false);
                databaseStopwatch.Start();
                IReadOnlyList<CanvasFeature> savedFeatures =
                    await featureService.SaveExistingShapesAsync(saveItems);
                afterSnapshots.AddRange(savedFeatures.Select(feature =>
                    CloneCanvasObjectSnapshot(feature.CanvasObject)));
                anyAffectsRoadParcel |= savedFeatures.Any(feature =>
                    BlockRoadParcelRefreshService.AffectsGeneratedRoadParcel(feature.CanvasObject));

                if (anyAffectsRoadParcel)
                {
                    SetOperationProgress(72, "Refreshing generated road parcel", showProgressForm: false);
                    await RefreshGeneratedRoadParcelAsync(context);
                }
                databaseStopwatch.Stop();

                MarkProjectModifiedIfOpen();
                Stopwatch canvasStopwatch = Stopwatch.StartNew();
                if (anyAffectsRoadParcel)
                {
                    SetOperationProgress(84, "Reloading canvas after move", showProgressForm: false);
                    await RefreshVectorCanvasFeaturesAsync();
                }
                else
                {
                    SetOperationProgress(
                        84,
                        savedFeatures.Count == 1
                            ? "Updating moved object on canvas"
                            : $"Updating {savedFeatures.Count:N0} moved objects on canvas",
                        showProgressForm: false);
                    mapCanvasControlMain.UpsertVectorFeatures(savedFeatures);
                }
                canvasStopwatch.Stop();
                totalStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Edit or move objects",
                    $"{savedFeatures.Count:n0} objects saved",
                    databaseStopwatch.Elapsed.TotalMilliseconds,
                    canvasStopwatch.Elapsed.TotalMilliseconds,
                    totalStopwatch.Elapsed.TotalMilliseconds);

                if (firstSavedShapeId.HasValue)
                {
                    SetOperationProgress(94, "Refreshing selected object properties", showProgressForm: false);
                    await RefreshCurrentSelectedCanvasObjectPropertiesAsync(firstSavedShapeId.Value);
                }

                RegisterCanvasUndoCommand(new ModifyCanvasObjectsCommand(beforeSnapshots, afterSnapshots));
                SetOperationProgress(100, "Move complete", showProgressForm: false);
                SetCanvasCommandStatus(beforeSnapshots.Count == 1
                    ? $"Edited {lastSavedLabel}"
                    : $"Edited {beforeSnapshots.Count} objects");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save the edit: {ex.Message}",
                    "Edit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                await RefreshVectorCanvasFeaturesAsync();
            }
            finally
            {
                _shapeEditSaveLock.Release();
                HideOperationProgress();
            }
        }

        private async void MapCanvasControlMain_SelectedObjectsDeleteRequested(IReadOnlyList<Guid> shapeIds)
        {
            if (!EnsureApplicationUnlockedForEditing("deleting canvas objects"))
                return;

            if (!AppServices.HasContext || shapeIds.Count == 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show(
                shapeIds.Count == 1
                    ? "Delete the selected object?"
                    : $"Delete {shapeIds.Count} selected objects?",
                "Delete Object",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Stopwatch totalStopwatch = Stopwatch.StartNew();
            try
            {
                SetOperationProgress(
                    4,
                    shapeIds.Count == 1
                        ? "Preparing selected object delete"
                        : $"Preparing delete for {shapeIds.Count:N0} selected objects",
                    showProgressForm: false);
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                Stopwatch databaseStopwatch = Stopwatch.StartNew();
                List<CanvasObject> selectedObjects =
                    (await LoadCanvasObjectsForEditAsync(context, shapeIds)).Values.ToList();
                databaseStopwatch.Stop();

                CanvasObject? lockedObject = selectedObjects.FirstOrDefault(item =>
                    item.CanvasLayer?.IsLocked == true ||
                    item.IsLocked);
                if (lockedObject != null)
                {
                    MessageBox.Show(
                        $"Object deletion is blocked because layer '{lockedObject.CanvasLayer?.Name ?? "Unknown"}' or the selected object is locked.",
                        "Delete Object",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                List<CanvasObject> deletedObjectSnapshots = selectedObjects
                    .Select(CloneCanvasObjectSnapshot)
                    .ToList();

                int cadastralObjectCount = selectedObjects.Count(item =>
                    item.CanvasLayer != null && IsCadastralCanvasLayer(item.CanvasLayer));
                if (cadastralObjectCount > 0)
                {
                    DialogResult cadastralDeleteResult = MessageBox.Show(
                        this,
                        cadastralObjectCount == 1
                            ? "The selected object is part of an imported cadastral map. Deleting it will remove its parcel assignment link from the map.\n\nDelete this cadastral object?"
                            : $"{cadastralObjectCount} selected objects are part of imported cadastral maps. Deleting them will remove their parcel assignment links from the map.\n\nDelete these cadastral objects?",
                        "Delete Cadastral Map Object",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button2);
                    if (cadastralDeleteResult != DialogResult.Yes)
                        return;
                }

                CanvasFeatureService featureService =
                    _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);

                Guid[] confirmedDeleteIds = selectedObjects
                    .Select(item => item.Id)
                    .Distinct()
                    .ToArray();
                Guid[] distinctShapeIds = shapeIds
                    .Distinct()
                    .ToArray();
                bool shouldRefreshRoadParcel = ShouldRefreshGeneratedRoadParcel(selectedObjects);
                int totalProgressSteps = Math.Max(
                    1,
                    (shouldRefreshRoadParcel ? 1 : 0) + 2);
                int completedProgressSteps = 0;

                void ReportDeleteProgress(string status)
                {
                    int percent = Math.Clamp(
                        5 + (int)Math.Round(completedProgressSteps * 90.0 / totalProgressSteps),
                        5,
                        95);
                    SetOperationProgress(percent, status, showProgressForm: false);
                }

                Stopwatch canvasStopwatch = Stopwatch.StartNew();
                mapCanvasControlMain.RemoveCanvasObjectsImmediatelyAfterDelete(confirmedDeleteIds);
                canvasStopwatch.Stop();
                SetCanvasCommandStatus(confirmedDeleteIds.Length == 1
                    ? "Deleting selected object..."
                    : $"Deleting {confirmedDeleteIds.Length} selected objects...");
                ReportDeleteProgress(confirmedDeleteIds.Length == 1
                    ? "Deleting selected object..."
                    : $"Deleting {confirmedDeleteIds.Length} selected objects...");

                databaseStopwatch.Start();
                await featureService.DeleteShapesAsync(distinctShapeIds);
                databaseStopwatch.Stop();
                completedProgressSteps++;
                ReportDeleteProgress(distinctShapeIds.Length == 1
                    ? "Deleted selected object from database"
                    : $"Deleted {distinctShapeIds.Length} objects from database");

                if (shouldRefreshRoadParcel)
                {
                    ReportDeleteProgress("Refreshing generated road parcel...");
                    databaseStopwatch.Start();
                    await RefreshGeneratedRoadParcelAsync(context);
                    databaseStopwatch.Stop();
                    completedProgressSteps++;
                    ReportDeleteProgress("Refreshing canvas after delete...");
                    canvasStopwatch.Start();
                    await RefreshVectorCanvasFeaturesAsync();
                    canvasStopwatch.Stop();
                    completedProgressSteps++;
                }

                MarkProjectModifiedIfOpen();
                totalStopwatch.Stop();
                CanvasPerformanceTelemetry.RecordOperation(
                    "Delete objects",
                    $"{distinctShapeIds.Length:n0} objects deleted",
                    databaseStopwatch.Elapsed.TotalMilliseconds,
                    canvasStopwatch.Elapsed.TotalMilliseconds,
                    totalStopwatch.Elapsed.TotalMilliseconds);
                SetOperationProgress(100, "Delete complete", showProgressForm: false);
                RegisterCanvasUndoCommand(new DeleteCanvasObjectsCommand(deletedObjectSnapshots));
                SetCanvasCommandStatus(shapeIds.Count == 1
                    ? "Deleted selected object"
                    : $"Deleted {shapeIds.Count} selected objects");
            }
            catch (Exception ex)
            {
                SetOperationProgress(95, "Restoring canvas after failed delete...", showProgressForm: false);
                await RefreshVectorCanvasFeaturesAsync();
                MessageBox.Show(
                    $"Failed to delete the selected objects: {ex.Message}",
                    "Delete Object",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                HideOperationProgress();
            }
        }

        private void MapCanvasControlMain_SelectedObjectsAssignDataRequested(
            CanvasObjectAssignmentKind assignmentKind)
        {
            if (!EnsureApplicationUnlockedForEditing("assigning data"))
                return;

            Guid? preferredCanvasObjectId = _currentSelectedCanvasObjectIds
                .FirstOrDefault(id => id != Guid.Empty);
            if (preferredCanvasObjectId == Guid.Empty)
                preferredCanvasObjectId = null;

            switch (assignmentKind)
            {
                case CanvasObjectAssignmentKind.Parcel:
                    ShowCadastralRecordAssignmentForm(
                        openAutoAssignment: false,
                        preferredCanvasObjectId: preferredCanvasObjectId);
                    break;
                case CanvasObjectAssignmentKind.Road:
                    ShowRoadCenterlineAssignmentForm(preferredCanvasObjectId);
                    break;
                case CanvasObjectAssignmentKind.Block:
                    ShowBlockAssignmentForm(preferredCanvasObjectId);
                    break;
            }
        }

        private async void MapCanvasControlMain_SelectedObjectsViewEditDataRequested(
            CanvasObjectAssignmentKind assignmentKind)
        {
            try
            {
                switch (assignmentKind)
                {
                    case CanvasObjectAssignmentKind.Parcel:
                        await ViewEditSelectedOriginalParcelDataAsync();
                        break;
                    case CanvasObjectAssignmentKind.Road:
                        await ViewEditSelectedRoadDataAsync();
                        break;
                    case CanvasObjectAssignmentKind.Block:
                        await ViewEditSelectedBlockDataAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not open linked data: {ex.Message}",
                    "View/Edit Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task ViewEditSelectedOriginalParcelDataAsync()
        {
            if (!AppServices.HasContext)
            {
                SetCanvasCommandStatus("Open a project before editing parcel data.");
                return;
            }

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            CanvasObject? selectedObject = await GetFirstSelectedCanvasObjectWithLinkedDataAsync(
                context,
                CanvasObjectAssignmentKind.Parcel);
            if (selectedObject == null)
            {
                SetCanvasCommandStatus("Selected object has no linked parcel data.");
                return;
            }

            BaselineParcel? parcel = await ResolveLinkedBaselineParcelAsync(context, selectedObject);
            if (parcel == null)
            {
                SetCanvasCommandStatus("Linked parcel data was not found.");
                return;
            }

            var landRecordsService = _projectScopedFactory.CreateLandRecordsService(
                AppServices.Context.Session,
                AppServices.Context.ProjectFilePath);
            Land_Readjustment_Tool.Models.BaselineLandParcelRecord record =
                ConvertBaselineParcelToEditableRecord(parcel);

            using var editor = new frmAddEditRecord(
                record,
                parcel.Id,
                landRecordsService.ParcelExists,
                ownerFieldsReadOnly: true);
            editor.ReadOnlyMode = _isApplicationEditLocked;
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            if (_isApplicationEditLocked)
                return;

            if (editor.IsDeleted)
            {
                landRecordsService.DeleteParcel(parcel.Id);
                await ClearDeletedBaselineParcelLinksAsync(context, parcel.Id);
                SetCanvasCommandStatus("Deleted linked parcel data.");
            }
            else
            {
                Land_Readjustment_Tool.Models.BaselineLandParcelRecord updated = editor.Record;
                landRecordsService.UpdateParcel(new Land_Readjustment_Tool.Models.OriginalLandParcel
                {
                    ParcelId = parcel.Id,
                    LandOwnerId = parcel.LandOwnerId,
                    ParcelNo = updated.ParcelNo ?? string.Empty,
                    MapSheetNo = updated.MapSheetNo ?? string.Empty,
                    Province = updated.Province,
                    District = updated.District,
                    MunicipalityVillage = updated.MunicipalityVillage,
                    WardNo = updated.WardNo,
                    ParcelLocation = updated.ParcelLocation,
                    IsTenant = updated.Tenant,
                    TenantName = updated.TenantName,
                    LandUse = updated.LandUse,
                    LandOwnershipType = updated.LandOwnershipType,
                    AreaInSqm = updated.AreaInSqm,
                    FieldMeasuredAreaSqm = updated.FieldMeasuredAreaSqm,
                    AreaInRAPD = updated.AreaInRAPD,
                    AreaInBKD = updated.AreaInBKD,
                    MothNo = updated.MothNo,
                    PaanaNo = updated.PaanaNo,
                    Remarks = updated.Remarks,
                    JointCoOwners = updated.JointCoOwners
                });
                SetCanvasCommandStatus("Updated linked parcel data.");
            }

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject.Id);
        }

        private async Task ViewEditSelectedRoadDataAsync()
        {
            if (!AppServices.HasContext)
            {
                SetCanvasCommandStatus("Open a project before editing road data.");
                return;
            }

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            CanvasObject? selectedObject = await GetFirstSelectedCanvasObjectWithLinkedDataAsync(
                context,
                CanvasObjectAssignmentKind.Road);
            Core.Entities.Layout.Road? road = selectedObject == null
                ? null
                : await ResolveLinkedRoadAsync(context, selectedObject);
            if (road == null)
            {
                SetCanvasCommandStatus("Selected object has no linked road data.");
                return;
            }

            using var editor = new frmRoadDefinitionEditor(
                road,
                readOnlyMode: _isApplicationEditLocked);
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            if (_isApplicationEditLocked)
                return;

            Core.Entities.Layout.Road edited = editor.Road;
            road.RoadName = edited.RoadName;
            road.RoadCode = edited.RoadCode;
            road.SurfaceType = edited.SurfaceType;
            road.RoadWidth = edited.RoadWidth;
            road.RightOfWayWidth = edited.RightOfWayWidth;
            road.RoadType = edited.RoadType;
            road.RoadStatus = edited.RoadStatus;
            road.Description = edited.Description;
            road.LastModifiedDate = DateTime.Now;
            await context.SaveChangesAsync();

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject!.Id);
            SetCanvasCommandStatus("Updated linked road data.");
        }

        private async Task ViewEditSelectedBlockDataAsync()
        {
            if (!AppServices.HasContext)
            {
                SetCanvasCommandStatus("Open a project before editing block data.");
                return;
            }

            AppDbContext context = AppServices.Context.Session.GetDbContext();
            CanvasObject? selectedObject = await GetFirstSelectedCanvasObjectWithLinkedDataAsync(
                context,
                CanvasObjectAssignmentKind.Block);
            Core.Entities.Layout.Block? block = selectedObject == null
                ? null
                : await ResolveLinkedBlockAsync(context, selectedObject);
            if (block == null)
            {
                SetCanvasCommandStatus("Selected object has no linked block data.");
                return;
            }

            using var editor = new frmBlockDefinitionEditor(
                block,
                readOnlyMode: _isApplicationEditLocked);
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            if (_isApplicationEditLocked)
                return;

            Core.Entities.Layout.Block edited = editor.Block;
            block.BlockName = edited.BlockName;
            block.BlockCode = edited.BlockCode;
            block.BlockDepth = edited.BlockDepth;
            block.BlockLength = edited.BlockLength;
            block.BlockLandUse = edited.BlockLandUse;
            block.BlockArea = edited.BlockArea;
            block.Description = edited.Description;
            block.LastModifiedDate = DateTime.Now;
            await context.SaveChangesAsync();

            MarkProjectModifiedIfOpen();
            await RefreshVectorCanvasFeaturesAsync();
            await RefreshCurrentSelectedCanvasObjectPropertiesAsync(selectedObject!.Id);
            SetCanvasCommandStatus("Updated linked block data.");
        }

        private async Task<CanvasObject?> GetFirstSelectedCanvasObjectWithLinkedDataAsync(
            AppDbContext context,
            CanvasObjectAssignmentKind assignmentKind)
        {
            List<Guid> selectedIds = _currentSelectedCanvasObjectIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .Take(2)
                .ToList();
            if (selectedIds.Count != 1)
                return null;

            Guid selectedId = selectedIds[0];
            CanvasObject? canvasObject = await context.CanvasObjects
                .Include(item => item.BaselineParcel)
                    .ThenInclude(parcel => parcel!.LandOwner)
                .Include(item => item.BaselineParcel)
                    .ThenInclude(parcel => parcel!.CoOwners)
                        .ThenInclude(coOwner => coOwner.LandOwner)
                .Include(item => item.BaselineParcel)
                    .ThenInclude(parcel => parcel!.MalpotReference)
                .Include(item => item.Road)
                .Include(item => item.Block)
                .FirstOrDefaultAsync(item => item.Id == selectedId);
            if (canvasObject == null)
                return null;

            if (assignmentKind == CanvasObjectAssignmentKind.Parcel &&
                await ResolveLinkedBaselineParcelAsync(context, canvasObject) != null)
            {
                return canvasObject;
            }

            if (assignmentKind == CanvasObjectAssignmentKind.Road &&
                await ResolveLinkedRoadAsync(context, canvasObject) != null)
            {
                return canvasObject;
            }

            if (assignmentKind == CanvasObjectAssignmentKind.Block &&
                await ResolveLinkedBlockAsync(context, canvasObject) != null)
            {
                return canvasObject;
            }

            return null;
        }

        private static async Task<BaselineParcel?> ResolveLinkedBaselineParcelAsync(
            AppDbContext context,
            CanvasObject canvasObject)
        {
            if (canvasObject.BaselineParcel != null)
                return canvasObject.BaselineParcel;

            int? parcelId = canvasObject.BaselineParcelId ??
                            GetAssignedCadastralMetadata(ReadCadastralMetadata(canvasObject.GeometryMetadataJson))?.BaselineParcelId;
            if (parcelId.HasValue)
            {
                BaselineParcel? parcelById = await IncludeParcelPropertyDetails(context.BaselineParcels)
                    .FirstOrDefaultAsync(parcel => parcel.Id == parcelId.Value);
                if (parcelById != null)
                    return parcelById;
            }

            CadastralCanvasMetadata? assignedMetadata =
                GetAssignedCadastralMetadata(ReadCadastralMetadata(canvasObject.GeometryMetadataJson));
            if (assignedMetadata == null)
                return null;

            if (!string.IsNullOrWhiteSpace(assignedMetadata.FullUniqueParcelCode))
            {
                BaselineParcel? parcelByCode = await IncludeParcelPropertyDetails(context.BaselineParcels)
                    .FirstOrDefaultAsync(parcel => parcel.FullUniqueParcelCode == assignedMetadata.FullUniqueParcelCode);
                if (parcelByCode != null)
                    return parcelByCode;
            }

            if (string.IsNullOrWhiteSpace(assignedMetadata.MapSheetNo) ||
                string.IsNullOrWhiteSpace(assignedMetadata.ParcelNo))
            {
                return null;
            }

            string lookupCode = BuildParcelLookupCode(assignedMetadata.MapSheetNo, assignedMetadata.ParcelNo);
            return await IncludeParcelPropertyDetails(context.BaselineParcels)
                .FirstOrDefaultAsync(parcel => parcel.FullUniqueParcelCode == lookupCode);
        }

        private static async Task<Core.Entities.Layout.Road?> ResolveLinkedRoadAsync(
            AppDbContext context,
            CanvasObject canvasObject)
        {
            if (canvasObject.Road != null)
                return canvasObject.Road;

            if (canvasObject.RoadId.HasValue)
                return await context.Roads.FirstOrDefaultAsync(road => road.Id == canvasObject.RoadId.Value);

            return await context.Roads.FirstOrDefaultAsync(road => road.CanvasObjectId == canvasObject.Id);
        }

        private static async Task<Core.Entities.Layout.Block?> ResolveLinkedBlockAsync(
            AppDbContext context,
            CanvasObject canvasObject)
        {
            if (canvasObject.Block != null)
                return canvasObject.Block;

            if (canvasObject.BlockId.HasValue)
                return await context.Blocks.FirstOrDefaultAsync(block => block.Id == canvasObject.BlockId.Value);

            return await context.Blocks.FirstOrDefaultAsync(block => block.CanvasObjectId == canvasObject.Id);
        }

        private static Land_Readjustment_Tool.Models.BaselineLandParcelRecord ConvertBaselineParcelToEditableRecord(
            BaselineParcel parcel)
        {
            return new Land_Readjustment_Tool.Models.BaselineLandParcelRecord
            {
                ParcelNo = parcel.ParcelNo,
                MapSheetNo = parcel.MapSheetNo,
                Province = parcel.Province,
                District = parcel.District,
                MunicipalityVillage = parcel.Municipality,
                WardNo = parcel.WardNo,
                LandOwnersName = parcel.LandOwner?.FullName,
                FatherSpouse = parcel.LandOwner?.FatherOrSpouseName,
                Gender = parcel.LandOwner?.Gender,
                CitizenshipNumber = parcel.LandOwner?.CitizenshipNumber,
                CitizenshipIssuedDistrict = parcel.LandOwner?.CitizenshipIssueDistrict,
                CitizenshipIssuedDate = parcel.LandOwner?.CitizenshipIssueDate,
                PermanentAddress = parcel.LandOwner?.PermanentAddress,
                TemporaryAddress = parcel.LandOwner?.TemporaryAddress,
                ContactNumber = parcel.LandOwner?.ContactNumber,
                EmailID = parcel.LandOwner?.Email,
                Tenant = parcel.HasTenant ? "Yes" : "No",
                TenantName = parcel.TenantName,
                LandUse = parcel.LandUse,
                LandOwnershipType = parcel.LandOwnershipType,
                AreaInSqm = parcel.OriginalAreaSqm,
                FieldMeasuredAreaSqm = parcel.FieldMeasuredAreaSqm,
                MothNo = parcel.MalpotReference?.MothNo,
                PaanaNo = parcel.MalpotReference?.PaanaNo,
                Remarks = parcel.Remarks,
                JointCoOwners = parcel.CoOwners
                    .OrderBy(coOwner => coOwner.LandOwner.FullName)
                    .Select(coOwner => new Land_Readjustment_Tool.Models.CoOwnerRecord
                    {
                        OwnerName = coOwner.LandOwner.FullName,
                        FatherSpouse = coOwner.LandOwner.FatherOrSpouseName,
                        Gender = coOwner.LandOwner.Gender,
                        CitizenshipNumber = coOwner.LandOwner.CitizenshipNumber,
                        CitizenshipIssuedDistrict = coOwner.LandOwner.CitizenshipIssueDistrict,
                        CitizenshipIssuedDate = coOwner.LandOwner.CitizenshipIssueDate,
                        PermanentAddress = coOwner.LandOwner.PermanentAddress,
                        TemporaryAddress = coOwner.LandOwner.TemporaryAddress,
                        ContactNumber = coOwner.LandOwner.ContactNumber,
                        EmailID = coOwner.LandOwner.Email,
                        OwnershipSharePercent = coOwner.OwnershipSharePercent
                    })
                    .ToList()
            };
        }

        private static async Task ClearDeletedBaselineParcelLinksAsync(
            AppDbContext context,
            int parcelId)
        {
            List<CanvasObject> linkedObjects = await context.CanvasObjects
                .Where(item => item.BaselineParcelId == parcelId)
                .ToListAsync();

            foreach (CanvasObject canvasObject in linkedObjects)
            {
                canvasObject.BaselineParcelId = null;
                canvasObject.LastModifiedDate = DateTime.Now;
            }

            if (linkedObjects.Count > 0)
                await context.SaveChangesAsync();
        }

        private void ShowAssignmentFormPendingMessage(string title)
        {
            MessageBox.Show(
                this,
                "This assignment form will be implemented later.",
                title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MapCanvasControlMain_SelectedObjectsCreateFeaturesMenuOpening(
            object? sender,
            CanvasCreateFeaturesMenuOpeningEventArgs e)
        {
            e.MenuItem.DropDownItems.Clear();

            if (_isApplicationEditLocked)
            {
                e.MenuItem.DropDownItems.Add(new ToolStripMenuItem("Project Edit Lock is active")
                {
                    Enabled = false
                });
                e.MenuItem.Enabled = false;
                return;
            }

            IReadOnlyList<FeatureTransferTarget> targetLayers =
                GetDefaultTransferTargetLayersForSelectedDrawingObjects(e.SelectedObjectIds);
            if (targetLayers.Count == 0)
            {
                e.MenuItem.DropDownItems.Add(new ToolStripMenuItem("No compatible feature layers")
                {
                    Enabled = false
                });
                return;
            }

            AddFeatureTargetMenuItems(
                e.MenuItem.DropDownItems,
                targetLayers,
                targetLayer => CreateFeaturesFromSelectedObjectsAsync(e.SelectedObjectIds, targetLayer));
        }

        private IReadOnlyList<FeatureTransferTarget> GetDefaultTransferTargetLayersForSelectedDrawingObjects(
            IReadOnlyList<Guid> selectedObjectIds)
        {
            HashSet<Guid> selectedIds = selectedObjectIds
                .Where(id => id != Guid.Empty)
                .ToHashSet();
            List<CanvasObject> sourceObjects = _currentPropertyGridObjects
                .Where(item => selectedIds.Contains(item.Id) &&
                               item.CanvasLayer != null &&
                               (CanvasLayerTreeService.IsDrawingMarkupLayer(item.CanvasLayer) ||
                                CanvasLayerTreeService.IsExternalImportedLayer(item.CanvasLayer)))
                .ToList();
            List<CanvasLayer> sourceLayers = sourceObjects
                .Select(item => item.CanvasLayer!)
                .GroupBy(layer => layer.Id)
                .Select(group => group.First())
                .ToList();
            List<FeatureTransferGeometryKind> sourceKinds = sourceObjects
                .Select(GetTransferGeometryKind)
                .Where(kind => kind != FeatureTransferGeometryKind.None)
                .Distinct()
                .ToList();
            if (sourceObjects.Count == 0 || sourceKinds.Count == 0)
                return [];

            IEnumerable<FeatureTransferTarget> targets = GetDefaultFeatureTargetLayers();
            targets = targets.Where(target =>
                sourceKinds.Any(sourceKind => AreTransferGeometryKindsCompatible(
                    sourceKind,
                    GetTransferGeometryKind(target.Layer))));

            if (sourceLayers.Any(CanvasLayerTreeService.IsExternalImportedLayer))
            {
                HashSet<int> yieldedIds = targets
                    .Select(target => target.Layer.Id)
                    .ToHashSet();

                targets = targets.Concat(GetDrawingMarkupLayerTransferTargets()
                    .Where(target =>
                        yieldedIds.Add(target.Layer.Id) &&
                        sourceKinds.Any(sourceKind => AreTransferGeometryKindsCompatible(
                            sourceKind,
                            GetTransferGeometryKind(target.Layer)))));
            }

            return targets.ToList();
        }

        private async Task RefreshLayerTreeAsync(
            bool rebuildRasterLayersAfterCrsChange = false,
            bool refreshVectorFeatures = true)
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
                if (refreshVectorFeatures)
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
            using var redrawScope = SuspendRedraw(leftSplitContainer.Panel1, grpLayer, treeViewLayers);

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
            if (string.Equals(group.Key, ExternalLayerGroupKey, StringComparison.OrdinalIgnoreCase))
            {
                int externalFileIndex = 0;
                foreach (IGrouping<string, CanvasLayer> sourceGroup in group.Layers
                    .GroupBy(GetExternalLayerSourceGroupKey, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => GetExternalLayerSourceDisplayName(item.Key)))
                {
                    TreeNode sourceNode = CreateExternalFileGroupNode(
                        sourceGroup.Key,
                        externalFileIndex++);

                    foreach (CanvasLayer layer in sourceGroup
                        .OrderBy(item => item.DisplayOrder)
                        .ThenBy(item => item.Name))
                    {
                        sourceNode.Nodes.Add(CreateLayerNode(layer));
                    }

                    groupNode.Nodes.Add(sourceNode);
                    sourceNode.Expand();
                }

                return;
            }

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

        private static string GetExternalLayerSourceGroupKey(CanvasLayer layer)
        {
            return string.IsNullOrWhiteSpace(layer.SourceFile)
                ? "External import"
                : layer.SourceFile;
        }

        private static string GetExternalLayerSourceDisplayName(string sourceFile)
        {
            string fileName = Path.GetFileName(sourceFile);
            return string.IsNullOrWhiteSpace(fileName)
                ? sourceFile
                : fileName;
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
            using var redrawScope = SuspendRedraw(leftSplitContainer.Panel1, grpLayer, treeViewLayers);

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

        private static string? GetLayerGroupKeyForNode(TreeNode? node)
        {
            for (TreeNode? current = node; current != null; current = current.Parent)
            {
                if (current.Tag is LayerTreeNodeState state &&
                    !state.IsLayerNode &&
                    !string.IsNullOrWhiteSpace(state.GroupKey))
                {
                    return state.GroupKey;
                }
            }

            return null;
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

        private static bool IsExternalLayerGroupNode(TreeNode? node)
        {
            return string.Equals(
                node?.Name,
                $"{LayerGroupNodeNamePrefix}{ExternalLayerGroupKey}",
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsExternalFileGroupNode(TreeNode? node)
        {
            return node?.Name.StartsWith(
                $"{LayerGroupNodeNamePrefix}{ExternalLayerGroupKey}_File_",
                StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool IsRePlotDataLayerGroupNode(TreeNode? node)
        {
            string? groupKey = GetLayerGroupKeyForGroupNode(node);
            if (string.IsNullOrWhiteSpace(groupKey) ||
                string.Equals(groupKey, RePlotRootNodeKey, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return CanvasLayerTreeService.IsRePlotDataGroupKey(groupKey) ||
                   string.Equals(groupKey, CadastralMapGroupKey, StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetLayerGroupKeyForGroupNode(TreeNode? node)
        {
            return node?.Tag is LayerTreeNodeState state && !state.IsLayerNode
                ? state.GroupKey
                : null;
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

        private static List<CanvasLayer> GetLayersFromGroupNode(TreeNode? groupNode)
        {
            if (groupNode == null)
                return [];

            return EnumerateLayerNodes(groupNode)
                .Select(GetLayerFromNode)
                .Where(layer => layer != null)
                .Select(layer => layer!)
                .ToList();
        }

        private static List<CanvasLayer> GetSelectableVectorLayersFromGroupNode(TreeNode? groupNode)
        {
            return GetLayersFromGroupNode(groupNode)
                .Where(layer => !IsRasterLayer(layer))
                .ToList();
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

        private Rectangle GetLayerNodeTextHighlightRect(Graphics graphics, string text, Rectangle textRect)
        {
            Size measured = TextRenderer.MeasureText(
                graphics,
                text,
                treeViewLayers.Font,
                new Size(Math.Max(1, textRect.Width), textRect.Height),
                TextFormatFlags.Left |
                TextFormatFlags.SingleLine |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix);

            int width = Math.Min(textRect.Width, Math.Max(8, measured.Width));
            return new Rectangle(
                textRect.X,
                textRect.Y + 2,
                width,
                Math.Max(1, textRect.Height - 4));
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
            if (!EnsureApplicationUnlockedForEditing("editing layer properties"))
                return;

            if (node == null ||
                node.Tag is not LayerTreeNodeState nodeState ||
                !nodeState.IsLayerNode ||
                nodeState.Layer == null)
            {
                return;
            }

            CanvasLayer beforeSnapshot = CloneCanvasLayerSnapshot(nodeState.Layer);
            CanvasLayer editableLayer =
                _layerCommandService.CreateEditableCopy(nodeState.Layer);

            var sampleRecords = await GetLayerSampleRecordsAsync(nodeState.Layer);
            bool allowRename = CanEditLayerName(node, nodeState.Layer);
            using var frm = new frmLayerPropertyManager(
                editableLayer,
                _hatchPatternService,
                sampleRecords,
                allowRename);
            PositionLayerPropertyManager(frm);

            frm.LayerApplied += async (_, _) =>
            {
                try
                {
                    CanvasLayer snapshotBeforeApply = CloneCanvasLayerSnapshot(beforeSnapshot);
                    CanvasLayer updatedLayer =
                        await _layerCommandService.UpdatePropertiesAsync(
                            AppServices.HasContext ? AppServices.Context.Session : null,
                            editableLayer);

                    RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                        [snapshotBeforeApply],
                        [CloneCanvasLayerSnapshot(updatedLayer)]));
                    beforeSnapshot = CloneCanvasLayerSnapshot(updatedLayer);

                    bool isRaster = IsRasterLayer(updatedLayer);
                    UpdateLayerNode(node, updatedLayer, updateRasterStack: false);
                    MarkProjectModifiedIfOpen();

                    using (mapCanvasControlMain.DeferRenderUpdates())
                    {
                        if (isRaster)
                        {
                            if (!mapCanvasControlMain.SetRasterLayerRenderState(
                                updatedLayer.Id,
                                updatedLayer.IsVisible,
                                updatedLayer.FillTransparency))
                            {
                                UpdateRasterCanvasLayersFromTree();
                            }

                            SetCanvasCommandStatus($"Raster layer applied: {updatedLayer.Name}");
                        }
                        else
                        {
                            mapCanvasControlMain.UpdateVectorLayer(updatedLayer);
                            await RefreshVectorCanvasFeaturesAsync();
                            await RefreshCurrentSelectedCanvasObjectPropertiesAsync();
                            RefreshCurrentDrawingLayerCombo();
                            SetCanvasCommandStatus($"Layer properties applied: {updatedLayer.Name}");
                        }

                        mapCanvasControlMain.RequestRender();
                    }

                    SelectLayerNodeById(updatedLayer.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to apply layer properties: {ex.Message}",
                        "Layer Properties",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            if (frm.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                CanvasLayer updatedLayer =
                    await _layerCommandService.UpdatePropertiesAsync(
                        AppServices.HasContext ? AppServices.Context.Session : null,
                        editableLayer);

                RegisterCanvasUndoCommand(new ModifyCanvasLayersCommand(
                    [beforeSnapshot],
                    [CloneCanvasLayerSnapshot(updatedLayer)]));
                bool isRaster = IsRasterLayer(updatedLayer);
                UpdateLayerNode(node, updatedLayer, updateRasterStack: false);
                MarkProjectModifiedIfOpen();

                using (mapCanvasControlMain.DeferRenderUpdates())
                {
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
                        await RefreshVectorCanvasFeaturesAsync();
                        await RefreshCurrentSelectedCanvasObjectPropertiesAsync();
                        // Sync the drawing-layer combo so the updated layer object
                        // (with the new color/style) replaces the stale cached item.
                        // Without this, re-activating a drawing tool passes the old
                        // layer to SetActiveTool and reverts the preview back to the
                        // previous color.
                        RefreshCurrentDrawingLayerCombo();
                        SetCanvasCommandStatus($"Layer properties updated: {updatedLayer.Name}");
                    }

                    mapCanvasControlMain.RequestRender();
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

        private async Task<IReadOnlyList<IReadOnlyDictionary<string, string>>?> GetLayerSampleRecordsAsync(
            CanvasLayer layer,
            int maxSamples = 10)
        {
            if (!AppServices.HasContext)
                return null;

            try
            {
                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<Core.Entities.Canvas.CanvasObject> objects =
                    await context.CanvasObjects
                        .AsNoTracking()
                        .Include(item => item.CanvasLayer)
                        .Include(item => item.BaselineParcel)
                            .ThenInclude(parcel => parcel!.LandOwner)
                        .Include(item => item.BaselineParcel)
                            .ThenInclude(parcel => parcel!.MalpotReference)
                        .Include(item => item.Road)
                        .Include(item => item.Block)
                        .Include(item => item.ReplottedParcel)
                            .ThenInclude(parcel => parcel!.Block)
                        .Include(item => item.ReplottedParcel)
                            .ThenInclude(parcel => parcel!.PlotType)
                        .Where(item => item.CanvasLayerId == layer.Id)
                        .ToListAsync();

                await LoadFallbackBaselineParcelsForSelectionAsync(context, objects);
                await LoadFallbackRoadsForSelectionAsync(context, objects);
                await LoadFallbackBlocksForSelectionAsync(context, objects);
                await LoadFallbackReplottedParcelsForSelectionAsync(context, objects);

                if (objects.Count == 0)
                    return null;

                var rng = new Random();
                var sampled = objects.OrderBy(_ => rng.Next()).Take(maxSamples).ToList();
                var result = new List<IReadOnlyDictionary<string, string>>(sampled.Count);
                var (sampleSqmPrec, sampleTradPrec) = GetAreaPrecisionSettingsStatic();

                foreach (var obj in sampled)
                {
                    Core.Models.Import.CadastralCanvasMetadata? meta = null;
                    if (!string.IsNullOrWhiteSpace(obj.GeometryMetadataJson))
                    {
                        try
                        {
                            meta = JsonSerializer.Deserialize<Core.Models.Import.CadastralCanvasMetadata>(
                                obj.GeometryMetadataJson,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                        catch { /* ignore */ }
                    }

                    Core.Models.Import.CadastralCanvasMetadata? assignedMeta = GetAssignedCadastralMetadata(meta);
                    double? geometryAreaSqm = CanvasGeometryMetricsService.GetArea(obj);
                    double? calculatedAreaSqm = meta != null && meta.CalculatedAreaSqm > 0
                        ? meta.CalculatedAreaSqm
                        : geometryAreaSqm;

                    double? areaSqm = assignedMeta?.RecordAreaSqm ?? calculatedAreaSqm;

                    // Geometry-computed fields (safe — Shape may be null if NTS not loaded)
                    string geoLength = string.Empty;
                    string geoPerimeter = string.Empty;
                    string geoX = string.Empty;
                    string geoY = string.Empty;
                    try
                    {
                        if (obj.Shape != null)
                        {
                            string ot = (obj.ObjectType ?? string.Empty).ToLowerInvariant();
                            if (ot is "polyline" or "line" or "arc")
                                geoLength = obj.Shape.Length.ToString("F2", CultureInfo.InvariantCulture);
                            if (ot is "polygon" or "circle")
                                geoPerimeter = obj.Shape.Length.ToString("F2", CultureInfo.InvariantCulture);
                            if (ot == "point")
                            {
                                geoX = obj.Shape.Coordinate?.X.ToString("F4", CultureInfo.InvariantCulture) ?? string.Empty;
                                geoY = obj.Shape.Coordinate?.Y.ToString("F4", CultureInfo.InvariantCulture) ?? string.Empty;
                            }
                        }
                    }
                    catch { /* Shape access failed — leave empty */ }

                    // Resolve data-linked fields from loaded navigation properties.
                    var bp = obj.BaselineParcel;
                    var owner = bp?.LandOwner;
                    var road = obj.Road;
                    var block = obj.Block;
                    double? blockDepthFromGeometry = IsBlockObject(obj)
                        ? CanvasGeometryMetricsService.GetBlockDepthFromGeometry(obj)
                        : null;
                    var plot = obj.ReplottedParcel;

                    // Record area: prefer entity value → metadata → null
                    double? recordAreaSqm = bp != null
                        ? (double?)bp.OriginalAreaSqm
                        : assignedMeta?.RecordAreaSqm;

                    string FormatSqmSample(double? v) =>
                        v.HasValue && v.Value > 0
                            ? v.Value.ToString($"F{sampleSqmPrec}", CultureInfo.InvariantCulture)
                            : string.Empty;
                    string FormatRAPD(double? v) =>
                        v.HasValue && v.Value > 0
                            ? Services.AreaConverterService.SqmToRAPDString(v.Value, sampleTradPrec)
                            : string.Empty;
                    string FormatBKD(double? v) =>
                        v.HasValue && v.Value > 0
                            ? Services.AreaConverterService.SqmToBKDString(v.Value, sampleTradPrec)
                            : string.Empty;
                    string FormatNumberSample(double? v) =>
                        v.HasValue && v.Value > 0
                            ? v.Value.ToString("0.##", CultureInfo.InvariantCulture)
                            : string.Empty;
                    string Attr(params string[] names) => GetCanvasMetadataAttributeValue(meta, names);
                    string First(params string?[] values) => values
                        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
                    string ResolveReplottedParcelNumber()
                    {
                        if (plot == null)
                            return string.Empty;

                        return plot.ActiveNumberType switch
                        {
                            "Derived" => plot.DerivedNumber ?? string.Empty,
                            "BlockSequence" => plot.BlockSequenceNumber ?? string.Empty,
                            _ => plot.SystemGeneratedNumber ?? string.Empty
                        };
                    }

                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        // ── Parcel identification ──────────────────────────────────
                        ["ParcelNo"] = bp?.ParcelNo ?? assignedMeta?.ParcelNo ?? string.Empty,
                        ["MapSheetNo"] = bp?.MapSheetNo ?? assignedMeta?.MapSheetNo ?? string.Empty,
                        ["FullUniqueParcelCode"] = bp?.FullUniqueParcelCode ?? assignedMeta?.FullUniqueParcelCode ?? string.Empty,
                        // ── Owner ──────────────────────────────────────────────────
                        ["OwnerName"] = owner?.FullName ?? assignedMeta?.OwnerName ?? string.Empty,
                        ["OwnerFatherSpouse"] = owner?.FatherOrSpouseName ?? string.Empty,
                        ["OwnershipType"] = bp?.LandOwnershipType ?? string.Empty,
                        ["HasTenant"] = bp != null ? (bp.HasTenant ? "Yes" : "No") : string.Empty,
                        ["TenantName"] = bp?.TenantName ?? string.Empty,
                        // ── Area — from records ────────────────────────────────────
                        ["AreaSqm"] = FormatSqmSample(recordAreaSqm ?? areaSqm),
                        ["AreaRAPD"] = FormatRAPD(recordAreaSqm ?? areaSqm),
                        ["AreaBKD"] = FormatBKD(recordAreaSqm ?? areaSqm),
                        ["FieldMeasuredAreaSqm"] = FormatSqmSample(bp?.FieldMeasuredAreaSqm),
                        ["EffectiveAreaSqm"] = FormatSqmSample(bp?.EffectiveAreaSqm),
                        // ── Area — from map ────────────────────────────────────────
                        ["CalculatedAreaSqm"] = FormatSqmSample(calculatedAreaSqm),
                        // ── Location ───────────────────────────────────────────────
                        ["Province"] = bp?.Province ?? string.Empty,
                        ["District"] = bp?.District ?? string.Empty,
                        ["Municipality"] = bp?.Municipality ?? string.Empty,
                        ["WardNo"] = bp?.WardNo ?? string.Empty,
                        ["LandUse"] = bp?.LandUse ?? assignedMeta?.LandUse ?? string.Empty,
                        ["MothNo"] = bp?.MalpotReference?.MothNo ?? string.Empty,
                        ["PaanaNo"] = bp?.MalpotReference?.PaanaNo ?? string.Empty,
                        // ── Status ─────────────────────────────────────────────────
                        ["AssignmentStatus"] = meta?.AssignmentStatus ?? string.Empty,
                        ["RoadName"] = First(road?.RoadName, Attr("RoadName", "Road Name", "Name"), obj.LabelText, meta?.MatchedText),
                        ["RoadCode"] = First(road?.RoadCode, Attr("RoadCode", "Road Code", "Code")),
                        ["RoadStatus"] = First(road?.RoadStatus, Attr("RoadStatus", "Road Status", "Status")),
                        ["RoadType"] = First(road?.RoadType, Attr("RoadType", "Road Type", "Type")),
                        ["SurfaceType"] = First(road?.SurfaceType, Attr("SurfaceType", "Surface Type", "Surface")),
                        ["RoadWidth"] = First(road != null ? FormatNumberSample(road.RoadWidth) : string.Empty, Attr("RoadWidth", "Road Width", "Width")),
                        ["RightOfWayWidth"] = First(FormatNumberSample(road?.RightOfWayWidth), Attr("RightOfWayWidth", "Right Of Way Width", "ROWWidth", "ROW Width")),
                        ["RoadDescription"] = First(road?.Description, Attr("RoadDescription", "Road Description", "Description")),
                        ["BlockName"] = First(block?.BlockName, Attr("BlockName", "Block Name", "Name"), obj.LabelText, meta?.MatchedText),
                        ["BlockCode"] = First(block?.BlockCode, Attr("BlockCode", "Block Code", "Code")),
                        ["BlockLandUse"] = First(block?.BlockLandUse, Attr("BlockLandUse", "Block Land Use", "LandUse", "Land Use", "Type")),
                        ["BlockDepth"] = First(block != null ? FormatNumberSample(block.BlockDepth) : string.Empty, Attr("BlockDepth", "Block Depth", "Depth")),
                        ["BlockDepthGeometry"] = FormatNumberSample(blockDepthFromGeometry),
                        ["BlockAreaSqm"] = First(FormatSqmSample(geometryAreaSqm ?? block?.BlockArea), Attr("BlockAreaSqm", "Block Area Sqm", "BlockArea", "Area")),
                        ["BlockAreaRAPD"] = FormatRAPD(geometryAreaSqm ?? block?.BlockArea),
                        ["BlockAreaBKD"] = FormatBKD(geometryAreaSqm ?? block?.BlockArea),
                        ["BlockDescription"] = First(block?.Description, Attr("BlockDescription", "Block Description", "Description")),
                        ["ReplottedParcelNo"] = First(ResolveReplottedParcelNumber(), Attr("ReplottedParcelNo", "PlotNo", "Plot No", "ParcelNo"), obj.LabelText),
                        ["SystemGeneratedNumber"] = First(plot?.SystemGeneratedNumber, Attr("SystemGeneratedNumber", "System Generated Number")),
                        ["DerivedNumber"] = First(plot?.DerivedNumber, Attr("DerivedNumber", "Derived Number")),
                        ["BlockSequenceNumber"] = First(plot?.BlockSequenceNumber, Attr("BlockSequenceNumber", "Block Sequence Number")),
                        ["PlotTypeName"] = First(plot?.PlotType?.TypeName, Attr("PlotTypeName", "Plot Type", "PlotType", "Type")),
                        ["PlotBlockName"] = First(plot?.Block?.BlockName, Attr("PlotBlockName", "Plot Block Name", "BlockName", "Block Name")),
                        ["PlotAreaSqm"] = First(plot != null ? FormatSqmSample(plot.PlotAreaSqm) : string.Empty, Attr("PlotAreaSqm", "Plot Area Sqm", "PlotArea", "Area")),
                        ["PlotAreaRAPD"] = plot != null ? FormatRAPD(plot.PlotAreaSqm) : string.Empty,
                        ["PlotAreaBKD"] = plot != null ? FormatBKD(plot.PlotAreaSqm) : string.Empty,
                        ["PlotNotes"] = First(plot?.Notes, Attr("PlotNotes", "Notes")),
                        // ── Object / layer ─────────────────────────────────────────
                        ["LabelText"] = obj.LabelText ?? string.Empty,
                        ["ObjectDescription"] = obj.ObjectDescription ?? string.Empty,
                        ["ObjectType"] = obj.ObjectType ?? string.Empty,
                        ["LayerName"] = layer.Name ?? string.Empty,
                        ["SourceLayer"] = meta?.SourceLayer ?? string.Empty,
                        ["SourceFileName"] = meta?.SourceFileName ?? string.Empty,
                        ["SourceFormat"] = meta?.SourceFormat ?? string.Empty,
                        ["MatchedText"] = meta?.MatchedText ?? string.Empty,
                        ["Id"] = obj.Id.ToString(),
                        ["BaselineParcelId"] = (obj.BaselineParcelId ?? assignedMeta?.BaselineParcelId)?.ToString()
                            ?? string.Empty,
                        // ── Geometry ───────────────────────────────────────────────
                        ["Length"] = geoLength,
                        ["Perimeter"] = geoPerimeter,
                        ["X"] = geoX,
                        ["Y"] = geoY,
                    };

                    foreach (var attribute in GetCanvasMetadataAttributes(meta))
                    {
                        if (!dict.ContainsKey(attribute.Key) &&
                            !string.IsNullOrWhiteSpace(attribute.Value))
                        {
                            dict[attribute.Key] = attribute.Value;
                        }
                    }

                    result.Add(dict);
                }

                return result.Count > 0 ? result : null;
            }
            catch
            {
                return null;
            }
        }

        private static IReadOnlyDictionary<string, string> GetCanvasMetadataAttributes(
            CadastralCanvasMetadata? metadata)
        {
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.AttributesJson))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using JsonDocument document = JsonDocument.Parse(metadata.AttributesJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                Dictionary<string, string> attributes = new(StringComparer.OrdinalIgnoreCase);
                foreach (JsonProperty property in document.RootElement.EnumerateObject())
                {
                    string value = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                        JsonValueKind.Number => property.Value.ToString(),
                        JsonValueKind.True => "True",
                        JsonValueKind.False => "False",
                        _ => property.Value.ToString()
                    };

                    if (!string.IsNullOrWhiteSpace(value))
                        attributes[property.Name] = value.Trim();
                }

                return attributes;
            }
            catch
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static string GetCanvasMetadataAttributeValue(
            CadastralCanvasMetadata? metadata,
            params string[] names)
        {
            if (names.Length == 0)
                return string.Empty;

            IReadOnlyDictionary<string, string> attributes = GetCanvasMetadataAttributes(metadata);
            if (attributes.Count == 0)
                return string.Empty;

            foreach (string name in names)
            {
                if (attributes.TryGetValue(name, out string? direct) &&
                    !string.IsNullOrWhiteSpace(direct))
                {
                    return direct;
                }

                string normalizedName = NormalizeCanvasSampleFieldName(name);
                foreach (var attribute in attributes)
                {
                    if (string.Equals(
                            NormalizeCanvasSampleFieldName(attribute.Key),
                            normalizedName,
                            StringComparison.OrdinalIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(attribute.Value))
                    {
                        return attribute.Value;
                    }
                }
            }

            return string.Empty;
        }

        private static string NormalizeCanvasSampleFieldName(string value) =>
            value
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("+", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToLowerInvariant();

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

            cboCurrentDrawingLayer.ComboBox.Invalidate();
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
        private void SetOperationProgress(int percent, string status, bool showProgressForm = false)
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
                _projectCrsStatus.Width +
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
            lblOperationProgressStatus.Visible = false;
            lblOperationProgressStatus.AutoSize = false;
            lblOperationProgressStatus.Width = CalculateOperationStatusWidth();
            lblOperationProgressStatus.TextAlign = ContentAlignment.MiddleRight;
            hostOperationProgress.Value = 0;
            hostOperationProgress.Visible = false;
            hostOperationProgress.Invalidate();
            hostProgressBarHost.Size = new Size(154, 26);
            hostProgressBarHost.Visible = false;
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
            PlaceCoordinateStatusItemsLeftOfCoordinates();

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
        /// Describes a persisted canvas edit that can undo and redo itself.
        /// </summary>
        private interface IPersistedCanvasUndoCommand
        {
            /// <summary>
            /// Gets the human-readable operation name shown in toolbar tooltips.
            /// </summary>
            string Description { get; }

            /// <summary>
            /// Reverses the command against the active project database and canvas.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            Task UndoAsync(frmMain owner);

            /// <summary>
            /// Reapplies the command against the active project database and canvas.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            Task RedoAsync(frmMain owner);
        }

        /// <summary>
        /// Maintains undo and redo stacks for persisted map canvas edits.
        /// </summary>
        private sealed class PersistedCanvasUndoRedoManager
        {
            private const int MaxUndoLevels = 100;
            private readonly LinkedList<IPersistedCanvasUndoCommand> _undoStack = new();
            private readonly LinkedList<IPersistedCanvasUndoCommand> _redoStack = new();

            /// <summary>
            /// Raised whenever stack availability or descriptions change.
            /// </summary>
            public event EventHandler? StateChanged;

            /// <summary>
            /// Gets whether an undo command is available.
            /// </summary>
            public bool CanUndo => _undoStack.Count > 0;

            /// <summary>
            /// Gets whether a redo command is available.
            /// </summary>
            public bool CanRedo => _redoStack.Count > 0;

            /// <summary>
            /// Adds an already-executed command to the undo stack and clears redo history.
            /// </summary>
            /// <param name="command">The completed persisted edit to remember.</param>
            public void Register(IPersistedCanvasUndoCommand command)
            {
                _undoStack.AddLast(command);
                _redoStack.Clear();

                while (_undoStack.Count > MaxUndoLevels)
                {
                    _undoStack.RemoveFirst();
                }

                OnStateChanged();
            }

            /// <summary>
            /// Undoes the most recent canvas edit and moves it to the redo stack.
            /// </summary>
            /// <param name="owner">The main form that can perform database refresh work.</param>
            public async Task UndoAsync(frmMain owner)
            {
                if (!CanUndo)
                    return;

                IPersistedCanvasUndoCommand command = _undoStack.Last!.Value;
                _undoStack.RemoveLast();
                await command.UndoAsync(owner);
                _redoStack.AddLast(command);
                OnStateChanged();
            }

            /// <summary>
            /// Redoes the most recently undone canvas edit and moves it to the undo stack.
            /// </summary>
            /// <param name="owner">The main form that can perform database refresh work.</param>
            public async Task RedoAsync(frmMain owner)
            {
                if (!CanRedo)
                    return;

                IPersistedCanvasUndoCommand command = _redoStack.Last!.Value;
                _redoStack.RemoveLast();
                await command.RedoAsync(owner);
                _undoStack.AddLast(command);
                OnStateChanged();
            }

            /// <summary>
            /// Removes all undo and redo history.
            /// </summary>
            public void Clear()
            {
                _undoStack.Clear();
                _redoStack.Clear();
                OnStateChanged();
            }

            /// <summary>
            /// Gets the description of the next undo command.
            /// </summary>
            public string GetUndoDescription()
            {
                return CanUndo ? _undoStack.Last!.Value.Description : string.Empty;
            }

            /// <summary>
            /// Gets the description of the next redo command.
            /// </summary>
            public string GetRedoDescription()
            {
                return CanRedo ? _redoStack.Last!.Value.Description : string.Empty;
            }

            private void OnStateChanged()
            {
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Undo command for one or more persisted canvas layer property changes.
        /// </summary>
        private sealed class ModifyCanvasLayersCommand : IPersistedCanvasUndoCommand
        {
            private readonly IReadOnlyList<CanvasLayer> _beforeSnapshots;
            private readonly IReadOnlyList<CanvasLayer> _afterSnapshots;

            /// <summary>
            /// Creates a layer modification command from before/after layer states.
            /// </summary>
            /// <param name="beforeSnapshots">Layer states to restore on undo.</param>
            /// <param name="afterSnapshots">Layer states to restore on redo.</param>
            public ModifyCanvasLayersCommand(
                IReadOnlyList<CanvasLayer> beforeSnapshots,
                IReadOnlyList<CanvasLayer> afterSnapshots)
            {
                _beforeSnapshots = beforeSnapshots;
                _afterSnapshots = afterSnapshots;
            }

            /// <summary>
            /// Gets the operation name shown in undo/redo tooltips.
            /// </summary>
            public string Description => _afterSnapshots.Count == 1
                ? $"Modify layer {_afterSnapshots[0].Name}"
                : $"Modify {_afterSnapshots.Count} layers";

            /// <summary>
            /// Restores the previous layer state.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task UndoAsync(frmMain owner)
            {
                return owner.RestoreCanvasLayerSnapshotsAsync(_beforeSnapshots);
            }

            /// <summary>
            /// Restores the changed layer state.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task RedoAsync(frmMain owner)
            {
                return owner.RestoreCanvasLayerSnapshotsAsync(_afterSnapshots);
            }
        }

        /// <summary>
        /// Undo command for one or more newly created persisted canvas objects.
        /// </summary>
        private sealed class AddCanvasObjectsCommand : IPersistedCanvasUndoCommand
        {
            private readonly IReadOnlyList<CanvasObject> _snapshots;

            /// <summary>
            /// Creates an add command from the created canvas object snapshots.
            /// </summary>
            /// <param name="snapshots">The created objects to remove on undo and restore on redo.</param>
            public AddCanvasObjectsCommand(IReadOnlyList<CanvasObject> snapshots)
            {
                _snapshots = snapshots;
            }

            /// <summary>
            /// Gets the operation name shown in undo/redo tooltips.
            /// </summary>
            public string Description => _snapshots.Count == 1
                ? $"Add {_snapshots[0].ObjectType}"
                : $"Add {_snapshots.Count} canvas objects";

            /// <summary>
            /// Deletes the added objects from the active project.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task UndoAsync(frmMain owner)
            {
                return owner.DeleteCanvasObjectsForUndoRedoAsync(_snapshots.Select(item => item.Id).ToArray());
            }

            /// <summary>
            /// Restores the added objects to the active project.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task RedoAsync(frmMain owner)
            {
                return owner.RestoreCanvasObjectSnapshotsAsync(_snapshots);
            }
        }

        /// <summary>
        /// Undo command for a persisted canvas geometry/style modification.
        /// </summary>
        private sealed class ModifyCanvasObjectCommand : IPersistedCanvasUndoCommand
        {
            private readonly CanvasObject _beforeSnapshot;
            private readonly CanvasObject _afterSnapshot;

            /// <summary>
            /// Creates a modify command from the object state before and after the edit.
            /// </summary>
            /// <param name="beforeSnapshot">Object state to restore on undo.</param>
            /// <param name="afterSnapshot">Object state to restore on redo.</param>
            public ModifyCanvasObjectCommand(CanvasObject beforeSnapshot, CanvasObject afterSnapshot)
            {
                _beforeSnapshot = beforeSnapshot;
                _afterSnapshot = afterSnapshot;
            }

            /// <summary>
            /// Gets the operation name shown in undo/redo tooltips.
            /// </summary>
            public string Description => $"Modify {_afterSnapshot.ObjectType}";

            /// <summary>
            /// Restores the geometry before the edit.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task UndoAsync(frmMain owner)
            {
                return owner.RestoreCanvasObjectSnapshotsAsync([_beforeSnapshot]);
            }

            /// <summary>
            /// Restores the geometry after the edit.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task RedoAsync(frmMain owner)
            {
                return owner.RestoreCanvasObjectSnapshotsAsync([_afterSnapshot]);
            }
        }

        /// <summary>
        /// Undo command for a batch of edited canvas objects (e.g. moving or grip
        /// editing several shapes at once) — restored/redone in a single pass.
        /// </summary>
        private sealed class ModifyCanvasObjectsCommand : IPersistedCanvasUndoCommand
        {
            private readonly IReadOnlyList<CanvasObject> _beforeSnapshots;
            private readonly IReadOnlyList<CanvasObject> _afterSnapshots;

            public ModifyCanvasObjectsCommand(
                IReadOnlyList<CanvasObject> beforeSnapshots,
                IReadOnlyList<CanvasObject> afterSnapshots)
            {
                _beforeSnapshots = beforeSnapshots;
                _afterSnapshots = afterSnapshots;
            }

            public string Description => _afterSnapshots.Count == 1
                ? $"Modify {_afterSnapshots[0].ObjectType}"
                : $"Modify {_afterSnapshots.Count} objects";

            public Task UndoAsync(frmMain owner)
            {
                return owner.RestoreCanvasObjectSnapshotsAsync(_beforeSnapshots);
            }

            public Task RedoAsync(frmMain owner)
            {
                return owner.RestoreCanvasObjectSnapshotsAsync(_afterSnapshots);
            }
        }

        /// <summary>
        /// Undo command for one or more deleted persisted canvas objects.
        /// </summary>
        private sealed class DeleteCanvasObjectsCommand : IPersistedCanvasUndoCommand
        {
            private readonly IReadOnlyList<CanvasObject> _snapshots;

            /// <summary>
            /// Creates a delete command from snapshots captured before deletion.
            /// </summary>
            /// <param name="snapshots">The deleted objects to restore on undo and delete again on redo.</param>
            public DeleteCanvasObjectsCommand(IReadOnlyList<CanvasObject> snapshots)
            {
                _snapshots = snapshots;
            }

            /// <summary>
            /// Gets the operation name shown in undo/redo tooltips.
            /// </summary>
            public string Description => _snapshots.Count == 1
                ? "Delete selected drawing object"
                : $"Delete {_snapshots.Count} selected drawing objects";

            /// <summary>
            /// Restores the deleted objects to the active project.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task UndoAsync(frmMain owner)
            {
                return owner.RestoreCanvasObjectSnapshotsAsync(_snapshots);
            }

            /// <summary>
            /// Deletes the restored objects again from the active project.
            /// </summary>
            /// <param name="owner">The main form that owns the active project services.</param>
            public Task RedoAsync(frmMain owner)
            {
                return owner.DeleteCanvasObjectsForUndoRedoAsync(_snapshots.Select(item => item.Id).ToArray());
            }
        }

        private static bool ShouldClearLayerObjectsInsteadOfDeletingLayer(CanvasLayer layer)
        {
            return (CanvasLayerTreeService.IsProtectedDefaultLayer(layer) ||
                    CanvasLayerTreeService.IsRePlotDataLayer(layer)) &&
                   !CanvasLayerTreeService.IsDrawingMarkupLayer(layer);
        }

        private async Task DeleteObjectsFromDefaultLayerAsync(CanvasLayer layer)
        {
            DialogResult result = MessageBox.Show(
                $"Delete all objects in layer '{layer.Name}'?\n\nThe layer node will remain in the layer tree.",
                "Delete Layer Objects",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            await DeleteObjectsFromLayersAsync([layer], $"Deleted all objects from layer '{layer.Name}'");
        }

        private async Task DeleteObjectsFromDefaultLayerGroupAsync(TreeNode groupNode, IReadOnlyList<CanvasLayer> layers)
        {
            DialogResult result = MessageBox.Show(
                $"Delete all objects in layer group '{groupNode.Text}'?\n\nThe layer nodes will remain in the layer tree.",
                "Delete Layer Objects",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
                return;

            await DeleteObjectsFromLayersAsync(layers, $"Deleted all objects from layer group '{groupNode.Text}'");
        }

        private async Task DeleteObjectsFromLayersAsync(IReadOnlyList<CanvasLayer> layers, string successStatus)
        {
            if (layers.Count == 0)
                return;

            if (!AppServices.HasContext)
            {
                await RefreshMapCanvasAsync("Refreshing map canvas");
                return;
            }

            try
            {
                int[] layerIds = layers
                    .Where(layer => layer.Id > 0)
                    .Select(layer => layer.Id)
                    .Distinct()
                    .ToArray();
                if (layerIds.Length == 0)
                    return;

                AppDbContext context = AppServices.Context.Session.GetDbContext();
                List<Guid> objectIds = await context.CanvasObjects
                    .AsNoTracking()
                    .Where(item => layerIds.Contains(item.CanvasLayerId))
                    .Select(item => item.Id)
                    .ToListAsync();

                if (objectIds.Count == 0)
                {
                    MessageBox.Show(
                        "There are no objects to delete in the selected layer.",
                        "Delete Layer Objects",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                CanvasFeatureService featureService =
                    _projectScopedFactory.CreateCanvasFeatureService(AppServices.Context.Session);
                foreach (Guid objectId in objectIds.Distinct())
                {
                    await featureService.DeleteShapeAsync(objectId);
                }

                mapCanvasControlMain.ClearSelectionAfterDelete();
                MarkProjectModifiedIfOpen();
                await RefreshMapCanvasAsync("Refreshing map canvas");
                await RefreshCurrentSelectedCanvasObjectPropertiesAsync();
                SetCanvasCommandStatus(successStatus);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete layer objects: {ex.Message}",
                    "Delete Layer Objects",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private enum DuplicateFeatureGeometryChoice
        {
            Replace,
            Skip,
            Cancel
        }

        private sealed record DuplicateFeatureGeometryResolution(
            List<CanvasObject> ObjectsToCreate,
            List<CanvasObject> ObjectsToReplace);

        private sealed record ProjectBoundaryFeatureResolution(
            List<CanvasObject> ObjectsToCreate,
            List<CanvasObject> ObjectsToReplace,
            bool SkippedForExistingBoundary);

        /// <summary>
        /// Undo command for converting drawing objects into feature objects while removing originals.
        /// </summary>
        private sealed class CreateFeaturesFromCanvasObjectsCommand : IPersistedCanvasUndoCommand
        {
            private readonly IReadOnlyList<CanvasObject> _createdSnapshots;
            private readonly IReadOnlyList<CanvasObject> _deletedSourceSnapshots;

            public CreateFeaturesFromCanvasObjectsCommand(
                IReadOnlyList<CanvasObject> createdSnapshots,
                IReadOnlyList<CanvasObject> deletedSourceSnapshots)
            {
                _createdSnapshots = createdSnapshots;
                _deletedSourceSnapshots = deletedSourceSnapshots;
            }

            public string Description => _createdSnapshots.Count == 1
                ? "Create feature from drawing object"
                : $"Create {_createdSnapshots.Count} features from drawing objects";

            public async Task UndoAsync(frmMain owner)
            {
                await owner.DeleteCanvasObjectsForUndoRedoAsync(_createdSnapshots.Select(item => item.Id).ToArray());
                await owner.RestoreCanvasObjectSnapshotsAsync(_deletedSourceSnapshots);
            }

            public async Task RedoAsync(frmMain owner)
            {
                await owner.RestoreCanvasObjectSnapshotsAsync(_createdSnapshots);
                await owner.DeleteCanvasObjectsForUndoRedoAsync(_deletedSourceSnapshots.Select(item => item.Id).ToArray());
            }
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

        private void tsCanvasTools_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void mapSelectAllSelectableToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            mapSelectAllSelectableToolStripMenuItem_Click(sender, e);
        }
    }
}

