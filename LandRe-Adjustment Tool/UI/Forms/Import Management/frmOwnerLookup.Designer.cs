namespace Land_Readjustment_Tool.Forms
{
    partial class frmOwnerLookup
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private TableLayoutPanel headerLayout;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblSearch;
        private TextBox txtSearch;
        private SplitContainer splitMain;
        private Panel gridPanel;
        private DataGridView dgvOwners;
        private Panel cardPanel;
        private TableLayoutPanel cardLayout;
        private Label lblPreviewTitle;
        private PictureBox picOwner;
        private Label lblPrimaryCaption;
        private Label lblPrimaryValue;
        private Label lblFatherCaption;
        private Label lblFatherValue;
        private Label lblCitizenshipCaption;
        private Label lblCitizenshipValue;
        private Label lblAddressCaption;
        private Label lblAddressValue;
        private Label lblCoOwnerCount;
        private Panel pnlActions;
        private Label lblSelectionHint;
        private Button btnLoad;
        private Button btnCancel;
        private DataGridViewRadioButtonColumn colIsPrimaryOwner;
        private DataGridViewCheckBoxColumn colIsCoOwner;
        private DataGridViewTextBoxColumn colLandOwnerId;
        private DataGridViewTextBoxColumn colLandOwnersName;
        private DataGridViewTextBoxColumn colFatherSpouse;
        private DataGridViewTextBoxColumn colCitizenshipNumber;
        private DataGridViewTextBoxColumn colPermanentAddress;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            mainLayout = new TableLayoutPanel();
            headerLayout = new TableLayoutPanel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblSearch = new Label();
            txtSearch = new TextBox();
            splitMain = new SplitContainer();
            gridPanel = new Panel();
            dgvOwners = new DataGridView();
            cardPanel = new Panel();
            cardLayout = new TableLayoutPanel();
            lblPreviewTitle = new Label();
            picOwner = new PictureBox();
            lblPrimaryCaption = new Label();
            lblPrimaryValue = new Label();
            lblFatherCaption = new Label();
            lblFatherValue = new Label();
            lblCitizenshipCaption = new Label();
            lblCitizenshipValue = new Label();
            lblAddressCaption = new Label();
            lblAddressValue = new Label();
            lblCoOwnerCount = new Label();
            pnlActions = new Panel();
            lblSelectionHint = new Label();
            btnLoad = new Button();
            btnCancel = new Button();
            mainLayout.SuspendLayout();
            headerLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            gridPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvOwners).BeginInit();
            cardPanel.SuspendLayout();
            cardLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picOwner).BeginInit();
            pnlActions.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.BackColor = Color.FromArgb(248, 250, 252);
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(headerLayout, 0, 0);
            mainLayout.Controls.Add(splitMain, 0, 1);
            mainLayout.Controls.Add(pnlActions, 0, 2);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(16);
            mainLayout.RowCount = 3;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 111F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
            mainLayout.Size = new Size(1029, 574);
            mainLayout.TabIndex = 0;
            // 
            // headerLayout
            // 
            headerLayout.ColumnCount = 3;
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 331F));
            headerLayout.Controls.Add(lblTitle, 0, 0);
            headerLayout.Controls.Add(lblSubtitle, 0, 1);
            headerLayout.Controls.Add(lblSearch, 0, 2);
            headerLayout.Controls.Add(txtSearch, 1, 2);
            headerLayout.Dock = DockStyle.Fill;
            headerLayout.Location = new Point(16, 16);
            headerLayout.Margin = new Padding(0, 0, 0, 12);
            headerLayout.Name = "headerLayout";
            headerLayout.RowCount = 3;
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            headerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            headerLayout.Size = new Size(997, 99);
            headerLayout.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            headerLayout.SetColumnSpan(lblTitle, 3);
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(31, 41, 55);
            lblTitle.Location = new Point(0, 0);
            lblTitle.Margin = new Padding(0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(997, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Select Primary Owner and Co-Owners";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            headerLayout.SetColumnSpan(lblSubtitle, 3);
            lblSubtitle.Dock = DockStyle.Fill;
            lblSubtitle.ForeColor = Color.FromArgb(89, 99, 110);
            lblSubtitle.Location = new Point(0, 30);
            lblSubtitle.Margin = new Padding(0);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(997, 26);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Choose exactly one primary owner and any number of co-owners.";
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSearch
            // 
            lblSearch.Dock = DockStyle.Fill;
            lblSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSearch.ForeColor = Color.FromArgb(55, 65, 81);
            lblSearch.Location = new Point(0, 56);
            lblSearch.Margin = new Padding(0);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(72, 43);
            lblSearch.TabIndex = 2;
            lblSearch.Text = "Search";
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtSearch
            // 
            txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.Location = new Point(72, 64);
            txtSearch.Margin = new Padding(0, 0, 16, 0);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Name, father/spouse, citizenship, address";
            txtSearch.Size = new Size(578, 27);
            txtSearch.TabIndex = 3;
            txtSearch.TextChanged += TxtSearch_TextChanged;
            // 
            // splitMain
            // 
            splitMain.BackColor = Color.FromArgb(226, 232, 240);
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel2;
            splitMain.Location = new Point(16, 127);
            splitMain.Margin = new Padding(0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(gridPanel);
            splitMain.Panel1MinSize = 560;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(cardPanel);
            splitMain.Panel2MinSize = 300;
            splitMain.Size = new Size(997, 369);
            splitMain.SplitterDistance = 651;
            splitMain.SplitterWidth = 8;
            splitMain.TabIndex = 1;
            // 
            // gridPanel
            // 
            gridPanel.BackColor = Color.White;
            gridPanel.Controls.Add(dgvOwners);
            gridPanel.Dock = DockStyle.Fill;
            gridPanel.Location = new Point(0, 0);
            gridPanel.Name = "gridPanel";
            gridPanel.Padding = new Padding(1);
            gridPanel.Size = new Size(651, 369);
            gridPanel.TabIndex = 0;
            // 
            // dgvOwners
            // 
            dgvOwners.AllowUserToAddRows = false;
            dgvOwners.AllowUserToDeleteRows = false;
            dgvOwners.AllowUserToResizeRows = false;
            dgvOwners.BackgroundColor = Color.White;
            dgvOwners.BorderStyle = BorderStyle.None;
            dgvOwners.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOwners.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(241, 245, 249);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(31, 41, 55);
            dataGridViewCellStyle1.SelectionBackColor = Color.FromArgb(241, 245, 249);
            dataGridViewCellStyle1.SelectionForeColor = Color.FromArgb(31, 41, 55);
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvOwners.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvOwners.ColumnHeadersHeight = 42;
            dgvOwners.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.White;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(31, 41, 55);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(17, 24, 39);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvOwners.DefaultCellStyle = dataGridViewCellStyle2;
            dgvOwners.Dock = DockStyle.Fill;
            dgvOwners.EnableHeadersVisualStyles = false;
            dgvOwners.GridColor = Color.FromArgb(226, 232, 240);
            dgvOwners.Location = new Point(1, 1);
            dgvOwners.MultiSelect = false;
            dgvOwners.Name = "dgvOwners";
            dgvOwners.RowHeadersVisible = false;
            dgvOwners.RowHeadersWidth = 51;
            dgvOwners.RowTemplate.Height = 32;
            dgvOwners.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOwners.Size = new Size(649, 367);
            dgvOwners.TabIndex = 0;
            dgvOwners.CellContentClick += DgvOwners_CellContentClick;
            dgvOwners.CellDoubleClick += DgvOwners_CellDoubleClick;
            dgvOwners.CellValueChanged += DgvOwners_CellValueChanged;
            dgvOwners.CurrentCellDirtyStateChanged += DgvOwners_CurrentCellDirtyStateChanged;
            dgvOwners.SelectionChanged += DgvOwners_SelectionChanged;
            // 
            // cardPanel
            // 
            cardPanel.BackColor = Color.White;
            cardPanel.Controls.Add(cardLayout);
            cardPanel.Dock = DockStyle.Fill;
            cardPanel.Location = new Point(0, 0);
            cardPanel.Name = "cardPanel";
            cardPanel.Padding = new Padding(18);
            cardPanel.Size = new Size(338, 369);
            cardPanel.TabIndex = 0;
            // 
            // cardLayout
            // 
            cardLayout.ColumnCount = 1;
            cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cardLayout.Controls.Add(lblPreviewTitle, 0, 0);
            cardLayout.Controls.Add(picOwner, 0, 1);
            cardLayout.Controls.Add(lblPrimaryCaption, 0, 2);
            cardLayout.Controls.Add(lblPrimaryValue, 0, 3);
            cardLayout.Controls.Add(lblFatherCaption, 0, 4);
            cardLayout.Controls.Add(lblFatherValue, 0, 5);
            cardLayout.Controls.Add(lblCitizenshipCaption, 0, 6);
            cardLayout.Controls.Add(lblCitizenshipValue, 0, 7);
            cardLayout.Controls.Add(lblAddressCaption, 0, 8);
            cardLayout.Controls.Add(lblAddressValue, 0, 9);
            cardLayout.Controls.Add(lblCoOwnerCount, 0, 10);
            cardLayout.Dock = DockStyle.Fill;
            cardLayout.Location = new Point(18, 18);
            cardLayout.Name = "cardLayout";
            cardLayout.RowCount = 11;
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 136F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            cardLayout.Size = new Size(302, 333);
            cardLayout.TabIndex = 0;
            // 
            // lblPreviewTitle
            // 
            lblPreviewTitle.Dock = DockStyle.Fill;
            lblPreviewTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPreviewTitle.ForeColor = Color.FromArgb(31, 41, 55);
            lblPreviewTitle.Location = new Point(0, 0);
            lblPreviewTitle.Margin = new Padding(0);
            lblPreviewTitle.Name = "lblPreviewTitle";
            lblPreviewTitle.Size = new Size(302, 34);
            lblPreviewTitle.TabIndex = 0;
            lblPreviewTitle.Text = "Primary Owner";
            lblPreviewTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // picOwner
            // 
            picOwner.Anchor = AnchorStyles.Top;
            picOwner.BackColor = Color.FromArgb(241, 245, 249);
            picOwner.BackgroundImage = Properties.Resources.Portrait_Placeholder1;
            picOwner.BackgroundImageLayout = ImageLayout.Zoom;
            picOwner.Location = new Point(91, 44);
            picOwner.Margin = new Padding(0, 10, 0, 12);
            picOwner.Name = "picOwner";
            picOwner.Size = new Size(120, 114);
            picOwner.SizeMode = PictureBoxSizeMode.Zoom;
            picOwner.TabIndex = 1;
            picOwner.TabStop = false;
            // 
            // lblPrimaryCaption
            // 
            lblPrimaryCaption.Dock = DockStyle.Fill;
            lblPrimaryCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPrimaryCaption.ForeColor = Color.FromArgb(55, 65, 81);
            lblPrimaryCaption.Location = new Point(0, 170);
            lblPrimaryCaption.Margin = new Padding(0);
            lblPrimaryCaption.Name = "lblPrimaryCaption";
            lblPrimaryCaption.Size = new Size(302, 27);
            lblPrimaryCaption.TabIndex = 2;
            lblPrimaryCaption.Text = "Owner Name";
            lblPrimaryCaption.TextAlign = ContentAlignment.BottomLeft;
            // 
            // lblPrimaryValue
            // 
            lblPrimaryValue.AutoEllipsis = true;
            lblPrimaryValue.Dock = DockStyle.Fill;
            lblPrimaryValue.Location = new Point(0, 197);
            lblPrimaryValue.Margin = new Padding(0);
            lblPrimaryValue.Name = "lblPrimaryValue";
            lblPrimaryValue.Size = new Size(302, 30);
            lblPrimaryValue.TabIndex = 3;
            lblPrimaryValue.Text = "-";
            lblPrimaryValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFatherCaption
            // 
            lblFatherCaption.Dock = DockStyle.Fill;
            lblFatherCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFatherCaption.ForeColor = Color.FromArgb(55, 65, 81);
            lblFatherCaption.Location = new Point(0, 227);
            lblFatherCaption.Margin = new Padding(0);
            lblFatherCaption.Name = "lblFatherCaption";
            lblFatherCaption.Size = new Size(302, 27);
            lblFatherCaption.TabIndex = 4;
            lblFatherCaption.Text = "Father/Spouse";
            lblFatherCaption.TextAlign = ContentAlignment.BottomLeft;
            // 
            // lblFatherValue
            // 
            lblFatherValue.AutoEllipsis = true;
            lblFatherValue.Dock = DockStyle.Fill;
            lblFatherValue.Location = new Point(0, 254);
            lblFatherValue.Margin = new Padding(0);
            lblFatherValue.Name = "lblFatherValue";
            lblFatherValue.Size = new Size(302, 30);
            lblFatherValue.TabIndex = 5;
            lblFatherValue.Text = "-";
            lblFatherValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCitizenshipCaption
            // 
            lblCitizenshipCaption.Dock = DockStyle.Fill;
            lblCitizenshipCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCitizenshipCaption.ForeColor = Color.FromArgb(55, 65, 81);
            lblCitizenshipCaption.Location = new Point(0, 284);
            lblCitizenshipCaption.Margin = new Padding(0);
            lblCitizenshipCaption.Name = "lblCitizenshipCaption";
            lblCitizenshipCaption.Size = new Size(302, 27);
            lblCitizenshipCaption.TabIndex = 6;
            lblCitizenshipCaption.Text = "Citizenship";
            lblCitizenshipCaption.TextAlign = ContentAlignment.BottomLeft;
            // 
            // lblCitizenshipValue
            // 
            lblCitizenshipValue.AutoEllipsis = true;
            lblCitizenshipValue.Dock = DockStyle.Fill;
            lblCitizenshipValue.Location = new Point(0, 311);
            lblCitizenshipValue.Margin = new Padding(0);
            lblCitizenshipValue.Name = "lblCitizenshipValue";
            lblCitizenshipValue.Size = new Size(302, 30);
            lblCitizenshipValue.TabIndex = 7;
            lblCitizenshipValue.Text = "-";
            lblCitizenshipValue.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAddressCaption
            // 
            lblAddressCaption.Dock = DockStyle.Fill;
            lblAddressCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblAddressCaption.ForeColor = Color.FromArgb(55, 65, 81);
            lblAddressCaption.Location = new Point(0, 341);
            lblAddressCaption.Margin = new Padding(0);
            lblAddressCaption.Name = "lblAddressCaption";
            lblAddressCaption.Size = new Size(302, 27);
            lblAddressCaption.TabIndex = 8;
            lblAddressCaption.Text = "Address";
            lblAddressCaption.TextAlign = ContentAlignment.BottomLeft;
            // 
            // lblAddressValue
            // 
            lblAddressValue.Dock = DockStyle.Fill;
            lblAddressValue.Location = new Point(0, 368);
            lblAddressValue.Margin = new Padding(0);
            lblAddressValue.Name = "lblAddressValue";
            lblAddressValue.Size = new Size(302, 1);
            lblAddressValue.TabIndex = 9;
            lblAddressValue.Text = "-";
            // 
            // lblCoOwnerCount
            // 
            lblCoOwnerCount.BackColor = Color.FromArgb(239, 246, 255);
            lblCoOwnerCount.Dock = DockStyle.Fill;
            lblCoOwnerCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCoOwnerCount.ForeColor = Color.FromArgb(30, 64, 175);
            lblCoOwnerCount.Location = new Point(0, 299);
            lblCoOwnerCount.Margin = new Padding(0, 8, 0, 0);
            lblCoOwnerCount.Name = "lblCoOwnerCount";
            lblCoOwnerCount.Padding = new Padding(10, 0, 10, 0);
            lblCoOwnerCount.Size = new Size(302, 34);
            lblCoOwnerCount.TabIndex = 10;
            lblCoOwnerCount.Text = "0 co-owner(s) selected";
            lblCoOwnerCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlActions
            // 
            pnlActions.Controls.Add(lblSelectionHint);
            pnlActions.Controls.Add(btnLoad);
            pnlActions.Controls.Add(btnCancel);
            pnlActions.Dock = DockStyle.Fill;
            pnlActions.Location = new Point(16, 496);
            pnlActions.Margin = new Padding(0);
            pnlActions.Name = "pnlActions";
            pnlActions.Size = new Size(997, 62);
            pnlActions.TabIndex = 2;
            // 
            // lblSelectionHint
            // 
            lblSelectionHint.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblSelectionHint.ForeColor = Color.FromArgb(89, 99, 110);
            lblSelectionHint.Location = new Point(3, 19);
            lblSelectionHint.Name = "lblSelectionHint";
            lblSelectionHint.Size = new Size(673, 24);
            lblSelectionHint.TabIndex = 0;
            lblSelectionHint.Text = "Set one Primary owner, then tick co-owners as needed.";
            lblSelectionHint.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoad.Enabled = false;
            btnLoad.Location = new Point(729, 19);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(132, 34);
            btnLoad.TabIndex = 1;
            btnLoad.Text = "Load Selection";
            btnLoad.UseVisualStyleBackColor = true;
            btnLoad.Click += BtnLoad_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Location = new Point(867, 19);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(128, 34);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // frmOwnerLookup
            // 
            AcceptButton = btnLoad;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(1029, 574);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(980, 600);
            Name = "frmOwnerLookup";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select Primary Owner and Co-Owners";
            mainLayout.ResumeLayout(false);
            headerLayout.ResumeLayout(false);
            headerLayout.PerformLayout();
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            gridPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvOwners).EndInit();
            cardPanel.ResumeLayout(false);
            cardLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picOwner).EndInit();
            pnlActions.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
