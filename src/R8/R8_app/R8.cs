using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
//using System.Collections.Generic;

namespace R8
{ 
    public class R8
    {

        public static string ConfigFilePath = System.Windows.Forms.Application.StartupPath + "\\R8.config";
        //size 定義搬到 R7 去
        //public const int ARRAY_SIZE = 1024;
        //public const int STRING_SIZE = 4096;
        public static bool HAS_POS = false;//是否有 posX 、posY

        public static string MacAddress = "";
        public string version = "0.0.0";
        private int functionSnCount = 0;
        private int variableSnCount = 0;
        //20170119 leo: 討論後改為用 snMax 取代原本的 snCount
        private int functionSnMax = 0;
        private int variableSnMax = 0;
        //<functionSnMax>3</functionSnMax>
        //<variableSnMax>7</variableSnMax>
        //<functionSnCount>3</functionSnCount>
        //<variableSnCount>5</variableSnCount>

        //20170929 leo: 要做 undo redo....這兩個 private 後開 get 與 set ， 然後 get set 時通通觸發 record
        //修改，以上方法會發生[明明只做了一個動作，卻記錄了好幾步]的現象(因為有些動作會有多次的 set )
        //，還是得因應各種動作分別寫操作才能避開此現象。
        public Function[] functions = new Function[R7.FUNCTIONS_SIZE];
        public Variable[] variables = new Variable[R7.VARIABLES_SIZE];
        
        //public static string[] typeArray = null;

        public static RichTextBox logBox = null;

        public static FormMain formMain = null;

        public static bool IsLogin = false;

        public R8 clone() {
            R8 newR8 = new R8();
            newR8.version = this.version;
            newR8.functionSnCount = this.functionSnCount;
            newR8.variableSnCount = this.variableSnCount;
            newR8.functionSnMax = this.functionSnMax;
            newR8.variableSnMax = this.variableSnMax;

            // public Function[] functions = new Function[R7.FUNCTIONS_SIZE];
            // public Variable[] variables = new Variable[R7.VARIABLES_SIZE];

            for (int i = 0; i < this.getFunctionSnLast(); i++)
            {
                if (this.functions[i] != null)
                {
                    newR8.functions[i] = this.functions[i].clone();
                }
            }


            for (int i = 0; i < this.getVariableSnLast(); i++)
            {
                if (this.variables[i] != null)
                {
                    newR8.variables[i] = this.variables[i].clone();
                }
            }

            return newR8;
        }


        public static int getVariableTypeByString(string type) {
            int res = 0;
            int variableNum = R8.GetVariableNum();
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);            
            for (int i = 0; i < variableNum; i++) {
                R8.GetVariableType(str, R7.STRING_SIZE, i);
                if (str.ToString().Equals(type)) {
                    res = i;
                    break;
                }
            }
            str.Clear();
            return res;
        }


        public static void setLogBox(RichTextBox richTextBox)
        {
            logBox = richTextBox;
        }
        //static string NewLine = "\r\n";

        public static int stringToInt(string str)
        {
            int res = 0;
            if (!Int32.TryParse(str, out res))
            {
                res = 0;
            }
            return res;
        }

        public static double stringToDouble(string str)
        {
            double res = 0;
            if (!Double.TryParse(str, out res))
            {
                res = 0;
            }
            return res;
        }

        public static void clearLogBox()
        {
            if (logBox != null)
            {
                if (!logBox.IsDisposed)
                {
                    logBox.Text = "";
                }
            }
        }

        public static void logToLogBox(string text) {
            if (logBox != null) {
                if (!logBox.IsDisposed && text.Length > 0) {
                    //20170330 關於 [第一筆 log 發生後，在第一筆 log 上方，請插入一行 2017-03-30 3:23:50 的時間資訊。]
                    //如果加在 R7 會導致 A2 A3 的 result 格式不符合，所以直接加在 R8
                    //20170407 json 要換行，換在 { 符號
                    text = text.Replace(" {", "\n{");
                    logBox.Text = text;
                    //logBox.Text = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + text + Environment.NewLine;
                }
            }
        }

        public static void addLogToLogBox(string text)
        {
            // TODO: Add semaphore.
            if (logBox != null)
            {
                if (!logBox.IsDisposed && text.Length > 0)
                {
                    //20170407 json 要換行，換在 { 符號
                    text = text.Replace(" {", "\n{");
                    logBox.Text = logBox.Text + text;
                    //logBox.Text = logBox.Text + Environment.NewLine + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + text;
                }
            }
        }

        public R8() {
            //固定產生一個 Start; //20170117 取消
            //Function start = new Function();
            //functions[0] = start;
        }

