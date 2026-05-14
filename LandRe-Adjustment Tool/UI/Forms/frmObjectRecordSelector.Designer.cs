using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmObjectRecordSelector
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private TabControl tabRecords;
        private TabPage tabOriginalParcels;
        private TabPage tabReplottedParcels;
        private TabPage tabBlocks;
        private TabPage tabRoads;
        private TableLayoutPanel originalLayout;
        private Panel searchPanel;
        private Label lblSearch;
        private TextBox txtSearch;
        private Button btnClearSearch;
        private FlowLayoutPanel selectionToolsPanel;
        private Button btnSelectAll;
        private Button btnSelectNone;
        private Button btnDeselectVisible;
        private Button btnInvertSelection;
        private DataGridView dgvOriginalParcels;
        private DataGridViewCheckBoxColumn colSelected;
        private DataGridViewTextBoxColumn colParcelNo;
        private DataGridViewTextBoxColumn colMapSheetNo;
        private DataGridViewTextBoxColumn colOwner;
        private DataGridViewTextBoxColumn colArea;
        private DataGridViewTextBoxColumn colLayer;
        private DataGridViewTextBoxColumn colStatus;
        private Panel footerPanel;
        private CheckBox chkZoomToSelection;
        private Label lblStatus;
        private FlowLayoutPanel actionPanel;
        private Button btnCancel;
        private Button btnApply;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();
            mainLayout = new TableLayoutPanel();
            headerPanel = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            tabRecords = new TabControl();
            tabOriginalParcels = new TabPage();
            originalLayout = new TableLayoutPanel();
            searchPanel = new Panel();
            lblSearch = new Label();
            txtSearch = new TextBox();
            btnClearSearch = new Button();
            selectionToolsPanel = new FlowLayoutPanel();
            btnSelectAll = new Button();
            btnSelectNone = new Button();
            btnDeselectVisible = new Button();
            btnInvertSelection = new Button();
            dgvOriginalParcels = new DataGridView();
            colSelected = new DataGridViewCheckBoxColumn();
            colParcelNo = new DataGridViewTextBoxColumn();
            colMapSheetNo = new DataGridViewTextBoxColumn();
            colOwner = new DataGridViewTextBoxColumn();
            colArea = new DataGridViewTextBoxColumn();
            colLayer = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            tabReplottedParcels = new TabPage();
            tabBlocks = new TabPage();
            tabRoads = new TabPage();
            footerPanel = new Panel();
            chkZoomToSelection = new CheckBox();
            lblStatus = new Label();
            actionPanel = new FlowLayoutPanel();
            btnCancel = new Button();
            btnApply = new Button();
            mainLayout.SuspendLayout();
            headerPanel.SuspendLayout();
            tabRecords.SuspendLayout();
            tabOriginalParcels.SuspendLayout();
            originalLayout.SuspendLayout();
            searchPanel.SuspendLayout();
            selectionToolsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOriginalParcels).BeginInit();
            footerPanel.SuspendLayout();
            actionPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.Controls.Add(tabRecords, 0, 1);
            mainLayout.Controls.Add(footerPanel, 0, 2);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(12);
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
            mainLayout.Size = new Size(920, 620);
            mainLayout.TabIndex = 0;
            // 
            // headerPanel
            // 
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblSubtitle);
            headerPanel.Dock = DockStyle.Fill;
            headerPanel.Location = new Point(15, 15);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new Size(890, 58);
            headerPanel.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 41, 55);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(217, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Select From Records";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.FromArgb(89, 99, 110);
            lblSubtitle.Location = new Point(2, 34);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(486, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Search records, check one or more linked objects, then apply selection.";
            // 
            // tabRecords
            // 
            tabRecords.Controls.Add(tabOriginalParcels);
            tabRecords.Controls.Add(tabReplottedParcels);
            tabRecords.Controls.Add(tabBlocks);
            tabRecords.Controls.Add(tabRoads);
            tabRecords.Dock = DockStyle.Fill;
            tabRecords.Location = new Point(15, 79);
            tabRecords.Name = "tabRecords";
            tabRecords.SelectedIndex = 0;
            tabRecords.Size = new Size(890, 472);
            tabRecords.TabIndex = 1;
            // 
            // tabOriginalParcels
            // 
            tabOriginalParcels.Controls.Add(originalLayout);
            tabOriginalParcels.Location = new Point(4, 29);
            tabOriginalParcels.Name = "tabOriginalParcels";
            tabOriginalParcels.Padding = new Padding(10);
            tabOriginalParcels.Size = new Size(882, 439);
            tabOriginalParcels.TabIndex = 0;
            tabOriginalParcels.Text = "Original Parcel Records";
            tabOriginalParcels.UseVisualStyleBackColor = true;
            // 
            // originalLayout
            // 
            originalLayout.ColumnCount = 1;
            originalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            originalLayout.Controls.Add(searchPanel, 0, 0);
            originalLayout.Controls.Add(selectionToolsPanel, 0, 1);
            originalLayout.Controls.Add(dgvOriginalParcels, 0, 2);
            originalLayout.Dock = DockStyle.Fill;
            originalLayout.Location = new Point(10, 10);
            originalLayout.Name = "originalLayout";
            originalLayout.RowCount = 3;
            originalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            originalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            originalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            originalLayout.Size = new Size(862, 419);
            originalLayout.TabIndex = 0;
            // 
            // searchPanel
            // 
            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnClearSearch);
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.Location = new Point(0, 0);
            searchPanel.Margin = new Padding(0);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(862, 44);
            searchPanel.TabIndex = 0;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSearch.Location = new Point(0, 11);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(56, 20);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search";
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(70, 7);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Parcel no, map sheet, owner, layer...";
            txtSearch.Size = new Size(680, 27);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // btnClearSearch
            // 
            btnClearSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearSearch.Location = new Point(762, 5);
            btnClearSearch.Name = "btnClearSearch";
            btnClearSearch.Size = new Size(96, 31);
            btnClearSearch.TabIndex = 2;
            btnClearSearch.Text = "Clear";
            btnClearSearch.UseVisualStyleBackColor = true;
            btnClearSearch.Click += btnClearSearch_Click;
            // 
            // selectionToolsPanel
            // 
            selectionToolsPanel.Controls.Add(btnSelectAll);
            selectionToolsPanel.Controls.Add(btnSelectNone);
            selectionToolsPanel.Controls.Add(btnDeselectVisible);
            selectionToolsPanel.Controls.Add(btnInvertSelection);
            selectionToolsPanel.Dock = DockStyle.Fill;
            selectionToolsPanel.Location = new Point(0, 44);
            selectionToolsPanel.Margin = new Padding(0);
            selectionToolsPanel.Name = "selectionToolsPanel";
            selectionToolsPanel.Padding = new Padding(0, 5, 0, 0);
            selectionToolsPanel.Size = new Size(862, 44);
            selectionToolsPanel.TabIndex = 1;
            // 
            // btnSelectAll
            // 
            btnSelectAll.Location = new Point(0, 5);
            btnSelectAll.Margin = new Padding(0, 0, 8, 0);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.Size = new Size(96, 31);
            btnSelectAll.TabIndex = 0;
            btnSelectAll.Text = "Select All";
            btnSelectAll.UseVisualStyleBackColor = true;
            btnSelectAll.Click += btnSelectAll_Click;
            // 
            // btnSelectNone
            // 
            btnSelectNone.Location = new Point(104, 5);
            btnSelectNone.Margin = new Padding(0, 0, 8, 0);
            btnSelectNone.Name = "btnSelectNone";
            btnSelectNone.Size = new Size(102, 31);
            btnSelectNone.TabIndex = 1;
            btnSelectNone.Text = "Select None";
            btnSelectNone.UseVisualStyleBackColor = true;
            btnSelectNone.Click += btnSelectNone_Click;
            // 
            // btnDeselectVisible
            // 
            btnDeselectVisible.Location = new Point(214, 5);
            btnDeselectVisible.Margin = new Padding(0, 0, 8, 0);
            btnDeselectVisible.Name = "btnDeselectVisible";
            btnDeselectVisible.Size = new Size(124, 31);
            btnDeselectVisible.TabIndex = 2;
            btnDeselectVisible.Text = "Deselect";
            btnDeselectVisible.UseVisualStyleBackColor = true;
            btnDeselectVisible.Click += btnDeselectVisible_Click;
            // 
            // btnInvertSelection
            // 
            btnInvertSelection.Location = new Point(346, 5);
            btnInvertSelection.Margin = new Padding(0, 0, 8, 0);
            btnInvertSelection.Name = "btnInvertSelection";
            btnInvertSelection.Size = new Size(132, 31);
            btnInvertSelection.TabIndex = 3;
            btnInvertSelection.Text = "Inverse Selection";
            btnInvertSelection.UseVisualStyleBackColor = true;
            btnInvertSelection.Click += btnInvertSelection_Click;
            // 
            // dgvOriginalParcels
            // 
            dgvOriginalParcels.AllowUserToAddRows = false;
            dgvOriginalParcels.AllowUserToDeleteRows = false;
            dgvOriginalParcels.AllowUserToResizeRows = false;
            dgvOriginalParcels.BackgroundColor = Color.White;
            dgvOriginalParcels.BorderStyle = BorderStyle.Fixed3D;
            dgvOriginalParcels.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOriginalParcels.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            headerStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            headerStyle.BackColor = Color.FromArgb(245, 247, 250);
            headerStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            headerStyle.ForeColor = Color.FromArgb(31, 41, 55);
            headerStyle.SelectionBackColor = Color.FromArgb(245, 247, 250);
            headerStyle.SelectionForeColor = Color.FromArgb(31, 41, 55);
            headerStyle.WrapMode = DataGridViewTriState.True;
            dgvOriginalParcels.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvOriginalParcels.ColumnHeadersHeight = 34;
            dgvOriginalParcels.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOriginalParcels.Columns.AddRange(new DataGridViewColumn[] { colSelected, colParcelNo, colMapSheetNo, colOwner, colArea, colLayer, colStatus });
            cellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            cellStyle.BackColor = Color.White;
            cellStyle.Font = new Font("Segoe UI", 9F);
            cellStyle.ForeColor = Color.FromArgb(31, 41, 55);
            cellStyle.SelectionBackColor = Color.FromArgb(226, 240, 255);
            cellStyle.SelectionForeColor = Color.FromArgb(17, 24, 39);
            cellStyle.WrapMode = DataGridViewTriState.False;
            dgvOriginalParcels.DefaultCellStyle = cellStyle;
            dgvOriginalParcels.Dock = DockStyle.Fill;
            dgvOriginalParcels.EnableHeadersVisualStyles = false;
            dgvOriginalParcels.GridColor = Color.FromArgb(225, 229, 235);
            dgvOriginalParcels.Location = new Point(0, 88);
            dgvOriginalParcels.Margin = new Padding(0);
            dgvOriginalParcels.MultiSelect = true;
            dgvOriginalParcels.Name = "dgvOriginalParcels";
            dgvOriginalParcels.RowHeadersVisible = false;
            dgvOriginalParcels.RowHeadersWidth = 51;
            dgvOriginalParcels.RowTemplate.Height = 30;
            dgvOriginalParcels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOriginalParcels.Size = new Size(862, 331);
            dgvOriginalParcels.TabIndex = 2;
            dgvOriginalParcels.CellContentClick += dgvOriginalParcels_CellContentClick;
            dgvOriginalParcels.CellValueChanged += dgvOriginalParcels_CellValueChanged;
            dgvOriginalParcels.CurrentCellDirtyStateChanged += dgvOriginalParcels_CurrentCellDirtyStateChanged;
            // 
            // colSelected
            // 
            colSelected.HeaderText = "";
            colSelected.MinimumWidth = 6;
            colSelected.Name = "colSelected";
            colSelected.Width = 42;
            // 
            // colParcelNo
            // 
            colParcelNo.HeaderText = "Parcel No.";
            colParcelNo.MinimumWidth = 6;
            colParcelNo.Name = "colParcelNo";
            colParcelNo.ReadOnly = true;
            colParcelNo.Width = 105;
            // 
            // colMapSheetNo
            // 
            colMapSheetNo.HeaderText = "Map Sheet";
            colMapSheetNo.MinimumWidth = 6;
            colMapSheetNo.Name = "colMapSheetNo";
            colMapSheetNo.ReadOnly = true;
            colMapSheetNo.Width = 130;
            // 
            // colOwner
            // 
            colOwner.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colOwner.HeaderText = "Owner";
            colOwner.MinimumWidth = 150;
            colOwner.Name = "colOwner";
            colOwner.ReadOnly = true;
            // 
            // colArea
            // 
            colArea.HeaderText = "Area";
            colArea.MinimumWidth = 6;
            colArea.Name = "colArea";
            colArea.ReadOnly = true;
            colArea.Width = 120;
            // 
            // colLayer
            // 
            colLayer.HeaderText = "Layer";
            colLayer.MinimumWidth = 6;
            colLayer.Name = "colLayer";
            colLayer.ReadOnly = true;
            colLayer.Width = 140;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "Status";
            colStatus.MinimumWidth = 6;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.Width = 120;
            // 
            // tabReplottedParcels
            // 
            tabReplottedParcels.Location = new Point(4, 29);
            tabReplottedParcels.Name = "tabReplottedParcels";
            tabReplottedParcels.Padding = new Padding(10);
            tabReplottedParcels.Size = new Size(882, 439);
            tabReplottedParcels.TabIndex = 1;
            tabReplottedParcels.Text = "Replotted Parcel Records";
            tabReplottedParcels.UseVisualStyleBackColor = true;
            // 
            // tabBlocks
            // 
            tabBlocks.Location = new Point(4, 29);
            tabBlocks.Name = "tabBlocks";
            tabBlocks.Padding = new Padding(10);
            tabBlocks.Size = new Size(882, 439);
            tabBlocks.TabIndex = 2;
            tabBlocks.Text = "Blocks";
            tabBlocks.UseVisualStyleBackColor = true;
            // 
            // tabRoads
            // 
            tabRoads.Location = new Point(4, 29);
            tabRoads.Name = "tabRoads";
            tabRoads.Padding = new Padding(10);
            tabRoads.Size = new Size(882, 439);
            tabRoads.TabIndex = 3;
            tabRoads.Text = "Roads";
            tabRoads.UseVisualStyleBackColor = true;
            // 
            // footerPanel
            // 
            footerPanel.Controls.Add(chkZoomToSelection);
            footerPanel.Controls.Add(lblStatus);
            footerPanel.Controls.Add(actionPanel);
            footerPanel.Dock = DockStyle.Fill;
            footerPanel.Location = new Point(15, 557);
            footerPanel.Name = "footerPanel";
            footerPanel.Size = new Size(890, 48);
            footerPanel.TabIndex = 2;
            // 
            // chkZoomToSelection
            // 
            chkZoomToSelection.AutoSize = true;
            chkZoomToSelection.Checked = true;
            chkZoomToSelection.CheckState = CheckState.Checked;
            chkZoomToSelection.Location = new Point(0, 12);
            chkZoomToSelection.Name = "chkZoomToSelection";
            chkZoomToSelection.Size = new Size(150, 24);
            chkZoomToSelection.TabIndex = 0;
            chkZoomToSelection.Text = "Zoom to selection";
            chkZoomToSelection.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.ForeColor = Color.FromArgb(89, 99, 110);
            lblStatus.Location = new Point(166, 11);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(468, 25);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Ready.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // actionPanel
            // 
            actionPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            actionPanel.Controls.Add(btnCancel);
            actionPanel.Controls.Add(btnApply);
            actionPanel.FlowDirection = FlowDirection.RightToLeft;
            actionPanel.Location = new Point(640, 6);
            actionPanel.Name = "actionPanel";
            actionPanel.Size = new Size(250, 38);
            actionPanel.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(149, 0);
            btnCancel.Margin = new Padding(8, 0, 0, 0);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(101, 34);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(40, 0);
            btnApply.Margin = new Padding(8, 0, 0, 0);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(101, 34);
            btnApply.TabIndex = 0;
            btnApply.Text = "Select";
            btnApply.UseVisualStyleBackColor = true;
            btnApply.Click += btnApply_Click;
            // 
            // frmObjectRecordSelector
            // 
            AcceptButton = btnApply;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(920, 620);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(820, 520);
            Name = "frmObjectRecordSelector";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select From Records";
            mainLayout.ResumeLayout(false);
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            tabRecords.ResumeLayout(false);
            tabOriginalParcels.ResumeLayout(false);
            originalLayout.ResumeLayout(false);
            searchPanel.ResumeLayout(false);
            searchPanel.PerformLayout();
            selectionToolsPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOriginalParcels).EndInit();
            footerPanel.ResumeLayout(false);
            footerPanel.PerformLayout();
            actionPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
