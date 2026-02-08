using Land_Readjustment_Tool.Forms.Land_Owners_Record;
using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;
using System.Data.SQLite;

namespace Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment
{
    /// <summary>
    /// Form for viewing, filtering, and managing land parcel records with multi-layer filtering
    /// </summary>
    public partial class frmLandParcelOwnersRecord : Form
    {
        #region Fields

        private readonly string _projectPath;
        private SQLiteConnection _connection = null!;
        private LandOwnerRepository _repository = null!;
        private List<OriginalLandParcel> _allRecords = [];
        private List<OriginalLandParcel> _filteredRecords = [];
        private BindingList<LandParcelDisplayModel> _displayedRecords = [];

        // Filter unique values cache
        private HashSet<string> _uniqueProvinces = [];
        private HashSet<string> _uniqueDistricts = [];
        private HashSet<string> _uniqueMunicipalities = [];
        private HashSet<string> _uniqueWards = [];
        private HashSet<string> _uniqueMapSheets = [];
        private HashSet<string> _uniqueOwnershipTypes = [];

        private double _maxAreaSqm;

        #endregion

        #region Constructor

        public frmLandParcelOwnersRecord()
        {
            InitializeComponent();
            _projectPath = CurrentProject.Info.ProjectPath;
            Text = "Original Land Parcel Records";

            InitializeRepository();
            SetupEventHandlers();
            SetupInputValidation();
            InitializeDataGridView();
        }

        #endregion

        #region Initialization

        private void InitializeRepository()
        {
            var dbHelper = new DatabaseHelper(_projectPath);
            dbHelper.InitializeDatabase();
            _connection = dbHelper.GetConnection();

            if (!DatabaseSchema.HasCorrectSchema(_connection))
            {
                DatabaseSchema.RecreateSchema(_connection);
            }
            else
            {
                DatabaseSchema.CreateSchema(_connection);
            }

            _repository = new LandOwnerRepository(_connection);
        }

        private void SetupEventHandlers()
        {
            // Filter buttons
            btnApplyFilter.Click += BtnApplyFilter_Click;
            btnClearFilter.Click += BtnClearFilter_Click;

            // Search buttons
            btnApplySearch.Click += BtnApplySearch_Click;
            btnClearSearch.Click += BtnClearSearch_Click;

            // Quick filter/search toggles
            chkToggleQuickFilter.CheckedChanged += ChkToggleQuickFilter_CheckedChanged;
            chkToggleQuickSearch.CheckedChanged += ChkToggleQuickSearch_CheckedChanged;

            // ComboBox changes for quick filter
            cbProvince.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbDistrict.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbMunicipalityVillage.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbWardNo.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbMapSheet.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbLandOwnership.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;

            // Radio button changes
            rbSqm.CheckedChanged += RadioButton_CheckedChanged;
            rbRopanee.CheckedChanged += RadioButton_CheckedChanged;
            rbAana.CheckedChanged += RadioButton_CheckedChanged;

            // Text changes for quick search
            txtParcelNo.TextChanged += TxtSearch_TextChanged;
            txtLandOwner.TextChanged += TxtSearch_TextChanged;
            txtFromArea.TextChanged += TxtArea_TextChanged;
            txtToArea.TextChanged += TxtArea_TextChanged;

            // CRUD buttons
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            toolStripDropDownButton1.Click += BtnRefresh_Click;
            toolStripButton1.Click += BtnViewLandOwnerDetails_Click;

            // DataGridView events
            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            dgvRecords.CellDoubleClick += DgvRecords_CellDoubleClick;
            dgvRecords.RowPostPaint += DgvRecords_RowPostPaint;
        }

        private void SetupInputValidation()
        {
            // Area textboxes: only allow digits and decimal point
            txtFromArea.KeyPress += AreaTextBox_KeyPress;
            txtToArea.KeyPress += AreaTextBox_KeyPress;

            // Parcel number: only allow positive integers
            txtParcelNo.KeyPress += ParcelNo_KeyPress;

            // Land owner name: allow Unicode including Devanagari
            // Enable IME for Devanagari/Nepali input
            txtLandOwner.ImeMode = ImeMode.On;
            txtLandOwner.KeyPress += LandOwnerName_KeyPress;

            // Set default radio button
            rbSqm.Checked = true;
        }

