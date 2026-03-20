using Land_Readjustment_Tool.Core.Entities.Project;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Contract for ProjectSettings repository.
    /// Services depend on this interface, not the concrete class.
    /// </summary>
    public interface IProjectSettingsRepository: IRepository<ProjectSettings>
    {
        /// <summary>
        /// Gets the single ProjectSettings record.
        /// Always one record per project file.
        /// Returns null if not found.
        /// </summary>
        Task<ProjectSettings?> GetProjectSettingsAsync( CancellationToken ct = default);

        /// <summary>
        /// Marks settings as configured.
        /// Sets IsConfigured = true in database.
        /// </summary>
        Task MarkAsConfiguredAsync(CancellationToken ct = default);
    }
}