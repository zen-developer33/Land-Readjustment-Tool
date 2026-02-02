
//using Land_Readjustment_Tool.Forms;
//using Land_Readjustment_Tool.Models;

//using Land_Readjustment_Tool.Services;
//using System.ComponentModel;
//using System.Data;
//using System.Reflection;


//namespace Land_Readjustment_Tool
//{
//    public partial class frmLandownersRecord : Form
//    {
//        //private OriginalLandParcelsWithLandOwnersRepository _repository;
//        private DatabaseHelper _dbHelper;
//        private BindingList<OriginalLandParcelWithLandOwner> _OriginalParcelWithOwnerBindingList;

//        private TransformationResult transformResult = new();

//        // ==================== CONTEXT MENU SETUP ====================
//        private ContextMenuStrip contextMenuGrid;
//        public frmLandownersRecord()
//        {
//            InitializeComponent();

//            InitializeDataGridView();



//            _OriginalParcelWithOwnerBindingList = new BindingList<OriginalLandParcelWithLandOwner>();
//            dataLandOwnersRecord.DataSource = _OriginalParcelWithOwnerBindingList;

//            SetupContextMenu();

//            dataLandOwnersRecord.CellDoubleClick += dataLandOwnersRecord_CellDoubleClick;
//            dataLandOwnersRecord.DataBindingComplete += (_, _) => updateRecordCount();

//            updateRecordCount();
//        }


//        private void SetupContextMenu()
//        {
//            contextMenuGrid = new ContextMenuStrip();

//            var editMenuItem = new ToolStripMenuItem("Edit Record");
//            editMenuItem.Click += ContextMenu_EditRecord_Click;

//            var deleteMenuItem = new ToolStripMenuItem("Delete Record");
//            deleteMenuItem.Click += ContextMenu_DeleteRecord_Click;

//            _ = contextMenuGrid.Items.Add(editMenuItem);
//            _ = contextMenuGrid.Items.Add(deleteMenuItem);

//            dataLandOwnersRecord.ContextMenuStrip = contextMenuGrid;
//        }



//        // ==================== ADD RECORD BUTTON ====================


//        private void btnAddRecord_Click(object sender, EventArgs e)
//        {
//            var addForm = new frmAddEditRecord();

//            if (addForm.ShowDialog() == DialogResult.OK)
//            {
//                // Get the binding list
//                _OriginalParcelWithOwnerBindingList.Add(addForm.Record);

//                dataLandOwnersRecord.ClearSelection();
//            }
//        }

//        // ==================== EDIT RECORD BUTTON ====================
//        private void btnEditRecord_Click(object sender, EventArgs e)
//        {
//            if (dataLandOwnersRecord.Rows.Count == 0)
//            {
//                _ = MessageBox.Show("There are no records to edit.", "No Records",
//                    MessageBoxButtons.OK, MessageBoxIcon.Information);
//                return;
//            }
//            if (dataLandOwnersRecord.SelectedRows.Count == 0)
//            {
//                _ = MessageBox.Show("Please select a record to edit.", "No Selection",
//                    MessageBoxButtons.OK, MessageBoxIcon.Information);
//                return;
//            }



//            int rowIndex = dataLandOwnersRecord.SelectedRows[0].Index;
//            OpenEditForm(rowIndex);

//        }


//        // ==================== DATAGRIDVIEW DOUBLE-CLICK ====================
//        private void dataLandOwnersRecord_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
//        {
//            // Ignore header row and new row
//            if (e.RowIndex < 0 || dataLandOwnersRecord.Rows[e.RowIndex].IsNewRow)
//                return;

//            OpenEditForm(e.RowIndex);
//        }

//        // ==================== CONTEXT MENU HANDLERS ====================
//        private void ContextMenu_EditRecord_Click(object sender, EventArgs e)
//        {
//            if (dataLandOwnersRecord.SelectedRows.Count > 0)
//            {
//                int rowIndex = dataLandOwnersRecord.SelectedRows[0].Index;
//                OpenEditForm(rowIndex);
//            }
//        }

//        private void ContextMenu_DeleteRecord_Click(object sender, EventArgs e)
//        {
//            if (dataLandOwnersRecord.SelectedRows.Count == 0)
//                return;

