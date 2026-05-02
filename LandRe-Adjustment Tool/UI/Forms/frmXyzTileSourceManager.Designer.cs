namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmXyzTileSourceManager
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layout;
        private Label lblLegend;
        private FlowLayoutPanel buttonLayout;
        private DataGridView dgvSources;
        private Button btnAdd;
        private Button btnDelete;
        private Button btnSave;
        private Button btnClose;
        private DataGridViewTextBoxColumn colName;
        private DataGridViewTextBoxColumn colUrlTemplate;
        private DataGridViewTextBoxColumn colMinZoom;
        private DataGridViewTextBoxColumn colMaxZoom;
        private DataGridViewComboBoxColumn colImageExtension;
        private DataGridViewCheckBoxColumn colIsBuiltIn;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            colName = new DataGridViewTextBoxColumn();
            colUrlTemplate = new DataGridViewTextBoxColumn();
            colMinZoom = new DataGridViewTextBoxColumn();
            colMaxZoom = new DataGridViewTextBoxColumn();
            colImageExtension = new DataGridViewComboBoxColumn();
            colIsBuiltIn = new DataGridViewCheckBoxColumn();
            layout = new TableLayoutPanel();
            lblLegend = new Label();
            dgvSources = new DataGridView();
            buttonLayout = new FlowLayoutPanel();
            btnClose = new Button();
            btnSave = new Button();
            btnDelete = new Button();
            btnAdd = new Button();
            layout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSources).BeginInit();
            buttonLayout.SuspendLayout();
            SuspendLayout();
            //
            // colName
            //
            colName.DataPropertyName = "Name";
            colName.HeaderText = "Name";
            colName.MinimumWidth = 160;
            colName.Name = "colName";
            colName.Width = 200;
            //
            // colUrlTemplate
            //
            colUrlTemplate.DataPropertyName = "UrlTemplate";
            colUrlTemplate.HeaderText = "URL Template";
            colUrlTemplate.MinimumWidth = 260;
            colUrlTemplate.Name = "colUrlTemplate";
            colUrlTemplate.Width = 340;
            //
            // colMinZoom
            //
            colMinZoom.DataPropertyName = "MinZoom";
            colMinZoom.HeaderText = "Min Zoom";
            colMinZoom.MinimumWidth = 60;
            colMinZoom.Name = "colMinZoom";
            colMinZoom.Width = 70;
            //
            // colMaxZoom
            //
            colMaxZoom.DataPropertyName = "MaxZoom";
            colMaxZoom.HeaderText = "Max Zoom";
            colMaxZoom.MinimumWidth = 60;
            colMaxZoom.Name = "colMaxZoom";
            colMaxZoom.Width = 70;
            //
            // colImageExtension
            //
            colImageExtension.DataPropertyName = "ImageExtension";
            colImageExtension.FlatStyle = FlatStyle.Flat;
            colImageExtension.HeaderText = "Format";
            colImageExtension.Items.AddRange(new object[] { "png", "jpg" });
            colImageExtension.MinimumWidth = 60;
            colImageExtension.Name = "colImageExtension";
            colImageExtension.Width = 70;
            //
            // colIsBuiltIn  — hidden; controls row behaviour via code
            //
            colIsBuiltIn.DataPropertyName = "IsBuiltIn";
            colIsBuiltIn.HeaderText = "Built-In";
            colIsBuiltIn.Name = "colIsBuiltIn";
            colIsBuiltIn.ReadOnly = true;
            colIsBuiltIn.Visible = false;
            //
            // layout
            //
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(lblLegend, 0, 0);
            layout.Controls.Add(dgvSources, 0, 1);
            layout.Controls.Add(buttonLayout, 0, 2);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Margin = new Padding(3, 4, 3, 4);
            layout.Name = "layout";
            layout.Padding = new Padding(11, 8, 11, 8);
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
            layout.Size = new Size(800, 480);
            layout.TabIndex = 0;
            //
            // lblLegend
            //
            lblLegend.AutoSize = false;
            lblLegend.BackColor = Color.FromArgb(235, 243, 255);
            lblLegend.BorderStyle = BorderStyle.FixedSingle;
            lblLegend.Dock = DockStyle.Fill;
            lblLegend.ForeColor = Color.FromArgb(50, 80, 130);
            lblLegend.Location = new Point(14, 8);
            lblLegend.Margin = new Padding(3, 0, 3, 4);
            lblLegend.Name = "lblLegend";
            lblLegend.Padding = new Padding(4, 0, 0, 0);
            lblLegend.Size = new Size(772, 28);
            lblLegend.TabIndex = 2;
            lblLegend.Text =
                "Highlighted rows are built-in sources — they are read-only and cannot be edited or deleted.";
            lblLegend.TextAlign = ContentAlignment.MiddleLeft;
            //
            // dgvSources
            //
            dgvSources.AllowUserToAddRows = false;
            dgvSources.AllowUserToDeleteRows = false;
            dgvSources.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvSources.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSources.Columns.AddRange(new DataGridViewColumn[]
            {
                colName,
                colUrlTemplate,
                colMinZoom,
                colMaxZoom,
                colImageExtension,
                colIsBuiltIn
            });
            dgvSources.Dock = DockStyle.Fill;
            dgvSources.Location = new Point(14, 44);
            dgvSources.Margin = new Padding(3, 4, 3, 4);
            dgvSources.MultiSelect = false;
            dgvSources.Name = "dgvSources";
            dgvSources.RowHeadersVisible = false;
            dgvSources.RowHeadersWidth = 51;
            dgvSources.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSources.Size = new Size(772, 369);
            dgvSources.TabIndex = 0;
            //
            // buttonLayout
            //
            buttonLayout.Controls.Add(btnClose);
            buttonLayout.Controls.Add(btnSave);
            buttonLayout.Controls.Add(btnDelete);
            buttonLayout.Controls.Add(btnAdd);
            buttonLayout.Dock = DockStyle.Fill;
            buttonLayout.FlowDirection = FlowDirection.RightToLeft;
            buttonLayout.Location = new Point(14, 417);
            buttonLayout.Margin = new Padding(3, 4, 3, 4);
            buttonLayout.Name = "buttonLayout";
            buttonLayout.Size = new Size(772, 51);
            buttonLayout.TabIndex = 1;
            //
            // btnClose
            //
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(683, 4);
            btnClose.Margin = new Padding(3, 4, 3, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(86, 37);
            btnClose.TabIndex = 3;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            //
            // btnSave
            //
            btnSave.Location = new Point(591, 4);
            btnSave.Margin = new Padding(3, 4, 3, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(86, 37);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            //
            // btnDelete
            //
            btnDelete.Location = new Point(499, 4);
            btnDelete.Margin = new Padding(3, 4, 3, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(86, 37);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            //
            // btnAdd
            //
            btnAdd.Location = new Point(407, 4);
            btnAdd.Margin = new Padding(3, 4, 3, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(86, 37);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            //
            // frmXyzTileSourceManager
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(800, 480);
            Controls.Add(layout);
            Margin = new Padding(3, 4, 3, 4);
            MinimizeBox = false;
            MinimumSize = new Size(720, 400);
            Name = "frmXyzTileSourceManager";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "XYZ Tile Sources";
            layout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSources).EndInit();
            buttonLayout.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
