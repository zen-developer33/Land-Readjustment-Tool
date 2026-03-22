using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Land_Readjustment_Tool.Core.Entities.Canvas;

namespace Land_Readjustment_Tool.UI.Forms
{
    /// <summary>
    /// Layer Property Manager — frmLayerManager
    ///
    /// PURPOSE:
    ///   Manage CanvasLayer objects: add, delete, reorder, set visibility/lock/print,
    ///   configure colors, line style, fill, and label settings.
    ///
    /// USAGE (open from frmMain or ribbon):
    ///   using var frm = new frmLayerManager(_layerManager.Layers.ToList());
    ///   if (frm.ShowDialog() == DialogResult.OK)
    ///       _layerManager.ApplyLayers(frm.ResultLayers);
    ///
    /// DESIGN DECISIONS:
    ///   - Works on a local COPY of layers — only commits on OK/Apply
    ///   - Color swatches are custom-painted in CellPainting (no 3rd party needed)
    ///   - Right panel syncs with selected grid row (master-detail pattern)
    ///   - Search filters the binding list without touching the source data
    /// </summary>
    public partial class frmLayerManager : Form
    {
        // ── State ────────────────────────────────────────────────────────────
        private List<CanvasLayer> _layers;          // working copy
        private CanvasLayer _selected;              // currently selected layer
        private bool _suppressSync = false;         // prevent feedback loop

        // ── Result (caller reads this after OK) ──────────────────────────────
        public IReadOnlyList<CanvasLayer> ResultLayers => _layers.AsReadOnly();

        // ── Constructor ──────────────────────────────────────────────────────
        public frmLayerManager(IEnumerable<CanvasLayer> existingLayers)
        {
            InitializeComponent();

            // Deep-copy so user can cancel without side effects
            _layers = existingLayers
                .Select(l => CloneLayer(l))
                .OrderBy(l => l.DisplayOrder)
                .ToList();

            BindGrid(_layers);
            UpdateLayerCount();
            SelectFirstRow();
        }

        // ════════════════════════════════════════════════════════════════════
        // Grid binding
        // ════════════════════════════════════════════════════════════════════

        private void BindGrid(IEnumerable<CanvasLayer> source)
        {
            dgvLayers.Rows.Clear();

            foreach (var layer in source)
            {
                int row = dgvLayers.Rows.Add();
                WriteRowFromLayer(dgvLayers.Rows[row], layer);
            }

            UpdateLayerCount();
        }

        private void WriteRowFromLayer(DataGridViewRow row, CanvasLayer layer)
        {
            row.Tag = layer;
            row.Cells["colVisible"].Value = layer.IsVisible;
            row.Cells["colLocked"].Value = layer.IsLocked;
            row.Cells["colPrintable"].Value = layer.IsPrintable;
            row.Cells["colColor"].Value = layer.BorderColor;   // hex string — painted custom
            row.Cells["colName"].Value = layer.Name;
            row.Cells["colLayerType"].Value = layer.LayerType;
            row.Cells["colLineStyle"].Value = layer.LineStyle;
            row.Cells["colLineWeight"].Value = layer.LineWeight.ToString("0.##");
            row.Cells["colDisplayOrder"].Value = layer.DisplayOrder;
        }

        // ════════════════════════════════════════════════════════════════════
        // Custom cell painting — color swatch column
        // ════════════════════════════════════════════════════════════════════

        private void dgvLayers_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex != dgvLayers.Columns["colColor"].Index || e.RowIndex < 0)
                return;

            e.PaintBackground(e.ClipBounds, true);

            string hex = e.Value?.ToString() ?? "#000000";
            Color c = ParseHex(hex);

            // Draw swatch centred in cell
            int sw = 28, sh = 14;
            int sx = e.CellBounds.X + (e.CellBounds.Width - sw) / 2;
            int sy = e.CellBounds.Y + (e.CellBounds.Height - sh) / 2;
            var swatchRect = new Rectangle(sx, sy, sw, sh);

            using var fill = new SolidBrush(c);
            using var border = new Pen(Color.FromArgb(120, 120, 120));
            e.Graphics.FillRectangle(fill, swatchRect);
            e.Graphics.DrawRectangle(border, swatchRect);