        public R8(XElement xml)
        {

            XElement element = xml.Element("functionSnCount");
            //20170626 leo:加入舊版支援
            bool isOldVerson = false;
            if (element == null)
            {
                //此時判定為舊版
                isOldVerson = true;
                element = xml.Element("intentSnCount");
                if (!Int32.TryParse(element.Value, out functionSnCount))
                {
                    functionSnCount = 0;
                }

                element = xml.Element("dataSnCount");
                if (!Int32.TryParse(element.Value, out variableSnCount))
                {
                    variableSnCount = 0;
                }

                element = xml.Element("intentSnMax");
                if (!Int32.TryParse(element.Value, out functionSnMax))
                {
                    functionSnMax = 0;
                }

                element = xml.Element("dataSnMax");
                if (!Int32.TryParse(element.Value, out variableSnMax))
                {
                    variableSnMax = 0;
                }

            }
            else
            {
                if (!Int32.TryParse(element.Value, out functionSnCount))
                {
                    functionSnCount = 0;
                }

                element = xml.Element("variableSnCount");
                if (!Int32.TryParse(element.Value, out variableSnCount))
                {
                    variableSnCount = 0;
                }

                element = xml.Element("functionSnMax");
                if (!Int32.TryParse(element.Value, out functionSnMax))
                {
                    functionSnMax = 0;
                }

                element = xml.Element("variableSnMax");
                if (!Int32.TryParse(element.Value, out variableSnMax))
                {
                    variableSnMax = 0;
                }
            }

            element = xml.Element("version");
            version = element.Value.ToString();


            int res;
            IEnumerable<XElement> variableElements;// = xml.Element("variables").Elements("variable");
            if (isOldVerson)
            {
                variableElements = xml.Element("datas").Elements("data");
            }
            else {
                variableElements = xml.Element("variables").Elements("variable");
            }
            Variable variable;
            string tempTypeString;
            int variableNum = R8.GetVariableNum();
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            int tempInt = -1;
            for (int i = 0; i < variableElements.Count(); i++)
            {
                variable = new Variable();
                element = variableElements.ElementAt(i);
                if (!Int32.TryParse(element.Element("sn").Value, out res))
                {
                    res = -1;
                }
                variable.sn = res;
                /*
                if (!Int32.TryParse(element.Element("type").Value, out res))
                {
                    res = -1;
                }
                */

                tempTypeString = element.Element("type").Value;
                tempInt = -1;
                for (int j = 0; j < variableNum; j++)
                {
                    R8.GetVariableType(str, R7.STRING_SIZE, j);
                    //統一轉小寫再比
                    if (str.ToString().ToLower().Equals(tempTypeString.ToLower()))
                    {
                        tempInt = j;
                        break;
                    }
                }
                //System.Console.WriteLine("type: " + tempTypeString + "  value:" + tempInt);
                if (tempInt != -1)
                {
                    variable.type = tempInt;
                }
                //Variable.VariableType.String;
                variable.value = element.Element("value").Value;
                variable.name = element.Element("name").Value;
                variable.remark = element.Element("remark").Value;
                variables[variable.sn] = variable;
            }
            str.Clear();

            IEnumerable<XElement> functionElements;// = xml.Element("functions").Elements("function");

            if (isOldVerson)
            {
                functionElements = xml.Element("intents").Elements("intent");
            }
            else {
                functionElements = xml.Element("functions").Elements("function");
            }
            IEnumerable<XElement> variableSnElements = null;
            XElement subElement;
            Function function;
            
            for (int i = 0; i < functionElements.Count(); i++)
            {
                function = new Function();
                element = functionElements.ElementAt(i);
                if (!Int32.TryParse(element.Element("sn").Value, out res))
                {
                    res = -1;
                }
                function.sn = res;
                function.name = element.Element("name").Value;
                function.remark = element.Element("remark").Value;
                function.parameters = Function.getFunctionParameters(function.name);

                if (element.Element("enable") != null)
                {
                    if (element.Element("enable").Value.Equals("0"))
                    {
                        function.enable = false;
                    }
                    else {
                        function.enable = true;
                    }
                }
                else {
                    function.enable = true;
                }

                if (R8.HAS_POS)
                {
                    if (!Int32.TryParse(element.Element("posX").Value, out res))
                    {
                        res = -1;
                    }
                    function.rectangle.X = res;
                    if (!Int32.TryParse(element.Element("posY").Value, out res))
                    {
                        res = -1;
                    }
                    function.rectangle.Y = res;
                }
                else {
                    function.rectangle.X = 0;
                    function.rectangle.Y = function.sn * 60;
                    //20170118 leo 新的 posY
                    if (!Int32.TryParse(element.Element("posY").Value, out res))
                    {
                        res = 0;
                    }
                    function.posY = res;
                }
                functions[function.sn] = function;

                //variableSn 不分 input/ output 了，由 Function.Parameter 定義 (以後要從 json 撈)
                if (isOldVerson)
                {
                    variableSnElements = element.Elements("dataSn");
                }
                else {
                    variableSnElements = element.Elements("variableSn");
                }
                int j = 0;
                //20170407 leo add: 就算 supportList 沒找到目標 function name，也把 parameters variable 補齊，避免 user 開不同版本運行後， program 壞掉。
                if (variableSnElements.Count() != function.parameters.Count && function.parameters.Count == 0)
                {
                    //MessageBox.Show("Warning: function [" + function.name + "] may not in support list.");

                    for (j = 0; j < variableSnElements.Count(); j++)
                    {
                        subElement = variableSnElements.ElementAt(j);
                        if (!Int32.TryParse(subElement.Value, out res))
                        {
                            res = -1;
                        }

                        int type = 0;
                        if (res != -1)
                        {
                            //parameter 名稱遺失了，但 variable 的 type 還是會有
                            //type = variables[res].type;
                            if (variables[res] != null)
                            {
                                type = variables[res].type;
                            }
                        }

                        Function.Parameter parameter = new Function.Parameter("unload", type);
                        parameter.option = "IN";
                        parameter.variableSn = res;
                        function.parameters.Add(parameter);
                        // MessageBox.Show("Adding: " + parameter.name + " " + parameter.type);
                    }
                }
                //20170817 leo: 發現當版本不同時，如果同一個 function 名稱，新版的參數比舊版多，
                //然後用舊版 R8 開啟 .r6 檔案時，這邊會 error 。
                //修正：
                else if (variableSnElements.Count() != function.parameters.Count)
                {

                    for (j = 0; j < variableSnElements.Count() && j < function.parameters.Count; j++)
                    {
                        subElement = variableSnElements.ElementAt(j);
                        if (!Int32.TryParse(subElement.Value, out res))
                        {
                            res = -1;
                        }
                        function.parameters.ElementAt(j).variableSn = res;


                    }

                    for (j = function.parameters.Count; j < variableSnElements.Count(); j++)
                    {
                        subElement = variableSnElements.ElementAt(j);
                        if (!Int32.TryParse(subElement.Value, out res))
                        {
                            res = -1;
                        }

                        int type = 0;
                        if (res != -1)
                        {
                            //parameter 名稱遺失了，但 variable 的 type 還是會有
                            //type = variables[res].type;
                            if (variables[res] != null)
                            {
                                type = variables[res].type;
                            }
                        }

                        Function.Parameter parameter = new Function.Parameter("unload", type);
                        parameter.option = "IN";
                        parameter.variableSn = res;
                        function.parameters.Add(parameter);
                        // MessageBox.Show("Adding: " + parameter.name + " " + parameter.type);
                    }
                }
                else
                {

                    //for (int j = 0; j < variableSnElements.Count() && j < function.parameters.Count; j++)
                    for (j = 0; j < variableSnElements.Count(); j++)
                    {
                        subElement = variableSnElements.ElementAt(j);
                        if (!Int32.TryParse(subElement.Value, out res))
                        {
                            res = -1;
                        }
                        function.parameters.ElementAt(j).variableSn = res;


                    }
                }

                //Console.WriteLine("sn = " + function.sn);
                //Console.WriteLine("name = " + function.name);
            }
            
            // Console.WriteLine("functionSnCount = " + functionSnCount);
        }

