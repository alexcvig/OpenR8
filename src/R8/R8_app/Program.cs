using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace R8
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }


        public static List<R8> recordList = new List<R8>();
        private static int undoRedoNowAt = 0;
        private static int recordListMaxSize = 11; //紀錄上限10個，然後第0個放目前狀態，所以實質上是11個


        public static bool isFileChange = false;

        //20170930 依昨天討論， Undo 與 Redo 按鈕在不能按的時候讓它灰掉。
        //所以 [除非有按 Undo ，不然 Redo 一直保持灰色](undoRedoNowAt 為0時灰色)
        //另外 [ recordList 大於1， Undo 才能按]
        //[ undoRedoNowAt 等於 recordList.size - 1， Undo 不能按]

        //20170930 依照 recordList 與 undoRedoNowAt 切換 按鈕狀況
        public static void undoRedoButtonSetEnableOrDisable(ToolStripMenuItem undoButton, ToolStripMenuItem redoButton)
        {
           
            if (undoRedoNowAt < 1)
            {
                redoButton.Enabled = false;
            }
            else {
                redoButton.Enabled = true;
            }

            if (undoRedoNowAt >= recordList.Count - 1)
            {
                undoButton.Enabled = false;
            }
            else {
                undoButton.Enabled = true;
            }

            return;
        }

        public static void recordUndo() {
            undoRedoNowAt++;
            if (undoRedoNowAt > recordList.Count - 1)
            {
                undoRedoNowAt = recordList.Count - 1;
            }
        }

        public static void recordRedo()
        {
            undoRedoNowAt--;
            if (undoRedoNowAt < 0) {
                undoRedoNowAt = 0;
            }
        }

        //20170929 leo: undo 與 redo 用，整個 R8 記錄下來
        public static void recordProgram(R8 r8, ToolStripMenuItem undoButton, ToolStripMenuItem redoButton)
        {
            /*
            for (int i = recordList.Count - 1; i > undoRedoNowAt + 1; i--) {
                recordList.RemoveAt(i);
            }
            */
            
            if (recordList.Count > 0)
            {
                for (int i = undoRedoNowAt - 1; i >= 0; i--)
                {
                    //MessageBox.Show("RemoveAt " + i);
                    recordList.RemoveAt(i);
                }
                undoRedoNowAt = 0;
            }
            
            
            recordList.Insert(undoRedoNowAt, r8.clone());

            if (recordList.Count > recordListMaxSize) {
                recordList.RemoveAt(recordList.Count - 1);
            }
            Program.undoRedoButtonSetEnableOrDisable(undoButton, redoButton);

            isFileChange = true;
            //MessageBox.Show("isFileChange = true");
            /*
            if (false)
            {
                String str = "recordProgram\r\n";
                for (int j = 0; j < recordList.Count; j++)
                {
                    str += "at " + j + "\r\n";
                    str += "undoRedoNowAt = " + undoRedoNowAt + "\r\n";
                    str += "recordList Size = " + recordList.Count + "\r\n";
                    //str += "getFunctionSnLast = " + r8.getFunctionSnLast() + "\r\n";
                    //str += "getVariableSnLast = " + r8.getVariableSnLast() + "\r\n";
                    for (int i = 0; i < recordList[j].getFunctionSnLast(); i++)
                    {
                        if (recordList[j].functions[i] != null)
                        {
                            str += "function " + recordList[j].functions[i] + "\r\n";
                        }
                    }
                    str += "===\r\n";
                    for (int i = 0; i < recordList[j].getVariableSnLast(); i++)
                    {
                        if (recordList[j].variables[i] != null)
                        {
                            str += "variables " + recordList[j].variables[i] + "\r\n";
                        }
                    }
                    str += "\r\n";
                }
                MessageBox.Show(str);
            }
            */

            /*
            if (false)
            {
                String str = "recordProgram\r\n";
                
                str += "undoRedoNowAt = " + undoRedoNowAt + "\r\n";
                str += "recordList Size = " + recordList.Count + "\r\n";
                //str += "getFunctionSnLast = " + r8.getFunctionSnLast() + "\r\n";
                //str += "getVariableSnLast = " + r8.getVariableSnLast() + "\r\n";
                for (int i = 0; i < r8.getFunctionSnLast(); i++)
                {
                    if (r8.functions[i] != null)
                    {
                        str += "function " + r8.functions[i] + "\r\n";
                    }
                }
                str += "===\r\n";
                for (int i = 0; i < r8.getVariableSnLast(); i++)
                {
                    if (r8.variables[i] != null)
                    {
                        str += "variables " + r8.variables[i] + "\r\n";
                    }
                }
                str += "\r\n";

                MessageBox.Show(str);
            }
            */

            return;
        }

        //然後這邊是把整個 R8 讀取出來
        public static R8 loadProgramFromRecord()
        {
            /*
            if (true)
            {
                MessageBox.Show("undoRedoNowAt at " + undoRedoNowAt);
                String str = "recordProgram\r\n";
                for (int j = 0; j < recordList.Count; j++)
                {
                    str += "at " + j + "\r\n";
                    str += "undoRedoNowAt = " + undoRedoNowAt + "\r\n";
                    str += "recordList Size = " + recordList.Count + "\r\n";
                    //str += "getFunctionSnLast = " + r8.getFunctionSnLast() + "\r\n";
                    //str += "getVariableSnLast = " + r8.getVariableSnLast() + "\r\n";
                    for (int i = 0; i < recordList[j].getFunctionSnLast(); i++)
                    {
                        if (recordList[j].functions[i] != null)
                        {
                            str += "function " + recordList[j].functions[i] + "\r\n";
                        }
                    }
                    str += "\r\n";
                }
                MessageBox.Show(str);
            }
            */
            R8 r8 = null;
           
            if (undoRedoNowAt >= 0 && undoRedoNowAt < recordList.Count)
            {
                //r8 = recordList.ElementAt(undoRedoNowAt).clone();
                r8 = recordList[undoRedoNowAt].clone();
            }
            else {
                if (FormMain.r8 != null)
                {
                    return FormMain.r8.clone(); //如果沒有上一步的紀錄，回傳原本的 R8
                }
                else {
                    return null;
                }
            }

            return r8;
        }

        public static void clearRecord() {
            recordList.Clear();
            undoRedoNowAt = 0;
            return;
        }
    }
}
