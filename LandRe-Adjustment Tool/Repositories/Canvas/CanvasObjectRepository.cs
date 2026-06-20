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
        private const int WriteBatchSize = 1000;
        private const int IdBatchSize = 500;
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
                CanvasObject? entity = await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .FirstOrDefaultAsync(item => item.Id == id, ct);

                if (entity != null)
                    await HydrateScalarLinkedRecordsAsync([entity], ct);

                return entity;
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
                List<CanvasObject> objects = await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .ToListAsync(ct);

                await HydrateScalarLinkedRecordsAsync(objects, ct);
                return objects;
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
                List<CanvasObject> objects = await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Where(item => item.IsVisible)
                    .ToListAsync(ct);

                await HydrateScalarLinkedRecordsAsync(objects, ct);
                return objects;
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
                List<CanvasObject> objects = await _dbSet
                    .AsNoTracking()
                    .Include(item => item.CanvasLayer)
                    .Where(item => item.CanvasLayerId == canvasLayerId)
                    .ToListAsync(ct);

                await HydrateScalarLinkedRecordsAsync(objects, ct);
                return objects;
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
                    .Where(item => item.IsVisible)
                    .ToListAsync(ct);

                await HydrateScalarLinkedRecordsAsync(candidates, ct);

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

        public async Task AddRangeAsync(
            IReadOnlyList<CanvasObject> entities,
            CancellationToken ct = default)
        {
            if (entities.Count == 0)
            {
                return;
            }

            bool previousAutoDetectChanges = _context.ChangeTracker.AutoDetectChangesEnabled;
            bool ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction
                ? await _context.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                foreach (CanvasObject[] batch in Chunk(entities, WriteBatchSize))
                {
                    foreach (CanvasObject entity in batch)
                    {
                        ClearNavigationProperties(entity);
                    }

                    await _dbSet.AddRangeAsync(batch, ct);
                    await _context.SaveChangesAsync(ct);
                    DetachRange(batch);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(ct);
                }

                _logger.LogInfo($"[CanvasObject] added batch. Count={entities.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] AddRangeAsync failed.", ex);
                throw;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetectChanges;
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

                if (entity.CanvasLayer != null)
                    entity.CanvasLayerId = entity.CanvasLayer.Id;

                // Persist the canvas object as scalar state only. Grip edits may
                // arrive with no-tracking navigation objects from the map cache;
                // attaching those can collide with already tracked layer/parcel
                // instances in the shared project DbContext.
                ClearNavigationProperties(entity);

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

        public async Task UpdateRangeAsync(
            IReadOnlyList<CanvasObject> entities,
            CancellationToken ct = default)
        {
            if (entities.Count == 0)
            {
                return;
            }

            bool previousAutoDetectChanges = _context.ChangeTracker.AutoDetectChangesEnabled;
            bool ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction
                ? await _context.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                foreach (CanvasObject[] batch in Chunk(entities, WriteBatchSize))
                {
                    foreach (CanvasObject entity in batch)
                    {
                        DetachTrackedCanvasObject(entity.Id);
                        if (entity.CanvasLayer != null)
                            entity.CanvasLayerId = entity.CanvasLayer.Id;

                        ClearNavigationProperties(entity);
                        _dbSet.Attach(entity);
                        _context.Entry(entity).State = EntityState.Modified;
                    }

                    await _context.SaveChangesAsync(ct);
                    DetachRange(batch);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(ct);
                }

                _logger.LogInfo($"[CanvasObject] updated batch. Count={entities.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] UpdateRangeAsync failed.", ex);
                throw;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetectChanges;
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

        public async Task DeleteRangeAsync(
            IReadOnlyList<Guid> ids,
            CancellationToken ct = default)
        {
            Guid[] distinctIds = ids
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();

            if (distinctIds.Length == 0)
            {
                return;
            }

            bool ownsTransaction = _context.Database.CurrentTransaction == null;
            await using var transaction = ownsTransaction
                ? await _context.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                int deleted = 0;
                foreach (Guid[] batch in Chunk(distinctIds, IdBatchSize))
                {
                    deleted += await _dbSet
                        .Where(item => batch.Contains(item.Id))
                        .ExecuteDeleteAsync(ct);
                }

                if (transaction != null)
                {
                    await transaction.CommitAsync(ct);
                }

                _logger.LogInfo($"[CanvasObject] deleted batch. Requested={distinctIds.Length}; Deleted={deleted}");
            }
            catch (Exception ex)
            {
                _logger.LogError("[CanvasObject] DeleteRangeAsync failed.", ex);
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

        private void DetachTrackedCanvasObject(Guid id)
        {
            var localTracked = _context
                .ChangeTracker
                .Entries<CanvasObject>()
                .FirstOrDefault(entry => entry.Entity.Id == id);

            if (localTracked is not null)
            {
                localTracked.State = EntityState.Detached;
            }
        }

        private static void ClearNavigationProperties(CanvasObject entity)
        {
            entity.CanvasLayer = null!;
            entity.BaselineParcel = null;
            entity.ReplottedParcel = null;
            entity.Road = null;
            entity.Block = null;
        }

        private void DetachRange(IEnumerable<CanvasObject> entities)
        {
            foreach (CanvasObject entity in entities)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
        }

        private static IEnumerable<T[]> Chunk<T>(
            IReadOnlyList<T> source,
            int size)
        {
            for (int index = 0; index < source.Count; index += size)
            {
                int count = Math.Min(size, source.Count - index);
                T[] batch = new T[count];
                for (int offset = 0; offset < count; offset++)
                {
                    batch[offset] = source[index + offset];
                }

                yield return batch;
            }
        }

        private async Task HydrateScalarLinkedRecordsAsync(
            IReadOnlyList<CanvasObject> objects,
            CancellationToken ct)
        {
            if (objects.Count == 0)
                return;

            List<int> baselineParcelIds = objects
                .Where(item => item.BaselineParcel == null && item.BaselineParcelId.HasValue)
                .Select(item => item.BaselineParcelId!.Value)
                .Distinct()
                .ToList();

            if (baselineParcelIds.Count > 0)
            {
                var parcelsById = await _context.BaselineParcels
                    .AsNoTracking()
                    .Include(parcel => parcel.LandOwner)
                    .Include(parcel => parcel.MalpotReference)
                    .Where(parcel => baselineParcelIds.Contains(parcel.Id))
                    .ToDictionaryAsync(parcel => parcel.Id, ct);

                foreach (CanvasObject canvasObject in objects)
                {
                    if (canvasObject.BaselineParcel == null &&
                        canvasObject.BaselineParcelId.HasValue &&
                        parcelsById.TryGetValue(canvasObject.BaselineParcelId.Value, out var parcel))
                    {
                        canvasObject.BaselineParcel = parcel;
                    }
                }
            }

            List<int> roadIds = objects
                .Where(item => item.Road == null && item.RoadId.HasValue)
                .Select(item => item.RoadId!.Value)
                .Distinct()
                .ToList();

            if (roadIds.Count > 0)
            {
                var roadsById = await _context.Roads
                    .AsNoTracking()
                    .Where(road => roadIds.Contains(road.Id))
                    .ToDictionaryAsync(road => road.Id, ct);

                foreach (CanvasObject canvasObject in objects)
                {
                    if (canvasObject.Road == null &&
                        canvasObject.RoadId.HasValue &&
                        roadsById.TryGetValue(canvasObject.RoadId.Value, out var road))
                    {
                        canvasObject.Road = road;
                    }
                }
            }

            List<int> blockIds = objects
                .Where(item => item.Block == null && item.BlockId.HasValue)
                .Select(item => item.BlockId!.Value)
                .Distinct()
                .ToList();

            if (blockIds.Count > 0)
            {
                var blocksById = await _context.Blocks
                    .AsNoTracking()
                    .Where(block => blockIds.Contains(block.Id))
                    .ToDictionaryAsync(block => block.Id, ct);

                foreach (CanvasObject canvasObject in objects)
                {
                    if (canvasObject.Block == null &&
                        canvasObject.BlockId.HasValue &&
                        blocksById.TryGetValue(canvasObject.BlockId.Value, out var block))
                    {
                        canvasObject.Block = block;
                    }
                }
            }

            List<int> replottedParcelIds = objects
                .Where(item => item.ReplottedParcel == null && item.ReplottedParcelId.HasValue)
                .Select(item => item.ReplottedParcelId!.Value)
                .Distinct()
                .ToList();

            if (replottedParcelIds.Count > 0)
            {
                var replottedParcelsById = await _context.ReplottedParcels
                    .AsNoTracking()
                    .Include(parcel => parcel.Block)
                    .Include(parcel => parcel.PlotType)
                    .Where(parcel => replottedParcelIds.Contains(parcel.Id))
                    .ToDictionaryAsync(parcel => parcel.Id, ct);

                foreach (CanvasObject canvasObject in objects)
                {
                    if (canvasObject.ReplottedParcel == null &&
                        canvasObject.ReplottedParcelId.HasValue &&
                        replottedParcelsById.TryGetValue(
                            canvasObject.ReplottedParcelId.Value,
                            out var parcel))
                    {
                        canvasObject.ReplottedParcel = parcel;
                    }
                }
            }
        }
    }
}