        public static void writeProgramXml(string path)
        {

            XElement xml = new XElement("program");
            //根據 http://stackoverflow.com/questions/27548227/add-new-xelements-with-new-line-in-xdocument-with-preservewhitespace-loadoptions
            //換行需要自己加...
            xml.Add(Environment.NewLine);
            xml.Add(new XElement("version", FormMain.r8.getVersion()));
            xml.Add(Environment.NewLine);
            //xml.Add(new XElement("workSpacePath", FormMain.workSpacePath));//20170120 leo: 寫檔時，應該也把 workSpacePath 寫進去
            //xml.Add(Environment.NewLine);//20170306 leo: 改為不寫入 workSpacePath
            xml.Add(new XElement("functionSnMax", FormMain.r8.getFunctionSnMax()));
            xml.Add(Environment.NewLine);
            xml.Add(new XElement("variableSnMax", FormMain.r8.getVariableSnMax()));
            xml.Add(Environment.NewLine);
            xml.Add(new XElement("functionSnCount", FormMain.r8.getFunctionsCount()));
            xml.Add(Environment.NewLine);
            xml.Add(new XElement("variableSnCount", FormMain.r8.getVariablesCount()));
            xml.Add(Environment.NewLine);

            XElement functionsXml = new XElement("functions");
            xml.Add(functionsXml);
            xml.Add(Environment.NewLine);
            functionsXml.Add(Environment.NewLine);

            int i, j;
            Function function;
            XElement functionXml = null;

            for (i = 0; i < FormMain.r8.getFunctionSnLast(); i++)
            {
                function = FormMain.r8.functions[i];
                if (function == null)
                {
                    continue;
                }
                functionXml = new XElement("function");
                functionXml.Add(Environment.NewLine);
                functionXml.Add(new XElement("sn", function.sn));
                functionXml.Add(Environment.NewLine);
                functionXml.Add(new XElement("name", function.name));
                functionXml.Add(Environment.NewLine);
                functionXml.Add(new XElement("posY", function.posY));
                functionXml.Add(Environment.NewLine);
                if (function.enable)
                {
                    functionXml.Add(new XElement("enable", "1"));
                }
                else {
                    functionXml.Add(new XElement("enable", "0"));
                }
                functionXml.Add(Environment.NewLine);
                functionXml.Add(new XElement("remark", function.remark));
                functionXml.Add(Environment.NewLine);
                if (R8.HAS_POS)
                {
                    functionXml.Add(new XElement("posX", function.getPosX()));
                    functionXml.Add(Environment.NewLine);
                    functionXml.Add(new XElement("posY", function.getPosY()));
                    functionXml.Add(Environment.NewLine);
                }
                for (j = 0; j < function.parameters.Count; j++)
                {
                    functionXml.Add(new XElement("variableSn", function.parameters.ElementAt(j).variableSn));
                    functionXml.Add(Environment.NewLine);
                }
                functionsXml.Add(functionXml);
                functionsXml.Add(Environment.NewLine);
            }


            XElement variablesXml = new XElement("variables");
            xml.Add(variablesXml);
            xml.Add(Environment.NewLine);
            variablesXml.Add(Environment.NewLine);
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            Variable variable;
            XElement variableXml = null;
            for (i = 0; i < FormMain.r8.getVariableSnLast(); i++)
            {
                variable = FormMain.r8.variables[i];
                if (variable == null)
                {
                    continue;
                }
                variableXml = new XElement("variable");
                variableXml.Add(Environment.NewLine);
                variableXml.Add(new XElement("sn", variable.sn));
                variableXml.Add(Environment.NewLine);
                variableXml.Add(new XElement("name", variable.name));
                variableXml.Add(Environment.NewLine);
                //20170119 leo 今天討論後， type 要改為 string 小寫
                //variableXml.Add(new XElement("type", variable.type));
                //variableXml.Add(new XElement("type", Enum.GetName(typeof(Variable.VariableType), variable.type).ToLower()));
                //variableXml.Add(new XElement("type", R8.typeArray[variable.type].ToLower()));
                R8.GetVariableType(str, R7.STRING_SIZE, variable.type);
                variableXml.Add(new XElement("type", str));
                variableXml.Add(Environment.NewLine);
                variableXml.Add(new XElement("value", variable.value));
                variableXml.Add(Environment.NewLine);
                variableXml.Add(new XElement("remark", variable.remark));
                variableXml.Add(Environment.NewLine);
                variablesXml.Add(variableXml);
                variablesXml.Add(Environment.NewLine);
            }
            str.Clear();
            //xml.Save(path);
            //20180212 現在這邊要求要加標頭：
            /*
            #R7
            # -*- coding: utf-8 -*-
            # R7 version 1.8.6
            */
            //改用 stream
            StreamWriter streamWriter = new StreamWriter(path);
            streamWriter.WriteLine("#!R7");
            streamWriter.WriteLine("# -*- coding: utf-8 -*-");
            StringBuilder sb = new StringBuilder(1024);
            R7.GetVersion(sb, 1024);
            streamWriter.WriteLine("# R7 version " + sb.ToString());
            sb.Clear();
            xml.Save(streamWriter);
            streamWriter.Close();
            return;
        }

