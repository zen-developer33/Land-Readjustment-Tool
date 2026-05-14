using Land_Readjustment_Tool.Core.Entities.Import;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Infrastructure.Logging;
using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Globalization;
using System.Data;
using System.Text.Json;
using ImportValidationError = Land_Readjustment_Tool.Core.Entities.Import.ValidationError;

namespace Land_Readjustment_Tool.Services.Import
{
    /// <summary>
    /// New EF Core first implementation for Import Manager staging.
    /// This service only stages and validates imported rows.
    /// </summary>
    public sealed class ImportManagerService : IImportManagerService
    {
        private readonly AppDbContext _context;
        private readonly IAppLogger _logger;

        public ImportManagerService(ProjectSession session)
        {
            _context = session.GetDbContext();
            _logger = session.Logger;
        }

        public async Task<ImportSession> StageImportAsync(
            DataTable sourceData,
            IReadOnlyDictionary<string, string> fieldMappings,
            string sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            bool replacePreviousSession = true,
            CancellationToken ct = default)
        {
            if (sourceData == null)
                throw new ArgumentNullException(nameof(sourceData));

            if (string.IsNullOrWhiteSpace(sourceFileName))
                throw new ArgumentException("Source file name is required.", nameof(sourceFileName));

            if (fieldMappings == null || fieldMappings.Count == 0)
                throw new ArgumentException("At least one field mapping is required.", nameof(fieldMappings));

            var ownsTransaction = _context.Database.CurrentTransaction == null;
            IDbContextTransaction? tx = null;
            if (ownsTransaction)
            {
                tx = await _context.Database.BeginTransactionAsync(ct);
            }

            try
            {
                var session = new ImportSession
                {
                    SourceFileName = sourceFileName,
                    SourceFilePath = sourceFilePath,
                    ImportDate = DateTime.UtcNow,
                    TotalRowsInFile = sourceData.Rows.Count,
                    TotalRowsImported = 0,
                    TotalRowsInvalid = 0,
                    Notes = BuildSessionNotes(sheetName)
                };

                await _context.ImportSessions.AddAsync(session, ct);
                await _context.SaveChangesAsync(ct);

                var records = new List<ImportedRawRecord>(sourceData.Rows.Count);
                var errors = new List<ImportValidationError>();

                for (int i = 0; i < sourceData.Rows.Count; i++)
                {
                    var row = sourceData.Rows[i];
                    var rowNumber = i + 1;

                    var record = MapRowToImportedRecord(
                        row,
                        rowNumber,
                        session.Id,
                        sourceData.Columns,
                        fieldMappings);

                    var rowErrors = ValidateStagedRecord(record);

                    if (rowErrors.Count > 0)
                    {
                        record.IsValid = false;
                        foreach (var rowError in rowErrors)
                        {
                            errors.Add(new ImportValidationError
                            {
                                ImportSessionId = session.Id,
                                ImportedRawRecordId = 0, // populated after record insert
                                FieldName = rowError.FieldName,
                                ErrorType = rowError.ErrorType,
                                ErrorMessage = rowError.Message,
                                IsResolved = false
                            });
                        }
                    }

                    records.Add(record);
                }

                await _context.ImportedRawRecords.AddRangeAsync(records, ct);
                await _context.SaveChangesAsync(ct);

                if (errors.Count > 0)
                {
                    var recordByRow = records.ToDictionary(r => r.RowNumber, r => r.Id);
                    foreach (var error in errors)
                    {
                        var rowNo = ExtractRowNumberFromMessage(error.ErrorMessage);
                        if (rowNo.HasValue && recordByRow.TryGetValue(rowNo.Value, out var recordId))
                        {
                            error.ImportedRawRecordId = recordId;
                        }
                    }

                    errors.RemoveAll(e => e.ImportedRawRecordId <= 0);

                    if (errors.Count > 0)
                    {
                        await _context.ValidationErrors.AddRangeAsync(errors, ct);
                        await _context.SaveChangesAsync(ct);
                    }
                }

                session.TotalRowsImported = records.Count(r => r.IsValid);
                session.TotalRowsInvalid = records.Count - session.TotalRowsImported;
                _context.ImportSessions.Update(session);
                await _context.SaveChangesAsync(ct);

                if (replacePreviousSession)
                {
                    var previousSessions = await _context.ImportSessions
                        .Where(s => s.Id != session.Id && !s.IsReplaced)
                        .ToListAsync(ct);

                    foreach (var previous in previousSessions)
                    {
                        previous.IsReplaced = true;
                        previous.ReplacedBySessionID = session.Id;
                    }

                    if (previousSessions.Count > 0)
                        await _context.SaveChangesAsync(ct);
                }

                if (tx != null)
                {
                    await tx.CommitAsync(ct);
                }

                _logger.LogInfo(
                    $"Import staged. SessionId={session.Id}, Total={session.TotalRowsInFile}, " +
                    $"Valid={session.TotalRowsImported}, Invalid={session.TotalRowsInvalid}");

                return session;
            }
            catch (Exception ex)
            {
                if (tx != null)
                {
                    await tx.RollbackAsync(ct);
                }
                _logger.LogError("StageImportAsync failed.", ex);
                throw;
            }
        }

