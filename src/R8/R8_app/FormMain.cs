using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Resources;
using System.IO;
using System.IO.Ports;
using System.Drawing;
using System.Xml.Linq;


// TODO: Open File -> Clear log.

/*
 * TODO: 錯誤代碼
 *
 *
 */

namespace R8
{
    public partial class FormMain : Form
    {
        #region SUB_FORMS
        public FormLibrary formLibrary = null;
        public FormFunctions formFunctions = null;
        public FormVariables formVariables = null;
        public FormFunction formFunction = null;
        public FormVariable formVariable = null;
        public FormLog formLog = null;
        public FormRegister formRegister = null;
        public FormAbout formAbout = null;
        public FormLibraryList formLibraryList = null;
        public FormLogin formLogin = null;
        // public FormPreviewImage formPreviewImage = null;
        #endregion

        public static R8 r8 = new R8();
        private int formDistance = 5;
        //20171005 leo: 依討論把 OpenR8 字眼全都改成 R8 。(不確定有沒有露改，總之有看到的都改掉了)
        //20171005 Annie 回報 FormImage 在圖為一定大小時，rects 框框會不能拉動。已修正。
        //20171005 項目[Export bat file 後，會「立刻執行一次」，請移除「立刻執行一次」。]

        //20171005 項目[由於影像通常不是正的，框選 ROI 及 View Image 視窗上方加上輸入旋轉度數的輸入框]
        // 話說就算在 R8 的介面上圖依該角度旋轉了， R7 運行時仍然會沒有旋轉(因為沒對應的參數紀錄並傳過去)
        // 總之先想辦法加入此功能，然後塞給 ImageRotate Function 。


        //20170126 leo: 準備整包可以測試的，預設路徑從System.Windows.Forms.Application.LocalUserAppDataPath 換 System.Windows.Forms.Application.StartupPath
        //System.Windows.Forms.Application.ExecutablePath 呼叫路徑
        //System.Windows.Forms.Application.StartupPath 此 exe 的路徑
        //20170929 leo: 準備增加 undo redo 功能。需要
        //1. 記錄每一步的動作，介面操作時觸發。[增加 function 、移動 function 、刪除 function 、
        //   增加 variable 、修改 variable 、更換 function 中的 variable  、刪除 variable ]
        //   時，都要記錄動作。

        //2. 開啟新檔案、開啟舊檔時，要把紀錄的動作清空。
        //3. 要準備讀取與存入狀態的 function ，存入狀態就開一個 List 把 R8 copy 一份塞進去，讀取狀態從 openToolStripMenuItem_Click 改
        //依昨天討論結果，紀錄動作方式為整個 R8 物件記錄下來(也就是所有 function 與 variable)，共紀錄10個

        //20170930 依昨天討論， Undo 與 Redo 按鈕在不能按的時候讓它灰掉。
        //所以 [除非有按 Undo ，不然 Redo 一直保持灰色](undoRedoNowAt 為0時灰色)
        //另外 [ recordList 大於1， Undo 才能按]
        //[ undoRedoNowAt 等於 recordList.size - 1， Undo 不能按]
        //需要判斷的時間點有：程式開啟後、 Open New 與 Undo Redo 按鈕按下後

        //20171016 1.項目 Image Form 上方順序改成 Save - + Rotate Degree Set Close ，
        //由於 Rotate 是用 NumericUpDown 不屬於 ToolStripMenuItem ，無法與其它項目放一起，
        //目前先把順序改為 Save - + Set Close Rotate Degree
        //2. 文字改成 Export to Windows bat file / Export Visual C++ file
        //3. Kelly 回報有 bug [長方形的圖按 View 會變形] ，已修復。
        //4. [點擊 function block 兩下，執行 debug run 到該 function。]
        //5. Export C++ 時格式更動，目前不傳 Sn 與 posY 了，改為自動產生。
        //6. [data 往下，跳出 R8 ，不應回到上面].....測不出此現象？
        //7. [每次存檔都用排序存檔，存檔完畢後，自動 reload]
        //8. doing [R8 變數名稱限制只能輸入英文數字及底線].....所以代入[預設 variable name] 時，也要檢查一輪....?

        //20170930 依昨天討論，要增加[Library 切換開關功能]。
        //目前只做[固定位置的 Library 資料夾內的切換開關]，不做[相對 .r6 檔案的切換] 

        //20171103 項目[Program Functions 跟 Program Variables 無法連續按 Backspace 或 Del 連續進行刪除]
        // 項目 [Undo: Control + z, Redo: Control + Shift + z]
        //20171114 項目[第一次開啟檔案時，檔案對話框路徑請預設為 OpenR8/workspace ]
        //public static string workSpacePath = System.Windows.Forms.Application.StartupPath;
        public static string workSpacePath = System.Windows.Forms.Application.StartupPath + "\\workspace";
        public string programFilePath = null;
        private string formMainTitle;
        private string str_Community = "Community Edition";
        private string str_Slogan = "AI Tool for Everyone";
        private string str_Version = "Version";
        private string defaultFileName = "program.r6";//20170328 預設名稱要改小寫

        //20170906 leo: 依之前討論 title 改為不顯示全路徑，只顯示檔名
        public string programFileName = null;

        //20171024 按下 Control + S 即可存檔，按下 Control + O 即可開檔，按下 Control + N 即可開新檔
        //設置快捷鍵方式參照 http://www.360doc.com/content/11/0831/21/5482098_144851296.shtml
        //用 ShortcutKey 方式加入

        //20171024 項目[R8 Login Form to get 20 days license from OpenRobot.Club]
        //先拉一個 FormLogin 介面出來。
        //之後還會需要[OpenRobot.Club php 端增加 license key 產生程式]與 [R8 增加 license key 解碼程式]
        //然後還需要讀 MAC Address

        //20171025 找了個讀 MAC Address 的方法 https://stackoverflow.com/questions/2069855/getting-machines-mac-address-good-solution ，加入 R7
        //然後項目優先順序更動， [R8 Login Form to get 20 days license from OpenRobot.Club] 暫緩

        //20171025 項目[Function block 前面顯示行號]
        //要顯示行號而不是顯示 Function Sn ，行號的數字不適合放在 function button 裡面(不然拖曳一個 function block 更換位置會造成好幾個 function block 都要重設 text)
        //因此設置為每個 function button 左邊加標籤

        //20171101 討論後 Save 功能改回不 sort ，只有 Save as 才 sort

        //20171114 項目[另存新檔自動帶入檔名]

        //20171115 項目[R8 內部運算時改用 .r6t 的暫存檔，不要背景覆寫 .r6 ，關閉 r6 檔案自動刪除 .r6t]
        // .r6t 的產生時間點....基本上從以下兩個 function 查連結應該可以涵蓋全部
        //R8.writeProgramXml(programFilePath); saveToolStripMenuItem_Click(object sender, EventArgs e)

        //然後是[砍檔案]的時間點，關閉 r6 檔案的情況有
        //a. 關閉 R8 
        //b. 開啟新檔案
        // 然後模糊地帶的有
        //a. 另存新檔(可以解釋為關閉檔案A並開啟一個不同名稱相同內容的檔案B)
        // 總之先設置成[只要 programFilePath 更換]就砍 .r6t 。 以及[FormMain關閉時]也砍。


        //20171117 leo:項目[關閉 R8 時，如果有修改內容卻沒存檔，請顯示 message box ： This program is not saved. Are you sure to exit? (No/Yes)]
        //需要知道存檔後是否有改過.....設置一個變數[isFileChange]
        //修改內容會記錄 undo redo ，所以應該可以加在 recordProgram 裡面， isFileChange = true
        //然後存檔成功後，isFileChange = false
        //最後在 FormMain 的 onClose 看 isFileChange 顯示訊息就好
        //-> 有例外條件： 剛開啟 R8 或者 Open 檔案都會觸發 recordProgram 動作...需要在對應動作結束後， isFileChange = false


        public string tempProgramFilePath = null;

        public void deleteTempR6File() {

            if (tempProgramFilePath == null) {
                return;
            }
            if (File.Exists(tempProgramFilePath)) {
                File.Delete(tempProgramFilePath);
            }

            return;
        }

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            
            R8.formMain = this;
            //20170613 leo: 從 Ivy 加在 A15 的 Registry 檢查移植過來
            //20170612: Ivy, checking visual studio 2015 redistribution
            //const string userRoot = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\Installer\\Dependencies\\";
            //const string subkey = "{d992c12e-cab2-426f-bde3-fb8c53950b0d}";
            //const string keyName = userRoot + subkey;
            int res;
            /*
             Microsoft Visual C++ 2015 Redistributable (x64) - 14.0.24215
             Registry Key: HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Installer\Dependencies\{d992c12e-cab2-426f-bde3-fb8c53950b0d}
             Configuration: x64
             Version: 14.0.24215.1 
             */
            //會遇到版本不同時會不能用的問題(例如我這台電腦是 v14.0.24210)
            //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\DevDiv\vc\Servicing\14.0\RuntimeMinimum
            //改用HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\DevDiv\vc\Servicing\14.0\RuntimeMinimum\In‌​stall

            //確認 x64 x86 ，根據 https://stackoverflow.com/questions/5054905/how-to-know-in-run-time-if-im-on-x86-or-x64-mode-in-c-sharp
            //用 IntPtr.Size
            if (IntPtr.Size == 4) //x86
            {
                //x86 會依據作業系統，有兩種位置
                int returedInt = (int)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\DevDiv\\vc\\Servicing\\14.0\\RuntimeMinimum\\", "Install", -1);
                if (returedInt != 1)
                {
                    //returedInt = (int)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\VisualStudio\\14.0\\VC\\Runtimes\\x86\\", "Installed", -1);
                    returedInt = (int)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\DevDiv\\vc\\Servicing\\14.0\\RuntimeMinimum\\", "Install", -1);
                    if (returedInt != 1)
                    {
                        MessageBox.Show("Please install Microsoft Visual C++ 2015 Redistributable Package (x86). (vc_redist.x86.exe)");
                        Application.Exit();
                    }
                }
            }
            else //x64
            {
                int returedInt = (int)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\DevDiv\\vc\\Servicing\\14.0\\RuntimeMinimum\\", "Install", -1);
                if (returedInt != 1)
                {
                    MessageBox.Show("Please install Microsoft Visual C++ 2015 Redistributable Package (x64). (vc_redist.x64.exe)");
                    Application.Exit();
                }
            }


            //string returedStr = (string)Microsoft.Win32.Registry.GetValue(keyName, "DisplayName", null);
            //if (returedStr == null)
            //{
            //    MessageBox.Show("Please install Microsoft Visual C++ 2015 Redistributable Package (x64). (vc_redist.x64.exe)");
            //    Application.Exit();
            //}



            StringBuilder versionSB = new StringBuilder(R7.STRING_SIZE);
            R7.GetVersion(versionSB, R7.STRING_SIZE);
            String version = versionSB.ToString();
            versionSB.Clear();

           


            var p = System.Diagnostics.Process.GetCurrentProcess();
            p.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            this.WindowState = FormWindowState.Maximized;

            res = R7.InitLib();
            if (res <= 0)
            {
                MessageBox.Show("ERROR! R7.InitLib = " + res);
                Application.Exit();
            }

            res = R8.InitLib();
            if (res <= 0)
            {
                MessageBox.Show("ERROR! R8.InitLib = " + res);
                Application.Exit();
            }

            //20180212 leo: config 設定測試
   
            R8.LoadConfigFile(R8.ConfigFilePath);

            StringBuilder sb_key = new StringBuilder("language");
            StringBuilder sb_value = new StringBuilder(R7.STRING_SIZE);

            R8.ReadConfig(sb_key, sb_value);

            string languageMode = sb_value.ToString();

            //20180212 leo: 語系切換功能
            res = R8.InitLanguage(System.Windows.Forms.Application.StartupPath + "\\Language\\" + languageMode + ".txt");
            //MessageBox.Show("R8.InitLanguage = " + res);
            TranslationMenuBar();
            
            /*
            if (false)
            {   //test
                string strValue;
                strValue = R8.TranslationString("File");
                MessageBox.Show("get Value: " + strValue);
            }
            */

            loadLibraryFunctions();

            //R7.LoadLibrary(workSpacePath);//20171110 leo: 項目[開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。]

            //reloadToolBox();

            loadForms();

            R8.setLogBox(formLog.getRichTextBox());
            //R8.logToLogBox("Start : " + res);
            toolStripWorkSpaceTextBox.Text = workSpacePath;
            //this.Close();


            //20170626 log 測試
            //if(false)
            //{
            //R7.OpenLogW("中文Log測試.txt");
            //R7.Log("測試");
            //R7.Log("テスト");
            //R7.CloseLog();
            //}

            str_Community = R8.TranslationString(str_Community);
            str_Slogan = R8.TranslationString(str_Slogan);
            str_Version = R8.TranslationString(str_Version);
            // "OpenR8 Community Edition  -  AI Tool for Everyone  -  Version "
            formMainTitle = "OpenR8 " + str_Community + " - " + str_Slogan + " - " + str_Version + " " + version;
            this.Text = formMainTitle;

            //20170411 leo :從 A3 call R8 會帶 programPath 參數，要把該 program 開起來。
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                //MessageBox.Show("Get Arg: " + args[args.Length - 1]);
                if (File.Exists(args[1]))
                {
                    clearSearchBox();
                    formFunctions.readProgramXml(args[1]);
                    programFilePath = args[1]; //20171115 這邊是剛開啟，不用砍暫存檔
                    workSpacePath = args[1].Substring(0, args[1].LastIndexOf("\\"));
                    //R7.LoadLibrary(workSpacePath); //開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。
                    reloadToolBox();
                    //20170119 leo open 成功後設置 title


                    int shortFileNameLength = args[1].Length - args[1].LastIndexOf("\\") - 1;
                    if (shortFileNameLength > 0)
                    {
                        programFileName = args[1].Substring(args[1].LastIndexOf("\\") + 1, shortFileNameLength);
                    }
                    else
                    {
                        programFileName = programFilePath;
                    }


                    string str = formMainTitle + "  -  " + programFileName;
                    this.Text = str;
                    toolStripWorkSpaceTextBox.Text = workSpacePath;
                }
                else
                {
                    if (args[1].Length == 0)
                    {
                        MessageBox.Show("Program file not exist");
                    }
                    else
                    {
                        MessageBox.Show("Program file not exist: " + args[1]);
                    }
                }
            }
            //MessageBox.Show("Environment.GetCommandLineArgs = " + str[str.Length - 1]);

            R7.r7PrintfCallbackFunctionDelegate = new R7.R7PrintfCallbackFunctionDelegate(R7PrintfCallback);

            R7.r7LogCallbackFunctionDelegate = new R7.R7LogCallbackFunctionDelegate(R7LogCallback);

            R7.RegisterLogCallbackFunction(R7.r7LogCallbackFunctionDelegate);

            Program.recordProgram(FormMain.r8, toolStripMenuItemUndo, toolStripMenuItemRedo);
            //Program.recordProgram(FormMain.r8);//leo: 第0步也要記錄，不然 undo 後會 redo 不回去

