using System;
using System.Drawing;
using System.Windows.Forms;

namespace R8
{
    //20170331 leo: R8 準備添加從介面框 rect 的功能，把這個從 A3 搬過來
    public class SizeableArea
    {

        public static Rectangle rectRatio(double ratio, Rectangle r)
        {
            Rectangle rect = new Rectangle();
            rect.X = (int)(r.X * ratio);
            rect.Y = (int)(r.Y * ratio);
            rect.Width = (int)(r.Width * ratio);
            rect.Height = (int)(r.Height * ratio);

            return rect;
        }


        public static Quadrilateral quadRatio(double ratio, Quadrilateral q)
        {
            Quadrilateral quad = new Quadrilateral();
            quad.LeftUp = new Point((int)(q.LeftUp.X * ratio), (int)(quad.LeftUp.Y * ratio));
            quad.RightUp = new Point((int)(q.RightUp.X * ratio), (int)(quad.RightUp.Y * ratio));
            quad.RightDown = new Point((int)(q.RightDown.X * ratio), (int)(quad.RightDown.Y * ratio));
            quad.LeftDown = new Point((int)(q.LeftDown.X * ratio), (int)(quad.LeftDown.Y * ratio));

            return quad;
        }


        public class AreaRect
        {
            private Color color;
            public Rectangle rect;
            public Pen pen;


            public AreaRect(Color c)
            {
                color = c;
                pen = new Pen(color, 1);
                rect = new Rectangle(0, 0, 0, 0);
            }

            public AreaRect Clone()
            {
                AreaRect areaRectClone = new AreaRect(color);
                areaRectClone.rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

                return areaRectClone;
            }

            public Rectangle rectRatio(double ratio)
            {
                return SizeableArea.rectRatio(ratio, rect);
            }

            public static AreaRect[] InitArray(int num, Color c)
            {
                AreaRect[] areaRect = new AreaRect[num];
                int i;
                for (i = 0; i < num; i++)
                {
                    areaRect[i] = new AreaRect(c);
                }

                return areaRect;
            }
        }
        public class AreaQuad
        {
            private Color color;
            public Quadrilateral quadOriginal = new Quadrilateral();
            public Pen pen;

            public AreaQuad(Color c)
            {
                color = c;
                pen = new Pen(color, 1);
            }

            public AreaQuad Clone()
            {
                AreaQuad areaQuadClone = new AreaQuad(color);
                areaQuadClone.quadOriginal = quadOriginal.Clone();

                return areaQuadClone;
            }

            public static AreaQuad[] initArea(int num, Color c)
            {
                AreaQuad[] areaQuad = new AreaQuad[num];
                int i;
                for (i = 0; i < num; i++)
                {
                    areaQuad[i] = new AreaQuad(c);
                }

                return areaQuad;
            }
        }

        public Quadrilateral quad;
        public Rectangle rect;
        public int isRect = 0;
        private int sizeNodeRect = 10;

        public Rectangle oldRect;
        public Quadrilateral oldQuad;
        public int oldX;
        public int oldY;

        public int isMove = 0;

        public static Color ROI = Color.LimeGreen;
        public static Color PATTERN = Color.DodgerBlue;
        // 框選
        public static Color SEARCH = Color.LimeGreen;
        public static Color CHIP = Color.Red;
        public static Color CROSS = Color.Violet;
        public static Color FRAME = Color.MediumPurple;
        // 複數框選
        public static Color FINGER = Color.Gold;
        public static Color FINGERMASK = Color.SkyBlue;
        public static Color MASK = Color.Turquoise;
        public Color quadType;

        public PosSizableQuad nodeSelected = PosSizableQuad.None;

        public int isDash = 0;

        //20170705 leo: Kelly 說 A3 客戶要[框框有十字讓它們可以對中心點]
        public bool isShowCross = false;


        public enum PosSizableQuad
        {
            UpMiddle,
            DownMiddle,
            LeftMiddle,
            RightMiddle,
            LeftDown,
            LeftUp,
            RightUp,
            RightDown,
            None
        };

