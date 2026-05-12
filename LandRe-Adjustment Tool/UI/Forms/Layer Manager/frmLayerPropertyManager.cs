using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.Helpers;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmLayerPropertyManager : Form
    {
        public CanvasLayer Layer { get; }
        private readonly IHatchPatternService _hatchPatternService;
        private readonly bool _isRasterLayer;
        private readonly bool _isLineLayer;
        private readonly bool _isDrawingMarkupLayer;
        private readonly bool _allowRename;
        private string _selectedHatchPatternKey = string.Empty;
        private string _selectedPointMarkerKey = "Dot";

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
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));
            _hatchPatternService = hatchPatternService
                ?? throw new ArgumentNullException(nameof(hatchPatternService));
            _isRasterLayer = IsRasterLayer(layer);
            _isLineLayer = CanvasLayerTreeService.IsLineLayer(layer);
            _isDrawingMarkupLayer = CanvasLayerTreeService.IsDrawingMarkupLayer(layer);
            _allowRename = allowRename;

            InitializeComponent();
            NumericUpDownSelectAllBehavior.AttachTo(this);
            LoadLayer();
            ConfigureApplicableControls();
            _chkLocked.CheckedChanged += (_, _) => ConfigureLockedControlState();
            ConfigureLockedControlState();
        }

        /// <summary>
        /// Loads persisted layer values into the editable property controls.
        /// </summary>
        private void LoadLayer()
        {
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
            _trkTransparency.Value = Math.Max(0, Math.Min(100, Layer.FillTransparency));
            _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
            UpdateFillControlState();
            UpdateLayerKindPresentation();

            _chkShowLabels.Checked = Layer.ShowLabels;
            _txtFontName.Text = string.IsNullOrWhiteSpace(Layer.LabelFontName)
                ? "Segoe UI"
                : Layer.LabelFontName;
            _numFontSize.Value = ClampDecimal((decimal)Layer.LabelFontSize, _numFontSize);
            _pnlLabelColor.BackColor = ParseColorOrDefault(Layer.LabelColor, Color.Black);
            SetComboText(_cboLabelField, Layer.LabelField ?? string.Empty);
        }

        /// <summary>
        /// Shows only the property groups that are meaningful for the selected layer type.
        /// </summary>
        private void ConfigureApplicableControls()
        {
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
            _tabFill.Text = "Raster";
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
            _generalLayout.SetRow(_lblState, 2);
            _generalLayout.SetRow(_statePanel, 2);

            SetControlVisible(_lblFillStyle, false);
            SetControlVisible(_cboFillStyle, false);
            SetControlVisible(_lblFillColor, false);
            SetControlVisible(_fillColorPanel, false);
            SetControlVisible(_lblHatchPattern, false);
            SetControlVisible(_hatchPatternPanel, false);
            _fillLayout.SetRow(_lblTransparency, 0);
            _fillLayout.SetRow(_transparencyLayout, 0);

            _lblTransparency.Text = "Raster Transparency";
            _trkTransparency.Enabled = true;
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

        private void cboLayerKind_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateLayerKindPresentation();
            ConfigureLockedControlState();
            UpdateFillControlState();
            _pnlLinePreview.Invalidate();
            _pnlPointMarkerPreview.Invalidate();
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
                string.IsNullOrWhiteSpace(_txtFontName.Text) ? "Segoe UI" : _txtFontName.Text,
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
                _trkTransparency.Value,
                (double)_numHatchScale.Value,
                _pnlHatchPatternPreview.Parent?.BackColor ?? Color.White);
        }

        private void numHatchScale_ValueChanged(object? sender, EventArgs e)
        {
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
                _trkTransparency.Enabled = canEdit;
                _txtTransparencyValue.Enabled = canEdit;
                return;
            }

            bool hasFill = !string.Equals(_cboFillStyle.Text, "None", StringComparison.OrdinalIgnoreCase);
            bool isHatched = IsHatchedFill();

            _pnlFillColor.Enabled = canEdit && hasFill;
            _btnFillColor.Enabled = canEdit && hasFill;
            _trkTransparency.Enabled = canEdit && hasFill;
            _txtTransparencyValue.Enabled = canEdit && hasFill;
            _lblFillColor.Text = isHatched ? "Hatch Color" : "Fill Color";
            _btnFillColor.Text = "Change...";

            bool showHatchPattern = hasFill && isHatched;
            SetControlVisible(_lblHatchPattern, showHatchPattern);
            SetControlVisible(_hatchPatternPanel, showHatchPattern);
            _pnlHatchPatternPreview.Enabled = canEdit && showHatchPattern;
            _btnHatchPattern.Enabled = canEdit && showHatchPattern;
            _numHatchScale.Enabled = canEdit && showHatchPattern;
            SetFillRowHeight(2, showHatchPattern ? 38F : 0F);

            if (isHatched && string.IsNullOrWhiteSpace(_selectedHatchPatternKey))
                _selectedHatchPatternKey = _hatchPatternService.GetPatterns()[0].Key;
        }

        private void ConfigureLockedControlState()
        {
            bool canEdit = !_chkLocked.Checked;

            _txtName.ReadOnly = !_allowRename;
            _txtName.Enabled = canEdit && _allowRename;
            // Layer type can only be changed for NEW drawing layers, not existing ones
            _cboLayerKind.Enabled = canEdit && _isDrawingMarkupLayer && _allowRename;
            _chkVisible.Enabled = canEdit;

            bool isPoint = IsSelectedPointLayerType();
            bool isAnnotation = IsSelectedAnnotationLayerType();
            bool hasBorder = !_chkNoBorder.Checked;
            _pnlBorderColor.Enabled = canEdit && !isAnnotation && (hasBorder || isPoint);
            _btnBorderColor.Enabled = canEdit && !isAnnotation && (hasBorder || isPoint);
            _chkNoBorder.Enabled = canEdit && !isPoint && !isAnnotation;
            _lineTypePanel.Enabled = canEdit && hasBorder && !isPoint && !isAnnotation;
            _cboLineStyle.Enabled = canEdit && hasBorder && !isPoint && !isAnnotation;
            _numLineTypeScale.Enabled = canEdit && hasBorder && !isPoint && !isAnnotation;
            _numLineWeight.Enabled = canEdit && hasBorder && !isPoint && !isAnnotation;
            _pnlPointMarkerPreview.Enabled = canEdit && isPoint;
            _btnPointMarker.Enabled = canEdit && isPoint;
            _numPointSize.Enabled = canEdit && isPoint;

            _cboFillStyle.Enabled = canEdit && !isAnnotation;
            _pnlFillColor.Enabled = canEdit && !isAnnotation;
            _btnFillColor.Enabled = canEdit && !isAnnotation;
            _numHatchScale.Enabled = canEdit && !isAnnotation;
            _trkTransparency.Enabled = canEdit && !isAnnotation;
            _txtTransparencyValue.Enabled = canEdit && !isAnnotation;

            _chkShowLabels.Enabled = canEdit;
            _btnFont.Enabled = canEdit;
            _numFontSize.Enabled = canEdit;
            _pnlLabelColor.Enabled = canEdit;
            _btnLabelColor.Enabled = canEdit;
            _cboLabelField.Enabled = canEdit;

            _chkLocked.Enabled = true;
            UpdateLayerKindPresentation();
            UpdateFillControlState();
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

        private bool ApplyChanges()
        {
            Layer.IsVisible = _chkVisible.Checked;
            Layer.IsLocked = _chkLocked.Checked;
            Layer.FillTransparency = _trkTransparency.Value;
            if (_allowRename && !string.IsNullOrWhiteSpace(_txtName.Text))
            {
                Layer.Name = _txtName.Text.Trim();
            }

            if (!_isRasterLayer)
            {
                if (_isDrawingMarkupLayer)
                {
                    Layer.LayerType = NormalizeDrawingLayerType(_cboLayerKind.Text);
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
                    Layer.FillTransparency = 100;
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
                    Layer.HatchPattern = string.Equals(Layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase) &&
                                         !string.IsNullOrWhiteSpace(_selectedHatchPatternKey)
                        ? _selectedHatchPatternKey.Trim()
                        : null;
                    Layer.HatchScale = string.Equals(Layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase)
                        ? NormalizeHatchScale((double)_numHatchScale.Value)
                        : 1.0;
                }

                Layer.ShowLabels = _chkShowLabels.Checked;
                Layer.LabelFontName = string.IsNullOrWhiteSpace(_txtFontName.Text)
                    ? "Segoe UI"
                    : _txtFontName.Text.Trim();
                Layer.LabelFontSize = (double)_numFontSize.Value;
                Layer.LabelColor = ColorTranslator.ToHtml(_pnlLabelColor.BackColor);
                Layer.LabelField = string.IsNullOrWhiteSpace(_cboLabelField.Text)
                    ? null
                    : _cboLabelField.Text.Trim();
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
                _trkTransparency.Value,
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
                return;

            string layerType = NormalizeDrawingLayerType(_cboLayerKind.Text);
            bool isPoint = string.Equals(layerType, CanvasLayerTreeService.PointLayerType, StringComparison.OrdinalIgnoreCase);
            bool isPolyline = string.Equals(layerType, CanvasLayerTreeService.PolylineLayerType, StringComparison.OrdinalIgnoreCase);
            bool isPolygon = string.Equals(layerType, CanvasLayerTreeService.PolygonLayerType, StringComparison.OrdinalIgnoreCase);
            bool isAnnotation = string.Equals(layerType, CanvasLayerTreeService.AnnotationLayerType, StringComparison.OrdinalIgnoreCase);

            Text = _isDrawingMarkupLayer
                ? "Drawing Layer Properties"
                : isAnnotation ? "Annotation Layer Properties"
                : isPolyline ? "Line Layer Properties" : "Polygon Layer Properties";
            _lblBorderColor.Text = isPoint
                ? "Point Color"
                : isAnnotation ? "Text Color"
                : isPolyline ? "Line Color" : "Border Color";

            SetControlVisible(_chkNoBorder, !isPoint && !isAnnotation);
            SetControlVisible(_lblLineStyle, !isPoint && !isAnnotation);
            SetControlVisible(_lineTypePanel, !isPoint && !isAnnotation);
            SetControlVisible(_lblLinePreview, !isPoint && !isAnnotation);
            SetControlVisible(_pnlLinePreview, !isPoint && !isAnnotation);
            SetControlVisible(_lblLineWeight, !isPoint && !isAnnotation);
            SetControlVisible(_numLineWeight, !isPoint && !isAnnotation);
            SetControlVisible(_lblBorderColor, !isAnnotation);
            SetControlVisible(_borderColorPanel, !isAnnotation);
            SetGeneralRowHeight(2, isAnnotation ? 0F : 38F);
            SetGeneralRowHeight(3, isPoint || isAnnotation ? 0F : 38F);
            SetGeneralRowHeight(4, isPoint || isAnnotation ? 0F : 38F);
            SetGeneralRowHeight(5, isPoint || isAnnotation ? 0F : 38F);

            SetControlVisible(_lblPointMarker, isPoint);
            SetControlVisible(_pointMarkerPanel, isPoint);
            SetGeneralRowHeight(6, isPoint ? 38F : 0F);

            if (_tabs.TabPages.Contains(_tabFill) && !isPolygon)
            {
                _tabs.TabPages.Remove(_tabFill);
            }
            else if (!_tabs.TabPages.Contains(_tabFill) && isPolygon && (!_isLineLayer || _isDrawingMarkupLayer))
            {
                int insertIndex = Math.Min(1, _tabs.TabPages.Count);
                _tabs.TabPages.Insert(insertIndex, _tabFill);
            }
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
            return string.Equals(
                NormalizeDrawingLayerType(_cboLayerKind.Text),
                CanvasLayerTreeService.AnnotationLayerType,
                StringComparison.OrdinalIgnoreCase);
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

        private static void SetControlVisible(Control control, bool visible)
        {
            control.Visible = visible;
            control.Enabled = visible;
        }

        private void SetFillRowHeight(int rowIndex, float height)
        {
            if (rowIndex < 0 || rowIndex >= _fillLayout.RowStyles.Count)
                return;

            _fillLayout.RowStyles[rowIndex].SizeType = SizeType.Absolute;
            _fillLayout.RowStyles[rowIndex].Height = height;
        }

        private void SetGeneralRowHeight(int rowIndex, float height)
        {
            if (rowIndex < 0 || rowIndex >= _generalLayout.RowStyles.Count)
                return;

            _generalLayout.RowStyles[rowIndex].SizeType = SizeType.Absolute;
            _generalLayout.RowStyles[rowIndex].Height = height;
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

        }
    }
}
