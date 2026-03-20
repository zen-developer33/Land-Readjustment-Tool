using Land_Readjustment_Tool.Core.Entities.Spatial;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Contract for CoordinateSystem repository.
    /// </summary>
    public interface ICoordinateSystemRepository
        : IRepository<CoordinateSystem>
    {
        /// <summary>Gets all active coordinate systems.</summary>
        Task<List<CoordinateSystem>> GetAllActiveAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Gets CRS with its projection parameters included.
        /// </summary>
        Task<CoordinateSystem?> GetWithParametersAsync(
            int id,
            CancellationToken ct = default);

        /// <summary>Gets CRS by code. e.g. "MUTM82"</summary>
        Task<CoordinateSystem?> GetByCodeAsync(
            string code,
            CancellationToken ct = default);
    }
}
