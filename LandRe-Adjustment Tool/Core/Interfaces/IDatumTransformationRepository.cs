using Land_Readjustment_Tool.Core.Entities.Spatial;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// Contract for DatumTransformation repository.
    /// </summary>
    public interface IDatumTransformationRepository
        : IRepository<DatumTransformation>
    {
        /// <summary>Gets all active datum transformations.</summary>
        Task<List<DatumTransformation>> GetAllActiveAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Gets transformations applicable to a specific CRS code.
        /// e.g. passing "MUTM82" returns Nagarkot, Kalianpur, SurveyDept
        /// </summary>
        Task<List<DatumTransformation>>
            GetForCoordinateSystemAsync(
            string crsCode,
            CancellationToken ct = default);

        /// <summary>Gets transformation by code.</summary>
        Task<DatumTransformation?> GetByCodeAsync(
            string code,
            CancellationToken ct = default);
    }
}
