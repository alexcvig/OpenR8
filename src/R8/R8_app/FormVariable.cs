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
    public partial class FormVariable : Form
    {

        private FormMain formMain;
        //public int variableSn = -1;
        public int variableSn = 0;
        private string value = "";
        ComboBox valueComboBox = null;
        NumericUpDown valueNumericUpDown = null;
        TextBox valueTextBox = null;
        private string preVariableType = "";
        private string str_Variable = "Variable";
        public FormVariable()
        {
            InitializeComponent();
        }

        private void FormVariable_Load(object sender, EventArgs e)
        {
            formMain = (FormMain)this.MdiParent;
            //comboBox1.DataSource = Enum.GetValues(typeof(Data.VariableType));
            //comboBox1.DataSource = R8.typeArray;
            int variableNum = R8.GetVariableNum();
            //MessageBox.Show("variableNum = " + variableNum);
            variableNum = R8.GetVariableNum();
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            string[] typeArray;
            if (variableNum > 0)
            {
                typeArray = new string[variableNum];
                for (int i = 0; i < variableNum; i++)
                {
                    R8.GetVariableType(str, R7.STRING_SIZE, i);
                    typeArray[i] = str.ToString();
                   // MessageBox.Show("typeArray" + i + " = " + str);
                }
                comboBox1.DataSource = typeArray;
            }
            else {
                //如果沒讀到，用預設值
                comboBox1.DataSource = new string[] {"int", "bool", "float", "double", "string", "image", "json" };
            }

            //20170327 Variable 沒 SN ，不顯示視窗元件。
            if (variableSn == -1 || variableSn == 0)
            {
                comboBox1.Visible = false;
                label1.Visible = false;
                labelValue.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                labelSn.Visible = false;
                textBoxName.Visible = false;
                label5.Visible = false;
                textBoxRemark.Visible = false;
                buttonDelete.Visible = false;
            }
            else {
                comboBox1.Visible = true;
                label1.Visible = true;
                labelValue.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
                labelSn.Visible = true;
                textBoxName.Visible = true;
                label5.Visible = true;
                textBoxRemark.Visible = true;
                buttonDelete.Visible = true;
            }

            str_Variable = R8.TranslationString(str_Variable);
            this.Text = str_Variable;
        }

        public void focusToTextBoxName() {
            textBoxName.Focus();
            return;
        }


        //20170327 leo: 依今天信件增加 [選到 variable 時，自動 focus 到 Value] 功能。
        public void focusToValueBox()
        {
            //20170531 leo: 依信件指示改為[Edit 時，如果 variable name 是預設值，則 focus 到 Variable From Name ，如果 variable name 有改過，則 focus 到 Variable From Value ]
            if (variableSn != -1 && variableSn != 0)
            {

                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, FormMain.r8.variables[variableSn].type);

                if ((str.ToString() + variableSn.ToString()).Equals(FormMain.r8.variables[variableSn].name)) {
                    textBoxName.Focus();

                }
                else if (valueComboBox != null)
                {
                    valueComboBox.Focus();
                }
                else if (valueNumericUpDown != null)
                {
                    valueNumericUpDown.Focus();
                }
                else if (valueTextBox != null)
                {
                    valueTextBox.Focus();
                }


                preVariableType = str.ToString();

                str.Clear();
            }



            
            return;
        }


        bool isShowVariableRunning = false;
        public void showVariable(int variableSn) {
            //20170315 leo: 發現介面 bug ：若 showVariable 運行中時，又觸發一次 showVariable ，會導致 FormVariable 介面錯亂。
            //加個 bool 變數，若 showVariable 運行中，則不再觸發
            if (isShowVariableRunning) {
                return;
            }
            isShowVariableRunning = true;
            //System.Console.WriteLine("showVariable variableSn " + variableSn + ", " + this.variableSn);
            if (this.variableSn != variableSn) {
                //variableSn 更換， setVariable
               // setVariable();//20170214 leo: 改為 任何一個FormVariable 的零件 unfocus 時觸發
            }
            this.variableSn = variableSn;
            if (variableSn == -1 || variableSn == 0)
            {
                textBoxName.Text = "";
                labelSn.Text = "";
                this.comboBox1.SelectedIndex = 0;
                textBoxRemark.Text = "";
                if (valueComboBox != null)
                {
                    valueComboBox.Dispose();
                    valueComboBox = null;
                }
                if (valueNumericUpDown != null)
                {
                    valueNumericUpDown.Dispose();
                    valueNumericUpDown = null;
                }
                if (valueTextBox != null)
                {
                    valueTextBox.Dispose();
                    valueTextBox = null;
                }

                //20170327 Variable 沒 SN ，不顯示視窗元件。
                comboBox1.Visible = false;
                label1.Visible = false;
                labelValue.Visible = false;
                label3.Visible = false;
                label4.Visible = false;
                labelSn.Visible = false;
                textBoxName.Visible = false;
                label5.Visible = false;
                textBoxRemark.Visible = false;
                buttonDelete.Visible = false;
            }
            else
            {
                //20170327 Variable 沒 SN ，不顯示視窗元件。
                comboBox1.Visible = true;
                label1.Visible = true;
                labelValue.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
                labelSn.Visible = true;
                textBoxName.Visible = true;
                label5.Visible = true;
                textBoxRemark.Visible = true;
                buttonDelete.Visible = true;

                textBoxName.Text = FormMain.r8.variables[variableSn].name;
                labelSn.Text = FormMain.r8.variables[variableSn].sn.ToString();
                //20170117 leo: type 改為不鎖定
                //this.comboBox1.Enabled = true;
                this.comboBox1.SelectedIndex = FormMain.r8.variables[variableSn].type;
                //this.comboBox1.Enabled = false;
                value = FormMain.r8.variables[variableSn].value;
                this.textBoxRemark.Text = FormMain.r8.variables[variableSn].remark;

                //如果 type 為 image ，不顯示 value
                if (FormMain.r8.variables[variableSn].type == R8.getVariableTypeByString("image") 
                    || FormMain.r8.variables[variableSn].type == R8.getVariableTypeByString("json")
                    || FormMain.r8.variables[variableSn].type == R8.getVariableTypeByString("object")
                    )
                {
                    FormMain.r8.variables[variableSn].value = "";
                    //this.textBoxValue.Visible = false;
                }
                else {
                    //this.textBoxValue.Visible = true;
                }
                //20170120 leo value 欄位依 type 切換元件。 textBox / comboBox / domainUpDown
                Point valueLocation = new Point(this.labelValue.Location.X + this.labelValue.Width + 10, this.labelValue.Location.Y);

                

                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, FormMain.r8.variables[variableSn].type);

                //20170330 leo: 切換元件時，若[type沒換]則不進行切換 (避免設置 value，按 enter 然後focus跳掉)
                bool needChangeValueBox = true;
                if (preVariableType.Equals(str.ToString())) {
                    needChangeValueBox = false;
                }

                preVariableType = str.ToString();

                if (needChangeValueBox)
                {
                    if (valueComboBox != null)
                    {
                        valueComboBox.Dispose();
                        valueComboBox = null;
                    }
                    if (valueNumericUpDown != null)
                    {
                        valueNumericUpDown.Dispose();
                        valueNumericUpDown = null;
                    }
                    if (valueTextBox != null)
                    {
                        valueTextBox.Dispose();
                        valueTextBox = null;
                    }
                }
                switch (str.ToString())
                {
                    case "bool":
                    case "Bool":
                    case "boolean":
                    case "Boolean":
                        //comboBox
                        {
                            if (needChangeValueBox || valueComboBox == null)
                            {
                                valueComboBox = new ComboBox();

                                valueComboBox.Items.AddRange(new string[] { "False", "True" });
                                int res = 0;
                                if (!Int32.TryParse(value, out res))
                                {
                                    res = 0;
                                }
                                if (res >= 1)
                                {
                                    res = 1;
                                }
                                else
                                {
                                    res = 0;
                                }
                                valueComboBox.SelectedIndex = res;
                                valueComboBox.Location = valueLocation;
                                valueComboBox.SelectedIndexChanged += valueComboBox_SelectedIndexChanged;
                                //valueComboBox.LostFocus += buttonSet_Click;
                                valueComboBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
                                valueComboBox.Leave += new System.EventHandler(this.focus_Leave);
                                valueComboBox.TabIndex = 4;
                                this.Controls.Add(valueComboBox);
                            }
                            else {
                                int res = 0;
                                if (!Int32.TryParse(value, out res))
                                {
                                    res = 0;
                                }
                                if (res >= 1)
                                {
                                    res = 1;
                                }
                                else
                                {
                                    res = 0;
                                }
                                valueComboBox.SelectedIndex = res;
                            }
                        }
                        break;
                    case "double":
                    case "Double":
                        //numericUpDown
                        {
                            if (needChangeValueBox || valueNumericUpDown == null)
                            {
                                valueNumericUpDown = new NumericUpDown();
                                valueNumericUpDown.Maximum = (decimal)int.MaxValue;
                                valueNumericUpDown.Minimum = (decimal)int.MinValue;
                                valueNumericUpDown.Increment = (decimal)0.5;
                                valueNumericUpDown.DecimalPlaces = 3;
                                valueNumericUpDown.Text = value;
                                valueNumericUpDown.Location = valueLocation;
                                valueNumericUpDown.TextChanged += valueNumericUpDown_TextChanged;
                                valueNumericUpDown.Leave += new System.EventHandler(this.focus_Leave);
                                valueNumericUpDown.TabIndex = 4;
                                valueNumericUpDown.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
                                //valueNumericUpDown.LostFocus += buttonSet_Click;
                                this.Controls.Add(valueNumericUpDown);
                            }
                            else {
                                valueNumericUpDown.Text = value;
                            }
                        }
                        break;
                    case "float":
                    case "Float":
                        //numericUpDown
                        {
                            if (needChangeValueBox || valueNumericUpDown == null)
                            {
                                valueNumericUpDown = new NumericUpDown();
                                valueNumericUpDown.Maximum = (decimal)int.MaxValue;
                                valueNumericUpDown.Minimum = (decimal)int.MinValue;
                                valueNumericUpDown.Increment = (decimal)0.5;
                                valueNumericUpDown.DecimalPlaces = 3;
                                valueNumericUpDown.Text = value;
                                valueNumericUpDown.Location = valueLocation;
                                valueNumericUpDown.TextChanged += valueNumericUpDown_TextChanged;
                                valueNumericUpDown.Leave += new System.EventHandler(this.focus_Leave);
                                valueNumericUpDown.TabIndex = 4;
                                //valueNumericUpDown.LostFocus += buttonSet_Click;
                                valueNumericUpDown.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
                                this.Controls.Add(valueNumericUpDown);
                            }
                            else {
                                valueNumericUpDown.Text = value;
                            }
                        }
                        break;
                    case "image":
                    case "Image":
                    case "mat":
                    case "Mat":
                    case "json":
                    case "Json": //20170216 新增 type: json
                    case "object":
                    case "Object": //20170612 新增 type: object
                        //none

                        break;
                    case "int":
                    case "Int":
                        //numericUpDown
                        {
                            if (needChangeValueBox || valueNumericUpDown == null)
                            {
                                valueNumericUpDown = new NumericUpDown();
                                valueNumericUpDown.Maximum = (decimal)int.MaxValue;
                                valueNumericUpDown.Minimum = (decimal)int.MinValue;
                                valueNumericUpDown.Increment = (decimal)1;
                                valueNumericUpDown.Text = value;
                                valueNumericUpDown.Location = valueLocation;
                                valueNumericUpDown.TextChanged += valueNumericUpDown_TextChanged;
                                valueNumericUpDown.Leave += new System.EventHandler(this.focus_Leave);
                                valueNumericUpDown.TabIndex = 4;
                                // valueNumericUpDown.LostFocus += buttonSet_Click;
                                valueNumericUpDown.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
                                this.Controls.Add(valueNumericUpDown);
                            }
                            else {
                                valueNumericUpDown.Text = value;
                            }
                        }
                        break;
                    case "string":
                    case "String":
                    case "str":
                    case "Str":
                        //textBox
                        {
                            if (needChangeValueBox || valueTextBox == null)
                            {
                                valueTextBox = new TextBox();
                                valueTextBox.Text = value;
                                valueTextBox.Location = valueLocation;
                                valueTextBox.Width = textBoxName.Width; // 20170316_ 2. Value 的文字框寬度拉到跟 Name 的文字框相同。
                                valueTextBox.TextChanged += valueTextBox_TextChanged;
                                valueTextBox.Leave += new System.EventHandler(this.focus_Leave);
                                valueTextBox.TabIndex = 4;
                                //valueTextBox.Multiline = true;
                                valueTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
                                // valueTextBox.LostFocus += buttonSet_Click;
                                this.Controls.Add(valueTextBox);
                            }
                            else {
                                valueTextBox.Text = value;
                            }
                        }
                        break;
                }
                str.Clear();
            }
            isShowVariableRunning = false;
            return;
        }

        private void valueComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ComboBox valueComboBox = (ComboBox)sender;
            value = valueComboBox.SelectedIndex.ToString();
            return;
        }
        private void valueNumericUpDown_TextChanged(object sender, EventArgs e)
        {
            //DomainUpDown valueNumericUpDown = (DomainUpDown)sender;
            value = valueNumericUpDown.Text;
            return;
        }

        private void valueTextBox_TextChanged(object sender, EventArgs e)
        {
            //TextBox valueTextBox = (TextBox)sender;
            value = valueTextBox.Text;
            return;
        }

        //20170119 leo: 討論後 unfocus 時自動觸發 set 功能
        //unfocus 定義為 [前一次選取的 variable 與目前選取的 variable 之 sn 不相同]
        //於 showVariable 觸發時檢查。
        //其餘規則：存檔，讀檔時的額外處理：
        //1. 存檔時，最後一筆 variable 就算沒有 unfocus 也應該被儲存 -> 也就是存檔時觸發 setVariable()
        //2. 讀檔時，要把 focus 清除 -> 也就是讀檔時把 variableSn 設為 -1
        //20170120 leo 關於名稱重複：判斷有重複時，就把名稱後面加上編號，然後重新檢查加上該編號的名稱是否與其他的有重複，如果有，編號加一

        //20170202 leo 關於[修改 Variable Name 之後，其他地方不會連動]之修正：
        //之前設置為[點選其他 Variable 後]才會把值存入。改為 Form Variable 的任何一個物件被 unfocus 時，即更動 variable 
        public void setVariable() {
            // System.Console.WriteLine("FormVariable.cs SetVariable");
            /*
 showVariable variableSn 2, 1
setVariable variableSn 1
Variable_SameName = Variable1at1and1
isSameName = False
FormVariable setVariable OK
showVariable variableSn 1, 1
typeComboBox_SelectedIndexChanged
editValue variableSn 2
showVariable variableSn 2, 1
setVariable variableSn 1 
             */
            //System.Console.WriteLine("setVariable variableSn " + this.variableSn);
            if (variableSn != -1 && variableSn != 0 && FormMain.r8.variables[variableSn] != null)
            {
                //20170120 leo setVariable 時檢查是否重複名稱
                Variable[] variables = FormMain.r8.getVariableArray();
                int i;
                bool isSameName = true;
                String tempString = this.textBoxName.Text.ToString();
                int startNumber = 1;
                int nameNumberCount = startNumber;
                while (isSameName)
                {
                    isSameName = false;
                    if (nameNumberCount == startNumber)
                    {
                        tempString = this.textBoxName.Text.ToString();
                    }
                    else
                    {
                        //tempString = this.textBoxName.Text.ToString() + "_" + nameNumberCount;
                        //tempString = this.textBoxName.Text.ToString() + "" + nameNumberCount;//20170602 討論後暫時改為不加底線
                        // 20171016 變數名稱不能重複，輸入重複的變數後，後方自動加上 _2 _3…所以改回加底線的版本
                        tempString = this.textBoxName.Text.ToString() + "_" + nameNumberCount;
                    }
                    for (i = 0; i < variables.Count(); i++)
                    {
                        if (variables[i].name.Equals(tempString))
                        {
                            //System.Console.WriteLine("Variable_SameName = " + tempString + "at" + variables[i].sn + "and" + FormMain.r8.variables[variableSn].sn);
                            if (variables[i].sn != FormMain.r8.variables[variableSn].sn)
                            {
                                isSameName = true;
                                break;
                            }
                        }
                    }
                   // System.Console.WriteLine("isSameName = " + isSameName);
                    if (isSameName)
                    {
                        nameNumberCount++;
                    }
                }


                //20170214 leo: 需要增加檢查條件：若 variable 各欄位都沒被改就直接 return;

                if (FormMain.r8.variables[variableSn].value.Equals(value) &&
                    FormMain.r8.variables[variableSn].name.Equals(tempString) &&
                    FormMain.r8.variables[variableSn].type == this.comboBox1.SelectedIndex &&
                    FormMain.r8.variables[variableSn].remark.Equals(this.textBoxRemark.Text.ToString()))
                {
                    //System.Console.WriteLine("FormVariable setVariable: No Change->return");
                    //System.Console.WriteLine("value = " + FormMain.r8.variables[variableSn].value + "  " + value);
                    //System.Console.WriteLine("name = " + FormMain.r8.variables[variableSn].name + "  " + tempString);
                    //System.Console.WriteLine("type = " + FormMain.r8.variables[variableSn].type + "  " + this.comboBox1.SelectedIndex);
                    //System.Console.WriteLine("remark = " + FormMain.r8.variables[variableSn].remark + "  " + this.textBoxRemark.Text.ToString());
                    return;
                }
                else
                {
                    // System.Console.WriteLine("FormVariable setVariable: Has Change-> Set");
                }

                FormMain.r8.variables[variableSn].value = value;
                FormMain.r8.variables[variableSn].name = tempString;
                FormMain.r8.variables[variableSn].type = this.comboBox1.SelectedIndex;
                FormMain.r8.variables[variableSn].remark = this.textBoxRemark.Text.ToString();
                // System.Console.WriteLine("FormVariable setVariable OK");
                formMain.formVariables.showVariables(formMain.formVariables.function);
                formMain.formVariable.showVariable(variableSn);
                //formMain.formFunction.resetFunction();//20170124 leo: 這個應該要只重設有關聯的物件，而不是整個畫面重設 //20170214 連這個也無窮迴圈了?
                formMain.formFunction.resetPanels(null);//...但換這個會無窮迴圈//20170214 把這個修復了
                Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                //Program.recordProgram(FormMain.r8);
            }
            
            return;
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            setVariable();
            return;
        }

        public void buttonDelete_Click(object sender, EventArgs e)
        {
            if (variableSn != -1 && variableSn != 0)
            {
                FormMain.r8.variables[variableSn] = null;
                /*
                formMain.formVariables.showVariables(formMain.formFunction.function);
                formMain.formFunctions.resetButton();
                resetFunction();
                */

                variableSn = -1;
                formMain.formVariables.showVariables(formMain.formFunction.function);
                formMain.formFunctions.resetButton();
                formMain.formFunction.resetFunction();
                //setVariable();
                showVariable(variableSn);
                Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                //Program.recordProgram(FormMain.r8);
            }
            
            return;
        }

        private void buttonDelete_MouseLeave(object sender, EventArgs e)
        {

        }

        private void focus_Leave(object sender, EventArgs e)
        {
            setVariable();
            return;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                setVariable();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void FormVariable_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formVariable = null;
        }

       
    }
}
