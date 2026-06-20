namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Backend-neutral description of how map text should be measured and drawn.
    /// </summary>
    /// <param name="FontFamily">Preferred font family name.</param>
    /// <param name="SizePx">Font size in screen pixels.</param>
    /// <param name="Color">Text color, including alpha.</param>
    /// <param name="Bold">Whether the font should be bold.</param>
    /// <param name="HorizontalAlign">Horizontal alignment inside the layout rectangle.</param>
    /// <param name="VerticalAlign">Vertical alignment inside the layout rectangle.</param>
    /// <param name="RotationDegrees">Clockwise screen-space rotation in degrees.</param>
    /// <param name="RotationOrigin">
    /// Optional screen-space point used as the rotation origin. When omitted,
    /// the backend rotates around the center of the layout rectangle.
    /// </param>
    public readonly record struct TextStyle(
        string FontFamily,
        float SizePx,
        Color Color,
        bool Bold = false,
        TextAlign HorizontalAlign = TextAlign.Near,
        TextAlign VerticalAlign = TextAlign.Near,
        float RotationDegrees = 0.0f,
        PointF? RotationOrigin = null);
}
