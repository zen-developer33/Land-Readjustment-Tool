namespace Land_Readjustment_Tool.Forms
{
    partial class frmBackupManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lstBackups = new ListView();
            columnBackup = new ColumnHeader();
            columnDateModified = new ColumnHeader();
            columnSize = new ColumnHeader();
            btnRestore = new Button();
            btnCancel = new Button();
            lblInfo = new Label();
            SuspendLayout();
            // 
            // lstBackups
            // 
            lstBackups.Columns.AddRange(new ColumnHeader[] { columnBackup, columnDateModified, columnSize });
            lstBackups.FullRowSelect = true;
            lstBackups.GridLines = true;
            lstBackups.Location = new Point(14, 53);
            lstBackups.Margin = new Padding(3, 4, 3, 4);
            lstBackups.Name = "lstBackups";
            lstBackups.Size = new Size(525, 332);
            lstBackups.TabIndex = 0;
            lstBackups.UseCompatibleStateImageBehavior = false;
            lstBackups.View = View.Details;
            lstBackups.SelectedIndexChanged += LstBackups_SelectedIndexChanged;
            lstBackups.DoubleClick += LstBackups_DoubleClick;
            // 
            // columnBackup
            // 
            columnBackup.Text = "Backup";
            columnBackup.Width = 100;
            // 
            // columnDateModified
            // 
            columnDateModified.Text = "Date Modified";
            columnDateModified.Width = 200;
            // 
            // columnSize
            // 
            columnSize.Text = "Size";
            columnSize.Width = 100;
            // 
            // btnRestore
            // 
            btnRestore.Enabled = false;
            btnRestore.Location = new Point(362, 393);
            btnRestore.Margin = new Padding(3, 4, 3, 4);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(86, 40);
            btnRestore.TabIndex = 2;
            btnRestore.Text = "Restore";
            btnRestore.UseVisualStyleBackColor = true;
            btnRestore.Click += BtnRestore_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(455, 393);
            btnCancel.Margin = new Padding(3, 4, 3, 4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(86, 40);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblInfo
            // 
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(14, 20);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(184, 20);
            lblInfo.TabIndex = 1;
            lblInfo.Text = "Select a backup to restore:";
            // 
            // frmBackupManager
            // 
            AcceptButton = btnRestore;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(553, 441);
            Controls.Add(btnCancel);
            Controls.Add(btnRestore);
            Controls.Add(lblInfo);
            Controls.Add(lstBackups);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmBackupManager";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Restore from Backup";
            Load += frmBackupManager_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstBackups;
        private System.Windows.Forms.ColumnHeader columnBackup;
        private System.Windows.Forms.ColumnHeader columnDateModified;
        private System.Windows.Forms.ColumnHeader columnSize;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblInfo;
    }
}