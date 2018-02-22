using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace R8
{
   public class SizeableRectangle
    {
        public Rectangle rect;
        public Rectangle absRect;
        public int oldX;
        public int oldY;
        private int sizeNodeRect = 5;

        private int allowDeformingDuringMovement = 0;

        public posSizableRect nodeSelected = posSizableRect.None;

        public Color imageToolType;
        public static Color ROI = Color.LimeGreen;
        public static Color PATTERN = Color.Khaki;
        public static Color MASK = Color.Violet;
        public static Color PIN = Color.Pink;
        public static Color MARK = Color.DeepSkyBlue;
        public static Color FUNCTION = Color.DarkGreen;


        public enum posSizableRect
        {
            UpMiddle,
            BottomMiddle,
            LeftMiddle,
            RightMiddle,
            LeftBottom,
            LeftUp,
            RightUp,
            RightBottom,
            Middle,
            None
        };

        //public FormSelectedEditFunction formSelectedEditFunction;

        public SizeableRectangle(Rectangle r, Color COL)
        {
            rect = r;
            imageToolType = COL;
            absRect = new Rectangle(r.X * 2, r.Y * 2, r.Width * 2, r.Height * 2);
            //formSelectedEditFunction = new FormSelectedEditFunction();
        }

        //畫出框
        public void Draw(Graphics g)
        {
            foreach (posSizableRect pos in Enum.GetValues(typeof(posSizableRect)))
            {
                g.DrawRectangle(new Pen(imageToolType, 1), GetRect(pos));  // little 8 rectangles
            }

            g.DrawRectangle(new Pen(imageToolType, 1), rect); // outside big rectangle
        }

        public void TestIfRectInsideArea(PictureBox pb, ref int isMouseDown)
        {
            // Test if rectangle still inside the area.
            if (rect.X < 0) rect.X = 0;
            if (rect.Y < 0) rect.Y = 0;
            if (rect.Width <= 0) rect.Width = 1;
            if (rect.Height <= 0) rect.Height = 1;

            if (rect.X + rect.Width > pb.Width)
            {
                rect.Width = pb.Width - rect.X - 1; // -1 to be still show 
                if (allowDeformingDuringMovement == 0)
                {
                    isMouseDown = 0;
                }
            }
            if (rect.Y + rect.Height > pb.Height)
            {
                rect.Height = pb.Height - rect.Y - 1;// -1 to be still show 
                if (allowDeformingDuringMovement == 0)
                {
                    isMouseDown = 0;
                }
            }
        }

        private Rectangle CreateRectSizableNode(int x, int y)
        {
            return new Rectangle(x - sizeNodeRect / 2, y - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
        }

        private Rectangle GetRect(posSizableRect p)
        {
            switch (p)
            {
                case posSizableRect.LeftUp:
                    return CreateRectSizableNode(rect.X, rect.Y);

                case posSizableRect.LeftMiddle:
                    return CreateRectSizableNode(rect.X, rect.Y + +rect.Height / 2);

                case posSizableRect.LeftBottom:
                    return CreateRectSizableNode(rect.X, rect.Y + rect.Height);

                case posSizableRect.BottomMiddle:
                    return CreateRectSizableNode(rect.X + rect.Width / 2, rect.Y + rect.Height);

                case posSizableRect.RightUp:
                    return CreateRectSizableNode(rect.X + rect.Width, rect.Y);

                case posSizableRect.RightBottom:
                    return CreateRectSizableNode(rect.X + rect.Width, rect.Y + rect.Height);

                case posSizableRect.RightMiddle:
                    return CreateRectSizableNode(rect.X + rect.Width, rect.Y + rect.Height / 2);

                case posSizableRect.UpMiddle:
                    return CreateRectSizableNode(rect.X + rect.Width / 2, rect.Y);

                case posSizableRect.Middle:
                    return CreateRectSizableNode(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
                default:
                    return new Rectangle();
            }
        }

        public posSizableRect GetNodeSelectable(Point p)
        {
            foreach (posSizableRect r in Enum.GetValues(typeof(posSizableRect)))
            {
                if (GetRect(r).Contains(p))
                {
                    return r;
                }
            }
            return posSizableRect.None;
        }

        public bool GetPointInArea(Point p)
        {
            return this.rect.Contains(p);
        }

        public Cursor GetCursor(posSizableRect p)
        {
            switch (p)
            {
                case posSizableRect.LeftUp:
                    return Cursors.SizeNWSE;

                case posSizableRect.LeftMiddle:
                    return Cursors.SizeWE;

                case posSizableRect.LeftBottom:
                    return Cursors.SizeNESW;

                case posSizableRect.BottomMiddle:
                    return Cursors.SizeNS;

                case posSizableRect.RightUp:
                    return Cursors.SizeNESW;

                case posSizableRect.RightBottom:
                    return Cursors.SizeNWSE;

                case posSizableRect.RightMiddle:
                    return Cursors.SizeWE;

                case posSizableRect.UpMiddle:
                    return Cursors.SizeNS;

                case posSizableRect.Middle:
                    return Cursors.Hand;

                default:
                    return Cursors.Default;
            }
        }
    }
}
