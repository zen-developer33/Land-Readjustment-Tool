using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class BrushCache : IDisposable
    {
        private readonly Dictionary<int, SolidBrush> _solidBrushes = new Dictionary<int, SolidBrush>();
        private readonly Dictionary<HatchBrushKey, HatchBrush> _hatchBrushes = new Dictionary<HatchBrushKey, HatchBrush>();
        private readonly Dictionary<TextureHatchBrushKey, TextureHatchBrush> _textureHatchBrushes = new();

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

        public TextureBrush GetTextureHatch(
            string? patternKey,
            Color hatchColor,
            double hatchScale)
        {
            string normalizedPatternKey = HatchPatternService.NormalizePatternKey(patternKey);
            int scaleKey = (int)Math.Round(Math.Clamp(hatchScale, 0.1, 25.0) * 1000.0);
            TextureHatchBrushKey key = new(
                normalizedPatternKey.ToUpperInvariant(),
                hatchColor.ToArgb(),
                scaleKey);

            if (_textureHatchBrushes.TryGetValue(key, out TextureHatchBrush? cached))
            {
                return cached.Brush;
            }

            Bitmap tile = HatchPatternService.CreatePatternTile(
                normalizedPatternKey,
                hatchColor,
                scaleKey / 1000.0);
            TextureBrush brush = new(tile, WrapMode.Tile);
            _textureHatchBrushes[key] = new TextureHatchBrush(tile, brush);
            return brush;
        }

        public void Dispose()
        {
            foreach (SolidBrush brush in _solidBrushes.Values)
                brush.Dispose();

            foreach (HatchBrush brush in _hatchBrushes.Values)
                brush.Dispose();

            foreach (TextureHatchBrush textureHatchBrush in _textureHatchBrushes.Values)
                textureHatchBrush.Dispose();

            _solidBrushes.Clear();
            _hatchBrushes.Clear();
            _textureHatchBrushes.Clear();
        }

        private readonly record struct HatchBrushKey(
            HatchStyle HatchStyle,
            int ForeColorArgb,
            int BackColorArgb);

        private readonly record struct TextureHatchBrushKey(
            string PatternKey,
            int HatchColorArgb,
            int ScaleKey);

        private sealed class TextureHatchBrush : IDisposable
        {
            private readonly Bitmap _tile;

            public TextureHatchBrush(Bitmap tile, TextureBrush brush)
            {
                _tile = tile;
                Brush = brush;
            }

            public TextureBrush Brush { get; }

            public void Dispose()
            {
                Brush.Dispose();
                _tile.Dispose();
            }
        }
    }
}
