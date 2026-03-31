using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Base
{

    /// <summary>
    /// Base class for all repositories.
    /// Contains common database operations shared by every repository.
    /// All repositories inherit from this class.
    /// </summary>
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        // ------ PROTECTED FIELDS ------
        /// <summary>EF Core database connection.</summary>
        protected readonly AppDbContext Context;

        /// <summary>Logger for errors and info messages.</summary>
        protected readonly IAppLogger Logger;

        /// <summary>
        /// Shortcut to the database table for entity T.
        /// Example: DbSet of LandOwner = tblLandOwners.
        /// </summary>
        protected readonly DbSet<T> DbSet;

        /// <summary>
        /// Gets context and logger from session.
        /// Called by child repositories using : base(session).
        /// </summary>
        protected BaseRepository(ProjectSession session)
        {
            Context = session.GetDbContext();
            Logger = session.Logger;
            DbSet = Context.Set<T>();
        }
        /// <summary>
        /// Gets one record by its primary key.
        /// Returns null if not found.
        /// Uses AsNoTracking — result is never added to the ChangeTracker.
        /// </summary>
        public virtual async Task<T?> GetByIDAsync(int id, CancellationToken ct = default)
        {
            try
            {
                // Resolve the primary key property name from EF Core metadata
                // so this works generically for every entity type.
                var keyName = Context.Model
                    .FindEntityType(typeof(T))
                    ?.FindPrimaryKey()
                    ?.Properties.FirstOrDefault()
                    ?.Name ?? "Id";

                return await DbSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        e => EF.Property<int>(e, keyName) == id, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] GetByIdAsync failed. Id = {id}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets all records from the table.
        /// AsNoTracking makes it faster for read-only queries.
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        {
            try
            {
                return await DbSet.AsNoTracking().ToListAsync(ct); //Retrieve all records from the database table asynchronously, without tracking them, and return them as a list.
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{typeof(T).Name}]" + $"GetAllAsync failed.", ex); //example: [LandOwner] GetAllAsync failed.
                throw;
            }
        }

        /// <summary>
        /// Inserts a new record and immediately commits to the database.
        /// Returns the entity with its database-assigned Id populated.
        /// The entity is detached from the ChangeTracker after save.
        /// </summary>
        public virtual async Task<T> AddAsync(
            T entity,
            CancellationToken ct = default)
        {
            try
            {
                await DbSet.AddAsync(entity, ct);
                await Context.SaveChangesAsync(ct);
                // Detach immediately — keeps the ChangeTracker clean
                // so the backup/restore model stays reliable.
                Context.Entry(entity).State = EntityState.Detached;
                Logger.LogInfo($"[{typeof(T).Name}] added and saved.");
                return entity;
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] " +
                    $"AddAsync failed.", ex);
                throw;
            }
        }


        /// <summary>
        /// Updates an existing record and immediately commits to the database.
        /// Since all reads use AsNoTracking, entities are always untracked on arrival.
        /// The entity is detached from the ChangeTracker after save.
        /// LastModifiedDate is set automatically by AppDbContext.
        /// </summary>
        public virtual async Task UpdateAsync(
            T entity,
            CancellationToken ct = default)
        {
            try
            {
                // Attach only if not already tracked (safe for both paths).
                var entry = Context.Entry(entity);
                if (entry.State == EntityState.Detached)
                    DbSet.Attach(entity);

                entry.State = EntityState.Modified;
                await Context.SaveChangesAsync(ct);

                // Detach immediately — keeps the ChangeTracker clean
                // so the backup/restore model stays reliable.
                entry.State = EntityState.Detached;

                Logger.LogInfo(
                    $"[{typeof(T).Name}] updated and saved.");
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] " +
                    $"UpdateAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Deletes a record by Id and immediately commits to the database.
        /// Does nothing if the record is not found (idempotent).
        /// FindAsync is used internally here so EF Core can track the
        /// entity just long enough to issue the DELETE statement.
        /// </summary>
        public virtual async Task DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                // FindAsync tracks the entity — required for DbSet.Remove.
                var entity = await DbSet.FindAsync(
                    new object[] { id }, ct);

                if (entity == null)
                {
                    Logger.LogWarning(
                        $"[{typeof(T).Name}] " +
                        $"Delete: not found. Id={id}");
                    return;
                }

                DbSet.Remove(entity);
                await Context.SaveChangesAsync(ct);
                Logger.LogInfo(
                    $"[{typeof(T).Name}] " +
                    $"deleted and saved. Id={id}");
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] " +
                    $"DeleteAsync failed. Id={id}", ex);
                throw;
            }
        }

        /// <summary>
        /// Checks if a record with the given Id exists.
        /// Faster than GetByIdAsync — only checks existence.
        /// </summary>
        public virtual async Task<bool> ExistsAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet.AnyAsync(
                    entity => EF.Property<int>(entity, "Id") == id,
                    ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] " +
                    $"ExistsAsync failed. Id={id}", ex);
                throw;
            }
        }
    }
}