        public Task<ImportSession?> GetLatestSessionAsync(CancellationToken ct = default)
        {
            return _context.ImportSessions
                .OrderByDescending(s => s.ImportDate)
                .FirstOrDefaultAsync(ct);
        }

        public Task<List<ImportedRawRecord>> GetSessionRowsAsync(
            int importSessionId,
            bool includeInvalid = true,
            CancellationToken ct = default)
        {
            var query = _context.ImportedRawRecords
                .AsNoTracking()
                .Where(r => r.ImportSessionId == importSessionId);

            if (!includeInvalid)
                query = query.Where(r => r.IsValid);

            return query
                .OrderBy(r => r.RowNumber)
                .ToListAsync(ct);
        }

        public async Task<ImportSession> StageNormalizedRecordsAsync(
            IReadOnlyList<BaselineLandParcelRecord> records,
            string sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            CancellationToken ct = default)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            if (string.IsNullOrWhiteSpace(sourceFileName))
                throw new ArgumentException("Source file name is required.", nameof(sourceFileName));

            var ownsTransaction = _context.Database.CurrentTransaction == null;
            IDbContextTransaction? tx = null;
            if (ownsTransaction)
            {
                tx = await _context.Database.BeginTransactionAsync(ct);
            }
            try
            {
                var session = new ImportSession
                {
                    SourceFileName = sourceFileName,
                    SourceFilePath = sourceFilePath,
                    ImportDate = DateTime.UtcNow,
                    TotalRowsInFile = records.Count,
                    Notes = BuildSessionNotes(sheetName)
                };

                await _context.ImportSessions.AddAsync(session, ct);
                await _context.SaveChangesAsync(ct);

                var stagedRecords = new List<ImportedRawRecord>(records.Count);
                var errors = new List<ImportValidationError>();

                for (int i = 0; i < records.Count; i++)
                {
                    var rowNumber = i + 1;
                    var source = records[i];
                    var staged = MapNormalizedRecord(source, rowNumber, session.Id);
                    var rowErrors = DataTransformationService.ValidateSingleRecord(source, rowNumber);
                    if (rowErrors.Count > 0)
                    {
                        staged.IsValid = false;
                        foreach (var err in rowErrors)
                        {
                            errors.Add(new ImportValidationError
                            {
                                ImportSessionId = session.Id,
                                ImportedRawRecordId = 0,
                                FieldName = InferFieldName(err),
                                ErrorType = "BusinessRule",
                                ErrorMessage = $"Row {rowNumber}: {err}",
                                IsResolved = false
                            });
                        }
                    }

                    stagedRecords.Add(staged);
                }

                await _context.ImportedRawRecords.AddRangeAsync(stagedRecords, ct);
                await _context.SaveChangesAsync(ct);

                if (errors.Count > 0)
                {
                    var recordIdByRow = stagedRecords.ToDictionary(r => r.RowNumber, r => r.Id);
                    foreach (var err in errors)
                    {
                        var rowNo = ExtractRowNumberFromMessage(err.ErrorMessage);
                        if (rowNo.HasValue && recordIdByRow.TryGetValue(rowNo.Value, out var recordId))
                        {
                            err.ImportedRawRecordId = recordId;
                        }
                    }

                    errors.RemoveAll(e => e.ImportedRawRecordId <= 0);

                    if (errors.Count > 0)
                    {
                        await _context.ValidationErrors.AddRangeAsync(errors, ct);
                        await _context.SaveChangesAsync(ct);
                    }
                }

                session.TotalRowsImported = stagedRecords.Count(r => r.IsValid);
                session.TotalRowsInvalid = stagedRecords.Count - session.TotalRowsImported;
                _context.ImportSessions.Update(session);
                await _context.SaveChangesAsync(ct);

                if (tx != null)
                {
                    await tx.CommitAsync(ct);
                }

                _logger.LogInfo(
                    $"Normalized import staged. SessionId={session.Id}, Total={session.TotalRowsInFile}, " +
                    $"Valid={session.TotalRowsImported}, Invalid={session.TotalRowsInvalid}");

                return session;
            }
            catch (Exception ex)
            {
                if (tx != null)
                {
                    await tx.RollbackAsync(ct);
                }
                _logger.LogError("StageNormalizedRecordsAsync failed.", ex);
                throw;
            }
        }

