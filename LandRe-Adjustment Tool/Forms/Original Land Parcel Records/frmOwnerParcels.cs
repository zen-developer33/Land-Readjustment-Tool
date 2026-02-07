using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Repositories;

namespace Land_Readjustment_Tool.Forms
{
    /// <summary>
    /// Displays all land parcels belonging to a specific landowner in a DataGridView.
    /// </summary>
    public partial class frmOwnerParcels : Form
    {
        private readonly LandOwner _owner;
        private readonly LandOwnerRepository _repository;

        public frmOwnerParcels(LandOwner owner, LandOwnerRepository repository)
        {
            InitializeComponent();
            _owner = owner;
            _repository = repository;

            Text = $"Parcels - {_owner.LandOwnersName}";
            SetupColumns();
            LoadParcels();

            // Wire up event to populate serial numbers after data binding
            dgvParcels.DataBindingComplete += DgvParcels_DataBindingComplete;
            
        }

        private void SetupColumns()
        {
            dgvParcels.AutoGenerateColumns = false;
            dgvParcels.Columns.Clear();

            // DataGridView appearance settings
            dgvParcels.ReadOnly = true;
            dgvParcels.AllowUserToAddRows = false;
            dgvParcels.AllowUserToDeleteRows = false;
            dgvParcels.AllowUserToResizeRows = false;
            dgvParcels.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvParcels.MultiSelect = false;
            dgvParcels.RowHeadersVisible = false; // Hide row headers since we have serial numbers
            dgvParcels.RowTemplate.Height = 28; // Match row height with main form

            // Add serial number column (unbound, manually populated)
            var colSerialNo = new DataGridViewTextBoxColumn
            {
                Name = "SerialNo",
                HeaderText = "#",
                Width = 40,
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgvParcels.Columns.Add(colSerialNo);

            AddColumn("ParcelNo", "Parcel No", 80);
            AddColumn("MapSheetNo", "Map Sheet", 85);
            AddColumn("FormattedLocation", "Location", 240);
            AddColumn("AreaInSqm", "Area (sqm)", 90);
            AddColumn("AreaInRAPD", "Area (RAPD)", 90);
            AddColumn("AreaInBKD", "Area (BKD)", 90);
            AddColumn("LandUse", "Land Use", 80);
            AddColumn("MothNo", "Moth No", 70);
            AddColumn("PaanaNo", "Paana No", 70);
            AddColumn("Remarks", "Remarks", 100);

            // Make column headers bold
            dgvParcels.ColumnHeadersDefaultCellStyle.Font = new Font(dgvParcels.Font, FontStyle.Bold);
            dgvParcels.ColumnHeadersHeight = 34; // Match header height with main form
            dgvParcels.EnableHeadersVisualStyles = false;

            // Styling to match main form
            dgvParcels.BorderStyle = BorderStyle.None;
            dgvParcels.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvParcels.GridColor = Color.FromArgb(220, 220, 220);
            dgvParcels.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvParcels.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvParcels.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 230, 242);
            dgvParcels.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        private void AddColumn(string dataPropertyName, string headerText, int width)
        {
            dgvParcels.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = dataPropertyName,
                HeaderText = headerText,
                DataPropertyName = dataPropertyName,
                Width = width
            });
        }

        private void LoadParcels()
        {
            try
            {
                var parcels = _repository.GetParcelsByOwnerId(_owner.LandOwnerId);
                
                // Transform data to include formatted location
                var displayData = parcels.Select(p => new
                {
                    p.ParcelNo,
                    p.MapSheetNo,
                    FormattedLocation = FormatLocation(p.Province, p.District, p.MunicipalityVillage, p.WardNo),
                    p.AreaInSqm,
                    p.AreaInRAPD,
                    p.AreaInBKD,
                    p.LandUse,
                    p.MothNo,
                    p.PaanaNo,
                    p.Remarks
                }).ToList();

                dgvParcels.DataSource = displayData;
                lblParcelCount.Text = $"Parcels: {parcels.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load parcels: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Formats location string from Province, District, Municipality/Village, and Ward No
        /// </summary>
        private static string FormatLocation(string? province, string? district, string? municipality, string? wardNo)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(province))
                parts.Add(province);

            if (!string.IsNullOrWhiteSpace(district))
                parts.Add(district);

            if (!string.IsNullOrWhiteSpace(municipality))
            {
                if (!string.IsNullOrWhiteSpace(wardNo))
                    parts.Add($"{municipality} - {wardNo}");
                else
                    parts.Add(municipality);
            }
            else if (!string.IsNullOrWhiteSpace(wardNo))
            {
                parts.Add($"Ward {wardNo}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : "";
        }

        private void DgvParcels_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Populate serial numbers after data binding is complete
            for (int i = 0; i < dgvParcels.Rows.Count; i++)
            {
                dgvParcels.Rows[i].Cells["SerialNo"].Value = (i + 1).ToString();
            }
        }

        private void frmOwnerParcels_Load(object sender, EventArgs e)
        {

        }
    }
}
