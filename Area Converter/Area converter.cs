using Land_Readjustment_Tool.Services;
using System.Globalization;

namespace Land_Readjustment_Tool
{
    public partial class frmAreaConverter : Form
    {
        private const int MaxOtherUnitPrecision = 6;
        private const int MinTraditionalPrecision = 0;

        private readonly Dictionary<TextBox, string> _lastValidNumericText = new();
        private readonly Dictionary<TextBox, Color> _quickConvertDefaultBackColors = new();
        private string _lastValidRapdInput = string.Empty;
        private string _lastValidBkdInput = string.Empty;
        private bool _suppressTextValidation;
        private bool _suppressConvertSectionUpdates;
        private TextBox? _lastFocusedQuickConvertTextBox;
        private Point _convertFromGroupLocation;
        private Point _convertToGroupLocation;
        private bool _unitGroupLayoutInitialized;
        private static readonly Color QuickConvertHighlightColor = Color.FromArgb(255, 251, 235);

        public frmAreaConverter()
        {
            InitializeComponent();
            HookValidationEvents();
        }

        private void frmAreaConverter_Load(object sender, EventArgs e)
        {
            InitializeUnitGroupLayouts();
            InitializePrecisionValidation();
            InitializeConvertSectionOutputReadOnly();
            ApplyUnitGroupVisibility();
            ResetValues();
            ResetConvertFromInputsAndFocus();
            UpdateConvertToFromActiveInput();
            BeginInvoke(new Action(FocusActiveConvertFromInput));
        }

        private void HookValidationEvents()
        {
            foreach (var box in new[] { txtSqm, txtSqft, txtRopani, txtAana, txtPaisa, txtDam, txtRapd, txtBigha, txtKattha, txtDhur, txtBkd })
            {
                box.Enter += TextBox_EnterSelectAll;
                box.Enter += QuickConvertTextBox_Enter;

                if (!_quickConvertDefaultBackColors.ContainsKey(box))
                {
                    _quickConvertDefaultBackColors[box] = box.BackColor;
                }
            }

            txtSqm.TextChanged += NumericTextBox_TextChanged;
            txtSqft.TextChanged += NumericTextBox_TextChanged;
            txtRopani.TextChanged += NumericTextBox_TextChanged;
            txtAana.TextChanged += NumericTextBox_TextChanged;
            txtPaisa.TextChanged += NumericTextBox_TextChanged;
            txtDam.TextChanged += NumericTextBox_TextChanged;
            txtBigha.TextChanged += NumericTextBox_TextChanged;
            txtKattha.TextChanged += NumericTextBox_TextChanged;
            txtDhur.TextChanged += NumericTextBox_TextChanged;

            txtRapd.TextChanged += txtRapd_TextChanged;
            txtBkd.TextChanged += txtBkd_TextChanged;

            txtSqm.KeyPress += NumericTextBox_KeyPress;
            txtSqft.KeyPress += NumericTextBox_KeyPress;
            txtRopani.KeyPress += NumericTextBox_KeyPress;
            txtAana.KeyPress += NumericTextBox_KeyPress;
            txtPaisa.KeyPress += NumericTextBox_KeyPress;
            txtDam.KeyPress += NumericTextBox_KeyPress;
            txtBigha.KeyPress += NumericTextBox_KeyPress;
            txtKattha.KeyPress += NumericTextBox_KeyPress;
            txtDhur.KeyPress += NumericTextBox_KeyPress;

            txtRapd.KeyPress += txtRapd_KeyPress;
            txtBkd.KeyPress += txtBkd_KeyPress;

            btnResetQuickConvert.Click += btnReset_Click;
            btnCopyToClipboard.Click += btnCopyToClipboard_Click;
            btnExit.Click += (_, _) => Close();
            btnReset.Click += btnConvertFromReset_Click;
            btnCopy.Click += btnCopy_Click;

            radioButton1.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton2.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton3.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton4.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton8.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton7.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton6.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            radioButton5.CheckedChanged += UnitSelectionRadio_CheckedChanged;
            nudOtherUnitPrecision.ValueChanged += nudOtherUnitPrecision_ValueChanged;
            nudOtherUnitPrecision.KeyPress += nudOtherUnitPrecision_KeyPress;
            nudTraditionalUnitPrecision.ValueChanged += nudPrecision_ValueChanged;
            nudTraditionalUnitPrecision.KeyPress += nudPrecision_KeyPress;

            txtFromSqm.TextChanged += ConvertFromInput_TextChanged;
            txtFromSqft.TextChanged += ConvertFromInput_TextChanged;
            txtFromRopanee.TextChanged += ConvertFromInput_TextChanged;
            txtFromAana.TextChanged += ConvertFromInput_TextChanged;
            txtFromPaisa.TextChanged += ConvertFromInput_TextChanged;
            txtFromDaam.TextChanged += ConvertFromInput_TextChanged;
            txtFromBigha.TextChanged += ConvertFromInput_TextChanged;
            txtFromKattha.TextChanged += ConvertFromInput_TextChanged;
            txtFromDhur.TextChanged += ConvertFromInput_TextChanged;

            foreach (var box in new[] { txtFromSqm, txtFromSqft, txtFromRopanee, txtFromAana, txtFromPaisa, txtFromDaam, txtFromBigha, txtFromKattha, txtFromDhur })
            {
                box.KeyPress += NumericTextBox_KeyPress;
                box.Enter += TextBox_EnterSelectAll;
            }

            foreach (var box in new[] { txtToSqm, txtToSqft, txtToRopanee, txtToAana, txtToPaisa, txtToDaam, txtToBigha, txtToKattha, txtToDhur })
            {
                box.Enter += TextBox_EnterSelectAll;
            }

            foreach (var box in new[] { txtSqm, txtSqft, txtRopani, txtAana, txtPaisa, txtDam, txtBigha, txtKattha, txtDhur })
            {
                _lastValidNumericText[box] = string.Empty;
            }
        }

