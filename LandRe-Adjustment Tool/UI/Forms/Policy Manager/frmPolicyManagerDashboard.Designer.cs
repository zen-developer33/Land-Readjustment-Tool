namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPolicyManagerDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private ToolStrip toolStrip;
        private ToolStripLabel lblPolicySelector;
        private ToolStripComboBox cboPolicies;
        private ToolStripButton btnNewPolicy;
        private ToolStripButton btnSaveDraft;
        private ToolStripButton btnNewDraft;
        private ToolStripButton btnApprove;
        private ToolStripButton btnLockUnlock;
        private ToolStripButton btnImport;
        private ToolStripButton btnExport;
        private ToolStripControlHost statusHost;
        private Label lblStatus;
        private SplitContainer outerSplit;
        private DataGridView dgvClauses;
        private DataGridViewTextBoxColumn colClauseCode;
        private DataGridViewTextBoxColumn colClauseSection;
        private DataGridViewTextBoxColumn colClauseHeading;
        private SplitContainer rightSplit;
        private SplitContainer detailSplit;
        private TableLayoutPanel clauseEditorLayout;
        private TextBox txtPolicyCode;
        private TextBox txtPolicyName;
        private TextBox txtClauseCode;
        private TextBox txtClauseHeading;
        private TextBox txtClauseSection;
        private TextBox txtClauseDescription;
        private FlowLayoutPanel clauseButtonPanel;
        private Button btnAddClause;
        private Button btnAddSubClause;
        private Button btnDuplicateClause;
        private Button btnDeleteClause;
        private Button btnMoveUp;
        private Button btnMoveDown;
        private Panel attachmentPanel;
        private PictureBox pictureBox;
        private FlowLayoutPanel attachmentButtonPanel;
        private Button btnAttachImage;
        private TabControl tabDetails;
        private TabPage tabParameters;
        private DataGridView dgvParameters;
        private DataGridViewTextBoxColumn colParameterKey;
        private DataGridViewTextBoxColumn colParameterLabel;
        private DataGridViewTextBoxColumn colParameterType;
        private DataGridViewTextBoxColumn colParameterValue;
        private DataGridViewTextBoxColumn colParameterUnit;
        private DataGridViewTextBoxColumn colParameterDescription;
        private FlowLayoutPanel parameterButtonPanel;
        private Button btnAddParameter;
        private Button btnDeleteParameter;
        private TabPage tabLookupTables;
        private ComboBox cboLookupTables;
        private DataGridView dgvLookup;
        private TabPage tabAudit;
        private DataGridView dgvAudit;
        private DataGridViewTextBoxColumn colAuditDate;
        private DataGridViewTextBoxColumn colAuditAction;
        private DataGridViewTextBoxColumn colAuditDetails;
        private TabPage tabValidation;
        private ListBox lstValidation;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            toolStrip = new ToolStrip();
            lblPolicySelector = new ToolStripLabel();
            cboPolicies = new ToolStripComboBox();
            btnNewPolicy = new ToolStripButton();
            btnSaveDraft = new ToolStripButton();
            btnNewDraft = new ToolStripButton();
            btnApprove = new ToolStripButton();
            btnLockUnlock = new ToolStripButton();
            btnImport = new ToolStripButton();
            btnExport = new ToolStripButton();
            lblStatus = new Label();
            statusHost = new ToolStripControlHost(lblStatus);
            outerSplit = new SplitContainer();
            dgvClauses = new DataGridView();
            colClauseCode = new DataGridViewTextBoxColumn();
            colClauseSection = new DataGridViewTextBoxColumn();
            colClauseHeading = new DataGridViewTextBoxColumn();
            rightSplit = new SplitContainer();
            detailSplit = new SplitContainer();
            clauseEditorLayout = new TableLayoutPanel();
            txtPolicyCode = new TextBox();
            txtPolicyName = new TextBox();
            txtClauseCode = new TextBox();
            txtClauseHeading = new TextBox();
            txtClauseSection = new TextBox();
            txtClauseDescription = new TextBox();
            clauseButtonPanel = new FlowLayoutPanel();
            btnAddClause = new Button();
            btnAddSubClause = new Button();
            btnDuplicateClause = new Button();
            btnDeleteClause = new Button();
            btnMoveUp = new Button();
            btnMoveDown = new Button();
            attachmentPanel = new Panel();
            pictureBox = new PictureBox();
            attachmentButtonPanel = new FlowLayoutPanel();
            btnAttachImage = new Button();
            tabDetails = new TabControl();
            tabParameters = new TabPage();
            dgvParameters = new DataGridView();
            colParameterKey = new DataGridViewTextBoxColumn();
            colParameterLabel = new DataGridViewTextBoxColumn();
            colParameterType = new DataGridViewTextBoxColumn();
            colParameterValue = new DataGridViewTextBoxColumn();
            colParameterUnit = new DataGridViewTextBoxColumn();
            colParameterDescription = new DataGridViewTextBoxColumn();
            parameterButtonPanel = new FlowLayoutPanel();
            btnAddParameter = new Button();
            btnDeleteParameter = new Button();
            tabLookupTables = new TabPage();
            cboLookupTables = new ComboBox();
            dgvLookup = new DataGridView();
            tabAudit = new TabPage();
            dgvAudit = new DataGridView();
            colAuditDate = new DataGridViewTextBoxColumn();
            colAuditAction = new DataGridViewTextBoxColumn();
            colAuditDetails = new DataGridViewTextBoxColumn();
            tabValidation = new TabPage();
            lstValidation = new ListBox();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)outerSplit).BeginInit();
            outerSplit.Panel1.SuspendLayout();
            outerSplit.Panel2.SuspendLayout();
            outerSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvClauses).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightSplit).BeginInit();
            rightSplit.Panel1.SuspendLayout();
            rightSplit.Panel2.SuspendLayout();
            rightSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)detailSplit).BeginInit();
            detailSplit.Panel1.SuspendLayout();
            detailSplit.Panel2.SuspendLayout();
            detailSplit.SuspendLayout();
            clauseEditorLayout.SuspendLayout();
            clauseButtonPanel.SuspendLayout();
            attachmentPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            attachmentButtonPanel.SuspendLayout();
            tabDetails.SuspendLayout();
            tabParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvParameters).BeginInit();
            parameterButtonPanel.SuspendLayout();
            tabLookupTables.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvLookup).BeginInit();
            tabAudit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvAudit).BeginInit();
            tabValidation.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip
            // 
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.ImageScalingSize = new Size(20, 20);
            toolStrip.Items.AddRange(new ToolStripItem[] { lblPolicySelector, cboPolicies, new ToolStripSeparator(), btnNewPolicy, btnSaveDraft, btnNewDraft, btnApprove, btnLockUnlock, new ToolStripSeparator(), btnImport, btnExport, new ToolStripSeparator(), statusHost });
            toolStrip.Location = new Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(1180, 28);
            toolStrip.TabIndex = 0;
            // 
            // lblPolicySelector
            // 
            lblPolicySelector.Name = "lblPolicySelector";
            lblPolicySelector.Size = new Size(52, 25);
            lblPolicySelector.Text = "Policy:";
            // 
            // cboPolicies
            // 
            cboPolicies.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPolicies.Name = "cboPolicies";
            cboPolicies.Size = new Size(360, 28);
            cboPolicies.SelectedIndexChanged += cboPolicies_SelectedIndexChanged;
            // 
            // btnNewPolicy
            // 
            btnNewPolicy.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnNewPolicy.Name = "btnNewPolicy";
            btnNewPolicy.Size = new Size(88, 25);
            btnNewPolicy.Text = "New Policy";
            btnNewPolicy.Click += btnNewPolicy_Click;
            // 
            // btnSaveDraft
            // 
            btnSaveDraft.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnSaveDraft.Name = "btnSaveDraft";
            btnSaveDraft.Size = new Size(82, 25);
            btnSaveDraft.Text = "Save Draft";
            btnSaveDraft.Click += btnSaveDraft_Click;
            // 
            // btnNewDraft
            // 
            btnNewDraft.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnNewDraft.Name = "btnNewDraft";
            btnNewDraft.Size = new Size(183, 25);
            btnNewDraft.Text = "New Draft From Approved";
            btnNewDraft.Click += btnNewDraft_Click;
            // 
            // btnApprove
            // 
            btnApprove.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnApprove.Name = "btnApprove";
            btnApprove.Size = new Size(70, 25);
            btnApprove.Text = "Approve";
            btnApprove.Click += btnApprove_Click;
            // 
            // btnLockUnlock
            // 
            btnLockUnlock.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLockUnlock.Name = "btnLockUnlock";
            btnLockUnlock.Size = new Size(99, 25);
            btnLockUnlock.Text = "Lock Editing";
            btnLockUnlock.Click += btnLockUnlock_Click;
            // 
            // btnImport
            // 
            btnImport.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnImport.Name = "btnImport";
            btnImport.Size = new Size(61, 25);
            btnImport.Text = "Import";
            btnImport.Click += btnImport_Click;
            // 
            // btnExport
            // 
            btnExport.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(58, 25);
            btnExport.Text = "Export";
            btnExport.Click += btnExport_Click;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = false;
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(220, 23);
            lblStatus.Text = "Draft";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // statusHost
            // 
            statusHost.Name = "statusHost";
            statusHost.Size = new Size(220, 25);
            // 
            // outerSplit
            // 
            outerSplit.Dock = DockStyle.Fill;
            outerSplit.Location = new Point(0, 28);
            outerSplit.Name = "outerSplit";
            // 
            // outerSplit.Panel1
            // 
            outerSplit.Panel1.Controls.Add(dgvClauses);
            // 
            // outerSplit.Panel2
            // 
            outerSplit.Panel2.Controls.Add(rightSplit);
            outerSplit.Size = new Size(1180, 642);
            outerSplit.SplitterDistance = 360;
            outerSplit.TabIndex = 1;
            // 
            // dgvClauses
            // 
            dgvClauses.AllowUserToAddRows = false;
            dgvClauses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvClauses.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClauses.Columns.AddRange(new DataGridViewColumn[] { colClauseCode, colClauseSection, colClauseHeading });
            dgvClauses.Dock = DockStyle.Fill;
            dgvClauses.MultiSelect = false;
            dgvClauses.Name = "dgvClauses";
            dgvClauses.ReadOnly = true;
            dgvClauses.RowHeadersWidth = 35;
            dgvClauses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvClauses.Size = new Size(360, 642);
            dgvClauses.TabIndex = 0;
            dgvClauses.SelectionChanged += dgvClauses_SelectionChanged;
            // 
            // colClauseCode
            // 
            colClauseCode.HeaderText = "Code";
            colClauseCode.MinimumWidth = 6;
            colClauseCode.Name = "colClauseCode";
            colClauseCode.ReadOnly = true;
            colClauseCode.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClauseCode.Width = 78;
            // 
            // colClauseSection
            // 
            colClauseSection.HeaderText = "Section";
            colClauseSection.MinimumWidth = 6;
            colClauseSection.Name = "colClauseSection";
            colClauseSection.ReadOnly = true;
            colClauseSection.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClauseSection.Width = 130;
            // 
            // colClauseHeading
            // 
            colClauseHeading.HeaderText = "Heading";
            colClauseHeading.MinimumWidth = 6;
            colClauseHeading.Name = "colClauseHeading";
            colClauseHeading.ReadOnly = true;
            colClauseHeading.SortMode = DataGridViewColumnSortMode.NotSortable;
            colClauseHeading.Width = 310;
            // 
            // rightSplit
            // 
            rightSplit.Dock = DockStyle.Fill;
            rightSplit.Location = new Point(0, 0);
            rightSplit.Name = "rightSplit";
            rightSplit.Orientation = Orientation.Horizontal;
            rightSplit.Panel2Collapsed = true;
            // 
            // rightSplit.Panel1
            // 
            rightSplit.Panel1.Controls.Add(detailSplit);
            // 
            // rightSplit.Panel2
            // 
            rightSplit.Panel2.Controls.Add(tabDetails);
            rightSplit.Size = new Size(816, 642);
            rightSplit.SplitterDistance = 310;
            rightSplit.TabIndex = 0;
            // 
            // detailSplit
            // 
            detailSplit.Dock = DockStyle.Fill;
            detailSplit.Location = new Point(0, 0);
            detailSplit.Name = "detailSplit";
            // 
            // detailSplit.Panel1
            // 
            detailSplit.Panel1.Controls.Add(clauseEditorLayout);
            // 
            // detailSplit.Panel2
            // 
            detailSplit.Panel2.Controls.Add(attachmentPanel);
            detailSplit.Size = new Size(816, 310);
            detailSplit.SplitterDistance = 610;
            detailSplit.TabIndex = 0;
            // 
            // clauseEditorLayout
            // 
            clauseEditorLayout.ColumnCount = 4;
            clauseEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            clauseEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            clauseEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            clauseEditorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            clauseEditorLayout.Controls.Add(new Label { Text = "Policy Code:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 0);
            clauseEditorLayout.Controls.Add(txtPolicyCode, 1, 0);
            clauseEditorLayout.Controls.Add(new Label { Text = "Policy Name:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 2, 0);
            clauseEditorLayout.Controls.Add(txtPolicyName, 3, 0);
            clauseEditorLayout.Controls.Add(new Label { Text = "Clause Code:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 1);
            clauseEditorLayout.Controls.Add(txtClauseCode, 1, 1);
            clauseEditorLayout.Controls.Add(new Label { Text = "Section:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 2, 1);
            clauseEditorLayout.Controls.Add(txtClauseSection, 3, 1);
            clauseEditorLayout.Controls.Add(new Label { Text = "Heading:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 2);
            clauseEditorLayout.Controls.Add(txtClauseHeading, 1, 2);
            clauseEditorLayout.Controls.Add(new Label { Text = "Description:", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, 3);
            clauseEditorLayout.Controls.Add(txtClauseDescription, 1, 3);
            clauseEditorLayout.Controls.Add(clauseButtonPanel, 0, 6);
            clauseEditorLayout.Dock = DockStyle.Fill;
            clauseEditorLayout.Location = new Point(0, 0);
            clauseEditorLayout.Name = "clauseEditorLayout";
            clauseEditorLayout.Padding = new Padding(8);
            clauseEditorLayout.RowCount = 7;
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0F));
            clauseEditorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            clauseEditorLayout.SetColumnSpan(txtClauseHeading, 3);
            clauseEditorLayout.SetColumnSpan(txtClauseDescription, 3);
            clauseEditorLayout.SetColumnSpan(clauseButtonPanel, 4);
            clauseEditorLayout.Size = new Size(610, 310);
            clauseEditorLayout.TabIndex = 0;
            // 
            // text boxes
            // 
            txtPolicyCode.Dock = DockStyle.Fill;
            txtPolicyName.Dock = DockStyle.Fill;
            txtClauseCode.Dock = DockStyle.Fill;
            txtClauseHeading.Dock = DockStyle.Fill;
            txtClauseSection.Dock = DockStyle.Fill;
            txtClauseDescription.Dock = DockStyle.Fill;
            txtClauseDescription.Multiline = true;
            txtClauseDescription.ScrollBars = ScrollBars.Vertical;
            // 
            // clauseButtonPanel
            // 
            clauseButtonPanel.Controls.Add(btnAddClause);
            clauseButtonPanel.Controls.Add(btnAddSubClause);
            clauseButtonPanel.Controls.Add(btnDuplicateClause);
            clauseButtonPanel.Controls.Add(btnDeleteClause);
            clauseButtonPanel.Controls.Add(btnMoveUp);
            clauseButtonPanel.Controls.Add(btnMoveDown);
            clauseButtonPanel.Dock = DockStyle.Fill;
            clauseButtonPanel.Location = new Point(11, 270);
            clauseButtonPanel.Name = "clauseButtonPanel";
            clauseButtonPanel.Size = new Size(588, 29);
            clauseButtonPanel.TabIndex = 10;
            // 
            // clause buttons
            // 
            btnAddClause.AutoSize = true;
            btnAddClause.Text = "Add Clause";
            btnAddClause.Click += btnAddClause_Click;
            btnAddSubClause.AutoSize = true;
            btnAddSubClause.Text = "Add Sub-Clause";
            btnAddSubClause.Click += btnAddSubClause_Click;
            btnDuplicateClause.AutoSize = true;
            btnDuplicateClause.Text = "Duplicate";
            btnDuplicateClause.Click += btnDuplicateClause_Click;
            btnDeleteClause.AutoSize = true;
            btnDeleteClause.Text = "Delete";
            btnDeleteClause.Click += btnDeleteClause_Click;
            btnMoveUp.AutoSize = true;
            btnMoveUp.Text = "Move Up";
            btnMoveUp.Click += btnMoveUp_Click;
            btnMoveDown.AutoSize = true;
            btnMoveDown.Text = "Move Down";
            btnMoveDown.Click += btnMoveDown_Click;
            // 
            // attachmentPanel
            // 
            attachmentPanel.Controls.Add(pictureBox);
            attachmentPanel.Controls.Add(attachmentButtonPanel);
            attachmentPanel.Dock = DockStyle.Fill;
            attachmentPanel.Padding = new Padding(8);
            // 
            // pictureBox
            // 
            pictureBox.BackColor = Color.White;
            pictureBox.BorderStyle = BorderStyle.FixedSingle;
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            // 
            // attachmentButtonPanel
            // 
            attachmentButtonPanel.Controls.Add(btnAttachImage);
            attachmentButtonPanel.Dock = DockStyle.Bottom;
            attachmentButtonPanel.Height = 38;
            // 
            // btnAttachImage
            // 
            btnAttachImage.AutoSize = true;
            btnAttachImage.Text = "Attach Image";
            btnAttachImage.Click += btnAttachImage_Click;
            // 
            // tabDetails
            // 
            tabDetails.Controls.Add(tabParameters);
            tabDetails.Controls.Add(tabLookupTables);
            tabDetails.Controls.Add(tabAudit);
            tabDetails.Controls.Add(tabValidation);
            tabDetails.Dock = DockStyle.Fill;
            // 
            // tabParameters
            // 
            tabParameters.Controls.Add(dgvParameters);
            tabParameters.Controls.Add(parameterButtonPanel);
            tabParameters.Text = "Parameters";
            // 
            // dgvParameters
            // 
            dgvParameters.AllowUserToAddRows = false;
            dgvParameters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvParameters.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvParameters.Columns.AddRange(new DataGridViewColumn[] { colParameterKey, colParameterLabel, colParameterType, colParameterValue, colParameterUnit, colParameterDescription });
            dgvParameters.Dock = DockStyle.Fill;
            dgvParameters.RowHeadersWidth = 35;
            dgvParameters.CellEndEdit += dgvParameters_CellEndEdit;
            // 
            // parameter columns
            // 
            colParameterKey.HeaderText = "Key";
            colParameterKey.Name = "colParameterKey";
            colParameterKey.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterKey.Width = 180;
            colParameterLabel.HeaderText = "Label";
            colParameterLabel.Name = "colParameterLabel";
            colParameterLabel.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterLabel.Width = 220;
            colParameterType.HeaderText = "Type";
            colParameterType.Name = "colParameterType";
            colParameterType.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterType.Width = 90;
            colParameterValue.HeaderText = "Value";
            colParameterValue.Name = "colParameterValue";
            colParameterValue.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterValue.Width = 120;
            colParameterUnit.HeaderText = "Unit";
            colParameterUnit.Name = "colParameterUnit";
            colParameterUnit.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterUnit.Width = 75;
            colParameterDescription.HeaderText = "Description";
            colParameterDescription.Name = "colParameterDescription";
            colParameterDescription.SortMode = DataGridViewColumnSortMode.NotSortable;
            colParameterDescription.Width = 420;
            // 
            // parameterButtonPanel
            // 
            parameterButtonPanel.Controls.Add(btnAddParameter);
            parameterButtonPanel.Controls.Add(btnDeleteParameter);
            parameterButtonPanel.Dock = DockStyle.Bottom;
            parameterButtonPanel.Height = 38;
            // 
            // parameter buttons
            // 
            btnAddParameter.AutoSize = true;
            btnAddParameter.Text = "Add Parameter";
            btnAddParameter.Click += btnAddParameter_Click;
            btnDeleteParameter.AutoSize = true;
            btnDeleteParameter.Text = "Delete Parameter";
            btnDeleteParameter.Click += btnDeleteParameter_Click;
            // 
            // tabLookupTables
            // 
            tabLookupTables.Controls.Add(dgvLookup);
            tabLookupTables.Controls.Add(cboLookupTables);
            tabLookupTables.Text = "Lookup Tables";
            // 
            // cboLookupTables
            // 
            cboLookupTables.Dock = DockStyle.Top;
            cboLookupTables.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLookupTables.SelectedIndexChanged += cboLookupTables_SelectedIndexChanged;
            // 
            // dgvLookup
            // 
            dgvLookup.AllowUserToAddRows = false;
            dgvLookup.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvLookup.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLookup.Dock = DockStyle.Fill;
            dgvLookup.RowHeadersWidth = 35;
            dgvLookup.CellEndEdit += dgvLookup_CellEndEdit;
            // 
            // tabAudit
            // 
            tabAudit.Controls.Add(dgvAudit);
            tabAudit.Text = "Audit";
            // 
            // dgvAudit
            // 
            dgvAudit.AllowUserToAddRows = false;
            dgvAudit.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvAudit.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvAudit.Columns.AddRange(new DataGridViewColumn[] { colAuditDate, colAuditAction, colAuditDetails });
            dgvAudit.Dock = DockStyle.Fill;
            dgvAudit.ReadOnly = true;
            dgvAudit.RowHeadersWidth = 35;
            colAuditDate.HeaderText = "Date";
            colAuditDate.Name = "colAuditDate";
            colAuditDate.Width = 150;
            colAuditAction.HeaderText = "Action";
            colAuditAction.Name = "colAuditAction";
            colAuditAction.Width = 130;
            colAuditDetails.HeaderText = "Details";
            colAuditDetails.Name = "colAuditDetails";
            colAuditDetails.Width = 520;
            // 
            // tabValidation
            // 
            tabValidation.Controls.Add(lstValidation);
            tabValidation.Text = "Validation";
            // 
            // lstValidation
            // 
            lstValidation.Dock = DockStyle.Fill;
            // 
            // frmPolicyManagerDashboard
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(1180, 670);
            Controls.Add(outerSplit);
            Controls.Add(toolStrip);
            MinimumSize = new Size(1000, 650);
            Name = "frmPolicyManagerDashboard";
            Text = "Policy Dashboard";
            Load += frmPolicyManagerDashboard_Load;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            outerSplit.Panel1.ResumeLayout(false);
            outerSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)outerSplit).EndInit();
            outerSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvClauses).EndInit();
            rightSplit.Panel1.ResumeLayout(false);
            rightSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rightSplit).EndInit();
            rightSplit.ResumeLayout(false);
            detailSplit.Panel1.ResumeLayout(false);
            detailSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)detailSplit).EndInit();
            detailSplit.ResumeLayout(false);
            clauseEditorLayout.ResumeLayout(false);
            clauseEditorLayout.PerformLayout();
            clauseButtonPanel.ResumeLayout(false);
            clauseButtonPanel.PerformLayout();
            attachmentPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            attachmentButtonPanel.ResumeLayout(false);
            attachmentButtonPanel.PerformLayout();
            tabDetails.ResumeLayout(false);
            tabParameters.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvParameters).EndInit();
            parameterButtonPanel.ResumeLayout(false);
            parameterButtonPanel.PerformLayout();
            tabLookupTables.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvLookup).EndInit();
            tabAudit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvAudit).EndInit();
            tabValidation.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
