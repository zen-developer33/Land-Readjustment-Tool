using Land_Readjustment_Tool.Services;

namespace Land_Readjustment_Tool
{
    public partial class frmAreaConverter : Form
    {
        public frmAreaConverter()
        {
            InitializeComponent();
        }

        private void frmAreaConverter_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rapd = "";
            float sqm = textbox1.Text != "" ? float.Parse(textbox1.Text) : 0;
            rapd = AreaConverterService.SqmToRAPDString(sqm);
            textBox2.Text = rapd;
        }
    }
}
