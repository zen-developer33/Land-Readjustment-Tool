namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmLayerManager
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle8 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            pnlHeader = new Panel();
            lblSubtitle = new Label();
            lblTitle = new Label();
            picLogo = new PictureBox();
            pnlToolbar = new Panel();
            btnNewLayer = new Button();
            btnDeleteLayer = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            sep1 = new Label();
            btnShowAll = new Button();
            btnHideAll = new Button();
            btnLockAll = new Button();
            sep2 = new Label();
            lblSearch = new Label();
            txtSearch = new TextBox();
            dgvLayers = new DataGridView();
            colVisible = new DataGridViewCheckBoxColumn();
            colLocked = new DataGridViewCheckBoxColumn();
            colPrintable = new DataGridViewCheckBoxColumn();
            colColor = new DataGridViewTextBoxColumn();
            colName = new DataGridViewTextBoxColumn();
            colLayerType = new DataGridViewComboBoxColumn();
            colLineStyle = new DataGridViewComboBoxColumn();
            colLineWeight = new DataGridViewComboBoxColumn();
            colDisplayOrder = new DataGridViewTextBoxColumn();
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
            trkTransparency = new TrackBar();
            lblTranspValue = new Label();
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
            pnlBottom = new Panel();
            lblLayerCount = new Label();
            btnApply = new Button();
            btnOK = new Button();
            btnCancel = new Button();
            toolTip1 = new ToolTip(components);
            colorDialog1 = new ColorDialog();
            fontDialog1 = new FontDialog();
            splitMain = new SplitContainer();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayers).BeginInit();
            grpProperties.SuspendLayout();
            tabProperties.SuspendLayout();
            tabGeneral.SuspendLayout();
            tabFill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trkTransparency).BeginInit();
            tabLabel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numFontSize).BeginInit();
            pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(26, 82, 118);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(3, 4, 3, 4);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Padding = new Padding(14, 0, 0, 0);
            pnlHeader.Size = new Size(1237, 75);
            pnlHeader.TabIndex = 2;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 8F);
            lblSubtitle.ForeColor = Color.FromArgb(189, 215, 238);
            lblSubtitle.Location = new Point(16, 45);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(214, 19);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "RePlot  ·  Land Readjustment Tool";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(14, 11);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(292, 32);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Layer Property Manager";
            // 
            // picLogo
            // 
            picLogo.Location = new Point(0, 0);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(100, 50);
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.FromArgb(245, 245, 245);
            pnlToolbar.BorderStyle = BorderStyle.FixedSingle;
            pnlToolbar.Controls.Add(btnNewLayer);
            pnlToolbar.Controls.Add(btnDeleteLayer);
            pnlToolbar.Controls.Add(btnMoveUp);
            pnlToolbar.Controls.Add(btnMoveDown);
            pnlToolbar.Controls.Add(sep1);
            pnlToolbar.Controls.Add(btnShowAll);
            pnlToolbar.Controls.Add(btnHideAll);
            pnlToolbar.Controls.Add(btnLockAll);
            pnlToolbar.Controls.Add(sep2);
            pnlToolbar.Controls.Add(lblSearch);
            pnlToolbar.Controls.Add(txtSearch);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 75);
            pnlToolbar.Margin = new Padding(3, 4, 3, 4);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(1237, 50);
            pnlToolbar.TabIndex = 1;
            // 
            // btnNewLayer
            // 
            btnNewLayer.BackColor = Color.White;
            btnNewLayer.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnNewLayer.FlatStyle = FlatStyle.Flat;
            btnNewLayer.Font = new Font("Segoe UI", 8.5F);
            btnNewLayer.Location = new Point(7, 8);
            btnNewLayer.Margin = new Padding(3, 4, 3, 4);
            btnNewLayer.Name = "btnNewLayer";
            btnNewLayer.Size = new Size(101, 35);
            btnNewLayer.TabIndex = 0;
            btnNewLayer.Text = "+ New Layer";
            toolTip1.SetToolTip(btnNewLayer, "Add a new layer");
            btnNewLayer.UseVisualStyleBackColor = false;
            btnNewLayer.Click += btnNewLayer_Click;
            // 
            // btnDeleteLayer
            // 
            btnDeleteLayer.BackColor = Color.White;
            btnDeleteLayer.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnDeleteLayer.FlatStyle = FlatStyle.Flat;
            btnDeleteLayer.Font = new Font("Segoe UI", 8.5F);
            btnDeleteLayer.Location = new Point(114, 8);
            btnDeleteLayer.Margin = new Padding(3, 4, 3, 4);
            btnDeleteLayer.Name = "btnDeleteLayer";
            btnDeleteLayer.Size = new Size(82, 35);
            btnDeleteLayer.TabIndex = 1;
            btnDeleteLayer.Text = "Delete";
            toolTip1.SetToolTip(btnDeleteLayer, "Delete selected layer");
            btnDeleteLayer.UseVisualStyleBackColor = false;
            btnDeleteLayer.Click += btnDeleteLayer_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.BackColor = Color.White;
            btnMoveUp.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnMoveUp.FlatStyle = FlatStyle.Flat;
            btnMoveUp.Font = new Font("Segoe UI", 8.5F);
            btnMoveUp.Location = new Point(203, 8);
            btnMoveUp.Margin = new Padding(3, 4, 3, 4);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(64, 35);
            btnMoveUp.TabIndex = 2;
            btnMoveUp.Text = "▲ Up";
            toolTip1.SetToolTip(btnMoveUp, "Move layer up in draw order");
            btnMoveUp.UseVisualStyleBackColor = false;
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.BackColor = Color.White;
            btnMoveDown.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnMoveDown.FlatStyle = FlatStyle.Flat;
            btnMoveDown.Font = new Font("Segoe UI", 8.5F);
            btnMoveDown.Location = new Point(274, 8);
            btnMoveDown.Margin = new Padding(3, 4, 3, 4);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(82, 35);
            btnMoveDown.TabIndex = 3;
            btnMoveDown.Text = "▼ Down";
            toolTip1.SetToolTip(btnMoveDown, "Move layer down in draw order");
            btnMoveDown.UseVisualStyleBackColor = false;
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // sep1
            // 
            sep1.BackColor = Color.FromArgb(200, 200, 200);
            sep1.Location = new Point(363, 8);
            sep1.Name = "sep1";
            sep1.Size = new Size(1, 35);
            sep1.TabIndex = 4;
            // 
            // btnShowAll
            // 
            btnShowAll.BackColor = Color.White;
            btnShowAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnShowAll.FlatStyle = FlatStyle.Flat;
            btnShowAll.Font = new Font("Segoe UI", 8.5F);
            btnShowAll.Location = new Point(373, 8);
            btnShowAll.Margin = new Padding(3, 4, 3, 4);
            btnShowAll.Name = "btnShowAll";
            btnShowAll.Size = new Size(78, 35);
            btnShowAll.TabIndex = 5;
            btnShowAll.Text = "Show All";
            btnShowAll.UseVisualStyleBackColor = false;
            btnShowAll.Click += btnShowAll_Click;
            // 
            // btnHideAll
            // 
            btnHideAll.BackColor = Color.White;
            btnHideAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnHideAll.FlatStyle = FlatStyle.Flat;
            btnHideAll.Font = new Font("Segoe UI", 8.5F);
            btnHideAll.Location = new Point(457, 8);
            btnHideAll.Margin = new Padding(3, 4, 3, 4);
            btnHideAll.Name = "btnHideAll";
            btnHideAll.Size = new Size(75, 35);
            btnHideAll.TabIndex = 6;
            btnHideAll.Text = "Hide All";
            btnHideAll.UseVisualStyleBackColor = false;
            btnHideAll.Click += btnHideAll_Click;
            // 
            // btnLockAll
            // 
            btnLockAll.BackColor = Color.White;
            btnLockAll.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnLockAll.FlatStyle = FlatStyle.Flat;
            btnLockAll.Font = new Font("Segoe UI", 8.5F);
            btnLockAll.Location = new Point(539, 8);
            btnLockAll.Margin = new Padding(3, 4, 3, 4);
            btnLockAll.Name = "btnLockAll";
            btnLockAll.Size = new Size(75, 35);
            btnLockAll.TabIndex = 7;
            btnLockAll.Text = "Lock All";
            btnLockAll.UseVisualStyleBackColor = false;
            btnLockAll.Click += btnLockAll_Click;
            // 
            // sep2
            // 
            sep2.BackColor = Color.FromArgb(200, 200, 200);
            sep2.Location = new Point(622, 8);
            sep2.Name = "sep2";
            sep2.Size = new Size(1, 35);
            sep2.TabIndex = 8;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI", 8.5F);
            lblSearch.ForeColor = Color.FromArgb(100, 100, 100);
            lblSearch.Location = new Point(631, 16);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(56, 20);
            lblSearch.TabIndex = 9;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.Font = new Font("Segoe UI", 8.5F);
            txtSearch.Location = new Point(688, 11);
            txtSearch.Margin = new Padding(3, 4, 3, 4);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(160, 26);
            txtSearch.TabIndex = 10;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // dgvLayers
            // 
            dgvLayers.AllowUserToAddRows = false;
            dgvLayers.AllowUserToDeleteRows = false;
            dgvLayers.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(248, 250, 253);
            dgvLayers.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            dgvLayers.BackgroundColor = Color.White;
            dgvLayers.BorderStyle = BorderStyle.None;
            dgvLayers.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvLayers.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(26, 82, 118);
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dataGridViewCellStyle6.ForeColor = Color.White;
            dataGridViewCellStyle6.Padding = new Padding(4, 0, 0, 0);
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvLayers.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvLayers.ColumnHeadersHeight = 30;
            dgvLayers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvLayers.Columns.AddRange(new DataGridViewColumn[] { colVisible, colLocked, colPrintable, colColor, colName, colLayerType, colLineStyle, colLineWeight, colDisplayOrder });
            dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = Color.White;
            dataGridViewCellStyle8.Font = new Font("Segoe UI", 8.5F);
            dataGridViewCellStyle8.ForeColor = Color.FromArgb(40, 40, 40);
            dataGridViewCellStyle8.Padding = new Padding(2, 0, 0, 0);
            dataGridViewCellStyle8.SelectionBackColor = Color.FromArgb(210, 228, 244);
            dataGridViewCellStyle8.SelectionForeColor = Color.FromArgb(20, 20, 20);
            dataGridViewCellStyle8.WrapMode = DataGridViewTriState.False;
            dgvLayers.DefaultCellStyle = dataGridViewCellStyle8;
            dgvLayers.Dock = DockStyle.Fill;
            dgvLayers.EnableHeadersVisualStyles = false;
            dgvLayers.GridColor = Color.FromArgb(220, 220, 220);
            dgvLayers.Location = new Point(0, 0);
            dgvLayers.Margin = new Padding(3, 4, 3, 4);
            dgvLayers.MultiSelect = false;
            dgvLayers.Name = "dgvLayers";
            dgvLayers.RowHeadersVisible = false;
            dgvLayers.RowHeadersWidth = 51;
            dgvLayers.RowTemplate.Height = 26;
            dgvLayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLayers.Size = new Size(823, 587);
            dgvLayers.TabIndex = 10;
            dgvLayers.CellClick += dgvLayers_CellClick;
            dgvLayers.CellPainting += dgvLayers_CellPainting;
            dgvLayers.CellValueChanged += dgvLayers_CellValueChanged;
            dgvLayers.CurrentCellDirtyStateChanged += dgvLayers_CurrentCellDirtyStateChanged;
            dgvLayers.SelectionChanged += dgvLayers_SelectionChanged;
            // 
            // colVisible
            // 
            colVisible.DataPropertyName = "IsVisible";
            colVisible.HeaderText = "👁";
            colVisible.MinimumWidth = 34;
            colVisible.Name = "colVisible";
            colVisible.Resizable = DataGridViewTriState.False;
            colVisible.SortMode = DataGridViewColumnSortMode.Automatic;
            colVisible.ToolTipText = "Visible";
            colVisible.Width = 34;
            // 
            // colLocked
            // 
            colLocked.DataPropertyName = "IsLocked";
            colLocked.HeaderText = "🔒";
            colLocked.MinimumWidth = 34;
            colLocked.Name = "colLocked";
            colLocked.Resizable = DataGridViewTriState.False;
            colLocked.ToolTipText = "Locked";
            colLocked.Width = 34;
            // 
            // colPrintable
            // 
            colPrintable.DataPropertyName = "IsPrintable";
            colPrintable.HeaderText = "🖨";
            colPrintable.MinimumWidth = 34;
            colPrintable.Name = "colPrintable";
            colPrintable.Resizable = DataGridViewTriState.False;
            colPrintable.ToolTipText = "Printable";
            colPrintable.Width = 34;
            // 
            // colColor
            // 
            colColor.DataPropertyName = "BorderColor";
            colColor.HeaderText = "Color";
            colColor.MinimumWidth = 6;
            colColor.Name = "colColor";
            colColor.ReadOnly = true;
            colColor.ToolTipText = "Border colour — click to change";
            colColor.Width = 52;
            // 
            // colName
            // 
            colName.DataPropertyName = "Name";
            colName.HeaderText = "Layer Name";
            colName.MinimumWidth = 6;
            colName.Name = "colName";
            colName.Width = 180;
            // 
            // colLayerType
            // 
            colLayerType.DataPropertyName = "LayerType";
            colLayerType.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            colLayerType.FlatStyle = FlatStyle.Flat;
            colLayerType.HeaderText = "Type";
            colLayerType.Items.AddRange(new object[] { "BaselineParcel", "ReplottedParcel", "ProposedRoad", "ExistingRoad", "Block", "ProjectBoundary", "Annotation", "Reference" });
            colLayerType.MinimumWidth = 6;
            colLayerType.Name = "colLayerType";
            colLayerType.Width = 130;
            // 
            // colLineStyle
            // 
            colLineStyle.DataPropertyName = "LineStyle";
            colLineStyle.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            colLineStyle.FlatStyle = FlatStyle.Flat;
            colLineStyle.HeaderText = "Line Style";
            colLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "DashDot" });
            colLineStyle.MinimumWidth = 6;
            colLineStyle.Name = "colLineStyle";
            colLineStyle.Width = 90;
            // 
            // colLineWeight
            // 
            colLineWeight.DataPropertyName = "LineWeight";
            colLineWeight.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
            colLineWeight.FlatStyle = FlatStyle.Flat;
            colLineWeight.HeaderText = "Weight";
            colLineWeight.Items.AddRange(new object[] { "0.25", "0.5", "1.0", "1.5", "2.0", "3.0" });
            colLineWeight.MinimumWidth = 6;
            colLineWeight.Name = "colLineWeight";
            colLineWeight.Width = 72;
            // 
            // colDisplayOrder
            // 
            colDisplayOrder.DataPropertyName = "DisplayOrder";
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDisplayOrder.DefaultCellStyle = dataGridViewCellStyle7;
            colDisplayOrder.HeaderText = "Order";
            colDisplayOrder.MinimumWidth = 6;
            colDisplayOrder.Name = "colDisplayOrder";
            colDisplayOrder.Width = 52;
            // 
            // grpProperties
            // 
            grpProperties.Controls.Add(tabProperties);
            grpProperties.Dock = DockStyle.Fill;
            grpProperties.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            grpProperties.ForeColor = Color.FromArgb(26, 82, 118);
            grpProperties.Location = new Point(0, 0);
            grpProperties.Margin = new Padding(3, 4, 3, 4);
            grpProperties.Name = "grpProperties";
            grpProperties.Padding = new Padding(7, 8, 7, 8);
            grpProperties.Size = new Size(409, 587);
            grpProperties.TabIndex = 0;
            grpProperties.TabStop = false;
            grpProperties.Text = "Layer Properties";
            // 
            // tabProperties
            // 
            tabProperties.Controls.Add(tabGeneral);
            tabProperties.Controls.Add(tabFill);
            tabProperties.Controls.Add(tabLabel);
            tabProperties.Dock = DockStyle.Fill;
            tabProperties.Font = new Font("Segoe UI", 8.5F);
            tabProperties.Location = new Point(7, 27);
            tabProperties.Margin = new Padding(3, 4, 3, 4);
            tabProperties.Name = "tabProperties";
            tabProperties.SelectedIndex = 0;
            tabProperties.Size = new Size(395, 552);
            tabProperties.TabIndex = 0;
            // 
            // tabGeneral
            // 
            tabGeneral.BackColor = Color.White;
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
            tabGeneral.Location = new Point(4, 28);
            tabGeneral.Margin = new Padding(3, 4, 3, 4);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(11, 11, 11, 11);
            tabGeneral.Size = new Size(387, 520);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            // 
            // lblLayerName
            // 
            lblLayerName.AutoSize = true;
            lblLayerName.Font = new Font("Segoe UI", 8.5F);
            lblLayerName.ForeColor = Color.FromArgb(60, 60, 60);
            lblLayerName.Location = new Point(11, 17);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(52, 20);
            lblLayerName.TabIndex = 0;
            lblLayerName.Text = "Name:";
            // 
            // txtLayerName
            // 
            txtLayerName.BorderStyle = BorderStyle.FixedSingle;
            txtLayerName.Font = new Font("Segoe UI", 8.5F);
            txtLayerName.Location = new Point(137, 11);
            txtLayerName.Margin = new Padding(3, 4, 3, 4);
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(183, 26);
            txtLayerName.TabIndex = 1;
            // 
            // lblLayerType
            // 
            lblLayerType.AutoSize = true;
            lblLayerType.Font = new Font("Segoe UI", 8.5F);
            lblLayerType.ForeColor = Color.FromArgb(60, 60, 60);
            lblLayerType.Location = new Point(11, 57);
            lblLayerType.Name = "lblLayerType";
            lblLayerType.Size = new Size(43, 20);
            lblLayerType.TabIndex = 2;
            lblLayerType.Text = "Type:";
            // 
            // cboLayerType
            // 
            cboLayerType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLayerType.FlatStyle = FlatStyle.Flat;
            cboLayerType.Font = new Font("Segoe UI", 8.5F);
            cboLayerType.Items.AddRange(new object[] { "BaselineParcel", "ReplottedParcel", "ProposedRoad", "ExistingRoad", "Block", "ProjectBoundary", "Annotation", "Reference" });
            cboLayerType.Location = new Point(137, 51);
            cboLayerType.Margin = new Padding(3, 4, 3, 4);
            cboLayerType.Name = "cboLayerType";
            cboLayerType.Size = new Size(182, 27);
            cboLayerType.TabIndex = 3;
            // 
            // lblBorderColor
            // 
            lblBorderColor.AutoSize = true;
            lblBorderColor.Font = new Font("Segoe UI", 8.5F);
            lblBorderColor.ForeColor = Color.FromArgb(60, 60, 60);
            lblBorderColor.Location = new Point(11, 97);
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
            pnlBorderColor.Location = new Point(137, 92);
            pnlBorderColor.Margin = new Padding(3, 4, 3, 4);
            pnlBorderColor.Name = "pnlBorderColor";
            pnlBorderColor.Size = new Size(45, 29);
            pnlBorderColor.TabIndex = 5;
            pnlBorderColor.Click += pnlBorderColor_Click;
            // 
            // btnBorderColor
            // 
            btnBorderColor.FlatStyle = FlatStyle.Flat;
            btnBorderColor.Font = new Font("Segoe UI", 7.5F);
            btnBorderColor.Location = new Point(190, 92);
            btnBorderColor.Margin = new Padding(3, 4, 3, 4);
            btnBorderColor.Name = "btnBorderColor";
            btnBorderColor.Size = new Size(69, 29);
            btnBorderColor.TabIndex = 6;
            btnBorderColor.Text = "Choose…";
            btnBorderColor.Click += btnBorderColor_Click;
            // 
            // lblLineStyle
            // 
            lblLineStyle.AutoSize = true;
            lblLineStyle.Font = new Font("Segoe UI", 8.5F);
            lblLineStyle.ForeColor = Color.FromArgb(60, 60, 60);
            lblLineStyle.Location = new Point(11, 137);
            lblLineStyle.Name = "lblLineStyle";
            lblLineStyle.Size = new Size(75, 20);
            lblLineStyle.TabIndex = 7;
            lblLineStyle.Text = "Line Style:";
            // 
            // cboLineStyle
            // 
            cboLineStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLineStyle.FlatStyle = FlatStyle.Flat;
            cboLineStyle.Font = new Font("Segoe UI", 8.5F);
            cboLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "DashDot" });
            cboLineStyle.Location = new Point(137, 131);
            cboLineStyle.Margin = new Padding(3, 4, 3, 4);
            cboLineStyle.Name = "cboLineStyle";
            cboLineStyle.Size = new Size(182, 27);
            cboLineStyle.TabIndex = 8;
            // 
            // lblLineWeight
            // 
            lblLineWeight.AutoSize = true;
            lblLineWeight.Font = new Font("Segoe UI", 8.5F);
            lblLineWeight.ForeColor = Color.FromArgb(60, 60, 60);
            lblLineWeight.Location = new Point(11, 177);
            lblLineWeight.Name = "lblLineWeight";
            lblLineWeight.Size = new Size(90, 20);
            lblLineWeight.TabIndex = 9;
            lblLineWeight.Text = "Line Weight:";
            // 
            // cboLineWeight
            // 
            cboLineWeight.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLineWeight.FlatStyle = FlatStyle.Flat;
            cboLineWeight.Font = new Font("Segoe UI", 8.5F);
            cboLineWeight.Items.AddRange(new object[] { "0.25", "0.5", "1.0", "1.5", "2.0", "3.0" });
            cboLineWeight.Location = new Point(137, 171);
            cboLineWeight.Margin = new Padding(3, 4, 3, 4);
            cboLineWeight.Name = "cboLineWeight";
            cboLineWeight.Size = new Size(182, 27);
            cboLineWeight.TabIndex = 10;
            // 
            // chkVisible
            // 
            chkVisible.AutoSize = true;
            chkVisible.Font = new Font("Segoe UI", 8.5F);
            chkVisible.Location = new Point(11, 219);
            chkVisible.Margin = new Padding(3, 4, 3, 4);
            chkVisible.Name = "chkVisible";
            chkVisible.Size = new Size(75, 24);
            chkVisible.TabIndex = 11;
            chkVisible.Text = "Visible";
            // 
            // chkLocked
            // 
            chkLocked.AutoSize = true;
            chkLocked.Font = new Font("Segoe UI", 8.5F);
            chkLocked.Location = new Point(91, 219);
            chkLocked.Margin = new Padding(3, 4, 3, 4);
            chkLocked.Name = "chkLocked";
            chkLocked.Size = new Size(78, 24);
            chkLocked.TabIndex = 12;
            chkLocked.Text = "Locked";
            // 
            // chkSelectable
            // 
            chkSelectable.AutoSize = true;
            chkSelectable.Font = new Font("Segoe UI", 8.5F);
            chkSelectable.Location = new Point(183, 219);
            chkSelectable.Margin = new Padding(3, 4, 3, 4);
            chkSelectable.Name = "chkSelectable";
            chkSelectable.Size = new Size(100, 24);
            chkSelectable.TabIndex = 13;
            chkSelectable.Text = "Selectable";
            // 
            // chkPrintable
            // 
            chkPrintable.AutoSize = true;
            chkPrintable.Font = new Font("Segoe UI", 8.5F);
            chkPrintable.Location = new Point(286, 219);
            chkPrintable.Margin = new Padding(3, 4, 3, 4);
            chkPrintable.Name = "chkPrintable";
            chkPrintable.Size = new Size(90, 24);
            chkPrintable.TabIndex = 14;
            chkPrintable.Text = "Printable";
            // 
            // tabFill
            // 
            tabFill.BackColor = Color.White;
            tabFill.Controls.Add(lblFillColor);
            tabFill.Controls.Add(pnlFillColor);
            tabFill.Controls.Add(btnFillColor);
            tabFill.Controls.Add(lblFillStyle);
            tabFill.Controls.Add(cboFillStyle);
            tabFill.Controls.Add(lblHatch);
            tabFill.Controls.Add(cboHatch);
            tabFill.Controls.Add(lblTransparency);
            tabFill.Controls.Add(trkTransparency);
            tabFill.Controls.Add(lblTranspValue);
            tabFill.Location = new Point(4, 28);
            tabFill.Margin = new Padding(3, 4, 3, 4);
            tabFill.Name = "tabFill";
            tabFill.Padding = new Padding(11, 11, 11, 11);
            tabFill.Size = new Size(387, 520);
            tabFill.TabIndex = 1;
            tabFill.Text = "Fill";
            // 
            // lblFillColor
            // 
            lblFillColor.AutoSize = true;
            lblFillColor.Font = new Font("Segoe UI", 8.5F);
            lblFillColor.ForeColor = Color.FromArgb(60, 60, 60);
            lblFillColor.Location = new Point(11, 17);
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
            pnlFillColor.Location = new Point(137, 12);
            pnlFillColor.Margin = new Padding(3, 4, 3, 4);
            pnlFillColor.Name = "pnlFillColor";
            pnlFillColor.Size = new Size(45, 29);
            pnlFillColor.TabIndex = 1;
            pnlFillColor.Click += pnlFillColor_Click;
            // 
            // btnFillColor
            // 
            btnFillColor.FlatStyle = FlatStyle.Flat;
            btnFillColor.Font = new Font("Segoe UI", 7.5F);
            btnFillColor.Location = new Point(190, 12);
            btnFillColor.Margin = new Padding(3, 4, 3, 4);
            btnFillColor.Name = "btnFillColor";
            btnFillColor.Size = new Size(69, 29);
            btnFillColor.TabIndex = 2;
            btnFillColor.Text = "Choose…";
            btnFillColor.Click += btnFillColor_Click;
            // 
            // lblFillStyle
            // 
            lblFillStyle.AutoSize = true;
            lblFillStyle.Font = new Font("Segoe UI", 8.5F);
            lblFillStyle.ForeColor = Color.FromArgb(60, 60, 60);
            lblFillStyle.Location = new Point(11, 57);
            lblFillStyle.Name = "lblFillStyle";
            lblFillStyle.Size = new Size(67, 20);
            lblFillStyle.TabIndex = 3;
            lblFillStyle.Text = "Fill Style:";
            // 
            // cboFillStyle
            // 
            cboFillStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFillStyle.FlatStyle = FlatStyle.Flat;
            cboFillStyle.Font = new Font("Segoe UI", 8.5F);
            cboFillStyle.Items.AddRange(new object[] { "None", "Solid", "Hatched" });
            cboFillStyle.Location = new Point(137, 51);
            cboFillStyle.Margin = new Padding(3, 4, 3, 4);
            cboFillStyle.Name = "cboFillStyle";
            cboFillStyle.Size = new Size(148, 27);
            cboFillStyle.TabIndex = 4;
            cboFillStyle.SelectedIndexChanged += cboFillStyle_SelectedIndexChanged;
            // 
            // lblHatch
            // 
            lblHatch.AutoSize = true;
            lblHatch.Font = new Font("Segoe UI", 8.5F);
            lblHatch.ForeColor = Color.FromArgb(60, 60, 60);
            lblHatch.Location = new Point(11, 97);
            lblHatch.Name = "lblHatch";
            lblHatch.Size = new Size(101, 20);
            lblHatch.TabIndex = 5;
            lblHatch.Text = "Hatch Pattern:";
            // 
            // cboHatch
            // 
            cboHatch.DropDownStyle = ComboBoxStyle.DropDownList;
            cboHatch.Enabled = false;
            cboHatch.FlatStyle = FlatStyle.Flat;
            cboHatch.Font = new Font("Segoe UI", 8.5F);
            cboHatch.Items.AddRange(new object[] { "ANSI31", "ANSI32", "ANSI33", "ANSI34", "AR-BRSTD", "DOTS", "EARTH" });
            cboHatch.Location = new Point(137, 91);
            cboHatch.Margin = new Padding(3, 4, 3, 4);
            cboHatch.Name = "cboHatch";
            cboHatch.Size = new Size(148, 27);
            cboHatch.TabIndex = 6;
            // 
            // lblTransparency
            // 
            lblTransparency.AutoSize = true;
            lblTransparency.Font = new Font("Segoe UI", 8.5F);
            lblTransparency.ForeColor = Color.FromArgb(60, 60, 60);
            lblTransparency.Location = new Point(11, 137);
            lblTransparency.Name = "lblTransparency";
            lblTransparency.Size = new Size(98, 20);
            lblTransparency.TabIndex = 7;
            lblTransparency.Text = "Transparency:";
            // 
            // trkTransparency
            // 
            trkTransparency.Location = new Point(137, 128);
            trkTransparency.Margin = new Padding(3, 4, 3, 4);
            trkTransparency.Maximum = 100;
            trkTransparency.Name = "trkTransparency";
            trkTransparency.Size = new Size(171, 56);
            trkTransparency.TabIndex = 8;
            trkTransparency.TickFrequency = 10;
            trkTransparency.Scroll += trkTransparency_Scroll;
            // 
            // lblTranspValue
            // 
            lblTranspValue.AutoSize = true;
            lblTranspValue.Font = new Font("Segoe UI", 8.5F);
            lblTranspValue.Location = new Point(315, 133);
            lblTranspValue.Name = "lblTranspValue";
            lblTranspValue.Size = new Size(29, 20);
            lblTranspValue.TabIndex = 9;
            lblTranspValue.Text = "0%";
            // 
            // tabLabel
            // 
            tabLabel.BackColor = Color.White;
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
            tabLabel.Location = new Point(4, 28);
            tabLabel.Margin = new Padding(3, 4, 3, 4);
            tabLabel.Name = "tabLabel";
            tabLabel.Padding = new Padding(11, 11, 11, 11);
            tabLabel.Size = new Size(387, 520);
            tabLabel.TabIndex = 2;
            tabLabel.Text = "Labels";
            // 
            // chkShowLabels
            // 
            chkShowLabels.AutoSize = true;
            chkShowLabels.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            chkShowLabels.Location = new Point(11, 13);
            chkShowLabels.Margin = new Padding(3, 4, 3, 4);
            chkShowLabels.Name = "chkShowLabels";
            chkShowLabels.Size = new Size(192, 24);
            chkShowLabels.TabIndex = 0;
            chkShowLabels.Text = "Show Labels on Canvas";
            chkShowLabels.CheckedChanged += chkShowLabels_CheckedChanged;
            // 
            // lblFont
            // 
            lblFont.AutoSize = true;
            lblFont.Font = new Font("Segoe UI", 8.5F);
            lblFont.ForeColor = Color.FromArgb(60, 60, 60);
            lblFont.Location = new Point(11, 60);
            lblFont.Name = "lblFont";
            lblFont.Size = new Size(41, 20);
            lblFont.TabIndex = 1;
            lblFont.Text = "Font:";
            // 
            // txtFontName
            // 
            txtFontName.BorderStyle = BorderStyle.FixedSingle;
            txtFontName.Font = new Font("Segoe UI", 8.5F);
            txtFontName.Location = new Point(137, 53);
            txtFontName.Margin = new Padding(3, 4, 3, 4);
            txtFontName.Name = "txtFontName";
            txtFontName.ReadOnly = true;
            txtFontName.Size = new Size(125, 26);
            txtFontName.TabIndex = 2;
            // 
            // btnPickFont
            // 
            btnPickFont.FlatStyle = FlatStyle.Flat;
            btnPickFont.Location = new Point(270, 53);
            btnPickFont.Margin = new Padding(3, 4, 3, 4);
            btnPickFont.Name = "btnPickFont";
            btnPickFont.Size = new Size(50, 29);
            btnPickFont.TabIndex = 3;
            btnPickFont.Text = "…";
            btnPickFont.Click += btnPickFont_Click;
            // 
            // lblFontSize
            // 
            lblFontSize.AutoSize = true;
            lblFontSize.Font = new Font("Segoe UI", 8.5F);
            lblFontSize.ForeColor = Color.FromArgb(60, 60, 60);
            lblFontSize.Location = new Point(11, 100);
            lblFontSize.Name = "lblFontSize";
            lblFontSize.Size = new Size(72, 20);
            lblFontSize.TabIndex = 4;
            lblFontSize.Text = "Font Size:";
            // 
            // numFontSize
            // 
            numFontSize.Font = new Font("Segoe UI", 8.5F);
            numFontSize.Location = new Point(137, 93);
            numFontSize.Margin = new Padding(3, 4, 3, 4);
            numFontSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numFontSize.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numFontSize.Name = "numFontSize";
            numFontSize.Size = new Size(80, 26);
            numFontSize.TabIndex = 5;
            numFontSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblLabelColor
            // 
            lblLabelColor.AutoSize = true;
            lblLabelColor.Font = new Font("Segoe UI", 8.5F);
            lblLabelColor.ForeColor = Color.FromArgb(60, 60, 60);
            lblLabelColor.Location = new Point(11, 140);
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
            pnlLabelColor.Location = new Point(137, 135);
            pnlLabelColor.Margin = new Padding(3, 4, 3, 4);
            pnlLabelColor.Name = "pnlLabelColor";
            pnlLabelColor.Size = new Size(45, 29);
            pnlLabelColor.TabIndex = 7;
            pnlLabelColor.Click += pnlLabelColor_Click;
            // 
            // btnLabelColor
            // 
            btnLabelColor.FlatStyle = FlatStyle.Flat;
            btnLabelColor.Font = new Font("Segoe UI", 7.5F);
            btnLabelColor.Location = new Point(190, 135);
            btnLabelColor.Margin = new Padding(3, 4, 3, 4);
            btnLabelColor.Name = "btnLabelColor";
            btnLabelColor.Size = new Size(69, 29);
            btnLabelColor.TabIndex = 8;
            btnLabelColor.Text = "Choose…";
            btnLabelColor.Click += btnLabelColor_Click;
            // 
            // lblLabelField
            // 
            lblLabelField.AutoSize = true;
            lblLabelField.Font = new Font("Segoe UI", 8.5F);
            lblLabelField.ForeColor = Color.FromArgb(60, 60, 60);
            lblLabelField.Location = new Point(11, 180);
            lblLabelField.Name = "lblLabelField";
            lblLabelField.Size = new Size(84, 20);
            lblLabelField.TabIndex = 9;
            lblLabelField.Text = "Show Field:";
            // 
            // cboLabelField
            // 
            cboLabelField.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLabelField.FlatStyle = FlatStyle.Flat;
            cboLabelField.Font = new Font("Segoe UI", 8.5F);
            cboLabelField.Items.AddRange(new object[] { "ParcelNo", "OwnerName", "AreaSqm", "AreaRAPD", "LandUse", "PlotNumber" });
            cboLabelField.Location = new Point(137, 173);
            cboLabelField.Margin = new Padding(3, 4, 3, 4);
            cboLabelField.Name = "cboLabelField";
            cboLabelField.Size = new Size(182, 27);
            cboLabelField.TabIndex = 10;
            // 
            // pnlBottom
            // 
            pnlBottom.BackColor = Color.FromArgb(245, 245, 245);
            pnlBottom.BorderStyle = BorderStyle.FixedSingle;
            pnlBottom.Controls.Add(lblLayerCount);
            pnlBottom.Controls.Add(btnApply);
            pnlBottom.Controls.Add(btnOK);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 712);
            pnlBottom.Margin = new Padding(3, 4, 3, 4);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1237, 61);
            pnlBottom.TabIndex = 3;
            // 
            // lblLayerCount
            // 
            lblLayerCount.AutoSize = true;
            lblLayerCount.Font = new Font("Segoe UI", 8F);
            lblLayerCount.ForeColor = Color.FromArgb(100, 100, 100);
            lblLayerCount.Location = new Point(11, 20);
            lblLayerCount.Name = "lblLayerCount";
            lblLayerCount.Size = new Size(56, 19);
            lblLayerCount.TabIndex = 0;
            lblLayerCount.Text = "0 layers";
            // 
            // btnApply
            // 
            btnApply.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApply.FlatAppearance.BorderColor = Color.FromArgb(26, 82, 118);
            btnApply.FlatStyle = FlatStyle.Flat;
            btnApply.Font = new Font("Segoe UI", 8.5F);
            btnApply.Location = new Point(1686, 13);
            btnApply.Margin = new Padding(3, 4, 3, 4);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(82, 35);
            btnApply.TabIndex = 1;
            btnApply.Text = "Apply";
            btnApply.Click += btnApply_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOK.BackColor = Color.FromArgb(26, 82, 118);
            btnOK.DialogResult = DialogResult.OK;
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(20, 66, 99);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Font = new Font("Segoe UI", 8.5F);
            btnOK.ForeColor = Color.White;
            btnOK.Location = new Point(1775, 13);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(82, 35);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = false;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.BackColor = Color.White;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180, 180, 180);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 8.5F);
            btnCancel.Location = new Point(1864, 13);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(82, 35);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 125);
            splitMain.Margin = new Padding(3, 4, 3, 4);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(dgvLayers);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(grpProperties);
            splitMain.Size = new Size(1237, 587);
            splitMain.SplitterDistance = 823;
            splitMain.SplitterWidth = 5;
            splitMain.TabIndex = 0;
            // 
            // frmLayerManager
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1237, 773);
            Controls.Add(splitMain);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlHeader);
            Controls.Add(pnlBottom);
            Font = new Font("Segoe UI", 9F);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(820, 651);
            Name = "frmLayerManager";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Layer Property Manager — RePlot";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            pnlToolbar.ResumeLayout(false);
            pnlToolbar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLayers).EndInit();
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
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Panel pnlToolbar;
        private System.Windows.Forms.Button btnNewLayer;
        private System.Windows.Forms.Button btnDeleteLayer;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Label sep1;
        private System.Windows.Forms.Button btnShowAll;
        private System.Windows.Forms.Button btnHideAll;
        private System.Windows.Forms.Button btnLockAll;
        private System.Windows.Forms.Label sep2;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.DataGridView dgvLayers;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colVisible;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colLocked;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPrintable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLayerType;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLineStyle;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLineWeight;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisplayOrder;
        private System.Windows.Forms.GroupBox grpProperties;
        private System.Windows.Forms.TabControl tabProperties;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabFill;
        private System.Windows.Forms.TabPage tabLabel;
        private System.Windows.Forms.Label lblLayerName;
        private System.Windows.Forms.TextBox txtLayerName;
        private System.Windows.Forms.Label lblLayerType;
        private System.Windows.Forms.ComboBox cboLayerType;
        private System.Windows.Forms.Label lblBorderColor;
        private System.Windows.Forms.Panel pnlBorderColor;
        private System.Windows.Forms.Button btnBorderColor;
        private System.Windows.Forms.Label lblLineStyle;
        private System.Windows.Forms.ComboBox cboLineStyle;
        private System.Windows.Forms.Label lblLineWeight;
        private System.Windows.Forms.ComboBox cboLineWeight;
        private System.Windows.Forms.CheckBox chkVisible;
        private System.Windows.Forms.CheckBox chkLocked;
        private System.Windows.Forms.CheckBox chkSelectable;
        private System.Windows.Forms.CheckBox chkPrintable;
        private System.Windows.Forms.Label lblFillColor;
        private System.Windows.Forms.Panel pnlFillColor;
        private System.Windows.Forms.Button btnFillColor;
        private System.Windows.Forms.Label lblFillStyle;
        private System.Windows.Forms.ComboBox cboFillStyle;
        private System.Windows.Forms.Label lblTransparency;
        private System.Windows.Forms.TrackBar trkTransparency;
        private System.Windows.Forms.Label lblTranspValue;
        private System.Windows.Forms.Label lblHatch;
        private System.Windows.Forms.ComboBox cboHatch;
        private System.Windows.Forms.CheckBox chkShowLabels;
        private System.Windows.Forms.Label lblFont;
        private System.Windows.Forms.TextBox txtFontName;
        private System.Windows.Forms.Button btnPickFont;
        private System.Windows.Forms.Label lblFontSize;
        private System.Windows.Forms.NumericUpDown numFontSize;
        private System.Windows.Forms.Label lblLabelColor;
        private System.Windows.Forms.Panel pnlLabelColor;
        private System.Windows.Forms.Button btnLabelColor;
        private System.Windows.Forms.Label lblLabelField;
        private System.Windows.Forms.ComboBox cboLabelField;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblLayerCount;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.FontDialog fontDialog1;
    }
}