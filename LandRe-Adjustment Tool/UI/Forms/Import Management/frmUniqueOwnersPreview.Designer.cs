namespace Land_Readjustment_Tool.Forms
{
    partial class frmUniqueOwnersPreview
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnOK;
        private DataGridView dgvUniqueOwners;

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
            dgvUniqueOwners = new DataGridView();
            btnOK = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvUniqueOwners).BeginInit();
            SuspendLayout();
            // 
            // dgvUniqueOwners
            // 
            dgvUniqueOwners.AllowUserToAddRows = false;
            dgvUniqueOwners.AllowUserToDeleteRows = false;
            dgvUniqueOwners.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvUniqueOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUniqueOwners.Location = new Point(12, 12);
            dgvUniqueOwners.Name = "dgvUniqueOwners";
            dgvUniqueOwners.ReadOnly = true;
            dgvUniqueOwners.RowHeadersWidth = 50;
            dgvUniqueOwners.Size = new Size(694, 389);
            dgvUniqueOwners.TabIndex = 0;
            dgvUniqueOwners.RowPostPaint += DgvUniqueOwners_RowPostPaint;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(606, 408);
            btnOK.Margin = new Padding(3, 4, 3, 4);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(100, 35);
            btnOK.TabIndex = 10;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // frmUniqueOwnersPreview
            // 
            ClientSize = new Size(718, 444);
            Controls.Add(btnOK);
            Controls.Add(dgvUniqueOwners);
            Name = "frmUniqueOwnersPreview";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Unique Owners Preview";
            Load += frmUniqueOwnersPreview_Load;
            ((System.ComponentModel.ISupportInitialize)dgvUniqueOwners).EndInit();
            ResumeLayout(false);
        }
    }
}
