using Land_Readjustment_Tool.Core.Entities.Canvas;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Repository contract for persisted canvas layers.
    /// </summary>
    public interface ICanvasLayerRepository : IRepository<CanvasLayer>
    {
        Task<List<CanvasLayer>> GetAllOrderedAsync(
            CancellationToken ct = default);

        Task<List<CanvasLayer>> GetAllVisibleOrderedAsync(
            CancellationToken ct = default);

        Task<List<CanvasLayer>> GetAllByLayerTypeOrderedAsync(
            string layerType,
            CancellationToken ct = default);

        Task<CanvasLayer?> GetByNameAsync(
            string name,
            CancellationToken ct = default);
    }
}
