using System.Drawing;
using System.Drawing.Drawing2D;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    public sealed class HatchPatternService : IHatchPatternService
    {
        private const int TileSize = 32;
        private const int MinTileSize = 4;
        private const int MaxTileSize = 2048;
        private const float HatchStrokeWidth = 1.0f;

        private static readonly IReadOnlyList<HatchPatternDefinition> Patterns =
        [
            new("ANSI31", "ANSI31 - Diagonal", "AutoCAD-style 45 degree hatch for general parcel and boundary fills."),
            new("ANSI32", "ANSI32 - Steel", "AutoCAD-style paired 45 degree lines for steel/material hatching."),
            new("ANSI33", "ANSI33 - Bronze / Copper", "AutoCAD-style diagonal hatch with alternating dashed companion lines."),
            new("ANSI34", "ANSI34 - Plastic / Rubber", "AutoCAD-style grouped diagonal hatch for plastic, rubber, and similar areas."),
            new("ANSI37", "ANSI37 - Diagonal Cross", "AutoCAD-style 45 and 135 degree cross hatch."),
            new("HORIZONTAL", "GIS Horizontal", "Common GIS horizontal line fill."),
            new("VERTICAL", "GIS Vertical", "Common GIS vertical line fill."),
            new("CROSS", "GIS Cross", "Common GIS horizontal and vertical cross fill."),
            new("DIAGONAL-CROSS", "GIS Diagonal Cross", "Common GIS forward and backward diagonal fill."),
            new("DOTS", "Dots", "Point pattern fill for categories that need a lighter texture."),
            new("SAND", "Sand", "Fine dots for sandy, soft, or low-bearing ground."),
            new("GRAVEL", "Gravel / Aggregate", "Irregular dot and pebble texture useful for roads, open ground, and material-like areas."),
            new("GRASS", "Grass", "Tuft pattern for parks, open spaces, buffers, and landscaped areas."),
            new("EARTH", "Earth", "Short broken strokes for natural soil, embankment, and existing ground areas."),
            new("WATER", "Water", "Soft wave pattern for ponds, drains, channels, and water features."),
            new("CONCRETE", "Concrete", "Aggregate-like hatch for concrete slabs, paved surfaces, and structures."),
            new("BRICK", "Brick", "Staggered masonry pattern for walls, paths, and built features."),
            new("NET", "Net", "Diamond mesh for restricted, fenced, or special-use areas."),
            new("WOOD", "Wood", "Wavy grain hatch for timber or temporary structural areas.")
        ];

        public IReadOnlyList<HatchPatternDefinition> GetPatterns()
        {
            return Patterns;
        }

        public HatchPatternDefinition GetPatternOrDefault(string? key)
        {
            return Patterns.FirstOrDefault(pattern =>
                    string.Equals(pattern.Key, key, StringComparison.OrdinalIgnoreCase))
                ?? Patterns[0];
        }

        public void DrawPreview(
            Graphics graphics,
            Rectangle bounds,
            string? patternKey,
            Color hatchColor,
            Color fillColor,
            int transparency,
            double hatchScale,
            Color backgroundColor)
        {
            ArgumentNullException.ThrowIfNull(graphics);

            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            int clampedTransparency = Math.Clamp(transparency, 0, 100);
            Color visibleFill = BlendColor(
                Color.FromArgb(255, fillColor.R, fillColor.G, fillColor.B),
                backgroundColor,
                clampedTransparency / 100f);

            using SolidBrush fillBrush = new(visibleFill);
            graphics.FillRectangle(fillBrush, bounds);

            string key = GetPatternOrDefault(patternKey).Key;
            Color visibleHatchColor = Color.FromArgb(
                Math.Max(0, (int)Math.Round(230 * (1f - clampedTransparency / 100f))),
                hatchColor.R,
                hatchColor.G,
                hatchColor.B);

            using GraphicsPath previewPath = new();
            previewPath.AddRectangle(bounds);
            if (TryDrawLinePattern(
                    graphics,
                    previewPath,
                    key,
                    visibleHatchColor,
                    NormalizeHatchScale(hatchScale),
                    new PointF(bounds.Left, bounds.Top)))
            {
                return;
            }

            using Bitmap tile = CreatePatternTile(
                key,
                visibleHatchColor,
                NormalizeHatchScale(hatchScale));

            using TextureBrush patternBrush = new(tile, WrapMode.Tile);
            patternBrush.TranslateTransform(bounds.Left, bounds.Top);
            graphics.FillRectangle(patternBrush, bounds);
        }

        public static void FillPath(
            Graphics graphics,
            GraphicsPath path,
            string? patternKey,
            Color hatchColor,
            double hatchScale,
            PointF origin)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(path);

            using Bitmap tile = CreatePatternTile(patternKey, hatchColor, hatchScale);

            using TextureBrush patternBrush = new(tile, WrapMode.Tile);
            patternBrush.TranslateTransform(origin.X, origin.Y);
            graphics.FillPath(patternBrush, path);
        }

        internal static Bitmap CreatePatternTile(
            string? patternKey,
            Color hatchColor,
            double hatchScale)
        {
            return CreatePatternTile(
                NormalizePatternKey(patternKey),
                hatchColor,
                NormalizeHatchScale(hatchScale));
        }

        private static Bitmap CreatePatternTile(string patternKey, Color hatchColor, float hatchScale)
        {
            patternKey = NormalizePatternKey(patternKey).ToUpperInvariant();
            float visualScale = Math.Max(0.01f, hatchScale * GetPatternScaleMultiplier(patternKey));
            int tileSize = Math.Clamp(
                (int)Math.Round(TileSize * visualScale),
                MinTileSize,
                MaxTileSize);
            float effectiveScale = tileSize / (float)TileSize;
            Bitmap tile = new(tileSize, tileSize);

            using Graphics graphics = Graphics.FromImage(tile);
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.ScaleTransform(effectiveScale, effectiveScale);

            float patternPenWidth = HatchStrokeWidth / effectiveScale;
            using Pen pen = new(hatchColor, patternPenWidth)
            {
                StartCap = LineCap.Flat,
                EndCap = LineCap.Flat
            };
            float markerRadius = Math.Max(1.0f / effectiveScale, 0.9f);

            switch (patternKey.ToUpperInvariant())
            {
                case "ANSI31":
                    DrawDiagonalLines(graphics, pen, spacing: 3.175f, forward: true);
                    break;
                case "ANSI32":
                    DrawDiagonalLines(graphics, pen, spacing: 9.525f, forward: true);
                    DrawDiagonalLines(graphics, pen, spacing: 9.525f, forward: true, offset: 4.49013f);
                    break;
                case "ANSI33":
                    DrawDiagonalLines(graphics, pen, spacing: 6.35f, forward: true);
                    using (Pen dashedPen = (Pen)pen.Clone())
                    {
                        dashedPen.DashPattern = [3.175f, 1.5875f];
                        DrawDiagonalLines(graphics, dashedPen, spacing: 6.35f, forward: true, offset: 4.49013f);
                    }
                    break;
                case "ANSI34":
                    DrawDiagonalLines(graphics, pen, spacing: 19.05f, forward: true);
                    DrawDiagonalLines(graphics, pen, spacing: 19.05f, forward: true, offset: 4.49013f);
                    DrawDiagonalLines(graphics, pen, spacing: 19.05f, forward: true, offset: 8.98026f);
                    DrawDiagonalLines(graphics, pen, spacing: 19.05f, forward: true, offset: 13.4704f);
                    break;
                case "ANSI37":
                    DrawDiagonalLines(graphics, pen, spacing: 3.175f, forward: true);
                    DrawDiagonalLines(graphics, pen, spacing: 3.175f, forward: false);
                    break;
                case "HORIZONTAL":
                    DrawHorizontalLines(graphics, pen, spacing: 8);
                    break;
                case "VERTICAL":
                    DrawVerticalLines(graphics, pen, spacing: 8);
                    break;
                case "CROSS":
                    DrawHorizontalLines(graphics, pen, spacing: 9);
                    DrawVerticalLines(graphics, pen, spacing: 9);
                    break;
                case "DIAGONAL-CROSS":
                    DrawDiagonalLines(graphics, pen, spacing: 10, forward: true);
                    DrawDiagonalLines(graphics, pen, spacing: 10, forward: false);
                    break;
                case "DOTS":
                    DrawDots(graphics, hatchColor, spacing: 8, radius: 1.3f);
                    break;
                case "SAND":
                    DrawSand(graphics, hatchColor, markerRadius);
                    break;
                case "GRAVEL":
                    DrawGravel(graphics, hatchColor, patternPenWidth);
                    break;
                case "GRASS":
                    DrawGrass(graphics, hatchColor, patternPenWidth);
                    break;
                case "EARTH":
                    DrawEarth(graphics, hatchColor, patternPenWidth);
                    break;
                case "WATER":
                    DrawWater(graphics, hatchColor, patternPenWidth);
                    break;
                case "CONCRETE":
                    DrawConcrete(graphics, hatchColor, patternPenWidth);
                    break;
                case "BRICK":
                    DrawBrick(graphics, pen);
                    break;
                case "NET":
                    DrawNet(graphics, pen);
                    break;
                case "WOOD":
                    DrawWood(graphics, pen);
                    break;
                default:
                    DrawDiagonalLines(graphics, pen, spacing: 10, forward: true);
                    break;
            }

            return tile;
        }

        internal static string NormalizePatternKey(string? patternKey)
        {
            if (string.IsNullOrWhiteSpace(patternKey))
                return Patterns[0].Key;

            HatchPatternDefinition? pattern = Patterns.FirstOrDefault(pattern =>
                string.Equals(pattern.Key, patternKey, StringComparison.OrdinalIgnoreCase));

            return pattern == null
                ? patternKey.Trim()
                : pattern.Key;
        }

        internal static bool TryDrawLinePattern(
            Graphics graphics,
            GraphicsPath clipPath,
            string? patternKey,
            Color hatchColor,
            double screenScale,
            PointF origin)
        {
            ArgumentNullException.ThrowIfNull(graphics);
            ArgumentNullException.ThrowIfNull(clipPath);

            string key = NormalizePatternKey(patternKey).ToUpperInvariant();
            HatchLineDefinition[]? lines = GetLineDefinitions(key);
            if (lines == null || lines.Length == 0)
                return false;

            float scale = ResolveScreenScale(screenScale);
            if (scale <= 0.0f)
                return true;

            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(clipPath, CombineMode.Intersect);

                RectangleF bounds = clipPath.GetBounds();
                if (bounds.Width <= 0.0f || bounds.Height <= 0.0f)
                    return true;

                foreach (HatchLineDefinition line in lines)
                {
                    DrawHatchLineFamily(graphics, bounds, line, hatchColor, scale, origin);
                }
            }
            finally
            {
                graphics.Restore(state);
            }

            return true;
        }

        internal static HatchLineDefinition[]? GetLineDefinitions(string patternKey)
        {
            return patternKey switch
            {
                "ANSI31" => [new(45.0f, 0.0f, 0.0f, 3.175f)],
                "ANSI32" =>
                [
                    new(45.0f, 0.0f, 0.0f, 9.525f),
                    new(45.0f, 4.49013f, 0.0f, 9.525f)
                ],
                "ANSI33" =>
                [
                    new(45.0f, 0.0f, 0.0f, 6.35f),
                    new(45.0f, 4.49013f, 0.0f, 6.35f, [3.175f, 1.5875f])
                ],
                "ANSI34" =>
                [
                    new(45.0f, 0.0f, 0.0f, 19.05f),
                    new(45.0f, 4.49013f, 0.0f, 19.05f),
                    new(45.0f, 8.98026f, 0.0f, 19.05f),
                    new(45.0f, 13.4704f, 0.0f, 19.05f)
                ],
                "ANSI37" =>
                [
                    new(45.0f, 0.0f, 0.0f, 3.175f),
                    new(135.0f, 0.0f, 0.0f, 3.175f)
                ],
                "HORIZONTAL" => [new(0.0f, 0.0f, 0.0f, 8.0f)],
                "VERTICAL" => [new(90.0f, 0.0f, 0.0f, 8.0f)],
                "CROSS" =>
                [
                    new(0.0f, 0.0f, 0.0f, 9.0f),
                    new(90.0f, 0.0f, 0.0f, 9.0f)
                ],
                "DIAGONAL-CROSS" =>
                [
                    new(45.0f, 0.0f, 0.0f, 10.0f),
                    new(135.0f, 0.0f, 0.0f, 10.0f)
                ],
                "NET" =>
                [
                    new(45.0f, 0.0f, 0.0f, 8.0f),
                    new(135.0f, 0.0f, 0.0f, 8.0f)
                ],
                _ => null
            };
        }

        private static void DrawHatchLineFamily(
            Graphics graphics,
            RectangleF bounds,
            HatchLineDefinition line,
            Color hatchColor,
            float scale,
            PointF origin)
        {
            float spacing = Math.Abs(line.Spacing * scale);
            if (!float.IsFinite(spacing) || spacing <= 0.0f)
                return;

            float radians = line.AngleDegrees * MathF.PI / 180.0f;
            PointF direction = new(MathF.Cos(radians), -MathF.Sin(radians));
            PointF normal = new(-direction.Y, direction.X);

            float familyOriginX = origin.X + line.OriginX * scale;
            float familyOriginY = origin.Y - line.OriginY * scale;
            float originDistance = Dot(familyOriginX, familyOriginY, normal);
            float minDistance = MinDot(bounds, normal);
            float maxDistance = MaxDot(bounds, normal);
            float diagonal = MathF.Sqrt(bounds.Width * bounds.Width + bounds.Height * bounds.Height) + spacing * 2.0f + 32.0f;
            float boundsCenterX = bounds.Left + bounds.Width / 2.0f;
            float boundsCenterY = bounds.Top + bounds.Height / 2.0f;
            float boundsCenterDistance = Dot(boundsCenterX, boundsCenterY, normal);
            int firstIndex = (int)MathF.Floor((minDistance - originDistance) / spacing) - 1;
            int lastIndex = (int)MathF.Ceiling((maxDistance - originDistance) / spacing) + 1;

            using Pen pen = new(hatchColor, HatchStrokeWidth)
            {
                StartCap = LineCap.Flat,
                EndCap = LineCap.Flat
            };

            if (line.DashPattern is { Length: > 0 })
            {
                pen.DashStyle = DashStyle.Custom;
                pen.DashPattern = line.DashPattern
                    .Select(value => Math.Max(0.1f, Math.Abs(value * scale)))
                    .ToArray();
            }

            for (int index = firstIndex; index <= lastIndex; index++)
            {
                float distance = originDistance + index * spacing;
                float centerOffset = distance - boundsCenterDistance;
                PointF center = new(
                    boundsCenterX + normal.X * centerOffset,
                    boundsCenterY + normal.Y * centerOffset);
                PointF a = new(
                    center.X - direction.X * diagonal,
                    center.Y - direction.Y * diagonal);
                PointF b = new(
                    center.X + direction.X * diagonal,
                    center.Y + direction.Y * diagonal);
                graphics.DrawLine(pen, a, b);
            }
        }

        private static float ResolveScreenScale(double screenScale)
        {
            if (double.IsNaN(screenScale) || double.IsInfinity(screenScale) || screenScale <= 0.0)
                return 1.0f;

            if (screenScale > float.MaxValue)
                return float.MaxValue;

            return (float)screenScale;
        }

        private static float Dot(float x, float y, PointF normal) =>
            x * normal.X + y * normal.Y;

        private static float MinDot(RectangleF bounds, PointF normal) =>
            Math.Min(
                Math.Min(Dot(bounds.Left, bounds.Top, normal), Dot(bounds.Right, bounds.Top, normal)),
                Math.Min(Dot(bounds.Left, bounds.Bottom, normal), Dot(bounds.Right, bounds.Bottom, normal)));

        private static float MaxDot(RectangleF bounds, PointF normal) =>
            Math.Max(
                Math.Max(Dot(bounds.Left, bounds.Top, normal), Dot(bounds.Right, bounds.Top, normal)),
                Math.Max(Dot(bounds.Left, bounds.Bottom, normal), Dot(bounds.Right, bounds.Bottom, normal)));

        private static void DrawHorizontalLines(Graphics graphics, Pen pen, int spacing)
        {
            for (int y = -TileSize; y <= TileSize * 2; y += spacing)
                graphics.DrawLine(pen, -TileSize, y, TileSize * 2, y);
        }

        private static void DrawVerticalLines(Graphics graphics, Pen pen, int spacing)
        {
            for (int x = -TileSize; x <= TileSize * 2; x += spacing)
                graphics.DrawLine(pen, x, -TileSize, x, TileSize * 2);
        }

        private static void DrawDiagonalLines(
            Graphics graphics,
            Pen pen,
            float spacing,
            bool forward,
            float offset = 0.0f)
        {
            for (float position = -TileSize * 2 + offset; position <= TileSize * 2; position += spacing)
            {
                if (forward)
                    graphics.DrawLine(pen, position, TileSize, position + TileSize, 0);
                else
                    graphics.DrawLine(pen, position, 0, position + TileSize, TileSize);
            }
        }

        private static void DrawDots(Graphics graphics, Color hatchColor, int spacing, float radius)
        {
            using SolidBrush brush = new(hatchColor);
            for (int y = spacing / 2; y < TileSize; y += spacing)
            {
                for (int x = spacing / 2; x < TileSize; x += spacing)
                {
                    graphics.FillEllipse(
                        brush,
                        x - radius,
                        y - radius,
                        radius * 2,
                        radius * 2);
                }
            }
        }

        private static void DrawGravel(Graphics graphics, Color hatchColor, float penWidth)
        {
            using Pen pen = new(hatchColor, penWidth);
            using SolidBrush brush = new(hatchColor);

            PointF[] centers =
            [
                new(5, 6),
                new(16, 4),
                new(27, 9),
                new(9, 18),
                new(21, 17),
                new(3, 29),
                new(29, 26)
            ];

            foreach (PointF center in centers)
            {
                graphics.DrawEllipse(pen, center.X - 2.2f, center.Y - 1.5f, 4.4f, 3f);
            }

            graphics.FillEllipse(brush, 25, 3, 2.5f, 2.5f);
            graphics.FillEllipse(brush, 14, 26, 2.5f, 2.5f);
        }

        private static void DrawSand(Graphics graphics, Color hatchColor, float radius)
        {
            using SolidBrush brush = new(hatchColor);
            PointF[] points =
            [
                new(4, 5), new(13, 3), new(24, 6),
                new(8, 13), new(19, 15), new(29, 12),
                new(3, 23), new(14, 25), new(25, 22),
                new(31, 29)
            ];

            foreach (PointF point in points)
                graphics.FillEllipse(brush, point.X - radius, point.Y - radius, radius * 2.0f, radius * 2.0f);
        }

        private static void DrawGrass(Graphics graphics, Color hatchColor, float penWidth)
        {
            using Pen pen = new(hatchColor, penWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            PointF[] bases =
            [
                new(5, 9), new(17, 7), new(28, 11),
                new(9, 22), new(22, 21), new(30, 29)
            ];

            foreach (PointF basePoint in bases)
            {
                graphics.DrawLine(pen, basePoint.X, basePoint.Y, basePoint.X - 3, basePoint.Y - 5);
                graphics.DrawLine(pen, basePoint.X, basePoint.Y, basePoint.X, basePoint.Y - 7);
                graphics.DrawLine(pen, basePoint.X, basePoint.Y, basePoint.X + 3, basePoint.Y - 5);
            }
        }

        private static void DrawEarth(Graphics graphics, Color hatchColor, float penWidth)
        {
            using Pen pen = new(hatchColor, penWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            graphics.DrawLine(pen, 3, 7, 12, 6);
            graphics.DrawLine(pen, 20, 5, 29, 8);
            graphics.DrawLine(pen, 6, 17, 16, 19);
            graphics.DrawLine(pen, 22, 18, 30, 16);
            graphics.DrawLine(pen, 2, 28, 11, 25);
            graphics.DrawLine(pen, 17, 28, 28, 27);
        }

        private static void DrawWater(Graphics graphics, Color hatchColor, float penWidth)
        {
            using Pen pen = new(hatchColor, penWidth)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            for (int y = 7; y < TileSize; y += 10)
            {
                using GraphicsPath path = new();
                path.AddBezier(0, y, 6, y - 4, 10, y + 4, 16, y);
                path.AddBezier(16, y, 22, y - 4, 26, y + 4, 32, y);
                graphics.DrawPath(pen, path);
            }
        }

        private static void DrawConcrete(Graphics graphics, Color hatchColor, float penWidth)
        {
            DrawGravel(graphics, hatchColor, penWidth);
            using Pen pen = new(hatchColor, penWidth);
            graphics.DrawLine(pen, 2, 14, 9, 20);
            graphics.DrawLine(pen, 18, 9, 24, 3);
            graphics.DrawLine(pen, 21, 24, 30, 18);
        }

        private static void DrawBrick(Graphics graphics, Pen pen)
        {
            DrawHorizontalLines(graphics, pen, spacing: 8);
            for (int y = 0; y <= TileSize; y += 8)
            {
                int offset = y % 16 == 0 ? 0 : 8;
                for (int x = -TileSize; x <= TileSize * 2; x += 16)
                    graphics.DrawLine(pen, x + offset, y, x + offset, y + 8);
            }
        }

        private static void DrawNet(Graphics graphics, Pen pen)
        {
            DrawDiagonalLines(graphics, pen, spacing: 8, forward: true);
            DrawDiagonalLines(graphics, pen, spacing: 8, forward: false);
        }

        private static void DrawWood(Graphics graphics, Pen pen)
        {
            for (int y = 5; y < TileSize; y += 9)
            {
                using GraphicsPath path = new();
                path.AddBezier(0, y, 7, y - 3, 13, y + 3, 20, y);
                path.AddBezier(20, y, 25, y - 2, 28, y + 2, 32, y);
                graphics.DrawPath(pen, path);
            }

            graphics.DrawEllipse(pen, 10, 13, 10, 5);
        }

        private static Color BlendColor(Color source, Color target, float targetWeight)
        {
            float clampedWeight = Math.Clamp(targetWeight, 0f, 1f);
            float sourceWeight = 1f - clampedWeight;

            return Color.FromArgb(
                (int)Math.Round(source.R * sourceWeight + target.R * clampedWeight),
                (int)Math.Round(source.G * sourceWeight + target.G * clampedWeight),
                (int)Math.Round(source.B * sourceWeight + target.B * clampedWeight));
        }

        private static float NormalizeHatchScale(double hatchScale)
        {
            if (double.IsNaN(hatchScale) || double.IsInfinity(hatchScale) || hatchScale <= 0)
                return 1.0f;

            return (float)Math.Clamp(hatchScale, 0.1, 20.0);
        }

        private static float GetPatternScaleMultiplier(string patternKey)
        {
            return patternKey.ToUpperInvariant() switch
            {
                "BRICK" => 0.5f,
                "GRASS" => 0.45f,
                "GRAVEL" => 0.55f,
                "SAND" => 0.55f,
                "EARTH" => 0.55f,
                "WATER" => 0.55f,
                "CONCRETE" => 0.55f,
                "WOOD" => 0.55f,
                "DOTS" => 0.75f,
                _ => 1.0f
            };
        }

        internal readonly record struct HatchLineDefinition(
            float AngleDegrees,
            float OriginX,
            float OriginY,
            float Spacing,
            float[]? DashPattern = null);
    }
}
