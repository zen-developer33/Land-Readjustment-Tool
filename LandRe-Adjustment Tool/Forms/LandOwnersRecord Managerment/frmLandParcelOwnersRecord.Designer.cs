namespace Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment
{
    partial class frmLandParcelOwnersRecord
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
            grpFilterByMapSheet = new GroupBox();
            cboMapSheet = new ComboBox();
            lblMapSheet = new Label();
            chkAllMapSheets = new CheckBox();
            grpFilterByMapSheet.SuspendLayout();
            SuspendLayout();
            // 
            // grpFilterByMapSheet
            // 
            grpFilterByMapSheet.Controls.Add(cboMapSheet);
            grpFilterByMapSheet.Controls.Add(lblMapSheet);
            grpFilterByMapSheet.Controls.Add(chkAllMapSheets);
            grpFilterByMapSheet.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpFilterByMapSheet.ForeColor = Color.FromArgb(45, 65, 95);
            grpFilterByMapSheet.Location = new Point(12, 13);
            grpFilterByMapSheet.Margin = new Padding(3, 4, 3, 4);
            grpFilterByMapSheet.Name = "grpFilterByMapSheet";
            grpFilterByMapSheet.Padding = new Padding(3, 4, 3, 4);
            grpFilterByMapSheet.Size = new Size(500, 147);
            grpFilterByMapSheet.TabIndex = 1;
            grpFilterByMapSheet.TabStop = false;
            grpFilterByMapSheet.Text = "Filter by Map Sheet";
            // 
            // cboMapSheet
            // 
            cboMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMapSheet.Font = new Font("Segoe UI", 9F);
            cboMapSheet.Location = new Point(11, 64);
            cboMapSheet.Margin = new Padding(3, 4, 3, 4);
            cboMapSheet.Name = "cboMapSheet";
            cboMapSheet.Size = new Size(163, 28);
            cboMapSheet.TabIndex = 1;
            // 
            // lblMapSheet
            // 
            lblMapSheet.AutoSize = true;
            lblMapSheet.Font = new Font("Segoe UI", 9F);
            lblMapSheet.ForeColor = Color.Black;
            lblMapSheet.Location = new Point(11, 38);
            lblMapSheet.Name = "lblMapSheet";
            lblMapSheet.Size = new Size(83, 20);
            lblMapSheet.TabIndex = 2;
            lblMapSheet.Text = "Map Sheet:";
            // 
            // chkAllMapSheets
            // 
            chkAllMapSheets.AutoSize = true;
            chkAllMapSheets.Checked = true;
            chkAllMapSheets.CheckState = CheckState.Checked;
            chkAllMapSheets.Font = new Font("Segoe UI", 9F);
            chkAllMapSheets.ForeColor = Color.Black;
            chkAllMapSheets.Location = new Point(11, 108);
            chkAllMapSheets.Margin = new Padding(3, 4, 3, 4);
            chkAllMapSheets.Name = "chkAllMapSheets";
            chkAllMapSheets.Size = new Size(136, 24);
            chkAllMapSheets.TabIndex = 3;
            chkAllMapSheets.Text = "Show All Sheets";
            // 
            // frmLandParcelOwnersRecord
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1476, 874);
            Controls.Add(grpFilterByMapSheet);
            Name = "frmLandParcelOwnersRecord";
            Text = "frmLandParcelOwnersRecord";
            grpFilterByMapSheet.ResumeLayout(false);
            grpFilterByMapSheet.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpFilterByMapSheet;
        private ComboBox cboMapSheet;
        private Label lblMapSheet;
        private CheckBox chkAllMapSheets;
    }
}