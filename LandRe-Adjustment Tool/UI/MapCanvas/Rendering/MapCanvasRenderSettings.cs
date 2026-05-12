using System.Drawing;
using System.Drawing.Drawing2D;
using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.UI.MapCanvas.Core;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public sealed class MapCanvasRenderSettings
    {
        public Color BackgroundColor { get; set; }
        public Color MinorGridColor { get; set; }
        public Color MajorGridColor { get; set; }
        public Color GridLabelColor { get; set; }

        public Color AxisXColor { get; set; }
        public Color AxisYColor { get; set; }
        public Color AxisMarkerColor { get; set; }
        public Color AxisLabelColor { get; set; }
        public float AxisLineWidth { get; set; } = 0.3f;
        public float AxisMarkerLineWidth { get; set; } = 0.7f;
        public float AxisMarkerLengthPx { get; set; } = 32.0f;
        public float AxisMarkerSquareSizePx { get; set; } = 7.0f;

        public Color OverlayBackgroundColor { get; set; }
        public Color OverlayBorderColor { get; set; }
        public Color OverlayTextColor { get; set; }
        public Color CoordinateTextColor { get; set; }
        public Color AccentColor { get; set; }
        public Color ZoomWindowBorderColor { get; set; }
        public Color ZoomWindowFillColor { get; set; }
        public float ZoomWindowLineWidth { get; set; } = 1.0f;
        public DashStyle ZoomWindowLineType { get; set; } = DashStyle.Solid;

        public bool ShowGrid { get; set; } = true;
        public bool ShowMinorGridLines { get; set; } = false;
        public bool ShowGridLabels { get; set; } = true;
        public bool ShowAxisLines { get; set; } = true;
        public bool ShowAxisLabels { get; set; } = true;
        public bool ShowCoordinateOverlay { get; set; } = true;
        public bool ShowOriginMarker { get; set; } = true;
        public bool ShowNorthMarker { get; set; } = false;
        public bool AntiAliasingEnabled { get; set; } = true;
        public MapCanvasZoomBehavior ZoomBehavior { get; set; } = MapCanvasZoomBehavior.StandardScaleSteps;

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
                AxisXColor = Color.FromArgb(205, 206, 68, 68),
                AxisYColor = Color.FromArgb(205, 74, 170, 96),
                AxisMarkerColor = IsDark(backgroundColor)
                    ? Color.FromArgb(232, 232, 232)
                    : Color.FromArgb(56, 67, 76),
                AxisLabelColor = IsDark(backgroundColor)
                    ? Color.FromArgb(238, 238, 238)
                    : Color.FromArgb(56, 67, 76),
                OverlayBackgroundColor = Color.FromArgb(235, 255, 255, 255),
                OverlayBorderColor = Color.FromArgb(170, 178, 190, 201),
                OverlayTextColor = Color.FromArgb(42, 55, 66),
                CoordinateTextColor = Color.FromArgb(21, 104, 139),
                AccentColor = Color.FromArgb(41, 128, 185),
                ZoomWindowBorderColor = Color.FromArgb(41, 128, 185),
                ZoomWindowFillColor = Color.FromArgb(45, 41, 128, 185),
                ZoomWindowLineWidth = 1.0f,
                ZoomWindowLineType = DashStyle.Dash,
                ShowGrid = projectSettings.CanvasGridVisible,
                ShowMinorGridLines = ShouldShowMinorGridLines(projectSettings.CanvasGridMode),
                ShowGridLabels = true,
                ShowAxisLines = projectSettings.CanvasAxisMarkerVisible,
                ShowAxisLabels = projectSettings.CanvasAxisMarkerVisible,
                ShowCoordinateOverlay = true,
                ShowOriginMarker = projectSettings.CanvasAxisMarkerVisible,
                ShowNorthMarker = projectSettings.CanvasNorthMarkerVisible,
                AntiAliasingEnabled = projectSettings.CanvasAntiAliasingEnabled,
                ZoomBehavior = ParseZoomBehavior(projectSettings.CanvasZoomBehavior),
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
                AxisXColor = Color.FromArgb(206, 72, 72),
                AxisYColor = Color.FromArgb(70, 162, 92),
                AxisMarkerColor = Color.FromArgb(44, 58, 71),
                AxisLabelColor = Color.FromArgb(54, 70, 82),
                OverlayBackgroundColor = Color.FromArgb(235, 255, 255, 255),
                OverlayBorderColor = Color.FromArgb(170, 178, 190, 201),
                OverlayTextColor = Color.FromArgb(42, 55, 66),
                CoordinateTextColor = Color.FromArgb(21, 104, 139),
                AccentColor = Color.FromArgb(41, 128, 185),
                ZoomWindowBorderColor = Color.FromArgb(41, 128, 185),
                ZoomWindowFillColor = Color.FromArgb(45, 41, 128, 185),
                ZoomWindowLineWidth = 1.0f,
                ZoomWindowLineType = DashStyle.Dash,
                ShowGrid = true,
                ShowMinorGridLines = false,
                ShowGridLabels = true,
                ShowAxisLines = true,
                ShowAxisLabels = true,
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
                AxisXColor = Color.FromArgb(218, 225, 97, 97),
                AxisYColor = Color.FromArgb(218, 115, 204, 135),
                AxisMarkerColor = Color.FromArgb(235, 243, 246),
                AxisLabelColor = Color.FromArgb(235, 243, 246),
                OverlayBackgroundColor = Color.FromArgb(220, 22, 29, 34),
                OverlayBorderColor = Color.FromArgb(120, 115, 145, 154),
                OverlayTextColor = Color.FromArgb(222, 233, 236),
                CoordinateTextColor = Color.FromArgb(118, 222, 157),
                AccentColor = Color.FromArgb(78, 201, 176),
                ZoomWindowBorderColor = Color.FromArgb(78, 201, 176),
                ZoomWindowFillColor = Color.FromArgb(45, 78, 201, 176),
                ZoomWindowLineWidth = 1.0f,
                ZoomWindowLineType = DashStyle.Dash,
                ShowGrid = true,
                ShowMinorGridLines = false,
                ShowGridLabels = true,
                ShowAxisLines = true,
                ShowAxisLabels = true,
                ShowCoordinateOverlay = true
            };
        }

        public MapCanvasRenderSettings Clone()
        {
            return (MapCanvasRenderSettings)MemberwiseClone();
        }

        private static bool IsDark(Color color)
        {
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return luminance < 0.45;
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
