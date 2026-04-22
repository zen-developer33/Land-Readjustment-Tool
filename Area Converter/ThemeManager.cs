using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Land_Readjustment_Tool
{
    public static class ThemeManager
    {
        private static readonly Color DarkBackgroundColor = Color.FromArgb(45, 45, 48);
        private static readonly Color DarkControlColor = Color.FromArgb(37, 37, 38);
        private static readonly Color DarkForegroundColor = Color.FromArgb(220, 220, 220);
        private static readonly Color DarkButtonColor = Color.FromArgb(62, 62, 64);
        private static readonly Color DarkButtonBorder = Color.FromArgb(90, 90, 95);
        private static readonly Color DarkButtonHover = Color.FromArgb(80, 80, 84);
        private static readonly Color DarkButtonPressed = Color.FromArgb(0, 122, 204);
        private static readonly Color DarkFocusedTextBoxColor = Color.FromArgb(60, 75, 95);
        private static readonly Color DarkGroupBoxBorder = Color.FromArgb(64, 64, 64) ;

        private static readonly Color LightBackgroundColor = Color.White;
        private static readonly Color LightControlColor = Color.White;
        private static readonly Color LightForegroundColor = Color.Black;
        private static readonly Color LightButtonColor = SystemColors.Control;
        private static readonly Color LightGroupBoxBorder = SystemColors.ControlLight;
        private static readonly Color LightFocusedTextBoxColor = Color.FromArgb(255, 251, 235);

        public enum Theme { Light, Dark }

        public static void ApplyTheme(Control control, Theme theme)
        {
            if (theme == Theme.Dark)
            {
                ApplyDarkTheme(control);
                return;
            }

            ApplyLightTheme(control);
        }

        public static Color GetTextBoxDefaultBackColor(Theme theme)
            => theme == Theme.Dark ? DarkControlColor : LightControlColor;

        public static Color GetTextBoxFocusBackColor(Theme theme)
            => theme == Theme.Dark ? DarkFocusedTextBoxColor : LightFocusedTextBoxColor;

        public static Color GetQuickConvertHighlightColor(Theme theme)
            => GetTextBoxFocusBackColor(theme);

        // ── Dark theme ────────────────────────────────────────────────────────────

        private static void ApplyDarkTheme(Control control)
        {
            switch (control)
            {
                case Form form:
                    form.BackColor = DarkBackgroundColor;
                    form.ForeColor = DarkForegroundColor;
                    break;

                case TextBox textBox:
                    textBox.BackColor = DarkControlColor;
                    textBox.ForeColor = DarkForegroundColor;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case Button button:
                    button.UseVisualStyleBackColor = false;
                    button.BackColor = DarkButtonColor;
                    button.ForeColor = DarkForegroundColor;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderColor = DarkButtonBorder;
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatAppearance.MouseOverBackColor = DarkButtonHover;
                    button.FlatAppearance.MouseDownBackColor = DarkButtonPressed;
                    button.SizeChanged -= Button_SizeChanged;
                    button.SizeChanged += Button_SizeChanged;
                    button.Paint -= Button_PaintDarkRoundedBorder;
                    button.Paint += Button_PaintDarkRoundedBorder;
                    ApplyRoundedButtonRegion(button);
                    break;

                // ThemedGroupBox: set colour properties — OnPaint handles drawing.
                case ThemedGroupBox themedGroupBox:
                    themedGroupBox.BackColor = DarkBackgroundColor;
                    themedGroupBox.ForeColor = DarkForegroundColor;
                    themedGroupBox.BorderColor = DarkGroupBoxBorder;
                    
                    break;

                // Plain GroupBox fallback — should not appear if designer uses ThemedGroupBox.
                case GroupBox groupBox:
                    groupBox.BackColor = DarkBackgroundColor;
                    groupBox.ForeColor = DarkForegroundColor;
                    break;

                case Label label:
                    label.BackColor = DarkBackgroundColor;
                    label.ForeColor = DarkForegroundColor;
                    break;

                case RadioButton radioButton:
                    radioButton.BackColor = DarkBackgroundColor;
                    radioButton.ForeColor = DarkForegroundColor;
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = DarkControlColor;
                    numericUpDown.ForeColor = DarkForegroundColor;
                    break;

                default:
                    control.BackColor = DarkBackgroundColor;
                    control.ForeColor = DarkForegroundColor;
                    break;
            }

            foreach (Control child in control.Controls)
                ApplyDarkTheme(child);
        }

        // ── Light theme ───────────────────────────────────────────────────────────

        private static void ApplyLightTheme(Control control)
        {
            switch (control)
            {
                case Form form:
                    form.BackColor = LightBackgroundColor;
                    form.ForeColor = LightForegroundColor;
                    break;

                case TextBox textBox:
                    textBox.BackColor = LightControlColor;
                    textBox.ForeColor = LightForegroundColor;
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    break;

                case Button button:
                    button.UseVisualStyleBackColor = true;
                  
                    button.ForeColor = LightForegroundColor;
                    button.FlatStyle = FlatStyle.Standard;
                    button.SizeChanged -= Button_SizeChanged;
                    button.Paint -= Button_PaintDarkRoundedBorder;
                    button.Region?.Dispose();
                    button.Region = null;
                    break;

                case ThemedGroupBox themedGroupBox:
                    themedGroupBox.BackColor = LightBackgroundColor;
                    themedGroupBox.ForeColor = LightForegroundColor;
                    themedGroupBox.BorderColor = LightGroupBoxBorder;
                    break;

                case GroupBox groupBox:
                    groupBox.BackColor = LightBackgroundColor;
                    groupBox.ForeColor = LightForegroundColor;
                    break;

                case Label label:
                    label.BackColor = LightBackgroundColor;
                    label.ForeColor = LightForegroundColor;
                    break;

                case RadioButton radioButton:
                    radioButton.BackColor = LightBackgroundColor;
                    radioButton.ForeColor = LightForegroundColor;
                    break;

                case NumericUpDown numericUpDown:
                    numericUpDown.BackColor = LightControlColor;
                    numericUpDown.ForeColor = LightForegroundColor;
                    break;

                default:
                    control.BackColor = LightBackgroundColor;
                    control.ForeColor = LightForegroundColor;
                    break;
            }

            foreach (Control child in control.Controls)
                ApplyLightTheme(child);
        }

        // ── Button helpers ────────────────────────────────────────────────────────

        private static void Button_SizeChanged(object? sender, EventArgs e)
        {
            if (sender is Button button)
                ApplyRoundedButtonRegion(button);
        }

        private static void ApplyRoundedButtonRegion(Button button)
        {
            const int radius = 3;
            int diameter = radius * 2;
            Rectangle rect = new(0, 0, button.Width, button.Height);

            using GraphicsPath path = new();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            button.Region?.Dispose();
            button.Region = new Region(path);
        }

        private static void Button_PaintDarkRoundedBorder(object? sender, PaintEventArgs e)
        {
            if (sender is not Button button || button.Width <= 1 || button.Height <= 1)
                return;

            const int radius = 4;
            int diameter = radius * 2;
            Rectangle rect = new(0, 0, button.Width - 1, button.Height - 1);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using GraphicsPath path = new();
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            using Pen pen = new(button.FlatAppearance.BorderColor, 1f);
            e.Graphics.DrawPath(pen, path);
        }
    }
}