using Land_Readjustment_Tool.Core.Entities.Project;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Contract for ProjectInfo repository.
    /// Services depend on this interface, not the concrete class.
    /// </summary>
    public interface IProjectInfoRepository
        : IRepository<ProjectInfo>
    {
        /// <summary>
        /// Gets the single ProjectInfo record.
        /// Always one record per project file.
        /// Returns null if not found.
        /// </summary>
        Task<ProjectInfo?> GetProjectInfoAsync(
            CancellationToken ct = default);
    }
}