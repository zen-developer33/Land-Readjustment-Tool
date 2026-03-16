using System;
using System.Collections.Generic;
using System.Text;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        ///<summary>
        ///Gets a single entity bu tis primary key.
        /// Returns null if not found.
        /// Use when you need one specific record.
        ///</summary>
        Task<T?> GetByIDAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Gets all entities from the table.
        /// Use with caution on large tables.
        /// Prefer filtered methods for large datasets.
        /// </summary>
        Task<List<T>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Adds a new entity to the database.
        /// Returns the entity with Id set
        /// after successful save.
        Task<T> AddAsync(T entity, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing entity in the database.
        /// Entity must have a valid Id.
        /// Last ModifiedDate is set automatically.
        /// by AppDbContext.SetDates().
        /// </summary>
        Task UpdateAsync(T entity, CancellationToken ct = default);

        /// <summary>
        /// Deletes an entity from the database by its primary key.
        /// Does nothing if entity not found.
        /// </summary>
        Task DeleteAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Returns true if an entity with the given primary key exists.
        /// Faster than GetByIDAsync when you only need to check existence.
        /// </summary>
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    }
}