//            DialogResult result = MessageBox.Show(
//                "Are you sure you want to delete this record?",
//                "Confirm Delete",
//                MessageBoxButtons.YesNo,
//                MessageBoxIcon.Warning);

//            if (result == DialogResult.Yes)
//            {
//                foreach (DataGridViewRow row in dataLandOwnersRecord.SelectedRows)
//                {
//                    if (!row.IsNewRow)
//                        _OriginalParcelWithOwnerBindingList.RemoveAt(row.Index);
//                }
//                RefreshValidationStatus();
//                updateRecordCount();
//            }
//        }

//        // ==================== SHARED EDIT FORM LOGIC ====================
//        private void OpenEditForm(int rowIndex)
//        {

//            if (_OriginalParcelWithOwnerBindingList == null || rowIndex < 0 || rowIndex >= _OriginalParcelWithOwnerBindingList.Count)
//                return;

//            // Get the record
//            var record = _OriginalParcelWithOwnerBindingList[rowIndex];

//            // Open edit form with record data
//            var editForm = new frmAddEditRecord(record, rowIndex);

//            if (editForm.ShowDialog() == DialogResult.OK)
//            {
//                if (editForm.IsDeleted)
//                {
//                    // Remove record
//                    _OriginalParcelWithOwnerBindingList.RemoveAt(rowIndex);
//                    dataLandOwnersRecord.Refresh();
//                    dataLandOwnersRecord.ClearSelection();
//                    _ = MessageBox.Show("Record deleted successfully!", "Deleted",
//                      MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//                else
//                {
//                    // Update record
//                    _OriginalParcelWithOwnerBindingList[rowIndex] = editForm.Record;
//                    dataLandOwnersRecord.Refresh();
//                    dataLandOwnersRecord.ClearSelection();
//                    _ = MessageBox.Show("Record updated successfully!", "Updated",
//                        MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//            }
//            RefreshValidationStatus();
//        }


//        //private void btnImportFromExcel_Click(object? sender, EventArgs e)
//        //{
//        //    // Step 1: Read Excel file
//        //    DataTable? excelTable = ExcelImportService.GetDataTableFromExcelWithDialog();
//        //    if (excelTable == null)
//        //        return;

//        //    // Step 2: Show mapping form
//        //    frmMapping mappingForm = new frmMapping();
//        //    mappingForm.InititalizeMappingGrid(excelTable);

//        //    if (mappingForm.ShowDialog() != DialogResult.OK)
//        //        return;

//        //    // Step 3: Get field mappings
//        //    var fieldMapping = mappingForm.GetFieldMappings();

//        //    // Step 4: Transform and validate data (NEW!)
//        //    transformResult = DataTransformationService.TransformDataToEntities(excelTable, fieldMapping);

//        //    _OriginalParcelWithOwnerBindingList = new BindingList<OriginalLandParcelWithLandOwner>(transformResult.AllOriginalRecords);
//        //    // Step 5: Show validation summary

//        //    if (transformResult.HasErrors)
//        //    {
//        //        string message = $"Import Summary:\n\n" +
//        //                        $"There are validation Errors !\n" +
//        //                        $"✅ Valid records: {transformResult.ValidRecords.Count}\n" +
//        //                        $"❌ Invalid records: {transformResult.InvalidRecords.Count}\n\n" +
//        //                        $"Do you want to continue ?";

//        //        DialogResult choice = MessageBox.Show(message, "Validation Results",
//        //            MessageBoxButtons.YesNo, MessageBoxIcon.Question);



//        //        if (choice == DialogResult.No)
//        //        {
//        //            return;
//        //        }
//        //    }


//        //    // Step 7: Display in DataGridView
//        //    InitializeDataGridView();
//        //    dataLandOwnersRecord.DataSource = _OriginalParcelWithOwnerBindingList;
//        //    dataLandOwnersRecord.ClearSelection();

//        //    // Step 8: Color-code rows based on validation

//        //    // Step 9: Update record count
//        //    updateRecordCount();

