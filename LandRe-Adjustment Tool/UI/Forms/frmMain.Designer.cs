using Land_Readjustment_Tool.UI.CustomControls;
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
            TreeNode treeNode1 = new TreeNode("Node1");
            TreeNode treeNode2 = new TreeNode("Node0", new TreeNode[] { treeNode1 });
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
            importCadastralDataDXFDWGShapefileToolStripMenuItem = new ToolStripMenuItem();
            ImportProjectBoundaryDXFDWGToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            baseMapsToolStripMenuItem = new ToolStripMenuItem();
            geotiffToolStripMenuItem = new ToolStripMenuItem();
            mBTilesToolStripMenuItem = new ToolStripMenuItem();
            xYZToolStripMenuItem = new ToolStripMenuItem();
            topographicalMapToolStripMenuItem = new ToolStripMenuItem();
            viewEditRecordToolStripMenuItem = new ToolStripMenuItem();
            landOwnerDataToolStripMenuItem = new ToolStripMenuItem();
            importToolStripMenuItem = new ToolStripMenuItem();
            contributionToolStripMenuItem = new ToolStripMenuItem();
            replottingToolStripMenuItem = new ToolStripMenuItem();
            startReplotWorkspaceToolStripMenuItem = new ToolStripMenuItem();
            validationToolStripMenuItem = new ToolStripMenuItem();
            reportsToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            areaConverterToolStripMenuItem = new ToolStripMenuItem();
            helToolStripMenuItem = new ToolStripMenuItem();
            cadastralDataToolStripMenuItem = new ToolStripMenuItem();
            importDataToolStripMenuItem = new ToolStripMenuItem();
            viewEditRecordsToolStripMenuItem = new ToolStripMenuItem();
            colorDialog1 = new ColorDialog();
            leftSplitContainer = new SplitContainer();
            grpLayer = new GroupBox();
            treeView1 = new TreeView();
            grpProperties = new GroupBox();
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
            toolStrip2 = new ToolStrip();
            tsmExpandCollapseLeftPanel = new ToolStripButton();
            toolStripLabel1 = new ToolStripLabel();
            tsmExpandCollapseRightPanel = new ToolStripButton();
            toolStripSeparator10 = new ToolStripSeparator();
            toolStripSeparator11 = new ToolStripSeparator();
            grpParcelObjProp = new GroupBox();
            dgvParcelObjProperty = new DataGridView();
            toolStrip1 = new ToolStrip();
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
            toolStripComboBox1 = new ToolStripComboBox();
            mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)leftSplitContainer).BeginInit();
            leftSplitContainer.Panel1.SuspendLayout();
            leftSplitContainer.Panel2.SuspendLayout();
            leftSplitContainer.SuspendLayout();
            grpLayer.SuspendLayout();
            grpProperties.SuspendLayout();
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
            toolStrip2.SuspendLayout();
            grpParcelObjProp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParcelObjProperty).BeginInit();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.BackColor = Color.White;
            mainMenuStrip.ImageScalingSize = new Size(20, 20);
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, projectToolStripMenuItem, dataToolStripMenuItem, contributionToolStripMenuItem, replottingToolStripMenuItem, validationToolStripMenuItem, reportsToolStripMenuItem, toolsToolStripMenuItem, helToolStripMenuItem });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(5, 2, 0, 2);
            mainMenuStrip.Size = new Size(1328, 28);
            mainMenuStrip.TabIndex = 0;
            mainMenuStrip.Text = "menuStrip1";
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
            tsmBackupProject.Image = Properties.Resources.icons8_database_export_501;
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
            dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { importDataToolStripMenuItem1, viewEditRecordToolStripMenuItem, landOwnerDataToolStripMenuItem, importToolStripMenuItem });
            dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            dataToolStripMenuItem.Size = new Size(55, 24);
            dataToolStripMenuItem.Text = "Data";
            // 
            // importDataToolStripMenuItem1
            // 
            importDataToolStripMenuItem1.DropDownItems.AddRange(new ToolStripItem[] { ImportParcelOwnerShipRecords, toolStripSeparator5, importCadastralDataDXFDWGShapefileToolStripMenuItem, ImportProjectBoundaryDXFDWGToolStripMenuItem, toolStripSeparator6, baseMapsToolStripMenuItem, topographicalMapToolStripMenuItem });
            importDataToolStripMenuItem1.Name = "importDataToolStripMenuItem1";
            importDataToolStripMenuItem1.Size = new Size(207, 26);
            importDataToolStripMenuItem1.Text = "Import Data";
            // 
            // ImportParcelOwnerShipRecords
            // 
            ImportParcelOwnerShipRecords.Enabled = false;
            ImportParcelOwnerShipRecords.Name = "ImportParcelOwnerShipRecords";
            ImportParcelOwnerShipRecords.Size = new Size(342, 26);
            ImportParcelOwnerShipRecords.Text = "Parcel Ownership Records (Excel/CSV)";
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(339, 6);
            // 
            // importCadastralDataDXFDWGShapefileToolStripMenuItem
            // 
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Name = "importCadastralDataDXFDWGShapefileToolStripMenuItem";
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Size = new Size(342, 26);
            importCadastralDataDXFDWGShapefileToolStripMenuItem.Text = "Cadastral Map (DXF/DWG/Shapefile)";
            // 
            // ImportProjectBoundaryDXFDWGToolStripMenuItem
            // 
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Name = "ImportProjectBoundaryDXFDWGToolStripMenuItem";
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Size = new Size(342, 26);
            ImportProjectBoundaryDXFDWGToolStripMenuItem.Text = "Project Boundary (DXF/DWG)";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(339, 6);
            // 
            // baseMapsToolStripMenuItem
            // 
            baseMapsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { geotiffToolStripMenuItem, mBTilesToolStripMenuItem, xYZToolStripMenuItem });
            baseMapsToolStripMenuItem.Name = "baseMapsToolStripMenuItem";
            baseMapsToolStripMenuItem.Size = new Size(342, 26);
            baseMapsToolStripMenuItem.Text = "Raster Base Map";
            // 
            // geotiffToolStripMenuItem
            // 
            geotiffToolStripMenuItem.Name = "geotiffToolStripMenuItem";
            geotiffToolStripMenuItem.Size = new Size(148, 26);
            geotiffToolStripMenuItem.Text = "GeoTIFF";
            // 
            // mBTilesToolStripMenuItem
            // 
            mBTilesToolStripMenuItem.Name = "mBTilesToolStripMenuItem";
            mBTilesToolStripMenuItem.Size = new Size(148, 26);
            mBTilesToolStripMenuItem.Text = "MBTiles";
            // 
            // xYZToolStripMenuItem
            // 
            xYZToolStripMenuItem.Name = "xYZToolStripMenuItem";
            xYZToolStripMenuItem.Size = new Size(148, 26);
            xYZToolStripMenuItem.Text = "XYZTiles";
            // 
            // topographicalMapToolStripMenuItem
            // 
            topographicalMapToolStripMenuItem.Name = "topographicalMapToolStripMenuItem";
            topographicalMapToolStripMenuItem.Size = new Size(342, 26);
            topographicalMapToolStripMenuItem.Text = "Topographical Map  (DXF/DWG)";
            // 
            // viewEditRecordToolStripMenuItem
            // 
            viewEditRecordToolStripMenuItem.Name = "viewEditRecordToolStripMenuItem";
            viewEditRecordToolStripMenuItem.Size = new Size(207, 26);
            viewEditRecordToolStripMenuItem.Text = "View/Edit Record";
            viewEditRecordToolStripMenuItem.Click += viewEditRecordToolStripMenuItem_Click;
            // 
            // landOwnerDataToolStripMenuItem
            // 
            landOwnerDataToolStripMenuItem.Enabled = false;
            landOwnerDataToolStripMenuItem.Name = "landOwnerDataToolStripMenuItem";
            landOwnerDataToolStripMenuItem.Size = new Size(207, 26);
            landOwnerDataToolStripMenuItem.Text = "Land Owner Data";
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new Size(207, 26);
            // 
            // contributionToolStripMenuItem
            // 
            contributionToolStripMenuItem.Name = "contributionToolStripMenuItem";
            contributionToolStripMenuItem.Size = new Size(106, 24);
            contributionToolStripMenuItem.Text = "Contribution";
            // 
            // replottingToolStripMenuItem
            // 
            replottingToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { startReplotWorkspaceToolStripMenuItem });
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
            // validationToolStripMenuItem
            // 
            validationToolStripMenuItem.Name = "validationToolStripMenuItem";
            validationToolStripMenuItem.Size = new Size(90, 24);
            validationToolStripMenuItem.Text = "Validation";
            // 
            // reportsToolStripMenuItem
            // 
            reportsToolStripMenuItem.Name = "reportsToolStripMenuItem";
            reportsToolStripMenuItem.Size = new Size(69, 24);
            reportsToolStripMenuItem.Text = "Output";
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { areaConverterToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(58, 24);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // areaConverterToolStripMenuItem
            // 
            areaConverterToolStripMenuItem.Name = "areaConverterToolStripMenuItem";
            areaConverterToolStripMenuItem.Size = new Size(191, 26);
            areaConverterToolStripMenuItem.Text = "Area Converter";
            // 
            // helToolStripMenuItem
            // 
            helToolStripMenuItem.Name = "helToolStripMenuItem";
            helToolStripMenuItem.Size = new Size(55, 24);
            helToolStripMenuItem.Text = "Help";
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
            // colorDialog1
            // 
            colorDialog1.AllowFullOpen = false;
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
            leftSplitContainer.Panel2.Controls.Add(grpProperties);
            leftSplitContainer.Panel2.Controls.Add(label1);
            leftSplitContainer.Size = new Size(284, 564);
            leftSplitContainer.SplitterDistance = 220;
            leftSplitContainer.TabIndex = 0;
            // 
            // grpLayer
            // 
            grpLayer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLayer.Controls.Add(treeView1);
            grpLayer.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpLayer.Location = new Point(4, 4);
            grpLayer.Margin = new Padding(4);
            grpLayer.Name = "grpLayer";
            grpLayer.Padding = new Padding(4);
            grpLayer.RightToLeft = RightToLeft.No;
            grpLayer.Size = new Size(277, 212);
            grpLayer.TabIndex = 0;
            grpLayer.TabStop = false;
            grpLayer.Text = "Layers";
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.CheckBoxes = true;
            treeView1.Location = new Point(6, 23);
            treeView1.Margin = new Padding(4);
            treeView1.Name = "treeView1";
            treeNode1.BackColor = Color.Red;
            treeNode1.Name = "Node1";
            treeNode1.Text = "Node1";
            treeNode2.Name = "Node0";
            treeNode2.Text = "Node0";
            treeView1.Nodes.AddRange(new TreeNode[] { treeNode2 });
            treeView1.ShowLines = false;
            treeView1.Size = new Size(263, 181);
            treeView1.TabIndex = 0;
            // 
            // grpProperties
            // 
            grpProperties.Controls.Add(tabProperties);
            grpProperties.Dock = DockStyle.Fill;
            grpProperties.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpProperties.Location = new Point(0, 0);
            grpProperties.Name = "grpProperties";
            grpProperties.Padding = new Padding(6, 8, 6, 6);
            grpProperties.Size = new Size(284, 340);
            grpProperties.TabIndex = 2;
            grpProperties.TabStop = false;
            grpProperties.Text = "Layer Properties";
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
            tabProperties.Size = new Size(272, 306);
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
            tabGeneral.Size = new Size(264, 273);
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
            txtLayerName.Size = new Size(124, 27);
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
            cboLayerType.FlatStyle = FlatStyle.Flat;
            cboLayerType.Font = new Font("Segoe UI", 9F);
            cboLayerType.Items.AddRange(new object[] { "BaselineParcel", "ReplottedParcel", "ProposedRoad", "ExistingRoad", "Block", "ProjectBoundary", "Annotation", "Reference" });
            cboLayerType.Location = new Point(127, 48);
            cboLayerType.Name = "cboLayerType";
            cboLayerType.Size = new Size(124, 28);
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
            cboLineStyle.FlatStyle = FlatStyle.Flat;
            cboLineStyle.Font = new Font("Segoe UI", 9F);
            cboLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "DashDot" });
            cboLineStyle.Location = new Point(127, 123);
            cboLineStyle.Name = "cboLineStyle";
            cboLineStyle.Size = new Size(124, 28);
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
            cboLineWeight.FlatStyle = FlatStyle.Flat;
            cboLineWeight.Font = new Font("Segoe UI", 9F);
            cboLineWeight.Items.AddRange(new object[] { "0.25", "0.5", "1.0", "1.5", "2.0", "3.0" });
            cboLineWeight.Location = new Point(127, 162);
            cboLineWeight.Name = "cboLineWeight";
            cboLineWeight.Size = new Size(124, 28);
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
            tabFill.Size = new Size(156, 273);
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
            trkTransparency.Size = new Size(0, 56);
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
            tabLabel.Size = new Size(156, 273);
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
            txtFontName.Size = new Size(0, 27);
            txtFontName.TabIndex = 2;
            // 
            // btnPickFont
            // 
            btnPickFont.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPickFont.Location = new Point(103, 46);
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
            cboLabelField.Items.AddRange(new object[] { "ParcelNo", "OwnerName", "AreaSqm", "AreaRAPD", "LandUse", "PlotNumber" });
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
            mainSplitContainer.Location = new Point(0, 59);
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
            mainSplitContainer.Size = new Size(1328, 568);
            mainSplitContainer.SplitterDistance = 288;
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
            splitContainer3.Panel1.Controls.Add(toolStrip2);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(grpParcelObjProp);
            splitContainer3.Size = new Size(1036, 568);
            splitContainer3.SplitterDistance = 712;
            splitContainer3.TabIndex = 1;
            // 
            // toolStrip2
            // 
            toolStrip2.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip2.ImageScalingSize = new Size(20, 20);
            toolStrip2.Items.AddRange(new ToolStripItem[] { tsmExpandCollapseLeftPanel, toolStripLabel1, tsmExpandCollapseRightPanel, toolStripSeparator10, toolStripSeparator11 });
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(708, 27);
            toolStrip2.TabIndex = 1;
            toolStrip2.Text = "toolStrip2";
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
            tsmExpandCollapseLeftPanel.Size = new Size(29, 24);
            tsmExpandCollapseLeftPanel.Text = "toolStripButton5";
            tsmExpandCollapseLeftPanel.Click += tsmExpandCollapseLeftPanel_Click;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(0, 24);
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
            tsmExpandCollapseRightPanel.Size = new Size(29, 24);
            tsmExpandCollapseRightPanel.Text = "toolStripLabel2";
            tsmExpandCollapseRightPanel.Click += tsmExpandCollapseRightPanel_Click;
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new Size(6, 27);
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Alignment = ToolStripItemAlignment.Right;
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new Size(6, 27);
            // 
            // grpParcelObjProp
            // 
            grpParcelObjProp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpParcelObjProp.Controls.Add(dgvParcelObjProperty);
            grpParcelObjProp.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpParcelObjProp.Location = new Point(4, 4);
            grpParcelObjProp.Margin = new Padding(4);
            grpParcelObjProp.Name = "grpParcelObjProp";
            grpParcelObjProp.Padding = new Padding(4);
            grpParcelObjProp.RightToLeft = RightToLeft.No;
            grpParcelObjProp.Size = new Size(312, 560);
            grpParcelObjProp.TabIndex = 1;
            grpParcelObjProp.TabStop = false;
            grpParcelObjProp.Text = "Parcel";
            // 
            // dgvParcelObjProperty
            // 
            dgvParcelObjProperty.BackgroundColor = SystemColors.Control;
            dgvParcelObjProperty.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParcelObjProperty.Dock = DockStyle.Fill;
            dgvParcelObjProperty.Location = new Point(4, 24);
            dgvParcelObjProperty.Name = "dgvParcelObjProperty";
            dgvParcelObjProperty.RowHeadersWidth = 51;
            dgvParcelObjProperty.Size = new Size(304, 532);
            dgvParcelObjProperty.TabIndex = 0;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { mnuNewProject, mnuOpenProject, mnuSaveProject, mnuSaveAsProject, mnuBackup, mnuRestoreBackup, mnuCloseProject, toolStripSeparator9, toolStripButton3, mnuProjectInfo, mnuProjectSettings, toolStripSeparator12, toolStripSeparator13, mnuUndo, mnuRedo, toolStripSeparator14, mnuPan, mnuZoomIn, mnuZoomOut, mnuZoomExtent, mnuZoomWindow, toolStripSeparator15, toolStripSeparator16, toolStripComboBox1 });
            toolStrip1.Location = new Point(0, 28);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1328, 28);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // mnuNewProject
            // 
            mnuNewProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuNewProject.Image = Properties.Resources.icons8_file_501;
            mnuNewProject.ImageTransparentColor = Color.Magenta;
            mnuNewProject.Name = "mnuNewProject";
            mnuNewProject.Size = new Size(29, 25);
            mnuNewProject.Text = "New Project";
            // 
            // mnuOpenProject
            // 
            mnuOpenProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuOpenProject.Image = Properties.Resources.icons8_open_folder_50;
            mnuOpenProject.ImageTransparentColor = Color.Magenta;
            mnuOpenProject.Name = "mnuOpenProject";
            mnuOpenProject.Size = new Size(29, 25);
            mnuOpenProject.Text = "Open Project";
            // 
            // mnuSaveProject
            // 
            mnuSaveProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuSaveProject.Image = Properties.Resources.icons8_save_50;
            mnuSaveProject.ImageTransparentColor = Color.Magenta;
            mnuSaveProject.Name = "mnuSaveProject";
            mnuSaveProject.Size = new Size(29, 25);
            mnuSaveProject.Text = "Save Project";
            // 
            // mnuSaveAsProject
            // 
            mnuSaveAsProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuSaveAsProject.Image = Properties.Resources.icons8_save_as_50;
            mnuSaveAsProject.ImageTransparentColor = Color.Magenta;
            mnuSaveAsProject.Name = "mnuSaveAsProject";
            mnuSaveAsProject.Size = new Size(29, 25);
            mnuSaveAsProject.Text = "Save As Project";
            mnuSaveAsProject.Click += mnuSaveAsProject_Click;
            // 
            // mnuBackup
            // 
            mnuBackup.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuBackup.Image = Properties.Resources.icons8_database_export_50;
            mnuBackup.ImageTransparentColor = Color.Magenta;
            mnuBackup.Name = "mnuBackup";
            mnuBackup.Size = new Size(29, 25);
            mnuBackup.Text = "Backup Project";
            // 
            // mnuRestoreBackup
            // 
            mnuRestoreBackup.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuRestoreBackup.Image = Properties.Resources.icons8_data_backup_501;
            mnuRestoreBackup.ImageTransparentColor = Color.Magenta;
            mnuRestoreBackup.Name = "mnuRestoreBackup";
            mnuRestoreBackup.Size = new Size(29, 25);
            mnuRestoreBackup.Text = "Restore Project from Backup";
            // 
            // mnuCloseProject
            // 
            mnuCloseProject.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuCloseProject.Image = Properties.Resources.icons8_close_501;
            mnuCloseProject.ImageTransparentColor = Color.Magenta;
            mnuCloseProject.Name = "mnuCloseProject";
            mnuCloseProject.Size = new Size(29, 25);
            mnuCloseProject.Text = "Close Project";
            // 
            // toolStripSeparator9
            // 
            toolStripSeparator9.Name = "toolStripSeparator9";
            toolStripSeparator9.Size = new Size(6, 28);
            // 
            // toolStripButton3
            // 
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(6, 28);
            // 
            // mnuProjectInfo
            // 
            mnuProjectInfo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuProjectInfo.Image = Properties.Resources.icons8_info_squared_501;
            mnuProjectInfo.ImageTransparentColor = Color.Magenta;
            mnuProjectInfo.Name = "mnuProjectInfo";
            mnuProjectInfo.Size = new Size(29, 25);
            mnuProjectInfo.Text = "Project Information";
            // 
            // mnuProjectSettings
            // 
            mnuProjectSettings.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuProjectSettings.Image = Properties.Resources.icons8_wrench_501;
            mnuProjectSettings.ImageTransparentColor = Color.Magenta;
            mnuProjectSettings.Name = "mnuProjectSettings";
            mnuProjectSettings.Size = new Size(29, 25);
            mnuProjectSettings.Text = "Project Setting";
            // 
            // toolStripSeparator12
            // 
            toolStripSeparator12.Name = "toolStripSeparator12";
            toolStripSeparator12.Size = new Size(6, 28);
            // 
            // toolStripSeparator13
            // 
            toolStripSeparator13.Name = "toolStripSeparator13";
            toolStripSeparator13.Size = new Size(6, 28);
            // 
            // mnuUndo
            // 
            mnuUndo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuUndo.Image = Properties.Resources.icons8_undo_50;
            mnuUndo.ImageTransparentColor = Color.Magenta;
            mnuUndo.Name = "mnuUndo";
            mnuUndo.Size = new Size(29, 25);
            mnuUndo.Text = "Undo";
            // 
            // mnuRedo
            // 
            mnuRedo.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuRedo.Image = Properties.Resources.icons8_redo_50;
            mnuRedo.ImageTransparentColor = Color.Magenta;
            mnuRedo.Name = "mnuRedo";
            mnuRedo.Size = new Size(29, 25);
            mnuRedo.Text = "Redo";
            // 
            // toolStripSeparator14
            // 
            toolStripSeparator14.Name = "toolStripSeparator14";
            toolStripSeparator14.Size = new Size(6, 28);
            // 
            // mnuPan
            // 
            mnuPan.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuPan.Image = Properties.Resources.icons8_hand_50;
            mnuPan.ImageTransparentColor = Color.Magenta;
            mnuPan.Name = "mnuPan";
            mnuPan.Size = new Size(29, 25);
            mnuPan.Text = "Pan";
            // 
            // mnuZoomIn
            // 
            mnuZoomIn.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomIn.Image = Properties.Resources.icons8_zoom_in_50;
            mnuZoomIn.ImageTransparentColor = Color.Magenta;
            mnuZoomIn.Name = "mnuZoomIn";
            mnuZoomIn.Size = new Size(29, 25);
            mnuZoomIn.Text = "Zoom In";
            // 
            // mnuZoomOut
            // 
            mnuZoomOut.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomOut.Image = Properties.Resources.icons8_zoom_out_50;
            mnuZoomOut.ImageTransparentColor = Color.Magenta;
            mnuZoomOut.Name = "mnuZoomOut";
            mnuZoomOut.Size = new Size(29, 25);
            mnuZoomOut.Text = "Zoom Out ";
            // 
            // mnuZoomExtent
            // 
            mnuZoomExtent.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomExtent.Image = Properties.Resources.icons8_zoom_to_extents_50;
            mnuZoomExtent.ImageTransparentColor = Color.Magenta;
            mnuZoomExtent.Name = "mnuZoomExtent";
            mnuZoomExtent.Size = new Size(29, 25);
            mnuZoomExtent.Text = "toolStripButton6";
            // 
            // mnuZoomWindow
            // 
            mnuZoomWindow.DisplayStyle = ToolStripItemDisplayStyle.Image;
            mnuZoomWindow.Image = Properties.Resources.icons8_zoom_to_fit_50;
            mnuZoomWindow.ImageTransparentColor = Color.Magenta;
            mnuZoomWindow.Name = "mnuZoomWindow";
            mnuZoomWindow.Size = new Size(29, 25);
            mnuZoomWindow.Text = "toolStripButton7";
            // 
            // toolStripSeparator15
            // 
            toolStripSeparator15.Name = "toolStripSeparator15";
            toolStripSeparator15.Size = new Size(6, 28);
            // 
            // toolStripSeparator16
            // 
            toolStripSeparator16.Name = "toolStripSeparator16";
            toolStripSeparator16.Size = new Size(6, 28);
            // 
            // toolStripComboBox1
            // 
            toolStripComboBox1.FlatStyle = FlatStyle.Standard;
            toolStripComboBox1.Name = "toolStripComboBox1";
            toolStripComboBox1.Size = new Size(121, 28);
            // 
            // frmMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1328, 629);
            Controls.Add(toolStrip1);
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
            leftSplitContainer.Panel2.ResumeLayout(false);
            leftSplitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)leftSplitContainer).EndInit();
            leftSplitContainer.ResumeLayout(false);
            grpLayer.ResumeLayout(false);
            grpProperties.ResumeLayout(false);
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
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            grpParcelObjProp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvParcelObjProperty).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem areaConverterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tsmNewProject;
        private System.Windows.Forms.ToolStripMenuItem tsmOpenProject;
        private System.Windows.Forms.ToolStripMenuItem tsmSave;
        private System.Windows.Forms.ToolStripMenuItem exportProjectToolStripMenuItem;
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
        private ToolStripMenuItem importCadastralDataDXFDWGShapefileToolStripMenuItem;
        private ToolStripMenuItem viewEditRecordToolStripMenuItem;
        private ToolStripMenuItem baseMapsToolStripMenuItem;
        private ToolStripMenuItem ImportProjectBoundaryDXFDWGToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem geotiffToolStripMenuItem;
        private ToolStripMenuItem mBTilesToolStripMenuItem;
        private ToolStripMenuItem xYZToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem topographicalMapToolStripMenuItem;
        private ToolStripMenuItem ImportParcelOwnerShipRecords;
        private DrawingCanvasControl drawingCanvasControl1;
        private ColorDialog colorDialog1;
        private DrawingCanvasControl drawingCanvasControl2;
        private SplitContainer leftSplitContainer;
        private GroupBox grpLayer;
        private TreeView treeView1;
        private GroupBox grpProperties;
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
        private ToolStrip toolStrip1;
        private ToolStripButton mnuNewProject;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripButton mnuOpenProject;
        private ToolStripSeparator toolStripButton3;
        private ToolStripButton mnuSaveProject;
        private ToolStripButton mnuSaveAsProject;
        private ToolStripSeparator toolStripSeparator9;
        private ToolStripButton mnuCloseProject;
        private ToolStripButton mnuProjectInfo;
        private ToolStripButton mnuProjectSettings;
        private SplitContainer splitContainer3;
        private ToolStrip toolStrip2;
        private ToolStripButton tsmExpandCollapseLeftPanel;
        private ToolStripLabel toolStripLabel1;
        private ToolStripButton tsmExpandCollapseRightPanel;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private GroupBox grpParcelObjProp;
        private DataGridView dgvParcelObjProperty;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripSeparator13;
        private ToolStripButton mnuUndo;
        private ToolStripButton mnuBackup;
        private ToolStripButton mnuRestoreBackup;
        private ToolStripButton mnuRedo;
        private ToolStripSeparator toolStripSeparator14;
        private ToolStripButton mnuPan;
        private ToolStripButton mnuZoomIn;
        private ToolStripButton mnuZoomOut;
        private ToolStripButton mnuZoomExtent;
        private ToolStripButton mnuZoomWindow;
        private ToolStripSeparator toolStripSeparator15;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripComboBox toolStripComboBox1;
    }
}