using System;
using System.Windows.Forms;

namespace NPSTransectTool
{
    public partial class HasTransectForm : Form
    {
        private FormOptions m_UserFormOptions;

        public FormOptions UserFormOptions { get { return m_UserFormOptions; } }

        public HasTransectForm()
        {
            InitializeComponent();
        }

        private void HasTransectForm_Load(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.None;
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.Yes;
            Close();
        }

        private void btnYesToAll_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.YesAll;
            Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.No;
            Close();
        }

        private void btnNoAll_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.NoAll;
            Close();
        }
    }

    public enum FormOptions
    {
        Yes,
        No,
        NoAll,
        YesAll,
        None
    }
}
