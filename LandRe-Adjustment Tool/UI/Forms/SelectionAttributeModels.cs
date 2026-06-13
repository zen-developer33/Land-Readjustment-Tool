using System;
using System.Collections.Generic;
using System.Linq;

namespace Land_Readjustment_Tool.UI.Forms
{
    public sealed class SelectionAttributeLayer
    {
        public SelectionAttributeLayer(
            int? layerId,
            string name,
            bool isSelectable,
            IEnumerable<SelectionAttributeRow> rows)
        {
            LayerId = layerId;
            Name = string.IsNullOrWhiteSpace(name) ? "Unnamed Layer" : name;
            IsSelectable = isSelectable;
            Rows = rows.ToList();
        }

        public int? LayerId { get; }
        public string Name { get; }
        public bool IsSelectable { get; }
        public IReadOnlyList<SelectionAttributeRow> Rows { get; }

        public override string ToString() => Name;
    }

    public sealed class SelectionAttributeRow
    {
        public SelectionAttributeRow(
            Guid canvasObjectId,
            IReadOnlyDictionary<string, object?> values,
            IReadOnlyDictionary<string, string> labels)
        {
            CanvasObjectId = canvasObjectId;
            Values = values;
            Labels = labels;
        }

        public Guid CanvasObjectId { get; }
        public IReadOnlyDictionary<string, object?> Values { get; }
        public IReadOnlyDictionary<string, string> Labels { get; }
    }

    public sealed class ObjectTypeSelectorItem
    {
        public ObjectTypeSelectorItem(int recordId, string displayName, IEnumerable<Guid> canvasObjectIds)
        {
            RecordId = recordId;
            DisplayName = string.IsNullOrWhiteSpace(displayName)
                ? $"Record {recordId}"
                : displayName.Trim();
            CanvasObjectIds = canvasObjectIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToArray();
        }

        public int RecordId { get; }
        public string DisplayName { get; }
        public IReadOnlyList<Guid> CanvasObjectIds { get; }
        public bool CanSelect => CanvasObjectIds.Count > 0;

        public override string ToString() =>
            CanSelect ? DisplayName : $"{DisplayName} (no map object)";
    }
}
