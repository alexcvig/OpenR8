using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace R8
{
    public partial class FormImage : Form
    {
        Image orgImage = null;
        double zoomRate = 1.0;
        double zoomRateMin = 0.125;
        double zoomRateMax = 4;
        int mode = 0; //0 = show 圖 ，1 = 拉 rect ， 2 = 多個 rect(給 A3 的 mask 用) ; 3 = Rotate 用的 4 = Rotate 加裁切 
        private List<SizeableArea> listSQ = new List<SizeableArea>();//目前只提供單一 rect 但以後可能增加，總之與其它專案相同，開成 list
        Function function = null;
        FormFunction formFunction = null;
        int baseSn = 0;
        string imageName = "";
        double rotateValue = 0;

        public FormImage(int mode)
        {
            
            GC.Collect();
            this.mode = mode;
            if (mode == 1) {
                Rectangle rect = new Rectangle(100, 100, 100, 100);                
                SizeableArea sa = new SizeableArea(rect, Color.Blue);
                listSQ.Add(sa);
            }
            InitializeComponent();

            if (mode == 3 || mode == 4) {
                labelRotate.Visible = true;
                numericUpDownRotate.Visible = true;
            }
        }        

        public FormImage(int mode, Function function, FormFunction formFunction, int baseSn)
        {
  
            GC.Collect();
            this.mode = mode;
            this.function = function;
            this.formFunction = formFunction;
            this.baseSn = baseSn;
            if (mode == 1)//mode1: imageCut
            {
                Rectangle rect =
                    new Rectangle(R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(1).variableSn].value)
                    , R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(2).variableSn].value)
                    , R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(3).variableSn].value)
                    , R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(4).variableSn].value));
                SizeableArea sa = new SizeableArea(rect, Color.Blue);
                listSQ.Add(sa);                
            }
            else if (mode == 2) {//mode2: 多個 rect
                inputRectArrayString(FormMain.r8.variables[function.parameters.ElementAt(baseSn).variableSn].value);
            }
            InitializeComponent();
            if (mode != 0) {
                setToolStripMenuItem.Visible = true;
            }

            if (mode == 3 || mode == 4)
            {
                rotateValue = R8.stringToDouble(FormMain.r8.variables[function.parameters.ElementAt(baseSn).variableSn].value);
                labelRotate.Visible = true;
                numericUpDownRotate.Visible = true;
                numericUpDownRotate.Value = (decimal)rotateValue;

                if (mode == 4) {
                    Rectangle rect =
                    new Rectangle(R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(baseSn + 1).variableSn].value)
                    , R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(baseSn + 2).variableSn].value)
                    , R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(baseSn + 3).variableSn].value)
                    , R8.stringToInt(FormMain.r8.variables[function.parameters.ElementAt(baseSn + 4).variableSn].value));
                    SizeableArea sa = new SizeableArea(rect, Color.Blue);
                    listSQ.Add(sa);

                }
            }
        }

        private void colseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public void setImage(Image image, String imageName) {
            this.imageName = imageName;
            zoomRate = 1.0;
            orgImage = image;
            pictureBoxImage.Width = orgImage.Width;
            pictureBoxImage.Height = orgImage.Height;
            //pictureBoxImage.BackgroundImage = (Image)(new Bitmap(orgImage, pictureBoxImage.Size));
            pictureBoxImage.BackgroundImage = (Image)(rotatePictureBoxImage(new Bitmap(orgImage, pictureBoxImage.Size)));
            this.Text = this.imageName + " (" + (zoomRate * 100) + "%)";
            pictureBoxImage.Invalidate();
            return;
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GC.Collect();
            zoomRate *= 2;
            if (zoomRate > zoomRateMax) {
                zoomRate = zoomRateMax;
            }
            pictureBoxImage.Width = (int)(orgImage.Width * zoomRate);
            pictureBoxImage.Height = (int)(orgImage.Height * zoomRate);
            //pictureBoxImage.BackgroundImage = (Image)(new Bitmap(orgImage, pictureBoxImage.Size));
            pictureBoxImage.BackgroundImage = (Image)(rotatePictureBoxImage(new Bitmap(orgImage, pictureBoxImage.Size)));
            this.Text = this.imageName + " (" + (zoomRate * 100) + "%)";
            pictureBoxImage.Invalidate();
            return;
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GC.Collect();
            zoomRate /= 2;
            if (zoomRate < zoomRateMin)
            {
                zoomRate = zoomRateMin;
            }
            pictureBoxImage.Width = (int)(orgImage.Width * zoomRate);
            pictureBoxImage.Height = (int)(orgImage.Height * zoomRate);
            pictureBoxImage.BackgroundImage = (Image)(rotatePictureBoxImage(new Bitmap(orgImage, pictureBoxImage.Size)));
            this.Text = this.imageName + " (" + (zoomRate * 100) + "%)";
            pictureBoxImage.Invalidate();
            return;
        }

        //20170331 leo: 畫框框系列 function，從 A3 搬過來用

        private int isMouseDown = 0;
        private int isAction = 0;
        private int isQuadMove = 0;
        private void pictureBoxImage_Paint(object sender, PaintEventArgs e)
        {
            foreach (SizeableArea sa in listSQ)
            {
                sa.Draw(e.Graphics, zoomRate);
            }
            return;
        }

        private void contextMenuStrip_itemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Point oriPoint = new Point((int)(mouseRightDownPointX / zoomRate), (int)(mouseRightDownPointY / zoomRate));
            switch (e.ClickedItem.Text) {
                case "Add":
                    {
                        int w = 100;
                        int h = 100;
                        if (listSQ.Count > 0) {
                            w = listSQ.ElementAt(listSQ.Count - 1).rect.Width;
                            h = listSQ.ElementAt(listSQ.Count - 1).rect.Height;
                        }
                        Rectangle rect = new Rectangle(oriPoint.X, oriPoint.Y, w, h);
                        SizeableArea sa = new SizeableArea(rect, Color.Blue);
                        listSQ.Add(sa);
                        pictureBoxImage.Invalidate();
                    }
                    break;
                case "Remove":
                    {
                        
                        for (int i = listSQ.Count - 1; i > -1; i--) {
                            SizeableArea sq = listSQ.ElementAt(i);
                            if (sq.isRect == 1)
                            {
                                if (sq.rect.Contains(new Point(oriPoint.X, oriPoint.Y)))
                                {
                                    listSQ.Remove(sq);
                                    break;
                                }
                            }
                        }
                        pictureBoxImage.Invalidate();
                    }
                    break;
                case "Clear All":
                    listSQ.Clear();
                    pictureBoxImage.Invalidate();
                    break;
            }
        }
        private string outputRectArrayString() {
            StringBuilder str = new StringBuilder();
            //格式：
            /*
             [
             {"x": "725", "y": "15", "width": "110", "height": "110"},
             {"x": "20", "y": "25", "width": "110", "height": "110"},
             {"x": "732", "y": "485", "width": "110", "height": "110"},
             {"x": "25", "y": "490", "width": "110", "height": "110"}
             ]
            */
            str.Append("[");
            bool isFirst = true;
            for (int i = 0; i < listSQ.Count; i++)
            {
                SizeableArea sq = listSQ.ElementAt(i);
                if (sq.isRect == 1)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else {
                        str.Append(",");
                    }
                    str.Append("{\"x\": \"" + sq.rect.X + "\", \"y\": \"" + sq.rect.Y + "\", \"width\": \"" + sq.rect.Width + "\", \"height\": \"" + sq.rect.Height + "\"}");
                }
            }

            str.Append("]");
            return str.ToString();
        }

        private int inputRectArrayString(string str)
        {
            if (str == null || str.Length < 2) { //json array 至少會有前後刮符，至少有2個字元
                return -1;
            }
            /*
             [
             {"x": "725", "y": "15", "width": "110", "height": "110"},
             {"x": "20", "y": "25", "width": "110", "height": "110"},
             {"x": "732", "y": "485", "width": "110", "height": "110"},
             {"x": "25", "y": "490", "width": "110", "height": "110"}
             ]
            */
            //MessageBox.Show(str);
            //Rectangle rect = new Rectangle();
            //string 避免 user 亂填導致 error ， try catch 一下
            try
            {
                int start = str.IndexOf("{");
                if (start == -1) {
                    return -2;
                }
                int end = str.IndexOf("}");
                string targetStr = str.Substring(start, end - start);
                string[] rectStrings;
                string objectString;
                int subStart, subEnd;
                string key, value;
                while (start != -1)
                {
                    end = str.IndexOf("}", start + 1);
                    targetStr = str.Substring(start, end - start + 1);
                    //MessageBox.Show(targetStr);
                    start = str.IndexOf("{", start + 1);
                    rectStrings = targetStr.Split(',');
                    Rectangle rect = new Rectangle(0, 0, 10, 10);
                    for (int i = 0; i < rectStrings.Length; i++)
                    {
                        objectString = rectStrings[i];
                        //每個 objectString 裡面會有4個 " 符號
                        subStart = objectString.IndexOf("\"");
                        subEnd = objectString.IndexOf("\"", subStart + 1);
                        //冒號本身不抓所以要去頭去尾 
                        key = objectString.Substring(subStart + 1, subEnd - subStart - 1);

                        subStart = objectString.IndexOf("\"", subEnd + 1);
                        subEnd = objectString.IndexOf("\"", subStart + 1);
                        value = objectString.Substring(subStart + 1, subEnd - subStart - 1);
                        //MessageBox.Show("key = " + key + " value = " + value);
                        switch (key)
                        {
                            case "x":
                                rect.X = R8.stringToInt(value);
                                break;
                            case "y":
                                rect.Y = R8.stringToInt(value);
                                break;
                            case "width":
                                rect.Width = R8.stringToInt(value);
                                break;
                            case "height":
                                rect.Height = R8.stringToInt(value);
                                break;
                        }
                    }
                    SizeableArea sa = new SizeableArea(rect, Color.Blue);
                    listSQ.Add(sa);
                }
            } catch (Exception e) {
                MessageBox.Show("Decode rect array string error, target string may be not a rect array." + e.ToString());
            }
            
            return 1;
        }

        //https://msdn.microsoft.com/zh-tw/library/system.windows.forms.contextmenustrip(v=vs.110).aspx
        private ContextMenuStrip contextMenuStrip;

        private int mouseRightDownPointX = 0;
        private int mouseRightDownPointY = 0;
        private void pictureBoxImage_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right && mode == 2)
            {
                contextMenuStrip = new ContextMenuStrip();
                contextMenuStrip.Items.Add("Add");
                contextMenuStrip.Items.Add("Remove");
                contextMenuStrip.Items.Add("Clear All");

                Point point = PointToClient(Cursor.Position);
                //Point point = PointToScreen(Cursor.Position);
                //Point point = PointToClient(new Point(e.X, e.Y));
                //Point point = PointToClient(Form.MousePosition);
                contextMenuStrip.ItemClicked += contextMenuStrip_itemClicked;
                mouseRightDownPointX = e.X;
                mouseRightDownPointY = e.Y;
                contextMenuStrip.Show(this, point);
                return;
            }



            isMouseDown = 1;

            if (listSQ.Count == 0)
            {
                return;
            }

            isQuadMove = 1;

            List<SizeableArea> listMoveSQ = new List<SizeableArea>();

            // 算原圖 Size 的位置
            Point oriPoint = new Point((int)(e.X / zoomRate), (int)(e.Y / zoomRate));

            foreach (SizeableArea sq in listSQ)
            {
                sq.isMove = 0;

                if (sq.isRect == 1)
                {
                    if (sq.rect.Contains(new Point(oriPoint.X, oriPoint.Y)))
                    {
                        listMoveSQ.Add(sq);
                    }
                }
                else
                {
                    if (sq.quad.Contains(new Point(oriPoint.X, oriPoint.Y)))
                    {
                        listMoveSQ.Add(sq);
                    }
                }

                sq.nodeSelected = SizeableArea.PosSizableQuad.None;
                sq.nodeSelected = sq.GetNodeSelectable(oriPoint);
                if (sq.nodeSelected != SizeableArea.PosSizableQuad.None)
                {
                    isQuadMove = 0;
                    isAction = 1;
                }

                sq.oldQuad = new Quadrilateral(sq.quad.GetPointArray());
                sq.oldRect = new Rectangle(sq.rect.X, sq.rect.Y, sq.rect.Width, sq.rect.Height);
                sq.oldX = oriPoint.X;
                sq.oldY = oriPoint.Y;
            }

            if (isQuadMove == 1)
            {
                //double size = pictureBoxImage.Width * pictureBoxImage.Height * 100; //這東西會 overflow......換下面那行
                double size = double.MaxValue;
                List<SizeableArea> moveSQ = new List<SizeableArea>();

                //string str = "";
                foreach (SizeableArea sq in listMoveSQ)
                {
                    //str += " " + sq.isRect;
                    if (SizeableArea.calQuadArea(sq) < size)
                    {
                        moveSQ.Clear();
                        moveSQ.Add(sq);
                        size = SizeableArea.calQuadArea(sq);
                       // str += " moveSQ ";
                    }
                }
                //this.Text = str;
                foreach (SizeableArea sq in moveSQ)
                //foreach (SizeableArea sq in listSQ)
                {
                    isAction = 1;
                    sq.isMove = 1;
                    //this.Text = "MouseDown sq" + sq.rect + " is move";
                    break;
                }
            }
            
        }


        //旋轉用
        //來源 https://stackoverflow.com/questions/472428/rotate-image-math-c
        private Bitmap rotatePictureBoxImage(Bitmap bmpSrc) {
            //pictureBoxImage.Width
            Matrix mRotate = new Matrix();
            mRotate.Translate(Convert.ToInt32(bmpSrc.Width) / -2, Convert.ToInt32(bmpSrc.Height) / -2, MatrixOrder.Append);
            mRotate.RotateAt((float)-rotateValue, new Point(0, 0), MatrixOrder.Append);

            using (GraphicsPath gp = new GraphicsPath())
            {  // transform image points by rotation matrix
                gp.AddPolygon(new Point[] { new Point(0, 0), new Point(Convert.ToInt32(bmpSrc.Width), 0), new Point(0, Convert.ToInt32(bmpSrc.Height)) });
                gp.Transform(mRotate);
                PointF[] pts = gp.PathPoints;

                // create destination bitmap sized to contain rotated source image
                Bitmap bmpDest = new Bitmap(bmpSrc.Width, bmpSrc.Height);


                using (Graphics gDest = Graphics.FromImage(bmpDest))
                {  // draw source into dest
                    
                    SolidBrush blueBrush = new SolidBrush(Color.Black);
                    Rectangle rect = new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height);
                    gDest.FillRectangle(blueBrush, rect);

                    Matrix mDest = new Matrix();
                    mDest.Translate(bmpDest.Width / 2, bmpDest.Height / 2, MatrixOrder.Append);
                    gDest.Transform = mDest;
                    gDest.DrawRectangle(Pens.Black, new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height));
                    gDest.DrawImage(bmpSrc, pts);
                    //gDest.DrawRectangle(Pens.Transparent, new Rectangle(0, 0, bmpSrc.Width, bmpSrc.Height));
                    //drawAxes(gDest, Color.Red, 0, 0, 1, 100, "");
                    return bmpDest;
                }
            }
        }

        private void pictureBoxImage_MouseMove(object sender, MouseEventArgs e)
        {
             //this.Text = "( " + e.X + ", " + e.Y + ")";
            if (listSQ.Count == 0)
            {
                return;
            }

            // 算原圖 Size 的位置
            Point oriPoint = new Point((int)(e.X / zoomRate), (int)(e.Y / zoomRate));

            if (isMouseDown == 0 || isAction == 1)
            {
                isAction = 0;

                int isDefault = 1;


                foreach (SizeableArea sq in listSQ)
                {
                    if (sq.GetCursor(sq.GetNodeSelectable(oriPoint)) != Cursors.Default)
                    {
                        pictureBoxImage.Cursor = sq.GetCursor(sq.GetNodeSelectable(oriPoint));
                        isDefault = 0;
                        break;
                    }
                }


                if (isDefault == 1)
                {
                    foreach (SizeableArea sq in listSQ)
                    {
                        if (sq.isRect == 1)
                        {
                            if (sq.rect.Contains(oriPoint))
                            {
                                pictureBoxImage.Cursor = Cursors.Hand;
                                isDefault = 0;
                                break;
                            }
                        }
                        else
                        {
                            if (sq.quad.Contains(oriPoint))
                            {
                                pictureBoxImage.Cursor = Cursors.Hand;
                                isDefault = 0;
                                break;
                            }
                        }
                    }
                }

                if (isDefault == 1)
                {
                    pictureBoxImage.Cursor = Cursors.Default;
                }


                return;
            }

            foreach (SizeableArea sq in listSQ)
            {
                int isFind = 1;

                if (sq.isRect == 1)
                {
                    Rectangle backupRect = sq.rect;
                    switch (sq.nodeSelected)
                    {
                        case SizeableArea.PosSizableQuad.LeftUp:
                            sq.rect.Location = oriPoint;
                            sq.rect.Width = sq.oldRect.Width - (oriPoint.X - sq.oldRect.X);
                            sq.rect.Height = sq.oldRect.Height - (oriPoint.Y - sq.oldRect.Y);
                            break;

                        case SizeableArea.PosSizableQuad.LeftMiddle:
                            sq.rect.Location = new Point(oriPoint.X, sq.oldRect.Y);
                            sq.rect.Width = sq.oldRect.Width - (oriPoint.X - sq.oldRect.X);
                            break;

                        case SizeableArea.PosSizableQuad.LeftDown:
                            sq.rect.Location = new Point(oriPoint.X, sq.oldRect.Y);
                            sq.rect.Width = sq.oldRect.Width - (oriPoint.X - sq.oldRect.X);
                            sq.rect.Height = sq.oldRect.Height + (oriPoint.Y - sq.oldRect.Height - sq.oldRect.Y);
                            break;

                        case SizeableArea.PosSizableQuad.DownMiddle:
                            sq.rect.Location = new Point(sq.oldRect.X, sq.oldRect.Y);
                            sq.rect.Height = sq.oldRect.Height + (oriPoint.Y - sq.oldRect.Height - sq.oldRect.Y);
                            break;

                        case SizeableArea.PosSizableQuad.RightDown:
                            sq.rect.Location = new Point(sq.oldRect.X, sq.oldRect.Y);
                            sq.rect.Width = sq.oldRect.Width + (oriPoint.X - sq.oldRect.Width - sq.oldRect.X);
                            sq.rect.Height = sq.oldRect.Height + (oriPoint.Y - sq.oldRect.Height - sq.oldRect.Y);
                            break;


                        case SizeableArea.PosSizableQuad.RightMiddle:
                            sq.rect.Location = new Point(sq.oldRect.X, sq.oldRect.Y);
                            sq.rect.Width = sq.oldRect.Width + (oriPoint.X - sq.oldRect.Width - sq.oldRect.X);
                            break;

                        case SizeableArea.PosSizableQuad.RightUp:
                            sq.rect.Location = new Point(sq.oldRect.X, oriPoint.Y);
                            sq.rect.Width = sq.oldRect.Width + (oriPoint.X - sq.oldRect.Width - sq.oldRect.X);
                            sq.rect.Height = sq.oldRect.Height - (oriPoint.Y - sq.oldRect.Y);
                            break;

                        case SizeableArea.PosSizableQuad.UpMiddle:
                            sq.rect.Location = new Point(sq.oldRect.X, oriPoint.Y);
                            sq.rect.Height = sq.oldRect.Height - (oriPoint.Y - sq.oldRect.Y);
                            break;

                        default:
                            if (sq.isMove == 1 && isQuadMove == 1)
                            {
                                sq.rect.Location = new Point(sq.oldRect.X + oriPoint.X - sq.oldX, sq.oldRect.Y + oriPoint.Y - sq.oldY);
                            }
                            else
                            {
                                isFind = 0;
                            }
                            break;
                    }
                    if (isFind == 1)
                    {
                        sq.isShowCross = true;
                    }
                    if (sq.rect.Width < 4 || sq.rect.Height < 4)
                    {
                        sq.rect = backupRect;
                    }
                }
                else
                {
                    switch (sq.nodeSelected)
                    {
                        case SizeableArea.PosSizableQuad.LeftUp:
                            sq.quad.LeftUp = oriPoint;
                            break;

                        case SizeableArea.PosSizableQuad.LeftMiddle:
                            sq.quad.LeftUp.X = sq.oldQuad.LeftUp.X + oriPoint.X - sq.oldX;
                            sq.quad.LeftUp.Y = sq.oldQuad.LeftUp.Y + oriPoint.Y - sq.oldY;
                            sq.quad.LeftDown.X = sq.oldQuad.LeftDown.X + oriPoint.X - sq.oldX;
                            sq.quad.LeftDown.Y = sq.oldQuad.LeftDown.Y + oriPoint.Y - sq.oldY;
                            break;

                        case SizeableArea.PosSizableQuad.LeftDown:
                            sq.quad.LeftDown = oriPoint;
                            break;

                        case SizeableArea.PosSizableQuad.DownMiddle:
                            sq.quad.LeftDown.X = sq.oldQuad.LeftDown.X + oriPoint.X - sq.oldX;
                            sq.quad.LeftDown.Y = sq.oldQuad.LeftDown.Y + oriPoint.Y - sq.oldY;
                            sq.quad.RightDown.X = sq.oldQuad.RightDown.X + oriPoint.X - sq.oldX;
                            sq.quad.RightDown.Y = sq.oldQuad.RightDown.Y + oriPoint.Y - sq.oldY;
                            break;

                        case SizeableArea.PosSizableQuad.RightDown:
                            sq.quad.RightDown = oriPoint;
                            break;

                        case SizeableArea.PosSizableQuad.RightMiddle:
                            sq.quad.RightDown.X = sq.oldQuad.RightDown.X + oriPoint.X - sq.oldX;
                            sq.quad.RightDown.Y = sq.oldQuad.RightDown.Y + oriPoint.Y - sq.oldY;
                            sq.quad.RightUp.X = sq.oldQuad.RightUp.X + oriPoint.X - sq.oldX;
                            sq.quad.RightUp.Y = sq.oldQuad.RightUp.Y + oriPoint.Y - sq.oldY;
                            break;

                        case SizeableArea.PosSizableQuad.RightUp:
                            sq.quad.RightUp = oriPoint;
                            break;

                        case SizeableArea.PosSizableQuad.UpMiddle:
                            sq.quad.RightUp.X = sq.oldQuad.RightUp.X + oriPoint.X - sq.oldX;
                            sq.quad.RightUp.Y = sq.oldQuad.RightUp.Y + oriPoint.Y - sq.oldY;
                            sq.quad.LeftUp.X = sq.oldQuad.LeftUp.X + oriPoint.X - sq.oldX;
                            sq.quad.LeftUp.Y = sq.oldQuad.LeftUp.Y + oriPoint.Y - sq.oldY;
                            break;

                        default:
                            if (sq.isMove == 1 && isQuadMove == 1)
                            {
                                sq.quad.LeftUp.X = sq.oldQuad.LeftUp.X + oriPoint.X - sq.oldX;
                                sq.quad.LeftUp.Y = sq.oldQuad.LeftUp.Y + oriPoint.Y - sq.oldY;
                                sq.quad.LeftDown.X = sq.oldQuad.LeftDown.X + oriPoint.X - sq.oldX;
                                sq.quad.LeftDown.Y = sq.oldQuad.LeftDown.Y + oriPoint.Y - sq.oldY;
                                sq.quad.RightUp.X = sq.oldQuad.RightUp.X + oriPoint.X - sq.oldX;
                                sq.quad.RightUp.Y = sq.oldQuad.RightUp.Y + oriPoint.Y - sq.oldY;
                                sq.quad.RightDown.X = sq.oldQuad.RightDown.X + oriPoint.X - sq.oldX;
                                sq.quad.RightDown.Y = sq.oldQuad.RightDown.Y + oriPoint.Y - sq.oldY;
                            }
                            else
                            {
                                isFind = 0;
                            }
                            break;
                    }
                }
                pictureBoxImage.Invalidate();

                if (isFind == 1)
                {
                    break;
                }
            }
        }
        private void pictureBoxImage_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = 0;
            isAction = 0;
            isQuadMove = 0;

            foreach (SizeableArea sq in listSQ)
            {

                if (sq.isRect == 1)
                {
                    sq.isShowCross = false;
                }
            }
            pictureBoxImage.Invalidate();
        }

        private void setToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mode == 1 && listSQ.Count > 0)
            {
                int[] tempSize = { listSQ.ElementAt(0).rect.X, listSQ.ElementAt(0).rect.Y, listSQ.ElementAt(0).rect.Width, listSQ.ElementAt(0).rect.Height };
                for (int i = 0; i < 4; i++)
                {
                    if (function.parameters.ElementAt(baseSn + i).variableSn != -1 && function.parameters.ElementAt(baseSn + i).variableSn != 0)
                    {
                        //function.parameters.ElementAt(baseSn + i).addData("");
                        Variable variable = FormMain.r8.variables[function.parameters.ElementAt(baseSn + i).variableSn];
                        //                        variable.value = subString;
                        variable.value = "" + tempSize[i];
                        formFunction.PanelParameterList.ElementAt(baseSn + i).editVariable(variable);
                    }
                }
                this.Close();
            }
            else if (mode == 2)
            {
                String str = outputRectArrayString();
                //MessageBox.Show(str);
                Variable variable = FormMain.r8.variables[function.parameters.ElementAt(baseSn).variableSn];
                variable.value = str;
                formFunction.PanelParameterList.ElementAt(baseSn).editVariable(variable);
                this.Close();
            }
            else if (mode == 3) {
                if (function.parameters.ElementAt(baseSn).variableSn != -1 && function.parameters.ElementAt(baseSn).variableSn != 0)
                {
                    Variable variable = FormMain.r8.variables[function.parameters.ElementAt(baseSn).variableSn];
                    variable.value = "" + rotateValue;
                    formFunction.PanelParameterList.ElementAt(baseSn).editVariable(variable);
                }
               
                this.Close();

            }
            else if (mode == 4)
            {
                if (function.parameters.ElementAt(baseSn).variableSn != -1 && function.parameters.ElementAt(baseSn).variableSn != 0)
                {
                    Variable variable = FormMain.r8.variables[function.parameters.ElementAt(baseSn).variableSn];
                    variable.value = "" + rotateValue;
                    formFunction.PanelParameterList.ElementAt(baseSn).editVariable(variable);
                }

                int[] tempSize = { listSQ.ElementAt(0).rect.X, listSQ.ElementAt(0).rect.Y, listSQ.ElementAt(0).rect.Width, listSQ.ElementAt(0).rect.Height };
                for (int i = 1; i < 5; i++)
                {
                    if (function.parameters.ElementAt(baseSn + i).variableSn != -1 && function.parameters.ElementAt(baseSn + i).variableSn != 0)
                    {
                        //function.parameters.ElementAt(baseSn + i).addData("");
                        Variable variable = FormMain.r8.variables[function.parameters.ElementAt(baseSn + i).variableSn];
                        //                        variable.value = subString;
                        variable.value = "" + tempSize[i - 1];
                        formFunction.PanelParameterList.ElementAt(baseSn + i).editVariable(variable);
                    }
                }
                this.Close();

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //orgImage
            if (orgImage != null)
            {
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                //png, bmp, tif, jpg
                saveFileDialog1.Filter = "png Files|*.png|bmp Files|*.bmp|tif Files|*.tif|jpg Files|*.jpg";
                saveFileDialog1.Title = "Save Json";
                saveFileDialog1.InitialDirectory = FormMain.workSpacePath;
                saveFileDialog1.FileName = "1.png";
                if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //MessageBox.Show("FilterIndex = " + saveFileDialog1.FilterIndex);
                    switch (saveFileDialog1.FilterIndex) {
                        case 1:
                            orgImage.Save(saveFileDialog1.FileName, ImageFormat.Png);
                            break;
                        case 2:
                            orgImage.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
                            break;
                        case 3:
                            orgImage.Save(saveFileDialog1.FileName, ImageFormat.Tiff);
                            break;
                        case 4:
                            orgImage.Save(saveFileDialog1.FileName, ImageFormat.Jpeg);
                            break;
                    }   
                    //R8.writeProgramXml(saveFileDialog1.FileName);
                    //File.WriteAllText(saveFileDialog1.FileName, richTextBox1.Text.ToString().Replace("\n", "\r\n"));
                }
            }
        }

        private void FormImage_SizeChanged(object sender, EventArgs e)
        {

            panel1.Height = this.ClientSize.Height - panel1.Location.Y;
            panel1.Width = this.ClientSize.Width;
        }

        private void FormImage_Load(object sender, EventArgs e)
        {
            panel1.Height = this.ClientSize.Height - panel1.Location.Y;
            panel1.Width = this.ClientSize.Width;
        }

        private void pictureBoxImage_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void FormImage_FormClosed(object sender, FormClosedEventArgs e)
        {
            //FormMain formMain = this.MdiParent as FormMain;
            //formMain.formImage = null;
        }


        private void numericUpDownRotate_ValueChanged(object sender, EventArgs e)
        {
            //由於影像通常不是正的，框選 ROI 及 View Image 視窗上方加上輸入旋轉度數的輸入框
            rotateValue = (double)(numericUpDownRotate.Value);
            if (orgImage != null)
            {
                pictureBoxImage.BackgroundImage = (Image)(rotatePictureBoxImage(new Bitmap(orgImage, pictureBoxImage.Size)));

                this.Text = this.imageName + " (" + (zoomRate * 100) + "%)";
                pictureBoxImage.Invalidate();
            }
        }
    }

}
