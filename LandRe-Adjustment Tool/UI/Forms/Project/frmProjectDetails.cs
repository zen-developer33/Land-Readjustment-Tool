using Land_Readjustment_Tool.Core.Entities.Project;
using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Extensions;
using Land_Readjustment_Tool.Repositories.Project;
using Land_Readjustment_Tool.Services.Project;
using System.Diagnostics;

namespace Land_Readjustment_Tool
{
    /// <summary>
    /// Form for viewing and editing project details.
    /// Uses IProjectInfoService via AppServices.
    /// Never touches repository or database directly.
    /// </summary>
    public partial class frm_ProjectDetails : Form
    {
        private readonly IProjectInfoService _service;
        private Core.Entities.Project.ProjectInfo?
            _projectInfo;

        /// <summary>
        /// Constructor — receives service via DI.
        /// Called from frmMain.OpenProjectDetails().
        /// </summary>
        public frm_ProjectDetails(IProjectInfoService service)
        {
            InitializeComponent();
            _service = service;
            SetReadOnlyFields();
            this.Shown += frm_ProjectDetails_Shown!;
        }

        // ── SETUP ────────────────────────────────────

        /// <summary>
        /// Makes read only fields visually distinct.
        /// User cannot edit these fields.
        /// </summary>
        private void SetReadOnlyFields()
        {
            txtProjectName.ReadOnly = true;
            txtProjectName.BackColor =
                SystemColors.Control;
            txtProjectName.TabStop = false;

            txtProjectPath.ReadOnly = true;
            txtProjectPath.BackColor =
                SystemColors.Control;
            txtProjectPath.TabStop = false;

            txtCreatedDate.ReadOnly = true;
            txtCreatedDate.BackColor =
                SystemColors.Control;
            txtCreatedDate.TabStop = false;

            // First editable field gets focus
            txtProvince.TabIndex = 0;
        }

        // ── EVENTS ───────────────────────────────────

        private void frm_ProjectDetails_Shown(
            object? sender, EventArgs e)
        {
            txtProvince.Focus();
            txtProvince.SelectAll();
        }

        private async void frm_ProjectDetails_Load(
            object? sender, EventArgs e)
        {
            await LoadAsync();
        }

        // ── LOAD ─────────────────────────────────────

        /// <summary>
        /// Loads project info from database via service.
        /// Disables form while loading.
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                SetFormEnabled(false);

                if (AppServices.HasContext &&
                    AppServices.Context.Info != null)
                {
                    _projectInfo = AppServices.Context.Info;
                    Debug.WriteLine(
                        "[ProjectDetails] Load source: runtime context.");
                }
                else
                {
                    _projectInfo = await _service.GetAsync();
                    Debug.WriteLine(
                        "[ProjectDetails] Load source: repository/service.");
                }