        private static string BuildSessionNotes(string? sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
                return "Imported via EF Core import manager.";

            return $"Imported via EF Core import manager. Sheet: {sheetName}";
        }

        private static ImportedRawRecord MapRowToImportedRecord(
            DataRow row,
            int rowNumber,
            int sessionId,
            DataColumnCollection columns,
            IReadOnlyDictionary<string, string> fieldMappings)
        {
            string? GetValue(string targetField)
            {
                if (!fieldMappings.TryGetValue(targetField, out var sourceColumn))
                    return null;

                if (string.IsNullOrWhiteSpace(sourceColumn))
                    return null;

                if (!columns.Contains(sourceColumn))
                    return null;

                var value = row[sourceColumn];
                if (value == null || value == DBNull.Value)
                    return null;

                var text = value.ToString()?.Trim();
                return string.IsNullOrWhiteSpace(text) ? null : text;
            }

            var areaSqmText = GetValue("AreaInSqm") ?? GetValue("AreaSqm");
            var tenantText = GetValue("Tenant");
            var tenantName = GetValue("TenantName");

            var record = new ImportedRawRecord
            {
                ImportSessionId = sessionId,
                RowNumber = rowNumber,
                MapSheetNo = GetValue("MapSheetNo"),
                ParcelNo = GetValue("ParcelNo"),
                Province = GetValue("Province"),
                District = GetValue("District"),
                Municipality = GetValue("MunicipalityVillage") ?? GetValue("Municipality"),
                WardNo = GetValue("WardNo"),
                MothNo = GetValue("MothNo"),
                PaanaNo = GetValue("PaanaNo"),
                LandUse = GetValue("LandUse"),
                AreaSqm = TryParseNullableDouble(areaSqmText),
                FieldMeasuredAreaSqm = TryParseNullableDouble(GetValue("FieldMeasuredAreaSqm")),
                AreaRAPD = GetValue("AreaInRAPD"),
                AreaBKD = GetValue("AreaInBKD"),
                OwnerName = GetValue("LandOwnersName") ?? GetValue("OwnerName"),
                FatherSpouseName = GetValue("FatherSpouse"),
                Gender = GetValue("Gender"),
                CitizenshipNumber = GetValue("CitizenshipNumber"),
                CitizenshipDistrict = GetValue("CitizenshipIssuedDistrict"),
                CitizenshipDate = GetValue("CitizenshipIssuedDate") ?? GetValue("CitizenshipIssuedDate"),
                PermanentAddress = GetValue("PermanentAddress"),
                TemporaryAddress = GetValue("TemporaryAddress") ?? GetValue("TemporaryAddress"),
                ContactNumber = GetValue("ContactNumber"),
                Email = GetValue("EmailID") ?? GetValue("Email"),
                IsTenant = TryParseNullableBool(tenantText)
                    ?? (!string.IsNullOrWhiteSpace(tenantName) || !string.IsNullOrWhiteSpace(tenantText) ? true : null),
                TenantName = tenantName,
                Remarks = GetValue("Remarks"),
                IsValid = true,
                RawRowData = SerializeRow(row, columns)
            };

            return record;
        }