            //程式剛開啟時，把 Undo Redo 按鈕 disable
            Program.undoRedoButtonSetEnableOrDisable(toolStripMenuItemUndo, toolStripMenuItemRedo);


            Program.isFileChange = false;
            //MessageBox.Show("isFileChange = false");
            return;
        }

        private void TranslationAllSubToolStripItem(ToolStripMenuItem toolItem)
        {
            foreach (ToolStripMenuItem child in toolItem.DropDownItems)
            {
                if (child is ToolStripMenuItem) {
                    MessageBox.Show(toolItem.Text + " Tool child:" + child.Text);
                }
            }
            return;
        }

        private void TranslationAllChild(Control control) {
            foreach (Control child in control.Controls)
            {
                MessageBox.Show("get child:" + child.Text);
                child.Text = R8.TranslationString(child.Text);
                TranslationAllChild(child);
                if (child is MenuStrip)
                {

                    
                    MessageBox.Show("target is MenuStrip");
                    //if(((MenuStrip)(child)).Items.g)
                    foreach (ToolStripMenuItem toolItem in ((MenuStrip)(child)).Items) {
                        if (toolItem is ToolStripMenuItem)
                        {
                            MessageBox.Show("get toolItem:" + toolItem.Text);
                            //TranslationAllSubToolStripItem(toolItem);
                        }
                    }
                }
            }
            return;
        }

        private int TranslationMenuBar() {
            
            // File
            toolStripMenuItemFile.Text = R8.TranslationString(toolStripMenuItemFile.Text);
            // Undo
            toolStripMenuItemUndo.Text = R8.TranslationString(toolStripMenuItemUndo.Text);
            // Redo
            toolStripMenuItemRedo.Text = R8.TranslationString(toolStripMenuItemRedo.Text);
            // Library
            toolStripMenuItemLibrary.Text = R8.TranslationString(toolStripMenuItemLibrary.Text);
            // Workspace
            toolStripMenuItemWorkSpace.Text = R8.TranslationString(toolStripMenuItemWorkSpace.Text);
            // Release
            toolStripMenuItemRun.Text = R8.TranslationString(toolStripMenuItemRun.Text);
            // Debug
            toolStripMenuItemDebug.Text = R8.TranslationString(toolStripMenuItemDebug.Text);
            // About
            toolStripMenuItemAbout.Text = R8.TranslationString(toolStripMenuItemAbout.Text);
            // New
            newToolStripMenuItem.Text = R8.TranslationString(newToolStripMenuItem.Text);
            // Open
            openToolStripMenuItem.Text = R8.TranslationString(openToolStripMenuItem.Text);
            // Save
            saveToolStripMenuItem1.Text = R8.TranslationString(saveToolStripMenuItem1.Text);
            // Save As
            saveAsToolStripMenuItem.Text = R8.TranslationString(saveAsToolStripMenuItem.Text);
            // Export
            exportToolStripMenuItem1.Text = R8.TranslationString(exportToolStripMenuItem1.Text);
            // Exit
            exitToolStripMenuItem.Text = R8.TranslationString(exitToolStripMenuItem.Text);
            // Language
            //languageToolStripMenuItem.Text = R8.TranslationString(languageToolStripMenuItem.Text);

            // Export
            string str_Export1 = "Export to Windows batch file";
            string str_Export2 = "Export to Visual C++ file";
            str_Export1 = R8.TranslationString(str_Export1);
            str_Export2 = R8.TranslationString(str_Export2);
            exportBatchFileToolStripMenuItem.Text = str_Export1;
            exportCFileToolStripMenuItem.Text = str_Export2;

            //toolStripMenuItemRedo.Text = R8.TranslationString(toolStripMenuItemRedo.Text);
            /*
            //.....一個一個加工程有點浩大....試試看有沒有辦法[抓出該 Form 的所有 Label]
            
            //https://stackoverflow.com/questions/12808943/how-can-i-get-all-labels-on-a-form-and-set-the-text-property-of-those-with-a-par
            foreach (Control child in this.Controls)
            {
               //MessageBox.Show("get child:" + child.Text);

                if (child is MenuStrip || child is Panel)
                {
                    //child.GetContainerControl
                    MessageBox.Show("get child:" + child.Text);
                    foreach (Control subChild in child.Controls)
                    {
                        MessageBox.Show("get subChild:" + subChild.Text);
                        subChild.Text = R8.TranslationString(subChild.Text);

                    }
                    
                }
                else {
                    MessageBox.Show("get child:" + child.Text);
                    child.Text = R8.TranslationString(child.Text);
                }
            }
            */
            /*
            //https://stackoverflow.com/questions/8595425/how-to-get-all-children-of-a-parent-control
            var queue = new Queue<Control>();

            queue.Enqueue(this);

            do
            {
                var control = queue.Dequeue();

                //yield return control;

                foreach (var child in control.Controls.OfType<Control>())
                {
                    queue.Enqueue(child);

                    MessageBox.Show("get child:" + child.Text);
                    child.Text = R8.TranslationString(child.Text);
                }

            } while (queue.Count > 0);
            */

            //20180212 上午 10:17 接到其他任務，這邊先中斷
            //20180212 下午 14:21 繼續
            //下午 3:10 接到其他任務，再度中斷
            // TranslationAllChild(this);
            return 1;
        }

        private int R7PrintfCallback(int r7Sn, StringBuilder sb)
        {
            //R8.Log("R7PrintfCallback " + r7Sn);
            //MessageBox.Show(sb.ToString());
            R8.addLogToLogBox(sb.ToString());
            return 1;
        }

        private int R7LogCallback(StringBuilder sb)
        {
            //R8.Log("R7PrintfCallback " + r7Sn);
            //MessageBox.Show(sb.ToString());
            R8.addLogToLogBox(sb.ToString());
            return 1;
        }

        private void loadLibraryFunctions()
        {
            //20170206 leo: 改成從 r7 讀 list
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            //int res = R7.GetSupportList(str, R7.RESULT_SIZE);
            //int res = R8.GetSupportListByFileName("R7.support");
            //int res = R8.GetSupportListByFileName(System.Windows.Forms.Application.StartupPath + "\\R7.support");
            //byte[] bytes;
            //string fileNameString;
            int res = 0;
            //if (false)
            //{
            //    fileNameString = System.Windows.Forms.Application.StartupPath + "\\R7.support";
            //    bytes = new byte[fileNameString.Length * sizeof(char)];
            //    System.Buffer.BlockCopy(fileNameString.ToCharArray(), 0, bytes, 0, bytes.Length);
            //    res = R8.GetSupportListByFileName(bytes);
            //}
            //20170502 leo 依早上討論，要改回 [程式產生 supportList，不讀 R7.support 檔案]。
            //然後現在讀 supportList 是在 R8 的 dll ，
            //所以要改成
            //1. R7 產生 supportList 的 string buffer ，R8 吃該 buffer 產生對應結構

            StringBuilder supportListStr = new StringBuilder(R7.STRING_SIZE * 100);
            R7.GetSupportList(supportListStr, R7.STRING_SIZE * 100);
            R8.BuildSupportListByString(supportListStr);



            //20170407 需要增加讀 library 的 support lise
            //20170510 leo: library也改為可以從 dll 讀 supportList 了。
            //目前先新舊並行，也從 xxx.support 讀 supportList
            //if (true) {
            //    //20170428 leo: 依上午討論 library 架構修改，要多讀 workspace 的 librarys
            //    //此架構下可能重複 load ，讀 xxx.support ，然後紀錄一下名稱有沒有重複，重複名稱的就不讀
            //    List<string> supportFileNameList = new List<string>();
            //    string librarysPath = System.Windows.Forms.Application.StartupPath + "\\librarys";
            //    //String librarysPath = System.Windows.Forms.Application.StartupPath;
            //    // string temp = "";
            //    //string[] fileEntries = Directory.GetFiles(librarysPath);
            //    if (Directory.Exists(librarysPath))
            //    {
            //        string[] subDirs = Directory.GetDirectories(librarysPath);

            //        // MessageBox.Show("librarysPath " + librarysPath + ", find " + subDirs.Length + "subDirs ");
            //        string librarySupportFile;
            //        string folderName;
            //        foreach (string dirName in subDirs)
            //        {

            //            //librarySupportFile = dirName + "\\library.support";
            //            //20170407 library.support 必需更名為 [資料夾名稱.support]...需要加一小段抓資料夾名稱
            //            folderName = dirName.Substring(librarysPath.Length);//這樣有抓到斜線符號，剛好要用
            //            //MessageBox.Show(folderName);
            //            librarySupportFile = dirName + folderName + ".support";
            //            //MessageBox.Show(librarySupportFile);

            //            if (supportFileNameList.IndexOf(folderName) == -1)
            //            {
            //                if (File.Exists(librarySupportFile))
            //                {
            //                    //MessageBox.Show("Find File.");
            //                    byte[] librarySupportFilePathByte = new byte[librarySupportFile.Length * sizeof(char)];
            //                    System.Buffer.BlockCopy(librarySupportFile.ToCharArray(), 0, librarySupportFilePathByte, 0, librarySupportFilePathByte.Length);
            //                     R8.AddLibrarySupportListByFileName(librarySupportFilePathByte);

            //                    //res = R8.AddLibrarySupportListByFileName(librarySupportFile);
            //                    //MessageBox.Show("AddLibrary res = " + ret);
            //                    supportFileNameList.Add(folderName);
            //                }
            //            }
            //            else {
            //            }
            //        }
            //    }

            //    //MessageBox.Show("dirName " + temp);


            //    librarysPath = workSpacePath + "\\library";
            //    //String librarysPath = System.Windows.Forms.Application.StartupPath;
            //    // string temp = "";
            //    //string[] fileEntries = Directory.GetFiles(librarysPath);
            //    if (Directory.Exists(librarysPath))
            //    {
            //        string[] subDirs = Directory.GetDirectories(librarysPath);

            //        //MessageBox.Show("librarysPath " + librarysPath + ", find " + subDirs.Length + "subDirs ");
            //        string librarySupportFile;
            //        string folderName;
            //        foreach (string dirName in subDirs)
            //        {

            //            //librarySupportFile = dirName + "\\library.support";
            //            //20170407 library.support 必需更名為 [資料夾名稱.support]...需要加一小段抓資料夾名稱
            //            folderName = dirName.Substring(librarysPath.Length);//這樣有抓到斜線符號，剛好要用
            //            //MessageBox.Show(folderName);
            //            librarySupportFile = dirName + folderName + ".support";
            //            //MessageBox.Show(librarySupportFile);

            //            if (supportFileNameList.IndexOf(folderName) == -1)
            //            {
            //                if (File.Exists(librarySupportFile))
            //                {
            //                    //MessageBox.Show("Find File.");
            //                    byte[] librarySupportFilePathByte = new byte[librarySupportFile.Length * sizeof(char)];
            //                    System.Buffer.BlockCopy(librarySupportFile.ToCharArray(), 0, librarySupportFilePathByte, 0, librarySupportFilePathByte.Length);
            //                    R8.AddLibrarySupportListByFileName(librarySupportFilePathByte);
            //                    //res = R8.AddLibrarySupportListByFileName(librarySupportFile);
            //                    //MessageBox.Show("AddLibrary res = " + ret);
            //                    supportFileNameList.Add(folderName);
            //                }
            //            }
            //            else
            //            {
            //            }

            //        }
            //    }
            //}


            //20170417 巨集功能測試
            //if (false) {
            //    //20170419 依信件暫時關閉此功能 
            //    //20170421 編輯 A9 program 會用到，在測試版開啟此功能
            //    //20170425 新架構下 macros 功能已失效。
            //    string macrosPath = System.Windows.Forms.Application.StartupPath + "\\macros";
            //    if (Directory.Exists(macrosPath))
            //    {
            //        R8.StartMacroSupportList();
            //        string[] fileNames = Directory.GetFiles(macrosPath);
            //        foreach (string fileName in fileNames)
            //        {
            //            //MessageBox.Show("File Name: " + fileName);
            //            //int r = R8.AddMacroSupportListByFileName(fileName);
            //            //MessageBox.Show("res = " + r);
            //            //R8.AddMacroSupportListByFileName(fileName);

            //            bytes = new byte[fileName.Length * sizeof(char)];
            //            System.Buffer.BlockCopy(fileName.ToCharArray(), 0, bytes, 0, bytes.Length);
            //            R8.AddMacroSupportListByFileName(bytes);
            //            //res = R8.GetSupportListByFileName(bytes);
            //        }
            //        R8.EndOfMacroSupportList();
            //    }
            //}

            str.Clear();
            if (res < 0)
            {
                R8.logToLogBox("loadFunctionFunction Fail");
            }
            else
            {
                str = new StringBuilder(R7.STRING_SIZE);
                R7.GetVersion(str, R7.STRING_SIZE);
                str.Clear();
            }

            //20170206 leo: 下面是舊版，先留著。
            /*
            //MessageBox.Show("loadFunctionFunction");
            //int res = R8.ReadList("C:\\Users\\a1s\\AppData\\Local\\app\\app\\1.0.0.0\\support_list_20170123.json");
            int res = R8.ReadList(System.Windows.Forms.Application.StartupPath + "\\toolbox.json");

            //如果沒讀到檔案，塞預設值
            if (res < 0)
            {
                //20170123 leo : 這個結構整個搬去 dll 了，
                //R8.typeArray = new string[] { "int", "bool", "float", "double", "string", "image" };
                R8.logToLogBox("loadFunctionFunction Fail");
            }
            else
            {

                //int variableNum;
                //variableNum = R8.GetVariableNum();
                //System.Console.WriteLine("GetVariableNum = " + variableNum);
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetBrainVersion(str, R7.STRING_SIZE);
                str.Clear();
            }
            */
            //MessageBox.Show("loadFunctionFunction OK");
            return;
        }

        private void loadForms()
        {
            if (formLibrary != null)
            {
                formLibrary.Close();
            }
            formLibrary = new FormLibrary();
            formLibrary.MdiParent = this;
            formLibrary.Show();

            if (formFunctions != null)
            {
                formFunctions.Close();
            }
            formFunctions = new FormFunctions();
            formFunctions.MdiParent = this;
            formFunctions.Location = new Point(formLibrary.Location.X + formLibrary.Width + formDistance, formLibrary.Location.Y);
            formFunctions.Show();

            if (formFunction != null)
            {
                formFunction.Close();
            }
            formFunction = new FormFunction();
            formFunction.MdiParent = this;
            formFunction.Location = new Point(formFunctions.Location.X + formFunctions.Width + formDistance, formFunctions.Location.Y);
            formFunction.Show();

            if (formVariable != null)
            {
                formVariable.Close();
            }
            formVariable = new FormVariable();
            formVariable.MdiParent = this;
            formVariable.Location = new Point(formFunction.Location.X, formFunction.Location.Y + formFunction.Height + formDistance);
            formVariable.Show();

            //20170118 leo: 今天討論後，拿掉 formPreviewImage
            /*
            if (formPreviewImage != null)
            {
                formPreviewImage.Close();
            }
            formPreviewImage = new FormPreviewImage();
            formPreviewImage.MdiParent = this;
            formPreviewImage.Location = new Point(formVariable.Location.X, formVariable.Location.Y + formVariable.Height + formDistance);
            formPreviewImage.Show();
            */

            
            if (formLog != null)
            {
                formLog.Close();
            }
            formLog = new FormLog();
            formLog.MdiParent = this;
            formLog.Location = new Point(formVariable.Location.X, formVariable.Location.Y + formVariable.Height + formDistance);
            formLog.Show();
            formLog.Visible = false;



            if (formVariables != null)
            {
                formVariables.Close();
            }
            formVariables = new FormVariables();
            formVariables.MdiParent = this;
            formVariables.Location = new Point(formFunction.Location.X + formFunction.Width + formDistance, formFunction.Location.Y);
            formVariables.Show();

            //20170202移除 versionTextBox
            //versionTextBox.Text = "version " + r8.version;

            return;
        }

        //已關閉主頁面
        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            //MessageBox.Show("Closeed");
            deleteTempR6File();
            R8.CloseLib();
            R7.CloseLib();
            return;
        }

        //DLL回傳狀態
        private int aoiCallback(int type, int parameter1, int parameter2)
        {
            /*
            if (isGoHome == 0)
            {
                if (type == R8.AOI_CALLBACK_TYPE_MOTION_DONE)
                {

                    isGoHome = 1;
                    RefreshToolBar();

                }
                else if (type == R8.AOI_CALLBACK_TYPE_MOTION_ERROR)
                {
                    RefreshToolBar();

                    MessageBox.Show("E005 錯誤！馬達尚未準備好！");
                    Application.Exit();
                }
            }
            //if (this.formCalibration != null) {
            //    this.formCalibration.SetEvent(type, parameter1, parameter2);
            //}

            //if (this.formAOI != null) {
            //    this.formAOI.SetEvent(type, parameter1, parameter2);
            //}

            if (this.formVision != null)
            {
                this.formVision.SetEvent(type, parameter1, parameter2);
            }

            if (this.formSetGoodDie != null)
            {
                this.formSetGoodDie.SetEvent(type, parameter1, parameter2);
            }

            //if (this.formAutoMapping != null)
            //{
            //    this.formAutoMapping.SetEvent(type, parameter1, parameter2);
            //}


            //if (this.formRegulateDie != null) {
            //    this.formRegulateDie.SetEvent(type, parameter1, parameter2);
            //}

            //if (this.formSetThreshold != null)
            //{
            //    this.formSetThreshold.SetEvent(type, parameter1, parameter2);
            //}

            //if (this.formDieFindAlignment != null)
            //{
            //    this.formDieFindAlignment.SetEvent(type, parameter1, parameter2);
            //}

            //if (this.formJob != null) {
            //    this.formJob.SetEvent(type, parameter1, parameter2);
            //}


            //if (this.formscan != null) {
            //    this.formscan.SetEvent(type, parameter1, parameter2);
            //}

            if (this.formScanSample != null)
            {
                this.formScanSample.SetEvent(type, parameter1, parameter2);
            }

            if (this.formDirectoryList != null)
            {
                this.formDirectoryList.SetEvent(type, parameter1, parameter2);
            }

            //if (this.formMessage != null)
            //{
            //    this.formMessage.SetEvent(type, parameter1, parameter2);
            //}

            if (this.formSelectWaferPosition != null)
            {
                this.formSelectWaferPosition.SetEvent(type, parameter1, parameter2);
            }

            //鏡頭切換
            //if (this.formMicroscopeSelect != null) {
            //    this.formMicroscopeSelect.SetEvent(type, parameter1, parameter2);
            //}

            //Die size
            //if (this.formMeasureDie != null) {
            //    this.formMeasureDie.SetEvent(type, parameter1, parameter2);
            //}

            */
            return 1;
        }

        //正在關閉主頁面
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //R8.CloseLib();
            // MessageBox.Show("Closeing");

            //20171117 leo:項目[關閉 R8 時，如果有修改內容卻沒存檔，請顯示 message box ： This program is not saved. Are you sure to exit? (No/Yes)]
            if (Program.isFileChange) {

                DialogResult dialogResult = MessageBox.Show("This program is not saved. Are you sure to exit?", "Save File?", MessageBoxButtons.YesNo);

                //20180115 leo: 發現實際行為與那串英文描述的不一樣....
                //之前行為是：選 [yes] 存檔後退出，選 [no] 直接退出
                //但那串英文是說[是否要退出]
                //所以改成[如果選 yes 則退出，選 no 則不退出]
                //實作方法參考 http://www.cnblogs.com/scottckt/archive/2007/11/16/961681.html

                e.Cancel = (dialogResult == DialogResult.No);
                /*
                //舊版(選 yes 會幫忙存檔的)先留著
                if (dialogResult == DialogResult.Yes)
                {
                    if (programFilePath == null)
                    {
                        saveAsToolStripMenuItem_Click(sender, e);
                    }
                    else
                    {
                        if (formVariable != null)
                        {
                            formVariable.setVariable();
                        }
                        R8.writeProgramXml(programFilePath);
                       // MessageBox.Show("Saved. ");
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                  
                }
                */
            }

            return;
        }


        //系統設定
        private void systemSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }



        public int enableUI(int isEnable)
        {
            /*
            this.Invoke((MethodInvoker)delegate {

                if (isEnable == 1)
                {
                    //語言切換
                    //AOI.EnableButton(buttonLanguage, 1);

                    if (formJob == null)
                    {
                        //AOI.EnableButton(buttonLotInfo, 1);
                        R8.EnableButton(buttonStart, 0);
                        R8.EnableButton(buttonAbortLot, 0);
                        R8.EnableButton(btnOpen, 0);
                        //AOI.EnableButton(buttonSchedule, 0);

                        //AOI.EnableButton(buttonAbortLot, 0);
                        R8.EnableButton(buttonEndLot, 0);
                        R8.EnableButton(buttonHome, 1);
                    }
                    else
                    {
                        //if (formscan != null) {
                        //AOI.EnableButton(buttonLotInfo, lotInfoEnable);
                        //AOI.EnableButton(buttonStart, startEnable);
                        //AOI.EnableButton(buttonAbortLot, rescanEnable);
                        //AOI.EnableButton(buttonAbortLot, abortLotEnable);
                        //AOI.EnableButton(buttonEndLot, endLotEnable);
                        //AOI.EnableButton(buttonHome, homeEnable);
                        //AOI.EnableButton(buttonLotInfo, 1);
                        //AOI.EnableButton(buttonStart, 1);
                        //AOI.EnableButton(buttonAbortLot, 1);

                        //AOI.EnableButton(buttonEndLot, 1);
                        //AOI.EnableButton(buttonHome, 1);
                        //}
                    }
                }
                else
                {
                    //AOI.EnableButton(buttonLotInfo, 1);
                    R8.EnableButton(buttonStart, 1);
                    R8.EnableButton(buttonAbortLot, 1);
                    R8.EnableButton(btnOpen, 1);
                    //AOI.EnableButton(buttonSchedule, 1);
                    //AOI.EnableButton(buttonAbortLot, 0);
                    R8.EnableButton(buttonEndLot, 1);
                    R8.EnableButton(buttonHome, 1);
                    R8.EnableButton(buttonSchedule, 1);
                    //if (formJob == null && callBackManager.isMotionForm() == 0)
                    //{
                    //    AOI.EnableButton(buttonLanguage, 0);
                    //}
                }
            });
            */
            return 1;
        }

        public void initUI()
        {
            /*
            this.newToolStripMenuItem.Enabled = false;
            this.openToolStripMenuItem.Enabled = false;
            this.systemStripMenuItem.Enabled = false;
            this.SpeedToolStripMenuItem.Enabled = false;
            R8.EnableButton(buttonStart, 0);
            R8.EnableButton(buttonAbortLot, 0);
            R8.EnableButton(btnOpen, 0);
            R8.EnableButton(buttonSchedule, 0);
            R8.EnableButton(buttonEndLot, 0);
            R8.EnableButton(buttonHome, 1);
            */
        }


        //測試ScanForm畫面
        private void toolStripMenuItemTest1_Click(object sender, EventArgs e)
        {   /*
            formScanSample = new FormScanSample(this);
            //formScanSample.MdiParent = this;
            formScanSample.Show();
            */
            return;
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }


        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        //目前未使用(現在用 open 不用 load)
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            if (formRCP != null)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "R8 Files|*.r8;*.program";
                openFileDialog1.Title = "Select a R8 File";
                openFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath;
                openFileDialog1.FileName = defaultFileName;
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    formVariables.isShowTargetData = false;
                    formRCP.readProgramXml(openFileDialog1.FileName);
                }
                //formRCP.readProgramXml("test3.xml");
            }

            if (formFunctions != null)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "R8 Files|*.r8;*.program";
                openFileDialog1.Title = "Select a R8 File";
                openFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath;
                openFileDialog1.FileName = defaultFileName;
                if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    formVariables.isShowTargetData = false;
                    formFunctions.readProgramXml(openFileDialog1.FileName);
                }
            }
            */
        }


        /*
        private void toolStripMenuItemTest1_Click_1(object sender, EventArgs e)
        {
       
            if (formTools != null)
            {
                formTools.Close();
            }
            formTools = new FormTools();
            formTools.MdiParent = this;
            formTools.Show();


            if (formRCP != null)
            {
                formRCP.Close();
            }
            formRCP = new FormRCP();
            formRCP.MdiParent = this;
            formRCP.Location = new Point(formTools.Location.X + formTools.Width, formTools.Location.Y);
            formRCP.Show();
        }
        */

        public int actionMode = 0; // 0 = 拖曳模式， 1 = 拉線模式
                                   /*
                                   private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
                                   {
                                       actionMode = toolStripComboBox1.SelectedIndex;
                                   }
                                   */


        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (formFunctions == null)
            {
                loadForms();
            }
            if (formVariable != null)
            {
                formVariable.variableSn = -1;
                formVariable.showVariable(-1);
            }

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Open Robot Language File|*.r6;*.xml";//20170531 leo: 增加舊版 recipe 支援
            openFileDialog1.Title = "Open File";
            openFileDialog1.InitialDirectory = workSpacePath;
            openFileDialog1.FileName = defaultFileName;
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                clearSearchBox();

                formFunctions.readProgramXml(openFileDialog1.FileName);
                deleteTempR6File();//20171115 add
                programFilePath = openFileDialog1.FileName;
                workSpacePath = openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.LastIndexOf("\\"));

                int shortFileNameLength = openFileDialog1.FileName.Length - openFileDialog1.FileName.LastIndexOf("\\") - 1;
                if (shortFileNameLength > 0)
                {
                    programFileName = openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1, shortFileNameLength);
                }
                else
                {
                    programFileName = programFilePath;
                }
                Program.clearRecord();
                Program.recordProgram(FormMain.r8, toolStripMenuItemUndo, toolStripMenuItemRedo);
                //Program.recordProgram(FormMain.r8);//leo: 第0步也要記錄，不然 undo 後會 redo 不回去

                //R7.LoadLibrary(workSpacePath);//20171110 開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。
                reloadToolBox();
                //20170119 leo open 成功後設置 title
                string str = formMainTitle + "  -  " + programFileName;
                this.Text = str;
                //this.Text = formMainTitle + "      " + programFilePath;
                //System.Console.WriteLine(this.Text.ToString());
                //System.Console.WriteLine(str);
                //System.Console.WriteLine(this.Text.ToString().Length);

                toolStripWorkSpaceTextBox.Text = workSpacePath;


                Program.isFileChange = false;
                //MessageBox.Show("isFileChange = false");
            }
            //versionTextBox.Text = "version " + r8.version;
            Program.undoRedoButtonSetEnableOrDisable(toolStripMenuItemUndo, toolStripMenuItemRedo);
        }


        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //
            //20170808 leo: 由於 recipe 檔案大了會很亂，加入整理並另存功能
            //1. 把 function 依照 posY 重新排序
            //2. 把 variable 依照 name 重新排序
            //(討論後取消)3. 把未在任何 function 中使用到的 variable 移除

            //20170815 leo: Kelly 回報 Function_Enable 以及 Function_EnableList 在排序後會異常。
            //因為這兩個 function 有使用到 FunctionSn 作為 Variable ，排序時必須對該 Variable 另外進行處理。
            if (formVariable != null)
            {
                formVariable.setVariable();
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Open Robot Language File|*.r6;*.xml";
            saveFileDialog1.Title = "Sort And Save As";
            saveFileDialog1.InitialDirectory = workSpacePath;
            if (programFileName == null) {
                programFileName = defaultFileName;
            }
            //saveFileDialog1.FileName = defaultFileName;
            saveFileDialog1.FileName = programFileName; //programFileName  programFilePath
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                int i, j;
                List<Function> sortFunctions = new List<Function>();// = r8.functions.ToList();
                Function function;
                List<Variable> sortVariables = new List<Variable>();
                Variable variable;
                for (i = 0; i <= r8.getFunctionSnMax(); i++)
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
                    sortFunctions.Add(function);
                }
                sortFunctions.Sort((x, y) => { return x.posY.CompareTo(y.posY); });

                for (i = 0; i <= r8.getVariableSnMax(); i++)
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
                    sortVariables.Add(variable);
                }
                sortVariables.Sort((x, y) => { return x.name.CompareTo(y.name); });

                //排序後，各 function 之對應的 variablesSn 會變 
                int[] sortVariablesSn = new int[R7.VARIABLES_SIZE];
                for (i = 0; i < sortVariables.Count; i++)
                {
                    //function varibable 都是從1開始，所以轉換時要 +1
                    variable = sortVariables.ElementAt(i);
                    sortVariablesSn[variable.sn] = i + 1;
                }

                int[] sortFunctionsSn = new int[R7.VARIABLES_SIZE];
                for (i = 0; i < sortFunctions.Count; i++)
                {
                    //function varibable 都是從1開始，所以轉換時要 +1
                    function = sortFunctions.ElementAt(i);
                    sortFunctionsSn[function.sn] = i + 1;
                }

                int targetSn = 0;

                for (i = 0; i < sortFunctions.Count; i++)
                {

                    function = sortFunctions.ElementAt(i);

                    if (function.name != null)
                    {
                        if (function.name.Equals("Function_Enable") ||
                            function.name.Equals("FunctionEnable") ||
                            function.name.Equals("IntentEnable"))
                        {
                            if (function.parameters.Count > 1)
                            {
                                targetSn = function.parameters.ElementAt(1).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    //variable = FormMain.r8.variables[function.parameters.ElementAt(1).variableSn];
                                    //variable = FormMain.r8.variables[targetSn];
                                    //MessageBox.Show("pre getValue: " + variable.sn + variable.name + variable.value);
                                    //MessageBox.Show("sortVariablesSn: " + sortVariablesSn[function.parameters.ElementAt(1).variableSn]);

                                    //比對測試後，要轉換的目標 Value 應該是這個
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    //MessageBox.Show("sortVariables Value: " + variable.sn + variable.name + variable.value);
                                    //後半要用原始 value ，避免[同一個 variable 用在兩個以上的 Function_Enable]導致錯誤
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }
                            }
                        }
                        else if (function.name.Equals("Function_EnableList") ||
                           function.name.Equals("FunctionEnableList") ||
                           function.name.Equals("IntentEnableList"))
                        {
                            if (function.parameters.Count > 2)
                            {
                                targetSn = function.parameters.ElementAt(1).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    //variable.value = sortFunctionsSn[Convert.ToInt32(variable.value)].ToString();
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }

                                targetSn = function.parameters.ElementAt(2).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }
                            }
                        }
                    }

                    for (j = 0; j < function.parameters.Count; j++)
                    {
                        if (function.parameters.ElementAt(j).variableSn != -1 && function.parameters.ElementAt(j).variableSn != 0)
                        {
                            function.parameters.ElementAt(j).variableSn = sortVariablesSn[function.parameters.ElementAt(j).variableSn];
                        }
                    }
                }

                //然後把排序好的東西存回去
                r8 = new R8();
                for (i = 0; i < sortFunctions.Count; i++)
                {
                    function = sortFunctions.ElementAt(i);

                    r8.addFunction(function);
                }

                for (i = 0; i < sortVariables.Count; i++)
                {
                    variable = sortVariables.ElementAt(i);
                    r8.addVariable(variable);
                }

                deleteTempR6File();//20171115 add
                programFilePath = saveFileDialog1.FileName;
                workSpacePath = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf("\\"));
                //R7.LoadLibrary(workSpacePath); //20171110 項目[開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。]
                reloadToolBox();
                R8.writeProgramXml(saveFileDialog1.FileName);

                //存檔後把排序後的檔案打開
                if (formFunctions == null)
                {
                    loadForms();
                }
                if (formVariable != null)
                {
                    formVariable.variableSn = -1;
                    formVariable.showVariable(-1);
                }

                formFunctions.readProgramXml(saveFileDialog1.FileName);

                int shortFileNameLength = saveFileDialog1.FileName.Length - saveFileDialog1.FileName.LastIndexOf("\\") - 1;
                if (shortFileNameLength > 0)
                {
                    programFileName = saveFileDialog1.FileName.Substring(saveFileDialog1.FileName.LastIndexOf("\\") + 1, shortFileNameLength);
                }
                else
                {
                    programFileName = programFilePath;
                }

                string str = formMainTitle + "  -  " + programFileName;
                this.Text = str;
                toolStripWorkSpaceTextBox.Text = workSpacePath;
                Program.isFileChange = false;
                //MessageBox.Show("isFileChange = false");
            }
        }

        public void saveTempFile()
        {
            if (programFilePath == null)
            {   // 未存過檔時....暫定檔名設為 temp.r6t
                tempProgramFilePath = "temp.r6t";
            }
            else {
                tempProgramFilePath = programFilePath + "t";
            }

            if (formVariable != null)
            {
                formVariable.setVariable();
            }
            R8.writeProgramXml(tempProgramFilePath);

            return;
        }


        public void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (programFilePath == null)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                if (formVariable != null)
                {
                    formVariable.setVariable();
                }
                R8.writeProgramXml(programFilePath);
                Program.isFileChange = false;
                //MessageBox.Show("isFileChange = false");
            }
        }


        //項目[每次存檔都用排序存檔，存檔完畢後，自動 reload]
        public void saveToolStripMenuItemAndSort_Click(object sender, EventArgs e)
        {
            if (programFilePath == null)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                if (formVariable != null)
                {
                    formVariable.setVariable();
                }

                //R8.writeProgramXml(programFilePath);

                int i, j;
                List<Function> sortFunctions = new List<Function>();// = r8.functions.ToList();
                Function function;
                List<Variable> sortVariables = new List<Variable>();
                Variable variable;
                for (i = 0; i <= r8.getFunctionSnMax(); i++)
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
                    sortFunctions.Add(function);
                }
                sortFunctions.Sort((x, y) => { return x.posY.CompareTo(y.posY); });

                for (i = 0; i <= r8.getVariableSnMax(); i++)
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
                    sortVariables.Add(variable);
                }
                sortVariables.Sort((x, y) => { return x.name.CompareTo(y.name); });

                //排序後，各 function 之對應的 variablesSn 會變 
                int[] sortVariablesSn = new int[R7.VARIABLES_SIZE];
                for (i = 0; i < sortVariables.Count; i++)
                {
                    //function varibable 都是從1開始，所以轉換時要 +1
                    variable = sortVariables.ElementAt(i);
                    sortVariablesSn[variable.sn] = i + 1;
                }

                int[] sortFunctionsSn = new int[R7.VARIABLES_SIZE];
                for (i = 0; i < sortFunctions.Count; i++)
                {
                    //function varibable 都是從1開始，所以轉換時要 +1
                    function = sortFunctions.ElementAt(i);
                    sortFunctionsSn[function.sn] = i + 1;
                }

                int targetSn = 0;

                for (i = 0; i < sortFunctions.Count; i++)
                {

                    function = sortFunctions.ElementAt(i);

                    if (function.name != null)
                    {
                        if (function.name.Equals("Function_Enable") ||
                            function.name.Equals("FunctionEnable") ||
                            function.name.Equals("IntentEnable"))
                        {
                            if (function.parameters.Count > 1)
                            {
                                targetSn = function.parameters.ElementAt(1).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    //variable = FormMain.r8.variables[function.parameters.ElementAt(1).variableSn];
                                    //variable = FormMain.r8.variables[targetSn];
                                    //MessageBox.Show("pre getValue: " + variable.sn + variable.name + variable.value);
                                    //MessageBox.Show("sortVariablesSn: " + sortVariablesSn[function.parameters.ElementAt(1).variableSn]);

                                    //比對測試後，要轉換的目標 Value 應該是這個
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    //MessageBox.Show("sortVariables Value: " + variable.sn + variable.name + variable.value);
                                    //後半要用原始 value ，避免[同一個 variable 用在兩個以上的 Function_Enable]導致錯誤
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }
                            }
                        }
                        else if (function.name.Equals("Function_EnableList") ||
                           function.name.Equals("FunctionEnableList") ||
                           function.name.Equals("IntentEnableList"))
                        {
                            if (function.parameters.Count > 2)
                            {
                                targetSn = function.parameters.ElementAt(1).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    //variable.value = sortFunctionsSn[Convert.ToInt32(variable.value)].ToString();
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }

                                targetSn = function.parameters.ElementAt(2).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }
                            }
                        }
                    }

                    for (j = 0; j < function.parameters.Count; j++)
                    {
                        if (function.parameters.ElementAt(j).variableSn != -1 && function.parameters.ElementAt(j).variableSn != 0)
                        {
                            function.parameters.ElementAt(j).variableSn = sortVariablesSn[function.parameters.ElementAt(j).variableSn];
                        }
                    }
                }

                //然後把排序好的東西存回去
                r8 = new R8();
                for (i = 0; i < sortFunctions.Count; i++)
                {
                    function = sortFunctions.ElementAt(i);

                    r8.addFunction(function);
                }

                for (i = 0; i < sortVariables.Count; i++)
                {
                    variable = sortVariables.ElementAt(i);
                    r8.addVariable(variable);
                }

                //R7.LoadLibrary(workSpacePath); ////20171110 項目[開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。]
                reloadToolBox();
                R8.writeProgramXml(programFilePath);

                //存檔後重 load 排序後的檔案
                if (formFunctions == null)
                {
                    loadForms();
                }
                if (formVariable != null)
                {
                    formVariable.variableSn = -1;
                    formVariable.showVariable(-1);
                }

                formFunctions.readProgramXml(programFilePath);
                
            }
        }


        //20171016 舊版保留
        private void saveAsToolStripMenuItem_Click_Back(object sender, EventArgs e)
        {
            /*
            if (formFunctions == null)
            {
                loadForms();
            }
            */
            if (formVariable != null)
            {
                formVariable.setVariable();
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Open Robot Language File|*.r6;*.xml";
            saveFileDialog1.Title = "Save As";
            saveFileDialog1.InitialDirectory = workSpacePath;
            saveFileDialog1.FileName = defaultFileName;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                deleteTempR6File();//20171115 add
                programFilePath = saveFileDialog1.FileName;
                workSpacePath = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf("\\"));
                //R7.LoadLibrary(workSpacePath); //20171110 項目[開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。]
                reloadToolBox();
                R8.writeProgramXml(saveFileDialog1.FileName);
                //20170119 leo save 成功後設置 title
                //this.Text = formMainTitle + "      " + programFilePath;

                int shortFileNameLength = saveFileDialog1.FileName.Length - saveFileDialog1.FileName.LastIndexOf("\\") - 1;
                if (shortFileNameLength > 0)
                {
                    programFileName = saveFileDialog1.FileName.Substring(saveFileDialog1.FileName.LastIndexOf("\\") + 1, shortFileNameLength);
                }
                else
                {
                    programFileName = programFilePath;
                }

                string str = formMainTitle + "  -  " + programFileName;
                this.Text = str;
                toolStripWorkSpaceTextBox.Text = workSpacePath;

                //20170417 如果存檔到巨集資料夾則需要重 load
                //MessageBox.Show("workSpacePath = " + workSpacePath + "\nmacroPath = " + System.Windows.Forms.Application.StartupPath + "\\macros");
                // if (workSpacePath.Equals(System.Windows.Forms.Application.StartupPath + "\\macros")) {
                //    reloadToolBox();
                //}
            }
        }

        //來源 http://stackoverflow.com/questions/14237269/c-sharp-encoding-conversion
        //string 轉 byte
        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        /*
        private void toolStripMenuItemDebug_Click(object sender, EventArgs e)
        {
            if (programFilePath == null) {
                //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                //加個警示訊息
                MessageBox.Show("Please save program before run it.");
                return;
            }
            //20170202 leo : Run 時自動 save
            saveToolStripMenuItem_Click(sender, e);

            //20170206 leo: 新版換成直接呼叫 dll
            //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
            if (programFilePath != null)
            {
                //20170330 上方的按鈕在按下 Run 及 Debug 之後，請 disable 。Run 及 Debug 完畢之後，才 Enable 。
                toolStripMenuItemDebug.Enabled = false;
                //debugToolStripMenuItem.Enabled = false;
                StringBuilder resultBuffer = new StringBuilder(R7.RESULT_SIZE);
                StringBuilder workSpacePathBuffer = new StringBuilder(workSpacePath + "\\", R7.RESULT_SIZE);
                int r7h = R7.New();
                //20170314 leo: 發現之前的方式會機率性當機， byte array 讀檔案，最後一格要補 0 
                //http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
                FileStream sourceFile = new FileStream(programFilePath, FileMode.Open);
                BinaryReader binReader = new BinaryReader(sourceFile);
                byte[] programFileBytes = new byte[sourceFile.Length + 1];
                for (long i = 0; i < sourceFile.Length; i++)
                {
                    programFileBytes[i] = binReader.ReadByte();
                }
                programFileBytes[sourceFile.Length] = 0;
                sourceFile.Close();
                binReader.Close();
                //int result = R7.Run(r7h, File.ReadAllBytes(programFilePath), new Byte[] { }, GetBytes(workSpacePathBuffer.ToString()), 0, resultBuffer, R7.RESULT_SIZE);

                //20170330 leo: 關於 "在顯示 Image 時就顯示 ReleasePrint" 的修改，下午接到指示暫時取消，這邊架構先留著，從 R7 那邊加開關，關閉這個功能
                int result = R7.Run(r7h, programFileBytes, new Byte[] { 0 }, GetBytes(workSpacePathBuffer.ToString()), 0, resultBuffer, R7.RESULT_SIZE);
                if (result > 0)
                {
                    R8.logToLogBox(resultBuffer.ToString());            
                    R7.ShowImage(r7h);
                }
                result = R7.Release(r7h);
                
                resultBuffer.Clear();
                workSpacePathBuffer.Clear();

                toolStripMenuItemDebug.Enabled = true;
                //debugToolStripMenuItem.Enabled = true;
            }
        }
        */

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //目前是空的，整合brain_dll後才有作用
            // this.Dispose();
        }

        private void selectPathToolStripMenuItem_Click(object sender, EventArgs e)
        {


            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            //folderBrowserDialog.Description = "Browse Folders";
            folderBrowserDialog.SelectedPath = workSpacePath;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                workSpacePath = folderBrowserDialog.SelectedPath;
                //R7.LoadLibrary(workSpacePath); //20171110 項目[開啟 .r6 檔案時，不用載入 workspace 裡面的 dll 。]
                reloadToolBox();
                toolStripWorkSpaceTextBox.Text = workSpacePath;
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void clearSearchBox()
        {
            formLibrary.clearSearchBox();
            formFunctions.clearSearchBox();
            formVariables.clearSearchBox();
            return;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            r8 = new R8();
            deleteTempR6File();//20171115 add
            programFilePath = null;

            string str = formMainTitle;
            this.Text = str;

            //toolStripWorkSpaceTextBox.Text = workSpacePath;

            if (formFunctions == null)
            {
                loadForms();
            }
            if (formVariable != null)
            {
                formVariable.variableSn = -1;
                formVariable.showVariable(-1);
            }
            formFunctions.readProgramXml(null);
            clearSearchBox();
            Program.clearRecord();
            Program.recordProgram(FormMain.r8, toolStripMenuItemUndo, toolStripMenuItemRedo);
            //Program.recordProgram(FormMain.r8);//leo: 第0步也要記錄，不然 undo 後會 redo 不回去
            /*
            //清空畫面
            foreach (Button b in formFunctions.buttons)
            {
                b.Dispose();
            }
            buttons.Clear();
            buttonsLocationY = buttonStartY;
            //讀 xml
            FormMain.r8 = new R8(XElement.Load(path));
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
            for (int i = 0; i < functionList.Count; i++)
            {
                function = functionList.ElementAt(i);
                //20170309 leo: 依早上討論，顯示格式修改 ImageOpen 1 [remark][disabled]
                //addButton("[" + function.sn + "] [" + function.name + "]", function.sn, function.enable);
                if (function.remark.Length > 0)
                {
                    addButton(function.name + " " + function.sn + " " + "[" + function.remark + "]", function.sn, function.enable);
                }
                else
                {
                    addButton(function.name + " " + function.sn + " ", function.sn, function.enable);
                }
                //20170309 leo end
                function.posY = buttons.Count;//讀出 button 後，應該要再依實際 button 位置進行 posY排序
            }
            formVariables.showVariables(null);
            formFunction.showFunction(null);
            this.Invalidate();
            */
            Program.undoRedoButtonSetEnableOrDisable(toolStripMenuItemUndo, toolStripMenuItemRedo);
            return;
        }

        private void reloadToolBox()
        {
            //20171110 項目 [開啟 .r6 檔案時，不重新載入 library 裡面的 dll 。]
            //loadLibraryFunctions();
            //formLibrary.resetButton();
            return;
        }

        //20170417 巨集功能會需要重讀 toolBox
        private void reLoadToolBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reloadToolBox();
            return;
        }

        private void toolStripMenuItemStore_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("http://www.openrobot.club/store/index?sn=10938");
            System.Diagnostics.Process.Start("http://www.openrobot.club/store/index?sn=8");
            return;
        }

        private void exportToVisualStudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon.", "Alert");
            return;
        }

        private void registerToolStripMenuItem_Click(object sender, EventArgs e)
        {   //20170502 leo:依上午討論，目前先改為跳出 register 視窗，而不是 Register Online 與 Purchase Product Serial Number。
            if (formRegister == null)
            {
                formRegister = new FormRegister();
                formRegister.MdiParent = this;
                //formLog.Location = new Point(formVariable.Location.X, formVariable.Location.Y + formVariable.Height + formDistance);
                formRegister.Show();
            }

            return;
        }

        private void exportToLinuxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Coming soon.", "Alert");
            return;
        }

        private void toolStripMenuItemAbout_Click(object sender, EventArgs e)
        {
            if (formAbout == null)
            {
                formAbout = new FormAbout();
                formAbout.MdiParent = this;
            }

            this.formAbout.Show();

            return;
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void runR6UseExe_Click(object sender, EventArgs e, bool isRelease)
        {
            //20171002 leo: fix ，相對路徑還是要改為以 .r6 檔案為基準(以 r7.exe 為基準時，部分 function(例如r7_ReadFilesFromFolder)會失效)
            if (programFilePath == null)
            {
                MessageBox.Show("Please save program before run it.");
                return;
            }
            //saveToolStripMenuItem_Click(sender, e);
            saveTempFile();//20171115 change

            if (tempProgramFilePath != null)
            {
                string selectedFileName;
                string r6FilePath;
                selectedFileName = tempProgramFilePath.Substring(tempProgramFilePath.LastIndexOf("\\") + 1);

                r6FilePath = tempProgramFilePath.Substring(0, tempProgramFilePath.LastIndexOf("\\"));
                string r7Path = System.Windows.Forms.Application.StartupPath + "\\" + "R7.exe";

                string r7RelativePath = R8.GetRelativePath(r6FilePath + "\\", r7Path);
                string r6RelativePath = R8.GetRelativePath(r6FilePath + "\\", workSpacePath);

                StringBuilder sb = new StringBuilder();
                if (isRelease)
                {
                    sb.AppendLine(" \"release_pause\" \"" + r6RelativePath.Replace("\\", "/") + "\" \"" + selectedFileName + "\" \"null\"");
                }
                else
                {
                    sb.AppendLine(" \"debug_pause\" \"" + r6RelativePath.Replace("\\", "/") + "\" \"" + selectedFileName + "\" \"null\"");
                }

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = r7RelativePath;
                info.WorkingDirectory = r6FilePath;
                info.Arguments = sb.ToString();
                Process process = new Process();
                process.StartInfo = info;
                process.Start();
                sb.Clear();

            }
        }


        private void runR6UseExe_Click_back(object sender, EventArgs e, bool isRelease)
        {
            if (programFilePath == null)
            {
                MessageBox.Show("Please save program before run it.");
                return;
            }
            //saveToolStripMenuItem_Click(sender, e);
            saveTempFile();//20171115 change
            if (programFilePath != null)
            {
                string selectedFileName;
                string r7FilePath;
                selectedFileName = tempProgramFilePath.Substring(tempProgramFilePath.LastIndexOf("\\") + 1); // xxx.r6 檔案名稱
                r7FilePath = System.Windows.Forms.Application.StartupPath;
                string r6FilePath = workSpacePath + "\\";
                //R7 路徑，基本上就是 R8 路徑
                string r7Path = System.Windows.Forms.Application.StartupPath + "\\" + "R7.exe";

                string r7RelativePath = R8.GetRelativePath(r7FilePath + "\\", r7Path);
                string r6RelativePath = R8.GetRelativePath(r7FilePath + "\\", workSpacePath);
                StringBuilder sb = new StringBuilder();
                if (isRelease)
                {
                    sb.AppendLine(" \"release_pause\" \"" + r6RelativePath.Replace("\\", "/") + "\" \"" + selectedFileName + "\" \"null\"");
                }
                else
                {
                    sb.AppendLine(" \"debug_pause\" \"" + r6RelativePath.Replace("\\", "/") + "\" \"" + selectedFileName + "\" \"null\"");

                }
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "R7.exe";
                info.WorkingDirectory = System.Windows.Forms.Application.StartupPath;
                info.Arguments = sb.ToString();
                Process process = new Process();
                process.StartInfo = info;
                process.Start();
                sb.Clear();
            }
        }

        private void runR6UseBat_Click(object sender, EventArgs e, bool isRelease)
        {
            if (programFilePath == null)
            {
                //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                //加個警示訊息
                MessageBox.Show("Please save program before run it.");
                return;
            }
            //20170202 leo : Run 時自動 save
            saveToolStripMenuItem_Click(sender, e);

            //20170206 leo: 新版換成直接呼叫 dll
            //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
            if (programFilePath != null)
            {
                //20170330 上方的按鈕在按下 Run 及 Debug 之後，請 disable 。Run 及 Debug 完畢之後，才 Enable 。
                //toolStripMenuItemRelease.Enabled = false;
                //toolStripMenuItemDebug.Enabled = false;
                //debugToolStripMenuItem.Enabled = false;
                /*
                StringBuilder workSpacePathBuffer = new StringBuilder(workSpacePath + "\\", R7.STRING_SIZE);
                int r7Sn = R7.New();

                R7.RegisterPrintfCallbackFunction(r7Sn, R7.r7PrintfCallbackFunctionDelegate);

                //20170314 leo: 發現之前的方式會機率性當機， byte array 讀檔案，最後一格要補 0 
                //http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
                FileStream sourceFile = new FileStream(programFilePath, FileMode.Open);
                BinaryReader binReader = new BinaryReader(sourceFile);
                byte[] programFileBytes = new byte[sourceFile.Length + 1];
                for (long i = 0; i < sourceFile.Length; i++)
                {
                    programFileBytes[i] = binReader.ReadByte();
                }
                programFileBytes[sourceFile.Length] = 0;
                sourceFile.Close();
                binReader.Close();
                R8.clearLogBox();
                //20170330 leo: 關於 "在顯示 Image 時就顯示 ReleasePrint" 的修改，下午接到指示暫時取消，這邊架構先留著，從 R7 那邊加開關，關閉這個功能
                int res = R7.Run(r7Sn, programFileBytes, new Byte[] { 0 }, GetBytes(workSpacePathBuffer.ToString()), 0);

                res = R7.Release(r7Sn);
                workSpacePathBuffer.Clear();
                */


                //20170817 leo: 今天討論後，這邊要改成產生 bat 、運行該 .bat 檔案
                string selectedFileName;
                string batFilePath;
                string batFileName;
                selectedFileName = programFilePath.Substring(programFilePath.LastIndexOf("\\") + 1); // xxx.r6 檔案名稱
                //batFileName = selectedFileName.Substring(0, selectedFileName.LastIndexOf(".")) + ".bat";
                batFileName = programFilePath.Substring(0, programFilePath.LastIndexOf(".")) + ".bat";

                batFilePath = programFilePath.Substring(0, programFilePath.LastIndexOf("\\"));
                //R7 路徑，基本上就是 R8 路徑
                string r7Path = System.Windows.Forms.Application.StartupPath + "\\" + "R7.exe";
                //recipe 路徑就是 workSpacePath ，

                //programFilePath = saveFileDialog1.FileName;
                //workSpacePath = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf("\\"));



                string r7RelativePath = R8.GetRelativePath(batFilePath + "\\", r7Path);
                //recipe 之 workSpace 的相對路徑格式
                //string r6RelativePath = "\\" + GetRelativePath(batFilePath + "\\", workSpacePath);

                //string r6RelativePath = R8.GetRelativePath(r7Path, workSpacePath) + "\\";
                string r6RelativePath = R8.GetRelativePath(batFilePath + "\\", workSpacePath);
                //string r6RelativePath = R8.GetRelativePath(batFilePath + "\\", workSpacePath + "\\");


                //string r6RelativePath = R8.GetRelativePath(r7Path + "\\", workSpacePath);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("chcp 65001");
                sb.AppendLine("cls");//避免運行時出現亂碼
                if (isRelease)
                {
                    sb.AppendLine("\"" + r7RelativePath + "\" \"release\" \"" + r6RelativePath + "\" \"" + selectedFileName + "\" \"null\"");
                    sb.AppendLine("pause");
                }
                else
                {
                    sb.AppendLine("\"" + r7RelativePath + "\" \"debug\" \"" + r6RelativePath + "\" \"" + selectedFileName + "\" \"null\"");
                    sb.AppendLine("pause");
                }

                //MessageBox.Show("Export: " + sb.ToString());
                //使用 UTF8 編碼，有中文時會讀不了檔案？總之先用big5 -> 晚上討論後先保留 UTF8，先讓英文路徑可以讀就好。
                Encoding utf8WithoutBom = new UTF8Encoding(false);
                using (StreamWriter sw = new StreamWriter(batFileName, false, utf8WithoutBom))
                //using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.GetEncoding("big5")))
                {
                    sw.Write(sb.ToString());
                    sw.Close();
                }
                sb.Clear();
                //20170817 leo: 直接運行 .bat 會遇到路徑問題(相對路徑讀不到)
                //參照 https://social.msdn.microsoft.com/Forums/zh-TW/046e6953-0312-47ba-b206-abaf616a7931/c-formcmdexe?forum=233
                //要設 WorkingDirectory
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = batFileName;
                info.WorkingDirectory = batFilePath;
                Process process = new Process();
                process.StartInfo = info;
                /*
                process.EnableRaisingEvents = true; //設定是否應該在處理序終止時引發 Exited 事件。
                process.Exited += new EventHandler(runR7Process_Exited);
                */
                /*
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += new DataReceivedEventHandler((dataSender, dataEvent) =>
                {
                    if (!String.IsNullOrEmpty(dataEvent.Data))
                    {

                        this.Invoke((MethodInvoker)delegate
                        {
                            R8.addLogToLogBox(dataEvent.Data.ToString());
                            R8.addLogToLogBox("next..");
                        });
                    }
                });
               */
                process.Start();

            }

        }

        private void toolStripMenuItemRun_Click(object sender, EventArgs e)
        {
            runR6UseExe_Click(sender, e, true);
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runR6UseExe_Click(sender, e, false);
        }

        private void runR7Process_Exited(object sender, EventArgs e)
        {

            this.Invoke((MethodInvoker)delegate
            {
                //toolStripMenuItemRun.Enabled = true;
                //toolStripMenuItemDebug.Enabled = true;
            });

        }

        private void backup_toolStripMenuItemRun_Click(object sender, EventArgs e)
        {
            if (programFilePath == null)
            {
                //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                //加個警示訊息
                MessageBox.Show("Please save program before run it.");
                return;
            }
            //20170202 leo : Run 時自動 save
            saveToolStripMenuItem_Click(sender, e);

            //20170206 leo: 新版換成直接呼叫 dll
            //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
            if (programFilePath != null)
            {
                //20170330 上方的按鈕在按下 Run 及 Debug 之後，請 disable 。Run 及 Debug 完畢之後，才 Enable 。
                toolStripMenuItemRun.Enabled = false;
                //debugToolStripMenuItem.Enabled = false;
                StringBuilder workSpacePathBuffer = new StringBuilder(workSpacePath + "\\", R7.STRING_SIZE);
                int r7Sn = R7.New();

                R7.RegisterPrintfCallbackFunction(r7Sn, R7.r7PrintfCallbackFunctionDelegate);

                //20170314 leo: 發現之前的方式會機率性當機， byte array 讀檔案，最後一格要補 0 
                //http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
                FileStream sourceFile = new FileStream(programFilePath, FileMode.Open);
                BinaryReader binReader = new BinaryReader(sourceFile);
                byte[] programFileBytes = new byte[sourceFile.Length + 1];
                for (long i = 0; i < sourceFile.Length; i++)
                {
                    programFileBytes[i] = binReader.ReadByte();
                }
                programFileBytes[sourceFile.Length] = 0;
                sourceFile.Close();
                binReader.Close();
                R8.clearLogBox();
                //20170330 leo: 關於 "在顯示 Image 時就顯示 ReleasePrint" 的修改，下午接到指示暫時取消，這邊架構先留著，從 R7 那邊加開關，關閉這個功能
                int res = R7.Run_Internal(r7Sn, programFileBytes, new Byte[] { 0 }, GetBytes(workSpacePathBuffer.ToString()), 0);
                //if (res > 0)
                //{
                //R8.logToLogBox(resultBuffer.ToString());
                //R7.ShowImage(r7Sn);
                //}
                res = R7.Release(r7Sn);
                workSpacePathBuffer.Clear();
                toolStripMenuItemRun.Enabled = true;
                //debugToolStripMenuItem.Enabled = true;
            }

        }


        private void backup_debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (programFilePath == null)
            {
                //這邊如果[未開啟/儲存過檔案]，很容易操作錯誤
                //(點選 Run -> 觸發 saveAs -> 以為是 open 檔案 -> 然後就把之前辛苦編輯好的 program 蓋掉了)
                //加個警示訊息
                MessageBox.Show("Please save program before run it.");
                return;
            }
            //20170202 leo : Run 時自動 save
            saveToolStripMenuItem_Click(sender, e);

            //20170206 leo: 新版換成直接呼叫 dll
            //StringBuilder str = new StringBuilder(R7.RESULT_SIZE);
            if (programFilePath != null)
            {
                //20170330 上方的按鈕在按下 Run 及 Debug 之後，請 disable 。Run 及 Debug 完畢之後，才 Enable 。
                toolStripMenuItemDebug.Enabled = false;
                //debugToolStripMenuItem.Enabled = false;
                StringBuilder workSpacePathBuffer = new StringBuilder(workSpacePath + "\\", R7.STRING_SIZE);
                int r7Sn = R7.New();

                R7.RegisterPrintfCallbackFunction(r7Sn, R7.r7PrintfCallbackFunctionDelegate);

                //20170314 leo: 發現之前的方式會機率性當機， byte array 讀檔案，最後一格要補 0 
                //http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
                FileStream sourceFile = new FileStream(programFilePath, FileMode.Open);
                BinaryReader binReader = new BinaryReader(sourceFile);
                byte[] programFileBytes = new byte[sourceFile.Length + 1];
                for (long i = 0; i < sourceFile.Length; i++)
                {
                    programFileBytes[i] = binReader.ReadByte();
                }
                programFileBytes[sourceFile.Length] = 0;
                sourceFile.Close();
                binReader.Close();
                R8.clearLogBox();
                //20170330 leo: 關於 "在顯示 Image 時就顯示 ReleasePrint" 的修改，下午接到指示暫時取消，這邊架構先留著，從 R7 那邊加開關，關閉這個功能
                int result = R7.Run_Internal(r7Sn, programFileBytes, new Byte[] { 0 }, GetBytes(workSpacePathBuffer.ToString()), 1);

                //20170427: 關於 C++ API 的測試
                //if (false) {
                //    int intVariable = 0;
                //    float floatVariable = 0.0f;
                //    bool boolVariable = false;
                //    //R7.GetVariableValue(r7h, 0, 0, ref intVariable);
                //    R7.GetVariableInt(1, 1, ref intVariable);
                //    MessageBox.Show("intVariable = " + intVariable);
                //    R7.SetVariableInt(1, 1, 100);
                //    R7.GetVariableInt(1, 1, ref intVariable);
                //    MessageBox.Show("(after)intVariable = " + intVariable);
                //}
                //if (result > 0)
                //{
                //R8.logToLogBox(resultBuffer.ToString());
                //R7.ShowImage(r7Sn);
                //}
                result = R7.Release(r7Sn);
                workSpacePathBuffer.Clear();
                toolStripMenuItemDebug.Enabled = true;
                //debugToolStripMenuItem.Enabled = true;
            }

        }

        private void sortAndSaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //20170808 leo: 由於 recipe 檔案大了會很亂，加入整理並另存功能
            //1. 把 function 依照 posY 重新排序
            //2. 把 variable 依照 name 重新排序
            //(討論後取消)3. 把未在任何 function 中使用到的 variable 移除

            //20170815 leo: Kelly 回報 Function_Enable 以及 Function_EnableList 在排序後會異常。
            //因為這兩個 function 有使用到 FunctionSn 作為 Variable ，排序時必須對該 Variable 另外進行處理。
            if (formVariable != null)
            {
                formVariable.setVariable();
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Open Robot Language File|*.r6;*.xml";
            saveFileDialog1.Title = "Sort And Save As";
            saveFileDialog1.InitialDirectory = workSpacePath;
            saveFileDialog1.FileName = defaultFileName;
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                int i, j;
                List<Function> sortFunctions = new List<Function>();// = r8.functions.ToList();
                Function function;
                List<Variable> sortVariables = new List<Variable>();
                Variable variable;
                for (i = 0; i <= r8.getFunctionSnMax(); i++)
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
                    sortFunctions.Add(function);
                }
                sortFunctions.Sort((x, y) => { return x.posY.CompareTo(y.posY); });

                for (i = 0; i <= r8.getVariableSnMax(); i++)
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
                    sortVariables.Add(variable);
                }
                sortVariables.Sort((x, y) => { return x.name.CompareTo(y.name); });

                //排序後，各 function 之對應的 variablesSn 會變 
                int[] sortVariablesSn = new int[R7.VARIABLES_SIZE];
                for (i = 0; i < sortVariables.Count; i++)
                {
                    //function varibable 都是從1開始，所以轉換時要 +1
                    variable = sortVariables.ElementAt(i);
                    sortVariablesSn[variable.sn] = i + 1;
                }

                int[] sortFunctionsSn = new int[R7.VARIABLES_SIZE];
                for (i = 0; i < sortFunctions.Count; i++)
                {
                    //function varibable 都是從1開始，所以轉換時要 +1
                    function = sortFunctions.ElementAt(i);
                    sortFunctionsSn[function.sn] = i + 1;
                }

                int targetSn = 0;

                for (i = 0; i < sortFunctions.Count; i++)
                {

                    function = sortFunctions.ElementAt(i);

                    if (function.name != null)
                    {
                        if (function.name.Equals("Function_Enable") ||
                            function.name.Equals("FunctionEnable") ||
                            function.name.Equals("IntentEnable"))
                        {
                            if (function.parameters.Count > 1)
                            {
                                targetSn = function.parameters.ElementAt(1).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    //variable = FormMain.r8.variables[function.parameters.ElementAt(1).variableSn];
                                    //variable = FormMain.r8.variables[targetSn];
                                    //MessageBox.Show("pre getValue: " + variable.sn + variable.name + variable.value);
                                    //MessageBox.Show("sortVariablesSn: " + sortVariablesSn[function.parameters.ElementAt(1).variableSn]);

                                    //比對測試後，要轉換的目標 Value 應該是這個
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    //MessageBox.Show("sortVariables Value: " + variable.sn + variable.name + variable.value);
                                    //後半要用原始 value ，避免[同一個 variable 用在兩個以上的 Function_Enable]導致錯誤
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }
                            }
                        }
                        else if (function.name.Equals("Function_EnableList") ||
                           function.name.Equals("FunctionEnableList") ||
                           function.name.Equals("IntentEnableList"))
                        {
                            if (function.parameters.Count > 2)
                            {
                                targetSn = function.parameters.ElementAt(1).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    //variable.value = sortFunctionsSn[Convert.ToInt32(variable.value)].ToString();
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }

                                targetSn = function.parameters.ElementAt(2).variableSn;
                                if (targetSn != -1 && targetSn != 0)
                                {
                                    variable = sortVariables.ElementAt(sortVariablesSn[targetSn] - 1);
                                    variable.value = sortFunctionsSn[Convert.ToInt32(FormMain.r8.variables[targetSn].value)].ToString();
                                }
                            }
                        }
                    }

                    for (j = 0; j < function.parameters.Count; j++)
                    {
                        if (function.parameters.ElementAt(j).variableSn != -1 && function.parameters.ElementAt(j).variableSn != 0)
                        {
                            function.parameters.ElementAt(j).variableSn = sortVariablesSn[function.parameters.ElementAt(j).variableSn];
                        }
                    }
                }

                //然後把排序好的東西存回去
                r8 = new R8();
                for (i = 0; i < sortFunctions.Count; i++)
                {
                    function = sortFunctions.ElementAt(i);

                    r8.addFunction(function);
                }

                for (i = 0; i < sortVariables.Count; i++)
                {
                    variable = sortVariables.ElementAt(i);
                    r8.addVariable(variable);
                }

                deleteTempR6File();//20171115 add
                programFilePath = saveFileDialog1.FileName;
                workSpacePath = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf("\\"));
                //R7.LoadLibrary(workSpacePath); //項目 [開啟 .r6 檔案時，不重新載入 library 裡面的 dll 。]
                reloadToolBox();
                R8.writeProgramXml(saveFileDialog1.FileName);

                //存檔後把排序後的檔案打開
                if (formFunctions == null)
                {
                    loadForms();
                }
                if (formVariable != null)
                {
                    formVariable.variableSn = -1;
                    formVariable.showVariable(-1);
                }

                formFunctions.readProgramXml(saveFileDialog1.FileName);

                int shortFileNameLength = saveFileDialog1.FileName.Length - saveFileDialog1.FileName.LastIndexOf("\\") - 1;
                if (shortFileNameLength > 0)
                {
                    programFileName = saveFileDialog1.FileName.Substring(saveFileDialog1.FileName.LastIndexOf("\\") + 1, shortFileNameLength);
                }
                else
                {
                    programFileName = programFilePath;
                }

                string str = formMainTitle + "  -  " + programFileName;
                this.Text = str;
                toolStripWorkSpaceTextBox.Text = workSpacePath;
                Program.isFileChange = false;
                //MessageBox.Show("isFileChange = false");
            }
        }



        private void exportToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //20170808 產生 .bat
            if (programFilePath == null)
            {
                MessageBox.Show("Please save file before export.");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "batch file|*.bat";
            saveFileDialog1.Title = "Export to Windows batch file";
            saveFileDialog1.InitialDirectory = workSpacePath;
            string selectedFileName = programFilePath.Substring(programFilePath.LastIndexOf("\\") + 1); // xxx.r6 檔案名稱
            saveFileDialog1.FileName = selectedFileName.Substring(0, selectedFileName.LastIndexOf(".")) + ".bat";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //相對路徑...首先抓出各自的絕對路徑
                //要產生的 bat 的資料夾之路徑
                string batFilePath = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf("\\"));
                //R7 路徑，基本上就是 R8 路徑
                string r7Path = System.Windows.Forms.Application.StartupPath + "\\" + "R7.exe";
                //recipe 路徑就是 workSpacePath ，

                //programFilePath = saveFileDialog1.FileName;
                //workSpacePath = saveFileDialog1.FileName.Substring(0, saveFileDialog1.FileName.LastIndexOf("\\"));



                string r7RelativePath = R8.GetRelativePath(batFilePath + "\\", r7Path);
                //recipe 之 workSpace 的相對路徑格式
                //string r6RelativePath = "\\" + GetRelativePath(batFilePath + "\\", workSpacePath);

                //string r6RelativePath = R8.GetRelativePath(r7Path, workSpacePath) + "\\";
                string r6RelativePath = R8.GetRelativePath(batFilePath + "\\", workSpacePath);
                //string r6RelativePath = R8.GetRelativePath(batFilePath + "\\", workSpacePath + "\\");
                //string r6RelativePath = R8.GetRelativePath(r7Path + "\\", workSpacePath);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("chcp 65001");
                sb.AppendLine("cls");//避免運行時出現亂碼
                sb.AppendLine("::\"" + r7RelativePath + "\" \"release\" \"" + r6RelativePath + "\" \"" + selectedFileName + "\" \"null\"");
                sb.AppendLine("\"" + r7RelativePath + "\" \"debug\" \"" + r6RelativePath + "\" \"" + selectedFileName + "\" \"null\"");
                sb.AppendLine("pause");
                //MessageBox.Show("Export: " + sb.ToString());
                //使用 UTF8 編碼，有中文時會讀不了檔案？總之先用big5 -> 晚上討論後先保留 UTF8，先讓英文路徑可以讀就好。
                Encoding utf8WithoutBom = new UTF8Encoding(false);
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, utf8WithoutBom))
                //using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.GetEncoding("big5")))
                {
                    sw.Write(sb.ToString());
                    sw.Close();
                }

                /*
                //20170817 leo: 直接運行 .bat 會遇到路徑問題(相對路徑讀不到)
                //參照 https://social.msdn.microsoft.com/Forums/zh-TW/046e6953-0312-47ba-b206-abaf616a7931/c-formcmdexe?forum=233
                //要設 WorkingDirectory
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = saveFileDialog1.FileName;
                info.WorkingDirectory = batFilePath;
                Process process = Process.Start(info);
                */
            }
        }

        private void exportCFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (true)
            {
                exportCFileToolStripMenuItem_Click4(sender, e);
                return;
            }
            /*
            if (programFilePath == null)
            {
                MessageBox.Show("Please save file before export.");
                return;
            }
            */

            //20171006 leo: 產生 main.cpp
            //存檔路徑暫定與 .r6 同層，檔名 main.cpp....但這樣容易互相覆蓋，開個資料夾好了

            /*
            string dirname = programFilePath.Substring(programFilePath.LastIndexOf("\\") + 1);
            dirname = dirname.Substring(0, dirname.LastIndexOf("."));
            string dir = workSpacePath + "\\" + dirname;
            System.IO.Directory.CreateDirectory(dir);
            string savePath = dir + "\\" + "main.cpp";
            */

            /*
             格式：
                #include <r7.hpp>

                int main () {

                int r7h = R7_New();

                // Variables

                R7_AddVariable(r7h, "object", "TimerObject", NULL);
                // [remark]

                R7_AddVariable(r7h, "double", "Microsecond_1", "123");
                // [remark]


                // Functions

                R7_AddFunction(r7h, "Timer_Reset", "v1");
                // [remark]
                // posY = 8
                // enable = 0
                // sn = 123

                R7_Run(r7h);

                R7_Free(r7h);

                }
             */

            /*
             初版先按照昨天明達信件中寫的格式，做完後如果有時間再做可以運行的版本。
             */

            /*
            int i, j;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#include <r7.hpp>");
            sb.Append("\r\n\r\n\r\n\r\n");
            sb.AppendLine("int main (void) {");
            sb.Append("\r\n\r\n");
            sb.AppendLine("    int r7h = R7_New();");
            sb.Append("\r\n");
            sb.AppendLine("    // Variables");
            sb.Append("\r\n");
            for (i = 0; i < r8.getVariableSnLast(); i++)
            {
                if (r8.variables[i] != null)
                {
                    if (r8.variables[i].value != null)
                    {
                        //依昨晚討論，空格要變底線符號。然後 name 或 value 可能有冒號，要改為斜線冒號
                        sb.AppendLine("    R7_AddVariable(r7h, \"" + r8.variables[i].name.Replace(" ", "_").Replace("\"", "\\\"") + "\", \"" + r8.variables[i].type + "\", \"" + r8.variables[i].value.Replace("\"", "\\\"") + "\");");
                    }
                    else
                    {
                        sb.AppendLine("    R7_AddVariable(r7h, \"" + r8.variables[i].name.Replace(" ", "_").Replace("\"", "\\\"") + "\", \"" + r8.variables[i].type + "\", NULL);");
                    }
                    if (r8.variables[i].remark != null && r8.variables[i].remark.Length > 0)
                    {
                        sb.AppendLine("    // [remark]" + r8.variables[i].remark);
                    }
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            sb.AppendLine("    // Functions");
            sb.Append("\r\n");
            for (i = 0; i < r8.getFunctionSnLast(); i++)
            {
                if (r8.functions[i] != null)
                {
                    sb.Append("    R7_AddFunction(r7h, \"" + r8.functions[i].name.Replace(" ", "_").Replace("\"", "\\\"") + "\"");
                    //, \"v1\"
                    for (j = 0; j < r8.functions[i].parameters.Count; j++)
                    {
                        //如果沒有 variable 塞 null ，如果有的話塞 variable name
                        if (r8.functions[i].parameters[j].variableSn != -1 && r8.functions[i].parameters[j].variableSn != 0 && r8.variables[r8.functions[i].parameters[j].variableSn] != null)
                        {
                            sb.Append(", \"" + r8.variables[r8.functions[i].parameters[j].variableSn].name.Replace(" ", "_").Replace("\"", "\\\"") + "\"");

                        }
                        else
                        {

                            sb.Append(", NULL");

                        }
                    }
                    sb.AppendLine(");");
                    if (r8.functions[i].remark != null && r8.functions[i].remark.Length > 0)
                    {
                        sb.AppendLine("    // [remark]" + r8.functions[i].remark);
                    }
                    sb.AppendLine("    // posY = " + r8.functions[i].posY);
                    sb.AppendLine("    // enable = " + r8.functions[i].enable);
                    sb.AppendLine("    // sn = " + r8.functions[i].sn);
                    sb.Append("\r\n");
                }
            }

            sb.Append("\r\n");
            sb.AppendLine("    R7_Run(r7h);");
            sb.Append("\r\n");
            sb.AppendLine("    R7_Free(r7h);");
            sb.Append("\r\n");
            sb.AppendLine("    return 1;");
            sb.AppendLine("}");
            sb.Append("\r\n");
            //從 exportToolStripMenuItem1_Click 留下來的轉 UTF8
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            using (StreamWriter sw = new StreamWriter(savePath, false, utf8WithoutBom))
            {
                sw.Write(sb.ToString());
                sw.Close();
            }
            MessageBox.Show("Export: " + savePath);
            */
        }

        //20171006 exportCFileToolStripMenuItem_Click 版本2: 這邊的目標是出來的 main.cpp 可以編譯
        //要能編譯，所以 R7 那邊要添加對應的 function 來接。
        /*
        R7_API int R7_Run2(int r7Sn, int isDebug);
	    R7_API int R7_SetWorkspacePath2(int r7Sn, char *workspacePath);
	    R7_API int R7_AddVariable(int r7Sn, int variableSn, char *name, char *type, char *value);
	    R7_API int R7_AddFunction(int r7Sn, int functionSn, char *name, int posY, bool enable);
	    R7_API int R7_FunctionAddVariableSn(int r7Sn, int functionSn, int variableSn);
         */
        private void exportCFileToolStripMenuItem_Click2(object sender, EventArgs e)
        {


            if (programFilePath == null)
            {
                MessageBox.Show("Please save file before export.");
                return;
            }

            //20171006 leo: 產生 main.cpp
            //存檔路徑暫定與 .r6 同層，檔名 main.cpp....但這樣容易互相覆蓋，開個資料夾好了

            string dirname = programFilePath.Substring(programFilePath.LastIndexOf("\\") + 1);
            dirname = dirname.Substring(0, dirname.LastIndexOf("."));
            string dir = workSpacePath + "\\" + dirname + "_B";
            System.IO.Directory.CreateDirectory(dir);
            string savePath = dir + "\\" + "main.cpp";

            /*
             格式類似 R7B 的 main.cpp
            int res;
            res = R7_InitLib();
            int r7h = R7_New();

            R7_SetWorkspacePath(r7h, workspacePath);

            R7_AddVariable(); //n個

            R7_AddFunction(); //n個
            R7_SetVariableToFunction(); //n個


            //然後要新版 R7 Run (不塞 program variable 的版本)
            res = R7_Run(isDebug);
            
            res = R7_Release(r7h);

            res = R7_CloseLib();
             */


            int i, j;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#include <r7.hpp>");
            sb.AppendLine("#include <tchar.h>");

            sb.Append("\r\n\r\n\r\n\r\n");
            sb.AppendLine("int _tmain(int argc, wchar_t* argv[]){");
            sb.Append("\r\n\r\n");
            sb.AppendLine("    int res;");
            sb.AppendLine("    res = R7_InitLib();");
            sb.Append("\r\n");
            sb.AppendLine("    int r7h = R7_New();");
            sb.Append("\r\n");
            sb.AppendLine("    R7_SetWorkspacePath2(r7h, \"" + workSpacePath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");

            sb.AppendLine("    // Variables");
            sb.Append("\r\n");

            StringBuilder tempTypeString = new StringBuilder(R7.STRING_SIZE);


            for (i = 0; i < r8.getVariableSnLast(); i++)
            {
                if (r8.variables[i] != null)
                {
                    if (r8.variables[i].value != null)
                    {
                        //R7_AddVariable(int r7Sn, int variableSn, char *name, char *type, char *value);
                        R8.GetVariableType(tempTypeString, R7.STRING_SIZE, r8.variables[i].type);
                        sb.AppendLine("    R7_AddVariable(r7h, " + r8.variables[i].sn + ", \"" + r8.variables[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"" + tempTypeString + "\", \"" + r8.variables[i].value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");
                    }
                    else
                    {
                        R8.GetVariableType(tempTypeString, R7.STRING_SIZE, r8.variables[i].type);
                        sb.AppendLine("    R7_AddVariable(r7h, " + r8.variables[i].sn + ", \"" + r8.variables[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"" + tempTypeString + "\", NULL);");
                    }
                    if (r8.variables[i].remark != null && r8.variables[i].remark.Length > 0)
                    {
                        sb.AppendLine("    // [remark]" + r8.variables[i].remark);
                    }
                    sb.Append("\r\n");
                }
            }
            sb.Append("\r\n");
            sb.AppendLine("    // Functions");
            sb.Append("\r\n");
            for (i = 0; i < r8.getFunctionSnLast(); i++)
            {
                if (r8.functions[i] != null)
                {
                    //R7_AddFunction(int r7Sn, int functionSn, char *name, int posY, bool enable);
                    sb.AppendLine("    R7_AddFunction(r7h, " + r8.functions[i].sn + ", \"" + r8.functions[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", " +
                        (int)(r8.functions[i].posY) + ", " + r8.functions[i].enable.ToString().ToLower() + ");");
                    //, \"v1\"
                    for (j = 0; j < r8.functions[i].parameters.Count; j++)
                    {
                        if (r8.functions[i].parameters[j].variableSn != -1 && r8.functions[i].parameters[j].variableSn != 0 && r8.variables[r8.functions[i].parameters[j].variableSn] != null)
                        {
                            // R7_FunctionAddVariableSn(int r7Sn, int functionSn, int variableSn);
                            sb.AppendLine("    R7_FunctionAddVariableSn(r7h, " + r8.functions[i].sn + ", " + r8.functions[i].parameters[j].variableSn + ");");
                        }
                        else
                        {
                            sb.AppendLine("    R7_FunctionAddVariableSn(r7h, " + r8.functions[i].sn + ", 0);");
                        }
                    }
                    if (r8.functions[i].remark != null && r8.functions[i].remark.Length > 0)
                    {
                        sb.AppendLine("    // [remark]" + r8.functions[i].remark);
                    }
                    sb.Append("\r\n");
                }
            }

            sb.Append("\r\n");
            sb.AppendLine("    R7_Run2(r7h, 1);");
            sb.Append("\r\n");
            sb.AppendLine("    R7_Release(r7h);");
            sb.Append("\r\n");
            sb.AppendLine("    return 1;");
            sb.AppendLine("}");
            sb.Append("\r\n");
            //從 exportToolStripMenuItem1_Click 留下來的轉 UTF8
            Encoding utf8WithoutBom = new UTF8Encoding(false);
            using (StreamWriter sw = new StreamWriter(savePath, false, utf8WithoutBom))
            {
                sw.Write(sb.ToString());
                sw.Close();
            }
            MessageBox.Show("Export to " + savePath);

        }

        //20171006 leo: 下午接到指示，改成[準備一個 vs 模板，然後直接預設把 main.cpp 存入該模板裡面]
        //另外還要 R7_SetWorkspacePath2 與 R7_Run2 拿掉
        //20171011 leo: 增加 utf8 支援。改動最小的方式應該是字串前面加 u8 ，然後 cpp 存檔成 UTF8 With BOM


        private void exportCFileToolStripMenuItem_Click3(object sender, EventArgs e)
        {
            //200171012 leo: 今天明達交代 sn 為空時， -1 改 0 。
            //支援舊版 .r6 ，這邊讀到 -1 也都設 0
            if (programFilePath == null)
            {
                MessageBox.Show("Please save file before export.");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "cpp file|*.cpp";
            saveFileDialog1.Title = "Export to Visual C++ file";
            saveFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath + "\\src\\VisualStudioTemplate";
            saveFileDialog1.FileName = "main.cpp";

            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int i, j;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("#include <r7.hpp>");
                sb.AppendLine("#include <tchar.h>");

                sb.Append("\r\n\r\n\r\n\r\n");
                sb.AppendLine("//Export from " + programFilePath);
                sb.AppendLine("int _tmain(int argc, wchar_t* argv[]){");
                sb.Append("\r\n\r\n");
                sb.AppendLine("    SetConsoleOutputCP(65001);");
                sb.AppendLine("    setlocale(LC_ALL, \"en_US.UTF-8\");");
                sb.Append("\r\n\r\n");
                sb.AppendLine("    int isPause = 1;");
                sb.AppendLine("    int isDebug = 1;");
                sb.AppendLine("    int res;");
                sb.AppendLine("    res = R7_InitLib();");
                sb.Append("\r\n");
                sb.AppendLine("    int r7h = R7_New();");
                sb.Append("\r\n");
                //sb.AppendLine("    R7_SetWorkspacePath2(r7h, \"" + workSpacePath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");
                //R7_SetWorkspacePath(int r7Sn, char *path, int pathStrSize)
                sb.AppendLine("    R7_SetWorkspacePath(r7h, u8\"" + workSpacePath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\\\\\", 2048);");
                sb.Append("\r\n\r\n");
                sb.AppendLine("    // Variables");
                sb.Append("\r\n");

                StringBuilder tempTypeString = new StringBuilder(R7.STRING_SIZE);


                for (i = 0; i < r8.getVariableSnLast(); i++)
                {
                    if (r8.variables[i] != null)
                    {
                        if (r8.variables[i].value != null)
                        {
                            //R7_AddVariable(int r7Sn, int variableSn, char *name, char *type, char *value);
                            R8.GetVariableType(tempTypeString, R7.STRING_SIZE, r8.variables[i].type);
                            sb.AppendLine("    R7_AddVariable(r7h, " + r8.variables[i].sn + ", u8\"" + r8.variables[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"" + tempTypeString + "\", u8\"" + r8.variables[i].value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");
                        }
                        else
                        {
                            R8.GetVariableType(tempTypeString, R7.STRING_SIZE, r8.variables[i].type);
                            sb.AppendLine("    R7_AddVariable(r7h, " + r8.variables[i].sn + ", u8\"" + r8.variables[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"" + tempTypeString + "\", NULL);");
                        }
                        if (r8.variables[i].remark != null && r8.variables[i].remark.Length > 0)
                        {
                            sb.AppendLine("    // [remark]" + r8.variables[i].remark);
                        }
                        sb.Append("\r\n");
                    }
                }
                sb.Append("\r\n");
                sb.AppendLine("    // Functions");
                sb.Append("\r\n");
                for (i = 0; i < r8.getFunctionSnLast(); i++)
                {
                    if (r8.functions[i] != null)
                    {
                        //R7_AddFunction(int r7Sn, int functionSn, char *name, int posY, bool enable);
                        sb.AppendLine("    R7_AddFunction(r7h, " + r8.functions[i].sn + ", u8\"" + r8.functions[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", " +
                            (int)(r8.functions[i].posY) + ", " + r8.functions[i].enable.ToString().ToLower() + ");");
                        //, \"v1\"
                        for (j = 0; j < r8.functions[i].parameters.Count; j++)
                        {
                            if (r8.functions[i].parameters[j].variableSn != -1 && r8.functions[i].parameters[j].variableSn != 0 && r8.variables[r8.functions[i].parameters[j].variableSn] != null)
                            {
                                // R7_FunctionAddVariableSn(int r7Sn, int functionSn, int variableSn);
                                sb.AppendLine("    R7_AddFunctionVariable(r7h, " + r8.functions[i].sn + ", " + r8.functions[i].parameters[j].variableSn + ");");
                            }
                            else
                            {
                                //sb.AppendLine("    R7_AddFunctionVariable(r7h, " + r8.functions[i].sn + ", -1);");
                                sb.AppendLine("    R7_AddFunctionVariable(r7h, " + r8.functions[i].sn + ", 0);");
                            }
                        }
                        if (r8.functions[i].remark != null && r8.functions[i].remark.Length > 0)
                        {
                            sb.AppendLine("    // [remark]" + r8.functions[i].remark);
                        }
                        sb.Append("\r\n");
                    }
                }

                sb.Append("\r\n");
                sb.AppendLine("    R7_Run(r7h, NULL, NULL, NULL, isDebug);");
                sb.Append("\r\n");
                sb.AppendLine("    R7_Release(r7h);");
                sb.Append("\r\n");

                //sb.AppendLine("    if (isPause == 1)");
                //sb.AppendLine("    {");
                sb.AppendLine("        getchar();");
                //sb.AppendLine("    }");
                sb.Append("\r\n");
                sb.AppendLine("    return 0;");
                sb.AppendLine("}");
                sb.Append("\r\n");
                //從 exportToolStripMenuItem1_Click 留下來的轉 UTF8
                //20171011 leo: 實測結果，要存成 utf8WithBom， 用 visual studio 開啟時中文路徑才會正常
                Encoding utf8WithBom = new UTF8Encoding(true);
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, utf8WithBom))
                {
                    sw.Write(sb.ToString());
                    sw.Close();
                }
                //MessageBox.Show("Export to " + saveFileDialog1.FileName);
            }
        }


        //20171016 早上討論後， Export to Visual C++ file 格式要簡化。
        //R7_SetWorkspacePath 移除
        //R7_AddVariable 參數不傳 Sn ， Sn 由程式自動給
        //R7_AddFunction 也變成沒有 Sn 了，另外參數改為類似 Printf 的方式，把 Variable都傳進去
        //其他小項目(例如 isPause isDebug 移除)
        private void exportCFileToolStripMenuItem_Click4(object sender, EventArgs e)
        {
            if (programFilePath == null)
            {
                MessageBox.Show("Please save file before export.");
                return;
            }

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "cpp file|*.cpp";
            saveFileDialog1.Title = "Export to Visual C++ file";
            saveFileDialog1.InitialDirectory = System.Windows.Forms.Application.StartupPath + "\\src\\VisualStudioTemplate";
            saveFileDialog1.FileName = "main.cpp";

            /*
             20171024 leo:
             R7_AddFunction 現在禁止傳遞 Variables
             然而目前 R7 本身沒有取得 Function 之 Variable 數量的方法。
             需要在 R7_InitLib 時要增加讀取 SupportList 然後把數量存下來的動作
             */
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int i, j;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("#include <r7.hpp>");
                //sb.AppendLine("#include <tchar.h>");

                sb.Append("\r\n\r\n\r\n\r\n");
                sb.AppendLine("//Export from " + programFilePath);
                //sb.AppendLine("int _tmain(int argc, wchar_t* argv[]) {");
                sb.AppendLine("int main(void) {");
                sb.Append("\r\n");
                sb.AppendLine("    SetConsoleOutputCP(65001);");
                sb.AppendLine("    setlocale(LC_ALL, \"en_US.UTF-8\");");
                sb.Append("\r\n");
                sb.AppendLine("    int res;");
                sb.AppendLine("    int r7h;");
                sb.Append("\r\n");
                sb.AppendLine("    res = R7_InitLib();");
                sb.AppendLine("    if (res <= 0) {");
                sb.AppendLine("        return 1;");
                sb.AppendLine("    }");
                sb.Append("\r\n");
                sb.AppendLine("    r7h = R7_New();");
                //sb.Append("\r\n");
                //sb.AppendLine("    R7_SetWorkspacePath(r7h, u8\"" + workSpacePath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\\\\\", 2048);");
                sb.Append("\r\n\r\n");
                sb.AppendLine("    // Variables");
                sb.Append("\r\n");

                StringBuilder tempTypeString = new StringBuilder(R7.STRING_SIZE);


                for (i = 0; i < r8.getVariableSnLast(); i++)
                {
                    if (r8.variables[i] != null)
                    {
                        R8.GetVariableType(tempTypeString, R7.STRING_SIZE, r8.variables[i].type);
                        //R7_AddVariable(r7h, u8"ImageFileName", "string", u8"sample.png");
                        if (r8.variables[i].value != null)
                        {
                            sb.AppendLine("    R7_AddVariable(r7h, u8\"" + r8.variables[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"" + tempTypeString + "\", u8\"" + r8.variables[i].value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\");");
                        }
                        else
                        {
                            sb.AppendLine("    R7_AddVariable(r7h, u8\"" + r8.variables[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"" + tempTypeString + "\", NULL);");
                        }
                        if (r8.variables[i].remark != null && r8.variables[i].remark.Length > 0)
                        {
                            sb.AppendLine("    // [remark]" + r8.variables[i].remark);
                        }
                        sb.Append("\r\n");
                    }
                }
                sb.Append("\r\n");
                sb.AppendLine("    // Functions");
                sb.Append("\r\n");
                //function 的 sn 與 pos 沒了，所以這邊一進去就需要排序過的 function
                List<Function> functionList = new List<Function>();
                Function function;
                for (i = 0; i < FormMain.r8.getFunctionSnLast(); i++)
                {
                    function = FormMain.r8.functions[i];
                    if (function == null)
                    {
                        continue;
                    }
                    functionList.Add(function);
                }
                functionList.Sort(delegate (Function i1, Function i2) { return i1.posY.CompareTo(i2.posY); });

                for (i = 0; i < functionList.Count; i++)
                {
                    //20171016 leo: 依早上討論， [非 Enable 的 Function 不輸出]，也就是 [Function_Enable] 與 [Function_EnableList] 失效
                    if (functionList[i] != null && functionList[i].enable)
                    {
                        //R7_AddFunction(r7h, u8"Image_Open", u8"ImageFileName");
                        //實作時還需要傳 variable 數量過去

                        //sb.Append("    R7_AddFunction(r7h, u8\"" + functionList[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"" + ", " + functionList[i].parameters.Count);
                        // 20171024 改成不能傳數量了
                        sb.Append("    R7_AddFunction(r7h, u8\"" + functionList[i].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                        //, \"v1\"
                        for (j = 0; j < functionList[i].parameters.Count; j++)
                        {
                            if (functionList[i].parameters[j].variableSn != -1 && functionList[i].parameters[j].variableSn != 0 && r8.variables[functionList[i].parameters[j].variableSn] != null)
                            {
                                //然後 Variable 傳 name 不傳 sn
                                sb.Append(", u8\"" + r8.variables[functionList[i].parameters[j].variableSn].name.Replace(" ", "_").Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                            }
                            else
                            {
                                sb.Append(", NULL");
                            }
                        }
                        sb.AppendLine(");");
                        if (functionList[i].remark != null && functionList[i].remark.Length > 0)
                        {
                            sb.AppendLine("    // [remark]" + functionList[i].remark);
                        }
                        sb.Append("\r\n");
                    }
                }
                sb.Append("\r\n");
                sb.AppendLine("    // Run");
                sb.Append("\r\n");
                //sb.AppendLine("    R7_Run(r7h, NULL, NULL, NULL, isDebug);");
                sb.AppendLine("    R7_Run(r7h, " + "u8\"" + workSpacePath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\\\\\", 1);");
                sb.Append("\r\n");
                sb.AppendLine("    R7_Release(r7h);");
                sb.Append("\r\n");

                //sb.AppendLine("    if (isPause == 1)");
                //sb.AppendLine("    {");
                //sb.AppendLine("        getchar();");
                //sb.AppendLine("    }");
                sb.AppendLine("    getchar();");
                sb.Append("\r\n");
                sb.AppendLine("    return 0;");
                sb.AppendLine("}");
                sb.Append("\r\n");
                //從 exportToolStripMenuItem1_Click 留下來的轉 UTF8
                //20171011 leo: 實測結果，要存成 utf8WithBom， 用 visual studio 開啟時中文路徑才會正常
                Encoding utf8WithBom = new UTF8Encoding(true);
                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, utf8WithBom))
                {
                    sw.Write(sb.ToString());
                    sw.Close();
                }
                //MessageBox.Show("Export to " + saveFileDialog1.FileName);
            }
        }

        //20170929 leo: Undo 與 Redo
        private void toolStripMenuItemUndo_Click(object sender, EventArgs e)
        {
            Program.recordUndo();
            FormMain.r8 = Program.loadProgramFromRecord();
            clearSearchBox();
            if (formFunctions == null)
            {
                loadForms();
            }
            if (formVariable != null)
            {
                formVariable.variableSn = -1;
                formVariable.showVariable(-1);
            }

            formFunctions.readProgramR8(FormMain.r8);
            Program.undoRedoButtonSetEnableOrDisable(toolStripMenuItemUndo, toolStripMenuItemRedo);
            return;
        }

        private void toolStripMenuItemRedo_Click(object sender, EventArgs e)
        {
            Program.recordRedo();
            FormMain.r8 = Program.loadProgramFromRecord();
            clearSearchBox();
            if (formFunctions == null)
            {
                loadForms();
            }
            if (formVariable != null)
            {
                formVariable.variableSn = -1;
                formVariable.showVariable(-1);
            }
            formFunctions.readProgramR8(FormMain.r8);
            Program.undoRedoButtonSetEnableOrDisable(toolStripMenuItemUndo, toolStripMenuItemRedo);
            return;
        }



        private void toolStripMenuItemLibrary_Click(object sender, EventArgs e)
        {
            if (formLibraryList != null)
            {
                return;
            }

            string librarysPath = System.Windows.Forms.Application.StartupPath + "\\library";
            if (Directory.Exists(librarysPath))
            {
                R8.StartMacroSupportList();
                string[] dirNames = Directory.GetDirectories(librarysPath);

                //加一個排序
                List<string> tempSortList = dirNames.ToList();
                tempSortList.Sort((x, y) => { return x.CompareTo(y); });
                dirNames = tempSortList.ToArray();

                formLibraryList = new FormLibraryList(dirNames);
                formLibraryList.MdiParent = this;
                formLibraryList.Location = new Point(formLibrary.Location.X + formLibrary.Width + formDistance, formLibrary.Location.Y);

                formLibraryList.Show();
                //R8.EndOfMacroSupportList();
            }
        }


        //20171016 拖曳.r6檔案到 R8 ，自動打開該檔案
        private void FormMain_DragEnter(object sender, DragEventArgs e)
        {
            //R8.logToLogBox("FormMain_DragEnter");
            e.Effect = DragDropEffects.All;
        }

        private void FormMain_DragDrop(object sender, DragEventArgs e)
        {
            //R8.logToLogBox("FormMain_DragDrop");
            String[] dragEnterData = (String[])e.Data.GetData(DataFormats.FileDrop);

            String targetFileName = "";
            if (dragEnterData != null)
            {
                //int i;
                //String str = "";
                //for (i = 0; i < dragEnterData.Length; i++) {
                //    str += dragEnterData[i];
                //}
                //MessageBox.Show("FormMain_DragDrop : " + str);
                if (dragEnterData.Length == 1)
                { //一次拉進來一個檔案時才算....一次拉n個時不執行動作
                    targetFileName = dragEnterData[0];
                    //檢查副檔名
                    int index = targetFileName.LastIndexOf('.');
                    if (index >= 0)
                    {
                        string subStr = targetFileName.Substring(index);
                        //MessageBox.Show("subStr = " + subStr);
                        if (subStr.CompareTo(".r6") == 0 || subStr.CompareTo(".xml") == 0)
                        {
                            if (formFunctions == null)
                            {
                                loadForms();
                            }
                            if (formVariable != null)
                            {
                                formVariable.variableSn = -1;
                                formVariable.showVariable(-1);
                            }


                            clearSearchBox();

                            formFunctions.readProgramXml(targetFileName);

                            deleteTempR6File();//20171115 add
                            programFilePath = targetFileName;
                            workSpacePath = targetFileName.Substring(0, targetFileName.LastIndexOf("\\"));

                            int shortFileNameLength = targetFileName.Length - targetFileName.LastIndexOf("\\") - 1;
                            if (shortFileNameLength > 0)
                            {
                                programFileName = targetFileName.Substring(targetFileName.LastIndexOf("\\") + 1, shortFileNameLength);
                            }
                            else
                            {
                                programFileName = programFilePath;
                            }
                            Program.clearRecord();
                            Program.recordProgram(FormMain.r8, toolStripMenuItemUndo, toolStripMenuItemRedo);
                            //Program.recordProgram(FormMain.r8);//leo: 第0步也要記錄，不然 undo 後會 redo 不回去

                            //R7.LoadLibrary(workSpacePath);//項目 [開啟 .r6 檔案時，不重新載入 library 裡面的 dll 。]
                            reloadToolBox();
                            //20170119 leo open 成功後設置 title
                            string str = formMainTitle + "  -  " + programFileName;
                            this.Text = str;

                            toolStripWorkSpaceTextBox.Text = workSpacePath;

                            Program.undoRedoButtonSetEnableOrDisable(toolStripMenuItemUndo, toolStripMenuItemRedo);

                        }
                    }

                }

            }



        }

        private void FormMain_MdiChildActivate(object sender, EventArgs e)
        {
            //ActiveMdiChild   ActivateMdiChild
            //  MessageBox.Show("Mdi Activate: " + this.ActiveMdiChild);
        }


        //https://msdn.microsoft.com/zh-tw/library/system.windows.forms.control.previewkeydown(v=vs.110).aspx
        public void button_From_delete_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("button_KeyDown " + e.KeyValue + e.KeyCode);

            //20171103 現在要連續刪除，需要額外判定 Form 有沒有被 focus
            //MessageBox.Show("formMain.ActiveMdiChild: " + formMain.ActiveMdiChild.Text);

            //  MessageBox.Show("formMain ActiveControl = " + formMain.ActiveControl);
            //MessageBox.Show("formMain ActiveMdiChild = " + this.ActiveMdiChild);
            //MessageBox.Show("ActiveForm = " + Form.ActiveForm);
            //ActiveForm
            //MessageBox.Show("formMain ContainsFocus = " + formMain.ContainsFocus);
            //MessageBox.Show("" + this.ActiveMdiChild.GetType());
            if (this.ActiveMdiChild != null)
            {
                //快捷鍵會被覆蓋掉...這邊要額外添加 
                if (e.KeyData == (Keys.Control | Keys.N))
                {
                    newToolStripMenuItem_Click(sender, e);
                }
                else if (e.KeyData == (Keys.Control | Keys.O))
                {
                    openToolStripMenuItem_Click(sender, e);
                }
                else if (e.KeyData == (Keys.Control | Keys.S))
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
                else if (e.KeyData == (Keys.Control | Keys.Z))
                {
                    toolStripMenuItemUndo_Click(sender, e);
                }
                else if (e.KeyData == (Keys.Control | Keys.Shift | Keys.Z))
                {
                    toolStripMenuItemRedo_Click(sender, e);
                }
                else if (this.ActiveMdiChild.GetType() == typeof(FormVariables))
                {
                    if (formVariables != null && formVariable != null)
                    {
                        if (e.KeyValue == 8 || e.KeyValue == 46)
                        {

                            formVariable.buttonDelete_Click(sender, null);
                            //20171103 項目[Program Functions 跟 Program Variables 無法連續按 Backspace 或 Del 連續進行刪除]
                            //所以要[刪除掉後，強制選擇下一個 Function 或 Variable]
                            Control con;
                            con = formVariables.ActiveControl;
                            if (con != null)
                            {
                                //MessageBox.Show("V_focus: " + con.Name + ", " + con.Text + ", MDI = " + formMain.ActiveMdiChild.GetType());
                                if (con.GetType() == typeof(Button))
                                {

                                    if (formVariables.buttons.Contains(con))
                                    {
                                        formVariables.variableButton_Click(con, EventArgs.Empty);

                                    }

                                }
                                else
                                {
                                    //   MessageBox.Show("it not button");
                                }
                            }
                            else
                            {
                                //  MessageBox.Show("focus null variableButton");
                            }

                        }
                    }
                }
                else if (this.ActiveMdiChild.GetType() == typeof(FormFunctions))
                {
                    if (formFunctions != null && formFunction != null)
                    {


                        if (e.KeyValue == 8 || e.KeyValue == 46)
                        {

                            formFunction.removeButton_Click(sender, null);
                            //20171103 項目[Program Functions 跟 Program Variables 無法連續按 Backspace 或 Del 連續進行刪除]
                            //所以要[刪除掉後，強制選擇下一個 Function 或 Variable]
                            Control con;
                            con = formFunctions.ActiveControl;
                            if (con != null)
                            {


                                if (con.GetType() == typeof(Button))
                                {
                                    //MessageBox.Show("it is button");
                                    if (formFunctions.buttons.Contains(con))
                                    {
                                        //MessageBox.Show("it in buttons");
                                        formFunctions.functionButton_Click(con, EventArgs.Empty);
                                    }
                                }
                                else
                                {
                                    // MessageBox.Show("it not button");
                                }
                            }
                            else
                            {
                                // MessageBox.Show("focus null functionButton");
                            }


                        }


                    }
                }

            }

        }

        private void zhTWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb_key = new StringBuilder("language");
            StringBuilder sb_value = new StringBuilder(R7.STRING_SIZE);
            sb_value.Append("zh_TW");
            R8.WriteConfig(sb_key, sb_value);
            sb_value.Clear();
            R8.SaveConfigFile(R8.ConfigFilePath);
            MessageBox.Show("It will effect when next time you open R8");
        }

        private void enToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb_key = new StringBuilder("language");
            StringBuilder sb_value = new StringBuilder(R7.STRING_SIZE);
            sb_value.Append("EN");
            R8.WriteConfig(sb_key, sb_value);
            sb_value.Clear();
            R8.SaveConfigFile(R8.ConfigFilePath);
            MessageBox.Show("It will effect when next time you open R8");
        }

        private void jpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb_key = new StringBuilder("language");
            StringBuilder sb_value = new StringBuilder(R7.STRING_SIZE);
            sb_value.Append("JP");
            R8.WriteConfig(sb_key, sb_value);
            sb_value.Clear();
            R8.SaveConfigFile(R8.ConfigFilePath);
            MessageBox.Show("It will effect when next time you open R8");
        }

        private void zhCNToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb_key = new StringBuilder("language");
            StringBuilder sb_value = new StringBuilder(R7.STRING_SIZE);
            sb_value.Append("zh_CN");
            R8.WriteConfig(sb_key, sb_value);
            sb_value.Clear();
            R8.SaveConfigFile(R8.ConfigFilePath);
            MessageBox.Show("It will effect when next time you open R8");
        }

        private void exportToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {/*
            string str_Export1 = "Export to Windows batch file";
            string str_Export2 = "Export to Visual C++ file";
            str_Export1 = R8.TranslationString(str_Export1);
            str_Export2 = R8.TranslationString(str_Export2);
            exportBatchFileToolStripMenuItem.Text = str_Export1;
            exportCFileToolStripMenuItem.Text = str_Export2;
            */
        }

        /*

        private void FormFunctions_DragEnter(object sender, DragEventArgs e)
        {
            //R8.Log("FormFunctions_DragEnter");
            dragX = e.X;
            dragY = e.Y;
            dragData = e.Data.GetData(DataFormats.Text).ToString();
            e.Effect = DragDropEffects.All;
            return;
        }

        private void FormFunctions_DragOver(object sender, DragEventArgs e)
        {
            return;
    }*/

    }
}
