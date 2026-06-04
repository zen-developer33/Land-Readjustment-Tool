namespace Land_Readjustment_Tool.UI.Forms
{
    partial class frmPointMarkerPicker
    {
        private System.ComponentModel.IContainer components = null;
        private FlowLayoutPanel _markerLayout;
        private FlowLayoutPanel buttonPanel;
        private Button _okButton;
        private Button _cancelButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _markerLayout = new FlowLayoutPanel();
            buttonPanel = new FlowLayoutPanel();
            _okButton = new Button();
            _cancelButton = new Button();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // _markerLayout
            // 
            _markerLayout.AutoScroll = true;
            _markerLayout.Dock = DockStyle.Fill;
            _markerLayout.FlowDirection = FlowDirection.LeftToRight;
            _markerLayout.Location = new Point(0, 0);
            _markerLayout.Name = "_markerLayout";
            _markerLayout.Padding = new Padding(10);
            _markerLayout.Size = new Size(520, 310);
            _markerLayout.TabIndex = 0;
            _markerLayout.WrapContents = true;
            // 
            // buttonPanel
            // 
            buttonPanel.BackColor = SystemColors.Control;
            buttonPanel.Controls.Add(_okButton);
            buttonPanel.Controls.Add(_cancelButton);
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(0, 310);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(10);
            buttonPanel.Size = new Size(520, 50);
            buttonPanel.TabIndex = 1;
            // 
            // _okButton
            // 
            _okButton.DialogResult = DialogResult.OK;
            _okButton.Location = new Point(423, 13);
            _okButton.Name = "_okButton";
            _okButton.Size = new Size(84, 29);
            _okButton.TabIndex = 0;
            _okButton.Text = "OK";
            _okButton.UseVisualStyleBackColor = true;
            // 
            // _cancelButton
            // 
            _cancelButton.DialogResult = DialogResult.Cancel;
            _cancelButton.Location = new Point(333, 13);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new Size(84, 29);
            _cancelButton.TabIndex = 1;
            _cancelButton.Text = "Cancel";
            _cancelButton.UseVisualStyleBackColor = true;
            // 
            // frmPointMarkerPicker
            // 
            AcceptButton = _okButton;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = _cancelButton;
            ClientSize = new Size(520, 360);
            Controls.Add(_markerLayout);
            Controls.Add(buttonPanel);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmPointMarkerPicker";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Choose Point Marker";
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
