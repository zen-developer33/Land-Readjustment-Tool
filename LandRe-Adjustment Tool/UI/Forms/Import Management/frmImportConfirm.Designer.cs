namespace Land_Readjustment_Tool.Forms
{
    partial class frmImportConfirm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpSummary = new GroupBox();
            txtSummary = new TextBox();
            grpConflict = new GroupBox();
            pnlConflictInner = new Panel();
            lblConflictStats = new Label();
            rbReplace = new RadioButton();
            lblReplaceHint = new Label();
            rbAdd = new RadioButton();
            lblAddHint = new Label();
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnConfirm = new Button();
            grpSummary.SuspendLayout();
            grpConflict.SuspendLayout();
            pnlConflictInner.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // grpSummary
            // 
            grpSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpSummary.Controls.Add(txtSummary);
            grpSummary.Location = new Point(16, 16);
            grpSummary.Margin = new Padding(3, 4, 3, 4);
            grpSummary.Name = "grpSummary";
            grpSummary.Padding = new Padding(0);
            grpSummary.Size = new Size(553, 230);
            grpSummary.TabIndex = 0;
            grpSummary.TabStop = false;
            grpSummary.Text = "Import Summary";
            // 
            // txtSummary
            // 
            txtSummary.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSummary.Location = new Point(14, 30);
            txtSummary.Margin = new Padding(3, 4, 3, 4);
            txtSummary.Multiline = true;
            txtSummary.Name = "txtSummary";
            txtSummary.ReadOnly = true;
            txtSummary.ScrollBars = ScrollBars.Vertical;
            txtSummary.Size = new Size(525, 188);
            txtSummary.TabIndex = 0;
            txtSummary.TabStop = false;
            // 
            // grpConflict
            // 
            grpConflict.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpConflict.Controls.Add(pnlConflictInner);
            grpConflict.Location = new Point(16, 254);
            grpConflict.Margin = new Padding(3, 4, 3, 4);
            grpConflict.Name = "grpConflict";
            grpConflict.Padding = new Padding(0);
            grpConflict.Size = new Size(553, 247);
            grpConflict.TabIndex = 1;
            grpConflict.TabStop = false;
            grpConflict.Text = "⚠   Existing Data Detected";
            grpConflict.Visible = false;
            // 
            // pnlConflictInner
            // 
            pnlConflictInner.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlConflictInner.Controls.Add(lblConflictStats);
            pnlConflictInner.Controls.Add(rbReplace);
            pnlConflictInner.Controls.Add(lblReplaceHint);
            pnlConflictInner.Controls.Add(rbAdd);
            pnlConflictInner.Controls.Add(lblAddHint);
            pnlConflictInner.Location = new Point(14, 28);
            pnlConflictInner.Name = "pnlConflictInner";
            pnlConflictInner.Size = new Size(525, 211);
            pnlConflictInner.TabIndex = 0;
            // 
            // lblConflictStats
            // 
            lblConflictStats.Location = new Point(12, 10);
            lblConflictStats.Name = "lblConflictStats";
            lblConflictStats.Size = new Size(489, 58);
            lblConflictStats.TabIndex = 0;
            lblConflictStats.Text = "The database already contains data.";
            // 
            // rbReplace
            // 
            rbReplace.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            rbReplace.Location = new Point(0, 85);
            rbReplace.Name = "rbReplace";
            rbReplace.Size = new Size(460, 24);
            rbReplace.TabIndex = 1;
            rbReplace.Text = "Replace all existing data with this new import";
            rbReplace.UseVisualStyleBackColor = true;
            // 
            // lblReplaceHint
            // 
            lblReplaceHint.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblReplaceHint.Location = new Point(22, 111);
            lblReplaceHint.Name = "lblReplaceHint";
            lblReplaceHint.Size = new Size(438, 18);
            lblReplaceHint.TabIndex = 2;
            lblReplaceHint.Text = "Permanently deletes all existing owners and parcels — this cannot be undone.";
            // 
            // rbAdd
            // 
            rbAdd.Checked = true;
            rbAdd.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            rbAdd.Location = new Point(0, 139);
            rbAdd.Name = "rbAdd";
            rbAdd.Size = new Size(460, 24);
            rbAdd.TabIndex = 3;
            rbAdd.TabStop = true;
            rbAdd.Text = "Add new records to existing data";
            rbAdd.UseVisualStyleBackColor = true;
            // 
            // lblAddHint
            // 
            lblAddHint.Font = new Font("Segoe UI", 8.25F, FontStyle.Italic, GraphicsUnit.Point, 0);
            lblAddHint.Location = new Point(22, 165);
            lblAddHint.Name = "lblAddHint";
            lblAddHint.Size = new Size(463, 36);
            lblAddHint.TabIndex = 4;
            lblAddHint.Text = "Keeps all existing records. Incoming parcels already in the database will be skipped.";
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnConfirm);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 505);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Padding = new Padding(16, 14, 16, 14);
            pnlButtons.Size = new Size(585, 64);
            pnlButtons.TabIndex = 2;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(483, 17);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(79, 36);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnConfirm
            // 
            btnConfirm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnConfirm.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnConfirm.Location = new Point(322, 17);
            btnConfirm.Margin = new Padding(3, 4, 3, 4);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(145, 36);
            btnConfirm.TabIndex = 1;
            btnConfirm.Text = "Confirm Import";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += BtnConfirm_Click;
            // 
            // frmImportConfirm
            // 
            AcceptButton = btnConfirm;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(585, 569);
            Controls.Add(grpSummary);
            Controls.Add(grpConflict);
            Controls.Add(pnlButtons);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmImportConfirm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Confirm Import";
            grpSummary.ResumeLayout(false);
            grpSummary.PerformLayout();
            grpConflict.ResumeLayout(false);
            pnlConflictInner.ResumeLayout(false);
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private GroupBox grpSummary;
        private TextBox txtSummary;
        private GroupBox grpConflict;
        private Panel pnlConflictInner;
        private Label lblConflictStats;
        private RadioButton rbReplace;
        private Label lblReplaceHint;
        private RadioButton rbAdd;
        private Label lblAddHint;
        private Panel pnlButtons;
        private Button btnCancel;
        private Button btnConfirm;
    }
}
