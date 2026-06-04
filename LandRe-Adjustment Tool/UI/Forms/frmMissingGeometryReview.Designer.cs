namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmMissingGeometryReview
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layout;
        private FlowLayoutPanel filters;
        private Label lblSearch;
        private TextBox _searchBox;
        private Label lblIssue;
        private ComboBox _issueFilter;
        private DataGridView _grid;
        private DataGridViewTextBoxColumn colIssueType;
        private DataGridViewTextBoxColumn colMapSheetNo;
        private DataGridViewTextBoxColumn colParcelNo;
        private DataGridViewTextBoxColumn colOwnerName;
        private DataGridViewTextBoxColumn colAreaSqm;
        private DataGridViewTextBoxColumn colCanvasObjectId;
        private DataGridViewTextBoxColumn colDetail;
        private Label _statusLabel;
        private FlowLayoutPanel actions;
        private Button _closeButton;
        private Button _openParcelLinkReviewButton;
        private Button _clearBrokenLinksButton;
        private Button _exportButton;
        private Button _refreshButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            layout = new TableLayoutPanel();
            filters = new FlowLayoutPanel();
            lblSearch = new Label();
            _searchBox = new TextBox();
            lblIssue = new Label();
            _issueFilter = new ComboBox();
            _grid = new DataGridView();
            colIssueType = new DataGridViewTextBoxColumn();
            colMapSheetNo = new DataGridViewTextBoxColumn();
            colParcelNo = new DataGridViewTextBoxColumn();
            colOwnerName = new DataGridViewTextBoxColumn();
            colAreaSqm = new DataGridViewTextBoxColumn();
            colCanvasObjectId = new DataGridViewTextBoxColumn();
            colDetail = new DataGridViewTextBoxColumn();
            _statusLabel = new Label();
            actions = new FlowLayoutPanel();
            _closeButton = new Button();
            _openParcelLinkReviewButton = new Button();
            _clearBrokenLinksButton = new Button();
            _exportButton = new Button();
            _refreshButton = new Button();
            layout.SuspendLayout();
            filters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).BeginInit();
            actions.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(filters, 0, 0);
            layout.Controls.Add(_grid, 0, 1);
            layout.Controls.Add(_statusLabel, 0, 2);
            layout.Controls.Add(actions, 0, 3);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Name = "layout";
            layout.Padding = new Padding(12);
            layout.RowCount = 4;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            layout.Size = new Size(1120, 680);
            layout.TabIndex = 0;
            // 
            // filters
            // 
            filters.Controls.Add(lblSearch);
            filters.Controls.Add(_searchBox);
            filters.Controls.Add(lblIssue);
            filters.Controls.Add(_issueFilter);
            filters.Dock = DockStyle.Fill;
            filters.Location = new Point(15, 15);
            filters.Name = "filters";
            filters.Size = new Size(1090, 34);
            filters.TabIndex = 0;
            filters.WrapContents = false;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = false;
            lblSearch.Location = new Point(3, 0);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(58, 30);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search";
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _searchBox
            // 
            _searchBox.Location = new Point(67, 3);
            _searchBox.Name = "_searchBox";
            _searchBox.Size = new Size(260, 27);
            _searchBox.TabIndex = 1;
            // 
            // lblIssue
            // 
            lblIssue.AutoSize = false;
            lblIssue.Location = new Point(346, 3);
            lblIssue.Margin = new Padding(16, 3, 3, 3);
            lblIssue.Name = "lblIssue";
            lblIssue.Size = new Size(48, 30);
            lblIssue.TabIndex = 2;
            lblIssue.Text = "Issue";
            lblIssue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _issueFilter
            // 
            _issueFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _issueFilter.FormattingEnabled = true;
            _issueFilter.Location = new Point(400, 3);
            _issueFilter.Name = "_issueFilter";
            _issueFilter.Size = new Size(230, 28);
            _issueFilter.TabIndex = 3;
            // 
            // _grid
            // 
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AutoGenerateColumns = false;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(245, 247, 250);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(31, 41, 55);
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            _grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _grid.Columns.AddRange(new DataGridViewColumn[] { colIssueType, colMapSheetNo, colParcelNo, colOwnerName, colAreaSqm, colCanvasObjectId, colDetail });
            _grid.Dock = DockStyle.Fill;
            _grid.EnableHeadersVisualStyles = false;
            _grid.Location = new Point(15, 55);
            _grid.MultiSelect = true;
            _grid.Name = "_grid";
            _grid.ReadOnly = true;
            _grid.RowHeadersVisible = false;
            _grid.RowHeadersWidth = 51;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.Size = new Size(1090, 528);
            _grid.TabIndex = 1;
            // 
            // colIssueType
            // 
            colIssueType.DataPropertyName = "IssueType";
            colIssueType.HeaderText = "Issue";
            colIssueType.MinimumWidth = 6;
            colIssueType.Name = "IssueType";
            colIssueType.ReadOnly = true;
            colIssueType.Width = 170;
            // 
            // colMapSheetNo
            // 
            colMapSheetNo.DataPropertyName = "MapSheetNo";
            colMapSheetNo.HeaderText = "Map Sheet";
            colMapSheetNo.MinimumWidth = 6;
            colMapSheetNo.Name = "MapSheetNo";
            colMapSheetNo.ReadOnly = true;
            colMapSheetNo.Width = 105;
            // 
            // colParcelNo
            // 
            colParcelNo.DataPropertyName = "ParcelNo";
            colParcelNo.HeaderText = "Parcel No";
            colParcelNo.MinimumWidth = 6;
            colParcelNo.Name = "ParcelNo";
            colParcelNo.ReadOnly = true;
            colParcelNo.Width = 105;
            // 
            // colOwnerName
            // 
            colOwnerName.DataPropertyName = "OwnerName";
            colOwnerName.HeaderText = "Owner";
            colOwnerName.MinimumWidth = 6;
            colOwnerName.Name = "OwnerName";
            colOwnerName.ReadOnly = true;
            colOwnerName.Width = 220;
            // 
            // colAreaSqm
            // 
            colAreaSqm.DataPropertyName = "AreaSqm";
            colAreaSqm.HeaderText = "Area sq.m";
            colAreaSqm.MinimumWidth = 6;
            colAreaSqm.Name = "AreaSqm";
            colAreaSqm.ReadOnly = true;
            // 
            // colCanvasObjectId
            // 
            colCanvasObjectId.DataPropertyName = "CanvasObjectId";
            colCanvasObjectId.HeaderText = "Canvas Object";
            colCanvasObjectId.MinimumWidth = 6;
            colCanvasObjectId.Name = "CanvasObjectId";
            colCanvasObjectId.ReadOnly = true;
            colCanvasObjectId.Width = 210;
            // 
            // colDetail
            // 
            colDetail.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDetail.DataPropertyName = "Detail";
            colDetail.HeaderText = "Detail";
            colDetail.MinimumWidth = 6;
            colDetail.Name = "Detail";
            colDetail.ReadOnly = true;
            // 
            // _statusLabel
            // 
            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.ForeColor = Color.FromArgb(75, 85, 99);
            _statusLabel.Location = new Point(15, 586);
            _statusLabel.Name = "_statusLabel";
            _statusLabel.Size = new Size(1090, 34);
            _statusLabel.TabIndex = 2;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // actions
            // 
            actions.Controls.Add(_closeButton);
            actions.Controls.Add(_openParcelLinkReviewButton);
            actions.Controls.Add(_clearBrokenLinksButton);
            actions.Controls.Add(_exportButton);
            actions.Controls.Add(_refreshButton);
            actions.Dock = DockStyle.Fill;
            actions.FlowDirection = FlowDirection.RightToLeft;
            actions.Location = new Point(15, 623);
            actions.Name = "actions";
            actions.Size = new Size(1090, 42);
            actions.TabIndex = 3;
            actions.WrapContents = false;
            // 
            // _closeButton
            // 
            _closeButton.Location = new Point(993, 3);
            _closeButton.Name = "_closeButton";
            _closeButton.Size = new Size(94, 30);
            _closeButton.TabIndex = 0;
            _closeButton.Text = "Close";
            _closeButton.UseVisualStyleBackColor = true;
            // 
            // _openParcelLinkReviewButton
            // 
            _openParcelLinkReviewButton.Location = new Point(811, 3);
            _openParcelLinkReviewButton.Name = "_openParcelLinkReviewButton";
            _openParcelLinkReviewButton.Size = new Size(176, 30);
            _openParcelLinkReviewButton.TabIndex = 1;
            _openParcelLinkReviewButton.Text = "Open Parcel-Link Review";
            _openParcelLinkReviewButton.UseVisualStyleBackColor = true;
            // 
            // _clearBrokenLinksButton
            // 
            _clearBrokenLinksButton.Location = new Point(663, 3);
            _clearBrokenLinksButton.Name = "_clearBrokenLinksButton";
            _clearBrokenLinksButton.Size = new Size(142, 30);
            _clearBrokenLinksButton.TabIndex = 2;
            _clearBrokenLinksButton.Text = "Clear Broken Links";
            _clearBrokenLinksButton.UseVisualStyleBackColor = true;
            // 
            // _exportButton
            // 
            _exportButton.Location = new Point(553, 3);
            _exportButton.Name = "_exportButton";
            _exportButton.Size = new Size(104, 30);
            _exportButton.TabIndex = 3;
            _exportButton.Text = "Export CSV";
            _exportButton.UseVisualStyleBackColor = true;
            // 
            // _refreshButton
            // 
            _refreshButton.Location = new Point(453, 3);
            _refreshButton.Name = "_refreshButton";
            _refreshButton.Size = new Size(94, 30);
            _refreshButton.TabIndex = 4;
            _refreshButton.Text = "Refresh";
            _refreshButton.UseVisualStyleBackColor = true;
            // 
            // frmMissingGeometryReview
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1120, 680);
            Controls.Add(layout);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(920, 560);
            Name = "frmMissingGeometryReview";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Missing Geometry Review";
            layout.ResumeLayout(false);
            filters.ResumeLayout(false);
            filters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_grid).EndInit();
            actions.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
