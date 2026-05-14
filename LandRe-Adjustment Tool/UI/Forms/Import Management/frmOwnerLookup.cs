using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Models;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;

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
        private int? _primaryOwnerId;
        private Image? _ownerPreviewImage;

        public LandOwner? SelectedOwner { get; private set; }
        public List<LandOwner> SelectedCoOwners { get; private set; } = [];

        // Constructor for database mode (LandParcelRecords form)
        public frmOwnerLookup(List<LandOwner> owners)
        {
            _ownersFromDatabase = owners;
            _importedRecords = null;
            InitializeComponent();
            ConfigureOwnerGridColumns();
            LoadOwners();
        }

        // Constructor for imported records mode (Import Manager)
        public frmOwnerLookup(List<BaselineLandParcelRecord> importedRecords)
        {
            _ownersFromDatabase = null;
            _importedRecords = importedRecords;
            InitializeComponent();
            ConfigureOwnerGridColumns();
            LoadOwnersFromImportedRecords();
        }

        private void ConfigureOwnerGridColumns()
        {
            dgvOwners.AutoGenerateColumns = false;
            dgvOwners.Columns.Clear();

            colIsPrimaryOwner = new DataGridViewRadioButtonColumn
            {
                Name = "colIsPrimaryOwner",
                HeaderText = "Primary",
                DataPropertyName = nameof(OwnerLookupDisplayModel.IsPrimaryOwner),
                Width = 72,
                TrueValue = true,
                FalseValue = false,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            colIsCoOwner = new DataGridViewCheckBoxColumn
            {
                Name = "colIsCoOwner",
                HeaderText = "Co-owner",
                DataPropertyName = nameof(OwnerLookupDisplayModel.IsCoOwner),
                Width = 82,
                TrueValue = true,
                FalseValue = false,
                ThreeState = false,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };

            colLandOwnerId = new DataGridViewTextBoxColumn
            {
                Name = "colLandOwnerId",
                HeaderText = "ID",
                DataPropertyName = nameof(OwnerLookupDisplayModel.LandOwnerId),
                Visible = false
            };

            colLandOwnersName = new DataGridViewTextBoxColumn
            {
                Name = "colLandOwnersName",
                HeaderText = "Owner Name",
                DataPropertyName = nameof(OwnerLookupDisplayModel.LandOwnersName),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 140
            };

            colFatherSpouse = new DataGridViewTextBoxColumn
            {
                Name = "colFatherSpouse",
                HeaderText = "Father/Spouse",
                DataPropertyName = nameof(OwnerLookupDisplayModel.FatherSpouse),
                Width = 150
            };

            colCitizenshipNumber = new DataGridViewTextBoxColumn
            {
                Name = "colCitizenshipNumber",
                HeaderText = "Citizenship",
                DataPropertyName = nameof(OwnerLookupDisplayModel.CitizenshipNumber),
                Width = 120
            };

            colPermanentAddress = new DataGridViewTextBoxColumn
            {
                Name = "colPermanentAddress",
                HeaderText = "Address",
                DataPropertyName = nameof(OwnerLookupDisplayModel.PermanentAddress),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 120
            };

            dgvOwners.Columns.AddRange(
                colIsPrimaryOwner,
                colIsCoOwner,
                colLandOwnerId,
                colLandOwnersName,
                colFatherSpouse,
                colCitizenshipNumber,
                colPermanentAddress);
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
            _primaryOwnerId = SelectedOwner?.LandOwnerId;
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
                IsPrimaryOwner = _primaryOwnerId == o.LandOwnerId,
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

        private void DgvOwners_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dgvOwners.Columns[e.ColumnIndex].Name;
            if (columnName == "colIsPrimaryOwner" || columnName == "IsPrimaryOwner")
            {
                dgvOwners.CommitEdit(DataGridViewDataErrorContexts.Commit);
                if (dgvOwners.Rows[e.RowIndex].DataBoundItem is OwnerLookupDisplayModel model)
                {
                    SetPrimaryOwner(model.LandOwnerId);
                }
            }
            else if (columnName == "colIsCoOwner" || columnName == "IsCoOwner")
            {
                dgvOwners.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvOwners_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                dgvOwners.Rows[e.RowIndex].DataBoundItem is not OwnerLookupDisplayModel model)
            {
                return;
            }

            string columnName = dgvOwners.Columns[e.ColumnIndex].Name;
            if (columnName == "colIsCoOwner" || columnName == "IsCoOwner")
                return;

            SetPrimaryOwner(model.LandOwnerId);
            LoadSelectedOwner();
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
            if (e.RowIndex < 0)
            {
                return;
            }

            if (dgvOwners.Rows[e.RowIndex].DataBoundItem is not OwnerLookupDisplayModel model)
            {
                return;
            }

            string columnName = dgvOwners.Columns[e.ColumnIndex].Name;
            if (columnName == "colIsPrimaryOwner" || columnName == "IsPrimaryOwner")
            {
                if (Convert.ToBoolean(dgvOwners.Rows[e.RowIndex].Cells[e.ColumnIndex].Value))
                    SetPrimaryOwner(model.LandOwnerId);

                return;
            }

            if (columnName != "colIsCoOwner" && columnName != "IsCoOwner")
            {
                return;
            }

            bool isChecked = Convert.ToBoolean(dgvOwners.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            if (model.LandOwnerId == _primaryOwnerId)
            {
                model.IsCoOwner = false;
                dgvOwners.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = false;
                _selectedCoOwnerIds.Remove(model.LandOwnerId);
            }
            else if (isChecked)
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
            if (!_primaryOwnerId.HasValue)
                return;

            SelectedOwner = _allOwners.FirstOrDefault(o => o.LandOwnerId == _primaryOwnerId.Value);
            if (SelectedOwner == null)
                return;

            _selectedCoOwnerIds.Remove(SelectedOwner.LandOwnerId);
            SelectedCoOwners = _allOwners
                .Where(o => _selectedCoOwnerIds.Contains(o.LandOwnerId))
                .OrderBy(o => o.LandOwnersName)
                .ToList();

            DialogResult = DialogResult.OK;
            Close();
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
                if (!_primaryOwnerId.HasValue && dgvOwners.Rows.Count > 0 &&
                    dgvOwners.Rows[0].DataBoundItem is OwnerLookupDisplayModel firstModel)
                {
                    SetPrimaryOwner(firstModel.LandOwnerId);
                    return;
                }

                UpdateSelectedOwnerPreview();
                return;
            }

            foreach (DataGridViewRow row in dgvOwners.Rows)
            {
                if (row.DataBoundItem is OwnerLookupDisplayModel model &&
                    model.LandOwnerId == SelectedOwner.LandOwnerId)
                {
                    _primaryOwnerId = model.LandOwnerId;
                    model.IsPrimaryOwner = true;
                    row.Selected = true;
                    dgvOwners.CurrentCell = row.Cells[ResolveOwnerNameColumnName()];
                    RefreshPrimaryColumn();
                    return;
                }
            }

            UpdateSelectedOwnerPreview();
        }

        private string ResolveOwnerNameColumnName()
        {
            if (dgvOwners.Columns.Contains("colLandOwnersName"))
                return "colLandOwnersName";

            if (dgvOwners.Columns.Contains(nameof(OwnerLookupDisplayModel.LandOwnersName)))
                return nameof(OwnerLookupDisplayModel.LandOwnersName);

            return dgvOwners.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(column =>
                    string.Equals(
                        column.DataPropertyName,
                        nameof(OwnerLookupDisplayModel.LandOwnersName),
                        StringComparison.OrdinalIgnoreCase))
                ?.Name
                ?? dgvOwners.Columns
                    .Cast<DataGridViewColumn>()
                    .First(column => column.Visible)
                    .Name;
        }

        private void UpdateSelectedOwnerPreview()
        {
            var owner = GetPrimaryOwner() ?? GetCurrentOwner();
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

        private LandOwner? GetPrimaryOwner()
        {
            return _primaryOwnerId.HasValue
                ? _allOwners.FirstOrDefault(o => o.LandOwnerId == _primaryOwnerId.Value)
                : null;
        }

        private void SetPrimaryOwner(int landOwnerId)
        {
            if (_primaryOwnerId == landOwnerId)
            {
                RefreshPrimaryColumn();
                UpdateSelectedOwnerPreview();
                UpdateLoadButtonState();
                return;
            }

            _primaryOwnerId = landOwnerId;
            SelectedOwner = _allOwners.FirstOrDefault(o => o.LandOwnerId == landOwnerId);
            _selectedCoOwnerIds.Remove(landOwnerId);
            RefreshPrimaryColumn();
            UpdateSelectionSummary();
            UpdateSelectedOwnerPreview();
            UpdateLoadButtonState();
        }

        private void RefreshPrimaryColumn()
        {
            foreach (OwnerLookupDisplayModel item in _displayedOwners)
            {
                item.IsPrimaryOwner = item.LandOwnerId == _primaryOwnerId;
                if (item.IsPrimaryOwner)
                    item.IsCoOwner = false;
            }

            dgvOwners.Refresh();
        }

        private void UpdateSelectionSummary()
        {
            lblCoOwnerCount.Text = $"{_selectedCoOwnerIds.Count} co-owner(s) selected";
        }

        private void UpdateLoadButtonState()
        {
            btnLoad.Enabled = _primaryOwnerId.HasValue;
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
        public bool IsPrimaryOwner { get; set; }
        public bool IsCoOwner { get; set; }
        public int LandOwnerId { get; set; }
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
    }

    internal sealed class DataGridViewRadioButtonColumn : DataGridViewCheckBoxColumn
    {
        public DataGridViewRadioButtonColumn()
        {
            CellTemplate = new DataGridViewRadioButtonCell();
            ThreeState = false;
            FlatStyle = FlatStyle.Flat;
        }
    }

    internal sealed class DataGridViewRadioButtonCell : DataGridViewCheckBoxCell
    {
        public DataGridViewRadioButtonCell()
        {
            ThreeState = false;
        }

        protected override void Paint(
            Graphics graphics,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates elementState,
            object? value,
            object? formattedValue,
            string? errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            base.Paint(
                graphics,
                clipBounds,
                cellBounds,
                rowIndex,
                elementState,
                value,
                formattedValue,
                errorText,
                cellStyle,
                advancedBorderStyle,
                paintParts & ~DataGridViewPaintParts.ContentForeground);

            bool isChecked = value is bool checkedValue && checkedValue;
            const int size = 14;
            Point glyphLocation = new(
                cellBounds.Left + (cellBounds.Width - size) / 2,
                cellBounds.Top + (cellBounds.Height - size) / 2);

            if (Application.RenderWithVisualStyles)
            {
                RadioButtonState state = isChecked
                    ? RadioButtonState.CheckedNormal
                    : RadioButtonState.UncheckedNormal;
                RadioButtonRenderer.DrawRadioButton(graphics, glyphLocation, state);
            }
            else
            {
                ButtonState state = isChecked ? ButtonState.Checked : ButtonState.Normal;
                ControlPaint.DrawRadioButton(graphics, glyphLocation.X, glyphLocation.Y, size, size, state);
            }
        }
    }
}
