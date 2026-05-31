using System.Globalization;
using System.Text.RegularExpressions;

namespace Land_Readjustment_Tool.UI.Forms
{
    public partial class frmLabelExpressionEditor : Form
    {
        // ── Entity / layer category constants ─────────────────────────────────
        private const string CatBaselineParcel  = "BaselineParcel";
        private const string CatRoad            = "Road";
        private const string CatBlock           = "Block";
        private const string CatReplottedParcel = "ReplottedParcel";
        private const string CatPolyline        = "Polyline";
        private const string CatPoint           = "Point";
        private const string CatText            = "Text";
        private const string CatGeneric         = "Generic";
        private const string NoDataPreviewText  = "NO DATA AVAILABLE";

        // ── Fields per entity / layer category ────────────────────────────────
        private static (string Display, string FieldKey)[] GetAvailableFields(string category) =>
            category switch
            {
                CatBaselineParcel =>
                [
                    // Parcel identification
                    ("Parcel No.",                       "ParcelNo"),
                    ("Map Sheet No.",                    "MapSheetNo"),
                    ("Full Parcel Code (Map-Parcel)",    "FullUniqueParcelCode"),
                    // Owner
                    ("Owner Name",                       "OwnerName"),
                    ("Father / Spouse Name",             "OwnerFatherSpouse"),
                    ("Ownership Type",                   "OwnershipType"),
                    ("Has Tenant",                       "HasTenant"),
                    ("Tenant Name",                      "TenantName"),
                    // Area — from records
                    ("Record Area (sq.m)",               "AreaSqm"),
                    ("Record Area — RAPD / Ropani",      "AreaRAPD"),
                    ("Record Area — BKD / Bigha",        "AreaBKD"),
                    ("Field Measured Area (sq.m)",       "FieldMeasuredAreaSqm"),
                    ("Effective Area (sq.m)",            "EffectiveAreaSqm"),
                    // Area — from map
                    ("Calculated Area (sq.m)",           "CalculatedAreaSqm"),
                    ("Perimeter (m)",                    "Perimeter"),
                    // Location
                    ("Province",                         "Province"),
                    ("District",                         "District"),
                    ("Municipality",                     "Municipality"),
                    ("Ward No.",                         "WardNo"),
                    ("Land Use",                         "LandUse"),
                    // Land record (Malpot)
                    ("Moth No.",                         "MothNo"),
                    ("Paana No.",                        "PaanaNo"),
                    // Status
                    ("Assignment Status",                "AssignmentStatus"),
                    // Object / layer
                    ("Label Text",                       "LabelText"),
                    ("Object Description",               "ObjectDescription"),
                    ("Layer Name",                       "LayerName"),
                    ("Object ID",                        "Id"),
                    ("Baseline Parcel ID",               "BaselineParcelId"),
                ],

                CatRoad =>
                [
                    // Road identity
                    ("Road Name",                        "RoadName"),
                    ("Road Code",                        "RoadCode"),
                    ("Road Status",                      "RoadStatus"),
                    ("Road Type",                        "RoadType"),
                    ("Surface Type",                     "SurfaceType"),
                    // Dimensions
                    ("Road Width (m)",                   "RoadWidth"),
                    ("Right of Way Width (m)",           "RightOfWayWidth"),
                    ("Length (m)",                       "Length"),
                    // Other
                    ("Description",                      "RoadDescription"),
                    ("Label Text",                       "LabelText"),
                    ("Layer Name",                       "LayerName"),
                    ("Object ID",                        "Id"),
                ],

                CatBlock =>
                [
                    // Block identity
                    ("Block Name",                       "BlockName"),
                    ("Block Code",                       "BlockCode"),
                    ("Block Land Use",                   "BlockLandUse"),
                    ("Block Depth (m)",                  "BlockDepth"),
                    // Area
                    ("Block Area (sq.m)",                "BlockAreaSqm"),
                    ("Block Area — RAPD / Ropani",       "BlockAreaRAPD"),
                    ("Block Area — BKD / Bigha",         "BlockAreaBKD"),
                    ("Perimeter (m)",                    "Perimeter"),
                    // Other
                    ("Description",                      "BlockDescription"),
                    ("Label Text",                       "LabelText"),
                    ("Layer Name",                       "LayerName"),
                    ("Object ID",                        "Id"),
                ],

                CatReplottedParcel =>
                [
                    // Plot number
                    ("Plot No. (Active)",                "ReplottedParcelNo"),
                    ("System Generated No.",             "SystemGeneratedNumber"),
                    ("Derived No.",                      "DerivedNumber"),
                    ("Block Sequence No.",               "BlockSequenceNumber"),
                    // Classification
                    ("Plot Type",                        "PlotTypeName"),
                    ("Block Name",                       "PlotBlockName"),
                    // Area
                    ("Plot Area (sq.m)",                 "PlotAreaSqm"),
                    ("Plot Area — RAPD / Ropani",        "PlotAreaRAPD"),
                    ("Plot Area — BKD / Bigha",          "PlotAreaBKD"),
                    ("Perimeter (m)",                    "Perimeter"),
                    // Other
                    ("Notes",                            "PlotNotes"),
                    ("Label Text",                       "LabelText"),
                    ("Layer Name",                       "LayerName"),
                    ("Object ID",                        "Id"),
                ],

                CatPolyline =>
                [
                    ("Length (m)",                       "Length"),
                    ("Label Text",                       "LabelText"),
                    ("Object Description",               "ObjectDescription"),
                    ("Object Type",                      "ObjectType"),
                    ("Layer Name",                       "LayerName"),
                    ("Source Layer",                     "SourceLayer"),
                    ("Source File Name",                 "SourceFileName"),
                    ("Source Format",                    "SourceFormat"),
                    ("Object ID",                        "Id"),
                ],

                CatPoint =>
                [
                    ("X Coordinate",                     "X"),
                    ("Y Coordinate",                     "Y"),
                    ("Label Text",                       "LabelText"),
                    ("Object Description",               "ObjectDescription"),
                    ("Object Type",                      "ObjectType"),
                    ("Layer Name",                       "LayerName"),
                    ("Object ID",                        "Id"),
                ],

                CatText =>
                [
                    ("Label Text",                       "LabelText"),
                    ("Object Description",               "ObjectDescription"),
                    ("Object Type",                      "ObjectType"),
                    ("Layer Name",                       "LayerName"),
                    ("Object ID",                        "Id"),
                ],

                _ => // Generic — geometry metrics + object metadata
                [
                    ("Label Text",                       "LabelText"),
                    ("Object Description",               "ObjectDescription"),
                    ("Object Type",                      "ObjectType"),
                    ("Layer Name",                       "LayerName"),
                    ("Area (sq.m)",                      "CalculatedAreaSqm"),
                    ("Area — RAPD / Ropani",             "AreaRAPD"),
                    ("Area — BKD / Bigha",               "AreaBKD"),
                    ("Perimeter (m)",                    "Perimeter"),
                    ("Length (m)",                       "Length"),
                    ("X Coordinate",                     "X"),
                    ("Y Coordinate",                     "Y"),
                    ("Source Layer",                     "SourceLayer"),
                    ("Source File Name",                 "SourceFileName"),
                    ("Source Format",                    "SourceFormat"),
                    ("Matched Text",                     "MatchedText"),
                    ("Object ID",                        "Id"),
                ],
            };

