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
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void linkLabelGetLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //System.Diagnostics.Process.Start("http://www.openrobot.club");//對應的網頁端還沒有，暫時先導到首頁
            System.Diagnostics.Process.Start("http://www.openrobot.club/user/register");
        }

        private void forgetPasswordLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.openrobot.club/user/password_forget");
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

            StringBuilder resultBuffer = new StringBuilder(R7.STRING_SIZE);
            StringBuilder email = new StringBuilder(R7.STRING_SIZE);
            StringBuilder password = new StringBuilder(R7.STRING_SIZE);
            email.Append(textBoxEmail.Text);
            password.Append(textBoxPassword.Text);
            R7.Login(resultBuffer, R7.STRING_SIZE, email, password);
            //MessageBox.Show("login test" + resultBuffer);

            int year = 0, month = 0, day = 0;
            R7.CheckLicense(resultBuffer, ref year, ref month, ref day);

            if (new DateTime(year, month, day) < DateTime.Now)
            {
                //以後當 license 過期時，應該是在這邊跳 FormLogin 出來
                //MessageBox.Show("Login not success:" + resultBuffer);
                if (resultBuffer.Length == 0)
                {
                    MessageBox.Show("Internet connection to www.openrobot.club is not available!");
                }
                else {
                    MessageBox.Show(resultBuffer.ToString());
                }
                
            }
            else {
                R8.IsLogin = true;
                this.Close();
            }

            
        }

        //讓它按 Enter 也等於按 OK ，http://blog.csdn.net/bydxyj/article/details/3987412
        private void txtPwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.buttonOK_Click(sender, e);
            }
        }
    }
}
