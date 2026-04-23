using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.LandData;

namespace Land_Readjustment_Tool.Forms.Land_Owners_Record
{
    public partial class frmLandOwnerDetails : Form
    {
        private readonly int _landOwnerId;
        private bool _readOnlyMode;
        private readonly bool _isAddMode;
        private readonly LandRecordsService _landRecordsService;
        private readonly OwnerFileStorageService _ownerFileStorageService;
        private readonly string _projectPath;
        private readonly bool _allowEditInReadOnly;
        private LandOwner? _owner;
        private string? _tempPhotoPath; // Temporary photo path for add mode
        private bool _hasChanges;

        /// <summary>
        /// Constructor for viewing/editing existing owner
        /// </summary>
        public frmLandOwnerDetails(int landOwnerId, bool readOnlyMode)
            : this(
                landOwnerId,
                readOnlyMode,
                CreateDefaultLandRecordsService(out var projectPath),
                projectPath)
        {
        }

        /// <summary>
        /// Constructor for viewing/editing existing owner with injected dependencies.
        /// </summary>
        public frmLandOwnerDetails(
            int landOwnerId,
            bool readOnlyMode,
            LandRecordsService landRecordsService,
            string projectPath)
            : this(landOwnerId, readOnlyMode, landRecordsService, projectPath, allowEditInReadOnly: true)
        {
        }

        /// <summary>
        /// Constructor for viewing/editing existing owner with injected dependencies.
        /// </summary>
        public frmLandOwnerDetails(
            int landOwnerId,
            bool readOnlyMode,
            LandRecordsService landRecordsService,
            string projectPath,
            bool allowEditInReadOnly)
        {
            InitializeComponent();
            _landOwnerId = landOwnerId;
            _readOnlyMode = readOnlyMode;
            _isAddMode = false;
            _projectPath = projectPath;
            _landRecordsService = landRecordsService;
            _ownerFileStorageService = new OwnerFileStorageService(_projectPath);
            _allowEditInReadOnly = allowEditInReadOnly;

            LoadOwnerDetails();
            if (_owner == null)
            {
                return;
            }
            LoadOwnerSummary();
            SetReadOnlyMode(_readOnlyMode);
        }

        /// <summary>
        /// Constructor for adding new owner
        /// </summary>
        public frmLandOwnerDetails()
            : this(CreateDefaultLandRecordsService(out var projectPath), projectPath)
        {
        }

        /// <summary>
        /// Constructor for adding new owner with injected dependencies.
        /// </summary>
        public frmLandOwnerDetails(LandRecordsService landRecordsService, string projectPath)
        {
            InitializeComponent();
            _landOwnerId = 0;
            _readOnlyMode = false;
            _isAddMode = true;
            _projectPath = projectPath;
            _landRecordsService = landRecordsService;
            _ownerFileStorageService = new OwnerFileStorageService(_projectPath);
            _allowEditInReadOnly = false;

            SetAddMode();
        }

        private static LandRecordsService CreateDefaultLandRecordsService(out string projectPath)
        {
            if (!AppServices.HasContext)
                throw new InvalidOperationException("No open project context found.");

            projectPath = AppServices.Context.ProjectFilePath;
            return new LandRecordsService(AppServices.Context.Session, projectPath);
        }

        private void SetAddMode()
        {
            Text = "Add New Land Owner";

            // Clear all fields
            txtFullName.Text = "";
            txtFatherSpouse.Text = "";
            cbGender.SelectedIndex = -1;
            cbGender.Text = "";
            txtCitizenshipNo.Text = "";
            txtIssueDistrict.Text = "";
            txtIssueDate.Text = "";
            txtPermanentAddress.Text = "";
            txtTemporaryAddress.Text = "";
            txtContactNumber.Text = "";
            txtEmailID.Text = "";
            ReplacePhotoPreview(null);

            // Update summary labels to show no parcels
            lblParcelCount.Text = "-";
            lblAreasqm.Text = "-";
            lblAreaLocal.Text = "-";

            // Disable parcel and document buttons (can't view for new owner)
            btnViewParcels.Text = "View Parcels Owned (0)";
            btnViewParcels.Enabled = false;

            btnAttachViewDocuments.Text = "Attach/View Documents (0)";
            btnAttachViewDocuments.Enabled = false;


            // Show save and cancel buttons
            btnSave.Text = "Add Owner";
            btnSave.Visible = true;
            btnCancel.Visible = true;

            // Enable photo upload in add mode
            btnUploadChangePhoto.Enabled = true;
        }

