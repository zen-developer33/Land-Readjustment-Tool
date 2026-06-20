namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Backend-neutral line pattern names used by strokes.
    /// </summary>
    public enum DashPatternKind
    {
        /// <summary>Continuous line.</summary>
        Solid,

        /// <summary>Repeating dash pattern.</summary>
        Dashed,

        /// <summary>Repeating dot pattern.</summary>
        Dotted,

        /// <summary>Dash followed by dot.</summary>
        DashDot,

        /// <summary>Dash followed by two dots.</summary>
        DashDoubleDot,

        /// <summary>Long dash and short dash pattern commonly used for centerlines.</summary>
        CenterLine,

        /// <summary>Reserved for future named/custom patterns.</summary>
        Custom
    }

    /// <summary>
    /// Describes how an enclosed shape should be filled.
    /// </summary>
    public enum FillPatternKind
    {
        /// <summary>Fill with one solid color.</summary>
        Solid,

        /// <summary>Fill using a built-in hatch pattern.</summary>
        Hatch,

        /// <summary>Fill using a generated texture/tile hatch pattern.</summary>
        TextureHatch
    }

    /// <summary>
    /// Controls how overlapping path contours decide the filled region.
    /// </summary>
    public enum FillRule
    {
        /// <summary>Non-zero winding fill rule.</summary>
        Winding,

        /// <summary>Alternate/even-odd fill rule, useful for donut polygons.</summary>
        Alternate
    }

    /// <summary>
    /// Image resampling preference used when drawing raster/cache frames.
    /// </summary>
    public enum ImageInterpolation
    {
        /// <summary>Fast pixel-preserving interpolation for map tiles and cache frames.</summary>
        NearestNeighbor,

        /// <summary>Higher-quality interpolation for scaled imagery when visual quality matters.</summary>
        HighQuality
    }

    /// <summary>
    /// Backend-neutral stroke end-cap style.
    /// </summary>
    public enum LineCapKind
    {
        /// <summary>Ends exactly at the path endpoint.</summary>
        Flat,

        /// <summary>Extends half a stroke width beyond the endpoint with a square cap.</summary>
        Square,

        /// <summary>Uses a rounded cap at the endpoint.</summary>
        Round
    }

    /// <summary>
    /// Backend-neutral stroke join style.
    /// </summary>
    public enum LineJoinKind
    {
        /// <summary>Sharp corner join.</summary>
        Miter,

        /// <summary>Beveled corner join.</summary>
        Bevel,

        /// <summary>Rounded corner join.</summary>
        Round
    }

    /// <summary>
    /// Named quality presets used by render stages.
    /// </summary>
    public enum RenderQuality
    {
        /// <summary>High quality vector drawing for settled frames.</summary>
        VectorHighQuality,

        /// <summary>Faster vector drawing for interaction or low-quality modes.</summary>
        VectorHighSpeed,

        /// <summary>Fast raster drawing for tiles and bitmap cache frames.</summary>
        RasterHighSpeed
    }

    /// <summary>
    /// Horizontal or vertical text alignment inside a layout rectangle.
    /// </summary>
    public enum TextAlign
    {
        /// <summary>Near edge: left for horizontal text, top for vertical alignment.</summary>
        Near,

        /// <summary>Centered alignment.</summary>
        Center,

        /// <summary>Far edge: right for horizontal text, bottom for vertical alignment.</summary>
        Far
    }
}
