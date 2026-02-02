using ExcelDataReader;
using ExcelDataReader.Exceptions;
using System.Data;

namespace Land_Readjustment_Tool.Services
{
    /// <summary>
    /// Enhanced service for importing Excel files with multi-sheet support
    /// </summary>
    public class ExcelImportService
    {
        public ExcelImportService()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        /// <summary>
        /// Opens file dialog and returns path of selected file
        /// </summary>
        public static string? GetExcelFilePathWithDialog()
        {
            using OpenFileDialog ofd = new()
            {
                Filter = "Excel Files (*.xls;*.xlsx)|*.xls;*.xlsx",
                Title = "Select an Excel File"
            };

            return ofd.ShowDialog() == DialogResult.OK ? ofd.FileName : null;
        }

        /// <summary>
        /// Reads Excel file and returns DataSet with ALL sheets
        /// </summary>
        public static DataSet? ReadExcelFileAsDataSet(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    MessageBox.Show("The selected file does not exist.", "File Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                FileInfo fileInfo = new(fileName);
                if (fileInfo.Length == 0)
                {
                    MessageBox.Show("The selected file is empty.", "Invalid File",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                using var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = ExcelReaderFactory.CreateReader(stream);

                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });

                if (dataSet.Tables.Count == 0)
                {
                    MessageBox.Show("The Excel file does not contain any sheets with data.",
                        "No Data Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                // Validate that at least one table has data
                bool hasValidData = false;
                foreach (DataTable table in dataSet.Tables)
                {
                    if (table.Rows.Count > 0 && table.Columns.Count > 0)
                    {
                        hasValidData = true;
                        break;
                    }
                }

                if (!hasValidData)
                {
                    MessageBox.Show("The Excel file does not contain any valid data.",
                        "No Valid Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                return dataSet;
            }
            catch (IOException ioEx)
            {
                MessageBox.Show($"Unable to access the file. It may be open in another program.\n\nError: {ioEx.Message}",
                    "File Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (UnauthorizedAccessException uaEx)
            {
                MessageBox.Show($"You do not have permission to access this file.\n\nError: {uaEx.Message}",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (ExcelReaderException erEx)
            {
                MessageBox.Show($"Failed to read the Excel file. The file may be corrupted.\n\nError: {erEx.Message}",
                    "Excel Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred while reading the Excel file.\n\nError: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// Gets a specific sheet from DataSet by name
        /// </summary>
        public static DataTable? GetSheetByName(DataSet dataSet, string sheetName)
        {
            if (dataSet == null || string.IsNullOrWhiteSpace(sheetName))
                return null;

            return dataSet.Tables.Contains(sheetName) ? dataSet.Tables[sheetName] : null;
        }

        /// <summary>
        /// Gets a specific sheet from DataSet by index
        /// </summary>
        public static DataTable? GetSheetByIndex(DataSet dataSet, int index)
        {
            if (dataSet == null || index < 0 || index >= dataSet.Tables.Count)
                return null;

            return dataSet.Tables[index];
        }

        /// <summary>
        /// Gets list of all sheet names from DataSet
        /// </summary>
        public static List<string> GetSheetNames(DataSet dataSet)
        {
            var sheetNames = new List<string>();
            if (dataSet == null) return sheetNames;

            foreach (DataTable table in dataSet.Tables)
            {
                sheetNames.Add(table.TableName);
            }

            return sheetNames;
        }

        /// <summary>
        /// Validates if a DataTable has valid structure for import
        /// </summary>
        public static bool ValidateDataTable(DataTable table, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (table == null)
            {
                errorMessage = "Table is null";
                return false;
            }

            if (table.Columns.Count == 0)
            {
                errorMessage = "Table has no columns";
                return false;
            }

            if (table.Rows.Count == 0)
            {
                errorMessage = "Table has no data rows";
                return false;
            }

            // Check if all rows are empty
            bool hasData = false;
            foreach (DataRow row in table.Rows)
            {
                if (row.ItemArray.Any(field => field != null && !string.IsNullOrWhiteSpace(field.ToString())))
                {
                    hasData = true;
                    break;
                }
            }

            if (!hasData)
            {
                errorMessage = "Table contains only empty rows";
                return false;
            }

            return true;
        }
    }
}