        private void InitializeDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.MultiSelect = false; // Single selection only for View Land Owner Details
            dgvRecords.ReadOnly = true;
            dgvRecords.DoubleBuffered(true);
            dgvRecords.RowHeadersVisible = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.GridColor = Color.FromArgb(220, 220, 220);

            dgvRecords.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvRecords.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRecords.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            dgvRecords.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;

            dgvRecords.Columns.Clear();

            AddColumn("ParcelNo", "Parcel No", 80);
            AddColumn("MapSheetNo", "Map Sheet", 85);
            AddColumn("ParcelLocation", "Parcel Location", 130);
            AddColumn("Province", "Province", 80);
            AddColumn("District", "District", 80);
            AddColumn("MunicipalityVillage", "Municipality", 100);
            AddColumn("WardNo", "Ward", 55);
            AddColumn("LandOwnersName", "Owner Name", 150);
            AddColumn("FatherSpouse", "Father/Spouse", 130);
            AddColumn("Gender", "Gender", 60);
            AddColumn("CitizenshipNumber", "Citizenship No", 110);
            AddColumn("PermanentAddress", "Permanent Address", 140);
            AddColumn("AreaInSqm", "Area (sqm)", 85);
            AddColumn("AreaInRAPD", "Area (RAPD)", 85);
            AddColumn("AreaInBKD", "Area (BKD)", 85);
            AddColumn("LandOwnershipType", "Ownership", 80);
            AddColumn("LandUse", "Land Use", 80);

            AddColumn("IsTenant", "Tenant", 55);
            AddColumn("MothNo", "Moth No", 65);
            AddColumn("PaanaNo", "Paana No", 65);
            AddColumn("Remarks", "Remarks", 100);

            foreach (DataGridViewColumn col in dgvRecords.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecords.ColumnHeadersHeight = 34;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvRecords.RowTemplate.Height = 28;
        }

