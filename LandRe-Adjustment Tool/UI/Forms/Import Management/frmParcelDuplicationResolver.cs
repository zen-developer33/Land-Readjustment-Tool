using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services.Import;

namespace Land_Readjustment_Tool.Forms
{
    public sealed partial class frmParcelDuplicationResolver : Form
    {
        private readonly List<BaselineLandParcelRecord> _records;

        /// <summary>All duplicate groups detected at load time. Never re-queried so merged groups stay visible.</summary>
        private readonly List<ParcelDuplicateResolutionService.ParcelDuplicateGroup> _allGroups;

        /// <summary>Keys of groups the user has merged into joint ownership.</summary>
        private readonly HashSet<string> _mergedKeys = new(StringComparer.OrdinalIgnoreCase);

        public bool ChangesWereMade { get; private set; }

        public frmParcelDuplicationResolver(List<BaselineLandParcelRecord> records)
        {
            _records = records ?? throw new ArgumentNullException(nameof(records));
            InitializeComponent();

            // Detect groups once; keep them for the lifetime of the form so merged rows stay visible.
            _allGroups = ParcelDuplicateResolutionService.FindDuplicateGroups(_records);

            // Context menu: Merge / Unmerge
            var ctxMenu = new ContextMenuStrip();
            var miMerge   = new ToolStripMenuItem("Merge to Joint Ownership");
            var miUnmerge = new ToolStripMenuItem("Unmerge");
            miMerge.Click   += (_, _) => MergeSelected();
            miUnmerge.Click += (_, _) => UnmergeSelected();
            ctxMenu.Items.AddRange([miMerge, miUnmerge]);
            ctxMenu.Opening += (_, _) =>
            {
                var hasPending = SelectedGroups().Any(g => !_mergedKeys.Contains(g.Key));
                var hasMerged  = SelectedGroups().Any(g =>  _mergedKeys.Contains(g.Key));
                miMerge.Enabled   = hasPending;
                miUnmerge.Enabled = hasMerged;
            };
            dgvGroups.ContextMenuStrip = ctxMenu;

            RefreshDisplay();
        }

        // ── Display ────────────────────────────────────────────────────────────────

        private void RefreshDisplay()
        {
            // Remember which group was selected so we can restore it after rebuilding.
            var selectedKey = dgvGroups.SelectedRows.Count > 0
                ? (dgvGroups.SelectedRows[0].Tag as ParcelDuplicateResolutionService.ParcelDuplicateGroup)?.Key
                : null;

            dgvGroups.SuspendLayout();
            dgvGroups.Rows.Clear();

            int pendingCount = 0, mergedCount = 0, restoreIndex = -1;

            for (int i = 0; i < _allGroups.Count; i++)
            {
                var group    = _allGroups[i];
                var isMerged = _mergedKeys.Contains(group.Key);

                var rowIndex = dgvGroups.Rows.Add();
                var row      = dgvGroups.Rows[rowIndex];
                row.Cells["Status"].Value   = isMerged ? "Merged" : "Pending";
                row.Cells["MapSheet"].Value = group.MapSheetNo;
                row.Cells["Parcel"].Value   = group.ParcelNo;
                row.Cells["Owners"].Value   = group.OwnersSummary;
                row.Tag = group;

                if (isMerged)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); // light green
                    mergedCount++;
                }
                else
                {
                    pendingCount++;
                }

                if (string.Equals(group.Key, selectedKey, StringComparison.OrdinalIgnoreCase))
                    restoreIndex = i;
            }

            dgvGroups.ResumeLayout();

            lblSummary.Text = _allGroups.Count == 0
                ? "No duplicate parcel groups found."
                : $"{_allGroups.Count} duplicate parcel group(s) found — {pendingCount} Pending, {mergedCount} Merged." +
                  (pendingCount > 0 ? " Review a parcel, choose the primary owner, then confirm joint ownership." : string.Empty);

            var hasPending = pendingCount > 0;
            btnResolveSelected.Enabled          = hasPending;
            btnSetSelectedJointOwnership.Enabled = hasPending;
            btnResolveAll.Enabled               = hasPending;
            btnUnmergeSelected.Enabled          = mergedCount > 0;

