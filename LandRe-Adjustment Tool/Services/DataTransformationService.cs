using Land_Readjustment_Tool.Models;
using System.Data;
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

                var record = new OriginalLandParcelWithLandOwner();
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

                var record = new OriginalLandParcelWithLandOwner();
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
            string? parcelNo = GetMappedValue(row, fieldMappings, nameof(OriginalLandParcelWithLandOwner.ParcelNo));
            string? mapSheetNo = GetMappedValue(row, fieldMappings, nameof(OriginalLandParcelWithLandOwner.MapSheetNo));
            string? areaInSqm = GetMappedValue(row, fieldMappings, nameof(OriginalLandParcelWithLandOwner.AreaInSqm));

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
                    MapSheet = x.record.MapSheetNo.Trim().ToUpper(),
                    Parcel = x.record.ParcelNo.Trim().ToUpper()
                })
                .Where(g => g.Count() > 1);

            foreach (var group in groups)
            {
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
                            Errors = new List<string>
                            {
                                $"Duplicate ParcelNo '{item.record.ParcelNo}' found in MapSheet '{item.record.MapSheetNo}'"
                            }
                        };

                        result.ValidationErrors.Add(error);
                        result.InvalidRecords.Add(item.record);
                        result.ValidRecords.Remove(item.record);
                    }
                    else
                    {
                        existingError.Errors.Add(
                            $"Duplicate ParcelNo '{item.record.ParcelNo}' found in MapSheet '{item.record.MapSheetNo}'"
                        );
                    }
                }
            }
        }

        // ==================== PROPERTY MAPPING ====================

        private static void SetPropertyValue(
            OriginalLandParcelWithLandOwner record,
            string propertyName,
            object value,
            List<string> errors)
        {
            var property = typeof(OriginalLandParcelWithLandOwner).GetProperty(propertyName);
            if (property == null) return;

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

        // ==================== BASE BUSINESS RULES ====================

        private static void ValidateBusinessRules(
            OriginalLandParcelWithLandOwner record,
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
        public static List<string> ValidateSingleRecord(OriginalLandParcelWithLandOwner record, int rowNumber)
        {
            var errors = new List<string>();
            ValidateBusinessRules(record, rowNumber, errors);
            return errors;
        }
    }

    // ==================== RESULT MODELS ====================

    public class TransformationResult
    {
        public List<OriginalLandParcelWithLandOwner> ValidRecords { get; set; } = new();
        public List<OriginalLandParcelWithLandOwner> InvalidRecords { get; set; } = new();
        public List<OriginalLandParcelWithLandOwner> AllOriginalRecords { get; set; } = new();
        public List<ValidationError> ValidationErrors { get; set; } = new();
        public int SkippedRows { get; set; } = 0;

        public int TotalRecords => ValidRecords.Count + InvalidRecords.Count;
        public bool HasErrors => ValidationErrors.Count > 0;
    }

    public class ValidationError
    {
        public int RowNumber { get; set; }
        public OriginalLandParcelWithLandOwner? RecordData { get; set; }
        public List<string> Errors { get; set; } = new();

        public string ErrorSummary => $"Row {RowNumber}: {string.Join("; ", Errors)}";
    }
}
