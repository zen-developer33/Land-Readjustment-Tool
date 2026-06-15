namespace Land_Pooling_Policy_Manager.UI.Forms
{
    partial class frmPolicyManagerMdiHost
    {
        private System.ComponentModel.IContainer components = null;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenuItem;
        private ToolStripMenuItem policyEditorMenuItem;
        private ToolStripSeparator fileSeparator1;
        private ToolStripMenuItem closeMenuItem;
        private ToolStripMenuItem viewMenuItem;
        private ToolStripMenuItem parametersMenuItem;
        private ToolStripMenuItem lookupTablesMenuItem;
        private ToolStripMenuItem layoutMenuItem;
        private ToolStripMenuItem layout2VerticalMenuItem;
        private ToolStripMenuItem layout2HorizontalMenuItem;
        private ToolStripMenuItem layout3EditorLeftMenuItem;
        private ToolStripMenuItem layout3ColumnsMenuItem;
        private ToolStripMenuItem layout3RowsMenuItem;
        private ToolStripSeparator layoutSeparator1;
        private ToolStripMenuItem cascadeMenuItem;
        private ToolStripSeparator layoutSeparator2;
        private ToolStripMenuItem sizeMenuItem;
        private ToolStripMenuItem sizeFullMenuItem;
        private ToolStripMenuItem sizeComfortMenuItem;
        private ToolStripMenuItem sizeCompactMenuItem;
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
            fileMenuItem = new ToolStripMenuItem();
            policyEditorMenuItem = new ToolStripMenuItem();
            fileSeparator1 = new ToolStripSeparator();
            closeMenuItem = new ToolStripMenuItem();
            viewMenuItem = new ToolStripMenuItem();
            parametersMenuItem = new ToolStripMenuItem();
            lookupTablesMenuItem = new ToolStripMenuItem();
            layoutMenuItem = new ToolStripMenuItem();
            layout2VerticalMenuItem = new ToolStripMenuItem();
            layout2HorizontalMenuItem = new ToolStripMenuItem();
            layout3EditorLeftMenuItem = new ToolStripMenuItem();
            layout3ColumnsMenuItem = new ToolStripMenuItem();
            layout3RowsMenuItem = new ToolStripMenuItem();
            layoutSeparator1 = new ToolStripSeparator();
            cascadeMenuItem = new ToolStripMenuItem();
            layoutSeparator2 = new ToolStripSeparator();
            sizeMenuItem = new ToolStripMenuItem();
            sizeFullMenuItem = new ToolStripMenuItem();
            sizeComfortMenuItem = new ToolStripMenuItem();
            sizeCompactMenuItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // menuStrip
            //
            menuStrip.ImageScalingSize = new Size(20, 20);
            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenuItem, viewMenuItem, layoutMenuItem });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(1200, 28);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            //
            // fileMenuItem
            //
            fileMenuItem.DropDownItems.AddRange(new ToolStripItem[] { policyEditorMenuItem, fileSeparator1, closeMenuItem });
            fileMenuItem.Name = "fileMenuItem";
            fileMenuItem.Size = new Size(46, 24);
            fileMenuItem.Text = "&File";
            //
            // policyEditorMenuItem
            //
            policyEditorMenuItem.Name = "policyEditorMenuItem";
            policyEditorMenuItem.Size = new Size(200, 26);
            policyEditorMenuItem.Text = "&Policy Editor";
            policyEditorMenuItem.Click += btnPolicyEditor_Click;
            //
            // fileSeparator1
            //
            fileSeparator1.Name = "fileSeparator1";
            fileSeparator1.Size = new Size(197, 6);
            //
            // closeMenuItem
            //
            closeMenuItem.Name = "closeMenuItem";
            closeMenuItem.Size = new Size(200, 26);
            closeMenuItem.Text = "&Close";
            closeMenuItem.Click += btnClose_Click;
            //
            // viewMenuItem
            //
            viewMenuItem.DropDownItems.AddRange(new ToolStripItem[] { parametersMenuItem, lookupTablesMenuItem });
            viewMenuItem.Name = "viewMenuItem";
            viewMenuItem.Size = new Size(55, 24);
            viewMenuItem.Text = "&View";
            //
            // parametersMenuItem
            //
            parametersMenuItem.Name = "parametersMenuItem";
            parametersMenuItem.Size = new Size(200, 26);
            parametersMenuItem.Text = "&Parameters";
            parametersMenuItem.Click += btnParameters_Click;
            //
            // lookupTablesMenuItem
            //
            lookupTablesMenuItem.Name = "lookupTablesMenuItem";
            lookupTablesMenuItem.Size = new Size(200, 26);
            lookupTablesMenuItem.Text = "&Lookup Tables";
            lookupTablesMenuItem.Click += btnLookupTables_Click;
            //
            // layoutMenuItem
            //
            layoutMenuItem.DropDownItems.AddRange(new ToolStripItem[] { layout2VerticalMenuItem, layout2HorizontalMenuItem, layout3EditorLeftMenuItem, layout3ColumnsMenuItem, layout3RowsMenuItem, layoutSeparator1, cascadeMenuItem, layoutSeparator2, sizeMenuItem });
            layoutMenuItem.Name = "layoutMenuItem";
            layoutMenuItem.Size = new Size(67, 24);
            layoutMenuItem.Text = "&Layout";
            //
            // layout2VerticalMenuItem
            //
            layout2VerticalMenuItem.Name = "layout2VerticalMenuItem";
            layout2VerticalMenuItem.Size = new Size(220, 26);
            layout2VerticalMenuItem.Text = "2 &Vertical";
            layout2VerticalMenuItem.Click += btnLayout2Vertical_Click;
            //
            // layout2HorizontalMenuItem
            //
            layout2HorizontalMenuItem.Name = "layout2HorizontalMenuItem";
            layout2HorizontalMenuItem.Size = new Size(220, 26);
            layout2HorizontalMenuItem.Text = "2 &Horizontal";
            layout2HorizontalMenuItem.Click += btnLayout2Horizontal_Click;
            //
            // layout3EditorLeftMenuItem
            //
            layout3EditorLeftMenuItem.Name = "layout3EditorLeftMenuItem";
            layout3EditorLeftMenuItem.Size = new Size(220, 26);
            layout3EditorLeftMenuItem.Text = "&Editor + 2";
            layout3EditorLeftMenuItem.Click += btnLayout3EditorLeft_Click;
            //
            // layout3ColumnsMenuItem
            //
            layout3ColumnsMenuItem.Name = "layout3ColumnsMenuItem";
            layout3ColumnsMenuItem.Size = new Size(220, 26);
            layout3ColumnsMenuItem.Text = "3 &Columns";
            layout3ColumnsMenuItem.Click += btnLayout3Columns_Click;
            //
            // layout3RowsMenuItem
            //
            layout3RowsMenuItem.Name = "layout3RowsMenuItem";
            layout3RowsMenuItem.Size = new Size(220, 26);
            layout3RowsMenuItem.Text = "3 &Rows";
            layout3RowsMenuItem.Click += btnLayout3Rows_Click;
            //
            // layoutSeparator1
            //
            layoutSeparator1.Name = "layoutSeparator1";
            layoutSeparator1.Size = new Size(217, 6);
            //
            // cascadeMenuItem
            //
            cascadeMenuItem.Name = "cascadeMenuItem";
            cascadeMenuItem.Size = new Size(220, 26);
            cascadeMenuItem.Text = "C&ascade";
            cascadeMenuItem.Click += btnCascade_Click;
            //
            // layoutSeparator2
            //
            layoutSeparator2.Name = "layoutSeparator2";
            layoutSeparator2.Size = new Size(217, 6);
            //
            // sizeMenuItem
            //
            sizeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sizeFullMenuItem, sizeComfortMenuItem, sizeCompactMenuItem });
            sizeMenuItem.Name = "sizeMenuItem";
            sizeMenuItem.Size = new Size(220, 26);
            sizeMenuItem.Text = "&Size";
            //
            // sizeFullMenuItem
            //
            sizeFullMenuItem.Checked = true;
            sizeFullMenuItem.CheckState = CheckState.Checked;
            sizeFullMenuItem.Name = "sizeFullMenuItem";
            sizeFullMenuItem.Size = new Size(200, 26);
            sizeFullMenuItem.Tag = "0";
            sizeFullMenuItem.Text = "&Full";
            sizeFullMenuItem.Click += sizeMenuItem_Click;
            //
            // sizeComfortMenuItem
            //
            sizeComfortMenuItem.Name = "sizeComfortMenuItem";
            sizeComfortMenuItem.Size = new Size(200, 26);
            sizeComfortMenuItem.Tag = "1";
            sizeComfortMenuItem.Text = "Comfort &90%";
            sizeComfortMenuItem.Click += sizeMenuItem_Click;
            //
            // sizeCompactMenuItem
            //
            sizeCompactMenuItem.Name = "sizeCompactMenuItem";
            sizeCompactMenuItem.Size = new Size(200, 26);
            sizeCompactMenuItem.Tag = "2";
            sizeCompactMenuItem.Text = "Compact &75%";
            sizeCompactMenuItem.Click += sizeMenuItem_Click;
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
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
            RecordFormTheme.Apply(this);
        }
    }
}