                if (_projectInfo == null)
                {
                    MessageBox.Show(
                        "Could not load project info.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                PopulateForm(_projectInfo);
                Debug.WriteLine(
                    $"[ProjectDetails] Loaded province='{_projectInfo.Province}', " +
                    $"district='{_projectInfo.District}', " +
                    $"snap context unsaved='{(AppServices.HasContext ? AppServices.Context.HasUnsavedChanges : false)}'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
            finally
            {
                SetFormEnabled(true);
                txtProvince.Focus();
                txtProvince.SelectAll();
            }
        }

        /// <summary>
        /// Fills all form controls from entity.
        /// </summary>
        private void PopulateForm(
            Core.Entities.Project.ProjectInfo info)
        {
            // Read only — from entity and AppServices
            txtProjectName.Text = info.ProjectName;
            txtProjectPath.Text =
                AppServices.HasContext
                ? AppServices.Context.ProjectFilePath
                : string.Empty;
            txtCreatedDate.Text =
                info.CreatedDate
                .ToString("yyyy-MM-dd HH:mm:ss");

            // Location
            txtProvince.Text = info.Province ?? "";
            txtDistrict.Text = info.District ?? "";
            txtMunicipality.Text =
                info.Municipality ?? "";
            txtWardNo.Text = info.WardNo ?? "";
            txtProjectSite.Text =
                info.ProjectSite ?? "";

            // Stakeholders
            txtImplementingAgency.Text =
                info.ImplementingAgency ?? "";
            txtConsultingAgency.Text =
                info.ConsultingAgency ?? "";

            // Gazette date
            if (info.GazetteDate.HasValue)
            {
                dtpApprovalDate.Checked = true;
                dtpApprovalDate.Value =
                    info.GazetteDate.Value;
            }
            else
            {
                dtpApprovalDate.Checked = false;
            }

            // Project start date
            if (info.ProjectStartDate.HasValue)
            {
                dtpProjectStartDate.Checked = true;
                dtpProjectStartDate.Value =
                    info.ProjectStartDate.Value;
            }
            else
            {
                dtpProjectStartDate.Checked = false;
            }

            // Project end date
            if (info.ProjectEndDate.HasValue)
            {
                dtpProjectEndDate.Checked = true;
                dtpProjectEndDate.Value =
                    info.ProjectEndDate.Value;
            }
            else
            {
                dtpProjectEndDate.Checked = false;
            }

            // Notes
            txtProjectNotes.Text =
                info.ProjectNotes ?? "";
        }

        /// <summary>
        /// Reads form controls into entity.
        /// Never updates read only fields.
        /// </summary>
        private void CollectFormData(ProjectInfo info)
        {
            // Location
            info.Province =
                txtProvince.Text.NullIfEmpty();
            info.District =
                txtDistrict.Text.NullIfEmpty();
            info.Municipality =
                txtMunicipality.Text.NullIfEmpty();
            info.WardNo =
                txtWardNo.Text.NullIfEmpty();
            info.ProjectSite =
                txtProjectSite.Text.NullIfEmpty();

            // Stakeholders
            info.ImplementingAgency =
                txtImplementingAgency.Text.NullIfEmpty();
            info.ConsultingAgency =
                txtConsultingAgency.Text.NullIfEmpty();

            // Dates — Checked = user selected a date
            info.GazetteDate =
                dtpApprovalDate.Checked
                ? dtpApprovalDate.Value
                : null;

            info.ProjectStartDate =
                dtpProjectStartDate.Checked
                ? dtpProjectStartDate.Value
                : null;

            info.ProjectEndDate =
                dtpProjectEndDate.Checked
                ? dtpProjectEndDate.Value
                : null;

            // Notes
            info.ProjectNotes =
                txtProjectNotes.Text.NullIfEmpty();
        }

        // ── SAVE ─────────────────────────────────────

        /// <summary>
        /// Saves project info via service.
        /// Service validates business rules.
        /// Form catches and shows errors to user.
        /// </summary>
        private async void btnOK_Click(
            object? sender, EventArgs e)
        {
            if (_projectInfo == null) return;

            try
            {
                SetFormEnabled(false);

                // Read form into entity
                CollectFormData(_projectInfo);

                Debug.WriteLine(
                    $"[ProjectDetails] Staging on OK province='{_projectInfo.Province}', " +
                    $"district='{_projectInfo.District}', start='{_projectInfo.ProjectStartDate}', end='{_projectInfo.ProjectEndDate}'.");

                // Service validates and stages only.
                await _service.SaveAsync(_projectInfo);

                // Update AppServices context
                if (AppServices.HasContext)
                {
                    AppServices.Context
                        .UpdateInfo(_projectInfo);
                    AppServices.Context.MarkAsModified();

                    Debug.WriteLine(
                        "[ProjectDetails] Context updated and marked modified.");
                }

                DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (InvalidOperationException ex)
            {
                // Business rule violation
                // Show exact message to user
                MessageBox.Show(
                    ex.Message,
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                // Unexpected error
                // Logger already has full details
                MessageBox.Show(
                    "Failed to save. Please try again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                SetFormEnabled(true);
            }
        }

        private void dtp_ValueChanged(
            object? sender, EventArgs e)
        {
            if (sender == dtpApprovalDate)
            {
                txtApprovalDate.Text = dtpApprovalDate.Checked
                    ? dtpApprovalDate.Value.ToString("yyyy-MM-dd")
                    : string.Empty;
                return;
            }

            if (sender == dtpProjectStartDate)
            {
                txtProjectStartDate.Text = dtpProjectStartDate.Checked
                    ? dtpProjectStartDate.Value.ToString("yyyy-MM-dd")
                    : string.Empty;
                return;
            }

            if (sender == dtpProjectEndDate)
            {
                txtProjectEndDate.Text = dtpProjectEndDate.Checked
                    ? dtpProjectEndDate.Value.ToString("yyyy-MM-dd")
                    : string.Empty;
            }
        }

        // ── HELPERS ──────────────────────────────────

        /// <summary>
        /// Enables or disables all editable controls.
        /// Disabled during load and save operations.
        /// </summary>
        private void SetFormEnabled(bool enabled)
        {
            btnOK.Enabled = enabled;
            txtProvince.Enabled = enabled;
            txtDistrict.Enabled = enabled;
            txtMunicipality.Enabled = enabled;
            txtWardNo.Enabled = enabled;
            txtProjectSite.Enabled = enabled;
            txtImplementingAgency.Enabled = enabled;
            txtConsultingAgency.Enabled = enabled;
            txtProjectNotes.Enabled = enabled;
            dtpApprovalDate.Enabled = enabled;
            dtpProjectStartDate.Enabled = enabled;
            dtpProjectEndDate.Enabled = enabled;
        }


    }
}