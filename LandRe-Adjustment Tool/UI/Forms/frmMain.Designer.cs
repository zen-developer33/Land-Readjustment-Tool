using System.Drawing;
using System.Windows.Forms;
using Land_Readjustment_Tool.UI.CustomControls;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Core;
namespace Land_Readjustment_Tool
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TreeNode treeNode1 = new TreeNode("Original Data Layer");
            TreeNode treeNode2 = new TreeNode("Roads");
            TreeNode treeNode3 = new TreeNode("Block Layout Plan", new TreeNode[] { treeNode2 });
            TreeNode treeNode4 = new TreeNode("Replotted Parcels");
            TreeNode treeNode5 = new TreeNode("RePlot", new TreeNode[] { treeNode1, treeNode3, treeNode4 });
            TreeNode treeNode6 = new TreeNode("Drawing/Mark Up Layers");
            TreeNode treeNode7 = new TreeNode("Other External Layers");
            TreeNode treeNode8 = new TreeNode("Raster Layers");
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            tsmNewProject = new ToolStripMenuItem();
            tsmOpenProject = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            tsmRecentProjects = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tsmSave = new ToolStripMenuItem();
            tsmSaveAs = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            tsmExit = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            projectToolStripMenuItem = new ToolStripMenuItem();
            tsmProjectInformation = new ToolStripMenuItem();
            tsmProjectSetting = new ToolStripMenuItem();
            toolStripSeparator7 = new ToolStripSeparator();
            tsmBackupProject = new ToolStripMenuItem();
            tsmRestoreBackup = new ToolStripMenuItem();
            toolStripSeparator8 = new ToolStripSeparator();
            tsmCloseProject = new ToolStripMenuItem();
            dataToolStripMenuItem = new ToolStripMenuItem();
            importDataToolStripMenuItem1 = new ToolStripMenuItem();
            ImportParcelOwnerShipRecords = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            ImportProjectBoundaryDXFDWGToolStripMenuItem = new ToolStripMenuItem();
            importCadastralDataDXFDWGShapefileToolStripMenuItem = new ToolStripMenuItem();
            mnuImportBlockLayoutPlan = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            baseMapsToolStripMenuItem = new ToolStripMenuItem();
            mnuImportXyzTiles = new ToolStripMenuItem();
            mnuImportExternalLayers = new ToolStripMenuItem();
            importToolStripMenuItem = new ToolStripMenuItem();
            viewEditRecordToolStripMenuItem = new ToolStripMenuItem();
            landOwnerDataToolStripMenuItem = new ToolStripMenuItem();
            buildingInventoryToolStripMenuItem = new ToolStripMenuItem();
            originalScenarioSummaryToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator18 = new ToolStripSeparator();
            assignToolStripMenuItem = new ToolStripMenuItem();
            roadDataToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator20 = new ToolStripSeparator();
            blockDataToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator19 = new ToolStripSeparator();
            assignmentToolStripMenuItem = new ToolStripMenuItem();
            projectBoundaryAssignmentToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator21 = new ToolStripSeparator();
            cadastralRecordsAssignmentToolStripMenuItem = new ToolStripMenuItem();
            buildingInventoryDataToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            contributionToolStripMenuItem = new ToolStripMenuItem();
            contributionSettingsToolStripMenuItem = new ToolStripMenuItem();
            calculateContributionToolStripMenuItem = new ToolStripMenuItem();
            contributionSummaryToolStripMenuItem = new ToolStripMenuItem();
            replottingToolStripMenuItem = new ToolStripMenuItem();
            startReplotWorkspaceToolStripMenuItem = new ToolStripMenuItem();
            parcelAdjustmentToolStripMenuItem = new ToolStripMenuItem();
            roadNetworkToolStripMenuItem = new ToolStripMenuItem();
            validationToolStripMenuItem = new ToolStripMenuItem();
            validateOwnershipRecordsToolStripMenuItem = new ToolStripMenuItem();
            validateSpatialDataToolStripMenuItem = new ToolStripMenuItem();
            validationIssuesToolStripMenuItem = new ToolStripMenuItem();
            reportsToolStripMenuItem = new ToolStripMenuItem();
            ownerParcelReportToolStripMenuItem = new ToolStripMenuItem();
            contributionReportToolStripMenuItem = new ToolStripMenuItem();
            exportDataToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            mnuAreaConverterTool = new ToolStripMenuItem();
            helToolStripMenuItem = new ToolStripMenuItem();
            userGuideToolStripMenuItem = new ToolStripMenuItem();
            aboutRePlotToolStripMenuItem = new ToolStripMenuItem();
            cadastralDataToolStripMenuItem = new ToolStripMenuItem();
            importDataToolStripMenuItem = new ToolStripMenuItem();
            viewEditRecordsToolStripMenuItem = new ToolStripMenuItem();
            leftSplitContainer = new SplitContainer();
            grpLayer = new GroupBox();
            treeViewLayers = new TreeView();
            tabProperties = new TabControl();
            tabGeneral = new TabPage();
            lblLayerName = new Label();
            txtLayerName = new TextBox();
            lblLayerType = new Label();
            cboLayerType = new ComboBox();
            lblBorderColor = new Label();
            pnlBorderColor = new Panel();
            btnBorderColor = new Button();
            lblLineStyle = new Label();
            cboLineStyle = new ComboBox();
            lblLineWeight = new Label();
            cboLineWeight = new ComboBox();
            chkVisible = new CheckBox();
            chkLocked = new CheckBox();
            chkSelectable = new CheckBox();
            chkPrintable = new CheckBox();
            tabFill = new TabPage();
            lblFillColor = new Label();
            pnlFillColor = new Panel();
            btnFillColor = new Button();
            lblFillStyle = new Label();
            cboFillStyle = new ComboBox();
            lblHatch = new Label();
            cboHatch = new ComboBox();
            lblTransparency = new Label();
            lblTranspValue = new Label();
            trkTransparency = new TrackBar();
            tabLabel = new TabPage();
            chkShowLabels = new CheckBox();
            lblFont = new Label();
            txtFontName = new TextBox();
            btnPickFont = new Button();
            lblFontSize = new Label();
            numFontSize = new NumericUpDown();
            lblLabelColor = new Label();
            pnlLabelColor = new Panel();
            btnLabelColor = new Button();
            lblLabelField = new Label();
            cboLabelField = new ComboBox();
            label1 = new Label();
            mainSplitContainer = new SplitContainer();
            splitContainer3 = new SplitContainer();
            mapCanvasControlMain = new MapCanvasControl();
            tsCanvasTools = new ToolStrip();
            tsmExpandCollapseLeftPanel = new ToolStripButton();
            toolStripSeparator10 = new ToolStripSeparator();
            mnuSelectTool = new ToolStripButton();
            mnuDrawPoint = new ToolStripButton();
            mnuDrawLine = new ToolStripButton();
            mnuDrawPolyline = new ToolStripButton();
            mnuDrawArc = new ToolStripButton();
            mnuDrawRectangle = new ToolStripButton();
            mnuDrawPolygon = new ToolStripButton();
            mnuDrawCircle = new ToolStripButton();
            mnuDrawText = new ToolStripButton();
            toolStripSeparator17 = new ToolStripSeparator();
            lblCurrentDrawingLayer = new ToolStripLabel();
            cboCurrentDrawingLayer = new ToolStripComboBox();
            mnuCanvasDebugOverlay = new ToolStripButton();
            toolStripLabel1 = new ToolStripLabel();
            tsmExpandCollapseRightPanel = new ToolStripButton();
            toolStripSeparator11 = new ToolStripSeparator();
            mnuOrthoToggle = new ToolStripButton();
            mnuOSnapToggle = new ToolStripButton();
            grpParcelObjProp = new GroupBox();
            btnSelectFromRecords = new Button();
            btnConfigureParcelProperties = new Button();
            cboSelectedPropertyObject = new ComboBox();
            btnPreviousSelectedPropertyObject = new Button();
            btnNextSelectedPropertyObject = new Button();
            dgvParcelObjProperty = new DataGridView();
            colParcelPropertyField = new DataGridViewTextBoxColumn();
            colParcelPropertyValue = new DataGridViewTextBoxColumn();
            statusCanvas = new StatusStrip();
            lblProjectName = new ToolStripStatusLabel();
            tsStatusSep1 = new ToolStripSeparator();
            lblActiveTool = new ToolStripStatusLabel();
            lblStatusMessage = new ToolStripStatusLabel();
            hostProgressBarHost = new StatusProgressBar();
            lblOperationProgressStatus = new ToolStripStatusLabel();
            lblScale = new ToolStripStatusLabel();
            lblCanvasCoordinates = new ToolStripStatusLabel();
            lblStatusSpacer = new ToolStripStatusLabel();
            tsStatusSep2 = new ToolStripSeparator();
            mnuNewProject = new ToolStripButton();
            mnuOpenProject = new ToolStripButton();
            mnuSaveProject = new ToolStripButton();
            mnuSaveAsProject = new ToolStripButton();
            mnuBackup = new ToolStripButton();
            mnuRestoreBackup = new ToolStripButton();
            mnuCloseProject = new ToolStripButton();
            toolStripSeparator9 = new ToolStripSeparator();
            toolStripButton3 = new ToolStripSeparator();
            mnuProjectInfo = new ToolStripButton();
            mnuProjectSettings = new ToolStripButton();
            btnOriginalScenarioSummary = new ToolStripButton();
            toolStripSeparator12 = new ToolStripSeparator();
            toolStripSeparator13 = new ToolStripSeparator();
            mnuUndo = new ToolStripButton();
            mnuRedo = new ToolStripButton();
            toolStripSeparator14 = new ToolStripSeparator();
            mnuPan = new ToolStripButton();
            mnuZoomIn = new ToolStripButton();
            mnuZoomOut = new ToolStripButton();
            mnuZoomExtent = new ToolStripButton();
            mnuZoomWindow = new ToolStripButton();
            toolStripSeparator15 = new ToolStripSeparator();
            toolStripSeparator16 = new ToolStripSeparator();
            tsProjectMenu = new ToolStrip();
            toolStripSeparator22 = new ToolStripSeparator();
            toolStripMenuItem2 = new ToolStripMenuItem();
            mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)leftSplitContainer).BeginInit();
            leftSplitContainer.Panel1.SuspendLayout();
            leftSplitContainer.SuspendLayout();
            grpLayer.SuspendLayout();
            tabProperties.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabFill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkTransparency).BeginInit();
            tabLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFontSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            tsCanvasTools.SuspendLayout();
            grpParcelObjProp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParcelObjProperty).BeginInit();
            statusCanvas.SuspendLayout();
            tsProjectMenu.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.BackColor = Color.White;
            mainMenuStrip.Font = new Font("Segoe UI", 9F);
            mainMenuStrip.ImageScalingSize = new Size(20, 20);
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, projectToolStripMenuItem, dataToolStripMenuItem, contributionToolStripMenuItem, replottingToolStripMenuItem, validationToolStripMenuItem, reportsToolStripMenuItem, toolsToolStripMenuItem, helToolStripMenuItem });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(5, 2, 0, 2);
            mainMenuStrip.Size = new Size(1624, 28);
            mainMenuStrip.TabIndex = 0;
            mainMenuStrip.Text = "Main Menu Strip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmNewProject, tsmOpenProject, toolStripSeparator1, tsmRecentProjects, toolStripSeparator2, tsmSave, tsmSaveAs, toolStripSeparator3, tsmExit, toolStripSeparator4 });
            fileToolStripMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // tsmNewProject
            // 
            tsmNewProject.Image = Properties.Resources.icons8_file_50;
            tsmNewProject.Name = "tsmNewProject";
            tsmNewProject.ShortcutKeys = Keys.Control | Keys.N;
            tsmNewProject.Size = new Size(283, 26);
            tsmNewProject.Text = "&New Project";
            tsmNewProject.Click += tsmNewProject_Click;
            // 
            // tsmOpenProject
            // 
            tsmOpenProject.Image = Properties.Resources.icons8_open_folder_50;
            tsmOpenProject.Name = "tsmOpenProject";
            tsmOpenProject.ShortcutKeys = Keys.Control | Keys.O;
            tsmOpenProject.Size = new Size(283, 26);
            tsmOpenProject.Text = "&Open Project";
            tsmOpenProject.Click += tsmOpenProject_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(280, 6);
            // 
            // tsmRecentProjects
            // 
            tsmRecentProjects.Image = Properties.Resources.icons8_time_machine_50;
            tsmRecentProjects.Name = "tsmRecentProjects";
            tsmRecentProjects.Size = new Size(283, 26);
            tsmRecentProjects.Text = "Recent Projects";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(280, 6);
            // 
            // tsmSave
            // 
            tsmSave.Enabled = false;
            tsmSave.Image = Properties.Resources.icons8_save_50;
            tsmSave.Name = "tsmSave";
            tsmSave.ShortcutKeys = Keys.Control | Keys.S;
            tsmSave.Size = new Size(283, 26);
            tsmSave.Text = "Save Project";
            // 
            // tsmSaveAs
            // 
            tsmSaveAs.Enabled = false;
            tsmSaveAs.Image = Properties.Resources.icons8_save_as_50;
            tsmSaveAs.Name = "tsmSaveAs";
            tsmSaveAs.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            tsmSaveAs.Size = new Size(283, 26);
            tsmSaveAs.Text = "Save As Project";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(280, 6);
            // 
            // tsmExit
            // 
            tsmExit.Image = Properties.Resources.icons8_exit_50;
            tsmExit.Name = "tsmExit";
            tsmExit.Size = new Size(283, 26);
            tsmExit.Text = "Exit";
            tsmExit.Click += ExitToolStripMenuItem_Click_1;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(280, 6);
            // 
            // projectToolStripMenuItem
            // 
            projectToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmProjectInformation, tsmProjectSetting, toolStripSeparator7, tsmBackupProject, tsmRestoreBackup, toolStripSeparator8, tsmCloseProject });
            projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            projectToolStripMenuItem.Size = new Size(69, 24);
            projectToolStripMenuItem.Text = "Project";
            // 
            // tsmProjectInformation
            // 
            tsmProjectInformation.Enabled = false;
            tsmProjectInformation.Image = Properties.Resources.icons8_info_squared_50;
            tsmProjectInformation.Name = "tsmProjectInformation";
            tsmProjectInformation.Size = new Size(232, 26);
            tsmProjectInformation.Text = "Project Information";
            tsmProjectInformation.Click += tsmProjectInformation_Click;
            // 
            // tsmProjectSetting
            // 
            tsmProjectSetting.Enabled = false;
            tsmProjectSetting.Image = Properties.Resources.icons8_wrench_50;
            tsmProjectSetting.Name = "tsmProjectSetting";
            tsmProjectSetting.Size = new Size(232, 26);
            tsmProjectSetting.Text = "Project Setting";
            tsmProjectSetting.Click += tsmProjectSetting_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(229, 6);
            // 
            // tsmBackupProject
            // 
            tsmBackupProject.Enabled = false;
            tsmBackupProject.Image = Properties.Resources.icons8_database_export_503;
            tsmBackupProject.Name = "tsmBackupProject";
            tsmBackupProject.Size = new Size(232, 26);
            tsmBackupProject.Text = "Backup Project";
            tsmBackupProject.Click += tsmBackupProject_Click;
            // 
            // tsmRestoreBackup
            // 
            tsmRestoreBackup.Image = Properties.Resources.icons8_data_backup_50;
            tsmRestoreBackup.Name = "tsmRestoreBackup";
            tsmRestoreBackup.Size = new Size(232, 26);
            tsmRestoreBackup.Text = "Restore From Backup";
            tsmRestoreBackup.Click += tsmRestoreBackup_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new Size(229, 6);
            // 
            // tsmCloseProject
            // 
            tsmCloseProject.Enabled = false;
            tsmCloseProject.Image = Properties.Resources.icons8_close_50;
            tsmCloseProject.Name = "tsmCloseProject";
            tsmCloseProject.Size = new Size(232, 26);
            tsmCloseProject.Text = "Close Project";
            tsmCloseProject.Click += tsmCloseProject_Click;
            // 
            // dataToolStripMenuItem
            // 
            dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { importDataToolStripMenuItem1, importToolStripMenuItem, originalScenarioSummaryToolStripMenuItem, toolStripSeparator18, assignToolStripMenuItem, toolStripSeparator19, assignmentToolStripMenuItem });
            dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            dataToolStripMenuItem.Size = new Size(147, 24);
            dataToolStripMenuItem.Text = "Data Management";
            // 
            // importDataToolStripMenuItem1
            // 
            importDataToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { ImportParcelOwnerShipRecords, toolStripSeparator5, ImportProjectBoundaryDXFDWGToolStripMenuItem, importCadastralDataDXFDWGShapefileToolStripMenuItem, mnuImportBlockLayoutPlan, toolStripSeparator6, baseMapsToolStripMenuItem, mnuImportXyzTiles, mnuImportExternalLayers });
            importDataToolStripMenuItem1.Name = "importDataToolStripMenuItem1";
            importDataToolStripMenuItem1.Size = new Size(224, 26);
            importDataToolStripMenuItem1.Text = "Import...";
            // 
            // ImportParcelOwnerShipRecords
            // 
            ImportParcelOwnerShipRecords.Enabled = false;
            ImportParcelOwnerShipRecords.Name = "ImportParcelOwnerShipRecords";
            ImportParcelOwnerShipRecords.Size = new Size(385, 26);
            ImportParcelOwnerShipRecords.Text = "Parcel Ownership Records (Excel/CSV)";
            ImportParcelOwnerShipRecords.Click += ImportParcelOwnerShipRecords_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(382, 6);
            // 
            // ImportProjectBoundaryDXFDWGToolStripMenuItem
            // 
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Name = "ImportProjectBoundaryDXFDWGToolStripMenuItem";
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Size = new Size(385, 26);
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Text = "Project Boundary (DXF/SHP/KML)";
            // 
            // importCadastralDataDXFDWGShapefileToolStripMenuItem
            // 
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Name = "importCadastralDataDXFDWGShapefileToolStripMenuItem";
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Size = new Size(385, 26);
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Text = "Cadastral Map (DXF/DWG/Shapefile)";
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Click += importCadastralDataDXFDWGShapefileToolStripMenuItem_Click;
            // 
            // mnuImportBlockLayoutPlan
            // 
            mnuImportBlockLayoutPlan.Name = "mnuImportBlockLayoutPlan";
            mnuImportBlockLayoutPlan.Size = new Size(385, 26);
            mnuImportBlockLayoutPlan.Text = "Block Layout Plan (DXF/DWG)";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(382, 6);
            // 
            // baseMapsToolStripMenuItem
            // 
            baseMapsToolStripMenuItem.Name = "baseMapsToolStripMenuItem";
            baseMapsToolStripMenuItem.Size = new Size(385, 26);
            baseMapsToolStripMenuItem.Text = "Import Raster (GeoTIFF, MBTiles, TIFF, PNG...)";
            // 
            // mnuImportXyzTiles
            // 
            mnuImportXyzTiles.Name = "mnuImportXyzTiles";
            mnuImportXyzTiles.Size = new Size(385, 26);
            mnuImportXyzTiles.Text = "Import XYZ Tiles...";
            // 
            // mnuImportExternalLayers
            // 
            mnuImportExternalLayers.Name = "mnuImportExternalLayers";
            mnuImportExternalLayers.Size = new Size(385, 26);
            mnuImportExternalLayers.Text = "External Layers (DXF/DWG/KML)...";
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { viewEditRecordToolStripMenuItem, landOwnerDataToolStripMenuItem, buildingInventoryToolStripMenuItem });
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new Size(224, 26);
            importToolStripMenuItem.Text = "Records";
            // 
            // viewEditRecordToolStripMenuItem
            // 
            viewEditRecordToolStripMenuItem.Name = "viewEditRecordToolStripMenuItem";
            viewEditRecordToolStripMenuItem.Size = new Size(245, 26);
            viewEditRecordToolStripMenuItem.Text = "Original Parcel Records";
            viewEditRecordToolStripMenuItem.Click += viewEditRecordToolStripMenuItem_Click;
            // 
            // landOwnerDataToolStripMenuItem
            // 
            landOwnerDataToolStripMenuItem.Enabled = false;
            landOwnerDataToolStripMenuItem.Name = "landOwnerDataToolStripMenuItem";
            landOwnerDataToolStripMenuItem.Size = new Size(245, 26);
            landOwnerDataToolStripMenuItem.Text = "Land Owners";
            landOwnerDataToolStripMenuItem.Click += landOwnerDataToolStripMenuItem_Click;
            // 
            // buildingInventoryToolStripMenuItem
            // 
            buildingInventoryToolStripMenuItem.Name = "buildingInventoryToolStripMenuItem";
            buildingInventoryToolStripMenuItem.Size = new Size(245, 26);
            buildingInventoryToolStripMenuItem.Text = "Building Inventory ";
            // 
            // originalScenarioSummaryToolStripMenuItem
            // 
            originalScenarioSummaryToolStripMenuItem.Name = "originalScenarioSummaryToolStripMenuItem";
            originalScenarioSummaryToolStripMenuItem.Size = new Size(224, 26);
            originalScenarioSummaryToolStripMenuItem.Text = "Original Scenario Summary";
            // 
            // toolStripSeparator18
            // 
            toolStripSeparator18.Name = "toolStripSeparator18";
            toolStripSeparator18.Size = new Size(221, 6);
            // 
            // assignToolStripMenuItem
            // 
            assignToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { roadDataToolStripMenuItem, toolStripSeparator20, blockDataToolStripMenuItem });
            assignToolStripMenuItem.Name = "assignToolStripMenuItem";
            assignToolStripMenuItem.Size = new Size(224, 26);
            assignToolStripMenuItem.Text = "Define...";
            // 
            // roadDataToolStripMenuItem
            // 
            roadDataToolStripMenuItem.Name = "roadDataToolStripMenuItem";
            roadDataToolStripMenuItem.Size = new Size(134, 26);
            roadDataToolStripMenuItem.Text = "Roads";
            // 
            // toolStripSeparator20
            // 
            toolStripSeparator20.Name = "toolStripSeparator20";
            toolStripSeparator20.Size = new Size(131, 6);
            // 
            // blockDataToolStripMenuItem
            // 
            blockDataToolStripMenuItem.Name = "blockDataToolStripMenuItem";
            blockDataToolStripMenuItem.Size = new Size(134, 26);
            blockDataToolStripMenuItem.Text = "Blocks";
            // 
            // toolStripSeparator19
            // 
            toolStripSeparator19.Name = "toolStripSeparator19";
            toolStripSeparator19.Size = new Size(221, 6);
            // 
            // assignmentToolStripMenuItem
            // 
            assignmentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { projectBoundaryAssignmentToolStripMenuItem, toolStripSeparator21, cadastralRecordsAssignmentToolStripMenuItem, buildingInventoryDataToolStripMenuItem, toolStripSeparator22, toolStripMenuItem2, toolStripMenuItem1 });
            assignmentToolStripMenuItem.Name = "assignmentToolStripMenuItem";
            assignmentToolStripMenuItem.Size = new Size(224, 26);
            assignmentToolStripMenuItem.Text = "Assign...";
            // 
            // projectBoundaryAssignmentToolStripMenuItem
            // 
            projectBoundaryAssignmentToolStripMenuItem.Name = "projectBoundaryAssignmentToolStripMenuItem";
            projectBoundaryAssignmentToolStripMenuItem.Size = new Size(257, 26);
            projectBoundaryAssignmentToolStripMenuItem.Text = "Project Boundary";
            // 
            // toolStripSeparator21
            // 
            toolStripSeparator21.Name = "toolStripSeparator21";
            toolStripSeparator21.Size = new Size(254, 6);
            // 
            // cadastralRecordsAssignmentToolStripMenuItem
            // 
            cadastralRecordsAssignmentToolStripMenuItem.Name = "cadastralRecordsAssignmentToolStripMenuItem";
            cadastralRecordsAssignmentToolStripMenuItem.Size = new Size(257, 26);
            cadastralRecordsAssignmentToolStripMenuItem.Text = "Cadastral Records";
            cadastralRecordsAssignmentToolStripMenuItem.Click += CadastralRecordsAssignmentToolStripMenuItem_Click;
            // 
            // buildingInventoryDataToolStripMenuItem
            // 
            buildingInventoryDataToolStripMenuItem.Name = "buildingInventoryDataToolStripMenuItem";
            buildingInventoryDataToolStripMenuItem.Size = new Size(257, 26);
            buildingInventoryDataToolStripMenuItem.Text = "Building Inventory Data";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(257, 26);
            toolStripMenuItem1.Text = "Proposed Blocks";
            // 
            // contributionToolStripMenuItem
            // 
            contributionToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { contributionSettingsToolStripMenuItem, calculateContributionToolStripMenuItem, contributionSummaryToolStripMenuItem });
            contributionToolStripMenuItem.Name = "contributionToolStripMenuItem";
            contributionToolStripMenuItem.Size = new Size(106, 24);
            contributionToolStripMenuItem.Text = "Contribution";
            // 
            // contributionSettingsToolStripMenuItem
            // 
            contributionSettingsToolStripMenuItem.Enabled = false;
            contributionSettingsToolStripMenuItem.Name = "contributionSettingsToolStripMenuItem";
            contributionSettingsToolStripMenuItem.Size = new Size(241, 26);
            contributionSettingsToolStripMenuItem.Text = "Contribution Settings";
            // 
            // calculateContributionToolStripMenuItem
            // 
            calculateContributionToolStripMenuItem.Enabled = false;
            calculateContributionToolStripMenuItem.Name = "calculateContributionToolStripMenuItem";
            calculateContributionToolStripMenuItem.Size = new Size(241, 26);
            calculateContributionToolStripMenuItem.Text = "Calculate Contribution";
            // 
            // contributionSummaryToolStripMenuItem
            // 
            contributionSummaryToolStripMenuItem.Enabled = false;
            contributionSummaryToolStripMenuItem.Name = "contributionSummaryToolStripMenuItem";
            contributionSummaryToolStripMenuItem.Size = new Size(241, 26);
            contributionSummaryToolStripMenuItem.Text = "Contribution Summary";
            // 
            // replottingToolStripMenuItem
            // 
            replottingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { startReplotWorkspaceToolStripMenuItem, parcelAdjustmentToolStripMenuItem, roadNetworkToolStripMenuItem });
            replottingToolStripMenuItem.Name = "replottingToolStripMenuItem";
            replottingToolStripMenuItem.Size = new Size(93, 24);
            replottingToolStripMenuItem.Text = "Replotting";
            // 
            // startReplotWorkspaceToolStripMenuItem
            // 
            startReplotWorkspaceToolStripMenuItem.Name = "startReplotWorkspaceToolStripMenuItem";
            startReplotWorkspaceToolStripMenuItem.Size = new Size(247, 26);
            startReplotWorkspaceToolStripMenuItem.Text = "Start Replot Workspace";
            // 
            // parcelAdjustmentToolStripMenuItem
            // 
            parcelAdjustmentToolStripMenuItem.Enabled = false;
            parcelAdjustmentToolStripMenuItem.Name = "parcelAdjustmentToolStripMenuItem";
            parcelAdjustmentToolStripMenuItem.Size = new Size(247, 26);
            parcelAdjustmentToolStripMenuItem.Text = "Parcel Adjustment";
            // 
            // roadNetworkToolStripMenuItem
            // 
            roadNetworkToolStripMenuItem.Enabled = false;
            roadNetworkToolStripMenuItem.Name = "roadNetworkToolStripMenuItem";
            roadNetworkToolStripMenuItem.Size = new Size(247, 26);
            roadNetworkToolStripMenuItem.Text = "Road Network";
            // 
            // validationToolStripMenuItem
            // 
            validationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { validateOwnershipRecordsToolStripMenuItem, validateSpatialDataToolStripMenuItem, validationIssuesToolStripMenuItem });
            validationToolStripMenuItem.Name = "validationToolStripMenuItem";
            validationToolStripMenuItem.Size = new Size(90, 24);
            validationToolStripMenuItem.Text = "Validation";
            // 
            // validateOwnershipRecordsToolStripMenuItem
            // 
            validateOwnershipRecordsToolStripMenuItem.Enabled = false;
            validateOwnershipRecordsToolStripMenuItem.Name = "validateOwnershipRecordsToolStripMenuItem";
            validateOwnershipRecordsToolStripMenuItem.Size = new Size(277, 26);
            validateOwnershipRecordsToolStripMenuItem.Text = "Validate Ownership Records";
            // 
            // validateSpatialDataToolStripMenuItem
            // 
            validateSpatialDataToolStripMenuItem.Enabled = false;
            validateSpatialDataToolStripMenuItem.Name = "validateSpatialDataToolStripMenuItem";
            validateSpatialDataToolStripMenuItem.Size = new Size(277, 26);
            validateSpatialDataToolStripMenuItem.Text = "Validate Spatial Data";
            // 
            // validationIssuesToolStripMenuItem
            // 
            validationIssuesToolStripMenuItem.Enabled = false;
            validationIssuesToolStripMenuItem.Name = "validationIssuesToolStripMenuItem";
            validationIssuesToolStripMenuItem.Size = new Size(277, 26);
            validationIssuesToolStripMenuItem.Text = "Validation Issues";
            // 
            // reportsToolStripMenuItem
            // 
            reportsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { ownerParcelReportToolStripMenuItem, contributionReportToolStripMenuItem, exportDataToolStripMenuItem });
            reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
            reportsToolStripMenuItem.Size = new Size(69, 24);
            reportsToolStripMenuItem.Text = "Output";
            // 
            // ownerParcelReportToolStripMenuItem
            // 
            ownerParcelReportToolStripMenuItem.Enabled = false;
            ownerParcelReportToolStripMenuItem.Name = "ownerParcelReportToolStripMenuItem";
            ownerParcelReportToolStripMenuItem.Size = new Size(229, 26);
            ownerParcelReportToolStripMenuItem.Text = "Owner/Parcel Report";
            // 
            // contributionReportToolStripMenuItem
            // 
            contributionReportToolStripMenuItem.Enabled = false;
            contributionReportToolStripMenuItem.Name = "contributionReportToolStripMenuItem";
            contributionReportToolStripMenuItem.Size = new Size(229, 26);
            contributionReportToolStripMenuItem.Text = "Contribution Report";
            // 
            // exportDataToolStripMenuItem
            // 
            exportDataToolStripMenuItem.Enabled = false;
            exportDataToolStripMenuItem.Name = "exportDataToolStripMenuItem";
            exportDataToolStripMenuItem.Size = new Size(229, 26);
            exportDataToolStripMenuItem.Text = "Export Data";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mnuAreaConverterTool });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(58, 24);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // mnuAreaConverterTool
            // 
            mnuAreaConverterTool.Name = "mnuAreaConverterTool";
            mnuAreaConverterTool.Size = new Size(191, 26);
            mnuAreaConverterTool.Text = "Area Converter";
            mnuAreaConverterTool.Click += mnuAreaConverterTool_Click;
            // 
            // helToolStripMenuItem
            // 
            helToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { userGuideToolStripMenuItem, aboutRePlotToolStripMenuItem });
            helToolStripMenuItem.Name = "helToolStripMenuItem";
            helToolStripMenuItem.Size = new Size(55, 24);
            helToolStripMenuItem.Text = "Help";
            // 
            // userGuideToolStripMenuItem
            // 
            userGuideToolStripMenuItem.Enabled = false;
            userGuideToolStripMenuItem.Name = "userGuideToolStripMenuItem";
            userGuideToolStripMenuItem.Size = new Size(180, 26);
            userGuideToolStripMenuItem.Text = "User Guide";
            // 
            // aboutRePlotToolStripMenuItem
            // 
            aboutRePlotToolStripMenuItem.Enabled = false;
            aboutRePlotToolStripMenuItem.Name = "aboutRePlotToolStripMenuItem";
            aboutRePlotToolStripMenuItem.Size = new Size(180, 26);
            aboutRePlotToolStripMenuItem.Text = "About RePlot";
            // 
            // cadastralDataToolStripMenuItem
            // 
            cadastralDataToolStripMenuItem.Name = "cadastralDataToolStripMenuItem";
            cadastralDataToolStripMenuItem.Size = new Size(234, 26);
            cadastralDataToolStripMenuItem.Text = "Land Ownership Data";
            // 
            // importDataToolStripMenuItem
            // 
            importDataToolStripMenuItem.Name = "importDataToolStripMenuItem";
            importDataToolStripMenuItem.Size = new Size(213, 26);
            importDataToolStripMenuItem.Text = "Import";
            // 
            // viewEditRecordsToolStripMenuItem
            // 
            viewEditRecordsToolStripMenuItem.Name = "viewEditRecordsToolStripMenuItem";
            viewEditRecordsToolStripMenuItem.Size = new Size(213, 26);
            viewEditRecordsToolStripMenuItem.Text = "View/Edit Records";
            // 
            // leftSplitContainer
            // 
            leftSplitContainer.AllowDrop = true;
            leftSplitContainer.Dock = DockStyle.Fill;
            leftSplitContainer.Location = new Point(0, 0);
            leftSplitContainer.Margin = new Padding(4);
            leftSplitContainer.Name = "leftSplitContainer";
            leftSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // leftSplitContainer.Panel1
            // 
            leftSplitContainer.Panel1.Controls.Add(grpLayer);
            // 
            // leftSplitContainer.Panel2
            // 
            leftSplitContainer.Panel2.BackColor = Color.White;
            leftSplitContainer.Panel2Collapsed = true;
            leftSplitContainer.Size = new Size(272, 555);
            leftSplitContainer.SplitterDistance = 484;
            leftSplitContainer.TabIndex = 0;
            // 
            // grpLayer
            // 
            grpLayer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLayer.Controls.Add(treeViewLayers);
            grpLayer.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpLayer.Location = new Point(4, 4);
            grpLayer.Margin = new Padding(4);
            grpLayer.Name = "grpLayer";
            grpLayer.Padding = new Padding(4);
            grpLayer.RightToLeft = RightToLeft.No;
            grpLayer.Size = new Size(265, 548);
            grpLayer.TabIndex = 0;
            grpLayer.TabStop = false;
            grpLayer.Text = "Layers";
            // 
            // treeViewLayers
            // 
            treeViewLayers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeViewLayers.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            treeViewLayers.Location = new Point(6, 23);
            treeViewLayers.Margin = new Padding(4);
            treeViewLayers.Name = "treeViewLayers";
            treeNode1.Name = "LayerGroup_OriginalDataLayer";
            treeNode1.Text = "Original Data Layer";
            treeNode2.Name = "LayerGroup_Roads";
            treeNode2.Text = "Roads";
            treeNode3.Name = "LayerGroup_BlockLayoutPlan";
            treeNode3.Text = "Block Layout Plan";
            treeNode4.Name = "LayerGroup_ReplottedParcels";
            treeNode4.Text = "Replotted Parcels";
            treeNode5.Name = "LayerGroup_RePlotRoot";
            treeNode5.Text = "RePlot";
            treeNode6.Name = "LayerGroup_DrawingMarkupLayers";
            treeNode6.Text = "Drawing/Mark Up Layers";
            treeNode7.Name = "LayerGroup_OtherExternalLayers";
            treeNode7.Text = "Other External Layers";
            treeNode8.Name = "LayerGroup_RasterLayer";
            treeNode8.Text = "Raster Layers";
            treeViewLayers.Nodes.AddRange(new TreeNode[] { treeNode5, treeNode6, treeNode7, treeNode8 });
            treeViewLayers.Size = new Size(251, 517);
            treeViewLayers.TabIndex = 0;
            // 
            // tabProperties
            // 
            tabProperties.Controls.Add(tabGeneral);
            tabProperties.Controls.Add(tabFill);
            tabProperties.Controls.Add(tabLabel);
            tabProperties.Dock = DockStyle.Fill;
            tabProperties.Font = new Font("Segoe UI", 9F);
            tabProperties.Location = new Point(6, 28);
            tabProperties.Name = "tabProperties";
            tabProperties.SelectedIndex = 0;
            tabProperties.Size = new Size(177, 223);
            tabProperties.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.Controls.Add(lblLayerName);
            tabGeneral.Controls.Add(txtLayerName);
            tabGeneral.Controls.Add(lblLayerType);
            tabGeneral.Controls.Add(cboLayerType);
            tabGeneral.Controls.Add(lblBorderColor);
            tabGeneral.Controls.Add(pnlBorderColor);
            tabGeneral.Controls.Add(btnBorderColor);
            tabGeneral.Controls.Add(lblLineStyle);
            tabGeneral.Controls.Add(cboLineStyle);
            tabGeneral.Controls.Add(lblLineWeight);
            tabGeneral.Controls.Add(cboLineWeight);
            tabGeneral.Controls.Add(chkVisible);
            tabGeneral.Controls.Add(chkLocked);
            tabGeneral.Controls.Add(chkSelectable);
            tabGeneral.Controls.Add(chkPrintable);
            tabGeneral.Location = new Point(4, 29);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(10);
            tabGeneral.Size = new Size(169, 190);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // lblLayerName
            // 
            lblLayerName.AutoSize = true;
            lblLayerName.Font = new Font("Segoe UI", 9F);
            lblLayerName.Location = new Point(20, 17);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(52, 20);
            lblLayerName.TabIndex = 0;
            lblLayerName.Text = "Name:";
            // 
            // txtLayerName
            // 
            txtLayerName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLayerName.Font = new Font("Segoe UI", 9F);
            txtLayerName.Location = new Point(127, 14);
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(29, 27);
            txtLayerName.TabIndex = 1;
            // 
            // lblLayerType
            // 
            lblLayerType.AutoSize = true;
            lblLayerType.Font = new Font("Segoe UI", 9F);
            lblLayerType.Location = new Point(20, 54);
            lblLayerType.Name = "lblLayerType";
            lblLayerType.Size = new Size(43, 20);
            lblLayerType.TabIndex = 2;
            lblLayerType.Text = "Type:";
            // 
            // cboLayerType
            // 
            cboLayerType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboLayerType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLayerType.Font = new Font("Segoe UI", 9F);
            cboLayerType.Items.AddRange(new object[] { "Point", "Polyline", "Polygon", "Annotation" });
            cboLayerType.Location = new Point(127, 48);
            cboLayerType.Name = "cboLayerType";
            cboLayerType.Size = new Size(29, 28);
            cboLayerType.TabIndex = 3;
            // 
            // lblBorderColor
            // 
            lblBorderColor.AutoSize = true;
            lblBorderColor.Font = new Font("Segoe UI", 9F);
            lblBorderColor.Location = new Point(20, 91);
            lblBorderColor.Name = "lblBorderColor";
            lblBorderColor.Size = new Size(97, 20);
            lblBorderColor.TabIndex = 4;
            lblBorderColor.Text = "Border Color:";
            // 
            // pnlBorderColor
            // 
            pnlBorderColor.BackColor = Color.Black;
            pnlBorderColor.BorderStyle = BorderStyle.FixedSingle;
            pnlBorderColor.Cursor = Cursors.Hand;
            pnlBorderColor.Location = new Point(127, 82);
            pnlBorderColor.Name = "pnlBorderColor";
            pnlBorderColor.Size = new Size(42, 30);
            pnlBorderColor.TabIndex = 5;
            // 
            // btnBorderColor
            // 
            btnBorderColor.Location = new Point(175, 82);
            btnBorderColor.Name = "btnBorderColor";
            btnBorderColor.Size = new Size(79, 30);
            btnBorderColor.TabIndex = 6;
            btnBorderColor.Text = "Choose…";
            // 
            // lblLineStyle
            // 
            lblLineStyle.AutoSize = true;
            lblLineStyle.Font = new Font("Segoe UI", 9F);
            lblLineStyle.Location = new Point(20, 128);
            lblLineStyle.Name = "lblLineStyle";
            lblLineStyle.Size = new Size(75, 20);
            lblLineStyle.TabIndex = 7;
            lblLineStyle.Text = "Line Style:";
            // 
            // cboLineStyle
            // 
            cboLineStyle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboLineStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLineStyle.Font = new Font("Segoe UI", 9F);
            cboLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "DashDot" });
            cboLineStyle.Location = new Point(127, 123);
            cboLineStyle.Name = "cboLineStyle";
            cboLineStyle.Size = new Size(29, 28);
            cboLineStyle.TabIndex = 8;
            // 
            // lblLineWeight
            // 
            lblLineWeight.AutoSize = true;
            lblLineWeight.Font = new Font("Segoe UI", 9F);
            lblLineWeight.Location = new Point(20, 165);
            lblLineWeight.Name = "lblLineWeight";
            lblLineWeight.Size = new Size(90, 20);
            lblLineWeight.TabIndex = 9;
            lblLineWeight.Text = "Line Weight:";
            // 
            // cboLineWeight
            // 
            cboLineWeight.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboLineWeight.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLineWeight.Font = new Font("Segoe UI", 9F);
            cboLineWeight.Items.AddRange(new object[] { "0.25", "0.5", "1.0", "1.5", "2.0", "3.0" });
            cboLineWeight.Location = new Point(127, 162);
            cboLineWeight.Name = "cboLineWeight";
            cboLineWeight.Size = new Size(29, 28);
            cboLineWeight.TabIndex = 10;
            // 
            // chkVisible
            // 
            chkVisible.AutoSize = true;
            chkVisible.Font = new Font("Segoe UI", 9F);
            chkVisible.Location = new Point(20, 202);
            chkVisible.Name = "chkVisible";
            chkVisible.Size = new Size(75, 24);
            chkVisible.TabIndex = 11;
            chkVisible.Text = "Visible";
            // 
            // chkLocked
            // 
            chkLocked.AutoSize = true;
            chkLocked.Font = new Font("Segoe UI", 9F);
            chkLocked.Location = new Point(150, 202);
            chkLocked.Name = "chkLocked";
            chkLocked.Size = new Size(78, 24);
            chkLocked.TabIndex = 12;
            chkLocked.Text = "Locked";
            // 
            // chkSelectable
            // 
            chkSelectable.AutoSize = true;
            chkSelectable.Font = new Font("Segoe UI", 9F);
            chkSelectable.Location = new Point(20, 232);
            chkSelectable.Name = "chkSelectable";
            chkSelectable.Size = new Size(100, 24);
            chkSelectable.TabIndex = 13;
            chkSelectable.Text = "Selectable";
            // 
            // chkPrintable
            // 
            chkPrintable.AutoSize = true;
            chkPrintable.Font = new Font("Segoe UI", 9F);
            chkPrintable.Location = new Point(150, 232);
            chkPrintable.Name = "chkPrintable";
            chkPrintable.Size = new Size(90, 24);
            chkPrintable.TabIndex = 14;
            chkPrintable.Text = "Printable";
            // 
            // tabFill
            // 
            tabFill.Controls.Add(lblFillColor);
            tabFill.Controls.Add(pnlFillColor);
            tabFill.Controls.Add(btnFillColor);
            tabFill.Controls.Add(lblFillStyle);
            tabFill.Controls.Add(cboFillStyle);
            tabFill.Controls.Add(lblHatch);
            tabFill.Controls.Add(cboHatch);
            tabFill.Controls.Add(lblTransparency);
            tabFill.Controls.Add(lblTranspValue);
            tabFill.Controls.Add(trkTransparency);
            tabFill.Location = new Point(4, 29);
            tabFill.Name = "tabFill";
            tabFill.Padding = new Padding(10);
            tabFill.Size = new Size(221, 243);
            tabFill.TabIndex = 1;
            tabFill.Text = "Fill";
            tabFill.UseVisualStyleBackColor = true;
            // 
            // lblFillColor
            // 
            lblFillColor.AutoSize = true;
            lblFillColor.Font = new Font("Segoe UI", 9F);
            lblFillColor.Location = new Point(20, 15);
            lblFillColor.Name = "lblFillColor";
            lblFillColor.Size = new Size(71, 20);
            lblFillColor.TabIndex = 0;
            lblFillColor.Text = "Fill Color:";
            // 
            // pnlFillColor
            // 
            pnlFillColor.BackColor = Color.LightYellow;
            pnlFillColor.BorderStyle = BorderStyle.FixedSingle;
            pnlFillColor.Cursor = Cursors.Hand;
            pnlFillColor.Location = new Point(140, 12);
            pnlFillColor.Name = "pnlFillColor";
            pnlFillColor.Size = new Size(42, 30);
            pnlFillColor.TabIndex = 1;
            // 
            // btnFillColor
            // 
            btnFillColor.Location = new Point(188, 12);
            btnFillColor.Name = "btnFillColor";
            btnFillColor.Size = new Size(69, 30);
            btnFillColor.TabIndex = 2;
            btnFillColor.Text = "Choose…";
            // 
            // lblFillStyle
            // 
            lblFillStyle.AutoSize = true;
            lblFillStyle.Font = new Font("Segoe UI", 9F);
            lblFillStyle.Location = new Point(20, 51);
            lblFillStyle.Name = "lblFillStyle";
            lblFillStyle.Size = new Size(67, 20);
            lblFillStyle.TabIndex = 3;
            lblFillStyle.Text = "Fill Style:";
            // 
            // cboFillStyle
            // 
            cboFillStyle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboFillStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFillStyle.FlatStyle = FlatStyle.Flat;
            cboFillStyle.Font = new Font("Segoe UI", 9F);
            cboFillStyle.Items.AddRange(new object[] { "None", "Solid", "Hatched" });
            cboFillStyle.Location = new Point(140, 48);
            cboFillStyle.Name = "cboFillStyle";
            cboFillStyle.Size = new Size(0, 28);
            cboFillStyle.TabIndex = 4;
            // 
            // lblHatch
            // 
            lblHatch.AutoSize = true;
            lblHatch.Font = new Font("Segoe UI", 9F);
            lblHatch.Location = new Point(20, 85);
            lblHatch.Name = "lblHatch";
            lblHatch.Size = new Size(101, 20);
            lblHatch.TabIndex = 5;
            lblHatch.Text = "Hatch Pattern:";
            // 
            // cboHatch
            // 
            cboHatch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboHatch.DropDownStyle = ComboBoxStyle.DropDownList;
            cboHatch.Enabled = false;
            cboHatch.FlatStyle = FlatStyle.Flat;
            cboHatch.Font = new Font("Segoe UI", 9F);
            cboHatch.Items.AddRange(new object[] { "ANSI31", "ANSI32", "ANSI33", "ANSI34", "AR-BRSTD", "DOTS", "EARTH" });
            cboHatch.Location = new Point(140, 82);
            cboHatch.Name = "cboHatch";
            cboHatch.Size = new Size(0, 28);
            cboHatch.TabIndex = 6;
            // 
            // lblTransparency
            // 
            lblTransparency.AutoSize = true;
            lblTransparency.Font = new Font("Segoe UI", 9F);
            lblTransparency.Location = new Point(20, 117);
            lblTransparency.Name = "lblTransparency";
            lblTransparency.Size = new Size(98, 20);
            lblTransparency.TabIndex = 7;
            lblTransparency.Text = "Transparency:";
            lblTransparency.Click += lblTransparency_Click;
            // 
            // lblTranspValue
            // 
            lblTranspValue.AutoSize = true;
            lblTranspValue.Font = new Font("Segoe UI", 9F);
            lblTranspValue.Location = new Point(215, 153);
            lblTranspValue.Name = "lblTranspValue";
            lblTranspValue.Size = new Size(29, 20);
            lblTranspValue.TabIndex = 9;
            lblTranspValue.Text = "0%";
            // 
            // trkTransparency
            // 
            trkTransparency.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            trkTransparency.BackColor = SystemColors.ControlLightLight;
            trkTransparency.LargeChange = 1;
            trkTransparency.Location = new Point(140, 117);
            trkTransparency.Maximum = 100;
            trkTransparency.Name = "trkTransparency";
            trkTransparency.Size = new Size(65, 56);
            trkTransparency.TabIndex = 8;
            trkTransparency.TickFrequency = 10;
            // 
            // tabLabel
            // 
            tabLabel.Controls.Add(chkShowLabels);
            tabLabel.Controls.Add(lblFont);
            tabLabel.Controls.Add(txtFontName);
            tabLabel.Controls.Add(btnPickFont);
            tabLabel.Controls.Add(lblFontSize);
            tabLabel.Controls.Add(numFontSize);
            tabLabel.Controls.Add(lblLabelColor);
            tabLabel.Controls.Add(pnlLabelColor);
            tabLabel.Controls.Add(btnLabelColor);
            tabLabel.Controls.Add(lblLabelField);
            tabLabel.Controls.Add(cboLabelField);
            tabLabel.Location = new Point(4, 29);
            tabLabel.Name = "tabLabel";
            tabLabel.Padding = new Padding(10);
            tabLabel.Size = new Size(221, 243);
            tabLabel.TabIndex = 2;
            tabLabel.Text = "Labels";
            tabLabel.UseVisualStyleBackColor = true;
            // 
            // chkShowLabels
            // 
            chkShowLabels.AutoSize = true;
            chkShowLabels.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkShowLabels.Location = new Point(20, 22);
            chkShowLabels.Name = "chkShowLabels";
            chkShowLabels.Size = new Size(192, 24);
            chkShowLabels.TabIndex = 0;
            chkShowLabels.Text = "Show Labels on Canvas";
            // 
            // lblFont
            // 
            lblFont.AutoSize = true;
            lblFont.Font = new Font("Segoe UI", 9F);
            lblFont.Location = new Point(20, 59);
            lblFont.Name = "lblFont";
            lblFont.Size = new Size(41, 20);
            lblFont.TabIndex = 1;
            lblFont.Text = "Font:";
            // 
            // txtFontName
            // 
            txtFontName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFontName.Font = new Font("Segoe UI", 9F);
            txtFontName.Location = new Point(140, 46);
            txtFontName.Name = "txtFontName";
            txtFontName.ReadOnly = true;
            txtFontName.Size = new Size(65, 27);
            txtFontName.TabIndex = 2;
            // 
            // btnPickFont
            // 
            btnPickFont.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPickFont.Location = new Point(168, 46);
            btnPickFont.Name = "btnPickFont";
            btnPickFont.Size = new Size(40, 26);
            btnPickFont.TabIndex = 3;
            btnPickFont.Text = "…";
            // 
            // lblFontSize
            // 
            lblFontSize.AutoSize = true;
            lblFontSize.Font = new Font("Segoe UI", 9F);
            lblFontSize.Location = new Point(20, 93);
            lblFontSize.Name = "lblFontSize";
            lblFontSize.Size = new Size(72, 20);
            lblFontSize.TabIndex = 4;
            lblFontSize.Text = "Font Size:";
            // 
            // numFontSize
            // 
            numFontSize.Font = new Font("Segoe UI", 9F);
            numFontSize.Location = new Point(140, 80);
            numFontSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numFontSize.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numFontSize.Name = "numFontSize";
            numFontSize.Size = new Size(80, 27);
            numFontSize.TabIndex = 5;
            numFontSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblLabelColor
            // 
            lblLabelColor.AutoSize = true;
            lblLabelColor.Font = new Font("Segoe UI", 9F);
            lblLabelColor.Location = new Point(20, 126);
            lblLabelColor.Name = "lblLabelColor";
            lblLabelColor.Size = new Size(88, 20);
            lblLabelColor.TabIndex = 6;
            lblLabelColor.Text = "Label Color:";
            // 
            // pnlLabelColor
            // 
            pnlLabelColor.BackColor = Color.Black;
            pnlLabelColor.BorderStyle = BorderStyle.FixedSingle;
            pnlLabelColor.Cursor = Cursors.Hand;
            pnlLabelColor.Location = new Point(140, 114);
            pnlLabelColor.Name = "pnlLabelColor";
            pnlLabelColor.Size = new Size(42, 26);
            pnlLabelColor.TabIndex = 7;
            // 
            // btnLabelColor
            // 
            btnLabelColor.Location = new Point(188, 114);
            btnLabelColor.Name = "btnLabelColor";
            btnLabelColor.Size = new Size(75, 26);
            btnLabelColor.TabIndex = 8;
            btnLabelColor.Text = "Choose…";
            // 
            // lblLabelField
            // 
            lblLabelField.AutoSize = true;
            lblLabelField.Font = new Font("Segoe UI", 9F);
            lblLabelField.Location = new Point(20, 163);
            lblLabelField.Name = "lblLabelField";
            lblLabelField.Size = new Size(84, 20);
            lblLabelField.TabIndex = 9;
            lblLabelField.Text = "Show Field:";
            // 
            // cboLabelField
            // 
            cboLabelField.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboLabelField.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLabelField.FlatStyle = FlatStyle.Flat;
            cboLabelField.Font = new Font("Segoe UI", 9F);
            cboLabelField.Items.AddRange(new object[] { "ParcelNo", "MapSheetNo", "MapSheetParcelNo", "OwnerName", "AreaSqm", "CalculatedAreaSqm", "AreaRAPD", "LandUse", "PlotNumber", "AssignmentStatus", "RoadName", "RoadCode", "RoadStatus", "RoadType", "SurfaceType", "RoadWidth", "RightOfWayWidth", "RoadDescription", "BlockName", "BlockCode", "BlockLandUse", "BlockDepth", "BlockDepthGeometry", "BlockAreaSqm", "BlockAreaRAPD", "BlockAreaBKD", "BlockDescription", "SourceLayer", "LabelText", "ObjectDescription" });
            cboLabelField.Location = new Point(140, 150);
            cboLabelField.Name = "cboLabelField";
            cboLabelField.Size = new Size(0, 28);
            cboLabelField.TabIndex = 10;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(12, 12);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(81, 20);
            label1.TabIndex = 1;
            label1.Text = "Properties";
            // 
            // mainSplitContainer
            // 
            mainSplitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainSplitContainer.BorderStyle = BorderStyle.Fixed3D;
            mainSplitContainer.Location = new Point(0, 60);
            mainSplitContainer.Margin = new Padding(4);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(leftSplitContainer);
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(splitContainer3);
            mainSplitContainer.Size = new Size(1624, 559);
            mainSplitContainer.SplitterDistance = 276;
            mainSplitContainer.TabIndex = 3;
            mainSplitContainer.Visible = false;
            // 
            // splitContainer3
            // 
            splitContainer3.BorderStyle = BorderStyle.Fixed3D;
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.FixedPanel = FixedPanel.Panel2;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(mapCanvasControlMain);
            splitContainer3.Panel1.Controls.Add(tsCanvasTools);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(grpParcelObjProp);
            splitContainer3.Size = new Size(1344, 559);
            splitContainer3.SplitterDistance = 1020;
            splitContainer3.TabIndex = 1;
            // 
            // mapCanvasControlMain
            // 
            mapCanvasControlMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mapCanvasControlMain.BackColor = Color.White;
            mapCanvasControlMain.Location = new Point(0, 31);
            mapCanvasControlMain.Name = "mapCanvasControlMain";
            mapCanvasControlMain.Size = new Size(1016, 524);
            mapCanvasControlMain.TabIndex = 0;
            // 
            // tsCanvasTools
            // 
            tsCanvasTools.CanOverflow = false;
            tsCanvasTools.Font = new Font("Segoe UI", 9F);
            tsCanvasTools.GripStyle = ToolStripGripStyle.Hidden;
            tsCanvasTools.ImageScalingSize = new Size(20, 20);
            tsCanvasTools.Items.AddRange(new ToolStripItem[] { tsmExpandCollapseLeftPanel, toolStripSeparator10, mnuSelectTool, mnuDrawPoint, mnuDrawLine, mnuDrawPolyline, mnuDrawArc, mnuDrawRectangle, mnuDrawPolygon, mnuDrawCircle, mnuDrawText, toolStripSeparator17, lblCurrentDrawingLayer, cboCurrentDrawingLayer, mnuCanvasDebugOverlay, toolStripLabel1, tsmExpandCollapseRightPanel, toolStripSeparator11, mnuOrthoToggle, mnuOSnapToggle });
            tsCanvasTools.Location = new Point(0, 0);
            tsCanvasTools.Name = "tsCanvasTools";
            tsCanvasTools.Size = new Size(1016, 28);
            tsCanvasTools.TabIndex = 1;
            tsCanvasTools.ItemClicked += tsCanvasTools_ItemClicked;
            // 
            // tsmExpandCollapseLeftPanel
            // 
            tsmExpandCollapseLeftPanel.Checked = true;
            tsmExpandCollapseLeftPanel.CheckOnClick = true;
            tsmExpandCollapseLeftPanel.CheckState = CheckState.Checked;
            tsmExpandCollapseLeftPanel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsmExpandCollapseLeftPanel.Image = Properties.Resources.icons8_close_left_pane_50;
            tsmExpandCollapseLeftPanel.ImageTransparentColor = Color.Magenta;
            tsmExpandCollapseLeftPanel.Name = "tsmExpandCollapseLeftPanel";
            tsmExpandCollapseLeftPanel.Size = new Size(29, 25);
            tsmExpandCollapseLeftPanel.Text = "Collapse Left Panel";
            tsmExpandCollapseLeftPanel.Click += tsmExpandCollapseLeftPanel_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(6, 28);
            // 
            // mnuSelectTool
            // 
            mnuSelectTool.Checked = true;
            mnuSelectTool.CheckOnClick = true;
            mnuSelectTool.CheckState = CheckState.Checked;
            mnuSelectTool.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuSelectTool.Image = Properties.Resources.selection_Tool;
            mnuSelectTool.ImageTransparentColor = Color.Magenta;
            mnuSelectTool.Name = "mnuSelectTool";
            mnuSelectTool.Size = new Size(29, 25);
            mnuSelectTool.Text = "Select";
            mnuSelectTool.Click += mnuSelectTool_Click;
            // 
            // mnuDrawPoint
            // 
            mnuDrawPoint.CheckOnClick = true;
            mnuDrawPoint.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawPoint.Image = Properties.Resources.Point;
            mnuDrawPoint.ImageTransparentColor = Color.Magenta;
            mnuDrawPoint.Name = "mnuDrawPoint";
            mnuDrawPoint.Size = new Size(29, 25);
            mnuDrawPoint.Text = "Point";
            mnuDrawPoint.ToolTipText = "Draw Point";
            mnuDrawPoint.Click += mnuDrawPoint_Click;
            // 
            // mnuDrawLine
            // 
            mnuDrawLine.CheckOnClick = true;
            mnuDrawLine.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawLine.Image = Properties.Resources.icons8_line_24;
            mnuDrawLine.ImageTransparentColor = Color.Magenta;
            mnuDrawLine.Name = "mnuDrawLine";
            mnuDrawLine.Size = new Size(29, 25);
            mnuDrawLine.Text = "Draw Line";
            mnuDrawLine.Click += mnuDrawLine_Click;
            // 
            // mnuDrawPolyline
            // 
            mnuDrawPolyline.CheckOnClick = true;
            mnuDrawPolyline.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawPolyline.Image = Properties.Resources.icons8_polyline_24;
            mnuDrawPolyline.ImageTransparentColor = Color.Magenta;
            mnuDrawPolyline.Name = "mnuDrawPolyline";
            mnuDrawPolyline.Size = new Size(29, 25);
            mnuDrawPolyline.Text = "Draw Polyline";
            mnuDrawPolyline.Click += mnuDrawPolyline_Click;
            // 
            // mnuDrawArc
            // 
            mnuDrawArc.CheckOnClick = true;
            mnuDrawArc.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawArc.Image = Properties.Resources.arc;
            mnuDrawArc.ImageTransparentColor = Color.Magenta;
            mnuDrawArc.Name = "mnuDrawArc";
            mnuDrawArc.Size = new Size(29, 25);
            mnuDrawArc.Text = "Arc";
            mnuDrawArc.ToolTipText = "Draw 3-Point Arc";
            mnuDrawArc.Click += mnuDrawArc_Click;
            // 
            // mnuDrawRectangle
            // 
            mnuDrawRectangle.CheckOnClick = true;
            mnuDrawRectangle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawRectangle.Image = Properties.Resources.rectangle1;
            mnuDrawRectangle.ImageTransparentColor = Color.Magenta;
            mnuDrawRectangle.Name = "mnuDrawRectangle";
            mnuDrawRectangle.Size = new Size(29, 25);
            mnuDrawRectangle.Text = "Draw Rectangle";
            mnuDrawRectangle.Click += mnuDrawRectangle_Click;
            // 
            // mnuDrawPolygon
            // 
            mnuDrawPolygon.CheckOnClick = true;
            mnuDrawPolygon.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawPolygon.Image = Properties.Resources.icons8_polygon_24__1_;
            mnuDrawPolygon.ImageTransparentColor = Color.Magenta;
            mnuDrawPolygon.Name = "mnuDrawPolygon";
            mnuDrawPolygon.Size = new Size(29, 25);
            mnuDrawPolygon.Text = "Draw Polygon";
            mnuDrawPolygon.Click += mnuDrawPolygon_Click;
            // 
            // mnuDrawCircle
            // 
            mnuDrawCircle.CheckOnClick = true;
            mnuDrawCircle.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawCircle.Image = Properties.Resources.circle1;
            mnuDrawCircle.ImageTransparentColor = Color.Magenta;
            mnuDrawCircle.Name = "mnuDrawCircle";
            mnuDrawCircle.Size = new Size(29, 25);
            mnuDrawCircle.Text = "Draw Circle";
            mnuDrawCircle.Click += mnuDrawCircle_Click;
            // 
            // mnuDrawText
            // 
            mnuDrawText.CheckOnClick = true;
            mnuDrawText.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuDrawText.Image = Properties.Resources.icons8_aa_64;
            mnuDrawText.ImageTransparentColor = Color.Magenta;
            mnuDrawText.Name = "mnuDrawText";
            mnuDrawText.Size = new Size(29, 25);
            mnuDrawText.Text = "Text";
            mnuDrawText.ToolTipText = "Draw Single Line Text";
            mnuDrawText.Click += mnuDrawText_Click;
            // 
            // toolStripSeparator17
            // 
            toolStripSeparator17.Name = "toolStripSeparator17";
            toolStripSeparator17.Size = new Size(6, 28);
            // 
            // lblCurrentDrawingLayer
            // 
            lblCurrentDrawingLayer.Name = "lblCurrentDrawingLayer";
            lblCurrentDrawingLayer.Size = new Size(89, 25);
            lblCurrentDrawingLayer.Text = "Active Layer";
            // 
            // cboCurrentDrawingLayer
            // 
            cboCurrentDrawingLayer.BackColor = SystemColors.ControlLight;
            cboCurrentDrawingLayer.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCurrentDrawingLayer.FlatStyle = FlatStyle.System;
            cboCurrentDrawingLayer.Name = "cboCurrentDrawingLayer";
            cboCurrentDrawingLayer.Size = new Size(180, 28);
            cboCurrentDrawingLayer.ToolTipText = "Current drawing/markup layer";
            cboCurrentDrawingLayer.SelectedIndexChanged += cboCurrentDrawingLayer_SelectedIndexChanged;
            cboCurrentDrawingLayer.Click += cboCurrentDrawingLayer_Click;
            // 
            // mnuCanvasDebugOverlay
            // 
            mnuCanvasDebugOverlay.CheckOnClick = true;
            mnuCanvasDebugOverlay.DisplayStyle = ToolStripItemDisplayStyle.Text;
            mnuCanvasDebugOverlay.ImageTransparentColor = Color.Magenta;
            mnuCanvasDebugOverlay.Name = "mnuCanvasDebugOverlay";
            mnuCanvasDebugOverlay.Size = new Size(58, 25);
            mnuCanvasDebugOverlay.Text = "Debug";
            mnuCanvasDebugOverlay.ToolTipText = "Show map canvas debug overlay";
            mnuCanvasDebugOverlay.Click += mnuCanvasDebugOverlay_Click;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(0, 25);
            // 
            // tsmExpandCollapseRightPanel
            // 
            tsmExpandCollapseRightPanel.Alignment = ToolStripItemAlignment.Right;
            tsmExpandCollapseRightPanel.Checked = true;
            tsmExpandCollapseRightPanel.CheckOnClick = true;
            tsmExpandCollapseRightPanel.CheckState = CheckState.Checked;
            tsmExpandCollapseRightPanel.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsmExpandCollapseRightPanel.Image = Properties.Resources.icons8_close_right_pane_50;
            tsmExpandCollapseRightPanel.ImageTransparentColor = Color.Magenta;
            tsmExpandCollapseRightPanel.Name = "tsmExpandCollapseRightPanel";
            tsmExpandCollapseRightPanel.Size = new Size(29, 25);
            tsmExpandCollapseRightPanel.Text = "Collapse Right Panel";
            tsmExpandCollapseRightPanel.Click += tsmExpandCollapseRightPanel_Click;
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(6, 28);
            // 
            // mnuOrthoToggle
            // 
            mnuOrthoToggle.Alignment = ToolStripItemAlignment.Right;
            mnuOrthoToggle.CheckOnClick = true;
            mnuOrthoToggle.DisplayStyle = ToolStripItemDisplayStyle.Text;
            mnuOrthoToggle.ImageTransparentColor = Color.Magenta;
            mnuOrthoToggle.Name = "mnuOrthoToggle";
            mnuOrthoToggle.Size = new Size(76, 25);
            mnuOrthoToggle.Text = "Ortho(F8)";
            mnuOrthoToggle.ToolTipText = "Ortho Mode (F8)";
            mnuOrthoToggle.Click += mnuOrthoToggle_Click;
            // 
            // mnuOSnapToggle
            // 
            mnuOSnapToggle.Alignment = ToolStripItemAlignment.Right;
            mnuOSnapToggle.Checked = true;
            mnuOSnapToggle.CheckOnClick = true;
            mnuOSnapToggle.CheckState = CheckState.Checked;
            mnuOSnapToggle.DisplayStyle = ToolStripItemDisplayStyle.Text;
            mnuOSnapToggle.ImageTransparentColor = Color.Magenta;
            mnuOSnapToggle.Name = "mnuOSnapToggle";
            mnuOSnapToggle.Size = new Size(86, 25);
            mnuOSnapToggle.Text = "OSnap (F3)";
            mnuOSnapToggle.ToolTipText = "Object Snap (F3)";
            mnuOSnapToggle.Click += mnuOSnapToggle_Click;
            // 
            // grpParcelObjProp
            // 
            grpParcelObjProp.Controls.Add(btnSelectFromRecords);
            grpParcelObjProp.Controls.Add(btnConfigureParcelProperties);
            grpParcelObjProp.Controls.Add(cboSelectedPropertyObject);
            grpParcelObjProp.Controls.Add(btnPreviousSelectedPropertyObject);
            grpParcelObjProp.Controls.Add(btnNextSelectedPropertyObject);
            grpParcelObjProp.Controls.Add(dgvParcelObjProperty);
            grpParcelObjProp.Dock = DockStyle.Fill;
            grpParcelObjProp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpParcelObjProp.Location = new Point(0, 0);
            grpParcelObjProp.Margin = new Padding(4);
            grpParcelObjProp.Name = "grpParcelObjProp";
            grpParcelObjProp.Padding = new Padding(4);
            grpParcelObjProp.RightToLeft = RightToLeft.No;
            grpParcelObjProp.Size = new Size(316, 555);
            grpParcelObjProp.TabIndex = 1;
            grpParcelObjProp.TabStop = false;
            grpParcelObjProp.Text = "Properties";
            // 
            // btnSelectFromRecords
            // 
            btnSelectFromRecords.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnSelectFromRecords.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSelectFromRecords.Location = new Point(8, 27);
            btnSelectFromRecords.Name = "btnSelectFromRecords";
            btnSelectFromRecords.Size = new Size(216, 29);
            btnSelectFromRecords.TabIndex = 1;
            btnSelectFromRecords.Text = "Select from Records...";
            btnSelectFromRecords.UseVisualStyleBackColor = true;
            btnSelectFromRecords.Click += btnSelectFromRecords_Click;
            // 
            // btnConfigureParcelProperties
            // 
            btnConfigureParcelProperties.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfigureParcelProperties.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnConfigureParcelProperties.Location = new Point(230, 27);
            btnConfigureParcelProperties.Name = "btnConfigureParcelProperties";
            btnConfigureParcelProperties.Size = new Size(76, 29);
            btnConfigureParcelProperties.TabIndex = 2;
            btnConfigureParcelProperties.Text = "Fields...";
            btnConfigureParcelProperties.UseVisualStyleBackColor = true;
            btnConfigureParcelProperties.Click += btnConfigureParcelProperties_Click;
            // 
            // cboSelectedPropertyObject
            // 
            cboSelectedPropertyObject.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cboSelectedPropertyObject.DropDownStyle = ComboBoxStyle.DropDownList;
            cboSelectedPropertyObject.Enabled = false;
            cboSelectedPropertyObject.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cboSelectedPropertyObject.FormattingEnabled = true;
            cboSelectedPropertyObject.Location = new Point(8, 62);
            cboSelectedPropertyObject.Name = "cboSelectedPropertyObject";
            cboSelectedPropertyObject.Size = new Size(216, 28);
            cboSelectedPropertyObject.TabIndex = 3;
            cboSelectedPropertyObject.SelectedIndexChanged += cboSelectedPropertyObject_SelectedIndexChanged;
            cboSelectedPropertyObject.SelectionChangeCommitted += cboSelectedPropertyObject_SelectionChangeCommitted;
            cboSelectedPropertyObject.DropDownClosed += cboSelectedPropertyObject_DropDownClosed;
            // 
            // btnPreviousSelectedPropertyObject
            // 
            btnPreviousSelectedPropertyObject.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPreviousSelectedPropertyObject.Enabled = false;
            btnPreviousSelectedPropertyObject.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnPreviousSelectedPropertyObject.Location = new Point(230, 62);
            btnPreviousSelectedPropertyObject.Name = "btnPreviousSelectedPropertyObject";
            btnPreviousSelectedPropertyObject.Size = new Size(36, 28);
            btnPreviousSelectedPropertyObject.TabIndex = 4;
            btnPreviousSelectedPropertyObject.Text = "<";
            btnPreviousSelectedPropertyObject.UseVisualStyleBackColor = true;
            btnPreviousSelectedPropertyObject.Click += btnPreviousSelectedPropertyObject_Click;
            // 
            // btnNextSelectedPropertyObject
            // 
            btnNextSelectedPropertyObject.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNextSelectedPropertyObject.Enabled = false;
            btnNextSelectedPropertyObject.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnNextSelectedPropertyObject.Location = new Point(270, 62);
            btnNextSelectedPropertyObject.Name = "btnNextSelectedPropertyObject";
            btnNextSelectedPropertyObject.Size = new Size(36, 28);
            btnNextSelectedPropertyObject.TabIndex = 5;
            btnNextSelectedPropertyObject.Text = ">";
            btnNextSelectedPropertyObject.UseVisualStyleBackColor = true;
            btnNextSelectedPropertyObject.Click += btnNextSelectedPropertyObject_Click;
            // 
            // dgvParcelObjProperty
            // 
            dgvParcelObjProperty.AllowUserToAddRows = false;
            dgvParcelObjProperty.AllowUserToDeleteRows = false;
            dgvParcelObjProperty.AllowUserToResizeRows = false;
            dgvParcelObjProperty.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvParcelObjProperty.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvParcelObjProperty.BackgroundColor = Color.White;
            dgvParcelObjProperty.BorderStyle = BorderStyle.None;
            dgvParcelObjProperty.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvParcelObjProperty.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(242, 242, 242);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.Black;
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(242, 242, 242);
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvParcelObjProperty.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvParcelObjProperty.ColumnHeadersHeight = 28;
            dgvParcelObjProperty.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvParcelObjProperty.Columns.AddRange(new DataGridViewColumn[] { colParcelPropertyField, colParcelPropertyValue });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(235, 235, 235);
            dataGridViewCellStyle2.SelectionForeColor = Color.Black;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvParcelObjProperty.DefaultCellStyle = dataGridViewCellStyle2;
            dgvParcelObjProperty.EnableHeadersVisualStyles = false;
            dgvParcelObjProperty.GridColor = Color.FromArgb(230, 230, 230);
            dgvParcelObjProperty.Location = new Point(4, 96);
            dgvParcelObjProperty.MultiSelect = false;
            dgvParcelObjProperty.Name = "dgvParcelObjProperty";
            dgvParcelObjProperty.ReadOnly = true;
            dgvParcelObjProperty.RowHeadersVisible = false;
            dgvParcelObjProperty.RowHeadersWidth = 51;
            dgvParcelObjProperty.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParcelObjProperty.Size = new Size(308, 455);
            dgvParcelObjProperty.TabIndex = 4;
            // 
            // colParcelPropertyField
            // 
            colParcelPropertyField.HeaderText = "Property";
            colParcelPropertyField.MinimumWidth = 96;
            colParcelPropertyField.Name = "colParcelPropertyField";
            colParcelPropertyField.ReadOnly = true;
            colParcelPropertyField.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParcelPropertyField.Width = 118;
            // 
            // colParcelPropertyValue
            // 
            colParcelPropertyValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colParcelPropertyValue.HeaderText = "Value";
            colParcelPropertyValue.MinimumWidth = 120;
            colParcelPropertyValue.Name = "colParcelPropertyValue";
            colParcelPropertyValue.ReadOnly = true;
            colParcelPropertyValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // statusCanvas
            // 
            statusCanvas.AutoSize = false;
            statusCanvas.BackColor = SystemColors.ControlLightLight;
            statusCanvas.ForeColor = SystemColors.ControlText;
            statusCanvas.ImageScalingSize = new Size(20, 20);
            statusCanvas.Items.AddRange(new ToolStripItem[] { lblProjectName, tsStatusSep1, lblActiveTool, lblStatusMessage, hostProgressBarHost, lblOperationProgressStatus, lblScale, lblCanvasCoordinates });
            statusCanvas.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            statusCanvas.Location = new Point(0, 623);
            statusCanvas.Name = "statusCanvas";
            statusCanvas.RightToLeft = RightToLeft.No;
            statusCanvas.Size = new Size(1624, 38);
            statusCanvas.TabIndex = 6;
            statusCanvas.Text = "Map Canvas Status";
            statusCanvas.ItemClicked += statusCanvas_ItemClicked;
            // 
            // lblProjectName
            // 
            lblProjectName.AutoSize = false;
            lblProjectName.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProjectName.ForeColor = SystemColors.GrayText;
            lblProjectName.Margin = new Padding(4, 3, 6, 2);
            lblProjectName.Name = "lblProjectName";
            lblProjectName.Size = new Size(270, 33);
            lblProjectName.Text = "● No Project";
            lblProjectName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tsStatusSep1
            // 
            tsStatusSep1.Name = "tsStatusSep1";
            tsStatusSep1.Size = new Size(6, 38);
            // 
            // lblActiveTool
            // 
            lblActiveTool.AutoSize = false;
            lblActiveTool.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblActiveTool.Margin = new Padding(4, 3, 0, 2);
            lblActiveTool.Name = "lblActiveTool";
            lblActiveTool.Size = new Size(250, 37);
            lblActiveTool.Text = "Active Tool: Select";
            lblActiveTool.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblStatusMessage
            // 
            lblStatusMessage.AutoSize = false;
            lblStatusMessage.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblStatusMessage.ForeColor = SystemColors.ControlText;
            lblStatusMessage.Margin = new Padding(6, 3, 0, 2);
            lblStatusMessage.Name = "lblStatusMessage";
            lblStatusMessage.Size = new Size(720, 37);
            lblStatusMessage.Spring = true;
            lblStatusMessage.Text = "Ready";
            lblStatusMessage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // hostProgressBarHost
            // 
            hostProgressBarHost.Alignment = ToolStripItemAlignment.Right;
            hostProgressBarHost.Name = "hostProgressBarHost";
            hostProgressBarHost.Size = new Size(154, 36);
            hostProgressBarHost.Visible = false;
            hostProgressBarHost.Click += hostProgressBarHost_Click;
            // 
            // lblOperationProgressStatus
            // 
            lblOperationProgressStatus.Alignment = ToolStripItemAlignment.Right;
            lblOperationProgressStatus.AutoSize = false;
            lblOperationProgressStatus.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblOperationProgressStatus.ForeColor = SystemColors.ControlText;
            lblOperationProgressStatus.Name = "lblOperationProgressStatus";
            lblOperationProgressStatus.Size = new Size(500, 36);
            lblOperationProgressStatus.TextAlign = ContentAlignment.MiddleRight;
            lblOperationProgressStatus.Visible = false;
            // 
            // lblScale
            // 
            lblScale.Alignment = ToolStripItemAlignment.Right;
            lblScale.AutoSize = false;
            lblScale.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblScale.Margin = new Padding(0, 3, 4, 2);
            lblScale.Name = "lblScale";
            lblScale.Size = new Size(132, 33);
            lblScale.Text = "Scale: 1:—";
            lblScale.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblCanvasCoordinates
            // 
            lblCanvasCoordinates.Alignment = ToolStripItemAlignment.Right;
            lblCanvasCoordinates.AutoSize = false;
            lblCanvasCoordinates.BorderSides = ToolStripStatusLabelBorderSides.Left;
            lblCanvasCoordinates.BorderStyle = Border3DStyle.RaisedOuter;
            lblCanvasCoordinates.ForeColor = SystemColors.ControlText;
            lblCanvasCoordinates.Margin = new Padding(0, 3, 6, 2);
            lblCanvasCoordinates.Name = "lblCanvasCoordinates";
            lblCanvasCoordinates.Size = new Size(285, 33);
            lblCanvasCoordinates.Text = "E: 0.0000    N: 0.0000";
            lblCanvasCoordinates.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblStatusSpacer
            // 
            lblStatusSpacer.Name = "lblStatusSpacer";
            lblStatusSpacer.Size = new Size(0, 32);
            lblStatusSpacer.Spring = true;
            // 
            // tsStatusSep2
            // 
            tsStatusSep2.Name = "tsStatusSep2";
            tsStatusSep2.Size = new Size(6, 38);
            // 
            // mnuNewProject
            // 
            mnuNewProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuNewProject.Image = Properties.Resources.icons8_file_501;
            mnuNewProject.ImageTransparentColor = Color.Magenta;
            mnuNewProject.Name = "mnuNewProject";
            mnuNewProject.Size = new Size(29, 24);
            mnuNewProject.Text = "New Project";
            // 
            // mnuOpenProject
            // 
            mnuOpenProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuOpenProject.Image = Properties.Resources.icons8_open_folder_50;
            mnuOpenProject.ImageTransparentColor = Color.Magenta;
            mnuOpenProject.Name = "mnuOpenProject";
            mnuOpenProject.Size = new Size(29, 24);
            mnuOpenProject.Text = "Open Project";
            // 
            // mnuSaveProject
            // 
            mnuSaveProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuSaveProject.Image = Properties.Resources.icons8_save_50;
            mnuSaveProject.ImageTransparentColor = Color.Magenta;
            mnuSaveProject.Name = "mnuSaveProject";
            mnuSaveProject.Size = new Size(29, 24);
            mnuSaveProject.Text = "Save Project";
            // 
            // mnuSaveAsProject
            // 
            mnuSaveAsProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuSaveAsProject.Image = Properties.Resources.icons8_save_as_50;
            mnuSaveAsProject.ImageTransparentColor = Color.Magenta;
            mnuSaveAsProject.Name = "mnuSaveAsProject";
            mnuSaveAsProject.Size = new Size(29, 24);
            mnuSaveAsProject.Text = "Save As Project";
            mnuSaveAsProject.Click += mnuSaveAsProject_Click;
            // 
            // mnuBackup
            // 
            mnuBackup.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuBackup.Image = Properties.Resources.icons8_database_export_502;
            mnuBackup.ImageTransparentColor = Color.Magenta;
            mnuBackup.Name = "mnuBackup";
            mnuBackup.Size = new Size(29, 24);
            mnuBackup.Text = "Backup Project";
            // 
            // mnuRestoreBackup
            // 
            mnuRestoreBackup.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuRestoreBackup.Image = Properties.Resources.icons8_data_backup_503;
            mnuRestoreBackup.ImageTransparentColor = Color.Magenta;
            mnuRestoreBackup.Name = "mnuRestoreBackup";
            mnuRestoreBackup.Size = new Size(29, 24);
            mnuRestoreBackup.Text = "Restore from Backup";
            // 
            // mnuCloseProject
            // 
            mnuCloseProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuCloseProject.Image = Properties.Resources.icons8_close_501;
            mnuCloseProject.ImageTransparentColor = Color.Magenta;
            mnuCloseProject.Name = "mnuCloseProject";
            mnuCloseProject.Size = new Size(29, 24);
            mnuCloseProject.Text = "Close Project";
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(6, 27);
            // 
            // toolStripButton3
            // 
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(6, 27);
            // 
            // mnuProjectInfo
            // 
            mnuProjectInfo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuProjectInfo.Image = Properties.Resources.icons8_info_squared_501;
            mnuProjectInfo.ImageTransparentColor = Color.Magenta;
            mnuProjectInfo.Name = "mnuProjectInfo";
            mnuProjectInfo.Size = new Size(29, 24);
            mnuProjectInfo.Text = "Project Information";
            // 
            // mnuProjectSettings
            // 
            mnuProjectSettings.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuProjectSettings.Image = Properties.Resources.icons8_wrench_501;
            mnuProjectSettings.ImageTransparentColor = Color.Magenta;
            mnuProjectSettings.Name = "mnuProjectSettings";
            mnuProjectSettings.Size = new Size(29, 24);
            mnuProjectSettings.Text = "Project Setting";
            // 
            // btnOriginalScenarioSummary
            // 
            btnOriginalScenarioSummary.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnOriginalScenarioSummary.ImageTransparentColor = Color.Magenta;
            btnOriginalScenarioSummary.Name = "btnOriginalScenarioSummary";
            btnOriginalScenarioSummary.Size = new Size(126, 24);
            btnOriginalScenarioSummary.Text = "Original Scenario";
            btnOriginalScenarioSummary.ToolTipText = "Open Original Scenario Summary";
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(6, 27);
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(6, 27);
            // 
            // mnuUndo
            // 
            mnuUndo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuUndo.Image = Properties.Resources.icons8_undo_502;
            mnuUndo.ImageTransparentColor = Color.Magenta;
            mnuUndo.Name = "mnuUndo";
            mnuUndo.Size = new Size(29, 24);
            mnuUndo.Text = "Undo";
            // 
            // mnuRedo
            // 
            mnuRedo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuRedo.Image = Properties.Resources.icons8_redo_502;
            mnuRedo.ImageTransparentColor = Color.Magenta;
            mnuRedo.Name = "mnuRedo";
            mnuRedo.Size = new Size(29, 24);
            mnuRedo.Text = "Redo";
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(6, 27);
            // 
            // mnuPan
            // 
            mnuPan.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuPan.Image = Properties.Resources.pngegg;
            mnuPan.ImageTransparentColor = Color.Magenta;
            mnuPan.Name = "mnuPan";
            mnuPan.Size = new Size(29, 24);
            mnuPan.Text = "Pan";
            mnuPan.Click += mnuPan_Click;
            // 
            // mnuZoomIn
            // 
            mnuZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomIn.Image = Properties.Resources.icons8_zoom_in_502;
            mnuZoomIn.ImageTransparentColor = Color.Magenta;
            mnuZoomIn.Name = "mnuZoomIn";
            mnuZoomIn.Size = new Size(29, 24);
            mnuZoomIn.Text = "Zoom In";
            // 
            // mnuZoomOut
            // 
            mnuZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomOut.Image = Properties.Resources.icons8_zoom_out_502;
            mnuZoomOut.ImageTransparentColor = Color.Magenta;
            mnuZoomOut.Name = "mnuZoomOut";
            mnuZoomOut.Size = new Size(29, 24);
            mnuZoomOut.Text = "Zoom Out ";
            // 
            // mnuZoomExtent
            // 
            mnuZoomExtent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomExtent.Image = Properties.Resources.icons8_zoom_to_extents_502;
            mnuZoomExtent.ImageTransparentColor = Color.Magenta;
            mnuZoomExtent.Name = "mnuZoomExtent";
            mnuZoomExtent.Size = new Size(29, 24);
            mnuZoomExtent.Text = "Zoom to Extents";
            // 
            // mnuZoomWindow
            // 
            mnuZoomWindow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomWindow.Image = Properties.Resources.icons8_zoom_to_fit_502;
            mnuZoomWindow.ImageTransparentColor = Color.Magenta;
            mnuZoomWindow.Name = "mnuZoomWindow";
            mnuZoomWindow.Size = new Size(29, 24);
            mnuZoomWindow.Text = "Zoom Window";
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(6, 27);
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(6, 27);
            // 
            // tsProjectMenu
            // 
            tsProjectMenu.Font = new Font("Segoe UI", 9F);
            tsProjectMenu.ImageScalingSize = new Size(20, 20);
            tsProjectMenu.Items.AddRange(new ToolStripItem[] { mnuNewProject, mnuOpenProject, mnuSaveProject, mnuSaveAsProject, mnuBackup, mnuRestoreBackup, mnuCloseProject, toolStripSeparator9, toolStripButton3, mnuProjectInfo, mnuProjectSettings, btnOriginalScenarioSummary, toolStripSeparator12, toolStripSeparator13, mnuUndo, mnuRedo, toolStripSeparator14, mnuPan, mnuZoomIn, mnuZoomOut, mnuZoomExtent, mnuZoomWindow, toolStripSeparator15, toolStripSeparator16 });
            tsProjectMenu.Location = new Point(0, 28);
            tsProjectMenu.Name = "tsProjectMenu";
            tsProjectMenu.Size = new Size(1624, 27);
            tsProjectMenu.TabIndex = 4;
            tsProjectMenu.Text = "Project Menu";
            // 
            // toolStripSeparator22
            // 
            toolStripSeparator22.Name = "toolStripSeparator22";
            toolStripSeparator22.Size = new Size(254, 6);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(257, 26);
            toolStripMenuItem2.Text = "Existing/Proposed Roads";
            // 
            // frmMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1624, 661);
            Controls.Add(statusCanvas);
            Controls.Add(tsProjectMenu);
            Controls.Add(mainSplitContainer);
            Controls.Add(mainMenuStrip);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(2);
            Name = "frmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "RePlot";
            WindowState = FormWindowState.Maximized;
            Load += frmMain_Load;
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            leftSplitContainer.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)leftSplitContainer).EndInit();
            leftSplitContainer.ResumeLayout(false);
            grpLayer.ResumeLayout(false);
            tabProperties.ResumeLayout(false);
            tabGeneral.ResumeLayout(false);
            tabGeneral.PerformLayout();
            tabFill.ResumeLayout(false);
            tabFill.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trkTransparency).EndInit();
            tabLabel.ResumeLayout(false);
            tabLabel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numFontSize).EndInit();
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            tsCanvasTools.ResumeLayout(false);
            tsCanvasTools.PerformLayout();
            grpParcelObjProp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvParcelObjProperty).EndInit();
            statusCanvas.ResumeLayout(false);
            statusCanvas.PerformLayout();
            tsProjectMenu.ResumeLayout(false);
            tsProjectMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem contributionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replottingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startReplotWorkspaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem validationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuAreaConverterTool;
        private System.Windows.Forms.ToolStripMenuItem helToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmNewProject;
        private System.Windows.Forms.ToolStripMenuItem tsmOpenProject;
        private System.Windows.Forms.ToolStripMenuItem tsmSave;
        private System.Windows.Forms.ToolStripMenuItem tsmSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem tsmExit;
        private System.Windows.Forms.ToolStripMenuItem tsmProjectInformation;
        private System.Windows.Forms.ToolStripMenuItem tsmProjectSetting;
        private System.Windows.Forms.ToolStripMenuItem cadastralDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmCloseProject;
        private System.Windows.Forms.ToolStripMenuItem tsmBackupProject;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem tsmRecentProjects;
        private ToolStripMenuItem tsmRestoreBackup;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem importDataToolStripMenuItem;
        private ToolStripMenuItem viewEditRecordsToolStripMenuItem;
        private ToolStripMenuItem landOwnerDataToolStripMenuItem;
        private ToolStripMenuItem importDataToolStripMenuItem1;
        private ToolStripMenuItem importToolStripMenuItem;
        private ToolStripMenuItem assignmentToolStripMenuItem;
        private ToolStripMenuItem projectBoundaryAssignmentToolStripMenuItem;
        private ToolStripMenuItem contributionSettingsToolStripMenuItem;
        private ToolStripMenuItem calculateContributionToolStripMenuItem;
        private ToolStripMenuItem contributionSummaryToolStripMenuItem;
        private ToolStripMenuItem parcelAdjustmentToolStripMenuItem;
        private ToolStripMenuItem roadNetworkToolStripMenuItem;
        private ToolStripMenuItem validateOwnershipRecordsToolStripMenuItem;
        private ToolStripMenuItem validateSpatialDataToolStripMenuItem;
        private ToolStripMenuItem validationIssuesToolStripMenuItem;
        private ToolStripMenuItem ownerParcelReportToolStripMenuItem;
        private ToolStripMenuItem contributionReportToolStripMenuItem;
        private ToolStripMenuItem exportDataToolStripMenuItem;
        private ToolStripMenuItem userGuideToolStripMenuItem;
        private ToolStripMenuItem aboutRePlotToolStripMenuItem;
        private ToolStripMenuItem importCadastralDataDXFDWGShapefileToolStripMenuItem;
        private ToolStripMenuItem viewEditRecordToolStripMenuItem;
        private ToolStripMenuItem baseMapsToolStripMenuItem;
        private ToolStripMenuItem ImportProjectBoundaryDXFDWGToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem ImportParcelOwnerShipRecords;
        private SplitContainer leftSplitContainer;
        private GroupBox grpLayer;
        private TreeView treeViewLayers;
        private TabControl tabProperties;
        private TabPage tabGeneral;
        private Label lblLayerName;
        private TextBox txtLayerName;
        private Label lblLayerType;
        private ComboBox cboLayerType;
        private Label lblBorderColor;
        private Panel pnlBorderColor;
        private Button btnBorderColor;
        private Label lblLineStyle;
        private ComboBox cboLineStyle;
        private Label lblLineWeight;
        private ComboBox cboLineWeight;
        private CheckBox chkVisible;
        private CheckBox chkLocked;
        private CheckBox chkSelectable;
        private CheckBox chkPrintable;
        private TabPage tabFill;
        private Label lblFillColor;
        private Panel pnlFillColor;
        private Button btnFillColor;
        private Label lblFillStyle;
        private ComboBox cboFillStyle;
        private Label lblHatch;
        private ComboBox cboHatch;
        private Label lblTransparency;
        private Label lblTranspValue;
        private TrackBar trkTransparency;
        private TabPage tabLabel;
        private CheckBox chkShowLabels;
        private Label lblFont;
        private TextBox txtFontName;
        private Button btnPickFont;
        private Label lblFontSize;
        private NumericUpDown numFontSize;
        private Label lblLabelColor;
        private Panel pnlLabelColor;
        private Button btnLabelColor;
        private Label lblLabelField;
        private ComboBox cboLabelField;
        private Label label1;
        private SplitContainer mainSplitContainer;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator toolStripSeparator8;
        private SplitContainer splitContainer3;
        private GroupBox grpParcelObjProp;
        private Button btnSelectFromRecords;
        private ComboBox cboSelectedPropertyObject;
        private Button btnPreviousSelectedPropertyObject;
        private Button btnNextSelectedPropertyObject;
        private DataGridView dgvParcelObjProperty;
        private DataGridViewTextBoxColumn colParcelPropertyField;
        private DataGridViewTextBoxColumn colParcelPropertyValue;
        private ToolStripButton mnuNewProject;
        private ToolStripButton mnuOpenProject;
        private ToolStripButton mnuSaveProject;
        private ToolStripButton mnuSaveAsProject;
        private ToolStripButton mnuBackup;
        private ToolStripButton mnuRestoreBackup;
        private ToolStripButton mnuCloseProject;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripSeparator toolStripButton3;
        private ToolStripButton mnuProjectInfo;
        private ToolStripButton mnuProjectSettings;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripButton mnuUndo;
        private ToolStripButton mnuRedo;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripButton mnuPan;
        private ToolStripButton mnuZoomIn;
        private ToolStripButton mnuZoomOut;
        private ToolStripButton mnuZoomExtent;
        private ToolStripButton mnuZoomWindow;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStrip tsProjectMenu;
        private ToolStrip tsCanvasTools;
        private ToolStripButton tsmExpandCollapseLeftPanel;
        private ToolStripLabel toolStripLabel1;
        private ToolStripButton tsmExpandCollapseRightPanel;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripButton mnuSelectTool;
        private ToolStripButton mnuDrawPoint;
        private ToolStripButton mnuDrawLine;
        private ToolStripButton mnuDrawPolyline;
        private ToolStripButton mnuDrawPolygon;
        private ToolStripButton mnuDrawRectangle;
        private ToolStripButton mnuDrawCircle;
        private ToolStripButton mnuDrawArc;
        private ToolStripButton mnuDrawText;
        private ToolStripButton mnuCanvasDebugOverlay;
        private ToolStripButton mnuOSnapToggle;
        private ToolStripButton mnuOrthoToggle;
        private ToolStripSeparator toolStripSeparator17;
        private ToolStripLabel lblCurrentDrawingLayer;
        private ToolStripComboBox cboCurrentDrawingLayer;
        private StatusStrip statusCanvas;
        private ToolStripStatusLabel lblProjectName;
        private ToolStripSeparator tsStatusSep1;
        private ToolStripStatusLabel lblActiveTool;
        private ToolStripSeparator tsStatusSep2;
        private ToolStripStatusLabel lblStatusMessage;
        private ToolStripStatusLabel lblStatusSpacer;
        private ToolStripStatusLabel lblOperationProgressStatus;
        private ToolStripStatusLabel lblScale;
        private ToolStripStatusLabel lblCanvasCoordinates;
        private StatusProgressBar hostOperationProgress;
        private MapCanvasControl mapCanvasControlMain;
        private StatusProgressBar hostProgressBarHost;
        private ToolStripSeparator toolStripSeparator18;
        private ToolStripMenuItem assignToolStripMenuItem;
        private ToolStripMenuItem blockDataToolStripMenuItem;
        private ToolStripMenuItem roadDataToolStripMenuItem;
        private Button btnConfigureParcelProperties;
        private ToolStripSeparator toolStripSeparator19;
        private ToolStripMenuItem mnuImportBlockLayoutPlan;
        private ToolStripMenuItem mnuImportXyzTiles;
        private ToolStripMenuItem mnuImportExternalLayers;
        private ToolStripMenuItem originalScenarioSummaryToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator20;
        private ToolStripSeparator toolStripSeparator21;
        private ToolStripMenuItem cadastralRecordsAssignmentToolStripMenuItem;
        private ToolStripMenuItem buildingInventoryDataToolStripMenuItem;
        private ToolStripMenuItem buildingInventoryToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator22;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripButton btnOriginalScenarioSummary;
    }
}
