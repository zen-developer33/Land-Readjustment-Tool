namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmLayerPropertyManager
    {
        private System.ComponentModel.IContainer components = null!;

        private TabControl _tabs = null!;
        private TabPage _tabGeneral = null!;
        private TabPage _tabFill = null!;
        private TabPage _tabLabel = null!;
        private TableLayoutPanel _generalLayout = null!;
        private TableLayoutPanel _fillLayout = null!;
        private TableLayoutPanel _labelLayout = null!;
        private TableLayoutPanel _transparencyLayout = null!;
        private FlowLayoutPanel _statePanel = null!;
        private FlowLayoutPanel _borderColorPanel = null!;
        private FlowLayoutPanel _fillColorPanel = null!;
        private FlowLayoutPanel _labelColorPanel = null!;
        private FlowLayoutPanel _footerPanel = null!;
        private TableLayoutPanel _fontPanel = null!;

        private Label _lblName = null!;
        private Label _lblBorderColor = null!;
        private Label _lblLineStyle = null!;
        private Label _lblLineWeight = null!;
        private Label _lblState = null!;
        private Label _lblFillStyle = null!;
        private Label _lblFillColor = null!;
        private Label _lblHatch = null!;
        private Label _lblTransparency = null!;
        private Label _lblTransparencyValue = null!;
        private Label _lblLabels = null!;
        private Label _lblFont = null!;
        private Label _lblFontSize = null!;
        private Label _lblTextColor = null!;
        private Label _lblLabelField = null!;

        private TextBox _txtName = null!;
        private Panel _pnlBorderColor = null!;
        private ComboBox _cboLineStyle = null!;
        private NumericUpDown _numLineWeight = null!;
        private CheckBox _chkVisible = null!;
        private CheckBox _chkLocked = null!;
        private ComboBox _cboFillStyle = null!;
        private Panel _pnlFillColor = null!;
        private ComboBox _cboHatch = null!;
        private TrackBar _trkTransparency = null!;
        private CheckBox _chkShowLabels = null!;
        private TextBox _txtFontName = null!;
        private Button _btnFont = null!;
        private NumericUpDown _numFontSize = null!;
        private Panel _pnlLabelColor = null!;
        private ComboBox _cboLabelField = null!;
        private Button _btnBorderColor = null!;
        private Button _btnFillColor = null!;
        private Button _btnLabelColor = null!;
        private Button _btnOk = null!;
        private Button _btnCancel = null!;
        private ColorDialog _colorDialog = null!;
        private FontDialog _fontDialog = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _tabs = new TabControl();
            _tabGeneral = new TabPage();
            _generalLayout = new TableLayoutPanel();
            _lblName = new Label();
            _txtName = new TextBox();
            _lblBorderColor = new Label();
            _borderColorPanel = new FlowLayoutPanel();
            _pnlBorderColor = new Panel();
            _btnBorderColor = new Button();
            _lblLineStyle = new Label();
            _cboLineStyle = new ComboBox();
            _lblLineWeight = new Label();
            _numLineWeight = new NumericUpDown();
            _lblState = new Label();
            _statePanel = new FlowLayoutPanel();
            _chkVisible = new CheckBox();
            _chkLocked = new CheckBox();
            _tabFill = new TabPage();
            _fillLayout = new TableLayoutPanel();
            _lblFillStyle = new Label();
            _cboFillStyle = new ComboBox();
            _lblFillColor = new Label();
            _fillColorPanel = new FlowLayoutPanel();
            _pnlFillColor = new Panel();
            _btnFillColor = new Button();
            _lblHatch = new Label();
            _cboHatch = new ComboBox();
            _lblTransparency = new Label();
            _transparencyLayout = new TableLayoutPanel();
            _trkTransparency = new TrackBar();
            _lblTransparencyValue = new Label();
            _tabLabel = new TabPage();
            _labelLayout = new TableLayoutPanel();
            _lblLabels = new Label();
            _chkShowLabels = new CheckBox();
            _lblFont = new Label();
            _fontPanel = new TableLayoutPanel();
            _txtFontName = new TextBox();
            _btnFont = new Button();
            _lblFontSize = new Label();
            _numFontSize = new NumericUpDown();
            _lblTextColor = new Label();
            _labelColorPanel = new FlowLayoutPanel();
            _pnlLabelColor = new Panel();
            _btnLabelColor = new Button();
            _lblLabelField = new Label();
            _cboLabelField = new ComboBox();
            _footerPanel = new FlowLayoutPanel();
            _btnOk = new Button();
            _btnCancel = new Button();
            _colorDialog = new ColorDialog();
            _fontDialog = new FontDialog();
            _tabs.SuspendLayout();
            _tabGeneral.SuspendLayout();
            _generalLayout.SuspendLayout();
            _borderColorPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numLineWeight).BeginInit();
            _statePanel.SuspendLayout();
            _tabFill.SuspendLayout();
            _fillLayout.SuspendLayout();
            _fillColorPanel.SuspendLayout();
            _transparencyLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_trkTransparency).BeginInit();
            _tabLabel.SuspendLayout();
            _labelLayout.SuspendLayout();
            _fontPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numFontSize).BeginInit();
            _labelColorPanel.SuspendLayout();
            _footerPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _tabs
            // 
            _tabs.Controls.Add(_tabGeneral);
            _tabs.Controls.Add(_tabFill);
            _tabs.Controls.Add(_tabLabel);
            _tabs.Dock = DockStyle.Fill;
            _tabs.Font = new Font("Segoe UI", 9F);
            _tabs.Location = new Point(0, 0);
            _tabs.Name = "_tabs";
            _tabs.Padding = new Point(12, 4);
            _tabs.SelectedIndex = 0;
            _tabs.Size = new Size(351, 271);
            _tabs.TabIndex = 0;
            // 
            // _tabGeneral
            // 
            _tabGeneral.BackColor = Color.White;
            _tabGeneral.Controls.Add(_generalLayout);
            _tabGeneral.Location = new Point(4, 31);
            _tabGeneral.Name = "_tabGeneral";
            _tabGeneral.Padding = new Padding(12);
            _tabGeneral.Size = new Size(343, 236);
            _tabGeneral.TabIndex = 0;
            _tabGeneral.Text = "General";
            // 
            // _generalLayout
            // 
            _generalLayout.AutoScroll = true;
            _generalLayout.AutoSize = true;
            _generalLayout.ColumnCount = 2;
            _generalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            _generalLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _generalLayout.Controls.Add(_lblName, 0, 0);
            _generalLayout.Controls.Add(_txtName, 1, 0);
            _generalLayout.Controls.Add(_lblBorderColor, 0, 1);
            _generalLayout.Controls.Add(_borderColorPanel, 1, 1);
            _generalLayout.Controls.Add(_lblLineStyle, 0, 2);
            _generalLayout.Controls.Add(_cboLineStyle, 1, 2);
            _generalLayout.Controls.Add(_lblLineWeight, 0, 3);
            _generalLayout.Controls.Add(_numLineWeight, 1, 3);
            _generalLayout.Controls.Add(_lblState, 0, 4);
            _generalLayout.Controls.Add(_statePanel, 1, 4);
            _generalLayout.Dock = DockStyle.Fill;
            _generalLayout.Location = new Point(12, 12);
            _generalLayout.Name = "_generalLayout";
            _generalLayout.RowCount = 8;
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _generalLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _generalLayout.Size = new Size(319, 212);
            _generalLayout.TabIndex = 0;
            // 
            // _lblName
            // 
            _lblName.Dock = DockStyle.Fill;
            _lblName.Location = new Point(3, 0);
            _lblName.Name = "_lblName";
            _lblName.Size = new Size(104, 38);
            _lblName.TabIndex = 0;
            _lblName.Text = "Layer Name";
            _lblName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _txtName
            // 
            _txtName.Dock = DockStyle.Fill;
            _txtName.Location = new Point(110, 4);
            _txtName.Margin = new Padding(0, 4, 0, 4);
            _txtName.Name = "_txtName";
            _txtName.ReadOnly = true;
            _txtName.Size = new Size(209, 27);
            _txtName.TabIndex = 1;
            // 
            // _lblBorderColor
            // 
            _lblBorderColor.Dock = DockStyle.Fill;
            _lblBorderColor.Location = new Point(3, 38);
            _lblBorderColor.Name = "_lblBorderColor";
            _lblBorderColor.Size = new Size(104, 38);
            _lblBorderColor.TabIndex = 2;
            _lblBorderColor.Text = "Border Color";
            _lblBorderColor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _borderColorPanel
            // 
            _borderColorPanel.Controls.Add(_pnlBorderColor);
            _borderColorPanel.Controls.Add(_btnBorderColor);
            _borderColorPanel.Dock = DockStyle.Fill;
            _borderColorPanel.Location = new Point(110, 38);
            _borderColorPanel.Margin = new Padding(0);
            _borderColorPanel.Name = "_borderColorPanel";
            _borderColorPanel.Size = new Size(209, 38);
            _borderColorPanel.TabIndex = 3;
            _borderColorPanel.WrapContents = false;
            // 
            // _pnlBorderColor
            // 
            _pnlBorderColor.BorderStyle = BorderStyle.FixedSingle;
            _pnlBorderColor.Cursor = Cursors.Hand;
            _pnlBorderColor.Location = new Point(0, 4);
            _pnlBorderColor.Margin = new Padding(0, 4, 8, 0);
            _pnlBorderColor.Name = "_pnlBorderColor";
            _pnlBorderColor.Size = new Size(36, 29);
            _pnlBorderColor.TabIndex = 0;
            _pnlBorderColor.Click += pnlBorderColor_Click;
            // 
            // _btnBorderColor
            // 
            _btnBorderColor.AutoSize = true;
            _btnBorderColor.Location = new Point(47, 3);
            _btnBorderColor.Name = "_btnBorderColor";
            _btnBorderColor.Size = new Size(78, 30);
            _btnBorderColor.TabIndex = 1;
            _btnBorderColor.Text = "Change...";
            _btnBorderColor.UseVisualStyleBackColor = true;
            _btnBorderColor.Click += btnBorderColor_Click;
            // 
            // _lblLineStyle
            // 
            _lblLineStyle.Dock = DockStyle.Fill;
            _lblLineStyle.Location = new Point(3, 76);
            _lblLineStyle.Name = "_lblLineStyle";
            _lblLineStyle.Size = new Size(104, 38);
            _lblLineStyle.TabIndex = 4;
            _lblLineStyle.Text = "Line Style";
            _lblLineStyle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboLineStyle
            // 
            _cboLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "DashDot" });
            _cboLineStyle.Location = new Point(110, 80);
            _cboLineStyle.Margin = new Padding(0, 4, 0, 4);
            _cboLineStyle.Name = "_cboLineStyle";
            _cboLineStyle.Size = new Size(209, 28);
            _cboLineStyle.TabIndex = 5;
            // 
            // _lblLineWeight
            // 
            _lblLineWeight.Dock = DockStyle.Fill;
            _lblLineWeight.Location = new Point(3, 114);
            _lblLineWeight.Name = "_lblLineWeight";
            _lblLineWeight.Size = new Size(104, 38);
            _lblLineWeight.TabIndex = 6;
            _lblLineWeight.Text = "Line Weight";
            _lblLineWeight.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _numLineWeight
            // 
            _numLineWeight.DecimalPlaces = 2;
            _numLineWeight.Dock = DockStyle.Left;
            _numLineWeight.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
            _numLineWeight.Location = new Point(110, 118);
            _numLineWeight.Margin = new Padding(0, 4, 0, 4);
            _numLineWeight.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            _numLineWeight.Name = "_numLineWeight";
            _numLineWeight.Size = new Size(110, 27);
            _numLineWeight.TabIndex = 7;
            _numLineWeight.Value = new decimal(new int[] { 1, 0, 0, 131072 });
            // 
            // _lblState
            // 
            _lblState.Dock = DockStyle.Fill;
            _lblState.Location = new Point(3, 152);
            _lblState.Name = "_lblState";
            _lblState.Size = new Size(104, 39);
            _lblState.TabIndex = 8;
            _lblState.Text = "State";
            _lblState.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _statePanel
            // 
            _statePanel.Controls.Add(_chkVisible);
            _statePanel.Controls.Add(_chkLocked);
            _statePanel.Dock = DockStyle.Fill;
            _statePanel.Location = new Point(110, 152);
            _statePanel.Margin = new Padding(0);
            _statePanel.Name = "_statePanel";
            _statePanel.Size = new Size(209, 39);
            _statePanel.TabIndex = 9;
            // 
            // _chkVisible
            // 
            _chkVisible.AutoSize = true;
            _chkVisible.Location = new Point(0, 8);
            _chkVisible.Margin = new Padding(0, 8, 18, 0);
            _chkVisible.Name = "_chkVisible";
            _chkVisible.Size = new Size(75, 24);
            _chkVisible.TabIndex = 0;
            _chkVisible.Text = "Visible";
            _chkVisible.UseVisualStyleBackColor = true;
            // 
            // _chkLocked
            // 
            _chkLocked.AutoSize = true;
            _chkLocked.Location = new Point(93, 8);
            _chkLocked.Margin = new Padding(0, 8, 18, 0);
            _chkLocked.Name = "_chkLocked";
            _chkLocked.Size = new Size(78, 24);
            _chkLocked.TabIndex = 1;
            _chkLocked.Text = "Locked";
            _chkLocked.UseVisualStyleBackColor = true;
            // 
            // _tabFill
            // 
            _tabFill.BackColor = Color.White;
            _tabFill.Controls.Add(_fillLayout);
            _tabFill.Location = new Point(4, 31);
            _tabFill.Name = "_tabFill";
            _tabFill.Padding = new Padding(12);
            _tabFill.Size = new Size(343, 236);
            _tabFill.TabIndex = 1;
            _tabFill.Text = "Fill";
            // 
            // _fillLayout
            // 
            _fillLayout.AutoScroll = true;
            _fillLayout.ColumnCount = 2;
            _fillLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            _fillLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _fillLayout.Controls.Add(_lblFillStyle, 0, 0);
            _fillLayout.Controls.Add(_cboFillStyle, 1, 0);
            _fillLayout.Controls.Add(_lblFillColor, 0, 1);
            _fillLayout.Controls.Add(_fillColorPanel, 1, 1);
            _fillLayout.Controls.Add(_lblHatch, 0, 2);
            _fillLayout.Controls.Add(_cboHatch, 1, 2);
            _fillLayout.Controls.Add(_lblTransparency, 0, 3);
            _fillLayout.Controls.Add(_transparencyLayout, 1, 3);
            _fillLayout.Dock = DockStyle.Fill;
            _fillLayout.Location = new Point(12, 12);
            _fillLayout.Name = "_fillLayout";
            _fillLayout.RowCount = 8;
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _fillLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _fillLayout.Size = new Size(319, 212);
            _fillLayout.TabIndex = 0;
            // 
            // _lblFillStyle
            // 
            _lblFillStyle.Dock = DockStyle.Fill;
            _lblFillStyle.Location = new Point(3, 0);
            _lblFillStyle.Name = "_lblFillStyle";
            _lblFillStyle.Size = new Size(104, 38);
            _lblFillStyle.TabIndex = 0;
            _lblFillStyle.Text = "Fill Style";
            _lblFillStyle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboFillStyle
            // 
            _cboFillStyle.Dock = DockStyle.Fill;
            _cboFillStyle.Items.AddRange(new object[] { "None", "Solid", "Hatched" });
            _cboFillStyle.Location = new Point(110, 4);
            _cboFillStyle.Margin = new Padding(0, 4, 0, 4);
            _cboFillStyle.Name = "_cboFillStyle";
            _cboFillStyle.Size = new Size(209, 28);
            _cboFillStyle.TabIndex = 1;
            _cboFillStyle.SelectedIndexChanged += cboFillStyle_SelectedIndexChanged;
            // 
            // _lblFillColor
            // 
            _lblFillColor.Dock = DockStyle.Fill;
            _lblFillColor.Location = new Point(3, 38);
            _lblFillColor.Name = "_lblFillColor";
            _lblFillColor.Size = new Size(104, 38);
            _lblFillColor.TabIndex = 2;
            _lblFillColor.Text = "Fill Color";
            _lblFillColor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fillColorPanel
            // 
            _fillColorPanel.Controls.Add(_pnlFillColor);
            _fillColorPanel.Controls.Add(_btnFillColor);
            _fillColorPanel.Dock = DockStyle.Fill;
            _fillColorPanel.Location = new Point(110, 38);
            _fillColorPanel.Margin = new Padding(0);
            _fillColorPanel.Name = "_fillColorPanel";
            _fillColorPanel.Size = new Size(209, 38);
            _fillColorPanel.TabIndex = 3;
            _fillColorPanel.WrapContents = false;
            // 
            // _pnlFillColor
            // 
            _pnlFillColor.BorderStyle = BorderStyle.FixedSingle;
            _pnlFillColor.Cursor = Cursors.Hand;
            _pnlFillColor.Location = new Point(0, 4);
            _pnlFillColor.Margin = new Padding(0, 4, 8, 0);
            _pnlFillColor.Name = "_pnlFillColor";
            _pnlFillColor.Size = new Size(36, 29);
            _pnlFillColor.TabIndex = 0;
            _pnlFillColor.Click += pnlFillColor_Click;
            // 
            // _btnFillColor
            // 
            _btnFillColor.AutoSize = true;
            _btnFillColor.Location = new Point(47, 3);
            _btnFillColor.Name = "_btnFillColor";
            _btnFillColor.Size = new Size(78, 30);
            _btnFillColor.TabIndex = 1;
            _btnFillColor.Text = "Change...";
            _btnFillColor.UseVisualStyleBackColor = true;
            _btnFillColor.Click += btnFillColor_Click;
            // 
            // _lblHatch
            // 
            _lblHatch.Dock = DockStyle.Fill;
            _lblHatch.Location = new Point(3, 76);
            _lblHatch.Name = "_lblHatch";
            _lblHatch.Size = new Size(104, 38);
            _lblHatch.TabIndex = 4;
            _lblHatch.Text = "Hatch";
            _lblHatch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboHatch
            // 
            _cboHatch.Dock = DockStyle.Fill;
            _cboHatch.Items.AddRange(new object[] { "ANSI31", "ANSI32", "ANSI33", "ANSI34", "AR-BRSTD", "DOTS", "EARTH" });
            _cboHatch.Location = new Point(110, 80);
            _cboHatch.Margin = new Padding(0, 4, 0, 4);
            _cboHatch.Name = "_cboHatch";
            _cboHatch.Size = new Size(209, 28);
            _cboHatch.TabIndex = 5;
            // 
            // _lblTransparency
            // 
            _lblTransparency.Dock = DockStyle.Fill;
            _lblTransparency.Location = new Point(3, 114);
            _lblTransparency.Name = "_lblTransparency";
            _lblTransparency.Size = new Size(104, 46);
            _lblTransparency.TabIndex = 6;
            _lblTransparency.Text = "Transparency";
            _lblTransparency.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _transparencyLayout
            // 
            _transparencyLayout.ColumnCount = 2;
            _transparencyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _transparencyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
            _transparencyLayout.Controls.Add(_trkTransparency, 0, 0);
            _transparencyLayout.Controls.Add(_lblTransparencyValue, 1, 0);
            _transparencyLayout.Dock = DockStyle.Fill;
            _transparencyLayout.Location = new Point(110, 114);
            _transparencyLayout.Margin = new Padding(0);
            _transparencyLayout.Name = "_transparencyLayout";
            _transparencyLayout.RowCount = 1;
            _transparencyLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _transparencyLayout.Size = new Size(209, 46);
            _transparencyLayout.TabIndex = 7;
            // 
            // _trkTransparency
            // 
            _trkTransparency.Dock = DockStyle.Fill;
            _trkTransparency.Location = new Point(3, 3);
            _trkTransparency.Maximum = 100;
            _trkTransparency.Name = "_trkTransparency";
            _trkTransparency.Size = new Size(155, 40);
            _trkTransparency.TabIndex = 0;
            _trkTransparency.TickFrequency = 10;
            _trkTransparency.ValueChanged += trkTransparency_ValueChanged;
            // 
            // _lblTransparencyValue
            // 
            _lblTransparencyValue.Location = new Point(164, 0);
            _lblTransparencyValue.Name = "_lblTransparencyValue";
            _lblTransparencyValue.Size = new Size(30, 43);
            _lblTransparencyValue.TabIndex = 1;
            _lblTransparencyValue.Text = "0%";
            _lblTransparencyValue.TextAlign = ContentAlignment.MiddleRight;
            // 
            // _tabLabel
            // 
            _tabLabel.BackColor = Color.White;
            _tabLabel.Controls.Add(_labelLayout);
            _tabLabel.Location = new Point(4, 31);
            _tabLabel.Name = "_tabLabel";
            _tabLabel.Padding = new Padding(12);
            _tabLabel.Size = new Size(343, 236);
            _tabLabel.TabIndex = 2;
            _tabLabel.Text = "Label";
            // 
            // _labelLayout
            // 
            _labelLayout.AutoScroll = true;
            _labelLayout.ColumnCount = 2;
            _labelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            _labelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _labelLayout.Controls.Add(_lblLabels, 0, 0);
            _labelLayout.Controls.Add(_chkShowLabels, 1, 0);
            _labelLayout.Controls.Add(_lblFont, 0, 1);
            _labelLayout.Controls.Add(_fontPanel, 1, 1);
            _labelLayout.Controls.Add(_lblFontSize, 0, 2);
            _labelLayout.Controls.Add(_numFontSize, 1, 2);
            _labelLayout.Controls.Add(_lblTextColor, 0, 3);
            _labelLayout.Controls.Add(_labelColorPanel, 1, 3);
            _labelLayout.Controls.Add(_lblLabelField, 0, 4);
            _labelLayout.Controls.Add(_cboLabelField, 1, 4);
            _labelLayout.Dock = DockStyle.Fill;
            _labelLayout.Location = new Point(12, 12);
            _labelLayout.Name = "_labelLayout";
            _labelLayout.RowCount = 8;
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            _labelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _labelLayout.Size = new Size(319, 212);
            _labelLayout.TabIndex = 0;
            // 
            // _lblLabels
            // 
            _lblLabels.Dock = DockStyle.Fill;
            _lblLabels.Location = new Point(3, 0);
            _lblLabels.Name = "_lblLabels";
            _lblLabels.Size = new Size(104, 38);
            _lblLabels.TabIndex = 0;
            _lblLabels.Text = "Labels";
            _lblLabels.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _chkShowLabels
            // 
            _chkShowLabels.AutoSize = true;
            _chkShowLabels.Location = new Point(110, 8);
            _chkShowLabels.Margin = new Padding(0, 8, 18, 0);
            _chkShowLabels.Name = "_chkShowLabels";
            _chkShowLabels.Size = new Size(113, 24);
            _chkShowLabels.TabIndex = 1;
            _chkShowLabels.Text = "Show Labels";
            _chkShowLabels.UseVisualStyleBackColor = true;
            // 
            // _lblFont
            // 
            _lblFont.Dock = DockStyle.Fill;
            _lblFont.Location = new Point(3, 38);
            _lblFont.Name = "_lblFont";
            _lblFont.Size = new Size(104, 38);
            _lblFont.TabIndex = 2;
            _lblFont.Text = "Font";
            _lblFont.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fontPanel
            // 
            _fontPanel.ColumnCount = 2;
            _fontPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _fontPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
            _fontPanel.Controls.Add(_txtFontName, 0, 0);
            _fontPanel.Controls.Add(_btnFont, 1, 0);
            _fontPanel.Dock = DockStyle.Fill;
            _fontPanel.Location = new Point(110, 38);
            _fontPanel.Margin = new Padding(0);
            _fontPanel.Name = "_fontPanel";
            _fontPanel.RowCount = 1;
            _fontPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _fontPanel.Size = new Size(209, 38);
            _fontPanel.TabIndex = 3;
            // 
            // _txtFontName
            // 
            _txtFontName.Dock = DockStyle.Fill;
            _txtFontName.Location = new Point(0, 4);
            _txtFontName.Margin = new Padding(0, 4, 4, 4);
            _txtFontName.Name = "_txtFontName";
            _txtFontName.ReadOnly = true;
            _txtFontName.Size = new Size(119, 27);
            _txtFontName.TabIndex = 0;
            // 
            // _btnFont
            // 
            _btnFont.Dock = DockStyle.Fill;
            _btnFont.Location = new Point(123, 3);
            _btnFont.Margin = new Padding(0, 3, 0, 3);
            _btnFont.Name = "_btnFont";
            _btnFont.Size = new Size(86, 32);
            _btnFont.TabIndex = 1;
            _btnFont.Text = "Change...";
            _btnFont.UseVisualStyleBackColor = true;
            _btnFont.Click += btnFont_Click;
            // 
            // _lblFontSize
            // 
            _lblFontSize.Dock = DockStyle.Fill;
            _lblFontSize.Location = new Point(3, 76);
            _lblFontSize.Name = "_lblFontSize";
            _lblFontSize.Size = new Size(104, 38);
            _lblFontSize.TabIndex = 4;
            _lblFontSize.Text = "Font Size";
            _lblFontSize.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _numFontSize
            // 
            _numFontSize.DecimalPlaces = 1;
            _numFontSize.Dock = DockStyle.Left;
            _numFontSize.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            _numFontSize.Location = new Point(110, 80);
            _numFontSize.Margin = new Padding(0, 4, 0, 4);
            _numFontSize.Maximum = new decimal(new int[] { 72, 0, 0, 0 });
            _numFontSize.Minimum = new decimal(new int[] { 4, 0, 0, 0 });
            _numFontSize.Name = "_numFontSize";
            _numFontSize.Size = new Size(110, 27);
            _numFontSize.TabIndex = 5;
            _numFontSize.Value = new decimal(new int[] { 4, 0, 0, 0 });
            // 
            // _lblTextColor
            // 
            _lblTextColor.Dock = DockStyle.Fill;
            _lblTextColor.Location = new Point(3, 114);
            _lblTextColor.Name = "_lblTextColor";
            _lblTextColor.Size = new Size(104, 38);
            _lblTextColor.TabIndex = 6;
            _lblTextColor.Text = "Text Color";
            _lblTextColor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _labelColorPanel
            // 
            _labelColorPanel.Controls.Add(_pnlLabelColor);
            _labelColorPanel.Controls.Add(_btnLabelColor);
            _labelColorPanel.Dock = DockStyle.Fill;
            _labelColorPanel.Location = new Point(110, 114);
            _labelColorPanel.Margin = new Padding(0);
            _labelColorPanel.Name = "_labelColorPanel";
            _labelColorPanel.Size = new Size(209, 38);
            _labelColorPanel.TabIndex = 7;
            _labelColorPanel.WrapContents = false;
            // 
            // _pnlLabelColor
            // 
            _pnlLabelColor.BorderStyle = BorderStyle.FixedSingle;
            _pnlLabelColor.Cursor = Cursors.Hand;
            _pnlLabelColor.Location = new Point(0, 4);
            _pnlLabelColor.Margin = new Padding(0, 4, 8, 0);
            _pnlLabelColor.Name = "_pnlLabelColor";
            _pnlLabelColor.Size = new Size(36, 24);
            _pnlLabelColor.TabIndex = 0;
            _pnlLabelColor.Click += pnlLabelColor_Click;
            // 
            // _btnLabelColor
            // 
            _btnLabelColor.AutoSize = true;
            _btnLabelColor.Location = new Point(47, 3);
            _btnLabelColor.Name = "_btnLabelColor";
            _btnLabelColor.Size = new Size(78, 30);
            _btnLabelColor.TabIndex = 1;
            _btnLabelColor.Text = "Change...";
            _btnLabelColor.UseVisualStyleBackColor = true;
            _btnLabelColor.Click += btnLabelColor_Click;
            // 
            // _lblLabelField
            // 
            _lblLabelField.Dock = DockStyle.Fill;
            _lblLabelField.Location = new Point(3, 152);
            _lblLabelField.Name = "_lblLabelField";
            _lblLabelField.Size = new Size(104, 38);
            _lblLabelField.TabIndex = 8;
            _lblLabelField.Text = "Label Field";
            _lblLabelField.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboLabelField
            // 
            _cboLabelField.Dock = DockStyle.Fill;
            _cboLabelField.Items.AddRange(new object[] { "ParcelNo", "OwnerName", "AreaSqm", "AreaRAPD", "LandUse", "PlotNumber" });
            _cboLabelField.Location = new Point(110, 156);
            _cboLabelField.Margin = new Padding(0, 4, 0, 4);
            _cboLabelField.Name = "_cboLabelField";
            _cboLabelField.Size = new Size(209, 28);
            _cboLabelField.TabIndex = 9;
            // 
            // _footerPanel
            // 
            _footerPanel.Controls.Add(_btnOk);
            _footerPanel.Controls.Add(_btnCancel);
            _footerPanel.Dock = DockStyle.Bottom;
            _footerPanel.FlowDirection = FlowDirection.RightToLeft;
            _footerPanel.Location = new Point(0, 271);
            _footerPanel.Name = "_footerPanel";
            _footerPanel.Padding = new Padding(12, 8, 12, 8);
            _footerPanel.Size = new Size(351, 48);
            _footerPanel.TabIndex = 1;
            _footerPanel.WrapContents = false;
            // 
            // _btnOk
            // 
            _btnOk.AutoSize = true;
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(249, 11);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new Size(75, 30);
            _btnOk.TabIndex = 0;
            _btnOk.Text = "Save";
            _btnOk.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            _btnCancel.AutoSize = true;
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(168, 11);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(75, 30);
            _btnCancel.TabIndex = 1;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // _colorDialog
            // 
            _colorDialog.FullOpen = true;
            // 
            // frmLayerPropertyManager
            // 
            AcceptButton = _btnOk;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CancelButton = _btnCancel;
            ClientSize = new Size(351, 319);
            Controls.Add(_tabs);
            Controls.Add(_footerPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLayerPropertyManager";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Layer Properties";
            _tabs.ResumeLayout(false);
            _tabGeneral.ResumeLayout(false);
            _tabGeneral.PerformLayout();
            _generalLayout.ResumeLayout(false);
            _generalLayout.PerformLayout();
            _borderColorPanel.ResumeLayout(false);
            _borderColorPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numLineWeight).EndInit();
            _statePanel.ResumeLayout(false);
            _statePanel.PerformLayout();
            _tabFill.ResumeLayout(false);
            _fillLayout.ResumeLayout(false);
            _fillColorPanel.ResumeLayout(false);
            _fillColorPanel.PerformLayout();
            _transparencyLayout.ResumeLayout(false);
            _transparencyLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_trkTransparency).EndInit();
            _tabLabel.ResumeLayout(false);
            _labelLayout.ResumeLayout(false);
            _labelLayout.PerformLayout();
            _fontPanel.ResumeLayout(false);
            _fontPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numFontSize).EndInit();
            _labelColorPanel.ResumeLayout(false);
            _labelColorPanel.PerformLayout();
            _footerPanel.ResumeLayout(false);
            _footerPanel.PerformLayout();
            ResumeLayout(false);
        }
    }
}
