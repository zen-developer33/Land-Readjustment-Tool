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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            tsmNewProject = new ToolStripMenuItem();
            tsmOpenProject = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            tsmSave = new ToolStripMenuItem();
            tsmSaveAs = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tsmRecentProjects = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            tsmExit = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            projectToolStripMenuItem = new ToolStripMenuItem();
            tsmProjectInformation = new ToolStripMenuItem();
            tsmProjectSetting = new ToolStripMenuItem();
            tsmCloseProject = new ToolStripMenuItem();
            tsmBackupProject = new ToolStripMenuItem();
            tsmRestoreBackup = new ToolStripMenuItem();
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
            mainSplitContainer = new SplitContainer();
            splitContainer2 = new SplitContainer();
            grpLayer = new GroupBox();
            treeView1 = new TreeView();
            label1 = new Label();
            splitContainer3 = new SplitContainer();
            importDataToolStripMenuItem = new ToolStripMenuItem();
            viewEditRecordsToolStripMenuItem = new ToolStripMenuItem();
            colorDialog2 = new ColorDialog();
            mainMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            grpLayer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.SuspendLayout();
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
            mainMenuStrip.Size = new Size(1313, 28);
            mainMenuStrip.TabIndex = 0;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmNewProject, tsmOpenProject, toolStripSeparator1, tsmSave, tsmSaveAs, toolStripSeparator2, tsmRecentProjects, toolStripSeparator3, tsmExit, toolStripSeparator4 });
            fileToolStripMenuItem.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // tsmNewProject
            // 
            tsmNewProject.Name = "tsmNewProject";
            tsmNewProject.ShortcutKeys = Keys.Control | Keys.N;
            tsmNewProject.Size = new Size(233, 26);
            tsmNewProject.Text = "&New Project";
            tsmNewProject.Click += tsmNewProject_Click;
            // 
            // tsmOpenProject
            // 
            tsmOpenProject.Name = "tsmOpenProject";
            tsmOpenProject.ShortcutKeys = Keys.Control | Keys.O;
            tsmOpenProject.Size = new Size(233, 26);
            tsmOpenProject.Text = "&Open Project";
            tsmOpenProject.Click += tsmOpenProject_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(230, 6);
            // 
            // tsmSave
            // 
            tsmSave.Enabled = false;
            tsmSave.Name = "tsmSave";
            tsmSave.ShortcutKeys = Keys.Control | Keys.S;
            tsmSave.Size = new Size(233, 26);
            tsmSave.Text = "Save";
            // 
            // tsmSaveAs
            // 
            tsmSaveAs.Enabled = false;
            tsmSaveAs.Name = "tsmSaveAs";
            tsmSaveAs.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            tsmSaveAs.Size = new Size(233, 26);
            tsmSaveAs.Text = "Save As";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(230, 6);
            // 
            // tsmRecentProjects
            // 
            tsmRecentProjects.Name = "tsmRecentProjects";
            tsmRecentProjects.Size = new Size(233, 26);
            tsmRecentProjects.Text = "Recent Projects";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(230, 6);
            // 
            // tsmExit
            // 
            tsmExit.Name = "tsmExit";
            tsmExit.Size = new Size(233, 26);
            tsmExit.Text = "Exit";
            tsmExit.Click += ExitToolStripMenuItem_Click_1;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(230, 6);
            // 
            // projectToolStripMenuItem
            // 
            projectToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmProjectInformation, tsmProjectSetting, tsmCloseProject, tsmBackupProject, tsmRestoreBackup });
            projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            projectToolStripMenuItem.Size = new Size(69, 24);
            projectToolStripMenuItem.Text = "Project";
            // 
            // tsmProjectInformation
            // 
            tsmProjectInformation.Enabled = false;
            tsmProjectInformation.Name = "tsmProjectInformation";
            tsmProjectInformation.Size = new Size(232, 26);
            tsmProjectInformation.Text = "Project Information";
            tsmProjectInformation.Click += tsmProjectInformation_Click;
            // 
            // tsmProjectSetting
            // 
            tsmProjectSetting.Enabled = false;
            tsmProjectSetting.Name = "tsmProjectSetting";
            tsmProjectSetting.Size = new Size(232, 26);
            tsmProjectSetting.Text = "Project Setting";
            tsmProjectSetting.Click += tsmProjectSetting_Click;
            // 
            // tsmCloseProject
            // 
            tsmCloseProject.Enabled = false;
            tsmCloseProject.Name = "tsmCloseProject";
            tsmCloseProject.Size = new Size(232, 26);
            tsmCloseProject.Text = "Close Project";

            // 
            // tsmBackupProject
            // 
            tsmBackupProject.Enabled = false;
            tsmBackupProject.Name = "tsmBackupProject";
            tsmBackupProject.Size = new Size(232, 26);
            tsmBackupProject.Text = "Backup Project";
            // 
            // tsmRestoreBackup
            // 
            tsmRestoreBackup.Name = "tsmRestoreBackup";
            tsmRestoreBackup.Size = new Size(232, 26);
            tsmRestoreBackup.Text = "Restore From Backup";
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
            // mainSplitContainer
            // 
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.Location = new Point(0, 28);
            mainSplitContainer.Margin = new Padding(4);
            mainSplitContainer.Name = "mainSplitContainer";
            // 
            // mainSplitContainer.Panel1
            // 
            mainSplitContainer.Panel1.Controls.Add(splitContainer2);
            mainSplitContainer.Panel1MinSize = 200;
            // 
            // mainSplitContainer.Panel2
            // 
            mainSplitContainer.Panel2.Controls.Add(splitContainer3);
            mainSplitContainer.Size = new Size(1313, 696);
            mainSplitContainer.SplitterDistance = 260;
            mainSplitContainer.TabIndex = 3;
            mainSplitContainer.Visible = false;
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
            splitContainer2.Size = new Size(260, 696);
            splitContainer2.SplitterDistance = 338;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 0;
            // 
            // grpLayer
            // 
            grpLayer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLayer.Controls.Add(treeView1);
            grpLayer.Font = new Font("Microsoft Sans Serif", 7.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpLayer.Location = new Point(4, 4);
            grpLayer.Margin = new Padding(4);
            grpLayer.Name = "grpLayer";
            grpLayer.Padding = new Padding(4);
            grpLayer.RightToLeft = RightToLeft.No;
            grpLayer.Size = new Size(253, 330);
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
            treeView1.Size = new Size(239, 299);
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
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Size = new Size(1049, 696);
            splitContainer3.SplitterDistance = 779;
            splitContainer3.TabIndex = 1;
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
            // frmMain
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1313, 724);
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
            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
            mainSplitContainer.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            grpLayer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
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
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox grpLayer;
        private TreeView treeView1;
        private Label label1;
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
        private ColorDialog colorDialog2;
        private SplitContainer splitContainer3;
    }
}