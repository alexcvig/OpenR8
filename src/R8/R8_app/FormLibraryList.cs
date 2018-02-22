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
    public partial class FormLibraryList : Form
    {

        public class LibraryIsEnableRecord //紀錄該 Library 是否 enable
        {
            public string name = "";
            public bool boolValue = true;
            public LibraryIsEnableRecord(String name, bool boolValue)
            {
                this.name = name;
                this.boolValue = boolValue;
            }
        }
        private List<CheckBox> checkBoxs = new List<CheckBox>();
        private List<LibraryIsEnableRecord> libraryIsEnableRecords = new List<LibraryIsEnableRecord>();
        private FormMain formMain = null;
        //string disableString = "-disable";
        string disableString = "-disabled";
        public FormLibraryList()
        {
            InitializeComponent();
        }

        public FormLibraryList(string[] dirNames)
        {
            InitializeComponent();

            string tempString = "";
            string dirChar = "\\";
            
            //string disableString = "15";
            for(int i = 0; i < dirNames.Length; i++)  
            {
                string dirName = dirNames[i];
                bool isEnable = true;
                tempString = "";
                if (dirName.LastIndexOf(dirChar) != -1 && dirName.LastIndexOf(dirChar) < dirName.Length)
                {
                    tempString = dirName.Substring(dirName.LastIndexOf(dirChar) + 1);
                }
                if (tempString.Length - disableString.Length > 0) {
                    string targetStr = tempString.Substring(tempString.Length - disableString.Length);
                    if (targetStr.CompareTo(disableString) == 0) {
                        isEnable = false;
                        tempString = tempString.Substring(0, tempString.Length - disableString.Length);
                    }
                }
                string fullDirPath = dirName;
                if (!isEnable) {
                    fullDirPath = fullDirPath.Substring(0, fullDirPath.Length - disableString.Length);
                }
                LibraryIsEnableRecord libraryIsEnableRecord = new LibraryIsEnableRecord(fullDirPath, isEnable);
                addCheckBox(tempString, isEnable);
                libraryIsEnableRecords.Add(libraryIsEnableRecord);
            }
        }


        private void FormLibraryList_Load(object sender, EventArgs e)
        {
            formMain = (FormMain)this.MdiParent;
        }

        public void addCheckBox(string text, bool isEnable)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Name = text;
            checkBox.Text = text;
            //MessageBox.Show(text.ToString());
            //checkBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            checkBox.Width = this.Width - 100;
            //checkBox.Height = 40;
            int upPadding = 10;
            if (checkBoxs.Count == 0)
            {
                checkBox.Location = new Point(0, 0 + upPadding);
            }
            else
            {
                checkBox.Location = new Point(0, checkBoxs.ElementAt(checkBoxs.Count - 1).Location.Y + checkBoxs.ElementAt(checkBoxs.Count - 1).Height + 5);
            }

            checkBox.Checked = isEnable;
            //buttonsLocationY += button.Height + 5;
            // checkBox.Click += new System.EventHandler(this.variableButton_Click);
            //checkBox.Tag = sn;
            //button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttons_MouseDown);
            this.panel1.Controls.Add(checkBox);
            checkBoxs.Add(checkBox);
            return;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            //Directory.Move(source, destination);
            for (int i = 0; i < libraryIsEnableRecords.Count; i++) {
                if (checkBoxs[i].Checked != libraryIsEnableRecords[i].boolValue)
                {
                    if (checkBoxs[i].Checked)
                    { //從 disable 變成 enable
                        Directory.Move(libraryIsEnableRecords[i].name + disableString, libraryIsEnableRecords[i].name);
                    }
                    else
                    {  //從 enable 變成 disable
                        Directory.Move(libraryIsEnableRecords[i].name, libraryIsEnableRecords[i].name + disableString);
                    }
                }
            }
            
            this.Close();
            MessageBox.Show("Please restart OpenR8 to enable libraries.");
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormLibraryList_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (formMain != null) {
                if (formMain.formLibraryList != null)
                {
                    formMain.formLibraryList = null;
                }
            }
        }

    }
}
