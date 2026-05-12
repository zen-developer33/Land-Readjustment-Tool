using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmProjectBoundaryAssignment
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
            lblSourceLayer = new Label();
            cmbLayerFilter = new ComboBox();
            lblObjects = new Label();
            lstObjects = new ListBox();
            btnPrevious = new Button();
            btnNext = new Button();
            chkZoomOnSelect = new CheckBox();
            chkDeleteExistingBoundary = new CheckBox();
            lblStatus = new Label();
            btnRemoveBoundary = new Button();
            btnImportBoundary = new Button();
            btnAssign = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // lblSourceLayer
            // 
            lblSourceLayer.AutoSize = true;
            lblSourceLayer.Location = new Point(16, 22);
            lblSourceLayer.Name = "lblSourceLayer";
            lblSourceLayer.Size = new Size(86, 20);
            lblSourceLayer.TabIndex = 0;
            lblSourceLayer.Text = "Source layer";
            // 
            // cmbLayerFilter
            // 
            cmbLayerFilter.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLayerFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLayerFilter.FormattingEnabled = true;
            cmbLayerFilter.Location = new Point(138, 18);
            cmbLayerFilter.Name = "cmbLayerFilter";
            cmbLayerFilter.Size = new Size(454, 28);
            cmbLayerFilter.TabIndex = 1;
            cmbLayerFilter.SelectedIndexChanged += cmbLayerFilter_SelectedIndexChanged;
            // 
            // lblObjects
            // 
            lblObjects.AutoSize = true;
            lblObjects.Location = new Point(16, 62);
            lblObjects.Name = "lblObjects";
            lblObjects.Size = new Size(110, 20);
            lblObjects.TabIndex = 2;
            lblObjects.Text = "Canvas objects";
            // 
            // lstObjects
            // 
            lstObjects.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstObjects.FormattingEnabled = true;
            lstObjects.ItemHeight = 20;
            lstObjects.Location = new Point(138, 62);
            lstObjects.Name = "lstObjects";
            lstObjects.Size = new Size(454, 184);
            lstObjects.TabIndex = 3;
            lstObjects.SelectedIndexChanged += lstObjects_SelectedIndexChanged;
            // 
            // btnPrevious
            // 
            btnPrevious.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPrevious.Location = new Point(138, 258);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(94, 32);
            btnPrevious.TabIndex = 4;
            btnPrevious.Text = "Previous";
            btnPrevious.UseVisualStyleBackColor = true;
            btnPrevious.Click += btnPrevious_Click;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNext.Location = new Point(238, 258);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(94, 32);
            btnNext.TabIndex = 5;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click += btnNext_Click;
            // 
            // chkZoomOnSelect
            // 
            chkZoomOnSelect.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            chkZoomOnSelect.AutoSize = true;
            chkZoomOnSelect.Checked = true;
            chkZoomOnSelect.CheckState = CheckState.Checked;
            chkZoomOnSelect.Location = new Point(350, 263);
            chkZoomOnSelect.Name = "chkZoomOnSelect";
            chkZoomOnSelect.Size = new Size(143, 24);
            chkZoomOnSelect.TabIndex = 6;
            chkZoomOnSelect.Text = "Zoom to selected";
            chkZoomOnSelect.UseVisualStyleBackColor = true;
            chkZoomOnSelect.CheckedChanged += chkZoomOnSelect_CheckedChanged;
            // 
            // chkDeleteExistingBoundary
            // 
            chkDeleteExistingBoundary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chkDeleteExistingBoundary.AutoSize = true;
            chkDeleteExistingBoundary.Checked = true;
            chkDeleteExistingBoundary.CheckState = CheckState.Checked;
            chkDeleteExistingBoundary.Enabled = false;
            chkDeleteExistingBoundary.Location = new Point(138, 304);
            chkDeleteExistingBoundary.Name = "chkDeleteExistingBoundary";
            chkDeleteExistingBoundary.Size = new Size(320, 24);
            chkDeleteExistingBoundary.TabIndex = 7;
            chkDeleteExistingBoundary.Text = "Replace existing Project Boundary objects";
            chkDeleteExistingBoundary.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.ForeColor = Color.DimGray;
            lblStatus.Location = new Point(138, 338);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(454, 47);
            lblStatus.TabIndex = 8;
            lblStatus.Text = "Select a polygon object to assign as Project Boundary.";
            // 
            // btnRemoveBoundary
            // 
            btnRemoveBoundary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnRemoveBoundary.Location = new Point(16, 399);
            btnRemoveBoundary.Name = "btnRemoveBoundary";
            btnRemoveBoundary.Size = new Size(156, 34);
            btnRemoveBoundary.TabIndex = 9;
            btnRemoveBoundary.Text = "Remove Boundary";
            btnRemoveBoundary.UseVisualStyleBackColor = true;
            btnRemoveBoundary.Click += btnRemoveBoundary_Click;
            // 
            // btnImportBoundary
            // 
            btnImportBoundary.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnImportBoundary.Location = new Point(184, 399);
            btnImportBoundary.Name = "btnImportBoundary";
            btnImportBoundary.Size = new Size(142, 34);
            btnImportBoundary.TabIndex = 10;
            btnImportBoundary.Text = "Import Boundary";
            btnImportBoundary.UseVisualStyleBackColor = true;
            btnImportBoundary.Click += btnImportBoundary_Click;
            // 
            // btnAssign
            // 
            btnAssign.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAssign.Location = new Point(392, 399);
            btnAssign.Name = "btnAssign";
            btnAssign.Size = new Size(94, 34);
            btnAssign.TabIndex = 11;
            btnAssign.Text = "Assign";
            btnAssign.UseVisualStyleBackColor = true;
            btnAssign.Click += btnAssign_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(498, 399);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 34);
            btnCancel.TabIndex = 12;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmProjectBoundaryAssignment
            // 
            AcceptButton = btnAssign;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(612, 453);
            Controls.Add(btnCancel);
            Controls.Add(btnAssign);
            Controls.Add(btnImportBoundary);
            Controls.Add(btnRemoveBoundary);
            Controls.Add(lblStatus);
            Controls.Add(chkDeleteExistingBoundary);
            Controls.Add(chkZoomOnSelect);
            Controls.Add(btnNext);
            Controls.Add(btnPrevious);
            Controls.Add(lstObjects);
            Controls.Add(lblObjects);
            Controls.Add(cmbLayerFilter);
            Controls.Add(lblSourceLayer);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmProjectBoundaryAssignment";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Assign Project Boundary";
            FormClosed += frmProjectBoundaryAssignment_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblSourceLayer;
        private ComboBox cmbLayerFilter;
        private Label lblObjects;
        private ListBox lstObjects;
        private Button btnPrevious;
        private Button btnNext;
        private CheckBox chkZoomOnSelect;
        private CheckBox chkDeleteExistingBoundary;
        private Label lblStatus;
        private Button btnRemoveBoundary;
        private Button btnImportBoundary;
        private Button btnAssign;
        private Button btnCancel;
    }
}
