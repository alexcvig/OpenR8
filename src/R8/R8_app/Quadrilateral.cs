using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace R8
{
    //20170331 leo: R8 準備添加從介面框 rect 的功能，把這個從 A3 搬過來
    public class Quadrilateral
    {
        public Point LeftUp = new Point(0, 0);
        public Point RightUp = new Point(0, 0);
        public Point RightDown = new Point(0, 0);
        public Point LeftDown = new Point(0, 0);


        public bool Contains(Point p)
        {
            GraphicsPath range = new GraphicsPath();
            range.AddPolygon(new Point[] { LeftUp, RightUp, RightDown, LeftDown });

            return range.IsVisible(p);
        }

        public Quadrilateral Clone()
        {
            Quadrilateral q = new Quadrilateral(LeftUp, RightUp, RightDown, LeftDown);

            return q;
        }

        public Quadrilateral(Point Left_Up, Point Right_Up, Point Right_Down, Point Left_Down)
        {
            LeftUp = Left_Up;
            RightUp = Right_Up;
            RightDown = Right_Down;
            LeftDown = Left_Down;
        }

        public Quadrilateral()
        {
            LeftUp = new Point(0, 0);
            RightUp = new Point(0, 0);
            RightDown = new Point(0, 0);
            LeftDown = new Point(0, 0);
        }

        public Quadrilateral(Rectangle r)
        {
            LeftUp = r.Location;
            RightUp = new Point(r.X + r.Width, r.Y);
            RightDown = new Point(r.X + r.Width, r.Y + r.Height);
            LeftDown = new Point(r.X, r.Y + r.Height);
        }

        public Quadrilateral(int[] p)
        {
            if (p.Length == 8)
            {
                LeftUp.X = p[0];
                LeftUp.Y = p[1];
                RightUp.X = p[2];
                RightUp.Y = p[3];
                RightDown.X = p[4];
                RightDown.Y = p[5];
                LeftDown.X = p[6];
                LeftDown.Y = p[7];
            }
        }

        public void DrawQuadrilateral(Graphics g, Pen p)
        {
            g.DrawLine(p, LeftUp, RightUp);
            g.DrawLine(p, RightUp, RightDown);
            g.DrawLine(p, RightDown, LeftDown);
            g.DrawLine(p, LeftDown, LeftUp);
        }

        public void PointRatio(double ratio)
        {
            LeftUp.X = (int)(LeftUp.X * ratio);
            LeftUp.Y = (int)(LeftUp.Y * ratio);
            RightUp.X = (int)(RightUp.X * ratio);
            RightUp.Y = (int)(RightUp.Y * ratio);
            RightDown.X = (int)(RightDown.X * ratio);
            RightDown.Y = (int)(RightDown.Y * ratio);
            LeftDown.X = (int)(LeftDown.X * ratio);
            LeftDown.Y = (int)(LeftDown.Y * ratio);
        }

        public int[] GetPointArray()
        {
            int[] p = new int[8];

            p[0] = LeftUp.X;
            p[1] = LeftUp.Y;
            p[2] = RightUp.X;
            p[3] = RightUp.Y;
            p[4] = RightDown.X;
            p[5] = RightDown.Y;
            p[6] = LeftDown.X;
            p[7] = LeftDown.Y;

            return p;
        }

        public void DrawQuadrilateral(Graphics g, Color c, int lineWidth)
        {
            int width = 1;
            if (lineWidth > 1)
            {
                width = lineWidth;
            }
            g.DrawLine(new Pen(c, width), LeftUp, RightUp);
            g.DrawLine(new Pen(c, width), RightUp, RightDown);
            g.DrawLine(new Pen(c, width), RightDown, LeftDown);
            g.DrawLine(new Pen(c, width), LeftDown, LeftUp);
        }

        public void DrawQuadrilateral(Graphics g, Color up, Color right, Color down, Color left)
        {
            g.DrawLine(new Pen(up, 1), LeftUp, RightUp);
            g.DrawLine(new Pen(right, 1), RightUp, RightDown);
            g.DrawLine(new Pen(down, 1), RightDown, LeftDown);
            g.DrawLine(new Pen(left, 1), LeftDown, LeftUp);
        }

        public void DrawQuadrilateral(Graphics g, Color up, Color right, Color down, Color left, int lineWidth)
        {
            int width = 1;
            if (lineWidth > 1)
            {
                width = lineWidth;
            }
            g.DrawLine(new Pen(up, width), LeftUp, RightUp);
            g.DrawLine(new Pen(right, width), RightUp, RightDown);
            g.DrawLine(new Pen(down, width), RightDown, LeftDown);
            g.DrawLine(new Pen(left, width), LeftDown, LeftUp);
        }

        public Rectangle rect()
        {
            Rectangle r = new Rectangle(LeftUp, new Size(RightUp.X - LeftUp.X, LeftDown.Y - LeftUp.Y));

            return r;
        }

        /// <summary>
        /// 計算不規則四邊形面積函式。
        /// </summary>
        public static double Area(Quadrilateral quad)
        {
            if (quad == null)
            {
                return 0;
            }

            double quadArea = 0;

            quadArea = (quad.LeftUp.X * quad.LeftDown.Y + quad.LeftDown.X * quad.RightDown.Y +
                        quad.RightDown.X * quad.RightUp.Y + quad.RightUp.X * quad.LeftUp.Y -
                        quad.LeftUp.Y * quad.LeftDown.X - quad.LeftDown.Y * quad.RightDown.X -
                        quad.RightDown.Y * quad.RightUp.X - quad.RightUp.Y * quad.LeftUp.X) / 2;

            return Math.Abs(quadArea);
        }
    }
}