        private void LoadOwnerDetails()
        {
            _owner = _landRecordsService.GetOwnerById(_landOwnerId);
            if (_owner == null)
            {
                _ = MessageBox.Show("Owner not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // Populate controls
            txtFullName.Text = _owner.LandOwnersName ?? "";
            txtFatherSpouse.Text = _owner.FatherSpouse ?? "";
            SetGenderValue(_owner.Gender);
            txtCitizenshipNo.Text = _owner.CitizenshipNumber ?? "";
            txtIssueDistrict.Text = _owner.CitizenshipIssuedDistrict ?? "";
            txtIssueDate.Text = _owner.CitizenshipIssuedDate ?? "";
            txtPermanentAddress.Text = _owner.PermanentAddress ?? "";
            txtTemporaryAddress.Text = _owner.TemporaryAddress ?? "";
            txtContactNumber.Text = _owner.ContactNumber ?? "";
            txtEmailID.Text = _owner.EmailID ?? "";

            // Load photo if available
            LoadPhoto();
        }

        private void LoadPhoto()
        {
            var image = _ownerFileStorageService.LoadPhotoFromStoredPath(_owner?.PhotoPath);
            ReplacePhotoPreview(image);
        }

        private void ReplacePhotoPreview(Image? image)
        {
            var previousImage = picPhoto.Image;
            picPhoto.Image = image;
            picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            previousImage?.Dispose();
        }

        private void SetGenderValue(string? gender)
        {
            cbGender.SelectedIndex = -1;
            var value = gender?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value))
            {
                cbGender.Text = string.Empty;
                return;
            }

            var index = cbGender.FindStringExact(value);
            if (index >= 0)
            {
                cbGender.SelectedIndex = index;
                return;
            }

            cbGender.Items.Add(value);
            cbGender.SelectedIndex = cbGender.FindStringExact(value);
        }

        private void LoadOwnerSummary()
        {
            int parcelCount = _landRecordsService.GetParcelsByOwnerId(_landOwnerId).Count;
            double totalAreaSqm = _landRecordsService.GetTotalAreaByOwnerId(_landOwnerId);
            int documentCount = _landRecordsService.GetDocumentCountByOwnerId(_landOwnerId);
            string traditionalUnit = _landRecordsService.GetTraditionalAreaUnit();

            // Update summary labels
            if (parcelCount == 0)
            {
                lblParcelCount.Text = "-";
                lblAreasqm.Text = "-";
                lblAreaLocal.Text = "-";
            }
            else
            {
                string areaTraditional = string.Equals(traditionalUnit, "BKD", StringComparison.OrdinalIgnoreCase)
                    ? AreaConverterService.SqmToBKDString(totalAreaSqm)
                    : AreaConverterService.SqmToRAPDString(totalAreaSqm);
                lblParcelCount.Text = parcelCount.ToString();
                lblAreasqm.Text = $"{totalAreaSqm:F2} sq.m";
                lblAreaLocal.Text = areaTraditional;
            }

            // Update button texts with counts
            btnViewParcels.Text = $"View Parcels Owned ({parcelCount})";
            btnAttachViewDocuments.Text = $"Attach/View Documents ({documentCount})";
            btnViewParcels.Enabled = true;
            btnAttachViewDocuments.Enabled = true;
        }

        private void SetReadOnlyMode(bool readOnly)
        {

            if (readOnly)
            {
                // Readonly mode - disable all input controls

                txtFullName.ReadOnly = true;
                txtFatherSpouse.ReadOnly = true;
                cbGender.Enabled = false;
                txtCitizenshipNo.ReadOnly = true;
                txtIssueDistrict.ReadOnly = true;
                txtIssueDate.ReadOnly = true;
                txtPermanentAddress.ReadOnly = true;
                txtTemporaryAddress.ReadOnly = true;
                txtContactNumber.ReadOnly = true;
                txtEmailID.ReadOnly = true;

                // Change background to indicate readonly
                Color readOnlyColor = Color.FromArgb(240, 240, 240);
                txtFullName.BackColor = readOnlyColor;
                txtFatherSpouse.BackColor = readOnlyColor;
                txtCitizenshipNo.BackColor = readOnlyColor;
                txtIssueDistrict.BackColor = readOnlyColor;
                txtIssueDate.BackColor = readOnlyColor;
                txtPermanentAddress.BackColor = readOnlyColor;
                txtTemporaryAddress.BackColor = readOnlyColor;
                txtContactNumber.BackColor = readOnlyColor;
                txtEmailID.BackColor = readOnlyColor;

                // Hide edit buttons
                btnSave.Enabled = false;
                btnCancel.Enabled = false;
                btnUploadChangePhoto.Enabled = false;
                chkEdit.Visible = _allowEditInReadOnly;
                chkEdit.Enabled = _allowEditInReadOnly;

                Text = "Land Owner Details (Read-Only)";
                _readOnlyMode = true;
            }
            else
            {
                enableEditing();
                chkEdit.Visible = _allowEditInReadOnly;
                chkEdit.Enabled = _allowEditInReadOnly;
            }
        }

