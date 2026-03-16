using Land_Readjustment_Tool.Core.Entities.Project;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Contract for ProjectInfo service.
    /// Forms depend on this interface, not the concrete class.
    /// </summary>
    public interface IProjectInfoService
    {
        /// <summary>
        /// Loads project info from database.
        /// Returns null if not found.
        /// </summary>
        Task<ProjectInfo?> GetAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Validates and saves project info.
        /// Throws InvalidOperationException if data is invalid.
        /// </summary>
        Task SaveAsync(
            ProjectInfo projectInfo,
            CancellationToken ct = default);
    }
}