using Land_Readjustment_Tool.Forms.Land_Owners_Record;
using Land_Readjustment_Tool.Models;
using Land_Readjustment_Tool.Data;
using Land_Readjustment_Tool.Services;
using Land_Readjustment_Tool.Services.LandData;
using System.ComponentModel;
using System.Globalization;

namespace Land_Readjustment_Tool.Forms.LandOwnersRecord_Managerment
{
    /// <summary>
    /// Form for viewing, filtering, and managing land parcel records with multi-layer filtering
    /// </summary>
    public partial class frmLandParcelOwnersRecord : Form
    {
        #region Fields

        private readonly string _projectPath;
        private readonly LandRecordsService _landRecordsService;
        private string _traditionalAreaUnit;
        private List<OriginalLandParcel> _allRecords = [];
        private List<OriginalLandParcel> _filteredRecords = [];
        private BindingList<LandParcelDisplayModel> _displayedRecords = [];
        private GroupBox? _groupBoxAreaBkd;
        private RadioButton? _rbBkdSqm;
        private RadioButton? _rbBigha;
        private RadioButton? _rbKattha;
        private RadioButton? _rbDhur;
        private TextBox? _txtFromAreaBkd;
        private TextBox? _txtToAreaBkd;

        // Filter unique values cache
        private HashSet<string> _uniqueProvinces = [];
        private HashSet<string> _uniqueDistricts = [];
        private HashSet<string> _uniqueMunicipalities = [];
        private HashSet<string> _uniqueWards = [];
        private HashSet<string> _uniqueMapSheets = [];
        private HashSet<string> _uniqueOwnershipTypes = [];

        private double _maxAreaSqm;
        private bool _isUpdatingControls;

        private readonly FilterCriteria _appliedFilter = new();
        private readonly SearchCriteria _appliedSearch = new();

        #endregion

        #region Constructor

        public frmLandParcelOwnersRecord()
            : this(CreateDefaultLandRecordsService(out var projectPath), projectPath)
        {
        }

        public frmLandParcelOwnersRecord(LandRecordsService landRecordsService, string projectPath)
        {
            InitializeComponent();
            _landRecordsService = landRecordsService ?? throw new ArgumentNullException(nameof(landRecordsService));
            _projectPath = projectPath ?? throw new ArgumentNullException(nameof(projectPath));
            _traditionalAreaUnit = _landRecordsService.GetTraditionalAreaUnit();
            Text = "Original Land Parcel Records";

            InitializeTraditionalAreaFilterGroups();
            SetupEventHandlers();
            SetupInputValidation();
            InitializeDataGridView();
            ApplyTraditionalAreaUnitColumns();
            ApplyTraditionalAreaFilterGroupVisibility();
            UpdateAreaFilterPlaceholders();
            UpdateApplyButtonStates();
        }

        #endregion

        #region Criteria State

        private enum AreaUnit
        {
            Sqm,
            Ropani,
            Aana,
            Bigha,
            Kattha,
            Dhur
        }

        private sealed class FilterCriteria
        {
            public string? Province { get; set; }
            public string? District { get; set; }
            public string? MunicipalityVillage { get; set; }
            public string? WardNo { get; set; }
            public string? MapSheetNo { get; set; }
            public string? LandOwnershipType { get; set; }
            public double? FromArea { get; set; }
            public double? ToArea { get; set; }
            public AreaUnit AreaUnit { get; set; } = AreaUnit.Sqm;
        }

        private sealed class SearchCriteria
        {
            public string ParcelNo { get; set; } = string.Empty;
            public string OwnerSearchText { get; set; } = string.Empty;
        }

        #endregion

        #region Initialization

        private static LandRecordsService CreateDefaultLandRecordsService(out string projectPath)
        {
            if (!AppServices.HasContext)
                throw new InvalidOperationException("No open project context found.");

            projectPath = AppServices.Context.ProjectFilePath;
            return new LandRecordsService(AppServices.Context.Session, projectPath);
        }

        private void SetupEventHandlers()
        {
            // Filter buttons
            btnApplyFilter.Click += BtnApplyFilter_Click;
            btnClearFilter.Click += BtnClearFilter_Click;

            // Search buttons
            btnApplySearch.Click += BtnApplySearch_Click;
            btnClearSearch.Click += BtnClearSearch_Click;

            // Quick filter/search toggles
            chkToggleQuickFilter.CheckedChanged += ChkToggleQuickFilter_CheckedChanged;
            chkToggleQuickSearch.CheckedChanged += ChkToggleQuickSearch_CheckedChanged;

            // ComboBox changes for quick filter
            cbProvince.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbDistrict.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbMunicipalityVillage.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbWardNo.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbMapSheet.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;
            cbLandOwnership.SelectedIndexChanged += ComboFilter_SelectedIndexChanged;

            // Radio button changes
            rbSqm.CheckedChanged += RadioButton_CheckedChanged;
            rbRopanee.CheckedChanged += RadioButton_CheckedChanged;
            rbAana.CheckedChanged += RadioButton_CheckedChanged;
            if (_rbBkdSqm != null)
            {
                _rbBkdSqm.CheckedChanged += RadioButton_CheckedChanged;
            }
            if (_rbBigha != null)
            {
                _rbBigha.CheckedChanged += RadioButton_CheckedChanged;
            }
            if (_rbKattha != null)
            {
                _rbKattha.CheckedChanged += RadioButton_CheckedChanged;
            }
            if (_rbDhur != null)
            {
                _rbDhur.CheckedChanged += RadioButton_CheckedChanged;
            }

            // Text changes for quick search
            txtParcelNo.TextChanged += TxtSearch_TextChanged;
            txtLandOwner.TextChanged += TxtSearch_TextChanged;
            txtFromArea.TextChanged += TxtArea_TextChanged;
            txtToArea.TextChanged += TxtArea_TextChanged;
            if (_txtFromAreaBkd != null)
            {
                _txtFromAreaBkd.TextChanged += TxtArea_TextChanged;
            }
            if (_txtToAreaBkd != null)
            {
                _txtToAreaBkd.TextChanged += TxtArea_TextChanged;
            }

            // CRUD buttons
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            toolStripDropDownButton1.Click += BtnRefresh_Click;
            toolStripButton1.Click += BtnViewLandOwnerDetails_Click;

            // DataGridView events
            dgvRecords.SelectionChanged += DgvRecords_SelectionChanged;
            dgvRecords.CellDoubleClick += DgvRecords_CellDoubleClick;
            dgvRecords.RowPostPaint += DgvRecords_RowPostPaint;
            Activated += FrmLandParcelOwnersRecord_Activated;
        }

