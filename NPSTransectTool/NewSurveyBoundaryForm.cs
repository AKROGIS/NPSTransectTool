using System;
using System.Windows.Forms;

namespace NPSTransectTool
{
    public partial class NewSurveyBoundaryForm : Form
    {
        public NewSurveyBoundaryForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
