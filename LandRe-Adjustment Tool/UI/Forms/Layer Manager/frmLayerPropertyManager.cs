using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.Helpers;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmLayerPropertyManager : Form
    {
        private const string DefaultCanvasLabelFontName = "Nirmala UI";
        private const decimal DefaultFixedLabelFontSize = 6.0m;
        private const decimal DefaultScaledLabelFontSize = 2.0m;
        private const decimal DefaultAnnotationLabelFontSize = 10.0m;
        private const decimal MinLabelFontSize = 1.0m;
        private const decimal MaxFixedLabelFontSize = 72.0m;
        private const decimal MaxScaledLabelFontSize = 120.0m;
        private const int PropertyLabelLeft = 12;
        private const int PropertyValueLeft = 144;
        private const int PropertyLabelWidth = 122;
        private const int PropertyValueWidth = 332;
        private const int PropertyRowHeight = 38;

        public CanvasLayer Layer { get; }
        private readonly IHatchPatternService _hatchPatternService;
        private readonly bool _isRasterLayer;
        private readonly bool _isLineLayer;
        private readonly bool _isDrawingMarkupLayer;
        private readonly bool _isExternalLayer;
        private readonly bool _allowRename;
        private readonly bool _allowLayerKindChange;
        private readonly Func<string, string>? _layerNameSuggestionProvider;
        private string _selectedHatchPatternKey = string.Empty;
        private string _selectedPointMarkerKey = "Dot";
        private string _lastSelectedLayerKind = string.Empty;
        private string _lastSuggestedLayerName = string.Empty;
        private bool _isLoadingLayer;
        private bool _suppressLayerNameTextChanged;
        private bool _layerNameEditedByUser;
        private IReadOnlyList<IReadOnlyDictionary<string, string>>? _sampleRecords;

        public frmLayerPropertyManager(CanvasLayer layer)
            : this(layer, new HatchPatternService())
        {
        }

        public frmLayerPropertyManager(
            CanvasLayer layer,
            IHatchPatternService hatchPatternService)
            : this(layer, hatchPatternService, allowRename: false)
        {
        }

        public frmLayerPropertyManager(
            CanvasLayer layer,
            IHatchPatternService hatchPatternService,
            bool allowRename)
            : this(
                layer,
                hatchPatternService,
                allowRename,
                allowLayerKindChange: allowRename)
        {
        }

        public frmLayerPropertyManager(
            CanvasLayer layer,
            IHatchPatternService hatchPatternService,
            bool allowRename,
            bool allowLayerKindChange,
            Func<string, string>? layerNameSuggestionProvider = null)
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));
            _hatchPatternService = hatchPatternService
                ?? throw new ArgumentNullException(nameof(hatchPatternService));
            _isRasterLayer = IsRasterLayer(layer);
            _isLineLayer = CanvasLayerTreeService.IsLineLayer(layer);
            _isDrawingMarkupLayer = CanvasLayerTreeService.IsDrawingMarkupLayer(layer);
            _isExternalLayer = CanvasLayerTreeService.IsExternalImportedLayer(layer);
            _allowRename = allowRename;
            _allowLayerKindChange = allowLayerKindChange;
            _layerNameSuggestionProvider = layerNameSuggestionProvider;

            InitializeComponent();
            NumericUpDownSelectAllBehavior.AttachTo(this);
            LoadLayer();
            _lastSelectedLayerKind = NormalizeDrawingLayerType(_cboLayerKind.Text);
            _lastSuggestedLayerName = _txtName.Text.Trim();
            _txtName.TextChanged += txtName_TextChanged;
            ConfigureApplicableControls();
            _rdoFontFixedSize.CheckedChanged += FontScalingRadio_CheckedChanged;
            _rdoFontScalesWithZoom.CheckedChanged += FontScalingRadio_CheckedChanged;
            _chkLocked.CheckedChanged += (_, _) => ConfigureLockedControlState();
            _tabs.SelectedIndexChanged += (_, _) => ArrangeAllPropertyPanels();
            ConfigureLockedControlState();
            ArrangeAllPropertyPanels();
        }

        public frmLayerPropertyManager(
            CanvasLayer layer,
            IHatchPatternService hatchPatternService,
            IReadOnlyList<IReadOnlyDictionary<string, string>>? sampleRecords)
            : this(layer, hatchPatternService, sampleRecords, allowRename: false)
        {
        }

        public frmLayerPropertyManager(
            CanvasLayer layer,
            IHatchPatternService hatchPatternService,
            IReadOnlyList<IReadOnlyDictionary<string, string>>? sampleRecords,
            bool allowRename)
            : this(
                layer,
                hatchPatternService,
                sampleRecords,
                allowRename,
                allowLayerKindChange: false)
        {
        }

        public frmLayerPropertyManager(
            CanvasLayer layer,
            IHatchPatternService hatchPatternService,
            IReadOnlyList<IReadOnlyDictionary<string, string>>? sampleRecords,
            bool allowRename,
            bool allowLayerKindChange,
            Func<string, string>? layerNameSuggestionProvider = null)
            : this(
                layer,
                hatchPatternService,
                allowRename,
                allowLayerKindChange,
                layerNameSuggestionProvider)
        {
            _sampleRecords = sampleRecords;
            PopulateLabelFieldOptions(_cboLabelField.Text.Trim());
        }

        /// <summary>
        /// Loads persisted layer values into the editable property controls.
        /// </summary>
        private void LoadLayer()
        {
            _isLoadingLayer = true;
            _txtName.Text = Layer.Name;
            SetComboText(_cboLayerKind, GetLayerKindText(Layer));
            _pnlBorderColor.BackColor = ParseColorOrDefault(Layer.BorderColor, Color.Black);
            SetComboText(_cboLineStyle, Layer.LineStyle);
            _numLineTypeScale.Value = ClampDecimal((decimal)NormalizeLineTypeScale(Layer.LineTypeScale), _numLineTypeScale);
            _chkNoBorder.Checked = Layer.LineWeight <= 0;
            _numLineWeight.Value = ClampDecimal((decimal)Math.Max(Layer.LineWeight, 0.01), _numLineWeight);
            _numPointSize.Value = ClampDecimal((decimal)Math.Clamp(Layer.PointSize, 1.0, 48.0), _numPointSize);
            _chkVisible.Checked = Layer.IsVisible;
            _chkLocked.Checked = Layer.IsLocked;
            _selectedPointMarkerKey = PointMarkerRenderer.Normalize(Layer.PointSymbol);

            SetComboText(_cboFillStyle, string.IsNullOrWhiteSpace(Layer.FillStyle) ? "None" : Layer.FillStyle);
            _pnlFillColor.BackColor = ParseColorOrDefault(Layer.FillColor, Color.White);
            _selectedHatchPatternKey = _hatchPatternService
                .GetPatternOrDefault(Layer.HatchPattern)
                .Key;
            _numHatchScale.Value = ClampDecimal((decimal)NormalizeHatchScale(Layer.HatchScale), _numHatchScale);
            int fillTransparency = Math.Clamp(Layer.FillTransparency, 0, 100);
            if (!_isRasterLayer && fillTransparency == 0 && !Layer.ShowFillTransparency)
                fillTransparency = 50;

            _trkTransparency.Value = fillTransparency;
            _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
            _chkShowFillTransparency.Checked = !_isRasterLayer && Layer.ShowFillTransparency;
            UpdateFillControlState();
            UpdateLayerKindPresentation();

            _chkShowLabels.Checked = Layer.ShowLabels;
            _txtFontName.Text = string.IsNullOrWhiteSpace(Layer.LabelFontName)
                ? DefaultCanvasLabelFontName
                : Layer.LabelFontName;
            ConfigureLabelFontSizeControl(Layer.LabelScaleWithZoom, resetToDefault: false);
            decimal labelFontSize = Layer.LabelFontSize <= 0
                ? GetDefaultLabelFontSize(Layer.LabelScaleWithZoom)
                : (decimal)Layer.LabelFontSize;
            _numFontSize.Value = ClampDecimal(labelFontSize, _numFontSize);
            _pnlLabelColor.BackColor = ParseColorOrDefault(Layer.LabelColor, Color.Black);
            SetComboText(_cboTextAlignment, NormalizeTextAlignment(Layer.TextAlignment));
            _rdoFontScalesWithZoom.Checked = Layer.LabelScaleWithZoom;
            _rdoFontFixedSize.Checked = !Layer.LabelScaleWithZoom;

            // Label source: "static:text" = fixed text, anything else = from object data.
            string? labelField = Layer.LabelField;
            PopulateLabelFieldOptions(labelField);

            // Pre-fill the parcel number expression for BaselineParcel layers that have no label set yet.
            if (string.IsNullOrEmpty(labelField) &&
                string.Equals(Layer.LayerType, "BaselineParcel", StringComparison.OrdinalIgnoreCase))
            {
                labelField = "{ParcelNo}";
            }

            if (!string.IsNullOrEmpty(labelField) &&
                labelField.StartsWith("static:", StringComparison.OrdinalIgnoreCase))
            {
                _txtLabelFixedText.Text = labelField["static:".Length..];
                SetComboText(_cboLabelField, string.Empty);
            }
            else
            {
                SetComboText(_cboLabelField, labelField ?? string.Empty);
            }

            // Annotation default text.
            string? annotationStaticField = Layer.LabelField;
            if (!string.IsNullOrEmpty(annotationStaticField) &&
                annotationStaticField.StartsWith("static:", StringComparison.OrdinalIgnoreCase))
            {
                _txtAnnotationText.Text = annotationStaticField["static:".Length..];
            }

            UpdateLabelSourceControlVisibility();
            _isLoadingLayer = false;
        }

        /// <summary>
        /// Shows only the property groups that are meaningful for the selected layer type.
        /// </summary>
        private void ConfigureApplicableControls()
        {
            if (_isExternalLayer && _tabs.TabPages.Contains(_tabLabel))
            {
                _tabs.TabPages.Remove(_tabLabel);
            }

            if (_isDrawingMarkupLayer)
            {
                UpdateLayerKindPresentation();
                return;
            }

            if (_isLineLayer && !_isDrawingMarkupLayer)
            {
                Text = "Line Layer Properties";
                _lblBorderColor.Text = "Line Color";
                _tabs.TabPages.Remove(_tabFill);
                return;
            }

            if (!_isRasterLayer)
            {
                Text = "Polygon Layer Properties";
                _lblBorderColor.Text = "Border Color";
                return;
            }

            if (!_isRasterLayer)
                return;

            Text = "Raster Layer Properties";
            _tabs.TabPages.Remove(_tabFill);
            _tabs.TabPages.Remove(_tabLabel);

            SetControlVisible(_lblLayerKind, true);
            SetControlVisible(_cboLayerKind, true);
            SetControlVisible(_lblBorderColor, false);
            SetControlVisible(_borderColorPanel, false);
            SetControlVisible(_lblPointMarker, false);
            SetControlVisible(_pointMarkerPanel, false);
            SetControlVisible(_lblLineStyle, false);
            SetControlVisible(_lineTypePanel, false);
            SetControlVisible(_lblLinePreview, false);
            SetControlVisible(_pnlLinePreview, false);
            SetControlVisible(_lblLineWeight, false);
            SetControlVisible(_numLineWeight, false);

            SetControlVisible(_lblFillStyle, false);
            SetControlVisible(_cboFillStyle, false);
            SetControlVisible(_lblFillColor, false);
            SetControlVisible(_fillColorPanel, false);
            SetControlVisible(_lblHatchPattern, false);
            SetControlVisible(_hatchPatternPanel, false);
            SetControlVisible(_chkShowFillTransparency, false);
            ArrangeAllPropertyPanels();
        }

        private void pnlBorderColor_Click(object? sender, EventArgs e)
        {
            PickColor(_pnlBorderColor);
        }

        private void btnBorderColor_Click(object? sender, EventArgs e)
        {
            PickColor(_pnlBorderColor);
        }

        private void pnlFillColor_Click(object? sender, EventArgs e)
        {
            PickColor(_pnlFillColor);
        }

        private void btnFillColor_Click(object? sender, EventArgs e)
        {
            PickColor(_pnlFillColor);
        }

        private void btnHatchPattern_Click(object? sender, EventArgs e)
        {
            ShowHatchPicker();
        }

        private void btnPointMarker_Click(object? sender, EventArgs e)
        {
            using frmPointMarkerPicker picker = new(
                _selectedPointMarkerKey,
                _pnlBorderColor.BackColor);

            if (picker.ShowDialog(this) != DialogResult.OK)
                return;

            _selectedPointMarkerKey = picker.SelectedMarkerKey;
            _pnlPointMarkerPreview.Invalidate();
        }

        private void txtName_TextChanged(object? sender, EventArgs e)
        {
            if (_isLoadingLayer ||
                _suppressLayerNameTextChanged ||
                _layerNameSuggestionProvider == null)
            {
                return;
            }

            if (!string.Equals(
                    _txtName.Text.Trim(),
                    _lastSuggestedLayerName,
                    StringComparison.OrdinalIgnoreCase))
            {
                _layerNameEditedByUser = true;
            }
        }

        private void cboLayerKind_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string selectedLayerKind = NormalizeDrawingLayerType(_cboLayerKind.Text);

            UpdateSuggestedLayerNameForKind(selectedLayerKind);
            UpdateLayerKindPresentation();
            if (!_isLoadingLayer &&
                IsAnnotationLayerType(selectedLayerKind) &&
                !IsAnnotationLayerType(_lastSelectedLayerKind) &&
                IsDefaultNonAnnotationLabelFontSize(_numFontSize.Value))
            {
                _numFontSize.Value = ClampDecimal(DefaultAnnotationLabelFontSize, _numFontSize);
            }

            _lastSelectedLayerKind = selectedLayerKind;
            ConfigureLockedControlState();
            UpdateFillControlState();
            _pnlLinePreview.Invalidate();
            _pnlPointMarkerPreview.Invalidate();
        }

        private void UpdateSuggestedLayerNameForKind(string layerKind)
        {
            if (_isLoadingLayer ||
                !_allowRename ||
                !_allowLayerKindChange ||
                _layerNameSuggestionProvider == null)
            {
                return;
            }

            string currentName = _txtName.Text.Trim();
            if (_layerNameEditedByUser &&
                !string.Equals(currentName, _lastSuggestedLayerName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string suggestedName = _layerNameSuggestionProvider(layerKind).Trim();
            if (string.IsNullOrWhiteSpace(suggestedName) ||
                string.Equals(currentName, suggestedName, StringComparison.OrdinalIgnoreCase))
            {
                _lastSuggestedLayerName = suggestedName;
                return;
            }

            _suppressLayerNameTextChanged = true;
            try
            {
                _txtName.Text = suggestedName;
                _lastSuggestedLayerName = suggestedName;
                _layerNameEditedByUser = false;
            }
            finally
            {
                _suppressLayerNameTextChanged = false;
            }
        }

        private void chkNoBorder_CheckedChanged(object? sender, EventArgs e)
        {
            ConfigureLockedControlState();
            _pnlLinePreview.Invalidate();
        }

        private void numPointSize_ValueChanged(object? sender, EventArgs e)
        {
            _pnlPointMarkerPreview.Invalidate();
        }

        private void pnlLabelColor_Click(object? sender, EventArgs e)
        {
            PickColor(_pnlLabelColor);
        }

        private void btnLabelColor_Click(object? sender, EventArgs e)
        {
            PickColor(_pnlLabelColor);
        }

        private void btnLabelExpression_Click(object? sender, EventArgs e)
        {
            string current          = _cboLabelField.Text.Trim();
            string currentAlignment = NormalizeTextAlignment(_cboTextAlignment.Text);

            using frmLabelExpressionEditor editor = new(current, currentAlignment, _sampleRecords, Layer.LayerType);
            if (editor.ShowDialog(this) != DialogResult.OK)
                return;

            string expression = editor.LabelExpression;
            if (!string.IsNullOrWhiteSpace(expression) &&
                !_cboLabelField.Items.Cast<object>().Any(item =>
                    string.Equals(item?.ToString(), expression, StringComparison.Ordinal)))
            {
                _cboLabelField.Items.Add(expression);
            }

            _cboLabelField.Text = expression;

            // Apply the alignment the user chose in the expression editor
            if (!string.IsNullOrWhiteSpace(editor.TextAlignment))
                SetComboText(_cboTextAlignment, editor.TextAlignment);
        }

        private void PopulateLabelFieldOptions(string? currentExpression)
        {
            string current = currentExpression?.Trim() ?? string.Empty;
            _cboLabelField.Items.Clear();

            foreach (string expression in frmLabelExpressionEditor.GetApplicableLabelExpressions(
                         Layer.LayerType,
                         _sampleRecords))
            {
                _cboLabelField.Items.Add(expression);
            }

            if (!string.IsNullOrWhiteSpace(current))
                SetComboText(_cboLabelField, current);
        }

        private void PickColor(Panel swatch)
        {
            _colorDialog.Color = swatch.BackColor;
            ColorDialogCustomColorsStore.LoadInto(_colorDialog);

            if (_colorDialog.ShowDialog(this) == DialogResult.OK)
                swatch.BackColor = _colorDialog.Color;

            ColorDialogCustomColorsStore.SaveFrom(_colorDialog);
            _pnlLinePreview.Invalidate();
            _pnlFillColor.Invalidate();
            _pnlHatchPatternPreview.Invalidate();
            _pnlPointMarkerPreview.Invalidate();
        }

        private void btnFont_Click(object? sender, EventArgs e)
        {
            _fontDialog.Font = new Font(
                string.IsNullOrWhiteSpace(_txtFontName.Text) ? DefaultCanvasLabelFontName : _txtFontName.Text,
                (float)_numFontSize.Value);

            if (_fontDialog.ShowDialog(this) != DialogResult.OK)
                return;

            _txtFontName.Text = _fontDialog.Font.Name;
            _numFontSize.Value = ClampDecimal((decimal)_fontDialog.Font.Size, _numFontSize);
        }

        private void cboFillStyle_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateFillControlState();
            _pnlFillColor.Invalidate();
            _pnlHatchPatternPreview.Invalidate();
        }

        private void cboLineStyle_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _pnlLinePreview.Invalidate();
        }

        private void numLineWeight_ValueChanged(object? sender, EventArgs e)
        {
            _pnlLinePreview.Invalidate();
        }

        private void numLineTypeScale_ValueChanged(object? sender, EventArgs e)
        {
            _pnlLinePreview.Invalidate();
        }

        private void pnlLinePreview_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(_pnlLinePreview.BackColor);
            Rectangle previewRect = Rectangle.Inflate(_pnlLinePreview.ClientRectangle, -10, -4);
            if (previewRect.Width <= 0 || previewRect.Height <= 0)
                return;

            if (_chkNoBorder.Checked)
            {
                TextRenderer.DrawText(
                    e.Graphics,
                    "No Border",
                    Font,
                    _pnlLinePreview.ClientRectangle,
                    Color.FromArgb(91, 97, 110),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            using Pen pen = new(_pnlBorderColor.BackColor, Math.Max(1f, (float)_numLineWeight.Value))
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Flat,
                EndCap = System.Drawing.Drawing2D.LineCap.Flat
            };
            ApplyPenLineStyle(pen, _cboLineStyle.Text, (float)_numLineTypeScale.Value);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            int y = previewRect.Top + previewRect.Height / 2;
            e.Graphics.DrawLine(pen, previewRect.Left, y, previewRect.Right, y);
        }

        private void pnlPointMarkerPreview_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.Clear(_pnlPointMarkerPreview.BackColor);
            Rectangle previewRect = Rectangle.Inflate(_pnlPointMarkerPreview.ClientRectangle, -5, -4);
            if (previewRect.Width <= 0 || previewRect.Height <= 0)
                return;

            PointMarkerRenderer.Draw(
                e.Graphics,
                previewRect,
                _selectedPointMarkerKey,
                _pnlBorderColor.BackColor,
                1.5f);
        }

        private void pnlHatchPatternPreview_Paint(object? sender, PaintEventArgs e)
        {
            if (!IsHatchedFill())
                return;

            Rectangle previewRect = Rectangle.Inflate(_pnlHatchPatternPreview.ClientRectangle, -2, -2);
            if (previewRect.Width <= 0 || previewRect.Height <= 0)
                return;

            _hatchPatternService.DrawPreview(
                e.Graphics,
                previewRect,
                _selectedHatchPatternKey,
                _pnlFillColor.BackColor,
                Color.White,
                GetEffectivePreviewTransparency(),
                (double)_numHatchScale.Value,
                _pnlHatchPatternPreview.Parent?.BackColor ?? Color.White);
        }

        private void numHatchScale_ValueChanged(object? sender, EventArgs e)
        {
            _pnlHatchPatternPreview.Invalidate();
        }

        private void chkShowFillTransparency_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateFillControlState();
            _pnlHatchPatternPreview.Invalidate();
        }

        private void trkTransparency_ValueChanged(object? sender, EventArgs e)
        {
            _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
            _pnlHatchPatternPreview.Invalidate();
        }

        private void txtTransparencyValue_Leave(object? sender, EventArgs e)
        {
            ApplyTransparencyTextValue();
        }

        private void txtTransparencyValue_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            ApplyTransparencyTextValue();
            e.SuppressKeyPress = true;
        }

        private void UpdateFillControlState()
        {
            bool canEdit = !_chkLocked.Checked;

            if (_isRasterLayer)
            {
                SetControlVisible(_chkShowFillTransparency, false);
                SetControlVisible(_lblTransparency, false);
                SetControlVisible(_transparencyLayout, false);
                _trkTransparency.Enabled = false;
                _txtTransparencyValue.Enabled = false;
                return;
            }

            bool hasFill = !string.Equals(_cboFillStyle.Text, "None", StringComparison.OrdinalIgnoreCase);
            bool isHatched = IsHatchedFill();
            bool applyTransparency = hasFill && _chkShowFillTransparency.Checked;

            _pnlFillColor.Enabled = canEdit && hasFill;
            _btnFillColor.Enabled = canEdit && hasFill;
            SetControlVisible(_chkShowFillTransparency, hasFill);
            _chkShowFillTransparency.Enabled = canEdit && hasFill;
            SetControlVisible(_lblTransparency, hasFill);
            SetControlVisible(_transparencyLayout, hasFill);
            _trkTransparency.Enabled = canEdit && applyTransparency;
            _txtTransparencyValue.Enabled = canEdit && applyTransparency;
            _lblFillColor.Text = isHatched ? "Hatch Color" : "Fill Color";
            _btnFillColor.Text = "Change...";

            bool showHatchPattern = hasFill && isHatched;
            SetControlVisible(_lblHatchPattern, showHatchPattern);
            SetControlVisible(_hatchPatternPanel, showHatchPattern);
            _pnlHatchPatternPreview.Enabled = canEdit && showHatchPattern;
            _btnHatchPattern.Enabled = canEdit && showHatchPattern;
            _numHatchScale.Enabled = canEdit && showHatchPattern;

            if (isHatched && string.IsNullOrWhiteSpace(_selectedHatchPatternKey))
                _selectedHatchPatternKey = _hatchPatternService.GetPatterns()[0].Key;

            ArrangeAllPropertyPanels();
        }

        private void ConfigureLockedControlState()
        {
            // Lock state only changes which controls are editable — it never changes
            // which rows are visible or their layout positions.  Do NOT call
            // UpdateLayerKindPresentation or UpdateFillControlState here: both
            // ultimately call ArrangeAllPropertyPanels → SetBounds on every row,
            // which physically moves all controls mid-click and causes the visual
            // "position shift" the user sees when toggling the Locked checkbox.
            bool canEdit = !_chkLocked.Checked;

            // General tab controls
            _txtName.ReadOnly = !_allowRename;
            _txtName.Enabled = canEdit && _allowRename;
            _cboLayerKind.Enabled = canEdit && _isDrawingMarkupLayer && _allowLayerKindChange;
            _chkVisible.Enabled = canEdit;

            bool isPoint      = IsSelectedPointLayerType();
            bool isAnnotation = IsSelectedAnnotationLayerType();
            bool hasBorder    = !_chkNoBorder.Checked;
            _pnlBorderColor.Enabled      = canEdit && !isAnnotation && (hasBorder || isPoint);
            _btnBorderColor.Enabled      = canEdit && !isAnnotation && (hasBorder || isPoint);
            _chkNoBorder.Enabled         = canEdit && !isPoint && !isAnnotation;
            _lineTypePanel.Enabled       = canEdit && hasBorder && !isPoint && !isAnnotation;
            _cboLineStyle.Enabled        = canEdit && hasBorder && !isPoint && !isAnnotation;
            _numLineTypeScale.Enabled    = canEdit && hasBorder && !isPoint && !isAnnotation;
            _numLineWeight.Enabled       = canEdit && hasBorder && !isPoint && !isAnnotation;
            _pnlPointMarkerPreview.Enabled = canEdit && isPoint;
            _btnPointMarker.Enabled      = canEdit && isPoint;
            _numPointSize.Enabled        = canEdit && isPoint;

            // Fill tab controls
            _cboFillStyle.Enabled          = canEdit && !isAnnotation;
            _pnlFillColor.Enabled          = canEdit && !isAnnotation;
            _btnFillColor.Enabled          = canEdit && !isAnnotation;
            _numHatchScale.Enabled         = canEdit && !isAnnotation;
            _pnlHatchPatternPreview.Enabled = canEdit && !isAnnotation;
            _btnHatchPattern.Enabled        = canEdit && !isAnnotation;
            _chkShowFillTransparency.Enabled = canEdit && !isAnnotation;
            _trkTransparency.Enabled       = canEdit && !isAnnotation && _chkShowFillTransparency.Checked;
            _txtTransparencyValue.Enabled  = canEdit && !isAnnotation && _chkShowFillTransparency.Checked;

            // Label tab controls
            _chkShowLabels.Enabled      = canEdit;
            _btnFont.Enabled            = canEdit;
            _numFontSize.Enabled        = canEdit;
            _pnlLabelColor.Enabled      = canEdit;
            _btnLabelColor.Enabled      = canEdit;
            _cboTextAlignment.Enabled   = canEdit && isAnnotation;
            _cboLabelField.Enabled      = canEdit;
            _btnLabelExpression.Enabled = canEdit;
            _fontScalingPanel.Enabled   = canEdit;
            _rdoFontFixedSize.Enabled   = canEdit;
            _rdoFontScalesWithZoom.Enabled = canEdit;

            // Keep the Locked checkbox itself always interactable
            _chkLocked.Enabled = true;
        }

        public event EventHandler<CanvasLayer>? LayerApplied;

        private void btnApply_Click(object? sender, EventArgs e)
        {
            if (ApplyChanges())
                LayerApplied?.Invoke(this, Layer);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK && !ApplyChanges())
            {
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
        }

        private void FontScalingRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (_rdoFontScalesWithZoom.Checked)
            {
                ConfigureLabelFontSizeControl(scaleWithZoom: true, resetToDefault: true);
                return;
            }

            if (_rdoFontFixedSize.Checked)
            {
                ConfigureLabelFontSizeControl(scaleWithZoom: false, resetToDefault: true);
            }
        }

        private void ConfigureLabelFontSizeControl(bool scaleWithZoom, bool resetToDefault)
        {
            decimal currentValue = _numFontSize.Value;
            _numFontSize.Minimum = MinLabelFontSize;
            _numFontSize.Maximum = scaleWithZoom
                ? MaxScaledLabelFontSize
                : MaxFixedLabelFontSize;

            _numFontSize.Value = resetToDefault
                ? GetDefaultLabelFontSize(scaleWithZoom)
                : ClampDecimal(currentValue, _numFontSize);
        }

        private decimal GetDefaultLabelFontSize(bool scaleWithZoom)
        {
            if (IsSelectedAnnotationLayerType())
                return DefaultAnnotationLabelFontSize;

            return scaleWithZoom
                ? DefaultScaledLabelFontSize
                : DefaultFixedLabelFontSize;
        }

        private bool ApplyChanges()
        {
            Layer.IsVisible = _chkVisible.Checked;
            Layer.IsLocked = _chkLocked.Checked;
            Layer.FillTransparency = _isRasterLayer ? 0 : _trkTransparency.Value;
            Layer.ShowFillTransparency = !_isRasterLayer && _chkShowFillTransparency.Checked;
            if (_allowRename && !string.IsNullOrWhiteSpace(_txtName.Text))
            {
                Layer.Name = _txtName.Text.Trim();
            }

            if (!_isRasterLayer)
            {
                if (_isDrawingMarkupLayer)
                {
                    if (_allowLayerKindChange)
                    {
                        Layer.LayerType = NormalizeDrawingLayerType(_cboLayerKind.Text);
                    }
                }

                Layer.BorderColor = ColorTranslator.ToHtml(_pnlBorderColor.BackColor);
                Layer.LineStyle = string.IsNullOrWhiteSpace(_cboLineStyle.Text) ? "Solid" : _cboLineStyle.Text.Trim();
                Layer.LineTypeScale = NormalizeLineTypeScale((double)_numLineTypeScale.Value);
                Layer.LineWeight = _chkNoBorder.Checked ? 0 : (double)_numLineWeight.Value;
                Layer.PointSymbol = PointMarkerRenderer.Normalize(_selectedPointMarkerKey);
                Layer.PointSize = (double)_numPointSize.Value;

                bool selectedPolyline =
                    string.Equals(NormalizeDrawingLayerType(_cboLayerKind.Text), CanvasLayerTreeService.PolylineLayerType, StringComparison.OrdinalIgnoreCase);
                bool selectedPoint =
                    string.Equals(NormalizeDrawingLayerType(_cboLayerKind.Text), CanvasLayerTreeService.PointLayerType, StringComparison.OrdinalIgnoreCase);
                bool selectedAnnotation =
                    string.Equals(NormalizeDrawingLayerType(_cboLayerKind.Text), CanvasLayerTreeService.AnnotationLayerType, StringComparison.OrdinalIgnoreCase);

                if ((!_isDrawingMarkupLayer && _isLineLayer) || selectedPolyline || selectedPoint || selectedAnnotation)
                {
                    Layer.FillStyle = "None";
                    Layer.FillColor = null;
                    Layer.HatchPattern = null;
                    Layer.HatchScale = 1.0;
                    Layer.ShowFillTransparency = false;
                    Layer.FillTransparency = 50;
                    if (selectedAnnotation)
                    {
                        Layer.LineWeight = 0;
                        Layer.PointSize = Math.Max(1.0, Layer.PointSize);
                    }
                }
                else
                {
                    Layer.FillStyle = string.IsNullOrWhiteSpace(_cboFillStyle.Text) ? "None" : _cboFillStyle.Text.Trim();
                    Layer.FillColor = string.Equals(Layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : ColorTranslator.ToHtml(_pnlFillColor.BackColor);
                    Layer.ShowFillTransparency =
                        !string.Equals(Layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase) &&
                        _chkShowFillTransparency.Checked;
                    Layer.HatchPattern = string.Equals(Layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase) &&
                                         !string.IsNullOrWhiteSpace(_selectedHatchPatternKey)
                        ? _selectedHatchPatternKey.Trim()
                        : null;
                    Layer.HatchScale = string.Equals(Layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase)
                        ? NormalizeHatchScale((double)_numHatchScale.Value)
                        : 1.0;
                }

                if (!_isExternalLayer)
                {
                    Layer.ShowLabels = selectedAnnotation ? false : _chkShowLabels.Checked;
                Layer.LabelFontName = string.IsNullOrWhiteSpace(_txtFontName.Text)
                    ? DefaultCanvasLabelFontName
                    : _txtFontName.Text.Trim();
                Layer.LabelFontSize = (double)_numFontSize.Value;
                Layer.LabelColor = ColorTranslator.ToHtml(_pnlLabelColor.BackColor);
                Layer.TextAlignment = NormalizeTextAlignment(_cboTextAlignment.Text);
                Layer.LabelScaleWithZoom = _rdoFontScalesWithZoom.Checked;

                if (selectedAnnotation)
                {
                    // Annotation (text) layer — store optional default text in LabelField.
                    string defaultText = _txtAnnotationText.Text.Trim();
                    Layer.LabelField = string.IsNullOrEmpty(defaultText)
                        ? null
                        : $"static:{defaultText}";
                }
                else
                {
                    // Field-based label; fixed-text falls back to cboLabelField.
                    Layer.LabelField = string.IsNullOrWhiteSpace(_cboLabelField.Text)
                        ? null
                        : _cboLabelField.Text.Trim();
                }

                if (selectedAnnotation)
                {
                    Layer.BorderColor = Layer.LabelColor;
                }
                }
            }

            return true;
        }

        private void ShowHatchPicker()
        {
            using frmHatchPatternPicker picker = new(
                _hatchPatternService,
                _selectedHatchPatternKey,
                _pnlFillColor.BackColor,
                Color.White,
                GetEffectivePreviewTransparency(),
                (double)_numHatchScale.Value);

            if (picker.ShowDialog(this) != DialogResult.OK)
                return;

            _selectedHatchPatternKey = picker.SelectedPatternKey;
            _pnlHatchPatternPreview.Invalidate();
        }

        private bool IsHatchedFill()
        {
            return string.Equals(_cboFillStyle.Text, "Hatched", StringComparison.OrdinalIgnoreCase);
        }

        private int GetEffectivePreviewTransparency()
        {
            return _isRasterLayer || _chkShowFillTransparency.Checked
                ? _trkTransparency.Value
                : 0;
        }

        /// <summary>
        /// Applies typed transparency text to the slider, keeping the value between 0 and 100 percent.
        /// </summary>
        private void ApplyTransparencyTextValue()
        {
            if (!int.TryParse(_txtTransparencyValue.Text.Trim().TrimEnd('%'), out int value))
            {
                _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
                return;
            }

            _trkTransparency.Value = Math.Clamp(value, _trkTransparency.Minimum, _trkTransparency.Maximum);
            _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
        }

        private static bool IsRasterLayer(CanvasLayer layer)
        {
            return string.Equals(
                layer.LayerType,
                "Raster",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string GetLayerKindText(CanvasLayer layer)
        {
            if (IsRasterLayer(layer))
                return "Raster";

            if (CanvasLayerTreeService.IsPointLayer(layer))
                return CanvasLayerTreeService.PointLayerType;

            if (CanvasLayerTreeService.IsAnnotationLayer(layer))
                return CanvasLayerTreeService.AnnotationLayerType;

            if (CanvasLayerTreeService.IsLineLayer(layer))
                return CanvasLayerTreeService.PolylineLayerType;

            return CanvasLayerTreeService.PolygonLayerType;
        }

        private void UpdateLayerKindPresentation()
        {
            if (_isRasterLayer)
            {
                ArrangeAllPropertyPanels();
                return;
            }

            string layerType = NormalizeDrawingLayerType(_cboLayerKind.Text);
            bool isPoint = string.Equals(layerType, CanvasLayerTreeService.PointLayerType, StringComparison.OrdinalIgnoreCase);
            bool isPolyline = string.Equals(layerType, CanvasLayerTreeService.PolylineLayerType, StringComparison.OrdinalIgnoreCase);
            bool isPolygon = string.Equals(layerType, CanvasLayerTreeService.PolygonLayerType, StringComparison.OrdinalIgnoreCase);
            bool isAnnotation = string.Equals(layerType, CanvasLayerTreeService.AnnotationLayerType, StringComparison.OrdinalIgnoreCase);

            ConfigureAnnotationPropertyLayout(isAnnotation);

            Text = _isDrawingMarkupLayer
                ? "Drawing Layer Properties"
                : isAnnotation ? "Annotation Layer Properties"
                : isPolyline ? "Line Layer Properties" : "Polygon Layer Properties";
            if (isAnnotation)
                Text = "Annotation Layer Properties";

            _lblBorderColor.Text = isPoint
                ? "Point Color"
                : isAnnotation ? "Text Color"
                : isPolyline ? "Line Color" : "Border Color";

            if (isAnnotation)
            {
                SetControlVisible(_chkNoBorder, false);
                SetControlVisible(_lblLineStyle, false);
                SetControlVisible(_lineTypePanel, false);
                SetControlVisible(_lblLinePreview, false);
                SetControlVisible(_pnlLinePreview, false);
                SetControlVisible(_lblLineWeight, false);
                SetControlVisible(_numLineWeight, false);
                SetControlVisible(_lblBorderColor, false);
                SetControlVisible(_borderColorPanel, false);
                SetControlVisible(_lblPointMarker, false);
                SetControlVisible(_pointMarkerPanel, false);
                ArrangeAllPropertyPanels();
                return;
            }

            SetControlVisible(_chkNoBorder, !isPoint && !isAnnotation);
            SetControlVisible(_lblLineStyle, !isPoint && !isAnnotation);
            SetControlVisible(_lineTypePanel, !isPoint && !isAnnotation);
            SetControlVisible(_lblLinePreview, !isPoint && !isAnnotation);
            SetControlVisible(_pnlLinePreview, !isPoint && !isAnnotation);
            SetControlVisible(_lblLineWeight, !isPoint && !isAnnotation);
            SetControlVisible(_numLineWeight, !isPoint && !isAnnotation);
            SetControlVisible(_lblBorderColor, !isAnnotation);
            SetControlVisible(_borderColorPanel, !isAnnotation);

            SetControlVisible(_lblPointMarker, isPoint);
            SetControlVisible(_pointMarkerPanel, isPoint);

            if (_tabs.TabPages.Contains(_tabFill) && !isPolygon)
            {
                _tabs.TabPages.Remove(_tabFill);
            }
            else if (!_tabs.TabPages.Contains(_tabFill) && isPolygon && (!_isLineLayer || _isDrawingMarkupLayer))
            {
                int insertIndex = Math.Min(1, _tabs.TabPages.Count);
                _tabs.TabPages.Insert(insertIndex, _tabFill);
            }

            ArrangeAllPropertyPanels();
        }

        private void ConfigureAnnotationPropertyLayout(bool isAnnotation)
        {
            if (isAnnotation)
            {
                // Remove all standard tabs — annotation layers get only the Annotation tab.
                if (_tabs.TabPages.Contains(_tabGeneral)) _tabs.TabPages.Remove(_tabGeneral);
                if (_tabs.TabPages.Contains(_tabFill))    _tabs.TabPages.Remove(_tabFill);
                if (_tabs.TabPages.Contains(_tabLabel))   _tabs.TabPages.Remove(_tabLabel);

                if (!_tabs.TabPages.Contains(_tabAnnotation))
                    _tabs.TabPages.Add(_tabAnnotation);

                // Reparent annotation-relevant controls into _annPanel.
                // WinForms automatically removes each control from its previous parent.
                _annPanel.Controls.AddRange([
                    _lblLayerKind, _cboLayerKind,
                    _lblName, _txtName,
                    _lblFont, _fontPanel,
                    _lblFontSize, _numFontSize,
                    _lblTextColor, _labelColorPanel,
                    _lblTextAlignment, _cboTextAlignment,
                    _lblAnnotationText, _txtAnnotationText,
                    _lblFontScaling, _fontScalingPanel,
                    _lblState, _statePanel
                ]);

                // Make all annotation rows visible.
                foreach (Control c in _annPanel.Controls)
                    c.Visible = true;

                ArrangeAnnotationPanel();
                return;
            }

            // Restore controls that were reparented into _annPanel back to their home panels.
            if (_tabs.TabPages.Contains(_tabAnnotation))
            {
                // Only restore what was moved (General: Name + State; Label: text properties).
                _generalLayout.Controls.AddRange([
                    _lblLayerKind, _cboLayerKind,
                    _lblName, _txtName,
                    _lblState, _statePanel
                ]);
                _labelLayout.Controls.AddRange([
                    _lblFont, _fontPanel,
                    _lblFontSize, _numFontSize,
                    _lblTextColor, _labelColorPanel,
                    _lblTextAlignment, _cboTextAlignment,
                    _lblFontScaling, _fontScalingPanel,
                    _lblAnnotationText, _txtAnnotationText
                ]);
                _tabs.TabPages.Remove(_tabAnnotation);
            }

            if (!_tabs.TabPages.Contains(_tabGeneral))
                _tabs.TabPages.Insert(0, _tabGeneral);
            if (!_tabs.TabPages.Contains(_tabLabel) && !_isRasterLayer)
                _tabs.TabPages.Add(_tabLabel);

            _tabLabel.Text = "Label";
            SetControlVisible(_lblFont, true);
            SetControlVisible(_fontPanel, true);
            SetControlVisible(_lblFontSize, true);
            SetControlVisible(_numFontSize, true);
            SetControlVisible(_lblTextColor, true);
            SetControlVisible(_labelColorPanel, true);
            SetControlVisible(_lblTextAlignment, false);
            SetControlVisible(_cboTextAlignment, false);
            SetControlVisible(_lblLabels, true);
            SetControlVisible(_chkShowLabels, true);
            SetControlVisible(_lblFontScaling, true);
            SetControlVisible(_fontScalingPanel, true);
            SetControlVisible(_lblAnnotationText, false);
            SetControlVisible(_txtAnnotationText, false);
            UpdateLabelSourceControlVisibility();
            ArrangeAllPropertyPanels();
        }

        private void ArrangeAnnotationPanel()
        {
            ArrangeRows(
                _annPanel,
                (_lblLayerKind, _cboLayerKind),
                (_lblName, _txtName),
                (_lblFont, _fontPanel),
                (_lblFontSize, _numFontSize),
                (_lblTextColor, _labelColorPanel),
                (_lblTextAlignment, _cboTextAlignment),
                (_lblAnnotationText, _txtAnnotationText),
                (_lblFontScaling, _fontScalingPanel),
                (_lblState, _statePanel));
        }

        private static string NormalizeDrawingLayerType(string? layerType)
        {
            return layerType?.Trim().ToLowerInvariant() switch
            {
                "point" => CanvasLayerTreeService.PointLayerType,
                "line" => CanvasLayerTreeService.PolylineLayerType,
                "polyline" => CanvasLayerTreeService.PolylineLayerType,
                "polygon" => CanvasLayerTreeService.PolygonLayerType,
                "annotation" => CanvasLayerTreeService.AnnotationLayerType,
                _ => CanvasLayerTreeService.PolylineLayerType
            };
        }

        private bool IsSelectedPointLayerType()
        {
            return string.Equals(
                NormalizeDrawingLayerType(_cboLayerKind.Text),
                CanvasLayerTreeService.PointLayerType,
                StringComparison.OrdinalIgnoreCase);
        }

        private bool IsSelectedAnnotationLayerType()
        {
            return IsAnnotationLayerType(NormalizeDrawingLayerType(_cboLayerKind.Text));
        }

        private static bool IsAnnotationLayerType(string? layerType)
        {
            return string.Equals(
                layerType,
                CanvasLayerTreeService.AnnotationLayerType,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDefaultNonAnnotationLabelFontSize(decimal fontSize)
        {
            return fontSize == DefaultScaledLabelFontSize ||
                   fontSize == DefaultFixedLabelFontSize;
        }

        private static double NormalizeLineTypeScale(double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
                return 1.0;

            return Math.Clamp(scale, 0.1, 100.0);
        }

        private static double NormalizeHatchScale(double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
                return 1.0;

            return Math.Clamp(scale, 0.1, 20.0);
        }

        private static void ApplyPenLineStyle(Pen pen, string? lineStyle, float lineTypeScale)
        {
            float scale = Math.Clamp(lineTypeScale, 0.1f, 100f);
            switch (NormalizeLineStyleKey(lineStyle))
            {
                case "DASHED":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [4f * scale, 2f * scale];
                    break;
                case "DOTTED":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [1f * scale, 2f * scale];
                    break;
                case "DASHDOT":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case "CENTERLINE":
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
                    pen.DashPattern = [8f * scale, 3f * scale, 2f * scale, 3f * scale];
                    break;
                default:
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                    break;
            }
        }

        private static string NormalizeLineStyleKey(string? lineStyle)
        {
            string normalized = (lineStyle ?? string.Empty)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant();

            return normalized switch
            {
                "DASH" => "DASHED",
                "DOT" => "DOTTED",
                _ => normalized
            };
        }

        private static string NormalizeTextAlignment(string? alignment)
        {
            if (string.IsNullOrWhiteSpace(alignment))
                return "Center Middle";

            string[] parts = alignment.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string h = (parts.Length > 0 ? parts[0] : "").ToLowerInvariant() switch
            {
                "center" or "centre" or "middle" => "Center",
                "right" => "Right",
                _ => "Left"
            };
            string v = (parts.Length > 1 ? parts[1] : "").ToLowerInvariant() switch
            {
                "middle" or "center" or "centre" => "Middle",
                "bottom" => "Bottom",
                _ => "Top"
            };
            return $"{h} {v}";
        }

        private void UpdateLabelSourceControlVisibility()
        {
            SetControlVisible(_lblLabelField, true);
            SetControlVisible(_labelFieldRow, true);
            SetControlVisible(_cboLabelField, true);
            SetControlVisible(_lblLabelFixedText, false);
            SetControlVisible(_txtLabelFixedText, false);
            ArrangeAllPropertyPanels();
        }

        private void SetControlVisible(Control control, bool visible)
        {
            control.Visible = visible;
        }

        public void FocusLayerNameTextBox()
        {
            BeginInvoke((Action)(() =>
            {
                if (_txtName.IsDisposed ||
                    !_txtName.Visible ||
                    !_txtName.Enabled)
                {
                    return;
                }

                _txtName.Focus();
                _txtName.SelectAll();
            }));
        }

        private void ArrangeAllPropertyPanels()
        {
            if (_tabs.TabPages.Contains(_tabAnnotation))
            {
                ArrangeAnnotationPanel();
                return;
            }
            ArrangeGeneralPanel();
            ArrangeFillPanel();
            ArrangeLabelPanel();
        }

        private void ArrangeGeneralPanel()
        {
            ArrangeRows(
                _generalLayout,
                (_lblLayerKind, _cboLayerKind),
                (_lblName, _txtName),
                (_lblBorderColor, _borderColorPanel),
                (_lblLineStyle, _lineTypePanel),
                (_lblLinePreview, _pnlLinePreview),
                (_lblLineWeight, _numLineWeight),
                (_lblPointMarker, _pointMarkerPanel),
                (_lblState, _statePanel));
        }

        private void ArrangeFillPanel()
        {
            ArrangeRows(
                _fillLayout,
                (_lblFillStyle, _cboFillStyle),
                (_lblFillColor, _fillColorPanel),
                (_lblHatchPattern, _hatchPatternPanel),
                (null, _chkShowFillTransparency),
                (_lblTransparency, _transparencyLayout));
        }

        private void ArrangeLabelPanel()
        {
            ArrangeRows(
                _labelLayout,
                (_lblLabels, _chkShowLabels),
                (_lblFont, _fontPanel),
                (_lblFontSize, _numFontSize),
                (_lblTextColor, _labelColorPanel),
                (_lblTextAlignment, _cboTextAlignment),
                (_lblLabelField, _labelFieldRow),
                (_lblLabelFixedText, _txtLabelFixedText),
                (_lblFontScaling, _fontScalingPanel),
                (_lblAnnotationText, _txtAnnotationText));
        }

        private static void ArrangeRows(Panel panel, params (Control? Label, Control Value)[] rows)
        {
            panel.SuspendLayout();
            panel.AutoScroll = false;
            panel.AutoScrollMinSize = Size.Empty;

            int y = 10;
            int valueWidth = Math.Min(
                PropertyValueWidth,
                Math.Max(80, panel.ClientSize.Width - PropertyValueLeft - 12));

            foreach ((Control? label, Control value) in rows)
            {
                // Skip if the control has been reparented away from this panel.
                if (value.Parent != panel) continue;

                bool visible = value.Visible && (label == null || label.Visible);
                if (!visible)
                    continue;

                int rowHeight = GetPreferredRowHeight(value);
                int labelTop = y + Math.Max(0, (rowHeight - 27) / 2);
                if (label != null)
                {
                    label.SetBounds(PropertyLabelLeft, labelTop, PropertyLabelWidth, 27);
                }

                value.SetBounds(PropertyValueLeft, y, valueWidth, rowHeight);
                y += rowHeight + 8;
            }
            panel.ResumeLayout(performLayout: true);
        }

        private static int GetPreferredRowHeight(Control control)
        {
            if (control is TrackBar)
                return 40;

            if (control is Panel panel && panel.Controls.OfType<TrackBar>().Any())
                return 40;

            if (control.Height > PropertyRowHeight)
                return control.Height;

            return PropertyRowHeight;
        }

        private static void SetComboText(ComboBox comboBox, string value)
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                !comboBox.Items.Cast<object>().Any(item =>
                    string.Equals(item.ToString(), value, StringComparison.OrdinalIgnoreCase)))
            {
                comboBox.Items.Add(value);
            }

            comboBox.Text = value;
        }

        private static decimal ClampDecimal(decimal value, NumericUpDown input)
        {
            if (value < input.Minimum)
                return input.Minimum;

            if (value > input.Maximum)
                return input.Maximum;

            return value;
        }

        private static Color ParseColorOrDefault(string? htmlColor, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(htmlColor))
                return fallback;

            try
            {
                return ColorTranslator.FromHtml(htmlColor);
            }
            catch
            {
                return fallback;
            }
        }

        private void frmLayerPropertyManager_Load(object sender, EventArgs e)
        {
            ArrangeAllPropertyPanels();
        }
    }
}
