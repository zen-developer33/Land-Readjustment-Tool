using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    public static class MapCanvasSettingsService
    {
        public static MapCanvasRenderSettings FromProjectSettings(ProjectSettings? projectSettings)
        {
            MapCanvasRenderSettings settings = MapCanvasRenderSettings.CreateLightDefaults();

            if (projectSettings == null)
            {
                return settings;
            }

            Color backgroundColor = ParseColorOrDefault(
                projectSettings.CanvasBackgroundColor,
                settings.BackgroundColor);

            Color gridColor = ParseColorOrDefault(
                projectSettings.CanvasGridColor,
                settings.MajorGridColor);

            settings.BackgroundColor = backgroundColor;
            settings.MajorGridColor = Color.FromArgb(150, gridColor.R, gridColor.G, gridColor.B);
            settings.MinorGridColor = Color.FromArgb(70, gridColor.R, gridColor.G, gridColor.B);
            settings.GridLabelColor = GetReadableTextColor(backgroundColor, lightAlpha: 130, darkAlpha: 145);
            settings.OverlayTextColor = GetReadableTextColor(backgroundColor, lightAlpha: 230, darkAlpha: 225);
            settings.CoordinateTextColor = IsDark(backgroundColor)
                ? Color.FromArgb(118, 222, 157)
                : Color.FromArgb(21, 104, 139);
            settings.OverlayBackgroundColor = IsDark(backgroundColor)
                ? Color.FromArgb(220, 22, 29, 34)
                : Color.FromArgb(235, 255, 255, 255);
            settings.OverlayBorderColor = IsDark(backgroundColor)
                ? Color.FromArgb(125, 105, 134, 142)
                : Color.FromArgb(170, 178, 190, 201);
            settings.AccentColor = IsDark(backgroundColor)
                ? Color.FromArgb(78, 201, 176)
                : Color.FromArgb(41, 128, 185);
            settings.ShowGrid = projectSettings.CanvasGridVisible;

            return settings;
        }

        private static Color ParseColorOrDefault(string? htmlColor, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(htmlColor))
            {
                return fallback;
            }

            try
            {
                return ColorTranslator.FromHtml(htmlColor);
            }
            catch
            {
                return fallback;
            }
        }

        private static bool IsDark(Color color)
        {
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return luminance < 0.45;
        }

        private static Color GetReadableTextColor(Color backgroundColor, int lightAlpha, int darkAlpha)
        {
            return IsDark(backgroundColor)
                ? Color.FromArgb(lightAlpha, 232, 239, 241)
                : Color.FromArgb(darkAlpha, 49, 61, 72);
        }
    }
}