        private static void TextBox_EnterSelectAll(object? sender, EventArgs e)
        {
            if (sender is TextBox box)
            {
                box.BeginInvoke(new Action(box.SelectAll));
            }
        }

        private void QuickConvertTextBox_Enter(object? sender, EventArgs e)
        {
            if (sender is TextBox box)
            {
                _lastFocusedQuickConvertTextBox = box;
                HighlightQuickConvertTextBox(box);
            }
        }

        private void HighlightQuickConvertTextBox(TextBox focusedBox)
        {
            foreach (var pair in _quickConvertDefaultBackColors)
            {
                pair.Key.BackColor = pair.Key == focusedBox
                    ? QuickConvertHighlightColor
                    : pair.Value;
            }
        }

        private void ResetValues()
        {
            _suppressTextValidation = true;

            string formattedZero = FormatNumber(0);
            txtSqm.Text = formattedZero;
            txtSqft.Text = formattedZero;
            txtRopani.Text = formattedZero;
            txtAana.Text = formattedZero;
            txtPaisa.Text = formattedZero;
            txtDam.Text = formattedZero;
            txtRapd.Text = AreaConverterService.FormatRAPD(0, 0, 0, 0, GetTraditionalPrecision());
            txtBigha.Text = formattedZero;
            txtKattha.Text = formattedZero;
            txtDhur.Text = formattedZero;
            txtBkd.Text = AreaConverterService.FormatBKD(0, 0, 0, GetTraditionalPrecision());

            _lastValidRapdInput = txtRapd.Text;
            _lastValidBkdInput = txtBkd.Text;

            foreach (var box in _lastValidNumericText.Keys.ToList())
            {
                _lastValidNumericText[box] = box.Text;
            }

            _suppressTextValidation = false;
        }

        private void btnReset_Click(object? sender, EventArgs e)
        {
            ResetValues();
        }

