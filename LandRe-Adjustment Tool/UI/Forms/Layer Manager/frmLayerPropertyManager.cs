using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.Helpers;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmLayerPropertyManager : Form
    {
        public CanvasLayer Layer { get; }
        private readonly bool _isRasterLayer;

        public frmLayerPropertyManager(CanvasLayer layer)
        {
            Layer = layer ?? throw new ArgumentNullException(nameof(layer));
            _isRasterLayer = IsRasterLayer(layer);

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
            _pnlBorderColor.BackColor = ParseColorOrDefault(Layer.BorderColor, Color.Black);
            SetComboText(_cboLineStyle, Layer.LineStyle);
            _numLineWeight.Value = ClampDecimal((decimal)Layer.LineWeight, _numLineWeight);
            _chkVisible.Checked = Layer.IsVisible;
            _chkLocked.Checked = Layer.IsLocked;

            SetComboText(_cboFillStyle, string.IsNullOrWhiteSpace(Layer.FillStyle) ? "None" : Layer.FillStyle);
            _pnlFillColor.BackColor = ParseColorOrDefault(Layer.FillColor, Color.White);
            SetComboText(_cboHatch, Layer.HatchPattern ?? string.Empty);
            _trkTransparency.Value = Math.Max(0, Math.Min(100, Layer.FillTransparency));
            _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
            UpdateFillControlState();

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
            if (!_isRasterLayer)
                return;

            Text = "Raster Layer Properties";
            _tabFill.Text = "Raster";
            _tabs.TabPages.Remove(_tabLabel);

            SetControlVisible(_lblBorderColor, false);
            SetControlVisible(_borderColorPanel, false);
            SetControlVisible(_lblLineStyle, false);
            SetControlVisible(_cboLineStyle, false);
            SetControlVisible(_lblLineWeight, false);
            SetControlVisible(_numLineWeight, false);
            _generalLayout.SetRow(_lblState, 1);
            _generalLayout.SetRow(_statePanel, 1);

            SetControlVisible(_lblFillStyle, false);
            SetControlVisible(_cboFillStyle, false);
            SetControlVisible(_lblFillColor, false);
            SetControlVisible(_fillColorPanel, false);
            SetControlVisible(_lblHatch, false);
            SetControlVisible(_cboHatch, false);
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
        }

        private void trkTransparency_ValueChanged(object? sender, EventArgs e)
        {
            _txtTransparencyValue.Text = _trkTransparency.Value.ToString();
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
            bool isHatched = string.Equals(_cboFillStyle.Text, "Hatched", StringComparison.OrdinalIgnoreCase);

            _pnlFillColor.Enabled = canEdit && hasFill;
            _btnFillColor.Enabled = canEdit && hasFill;
            _cboHatch.Enabled = canEdit && isHatched;
            _trkTransparency.Enabled = canEdit && hasFill;
            _txtTransparencyValue.Enabled = canEdit && hasFill;
        }

        private void ConfigureLockedControlState()
        {
            bool canEdit = !_chkLocked.Checked;

            _txtName.Enabled = canEdit;
            _chkVisible.Enabled = canEdit;

            _pnlBorderColor.Enabled = canEdit;
            _btnBorderColor.Enabled = canEdit;
            _cboLineStyle.Enabled = canEdit;
            _numLineWeight.Enabled = canEdit;

            _cboFillStyle.Enabled = canEdit;
            _pnlFillColor.Enabled = canEdit;
            _btnFillColor.Enabled = canEdit;
            _cboHatch.Enabled = canEdit;
            _trkTransparency.Enabled = canEdit;
            _txtTransparencyValue.Enabled = canEdit;

            _chkShowLabels.Enabled = canEdit;
            _btnFont.Enabled = canEdit;
            _numFontSize.Enabled = canEdit;
            _pnlLabelColor.Enabled = canEdit;
            _btnLabelColor.Enabled = canEdit;
            _cboLabelField.Enabled = canEdit;

            _chkLocked.Enabled = true;
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

            if (!_isRasterLayer)
            {
                Layer.BorderColor = ColorTranslator.ToHtml(_pnlBorderColor.BackColor);
                Layer.LineStyle = string.IsNullOrWhiteSpace(_cboLineStyle.Text) ? "Solid" : _cboLineStyle.Text.Trim();
                Layer.LineWeight = (double)_numLineWeight.Value;

                Layer.FillStyle = string.IsNullOrWhiteSpace(_cboFillStyle.Text) ? "None" : _cboFillStyle.Text.Trim();
                Layer.FillColor = string.Equals(Layer.FillStyle, "None", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : ColorTranslator.ToHtml(_pnlFillColor.BackColor);
                Layer.HatchPattern = string.Equals(Layer.FillStyle, "Hatched", StringComparison.OrdinalIgnoreCase) &&
                                     !string.IsNullOrWhiteSpace(_cboHatch.Text)
                    ? _cboHatch.Text.Trim()
                    : null;

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

        private static void SetControlVisible(Control control, bool visible)
        {
            control.Visible = visible;
            control.Enabled = visible;
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
