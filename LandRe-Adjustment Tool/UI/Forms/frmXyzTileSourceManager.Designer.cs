namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmXyzTileSourceManager
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layout;
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
            layout = new TableLayoutPanel();
            dgvSources = new DataGridView();
            buttonLayout = new FlowLayoutPanel();
            btnAdd = new Button();
            btnDelete = new Button();
            btnSave = new Button();
            btnClose = new Button();
            layout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSources).BeginInit();
            buttonLayout.SuspendLayout();
            SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.Controls.Add(dgvSources, 0, 0);
            layout.Controls.Add(buttonLayout, 0, 1);
            layout.Dock = DockStyle.Fill;
            layout.Location = new Point(0, 0);
            layout.Margin = new Padding(3, 4, 3, 4);
            layout.Name = "layout";
            layout.Padding = new Padding(11, 13, 11, 13);
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 59F));
            layout.Size = new Size(810, 392);
            layout.TabIndex = 0;
            // 
            // dgvSources
            // 
            dgvSources.AllowUserToAddRows = false;
            dgvSources.AllowUserToDeleteRows = false;
            dgvSources.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSources.Dock = DockStyle.Fill;
            dgvSources.Location = new Point(14, 17);
            dgvSources.Margin = new Padding(3, 4, 3, 4);
            dgvSources.MultiSelect = false;
            dgvSources.Name = "dgvSources";
            dgvSources.RowHeadersVisible = false;
            dgvSources.RowHeadersWidth = 51;
            dgvSources.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSources.Size = new Size(782, 299);
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
            buttonLayout.Location = new Point(14, 324);
            buttonLayout.Margin = new Padding(3, 4, 3, 4);
            buttonLayout.Name = "buttonLayout";
            buttonLayout.Size = new Size(782, 51);
            buttonLayout.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(417, 4);
            btnAdd.Margin = new Padding(3, 4, 3, 4);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(86, 37);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(509, 4);
            btnDelete.Margin = new Padding(3, 4, 3, 4);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(86, 37);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(601, 4);
            btnSave.Margin = new Padding(3, 4, 3, 4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(86, 37);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnClose
            // 
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.Location = new Point(693, 4);
            btnClose.Margin = new Padding(3, 4, 3, 4);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(86, 37);
            btnClose.TabIndex = 3;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // frmXyzTileSourceManager
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(810, 392);
            Controls.Add(layout);
            Margin = new Padding(3, 4, 3, 4);
            MinimizeBox = false;
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
