using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Land_Readjustment_Tool.UI.CustomControls
{
    public sealed class CanvasCreateFeaturesMenuOpeningEventArgs : EventArgs
    {
        public CanvasCreateFeaturesMenuOpeningEventArgs(
            IReadOnlyList<Guid> selectedObjectIds,
            ToolStripMenuItem menuItem)
        {
            SelectedObjectIds = selectedObjectIds;
            MenuItem = menuItem;
        }

        public IReadOnlyList<Guid> SelectedObjectIds { get; }

        public ToolStripMenuItem MenuItem { get; }
    }
}
