namespace Land_Pooling_Policy_Manager.UI.Forms
{
    partial class frmPolicyManagerMdiHost
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openPolicyEditorToolStripMenuItem;
        private ToolStripSeparator fileSeparator1;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem openParametersToolStripMenuItem;
        private ToolStripMenuItem openLookupTablesToolStripMenuItem;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem tileHorizontalToolStripMenuItem;
        private ToolStripMenuItem tileVerticalToolStripMenuItem;
        private ToolStripMenuItem cascadeToolStripMenuItem;
        private ToolStrip toolStrip;
        private ToolStripButton btnPolicyEditor;
        private ToolStripButton btnParameters;
        private ToolStripButton btnLookupTables;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton btnTileHorizontal;
        private ToolStripButton btnTileVertical;
        private ToolStripButton btnCascade;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openPolicyEditorToolStripMenuItem = new ToolStripMenuItem();
            fileSeparator1 = new ToolStripSeparator();
            closeToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            openParametersToolStripMenuItem = new ToolStripMenuItem();
            openLookupTablesToolStripMenuItem = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            tileHorizontalToolStripMenuItem = new ToolStripMenuItem();
            tileVerticalToolStripMenuItem = new ToolStripMenuItem();
            cascadeToolStripMenuItem = new ToolStripMenuItem();
            toolStrip = new ToolStrip();
            btnPolicyEditor = new ToolStripButton();
            btnParameters = new ToolStripButton();
            btnLookupTables = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            btnTileHorizontal = new ToolStripButton();
            btnTileVertical = new ToolStripButton();
            btnCascade = new ToolStripButton();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            menuStrip.SuspendLayout();
            toolStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, viewToolStripMenuItem, windowToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1200, 28);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openPolicyEditorToolStripMenuItem, fileSeparator1, closeToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openPolicyEditorToolStripMenuItem
            // 
            openPolicyEditorToolStripMenuItem.Name = "openPolicyEditorToolStripMenuItem";
            openPolicyEditorToolStripMenuItem.Size = new Size(174, 26);
            openPolicyEditorToolStripMenuItem.Text = "&Policy Editor";
            openPolicyEditorToolStripMenuItem.Click += btnPolicyEditor_Click;
            // 
            // fileSeparator1
            // 
            fileSeparator1.Name = "fileSeparator1";
            fileSeparator1.Size = new Size(171, 6);
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(174, 26);
            closeToolStripMenuItem.Text = "&Close";
            closeToolStripMenuItem.Click += btnClose_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openParametersToolStripMenuItem, openLookupTablesToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(55, 24);
            viewToolStripMenuItem.Text = "&View";
            // 
            // openParametersToolStripMenuItem
            // 
            openParametersToolStripMenuItem.Name = "openParametersToolStripMenuItem";
            openParametersToolStripMenuItem.Size = new Size(186, 26);
            openParametersToolStripMenuItem.Text = "&Parameters";
            openParametersToolStripMenuItem.Click += btnParameters_Click;
            // 
            // openLookupTablesToolStripMenuItem
            // 
            openLookupTablesToolStripMenuItem.Name = "openLookupTablesToolStripMenuItem";
            openLookupTablesToolStripMenuItem.Size = new Size(186, 26);
            openLookupTablesToolStripMenuItem.Text = "&Lookup Tables";
            openLookupTablesToolStripMenuItem.Click += btnLookupTables_Click;
            // 
            // windowToolStripMenuItem
            // 
            windowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tileHorizontalToolStripMenuItem, tileVerticalToolStripMenuItem, cascadeToolStripMenuItem });
            windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            windowToolStripMenuItem.Size = new Size(78, 24);
            windowToolStripMenuItem.Text = "&Window";
            // 
            // tileHorizontalToolStripMenuItem
            // 
            tileHorizontalToolStripMenuItem.Name = "tileHorizontalToolStripMenuItem";
            tileHorizontalToolStripMenuItem.Size = new Size(190, 26);
            tileHorizontalToolStripMenuItem.Text = "Tile &Horizontal";
            tileHorizontalToolStripMenuItem.Click += btnTileHorizontal_Click;
            // 
            // tileVerticalToolStripMenuItem
            // 
            tileVerticalToolStripMenuItem.Name = "tileVerticalToolStripMenuItem";
            tileVerticalToolStripMenuItem.Size = new Size(190, 26);
            tileVerticalToolStripMenuItem.Text = "Tile &Vertical";
            tileVerticalToolStripMenuItem.Click += btnTileVertical_Click;
            // 
            // cascadeToolStripMenuItem
            // 
            cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            cascadeToolStripMenuItem.Size = new Size(190, 26);
            cascadeToolStripMenuItem.Text = "&Cascade";
            cascadeToolStripMenuItem.Click += btnCascade_Click;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { btnPolicyEditor, btnParameters, btnLookupTables, toolStripSeparator1, btnTileHorizontal, btnTileVertical, btnCascade });
            toolStrip.Location = new Point(0, 28);
            toolStrip.Name = "toolStrip";
            toolStrip.Padding = new Padding(4, 2, 4, 2);
            toolStrip.Size = new Size(1200, 31);
            toolStrip.TabIndex = 1;
            toolStrip.Visible = false;
            // 
            // btnPolicyEditor
            // 
            btnPolicyEditor.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnPolicyEditor.Name = "btnPolicyEditor";
            btnPolicyEditor.Size = new Size(95, 24);
            btnPolicyEditor.Text = "Policy Editor";
            btnPolicyEditor.ToolTipText = "Open the policy editor";
            btnPolicyEditor.Click += btnPolicyEditor_Click;
            // 
            // btnParameters
            // 
            btnParameters.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnParameters.Name = "btnParameters";
            btnParameters.Size = new Size(86, 24);
            btnParameters.Text = "Parameters";
            btnParameters.ToolTipText = "Open all-parameters grid";
            btnParameters.Click += btnParameters_Click;
            // 
            // btnLookupTables
            // 
            btnLookupTables.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLookupTables.Name = "btnLookupTables";
            btnLookupTables.Size = new Size(107, 24);
            btnLookupTables.Text = "Lookup Tables";
            btnLookupTables.ToolTipText = "Open lookup-tables editor";
            btnLookupTables.Click += btnLookupTables_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 27);
            // 
            // btnTileHorizontal
            // 
            btnTileHorizontal.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnTileHorizontal.Name = "btnTileHorizontal";
            btnTileHorizontal.Size = new Size(111, 24);
            btnTileHorizontal.Text = "Tile Horizontal";
            btnTileHorizontal.Click += btnTileHorizontal_Click;
            // 
            // btnTileVertical
            // 
            btnTileVertical.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnTileVertical.Name = "btnTileVertical";
            btnTileVertical.Size = new Size(90, 24);
            btnTileVertical.Text = "Tile Vertical";
            btnTileVertical.Click += btnTileVertical_Click;
            // 
            // btnCascade
            // 
            btnCascade.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCascade.Name = "btnCascade";
            btnCascade.Size = new Size(68, 24);
            btnCascade.Text = "Cascade";
            btnCascade.Click += btnCascade_Click;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 674);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 26);
            statusStrip.SizingGrip = false;
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 20);
            lblStatus.Text = "Ready";
            // 
            // frmPolicyManagerMdiHost
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1200, 700);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip;
            MinimumSize = new Size(1100, 700);
            Name = "frmPolicyManagerMdiHost";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Contribution / Return Policy Manager";
            WindowState = FormWindowState.Maximized;
            Load += frmPolicyManagerMdiHost_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
