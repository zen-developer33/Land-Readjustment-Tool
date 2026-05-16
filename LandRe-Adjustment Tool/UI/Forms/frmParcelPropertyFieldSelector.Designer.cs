using System.Drawing;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmParcelPropertyFieldSelector
    {
        private System.ComponentModel.IContainer components = null;

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
            grpFields = new GroupBox();
            lblAvailable = new Label();
            lblSelected = new Label();
            lstAvailable = new ListBox();
            lstSelected = new ListBox();
            btnAdd = new Button();
            btnRemove = new Button();
            pnlButtons = new Panel();
            btnOk = new Button();
            btnCancel = new Button();
            grpFields.SuspendLayout();
            pnlButtons.SuspendLayout();
            SuspendLayout();
            // 
            // grpFields
            // 
            grpFields.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpFields.Controls.Add(lblAvailable);
            grpFields.Controls.Add(lblSelected);
            grpFields.Controls.Add(lstAvailable);
            grpFields.Controls.Add(lstSelected);
            grpFields.Controls.Add(btnAdd);
            grpFields.Controls.Add(btnRemove);
            grpFields.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grpFields.Location = new Point(12, 12);
            grpFields.Name = "grpFields";
            grpFields.Size = new Size(610, 306);
            grpFields.TabIndex = 0;
            grpFields.TabStop = false;
            grpFields.Text = "Choose Fields";
            // 
            // lblAvailable
            // 
            lblAvailable.AutoSize = true;
            lblAvailable.Font = new Font("Segoe UI", 9F);
            lblAvailable.Location = new Point(20, 35);
            lblAvailable.Name = "lblAvailable";
            lblAvailable.Size = new Size(108, 20);
            lblAvailable.TabIndex = 0;
            lblAvailable.Text = "Available fields";
            // 
            // lblSelected
            // 
            lblSelected.AutoSize = true;
            lblSelected.Font = new Font("Segoe UI", 9F);
            lblSelected.Location = new Point(374, 35);
            lblSelected.Name = "lblSelected";
            lblSelected.Size = new Size(107, 20);
            lblSelected.TabIndex = 3;
            lblSelected.Text = "Displayed fields";
            // 
            // lstAvailable
            // 
            lstAvailable.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstAvailable.Font = new Font("Segoe UI", 9F);
            lstAvailable.FormattingEnabled = true;
            lstAvailable.ItemHeight = 20;
            lstAvailable.Location = new Point(20, 61);
            lstAvailable.Name = "lstAvailable";
            lstAvailable.SelectionMode = SelectionMode.MultiExtended;
            lstAvailable.Size = new Size(240, 224);
            lstAvailable.TabIndex = 1;
            // 
            // lstSelected
            // 
            lstSelected.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstSelected.Font = new Font("Segoe UI", 9F);
            lstSelected.FormattingEnabled = true;
            lstSelected.ItemHeight = 20;
            lstSelected.Location = new Point(374, 61);
            lstSelected.Name = "lstSelected";
            lstSelected.SelectionMode = SelectionMode.MultiExtended;
            lstSelected.Size = new Size(216, 224);
            lstSelected.TabIndex = 4;
            // 
            // btnAdd
            // 
            btnAdd.Font = new Font("Segoe UI", 9F);
            btnAdd.Location = new Point(281, 121);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(72, 32);
            btnAdd.TabIndex = 2;
            btnAdd.Text = ">>";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // btnRemove
            // 
            btnRemove.Font = new Font("Segoe UI", 9F);
            btnRemove.Location = new Point(281, 165);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new Size(72, 32);
            btnRemove.TabIndex = 5;
            btnRemove.Text = "<<";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            // 
            // pnlButtons
            // 
            pnlButtons.Controls.Add(btnOk);
            pnlButtons.Controls.Add(btnCancel);
            pnlButtons.Dock = DockStyle.Bottom;
            pnlButtons.Location = new Point(0, 329);
            pnlButtons.Name = "pnlButtons";
            pnlButtons.Size = new Size(634, 54);
            pnlButtons.TabIndex = 1;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnOk.Location = new Point(415, 12);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(94, 29);
            btnOk.TabIndex = 0;
            btnOk.Text = "OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(528, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(94, 29);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // frmParcelPropertyFieldSelector
            // 
            AcceptButton = btnOk;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(634, 383);
            Controls.Add(grpFields);
            Controls.Add(pnlButtons);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmParcelPropertyFieldSelector";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Object Property Fields";
            grpFields.ResumeLayout(false);
            grpFields.PerformLayout();
            pnlButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private GroupBox grpFields;
        private Label lblAvailable;
        private Label lblSelected;
        private ListBox lstAvailable;
        private ListBox lstSelected;
        private Button btnAdd;
        private Button btnRemove;
        private Panel pnlButtons;
        private Button btnOk;
        private Button btnCancel;
    }
}
