using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services.Import;
using Land_Readjustment_Tool.Services.LandData;
using Land_Readjustment_Tool.Services.Project;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Creates project-scoped services/repositories that depend on an open ProjectSession.
    /// Keeps runtime wiring in one place.
    /// </summary>
    public interface IProjectScopedFactory
    {
        ProjectInfoService CreateProjectInfoService(ProjectSession session);
        ProjectSettingsService CreateProjectSettingsService(ProjectSession session);
        ProjectSettingsRepository CreateProjectSettingsRepository(ProjectSession session);
        CoordinateSystemRepository CreateCoordinateSystemRepository(ProjectSession session);
        DatumTransformationRepository CreateDatumTransformationRepository(ProjectSession session);
        ImportPersistenceService CreateImportPersistenceService(ProjectSession session);
        LandRecordsService CreateLandRecordsService(ProjectSession session, string projectFilePath);
    }
}
