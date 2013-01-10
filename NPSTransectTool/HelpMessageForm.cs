using System;
using System.Windows.Forms;

namespace NPSTransectTool
{
    public partial class HelpMessageForm : Form
    {
        public HelpMessageForm()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
