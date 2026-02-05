namespace Land_Readjustment_Tool
{
    partial class frmAreaConverter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textbox1 = new TextBox();
            textBox2 = new TextBox();
            button1 = new Button();
            SuspendLayout();
            // 
            // textbox1
            // 
            textbox1.Location = new Point(70, 116);
            textbox1.Name = "textbox1";
            textbox1.Size = new Size(209, 27);
            textbox1.TabIndex = 0;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(422, 116);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(125, 27);
            textBox2.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(300, 107);
            button1.Name = "button1";
            button1.Size = new Size(94, 44);
            button1.TabIndex = 2;
            button1.Text = "convert";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // frmAreaConverter
            // 
            AutoScaleDimensions = new SizeF(120F, 120F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(727, 503);
            Controls.Add(button1);
            Controls.Add(textBox2);
            Controls.Add(textbox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAreaConverter";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Area Converter";
            Load += frmAreaConverter_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private TextBox textbox1;
        private TextBox textBox2;
        private Button button1;
    }
}