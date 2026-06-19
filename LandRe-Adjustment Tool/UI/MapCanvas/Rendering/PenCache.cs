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
            return Get(color, width, NormalizeDashStyle(dashStyle), lineTypeScale);
        }

        public Pen Get(
            Color color,
            float width,
            string? lineStyle,
            float lineTypeScale = 1.0f)
        {
            width = Math.Max(0.1f, width);
            lineTypeScale = Math.Clamp(lineTypeScale, 0.1f, 100.0f);
            string lineStyleKey = NormalizeLineStyleKey(lineStyle);
            PenKey key = new(color.ToArgb(), width, lineStyleKey, lineTypeScale);
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
            ApplyLineStyle(pen, lineStyleKey, lineTypeScale);
            _pens[key] = pen;
            return pen;
        }

        private static void ApplyLineStyle(Pen pen, string lineStyleKey, float scale)
        {
            switch (lineStyleKey)
            {
                case "DASHED":
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale];
                    break;
                case "DOTTED":
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Round;
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    pen.DashPattern = [0.1f, Math.Max(1.5f, 2f * scale)];
                    break;
                case "DASHDOT":
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case "DASHDOUBLEDOT":
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [4f * scale, 2f * scale, 1f * scale, 2f * scale, 1f * scale, 2f * scale];
                    break;
                case "CENTERLINE":
                    pen.DashStyle = DashStyle.Custom;
                    pen.DashCap = DashCap.Flat;
                    pen.DashPattern = [8f * scale, 3f * scale, 2f * scale, 3f * scale];
                    break;
                default:
                    pen.DashStyle = DashStyle.Solid;
                    break;
            }
        }

        private static string NormalizeDashStyle(DashStyle dashStyle)
        {
            return dashStyle switch
            {
                DashStyle.Dash => "DASHED",
                DashStyle.Dot => "DOTTED",
                DashStyle.DashDot => "DASHDOT",
                DashStyle.DashDotDot => "DASHDOUBLEDOT",
                _ => "SOLID"
            };
        }

        private static string NormalizeLineStyleKey(string? lineStyle)
        {
            string normalized = (lineStyle ?? string.Empty)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant();

            return normalized switch
            {
                "DASH" => "DASHED",
                "DOT" => "DOTTED",
                "DASHDOTDOT" => "DASHDOUBLEDOT",
                _ => normalized
            };
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
            string LineStyleKey,
            float LineTypeScale);
    }
}
