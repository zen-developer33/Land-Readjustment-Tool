using Land_Readjustment_Tool.Data;

namespace Land_Readjustment_Tool.Services.Raster
{
    /// <summary>
    /// Resolves the active project CRS into a GDAL-ready target SRS definition.
    /// </summary>
    public interface IProjectRasterCrsResolver
    {
        /// <summary>
        /// Loads the project CRS and datum transformation required for raster reprojection.
        /// </summary>
        Task<ProjectRasterCrsContext> ResolveAsync(
            ProjectSession session,
            CancellationToken ct = default);
    }
}
