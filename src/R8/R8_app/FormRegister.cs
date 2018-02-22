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
    public partial class FormRegister : Form
    {
        private FormMain formMain = null;
        public FormRegister()
        {
            InitializeComponent();
        }

        private void FormRegister_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (formMain != null) {
                formMain.formRegister = null;
            }
        }

        private void FormRegister_Load(object sender, EventArgs e)
        {
            formMain = (FormMain)this.MdiParent;
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Your serial number is invalid.", "Alert");
        }
    }
}
