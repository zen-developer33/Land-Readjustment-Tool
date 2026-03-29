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
            Context = session.GetContext();
            Logger = session.Logger;
            DbSet = Context.Set<T>();
        }
        ///<summary>
        ///Gets one record by its ID.
        ///Returns null if not found.
        ///Checks memory cache first before hitting database.   
        ///</summary>
        public virtual async Task<T?> GetByIDAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await DbSet.FindAsync(new object[] { id }, ct);
            }
            catch (Exception ex)
            {
                Logger.LogError($"[{typeof(T).Name}]" + $"GetByIdAsync failed. Id = {id}", ex); //example: [LandOwner] GetByIdAsync failed. Id = 5
                throw; //rethrow to let upper layers handle it (e.g. show error message to user)
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
        /// Stages a new entity for insertion.
        /// The entity Id is assigned by the database only after
        /// SaveChangesAsync is called (Ctrl+S). Do not rely on Id before save.
        /// </summary>
        public virtual async Task<T> AddAsync(
            T entity,
            CancellationToken ct = default)
        {
            try
            {
                await DbSet.AddAsync(entity, ct);
                Logger.LogInfo($"[{typeof(T).Name}] staged for insert.");
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
        /// Updates an existing record.
        /// LastModifiedDate is set automatically by AppDbContext.
        /// </summary>
        public virtual async Task UpdateAsync(
            T entity,
            CancellationToken ct = default)
        {
            try
            {
                var entityType = Context.Model.FindEntityType(typeof(T));
                var primaryKey = entityType?.FindPrimaryKey();

                if (primaryKey is { Properties.Count: > 0 })
                {
                    var trackedEntry = Context.ChangeTracker
                        .Entries<T>()
                        .FirstOrDefault(e =>
                            primaryKey.Properties.All(p =>
                                Equals(
                                    e.Property(p.Name).CurrentValue,
                                    Context.Entry(entity)
                                        .Property(p.Name)
                                        .CurrentValue)));

                    if (trackedEntry != null)
                    {
                        trackedEntry.CurrentValues.SetValues(entity);
                    }
                    else
                    {
                        DbSet.Attach(entity);
                        Context.Entry(entity).State = EntityState.Modified;
                    }
                }
                else
                {
                    Context.Update(entity);
                }

                Logger.LogInfo(
                    $"[{typeof(T).Name}] staged for update.");
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
        /// Deletes a record by Id.
        /// Does nothing if record is not found.
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
