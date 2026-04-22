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
        private readonly string _projectPath;
        private LandOwner? _owner;
        private string? _tempPhotoPath; // Temporary photo path for add mode

        /// <summary>
        /// Constructor for viewing/editing existing owner
        /// </summary>
        public frmLandOwnerDetails(int landOwnerId, bool readOnlyMode)
        {
            InitializeComponent();
            _landOwnerId = landOwnerId;
            _readOnlyMode = readOnlyMode;
            _isAddMode = false;
            if (!AppServices.HasContext)
                throw new InvalidOperationException("No open project context found.");

            _projectPath = AppServices.Context.ProjectFilePath;
            _landRecordsService = new LandRecordsService(AppServices.Context.Session, _projectPath);

            LoadOwnerDetails();
            LoadOwnerSummary();
            SetReadOnlyMode(_readOnlyMode);
        }

        /// <summary>
        /// Constructor for adding new owner
        /// </summary>
        public frmLandOwnerDetails()
        {
            InitializeComponent();
            _landOwnerId = 0;
            _readOnlyMode = false;
            _isAddMode = true;
            if (!AppServices.HasContext)
                throw new InvalidOperationException("No open project context found.");

            _projectPath = AppServices.Context.ProjectFilePath;
            _landRecordsService = new LandRecordsService(AppServices.Context.Session, _projectPath);

            SetAddMode();
        }

        private void SetAddMode()
        {
            Text = "Add New Land Owner";

            // Clear all fields
            txtFullName.Text = "";
            txtFatherSpouse.Text = "";
            cbGender.SelectedIndex = -1;
            txtCitizenshipNo.Text = "";
            txtIssueDistrict.Text = "";
            txtIssueDate.Text = "";
            txtPermanentAddress.Text = "";
            txtTemporaryAddress.Text = "";
            txtContactNumber.Text = "";
            txtEmailID.Text = "";
            picPhoto.Image = null;

            // Update summary labels to show no parcels
            lblParcelCount.Text = "-";
            lblAreasqm.Text = "-";
            lblAreaLocal.Text = "-";

            // Disable parcel and document buttons (can't view for new owner)
            btnViewParcels.Text = "View Parcels Owned (0)";

            btnAttachViewDocuments.Text = "Attach/View Documents (0)";


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
            cbGender.SelectedItem = _owner.Gender ?? "";
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
            if (_owner == null || string.IsNullOrWhiteSpace(_owner.PhotoPath))
            {
                picPhoto.Image = null; // No default image
                return;
            }

            string projectDir = Path.GetDirectoryName(_projectPath) ?? "";
            string photoPath = Path.Combine(projectDir, _owner.PhotoPath);

            if (File.Exists(photoPath))
            {
                try
                {
                    picPhoto.Image = Image.FromFile(photoPath);
                    picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
                }
                catch
                {
                    picPhoto.Image = null;
                }
            }
            else
            {
                picPhoto.Image = null;
            }
        }

        private void LoadOwnerSummary()
        {
            int parcelCount = _landRecordsService.GetParcelsByOwnerId(_landOwnerId).Count;
            double totalAreaSqm = _landRecordsService.GetTotalAreaByOwnerId(_landOwnerId);
            int documentCount = _landRecordsService.GetDocumentCountByOwnerId(_landOwnerId);

            // Update summary labels
            if (parcelCount == 0)
            {
                lblParcelCount.Text = "-";
                lblAreasqm.Text = "-";
                lblAreaLocal.Text = "-";
            }
            else
            {
                string areaRAPD = AreaConverterService.SqmToRAPDString(totalAreaSqm);
                lblParcelCount.Text = parcelCount.ToString();
                lblAreasqm.Text = $"{totalAreaSqm:F2} sq.m";
                lblAreaLocal.Text = areaRAPD;
            }

            // Update button texts with counts
            btnViewParcels.Text = $"View Parcels Owned ({parcelCount})";
            btnAttachViewDocuments.Text = $"Attach/View Documents ({documentCount})";
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

                Text = "Land Owner Details (Read-Only)";
                _readOnlyMode = true;
            }
            else
            {
                _readOnlyMode = false;
                Text = "Land Owner Details";
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
                        Gender = cbGender.SelectedItem?.ToString() ?? "",
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
                            string projectDir = Path.GetDirectoryName(_projectPath) ?? "";
                            string photosFolder = Path.Combine(projectDir, "OwnerPhotos");
                            _ = Directory.CreateDirectory(photosFolder);

                            string extension = Path.GetExtension(_tempPhotoPath);
                            string fileName = $"Owner_{newOwnerId}{extension}";
                            string destPath = Path.Combine(photosFolder, fileName);

                            File.Copy(_tempPhotoPath, destPath, overwrite: true);

                            string relativePath = Path.Combine("OwnerPhotos", fileName);
                            _landRecordsService.UpdateOwnerPhotoPath(newOwnerId, relativePath);
                        }
                        catch
                        {
                            // Photo save failed, but owner was created successfully - just continue
                        }
                    }

                    _ = MessageBox.Show($"Land owner added successfully!\n\nOwner ID: {newOwnerId}\n\nYou can assign parcels to this owner from the Land Parcel Records form.",
                        "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

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
                    _owner.Gender = cbGender.SelectedItem?.ToString() ?? "";
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
            //chkEdit.Text = "Stop Editing";
            this.Text = "Land Owner Details";
        }


        private void btnAttachViewDocuments_Click(object sender, EventArgs e)
        {
            if (_owner == null) return;

            using var docsForm = new frmOwnerDocuments(_projectPath, _owner, _landRecordsService, _readOnlyMode);
            _ = docsForm.ShowDialog();

            // Refresh summary after closing documents form
            LoadOwnerSummary();
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
                    picPhoto.Image = Image.FromFile(_tempPhotoPath);
                    picPhoto.SizeMode = PictureBoxSizeMode.Zoom;

                    _ = MessageBox.Show("Photo selected. It will be saved when you add the owner.", "Photo Selected",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Edit mode - save photo immediately
                    if (_owner == null) return;

                    // Create OwnerPhotos folder if it doesn't exist
                    string projectDir = Path.GetDirectoryName(_projectPath) ?? "";
                    string photosFolder = Path.Combine(projectDir, "OwnerPhotos");
                    _ = Directory.CreateDirectory(photosFolder);

                    // Generate unique filename
                    string extension = Path.GetExtension(ofd.FileName);
                    string fileName = $"Owner_{_landOwnerId}{extension}";
                    string destPath = Path.Combine(photosFolder, fileName);

                    // Copy file
                    File.Copy(ofd.FileName, destPath, overwrite: true);

                    // Update database
                    string relativePath = Path.Combine("OwnerPhotos", fileName);
                    _owner.PhotoPath = relativePath;
                    _landRecordsService.UpdateOwnerPhotoPath(_landOwnerId, relativePath);

                    // Reload photo
                    LoadPhoto();

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
            if (chkEdit.Checked)
            {
                enableEditing();
                this.Text = "Land Owner Details (Edit)";
                chkEdit.Checked = false;

            }
        }
    }
}