        private void SetupInputValidation()
        {
            // Area textboxes: only allow digits and decimal point
            txtFromArea.KeyPress += AreaTextBox_KeyPress;
            txtToArea.KeyPress += AreaTextBox_KeyPress;
            if (_txtFromAreaBkd != null)
            {
                _txtFromAreaBkd.KeyPress += AreaTextBox_KeyPress;
            }
            if (_txtToAreaBkd != null)
            {
                _txtToAreaBkd.KeyPress += AreaTextBox_KeyPress;
            }

            // Parcel number: only allow positive integers
            txtParcelNo.KeyPress += ParcelNo_KeyPress;

            // Land owner name: allow Unicode including Devanagari
            // Enable IME for Devanagari/Nepali input
            txtLandOwner.ImeMode = ImeMode.On;
            txtLandOwner.KeyPress += LandOwnerName_KeyPress;

            // Set default radio button
            rbSqm.Checked = true;
            if (_rbBkdSqm != null)
            {
                _rbBkdSqm.Checked = true;
            }
        }

        private void InitializeDataGridView()
        {
            dgvRecords.AutoGenerateColumns = false;
            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToDeleteRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvRecords.MultiSelect = false; // Single selection only for View Land Owner Details
            dgvRecords.ReadOnly = true;
            dgvRecords.DoubleBuffered(true);
            dgvRecords.RowHeadersVisible = true;
            dgvRecords.RowHeadersWidth = 50;
            dgvRecords.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            dgvRecords.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvRecords.Columns.Clear();

            AddColumn("ParcelNo", "Parcel No", 80);
            AddColumn("MapSheetNo", "Map Sheet", 85);
            AddColumn("ParcelLocation", "Parcel Location", 130);
            AddColumn("Province", "Province", 80);
            AddColumn("District", "District", 80);
            AddColumn("MunicipalityVillage", "Municipality", 100);
            AddColumn("WardNo", "Ward", 55);
            AddColumn("LandOwnersName", "Owner Name", 150);
            AddColumn("FatherSpouse", "Father/Spouse", 130);
            AddColumn("Gender", "Gender", 60);
            AddColumn("CitizenshipNumber", "Citizenship No", 110);
            AddColumn("PermanentAddress", "Permanent Address", 140);
            AddColumn("AreaInSqm", "Area (sqm)", 85);
            AddColumn("FieldMeasuredAreaSqm", "Field Area (sqm)", 105);
            AddColumn("AreaInRAPD", "Area (RAPD)", 85);
            AddColumn("AreaInBKD", "Area (BKD)", 85);
            AddColumn("LandOwnershipType", "Ownership", 80);
            AddColumn("LandUse", "Land Use", 80);

            AddColumn("IsTenant", "Tenant", 55);
            AddColumn("TenantName", "Tenant Name", 105);
            AddColumn("MothNo", "Moth No", 65);
            AddColumn("PaanaNo", "Paana No", 65);
            AddColumn("Remarks", "Remarks", 100);

            foreach (DataGridViewColumn col in dgvRecords.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            dgvRecords.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvRecords.RowTemplate.Height = 28;
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

        private void ApplyTraditionalAreaUnitColumns()
        {
            bool showBkd = string.Equals(_traditionalAreaUnit, "BKD", StringComparison.OrdinalIgnoreCase);

            var rapdColumn = dgvRecords.Columns["AreaInRAPD"];
            if (rapdColumn != null)
            {
                rapdColumn.Visible = !showBkd;
            }

            var bkdColumn = dgvRecords.Columns["AreaInBKD"];
            if (bkdColumn != null)
            {
                bkdColumn.Visible = showBkd;
            }
        }

        private void InitializeTraditionalAreaFilterGroups()
        {
            _groupBoxAreaBkd = new GroupBox
            {
                Name = "groupBoxAreaBkd",
                Text = "Filter by Area Range",
                Font = groupBox3.Font,
                ForeColor = groupBox3.ForeColor,
                Location = groupBox3.Location,
                Size = groupBox3.Size,
                Margin = groupBox3.Margin,
                Padding = groupBox3.Padding,
                Visible = false
            };

            _rbBkdSqm = new RadioButton
            {
                Name = "rbBkdSqm",
                Text = "sq.m.",
                AutoSize = true,
                Font = rbSqm.Font,
                ForeColor = rbSqm.ForeColor
            };

            _rbBigha = new RadioButton
            {
                Name = "rbBigha",
                Text = "Bigha",
                AutoSize = true,
                Font = rbSqm.Font,
                ForeColor = rbSqm.ForeColor
            };

            _rbKattha = new RadioButton
            {
                Name = "rbKattha",
                Text = "Kattha",
                AutoSize = true,
                Font = rbSqm.Font,
                ForeColor = rbSqm.ForeColor
            };

            _rbDhur = new RadioButton
            {
                Name = "rbDhur",
                Text = "Dhur",
                AutoSize = true,
                Font = rbSqm.Font,
                ForeColor = rbSqm.ForeColor
            };

            _txtFromAreaBkd = new TextBox
            {
                Name = "txtFromAreaBkd",
                BorderStyle = txtFromArea.BorderStyle,
                Font = txtFromArea.Font,
                Location = txtFromArea.Location,
                Size = txtFromArea.Size,
                PlaceholderText = "sq.m.",
                TextAlign = txtFromArea.TextAlign
            };

            _txtToAreaBkd = new TextBox
            {
                Name = "txtToAreaBkd",
                BorderStyle = txtToArea.BorderStyle,
                Font = txtToArea.Font,
                Location = txtToArea.Location,
                Size = txtToArea.Size,
                PlaceholderText = "sq.m.",
                TextAlign = txtToArea.TextAlign
            };

            var lblToBkd = new Label
            {
                Name = "labelAreaToBkd",
                Text = label8.Text,
                AutoSize = true,
                Font = label8.Font,
                ForeColor = label8.ForeColor,
                Location = label8.Location
            };

            _groupBoxAreaBkd.Controls.Add(_rbBkdSqm);
            _groupBoxAreaBkd.Controls.Add(_rbBigha);
            _groupBoxAreaBkd.Controls.Add(_rbKattha);
            _groupBoxAreaBkd.Controls.Add(_rbDhur);
            _groupBoxAreaBkd.Controls.Add(_txtFromAreaBkd);
            _groupBoxAreaBkd.Controls.Add(_txtToAreaBkd);
            _groupBoxAreaBkd.Controls.Add(lblToBkd);
            _groupBoxAreaBkd.Resize += GroupBoxAreaBkd_Resize;
            LayoutBkdAreaRadioButtons();
            panel1.Controls.Add(_groupBoxAreaBkd);
            _groupBoxAreaBkd.BringToFront();
        }

        private void GroupBoxAreaBkd_Resize(object? sender, EventArgs e)
        {
            LayoutBkdAreaRadioButtons();
        }

        private void LayoutBkdAreaRadioButtons()
        {
            if (_groupBoxAreaBkd == null ||
                _rbBkdSqm == null ||
                _rbBigha == null ||
                _rbKattha == null ||
                _rbDhur == null)
            {
                return;
            }

            var radios = new[] { _rbBkdSqm, _rbBigha, _rbKattha, _rbDhur };
            const int leftPadding = 16;
            const int rightPadding = 12;
            const int top = 27;
            const int minGap = 10;

            int totalWidth = radios.Sum(r => r.PreferredSize.Width);
            int availableWidth = _groupBoxAreaBkd.ClientSize.Width - leftPadding - rightPadding;
            int calculatedGap = (availableWidth - totalWidth) / Math.Max(1, radios.Length - 1);
            int gap = Math.Max(minGap, calculatedGap);

            int x = leftPadding;
            foreach (var rb in radios)
            {
                rb.Location = new Point(x, top);
                x += rb.PreferredSize.Width + gap;
            }
        }

        private void ApplyTraditionalAreaFilterGroupVisibility()
        {
            bool showBkd = string.Equals(_traditionalAreaUnit, "BKD", StringComparison.OrdinalIgnoreCase);
            groupBox3.Visible = !showBkd;

            if (_groupBoxAreaBkd != null)
            {
                _groupBoxAreaBkd.Visible = showBkd;
            }
        }

        private void UpdateAreaFilterPlaceholders()
        {
            if (rbRopanee.Checked)
            {
                txtFromArea.PlaceholderText = "Ropani";
                txtToArea.PlaceholderText = "Ropani";
            }
            else if (rbAana.Checked)
            {
                txtFromArea.PlaceholderText = "Aana";
                txtToArea.PlaceholderText = "Aana";
            }
            else
            {
                txtFromArea.PlaceholderText = "sq.m.";
                txtToArea.PlaceholderText = "sq.m.";
            }

            if (_txtFromAreaBkd == null || _txtToAreaBkd == null)
                return;

            if (_rbBigha?.Checked == true)
            {
                _txtFromAreaBkd.PlaceholderText = "Bigha";
                _txtToAreaBkd.PlaceholderText = "Bigha";
            }
            else if (_rbKattha?.Checked == true)
            {
                _txtFromAreaBkd.PlaceholderText = "Kattha";
                _txtToAreaBkd.PlaceholderText = "Kattha";
            }
            else if (_rbDhur?.Checked == true)
            {
                _txtFromAreaBkd.PlaceholderText = "Dhur";
                _txtToAreaBkd.PlaceholderText = "Dhur";
            }
            else
            {
                _txtFromAreaBkd.PlaceholderText = "sq.m.";
                _txtToAreaBkd.PlaceholderText = "sq.m.";
            }
        }

        private TextBox GetActiveFromAreaTextBox()
        {
            return string.Equals(_traditionalAreaUnit, "BKD", StringComparison.OrdinalIgnoreCase) &&
                   _txtFromAreaBkd != null
                ? _txtFromAreaBkd
                : txtFromArea;
        }

        private TextBox GetActiveToAreaTextBox()
        {
            return string.Equals(_traditionalAreaUnit, "BKD", StringComparison.OrdinalIgnoreCase) &&
                   _txtToAreaBkd != null
                ? _txtToAreaBkd
                : txtToArea;
        }

        #endregion

        #region Data Loading

        private void frmLandParcelOwnersRecord_Load(object sender, EventArgs e)
        {
            _traditionalAreaUnit = _landRecordsService.GetTraditionalAreaUnit();
            ApplyTraditionalAreaUnitColumns();
            ApplyTraditionalAreaFilterGroupVisibility();
            UpdateAreaFilterPlaceholders();

            LoadAllRecords();
            PopulateFilterDropdowns();
            CaptureFilterCriteriaFromControls();
            CaptureSearchCriteriaFromControls();
            ApplyCurrentCriteria(showValidationMessage: false);
            UpdateButtonStates();
        }

        private void LoadAllRecords()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _allRecords = _landRecordsService.GetAllParcelsWithOwners();
                _filteredRecords = [.. _allRecords];

                CacheUniqueValues();
                BindRecordsToGrid(_filteredRecords);
                UpdateStatusLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load records: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _allRecords = [];
                _filteredRecords = [];
                BindRecordsToGrid(_filteredRecords);
                UpdateStatusLabels();
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void CacheUniqueValues()
        {
            _uniqueProvinces = [.. _allRecords.Select(r => r.Province ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueDistricts = [.. _allRecords.Select(r => r.District ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueMunicipalities = [.. _allRecords.Select(r => r.MunicipalityVillage ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueWards = [.. _allRecords.Select(r => r.WardNo ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueMapSheets = [.. _allRecords.Select(r => r.MapSheetNo ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];
            _uniqueOwnershipTypes = [.. _allRecords.Select(r => r.LandOwnershipType ?? "").Where(v => !string.IsNullOrWhiteSpace(v)).Distinct()];

            // Calculate max area for filtering
            _maxAreaSqm = _allRecords.Where(r => r.AreaInSqm.HasValue).Select(r => r.AreaInSqm!.Value).DefaultIfEmpty(0).Max();
        }

        private void PopulateFilterDropdowns()
        {
            _isUpdatingControls = true;
            try
            {
                PopulateComboBox(cbProvince, _uniqueProvinces, "-- All Provinces --");
                PopulateComboBox(cbDistrict, _uniqueDistricts, "-- All Districts --");
                PopulateComboBox(cbMunicipalityVillage, _uniqueMunicipalities, "-- All Municipalities --");
                PopulateComboBox(cbWardNo, _uniqueWards, "-- All --");
                PopulateComboBox(cbMapSheet, _uniqueMapSheets, "-- All Map Sheets --");
                PopulateComboBox(cbLandOwnership, _uniqueOwnershipTypes, "-- All Types --");
            }
            finally
            {
                _isUpdatingControls = false;
            }
        }

        private static void PopulateComboBox(ComboBox comboBox, HashSet<string> values, string defaultText)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add(defaultText);

            // If only one unique value, disable the combobox
            if (values.Count <= 1)
            {
                comboBox.Enabled = false;
                if (values.Count == 1)
                {
                    comboBox.Items.Add(values.First());
                }
            }
            else
            {
                comboBox.Enabled = true;
                foreach (var value in values.OrderBy(v => v))
                {
                    comboBox.Items.Add(value);
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private void BindRecordsToGrid(List<OriginalLandParcel> records)
        {
            // Sort by MapSheet first, then by ParcelNo
            var sortedRecords = records
                .OrderBy(r => r.MapSheetNo)
                .ThenBy(r => r.ParcelNo, new NaturalStringComparer())
                .ToList();

            var displayModels = sortedRecords.Select(r => new LandParcelDisplayModel
            {
                ParcelId = r.ParcelId,
                LandOwnerId = r.LandOwnerId,
                ParcelNo = r.ParcelNo,
                MapSheetNo = r.MapSheetNo,
                ParcelLocation = r.ParcelLocation ?? "",
                Province = r.Province ?? "",
                District = r.District ?? "",
                MunicipalityVillage = r.MunicipalityVillage ?? "",
                WardNo = r.WardNo ?? "",
                LandOwnersName = r.Owner?.LandOwnersName ?? "",
                FatherSpouse = r.Owner?.FatherSpouse ?? "",
                Gender = r.Owner?.Gender ?? "",
                CitizenshipNumber = r.Owner?.CitizenshipNumber ?? "",
                PermanentAddress = r.Owner?.PermanentAddress ?? "",
                AreaInSqm = r.AreaInSqm,
                FieldMeasuredAreaSqm = r.FieldMeasuredAreaSqm,
                AreaInRAPD = r.AreaInRAPD ?? "",
                AreaInBKD = r.AreaInBKD ?? "",
                LandOwnershipType = r.LandOwnershipType ?? "",
                LandUse = r.LandUse ?? "",
                IsTenant = r.IsTenant ?? "",
                TenantName = r.TenantName ?? "",
                MothNo = r.MothNo ?? "",
                PaanaNo = r.PaanaNo ?? "",
                Remarks = r.Remarks ?? ""
            }).ToList();

            _displayedRecords = new BindingList<LandParcelDisplayModel>(displayModels);
            dgvRecords.DataSource = _displayedRecords;
        }

        #endregion

        #region Filtering Logic

        private void ApplyFilters()
        {
            ApplyCurrentCriteria(showValidationMessage: true);
        }

        private void ApplyCurrentCriteria(bool showValidationMessage)
        {
            var filtered = _allRecords.AsEnumerable();

            // Location filters
            filtered = ApplyComboFilter(filtered, _appliedFilter.Province, r => r.Province);
            filtered = ApplyComboFilter(filtered, _appliedFilter.District, r => r.District);
            filtered = ApplyComboFilter(filtered, _appliedFilter.MunicipalityVillage, r => r.MunicipalityVillage);
            filtered = ApplyComboFilter(filtered, _appliedFilter.WardNo, r => r.WardNo);

            // Map sheet filter
            filtered = ApplyComboFilter(filtered, _appliedFilter.MapSheetNo, r => r.MapSheetNo);

            // Ownership filter
            filtered = ApplyComboFilter(filtered, _appliedFilter.LandOwnershipType, r => r.LandOwnershipType);

            // Area range filter (returns null if validation fails)
            var areaFiltered = ApplyAreaFilter(filtered, _appliedFilter, showValidationMessage);
            if (areaFiltered == null)
                return; // Stop filtering if area validation failed

            filtered = areaFiltered;

            // Search filters
            filtered = ApplySearchFiltersToRecords(filtered, _appliedSearch);

            _filteredRecords = [.. filtered];
            BindRecordsToGrid(_filteredRecords);
            UpdateStatusLabels();
        }

        private static IEnumerable<OriginalLandParcel> ApplyComboFilter(
            IEnumerable<OriginalLandParcel> records,
            string? selectedValue,
            Func<OriginalLandParcel, string?> selector)
        {
            if (string.IsNullOrWhiteSpace(selectedValue))
                return records;

            return records.Where(r =>
                string.Equals(selector(r) ?? string.Empty, selectedValue, StringComparison.OrdinalIgnoreCase));
        }

        private IEnumerable<OriginalLandParcel>? ApplyAreaFilter(
            IEnumerable<OriginalLandParcel> records,
            FilterCriteria criteria,
            bool showValidationMessage)
        {
            // Skip if both fields are empty
            if (!criteria.FromArea.HasValue && !criteria.ToArea.HasValue)
                return records;

            double maxInSelectedUnit = ConvertSqmToSelectedAreaUnit(_maxAreaSqm, criteria.AreaUnit);
            double fromArea = criteria.FromArea ?? 0;
            double toArea = criteria.ToArea ?? (maxInSelectedUnit > 0 ? maxInSelectedUnit : double.MaxValue);
            string unitName = GetAreaUnitDisplayName(criteria.AreaUnit);

            // Validate: From area should not be greater than To area
            if (fromArea > 0 && toArea < double.MaxValue && fromArea > toArea)
            {
                if (showValidationMessage)
                {
                    string fromDisplay = fromArea.ToString("0.###", CultureInfo.InvariantCulture);
                    string toDisplay = toArea.ToString("0.###", CultureInfo.InvariantCulture);

                    MessageBox.Show(
                        $"'From Area' ({fromDisplay} {unitName}) cannot be greater than 'To Area' ({toDisplay} {unitName}).\n\nPlease correct the area range.",
                        "Invalid Area Range",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return null; // Return null to indicate validation failed
                }

                return records; // Ignore invalid area range in quick mode
            }

            // Skip filtering if range covers all
            if (fromArea <= 0 && toArea >= maxInSelectedUnit)
                return records;

            return records.Where(r =>
            {
                double? areaSqm = AreaConverterService.GetAreaInSqm(r.AreaInSqm, r.AreaInRAPD, r.AreaInBKD);
                if (!areaSqm.HasValue) return false;

                // Convert the parcel area to the selected unit
                double areaInSelectedUnit = ConvertSqmToSelectedAreaUnit(areaSqm.Value, criteria.AreaUnit);

                return areaInSelectedUnit >= fromArea && areaInSelectedUnit <= toArea;
            });
        }

        private static IEnumerable<OriginalLandParcel> ApplySearchFiltersToRecords(
            IEnumerable<OriginalLandParcel> records,
            SearchCriteria criteria)
        {
            string parcelSearch = criteria.ParcelNo.Trim();
            string ownerSearch = criteria.OwnerSearchText.Trim();

            // Exact match for parcel number
            if (!string.IsNullOrWhiteSpace(parcelSearch))
            {
                records = records.Where(r => r.ParcelNo?.Equals(parcelSearch, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Partial match for owner name (contains search)
            if (!string.IsNullOrWhiteSpace(ownerSearch))
            {
                records = records.Where(r =>
                    r.Owner?.LandOwnersName?.Contains(ownerSearch, StringComparison.OrdinalIgnoreCase) == true ||
                    r.Owner?.FatherSpouse?.Contains(ownerSearch, StringComparison.OrdinalIgnoreCase) == true);
            }

            return records;
        }

        /// <summary>
        /// Standalone search filter application (for quick search toggle)
        /// </summary>
        private void ApplySearchFilters()
        {
            ApplyCurrentCriteria(showValidationMessage: true);
        }

        private void CaptureFilterCriteriaFromControls()
        {
            _appliedFilter.Province = GetSelectedComboValue(cbProvince);
            _appliedFilter.District = GetSelectedComboValue(cbDistrict);
            _appliedFilter.MunicipalityVillage = GetSelectedComboValue(cbMunicipalityVillage);
            _appliedFilter.WardNo = GetSelectedComboValue(cbWardNo);
            _appliedFilter.MapSheetNo = GetSelectedComboValue(cbMapSheet);
            _appliedFilter.LandOwnershipType = GetSelectedComboValue(cbLandOwnership);
            var fromAreaTextBox = GetActiveFromAreaTextBox();
            var toAreaTextBox = GetActiveToAreaTextBox();
            _appliedFilter.FromArea = TryParseAreaInput(fromAreaTextBox.Text, out var fromValue) ? fromValue : null;
            _appliedFilter.ToArea = TryParseAreaInput(toAreaTextBox.Text, out var toValue) ? toValue : null;
            _appliedFilter.AreaUnit = GetSelectedAreaUnit();
        }

        private void CaptureSearchCriteriaFromControls()
        {
            _appliedSearch.ParcelNo = txtParcelNo.Text.Trim();
            _appliedSearch.OwnerSearchText = txtLandOwner.Text.Trim();
        }

        private static string? GetSelectedComboValue(ComboBox comboBox)
        {
            if (!comboBox.Enabled || comboBox.SelectedIndex <= 0)
                return null;

            var value = comboBox.SelectedItem?.ToString()?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private AreaUnit GetSelectedAreaUnit()
        {
            if (string.Equals(_traditionalAreaUnit, "BKD", StringComparison.OrdinalIgnoreCase))
            {
                if (_rbBigha?.Checked == true)
                    return AreaUnit.Bigha;
                if (_rbKattha?.Checked == true)
                    return AreaUnit.Kattha;
                if (_rbDhur?.Checked == true)
                    return AreaUnit.Dhur;
                return AreaUnit.Sqm;
            }

            if (rbRopanee.Checked)
                return AreaUnit.Ropani;
            if (rbAana.Checked)
                return AreaUnit.Aana;
            return AreaUnit.Sqm;
        }

        private static bool TryParseAreaInput(string? text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var normalized = text.Trim();
            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.CurrentCulture, out value) && value >= 0)
                return true;

            if (double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value >= 0)
                return true;

            var swapped = normalized.Replace(',', '.');
            if (double.TryParse(swapped, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value >= 0)
                return true;

            value = 0;
            return false;
        }

        private static string GetAreaUnitDisplayName(AreaUnit areaUnit)
        {
            return areaUnit switch
            {
                AreaUnit.Ropani => "Ropani",
                AreaUnit.Aana => "Aana",
                AreaUnit.Bigha => "Bigha",
                AreaUnit.Kattha => "Kattha",
                AreaUnit.Dhur => "Dhur",
                _ => "sq.m."
            };
        }

        private static double ConvertSqmToSelectedAreaUnit(double areaSqm, AreaUnit areaUnit)
        {
            return areaUnit switch
            {
                AreaUnit.Ropani => AreaConverterService.SqmToRopani(areaSqm),
                AreaUnit.Aana => AreaConverterService.SqmToAana(areaSqm),
                AreaUnit.Bigha => AreaConverterService.SqmToBigha(areaSqm),
                AreaUnit.Kattha => AreaConverterService.SqmToKattha(areaSqm),
                AreaUnit.Dhur => AreaConverterService.SqmToDhur(areaSqm),
                _ => areaSqm
            };
        }

        private void RefreshTraditionalAreaSettingsFromProject()
        {
            string latestUnit = _landRecordsService.GetTraditionalAreaUnit();
            if (string.Equals(_traditionalAreaUnit, latestUnit, StringComparison.OrdinalIgnoreCase))
                return;

            _traditionalAreaUnit = latestUnit;
            ApplyTraditionalAreaUnitColumns();
            ApplyTraditionalAreaFilterGroupVisibility();
            UpdateAreaFilterPlaceholders();
            CaptureFilterCriteriaFromControls();
            ApplyCurrentCriteria(showValidationMessage: false);
        }

        private void UpdateApplyButtonStates()
        {
            btnApplyFilter.Enabled = !chkToggleQuickFilter.Checked;
            btnApplySearch.Enabled = !chkToggleQuickSearch.Checked;
        }

        private void ClearFilters()
        {
            _isUpdatingControls = true;
            try
            {
                // Reset all comboboxes to first item
                if (cbProvince.Items.Count > 0) cbProvince.SelectedIndex = 0;
                if (cbDistrict.Items.Count > 0) cbDistrict.SelectedIndex = 0;
                if (cbMunicipalityVillage.Items.Count > 0) cbMunicipalityVillage.SelectedIndex = 0;
                if (cbWardNo.Items.Count > 0) cbWardNo.SelectedIndex = 0;
                if (cbMapSheet.Items.Count > 0) cbMapSheet.SelectedIndex = 0;
                if (cbLandOwnership.Items.Count > 0) cbLandOwnership.SelectedIndex = 0;

                // Clear area filter text boxes
                txtFromArea.Clear();
                txtToArea.Clear();
                if (_txtFromAreaBkd != null)
                {
                    _txtFromAreaBkd.Clear();
                }
                if (_txtToAreaBkd != null)
                {
                    _txtToAreaBkd.Clear();
                }

                // Reset radio button
                rbSqm.Checked = true;
                if (_rbBkdSqm != null)
                {
                    _rbBkdSqm.Checked = true;
                }
            }
            finally
            {
                _isUpdatingControls = false;
            }

            CaptureFilterCriteriaFromControls();
            ApplyCurrentCriteria(showValidationMessage: false);
        }

        private void ClearSearch()
        {
            _isUpdatingControls = true;
            try
            {
                // Clear search text boxes
                txtParcelNo.Clear();
                txtLandOwner.Clear();
            }
            finally
            {
                _isUpdatingControls = false;
            }

            CaptureSearchCriteriaFromControls();
            ApplyCurrentCriteria(showValidationMessage: false);
        }

        private void ClearAllFilters()
        {
            ClearFilters();
            ClearSearch();
        }

        #endregion

        #region Input Validation

        private void AreaTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow: digits, decimal point, backspace, delete
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
                return;

            // Allow single decimal point
            if (e.KeyChar == '.' && sender is TextBox textBox && !textBox.Text.Contains('.'))
                return;

            e.Handled = true; // Block all other characters
        }

        private void ParcelNo_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow: digits, backspace
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '\b')
                return;

            e.Handled = true;
        }

        private void LandOwnerName_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow control characters (backspace, etc.)
            if (char.IsControl(e.KeyChar))
                return;

            // Allow all Unicode letters (including Devanagari U+0900-U+097F)
            if (char.IsLetter(e.KeyChar))
                return;

            // Allow whitespace
            if (char.IsWhiteSpace(e.KeyChar))
                return;

            // Allow common name punctuation
            if (e.KeyChar == '.' || e.KeyChar == '\'' || e.KeyChar == '-')
                return;

            // Allow Devanagari Unicode range (U+0900 to U+097F) including vowel signs, matras, etc.
            if (e.KeyChar >= '\u0900' && e.KeyChar <= '\u097F')
                return;

            // Allow Devanagari Extended range (U+A8E0 to U+A8FF)
            if (e.KeyChar >= '\uA8E0' && e.KeyChar <= '\uA8FF')
                return;

            // Block other characters
            e.Handled = true;
        }

        #endregion

        #region Event Handlers

        private void BtnApplyFilter_Click(object? sender, EventArgs e)
        {
            CaptureFilterCriteriaFromControls();
            ApplyFilters();
        }

        private void BtnClearFilter_Click(object? sender, EventArgs e)
        {
            ClearFilters();
        }

        private void BtnApplySearch_Click(object? sender, EventArgs e)
        {
            CaptureSearchCriteriaFromControls();
            ApplySearchFilters();
        }

        private void BtnClearSearch_Click(object? sender, EventArgs e)
        {
            ClearSearch();
        }

        private void ChkToggleQuickFilter_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateApplyButtonStates();

            if (chkToggleQuickFilter.Checked)
            {
                CaptureFilterCriteriaFromControls();
                ApplyCurrentCriteria(showValidationMessage: false);
            }
        }

        private void ChkToggleQuickSearch_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateApplyButtonStates();

            if (chkToggleQuickSearch.Checked)
            {
                CaptureSearchCriteriaFromControls();
                ApplyCurrentCriteria(showValidationMessage: false);
            }
        }

        private void FrmLandParcelOwnersRecord_Activated(object? sender, EventArgs e)
        {
            RefreshTraditionalAreaSettingsFromProject();
        }

        private void ComboFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingControls)
                return;

            if (chkToggleQuickFilter.Checked)
            {
                CaptureFilterCriteriaFromControls();
                ApplyCurrentCriteria(showValidationMessage: false);
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingControls)
                return;

            if (chkToggleQuickSearch.Checked)
            {
                CaptureSearchCriteriaFromControls();
                ApplyCurrentCriteria(showValidationMessage: false);
            }
        }

        private void TxtArea_TextChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingControls)
                return;

            if (chkToggleQuickFilter.Checked)
            {
                CaptureFilterCriteriaFromControls();
                ApplyCurrentCriteria(showValidationMessage: false);
            }
        }

        private void RadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingControls)
                return;

            UpdateAreaFilterPlaceholders();
            if (chkToggleQuickFilter.Checked)
            {
                CaptureFilterCriteriaFromControls();
                ApplyCurrentCriteria(showValidationMessage: false);
            }
        }

        private void DgvRecords_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void DgvRecords_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            EditSelectedRecord();
        }

        private void DgvRecords_RowPostPaint(object? sender, DataGridViewRowPostPaintEventArgs e)
        {
            string rowNumber = (e.RowIndex + 1).ToString();

            var headerBounds = new Rectangle(
                e.RowBounds.Left,
                e.RowBounds.Top,
                dgvRecords.RowHeadersWidth - 4,
                e.RowBounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                rowNumber,
                dgvRecords.DefaultCellStyle.Font,
                headerBounds,
                dgvRecords.RowHeadersDefaultCellStyle.ForeColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }

        private void DgvRecords_Leave(object? sender, EventArgs e)
        {
            dgvRecords.ClearSelection();
        }

        private void DgvRecords_MouseLeave(object? sender, EventArgs e)
        {
            dgvRecords.ClearSelection();
        }

        #endregion

        #region CRUD Operations

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var addForm = new frmAddEditRecord(_landRecordsService.ParcelExists, ownerFieldsReadOnly: true);
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var record = addForm.Record;
                    if (!SaveNewRecord(record))
                    {
                        return;
                    }
                    LoadAllRecords();
                    PopulateFilterDropdowns();
                    CaptureFilterCriteriaFromControls();
                    CaptureSearchCriteriaFromControls();
                    ApplyFilters();

                    MessageBox.Show("Record added successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add record: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count != 1) return;
            EditSelectedRecord();
        }

        private void EditSelectedRecord()
        {
            if (dgvRecords.SelectedRows[0].DataBoundItem is not LandParcelDisplayModel model)
                return;

            var parcel = _allRecords.FirstOrDefault(r => r.ParcelId == model.ParcelId);
            if (parcel == null) return;

            var record = ConvertToEditableRecord(parcel);

            using var editForm = new frmAddEditRecord(record, model.ParcelId, _landRecordsService.ParcelExists, ownerFieldsReadOnly: true);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (editForm.IsDeleted)
                    {
                        _landRecordsService.DeleteParcel(model.ParcelId);
                        MessageBox.Show("Record deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var updatedRecord = editForm.Record;
                        if (!UpdateExistingRecord(model.ParcelId, model.LandOwnerId, updatedRecord))
                        {
                            return;
                        }
                        MessageBox.Show("Record updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    LoadAllRecords();
                    PopulateFilterDropdowns();
                    CaptureFilterCriteriaFromControls();
                    CaptureSearchCriteriaFromControls();
                    ApplyFilters();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to update record: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvRecords.SelectedRows.Count == 0) return;

            var result = MessageBox.Show(
                "Are you sure you want to delete the selected record?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            try
            {
                if (dgvRecords.SelectedRows[0].DataBoundItem is LandParcelDisplayModel model)
                {
                    _landRecordsService.DeleteParcel(model.ParcelId);
                    LoadAllRecords();
                    PopulateFilterDropdowns();
                    CaptureFilterCriteriaFromControls();
                    CaptureSearchCriteriaFromControls();
                    ApplyFilters();

                    MessageBox.Show("Record deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadAllRecords();
            PopulateFilterDropdowns();
            CaptureFilterCriteriaFromControls();
            CaptureSearchCriteriaFromControls();
            ClearAllFilters();
        }

        private void BtnViewLandOwnerDetails_Click(object? sender, EventArgs e)
        {
            ViewLandOwnerDetails();
        }

        private void ViewLandOwnerDetails()
        {
            if (dgvRecords.SelectedRows.Count != 1)
            {
                MessageBox.Show("Please select a single record to view details.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvRecords.SelectedRows[0].DataBoundItem is not LandParcelDisplayModel model)
                return;

            var parcel = _allRecords.FirstOrDefault(r => r.ParcelId == model.ParcelId);
            if (parcel == null) return;

            using var detailsForm = new frmLandOwnerDetails(
                parcel.LandOwnerId,
                readOnlyMode: true,
                _landRecordsService,
                _projectPath,
                allowEditInReadOnly: false);
            detailsForm.ShowDialog();

            // Refresh after details form closes in case changes were made
            LoadAllRecords();
            PopulateFilterDropdowns();
            CaptureFilterCriteriaFromControls();
            CaptureSearchCriteriaFromControls();
            ApplyFilters();
        }

        #endregion

        #region Helper Methods

        private bool SaveNewRecord(BaselineLandParcelRecord record)
        {
            var records = new List<BaselineLandParcelRecord> { record };

            // Step 1: Run owner deduplication on the single record
            var deduplicationResult = OwnerDeduplicationService.ExtractUniqueOwners(records, excludeAnonymous: false);

            // Step 2: If there are duplicates needing review, show the review form
            if (deduplicationResult.DuplicatesNeedingReview.Count > 0)
            {
                var bindingList = new BindingList<BaselineLandParcelRecord>(records);
                using var reviewForm = new frmReviewDuplicates(deduplicationResult, bindingList);

                if (reviewForm.ShowDialog() != DialogResult.OK)
                {
                    // User cancelled the review
                    return false;
                }

                // The deduplicationResult was modified in-place by the review form
                // No need to retrieve it - just use the same reference
            }

            // Step 3: Save using the deduplication result
            var parcelToOwnerMap = _landRecordsService.SaveUniqueOwnersFromDeduplication(deduplicationResult);
            int savedCount = _landRecordsService.SaveParcelsWithDeduplication(records, parcelToOwnerMap);

            if (savedCount == 0)
            {
                throw new Exception("Failed to save parcel - it may be a duplicate.");
            }

            return true;
        }

        private bool UpdateExistingRecord(int parcelId, int existingLandOwnerId, BaselineLandParcelRecord record)
        {
            // Step 1: Check if owner information has changed
            var existingParcel = _allRecords.FirstOrDefault(p => p.ParcelId == parcelId);
            bool ownerChanged = false;

            if (existingParcel?.Owner != null)
            {
                ownerChanged = existingParcel.Owner.LandOwnersName != (record.LandOwnersName ?? "") ||
                               existingParcel.Owner.FatherSpouse != record.FatherSpouse ||
                               existingParcel.Owner.CitizenshipNumber != record.CitizenshipNumber;
            }

            int landOwnerIdToUse = existingLandOwnerId;

            if (ownerChanged)
            {
                // Step 2: Run owner deduplication to find if this owner already exists
                var records = new List<BaselineLandParcelRecord> { record };
                var deduplicationResult = OwnerDeduplicationService.ExtractUniqueOwners(records, excludeAnonymous: false);

                // Step 3: If there are duplicates needing review, show the review form
                if (deduplicationResult.DuplicatesNeedingReview.Count > 0)
                {
                    var bindingList = new BindingList<BaselineLandParcelRecord>(records);
                    using var reviewForm = new frmReviewDuplicates(deduplicationResult, bindingList);

                    if (reviewForm.ShowDialog() != DialogResult.OK)
                    {
                        // User cancelled the review
                        return false;
                    }

                    // The deduplicationResult was modified in-place by the review form
                }

                // Step 4: Save or get the owner ID
                var parcelToOwnerMap = _landRecordsService.SaveUniqueOwnersFromDeduplication(deduplicationResult);

                // The first (and only) record in our list is at index 0
                if (parcelToOwnerMap.TryGetValue(0, out int newOwnerId))
                {
                    landOwnerIdToUse = newOwnerId;
                }
                else
                {
                    throw new Exception("Failed to get owner ID after deduplication.");
                }
            }
            else
            {
                // Step 5: Owner info hasn't changed, just update the existing owner record
                var owner = new LandOwner
                {
                    LandOwnerId = existingLandOwnerId,
                    LandOwnersName = record.LandOwnersName ?? "",
                    FatherSpouse = record.FatherSpouse,
                    Gender = record.Gender,
                    CitizenshipNumber = record.CitizenshipNumber,
                    CitizenshipIssuedDistrict = record.CitizenshipIssuedDistrict,
                    CitizenshipIssuedDate = record.CitizenshipIssuedDate, // lowercase 'c' property
                    PermanentAddress = record.PermanentAddress,
                    TemporaryAddress = record.TemporaryAddress, // Note: typo in model - 'Tempoary'
                    ContactNumber = record.ContactNumber,
                    EmailID = record.EmailID,
                    ModifiedDate = DateTime.Now
                };
                _landRecordsService.UpdateOwner(owner);
            }

            // Step 6: Update the parcel with the correct owner ID
            var parcel = new OriginalLandParcel
            {
                ParcelId = parcelId,
                LandOwnerId = landOwnerIdToUse,
                ParcelNo = record.ParcelNo ?? "",
                MapSheetNo = record.MapSheetNo ?? "",
                Province = record.Province,
                District = record.District,
                MunicipalityVillage = record.MunicipalityVillage,
                WardNo = record.WardNo,
                ParcelLocation = record.ParcelLocation,
                IsTenant = record.Tenant,
                TenantName = record.TenantName,
                LandUse = record.LandUse,
                LandOwnershipType = record.LandOwnershipType,
                AreaInSqm = record.AreaInSqm,
                FieldMeasuredAreaSqm = record.FieldMeasuredAreaSqm,
                AreaInRAPD = record.AreaInRAPD,
                AreaInBKD = record.AreaInBKD,
                MothNo = record.MothNo,
                PaanaNo = record.PaanaNo,
                Remarks = record.Remarks,
                JointCoOwners = record.JointCoOwners
            };
            _landRecordsService.UpdateParcel(parcel);

            return true;
        }

        private static BaselineLandParcelRecord ConvertToEditableRecord(OriginalLandParcel parcel)
        {
            return new BaselineLandParcelRecord
            {
                ParcelNo = parcel.ParcelNo,
                MapSheetNo = parcel.MapSheetNo,
                Province = parcel.Province,
                District = parcel.District,
                MunicipalityVillage = parcel.MunicipalityVillage,
                WardNo = parcel.WardNo,
                ParcelLocation = parcel.ParcelLocation,
                LandOwnersName = parcel.Owner?.LandOwnersName,
                FatherSpouse = parcel.Owner?.FatherSpouse,
                Gender = parcel.Owner?.Gender,
                CitizenshipNumber = parcel.Owner?.CitizenshipNumber,
                PermanentAddress = parcel.Owner?.PermanentAddress,
                Tenant = parcel.IsTenant,
                TenantName = parcel.TenantName,
                LandUse = parcel.LandUse,
                LandOwnershipType = parcel.LandOwnershipType,
                AreaInSqm = parcel.AreaInSqm,
                FieldMeasuredAreaSqm = parcel.FieldMeasuredAreaSqm,
                AreaInRAPD = parcel.AreaInRAPD,
                AreaInBKD = parcel.AreaInBKD,
                MothNo = parcel.MothNo,
                PaanaNo = parcel.PaanaNo,
                Remarks = parcel.Remarks,
                JointCoOwners = parcel.JointCoOwners
                    .Select(coOwner => new CoOwnerRecord
                    {
                        OwnerName = coOwner.OwnerName,
                        FatherSpouse = coOwner.FatherSpouse,
                        Gender = coOwner.Gender,
                        CitizenshipNumber = coOwner.CitizenshipNumber,
                        CitizenshipIssuedDistrict = coOwner.CitizenshipIssuedDistrict,
                        CitizenshipIssuedDate = coOwner.CitizenshipIssuedDate,
                        PermanentAddress = coOwner.PermanentAddress,
                        TemporaryAddress = coOwner.TemporaryAddress,
                        ContactNumber = coOwner.ContactNumber,
                        EmailID = coOwner.EmailID,
                        OwnershipSharePercent = coOwner.OwnershipSharePercent
                    })
                    .ToList()
            };
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = dgvRecords.SelectedRows.Count == 1;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
            toolStripButton1.Enabled = hasSelection;
        }

        private void UpdateStatusLabels()
        {
            lblTotalRecords.Text = $"Total Records: {_allRecords.Count}";
            lblFilteredRecords.Text = $"Filtered Records: {_filteredRecords.Count}";
            lblSelectedRecords.Text = $"Selected: {dgvRecords.SelectedRows.Count}";
        }

        #endregion

        #region Unused Designer Events

        private void label2_Click(object sender, EventArgs e) { }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void radioButton3_CheckedChanged(object sender, EventArgs e) { }

        #endregion
    }

    #region Display Model

    /// <summary>
    /// Display model for land parcel records grid
    /// </summary>
    public class LandParcelDisplayModel
    {
        public int ParcelId { get; set; }
        public int LandOwnerId { get; set; }
        public string ParcelNo { get; set; } = "";
        public string MapSheetNo { get; set; } = "";
        public string ParcelLocation { get; set; } = "";
        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string MunicipalityVillage { get; set; } = "";
        public string WardNo { get; set; } = "";
        public string LandOwnersName { get; set; } = "";
        public string FatherSpouse { get; set; } = "";
        public string Gender { get; set; } = "";
        public string CitizenshipNumber { get; set; } = "";
        public string PermanentAddress { get; set; } = "";
        public double? AreaInSqm { get; set; }
        public double? FieldMeasuredAreaSqm { get; set; }
        public string AreaInRAPD { get; set; } = "";
        public string AreaInBKD { get; set; } = "";
        public string LandOwnershipType { get; set; } = "";
        public string LandUse { get; set; } = "";
        public string IsTenant { get; set; } = "";
        public string TenantName { get; set; } = "";
        public string MothNo { get; set; } = "";
        public string PaanaNo { get; set; } = "";
        public string Remarks { get; set; } = "";
    }

    #endregion

    #region Natural String Comparer

    /// <summary>
    /// Compares strings with natural sorting (e.g., "2" comes before "10")
    /// </summary>
    public class NaturalStringComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int xIndex = 0, yIndex = 0;

            while (xIndex < x.Length && yIndex < y.Length)
            {
                if (char.IsDigit(x[xIndex]) && char.IsDigit(y[yIndex]))
                {
                    // Extract numeric parts
                    string xNum = ExtractNumber(x, ref xIndex);
                    string yNum = ExtractNumber(y, ref yIndex);

                    // Compare as integers
                    if (int.TryParse(xNum, out int xInt) && int.TryParse(yNum, out int yInt))
                    {
                        int numCompare = xInt.CompareTo(yInt);
                        if (numCompare != 0) return numCompare;
                    }
                    else
                    {
                        // Fallback to string comparison if parsing fails
                        int strCompare = string.Compare(xNum, yNum, StringComparison.OrdinalIgnoreCase);
                        if (strCompare != 0) return strCompare;
                    }
                }
                else
                {
                    // Compare character by character
                    int charCompare = char.ToLower(x[xIndex]).CompareTo(char.ToLower(y[yIndex]));
                    if (charCompare != 0) return charCompare;
                    
                    xIndex++;
                    yIndex++;
                }
            }

            // If one string is longer, it comes after
            return x.Length.CompareTo(y.Length);
        }

        private static string ExtractNumber(string str, ref int index)
        {
            int start = index;
            while (index < str.Length && char.IsDigit(str[index]))
            {
                index++;
            }
            return str.Substring(start, index - start);
        }
    }

    #endregion
}

