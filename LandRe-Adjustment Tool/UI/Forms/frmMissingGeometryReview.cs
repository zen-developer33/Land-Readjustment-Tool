using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed partial class frmMissingGeometryReview : Form
    {
        private readonly DataQualityReviewService _service;
        private readonly bool _readOnlyMode;

        private List<DataQualityReviewService.MissingGeometryIssue> _issues = [];
        private bool _isBusy;

        public event Action? OpenParcelLinkReviewRequested;
        public event Action? GeometryLinksChanged;

        public frmMissingGeometryReview(
            DataQualityReviewService service,
            bool readOnlyMode = false)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _readOnlyMode = readOnlyMode;

            InitializeComponent();
            WireEvents();
            if (_readOnlyMode)
            {
                Text = "Missing Geometry Review (Read Only)";
                _clearBrokenLinksButton.Enabled = false;
            }
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadIssuesAsync();
        }

        private void WireEvents()
        {
            _searchBox.TextChanged += (_, _) => ApplyFilter();
            _issueFilter.SelectedIndexChanged += (_, _) => ApplyFilter();
            _refreshButton.Click += async (_, _) => await LoadIssuesAsync();
            _exportButton.Click += (_, _) => ExportCsv();
            _clearBrokenLinksButton.Click += async (_, _) => await ClearBrokenLinksAsync();
            _openParcelLinkReviewButton.Click += (_, _) => OpenParcelLinkReviewRequested?.Invoke();
            _closeButton.Click += (_, _) => Close();
        }

        private async Task LoadIssuesAsync(bool force = false)
        {
            if (_isBusy && !force)
                return;

            SetBusy(true, "Loading geometry issues...");
            try
            {
                _issues = (await _service.GetMissingGeometryIssuesAsync()).ToList();
                PopulateIssueFilter();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not load missing geometry issues: {ex.Message}",
                    "Missing Geometry Review",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                _statusLabel.Text = "Could not load geometry issues.";
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void PopulateIssueFilter()
        {
            string previous = _issueFilter.SelectedItem?.ToString() ?? "All issues";
            _issueFilter.Items.Clear();
            _issueFilter.Items.Add("All issues");
            foreach (string issueType in _issues
                .Select(issue => issue.IssueType)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase))
            {
                _issueFilter.Items.Add(issueType);
            }

            int previousIndex = _issueFilter.Items.IndexOf(previous);
            _issueFilter.SelectedIndex = previousIndex >= 0 ? previousIndex : 0;
        }

        private void ApplyFilter()
        {
            string search = _searchBox.Text.Trim();
            string issueFilter = _issueFilter.SelectedItem?.ToString() ?? "All issues";
            IEnumerable<DataQualityReviewService.MissingGeometryIssue> filtered = _issues;

            if (!string.Equals(issueFilter, "All issues", StringComparison.OrdinalIgnoreCase))
            {
                filtered = filtered.Where(issue =>
                    string.Equals(issue.IssueType, issueFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(issue =>
                    Contains(issue.IssueType, search) ||
                    Contains(issue.MapSheetNo, search) ||
                    Contains(issue.ParcelNo, search) ||
                    Contains(issue.OwnerName, search) ||
                    Contains(issue.Detail, search) ||
                    Contains(issue.CanvasObjectId?.ToString(), search));
            }

            List<DataQualityReviewService.MissingGeometryIssue> rows = filtered.ToList();
            _grid.Rows.Clear();
            foreach (DataQualityReviewService.MissingGeometryIssue issue in rows)
            {
                int rowIndex = _grid.Rows.Add(
                    issue.IssueType,
                    issue.MapSheetNo,
                    issue.ParcelNo,
                    issue.OwnerName,
                    issue.AreaSqm.ToString("0.###"),
                    issue.CanvasObjectId?.ToString() ?? "-",
                    issue.Detail);
                DataGridViewRow row = _grid.Rows[rowIndex];
                row.Tag = issue;
                if (issue.CanClearParcelLink)
                    row.Cells["IssueType"].Style.ForeColor = Color.FromArgb(176, 71, 61);
            }

            _grid.ClearSelection();
            _statusLabel.Text = rows.Count == 0
                ? "No missing or broken original parcel geometry links found."
                : $"{rows.Count:N0} of {_issues.Count:N0} geometry issue(s) shown.";
            _clearBrokenLinksButton.Enabled = !_readOnlyMode && rows.Any(issue => issue.CanClearParcelLink);
            _exportButton.Enabled = _issues.Count > 0;
        }

        private async Task ClearBrokenLinksAsync()
        {
            if (_readOnlyMode)
                return;

            List<DataQualityReviewService.MissingGeometryIssue> selected = GetSelectedIssues()
                .Where(issue => issue.CanClearParcelLink)
                .ToList();

            if (selected.Count == 0)
            {
                selected = _issues.Where(issue => issue.CanClearParcelLink).ToList();
                if (selected.Count == 0)
                    return;
            }

            DialogResult confirm = MessageBox.Show(
                this,
                $"Clear stale CanvasObjectId values for {selected.Count:N0} parcel record(s)? This does not delete parcel records or drawing objects.",
                "Clear Broken Links",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.OK)
                return;

            SetBusy(true, "Clearing broken links...");
            try
            {
                int cleared = await _service.ClearBrokenGeometryLinksAsync(selected.Select(issue => issue.ParcelId));
                if (cleared > 0)
                    GeometryLinksChanged?.Invoke();

                _statusLabel.Text = $"Cleared {cleared:N0} stale parcel link(s).";
                await LoadIssuesAsync(force: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not clear broken links: {ex.Message}",
                    "Missing Geometry Review",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ExportCsv()
        {
            using SaveFileDialog dialog = new()
            {
                Title = "Export Missing Geometry Review",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = "Missing Geometry Review.csv",
                RestoreDirectory = true
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                File.WriteAllText(dialog.FileName, DataQualityReviewService.BuildMissingGeometryCsv(_issues));
                _statusLabel.Text = $"Exported {_issues.Count:N0} issue(s).";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Could not export review CSV: {ex.Message}",
                    "Missing Geometry Review",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private List<DataQualityReviewService.MissingGeometryIssue> GetSelectedIssues()
        {
            return _grid.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => row.Tag as DataQualityReviewService.MissingGeometryIssue)
                .Where(issue => issue != null)
                .Cast<DataQualityReviewService.MissingGeometryIssue>()
                .ToList();
        }

        private void SetBusy(bool busy, string? status = null)
        {
            _isBusy = busy;
            UseWaitCursor = busy;
            _searchBox.Enabled = !busy;
            _issueFilter.Enabled = !busy;
            _refreshButton.Enabled = !busy;
            _openParcelLinkReviewButton.Enabled = !busy;
            _clearBrokenLinksButton.Enabled = !busy && !_readOnlyMode && _issues.Any(issue => issue.CanClearParcelLink);
            _exportButton.Enabled = !busy && _issues.Count > 0;
            if (!string.IsNullOrWhiteSpace(status))
                _statusLabel.Text = status;
        }

        private static bool Contains(string? value, string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Contains(search, StringComparison.OrdinalIgnoreCase);
        }
    }
}