//        //    // Step 10: Show success message
//        //    DialogResult result = MessageBox.Show(
//        //       $"Import completed!\n\n" +
//        //       $"Valid: {transformResult.ValidRecords.Count}\n" +
//        //       $"Invalid: {transformResult.InvalidRecords.Count}\n" +
//        //       $"Total: {transformResult.TotalRecords}",
//        //       "Import Complete",
//        //       MessageBoxButtons.OK,
//        //       MessageBoxIcon.Information);

//        //    if (result == DialogResult.OK)
//        //    {
//        //        // Show error details form (create this later)
//        //        ShowValidationErrors(transformResult.ValidationErrors);
//        //        ColorCodeValidationStatus(transformResult.ValidationErrors);
//        //    }
//        //    btnShowValidationErrors.Enabled = true;
//        //}

//        //To update the validation error and color code after editing or adding a record    \
//        private void RefreshValidationStatus()
//        {
//            DataTable DataGridTable = DataTransformationService.ConvertGridToDataTable(dataLandOwnersRecord);
//            transformResult = DataTransformationService.ValidateFromDataTable(DataGridTable);

//            UpdateValidationStatus();
//            ColorCodeValidationStatus(transformResult.ValidationErrors);
//            updateRecordCount();
//        }



//        //To Update the Validation Status Column
//        private void UpdateValidationStatus()
//        {
//            var invalidRows = transformResult.ValidationErrors
//               .Select(e => e.RowNumber - 1)
//               .ToHashSet();

//            foreach (DataGridViewRow row in dataLandOwnersRecord.Rows)
//            {
//                if (row.IsNewRow) continue;

//                row.Cells["ValidationStatus"].Value =
//                    invalidRows.Contains(row.Index) ? "Invalid" : "Valid";
//            }
//        }

//        // New helper method: Color-code rows
//        private void ColorCodeValidationStatus(List<ValidationError> errors)
//        {
//            var invalidRows = errors
//                .Select(e => e.RowNumber - 1)
//                .ToHashSet();

//            foreach (DataGridViewRow row in dataLandOwnersRecord.Rows)
//            {
//                if (row.IsNewRow) continue;

//                if (invalidRows.Contains(row.Index))
//                {
//                    row.DefaultCellStyle.BackColor = Color.LightCoral;
//                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
//                }
//                else
//                {
//                    row.DefaultCellStyle.BackColor = Color.White;
//                    row.DefaultCellStyle.ForeColor = Color.Black;
//                }
//            }
//        }

//        // New helper method: Show validation errors (placeholder)
//        private void ShowValidationErrors(List<ValidationError> ValError)
//        {
//            // Create a simple error display form
//            var errorForm = new Form
//            {
//                Text = "Validation Errors",
//                Width = 600,
//                Height = 400,
//                StartPosition = FormStartPosition.CenterScreen,
//                TopMost = true,
//                ShowIcon = false
//            };

//            var listBox = new ListBox
//            {
//                Dock = DockStyle.Fill,
//                Font = new Font("Consolas", 9),
//                ForeColor = Color.DarkRed
//            };

//            errorForm.Controls.Add(listBox);

//            var btnClose = new Button
//            {
//                Text = "Close",
//                Dock = DockStyle.Bottom,
//                Height = 40
//            };
//            btnClose.Click += (s, e) => errorForm.Close();
//            errorForm.Controls.Add(btnClose);

//            errorForm.Show(); // this opens as a non-modal window and it allows interaction with the main form.
//            foreach (var errors in ValError)
//            {
//                _ = listBox.Items.Add(errors.ErrorSummary);
//            }
//        }


//        private void updateRecordCount()
//        {
//            txtRecordsCount.Text =
//                (dataLandOwnersRecord.Rows.Count - (dataLandOwnersRecord.AllowUserToAddRows ? 1 : 0)).ToString();
//        }


//        private void btnDelete_Click(object? sender, EventArgs e)
//        {
//            if (dataLandOwnersRecord.Rows.Count == 0)
//            {
//                _ = MessageBox.Show("There are no records to delete.", "No Records",
//                    MessageBoxButtons.OK, MessageBoxIcon.Information);
//                return;
//            }
//            if (dataLandOwnersRecord.SelectedRows.Count == 0)
//            {
//                _ = MessageBox.Show("Please select a record to delete.", "No Selection",
//                    MessageBoxButtons.OK, MessageBoxIcon.Information);
//                return;
//            }

