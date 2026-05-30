using System.Drawing;

namespace Land_Readjustment_Tool.Services.Canvas
{
    /// <summary>
    /// Service for adjusting layer colors based on canvas theme brightness.
    /// Ensures layer colors maintain adequate contrast with the canvas background.
    /// </summary>
    public sealed class CanvasThemeColorService
    {
        /// <summary>
        /// Threshold for determining if canvas is dark or darkish.
        /// </summary>
        private const double DarkishCanvasThreshold = 0.65;

        /// <summary>
        /// Calculates the relative luminance of a color using the relative luminance formula.
        /// </summary>
        /// <param name="color">The color to analyze.</param>
        /// <returns>Luminance value between 0 (darkest) and 1 (lightest).</returns>
        public static double CalculateLuminance(Color color)
        {
            return (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
        }

        /// Determines if the canvas background is dark enough that black layer
        /// colors need to be lifted for visibility.
        /// </summary>
        /// <param name="canvasColor">The canvas background color.</param>
        /// <returns>True if the canvas is dark or darkish; otherwise false.</returns>
        public static bool IsDarkCanvas(Color canvasColor)
        {
            return CalculateLuminance(canvasColor) < DarkishCanvasThreshold;
        }

        /// <summary>
        /// Adjusts a layer color based on canvas theme.
        /// Only pure black and pure white are swapped; all other colors remain unchanged.
        /// </summary>
        /// <param name="canvasColor">The canvas background color.</param>
        /// <param name="layerColor">The original layer color.</param>
        /// <returns>Adjusted color based on the canvas theme.</returns>
        public static Color AdjustColorForCanvasTheme(Color canvasColor, Color layerColor)
        {
            bool isCanvasDark = IsDarkCanvas(canvasColor);

            if (isCanvasDark)
            {
                if (IsBlack(layerColor))
                    return Color.White;

                // White remains white on dark and darkish canvas themes.
                return layerColor;
            }

            if (IsWhite(layerColor))
                return Color.Black;

            // Black remains black on light canvas themes.
            return layerColor;
        }

        /// <summary>
        /// Adjusts a hex color string based on canvas theme to ensure visibility and contrast.
        /// </summary>
        /// <param name="canvasColor">The canvas background color.</param>
        /// <param name="hexColor">The original layer color as hex string (e.g., "#FF0000").</param>
        /// <returns>Adjusted hex color string that maintains contrast with the canvas background.</returns>
        public static string AdjustHexColorForCanvasTheme(Color canvasColor, string hexColor)
        {
            try
            {
                Color layerColor = ColorTranslator.FromHtml(hexColor);
                Color adjustedColor = AdjustColorForCanvasTheme(canvasColor, layerColor);
                return ToHtml(adjustedColor);
            }
            catch
            {
                return hexColor;
            }
        }

        /// <summary>
        /// Adjusts a nullable hex color string while preserving null/empty values.
        /// </summary>
        /// <param name="canvasColor">The canvas background color.</param>
        /// <param name="hexColor">The original color as hex string.</param>
        /// <returns>The adjusted hex string, or the original null/empty value.</returns>
        public static string? AdjustNullableHexColorForCanvasTheme(
            Color canvasColor,
            string? hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor))
                return hexColor;

            return AdjustHexColorForCanvasTheme(canvasColor, hexColor);
        }

        /// <summary>
        /// Adjusts multiple layer colors (BorderColor, FillColor, LabelColor) based on canvas theme.
        /// </summary>
        /// <param name="canvasColor">The canvas background color.</param>
        /// <param name="borderColorHex">The border color as hex string.</param>
        /// <param name="fillColorHex">The fill color as hex string.</param>
        /// <param name="labelColorHex">The label color as hex string.</param>
        /// <returns>Tuple of adjusted hex color strings (border, fill, label).</returns>
        public static (string borderColor, string fillColor, string labelColor) AdjustLayerColorsForCanvasTheme(
            Color canvasColor,
            string borderColorHex,
            string fillColorHex,
            string labelColorHex)
        {
            return (
                AdjustHexColorForCanvasTheme(canvasColor, borderColorHex),
                AdjustHexColorForCanvasTheme(canvasColor, fillColorHex),
                AdjustHexColorForCanvasTheme(canvasColor, labelColorHex)
            );
        }

        private static bool IsBlack(Color color)
        {
            return color.R <= 8 && color.G <= 8 && color.B <= 8;
        }

        private static bool IsWhite(Color color)
        {
            return color.R >= 247 && color.G >= 247 && color.B >= 247;
        }

        private static string ToHtml(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
