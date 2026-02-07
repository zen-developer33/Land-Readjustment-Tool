using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Detail form for viewing landowner information with photo, documents, and parcels access
    /// </summary>
    public partial class frmLandownerDetails_2 : Form
    {
        private readonly string _projectPath;
        private readonly OriginalLandParcel _parcel;
        private LandOwnerRepository _repository;

        public frmLandownerDetails_2(string projectPath, OriginalLandParcel parcel)
        {
            InitializeComponent();
            _projectPath = projectPath;
            _parcel = parcel;

            InitializeRepository();
            LoadOwnerData();
            LoadPhoto();
            LoadParcelCount();
        }

        private void InitializeRepository()
        {
            var dbHelper = new DatabaseHelper(_projectPath);
            dbHelper.InitializeDatabase();
            var connection = dbHelper.GetConnection();
            _repository = new LandOwnerRepository(connection);
        }

        private void LoadOwnerData()
        {
            if (_parcel.Owner == null) return;

            lblNameValue.Text = _parcel.Owner.LandOwnersName;
            lblFatherSpouseValue.Text = string.IsNullOrWhiteSpace(_parcel.Owner.FatherSpouse) ? "-" : _parcel.Owner.FatherSpouse;
            lblCitizenshipNoValue.Text = string.IsNullOrWhiteSpace(_parcel.Owner.CitizenshipNumber) ? "-" : _parcel.Owner.CitizenshipNumber;
            lblGenderValue.Text = string.IsNullOrWhiteSpace(_parcel.Owner.Gender) ? "-" : _parcel.Owner.Gender;
            lblPermanentAddressValue.Text = string.IsNullOrWhiteSpace(_parcel.Owner.PermanentAddress) ? "-" : _parcel.Owner.PermanentAddress;

            Text = $"Landowner Details - {_parcel.Owner.LandOwnersName}";
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
                    using var stream = new FileStream(photoPath, FileMode.Open, FileAccess.Read);
                    picPhoto.Image = Image.FromStream(stream);
                    picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load photo: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadParcelCount()
        {
            if (_parcel.Owner == null) return;

            int count = _repository.GetParcelCountByOwnerId(_parcel.Owner.LandOwnerId);

        }

        private void btnUploadPhoto_Click(object sender, EventArgs e)
        {
            if (_parcel.Owner == null)
            {
                MessageBox.Show("Cannot upload photo: Owner information is missing.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            ofd.Title = "Select Owner Photo";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string ownerFolder = $"{_parcel.Owner.LandOwnersName}_{_parcel.Owner.CitizenshipNumber}";
                ownerFolder = string.Join("_", ownerFolder.Split(Path.GetInvalidFileNameChars()));

                string photoFolder = Path.Combine(
                    Path.GetDirectoryName(_projectPath)!,
                    "Images",
                    "LandOwners Photos",
                    ownerFolder
                );

                Directory.CreateDirectory(photoFolder);

                string photoPath = Path.Combine(photoFolder, "photo.jpg");
                File.Copy(ofd.FileName, photoPath, overwrite: true);

                string relativePath = Path.Combine("Images", "LandOwners Photos", ownerFolder, "photo.jpg");
                _repository.UpdateOwnerPhoto(_parcel.Owner.LandOwnerId, relativePath);
                _parcel.Owner.PhotoPath = relativePath;

                LoadPhoto();

                MessageBox.Show("Photo uploaded successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to upload photo: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewDocuments_Click(object sender, EventArgs e)
        {
            if (_parcel.Owner == null) return;

            using var docsForm = new frmOwnerDocuments(_projectPath, _parcel.Owner, _repository);
            docsForm.ShowDialog(this);
        }

        private void btnViewParcels_Click(object sender, EventArgs e)
        {
            if (_parcel.Owner == null) return;

            using var parcelsForm = new frmOwnerParcels(_parcel.Owner, _repository);
            parcelsForm.ShowDialog(this);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void lblParcelCount_Click(object sender, EventArgs e)
        {

        }

        private void frmLandownerDetails_Load(object sender, EventArgs e)
        {

        }
    }
}
