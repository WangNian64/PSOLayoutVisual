using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace LayoutVisual
{
    struct Size
    {
        public double x;
        public double y;
        public Size(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
    enum PointDirect
    { 
        Vertical = 1,
        Horizontal = 2
    }
    struct LinkPoint
    {
        public int device1Index;
        public int device2Index;
        //public PointDirect device1PD;
        //public PointDirect device2PD;
        public List<Size> points;//路径的所有点
        public LinkPoint(int device1Index, int device2Index, List<Size> points)
        {
            this.device1Index = device1Index;
            //this.device1PD = device1PD;
            this.device2Index = device2Index;
            //this.device2PD = device2PD;
            this.points = points;
        }
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 800;
            this.Height = 800;
        }

        //绘制布局结果
        private void Button1_Click(object sender, EventArgs e)
        {
            #region 读取参数
            List<Size> deviceSizeList = new List<Size>();//设备尺寸数组
            List<List<int>> cargoDeviceList = new List<List<int>>();//物料经过的设备列表
            List<Size> devicePosList = new List<Size>();//设备坐标数组
            List<LinkPoint> linkPointList = new List<LinkPoint>();//设备点连线数组
            string line = "";
            int screenLength, screenWidth;
            screenLength = screenWidth = 800;
            
            //读取车间尺寸
            StreamReader file1 = new StreamReader("../../../../InputPara.txt");
            file1.ReadLine();
            line = file1.ReadLine();
            string[] shopSize = line.Split(',');
            int shopLength = Convert.ToInt32(shopSize[0]);
            int shopWidth = Convert.ToInt32(shopSize[1]);
            //读取仓库入口位置
            file1.ReadLine();
            line = file1.ReadLine();
            string[] enterStr = line.Split(',');
            double enterAxisX = Convert.ToDouble(enterStr[0]);
            double enterAxisY = Convert.ToDouble(enterStr[1]);
            //读取仓库出口位置
            file1.ReadLine();
            line = file1.ReadLine();
            string[] exitStr = line.Split(',');
            double exitAxisX = Convert.ToDouble(exitStr[0]);
            double exitAxisY = Convert.ToDouble(exitStr[1]);


            //读取设备坐标
            int deviceNum = 6;
            int roundSum = 2;
            StreamReader file3 = new StreamReader("../../../../FinalResult.txt");
            for (int i = 0; i < deviceNum; i++)
            {
                line = file3.ReadLine();
                string[] axisStr = line.Split(',');
                double axisX = Math.Round(Convert.ToDouble(axisStr[0]), roundSum);
                double axisY = Math.Round(Convert.ToDouble(axisStr[1]), roundSum);
                devicePosList.Add(new LayoutVisual.Size(axisX, axisY));
            }
            //读取设备尺寸
            for (int i = 0; i < deviceNum; i++)
            {

                line = file3.ReadLine();
                string[] sizeStr = line.Split(',');
                double sizeX = Math.Round(Convert.ToDouble(sizeStr[0]), roundSum);
                double sizeY = Math.Round(Convert.ToDouble(sizeStr[1]), roundSum);
                deviceSizeList.Add(new LayoutVisual.Size(sizeX, sizeY));
            }
            //读取出入口的连线关系
            while ((line = file3.ReadLine()) != null)
            {
                string[] axisStr = line.Split(' ');
                int device1Index = Convert.ToInt32(axisStr[0]);
                int device2Index = Convert.ToInt32(axisStr[1]);
                string[] pointsStr = axisStr[2].Split('|');
                //PointDirect PD1 = (PointDirect)Convert.ToInt32(p1Str[0]);
                //PointDirect PD2 = (PointDirect)Convert.ToInt32(p2Str[0]);
                List<Size> points = new List<Size>();
                for (int i = 0; i < pointsStr.Length; i++)
                {
                    string[] pointStr = pointsStr[i].Split(',');
                    points.Add(new LayoutVisual.Size(Math.Round(Convert.ToDouble(pointStr[0]), roundSum), 
                        Math.Round(Convert.ToDouble(pointStr[1]), roundSum)));
                }
                linkPointList.Add(new LinkPoint(device1Index, device2Index, points));
            }
            #endregion

            #region 绘制布局图
            Graphics GPS = this.CreateGraphics();
            GPS.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            GPS.Clear(Color.White);

            #region 绘制适应度散点图
            //StreamReader file2 = new StreamReader("../../../../IterateResult.txt");
            //int iterAxisX = 0;
            //int iterAxisY = 0;
            //int iterGraphHeight = 600;

            //double lastIterNum = 0.0;
            //int iterIndex = 0;
            //while ((line = file2.ReadLine()) != null && !line.Equals("inf"))
            //{
            //    if (iterIndex % 10 == 0)
            //    {
            //        double tempIterAxisY = Convert.ToDouble(line) * 10000;
            //        iterAxisY = Convert.ToInt32(iterGraphHeight - tempIterAxisY);
            //        //if (lastIterNum != Convert.ToDouble(line))
            //        //{
            //        // 画点
            //        GPS.FillEllipse(Brushes.Black, iterAxisX++, iterAxisY, 2, 2);
            //        //}
            //        lastIterNum = Convert.ToDouble(line);
            //    }
            //    iterIndex++;
            //}

            #endregion

            Pen MyPen = new Pen(Color.Red, 2f);
            int offset_X = 80;
            int offset_Y = 80;
            
            int enlargeNum = 30; //放大倍数

            Pen outLinePen1 = new Pen(Color.Black, 1f);
            Pen outLinePen2 = new Pen(Color.Blue, 1f);
            //绘制设备&设备边缘的线
            for (int i = 0; i < devicePosList.Count; i++)
            {
                int rectAxisX = GetRectAxis(devicePosList[i].x - 0.5 * deviceSizeList[i].x, enlargeNum);
                int rectAxisY = GetRectAxis(devicePosList[i].y - 0.5 * deviceSizeList[i].y, enlargeNum);
                int rectSizeX = GetRectAxis(deviceSizeList[i].x, enlargeNum);
                int rectSizeY = GetRectAxis(deviceSizeList[i].y, enlargeNum);

                Rectangle Rect = new Rectangle(rectAxisX + offset_X, rectAxisY + offset_Y, rectSizeX, rectSizeY);
                int deviceCenterX = GetRectAxis(devicePosList[i].x, enlargeNum) + offset_X - 3;
                int deviceCenterY = GetRectAxis(devicePosList[i].y, enlargeNum) + offset_Y - 3;
                GPS.FillEllipse(Brushes.Red, deviceCenterX, deviceCenterY, 6, 6);//设备中心点
                GPS.DrawRectangle(MyPen, Rect);//设备外框
                #region 绘制设备外边缘的线
                //double offsetOut = 0;
                //int lineLeftX = GetRectAxis(devicePosList[i].x - 0.5 * deviceSizeList[i].x - offsetOut, enlargeNum);
                //int lineUpY = GetRectAxis(devicePosList[i].y - 0.5 * deviceSizeList[i].y - offsetOut, enlargeNum);
                //int lineRightX = GetRectAxis(devicePosList[i].x + 0.5 * deviceSizeList[i].x + offsetOut, enlargeNum);
                //int lineDownY = GetRectAxis(devicePosList[i].y + 0.5 * deviceSizeList[i].y + offsetOut, enlargeNum);
                //GPS.DrawLine(outLinePen1, 0, lineUpY + offset_Y, screenLength, lineUpY + offset_Y);
                //GPS.DrawLine(outLinePen1, 0, lineDownY + offset_Y, screenLength, lineDownY + offset_Y);
                //GPS.DrawLine(outLinePen1, lineLeftX + offset_X, 0, lineLeftX + offset_X, screenWidth);
                //GPS.DrawLine(outLinePen1, lineRightX + offset_X, 0, lineRightX + offset_X, screenWidth);


                #endregion 

                Font myFont = new Font("宋体", 12, FontStyle.Bold);
                GPS.DrawString((i + 1).ToString(), myFont, Brushes.Black, deviceCenterX - 3, deviceCenterY + 3);//设备编号
            }

            //绘制入口&出口
            int enterRectAxisX = Convert.ToInt32(enterAxisX * enlargeNum);
            int enterRectAxisY = Convert.ToInt32(enterAxisY * enlargeNum);
            int enterPointSize = 10;
            GPS.FillEllipse(Brushes.Red, enterRectAxisX + offset_X - (int)(0.5 * enterPointSize), enterRectAxisY + offset_Y - (int)(0.5 * enterPointSize), enterPointSize, enterPointSize);

            int exitRectAxisX = Convert.ToInt32(exitAxisX * enlargeNum);
            int exitRectAxisY = Convert.ToInt32(exitAxisY * enlargeNum);
            int exitPointSize = 10;
            GPS.FillEllipse(Brushes.Blue, exitRectAxisX + offset_X - (int)(0.5 * exitPointSize), exitRectAxisY + offset_Y - (int)(0.5 * exitPointSize), exitPointSize, exitPointSize);

            //绘制出入口连线
            Point a, b;
            int startX, startY, endX, endY;

            Pen linePen = new Pen(GetRandomColor(), 2);
            for (int i = 0; i < linkPointList.Count; i++)
            {
                linePen = new Pen(GetRandomColor(), 2);
                for (int j = linkPointList[i].points.Count - 1; j > 0; j--)
                {
                    startX = GetRectAxis(linkPointList[i].points[j].x, enlargeNum);
                    startY = GetRectAxis(linkPointList[i].points[j].y, enlargeNum);
                    endX = GetRectAxis(linkPointList[i].points[j - 1].x, enlargeNum);
                    endY = GetRectAxis(linkPointList[i].points[j - 1].y, enlargeNum);
                    a = new Point(startX + offset_X, startY + offset_Y);
                    b = new Point(endX + offset_X, endY + offset_Y);
                    if (j == 1)
                    {
                        System.Drawing.Drawing2D.AdjustableArrowCap _LineCap = new System.Drawing.Drawing2D.AdjustableArrowCap(9, 9, false);   //设置一个线头
                        linePen.CustomEndCap = (System.Drawing.Drawing2D.CustomLineCap)_LineCap;
                    }
                    else
                    {
                        linePen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    }
                    GPS.DrawLine(linePen, a, b);

                }

                #region 绘制出入口点的水平垂直线
                //if (linkPointList[i].device1PD == PointDirect.Horizontal)
                //{
                //    a = new Point(0, startY + offset_Y + device1OffsetY);
                //    b = new Point(screenLength, startY + offset_Y + device1OffsetY);

                //    GPS.DrawLine(outLinePen2, a, b);
                //}
                //else
                //{
                //    a = new Point(startX + offset_X + device1OffsetX, 0);
                //    b = new Point(startX + offset_X + device1OffsetX, screenWidth);
                //    GPS.DrawLine(outLinePen2, a, b);
                //}

                //if (linkPointList[i].device2PD == PointDirect.Horizontal)
                //{
                //    a = new Point(0, endY + offset_Y + device2OffsetY);
                //    b = new Point(screenLength, endY + offset_Y + device2OffsetY);

                //    GPS.DrawLine(outLinePen2, a, b);
                //}
                //else
                //{
                //    a = new Point(endX + offset_X + device2OffsetX, 0);
                //    b = new Point(endX + offset_X + device2OffsetX, screenWidth);
                //    GPS.DrawLine(outLinePen2, a, b);
                //}
                #endregion

            }
            //for (int i = 0; i < cargoDeviceList.Count; i++)
            //{
            //    Pen linePen = new Pen(GetRandomColor(), 1);
            //    //入口也要考虑
            //    a = new Point(enterRectAxisX + offset_X, enterRectAxisY + offset_Y);
            //    b = new Point(GetRectAxis(devicePosList[cargoDeviceList[i][0] - 1].x, enlargeNum) + offset_X,
            //        GetRectAxis(devicePosList[cargoDeviceList[i][0] - 1].y, enlargeNum) + offset_Y);
            //    GPS.DrawLine(linePen, a, b);
            //    for (int j = 0; j < cargoDeviceList[i].Count - 1; j++)
            //    {
            //        Size deviceA = devicePosList[cargoDeviceList[i][j] - 1];
            //        int x = cargoDeviceList[i][j] - 1;
            //        Size deviceB = devicePosList[cargoDeviceList[i][j + 1] - 1];
            //        int y = cargoDeviceList[i][j + 1] - 1;
            //        int startX = GetRectAxis(deviceA.x, enlargeNum);
            //        int startY = GetRectAxis(deviceA.y, enlargeNum);
            //        int endX = GetRectAxis(deviceB.x, enlargeNum);
            //        int endY = GetRectAxis(deviceB.y, enlargeNum);
            //        a = new Point(startX + offset_X, startY + offset_Y);
            //        b = new Point(endX + offset_X, endY + offset_Y);
            //        GPS.DrawLine(linePen, a, b);
            //    }
            //}


            //绘制外部轮廓
            int enlargeNum1 = enlargeNum;
            Pen MyPen1 = new Pen(Color.Gray, 2f);
            int sizeWorkshop_x = shopLength * enlargeNum1;
            int sizeWorkshop_y = shopWidth * enlargeNum1;
            Rectangle Rect1 = new Rectangle(offset_X, offset_Y, sizeWorkshop_x, sizeWorkshop_y);
            GPS.DrawRectangle(MyPen1, Rect1);

            #endregion

            file1.Close();
            file3.Close();
        }

        //输入坐标，得到绘制的实际坐标
        private int GetRectAxis(double axis, int enlargeNum)
        {
            return Convert.ToInt32(axis * enlargeNum);
        }

        //返回一个随机颜色
        private Color GetRandomColor()
        {
            int iSeed = 10;
            Random ro = new Random(10);
            long tick = DateTime.Now.Ticks;
            Random ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));

            int R = ran.Next(255);
            int G = ran.Next(255);
            int B = ran.Next(255);
            B = (R + G > 400) ? R + G - 400 : B;//0 : 380 - R - G;
            B = (B > 255) ? 255 : B;
            return Color.FromArgb(R, G, B);
        }
    }
}
