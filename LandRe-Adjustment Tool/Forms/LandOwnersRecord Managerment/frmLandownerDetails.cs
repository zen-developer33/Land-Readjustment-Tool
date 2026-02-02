using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.Diagnostics;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Detail form for viewing/editing landowner records with photo and document management
    /// Matches reference Image 1
    /// </summary>
    public partial class frmLandownerDetails : Form
    {
        private readonly string _projectPath;
        private readonly OriginalLandParcel _parcel;
        private LandOwnerRepository _repository;
        private bool _hasChanges = false;

        public frmLandownerDetails(string projectPath, OriginalLandParcel parcel)
        {
            InitializeComponent();
            _projectPath = projectPath;
            _parcel = parcel;
            
            InitializeRepository();
            LoadParcelData();
            LoadPhoto();
            LoadDocuments();
            UpdateTotalRecordsLabel();
        }

        private void InitializeRepository()
        {
            var dbHelper = new DatabaseHelper(_projectPath);
            dbHelper.InitializeDatabase();
            var connection = dbHelper.GetConnection();
            _repository = new LandOwnerRepository(connection);
        }

        private void LoadParcelData()
        {
            // Owner information
            txtName.Text = _parcel.Owner?.LandOwnersName ?? "";
            txtFatherSpouse.Text = _parcel.Owner?.FatherSpouse ?? "";
            txtCitizenshipNo.Text = _parcel.Owner?.CitizenshipNumber ?? "";
            
            // Parcel information
            txtParcelNo.Text = _parcel.ParcelNo;
            txtAreaSqm.Text = _parcel.AreaInSqm?.ToString("F2") ?? "";
            
            // Land use dropdown
            cmbLandUse.Items.Clear();
            cmbLandUse.Items.AddRange(new object[] 
            { 
                "Residential", "Agricultural", "Commercial", 
                "Industrial", "Forest", "Other" 
            });
            
            if (!string.IsNullOrWhiteSpace(_parcel.LandUse))
            {
                int index = cmbLandUse.FindStringExact(_parcel.LandUse);
                cmbLandUse.SelectedIndex = index >= 0 ? index : -1;
            }
            
            // Parcel Location (multiline) - now from parcel, not owner
            txtAddress.Text = _parcel.ParcelLocation ?? "";
            
            // Mark form as unchanged initially
            _hasChanges = false;
        }

        private void LoadPhoto()
        {
            if (_parcel.Owner == null || string.IsNullOrWhiteSpace(_parcel.Owner.PhotoPath))
            {
                picPhoto.Image = null;
                return;
            }

            string photoPath = Path.Combine(
                Path.GetDirectoryName(_projectPath)!,
                _parcel.Owner.PhotoPath
            );

            if (File.Exists(photoPath))
            {
                try
                {
                    // Load image without locking file
                    using (var stream = new FileStream(photoPath, FileMode.Open, FileAccess.Read))
                    {
                        picPhoto.Image = Image.FromStream(stream);
                    }
                    picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load photo: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadDocuments()
        {
            lstDocuments.Items.Clear();

            if (_parcel.Owner == null || string.IsNullOrWhiteSpace(_parcel.Owner.DocumentsFolderPath))
                return;

            string docsFolder = Path.Combine(
                Path.GetDirectoryName(_projectPath)!,
                _parcel.Owner.DocumentsFolderPath
            );

            if (!Directory.Exists(docsFolder))
                return;

            try
            {
                var files = Directory.GetFiles(docsFolder);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    lstDocuments.Items.Add(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load documents: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            if (_parcel.Owner == null)
            {
                MessageBox.Show("Cannot upload photo: Owner information is missing.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                ofd.Title = "Select Owner Photo";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // Create photo folder
                    string ownerFolder = $"{_parcel.Owner.LandOwnersName}_{_parcel.Owner.CitizenshipNumber}";
                    ownerFolder = string.Join("_", ownerFolder.Split(Path.GetInvalidFileNameChars()));
                    
                    string photoFolder = Path.Combine(
                        Path.GetDirectoryName(_projectPath)!,
                        "Images",
                        "LandOwners Photos",
                        ownerFolder
                    );

                    Directory.CreateDirectory(photoFolder);

                    // Copy photo
                    string photoPath = Path.Combine(photoFolder, "photo.jpg");
                    File.Copy(ofd.FileName, photoPath, overwrite: true);

                    // Update database
                    string relativePath = Path.Combine("Images", "LandOwners Photos", ownerFolder, "photo.jpg");
                    _repository.UpdateOwnerPhoto(_parcel.Owner.LandOwnerId, relativePath);
                    _parcel.Owner.PhotoPath = relativePath;

                    // Reload photo
                    LoadPhoto();
                    _hasChanges = true;

                    MessageBox.Show("Photo uploaded successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to upload photo: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAttachDocument_Click(object sender, EventArgs e)
        {
            if (_parcel.Owner == null)
            {
                MessageBox.Show("Cannot attach document: Owner information is missing.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "All Files|*.*|PDF Files|*.pdf|Image Files|*.jpg;*.jpeg;*.png";
                ofd.Title = "Select Document to Attach";
                ofd.Multiselect = true;

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    // Create documents folder
                    string docsFolder = Path.Combine(
                        Path.GetDirectoryName(_projectPath)!,
                        "Documents",
                        $"LandOwner_{_parcel.Owner.LandOwnerId}"
                    );

                    Directory.CreateDirectory(docsFolder);

                    // Copy each selected file
                    foreach (string filePath in ofd.FileNames)
                    {
                        string fileName = Path.GetFileName(filePath);
                        string destPath = Path.Combine(docsFolder, fileName);
                        
                        // Handle duplicate filenames
                        int counter = 1;
                        while (File.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string extension = Path.GetExtension(fileName);
                            destPath = Path.Combine(docsFolder, $"{nameWithoutExt}_{counter}{extension}");
                            counter++;
                        }

                        File.Copy(filePath, destPath);
                    }

                    // Update database with folder path
                    string relativePath = Path.Combine("Documents", $"LandOwner_{_parcel.Owner.LandOwnerId}");
                    _repository.UpdateOwnerDocumentsFolder(_parcel.Owner.LandOwnerId, relativePath);
                    _parcel.Owner.DocumentsFolderPath = relativePath;

                    // Reload documents list
                    LoadDocuments();
                    _hasChanges = true;

                    MessageBox.Show($"{ofd.FileNames.Length} document(s) attached successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to attach document: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDeleteDocument_Click(object sender, EventArgs e)
        {
            if (lstDocuments.SelectedItem == null)
            {
                MessageBox.Show("Please select a document to delete.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{lstDocuments.SelectedItem}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            try
            {
                string fileName = lstDocuments.SelectedItem.ToString()!;
                string docsFolder = Path.Combine(
                    Path.GetDirectoryName(_projectPath)!,
                    _parcel.Owner!.DocumentsFolderPath!
                );

                string filePath = Path.Combine(docsFolder, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LoadDocuments();
                    _hasChanges = true;
                    
                    MessageBox.Show("Document deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete document: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstDocuments_DoubleClick(object sender, EventArgs e)
        {
            if (lstDocuments.SelectedItem == null)
                return;

            try
            {
                string fileName = lstDocuments.SelectedItem.ToString()!;
                string docsFolder = Path.Combine(
                    Path.GetDirectoryName(_projectPath)!,
                    _parcel.Owner!.DocumentsFolderPath!
                );

                string filePath = Path.Combine(docsFolder, fileName);

                if (File.Exists(filePath))
                {
                    // Open document with default application
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open document: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // Update owner information
                if (_parcel.Owner != null)
                {
                    _parcel.Owner.LandOwnersName = txtName.Text.Trim();
                    _parcel.Owner.FatherSpouse = txtFatherSpouse.Text.Trim();
                    _parcel.Owner.CitizenshipNumber = txtCitizenshipNo.Text.Trim();
                    _parcel.Owner.ModifiedDate = DateTime.Now;
                }

                // Update parcel information
                _parcel.ParcelNo = txtParcelNo.Text.Trim();
                _parcel.ParcelLocation = txtAddress.Text.Trim();
                
                if (double.TryParse(txtAreaSqm.Text, out double area))
                    _parcel.AreaInSqm = area;

                _parcel.LandUse = cmbLandUse.SelectedItem?.ToString();
                _parcel.ModifiedDate = DateTime.Now;

                // TODO: Save to database
                // _repository.UpdateParcel(_parcel);

                _hasChanges = false;
                this.DialogResult = DialogResult.OK;

                MessageBox.Show("Changes saved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save changes: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Owner name is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtParcelNo.Text))
            {
                MessageBox.Show("Parcel number is required.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtParcelNo.Focus();
                return false;
            }

            if (!double.TryParse(txtAreaSqm.Text, out double area) || area <= 0)
            {
                MessageBox.Show("Please enter a valid area (sq.m) greater than 0.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAreaSqm.Focus();
                return false;
            }

            return true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_hasChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save before closing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    btnSaveChanges_Click(sender, e);
                    return;
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void UpdateTotalRecordsLabel()
        {
            int totalRecords = _repository.GetTotalRecordCount();
            lblTotalRecords.Text = $"Total Records: {totalRecords}";
        }

        // Track changes
        private void MarkAsChanged(object sender, EventArgs e)
        {
            _hasChanges = true;
        }

        private void frmLandownerDetails_Load(object sender, EventArgs e)
        {
            // Attach change tracking to controls
            txtName.TextChanged += MarkAsChanged;
            txtFatherSpouse.TextChanged += MarkAsChanged;
            txtCitizenshipNo.TextChanged += MarkAsChanged;
            txtParcelNo.TextChanged += MarkAsChanged;
            txtAreaSqm.TextChanged += MarkAsChanged;
            txtAddress.TextChanged += MarkAsChanged;
            cmbLandUse.SelectedIndexChanged += MarkAsChanged;
        }
    }
}
