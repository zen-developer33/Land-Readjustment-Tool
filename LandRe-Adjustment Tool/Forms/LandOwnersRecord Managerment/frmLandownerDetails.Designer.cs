namespace Land_Readjustment_Tool.Forms
{
    partial class frmLandownerDetails
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
            pnlLeft = new Panel();
            btnAttachDocument = new Button();
            btnDeleteDocument = new Button();
            lstDocuments = new ListBox();
            lblAttachDocuments = new Label();
            btnUploadPhoto = new Button();
            picPhoto = new PictureBox();
            pnlRight = new Panel();
            txtAddress = new TextBox();
            lblAddress = new Label();
            cmbLandUse = new ComboBox();
            lblLandUse = new Label();
            txtAreaSqm = new TextBox();
            lblAreaSqm = new Label();
            txtParcelNo = new TextBox();
            lblParcelNo = new Label();
            txtCitizenshipNo = new TextBox();
            lblCitizenshipNo = new Label();
            txtFatherSpouse = new TextBox();
            lblFatherSpouse = new Label();
            txtName = new TextBox();
            lblName = new Label();
            pnlBottom = new Panel();
            lblTotalRecords = new Label();
            btnClose = new Button();
            btnSaveChanges = new Button();
            pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto).BeginInit();
            pnlRight.SuspendLayout();
            pnlBottom.SuspendLayout();
            SuspendLayout();
            // 
            // pnlLeft
            // 
            pnlLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlLeft.Controls.Add(btnAttachDocument);
            pnlLeft.Controls.Add(btnDeleteDocument);
            pnlLeft.Controls.Add(lstDocuments);
            pnlLeft.Controls.Add(lblAttachDocuments);
            pnlLeft.Controls.Add(btnUploadPhoto);
            pnlLeft.Controls.Add(picPhoto);
            pnlLeft.Location = new Point(12, 12);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(350, 690);
            pnlLeft.TabIndex = 0;
            // 
            // btnAttachDocument
            // 
            btnAttachDocument.Location = new Point(15, 390);
            btnAttachDocument.Name = "btnAttachDocument";
            btnAttachDocument.Size = new Size(320, 30);
            btnAttachDocument.TabIndex = 6;
            btnAttachDocument.Text = "+ Attach Document";
            btnAttachDocument.UseVisualStyleBackColor = true;
            btnAttachDocument.Click += btnAttachDocument_Click;
            // 
            // btnDeleteDocument
            // 
            btnDeleteDocument.ImageAlign = ContentAlignment.MiddleLeft;
            btnDeleteDocument.Location = new Point(15, 645);
            btnDeleteDocument.Name = "btnDeleteDocument";
            btnDeleteDocument.Padding = new Padding(5, 0, 5, 0);
            btnDeleteDocument.Size = new Size(320, 35);
            btnDeleteDocument.TabIndex = 5;
            btnDeleteDocument.Text = "Delete";
            btnDeleteDocument.UseVisualStyleBackColor = true;
            btnDeleteDocument.Click += btnDeleteDocument_Click;
            // 
            // lstDocuments
            // 
            lstDocuments.FormattingEnabled = true;
            lstDocuments.Location = new Point(15, 425);
            lstDocuments.Name = "lstDocuments";
            lstDocuments.Size = new Size(320, 184);
            lstDocuments.TabIndex = 4;
            lstDocuments.DoubleClick += lstDocuments_DoubleClick;
            // 
            // lblAttachDocuments
            // 
            lblAttachDocuments.AutoSize = true;
            lblAttachDocuments.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAttachDocuments.Location = new Point(15, 365);
            lblAttachDocuments.Name = "lblAttachDocuments";
            lblAttachDocuments.Size = new Size(140, 20);
            lblAttachDocuments.TabIndex = 3;
            lblAttachDocuments.Text = "Attach Documents";
            // 
            // btnUploadPhoto
            // 
            btnUploadPhoto.ImageAlign = ContentAlignment.MiddleLeft;
            btnUploadPhoto.Location = new Point(15, 315);
            btnUploadPhoto.Name = "btnUploadPhoto";
            btnUploadPhoto.Padding = new Padding(5, 0, 5, 0);
            btnUploadPhoto.Size = new Size(320, 35);
            btnUploadPhoto.TabIndex = 1;
            btnUploadPhoto.Text = "Upload Photo...";
            btnUploadPhoto.UseVisualStyleBackColor = true;
            btnUploadPhoto.Click += btnUploadPhoto_Click;
            // 
            // picPhoto
            // 
            picPhoto.BorderStyle = BorderStyle.FixedSingle;
            picPhoto.Location = new Point(15, 15);
            picPhoto.Name = "picPhoto";
            picPhoto.Size = new Size(180, 210);
            picPhoto.SizeMode = PictureBoxSizeMode.Zoom;
            picPhoto.TabIndex = 0;
            picPhoto.TabStop = false;
            // 
            // pnlRight
            // 
            pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlRight.Controls.Add(txtAddress);
            pnlRight.Controls.Add(lblAddress);
            pnlRight.Controls.Add(cmbLandUse);
            pnlRight.Controls.Add(lblLandUse);
            pnlRight.Controls.Add(txtAreaSqm);
            pnlRight.Controls.Add(lblAreaSqm);
            pnlRight.Controls.Add(txtParcelNo);
            pnlRight.Controls.Add(lblParcelNo);
            pnlRight.Controls.Add(txtCitizenshipNo);
            pnlRight.Controls.Add(lblCitizenshipNo);
            pnlRight.Controls.Add(txtFatherSpouse);
            pnlRight.Controls.Add(lblFatherSpouse);
            pnlRight.Controls.Add(txtName);
            pnlRight.Controls.Add(lblName);
            pnlRight.Location = new Point(368, 12);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(850, 690);
            pnlRight.TabIndex = 1;
            // 
            // txtAddress
            // 
            txtAddress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAddress.Location = new Point(160, 420);
            txtAddress.Multiline = true;
            txtAddress.Name = "txtAddress";
            txtAddress.ScrollBars = ScrollBars.Vertical;
            txtAddress.Size = new Size(675, 250);
            txtAddress.TabIndex = 13;
            // 
            // lblAddress
            // 
            lblAddress.AutoSize = true;
            lblAddress.Location = new Point(15, 423);
            lblAddress.Name = "lblAddress";
            lblAddress.Size = new Size(65, 20);
            lblAddress.TabIndex = 12;
            lblAddress.Text = "Address:";
            // 
            // cmbLandUse
            // 
            cmbLandUse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbLandUse.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLandUse.FormattingEnabled = true;
            cmbLandUse.Location = new Point(160, 360);
            cmbLandUse.Name = "cmbLandUse";
            cmbLandUse.Size = new Size(675, 28);
            cmbLandUse.TabIndex = 11;
            // 
            // lblLandUse
            // 
            lblLandUse.AutoSize = true;
            lblLandUse.Location = new Point(15, 363);
            lblLandUse.Name = "lblLandUse";
            lblLandUse.Size = new Size(72, 20);
            lblLandUse.TabIndex = 10;
            lblLandUse.Text = "Land Use:";
            // 
            // txtAreaSqm
            // 
            txtAreaSqm.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtAreaSqm.Location = new Point(160, 300);
            txtAreaSqm.Name = "txtAreaSqm";
            txtAreaSqm.Size = new Size(675, 27);
            txtAreaSqm.TabIndex = 9;
            // 
            // lblAreaSqm
            // 
            lblAreaSqm.AutoSize = true;
            lblAreaSqm.Location = new Point(15, 303);
            lblAreaSqm.Name = "lblAreaSqm";
            lblAreaSqm.Size = new Size(85, 20);
            lblAreaSqm.TabIndex = 8;
            lblAreaSqm.Text = "Area (sqm):";
            // 
            // txtParcelNo
            // 
            txtParcelNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtParcelNo.Location = new Point(160, 240);
            txtParcelNo.Name = "txtParcelNo";
            txtParcelNo.Size = new Size(675, 27);
            txtParcelNo.TabIndex = 7;
            // 
            // lblParcelNo
            // 
            lblParcelNo.AutoSize = true;
            lblParcelNo.Location = new Point(15, 243);
            lblParcelNo.Name = "lblParcelNo";
            lblParcelNo.Size = new Size(75, 20);
            lblParcelNo.TabIndex = 6;
            lblParcelNo.Text = "Parcel No:";
            // 
            // txtCitizenshipNo
            // 
            txtCitizenshipNo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCitizenshipNo.Location = new Point(160, 180);
            txtCitizenshipNo.Name = "txtCitizenshipNo";
            txtCitizenshipNo.Size = new Size(675, 27);
            txtCitizenshipNo.TabIndex = 5;
            // 
            // lblCitizenshipNo
            // 
            lblCitizenshipNo.AutoSize = true;
            lblCitizenshipNo.Location = new Point(15, 183);
            lblCitizenshipNo.Name = "lblCitizenshipNo";
            lblCitizenshipNo.Size = new Size(108, 20);
            lblCitizenshipNo.TabIndex = 4;
            lblCitizenshipNo.Text = "Citizenship No:";
            // 
            // txtFatherSpouse
            // 
            txtFatherSpouse.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFatherSpouse.Location = new Point(160, 120);
            txtFatherSpouse.Name = "txtFatherSpouse";
            txtFatherSpouse.Size = new Size(675, 27);
            txtFatherSpouse.TabIndex = 3;
            // 
            // lblFatherSpouse
            // 
            lblFatherSpouse.AutoSize = true;
            lblFatherSpouse.Location = new Point(15, 123);
            lblFatherSpouse.Name = "lblFatherSpouse";
            lblFatherSpouse.Size = new Size(106, 20);
            lblFatherSpouse.TabIndex = 2;
            lblFatherSpouse.Text = "Father/Spouse:";
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtName.Location = new Point(160, 60);
            txtName.Name = "txtName";
            txtName.Size = new Size(675, 27);
            txtName.TabIndex = 1;
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(15, 63);
            lblName.Name = "lblName";
            lblName.Size = new Size(52, 20);
            lblName.TabIndex = 0;
            lblName.Text = "Name:";
            // 
            // pnlBottom
            // 
            pnlBottom.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlBottom.Controls.Add(lblTotalRecords);
            pnlBottom.Controls.Add(btnClose);
            pnlBottom.Controls.Add(btnSaveChanges);
            pnlBottom.Location = new Point(12, 708);
            pnlBottom.Name = "pnlBottom";
            pnlBottom.Size = new Size(1206, 60);
            pnlBottom.TabIndex = 2;
            // 
            // lblTotalRecords
            // 
            lblTotalRecords.AutoSize = true;
            lblTotalRecords.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblTotalRecords.Location = new Point(15, 20);
            lblTotalRecords.Name = "lblTotalRecords";
            lblTotalRecords.Size = new Size(121, 20);
            lblTotalRecords.TabIndex = 2;
            lblTotalRecords.Text = "Total Records: 6";
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.ImageAlign = ContentAlignment.MiddleLeft;
            btnClose.Location = new Point(1070, 10);
            btnClose.Name = "btnClose";
            btnClose.Padding = new Padding(10, 0, 10, 0);
            btnClose.Size = new Size(120, 40);
            btnClose.TabIndex = 1;
            btnClose.Text = "Close";
            btnClose.TextAlign = ContentAlignment.MiddleRight;
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveChanges.ImageAlign = ContentAlignment.MiddleLeft;
            btnSaveChanges.Location = new Point(900, 10);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Padding = new Padding(10, 0, 10, 0);
            btnSaveChanges.Size = new Size(160, 40);
            btnSaveChanges.TabIndex = 0;
            btnSaveChanges.Text = "Save Changes";
            btnSaveChanges.TextAlign = ContentAlignment.MiddleRight;
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // frmLandownerDetails
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1230, 780);
            Controls.Add(pnlBottom);
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmLandownerDetails";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Landowner Record Details";
            Load += frmLandownerDetails_Load;
            pnlLeft.ResumeLayout(false);
            pnlLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picPhoto).EndInit();
            pnlRight.ResumeLayout(false);
            pnlRight.PerformLayout();
            pnlBottom.ResumeLayout(false);
            pnlBottom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlLeft;
        private PictureBox picPhoto;
        private Button btnUploadPhoto;
        private Label lblAttachDocuments;
        private ListBox lstDocuments;
        private Button btnDeleteDocument;
        private Panel pnlRight;
        private Label lblName;
        private TextBox txtName;
        private Label lblFatherSpouse;
        private TextBox txtFatherSpouse;
        private Label lblCitizenshipNo;
        private TextBox txtCitizenshipNo;
        private Label lblParcelNo;
        private TextBox txtParcelNo;
        private Label lblAreaSqm;
        private TextBox txtAreaSqm;
        private Label lblLandUse;
        private ComboBox cmbLandUse;
        private Label lblAddress;
        private TextBox txtAddress;
        private Panel pnlBottom;
        private Button btnSaveChanges;
        private Button btnClose;
        private Label lblTotalRecords;
        private Button btnAttachDocument;
    }
}
