using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class PenCache : IDisposable
    {
        private readonly Dictionary<PenKey, Pen> _pens = [];

        public Pen Get(Color color, float width, DashStyle dashStyle = DashStyle.Solid)
        {
            width = Math.Max(0.1f, width);
            PenKey key = new(color.ToArgb(), width, dashStyle);
            if (_pens.TryGetValue(key, out Pen? pen))
            {
                return pen;
            }

            pen = new Pen(color, width)
            {
                DashStyle = dashStyle,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            _pens[key] = pen;
            return pen;
        }

        public void Dispose()
        {
            foreach (Pen pen in _pens.Values)
                pen.Dispose();

            _pens.Clear();
        }

        private readonly record struct PenKey(int ColorArgb, float Width, DashStyle DashStyle);
    }
}
