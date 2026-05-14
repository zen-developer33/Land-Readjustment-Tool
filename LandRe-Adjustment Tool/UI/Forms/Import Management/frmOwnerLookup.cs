using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Models;
using System.ComponentModel;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmOwnerLookup : Form
    {
        private readonly List<LandOwner>? _ownersFromDatabase;
        private readonly List<BaselineLandParcelRecord>? _importedRecords;
        private List<LandOwner> _allOwners = [];
        private List<LandOwner> _filteredOwners = [];
        private BindingList<OwnerLookupDisplayModel> _displayedOwners = [];
        private readonly HashSet<int> _selectedCoOwnerIds = new();
        private Image? _ownerPreviewImage;

        public LandOwner? SelectedOwner { get; private set; }
        public List<LandOwner> SelectedCoOwners { get; private set; } = [];

        // Constructor for database mode (LandParcelRecords form)
        public frmOwnerLookup(List<LandOwner> owners)
        {
            _ownersFromDatabase = owners;
            _importedRecords = null;
            InitializeComponent();
            LoadOwners();
        }

        // Constructor for imported records mode (Import Manager)
        public frmOwnerLookup(List<BaselineLandParcelRecord> importedRecords)
        {
            _ownersFromDatabase = null;
            _importedRecords = importedRecords;
            InitializeComponent();
            LoadOwnersFromImportedRecords();
        }

        private void LoadOwners()
        {
            try
            {
                _allOwners = _ownersFromDatabase?.ToList() ?? [];
                _filteredOwners = [.. _allOwners];
                BindOwnersToGrid(_filteredOwners);
                SelectInitialPrimaryOwner();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load owners: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOwnersFromImportedRecords()
        {
            try
            {
                if (_importedRecords == null || _importedRecords.Count == 0)
                {
                    _allOwners = [];
                    _filteredOwners = [];
                    BindOwnersToGrid(_filteredOwners);
                    return;
                }

                var uniqueOwners = new Dictionary<string, LandOwner>();
                int tempId = 1;

                foreach (var record in _importedRecords)
                {
                    AddOwnerIfUnique(uniqueOwners, ref tempId, new LandOwner
                    {
                        LandOwnersName = record.LandOwnersName ?? "",
                        FatherSpouse = record.FatherSpouse,
                        Gender = record.Gender,
                        CitizenshipNumber = record.CitizenshipNumber,
                        CitizenshipIssuedDistrict = record.CitizenshipIssuedDistrict,
                        CitizenshipIssuedDate = record.CitizenshipIssuedDate,
                        PermanentAddress = record.PermanentAddress,
                        TemporaryAddress = record.TemporaryAddress,
                        ContactNumber = record.ContactNumber,
                        EmailID = record.EmailID
                    });

                    foreach (var coOwner in record.JointCoOwners)
                    {
                        AddOwnerIfUnique(uniqueOwners, ref tempId, new LandOwner
                        {
                            LandOwnersName = coOwner.OwnerName ?? "",
                            FatherSpouse = coOwner.FatherSpouse,
                            Gender = coOwner.Gender,
                            CitizenshipNumber = coOwner.CitizenshipNumber,
                            CitizenshipIssuedDistrict = coOwner.CitizenshipIssuedDistrict,
                            CitizenshipIssuedDate = coOwner.CitizenshipIssuedDate,
                            PermanentAddress = coOwner.PermanentAddress,
                            TemporaryAddress = coOwner.TemporaryAddress,
                            ContactNumber = coOwner.ContactNumber,
                            EmailID = coOwner.EmailID
                        });
                    }
                }

                _allOwners = [.. uniqueOwners.Values];
                _filteredOwners = [.. _allOwners];
                BindOwnersToGrid(_filteredOwners);
                SelectInitialPrimaryOwner();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load owners from imported records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void PreselectOwners(LandOwner? primaryOwner, IEnumerable<CoOwnerRecord>? coOwners)
        {
            SelectedOwner = FindMatchingOwner(primaryOwner);
            _selectedCoOwnerIds.Clear();

            if (coOwners != null)
            {
                foreach (var coOwner in coOwners)
                {
                    var matchedOwner = FindMatchingOwner(new LandOwner
                    {
                        LandOwnersName = coOwner.OwnerName ?? string.Empty,
                        FatherSpouse = coOwner.FatherSpouse,
                        CitizenshipNumber = coOwner.CitizenshipNumber
                    });

                    if (matchedOwner != null && matchedOwner.LandOwnerId != SelectedOwner?.LandOwnerId)
                    {
                        _selectedCoOwnerIds.Add(matchedOwner.LandOwnerId);
                    }
                }
            }

            BindOwnersToGrid(_filteredOwners);
            SelectInitialPrimaryOwner();
        }

        private void BindOwnersToGrid(List<LandOwner> owners)
        {
            var displayModels = owners.Select(o => new OwnerLookupDisplayModel
            {
                IsCoOwner = _selectedCoOwnerIds.Contains(o.LandOwnerId),
                LandOwnerId = o.LandOwnerId,
                LandOwnersName = o.LandOwnersName ?? "",
                FatherSpouse = o.FatherSpouse ?? "",
                CitizenshipNumber = o.CitizenshipNumber ?? "",
                PermanentAddress = o.PermanentAddress ?? ""
            }).ToList();

            _displayedOwners = new BindingList<OwnerLookupDisplayModel>(displayModels);

            dgvOwners.DataSource = _displayedOwners;
            UpdateSelectionSummary();
            UpdateLoadButtonState();
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            var txtSearch = sender as TextBox;
            string searchText = txtSearch?.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredOwners = [.. _allOwners];
            }
            else
            {
                _filteredOwners = _allOwners.Where(o =>
                    (o.LandOwnersName?.ToLower().Contains(searchText) ?? false) ||
                    (o.FatherSpouse?.ToLower().Contains(searchText) ?? false) ||
                    (o.CitizenshipNumber?.ToLower().Contains(searchText) ?? false) ||
                    (o.PermanentAddress?.ToLower().Contains(searchText) ?? false)
                ).ToList();
            }

            BindOwnersToGrid(_filteredOwners);
        }

        private void DgvOwners_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateSelectedOwnerPreview();
            UpdateLoadButtonState();
        }

        private void DgvOwners_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            var coOwnerColumn = dgvOwners.Columns["IsCoOwner"];
            if (e.RowIndex >= 0 && coOwnerColumn != null && e.ColumnIndex != coOwnerColumn.Index)
            {
                LoadSelectedOwner();
            }
        }

        private void DgvOwners_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dgvOwners.IsCurrentCellDirty)
            {
                dgvOwners.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvOwners_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvOwners.Columns[e.ColumnIndex].Name != "IsCoOwner")
            {
                return;
            }

            if (dgvOwners.Rows[e.RowIndex].DataBoundItem is not OwnerLookupDisplayModel model)
            {
                return;
            }

            var isChecked = Convert.ToBoolean(dgvOwners.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            if (isChecked)
            {
                _selectedCoOwnerIds.Add(model.LandOwnerId);
            }
            else
            {
                _selectedCoOwnerIds.Remove(model.LandOwnerId);
            }

            UpdateSelectionSummary();
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            LoadSelectedOwner();
        }

        private void LoadSelectedOwner()
        {
            if (dgvOwners.SelectedRows.Count != 1) return;

            if (dgvOwners.SelectedRows[0].DataBoundItem is OwnerLookupDisplayModel model)
            {
                SelectedOwner = _allOwners.FirstOrDefault(o => o.LandOwnerId == model.LandOwnerId);
                if (SelectedOwner == null)
                {
                    return;
                }

                _selectedCoOwnerIds.Remove(SelectedOwner.LandOwnerId);
                SelectedCoOwners = _allOwners
                    .Where(o => _selectedCoOwnerIds.Contains(o.LandOwnerId))
                    .OrderBy(o => o.LandOwnersName)
                    .ToList();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            DisposeOwnerPreviewImage();
            base.OnFormClosed(e);
        }

        private static void AddOwnerIfUnique(Dictionary<string, LandOwner> owners, ref int nextId, LandOwner owner)
        {
            if (string.IsNullOrWhiteSpace(owner.LandOwnersName))
            {
                return;
            }

            var key = BuildOwnerKey(owner);
            if (owners.ContainsKey(key))
            {
                return;
            }

            owner.LandOwnerId = nextId++;
            owners[key] = owner;
        }

        private static string BuildOwnerKey(LandOwner owner)
        {
            return $"{owner.LandOwnersName.Trim().ToLowerInvariant()}|{owner.FatherSpouse?.Trim().ToLowerInvariant()}|{owner.CitizenshipNumber?.Trim().ToLowerInvariant()}";
        }

        private LandOwner? FindMatchingOwner(LandOwner? owner)
        {
            if (owner == null || string.IsNullOrWhiteSpace(owner.LandOwnersName))
            {
                return null;
            }

            if (owner.LandOwnerId > 0)
            {
                var byId = _allOwners.FirstOrDefault(o => o.LandOwnerId == owner.LandOwnerId);
                if (byId != null)
                {
                    return byId;
                }
            }

            var ownerKey = BuildOwnerKey(owner);
            return _allOwners.FirstOrDefault(o => BuildOwnerKey(o) == ownerKey);
        }

        private void SelectInitialPrimaryOwner()
        {
            if (SelectedOwner == null || dgvOwners.Rows.Count == 0)
            {
                UpdateSelectedOwnerPreview();
                return;
            }

            foreach (DataGridViewRow row in dgvOwners.Rows)
            {
                if (row.DataBoundItem is OwnerLookupDisplayModel model &&
                    model.LandOwnerId == SelectedOwner.LandOwnerId)
                {
                    row.Selected = true;
                    dgvOwners.CurrentCell = row.Cells["LandOwnersName"];
                    return;
                }
            }

            UpdateSelectedOwnerPreview();
        }

        private void UpdateSelectedOwnerPreview()
        {
            var owner = GetCurrentOwner();
            lblPrimaryValue.Text = owner?.LandOwnersName ?? "-";
            lblFatherValue.Text = owner?.FatherSpouse ?? "-";
            lblCitizenshipValue.Text = owner?.CitizenshipNumber ?? "-";
            lblAddressValue.Text = owner?.PermanentAddress ?? "-";
            LoadOwnerPreviewImage(owner);
        }

        private LandOwner? GetCurrentOwner()
        {
            if (dgvOwners.SelectedRows.Count != 1 ||
                dgvOwners.SelectedRows[0].DataBoundItem is not OwnerLookupDisplayModel model)
            {
                return null;
            }

            return _allOwners.FirstOrDefault(o => o.LandOwnerId == model.LandOwnerId);
        }

        private void UpdateSelectionSummary()
        {
            lblCoOwnerCount.Text = $"{_selectedCoOwnerIds.Count} co-owner(s) checked";
        }

        private void UpdateLoadButtonState()
        {
            btnLoad.Enabled = dgvOwners.SelectedRows.Count == 1;
        }

        private void LoadOwnerPreviewImage(LandOwner? owner)
        {
            DisposeOwnerPreviewImage();
            picOwner.Image = null;

            var resolvedPath = ResolveOwnerPhotoPath(owner?.PhotoPath);
            if (resolvedPath == null)
            {
                return;
            }

            try
            {
                using var stream = File.OpenRead(resolvedPath);
                _ownerPreviewImage = Image.FromStream(stream);
                picOwner.Image = _ownerPreviewImage;
            }
            catch
            {
                picOwner.Image = null;
            }
        }

        private static string? ResolveOwnerPhotoPath(string? photoPath)
        {
            if (string.IsNullOrWhiteSpace(photoPath))
            {
                return null;
            }

            var candidate = Path.IsPathRooted(photoPath)
                ? photoPath
                : AppServices.HasContext
                    ? Path.Combine(Path.GetDirectoryName(AppServices.Context.ProjectFilePath) ?? string.Empty, photoPath)
                    : photoPath;

            return File.Exists(candidate) ? candidate : null;
        }

        private void DisposeOwnerPreviewImage()
        {
            var oldImage = _ownerPreviewImage;
            _ownerPreviewImage = null;
            oldImage?.Dispose();
        }
    }

    public class OwnerLookupDisplayModel
    {
        public bool IsCoOwner { get; set; }
        public int LandOwnerId { get; set; }
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
    }
}
