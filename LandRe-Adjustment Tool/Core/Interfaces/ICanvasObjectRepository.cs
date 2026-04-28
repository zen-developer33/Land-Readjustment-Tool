using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Repository contract for persisted canvas objects (GUID primary key).
    /// </summary>
    public interface ICanvasObjectRepository
    {
        Task<CanvasObject?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default);

        Task<List<CanvasObject>> GetAllAsync(
            CancellationToken ct = default);

        Task<List<CanvasObject>> GetAllVisibleAsync(
            CancellationToken ct = default);

        Task<List<CanvasObject>> GetByLayerIdAsync(
            int canvasLayerId,
            CancellationToken ct = default);

        Task<List<CanvasObject>> QueryByViewportAsync(
            RectangleD viewportWorldBounds,
            CancellationToken ct = default);

        Task<CanvasObject> AddAsync(
            CanvasObject entity,
            CancellationToken ct = default);

        Task UpdateAsync(
            CanvasObject entity,
            CancellationToken ct = default);

        Task DeleteAsync(
            Guid id,
            CancellationToken ct = default);

        Task<bool> ExistsAsync(
            Guid id,
            CancellationToken ct = default);
    }
}
