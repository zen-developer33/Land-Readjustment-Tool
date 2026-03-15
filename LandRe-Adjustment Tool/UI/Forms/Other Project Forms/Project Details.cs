using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;
using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool
{
    public partial class frm_ProjectDetails : Form
    {


        public frm_ProjectDetails()
        {
            InitializeComponent();

            // Set read-only fields
            txtProjectName.ReadOnly = true;
            txtProjectName.BackColor = SystemColors.Control;
            txtProjectName.TabStop = false;  // Skip when tabbing
            txtProjectName.TabIndex = 100;   // Put at end of tab order

            txtProjectPath.ReadOnly = true;
            txtProjectPath.BackColor = SystemColors.Control;
            txtProjectPath.TabStop = false;
            txtProjectPath.TabIndex = 101;

            txtCreatedDate.ReadOnly = true;
            txtCreatedDate.BackColor = SystemColors.Control;
            txtCreatedDate.TabStop = false;
            txtCreatedDate.TabIndex = 102;

            // Set first editable field
            txtProvince.TabIndex = 0;  // First in tab order

            // Wire up the Shown event
            this.Shown += frm_ProjectDetails_Shown!;
        }


        private void frm_ProjectDetails_Shown(object? sender, EventArgs e)
        {
            txtProvince.Focus();
            txtProvince.SelectAll();
        }

        public void LoadProjectInfoToForm()
        {
            if (!CurrentProject.IsOpen) return;

            ProjectInfo info = CurrentProject.Info!;
            txtProjectName.Text = info.ProjectName;
            txtProjectPath.Text = info.ProjectPath;

            // Created date - read-only, just display
            txtCreatedDate.Text = info.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            txtCreatedDate.ReadOnly = true; // Make it read-only
            txtCreatedDate.BackColor = SystemColors.Control; // Visual indicator it's read-only

            // Approval Date - only set DateTimePicker if it's a valid date
            if (info.ApprovalDate != DateTime.MinValue &&
                info.ApprovalDate > dtpApprovalDate.MinDate &&
                info.ApprovalDate < dtpApprovalDate.MaxDate)
            {
                txtApprovalDate.Text = info.ApprovalDate.ToString("yyyy-MM-dd");
                dtpApprovalDate.Value = info.ApprovalDate;
            }
            else
            {
                txtApprovalDate.Text = ""; // Leave empty if not set
            }

            txtProvince.Text = info.Location.Province;
            txtDistrict.Text = info.Location.District;
            txtMunicipality.Text = info.Location.Municipality;
            txtWardNo.Text = info.Location.WardNo;
            txtProjectSite.Text = info.Location.ProjectSite;
            txtImplementingAgency.Text = info.Stakeholders.ImplementingAgency;
            txtConsultingAgency.Text = info.Stakeholders.ConsultingAgency;
        }

        private void textBox1_TextChanged(object? sender, EventArgs e)
        {

        }

        private void frm_ProjectDetails_Load(object? sender, EventArgs e)
        {
            // Load data first
            LoadProjectInfoToForm();

            // Then set focus and select text
            txtProvince.Focus();
            txtProvince.SelectAll();
        }

        private void btnOK_click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtProjectName.Text))
            {
                MessageBox.Show("Project name is required.", "Validation Error");
                return;
            }


            //Upadate CurrentProject.Info with form DAta
            bool flowControl = SaveFormDataToProjectInfo();
            if (!flowControl)
            {
                return;
            }


            DatabaseHelper db = new DatabaseHelper(CurrentProject.Info.ProjectPath);
            db.InitializeDatabase();
            ProjectInfoRepository repo = new(db.GetConnection());
            repo.SaveProjectInfo(CurrentProject.Info);

            this.Close();
        }

        private bool SaveFormDataToProjectInfo()
        {
            if (CurrentProject.Info == null) return false;

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtProjectName.Text))
                {
                    MessageBox.Show("Project name is required.", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtProjectName.Focus();
                    return false;
                }

               
                // Parse approval date (optional - allow empty)
                DateTime approvalDate = DateTime.MinValue;
                if (!string.IsNullOrWhiteSpace(txtApprovalDate.Text))
                {
                    if (!DateTime.TryParse(txtApprovalDate.Text, out approvalDate))
                    {
                        MessageBox.Show("Invalid approval date format. Please enter a valid date or leave it empty.",
                            "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtApprovalDate.Focus();
                        txtApprovalDate.SelectAll();
                        return false;
                    }
                }

                // Update CurrentProject.Info (update existing, don't create new)
                // Dont Update: Project Name, ProjectPath, Created Date (immutable)
                CurrentProject.Info.ProjectName = txtProjectName.Text;
                CurrentProject.Info.ProjectPath = txtProjectPath.Text;
                CurrentProject.Info.CreatedDate = DateTime.Parse(txtCreatedDate.Text);
                CurrentProject.Info.ApprovalDate = approvalDate;

                CurrentProject.Info.Location.Province = txtProvince.Text;
                CurrentProject.Info.Location.District = txtDistrict.Text;
                CurrentProject.Info.Location.Municipality = txtMunicipality.Text;
                CurrentProject.Info.Location.WardNo = txtWardNo.Text;
                CurrentProject.Info.Location.ProjectSite = txtProjectSite.Text;

                CurrentProject.Info.Stakeholders.ImplementingAgency = txtImplementingAgency.Text;
                CurrentProject.Info.Stakeholders.ConsultingAgency = txtConsultingAgency.Text;

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving form data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private void dtpApprovalDate_ValueChanged(object? sender, EventArgs e)
        {
            txtApprovalDate.Text = dtpApprovalDate.Value.ToString("yyyy-MM-dd");
        }


    }
}
