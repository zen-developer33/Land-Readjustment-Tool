using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed class frmMissingGeometryReview : Form
    {
        private readonly DataQualityReviewService _service;
        private readonly DataGridView _grid = new();
        private readonly TextBox _searchBox = new();
        private readonly ComboBox _issueFilter = new();
        private readonly Label _statusLabel = new();
        private readonly Button _clearBrokenLinksButton = new();
        private readonly Button _openParcelLinkReviewButton = new();
        private readonly Button _refreshButton = new();
        private readonly Button _exportButton = new();
        private readonly Button _closeButton = new();

        private List<DataQualityReviewService.MissingGeometryIssue> _issues = [];
        private bool _isBusy;

        public event Action? OpenParcelLinkReviewRequested;
        public event Action? GeometryLinksChanged;

        public frmMissingGeometryReview(DataQualityReviewService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));

            Text = "Missing Geometry Review";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(920, 560);
            Size = new Size(1120, 680);
            Font = new Font("Segoe UI", 9F);

            BuildLayout();
            ConfigureGrid();
            WireEvents();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadIssuesAsync();
        }

        private void BuildLayout()
        {
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            FlowLayoutPanel filters = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            filters.Controls.Add(new Label
            {
                AutoSize = false,
                Text = "Search",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 58,
                Height = 30
            });
            _searchBox.Width = 260;
            filters.Controls.Add(_searchBox);
            filters.Controls.Add(new Label
            {
                AutoSize = false,
                Text = "Issue",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = 48,
                Height = 30,
                Margin = new Padding(16, 3, 3, 3)
            });
            _issueFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            _issueFilter.Width = 230;
            filters.Controls.Add(_issueFilter);

            _statusLabel.Dock = DockStyle.Fill;
            _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            _statusLabel.ForeColor = Color.FromArgb(75, 85, 99);

            FlowLayoutPanel actions = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };
            _closeButton.Text = "Close";
            _closeButton.Width = 94;
            _openParcelLinkReviewButton.Text = "Open Parcel-Link Review";
            _openParcelLinkReviewButton.Width = 176;
            _clearBrokenLinksButton.Text = "Clear Broken Links";
            _clearBrokenLinksButton.Width = 142;
            _exportButton.Text = "Export CSV";
            _exportButton.Width = 104;
            _refreshButton.Text = "Refresh";
            _refreshButton.Width = 94;
            actions.Controls.AddRange([
                _closeButton,
                _openParcelLinkReviewButton,
                _clearBrokenLinksButton,
                _exportButton,
                _refreshButton
            ]);

            layout.Controls.Add(filters, 0, 0);
            layout.Controls.Add(_grid, 0, 1);
            layout.Controls.Add(_statusLabel, 0, 2);
            layout.Controls.Add(actions, 0, 3);
            Controls.Add(layout);
        }

        private void ConfigureGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.AutoGenerateColumns = false;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.MultiSelect = true;
            _grid.ReadOnly = true;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 247, 250);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);

            AddColumn("IssueType", "Issue", 170);
            AddColumn("MapSheetNo", "Map Sheet", 105);
            AddColumn("ParcelNo", "Parcel No", 105);
            AddColumn("OwnerName", "Owner", 220);
            AddColumn("AreaSqm", "Area sq.m", 100);
            AddColumn("CanvasObjectId", "Canvas Object", 210);
            AddColumn("Detail", "Detail", 360, fill: true);
        }

        private void AddColumn(string name, string header, int width, bool fill = false)
        {
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                DataPropertyName = name,
                Width = width,
                AutoSizeMode = fill ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None
            });
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
            _clearBrokenLinksButton.Enabled = rows.Any(issue => issue.CanClearParcelLink);
            _exportButton.Enabled = _issues.Count > 0;
        }

        private async Task ClearBrokenLinksAsync()
        {
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
            _clearBrokenLinksButton.Enabled = !busy && _issues.Any(issue => issue.CanClearParcelLink);
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
