namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmLayerPropertyManager
    {
        private System.ComponentModel.IContainer components = null!;

        private TabControl _tabs = null!;
        private TabPage _tabGeneral = null!;
        private TabPage _tabFill = null!;
        private TabPage _tabLabel = null!;
        private TabPage _tabAnnotation = null!;
        private Panel _annPanel = null!;
        private Panel _generalLayout = null!;
        private Panel _fillLayout = null!;
        private Panel _labelLayout = null!;
        private Panel _transparencyLayout = null!;
        private FlowLayoutPanel _lineTypePanel = null!;
        private FlowLayoutPanel _statePanel = null!;
        private FlowLayoutPanel _borderColorPanel = null!;
        private FlowLayoutPanel _fillColorPanel = null!;
        private FlowLayoutPanel _hatchPatternPanel = null!;
        private FlowLayoutPanel _labelColorPanel = null!;
        private FlowLayoutPanel _fontScalingPanel = null!;
        private FlowLayoutPanel _footerPanel = null!;
        private Panel _fontPanel = null!;

        private Label _lblName = null!;
        private Label _lblLayerKind = null!;
        private Label _lblBorderColor = null!;
        private Label _lblLineStyle = null!;
        private Label _lblLineTypeScale = null!;
        private Label _lblLinePreview = null!;
        private Label _lblLineWeight = null!;
        private Label _lblPointMarker = null!;
        private Label _lblState = null!;
        private Label _lblFillStyle = null!;
        private Label _lblFillColor = null!;
        private Label _lblHatchPattern = null!;
        private Label _lblHatchScale = null!;
        private Label _lblTransparency = null!;
        private CheckBox _chkShowFillTransparency = null!;
        private TextBox _txtTransparencyValue = null!;
        private Label _lblLabels = null!;
        private Label _lblFont = null!;
        private Label _lblFontSize = null!;
        private Label _lblTextColor = null!;
        private Label _lblTextAlignment = null!;
        private Label _lblLabelField = null!;
        private Label _lblFontScaling = null!;

        private TextBox _txtName = null!;
        private ComboBox _cboLayerKind = null!;
        private Panel _pnlBorderColor = null!;
        private CheckBox _chkNoBorder = null!;
        private ComboBox _cboLineStyle = null!;
        private NumericUpDown _numLineTypeScale = null!;
        private Panel _pnlLinePreview = null!;
        private NumericUpDown _numLineWeight = null!;
        private FlowLayoutPanel _pointMarkerPanel = null!;
        private Panel _pnlPointMarkerPreview = null!;
        private Button _btnPointMarker = null!;
        private Label _lblPointSize = null!;
        private NumericUpDown _numPointSize = null!;
        private CheckBox _chkVisible = null!;
        private CheckBox _chkLocked = null!;
        private ComboBox _cboFillStyle = null!;
        private Panel _pnlFillColor = null!;
        private Panel _pnlHatchPatternPreview = null!;
        private NumericUpDown _numHatchScale = null!;
        private TrackBar _trkTransparency = null!;
        private CheckBox _chkShowLabels = null!;
        private TextBox _txtFontName = null!;
        private Button _btnFont = null!;
        private NumericUpDown _numFontSize = null!;
        private Panel _pnlLabelColor = null!;
        private ComboBox _cboTextAlignment = null!;
        private ComboBox _cboLabelField = null!;
        private RadioButton _rdoFontFixedSize = null!;
        private RadioButton _rdoFontScalesWithZoom = null!;
        private TextBox _txtLabelFixedText = null!;
        private Label _lblLabelFixedText = null!;
        private FlowLayoutPanel _labelFieldRow = null!;
        private Button _btnLabelExpression = null!;
        private Button _btnBorderColor = null!;
        private Button _btnFillColor = null!;
        private Button _btnHatchPattern = null!;
        private Button _btnLabelColor = null!;
        private Button _btnOk = null!;
        private Button _btnCancel = null!;
        private Button _btnApply = null!;
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
            _generalLayout = new Panel();
            _lblName = new Label();
            _txtName = new TextBox();
            _lblLayerKind = new Label();
            _cboLayerKind = new ComboBox();
            _lblBorderColor = new Label();
            _borderColorPanel = new FlowLayoutPanel();
            _pnlBorderColor = new Panel();
            _btnBorderColor = new Button();
            _chkNoBorder = new CheckBox();
            _lblLineStyle = new Label();
            _lineTypePanel = new FlowLayoutPanel();
            _cboLineStyle = new ComboBox();
            _lblLineTypeScale = new Label();
            _numLineTypeScale = new NumericUpDown();
            _lblLinePreview = new Label();
            _pnlLinePreview = new Panel();
            _lblLineWeight = new Label();
            _numLineWeight = new NumericUpDown();
            _lblPointMarker = new Label();
            _pointMarkerPanel = new FlowLayoutPanel();
            _pnlPointMarkerPreview = new Panel();
            _btnPointMarker = new Button();
            _lblPointSize = new Label();
            _numPointSize = new NumericUpDown();
            _lblState = new Label();
            _statePanel = new FlowLayoutPanel();
            _chkVisible = new CheckBox();
            _chkLocked = new CheckBox();
            _tabFill = new TabPage();
            _fillLayout = new Panel();
            _lblFillStyle = new Label();
            _cboFillStyle = new ComboBox();
            _lblFillColor = new Label();
            _fillColorPanel = new FlowLayoutPanel();
            _pnlFillColor = new Panel();
            _btnFillColor = new Button();
            _lblHatchPattern = new Label();
            _hatchPatternPanel = new FlowLayoutPanel();
            _pnlHatchPatternPreview = new Panel();
            _btnHatchPattern = new Button();
            _lblHatchScale = new Label();
            _numHatchScale = new NumericUpDown();
            _chkShowFillTransparency = new CheckBox();
            _lblTransparency = new Label();
            _transparencyLayout = new Panel();
            _trkTransparency = new TrackBar();
            _txtTransparencyValue = new TextBox();
            _tabLabel = new TabPage();
            _labelLayout = new Panel();
            _lblLabels = new Label();
            _chkShowLabels = new CheckBox();
            _lblFont = new Label();
            _fontPanel = new Panel();
            _txtFontName = new TextBox();
            _btnFont = new Button();
            _lblFontSize = new Label();
            _numFontSize = new NumericUpDown();
            _lblTextColor = new Label();
            _labelColorPanel = new FlowLayoutPanel();
            _pnlLabelColor = new Panel();
            _btnLabelColor = new Button();
            _lblTextAlignment = new Label();
            _cboTextAlignment = new ComboBox();
            _lblLabelField = new Label();
            _cboLabelField = new ComboBox();
            _lblLabelFixedText = new Label();
            _txtLabelFixedText = new TextBox();
            _labelFieldRow = new FlowLayoutPanel();
            _btnLabelExpression = new Button();
            _lblFontScaling = new Label();
            _fontScalingPanel = new FlowLayoutPanel();
            _rdoFontFixedSize = new RadioButton();
            _rdoFontScalesWithZoom = new RadioButton();
            _lblAnnotationText = new Label();
            _txtAnnotationText = new TextBox();
            _footerPanel = new FlowLayoutPanel();
            _btnOk = new Button();
            _btnCancel = new Button();
            _btnApply = new Button();
            _colorDialog = new ColorDialog();
            _fontDialog = new FontDialog();
            _tabAnnotation = new TabPage();
            _annPanel = new Panel();
            _tabs.SuspendLayout();
            _tabGeneral.SuspendLayout();
            _generalLayout.SuspendLayout();
            _borderColorPanel.SuspendLayout();
            _lineTypePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numLineTypeScale).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_numLineWeight).BeginInit();
            _pointMarkerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numPointSize).BeginInit();
            _statePanel.SuspendLayout();
            _tabFill.SuspendLayout();
            _fillLayout.SuspendLayout();
            _fillColorPanel.SuspendLayout();
            _hatchPatternPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numHatchScale).BeginInit();
            _transparencyLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_trkTransparency).BeginInit();
            _tabLabel.SuspendLayout();
            _labelLayout.SuspendLayout();
            _fontPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_numFontSize).BeginInit();
            _labelColorPanel.SuspendLayout();
            _fontScalingPanel.SuspendLayout();
            _labelFieldRow.SuspendLayout();
            _footerPanel.SuspendLayout();
            _tabAnnotation.SuspendLayout();
            SuspendLayout();
            // 
            // _tabs
            // 
            _tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _tabs.Controls.Add(_tabGeneral);
            _tabs.Controls.Add(_tabFill);
            _tabs.Controls.Add(_tabLabel);
            _tabs.Font = new Font("Segoe UI", 9F);
            _tabs.Location = new Point(12, 12);
            _tabs.Name = "_tabs";
            _tabs.Padding = new Point(12, 4);
            _tabs.SelectedIndex = 0;
            _tabs.Size = new Size(520, 544);
            _tabs.TabIndex = 0;
            // 
            // _tabGeneral
            // 
            _tabGeneral.BackColor = Color.White;
            _tabGeneral.Controls.Add(_generalLayout);
            _tabGeneral.Location = new Point(4, 31);
            _tabGeneral.Name = "_tabGeneral";
            _tabGeneral.Padding = new Padding(12);
            _tabGeneral.Size = new Size(512, 581);
            _tabGeneral.TabIndex = 0;
            _tabGeneral.Text = "General";
            // 
            // _generalLayout
            // 
            _generalLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _generalLayout.AutoScroll = true;
            _generalLayout.Controls.Add(_lblName);
            _generalLayout.Controls.Add(_txtName);
            _generalLayout.Controls.Add(_lblLayerKind);
            _generalLayout.Controls.Add(_cboLayerKind);
            _generalLayout.Controls.Add(_lblBorderColor);
            _generalLayout.Controls.Add(_borderColorPanel);
            _generalLayout.Controls.Add(_lblLineStyle);
            _generalLayout.Controls.Add(_lineTypePanel);
            _generalLayout.Controls.Add(_lblLinePreview);
            _generalLayout.Controls.Add(_pnlLinePreview);
            _generalLayout.Controls.Add(_lblLineWeight);
            _generalLayout.Controls.Add(_numLineWeight);
            _generalLayout.Controls.Add(_lblPointMarker);
            _generalLayout.Controls.Add(_pointMarkerPanel);
            _generalLayout.Controls.Add(_lblState);
            _generalLayout.Controls.Add(_statePanel);
            _generalLayout.Location = new Point(12, 12);
            _generalLayout.Name = "_generalLayout";
            _generalLayout.Size = new Size(488, 557);
            _generalLayout.TabIndex = 0;
            // 
            // _lblName
            // 
            _lblName.Location = new Point(12, 10);
            _lblName.Name = "_lblName";
            _lblName.Size = new Size(122, 27);
            _lblName.TabIndex = 0;
            _lblName.Text = "Layer Name";
            _lblName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _txtName
            // 
            _txtName.Location = new Point(144, 10);
            _txtName.Name = "_txtName";
            _txtName.ReadOnly = true;
            _txtName.Size = new Size(332, 27);
            _txtName.TabIndex = 1;
            // 
            // _lblLayerKind
            // 
            _lblLayerKind.Location = new Point(12, 48);
            _lblLayerKind.Name = "_lblLayerKind";
            _lblLayerKind.Size = new Size(122, 27);
            _lblLayerKind.TabIndex = 2;
            _lblLayerKind.Text = "Layer Type";
            _lblLayerKind.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboLayerKind
            // 
            _cboLayerKind.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboLayerKind.Items.AddRange(new object[] { "Point", "Polyline", "Polygon", "Annotation" });
            _cboLayerKind.Location = new Point(144, 48);
            _cboLayerKind.Name = "_cboLayerKind";
            _cboLayerKind.Size = new Size(220, 28);
            _cboLayerKind.TabIndex = 3;
            _cboLayerKind.SelectedIndexChanged += cboLayerKind_SelectedIndexChanged;
            // 
            // _lblBorderColor
            // 
            _lblBorderColor.Location = new Point(12, 86);
            _lblBorderColor.Name = "_lblBorderColor";
            _lblBorderColor.Size = new Size(122, 30);
            _lblBorderColor.TabIndex = 4;
            _lblBorderColor.Text = "Border Color";
            _lblBorderColor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _borderColorPanel
            // 
            _borderColorPanel.Controls.Add(_pnlBorderColor);
            _borderColorPanel.Controls.Add(_btnBorderColor);
            _borderColorPanel.Controls.Add(_chkNoBorder);
            _borderColorPanel.Location = new Point(144, 84);
            _borderColorPanel.Name = "_borderColorPanel";
            _borderColorPanel.Size = new Size(332, 34);
            _borderColorPanel.TabIndex = 5;
            _borderColorPanel.WrapContents = false;
            // 
            // _pnlBorderColor
            // 
            _pnlBorderColor.BorderStyle = BorderStyle.FixedSingle;
            _pnlBorderColor.Cursor = Cursors.Hand;
            _pnlBorderColor.Location = new Point(0, 4);
            _pnlBorderColor.Margin = new Padding(0, 4, 8, 0);
            _pnlBorderColor.Name = "_pnlBorderColor";
            _pnlBorderColor.Size = new Size(36, 26);
            _pnlBorderColor.TabIndex = 0;
            _pnlBorderColor.Click += pnlBorderColor_Click;
            // 
            // _btnBorderColor
            // 
            _btnBorderColor.Location = new Point(47, 3);
            _btnBorderColor.Name = "_btnBorderColor";
            _btnBorderColor.Size = new Size(86, 29);
            _btnBorderColor.TabIndex = 1;
            _btnBorderColor.Text = "Change...";
            _btnBorderColor.UseVisualStyleBackColor = true;
            _btnBorderColor.Click += btnBorderColor_Click;
            // 
            // _chkNoBorder
            // 
            _chkNoBorder.AutoSize = true;
            _chkNoBorder.Location = new Point(139, 3);
            _chkNoBorder.Name = "_chkNoBorder";
            _chkNoBorder.Size = new Size(100, 24);
            _chkNoBorder.TabIndex = 2;
            _chkNoBorder.Text = "No Border";
            _chkNoBorder.UseVisualStyleBackColor = true;
            _chkNoBorder.CheckedChanged += chkNoBorder_CheckedChanged;
            // 
            // _lblLineStyle
            // 
            _lblLineStyle.Location = new Point(12, 124);
            _lblLineStyle.Name = "_lblLineStyle";
            _lblLineStyle.Size = new Size(122, 30);
            _lblLineStyle.TabIndex = 6;
            _lblLineStyle.Text = "Line Type";
            _lblLineStyle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _lineTypePanel
            // 
            _lineTypePanel.Controls.Add(_cboLineStyle);
            _lineTypePanel.Controls.Add(_lblLineTypeScale);
            _lineTypePanel.Controls.Add(_numLineTypeScale);
            _lineTypePanel.Location = new Point(144, 122);
            _lineTypePanel.Name = "_lineTypePanel";
            _lineTypePanel.Size = new Size(332, 34);
            _lineTypePanel.TabIndex = 7;
            _lineTypePanel.WrapContents = false;
            // 
            // _cboLineStyle
            // 
            _cboLineStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboLineStyle.Items.AddRange(new object[] { "Solid", "Dashed", "Dotted", "Centerline", "DashDot", "DashDoubleDot" });
            _cboLineStyle.Location = new Point(0, 3);
            _cboLineStyle.Margin = new Padding(0, 3, 10, 0);
            _cboLineStyle.Name = "_cboLineStyle";
            _cboLineStyle.Size = new Size(132, 28);
            _cboLineStyle.TabIndex = 0;
            _cboLineStyle.SelectedIndexChanged += cboLineStyle_SelectedIndexChanged;
            // 
            // _lblLineTypeScale
            // 
            _lblLineTypeScale.AutoSize = true;
            _lblLineTypeScale.Location = new Point(145, 7);
            _lblLineTypeScale.Margin = new Padding(3, 7, 6, 0);
            _lblLineTypeScale.Name = "_lblLineTypeScale";
            _lblLineTypeScale.Size = new Size(44, 20);
            _lblLineTypeScale.TabIndex = 1;
            _lblLineTypeScale.Text = "Scale";
            // 
            // _numLineTypeScale
            // 
            _numLineTypeScale.DecimalPlaces = 1;
            _numLineTypeScale.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            _numLineTypeScale.Location = new Point(195, 3);
            _numLineTypeScale.Margin = new Padding(0, 3, 0, 0);
            _numLineTypeScale.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            _numLineTypeScale.Name = "_numLineTypeScale";
            _numLineTypeScale.Size = new Size(74, 27);
            _numLineTypeScale.TabIndex = 2;
            _numLineTypeScale.Value = new decimal(new int[] { 10, 0, 0, 65536 });
            _numLineTypeScale.ValueChanged += numLineTypeScale_ValueChanged;
            // 
            // _lblLinePreview
            // 
            _lblLinePreview.Location = new Point(12, 162);
            _lblLinePreview.Name = "_lblLinePreview";
            _lblLinePreview.Size = new Size(122, 30);
            _lblLinePreview.TabIndex = 8;
            _lblLinePreview.Text = "Preview";
            _lblLinePreview.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _pnlLinePreview
            // 
            _pnlLinePreview.BackColor = Color.White;
            _pnlLinePreview.BorderStyle = BorderStyle.FixedSingle;
            _pnlLinePreview.Location = new Point(144, 162);
            _pnlLinePreview.Name = "_pnlLinePreview";
            _pnlLinePreview.Size = new Size(332, 32);
            _pnlLinePreview.TabIndex = 9;
            _pnlLinePreview.Paint += pnlLinePreview_Paint;
            // 
            // _lblLineWeight
            // 
            _lblLineWeight.Location = new Point(12, 202);
            _lblLineWeight.Name = "_lblLineWeight";
            _lblLineWeight.Size = new Size(122, 27);
            _lblLineWeight.TabIndex = 10;
            _lblLineWeight.Text = "Line Weight";
            _lblLineWeight.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _numLineWeight
            // 
            _numLineWeight.DecimalPlaces = 2;
            _numLineWeight.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            _numLineWeight.Location = new Point(144, 202);
            _numLineWeight.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            _numLineWeight.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            _numLineWeight.Name = "_numLineWeight";
            _numLineWeight.Size = new Size(110, 27);
            _numLineWeight.TabIndex = 11;
            _numLineWeight.Value = new decimal(new int[] { 1, 0, 0, 0 });
            _numLineWeight.ValueChanged += numLineWeight_ValueChanged;
            // 
            // _lblPointMarker
            // 
            _lblPointMarker.Location = new Point(12, 240);
            _lblPointMarker.Name = "_lblPointMarker";
            _lblPointMarker.Size = new Size(122, 30);
            _lblPointMarker.TabIndex = 12;
            _lblPointMarker.Text = "Point Marker";
            _lblPointMarker.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _pointMarkerPanel
            // 
            _pointMarkerPanel.Controls.Add(_pnlPointMarkerPreview);
            _pointMarkerPanel.Controls.Add(_btnPointMarker);
            _pointMarkerPanel.Controls.Add(_lblPointSize);
            _pointMarkerPanel.Controls.Add(_numPointSize);
            _pointMarkerPanel.Location = new Point(144, 238);
            _pointMarkerPanel.Name = "_pointMarkerPanel";
            _pointMarkerPanel.Size = new Size(332, 36);
            _pointMarkerPanel.TabIndex = 13;
            _pointMarkerPanel.WrapContents = false;
            // 
            // _pnlPointMarkerPreview
            // 
            _pnlPointMarkerPreview.BackColor = Color.White;
            _pnlPointMarkerPreview.BorderStyle = BorderStyle.FixedSingle;
            _pnlPointMarkerPreview.Location = new Point(0, 3);
            _pnlPointMarkerPreview.Margin = new Padding(0, 3, 8, 0);
            _pnlPointMarkerPreview.Name = "_pnlPointMarkerPreview";
            _pnlPointMarkerPreview.Size = new Size(38, 28);
            _pnlPointMarkerPreview.TabIndex = 0;
            _pnlPointMarkerPreview.Paint += pnlPointMarkerPreview_Paint;
            // 
            // _btnPointMarker
            // 
            _btnPointMarker.Location = new Point(49, 3);
            _btnPointMarker.Name = "_btnPointMarker";
            _btnPointMarker.Size = new Size(86, 29);
            _btnPointMarker.TabIndex = 1;
            _btnPointMarker.Text = "Change...";
            _btnPointMarker.UseVisualStyleBackColor = true;
            _btnPointMarker.Click += btnPointMarker_Click;
            // 
            // _lblPointSize
            // 
            _lblPointSize.AutoSize = true;
            _lblPointSize.Location = new Point(144, 7);
            _lblPointSize.Margin = new Padding(6, 7, 6, 0);
            _lblPointSize.Name = "_lblPointSize";
            _lblPointSize.Size = new Size(36, 20);
            _lblPointSize.TabIndex = 2;
            _lblPointSize.Text = "Size";
            // 
            // _numPointSize
            // 
            _numPointSize.DecimalPlaces = 1;
            _numPointSize.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            _numPointSize.Location = new Point(189, 3);
            _numPointSize.Maximum = new decimal(new int[] { 48, 0, 0, 0 });
            _numPointSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _numPointSize.Name = "_numPointSize";
            _numPointSize.Size = new Size(74, 27);
            _numPointSize.TabIndex = 3;
            _numPointSize.Value = new decimal(new int[] { 5, 0, 0, 0 });
            _numPointSize.ValueChanged += numPointSize_ValueChanged;
            // 
            // _lblState
            // 
            _lblState.Location = new Point(12, 280);
            _lblState.Name = "_lblState";
            _lblState.Size = new Size(122, 30);
            _lblState.TabIndex = 14;
            _lblState.Text = "State";
            _lblState.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _statePanel
            // 
            _statePanel.Controls.Add(_chkVisible);
            _statePanel.Controls.Add(_chkLocked);
            _statePanel.Location = new Point(144, 278);
            _statePanel.Name = "_statePanel";
            _statePanel.Size = new Size(332, 34);
            _statePanel.TabIndex = 15;
            _statePanel.WrapContents = false;
            // 
            // _chkVisible
            // 
            _chkVisible.AutoSize = true;
            _chkVisible.Location = new Point(0, 5);
            _chkVisible.Margin = new Padding(0, 5, 24, 0);
            _chkVisible.Name = "_chkVisible";
            _chkVisible.Size = new Size(75, 24);
            _chkVisible.TabIndex = 0;
            _chkVisible.Text = "Visible";
            _chkVisible.UseVisualStyleBackColor = true;
            // 
            // _chkLocked
            // 
            _chkLocked.AutoSize = true;
            _chkLocked.Location = new Point(99, 5);
            _chkLocked.Margin = new Padding(0, 5, 0, 0);
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
            _tabFill.Size = new Size(512, 581);
            _tabFill.TabIndex = 1;
            _tabFill.Text = "Fill";
            // 
            // _fillLayout
            // 
            _fillLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _fillLayout.AutoScroll = true;
            _fillLayout.Controls.Add(_lblFillStyle);
            _fillLayout.Controls.Add(_cboFillStyle);
            _fillLayout.Controls.Add(_lblFillColor);
            _fillLayout.Controls.Add(_fillColorPanel);
            _fillLayout.Controls.Add(_lblHatchPattern);
            _fillLayout.Controls.Add(_hatchPatternPanel);
            _fillLayout.Controls.Add(_chkShowFillTransparency);
            _fillLayout.Controls.Add(_lblTransparency);
            _fillLayout.Controls.Add(_transparencyLayout);
            _fillLayout.Location = new Point(12, 12);
            _fillLayout.Name = "_fillLayout";
            _fillLayout.Size = new Size(488, 557);
            _fillLayout.TabIndex = 0;
            // 
            // _lblFillStyle
            // 
            _lblFillStyle.Location = new Point(12, 10);
            _lblFillStyle.Name = "_lblFillStyle";
            _lblFillStyle.Size = new Size(122, 27);
            _lblFillStyle.TabIndex = 0;
            _lblFillStyle.Text = "Fill Style";
            _lblFillStyle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboFillStyle
            // 
            _cboFillStyle.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboFillStyle.Items.AddRange(new object[] { "None", "Solid", "Hatched" });
            _cboFillStyle.Location = new Point(144, 10);
            _cboFillStyle.Name = "_cboFillStyle";
            _cboFillStyle.Size = new Size(133, 28);
            _cboFillStyle.TabIndex = 1;
            _cboFillStyle.SelectedIndexChanged += cboFillStyle_SelectedIndexChanged;
            // 
            // _lblFillColor
            // 
            _lblFillColor.Location = new Point(12, 48);
            _lblFillColor.Name = "_lblFillColor";
            _lblFillColor.Size = new Size(122, 30);
            _lblFillColor.TabIndex = 2;
            _lblFillColor.Text = "Fill Color";
            _lblFillColor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fillColorPanel
            // 
            _fillColorPanel.Controls.Add(_pnlFillColor);
            _fillColorPanel.Controls.Add(_btnFillColor);
            _fillColorPanel.Location = new Point(144, 46);
            _fillColorPanel.Name = "_fillColorPanel";
            _fillColorPanel.Size = new Size(332, 34);
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
            _pnlFillColor.Size = new Size(36, 26);
            _pnlFillColor.TabIndex = 0;
            _pnlFillColor.Click += pnlFillColor_Click;
            // 
            // _btnFillColor
            // 
            _btnFillColor.Location = new Point(47, 3);
            _btnFillColor.Name = "_btnFillColor";
            _btnFillColor.Size = new Size(86, 29);
            _btnFillColor.TabIndex = 1;
            _btnFillColor.Text = "Change...";
            _btnFillColor.UseVisualStyleBackColor = true;
            _btnFillColor.Click += btnFillColor_Click;
            // 
            // _lblHatchPattern
            // 
            _lblHatchPattern.Location = new Point(12, 86);
            _lblHatchPattern.Name = "_lblHatchPattern";
            _lblHatchPattern.Size = new Size(122, 30);
            _lblHatchPattern.TabIndex = 4;
            _lblHatchPattern.Text = "Hatch Pattern";
            _lblHatchPattern.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _hatchPatternPanel
            // 
            _hatchPatternPanel.Controls.Add(_pnlHatchPatternPreview);
            _hatchPatternPanel.Controls.Add(_btnHatchPattern);
            _hatchPatternPanel.Controls.Add(_lblHatchScale);
            _hatchPatternPanel.Controls.Add(_numHatchScale);
            _hatchPatternPanel.Location = new Point(144, 84);
            _hatchPatternPanel.Name = "_hatchPatternPanel";
            _hatchPatternPanel.Size = new Size(332, 36);
            _hatchPatternPanel.TabIndex = 5;
            _hatchPatternPanel.WrapContents = false;
            // 
            // _pnlHatchPatternPreview
            // 
            _pnlHatchPatternPreview.BackColor = Color.White;
            _pnlHatchPatternPreview.BorderStyle = BorderStyle.FixedSingle;
            _pnlHatchPatternPreview.Location = new Point(0, 3);
            _pnlHatchPatternPreview.Margin = new Padding(0, 3, 8, 0);
            _pnlHatchPatternPreview.Name = "_pnlHatchPatternPreview";
            _pnlHatchPatternPreview.Size = new Size(38, 28);
            _pnlHatchPatternPreview.TabIndex = 0;
            _pnlHatchPatternPreview.Paint += pnlHatchPatternPreview_Paint;
            // 
            // _btnHatchPattern
            // 
            _btnHatchPattern.Location = new Point(49, 3);
            _btnHatchPattern.Name = "_btnHatchPattern";
            _btnHatchPattern.Size = new Size(86, 29);
            _btnHatchPattern.TabIndex = 1;
            _btnHatchPattern.Text = "Choose...";
            _btnHatchPattern.UseVisualStyleBackColor = true;
            _btnHatchPattern.Click += btnHatchPattern_Click;
            // 
            // _lblHatchScale
            // 
            _lblHatchScale.AutoSize = true;
            _lblHatchScale.Location = new Point(144, 7);
            _lblHatchScale.Margin = new Padding(6, 7, 6, 0);
            _lblHatchScale.Name = "_lblHatchScale";
            _lblHatchScale.Size = new Size(44, 20);
            _lblHatchScale.TabIndex = 2;
            _lblHatchScale.Text = "Scale";
            // 
            // _numHatchScale
            // 
            _numHatchScale.DecimalPlaces = 1;
            _numHatchScale.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            _numHatchScale.Location = new Point(197, 3);
            _numHatchScale.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            _numHatchScale.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            _numHatchScale.Name = "_numHatchScale";
            _numHatchScale.Size = new Size(74, 27);
            _numHatchScale.TabIndex = 3;
            _numHatchScale.Value = new decimal(new int[] { 10, 0, 0, 65536 });
            // 
            // _chkShowFillTransparency
            // 
            _chkShowFillTransparency.AutoSize = true;
            _chkShowFillTransparency.Location = new Point(144, 128);
            _chkShowFillTransparency.Name = "_chkShowFillTransparency";
            _chkShowFillTransparency.Size = new Size(219, 24);
            _chkShowFillTransparency.TabIndex = 6;
            _chkShowFillTransparency.Text = "Use fill transparency on map";
            _chkShowFillTransparency.UseVisualStyleBackColor = true;
            _chkShowFillTransparency.CheckedChanged += chkShowFillTransparency_CheckedChanged;
            // 
            // _lblTransparency
            // 
            _lblTransparency.Location = new Point(12, 164);
            _lblTransparency.Name = "_lblTransparency";
            _lblTransparency.Size = new Size(122, 30);
            _lblTransparency.TabIndex = 7;
            _lblTransparency.Text = "Transparency";
            _lblTransparency.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _transparencyLayout
            // 
            _transparencyLayout.Controls.Add(_trkTransparency);
            _transparencyLayout.Controls.Add(_txtTransparencyValue);
            _transparencyLayout.Location = new Point(144, 160);
            _transparencyLayout.Name = "_transparencyLayout";
            _transparencyLayout.Size = new Size(332, 40);
            _transparencyLayout.TabIndex = 8;
            // 
            // _trkTransparency
            // 
            _trkTransparency.AutoSize = false;
            _trkTransparency.Location = new Point(0, 2);
            _trkTransparency.Maximum = 100;
            _trkTransparency.Name = "_trkTransparency";
            _trkTransparency.Size = new Size(219, 34);
            _trkTransparency.TabIndex = 0;
            _trkTransparency.TickFrequency = 10;
            _trkTransparency.ValueChanged += trkTransparency_ValueChanged;
            // 
            // _txtTransparencyValue
            // 
            _txtTransparencyValue.Location = new Point(225, 7);
            _txtTransparencyValue.Name = "_txtTransparencyValue";
            _txtTransparencyValue.ReadOnly = true;
            _txtTransparencyValue.Size = new Size(46, 27);
            _txtTransparencyValue.TabIndex = 1;
            _txtTransparencyValue.TextAlign = HorizontalAlignment.Center;
            // 
            // _tabLabel
            // 
            _tabLabel.BackColor = Color.White;
            _tabLabel.Controls.Add(_labelLayout);
            _tabLabel.Location = new Point(4, 31);
            _tabLabel.Name = "_tabLabel";
            _tabLabel.Padding = new Padding(12);
            _tabLabel.Size = new Size(512, 581);
            _tabLabel.TabIndex = 2;
            _tabLabel.Text = "Label";
            // 
            // _labelLayout
            // 
            _labelLayout.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _labelLayout.AutoScroll = true;
            _labelLayout.Controls.Add(_lblLabels);
            _labelLayout.Controls.Add(_chkShowLabels);
            _labelLayout.Controls.Add(_lblFont);
            _labelLayout.Controls.Add(_fontPanel);
            _labelLayout.Controls.Add(_lblFontSize);
            _labelLayout.Controls.Add(_numFontSize);
            _labelLayout.Controls.Add(_lblTextColor);
            _labelLayout.Controls.Add(_labelColorPanel);
            _labelLayout.Controls.Add(_lblTextAlignment);
            _labelLayout.Controls.Add(_cboTextAlignment);
            _labelLayout.Controls.Add(_lblLabelField);
            _labelLayout.Controls.Add(_labelFieldRow);
            _labelLayout.Controls.Add(_lblLabelFixedText);
            _labelLayout.Controls.Add(_txtLabelFixedText);
            _labelLayout.Controls.Add(_lblFontScaling);
            _labelLayout.Controls.Add(_fontScalingPanel);
            _labelLayout.Controls.Add(_lblAnnotationText);
            _labelLayout.Controls.Add(_txtAnnotationText);
            _labelLayout.Location = new Point(12, 12);
            _labelLayout.Name = "_labelLayout";
            _labelLayout.Size = new Size(488, 557);
            _labelLayout.TabIndex = 0;
            // 
            // _lblLabels
            // 
            _lblLabels.Location = new Point(12, 10);
            _lblLabels.Name = "_lblLabels";
            _lblLabels.Size = new Size(122, 27);
            _lblLabels.TabIndex = 0;
            _lblLabels.Text = "Labels";
            _lblLabels.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _chkShowLabels
            // 
            _chkShowLabels.AutoSize = true;
            _chkShowLabels.Location = new Point(144, 12);
            _chkShowLabels.Name = "_chkShowLabels";
            _chkShowLabels.Size = new Size(113, 24);
            _chkShowLabels.TabIndex = 1;
            _chkShowLabels.Text = "Show Labels";
            _chkShowLabels.UseVisualStyleBackColor = true;
            // 
            // _lblFont
            // 
            _lblFont.Location = new Point(12, 48);
            _lblFont.Name = "_lblFont";
            _lblFont.Size = new Size(122, 27);
            _lblFont.TabIndex = 2;
            _lblFont.Text = "Font";
            _lblFont.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fontPanel
            // 
            _fontPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _fontPanel.Controls.Add(_txtFontName);
            _fontPanel.Controls.Add(_btnFont);
            _fontPanel.Location = new Point(144, 46);
            _fontPanel.Name = "_fontPanel";
            _fontPanel.Size = new Size(333, 34);
            _fontPanel.TabIndex = 3;
            // 
            // _txtFontName
            // 
            _txtFontName.Location = new Point(0, 3);
            _txtFontName.Name = "_txtFontName";
            _txtFontName.ReadOnly = true;
            _txtFontName.Size = new Size(230, 27);
            _txtFontName.TabIndex = 0;
            // 
            // _btnFont
            // 
            _btnFont.Location = new Point(240, 2);
            _btnFont.Name = "_btnFont";
            _btnFont.Size = new Size(86, 29);
            _btnFont.TabIndex = 1;
            _btnFont.Text = "Change...";
            _btnFont.UseVisualStyleBackColor = true;
            _btnFont.Click += btnFont_Click;
            // 
            // _lblFontSize
            // 
            _lblFontSize.Location = new Point(12, 88);
            _lblFontSize.Name = "_lblFontSize";
            _lblFontSize.Size = new Size(122, 27);
            _lblFontSize.TabIndex = 4;
            _lblFontSize.Text = "Font Size";
            _lblFontSize.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _numFontSize
            // 
            _numFontSize.DecimalPlaces = 1;
            _numFontSize.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            _numFontSize.Location = new Point(144, 88);
            _numFontSize.Maximum = new decimal(new int[] { 120, 0, 0, 0 });
            _numFontSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _numFontSize.Name = "_numFontSize";
            _numFontSize.Size = new Size(110, 27);
            _numFontSize.TabIndex = 5;
            _numFontSize.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // _lblTextColor
            // 
            _lblTextColor.Location = new Point(12, 126);
            _lblTextColor.Name = "_lblTextColor";
            _lblTextColor.Size = new Size(122, 30);
            _lblTextColor.TabIndex = 6;
            _lblTextColor.Text = "Text Color";
            _lblTextColor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _labelColorPanel
            // 
            _labelColorPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _labelColorPanel.Controls.Add(_pnlLabelColor);
            _labelColorPanel.Controls.Add(_btnLabelColor);
            _labelColorPanel.Location = new Point(144, 124);
            _labelColorPanel.Name = "_labelColorPanel";
            _labelColorPanel.Size = new Size(333, 34);
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
            _pnlLabelColor.Size = new Size(36, 26);
            _pnlLabelColor.TabIndex = 0;
            _pnlLabelColor.Click += pnlLabelColor_Click;
            // 
            // _btnLabelColor
            // 
            _btnLabelColor.Location = new Point(47, 3);
            _btnLabelColor.Name = "_btnLabelColor";
            _btnLabelColor.Size = new Size(86, 29);
            _btnLabelColor.TabIndex = 1;
            _btnLabelColor.Text = "Change...";
            _btnLabelColor.UseVisualStyleBackColor = true;
            _btnLabelColor.Click += btnLabelColor_Click;
            // 
            // _lblTextAlignment
            // 
            _lblTextAlignment.Location = new Point(12, 164);
            _lblTextAlignment.Name = "_lblTextAlignment";
            _lblTextAlignment.Size = new Size(122, 27);
            _lblTextAlignment.TabIndex = 8;
            _lblTextAlignment.Text = "Alignment";
            _lblTextAlignment.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboTextAlignment
            // 
            _cboTextAlignment.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboTextAlignment.Items.AddRange(new object[] {
                "Left Top", "Center Top", "Right Top",
                "Left Middle", "Center Middle", "Right Middle",
                "Left Bottom", "Center Bottom", "Right Bottom"
            });
            _cboTextAlignment.Location = new Point(144, 164);
            _cboTextAlignment.Name = "_cboTextAlignment";
            _cboTextAlignment.Size = new Size(160, 28);
            _cboTextAlignment.TabIndex = 9;
            // 
            // _lblLabelField
            // 
            _lblLabelField.Location = new Point(12, 198);
            _lblLabelField.Name = "_lblLabelField";
            _lblLabelField.Size = new Size(122, 27);
            _lblLabelField.TabIndex = 12;
            _lblLabelField.Text = "Label Field";
            _lblLabelField.TextAlign = ContentAlignment.MiddleLeft;
            //
            // _cboLabelField  (lives inside _labelFieldRow)
            //
            _cboLabelField.Location = new Point(0, 3);
            _cboLabelField.Margin = new Padding(0, 3, 6, 0);
            _cboLabelField.Name = "_cboLabelField";
            _cboLabelField.Size = new Size(214, 28);
            _cboLabelField.TabIndex = 0;
            //
            // _labelFieldRow
            //
            _labelFieldRow.Controls.Add(_cboLabelField);
            _labelFieldRow.Controls.Add(_btnLabelExpression);
            _labelFieldRow.Location = new Point(144, 198);
            _labelFieldRow.Name = "_labelFieldRow";
            _labelFieldRow.Size = new Size(333, 34);
            _labelFieldRow.TabIndex = 13;
            _labelFieldRow.WrapContents = false;
            //
            // _btnLabelExpression
            //
            _btnLabelExpression.Location = new Point(220, 3);
            _btnLabelExpression.Margin = new Padding(0, 3, 0, 0);
            _btnLabelExpression.Name = "_btnLabelExpression";
            _btnLabelExpression.Size = new Size(113, 29);
            _btnLabelExpression.TabIndex = 1;
            _btnLabelExpression.Text = "Expression...";
            _btnLabelExpression.UseVisualStyleBackColor = true;
            _btnLabelExpression.Click += btnLabelExpression_Click;
            //
            // _lblLabelFixedText
            // 
            _lblLabelFixedText.Location = new Point(12, 199);
            _lblLabelFixedText.Name = "_lblLabelFixedText";
            _lblLabelFixedText.Size = new Size(122, 27);
            _lblLabelFixedText.TabIndex = 14;
            _lblLabelFixedText.Text = "Fixed text";
            _lblLabelFixedText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _txtLabelFixedText
            // 
            _txtLabelFixedText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtLabelFixedText.Location = new Point(144, 199);
            _txtLabelFixedText.Name = "_txtLabelFixedText";
            _txtLabelFixedText.PlaceholderText = "Text shown on every object";
            _txtLabelFixedText.Size = new Size(333, 27);
            _txtLabelFixedText.TabIndex = 15;
            // 
            // _lblFontScaling
            // 
            _lblFontScaling.Location = new Point(12, 234);
            _lblFontScaling.Name = "_lblFontScaling";
            _lblFontScaling.Size = new Size(122, 30);
            _lblFontScaling.TabIndex = 16;
            _lblFontScaling.Text = "Font Scaling";
            _lblFontScaling.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fontScalingPanel
            // 
            _fontScalingPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _fontScalingPanel.Controls.Add(_rdoFontFixedSize);
            _fontScalingPanel.Controls.Add(_rdoFontScalesWithZoom);
            _fontScalingPanel.Location = new Point(144, 232);
            _fontScalingPanel.Name = "_fontScalingPanel";
            _fontScalingPanel.Size = new Size(333, 34);
            _fontScalingPanel.TabIndex = 17;
            _fontScalingPanel.WrapContents = false;
            // 
            // _rdoFontFixedSize
            // 
            _rdoFontFixedSize.AutoSize = true;
            _rdoFontFixedSize.Location = new Point(0, 5);
            _rdoFontFixedSize.Margin = new Padding(0, 5, 18, 0);
            _rdoFontFixedSize.Name = "_rdoFontFixedSize";
            _rdoFontFixedSize.Size = new Size(120, 24);
            _rdoFontFixedSize.TabIndex = 0;
            _rdoFontFixedSize.Text = "Fixed on view";
            _rdoFontFixedSize.UseVisualStyleBackColor = true;
            // 
            // _rdoFontScalesWithZoom
            // 
            _rdoFontScalesWithZoom.AutoSize = true;
            _rdoFontScalesWithZoom.Checked = true;
            _rdoFontScalesWithZoom.Location = new Point(138, 5);
            _rdoFontScalesWithZoom.Margin = new Padding(0, 5, 0, 0);
            _rdoFontScalesWithZoom.Name = "_rdoFontScalesWithZoom";
            _rdoFontScalesWithZoom.Size = new Size(139, 24);
            _rdoFontScalesWithZoom.TabIndex = 1;
            _rdoFontScalesWithZoom.TabStop = true;
            _rdoFontScalesWithZoom.Text = "Scale with zoom";
            _rdoFontScalesWithZoom.UseVisualStyleBackColor = true;
            // 
            // _lblAnnotationText
            // 
            _lblAnnotationText.Location = new Point(12, 324);
            _lblAnnotationText.Name = "_lblAnnotationText";
            _lblAnnotationText.Size = new Size(122, 27);
            _lblAnnotationText.TabIndex = 18;
            _lblAnnotationText.Text = "Default text";
            _lblAnnotationText.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _txtAnnotationText
            // 
            _txtAnnotationText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtAnnotationText.Location = new Point(144, 324);
            _txtAnnotationText.Name = "_txtAnnotationText";
            _txtAnnotationText.PlaceholderText = "Text placed when clicking (optional)";
            _txtAnnotationText.Size = new Size(333, 27);
            _txtAnnotationText.TabIndex = 19;
            // 
            // _footerPanel
            // 
            _footerPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _footerPanel.Controls.Add(_btnOk);
            _footerPanel.Controls.Add(_btnCancel);
            _footerPanel.Controls.Add(_btnApply);
            _footerPanel.FlowDirection = FlowDirection.RightToLeft;
            _footerPanel.Location = new Point(12, 568);
            _footerPanel.Name = "_footerPanel";
            _footerPanel.Size = new Size(520, 40);
            _footerPanel.TabIndex = 1;
            _footerPanel.WrapContents = false;
            // 
            // _btnOk
            // 
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Location = new Point(432, 4);
            _btnOk.Margin = new Padding(8, 4, 0, 0);
            _btnOk.Name = "_btnOk";
            _btnOk.Size = new Size(88, 30);
            _btnOk.TabIndex = 0;
            _btnOk.Text = "Save";
            _btnOk.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(336, 4);
            _btnCancel.Margin = new Padding(8, 4, 0, 0);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(88, 30);
            _btnCancel.TabIndex = 1;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            //
            // _btnApply
            //
            _btnApply.Margin = new Padding(8, 4, 0, 0);
            _btnApply.Name = "_btnApply";
            _btnApply.Size = new Size(88, 30);
            _btnApply.TabIndex = 2;
            _btnApply.Text = "Apply";
            _btnApply.UseVisualStyleBackColor = true;
            _btnApply.Click += btnApply_Click;
            //
            // _colorDialog
            // 
            _colorDialog.FullOpen = true;
            // 
            // _tabAnnotation
            // 
            _tabAnnotation.BackColor = Color.White;
            _tabAnnotation.Controls.Add(_annPanel);
            _tabAnnotation.Location = new Point(4, 31);
            _tabAnnotation.Name = "_tabAnnotation";
            _tabAnnotation.Padding = new Padding(12);
            _tabAnnotation.Size = new Size(512, 581);
            _tabAnnotation.TabIndex = 3;
            _tabAnnotation.Text = "Annotation";
            // 
            // _annPanel
            // 
            _annPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _annPanel.AutoScroll = true;
            _annPanel.Location = new Point(12, 12);
            _annPanel.Name = "_annPanel";
            _annPanel.Size = new Size(488, 557);
            _annPanel.TabIndex = 0;
            // 
            // frmLayerPropertyManager
            // 
            AcceptButton = _btnOk;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            CancelButton = _btnCancel;
            ClientSize = new Size(544, 620);
            MinimumSize = new Size(560, 659);
            Controls.Add(_footerPanel);
            Controls.Add(_tabs);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLayerPropertyManager";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Layer Properties";
            Load += frmLayerPropertyManager_Load;
            _tabs.ResumeLayout(false);
            _tabGeneral.ResumeLayout(false);
            _generalLayout.ResumeLayout(false);
            _generalLayout.PerformLayout();
            _borderColorPanel.ResumeLayout(false);
            _borderColorPanel.PerformLayout();
            _lineTypePanel.ResumeLayout(false);
            _lineTypePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numLineTypeScale).EndInit();
            ((System.ComponentModel.ISupportInitialize)_numLineWeight).EndInit();
            _pointMarkerPanel.ResumeLayout(false);
            _pointMarkerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numPointSize).EndInit();
            _statePanel.ResumeLayout(false);
            _statePanel.PerformLayout();
            _tabFill.ResumeLayout(false);
            _fillLayout.ResumeLayout(false);
            _fillLayout.PerformLayout();
            _fillColorPanel.ResumeLayout(false);
            _hatchPatternPanel.ResumeLayout(false);
            _hatchPatternPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_numHatchScale).EndInit();
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
            _labelFieldRow.ResumeLayout(false);
            _fontScalingPanel.ResumeLayout(false);
            _fontScalingPanel.PerformLayout();
            _footerPanel.ResumeLayout(false);
            _tabAnnotation.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Label _lblAnnotationText;
        private TextBox _txtAnnotationText;
    }
}
