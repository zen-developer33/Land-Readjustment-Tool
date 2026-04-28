using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Canvas
{
    /// <summary>
    /// Handles database operations for canvas layers.
    /// </summary>
    public sealed class CanvasLayerRepository
        : BaseRepository<CanvasLayer>, ICanvasLayerRepository
    {
        public CanvasLayerRepository(ProjectSession session)
            : base(session)
        {
        }

        public async Task<List<CanvasLayer>> GetAllOrderedAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
                    .OrderBy(layer => layer.DisplayOrder)
                    .ThenBy(layer => layer.Name)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError("GetAllOrderedAsync failed.", ex);
                throw;
            }
        }

        public async Task<List<CanvasLayer>> GetAllVisibleOrderedAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
                    .Where(layer => layer.IsVisible)
                    .OrderBy(layer => layer.DisplayOrder)
                    .ThenBy(layer => layer.Name)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError("GetAllVisibleOrderedAsync failed.", ex);
                throw;
            }
        }

        public async Task<CanvasLayer?> GetByNameAsync(
            string name,
            CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }

                string normalized = name.Trim();

                return await DbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        layer => layer.Name == normalized,
                        ct);
            }
            catch (Exception ex)
            {
                Logger.LogError($"GetByNameAsync failed. Name={name}", ex);
                throw;
            }
        }
    }
}
