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
        private string str_AboutR8 = "OpenR8 Community Edition.";
        private string str_Copyright = "© 2004-2018 Open Robot Club";
        private string str_Website = "Website: http://www.openrobot.club";
        private string str_Email = "Contact Email: openrobot@openrobot.club";
        private string str_label1 = "This Community Edition is only for non-commercial application.";
        private string str_label2 = "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY.";
        private string str_label3 = "Bug Report: openrobot@openrobot.club";
        private string str_label4 = "For commercial application, please contact us to purchase Commercial License.";



        public FormAbout()
        {
            InitializeComponent();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            //this.textBoxMac.Text = R8.MacAddress;
            str_AboutR8 = R8.TranslationString(str_AboutR8);
            this.labelAboutR8.Text = str_AboutR8;
            str_Copyright = R8.TranslationString(str_Copyright);
            this.labelAboutCopyright.Text = str_Copyright;
            str_Website = R8.TranslationString(str_Website);
            this.labelAboutWebsite.Text = str_Website;
            str_Email = R8.TranslationString(str_Email);
            this.labelAboutEmail.Text = str_Email;
            str_label1 = R8.TranslationString(str_label1);
            this.label1.Text = str_label1;
            str_label2 = R8.TranslationString(str_label2);
            this.label2.Text = str_label2;
            str_label3 = R8.TranslationString(str_label3);
            this.label3.Text = str_label3;
            str_label4 = R8.TranslationString(str_label4);
            this.label4.Text = str_label4;
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
