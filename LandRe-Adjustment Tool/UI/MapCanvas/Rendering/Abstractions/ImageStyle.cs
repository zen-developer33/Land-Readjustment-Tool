namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering.Abstractions
{
    /// <summary>
    /// Backend-neutral description of how a bitmap/image should be composited.
    /// </summary>
    /// <param name="Opacity">Image opacity from 0.0 to 1.0.</param>
    /// <param name="Interpolation">Interpolation mode used when scaling.</param>
    /// <param name="TileFlipXY">
    /// Whether image edge sampling should mirror at tile boundaries where the
    /// backend supports that behavior. This is mainly used by raster/tile layers
    /// to reduce seam artifacts.
    /// </param>
    public readonly record struct ImageStyle(
        float Opacity = 1.0f,
        ImageInterpolation Interpolation = ImageInterpolation.NearestNeighbor,
        bool TileFlipXY = false);
}
