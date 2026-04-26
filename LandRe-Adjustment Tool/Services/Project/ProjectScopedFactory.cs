using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Repositories.Spatial;
using Land_Readjustment_Tool.Services.Import;
using Land_Readjustment_Tool.Services.LandData;

namespace Land_Readjustment_Tool.Services.Project
{
    /// <summary>
    /// Default implementation that composes project-scoped dependencies
    /// from an active ProjectSession.
    /// </summary>
    public sealed class ProjectScopedFactory : IProjectScopedFactory
    {
        public ProjectInfoService CreateProjectInfoService(ProjectSession session)
        {
            var repo = new ProjectInfoRepository(session);
            return new ProjectInfoService(repo, session.Logger);
        }

        public ProjectSettingsService CreateProjectSettingsService(ProjectSession session)
        {
            var repo = CreateProjectSettingsRepository(session);
            return new ProjectSettingsService(repo, session.Logger);
        }

        public ProjectSettingsRepository CreateProjectSettingsRepository(ProjectSession session)
        {
            return new ProjectSettingsRepository(session);
        }

        public CoordinateSystemRepository CreateCoordinateSystemRepository(ProjectSession session)
        {
            return new CoordinateSystemRepository(session);
        }

        public DatumTransformationRepository CreateDatumTransformationRepository(ProjectSession session)
        {
            return new DatumTransformationRepository(session);
        }

        public ImportPersistenceService CreateImportPersistenceService(ProjectSession session)
        {
            return new ImportPersistenceService(session);
        }

        public LandRecordsService CreateLandRecordsService(ProjectSession session, string projectFilePath)
        {
            return new LandRecordsService(session, projectFilePath);
        }
    }
}
