using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace R8
{
    public partial class FormFunctions : Form
    {
        private int dragX = 0;
        private int dragY = 0;
        private string dragData = "";
        public List<Button> buttons = new List<Button>();
        private const int buttonStartY = 0;
        private int buttonsLocationY = buttonStartY;
        private int buttonsLocationGap = 15;

        private FormMain formMain;
        private int moveFrom = 0;
        private int moveTo = 0;

        //20170215 leo: 準備修改[新增移除 function 時，整個畫面會閃一下]的現象。
        //也就是 reset 時，不能把物件都 Dispose 掉再重建。
        private int lastFocusButtonIndex = -1;
        private int lastFocusButtonSn = -1;

        private bool isInSearchMode = false;//20170317_搜尋功能

        private List<Label> lineLabels = new List<Label>();


        //20170817 leo: 發現有時候在 FormFunctions 拉 function 順序時，會話畫面捲動導致 function 被誤拖曳，
        //原因為 Form 在 onFocus 時會自動捲動到 [該 Form 中，前一個被選取物件] 的位置。
        //參照 https://stackoverflow.com/questions/419774/how-can-you-stop-a-winforms-panel-from-scrolling/912610#912610
        //用 override 把該功能關掉
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }

        private R8.PanelNoScrollOnFocus panel2;
        

        public FormFunctions()
        {
            panel2 = new R8.PanelNoScrollOnFocus();
            this.panel2.AutoScroll = true;
            this.panel2.Location = new System.Drawing.Point(0, 50);
            this.panel2.Name = "panel2";
            this.panel2.TabIndex = 1;
            this.Controls.Add(this.panel2);
            InitializeComponent();
        }
        private void FormFunctions_Load(object sender, EventArgs e)
        {
            formMain = (FormMain)this.MdiParent;
            //20170117 leo: 討論後移除 start
            /*
            Function function = new Function("start", 0, 0);
            formMain.r8.addFunction(function);
            addVariableSn(function);
            addButton("start", 0);
            */
            this.Text = "Program Functions  0 of 0";

            FormFunctions_SizeChanged(sender, e);
            return;
        }

        private void FormFunctions_DragEnter(object sender, DragEventArgs e)
        {
            try
            {
                //R8.Log("FormFunctions_DragEnter");
                dragX = e.X;
                dragY = e.Y;
                dragData = e.Data.GetData(DataFormats.Text).ToString();
                e.Effect = DragDropEffects.All;
            } catch (Exception ex) {
                ex.ToString();  // Prevent warning.
            }
            return;
        }
        
        private void FormFunctions_DragOver(object sender, DragEventArgs e)
        {
            return;
            /*
            //R8.Log("FormFunctions_DragOver");
            dragX = e.X;
            dragY = e.Y;
            dragData = e.Variable.GetVariable(DataFormats.Text).ToString();
            return;
            */
        }

        public void addButtonAtLast(string buttonName) {
            if (isInSearchMode) {// SearchMode 禁止移動 Function
                return;
            }

            Function function = new Function(buttonName, 0, 0);
            FormMain.r8.addFunction(function);
            //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
            //Button button = addButton("[" + function.sn + "] [" + buttonName + "]", function.sn, function.enable);
            Button button;
            /*
            if (function.remark.Length > 0)
            {
                button = addButton(function.name + " " + function.sn + " " + "[" + function.remark + "]", function.sn, function.enable);
            }
            else
            {
                button = addButton(function.name + " " + function.sn + " ", function.sn, function.enable);
            }*/
            //20170309 leo end
            button = addButton(getFunctionShowText(function), function.sn, function.enable);
            button.Select();
            functionButton_Click(button, EventArgs.Empty);
            function.posY = buttons.Count;//posY 的預設值會是 button總數 (因為此時button位置還沒搬家)
            return;
        }

        public void copyButtonToLast(Function srcFunction)
        {
            if (isInSearchMode)
            {// SearchMode 禁止移動 Function
                return;
            }

            Function function = new Function(srcFunction.name, 0, 0);
            if (srcFunction.remark != null)
            {
                function.remark = srcFunction.remark.ToString();
            }
            for (int i = 0; i < function.parameters.Count(); i++)
            {
                function.parameters.ElementAt(i).variableSn = srcFunction.parameters.ElementAt(i).variableSn;
                function.parameters.ElementAt(i).type = srcFunction.parameters.ElementAt(i).type;
                function.parameters.ElementAt(i).name = srcFunction.parameters.ElementAt(i).name.ToString();
            }
            FormMain.r8.addFunction(function);
            //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
            //Button button = addButton("[" + function.sn + "] [" + srcFunction.name + "]", function.sn, function.enable);
            Button button;
            /*
            if (function.remark.Length > 0)
            {
                button = addButton(function.name + " " + function.sn + " " + "[" + function.remark + "]", function.sn, function.enable);
            }
            else
            {
                button = addButton(function.name + " " + function.sn + " ", function.sn, function.enable);
            }*/
            //20170309 leo end
            button = addButton(getFunctionShowText(function), function.sn, function.enable);
            //button.Select();
            //functionButton_Click(button, EventArgs.Empty);
            function.posY = buttons.Count;//posY 的預設值會是 button總數 (因為此時button位置還沒搬家)
            return;
        }

        int labelWidth = 25;
        private Button addButton(string text, int sn, bool isEnable)
        {
            Button button = new Button();
            // button.Name = text;
            //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
            if (isEnable)
            {
                button.Text = text;
                button.ForeColor = Color.Black;
            }
            else {
                //button.Text = text + "[disabled]";
                button.Text = text;
                button.ForeColor = Color.Gray;
            }
            button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.Width = this.Width - 35 - labelWidth;
            button.Height = 40;
            //MessageBox.Show("VerticalScroll = " + this.panel2.VerticalScroll.Value);
            //AutoScrollPosition
            //button.Location = new Point(0 + labelWidth, buttonsLocationY - this.panel2.VerticalScroll.Value);
            //20171025 leo: 因應項目[搜尋 bar 不要跟著滑動]增加 panel 後原本的位置計算方法會出問題，
            //參照 https://stackoverflow.com/questions/35393610/dynamic-buttons-on-panel1-locations
            //改為以下方式
            button.Location = new Point(0 + labelWidth, buttonsLocationY + this.panel2.AutoScrollPosition.Y);
            buttonsLocationY += button.Height + buttonsLocationGap;
            button.Click += new System.EventHandler(this.functionButton_Click);
            button.Tag = sn;
            button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttons_MouseDown);
            button.Leave += new System.EventHandler(this.button_FocusLeave);
            button.Enter += new System.EventHandler(this.button_FocusIn);
            //this.Controls.Add(button);
            panel2.Controls.Add(button);
            buttons.Add(button);

            //20171026 項目[按下 del 或 backspace 可以刪除 function 或是 variable 。]
            button.PreviewKeyDown += new PreviewKeyDownEventHandler(button_PreviewKeyDown);
            button.KeyDown += new KeyEventHandler(formMain.button_From_delete_KeyDown);


            //20171025 leo: 項目[Function block 前面顯示行號]
            Label label = new Label();
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label.Width = labelWidth;
            label.Height = 40;
            label.Location = new Point(0, button.Location.Y);
            label.Text = "" + (lineLabels.Count + 1);
            lineLabels.Add(label);
            //this.Controls.Add(label);
            panel2.Controls.Add(label);

            

            return button;
        }
        private void button_FocusIn(object sender, EventArgs e)
        {
            //System.Console.WriteLine("button_FocusIn " + (sender as Button).Tag + ", " + buttons.IndexOf(sender as Button));
            lastFocusButtonIndex = buttons.IndexOf(sender as Button);
            lastFocusButtonSn = (int)((sender as Button).Tag);
            return;
        }

        private void button_FocusLeave(object sender, EventArgs e)
        {
            //System.Console.WriteLine("button_FocusLeave " + (sender as Button).Tag + ", " + buttons.IndexOf(sender as Button));
            return;
        }

        private void FormFunctions_DragDrop(object sender, DragEventArgs e)
        {
            if (isInSearchMode)
            {// SearchMode 禁止移動 Function
                switch (e.AllowedEffect)
                {
                    case DragDropEffects.Move:
                        functionButton_Click(buttons.ElementAt(moveFrom), e);
                        break;
                }
                    return;
            }


            //R8.logToLogBox("FormFunctions_DragDrop: " + e.X + "," + e.Y + e.AllowedEffect);
            Point point;
            Button button;
            int i;
            switch (e.AllowedEffect) {

                case DragDropEffects.Copy:
                    //20170125 leo 討論後新增 function button 要放到指定位置.... 動作為增加 button 後 moveTo 
                    point = PointToClient(Cursor.Position);
                    point.Y -= panel2.Location.Y;//20171025 項目 [搜尋 bar 不要跟著滑動]導致介面變了，這邊會增加位移量
                    Function function = new Function(dragData, 0, 0);
                    //addVariableSn(function);
                    FormMain.r8.addFunction(function);
                    //20170117 leo: 新增按鈕時，預設 click 它，按鈕名稱加上 sn 
                    //button = addButton("[" + function.sn + "] [" + e.Variable.GetVariable(DataFormats.Text).ToString() + "]", function.sn, function.enable);

                    //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
                    /*
                    if (function.remark.Length > 0)
                    {
                        button = addButton(function.name + " " + function.sn + " " + "[" + function.remark + "]", function.sn, function.enable);
                    }
                    else
                    {
                        button = addButton(function.name + " " + function.sn + " ", function.sn, function.enable);
                    }*/
                    //20170309 leo end
                    button = addButton(getFunctionShowText(function), function.sn, function.enable);
                    function.posY = buttons.Count;//posY 的預設值會是 button總數 (因為此時button位置還沒搬家)
                    
                    for (i = 0; i < buttons.Count; i++)
                    {
                        if (buttons.ElementAt(i).Location.Y > point.Y)
                        {
                            moveTo = i;
                            break;
                        }
                        if (i == buttons.Count - 1)
                        {
                            moveTo = i + 1; //移到最後一格要另外控制
                        }
                    }
                    //System.Console.WriteLine("moveSetTo " + moveTo);
                    if (buttons.Count > 0)
                    {
                        //Function tempFunction = FormMain.r8.getFunction(function.sn);
                        if (moveTo < buttons.Count)
                        {
                            FormMain.r8.functions[function.sn].posY = (float)(FormMain.r8.functions[((int)(buttons.ElementAt(moveTo).Tag))].posY - 0.5f);
                        }
                        else
                        {
                            FormMain.r8.functions[function.sn].posY = (float)(FormMain.r8.functions[((int)(buttons.ElementAt(buttons.Count - 1).Tag))].posY + 0.5f);
                        }
                    }
                    Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                    //Program.recordProgram(FormMain.r8);
                    button.Select();
                    functionButton_Click(button, EventArgs.Empty);

                    resetButton();
                    foreach(Button b in buttons) {
                        if ((int)(b.Tag) == function.sn) {
                            b.Select();
                            break;
                        }
                    }                    
                    break;
                case DragDropEffects.Move:
                    point = PointToClient(Cursor.Position);
                    point.Y -= panel2.Location.Y;//20171025 項目 [搜尋 bar 不要跟著滑動]導致介面變了，這邊會增加位移量
                    for (i = 0; i < buttons.Count; i++) {
                        if (buttons.ElementAt(i).Location.Y > point.Y) {
                            moveTo = i;
                            break;
                        }
                        if (i == buttons.Count - 1) {
                            moveTo = i + 1; //移到最後一格要另外控制
                        }
                    }
                    //System.Console.WriteLine("moveFrom " + moveFrom);
                    //System.Console.WriteLine("moveTo " + moveTo);
                    if (buttons.Count > 0)
                    {
                        if (moveTo != moveFrom && (moveTo - 1) != moveFrom)
                        {
                            if (moveTo < buttons.Count)
                            {
                                FormMain.r8.functions[((int)(buttons.ElementAt(moveFrom).Tag))].posY = (float)(FormMain.r8.functions[((int)(buttons.ElementAt(moveTo).Tag))].posY - 0.5f);
                            }
                            else
                            {
                                FormMain.r8.functions[((int)(buttons.ElementAt(moveFrom).Tag))].posY = (float)(FormMain.r8.functions[((int)(buttons.ElementAt(buttons.Count - 1).Tag))].posY + 0.5f);
                            }
                            Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                            //Program.recordProgram(FormMain.r8);
                            resetButton();
                        }
                        else {
                            //20170214 leo 把拖曳與點選通通整合到左鍵，當[function 位置沒更換]，代表是點選不是拖曳
                            functionButton_Click(buttons.ElementAt(moveFrom), e);
                        }
                    }
                   
                    break;
            }
           
        }

        private void FormFunctions_DragLeave(object sender, EventArgs e)
        {
            //20170116 leo : 改用 DragDrop
            return;
            /*
            Point point = PointToClient(Cursor.Position);
            R8.Log("FormFunctions_DragLeave: " + point.X + "," + point.Y);
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            if (rect.Contains(point.X, point.Y))
            {
               // string[] senderArgs = (string[])sender;
                
                Function function = new Function(dragData, 0, 0);
                addDataSn(function);
                r8.addFunction(function);
                addButton(dragData, function.sn);
            }
            return;
            */
        }
        

        
        private void buttons_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 2)
            {
                //20170324 leo add 雙擊時切換 function enable
                //formMain.formFunction.checkBox.Checked = !formMain.formFunction.checkBox.Checked;

                //20171016 此功能改為[點擊 function block 兩下，執行 debug run 到該 function。]
                if (formMain.programFilePath == null)
                {
                    MessageBox.Show("Please save program before run it.");
                    return;
                }
                formMain.saveTempFile();
                StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                int r7h = R7.New();
                FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                BinaryReader binReader = new BinaryReader(sourceFile);
                byte[] programFileBytes = new byte[sourceFile.Length + 1];
                for (long i = 0; i < sourceFile.Length; i++)
                {
                    programFileBytes[i] = binReader.ReadByte();
                }
                programFileBytes[sourceFile.Length] = 0;
                sourceFile.Close();
                binReader.Close();
                //所以這邊需要 Debug 版 ， R7_RunToTargetFunction 多傳一個 isDebug 進去
                //.....但是這樣跑的話，會發生 Debug Image 被 Show 出來後，按 Enter 關不掉 (必須一個個視窗按右上角的 x 關閉)，估計原因為這邊不會停下來等，所以直接被 R7.Release 了。
                //所以在 R7_RunToTargetFunction 裡面加上[如果是 debug 模式則進行 cv::waitKey]

                int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 1);


                result = R7.Release(r7h);

            }
            else
            {
                 moveFrom = buttons.IndexOf((Button)sender);
                (sender as Button).DoDragDrop((sender as Button).Text, DragDropEffects.Move);
            }
            
            return;
        }

        public void functionButton_Click(object sender, EventArgs e)
        {            
            Button button = (sender as Button);
            //R8.logToLogBox("functionButton_Click: " + button.Name);
            Function function = FormMain.r8.functions[(int)button.Tag];
            //20170317 leo: 依早上討論， function 切換時才切換 isShowTargetVariable
            formMain.formVariables.isShowTargetVariable = true;
            formMain.formVariables.showVariables(function);
            formMain.formFunction.showFunction(function);
            return;
        }
        List<Function> targetFunctionList = new List<Function>();

        //20170215 leo: resetButton 改為不 clear 既有的 button 。
        public void resetButton()
        {

            int preScrollValue = this.panel2.VerticalScroll.Value;
            //System.Console.WriteLine("FormFunction resetButton: " + "buttons.Count = " + buttons.Count + ", functions = " + FormMain.r8.getFunctionsCount());
            int targetFunctionCounts = 0;
            targetFunctionList.Clear();
            int i = 0;
            Function function;
            Button button;
            Label label;


            //20171025 現在要顯示行號，這邊要增加進行編號
            List<Function> functionNumberList = new List<Function>();
            for (i = 0; i < FormMain.r8.getFunctionSnLast(); i++)
            {
                function = FormMain.r8.functions[i];
                if (function == null)
                {
                    continue;
                }
                functionNumberList.Add(function);
            }
            functionNumberList.Sort(delegate (Function i1, Function i2) { return i1.posY.CompareTo(i2.posY); });
            for (i = 0; i < functionNumberList.Count; i++) {
                functionNumberList[i].lineNumber = i + 1;
            }
            //function 編號結束

            if (isInSearchMode)
            {
                //20170317 add search mode
                for (i = 0; i <= FormMain.r8.getFunctionSnMax(); i++)
                {
                    function = FormMain.r8.functions[i];
                    if (function == null)
                    {
                        continue;
                    }
                    if (function.sn == -1 || function.sn == 0)
                    {
                        continue;
                    }
                    if (function.ToString().ToLower().Contains(textBoxSearch.Text.ToLower()))
                    {
                        targetFunctionList.Add(function);
                    }
                }
                targetFunctionCounts = targetFunctionList.Count;

            }
            else {
                targetFunctionCounts = FormMain.r8.getFunctionsCount();
               
            }
            
            while (buttons.Count < targetFunctionCounts)
            {
                addButton("", -1, true);
            }

            
            while (buttons.Count > targetFunctionCounts)
            {
                if (buttons.Count > 0)
                {
                    button = buttons.ElementAt(buttons.Count - 1);
                    
                    if (lastFocusButtonIndex == buttons.Count - 1)
                    {
                        if (buttons.Count > 2)
                        {
                            buttons.ElementAt(buttons.Count - 2).Focus();
                        }
                    }

                    button.Dispose();
                    buttons.Remove(button);
                    //20171025 leo: 行號相關處理：移除掉一個 button 時，就對應砍掉一個行號
                    if (lineLabels.Count > 0) {
                        label = lineLabels[lineLabels.Count - 1];
                        label.Dispose();
                        lineLabels.Remove(label);
                    }

                    buttonsLocationY = buttonsLocationY - button.Height - buttonsLocationGap;
                }
                else
                {
                    break;
                }
            }
            



            List<Function> functionList = new List<Function>();
            if (isInSearchMode)
            {
                functionList = targetFunctionList;
            }
            else {
                for (i = 0; i < FormMain.r8.getFunctionSnLast(); i++)
                {
                    function = FormMain.r8.functions[i];
                    if (function == null)
                    {
                        continue;
                    }
                    //addButton("[" + function.sn + "] [" + function.name + "]", function.sn);
                    //function.posY = buttons.Count;
                    functionList.Add(function);
                    // addButton("[" + function.sn + "] [" + function.name + "]", function.sn);
                }

            }
            
            functionList.Sort(delegate (Function i1, Function i2) { return i1.posY.CompareTo(i2.posY); });
            for (i = 0; i < buttons.Count; i++)
            {
                if (i < functionList.Count)
                {
                    function = functionList.ElementAt(i);
                    button = buttons.ElementAt(i);
                    label = lineLabels[i];
                    //20171025 leo: 行號相關處理
                    label.Text = "" + function.lineNumber;
                    if (function.enable)
                    {
                        button.Text = getFunctionShowText(function);
                        button.ForeColor = Color.Black;
                    }
                    else
                    {
                        //button.Text = getFunctionShowText(function) + "[disabled]";
                        button.Text = getFunctionShowText(function);
                        button.ForeColor = Color.Gray;
                    }


                    button.Tag = function.sn;
                    if (!isInSearchMode)
                    {
                        function.posY = i + 1;
                    }
                }
                else {
                    //多餘的 button 要移除
                    button = buttons.ElementAt(i);
                    button.Tag = -1;
                }
            }

            /*
            //依目前架構，buttons數量只會比functions數量大或數量相等，所以只需要把最後面的 button 移除
            //移除多出來的 button
            for (i = buttons.Count - 1; i > -1; i--)
            {
                button = buttons.ElementAt(i);
                if ((int)(button.Tag) == -1)
                {
                    buttonsLocationY = buttonsLocationY - button.Height - buttonsLocationGap;
                    if (lastFocusButtonIndex == i) {
                        //System.Console.WriteLine("Focus At Last:" + lastFocusButtonIndex);
                        //特殊情況：若最後一顆 button 被 focus ，移除前先把 focus 往前一格
                        if (buttons.Count > 1) {
                            buttons.ElementAt(i - 1).Focus();
                        }
                    }
                    button.Dispose();
                    buttons.Remove(button);
                }
            }
            */
            //System.Console.WriteLine("focus:" + lastFocusButtonIndex + ", " + lastFocusButtonSn);
            //設置 focus
            if (lastFocusButtonIndex != -1)
            {
                bool isFind = false;
                for (i = 0; i < buttons.Count; i++)
                {
                    button = buttons.ElementAt(i);
                    if ((int)(button.Tag) == lastFocusButtonSn)
                    {
                        isFind = true;
                        button.Focus();
                        break;
                    }
                }

                if (!isFind) {
                    if (lastFocusButtonIndex < buttons.Count)
                    {
                        buttons[lastFocusButtonIndex].Focus();
                    }
                    else {
                        if (buttons.Count > 0)
                        {
                            buttons[buttons.Count - 1].Focus();
                        }
                    }
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

            this.Text = "Program Functions  " + buttons.Count + " of " +FormMain.r8.getFunctionsCount();


            this.panel2.VerticalScroll.Value = preScrollValue;
            this.PerformLayout();

            this.Invalidate();
            return;
        }

        public void resetButtonBackup()
        {

            int preScrollValue = this.panel2.VerticalScroll.Value;
            //清空畫面
            foreach (Button b in buttons)
            {
                b.Dispose();
            }
            
            buttons.Clear();
            buttonsLocationY = buttonStartY;
            //讀 xml
            Function function = null;
            List<Function> functionList = new List<Function>();
            for (int i = 0; i < FormMain.r8.getFunctionSnLast(); i++)
            {
                function = FormMain.r8.functions[i];
                if (function == null)
                {
                    continue;
                }
                //addButton("[" + function.sn + "] [" + function.name + "]", function.sn);
                //function.posY = buttons.Count;
                functionList.Add(function);
               // addButton("[" + function.sn + "] [" + function.name + "]", function.sn);
            }
            
            functionList.Sort(delegate (Function i1, Function i2) { return i1.posY.CompareTo(i2.posY); });
            
            for (int i = 0; i < functionList.Count; i++)
            {
                function = functionList.ElementAt(i);
                // System.Console.WriteLine("button:" + function.name + " pos =" + function.posY);   
                //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
                //addButton("[" + function.sn + "] [" + function.name + "]", function.sn, function.enable);
                /*
                if (function.remark.Length > 0)
                {
                    addButton(function.name + " " + function.sn + " " + "[" + function.remark + "]", function.sn, function.enable);
                }
                else
                {
                    addButton(function.name + " " + function.sn + " ", function.sn, function.enable);
                }
                */
                //20170309 leo end
                addButton(getFunctionShowText(function), function.sn, function.enable);
                function.posY = buttons.Count;
                //System.Console.WriteLine("After_button:" + function.name + " pos =" + function.posY);
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

            this.Invalidate();
            return;
        }

        public void readProgramXml(string path)
        {

            //清空畫面
            foreach (Button b in buttons)
            {
                b.Dispose();
            }
            buttons.Clear();
            buttonsLocationY = buttonStartY;

            //標籤也要清空
            foreach (Label l in lineLabels)
            {
                l.Dispose();
            }
            lineLabels.Clear();


            //20170322 leo: 增加[New File] 模式，清空 variable 後就 return 
            if (path == null) {                
                FormMain.r8 = new R8();
                formMain.formVariables.showVariables(null);
                formMain.formFunction.showFunction(null);
                R8.clearLogBox();
                this.Text = "Program Functions  " + buttons.Count + " of " + FormMain.r8.getFunctionsCount();
                return;
            }
            //20170322 end

            //讀 xml
            //20180212 新的 r6 不是純 xml 了，前面會有額外的字串 # XXXXX
            //總之改用 Stream 先讀一遍，把是 # 開頭的砍了

            int counter = 0;
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(path);
            StringBuilder stringBuilder = new StringBuilder();
            while ((line = file.ReadLine()) != null)
            {
               // MessageBox.Show("Line " + counter + " = " + line);
                if (line.Length > 1) {
                    if (line[0] != '#') {
                        stringBuilder.Append(line);
                    }
                }
                counter++;
            }
            FormMain.r8 = new R8(XElement.Parse(stringBuilder.ToString()));
            file.Close();
            stringBuilder.Clear();

            //FormMain.r8 = new R8(XElement.Load(path));

            Function function = null;
            List<Function> functionList = new List<Function>();
            //改為 sort 完依 posY 排序再放 button
            for (int i = 0; i < FormMain.r8.getFunctionSnLast(); i++)
            {
                function = FormMain.r8.functions[i];
                if (function == null)
                {
                    continue;
                }
                functionList.Add(function);
                //addButton("[" + function.sn + "] [" + function.name + "]", function.sn);
            }
            
            functionList.Sort(delegate (Function i1, Function i2) { return i1.posY.CompareTo(i2.posY); });
            for (int i = 0; i < functionList.Count; i++) {
                function = functionList.ElementAt(i);
                //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
                //addButton("[" + function.sn + "] [" + function.name + "]", function.sn, function.enable);
                /*
                if (function.remark.Length > 0)
                {
                    addButton(function.name + " " + function.sn + " " + "[" + function.remark + "]" , function.sn, function.enable);
                }
                else {
                    addButton(function.name + " " + function.sn + " ", function.sn, function.enable);
                }*/
                addButton(getFunctionShowText(function), function.sn, function.enable);
                //20170309 leo end
                function.posY = buttons.Count;//讀出 button 後，應該要再依實際 button 位置進行 posY排序
            }
            this.Text = "Program Functions  " + buttons.Count + " of " + FormMain.r8.getFunctionsCount();
            //20170125 leo: 讀檔案後 show variable
            formMain.formVariables.showVariables(null);
            //20170208 leo: 讀檔案後 formFunction 清空
            formMain.formFunction.showFunction(null);
            this.Invalidate();
            Program.clearRecord();
            Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
            //Program.recordProgram(FormMain.r8);
            return;
        }

        //20170929 leo: undo redo 用
        public void readProgramR8(R8 r8) {

            //清空畫面
            foreach (Button b in buttons)
            {
                b.Dispose();
            }
            buttons.Clear();
            buttonsLocationY = buttonStartY;

            //標籤也要清空
            foreach (Label l in lineLabels)
            {
                l.Dispose();
            }
            lineLabels.Clear();


            //20170322 leo: 增加[New File] 模式，清空 variable 後就 return 
            if (r8 == null)
            {
                formMain.formVariables.showVariables(null);
                formMain.formFunction.showFunction(null);
                R8.clearLogBox();
                this.Text = "Program Functions  " + buttons.Count + " of " + FormMain.r8.getFunctionsCount();
                return;
            }
            Function function = null;
            List<Function> functionList = new List<Function>();
            //改為 sort 完依 posY 排序再放 button
            for (int i = 0; i < r8.getFunctionSnLast(); i++)
            {
                function = r8.functions[i];
                if (function == null)
                {
                    continue;
                }
                functionList.Add(function);
            }

            functionList.Sort(delegate (Function i1, Function i2) { return i1.posY.CompareTo(i2.posY); });
            for (int i = 0; i < functionList.Count; i++)
            {
                function = functionList.ElementAt(i);
                addButton(getFunctionShowText(function), function.sn, function.enable);
                function.posY = buttons.Count;//讀出 button 後，應該要再依實際 button 位置進行 posY排序
            }
            this.Text = "Program Functions  " + buttons.Count + " of " + FormMain.r8.getFunctionsCount();
            formMain.formVariables.showVariables(null);
            formMain.formFunction.showFunction(null);
            this.Invalidate();
            return;
        }



        private void pictureBoxSearch_Click(object sender, EventArgs e)
        {
            isInSearchMode = true;
            resetButton();
            return;
        }

        public void clearSearchBox()
        {
            if (isInSearchMode)
            {
                textBoxSearch.Text = "";
                isInSearchMode = false;
                resetButton();
            }
        }

        private void pictureBoxCancel_Click(object sender, EventArgs e)
        {
            textBoxSearch.Text = "";
            isInSearchMode = false;
            resetButton();
            return;
        }

        private void textBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                isInSearchMode = true;
                resetButton();
            }
            return;
        }

        //20170327 Functions Variables 方塊不顯示 SN
        // Functions 的名稱顯示分散太多地方，把它都整合到這邊
        private string getFunctionShowText(Function function) {
            string str;
            if (function.remark.Length > 0)
            {
                str = function.name + " (" + function.remark + ")";
            }
            else
            {
                str = function.name;
            }
            return str;
        }

        private void FormFunctions_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formFunctions = null;
        }



        private void FormFunctions_SizeChanged(object sender, EventArgs e)
        {

            panel2.Height = this.ClientSize.Height - panel2.Location.Y;
            panel2.Width = this.ClientSize.Width;
        }


        private void panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }

        //https://msdn.microsoft.com/zh-tw/library/system.windows.forms.control.previewkeydown(v=vs.110).aspx
        void button_KeyDown(object sender, KeyEventArgs e)
        {
        //    return;
            //MessageBox.Show("button_KeyDown " + e.KeyValue + e.KeyCode);

          //  MessageBox.Show("formMain ActiveMdiChild = " + formMain.ActiveMdiChild);
            //MessageBox.Show("ActiveForm = " + Form.ActiveForm);
            //20171103 現在要連續刪除，需要額外判定 Form 有沒有被 focus
            //MessageBox.Show("formMain.ActiveMdiChild: " + formMain.ActiveMdiChild.Text);
            if (formMain.ActiveMdiChild.GetType() == this.GetType())
            {
                if (e.KeyValue == 8 || e.KeyValue == 46)
                {
                    if (formMain.formFunction != null)
                    {
                        formMain.formFunction.removeButton_Click(sender, null);
                        //20171103 項目[Program Functions 跟 Program Variables 無法連續按 Backspace 或 Del 連續進行刪除]
                        //所以要[刪除掉後，強制選擇下一個 Function 或 Variable]
                        Control con;
                        con = this.ActiveControl;
                        if (con != null)
                        {

                            //MessageBox.Show("focus: " + con.Name + ", " + con.Text + ", MDI = " + formMain.ActiveMdiChild.GetType());
                            if (con.GetType() == typeof(Button))
                            {
                                //MessageBox.Show("it is button");
                                if (buttons.Contains(con))
                                {
                                    //MessageBox.Show("it in buttons");
                                    functionButton_Click(con, EventArgs.Empty);
                                }
                            }
                            else
                            {
                                //MessageBox.Show("it not button");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("focus null");
                        }

                    }
                }
            }
            else {

              //  MessageBox.Show("GetType: " + formMain.ActiveMdiChild.GetType() + " != " + this.GetType());
               
            }

        }

        private void button_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }
    }
}
