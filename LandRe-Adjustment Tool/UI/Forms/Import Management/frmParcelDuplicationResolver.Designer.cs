namespace Land_Readjustment_Tool.Forms
{
    partial class frmParcelDuplicationResolver
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblSummary;
        private SplitContainer splitContainer;
        private DataGridView dgvGroups;
        private DataGridView dgvOwners;
        private Panel pnlButtons;
        private Button btnResolveSelected;
        private Button btnSetSelectedJointOwnership;
        private Button btnResolveAll;
        private Button btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblSummary = new Label();
            splitContainer = new SplitContainer();
            dgvGroups = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            dgvOwners = new DataGridView();
            pnlButtons = new Panel();
            btnResolveSelected = new Button();
            btnResolveAll = new Button();
            btnClose = new Button();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
            btnSetSelectedJointOwnership = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGroups).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvOwners).BeginInit();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblSummary
            // 
            lblSummary.Dock = DockStyle.Top;
            lblSummary.Location = new Point(0, 0);
            lblSummary.Name = "lblSummary";
            lblSummary.Padding = new Padding(14, 13, 14, 11);
            lblSummary.Size = new Size(1257, 59);
            lblSummary.TabIndex = 0;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(0, 59);
            splitContainer.Margin = new Padding(3, 4, 3, 4);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(dgvGroups);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(dgvOwners);
            splitContainer.Size = new Size(1257, 731);
            splitContainer.SplitterDistance = 514;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 1;
            // 
            // dgvGroups
            // 
            dgvGroups.AllowUserToAddRows = false;
            dgvGroups.AllowUserToDeleteRows = false;
            dgvGroups.AllowUserToResizeRows = false;
            dgvGroups.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvGroups.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            dgvGroups.Dock = DockStyle.Fill;
            dgvGroups.Location = new Point(0, 0);
            dgvGroups.Margin = new Padding(3, 4, 3, 4);
            dgvGroups.Name = "dgvGroups";
            dgvGroups.ReadOnly = true;
            dgvGroups.RowHeadersWidth = 34;
            dgvGroups.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGroups.Size = new Size(514, 731);
            dgvGroups.TabIndex = 0;
            dgvGroups.SelectionChanged += DgvGroups_SelectionChanged;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.HeaderText = "Status";
            dataGridViewTextBoxColumn1.Name = "Status";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.Width = 90;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.HeaderText = "Map Sheet";
            dataGridViewTextBoxColumn2.Name = "MapSheet";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            dataGridViewTextBoxColumn2.Width = 110;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.HeaderText = "Parcel";
            dataGridViewTextBoxColumn3.Name = "Parcel";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.Width = 90;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.HeaderText = "Owners";
            dataGridViewTextBoxColumn4.Name = "Owners";
            dataGridViewTextBoxColumn4.ReadOnly = true;
            dataGridViewTextBoxColumn4.Width = 125;
            // 
            // dgvOwners
            // 
            dgvOwners.AllowUserToAddRows = false;
            dgvOwners.AllowUserToDeleteRows = false;
            dgvOwners.AllowUserToResizeRows = false;
            dgvOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOwners.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn7, dataGridViewTextBoxColumn8, dataGridViewTextBoxColumn9 });
            dgvOwners.Dock = DockStyle.Fill;
            dgvOwners.Location = new Point(0, 0);
            dgvOwners.Margin = new Padding(3, 4, 3, 4);
            dgvOwners.MultiSelect = false;
            dgvOwners.Name = "dgvOwners";
            dgvOwners.ReadOnly = true;
            dgvOwners.RowHeadersWidth = 34;
            dgvOwners.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOwners.Size = new Size(738, 731);
            dgvOwners.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnSetSelectedJointOwnership);
            pnlButtons.Controls.Add(btnResolveSelected);
            pnlButtons.Controls.Add(btnResolveAll);
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 790);
            pnlButtons.Margin = new Padding(3, 4, 3, 4);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(14, 13, 14, 13);
            pnlButtons.Size = new Size(1257, 77);
            pnlButtons.TabIndex = 2;
            // 
            // btnResolveSelected
            // 
            btnResolveSelected.Location = new Point(12, 19);
            btnResolveSelected.Margin = new Padding(3, 4, 3, 4);
            btnResolveSelected.Name = "btnResolveSelected";
            btnResolveSelected.Size = new Size(171, 45);
            btnResolveSelected.TabIndex = 0;
            btnResolveSelected.Text = "Resolve Selected";
            btnResolveSelected.UseVisualStyleBackColor = true;
            btnResolveSelected.Click += BtnResolveSelected_Click;
            // 
            // btnResolveAll
            // 
            btnResolveAll.Location = new Point(423, 19);
            btnResolveAll.Margin = new Padding(3, 4, 3, 4);
            btnResolveAll.Name = "btnResolveAll";
            btnResolveAll.Size = new Size(240, 45);
            btnResolveAll.TabIndex = 1;
            btnResolveAll.Text = "Set All as Joint Ownership";
            btnResolveAll.UseVisualStyleBackColor = true;
            btnResolveAll.Click += BtnResolveAll_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(1118, 16);
            btnClose.Margin = new Padding(3, 4, 3, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(126, 45);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += BtnClose_Click;
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.MinimumWidth = 6;
            dataGridViewTextBoxColumn5.HeaderText = "Role";
            dataGridViewTextBoxColumn5.Name = "Role";
            dataGridViewTextBoxColumn5.ReadOnly = true;
            dataGridViewTextBoxColumn5.Width = 110;
            // 
            // dataGridViewTextBoxColumn6
            // 
            dataGridViewTextBoxColumn6.MinimumWidth = 6;
            dataGridViewTextBoxColumn6.HeaderText = "Owner";
            dataGridViewTextBoxColumn6.Name = "Owner";
            dataGridViewTextBoxColumn6.ReadOnly = true;
            dataGridViewTextBoxColumn6.Width = 180;
            // 
            // dataGridViewTextBoxColumn7
            // 
            dataGridViewTextBoxColumn7.MinimumWidth = 6;
            dataGridViewTextBoxColumn7.HeaderText = "Father/Spouse";
            dataGridViewTextBoxColumn7.Name = "Father";
            dataGridViewTextBoxColumn7.ReadOnly = true;
            dataGridViewTextBoxColumn7.Width = 160;
            // 
            // dataGridViewTextBoxColumn8
            // 
            dataGridViewTextBoxColumn8.MinimumWidth = 6;
            dataGridViewTextBoxColumn8.HeaderText = "Citizenship";
            dataGridViewTextBoxColumn8.Name = "Citizenship";
            dataGridViewTextBoxColumn8.ReadOnly = true;
            dataGridViewTextBoxColumn8.Width = 120;
            // 
            // dataGridViewTextBoxColumn9
            // 
            dataGridViewTextBoxColumn9.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn9.MinimumWidth = 6;
            dataGridViewTextBoxColumn9.HeaderText = "Address";
            dataGridViewTextBoxColumn9.Name = "Address";
            dataGridViewTextBoxColumn9.ReadOnly = true;
            dataGridViewTextBoxColumn9.Width = 125;
            // 
            // btnSetSelectedJointOwnership
            // 
            btnSetSelectedJointOwnership.Location = new Point(189, 19);
            btnSetSelectedJointOwnership.Margin = new Padding(3, 4, 3, 4);
            btnSetSelectedJointOwnership.Name = "btnSetSelectedJointOwnership";
            btnSetSelectedJointOwnership.Size = new Size(228, 45);
            btnSetSelectedJointOwnership.TabIndex = 3;
            btnSetSelectedJointOwnership.Text = "Set Selected As Joint Ownership";
            btnSetSelectedJointOwnership.UseVisualStyleBackColor = true;
            btnSetSelectedJointOwnership.Click += BtnSetSelectedJointOwnership_Click;
            // 
            // frmParcelDuplicationResolver
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1257, 867);
            Controls.Add(splitContainer);
            Controls.Add(pnlButtons);
            Controls.Add(lblSummary);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1117, 731);
            Name = "frmParcelDuplicationResolver";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Resolve Parcel Duplication";
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGroups).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvOwners).EndInit();
            pnlButtons.ResumeLayout(false);
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
            ResumeLayout(false);
        }

        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
    }
}
