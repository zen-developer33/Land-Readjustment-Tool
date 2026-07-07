using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;
using Land_Readjustment_Tool.UI.MapCanvas.Services;
using SkiaSharp;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Skia
{
    internal static class SkiaHatchPatternRenderer
    {
        public static bool TryDrawLinePattern(
            SKCanvas canvas,
            SKPath path,
            in FillStyle fill,
            bool isAntialiasEnabled)
        {
            string key = HatchPatternService.NormalizePatternKey(fill.PatternKey).ToUpperInvariant();
            HatchPatternService.HatchLineDefinition[]? lines = HatchPatternService.GetLineDefinitions(key);
            if (lines == null || lines.Length == 0)
                return false;

            float scale = ResolveScreenScale(fill.PatternScreenScale);
            if (scale <= 0.0f)
                return true;

            SKRect bounds = path.Bounds;
            if (bounds.Width <= 0.0f || bounds.Height <= 0.0f)
                return true;

            SKColor hatchColor = ToSkiaColor(fill.PatternColor.IsEmpty ? Color.Black : fill.PatternColor);
            foreach (HatchPatternService.HatchLineDefinition line in lines)
            {
                DrawLineFamily(canvas, bounds, line, hatchColor, scale, fill.PatternOriginScreen, isAntialiasEnabled);
            }

            return true;
        }

        private static void DrawLineFamily(
            SKCanvas canvas,
            SKRect bounds,
            HatchPatternService.HatchLineDefinition line,
            SKColor hatchColor,
            float scale,
            PointF origin,
            bool isAntialiasEnabled)
        {
            float spacing = Math.Abs(line.Spacing * scale);
            if (!float.IsFinite(spacing) || spacing <= 0.0f)
                return;

            float radians = line.AngleDegrees * MathF.PI / 180.0f;
            SKPoint direction = new(MathF.Cos(radians), -MathF.Sin(radians));
            SKPoint normal = new(-direction.Y, direction.X);

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

            using SKPaint paint = new()
            {
                Style = SKPaintStyle.Stroke,
                Color = hatchColor,
                StrokeWidth = 1.0f,
                StrokeCap = SKStrokeCap.Butt,
                IsAntialias = isAntialiasEnabled
            };

            if (line.DashPattern is { Length: > 0 })
            {
                float[] dash = line.DashPattern
                    .Select(value => Math.Max(0.1f, Math.Abs(value * scale)))
                    .ToArray();
                paint.PathEffect = SKPathEffect.CreateDash(dash, 0.0f);
            }

            for (int index = firstIndex; index <= lastIndex; index++)
            {
                float distance = originDistance + index * spacing;
                float centerOffset = distance - boundsCenterDistance;
                SKPoint center = new(
                    boundsCenterX + normal.X * centerOffset,
                    boundsCenterY + normal.Y * centerOffset);
                SKPoint a = new(
                    center.X - direction.X * diagonal,
                    center.Y - direction.Y * diagonal);
                SKPoint b = new(
                    center.X + direction.X * diagonal,
                    center.Y + direction.Y * diagonal);
                canvas.DrawLine(a, b, paint);
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

        private static float Dot(float x, float y, SKPoint normal) =>
            x * normal.X + y * normal.Y;

        private static float MinDot(SKRect bounds, SKPoint normal) =>
            Math.Min(
                Math.Min(Dot(bounds.Left, bounds.Top, normal), Dot(bounds.Right, bounds.Top, normal)),
                Math.Min(Dot(bounds.Left, bounds.Bottom, normal), Dot(bounds.Right, bounds.Bottom, normal)));

        private static float MaxDot(SKRect bounds, SKPoint normal) =>
            Math.Max(
                Math.Max(Dot(bounds.Left, bounds.Top, normal), Dot(bounds.Right, bounds.Top, normal)),
                Math.Max(Dot(bounds.Left, bounds.Bottom, normal), Dot(bounds.Right, bounds.Bottom, normal)));

        private static SKColor ToSkiaColor(Color color) =>
            new(color.R, color.G, color.B, color.A);
    }
}
