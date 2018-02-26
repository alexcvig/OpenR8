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

    
    public partial class FormLibrary : Form
    {
        private string str_Libraries = "Libraries";

        public static Dictionary<string, string> functionURL = new Dictionary<string, string>();
        private List<Button> buttons = new List<Button>();
        private List<string> functionsURL = new List<string>();
        //Label
        private List<Label> labels = new List<Label>();
        const int buttonStartY = 0;
        int buttonsLocationY = buttonStartY;
        private FormMain formMain;
        private int allFunctionCount = 0;

        bool isInSearchMode = false;

        //20170817 leo: 發現有時候在 FormFunctions 拉 function 順序時，會話畫面捲動導致 function 被誤拖曳，
        //原因為 Form 在 onFocus 時會自動捲動到 [該 Form 中，前一個被選取物件] 的位置。
        //參照 https://stackoverflow.com/questions/419774/how-can-you-stop-a-winforms-panel-from-scrolling/912610#912610
        //用 override 把該功能關掉
        //然後其他類似結構的地方也加上這一段
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }

        private R8.PanelNoScrollOnFocus panel2;

        public FormLibrary()
        {
            panel2 = new R8.PanelNoScrollOnFocus();
            this.panel2.AutoScroll = true;
            this.panel2.Location = new System.Drawing.Point(0, 50);
            this.panel2.Name = "panel2";
            this.panel2.TabIndex = 1;
            this.Controls.Add(this.panel2);
            InitializeComponent();

            str_Libraries = R8.TranslationString(str_Libraries);
            this.Text = str_Libraries;
        }

        private void FormToolBox_Load(object sender, EventArgs e)
        {
            formMain = (FormMain)this.MdiParent;
            FormLibrary_SizeChanged(sender, e);
            loadButtons();
            this.Text = str_Libraries + "  " + buttons.Count + " of " + allFunctionCount;
        }

        private void loadButtons()
        {
            allFunctionCount = 0;
            //addButton("OpenImage", "OpenImage");
            //addButton("SaveImage", "SaveImage");
            //addButton("DebugImage", "DebugImage");
            //addButton("Binarize", "Binarize");
            //20170123 leo : 改為從 function list 的 json 檔案 動態產生

            buttonsLocationY = buttonStartY;

            int functionGroupNum = 0;
            int functionNum = 0;
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            StringBuilder str2 = new StringBuilder(R7.STRING_SIZE);
            functionGroupNum = R8.GetFunctionGroupNum();

            //20170124 leo: 加入 BrainVersion
            R7.GetVersion(str, R7.STRING_SIZE);
            //addLabel("version:" + str.ToString());
            buttonsLocationY += 5;

            functionURL.Clear();
            for (int i = 0; i < functionGroupNum; i++)
            {
                R8.GetFunctionGroupName(str, R7.STRING_SIZE, i);
                addLabel(str.ToString());//GroupName
                functionNum = R8.GetFunctionNumInGroup(i);
                for (int j = 0; j < functionNum; j++)
                {
                    allFunctionCount++;
                    R8.GetFunctionName(str, R7.STRING_SIZE, i, j);
                    R8.GetFunctionDoc(str2, R7.STRING_SIZE, i, j);
                    //System.Console.WriteLine("getURL:" + str2);
                    addButton(str.ToString(), str2.ToString());

                    //20170603 leo: 增加 "如果 function 有說明 URL ，在右方 function 小視窗中的右下方顯示 help 按鈕。"
                    //所以有url 連結的 name 要存起來
                    functionURL.Add(str.ToString(), str2.ToString());
                }
            }
            str.Clear();
            str2.Clear();
            return;
        }

        private void addLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            label.Width = 155;
            label.Location = new Point(0, buttonsLocationY);
            buttonsLocationY += label.Height + 5;
            panel2.Controls.Add(label);
            labels.Add(label);

        }

        private void addButton(string text, string url)
        {
            Button button = new Button();

            button.Tag = (int)(buttons.Count);
            //button.Name = name;
            button.Text = text;
            button.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            button.Width = this.Width - 35;
            button.Height = 40;
            button.Location = new Point(0, buttonsLocationY);
            buttonsLocationY += button.Height + 5;
           // button.AllowDrop = true;
            button.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttons_MouseDown);
            //button.MouseMove += new System.Windows.Forms.MouseEventHandler(this.buttons_MouseMove);
            //button.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttons_MouseUp);
            // button.MouseLeave += new System.EventHandler(this.buttons_MouseLeave);
            panel2.Controls.Add(button);

            if (url.Length > 0) {
                ToolTip toolTip = new ToolTip();
                toolTip.SetToolTip(button, "Right click for help.");
                //toolTip.SetToolTip(button, "Right click to view description: \n" + url);
            }

            buttons.Add(button);
            functionsURL.Add(url);
        }

        

      //  private long preClickTime = 0;
      //  private long nowClickTime = 0;
      //  long doubleClickTimer = 3000000;


        private void buttons_MouseDown(object sender, MouseEventArgs e)
        {
            //20170308 leo: 增加[右鍵連結到 OpenRobot 網站，對應的 Api 說明頁面]
            if (e.Button == MouseButtons.Right)
            {
                //R8.logToLogBox("URL: " + functionsURL.ElementAt((int)(((Button)sender).Tag)));
                string url = functionsURL.ElementAt((int)(((Button)sender).Tag));
                if (url.Length > 0)
                {
                   // System.Diagnostics.Process.Start("C:\\Program Files\\Internet Explorer\\IExplore.exe", url);
                   System.Diagnostics.Process.Start(url);
                }
                else {
                    //R8.logToLogBox("no doc file");
                }
                return;
            }


            //R8.logToLogBox("buttons_MouseDown " + ((Button)sender).Text + e.Button);
            //R8.logToLogBox("buttons_MouseDown " + ((Button)sender).Text + e.Button + " C = " + e.Clicks);
            //20170125 改為只有右鍵會觸發拖曳
            /*
            if (e.Button == MouseButtons.Right)
            {
                (sender as Button).DoDragDrop((sender as Button).Text, DragDropEffects.Copy);
            }
            else
            {
                //20170125 根據 https://msdn.microsoft.com/zh-tw/library/ms171543(v=vs.110).aspx 滑鼠雙擊，用 Timer 做
                //但是 button數量不固定，用同一個 Timer 會出事
                //改用 DateTime
                nowClickTime = DateTime.Now.Ticks;
                //System.Console.WriteLine("Time = " + nowClickTime);
                System.Console.WriteLine("Time Diff = " + (nowClickTime - preClickTime));
                if (nowClickTime - preClickTime < doubleClickTimer) {

                    formMain.formFunctions.addButtonAtLast((sender as Button).Text);
                }
                preClickTime = DateTime.Now.Ticks;
            }
            */
            //20170202 通通合併到左鍵
            //            nowClickTime = DateTime.Now.Ticks;
            //System.Console.WriteLine("Time = " + nowClickTime);
            //          System.Console.WriteLine("Time Diff = " + (nowClickTime - preClickTime));
            if (e.Clicks == 2)
            {
                formMain.formFunctions.addButtonAtLast((sender as Button).Text);
                Program.recordProgram(FormMain.r8, formMain.toolStripMenuItemUndo, formMain.toolStripMenuItemRedo);
                //Program.recordProgram(FormMain.r8);
            }
            else {
                (sender as Button).DoDragDrop((sender as Button).Text, DragDropEffects.Copy);

            }
        //    preClickTime = DateTime.Now.Ticks;


            return;
        }

        public void resetButton()
        {
            foreach (Button b in buttons)
            {
                b.Dispose();
            }
            buttons.Clear();
            buttonsLocationY = buttonStartY;

            //labels

            foreach (Label l in labels)
            {
                l.Dispose();
            }
            labels.Clear();

            //urls
            functionsURL.Clear();

            int functionGroupNum = 0;
            int functionNum = 0;
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            StringBuilder str2 = new StringBuilder(R7.STRING_SIZE);
            functionGroupNum = R8.GetFunctionGroupNum();

            //20170124 leo: 加入 BrainVersion
            R7.GetVersion(str, R7.STRING_SIZE);
            //addLabel("version:" + str.ToString());
            buttonsLocationY += 5;

            for (int i = 0; i < functionGroupNum; i++)
            {
                R8.GetFunctionGroupName(str, R7.STRING_SIZE, i);
                addLabel(str.ToString());//GroupName
                functionNum = R8.GetFunctionNumInGroup(i);
                for (int j = 0; j < functionNum; j++)
                {
                    R8.GetFunctionName(str, R7.STRING_SIZE, i, j);
                    R8.GetFunctionDoc(str2, R7.STRING_SIZE, i, j);
                    //System.Console.WriteLine("getURL:" + str2);
                    if (isInSearchMode)
                    {
                        if (str.ToString().ToLower().Contains(textBoxSearch.Text.ToLower())) {
                            addButton(str.ToString(), str2.ToString());
                        }
                    }
                    else
                    {
                        addButton(str.ToString(), str2.ToString());
                    }
                }
            }
            str.Clear();
            str2.Clear();
            this.Text = str_Libraries + "  " + buttons.Count + " of " + allFunctionCount;
            return;
        }

        public void clearSearchBox() {
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
        
        private void pictureBoxSearch_Click(object sender, EventArgs e)
        {
            isInSearchMode = true;
            resetButton();
            return;
        }

        private void FormToolBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormMain formMain = this.MdiParent as FormMain;
            formMain.formLibrary = null;
        }


        private void FormLibrary_SizeChanged(object sender, EventArgs e)
        {

            panel2.Height = this.ClientSize.Height - panel2.Location.Y;
            panel2.Width = this.ClientSize.Width;
        }

        /*
        private void buttons_MouseMove(object sender, MouseEventArgs e)
        {
           // R8.logToLogBox("" + e.Button);
            return;
        }

        private void buttons_MouseUp(object sender, MouseEventArgs e)
        {
            R8.addLogToLogBox("buttons_MouseUp " + ((Button)sender).Text + e.Button);
            return;
        }


        private void buttons_MouseLeave(object sender, EventArgs e)
        {
            R8.addLogToLogBox("buttons_MouseLeave" + ((Button)sender).Text);
            return;
        }
        */
        /*
        private void buttons_DoubleClick(object sender, EventArgs e)
        {
            formMain.formFunctions.addButtonAtLast((sender as Button).Text);
            return;
        }
        */
    }
}
