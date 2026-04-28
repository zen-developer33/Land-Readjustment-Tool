using System.Drawing;
using Land_Readjustment_Tool.Core.Entities.Project;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class MapCanvasRenderSettings
    {
        public Color BackgroundColor { get; set; }
        public Color MinorGridColor { get; set; }
        public Color MajorGridColor { get; set; }
        public Color GridLabelColor { get; set; }
        public Color OverlayBackgroundColor { get; set; }
        public Color OverlayBorderColor { get; set; }
        public Color OverlayTextColor { get; set; }
        public Color CoordinateTextColor { get; set; }
        public Color AccentColor { get; set; }
        public bool ShowGrid { get; set; } = true;
        public bool ShowGridLabels { get; set; } = true;
        public bool ShowCoordinateOverlay { get; set; } = true;
        public bool ShowOriginMarker { get; set; } = true;

        /// <summary>
        /// Creates render settings from project settings entity.
        /// Uses actual project canvas colors if available.
        /// </summary>
        public static MapCanvasRenderSettings CreateFromProjectSettings(ProjectSettings projectSettings)
        {
            Color backgroundColor;
            Color gridColor;

            try
            {
                backgroundColor = ColorTranslator.FromHtml(projectSettings.CanvasBackgroundColor);
            }
            catch
            {
                backgroundColor = Color.White;
            }

            try
            {
                gridColor = ColorTranslator.FromHtml(projectSettings.CanvasGridColor);
            }
            catch
            {
                gridColor = Color.FromArgb(42, 58, 71);
            }

            return new MapCanvasRenderSettings
            {
                BackgroundColor = backgroundColor,
                MinorGridColor = Color.FromArgb(100, gridColor.R, gridColor.G, gridColor.B),
                MajorGridColor = Color.FromArgb(200, gridColor.R, gridColor.G, gridColor.B),
                GridLabelColor = gridColor,
                OverlayBackgroundColor = Color.FromArgb(235, 255, 255, 255),
                OverlayBorderColor = Color.FromArgb(170, 178, 190, 201),
                OverlayTextColor = Color.FromArgb(42, 55, 66),
                CoordinateTextColor = Color.FromArgb(21, 104, 139),
                AccentColor = Color.FromArgb(41, 128, 185),
                ShowGrid = projectSettings.CanvasGridVisible,
                ShowGridLabels = true,
                ShowCoordinateOverlay = true,
                ShowOriginMarker = true,
            };
        }

        public static MapCanvasRenderSettings CreateLightDefaults()
        {
            return new MapCanvasRenderSettings
            {
                BackgroundColor = Color.White,
                MinorGridColor = Color.FromArgb(222, 232, 239),
                MajorGridColor = Color.FromArgb(185, 202, 214),
                GridLabelColor = Color.FromArgb(101, 116, 130),
                OverlayBackgroundColor = Color.FromArgb(235, 255, 255, 255),
                OverlayBorderColor = Color.FromArgb(170, 178, 190, 201),
                OverlayTextColor = Color.FromArgb(42, 55, 66),
                CoordinateTextColor = Color.FromArgb(21, 104, 139),
                AccentColor = Color.FromArgb(41, 128, 185),
                ShowGrid = true,
                ShowGridLabels = true,
                ShowCoordinateOverlay = true,
                ShowOriginMarker = true,
            };
        }

        public static MapCanvasRenderSettings CreateDarkDefaults()
        {
            return new MapCanvasRenderSettings
            {
                BackgroundColor = Color.FromArgb(31, 39, 46),
                MinorGridColor = Color.FromArgb(34, 58, 68),
                MajorGridColor = Color.FromArgb(67, 97, 111),
                GridLabelColor = Color.FromArgb(140, 160, 168),
                OverlayBackgroundColor = Color.FromArgb(220, 22, 29, 34),
                OverlayBorderColor = Color.FromArgb(120, 115, 145, 154),
                OverlayTextColor = Color.FromArgb(222, 233, 236),
                CoordinateTextColor = Color.FromArgb(118, 222, 157),
                AccentColor = Color.FromArgb(78, 201, 176),
                ShowGrid = true,
                ShowGridLabels = true,
                ShowCoordinateOverlay = true
            };
        }

        public MapCanvasRenderSettings Clone()
        {
            return (MapCanvasRenderSettings)MemberwiseClone();
        }
    }
}
