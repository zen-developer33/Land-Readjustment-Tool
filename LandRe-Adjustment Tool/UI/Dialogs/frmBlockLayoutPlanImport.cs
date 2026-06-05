using System.Text.RegularExpressions;
using Land_Readjustment_Tool.Core.Models.Import;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed partial class frmBlockLayoutPlanImport : Form
    {
        private readonly ExternalLayerFileInfo _fileInfo;
        private readonly string _fallbackSourceCrsDefinition;
        private readonly string _projectCrsLabel;
        private readonly IReadOnlyList<CrsChoice> _sourceCrsChoices;
        private CheckState _includeHeaderState = CheckState.Checked;

        private const string IncludeColumn = "_colInclude";
        private const string LayerColumn = "_colLayer";
        private const string TypesColumn = "_colTypes";
        private const string TargetColumn = "_colTarget";

        public frmBlockLayoutPlanImport()
            : this(
                CreateDesignerFileInfo(),
                string.Empty,
                "Project CRS",
                Array.Empty<CrsChoice>())
        {
        }

        public frmBlockLayoutPlanImport(
            ExternalLayerFileInfo fileInfo,
            string sourceCrsDefinition,
            string projectCrsLabel,
            IReadOnlyList<CrsChoice>? sourceCrsChoices = null)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _fallbackSourceCrsDefinition = sourceCrsDefinition ?? string.Empty;
            _projectCrsLabel = projectCrsLabel ?? string.Empty;
            _sourceCrsChoices = sourceCrsChoices ?? [];

            InitializeComponent();
            ConfigureRuntimeControls();
            PopulateGrid();
            PopulateBlockLabelLayerChoices();
            ConfigureCrsControls();
            UpdateImportState();
        }

        private static ExternalLayerFileInfo CreateDesignerFileInfo()
        {
            return new ExternalLayerFileInfo(
                "Block Layout Plan.dwg",
                "DWG",
                new List<ExternalLayerInfo>
                {
                    new("Blocks", 12, "Polygon: 12"),
                    new("Road Parcel", 5, "Polygon: 5"),
                    new("Road Centerline", 8, "Polyline: 8"),
                    new("Block Labels", 12, "Text: 12")
                },
                null,
                RequiresCrsFromUser: false);
        }

        private void ConfigureRuntimeControls()
        {
            _lblSummary.Text = string.Format(
                "{0}  |  {1}  |  {2} source layer(s)",
                Path.GetFileName(_fileInfo.FilePath),
                _fileInfo.FileFormat,
                _fileInfo.Layers.Count);

            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(32, 41, 57);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);

            _colTarget.Items.Clear();
            foreach (string target in GetTargetLayerDisplayNames())
                _colTarget.Items.Add(target);
        }

        public ExternalLayerImportOptions ImportOptions =>
            new(
                GetLayerOptions(),
                GetSourceCrsDefinition(),
                UseTargetLayerMapping: true,
                ImportKind: "BlockLayoutPlan",
                BlockLabelLayerName: SelectedBlockLabelLayerName);

        public string? SelectedBlockLabelLayerName =>
            _cmbBlockLabelLayer.SelectedItem is LabelLayerChoice choice && !string.IsNullOrWhiteSpace(choice.LayerName)
                ? choice.LayerName
                : null;

        private void PopulateGrid()
        {
            foreach (ExternalLayerInfo layer in _fileInfo.Layers)
            {
                LayerMatch match = MatchTargetLayer(layer.Name, layer.ObjectTypes);
                int rowIndex = _grid.Rows.Add(
                    true,
                    layer.Name,
                    layer.ObjectTypes,
                    string.Empty);

                DataGridViewRow row = _grid.Rows[rowIndex];
                row.Tag = layer;

                ConfigureTargetCellChoices(row, layer);
                ApplyTargetCellValue(row, match.TargetLayerName, match.Note);
                ApplyRowEnabledStyle(row);
            }

            _grid.ClearSelection();
            _grid.CurrentCell = null;
        }

        private void PopulateBlockLabelLayerChoices()
        {
            _cmbBlockLabelLayer.Items.Clear();
            _cmbBlockLabelLayer.Items.Add(new LabelLayerChoice(null, "(none)"));

            foreach (ExternalLayerInfo layer in _fileInfo.Layers.Where(IsTextLayer).OrderBy(layer => layer.Name))
                _cmbBlockLabelLayer.Items.Add(new LabelLayerChoice(layer.Name, layer.Name));

            _cmbBlockLabelLayer.SelectedIndex = 0;
            _cmbBlockLabelLayer.Enabled = _cmbBlockLabelLayer.Items.Count > 1;
        }

        private void EnsureSelectedLabelLayerIncluded()
        {
            string? labelLayerName = SelectedBlockLabelLayerName;
            if (string.IsNullOrWhiteSpace(labelLayerName))
                return;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is not ExternalLayerInfo layer ||
                    !string.Equals(layer.Name, labelLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                row.Cells[IncludeColumn].Value = true;
                row.Cells[TargetColumn].Value = BlockLayoutPlanImportTargets.KeepSourceLayerTarget;
                ApplyRowEnabledStyle(row);
                break;
            }

            UpdateImportState();
        }

        private List<ExternalLayerImportOption> GetLayerOptions()
        {
            List<ExternalLayerImportOption> options = [];
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.Tag is not ExternalLayerInfo layer)
                    continue;

                bool include = row.Cells[IncludeColumn].Value is bool value && value;
                string? target = row.Cells[TargetColumn].Value?.ToString();
                if (string.Equals(target, BlockLayoutPlanImportTargets.KeepSourceLayerTarget, StringComparison.OrdinalIgnoreCase))
                    target = null;

                options.Add(new ExternalLayerImportOption(layer.Name, include, target));
            }

            return options;
        }

        private void SetAllIncluded(bool included)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                row.Cells[IncludeColumn].Value = included;
                ApplyRowEnabledStyle(row);
            }

            UpdateImportState();
        }

        private void UpdateImportState()
        {
            List<ExternalLayerImportOption> options = GetLayerOptions();
            int selected = options.Count(option => option.Include);
            int mapped = options.Count(option => option.Include && !string.IsNullOrWhiteSpace(option.TargetLayerName));
            int external = selected - mapped;

            _btnImport.Enabled = selected > 0;
            _lblSelection.Text = selected == 0
                ? "No source layers selected."
                : $"{selected} selected | {mapped} mapped | {external} external";
            UpdateHeaderCheckState();
        }

        private void ApplyRowEnabledStyle(DataGridViewRow row)
        {
            bool included = row.Cells[IncludeColumn].Value is bool value && value;
            row.DefaultCellStyle.ForeColor = included
                ? SystemColors.ControlText
                : SystemColors.GrayText;
            row.DefaultCellStyle.BackColor = included
                ? SystemColors.Window
                : Color.FromArgb(248, 248, 248);

            if (row.Cells[TargetColumn] is DataGridViewComboBoxCell targetCell)
            {
                targetCell.ReadOnly = !included;
                targetCell.DisplayStyle = included
                    ? DataGridViewComboBoxDisplayStyle.DropDownButton
                    : DataGridViewComboBoxDisplayStyle.Nothing;
                targetCell.Style.ForeColor = included
                    ? SystemColors.ControlText
                    : SystemColors.GrayText;
                targetCell.Style.BackColor = included
                    ? SystemColors.Window
                    : Color.FromArgb(240, 240, 240);
            }
        }

        private void ConfigureCrsControls()
        {
            _lblProjectCrsValue.Text = _projectCrsLabel;

            if (_fileInfo.RequiresCrsFromUser)
            {
                _cmbSourceCrs.Visible = true;
                _lblSourceCrsValue.Visible = false;
                _cmbSourceCrs.Items.Clear();

                foreach (CrsChoice choice in _sourceCrsChoices)
                    _cmbSourceCrs.Items.Add(choice);

                if (_cmbSourceCrs.Items.Count == 0 && !string.IsNullOrWhiteSpace(_fallbackSourceCrsDefinition))
                    _cmbSourceCrs.Items.Add(new CrsChoice("Project CRS", _fallbackSourceCrsDefinition));

                if (_cmbSourceCrs.Items.Count > 0)
                    _cmbSourceCrs.SelectedIndex = 0;

                return;
            }

            _cmbSourceCrs.Visible = false;
            _lblSourceCrsValue.Visible = true;
            _lblSourceCrsValue.Text = _fileInfo.DetectedCrsCode ?? "Unknown";
        }

        private string GetSourceCrsDefinition()
        {
            if (_fileInfo.RequiresCrsFromUser)
            {
                return _cmbSourceCrs.SelectedItem is CrsChoice choice
                    ? choice.Definition
                    : _fallbackSourceCrsDefinition;
            }

            return string.IsNullOrWhiteSpace(_fileInfo.DetectedCrsCode)
                ? _fallbackSourceCrsDefinition
                : _fileInfo.DetectedCrsCode;
        }

        private void Grid_CellPainting(object? sender, DataGridViewCellPaintingEventArgs args)
        {
            int includeColumnIndex = _grid.Columns[IncludeColumn]?.Index ?? -1;
            if (args.RowIndex != -1 || args.ColumnIndex != includeColumnIndex || args.Graphics == null)
                return;

            args.Paint(args.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            System.Windows.Forms.VisualStyles.CheckBoxState state = _includeHeaderState switch
            {
                CheckState.Checked => System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal,
                CheckState.Indeterminate => System.Windows.Forms.VisualStyles.CheckBoxState.MixedNormal,
                _ => System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal
            };

            Size checkSize = CheckBoxRenderer.GetGlyphSize(args.Graphics, state);
            Point location = new(
                args.CellBounds.Left + (args.CellBounds.Width - checkSize.Width) / 2,
                args.CellBounds.Top + (args.CellBounds.Height - checkSize.Height) / 2);

            CheckBoxRenderer.DrawCheckBox(args.Graphics, location, state);
            args.Handled = true;
        }

        private void cmbBlockLabelLayer_SelectedIndexChanged(object? sender, EventArgs e)
        {
            EnsureSelectedLabelLayerIncluded();
        }

        private void grid_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (_grid.IsCurrentCellDirty)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void grid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                ApplyRowEnabledStyle(_grid.Rows[e.RowIndex]);

            UpdateImportState();
        }

        private void grid_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn? includeColumn = _grid.Columns[IncludeColumn];
            int includeColumnIndex = includeColumn == null ? -1 : includeColumn.Index;
            if (e.RowIndex == -1 && e.ColumnIndex == includeColumnIndex)
                SetAllIncluded(_includeHeaderState != CheckState.Checked);
        }

        private void grid_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void UpdateHeaderCheckState()
        {
            int included = _grid.Rows
                .Cast<DataGridViewRow>()
                .Count(row => row.Cells[IncludeColumn].Value is bool value && value);

            _includeHeaderState = included == 0
                ? CheckState.Unchecked
                : included == _grid.Rows.Count
                    ? CheckState.Checked
                    : CheckState.Indeterminate;

            DataGridViewColumn? includeColumn = _grid.Columns[IncludeColumn];
            if (includeColumn != null)
                _grid.InvalidateCell(includeColumn.HeaderCell);
        }

        private static void ConfigureTargetCellChoices(DataGridViewRow row, ExternalLayerInfo layer)
        {
            if (row.Cells[TargetColumn] is not DataGridViewComboBoxCell targetCell)
                return;

            targetCell.Items.Clear();
            foreach (string name in GetApplicableTargetNames(layer.ObjectTypes))
                targetCell.Items.Add(name);
        }

        private static void ApplyTargetCellValue(DataGridViewRow row, string desiredTarget, string note)
        {
            if (row.Cells[TargetColumn] is not DataGridViewComboBoxCell targetCell)
                return;

            bool applicable = targetCell.Items.Contains(desiredTarget);
            string value = applicable
                ? desiredTarget
                : BlockLayoutPlanImportTargets.KeepSourceLayerTarget;

            targetCell.Value = value;
            targetCell.ToolTipText = applicable
                ? note
                : "No compatible target for this geometry";
        }

        private static IReadOnlyList<string> GetApplicableTargetNames(string objectTypes)
        {
            (bool hasArea, bool hasLinear) = GetSourceGeometryCapabilities(objectTypes);

            List<string> names = [];
            foreach (BlockLayoutPlanTargetLayerDefinition target in BlockLayoutPlanImportTargets.TargetLayers)
            {
                bool applicable = IsLineTargetLayerType(target.LayerType) ? hasLinear : hasArea;
                if (applicable)
                    names.Add(target.Name);
            }

            names.Add(BlockLayoutPlanImportTargets.KeepSourceLayerTarget);
            return names;
        }

        private static bool IsLineTargetLayerType(string layerType)
        {
            return string.Equals(layerType, "RoadCenterline", StringComparison.OrdinalIgnoreCase);
        }

        private static (bool HasArea, bool HasLinear) GetSourceGeometryCapabilities(string objectTypes)
        {
            bool hasClosedArea = false;
            bool hasOpenLinear = false;

            foreach (string token in (objectTypes ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string type = token.Split(':', 2)[0].Trim().ToLowerInvariant();
                if (type.Length == 0)
                    continue;

                if (type.Contains("polygon") || type.Contains("circle") || type.Contains("ellipse"))
                    hasClosedArea = true;
                else if (type.Contains("polyline") || type.Contains("line") || type.Contains("arc"))
                    hasOpenLinear = true;
            }

            return (hasClosedArea, hasOpenLinear);
        }

        private static bool IsTextLayer(ExternalLayerInfo layer) =>
            (layer.ObjectTypes ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(token => token.StartsWith("text", StringComparison.OrdinalIgnoreCase));

        private static IReadOnlyList<string> GetTargetLayerDisplayNames()
        {
            List<string> names = BlockLayoutPlanImportTargets.TargetLayers
                .Select(layer => layer.Name)
                .ToList();
            names.Add(BlockLayoutPlanImportTargets.KeepSourceLayerTarget);
            return names;
        }

        private static LayerMatch MatchTargetLayer(string layerName, string objectTypes)
        {
            string text = NormalizeForMatching(layerName);
            string objectText = NormalizeForMatching(objectTypes);

            Dictionary<string, int> scores = BlockLayoutPlanImportTargets.TargetLayers
                .ToDictionary(layer => layer.Name, _ => 0, StringComparer.OrdinalIgnoreCase);

            (bool hasClosedArea, bool _) = GetSourceGeometryCapabilities(objectTypes);
            string roadGeometryTarget = hasClosedArea ? "Road Parcel" : "Road Centerline";

            AddScore(scores, "Road Centerline", text, 120,
                "centerline", "centreline", "center line", "centre line", "road center", "road centre",
                "cl road", "road cl", "c l", "axis", "alignment", "median");
            AddScore(scores, "Road Centerline", text, 80, "rcl", "rdcl", "rd cl", "road axis");

            AddScore(scores, "Road Parcel", text, 105,
                "road parcel", "road polygon", "road area", "road reserve", "road corridor",
                "right of way", "row", "r o w");
            AddScore(scores, roadGeometryTarget, text, 105, "road", "roads", "proposed road");
            if (Regex.IsMatch(text, @"(^|\s)(r\s*)?\d{1,2}(\.\d+)?\s*m(\s|$)") ||
                Regex.IsMatch(text, @"(^|\s)\d{1,2}(\.\d+)?\s*m\s*roads?(\s|$)") ||
                Regex.IsMatch(text, @"road\s*\d{1,2}(\.\d+)?\s*m"))
            {
                scores[roadGeometryTarget] += 125;
            }

            AddScore(scores, "Project Boundary", text, 115,
                "project boundary", "project boundry", "project bdy", "project limit", "site boundary",
                "site limit", "outer boundary", "perimeter", "planning boundary", "scheme boundary");
            AddScore(scores, "Project Boundary", text, 65, "boundary", "boundry", "bdy", "limit");

            AddScore(scores, "Blocks", text, 110,
                "block", "blocks", "blk", "sector", "super block", "layout block", "planning block");

            AddScore(scores, "Open Spaces/Parks", text, 115,
                "open space", "openspace", "park", "parks", "green", "green belt", "playground",
                "recreation", "recreational", "garden", "os");

            AddScore(scores, "Public/Facilities/Community Spaces", text, 110,
                "public", "facility", "facilities", "community", "school", "temple", "hospital",
                "utility", "utilities", "institution", "communal", "public facility", "public facilities");

            AddScore(scores, "Service/Sales Plot", text, 100,
                "service", "sales", "sale plot", "sales plot", "reserve plot", "reserved plot",
                "commercial", "amenity plot");

            AddScore(scores, "Building Footprint", text, 100,
                "building", "buildings", "footprint", "house", "structure", "built up", "bldg");

            AddScore(scores, "Private", text, 85,
                "private", "replot", "replotted", "plot", "plots", "parcel", "lot", "residential", "housing");

            if (objectText.Contains("line") && !objectText.Contains("polygon"))
                scores["Road Centerline"] += text.Contains("road") ? 35 : 0;
            if (objectText.Contains("polygon"))
            {
                scores["Road Parcel"] += text.Contains("road") ? 25 : 0;
                scores["Blocks"] += text.Contains("block") ? 20 : 0;
            }

            HashSet<string> applicable = GetApplicableTargetNames(objectTypes)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            KeyValuePair<string, int> best = scores
                .Where(item => applicable.Contains(item.Key))
                .OrderByDescending(item => item.Value)
                .ThenBy(item => item.Key)
                .DefaultIfEmpty(new KeyValuePair<string, int>(string.Empty, 0))
                .First();

            if (best.Value <= 0 || string.IsNullOrEmpty(best.Key))
            {
                return new LayerMatch(
                    BlockLayoutPlanImportTargets.KeepSourceLayerTarget,
                    "No confident keyword match");
            }

            return new LayerMatch(best.Key, $"Matched by layer name keywords ({best.Value})");
        }

        private static void AddScore(
            Dictionary<string, int> scores,
            string targetLayer,
            string text,
            int score,
            params string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                string normalizedKeyword = NormalizeForMatching(keyword);
                if (Regex.IsMatch(text, $@"(^|\s){Regex.Escape(normalizedKeyword)}(\s|$)"))
                    scores[targetLayer] += score;
            }
        }

        private static string NormalizeForMatching(string value)
        {
            string normalized = value.ToLowerInvariant();
            normalized = Regex.Replace(normalized, @"[_\-/\\\.]+", " ");
            normalized = Regex.Replace(normalized, @"(?<=\p{L})(?=\d)|(?<=\d)(?=\p{L})", " ");
            normalized = Regex.Replace(normalized, @"[^\p{L}\p{Nd}]+", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ");
            return normalized.Trim();
        }

        private sealed record LayerMatch(string TargetLayerName, string Note);

        public sealed record CrsChoice(string Label, string Definition)
        {
            public override string ToString() => Label;
        }

        private sealed record LabelLayerChoice(string? LayerName, string DisplayText)
        {
            public override string ToString() => DisplayText;
        }
    }
}
