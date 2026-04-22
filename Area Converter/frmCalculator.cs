using Land_Readjustment_Tool.Services;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Land_Readjustment_Tool
{
    public partial class frmCalculator : Form
    {
        private readonly CalculatorService _calc = new();

        public decimal? Result { get; private set; }

        // ── Theme colours aligned with the main form ──────────────────────────────

        private static readonly Color DarkBackground = Color.FromArgb(45, 45, 48);
        private static readonly Color DarkSurface = Color.FromArgb(37, 37, 38);
        private static readonly Color DarkDisplay = Color.FromArgb(30, 30, 31);
        private static readonly Color DarkForeground = Color.FromArgb(220, 220, 220);
        private static readonly Color DarkOperator = Color.FromArgb(0, 122, 204);
        private static readonly Color DarkOperatorHover = Color.FromArgb(18, 139, 230);
        private static readonly Color DarkEquals = Color.FromArgb(0, 153, 102);
        private static readonly Color DarkEqualsHover = Color.FromArgb(18, 171, 120);
        private static readonly Color DarkOk = Color.FromArgb(0, 153, 102);
        private static readonly Color DarkOkHover = Color.FromArgb(18, 171, 120);
        private static readonly Color DarkClear = Color.FromArgb(204, 85, 85);
        private static readonly Color DarkClearHover = Color.FromArgb(220, 104, 104);
        private static readonly Color DarkButtonHover = Color.FromArgb(53, 53, 56);

        private static readonly Color LightBackground = Color.White;
        private static readonly Color LightSurface = SystemColors.Control;
        private static readonly Color LightDisplay = Color.White;
        private static readonly Color LightForeground = Color.Black;
        private static readonly Color LightOperator = Color.FromArgb(0, 122, 204);
        private static readonly Color LightOperatorHover = Color.FromArgb(26, 141, 224);
        private static readonly Color LightEquals = Color.FromArgb(0, 153, 102);
        private static readonly Color LightEqualsHover = Color.FromArgb(18, 171, 120);
        private static readonly Color LightOk = Color.FromArgb(0, 153, 102);
        private static readonly Color LightOkHover = Color.FromArgb(18, 171, 120);
        private static readonly Color LightClear = Color.FromArgb(204, 85, 85);
        private static readonly Color LightClearHover = Color.FromArgb(220, 104, 104);
        private static readonly Color LightButtonHover = SystemColors.ControlLight;

        private readonly bool _isDark;

        public frmCalculator(bool isDarkTheme = false)
        {
            _isDark = isDarkTheme;
            InitializeComponent();
            ApplyTheme();
            RefreshDisplay();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTitleBarTheme();
        }

        // ── Theme ─────────────────────────────────────────────────────────────────

        private void ApplyTheme()
        {
            Color bg = _isDark ? DarkBackground : LightBackground;
            Color surface = _isDark ? DarkSurface : LightSurface;
            Color display = _isDark ? DarkDisplay : LightDisplay;
            Color fg = _isDark ? DarkForeground : LightForeground;
            Color opColor = _isDark ? DarkOperator : LightOperator;
            Color opHover = _isDark ? DarkOperatorHover : LightOperatorHover;
            Color eqColor = _isDark ? DarkEquals : LightEquals;
            Color eqHover = _isDark ? DarkEqualsHover : LightEqualsHover;
            Color okColor = _isDark ? DarkOk : LightOk;
            Color okHover = _isDark ? DarkOkHover : LightOkHover;
            Color clrColor = _isDark ? DarkClear : LightClear;
            Color clrHover = _isDark ? DarkClearHover : LightClearHover;
            Color border = _isDark ? Color.FromArgb(90, 90, 95) : Color.FromArgb(210, 215, 225);
            Color digitHover = _isDark ? DarkButtonHover : LightButtonHover;

            BackColor = bg;
            ForeColor = fg;

            txtResult.BackColor = display;
            txtResult.ForeColor = fg;
            lblCalculationStep.BackColor = display;
            lblCalculationStep.ForeColor = _isDark
                ? Color.FromArgb(160, 160, 165)
                : Color.FromArgb(95, 95, 95);

            foreach (var btn in new[] { btn0, btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8, btn9, btnDot, btnSign })
            {
                btn.BackColor = surface;
                btn.ForeColor = fg;
                btn.FlatAppearance.BorderColor = border;
                btn.FlatAppearance.MouseOverBackColor = digitHover;
            }

            foreach (var btn in new[] { btnAdd, btnSub, btnMul, btnDiv })
            {
                btn.BackColor = opColor;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = opColor;
                btn.FlatAppearance.MouseOverBackColor = opHover;
            }

            btnEquals.BackColor = eqColor;
            btnEquals.ForeColor = Color.White;
            btnEquals.FlatAppearance.BorderColor = eqColor;
            btnEquals.FlatAppearance.MouseOverBackColor = eqHover;

            btnClear.BackColor = clrColor;
            btnClear.ForeColor = Color.White;
            btnClear.FlatAppearance.BorderColor = clrColor;
            btnClear.FlatAppearance.MouseOverBackColor = clrHover;

            btnBackspace.BackColor = surface;
            btnBackspace.ForeColor = _isDark
                ? Color.FromArgb(235, 147, 147)
                : Color.FromArgb(180, 70, 70);
            btnBackspace.FlatAppearance.BorderColor = border;
            btnBackspace.FlatAppearance.MouseOverBackColor = digitHover;

            btnOk.BackColor = surface;
            btnOk.ForeColor = fg;
            btnOk.FlatAppearance.BorderColor = border;
            btnOk.FlatAppearance.MouseOverBackColor = digitHover;

            btnCancel.BackColor = surface;
            btnCancel.ForeColor = fg;
            btnCancel.FlatAppearance.BorderColor = border;
            btnCancel.FlatAppearance.MouseOverBackColor = digitHover;

            ApplyTitleBarTheme();
        }

        private void ApplyTitleBarTheme()
        {
            if (!IsHandleCreated || !OperatingSystem.IsWindows())
                return;

            int useDark = _isDark ? 1 : 0;
            _ = DwmSetWindowAttribute(Handle, DwmwaUseImmersiveDarkMode, ref useDark, sizeof(int));
            _ = DwmSetWindowAttribute(Handle, DwmwaUseImmersiveDarkModeBefore20H1, ref useDark, sizeof(int));
        }

        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        // ── Display ───────────────────────────────────────────────────────────────

        private void RefreshDisplay()
        {
            txtResult.Text = _calc.Display;
            lblCalculationStep.Text = _calc.CalculationStep;
        }

        // ── Button handlers ───────────────────────────────────────────────────────

        private void BtnDigit_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                _calc.InputDigit(btn.Tag?.ToString() ?? btn.Text);
                RefreshDisplay();
            }
        }

        private void BtnOperator_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag?.ToString() is string op && op.Length == 1)
            {
                _calc.ApplyOperator(op[0]);
                RefreshDisplay();
            }
        }

        private void BtnEquals_Click(object? sender, EventArgs e)
        {
            _calc.Equals();
            RefreshDisplay();
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            _calc.Reset();
            RefreshDisplay();
        }

        private void BtnBackspace_Click(object? sender, EventArgs e)
        {
            _calc.Backspace();
            RefreshDisplay();
        }

        private void BtnSign_Click(object? sender, EventArgs e)
        {
            _calc.ToggleSign();
            RefreshDisplay();
        }

        private void BtnReturn_Click(object? sender, EventArgs e)
        {
            Result = _calc.GetResult();

            if (Result.HasValue)
            {
                string resultText = Result.Value.ToString(CultureInfo.InvariantCulture);
                Clipboard.SetText(resultText);
            }

            if (Result < 0)
            {
                if (MessageBox.Show(
                    "Negative value. The result will not be returned.",
                    "Invalid Result",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning) == DialogResult.Cancel)
                    return;

                Result = null;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            Result = null;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        // ── Keyboard support ──────────────────────────────────────────────────────

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.D0: case Keys.NumPad0: _calc.InputDigit("0"); RefreshDisplay(); return true;
                case Keys.D1: case Keys.NumPad1: _calc.InputDigit("1"); RefreshDisplay(); return true;
                case Keys.D2: case Keys.NumPad2: _calc.InputDigit("2"); RefreshDisplay(); return true;
                case Keys.D3: case Keys.NumPad3: _calc.InputDigit("3"); RefreshDisplay(); return true;
                case Keys.D4: case Keys.NumPad4: _calc.InputDigit("4"); RefreshDisplay(); return true;
                case Keys.D5: case Keys.NumPad5: _calc.InputDigit("5"); RefreshDisplay(); return true;
                case Keys.D6: case Keys.NumPad6: _calc.InputDigit("6"); RefreshDisplay(); return true;
                case Keys.D7: case Keys.NumPad7: _calc.InputDigit("7"); RefreshDisplay(); return true;
                case Keys.D8: case Keys.NumPad8: _calc.InputDigit("8"); RefreshDisplay(); return true;
                case Keys.D9: case Keys.NumPad9: _calc.InputDigit("9"); RefreshDisplay(); return true;
                case Keys.OemPeriod: case Keys.Decimal: _calc.InputDigit("."); RefreshDisplay(); return true;
                case Keys.Add: case Keys.Oemplus | Keys.Shift: _calc.ApplyOperator('+'); RefreshDisplay(); return true;
                case Keys.Subtract: case Keys.OemMinus: _calc.ApplyOperator('-'); RefreshDisplay(); return true;
                case Keys.Multiply: _calc.ApplyOperator('*'); RefreshDisplay(); return true;
                case Keys.Divide: case Keys.OemQuestion: _calc.ApplyOperator('/'); RefreshDisplay(); return true;
                case Keys.Enter: _calc.Equals(); RefreshDisplay(); return true;
                case Keys.Back: _calc.Backspace(); RefreshDisplay(); return true;
                case Keys.Escape: BtnCancel_Click(null, EventArgs.Empty); return true;
                case Keys.F2: BtnReturn_Click(null, EventArgs.Empty); return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ── Context menu (right-click on display) ─────────────────────────────────

        private void copyToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            string text = txtResult.Text.Trim();
            if (text.Length > 0)
                Clipboard.SetText(text);
        }

        private void pasteToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (!Clipboard.ContainsText())
                return;

            string pasted = Clipboard.GetText().Trim();
            if (!IsValidPastedNumber(pasted))
                return;

            if (_calc.TrySetDisplay(pasted))
                RefreshDisplay();
        }

        private static bool IsValidPastedNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            bool hasDot = false;
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsDigit(c)) continue;
                if (c == '.' && !hasDot) { hasDot = true; continue; }
                if (c == '-' && i == 0) continue;
                return false;
            }

            return input != "." && input != "-" && input != "-.";
        }
    }
}