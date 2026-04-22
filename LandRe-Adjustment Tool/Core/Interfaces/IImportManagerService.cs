using Land_Readjustment_Tool.Core.Entities.Import;
using Land_Readjustment_Tool.Models;
using System.Data;

namespace Land_Readjustment_Tool.Core.Interfaces
{
    /// <summary>
    /// EF Core based import manager contract.
    /// Stages imported rows into import tables before downstream processing.
    /// </summary>
    public interface IImportManagerService
    {
        Task<ImportSession> StageImportAsync(
            DataTable sourceData,
            IReadOnlyDictionary<string, string> fieldMappings,
            string sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            bool replacePreviousSession = true,
            CancellationToken ct = default);

        Task<ImportSession?> GetLatestSessionAsync(
            CancellationToken ct = default);

        Task<List<ImportedRawRecord>> GetSessionRowsAsync(
            int importSessionId,
            bool includeInvalid = true,
            CancellationToken ct = default);

        Task<ImportSession> StageNormalizedRecordsAsync(
            IReadOnlyList<BaselineLandParceRecord> records,
            string sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            CancellationToken ct = default);
    }
}
