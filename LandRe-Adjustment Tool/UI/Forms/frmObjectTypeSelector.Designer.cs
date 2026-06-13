using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmObjectTypeSelector
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainLayout;
        private Label lblInstruction;
        private TextBox txtFilter;
        private ListBox lstItems;
        private Panel buttonPanel;
        private Button btnSelect;
        private Button btnDeselect;
        private Button btnClose;
        private Label lblCount;

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
            lblInstruction = new Label();
            txtFilter = new TextBox();
            lstItems = new ListBox();
            buttonPanel = new Panel();
            btnSelect = new Button();
            btnDeselect = new Button();
            btnClose = new Button();
            lblCount = new Label();
            mainLayout.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayout
            // 
            mainLayout.ColumnCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.Controls.Add(lblInstruction, 0, 0);
            mainLayout.Controls.Add(txtFilter, 0, 1);
            mainLayout.Controls.Add(lstItems, 0, 2);
            mainLayout.Controls.Add(buttonPanel, 0, 3);
            mainLayout.Controls.Add(lblCount, 0, 4);
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.Location = new Point(0, 0);
            mainLayout.Name = "mainLayout";
            mainLayout.Padding = new Padding(10);
            mainLayout.RowCount = 5;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            mainLayout.Size = new Size(364, 459);
            mainLayout.TabIndex = 0;
            // 
            // lblInstruction
            // 
            lblInstruction.Dock = DockStyle.Fill;
            lblInstruction.Font = new Font("Segoe UI", 9F);
            lblInstruction.ForeColor = Color.FromArgb(71, 85, 105);
            lblInstruction.Location = new Point(10, 10);
            lblInstruction.Margin = new Padding(0);
            lblInstruction.Name = "lblInstruction";
            lblInstruction.Size = new Size(344, 38);
            lblInstruction.TabIndex = 0;
            lblInstruction.Text = "Select one or more items, then click Select or Deselect.";
            lblInstruction.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtFilter
            // 
            txtFilter.Dock = DockStyle.Fill;
            txtFilter.Location = new Point(10, 48);
            txtFilter.Margin = new Padding(0, 0, 0, 7);
            txtFilter.Name = "txtFilter";
            txtFilter.PlaceholderText = "Filter...";
            txtFilter.Size = new Size(344, 27);
            txtFilter.TabIndex = 1;
            // 
            // lstItems
            // 
            lstItems.Dock = DockStyle.Fill;
            lstItems.FormattingEnabled = true;
            lstItems.IntegralHeight = false;
            lstItems.Location = new Point(10, 82);
            lstItems.Margin = new Padding(0, 0, 0, 8);
            lstItems.Name = "lstItems";
            lstItems.SelectionMode = SelectionMode.MultiExtended;
            lstItems.Size = new Size(344, 289);
            lstItems.TabIndex = 2;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(btnSelect);
            buttonPanel.Controls.Add(btnDeselect);
            buttonPanel.Controls.Add(btnClose);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.Location = new Point(10, 379);
            buttonPanel.Margin = new Padding(0);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(344, 46);
            buttonPanel.TabIndex = 3;
            // 
            // btnSelect
            // 
            btnSelect.Location = new Point(0, 8);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new Size(84, 32);
            btnSelect.TabIndex = 0;
            btnSelect.Text = "Select";
            btnSelect.UseVisualStyleBackColor = true;
            // 
            // btnDeselect
            // 
            btnDeselect.Location = new Point(92, 8);
            btnDeselect.Name = "btnDeselect";
            btnDeselect.Size = new Size(84, 32);
            btnDeselect.TabIndex = 1;
            btnDeselect.Text = "Deselect";
            btnDeselect.UseVisualStyleBackColor = true;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Location = new Point(260, 8);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(84, 32);
            btnClose.TabIndex = 2;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            // 
            // lblCount
            // 
            lblCount.Dock = DockStyle.Fill;
            lblCount.ForeColor = Color.FromArgb(71, 85, 105);
            lblCount.Location = new Point(10, 425);
            lblCount.Margin = new Padding(0);
            lblCount.Name = "lblCount";
            lblCount.Size = new Size(344, 24);
            lblCount.TabIndex = 4;
            lblCount.Text = "0 of 0";
            lblCount.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // frmObjectTypeSelector
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(364, 459);
            Controls.Add(mainLayout);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.SizableToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new Size(280, 360);
            Name = "frmObjectTypeSelector";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Select Objects";
            mainLayout.ResumeLayout(false);
            mainLayout.PerformLayout();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
