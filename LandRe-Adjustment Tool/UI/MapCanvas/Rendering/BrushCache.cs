using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class BrushCache : IDisposable
    {
        private readonly Dictionary<int, SolidBrush> _solidBrushes = new Dictionary<int, SolidBrush>();
        private readonly Dictionary<HatchBrushKey, HatchBrush> _hatchBrushes = new Dictionary<HatchBrushKey, HatchBrush>();

        public SolidBrush GetSolid(Color color)
        {
            int key = color.ToArgb();
            if (_solidBrushes.TryGetValue(key, out SolidBrush? brush))
            {
                return brush;
            }

            brush = new SolidBrush(color);
            _solidBrushes[key] = brush;
            return brush;
        }

        public HatchBrush GetHatch(
            HatchStyle hatchStyle,
            Color foreColor,
            Color backColor)
        {
            HatchBrushKey key = new(hatchStyle, foreColor.ToArgb(), backColor.ToArgb());
            if (_hatchBrushes.TryGetValue(key, out HatchBrush? brush))
            {
                return brush;
            }

            brush = new HatchBrush(hatchStyle, foreColor, backColor);
            _hatchBrushes[key] = brush;
            return brush;
        }

        public void Dispose()
        {
            foreach (SolidBrush brush in _solidBrushes.Values)
                brush.Dispose();

            foreach (HatchBrush brush in _hatchBrushes.Values)
                brush.Dispose();

            _solidBrushes.Clear();
            _hatchBrushes.Clear();
        }

        private readonly record struct HatchBrushKey(
            HatchStyle HatchStyle,
            int ForeColorArgb,
            int BackColorArgb);
    }
}
