using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Core.Models.Import;
using Land_Readjustment_Tool.Services.Assignment;
using System.Drawing;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmCadastralParcelPicker : Form
    {
        private const string AllMapSheets = "All map sheets";

        private readonly ProjectSession _session;
        private readonly ICadastralRecordAssignmentService _assignmentService;
        private readonly string? _preferredMapSheet;
        private readonly string? _preferredParcelNo;
        private readonly Dictionary<string, List<CadastralParcelRecordChoice>> _parcelCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly System.Windows.Forms.Timer _searchTimer = new();
        private List<CadastralParcelRecordChoice> _visibleParcels = [];

        public CadastralParcelRecordChoice? SelectedParcel { get; private set; }

        public frmCadastralParcelPicker(
            ProjectSession session,
            ICadastralRecordAssignmentService assignmentService,
            string? preferredMapSheet,
            string? preferredParcelNo)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
            _preferredMapSheet = preferredMapSheet;
            _preferredParcelNo = preferredParcelNo;

            InitializeComponent();
            _searchTimer.Interval = 180;
            _searchTimer.Tick += (_, _) =>
            {
                _searchTimer.Stop();
                ApplySearchFilter();
            };
            Load += frmCadastralParcelPicker_Load;
            FormClosed += (_, _) => _searchTimer.Dispose();
            _cboMapSheet.SelectedIndexChanged += async (_, _) => await LoadParcelsForSelectedMapSheetAsync();
            _txtSearch.TextChanged += (_, _) =>
            {
                _searchTimer.Stop();
                _searchTimer.Start();
            };
            _dgvParcels.SelectionChanged += (_, _) => _btnAssign.Enabled = GetSelectedParcel() != null;
            _dgvParcels.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0)
                    AcceptSelection();
            };
            _btnAssign.Click += (_, _) => AcceptSelection();
            _btnCancel.Click += (_, _) => Close();
        }

        private async void frmCadastralParcelPicker_Load(object? sender, EventArgs e)
        {
            SetBusy(true, "Loading map sheets...");
            await Task.Yield();
            IReadOnlyList<string> mapSheets = await _assignmentService.GetMapSheetNumbersAsync(_session);
            _cboMapSheet.Items.Clear();
            _cboMapSheet.Items.Add(AllMapSheets);
            foreach (string mapSheet in mapSheets)
                _cboMapSheet.Items.Add(mapSheet);

            SelectComboValue(_cboMapSheet, _preferredMapSheet);
            if (_cboMapSheet.SelectedIndex < 0)
                _cboMapSheet.SelectedIndex = _cboMapSheet.Items.Count > 1 ? 1 : 0;

            if (!string.IsNullOrWhiteSpace(_preferredParcelNo))
                _txtSearch.Text = _preferredParcelNo;

            await LoadParcelsForSelectedMapSheetAsync();
            SetBusy(false);
        }

        private async Task LoadParcelsForSelectedMapSheetAsync()
        {
            string selected = Convert.ToString(_cboMapSheet.SelectedItem) ?? string.Empty;
            SetBusy(true, "Loading parcel records...");
            ReplaceGridRows(_dgvParcels, []);
            await Task.Yield();

            if (selected == AllMapSheets)
            {
                List<CadastralParcelRecordChoice> allParcels = [];
                foreach (object item in _cboMapSheet.Items)
                {
                    string mapSheet = Convert.ToString(item) ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(mapSheet) || mapSheet == AllMapSheets)
                        continue;

                    allParcels.AddRange(await GetParcelsForMapSheetAsync(mapSheet));
                }

                _visibleParcels = allParcels;
            }
            else
            {
                _visibleParcels = await GetParcelsForMapSheetAsync(selected);
            }

            ApplySearchFilter();
            SetBusy(false);
        }

        private async Task<List<CadastralParcelRecordChoice>> GetParcelsForMapSheetAsync(string mapSheet)
        {
            if (!_parcelCache.TryGetValue(mapSheet, out List<CadastralParcelRecordChoice>? parcels))
            {
                parcels = (await _assignmentService.GetParcelsByMapSheetAsync(_session, mapSheet)).ToList();
                _parcelCache[mapSheet] = parcels;
            }

            return parcels;
        }

        private void ApplySearchFilter()
        {
            string query = _txtSearch.Text.Trim();
            IEnumerable<CadastralParcelRecordChoice> matches = _visibleParcels;
            if (!string.IsNullOrWhiteSpace(query))
            {
                matches = matches.Where(parcel =>
                    parcel.ParcelNo.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    parcel.MapSheetNo.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    parcel.DisplayText.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            List<DataGridViewRow> rows = [];
            foreach (CadastralParcelRecordChoice parcel in matches.Take(500))
            {
                DataGridViewRow row = new();
                row.CreateCells(
                    _dgvParcels,
                    parcel.MapSheetNo,
                    parcel.ParcelNo,
                    parcel.OriginalAreaSqm.ToString("0.###"),
                    parcel.CanvasObjectId.HasValue ? "Already assigned" : "Available");
                row.Tag = parcel;
                rows.Add(row);
            }

            ReplaceGridRows(_dgvParcels, rows);

            if (_dgvParcels.Rows.Count > 0)
                _dgvParcels.Rows[0].Selected = true;

            _lblStatus.Text = $"{_dgvParcels.Rows.Count:N0} parcel record(s) shown.";
            _btnAssign.Enabled = GetSelectedParcel() != null;
        }

        private CadastralParcelRecordChoice? GetSelectedParcel()
        {
            return _dgvParcels.CurrentRow?.Tag as CadastralParcelRecordChoice
                ?? _dgvParcels.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault()?.Tag as CadastralParcelRecordChoice;
        }

        private void AcceptSelection()
        {
            SelectedParcel = GetSelectedParcel();
            if (SelectedParcel == null)
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private static void SelectComboValue(ComboBox comboBox, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            for (int index = 0; index < comboBox.Items.Count; index++)
            {
                if (string.Equals(comboBox.Items[index]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = index;
                    return;
                }
            }
        }

        private void SetBusy(bool busy, string? status = null)
        {
            _cboMapSheet.Enabled = !busy;
            _txtSearch.Enabled = !busy;
            _dgvParcels.Enabled = !busy;
            _btnAssign.Enabled = !busy && GetSelectedParcel() != null;
            _btnCancel.Enabled = !busy;
            if (!string.IsNullOrWhiteSpace(status))
                _lblStatus.Text = status;
        }

        private static void ReplaceGridRows(DataGridView grid, IReadOnlyList<DataGridViewRow> rows)
        {
            grid.SuspendLayout();
            try
            {
                grid.Rows.Clear();
                if (rows.Count > 0)
                    grid.Rows.AddRange(rows.ToArray());
            }
            finally
            {
                grid.ResumeLayout();
            }
        }
    }
}