        public SizeableArea(Quadrilateral q, Color col)
        {
            quad = q.Clone();
            rect = new Rectangle();
            quadType = col;
            isRect = 0;
        }

        public SizeableArea(Rectangle r, Color col)
        {
            rect = r;
            quad = new Quadrilateral();
            quadType = col;
            isRect = 1;
            isDash = 0;
        }

        public void DrawRect(Graphics g, Rectangle rectDraw, double ratio)
        {
            foreach (PosSizableQuad pos in Enum.GetValues(typeof(PosSizableQuad)))
            {
                g.DrawRectangle(new Pen(quadType, 1), GetRect(pos, ratio));
            }

            Pen p = new Pen(quadType, 1);
            if (isDash == 1)
            {
                float[] dashValues = { 15, 10 };
                p.DashPattern = dashValues;
            }

            g.DrawRectangle(p, rectDraw);

            if (isShowCross) {
                Pen p2 = new Pen(Color.LimeGreen, 1);
                g.DrawLine(p2, new Point(rectDraw.X + rectDraw.Width / 2, rectDraw.Y), new Point(rectDraw.X + rectDraw.Width / 2, rectDraw.Y + rectDraw.Height));
                g.DrawLine(p2, new Point(rectDraw.X, rectDraw.Y + rectDraw.Height / 2), new Point(rectDraw.X + rectDraw.Width, rectDraw.Y + rectDraw.Height / 2));
            }
        }

        public void DrawQuad(Graphics g, Quadrilateral quadDraw, double ratio)
        {
            foreach (PosSizableQuad pos in Enum.GetValues(typeof(PosSizableQuad)))
            {
                g.DrawRectangle(new Pen(quadType, 1), GetRect(pos, ratio));
            }

            Pen p = new Pen(quadType, 1);
            if (isDash == 1)
            {
                float[] dashValues = { 15, 10 };
                p.DashPattern = dashValues;
            }

            quadDraw.DrawQuadrilateral(g, p);
        }

        public void Draw(Graphics g, double ratio)
        {
            if (isRect == 1)
            {
                Rectangle rectRatio = SizeableArea.rectRatio(ratio, rect);
                DrawRect(g, rectRatio, ratio);
            }
            else
            {
                Quadrilateral quadRatio = SizeableArea.quadRatio(ratio, quad);
                DrawQuad(g, quadRatio, ratio);
            }
        }

        public void Draw(Graphics g)
        {
            if (isRect == 1)
            {
                DrawRect(g, rect, 1);
            }
            else
            {
                DrawQuad(g, quad, 1);
            }
        }

