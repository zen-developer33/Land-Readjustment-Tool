using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Form to display validation errors and allow users to fix them
    /// </summary>
    public partial class frmValidationErrors : Form
    {
        private List<ValidationError> _errors;
        private BindingList<BaselineLandParceRecord> _records;
        private BindingSource _errorBindingSource;

        public frmValidationErrors(List<ValidationError> errors, BindingList<BaselineLandParceRecord> records)
        {
            InitializeComponent();
            _errors = errors;
            _records = records;
            InitializeErrorList();
        }

        private void InitializeErrorList()
        {
            // Configure DataGridView
            dgvErrors.AutoGenerateColumns = false;
            dgvErrors.AllowUserToAddRows = false;
            dgvErrors.AllowUserToDeleteRows = false;
            dgvErrors.AllowUserToResizeRows = false;
            dgvErrors.ReadOnly = true;
            dgvErrors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvErrors.MultiSelect = false;

            // Add columns
            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RowNumber",
                HeaderText = "Row",
                DataPropertyName = "RowNumber",
                Width = 60
            });

            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ParcelNo",
                HeaderText = "Parcel No",
                Width = 100
            });

            dgvErrors.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ErrorSummary",
                HeaderText = "Error Description",
                DataPropertyName = "ErrorSummary",
                Width = 500,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Bind data
            _errorBindingSource = new BindingSource(_errors, null);
            dgvErrors.DataSource = _errorBindingSource;

            // Fill ParcelNo column manually
            for (int i = 0; i < dgvErrors.Rows.Count && i < _errors.Count; i++)
            {
                dgvErrors.Rows[i].Cells["ParcelNo"].Value = _errors[i].RecordData?.ParcelNo ?? "N/A";
            }

            // Update count
            lblErrorCount.Text = $"{_errors.Count} error(s) found";

            // Select first row if available
            if (dgvErrors.Rows.Count > 0)
            {
                dgvErrors.Rows[0].Selected = true;
            }
        }

        private void btnFixSelected_Click(object sender, EventArgs e)
        {
            if (dgvErrors.SelectedRows.Count == 0) return;

            int errorIndex = dgvErrors.SelectedRows[0].Index;
            if (errorIndex < 0 || errorIndex >= _errors.Count) return;
            
            var error = _errors[errorIndex];
            int recordIndex = error.RowNumber - 1;

            if (recordIndex < 0 || recordIndex >= _records.Count) return;

            // Open edit form
            var record = _records[recordIndex];
            using (var editForm = new frmAddEditRecord(record, recordIndex))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    _records[recordIndex] = editForm.Record;
                    
                    // Re-validate just this record to check if error is fixed
                    var validationErrors = DataTransformationService.ValidateSingleRecord(editForm.Record, recordIndex + 1);
                    
                    if (validationErrors.Count == 0)
                    {
                        // Error is fixed - remove from list
                        _errors.RemoveAt(errorIndex);
                        RefreshErrorList();
                        
                        if (_errors.Count == 0)
                        {
                            MessageBox.Show("All errors have been fixed!", "All Errors Resolved",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Error fixed! The record is now valid.", "Error Fixed",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        // Still has errors - update the error entry
                        _errors[errorIndex] = new ValidationError
                        {
                            RowNumber = recordIndex + 1,
                            RecordData = editForm.Record,
                            Errors = validationErrors
                        };
                        RefreshErrorList();
                        
                        MessageBox.Show("Record updated but still has validation errors:\n\n" + 
                            string.Join("\n", validationErrors),
                            "Errors Remaining", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the error list display after changes
        /// </summary>
        private void RefreshErrorList()
        {
            // Rebind data source
            _errorBindingSource.DataSource = null;
            _errorBindingSource.DataSource = _errors;
            dgvErrors.DataSource = _errorBindingSource;
            
            // Re-fill ParcelNo column
            for (int i = 0; i < dgvErrors.Rows.Count && i < _errors.Count; i++)
            {
                dgvErrors.Rows[i].Cells["ParcelNo"].Value = _errors[i].RecordData?.ParcelNo ?? "N/A";
            }
            
            // Update count
            lblErrorCount.Text = $"{_errors.Count} error(s) remaining";
            
            // Select first row if available
            if (dgvErrors.Rows.Count > 0)
            {
                dgvErrors.Rows[0].Selected = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void dgvErrors_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnFixSelected_Click(sender, e);
        }

        private void btnExportErrors_Click(object sender, EventArgs e)
        {
            // Export errors to text file
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text Files (*.txt)|*.txt|CSV Files (*.csv)|*.csv";
                sfd.Title = "Export Validation Errors";
                sfd.FileName = $"Validation_Errors_{DateTime.Now:yyyyMMdd_HHmmss}";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter(sfd.FileName))
                        {
                            writer.WriteLine("Validation Errors Report");
                            writer.WriteLine($"Generated: {DateTime.Now}");
                            writer.WriteLine($"Total Errors: {_errors.Count}");
                            writer.WriteLine(new string('=', 80));
                            writer.WriteLine();

                            foreach (var error in _errors)
                            {
                                writer.WriteLine($"Row {error.RowNumber}: Parcel {error.RecordData?.ParcelNo ?? "N/A"}");
                                foreach (var err in error.Errors)
                                {
                                    writer.WriteLine($"  - {err}");
                                }
                                writer.WriteLine();
                            }
                        }

                        MessageBox.Show("Errors exported successfully!", "Export Complete",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export errors: {ex.Message}", "Export Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void frmValidationErrors_Load(object sender, EventArgs e)
        {

        }
    }
}