            e.Handled = true;
        }

        // ════════════════════════════════════════════════════════════════════
        // Selection → populate right-panel
        // ════════════════════════════════════════════════════════════════════

        private void dgvLayers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvLayers.SelectedRows.Count == 0) return;

            _selected = dgvLayers.SelectedRows[0].Tag as CanvasLayer;
            if (_selected == null) return;

            PopulateProperties(_selected);
        }

        private void PopulateProperties(CanvasLayer layer)
        {
            _suppressSync = true;
            try
            {
                // General tab
                txtLayerName.Text = layer.Name;
                cboLayerType.Text = layer.LayerType;
                pnlBorderColor.BackColor = ParseHex(layer.BorderColor);
                cboLineStyle.Text = layer.LineStyle;
                cboLineWeight.Text = layer.LineWeight.ToString("0.##");
                chkVisible.Checked = layer.IsVisible;
                chkLocked.Checked = layer.IsLocked;
                chkSelectable.Checked = layer.IsSelectable;
                chkPrintable.Checked = layer.IsPrintable;

                // Fill tab
                pnlFillColor.BackColor = ParseHex(layer.FillColor ?? "#FFFFFF");
                cboFillStyle.Text = layer.FillStyle;
                trkTransparency.Value = Math.Max(0, Math.Min(100, layer.FillTransparency));
                lblTranspValue.Text = $"{layer.FillTransparency}%";
                cboHatch.Text = layer.HatchPattern ?? string.Empty;
                cboHatch.Enabled = layer.FillStyle == "Hatched";

                // Label tab
                chkShowLabels.Checked = layer.ShowLabels;
                txtFontName.Text = layer.LabelFontName ?? "Segoe UI";
                numFontSize.Value = (decimal)Math.Max(4, Math.Min(72, layer.LabelFontSize));
                pnlLabelColor.BackColor = ParseHex(layer.LabelColor);
                cboLabelField.Text = layer.LabelField ?? string.Empty;
                SetLabelControlsEnabled(layer.ShowLabels);
            }
            finally
            {
                _suppressSync = false;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Grid inline edit → update layer object
        // ════════════════════════════════════════════════════════════════════

        private void dgvLayers_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Commit checkbox changes immediately (no extra click needed)
            if (dgvLayers.CurrentCell is DataGridViewCheckBoxCell)
                dgvLayers.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvLayers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _suppressSync) return;

            var row = dgvLayers.Rows[e.RowIndex];
            var layer = row.Tag as CanvasLayer;
            if (layer == null) return;

            string col = dgvLayers.Columns[e.ColumnIndex].Name;

            switch (col)
            {
                case "colVisible":
                    layer.IsVisible = (bool)(row.Cells[col].Value ?? true);
                    break;
                case "colLocked":
                    layer.IsLocked = (bool)(row.Cells[col].Value ?? false);
                    break;
                case "colPrintable":
                    layer.IsPrintable = (bool)(row.Cells[col].Value ?? true);
                    break;
                case "colName":
                    layer.Name = row.Cells[col].Value?.ToString() ?? layer.Name;
                    break;
                case "colLayerType":
                    layer.LayerType = row.Cells[col].Value?.ToString() ?? layer.LayerType;
                    break;
                case "colLineStyle":
                    layer.LineStyle = row.Cells[col].Value?.ToString() ?? "Solid";
                    break;
                case "colLineWeight":
                    if (double.TryParse(row.Cells[col].Value?.ToString(), out double lw))
                        layer.LineWeight = lw;
                    break;
                case "colDisplayOrder":
                    if (int.TryParse(row.Cells[col].Value?.ToString(), out int ord))
                        layer.DisplayOrder = ord;
                    break;
            }

            // Sync right panel if this row is selected
            if (_selected == layer)
                PopulateProperties(layer);
        }

        // ════════════════════════════════════════════════════════════════════
        // Color swatch click in grid — open color picker
        // ════════════════════════════════════════════════════════════════════

        private void dgvLayers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvLayers.Columns[e.ColumnIndex].Name != "colColor") return;

            var layer = dgvLayers.Rows[e.RowIndex].Tag as CanvasLayer;
            if (layer == null) return;

            colorDialog1.Color = ParseHex(layer.BorderColor);
            if (colorDialog1.ShowDialog() != DialogResult.OK) return;

            layer.BorderColor = ToHex(colorDialog1.Color);
            dgvLayers.Rows[e.RowIndex].Cells["colColor"].Value = layer.BorderColor;
            dgvLayers.InvalidateRow(e.RowIndex);

            if (_selected == layer)
                pnlBorderColor.BackColor = colorDialog1.Color;
        }

        // ════════════════════════════════════════════════════════════════════
        // Right panel → write back to selected layer
        // ════════════════════════════════════════════════════════════════════

        private void SyncSelectedLayer()
        {
            if (_selected == null || _suppressSync) return;

            _selected.Name = txtLayerName.Text.Trim();
            _selected.LayerType = cboLayerType.Text;
            _selected.BorderColor = ToHex(pnlBorderColor.BackColor);
            _selected.LineStyle = cboLineStyle.Text;
            if (double.TryParse(cboLineWeight.Text, out double lw)) _selected.LineWeight = lw;
            _selected.IsVisible = chkVisible.Checked;
            _selected.IsLocked = chkLocked.Checked;
            _selected.IsSelectable = chkSelectable.Checked;
            _selected.IsPrintable = chkPrintable.Checked;

            _selected.FillColor = ToHex(pnlFillColor.BackColor);
            _selected.FillStyle = cboFillStyle.Text;
            _selected.FillTransparency = trkTransparency.Value;
            _selected.HatchPattern = cboFillStyle.Text == "Hatched" ? cboHatch.Text : null;

            _selected.ShowLabels = chkShowLabels.Checked;
            _selected.LabelFontName = txtFontName.Text;
            _selected.LabelFontSize = (double)numFontSize.Value;
            _selected.LabelColor = ToHex(pnlLabelColor.BackColor);
            _selected.LabelField = cboLabelField.Text;

            // Refresh the grid row
            RefreshSelectedRow();
        }

        private void RefreshSelectedRow()
        {
            if (dgvLayers.SelectedRows.Count == 0) return;
            var row = dgvLayers.SelectedRows[0];
            _suppressSync = true;
            WriteRowFromLayer(row, _selected);
            dgvLayers.InvalidateRow(row.Index);
            _suppressSync = false;
        }

        // ════════════════════════════════════════════════════════════════════
        // Toolbar: New / Delete / Move
        // ════════════════════════════════════════════════════════════════════

        private void btnNewLayer_Click(object sender, EventArgs e)
        {
            var layer = new CanvasLayer
            {
                Name = $"New Layer {_layers.Count + 1}",
                LayerType = "Reference",
                BorderColor = "#000000",
                LineStyle = "Solid",
                LineWeight = 1.0,
                IsVisible = true,
                IsSelectable = true,
                IsPrintable = true,
                FillStyle = "None",
                LabelColor = "#000000",
                DisplayOrder = _layers.Count,
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now
            };

            _layers.Add(layer);
            int row = dgvLayers.Rows.Add();
            WriteRowFromLayer(dgvLayers.Rows[row], layer);
            dgvLayers.ClearSelection();
            dgvLayers.Rows[row].Selected = true;
            dgvLayers.CurrentCell = dgvLayers.Rows[row].Cells["colName"];
            dgvLayers.BeginEdit(true);
            UpdateLayerCount();
        }

        private void btnDeleteLayer_Click(object sender, EventArgs e)
        {
            if (dgvLayers.SelectedRows.Count == 0) return;

            var layer = dgvLayers.SelectedRows[0].Tag as CanvasLayer;
            if (layer == null) return;

            var answer = MessageBox.Show(
                $"Delete layer \"{layer.Name}\"?\nShapes on this layer will be moved to the Default layer.",
                "Delete Layer — RePlot",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes) return;

            int idx = dgvLayers.SelectedRows[0].Index;
            _layers.Remove(layer);
            dgvLayers.Rows.RemoveAt(idx);
            UpdateLayerCount();
            SelectRow(Math.Min(idx, dgvLayers.Rows.Count - 1));
        }

        private void btnMoveUp_Click(object sender, EventArgs e) => MoveSelected(-1);
        private void btnMoveDown_Click(object sender, EventArgs e) => MoveSelected(+1);

        private void MoveSelected(int direction)
        {
            if (dgvLayers.SelectedRows.Count == 0) return;
            int idx = dgvLayers.SelectedRows[0].Index;
            int target = idx + direction;
            if (target < 0 || target >= dgvLayers.Rows.Count) return;

            // Swap in _layers list
            var a = dgvLayers.Rows[idx].Tag as CanvasLayer;
            var b = dgvLayers.Rows[target].Tag as CanvasLayer;
            if (a == null || b == null) return;

            int ia = _layers.IndexOf(a);
            int ib = _layers.IndexOf(b);
            _layers[ia] = b;
            _layers[ib] = a;

            // Swap display orders
            (a.DisplayOrder, b.DisplayOrder) = (b.DisplayOrder, a.DisplayOrder);

            // Refresh grid
            _suppressSync = true;
            WriteRowFromLayer(dgvLayers.Rows[idx], b);
            WriteRowFromLayer(dgvLayers.Rows[target], a);
            _suppressSync = false;

            dgvLayers.ClearSelection();
            dgvLayers.Rows[target].Selected = true;
        }

        // ════════════════════════════════════════════════════════════════════
        // Toolbar: Show/Hide/Lock All
        // ════════════════════════════════════════════════════════════════════

        private void btnShowAll_Click(object sender, EventArgs e) => SetAllVisibility(true);
        private void btnHideAll_Click(object sender, EventArgs e) => SetAllVisibility(false);

        private void SetAllVisibility(bool visible)
        {
            foreach (var l in _layers) l.IsVisible = visible;
            foreach (DataGridViewRow row in dgvLayers.Rows)
                row.Cells["colVisible"].Value = visible;
        }

        private void btnLockAll_Click(object sender, EventArgs e)
        {
            bool allLocked = _layers.All(l => l.IsLocked);
            bool newState = !allLocked;
            foreach (var l in _layers) l.IsLocked = newState;
            foreach (DataGridViewRow row in dgvLayers.Rows)
                row.Cells["colLocked"].Value = newState;
        }

        // ════════════════════════════════════════════════════════════════════
        // Search / filter
        // ════════════════════════════════════════════════════════════════════

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string term = txtSearch.Text.Trim().ToLowerInvariant();
            var filtered = string.IsNullOrEmpty(term)
                ? _layers
                : _layers.Where(l =>
                    l.Name.ToLowerInvariant().Contains(term) ||
                    l.LayerType.ToLowerInvariant().Contains(term)).ToList();

            _suppressSync = true;
            BindGrid(filtered);
            _suppressSync = false;
            SelectFirstRow();
        }

        // ════════════════════════════════════════════════════════════════════
        // Color pickers (right panel)
        // ════════════════════════════════════════════════════════════════════

        private void pnlBorderColor_Click(object sender, EventArgs e) => PickColor(pnlBorderColor);
        private void btnBorderColor_Click(object sender, EventArgs e) => PickColor(pnlBorderColor);
        private void pnlFillColor_Click(object sender, EventArgs e) => PickColor(pnlFillColor);
        private void btnFillColor_Click(object sender, EventArgs e) => PickColor(pnlFillColor);
        private void pnlLabelColor_Click(object sender, EventArgs e) => PickColor(pnlLabelColor);
        private void btnLabelColor_Click(object sender, EventArgs e) => PickColor(pnlLabelColor);

        private void PickColor(Panel swatch)
        {
            colorDialog1.Color = swatch.BackColor;
            if (colorDialog1.ShowDialog() != DialogResult.OK) return;
            swatch.BackColor = colorDialog1.Color;
            SyncSelectedLayer();
        }

        // ════════════════════════════════════════════════════════════════════
        // Fill style — enable/disable hatch combo
        // ════════════════════════════════════════════════════════════════════

        private void cboFillStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            cboHatch.Enabled = cboFillStyle.Text == "Hatched";
            SyncSelectedLayer();
        }

        // ════════════════════════════════════════════════════════════════════
        // Transparency slider
        // ════════════════════════════════════════════════════════════════════

        private void trkTransparency_Scroll(object sender, EventArgs e)
        {
            lblTranspValue.Text = $"{trkTransparency.Value}%";
            SyncSelectedLayer();
        }

        // ════════════════════════════════════════════════════════════════════
        // Labels tab
        // ════════════════════════════════════════════════════════════════════

        private void chkShowLabels_CheckedChanged(object sender, EventArgs e)
        {
            SetLabelControlsEnabled(chkShowLabels.Checked);
            SyncSelectedLayer();
        }

        private void SetLabelControlsEnabled(bool enabled)
        {
            txtFontName.Enabled = enabled;
            btnPickFont.Enabled = enabled;
            numFontSize.Enabled = enabled;
            pnlLabelColor.Enabled = enabled;
            btnLabelColor.Enabled = enabled;
            cboLabelField.Enabled = enabled;
        }

        private void btnPickFont_Click(object sender, EventArgs e)
        {
            fontDialog1.Font = new Font(
                string.IsNullOrEmpty(txtFontName.Text) ? "Segoe UI" : txtFontName.Text,
                (float)numFontSize.Value);

            if (fontDialog1.ShowDialog() != DialogResult.OK) return;
            txtFontName.Text = fontDialog1.Font.Name;
            numFontSize.Value = (decimal)fontDialog1.Font.Size;
            SyncSelectedLayer();
        }

        // ════════════════════════════════════════════════════════════════════
        // OK / Apply / Cancel
        // ════════════════════════════════════════════════════════════════════

        private void btnApply_Click(object sender, EventArgs e)
        {
            SyncSelectedLayer();
            RenumberDisplayOrders();
            // Caller can read ResultLayers at any time after Apply
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SyncSelectedLayer();
            RenumberDisplayOrders();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private void RenumberDisplayOrders()
        {
            for (int i = 0; i < _layers.Count; i++)
                _layers[i].DisplayOrder = i;
        }

        private void UpdateLayerCount()
        {
            lblLayerCount.Text = $"{_layers.Count} layer{(_layers.Count == 1 ? "" : "s")}";
        }

        private void SelectFirstRow()
        {
            if (dgvLayers.Rows.Count > 0)
                SelectRow(0);
        }

        private void SelectRow(int index)
        {
            if (index < 0 || index >= dgvLayers.Rows.Count) return;
            dgvLayers.ClearSelection();
            dgvLayers.Rows[index].Selected = true;
        }

        private static Color ParseHex(string hex)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex)) return Color.Black;
                return ColorTranslator.FromHtml(hex.StartsWith("#") ? hex : "#" + hex);
            }
            catch { return Color.Black; }
        }

        private static string ToHex(Color c) =>
            $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static CanvasLayer CloneLayer(CanvasLayer src) => new CanvasLayer
        {
            Id = src.Id,
            Name = src.Name,
            LayerType = src.LayerType,
            IsVisible = src.IsVisible,
            IsLocked = src.IsLocked,
            IsSelectable = src.IsSelectable,
            IsPrintable = src.IsPrintable,
            DisplayOrder = src.DisplayOrder,
            BorderColor = src.BorderColor,
            LineWeight = src.LineWeight,
            LineStyle = src.LineStyle,
            FillColor = src.FillColor,
            FillTransparency = src.FillTransparency,
            FillStyle = src.FillStyle,
            HatchPattern = src.HatchPattern,
            ShowLabels = src.ShowLabels,
            LabelFontName = src.LabelFontName,
            LabelFontSize = src.LabelFontSize,
            LabelColor = src.LabelColor,
            LabelField = src.LabelField,
            SourceFile = src.SourceFile,
            ImportedDate = src.ImportedDate,
            CreatedDate = src.CreatedDate,
            LastModifiedDate = src.LastModifiedDate
        };
    }
}