using System.ComponentModel;
using System.Reflection;

namespace Land_Readjustment_Tool
{
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(SplitContainer))]
    [Description("A flicker-free SplitContainer with double buffering enabled")]
    public class FlickerFreeSplitContainer : SplitContainer
    {
        public FlickerFreeSplitContainer()
        {
            // Enable double buffering for the split container itself
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // Enable double buffering for both internal panels
            EnableDoubleBuffering(Panel1);
            EnableDoubleBuffering(Panel2);
        }

        private void EnableDoubleBuffering(Control control)
        {
            // Skip in terminal server sessions
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            // Use reflection to set the protected DoubleBuffered property
            PropertyInfo property = typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance);

            property?.SetValue(control, true, null);
        }

        // Prevent background erase to reduce flicker
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs e)
        {
            // Intentionally empty - don't erase background
        }
    }
}

// USAGE:
// 1. Add this class to your project
// 2. Build the project
// 3. The FlickerFreeSplitContainer will appear in the Toolbox
// 4. Drag it onto your form like a regular SplitContainer
//
// Or use it in code:
// var splitContainer = new FlickerFreeSplitContainer
// {
//     Dock = DockStyle.Fill,
//     Orientation = Orientation.Vertical
// };
// this.Controls.Add(splitContainer);