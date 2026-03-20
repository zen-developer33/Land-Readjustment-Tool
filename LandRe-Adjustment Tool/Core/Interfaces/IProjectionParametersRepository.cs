using Land_Readjustment_Tool.Core.Entities.Spatial;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Contract for ProjectionParameters repository.
    /// </summary>
    public interface IProjectionParametersRepository
        : IRepository<ProjectionParameters>
    {
        /// <summary>
        /// Gets parameters for a specific coordinate system.
        /// </summary>
        Task<ProjectionParameters?> GetByCoordinateSystemIdAsync(
            int coordinateSystemId,
            CancellationToken ct = default);
    }
}
