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

        public LandOwner? SelectedOwner { get; private set; }

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

                // Extract unique owners from imported records
                // Use a dictionary to deduplicate by owner name + father/spouse + citizenship number
                var uniqueOwners = new Dictionary<string, LandOwner>();
                int tempId = 1;

                foreach (var record in _importedRecords)
                {
                    // Skip records with no owner name
                    if (string.IsNullOrWhiteSpace(record.LandOwnersName))
                        continue;

                    // Create a key for deduplication
                    string key = $"{record.LandOwnersName?.Trim().ToLower()}|{record.FatherSpouse?.Trim().ToLower()}|{record.CitizenshipNumber?.Trim().ToLower()}";

                    if (!uniqueOwners.ContainsKey(key))
                    {
                        uniqueOwners[key] = new LandOwner
                        {
                            LandOwnerId = tempId++, // Temporary ID for display purposes
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
                        };
                    }
                }

                _allOwners = [.. uniqueOwners.Values];
                _filteredOwners = [.. _allOwners];
                BindOwnersToGrid(_filteredOwners);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load owners from imported records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindOwnersToGrid(List<LandOwner> owners)
        {
            var displayModels = owners.Select(o => new OwnerLookupDisplayModel
            {
                LandOwnerId = o.LandOwnerId,
                LandOwnersName = o.LandOwnersName ?? "",
                FatherSpouse = o.FatherSpouse ?? "",
                CitizenshipNumber = o.CitizenshipNumber ?? "",
                PermanentAddress = o.PermanentAddress ?? ""
            }).ToList();

            _displayedOwners = new BindingList<OwnerLookupDisplayModel>(displayModels);

            dgvOwners.DataSource = _displayedOwners;
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
            var grid = sender as DataGridView;
            btnLoad.Enabled = grid?.SelectedRows.Count == 1;
        }

        private void DgvOwners_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            LoadSelectedOwner();
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
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    public class OwnerLookupDisplayModel
    {
        public int LandOwnerId { get; set; }
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
    }
}