            if (dgvGroups.Rows.Count > 0)
            {
                var selectRow = restoreIndex >= 0 ? restoreIndex : 0;
                dgvGroups.Rows[selectRow].Selected = true;
                PopulateOwners(_allGroups[selectRow]);
            }
            else
            {
                dgvOwners.Rows.Clear();
            }
        }

        private void DgvGroups_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvGroups.SelectedRows.Count == 0)
                return;

            if (dgvGroups.SelectedRows[0].Tag is ParcelDuplicateResolutionService.ParcelDuplicateGroup group)
                PopulateOwners(group);
        }

        private void PopulateOwners(ParcelDuplicateResolutionService.ParcelDuplicateGroup group)
        {
            dgvOwners.Rows.Clear();
            var isMerged = _mergedKeys.Contains(group.Key);

            for (int i = 0; i < group.Records.Count; i++)
            {
                var record   = group.Records[i];
                var rowIndex = dgvOwners.Rows.Add();
                var row      = dgvOwners.Rows[rowIndex];

                // Pending: first row is the default primary candidate; after merge: reflect actual flags.
                string role = isMerged
                    ? (record.IsJointCoOwnerRow ? "Co-owner" : "Primary owner")
                    : (i == 0 ? "Default primary" : "Co-owner");

                row.Cells["Role"].Value        = role;
                row.Cells["Owner"].Value       = record.LandOwnersName    ?? string.Empty;
                row.Cells["Father"].Value      = record.FatherSpouse       ?? string.Empty;
                row.Cells["Citizenship"].Value = record.CitizenshipNumber  ?? string.Empty;
                row.Cells["Address"].Value     = record.PermanentAddress   ?? string.Empty;
            }
        }

        // ── Button handlers ────────────────────────────────────────────────────────

        private void BtnResolveSelected_Click(object? sender, EventArgs e)
        {
            if (dgvGroups.SelectedRows.Count == 0)
                return;

            if (dgvGroups.SelectedRows[0].Tag is not ParcelDuplicateResolutionService.ParcelDuplicateGroup group)
                return;

            if (_mergedKeys.Contains(group.Key))
                return;

            using var resolver = new frmJointOwnershipResolver(group.Records, group.Key);
            if (resolver.ShowDialog(this) != DialogResult.OK)
                return;

            ParcelDuplicateResolutionService.ApplyJointOwnership(group, resolver.PrimaryIndex);
            _mergedKeys.Add(group.Key);
            ChangesWereMade = true;
            RefreshDisplay();
        }

        private void BtnSetSelectedJointOwnership_Click(object? sender, EventArgs e) => MergeSelected();

        private void BtnUnmergeSelected_Click(object? sender, EventArgs e) => UnmergeSelected();

        private void BtnResolveAll_Click(object? sender, EventArgs e)
        {
            var pending = _allGroups.Where(g => !_mergedKeys.Contains(g.Key)).ToList();
            if (pending.Count == 0)
                return;

            var confirm = MessageBox.Show(
                $"Set all {pending.Count} duplicate parcel group(s) as joint ownership using the first row as primary owner?",
                "Confirm Joint Ownership",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            foreach (var group in pending)
            {
                ParcelDuplicateResolutionService.ApplyJointOwnership(group, primaryIndex: 0);
                _mergedKeys.Add(group.Key);
            }

            ChangesWereMade = true;
            RefreshDisplay();
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            DialogResult = ChangesWereMade ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }

        // ── Core merge / unmerge logic ─────────────────────────────────────────────

        private void MergeSelected()
        {
            var pending = SelectedGroups()
                .Where(g => !_mergedKeys.Contains(g.Key))
                .ToList();

            if (pending.Count == 0)
            {
                MessageBox.Show("Please select at least one pending duplicate parcel group.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Set {pending.Count} selected duplicate parcel group(s) as joint ownership using the first row as primary owner?",
                "Confirm Joint Ownership",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            foreach (var group in pending)
            {
                ParcelDuplicateResolutionService.ApplyJointOwnership(group, primaryIndex: 0);
                _mergedKeys.Add(group.Key);
            }

            ChangesWereMade = true;
            RefreshDisplay();
        }

        private void UnmergeSelected()
        {
            var merged = SelectedGroups()
                .Where(g => _mergedKeys.Contains(g.Key))
                .ToList();

            if (merged.Count == 0)
            {
                MessageBox.Show("Please select at least one merged group to unmerge.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Unmerge {merged.Count} selected group(s)? The records will revert to separate ownership rows.",
                "Confirm Unmerge",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            foreach (var group in merged)
            {
                ParcelDuplicateResolutionService.UndoJointOwnership(group);
                _mergedKeys.Remove(group.Key);
            }

            ChangesWereMade = true;
            RefreshDisplay();
        }

        // ── Helpers ────────────────────────────────────────────────────────────────

        /// <summary>Returns distinct selected groups from dgvGroups.</summary>
        private IEnumerable<ParcelDuplicateResolutionService.ParcelDuplicateGroup> SelectedGroups() =>
            dgvGroups.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(r => r.Tag as ParcelDuplicateResolutionService.ParcelDuplicateGroup)
                .Where(g => g != null)
                .Cast<ParcelDuplicateResolutionService.ParcelDuplicateGroup>()
                .DistinctBy(g => g.Key, StringComparer.OrdinalIgnoreCase);
    }
}
