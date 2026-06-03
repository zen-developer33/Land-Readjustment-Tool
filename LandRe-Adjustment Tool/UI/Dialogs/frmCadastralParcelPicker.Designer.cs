namespace Land_Readjustment_Tool.UI.Dialogs
{
    partial class frmCadastralParcelPicker
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblMapSheet;
        private ComboBox _cboMapSheet;
        private Label lblSearch;
        private TextBox _txtSearch;
        private DataGridView _dgvParcels;
        private Label _lblStatus;
        private FlowLayoutPanel buttonPanel;
        private Button _btnAssign;
        private Button _btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            colMapSheet = new DataGridViewTextBoxColumn();
            colParcel = new DataGridViewTextBoxColumn();
            colArea = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            mainLayout = new TableLayoutPanel();
            lblMapSheet = new Label();
            _cboMapSheet = new ComboBox();
            lblSearch = new Label();
            _txtSearch = new TextBox();
            _dgvParcels = new DataGridView();
            _lblStatus = new Label();
            buttonPanel = new FlowLayoutPanel();
            _btnAssign = new Button();
            _btnCancel = new Button();
            mainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvParcels).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // colMapSheet
            // 
            colMapSheet.HeaderText = "MapSheetNo";
            colMapSheet.MinimumWidth = 6;
            colMapSheet.Name = "colMapSheet";
            colMapSheet.ReadOnly = true;
            colMapSheet.Width = 125;
            // 
            // colParcel
            // 
            colParcel.HeaderText = "ParcelNo";
            colParcel.MinimumWidth = 6;
            colParcel.Name = "colParcel";
            colParcel.ReadOnly = true;
            colParcel.Width = 97;
            // 
            // colArea
            // 
            colArea.HeaderText = "Area sq.m";
            colArea.MinimumWidth = 6;
            colArea.Name = "colArea";
            colArea.ReadOnly = true;
            colArea.Width = 96;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "Current map assignment";
            colStatus.MinimumWidth = 150;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.Width = 182;
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 2;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblMapSheet, 0, 0);
            mainLayout.Controls.Add(_cboMapSheet, 1, 0);
            mainLayout.Controls.Add(lblSearch, 0, 1);
            mainLayout.Controls.Add(_txtSearch, 1, 1);
            mainLayout.Controls.Add(_dgvParcels, 0, 2);
            mainLayout.Controls.Add(_lblStatus, 0, 3);
            mainLayout.Controls.Add(buttonPanel, 1, 4);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(14);
            mainLayout.RowCount = 5;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            mainLayout.Size = new Size(560, 420);
            mainLayout.TabIndex = 0;
            // 
            // lblMapSheet
            // 
            lblMapSheet.Dock = DockStyle.Fill;
            lblMapSheet.Location = new Point(17, 14);
            lblMapSheet.Name = "lblMapSheet";
            lblMapSheet.Size = new Size(89, 38);
            lblMapSheet.TabIndex = 0;
            lblMapSheet.Text = "Map sheet";
            lblMapSheet.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _cboMapSheet
            // 
            _cboMapSheet.Dock = DockStyle.Fill;
            _cboMapSheet.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboMapSheet.FormattingEnabled = true;
            _cboMapSheet.Location = new Point(112, 17);
            _cboMapSheet.Name = "_cboMapSheet";
            _cboMapSheet.Size = new Size(431, 28);
            _cboMapSheet.TabIndex = 0;
            // 
            // lblSearch
            // 
            lblSearch.Dock = DockStyle.Fill;
            lblSearch.Location = new Point(17, 52);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(89, 38);
            lblSearch.TabIndex = 1;
            lblSearch.Text = "Search";
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _txtSearch
            // 
            _txtSearch.Dock = DockStyle.Fill;
            _txtSearch.Location = new Point(112, 55);
            _txtSearch.Name = "_txtSearch";
            _txtSearch.Size = new Size(431, 27);
            _txtSearch.TabIndex = 1;
            // 
            // _dgvParcels
            // 
            _dgvParcels.AllowUserToAddRows = false;
            _dgvParcels.AllowUserToDeleteRows = false;
            _dgvParcels.AllowUserToResizeRows = false;
            _dgvParcels.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            _dgvParcels.BackgroundColor = SystemColors.Window;
            _dgvParcels.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            _dgvParcels.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            _dgvParcels.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            _dgvParcels.Columns.AddRange(new DataGridViewColumn[] { colMapSheet, colParcel, colArea, colStatus });
            mainLayout.SetColumnSpan(_dgvParcels, 2);
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            _dgvParcels.DefaultCellStyle = dataGridViewCellStyle2;
            _dgvParcels.Dock = DockStyle.Fill;
            _dgvParcels.Location = new Point(17, 93);
            _dgvParcels.MultiSelect = false;
            _dgvParcels.Name = "_dgvParcels";
            _dgvParcels.ReadOnly = true;
            _dgvParcels.RowHeadersVisible = false;
            _dgvParcels.RowHeadersWidth = 51;
            _dgvParcels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _dgvParcels.Size = new Size(526, 230);
            _dgvParcels.TabIndex = 2;
            // 
            // _lblStatus
            // 
            mainLayout.SetColumnSpan(_lblStatus, 2);
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.ForeColor = Color.DimGray;
            _lblStatus.Location = new Point(17, 326);
            _lblStatus.Name = "_lblStatus";
            _lblStatus.Size = new Size(526, 34);
            _lblStatus.TabIndex = 3;
            _lblStatus.Text = "Loading parcel records...";
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(_btnAssign);
            buttonPanel.Controls.Add(_btnCancel);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(112, 363);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(431, 40);
            buttonPanel.TabIndex = 3;
            // 
            // _btnAssign
            // 
            _btnAssign.Enabled = false;
            _btnAssign.Location = new Point(338, 3);
            _btnAssign.Name = "_btnAssign";
            _btnAssign.Size = new Size(90, 32);
            _btnAssign.TabIndex = 0;
            _btnAssign.Text = "Assign";
            _btnAssign.UseVisualStyleBackColor = true;
            // 
            // _btnCancel
            // 
            _btnCancel.DialogResult = DialogResult.Cancel;
            _btnCancel.Location = new Point(242, 3);
            _btnCancel.Name = "_btnCancel";
            _btnCancel.Size = new Size(90, 32);
            _btnCancel.TabIndex = 1;
            _btnCancel.Text = "Cancel";
            _btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmCadastralParcelPicker
            // 
            AcceptButton = _btnAssign;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _btnCancel;
            ClientSize = new Size(560, 420);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCadastralParcelPicker";
            StartPosition = FormStartPosition.Manual;
            Text = "Assign Parcel Record";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_dgvParcels).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        private DataGridViewTextBoxColumn colMapSheet;
        private DataGridViewTextBoxColumn colParcel;
        private DataGridViewTextBoxColumn colArea;
        private DataGridViewTextBoxColumn colStatus;
    }
}
