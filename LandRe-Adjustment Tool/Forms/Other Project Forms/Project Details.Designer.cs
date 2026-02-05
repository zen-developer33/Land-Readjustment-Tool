namespace Land_Readjustment_Tool
{
    partial class frm_ProjectDetails
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_ProjectDetails));
            groupBox1 = new GroupBox();
            txtCreatedDate = new TextBox();
            label6 = new Label();
            txtProjectPath = new TextBox();
            label2 = new Label();
            txtProjectName = new TextBox();
            label1 = new Label();
            groupBox2 = new GroupBox();
            dtpApprovalDate = new DateTimePicker();
            label5 = new Label();
            txtConsultingAgency = new TextBox();
            label3 = new Label();
            txtImplementingAgency = new TextBox();
            label4 = new Label();
            txtApprovalDate = new TextBox();
            btnOK = new Button();
            groupBox3 = new GroupBox();
            txtProjectSite = new TextBox();
            label11 = new Label();
            txtWardNo = new TextBox();
            label7 = new Label();
            txtMunicipality = new TextBox();
            label10 = new Label();
            txtDistrict = new TextBox();
            label9 = new Label();
            txtProvince = new TextBox();
            label8 = new Label();
            projectInfoBindingSource = new BindingSource(components);
            groupBox4 = new GroupBox();
            textBox1 = new TextBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)projectInfoBindingSource).BeginInit();
            groupBox4.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Controls.Add(txtCreatedDate);
            groupBox1.Controls.Add(label6);
            groupBox1.Controls.Add(txtProjectPath);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(txtProjectName);
            groupBox1.Controls.Add(label1);
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // txtCreatedDate
            // 
            resources.ApplyResources(txtCreatedDate, "txtCreatedDate");
            txtCreatedDate.Name = "txtCreatedDate";
            txtCreatedDate.ReadOnly = true;
            // 
            // label6
            // 
            resources.ApplyResources(label6, "label6");
            label6.Name = "label6";
            // 
            // txtProjectPath
            // 
            resources.ApplyResources(txtProjectPath, "txtProjectPath");
            txtProjectPath.Name = "txtProjectPath";
            txtProjectPath.ReadOnly = true;
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // txtProjectName
            // 
            resources.ApplyResources(txtProjectName, "txtProjectName");
            txtProjectName.Name = "txtProjectName";
            txtProjectName.ReadOnly = true;
            txtProjectName.TextChanged += textBox1_TextChanged;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // groupBox2
            // 
            resources.ApplyResources(groupBox2, "groupBox2");
            groupBox2.Controls.Add(dtpApprovalDate);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(txtConsultingAgency);
            groupBox2.Controls.Add(label3);
            groupBox2.Controls.Add(txtImplementingAgency);
            groupBox2.Controls.Add(label4);
            groupBox2.Controls.Add(txtApprovalDate);
            groupBox2.Name = "groupBox2";
            groupBox2.TabStop = false;
            groupBox2.Tag = "";
            // 
            // dtpApprovalDate
            // 
            resources.ApplyResources(dtpApprovalDate, "dtpApprovalDate");
            dtpApprovalDate.Format = DateTimePickerFormat.Custom;
            dtpApprovalDate.Name = "dtpApprovalDate";
            dtpApprovalDate.ValueChanged += dtpApprovalDate_ValueChanged;
            // 
            // label5
            // 
            resources.ApplyResources(label5, "label5");
            label5.Name = "label5";
            // 
            // txtConsultingAgency
            // 
            resources.ApplyResources(txtConsultingAgency, "txtConsultingAgency");
            txtConsultingAgency.Name = "txtConsultingAgency";
            // 
            // label3
            // 
            resources.ApplyResources(label3, "label3");
            label3.Name = "label3";
            // 
            // txtImplementingAgency
            // 
            resources.ApplyResources(txtImplementingAgency, "txtImplementingAgency");
            txtImplementingAgency.Name = "txtImplementingAgency";
            // 
            // label4
            // 
            resources.ApplyResources(label4, "label4");
            label4.Name = "label4";
            // 
            // txtApprovalDate
            // 
            resources.ApplyResources(txtApprovalDate, "txtApprovalDate");
            txtApprovalDate.Name = "txtApprovalDate";
            // 
            // btnOK
            // 
            resources.ApplyResources(btnOK, "btnOK");
            btnOK.Name = "btnOK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_click;
            // 
            // groupBox3
            // 
            resources.ApplyResources(groupBox3, "groupBox3");
            groupBox3.Controls.Add(txtProjectSite);
            groupBox3.Controls.Add(label11);
            groupBox3.Controls.Add(txtWardNo);
            groupBox3.Controls.Add(label7);
            groupBox3.Controls.Add(txtMunicipality);
            groupBox3.Controls.Add(label10);
            groupBox3.Controls.Add(txtDistrict);
            groupBox3.Controls.Add(label9);
            groupBox3.Controls.Add(txtProvince);
            groupBox3.Controls.Add(label8);
            groupBox3.Name = "groupBox3";
            groupBox3.TabStop = false;
            // 
            // txtProjectSite
            // 
            resources.ApplyResources(txtProjectSite, "txtProjectSite");
            txtProjectSite.Name = "txtProjectSite";
            // 
            // label11
            // 
            resources.ApplyResources(label11, "label11");
            label11.Name = "label11";
            // 
            // txtWardNo
            // 
            resources.ApplyResources(txtWardNo, "txtWardNo");
            txtWardNo.Name = "txtWardNo";
            // 
            // label7
            // 
            resources.ApplyResources(label7, "label7");
            label7.Name = "label7";
            // 
            // txtMunicipality
            // 
            resources.ApplyResources(txtMunicipality, "txtMunicipality");
            txtMunicipality.Name = "txtMunicipality";
            // 
            // label10
            // 
            resources.ApplyResources(label10, "label10");
            label10.Name = "label10";
            // 
            // txtDistrict
            // 
            resources.ApplyResources(txtDistrict, "txtDistrict");
            txtDistrict.Name = "txtDistrict";
            // 
            // label9
            // 
            resources.ApplyResources(label9, "label9");
            label9.Name = "label9";
            // 
            // txtProvince
            // 
            resources.ApplyResources(txtProvince, "txtProvince");
            txtProvince.Name = "txtProvince";
            // 
            // label8
            // 
            resources.ApplyResources(label8, "label8");
            label8.Name = "label8";
            // 
            // projectInfoBindingSource
            // 
            projectInfoBindingSource.DataSource = typeof(Models.ProjectInfo);
            // 
            // groupBox4
            // 
            resources.ApplyResources(groupBox4, "groupBox4");
            groupBox4.Controls.Add(textBox1);
            groupBox4.Name = "groupBox4";
            groupBox4.TabStop = false;
            // 
            // textBox1
            // 
            resources.ApplyResources(textBox1, "textBox1");
            textBox1.Name = "textBox1";
            // 
            // frm_ProjectDetails
            // 
            AcceptButton = btnOK;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(groupBox4);
            Controls.Add(btnOK);
            Controls.Add(groupBox2);
            Controls.Add(groupBox3);
            Controls.Add(groupBox1);
            DataBindings.Add(new Binding("DataContext", projectInfoBindingSource, "ProjectName", true));
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frm_ProjectDetails";
            Load += frm_ProjectDetails_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)projectInfoBindingSource).EndInit();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtProjectPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtApprovalDate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtConsultingAgency;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtImplementingAgency;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCreatedDate;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private GroupBox groupBox3;
        private TextBox txtMunicipality;
        private Label label10;
        private TextBox txtDistrict;
        private Label label9;
        private TextBox txtProvince;
        private Label label8;
        private TextBox txtWardNo;
        private Label label7;
        private TextBox txtProjectSite;
        private Label label11;
        private BindingSource projectInfoBindingSource;
        private DateTimePicker dtpApprovalDate;
        private GroupBox groupBox4;
        private TextBox textBox1;
    }
}