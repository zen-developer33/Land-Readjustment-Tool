namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmOriginalScenarioSummary
    {
        private System.ComponentModel.IContainer components = null;
        private Panel header;
        private Label title;
        private Label _subtitle;
        private FlowLayoutPanel actions;
        private Button _btnClose;
        private Button _btnExport;
        private Button _btnRefresh;
        private FlowLayoutPanel _metricPanel;
        private TabControl _tabs;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            header = new Panel();
            actions = new FlowLayoutPanel();
            _btnClose = new Button();
            _btnExport = new Button();
            _btnRefresh = new Button();
            _subtitle = new Label();
            title = new Label();
            _metricPanel = new FlowLayoutPanel();
            _tabs = new TabControl();
            header.SuspendLayout();
            actions.SuspendLayout();
            SuspendLayout();
            // 
            // header
            // 
            header.BackColor = Color.FromArgb(31, 45, 64);
            header.Controls.Add(actions);
            header.Controls.Add(_subtitle);
            header.Controls.Add(title);
            header.Dock = DockStyle.Top;
            header.Location = new Point(0, 0);
            header.Name = "header";
            header.Padding = new Padding(22, 14, 22, 12);
            header.Size = new Size(1260, 112);
            header.TabIndex = 0;
            // 
            // actions
            // 
            actions.Controls.Add(_btnClose);
            actions.Controls.Add(_btnExport);
            actions.Controls.Add(_btnRefresh);
            actions.Dock = DockStyle.Bottom;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.Location = new Point(22, 64);
            actions.Name = "actions";
            actions.Size = new Size(1216, 36);
            actions.TabIndex = 2;
            actions.WrapContents = false;
            // 
            // _btnClose
            // 
            _btnClose.BackColor = Color.FromArgb(82, 96, 113);
            _btnClose.Cursor = Cursors.Hand;
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.FlatStyle = FlatStyle.Flat;
            _btnClose.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            _btnClose.ForeColor = Color.White;
            _btnClose.Location = new Point(1104, 2);
            _btnClose.Margin = new Padding(8, 2, 0, 2);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new Size(112, 31);
            _btnClose.TabIndex = 0;
            _btnClose.Text = "Close";
            _btnClose.UseVisualStyleBackColor = false;
            // 
            // _btnExport
            // 
            _btnExport.BackColor = Color.FromArgb(32, 128, 92);
            _btnExport.Cursor = Cursors.Hand;
            _btnExport.FlatAppearance.BorderSize = 0;
            _btnExport.FlatStyle = FlatStyle.Flat;
            _btnExport.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            _btnExport.ForeColor = Color.White;
            _btnExport.Location = new Point(984, 2);
            _btnExport.Margin = new Padding(8, 2, 0, 2);
            _btnExport.Name = "_btnExport";
            _btnExport.Size = new Size(112, 31);
            _btnExport.TabIndex = 1;
            _btnExport.Text = "Export XLS";
            _btnExport.UseVisualStyleBackColor = false;
            // 
            // _btnRefresh
            // 
            _btnRefresh.BackColor = Color.FromArgb(56, 108, 176);
            _btnRefresh.Cursor = Cursors.Hand;
            _btnRefresh.FlatAppearance.BorderSize = 0;
            _btnRefresh.FlatStyle = FlatStyle.Flat;
            _btnRefresh.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            _btnRefresh.ForeColor = Color.White;
            _btnRefresh.Location = new Point(864, 2);
            _btnRefresh.Margin = new Padding(8, 2, 0, 2);
            _btnRefresh.Name = "_btnRefresh";
            _btnRefresh.Size = new Size(112, 31);
            _btnRefresh.TabIndex = 2;
            _btnRefresh.Text = "Refresh";
            _btnRefresh.UseVisualStyleBackColor = false;
            // 
            // _subtitle
            // 
            _subtitle.Dock = DockStyle.Top;
            _subtitle.ForeColor = Color.FromArgb(210, 222, 238);
            _subtitle.Location = new Point(22, 48);
            _subtitle.Name = "_subtitle";
            _subtitle.Size = new Size(1216, 24);
            _subtitle.TabIndex = 1;
            _subtitle.Text = "Project database summary, checks, and original land-use scenario.";
            _subtitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // title
            // 
            title.Dock = DockStyle.Top;
            title.Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Location = new Point(22, 14);
            title.Name = "title";
            title.Size = new Size(1216, 34);
            title.TabIndex = 0;
            title.Text = "Original Scenario Summary";
            title.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _metricPanel
            // 
            _metricPanel.AutoScroll = true;
            _metricPanel.BackColor = Color.FromArgb(244, 247, 251);
            _metricPanel.Dock = DockStyle.Top;
            _metricPanel.Location = new Point(0, 112);
            _metricPanel.Name = "_metricPanel";
            _metricPanel.Padding = new Padding(14, 12, 14, 6);
            _metricPanel.Size = new Size(1260, 118);
            _metricPanel.TabIndex = 1;
            _metricPanel.WrapContents = false;
            // 
            // _tabs
            // 
            _tabs.Appearance = TabAppearance.Normal;
            _tabs.Dock = DockStyle.Fill;
            _tabs.Location = new Point(0, 230);
            _tabs.Name = "_tabs";
            _tabs.Padding = new Point(16, 7);
            _tabs.SelectedIndex = 0;
            _tabs.Size = new Size(1260, 550);
            _tabs.TabIndex = 2;
            // 
            // frmOriginalScenarioSummary
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(244, 247, 251);
            ClientSize = new Size(1260, 780);
            Controls.Add(_tabs);
            Controls.Add(_metricPanel);
            Controls.Add(header);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1120, 720);
            Name = "frmOriginalScenarioSummary";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Original Scenario Summary";
            header.ResumeLayout(false);
            actions.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
