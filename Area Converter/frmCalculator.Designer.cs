namespace Land_Readjustment_Tool
{
    partial class frmCalculator
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblDisplay;

        private System.Windows.Forms.Button btn0;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Button btn3;
        private System.Windows.Forms.Button btn4;
        private System.Windows.Forms.Button btn5;
        private System.Windows.Forms.Button btn6;
        private System.Windows.Forms.Button btn7;
        private System.Windows.Forms.Button btn8;
        private System.Windows.Forms.Button btn9;
        private System.Windows.Forms.Button btnDot;
        private System.Windows.Forms.Button btnSign;

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnSub;
        private System.Windows.Forms.Button btnMul;
        private System.Windows.Forms.Button btnDiv;
        private System.Windows.Forms.Button btnEquals;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnBackspace;

        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblDisplay = new Label();
            ResultTextRightClickContext = new ContextMenuStrip(components);
            copyToolStripMenuItem = new ToolStripMenuItem();
            pasteToolStripMenuItem = new ToolStripMenuItem();
            btnClear = new Button();
            btnBackspace = new Button();
            btnSign = new Button();
            btnDiv = new Button();
            btn7 = new Button();
            btn8 = new Button();
            btn9 = new Button();
            btnMul = new Button();
            btn4 = new Button();
            btn5 = new Button();
            btn6 = new Button();
            btnSub = new Button();
            btn1 = new Button();
            btn2 = new Button();
            btn3 = new Button();
            btnAdd = new Button();
            btn0 = new Button();
            btnDot = new Button();
            btnEquals = new Button();
            btnCancel = new Button();
            btnOk = new Button();
            ResultTextRightClickContext.SuspendLayout();
            SuspendLayout();
            // 
            // lblDisplay
            // 
            lblDisplay.BackColor = SystemColors.ControlLightLight;
            lblDisplay.BorderStyle = BorderStyle.FixedSingle;
            lblDisplay.ContextMenuStrip = ResultTextRightClickContext;
            lblDisplay.Font = new Font("Arial Narrow", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDisplay.Location = new Point(20, 11);
            lblDisplay.Name = "lblDisplay";
            lblDisplay.Size = new Size(227, 42);
            lblDisplay.TabIndex = 0;
            lblDisplay.Text = "0";
            lblDisplay.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ResultTextRightClickContext
            // 
            ResultTextRightClickContext.ImageScalingSize = new Size(20, 20);
            ResultTextRightClickContext.Items.AddRange(new ToolStripItem[] { copyToolStripMenuItem, pasteToolStripMenuItem });
            ResultTextRightClickContext.Name = "contextMenuStrip1";
            ResultTextRightClickContext.Size = new Size(208, 52);
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            copyToolStripMenuItem.Size = new Size(207, 24);
            copyToolStripMenuItem.Text = "Copy Result";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
            pasteToolStripMenuItem.Size = new Size(207, 24);
            pasteToolStripMenuItem.Text = "Paste";
            pasteToolStripMenuItem.Click += pasteToolStripMenuItem_Click;
            // 
            // btnClear
            // 
            btnClear.Cursor = Cursors.Hand;
            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnClear.Location = new Point(20, 70);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(52, 44);
            btnClear.TabIndex = 0;
            btnClear.Text = "C";
            btnClear.UseVisualStyleBackColor = false;
            btnClear.Click += BtnClear_Click;
            // 
            // btnBackspace
            // 
            btnBackspace.Cursor = Cursors.Hand;
            btnBackspace.FlatStyle = FlatStyle.Flat;
            btnBackspace.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnBackspace.Location = new Point(79, 70);
            btnBackspace.Name = "btnBackspace";
            btnBackspace.Size = new Size(52, 44);
            btnBackspace.TabIndex = 1;
            btnBackspace.Text = "←";
            btnBackspace.UseVisualStyleBackColor = false;
            btnBackspace.Click += BtnBackspace_Click;
            // 
            // btnSign
            // 
            btnSign.Cursor = Cursors.Hand;
            btnSign.FlatStyle = FlatStyle.Flat;
            btnSign.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnSign.Location = new Point(138, 70);
            btnSign.Name = "btnSign";
            btnSign.Size = new Size(52, 44);
            btnSign.TabIndex = 2;
            btnSign.Tag = "SIGN";
            btnSign.Text = "±";
            btnSign.UseVisualStyleBackColor = false;
            btnSign.Click += BtnSign_Click;
            // 
            // btnDiv
            // 
            btnDiv.Cursor = Cursors.Hand;
            btnDiv.FlatStyle = FlatStyle.Flat;
            btnDiv.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnDiv.Location = new Point(197, 70);
            btnDiv.Name = "btnDiv";
            btnDiv.Size = new Size(52, 44);
            btnDiv.TabIndex = 3;
            btnDiv.Tag = "/";
            btnDiv.Text = "/";
            btnDiv.UseVisualStyleBackColor = false;
            btnDiv.Click += BtnOperator_Click;
            // 
            // btn7
            // 
            btn7.Cursor = Cursors.Hand;
            btn7.FlatStyle = FlatStyle.Flat;
            btn7.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn7.Location = new Point(20, 121);
            btn7.Name = "btn7";
            btn7.Size = new Size(52, 44);
            btn7.TabIndex = 4;
            btn7.Tag = "7";
            btn7.Text = "7";
            btn7.UseVisualStyleBackColor = false;
            btn7.Click += BtnDigit_Click;
            // 
            // btn8
            // 
            btn8.Cursor = Cursors.Hand;
            btn8.FlatStyle = FlatStyle.Flat;
            btn8.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn8.Location = new Point(79, 121);
            btn8.Name = "btn8";
            btn8.Size = new Size(52, 44);
            btn8.TabIndex = 5;
            btn8.Tag = "8";
            btn8.Text = "8";
            btn8.UseVisualStyleBackColor = false;
            btn8.Click += BtnDigit_Click;
            // 
            // btn9
            // 
            btn9.Cursor = Cursors.Hand;
            btn9.FlatStyle = FlatStyle.Flat;
            btn9.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn9.Location = new Point(138, 121);
            btn9.Name = "btn9";
            btn9.Size = new Size(52, 44);
            btn9.TabIndex = 6;
            btn9.Tag = "9";
            btn9.Text = "9";
            btn9.UseVisualStyleBackColor = false;
            btn9.Click += BtnDigit_Click;
            // 
            // btnMul
            // 
            btnMul.Cursor = Cursors.Hand;
            btnMul.FlatStyle = FlatStyle.Flat;
            btnMul.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnMul.Location = new Point(197, 121);
            btnMul.Name = "btnMul";
            btnMul.Size = new Size(52, 44);
            btnMul.TabIndex = 7;
            btnMul.Tag = "*";
            btnMul.Text = "×";
            btnMul.UseVisualStyleBackColor = false;
            btnMul.Click += BtnOperator_Click;
            // 
            // btn4
            // 
            btn4.Cursor = Cursors.Hand;
            btn4.FlatStyle = FlatStyle.Flat;
            btn4.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn4.Location = new Point(20, 172);
            btn4.Name = "btn4";
            btn4.Size = new Size(52, 44);
            btn4.TabIndex = 8;
            btn4.Tag = "4";
            btn4.Text = "4";
            btn4.UseVisualStyleBackColor = false;
            btn4.Click += BtnDigit_Click;
            // 
            // btn5
            // 
            btn5.Cursor = Cursors.Hand;
            btn5.FlatStyle = FlatStyle.Flat;
            btn5.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn5.Location = new Point(79, 172);
            btn5.Name = "btn5";
            btn5.Size = new Size(52, 44);
            btn5.TabIndex = 9;
            btn5.Tag = "5";
            btn5.Text = "5";
            btn5.UseVisualStyleBackColor = false;
            btn5.Click += BtnDigit_Click;
            // 
            // btn6
            // 
            btn6.Cursor = Cursors.Hand;
            btn6.FlatStyle = FlatStyle.Flat;
            btn6.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn6.Location = new Point(138, 172);
            btn6.Name = "btn6";
            btn6.Size = new Size(52, 44);
            btn6.TabIndex = 10;
            btn6.Tag = "6";
            btn6.Text = "6";
            btn6.UseVisualStyleBackColor = false;
            btn6.Click += BtnDigit_Click;
            // 
            // btnSub
            // 
            btnSub.Cursor = Cursors.Hand;
            btnSub.FlatStyle = FlatStyle.Flat;
            btnSub.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnSub.Location = new Point(197, 172);
            btnSub.Name = "btnSub";
            btnSub.Size = new Size(52, 44);
            btnSub.TabIndex = 11;
            btnSub.Tag = "-";
            btnSub.Text = "−";
            btnSub.UseVisualStyleBackColor = false;
            btnSub.Click += BtnOperator_Click;
            // 
            // btn1
            // 
            btn1.Cursor = Cursors.Hand;
            btn1.FlatStyle = FlatStyle.Flat;
            btn1.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn1.Location = new Point(20, 223);
            btn1.Name = "btn1";
            btn1.Size = new Size(52, 44);
            btn1.TabIndex = 12;
            btn1.Tag = "1";
            btn1.Text = "1";
            btn1.UseVisualStyleBackColor = false;
            btn1.Click += BtnDigit_Click;
            // 
            // btn2
            // 
            btn2.Cursor = Cursors.Hand;
            btn2.FlatStyle = FlatStyle.Flat;
            btn2.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn2.Location = new Point(79, 223);
            btn2.Name = "btn2";
            btn2.Size = new Size(52, 44);
            btn2.TabIndex = 13;
            btn2.Tag = "2";
            btn2.Text = "2";
            btn2.UseVisualStyleBackColor = false;
            btn2.Click += BtnDigit_Click;
            // 
            // btn3
            // 
            btn3.Cursor = Cursors.Hand;
            btn3.FlatStyle = FlatStyle.Flat;
            btn3.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn3.Location = new Point(138, 223);
            btn3.Name = "btn3";
            btn3.Size = new Size(52, 44);
            btn3.TabIndex = 14;
            btn3.Tag = "3";
            btn3.Text = "3";
            btn3.UseVisualStyleBackColor = false;
            btn3.Click += BtnDigit_Click;
            // 
            // btnAdd
            // 
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnAdd.Location = new Point(197, 223);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(52, 44);
            btnAdd.TabIndex = 15;
            btnAdd.Tag = "+";
            btnAdd.Text = "+";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += BtnOperator_Click;
            // 
            // btn0
            // 
            btn0.Cursor = Cursors.Hand;
            btn0.FlatStyle = FlatStyle.Flat;
            btn0.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btn0.Location = new Point(79, 274);
            btn0.Name = "btn0";
            btn0.Size = new Size(52, 44);
            btn0.TabIndex = 16;
            btn0.Tag = "0";
            btn0.Text = "0";
            btn0.UseVisualStyleBackColor = false;
            btn0.Click += BtnDigit_Click;
            // 
            // btnDot
            // 
            btnDot.Cursor = Cursors.Hand;
            btnDot.FlatStyle = FlatStyle.Flat;
            btnDot.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnDot.Location = new Point(20, 274);
            btnDot.Name = "btnDot";
            btnDot.Size = new Size(52, 44);
            btnDot.TabIndex = 17;
            btnDot.Tag = ".";
            btnDot.Text = ".";
            btnDot.UseVisualStyleBackColor = false;
            btnDot.Click += BtnDigit_Click;
            // 
            // btnEquals
            // 
            btnEquals.Cursor = Cursors.Hand;
            btnEquals.FlatStyle = FlatStyle.Flat;
            btnEquals.Font = new Font("Segoe UI Semibold", 10.2F, FontStyle.Bold);
            btnEquals.Location = new Point(138, 274);
            btnEquals.Name = "btnEquals";
            btnEquals.Size = new Size(111, 44);
            btnEquals.TabIndex = 18;
            btnEquals.Tag = "=";
            btnEquals.Text = "=";
            btnEquals.UseVisualStyleBackColor = false;
            btnEquals.Click += BtnEquals_Click;
            // 
            // btnCancel
            // 
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancel.Location = new Point(20, 338);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(108, 35);
            btnCancel.TabIndex = 19;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += BtnCancel_Click;
            // 
            // btnOk
            // 
            btnOk.Cursor = Cursors.Hand;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOk.Location = new Point(138, 338);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(111, 35);
            btnOk.TabIndex = 20;
            btnOk.Text = "↩Return(F2)";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += BtnReturn_Click;
            // 
            // frmCalculator
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(266, 384);
            Controls.Add(lblDisplay);
            Controls.Add(btnClear);
            Controls.Add(btnCancel);
            Controls.Add(btnEquals);
            Controls.Add(btnOk);
            Controls.Add(btnBackspace);
            Controls.Add(btnDot);
            Controls.Add(btn5);
            Controls.Add(btnSign);
            Controls.Add(btn6);
            Controls.Add(btn0);
            Controls.Add(btn4);
            Controls.Add(btnDiv);
            Controls.Add(btnSub);
            Controls.Add(btnAdd);
            Controls.Add(btnMul);
            Controls.Add(btn7);
            Controls.Add(btn1);
            Controls.Add(btn3);
            Controls.Add(btn9);
            Controls.Add(btn8);
            Controls.Add(btn2);
            Font = new Font("Segoe UI", 9.5F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCalculator";
            Padding = new Padding(8);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Calculator";
            TopMost = true;
            Load += frmCalculator_Load;
            ResultTextRightClickContext.ResumeLayout(false);
            ResumeLayout(false);
        }

        private ContextMenuStrip ResultTextRightClickContext;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem pasteToolStripMenuItem;
    }
}