        private void NumericTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || sender is not TextBox box)
                return;

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '.')
            {
                string nextText = GetNextText(box, e.KeyChar);
                if (nextText.Count(ch => ch == '.') > 1)
                {
                    e.Handled = true;
                }
            }
        }

        private void NumericTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressTextValidation || sender is not TextBox source)
                return;

            string text = source.Text.Trim();

            if (!IsValidNumericText(text))
            {
                RevertNumericText(source);
                ShowMessage("Invalid numeric input.");
                return;
            }

            _lastValidNumericText[source] = source.Text;

            if (!TryParseDouble(text, out double value))
                return;

            double sqm = source.Name switch
            {
                nameof(txtSqm) => value,
                nameof(txtSqft) => AreaConverterService.SqftToSqm(value, 9),
                nameof(txtRopani) => AreaConverterService.RopaniToSqm(value, 9),
                nameof(txtAana) => AreaConverterService.AanaToSqm(value, 9),
                nameof(txtPaisa) => AreaConverterService.PaisaToSqm(value, 9),
                nameof(txtDam) => AreaConverterService.DamToSqm(value, 9),
                nameof(txtBigha) => AreaConverterService.BighaToSqm(value, 9),
                nameof(txtKattha) => AreaConverterService.KatthaToSqm(value, 9),
                nameof(txtDhur) => AreaConverterService.DhurToSqm(value, 9),
                _ => 0
            };

            UpdateAllFromSqm(sqm, source);
        }

        private void txtRapd_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || sender is not TextBox box)
                return;

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != '.')
            {
                e.Handled = true;
                ShowRapdValidationMessage();
                return;
            }

            string nextText = GetNextText(box, e.KeyChar);
            if (!IsValidRapdForTyping(nextText))
            {
                e.Handled = true;
                ShowRapdValidationMessage();
            }
        }

        private void txtBkd_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || sender is not TextBox box)
                return;

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '-' && e.KeyChar != '.')
            {
                e.Handled = true;
                ShowBkdValidationMessage();
                return;
            }

            string nextText = GetNextText(box, e.KeyChar);
            if (!IsValidBkdForTyping(nextText))
            {
                e.Handled = true;
                ShowBkdValidationMessage();
            }
        }

        private void txtRapd_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressTextValidation)
                return;

            string current = txtRapd.Text.Trim();
            if (string.IsNullOrEmpty(current))
            {
                _lastValidRapdInput = AreaConverterService.FormatRAPD(0, 0, 0, 0, GetTraditionalPrecision());
                UpdateAllFromSqm(0, txtRapd);
                return;
            }

            if (!IsValidRapdForTyping(current))
            {
                RevertRapd();
                ShowRapdValidationMessage();
                return;
            }

            _lastValidRapdInput = txtRapd.Text;

            if (!IsValidRapdOverall(current))
                return;

            double? sqm = AreaConverterService.ParseRAPDToSqm(current, 9);
            if (!sqm.HasValue)
                return;

            UpdateAllFromSqm(sqm.Value, txtRapd);
        }

        private void txtBkd_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressTextValidation)
                return;

            string current = txtBkd.Text.Trim();
            if (string.IsNullOrEmpty(current))
            {
                _lastValidBkdInput = AreaConverterService.FormatBKD(0, 0, 0, GetTraditionalPrecision());
                UpdateAllFromSqm(0, txtBkd);
                return;
            }

            if (!IsValidBkdForTyping(current))
            {
                RevertBkd();
                ShowBkdValidationMessage();
                return;
            }

            _lastValidBkdInput = txtBkd.Text;

            if (!IsValidBkdOverall(current))
                return;

            double? sqm = AreaConverterService.ParseBKDToSqm(current, 9);
            if (!sqm.HasValue)
                return;

            UpdateAllFromSqm(sqm.Value, txtBkd);
        }

        private void UpdateAllFromSqm(double sqm, TextBox? source)
        {
            _suppressTextValidation = true;

            if (source != txtSqm) txtSqm.Text = FormatNumber(sqm);
            if (source != txtSqft) txtSqft.Text = FormatNumber(AreaConverterService.SqmToSqft(sqm, GetOtherUnitPrecision()));
            if (source != txtRopani) txtRopani.Text = FormatNumber(AreaConverterService.SqmToRopani(sqm, GetOtherUnitPrecision()));
            if (source != txtAana) txtAana.Text = FormatNumber(AreaConverterService.SqmToAana(sqm, GetOtherUnitPrecision()));
            if (source != txtPaisa) txtPaisa.Text = FormatNumber(AreaConverterService.SqmToPaisa(sqm, GetOtherUnitPrecision()));
            if (source != txtDam) txtDam.Text = FormatNumber(AreaConverterService.SqmToDam(sqm, GetOtherUnitPrecision()));
            if (source != txtRapd) txtRapd.Text = AreaConverterService.SqmToRAPDString(sqm, damPrecision: GetTraditionalPrecision());
            if (source != txtBigha) txtBigha.Text = FormatNumber(AreaConverterService.SqmToBigha(sqm, GetOtherUnitPrecision()));
            if (source != txtKattha) txtKattha.Text = FormatNumber(AreaConverterService.SqmToKattha(sqm, GetOtherUnitPrecision()));
            if (source != txtDhur) txtDhur.Text = FormatNumber(AreaConverterService.SqmToDhur(sqm, GetOtherUnitPrecision()));
            if (source != txtBkd) txtBkd.Text = AreaConverterService.SqmToBKDString(sqm, dhurPrecision: GetTraditionalPrecision());

            _lastValidRapdInput = txtRapd.Text;
            _lastValidBkdInput = txtBkd.Text;

            foreach (var box in _lastValidNumericText.Keys.ToList())
            {
                _lastValidNumericText[box] = box.Text;
            }

            _suppressTextValidation = false;
        }

        private void RevertRapd()
        {
            _suppressTextValidation = true;
            txtRapd.Text = _lastValidRapdInput;
            txtRapd.SelectionStart = txtRapd.Text.Length;
            _suppressTextValidation = false;
        }

        private void RevertBkd()
        {
            _suppressTextValidation = true;
            txtBkd.Text = _lastValidBkdInput;
            txtBkd.SelectionStart = txtBkd.Text.Length;
            _suppressTextValidation = false;
        }

        private void RevertNumericText(TextBox box)
        {
            _suppressTextValidation = true;
            box.Text = _lastValidNumericText.TryGetValue(box, out string? value) ? value : string.Empty;
            box.SelectionStart = box.Text.Length;
            _suppressTextValidation = false;
        }

        private static string GetNextText(TextBox box, char keyChar)
        {
            return box.Text.Remove(box.SelectionStart, box.SelectionLength)
                .Insert(box.SelectionStart, keyChar.ToString());
        }

        private static bool TryParseDouble(string value, out double result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return true;
            }

            if (value == ".")
            {
                result = 0;
                return false;
            }

            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result)
                || double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out result);
        }

        private static bool IsValidNumericText(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == ".")
                return true;

            int dotCount = value.Count(ch => ch == '.');
            if (dotCount > 1)
                return false;

            return value.All(ch => char.IsDigit(ch) || ch == '.');
        }

        private string FormatNumber(double value)
            => value.ToString($"F{GetOtherUnitPrecision()}", CultureInfo.InvariantCulture);

        private static void ShowMessage(string message)
        {
            MessageBox.Show(message, "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void ShowRapdValidationMessage()
        {
            ShowMessage(
                "Invalid RAPD. Use: R-A-P-D\n\n" +
                "Rules:\n" +
                "- Ropani: any integer\n" +
                "- Aana: less than 16\n" +
                "- Paisa: less than 4\n" +
                "- Dam: less than 4 (decimal allowed)");
        }

        private static void ShowBkdValidationMessage()
        {
            ShowMessage(
                "Invalid BKD. Use: B-K-D\n\n" +
                "Rules:\n" +
                "- Bigha: any integer\n" +
                "- Kattha: less than 20\n" +
                "- Dhur: less than 20 (decimal allowed)");
        }

        private static bool IsValidRapdForTyping(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            var parts = input.Split('-');
            if (parts.Length > 4)
                return false;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.IsNullOrEmpty(parts[i]))
                    return false;
            }

            if (!IsDigitsOnly(parts[0]))
                return false;

            if (parts.Length >= 2)
            {
                if (!IsDigitsOnly(parts[1]))
                    return false;

                if (parts[1].Length > 0 && int.Parse(parts[1], CultureInfo.InvariantCulture) >= 16)
                    return false;
            }

            if (parts.Length >= 3)
            {
                if (!IsDigitsOnly(parts[2]))
                    return false;

                if (parts[2].Length > 0 && int.Parse(parts[2], CultureInfo.InvariantCulture) >= 4)
                    return false;
            }

            if (parts.Length == 4)
            {
                string dam = parts[3];

                if (!IsValidDecimalText(dam))
                    return false;

                if (dam.Length > 0 && dam != "." &&
                    double.TryParse(dam, NumberStyles.Float, CultureInfo.InvariantCulture, out double damValue) &&
                    damValue >= 4)
                    return false;
            }

            return true;
        }

        private static bool IsValidRapdOverall(string input)
        {
            var parts = input.Split('-', StringSplitOptions.None);
            if (parts.Length != 4 || parts.Any(string.IsNullOrWhiteSpace))
                return false;

            if (!IsDigitsOnly(parts[0]))
                return false;

            if (!IsDigitsOnly(parts[1]) || int.Parse(parts[1], CultureInfo.InvariantCulture) >= 16)
                return false;

            if (!IsDigitsOnly(parts[2]) || int.Parse(parts[2], CultureInfo.InvariantCulture) >= 4)
                return false;

            return IsValidDecimalText(parts[3])
                && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double dam)
                && dam >= 0
                && dam < 4;
        }

        private static bool IsValidBkdForTyping(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            var parts = input.Split('-');
            if (parts.Length > 3)
                return false;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.IsNullOrEmpty(parts[i]))
                    return false;
            }

            if (!IsDigitsOnly(parts[0]))
                return false;

            if (parts.Length >= 2)
            {
                if (!IsDigitsOnly(parts[1]))
                    return false;

                if (parts[1].Length > 0 && int.Parse(parts[1], CultureInfo.InvariantCulture) >= 20)
                    return false;
            }

            if (parts.Length == 3)
            {
                string dhur = parts[2];

                if (!IsValidDecimalText(dhur))
                    return false;

                if (dhur.Length > 0 && dhur != "." &&
                    double.TryParse(dhur, NumberStyles.Float, CultureInfo.InvariantCulture, out double dhurValue) &&
                    dhurValue >= 20)
                    return false;
            }

            return true;
        }

        private static bool IsValidBkdOverall(string input)
        {
            var parts = input.Split('-', StringSplitOptions.None);
            if (parts.Length != 3 || parts.Any(string.IsNullOrWhiteSpace))
                return false;

            if (!IsDigitsOnly(parts[0]))
                return false;

            if (!IsDigitsOnly(parts[1]) || int.Parse(parts[1], CultureInfo.InvariantCulture) >= 20)
                return false;

            return IsValidDecimalText(parts[2])
                && double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double dhur)
                && dhur >= 0
                && dhur < 20;
        }

        private static bool IsDigitsOnly(string value)
        {
            return value.All(char.IsDigit);
        }

        private static bool IsValidDecimalText(string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            int dotCount = value.Count(ch => ch == '.');
            if (dotCount > 1)
                return false;

            return value.All(ch => char.IsDigit(ch) || ch == '.');
        }

        private void InitializeUnitGroupLayouts()
        {
            if (_unitGroupLayoutInitialized)
                return;

            _convertFromGroupLocation = grpFromSqm.Location;
            _convertToGroupLocation = grpToRAPD.Location;

            foreach (var group in new[] { grpFromSqm, grpFromSqft, grpFromRAPD, grpFromBKD })
            {
                group.Parent = grpConvertFrom;
                group.Location = _convertFromGroupLocation;
            }

            foreach (var group in new[] { grpToRAPD, grpToSqft, grpSqm, grpToBKD })
            {
                group.Parent = grpConvertTo;
                group.Location = _convertToGroupLocation;
            }

            _unitGroupLayoutInitialized = true;
        }

        private void UnitSelectionRadio_CheckedChanged(object? sender, EventArgs e)
        {
            if (sender is not RadioButton radioButton || !radioButton.Checked)
                return;

            ApplyUnitGroupVisibility();

            if (radioButton == radioButton1 || radioButton == radioButton2 || radioButton == radioButton3 || radioButton == radioButton4)
            {
                ResetConvertFromInputsAndFocus();
            }

            UpdateConvertToFromActiveInput();
        }

        private void ApplyUnitGroupVisibility()
        {
            ShowOnlySelectedGroup(
                radioButton1.Checked ? grpFromSqm :
                radioButton2.Checked ? grpFromSqft :
                radioButton3.Checked ? grpFromRAPD :
                grpFromBKD,
                grpFromSqm,
                grpFromSqft,
                grpFromRAPD,
                grpFromBKD);

            ShowOnlySelectedGroup(
                radioButton8.Checked ? grpSqm :
                radioButton7.Checked ? grpToSqft :
                radioButton6.Checked ? grpToRAPD :
                grpToBKD,
                grpSqm,
                grpToSqft,
                grpToRAPD,
                grpToBKD);
        }

        private static void ShowOnlySelectedGroup(GroupBox selectedGroup, params GroupBox[] allGroups)
        {
            foreach (GroupBox group in allGroups)
            {
                group.Visible = group == selectedGroup;
            }

            selectedGroup.BringToFront();
        }

        private void InitializePrecisionValidation()
        {
            nudOtherUnitPrecision.Minimum = 0;
            nudOtherUnitPrecision.Maximum = MaxOtherUnitPrecision;
            ValidateOtherUnitPrecisionRange();
            UpdateTraditionalPrecisionBounds();

            nudTraditionalUnitPrecision.Minimum = MinTraditionalPrecision;
            ValidatePrecisionRange();
        }

        private void nudOtherUnitPrecision_ValueChanged(object? sender, EventArgs e)
        {
            ValidateOtherUnitPrecisionRange();
            UpdateTraditionalPrecisionBounds();
            ApplyTraditionalPrecisionFormatting();
            ApplyOtherUnitPrecisionFormatting();
            UpdateConvertToFromActiveInput();
        }

        private void nudOtherUnitPrecision_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void nudPrecision_ValueChanged(object? sender, EventArgs e)
        {
            ValidatePrecisionRange();
            ApplyTraditionalPrecisionFormatting();
            UpdateConvertToFromActiveInput();
        }

        private void nudPrecision_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                ShowMessage($"Precision must be between {MinTraditionalPrecision} and {GetTraditionalPrecisionMax()}.");
                return;
            }

            string currentText = nudTraditionalUnitPrecision.Text.Trim();
            string nextText = string.IsNullOrEmpty(currentText) || currentText == "0"
                ? e.KeyChar.ToString()
                : currentText + e.KeyChar;

            if (!int.TryParse(nextText, out int nextValue)
                || nextValue < MinTraditionalPrecision
                || nextValue > GetTraditionalPrecisionMax())
            {
                e.Handled = true;
                ShowMessage($"Precision must be between {MinTraditionalPrecision} and {GetTraditionalPrecisionMax()}.");
            }
        }

        private void InitializeConvertSectionOutputReadOnly()
        {
            foreach (var box in new[] { txtToSqm, txtToSqft, txtToRopanee, txtToAana, txtToPaisa, txtToDaam, txtToBigha, txtToKattha, txtToDhur })
            {
                box.ReadOnly = true;
                box.TabStop = false;
            }
        }

        private int GetTraditionalPrecision() => (int)nudTraditionalUnitPrecision.Value;

        private int GetOtherUnitPrecision() => (int)nudOtherUnitPrecision.Value;

        private int GetTraditionalPrecisionMax()
            => Math.Max(MinTraditionalPrecision, GetOtherUnitPrecision() - 1);

        private void ResetConvertFromInputsAndFocus()
        {
            _suppressConvertSectionUpdates = true;

            string formattedZero = FormatNumber(0);
            txtFromSqm.Text = formattedZero;
            txtFromSqft.Text = formattedZero;

            txtFromRopanee.Text = "0";
            txtFromAana.Text = "0";
            txtFromPaisa.Text = "0";
            txtFromDaam.Text = FormatTraditionalSubUnit(0);

            txtFromBigha.Text = "0";
            txtFromKattha.Text = "0";
            txtFromDhur.Text = FormatTraditionalSubUnit(0);

            _suppressConvertSectionUpdates = false;

            TextBox first = GetActiveFromFirstTextBox();
            first.Focus();
            first.SelectAll();
        }

        private TextBox GetActiveFromFirstTextBox()
        {
            if (radioButton2.Checked) return txtFromSqft;
            if (radioButton3.Checked) return txtFromRopanee;
            if (radioButton4.Checked) return txtFromBigha;
            return txtFromSqm;
        }

        private void FocusActiveConvertFromInput()
        {
            TextBox first = GetActiveFromFirstTextBox();
            first.Focus();
            first.SelectAll();
        }

        private void ConvertFromInput_TextChanged(object? sender, EventArgs e)
        {
            if (_suppressConvertSectionUpdates)
                return;

            UpdateConvertToFromActiveInput();
        }

        private void UpdateConvertToFromActiveInput()
        {
            double sqm = GetSqmFromConvertFromInputs();
            UpdateConvertToOutputs(sqm);
        }

        private double GetSqmFromConvertFromInputs()
        {
            if (radioButton1.Checked)
            {
                return TryParseDouble(txtFromSqm.Text.Trim(), out double sqm) ? sqm : 0;
            }

            if (radioButton2.Checked)
            {
                return TryParseDouble(txtFromSqft.Text.Trim(), out double sqft)
                    ? AreaConverterService.SqftToSqm(sqft, 9)
                    : 0;
            }

            if (radioButton3.Checked)
            {
                int ropanee = ParseIntegerOrZero(txtFromRopanee.Text);
                int aana = ParseIntegerOrZero(txtFromAana.Text);
                int paisa = ParseIntegerOrZero(txtFromPaisa.Text);
                double daam = ParseDoubleOrZero(txtFromDaam.Text);

                if (aana >= 16 || paisa >= 4 || daam >= 4)
                    return 0;

                return AreaConverterService.RAPDToSqm(ropanee, aana, paisa, daam, 9);
            }

            int bigha = ParseIntegerOrZero(txtFromBigha.Text);
            int kattha = ParseIntegerOrZero(txtFromKattha.Text);
            double dhur = ParseDoubleOrZero(txtFromDhur.Text);

            if (kattha >= 20 || dhur >= 20)
                return 0;

            return AreaConverterService.BKDToSqm(bigha, kattha, dhur, 9);
        }

        private void UpdateConvertToOutputs(double sqm)
        {
            _suppressConvertSectionUpdates = true;

            txtToSqm.Text = FormatNumber(sqm);
            txtToSqft.Text = FormatNumber(AreaConverterService.SqmToSqft(sqm, GetOtherUnitPrecision()));

            var rapd = AreaConverterService.SqmToRAPDComponents(sqm, GetTraditionalPrecision());
            txtToRopanee.Text = rapd.Ropani.ToString(CultureInfo.InvariantCulture);
            txtToAana.Text = rapd.Aana.ToString(CultureInfo.InvariantCulture);
            txtToPaisa.Text = rapd.Paisa.ToString(CultureInfo.InvariantCulture);
            txtToDaam.Text = FormatTraditionalSubUnit(rapd.Dam);

            var bkd = AreaConverterService.SqmToBKDComponents(sqm, GetTraditionalPrecision());
            txtToBigha.Text = bkd.Bigha.ToString(CultureInfo.InvariantCulture);
            txtToKattha.Text = bkd.Kattha.ToString(CultureInfo.InvariantCulture);
            txtToDhur.Text = FormatTraditionalSubUnit(bkd.Dhur);

            _suppressConvertSectionUpdates = false;
        }

        private static int ParseIntegerOrZero(string? text)
        {
            return int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                ? Math.Max(0, value)
                : 0;
        }

        private static double ParseDoubleOrZero(string? text)
        {
            return TryParseDouble(text?.Trim() ?? string.Empty, out double value)
                ? Math.Max(0, value)
                : 0;
        }

        private string FormatTraditionalSubUnit(double value)
            => value.ToString($"F{GetTraditionalPrecision()}", CultureInfo.InvariantCulture);

        private void ApplyTraditionalPrecisionFormatting()
        {
            _suppressTextValidation = true;

            if (AreaConverterService.ParseRAPDToSqm(txtRapd.Text, 9) is double rapdSqm)
            {
                txtRapd.Text = AreaConverterService.SqmToRAPDString(rapdSqm, damPrecision: GetTraditionalPrecision());
            }

            if (AreaConverterService.ParseBKDToSqm(txtBkd.Text, 9) is double bkdSqm)
            {
                txtBkd.Text = AreaConverterService.SqmToBKDString(bkdSqm, dhurPrecision: GetTraditionalPrecision());
            }

            _lastValidRapdInput = txtRapd.Text;
            _lastValidBkdInput = txtBkd.Text;
            _suppressTextValidation = false;

            txtFromDaam.Text = FormatTraditionalSubUnit(ParseDoubleOrZero(txtFromDaam.Text));
            txtFromDhur.Text = FormatTraditionalSubUnit(ParseDoubleOrZero(txtFromDhur.Text));
            txtToDaam.Text = FormatTraditionalSubUnit(ParseDoubleOrZero(txtToDaam.Text));
            txtToDhur.Text = FormatTraditionalSubUnit(ParseDoubleOrZero(txtToDhur.Text));
        }

        private void btnConvertFromReset_Click(object? sender, EventArgs e)
        {
            ResetConvertFromInputsAndFocus();
            UpdateConvertToFromActiveInput();
        }

        private void btnCopy_Click(object? sender, EventArgs e)
        {
            string text = radioButton8.Checked
                ? txtToSqm.Text
                : radioButton7.Checked
                    ? txtToSqft.Text
                    : radioButton6.Checked
                        ? $"{txtToRopanee.Text}-{txtToAana.Text}-{txtToPaisa.Text}-{txtToDaam.Text}"
                        : $"{txtToBigha.Text}-{txtToKattha.Text}-{txtToDhur.Text}";

            Clipboard.SetText(text);
        }

        private void btnCopyToClipboard_Click(object? sender, EventArgs e)
        {
            if (_lastFocusedQuickConvertTextBox is not null)
            {
                Clipboard.SetText(_lastFocusedQuickConvertTextBox.Text);
            }
        }

        private void ValidatePrecisionRange()
        {
            decimal clamped = Math.Clamp(nudTraditionalUnitPrecision.Value, nudTraditionalUnitPrecision.Minimum, nudTraditionalUnitPrecision.Maximum);
            if (clamped != nudTraditionalUnitPrecision.Value)
            {
                nudTraditionalUnitPrecision.Value = clamped;
                ShowMessage($"Precision must be between {MinTraditionalPrecision} and {GetTraditionalPrecisionMax()}.");
            }
        }

        private void ValidateOtherUnitPrecisionRange()
        {
            decimal clamped = Math.Clamp(nudOtherUnitPrecision.Value, nudOtherUnitPrecision.Minimum, nudOtherUnitPrecision.Maximum);
            if (clamped != nudOtherUnitPrecision.Value)
            {
                nudOtherUnitPrecision.Value = clamped;
            }
        }

        private void UpdateTraditionalPrecisionBounds()
        {
            nudTraditionalUnitPrecision.Maximum = GetTraditionalPrecisionMax();
            ValidatePrecisionRange();
        }

        private void ApplyOtherUnitPrecisionFormatting()
        {
            TextBox? sourceToSkip = _lastFocusedQuickConvertTextBox;

            if (sourceToSkip is not null && TryGetSqmFromQuickConvertInput(sourceToSkip, out double sourceSqm))
            {
                UpdateAllFromSqm(sourceSqm, sourceToSkip);
                return;
            }

            if (TryParseDouble(txtSqm.Text, out double sqm))
            {
                UpdateAllFromSqm(sqm, null);
            }
        }

        private bool TryGetSqmFromQuickConvertInput(TextBox source, out double sqm)
        {
            sqm = 0;

            if (source == txtSqm)
                return TryParseDouble(txtSqm.Text.Trim(), out sqm);

            if (source == txtSqft)
            {
                if (!TryParseDouble(txtSqft.Text.Trim(), out double sqft))
                    return false;

                sqm = AreaConverterService.SqftToSqm(sqft, 9);
                return true;
            }

            if (source == txtRopani)
            {
                if (!TryParseDouble(txtRopani.Text.Trim(), out double ropani))
                    return false;

                sqm = AreaConverterService.RopaniToSqm(ropani, 9);
                return true;
            }

            if (source == txtAana)
            {
                if (!TryParseDouble(txtAana.Text.Trim(), out double aana))
                    return false;

                sqm = AreaConverterService.AanaToSqm(aana, 9);
                return true;
            }

            if (source == txtPaisa)
            {
                if (!TryParseDouble(txtPaisa.Text.Trim(), out double paisa))
                    return false;

                sqm = AreaConverterService.PaisaToSqm(paisa, 9);
                return true;
            }

            if (source == txtDam)
            {
                if (!TryParseDouble(txtDam.Text.Trim(), out double dam))
                    return false;

                sqm = AreaConverterService.DamToSqm(dam, 9);
                return true;
            }

            if (source == txtBigha)
            {
                if (!TryParseDouble(txtBigha.Text.Trim(), out double bigha))
                    return false;

                sqm = AreaConverterService.BighaToSqm(bigha, 9);
                return true;
            }

            if (source == txtKattha)
            {
                if (!TryParseDouble(txtKattha.Text.Trim(), out double kattha))
                    return false;

                sqm = AreaConverterService.KatthaToSqm(kattha, 9);
                return true;
            }

            if (source == txtDhur)
            {
                if (!TryParseDouble(txtDhur.Text.Trim(), out double dhur))
                    return false;

                sqm = AreaConverterService.DhurToSqm(dhur, 9);
                return true;
            }

            if (source == txtRapd)
            {
                double? rapdSqm = AreaConverterService.ParseRAPDToSqm(txtRapd.Text.Trim(), 9);
                if (!rapdSqm.HasValue)
                    return false;

                sqm = rapdSqm.Value;
                return true;
            }

            if (source == txtBkd)
            {
                double? bkdSqm = AreaConverterService.ParseBKDToSqm(txtBkd.Text.Trim(), 9);
                if (!bkdSqm.HasValue)
                    return false;

                sqm = bkdSqm.Value;
                return true;
            }

            return false;
        }

        private void lblConvertFrom_Click(object sender, EventArgs e)
        {

        }

        private void lblProjectTitle_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBigha_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnReset_Click_1(object sender, EventArgs e)
        {

        }

        private void txtPaisa_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtSqm_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
