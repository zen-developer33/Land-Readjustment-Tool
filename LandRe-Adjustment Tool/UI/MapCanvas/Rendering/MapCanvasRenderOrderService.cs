namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    /// <summary>
    /// Defines the stable order used to draw the logical map-canvas render passes.
    /// </summary>
    public sealed class MapCanvasRenderOrderService
    {
        private static readonly IReadOnlyList<MapCanvasRenderStage> FrameStages =
        [
            MapCanvasRenderStage.FixedReference,
            MapCanvasRenderStage.RasterContent,
            MapCanvasRenderStage.VectorContent,
            MapCanvasRenderStage.InteractionOverlay
        ];

        /// <summary>
        /// Creates a render-order service with the default map-canvas stage order.
        /// </summary>
        public MapCanvasRenderOrderService()
        {
        }

        /// <summary>
        /// Gets the ordered render stages for one full canvas frame.
        /// </summary>
        /// <returns>The render stages in the order they should be drawn.</returns>
        public IReadOnlyList<MapCanvasRenderStage> GetFrameStages()
        {
            return FrameStages;
        }
    }

    /// <summary>
    /// Represents one logical pass in the map-canvas frame renderer.
    /// </summary>
    public enum MapCanvasRenderStage
    {
        /// <summary>
        /// Draws fixed reference visuals such as grid lines and origin markers.
        /// </summary>
        FixedReference,

        /// <summary>
        /// Draws raster or basemap content for the current viewport.
        /// </summary>
        RasterContent,

        /// <summary>
        /// Draws vector content for the current viewport.
        /// </summary>

        VectorContent,
        /// <summary>
        /// Draws temporary interaction feedback such as zoom-window selection.
        /// </summary>
        InteractionOverlay
    }
}
