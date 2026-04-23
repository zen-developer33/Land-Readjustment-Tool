using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;
using DuplicateGroup = Land_Readjustment_Tool.Services.OwnerDeduplicationService.DuplicateGroup;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Manual review surface for owner deduplication decisions.
    /// Supports reviewing both auto-merged groups and suggestion groups.
    /// </summary>
    public partial class frmReviewDuplicates : Form
    {
        private readonly List<DuplicateGroup> _duplicateGroups;
        private readonly BindingList<BaselineLandParceRecord> _allRecords;
        private readonly OwnerDeduplicationService.DeduplicationResult _deduplicationResult;
        private readonly Dictionary<int, List<OwnerRowData>> _groupOwnerRowsCache = new();
        private readonly Dictionary<int, string> _mapSheetByParcelIndex;

        private readonly Stack<Dictionary<int, UserDecision>> _undoStack = new();
        private readonly Dictionary<int, UserDecision> _userDecisions = new();
        private readonly HashSet<string> _uniqueOwnerSignatures = new(StringComparer.Ordinal);

        private bool _showMergedRows;
        private int _currentGroupIndex = -1;

        public bool ChangesWereMade { get; private set; }

        private enum UserDecision
        {
            Merge,
            KeepSeparate
        }

        private sealed class OwnerRowData
        {
            public string OwnerSn { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public string FatherSpouse { get; init; } = string.Empty;
            public string Citizenship { get; init; } = string.Empty;
            public string Parcels { get; init; } = string.Empty;
            public string MapSheets { get; init; } = string.Empty;
            public bool IsBestOwner { get; init; }
        }

        public frmReviewDuplicates(
            OwnerDeduplicationService.DeduplicationResult deduplicationResult,
            BindingList<BaselineLandParceRecord> allRecords)
        {
            InitializeComponent();

            _deduplicationResult = deduplicationResult;
            _duplicateGroups = deduplicationResult.DuplicatesNeedingReview;
            _allRecords = allRecords;
            _mapSheetByParcelIndex = BuildMapSheetLookup(allRecords);

            // If every group is auto-merged, show them immediately so the list is not empty.
            _showMergedRows = _duplicateGroups.All(g => g.IsAutoMerged);

            var menu = new ContextMenuStrip();
            var miMerge = new ToolStripMenuItem("Set Merge");
            var miKeep = new ToolStripMenuItem("Set Keep Separate");
            miMerge.Click += (s, e) => ApplyDecisionToSelectedRows(UserDecision.Merge);
            miKeep.Click += (s, e) => ApplyDecisionToSelectedRows(UserDecision.KeepSeparate);
            menu.Items.AddRange([miMerge, miKeep]);
            dgvDuplicateGroups.ContextMenuStrip = menu;
        }

        private static Dictionary<int, string> BuildMapSheetLookup(IList<BaselineLandParceRecord> records)
        {
            var lookup = new Dictionary<int, string>(records.Count);
            for (int i = 0; i < records.Count; i++)
            {
                lookup[i] = records[i].MapSheetNo ?? string.Empty;
            }

            return lookup;
        }

        private void frmReviewDuplicates_Load(object sender, EventArgs e)
        {
            lblInstructions.Text = "High-confidence duplicates may be auto-merged. Use 'Show Auto-Merged' to review and override when needed.";
            InitializeGrid();
            RestoreUserDecisions();
            RebuildUniqueOwnerSignatures();
            LoadDuplicateGroups();

            if (_duplicateGroups.Count == 0)
            {
                MessageBox.Show(
                    "No potential duplicates to review.",
                    "No Duplicates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Close();
            }
        }

        private void InitializeGrid()
        {
            dgvDuplicateGroups.AutoGenerateColumns = false;
            dgvDuplicateGroups.AllowUserToAddRows = false;
            dgvDuplicateGroups.ReadOnly = true;
            dgvDuplicateGroups.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvDuplicateGroups.MultiSelect = true;
            foreach (DataGridViewColumn col in dgvDuplicateGroups.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }

            dgvGroupOwners.AutoGenerateColumns = false;
            dgvGroupOwners.AllowUserToAddRows = false;
            dgvGroupOwners.ReadOnly = true;
            dgvGroupOwners.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvGroupOwners.MultiSelect = false;
            foreach (DataGridViewColumn col in dgvGroupOwners.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
        }

        private void RestoreUserDecisions()
        {
            _userDecisions.Clear();
            for (int i = 0; i < _duplicateGroups.Count; i++)
            {
                var decision = _duplicateGroups[i].UserDecision;
                if (!decision.HasValue)
                {
                    continue;
                }

                _userDecisions[i] = decision.Value == OwnerDeduplicationService.UserDecisionType.Merge
                    ? UserDecision.Merge
                    : UserDecision.KeepSeparate;
            }
        }

        private void SaveUserDecisionsToResult()
        {
            foreach (var decision in _userDecisions)
            {
                var group = _duplicateGroups[decision.Key];
                group.UserDecision = decision.Value == UserDecision.Merge
                    ? OwnerDeduplicationService.UserDecisionType.Merge
                    : OwnerDeduplicationService.UserDecisionType.KeepSeparate;
            }
        }

        private void LoadDuplicateGroups()
        {
            dgvDuplicateGroups.SelectionChanged -= dgvDuplicateGroups_SelectionChanged!;
            dgvDuplicateGroups.SuspendLayout();

            try
            {
                dgvDuplicateGroups.Rows.Clear();
                _undoStack.Clear();
                _groupOwnerRowsCache.Clear();

                for (int i = 0; i < _duplicateGroups.Count; i++)
                {
                    var group = _duplicateGroups[i];
                    var rowIndex = dgvDuplicateGroups.Rows.Add();
                    var row = dgvDuplicateGroups.Rows[rowIndex];

                    row.Cells["colGroupNumber"].Value = i + 1;
                    row.Cells["colBestOwnerName"].Value = GetBestOwnerName(group);
                    row.Cells["colOwnerCount"].Value = group.Owners.Count;
                    row.Cells["colCitizenshipMatch"].Value = group.CitizenshipConfidence > 0
                        ? $"{group.CitizenshipConfidence:P0}"
                        : "-";
                    row.Cells["colNameFatherMatch"].Value = group.NameFatherConfidence > 0
                        ? $"{group.NameFatherConfidence:P0}"
                        : "-";
                    row.Tag = i;

                    if (!_userDecisions.ContainsKey(i))
                    {
                        var shouldAutoMergeByScore =
                            !group.IsAutoMerged &&
                            group.Owners.Count == 2 &&
                            group.NameFatherConfidence >= 0.9995;

                        if (shouldAutoMergeByScore)
                        {
                            group.IsAutoMerged = true;
                            group.AutoMergedOwner ??= OwnerDeduplicationService.MergeOwnersList(group.Owners);
                            if (string.IsNullOrWhiteSpace(group.MatchType) ||
                                group.MatchType.StartsWith("Medium Confidence", StringComparison.OrdinalIgnoreCase))
                            {
                                group.MatchType = "High Confidence (100% Name + Father/Spouse Exact)";
                            }
                        }

                        if (group.IsAutoMerged)
                        {
                            _userDecisions[i] = UserDecision.Merge;
                        }
                    }

                    if (_userDecisions.TryGetValue(i, out var savedDecision))
                    {
                        UpdateRowDecisionDisplay(row, savedDecision);
                    }
                    else
                    {
                        SetPendingDecisionDisplay(row);
                    }
                }

                ApplyMergedRowVisibility();
                UpdateStatsText();
                UpdateToggleButtonText();
                SelectFirstVisibleRow();
            }
            finally
            {
                dgvDuplicateGroups.ResumeLayout();
                dgvDuplicateGroups.SelectionChanged += dgvDuplicateGroups_SelectionChanged!;
            }
        }

        private static string GetBestOwnerName(DuplicateGroup group)
        {
            if (group.Owners.Count == 0)
            {
                return string.Empty;
            }

            var bestOwner = group.Owners[0];
            var bestScore = OwnerDeduplicationService.GetCompletenessScore(bestOwner);

            for (int i = 1; i < group.Owners.Count; i++)
            {
                var score = OwnerDeduplicationService.GetCompletenessScore(group.Owners[i]);
                if (score > bestScore)
                {
                    bestOwner = group.Owners[i];
                    bestScore = score;
                }
            }

            return bestOwner.LandOwnersName;
        }

        private void UpdateStatsText()
        {
            var totalGroups = _duplicateGroups.Count(g => g.Owners.Count > 1);
            var autoMergedCount = _duplicateGroups.Count(g => g.IsAutoMerged);
            var reviewCount = totalGroups - autoMergedCount;
            lblStats.Text =
                $"Duplicate Groups: {totalGroups} | Auto-Merged: {autoMergedCount} | Review Required: {reviewCount}";
        }

        private void dgvDuplicateGroups_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDuplicateGroups.SelectedRows.Count == 0)
            {
                return;
            }

            var firstRow = dgvDuplicateGroups.SelectedRows.Cast<DataGridViewRow>().FirstOrDefault();
            if (firstRow?.Tag is not int groupIndex || groupIndex < 0 || groupIndex >= _duplicateGroups.Count)
            {
                return;
            }

            _currentGroupIndex = groupIndex;
            var ownerRows = GetOrCreateOwnerRows(groupIndex);

            dgvGroupOwners.SuspendLayout();
            try
            {
                dgvGroupOwners.Rows.Clear();
                for (int i = 0; i < ownerRows.Count; i++)
                {
                    var ownerRow = ownerRows[i];
                    var rowIndex = dgvGroupOwners.Rows.Add();
                    var row = dgvGroupOwners.Rows[rowIndex];

                    row.Cells["colOwnerSn"].Value = ownerRow.OwnerSn;
                    row.Cells["colOwnerName"].Value = ownerRow.Name;
                    row.Cells["colOwnerFather"].Value = ownerRow.FatherSpouse;
                    row.Cells["colOwnerCitizenship"].Value = ownerRow.Citizenship;
                    row.Cells["colOwnerParcels"].Value = ownerRow.Parcels;
                    row.Cells["colOwnerMapSheets"].Value = ownerRow.MapSheets;

                    if (ownerRow.IsBestOwner && ownerRows.Count > 1)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230);
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                    }
                }

                if (dgvGroupOwners.Rows.Count > 0)
                {
                    dgvGroupOwners.Rows[0].Selected = true;
                }
            }
            finally
            {
                dgvGroupOwners.ResumeLayout();
            }

            lblGroupOwners.Text = $"Owners in Group ({_duplicateGroups[groupIndex].MatchType}):";
        }

        private List<OwnerRowData> GetOrCreateOwnerRows(int groupIndex)
        {
            if (_groupOwnerRowsCache.TryGetValue(groupIndex, out var cached))
            {
                return cached;
            }

            var group = _duplicateGroups[groupIndex];
            var rows = new List<OwnerRowData>(group.Owners.Count);

            var bestOwnerIndex = 0;
            var highestScore = int.MinValue;
            for (int i = 0; i < group.Owners.Count; i++)
            {
                var score = OwnerDeduplicationService.GetCompletenessScore(group.Owners[i]);
                if (score > highestScore)
                {
                    highestScore = score;
                    bestOwnerIndex = i;
                }
            }

            for (int i = 0; i < group.Owners.Count; i++)
            {
                var owner = group.Owners[i];
                var mapSheets = owner.ParcelIndices
                    .Where(idx => idx >= 0 && idx < _allRecords.Count)
                    .Select(idx => _mapSheetByParcelIndex.TryGetValue(idx, out var mapSheet) ? mapSheet : string.Empty)
                    .Where(ms => !string.IsNullOrWhiteSpace(ms))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(ms => ms)
                    .ToList();

                rows.Add(new OwnerRowData
                {
                    OwnerSn = (i + 1).ToString(),
                    Name = owner.LandOwnersName,
                    FatherSpouse = owner.FatherSpouse ?? string.Empty,
                    Citizenship = owner.CitizenshipNumber ?? string.Empty,
                    Parcels = string.Join(", ", owner.ParcelIndices),
                    MapSheets = string.Join(", ", mapSheets),
                    IsBestOwner = i == bestOwnerIndex
                });
            }

            _groupOwnerRowsCache[groupIndex] = rows;
            return rows;
        }

        private void btnPreviewUniqueOwners_Click(object? sender, EventArgs e)
        {
            using var preview = new frmUniqueOwnersPreview(_deduplicationResult.UniqueOwners);
            preview.ShowDialog();
        }

        private void btnToggleShowMerged_Click(object? sender, EventArgs e)
        {
            _showMergedRows = !_showMergedRows;
            ApplyMergedRowVisibility();
            UpdateToggleButtonText();
            SelectFirstVisibleRow();
        }

        private void btnUndoDecision_Click(object? sender, EventArgs e)
        {
            if (_undoStack.Count == 0)
            {
                return;
            }

            var snapshot = _undoStack.Pop();
            _userDecisions.Clear();
            foreach (var item in snapshot)
            {
                _userDecisions[item.Key] = item.Value;
            }

            RefreshAllRowDecisionDisplays();
            ApplyMergedRowVisibility();
            SelectFirstVisibleRow();
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            ApplyDecisionToSelectedRows(UserDecision.Merge);
        }

        private void btnKeepSeparate_Click(object sender, EventArgs e)
        {
            ApplyDecisionToSelectedRows(UserDecision.KeepSeparate);
        }

        private void ApplyDecisionToSelectedRows(UserDecision decision)
        {
            var rows = dgvDuplicateGroups.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (rows.Count == 0)
            {
                return;
            }

            var anyChange = rows.Any(row =>
                row.Tag is int idx && (!_userDecisions.TryGetValue(idx, out var existingDecision) || existingDecision != decision));

            if (!anyChange)
            {
                return;
            }

            SaveUndoSnapshot();

            foreach (var row in rows)
            {
                if (row.Tag is not int idx)
                {
                    continue;
                }

                _userDecisions[idx] = decision;
                UpdateRowDecisionDisplay(row, decision);
            }

            ApplyMergedRowVisibility();
            SelectFirstVisibleRow();
        }

        private void SaveUndoSnapshot()
        {
            _undoStack.Push(new Dictionary<int, UserDecision>(_userDecisions));
        }

        private void RefreshAllRowDecisionDisplays()
        {
            foreach (DataGridViewRow row in dgvDuplicateGroups.Rows)
            {
                if (row.Tag is not int idx)
                {
                    continue;
                }

                if (_userDecisions.TryGetValue(idx, out var decision))
                {
                    UpdateRowDecisionDisplay(row, decision);
                }
                else
                {
                    SetPendingDecisionDisplay(row);
                }
            }
        }

        private void UpdateRowDecisionDisplay(DataGridViewRow row, UserDecision decision)
        {
            var groupIndex = row.Tag as int?;
            var isAutoMergeDecision = groupIndex.HasValue &&
                                      _duplicateGroups[groupIndex.Value].IsAutoMerged &&
                                      decision == UserDecision.Merge;

            row.Cells["colDecision"].Value = isAutoMergeDecision
                ? "Auto-Merged"
                : (decision == UserDecision.Merge ? "Merge" : "Keep Separate");

            row.DefaultCellStyle.BackColor = decision == UserDecision.Merge
                ? Color.FromArgb(230, 255, 230)
                : Color.White;

            row.Cells["colDecision"].Style.ForeColor = decision == UserDecision.Merge
                ? Color.ForestGreen
                : Color.Maroon;
        }

        private static void SetPendingDecisionDisplay(DataGridViewRow row)
        {
            row.Cells["colDecision"].Value = "Pending Review";
            row.DefaultCellStyle.BackColor = Color.White;
            row.Cells["colDecision"].Style.ForeColor = Color.Maroon;
        }

        private bool IsAutoMerged(int groupIndex)
        {
            var group = _duplicateGroups[groupIndex];
            if (!group.IsAutoMerged)
            {
                return false;
            }

            if (_userDecisions.TryGetValue(groupIndex, out var decision) && decision == UserDecision.KeepSeparate)
            {
                return false;
            }

            return true;
        }

        private void ApplyMergedRowVisibility()
        {
            foreach (DataGridViewRow row in dgvDuplicateGroups.Rows)
            {
                if (row.Tag is int idx && IsAutoMerged(idx))
                {
                    row.Visible = _showMergedRows;
                }
                else
                {
                    row.Visible = true;
                }
            }
        }

        private void UpdateToggleButtonText()
        {
            btnToggleShowMerged.Text = _showMergedRows ? "Hide Auto-Merged" : "Show Auto-Merged";
        }

        private void SelectFirstVisibleRow()
        {
            dgvDuplicateGroups.ClearSelection();
            foreach (DataGridViewRow row in dgvDuplicateGroups.Rows)
            {
                if (!row.Visible)
                {
                    continue;
                }

                row.Selected = true;
                dgvDuplicateGroups.CurrentCell = row.Cells["colGroupNumber"];
                _currentGroupIndex = row.Tag is int idx ? idx : -1;
                return;
            }

            _currentGroupIndex = -1;
            dgvGroupOwners.Rows.Clear();
            lblGroupOwners.Text = "Owners in Group:";
        }

        private void btnAcceptAll_Click(object sender, EventArgs e)
        {
            var unresolvedCount = _duplicateGroups.Count - _userDecisions.Count;
            if (unresolvedCount > 0)
            {
                var result = MessageBox.Show(
                    $"You have {unresolvedCount} unresolved duplicate(s).\n\n" +
                    "Do you want to accept the current decisions and skip unresolved ones?\n" +
                    "(Unresolved duplicates will be kept separate)",
                    "Unresolved Duplicates",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                for (int i = 0; i < _duplicateGroups.Count; i++)
                {
                    if (!_userDecisions.ContainsKey(i))
                    {
                        _userDecisions[i] = UserDecision.KeepSeparate;
                    }
                }
            }

            RefreshAllRowDecisionDisplays();
            SaveUserDecisionsToResult();
            ApplyUserDecisions();

            ChangesWereMade = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ApplyUserDecisions()
        {
            RebuildUniqueOwnerSignatures();

            foreach (var decision in _userDecisions)
            {
                var group = _duplicateGroups[decision.Key];
                if (decision.Value == UserDecision.Merge)
                {
                    ApplyMergeDecision(group);
                }
                else
                {
                    ApplyKeepSeparateDecision(group);
                }
            }
        }

        private void ApplyMergeDecision(DuplicateGroup group)
        {
            var mergedOwner = OwnerDeduplicationService.MergeOwnersList(group.Owners);

            if (group.AutoMergedOwner != null)
            {
                var oldSignature = BuildParcelSignature(group.AutoMergedOwner.ParcelIndices);
                OverwriteOwner(group.AutoMergedOwner, mergedOwner);
                var newSignature = BuildParcelSignature(group.AutoMergedOwner.ParcelIndices);

                if (!string.Equals(oldSignature, newSignature, StringComparison.Ordinal))
                {
                    _uniqueOwnerSignatures.Remove(oldSignature);
                    _uniqueOwnerSignatures.Add(newSignature);
                }
            }
            else
            {
                foreach (var originalOwner in group.Owners)
                {
                    RemoveOwnerFromResult(originalOwner);
                }

                AddOwnerToResultIfMissing(mergedOwner);
                group.AutoMergedOwner = mergedOwner;
            }

            ApplyOwnerToRecords(mergedOwner, mergedOwner.ParcelIndices);
        }

        private void ApplyKeepSeparateDecision(DuplicateGroup group)
        {
            if (group.AutoMergedOwner != null)
            {
                RemoveOwnerFromResult(group.AutoMergedOwner);
                group.AutoMergedOwner = null;
            }

            foreach (var owner in group.Owners)
            {
                AddOwnerToResultIfMissing(owner);
                ApplyOwnerToRecords(owner, owner.ParcelIndices);
            }
        }

        private void ApplyOwnerToRecords(OwnerDeduplicationService.UniqueOwner owner, IEnumerable<int> parcelIndices)
        {
            foreach (var idx in parcelIndices)
            {
                if (idx < 0 || idx >= _allRecords.Count)
                {
                    continue;
                }

                var record = _allRecords[idx];
                record.LandOwnersName = owner.LandOwnersName;
                record.FatherSpouse = owner.FatherSpouse;
                record.Gender = owner.Gender;
                record.CitizenshipNumber = owner.CitizenshipNumber;
                record.CitizenshipIssuedDistrict = owner.CitizenshipIssuedDistrict;
                record.citizenshipIssuedDate = owner.CitizenshipIssuedDate;
                record.PermanentAddress = owner.PermanentAddress;
                record.TempoaryAddress = owner.TemporaryAddress;
                record.ContactNumber = owner.ContactNumber;
                record.EmailID = owner.EmailID;
            }
        }

        private void RebuildUniqueOwnerSignatures()
        {
            _uniqueOwnerSignatures.Clear();
            foreach (var owner in _deduplicationResult.UniqueOwners)
            {
                _uniqueOwnerSignatures.Add(BuildParcelSignature(owner.ParcelIndices));
            }
        }

        private void AddOwnerToResultIfMissing(OwnerDeduplicationService.UniqueOwner owner)
        {
            var signature = BuildParcelSignature(owner.ParcelIndices);
            if (_uniqueOwnerSignatures.Contains(signature))
            {
                return;
            }

            _deduplicationResult.UniqueOwners.Add(CloneOwner(owner));
            _uniqueOwnerSignatures.Add(signature);
        }

        private void RemoveOwnerFromResult(OwnerDeduplicationService.UniqueOwner owner)
        {
            var signature = BuildParcelSignature(owner.ParcelIndices);
            var existing = _deduplicationResult.UniqueOwners
                .FirstOrDefault(o => string.Equals(BuildParcelSignature(o.ParcelIndices), signature, StringComparison.Ordinal));

            if (existing != null)
            {
                _deduplicationResult.UniqueOwners.Remove(existing);
            }

            _uniqueOwnerSignatures.Remove(signature);
        }

        private static string BuildParcelSignature(IEnumerable<int> parcelIndices)
        {
            return string.Join(",", parcelIndices.OrderBy(i => i));
        }

        private static OwnerDeduplicationService.UniqueOwner CloneOwner(OwnerDeduplicationService.UniqueOwner owner)
        {
            return new OwnerDeduplicationService.UniqueOwner
            {
                LandOwnersName = owner.LandOwnersName,
                FatherSpouse = owner.FatherSpouse,
                Gender = owner.Gender,
                CitizenshipNumber = owner.CitizenshipNumber,
                CitizenshipIssuedDistrict = owner.CitizenshipIssuedDistrict,
                CitizenshipIssuedDate = owner.CitizenshipIssuedDate,
                PermanentAddress = owner.PermanentAddress,
                TemporaryAddress = owner.TemporaryAddress,
                ContactNumber = owner.ContactNumber,
                EmailID = owner.EmailID,
                ParcelIndices = owner.ParcelIndices.ToList(),
                IsAnonymous = owner.IsAnonymous
            };
        }

        private static void OverwriteOwner(OwnerDeduplicationService.UniqueOwner target, OwnerDeduplicationService.UniqueOwner source)
        {
            target.LandOwnersName = source.LandOwnersName;
            target.FatherSpouse = source.FatherSpouse;
            target.Gender = source.Gender;
            target.CitizenshipNumber = source.CitizenshipNumber;
            target.CitizenshipIssuedDistrict = source.CitizenshipIssuedDistrict;
            target.CitizenshipIssuedDate = source.CitizenshipIssuedDate;
            target.PermanentAddress = source.PermanentAddress;
            target.TemporaryAddress = source.TemporaryAddress;
            target.ContactNumber = source.ContactNumber;
            target.EmailID = source.EmailID;
            target.ParcelIndices = source.ParcelIndices.ToList();
            target.IsAnonymous = source.IsAnonymous;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Cancel duplicate review? Any changes will be lost.",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            ChangesWereMade = false;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }
    }
}
