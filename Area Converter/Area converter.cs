using Land_Readjustment_Tool.Services;
using System.Globalization;

namespace Land_Readjustment_Tool
{
    public partial class frmAreaConverter : Form
    {
        private const int DisplayPrecision = 3;

        private readonly Dictionary<TextBox, string> _lastValidNumericText = new();
        private string _lastValidRapdInput = string.Empty;
        private string _lastValidBkdInput = string.Empty;
        private bool _suppressTextValidation;

        public frmAreaConverter()
        {
            InitializeComponent();
            HookValidationEvents();
        }

        private void frmAreaConverter_Load(object sender, EventArgs e)
        {
            ResetValues();
        }

        private void HookValidationEvents()
        {
            foreach (var box in new[] { txtSqm, txtSqft, txtRopani, txtAana, txtPaisa, txtDam, txtRapd, txtBigha, txtKattha, txtDhur, txtBkd })
            {
                box.Enter += TextBox_EnterSelectAll;
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
            btnExit.Click += (_, _) => Close();

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

        private void ResetValues()
        {
            _suppressTextValidation = true;

            txtSqm.Text = "0";
            txtSqft.Text = "0";
            txtRopani.Text = "0";
            txtAana.Text = "0";
            txtPaisa.Text = "0";
            txtDam.Text = "0";
            txtRapd.Text = "0-0-0-0.00";
            txtBigha.Text = "0";
            txtKattha.Text = "0";
            txtDhur.Text = "0";
            txtBkd.Text = "0-0-0.00";

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
                _lastValidRapdInput = "0-0-0-0.00";
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
                _lastValidBkdInput = "0-0-0.00";
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

        private void UpdateAllFromSqm(double sqm, TextBox source)
        {
            _suppressTextValidation = true;

            if (source != txtSqm) txtSqm.Text = FormatNumber(sqm);
            if (source != txtSqft) txtSqft.Text = FormatNumber(AreaConverterService.SqmToSqft(sqm, DisplayPrecision));
            if (source != txtRopani) txtRopani.Text = FormatNumber(AreaConverterService.SqmToRopani(sqm, DisplayPrecision));
            if (source != txtAana) txtAana.Text = FormatNumber(AreaConverterService.SqmToAana(sqm, DisplayPrecision));
            if (source != txtPaisa) txtPaisa.Text = FormatNumber(AreaConverterService.SqmToPaisa(sqm, DisplayPrecision));
            if (source != txtDam) txtDam.Text = FormatNumber(AreaConverterService.SqmToDam(sqm, DisplayPrecision));
            if (source != txtRapd) txtRapd.Text = AreaConverterService.SqmToRAPDString(sqm, damPrecision: 2);
            if (source != txtBigha) txtBigha.Text = FormatNumber(AreaConverterService.SqmToBigha(sqm, DisplayPrecision));
            if (source != txtKattha) txtKattha.Text = FormatNumber(AreaConverterService.SqmToKattha(sqm, DisplayPrecision));
            if (source != txtDhur) txtDhur.Text = FormatNumber(AreaConverterService.SqmToDhur(sqm, DisplayPrecision));
            if (source != txtBkd) txtBkd.Text = AreaConverterService.SqmToBKDString(sqm, dhurPrecision: 2);

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

        private static string FormatNumber(double value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

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

        private void lblConvertFrom_Click(object sender, EventArgs e)
        {

        }

        private void lblProjectTitle_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
