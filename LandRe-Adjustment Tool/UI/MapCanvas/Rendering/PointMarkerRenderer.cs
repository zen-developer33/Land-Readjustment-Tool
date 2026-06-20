using System.Drawing;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Gdi;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Describes one named point marker symbol available for canvas point
    /// layers.
    /// </summary>
    /// <param name="Key">Stable marker key stored on layer/style settings.</param>
    /// <param name="Name">User-facing marker name.</param>
    public sealed record PointMarkerDefinition(string Key, string Name);

    /// <summary>
    /// Draws small point marker symbols through the map render surface.
    /// </summary>
    /// <remarks>
    /// The renderer keeps a <see cref="Graphics"/> overload for existing UI
    /// preview swatches, but the core implementation uses
    /// <see cref="IMapRenderSurface"/> so map rendering can move to SkiaSharp
    /// without marker-specific GDI calls.
    /// </remarks>
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

        /// <summary>
        /// Gets all marker definitions in picker/display order.
        /// </summary>
        public static IReadOnlyList<PointMarkerDefinition> GetMarkers() => Definitions;

        /// <summary>
        /// Normalizes a stored marker key to a known marker definition key.
        /// </summary>
        public static string Normalize(string? key)
        {
            string normalized = (key ?? string.Empty).Trim();
            return Definitions.Any(definition =>
                string.Equals(definition.Key, normalized, StringComparison.OrdinalIgnoreCase))
                ? Definitions.First(definition =>
                    string.Equals(definition.Key, normalized, StringComparison.OrdinalIgnoreCase)).Key
                : "Dot";
        }

        /// <summary>
        /// Draws a marker to a WinForms/GDI target by wrapping it in the current
        /// production render surface.
        /// </summary>
        public static void Draw(
            Graphics graphics,
            RectangleF bounds,
            string? markerKey,
            Color color,
            float lineWeight = 1.5f)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            using GdiMapRenderSurface surface = new(graphics, Size.Round(graphics.VisibleClipBounds.Size));
            Draw(surface, bounds, markerKey, color, lineWeight);
        }

        /// <summary>
        /// Draws a marker symbol through the active backend-neutral render
        /// surface.
        /// </summary>
        public static void Draw(
            IMapRenderSurface surface,
            RectangleF bounds,
            string? markerKey,
            Color color,
            float lineWeight = 1.5f)
        {
            ArgumentNullException.ThrowIfNull(surface);
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

            StrokeStyle stroke = new(color, Math.Max(1f, lineWeight));

            switch (marker)
            {
                case "Circle":
                    surface.DrawEllipse(square, stroke);
                    break;
                case "Cross":
                    DrawPlus(surface, stroke, square);
                    break;
                case "X":
                    DrawX(surface, stroke, square);
                    break;
                case "Square":
                    surface.DrawRectangle(square, stroke);
                    break;
                case "Diamond":
                    DrawPolygon(surface, CreateDiamond(square), stroke);
                    break;
                case "Triangle":
                    DrawPolygon(surface, CreateTriangle(square), stroke);
                    break;
                case "PlusCircle":
                    surface.DrawEllipse(square, stroke);
                    DrawPlus(surface, stroke, RectangleF.Inflate(square, -size * 0.22f, -size * 0.22f));
                    break;
                case "CircleCross":
                    surface.DrawEllipse(square, stroke);
                    DrawX(surface, stroke, RectangleF.Inflate(square, -size * 0.22f, -size * 0.22f));
                    break;
                case "Star":
                    DrawPolygon(surface, CreateStar(center, size / 2f, size * 0.22f), stroke);
                    break;
                default:
                    surface.FillEllipse(
                        RectangleF.Inflate(square, -size * 0.28f, -size * 0.28f),
                        new FillStyle(color));
                    break;
            }
        }

        /// <summary>
        /// Draws a plus sign using two backend line segments.
        /// </summary>
        private static void DrawPlus(IMapRenderSurface surface, in StrokeStyle stroke, RectangleF rect)
        {
            float centerX = rect.Left + rect.Width / 2f;
            float centerY = rect.Top + rect.Height / 2f;
            surface.DrawLine(new PointF(centerX, rect.Top), new PointF(centerX, rect.Bottom), stroke);
            surface.DrawLine(new PointF(rect.Left, centerY), new PointF(rect.Right, centerY), stroke);
        }

        /// <summary>
        /// Draws an X sign using two backend line segments.
        /// </summary>
        private static void DrawX(IMapRenderSurface surface, in StrokeStyle stroke, RectangleF rect)
        {
            surface.DrawLine(new PointF(rect.Left, rect.Top), new PointF(rect.Right, rect.Bottom), stroke);
            surface.DrawLine(new PointF(rect.Right, rect.Top), new PointF(rect.Left, rect.Bottom), stroke);
        }

        /// <summary>
        /// Builds and strokes one closed marker polygon.
        /// </summary>
        private static void DrawPolygon(IMapRenderSurface surface, PointF[] points, in StrokeStyle stroke)
        {
            IMapPathBuilder builder = surface.CreatePath();
            builder.AddPolygon(points);
            using IMapPath path = builder.Build();
            surface.DrawPath(path, stroke);
        }

        /// <summary>
        /// Creates the diamond marker vertices inside the supplied square.
        /// </summary>
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

        /// <summary>
        /// Creates the triangle marker vertices inside the supplied square.
        /// </summary>
        private static PointF[] CreateTriangle(RectangleF rect)
        {
            return
            [
                new(rect.Left + rect.Width / 2f, rect.Top),
                new(rect.Right, rect.Bottom),
                new(rect.Left, rect.Bottom)
            ];
        }

        /// <summary>
        /// Creates alternating outer/inner vertices for the star marker.
        /// </summary>
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
