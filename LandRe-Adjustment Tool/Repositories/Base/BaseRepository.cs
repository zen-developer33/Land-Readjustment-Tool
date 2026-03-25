using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace Land_Readjustment_Tool.Repositories.Base
{
    /// <summary>
    /// Base class for all repositories.
    /// Contains common database operations shared by every repository.
    ///
    /// IMPORTANT — TRANSACTION PATTERN:
    /// AddAsync / UpdateAsync / DeleteAsync do NOT call
    /// SaveChangesAsync. They only STAGE changes in the
    /// EF Core ChangeTracker (in memory).
    ///
    /// SaveChangesAsync is called ONLY from:
    ///   frmMain.SaveCurrentProjectAsync() ← Ctrl+S
    ///
    /// This means:
    ///   → User edits form → entity staged in memory
    ///   → .lpp file is NOT touched
    ///   → User presses Ctrl+S → SaveChangesAsync
    ///      commits ALL staged changes at once
    ///   → User closes without saving
    ///      → ChangeTracker.Clear() discards all staged
    ///      → .lpp file is NEVER touched ✅
    ///
    /// This is the EF Core equivalent of a transaction:
    ///   Stage = Begin Transaction + queue operations
    ///   SaveChangesAsync = Commit
    ///   ChangeTracker.Clear = Rollback
    /// </summary>
    public abstract class BaseRepository<T>
        : IRepository<T> where T : class
    {
        // ── PROTECTED FIELDS ─────────────────────────

        /// <summary>EF Core database context.</summary>
        protected readonly AppDbContext Context;

        /// <summary>Logger for errors and info.</summary>
        protected readonly IAppLogger Logger;

        /// <summary>
        /// Shortcut to the DbSet for entity T.
        /// Example: DbSet of LandOwner = tblLandOwners.
        /// </summary>
        protected readonly DbSet<T> DbSet;

        /// <summary>
        /// Gets context and logger from session.
        /// Called by child repositories via : base(session).
        /// </summary>
        protected BaseRepository(ProjectSession session)
        {
            Context = session.GetContext();
            Logger = session.Logger;
            DbSet = Context.Set<T>();
        }

        // ── READ OPERATIONS ──────────────────────────

        /// <summary>
        /// Gets one record by its ID.
        /// Checks EF Core memory cache first,
        /// then queries database if not found.
        /// </summary>
        public virtual async Task<T?> GetByIDAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet.FindAsync(
                    new object[] { id }, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] " +
                    $"GetByIdAsync failed. Id={id}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets all records from the table.
        /// AsNoTracking = read-only, faster query.
        /// Does not stage anything for change tracking.
        /// </summary>
        public virtual async Task<List<T>> GetAllAsync(
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet
                    .AsNoTracking()
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    $"[{typeof(T).Name}] " +
                    $"GetAllAsync failed.", ex);
                throw;
            }
        }

        // ── WRITE OPERATIONS (STAGING ONLY) ──────────
        //
        // These methods stage changes in EF Core memory.
        // They do NOT write to the .lpp file.
        // SaveChangesAsync is called only by frmMain.
        //
        // ─────────────────────────────────────────────

        /// <summary>
        /// Stages a new entity for insertion.
        /// Does NOT write to database.
        /// Database write happens on Ctrl+S (SaveChangesAsync).
        /// </summary>
        public virtual async Task<T> AddAsync(
            T entity,
            CancellationToken ct = default)
        {
            try
            {
                // Stages entity in EF Core ChangeTracker
                // EntityState = Added
                await DbSet.AddAsync(entity, ct);

                Logger.LogInfo(
                    $"[{typeof(T).Name}] " +
                    $"staged for add.");

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
        /// Stages an entity for update.
        /// Does NOT write to database.
        /// Database write happens on Ctrl+S (SaveChangesAsync).
        /// </summary>
        public virtual async Task UpdateAsync(
            T entity,
            CancellationToken ct = default)
        {
            try
            {
                var entityType = Context.Model
                    .FindEntityType(typeof(T));
                var primaryKey = entityType
                    ?.FindPrimaryKey();

                if (primaryKey is
                    { Properties.Count: > 0 })
                {
                    // Check if already tracked
                    var trackedEntry = Context
                        .ChangeTracker.Entries<T>()
                        .FirstOrDefault(e =>
                            primaryKey.Properties.All(
                                p => Equals(
                                    e.Property(p.Name)
                                        .CurrentValue,
                                    Context.Entry(entity)
                                        .Property(p.Name)
                                        .CurrentValue)));

                    if (trackedEntry != null)
                    {
                        // Already tracked — update values
                        trackedEntry.CurrentValues
                            .SetValues(entity);
                    }
                    else
                    {
                        // Not tracked — attach and mark modified
                        DbSet.Attach(entity);
                        Context.Entry(entity).State =
                            EntityState.Modified;
                    }
                }
                else
                {
                    Context.Update(entity);
                }

                // Changes staged in ChangeTracker
                // EntityState = Modified
                Logger.LogInfo(
                    $"[{typeof(T).Name}] " +
                    $"staged for update.");
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
        /// Stages an entity for deletion.
        /// Does NOT write to database.
        /// Database write happens on Ctrl+S (SaveChangesAsync).
        /// </summary>
        public virtual async Task DeleteAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                var entity = await DbSet.FindAsync(
                    new object[] { id }, ct);

                if (entity == null)
                {
                    Logger.LogWarning(
                        $"[{typeof(T).Name}] " +
                        $"Delete: not found. Id={id}");
                    return;
                }

                // Stages entity for removal
                // EntityState = Deleted
                DbSet.Remove(entity);

                Logger.LogInfo(
                    $"[{typeof(T).Name}] " +
                    $"staged for delete. Id={id}");
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
        /// Checks if a record with the given ID exists.
        /// Faster than GetByIDAsync — only checks existence.
        /// </summary>
        public virtual async Task<bool> ExistsAsync(
            int id,
            CancellationToken ct = default)
        {
            try
            {
                return await DbSet.AnyAsync(
                    e => EF.Property<int>(e, "Id") == id,
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