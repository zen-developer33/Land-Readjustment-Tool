using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    public interface IImportPersistenceService
    {
        Task<ImportPersistenceResult> PersistImportAsync(
            IReadOnlyList<BaselineLandParceRecord> records,
            OwnerDeduplicationService.DeduplicationResult? deduplicationResult,
            bool replaceExistingData,
            string sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            CancellationToken ct = default);

        Task<(int Owners, int Parcels)> GetExistingCountsAsync(CancellationToken ct = default);
    }

    public sealed class ImportPersistenceResult
    {
        public bool ReplacedExistingData { get; init; }
        public int InitialOwners { get; init; }
        public int InitialParcels { get; init; }
        public int DeletedOwners { get; init; }
        public int DeletedParcels { get; init; }
        public int SavedOwners { get; init; }
        public int NewOwnersCreated { get; init; }
        public int ExistingOwnersUpdated { get; init; }
        public int SavedParcels { get; init; }
        public int SkippedDuplicateParcels { get; init; }
        public int ImportSessionId { get; init; }
    }
}
