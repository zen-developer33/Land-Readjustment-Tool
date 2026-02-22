using System;
using System.Windows.Forms;

namespace Drawing_Canvas_Practice
{
    /// <summary>
    /// Simple launcher form to choose between old and new versions
    /// </summary>
    public partial class LauncherForm : Form
    {
        public LauncherForm()
        {
            InitializeComponent();
        }

        private void btnOldVersion_Click(object sender, EventArgs e)
        {
            // Open old version
            var oldForm = new frmDrawingCanvas();
            oldForm.Show();
        }

        private void btnNewVersion_Click(object sender, EventArgs e)
        {
            // Open new refactored version
            var newForm = new UI.frmDrawingCanvasRefactored();
            newForm.Show();
        }

        private void LauncherForm_Load(object sender, EventArgs e)
        {
            // Optional: Add any initialization code here
        }
    }
}