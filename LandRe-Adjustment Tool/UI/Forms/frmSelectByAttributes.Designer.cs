using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmSelectByAttributes
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel mainLayout;

        // ── Top: layer + method ─────────────────────
        private TableLayoutPanel topLayout;
        private Label lblLayer;
        private ComboBox cboLayer;
        private CheckBox chkOnlySelectableLayers;
        private Label lblMethod;
        private ComboBox cboMethod;

        // ── Expression ──────────────────────────────
        private Label lblExpression;
        private TextBox txtExpression;
        private Panel statusPanel;
        private CheckBox chkZoomToSelection;
        private Label lblApplyStatus;

        // ── Bottom buttons ──────────────────────────
        private Panel buttonPanel;
        private Button btnClear;
        private Button btnVerify;
        private Button btnLoad;
        private Button btnSave;
        private FlowLayoutPanel rightButtons;
        private Button btnOk;
        private Button btnApply;
        private Button btnClose;

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
            mainLayout = new TableLayoutPanel();
            topLayout = new TableLayoutPanel();
            lblLayer = new Label();
            cboLayer = new ComboBox();
            chkOnlySelectableLayers = new CheckBox();
            lblMethod = new Label();
            cboMethod = new ComboBox();
            middleLayout = new TableLayoutPanel();
            lblFields = new Label();
            lblValues = new Label();
            lstFields = new ListBox();
            operatorLayout = new TableLayoutPanel();
            btnOpEq = new Button();
            btnOpNeq = new Button();
            btnOpLike = new Button();
            btnOpGt = new Button();
            btnOpGte = new Button();
            btnOpAnd = new Button();
            btnOpLt = new Button();
            btnOpLte = new Button();
            btnOpOr = new Button();
            btnOpPercent = new Button();
            btnOpUnderscore = new Button();
            btnOpParens = new Button();
            btnOpIs = new Button();
            btnOpIn = new Button();
            btnOpNot = new Button();
            valuesLayout = new TableLayoutPanel();
            lstValues = new ListBox();
            valueToolsPanel = new Panel();
            btnGetUniqueValues = new Button();
            lblGoTo = new Label();
            txtGoTo = new TextBox();
            lblExpression = new Label();
            txtExpression = new TextBox();
            statusPanel = new Panel();
            chkZoomToSelection = new CheckBox();
            lblApplyStatus = new Label();
            buttonPanel = new Panel();
            btnClear = new Button();
            btnVerify = new Button();
            btnLoad = new Button();
            btnSave = new Button();
            rightButtons = new FlowLayoutPanel();
            btnClose = new Button();
            btnApply = new Button();
            btnOk = new Button();
            mainLayout.SuspendLayout();
            topLayout.SuspendLayout();
            middleLayout.SuspendLayout();
            operatorLayout.SuspendLayout();
            valuesLayout.SuspendLayout();
            valueToolsPanel.SuspendLayout();
            statusPanel.SuspendLayout();
            buttonPanel.SuspendLayout();
            rightButtons.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(topLayout, 0, 0);
            mainLayout.Controls.Add(middleLayout, 1, 1);
            mainLayout.Controls.Add(lblExpression, 0, 3);
            mainLayout.Controls.Add(txtExpression, 0, 4);
            mainLayout.Controls.Add(statusPanel, 0, 5);
            mainLayout.Controls.Add(buttonPanel, 0, 6);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(12);
            mainLayout.RowCount = 7;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 86F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            mainLayout.Size = new Size(856, 543);
            mainLayout.TabIndex = 0;
            // 
            // topLayout
            // 
            topLayout.ColumnCount = 3;
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            topLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topLayout.ColumnStyles.Add(new ColumnStyle());
            topLayout.Controls.Add(lblLayer, 0, 0);
            topLayout.Controls.Add(cboLayer, 1, 0);
            topLayout.Controls.Add(chkOnlySelectableLayers, 2, 0);
            topLayout.Controls.Add(lblMethod, 0, 1);
            topLayout.Controls.Add(cboMethod, 1, 1);
            topLayout.Dock = DockStyle.Fill;
            topLayout.Location = new Point(12, 12);
            topLayout.Margin = new Padding(0);
            topLayout.Name = "topLayout";
            topLayout.RowCount = 2;
            topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            topLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            topLayout.Size = new Size(832, 78);
            topLayout.TabIndex = 0;
            // 
            // lblLayer
            // 
            lblLayer.Anchor = AnchorStyles.Left;
            lblLayer.AutoSize = true;
            lblLayer.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblLayer.Location = new Point(3, 9);
            lblLayer.Name = "lblLayer";
            lblLayer.Size = new Size(52, 20);
            lblLayer.TabIndex = 0;
            lblLayer.Text = "Layer:";
            // 
            // cboLayer
            // 
            cboLayer.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cboLayer.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLayer.FormattingEnabled = true;
            cboLayer.Location = new Point(118, 5);
            cboLayer.Name = "cboLayer";
            cboLayer.Size = new Size(486, 28);
            cboLayer.TabIndex = 1;
            // 
            // chkOnlySelectableLayers
            // 
            chkOnlySelectableLayers.Anchor = AnchorStyles.Left;
            chkOnlySelectableLayers.AutoSize = true;
            chkOnlySelectableLayers.Checked = true;
            chkOnlySelectableLayers.CheckState = CheckState.Checked;
            chkOnlySelectableLayers.Location = new Point(617, 7);
            chkOnlySelectableLayers.Margin = new Padding(10, 3, 3, 3);
            chkOnlySelectableLayers.Name = "chkOnlySelectableLayers";
            chkOnlySelectableLayers.Size = new Size(212, 24);
            chkOnlySelectableLayers.TabIndex = 2;
            chkOnlySelectableLayers.Text = "Only show selectable layers";
            chkOnlySelectableLayers.UseVisualStyleBackColor = true;
            // 
            // lblMethod
            // 
            lblMethod.Anchor = AnchorStyles.Left;
            lblMethod.AutoSize = true;
            lblMethod.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblMethod.Location = new Point(3, 48);
            lblMethod.Name = "lblMethod";
            lblMethod.Size = new Size(68, 20);
            lblMethod.TabIndex = 3;
            lblMethod.Text = "Method:";
            // 
            // cboMethod
            // 
            cboMethod.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            cboMethod.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMethod.FormattingEnabled = true;
            cboMethod.Items.AddRange(new object[] { "Create a new selection", "Add to current selection", "Remove from current selection", "Select from current selection", "Switch selection", "Clear selection" });
            cboMethod.Location = new Point(118, 44);
            cboMethod.Name = "cboMethod";
            cboMethod.Size = new Size(486, 28);
            cboMethod.TabIndex = 4;
            // 
            // middleLayout
            // 
            middleLayout.AutoSize = true;
            middleLayout.ColumnCount = 3;
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 162F));
            middleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            middleLayout.Controls.Add(lblFields, 0, 0);
            middleLayout.Controls.Add(lblValues, 2, 0);
            middleLayout.Controls.Add(lstFields, 0, 1);
            middleLayout.Controls.Add(operatorLayout, 1, 1);
            middleLayout.Controls.Add(valuesLayout, 2, 1);
            middleLayout.Dock = DockStyle.Fill;
            middleLayout.Location = new Point(12, 90);
            middleLayout.Margin = new Padding(0);
            middleLayout.Name = "middleLayout";
            middleLayout.RowCount = 2;
            middleLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29F));
            middleLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            middleLayout.Size = new Size(832, 223);
            middleLayout.TabIndex = 1;
            // 
            // lblFields
            // 
            lblFields.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblFields.AutoSize = true;
            lblFields.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFields.Location = new Point(0, 7);
            lblFields.Margin = new Padding(0, 0, 0, 2);
            lblFields.Name = "lblFields";
            lblFields.Size = new Size(53, 20);
            lblFields.TabIndex = 0;
            lblFields.Text = "Fields:";
            // 
            // lblValues
            // 
            lblValues.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblValues.AutoSize = true;
            lblValues.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblValues.Location = new Point(497, 7);
            lblValues.Margin = new Padding(0, 0, 0, 2);
            lblValues.Name = "lblValues";
            lblValues.Size = new Size(111, 20);
            lblValues.TabIndex = 1;
            lblValues.Text = "Unique values:";
            // 
            // lstFields
            // 
            lstFields.Dock = DockStyle.Fill;
            lstFields.FormattingEnabled = true;
            lstFields.IntegralHeight = false;
            lstFields.Location = new Point(0, 29);
            lstFields.Margin = new Padding(0, 0, 8, 0);
            lstFields.Name = "lstFields";
            lstFields.Size = new Size(327, 194);
            lstFields.TabIndex = 2;
            // 
            // operatorLayout
            // 
            operatorLayout.ColumnCount = 3;
            operatorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            operatorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            operatorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            operatorLayout.Controls.Add(btnOpEq, 0, 0);
            operatorLayout.Controls.Add(btnOpNeq, 1, 0);
            operatorLayout.Controls.Add(btnOpLike, 2, 0);
            operatorLayout.Controls.Add(btnOpGt, 0, 1);
            operatorLayout.Controls.Add(btnOpGte, 1, 1);
            operatorLayout.Controls.Add(btnOpAnd, 2, 1);
            operatorLayout.Controls.Add(btnOpLt, 0, 2);
            operatorLayout.Controls.Add(btnOpLte, 1, 2);
            operatorLayout.Controls.Add(btnOpOr, 2, 2);
            operatorLayout.Controls.Add(btnOpPercent, 0, 3);
            operatorLayout.Controls.Add(btnOpUnderscore, 1, 3);
            operatorLayout.Controls.Add(btnOpParens, 2, 3);
            operatorLayout.Controls.Add(btnOpIs, 0, 4);
            operatorLayout.Controls.Add(btnOpIn, 1, 4);
            operatorLayout.Controls.Add(btnOpNot, 2, 4);
            operatorLayout.Dock = DockStyle.Top;
            operatorLayout.Location = new Point(335, 29);
            operatorLayout.Margin = new Padding(0, 0, 8, 0);
            operatorLayout.Name = "operatorLayout";
            operatorLayout.RowCount = 5;
            operatorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            operatorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            operatorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            operatorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            operatorLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            operatorLayout.Size = new Size(154, 160);
            operatorLayout.TabIndex = 3;
            // 
            // btnOpEq
            // 
            btnOpEq.Dock = DockStyle.Fill;
            btnOpEq.Location = new Point(1, 1);
            btnOpEq.Margin = new Padding(1);
            btnOpEq.Name = "btnOpEq";
            btnOpEq.Size = new Size(49, 30);
            btnOpEq.TabIndex = 0;
            btnOpEq.Text = "=";
            btnOpEq.UseVisualStyleBackColor = true;
            // 
            // btnOpNeq
            // 
            btnOpNeq.Dock = DockStyle.Fill;
            btnOpNeq.Location = new Point(52, 1);
            btnOpNeq.Margin = new Padding(1);
            btnOpNeq.Name = "btnOpNeq";
            btnOpNeq.Size = new Size(49, 30);
            btnOpNeq.TabIndex = 1;
            btnOpNeq.Text = "<>";
            btnOpNeq.UseVisualStyleBackColor = true;
            // 
            // btnOpLike
            // 
            btnOpLike.Dock = DockStyle.Fill;
            btnOpLike.Location = new Point(103, 1);
            btnOpLike.Margin = new Padding(1);
            btnOpLike.Name = "btnOpLike";
            btnOpLike.Size = new Size(50, 30);
            btnOpLike.TabIndex = 2;
            btnOpLike.Text = "Like";
            btnOpLike.UseVisualStyleBackColor = true;
            // 
            // btnOpGt
            // 
            btnOpGt.Dock = DockStyle.Fill;
            btnOpGt.Location = new Point(1, 33);
            btnOpGt.Margin = new Padding(1);
            btnOpGt.Name = "btnOpGt";
            btnOpGt.Size = new Size(49, 30);
            btnOpGt.TabIndex = 3;
            btnOpGt.Text = ">";
            btnOpGt.UseVisualStyleBackColor = true;
            // 
            // btnOpGte
            // 
            btnOpGte.Dock = DockStyle.Fill;
            btnOpGte.Location = new Point(52, 33);
            btnOpGte.Margin = new Padding(1);
            btnOpGte.Name = "btnOpGte";
            btnOpGte.Size = new Size(49, 30);
            btnOpGte.TabIndex = 4;
            btnOpGte.Text = ">=";
            btnOpGte.UseVisualStyleBackColor = true;
            // 
            // btnOpAnd
            // 
            btnOpAnd.Dock = DockStyle.Fill;
            btnOpAnd.Location = new Point(103, 33);
            btnOpAnd.Margin = new Padding(1);
            btnOpAnd.Name = "btnOpAnd";
            btnOpAnd.Size = new Size(50, 30);
            btnOpAnd.TabIndex = 5;
            btnOpAnd.Text = "And";
            btnOpAnd.UseVisualStyleBackColor = true;
            // 
            // btnOpLt
            // 
            btnOpLt.Dock = DockStyle.Fill;
            btnOpLt.Location = new Point(1, 65);
            btnOpLt.Margin = new Padding(1);
            btnOpLt.Name = "btnOpLt";
            btnOpLt.Size = new Size(49, 30);
            btnOpLt.TabIndex = 6;
            btnOpLt.Text = "<";
            btnOpLt.UseVisualStyleBackColor = true;
            // 
            // btnOpLte
            // 
            btnOpLte.Dock = DockStyle.Fill;
            btnOpLte.Location = new Point(52, 65);
            btnOpLte.Margin = new Padding(1);
            btnOpLte.Name = "btnOpLte";
            btnOpLte.Size = new Size(49, 30);
            btnOpLte.TabIndex = 7;
            btnOpLte.Text = "<=";
            btnOpLte.UseVisualStyleBackColor = true;
            // 
            // btnOpOr
            // 
            btnOpOr.Dock = DockStyle.Fill;
            btnOpOr.Location = new Point(103, 65);
            btnOpOr.Margin = new Padding(1);
            btnOpOr.Name = "btnOpOr";
            btnOpOr.Size = new Size(50, 30);
            btnOpOr.TabIndex = 8;
            btnOpOr.Text = "Or";
            btnOpOr.UseVisualStyleBackColor = true;
            // 
            // btnOpPercent
            // 
            btnOpPercent.Dock = DockStyle.Fill;
            btnOpPercent.Location = new Point(1, 97);
            btnOpPercent.Margin = new Padding(1);
            btnOpPercent.Name = "btnOpPercent";
            btnOpPercent.Size = new Size(49, 30);
            btnOpPercent.TabIndex = 9;
            btnOpPercent.Text = "%";
            btnOpPercent.UseVisualStyleBackColor = true;
            // 
            // btnOpUnderscore
            // 
            btnOpUnderscore.Dock = DockStyle.Fill;
            btnOpUnderscore.Location = new Point(52, 97);
            btnOpUnderscore.Margin = new Padding(1);
            btnOpUnderscore.Name = "btnOpUnderscore";
            btnOpUnderscore.Size = new Size(49, 30);
            btnOpUnderscore.TabIndex = 10;
            btnOpUnderscore.Text = "_";
            btnOpUnderscore.UseVisualStyleBackColor = true;
            // 
            // btnOpParens
            // 
            btnOpParens.Dock = DockStyle.Fill;
            btnOpParens.Location = new Point(103, 97);
            btnOpParens.Margin = new Padding(1);
            btnOpParens.Name = "btnOpParens";
            btnOpParens.Size = new Size(50, 30);
            btnOpParens.TabIndex = 11;
            btnOpParens.Text = "( )";
            btnOpParens.UseVisualStyleBackColor = true;
            // 
            // btnOpIs
            // 
            btnOpIs.Dock = DockStyle.Fill;
            btnOpIs.Location = new Point(1, 129);
            btnOpIs.Margin = new Padding(1);
            btnOpIs.Name = "btnOpIs";
            btnOpIs.Size = new Size(49, 30);
            btnOpIs.TabIndex = 12;
            btnOpIs.Text = "Is";
            btnOpIs.UseVisualStyleBackColor = true;
            // 
            // btnOpIn
            // 
            btnOpIn.Dock = DockStyle.Fill;
            btnOpIn.Location = new Point(52, 129);
            btnOpIn.Margin = new Padding(1);
            btnOpIn.Name = "btnOpIn";
            btnOpIn.Size = new Size(49, 30);
            btnOpIn.TabIndex = 13;
            btnOpIn.Text = "In";
            btnOpIn.UseVisualStyleBackColor = true;
            // 
            // btnOpNot
            // 
            btnOpNot.Dock = DockStyle.Fill;
            btnOpNot.Location = new Point(103, 129);
            btnOpNot.Margin = new Padding(1);
            btnOpNot.Name = "btnOpNot";
            btnOpNot.Size = new Size(50, 30);
            btnOpNot.TabIndex = 14;
            btnOpNot.Text = "Not";
            btnOpNot.UseVisualStyleBackColor = true;
            // 
            // valuesLayout
            // 
            valuesLayout.ColumnCount = 1;
            valuesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            valuesLayout.Controls.Add(lstValues, 0, 0);
            valuesLayout.Controls.Add(valueToolsPanel, 0, 1);
            valuesLayout.Dock = DockStyle.Fill;
            valuesLayout.Location = new Point(497, 29);
            valuesLayout.Margin = new Padding(0);
            valuesLayout.Name = "valuesLayout";
            valuesLayout.RowCount = 2;
            valuesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            valuesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            valuesLayout.Size = new Size(335, 194);
            valuesLayout.TabIndex = 4;
            // 
            // lstValues
            // 
            lstValues.Dock = DockStyle.Fill;
            lstValues.FormattingEnabled = true;
            lstValues.IntegralHeight = false;
            lstValues.Location = new Point(0, 0);
            lstValues.Margin = new Padding(0, 0, 0, 6);
            lstValues.Name = "lstValues";
            lstValues.Size = new Size(335, 150);
            lstValues.TabIndex = 0;
            // 
            // valueToolsPanel
            // 
            valueToolsPanel.Controls.Add(btnGetUniqueValues);
            valueToolsPanel.Controls.Add(lblGoTo);
            valueToolsPanel.Controls.Add(txtGoTo);
            valueToolsPanel.Dock = DockStyle.Fill;
            valueToolsPanel.Location = new Point(0, 156);
            valueToolsPanel.Margin = new Padding(0);
            valueToolsPanel.Name = "valueToolsPanel";
            valueToolsPanel.Size = new Size(335, 38);
            valueToolsPanel.TabIndex = 1;
            // 
            // btnGetUniqueValues
            // 
            btnGetUniqueValues.Location = new Point(0, 4);
            btnGetUniqueValues.Name = "btnGetUniqueValues";
            btnGetUniqueValues.Size = new Size(146, 30);
            btnGetUniqueValues.TabIndex = 0;
            btnGetUniqueValues.Text = "Get Unique Values";
            btnGetUniqueValues.UseVisualStyleBackColor = true;
            // 
            // lblGoTo
            // 
            lblGoTo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblGoTo.AutoSize = true;
            lblGoTo.Location = new Point(200, 10);
            lblGoTo.Name = "lblGoTo";
            lblGoTo.Size = new Size(51, 20);
            lblGoTo.TabIndex = 1;
            lblGoTo.Text = "Go To:";
            // 
            // txtGoTo
            // 
            txtGoTo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtGoTo.Location = new Point(257, 7);
            txtGoTo.Name = "txtGoTo";
            txtGoTo.Size = new Size(78, 27);
            txtGoTo.TabIndex = 2;
            // 
            // lblExpression
            // 
            lblExpression.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblExpression.AutoSize = true;
            lblExpression.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblExpression.Location = new Point(12, 337);
            lblExpression.Margin = new Padding(0, 4, 0, 2);
            lblExpression.Name = "lblExpression";
            lblExpression.Size = new Size(237, 20);
            lblExpression.TabIndex = 2;
            lblExpression.Text = "SELECT * FROM <layer> WHERE:";
            // 
            // txtExpression
            // 
            txtExpression.Dock = DockStyle.Fill;
            txtExpression.Font = new Font("Consolas", 10F);
            txtExpression.Location = new Point(12, 359);
            txtExpression.Margin = new Padding(0, 0, 0, 8);
            txtExpression.Multiline = true;
            txtExpression.Name = "txtExpression";
            txtExpression.ScrollBars = ScrollBars.Vertical;
            txtExpression.Size = new Size(832, 78);
            txtExpression.TabIndex = 3;
            // 
            // statusPanel
            // 
            statusPanel.Controls.Add(chkZoomToSelection);
            statusPanel.Controls.Add(lblApplyStatus);
            statusPanel.Dock = DockStyle.Fill;
            statusPanel.Location = new Point(12, 445);
            statusPanel.Margin = new Padding(0);
            statusPanel.Name = "statusPanel";
            statusPanel.Size = new Size(832, 34);
            statusPanel.TabIndex = 4;
            // 
            // chkZoomToSelection
            // 
            chkZoomToSelection.Anchor = AnchorStyles.Left;
            chkZoomToSelection.AutoSize = true;
            chkZoomToSelection.Checked = true;
            chkZoomToSelection.CheckState = CheckState.Checked;
            chkZoomToSelection.Location = new Point(0, 5);
            chkZoomToSelection.Name = "chkZoomToSelection";
            chkZoomToSelection.Size = new Size(152, 24);
            chkZoomToSelection.TabIndex = 0;
            chkZoomToSelection.Text = "Zoom to selection";
            chkZoomToSelection.UseVisualStyleBackColor = true;
            // 
            // lblApplyStatus
            // 
            lblApplyStatus.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lblApplyStatus.AutoEllipsis = true;
            lblApplyStatus.ForeColor = SystemColors.GrayText;
            lblApplyStatus.Location = new Point(170, 7);
            lblApplyStatus.Name = "lblApplyStatus";
            lblApplyStatus.Size = new Size(662, 20);
            lblApplyStatus.TabIndex = 1;
            lblApplyStatus.Text = "Ready.";
            lblApplyStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnClear);
            buttonPanel.Controls.Add(btnVerify);
            buttonPanel.Controls.Add(btnLoad);
            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(rightButtons);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Location = new Point(12, 479);
            buttonPanel.Margin = new Padding(0);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(832, 52);
            buttonPanel.TabIndex = 5;
            // 
            // btnClear
            // 
            btnClear.Location = new Point(0, 10);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(78, 32);
            btnClear.TabIndex = 0;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnVerify
            // 
            btnVerify.Location = new Point(84, 10);
            btnVerify.Name = "btnVerify";
            btnVerify.Size = new Size(78, 32);
            btnVerify.TabIndex = 1;
            btnVerify.Text = "Verify";
            btnVerify.UseVisualStyleBackColor = true;
            // 
            // btnLoad
            // 
            btnLoad.Location = new Point(168, 10);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(78, 32);
            btnLoad.TabIndex = 2;
            btnLoad.Text = "Load...";
            btnLoad.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(252, 10);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(78, 32);
            btnSave.TabIndex = 3;
            btnSave.Text = "Save...";
            btnSave.UseVisualStyleBackColor = true;
            // 
            // rightButtons
            // 
            rightButtons.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            rightButtons.Controls.Add(btnClose);
            rightButtons.Controls.Add(btnApply);
            rightButtons.Controls.Add(btnOk);
            rightButtons.FlowDirection = FlowDirection.RightToLeft;
            rightButtons.Location = new Point(550, 8);
            rightButtons.Name = "rightButtons";
            rightButtons.Size = new Size(282, 40);
            rightButtons.TabIndex = 4;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(198, 2);
            btnClose.Margin = new Padding(8, 2, 0, 2);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(84, 32);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            btnApply.Location = new Point(106, 2);
            btnApply.Margin = new Padding(8, 2, 0, 2);
            btnApply.Name = "btnApply";
            btnApply.Size = new Size(84, 32);
            btnApply.TabIndex = 1;
            btnApply.Text = "Apply";
            btnApply.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            btnOk.Location = new Point(14, 2);
            btnOk.Margin = new Padding(8, 2, 0, 2);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(84, 32);
            btnOk.TabIndex = 0;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            // 
            // frmSelectByAttributes
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(856, 543);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(620, 590);
            Name = "frmSelectByAttributes";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Select By Attributes";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            topLayout.ResumeLayout(false);
            topLayout.PerformLayout();
            middleLayout.ResumeLayout(false);
            middleLayout.PerformLayout();
            operatorLayout.ResumeLayout(false);
            valuesLayout.ResumeLayout(false);
            valueToolsPanel.ResumeLayout(false);
            valueToolsPanel.PerformLayout();
            statusPanel.ResumeLayout(false);
            statusPanel.PerformLayout();
            buttonPanel.ResumeLayout(false);
            rightButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private TableLayoutPanel middleLayout;
        private Label lblFields;
        private Label lblValues;
        private ListBox lstFields;
        private TableLayoutPanel operatorLayout;
        private Button btnOpEq;
        private Button btnOpNeq;
        private Button btnOpLike;
        private Button btnOpGt;
        private Button btnOpGte;
        private Button btnOpAnd;
        private Button btnOpLt;
        private Button btnOpLte;
        private Button btnOpOr;
        private Button btnOpPercent;
        private Button btnOpUnderscore;
        private Button btnOpParens;
        private Button btnOpIs;
        private Button btnOpIn;
        private Button btnOpNot;
        private TableLayoutPanel valuesLayout;
        private ListBox lstValues;
        private Panel valueToolsPanel;
        private Button btnGetUniqueValues;
        private Label lblGoTo;
        private TextBox txtGoTo;
    }
}
