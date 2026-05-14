using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class PenCache : IDisposable
    {
        private readonly Dictionary<PenKey, Pen> _pens = new Dictionary<PenKey, Pen>();

        public Pen Get(
            Color color,
            float width,
            DashStyle dashStyle = DashStyle.Solid,
            float lineTypeScale = 1.0f)
        {
            width = Math.Max(0.1f, width);
            lineTypeScale = Math.Clamp(lineTypeScale, 0.1f, 100.0f);
            PenKey key = new(color.ToArgb(), width, dashStyle, lineTypeScale);
            if (_pens.TryGetValue(key, out Pen? pen))
            {
                return pen;
            }

            pen = new Pen(color, width)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            ApplyDashStyle(pen, dashStyle, lineTypeScale);
            _pens[key] = pen;
            return pen;
        }

        private static void ApplyDashStyle(Pen pen, DashStyle dashStyle, float scale)
        {
            switch (dashStyle)
            {
                case DashStyle.Dash:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = [4f * scale, 2f * scale];
                    break;
                case DashStyle.Dot:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = [1f * scale, 2f * scale];
                    break;
                case DashStyle.DashDot:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case DashStyle.DashDotDot:
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashPattern = [8f * scale, 3f * scale, 2f * scale, 3f * scale];
                    break;
                default:
                    pen.DashStyle = DashStyle.Solid;
                    break;
            }
        }

        public void Dispose()
        {
            foreach (Pen pen in _pens.Values)
                pen.Dispose();

            _pens.Clear();
        }

        private readonly record struct PenKey(
            int ColorArgb,
            float Width,
            DashStyle DashStyle,
            float LineTypeScale);
    }
}
