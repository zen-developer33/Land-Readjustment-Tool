using Land_Readjustment_Tool.Core.Entities.Canvas;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.UI.MapCanvas.Models.Shapes;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Land_Readjustment_Tool.Repositories.Canvas
{
    /// <summary>
    /// Handles database operations for canvas objects (GUID key).
    /// </summary>
    public sealed class CanvasObjectRepository : ICanvasObjectRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<CanvasObject> _dbSet;
        private readonly Infrastructure.Logging.IAppLogger _logger;

        public CanvasObjectRepository(ProjectSession session)
        {
            _context = session.GetDbContext();
            _dbSet = _context.Set<CanvasObject>();
            _logger = session.Logger;
        }

        public async Task<CanvasObject?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.LandOwner)
                    .FirstOrDefaultAsync(item => item.Id == id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CanvasObject] GetByIdAsync failed. Id={id}", ex);
                throw;
            }
        }

        public async Task<List<CanvasObject>> GetAllAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.LandOwner)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] GetAllAsync failed.", ex);
                throw;
            }
        }

        public async Task<List<CanvasObject>> GetAllVisibleAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.LandOwner)
                    .Where(item => item.IsVisible)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] GetAllVisibleAsync failed.", ex);
                throw;
            }
        }

        public async Task<List<CanvasObject>> GetByLayerIdAsync(
            int canvasLayerId,
            CancellationToken ct = default)
        {
            try
            {
                return await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.LandOwner)
                    .Where(item => item.CanvasLayerId == canvasLayerId)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"[CanvasObject] GetByLayerIdAsync failed. LayerId={canvasLayerId}",
                    ex);
                throw;
            }
        }

        public async Task<List<CanvasObject>> QueryByViewportAsync(
            RectangleD viewportWorldBounds,
            CancellationToken ct = default)
        {
            try
            {
                // EF SQLite + NetTopologySuite does not reliably translate every envelope
                // expression, so we filter in memory for predictable behavior.
                List<CanvasObject> candidates = await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Include(item => item.BaselineParcel)
                        .ThenInclude(parcel => parcel!.LandOwner)
                    .Where(item => item.IsVisible)
                    .ToListAsync(ct);

                Envelope viewportEnvelope = new(
                    viewportWorldBounds.Left,
                    viewportWorldBounds.Right,
                    viewportWorldBounds.Top,
                    viewportWorldBounds.Bottom);

                return candidates
                    .Where(item => item.Shape != null &&
                                   item.Shape.EnvelopeInternal != null &&
                                   item.Shape.EnvelopeInternal.Intersects(viewportEnvelope))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] QueryByViewportAsync failed.", ex);
                throw;
            }
        }

        public async Task<CanvasObject> AddAsync(
            CanvasObject entity,
            CancellationToken ct = default)
        {
            try
            {
                await _dbSet.AddAsync(entity, ct);
                await _context.SaveChangesAsync(ct);

                _context.Entry(entity).State = EntityState.Detached;
                _logger.LogInfo($"[CanvasObject] added and saved. Id={entity.Id}");
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] AddAsync failed.", ex);
                throw;
            }
        }

        public async Task UpdateAsync(
            CanvasObject entity,
            CancellationToken ct = default)
        {
            try
            {
                // Prevent duplicate tracked instances for the same key.
                var localTracked = _context
                    .ChangeTracker
                    .Entries<CanvasObject>()
                    .FirstOrDefault(entry => entry.Entity.Id == entity.Id);

                if (localTracked is not null)
                {
                    localTracked.State = EntityState.Detached;
                }

                _dbSet.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync(ct);

                _context.Entry(entity).State = EntityState.Detached;
                _logger.LogInfo($"[CanvasObject] updated and saved. Id={entity.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"[CanvasObject] UpdateAsync failed. Id={entity.Id}",
                    ex);
                throw;
            }
        }

        public async Task DeleteAsync(
            Guid id,
            CancellationToken ct = default)
        {
            try
            {
                CanvasObject? entity = await _dbSet.FindAsync(
                    new object[] { id },
                    ct);

                if (entity == null)
                {
                    _logger.LogWarning($"[CanvasObject] Delete: not found. Id={id}");
                    return;
                }

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(ct);
                _logger.LogInfo($"[CanvasObject] deleted and saved. Id={id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CanvasObject] DeleteAsync failed. Id={id}", ex);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(
            Guid id,
            CancellationToken ct = default)
        {
            try
            {
                return await _dbSet.AnyAsync(item => item.Id == id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[CanvasObject] ExistsAsync failed. Id={id}", ex);
                throw;
            }
        }
    }
}
