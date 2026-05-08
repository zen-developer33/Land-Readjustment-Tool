using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class VectorRenderContext
    {
        private readonly PenCache _penCache;
        private readonly BrushCache _brushCache;

        public VectorRenderContext(
            PenCache penCache,
            BrushCache brushCache,
            double zoomScale,
            bool isPreview = false)
        {
            _penCache = penCache;
            _brushCache = brushCache;
            ZoomScale = zoomScale;
            IsPreview = isPreview;
        }

        public double ZoomScale { get; }

        public bool IsPreview { get; }

        public float AdaptiveLineWidth =>
            ZoomScale > 5000 ? 1.5f :
            ZoomScale > 500 ? 1.0f :
            ZoomScale > 50 ? 0.5f : 0.25f;

        public Pen GetPen(
            Color color,
            float width,
            DashStyle dashStyle = DashStyle.Solid) =>
            _penCache.Get(color, width, dashStyle);

        public SolidBrush GetSolidBrush(Color color) =>
            _brushCache.GetSolid(color);

        public HatchBrush GetHatchBrush(
            HatchStyle hatchStyle,
            Color foreColor,
            Color backColor) =>
            _brushCache.GetHatch(hatchStyle, foreColor, backColor);
    }
}