        public int addFunction(Function function)
        {
            functionSnMax++;
            function.sn = functionSnMax;
            functions[functionSnMax] = function;
            return 1;
        }

        public int addVariable(Variable variable)
        {
            variableSnMax++;
            variable.sn = variableSnMax;
            variables[variableSnMax] = variable;
            return 1;
        }


        
        public int getFunctionsCount()
        {
            int count = 0;
            for (int i = 0; i < getFunctionSnLast(); i++) {
                if (functions[i] != null) {
                    if (functions[i].sn != -1 && functions[i].sn != 0)
                    {
                        count++;
                    }
                }
            }
            functionSnCount = count;
            return functionSnCount;
        }

        public int getVariablesCount()
        {
            int count = 0;
            for (int i = 0; i < getVariableSnLast(); i++)
            {
                if (variables[i] != null)
                {
                    if (variables[i].sn != -1 && variables[i].sn != 0)
                    {
                        count++;
                    }
                }
            }
            variableSnCount = count;
            return variableSnCount;
        }
        

        //20170119 leo : 今天討論後結構改變，這個用來取代原本的 getFunctionsCount 。
        // functionSnCount 的定義也改變了，原定義的 functionSnCount ，會等於目前版本的 functionSnMax + 1
        public int getFunctionSnLast()
        {
            return functionSnMax + 1;
        }

        public int getVariableSnLast()
        {
            return variableSnMax + 1;
        }

        public int getFunctionSnMax()
        {
            return functionSnMax;
        }

        public int getVariableSnMax()
        {
            return variableSnMax;
        }

        public string getVersion()
        {
            return version;
        }

        //20171026 項目 [下拉式選單以排序方式顯示]....改在這邊比較快
        //20171120 被選取但不屬於搜尋項目需要[額外加入該項目]，這邊改傳 list 回去比較好處理
        public List<Variable> getVariableArrayByType(int type, bool needSort, ref int maxStringString, String searchStr) //取得特定 Type 的 variable
        {
            maxStringString = 0;
            List <Variable> variableList = new List<Variable>();
            for (int i = 0; i < variableSnMax + 1; i++) {
                if (variables[i] == null)
                {
                    continue;
                }
                if (variables[i].sn == -1 || variables[i].sn == 0) {
                    continue;
                }
                if (variables[i].type == (int)type) {
                    variableList.Add(variables[i]);
                }
            }
            if (searchStr != null) {
                if (searchStr.Length > 0) {
                    List<Variable> tempSearchList = new List<Variable>();

                    for (int i = 0; i < variableList.Count; i++)
                    {
                        if (variableList[i].ToString().ToLower().Contains(searchStr.ToLower())) {
                            tempSearchList.Add(variableList[i]);
                        }
                    }
                    if (tempSearchList.Count > 0) {

                        variableList = tempSearchList;
                    }
                }
            }

            for (int i = 0; i < variableList.Count; i++)
            {

                int tarLength = variableList[i].ToString().Length;
                if (maxStringString < tarLength)
                {
                    maxStringString = tarLength;
                }

            }

            if (needSort) {
                //variableList.Sort((x, y) => { return x.name.CompareTo(y.name); });
                variableList.Sort(delegate (Variable i1, Variable i2) { return i1.name.CompareTo(i2.name); });
            }
            //20170314 leo: add: 加上 [取消選取] 的功能，
            variableList.Add(new Variable());
            //return variableList.ToArray();
            return variableList;
        }


