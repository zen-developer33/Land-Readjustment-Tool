using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;
using System.Data.SQLite;

namespace Land_Readjustment_Tool.Forms
{
    public partial class frmOwnerLookup : Form
    {
        private readonly LandOwnerRepository? _repository;
        private readonly List<BaselineLandParceRecord>? _importedRecords;
        private List<LandOwner> _allOwners = [];
        private List<LandOwner> _filteredOwners = [];
        private BindingList<OwnerLookupDisplayModel> _displayedOwners = [];

        public LandOwner? SelectedOwner { get; private set; }

        // Constructor for database mode (LandParcelRecords form)
        public frmOwnerLookup(LandOwnerRepository repository)
        {
            _repository = repository;
            _importedRecords = null;
            InitializeComponents();
            LoadOwners();
        }

        // Constructor for imported records mode (Import Manager)
        public frmOwnerLookup(List<BaselineLandParceRecord> importedRecords)
        {
            _repository = null;
            _importedRecords = importedRecords;
            InitializeComponents();
            LoadOwnersFromImportedRecords();
        }

        private void InitializeComponents()
        {
            Text = "Select Land Owner";
            Size = new Size(600, 450);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(12, 15),
                AutoSize = true
            };

            var txtSearch = new TextBox
            {
                Name = "txtSearch",
                Location = new Point(70, 12),
                Size = new Size(300, 23)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            var dgvOwners = new DataGridView
            {
                Name = "dgvOwners",
                Location = new Point(12, 45),
                Size = new Size(560, 320),
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvOwners.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LandOwnerId",
                HeaderText = "ID",
                DataPropertyName = "LandOwnerId",
                Width = 50
            });
            dgvOwners.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LandOwnersName",
                HeaderText = "Owner Name",
                DataPropertyName = "LandOwnersName",
                Width = 180
            });
            dgvOwners.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "FatherSpouse",
                HeaderText = "Father/Spouse",
                DataPropertyName = "FatherSpouse",
                Width = 150
            });
            dgvOwners.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CitizenshipNumber",
                HeaderText = "Citizenship No",
                DataPropertyName = "CitizenshipNumber",
                Width = 120
            });
            dgvOwners.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PermanentAddress",
                HeaderText = "Address",
                DataPropertyName = "PermanentAddress",
                Width = 150
            });

            dgvOwners.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvOwners.ColumnHeadersHeight = 30;
            dgvOwners.RowTemplate.Height = 25;
            dgvOwners.SelectionChanged += DgvOwners_SelectionChanged;
            dgvOwners.CellDoubleClick += DgvOwners_CellDoubleClick;

            var btnLoad = new Button
            {
                Name = "btnLoad",
                Text = "Load",
                Location = new Point(400, 375),
                Size = new Size(80, 28),
                Enabled = false
            };
            btnLoad.Click += BtnLoad_Click;

            var btnCancel = new Button
            {
                Name = "btnCancel",
                Text = "Cancel",
                Location = new Point(490, 375),
                Size = new Size(80, 28)
            };
            btnCancel.Click += BtnCancel_Click;

            Controls.Add(lblSearch);
            Controls.Add(txtSearch);
            Controls.Add(dgvOwners);
            Controls.Add(btnLoad);
            Controls.Add(btnCancel);
        }

        private void LoadOwners()
        {
            try
            {
                _allOwners = _repository?.GetAllOwners() ?? [];
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
                            LandOwnersName = record.LandOwnersName,
                            FatherSpouse = record.FatherSpouse,
                            Gender = record.Gender,
                            CitizenshipNumber = record.CitizenshipNumber,
                            CitizenshipIssuedDistrict = record.CitizenshipIssuedDistrict,
                            CitizenshipIssuedDate = record.citizenshipIssuedDate,
                            PermanentAddress = record.PermanentAddress,
                            TemporaryAddress = record.TempoaryAddress,
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

            var dgv = Controls.Find("dgvOwners", false).FirstOrDefault() as DataGridView;
            if (dgv != null)
            {
                dgv.DataSource = _displayedOwners;
            }
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
            var dgv = sender as DataGridView;
            var btnLoad = Controls.Find("btnLoad", false).FirstOrDefault() as Button;
            if (btnLoad != null)
            {
                btnLoad.Enabled = dgv?.SelectedRows.Count == 1;
            }
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
            var dgv = Controls.Find("dgvOwners", false).FirstOrDefault() as DataGridView;
            if (dgv?.SelectedRows.Count != 1) return;

            if (dgv.SelectedRows[0].DataBoundItem is OwnerLookupDisplayModel model)
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