        private void AddColumn(string dataPropertyName, string headerText, int width)
        {
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = dataPropertyName,
                HeaderText = headerText,
                DataPropertyName = dataPropertyName,
                Width = width
            });
        }

        #endregion

        #region Data Loading

        private void frmLandParcelOwnersRecord_Load(object sender, EventArgs e)
        {
            LoadAllRecords();
            PopulateFilterDropdowns();
            UpdateButtonStates();
        }

        private void LoadAllRecords()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _allRecords = _repository.GetAllParcelsWithOwners();
                _filteredRecords = [.. _allRecords];

                CacheUniqueValues();
                BindRecordsToGrid(_filteredRecords);
                UpdateStatusLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allRecords = [];
                _filteredRecords = [];
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CacheUniqueValues()
        {
            _uniqueProvinces = [.. _allRecords.Select(r => r.Province ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueDistricts = [.. _allRecords.Select(r => r.District ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueMunicipalities = [.. _allRecords.Select(r => r.MunicipalityVillage ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueWards = [.. _allRecords.Select(r => r.WardNo ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueMapSheets = [.. _allRecords.Select(r => r.MapSheetNo ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueOwnershipTypes = [.. _allRecords.Select(r => r.LandOwnershipType ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];

            // Calculate max area for filtering
            _maxAreaSqm = _allRecords.Where(r => r.AreaInSqm.HasValue).Select(r => r.AreaInSqm!.Value).DefaultIfEmpty(0).Max();
        }

        private void PopulateFilterDropdowns()
        {
            PopulateComboBox(cbProvince, _uniqueProvinces, "-- All Provinces --");
            PopulateComboBox(cbDistrict, _uniqueDistricts, "-- All Districts --");
            PopulateComboBox(cbMunicipalityVillage, _uniqueMunicipalities, "-- All Municipalities --");
            PopulateComboBox(cbWardNo, _uniqueWards, "-- All --");
            PopulateComboBox(cbMapSheet, _uniqueMapSheets, "-- All Map Sheets --");
            PopulateComboBox(cbLandOwnership, _uniqueOwnershipTypes, "-- All Types --");
        }

        private static void PopulateComboBox(ComboBox comboBox, HashSet<string> values, string defaultText)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add(defaultText);

            // If only one unique value, disable the combobox
            if (values.Count <= 1)
            {
                comboBox.Enabled = false;
                if (values.Count == 1)
                {
                    comboBox.Items.Add(values.First());
                }
            }
            else
            {
                comboBox.Enabled = true;
                foreach (var value in values.OrderBy(v => v))
                {
                    comboBox.Items.Add(value);
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private void BindRecordsToGrid(List<OriginalLandParcel> records)
        {
            // Sort by MapSheet first, then by ParcelNo
            var sortedRecords = records
                .OrderBy(r => r.MapSheetNo)
                .ThenBy(r => r.ParcelNo, new NaturalStringComparer())
                .ToList();

            var displayModels = sortedRecords.Select(r => new LandParcelDisplayModel
            {
                ParcelId = r.ParcelId,
                LandOwnerId = r.LandOwnerId,
                ParcelNo = r.ParcelNo,
                MapSheetNo = r.MapSheetNo,
                ParcelLocation = r.ParcelLocation ?? "",
                Province = r.Province ?? "",
                District = r.District ?? "",
                MunicipalityVillage = r.MunicipalityVillage ?? "",
                WardNo = r.WardNo ?? "",
                LandOwnersName = r.Owner?.LandOwnersName ?? "",
                FatherSpouse = r.Owner?.FatherSpouse ?? "",
                Gender = r.Owner?.Gender ?? "",
                CitizenshipNumber = r.Owner?.CitizenshipNumber ?? "",
                PermanentAddress = r.Owner?.PermanentAddress ?? "",
                AreaInSqm = r.AreaInSqm,
                AreaInRAPD = r.AreaInRAPD ?? "",
                AreaInBKD = r.AreaInBKD ?? "",
                LandOwnershipType = r.LandOwnershipType ?? "",
                LandUse = r.LandUse ?? "",
                IsTenant = r.IsTenant ?? "",
                MothNo = r.MothNo ?? "",
                PaanaNo = r.PaanaNo ?? "",
                Remarks = r.Remarks ?? ""
            }).ToList();

            _displayedRecords = new BindingList<LandParcelDisplayModel>(displayModels);
            dgvRecords.DataSource = _displayedRecords;
        }

        #endregion

        #region Filtering Logic

        private void ApplyFilters()
        {
            ApplyFilters(showValidationMessage: true);
        }

        private void ApplyFilters(bool showValidationMessage)
        {
            var filtered = _allRecords.AsEnumerable();

            // Location filters
            filtered = ApplyComboFilter(filtered, cbProvince, r => r.Province);
            filtered = ApplyComboFilter(filtered, cbDistrict, r => r.District);
            filtered = ApplyComboFilter(filtered, cbMunicipalityVillage, r => r.MunicipalityVillage);
            filtered = ApplyComboFilter(filtered, cbWardNo, r => r.WardNo);

            // Map sheet filter
            filtered = ApplyComboFilter(filtered, cbMapSheet, r => r.MapSheetNo);

            // Ownership filter
            filtered = ApplyComboFilter(filtered, cbLandOwnership, r => r.LandOwnershipType);

            // Area range filter (returns null if validation fails)
            var areaFiltered = ApplyAreaFilter(filtered, showValidationMessage);
            if (areaFiltered == null)
                return; // Stop filtering if area validation failed

            filtered = areaFiltered;

            // Search filters
            filtered = ApplySearchFiltersToRecords(filtered);

            _filteredRecords = [.. filtered];
            BindRecordsToGrid(_filteredRecords);
            UpdateStatusLabels();
        }

        private static IEnumerable<OriginalLandParcel> ApplyComboFilter(
            IEnumerable<OriginalLandParcel> records,
            ComboBox comboBox,
            Func<OriginalLandParcel, string?> selector)
        {
            if (comboBox.SelectedIndex <= 0 || !comboBox.Enabled)
                return records;

            string selected = comboBox.SelectedItem?.ToString() ?? "";
            return records.Where(r => (selector(r) ?? "") == selected);
        }

        private IEnumerable<OriginalLandParcel>? ApplyAreaFilter(IEnumerable<OriginalLandParcel> records, bool showValidationMessage)
        {
            // Skip if both fields are empty
            if (string.IsNullOrWhiteSpace(txtFromArea.Text) && string.IsNullOrWhiteSpace(txtToArea.Text))
                return records;

            double fromArea = ParseAreaValue(txtFromArea.Text, 0);
            double toArea = ParseAreaValue(txtToArea.Text, _maxAreaSqm > 0 ? _maxAreaSqm : double.MaxValue);

            // Determine the selected unit for comparison
            bool isRopaneeMode = rbRopanee.Checked;
            bool isAanaMode = rbAana.Checked;

            // Validate: From area should not be greater than To area
            if (fromArea > 0 && toArea < double.MaxValue && fromArea > toArea)
            {
                if (showValidationMessage)
                {
                    string unit = isRopaneeMode ? "Ropani" : isAanaMode ? "Aana" : "sqm";
                    string fromDisplay = (isRopaneeMode || isAanaMode)
                        ? ParseAreaValue(txtFromArea.Text, 0).ToString("F2")
                        : txtFromArea.Text;
                    string toDisplay = (isRopaneeMode || isAanaMode)
                        ? ParseAreaValue(txtToArea.Text, 0).ToString("F2")
                        : txtToArea.Text;

                    MessageBox.Show(
                        $"'From Area' ({fromDisplay} {unit}) cannot be greater than 'To Area' ({toDisplay} {unit}).\n\nPlease correct the area range.",
                        "Invalid Area Range",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return null; // Return null to indicate validation failed
                }

                return records; // Ignore invalid area range in quick mode
            }

            // Skip filtering if range covers all
            if (isRopaneeMode)
            {
                double maxRopani = _maxAreaSqm / 508.74; // Convert max sqm to Ropani
                if (fromArea <= 0 && toArea >= maxRopani)
                    return records;
            }
            else if (isAanaMode)
            {
                double maxAana = _maxAreaSqm / 31.796875; // Convert max sqm to Aana
                if (fromArea <= 0 && toArea >= maxAana)
                    return records;
            }
            else
            {
                if (fromArea <= 0 && toArea >= _maxAreaSqm)
                    return records;
            }

            return records.Where(r =>
            {
                double? areaSqm = AreaConverterService.GetAreaInSqm(r.AreaInSqm, r.AreaInRAPD, r.AreaInBKD);
                if (!areaSqm.HasValue) return false;

                // Convert the parcel area to the selected unit
                double areaInSelectedUnit;
                if (isRopaneeMode)
                {
                    // Convert sqm to Ropani
                    areaInSelectedUnit = AreaConverterService.SqmToRopani(areaSqm.Value);
                }
                else if (isAanaMode)
                {
                    // Convert sqm to Aana (1 Aana = 31.796875 sqm)
                    areaInSelectedUnit = areaSqm.Value / 31.796875;
                }
                else
                {
                    // sqm mode - no conversion needed
                    areaInSelectedUnit = areaSqm.Value;
                }

                return areaInSelectedUnit >= fromArea && areaInSelectedUnit <= toArea;
            });
        }

        private IEnumerable<OriginalLandParcel> ApplySearchFiltersToRecords(IEnumerable<OriginalLandParcel> records)
        {
            string parcelSearch = txtParcelNo.Text.Trim();
            string ownerSearch = txtLandOwner.Text.Trim().ToLower();

            // Exact match for parcel number
            if (!string.IsNullOrWhiteSpace(parcelSearch))
            {
                records = records.Where(r => r.ParcelNo?.Equals(parcelSearch, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Partial match for owner name (contains search)
            if (!string.IsNullOrWhiteSpace(ownerSearch))
            {
                records = records.Where(r =>
                    r.Owner?.LandOwnersName?.ToLower().Contains(ownerSearch) == true ||
                    r.Owner?.FatherSpouse?.ToLower().Contains(ownerSearch) == true);
            }

            return records;
        }

        /// <summary>
        /// Standalone search filter application (for quick search toggle)
        /// </summary>
        private void ApplySearchFilters()
        {
            ApplySearchFilters(showValidationMessage: true);
        }

        private void ApplySearchFilters(bool showValidationMessage)
        {
            ApplyFilters(showValidationMessage);
        }

        private static double ParseAreaValue(string text, double defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return defaultValue;

            return double.TryParse(text.Trim(), out double value) && value >= 0
                ? value
                : defaultValue;
        }

        private void ClearFilters()
        {
            // Reset all comboboxes to first item
            if (cbProvince.Items.Count > 0) cbProvince.SelectedIndex = 0;
            if (cbDistrict.Items.Count > 0) cbDistrict.SelectedIndex = 0;
            if (cbMunicipalityVillage.Items.Count > 0) cbMunicipalityVillage.SelectedIndex = 0;
            if (cbWardNo.Items.Count > 0) cbWardNo.SelectedIndex = 0;
            if (cbMapSheet.Items.Count > 0) cbMapSheet.SelectedIndex = 0;
            if (cbLandOwnership.Items.Count > 0) cbLandOwnership.SelectedIndex = 0;

            // Clear area filter text boxes
            txtFromArea.Clear();
            txtToArea.Clear();

            // Reset radio button
            rbSqm.Checked = true;

            // Refresh data
            _filteredRecords = [.. _allRecords];
            BindRecordsToGrid(_filteredRecords);
            UpdateStatusLabels();
        }

        private void ClearSearch()
        {
            // Clear search text boxes
            txtParcelNo.Clear();
            txtLandOwner.Clear();

            // Refresh data
            _filteredRecords = [.. _allRecords];
            BindRecordsToGrid(_filteredRecords);
            UpdateStatusLabels();
        }

        private void ClearAllFilters()
        {
            ClearFilters();
            ClearSearch();
        }

        #endregion

        #region Input Validation

        private void AreaTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow: digits, decimal point, backspace, delete
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
                return;

            // Allow single decimal point
            if (e.KeyChar == '.' && sender is TextBox textBox && !textBox.Text.Contains('.'))
                return;

            e.Handled = true; // Block all other characters
        }

        private void ParcelNo_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow: digits, backspace
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
                return;

            e.Handled = true;
        }

        private void LandOwnerName_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow control characters (backspace, etc.)
            if (char.IsControl(e.KeyChar))
                return;

            // Allow all Unicode letters (including Devanagari U+0900-U+097F)
            if (char.IsLetter(e.KeyChar))
                return;

            // Allow whitespace
            if (char.IsWhiteSpace(e.KeyChar))
                return;

            // Allow common name punctuation
            if (e.KeyChar == '.' || e.KeyChar == '\'' || e.KeyChar == '-')
                return;

            // Allow Devanagari Unicode range (U+0900 to U+097F) including vowel signs, matras, etc.
            if (e.KeyChar >= '\u0900' && e.KeyChar <= '\u097F')
                return;

            // Allow Devanagari Extended range (U+A8E0 to U+A8FF)
            if (e.KeyChar >= '\uA8E0' && e.KeyChar <= '\uA8FF')
                return;

            // Block other characters
            e.Handled = true;
        }

        #endregion

        #region Event Handlers

        private void BtnApplyFilter_Click(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void BtnClearFilter_Click(object? sender, EventArgs e)
        {
            ClearFilters();
        }

        private void BtnApplySearch_Click(object? sender, EventArgs e)
        {
            ApplySearchFilters();
        }

        private void BtnClearSearch_Click(object? sender, EventArgs e)
        {
            ClearSearch();
        }

        private void ChkToggleQuickFilter_CheckedChanged(object? sender, EventArgs e)
        {
            btnApplyFilter.Enabled = !chkToggleQuickFilter.Checked;

            if (chkToggleQuickFilter.Checked)
            {
                ApplyFilters(showValidationMessage: false);
            }
        }

        private void ChkToggleQuickSearch_CheckedChanged(object? sender, EventArgs e)
        {
            btnApplySearch.Enabled = !chkToggleQuickSearch.Checked;

            if (chkToggleQuickSearch.Checked)
            {
                ApplySearchFilters(showValidationMessage: false);
            }
        }

        private void ComboFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (chkToggleQuickFilter.Checked)
            {
                ApplyFilters(showValidationMessage: false);
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            btnApplySearch.Focus();
            if (chkToggleQuickSearch.Checked)
            {
                ApplySearchFilters(showValidationMessage: false);
            }
        }

        private void TxtArea_TextChanged(object? sender, EventArgs e)
        {
            if (chkToggleQuickFilter.Checked)
            {
                ApplyFilters(showValidationMessage: false);
            }
        }

        private void RadioButton_CheckedChanged(object? sender, EventArgs e)
        {


            if (rbRopanee.Checked)
            {
                //double ropanee = AreaConverterService.SqmToRopani(double.Parse(txtFromArea.Text));
                //txtFromArea.Text = ropanee.ToString();
                //ropanee = AreaConverterService.SqmToRopani(double.Parse(txtToArea.Text));
                //txtToArea.Text = ropanee.ToString();
                txtFromArea.PlaceholderText = "Ropanee";
                txtToArea.PlaceholderText = "Ropanee";
            }
            else if (rbSqm.Checked)
            {
                //double  sqm= AreaConverterService.RopaniDecimalToSqm(double.Parse(txtFromArea.Text));
                //txtFromArea.Text = sqm.ToString();
                // sqm = AreaConverterService.RopaniDecimalToSqm(double.Parse(txtToArea.Text));
                //txtToArea.Text = sqm.ToString();
                txtFromArea.PlaceholderText = "sq.m.";
                txtToArea.PlaceholderText = "sq.m.";
            }
            else
            {
                txtFromArea.PlaceholderText = "Aana";
                txtToArea.PlaceholderText = "Aana";
            }
            if (chkToggleQuickFilter.Checked)
            {
                ApplyFilters(showValidationMessage: false);
            }
        }

        private void DgvRecords_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void DgvRecords_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            EditSelectedRecord();
        }

        private void DgvRecords_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            string rowNumber = (e.RowIndex + 1).ToString();

            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dgvRecords.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                rowNumber,
                dgvRecords.DefaultCellStyle.Font,
                headerBounds,
                dgvRecords.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void DgvRecords_Leave(object? sender, EventArgs e)
        {
            dgvRecords.ClearSelection();
        }

        private void DgvRecords_MouseLeave(object? sender, EventArgs e)
        {
            dgvRecords.ClearSelection();
        }

        #endregion

        #region CRUD Operations

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var addForm = new frmAddEditRecord(_repository.ParcelExists, ownerFieldsReadOnly: true);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var record = addForm.Record;
                    SaveNewRecord(record);
                    LoadAllRecords();
                    PopulateFilterDropdowns();
                    ApplyFilters();

                    MessageBox.Show("Record added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add record: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1) return;
            EditSelectedRecord();
        }

        private void EditSelectedRecord()
        {
            if (dgvRecords.SelectedRows[0].DataBoundItem is not LandParcelDisplayModel model)
                return;

            var parcel = _allRecords.FirstOrDefault(r => r.ParcelId == model.ParcelId);
            if (parcel == null) return;

            var record = ConvertToEditableRecord(parcel);

            using var editForm = new frmAddEditRecord(record, model.ParcelId, _repository.ParcelExists, ownerFieldsReadOnly: true);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (editForm.IsDeleted)
                    {
                        _repository.DeleteParcel(model.ParcelId);
                        MessageBox.Show("Record deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var updatedRecord = editForm.Record;
                        UpdateExistingRecord(model.ParcelId, model.LandOwnerId, updatedRecord);
                        MessageBox.Show("Record updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    LoadAllRecords();
                    PopulateFilterDropdowns();
                    ApplyFilters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update record: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 0) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete the selected record?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                if (dgvRecords.SelectedRows[0].DataBoundItem is LandParcelDisplayModel model)
                {
                    _repository.DeleteParcel(model.ParcelId);
                    LoadAllRecords();
                    PopulateFilterDropdowns();
                    ApplyFilters();

                    MessageBox.Show("Record deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadAllRecords();
            PopulateFilterDropdowns();
            ClearAllFilters();
        }

        private void BtnViewLandOwnerDetails_Click(object? sender, EventArgs e)
        {
            ViewLandOwnerDetails();
        }

        private void ViewLandOwnerDetails()
        {
            if (dgvRecords.SelectedRows.Count != 1)
            {
                MessageBox.Show("Please select a single record to view details.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvRecords.SelectedRows[0].DataBoundItem is not LandParcelDisplayModel model)
                return;

            var parcel = _allRecords.FirstOrDefault(r => r.ParcelId == model.ParcelId);
            if (parcel == null) return;

            using var detailsForm = new frmLandOwnerDetails(parcel.LandOwnerId, readOnlyMode: true);
            detailsForm.ShowDialog();

            // Refresh after details form closes in case changes were made
            LoadAllRecords();
            ApplyFilters();
        }

        #endregion

        #region Helper Methods

        private void SaveNewRecord(BaselineLandParceRecord record)
        {
            var records = new List<BaselineLandParceRecord> { record };

            // Step 1: Run owner deduplication on the single record
            var deduplicationResult = OwnerDeduplicationService.ExtractUniqueOwners(records, excludeAnonymous: false);

            // Step 2: If there are duplicates needing review, show the review form
            if (deduplicationResult.DuplicatesNeedingReview.Count > 0)
            {
                var bindingList = new BindingList<BaselineLandParceRecord>(records);
                using var reviewForm = new frmReviewDuplicates(deduplicationResult, bindingList);

                if (reviewForm.ShowDialog() != DialogResult.OK)
                {
                    // User cancelled the review
                    return;
                }

                // The deduplicationResult was modified in-place by the review form
                // No need to retrieve it - just use the same reference
            }

            // Step 3: Save using the deduplication result
            var parcelToOwnerMap = _repository.SaveUniqueOwnersFromDeduplication(deduplicationResult);
            int savedCount = _repository.SaveParcelsWithDeduplication(records, parcelToOwnerMap);

            if (savedCount == 0)
            {
                throw new Exception("Failed to save parcel - it may be a duplicate.");
            }
        }

        private void UpdateExistingRecord(int parcelId, int existingLandOwnerId, BaselineLandParceRecord record)
        {
            // Step 1: Check if owner information has changed
            var existingParcel = _allRecords.FirstOrDefault(p => p.ParcelId == parcelId);
            bool ownerChanged = false;

            if (existingParcel?.Owner != null)
            {
                ownerChanged = existingParcel.Owner.LandOwnersName != (record.LandOwnersName ?? "") ||
                               existingParcel.Owner.FatherSpouse != record.FatherSpouse ||
                               existingParcel.Owner.CitizenshipNumber != record.CitizenshipNumber;
            }

            int landOwnerIdToUse = existingLandOwnerId;

            if (ownerChanged)
            {
                // Step 2: Run owner deduplication to find if this owner already exists
                var records = new List<BaselineLandParceRecord> { record };
                var deduplicationResult = OwnerDeduplicationService.ExtractUniqueOwners(records, excludeAnonymous: false);

                // Step 3: If there are duplicates needing review, show the review form
                if (deduplicationResult.DuplicatesNeedingReview.Count > 0)
                {
                    var bindingList = new BindingList<BaselineLandParceRecord>(records);
                    using var reviewForm = new frmReviewDuplicates(deduplicationResult, bindingList);

                    if (reviewForm.ShowDialog() != DialogResult.OK)
                    {
                        // User cancelled the review
                        return;
                    }

                    // The deduplicationResult was modified in-place by the review form
                }

                // Step 4: Save or get the owner ID
                var parcelToOwnerMap = _repository.SaveUniqueOwnersFromDeduplication(deduplicationResult);

                // The first (and only) record in our list is at index 0
                if (parcelToOwnerMap.TryGetValue(0, out int newOwnerId))
                {
                    landOwnerIdToUse = newOwnerId;
                }
                else
                {
                    throw new Exception("Failed to get owner ID after deduplication.");
                }
            }
            else
            {
                // Step 5: Owner info hasn't changed, just update the existing owner record
                var owner = new LandOwner
                {
                    LandOwnerId = existingLandOwnerId,
                    LandOwnersName = record.LandOwnersName ?? "",
                    FatherSpouse = record.FatherSpouse,
                    Gender = record.Gender,
                    CitizenshipNumber = record.CitizenshipNumber,
                    CitizenshipIssuedDistrict = record.CitizenshipIssuedDistrict,
                    CitizenshipIssuedDate = record.citizenshipIssuedDate, // lowercase 'c' property
                    PermanentAddress = record.PermanentAddress,
                    TemporaryAddress = record.TempoaryAddress, // Note: typo in model - 'Tempoary'
                    ContactNumber = record.ContactNumber,
                    EmailID = record.EmailID,
                    ModifiedDate = DateTime.Now
                };
                _repository.UpdateOwner(owner);
            }

            // Step 6: Update the parcel with the correct owner ID
            var parcel = new OriginalLandParcel
            {
                ParcelId = parcelId,
                LandOwnerId = landOwnerIdToUse,
                ParcelNo = record.ParcelNo ?? "",
                MapSheetNo = record.MapSheetNo ?? "",
                Province = record.Province,
                District = record.District,
                MunicipalityVillage = record.MunicipalityVillage,
                WardNo = record.WardNo,
                ParcelLocation = record.ParcelLocation,
                IsTenant = record.Tenant,
                LandUse = record.LandUse,
                LandOwnershipType = record.LandOwnershipType,
                AreaInSqm = record.AreaInSqm,
                AreaInRAPD = record.AreaInRAPD,
                AreaInBKD = record.AreaInBKD,
                MothNo = record.MothNo,
                PaanaNo = record.PaanaNo,
                Remarks = record.Remarks
            };
            _repository.UpdateParcel(parcel);
        }

        private static BaselineLandParceRecord ConvertToEditableRecord(OriginalLandParcel parcel)
        {
            return new BaselineLandParceRecord
            {
                ParcelNo = parcel.ParcelNo,
                MapSheetNo = parcel.MapSheetNo,
                Province = parcel.Province,
                District = parcel.District,
                MunicipalityVillage = parcel.MunicipalityVillage,
                WardNo = parcel.WardNo,
                ParcelLocation = parcel.ParcelLocation,
                LandOwnersName = parcel.Owner?.LandOwnersName,
                FatherSpouse = parcel.Owner?.FatherSpouse,
                Gender = parcel.Owner?.Gender,
                CitizenshipNumber = parcel.Owner?.CitizenshipNumber,
                PermanentAddress = parcel.Owner?.PermanentAddress,
                Tenant = parcel.IsTenant,
                LandUse = parcel.LandUse,
                LandOwnershipType = parcel.LandOwnershipType,
                AreaInSqm = parcel.AreaInSqm,
                AreaInRAPD = parcel.AreaInRAPD,
                AreaInBKD = parcel.AreaInBKD,
                MothNo = parcel.MothNo,
                PaanaNo = parcel.PaanaNo,
                Remarks = parcel.Remarks
            };
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvRecords.SelectedRows.Count == 1;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            toolStripButton1.Enabled = hasSelection;
        }

        private void UpdateStatusLabels()
        {
            lblTotalRecords.Text = $"Total Records: {_allRecords.Count}";
            lblFilteredRecords.Text = $"Filtered Records: {_filteredRecords.Count}";
            lblSelectedRecords.Text = $"Selected: {dgvRecords.SelectedRows.Count}";
        }

        #endregion

        #region Unused Designer Events

        private void label2_Click(object sender, EventArgs e) { }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void radioButton3_CheckedChanged(object sender, EventArgs e) { }

        #endregion
    }

    #region Display Model

    /// <summary>
    /// Display model for land parcel records grid
    /// </summary>
    public class LandParcelDisplayModel
    {
        public int ParcelId { get; set; }
        public int LandOwnerId { get; set; }
        public string ParcelNo { get; set; } = "";
        public string MapSheetNo { get; set; } = "";
        public string ParcelLocation { get; set; } = "";
        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string MunicipalityVillage { get; set; } = "";
        public string WardNo { get; set; } = "";
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string Gender { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
        public double? AreaInSqm { get; set; }
        public string AreaInRAPD { get; set; } = "";
        public string AreaInBKD { get; set; } = "";
        public string LandOwnershipType { get; set; } = "";
        public string LandUse { get; set; } = "";
        public string IsTenant { get; set; } = "";
        public string MothNo { get; set; } = "";
        public string PaanaNo { get; set; } = "";
        public string Remarks { get; set; } = "";
    }

    #endregion

    #region Natural String Comparer

    /// <summary>
    /// Compares strings with natural sorting (e.g., "2" comes before "10")
    /// </summary>
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int xIndex = 0, yIndex = 0;

            while (xIndex < x.Length && yIndex < y.Length)
            {
                if (char.IsDigit(x[xIndex]) && char.IsDigit(y[yIndex]))
                {
                    // Extract numeric parts
                    string xNum = ExtractNumber(x, ref xIndex);
                    string yNum = ExtractNumber(y, ref yIndex);

                    // Compare as integers
                    if (int.TryParse(xNum, out int xInt) && int.TryParse(yNum, out int yInt))
                    {
                        int numCompare = xInt.CompareTo(yInt);
                        if (numCompare != 0) return numCompare;
                    }
                    else
                    {
                        // Fallback to string comparison if parsing fails
                        int strCompare = string.Compare(xNum, yNum, StringComparison.OrdinalIgnoreCase);
                        if (strCompare != 0) return strCompare;
                    }
                }
                else
                {
                    // Compare character by character
                    int charCompare = char.ToLower(x[xIndex]).CompareTo(char.ToLower(y[yIndex]));
                    if (charCompare != 0) return charCompare;
                    
                    xIndex++;
                    yIndex++;
                }
            }

            // If one string is longer, it comes after
            return x.Length.CompareTo(y.Length);
        }

        private static string ExtractNumber(string str, ref int index)
        {
            int start = index;
            while (index < str.Length && char.IsDigit(str[index]))
            {
                index++;
            }
            return str.Substring(start, index - start);
        }
    }

    #endregion
}