        public Variable[] getVariableArray() //取得非 null 且 sn 非 -1 的 所有 variable
        {
            List<Variable> variableList = new List<Variable>();
            for (int i = 0; i < variableSnMax + 1; i++)
            {
                if (variables[i] == null)
                {
                    continue;
                }
                if (variables[i].sn == -1 || variables[i].sn == 0)
                {
                    continue;
                }
                variableList.Add(variables[i]);
            }
            return variableList.ToArray();
        }


        //從 A3 移植過來的 ByteArrayToImage
        public static Image ByteArrayToImage(byte[] imgBuffer, int imgBufferLength, int width, int height, int channels)
        {
            try
            {
                if (imgBuffer == null)
                {
                    return null;
                }

                if (width < 4 || height <= 0)
                {
                    return null;
                }

                if (imgBufferLength < width * height * channels)
                {
                    return null;
                }

                if (channels == 3)
                {
                    byte[] buffer4;
                    int width4 = width - width % 4;
                    int bufferLength4;
                    if (width % 4 != 0)
                    {
                        buffer4 = new byte[width4 * height * 3];
                        bufferLength4 = buffer4.Length;
                        int i, j;
                        for (i = 0; i < height; i++)
                        {
                            for (j = 0; j < width4 * 3; j++)
                            {
                                buffer4[i * width4 * 3 + j] = imgBuffer[i * width * 3 + j];
                            }
                        }
                    }
                    else
                    {
                        buffer4 = imgBuffer;
                        bufferLength4 = imgBufferLength;
                    }


                    PixelFormat pixelFormat = PixelFormat.Format24bppRgb;


                    Bitmap camBitmap = new Bitmap(width4, height, pixelFormat);
                    Rectangle camRect = new Rectangle(0, 0, camBitmap.Width, camBitmap.Height);

                    // Lock all bitmap's pixels.
                    BitmapData bitmapData = camBitmap.LockBits(camRect, ImageLockMode.WriteOnly, pixelFormat);

                    // Copy the buffer into bitmapData.
                    Marshal.Copy(buffer4, 0, bitmapData.Scan0, bufferLength4);

                    // Unlock  all bitmap's pixels.
                    camBitmap.UnlockBits(bitmapData);

                    return camBitmap;
                }
                else
                {
                    if (imgBuffer == null)
                    {
                        return null;
                    }
                    if (imgBufferLength < width * height)
                    {
                        return null;
                    }

                    PixelFormat pixelFormat = PixelFormat.Format8bppIndexed;


                    Bitmap camBitmap = new Bitmap(width, height, pixelFormat);
                    Rectangle camRect = new Rectangle(0, 0, camBitmap.Width, camBitmap.Height);

                    // Set 8-bit gray scale palette.
                    //if (imageChannel == 1)  //Check: 有必要? 對3 Channel 會爆炸?
                    //{
                    ColorPalette grayPalette = camBitmap.Palette;
                    Color[] entries = grayPalette.Entries;
                    for (int i = 0; i < 256; i++)
                    {
                        entries[i] = Color.FromArgb(i, i, i);
                    }
                    camBitmap.Palette = grayPalette;
                    //}

                    // Lock all bitmap's pixels.
                    BitmapData bitmapData = camBitmap.LockBits(camRect, ImageLockMode.WriteOnly, pixelFormat);

                    // Copy the buffer into bitmapData.
                    //Marshal.Copy(imgBuffer, 0, bitmapData.Scan0, imgBufferLength);
                    //20170331 leo: fix 灰階 bitmap 要是4的倍數不然會歪斜
                    for (int i = 0; i < height; i++) {
                        Marshal.Copy(imgBuffer, i * width, bitmapData.Scan0 + i * bitmapData.Stride, width);
                    }


                    // Unlock  all bitmap's pixels.
                    camBitmap.UnlockBits(bitmapData);
                    //camBitmap.Save("C:\\Users\\a1s\\Documents\\R8temp.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    return camBitmap;
                }
            }
            catch
            {
                return null;
            }
        }
        static bool isAbsPath = false;//20170823 leo: Kelly 說她那邊用 3.7.0 rc4 會當機，
                                      //看起來問題像是出在相對路徑轉換出錯，
                                      //準備一個絕對路徑版本給她跑跑看
                                      // 20170808 leo: 絕對路徑轉相對路徑 : 根據 https://dotblogs.com.tw/larrynung/2011/03/17/21882
                                      // 再加上 https://stackoverflow.com/questions/1405048/how-do-i-decode-a-url-parameter-using-c 

