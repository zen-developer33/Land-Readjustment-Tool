using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering;
using Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions;

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

            settings.AxisXColor = IsDark(backgroundColor)
                ? Color.FromArgb(218, 225, 97, 97)
                : Color.FromArgb(206, 72, 72);

            settings.AxisYColor = IsDark(backgroundColor)
                ? Color.FromArgb(218, 115, 204, 135)
                : Color.FromArgb(70, 162, 92);

            settings.AxisMarkerColor = IsDark(backgroundColor)
                ? Color.FromArgb(235, 243, 246)
                : Color.FromArgb(44, 58, 71);

            settings.AxisLabelColor = IsDark(backgroundColor)
                ? Color.FromArgb(235, 243, 246)
                : Color.FromArgb(54, 70, 82);

            settings.ShowGrid = projectSettings.CanvasGridVisible;
            settings.ShowMinorGridLines = ShouldShowMinorGridLines(projectSettings.CanvasGridMode);
            settings.ShowAxisLines = projectSettings.CanvasAxisMarkerVisible;
            settings.ShowOriginMarker = projectSettings.CanvasAxisMarkerVisible;
            settings.ShowAxisLabels = projectSettings.CanvasAxisMarkerVisible;
            settings.ShowNorthMarker = projectSettings.CanvasNorthMarkerVisible;
            settings.AntiAliasingEnabled = projectSettings.CanvasAntiAliasingEnabled;
            settings.ZoomBehavior = ParseZoomBehavior(projectSettings.CanvasZoomBehavior);
            settings.RenderBackend = Enum.TryParse(
                projectSettings.CanvasRenderBackend,
                ignoreCase: true,
                out MapRenderBackend backend)
                ? backend
                : MapRenderBackend.GdiPlus;

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

        private static MapCanvasZoomBehavior ParseZoomBehavior(string? value)
        {
            return Enum.TryParse(value, ignoreCase: true, out MapCanvasZoomBehavior behavior)
                ? behavior
                : MapCanvasZoomBehavior.StandardScaleSteps;
        }

        private static bool ShouldShowMinorGridLines(string? gridMode)
        {
            return string.Equals(
                gridMode,
                ProjectSettings.GridModeMajorAndMinor,
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
