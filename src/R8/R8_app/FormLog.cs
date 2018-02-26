using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace R8
{
    public partial class FormLog : Form
    {
        private FormMain formMain;
        private string str_Log = "Log";
        public FormLog()
        {
            InitializeComponent();
        }

        private void FormLog_Load(object sender, EventArgs e)
        {
            richTextBox1.Height = this.ClientSize.Height - 15;
            richTextBox1.Width = this.ClientSize.Width - 15 - buttonClear.Width;
            formMain = (FormMain)this.MdiParent;

            str_Log = R8.TranslationString(str_Log);
            this.Text = str_Log;
        }
        public RichTextBox getRichTextBox() {
            return richTextBox1;
        }


        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void FormLog_SizeChanged(object sender, EventArgs e)
        {
            richTextBox1.Height = this.ClientSize.Height - 15;
            richTextBox1.Width = this.ClientSize.Width - 15 - buttonClear.Width;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            R8.clearLogBox();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text != null) {

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Text Files|*.txt";
                saveFileDialog1.Title = "Save Log File";
                saveFileDialog1.InitialDirectory = FormMain.workSpacePath;
                saveFileDialog1.FileName = "log.txt";
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //R8.writeProgramXml(saveFileDialog1.FileName);
                    File.WriteAllText(saveFileDialog1.FileName, richTextBox1.Text.ToString().Replace("\n", "\r\n"));
                }
            }
        }

        private void FormLog_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formLog = null;
        }
    }
}
