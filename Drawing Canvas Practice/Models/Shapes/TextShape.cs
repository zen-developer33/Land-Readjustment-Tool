using Drawing_Canvas_Practice.Models.Snapping;
using NetTopologySuite.Geometries.Implementation;
using System.Drawing;

namespace Drawing_Canvas_Practice.Models.Shapes
{
    public class TextShape : IShape
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public PointD Position { get; }
        public string Text { get; }
        public Font Font { get; }
        public Color BorderColor { get; set; } = Color.Black;
        public Color FillColor { get; set; } = Color.Transparent;
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; } = true;
        public string LayerName { get; set; } = "Default";
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public TextShape(PointD position, string text, Font font = null)
        {
            Position = position;
            Text = text;
            Font = font ?? SystemFonts.DefaultFont;
        }

        public RectangleD GetBoundingBox()
        {
            // Simple bounding box (could be improved with actual text measurement)
            return new RectangleD(Position.X, Position.Y, 100, 30);
        }

        public bool ContainsPoint(PointD point,float tolerance)
        {
            return point.X >= Position.X && point.X <= Position.X + 100 &&
                   point.Y >= Position.Y && point.Y <= Position.Y + 30;
        }

        public void Draw(Graphics g, Func<PointD, PointF> worldToScreen, bool isPrinting = false)
        {
            if (!IsVisible) return;
            try
            {
                var screenPos = worldToScreen(Position);
                // Validate position
                if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y) || float.IsInfinity(screenPos.X) || float.IsInfinity(screenPos.Y))
                    return;
                using (var brush = new SolidBrush(FillColor == Color.Transparent ? Color.Black : FillColor))
                {
                    g.DrawString(Text, Font, brush, screenPos);
                }
                if (IsSelected)
                {
                    using (var pen = new Pen(BorderColor, 2))
                    {
                        g.DrawRectangle(pen, screenPos.X, screenPos.Y, 100, 30);
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log or display error
                // System.Diagnostics.Debug.WriteLine($"TextShape.Draw error: {ex.Message}");
            }
        }

        public IShape Clone()
        {
            var clone = new TextShape(Position, Text, (Font)Font.Clone())
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

        public void Draw(Graphics g, Func<PointD, PointD> worldToScreen, bool isPrinting = false)
        {   
            if (!IsVisible) return;
            var screenPos = worldToScreen(Position);
            using (var brush = new SolidBrush(FillColor == Color.Transparent ? Color.Black : FillColor))
            {
                g.DrawString(Text, Font, brush, (float)screenPos.X, (float)screenPos.Y);
            }
            if (IsSelected)
            {
                using (var pen = new Pen(BorderColor, 2))
                {
                    g.DrawRectangle(pen, (float)screenPos.X, (float)screenPos.Y, 100, 30);
                }
            }
        }

        public IEnumerable<SnapPoint> GetSnapPoints()
        {
            // Snap to text anchor point (position)
            yield return new SnapPoint(SnapType.Endpoint, Position, this);
        }
    }
}