using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.Data.SQLite;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmAddEditRecord : Form
    {
        private bool _isEditMode = false;
        private bool _ownerFieldsReadOnly = false;
        private BaselineLandParceRecord _currentRecord;
        private int _recordIndex = -1;
        private readonly Func<string?, string?, int?, bool>? _parcelExists;
        private int? _parcelId;
        private LandOwnerRepository? _repository;
        private SQLiteConnection? _connection;
        private List<BaselineLandParceRecord>? _importedRecords;

        public BaselineLandParceRecord Record { get; private set; }
        public bool IsDeleted { get; private set; } = false;

        // Constructor for ADD mode (Import Manager - all fields editable)
        public frmAddEditRecord(Func<string?, string?, int?, bool>? parcelExists = null)
        {
            InitializeComponent();
            _parcelExists = parcelExists;
            _ownerFieldsReadOnly = false;
            InitializeMapSheetLookup();
            ConfigureInputBehavior();
            SetAddMode();
        }

        // Constructor for EDIT mode (basic - all fields editable, no owner lookup source)
        public frmAddEditRecord(BaselineLandParceRecord record, int recordIndex, Func<string?, string?, int?, bool>? parcelExists = null)
        {
            InitializeComponent();
            _currentRecord = record;
            _recordIndex = recordIndex;
            _parcelId = recordIndex;
            _parcelExists = parcelExists;
            _ownerFieldsReadOnly = false;
            InitializeMapSheetLookup();
            ConfigureInputBehavior();
            SetEditMode();
            LoadRecordData();
        }

        // Constructor for EDIT mode (Import Manager - all fields editable, with imported records for owner lookup)
        public frmAddEditRecord(BaselineLandParceRecord record, int recordIndex, Func<string?, string?, int?, bool>? parcelExists, List<BaselineLandParceRecord> importedRecords)
        {
            InitializeComponent();
            _currentRecord = record;
            _recordIndex = recordIndex;
            _parcelId = recordIndex;
            _parcelExists = parcelExists;
            _ownerFieldsReadOnly = false;
            _importedRecords = importedRecords;
            InitializeMapSheetLookup();
            ConfigureInputBehavior();
            SetEditMode();
            LoadRecordData();
        }

        // Constructor for ADD mode (LandParcelRecords - owner fields read-only with lookup)
        public frmAddEditRecord(Func<string?, string?, int?, bool>? parcelExists, bool ownerFieldsReadOnly)
        {
            InitializeComponent();
            _parcelExists = parcelExists;
            _ownerFieldsReadOnly = ownerFieldsReadOnly;
            InitializeMapSheetLookup();
            ConfigureInputBehavior();
            SetAddMode();
        }

        // Constructor for EDIT mode (LandParcelRecords - owner fields read-only with lookup)
        public frmAddEditRecord(BaselineLandParceRecord record, int recordIndex, Func<string?, string?, int?, bool>? parcelExists, bool ownerFieldsReadOnly)
        {
            InitializeComponent();
            _currentRecord = record;
            _recordIndex = recordIndex;
            _parcelId = recordIndex;
            _parcelExists = parcelExists;
            _ownerFieldsReadOnly = ownerFieldsReadOnly;
            InitializeMapSheetLookup();
            ConfigureInputBehavior();
            SetEditMode();
            LoadRecordData();
        }

        private void SetAddMode()
        {
            _isEditMode = false;
            this.Text = "Add New Parcel Record";
            btnAdd.Visible = true;

            btnUpdate.Visible = false;
            btnDelete.Visible = false;

            ClearAllFields();
        }

        private void SetEditMode()
        {
            _isEditMode = true;
            this.Text = "Edit Parcel Record";
            btnAdd.Visible = false;
            btnUpdate.Visible = true;
            btnUpdate.Enabled = true;
            btnDelete.Visible = true;
            btnDelete.Enabled = true;

        }

        private void ConfigureInputBehavior()
        {

            txtAreaInRAPD.ReadOnly = true;
            txtAreaInBKD.ReadOnly = true;
            txtParcelNo.Focus();

            txtAreaInSqm.TextChanged += TxtAreaInSqm_TextChanged;
            txtAreaInSqm.KeyPress += TxtAreaInSqm_KeyPress;
            txtParcelNo.KeyPress += TxtParcelNo_KeyPress;
            FormClosing += FrmAddEditRecord_FormClosing;

            // Always show load button for owner lookup
            btnLoadOwnerDetails.Visible = true;
            btnLoadOwnerDetails.Enabled = true;
            btnLoadOwnerDetails.Click += BtnLoadOwnerDetails_Click;

            // Configure owner fields based on mode
            if (_ownerFieldsReadOnly)
            {
                SetOwnerFieldsReadOnly();
            }
        }

        private void SetOwnerFieldsReadOnly()
        {
            txtLandOwnersName.ReadOnly = true;
            txtFatherSpouse.ReadOnly = true;
            cmbGender.Enabled = false;
            txtCitizenshipNumber.ReadOnly = true;
            txtIssueDistrict.ReadOnly = true;
            txtIssueDate.ReadOnly = true;
            txtPermanentAddress.ReadOnly = true;
            txtTemporaryAddress.ReadOnly = true;
            txtContactNo.ReadOnly = true;
            txtEmailID.ReadOnly = true;
        }

        private void BtnLoadOwnerDetails_Click(object? sender, EventArgs e)
        {
            frmOwnerLookup lookupForm;

            // If we have imported records, use them for owner lookup (Import Manager mode)
            if (_importedRecords != null && _importedRecords.Count > 0)
            {
                lookupForm = new frmOwnerLookup(_importedRecords);
            }
            // Otherwise use the repository (LandParcelRecords mode)
            else if (_repository != null)
            {
                lookupForm = new frmOwnerLookup(_repository);
            }
            else
            {
                MessageBox.Show("No owner data available for lookup.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (lookupForm)
            {
                if (lookupForm.ShowDialog() == DialogResult.OK && lookupForm.SelectedOwner != null)
                {
                    LoadOwnerToForm(lookupForm.SelectedOwner);
                }
            }
        }

        private void LoadOwnerToForm(LandOwner owner)
        {
            txtLandOwnersName.Text = owner.LandOwnersName ?? "";
            txtFatherSpouse.Text = owner.FatherSpouse ?? "";

            if (!string.IsNullOrWhiteSpace(owner.Gender))
            {
                int genderIndex = cmbGender.FindStringExact(owner.Gender);
                cmbGender.SelectedIndex = genderIndex >= 0 ? genderIndex : -1;
            }
            else
            {
                cmbGender.SelectedIndex = -1;
            }

            txtCitizenshipNumber.Text = owner.CitizenshipNumber ?? "";
            txtIssueDistrict.Text = owner.CitizenshipIssuedDistrict ?? "";
            txtIssueDate.Text = owner.CitizenshipIssuedDate ?? "";
            txtPermanentAddress.Text = owner.PermanentAddress ?? "";
            txtTemporaryAddress.Text = owner.TemporaryAddress ?? "";
            txtContactNo.Text = owner.ContactNumber ?? "";
            txtEmailID.Text = owner.EmailID ?? "";
        }

        private void InitializeMapSheetLookup()
        {
            cbMapSheetNo.DropDownStyle = ComboBoxStyle.DropDown;
            cbMapSheetNo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbMapSheetNo.AutoCompleteSource = AutoCompleteSource.ListItems;

            if (CurrentProject.Info == null || string.IsNullOrWhiteSpace(CurrentProject.Info.ProjectPath))
            {
                return;
            }

            try
            {
                var dbHelper = new DatabaseHelper(CurrentProject.Info.ProjectPath);
                dbHelper.InitializeDatabase();
                _connection = dbHelper.GetConnection();
                _repository = new LandOwnerRepository(_connection);

                var mapSheets = _repository.GetUniqueMapSheets();
                cbMapSheetNo.BeginUpdate();
                cbMapSheetNo.Items.Clear();
                foreach (var mapSheet in mapSheets)
                {
                    cbMapSheetNo.Items.Add(mapSheet);
                }
                cbMapSheetNo.EndUpdate();
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Failed to load map sheets: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearAllFields()
        {
            // Parcel Identification
            txtParcelNo.Clear();
            cbMapSheetNo.SelectedIndex = -1;
            cbMapSheetNo.Text = string.Empty;

            // Administrative Information
            txtProvince.Clear();
            txtDistrict.Clear();
            txtMunicipalityVillage.Clear();
            txtWardNo.Clear();

            // Owner Information
            txtLandOwnersName.Clear();
            txtFatherSpouse.Clear();
            cmbGender.SelectedIndex = -1;
            txtCitizenshipNumber.Clear();
            txtIssueDistrict.Clear();
            txtIssueDate.Clear();

            // Address & Contact
            txtPermanentAddress.Clear();
            txtTemporaryAddress.Clear();
            txtContactNo.Clear();
            txtEmailID.Clear();

            // Other Parcel Information
            txtTenant.Clear();
            cbOwnershipType.SelectedIndex = -1;
            cmbLandUse.SelectedIndex = -1;

            // Area Information
            txtAreaInSqm.Clear();
            txtAreaInRAPD.Clear();
            txtAreaInBKD.Clear();

            // Registry References
            txtMothNo.Clear();
            txtPaanaNo.Clear();

            // Remarks
            txtRemarks.Clear();
        }

        private void LoadRecordData()
        {
            if (_currentRecord == null) return;

            // Parcel Identification
            txtParcelNo.Text = _currentRecord.ParcelNo ?? "";
            if (!string.IsNullOrWhiteSpace(_currentRecord.MapSheetNo))
            {
                int mapSheetIndex = cbMapSheetNo.FindStringExact(_currentRecord.MapSheetNo);
                cbMapSheetNo.SelectedIndex = mapSheetIndex >= 0 ? mapSheetIndex : -1;
                if (mapSheetIndex < 0)
                {
                    cbMapSheetNo.Text = _currentRecord.MapSheetNo;
                }
            }
            else
            {
                cbMapSheetNo.SelectedIndex = -1;
            }

            // Administrative Information
            txtProvince.Text = _currentRecord.Province ?? "";
            txtDistrict.Text = _currentRecord.District ?? "";
            txtMunicipalityVillage.Text = _currentRecord.MunicipalityVillage ?? "";
            txtWardNo.Text = _currentRecord.WardNo ?? "";

            // Owner Information
            txtLandOwnersName.Text = _currentRecord.LandOwnersName ?? "";
            txtFatherSpouse.Text = _currentRecord.FatherSpouse ?? "";

            if (!string.IsNullOrWhiteSpace(_currentRecord.Gender))
            {
                int genderIndex = cmbGender.FindStringExact(_currentRecord.Gender);
                cmbGender.SelectedIndex = genderIndex >= 0 ? genderIndex : -1;
            }

            // Citizenship Information
            txtCitizenshipNumber.Text = _currentRecord.CitizenshipNumber ?? "";
            txtIssueDistrict.Text = _currentRecord.CitizenshipIssuedDistrict ?? "";
            txtIssueDate.Text = _currentRecord.citizenshipIssuedDate ?? "";

            // Address & Contact
            txtPermanentAddress.Text = _currentRecord.PermanentAddress ?? "";
            txtTemporaryAddress.Text = _currentRecord.TempoaryAddress ?? "";
            txtContactNo.Text = _currentRecord.ContactNumber ?? "";
            txtEmailID.Text = _currentRecord.EmailID ?? "";

            // Other Parcel Information
            txtTenant.Text = _currentRecord.Tenant ?? "";

            if (!string.IsNullOrWhiteSpace(_currentRecord.LandOwnershipType))
            {
                int ownershipIndex = cbOwnershipType.FindStringExact(_currentRecord.LandOwnershipType);
                cbOwnershipType.SelectedIndex = ownershipIndex >= 0 ? ownershipIndex : -1;
                if (ownershipIndex < 0)
                    cbOwnershipType.Text = _currentRecord.LandOwnershipType;
            }

            if (!string.IsNullOrWhiteSpace(_currentRecord.LandUse))
            {
                int landUseIndex = cmbLandUse.FindStringExact(_currentRecord.LandUse);
                cmbLandUse.SelectedIndex = landUseIndex >= 0 ? landUseIndex : -1;
            }

            // Area Information
            txtAreaInSqm.Text = _currentRecord.AreaInSqm?.ToString() ?? "";
            txtAreaInRAPD.Text = _currentRecord.AreaInRAPD ?? "";
            txtAreaInBKD.Text = _currentRecord.AreaInBKD ?? "";

            // Registry References
            txtMothNo.Text = _currentRecord.MothNo ?? "";
            txtPaanaNo.Text = _currentRecord.PaanaNo ?? "";

            // Remarks
            txtRemarks.Text = _currentRecord.Remarks ?? "";
        }

        private BaselineLandParceRecord GetRecordFromForm()
        {
            var record = new BaselineLandParceRecord
            {
                // Parcel Identification
                ParcelNo = txtParcelNo.Text.Trim(),
                MapSheetNo = cbMapSheetNo.Text.Trim(),

                // Administrative Information
                Province = txtProvince.Text.Trim(),
                District = txtDistrict.Text.Trim(),
                MunicipalityVillage = txtMunicipalityVillage.Text.Trim(),
                WardNo = txtWardNo.Text.Trim(),

                // Owner Information
                LandOwnersName = txtLandOwnersName.Text.Trim(),
                FatherSpouse = txtFatherSpouse.Text.Trim(),
                Gender = cmbGender.SelectedItem?.ToString() ?? "",

                // Citizenship Information
                CitizenshipNumber = txtCitizenshipNumber.Text.Trim(),
                CitizenshipIssuedDistrict = txtIssueDistrict.Text.Trim(),
                citizenshipIssuedDate = txtIssueDate.Text.Trim(),

                // Address & Contact
                PermanentAddress = txtPermanentAddress.Text.Trim(),
                TempoaryAddress = txtTemporaryAddress.Text.Trim(),
                ContactNumber = txtContactNo.Text.Trim(),
                EmailID = txtEmailID.Text.Trim(),

                // Other Parcel Information
                Tenant = txtTenant.Text.Trim(),
                LandOwnershipType = cbOwnershipType.SelectedItem?.ToString() ?? cbOwnershipType.Text.Trim(),
                LandUse = cmbLandUse.SelectedItem?.ToString() ?? "",

                // Registry References
                MothNo = txtMothNo.Text.Trim(),
                PaanaNo = txtPaanaNo.Text.Trim(),

                // Remarks
                Remarks = txtRemarks.Text.Trim()
            };

            // Parse AreaInSqm and auto-calculate RAPD and BKD
            if (double.TryParse(txtAreaInSqm.Text.Trim(), out double area))
            {
                record.AreaInSqm = area;
                record.AreaInRAPD = AreaConverterService.SqmToRAPDString(area);
                record.AreaInBKD = AreaConverterService.SqmToBKDString(area);
            }

            return record;
        }

        private bool ValidateRecord()
        {
            // Required: ParcelNo
            if (string.IsNullOrWhiteSpace(txtParcelNo.Text))
            {
                _ = MessageBox.Show("Parcel No is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = txtParcelNo.Focus();
                return false;
            }

            // Required: MapSheetNo
            if (string.IsNullOrWhiteSpace(cbMapSheetNo.Text))
            {
                _ = MessageBox.Show("Map Sheet No is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = cbMapSheetNo.Focus();
                return false;
            }

            // Required: AreaInSqm must be a valid number > 0
            if (!double.TryParse(txtAreaInSqm.Text.Trim(), out double area) || area <= 0)
            {
                _ = MessageBox.Show("Area (sq.m) must be a valid number greater than 0.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = txtAreaInSqm.Focus();
                return false;
            }

            if (_parcelExists != null)
            {
                string parcelNo = txtParcelNo.Text.Trim();
                string mapSheetNo = cbMapSheetNo.Text.Trim();
                if (_parcelExists(parcelNo, mapSheetNo, _parcelId))
                {
                    _ = MessageBox.Show(
                        $"The Parcel No.: {parcelNo} already exists in the mapsheet: {mapSheetNo}.",
                        "Duplicate Parcel",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    _ = txtParcelNo.Focus();
                    return false;
                }
            }

            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateRecord())
                return;

            Record = GetRecordFromForm();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateRecord())
                return;

            DialogResult result = MessageBox.Show(
                "Are you sure you want to update this record?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Record = GetRecordFromForm();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this record?\n\nThis action cannot be undone.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                IsDeleted = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void frmAddEditRecord_Load(object sender, EventArgs e)
        {
            txtParcelNo.Focus();
        }

        private void TxtAreaInSqm_TextChanged(object? sender, EventArgs e)
        {
            if (double.TryParse(txtAreaInSqm.Text.Trim(), out double area) && area > 0)
            {
                txtAreaInRAPD.Text = AreaConverterService.SqmToRAPDString(area);
                txtAreaInBKD.Text = AreaConverterService.SqmToBKDString(area);
            }
            else
            {
                txtAreaInRAPD.Clear();
                txtAreaInBKD.Clear();
            }
        }

        private void TxtAreaInSqm_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (char.IsDigit(e.KeyChar))
                return;

            if (e.KeyChar == '.' && sender is TextBox textBox && !textBox.Text.Contains('.'))
                return;

            e.Handled = true;
        }

        private void TxtParcelNo_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
                return;

            if (char.IsDigit(e.KeyChar))
                return;

            e.Handled = true;
        }

        private void FrmAddEditRecord_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK || DialogResult == DialogResult.Cancel)
                return;

            var result = MessageBox.Show(
                "Do you want to save changes before closing?",
                "Save Changes",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (result == DialogResult.No)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            if (ValidateRecord())
            {
                Record = GetRecordFromForm();
                DialogResult = DialogResult.OK;
                return;
            }

            e.Cancel = true;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnLoadOwnerDetails_Click_1(object sender, EventArgs e)
        {

        }
    }
}