using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NPSTransectTool
{
    public partial class HasTransectForm : Form
    {
        private FormOptions m_UserFormOptions;

        public FormOptions UserFormOptions{get { return m_UserFormOptions; }}

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
            this.Close();
        }

        private void btnYesToAll_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.YesAll;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.No;
            this.Close();
        }

        private void btnNoAll_Click(object sender, EventArgs e)
        {
            m_UserFormOptions = FormOptions.NoAll;
            this.Close();
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
