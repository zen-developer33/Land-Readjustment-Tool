namespace Land_Readjustment_Tool.UI.Forms.Project
{
    partial class frmBackupManager
    {
        private System.ComponentModel.IContainer
            components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lstBackups = new ListView();
            colBackup = new ColumnHeader();
            colDateModified = new ColumnHeader();
            colSize = new ColumnHeader();
            lblInfo = new Label();
            btnRestore = new Button();
            btnCancel = new Button();

            SuspendLayout();

            // ── FORM ─────────────────────────────────
            Text = "Restore from Backup";
            ClientSize = new Size(560, 420);
            MinimumSize = new Size(560, 420);
            MaximumSize = new Size(560, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            AutoScaleMode = AutoScaleMode.Font;
            AcceptButton = btnRestore;
            CancelButton = btnCancel;
            Load += frmBackupManager_Load;

            // ── INFO LABEL ────────────────────────────
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(12, 12);
            lblInfo.Name = "lblInfo";
            lblInfo.Font = new Font("Segoe UI", 9F);
            lblInfo.Text = "Select a backup to restore:";
            lblInfo.TabIndex = 0;

            // ── LIST VIEW ────────────────────────────
            lstBackups.Location = new Point(12, 40);
            lstBackups.Size = new Size(530, 300);
            lstBackups.Name = "lstBackups";
            lstBackups.TabIndex = 1;
            lstBackups.View = View.Details;
            lstBackups.FullRowSelect = true;
            lstBackups.GridLines = true;
            lstBackups.MultiSelect = false;
            lstBackups.UseCompatibleStateImageBehavior
                = false;
            lstBackups.Columns.AddRange(
                new ColumnHeader[]
                {
                    colBackup,
                    colDateModified,
                    colSize
                });
            lstBackups.SelectedIndexChanged +=
                LstBackups_SelectedIndexChanged;
            lstBackups.DoubleClick +=
                LstBackups_DoubleClick;

            colBackup.Text = "Backup";
            colBackup.Width = 130;

            colDateModified.Text = "Date Saved";
            colDateModified.Width = 200;

            colSize.Text = "Size";
            colSize.Width = 100;

            // ── BUTTONS ──────────────────────────────
            btnRestore.Text = "Restore";
            btnRestore.Size = new Size(90, 30);
            btnRestore.Location = new Point(366, 356);
            btnRestore.Enabled = false;
            btnRestore.TabIndex = 2;
            btnRestore.Click += BtnRestore_Click;

            btnCancel.Text = "Cancel";
            btnCancel.Size = new Size(90, 30);
            btnCancel.Location = new Point(462, 356);
            btnCancel.TabIndex = 3;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Click += BtnCancel_Click;

            // ── ASSEMBLE ─────────────────────────────
            Controls.Add(lblInfo);
            Controls.Add(lstBackups);
            Controls.Add(btnRestore);
            Controls.Add(btnCancel);

            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListView lstBackups;
        private ColumnHeader colBackup;
        private ColumnHeader colDateModified;
        private ColumnHeader colSize;
        private Label lblInfo;
        private Button btnRestore;
        private Button btnCancel;
    }
}