        private static List<RowValidationIssue> ValidateStagedRecord(ImportedRawRecord record)
        {
            var issues = new List<RowValidationIssue>();

            if (string.IsNullOrWhiteSpace(record.ParcelNo))
            {
                issues.Add(new RowValidationIssue(
                    "ParcelNo",
                    "Missing",
                    $"Row {record.RowNumber}: ParcelNo is required."));
            }

            if (string.IsNullOrWhiteSpace(record.MapSheetNo))
            {
                issues.Add(new RowValidationIssue(
                    "MapSheetNo",
                    "Missing",
                    $"Row {record.RowNumber}: MapSheetNo is required."));
            }

            if (record.AreaSqm.HasValue && record.AreaSqm <= 0)
            {
                issues.Add(new RowValidationIssue(
                    "AreaSqm",
                    "InvalidRange",
                    $"Row {record.RowNumber}: AreaInSqm must be greater than 0."));
            }

            if (!string.IsNullOrWhiteSpace(record.Email) && !record.Email.Contains('@'))
            {
                issues.Add(new RowValidationIssue(
                    "Email",
                    "InvalidFormat",
                    $"Row {record.RowNumber}: Email appears invalid."));
            }

            return issues;
        }

        private static double? TryParseNullableDouble(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;

            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return value;

            return null;
        }

        private static bool? TryParseNullableBool(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var normalized = text.Trim().ToLowerInvariant();
            if (normalized is "yes" or "y" or "true" or "1")
                return true;
            if (normalized is "no" or "n" or "false" or "0")
                return false;

            return null;
        }

        private static string SerializeRow(DataRow row, DataColumnCollection columns)
        {
            var data = new Dictionary<string, string?>(columns.Count, StringComparer.OrdinalIgnoreCase);
            foreach (DataColumn column in columns)
            {
                var value = row[column];
                data[column.ColumnName] = value == DBNull.Value ? null : value?.ToString();
            }

            return JsonSerializer.Serialize(data);
        }

        private static ImportedRawRecord MapNormalizedRecord(
            BaselineLandParcelRecord source,
            int rowNumber,
            int sessionId)
        {
            return new ImportedRawRecord
            {
                ImportSessionId = sessionId,
                RowNumber = rowNumber,
                MapSheetNo = source.MapSheetNo,
                ParcelNo = source.ParcelNo,
                Province = source.Province,
                District = source.District,
                Municipality = source.MunicipalityVillage,
                WardNo = source.WardNo,
                MothNo = source.MothNo,
                PaanaNo = source.PaanaNo,
                LandUse = source.LandUse,
                AreaSqm = source.AreaInSqm,
                FieldMeasuredAreaSqm = source.FieldMeasuredAreaSqm,
                AreaRAPD = source.AreaInRAPD,
                AreaBKD = source.AreaInBKD,
                OwnerName = source.LandOwnersName,
                FatherSpouseName = source.FatherSpouse,
                Gender = source.Gender,
                CitizenshipNumber = source.CitizenshipNumber,
                CitizenshipDistrict = source.CitizenshipIssuedDistrict,
                CitizenshipDate = source.CitizenshipIssuedDate,
                PermanentAddress = source.PermanentAddress,
                TemporaryAddress = source.TemporaryAddress,
                ContactNumber = source.ContactNumber,
                Email = source.EmailID,
                IsTenant = TryParseNullableBool(source.Tenant)
                    ?? (!string.IsNullOrWhiteSpace(source.TenantName) || !string.IsNullOrWhiteSpace(source.Tenant) ? true : null),
                TenantName = source.TenantName,
                Remarks = source.Remarks,
                IsValid = true,
                RawRowData = JsonSerializer.Serialize(source)
            };
        }

        private static string InferFieldName(string message)
        {
            if (message.StartsWith("ParcelNo", StringComparison.OrdinalIgnoreCase))
                return "ParcelNo";
            if (message.StartsWith("MapSheetNo", StringComparison.OrdinalIgnoreCase))
                return "MapSheetNo";
            if (message.StartsWith("AreaInSqm", StringComparison.OrdinalIgnoreCase))
                return "AreaSqm";
            return "General";
        }

        private static int? ExtractRowNumberFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            const string prefix = "Row ";
            if (!message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var colonIndex = message.IndexOf(':');
            if (colonIndex <= prefix.Length)
                return null;

            var numberSlice = message.Substring(prefix.Length, colonIndex - prefix.Length).Trim();
            return int.TryParse(numberSlice, out var rowNo) ? rowNo : null;
        }

        private sealed record RowValidationIssue(
            string FieldName,
            string ErrorType,
            string Message);
    }
}
