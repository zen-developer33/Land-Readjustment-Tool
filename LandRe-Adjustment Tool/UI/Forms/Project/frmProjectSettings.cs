using Land_Readjustment_Tool.Core.Interfaces;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Core.Entities.Project;

namespace Land_Readjustment_Tool.UI.Forms.Project
{
    /// <summary>
    /// Project settings dialog form.
    /// Allows user to configure project-wide settings.
    /// Receives IProjectSettingsService via constructor injection.
    /// </summary>
    public partial class frmProjectSettings : Form
    {
        private readonly IProjectSettingsService _service;
        private ProjectSettings? _settings;

        /// <summary>
        /// Receives service via constructor injection.
        /// </summary>
        public frmProjectSettings(IProjectSettingsService service)
        {
            InitializeComponent();

        }
    }
}