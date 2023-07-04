using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using NPOI.SS.Formula.Functions;

namespace TrunkAD.Core.GameSystem.GameHelper 
{
    public class PointHelper
    {
        public  static  PointHelper  Instance { get; set; }
        public void Awake()
         {
             Instance = this;
         }
         /// <summary>
         /// 计算角度
         /// </summary>
         /// <param name="cenPoint"></param>
         /// <param name="fristPoint"></param>
         /// <param name="secondPoint"></param>
         /// <returns></returns>
         public double CalculateAngle(Point cenPoint, Point fristPoint, Point secondPoint)
         {
             const double M_PI = 3.1415926535897;
             double ma_x = fristPoint.X - cenPoint.X;
             double ma_y = fristPoint.Y - cenPoint.Y;
             double mb_x = secondPoint.X - cenPoint.X;
             double mb_y = secondPoint.Y - cenPoint.Y;
             double v1 = (ma_x * mb_x) + (ma_y * mb_y);
             double ma_val = Math.Sqrt(ma_x * ma_x + ma_y * ma_y);
             double mb_val = Math.Sqrt(mb_x * mb_x + mb_y * mb_y);
             double ms = v1 / (ma_val * mb_val);
             double angle = Math.Acos(ms) * 180 / M_PI;
             return angle;
         }
         /// <summary>
         /// 根据长度，求俩点的坐标
         /// </summary>
         /// <param name="len"></param>
         /// <param name="fristPoint"></param>
         /// <param name="secondPoint"></param>
         /// <returns></returns>
         public System.Drawing.Point LengthToXY(double len, Point fristPoint, Point secondPoint)
         {
             Point po = new Point();
             double dy = secondPoint.Y - fristPoint.Y;
             double dx = secondPoint.X - fristPoint.X;
             double lens = len;
             double x1 = Math.Sqrt(lens * lens / (dy * dy / (dx * dx) + 1));
             double y1 = dy * x1 / dx;
             po.X = fristPoint.X + (int)(x1 + 0.5);
             po.Y = fristPoint.Y + (int)(y1 + 0.5);
             return po;
         }
         /// <summary>
         /// 根据长度，求俩点的坐标
         /// </summary>
         /// <param name="len"></param>
         /// <param name="fristPoint"></param>
         /// <param name="secondPoint"></param>
         /// <returns></returns>
         public System.Drawing.Point LengthToYX(double len, Point fristPoint, Point secondPoint)
         {
             Point po = new Point();
             double dy = secondPoint.Y - fristPoint.Y;
             double dx = secondPoint.X - fristPoint.X;
             double lens = len;
             double y1 = lens/ Math.Sqrt((dx * dx) / (dy * dy) + 1);
             double x1 = (dx * y1) / dy;
             po.X = fristPoint.X + (int)(x1 + 0.5);
             po.Y = fristPoint.Y + (int)(y1 + 0.5);
             return po;
         }
         /// <summary>
         /// 计算俩点的长度
         /// </summary>
         /// <param name="fristPoint"></param>
         /// <param name="secondPoint"></param>
         /// <returns></returns>
         public double CalculateLength(Point fristPoint, Point secondPoint)
         {
             double len = Math.Sqrt((fristPoint.X - secondPoint.X) * (fristPoint.X - secondPoint.X) + (fristPoint.Y - secondPoint.Y) * (fristPoint.Y - secondPoint.Y));
             return len;
         }
         /// <summary>
         /// 判断边界
         /// </summary>
         /// <param name="P1"></param>
         /// <param name="P2"></param>
         /// <param name="point"></param>
         /// <returns></returns>
         public int JudgeSide(Point P1, Point P2, Point point)
         {
             return ((P2.Y - P1.Y) * point.X + (P1.X - P2.X) * point.Y + (P2.X * P1.Y - P1.X * P2.Y));
         }
         /// <summary>
        /// 
        /// </summary>
        /// <param name="mousePoint"></param>
        /// <param name="topStartP"></param>
        /// <param name="topEndP"></param>
        /// <param name="bottomStartP"></param>
        /// <param name="bottomEndP"></param>
        /// <param name="outTopP"></param>
        /// <param name="outBottomP"></param>
        /// <param name="baseLen"></param>
        /// <param name="fullLenPix"></param>
        /// <param name="outLen"></param>
        public void DrawMousePointLine(Point mousePoint, Point topStartP, Point topEndP, Point bottomStartP, Point bottomEndP, ref Point outTopP, ref Point outBottomP, double baseLen, double fullLenPix, ref double outLen)
         {
              //计算上标点和下标点到鼠标点的角度,180度成直线,
            //上下长度不同应该走不同的step长度
            //超过区域边界长度就break
            //或者上下两点的水平x值都超过鼠标点的xbreak
            double lenq = 0;
            double nowLen = 0;
            double div1 = 0.0f;
            System.Drawing.Point AngleMaxtopP = new System.Drawing.Point();
            System.Drawing.Point AngleMaxbotP = new System.Drawing.Point();
            //顶部长度
            double topLength1a = CalculateLength(topStartP, topEndP);
            //底部长度
            double bottomLength1a =  CalculateLength(bottomStartP, bottomEndP);
            //实际长度和像素长度比值
            fullLenPix /= bottomLength1a;
            double AngleMin = 0;//最小角度
            double AngleMax = 180;
            double divStep = 0.00001f;//粗算
            double angleP12MTemp = 0;
            while (!((AngleMaxtopP.X >= mousePoint.X) && (AngleMaxbotP.X >= mousePoint.X)) && div1 <= 1)
            {

                try
                {
                    double angleP12M = 0;
                    int angleDecrementCount = 0;
                    div1 = div1 + divStep;
                    //上下标定线不停增加0.1f，直到角度>=0度
                    double topLen = topLength1a * div1;
                    double bottomLen = bottomLength1a * div1;
                    //求出顶部x，y值
                    System.Drawing.Point topP = LengthToXY(topLen, topStartP, topEndP);
                    //求出底部xy
                    System.Drawing.Point botP = LengthToXY(bottomLen, bottomStartP, bottomEndP);
                    angleP12M = CalculateAngle(mousePoint, topP, botP);

                    if (angleP12M < 120)
                    {
                        divStep = 0.05f; //精算
                    }
                    else if (angleP12M < 160)
                    {
                        divStep = 0.005f; //精算
                    }
                    else if (angleP12M < 170)
                    {
                        divStep = 0.001f; //精算
                    }
                    else if (angleP12M < 180)
                    {
                        divStep = 0.0001f; //精算
                    }
                    if (angleP12MTemp > angleP12M)
                    {
                        angleDecrementCount++;
                        if (angleDecrementCount > 10)
                            break;
                    }
                    else
                        angleDecrementCount = 0;
                    angleP12MTemp = angleP12M;
                    if (angleP12M <= AngleMax)
                    {
                        if (angleP12M > AngleMin)
                        {
                            AngleMin = angleP12M;
                            AngleMaxtopP = topP;
                            AngleMaxbotP = botP;
                        }
                    }
                    if (179 <= AngleMin && AngleMin <= 180)
                    {
                        outTopP = AngleMaxtopP;
                        outBottomP = AngleMaxbotP;
                        //计算距离
                        lenq = CalculateLength(bottomStartP, outBottomP);
                        nowLen = lenq * fullLenPix;
                        nowLen += (baseLen * 10);
                        outLen = nowLen; //测量长度
                        return;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Debug(ex);
                    return;
                }
            }

         }
        /// <summary>
        /// 计算垂直距离
        /// </summary>
        /// <param name="gPnts"></param>
        /// <param name="mousePoint"></param>
        /// <param name="outTopP"></param>
        /// <param name="outBottomP"></param>
        /// <param name="outLen"></param>
        /// <param name="gfencePntsDisValue"></param>
        public void CalculateVerticalDistance(List<Point[]> gPnts, Point mousePoint, ref Point outTopP, ref Point outBottomP, ref double outLen,  List<int[]> gfencePntsDisValue )
        {
             int nLen = gPnts.Count;
             int lens = gfencePntsDisValue.Count;
            if (nLen>0&&lens>0)
            { 
                System.Drawing.Point topStartP = gPnts[nLen - 1][1];
                System.Drawing.Point topEndP = gPnts[nLen - 1][3];
                System.Drawing.Point bottomStartP = gPnts[0][0];
                System.Drawing.Point bottomEndP = gPnts[0][2];
                //两点距离
                double fullLenPix = (gfencePntsDisValue[0][1] - gfencePntsDisValue[0][0]) * 10;
                //最小距离
                double baseLen = 0;
                //计算上标点和下标点到鼠标点的角度,180度成直线,
                //上下长度不同应该走不同的step长度
                //超过区域边界长度就break
                //或者上下两点的水平x值都超过鼠标点的xbreak
                double lenq = 0;
                double nowLen = 0;
                double div1 = 0.0f;

                System.Drawing.Point AngleMaxtopP = new System.Drawing.Point();
                System.Drawing.Point AngleMaxbotP = new System.Drawing.Point();
                //顶部长度
                double topLength1a =CalculateLength(topStartP, topEndP);
                //底部长度
                double bottomLength1a = CalculateLength(bottomStartP, bottomEndP);
                //实际长度和像素长度比值
                fullLenPix /= bottomLength1a;
                double AngleMin = 0;//最小角度
                double AngleMax = 180;
                double divStep = 0.00001f;//粗算
                double angleP12MTemp = 0;
                    while (!((AngleMaxtopP.Y >= mousePoint.Y) && (AngleMaxbotP.Y >= mousePoint.Y)) && div1 <= 1)
                    {
                        try
                        {
                            double angleP12M = 0;
                            int angleDecrementCount = 0;
                            div1 = div1 + divStep;
                            //上下标定线不停增加0.1f，直到角度>=0度
                            double topLen = topLength1a * div1;
                            double bottomLen = bottomLength1a * div1;
                            //求出顶部x，y值
                            System.Drawing.Point topP = LengthToYX(topLen, topStartP, topEndP);
                            //求出底部xy
                            System.Drawing.Point botP = LengthToYX(bottomLen, bottomStartP, bottomEndP);
                            angleP12M = CalculateAngle(mousePoint, topP, botP);
                            if (angleP12M < 120)
                            {
                                divStep = 0.05f; //精算
                            }
                            else if (angleP12M < 160)
                            {
                                divStep = 0.005f; //精算
                            }
                            else if (angleP12M < 170)
                            {
                                divStep = 0.001f; //精算
                            }
                            else if (angleP12M < 180)
                            {
                                divStep = 0.0001f; //精算
                            }

                            if (angleP12MTemp > angleP12M)
                            {
                                angleDecrementCount++;
                                if (angleDecrementCount > 10)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                angleDecrementCount = 0;
                            }
                            angleP12MTemp = angleP12M;
                            if (angleP12M <= AngleMax)
                            {
                                if (angleP12M > AngleMin)
                                {
                                    AngleMin = angleP12M;
                                    AngleMaxtopP = topP;
                                    AngleMaxbotP = botP;
                                }
                            }
                            if (179 <= AngleMin && AngleMin <= 180)
                            {
                                outTopP = AngleMaxtopP;
                                outBottomP = AngleMaxbotP;
                                //计算距离
                                lenq = CalculateLength(bottomStartP, outBottomP);
                                nowLen = lenq * fullLenPix;
                                nowLen += (baseLen * 10);
                                outLen = nowLen; //测量长度
                                                 //pictureBox2.Refresh();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Debug(ex);
                            return;
                        }
                    }

            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// 画标点十字
        /// </summary>
        /// <param name="g"></param>
        /// <param name="point"></param>
        /// <param name="pen"></param>
        public void DrawPointCross(Graphics g, Point point, Pen pen)
        {
            g.DrawLine(pen,point.X-15,point.Y,point.X+15,point.Y);
            g.DrawLine(pen,point.X,point.Y-15,point.X,point.Y+15);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="txt"></param>
        /// <param name="point"></param>
        /// <param name="drawFont"></param>
        /// <param name="drawBrush"></param>
        /// <param name="directionx"></param>
        /// <param name="distancex"></param>
        /// <param name="directiony"></param>
        /// <param name="distancey"></param>
        public void DrawPointText(Graphics g, string txt, System.Drawing.Point point, Font drawFont, SolidBrush drawBrush, int directionx, int distancex, int directiony, int distancey)
        {
            //directiony 0 1 上下
            //directionx 0 1 左右
            int x = point.X;
            int y = point.Y;
            switch (directionx)
            {
                case 0:
                    x -= distancex;
                    break;
                case 1:
                    x += distancex;
                    break;
                default:
                    break;
            }
            switch (directiony)
            {
                case 0:
                    y -= distancey;
                    break;
                case 1:
                    y += distancey;
                    break;
                default:
                    break;
            }

            g.DrawString(txt, drawFont, drawBrush, x, y);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public Point XYString2Point(string str)
        {
            string[] strg = str.Split(',');
            if (null == strg)
            {
                return new System.Drawing.Point(0, 0);
            }
            if (strg.Length == 1)
            {
                return new System.Drawing.Point(PointHelper.Instance.Str2int(strg[0]), 0);
            }
            System.Drawing.Point p = new System.Drawing.Point(PointHelper.Instance.Str2int(strg[0]), PointHelper.Instance.Str2int(strg[1]));
            return p;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public int  Str2int(string str)
        {
            int i = 0;
            if (null == str)
                return 0;
            int.TryParse(str, out i);
            return i;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ps"></param>
        /// <param name="pz"></param>
        /// <returns></returns>
        public Point CalculateMiddlePoint(Point ps, Point pz)
        {
            Point point = new Point((ps.X + pz.X) / 2, (ps.Y + pz.Y) / 2);
            return point;
        }
        /// <summary>
        /// 是否在四边形内
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool InQuadrangle(OpenCvSharp .Point a, OpenCvSharp.Point b, OpenCvSharp.Point c, OpenCvSharp.Point d, OpenCvSharp.Point p)
        {
            double dTriangle = TriangleArea(a, b, p) + TriangleArea(b, c, p)
                     + TriangleArea(c, d, p) + TriangleArea(d, a, p);
            double dQuadrangle = TriangleArea(a, b, c) + TriangleArea(c, d, a);
            return dTriangle == dQuadrangle;
        }
        /// <summary>
        /// 返回三角形的面积
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private double TriangleArea(OpenCvSharp.Point a, OpenCvSharp.Point b, OpenCvSharp.Point c)
        {
            double result = Math.Abs((a.X * b.Y + b.X * c.Y + c.X * a.Y - b.X * a.Y- c.X * b.Y - a.X * c.Y) / 2.0D);
            return result;
        }
    }
}