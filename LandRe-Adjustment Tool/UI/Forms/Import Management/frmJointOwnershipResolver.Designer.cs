namespace Land_Readjustment_Tool.Forms
{
    partial class frmJointOwnershipResolver
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblParcelInfo;
        private Label lblInstruction;
        private DataGridView dgvOwners;
        private Panel pnlButtons;
        private Button btnOK;
        private Button btnCancel;

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
            lblParcelInfo = new Label();
            lblInstruction = new Label();
            dgvOwners = new DataGridView();
            pnlButtons = new Panel();
            btnOK = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvOwners).BeginInit();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblParcelInfo
            // 
            lblParcelInfo.AutoSize = true;
            lblParcelInfo.Location = new Point(12, 10);
            lblParcelInfo.Name = "lblParcelInfo";
            lblParcelInfo.Size = new Size(39, 15);
            lblParcelInfo.TabIndex = 0;
            lblParcelInfo.Text = "Parcel";
            // 
            // lblInstruction
            // 
            lblInstruction.Location = new Point(12, 38);
            lblInstruction.Name = "lblInstruction";
            lblInstruction.Size = new Size(730, 52);
            lblInstruction.TabIndex = 1;
            lblInstruction.Text = "Multiple rows share this parcel number. Select the primary owner. All other owners will be added as co-owners. You may also set the ownership share percentage for each co-owner.";
            // 
            // dgvOwners
            // 
            dgvOwners.AllowUserToAddRows = false;
            dgvOwners.AllowUserToDeleteRows = false;
            dgvOwners.AllowUserToResizeRows = false;
            dgvOwners.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvOwners.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "colRole", HeaderText = "Role", Width = 90, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "colOwnerName", HeaderText = "Owner Name", Width = 200, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "colFather", HeaderText = "Father/Spouse", Width = 150, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "colCitizenship", HeaderText = "Citizenship No.", Width = 130, ReadOnly = true },
                new DataGridViewTextBoxColumn { Name = "colShare", HeaderText = "Share %", Width = 80 });
            dgvOwners.Location = new Point(12, 98);
            dgvOwners.MultiSelect = false;
            dgvOwners.Name = "dgvOwners";
            dgvOwners.RowHeadersWidth = 30;
            dgvOwners.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOwners.Size = new Size(730, 280);
            dgvOwners.TabIndex = 2;
            dgvOwners.CellValidating += DgvOwners_CellValidating;
            dgvOwners.SelectionChanged += DgvOwners_SelectionChanged;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnOK);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 410);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(760, 50);
            pnlButtons.TabIndex = 3;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(430, 10);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(190, 32);
            btnOK.TabIndex = 0;
            btnOK.Text = "Confirm Joint Ownership";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(630, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 32);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmJointOwnershipResolver
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 460);
            Controls.Add(dgvOwners);
            Controls.Add(pnlButtons);
            Controls.Add(lblInstruction);
            Controls.Add(lblParcelInfo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimumSize = new Size(680, 420);
            Name = "frmJointOwnershipResolver";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Resolve Joint Ownership";
            ((System.ComponentModel.ISupportInitialize)dgvOwners).EndInit();
            pnlButtons.ResumeLayout(false);
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
