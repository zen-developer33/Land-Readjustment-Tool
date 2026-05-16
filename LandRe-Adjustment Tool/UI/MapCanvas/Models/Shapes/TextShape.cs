using Land_Readjustment_Tool.UI.MapCanvas.Models.Snapping;
using System.Drawing;

namespace Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes
{
    public class TextShape : IShape
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public PointD Position { get; private set; }
        public string Text { get; set; }
        public Font Font { get; }
        public string HorizontalAlignment { get; }
        public Color BorderColor { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Transparent;
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; } = true;
        public string LayerName { get; set; } = "Default";
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        // Set during Draw() — used for accurate screen-space hit-testing.
        public RectangleF? LastRenderedBounds { get; private set; }

        public TextShape(PointD position, string text, Font? font = null, string horizontalAlignment = "Left")
        {
            Position = position;
            Text = text;
            Font = font ?? SystemFonts.DefaultFont;
            HorizontalAlignment = NormalizeHorizontalAlignment(horizontalAlignment);
        }

        public RectangleD GetBoundingBox()
        {
            // Estimate based on font metrics; canvas hit-testing uses LastRenderedBounds.
            SizeF approx = TextRenderer.MeasureText(
                string.IsNullOrEmpty(Text) ? "W" : Text,
                Font);
            float originX = NormalizeHorizontalAlignment(HorizontalAlignment) switch
            {
                "Center" => (float)Position.X - approx.Width / 2f,
                "Right"  => (float)Position.X - approx.Width,
                _        => (float)Position.X
            };
            return new RectangleD(originX, Position.Y, approx.Width, approx.Height);
        }

        public bool ContainsPoint(PointD point, float tolerance)
        {
            // Screen-space check is preferred (done in MapCanvasControl via LastRenderedBounds).
            // This world-space fallback just tests proximity to the anchor.
            double dx = point.X - Position.X;
            double dy = point.Y - Position.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= tolerance;
        }

        public void Draw(Graphics g, Func<PointD, PointF> worldToScreen, bool isPrinting = false)
        {
            if (!IsVisible) return;
            try
            {
                PointF screenPos = worldToScreen(Position);
                if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y) ||
                    float.IsInfinity(screenPos.X) || float.IsInfinity(screenPos.Y))
                    return;

                Color textColor = FillColor == Color.Transparent ? Color.Black : FillColor;
                using SolidBrush brush = new(textColor);
                using StringFormat format = CreateStringFormat(HorizontalAlignment);

                SizeF textSize = g.MeasureString(string.IsNullOrEmpty(Text) ? " " : Text, Font);
                float drawX = NormalizeHorizontalAlignment(HorizontalAlignment) switch
                {
                    "Center" => screenPos.X - textSize.Width / 2f,
                    "Right"  => screenPos.X - textSize.Width,
                    _        => screenPos.X
                };

                LastRenderedBounds = new RectangleF(drawX, screenPos.Y, textSize.Width, textSize.Height);

                g.DrawString(Text, Font, brush, screenPos, format);

                if (IsSelected)
                {
                    RectangleF bounds = LastRenderedBounds.Value;
                    bounds.Inflate(2, 2);
                    using Pen pen = new(BorderColor, 1f);
                    g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
                }
            }
            catch { }
        }

        public void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPrinting = false)
        {
            if (!IsVisible) return;
            PointD screenPosD = worldToScreen(Position);
            PointF screenPos = new((float)screenPosD.X, (float)screenPosD.Y);

            Color textColor = FillColor == Color.Transparent ? Color.Black : FillColor;
            using SolidBrush brush = new(textColor);
            using StringFormat format = CreateStringFormat(HorizontalAlignment);

            SizeF textSize = g.MeasureString(string.IsNullOrEmpty(Text) ? " " : Text, Font);
            float drawX = NormalizeHorizontalAlignment(HorizontalAlignment) switch
            {
                "Center" => screenPos.X - textSize.Width / 2f,
                "Right"  => screenPos.X - textSize.Width,
                _        => screenPos.X
            };

            LastRenderedBounds = new RectangleF(drawX, screenPos.Y, textSize.Width, textSize.Height);

            g.DrawString(Text, Font, brush, screenPos, format);

            if (IsSelected)
            {
                RectangleF bounds = LastRenderedBounds.Value;
                bounds.Inflate(2, 2);
                using Pen pen = new(BorderColor, 1f);
                g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width, bounds.Height);
            }
        }

        public IShape Clone()
        {
            var clone = new TextShape(Position, Text, (Font)Font.Clone(), HorizontalAlignment)
            {
                BorderColor = this.BorderColor,
                FillColor = this.FillColor,
                IsSelected = this.IsSelected,
                IsVisible = this.IsVisible,
                LayerName = this.LayerName
            };
            foreach (var kvp in this.Properties)
                clone.Properties[kvp.Key] = kvp.Value;
            return clone;
        }

        public void Translate(PointD delta)
        {
            Position = new PointD(Position.X + delta.X, Position.Y + delta.Y);
        }

        public IEnumerable<SnapPoint> GetSnapPoints()
        {
            yield return new SnapPoint(SnapType.Endpoint, Position, this);
        }

        public static string NormalizeHorizontalAlignment(string? alignment)
        {
            return alignment?.Trim().ToLowerInvariant() switch
            {
                "center" or "centre" or "middle" => "Center",
                "right" => "Right",
                _ => "Left"
            };
        }

        public static StringAlignment ToStringAlignment(string? alignment)
        {
            return NormalizeHorizontalAlignment(alignment) switch
            {
                "Center" => StringAlignment.Center,
                "Right"  => StringAlignment.Far,
                _        => StringAlignment.Near
            };
        }

        private static StringFormat CreateStringFormat(string? alignment)
        {
            return new StringFormat
            {
                Alignment = ToStringAlignment(alignment),
                LineAlignment = StringAlignment.Near,
                FormatFlags = StringFormatFlags.NoClip
            };
        }
    }
}