//            DialogResult result = MessageBox.Show(
//                "Are you sure you want to delete this record?",
//                "Confirm Delete",
//                MessageBoxButtons.YesNo,
//                MessageBoxIcon.Warning);

//            if (result == DialogResult.Yes)
//            {
//                foreach (DataGridViewRow row in dataLandOwnersRecord.SelectedRows)
//                {
//                    if (!row.IsNewRow)
//                        _OriginalParcelWithOwnerBindingList.RemoveAt(row.Index);
//                }
//                RefreshValidationStatus();
//                updateRecordCount();
//            }
//        }

//        private void frmLandownersRecord_Load(object? sender, EventArgs e)
//        {

//        }

//        private void InitializeDataGridView()
//        {
//            dataLandOwnersRecord.AutoGenerateColumns = false;
//            dataLandOwnersRecord.Columns.Clear();

//            // Add columns for each property
//            foreach (PropertyInfo property in typeof(OriginalLandParcelWithLandOwner).GetProperties())
//            {
//                _ = dataLandOwnersRecord.Columns.Add(new DataGridViewTextBoxColumn
//                {
//                    Name = property.Name,
//                    HeaderText = property.Name.Replace("_", " "),
//                    DataPropertyName = property.Name,
//                    SortMode = DataGridViewColumnSortMode.Automatic  // This enables sorting by clicking headers
//                });
//            }

//            // Add validation status column
//            var statusColumn = new DataGridViewTextBoxColumn
//            {
//                Name = "ValidationStatus",
//                HeaderText = "Status",
//                ReadOnly = true,
//                Width = 80,
//                SortMode = DataGridViewColumnSortMode.Automatic  // Enable sorting for status column too
//            };
//            _ = dataLandOwnersRecord.Columns.Add(statusColumn);

//            // Make headers bold
//            dataLandOwnersRecord.EnableHeadersVisualStyles = false;
//            dataLandOwnersRecord.ColumnHeadersDefaultCellStyle.Font =
//                new Font("Segoe UI", 9F, FontStyle.Regular);
//            dataLandOwnersRecord.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

//            // Enable column resizing
//            dataLandOwnersRecord.AllowUserToResizeColumns = true;

//            // Enable row resizing
//            dataLandOwnersRecord.AllowUserToResizeRows = true;

//            // Auto-size rows to fit content
//            dataLandOwnersRecord.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

//            // Optional: Auto-size columns to fit content (choose one of these options)
//            // Option 1: Auto-size all columns to fit content
//            dataLandOwnersRecord.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

//            // Option 2: Fill available space (alternative to Option 1)
//            // dataLandOwnersRecord.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

//            // Option 3: Display headers only (alternative to Options 1 & 2)
//            // dataLandOwnersRecord.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

//            // Enable word wrap for better multi-line cell display
//            dataLandOwnersRecord.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

//            // Set the grid to read-only
//            dataLandOwnersRecord.ReadOnly = true;
//        }

//        private void frmLandownersRecord_Load_1(object? sender, EventArgs e)
//        {
//            updateRecordCount();
//        }



//        private void btnShowValidationErrors_Click(object sender, EventArgs e)
//        {
//            DataTable DataGridTable = DataTransformationService.ConvertGridToDataTable(dataLandOwnersRecord);
//            transformResult = DataTransformationService.ValidateFromDataTable(DataGridTable);
//            ShowValidationErrors(transformResult.ValidationErrors);
//            ColorCodeValidationStatus(transformResult.ValidationErrors);

//        }

//        private void btnClose_Click(object sender, EventArgs e)
//        {
//            this.Close();

//        }

//        private void btnSave_Click(object sender, EventArgs e)
//        {

//        }


//        private void btnImport_Click(object? sender, EventArgs e)
//        {
//            using (var importManager = new frmImportManager())
//            {
//                if (importManager.ShowDialog() == DialogResult.OK)
//                {
//                    var validRecords = importManager.ImportedRecords;
//                    _OriginalParcelWithOwnerBindingList.Clear();
//                    foreach (var record in validRecords)
//                        _OriginalParcelWithOwnerBindingList.Add(record);

//                    dataLandOwnersRecord.Refresh();
//                    updateRecordCount();
//                }
//            }
//        }
//    }
//}
