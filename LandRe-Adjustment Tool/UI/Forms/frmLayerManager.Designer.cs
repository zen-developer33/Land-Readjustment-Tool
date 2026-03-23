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
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
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
            // pnlToolbar
            // 
            pnlToolbar.BackColor = SystemColors.ControlLight;
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
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Padding = new Padding(4);
            pnlToolbar.Size = new Size(1142, 49);
            pnlToolbar.TabIndex = 1;
            // 
            // btnNewLayer
            // 
            btnNewLayer.Location = new Point(7, 8);
            btnNewLayer.Name = "btnNewLayer";
            btnNewLayer.Size = new Size(121, 34);
            btnNewLayer.TabIndex = 0;
            btnNewLayer.Text = "+ New Layer";
            toolTip1.SetToolTip(btnNewLayer, "Add a new layer");
            btnNewLayer.Click += btnNewLayer_Click;
            // 
            // btnDeleteLayer
            // 
            btnDeleteLayer.Location = new Point(134, 8);
            btnDeleteLayer.Name = "btnDeleteLayer";
            btnDeleteLayer.Size = new Size(70, 34);
            btnDeleteLayer.TabIndex = 1;
            btnDeleteLayer.Text = "Delete";
            toolTip1.SetToolTip(btnDeleteLayer, "Delete selected layer");
            btnDeleteLayer.Click += btnDeleteLayer_Click;
            // 
            // btnMoveUp
            // 
            btnMoveUp.Location = new Point(208, 8);
            btnMoveUp.Name = "btnMoveUp";
            btnMoveUp.Size = new Size(64, 34);
            btnMoveUp.TabIndex = 2;
            btnMoveUp.Text = "▲ Up";
            toolTip1.SetToolTip(btnMoveUp, "Move layer up");
            btnMoveUp.Click += btnMoveUp_Click;
            // 
            // btnMoveDown
            // 
            btnMoveDown.Location = new Point(276, 8);
            btnMoveDown.Name = "btnMoveDown";
            btnMoveDown.Size = new Size(78, 34);
            btnMoveDown.TabIndex = 3;
            btnMoveDown.Text = "▼ Down";
            toolTip1.SetToolTip(btnMoveDown, "Move layer down");
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // sep1
            // 
            sep1.BackColor = SystemColors.ControlDark;
            sep1.Location = new Point(318, 5);
            sep1.Name = "sep1";
            sep1.Size = new Size(1, 26);
            sep1.TabIndex = 4;
            // 
            // btnShowAll
            // 
            btnShowAll.Location = new Point(360, 8);
            btnShowAll.Name = "btnShowAll";
            btnShowAll.Size = new Size(72, 34);
            btnShowAll.TabIndex = 5;
            btnShowAll.Text = "Show All";
            btnShowAll.Click += btnShowAll_Click;
            // 
            // btnHideAll
            // 
            btnHideAll.Location = new Point(438, 8);
            btnHideAll.Name = "btnHideAll";
            btnHideAll.Size = new Size(83, 34);
            btnHideAll.TabIndex = 6;
            btnHideAll.Text = "Hide All";
            btnHideAll.Click += btnHideAll_Click;
            // 
            // btnLockAll
            // 
            btnLockAll.Location = new Point(527, 8);
            btnLockAll.Name = "btnLockAll";
            btnLockAll.Size = new Size(70, 34);
            btnLockAll.TabIndex = 7;
            btnLockAll.Text = "Lock All";
            btnLockAll.Click += btnLockAll_Click;
            // 
            // sep2
            // 
            sep2.BackColor = SystemColors.ControlDark;
            sep2.Location = new Point(550, 5);
            sep2.Name = "sep2";
            sep2.Size = new Size(1, 26);
            sep2.TabIndex = 8;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI", 9F);
            lblSearch.Location = new Point(624, 15);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(56, 20);
            lblSearch.TabIndex = 9;
            lblSearch.Text = "Search:";
            // 
            // txtSearch
            // 
            txtSearch.Font = new Font("Segoe UI", 9F);
            txtSearch.Location = new Point(686, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(160, 27);
            txtSearch.TabIndex = 10;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // dgvLayers
            // 
            dgvLayers.AllowUserToAddRows = false;
            dgvLayers.AllowUserToDeleteRows = false;
            dgvLayers.AllowUserToResizeRows = false;
            dgvLayers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLayers.BackgroundColor = SystemColors.Window;
            dgvLayers.BorderStyle = BorderStyle.None;
            dgvLayers.ColumnHeadersHeight = 29;
            dgvLayers.Columns.AddRange(new DataGridViewColumn[] { colVisible, colLocked, colPrintable, colColor, colName, colLayerType, colLineStyle, colLineWeight, colDisplayOrder });
            dgvLayers.Dock = DockStyle.Fill;
            dgvLayers.Font = new Font("Segoe UI", 9F);
            dgvLayers.GridColor = SystemColors.ControlLight;
            dgvLayers.Location = new Point(0, 0);
            dgvLayers.MultiSelect = false;
            dgvLayers.Name = "dgvLayers";
            dgvLayers.RowHeadersVisible = false;
            dgvLayers.RowHeadersWidth = 51;
            dgvLayers.RowTemplate.Height = 26;
            dgvLayers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLayers.Size = new Size(780, 429);
            dgvLayers.TabIndex = 0;
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
            // 
            // colLocked
            // 
            colLocked.DataPropertyName = "IsLocked";
            colLocked.HeaderText = "🔒";
            colLocked.MinimumWidth = 34;
            colLocked.Name = "colLocked";
            colLocked.Resizable = DataGridViewTriState.False;
            // 
            // colPrintable
            // 
            colPrintable.DataPropertyName = "IsPrintable";
            colPrintable.HeaderText = "🖨";
            colPrintable.MinimumWidth = 34;
            colPrintable.Name = "colPrintable";
            colPrintable.Resizable = DataGridViewTriState.False;
            // 
            // colColor
            // 
            colColor.DataPropertyName = "BorderColor";
            colColor.HeaderText = "Color";
            colColor.MinimumWidth = 6;
            colColor.Name = "colColor";
            colColor.ReadOnly = true;
            // 
            // colName
            // 
            colName.DataPropertyName = "Name";
            colName.HeaderText = "Layer Name";
            colName.MinimumWidth = 6;
            colName.Name = "colName";
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
            // 
            // colDisplayOrder
            // 
            colDisplayOrder.DataPropertyName = "DisplayOrder";
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDisplayOrder.DefaultCellStyle = dataGridViewCellStyle4;
            colDisplayOrder.HeaderText = "Order";
            colDisplayOrder.MinimumWidth = 6;
            colDisplayOrder.Name = "colDisplayOrder";
            // 
            // grpProperties
            // 
            grpProperties.Controls.Add(tabProperties);
            grpProperties.Dock = DockStyle.Fill;
            grpProperties.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpProperties.Location = new Point(0, 0);
            grpProperties.Name = "grpProperties";
            grpProperties.Padding = new Padding(6, 8, 6, 6);
            grpProperties.Size = new Size(357, 429);
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
            tabProperties.Font = new Font("Segoe UI", 9F);
            tabProperties.Location = new Point(6, 28);
            tabProperties.Name = "tabProperties";
            tabProperties.SelectedIndex = 0;
            tabProperties.Size = new Size(345, 395);
            tabProperties.TabIndex = 0;
            // 
            // tabGeneral
            // 
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
            tabGeneral.Location = new Point(4, 29);
            tabGeneral.Name = "tabGeneral";
            tabGeneral.Padding = new Padding(10);
            tabGeneral.Size = new Size(337, 375);
            tabGeneral.TabIndex = 0;
            tabGeneral.Text = "General";
            tabGeneral.UseVisualStyleBackColor = true;
            // 
            // lblLayerName
            // 
            lblLayerName.AutoSize = true;
            lblLayerName.Font = new Font("Segoe UI", 9F);
            lblLayerName.Location = new Point(10, 17);
            lblLayerName.Name = "lblLayerName";
            lblLayerName.Size = new Size(52, 20);
            lblLayerName.TabIndex = 0;
            lblLayerName.Text = "Name:";
            // 
            // txtLayerName
            // 
            txtLayerName.Font = new Font("Segoe UI", 9F);
            txtLayerName.Location = new Point(140, 14);
            txtLayerName.Name = "txtLayerName";
            txtLayerName.Size = new Size(184, 27);
            txtLayerName.TabIndex = 1;
            // 
            // lblLayerType
            // 
            lblLayerType.AutoSize = true;
            lblLayerType.Font = new Font("Segoe UI", 9F);
            lblLayerType.Location = new Point(10, 51);
            lblLayerType.Name = "lblLayerType";
            lblLayerType.Size = new Size(43, 20);
            lblLayerType.TabIndex = 2;
            lblLayerType.Text = "Type:";
            // 
            // cboLayerType
            // 
            cboLayerType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLayerType.FlatStyle = FlatStyle.Flat;
            cboLayerType.Font = new Font("Segoe UI", 9F);
            cboLayerType.Items.AddRange(new object[] { "BaselineParcel", "ReplottedParcel", "ProposedRoad", "ExistingRoad", "Block", "ProjectBoundary", "Annotation", "Reference" });
            cboLayerType.Location = new Point(140, 48);
            cboLayerType.Name = "cboLayerType";
            cboLayerType.Size = new Size(184, 28);
            cboLayerType.TabIndex = 3;
            // 
            // lblBorderColor
            // 
            lblBorderColor.AutoSize = true;
            lblBorderColor.Font = new Font("Segoe UI", 9F);
            lblBorderColor.Location = new Point(10, 84);
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
            pnlBorderColor.Location = new Point(140, 82);
            pnlBorderColor.Name = "pnlBorderColor";
            pnlBorderColor.Size = new Size(42, 26);
            pnlBorderColor.TabIndex = 5;
            pnlBorderColor.Click += pnlBorderColor_Click;
            // 
            // btnBorderColor
            // 
            btnBorderColor.Location = new Point(188, 82);
            btnBorderColor.Name = "btnBorderColor";
            btnBorderColor.Size = new Size(75, 26);
            btnBorderColor.TabIndex = 6;
            btnBorderColor.Text = "Choose…";
            btnBorderColor.Click += btnBorderColor_Click;
            // 
            // lblLineStyle
            // 
            lblLineStyle.AutoSize = true;
            lblLineStyle.Font = new Font("Segoe UI", 9F);
            lblLineStyle.Location = new Point(10, 121);
            lblLineStyle.Name = "lblLineStyle";
            lblLineStyle.Size = new Size(75, 20);
            lblLineStyle.TabIndex = 7;
            lblLineStyle.Text = "Line Style:";
            // 
            // cboLineStyle
            // 
            cboLineStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLineStyle.FlatStyle = FlatStyle.Flat;
            cboLineStyle.Font = new Font("Segoe UI", 9F);
            cboLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "DashDot" });
            cboLineStyle.Location = new Point(140, 118);
            cboLineStyle.Name = "cboLineStyle";
            cboLineStyle.Size = new Size(184, 28);
            cboLineStyle.TabIndex = 8;
            // 
            // lblLineWeight
            // 
            lblLineWeight.AutoSize = true;
            lblLineWeight.Font = new Font("Segoe UI", 9F);
            lblLineWeight.Location = new Point(10, 155);
            lblLineWeight.Name = "lblLineWeight";
            lblLineWeight.Size = new Size(90, 20);
            lblLineWeight.TabIndex = 9;
            lblLineWeight.Text = "Line Weight:";
            // 
            // cboLineWeight
            // 
            cboLineWeight.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLineWeight.FlatStyle = FlatStyle.Flat;
            cboLineWeight.Font = new Font("Segoe UI", 9F);
            cboLineWeight.Items.AddRange(new object[] { "0.25", "0.5", "1.0", "1.5", "2.0", "3.0" });
            cboLineWeight.Location = new Point(140, 152);
            cboLineWeight.Name = "cboLineWeight";
            cboLineWeight.Size = new Size(184, 28);
            cboLineWeight.TabIndex = 10;
            // 
            // chkVisible
            // 
            chkVisible.AutoSize = true;
            chkVisible.Font = new Font("Segoe UI", 9F);
            chkVisible.Location = new Point(10, 192);
            chkVisible.Name = "chkVisible";
            chkVisible.Size = new Size(75, 24);
            chkVisible.TabIndex = 11;
            chkVisible.Text = "Visible";
            // 
            // chkLocked
            // 
            chkLocked.AutoSize = true;
            chkLocked.Font = new Font("Segoe UI", 9F);
            chkLocked.Location = new Point(140, 192);
            chkLocked.Name = "chkLocked";
            chkLocked.Size = new Size(78, 24);
            chkLocked.TabIndex = 12;
            chkLocked.Text = "Locked";
            // 
            // chkSelectable
            // 
            chkSelectable.AutoSize = true;
            chkSelectable.Font = new Font("Segoe UI", 9F);
            chkSelectable.Location = new Point(10, 222);
            chkSelectable.Name = "chkSelectable";
            chkSelectable.Size = new Size(100, 24);
            chkSelectable.TabIndex = 13;
            chkSelectable.Text = "Selectable";
            // 
            // chkPrintable
            // 
            chkPrintable.AutoSize = true;
            chkPrintable.Font = new Font("Segoe UI", 9F);
            chkPrintable.Location = new Point(140, 222);
            chkPrintable.Name = "chkPrintable";
            chkPrintable.Size = new Size(90, 24);
            chkPrintable.TabIndex = 14;
            chkPrintable.Text = "Printable";
            // 
            // tabFill
            // 
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
            tabFill.Location = new Point(4, 29);
            tabFill.Name = "tabFill";
            tabFill.Padding = new Padding(10);
            tabFill.Size = new Size(337, 375);
            tabFill.TabIndex = 1;
            tabFill.Text = "Fill";
            tabFill.UseVisualStyleBackColor = true;
            // 
            // lblFillColor
            // 
            lblFillColor.AutoSize = true;
            lblFillColor.Font = new Font("Segoe UI", 9F);
            lblFillColor.Location = new Point(10, 14);
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
            pnlFillColor.Location = new Point(140, 12);
            pnlFillColor.Name = "pnlFillColor";
            pnlFillColor.Size = new Size(42, 26);
            pnlFillColor.TabIndex = 1;
            pnlFillColor.Click += pnlFillColor_Click;
            // 
            // btnFillColor
            // 
            btnFillColor.Location = new Point(188, 12);
            btnFillColor.Name = "btnFillColor";
            btnFillColor.Size = new Size(75, 26);
            btnFillColor.TabIndex = 2;
            btnFillColor.Text = "Choose…";
            btnFillColor.Click += btnFillColor_Click;
            // 
            // lblFillStyle
            // 
            lblFillStyle.AutoSize = true;
            lblFillStyle.Font = new Font("Segoe UI", 9F);
            lblFillStyle.Location = new Point(10, 51);
            lblFillStyle.Name = "lblFillStyle";
            lblFillStyle.Size = new Size(67, 20);
            lblFillStyle.TabIndex = 3;
            lblFillStyle.Text = "Fill Style:";
            // 
            // cboFillStyle
            // 
            cboFillStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFillStyle.FlatStyle = FlatStyle.Flat;
            cboFillStyle.Font = new Font("Segoe UI", 9F);
            cboFillStyle.Items.AddRange(new object[] { "None", "Solid", "Hatched" });
            cboFillStyle.Location = new Point(140, 48);
            cboFillStyle.Name = "cboFillStyle";
            cboFillStyle.Size = new Size(183, 28);
            cboFillStyle.TabIndex = 4;
            cboFillStyle.SelectedIndexChanged += cboFillStyle_SelectedIndexChanged;
            // 
            // lblHatch
            // 
            lblHatch.AutoSize = true;
            lblHatch.Font = new Font("Segoe UI", 9F);
            lblHatch.Location = new Point(10, 85);
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
            cboHatch.Font = new Font("Segoe UI", 9F);
            cboHatch.Items.AddRange(new object[] { "ANSI31", "ANSI32", "ANSI33", "ANSI34", "AR-BRSTD", "DOTS", "EARTH" });
            cboHatch.Location = new Point(140, 82);
            cboHatch.Name = "cboHatch";
            cboHatch.Size = new Size(184, 28);
            cboHatch.TabIndex = 6;
            // 
            // lblTransparency
            // 
            lblTransparency.AutoSize = true;
            lblTransparency.Font = new Font("Segoe UI", 9F);
            lblTransparency.Location = new Point(10, 120);
            lblTransparency.Name = "lblTransparency";
            lblTransparency.Size = new Size(98, 20);
            lblTransparency.TabIndex = 7;
            lblTransparency.Text = "Transparency:";
            // 
            // trkTransparency
            // 
            trkTransparency.Location = new Point(140, 114);
            trkTransparency.Maximum = 100;
            trkTransparency.Name = "trkTransparency";
            trkTransparency.Size = new Size(148, 56);
            trkTransparency.TabIndex = 8;
            trkTransparency.TickFrequency = 10;
            trkTransparency.Scroll += trkTransparency_Scroll;
            // 
            // lblTranspValue
            // 
            lblTranspValue.AutoSize = true;
            lblTranspValue.Font = new Font("Segoe UI", 9F);
            lblTranspValue.Location = new Point(294, 120);
            lblTranspValue.Name = "lblTranspValue";
            lblTranspValue.Size = new Size(29, 20);
            lblTranspValue.TabIndex = 9;
            lblTranspValue.Text = "0%";
            // 
            // tabLabel
            // 
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
            tabLabel.Location = new Point(4, 29);
            tabLabel.Name = "tabLabel";
            tabLabel.Padding = new Padding(10);
            tabLabel.Size = new Size(337, 362);
            tabLabel.TabIndex = 2;
            tabLabel.Text = "Labels";
            tabLabel.UseVisualStyleBackColor = true;
            // 
            // chkShowLabels
            // 
            chkShowLabels.AutoSize = true;
            chkShowLabels.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            chkShowLabels.Location = new Point(10, 12);
            chkShowLabels.Name = "chkShowLabels";
            chkShowLabels.Size = new Size(192, 24);
            chkShowLabels.TabIndex = 0;
            chkShowLabels.Text = "Show Labels on Canvas";
            chkShowLabels.CheckedChanged += chkShowLabels_CheckedChanged;
            // 
            // lblFont
            // 
            lblFont.AutoSize = true;
            lblFont.Font = new Font("Segoe UI", 9F);
            lblFont.Location = new Point(10, 49);
            lblFont.Name = "lblFont";
            lblFont.Size = new Size(41, 20);
            lblFont.TabIndex = 1;
            lblFont.Text = "Font:";
            // 
            // txtFontName
            // 
            txtFontName.Font = new Font("Segoe UI", 9F);
            txtFontName.Location = new Point(140, 46);
            txtFontName.Name = "txtFontName";
            txtFontName.ReadOnly = true;
            txtFontName.Size = new Size(142, 27);
            txtFontName.TabIndex = 2;
            // 
            // btnPickFont
            // 
            btnPickFont.Location = new Point(288, 46);
            btnPickFont.Name = "btnPickFont";
            btnPickFont.Size = new Size(36, 26);
            btnPickFont.TabIndex = 3;
            btnPickFont.Text = "…";
            btnPickFont.Click += btnPickFont_Click;
            // 
            // lblFontSize
            // 
            lblFontSize.AutoSize = true;
            lblFontSize.Font = new Font("Segoe UI", 9F);
            lblFontSize.Location = new Point(10, 83);
            lblFontSize.Name = "lblFontSize";
            lblFontSize.Size = new Size(72, 20);
            lblFontSize.TabIndex = 4;
            lblFontSize.Text = "Font Size:";
            // 
            // numFontSize
            // 
            numFontSize.Font = new Font("Segoe UI", 9F);
            numFontSize.Location = new Point(140, 80);
            numFontSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            numFontSize.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            numFontSize.Name = "numFontSize";
            numFontSize.Size = new Size(80, 27);
            numFontSize.TabIndex = 5;
            numFontSize.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // lblLabelColor
            // 
            lblLabelColor.AutoSize = true;
            lblLabelColor.Font = new Font("Segoe UI", 9F);
            lblLabelColor.Location = new Point(10, 116);
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
            pnlLabelColor.Location = new Point(140, 114);
            pnlLabelColor.Name = "pnlLabelColor";
            pnlLabelColor.Size = new Size(42, 26);
            pnlLabelColor.TabIndex = 7;
            pnlLabelColor.Click += pnlLabelColor_Click;
            // 
            // btnLabelColor
            // 
            btnLabelColor.Location = new Point(188, 114);
            btnLabelColor.Name = "btnLabelColor";
            btnLabelColor.Size = new Size(75, 26);
            btnLabelColor.TabIndex = 8;
            btnLabelColor.Text = "Choose…";
            btnLabelColor.Click += btnLabelColor_Click;
            // 
            // lblLabelField
            // 
            lblLabelField.AutoSize = true;
            lblLabelField.Font = new Font("Segoe UI", 9F);
            lblLabelField.Location = new Point(10, 153);
            lblLabelField.Name = "lblLabelField";
            lblLabelField.Size = new Size(84, 20);
            lblLabelField.TabIndex = 9;
            lblLabelField.Text = "Show Field:";
            // 
            // cboLabelField
            // 
            cboLabelField.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLabelField.FlatStyle = FlatStyle.Flat;
            cboLabelField.Font = new Font("Segoe UI", 9F);
            cboLabelField.Items.AddRange(new object[] { "ParcelNo", "OwnerName", "AreaSqm", "AreaRAPD", "LandUse", "PlotNumber" });
            cboLabelField.Location = new Point(140, 150);
            cboLabelField.Name = "cboLabelField";
            cboLabelField.Size = new Size(184, 28);
            cboLabelField.TabIndex = 10;
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(lblLayerCount);
            pnlBottom.Controls.Add(btnApply);
            pnlBottom.Controls.Add(btnOK);
            pnlBottom.Controls.Add(btnCancel);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 478);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Padding = new Padding(6);
            pnlBottom.Size = new Size(1142, 42);
            pnlBottom.TabIndex = 3;
            // 
            // lblLayerCount
            // 
            lblLayerCount.AutoSize = true;
            lblLayerCount.Font = new Font("Segoe UI", 8.5F);
            lblLayerCount.ForeColor = SystemColors.GrayText;
            lblLayerCount.Location = new Point(8, 13);
            lblLayerCount.Name = "lblLayerCount";
            lblLayerCount.Size = new Size(59, 20);
            lblLayerCount.TabIndex = 0;
            lblLayerCount.Text = "0 layers";
            // 
            // btnApply
            // 
            btnApply.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApply.Location = new Point(1776, 6);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(80, 30);
            btnApply.TabIndex = 1;
            btnApply.Text = "Apply";
            btnApply.Click += btnApply_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(1862, 6);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 30);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(1948, 6);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 49);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(dgvLayers);
            splitMain.Panel1MinSize = 50;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(grpProperties);
            splitMain.Panel2MinSize = 350;
            splitMain.Size = new Size(1142, 429);
            splitMain.SplitterDistance = 780;
            splitMain.SplitterWidth = 5;
            splitMain.TabIndex = 0;
            // 
            // frmLayerManager
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1142, 520);
            Controls.Add(splitMain);
            Controls.Add(pnlToolbar);
            Controls.Add(pnlBottom);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(820, 560);
            Name = "frmLayerManager";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Layer Property Manager";
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

        private Panel pnlToolbar;
        private Button btnNewLayer;
        private Button btnDeleteLayer;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Label sep1;
        private Button btnShowAll;
        private Button btnHideAll;
        private Button btnLockAll;
        private Label sep2;
        private Label lblSearch;
        private TextBox txtSearch;
        private DataGridView dgvLayers;
        private DataGridViewCheckBoxColumn colVisible;
        private DataGridViewCheckBoxColumn colLocked;
        private DataGridViewCheckBoxColumn colPrintable;
        private DataGridViewTextBoxColumn colColor;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewComboBoxColumn colLayerType;
        private DataGridViewComboBoxColumn colLineStyle;
        private DataGridViewComboBoxColumn colLineWeight;
        private DataGridViewTextBoxColumn colDisplayOrder;
        private GroupBox grpProperties;
        private TabControl tabProperties;
        private TabPage tabGeneral;
        private Label lblLayerName;
        private TextBox txtLayerName;
        private Label lblLayerType;
        private ComboBox cboLayerType;
        private Label lblBorderColor;
        private Panel pnlBorderColor;
        private Button btnBorderColor;
        private Label lblLineStyle;
        private ComboBox cboLineStyle;
        private Label lblLineWeight;
        private ComboBox cboLineWeight;
        private CheckBox chkVisible;
        private CheckBox chkLocked;
        private CheckBox chkSelectable;
        private CheckBox chkPrintable;
        private TabPage tabFill;
        private Label lblFillColor;
        private Panel pnlFillColor;
        private Button btnFillColor;
        private Label lblFillStyle;
        private ComboBox cboFillStyle;
        private Label lblHatch;
        private ComboBox cboHatch;
        private Label lblTransparency;
        private TrackBar trkTransparency;
        private Label lblTranspValue;
        private TabPage tabLabel;
        private CheckBox chkShowLabels;
        private Label lblFont;
        private TextBox txtFontName;
        private Button btnPickFont;
        private Label lblFontSize;
        private NumericUpDown numFontSize;
        private Label lblLabelColor;
        private Panel pnlLabelColor;
        private Button btnLabelColor;
        private Label lblLabelField;
        private ComboBox cboLabelField;
        private Panel pnlBottom;
        private Label lblLayerCount;
        private Button btnApply;
        private Button btnOK;
        private Button btnCancel;
        private SplitContainer splitMain;
        private ToolTip toolTip1;
        private ColorDialog colorDialog1;
        private FontDialog fontDialog1;
        private PictureBox picLogo;
    }
}