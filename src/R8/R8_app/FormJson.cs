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
    public partial class FormJson : Form
    {
        public FormJson()
        {
            InitializeComponent();
        }

        public void setText(String str, String title) {
            //20170407 json 要換行，換在 { 符號
            str = str.Replace(" {", "\n{");
            richTextBox1.Text = str;
            this.Text = title;
            return;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Text Files|*.txt";
                saveFileDialog1.Title = "Save Json";
                saveFileDialog1.InitialDirectory = FormMain.workSpacePath;
                saveFileDialog1.FileName = "json.txt";
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog1.FileName, richTextBox1.Text.ToString().Replace("\n", "\r\n"));
                }
            }
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();       
            Clipboard.SetText(richTextBox1.Text.ToString());
        }

        private void FormJson_FormClosed(object sender, FormClosedEventArgs e)
        {
            //FormMain formMain = this.MdiParent as FormMain;
            //formMain.formJson = null;
        }

        private void FormJson_Load(object sender, EventArgs e)
        {

        }
    }
}