        // ── Preset templates per entity / layer category ───────────────────────
        private static (string Display, string Template)[] GetPresetTemplates(string category) =>
            category switch
            {
                CatBaselineParcel =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Parcel Number",                                             "{ParcelNo}"),
                    ("Parcel No. + Area (sq.m)",                                  "{ParcelNo}\\nArea: {AreaSqm} sq.m"),
                    ("Parcel No. + Area (RAPD / Ropani)",                         "{ParcelNo}\\n{AreaRAPD}"),
                    ("Parcel No. + Owner Name",                                   "{ParcelNo}\\n{OwnerName}"),
                    ("Full Info  (Map-Parcel | Owner | Area)",                    "{MapSheetNo}-{ParcelNo}\\n{OwnerName}\\nArea: {AreaSqm} sq.m"),
                    ("Parcel No. + Perimeter",                                    "{ParcelNo}\\nPerimeter: {Perimeter} m"),
                ],

                CatRoad =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Road Width (ROW)",                                          "{RightOfWayWidth}m Road"),
                    ("Road Name",                                                 "{RoadName}"),
                    ("Road Name + ROW",                                           "{RoadName}\\n{RightOfWayWidth}m Road"),
                ],

                CatBlock =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Block Name",                                                "{BlockName}"),
                    ("Block Name + Area (sq.m)",                                  "{BlockName}\\nArea: {BlockAreaSqm} sq.m"),
                    ("Block Name + Land Use",                                     "{BlockName}\\n{BlockLandUse}"),
                    ("Block Name + Area (RAPD)",                                  "{BlockName}\\n{BlockAreaRAPD}"),
                ],

                CatReplottedParcel =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Plot No. (Active)",                                         "{ReplottedParcelNo}"),
                    ("Plot No. + Plot Type",                                      "{ReplottedParcelNo}\\n({PlotTypeName})"),
                    ("Plot No. + Area (sq.m)",                                    "{ReplottedParcelNo}\\nArea: {PlotAreaSqm} sq.m"),
                    ("Block + Plot No.",                                          "{PlotBlockName}-{ReplottedParcelNo}"),
                    ("Plot No. + Area (RAPD)",                                    "{ReplottedParcelNo}\\n{PlotAreaRAPD}"),
                ],

                CatPolyline =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Length (m)",                                                "Length: {Length} m"),
                    ("Label Text",                                                "{LabelText}"),
                    ("Label + Length",                                            "{LabelText}\\n{Length} m"),
                    ("Object Description",                                        "{ObjectDescription}"),
                ],

                CatPoint =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Coordinates  (X, Y)",                                       "({X}, {Y})"),
                    ("Label Text",                                                "{LabelText}"),
                    ("Label + Coordinates",                                       "{LabelText}\\n({X}, {Y})"),
                    ("Object Description",                                        "{ObjectDescription}"),
                ],

                CatText =>
                [
                    ("— Select a preset template —",                              ""),
                    ("Label Text",                                                "{LabelText}"),
                    ("Object Description",                                        "{ObjectDescription}"),
                ],

                _ => // Generic
                [
                    ("— Select a preset template —",                              ""),
                    ("Label Text",                                                "{LabelText}"),
                    ("Area (sq.m)",                                               "Area: {CalculatedAreaSqm} sq.m"),
                    ("Area + Perimeter",                                          "Area: {CalculatedAreaSqm} sq.m\\nPerimeter: {Perimeter} m"),
                    ("Object Description",                                        "{ObjectDescription}"),
                ],
            };

        // ── Category derivation ───────────────────────────────────────────────
        // NOTE: Imported cadastral layers get LayerType = "Polygon" from the
        // importer — never "BaselineParcel". Entity detection for parcel layers
        // must therefore rely on the sample record content (parcel data present).
        public static IReadOnlyList<string> GetApplicableLabelExpressions(
            string? layerType,
            IReadOnlyList<IReadOnlyDictionary<string, string>>? sampleRecords)
        {
            string category = DeriveGeometryCategory(layerType?.Trim() ?? string.Empty, sampleRecords);
            List<string> expressions = [];

            foreach ((_, string template) in GetPresetTemplates(category))
            {
                if (!string.IsNullOrWhiteSpace(template) &&
                    !expressions.Contains(template, StringComparer.OrdinalIgnoreCase))
                {
                    expressions.Add(template);
                }
            }

            foreach ((_, string fieldKey) in GetAvailableFields(category))
            {
                string expression = $"{{{fieldKey}}}";
                if (!expressions.Contains(expression, StringComparer.OrdinalIgnoreCase))
                    expressions.Add(expression);
            }

            return expressions;
        }

        private static string DeriveGeometryCategory(
            string layerType,
            IReadOnlyList<IReadOnlyDictionary<string, string>>? sampleRecords)
        {
            // Step 1 — entity-specific layer types that ARE set correctly by the app
            switch (layerType.ToLowerInvariant())
            {
                case "baselineparcel":
                    return CatBaselineParcel;
                case "proposedroad" or "existingroad" or "roadparcel" or "roadcenterline":
                    return CatRoad;
                case "block":
                    return CatBlock;
                case "replottedparcel" or "privatereplotparcel"
                    or "publicfacility" or "openspace" or "servicesalesplot":
                    return CatReplottedParcel;
                case "polyline" or "line" or "drawingmarkup":
                    return CatPolyline;
                case "point":
                    return CatPoint;
                case "annotation":
                    return CatText;
            }

            // Step 2 — inspect sample record content to detect entity type.
            // Imported parcel layers have LayerType = "Polygon" but their
            // sample records carry parcel-specific keys populated by GetLayerSampleRecordsAsync.
            if (sampleRecords?.Count > 0)
            {
                bool hasParcelData = sampleRecords.Any(r =>
                    HasNonEmpty(r, "BaselineParcelId") ||
                    HasNonEmpty(r, "ParcelNo")         ||
                    HasNonEmpty(r, "AssignmentStatus"));

                if (hasParcelData)
                    return CatBaselineParcel;

                // Fall back to dominant ObjectType for generic imported layers
                string dominant = sampleRecords
                    .Select(r => r.TryGetValue("ObjectType", out string? t) ? t : string.Empty)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault() ?? string.Empty;

                return dominant.ToLowerInvariant() switch
                {
                    "polyline" or "line" or "arc" => CatPolyline,
                    "point"                        => CatPoint,
                    "text"                         => CatText,
                    _                              => CatGeneric,
                };
            }

            return CatGeneric;
        }

        private static bool HasNonEmpty(IReadOnlyDictionary<string, string> record, string key) =>
            record.TryGetValue(key, out string? v) && !string.IsNullOrEmpty(v);

        // ── Hardcoded fallback sample (when no real records are available) ────
        private static readonly IReadOnlyDictionary<string, string> FallbackSample =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // BaselineParcel
                ["ParcelNo"]              = "123",
                ["MapSheetNo"]            = "45",
                ["FullUniqueParcelCode"]  = "45-123",
                ["OwnerName"]            = "Ram Bahadur Shrestha",
                ["OwnerFatherSpouse"]    = "Hari Bahadur Shrestha",
                ["OwnershipType"]        = "Private",
                ["HasTenant"]            = "No",
                ["TenantName"]           = "",
                ["LandUse"]              = "Residential",
                ["AssignmentStatus"]     = "Assigned",
                ["Province"]             = "Bagmati",
                ["District"]             = "Kathmandu",
                ["Municipality"]         = "Kathmandu Metropolitan",
                ["WardNo"]               = "5",
                ["MothNo"]               = "45",
                ["PaanaNo"]              = "123",
                // Area — from records
                ["AreaSqm"]              = "250.500",
                ["AreaRAPD"]             = "2-1-0-0",
                ["AreaBKD"]              = "2-1-0-0",
                ["FieldMeasuredAreaSqm"] = "249.800",
                ["EffectiveAreaSqm"]     = "248.000",
                // Area — from map
                ["CalculatedAreaSqm"]    = "251.000",
                ["Perimeter"]            = "78.40",
                // Road
                ["RoadName"]             = "Main Street",
                ["RoadCode"]             = "RD-001",
                ["RoadStatus"]           = "Proposed",
                ["RoadType"]             = "Collector",
                ["SurfaceType"]          = "Blacktopped",
                ["RoadWidth"]            = "7.0",
                ["RightOfWayWidth"]      = "9.0",
                ["RoadDescription"]      = "",
                ["Length"]               = "125.30",
                // Block
                ["BlockName"]            = "Block A",
                ["BlockCode"]            = "BLK-A",
                ["BlockLandUse"]         = "Residential",
                ["BlockDepth"]           = "15.0",
                ["BlockAreaSqm"]         = "3200.000",
                ["BlockAreaRAPD"]        = "26-8-0-0",
                ["BlockAreaBKD"]         = "26-8-0-0",
                ["BlockDescription"]     = "",
                // ReplottedParcel
                ["ReplottedParcelNo"]     = "A-001",
                ["SystemGeneratedNumber"] = "2001",
                ["DerivedNumber"]         = "123/1",
                ["BlockSequenceNumber"]   = "A-001",
                ["PlotTypeName"]          = "Private",
                ["PlotBlockName"]         = "Block A",
                ["PlotAreaSqm"]           = "120.000",
                ["PlotAreaRAPD"]          = "1-0-0-0",
                ["PlotAreaBKD"]           = "1-0-0-0",
                ["PlotNotes"]             = "",
                // Geometry / coordinates
                ["X"]                    = "830245.2345",
                ["Y"]                    = "3025678.1234",
                // Generic
                ["LabelText"]            = "Sample Label",
                ["ObjectDescription"]    = "Description",
                ["ObjectType"]           = "Polygon",
                ["LayerName"]            = "Layer",
                ["SourceLayer"]          = "Import Layer",
                ["SourceFileName"]       = "import.shp",
                ["SourceFormat"]         = "Shapefile",
                ["MatchedText"]          = "ABC-123",
                ["Id"]                   = "42",
                ["BaselineParcelId"]     = "BLP-123",
            };

        // ── State ─────────────────────────────────────────────────────────────
        private readonly string   _layerType;
        private readonly string   _geometryCategory;
        private readonly (string Display, string FieldKey)[]   _availableFields;
        private readonly (string Display, string Template)[]   _presetTemplates;
        private readonly IReadOnlyList<IReadOnlyDictionary<string, string>>? _sampleRecords;
        private readonly Random _rng = new();
        private int  _currentSampleIndex;
        private bool _suppressPresetSync;

        // ── Public outputs ────────────────────────────────────────────────────
        public string LabelExpression { get; private set; }
        public string TextAlignment   { get; private set; }

        // ── Constructor ───────────────────────────────────────────────────────
        public frmLabelExpressionEditor(
            string? initialExpression = null,
            string? initialAlignment  = null,
            IReadOnlyList<IReadOnlyDictionary<string, string>>? sampleRecords = null,
            string? layerType = null)
        {
            InitializeComponent();

            _layerType        = layerType?.Trim() ?? string.Empty;
            _sampleRecords    = sampleRecords;
            _geometryCategory = DeriveGeometryCategory(_layerType, sampleRecords);
            _availableFields  = GetAvailableFields(_geometryCategory);
            _presetTemplates  = GetPresetTemplates(_geometryCategory);

            LabelExpression = initialExpression?.Trim() ?? string.Empty;
            TextAlignment   = NormalizeAlignment(initialAlignment) ?? "Center Middle";

            LoadFields();
            LoadPresets();
            LoadInitialExpression(LabelExpression);
            LoadAlignment();
            PickRandomSample();
            UpdatePreviewNote();
            UpdatePreview();
            _cboAlignment.SelectedIndexChanged += (_, _) => UpdatePreview();
            _cboPresets.SelectedIndexChanged   += cboPresets_SelectedIndexChanged;
        }

        // ── Initializers ──────────────────────────────────────────────────────
        private void LoadFields()
        {
            _lstFields.Items.Clear();
            foreach ((string display, _) in _availableFields)
                _lstFields.Items.Add(display);

            _grpFields.Text = _geometryCategory switch
            {
                CatBaselineParcel  => "Fields — Original Parcel",
                CatRoad            => "Fields — Road",
                CatBlock           => "Fields — Block",
                CatReplottedParcel => "Fields — Replotted Parcel",
                CatPolyline        => "Fields — Polyline / Line",
                CatPoint           => "Fields — Point",
                CatText            => "Fields — Text / Annotation",
                _                  => "Available Fields",
            };
        }

        private void LoadPresets()
        {
            _cboPresets.Items.Clear();
            foreach ((string display, _) in _presetTemplates)
                _cboPresets.Items.Add(display);
            _cboPresets.Items.Add("— Custom Expression —");
            _cboPresets.SelectedIndex = 0;
        }

        private void LoadInitialExpression(string? expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                _txtExpression.Text = string.Empty;
                return;
            }

            if (expression.StartsWith("template:", StringComparison.OrdinalIgnoreCase))
                expression = expression["template:".Length..];

            _txtExpression.Text = expression;
        }

        private void LoadAlignment()
        {
            for (int i = 0; i < _cboAlignment.Items.Count; i++)
            {
                if (string.Equals(
                    _cboAlignment.Items[i]?.ToString(),
                    TextAlignment,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _cboAlignment.SelectedIndex = i;
                    return;
                }
            }
            _cboAlignment.SelectedIndex = 0;
        }

        private void PickRandomSample()
        {
            if (_sampleRecords == null || _sampleRecords.Count == 0)
            {
                _currentSampleIndex = 0;
                return;
            }
            _currentSampleIndex = _rng.Next(_sampleRecords.Count);
        }

        private void UpdatePreviewNote()
        {
            if (_sampleRecords != null && _sampleRecords.Count > 0)
            {
                _lblPreviewSampleNote.Text =
                    $"Record {_currentSampleIndex + 1} of {_sampleRecords.Count} sampled from this layer's objects.";
            }
            else
            {
                _lblPreviewSampleNote.Text =
                    "Shown with hardcoded sample values — no layer objects available for live preview.";
            }
        }

        // ── Preset ────────────────────────────────────────────────────────────
        private void cboPresets_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_suppressPresetSync)
                return;

            int idx = _cboPresets.SelectedIndex;
            // Index 0 = placeholder; last index = "Custom" — neither modifies the expression
            if (idx <= 0 || idx >= _presetTemplates.Length)
                return;

            string template = _presetTemplates[idx].Template;
            if (string.IsNullOrEmpty(template))
                return;

            _suppressPresetSync = true;
            try
            {
                _txtExpression.Text = template;
                _txtExpression.SelectionStart = _txtExpression.Text.Length;
            }
            finally
            {
                _suppressPresetSync = false;
            }
            _txtExpression.Focus();
        }

        private void btnApplyPreset_Click(object? sender, EventArgs e)
        {
            cboPresets_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void SyncPresetCombo()
        {
            string current = _txtExpression.Text;
            for (int i = 1; i < _presetTemplates.Length; i++)
            {
                if (string.Equals(current, _presetTemplates[i].Template, StringComparison.Ordinal))
                {
                    SetPresetIndexSilently(i);
                    return;
                }
            }
            // No preset match: empty → placeholder, anything else → Custom
            SetPresetIndexSilently(
                string.IsNullOrWhiteSpace(current) ? 0 : _cboPresets.Items.Count - 1);
        }

        private void SetPresetIndexSilently(int index)
        {
            if (_cboPresets.SelectedIndex == index)
                return;
            _suppressPresetSync = true;
            try { _cboPresets.SelectedIndex = index; }
            finally { _suppressPresetSync = false; }
        }

        // ── Field insert ──────────────────────────────────────────────────────
        private void lstFields_DoubleClick(object? sender, EventArgs e)
        {
            InsertSelectedField();
        }

        private void btnInsertField_Click(object? sender, EventArgs e)
        {
            InsertSelectedField();
        }

        private void InsertSelectedField()
        {
            int idx = _lstFields.SelectedIndex;
            if (idx < 0 || idx >= _availableFields.Length)
                return;

            InsertTextAtCursor($"{{{_availableFields[idx].FieldKey}}}");
        }

        // ── Quick-insert helpers ──────────────────────────────────────────────
        private void btnInsertNewline_Click(object? sender, EventArgs e)
        {
            InsertTextAtCursor(@"\n");
        }

        private void btnInsertSpace_Click(object? sender, EventArgs e)
        {
            InsertTextAtCursor(" ");
        }

        private void btnClearExpression_Click(object? sender, EventArgs e)
        {
            _txtExpression.Clear();
            _txtExpression.Focus();
        }

        private void InsertTextAtCursor(string text)
        {
            int pos    = _txtExpression.SelectionStart;
            int selLen = _txtExpression.SelectionLength;
            string cur = _txtExpression.Text;
            _txtExpression.Text = cur[..pos] + text + cur[(pos + selLen)..];
            _txtExpression.SelectionStart  = pos + text.Length;
            _txtExpression.SelectionLength = 0;
            _txtExpression.Focus();
        }

        // ── Preview ───────────────────────────────────────────────────────────
        private void txtExpression_TextChanged(object? sender, EventArgs e)
        {
            UpdatePreview();
            if (!_suppressPresetSync)
                SyncPresetCombo();
        }

        private void btnRefreshPreview_Click(object? sender, EventArgs e)
        {
            PickRandomSample();
            UpdatePreviewNote();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            string expression = _txtExpression.Text;

            _lblPreviewOutput.TextAlign = GetPreviewContentAlignment();

            if (string.IsNullOrWhiteSpace(expression))
            {
                _lblPreviewOutput.ForeColor = Color.FromArgb(130, 130, 130);
                _lblPreviewOutput.Text = "(no label)";
                return;
            }

            IReadOnlyDictionary<string, string> sample = GetCurrentSample();
            string expanded = expression.Replace("\\n", "\n", StringComparison.Ordinal);
            bool hasMissingFieldValue = false;
            expanded = Regex.Replace(
                expanded,
                @"\{(?<field>[^{}]+)\}",
                match =>
                {
                    string field = match.Groups["field"].Value.Trim();
                    if (sample.TryGetValue(field, out string? val) && !string.IsNullOrWhiteSpace(val))
                        return FormatPreviewFieldValue(field, val);

                    hasMissingFieldValue = true;
                    return string.Empty;
                });

            _lblPreviewOutput.ForeColor = Color.Black;
            _lblPreviewOutput.Text = hasMissingFieldValue || string.IsNullOrWhiteSpace(expanded)
                ? NoDataPreviewText
                : expanded.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
        }

        private static string FormatPreviewFieldValue(string field, string value)
        {
            if ((string.Equals(field, "RoadWidth", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(field, "RightOfWayWidth", StringComparison.OrdinalIgnoreCase)) &&
                double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double width))
            {
                return width.ToString("0.##", CultureInfo.InvariantCulture);
            }

            return value;
        }

        private ContentAlignment GetPreviewContentAlignment()
        {
            return _cboAlignment.Text switch
            {
                "Left Top"      => ContentAlignment.TopLeft,
                "Center Top"    => ContentAlignment.TopCenter,
                "Right Top"     => ContentAlignment.TopRight,
                "Left Middle"   => ContentAlignment.MiddleLeft,
                "Center Middle" => ContentAlignment.MiddleCenter,
                "Right Middle"  => ContentAlignment.MiddleRight,
                "Left Bottom"   => ContentAlignment.BottomLeft,
                "Center Bottom" => ContentAlignment.BottomCenter,
                "Right Bottom"  => ContentAlignment.BottomRight,
                _               => ContentAlignment.MiddleLeft,
            };
        }

        private IReadOnlyDictionary<string, string> GetCurrentSample()
        {
            if (_sampleRecords == null || _sampleRecords.Count == 0)
                return FallbackSample;

            return _sampleRecords[Math.Clamp(_currentSampleIndex, 0, _sampleRecords.Count - 1)];
        }

        // ── Normalise helpers ─────────────────────────────────────────────────
        private static string? NormalizeAlignment(string? alignment)
        {
            if (string.IsNullOrWhiteSpace(alignment))
                return null;

            string[] parts = alignment.Trim()
                .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string h = (parts.Length > 0 ? parts[0] : "").ToLowerInvariant() switch
            {
                "center" or "centre" or "middle" => "Center",
                "right"                           => "Right",
                _                                 => "Left",
            };
            string v = (parts.Length > 1 ? parts[1] : "").ToLowerInvariant() switch
            {
                "middle" or "center" or "centre" => "Middle",
                "bottom"                          => "Bottom",
                _                                 => "Top",
            };
            return $"{h} {v}";
        }

        // ── Closing ───────────────────────────────────────────────────────────
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                LabelExpression = _txtExpression.Text.Trim();
                TextAlignment   = _cboAlignment.Text;
            }

            base.OnFormClosing(e);
        }
    }
}
