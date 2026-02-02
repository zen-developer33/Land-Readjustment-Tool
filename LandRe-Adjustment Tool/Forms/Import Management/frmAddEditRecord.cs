using Land_Readjustment_Tool.Models;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmAddEditRecord : Form
    {
        private bool _isEditMode = false;
        private OriginalLandParcelWithLandOwner _currentRecord;
        private int _recordIndex = -1;

        public OriginalLandParcelWithLandOwner Record { get; private set; }
        public bool IsDeleted { get; private set; } = false;

        // Constructor for ADD mode
        public frmAddEditRecord()
        {
            InitializeComponent();
            SetAddMode();
        }

        // Constructor for EDIT mode
        public frmAddEditRecord(OriginalLandParcelWithLandOwner record, int recordIndex)
        {
            InitializeComponent();
            _currentRecord = record;
            _recordIndex = recordIndex;
            SetEditMode();
            LoadRecordData();
        }

        private void SetAddMode()
        {
            _isEditMode = false;
            this.Text = "Add New Land Owner Record";

            btnAdd.Visible = true;
            btnAdd.Visible = true;
            btnUpdate.Visible = false;
            btnDelete.Visible = false;

            ClearAllFields();
        }

        private void SetEditMode()
        {
            _isEditMode = true;
            this.Text = "Edit Land Owner Record";

            btnAdd.Visible = false;
            btnUpdate.Visible = true;
            btnUpdate.Enabled = true;
            btnDelete.Visible = true;
            btnDelete.Enabled = true;
        }

        private void ClearAllFields()
        {
            txtParcelNo.Clear();
            txtMapSheetNo.Clear();
            txtProvince.Clear();
            txtDistrict.Clear();
            txtMunicipalityVillage.Clear();
            txtLandOwnersName.Clear();
            txtFatherSpouse.Clear();
            cmbGender.SelectedIndex = -1;
            txtCitizenshipNumber.Clear();
            RbtnNo.Checked = true;
            txtAddress.Clear();
            cmbLandUse.SelectedIndex = -1;
            txtAreaInSqm.Clear();
            txtAreaInRAPD.Clear();
            txtAreaInBKD.Clear();
            txtMothNo.Clear();
            txtPaanaNo.Clear();
            txtRemarks.Clear();
        }

        private void LoadRecordData()
        {
            if (_currentRecord == null) return;

            txtParcelNo.Text = _currentRecord.ParcelNo ?? "";
            txtMapSheetNo.Text = _currentRecord.MapSheetNo ?? "";
            txtProvince.Text = _currentRecord.Province ?? "";
            txtDistrict.Text = _currentRecord.District ?? "";
            txtMunicipalityVillage.Text = _currentRecord.MunicipalityVillage ?? "";
            txtLandOwnersName.Text = _currentRecord.LandOwnersName ?? "";
            txtFatherSpouse.Text = _currentRecord.FatherSpouse ?? "";

            // Set gender combobox
            if (!string.IsNullOrWhiteSpace(_currentRecord.Gender))
            {
                int genderIndex = cmbGender.FindStringExact(_currentRecord.Gender);
                cmbGender.SelectedIndex = genderIndex >= 0 ? genderIndex : -1;
            }

            txtCitizenshipNumber.Text = _currentRecord.CitizenshipNumber ?? "";
            RbtnYes.Checked = _currentRecord.IsTenant == "Yes";
            txtAddress.Text = _currentRecord.ParcelLocation ?? "";

            // Set land use combobox
            if (!string.IsNullOrWhiteSpace(_currentRecord.LandUse))
            {
                int landUseIndex = cmbLandUse.FindStringExact(_currentRecord.LandUse);
                cmbLandUse.SelectedIndex = landUseIndex >= 0 ? landUseIndex : -1;
            }

            txtAreaInSqm.Text = _currentRecord.AreaInSqm?.ToString() ?? "";
            txtAreaInRAPD.Text = _currentRecord.AreaInRAPD ?? "";
            txtAreaInBKD.Text = _currentRecord.AreaInBKD ?? "";
            txtMothNo.Text = _currentRecord.MothNo ?? "";
            txtPaanaNo.Text = _currentRecord.PaanaNo ?? "";
            txtRemarks.Text = _currentRecord.Remarks ?? "";
        }

        private OriginalLandParcelWithLandOwner GetRecordFromForm()
        {
            var record = new OriginalLandParcelWithLandOwner
            {
                ParcelNo = txtParcelNo.Text.Trim(),
                MapSheetNo = txtMapSheetNo.Text.Trim(),
                Province = txtProvince.Text.Trim(),
                District = txtDistrict.Text.Trim(),
                MunicipalityVillage = txtMunicipalityVillage.Text.Trim(),
                LandOwnersName = txtLandOwnersName.Text.Trim(),
                FatherSpouse = txtFatherSpouse.Text.Trim(),
                Gender = cmbGender.SelectedItem?.ToString() ?? "",
                CitizenshipNumber = txtCitizenshipNumber.Text.Trim(),
                IsTenant = RbtnNo.Checked ? "No" : RbtnYes.Checked ? "Yes" : "",
                ParcelLocation = txtAddress.Text.Trim(),
                LandUse = cmbLandUse.SelectedItem?.ToString() ?? "",
                AreaInRAPD = txtAreaInRAPD.Text.Trim(),
                AreaInBKD = txtAreaInBKD.Text.Trim(),
                MothNo = txtMothNo.Text.Trim(),
                PaanaNo = txtPaanaNo.Text.Trim(),
                Remarks = txtRemarks.Text.Trim()
            };

            // Parse AreaInSqm
            if (double.TryParse(txtAreaInSqm.Text.Trim(), out double area))
            {
                record.AreaInSqm = area;
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
            if (string.IsNullOrWhiteSpace(txtMapSheetNo.Text))
            {
                _ = MessageBox.Show("Map Sheet No is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = txtMapSheetNo.Focus();
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

        }
    }
}