namespace Land_Readjustment_Tool.Forms.Land_Owners_Record
{
    partial class frmLandOwnerDetails
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
            pnlPhoto = new Panel();
            btnUploadChangePhoto = new Button();
            picPhoto = new PictureBox();
            lblNameLabel = new Label();
            lblFatherSpouseLabel = new Label();
            lblCitizenshipNoLabel = new Label();
            lblGenderLabel = new Label();
            txtFullName = new TextBox();
            grpOwnerInfo = new GroupBox();
            txtIssueDate = new TextBox();
            txtIssueDistrict = new TextBox();
            txtCitizenshipNo = new TextBox();
            cbGender = new ComboBox();
            txtFatherSpouse = new TextBox();
            label2 = new Label();
            label1 = new Label();
            groupBox1 = new GroupBox();
            txtEmailID = new TextBox();
            label5 = new Label();
            txtContactNumber = new TextBox();
            label4 = new Label();
            txtTemporaryAddress = new TextBox();
            label3 = new Label();
            txtPermanentAddress = new TextBox();
            label8 = new Label();
            btnCancel = new Button();
            btnAttachViewDocuments = new Button();
            btnViewParcels = new Button();
            grpSummary = new GroupBox();
            lblParcelCount = new Label();
            lblAreaLocal = new Label();
            lblAreasqm = new Label();
            label11 = new Label();
            label12 = new Label();
            label7 = new Label();
            btnSave = new Button();
            groupBox2 = new GroupBox();
            btnClose = new Button();
            chkEdit = new CheckBox();
            pnlPhoto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto).BeginInit();
            grpOwnerInfo.SuspendLayout();
            groupBox1.SuspendLayout();
            grpSummary.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // pnlPhoto
            // 
            pnlPhoto.Controls.Add(btnUploadChangePhoto);
            pnlPhoto.Controls.Add(picPhoto);
            pnlPhoto.Location = new Point(13, 59);
            pnlPhoto.Margin = new Padding(4);
            pnlPhoto.Name = "pnlPhoto";
            pnlPhoto.Size = new Size(245, 290);
            pnlPhoto.TabIndex = 1;
            pnlPhoto.Paint += pnlPhoto_Paint;
            // 
            // btnUploadChangePhoto
            // 
            btnUploadChangePhoto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnUploadChangePhoto.Image = Properties.Resources.icons8_upload_25;
            btnUploadChangePhoto.ImageAlign = ContentAlignment.MiddleRight;
            btnUploadChangePhoto.Location = new Point(4, 247);
            btnUploadChangePhoto.Margin = new Padding(4);
            btnUploadChangePhoto.Name = "btnUploadChangePhoto";
            btnUploadChangePhoto.Size = new Size(237, 36);
            btnUploadChangePhoto.TabIndex = 11;
            btnUploadChangePhoto.Text = "Upload/Change Photo";
            btnUploadChangePhoto.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUploadChangePhoto.UseVisualStyleBackColor = true;
            btnUploadChangePhoto.Click += btnUploadChangePhoto_Click;
            // 
            // picPhoto
            // 
            picPhoto.BackgroundImage = Properties.Resources.Portrait_Placeholder1;
            picPhoto.BackgroundImageLayout = ImageLayout.Zoom;
            picPhoto.BorderStyle = BorderStyle.FixedSingle;
            picPhoto.Location = new Point(22, 7);
            picPhoto.Margin = new Padding(4);
            picPhoto.Name = "picPhoto";
            picPhoto.Size = new Size(198, 235);
            picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            picPhoto.TabIndex = 0;
            picPhoto.TabStop = false;
            // 
            // lblNameLabel
            // 
            lblNameLabel.Font = new Font("Segoe UI", 9F);
            lblNameLabel.ForeColor = Color.Black;
            lblNameLabel.Location = new Point(18, 27);
            lblNameLabel.Margin = new Padding(4, 0, 4, 0);
            lblNameLabel.Name = "lblNameLabel";
            lblNameLabel.Size = new Size(154, 31);
            lblNameLabel.TabIndex = 0;
            lblNameLabel.Text = "*Full Name:";
            lblNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFatherSpouseLabel
            // 
            lblFatherSpouseLabel.Font = new Font("Segoe UI", 9F);
            lblFatherSpouseLabel.ForeColor = Color.Black;
            lblFatherSpouseLabel.Location = new Point(18, 60);
            lblFatherSpouseLabel.Margin = new Padding(4, 0, 4, 0);
            lblFatherSpouseLabel.Name = "lblFatherSpouseLabel";
            lblFatherSpouseLabel.Size = new Size(154, 31);
            lblFatherSpouseLabel.TabIndex = 2;
            lblFatherSpouseLabel.Text = "Father/Spouse:";
            lblFatherSpouseLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCitizenshipNoLabel
            // 
            lblCitizenshipNoLabel.Font = new Font("Segoe UI", 9F);
            lblCitizenshipNoLabel.ForeColor = Color.Black;
            lblCitizenshipNoLabel.Location = new Point(18, 126);
            lblCitizenshipNoLabel.Margin = new Padding(4, 0, 4, 0);
            lblCitizenshipNoLabel.Name = "lblCitizenshipNoLabel";
            lblCitizenshipNoLabel.Size = new Size(154, 31);
            lblCitizenshipNoLabel.TabIndex = 4;
            lblCitizenshipNoLabel.Text = "Citizenship No:";
            lblCitizenshipNoLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGenderLabel
            // 
            lblGenderLabel.Font = new Font("Segoe UI", 9F);
            lblGenderLabel.ForeColor = Color.Black;
            lblGenderLabel.Location = new Point(18, 93);
            lblGenderLabel.Margin = new Padding(4, 0, 4, 0);
            lblGenderLabel.Name = "lblGenderLabel";
            lblGenderLabel.Size = new Size(154, 31);
            lblGenderLabel.TabIndex = 6;
            lblGenderLabel.Text = "Gender:";
            lblGenderLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtFullName
            // 
            txtFullName.BorderStyle = BorderStyle.FixedSingle;
            txtFullName.Font = new Font("Segoe UI", 9F);
            txtFullName.Location = new Point(205, 30);
            txtFullName.Name = "txtFullName";
            txtFullName.Size = new Size(257, 27);
            txtFullName.TabIndex = 1;
            // 
            // grpOwnerInfo
            // 
            grpOwnerInfo.Controls.Add(txtIssueDate);
            grpOwnerInfo.Controls.Add(txtIssueDistrict);
            grpOwnerInfo.Controls.Add(txtCitizenshipNo);
            grpOwnerInfo.Controls.Add(cbGender);
            grpOwnerInfo.Controls.Add(txtFatherSpouse);
            grpOwnerInfo.Controls.Add(label2);
            grpOwnerInfo.Controls.Add(txtFullName);
            grpOwnerInfo.Controls.Add(label1);
            grpOwnerInfo.Controls.Add(lblGenderLabel);
            grpOwnerInfo.Controls.Add(lblCitizenshipNoLabel);
            grpOwnerInfo.Controls.Add(lblFatherSpouseLabel);
            grpOwnerInfo.Controls.Add(lblNameLabel);
            grpOwnerInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpOwnerInfo.Location = new Point(266, 59);
            grpOwnerInfo.Margin = new Padding(4);
            grpOwnerInfo.Name = "grpOwnerInfo";
            grpOwnerInfo.Padding = new Padding(4);
            grpOwnerInfo.Size = new Size(469, 239);
            grpOwnerInfo.TabIndex = 100;
            grpOwnerInfo.TabStop = false;
            grpOwnerInfo.Text = "Personal Information";
            // 
            // txtIssueDate
            // 
            txtIssueDate.BorderStyle = BorderStyle.FixedSingle;
            txtIssueDate.Font = new Font("Segoe UI", 9F);
            txtIssueDate.Location = new Point(205, 195);
            txtIssueDate.Name = "txtIssueDate";
            txtIssueDate.Size = new Size(257, 27);
            txtIssueDate.TabIndex = 6;
            // 
            // txtIssueDistrict
            // 
            txtIssueDistrict.BorderStyle = BorderStyle.FixedSingle;
            txtIssueDistrict.Font = new Font("Segoe UI", 9F);
            txtIssueDistrict.Location = new Point(205, 162);
            txtIssueDistrict.Name = "txtIssueDistrict";
            txtIssueDistrict.Size = new Size(257, 27);
            txtIssueDistrict.TabIndex = 5;
            // 
            // txtCitizenshipNo
            // 
            txtCitizenshipNo.BorderStyle = BorderStyle.FixedSingle;
            txtCitizenshipNo.Font = new Font("Segoe UI", 9F);
            txtCitizenshipNo.Location = new Point(205, 129);
            txtCitizenshipNo.Name = "txtCitizenshipNo";
            txtCitizenshipNo.Size = new Size(257, 27);
            txtCitizenshipNo.TabIndex = 4;
            // 
            // cbGender
            // 
            cbGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cbGender.FlatStyle = FlatStyle.System;
            cbGender.Font = new Font("Segoe UI", 9F);
            cbGender.FormattingEnabled = true;
            cbGender.Items.AddRange(new object[] { "Male", "Female", "Other" });
            cbGender.Location = new Point(205, 96);
            cbGender.Name = "cbGender";
            cbGender.Size = new Size(257, 28);
            cbGender.TabIndex = 12;
            cbGender.Tag = "3";
            // 
            // txtFatherSpouse
            // 
            txtFatherSpouse.BorderStyle = BorderStyle.FixedSingle;
            txtFatherSpouse.Font = new Font("Segoe UI", 9F);
            txtFatherSpouse.Location = new Point(205, 63);
            txtFatherSpouse.Name = "txtFatherSpouse";
            txtFatherSpouse.Size = new Size(257, 27);
            txtFatherSpouse.TabIndex = 2;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 9F);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(18, 192);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(194, 31);
            label2.TabIndex = 4;
            label2.Text = "Citizenship Issue Date:";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.Font = new Font("Segoe UI", 9F);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(18, 159);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(194, 31);
            label1.TabIndex = 4;
            label1.Text = "Citizenship Issue District :";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtEmailID);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(txtContactNumber);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(txtTemporaryAddress);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(txtPermanentAddress);
            groupBox1.Controls.Add(label8);
            groupBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox1.Location = new Point(267, 306);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new Size(468, 170);
            groupBox1.TabIndex = 101;
            groupBox1.TabStop = false;
            groupBox1.Text = "Address and Contact Info";
            // 
            // txtEmailID
            // 
            txtEmailID.BorderStyle = BorderStyle.FixedSingle;
            txtEmailID.Font = new Font("Segoe UI", 9F);
            txtEmailID.Location = new Point(205, 129);
            txtEmailID.Name = "txtEmailID";
            txtEmailID.Size = new Size(256, 27);
            txtEmailID.TabIndex = 10;
            // 
            // label5
            // 
            label5.Font = new Font("Segoe UI", 9F);
            label5.ForeColor = Color.Black;
            label5.Location = new Point(18, 127);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(154, 31);
            label5.TabIndex = 0;
            label5.Text = "Email ID:";
            label5.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtContactNumber
            // 
            txtContactNumber.BorderStyle = BorderStyle.FixedSingle;
            txtContactNumber.Font = new Font("Segoe UI", 9F);
            txtContactNumber.Location = new Point(205, 96);
            txtContactNumber.Name = "txtContactNumber";
            txtContactNumber.Size = new Size(256, 27);
            txtContactNumber.TabIndex = 9;
            // 
            // label4
            // 
            label4.Font = new Font("Segoe UI", 9F);
            label4.ForeColor = Color.Black;
            label4.Location = new Point(18, 94);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(154, 31);
            label4.TabIndex = 0;
            label4.Text = "Contact Number:";
            label4.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtTemporaryAddress
            // 
            txtTemporaryAddress.BorderStyle = BorderStyle.FixedSingle;
            txtTemporaryAddress.Font = new Font("Segoe UI", 9F);
            txtTemporaryAddress.Location = new Point(205, 63);
            txtTemporaryAddress.Name = "txtTemporaryAddress";
            txtTemporaryAddress.Size = new Size(256, 27);
            txtTemporaryAddress.TabIndex = 8;
            // 
            // label3
            // 
            label3.Font = new Font("Segoe UI", 9F);
            label3.ForeColor = Color.Black;
            label3.Location = new Point(18, 61);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(154, 31);
            label3.TabIndex = 0;
            label3.Text = "Temporary Address:";
            label3.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtPermanentAddress
            // 
            txtPermanentAddress.BorderStyle = BorderStyle.FixedSingle;
            txtPermanentAddress.Font = new Font("Segoe UI", 9F);
            txtPermanentAddress.Location = new Point(205, 30);
            txtPermanentAddress.Name = "txtPermanentAddress";
            txtPermanentAddress.Size = new Size(256, 27);
            txtPermanentAddress.TabIndex = 7;
            // 
            // label8
            // 
            label8.Font = new Font("Segoe UI", 9F);
            label8.ForeColor = Color.Black;
            label8.Location = new Point(18, 28);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(154, 31);
            label8.TabIndex = 0;
            label8.Text = "Permanent Address:";
            label8.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnCancel
            // 
            btnCancel.Image = Properties.Resources.delete_icon_251;
            btnCancel.Location = new Point(637, 12);
            btnCancel.Margin = new Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(98, 38);
            btnCancel.TabIndex = 13;
            btnCancel.Text = "Cancel";
            btnCancel.TextAlign = ContentAlignment.MiddleRight;
            btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnAttachViewDocuments
            // 
            btnAttachViewDocuments.Font = new Font("Segoe UI", 9F);
            btnAttachViewDocuments.Image = Properties.Resources.attach_icon3;
            btnAttachViewDocuments.ImageAlign = ContentAlignment.MiddleRight;
            btnAttachViewDocuments.Location = new Point(368, 28);
            btnAttachViewDocuments.Margin = new Padding(4);
            btnAttachViewDocuments.Name = "btnAttachViewDocuments";
            btnAttachViewDocuments.Size = new Size(346, 42);
            btnAttachViewDocuments.TabIndex = 12;
            btnAttachViewDocuments.Text = "Attach/View Documents (0)";
            btnAttachViewDocuments.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAttachViewDocuments.UseVisualStyleBackColor = true;
            btnAttachViewDocuments.Click += btnAttachViewDocuments_Click;
            // 
            // btnViewParcels
            // 
            btnViewParcels.Font = new Font("Segoe UI", 9F);
            btnViewParcels.Image = Properties.Resources.icons8_view_25;
            btnViewParcels.ImageAlign = ContentAlignment.MiddleRight;
            btnViewParcels.Location = new Point(8, 28);
            btnViewParcels.Margin = new Padding(4);
            btnViewParcels.Name = "btnViewParcels";
            btnViewParcels.Size = new Size(346, 42);
            btnViewParcels.TabIndex = 1;
            btnViewParcels.Text = "View Parcels Owned (0)";
            btnViewParcels.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnViewParcels.UseVisualStyleBackColor = true;
            btnViewParcels.Click += btnViewParcels_Click;
            // 
            // grpSummary
            // 
            grpSummary.Controls.Add(lblParcelCount);
            grpSummary.Controls.Add(lblAreaLocal);
            grpSummary.Controls.Add(lblAreasqm);
            grpSummary.Controls.Add(label11);
            grpSummary.Controls.Add(label12);
            grpSummary.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpSummary.Location = new Point(13, 353);
            grpSummary.Margin = new Padding(4);
            grpSummary.Name = "grpSummary";
            grpSummary.Padding = new Padding(4);
            grpSummary.Size = new Size(245, 123);
            grpSummary.TabIndex = 2;
            grpSummary.TabStop = false;
            grpSummary.Text = "Summary";
            // 
            // lblParcelCount
            // 
            lblParcelCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblParcelCount.ForeColor = Color.Black;
            lblParcelCount.Location = new Point(157, 24);
            lblParcelCount.Margin = new Padding(4, 0, 4, 0);
            lblParcelCount.Name = "lblParcelCount";
            lblParcelCount.Size = new Size(27, 31);
            lblParcelCount.TabIndex = 3;
            lblParcelCount.Text = "-";
            lblParcelCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAreaLocal
            // 
            lblAreaLocal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAreaLocal.ForeColor = Color.Black;
            lblAreaLocal.Location = new Point(157, 87);
            lblAreaLocal.Margin = new Padding(4, 0, 4, 0);
            lblAreaLocal.Name = "lblAreaLocal";
            lblAreaLocal.Size = new Size(76, 22);
            lblAreaLocal.TabIndex = 3;
            lblAreaLocal.Text = "-";
            lblAreaLocal.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAreasqm
            // 
            lblAreasqm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAreasqm.ForeColor = Color.Black;
            lblAreasqm.Location = new Point(157, 60);
            lblAreasqm.Margin = new Padding(4, 0, 4, 0);
            lblAreasqm.Name = "lblAreasqm";
            lblAreasqm.Size = new Size(85, 31);
            lblAreasqm.TabIndex = 3;
            lblAreasqm.Text = "jgkjhkj";
            lblAreasqm.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            label11.Font = new Font("Segoe UI", 9F);
            label11.ForeColor = Color.Black;
            label11.Location = new Point(8, 24);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(150, 31);
            label11.TabIndex = 2;
            label11.Text = "No. of Parcel Owned:";
            label11.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            label12.Font = new Font("Segoe UI", 9F);
            label12.ForeColor = Color.Black;
            label12.Location = new Point(8, 55);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(141, 31);
            label12.TabIndex = 0;
            label12.Text = "Total Area Owned: ";
            label12.TextAlign = ContentAlignment.MiddleLeft;
            label12.Click += label12_Click;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.MidnightBlue;
            label7.Location = new Point(35, 14);
            label7.Name = "label7";
            label7.Size = new Size(202, 28);
            label7.TabIndex = 15;
            label7.Text = "Land Owner's Detail";
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            btnSave.Image = Properties.Resources.diskette21;
            btnSave.Location = new Point(519, 12);
            btnSave.Margin = new Padding(4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(110, 38);
            btnSave.TabIndex = 13;
            btnSave.Text = "Save";
            btnSave.TextAlign = ContentAlignment.MiddleRight;
            btnSave.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(btnAttachViewDocuments);
            groupBox2.Controls.Add(btnViewParcels);
            groupBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            groupBox2.Location = new Point(13, 484);
            groupBox2.Margin = new Padding(4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4);
            groupBox2.Size = new Size(722, 79);
            groupBox2.TabIndex = 102;
            groupBox2.TabStop = false;
            groupBox2.Text = "Actions";
            // 
            // btnClose
            // 
            btnClose.Location = new Point(637, 571);
            btnClose.Margin = new Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(98, 37);
            btnClose.TabIndex = 14;
            btnClose.Text = "Close";
            btnClose.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // chkEdit
            // 
            chkEdit.Appearance = Appearance.Button;
            chkEdit.Image = Properties.Resources.edit_icon1;
            chkEdit.Location = new Point(363, 12);
            chkEdit.Name = "chkEdit";
            chkEdit.Size = new Size(149, 38);
            chkEdit.TabIndex = 104;
            chkEdit.Text = "Edit";
            chkEdit.TextAlign = ContentAlignment.MiddleRight;
            chkEdit.TextImageRelation = TextImageRelation.ImageBeforeText;
            chkEdit.UseVisualStyleBackColor = true;
            chkEdit.CheckedChanged += chkEdit_CheckedChanged;
            // 
            // frmLandOwnerDetails
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(741, 614);
            Controls.Add(chkEdit);
            Controls.Add(btnClose);
            Controls.Add(btnSave);
            Controls.Add(label7);
            Controls.Add(btnCancel);
            Controls.Add(groupBox1);
            Controls.Add(groupBox2);
            Controls.Add(grpSummary);
            Controls.Add(grpOwnerInfo);
            Controls.Add(pnlPhoto);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "frmLandOwnerDetails";
            StartPosition = FormStartPosition.CenterParent;
            Text = "frmLandOwnerDetails";
            Load += frmLandOwnerDetails_Load;
            pnlPhoto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPhoto).EndInit();
            grpOwnerInfo.ResumeLayout(false);
            grpOwnerInfo.PerformLayout();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            grpSummary.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlPhoto;
        private Button btnUploadChangePhoto;
        private PictureBox picPhoto;
        private Label lblNameLabel;
        private Label lblFatherSpouseLabel;
        private Label lblCitizenshipNoLabel;
        private Label lblGenderLabel;
        private TextBox txtFullName;
        private GroupBox grpOwnerInfo;
        private TextBox txtIssueDistrict;
        private TextBox txtCitizenshipNo;
        private ComboBox cbGender;
        private TextBox txtFatherSpouse;
        private Label label1;
        private TextBox txtIssueDate;
        private Label label2;
        private GroupBox groupBox1;
        private TextBox txtPermanentAddress;
        private Label label8;
        private TextBox txtEmailID;
        private Label label5;
        private TextBox txtContactNumber;
        private Label label4;
        private TextBox txtTemporaryAddress;
        private Label label3;
        private Button btnAttachViewDocuments;
        private Button btnViewParcels;
        private GroupBox grpSummary;
        private Label label11;
        private Label label12;
        private Label lblParcelCount;
        private Button btnCancel;
        private Label label7;
        private Label lblAreaLocal;
        private Label lblAreasqm;
        private Button btnSave;
        private GroupBox groupBox2;
        private Button btnClose;
        private Button btnEdit;
        private CheckBox chkEdit;
    }
}