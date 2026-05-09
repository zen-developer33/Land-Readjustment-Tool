using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed record PointMarkerDefinition(string Key, string Name);

    public static class PointMarkerRenderer
    {
        private static readonly IReadOnlyList<PointMarkerDefinition> Definitions =
        [
            new("Dot", "Dot"),
            new("Circle", "Circle"),
            new("Cross", "Cross"),
            new("X", "X"),
            new("Square", "Square"),
            new("Diamond", "Diamond"),
            new("Triangle", "Triangle"),
            new("PlusCircle", "Plus Circle"),
            new("CircleCross", "Circle Cross"),
            new("Star", "Star")
        ];

        public static IReadOnlyList<PointMarkerDefinition> GetMarkers() => Definitions;

        public static string Normalize(string? key)
        {
            string normalized = (key ?? string.Empty).Trim();
            return Definitions.Any(definition =>
                string.Equals(definition.Key, normalized, StringComparison.OrdinalIgnoreCase))
                ? Definitions.First(definition =>
                    string.Equals(definition.Key, normalized, StringComparison.OrdinalIgnoreCase)).Key
                : "Dot";
        }

        public static void Draw(
            Graphics graphics,
            RectangleF bounds,
            string? markerKey,
            Color color,
            float lineWeight = 1.5f)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            string marker = Normalize(markerKey);
            float size = Math.Min(bounds.Width, bounds.Height);
            PointF center = new(bounds.Left + bounds.Width / 2f, bounds.Top + bounds.Height / 2f);
            RectangleF square = new(
                center.X - size / 2f,
                center.Y - size / 2f,
                size,
                size);

            using Pen pen = new(color, Math.Max(1f, lineWeight))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            using SolidBrush brush = new(color);

            switch (marker)
            {
                case "Circle":
                    graphics.DrawEllipse(pen, square);
                    break;
                case "Cross":
                    DrawPlus(graphics, pen, square);
                    break;
                case "X":
                    DrawX(graphics, pen, square);
                    break;
                case "Square":
                    graphics.DrawRectangle(pen, square.X, square.Y, square.Width, square.Height);
                    break;
                case "Diamond":
                    graphics.DrawPolygon(pen, CreateDiamond(square));
                    break;
                case "Triangle":
                    graphics.DrawPolygon(pen, CreateTriangle(square));
                    break;
                case "PlusCircle":
                    graphics.DrawEllipse(pen, square);
                    DrawPlus(graphics, pen, RectangleF.Inflate(square, -size * 0.22f, -size * 0.22f));
                    break;
                case "CircleCross":
                    graphics.DrawEllipse(pen, square);
                    DrawX(graphics, pen, RectangleF.Inflate(square, -size * 0.22f, -size * 0.22f));
                    break;
                case "Star":
                    graphics.DrawPolygon(pen, CreateStar(center, size / 2f, size * 0.22f));
                    break;
                default:
                    graphics.FillEllipse(brush, RectangleF.Inflate(square, -size * 0.28f, -size * 0.28f));
                    break;
            }

        }

        private static void DrawPlus(Graphics graphics, Pen pen, RectangleF rect)
        {
            float centerX = rect.Left + rect.Width / 2f;
            float centerY = rect.Top + rect.Height / 2f;
            graphics.DrawLine(pen, centerX, rect.Top, centerX, rect.Bottom);
            graphics.DrawLine(pen, rect.Left, centerY, rect.Right, centerY);
        }

        private static void DrawX(Graphics graphics, Pen pen, RectangleF rect)
        {
            graphics.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);
            graphics.DrawLine(pen, rect.Right, rect.Top, rect.Left, rect.Bottom);
        }

        private static PointF[] CreateDiamond(RectangleF rect)
        {
            float centerX = rect.Left + rect.Width / 2f;
            float centerY = rect.Top + rect.Height / 2f;
            return
            [
                new(centerX, rect.Top),
                new(rect.Right, centerY),
                new(centerX, rect.Bottom),
                new(rect.Left, centerY)
            ];
        }

        private static PointF[] CreateTriangle(RectangleF rect)
        {
            return
            [
                new(rect.Left + rect.Width / 2f, rect.Top),
                new(rect.Right, rect.Bottom),
                new(rect.Left, rect.Bottom)
            ];
        }

        private static PointF[] CreateStar(PointF center, float outerRadius, float innerRadius)
        {
            PointF[] points = new PointF[10];
            for (int i = 0; i < points.Length; i++)
            {
                double angle = (-90 + i * 36) * Math.PI / 180.0;
                float radius = i % 2 == 0 ? outerRadius : innerRadius;
                points[i] = new PointF(
                    center.X + (float)Math.Cos(angle) * radius,
                    center.Y + (float)Math.Sin(angle) * radius);
            }

            return points;
        }
    }
}
