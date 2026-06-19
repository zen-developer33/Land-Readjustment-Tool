using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    /// <summary>
    /// A <see cref="ContextMenuStrip"/>/<see cref="ToolStrip"/> renderer that draws
    /// a small, clean checkmark for checked menu items instead of the default
    /// boxed check glyph. This gives context menus a lighter, modern look while
    /// leaving every other aspect of rendering to the professional renderer.
    /// </summary>
    /// <remarks>
    /// Use the shared <see cref="Instance"/> for all context menus so the check
    /// style stays consistent application-wide. The renderer is stateless and
    /// safe to share across multiple menus.
    /// </remarks>
    public sealed class ElegantMenuRenderer : ToolStripProfessionalRenderer
    {
        /// <summary>Shared, stateless renderer instance for all context menus.</summary>
        public static ElegantMenuRenderer Instance { get; } = new();

        // Subtle slate accent that matches the app's panel foreground color.
        private static readonly Color CheckColor = Color.FromArgb(39, 55, 77);
        private static readonly Color DisabledCheckColor = Color.FromArgb(150, 160, 170);

        public ElegantMenuRenderer()
        {
            // Square, modern edges rather than the default rounded toolstrip look.
            RoundedEdges = false;
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            // Only checked items get a glyph; unchecked draw nothing (no box).
            if (e.Item is not ToolStripMenuItem { Checked: true } menuItem)
            {
                return;
            }

            Rectangle bounds = e.ImageRectangle;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // A small square centered in the check column keeps the mark compact
            // regardless of the menu's image-margin size.
            int side = Math.Min(bounds.Width, bounds.Height);
            float glyph = Math.Max(8f, side * 0.52f);
            float half = glyph / 2f;
            float cx = bounds.Left + bounds.Width / 2f;
            float cy = bounds.Top + bounds.Height / 2f;

            // Three points forming a checkmark within the glyph box.
            PointF p1 = new(cx - half, cy + half * 0.05f);
            PointF p2 = new(cx - half * 0.28f, cy + half * 0.72f);
            PointF p3 = new(cx + half, cy - half * 0.66f);

            Color color = menuItem.Enabled ? CheckColor : DisabledCheckColor;

            Graphics g = e.Graphics;
            SmoothingMode previousSmoothing = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using Pen pen = new(color, Math.Max(1.5f, glyph * 0.16f))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawLines(pen, new[] { p1, p2, p3 });
            g.SmoothingMode = previousSmoothing;
        }
    }
}
