namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Backend-neutral description of how a line or path should be stroked.
    /// </summary>
    /// <param name="Color">Stroke color, including alpha.</param>
    /// <param name="Width">Stroke width in screen pixels.</param>
    /// <param name="DashPattern">Named dash pattern.</param>
    /// <param name="DashScale">Scale multiplier for dash spacing.</param>
    /// <param name="Cap">Line end-cap style.</param>
    /// <param name="Join">Corner join style.</param>
    public readonly record struct StrokeStyle(
        Color Color,
        float Width,
        DashPatternKind DashPattern = DashPatternKind.Solid,
        float DashScale = 1.0f,
        LineCapKind Cap = LineCapKind.Round,
        LineJoinKind Join = LineJoinKind.Round);
}
