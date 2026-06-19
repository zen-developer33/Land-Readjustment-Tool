using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

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
            bool antiAliasingEnabled = true,
            bool isPreview = false,
            bool selectionDecorationOnly = false,
            RectangleD? clipWorldBounds = null)
        {
            _penCache = penCache;
            _brushCache = brushCache;
            ZoomScale = zoomScale;
            AntiAliasingEnabled = antiAliasingEnabled;
            IsPreview = isPreview;
            SelectionDecorationOnly = selectionDecorationOnly;
            ClipWorldBounds = clipWorldBounds;
        }

        public double ZoomScale { get; }

        public bool AntiAliasingEnabled { get; }

        public bool IsPreview { get; }

        public RectangleD? ClipWorldBounds { get; }

        /// <summary>
        /// When true, only selection decoration (highlight glow / selection
        /// stroke) is drawn — interior fills and hatch patterns are skipped so
        /// the overlay can be painted on top of an already-filled cached frame
        /// without doubling fill opacity.
        /// </summary>
        public bool SelectionDecorationOnly { get; }

        public float AdaptiveLineWidth =>
            ZoomScale > 5000 ? 1.5f :
            ZoomScale > 500 ? 1.0f :
            ZoomScale > 50 ? 0.5f : 0.25f;

        public Pen GetPen(
            Color color,
            float width,
            DashStyle dashStyle = DashStyle.Solid) =>
            _penCache.Get(color, width, dashStyle);

        public Pen GetPen(
            Color color,
            float width,
            DashStyle dashStyle,
            float lineTypeScale) =>
            _penCache.Get(color, width, dashStyle, lineTypeScale);

        public Pen GetPen(
            Color color,
            float width,
            string? lineStyle,
            float lineTypeScale) =>
            _penCache.Get(color, width, lineStyle, lineTypeScale);

        public SolidBrush GetSolidBrush(Color color) =>
            _brushCache.GetSolid(color);

        public HatchBrush GetHatchBrush(
            HatchStyle hatchStyle,
            Color foreColor,
            Color backColor) =>
            _brushCache.GetHatch(hatchStyle, foreColor, backColor);

        public TextureBrush GetTextureHatchBrush(
            string? patternKey,
            Color hatchColor,
            double hatchScale) =>
            _brushCache.GetTextureHatch(patternKey, hatchColor, hatchScale);
    }
}
