using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static R8.R8Program.Function2;

namespace R8
{
    //20170112 leo: 建立一個 class 存 Program 的結構
    class R8Program
    {

        private int functionSnCount = 1;
        private string version = "0.0.0.0";
        public Start start = new Start();
        public List<Function2> functionList = new List<Function2>();

        public R8Program()
        {
            //目前 Function 未定義，先固定產生一個
            functionList.Add(new Function2());
        }

        public R8Program(XElement xml)
        {

            XElement element = xml.Element("functionSnCount");
            if (!Int32.TryParse(element.Value, out functionSnCount)) {
                functionSnCount = -1;
            }

            element = xml.Element("version");
            version = element.Value.ToString();

            element = xml.Element("start").Element("posX");
            if (!Int32.TryParse(element.Value, out start.posX))
            {
                start.posX = -1;
            }

            element = xml.Element("start").Element("posY");
            if (!Int32.TryParse(element.Value, out start.posY))
            {
                start.posY = -1;
            }


            //IEnumerable<XElement> functionElementst = xml.Element("function").Elements("function");
            IEnumerable<XElement> functionElements = xml.Elements("function");
            IEnumerable<XElement> function2Elements;
            Function2 function2;
            Function3 function3;
            int res;
            for (int i = 0; i < functionElements.Count(); i++)
            {
                function2Elements = functionElements.ElementAt(i).Elements("function");
                function2 = new Function2();
                
                element = functionElements.ElementAt(i);
                if (!Int32.TryParse(element.Element("sn").Value, out res))
                {
                    res = -1;
                }
                function2.sn = res;

                if (!Int32.TryParse(element.Element("functionSnCount").Value, out res))
                {
                    res = -1;
                }
                function2.functionSnCount = res;

                for (int j = 0; j < functionElements.Count(); j++)
                {
                    element = functionElements.ElementAt(j);
                    function3 = new Function3();
                    function3.name = element.Element("name").Value;
                    if (!Int32.TryParse(element.Element("posX").Value, out res))
                    {
                        res = -1;
                    }
                    function3.rectangle.X = res;
                    if (!Int32.TryParse(element.Element("posY").Value, out res))
                    {
                        res = -1;
                    }
                    function3.rectangle.Y = res;
                    if (!Int32.TryParse(element.Element("sn").Value, out function3.sn))
                    {
                        function3.sn = -1;
                    }

                    //Console.WriteLine("function.name = " + function.name);
                    //Console.WriteLine("function.posX = " + function.posX);
                    //Console.WriteLine("function.posY = " + function.posY);
                    //Console.WriteLine("function.sn = " + function.sn);
                    function2.functionList.Add(function3);
                }
                functionList.Add(function2);
                //Console.WriteLine("snCount = " + snCount);
                //Console.WriteLine("version = " + version);
                //Console.WriteLine("start.posX = " + start.posX);

            }
        }

        public int getSnCount() {
            return functionSnCount;
        }

        public string getVersion() {
            return version;
        }

    

        public class Start {
            public int posX = 0;
            public int posY = 0;
           public Start() {

            }
        }

        public class Function2
        {
            public List<Function3> functionList = new List<Function3>();
            public int sn = 1;
            public int functionSnCount = 0;
            public Function2() {

            }

            public int getSnCount()
            {
                return functionSnCount;
            }


            public int addFunction(Function3 function3)
            {
                functionSnCount++;
                function3.sn = functionSnCount;
                functionList.Add(function3);
                return 1;
            }


            public class Function3
            {

                public static int DefaultRectangleWidth = 100;
                public static int DefaultRectangleHeight = 40;

                public int sn = 0;
                public string name = "";
                //public int posX = 0;
                //public int posY = 0;
                public Rectangle rectangle = new Rectangle(0, 0, DefaultRectangleWidth, DefaultRectangleHeight);
                //塞參數用的 List
                public List<int> intArgs = new List<int>();
                public List<double> doubleArgs = new List<double>();
                public List<string> stringArgs = new List<string>();

                public Function3()
                {

                }

                public int getPosX()
                {
                    return rectangle.X;
                }

                public int getPosY()
                {
                    return rectangle.Y;
                }

                public Function3(string name, int posX, int posY)
                {
                    this.name = name;
                    this.rectangle.X = posX;
                    this.rectangle.Y = posY;
                    this.rectangle.Width = DefaultRectangleWidth;
                    this.rectangle.Height = DefaultRectangleHeight;
                }
            }
        }
        
    }
}