        /*
    public static String GetRelativePath(String basePath, String targetPath)
    {
        if (isAbsPath) {
            return targetPath;
        }
        Uri baseUri = new Uri(basePath);
        Uri targetUri = new Uri(targetPath);
        Uri relativePath = baseUri.MakeRelativeUri(targetUri);
        string output = Uri.UnescapeDataString(relativePath.ToString().Replace(@"/", @"\"));
        //MessageBox.Show("basePath = " + basePath + "\r\n" + "targetPath = " + targetPath + "\r\n" + "output = " + output);
        return output;
    }
    */

        //20170825 leo: Kelly 測出來[路徑為根目錄]會當，修正
        public static String GetRelativePath(String basePath, String targetPath)
        {
            if (isAbsPath)
            {
                return targetPath;
            }
            string output = "";
            
            //原因是根目錄沒有斜線不被當成 URI ，所以這種時候，多補一個斜線給它
            if (!targetPath.Contains("\\") && !targetPath.Contains("/")) {
                targetPath += "/";
            }


            if (!basePath.Contains("\\") && !basePath.Contains("/"))
            {
                basePath += "/";
            }


            Uri baseUri = new Uri(basePath);
            Uri targetUri = new Uri(targetPath);
            Uri relativePath = baseUri.MakeRelativeUri(targetUri);
            output = Uri.UnescapeDataString(relativePath.ToString().Replace(@"/", @"\"));


            if (output.Equals(""))
            {
                //20170825 leo: 發現路徑如果空掉的時候可能出問題，需要另外處理(加個點斜線給它)
                output = "./";

            }

            //MessageBox.Show("basePath = " + basePath + "\r\n" + "targetPath = " + targetPath + "\r\n" + "output = " + output);
            return output;
            //MessageBox.Show("basePath = " + basePath + "\r\n" + "targetPath = " + targetPath + "\r\n" + "output = " + output);

        }

        private static List<FormImage> formImageList = new List<FormImage>();
        public static void AllFormImageBringToFront(FormImage form) {
            formImageList.Add(form);
            int i;
            FormImage tempForm;
            //先移除關掉的 form
            for (i = formImageList.Count - 1; i > -1; i--) {
                tempForm = formImageList[i];
                if (tempForm == null || tempForm.IsDisposed) {
                    formImageList.Remove(tempForm);
                }
            }
            //MessageBox.Show("formImageList Size: " + formImageList.Count);
            //然後再依序搬到最上層
            for (i = 0; i < formImageList.Count; i++) {
                tempForm = formImageList[i];
                tempForm.Focus();
                tempForm.BringToFront();
            }
            return;
        }

        private static Dictionary<string, string> languageDictionary;

        //20180212 leo 準備開始建立語言切換功能，
        //下午接到指示格式要改用 json ，這邊先保留
        public static void InitLanguage_backup(string languageFilePath) {
            languageDictionary = new Dictionary<string, string>();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(languageFilePath);
                int i;
                //string key, value;
                string[] parts;
                for (i = 0; i < lines.Length; i++)
                {
                    //MessageBox.Show(lines[i]);
                    parts = lines[i].Split(',');
                    if (parts.Length >= 2)
                    {
                        languageDictionary.Add(parts[0], parts[1]);
                    }
                }
            } catch (FileNotFoundException e) {
                //FileNotFoundException 該語系設定檔不存在
            }

            return;
        }

        public static int InitLanguage_backup2(string languageFilePath)
        {
            StringBuilder stringBuilder = new StringBuilder(languageFilePath, R7.STRING_SIZE);
            byte[] byteArray = new byte[stringBuilder.Length * sizeof(char)];
            System.Buffer.BlockCopy(stringBuilder.ToString().ToCharArray(), 0, byteArray, 0, byteArray.Length);
            StringBuilder err = new StringBuilder(4096);
            int res = LoadLanguageFile(byteArray, err);
            MessageBox.Show("error = " + err.ToString());
            return res;
        }


        //接到指示這個要變成 json 了.......
        public static string TranslationString_backup(string key) {
            string value;
            //如果沒抓到翻譯字串，就用原文
            if (languageDictionary.TryGetValue(key, out value))
            {
                //MessageBox.Show("Get : " + value);
            }
            else {
                //MessageBox.Show("No Get Value");
                value = (string)key.Clone();
            }

            return value;
        }

        public static string TranslationString_backup2(string key)
        {
            //銜接 json 版
            StringBuilder sb_key = new StringBuilder(key);
            StringBuilder sb_value = new StringBuilder(R7.STRING_SIZE);
            int res = R8.TranslationString(sb_key, sb_value);
            MessageBox.Show("res = " + res + "key = " + sb_key.ToString() + " value = " + sb_value.ToString());

            //然後轉 UTF8
            //https://msdn.microsoft.com/zh-tw/library/system.text.encoding.utf8(v=vs.110).aspx
            //https://www.chilkatsoft.com/p/p_320.asp
            //Encoding enc = new UTF8Encoding(true, true);
            //System.Text.Encoding utf_8 = System.Text.Encoding.UTF8; 
            byte[] byteArray = new byte[sb_value.Length * sizeof(char)];
            System.Buffer.BlockCopy(sb_value.ToString().ToCharArray(), 0, byteArray, 0, byteArray.Length);
            // string value = System.Text.Encoding.UTF8.GetString(byteArray);
            byte[] bytes = Encoding.UTF8.GetBytes(sb_value.ToString());
            byte[] bytes_toUTF8 = Encoding.Convert(Encoding.UTF8, Encoding.Default, bytes);

            string value = Encoding.Default.GetString(bytes_toUTF8);
            // String returnValue = enc.GetString(sb_value.ToString());
            //enc.GetBytes(sb_value.ToString());
            //......轉起來中文一直變亂碼有時候還會開檔失敗－ －....放棄使用 jansson 了，我直接手動轉
            return value;
        }


        public static int InitLanguage(string languageFilePath)
        {

            /* 格式為
{
  "File": "檔案", 
  "Undo": "還原", 
  "Redo": "反還原", 
  "Library": "函式庫", 
  "New": "新增", 
  "Open": "開啟", 
  "Save": "儲存"
}
             */
            languageDictionary = new Dictionary<string, string>();
            try
            {
                /*
                string[] lines = System.IO.File.ReadAllLines(languageFilePath);
                int i;
                //string key, value;
                string[] parts;
                for (i = 0; i < lines.Length; i++)
                {
                    //MessageBox.Show(lines[i]);
                    parts = lines[i].Split(',');
                    if (parts.Length >= 2)
                    {
                        languageDictionary.Add(parts[0], parts[1]);
                    }
                }
                */
                //手動拆 json....只拆 zh_TW.txt 格式
                string file = System.IO.File.ReadAllText(languageFilePath);
                //首先我只會有一層 json object ，裡面都是 key value
                //起點終點先抓，json object 最外層大括號
                int start = file.IndexOf('{');
                int end = file.LastIndexOf('}');
                string jsonObj;
                if (start >= 0 && end > start)
                {
                    jsonObj = file.Substring(start, end - start + 1);
                }
                else
                {
                    return -1;
                }

                //跳脫字元: \" 處理
                jsonObj = jsonObj.Replace("\\\"", "*colon*");

                bool isGetingKey = true;
                string key = "";
                string value = "";
                while (start >= 0 && end > start)
                {
                  
                   // MessageBox.Show("jsonObj : " + jsonObj);
                    //"File": "檔案", 

                    //get key
                    start = jsonObj.IndexOf("\"");
                    end = jsonObj.Substring(start + 1).IndexOf("\"") + 1 + start;
                    if (start >= 0 && end > start)
                    {
                        if (isGetingKey)
                        {
                            //string key = jsonObj.Substring(start, end - start + 1);  // 這樣是抓到 "File"
                            key = jsonObj.Substring(start + 1, end - start + 1 - 2); // 這樣是抓到 File

                           // MessageBox.Show("key : " + key);
                            jsonObj = jsonObj.Substring(end + 1);
                           // MessageBox.Show("next jsonObj : " + jsonObj);
                            isGetingKey = !isGetingKey;
                        }
                        else {
                            value = jsonObj.Substring(start + 1, end - start + 1 - 2);
                            jsonObj = jsonObj.Substring(end + 1);
                            //MessageBox.Show("Get: k = " + key + ", v = " + value);

                            //跳脫字元: \" 處理
                            key = key.Replace("*colon*", "\"");
                            value = value.Replace("*colon*", "\"");
                            languageDictionary.Add(key, value);
                            isGetingKey = !isGetingKey;
                        }
                    }
                    else
                    {
                       // MessageBox.Show("start " + start + " end " + end);
                    }
                }
                //string jsonObj = file.First('{');
            }
            catch (FileNotFoundException e)
            {
                //FileNotFoundException 該語系設定檔不存在
            }

            return 1;
        }

        
        public static string TranslationString(string key)
        {
            string value;
            //如果沒抓到翻譯字串，就用原文
            if (languageDictionary.TryGetValue(key, out value))
            {
                //MessageBox.Show("Get : " + value);
            }
            else
            {
                //MessageBox.Show("No Get Value");
                value = (string)key.Clone();
            }

            return value;
        }

        [DllImport("R8_lib.dll", EntryPoint = "R8_InitLib")]
        public static extern int InitLib();

        [DllImport("R8_lib.dll", EntryPoint = "R8_CloseLib")]
        public static extern int CloseLib();

        //20170206 leo: 這些都搬去 R7 了
        //20170301 leo: 依晨會討論，搬回來。
        [DllImport("R8_lib.dll", EntryPoint = "R8_GetSupportListByFileName")]
        public static extern int GetSupportListByFileName(byte[] fileName);

        //20170407 leo: 增加 library 用的
        [DllImport("R8_lib.dll", EntryPoint = "R8_AddLibrarySupportListByFileName")]
        public static extern int AddLibrarySupportListByFileName(byte[] fileName);

        //20170417 leo: 增加 macro 用的
        [DllImport("R8_lib.dll", EntryPoint = "R8_StartMacroSupportList")]
        public static extern int StartMacroSupportList();

        [DllImport("R8_lib.dll", EntryPoint = "R8_AddMacroSupportListByFileName")]
        public static extern int AddMacroSupportListByFileName(byte[] fileName);

        [DllImport("R8_lib.dll", EntryPoint = "R8_EndOfMacroSupportList")]
        public static extern int EndOfMacroSupportList();

        //20170123 leo: 加入取值的各種接口
        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableNum")]
        public static extern int GetVariableNum();

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableName")]
        public static extern int GetVariableName(StringBuilder str, int strSize, int typeSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableType")]
        public static extern int GetVariableType(StringBuilder str, int strSize, int typeSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetFunctionGroupNum")]
        public static extern int GetFunctionGroupNum();

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetFunctionNum")]
        public static extern int GetFunctionNum();

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetFunctionNumInGroup")]
        public static extern int GetFunctionNumInGroup(int groupSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetFunctionGroupName")]
        public static extern int GetFunctionGroupName(StringBuilder str, int strSize, int groupSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetFunctionName")]
        public static extern int GetFunctionName(StringBuilder str, int strSize, int groupSn, int functionSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetFunctionDoc")]
        public static extern int GetFunctionDoc(StringBuilder str, int strSize, int groupSn, int functionSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableNumInFunction")]
        public static extern int GetVariableNumInFunction(int groupSn, int functionSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableNameInFunction")]
        public static extern int GetVariableNameInFunction(StringBuilder str, int strSize, int groupSn, int functionSn, int variableSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableOptionInFunction")]
        public static extern int GetVariableOptionInFunction(StringBuilder str, int strSize, int groupSn, int functionSn, int variableSn);

        [DllImport("R8_lib.dll", EntryPoint = "R8_GetVariableTypeInFunction")]
        public static extern int GetVariableTypeInFunction(StringBuilder str, int strSize, int groupSn, int functionSn, int variableSn);

        //R8_API int R8_BuildSupportListByString(char* str)
        [DllImport("R8_lib.dll", EntryPoint = "R8_BuildSupportListByString")]
        public static extern int BuildSupportListByString(StringBuilder str);

        //20180212 add....config相關

        public static int LoadConfigFile(string configPath)
        {
            StringBuilder stringBuilder = new StringBuilder(configPath, R7.STRING_SIZE);
            byte[] byteArray = new byte[stringBuilder.Length * sizeof(char)];
            System.Buffer.BlockCopy(stringBuilder.ToString().ToCharArray(), 0, byteArray, 0, byteArray.Length);
            return LoadConfigFile(byteArray);
        }

        [DllImport("R8_lib.dll", EntryPoint = "R8_LoadConfigFile")]
        public static extern int LoadConfigFile(Byte[] configPath);

        // R8_API int R8_WriteConfig(char* key, char* value)
        [DllImport("R8_lib.dll", EntryPoint = "R8_WriteConfig")]
        public static extern int WriteConfig(StringBuilder key, StringBuilder value);

        [DllImport("R8_lib.dll", EntryPoint = "R8_ReadConfig")]
        public static extern int ReadConfig(StringBuilder key, StringBuilder value);


        public static int SaveConfigFile(string configPath)
        {
            StringBuilder stringBuilder = new StringBuilder(configPath, R7.STRING_SIZE);
            byte[] byteArray = new byte[stringBuilder.Length * sizeof(char)];
            System.Buffer.BlockCopy(stringBuilder.ToString().ToCharArray(), 0, byteArray, 0, byteArray.Length);
            return SaveConfigFile(byteArray);
        }

        [DllImport("R8_lib.dll", EntryPoint = "R8_SaveConfigFile")]
        public static extern int SaveConfigFile(Byte[] configPath);


        //20180212 leo: 下午的語言控制相關， json 版本...
        [DllImport("R8_lib.dll", EntryPoint = "R8_LoadLanguageFile")]
        public static extern int LoadLanguageFile(Byte[] filePath, StringBuilder err);
        
        [DllImport("R8_lib.dll", EntryPoint = "R8_TranslationString")]
        public static extern int TranslationString(StringBuilder key, StringBuilder value);

        public static int Log(string msg)
        {
            byte[] bytes = new byte[(msg.Length) * sizeof(char)];
            System.Buffer.BlockCopy(msg.ToCharArray(), 0, bytes, 0, bytes.Length);
            return Log(bytes);

        }

        [DllImport("r8_lib.dll", EntryPoint = "R8_LogW")]
        public static extern int Log(Byte[] msg);


        //20170817 leo: 發現有時候在 FormFunctions 拉 function 順序時，會話畫面捲動導致 function 被誤拖曳，
        //原因為 Form 在 onFocus 時會自動捲動到 [該 Form 中，前一個被選取物件] 的位置。
        //參照 https://stackoverflow.com/questions/419774/how-can-you-stop-a-winforms-panel-from-scrolling/912610#912610
        //用 override 把該功能關掉
        //20171025 項目 [搜尋 bar 不要跟著滑動]，覆蓋目標從 Form 層換成 Panel 層
        public class PanelNoScrollOnFocus : Panel
        {


            protected override System.Drawing.Point ScrollToControl(Control activeControl)
            {
                return DisplayRectangle.Location;
            }
        }
    }
}
