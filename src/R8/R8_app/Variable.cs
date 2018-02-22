using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R8
{
    public class Variable
    {

        //public enum VariableType { Int, Bool, Float, Double, String, Image };
        // Bool 的值使用0或1表示
        // Mat 的值定義為 variableSn ，如果是 output ，值為它自己的 variableSn ，如果是 input ，值為輸入目標的 variableSn
        //public int sn = -1;
        public int sn = 0;
        public int type = 0;
        public string value = "";
        public string name = "null";
        public string remark = "";
        public Variable() {

        }

        public Variable clone()
        {
            Variable newVariable = new Variable();
            newVariable.sn = this.sn;
            newVariable.type = this.type;
            newVariable.value = this.value;
            newVariable.name = this.name;
            newVariable.remark = this.remark;
            return newVariable;
        }

        public override string ToString()
        {
            //20170209 leo: 討論後， Mat 類型的不印 value
            /*
            if (sn != -1)
            {
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, FormMain.r8.variables[sn].type);
                switch (str.ToString())
                {
                    case "image":
                    case "Image":
                    case "mat":
                    case "Mat":
                    case "json":
                    case "Json":
                        return "[" + sn + "][" + name + "]";
                        //break;
                    default:
                        break;
                }
                str.Clear();
            }
                    return "[" + sn + "][" + name + "][" + value + "]";
            */
            //20170309 leo: 依早上討論，顯示格式修改 sample path #1 [a2_sample3.png][remark]
            string returnStr = "";
            if (sn != -1 && sn != 0 && FormMain.r8.variables[sn] != null)
            {
                StringBuilder str = new StringBuilder(R7.STRING_SIZE);
                R8.GetVariableType(str, R7.STRING_SIZE, FormMain.r8.variables[sn].type);
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
                        //returnStr = name + " " + sn + " ";
                        returnStr = name;
                        break;
                    default:
                        //returnStr = name + " " + sn + " " + "[" + value + "]";
                        if (value == null || value.Length == 0 || value.Equals(""))
                        {
                            returnStr = name;
                        }
                        else
                        {
                            returnStr = name + " (" + value + ")";
                        }
                        break;
                }
                str.Clear();
            }
            
            if (remark.Length > 0) {
                returnStr += " (" + remark + ")";
            }
            return returnStr;
        }
    }
}
