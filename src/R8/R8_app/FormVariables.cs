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
    public partial class FormVariables : Form
    {
        public List<Button> buttons = new List<Button>();
        //private int buttonsLocationY = 0;
        private FormMain formMain;
        public Function function;
        //bool isShowTargetVariable = true;
        public bool isShowTargetVariable = false;//20170316_ 1. Open program 之後展開所有的 variables 。
        public bool isInSearchMode = false;//20170317_搜尋功能
        List<Variable> targetVariableList = new List<Variable>();


        //20170817 leo: 發現有時候在 FormFunctions 拉 function 順序時，會話畫面捲動導致 function 被誤拖曳，
        //原因為 Form 在 onFocus 時會自動捲動到 [該 Form 中，前一個被選取物件] 的位置。
        //參照 https://stackoverflow.com/questions/419774/how-can-you-stop-a-winforms-panel-from-scrolling/912610#912610
        //用 override 把該功能關掉
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }


        private R8.PanelNoScrollOnFocus panel2;

        public FormVariables()
        {
            panel2 = new R8.PanelNoScrollOnFocus();
            this.panel2.AutoScroll = true;
            this.panel2.Location = new System.Drawing.Point(0, 50);
            this.panel2.Name = "panel2";
            this.panel2.TabIndex = 1;
            this.Controls.Add(this.panel2);
            InitializeComponent();
        }

        private void FormVariable_Load(object sender, EventArgs e)
        {
            formMain = (FormMain)this.MdiParent;
            FormVariables_SizeChanged(sender, e);
        }



        //20170309 leo: 討論後，要變成能切換[所有variable]或[目標function的variable]
        //20170317 leo: 討論後，加入搜尋功能， 然後 variable n of n 改為顯示在 title
        //search bar 繪製方式參照 http://stackoverflow.com/questions/25201011/windows-form-how-to-add-an-icon-to-the-right-or-left-of-a-text-box

        public void showVariables(Function function)
        {

            //System.Console.WriteLine("showVariables_IN_FormVariables");
            this.function = function;
            Button button;
            //20170208 leo: add: 畫面有 VerticalScroll 時，紀錄 Scroll 位置，避免畫面跑掉。
            int preScrollValue = this.panel2.VerticalScroll.Value;
            //clearButton();
            //讓 button 數量與 variable 數量相同
            int targetVariableCount = 0;
            Variable variable;
            int i = 0;
            int buttonIndex = 0;
            targetVariableList.Clear();


            if (isShowTargetVariable)
            {
                if (function != null)
                {
                    //textBoxSearch.Text = "i: " + function.name + " " + function.sn;
                    textBoxSearch.Text = "i: " + function.name;
                }
            }


            if (isShowTargetVariable)
            {
                if (function == null)
                {
                    targetVariableCount = 0;
                }
                else
                {
                    foreach (Function.Parameter parameter in function.parameters)
                    {
                        if (parameter.variableSn != -1 && parameter.variableSn != 0)
                        {
                            variable = FormMain.r8.variables[parameter.variableSn];
                            if (variable != null)
                            {
                                targetVariableCount++;
                            }

                        }
                    }
                }

            }
            else if (isInSearchMode)
            {
                //20170317 add search mode
                for (i = 0; i <= FormMain.r8.getVariableSnMax(); i++)
                {
                    variable = FormMain.r8.variables[i];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.sn == -1 || variable.sn == 0)
                    {
                        continue;
                    }
                    if (variable.ToString().ToLower().Contains(textBoxSearch.Text.ToLower()))
                    {
                        targetVariableList.Add(variable);
                    }
                }
                targetVariableCount = targetVariableList.Count;
            }
            else
            {

                targetVariableCount = FormMain.r8.getVariablesCount();

            }
            //System.Console.WriteLine("targetVariableCount = " + targetVariableCount + " buttons.Count = " + buttons.Count);
            while (buttons.Count < targetVariableCount)
            {
                addButton("", -1);
            }

            while (buttons.Count > targetVariableCount)
            {
                if (buttons.Count > 0)
                {
                    button = buttons.ElementAt(buttons.Count - 1);
                    button.Dispose();
                    buttons.Remove(button);
                    //buttonsLocationY = buttonsLocationY - button.Height - 5;
                }
                else
                {
                    break;
                }
            }
            //System.Console.WriteLine("after: targetVariableCount = " + targetVariableCount + " buttons.Count = " + buttons.Count);

            //20170531 Leo: 必須把 Variable 改為依照字母排序。原程式碼整段註解掉備用
            if (isShowTargetVariable)
            {
                if (function == null)
                {

                }
                else
                {
                    List<Variable> tempSortVariableList = new List<Variable>();
                    for (i = 0; i < function.parameters.Count; i++)
                    {
                        if (function.parameters.ElementAt(i).variableSn != -1 && function.parameters.ElementAt(i).variableSn != 0)
                        {
                            variable = FormMain.r8.variables[function.parameters.ElementAt(i).variableSn];
                            //tempSortVariableList.Add(variable);
                            if (variable != null)
                            {
                                tempSortVariableList.Add(variable);
                            }
                        }
                    }
                    tempSortVariableList.Sort((x, y) => { return x.name.CompareTo(y.name); });


                    for (i = 0; i < tempSortVariableList.Count; i++)
                    {

                        variable = tempSortVariableList.ElementAt(i);
                        if (variable != null)
                        {
                            button = buttons.ElementAt(buttonIndex);
                            //button.Text = variable.ToString();
                            button.Text = getVariableShowText(variable);
                            button.Tag = variable.sn;
                            buttonIndex++;
                        }

                    }

                }
            }
            else if (isInSearchMode)
            {
                targetVariableList.Sort((x, y) => { return x.name.CompareTo(y.name); });
                //20170317 add search mode
                for (i = 0; i < targetVariableList.Count; i++)
                {
                    variable = targetVariableList.ElementAt(i);
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.sn == -1 || variable.sn == 0)
                    {
                        continue;
                    }
                    button = buttons.ElementAt(buttonIndex);
                    //button.Text = variable.ToString();
                    button.Text = getVariableShowText(variable);
                    button.Tag = variable.sn;
                    buttonIndex++;
                }
                targetVariableCount = targetVariableList.Count();
            }
            else
            {

                List<Variable> tempSortVariableList = new List<Variable>();
                for (i = 0; i <= FormMain.r8.getVariableSnMax(); i++)
                {
                    variable = FormMain.r8.variables[i];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.sn == -1 || variable.sn == 0)
                    {
                        continue;
                    }
                    tempSortVariableList.Add(variable);
                }
                    
                tempSortVariableList.Sort((x, y) => { return x.name.CompareTo(y.name); });

                for (i = 0; i < tempSortVariableList.Count; i++)
                {

                    variable = tempSortVariableList.ElementAt(i);
                    if (variable != null)
                    {
                        button = buttons.ElementAt(buttonIndex);
                        button.Text = getVariableShowText(variable);
                        button.Tag = variable.sn;
                        buttonIndex++;
                    }

                }
                
            }

            /*
            if (isShowTargetVariable)
            {
                if (function == null)
                {

                }
                else
                {
                    foreach (Function.Parameter parameter in function.parameters)
                    {
                        if (parameter.variableSn != -1)
                        {
                            variable = FormMain.r8.variables[parameter.variableSn];
                            if (variable != null)
                            {
                                button = buttons.ElementAt(buttonIndex);
                                //button.Text = variable.ToString();
                                button.Text = getVariableShowText(variable);
                                button.Tag = variable.sn;
                                buttonIndex++;
                            }
                        }
                        else
                        {

                        }

                    }
                }
            }
            else if (isInSearchMode)
            {
                //20170317 add search mode
                for (i = 0; i < targetVariableList.Count; i++)
                {
                    variable = targetVariableList.ElementAt(i);
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.sn == -1)
                    {
                        continue;
                    }
                    button = buttons.ElementAt(buttonIndex);
                    //button.Text = variable.ToString();
                    button.Text = getVariableShowText(variable);
                    button.Tag = variable.sn;
                    buttonIndex++;
                }
                targetVariableCount = targetVariableList.Count();
            }
            else
            {
                for (i = 0; i <= FormMain.r8.getVariableSnMax(); i++)
                {
                    variable = FormMain.r8.variables[i];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.sn == -1)
                    {
                        continue;
                    }
                    button = buttons.ElementAt(buttonIndex);
                    //button.Text = variable.ToString();
                    button.Text = getVariableShowText(variable);
                    button.Tag = variable.sn;
                    buttonIndex++;
                }
            }
            */


            if (preScrollValue > this.panel2.VerticalScroll.Maximum)
            {
                preScrollValue = this.panel2.VerticalScroll.Maximum;
            }
            else if (preScrollValue < this.panel2.VerticalScroll.Minimum)
            {
                preScrollValue = this.panel2.VerticalScroll.Minimum;
            }
            this.panel2.VerticalScroll.Value = preScrollValue;
            this.PerformLayout();
            //buttonShowAll.Text = "" + targetVariableCount + " of " + FormMain.r8.getVariablesCount() + " variables";
            //this.Text = "Variables " + targetVariableCount + " of " + FormMain.r8.getVariablesCount();
            //20170327 視窗 Variables 改 title 為 Variables。
            this.Text = "Program Variables  " + targetVariableCount + " of " + FormMain.r8.getVariablesCount();
            return;
        }

        //20170215 leo: 改為不 clear 既有的 button 。
        public void showVariables_before_0309(Function function)
        {
            //System.Console.WriteLine("showVariables_IN_FormVariables");
            this.function = function;
            Button button;
            //20170208 leo: add: 畫面有 VerticalScroll 時，紀錄 Scroll 位置，避免畫面跑掉。
            int preScrollValue = this.panel2.VerticalScroll.Value;
            //clearButton();
            //讓 button 數量與 variable 數量相同
            while (buttons.Count < FormMain.r8.getVariablesCount())
            {
                addButton("", -1);
            }

            while (buttons.Count > FormMain.r8.getVariablesCount())
            {
                if (buttons.Count > 0)
                {
                    button = buttons.ElementAt(buttons.Count - 1);
                    button.Dispose();
                    buttons.Remove(button);
                    //buttonsLocationY = buttonsLocationY - button.Height - 5;
                }
                else
                {
                    break;
                }
            }
            Variable variable;
            int i = 0;
            int buttonIndex = 0;
            for (i = 0; i <= FormMain.r8.getVariableSnMax(); i++)
            {
                variable = FormMain.r8.variables[i];
                if (variable == null)
                {
                    continue;
                }
                if (variable.sn == -1 || variable.sn == 0)
                {
                    continue;
                }
                button = buttons.ElementAt(buttonIndex);
                //button.Text = variable.ToString();
                button.Text = getVariableShowText(variable);
                button.Tag = variable.sn;
                buttonIndex++;
            }
            if (preScrollValue > this.panel2.VerticalScroll.Maximum)
            {
                preScrollValue = this.panel2.VerticalScroll.Maximum;
            }
            else if (preScrollValue < this.panel2.VerticalScroll.Minimum)
            {
                preScrollValue = this.panel2.VerticalScroll.Minimum;
            }
            this.panel2.VerticalScroll.Value = preScrollValue;
            this.PerformLayout();
            return;
        }

        //舊版暫時保留
        public void showVariables_back(Function function)
        {
            //System.Console.WriteLine("showVariables_IN_FormVariables");
            this.function = function;

            //20170208 leo: add: 畫面有 VerticalScroll 時，紀錄 Scroll 位置，避免畫面跑掉。
            int preScrollValue = this.panel2.VerticalScroll.Value;

            clearButton();

            Variable variable;
            int i = 0;
            //20170309 leo: 討論後，變成模式切換

            //20170125 討論後改為 show 所有 variable
            if (isShowTargetVariable)
            {
                if (function == null)
                {
                    return;
                }

                foreach (Function.Parameter parameter in function.parameters)
                {
                    if (parameter.variableSn != -1 && parameter.variableSn != 0)
                    {
                        variable = FormMain.r8.variables[parameter.variableSn];
                        // addButton("[" + variable.sn + "][" + variable.name + "][" + variable.value + "]", variable.sn);
                        addButton(getVariableShowText(variable), variable.sn);
                    }
                    else
                    {

                    }

                }
            }
            else
            {
                //20170125 leo: 討論後改為 show 所有 variable
                for (i = 0; i <= FormMain.r8.getVariableSnMax(); i++)
                {
                    variable = FormMain.r8.variables[i];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.sn == -1 || variable.sn == 0)
                    {
                        continue;
                    }
                    //addButton("[" + variable.sn + "][" + variable.name + "][" + variable.value + "]", variable.sn);
                    addButton(getVariableShowText(variable), variable.sn);
                }
            }
            if (preScrollValue > this.panel2.VerticalScroll.Maximum)
            {
                preScrollValue = this.panel2.VerticalScroll.Maximum;
            }
            else if (preScrollValue < this.panel2.VerticalScroll.Minimum)
            {
                preScrollValue = this.panel2.VerticalScroll.Minimum;
            }
            this.panel2.VerticalScroll.Value = preScrollValue;
            this.PerformLayout();
            return;
        }

        public void addButton(string text, int sn)
        {
            Button button = new Button();
            button.Name = text;
            button.Text = text;
            button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.Width = this.Width - 35;
            button.Height = 40;
            int upPadding = 0;
            if (buttons.Count == 0)
            {
                button.Location = new Point(0, 0 + upPadding);
            }
            else
            {
                button.Location = new Point(0, buttons.ElementAt(buttons.Count - 1).Location.Y + buttons.ElementAt(buttons.Count - 1).Height + 5);
            }


            //buttonsLocationY += button.Height + 5;
            button.Click += new System.EventHandler(this.variableButton_Click);
            button.Tag = sn;
            //button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttons_MouseDown);
            panel2.Controls.Add(button);
            buttons.Add(button);


            //20171026 項目[按下 del 或 backspace 可以刪除 function 或是 variable 。]
            button.PreviewKeyDown += new PreviewKeyDownEventHandler(button_PreviewKeyDown);
            button.KeyDown += new KeyEventHandler(formMain.button_From_delete_KeyDown);
            return;
        }

        public void clearButton()
        {

            //清空畫面
            foreach (Button b in buttons)
            {
                b.Dispose();
            }
            buttons.Clear();
            //buttonsLocationY = 0;
            return;
        }

        public void variableButton_Click(object sender, EventArgs e)
        {

            Button button = (sender as Button);
            //R8.logToLogBox("functionButton_Click: " + button.Name);
            formMain.formVariable.showVariable((int)(button.Tag));

            //20170327 leo: 依今天信件增加 [選到 variable 時，自動 focus 到 Value] 功能。
            //20171026 leo:此功能與項目 [按下 del 或 backspace 可以刪除 function 或是 variable 。]衝突了，取消(會導致根本 focus 不到 variableButton 上面，所以 delete 失效)
            //formMain.formVariable.focusToValueBox();
            return;
        }

        private void buttonShowAll_Click(object sender, EventArgs e)
        {
            isShowTargetVariable = !isShowTargetVariable;
            showVariables(function);
            return;
        }

        private void pictureBoxSearch_Click(object sender, EventArgs e)
        {
            isInSearchMode = true;
            showVariables(function);
            isShowTargetVariable = false;
            formMain.formFunction.resetPanels(textBoxSearch.Text.ToLower());
            return;
        }
        public void clearSearchBox()
        {
            if (isInSearchMode || isShowTargetVariable)
            {
                textBoxSearch.Text = "";
                isInSearchMode = false;
                isShowTargetVariable = false;
                //showVariables(function);
                formMain.formFunction.resetPanels(textBoxSearch.Text.ToLower());
            }
        }

        private void pictureBoxCancel_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = "";
            isInSearchMode = false;
            isShowTargetVariable = false;
            showVariables(function);
            formMain.formFunction.resetPanels(textBoxSearch.Text.ToLower());
            return;
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                isInSearchMode = true;
                isShowTargetVariable = false;
                showVariables(function);
                formMain.formFunction.resetPanels(textBoxSearch.Text.ToLower());
            }
            return;
        }


        //20170327 Functions Variables 方塊不顯示 SN
        // Variables 的名稱顯示分散太多地方，把它都整合到這邊
        private string getVariableShowText(Variable variable)
        {
            string returnStr = "";
            if (variable.sn != -1 && variable.sn != 0)
            {
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, variable.type);
                switch (str.ToString())
                {
                    case "image":
                    case "Image":
                    case "mat":
                    case "Mat":
                    case "json":
                    case "Json":
                    case "object":
                    case "Object":
                        returnStr = variable.name;
                        break;
                    default:
                        if (variable.value == null || variable.value.Length == 0 || variable.value.Equals(""))
                        {
                            returnStr = variable.name;
                        }
                        else
                        {
                            returnStr = variable.name + " (" + variable.value + ")";
                        }
                        break;
                }
                str.Clear();
            }
            if (variable.remark.Length > 0)
            {
                returnStr += " (" + variable.remark + ")";
            }
            return returnStr;
        }

        private void FormVariables_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formVariables = null;
        }

        private void FormVariables_SizeChanged(object sender, EventArgs e)
        {
            panel2.Height = this.ClientSize.Height - panel2.Location.Y;
            panel2.Width = this.ClientSize.Width;
        }

        //https://msdn.microsoft.com/zh-tw/library/system.windows.forms.control.previewkeydown(v=vs.110).aspx
        void button_KeyDown(object sender, KeyEventArgs e)
        {
           // return;
            //MessageBox.Show("button_KeyDown " + e.KeyValue + e.KeyCode);

            //20171103 現在要連續刪除，需要額外判定 Form 有沒有被 focus
            //MessageBox.Show("formMain.ActiveMdiChild: " + formMain.ActiveMdiChild.Text);

            //  MessageBox.Show("formMain ActiveControl = " + formMain.ActiveControl);
            //MessageBox.Show("formMain ActiveMdiChild = " + formMain.ActiveMdiChild);
           // MessageBox.Show("ActiveForm = " + Form.ActiveForm);
            //ActiveForm
            //MessageBox.Show("formMain ContainsFocus = " + formMain.ContainsFocus);
            if (formMain.ActiveMdiChild.GetType() == this.GetType())
            {
                if (e.KeyValue == 8 || e.KeyValue == 46)
                {
                    if (formMain.formVariable != null)
                    {
                        formMain.formVariable.buttonDelete_Click(sender, null);
                        //20171103 項目[Program Functions 跟 Program Variables 無法連續按 Backspace 或 Del 連續進行刪除]
                        //所以要[刪除掉後，強制選擇下一個 Function 或 Variable]
                        Control con;
                        con = this.ActiveControl;
                        if (con != null)
                        {
                            //MessageBox.Show("V_focus: " + con.Name + ", " + con.Text + ", MDI = " + formMain.ActiveMdiChild.GetType());
                            if (con.GetType() == typeof(Button))
                            {
                                if (buttons.Contains(con))
                                {
                                    variableButton_Click(con, EventArgs.Empty);
                                    formMain.formVariables.Focus();
                                }
                            }
                        }
                    }
                }
            }
            else
            {

             //   MessageBox.Show("GetType: " + formMain.ActiveMdiChild.GetType() + " != " + this.GetType());

            }
        }

        private void button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }
    }
}
