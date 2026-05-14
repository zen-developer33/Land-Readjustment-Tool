using Land_Readjustment_Tool.Infrastructure.Constants;
using Land_Readjustment_Tool.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;


namespace Land_Readjustment_Tool.Services
{
public class DataTransformationService
{

        // ==================== GRID → DATATABLE ====================

        public static DataTable ConvertGridToDataTable(DataGridView grid)
        {
            if (grid == null)
                throw new ArgumentNullException(nameof(grid));

            var table = new DataTable();

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (!col.Visible) continue;
                table.Columns.Add(col.Name, typeof(string));
            }

            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;

                DataRow dr = table.NewRow();

                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (!col.Visible) continue;
                    dr[col.Name] = row.Cells[col.Name].Value ?? DBNull.Value;
                }

                table.Rows.Add(dr);
            }

            return table;
        }

        // ==================== EXCEL IMPORT ====================

        public static TransformationResult TransformDataToEntities(
            DataTable sourceData,
            Dictionary<string, string> fieldMappings)
        {
            var result = new TransformationResult();

            for (int i = 0; i < sourceData.Rows.Count; i++)
            {
                DataRow row = sourceData.Rows[i];
                int rowNumber = i + 1;

                // Skip empty rows or rows missing all required fields
                if (ShouldSkipRow(row, fieldMappings))
                {
                    result.SkippedRows++;
                    continue;
                }

                var record = new BaselineLandParcelRecord();
                var rowErrors = new List<string>();

                try
                {
                    foreach (var mapping in fieldMappings)
                    {
                        if (!sourceData.Columns.Contains(mapping.Value))
                            continue;

                        SetPropertyValue(
                            record,
                            mapping.Key,
                            row[mapping.Value],
                            rowErrors
                        );
                    }

                    ApplyOwnerSemanticNormalization(record);
                    ValidateBusinessRules(record, rowNumber, rowErrors);

                    result.AllOriginalRecords.Add(record);

                    if (rowErrors.Count > 0)
                    {
                        result.InvalidRecords.Add(record);
                        result.ValidationErrors.Add(new ValidationError
                        {
                            RowNumber = rowNumber,
                            RecordData = record,
                            Errors = rowErrors
                        });
                    }
                    else
                    {
                        result.ValidRecords.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    result.InvalidRecords.Add(record);
                    result.ValidationErrors.Add(new ValidationError
                    {
                        RowNumber = rowNumber,
                        RecordData = record,
                        Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                    });
                }
            }

            ApplyDuplicateParcelValidation(result);
            return result;
        }

        // ==================== GRID VALIDATION ====================

        public static TransformationResult ValidateFromDataTable(DataTable sourceData)
        {
            var result = new TransformationResult();

            for (int i = 0; i < sourceData.Rows.Count; i++)
            {
                DataRow row = sourceData.Rows[i];
                int rowNumber = i + 1;

                var record = new BaselineLandParcelRecord();
                var rowErrors = new List<string>();

                try
                {
                    foreach (DataColumn column in sourceData.Columns)
                    {
                        SetPropertyValue(
                            record,
                            column.ColumnName,
                            row[column],
                            rowErrors
                        );
                    }

                    ApplyOwnerSemanticNormalization(record);
                    ValidateBusinessRules(record, rowNumber, rowErrors);

                    result.AllOriginalRecords.Add(record);

                    if (rowErrors.Count > 0)
                    {
                        result.InvalidRecords.Add(record);
                        result.ValidationErrors.Add(new ValidationError
                        {
                            RowNumber = rowNumber,
                            RecordData = record,
                            Errors = rowErrors
                        });
                    }
                    else
                    {
                        result.ValidRecords.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    result.InvalidRecords.Add(record);
                    result.ValidationErrors.Add(new ValidationError
                    {
                        RowNumber = rowNumber,
                        RecordData = record,
                        Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                    });
                }
            }

            ApplyDuplicateParcelValidation(result);
            return result;
        }

        public static TransformationResult ValidateRecords(IReadOnlyList<BaselineLandParcelRecord> records)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            var result = new TransformationResult();

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                if (record.IsJointCoOwnerRow)
                    continue;

                int rowNumber = result.AllOriginalRecords.Count + 1;
                var rowErrors = new List<string>();

                try
                {
                    ApplyOwnerSemanticNormalization(record);
                    ValidateBusinessRules(record, rowNumber, rowErrors);

                    result.AllOriginalRecords.Add(record);

                    if (rowErrors.Count > 0)
                    {
                        result.InvalidRecords.Add(record);
                        result.ValidationErrors.Add(new ValidationError
                        {
                            RowNumber = rowNumber,
                            RecordData = record,
                            Errors = rowErrors
                        });
                    }
                    else
                    {
                        result.ValidRecords.Add(record);
                    }
                }
                catch (Exception ex)
                {
                    result.InvalidRecords.Add(record);
                    result.ValidationErrors.Add(new ValidationError
                    {
                        RowNumber = rowNumber,
                        RecordData = record,
                        Errors = new List<string> { $"Unexpected error: {ex.Message}" }
                    });
                }
            }

            ApplyDuplicateParcelValidation(result);
            return result;
        }

        // ==================== ROW SKIP LOGIC ====================

        /// <summary>
        /// Determines if a row should be skipped during import.
        /// Skips rows where:
        /// 1. All required fields (ParcelNo, MapSheetNo, AreaInSqm) are missing/empty
        /// 2. The entire row is empty
        /// </summary>
        private static bool ShouldSkipRow(DataRow row, Dictionary<string, string> fieldMappings)
        {
            // Check if entire row is empty
            bool entireRowEmpty = row.ItemArray.All(cell => 
                cell == null || cell == DBNull.Value || string.IsNullOrWhiteSpace(cell.ToString()));
            
            if (entireRowEmpty)
                return true;

            // Check if all required fields are missing
            string? parcelNo = GetMappedValue(row, fieldMappings, nameof(BaselineLandParcelRecord.ParcelNo));
            string? mapSheetNo = GetMappedValue(row, fieldMappings, nameof(BaselineLandParcelRecord.MapSheetNo));
            string? areaInSqm = GetMappedValue(row, fieldMappings, nameof(BaselineLandParcelRecord.AreaInSqm));

            bool allRequiredFieldsMissing = 
                string.IsNullOrWhiteSpace(parcelNo) &&
                string.IsNullOrWhiteSpace(mapSheetNo) &&
                string.IsNullOrWhiteSpace(areaInSqm);

            return allRequiredFieldsMissing;
        }

        /// <summary>
        /// Gets the value from a DataRow using the field mapping
        /// </summary>
        private static string? GetMappedValue(DataRow row, Dictionary<string, string> fieldMappings, string targetField)
        {
            if (!fieldMappings.TryGetValue(targetField, out string? sourceColumn))
                return null;

            if (!row.Table.Columns.Contains(sourceColumn))
                return null;

            var value = row[sourceColumn];
            if (value == null || value == DBNull.Value)
                return null;

            return value.ToString()?.Trim();
        }

        // ==================== DUPLICATE PARCEL VALIDATION ====================

        private static void ApplyDuplicateParcelValidation(TransformationResult result)
        {
            var groups = result.AllOriginalRecords
                .Select((record, index) => new { record, rowIndex = index })
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.record.ParcelNo) &&
                    !string.IsNullOrWhiteSpace(x.record.MapSheetNo))
                .GroupBy(x => new
                {
                    MapSheet = (x.record.MapSheetNo ?? string.Empty).Trim().ToUpperInvariant(),
                    Parcel = (x.record.ParcelNo ?? string.Empty).Trim().ToUpperInvariant()
                })
                .Where(g => g.Count() > 1);

            foreach (var group in groups)
            {
                // Normalised key shared by all rows in this duplicate group.
                var dupKey = $"{group.Key.MapSheet}::{group.Key.Parcel}";
                var dupMessage = $"Duplicate ParcelNo '{group.First().record.ParcelNo}' found in MapSheet '{group.First().record.MapSheetNo}' — possible Joint Ownership";

                foreach (var item in group)
                {
                    int rowNumber = item.rowIndex + 1;

                    var existingError = result.ValidationErrors
                        .FirstOrDefault(e => e.RowNumber == rowNumber);

                    if (existingError == null)
                    {
                        var error = new ValidationError
                        {
                            RowNumber = rowNumber,
                            RecordData = item.record,
                            Errors = new List<string> { dupMessage },
                            IsDuplicateParcel = true,
                            DuplicateParcelKey = dupKey
                        };

                        result.ValidationErrors.Add(error);
                        result.InvalidRecords.Add(item.record);
                        result.ValidRecords.Remove(item.record);
                    }
                    else
                    {
                        existingError.Errors.Add(dupMessage);
                        // If all errors on this row are now duplicate-parcel errors, mark it.
                        existingError.IsDuplicateParcel = existingError.Errors.All(
                            e => e.Contains("Duplicate ParcelNo"));
                        if (existingError.IsDuplicateParcel)
                            existingError.DuplicateParcelKey = dupKey;
                    }
                }
            }
        }

        // ==================== PROPERTY MAPPING ====================

        private static void SetPropertyValue(
            BaselineLandParcelRecord record,
            string propertyName,
            object value,
            List<string> errors)
        {
            var property = typeof(BaselineLandParcelRecord).GetProperty(propertyName);
            if (property == null) return;
            if (Attribute.IsDefined(property, typeof(NotMappedAttribute))) return;

            try
            {
                if (value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()))
                {
                    if (property.PropertyType == typeof(string))
                        property.SetValue(record, string.Empty);
                    else if (property.PropertyType == typeof(double?))
                        property.SetValue(record, null);
                    return;
                }

                object convertedValue = ConvertValue(value, property.PropertyType, propertyName, errors);
                property.SetValue(record, convertedValue);
            }
            catch (Exception ex)
            {
                errors.Add($"{propertyName}: {ex.Message}");
            }
        }

        private static object ConvertValue(
            object value,
            Type targetType,
            string fieldName,
            List<string> errors)
        {
            string stringValue = value.ToString()?.Trim() ?? string.Empty;

            if (targetType == typeof(string))
                return CleanString(stringValue);

            if (targetType == typeof(double?) || targetType == typeof(double))
                return ParseDouble(stringValue, fieldName, errors);

            return stringValue;
        }

        // ==================== CLEANING ====================

        private static string CleanString(string value)
        {
            value = Regex.Replace(value.Trim(), @"\s+", " ");
            value = Regex.Replace(value, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
            return value;
        }

        private static double ParseDouble(string value, string fieldName, List<string> errors)
        {
            string cleaned = Regex.Replace(value, @"[^\d.-]", "");
            if (double.TryParse(cleaned, out double result))
                return result;

            errors.Add($"{fieldName}: '{value}' is not a valid number");
            return 0.0;
        }

        // ==================== OWNER SEMANTICS ====================

        private static void ApplyOwnerSemanticNormalization(BaselineLandParcelRecord record)
        {
            record.LandOwnersName = NormalizeNullable(record.LandOwnersName);
            record.FatherSpouse = NormalizeNullable(record.FatherSpouse);
            record.Gender = NormalizeNullable(record.Gender);
            record.CitizenshipNumber = NormalizeNullable(record.CitizenshipNumber);
            record.CitizenshipIssuedDistrict = NormalizeNullable(record.CitizenshipIssuedDistrict);
            record.CitizenshipIssuedDate = NormalizeNullable(record.CitizenshipIssuedDate);
            record.PermanentAddress = NormalizeNullable(record.PermanentAddress);
            record.TemporaryAddress = NormalizeNullable(record.TemporaryAddress);
            record.ContactNumber = NormalizeNullable(record.ContactNumber);
            record.EmailID = NormalizeNullable(record.EmailID);
            record.Tenant = NormalizeNullable(record.Tenant);
            record.TenantName = NormalizeNullable(record.TenantName);
            record.LandOwnershipType = NormalizeNullable(record.LandOwnershipType);

            if (!string.IsNullOrWhiteSpace(record.CitizenshipNumber))
            {
                record.CitizenshipNumber = NormalizeCitizenship(record.CitizenshipNumber);
            }

            if (!IsInstitutionOwner(record.LandOwnersName, record.LandOwnershipType))
            {
                return;
            }

            // Institution/government owners should never retain person-only identity fields.
            record.FatherSpouse = null;
            record.Gender = null;
            record.CitizenshipNumber = null;
            record.CitizenshipIssuedDistrict = null;
            record.CitizenshipIssuedDate = null;
            record.PermanentAddress = null;
            record.TemporaryAddress = null;
            record.ContactNumber = null;
            record.EmailID = null;
            record.LandOwnershipType ??= "Government/Institution";
        }

        private static bool IsInstitutionOwner(string? ownerName, string? ownershipType)
        {
            return ContainsInstitutionKeyword(ownerName) || ContainsInstitutionKeyword(ownershipType);
        }

        private static bool ContainsInstitutionKeyword(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = OwnerDeduplicationService.NormalizeString(value);
            return NepalDomainConstants.InstitutionKeywords.Any(keyword =>
                normalized.Contains(
                    OwnerDeduplicationService.NormalizeString(keyword),
                    StringComparison.OrdinalIgnoreCase));
        }

        private static string? NormalizeNullable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var cleaned = CleanString(value);
            return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
        }

        private static string NormalizeCitizenship(string value)
        {
            var converted = ConvertDevanagariToArabicDigits(value);
            var normalized = new string(converted.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }

        private static string ConvertDevanagariToArabicDigits(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var builder = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (c >= '\u0966' && c <= '\u096F')
                {
                    builder.Append((char)('0' + (c - '\u0966')));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
        // ==================== BASE BUSINESS RULES ====================

        private static void ValidateBusinessRules(
            BaselineLandParcelRecord record,
            int rowNumber,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(record.ParcelNo))
                errors.Add("ParcelNo is required");

            if (string.IsNullOrWhiteSpace(record.MapSheetNo))
                errors.Add("MapSheetNo is required");

            if (!record.AreaInSqm.HasValue || record.AreaInSqm.Value <= 0)
                errors.Add("AreaInSqm must be greater than 0");
        }

        /// <summary>
        /// Validates a single record and returns a list of validation errors.
        /// Returns empty list if record is valid.
        /// </summary>
        public static List<string> ValidateSingleRecord(BaselineLandParcelRecord record, int rowNumber)
        {
            var errors = new List<string>();
            ValidateBusinessRules(record, rowNumber, errors);
            return errors;
        }
    }

    // ==================== RESULT MODELS ====================

    public class TransformationResult
    {
        public List<BaselineLandParcelRecord> ValidRecords { get; set; } = new();
        public List<BaselineLandParcelRecord> InvalidRecords { get; set; } = new();
        public List<BaselineLandParcelRecord> AllOriginalRecords { get; set; } = new();
        public List<ValidationError> ValidationErrors { get; set; } = new();
        public int SkippedRows { get; set; } = 0;

        public int TotalRecords => ValidRecords.Count + InvalidRecords.Count;
        public bool HasErrors => ValidationErrors.Count > 0;
    }

    public class ValidationError
    {
        public int RowNumber { get; set; }
        public BaselineLandParcelRecord? RecordData { get; set; }
        public List<string> Errors { get; set; } = new();

        // Set when ALL errors on this row are solely caused by a duplicate parcel number.
        // Used by frmValidationErrors to offer "Mark as Joint Ownership".
        public bool IsDuplicateParcel { get; set; }

        // Normalised "MAPSHEET::PARCELNO" key shared by all rows in the same duplicate group.
        public string DuplicateParcelKey { get; set; } = string.Empty;

        public string ErrorSummary => $"Row {RowNumber}: {string.Join("; ", Errors)}";
    }
}

