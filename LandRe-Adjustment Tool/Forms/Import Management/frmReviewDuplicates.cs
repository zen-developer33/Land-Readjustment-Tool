using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using System.ComponentModel;
using System.Linq;
using DuplicateGroup = Land_Readjustment_Tool.Services.OwnerDeduplicationService.DuplicateGroup;


namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Form for reviewing and manually resolving potential duplicate owners
    /// Shows side-by-side comparison of suspected duplicate pairs
    /// Allows user to merge or keep separate
    /// </summary>
    public partial class frmReviewDuplicates : Form
    {
        private List<OwnerDeduplicationService.DuplicateGroup> _duplicateGroups;
        private BindingList<BaselineLandParceRecord> _allRecords;
		private OwnerDeduplicationService.DeduplicationResult _deduplicationResult;
		private Stack<Dictionary<int, UserDecision>> _undoStack = new();
		private bool _showMergedRows = false;

        // Track user decisions
		private Dictionary<int, UserDecision> _userDecisions = new();
		private int _currentGroupIndex = -1;

        public bool ChangesWereMade { get; private set; } = false;

        public frmReviewDuplicates(
            OwnerDeduplicationService.DeduplicationResult deduplicationResult,
            BindingList<BaselineLandParceRecord> allRecords)
        {
            InitializeComponent();
            _deduplicationResult = deduplicationResult;
            _duplicateGroups = deduplicationResult.DuplicatesNeedingReview;
            _allRecords = allRecords;
            
            // Create context menu for batch decisions
            var menu = new ContextMenuStrip();
            var miMerge = new ToolStripMenuItem("Set Merge");
            var miKeep = new ToolStripMenuItem("Set Keep Separate");
            miMerge.Click += (s, e) => ApplyDecisionToSelectedRows(UserDecision.Merge);
            miKeep.Click += (s, e) => ApplyDecisionToSelectedRows(UserDecision.KeepSeparate);
            menu.Items.AddRange(new ToolStripItem[] { miMerge, miKeep });
            dgvDuplicateGroups.ContextMenuStrip = menu;
        }

		private void SaveUndoSnapshot()
		{
			_undoStack.Push(new Dictionary<int, UserDecision>(_userDecisions));
		}
		
		/// <summary>
		/// Restores user decisions from the deduplication result
		/// </summary>
		private void RestoreUserDecisions()
		{
			_userDecisions.Clear();
			for (int i = 0; i < _duplicateGroups.Count; i++)
			{
				var group = _duplicateGroups[i];
				if (group.UserDecision.HasValue)
				{
					// Convert from service enum to local enum
					var decision = group.UserDecision.Value == OwnerDeduplicationService.UserDecisionType.Merge 
					    ? UserDecision.Merge 
					    : UserDecision.KeepSeparate;
					_userDecisions[i] = decision;
				}
			}
		}
		
		/// <summary>
		/// Saves user decisions back to the deduplication result for persistence
		/// </summary>
		private void SaveUserDecisionsToResult()
		{
			foreach (var decision in _userDecisions)
			{
				var group = _duplicateGroups[decision.Key];
				// Convert from local enum to service enum
				group.UserDecision = decision.Value == UserDecision.Merge 
				    ? OwnerDeduplicationService.UserDecisionType.Merge 
				    : OwnerDeduplicationService.UserDecisionType.KeepSeparate;
			}
		}

		private DataGridViewRow? GetRowByGroupIndex(int groupIndex)
		{
			return dgvDuplicateGroups.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Tag is int idx && idx == groupIndex);
		}

		private void UpdateRowDecisionDisplay(DataGridViewRow row, UserDecision decision)
		{
			row.Cells["colDecision"].Value = decision == UserDecision.Merge ? "? Merge These" : "? Keep Separate";
			row.DefaultCellStyle.BackColor = decision == UserDecision.Merge ? Color.FromArgb(230, 255, 230) : Color.White;
			row.Cells["colDecision"].Style.ForeColor = decision == UserDecision.Merge ? Color.ForestGreen : Color.Maroon;
		}

		private void SetPendingDecisionDisplay(DataGridViewRow row)
		{
			row.Cells["colDecision"].Value = "? Keep Separate";
			row.DefaultCellStyle.BackColor = Color.White;
			row.Cells["colDecision"].Style.ForeColor = Color.Maroon;
		}

		private void RefreshAllRowDecisionDisplays()
		{
			foreach (DataGridViewRow row in dgvDuplicateGroups.Rows)
			{
				if (row.Tag is int idx)
				{
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
				if (row.Visible)
				{
					row.Selected = true;
					dgvDuplicateGroups.CurrentCell = row.Cells["colGroupNumber"];
					_currentGroupIndex = row.Tag is int idx ? idx : -1;
					return;
				}
			}
			_currentGroupIndex = -1;
		}

        private void frmReviewDuplicates_Load(object sender, EventArgs e)
        {
            InitializeGrid();
            RestoreUserDecisions();
            LoadDuplicateGroups();

			if (_duplicateGroups.Count == 0)
            {
                MessageBox.Show("No potential duplicates to review.", "No Duplicates",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
        }

        private void InitializeGrid()
        {
			// Columns are defined in Designer; enforce read-only selection view
			dgvDuplicateGroups.AutoGenerateColumns = false;
			dgvDuplicateGroups.AllowUserToAddRows = false;
			dgvDuplicateGroups.ReadOnly = true;
        }

		private void LoadDuplicateGroups()
		{
			dgvDuplicateGroups.SelectionChanged -= dgvDuplicateGroups_SelectionChanged!;
			dgvDuplicateGroups.Rows.Clear();
			_undoStack.Clear();
			_userDecisions.Clear();

			for (int i = 0; i < _duplicateGroups.Count; i++)
			{
				var group = _duplicateGroups[i];
				int rowIndex = dgvDuplicateGroups.Rows.Add();
				var row = dgvDuplicateGroups.Rows[rowIndex];
                row.Cells["colGroupNumber"].Value = i + 1;
				
				// Find the best owner (highest completeness score)
				var bestOwner = group.Owners
				.OrderByDescending(o => OwnerDeduplicationService.GetCompletenessScore(o))
				.First();
				
				row.Cells["colBestOwnerName"].Value = bestOwner.LandOwnersName;
				row.Cells["colOwnerCount"].Value = group.Owners.Count.ToString();
				                row.Cells["colCitizenshipMatch"].Value = group.CitizenshipConfidence > 0 ? $"{group.CitizenshipConfidence:P0}" : "-";
row.Cells["colNameFatherMatch"].Value = group.NameFatherConfidence > 0 ? $"{group.NameFatherConfidence:P0}" : "-";
				row.Tag = i;

                if (IsAutoMerged(i))
				{
				_userDecisions[i] = UserDecision.Merge;
				UpdateRowDecisionDisplay(row, UserDecision.Merge);
				row.Cells["colDecision"].Value = "? Auto-Merged";
				}
				else
				{
				SetPendingDecisionDisplay(row);
				}
				}

				ApplyMergedRowVisibility();
				dgvDuplicateGroups.SelectionChanged += dgvDuplicateGroups_SelectionChanged!;
			
				// Update stats with more detail
				int reviewCount = _duplicateGroups.Count(g => g.Owners.Count > 1 && !g.IsAutoMerged);
				int autoMergedCount = _duplicateGroups.Count(g => g.IsAutoMerged);
                lblStats.Text = $"Duplicate Groups: {_duplicateGroups.Count(g => g.Owners.Count > 1)} | Auto-Merged: {autoMergedCount} | Review Required: {reviewCount}";
			UpdateToggleButtonText();
			SelectFirstVisibleRow();
		}


        private void dgvDuplicateGroups_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDuplicateGroups.SelectedRows.Count == 0) return;

            // Support multi-select: use the first selected row for detailed view
            var firstRow = dgvDuplicateGroups.SelectedRows.Cast<DataGridViewRow>().First();
            int groupIndex = firstRow.Tag is int tag ? tag : firstRow.Index;

            if (groupIndex < 0 || groupIndex >= _duplicateGroups.Count) return;

            _currentGroupIndex = groupIndex;

            // Populate owners grid for this group
            dgvGroupOwners.Rows.Clear();
            var group = _duplicateGroups[groupIndex];
            
            // Find the owner with the highest completeness score (best data)
            int bestOwnerIndex = 0;
            int highestScore = 0;
            for (int i = 0; i < group.Owners.Count; i++)
            {
                int score = OwnerDeduplicationService.GetCompletenessScore(group.Owners[i]);
                if (score > highestScore)
                {
                    highestScore = score;
                    bestOwnerIndex = i;
                }
            }
            
            for (int i = 0; i < group.Owners.Count; i++)
            {
                var o = group.Owners[i];
                int r = dgvGroupOwners.Rows.Add();
                var row = dgvGroupOwners.Rows[r];
				row.Cells["colOwnerSn"].Value = (i + 1).ToString();
				row.Cells["colOwnerName"].Value = o.LandOwnersName;
				row.Cells["colOwnerFather"].Value = o.FatherSpouse;
				row.Cells["colOwnerCitizenship"].Value = o.CitizenshipNumber;
				row.Cells["colOwnerParcels"].Value = string.Join(", ", o.ParcelIndices);
				
				// Get unique map sheets for this owner's parcels
				var mapSheets = o.ParcelIndices
				    .Where(idx => idx >= 0 && idx < _allRecords.Count)
				    .Select(idx => _allRecords[idx].MapSheetNo)
				    .Distinct()
				    .Where(ms => !string.IsNullOrWhiteSpace(ms))
				    .OrderBy(ms => ms);
				row.Cells["colOwnerMapSheets"].Value = string.Join(", ", mapSheets);
				if (i == bestOwnerIndex && group.Owners.Count > 1)
				{
				    row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
				    row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); // Light green
				}
				else
				{
				    row.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
				    row.DefaultCellStyle.BackColor = Color.White;
				}
            }
            if (dgvGroupOwners.Rows.Count > 0) dgvGroupOwners.Rows[0].Selected = true;
            
            // Update the match type label
            lblGroupOwners.Text = $"Owners in Group ({group.MatchType}):";
        }

        private void btnPreviewUniqueOwners_Click(object? sender, EventArgs e)
        {
            var preview = new frmUniqueOwnersPreview(_deduplicationResult.UniqueOwners);
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
			if (_undoStack.Count == 0) return;
			_userDecisions = _undoStack.Pop();
			RefreshAllRowDecisionDisplays();
		}

        private void btnMerge_Click(object sender, EventArgs e)
        {
            ApplyDecisionToSelectedRows(UserDecision.Merge);
        }

        private void btnKeepSeparate_Click(object sender, EventArgs e)
        {
            ApplyDecisionToSelectedRows(UserDecision.KeepSeparate);
        }

		private void MarkGroupAsResolved(int groupIndex)
		{
			var row = GetRowByGroupIndex(groupIndex);
			if (row != null && _userDecisions.TryGetValue(groupIndex, out var decision))
			{
				UpdateRowDecisionDisplay(row, decision);
			}
		}

        private void MoveToNextGroup()
        {
            // Find next unresolved group
            for (int i = 0; i < _duplicateGroups.Count; i++)
            {
                if (!_userDecisions.ContainsKey(i))
                {
					if (!dgvDuplicateGroups.Rows[i].Visible) continue;
					dgvDuplicateGroups.Rows[i].Selected = true;
					dgvDuplicateGroups.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }

            // All resolved
            MessageBox.Show("All duplicates have been reviewed!", "Review Complete",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnAcceptAll.Focus();
        }

        private void btnAcceptAll_Click(object sender, EventArgs e)
        {
            // Check if all groups are resolved
            int unresolvedCount = _duplicateGroups.Count - _userDecisions.Count;

            if (unresolvedCount > 0)
            {
                var result = MessageBox.Show(
                    $"You have {unresolvedCount} unresolved duplicate(s).\n\n" +
                    $"Do you want to accept the current decisions and skip the unresolved ones?\n" +
                    $"(Unresolved duplicates will be kept as separate owners)",
                    "Unresolved Duplicates",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes) return;

                // Mark unresolved as KeepSeparate
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
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ApplyUserDecisions()
        {
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
				OverwriteOwner(group.AutoMergedOwner, mergedOwner);
			}
			else
			{
				_deduplicationResult.UniqueOwners.Add(mergedOwner);
			}

			var allIndices = group.Owners.SelectMany(o => o.ParcelIndices).Distinct().ToList();
			ApplyOwnerToRecords(mergedOwner, allIndices);
		}

		private void ApplyKeepSeparateDecision(DuplicateGroup group)
		{
			if (group.AutoMergedOwner != null)
			{
				_deduplicationResult.UniqueOwners.Remove(group.AutoMergedOwner);
				group.AutoMergedOwner = null;
			}

			foreach (var owner in group.Owners)
			{
				EnsureOwnerInResult(owner);
			}
		}

		private void ApplyOwnerToRecords(OwnerDeduplicationService.UniqueOwner owner, List<int> parcelIndices)
		{
			foreach (int idx in parcelIndices)
			{
				if (idx >= 0 && idx < _allRecords.Count)
				{
					var record = _allRecords[idx];
				record.LandOwnersName = owner.LandOwnersName;
				record.FatherSpouse = owner.FatherSpouse;
				record.Gender = owner.Gender;
				record.CitizenshipNumber = owner.CitizenshipNumber;
				// Note: ParcelLocation stays with the parcel record, not updated from owner
				record.PermanentAddress = owner.PermanentAddress;
				}
			}
		}

		private void EnsureOwnerInResult(OwnerDeduplicationService.UniqueOwner owner)
		{
			if (_deduplicationResult.UniqueOwners.Any(existing => OwnersShareSameParcels(existing, owner)))
			{
				return;
			}

			_deduplicationResult.UniqueOwners.Add(CloneOwner(owner));
		}

		private static bool OwnersShareSameParcels(OwnerDeduplicationService.UniqueOwner a, OwnerDeduplicationService.UniqueOwner b)
		{
			if (a.ParcelIndices.Count != b.ParcelIndices.Count) return false;
			var left = a.ParcelIndices.OrderBy(i => i);
			var right = b.ParcelIndices.OrderBy(i => i);
			return left.SequenceEqual(right);
		}

		private static OwnerDeduplicationService.UniqueOwner CloneOwner(OwnerDeduplicationService.UniqueOwner owner)
		{
			return new OwnerDeduplicationService.UniqueOwner
			{
			LandOwnersName = owner.LandOwnersName,
			FatherSpouse = owner.FatherSpouse,
			Gender = owner.Gender,
			CitizenshipNumber = owner.CitizenshipNumber,
			
			PermanentAddress = owner.PermanentAddress,
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
		
		target.PermanentAddress = source.PermanentAddress;
			target.ParcelIndices = source.ParcelIndices.ToList();
			target.IsAnonymous = source.IsAnonymous;
		}

        private void ApplyDecisionToSelectedRows(UserDecision decision)
        {
			var rows = dgvDuplicateGroups.SelectedRows.Cast<DataGridViewRow>().ToList();
			if (rows.Count == 0) return;

			bool anyChange = rows.Any(row => row.Tag is int idx && (!
				_userDecisions.TryGetValue(idx, out var existing) || existing != decision));
			if (!anyChange) return;

			SaveUndoSnapshot();

			foreach (var row in rows)
			{
				if (row.Tag is int idx)
				{
					_userDecisions[idx] = decision;
					UpdateRowDecisionDisplay(row, decision);
				}
			}
			ApplyMergedRowVisibility();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Cancel duplicate review? Any changes will be lost.",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ChangesWereMade = false;
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private enum UserDecision
        {
            Merge,
            KeepSeparate
        }
    }
}

