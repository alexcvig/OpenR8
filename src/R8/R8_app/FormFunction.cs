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
    public partial class FormFunction : Form
    {
        public static ToolTip toolTip = new ToolTip(); //20171110 Leo: 避免 memory leak ， ToolTip 應該放在 FormFunction 層而不是 PanelParameter 層
        //private List<Label> labels = new List<Label>();

        //20170327 信件項目 3. Function SN 的上方及左方 padding 請參照 Variable SN 。
        private const int labelsStartX = 10;
        private const int labelsStartY = 10;
        private int labelsLocationY = 0;
        public Function function = null;
        private TextBox remarkTextBox;//20170120 leo: 加入 remark 編輯用欄位

        public List<PanelParameter> PanelParameterList = new List<PanelParameter>();
        public CheckBox checkBox = null;

        //20170817 leo: 發現有時候在 FormFunctions 拉 function 順序時，會話畫面捲動導致 function 被誤拖曳，
        //原因為 Form 在 onFocus 時會自動捲動到 [該 Form 中，前一個被選取物件] 的位置。
        //參照 https://stackoverflow.com/questions/419774/how-can-you-stop-a-winforms-panel-from-scrolling/912610#912610
        //用 override 把該功能關掉
        //然後其他類似結構的地方也加上這一段
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }

        public FormFunction()
        {

            InitializeComponent();
        }

        private void FormFunction_Load(object sender, EventArgs e)
        {
        }

        public void showFunction(Function function) {
            //20170208 leo: add: 畫面有 VerticalScroll 時，紀錄 Scroll 位置，避免畫面跑掉。
            int preScrollValue = this.VerticalScroll.Value;


            this.function = function;
            //清空畫面
            //this.Controls.Clear();
            //20171110 allen 回報他跑 A3 發生當機，測一下似乎是 memory leak ，然後問題大概是出在這邊
            //參照 https://stackoverflow.com/questions/7705234/how-to-clear-controls-without-causing-a-memory-leak
            // 直接 Controls.Clear() 會 memory leak ，要改成一個一個 Dispose
            int i;
            for (i = Controls.Count - 1; i >= 0; i--)
            {
                this.Controls[i].Dispose();
            }
            FormFunction.toolTip.RemoveAll();

            labelsLocationY = labelsStartY;
            PanelParameterList.Clear();

            //20170208 leo: 當 function 為 null ，清空畫面後就 return
            if (function == null) {
                return;
            }

            //設畫面 20170117 leo 介面更改
            //addLabel("#" + function.sn);
            //addLabel("SN      " + function.sn);//20170407 SN要移到下面
            

            //20170118 leo 加入 removeFunction功能，按鈕放這邊
            Button buttonDelete = new Button();
            buttonDelete.Text = "Delete";
            //button.Width = 130;
            //button.Height = 60;
            buttonDelete.Width = 60;
            buttonDelete.Height = 30;
            buttonDelete.Location = new Point(this.Width - buttonDelete.Width - 40, labelsStartY);
            buttonDelete.Click += removeButton_Click;
            this.Controls.Add(buttonDelete);


            Button buttonCopy = new Button();
            buttonCopy.Text = "Copy";
            buttonCopy.Width = 60;
            buttonCopy.Height = 30;
            buttonCopy.Location = new Point(this.Width - buttonCopy.Width - 40, buttonDelete.Location.Y + buttonDelete.Height + 15);
            buttonCopy.Click += copyButton_Click;
            this.Controls.Add(buttonCopy);


            addLabel("Name: " + function.name);
            addLabel("");//Sn搬下去後標籤會黏住，多空一行
            //20170120 leo: 加入 remark 欄位

            Label label = new Label();
            label.Text = "Remark";
            label.Width = 60;
            label.Location = new Point(labelsStartX, labelsLocationY);
            this.Controls.Add(label);

            remarkTextBox = new TextBox();
            remarkTextBox.Text = function.remark;
            remarkTextBox.Width = 250;
            remarkTextBox.Location = new Point(label.Location.X + label.Width, labelsLocationY);
            remarkTextBox.LostFocus += remarkTextBox_LostFocus;
            //remarkTextBox_KeyDown
            remarkTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.remarkTextBox_KeyDown);
            this.Controls.Add(remarkTextBox);
            labelsLocationY += label.Height;

            //與 variable 間留個較大的空格
            labelsLocationY += 20;
            //System.Console.WriteLine("function.parameters.Count = " + function.parameters.Count);
            for (i = 0; i < function.parameters.Count; i++) {
                addParameter(function.parameters.ElementAt(i));
            }


            //20170215 leo: 改為: 先一個 Lable 寫 "enable" ，再一個無字串的 CheckBox
            label = new Label();
            label.Text = "Enable";
            label.Width = 60;
            label.Location = new Point(labelsStartX, labelsLocationY);
            this.Controls.Add(label);


            //20170213 leo: 加上 enable 
            checkBox = new CheckBox();
            checkBox.Text = "";
            checkBox.Checked = function.enable;
            //checkBox.Location = new Point(0, labelsLocationY);
            checkBox.Location = new Point(label.Location.X + label.Width, labelsLocationY - 5);
            checkBox.CheckedChanged += checkBox_CheckedChanged;
            labelsLocationY += checkBox.Height;
            this.Controls.Add(checkBox);

            //和上面距離有點近，多空一行
            addLabel("SN               " + function.sn);

            if (preScrollValue > this.VerticalScroll.Maximum)
            {
                preScrollValue = this.VerticalScroll.Maximum;
            }
            else if (preScrollValue < this.VerticalScroll.Minimum)
            {
                preScrollValue = this.VerticalScroll.Minimum;
            }

            //
            if (FormLibrary.functionURL.ContainsKey(function.name)) {
                string urlStr;
                FormLibrary.functionURL.TryGetValue(function.name, out urlStr);

                if (urlStr.Length > 0) {
                    Button button = new Button();
                    button.Text = "Help";
                    //button.Width = 130;
                    //button.Height = 60;
                    button.Width = 60;
                    button.Height = 30;
                    button.Location = new Point(this.Width - button.Width - 40, labelsLocationY - 30);
                    button.Tag = urlStr;
                    button.Click += helpButton_Click;
                    this.Controls.Add(button);
                }


            }

            this.VerticalScroll.Value = preScrollValue;
            this.PerformLayout();
            return;
        }

        public void resetFunction() {
            if (function != null)
            {
                showFunction(function);
            }
            else {
                //this.Controls.Clear(); 
                //20171110 allen 回報他跑 A3 發生當機，測一下似乎是 memory leak ，然後問題大概是出在這邊
                //參照 https://stackoverflow.com/questions/7705234/how-to-clear-controls-without-causing-a-memory-leak
                // 直接 Controls.Clear() 會 memory leak ，要改成一個一個 Dispose
                int i;
                for (i = Controls.Count - 1; i >= 0; i--)
                {
                    this.Controls[i].Dispose();
                }
                FormFunction.toolTip.RemoveAll();
            }
            return;
        }

        public void resetPanels(String searchStr)
        {
            foreach (PanelParameter panel in PanelParameterList) {
                panel.resetPanel(searchStr);
            }
            return;
        }


        private void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            //System.Console.WriteLine("function.checkBox_CheckedChanged = ");
            if (function != null) {
                if (function.sn != -1 && function.sn != 0) {
                    function.enable = (sender as CheckBox).Checked;
                }
            }

            FormMain formMain = this.MdiParent as FormMain;
            formMain.formFunctions.resetButton();
            Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
            return;
        }



        public void addLabel(string text)
        {
            Label label = new Label();
            label.Name = text;
            label.Text = text;
            label.Width = this.Width - 200;
            //label.Height = 40;
            label.Location = new Point(labelsStartX, labelsLocationY);
            labelsLocationY += label.Height;
            this.Controls.Add(label);
            //labels.Add(label);
        }

        public void addParameter(Function.Parameter parameter)
        {
            PanelParameter panel;
            
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            R8.GetVariableType(str, R7.STRING_SIZE, parameter.type);
            //MessageBox.Show("function.name = " + function.name + ", parameter.type = " + parameter.type + ", typestr = " + str);

            FormMain formMain = this.MdiParent as FormMain;

            
            if (parameter.option.Contains("RECTS")) {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 5, function);
            }
            else if (
                parameter.name.Equals("File Name") ||
                parameter.name.Equals("Image File Name") ||
                parameter.option.Contains("FILE_PATH")
                )
            {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 1);
            }
            else if (str.ToString().Equals("image") || str.ToString().Equals("Image") || str.ToString().Equals("mat") || str.ToString().Equals("Mat"))
            {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 2);
            }
            else if (((function.name.Equals("ImageCut") || function.name.Equals("ImageDrawRect")
                || function.name.Equals("Image_Crop") || function.name.Equals("Image_DrawRect")) && parameter.name.Equals("X"))) {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 3, function);
            }
            else if (str.ToString().Equals("json") || str.ToString().Equals("Json"))
            {
                //20170405 add: 加上 json 顯示
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 4);
            } else if (parameter.option.Contains("FOLDER_PATH")) {
                //20170908 leo add: 某些模組(例如 R7 tensorflow library)會需要選取資料夾功能而不是選取檔案
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 6);
            }
            else if ((function.name.Equals("Image_Rotate") && parameter.name.Equals("Degree")))
            {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 7, function);
            }
            else if (((function.name.Equals("Image_RotateAndCrop") || function.name.Equals("Image_CropAndRotate")) && parameter.name.Equals("Degree")))
            {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 8, function);
            }
            else {
                panel = new PanelParameter(formMain, this.Width - 50, 30, parameter, 0);
            }

            str.Clear();
            panel.Location = new Point(0, labelsLocationY);
            //panel.BackColor = Color.White;
            this.Controls.Add(panel);
            labelsLocationY += panel.Height;
            PanelParameterList.Add(panel);
        }
        
        private void remarkTextBox_LostFocus(object sender, EventArgs e)
        {
            if (function != null && remarkTextBox != null)
            {
               
                if (!remarkTextBox.Text.ToString().Equals(function.remark)) {
                    //MessageBox.Show("remarkTextBox_LostFocus");
                    function.remark = remarkTextBox.Text;
                    //20170309 leo: 現在 functions 也要顯示 remark ，所以這邊也需要聯動
                    FormMain formMain = this.MdiParent as FormMain;
                    //20170725 leo: 修正[選取 remarkTextBox 時關閉程式]會 crash 的問題
                    if (formMain != null && formMain.formFunctions != null)
                    {
                        formMain.formFunctions.resetButton();
                    }
                    
                    Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                }
               
            }
            return;
        }

        private void remarkTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                remarkTextBox_LostFocus(sender, e);
            }
        }

        public void removeButton_Click(object sender, EventArgs e)
        {
            if (function == null) {
                return;
            }
            FormMain.r8.functions[function.sn] = null;
            //FormMain.r8.setFunction(null, function.sn);
            function = null;
            //formMain.formVariables.showVariables(formMain.formFunction.function);//現在的 formVariables 是 show 所有 variable 所以不需要連動
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formFunctions.resetButton();
            resetFunction();
            //Program.recordProgram(FormMain.r8);
            Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
            return;
        }

        private void copyButton_Click(object sender, EventArgs e)
        {
            /*          FormMain.r8.functions[function.sn] = null;
                        function = null;
                        formMain.formVariables.showVariables(formMain.formFunction.function);
                        formMain.formFunctions.resetButton();
                        resetFunction();
                        */
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formFunctions.copyButtonToLast(this.function);
            // parameter.variableSn = variables[typeComboBox.SelectedIndex].sn
            Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
            return;
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            Button b = sender as Button;
            System.Diagnostics.Process.Start(b.Tag.ToString());
            return;
        }

        private void FormFunction_FormClosing(object sender, FormClosingEventArgs e)
        {

        }


        public class PanelParameter : Panel
        {
            private FormMain formMain;
            public Label nameLabel;
            public ComboBox typeComboBox;
            public Button editButton;
            public Button newButton;
            public Button extraButton;//20170324 特定 Function 要開額外功能的按鈕(例如 ImageOpen ，要加直接選檔案的功能)
            public Function.Parameter parameter;
            public int extraMode = 0;
            //private Variable[] variables;
            private List<Variable> variables;
            private bool isFocusVariableAfterCheckBoxChange = true;
            private Function function;

            public PanelParameter(FormMain formMain, int width, int height, Function.Parameter parameter, int extraMode)
            {
                this.formMain = formMain;
                this.Width = width;
                this.Height = height;
                this.parameter = parameter;
                this.extraMode = extraMode;
                setPanel();

                return;
            }

            public PanelParameter(FormMain formMain, int width, int height, Function.Parameter parameter, int extraMode, Function function)
            {
                this.formMain = formMain;
                this.Width = width;
                this.Height = height;
                this.parameter = parameter;
                this.extraMode = extraMode;
                this.function = function;
                setPanel();

                return;
            }

            void comboBox_MouseWheelDoNothing(object sender, MouseEventArgs e)
            {
                ((HandledMouseEventArgs)e).Handled = true;
            }


            //20171031 leo: A3 反映他們的參數名稱有些很長會無法顯示完全，加個 ToolTip
            //ToolTip toolTip = new ToolTip(); //20171110 Leo: 這邊有 memory leak ，更正
            public void setPanel() {
                //this.Controls.Clear();

                //20171110 allen 回報他跑 A3 發生當機，測一下似乎是 memory leak ，然後問題大概是出在這邊
                //參照 https://stackoverflow.com/questions/7705234/how-to-clear-controls-without-causing-a-memory-leak
                // 直接 Controls.Clear() 會 memory leak ，要改成一個一個 Dispose
                int i;
                for (i = Controls.Count - 1; i >= 0; i--)
                {
                    this.Controls[i].Dispose();
                }
                //FormFunction.toolTip.RemoveAll(); 20171116 fix: 這邊不應該 RemoveAll toolTip，否則會變成只有最後一個 label 才有 toolTip

                nameLabel = new Label();
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, parameter.type);
                nameLabel.Text = parameter.name + " (" + str.ToString() + ")";
                if (parameter.option.Contains("INOUT"))
                {
                    nameLabel.ForeColor = Color.Green;
                }
                else if (parameter.option.Contains("OUT"))
                {
                    nameLabel.ForeColor = Color.Blue;
                }
                str.Clear();
                nameLabel.Width = this.Width / 4;
                nameLabel.Location = new Point(labelsStartX, 0);
                

                FormFunction.toolTip.SetToolTip(nameLabel, nameLabel.Text.ToString());
                
                this.Controls.Add(nameLabel);

                
                typeComboBox = new ComboBox();
                //然後這邊 DropDown 寬度改為最長字寬
                int maxSize = 0;
                variables = FormMain.r8.getVariableArrayByType(parameter.type, true, ref maxSize, null);
                typeComboBox.Items.AddRange(variables.ToArray());
                //if (parameter.variableSn != -1)
                //{
                    for (i = 0; i < variables.Count; i++)
                    {
                        if (parameter.variableSn == variables[i].sn)
                        {
                            typeComboBox.SelectedIndex = i;
                        }
                    }
                //}
                //R8.logToLogBox("" + typeComboBox.Width);//121
                typeComboBox.Width = 160;
                typeComboBox.Location = new Point(nameLabel.Location.X + nameLabel.Width + 10, 0);

                typeComboBox.SelectedIndexChanged += typeComboBox_SelectedIndexChanged;
                typeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                // DropDownList 不好加 ToolTip，直接把它框框變寬比較快
                if (maxSize * 8 > 160) {
                    typeComboBox.DropDownWidth = maxSize * 8;
                }
                

                this.Controls.Add(typeComboBox);


               

                //20170614 leo: 依指示取消滑鼠滾輪
                //方法參照 https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                typeComboBox.MouseWheel += new MouseEventHandler(comboBox_MouseWheelDoNothing);

                //20170119 leo: 今天的架構更改後這顆按鈕好像用不到了？暫時還是先留著。
                editButton = new Button();
                //R8.logToLogBox("" + editButton.Width);//75
                editButton.Width = 40;
                editButton.Text = "Edit";
                editButton.Location = new Point(typeComboBox.Location.X + typeComboBox.Width + 10, 0);
                editButton.Click += editButton_Click;
                this.Controls.Add(editButton);

                newButton = new Button();
                newButton.Width = 40;
                newButton.Text = "New";
                newButton.Location = new Point(editButton.Location.X + editButton.Width + 10, 0);
                newButton.Click += newButton_Click;
                this.Controls.Add(newButton);
                switch (extraMode) {
                    case 1://openFileDailog 
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "...";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                              {
                                  if (R8.formMain.programFilePath == null)
                                  {
                                      //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                      //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                      //加個警示訊息
                                      MessageBox.Show("Please save program before choose file.");
                                      return;
                                  }

                                  OpenFileDialog openFileDialog = new OpenFileDialog();
                                  openFileDialog.Title = "Select a File";
                                  openFileDialog.InitialDirectory = FormMain.workSpacePath;
                                  if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                  {
                                      //formVariables.isShowTargetVariable = false;
                                      //formRCP.readProgramXml(openFileDialog.FileName);
                                      String subString = null;
                                      /*
                                      int subStringStart = openFileDialog.FileName.LastIndexOf('\\') + 1;
                                      if (subStringStart > openFileDialog.FileName.Length || subStringStart < 0)
                                      {
                                          //subString = null;
                                          subString = "";
                                      }
                                      else {
                                          subString = openFileDialog.FileName.Substring(subStringStart);
                                      }
                                      */
                                      //20170808 leo: 討論後這個要改成相對路徑
                                      subString = R8.GetRelativePath(FormMain.workSpacePath + "\\", openFileDialog.FileName);
                                      if (parameter.variableSn == -1 || parameter.variableSn == 0)
                                      {
                                          //addVariable(openFileDialog.FileName);
                                          addVariable(parameter.name, subString);
                                      }
                                      else {
                                          //editVariable
                                          Variable variable = FormMain.r8.variables[parameter.variableSn];
                                          variable.value = subString;
                                          editVariable(variable);
                                      }
                                  }
                                  return;
                              };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 2://display Image
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "View";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {

                                if (formMain.programFilePath == null)
                                {
                                    //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                    //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                    //加個警示訊息
                                    MessageBox.Show("Please save program before run it.");
                                    return;
                                }
                                //20170202 leo : Run 時自動 save
                                //formMain.saveToolStripMenuItem_Click(sender, e);
                                formMain.saveTempFile();//20171115 change

                                //20170206 leo: 新版換成直接呼叫 dll
                                //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
                                if (formMain.tempProgramFilePath != null)
                                {
                                    //StringBuilder resultBuffer = new StringBuilder(R7.RESULT_SIZE);
                                    //byte[] resultBuffer = new byte[2048 * 2048 * 3];
                                    StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                                    int r7h = R7.New();
                                    FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                                    BinaryReader binReader = new BinaryReader(sourceFile);
                                    byte[] programFileBytes = new byte[sourceFile.Length + 1];
                                    for (long il = 0; il < sourceFile.Length; il++)
                                    {
                                        programFileBytes[il] = binReader.ReadByte();
                                    }
                                    programFileBytes[sourceFile.Length] = 0;
                                    sourceFile.Close();
                                    binReader.Close();
                                    int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 0);
                                    //取圖要取到 C# 這邊，才有辦法進行後續動作(縮放、框 rect)
                                    int imageW = 0, imageH = 0, imageCh = 0;
                                    //R7.ImageGetInfo(r7h, ref imageW, ref imageH, ref imageCh);
                                    R7.ImageGetInfo(r7h, parameter.variableSn, ref imageW, ref imageH, ref imageCh);
                                    //MessageBox.Show("imageW = " + imageW + " imageH = " + imageH + " imageCh = " + imageCh);
                                    if (imageW == 0 || imageH == 0 || imageCh == 0)
                                    {
                                        MessageBox.Show("Empty image.");
                                    }
                                    else
                                    {
                                        int imageSize = imageW * imageH * imageCh;
                                        byte[] byteBuffer = new byte[imageSize];
                                        int res = R7.ImageGet(r7h, parameter.variableSn, byteBuffer, imageSize);
                                        //MessageBox.Show("RET: " + ret);
                                        Image image = R8.ByteArrayToImage(byteBuffer, byteBuffer.Length, imageW, imageH, imageCh);

                                        FormImage formImage = new FormImage(0);
                                        //formImage.MdiParent = formMain;
                                        formImage.setImage(image, FormMain.r8.variables[parameter.variableSn].name);
                                        formImage.Show();
                                        //formMain.Focus();
                                        //formMain.SendToBack();//20171116 開多個 ImageView 時保持 formMain 在最下面
                                        //但這樣會導致[有開其他軟體的視窗]時， formMain 壓在它們的下層
                                        //改為 [R8 開一個 List 存放所有 ImageView ，每個 ImageView 被 new 時各別 BringToFront]
                                        R8.AllFormImageBringToFront(formImage);
                                    }
                                    result = R7.Release(r7h);
                                }

                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 3://select rect
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "View";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {

                                if (formMain.programFilePath == null)
                                {
                                    //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                    //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                    //加個警示訊息
                                    MessageBox.Show("Please save program before run it.");
                                    return;
                                }
                                //20170202 leo : Run 時自動 save
                                //formMain.saveToolStripMenuItem_Click(sender, e);
                                formMain.saveTempFile();

                                //20170206 leo: 新版換成直接呼叫 dll
                                //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
                                if (formMain.tempProgramFilePath != null)
                                {
                                    //StringBuilder resultBuffer = new StringBuilder(R7.RESULT_SIZE);
                                   // byte[] resultBuffer = new byte[2048 * 2048 * 3];
                                    StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                                    int r7h = R7.New();
                                    FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                                    BinaryReader binReader = new BinaryReader(sourceFile);
                                    byte[] programFileBytes = new byte[sourceFile.Length + 1];
                                    for (long il = 0; il < sourceFile.Length; il++)
                                    {
                                        programFileBytes[il] = binReader.ReadByte();
                                    }
                                    programFileBytes[sourceFile.Length] = 0;
                                    sourceFile.Close();
                                    binReader.Close();
                                    //int result = R7.ImageDisplay(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, function.parameters.ElementAt(0).variableSn, resultBuffer, R7.RESULT_SIZE);
                                    int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 0);
                                    int imageW = 0, imageH = 0, imageCh = 0;
                                    R7.ImageGetInfo(r7h, function.parameters.ElementAt(0).variableSn, ref imageW, ref imageH, ref imageCh);
                                    //MessageBox.Show("imageW = " + imageW + " imageH = " + imageH + " imageCh = " + imageCh);
                                    if (imageW == 0 || imageH == 0 || imageCh == 0)
                                    {
                                        MessageBox.Show("Empty image.");
                                    }
                                    else
                                    {
                                        int imageSize = imageW * imageH * imageCh;
                                        byte[] byteBuffer = new byte[imageSize];
                                        int res = R7.ImageGet(r7h, function.parameters.ElementAt(0).variableSn, byteBuffer, imageSize);
                                        //MessageBox.Show("RET: " + ret);
                                        Image image = R8.ByteArrayToImage(byteBuffer, byteBuffer.Length, imageW, imageH, imageCh);


                                        //在這邊，如果 x y w h 未設置則設 variable
                                        int baseSn = function.parameters.IndexOf(parameter);
                                        int[] defaultSize = { 0, 0, imageW / 2, imageH / 2};
                                        for (i = 0; i < 4; i++) {
                                            if (function.parameters.ElementAt(baseSn + i).variableSn == -1 || function.parameters.ElementAt(baseSn + i).variableSn == 0) {
                                                //function.parameters.ElementAt(baseSn + i).addVariable("");
                                                //MessageBox.Show("formMain.formFunction.PanelParameterList.ElementAt(baseSn + " + i + ").Name = " + formMain.formFunction.PanelParameterList.ElementAt(baseSn + i).parameter.name);
                                                formMain.formFunction.PanelParameterList.ElementAt(baseSn + i).addVariable(formMain.formFunction.PanelParameterList.ElementAt(baseSn + i).parameter.name, "" + defaultSize[i]);
                                            }
                                        }

                                        FormImage formImage = new FormImage(1, function, formMain.formFunction, baseSn);
                                        //formImage.MdiParent = formMain;
                                        formImage.setImage(image, FormMain.r8.variables[function.parameters.ElementAt(0).variableSn].name);
                                        formImage.Show();
                                        R8.AllFormImageBringToFront(formImage);
                                    }
                                    result = R7.Release(r7h);
                                }

                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 4:
                        //20170405 加上 json 顯示功能
                         {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "View";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {
                                if (formMain.programFilePath == null)
                                {
                                    MessageBox.Show("Please save program before run it.");
                                    return;
                                }
                                //formMain.saveToolStripMenuItem_Click(sender, e);
                                formMain.saveTempFile();

                                if (formMain.tempProgramFilePath != null)
                                {
                                   // byte[] resultBuffer = new byte[2048 * 2048 * 3];
                                    StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                                    int r7h = R7.New();
                                    FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                                    BinaryReader binReader = new BinaryReader(sourceFile);
                                    byte[] programFileBytes = new byte[sourceFile.Length + 1];
                                    for (long il = 0; il < sourceFile.Length; il++)
                                    {
                                        programFileBytes[il] = binReader.ReadByte();
                                    }
                                    programFileBytes[sourceFile.Length] = 0;
                                    sourceFile.Close();
                                    binReader.Close();
                                    int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 0);

                                    StringBuilder jsonStr = new StringBuilder(R7.STRING_SIZE);
                                    if (parameter.variableSn == -1 || parameter.variableSn == 0)
                                    {
                                        MessageBox.Show("Empty json.");
                                    }
                                    else
                                    {
                                        result = R7.GetJsonVariable(r7h, parameter.variableSn, jsonStr, R7.STRING_SIZE);
                                        //MessageBox.Show("result = " + result + "variablesn = " + parameter.variableSn + "json = " + jsonStr);

                                        FormJson formJson = new FormJson();//(1, function, formMain.formFunction, baseSn);
                                                                           //formImage.MdiParent = formMain;
                                        formJson.setText(jsonStr.ToString(), FormMain.r8.variables[parameter.variableSn].name);
                                        formJson.Show();
                                        jsonStr.Clear();
                                    }
                                    result = R7.Release(r7h);
                                }
                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 5://20170412 複數 rect 功能
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "View";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {
                                if (formMain.programFilePath == null)
                                {
                                    //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                    //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                    //加個警示訊息
                                    MessageBox.Show("Please save program before run it.");
                                    return;
                                }
                                //20170202 leo : Run 時自動 save
                                //formMain.saveToolStripMenuItem_Click(sender, e);
                                formMain.saveTempFile();

                                //20170206 leo: 新版換成直接呼叫 dll
                                //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
                                if (formMain.tempProgramFilePath != null)
                                {
                                    //StringBuilder resultBuffer = new StringBuilder(R7.RESULT_SIZE);
                                    // byte[] resultBuffer = new byte[2048 * 2048 * 3];
                                    StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                                    int r7h = R7.New();
                                    FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                                    BinaryReader binReader = new BinaryReader(sourceFile);
                                    byte[] programFileBytes = new byte[sourceFile.Length + 1];
                                    for (long il = 0; il < sourceFile.Length; il++)
                                    {
                                        programFileBytes[il] = binReader.ReadByte();
                                    }
                                    programFileBytes[sourceFile.Length] = 0;
                                    sourceFile.Close();
                                    binReader.Close();
                                    //int result = R7.ImageDisplay(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, function.parameters.ElementAt(0).variableSn, resultBuffer, R7.RESULT_SIZE);
                                    int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 0);
                                    int imageW = 0, imageH = 0, imageCh = 0;
                                    R7.ImageGetInfo(r7h, function.parameters.ElementAt(0).variableSn, ref imageW, ref imageH, ref imageCh);
                                    //MessageBox.Show("imageW = " + imageW + " imageH = " + imageH + " imageCh = " + imageCh);
                                    if (imageW == 0 || imageH == 0 || imageCh == 0)
                                    {
                                        MessageBox.Show("Empty image.");
                                    }
                                    else
                                    {
                                        int imageSize = imageW * imageH * imageCh;
                                        byte[] byteBuffer = new byte[imageSize];
                                        int res = R7.ImageGet(r7h, function.parameters.ElementAt(0).variableSn, byteBuffer, imageSize);
                                        //MessageBox.Show("RET: " + ret);
                                        Image image = R8.ByteArrayToImage(byteBuffer, byteBuffer.Length, imageW, imageH, imageCh);


                                        //如果 variable 未設置則設 variable
                                        int baseSn = function.parameters.IndexOf(parameter);
                                        if (function.parameters.ElementAt(baseSn).variableSn == -1 || function.parameters.ElementAt(baseSn).variableSn == 0)
                                        {
                                            formMain.formFunction.PanelParameterList.ElementAt(baseSn).addVariable(formMain.formFunction.PanelParameterList.ElementAt(baseSn).parameter.name, "");
                                        }

                                        FormImage formImage = new FormImage(2, function, formMain.formFunction, baseSn);
                                        //formImage.MdiParent = formMain;
                                        formImage.setImage(image, FormMain.r8.variables[function.parameters.ElementAt(0).variableSn].name);
                                        formImage.Show();
                                        R8.AllFormImageBringToFront(formImage);
                                    }
                                    result = R7.Release(r7h);
                                }

                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 6:// FOLDER_PATH
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "...";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {
                                if (R8.formMain.programFilePath == null)
                                {
                                    //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                    //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                    //加個警示訊息
                                    MessageBox.Show("Please save program before choose file.");
                                    return;
                                }
                                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                                
                                folderBrowserDialog.SelectedPath = FormMain.workSpacePath;

                                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                {
                                    String subString = null;
                                    subString = R8.GetRelativePath(FormMain.workSpacePath + "\\", folderBrowserDialog.SelectedPath + "\\");
                                    if (parameter.variableSn == -1 || parameter.variableSn == 0)
                                    {
                                        addVariable(parameter.name, subString);
                                    }
                                    else
                                    {
                                        Variable variable = FormMain.r8.variables[parameter.variableSn];
                                        variable.value = subString;
                                        editVariable(variable);
                                    }
                                }
                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 7://rotate
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "View";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {

                                if (formMain.programFilePath == null)
                                {
                                    //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                    //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                    //加個警示訊息
                                    MessageBox.Show("Please save program before run it.");
                                    return;
                                }
                                //20170202 leo : Run 時自動 save
                                //formMain.saveToolStripMenuItem_Click(sender, e);
                                formMain.saveTempFile();

                                //20170206 leo: 新版換成直接呼叫 dll
                                //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
                                if (formMain.tempProgramFilePath != null)
                                {
                                    //StringBuilder resultBuffer = new StringBuilder(R7.RESULT_SIZE);
                                    // byte[] resultBuffer = new byte[2048 * 2048 * 3];
                                    StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                                    int r7h = R7.New();
                                    FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                                    BinaryReader binReader = new BinaryReader(sourceFile);
                                    byte[] programFileBytes = new byte[sourceFile.Length + 1];
                                    for (long il = 0; il < sourceFile.Length; il++)
                                    {
                                        programFileBytes[il] = binReader.ReadByte();
                                    }
                                    programFileBytes[sourceFile.Length] = 0;
                                    sourceFile.Close();
                                    binReader.Close();
                                    //int result = R7.ImageDisplay(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, function.parameters.ElementAt(0).variableSn, resultBuffer, R7.RESULT_SIZE);
                                    int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 0);
                                    int imageW = 0, imageH = 0, imageCh = 0;
                                    R7.ImageGetInfo(r7h, function.parameters.ElementAt(0).variableSn, ref imageW, ref imageH, ref imageCh);
                                    //MessageBox.Show("imageW = " + imageW + " imageH = " + imageH + " imageCh = " + imageCh);
                                    if (imageW == 0 || imageH == 0 || imageCh == 0)
                                    {
                                        MessageBox.Show("Empty image.");
                                    }
                                    else
                                    {
                                        int imageSize = imageW * imageH * imageCh;
                                        byte[] byteBuffer = new byte[imageSize];
                                        int res = R7.ImageGet(r7h, function.parameters.ElementAt(0).variableSn, byteBuffer, imageSize);
                                        //MessageBox.Show("RET: " + ret);
                                        Image image = R8.ByteArrayToImage(byteBuffer, byteBuffer.Length, imageW, imageH, imageCh);
                                        
                                        int baseSn = function.parameters.IndexOf(parameter);
                                        //這邊如果 Degree 未設置則設置
                                        if (function.parameters.ElementAt(baseSn).variableSn == -1 || function.parameters.ElementAt(baseSn).variableSn == 0) {
                                            formMain.formFunction.PanelParameterList.ElementAt(baseSn).addVariable(formMain.formFunction.PanelParameterList.ElementAt(baseSn).parameter.name, "0");
                                        }

                                        FormImage formImage = new FormImage(3, function, formMain.formFunction, baseSn);
                                        //formImage.MdiParent = formMain;
                                        formImage.setImage(image, FormMain.r8.variables[function.parameters.ElementAt(0).variableSn].name);
                                        formImage.Show();
                                        R8.AllFormImageBringToFront(formImage);
                                    }
                                    result = R7.Release(r7h);
                                }

                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                    case 8://rotate and select rect
                        {
                            extraButton = new Button();
                            extraButton.Width = 40;
                            extraButton.Text = "View";
                            extraButton.Location = new Point(newButton.Location.X + newButton.Width + 10, 0);
                            extraButton.Click += delegate (object sender, EventArgs e)
                            {

                                if (formMain.programFilePath == null)
                                {
                                    //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                                    //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                                    //加個警示訊息
                                    MessageBox.Show("Please save program before run it.");
                                    return;
                                }
                                //20170202 leo : Run 時自動 save
                                //formMain.saveToolStripMenuItem_Click(sender, e);
                                formMain.saveTempFile();

                                //20170206 leo: 新版換成直接呼叫 dll
                                //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
                                if (formMain.tempProgramFilePath != null)
                                {
                                    //StringBuilder resultBuffer = new StringBuilder(R7.RESULT_SIZE);
                                    // byte[] resultBuffer = new byte[2048 * 2048 * 3];
                                    StringBuilder workSpacePathBuffer = new StringBuilder(FormMain.workSpacePath + "\\", R7.STRING_SIZE);
                                    int r7h = R7.New();
                                    FileStream sourceFile = new FileStream(formMain.tempProgramFilePath, FileMode.Open);
                                    BinaryReader binReader = new BinaryReader(sourceFile);
                                    byte[] programFileBytes = new byte[sourceFile.Length + 1];
                                    for (long il = 0; il < sourceFile.Length; il++)
                                    {
                                        programFileBytes[il] = binReader.ReadByte();
                                    }
                                    programFileBytes[sourceFile.Length] = 0;
                                    sourceFile.Close();
                                    binReader.Close();
                                    //int result = R7.ImageDisplay(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, function.parameters.ElementAt(0).variableSn, resultBuffer, R7.RESULT_SIZE);
                                    int result = R7.RunToTargetFunction(r7h, programFileBytes, new Byte[] { 0 }, FormMain.GetBytes(workSpacePathBuffer.ToString()), formMain.formFunction.function.sn, 0);
                                    int imageW = 0, imageH = 0, imageCh = 0;
                                    R7.ImageGetInfo(r7h, function.parameters.ElementAt(0).variableSn, ref imageW, ref imageH, ref imageCh);
                                    //MessageBox.Show("imageW = " + imageW + " imageH = " + imageH + " imageCh = " + imageCh);
                                    if (imageW == 0 || imageH == 0 || imageCh == 0)
                                    {
                                        MessageBox.Show("Empty image.");
                                    }
                                    else
                                    {
                                        int imageSize = imageW * imageH * imageCh;
                                        byte[] byteBuffer = new byte[imageSize];
                                        int res = R7.ImageGet(r7h, function.parameters.ElementAt(0).variableSn, byteBuffer, imageSize);
                                        //MessageBox.Show("RET: " + ret);
                                        Image image = R8.ByteArrayToImage(byteBuffer, byteBuffer.Length, imageW, imageH, imageCh);

                                        int baseSn = function.parameters.IndexOf(parameter);
                                        //這邊如果 Degree 未設置則設置
                                        if (function.parameters.ElementAt(baseSn).variableSn == -1 || function.parameters.ElementAt(baseSn).variableSn == 0)
                                        {
                                            formMain.formFunction.PanelParameterList.ElementAt(baseSn).addVariable(formMain.formFunction.PanelParameterList.ElementAt(baseSn).parameter.name, "0");
                                        }

                                        //在這邊，如果 x y w h 未設置則設 variable
                                        int[] defaultSize = { 0, 0, imageW / 2, imageH / 2 };
                                        for (i = 1; i < 5; i++) //( 0 是 Degree ，所以這邊是 1 ~ 5)
                                        {
                                            if (function.parameters.ElementAt(baseSn + i).variableSn == -1 || function.parameters.ElementAt(baseSn + i).variableSn == 0)
                                            {
                                                //function.parameters.ElementAt(baseSn + i).addVariable("");
                                                //MessageBox.Show("formMain.formFunction.PanelParameterList.ElementAt(baseSn + " + i + ").Name = " + formMain.formFunction.PanelParameterList.ElementAt(baseSn + i).parameter.name);
                                                formMain.formFunction.PanelParameterList.ElementAt(baseSn + i).addVariable(formMain.formFunction.PanelParameterList.ElementAt(baseSn + i).parameter.name, "" + defaultSize[i - 1]);
                                            }
                                        }

                                        FormImage formImage = new FormImage(4, function, formMain.formFunction, baseSn);
                                        //formImage.MdiParent = formMain;
                                        formImage.setImage(image, FormMain.r8.variables[function.parameters.ElementAt(0).variableSn].name);
                                        formImage.Show();

                                        R8.AllFormImageBringToFront(formImage);

                                    }
                                    result = R7.Release(r7h);
                                }

                                return;
                            };
                            this.Controls.Add(extraButton);
                        }
                        break;
                }
                
                return;
            }


            public void resetPanel(String searchStr)
            {
                
                //System.Console.WriteLine("resetPanel");
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, parameter.type);
                nameLabel.Text = parameter.name + " (" + str.ToString() + ")";
                if (parameter.option.Contains("INOUT")) {
                    nameLabel.ForeColor = Color.Green;
                } else if (parameter.option.Contains("OUT"))
                {
                    nameLabel.ForeColor = Color.Blue;
                }
                FormFunction.toolTip.SetToolTip(nameLabel, nameLabel.Text.ToString());
                str.Clear();


                int maxSize = 0;
                variables = FormMain.r8.getVariableArrayByType(parameter.type, true, ref maxSize, searchStr);

                //variables = FormMain.r8.getVariableArrayByType(parameter.type, true);
                typeComboBox.Items.Clear();
                typeComboBox.Items.AddRange(variables.ToArray());
                //if (parameter.variableSn != -1)
                //{
                //20171031 leo: 有 search 模式時，要另外處理[若已選取的目標，不在搜尋到的陣列中時，還是額外把它填入 typeComboBox]

                bool isSeartched = false;
                for (int i = 0; i < variables.Count; i++)
                {
                    if (parameter.variableSn == variables[i].sn)
                    {
                        //SelectedIndexChanged 會觸發 editValue ，所以在這邊要先取消
                        typeComboBox.SelectedIndexChanged -= typeComboBox_SelectedIndexChanged;
                        typeComboBox.SelectedIndex = i;
                        typeComboBox.SelectedIndexChanged += typeComboBox_SelectedIndexChanged;
                        isSeartched = true;
                        break;
                    }
                }
                //}

                if (!isSeartched && parameter.variableSn > 0)
                {
                    if (FormMain.r8.variables[parameter.variableSn] != null)
                    {
                        Variable tempV = FormMain.r8.variables[parameter.variableSn].clone();
                        variables.Add(tempV);
                        typeComboBox.Items.Add(tempV);
                        typeComboBox.SelectedIndexChanged -= typeComboBox_SelectedIndexChanged;
                        typeComboBox.SelectedIndex = typeComboBox.Items.Count - 1;
                        typeComboBox.SelectedIndexChanged += typeComboBox_SelectedIndexChanged;
                        
                    }
                }
                else {
                   // MessageBox.Show("FIND " + parameter.name + ", " + parameter.variableSn);
                }


                // DropDownList 不好加 ToolTip，直接把它框框變寬比較快
                typeComboBox.DropDownWidth = 160;
                if (maxSize * 8 > 160)
                {
                    typeComboBox.DropDownWidth = maxSize * 8;
                }
                return;
            }

            public void editValue()
            {
                if (typeComboBox.SelectedIndex == -1) {
                    return;
                }
                if (variables[typeComboBox.SelectedIndex] == null) {
                    return;
                }
                if (variables[typeComboBox.SelectedIndex].sn != -1 && variables[typeComboBox.SelectedIndex].sn != 0)
                {
                    parameter.variableSn = variables[typeComboBox.SelectedIndex].sn;
                    //System.Console.WriteLine("editValue variableSn " + parameter.variableSn);
                    formMain.formVariable.showVariable(parameter.variableSn);
                    formMain.formVariables.showVariables(formMain.formFunction.function);
                }
                return;
            }
            
            private void typeComboBox_SelectedIndexChanged(object sender, EventArgs e)
            {
                //System.Console.WriteLine("typeComboBox_SelectedIndexChanged");
                if (typeComboBox.SelectedIndex == -1)
                {
                    return;
                }
                if (variables[typeComboBox.SelectedIndex] == null)
                {
                    return;
                }
                //20170314 leo: 改為允許取消選取 variable
                //if (variables[typeComboBox.SelectedIndex].sn != -1)
                //{
                    parameter.variableSn = variables[typeComboBox.SelectedIndex].sn;
                //}
                formMain.formVariables.showVariables(formMain.formFunction.function);
                //20170330 [在 Function 視窗選擇變數後，背景幫忙按 Edit ，將 focus 切到 Variable 視窗。]
                //實際使用時，如果編輯比較大型的 program 會不方便....加個切換開關
                if (isFocusVariableAfterCheckBoxChange)
                {
                    editValue();
                    formMain.formVariable.focusToValueBox();
                }
                Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                //Program.recordProgram(FormMain.r8);
            }

            private void editButton_Click(object sender, EventArgs e)
            {
                System.Console.WriteLine("editButton_Click");
                editValue();
                //20170124 leo: Edit點擊後， focus 到 formVariable 的 name 編輯欄位
                //formMain.formVariable.focusToTextBoxName();
                //20170327 leo: 依今天信件改為 [選到 variable 時，自動 focus 到 Value]。
                formMain.formVariable.focusToValueBox();
                return;
            }

            public Variable addVariable(string name, string value) {
                //MessageBox.Show("addVariable" + name + " _ " +value);
                Variable variable = new Variable();
                FormMain.r8.addVariable(variable);
                //20170327 New: Variable 123 -> String 123。
                //variable.name = "Variable" + variable.sn; //由於 addVariable 後才會分配 variableSn ，要顯示 sn 時，加在 addVariable 後顯示才正確
                variable.type = (int)(parameter.type);
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                if (value == null)
                {
                    R8.GetVariableType(str, R7.STRING_SIZE, variable.type);
                    switch (str.ToString())
                    {
                        case "bool":
                        case "Bool":
                        case "boolean":
                        case "Boolean":
                            //variable.name = "bool" + variable.sn;
                            variable.value = "0";
                            break;
                        case "double":
                        case "Double":
                            //variable.name = "double" + variable.sn;
                            variable.value = "0";
                            break;
                        case "float":
                        case "Float":
                            //variable.name = "float" + variable.sn;
                            variable.value = "0";
                            break;
                        case "image":
                        case "Image":
                        case "mat":
                        case "Mat":
                            //variable.name = "image" + variable.sn;
                            variable.value = "";
                            break;
                        case "int":
                        case "Int":
                            //variable.name = "int" + variable.sn;
                            variable.value = "0";
                            break;
                        case "string":
                        case "String":
                        case "str":
                        case "Str":
                            //variable.name = "string" + variable.sn;
                            variable.value = "";
                            break;
                        case "json":
                        case "Json":
                            //variable.name = "json" + variable.sn;
                            variable.value = "";
                            break;
                    }
                    str.Clear();
                }
                else {
                    R8.GetVariableType(str, R7.STRING_SIZE, variable.type);
                    switch (str.ToString())
                    {
                        case "bool":
                        case "Bool":
                        case "boolean":
                        case "Boolean":
                            //variable.name = "bool" + variable.sn;
                            break;
                        case "double":
                        case "Double":
                            //variable.name = "double" + variable.sn;
                            break;
                        case "float":
                        case "Float":
                            //variable.name = "float" + variable.sn;
                            break;
                        case "image":
                        case "Image":
                        case "mat":
                        case "Mat":
                            //variable.name = "image" + variable.sn;
                            break;
                        case "int":
                        case "Int":
                            //variable.name = "int" + variable.sn;
                            break;
                        case "string":
                        case "String":
                        case "str":
                        case "Str":
                            //variable.name = "string" + variable.sn;
                            break;
                        case "json":
                        case "Json":
                            //variable.name = "json" + variable.sn;
                            break;
                    }
                    str.Clear();
                    variable.value = value;
                }
                //20170612 variable name 的預設值修改：依照 parameter 命名
                if (name != null)
                {
                    //20170705 leo: 0602 的修改會產生 [重複名稱]，所以這邊要加一段檢查
                    Variable[] variables = FormMain.r8.getVariableArray();
                    int i;
                    bool isSameName = true;
                    String tempString = name;
                    int nameNumberCount = 0;
                    //MessageBox.Show("pre name = " + name);
                    while (isSameName)
                    {
                        isSameName = false;
                        if (nameNumberCount == 0)
                        {
                            tempString = name;
                        }
                        else
                        {
                            tempString = name + "" + nameNumberCount;
                        }
                        for (i = 0; i < variables.Count(); i++)
                        {
                            if (variables[i].name.Equals(tempString))
                            {
                                if (variables[i].sn != variable.sn)
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
                    }//20170705 add end
                    //MessageBox.Show("after name = " + tempString);
                    variable.name = tempString;
                }
                else {
                    variable.name = "Variable" + variable.sn;
                }

                //formMain.formFunction.function.inputVariableSns.Add(variable.sn);
                parameter.variableSn = variable.sn;
                formMain.formVariable.showVariable(variable.sn);
                formMain.formVariables.showVariables(formMain.formFunction.function);
                formMain.formFunction.showFunction(formMain.formFunction.function);
                //formMain.formFunction.resetPanels();
                //20170124 leo: Edit點擊後， focus 到 formVariable 的 name 編輯欄位
                formMain.formVariable.focusToTextBoxName();
                Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                //Program.recordProgram(FormMain.r8);
                return variable;
            }

            public void editVariable(Variable variable) {
                formMain.formVariable.showVariable(variable.sn);
                formMain.formVariables.showVariables(formMain.formFunction.function);
                formMain.formFunction.showFunction(formMain.formFunction.function);
                //Program.recordProgram(FormMain.r8);
                return;
            }

            private void newButton_Click(object sender, EventArgs e)
            {
                addVariable(this.parameter.name, null);
                //這邊不用 Program.recordProgram(FormMain.r8); 因為加在 addVariable 裡面
                return;
            }


        }

        private void FormFunction_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formFunction = null;
        }
    }
}
