namespace Land_Readjustment_Tool.Forms
{
    partial class frmLandownerDetails_2
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
            pnlPhoto = new Panel();
            btnUploadPhoto = new Button();
            picPhoto = new PictureBox();
            grpActions = new GroupBox();
            btnViewDocuments = new Button();
            btnViewParcels = new Button();
            pnlBottom = new Panel();
            btnClose = new Button();
            lblNameLabel = new Label();
            lblNameValue = new Label();
            lblFatherSpouseLabel = new Label();
            lblFatherSpouseValue = new Label();
            lblCitizenshipNoLabel = new Label();
            lblCitizenshipNoValue = new Label();
            lblPermanentAddressLabel = new Label();
            lblGenderLabel = new Label();
            lblGenderValue = new Label();
            label1 = new Label();
            lblPermanentAddressValue = new Label();
            label2 = new Label();
            grpOwnerInfo = new GroupBox();
            pnlPhoto.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto).BeginInit();
            grpActions.SuspendLayout();
            pnlBottom.SuspendLayout();
            grpOwnerInfo.SuspendLayout();
            SuspendLayout();
            // 
            // pnlPhoto
            // 
            pnlPhoto.BackColor = SystemColors.ControlLight;
            pnlPhoto.Controls.Add(btnUploadPhoto);
            pnlPhoto.Controls.Add(picPhoto);
            pnlPhoto.Location = new Point(13, 3);
            pnlPhoto.Margin = new Padding(4);
            pnlPhoto.Name = "pnlPhoto";
            pnlPhoto.Size = new Size(222, 368);
            pnlPhoto.TabIndex = 0;
            // 
            // btnUploadPhoto
            // 
            btnUploadPhoto.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnUploadPhoto.FlatStyle = FlatStyle.Flat;
            btnUploadPhoto.Image = Properties.Resources.icons8_photo_25;
            btnUploadPhoto.ImageAlign = ContentAlignment.MiddleRight;
            btnUploadPhoto.Location = new Point(0, 239);
            btnUploadPhoto.Margin = new Padding(4);
            btnUploadPhoto.Name = "btnUploadPhoto";
            btnUploadPhoto.Size = new Size(214, 41);
            btnUploadPhoto.TabIndex = 1;
            btnUploadPhoto.Text = "Upload/Change Photo";
            btnUploadPhoto.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnUploadPhoto.UseVisualStyleBackColor = true;
            btnUploadPhoto.Click += btnUploadPhoto_Click;
            // 
            // picPhoto
            // 
            picPhoto.BackgroundImage = Properties.Resources.Portrait_Placeholder1;
            picPhoto.BackgroundImageLayout = ImageLayout.Zoom;
            picPhoto.BorderStyle = BorderStyle.FixedSingle;
            picPhoto.Location = new Point(4, 4);
            picPhoto.Margin = new Padding(4);
            picPhoto.Name = "picPhoto";
            picPhoto.Size = new Size(173, 211);
            picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            picPhoto.TabIndex = 0;
            picPhoto.TabStop = false;
            // 
            // grpActions
            // 
            grpActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpActions.Controls.Add(btnViewDocuments);
            grpActions.Controls.Add(btnViewParcels);
            grpActions.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpActions.Location = new Point(243, 291);
            grpActions.Margin = new Padding(4);
            grpActions.Name = "grpActions";
            grpActions.Padding = new Padding(4);
            grpActions.Size = new Size(614, 80);
            grpActions.TabIndex = 2;
            grpActions.TabStop = false;
            grpActions.Text = "Actions";
            // 
            // btnViewDocuments
            // 
            btnViewDocuments.Font = new Font("Segoe UI", 9F);
            btnViewDocuments.Image = Properties.Resources.attach_icon3;
            btnViewDocuments.Location = new Point(8, 28);
            btnViewDocuments.Margin = new Padding(4);
            btnViewDocuments.Name = "btnViewDocuments";
            btnViewDocuments.Size = new Size(316, 44);
            btnViewDocuments.TabIndex = 0;
            btnViewDocuments.Text = "Attach/View Documents";
            btnViewDocuments.TextAlign = ContentAlignment.MiddleRight;
            btnViewDocuments.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnViewDocuments.UseVisualStyleBackColor = true;
            btnViewDocuments.Click += btnViewDocuments_Click;
            // 
            // btnViewParcels
            // 
            btnViewParcels.Font = new Font("Segoe UI", 9F);
            btnViewParcels.Image = Properties.Resources.icons8_view_25;
            btnViewParcels.Location = new Point(332, 28);
            btnViewParcels.Margin = new Padding(4);
            btnViewParcels.Name = "btnViewParcels";
            btnViewParcels.Size = new Size(270, 44);
            btnViewParcels.TabIndex = 1;
            btnViewParcels.Text = "View Parcels";
            btnViewParcels.TextAlign = ContentAlignment.MiddleRight;
            btnViewParcels.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnViewParcels.UseVisualStyleBackColor = true;
            btnViewParcels.Click += btnViewParcels_Click;
            // 
            // pnlBottom
            // 
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Dock = DockStyle.Bottom;
            pnlBottom.Location = new Point(0, 554);
            pnlBottom.Margin = new Padding(4);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(870, 53);
            pnlBottom.TabIndex = 3;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(719, 4);
            btnClose.Margin = new Padding(4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(138, 45);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblNameLabel
            // 
            lblNameLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNameLabel.ForeColor = Color.DimGray;
            lblNameLabel.Location = new Point(18, 28);
            lblNameLabel.Margin = new Padding(4, 0, 4, 0);
            lblNameLabel.Name = "lblNameLabel";
            lblNameLabel.Size = new Size(154, 31);
            lblNameLabel.TabIndex = 0;
            lblNameLabel.Text = "Name:";
            lblNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblNameValue
            // 
            lblNameValue.BorderStyle = BorderStyle.FixedSingle;
            lblNameValue.Font = new Font("Segoe UI", 9F);
            lblNameValue.Location = new Point(180, 28);
            lblNameValue.Margin = new Padding(4, 0, 4, 0);
            lblNameValue.Name = "lblNameValue";
            lblNameValue.Size = new Size(422, 31);
            lblNameValue.TabIndex = 1;
            lblNameValue.Text = "-";
            lblNameValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFatherSpouseLabel
            // 
            lblFatherSpouseLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFatherSpouseLabel.ForeColor = Color.DimGray;
            lblFatherSpouseLabel.Location = new Point(18, 67);
            lblFatherSpouseLabel.Margin = new Padding(4, 0, 4, 0);
            lblFatherSpouseLabel.Name = "lblFatherSpouseLabel";
            lblFatherSpouseLabel.Size = new Size(154, 31);
            lblFatherSpouseLabel.TabIndex = 2;
            lblFatherSpouseLabel.Text = "Father/Spouse:";
            lblFatherSpouseLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFatherSpouseValue
            // 
            lblFatherSpouseValue.BorderStyle = BorderStyle.FixedSingle;
            lblFatherSpouseValue.Font = new Font("Segoe UI", 9F);
            lblFatherSpouseValue.Location = new Point(180, 67);
            lblFatherSpouseValue.Margin = new Padding(4, 0, 4, 0);
            lblFatherSpouseValue.Name = "lblFatherSpouseValue";
            lblFatherSpouseValue.Size = new Size(422, 31);
            lblFatherSpouseValue.TabIndex = 3;
            lblFatherSpouseValue.Text = "-";
            lblFatherSpouseValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCitizenshipNoLabel
            // 
            lblCitizenshipNoLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCitizenshipNoLabel.ForeColor = Color.DimGray;
            lblCitizenshipNoLabel.Location = new Point(18, 106);
            lblCitizenshipNoLabel.Margin = new Padding(4, 0, 4, 0);
            lblCitizenshipNoLabel.Name = "lblCitizenshipNoLabel";
            lblCitizenshipNoLabel.Size = new Size(154, 31);
            lblCitizenshipNoLabel.TabIndex = 4;
            lblCitizenshipNoLabel.Text = "Citizenship No:";
            lblCitizenshipNoLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCitizenshipNoValue
            // 
            lblCitizenshipNoValue.BorderStyle = BorderStyle.FixedSingle;
            lblCitizenshipNoValue.Font = new Font("Segoe UI", 9F);
            lblCitizenshipNoValue.Location = new Point(180, 106);
            lblCitizenshipNoValue.Margin = new Padding(4, 0, 4, 0);
            lblCitizenshipNoValue.Name = "lblCitizenshipNoValue";
            lblCitizenshipNoValue.Size = new Size(422, 31);
            lblCitizenshipNoValue.TabIndex = 5;
            lblCitizenshipNoValue.Text = "-";
            lblCitizenshipNoValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPermanentAddressLabel
            // 
            lblPermanentAddressLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPermanentAddressLabel.ForeColor = Color.DimGray;
            lblPermanentAddressLabel.Location = new Point(18, 184);
            lblPermanentAddressLabel.Margin = new Padding(4, 0, 4, 0);
            lblPermanentAddressLabel.Name = "lblPermanentAddressLabel";
            lblPermanentAddressLabel.Size = new Size(154, 31);
            lblPermanentAddressLabel.TabIndex = 8;
            lblPermanentAddressLabel.Text = "Permanent Address:";
            lblPermanentAddressLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGenderLabel
            // 
            lblGenderLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblGenderLabel.ForeColor = Color.DimGray;
            lblGenderLabel.Location = new Point(18, 145);
            lblGenderLabel.Margin = new Padding(4, 0, 4, 0);
            lblGenderLabel.Name = "lblGenderLabel";
            lblGenderLabel.Size = new Size(154, 31);
            lblGenderLabel.TabIndex = 6;
            lblGenderLabel.Text = "Gender:";
            lblGenderLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblGenderValue
            // 
            lblGenderValue.BorderStyle = BorderStyle.FixedSingle;
            lblGenderValue.Font = new Font("Segoe UI", 9F);
            lblGenderValue.Location = new Point(180, 145);
            lblGenderValue.Margin = new Padding(4, 0, 4, 0);
            lblGenderValue.Name = "lblGenderValue";
            lblGenderValue.Size = new Size(422, 31);
            lblGenderValue.TabIndex = 7;
            lblGenderValue.Text = "-";
            lblGenderValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.BorderStyle = BorderStyle.FixedSingle;
            label1.Font = new Font("Segoe UI", 9F);
            label1.Location = new Point(180, 223);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(422, 31);
            label1.TabIndex = 11;
            label1.Text = "-";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblPermanentAddressValue
            // 
            lblPermanentAddressValue.BorderStyle = BorderStyle.FixedSingle;
            lblPermanentAddressValue.Font = new Font("Segoe UI", 9F);
            lblPermanentAddressValue.Location = new Point(180, 184);
            lblPermanentAddressValue.Margin = new Padding(4, 0, 4, 0);
            lblPermanentAddressValue.Name = "lblPermanentAddressValue";
            lblPermanentAddressValue.Size = new Size(422, 31);
            lblPermanentAddressValue.TabIndex = 9;
            lblPermanentAddressValue.Text = "-";
            lblPermanentAddressValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.ForeColor = Color.DimGray;
            label2.Location = new Point(18, 223);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(154, 31);
            label2.TabIndex = 10;
            label2.Text = "Contact Number: ";
            label2.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // grpOwnerInfo
            // 
            grpOwnerInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpOwnerInfo.Controls.Add(label2);
            grpOwnerInfo.Controls.Add(lblPermanentAddressValue);
            grpOwnerInfo.Controls.Add(label1);
            grpOwnerInfo.Controls.Add(lblGenderValue);
            grpOwnerInfo.Controls.Add(lblGenderLabel);
            grpOwnerInfo.Controls.Add(lblPermanentAddressLabel);
            grpOwnerInfo.Controls.Add(lblCitizenshipNoValue);
            grpOwnerInfo.Controls.Add(lblCitizenshipNoLabel);
            grpOwnerInfo.Controls.Add(lblFatherSpouseValue);
            grpOwnerInfo.Controls.Add(lblFatherSpouseLabel);
            grpOwnerInfo.Controls.Add(lblNameValue);
            grpOwnerInfo.Controls.Add(lblNameLabel);
            grpOwnerInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            grpOwnerInfo.Location = new Point(243, 3);
            grpOwnerInfo.Margin = new Padding(4);
            grpOwnerInfo.Name = "grpOwnerInfo";
            grpOwnerInfo.Padding = new Padding(4);
            grpOwnerInfo.Size = new Size(614, 280);
            grpOwnerInfo.TabIndex = 1;
            grpOwnerInfo.TabStop = false;
            grpOwnerInfo.Text = "Owner Information";
            // 
            // frmLandownerDetails_2
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(870, 607);
            Controls.Add(pnlBottom);
            Controls.Add(grpActions);
            Controls.Add(grpOwnerInfo);
            Controls.Add(pnlPhoto);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLandownerDetails_2";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Landowner Details";
            Load += frmLandownerDetails_Load;
            pnlPhoto.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picPhoto).EndInit();
            grpActions.ResumeLayout(false);
            pnlBottom.ResumeLayout(false);
            grpOwnerInfo.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlPhoto;
        private PictureBox picPhoto;
        private Button btnUploadPhoto;
        private GroupBox grpActions;
        private Button btnViewDocuments;
        private Button btnViewParcels;
        private Panel pnlBottom;
        private Button btnClose;
        private Label lblNameLabel;
        private Label lblNameValue;
        private Label lblFatherSpouseLabel;
        private Label lblFatherSpouseValue;
        private Label lblCitizenshipNoLabel;
        private Label lblCitizenshipNoValue;
        private Label lblPermanentAddressLabel;
        private Label lblGenderLabel;
        private Label lblGenderValue;
        private Label label1;
        private Label lblPermanentAddressValue;
        private Label label2;
        private GroupBox grpOwnerInfo;
    }
}
