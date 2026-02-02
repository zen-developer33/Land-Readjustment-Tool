namespace Land_Readjustment_Tool.Forms
{
    partial class frmValidationErrors
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
            dgvErrors = new DataGridView();
            btnFixSelected = new Button();
            btnClose = new Button();
            lblErrorCount = new Label();
            btnExportErrors = new Button();
            lblTitle = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvErrors).BeginInit();
            SuspendLayout();
            // 
            // dgvErrors
            // 
            dgvErrors.AllowUserToAddRows = false;
            dgvErrors.AllowUserToDeleteRows = false;
            dgvErrors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvErrors.BackgroundColor = SystemColors.ControlLight;
            dgvErrors.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvErrors.Location = new Point(12, 70);
            dgvErrors.Name = "dgvErrors";
            dgvErrors.ReadOnly = true;
            dgvErrors.RowHeadersWidth = 51;
            dgvErrors.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvErrors.Size = new Size(876, 400);
            dgvErrors.TabIndex = 0;
            dgvErrors.CellDoubleClick += dgvErrors_CellDoubleClick;
            // 
            // btnFixSelected
            // 
            btnFixSelected.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnFixSelected.Location = new Point(12, 480);
            btnFixSelected.Name = "btnFixSelected";
            btnFixSelected.Size = new Size(148, 35);
            btnFixSelected.TabIndex = 1;
            btnFixSelected.Text = "Fix Selected Error";
            btnFixSelected.UseVisualStyleBackColor = true;
            btnFixSelected.Click += btnFixSelected_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(732, 480);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(156, 35);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblErrorCount
            // 
            lblErrorCount.AutoSize = true;
            lblErrorCount.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblErrorCount.ForeColor = Color.Red;
            lblErrorCount.Location = new Point(12, 40);
            lblErrorCount.Name = "lblErrorCount";
            lblErrorCount.Size = new Size(137, 23);
            lblErrorCount.TabIndex = 3;
            lblErrorCount.Text = "0 error(s) found";
            // 
            // btnExportErrors
            // 
            btnExportErrors.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExportErrors.Location = new Point(166, 480);
            btnExportErrors.Name = "btnExportErrors";
            btnExportErrors.Size = new Size(131, 35);
            btnExportErrors.TabIndex = 4;
            btnExportErrors.Text = "Export to File";
            btnExportErrors.UseVisualStyleBackColor = true;
            btnExportErrors.Click += btnExportErrors_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Location = new Point(12, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(169, 28);
            lblTitle.TabIndex = 5;
            lblTitle.Text = "Validation Errors";
            // 
            // frmValidationErrors
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 527);
            Controls.Add(lblTitle);
            Controls.Add(btnExportErrors);
            Controls.Add(lblErrorCount);
            Controls.Add(btnClose);
            Controls.Add(btnFixSelected);
            Controls.Add(dgvErrors);
            MinimumSize = new Size(800, 400);
            Name = "frmValidationErrors";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Validation Errors";
            Load += frmValidationErrors_Load;
            ((System.ComponentModel.ISupportInitialize)dgvErrors).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvErrors;
        private Button btnFixSelected;
        private Button btnClose;
        private Label lblErrorCount;
        private Button btnExportErrors;
        private Label lblTitle;
    }
}
