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
        private Label lblMapSheetFilter;
        private ComboBox cboMapSheetFilter;
        private Label lblPlotNumberSearch;
        private TextBox txtPlotNumberSearch;
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

        // ── Replotted Parcels tab ───────────────────
        private TableLayoutPanel replottedLayout;
        private Panel searchPanelReplotted;
        private Label lblSearchReplotted;
        private TextBox txtSearchReplotted;
        private Button btnClearSearchReplotted;
        private FlowLayoutPanel selectionToolsPanelReplotted;
        private Button btnSelectAllReplotted;
        private Button btnSelectNoneReplotted;
        private Button btnDeselectReplotted;
        private Button btnInvertReplotted;
        private DataGridView dgvReplottedParcels;
        private DataGridViewCheckBoxColumn colReplottedSelected;
        private DataGridViewTextBoxColumn colReplottedPlotNo;
        private DataGridViewTextBoxColumn colReplottedBlock;
        private DataGridViewTextBoxColumn colReplottedOwner;
        private DataGridViewTextBoxColumn colReplottedArea;
        private DataGridViewTextBoxColumn colReplottedLayer;
        private DataGridViewTextBoxColumn colReplottedStatus;

        // ── Blocks tab ──────────────────────────────
        private TableLayoutPanel blocksLayout;
        private Panel searchPanelBlocks;
        private Label lblSearchBlocks;
        private TextBox txtSearchBlocks;
        private Button btnClearSearchBlocks;
        private FlowLayoutPanel selectionToolsPanelBlocks;
        private Button btnSelectAllBlocks;
        private Button btnSelectNoneBlocks;
        private Button btnDeselectBlocks;
        private Button btnInvertBlocks;
        private DataGridView dgvBlocks;
        private DataGridViewCheckBoxColumn colBlockSelected;
        private DataGridViewTextBoxColumn colBlockName;
        private DataGridViewTextBoxColumn colBlockCode;
        private DataGridViewTextBoxColumn colBlockLandUse;
        private DataGridViewTextBoxColumn colBlockArea;
        private DataGridViewTextBoxColumn colBlockLayer;
        private DataGridViewTextBoxColumn colBlockStatus;

        // ── Roads tab ───────────────────────────────
        private TableLayoutPanel roadsLayout;
        private Panel searchPanelRoads;
        private Label lblSearchRoads;
        private TextBox txtSearchRoads;
        private Button btnClearSearchRoads;
        private FlowLayoutPanel selectionToolsPanelRoads;
        private Button btnSelectAllRoads;
        private Button btnSelectNoneRoads;
        private Button btnDeselectRoads;
        private Button btnInvertRoads;
        private DataGridView dgvRoads;
        private DataGridViewCheckBoxColumn colRoadSelected;
        private DataGridViewTextBoxColumn colRoadName;
        private DataGridViewTextBoxColumn colRoadCode;
        private DataGridViewTextBoxColumn colRoadType;
        private DataGridViewTextBoxColumn colRoadWidth;
        private DataGridViewTextBoxColumn colRoadLayer;
        private DataGridViewTextBoxColumn colRoadStatus;

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
            lblMapSheetFilter = new Label();
            cboMapSheetFilter = new ComboBox();
            lblPlotNumberSearch = new Label();
            txtPlotNumberSearch = new TextBox();
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
            replottedLayout = new TableLayoutPanel();
            searchPanelReplotted = new Panel();
            lblSearchReplotted = new Label();
            txtSearchReplotted = new TextBox();
            btnClearSearchReplotted = new Button();
            selectionToolsPanelReplotted = new FlowLayoutPanel();
            btnSelectAllReplotted = new Button();
            btnSelectNoneReplotted = new Button();
            btnDeselectReplotted = new Button();
            btnInvertReplotted = new Button();
            dgvReplottedParcels = new DataGridView();
            colReplottedSelected = new DataGridViewCheckBoxColumn();
            colReplottedPlotNo = new DataGridViewTextBoxColumn();
            colReplottedBlock = new DataGridViewTextBoxColumn();
            colReplottedOwner = new DataGridViewTextBoxColumn();
            colReplottedArea = new DataGridViewTextBoxColumn();
            colReplottedLayer = new DataGridViewTextBoxColumn();
            colReplottedStatus = new DataGridViewTextBoxColumn();
            blocksLayout = new TableLayoutPanel();
            searchPanelBlocks = new Panel();
            lblSearchBlocks = new Label();
            txtSearchBlocks = new TextBox();
            btnClearSearchBlocks = new Button();
            selectionToolsPanelBlocks = new FlowLayoutPanel();
            btnSelectAllBlocks = new Button();
            btnSelectNoneBlocks = new Button();
            btnDeselectBlocks = new Button();
            btnInvertBlocks = new Button();
            dgvBlocks = new DataGridView();
            colBlockSelected = new DataGridViewCheckBoxColumn();
            colBlockName = new DataGridViewTextBoxColumn();
            colBlockCode = new DataGridViewTextBoxColumn();
            colBlockLandUse = new DataGridViewTextBoxColumn();
            colBlockArea = new DataGridViewTextBoxColumn();
            colBlockLayer = new DataGridViewTextBoxColumn();
            colBlockStatus = new DataGridViewTextBoxColumn();
            roadsLayout = new TableLayoutPanel();
            searchPanelRoads = new Panel();
            lblSearchRoads = new Label();
            txtSearchRoads = new TextBox();
            btnClearSearchRoads = new Button();
            selectionToolsPanelRoads = new FlowLayoutPanel();
            btnSelectAllRoads = new Button();
            btnSelectNoneRoads = new Button();
            btnDeselectRoads = new Button();
            btnInvertRoads = new Button();
            dgvRoads = new DataGridView();
            colRoadSelected = new DataGridViewCheckBoxColumn();
            colRoadName = new DataGridViewTextBoxColumn();
            colRoadCode = new DataGridViewTextBoxColumn();
            colRoadType = new DataGridViewTextBoxColumn();
            colRoadWidth = new DataGridViewTextBoxColumn();
            colRoadLayer = new DataGridViewTextBoxColumn();
            colRoadStatus = new DataGridViewTextBoxColumn();
            mainLayout.SuspendLayout();
            headerPanel.SuspendLayout();
            tabRecords.SuspendLayout();
            tabOriginalParcels.SuspendLayout();
            originalLayout.SuspendLayout();
            searchPanel.SuspendLayout();
            selectionToolsPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOriginalParcels).BeginInit();
            tabReplottedParcels.SuspendLayout();
            replottedLayout.SuspendLayout();
            searchPanelReplotted.SuspendLayout();
            selectionToolsPanelReplotted.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvReplottedParcels).BeginInit();
            tabBlocks.SuspendLayout();
            blocksLayout.SuspendLayout();
            searchPanelBlocks.SuspendLayout();
            selectionToolsPanelBlocks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBlocks).BeginInit();
            tabRoads.SuspendLayout();
            roadsLayout.SuspendLayout();
            searchPanelRoads.SuspendLayout();
            selectionToolsPanelRoads.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvRoads).BeginInit();
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
            originalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            originalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            originalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            originalLayout.Size = new Size(862, 419);
            originalLayout.TabIndex = 0;
            // 
            // searchPanel
            // 
            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(lblMapSheetFilter);
            searchPanel.Controls.Add(cboMapSheetFilter);
            searchPanel.Controls.Add(lblPlotNumberSearch);
            searchPanel.Controls.Add(txtPlotNumberSearch);
            searchPanel.Controls.Add(btnClearSearch);
            searchPanel.Dock = DockStyle.Fill;
            searchPanel.Location = new Point(0, 0);
            searchPanel.Margin = new Padding(0);
            searchPanel.Name = "searchPanel";
            searchPanel.Size = new Size(862, 86);
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
            // lblMapSheetFilter
            // 
            lblMapSheetFilter.AutoSize = true;
            lblMapSheetFilter.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblMapSheetFilter.Location = new Point(0, 51);
            lblMapSheetFilter.Name = "lblMapSheetFilter";
            lblMapSheetFilter.Size = new Size(79, 20);
            lblMapSheetFilter.TabIndex = 3;
            lblMapSheetFilter.Text = "Map Sheet";
            // 
            // cboMapSheetFilter
            // 
            cboMapSheetFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMapSheetFilter.FormattingEnabled = true;
            cboMapSheetFilter.Location = new Point(92, 47);
            cboMapSheetFilter.Name = "cboMapSheetFilter";
            cboMapSheetFilter.Size = new Size(240, 28);
            cboMapSheetFilter.TabIndex = 4;
            cboMapSheetFilter.SelectedIndexChanged += cboMapSheetFilter_SelectedIndexChanged;
            // 
            // lblPlotNumberSearch
            // 
            lblPlotNumberSearch.AutoSize = true;
            lblPlotNumberSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPlotNumberSearch.Location = new Point(352, 51);
            lblPlotNumberSearch.Name = "lblPlotNumberSearch";
            lblPlotNumberSearch.Size = new Size(62, 20);
            lblPlotNumberSearch.TabIndex = 5;
            lblPlotNumberSearch.Text = "Plot No.";
            // 
            // txtPlotNumberSearch
            // 
            txtPlotNumberSearch.Location = new Point(424, 47);
            txtPlotNumberSearch.Name = "txtPlotNumberSearch";
            txtPlotNumberSearch.PlaceholderText = "Plot number";
            txtPlotNumberSearch.Size = new Size(180, 27);
            txtPlotNumberSearch.TabIndex = 6;
            txtPlotNumberSearch.TextChanged += txtPlotNumberSearch_TextChanged;
            txtPlotNumberSearch.KeyPress += txtPlotNumberSearch_KeyPress;
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
            selectionToolsPanel.Location = new Point(0, 86);
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
            dgvOriginalParcels.Location = new Point(0, 130);
            dgvOriginalParcels.Margin = new Padding(0);
            dgvOriginalParcels.MultiSelect = true;
            dgvOriginalParcels.Name = "dgvOriginalParcels";
            dgvOriginalParcels.RowHeadersVisible = false;
            dgvOriginalParcels.RowHeadersWidth = 51;
            dgvOriginalParcels.RowTemplate.Height = 30;
            dgvOriginalParcels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOriginalParcels.Size = new Size(862, 289);
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
            tabReplottedParcels.Controls.Add(replottedLayout);
            tabReplottedParcels.Location = new Point(4, 29);
            tabReplottedParcels.Name = "tabReplottedParcels";
            tabReplottedParcels.Padding = new Padding(10);
            tabReplottedParcels.Size = new Size(882, 439);
            tabReplottedParcels.TabIndex = 1;
            tabReplottedParcels.Text = "Replotted Parcel Records";
            tabReplottedParcels.UseVisualStyleBackColor = true;
            //
            // replottedLayout
            //
            replottedLayout.ColumnCount = 1;
            replottedLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            replottedLayout.Controls.Add(searchPanelReplotted, 0, 0);
            replottedLayout.Controls.Add(selectionToolsPanelReplotted, 0, 1);
            replottedLayout.Controls.Add(dgvReplottedParcels, 0, 2);
            replottedLayout.Dock = DockStyle.Fill;
            replottedLayout.Location = new Point(10, 10);
            replottedLayout.Name = "replottedLayout";
            replottedLayout.RowCount = 3;
            replottedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            replottedLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            replottedLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            replottedLayout.Size = new Size(862, 419);
            replottedLayout.TabIndex = 0;
            //
            // searchPanelReplotted
            //
            searchPanelReplotted.Controls.Add(lblSearchReplotted);
            searchPanelReplotted.Controls.Add(txtSearchReplotted);
            searchPanelReplotted.Controls.Add(btnClearSearchReplotted);
            searchPanelReplotted.Dock = DockStyle.Fill;
            searchPanelReplotted.Location = new Point(0, 0);
            searchPanelReplotted.Margin = new Padding(0);
            searchPanelReplotted.Name = "searchPanelReplotted";
            searchPanelReplotted.Size = new Size(862, 44);
            searchPanelReplotted.TabIndex = 0;
            //
            // lblSearchReplotted
            //
            lblSearchReplotted.AutoSize = true;
            lblSearchReplotted.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSearchReplotted.Location = new Point(0, 11);
            lblSearchReplotted.Name = "lblSearchReplotted";
            lblSearchReplotted.Size = new Size(56, 20);
            lblSearchReplotted.TabIndex = 0;
            lblSearchReplotted.Text = "Search";
            //
            // txtSearchReplotted
            //
            txtSearchReplotted.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearchReplotted.Location = new Point(70, 7);
            txtSearchReplotted.Name = "txtSearchReplotted";
            txtSearchReplotted.PlaceholderText = "Plot no, block, owner, layer...";
            txtSearchReplotted.Size = new Size(680, 27);
            txtSearchReplotted.TabIndex = 1;
            //
            // btnClearSearchReplotted
            //
            btnClearSearchReplotted.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearSearchReplotted.Location = new Point(762, 5);
            btnClearSearchReplotted.Name = "btnClearSearchReplotted";
            btnClearSearchReplotted.Size = new Size(96, 31);
            btnClearSearchReplotted.TabIndex = 2;
            btnClearSearchReplotted.Text = "Clear";
            btnClearSearchReplotted.UseVisualStyleBackColor = true;
            //
            // selectionToolsPanelReplotted
            //
            selectionToolsPanelReplotted.Controls.Add(btnSelectAllReplotted);
            selectionToolsPanelReplotted.Controls.Add(btnSelectNoneReplotted);
            selectionToolsPanelReplotted.Controls.Add(btnDeselectReplotted);
            selectionToolsPanelReplotted.Controls.Add(btnInvertReplotted);
            selectionToolsPanelReplotted.Dock = DockStyle.Fill;
            selectionToolsPanelReplotted.Location = new Point(0, 44);
            selectionToolsPanelReplotted.Margin = new Padding(0);
            selectionToolsPanelReplotted.Name = "selectionToolsPanelReplotted";
            selectionToolsPanelReplotted.Padding = new Padding(0, 5, 0, 0);
            selectionToolsPanelReplotted.Size = new Size(862, 44);
            selectionToolsPanelReplotted.TabIndex = 1;
            //
            // btnSelectAllReplotted
            //
            btnSelectAllReplotted.Margin = new Padding(0, 0, 8, 0);
            btnSelectAllReplotted.Name = "btnSelectAllReplotted";
            btnSelectAllReplotted.Size = new Size(96, 31);
            btnSelectAllReplotted.TabIndex = 0;
            btnSelectAllReplotted.Text = "Select All";
            btnSelectAllReplotted.UseVisualStyleBackColor = true;
            //
            // btnSelectNoneReplotted
            //
            btnSelectNoneReplotted.Margin = new Padding(0, 0, 8, 0);
            btnSelectNoneReplotted.Name = "btnSelectNoneReplotted";
            btnSelectNoneReplotted.Size = new Size(102, 31);
            btnSelectNoneReplotted.TabIndex = 1;
            btnSelectNoneReplotted.Text = "Select None";
            btnSelectNoneReplotted.UseVisualStyleBackColor = true;
            //
            // btnDeselectReplotted
            //
            btnDeselectReplotted.Margin = new Padding(0, 0, 8, 0);
            btnDeselectReplotted.Name = "btnDeselectReplotted";
            btnDeselectReplotted.Size = new Size(124, 31);
            btnDeselectReplotted.TabIndex = 2;
            btnDeselectReplotted.Text = "Deselect";
            btnDeselectReplotted.UseVisualStyleBackColor = true;
            //
            // btnInvertReplotted
            //
            btnInvertReplotted.Margin = new Padding(0, 0, 8, 0);
            btnInvertReplotted.Name = "btnInvertReplotted";
            btnInvertReplotted.Size = new Size(132, 31);
            btnInvertReplotted.TabIndex = 3;
            btnInvertReplotted.Text = "Inverse Selection";
            btnInvertReplotted.UseVisualStyleBackColor = true;
            //
            // dgvReplottedParcels
            //
            dgvReplottedParcels.AllowUserToAddRows = false;
            dgvReplottedParcels.AllowUserToDeleteRows = false;
            dgvReplottedParcels.AllowUserToResizeRows = false;
            dgvReplottedParcels.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvReplottedParcels.Columns.AddRange(new DataGridViewColumn[] { colReplottedSelected, colReplottedPlotNo, colReplottedBlock, colReplottedOwner, colReplottedArea, colReplottedLayer, colReplottedStatus });
            dgvReplottedParcels.Dock = DockStyle.Fill;
            dgvReplottedParcels.Location = new Point(0, 88);
            dgvReplottedParcels.Margin = new Padding(0);
            dgvReplottedParcels.MultiSelect = true;
            dgvReplottedParcels.Name = "dgvReplottedParcels";
            dgvReplottedParcels.RowHeadersVisible = false;
            dgvReplottedParcels.RowTemplate.Height = 30;
            dgvReplottedParcels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReplottedParcels.Size = new Size(862, 331);
            dgvReplottedParcels.TabIndex = 2;
            //
            // colReplottedSelected
            //
            colReplottedSelected.HeaderText = "";
            colReplottedSelected.Name = "colReplottedSelected";
            colReplottedSelected.Width = 42;
            //
            // colReplottedPlotNo
            //
            colReplottedPlotNo.HeaderText = "Plot No.";
            colReplottedPlotNo.Name = "colReplottedPlotNo";
            colReplottedPlotNo.ReadOnly = true;
            colReplottedPlotNo.Width = 110;
            //
            // colReplottedBlock
            //
            colReplottedBlock.HeaderText = "Block";
            colReplottedBlock.Name = "colReplottedBlock";
            colReplottedBlock.ReadOnly = true;
            colReplottedBlock.Width = 120;
            //
            // colReplottedOwner
            //
            colReplottedOwner.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colReplottedOwner.HeaderText = "Owner";
            colReplottedOwner.MinimumWidth = 150;
            colReplottedOwner.Name = "colReplottedOwner";
            colReplottedOwner.ReadOnly = true;
            //
            // colReplottedArea
            //
            colReplottedArea.HeaderText = "Area";
            colReplottedArea.Name = "colReplottedArea";
            colReplottedArea.ReadOnly = true;
            colReplottedArea.Width = 120;
            //
            // colReplottedLayer
            //
            colReplottedLayer.HeaderText = "Layer";
            colReplottedLayer.Name = "colReplottedLayer";
            colReplottedLayer.ReadOnly = true;
            colReplottedLayer.Width = 140;
            //
            // colReplottedStatus
            //
            colReplottedStatus.HeaderText = "Status";
            colReplottedStatus.Name = "colReplottedStatus";
            colReplottedStatus.ReadOnly = true;
            colReplottedStatus.Width = 110;
            // 
            // tabBlocks
            // 
            tabBlocks.Controls.Add(blocksLayout);
            tabBlocks.Location = new Point(4, 29);
            tabBlocks.Name = "tabBlocks";
            tabBlocks.Padding = new Padding(10);
            tabBlocks.Size = new Size(882, 439);
            tabBlocks.TabIndex = 2;
            tabBlocks.Text = "Block Data";
            tabBlocks.UseVisualStyleBackColor = true;
            //
            // blocksLayout
            //
            blocksLayout.ColumnCount = 1;
            blocksLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            blocksLayout.Controls.Add(searchPanelBlocks, 0, 0);
            blocksLayout.Controls.Add(selectionToolsPanelBlocks, 0, 1);
            blocksLayout.Controls.Add(dgvBlocks, 0, 2);
            blocksLayout.Dock = DockStyle.Fill;
            blocksLayout.Location = new Point(10, 10);
            blocksLayout.Name = "blocksLayout";
            blocksLayout.RowCount = 3;
            blocksLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            blocksLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            blocksLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            blocksLayout.Size = new Size(862, 419);
            blocksLayout.TabIndex = 0;
            //
            // searchPanelBlocks
            //
            searchPanelBlocks.Controls.Add(lblSearchBlocks);
            searchPanelBlocks.Controls.Add(txtSearchBlocks);
            searchPanelBlocks.Controls.Add(btnClearSearchBlocks);
            searchPanelBlocks.Dock = DockStyle.Fill;
            searchPanelBlocks.Location = new Point(0, 0);
            searchPanelBlocks.Margin = new Padding(0);
            searchPanelBlocks.Name = "searchPanelBlocks";
            searchPanelBlocks.Size = new Size(862, 44);
            searchPanelBlocks.TabIndex = 0;
            //
            // lblSearchBlocks
            //
            lblSearchBlocks.AutoSize = true;
            lblSearchBlocks.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSearchBlocks.Location = new Point(0, 11);
            lblSearchBlocks.Name = "lblSearchBlocks";
            lblSearchBlocks.Size = new Size(56, 20);
            lblSearchBlocks.TabIndex = 0;
            lblSearchBlocks.Text = "Search";
            //
            // txtSearchBlocks
            //
            txtSearchBlocks.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearchBlocks.Location = new Point(70, 7);
            txtSearchBlocks.Name = "txtSearchBlocks";
            txtSearchBlocks.PlaceholderText = "Block name, code, land use, layer...";
            txtSearchBlocks.Size = new Size(680, 27);
            txtSearchBlocks.TabIndex = 1;
            //
            // btnClearSearchBlocks
            //
            btnClearSearchBlocks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearSearchBlocks.Location = new Point(762, 5);
            btnClearSearchBlocks.Name = "btnClearSearchBlocks";
            btnClearSearchBlocks.Size = new Size(96, 31);
            btnClearSearchBlocks.TabIndex = 2;
            btnClearSearchBlocks.Text = "Clear";
            btnClearSearchBlocks.UseVisualStyleBackColor = true;
            //
            // selectionToolsPanelBlocks
            //
            selectionToolsPanelBlocks.Controls.Add(btnSelectAllBlocks);
            selectionToolsPanelBlocks.Controls.Add(btnSelectNoneBlocks);
            selectionToolsPanelBlocks.Controls.Add(btnDeselectBlocks);
            selectionToolsPanelBlocks.Controls.Add(btnInvertBlocks);
            selectionToolsPanelBlocks.Dock = DockStyle.Fill;
            selectionToolsPanelBlocks.Location = new Point(0, 44);
            selectionToolsPanelBlocks.Margin = new Padding(0);
            selectionToolsPanelBlocks.Name = "selectionToolsPanelBlocks";
            selectionToolsPanelBlocks.Padding = new Padding(0, 5, 0, 0);
            selectionToolsPanelBlocks.Size = new Size(862, 44);
            selectionToolsPanelBlocks.TabIndex = 1;
            //
            // btnSelectAllBlocks
            //
            btnSelectAllBlocks.Margin = new Padding(0, 0, 8, 0);
            btnSelectAllBlocks.Name = "btnSelectAllBlocks";
            btnSelectAllBlocks.Size = new Size(96, 31);
            btnSelectAllBlocks.TabIndex = 0;
            btnSelectAllBlocks.Text = "Select All";
            btnSelectAllBlocks.UseVisualStyleBackColor = true;
            //
            // btnSelectNoneBlocks
            //
            btnSelectNoneBlocks.Margin = new Padding(0, 0, 8, 0);
            btnSelectNoneBlocks.Name = "btnSelectNoneBlocks";
            btnSelectNoneBlocks.Size = new Size(102, 31);
            btnSelectNoneBlocks.TabIndex = 1;
            btnSelectNoneBlocks.Text = "Select None";
            btnSelectNoneBlocks.UseVisualStyleBackColor = true;
            //
            // btnDeselectBlocks
            //
            btnDeselectBlocks.Margin = new Padding(0, 0, 8, 0);
            btnDeselectBlocks.Name = "btnDeselectBlocks";
            btnDeselectBlocks.Size = new Size(124, 31);
            btnDeselectBlocks.TabIndex = 2;
            btnDeselectBlocks.Text = "Deselect";
            btnDeselectBlocks.UseVisualStyleBackColor = true;
            //
            // btnInvertBlocks
            //
            btnInvertBlocks.Margin = new Padding(0, 0, 8, 0);
            btnInvertBlocks.Name = "btnInvertBlocks";
            btnInvertBlocks.Size = new Size(132, 31);
            btnInvertBlocks.TabIndex = 3;
            btnInvertBlocks.Text = "Inverse Selection";
            btnInvertBlocks.UseVisualStyleBackColor = true;
            //
            // dgvBlocks
            //
            dgvBlocks.AllowUserToAddRows = false;
            dgvBlocks.AllowUserToDeleteRows = false;
            dgvBlocks.AllowUserToResizeRows = false;
            dgvBlocks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBlocks.Columns.AddRange(new DataGridViewColumn[] { colBlockSelected, colBlockName, colBlockCode, colBlockLandUse, colBlockArea, colBlockLayer, colBlockStatus });
            dgvBlocks.Dock = DockStyle.Fill;
            dgvBlocks.Location = new Point(0, 88);
            dgvBlocks.Margin = new Padding(0);
            dgvBlocks.MultiSelect = true;
            dgvBlocks.Name = "dgvBlocks";
            dgvBlocks.RowHeadersVisible = false;
            dgvBlocks.RowTemplate.Height = 30;
            dgvBlocks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBlocks.Size = new Size(862, 331);
            dgvBlocks.TabIndex = 2;
            //
            // colBlockSelected
            //
            colBlockSelected.HeaderText = "";
            colBlockSelected.Name = "colBlockSelected";
            colBlockSelected.Width = 42;
            //
            // colBlockName
            //
            colBlockName.HeaderText = "Block Name";
            colBlockName.Name = "colBlockName";
            colBlockName.ReadOnly = true;
            colBlockName.Width = 150;
            //
            // colBlockCode
            //
            colBlockCode.HeaderText = "Block Code";
            colBlockCode.Name = "colBlockCode";
            colBlockCode.ReadOnly = true;
            colBlockCode.Width = 110;
            //
            // colBlockLandUse
            //
            colBlockLandUse.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colBlockLandUse.HeaderText = "Land Use";
            colBlockLandUse.MinimumWidth = 120;
            colBlockLandUse.Name = "colBlockLandUse";
            colBlockLandUse.ReadOnly = true;
            //
            // colBlockArea
            //
            colBlockArea.HeaderText = "Area";
            colBlockArea.Name = "colBlockArea";
            colBlockArea.ReadOnly = true;
            colBlockArea.Width = 120;
            //
            // colBlockLayer
            //
            colBlockLayer.HeaderText = "Layer";
            colBlockLayer.Name = "colBlockLayer";
            colBlockLayer.ReadOnly = true;
            colBlockLayer.Width = 140;
            //
            // colBlockStatus
            //
            colBlockStatus.HeaderText = "Status";
            colBlockStatus.Name = "colBlockStatus";
            colBlockStatus.ReadOnly = true;
            colBlockStatus.Width = 110;
            // 
            // tabRoads
            // 
            tabRoads.Controls.Add(roadsLayout);
            tabRoads.Location = new Point(4, 29);
            tabRoads.Name = "tabRoads";
            tabRoads.Padding = new Padding(10);
            tabRoads.Size = new Size(882, 439);
            tabRoads.TabIndex = 3;
            tabRoads.Text = "Road Data";
            tabRoads.UseVisualStyleBackColor = true;
            //
            // roadsLayout
            //
            roadsLayout.ColumnCount = 1;
            roadsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            roadsLayout.Controls.Add(searchPanelRoads, 0, 0);
            roadsLayout.Controls.Add(selectionToolsPanelRoads, 0, 1);
            roadsLayout.Controls.Add(dgvRoads, 0, 2);
            roadsLayout.Dock = DockStyle.Fill;
            roadsLayout.Location = new Point(10, 10);
            roadsLayout.Name = "roadsLayout";
            roadsLayout.RowCount = 3;
            roadsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            roadsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            roadsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            roadsLayout.Size = new Size(862, 419);
            roadsLayout.TabIndex = 0;
            //
            // searchPanelRoads
            //
            searchPanelRoads.Controls.Add(lblSearchRoads);
            searchPanelRoads.Controls.Add(txtSearchRoads);
            searchPanelRoads.Controls.Add(btnClearSearchRoads);
            searchPanelRoads.Dock = DockStyle.Fill;
            searchPanelRoads.Location = new Point(0, 0);
            searchPanelRoads.Margin = new Padding(0);
            searchPanelRoads.Name = "searchPanelRoads";
            searchPanelRoads.Size = new Size(862, 44);
            searchPanelRoads.TabIndex = 0;
            //
            // lblSearchRoads
            //
            lblSearchRoads.AutoSize = true;
            lblSearchRoads.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSearchRoads.Location = new Point(0, 11);
            lblSearchRoads.Name = "lblSearchRoads";
            lblSearchRoads.Size = new Size(56, 20);
            lblSearchRoads.TabIndex = 0;
            lblSearchRoads.Text = "Search";
            //
            // txtSearchRoads
            //
            txtSearchRoads.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSearchRoads.Location = new Point(70, 7);
            txtSearchRoads.Name = "txtSearchRoads";
            txtSearchRoads.PlaceholderText = "Road name, code, type, layer...";
            txtSearchRoads.Size = new Size(680, 27);
            txtSearchRoads.TabIndex = 1;
            //
            // btnClearSearchRoads
            //
            btnClearSearchRoads.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClearSearchRoads.Location = new Point(762, 5);
            btnClearSearchRoads.Name = "btnClearSearchRoads";
            btnClearSearchRoads.Size = new Size(96, 31);
            btnClearSearchRoads.TabIndex = 2;
            btnClearSearchRoads.Text = "Clear";
            btnClearSearchRoads.UseVisualStyleBackColor = true;
            //
            // selectionToolsPanelRoads
            //
            selectionToolsPanelRoads.Controls.Add(btnSelectAllRoads);
            selectionToolsPanelRoads.Controls.Add(btnSelectNoneRoads);
            selectionToolsPanelRoads.Controls.Add(btnDeselectRoads);
            selectionToolsPanelRoads.Controls.Add(btnInvertRoads);
            selectionToolsPanelRoads.Dock = DockStyle.Fill;
            selectionToolsPanelRoads.Location = new Point(0, 44);
            selectionToolsPanelRoads.Margin = new Padding(0);
            selectionToolsPanelRoads.Name = "selectionToolsPanelRoads";
            selectionToolsPanelRoads.Padding = new Padding(0, 5, 0, 0);
            selectionToolsPanelRoads.Size = new Size(862, 44);
            selectionToolsPanelRoads.TabIndex = 1;
            //
            // btnSelectAllRoads
            //
            btnSelectAllRoads.Margin = new Padding(0, 0, 8, 0);
            btnSelectAllRoads.Name = "btnSelectAllRoads";
            btnSelectAllRoads.Size = new Size(96, 31);
            btnSelectAllRoads.TabIndex = 0;
            btnSelectAllRoads.Text = "Select All";
            btnSelectAllRoads.UseVisualStyleBackColor = true;
            //
            // btnSelectNoneRoads
            //
            btnSelectNoneRoads.Margin = new Padding(0, 0, 8, 0);
            btnSelectNoneRoads.Name = "btnSelectNoneRoads";
            btnSelectNoneRoads.Size = new Size(102, 31);
            btnSelectNoneRoads.TabIndex = 1;
            btnSelectNoneRoads.Text = "Select None";
            btnSelectNoneRoads.UseVisualStyleBackColor = true;
            //
            // btnDeselectRoads
            //
            btnDeselectRoads.Margin = new Padding(0, 0, 8, 0);
            btnDeselectRoads.Name = "btnDeselectRoads";
            btnDeselectRoads.Size = new Size(124, 31);
            btnDeselectRoads.TabIndex = 2;
            btnDeselectRoads.Text = "Deselect";
            btnDeselectRoads.UseVisualStyleBackColor = true;
            //
            // btnInvertRoads
            //
            btnInvertRoads.Margin = new Padding(0, 0, 8, 0);
            btnInvertRoads.Name = "btnInvertRoads";
            btnInvertRoads.Size = new Size(132, 31);
            btnInvertRoads.TabIndex = 3;
            btnInvertRoads.Text = "Inverse Selection";
            btnInvertRoads.UseVisualStyleBackColor = true;
            //
            // dgvRoads
            //
            dgvRoads.AllowUserToAddRows = false;
            dgvRoads.AllowUserToDeleteRows = false;
            dgvRoads.AllowUserToResizeRows = false;
            dgvRoads.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvRoads.Columns.AddRange(new DataGridViewColumn[] { colRoadSelected, colRoadName, colRoadCode, colRoadType, colRoadWidth, colRoadLayer, colRoadStatus });
            dgvRoads.Dock = DockStyle.Fill;
            dgvRoads.Location = new Point(0, 88);
            dgvRoads.Margin = new Padding(0);
            dgvRoads.MultiSelect = true;
            dgvRoads.Name = "dgvRoads";
            dgvRoads.RowHeadersVisible = false;
            dgvRoads.RowTemplate.Height = 30;
            dgvRoads.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRoads.Size = new Size(862, 331);
            dgvRoads.TabIndex = 2;
            //
            // colRoadSelected
            //
            colRoadSelected.HeaderText = "";
            colRoadSelected.Name = "colRoadSelected";
            colRoadSelected.Width = 42;
            //
            // colRoadName
            //
            colRoadName.HeaderText = "Road Name";
            colRoadName.Name = "colRoadName";
            colRoadName.ReadOnly = true;
            colRoadName.Width = 150;
            //
            // colRoadCode
            //
            colRoadCode.HeaderText = "Road Code";
            colRoadCode.Name = "colRoadCode";
            colRoadCode.ReadOnly = true;
            colRoadCode.Width = 110;
            //
            // colRoadType
            //
            colRoadType.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colRoadType.HeaderText = "Type";
            colRoadType.MinimumWidth = 120;
            colRoadType.Name = "colRoadType";
            colRoadType.ReadOnly = true;
            //
            // colRoadWidth
            //
            colRoadWidth.HeaderText = "Width";
            colRoadWidth.Name = "colRoadWidth";
            colRoadWidth.ReadOnly = true;
            colRoadWidth.Width = 100;
            //
            // colRoadLayer
            //
            colRoadLayer.HeaderText = "Layer";
            colRoadLayer.Name = "colRoadLayer";
            colRoadLayer.ReadOnly = true;
            colRoadLayer.Width = 140;
            //
            // colRoadStatus
            //
            colRoadStatus.HeaderText = "Status";
            colRoadStatus.Name = "colRoadStatus";
            colRoadStatus.ReadOnly = true;
            colRoadStatus.Width = 110;
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
            tabReplottedParcels.ResumeLayout(false);
            replottedLayout.ResumeLayout(false);
            searchPanelReplotted.ResumeLayout(false);
            searchPanelReplotted.PerformLayout();
            selectionToolsPanelReplotted.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvReplottedParcels).EndInit();
            tabBlocks.ResumeLayout(false);
            blocksLayout.ResumeLayout(false);
            searchPanelBlocks.ResumeLayout(false);
            searchPanelBlocks.PerformLayout();
            selectionToolsPanelBlocks.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvBlocks).EndInit();
            tabRoads.ResumeLayout(false);
            roadsLayout.ResumeLayout(false);
            searchPanelRoads.ResumeLayout(false);
            searchPanelRoads.PerformLayout();
            selectionToolsPanelRoads.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvRoads).EndInit();
            footerPanel.ResumeLayout(false);
            footerPanel.PerformLayout();
            actionPanel.ResumeLayout(false);
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
            ResumeLayout(false);
        }
    }
}
