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

        private static readonly Color LightBackgroundColor = Color.White;
        private static readonly Color LightControlColor = Color.White;
        private static readonly Color LightForegroundColor = Color.Black;
        private static readonly Color LightButtonColor = SystemColors.Control;
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

                case GroupBox groupBox:
                    groupBox.BackColor = DarkBackgroundColor;
                    groupBox.ForeColor = DarkForegroundColor;
                    groupBox.FlatStyle = FlatStyle.Flat;
                    // Unhook first to avoid double-subscription on repeated theme calls.
                    groupBox.Paint -= GroupBox_PaintDarkBorder;
                    groupBox.Paint += GroupBox_PaintDarkBorder;
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
                    button.BackColor = LightButtonColor;
                    button.ForeColor = LightForegroundColor;
                    button.FlatStyle = FlatStyle.Standard;
                    button.SizeChanged -= Button_SizeChanged;
                    button.Paint -= Button_PaintDarkRoundedBorder;
                    button.Region?.Dispose();
                    button.Region = null;
                    break;

                case GroupBox groupBox:
                    groupBox.BackColor = LightBackgroundColor;
                    groupBox.ForeColor = LightForegroundColor;
                    // Remove dark paint handler when switching back to light.
                    groupBox.Paint -= GroupBox_PaintDarkBorder;
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

        // ── GroupBox border painter ───────────────────────────────────────────────

        /// <summary>
        /// Redraws the GroupBox border in the same DarkButtonBorder colour used by buttons,
        /// keeping the title text visible and correctly positioned.
        /// </summary>
        private static void GroupBox_PaintDarkBorder(object? sender, PaintEventArgs e)
        {
            if (sender is not GroupBox groupBox)
                return;

            Graphics g = e.Graphics;
            Rectangle bounds = groupBox.ClientRectangle;

            // Measure the title so we can leave a gap in the top border line.
            SizeF titleSize = g.MeasureString(groupBox.Text, groupBox.Font);
            int titleLeft = 8;                          // left indent of the title text
            int titleHeight = (int)(titleSize.Height / 2);
            int borderTop = titleHeight;                // vertical centre of the top border line

            using Pen borderPen = new(DarkButtonBorder, 1f);

            // Top border — left segment
            if (titleLeft > 0)
                g.DrawLine(borderPen, bounds.Left, borderTop, bounds.Left + titleLeft - 2, borderTop);

            // Top border — right segment (after the title)
            int titleRight = titleLeft + (int)titleSize.Width + 2;
            g.DrawLine(borderPen, titleRight, borderTop, bounds.Right - 1, borderTop);

            // Left, bottom, right sides
            g.DrawLine(borderPen, bounds.Left, borderTop, bounds.Left, bounds.Bottom - 1);
            g.DrawLine(borderPen, bounds.Left, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
            g.DrawLine(borderPen, bounds.Right - 1, borderTop, bounds.Right - 1, bounds.Bottom - 1);

            // Draw the title text in the foreground colour
            using SolidBrush textBrush = new(groupBox.ForeColor);
            g.DrawString(groupBox.Text, groupBox.Font, textBrush, titleLeft, 0);
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