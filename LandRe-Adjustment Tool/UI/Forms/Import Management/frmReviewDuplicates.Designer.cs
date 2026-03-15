namespace Land_Readjustment_Tool.Forms
{
    partial class frmReviewDuplicates
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnKeepSeparate = new Button();
            btnPreviewUniqueOwners = new Button();
            btnMerge = new Button();
            btnUndoDecision = new Button();
            btnToggleShowMerged = new Button();
            btnAcceptAll = new Button();
            splitContainer1 = new SplitContainer();
            lblStats = new Label();
            lblInstructions = new Label();
            dgvDuplicateGroups = new DataGridView();
            colGroupNumber = new DataGridViewTextBoxColumn();
            colBestOwnerName = new DataGridViewTextBoxColumn();
            colOwnerCount = new DataGridViewTextBoxColumn();
            colCitizenshipMatch = new DataGridViewTextBoxColumn();
            colNameFatherMatch = new DataGridViewTextBoxColumn();
            colDecision = new DataGridViewTextBoxColumn();
            grpComparison = new GroupBox();
            dgvGroupOwners = new DataGridView();
            colOwnerSn = new DataGridViewTextBoxColumn();
            colOwnerName = new DataGridViewTextBoxColumn();
            colOwnerFather = new DataGridViewTextBoxColumn();
            colOwnerCitizenship = new DataGridViewTextBoxColumn();
            colOwnerParcels = new DataGridViewTextBoxColumn();
            colOwnerMapSheets = new DataGridViewTextBoxColumn();
            lblGroupOwners = new Label();
            pnlButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvDuplicateGroups).BeginInit();
            grpComparison.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGroupOwners).BeginInit();
            SuspendLayout();
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnKeepSeparate);
            pnlButtons.Controls.Add(btnPreviewUniqueOwners);
            pnlButtons.Controls.Add(btnMerge);
            pnlButtons.Controls.Add(btnUndoDecision);
            pnlButtons.Controls.Add(btnToggleShowMerged);
            pnlButtons.Controls.Add(btnAcceptAll);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 770);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(1507, 60);
            pnlButtons.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(1387, 13);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(108, 38);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnKeepSeparate
            // 
            btnKeepSeparate.Location = new Point(173, 13);
            btnKeepSeparate.Name = "btnKeepSeparate";
            btnKeepSeparate.Size = new Size(150, 38);
            btnKeepSeparate.TabIndex = 1;
            btnKeepSeparate.Text = "✗ Keep Separate";
            btnKeepSeparate.UseVisualStyleBackColor = true;
            btnKeepSeparate.Click += btnKeepSeparate_Click;
            // 
            // btnPreviewUniqueOwners
            // 
            btnPreviewUniqueOwners.Location = new Point(626, 13);
            btnPreviewUniqueOwners.Name = "btnPreviewUniqueOwners";
            btnPreviewUniqueOwners.Size = new Size(195, 38);
            btnPreviewUniqueOwners.TabIndex = 5;
            btnPreviewUniqueOwners.Text = "Preview Unique Owners";
            btnPreviewUniqueOwners.Click += btnPreviewUniqueOwners_Click;
            // 
            // btnMerge
            // 
            btnMerge.BackColor = Color.FromArgb(230, 255, 230);
            btnMerge.Location = new Point(17, 13);
            btnMerge.Name = "btnMerge";
            btnMerge.Size = new Size(150, 38);
            btnMerge.TabIndex = 0;
            btnMerge.Text = "✓ Merge These";
            btnMerge.UseVisualStyleBackColor = false;
            btnMerge.Click += btnMerge_Click;
            // 
            // btnUndoDecision
            // 
            btnUndoDecision.Location = new Point(329, 13);
            btnUndoDecision.Name = "btnUndoDecision";
            btnUndoDecision.Size = new Size(122, 38);
            btnUndoDecision.TabIndex = 6;
            btnUndoDecision.Text = "Undo Decision";
            btnUndoDecision.UseVisualStyleBackColor = true;
            btnUndoDecision.Click += btnUndoDecision_Click;
            // 
            // btnToggleShowMerged
            // 
            btnToggleShowMerged.Location = new Point(457, 13);
            btnToggleShowMerged.Name = "btnToggleShowMerged";
            btnToggleShowMerged.Size = new Size(163, 38);
            btnToggleShowMerged.TabIndex = 6;
            btnToggleShowMerged.Text = "Show Auto-Merged";
            btnToggleShowMerged.Click += btnToggleShowMerged_Click;
            // 
            // btnAcceptAll
            // 
            btnAcceptAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAcceptAll.BackColor = SystemColors.Control;
            btnAcceptAll.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnAcceptAll.ForeColor = Color.Black;
            btnAcceptAll.Location = new Point(1186, 13);
            btnAcceptAll.Name = "btnAcceptAll";
            btnAcceptAll.Size = new Size(195, 38);
            btnAcceptAll.TabIndex = 2;
            btnAcceptAll.Text = "✓ Accept All && Continue";
            btnAcceptAll.UseVisualStyleBackColor = false;
            btnAcceptAll.Click += btnAcceptAll_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lblStats);
            splitContainer1.Panel1.Controls.Add(lblInstructions);
            splitContainer1.Panel1.Controls.Add(dgvDuplicateGroups);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(grpComparison);
            splitContainer1.Size = new Size(1507, 830);
            splitContainer1.SplitterDistance = 841;
            splitContainer1.TabIndex = 5;
            // 
            // lblStats
            // 
            lblStats.AutoSize = true;
            lblStats.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStats.ForeColor = Color.FromArgb(30, 50, 80);
            lblStats.Location = new Point(17, 3);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(397, 23);
            lblStats.TabIndex = 7;
            lblStats.Text = "Potential Duplicates Found: 0 (Review Required)";
            // 
            // lblInstructions
            // 
            lblInstructions.AutoSize = true;
            lblInstructions.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblInstructions.ForeColor = Color.FromArgb(40, 40, 40);
            lblInstructions.Location = new Point(17, 29);
            lblInstructions.Name = "lblInstructions";
            lblInstructions.Size = new Size(775, 20);
            lblInstructions.TabIndex = 6;
            lblInstructions.Text = "Review each potential duplicates. Click \"Merge\" if they are the same person, or \"Keep Separate\" if they are different.";
            // 
            // dgvDuplicateGroups
            // 
            dgvDuplicateGroups.AllowUserToAddRows = false;
            dgvDuplicateGroups.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvDuplicateGroups.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvDuplicateGroups.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDuplicateGroups.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvDuplicateGroups.BackgroundColor = SystemColors.ControlLight;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Control;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvDuplicateGroups.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvDuplicateGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDuplicateGroups.Columns.AddRange(new DataGridViewColumn[] { colGroupNumber, colBestOwnerName, colOwnerCount, colCitizenshipMatch, colNameFatherMatch, colDecision });
            dgvDuplicateGroups.Location = new Point(3, 52);
            dgvDuplicateGroups.Name = "dgvDuplicateGroups";
            dgvDuplicateGroups.ReadOnly = true;
            dgvDuplicateGroups.RowHeadersWidth = 51;
            dgvDuplicateGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDuplicateGroups.Size = new Size(835, 706);
            dgvDuplicateGroups.TabIndex = 5;
            // 
            // colGroupNumber
            // 
            colGroupNumber.FillWeight = 32.52595F;
            colGroupNumber.HeaderText = "#";
            colGroupNumber.MinimumWidth = 6;
            colGroupNumber.Name = "colGroupNumber";
            colGroupNumber.ReadOnly = true;
            colGroupNumber.Width = 47;
            // 
            // colBestOwnerName
            // 
            colBestOwnerName.FillWeight = 122.327896F;
            colBestOwnerName.HeaderText = "Best Owner Name";
            colBestOwnerName.MinimumWidth = 6;
            colBestOwnerName.Name = "colBestOwnerName";
            colBestOwnerName.ReadOnly = true;
            colBestOwnerName.Width = 151;
            // 
            // colOwnerCount
            // 
            colOwnerCount.FillWeight = 97.56497F;
            colOwnerCount.HeaderText = "Duplicates";
            colOwnerCount.MinimumWidth = 6;
            colOwnerCount.Name = "colOwnerCount";
            colOwnerCount.ReadOnly = true;
            colOwnerCount.Width = 111;
            // 
            // colCitizenshipMatch
            // 
            colCitizenshipMatch.FillWeight = 108.440292F;
            colCitizenshipMatch.HeaderText = "Citizenship";
            colCitizenshipMatch.MinimumWidth = 6;
            colCitizenshipMatch.Name = "colCitizenshipMatch";
            colCitizenshipMatch.ReadOnly = true;
            colCitizenshipMatch.Width = 114;
            // 
            // colNameFatherMatch
            // 
            colNameFatherMatch.FillWeight = 135.132019F;
            colNameFatherMatch.HeaderText = "Name+Father";
            colNameFatherMatch.MinimumWidth = 6;
            colNameFatherMatch.Name = "colNameFatherMatch";
            colNameFatherMatch.ReadOnly = true;
            colNameFatherMatch.Width = 136;
            // 
            // colDecision
            // 
            colDecision.FillWeight = 104.008888F;
            colDecision.HeaderText = "Decision";
            colDecision.MinimumWidth = 6;
            colDecision.Name = "colDecision";
            colDecision.ReadOnly = true;
            colDecision.Width = 97;
            // 
            // grpComparison
            // 
            grpComparison.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpComparison.Controls.Add(dgvGroupOwners);
            grpComparison.Controls.Add(lblGroupOwners);
            grpComparison.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpComparison.ForeColor = Color.FromArgb(35, 35, 35);
            grpComparison.Location = new Point(3, 3);
            grpComparison.Name = "grpComparison";
            grpComparison.Size = new Size(656, 761);
            grpComparison.TabIndex = 1;
            grpComparison.TabStop = false;
            grpComparison.Text = "Compare Potential Duplicates";
            // 
            // dgvGroupOwners
            // 
            dgvGroupOwners.AllowUserToAddRows = false;
            dgvGroupOwners.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dgvGroupOwners.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            dgvGroupOwners.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvGroupOwners.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dgvGroupOwners.BackgroundColor = SystemColors.ControlLight;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = SystemColors.Control;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            dataGridViewCellStyle4.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvGroupOwners.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvGroupOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGroupOwners.Columns.AddRange(new DataGridViewColumn[] { colOwnerSn, colOwnerName, colOwnerFather, colOwnerCitizenship, colOwnerParcels, colOwnerMapSheets });
            dgvGroupOwners.Location = new Point(0, 49);
            dgvGroupOwners.MultiSelect = false;
            dgvGroupOwners.Name = "dgvGroupOwners";
            dgvGroupOwners.ReadOnly = true;
            dgvGroupOwners.RowHeadersWidth = 51;
            dgvGroupOwners.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvGroupOwners.Size = new Size(650, 706);
            dgvGroupOwners.TabIndex = 4;
            dgvGroupOwners.TabStop = false;
            // 
            // colOwnerSn
            // 
            colOwnerSn.HeaderText = "#";
            colOwnerSn.MinimumWidth = 6;
            colOwnerSn.Name = "colOwnerSn";
            colOwnerSn.ReadOnly = true;
            colOwnerSn.Width = 47;
            // 
            // colOwnerName
            // 
            colOwnerName.HeaderText = "Name";
            colOwnerName.MinimumWidth = 6;
            colOwnerName.Name = "colOwnerName";
            colOwnerName.ReadOnly = true;
            colOwnerName.Width = 80;
            // 
            // colOwnerFather
            // 
            colOwnerFather.HeaderText = "Father/Spouse";
            colOwnerFather.MinimumWidth = 6;
            colOwnerFather.Name = "colOwnerFather";
            colOwnerFather.ReadOnly = true;
            colOwnerFather.Width = 140;
            // 
            // colOwnerCitizenship
            // 
            colOwnerCitizenship.HeaderText = "Citizenship";
            colOwnerCitizenship.MinimumWidth = 6;
            colOwnerCitizenship.Name = "colOwnerCitizenship";
            colOwnerCitizenship.ReadOnly = true;
            colOwnerCitizenship.Width = 114;
            // 
            // colOwnerParcels
            // 
            colOwnerParcels.HeaderText = "Parcels";
            colOwnerParcels.MinimumWidth = 6;
            colOwnerParcels.Name = "colOwnerParcels";
            colOwnerParcels.ReadOnly = true;
            colOwnerParcels.Width = 87;
            // 
            // colOwnerMapSheets
            // 
            colOwnerMapSheets.HeaderText = "Map Sheets";
            colOwnerMapSheets.MinimumWidth = 6;
            colOwnerMapSheets.Name = "colOwnerMapSheets";
            colOwnerMapSheets.ReadOnly = true;
            colOwnerMapSheets.Width = 119;
            // 
            // lblGroupOwners
            // 
            lblGroupOwners.AutoSize = true;
            lblGroupOwners.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblGroupOwners.Location = new Point(6, 26);
            lblGroupOwners.Name = "lblGroupOwners";
            lblGroupOwners.Size = new Size(187, 20);
            lblGroupOwners.TabIndex = 3;
            lblGroupOwners.Text = "Duplicate Owners in Group";
            // 
            // frmReviewDuplicates
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1507, 830);
            Controls.Add(pnlButtons);
            Controls.Add(splitContainer1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MinimizeBox = false;
            Name = "frmReviewDuplicates";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Tag = "";
            Text = " ";
            Load += frmReviewDuplicates_Load;
            pnlButtons.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvDuplicateGroups).EndInit();
            grpComparison.ResumeLayout(false);
            grpComparison.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGroupOwners).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Panel pnlButtons;
        private Button btnMerge;
        private Button btnKeepSeparate;
        private Button btnAcceptAll;
        private Button btnCancel;
        private Button btnPreviewUniqueOwners;
        private Button btnToggleShowMerged;
		private Button btnUndoDecision;
        private SplitContainer splitContainer1;
        private Label lblStats;
        private Label lblInstructions;
        private DataGridView dgvDuplicateGroups;
        private DataGridViewTextBoxColumn colGroupNumber;
        private DataGridViewTextBoxColumn colBestOwnerName;
        private DataGridViewTextBoxColumn colOwnerCount;
        private DataGridViewTextBoxColumn colCitizenshipMatch;
        private DataGridViewTextBoxColumn colNameFatherMatch;
        private DataGridViewTextBoxColumn colDecision;
        private GroupBox grpComparison;
        private DataGridView dgvGroupOwners;
        private DataGridViewTextBoxColumn colOwnerSn;
        private DataGridViewTextBoxColumn colOwnerName;
        private DataGridViewTextBoxColumn colOwnerFather;
        private DataGridViewTextBoxColumn colOwnerCitizenship;
        private DataGridViewTextBoxColumn colOwnerParcels;
        private DataGridViewTextBoxColumn colOwnerMapSheets;
        private Label lblGroupOwners;
    }
}
