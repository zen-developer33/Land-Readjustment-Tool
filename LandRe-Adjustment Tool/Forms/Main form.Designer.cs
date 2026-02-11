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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newProjectToolStripMenuItem = new ToolStripMenuItem();
            openProjectToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            recentProjectsToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            projectToolStripMenuItem = new ToolStripMenuItem();
            projectInformationToolStripMenuItem = new ToolStripMenuItem();
            projectSettingToolStripMenuItem = new ToolStripMenuItem();
            closeProjectToolStripMenuItem = new ToolStripMenuItem();
            backupProjectToolStripMenuItem = new ToolStripMenuItem();
            restoreFromBackupToolStripMenuItem = new ToolStripMenuItem();
            dataToolStripMenuItem = new ToolStripMenuItem();
            importDataToolStripMenuItem1 = new ToolStripMenuItem();
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
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            grpLayer = new GroupBox();
            treeView1 = new TreeView();
            label1 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            importDataToolStripMenuItem = new ToolStripMenuItem();
            viewEditRecordsToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            toolStripButton1 = new ToolStripButton();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripButton();
            toolStripSeparator7 = new ToolStripSeparator();
            toolStripButton4 = new ToolStripButton();
            toolStripLabel1 = new ToolStripLabel();
            toolStripComboBox1 = new ToolStripComboBox();
            ImportParcelOwnerShipRecords = new ToolStripMenuItem();
            mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            grpLayer.SuspendLayout();
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
            mainMenuStrip.Size = new Size(1258, 28);
            mainMenuStrip.TabIndex = 0;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newProjectToolStripMenuItem, openProjectToolStripMenuItem, toolStripSeparator1, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator2, recentProjectsToolStripMenuItem, toolStripSeparator3, exitToolStripMenuItem, toolStripSeparator4 });
            fileToolStripMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            fileToolStripMenuItem.Click += fileToolStripMenuItem_Click;
            // 
            // newProjectToolStripMenuItem
            // 
            newProjectToolStripMenuItem.Name = "newProjectToolStripMenuItem";
            newProjectToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            newProjectToolStripMenuItem.Size = new Size(233, 26);
            newProjectToolStripMenuItem.Text = "&New Project";
            newProjectToolStripMenuItem.Click += newProjectToolStripMenuItem_Click;
            // 
            // openProjectToolStripMenuItem
            // 
            openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            openProjectToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            openProjectToolStripMenuItem.Size = new Size(233, 26);
            openProjectToolStripMenuItem.Text = "&Open Project";
            openProjectToolStripMenuItem.Click += openProjectToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(230, 6);
            toolStripSeparator1.Click += toolStripSeparator1_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveToolStripMenuItem.Size = new Size(233, 26);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsToolStripMenuItem.Size = new Size(233, 26);
            saveAsToolStripMenuItem.Text = "Save As";
            saveAsToolStripMenuItem.Click += saveAsToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(230, 6);
            // 
            // recentProjectsToolStripMenuItem
            // 
            recentProjectsToolStripMenuItem.Name = "recentProjectsToolStripMenuItem";
            recentProjectsToolStripMenuItem.Size = new Size(233, 26);
            recentProjectsToolStripMenuItem.Text = "Recent Projects";
            recentProjectsToolStripMenuItem.Click += toolStripMenuItem1_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(230, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(233, 26);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click_1;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(230, 6);
            // 
            // projectToolStripMenuItem
            // 
            projectToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { projectInformationToolStripMenuItem, projectSettingToolStripMenuItem, closeProjectToolStripMenuItem, backupProjectToolStripMenuItem, restoreFromBackupToolStripMenuItem });
            projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            projectToolStripMenuItem.Size = new Size(69, 24);
            projectToolStripMenuItem.Text = "Project";
            // 
            // projectInformationToolStripMenuItem
            // 
            projectInformationToolStripMenuItem.Enabled = false;
            projectInformationToolStripMenuItem.Name = "projectInformationToolStripMenuItem";
            projectInformationToolStripMenuItem.Size = new Size(232, 26);
            projectInformationToolStripMenuItem.Text = "Project Information";
            projectInformationToolStripMenuItem.Click += ProjectInformationToolStripMenuItem_Click;
            // 
            // projectSettingToolStripMenuItem
            // 
            projectSettingToolStripMenuItem.Enabled = false;
            projectSettingToolStripMenuItem.Name = "projectSettingToolStripMenuItem";
            projectSettingToolStripMenuItem.Size = new Size(232, 26);
            projectSettingToolStripMenuItem.Text = "Project Setting";
            projectSettingToolStripMenuItem.Click += projectSettingToolStripMenuItem_Click;
            // 
            // closeProjectToolStripMenuItem
            // 
            closeProjectToolStripMenuItem.Enabled = false;
            closeProjectToolStripMenuItem.Name = "closeProjectToolStripMenuItem";
            closeProjectToolStripMenuItem.Size = new Size(232, 26);
            closeProjectToolStripMenuItem.Text = "Close Project";
            closeProjectToolStripMenuItem.Click += closeProjectToolStripMenuItem_Click;
            // 
            // backupProjectToolStripMenuItem
            // 
            backupProjectToolStripMenuItem.Enabled = false;
            backupProjectToolStripMenuItem.Name = "backupProjectToolStripMenuItem";
            backupProjectToolStripMenuItem.Size = new Size(232, 26);
            backupProjectToolStripMenuItem.Text = "Backup Project";
            // 
            // restoreFromBackupToolStripMenuItem
            // 
            restoreFromBackupToolStripMenuItem.Name = "restoreFromBackupToolStripMenuItem";
            restoreFromBackupToolStripMenuItem.Size = new Size(232, 26);
            restoreFromBackupToolStripMenuItem.Text = "Restore From Backup";
            restoreFromBackupToolStripMenuItem.Click += restoreFromBackupToolStripMenuItem_Click;
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
            importDataToolStripMenuItem1.Size = new Size(224, 26);
            importDataToolStripMenuItem1.Text = "Import Data";
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
            baseMapsToolStripMenuItem.Click += baseMapsToolStripMenuItem_Click;
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
            viewEditRecordToolStripMenuItem.Size = new Size(224, 26);
            viewEditRecordToolStripMenuItem.Text = "View/Edit Record";
            viewEditRecordToolStripMenuItem.Click += viewEditRecordToolStripMenuItem_Click;
            // 
            // landOwnerDataToolStripMenuItem
            // 
            landOwnerDataToolStripMenuItem.Enabled = false;
            landOwnerDataToolStripMenuItem.Name = "landOwnerDataToolStripMenuItem";
            landOwnerDataToolStripMenuItem.Size = new Size(224, 26);
            landOwnerDataToolStripMenuItem.Text = "Land Owner Data";
            landOwnerDataToolStripMenuItem.Click += landOwnerDataToolStripMenuItem_Click;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new Size(224, 26);
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
            areaConverterToolStripMenuItem.Click += AreaConverterToolStripMenuItem_Click;
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
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 28);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(splitContainer2);
            splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(flowLayoutPanel1);
            splitContainer1.Size = new Size(1258, 569);
            splitContainer1.SplitterDistance = 302;
            splitContainer1.TabIndex = 3;
            splitContainer1.Visible = false;
            // 
            // splitContainer2
            // 
            splitContainer2.AllowDrop = true;
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Margin = new Padding(4);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(grpLayer);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(label1);
            splitContainer2.Panel2.Paint += splitContainer2_Panel2_Paint;
            splitContainer2.Size = new Size(302, 569);
            splitContainer2.SplitterDistance = 277;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // grpLayer
            // 
            grpLayer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLayer.Controls.Add(treeView1);
            grpLayer.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpLayer.Location = new Point(4, 0);
            grpLayer.Margin = new Padding(4);
            grpLayer.Name = "grpLayer";
            grpLayer.Padding = new Padding(4);
            grpLayer.RightToLeft = RightToLeft.No;
            grpLayer.Size = new Size(295, 273);
            grpLayer.TabIndex = 0;
            grpLayer.TabStop = false;
            grpLayer.Text = "Layers";
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.CheckBoxes = true;
            treeView1.Location = new Point(6, 19);
            treeView1.Margin = new Padding(4);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(281, 246);
            treeView1.TabIndex = 0;
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
            label1.Click += label1_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.BackColor = SystemColors.ActiveCaptionText;
            flowLayoutPanel1.Cursor = Cursors.Cross;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(2);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(952, 569);
            flowLayoutPanel1.TabIndex = 0;
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
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton1, toolStripButton2, toolStripButton3, toolStripSeparator7, toolStripButton4, toolStripLabel1, toolStripComboBox1 });
            toolStrip1.Location = new Point(0, 28);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1258, 28);
            toolStrip1.TabIndex = 4;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(29, 25);
            toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripButton2
            // 
            toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(29, 25);
            toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripButton3
            // 
            toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
            toolStripButton3.ImageTransparentColor = Color.Magenta;
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(29, 25);
            toolStripButton3.Text = "toolStripButton3";
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new Size(6, 28);
            // 
            // toolStripButton4
            // 
            toolStripButton4.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(29, 25);
            toolStripButton4.Text = "toolStripButton4";
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(111, 25);
            toolStripLabel1.Text = "toolStripLabel1";
            // 
            // toolStripComboBox1
            // 
            toolStripComboBox1.Name = "toolStripComboBox1";
            toolStripComboBox1.Size = new Size(121, 28);
            // 
            // ImportParcelOwnerShipRecords
            // 
            ImportParcelOwnerShipRecords.Enabled = false;
            ImportParcelOwnerShipRecords.Name = "ImportParcelOwnerShipRecords";
            ImportParcelOwnerShipRecords.Size = new Size(342, 26);
            ImportParcelOwnerShipRecords.Text = "Parcel Ownership Records (Excel/CSV)";
            ImportParcelOwnerShipRecords.Click += ImportParcelOwnershipRecords_Click;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1258, 597);
            Controls.Add(toolStrip1);
            Controls.Add(splitContainer1);
            Controls.Add(mainMenuStrip);
            DoubleBuffered = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(2);
            Name = "frmMain";
            Text = "RePlot";
            WindowState = FormWindowState.Maximized;
            Load += frmMain_Load;
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            grpLayer.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem newProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectSettingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cadastralDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupProjectToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox grpLayer;
        private TreeView treeView1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Label label1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem recentProjectsToolStripMenuItem;
        private ToolStripMenuItem restoreFromBackupToolStripMenuItem;
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
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripButton toolStripButton4;
        private ToolStripLabel toolStripLabel1;
        private ToolStripComboBox toolStripComboBox1;
        private ToolStripMenuItem ImportParcelOwnerShipRecords;
    }
}