namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPolicyManagerMdiHost
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openDashboardToolStripMenuItem;
        private ToolStripMenuItem closeToolStripMenuItem;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem tileHorizontalToolStripMenuItem;
        private ToolStripMenuItem tileVerticalToolStripMenuItem;
        private ToolStripMenuItem cascadeToolStripMenuItem;
        private ToolStrip toolStrip;
        private ToolStripButton btnDashboard;
        private ToolStripButton btnParameters;
        private ToolStripButton btnLookupTables;
        private ToolStripButton btnCornerTypes;
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
            openDashboardToolStripMenuItem = new ToolStripMenuItem();
            closeToolStripMenuItem = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            tileHorizontalToolStripMenuItem = new ToolStripMenuItem();
            tileVerticalToolStripMenuItem = new ToolStripMenuItem();
            cascadeToolStripMenuItem = new ToolStripMenuItem();
            toolStrip = new ToolStrip();
            btnDashboard = new ToolStripButton();
            btnParameters = new ToolStripButton();
            btnLookupTables = new ToolStripButton();
            btnCornerTypes = new ToolStripButton();
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
            menuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, windowToolStripMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1200, 28);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openDashboardToolStripMenuItem, new ToolStripSeparator(), closeToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "&File";
            // 
            // openDashboardToolStripMenuItem
            // 
            openDashboardToolStripMenuItem.Name = "openDashboardToolStripMenuItem";
            openDashboardToolStripMenuItem.Size = new Size(211, 26);
            openDashboardToolStripMenuItem.Text = "&Policy Dashboard";
            openDashboardToolStripMenuItem.Click += openDashboardToolStripMenuItem_Click;
            // 
            // closeToolStripMenuItem
            // 
            closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            closeToolStripMenuItem.Size = new Size(211, 26);
            closeToolStripMenuItem.Text = "Close";
            closeToolStripMenuItem.Click += closeToolStripMenuItem_Click;
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
            tileHorizontalToolStripMenuItem.Size = new Size(195, 26);
            tileHorizontalToolStripMenuItem.Text = "Tile Horizontal";
            tileHorizontalToolStripMenuItem.Click += tileHorizontalToolStripMenuItem_Click;
            // 
            // tileVerticalToolStripMenuItem
            // 
            tileVerticalToolStripMenuItem.Name = "tileVerticalToolStripMenuItem";
            tileVerticalToolStripMenuItem.Size = new Size(195, 26);
            tileVerticalToolStripMenuItem.Text = "Tile Vertical";
            tileVerticalToolStripMenuItem.Click += tileVerticalToolStripMenuItem_Click;
            // 
            // cascadeToolStripMenuItem
            // 
            cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            cascadeToolStripMenuItem.Size = new Size(195, 26);
            cascadeToolStripMenuItem.Text = "Cascade";
            cascadeToolStripMenuItem.Click += cascadeToolStripMenuItem_Click;
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { btnDashboard, btnParameters, btnLookupTables, btnCornerTypes });
            toolStrip.Location = new Point(0, 28);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1200, 27);
            toolStrip.TabIndex = 1;
            // 
            // btnDashboard
            // 
            btnDashboard.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDashboard.Name = "btnDashboard";
            btnDashboard.Size = new Size(86, 24);
            btnDashboard.Text = "Dashboard";
            btnDashboard.Click += btnDashboard_Click;
            // 
            // btnParameters
            // 
            btnParameters.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnParameters.Name = "btnParameters";
            btnParameters.Size = new Size(89, 24);
            btnParameters.Text = "Parameters";
            btnParameters.Click += btnParameters_Click;
            // 
            // btnLookupTables
            // 
            btnLookupTables.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLookupTables.Name = "btnLookupTables";
            btnLookupTables.Size = new Size(105, 24);
            btnLookupTables.Text = "Lookup Tables";
            btnLookupTables.Click += btnLookupTables_Click;
            // 
            // btnCornerTypes
            // 
            btnCornerTypes.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnCornerTypes.Name = "btnCornerTypes";
            btnCornerTypes.Size = new Size(98, 24);
            btnCornerTypes.Text = "Corner Types";
            btnCornerTypes.Click += btnCornerTypes_Click;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new Size(20, 20);
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip.Location = new Point(0, 676);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1200, 24);
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(50, 18);
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