        private void pnlPhoto_Paint(object sender, PaintEventArgs e)
        {
            // Not used
        }

        private void label12_Click(object sender, EventArgs e)
        {
            // Not used
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                _ = MessageBox.Show("Full Name is required. Please enter the owner's name.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ = txtFullName.Focus();
                return;
            }

            // Confirm before saving
            string confirmMessage = _isAddMode
                ? $"Are you sure you want to add this land owner?\n\nName: {txtFullName.Text.Trim()}"
                : $"Are you sure you want to save changes to this land owner?\n\nName: {txtFullName.Text.Trim()}";

            var confirmResult = MessageBox.Show(confirmMessage,
                _isAddMode ? "Confirm Add Owner" : "Confirm Save Changes",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (_isAddMode)
                {
                    // Check for duplicate owner
                    if (_landRecordsService.OwnerExists(txtFullName.Text.Trim(), txtFatherSpouse.Text.Trim(), txtCitizenshipNo.Text.Trim()))
                    {
                        var result = MessageBox.Show(
                            "An owner with the same name, father/spouse, and citizenship number already exists.\n\nDo you want to add this owner anyway?",
                            "Possible Duplicate",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result != DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    // Create new owner
                    var newOwner = new LandOwner
                    {
                        LandOwnersName = txtFullName.Text.Trim(),
                        FatherSpouse = txtFatherSpouse.Text.Trim(),
                        Gender = cbGender.Text.Trim(),
                        CitizenshipNumber = txtCitizenshipNo.Text.Trim(),
                        CitizenshipIssuedDistrict = txtIssueDistrict.Text.Trim(),
                        CitizenshipIssuedDate = txtIssueDate.Text.Trim(),
                        PermanentAddress = txtPermanentAddress.Text.Trim(),
                        TemporaryAddress = txtTemporaryAddress.Text.Trim(),
                        ContactNumber = txtContactNumber.Text.Trim(),
                        EmailID = txtEmailID.Text.Trim(),
                        IsAnonymous = false,
                        CreatedDate = DateTime.Now
                    };

                    int newOwnerId = _landRecordsService.CreateOwner(newOwner);

                    // Save photo if one was selected in add mode
                    if (!string.IsNullOrWhiteSpace(_tempPhotoPath) && File.Exists(_tempPhotoPath))
                    {
                        try
                        {
                            string relativePath = _ownerFileStorageService.SaveOwnerPhoto(newOwnerId, _tempPhotoPath);
                            _landRecordsService.UpdateOwnerPhotoPath(newOwnerId, relativePath);
                        }
                        catch (Exception photoEx)
                        {
                            MessageBox.Show(
                                $"Owner was created, but the photo could not be saved: {photoEx.Message}",
                                "Photo Not Saved",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }

                    _ = MessageBox.Show($"Land owner added successfully!\n\nOwner ID: {newOwnerId}\n\nYou can assign parcels to this owner from the Land Parcel Records form.",
                        "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _hasChanges = true;
                    DialogResult = DialogResult.OK;
                    Close();

                }
                else
                {
                    // Edit mode - existing logic
                    if (_owner == null) return;

                    // Check for duplicate owner (excluding current owner)
                    if (_landRecordsService.OwnerExists(txtFullName.Text.Trim(), txtFatherSpouse.Text.Trim(), txtCitizenshipNo.Text.Trim(), _landOwnerId))
                    {
                        var result = MessageBox.Show(
                            "An owner with the same name, father/spouse, and citizenship number already exists.\n\nDo you want to save these changes anyway?",
                            "Possible Duplicate",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result != DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    // Update owner object with form values
                    _owner.LandOwnersName = txtFullName.Text.Trim();
                    _owner.FatherSpouse = txtFatherSpouse.Text.Trim();
                    _owner.Gender = cbGender.Text.Trim();
                    _owner.CitizenshipNumber = txtCitizenshipNo.Text.Trim();
                    _owner.CitizenshipIssuedDistrict = txtIssueDistrict.Text.Trim();
                    _owner.CitizenshipIssuedDate = txtIssueDate.Text.Trim();
                    _owner.PermanentAddress = txtPermanentAddress.Text.Trim();
                    _owner.TemporaryAddress = txtTemporaryAddress.Text.Trim();
                    _owner.ContactNumber = txtContactNumber.Text.Trim();
                    _owner.EmailID = txtEmailID.Text.Trim();
                    _owner.ModifiedDate = DateTime.Now;

                    // Save to database
                    _ = _landRecordsService.UpdateOwner(_owner);

                    _ = MessageBox.Show("Owner details updated successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    _hasChanges = true;
                    SetReadOnlyMode(true);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Failed to save owner details: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            enableEditing();
        }

        private void enableEditing()
        {
            // Readonly mode - disable all input controls

            txtFullName.ReadOnly = false;
            txtFatherSpouse.ReadOnly = false;
            cbGender.Enabled = true;
            txtCitizenshipNo.ReadOnly = false;
            txtIssueDistrict.ReadOnly = false;
            txtIssueDate.ReadOnly = false;
            txtPermanentAddress.ReadOnly = false;
            txtTemporaryAddress.ReadOnly = false;
            txtContactNumber.ReadOnly = false;
            txtEmailID.ReadOnly = false;
            // Change background to indicate readonly
            Color editColor = Color.FromKnownColor(KnownColor.Window);
            txtFullName.BackColor = Color.FromKnownColor(KnownColor.Window);
            txtFatherSpouse.BackColor = editColor;
            txtCitizenshipNo.BackColor = editColor;
            txtIssueDistrict.BackColor = editColor;
            txtIssueDate.BackColor = editColor;
            txtPermanentAddress.BackColor = editColor;
            txtTemporaryAddress.BackColor = editColor;
            txtContactNumber.BackColor = editColor;
            txtEmailID.BackColor = editColor;
            // Hide edit buttons
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnUploadChangePhoto.Enabled = true;
            _readOnlyMode = false;
            //chkEdit.Text = "Stop Editing";
            this.Text = "Land Owner Details";
        }


        private void btnAttachViewDocuments_Click(object sender, EventArgs e)
        {
            if (_owner == null) return;

            using var docsForm = new frmOwnerDocuments(
                _owner,
                _landRecordsService,
                _ownerFileStorageService,
                _readOnlyMode);
            var result = docsForm.ShowDialog();

            // Refresh summary after closing documents form
            LoadOwnerSummary();
            if (result == DialogResult.OK)
            {
                _hasChanges = true;
            }
        }

        private void frmLandOwnerDetails_Load(object sender, EventArgs e)
        {
            // Initialization done in constructor
        }

        private void btnViewParcels_Click(object sender, EventArgs e)
        {
            if (_owner == null) return;

            using var parcelsForm = new frmOwnerParcels(_owner, _landRecordsService);
            _ = parcelsForm.ShowDialog();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_hasChanges && DialogResult == DialogResult.None)
            {
                DialogResult = DialogResult.OK;
            }

            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            var image = picPhoto?.Image;
            if (picPhoto != null)
            {
                picPhoto.Image = null;
            }
            image?.Dispose();

            base.OnFormClosed(e);
        }

        private void btnUploadChangePhoto_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*";
            ofd.Title = "Select Owner Photo";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                if (_isAddMode)
                {
                    // In add mode, just store the photo temporarily and display it
                    _tempPhotoPath = ofd.FileName;
                    var previewImage = _ownerFileStorageService.LoadPhotoFromStoredPath(_tempPhotoPath);
                    ReplacePhotoPreview(previewImage);

                    _ = MessageBox.Show("Photo selected. It will be saved when you add the owner.", "Photo Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Edit mode - save photo immediately
                    if (_owner == null) return;

                    // Save and persist photo path
                    string relativePath = _ownerFileStorageService.SaveOwnerPhoto(_landOwnerId, ofd.FileName);
                    _owner.PhotoPath = relativePath;
                    _landRecordsService.UpdateOwnerPhotoPath(_landOwnerId, relativePath);

                    // Reload photo
                    LoadPhoto();
                    _hasChanges = true;

                    _ = MessageBox.Show("Photo uploaded successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show($"Failed to upload photo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkEdit_CheckedChanged(object sender, EventArgs e)
        {
            if (!_allowEditInReadOnly)
            {
                chkEdit.Checked = false;
                return;
            }

            if (chkEdit.Checked)
            {
                enableEditing();
                this.Text = "Land Owner Details (Edit)";
                chkEdit.Checked = false;

            }
        }
    }
}


