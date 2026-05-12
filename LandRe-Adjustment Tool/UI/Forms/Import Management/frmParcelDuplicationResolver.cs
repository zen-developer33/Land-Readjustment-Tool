using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services.Import;

namespace Land_Readjustment_Tool.Forms
{
    public sealed partial class frmParcelDuplicationResolver : Form
    {
        private readonly List<BaselineLandParcelRecord> _records;
        private List<ParcelDuplicateResolutionService.ParcelDuplicateGroup> _groups = new();
        private readonly HashSet<string> _resolvedKeys = new(StringComparer.OrdinalIgnoreCase);

        public bool ChangesWereMade { get; private set; }

        public frmParcelDuplicationResolver(List<BaselineLandParcelRecord> records)
        {
            _records = records ?? throw new ArgumentNullException(nameof(records));
            InitializeComponent();
            ReloadGroups();
        }

        private void ReloadGroups()
        {
            _groups = ParcelDuplicateResolutionService.FindDuplicateGroups(_records);
            dgvGroups.Rows.Clear();

            foreach (var group in _groups)
            {
                var rowIndex = dgvGroups.Rows.Add();
                var row = dgvGroups.Rows[rowIndex];
                row.Cells["Status"].Value = _resolvedKeys.Contains(group.Key) ? "Resolved" : "Pending";
                row.Cells["MapSheet"].Value = group.MapSheetNo;
                row.Cells["Parcel"].Value = group.ParcelNo;
                row.Cells["Owners"].Value = group.OwnersSummary;
                row.Tag = group;

            }

            lblSummary.Text = _groups.Count == 0
                ? "No duplicate parcel groups remain."
                : $"{_groups.Count} duplicate parcel group(s) found. Review a parcel, choose the primary owner, then confirm joint ownership.";

            btnResolveSelected.Enabled = _groups.Any(g => !_resolvedKeys.Contains(g.Key));
            btnSetSelectedJointOwnership.Enabled = _groups.Any(g => !_resolvedKeys.Contains(g.Key));
            btnResolveAll.Enabled = _groups.Any(g => !_resolvedKeys.Contains(g.Key));

            if (dgvGroups.Rows.Count > 0)
            {
                dgvGroups.Rows[0].Selected = true;
                PopulateOwners(_groups[0]);
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
            {
                PopulateOwners(group);
            }
        }

        private void PopulateOwners(ParcelDuplicateResolutionService.ParcelDuplicateGroup group)
        {
            dgvOwners.Rows.Clear();

            for (int i = 0; i < group.Records.Count; i++)
            {
                var record = group.Records[i];
                var rowIndex = dgvOwners.Rows.Add();
                var row = dgvOwners.Rows[rowIndex];
                row.Cells["Role"].Value = i == 0 ? "Default primary" : "Co-owner";
                row.Cells["Owner"].Value = record.LandOwnersName ?? string.Empty;
                row.Cells["Father"].Value = record.FatherSpouse ?? string.Empty;
                row.Cells["Citizenship"].Value = record.CitizenshipNumber ?? string.Empty;
                row.Cells["Address"].Value = record.PermanentAddress ?? string.Empty;
            }
        }

        private void BtnResolveSelected_Click(object? sender, EventArgs e)
        {
            if (dgvGroups.SelectedRows.Count == 0)
                return;

            if (dgvGroups.SelectedRows[0].Tag is not ParcelDuplicateResolutionService.ParcelDuplicateGroup group)
                return;

            if (_resolvedKeys.Contains(group.Key))
                return;

            using var resolver = new frmJointOwnershipResolver(group.Records, group.Key);
            if (resolver.ShowDialog(this) != DialogResult.OK)
                return;

            ParcelDuplicateResolutionService.ApplyJointOwnership(group, resolver.PrimaryIndex);
            _resolvedKeys.Add(group.Key);
            ChangesWereMade = true;
            ReloadGroups();
        }

        private void BtnResolveAll_Click(object? sender, EventArgs e)
        {
            var pending = _groups.Where(g => !_resolvedKeys.Contains(g.Key)).ToList();
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
                _resolvedKeys.Add(group.Key);
            }

            ChangesWereMade = true;
            ReloadGroups();
        }

        private void BtnSetSelectedJointOwnership_Click(object? sender, EventArgs e)
        {
            var selectedGroups = dgvGroups.SelectedRows
                .Cast<DataGridViewRow>()
                .Select(row => row.Tag as ParcelDuplicateResolutionService.ParcelDuplicateGroup)
                .Where(group => group != null && !_resolvedKeys.Contains(group.Key))
                .Cast<ParcelDuplicateResolutionService.ParcelDuplicateGroup>()
                .DistinctBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (selectedGroups.Count == 0)
            {
                MessageBox.Show("Please select at least one pending duplicate parcel group.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                $"Set {selectedGroups.Count} selected duplicate parcel group(s) as joint ownership using the first row as primary owner?",
                "Confirm Joint Ownership",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            foreach (var group in selectedGroups)
            {
                ParcelDuplicateResolutionService.ApplyJointOwnership(group, primaryIndex: 0);
                _resolvedKeys.Add(group.Key);
            }

            ChangesWereMade = true;
            ReloadGroups();
        }

        private void BtnClose_Click(object? sender, EventArgs e)
        {
            DialogResult = ChangesWereMade ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }
    }
}
