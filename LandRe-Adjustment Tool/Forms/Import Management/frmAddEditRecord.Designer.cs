using System.Windows.Forms;
using System.Xml.Linq;
using System;
using System.Collections.Generic;


namespace Land_Readjustment_Tool.Forms
{
    partial class frmAddEditRecord
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
            grpBasicInfo = new GroupBox();
            txtMunicipalityVillage = new TextBox();
            lblMunicipalityVillage = new Label();
            txtDistrict = new TextBox();
            lblDistrict = new Label();
            txtProvince = new TextBox();
            lblProvince = new Label();
            txtMapSheetNo = new TextBox();
            lblMapSheetNo = new Label();
            txtParcelNo = new TextBox();
            lblParcelNo = new Label();
            grpOwnerInfo = new GroupBox();
            RbtnNo = new RadioButton();
            RbtnYes = new RadioButton();
            txtAddress = new TextBox();
            lblAddress = new Label();
            lblIsTenant = new Label();
            txtCitizenshipNumber = new TextBox();
            lblCitizenshipNumber = new Label();
            cmbGender = new ComboBox();
            lblGender = new Label();
            txtFatherSpouse = new TextBox();
            lblFatherSpouse = new Label();
            txtLandOwnersName = new TextBox();
            lblLandOwnersName = new Label();
            grpLandInfo = new GroupBox();
            txtPaanaNo = new TextBox();
            lblPaanaNo = new Label();
            txtMothNo = new TextBox();
            lblMothNo = new Label();
            txtAreaInBKD = new TextBox();
            lblAreaInBKD = new Label();
            txtAreaInRAPD = new TextBox();
            lblAreaInRAPD = new Label();
            txtAreaInSqm = new TextBox();
            lblAreaInSqm = new Label();
            cmbLandUse = new ComboBox();
            lblLandUse = new Label();
            grpRemarks = new GroupBox();
            txtRemarks = new TextBox();
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnUpdate = new Button();
            btnAdd = new Button();
            btnDelete = new Button();
            grpBasicInfo.SuspendLayout();
            grpOwnerInfo.SuspendLayout();
            grpLandInfo.SuspendLayout();
            grpRemarks.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // grpBasicInfo
            // 
            grpBasicInfo.Controls.Add(txtMunicipalityVillage);
            grpBasicInfo.Controls.Add(lblMunicipalityVillage);
            grpBasicInfo.Controls.Add(txtDistrict);
            grpBasicInfo.Controls.Add(lblDistrict);
            grpBasicInfo.Controls.Add(txtProvince);
            grpBasicInfo.Controls.Add(lblProvince);
            grpBasicInfo.Controls.Add(txtMapSheetNo);
            grpBasicInfo.Controls.Add(lblMapSheetNo);
            grpBasicInfo.Controls.Add(txtParcelNo);
            grpBasicInfo.Controls.Add(lblParcelNo);
            grpBasicInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpBasicInfo.Location = new Point(12, 12);
            grpBasicInfo.Name = "grpBasicInfo";
            grpBasicInfo.Size = new Size(433, 203);
            grpBasicInfo.TabIndex = 0;
            grpBasicInfo.TabStop = false;
            grpBasicInfo.Text = "Basic Information";
            // 
            // txtMunicipalityVillage
            // 
            txtMunicipalityVillage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMunicipalityVillage.Font = new Font("Segoe UI", 9F);
            txtMunicipalityVillage.Location = new Point(180, 162);
            txtMunicipalityVillage.Name = "txtMunicipalityVillage";
            txtMunicipalityVillage.Size = new Size(233, 27);
            txtMunicipalityVillage.TabIndex = 9;
            // 
            // lblMunicipalityVillage
            // 
            lblMunicipalityVillage.AutoSize = true;
            lblMunicipalityVillage.Font = new Font("Segoe UI", 9F);
            lblMunicipalityVillage.Location = new Point(15, 165);
            lblMunicipalityVillage.Name = "lblMunicipalityVillage";
            lblMunicipalityVillage.Size = new Size(145, 20);
            lblMunicipalityVillage.TabIndex = 8;
            lblMunicipalityVillage.Text = "Municipality/Village:";
            // 
            // txtDistrict
            // 
            txtDistrict.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDistrict.Font = new Font("Segoe UI", 9F);
            txtDistrict.Location = new Point(180, 129);
            txtDistrict.Name = "txtDistrict";
            txtDistrict.Size = new Size(233, 27);
            txtDistrict.TabIndex = 7;
            // 
            // lblDistrict
            // 
            lblDistrict.AutoSize = true;
            lblDistrict.Font = new Font("Segoe UI", 9F);
            lblDistrict.Location = new Point(15, 132);
            lblDistrict.Name = "lblDistrict";
            lblDistrict.Size = new Size(59, 20);
            lblDistrict.TabIndex = 6;
            lblDistrict.Text = "District:";
            // 
            // txtProvince
            // 
            txtProvince.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProvince.Font = new Font("Segoe UI", 9F);
            txtProvince.Location = new Point(180, 96);
            txtProvince.Name = "txtProvince";
            txtProvince.Size = new Size(233, 27);
            txtProvince.TabIndex = 5;
            // 
            // lblProvince
            // 
            lblProvince.AutoSize = true;
            lblProvince.Font = new Font("Segoe UI", 9F);
            lblProvince.Location = new Point(15, 99);
            lblProvince.Name = "lblProvince";
            lblProvince.Size = new Size(68, 20);
            lblProvince.TabIndex = 4;
            lblProvince.Text = "Province:";
            // 
            // txtMapSheetNo
            // 
            txtMapSheetNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMapSheetNo.Font = new Font("Segoe UI", 9F);
            txtMapSheetNo.Location = new Point(180, 63);
            txtMapSheetNo.Name = "txtMapSheetNo";
            txtMapSheetNo.Size = new Size(233, 27);
            txtMapSheetNo.TabIndex = 3;
            // 
            // lblMapSheetNo
            // 
            lblMapSheetNo.AutoSize = true;
            lblMapSheetNo.Font = new Font("Segoe UI", 9F);
            lblMapSheetNo.Location = new Point(15, 66);
            lblMapSheetNo.Name = "lblMapSheetNo";
            lblMapSheetNo.Size = new Size(117, 20);
            lblMapSheetNo.TabIndex = 2;
            lblMapSheetNo.Text = "Map Sheet No: *";
            // 
            // txtParcelNo
            // 
            txtParcelNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtParcelNo.Font = new Font("Segoe UI", 9F);
            txtParcelNo.Location = new Point(180, 30);
            txtParcelNo.Name = "txtParcelNo";
            txtParcelNo.Size = new Size(233, 27);
            txtParcelNo.TabIndex = 1;
            // 
            // lblParcelNo
            // 
            lblParcelNo.AutoSize = true;
            lblParcelNo.Font = new Font("Segoe UI", 9F);
            lblParcelNo.Location = new Point(15, 33);
            lblParcelNo.Name = "lblParcelNo";
            lblParcelNo.Size = new Size(85, 20);
            lblParcelNo.TabIndex = 0;
            lblParcelNo.Text = "Parcel No: *";
            // 
            // grpOwnerInfo
            // 
            grpOwnerInfo.Controls.Add(RbtnNo);
            grpOwnerInfo.Controls.Add(RbtnYes);
            grpOwnerInfo.Controls.Add(txtAddress);
            grpOwnerInfo.Controls.Add(lblAddress);
            grpOwnerInfo.Controls.Add(lblIsTenant);
            grpOwnerInfo.Controls.Add(txtCitizenshipNumber);
            grpOwnerInfo.Controls.Add(lblCitizenshipNumber);
            grpOwnerInfo.Controls.Add(cmbGender);
            grpOwnerInfo.Controls.Add(lblGender);
            grpOwnerInfo.Controls.Add(txtFatherSpouse);
            grpOwnerInfo.Controls.Add(lblFatherSpouse);
            grpOwnerInfo.Controls.Add(txtLandOwnersName);
            grpOwnerInfo.Controls.Add(lblLandOwnersName);
            grpOwnerInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpOwnerInfo.Location = new Point(12, 221);
            grpOwnerInfo.Name = "grpOwnerInfo";
            grpOwnerInfo.Size = new Size(437, 235);
            grpOwnerInfo.TabIndex = 1;
            grpOwnerInfo.TabStop = false;
            grpOwnerInfo.Text = "Owner Information";
            // 
            // RbtnNo
            // 
            RbtnNo.AutoSize = true;
            RbtnNo.Checked = true;
            RbtnNo.Location = new Point(239, 163);
            RbtnNo.Name = "RbtnNo";
            RbtnNo.Size = new Size(51, 24);
            RbtnNo.TabIndex = 10;
            RbtnNo.TabStop = true;
            RbtnNo.Text = "No";
            RbtnNo.UseVisualStyleBackColor = true;
            // 
            // RbtnYes
            // 
            RbtnYes.AutoSize = true;
            RbtnYes.Location = new Point(180, 164);
            RbtnYes.Name = "RbtnYes";
            RbtnYes.Size = new Size(53, 24);
            RbtnYes.TabIndex = 9;
            RbtnYes.TabStop = true;
            RbtnYes.Text = "Yes";
            RbtnYes.UseVisualStyleBackColor = true;
            // 
            // txtAddress
            // 
            txtAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAddress.Font = new Font("Segoe UI", 9F);
            txtAddress.Location = new Point(180, 196);
            txtAddress.Name = "txtAddress";
            txtAddress.Size = new Size(237, 27);
            txtAddress.TabIndex = 13;
            // 
            // lblAddress
            // 
            lblAddress.AutoSize = true;
            lblAddress.Font = new Font("Segoe UI", 9F);
            lblAddress.Location = new Point(15, 199);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(65, 20);
            lblAddress.TabIndex = 12;
            lblAddress.Text = "Address:";
            // 
            // lblIsTenant
            // 
            lblIsTenant.AutoSize = true;
            lblIsTenant.Font = new Font("Segoe UI", 9F);
            lblIsTenant.Location = new Point(15, 166);
            lblIsTenant.Name = "lblIsTenant";
            lblIsTenant.Size = new Size(74, 20);
            lblIsTenant.TabIndex = 8;
            lblIsTenant.Text = "Is Tenant :";
            // 
            // txtCitizenshipNumber
            // 
            txtCitizenshipNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCitizenshipNumber.Font = new Font("Segoe UI", 9F);
            txtCitizenshipNumber.Location = new Point(180, 130);
            txtCitizenshipNumber.Name = "txtCitizenshipNumber";
            txtCitizenshipNumber.Size = new Size(237, 27);
            txtCitizenshipNumber.TabIndex = 7;
            // 
            // lblCitizenshipNumber
            // 
            lblCitizenshipNumber.AutoSize = true;
            lblCitizenshipNumber.Font = new Font("Segoe UI", 9F);
            lblCitizenshipNumber.Location = new Point(15, 133);
            lblCitizenshipNumber.Name = "lblCitizenshipNumber";
            lblCitizenshipNumber.Size = new Size(142, 20);
            lblCitizenshipNumber.TabIndex = 6;
            lblCitizenshipNumber.Text = "Citizenship Number:";
            // 
            // cmbGender
            // 
            cmbGender.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.Font = new Font("Segoe UI", 9F);
            cmbGender.FormattingEnabled = true;
            cmbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cmbGender.Location = new Point(180, 96);
            cmbGender.Name = "cmbGender";
            cmbGender.Size = new Size(237, 28);
            cmbGender.TabIndex = 5;
            // 
            // lblGender
            // 
            lblGender.AutoSize = true;
            lblGender.Font = new Font("Segoe UI", 9F);
            lblGender.Location = new Point(15, 99);
            lblGender.Name = "lblGender";
            lblGender.Size = new Size(60, 20);
            lblGender.TabIndex = 4;
            lblGender.Text = "Gender:";
            // 
            // txtFatherSpouse
            // 
            txtFatherSpouse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFatherSpouse.Font = new Font("Segoe UI", 9F);
            txtFatherSpouse.Location = new Point(180, 63);
            txtFatherSpouse.Name = "txtFatherSpouse";
            txtFatherSpouse.Size = new Size(237, 27);
            txtFatherSpouse.TabIndex = 3;
            // 
            // lblFatherSpouse
            // 
            lblFatherSpouse.AutoSize = true;
            lblFatherSpouse.Font = new Font("Segoe UI", 9F);
            lblFatherSpouse.Location = new Point(15, 66);
            lblFatherSpouse.Name = "lblFatherSpouse";
            lblFatherSpouse.Size = new Size(106, 20);
            lblFatherSpouse.TabIndex = 2;
            lblFatherSpouse.Text = "Father/Spouse:";
            // 
            // txtLandOwnersName
            // 
            txtLandOwnersName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLandOwnersName.Font = new Font("Segoe UI", 9F);
            txtLandOwnersName.Location = new Point(180, 30);
            txtLandOwnersName.Name = "txtLandOwnersName";
            txtLandOwnersName.Size = new Size(237, 27);
            txtLandOwnersName.TabIndex = 1;
            // 
            // lblLandOwnersName
            // 
            lblLandOwnersName.AutoSize = true;
            lblLandOwnersName.Font = new Font("Segoe UI", 9F);
            lblLandOwnersName.Location = new Point(15, 33);
            lblLandOwnersName.Name = "lblLandOwnersName";
            lblLandOwnersName.Size = new Size(99, 20);
            lblLandOwnersName.TabIndex = 0;
            lblLandOwnersName.Text = "Owner Name:";
            // 
            // grpLandInfo
            // 
            grpLandInfo.Controls.Add(txtPaanaNo);
            grpLandInfo.Controls.Add(lblPaanaNo);
            grpLandInfo.Controls.Add(txtMothNo);
            grpLandInfo.Controls.Add(lblMothNo);
            grpLandInfo.Controls.Add(txtAreaInBKD);
            grpLandInfo.Controls.Add(lblAreaInBKD);
            grpLandInfo.Controls.Add(txtAreaInRAPD);
            grpLandInfo.Controls.Add(lblAreaInRAPD);
            grpLandInfo.Controls.Add(txtAreaInSqm);
            grpLandInfo.Controls.Add(lblAreaInSqm);
            grpLandInfo.Controls.Add(cmbLandUse);
            grpLandInfo.Controls.Add(lblLandUse);
            grpLandInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpLandInfo.Location = new Point(455, 12);
            grpLandInfo.Name = "grpLandInfo";
            grpLandInfo.Size = new Size(433, 230);
            grpLandInfo.TabIndex = 2;
            grpLandInfo.TabStop = false;
            grpLandInfo.Text = "Land Information";
            // 
            // txtPaanaNo
            // 
            txtPaanaNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPaanaNo.Font = new Font("Segoe UI", 9F);
            txtPaanaNo.Location = new Point(180, 196);
            txtPaanaNo.Name = "txtPaanaNo";
            txtPaanaNo.Size = new Size(233, 27);
            txtPaanaNo.TabIndex = 11;
            // 
            // lblPaanaNo
            // 
            lblPaanaNo.AutoSize = true;
            lblPaanaNo.Font = new Font("Segoe UI", 9F);
            lblPaanaNo.Location = new Point(15, 199);
            lblPaanaNo.Name = "lblPaanaNo";
            lblPaanaNo.Size = new Size(75, 20);
            lblPaanaNo.TabIndex = 10;
            lblPaanaNo.Text = "Paana No:";
            // 
            // txtMothNo
            // 
            txtMothNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMothNo.Font = new Font("Segoe UI", 9F);
            txtMothNo.Location = new Point(180, 163);
            txtMothNo.Name = "txtMothNo";
            txtMothNo.Size = new Size(233, 27);
            txtMothNo.TabIndex = 9;
            // 
            // lblMothNo
            // 
            lblMothNo.AutoSize = true;
            lblMothNo.Font = new Font("Segoe UI", 9F);
            lblMothNo.Location = new Point(15, 166);
            lblMothNo.Name = "lblMothNo";
            lblMothNo.Size = new Size(71, 20);
            lblMothNo.TabIndex = 8;
            lblMothNo.Text = "Moth No:";
            // 
            // txtAreaInBKD
            // 
            txtAreaInBKD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaInBKD.Font = new Font("Segoe UI", 9F);
            txtAreaInBKD.Location = new Point(180, 130);
            txtAreaInBKD.Name = "txtAreaInBKD";
            txtAreaInBKD.Size = new Size(233, 27);
            txtAreaInBKD.TabIndex = 7;
            // 
            // lblAreaInBKD
            // 
            lblAreaInBKD.AutoSize = true;
            lblAreaInBKD.Font = new Font("Segoe UI", 9F);
            lblAreaInBKD.Location = new Point(15, 133);
            lblAreaInBKD.Name = "lblAreaInBKD";
            lblAreaInBKD.Size = new Size(98, 20);
            lblAreaInBKD.TabIndex = 6;
            lblAreaInBKD.Text = "Area (B-K-D):";
            // 
            // txtAreaInRAPD
            // 
            txtAreaInRAPD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaInRAPD.Font = new Font("Segoe UI", 9F);
            txtAreaInRAPD.Location = new Point(180, 97);
            txtAreaInRAPD.Name = "txtAreaInRAPD";
            txtAreaInRAPD.Size = new Size(233, 27);
            txtAreaInRAPD.TabIndex = 5;
            // 
            // lblAreaInRAPD
            // 
            lblAreaInRAPD.AutoSize = true;
            lblAreaInRAPD.Font = new Font("Segoe UI", 9F);
            lblAreaInRAPD.Location = new Point(15, 100);
            lblAreaInRAPD.Name = "lblAreaInRAPD";
            lblAreaInRAPD.Size = new Size(113, 20);
            lblAreaInRAPD.TabIndex = 4;
            lblAreaInRAPD.Text = "Area (R-A-P-D):";
            // 
            // txtAreaInSqm
            // 
            txtAreaInSqm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaInSqm.Font = new Font("Segoe UI", 9F);
            txtAreaInSqm.Location = new Point(180, 64);
            txtAreaInSqm.Name = "txtAreaInSqm";
            txtAreaInSqm.Size = new Size(233, 27);
            txtAreaInSqm.TabIndex = 3;
            // 
            // lblAreaInSqm
            // 
            lblAreaInSqm.AutoSize = true;
            lblAreaInSqm.Font = new Font("Segoe UI", 9F);
            lblAreaInSqm.Location = new Point(15, 67);
            lblAreaInSqm.Name = "lblAreaInSqm";
            lblAreaInSqm.Size = new Size(98, 20);
            lblAreaInSqm.TabIndex = 2;
            lblAreaInSqm.Text = "Area (sq.m): *";
            // 
            // cmbLandUse
            // 
            cmbLandUse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLandUse.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLandUse.Font = new Font("Segoe UI", 9F);
            cmbLandUse.FormattingEnabled = true;
            cmbLandUse.Items.AddRange(new object[] { "Residential", "Agricultural", "Commercial", "Industrial", "Forest", "Other" });
            cmbLandUse.Location = new Point(180, 30);
            cmbLandUse.Name = "cmbLandUse";
            cmbLandUse.Size = new Size(233, 28);
            cmbLandUse.TabIndex = 1;
            // 
            // lblLandUse
            // 
            lblLandUse.AutoSize = true;
            lblLandUse.Font = new Font("Segoe UI", 9F);
            lblLandUse.Location = new Point(15, 33);
            lblLandUse.Name = "lblLandUse";
            lblLandUse.Size = new Size(72, 20);
            lblLandUse.TabIndex = 0;
            lblLandUse.Text = "Land Use:";
            // 
            // grpRemarks
            // 
            grpRemarks.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpRemarks.Controls.Add(txtRemarks);
            grpRemarks.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpRemarks.Location = new Point(455, 251);
            grpRemarks.Name = "grpRemarks";
            grpRemarks.Size = new Size(433, 214);
            grpRemarks.TabIndex = 3;
            grpRemarks.TabStop = false;
            grpRemarks.Text = "Remarks";
            // 
            // txtRemarks
            // 
            txtRemarks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtRemarks.Font = new Font("Segoe UI", 9F);
            txtRemarks.Location = new Point(15, 26);
            txtRemarks.Multiline = true;
            txtRemarks.Name = "txtRemarks";
            txtRemarks.Size = new Size(398, 179);
            txtRemarks.TabIndex = 0;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnUpdate);
            pnlButtons.Controls.Add(btnAdd);
            pnlButtons.Controls.Add(btnDelete);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 585);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(901, 55);
            pnlButtons.TabIndex = 4;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(789, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUpdate.Location = new Point(577, 10);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(100, 35);
            btnUpdate.TabIndex = 1;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(700, 10);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(83, 35);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(683, 10);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 35);
            btnDelete.TabIndex = 2;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // frmAddEditRecord
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(901, 640);
            Controls.Add(pnlButtons);
            Controls.Add(grpRemarks);
            Controls.Add(grpLandInfo);
            Controls.Add(grpOwnerInfo);
            Controls.Add(grpBasicInfo);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAddEditRecord";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add/Edit Record";
            Load += frmAddEditRecord_Load;
            grpBasicInfo.ResumeLayout(false);
            grpBasicInfo.PerformLayout();
            grpOwnerInfo.ResumeLayout(false);
            grpOwnerInfo.PerformLayout();
            grpLandInfo.ResumeLayout(false);
            grpLandInfo.PerformLayout();
            grpRemarks.ResumeLayout(false);
            grpRemarks.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpBasicInfo;
        private GroupBox grpOwnerInfo;
        private GroupBox grpLandInfo;
        private GroupBox grpRemarks;
        private Panel pnlButtons;
        private TextBox txtParcelNo;
        private Label lblParcelNo;
        private TextBox txtMapSheetNo;
        private Label lblMapSheetNo;
        private TextBox txtProvince;
        private Label lblProvince;
        private TextBox txtDistrict;
        private Label lblDistrict;
        private TextBox txtMunicipalityVillage;
        private Label lblMunicipalityVillage;
        private TextBox txtLandOwnersName;
        private Label lblLandOwnersName;
        private TextBox txtFatherSpouse;
        private Label lblFatherSpouse;
        private ComboBox cmbGender;
        private Label lblGender;
        private TextBox txtCitizenshipNumber;
        private Label lblCitizenshipNumber;
        private Label lblIsTenant;
        private TextBox txtAddress;
        private Label lblAddress;
        private ComboBox cmbLandUse;
        private Label lblLandUse;
        private TextBox txtAreaInSqm;
        private Label lblAreaInSqm;
        private TextBox txtAreaInRAPD;
        private Label lblAreaInRAPD;
        private TextBox txtAreaInBKD;
        private Label lblAreaInBKD;
        private TextBox txtMothNo;
        private Label lblMothNo;
        private TextBox txtPaanaNo;
        private Label lblPaanaNo;
        private TextBox txtRemarks;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnCancel;
        private RadioButton RbtnNo;
        private RadioButton RbtnYes;
    }
}