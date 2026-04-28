using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;

namespace Land_Readjustment_Tool.UI.MapCanvas.Services
{
    /// <summary>
    /// Provides layer-tree-friendly queries and updates without pushing
    /// persistence details into WinForms code.
    /// </summary>
    public sealed class CanvasLayerTreeService
    {
        public const string RasterLayerType = "Raster";

        private readonly ICanvasLayerRepository _canvasLayerRepository;

        public CanvasLayerTreeService(ICanvasLayerRepository canvasLayerRepository)
        {
            _canvasLayerRepository = canvasLayerRepository
                ?? throw new ArgumentNullException(nameof(canvasLayerRepository));
        }

        public async Task<IReadOnlyList<CanvasLayer>> GetRasterLayersAsync(
            CancellationToken ct = default)
        {
            return await _canvasLayerRepository.GetAllByLayerTypeOrderedAsync(
                RasterLayerType,
                ct);
        }

        public async Task<IReadOnlyList<CanvasLayer>> GetAllLayersAsync(
            CancellationToken ct = default)
        {
            return await _canvasLayerRepository.GetAllOrderedAsync(ct);
        }

        public async Task<CanvasLayer?> SetVisibilityAsync(
            int layerId,
            bool isVisible,
            CancellationToken ct = default)
        {
            CanvasLayer? layer = await _canvasLayerRepository.GetByIDAsync(layerId, ct);
            if (layer == null)
            {
                return null;
            }

            if (layer.IsVisible == isVisible)
            {
                return layer;
            }

            layer.IsVisible = isVisible;
            layer.LastModifiedDate = DateTime.Now;
            await _canvasLayerRepository.UpdateAsync(layer, ct);
            return layer;
        }
    }
}
