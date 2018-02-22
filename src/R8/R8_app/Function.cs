using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace R8
{
    public class Function
    {
        //public List<Function> functionList = new List<Function>();
        // public Function[] functions = new Function[R8.ARRAY_SIZE];
        public int sn = 0;//-1;
        public string name = "";
        public string remark = "";
        public bool enable = true;
        public int lineNumber = 0; //20171025 leo: 行號相關處理用的(也就是只有 R8 介面在用，不用傳給 R7)

        public static int DefaultRectangleWidth = 100;
        public static int DefaultRectangleHeight = 40;
        public Rectangle rectangle = new Rectangle(0, 0, DefaultRectangleWidth, DefaultRectangleHeight);
        public float posY = 0;//20170118 leo: 這個 posY與舊版的定義不同了，現在是定義[位置在第幾個]，以前是定義[座標y軸在哪]
        //改為 float....拖曳換位置時比較方便
        //20170116 leo: variableSn 分為 input 與 output
        // public List<int> variableSns = new List<int>();
        //public List<int> inputDataSns = new List<int>();
        //public List<int> outputDataSns = new List<int>();
        //20170118 leo: 目前的模式下，function 沒有 variableSn List 了，改為用 parameters List 管理
        public List<Parameter> parameters = new List<Parameter>();

        public Function()
        {

        }

        public Function(string name, int posX, int posY)
        {
            this.name = name;
            this.rectangle.X = posX;
            this.rectangle.Y = posY;
            this.rectangle.Width = DefaultRectangleWidth;
            this.rectangle.Height = DefaultRectangleHeight;
            parameters = getFunctionParameters(name);
        }

        public Function clone()
        {
            Function newFunction = new Function();
            newFunction.sn = this.sn;
            newFunction.name = this.name;
            newFunction.remark = this.remark;
            newFunction.enable = this.enable;
            newFunction.rectangle = new Rectangle(this.rectangle.X, this.rectangle.Y, this.rectangle.Width, this.rectangle.Height);
            newFunction.posY = this.posY;
            //newFunction.parameters.AddRange(this.parameters);
            for (int i = 0; i < this.parameters.Count; i++) {

                Parameter p = this.parameters[i].clone();
                newFunction.parameters.Add(p);
            }
            return newFunction;
        }

        public int getPosX()
        {
            return rectangle.X;
        }

        public int getPosY()
        {
            return rectangle.Y;
        }

        //20170117 leo: 今天討論後， Function 預設裡面不塞 Data ，但[parameters 的格子] 留著
        //暫時開一個接 Parameters 的結構。以後這個應該會從 json 接。
        //20170123 leo : 改成從 json 接
        public static List<Parameter> getFunctionParameters(string name) {
            List<Parameter> args = new List<Parameter>();
            /*
            switch (name)
            {
                case "OpenImage":
                    args.Add(new Parameter("Image File Name", R8.getDataTypeByString("string"), true));
                    args.Add(new Parameter("Image ", R8.getDataTypeByString("image"), false));
                    break;
                case "SaveImage":
                    //arg1: (input, mat) image
                    //arg2: (input, string) imagePath
                    args.Add(new Parameter("Image", R8.getDataTypeByString("image"), true));
                    args.Add(new Parameter("Image File Name", R8.getDataTypeByString("string"), true));
                    break;
                case "Binarize":
                    //cv::threshold(image, thresholdedImage, gv, 255, type);

                    args.Add(new Parameter("Input Image", R8.getDataTypeByString("image"), true));
                    args.Add(new Parameter("Threshold", R8.getDataTypeByString("int"), true));
                    //args.Add(new Parameter("thresholdType", Data.VariableType.Int, true));//對應 cv::threshold 的 type 參數
                    args.Add(new Parameter("Output Image", R8.getDataTypeByString("image"), false));
                    break;
            }*/
            //20170123 leo : 改從 json 接
            int functionGroupNum = 0;
            int functionNum = 0;
            int variableNum = 0;
            StringBuilder str = new StringBuilder(R7.STRING_SIZE);
            functionGroupNum = R8.GetFunctionGroupNum();
            string tempNameString;
            for (int i = 0; i < functionGroupNum; i++)
            {
                functionNum = R8.GetFunctionNumInGroup(i);
                for (int j = 0; j < functionNum; j++)
                {
                    R8.GetFunctionName(str, R7.STRING_SIZE, i, j);
                    //addButton(str.ToString());
                    if (str.ToString().Equals(name)) {
                        variableNum = R8.GetVariableNumInFunction(i, j);
                        for (int k = 0; k < variableNum; k++) {
                            R8.GetVariableNameInFunction(str, R7.STRING_SIZE, i, j, k);
                            tempNameString = str.ToString();
                            R8.GetVariableTypeInFunction(str, R7.STRING_SIZE, i, j, k);
                            Parameter parameter = new Parameter(tempNameString, R8.getVariableTypeByString(str.ToString()));
                            //20170214 加入 Direction 
                            //20171024 Direction 變成 Option
                            //option
                            R8.GetVariableOptionInFunction(str, R7.STRING_SIZE, i, j, k);
                            parameter.option = str.ToString();
                            args.Add(parameter);

                            //GetVariableDirectionInFunction
                        }
                    }
                }
            }
            str.Clear();
            return args;
        }

        public override string ToString()
        {
            //return "[" + sn + "][" + name + "]";
            return name + " " + sn + " " + "[" + remark + "]";
        }


        public int CompareTo(Function compareFunction)
        {
            if (compareFunction == null)
                return 1;

            else
                return this.posY.CompareTo(compareFunction.posY);
        }
        public override int GetHashCode()
        {
            return (int)(posY * 10);
        }



        public class Parameter
        {
            public string name;
            //public string direction; //20171024 項目 [direction 改成 option]
            public string option;
            public int type;
            //            public bool isInput;
            //public int variableSn = -1;//設置給該 Paramater 的 variable 之 sn ， -1 代表未設置
            public int variableSn = 0;//現在變成 0 未設置了
            public Parameter()
            {
                option = "IN";
                name = "none";
                type = 0;
  //              isInput = false;
            }
            public Parameter(string name, int type)
            {
                this.name = name;
                this.type = type;
    //            this.isInput = isInput;
            }

            public Parameter clone()
            {
                Parameter newParameter = new Parameter();
                newParameter.name = this.name;
                newParameter.option = this.option;
                newParameter.type = this.type;
                newParameter.variableSn = this.variableSn;
                return newParameter;
            }

        }

        
    }
}
