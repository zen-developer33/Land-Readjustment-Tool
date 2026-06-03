using System.Text.RegularExpressions;
using Land_Readjustment_Tool.Core.Models.Import;

namespace Land_Readjustment_Tool.UI.Dialogs
{
    public sealed class frmBlockLayoutPlanImport : Form
    {
        private readonly ExternalLayerFileInfo _fileInfo;
        private readonly string _fallbackSourceCrsDefinition;
        private readonly string _projectCrsLabel;
        private readonly IReadOnlyList<CrsChoice> _sourceCrsChoices;
        private readonly DataGridView _grid = new();
        private readonly Label _lblSummary = new();
        private readonly Label _lblSelection = new();
        private readonly ComboBox _cmbSourceCrs = new();
        private readonly ComboBox _cmbBlockLabelLayer = new();
        private readonly Label _lblSourceCrsValue = new();
        private readonly Label _lblProjectCrsValue = new();
        private readonly Button _btnImport = new();
        private readonly Button _btnCancel = new();
        private CheckState _includeHeaderState = CheckState.Checked;

        private const string IncludeColumn = "Include";
        private const string LayerColumn = "Layer";
        private const string TypesColumn = "Types";
        private const string TargetColumn = "Target";

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

            InitializeForm();
            PopulateGrid();
            PopulateBlockLabelLayerChoices();
            ConfigureCrsControls();
            UpdateImportState();
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

        private void InitializeForm()
        {
            Text = "Import Block Layout Plan";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(720, 430);

            TableLayoutPanel root = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

            _lblSummary.Dock = DockStyle.Fill;
            _lblSummary.ForeColor = SystemColors.GrayText;
            _lblSummary.AutoEllipsis = true;
            _lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            _lblSummary.Text =
                $"{Path.GetFileName(_fileInfo.FilePath)}  |  {_fileInfo.FileFormat}  |  {_fileInfo.Layers.Count} source layer(s)";

            TableLayoutPanel crsLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 8)
            };
            crsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
            crsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            crsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            crsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            Label sourceCrsCaption = CreateCaption("Source CRS");
            Label projectCrsCaption = CreateCaption("Project CRS");

            _cmbSourceCrs.Dock = DockStyle.Fill;
            _cmbSourceCrs.DropDownStyle = ComboBoxStyle.DropDownList;

            _lblSourceCrsValue.Dock = DockStyle.Fill;
            _lblSourceCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            _lblSourceCrsValue.AutoEllipsis = true;

            _lblProjectCrsValue.Dock = DockStyle.Fill;
            _lblProjectCrsValue.TextAlign = ContentAlignment.MiddleLeft;
            _lblProjectCrsValue.AutoEllipsis = true;
            _lblProjectCrsValue.ForeColor = SystemColors.GrayText;

            crsLayout.Controls.Add(sourceCrsCaption, 0, 0);
            crsLayout.Controls.Add(_cmbSourceCrs, 1, 0);
            crsLayout.Controls.Add(_lblSourceCrsValue, 1, 0);
            crsLayout.Controls.Add(projectCrsCaption, 0, 1);
            crsLayout.Controls.Add(_lblProjectCrsValue, 1, 1);

            TableLayoutPanel labelLayout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 6)
            };
            labelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
            labelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            labelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _cmbBlockLabelLayer.Dock = DockStyle.Fill;
            _cmbBlockLabelLayer.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbBlockLabelLayer.SelectedIndexChanged += (_, _) => EnsureSelectedLabelLayerIncluded();
            labelLayout.Controls.Add(CreateCaption("Block labels"), 0, 0);
            labelLayout.Controls.Add(_cmbBlockLabelLayer, 1, 0);

            ConfigureGrid();

            TableLayoutPanel bottom = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196));

            _lblSelection.Dock = DockStyle.Fill;
            _lblSelection.ForeColor = SystemColors.GrayText;
            _lblSelection.TextAlign = ContentAlignment.MiddleLeft;

            FlowLayoutPanel buttons = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false
            };

            _btnImport.Text = "Import";
            _btnImport.Size = new Size(90, 32);
            _btnImport.DialogResult = DialogResult.OK;

            _btnCancel.Text = "Cancel";
            _btnCancel.Size = new Size(90, 32);
            _btnCancel.DialogResult = DialogResult.Cancel;

            buttons.Controls.Add(_btnCancel);
            buttons.Controls.Add(_btnImport);
            bottom.Controls.Add(_lblSelection, 0, 0);
            bottom.Controls.Add(buttons, 1, 0);

            root.Controls.Add(_lblSummary, 0, 0);
            root.Controls.Add(crsLayout, 0, 1);
            root.Controls.Add(labelLayout, 0, 2);
            root.Controls.Add(_grid, 0, 3);
            root.Controls.Add(bottom, 0, 4);

            Controls.Add(root);
            AcceptButton = _btnImport;
            CancelButton = _btnCancel;
        }

        private void ConfigureGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.BorderStyle = BorderStyle.FixedSingle;
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(32, 41, 57);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _grid.ColumnHeadersHeight = 28;
            _grid.RowTemplate.Height = 25;
            _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            _grid.EditMode = DataGridViewEditMode.EditOnEnter;

            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = IncludeColumn,
                HeaderText = "",
                Width = 36
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = LayerColumn,
                HeaderText = "Source layer",
                ReadOnly = true,
                Width = 230
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = TypesColumn,
                HeaderText = "Object types",
                ReadOnly = true,
                Width = 185
            });

            DataGridViewComboBoxColumn targetColumn = new()
            {
                Name = TargetColumn,
                HeaderText = "Target layer",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FlatStyle = FlatStyle.Flat
            };
            foreach (string target in GetTargetLayerDisplayNames())
                targetColumn.Items.Add(target);
            _grid.Columns.Add(targetColumn);

            _grid.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (_grid.IsCurrentCellDirty)
                    _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellValueChanged += (_, args) =>
            {
                if (args.RowIndex >= 0)
                    ApplyRowEnabledStyle(_grid.Rows[args.RowIndex]);
                UpdateImportState();
            };
            _grid.CellMouseClick += (_, args) =>
            {
                int includeColumnIndex = _grid.Columns[IncludeColumn]?.Index ?? -1;
                if (args.RowIndex == -1 && args.ColumnIndex == includeColumnIndex)
                    SetAllIncluded(_includeHeaderState != CheckState.Checked);
            };
            _grid.CellPainting += Grid_CellPainting;
            _grid.DataError += (_, args) => args.ThrowException = false;
        }

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

        private static Label CreateCaption(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft
            };
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
