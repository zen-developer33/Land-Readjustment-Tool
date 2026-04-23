using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.Import;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Core.Interfaces;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Complete Import Manager with Owner Deduplication
    /// Step 1: Load Excel File & Select Sheet
    /// Step 2: Map Fields to Model
    /// Step 3: Review & Edit Records (with fuzzy deduplication)
    /// Step 4: Validate & Save to Database
    /// </summary>
    public partial class frmImportParcelOwnershipRecords : Form
    {
        // ==================== FIELDS ====================
        private DataSet? _excelDataSet;
        private DataTable? _selectedSheet;
        private Dictionary<string, string> _fieldMappings = new();
        private SortableBindingList<BaselineLandParceRecord> _importedRecords = new();
        private List<ValidationError> _validationErrors = new();
        private HashSet<int> _deletedRowIndices = new();
        private HashSet<int> _editedRowIndices = new();
        private bool _isValidated = false;
        private bool _isDeduplicationDone = false;
        private OwnerDeduplicationService.DeduplicationResult? _deduplicationResult = null;
        private BackgroundWorker _backgroundWorker;
        private ContextMenuStrip _contextMenu;

        // Database connection (injected from main form)
        private readonly string _projectPath;

        // ==================== PROPERTIES ====================
        public int ImportedCount => _importedRecords.Count;

        // ==================== CONSTRUCTOR ====================
        public frmImportParcelOwnershipRecords(string projectPath)
        {
            InitializeComponent();
            _projectPath = projectPath;
            InitializeBackgroundWorker();
            InitializeContextMenu();
            InitializeSteps();
            InitializeDataGridView();
            InitializeMappingGridStyles();
        }

        private IImportPersistenceService GetPersistenceService()
        {
            if (!AppServices.HasContext)
            {
                throw new InvalidOperationException("No open project context found.");
            }

            return new ImportPersistenceService(AppServices.Context.Session);
        }

        private void InitializeMappingGridStyles()
        {
            dgvMapping.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvMapping.EnableHeadersVisualStyles = false;
            dgvMapping.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dgvMapping.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvMapping.RowsDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvMapping.AlternatingRowsDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvMapping.EditingControlShowing += dgvMapping_EditingControlShowing;
        }

        // ==================== INITIALIZATION ====================
        private void InitializeBackgroundWorker()
        {
            _backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            _backgroundWorker.DoWork += BackgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            var editItem = new ToolStripMenuItem("Edit Record");
            editItem.Click += ContextMenu_Edit_Click;

            var deleteItem = new ToolStripMenuItem("Delete Record");
            deleteItem.Click += ContextMenu_Delete_Click;

            var fixItem = new ToolStripMenuItem("Fix Error");
            fixItem.Click += ContextMenu_FixError_Click;

            _contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem, fixItem });
            _contextMenu.Opening += ContextMenu_Opening;

            dgvRecords.ContextMenuStrip = _contextMenu;
        }

        private void InitializeSteps()
        {
            EnableStep1();
            DisableStep2();
            DisableStep3();
            DisableStep4();
            UpdateStatusBar("Ready to import data.");
            InitializeStepStatusIndicators();
        }

        private void InitializeStepStatusIndicators()
        {
            UpdateStepStatus(1, StepStatus.Pending, "Pending");
            UpdateStepStatus(2, StepStatus.Pending, "Pending");
            UpdateStepStatus(3, StepStatus.Pending, "Pending");
            UpdateStepStatus(4, StepStatus.Pending, "Pending");
        }

        private enum StepStatus
        {
            Pending,
            InProgress,
            Success,
            Error
        }

        private void UpdateStepStatus(int stepNumber, StepStatus status, string message)
        {
            Label? stepLabel = stepNumber switch
            {
                1 => lblStep1Status,
                2 => lblStep2Status,
                3 => lblStep3Status,
                4 => lblStep4Status,
                _ => null
            };

            if (stepLabel == null) return;

            string icon = status switch
            {
                StepStatus.Pending => "⏳",
                StepStatus.InProgress => "🔄",
                StepStatus.Success => "✅",
                StepStatus.Error => "❌",
                _ => "⏳"
            };

            Color color = status switch
            {
                StepStatus.Pending => Color.Gray,
                StepStatus.InProgress => Color.DodgerBlue,
                StepStatus.Success => Color.Green,
                StepStatus.Error => Color.Red,
                _ => Color.Gray
            };

            stepLabel.Text = $"Step {stepNumber}: {icon} {message}";
            stepLabel.ForeColor = color;
        }

        private void InitializeDataGridView()
        {
            //dgvRecords.AutoGenerateColumns = false;
            //dgvRecords.AllowUserToAddRows = false;
            //dgvRecords.AllowUserToDeleteRows = false;
            //dgvRecords.AllowUserToResizeRows = false;
            //dgvRecords.AllowUserToResizeColumns = true;
            //dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            //dgvRecords.MultiSelect = true;
            //dgvRecords.ReadOnly = true;
            //dgvRecords.DoubleBuffered(true);
            //dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
        }

        // ==================== STEP 1: LOAD FILE & SELECT SHEET ====================
        private void EnableStep1()
        {
            grpStep1.Enabled = true;
            btnBrowse.Enabled = true;

            cbSelectSheet.Enabled = false;
            btnImportData.Enabled = false;
        }

        private void DisableStep1()
        {
            // Keep Step 1 enabled so user can reload a different file if needed
            // grpStep1.Enabled = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // Warn user if there's existing data
            if (_importedRecords.Count > 0)
            {
                var confirmResult = MessageBox.Show(
                    $"You have {_importedRecords.Count} records already loaded.\n\n" +
                    "Loading a new file will clear all current data and progress.\n\n" +
                    "Do you want to continue?",
                    "Confirm New Import",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmResult != DialogResult.Yes)
                    return;

                // Clear existing data
                ClearAllData();
            }

            string? filePath = ExcelImportService.GetExcelFilePathWithDialog();
            if (filePath != null)
            {
                txtFilePath.Text = filePath;


                string ext = Path.GetExtension(filePath).ToLower();
                cmbFileType.SelectedItem = ext == ".csv" ? "CSV (.csv)" : "Excel (.xlsx)";
            }

            if (string.IsNullOrWhiteSpace(txtFilePath.Text))
            {
                MessageBox.Show("Please select a file first.", "No File Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            progressBar.Style = ProgressBarStyle.Marquee;
            UpdateStatusBar("Loading Excel file...");

            _backgroundWorker.RunWorkerAsync(new BackgroundOperation
            {
                OperationType = OperationType.LoadFile,
                FilePath = txtFilePath.Text
            });
        }


        private void btnImportData_Click(object sender, EventArgs e)
        {
            if (cbSelectSheet.SelectedIndex < 0 || _excelDataSet == null)
            {
                MessageBox.Show("Please select a sheet to import.", "No Sheet Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Warn user if there's existing data
            if (_importedRecords.Count > 0)
            {
                var confirmResult = MessageBox.Show(
                    $"You have {_importedRecords.Count} records already loaded.\n\n" +
                    "Importing this sheet will clear all current data and progress.\n\n" +
                    "Do you want to continue?",
                    "Confirm New Import",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmResult != DialogResult.Yes)
                    return;

                // Clear existing data
                ClearAllData();
            }

            string? sheetName = cbSelectSheet.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                MessageBox.Show("Please select a valid sheet to import.", "No Sheet Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _selectedSheet = ExcelImportService.GetSheetByName(_excelDataSet, sheetName);
            string error = string.Empty;
            if (_selectedSheet == null || !ExcelImportService.ValidateDataTable(_selectedSheet, out error))
            {
                MessageBox.Show($"Invalid sheet data: {error}", "Invalid Data",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            lblStatusBar1.Text = $"Status: Sheet '{sheetName}' loaded ({_selectedSheet.Rows.Count} rows)";
            lblStatusBar1.ForeColor = Color.Green;

            DisableStep1();
            EnableStep2();
            UpdateStatusBar($"Sheet '{sheetName}' loaded successfully.");
        }

        /// <summary>
        /// Clears all imported data and resets the form state
        /// </summary>
        private void ClearAllData()
        {
            _importedRecords.Clear();
            _validationErrors.Clear();
            _deletedRowIndices.Clear();
            _editedRowIndices.Clear();
            _isValidated = false;
            _isDeduplicationDone = false;
            _deduplicationResult = null;
            _fieldMappings.Clear();

            dgvRecords.DataSource = null;
            dgvRecords.Rows.Clear();
            dgvMapping.Rows.Clear();

            // Reset UI
            lblValidationStatus.Text = "Not Validated";
            lblValidationStatus.ForeColor = Color.Gray;
            lblRecordsReady.Text = "N/A";
            lblTotalRecords.Text = "0";

            // Reset steps
            DisableStep2();
            DisableStep3();
            DisableStep4();

            // Reset step status indicators
            InitializeStepStatusIndicators();

            UpdateStatusBar("Data cleared. Ready for new import.");
        }

        // ==================== STEP 2: MAP FIELDS ====================
        private void EnableStep2()
        {
            SCImportManager.Panel2Collapsed = false;
            grpStep2.Enabled = true;
            PopulateFieldMappings();
            btnAutoMap.Focus();
            btnAutoMap.BackColor = Color.DodgerBlue;
        }

        private void DisableStep2()
        {
            grpStep2.Enabled = false;
        }

        private void PopulateFieldMappings()
        {
            if (_selectedSheet == null) return;

            List<string> excelColumns = _selectedSheet.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            excelColumns.Insert(0, "-- Not Mapped --");

            dgvMapping.Rows.Clear();
            dgvMapping.Columns.Clear();

            // Target Field column (read-only)
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TargetField",
                HeaderText = "Target Field",
                ReadOnly = true,
                Width = 200,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0) }
            });

            // Source Field column (ComboBox)
            var sourceColumn = new DataGridViewComboBoxColumn
            {
                Name = "SourceField",
                HeaderText = "Source Field",
                DataSource = excelColumns.ToList(),
                Width = 300,
                DefaultCellStyle = { Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0) }
            };
            dgvMapping.Columns.Add(sourceColumn);

            // Add rows for each model property
            foreach (var prop in typeof(BaselineLandParceRecord).GetProperties())
            {
                int rowIndex = dgvMapping.Rows.Add();
                dgvMapping.Rows[rowIndex].Cells["TargetField"].Value = prop.Name;
                dgvMapping.Rows[rowIndex].Cells["SourceField"].Value = "-- Not Mapped --";
            }

        }

        private void dgvMapping_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        }

        private void btnAutoMap_Click(object sender, EventArgs e)
        {
            if (_selectedSheet == null) return;

            AutoMapFields();
            MessageBox.Show("Auto-mapping completed. Please review the mappings.",
                "Auto-Map", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateStatusBar("Auto-mapping Completed. Please review the mappings.");

            btnApplyMapping.Focus();
        }

        private void AutoMapFields()
        {

            var mappingRules = new Dictionary<string[], string>
            {
                // Parcel / Plot Number
                { new[] {
                    "parcel", "plot", "kitta", "kittano", "kita",
                    "कित्ता", "कित्ता नं", "कित्ता नम्बर", "कित्ता न.", "कि.नं", "कि.न."
                }, "ParcelNo" },

                // Province
                { new[] {
                    "province", "pradesh",
                    "प्रदेश", "प्रदेश नं", "प्रदेश नम्बर"
                }, "Province" },

                // District
                { new[] {
                    "district", "jilla", "zilla",
                    "जिल्ला", "जि."
                }, "District" },

                // Municipality / Village / Local Government
                { new[] {
                    "municipality", "village", "gapa", "napa", "gaupalika", "nagarpalika", "local",
                    "गा.पा", "न.पा", "गा.पा.", "न.पा.", "गाउँपालिका", "नगरपालिका", "पालिका",
                    "स्थानीय तह", "गाविस", "गा.वि.स.", "नगर","गाउँ"
                }, "MunicipalityVillage" },

                // Ward Number
                { new[] {
                    "ward", "wada", "wardno", "ward no", "ward number",
                    "वडा", "वडा नं", "वडा न.", "वडा नम्बर"
                }, "WardNo" },

                // Parcel Location / Tole
                { new[] {
                    "location", "parcel location", "tole", "place", "tol"
                                    }, "ParcelLocation" },

                // Map Sheet Number
                { new[] {
                    "map", "sheet", "mapsheet", "naksa", "naksha", "sheetno",
                    "नक्सा", "नक्सा नं", "नक्सा नम्बर", "शीट", "शीट नं", "न.नं"
                }, "MapSheetNo" },

                // Land Owner Name
                { new[] {
                    "owner", "name", "ownername", "landowner", "malik", "dhani", "jaggadhani",
                    "मालिक", "जग्गाधनी", "धनी", "नाम", "जग्गावालाको नाम", "धनीको नाम"
                }, "LandOwnersName" },

                // Father / Spouse Name
                { new[] {
                    "father", "spouse", "husband", "wife", "guardian", "baba", "buwa", "pita",
                    "बाबु", "बुबा", "पिता", "पति", "पत्नी", "श्रीमान", "श्रीमती",
                    "बाबु/पति", "बाबुको नाम", "पिताको नाम", "पति/पत्नी"
                }, "FatherSpouse" },

                // Gender
                { new[] {
                    "gender", "sex", "ling",
                    "लिङ्ग", "लिंग", "पुरुष/महिला"
                }, "Gender" },

                // Citizenship Number
                { new[] {
                    "citizenship", "citizenshipno", "citizenshipnumber", "nagarikta", "naprano",
                    "नागरिकता", "ना.प्र.नं", "ना.प्र.न", "ना.प्र.", "नागरिकता नं", "नागरिकता नम्बर",
                    "ना. प्र. पत्र नं", "न.प्र.नं."
                }, "CitizenshipNumber" },

                // Citizenship Issued District
                { new[] {
                    "issued district", "citizenship district", "issue district", "issueddistrict",
                    "जारी जिल्ला","जारी स्थान", "स्थान", "ना.प्र. जिल्ला", "जारी जिल्ला", "नागरिकता जिल्ला", "जारी", "जारि"
                }, "CitizenshipIssuedDistrict" },

                // Citizenship Issued Date
                { new[] {
                    "issued date", "citizenship date", "issue date", "issueddate",
                    "जारी मिति", "ना.प्र. मिति", "जारी गरेको मिति", "नागरिकता मिति","जारि मिति", "जारी मीति", "जारी मिती"
                }, "citizenshipIssuedDate" },

                // Contact Info
                { new[] {
                    "contact", "phone", "email", "mobile",
                    "सम्पर्क न.", "फोन", "मोबाईल", "मोबाइल","ई-मेल", "ई-मेल", "मेल"
                }, "ContactNumber" },

                // Tenant / Mohi
                { new[] {
                    "tenant", "mohi", "kisaan", "kisan",
                    "मोही", "किसान", "बटाईदार", "मोही/किसान"
                }, "Tenant" },

                // Permanent Address
                { new[] {
                    "permanent", "permanentaddress", "permanent address", "address", "thegana", "basabas",
                    "ठेगाना", "स्थायी ठेगाना", "बसोबास", "स्थायी बसोबास", "घर ठेगाना"
                }, "PermanentAddress" },

                // Land Use
                { new[] {
                    "use", "landuse", "land use", "upayog", "prayog",
                    "प्रयोग", "उपयोग", "जग्गा प्रयोग", "जग्गाको किसिम"
                }, "LandUse" },

                
                // Land Ownership Type

                { new[] {
                    "ownership", "type", "ownershiptype",
                    "स्वामित्व", "प्रकार", "स्वामित्व प्रकार"
                }, "LandOwnershipType" },

                // Area (Square Meter)
                { new[] {
                    "sqm", "square", "squaremeter", "sqmeter", "meter", "vargameter",
                    "क्षेत्रफल", "वर्गमिटर", "वर्ग मि", "वर्ग मिटर", "व.मि."
                }, "AreaInSqm" },

                // Area (RAPD - Ropani/Aana/Paisa/Dam)
                { new[] {
                    "rapd", "ropani", "aana", "paisa", "dam",
                    "रोपनी", "रो.आ.पै.दा.", "रो-आ-पै-दा", "१-२-३-४"
                }, "AreaInRAPD" },

                // Area (BKD - Bigha/Kattha/Dhur)
                { new[] {
                    "bkd", "bigha", "kattha", "katha", "dhur",
                    "बिघा", "कट्ठा", "धुर", "बि.क.धु.", "बि-क-धु","क्षेत्रफल(१-२-३)"
                }, "AreaInBKD" },

                // Moth Number
                { new[] {
                    "moth", "mothno", "moth no", "mothnumber",
                    "मोठ", "मोठ नं", "मोठ नम्बर", "मोठ न."
                }, "MothNo" },

                // Paana Number  
                { new[] {
                    "paana", "pana", "paanano", "pananumber",
                    "पाना", "पाना नं", "पाना नम्बर", "पाना न."
                }, "PaanaNo" },

                // Remarks
                { new[] {
                    "remark", "remarks", "note", "notes", "kaifiyat", "tippani",
                    "कैफियत", "टिप्पणी", "कैफियत/टिप्पणी", "नोट"
                }, "Remarks" }
            };


            var excelColumns = _selectedSheet?.Columns.Cast<DataColumn>().ToList();

            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                string targetField = row.Cells["TargetField"].Value?.ToString() ?? "";

                foreach (var rule in mappingRules)
                {
                    if (rule.Value == targetField)
                    {
                        foreach (var col in excelColumns)
                        {
                            string colName = col.ColumnName.ToLower().Trim();
                            if (rule.Key.Any(keyword => colName.Contains(keyword.ToLower())))
                            {
                                row.Cells["SourceField"].Value = col.ColumnName;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void btnClearMapping_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                row.Cells["SourceField"].Value = "-- Not Mapped --";
            }
        }

        private void btnApplyMapping_Click(object sender, EventArgs e)
        {
            _fieldMappings = BuildFieldMappings();

            if (!ValidateRequiredMappings(out List<string> missingFields))
            {
                MessageBox.Show(
                    $"The following required fields must be mapped:\n\n{string.Join("\n", missingFields)}",
                    "Required Fields Missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            Cursor = Cursors.WaitCursor;
            progressBar.Style = ProgressBarStyle.Marquee;
            UpdateStatusBar("Transforming data...");

            _backgroundWorker.RunWorkerAsync(new BackgroundOperation
            {
                OperationType = OperationType.TransformData,
                SourceData = _selectedSheet!,
                FieldMappings = _fieldMappings
            });
            SCImportManager.Panel2Collapsed = true;
        }

        private Dictionary<string, string> BuildFieldMappings()
        {
            var mappings = new Dictionary<string, string>();

            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                string targetField = row.Cells["TargetField"].Value?.ToString() ?? "";
                string sourceField = row.Cells["SourceField"].Value?.ToString() ?? "";

                if (!string.IsNullOrEmpty(targetField) &&
                    !string.IsNullOrEmpty(sourceField) &&
                    sourceField != "-- Not Mapped --")
                {
                    mappings[targetField] = sourceField;
                }
            }

            return mappings;
        }

        private bool ValidateRequiredMappings(out List<string> missingFields)
        {
            missingFields = new List<string>();

            if (!_fieldMappings.ContainsKey(nameof(BaselineLandParceRecord.ParcelNo)))
                missingFields.Add("Parcel No");

            if (!_fieldMappings.ContainsKey(nameof(BaselineLandParceRecord.MapSheetNo)))
                missingFields.Add("Map Sheet No");

            if (!_fieldMappings.ContainsKey(nameof(BaselineLandParceRecord.AreaInSqm)))
                missingFields.Add("Area (sq.m)");

            return missingFields.Count == 0;
        }

        // ==================== STEP 3: REVIEW & VALIDATE ====================
        private void EnableStep3()
        {
            grpStep3.Enabled = true;
            InitializeRecordsDataGridView();
            SetupRecordsGridColumns();
            LoadRecordsIntoGrid();
            RunInitialValidation();
        }


        private void InitializeRecordsDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.AllowUserToResizeColumns = true;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.MultiSelect = true;
            dgvRecords.ReadOnly = true;
            dgvRecords.DoubleBuffered(true);
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);
            dgvRecords.RowHeadersVisible = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.Single;
            dgvRecords.GridColor = Color.FromArgb(220, 220, 220);


            // Make headers styled
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecords.ColumnHeadersHeight = 34;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvRecords.RowTemplate.Height = 28;

            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            dgvRecords.RowPostPaint += DgvRecords_RowPostPaint;
        }

        /// <summary>
        /// Paints row numbers in the row header for each visible row.
        /// This is the most performant approach - only paints visible rows on demand.
        /// </summary>
        private void DgvRecords_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Row number is 1-based for user display
            string rowNumber = (e.RowIndex + 1).ToString();

            // Calculate bounds for the row header
            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dgvRecords.RowHeadersWidth - 4,
                e.RowBounds.Height);

            // Use TextRenderer for crisp text rendering
            TextRenderer.DrawText(
                e.Graphics,
                rowNumber,
                dgvRecords.RowHeadersDefaultCellStyle.Font ?? dgvRecords.DefaultCellStyle.Font,
                headerBounds,
                dgvRecords.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }


        private void DisableStep3()
        {
            grpStep3.Enabled = false;
        }

        private void SetupRecordsGridColumns()
        {
            dgvRecords.Columns.Clear();

            foreach (var property in typeof(BaselineLandParceRecord).GetProperties())
            {
                dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
                {
                    Name = property.Name,
                    HeaderText = FormatHeaderText(property.Name),
                    DataPropertyName = property.Name,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    SortMode = DataGridViewColumnSortMode.NotSortable
                });
            }
        }

        private static string FormatHeaderText(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(propertyName.Length * 2);
            builder.Append(propertyName[0]);
            for (int i = 1; i < propertyName.Length; i++)
            {
                char current = propertyName[i];
                char previous = propertyName[i - 1];
                if (current == '_')
                {
                    builder.Append(' ');
                    continue;
                }

                bool isBoundary = char.IsUpper(current) && !char.IsUpper(previous) && previous != ' ';
                bool isDigitBoundary = char.IsDigit(current) && !char.IsDigit(previous) && previous != ' ';
                if (isBoundary || isDigitBoundary)
                {
                    builder.Append(' ');
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private void AddTextColumn(string propertyName, string headerText, int width)
        {
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = propertyName,
                HeaderText = headerText,
                DataPropertyName = propertyName,
                Width = width
            });
        }

        private void LoadRecordsIntoGrid()
        {
            dgvRecords.SuspendLayout();
            try
            {
                dgvRecords.DataSource = _importedRecords;
                UpdateRecordCount();

                // Clear selection after loading
                dgvRecords.ClearSelection();
            }
            finally
            {
                dgvRecords.ResumeLayout();
            }
        }

        private void RunInitialValidation()
        {
            Cursor = Cursors.WaitCursor;
            progressBar.Style = ProgressBarStyle.Marquee;
            UpdateStatusBar("Validating records...");

            _backgroundWorker.RunWorkerAsync(new BackgroundOperation
            {
                OperationType = OperationType.ValidateData,
                Records = _importedRecords.ToList()
            });
        }

        // ==================== CONTEXT MENU ====================
        private void ContextMenu_Opening(object sender, CancelEventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 0)
            {
                e.Cancel = true;
                return;
            }

            int rowIndex = dgvRecords.SelectedRows[0].Index;
            bool isInvalid = _validationErrors.Any(err => err.RowNumber - 1 == rowIndex);

            // Edit - only enabled for single selection
            _contextMenu.Items[0].Enabled = dgvRecords.SelectedRows.Count == 1;

            // Delete - always enabled
            _contextMenu.Items[1].Enabled = true;

            // Fix Error - only visible if row is invalid
            _contextMenu.Items[2].Visible = isInvalid;
        }

        private void ContextMenu_Edit_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1) return;

            int rowIndex = dgvRecords.SelectedRows[0].Index;
            EditRecord(rowIndex);
        }

        private void ContextMenu_Delete_Click(object sender, EventArgs e)
        {
            DeleteSelectedRecords();
        }

        private void ContextMenu_FixError_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1) return;

            int rowIndex = dgvRecords.SelectedRows[0].Index;
            EditRecord(rowIndex);
        }

        private void EditRecord(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _importedRecords.Count) return;

            var record = _importedRecords[rowIndex];

            // Duplicate parcel check delegate: checks if a parcel already exists in the binding list
            Func<string?, string?, int?, bool> parcelExists = (parcelNo, mapSheetNo, excludeIndex) =>
            {
                for (int i = 0; i < _importedRecords.Count; i++)
                {
                    if (i == excludeIndex) continue;
                    var r = _importedRecords[i];
                    if (string.Equals(r.ParcelNo?.Trim(), parcelNo?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(r.MapSheetNo?.Trim(), mapSheetNo?.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            };

            using (var editForm = new frmAddEditRecord(record, rowIndex, parcelExists, _importedRecords.ToList()))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    if (editForm.IsDeleted)
                    {
                        _importedRecords.RemoveAt(rowIndex);
                        _deletedRowIndices.Add(rowIndex);
                        RemoveErrorForRow(rowIndex);
                    }
                    else
                    {
                        _importedRecords[rowIndex] = editForm.Record;
                        _editedRowIndices.Add(rowIndex);
                    }

                    dgvRecords.Refresh();
                    UpdateRecordCount();
                    UpdateValidationStatus();
                }
            }
        }

        private void DeleteSelectedRecords()
        {
            if (dgvRecords.SelectedRows.Count == 0) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {dgvRecords.SelectedRows.Count} record(s)?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            var indicesToDelete = dgvRecords.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.Index)
                .OrderByDescending(i => i)
                .ToList();

            foreach (int index in indicesToDelete)
            {
                _deletedRowIndices.Add(index);
                _importedRecords.RemoveAt(index);
                RemoveErrorForRow(index);
            }

            dgvRecords.Refresh();
            UpdateRecordCount();
            UpdateValidationStatus();
        }

        private void RemoveErrorForRow(int rowIndex)
        {
            _validationErrors.RemoveAll(err => err.RowNumber - 1 == rowIndex);
        }

        // ==================== BUTTON HANDLERS ====================
        private void btnEditRecord_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1)
            {
                MessageBox.Show("Please select exactly one record to edit.", "Invalid Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int rowIndex = dgvRecords.SelectedRows[0].Index;
            EditRecord(rowIndex);
        }

        private void btnRemoveSelected_Click(object sender, EventArgs e)
        {
            DeleteSelectedRecords();
        }

        private void btnFixErrors_Click(object sender, EventArgs e)
        {
            if (!_validationErrors.Any())
            {
                MessageBox.Show("No errors to fix.", "No Errors",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show validation errors form
            using (var errorForm = new frmValidationErrors(_validationErrors, _importedRecords))
            {
                errorForm.ShowDialog();
            }
        }

        private void DgvRecords_SelectionChanged(object sender, EventArgs e)
        {
            // Update button states based on selection
            if (dgvRecords.SelectedRows.Count == 1)
            {
                btnEditRecord.Enabled = true;
                btnRemoveSelected.Enabled = true;
            }
            else if (dgvRecords.SelectedRows.Count > 1)
            {
                btnEditRecord.Enabled = false;
                btnRemoveSelected.Enabled = true;
            }
            else
            {
                btnEditRecord.Enabled = false;
                btnRemoveSelected.Enabled = false;
            }
        }

        // ==================== STEP 4: VALIDATE & SAVE ====================
        private void EnableStep4()
        {
            grpStep4.Enabled = true;
            UpdateValidationStatus();
        }

        private void DisableStep4()
        {
            grpStep4.Enabled = false;
        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            if (_importedRecords.Count == 0)
            {
                MessageBox.Show("No records to validate.", "No Records",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Cursor = Cursors.WaitCursor;
            progressBar.Style = ProgressBarStyle.Marquee;
            UpdateStatusBar("Validating records...");

            _backgroundWorker.RunWorkerAsync(new BackgroundOperation
            {
                OperationType = OperationType.ValidateData,
                Records = _importedRecords.ToList()
            });
        }

        private void UpdateValidationStatus()
        {
            if (!_isValidated)
            {
                lblValidationStatus.Text = "Not Validated";
                lblValidationStatus.ForeColor = Color.Gray;
                lblRecordsReady.Text = "N/A";
                btnSaveToDatabase.Enabled = false;
            }
            else
            {
                int errorCount = _validationErrors.Count;
                int validCount = _importedRecords.Count - errorCount;

                lblRecordsReady.Text = validCount.ToString();
                lblTotalRecords.Text = _importedRecords.Count.ToString();

                if (errorCount > 0)
                {
                    lblValidationStatus.Text = $"{errorCount} Error(s)";
                    lblValidationStatus.ForeColor = Color.Red;
                    btnSaveToDatabase.Enabled = false;
                    ColorCodeRows();
                }
                else
                {
                    lblValidationStatus.Text = "All Valid!";
                    lblValidationStatus.ForeColor = Color.Green;
                    // Only enable save button if deduplication is also done
                    btnSaveToDatabase.Enabled = _isDeduplicationDone;
                    ClearRowColors();
                }
            }
        }

        private void ColorCodeRows()
        {
            var invalidIndices = _validationErrors.Select(e => e.RowNumber - 1).ToHashSet();

            foreach (DataGridViewRow row in dgvRecords.Rows)
            {
                if (invalidIndices.Contains(row.Index))
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 245); // Very subtle red
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(139, 0, 0);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void ClearRowColors()
        {
            foreach (DataGridViewRow row in dgvRecords.Rows)
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
            }
        }

        private async void btnSaveToDatabase_Click(object sender, EventArgs e)
        {
            if (_validationErrors.Any())
            {
                MessageBox.Show("Please fix all validation errors before saving.",
                    "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_isDeduplicationDone)
            {
                MessageBox.Show("Please resolve owner duplication before saving.\n\n" +
                    "Click 'Resolve Owner Duplication' button to check for and merge duplicate owners.",
                    "Deduplication Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if there are existing records in the database
            int existingParcelCount = 0;
            int existingOwnerCount = 0;
            try
            {
                var counts = await GetPersistenceService().GetExistingCountsAsync();
                existingOwnerCount = counts.Owners;
                existingParcelCount = counts.Parcels;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to read existing database counts:\n{ex.Message}",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            bool replaceExisting = false;

            if (existingParcelCount > 0 || existingOwnerCount > 0)
            {
                var warningResult = MessageBox.Show(
                    $"⚠️ WARNING: Existing Data Detected!\n\n" +
                    $"The database already contains:\n" +
                    $"• {existingOwnerCount} Landowner(s)\n" +
                    $"• {existingParcelCount} Land Parcel(s)\n\n" +
                    $"How would you like to proceed?\n\n" +
                    $"• Click 'Yes' to REPLACE all existing data with new import\n" +
                    $"• Click 'No' to ADD to existing data (duplicates will be skipped)\n" +
                    $"• Click 'Cancel' to abort the import",
                    "Existing Data Warning",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (warningResult == DialogResult.Cancel)
                    return;

                replaceExisting = (warningResult == DialogResult.Yes);

                if (replaceExisting)
                {
                    var confirmReplace = MessageBox.Show(
                        $"⚠️ CONFIRM DATA REPLACEMENT\n\n" +
                        $"You are about to DELETE:\n" +
                        $"• {existingOwnerCount} Landowner(s)\n" +
                        $"• {existingParcelCount} Land Parcel(s)\n\n" +
                        $"This action CANNOT be undone!\n\n" +
                        $"Are you absolutely sure you want to proceed?",
                        "Confirm Replace",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);

                    if (confirmReplace != DialogResult.Yes)
                        return;
                }
            }

            // Show summary before saving
            int uniqueOwnerCount = _deduplicationResult?.UniqueOwners.Count ?? 0;
            var result = MessageBox.Show(
                $"Ready to save to database:\n\n" +
                $"- Unique Landowners: {uniqueOwnerCount}\n" +
                $"- Land Parcels: {_importedRecords.Count}\n\n" +
                $"Do you want to continue?",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            Cursor = Cursors.WaitCursor;
            progressBar.Style = ProgressBarStyle.Marquee;
            UpdateStatusBar("Saving to database...");

            var sourceFilePath = txtFilePath.Text;
            var sourceFileName = Path.GetFileName(sourceFilePath);
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                sourceFileName = "ManualImport.xlsx";
            }

            _backgroundWorker.RunWorkerAsync(new BackgroundOperation
            {
                OperationType = OperationType.SaveToDatabase,
                Records = _importedRecords.ToList(),
                DeduplicationResult = _deduplicationResult,
                ReplaceExistingData = replaceExisting,
                SourceFilePath = sourceFilePath,
                SourceFileName = sourceFileName,
                SheetName = cbSelectSheet.SelectedItem?.ToString()
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Cancel import? All progress will be lost.",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        // ==================== BACKGROUND WORKER ====================
        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var operation = (BackgroundOperation)e.Argument!;
            var worker = sender as BackgroundWorker;

            e.Result = operation.OperationType switch
            {
                OperationType.LoadFile => LoadFileOperation(operation.FilePath!, worker!),
                OperationType.TransformData => TransformDataOperation(operation.SourceData!, operation.FieldMappings!, worker!),
                OperationType.ValidateData => ValidateDataOperation(operation.Records!, worker!),
                OperationType.SaveToDatabase => SaveToDatabaseOperation(
                    operation.Records!,
                    operation.DeduplicationResult,
                    operation.ReplaceExistingData,
                    operation.SourceFileName,
                    operation.SourceFilePath,
                    operation.SheetName,
                    worker!),
                _ => new OperationResult { Success = false, Message = "Unknown operation" }
            };
        }

        private OperationResult LoadFileOperation(string filePath, BackgroundWorker worker)
        {
            worker.ReportProgress(10);
            var dataSet = ExcelImportService.ReadExcelFileAsDataSet(filePath);
            worker.ReportProgress(100);

            return new OperationResult
            {
                Success = dataSet != null,
                DataSet = dataSet,
                Message = dataSet != null ? "File loaded successfully." : "Failed to load file."
            };
        }

        private OperationResult TransformDataOperation(DataTable sourceData, Dictionary<string, string> fieldMappings, BackgroundWorker worker)
        {
            worker.ReportProgress(10);
            var result = DataTransformationService.TransformDataToEntities(sourceData, fieldMappings);
            worker.ReportProgress(100);

            return new OperationResult
            {
                Success = true,
                TransformResult = result,
                Message = $"Transformed {result.TotalRecords} records."
            };
        }

        private OperationResult ValidateDataOperation(List<BaselineLandParceRecord> records, BackgroundWorker worker)
        {
            worker.ReportProgress(30);
            var dt = ConvertRecordsToDataTable(records);
            var result = DataTransformationService.ValidateFromDataTable(dt);
            worker.ReportProgress(100);

            return new OperationResult
            {
                Success = true,
                TransformResult = result,
                Message = result.HasErrors
                    ? $"Validation complete: {result.InvalidRecords.Count} error(s) found."
                    : "Validation complete: All records valid!"
            };
        }

        private OperationResult SaveToDatabaseOperation(List<BaselineLandParceRecord> records,
            OwnerDeduplicationService.DeduplicationResult? deduplicationResult,
            bool replaceExisting,
            string? sourceFileName,
            string? sourceFilePath,
            string? sheetName,
            BackgroundWorker worker)
        {
            worker.ReportProgress(10);

            try
            {
                if (!AppServices.HasContext)
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = "Failed to save: no open project context found."
                    };
                }

                if (string.IsNullOrWhiteSpace(_projectPath) || !File.Exists(_projectPath))
                {
                    return new OperationResult
                    {
                        Success = false,
                        Message = $"Failed to save: project file not found at '{_projectPath}'."
                    };
                }

                var persistenceService = GetPersistenceService();
                worker.ReportProgress(25);

                if (string.IsNullOrWhiteSpace(sourceFileName))
                {
                    sourceFileName = "ManualImport.xlsx";
                }

                var persistResult = persistenceService
                    .PersistImportAsync(
                        records,
                        deduplicationResult,
                        replaceExisting,
                        sourceFileName,
                        sourceFilePath,
                        sheetName)
                    .GetAwaiter()
                    .GetResult();

                worker.ReportProgress(100);

                string resultMessage =
                    $"Successfully saved to database via EF Core.\n\n" +
                    $"- Replace Existing: {(persistResult.ReplacedExistingData ? "YES" : "NO")}\n" +
                    $"- Initial Data: {persistResult.InitialOwners} owners, {persistResult.InitialParcels} parcels\n" +
                    (persistResult.ReplacedExistingData
                        ? $"- Deleted: {persistResult.DeletedOwners} owners, {persistResult.DeletedParcels} parcels\n"
                        : string.Empty) +
                    $"- Import Session ID: {persistResult.ImportSessionId}\n" +
                    $"- Saved Owners (linked): {persistResult.SavedOwners}\n" +
                    $"- New Owners Created: {persistResult.NewOwnersCreated}\n" +
                    $"- Existing Owners Updated: {persistResult.ExistingOwnersUpdated}\n" +
                    $"- Saved Parcels: {persistResult.SavedParcels}\n" +
                    $"- Skipped Duplicate Parcels: {persistResult.SkippedDuplicateParcels}";

                return new OperationResult
                {
                    Success = true,
                    Message = resultMessage,
                    PersistenceResult = persistResult
                };
            }
            catch (Exception ex)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = $"Failed to save to database: {ex.Message}"
                };
            }
        }

        private DataTable ConvertRecordsToDataTable(List<BaselineLandParceRecord> records)
        {
            var dt = new DataTable();
            var properties = typeof(BaselineLandParceRecord).GetProperties();

            foreach (var prop in properties)
            {
                dt.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var record in records)
            {
                var row = dt.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(record) ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }

            return dt;
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;

            if (e.Error != null)
            {
                MessageBox.Show($"Operation failed: {e.Error.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = (OperationResult)e.Result!;

            if (!result.Success)
            {
                MessageBox.Show(result.Message, "Operation Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateStatusBar(result.Message);
                return;
            }

            // Handle success based on operation type
            if (result.DataSet != null)
            {
                HandleFileLoaded(result.DataSet);
            }
            else if (result.TransformResult != null && !_isValidated)
            {
                HandleDataTransformed(result.TransformResult);
            }
            else if (result.TransformResult != null)
            {
                HandleDataValidated(result.TransformResult);
            }
            else if (result.PersistenceResult != null)
            {
                HandleDataSaved(result.Message, result.PersistenceResult);
            }

            UpdateStatusBar(result.Message);
        }

        private void HandleFileLoaded(DataSet dataSet)
        {
            _excelDataSet = dataSet;
            var sheetNames = ExcelImportService.GetSheetNames(dataSet);

            cbSelectSheet.Items.Clear();
            cbSelectSheet.Items.AddRange(sheetNames.ToArray());
            cbSelectSheet.SelectedIndex = 0;
            cbSelectSheet.Enabled = true;
            btnImportData.Enabled = true;

            lblStatusBar1.Text = $"Status: File loaded with {sheetNames.Count} sheet(s)";
            lblStatusBar1.ForeColor = Color.Green;

            UpdateStepStatus(1, StepStatus.Success, $"Loaded ({sheetNames.Count} sheets)");
            UpdateStepStatus(2, StepStatus.InProgress, "Ready to map");
        }

        // ==================== DATA TRANSFORMATION HANDLER ====================
        private void HandleDataTransformed(TransformationResult result)
        {
            _importedRecords = new SortableBindingList<BaselineLandParceRecord>(result.AllOriginalRecords);

            // Auto-calculate RAPD and BKD from AreaInSqm, and auto-detect Ownership Type
            PostProcessImportedRecords();

            SortRecordsByParcelNo();
            _validationErrors = result.ValidationErrors;
            _isValidated = true;
            _isDeduplicationDone = false;

            DisableStep2();
            EnableStep3();
            EnableStep4();

            // Update step statuses
            UpdateStepStatus(2, StepStatus.Success, $"Mapped ({result.TotalRecords} records)");

            if (result.HasErrors)
            {
                UpdateStepStatus(3, StepStatus.Error, $"{result.InvalidRecords.Count} errors");
                UpdateStepStatus(4, StepStatus.Pending, "Fix errors first");
            }
            else
            {
                UpdateStepStatus(3, StepStatus.InProgress, "Review & deduplicate");
                UpdateStepStatus(4, StepStatus.Pending, "Ready when validated");
            }

            string skippedInfo = result.SkippedRows > 0
                ? $"Skipped Rows (empty/missing required fields): {result.SkippedRows}\n"
                : "";

            string message = $"Data transformation complete!\n\n" +
                            $"Total Records Imported: {result.TotalRecords}\n" +
                            $"Valid Records: {result.ValidRecords.Count}\n" +
                            $"Invalid Records: {result.InvalidRecords.Count}\n" +
                            skippedInfo + "\n" +
                            $"Next Steps:\n" +
                            $"1. Review and fix any validation errors\n" +
                            $"2. Click 'Resolve Owner Duplication' to merge duplicate owners\n" +
                            $"3. Save to database";

            MessageBox.Show(message, "Transformation Complete",
                MessageBoxButtons.OK,
                result.HasErrors ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        // ==================== POST-IMPORT PROCESSING ====================

        /// <summary>
        /// Auto-calculates RAPD and BKD from AreaInSqm and auto-detects Ownership Type
        /// for all imported records. Called once after data transformation.
        /// </summary>
        private void PostProcessImportedRecords()
        {
            foreach (var record in _importedRecords)
            {
                // Auto-calculate Area (R-A-P-D) and Area (B-K-D) from AreaInSqm
                if (record.AreaInSqm.HasValue && record.AreaInSqm.Value > 0)
                {
                    record.AreaInRAPD = AreaConverterService.SqmToRAPDString(record.AreaInSqm.Value);
                    record.AreaInBKD = AreaConverterService.SqmToBKDString(record.AreaInSqm.Value);
                }

                // Auto-detect Ownership Type if not already set
                if (string.IsNullOrWhiteSpace(record.LandOwnershipType))
                {
                    record.LandOwnershipType = DetectOwnershipType(record.LandOwnersName, null);
                }
                else
                {
                    // Even if set, try to refine based on keywords in existing value + name
                    string detected = DetectOwnershipType(record.LandOwnersName, record.LandOwnershipType);
                    if (!string.IsNullOrWhiteSpace(detected))
                    {
                        record.LandOwnershipType = detected;
                    }
                }
            }
        }

        /// <summary>
        /// Detects ownership type from owner name and/or existing ownership type field.
        /// Priority: Guthi > Public > Joint > Single (default).
        /// Checks Devanagari and English keywords.
        /// </summary>
        private static string DetectOwnershipType(string? ownerName, string? existingType)
        {
            string nameLower = (ownerName ?? "").Trim().ToLower();
            string typeLower = (existingType ?? "").Trim().ToLower();
            string combined = $"{typeLower} {nameLower}";

            // Check for Guthi (Trust) keywords
            string[] guthiKeywords = ["guthi", "गुठी", "गुठि"];
            if (guthiKeywords.Any(k => combined.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "Trust (Guthi)";

            // Check for Public (Government) keywords
            string[] publicKeywords =
            [
                "public", "government", "govt", "sarkar",
                "नेपाल सरकार", "सरकार", "सरकारी",
                "सार्वजनिक", "सार्वाजनिक", "सार्वाजिनिक",
                "नगरपालिका", "गाउँपालिका", "गाउपालिका", "गा.पा", "न.पा",
                "मन्त्रालय", "विभाग", "कार्यालय"
            ];
            if (publicKeywords.Any(k => combined.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "Public (Government)";

            // Check for Joint keywords in ownership type field
            string[] jointKeywords = ["joint", "संयुक्त", "sanyukta"];
            if (jointKeywords.Any(k => combined.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "Private (Joint)";

            // Check if owner name has multiple names separated by delimiters (indicates Joint ownership)
            if (!string.IsNullOrWhiteSpace(ownerName) && HasMultipleOwnerNames(ownerName))
                return "Private (Joint)";

            // Check for Single keywords
            string[] singleKeywords = ["niji", "single", "व्यक्ति", "निजि", "निजी", "व्यक्तिगत"];
            if (singleKeywords.Any(k => combined.Contains(k, StringComparison.OrdinalIgnoreCase)))
                return "Private (Single)";

            // Default: if owner name exists and no special keywords, assume Private (Single)
            if (!string.IsNullOrWhiteSpace(ownerName))
                return "Private (Single)";

            return "";
        }

        /// <summary>
        /// Checks if the owner name contains multiple names separated by common delimiters:
        /// comma, slash (/), "र" (Nepali "and"), "एवम्", "तथा", "and", ampersand (&amp;)
        /// </summary>
        private static bool HasMultipleOwnerNames(string ownerName)
        {
            string trimmed = ownerName.Trim();

            // Split by common delimiters
            string[] delimiters = [",", "/", " र ", " एवम् ", " तथा ", " and ", "&"];
            foreach (string delimiter in delimiters)
            {
                string[] parts = trimmed.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                // Each part must have at least 2 characters to be considered a real name
                int realNameCount = parts.Count(p => p.Trim().Length >= 2);
                if (realNameCount >= 2)
                    return true;
            }

            return false;
        }

        private void HandleDataValidated(TransformationResult result)
        {
            _validationErrors = result.ValidationErrors;
            _isValidated = true;
            UpdateValidationStatus();

            // Update step statuses
            if (result.HasErrors)
            {
                UpdateStepStatus(3, StepStatus.Error, $"{result.InvalidRecords.Count} errors");
                UpdateStepStatus(4, StepStatus.Pending, "Fix errors first");
            }
            else
            {
                UpdateStepStatus(3, StepStatus.Success, $"Valid ({result.ValidRecords.Count})");
                UpdateStepStatus(4, StepStatus.InProgress, "Ready to save");
            }

            MessageBox.Show(
                $"Validation complete!\n\n" +
                $"Valid: {result.ValidRecords.Count}\n" +
                $"Invalid: {result.InvalidRecords.Count}\n" +
                $"Total: {result.TotalRecords}",
                "Validation Complete",
                MessageBoxButtons.OK,
                result.HasErrors ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void HandleDataSaved(string message, ImportPersistenceResult saveResult)
        {
            UpdateStepStatus(4, StepStatus.Success, "Saved!");
            if (AppServices.HasContext)
            {
                AppServices.Context.MarkAsModified();
            }

            MessageBox.Show(message, "Save Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void UpdateRecordCount()
        {
            lblTotalRecords.Text = _importedRecords.Count.ToString();
        }

        private void UpdateStatusBar(string message)
        {
            lblStatusBar.ForeColor = Color.Green;
            lblStatusBar.Text = $"Status: {message}";
        }

        private void frmImportManager_Load(object sender, EventArgs e)
        {
        }

        // ==================== RE-MAP FIELDS HANDLER ====================
        private void btnReMapFields_Click(object sender, EventArgs e)
        {
            if (_selectedSheet == null)
            {
                MessageBox.Show("No data loaded. Please load an Excel file first.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                "Re-mapping fields will clear the current records and allow you to map fields again.\n\n" +
                "Do you want to continue?",
                "Confirm Re-Map",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Clear current data
            _importedRecords.Clear();
            _validationErrors.Clear();
            _isValidated = false;
            _isDeduplicationDone = false;
            _deduplicationResult = null;
            dgvRecords.DataSource = null;
            dgvRecords.Rows.Clear();

            // Re-enable Step 2 and disable Step 3/4
            EnableStep2();
            DisableStep3();
            DisableStep4();

            UpdateStatusBar("Ready to re-map fields. Please review and apply the field mappings.");
        }

        // ==================== OWNER DEDUPLICATION HANDLER ====================
        private void btnResolveOwnerDuplication_Click(object sender, EventArgs e)
        {
            if (_importedRecords.Count == 0)
            {
                MessageBox.Show("No records to process. Please import data first.",
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_validationErrors.Any())
            {
                var result = MessageBox.Show(
                    $"There are {_validationErrors.Count} validation error(s) in the data.\n\n" +
                    "It is recommended to fix all errors before resolving duplicates.\n\n" +
                    "Do you want to continue anyway?",
                    "Validation Errors Present",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return;
            }

            // No warning needed when re-running - just run fresh analysis
            RunOwnerDeduplication();
            UpdateValidationStatus();
        }

        private void RunOwnerDeduplication()
        {
            Cursor = Cursors.WaitCursor;
            UpdateStatusBar("Analyzing records for duplicate owners...");
            progressBar.Style = ProgressBarStyle.Marquee;

            try
            {
                // STEP 1: Extract unique owners with confidence-based matching on current data
                // On subsequent runs (when _isDeduplicationDone is true), exclude anonymous owners from matching
                bool excludeAnonymous = _isDeduplicationDone;
                _deduplicationResult = OwnerDeduplicationService.ExtractUniqueOwners(_importedRecords.ToList(), excludeAnonymous);

                // STEP 2: Count actual duplicates
                int autoMergedCount = _deduplicationResult.AutoMergedCount;
                int reviewNeededCount = _deduplicationResult.DuplicatesNeedingReview
                    .Count(g => !g.IsAutoMerged && g.Owners.Count > 1);
                int totalDuplicateGroups = autoMergedCount + reviewNeededCount;

                // STEP 3: Check if any duplicates were found
                if (totalDuplicateGroups == 0)
                {
                    // No duplicates found at all
                    string noDupMessage = _isDeduplicationDone
                        ? "Re-analysis complete!\n\n" +
                          $"✅ No remaining duplicates found.\n" +
                          $"🔍 Unique Landowners: {_deduplicationResult.UniqueOwners.Count}\n" +
                          $"📌 Note: Anonymous/Unknown owners were excluded from this analysis.\n\n" +
                          "All records are properly deduplicated."
                        : "Deduplication Analysis Complete!\n\n" +
                          $"✅ No duplicates found in the data.\n" +
                          $"🔍 Unique Landowners: {_deduplicationResult.UniqueOwners.Count}\n" +
                          $"👤 Anonymous Owners: {_deduplicationResult.AnonymousOwnersCreated}\n\n" +
                          "No action needed.";

                    MessageBox.Show(noDupMessage, "No Duplicates Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _isDeduplicationDone = true;
                    UpdateStepStatus(3, StepStatus.Success, "No duplicates");
                    UpdateStepStatus(4, StepStatus.InProgress, "Ready to save");
                    UpdateStatusBar("No duplicates found - records are clean.");
                    return;
                }

                // STEP 4: Show comprehensive results
                string message = $"Owner Deduplication Analysis Complete!\n\n" +
                                $"═══════════════════════════\n" +
                                $"🔍 Unique Landowners Found: {_deduplicationResult.UniqueOwners.Count}\n" +
                                $"👤 Anonymous(Unknown) Owners: {_deduplicationResult.AnonymousOwnersCreated}\n" +
                                $"🔗 Auto-Merged (High Confidence): {autoMergedCount}\n" +
                                $"⚠ Needs Review (Medium Confidence): {reviewNeededCount}";
                
                // Add note if this is a re-run
                if (excludeAnonymous)
                {
                    message += $"\n\n📌 Note: Anonymous/Unknown owners were excluded from this analysis.";
                }

                message += "\n\nClick OK to open duplicate review.";
                MessageBox.Show(message, "Deduplication Results",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Always allow manual review, including auto-merged groups.
                using (var reviewForm = new frmReviewDuplicates(_deduplicationResult, _importedRecords))
                {
                    if (reviewForm.ShowDialog() == DialogResult.OK)
                    {
                        if (reviewForm.ChangesWereMade)
                        {
                            _isDeduplicationDone = true;
                            RefreshDataGridView();

                            UpdateStepStatus(3, StepStatus.Success, "Deduplicated");
                            UpdateStepStatus(4, StepStatus.InProgress, "Ready to save");

                            MessageBox.Show(
                                "Duplicate resolution complete!\n\n" +
                                "Owner data has been normalized and deduplication decisions have been applied.\n\n" +
                                "You can:\n" +
                                "• Edit records if needed\n" +
                                "• Run 'Resolve Owner Duplication' again to re-check duplicates\n" +
                                "• Save the data to the database when satisfied",
                                "Resolution Complete",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            UpdateStatusBar("Owner deduplication review completed successfully.");
                        }
                        else
                        {
                            UpdateStatusBar("Duplicate review completed.");
                        }
                    }
                    else
                    {
                        UpdateStatusBar("Duplicate review cancelled.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred during owner deduplication:\n\n{ex.Message}",
                    "Deduplication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                UpdateStepStatus(3, StepStatus.Error, "Dedup failed");
                UpdateStatusBar("Owner deduplication failed.");
            }
            finally
            {
                Cursor = Cursors.Default;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 0;
            }
        }

        /// <summary>
        /// Applies deduplication results to records and refreshes the UI
        /// Used only when auto-merge happens without user review
        /// </summary>
        private void ApplyDeduplicationAndRefresh()
        {
            if (_deduplicationResult == null) return;

            // Apply changes to the actual records in _importedRecords
            // Since ToList() returns references to the same objects, changes are reflected
            OwnerDeduplicationService.ApplyDeduplicationToRecords(_importedRecords.ToList(), _deduplicationResult);

            _isDeduplicationDone = true;

            RefreshDataGridView();

            // Update step statuses
            UpdateStepStatus(3, StepStatus.Success, "Deduplicated");
            UpdateStepStatus(4, StepStatus.InProgress, "Ready to save");
        }

        /// <summary>
        /// Refreshes the DataGridView to show updated record data
        /// </summary>
        private void RefreshDataGridView()
        {
            // Force DataGridView to refresh by resetting the data source
            dgvRecords.SuspendLayout();
            try
            {
                // Store current position
                int currentRow = dgvRecords.CurrentRow?.Index ?? 0;

                SortRecordsByParcelNo();

                // Rebind to force refresh
                dgvRecords.DataSource = null;
                dgvRecords.DataSource = _importedRecords;

                // Restore position if valid
                if (currentRow >= 0 && currentRow < dgvRecords.Rows.Count)
                {
                    dgvRecords.CurrentCell = dgvRecords.Rows[currentRow].Cells[0];
                }
            }
            finally
            {
                dgvRecords.ResumeLayout();
            }

            UpdateRecordCount();
        }

        private void SortRecordsByParcelNo()
        {
            var sortedRecords = _importedRecords
                .OrderBy(record => TryParseParcelNo(record.ParcelNo, out var parcelNo) ? parcelNo : int.MaxValue)
                .ThenBy(record => record.ParcelNo)
                .ToList();

            _importedRecords.Clear();
            foreach (var record in sortedRecords)
            {
                _importedRecords.Add(record);
            }
        }

        private static bool TryParseParcelNo(string? value, out int parcelNo)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                parcelNo = 0;
                return false;
            }

            return int.TryParse(value.Trim(), out parcelNo);
        }

        private void SCImportManager_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dgvRecords_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

    // ==================== HELPER CLASSES ====================
    internal enum OperationType
    {
        LoadFile,
        TransformData,
        ValidateData,
        SaveToDatabase
    }

    internal class BackgroundOperation
    {
        public OperationType OperationType { get; set; }
        public string? FilePath { get; set; }
        public DataTable? SourceData { get; set; }
        public Dictionary<string, string>? FieldMappings { get; set; }
        public List<BaselineLandParceRecord>? Records { get; set; }
        public OwnerDeduplicationService.DeduplicationResult? DeduplicationResult { get; set; }
        public bool ReplaceExistingData { get; set; }
        public string? SourceFileName { get; set; }
        public string? SourceFilePath { get; set; }
        public string? SheetName { get; set; }
    }

    internal class OperationResult
    {
        public bool Success { get; set; }
        public DataSet? DataSet { get; set; }
        public TransformationResult? TransformResult { get; set; }
        public ImportPersistenceResult? PersistenceResult { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public static class DataGridViewExtensions
    {
        public static void DoubleBuffered(this DataGridView dgv, bool setting)
        {
            var property = dgv.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            property?.SetValue(dgv, setting, null);
        }
    }

    internal class SortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private ListSortDirection _sortDirection;
        private PropertyDescriptor? _sortProperty;

        public SortableBindingList() : base()
        {
        }

        public SortableBindingList(IList<T> list) : base(list)
        {
        }

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => _isSorted;
        protected override PropertyDescriptor? SortPropertyCore => _sortProperty;
        protected override ListSortDirection SortDirectionCore => _sortDirection;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (prop == null)
            {
                return;
            }

            var items = Items as List<T>;
            if (items == null)
            {
                return;
            }

            var sorted = direction == ListSortDirection.Ascending
                ? items.OrderBy(item => prop.GetValue(item)).ToList()
                : items.OrderByDescending(item => prop.GetValue(item)).ToList();

            items.Clear();
            foreach (var item in sorted)
            {
                items.Add(item);
            }

            _sortProperty = prop;
            _sortDirection = direction;
            _isSorted = true;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortProperty = null;
        }
    }
}
