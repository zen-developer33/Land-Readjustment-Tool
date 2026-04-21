using Land_Readjustment_Tool.Services;
using System.Globalization;

namespace Land_Readjustment_Tool
{
    /// <summary>
    /// A compact calculator window that inherits the theme of the calling form.
    /// On OK the result is placed in the caller's active numeric input and clipboard.
    /// On Cancel (or X) nothing is transferred.
    /// </summary>
    public partial class frmCalculator : Form
    {
        private readonly CalculatorService _calc = new();

        /// <summary>Numeric result after the user presses OK. Null if cancelled.</summary>
        public decimal? Result { get; private set; }

        // ── Theme colours ────────────────────────────────────────────────────────
        private static readonly Color DarkBackground   = Color.FromArgb(20,  20,  20);
        private static readonly Color DarkSurface      = Color.FromArgb(28,  28,  28);
        private static readonly Color DarkDisplay      = Color.FromArgb(14,  14,  14);
        private static readonly Color DarkForeground   = Color.FromArgb(230, 230, 230);
        private static readonly Color DarkOperator     = Color.FromArgb(79,  145, 214);
        private static readonly Color DarkEquals       = Color.FromArgb(67,  167, 114);
        private static readonly Color DarkOk           = Color.FromArgb(67,  167, 114);
        private static readonly Color DarkButtonHover  = Color.FromArgb(45,  45,  45);
        private static readonly Color DarkClear        = Color.FromArgb(204, 85,  85);

        private static readonly Color LightBackground  = Color.FromArgb(249, 249, 249);
        private static readonly Color LightSurface     = Color.FromArgb(255, 255, 255);
        private static readonly Color LightDisplay     = Color.FromArgb(245, 245, 245);
        private static readonly Color LightForeground  = Color.FromArgb(30,  30,  30);
        private static readonly Color LightOperator    = Color.FromArgb(79,  145, 214);
        private static readonly Color LightEquals      = Color.FromArgb(67,  167, 114);
        private static readonly Color LightOk          = Color.FromArgb(67,  167, 114);
        private static readonly Color LightClear       = Color.FromArgb(204, 85,  85);

        private readonly bool _isDark;

        public frmCalculator(bool isDarkTheme = false)
        {
            _isDark = isDarkTheme;
            InitializeComponent();
            ApplyTheme();
            RefreshDisplay();
        }

        // ── Theme application ────────────────────────────────────────────────────

        private void ApplyTheme()
        {
            Color bg       = _isDark ? DarkBackground  : LightBackground;
            Color surface  = _isDark ? DarkSurface     : LightSurface;
            Color display  = _isDark ? DarkDisplay     : LightDisplay;
            Color fg       = _isDark ? DarkForeground  : LightForeground;
            Color opColor  = _isDark ? DarkOperator    : LightOperator;
            Color eqColor  = _isDark ? DarkEquals      : LightEquals;
            Color okColor  = _isDark ? DarkOk          : LightOk;
            Color clrColor = _isDark ? DarkClear       : LightClear;

            BackColor                    = bg;
            txtDisplay.BackColor         = display;
            txtDisplay.ForeColor         = fg;

            // Digit buttons
            foreach (var btn in new[] { btn0, btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8, btn9, btnDot, btnSign })
            {
                btn.BackColor  = surface;
                btn.ForeColor  = fg;
                btn.FlatAppearance.BorderColor     = _isDark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(200, 200, 200);
                btn.FlatAppearance.MouseOverBackColor = _isDark ? DarkButtonHover : Color.FromArgb(220, 220, 220);
            }

            // Operator buttons
            foreach (var btn in new[] { btnAdd, btnSub, btnMul, btnDiv })
            {
                btn.BackColor  = opColor;
                btn.ForeColor  = Color.White;
                btn.FlatAppearance.BorderColor     = opColor;
                btn.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(opColor, 0.1f);
            }

            btnEquals.BackColor = eqColor;
            btnEquals.ForeColor = Color.White;
            btnEquals.FlatAppearance.BorderColor = eqColor;
            btnEquals.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(eqColor, 0.1f);

            btnClear.BackColor = clrColor;
            btnClear.ForeColor = Color.White;
            btnClear.FlatAppearance.BorderColor = clrColor;
            btnClear.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(clrColor, 0.1f);

            btnBackspace.BackColor = surface;
            btnBackspace.ForeColor = _isDark ? Color.FromArgb(230, 130, 130) : Color.FromArgb(180, 50, 50);
            btnBackspace.FlatAppearance.BorderColor = _isDark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(200, 200, 200);
            btnBackspace.FlatAppearance.MouseOverBackColor = _isDark ? DarkButtonHover : Color.FromArgb(220, 220, 220);

            btnOk.BackColor = okColor;
            btnOk.ForeColor = Color.White;
            btnOk.FlatAppearance.BorderColor = okColor;
            btnOk.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(okColor, 0.1f);

            btnCancel.BackColor = surface;
            btnCancel.ForeColor = fg;
            btnCancel.FlatAppearance.BorderColor = _isDark ? Color.FromArgb(70, 70, 70) : Color.FromArgb(200, 200, 200);
            btnCancel.FlatAppearance.MouseOverBackColor = _isDark ? DarkButtonHover : Color.FromArgb(220, 220, 220);
        }

        // ── Display ──────────────────────────────────────────────────────────────

        private void RefreshDisplay()
        {
            txtDisplay.Text = _calc.Display;
        }

        // ── Button event handlers ────────────────────────────────────────────────

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

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            Result = _calc.GetResult();

            if (Result.HasValue)
            {
                string resultText = Result.Value.ToString(CultureInfo.InvariantCulture);
                Clipboard.SetText(resultText);
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

        // ── Keyboard support ─────────────────────────────────────────────────────

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
                case Keys.Add:      case Keys.Oemplus | Keys.Shift: _calc.ApplyOperator('+'); RefreshDisplay(); return true;
                case Keys.Subtract: case Keys.OemMinus: _calc.ApplyOperator('-'); RefreshDisplay(); return true;
                case Keys.Multiply: _calc.ApplyOperator('*'); RefreshDisplay(); return true;
                case Keys.Divide:   case Keys.OemQuestion: _calc.ApplyOperator('/'); RefreshDisplay(); return true;
                case Keys.Enter: _calc.Equals(); RefreshDisplay(); return true;
                case Keys.Back:     _calc.Backspace(); RefreshDisplay(); return true;
                case Keys.Escape:   BtnCancel_Click(null, EventArgs.Empty); return true;
                case Keys.F2:       BtnOk_Click(null, EventArgs.Empty); return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
