using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace R8
{
    public partial class FormAbout : Form
    {

        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            //this.textBoxMac.Text = R8.MacAddress;
        }


        private void labelAboutWebsite_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.openrobot.club");
        }

        private void FormAbout_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formAbout = null;
        }

        private void labelAboutEmail_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:openrobot@openrobot.club");
        }

        private void labelAboutR8_Click(object sender, EventArgs e)
        {

        }
    }
}