        private Rectangle GetRect(PosSizableQuad p, double ratio)
        {
            if (isRect == 1)
            {
                Rectangle rectRatio = SizeableArea.rectRatio(ratio, rect);

                switch (p)
                {
                    case PosSizableQuad.LeftUp:
                        return CreateRectSizableNode(rectRatio.X, rectRatio.Y);

                    case PosSizableQuad.LeftMiddle:
                        return CreateRectSizableNode(rectRatio.X, rectRatio.Y + rectRatio.Height / 2);

                    case PosSizableQuad.LeftDown:
                        return CreateRectSizableNode(rectRatio.X, rectRatio.Y + rectRatio.Height);

                    case PosSizableQuad.DownMiddle:
                        return CreateRectSizableNode(rectRatio.X + rectRatio.Width / 2, rectRatio.Y + rectRatio.Height);

                    case PosSizableQuad.RightDown:
                        return CreateRectSizableNode(rectRatio.X + rectRatio.Width, rectRatio.Y + rectRatio.Height);

                    case PosSizableQuad.RightMiddle:
                        return CreateRectSizableNode(rectRatio.X + rectRatio.Width, rectRatio.Y + rectRatio.Height / 2);

                    case PosSizableQuad.RightUp:
                        return CreateRectSizableNode(rectRatio.X + rectRatio.Width, rectRatio.Y);

                    case PosSizableQuad.UpMiddle:
                        return CreateRectSizableNode(rectRatio.X + rectRatio.Width / 2, rectRatio.Y);
                    default:
                        return new Rectangle();
                }
            }
            else
            {
                Quadrilateral quadRatio = SizeableArea.quadRatio(ratio, quad);

                switch (p)
                {
                    case PosSizableQuad.LeftUp:
                        return CreateRectSizableNode(quadRatio.LeftUp.X, quadRatio.LeftUp.Y);

                    case PosSizableQuad.LeftMiddle:
                        return CreateRectSizableNode((quadRatio.LeftUp.X + quadRatio.LeftDown.X) / 2, (quadRatio.LeftUp.Y + quadRatio.LeftDown.Y) / 2);

                    case PosSizableQuad.LeftDown:
                        return CreateRectSizableNode(quadRatio.LeftDown.X, quadRatio.LeftDown.Y);

                    case PosSizableQuad.DownMiddle:
                        return CreateRectSizableNode((quadRatio.LeftDown.X + quadRatio.RightDown.X) / 2, (quadRatio.LeftDown.Y + quadRatio.RightDown.Y) / 2);

                    case PosSizableQuad.RightDown:
                        return CreateRectSizableNode(quadRatio.RightDown.X, quadRatio.RightDown.Y);

                    case PosSizableQuad.RightMiddle:
                        return CreateRectSizableNode((quadRatio.RightDown.X + quadRatio.RightUp.X) / 2, (quadRatio.RightDown.Y + quadRatio.RightUp.Y) / 2);

                    case PosSizableQuad.RightUp:
                        return CreateRectSizableNode(quadRatio.RightUp.X, quadRatio.RightUp.Y);

                    case PosSizableQuad.UpMiddle:
                        return CreateRectSizableNode((quadRatio.RightUp.X + quadRatio.LeftUp.X) / 2, (quadRatio.RightUp.Y + quadRatio.LeftUp.Y) / 2);

                    default:
                        return new Rectangle();
                }
            }
        }

        private Rectangle CreateRectSizableNode(int x, int y)
        {
            return new Rectangle(x - sizeNodeRect / 2, y - sizeNodeRect / 2, sizeNodeRect, sizeNodeRect);
        }

        public PosSizableQuad GetNodeSelectable(Point p)
        {
            foreach (PosSizableQuad r in Enum.GetValues(typeof(PosSizableQuad)))
            {
                if (GetRect(r, 1).Contains(p))
                {
                    return r;
                }
            }

            return PosSizableQuad.None;
        }

        public Cursor GetCursor(PosSizableQuad p)
        {
            switch (p)
            {
                case PosSizableQuad.LeftUp:
                    return Cursors.SizeNWSE;

                case PosSizableQuad.LeftDown:
                    return Cursors.SizeNESW;

                case PosSizableQuad.RightUp:
                    return Cursors.SizeNESW;

                case PosSizableQuad.RightDown:
                    return Cursors.SizeNWSE;

                case PosSizableQuad.LeftMiddle:
                    return Cursors.SizeWE;

                case PosSizableQuad.DownMiddle:
                    return Cursors.SizeNS;

                case PosSizableQuad.RightMiddle:
                    return Cursors.SizeWE;

                case PosSizableQuad.UpMiddle:
                    return Cursors.SizeNS;
                default:

                    return Cursors.Default;
            }
        }
        /// <summary>
        /// 計算框選範圍的四邊形或不規則四邊形面積。
        /// </summary>
        public static double calQuadArea(SizeableArea inputSQ)
        {
            if (inputSQ == null)
            {
                return 0;
            }

            if (inputSQ.isRect == 1)
            {
                return inputSQ.rect.Width * inputSQ.rect.Height;
            }
            else
            {
                return Quadrilateral.Area(inputSQ.quad);
            }
        }
    }
}
