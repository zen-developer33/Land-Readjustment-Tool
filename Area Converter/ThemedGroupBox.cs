using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace Land_Readjustment_Tool
{
    /// <summary>
    /// A GroupBox that fully owner-draws itself, allowing the border and text
    /// colour to be set explicitly. Drop this into the designer in place of the
    /// standard GroupBox — all existing properties (Text, Font, BackColor,
    /// ForeColor) continue to work exactly as before.
    /// </summary>
    public class ThemedGroupBox : GroupBox
    {
        private Color _borderColor = SystemColors.Control;

        /// <summary>Gets or sets the colour of the border rectangle.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor == value) return;
                _borderColor = value;
                Invalidate();
            }
        }

        public ThemedGroupBox()
        {
            // Tell Windows we are painting everything ourselves.
            // Without this the OS still draws the etched border first.
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Do NOT call base.OnPaint — that is what draws the unwanted system border.

            Graphics g      = e.Graphics;
            Rectangle rect  = ClientRectangle;

            // ── Measure the title ────────────────────────────────────────────────
            Size  titleSize = TextRenderer.MeasureText(Text, Font);
            int   titleH    = titleSize.Height;
            int   titleX    = 8;                        // left indent matches Windows default

            // The border rectangle starts halfway down the title text height
            Rectangle borderRect = new(
                rect.Left,
                rect.Top + titleH / 2,
                rect.Width - 1,
                rect.Height - titleH / 2 - 1);

            // ── Fill background ──────────────────────────────────────────────────
            using (SolidBrush bgBrush = new(BackColor))
                g.FillRectangle(bgBrush, rect);

            // ── Draw border ──────────────────────────────────────────────────────
            using (Pen pen = new(_borderColor, 1f))
                g.DrawRectangle(pen, borderRect);

            // ── Blank out the area behind the title text ─────────────────────────
            // This cuts the border line so the text sits "in" the border, just like
            // the native GroupBox renders.0.5
            Rectangle textBgRect = new(titleX, rect.Top, titleSize.Width, titleH);
            using (SolidBrush bgBrush = new(BackColor))
                g.FillRectangle(bgBrush, textBgRect);

            // ── Draw title text ──────────────────────────────────────────────────
            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Point(titleX, rect.Top),
                ForeColor);
        }
    }
}
