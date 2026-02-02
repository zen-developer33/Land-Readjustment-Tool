using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;
using System.Data;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Main form for viewing, editing, and managing land parcel ownership records
    /// </summary>
    public partial class frmLandownerRecordsManager : Form
    {
        private readonly string _projectPath;
        private LandOwnerRepository _repository;
        private List<ParcelOwnerDisplayModel> _allRecords;
        private BindingList<ParcelOwnerDisplayModel> _displayedRecords;
        private int _totalRecords = 0;

        public frmLandownerRecordsManager(string projectPath)
        {
            InitializeComponent();
            _projectPath = projectPath;
            _allRecords = new List<ParcelOwnerDisplayModel>();
            _displayedRecords = new BindingList<ParcelOwnerDisplayModel>();

            this.Text = "Land Parcel Ownership Records";

            InitializeRepository();
            InitializeDataGridView();
            LoadAllRecords();

        }

        private void InitializeRepository()
        {
            var dbHelper = new DatabaseHelper(_projectPath);
            dbHelper.InitializeDatabase();
            var connection = dbHelper.GetConnection();

            // Ensure schema exists and is correct
            if (!LandOwnerDatabaseSchema.HasCorrectSchema(connection))
            {
                LandOwnerDatabaseSchema.RecreateSchema(connection);
            }
            else
            {
                LandOwnerDatabaseSchema.CreateSchema(connection);
            }

            _repository = new LandOwnerRepository(connection);
        }

        private void InitializeDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.MultiSelect = true;
            dgvRecords.ReadOnly = true;
            dgvRecords.DoubleBuffered(true);
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);
            dgvRecords.RowHeadersVisible = false;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.GridColor = Color.FromArgb(220, 220, 220);

            // Clear existing columns
            dgvRecords.Columns.Clear();

            // Add columns for parcel and owner data
            AddColumn("SN", "SN", 45);
            AddColumn("ParcelNo", "Parcel No", 80);
            AddColumn("MapSheetNo", "Map Sheet", 85);
            AddColumn("LandOwnersName", "Owner Name", 160);
            AddColumn("FatherSpouse", "Father/Spouse", 140);
            AddColumn("CitizenshipNumber", "Citizenship No", 115);
            AddColumn("Gender", "Gender", 65);
            AddColumn("ParcelLocation", "Parcel Location", 140);
            AddColumn("AreaInSqm", "Area (sqm)", 90);
            AddColumn("AreaInRAPD", "Area (RAPD)", 90);
            AddColumn("AreaInBKD", "Area (BKD)", 90);
            AddColumn("LandUse", "Land Use", 90);
            AddColumn("IsTenant", "Tenant", 60);
            AddColumn("MothNo", "Moth No", 70);
            AddColumn("PaanaNo", "Paana No", 70);
            AddColumn("Province", "Province", 80);
            AddColumn("District", "District", 80);
            AddColumn("MunicipalityVillage", "Municipality", 100);
            AddColumn("Remarks", "Remarks", 120);

            // Make headers styled
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecords.ColumnHeadersHeight = 34;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvRecords.RowTemplate.Height = 28;

            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            dgvRecords.CellDoubleClick += DgvRecords_CellDoubleClick;
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

        private void LoadAllRecords()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Get all parcels with owner info
                var parcels = _repository.GetAllParcelsWithOwners();

                // Convert to display models
                _allRecords = new List<ParcelOwnerDisplayModel>();
                int sn = 1;
                foreach (var parcel in parcels)
                {
                    _allRecords.Add(new ParcelOwnerDisplayModel
                    {
                        SN = sn++,
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

                // Bind all records to the grid
                _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(_allRecords);
                dgvRecords.DataSource = _displayedRecords;

                UpdateStatusLabel();
                UpdateRecordCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                _allRecords = new List<ParcelOwnerDisplayModel>();
                _displayedRecords = new BindingList<ParcelOwnerDisplayModel>();
                _totalRecords = 0;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateStatusLabel()
        {
            if (_totalRecords == 0)
            {
                lblPaginationInfo.Text = "No records found. Import data first.";
            }
            else
            {
                lblPaginationInfo.Text = $"Showing all {_totalRecords} records";
            }
        }

        private void DgvRecords_SelectionChanged(object? sender, EventArgs e)
        {
            bool hasSelection = dgvRecords.SelectedRows.Count > 0;
            bool singleSelection = dgvRecords.SelectedRows.Count == 1;

            btnEdit.Enabled = singleSelection;
            btnDelete.Enabled = hasSelection;

        }

        private void DgvRecords_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            EditSelectedRecord();
        }

        // ==================== TOOLBAR HANDLERS ====================
        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var addForm = new frmAddEditRecord())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Save the new record to database
                        var record = addForm.Record;
                        SaveNewRecord(record);

                        LoadAllRecords();
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
        }

        private void SaveNewRecord(OriginalLandParcelWithLandOwner record)
        {
            // First, create or get the owner
            var owner = new LandOwner
            {
                LandOwnersName = record.LandOwnersName ?? "Unknown",
                FatherSpouse = record.FatherSpouse,
                Gender = record.Gender,
                CitizenshipNumber = record.CitizenshipNumber,
                PermanentAddress = record.PermanentAddress,
                CreatedDate = DateTime.Now
            };

            // Use the deduplication service to find or create owner
            var records = new List<OriginalLandParcelWithLandOwner> { record };
            var ownerMap = _repository.ExtractAndSaveUniqueOwners(records);

            // Save the parcel (ParcelLocation is saved with the parcel, not owner)
            _repository.SaveParcels(records, ownerMap);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1) return;
            EditSelectedRecord();
        }

        private void EditSelectedRecord()
        {
            var model = dgvRecords.SelectedRows[0].DataBoundItem as ParcelOwnerDisplayModel;
            if (model == null) return;

            // Convert to editable record
            var record = ConvertToEditableRecord(model);

            using (var editForm = new frmAddEditRecord(record, model.ParcelId))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (editForm.IsDeleted)
                        {
                            // Delete the record
                            _repository.DeleteParcel(model.ParcelId);
                            LoadAllRecords();
                            MessageBox.Show("Record deleted successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            // Update the record in database
                            var updatedRecord = editForm.Record;
                            UpdateExistingRecord(model.ParcelId, model.LandOwnerId, updatedRecord);

                            LoadAllRecords();
                            MessageBox.Show("Record updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update record: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void UpdateExistingRecord(int parcelId, int landOwnerId, OriginalLandParcelWithLandOwner record)
        {
            // Update owner info (without ParcelLocation - it belongs to parcel)
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

            // Update parcel info (including ParcelLocation)
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

        private void btnDelete_Click(object sender, EventArgs e)
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
                MessageBox.Show($"{deletedCount} record(s) deleted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFindDuplicates_Click(object sender, EventArgs e)
        {
            // Find duplicate parcels (same ParcelNo + MapSheetNo)
            var duplicates = _allRecords
                .GroupBy(r => new { r.ParcelNo, r.MapSheetNo })
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .ToList();

            if (duplicates.Count == 0)
            {
                MessageBox.Show("No duplicate parcel records found.", "Find Duplicates",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show only duplicates
            _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(duplicates);
            dgvRecords.DataSource = _displayedRecords;

            lblPaginationInfo.Text = $"Found {duplicates.Count} duplicate records";

            MessageBox.Show($"Found {duplicates.Count} duplicate parcel records.\n\n" +
                "These are shown in the grid. Click Refresh to see all records.",
                "Duplicates Found",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(_allRecords);
                dgvRecords.DataSource = _displayedRecords;
                UpdateStatusLabel();
                return;
            }

            var filtered = _allRecords.Where(r =>
                (r.ParcelNo?.ToLower().Contains(searchTerm) ?? false) ||
                (r.MapSheetNo?.ToLower().Contains(searchTerm) ?? false) ||
                (r.LandOwnersName?.ToLower().Contains(searchTerm) ?? false) ||
                (r.FatherSpouse?.ToLower().Contains(searchTerm) ?? false) ||
                (r.CitizenshipNumber?.ToLower().Contains(searchTerm) ?? false) ||
                (r.ParcelLocation?.ToLower().Contains(searchTerm) ?? false) ||
                (r.PermanentAddress?.ToLower().Contains(searchTerm) ?? false) ||
                (r.MothNo?.ToLower().Contains(searchTerm) ?? false)
            ).ToList();

            _displayedRecords = new BindingList<ParcelOwnerDisplayModel>(filtered);
            dgvRecords.DataSource = _displayedRecords;

            lblPaginationInfo.Text = $"Found {filtered.Count} matching records";
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadAllRecords();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // Not used
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            // Not used
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            // Same as edit
            EditSelectedRecord();
        }

        private void UpdateRecordCount()
        {
            lblTotalRecords.Text = $"Total Records: {_totalRecords}";
        }

        private OriginalLandParcelWithLandOwner ConvertToEditableRecord(ParcelOwnerDisplayModel model)
        {
            return new OriginalLandParcelWithLandOwner
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

   

        private void frmLandownerRecordsManager_Load(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Display model for parcel ownership records
    /// </summary>
    public class ParcelOwnerDisplayModel
    {
        public int SN { get; set; }
        public int ParcelId { get; set; }
        public int LandOwnerId { get; set; }
        
        // Parcel fields
        public string ParcelNo { get; set; } = "";
        public string MapSheetNo { get; set; } = "";
        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string MunicipalityVillage { get; set; } = "";
        public double? AreaInSqm { get; set; }
        public string AreaInRAPD { get; set; } = "";
        public string AreaInBKD { get; set; } = "";
        public string LandUse { get; set; } = "";
        public string IsTenant { get; set; } = "";
        public string MothNo { get; set; } = "";
        public string PaanaNo { get; set; } = "";
        public string Remarks { get; set; } = "";
        
        // Owner fields
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string Gender { get; set; } = "";
        public string ParcelLocation { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
    }
}
