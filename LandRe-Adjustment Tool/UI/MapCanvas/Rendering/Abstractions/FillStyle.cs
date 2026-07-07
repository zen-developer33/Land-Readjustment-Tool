namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Backend-neutral description of how an enclosed shape should be filled.
    /// </summary>
    /// <param name="Color">Base fill color, including alpha.</param>
    /// <param name="Pattern">Fill pattern type.</param>
    /// <param name="PatternColor">Foreground color for hatch or texture patterns.</param>
    /// <param name="PatternScale">Scale used by generated pattern tiles.</param>
    /// <param name="PatternKey">Optional named hatch/pattern identifier.</param>
    /// <param name="PatternScreenScale">
    /// Screen pixels per pattern unit after applying the current map zoom.
    /// Hatch spacing stays in world units while this value changes as the user zooms.
    /// </param>
    /// <param name="PatternOriginScreen">Screen-space position of hatch world origin.</param>
    public readonly record struct FillStyle(
        Color Color,
        FillPatternKind Pattern = FillPatternKind.Solid,
        Color PatternColor = default,
        double PatternScale = 1.0,
        string? PatternKey = null,
        double PatternScreenScale = 1.0,
        PointF PatternOriginScreen = default);
}
