using Land_Readjustment_Tool.UI.MapCanvas.Core;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.UI.MapCanvas.Rendering
{
    public interface IRasterRenderLayer : IDisposable
    {
        int LayerId { get; }
        string Name { get; }
        string FilePath { get; }
        RectangleD WorldBounds { get; }
        int Transparency { get; }
        bool IsVisible { get; }
        bool CanRenderFromMemoryCacheDuringInteraction { get; }

        bool RenderVisible(
            Graphics graphics,
            MapCanvasEngine engine,
            RectangleD visibleWorldBounds,
            bool interactive,
            CancellationToken cancellationToken = default);

        void UpdateRenderState(bool isVisible, int transparency);

        void InvalidateCache();
    }
}
