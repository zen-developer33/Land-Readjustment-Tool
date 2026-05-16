namespace Land_Readjustment_Tool.Forms
{
    partial class frmCoOwnersList
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblTitle;
        private DataGridView dgv;
        private Panel pnlButtons;
        private Button btnAdd;
        private Button btnRemove;
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
            lblTitle = new Label();
            dgv = new DataGridView();
            pnlButtons = new Panel();
            btnAdd = new Button();
            btnRemove = new Button();
            btnClose = new Button();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(12, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(184, 15);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Joint co-owners for this parcel:";
            // 
            // dgv
            // 
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Columns.AddRange(
                new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Owner Name", Width = 180 },
                new DataGridViewTextBoxColumn { Name = "colFather", HeaderText = "Father/Spouse", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colCitizenship", HeaderText = "Citizenship No.", Width = 130 },
                new DataGridViewTextBoxColumn { Name = "colAddress", HeaderText = "Address", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "colShare", HeaderText = "Share %", Width = 70 });
            dgv.Location = new Point(12, 38);
            dgv.MultiSelect = false;
            dgv.Name = "dgv";
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.Size = new Size(660, 240);
            dgv.TabIndex = 1;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnAdd);
            pnlButtons.Controls.Add(btnRemove);
            pnlButtons.Controls.Add(btnClose);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 332);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(700, 48);
            pnlButtons.TabIndex = 2;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(12, 9);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(140, 30);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Add Co-Owner";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += BtnAdd_Click;
            // 
            // btnRemove
            // 
            btnRemove.Location = new Point(158, 9);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(140, 30);
            btnRemove.TabIndex = 1;
            btnRemove.Text = "Remove Selected";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += BtnRemove_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.DialogResult = DialogResult.OK;
            btnClose.Location = new Point(588, 9);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 30);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmCoOwnersList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(700, 380);
            Controls.Add(dgv);
            Controls.Add(pnlButtons);
            Controls.Add(lblTitle);
            MinimumSize = new Size(600, 300);
            Name = "frmCoOwnersList";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Co-Owners";
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            pnlButtons.ResumeLayout(false);
            Land_Readjustment_Tool.UI.Forms.RecordFormTheme.Apply(this);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
