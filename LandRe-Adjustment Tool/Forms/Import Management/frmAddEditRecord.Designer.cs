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
            txtMapSheetNo = new TextBox();
            lblMapSheetNo = new Label();
            txtParcelNo = new TextBox();
            lblParcelNo = new Label();
            txtMunicipalityVillage = new TextBox();
            txtDistrict = new TextBox();
            txtProvince = new TextBox();
            grpOwnerInfo = new GroupBox();
            txtIssueDate = new TextBox();
            label6 = new Label();
            txtIssueDistrict = new TextBox();
            label5 = new Label();
            txtCitizenshipNumber = new TextBox();
            lblCitizenshipNumber = new Label();
            cmbGender = new ComboBox();
            lblGender = new Label();
            txtFatherSpouse = new TextBox();
            lblFatherSpouse = new Label();
            txtLandOwnersName = new TextBox();
            lblLandOwnersName = new Label();
            txtPermanentAddress = new TextBox();
            grpLandInfo = new GroupBox();
            txtAreaInBKD = new TextBox();
            lblAreaInBKD = new Label();
            txtAreaInRAPD = new TextBox();
            lblAreaInRAPD = new Label();
            txtAreaInSqm = new TextBox();
            lblAreaInSqm = new Label();
            txtPaanaNo = new TextBox();
            lblPaanaNo = new Label();
            txtMothNo = new TextBox();
            lblMothNo = new Label();
            grpRemarks = new GroupBox();
            txtRemarks = new TextBox();
            pnlButtons = new Panel();
            btnCancel = new Button();
            btnUpdate = new Button();
            btnAdd = new Button();
            btnDelete = new Button();
            groupBox1 = new GroupBox();
            txtWardNo = new TextBox();
            label4 = new Label();
            label3 = new Label();
            label1 = new Label();
            label2 = new Label();
            groupBox3 = new GroupBox();
            txtEmailID = new TextBox();
            label14 = new Label();
            txtTemporaryAddress = new TextBox();
            txtContactNo = new TextBox();
            label13 = new Label();
            label11 = new Label();
            label12 = new Label();
            grpRegistryRef = new GroupBox();
            groupBox6 = new GroupBox();
            txtTenant = new TextBox();
            label18 = new Label();
            label19 = new Label();
            cbOwnershipType = new ComboBox();
            label20 = new Label();
            cmbLandUse = new ComboBox();
            grpBasicInfo.SuspendLayout();
            grpOwnerInfo.SuspendLayout();
            grpLandInfo.SuspendLayout();
            grpRemarks.SuspendLayout();
            pnlButtons.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox3.SuspendLayout();
            grpRegistryRef.SuspendLayout();
            groupBox6.SuspendLayout();
            SuspendLayout();
            // 
            // grpBasicInfo
            // 
            grpBasicInfo.Controls.Add(txtMapSheetNo);
            grpBasicInfo.Controls.Add(lblMapSheetNo);
            grpBasicInfo.Controls.Add(txtParcelNo);
            grpBasicInfo.Controls.Add(lblParcelNo);
            grpBasicInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpBasicInfo.Location = new Point(12, 12);
            grpBasicInfo.Name = "grpBasicInfo";
            grpBasicInfo.Size = new Size(476, 68);
            grpBasicInfo.TabIndex = 0;
            grpBasicInfo.TabStop = false;
            grpBasicInfo.Tag = "100";
            grpBasicInfo.Text = "Parcel Identification";
            // 
            // txtMapSheetNo
            // 
            txtMapSheetNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMapSheetNo.Font = new Font("Segoe UI", 9F);
            txtMapSheetNo.Location = new Point(328, 30);
            txtMapSheetNo.Name = "txtMapSheetNo";
            txtMapSheetNo.Size = new Size(142, 27);
            txtMapSheetNo.TabIndex = 2;
            // 
            // lblMapSheetNo
            // 
            lblMapSheetNo.AutoSize = true;
            lblMapSheetNo.Font = new Font("Segoe UI", 9F);
            lblMapSheetNo.Location = new Point(208, 33);
            lblMapSheetNo.Name = "lblMapSheetNo";
            lblMapSheetNo.Size = new Size(117, 20);
            lblMapSheetNo.TabIndex = 2;
            lblMapSheetNo.Text = "Map Sheet No: *";
            // 
            // txtParcelNo
            // 
            txtParcelNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtParcelNo.Font = new Font("Segoe UI", 9F);
            txtParcelNo.Location = new Point(106, 30);
            txtParcelNo.Name = "txtParcelNo";
            txtParcelNo.Size = new Size(96, 27);
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
            // txtMunicipalityVillage
            // 
            txtMunicipalityVillage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMunicipalityVillage.Font = new Font("Segoe UI", 9F);
            txtMunicipalityVillage.Location = new Point(166, 63);
            txtMunicipalityVillage.Name = "txtMunicipalityVillage";
            txtMunicipalityVillage.Size = new Size(159, 27);
            txtMunicipalityVillage.TabIndex = 5;
            // 
            // txtDistrict
            // 
            txtDistrict.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtDistrict.Font = new Font("Segoe UI", 9F);
            txtDistrict.Location = new Point(328, 30);
            txtDistrict.Name = "txtDistrict";
            txtDistrict.Size = new Size(142, 27);
            txtDistrict.TabIndex = 4;
            // 
            // txtProvince
            // 
            txtProvince.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtProvince.Font = new Font("Segoe UI", 9F);
            txtProvince.Location = new Point(89, 30);
            txtProvince.Name = "txtProvince";
            txtProvince.Size = new Size(163, 27);
            txtProvince.TabIndex = 3;
            // 
            // grpOwnerInfo
            // 
            grpOwnerInfo.Controls.Add(txtIssueDate);
            grpOwnerInfo.Controls.Add(label6);
            grpOwnerInfo.Controls.Add(txtIssueDistrict);
            grpOwnerInfo.Controls.Add(label5);
            grpOwnerInfo.Controls.Add(txtCitizenshipNumber);
            grpOwnerInfo.Controls.Add(lblCitizenshipNumber);
            grpOwnerInfo.Controls.Add(cmbGender);
            grpOwnerInfo.Controls.Add(lblGender);
            grpOwnerInfo.Controls.Add(txtFatherSpouse);
            grpOwnerInfo.Controls.Add(lblFatherSpouse);
            grpOwnerInfo.Controls.Add(txtLandOwnersName);
            grpOwnerInfo.Controls.Add(lblLandOwnersName);
            grpOwnerInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpOwnerInfo.Location = new Point(12, 199);
            grpOwnerInfo.Name = "grpOwnerInfo";
            grpOwnerInfo.Size = new Size(476, 236);
            grpOwnerInfo.TabIndex = 1;
            grpOwnerInfo.TabStop = false;
            grpOwnerInfo.Tag = "100";
            grpOwnerInfo.Text = "Owner Information";
            // 
            // txtIssueDate
            // 
            txtIssueDate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIssueDate.Font = new Font("Segoe UI", 9F);
            txtIssueDate.Location = new Point(180, 196);
            txtIssueDate.Name = "txtIssueDate";
            txtIssueDate.Size = new Size(290, 27);
            txtIssueDate.TabIndex = 12;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 9F);
            label6.Location = new Point(15, 199);
            label6.Name = "label6";
            label6.Size = new Size(80, 20);
            label6.TabIndex = 6;
            label6.Text = "Issue Date:";
            // 
            // txtIssueDistrict
            // 
            txtIssueDistrict.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtIssueDistrict.Font = new Font("Segoe UI", 9F);
            txtIssueDistrict.Location = new Point(180, 163);
            txtIssueDistrict.Name = "txtIssueDistrict";
            txtIssueDistrict.Size = new Size(290, 27);
            txtIssueDistrict.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 9F);
            label5.Location = new Point(15, 166);
            label5.Name = "label5";
            label5.Size = new Size(95, 20);
            label5.TabIndex = 6;
            label5.Text = "Issue District:";
            // 
            // txtCitizenshipNumber
            // 
            txtCitizenshipNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCitizenshipNumber.Font = new Font("Segoe UI", 9F);
            txtCitizenshipNumber.Location = new Point(180, 130);
            txtCitizenshipNumber.Name = "txtCitizenshipNumber";
            txtCitizenshipNumber.Size = new Size(290, 27);
            txtCitizenshipNumber.TabIndex = 10;
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
            cmbGender.Size = new Size(165, 28);
            cmbGender.TabIndex = 9;
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
            txtFatherSpouse.Size = new Size(290, 27);
            txtFatherSpouse.TabIndex = 8;
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
            txtLandOwnersName.Size = new Size(290, 27);
            txtLandOwnersName.TabIndex = 7;
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
            // txtPermanentAddress
            // 
            txtPermanentAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPermanentAddress.Font = new Font("Segoe UI", 9F);
            txtPermanentAddress.Location = new Point(180, 30);
            txtPermanentAddress.Name = "txtPermanentAddress";
            txtPermanentAddress.Size = new Size(290, 27);
            txtPermanentAddress.TabIndex = 13;
            // 
            // grpLandInfo
            // 
            grpLandInfo.Controls.Add(txtAreaInBKD);
            grpLandInfo.Controls.Add(lblAreaInBKD);
            grpLandInfo.Controls.Add(txtAreaInRAPD);
            grpLandInfo.Controls.Add(lblAreaInRAPD);
            grpLandInfo.Controls.Add(txtAreaInSqm);
            grpLandInfo.Controls.Add(lblAreaInSqm);
            grpLandInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpLandInfo.Location = new Point(494, 24);
            grpLandInfo.Name = "grpLandInfo";
            grpLandInfo.Size = new Size(433, 137);
            grpLandInfo.TabIndex = 2;
            grpLandInfo.TabStop = false;
            grpLandInfo.Tag = "100";
            grpLandInfo.Text = "Area Information";
            // 
            // txtAreaInBKD
            // 
            txtAreaInBKD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaInBKD.Font = new Font("Segoe UI", 9F);
            txtAreaInBKD.Location = new Point(180, 96);
            txtAreaInBKD.Name = "txtAreaInBKD";
            txtAreaInBKD.Size = new Size(245, 27);
            txtAreaInBKD.TabIndex = 19;
            // 
            // lblAreaInBKD
            // 
            lblAreaInBKD.AutoSize = true;
            lblAreaInBKD.Font = new Font("Segoe UI", 9F);
            lblAreaInBKD.Location = new Point(15, 99);
            lblAreaInBKD.Name = "lblAreaInBKD";
            lblAreaInBKD.Size = new Size(98, 20);
            lblAreaInBKD.TabIndex = 6;
            lblAreaInBKD.Text = "Area (B-K-D):";
            // 
            // txtAreaInRAPD
            // 
            txtAreaInRAPD.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaInRAPD.Font = new Font("Segoe UI", 9F);
            txtAreaInRAPD.Location = new Point(180, 63);
            txtAreaInRAPD.Name = "txtAreaInRAPD";
            txtAreaInRAPD.Size = new Size(245, 27);
            txtAreaInRAPD.TabIndex = 18;
            // 
            // lblAreaInRAPD
            // 
            lblAreaInRAPD.AutoSize = true;
            lblAreaInRAPD.Font = new Font("Segoe UI", 9F);
            lblAreaInRAPD.Location = new Point(15, 66);
            lblAreaInRAPD.Name = "lblAreaInRAPD";
            lblAreaInRAPD.Size = new Size(113, 20);
            lblAreaInRAPD.TabIndex = 4;
            lblAreaInRAPD.Text = "Area (R-A-P-D):";
            // 
            // txtAreaInSqm
            // 
            txtAreaInSqm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaInSqm.Font = new Font("Segoe UI", 9F);
            txtAreaInSqm.Location = new Point(180, 30);
            txtAreaInSqm.Name = "txtAreaInSqm";
            txtAreaInSqm.Size = new Size(245, 27);
            txtAreaInSqm.TabIndex = 17;
            // 
            // lblAreaInSqm
            // 
            lblAreaInSqm.AutoSize = true;
            lblAreaInSqm.Font = new Font("Segoe UI", 9F);
            lblAreaInSqm.Location = new Point(15, 33);
            lblAreaInSqm.Name = "lblAreaInSqm";
            lblAreaInSqm.Size = new Size(98, 20);
            lblAreaInSqm.TabIndex = 2;
            lblAreaInSqm.Text = "Area (sq.m): *";
            // 
            // txtPaanaNo
            // 
            txtPaanaNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPaanaNo.Font = new Font("Segoe UI", 9F);
            txtPaanaNo.Location = new Point(320, 33);
            txtPaanaNo.Name = "txtPaanaNo";
            txtPaanaNo.Size = new Size(107, 27);
            txtPaanaNo.TabIndex = 24;
            // 
            // lblPaanaNo
            // 
            lblPaanaNo.AutoSize = true;
            lblPaanaNo.Font = new Font("Segoe UI", 9F);
            lblPaanaNo.Location = new Point(244, 36);
            lblPaanaNo.Name = "lblPaanaNo";
            lblPaanaNo.Size = new Size(75, 20);
            lblPaanaNo.TabIndex = 10;
            lblPaanaNo.Text = "Paana No:";
            // 
            // txtMothNo
            // 
            txtMothNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMothNo.Font = new Font("Segoe UI", 9F);
            txtMothNo.Location = new Point(106, 33);
            txtMothNo.Name = "txtMothNo";
            txtMothNo.Size = new Size(110, 27);
            txtMothNo.TabIndex = 23;
            // 
            // lblMothNo
            // 
            lblMothNo.AutoSize = true;
            lblMothNo.Font = new Font("Segoe UI", 9F);
            lblMothNo.Location = new Point(15, 36);
            lblMothNo.Name = "lblMothNo";
            lblMothNo.Size = new Size(71, 20);
            lblMothNo.TabIndex = 8;
            lblMothNo.Text = "Moth No.";
            // 
            // grpRemarks
            // 
            grpRemarks.Controls.Add(txtRemarks);
            grpRemarks.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpRemarks.Location = new Point(494, 391);
            grpRemarks.Name = "grpRemarks";
            grpRemarks.Size = new Size(433, 215);
            grpRemarks.TabIndex = 3;
            grpRemarks.TabStop = false;
            grpRemarks.Tag = "100";
            grpRemarks.Text = "Remarks/Notes";
            // 
            // txtRemarks
            // 
            txtRemarks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtRemarks.Font = new Font("Segoe UI", 9F);
            txtRemarks.Location = new Point(15, 26);
            txtRemarks.Multiline = true;
            txtRemarks.Name = "txtRemarks";
            txtRemarks.Size = new Size(410, 180);
            txtRemarks.TabIndex = 25;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Controls.Add(btnUpdate);
            pnlButtons.Controls.Add(btnAdd);
            pnlButtons.Controls.Add(btnDelete);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 613);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(935, 55);
            pnlButtons.TabIndex = 4;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(823, 10);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(100, 35);
            btnCancel.TabIndex = 28;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUpdate.Location = new Point(611, 10);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(100, 35);
            btnUpdate.TabIndex = 26;
            btnUpdate.Text = "Update";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.Location = new Point(734, 10);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(83, 35);
            btnAdd.TabIndex = 27;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.Location = new Point(717, 10);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(100, 35);
            btnDelete.TabIndex = 27;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtMunicipalityVillage);
            groupBox1.Controls.Add(txtWardNo);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(txtDistrict);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(txtProvince);
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox1.Location = new Point(12, 86);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(476, 105);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Tag = "100";
            groupBox1.Text = "Administrative Information";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // txtWardNo
            // 
            txtWardNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtWardNo.Font = new Font("Segoe UI", 9F);
            txtWardNo.Location = new Point(411, 63);
            txtWardNo.Name = "txtWardNo";
            txtWardNo.Size = new Size(59, 27);
            txtWardNo.TabIndex = 6;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F);
            label4.Location = new Point(331, 66);
            label4.Name = "label4";
            label4.Size = new Size(74, 20);
            label4.TabIndex = 2;
            label4.Text = "Ward No.:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F);
            label3.Location = new Point(15, 66);
            label3.Name = "label3";
            label3.Size = new Size(145, 20);
            label3.TabIndex = 2;
            label3.Text = "Municipality/Village:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(263, 33);
            label1.Name = "label1";
            label1.Size = new Size(59, 20);
            label1.TabIndex = 2;
            label1.Text = "District:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F);
            label2.Location = new Point(15, 33);
            label2.Name = "label2";
            label2.Size = new Size(68, 20);
            label2.TabIndex = 0;
            label2.Text = "Province:";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(txtEmailID);
            groupBox3.Controls.Add(txtPermanentAddress);
            groupBox3.Controls.Add(label14);
            groupBox3.Controls.Add(txtTemporaryAddress);
            groupBox3.Controls.Add(txtContactNo);
            groupBox3.Controls.Add(label13);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(label12);
            groupBox3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox3.Location = new Point(12, 441);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(476, 165);
            groupBox3.TabIndex = 0;
            groupBox3.TabStop = false;
            groupBox3.Tag = "100";
            groupBox3.Text = "Address and Contact Information";
            // 
            // txtEmailID
            // 
            txtEmailID.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtEmailID.Font = new Font("Segoe UI", 9F);
            txtEmailID.Location = new Point(180, 129);
            txtEmailID.Name = "txtEmailID";
            txtEmailID.Size = new Size(290, 27);
            txtEmailID.TabIndex = 16;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Segoe UI", 9F);
            label14.Location = new Point(15, 132);
            label14.Name = "label14";
            label14.Size = new Size(112, 20);
            label14.TabIndex = 2;
            label14.Text = "E-mail Address:";
            // 
            // txtTemporaryAddress
            // 
            txtTemporaryAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTemporaryAddress.Font = new Font("Segoe UI", 9F);
            txtTemporaryAddress.Location = new Point(180, 63);
            txtTemporaryAddress.Name = "txtTemporaryAddress";
            txtTemporaryAddress.Size = new Size(290, 27);
            txtTemporaryAddress.TabIndex = 14;
            // 
            // txtContactNo
            // 
            txtContactNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtContactNo.Font = new Font("Segoe UI", 9F);
            txtContactNo.Location = new Point(180, 96);
            txtContactNo.Name = "txtContactNo";
            txtContactNo.Size = new Size(290, 27);
            txtContactNo.TabIndex = 15;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Font = new Font("Segoe UI", 9F);
            label13.Location = new Point(15, 99);
            label13.Name = "label13";
            label13.Size = new Size(121, 20);
            label13.TabIndex = 2;
            label13.Text = "Contact Number:";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Font = new Font("Segoe UI", 9F);
            label11.Location = new Point(15, 66);
            label11.Name = "label11";
            label11.Size = new Size(140, 20);
            label11.TabIndex = 2;
            label11.Text = "Temporary Address:";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Segoe UI", 9F);
            label12.Location = new Point(15, 33);
            label12.Name = "label12";
            label12.Size = new Size(139, 20);
            label12.TabIndex = 0;
            label12.Text = "Permanent Address:";
            // 
            // grpRegistryRef
            // 
            grpRegistryRef.Controls.Add(txtPaanaNo);
            grpRegistryRef.Controls.Add(lblPaanaNo);
            grpRegistryRef.Controls.Add(txtMothNo);
            grpRegistryRef.Controls.Add(lblMothNo);
            grpRegistryRef.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpRegistryRef.Location = new Point(494, 310);
            grpRegistryRef.Name = "grpRegistryRef";
            grpRegistryRef.Size = new Size(433, 75);
            grpRegistryRef.TabIndex = 0;
            grpRegistryRef.TabStop = false;
            grpRegistryRef.Tag = "100";
            grpRegistryRef.Text = "Land Registry Reference";
            // 
            // groupBox6
            // 
            groupBox6.Controls.Add(txtTenant);
            groupBox6.Controls.Add(label18);
            groupBox6.Controls.Add(label19);
            groupBox6.Controls.Add(cbOwnershipType);
            groupBox6.Controls.Add(label20);
            groupBox6.Controls.Add(cmbLandUse);
            groupBox6.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            groupBox6.Location = new Point(494, 167);
            groupBox6.Name = "groupBox6";
            groupBox6.Size = new Size(433, 137);
            groupBox6.TabIndex = 2;
            groupBox6.TabStop = false;
            groupBox6.Tag = "100";
            groupBox6.Text = "Other Parcel Information";
            // 
            // txtTenant
            // 
            txtTenant.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTenant.Font = new Font("Segoe UI", 9F);
            txtTenant.Location = new Point(180, 30);
            txtTenant.Name = "txtTenant";
            txtTenant.Size = new Size(245, 27);
            txtTenant.TabIndex = 20;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Segoe UI", 9F);
            label18.Location = new Point(15, 67);
            label18.Name = "label18";
            label18.Size = new Size(117, 20);
            label18.TabIndex = 0;
            label18.Text = "Ownership Type:";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Segoe UI", 9F);
            label19.Location = new Point(15, 33);
            label19.Name = "label19";
            label19.Size = new Size(56, 20);
            label19.TabIndex = 0;
            label19.Text = "Tenant:";
            // 
            // cbOwnershipType
            // 
            cbOwnershipType.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cbOwnershipType.DropDownStyle = ComboBoxStyle.DropDownList;
            cbOwnershipType.Font = new Font("Segoe UI", 9F);
            cbOwnershipType.FormattingEnabled = true;
            cbOwnershipType.Items.AddRange(new object[] { "Private (Single)", "Private (Joint)", "Public (Government)", "Trust (Guthi)" });
            cbOwnershipType.Location = new Point(180, 64);
            cbOwnershipType.Name = "cbOwnershipType";
            cbOwnershipType.Size = new Size(247, 28);
            cbOwnershipType.TabIndex = 21;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Font = new Font("Segoe UI", 9F);
            label20.Location = new Point(15, 101);
            label20.Name = "label20";
            label20.Size = new Size(72, 20);
            label20.TabIndex = 0;
            label20.Text = "Land Use:";
            // 
            // cmbLandUse
            // 
            cmbLandUse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLandUse.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLandUse.Font = new Font("Segoe UI", 9F);
            cmbLandUse.FormattingEnabled = true;
            cmbLandUse.Items.AddRange(new object[] { "Residential", "Agricultural", "Commercial", "Industrial", "Forest", "Other" });
            cmbLandUse.Location = new Point(180, 98);
            cmbLandUse.Name = "cmbLandUse";
            cmbLandUse.Size = new Size(247, 28);
            cmbLandUse.TabIndex = 22;
            // 
            // frmAddEditRecord
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(935, 668);
            Controls.Add(pnlButtons);
            Controls.Add(grpRemarks);
            Controls.Add(groupBox6);
            Controls.Add(grpLandInfo);
            Controls.Add(grpOwnerInfo);
            Controls.Add(groupBox1);
            Controls.Add(groupBox3);
            Controls.Add(grpBasicInfo);
            Controls.Add(grpRegistryRef);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAddEditRecord";
            StartPosition = FormStartPosition.CenterParent;
            Tag = "100";
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
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            grpRegistryRef.ResumeLayout(false);
            grpRegistryRef.PerformLayout();
            groupBox6.ResumeLayout(false);
            groupBox6.PerformLayout();
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
        private TextBox txtDistrict;
        private TextBox txtMunicipalityVillage;
        private TextBox txtLandOwnersName;
        private Label lblLandOwnersName;
        private TextBox txtFatherSpouse;
        private Label lblFatherSpouse;
        private ComboBox cmbGender;
        private Label lblGender;
        private TextBox txtCitizenshipNumber;
        private Label lblCitizenshipNumber;
        private TextBox txtPermanentAddress;
        private Label lblAddress;
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
        private GroupBox groupBox1;
        private Label label1;
        private Label label2;
        private TextBox txtWardNo;
        private Label label4;
        private Label label3;
        private TextBox txtIssueDate;
        private Label label6;
        private TextBox txtIssueDistrict;
        private Label label5;
        private GroupBox groupBox3;
        private TextBox txtTemporaryAddress;
        private Label label11;
        private Label label12;
        private Label label13;
        private TextBox txtContactNo;
        private Label label14;
        private TextBox txtEmailID;
        private GroupBox grpRegistryRef;
        private GroupBox groupBox6;
        private Label label18;
        private Label label19;
        private ComboBox cbOwnershipType;
        private Label label20;
        private ComboBox cmbLandUse;
        private TextBox txtTenant;
    }
}