using Land_Readjustment_Tool.Core.Entities.Layout;

namespace Land_Readjustment_Tool.UI.Forms.Definitions
{
    internal sealed partial class frmBlockDefinitionEditor : Form
    {
        private readonly Block _block;
        private readonly bool _readOnlyMode;

        public Block Block => _block;

        public frmBlockDefinitionEditor(Block? block, bool readOnlyMode = false)
        {
            _readOnlyMode = readOnlyMode;
            _block = block == null
                ? new Block { BlockLandUse = "Residential" }
                : new Block
                {
                    Id = block.Id,
                    BlockName = block.BlockName,
                    BlockCode = block.BlockCode,
                    BlockDepth = block.BlockDepth,
                    BlockLength = block.BlockLength,
                    BlockLandUse = block.BlockLandUse,
                    BlockArea = block.BlockArea,
                    CanvasObjectId = block.CanvasObjectId,
                    Description = block.Description,
                    CreatedDate = block.CreatedDate,
                    LastModifiedDate = block.LastModifiedDate
                };

            InitializeComponent();
            Text = _block.Id == 0 ? "Add Block Definition" : "Edit Block Definition";
            LoadValues();
            ApplyReadOnlyMode();
            RecordFormTheme.Apply(this);
        }

        private void LoadValues()
        {
            txtCode.Text = _block.BlockCode ?? string.Empty;
            txtName.Text = _block.BlockName;
            cboType.Text = _block.BlockLandUse ?? "Residential";
            nudDepth.Value = ClampDecimal(_block.BlockDepth);
            nudLength.Value = ClampDecimal(_block.BlockLength);
            txtDescription.Text = _block.Description ?? string.Empty;
        }

        private static decimal ClampDecimal(double value)
        {
            if (value < 0) return 0;
            if (value > 1000) return 1000;
            return Convert.ToDecimal(value);
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            if (_readOnlyMode)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            string name = txtName.Text.Trim();
            string type = cboType.Text.Trim();
            string code = txtCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show(this, "Block Name is required.", "Block Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                MessageBox.Show(this, "Block Type is required.", "Block Definition", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                cboType.Focus();
                return;
            }

            _block.BlockName = name;
            _block.BlockCode = string.IsNullOrWhiteSpace(code) ? name : code;
            _block.BlockLandUse = type;
            _block.BlockDepth = Convert.ToSingle(nudDepth.Value);
            _block.BlockLength = Convert.ToSingle(nudLength.Value);
            _block.Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                ? null
                : txtDescription.Text.Trim();
        }

        private void ApplyReadOnlyMode()
        {
            if (!_readOnlyMode)
                return;

            Text = "Block Definition (Read-Only)";
            txtCode.ReadOnly = true;
            txtName.ReadOnly = true;
            txtDescription.ReadOnly = true;
            cboType.Enabled = false;
            nudDepth.Enabled = false;
            nudLength.Enabled = false;
            btnSave.Enabled = false;
            btnCancel.Text = "Close";
            AcceptButton = null;
        }
    }
}
