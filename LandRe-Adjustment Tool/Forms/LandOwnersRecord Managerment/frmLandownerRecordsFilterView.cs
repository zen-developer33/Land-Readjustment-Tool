using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;
using System.Data;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Advanced filter view form for viewing, filtering, and managing land parcel ownership records
    /// </summary>
    public partial class frmLandownerRecordsFilterView : Form
    {
        private readonly string _projectPath;
        private LandOwnerRepository _repository = null!;
        private List<ParcelOwnerDisplayModel> _allRecords = [];
        private BindingList<ParcelOwnerDisplayModel> _displayedRecords = [];
        private int _totalRecords;
        private int _filteredCount;

        public frmLandownerRecordsFilterView(string projectPath)
        {
            InitializeComponent();
            _projectPath = projectPath;

            InitializeRepository();
            SetupEventHandlers();
            SetupDataGridView();
        }

        #region Initialization

        private void InitializeRepository()
        {
            var dbHelper = new DatabaseHelper(_projectPath);
            dbHelper.InitializeDatabase();
            var connection = dbHelper.GetConnection();

            if (!DatabaseSchema.HasCorrectSchema(connection))
            {
                DatabaseSchema.RecreateSchema(connection);
            }
            else
            {
                DatabaseSchema.CreateSchema(connection);
            }

            _repository = new LandOwnerRepository(connection);
        }

        private void SetupEventHandlers()
        {
            // Filter controls
            btnApplyFilters.Click += BtnApplyFilters_Click;
            btnClearFilters.Click += BtnClearFilters_Click;
            btnExportFiltered.Click += BtnExportFiltered_Click;
            chkAllMapSheets.CheckedChanged += ChkAllMapSheets_CheckedChanged;

            // Real-time search (with debounce effect using timer could be added)
            txtParcelNo.TextChanged += TxtSearch_TextChanged;
            txtOwnerName.TextChanged += TxtSearch_TextChanged;
            txtCitizenshipNo.TextChanged += TxtSearch_TextChanged;

            // CRUD buttons
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnViewDetails.Click += BtnViewDetails_Click;

            // DataGridView events
            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            dgvRecords.CellDoubleClick += DgvRecords_CellDoubleClick;
            dgvRecords.RowPostPaint += DgvRecords_RowPostPaint;

            // ComboBox events
            cboMapSheet.SelectedIndexChanged += CboFilter_SelectedIndexChanged;
            cboDistrict.SelectedIndexChanged += CboFilter_SelectedIndexChanged;
            cboLandUse.SelectedIndexChanged += CboFilter_SelectedIndexChanged;
        }

        private void SetupDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.Columns.Clear();

            // Add columns
            AddColumn("ParcelNo", "Parcel No", 80);
            AddColumn("MapSheetNo", "Map Sheet", 85);
            AddColumn("LandOwnersName", "Owner Name", 160);
            AddColumn("FatherSpouse", "Father/Spouse", 130);
            AddColumn("PermanentAddress", "Permanent Address", 140);
            AddColumn("CitizenshipNumber", "Citizenship No", 110);
            AddColumn("CitizenshipIssueDate", "Issue Date", 90);
            AddColumn("CitizenshipIssueDistrict", "Issue District", 100);
            AddColumn("Gender", "Gender", 60);
            AddColumn("ParcelLocation", "Parcel Location", 130);
            AddColumn("AreaInSqm", "Area (sqm)", 85);
            AddColumn("AreaInRAPD", "Area (RAPD)", 85);
            AddColumn("AreaInBKD", "Area (BKD)", 85);
            AddColumn("LandUse", "Land Use", 80);
            AddColumn("IsTenant", "Tenant", 55);
            AddColumn("MothNo", "Moth No", 65);
            AddColumn("PaanaNo", "Paana No", 65);
            AddColumn("Province", "Province", 75);
            AddColumn("District", "District", 75);
            AddColumn("MunicipalityVillage", "Municipality", 95);
            AddColumn("Remarks", "Remarks", 100);

            // Enable sorting
            foreach (DataGridViewColumn col in dgvRecords.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }
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

        private void frmLandownerRecordsFilterView_Load(object sender, EventArgs e)
        {
            LoadAllRecords();
            PopulateFilterDropdowns();
        }

        private void LoadAllRecords()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                progressBar.Visible = true;

                var parcels = _repository.GetAllParcelsWithOwners();
                _allRecords = [];

                foreach (var parcel in parcels)
                {
                    _allRecords.Add(new ParcelOwnerDisplayModel
                    {
                        ParcelId = parcel.ParcelId,
                        LandOwnerId = parcel.LandOwnerId,
                        ParcelNo = parcel.ParcelNo,
                        MapSheetNo = parcel.MapSheetNo,
                        Province = parcel.Province ?? "",
                        District = parcel.District ?? "",
                        MunicipalityVillage = parcel.MunicipalityVillage ?? "",
                        LandOwnersName = parcel.Owner?.LandOwnersName ?? "",
                        FatherSpouse = parcel.Owner?.FatherSpouse ?? "",
                        CitizenshipNumber = parcel.Owner?.CitizenshipNumber ?? "",
                        CitizenshipIssueDate = parcel.Owner?.CitizenshipIssuedDate ?? "",
                        citizenshipIssueDistrict = parcel.Owner?.CitizenshipIssuedDistrict ?? "",
                        Gender = parcel.Owner?.Gender ?? "",
                        ParcelLocation = parcel.ParcelLocation ?? "",
                        PermanentAddress = parcel.Owner?.PermanentAddress ?? "",
                        AreaInSqm = parcel.AreaInSqm,
                        AreaInRAPD = parcel.AreaInRAPD ?? "",
                        AreaInBKD = parcel.AreaInBKD ?? "",
                        LandUse = parcel.LandUse ?? "",
                        IsTenant = parcel.IsTenant ?? "",
                        MothNo = parcel.MothNo ?? "",
                        PaanaNo = parcel.PaanaNo ?? "",
                        Remarks = parcel.Remarks ?? ""
                    });
                }

                _totalRecords = _allRecords.Count;

                // Sort by MapSheetNo then ParcelNo (natural numeric sorting)
                _allRecords = [.. _allRecords
                    .OrderBy(r => TryParseInt(r.MapSheetNo, out int mapSheet) ? mapSheet : int.MaxValue)
                    .ThenBy(r => r.MapSheetNo)
                    .ThenBy(r => TryParseInt(r.ParcelNo, out int parcelNo) ? parcelNo : int.MaxValue)
                    .ThenBy(r => r.ParcelNo)];

                _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(_allRecords);
                dgvRecords.DataSource = _displayedRecords;

                _filteredCount = _allRecords.Count;
                UpdateStatusLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                _allRecords = [];
                _displayedRecords = [];
                _totalRecords = 0;
            }
            finally
            {
                Cursor = Cursors.Default;
                progressBar.Visible = false;
            }
        }

        private void PopulateFilterDropdowns()
        {
            // MapSheet dropdown
            var mapSheets = _allRecords
                .Select(r => r.MapSheetNo)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .OrderBy(m => TryParseInt(m, out int val) ? val : int.MaxValue)
                .ThenBy(m => m)
                .ToList();

            cboMapSheet.Items.Clear();
            cboMapSheet.Items.Add("-- All Map Sheets --");
            foreach (var sheet in mapSheets)
            {
                cboMapSheet.Items.Add(sheet);
            }
            cboMapSheet.SelectedIndex = 0;

            // District dropdown
            var districts = _allRecords
                .Select(r => r.District)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            cboDistrict.Items.Clear();
            cboDistrict.Items.Add("-- All Districts --");
            foreach (var district in districts)
            {
                cboDistrict.Items.Add(district);
            }
            cboDistrict.SelectedIndex = 0;

            // LandUse dropdown
            var landUses = _allRecords
                .Select(r => r.LandUse)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct()
                .OrderBy(l => l)
                .ToList();

            cboLandUse.Items.Clear();
            cboLandUse.Items.Add("-- All Land Uses --");
            foreach (var landUse in landUses)
            {
                cboLandUse.Items.Add(landUse);
            }
            cboLandUse.SelectedIndex = 0;
        }

        #endregion

        #region Filtering

        private void ApplyFilters()
        {
            var filtered = _allRecords.AsEnumerable();

            // MapSheet filter
            if (!chkAllMapSheets.Checked && cboMapSheet.SelectedIndex > 0)
            {
                string selectedMapSheet = cboMapSheet.SelectedItem?.ToString() ?? "";
                filtered = filtered.Where(r => r.MapSheetNo == selectedMapSheet);
            }

            // District filter
            if (cboDistrict.SelectedIndex > 0)
            {
                string selectedDistrict = cboDistrict.SelectedItem?.ToString() ?? "";
                filtered = filtered.Where(r => r.District == selectedDistrict);
            }

            // LandUse filter
            if (cboLandUse.SelectedIndex > 0)
            {
                string selectedLandUse = cboLandUse.SelectedItem?.ToString() ?? "";
                filtered = filtered.Where(r => r.LandUse == selectedLandUse);
            }

            // Parcel No search
            string parcelSearch = txtParcelNo.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(parcelSearch))
            {
                filtered = filtered.Where(r =>
                    r.ParcelNo?.ToLower().Contains(parcelSearch) == true);
            }

            // Owner Name search
            string ownerSearch = txtOwnerName.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(ownerSearch))
            {
                filtered = filtered.Where(r =>
                    r.LandOwnersName?.ToLower().Contains(ownerSearch) == true ||
                    r.FatherSpouse?.ToLower().Contains(ownerSearch) == true);
            }

            // Citizenship No search
            string citizenshipSearch = txtCitizenshipNo.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(citizenshipSearch))
            {
                filtered = filtered.Where(r =>
                    r.CitizenshipNumber?.ToLower().Contains(citizenshipSearch) == true);
            }

            var filteredList = filtered.ToList();
            _filteredCount = filteredList.Count;

            _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(filteredList);
            dgvRecords.DataSource = _displayedRecords;

            UpdateStatusLabels();
        }

        private void ClearAllFilters()
        {
            // Reset MapSheet
            chkAllMapSheets.Checked = true;
            if (cboMapSheet.Items.Count > 0)
                cboMapSheet.SelectedIndex = 0;

            // Reset other dropdowns
            if (cboDistrict.Items.Count > 0)
                cboDistrict.SelectedIndex = 0;
            if (cboLandUse.Items.Count > 0)
                cboLandUse.SelectedIndex = 0;

            // Clear text boxes
            txtParcelNo.Clear();
            txtOwnerName.Clear();
            txtCitizenshipNo.Clear();

            // Reset grid
            _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(_allRecords);
            dgvRecords.DataSource = _displayedRecords;
            _filteredCount = _allRecords.Count;

            UpdateStatusLabels();
        }

        #endregion

        #region Event Handlers

        private void BtnApplyFilters_Click(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void BtnClearFilters_Click(object? sender, EventArgs e)
        {
            ClearAllFilters();
        }

        private void ChkAllMapSheets_CheckedChanged(object? sender, EventArgs e)
        {
            cboMapSheet.Enabled = !chkAllMapSheets.Checked;
            if (chkAllMapSheets.Checked)
            {
                ApplyFilters();
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            // Real-time filtering as user types
            ApplyFilters();
        }

        private void CboFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void DgvRecords_SelectionChanged(object? sender, EventArgs e)
        {
            int selectedCount = dgvRecords.SelectedRows.Count;
            bool hasSelection = selectedCount > 0;
            bool singleSelection = selectedCount == 1;

            btnEdit.Enabled = singleSelection;
            btnDelete.Enabled = hasSelection;
            btnViewDetails.Enabled = singleSelection;

            lblSelectedRecords.Text = $"Selected: {selectedCount}";
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

        #endregion

        #region CRUD Operations

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var addForm = new frmAddEditRecord();
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

        private void BtnViewDetails_Click(object? sender, EventArgs e)
        {
            EditSelectedRecord();
        }

        private void EditSelectedRecord()
        {
            if (dgvRecords.SelectedRows[0].DataBoundItem is not ParcelOwnerDisplayModel model)
                return;

            var record = ConvertToEditableRecord(model);

            using var editForm = new frmAddEditRecord(record, model.ParcelId);
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

            int selectedCount = dgvRecords.SelectedRows.Count;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedCount} record(s)?\n\n" +
                "This action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                int deletedCount = 0;
                foreach (DataGridViewRow row in dgvRecords.SelectedRows)
                {
                    if (row.DataBoundItem is ParcelOwnerDisplayModel model)
                    {
                        if (_repository.DeleteParcel(model.ParcelId))
                        {
                            deletedCount++;
                        }
                    }
                }

                LoadAllRecords();
                PopulateFilterDropdowns();
                ApplyFilters();

                MessageBox.Show($"{deletedCount} record(s) deleted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadAllRecords();
            PopulateFilterDropdowns();
            ClearAllFilters();
        }

        private void BtnExportFiltered_Click(object? sender, EventArgs e)
        {
            if (_displayedRecords.Count == 0)
            {
                MessageBox.Show("No records to export.", "Export",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var saveDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"LandOwnerRecords_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ExportToCsv(saveDialog.FileName);
                    MessageBox.Show($"Exported {_displayedRecords.Count} records successfully!",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Export failed: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Helper Methods

        private void SaveNewRecord(BaselineLandParceRecord record)
        {
            var records = new List<BaselineLandParceRecord> { record };
            var ownerMap = _repository.ExtractAndSaveUniqueOwners(records);
            _repository.SaveParcels(records, ownerMap);
        }

        private void UpdateExistingRecord(int parcelId, int landOwnerId, BaselineLandParceRecord record)
        {
            var owner = new LandOwner
            {
                LandOwnerId = landOwnerId,
                LandOwnersName = record.LandOwnersName ?? "",
                FatherSpouse = record.FatherSpouse,
                Gender = record.Gender,
                CitizenshipNumber = record.CitizenshipNumber,
                PermanentAddress = record.PermanentAddress
            };
            _repository.UpdateOwner(owner);

            var parcel = new OriginalLandParcel
            {
                ParcelId = parcelId,
                LandOwnerId = landOwnerId,
                ParcelNo = record.ParcelNo ?? "",
                MapSheetNo = record.MapSheetNo ?? "",
                Province = record.Province,
                District = record.District,
                MunicipalityVillage = record.MunicipalityVillage,
                WardNo = record.WardNo,
                ParcelLocation = record.ParcelLocation,
                IsTenant = record.IsTenant,
                LandUse = record.LandUse,
                AreaInSqm = record.AreaInSqm,
                AreaInRAPD = record.AreaInRAPD,
                AreaInBKD = record.AreaInBKD,
                MothNo = record.MothNo,
                PaanaNo = record.PaanaNo,
                Remarks = record.Remarks
            };
            _repository.UpdateParcel(parcel);
        }

        private BaselineLandParceRecord ConvertToEditableRecord(ParcelOwnerDisplayModel model)
        {
            return new BaselineLandParceRecord
            {
                ParcelNo = model.ParcelNo,
                MapSheetNo = model.MapSheetNo,
                Province = model.Province,
                District = model.District,
                MunicipalityVillage = model.MunicipalityVillage,
                LandOwnersName = model.LandOwnersName,
                FatherSpouse = model.FatherSpouse,
                Gender = model.Gender,
                CitizenshipNumber = model.CitizenshipNumber,
                ParcelLocation = model.ParcelLocation,
                PermanentAddress = model.PermanentAddress,
                IsTenant = model.IsTenant,
                LandUse = model.LandUse,
                AreaInSqm = model.AreaInSqm,
                AreaInRAPD = model.AreaInRAPD,
                AreaInBKD = model.AreaInBKD,
                MothNo = model.MothNo,
                PaanaNo = model.PaanaNo,
                Remarks = model.Remarks
            };
        }

        private void UpdateStatusLabels()
        {
            lblTotalRecords.Text = $"Total Records: {_totalRecords}";
            lblFilteredRecords.Text = $"Filtered Records: {_filteredCount}";
            lblSelectedRecords.Text = $"Selected: {dgvRecords.SelectedRows.Count}";
        }

        private static bool TryParseInt(string? value, out int result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = 0;
                return false;
            }
            return int.TryParse(value.Trim(), out result);
        }

        private void ExportToCsv(string filePath)
        {
            using var writer = new StreamWriter(filePath);

            // Header
            writer.WriteLine("Parcel No,Map Sheet,Owner Name,Father/Spouse,Permanent Address," +
                "Citizenship No,Issue Date,Issue District,Gender,Parcel Location," +
                "Area (sqm),Area (RAPD),Area (BKD),Land Use,Tenant,Moth No,Paana No," +
                "Province,District,Municipality,Remarks");

            // Data rows
            foreach (var record in _displayedRecords)
            {
                writer.WriteLine(
                    $"\"{Escape(record.ParcelNo)}\"," +
                    $"\"{Escape(record.MapSheetNo)}\"," +
                    $"\"{Escape(record.LandOwnersName)}\"," +
                    $"\"{Escape(record.FatherSpouse)}\"," +
                    $"\"{Escape(record.PermanentAddress)}\"," +
                    $"\"{Escape(record.CitizenshipNumber)}\"," +
                    $"\"{Escape(record.CitizenshipIssueDate)}\"," +
                    $"\"{Escape(record.citizenshipIssueDistrict)}\"," +
                    $"\"{Escape(record.Gender)}\"," +
                    $"\"{Escape(record.ParcelLocation)}\"," +
                    $"{record.AreaInSqm}," +
                    $"\"{Escape(record.AreaInRAPD)}\"," +
                    $"\"{Escape(record.AreaInBKD)}\"," +
                    $"\"{Escape(record.LandUse)}\"," +
                    $"\"{Escape(record.IsTenant)}\"," +
                    $"\"{Escape(record.MothNo)}\"," +
                    $"\"{Escape(record.PaanaNo)}\"," +
                    $"\"{Escape(record.Province)}\"," +
                    $"\"{Escape(record.District)}\"," +
                    $"\"{Escape(record.MunicipalityVillage)}\"," +
                    $"\"{Escape(record.Remarks)}\"");
            }
        }

        private static string Escape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\"", "\"\"");
        }

        #endregion

        private void cboMapSheet_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void chkAllMapSheets_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void txtParcelNo_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {

        }

        private void grpSearchFields_Enter(object sender, EventArgs e)
        {

        }
    }
}
