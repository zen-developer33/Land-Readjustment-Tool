using Land_Readjustment_Tool.Core.Entities.Spatial;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.UI.MapCanvas.Services;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Loads project CRS settings and converts them into a GDAL target SRS definition.
    /// </summary>
    public sealed class ProjectRasterCrsResolver : IProjectRasterCrsResolver
    {
        private readonly IProjectScopedFactory _projectScopedFactory;

        /// <summary>
        /// Creates a resolver using project-scoped repositories.
        /// </summary>
        public ProjectRasterCrsResolver(IProjectScopedFactory projectScopedFactory)
        {
            _projectScopedFactory = projectScopedFactory
                ?? throw new ArgumentNullException(nameof(projectScopedFactory));
        }

        /// <inheritdoc />
        public async Task<ProjectRasterCrsContext> ResolveAsync(
            ProjectSession session,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(session);

            var settingsRepository =
                _projectScopedFactory.CreateProjectSettingsRepository(session);
            var coordinateSystemRepository =
                _projectScopedFactory.CreateCoordinateSystemRepository(session);
            var datumTransformationRepository =
                _projectScopedFactory.CreateDatumTransformationRepository(session);

            var settings = await settingsRepository.GetProjectSettingsAsync(ct);
            if (settings?.CoordinateSystemId == null)
                throw new InvalidOperationException(
                    "Please configure the project coordinate system before importing raster data.");

            CoordinateSystem? coordinateSystem =
                await coordinateSystemRepository.GetWithParametersAsync(
                    settings.CoordinateSystemId.Value,
                    ct);

            if (coordinateSystem == null)
                throw new InvalidOperationException(
                    "The configured project coordinate system could not be loaded.");

            DatumTransformation? datumTransformation = null;
            if (settings.DatumTransformationId.HasValue)
            {
                datumTransformation =
                    await datumTransformationRepository.GetByIDAsync(
                        settings.DatumTransformationId.Value,
                        ct);
            }
            else if (RequiresDatumTransformation(coordinateSystem))
            {
                List<DatumTransformation> availableTransformations =
                    await datumTransformationRepository
                        .GetForCoordinateSystemAsync(coordinateSystem.Code, ct);

                datumTransformation =
                    availableTransformations.FirstOrDefault(IsOfficialSurveyDepartmentTransform) ??
                    availableTransformations.FirstOrDefault(d => d.IsSystemDefault) ??
                    availableTransformations.FirstOrDefault();
            }

            string targetSrsDefinition =
                ProjectCrsWktBuilder.BuildTargetSrsDefinition(
                    coordinateSystem,
                    datumTransformation);

            return new ProjectRasterCrsContext(
                coordinateSystem,
                datumTransformation,
                targetSrsDefinition);
        }

        private static bool RequiresDatumTransformation(
            CoordinateSystem coordinateSystem)
        {
            if (coordinateSystem.EpsgCode.HasValue)
            {
                return false;
            }

            string code = coordinateSystem.Code ?? string.Empty;
            if (code.StartsWith("MUTM", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string? projectionType = coordinateSystem.ProjectionType;
            string? ellipsoid = coordinateSystem.ProjectionParameters?.Ellipsoid;
            return projectionType?.Contains(
                       "Transverse",
                       StringComparison.OrdinalIgnoreCase) == true &&
                   ellipsoid?.Contains(
                       "Everest",
                       StringComparison.OrdinalIgnoreCase) == true;
        }

        private static bool IsOfficialSurveyDepartmentTransform(
            DatumTransformation datumTransformation)
        {
            return ContainsIgnoreCase(datumTransformation.Code, "SURVEY") ||
                   ContainsIgnoreCase(datumTransformation.Name, "Survey Department") ||
                   ContainsIgnoreCase(datumTransformation.Source, "Survey Department");
        }

        private static bool ContainsIgnoreCase(string? value, string text) =>
            value?.Contains(text, StringComparison.OrdinalIgnoreCase) == true;
    }
}
