using Land_Readjustment_Tool.Models;
using System.Data;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmMapping : Form
    {
        public frmMapping()
        {
            InitializeComponent();
        }

        public void InititalizeMappingGrid(DataTable exceldata)
        {
            // Clear existing columns and rows
            dgvMapping.RowHeadersVisible = false;
            dgvMapping.AllowUserToResizeColumns = true;
            dgvMapping.Columns.Clear();
            dgvMapping.Rows.Clear();

            // Enable custom header style and make headers bold
            dgvMapping.EnableHeadersVisualStyles = false;
            dgvMapping.ColumnHeadersDefaultCellStyle.Font = new Font(dgvMapping.Font, FontStyle.Bold);

            // Add Target Field column (TextBox column - read only)
            var targetFieldColumn = new DataGridViewTextBoxColumn
            {
                Name = "TargetField",
                HeaderText = "Target Field",
                ReadOnly = true
            };

            // Make first column cells bold
            targetFieldColumn.DefaultCellStyle.Font = new Font(dgvMapping.Font, FontStyle.Bold);

            _ = dgvMapping.Columns.Add(targetFieldColumn);

            // Get Excel column names as List
            List<string> excelColumnNames = exceldata.Columns
                .Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();

            // Add empty option at the beginning
            excelColumnNames.Insert(0, "-- Not Mapped --");

            // Add Source Field column (ComboBox column)
            var comboCol = new DataGridViewComboBoxColumn
            {
                Name = "SourceColumn",
                HeaderText = "Source Field",
                DataSource = excelColumnNames
            };

            _ = dgvMapping.Columns.Add(comboCol);

            // Add rows for each property in OriginalLandOwnersRecord
            foreach (var prop in typeof(BaselineLandParceRecord).GetProperties())
            {
                int rowIndex = dgvMapping.Rows.Add();
                dgvMapping.Rows[rowIndex].Cells["TargetField"].Value = prop.Name;
                dgvMapping.Rows[rowIndex].Cells["SourceColumn"].Value = "-- Not Mapped --";
            }

            // Auto-size columns based on content
            dgvMapping.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }


        public void ClearMappings()
        {
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                if (row.IsNewRow) continue;
                row.Cells["SourceColumn"].Value = "-- Not Mapped --";
            }
        }

        public Dictionary<string, string> GetFieldMappings()
        {
            var mappings = new Dictionary<string, string>();
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                if (row.IsNewRow) continue;
                string targetField = row.Cells["TargetField"].Value?.ToString() ?? string.Empty;
                string sourceField = row.Cells["SourceColumn"].Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(targetField) &&
                    !string.IsNullOrEmpty(sourceField) &&
                    sourceField != "-- Not Mapped --")
                {
                    mappings[targetField] = sourceField;
                }
            }
            return mappings;
        }

        // New method to validate required field mappings
        private bool ValidateRequiredFields(out List<string> missingFields)
        {
            missingFields = new List<string>();
            // Derive required field names from the data model (nameof) and DataGridView rows
            var allTargetFields = dgvMapping.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => r.Cells["TargetField"].Value?.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!) // ensure non-null strings
                .ToList();

            var requiredFields = allTargetFields
                .Where(name =>
                    name == nameof(BaselineLandParceRecord.ParcelNo) ||
                    name == nameof(BaselineLandParceRecord.MapSheetNo) ||
                    name == nameof(BaselineLandParceRecord.AreaInSqm))
                .ToList();

            var mappings = GetFieldMappings();

            foreach (string field in requiredFields)
            {
                if (!mappings.ContainsKey(field))
                {
                    missingFields.Add(field);
                }
            }

            return missingFields.Count == 0;
        }

        private void btnCLear_Click(object sender, EventArgs e)
        {
            ClearMappings();
        }

        private void btnConfirmMapping_Click(object sender, EventArgs e)
        {
            // Validate required fields first
            if (!ValidateRequiredFields(out List<string> missingFields))
            {
                string missingFieldsList = string.Join(", ", missingFields);
                MessageBox.Show(
                    $"The following required fields must be mapped:\n\n{missingFieldsList}\n\nPlease map these fields before confirming.",
                    "Required Fields Missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var mappings = GetFieldMappings();
            DialogResult result = MessageBox.Show(
                $"You have mapped {mappings.Count} field(s).\n\nAre you sure you want to apply this mapping?",
                "Confirm Mapping",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void frmMapping_Load(object sender, EventArgs e)
        {

        }
    }
}