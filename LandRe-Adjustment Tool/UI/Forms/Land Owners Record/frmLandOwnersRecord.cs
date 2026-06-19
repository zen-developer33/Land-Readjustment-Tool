using Land_Readjustment_Tool.Forms.Land_Owners_Record;
using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services.LandData;
using System.ComponentModel;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Main form for viewing, editing, and managing land owners
    /// </summary>
    public partial class frmLandOwnersRecord : Form
    {
        private readonly string _projectPath;
        private readonly LandRecordsService _landRecordsService;
        private List<LandOwner> _allOwners = [];
        private List<LandOwner> _filteredOwners = [];
        private BindingList<LandOwnerDisplayModel> _displayedOwners = [];
        private readonly bool _readOnlyMode;

        public frmLandOwnersRecord()
            : this(CreateDefaultLandRecordsService(out var projectPath), projectPath)
        {
        }

        public frmLandOwnersRecord(LandRecordsService landRecordsService, string projectPath)
            : this(landRecordsService, projectPath, readOnlyMode: false)
        {
        }

        public frmLandOwnersRecord(
            LandRecordsService landRecordsService,
            string projectPath,
            bool readOnlyMode)
        {
            InitializeComponent();
            _landRecordsService = landRecordsService ?? throw new ArgumentNullException(nameof(landRecordsService));
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));
            _readOnlyMode = readOnlyMode;
            Text = _readOnlyMode
                ? "Land Owners Record (Read-Only)"
                : "Land Owners Record";

            SetupEventHandlers();
            SetupDataGridView();
            ApplyReadOnlyMode();
        }

        private static LandRecordsService CreateDefaultLandRecordsService(out string projectPath)
        {
            if (!AppServices.HasContext)
                throw new InvalidOperationException("No open project context found.");

            projectPath = AppServices.Context.ProjectFilePath;
            return new LandRecordsService(AppServices.Context.Session, projectPath);
        }

        private void SetupEventHandlers()
        {
            // Toolbar buttons
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnRefresh.Click += BtnRefresh_Click;
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // DataGridView events
            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            dgvRecords.CellDoubleClick += DgvRecords_CellDoubleClick;
            dgvRecords.RowPostPaint += DgvRecords_RowPostPaint;
        }

        private void SetupDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.MultiSelect = false;
            dgvRecords.ReadOnly = true;
            dgvRecords.DoubleBuffered(true);
            dgvRecords.RowHeadersVisible = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.GridColor = Color.FromArgb(220, 220, 220);

            dgvRecords.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvRecords.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRecords.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            dgvRecords.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;

            dgvRecords.Columns.Clear();

            AddColumn("LandOwnerId", "Owner ID", 80);
            AddColumn("LandOwnersName", "Owner Name", 200);
            AddColumn("FatherSpouse", "Father/Spouse", 180);
            AddColumn("Gender", "Gender", 80);
            AddColumn("CitizenshipNumber", "Citizenship No", 130);
            AddColumn("IssuedDistrict", "Issued District", 150);
            AddColumn("IssuedDate", "Issued Date", 120);
            AddColumn("PermanentAddress", "Permanent Address", 250);
            AddColumn("TemporaryAddress", "Temporary Address", 250);
            AddColumn("ContactNumber", "Contact Number", 120);
            AddColumn("EmailID", "Email", 180);
            AddColumn("ParcelCount", "Parcels", 80);
            AddColumn("TotalAreaSqm", "Total Area (sqm)", 120);

            foreach (DataGridViewColumn col in dgvRecords.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecords.ColumnHeadersHeight = 34;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvRecords.RowTemplate.Height = 28;

            // Alternating row colors
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);
        }

        private void AddColumn(string dataPropertyName, string headerText, int width)
        {
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = dataPropertyName,
                HeaderText = headerText,
                DataPropertyName = dataPropertyName,
                Width = width
            });
        }

        private void frmLandOwnersRecord_Load(object sender, EventArgs e)
        {
            LoadOwners();
            UpdateButtonStates();
        }

        private void LoadOwners()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _allOwners = _landRecordsService.GetAllOwners();
                _filteredOwners = [.. _allOwners];

                BindOwnersToGrid(_filteredOwners);
                UpdateStatusLabels();

                // Clear selection after loading
                dgvRecords.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load owners: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allOwners = [];
                _filteredOwners = [];
                BindOwnersToGrid(_filteredOwners);
                UpdateStatusLabels();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BindOwnersToGrid(List<LandOwner> owners)
        {
            var ownerStats = _landRecordsService.GetOwnerParcelStats();
            var displayModels = owners.Select(o => new LandOwnerDisplayModel
            {
                ParcelCount = ownerStats.TryGetValue(o.LandOwnerId, out var stats) ? stats.ParcelCount : 0,
                TotalAreaSqm = stats?.TotalAreaSqm ?? 0,
                LandOwnerId = o.LandOwnerId,
                LandOwnersName = o.LandOwnersName ?? "",
                FatherSpouse = o.FatherSpouse ?? "",
                Gender = o.Gender ?? "",
                CitizenshipNumber = o.CitizenshipNumber ?? "",
                IssuedDistrict = o.CitizenshipIssuedDistrict ?? "",
                IssuedDate = o.CitizenshipIssuedDate ?? "",
                PermanentAddress = o.PermanentAddress ?? "",
                TemporaryAddress = o.TemporaryAddress ?? "",
                ContactNumber = o.ContactNumber ?? "",
                EmailID = o.EmailID ?? ""
            }).ToList();

            _displayedOwners = new BindingList<LandOwnerDisplayModel>(displayModels);
            dgvRecords.DataSource = _displayedOwners;
        }

        private void UpdateStatusLabels()
        {
            lblTotalRecords.Text = $"Total Owners: {_filteredOwners.Count}";
            if (_filteredOwners.Count != _allOwners.Count)
            {
                lblPaginationInfo.Text = $"Showing {_filteredOwners.Count} of {_allOwners.Count}";
            }
            else
            {
                lblPaginationInfo.Text = "";
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            string searchText = txtSearch.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _filteredOwners = [.. _allOwners];
            }
            else
            {
                _filteredOwners = _allOwners.Where(o =>
                    (o.LandOwnersName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.FatherSpouse?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.CitizenshipNumber?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.PermanentAddress?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.TemporaryAddress?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).ToList();
            }

            BindOwnersToGrid(_filteredOwners);
            UpdateStatusLabels();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

            using var addForm = new frmLandOwnerDetails(_landRecordsService, _projectPath);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadOwners();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1) return;
            ViewOwnerDetails();
        }

        private void ViewOwnerDetails()
        {
            if (dgvRecords.SelectedRows[0].DataBoundItem is not LandOwnerDisplayModel model)
                return;

            using var detailsForm = new frmLandOwnerDetails(
                model.LandOwnerId,
                readOnlyMode: true,
                _landRecordsService,
                _projectPath,
                allowEditInReadOnly: !_readOnlyMode);
            if (detailsForm.ShowDialog() == DialogResult.OK)
            {
                if (!_readOnlyMode)
                {
                    LoadOwners();
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
                return;

            if (dgvRecords.SelectedRows.Count == 0) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete the selected owner?\n\nThis will also delete all associated parcels!\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                if (dgvRecords.SelectedRows[0].DataBoundItem is LandOwnerDisplayModel model)
                {
                    _landRecordsService.DeleteOwnerWithParcels(model.LandOwnerId);
                    LoadOwners();

                    MessageBox.Show("Owner deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete owner: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadOwners();
        }

        private void DgvRecords_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void DgvRecords_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            ViewOwnerDetails();
        }

        private void DgvRecords_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            string rowNumber = (e.RowIndex + 1).ToString();

            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dgvRecords.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                rowNumber,
                dgvRecords.DefaultCellStyle.Font,
                headerBounds,
                dgvRecords.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvRecords.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection && !_readOnlyMode;
            btnAdd.Enabled = !_readOnlyMode;
            saveToolStripButton.Enabled = !_readOnlyMode;
        }

        private void ApplyReadOnlyMode()
        {
            btnAdd.Enabled = !_readOnlyMode;
            btnDelete.Enabled = false;
            saveToolStripButton.Enabled = !_readOnlyMode;
            btnEdit.Text = _readOnlyMode ? "View" : "Edit";
            btnEdit.ToolTipText = _readOnlyMode ? "View Record" : "Edit Record";
            dgvRecords.ReadOnly = true;
        }

        public void ConfigureAsRegister(string title)
        {
            Text = string.IsNullOrWhiteSpace(title) ? "Owner Register" : title;
            btnAdd.Visible = false;
            btnDelete.Visible = false;
            saveToolStripButton.Visible = false;
            btnEdit.Text = "View";
            btnEdit.ToolTipText = "View selected owner";
            dgvRecords.ReadOnly = true;
        }

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnEdit_Click_1(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Display model for land owner records
    /// </summary>
    public class LandOwnerDisplayModel
    {
        public int LandOwnerId { get; set; }
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string Gender { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string IssuedDistrict { get; set; } = "";
        public string IssuedDate { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
        public string TemporaryAddress { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string EmailID { get; set; } = "";
        public int ParcelCount { get; set; }
        public double TotalAreaSqm { get; set; }
    }
}

