using Land_Readjustment_Tool.Core.Entities.Layout;

namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    internal sealed partial class frmRoadDefinitionEditor : Form
    {
        private readonly Road _road;
        private readonly bool _readOnlyMode;

        public Road Road => _road;

        public frmRoadDefinitionEditor(Road? road, bool readOnlyMode = false)
        {
            _readOnlyMode = readOnlyMode;
            _road = road == null
                ? new Road { SurfaceType = "Earthen" }
                : new Road
                {
                    Id = road.Id,
                    RoadName = road.RoadName,
                    RoadCode = road.RoadCode,
                    SurfaceType = road.SurfaceType,
                    RoadWidth = road.RoadWidth,
                    RightOfWayWidth = road.RightOfWayWidth,
                    RoadType = road.RoadType,
                    CanvasObjectId = road.CanvasObjectId,
                    Description = road.Description,
                    CreatedDate = road.CreatedDate,
                    LastModifiedDate = road.LastModifiedDate
                };

            InitializeComponent();
            Text = _road.Id == 0 ? "Add Road Definition" : "Edit Road Definition";
            LoadValues();
            ApplyReadOnlyMode();
            RecordFormTheme.Apply(this);
        }

        private void LoadValues()
        {
            txtCode.Text = _road.RoadCode ?? string.Empty;
            nudRowWidth.Value = ClampDecimal(_road.RightOfWayWidth ?? _road.RoadWidth);
            cboSurface.SelectedItem = string.IsNullOrWhiteSpace(_road.SurfaceType)
                ? "Earthen"
                : NormalizeSurface(_road.SurfaceType);
            txtName.Text = _road.RoadName;
            cboType.Text = _road.RoadType ?? string.Empty;
            txtDescription.Text = _road.Description ?? string.Empty;
        }

        private static decimal ClampDecimal(double value)
        {
            if (value < 0) return 0;
            if (value > 200) return 200;
            return Convert.ToDecimal(value);
        }

        private static string NormalizeSurface(string surface)
        {
            return surface.Equals("Gravel", StringComparison.OrdinalIgnoreCase)
                ? "Gravelled"
                : surface;
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            string code = txtCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show(this, "Road Code is required.", "Road Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                txtCode.Focus();
                return;
            }

            if (nudRowWidth.Value <= 0)
            {
                MessageBox.Show(this, "ROW Width must be greater than zero.", "Road Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                nudRowWidth.Focus();
                return;
            }

            _road.RoadCode = code;
            _road.RightOfWayWidth = Convert.ToDouble(nudRowWidth.Value);
            _road.SurfaceType = cboSurface.SelectedItem?.ToString() ?? "Earthen";
            _road.RoadStatus = string.Empty;
            _road.RoadName = string.IsNullOrWhiteSpace(txtName.Text) ? code : txtName.Text.Trim();
            _road.RoadType = string.IsNullOrWhiteSpace(cboType.Text) ? null : cboType.Text.Trim();
            _road.RoadWidth = Convert.ToDouble(nudRowWidth.Value);
            _road.Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                ? null
                : txtDescription.Text.Trim();
        }

        private void ApplyReadOnlyMode()
        {
            if (!_readOnlyMode)
                return;

            Text = _road.Id == 0
                ? "Road Definition (Read-Only)"
                : "Road Definition (Read-Only)";
            txtCode.ReadOnly = true;
            txtName.ReadOnly = true;
            txtDescription.ReadOnly = true;
            cboType.Enabled = false;
            cboSurface.Enabled = false;
            nudRowWidth.Enabled = false;
            btnSave.Enabled = false;
            btnCancel.Text = "Close";
            AcceptButton = null;
        }
    }
}
