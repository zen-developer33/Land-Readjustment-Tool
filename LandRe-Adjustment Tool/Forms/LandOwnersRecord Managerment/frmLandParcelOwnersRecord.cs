using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment
{
    public partial class frmLandParcelOwnersRecord : Form
    {
        private readonly string _projectPath;
        private List<BaselineLandParceRecord> _landParcelRecords;
        private BindingList<BaselineLandParceRecord> _displayedRecords;
        
        public frmLandParcelOwnersRecord()
        {
            InitializeComponent();
            _projectPath = CurrentProject.Info.ProjectPath;
            _landParcelRecords = new List<BaselineLandParceRecord>();
            _displayedRecords = new BindingList<BaselineLandParceRecord>();
            this.Text = "Land Parcel Records";
            InitializeRepository();




            InitializeDataGridView();
        }

        private void InitializeRepository()
        {
            var _dbhelper = new DatabaseHelper(_projectPath);
            _dbhelper.InitializeDatabase();
            var conn = _dbhelper.GetConnection();
            if (!DatabaseSchema.HasCorrectSchema(conn))
            {
                DatabaseSchema.RecreateSchema(conn);
            }
            else
            {
                DatabaseSchema.CreateSchema(conn);
            }
            


        }

        private void InitializeDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.MultiSelect = true;
            dgvRecords.ReadOnly = true;
            dgvRecords.DoubleBuffered(true);
            dgvRecords.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 58, 79);
            dgvRecords.RowHeadersVisible = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.BorderStyle = BorderStyle.None;
            dgvRecords.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvRecords.GridColor = Color.FromArgb(220, 220, 220);

            // Style row headers to match data cells (simple, no bold)
            dgvRecords.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvRecords.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRecords.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            dgvRecords.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;

            // Clear existing columns
            dgvRecords.Columns.Clear();

            // Add columns for parcel and owner data (removed SN column)
            AddColumn("ParcelNo", "Parcel No", 80);
            AddColumn("ParcelLocation", "Parcel Location", 140);
            AddColumn("Province", "Province", 80);
            AddColumn("District", "District", 80);
            AddColumn("MunicipalityVillage", "Municipality", 100);
            AddColumn("MapSheetNo", "Map Sheet", 85);
            AddColumn("LandOwnersName", "Owner Name", 160);
            AddColumn("FatherSpouse", "Father/Spouse", 140);
            AddColumn("Gender", "Gender", 65);
            AddColumn("PermanentAddress", "Permanent Address", 150);
            AddColumn("CitizenshipIssueDate", "Citizenship Issue Date", 120);
            AddColumn("CitizenshipIssueDistrict", "Citizenship Issue District", 140);
            AddColumn("CitizenshipNumber", "Citizenship No", 115);
            AddColumn("ContactInfo", "Contact Info", 120);

            AddColumn("ParcelLocation", "Parcel Location", 140);
            AddColumn("AreaInSqm", "Area (sqm)", 90);
            AddColumn("AreaInRAPD", "Area (RAPD)", 90);
            AddColumn("AreaInBKD", "Area (BKD)", 90);
            AddColumn("LandOwnershipType", "Land OwnerShip", 90);
            AddColumn("LandUse", "Land Use", 90);

            AddColumn("MothNo", "Moth No", 70);
            AddColumn("PaanaNo", "Paana No", 70);

            AddColumn("Remarks", "Remarks", 120);

            // Enable sorting for specific columns
            //dgvRecords.Columns["ParcelNo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgvRecords.Columns["MapSheetNo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgvRecords.Columns["ParcelNo"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgvRecords.Columns["MapSheetNo"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvRecords.Columns["ParcelNo"]?.SortMode = DataGridViewColumnSortMode.Automatic;
            dgvRecords.Columns["MapSheetNo"]?.SortMode = DataGridViewColumnSortMode.Automatic;
            dgvRecords.Columns["LandOwnersName"]?.SortMode = DataGridViewColumnSortMode.Automatic;

            // Make headers styled
            dgvRecords.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvRecords.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 65, 95);
            dgvRecords.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRecords.ColumnHeadersHeight = 34;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvRecords.RowTemplate.Height = 28;

            //dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            //dgvRecords.CellDoubleClick += DgvRecords_CellDoubleClick;
            dgvRecords.RowPostPaint += DgvRecords_RowPostPaint;
        }
        private void DgvRecords_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            // Row number is 1-based for user display
            string rowNumber = (e.RowIndex + 1).ToString();

            // Calculate bounds for the row header
            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dgvRecords.RowHeadersWidth - 4,
                e.RowBounds.Height);

            // Use TextRenderer for crisp text rendering with same font as data cells
            TextRenderer.DrawText(
                e.Graphics,
                rowNumber,
                dgvRecords.DefaultCellStyle.Font,
                headerBounds,
                dgvRecords.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
        private void AddColumn(string dataPropertyName, string headerText, int width)
        {
            dgvRecords.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = dataPropertyName,
                HeaderText = headerText,
                DataPropertyName = dataPropertyName,
                Width = width
            });
        }



        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void frmLandParcelOwnersRecord_Load(object sender, EventArgs e)
        {

        }
    }
}
