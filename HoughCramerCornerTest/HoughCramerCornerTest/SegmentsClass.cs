/*   线段集--->分组--->直线,当直线条数=2时,再计算角点
          适合于四个直角顶点的识别与定位
 
 
 版权所有:安吉八塔机器人有限公司
 版本:2018-12-16
 * --------  安吉八塔机器人有限公司  项道德
 *******************************************************
 //应用DEMO: 
 
 #region XDD识别各斜率与角点
            SegmentsClass sc = new SegmentsClass(myLL.ToArray());
            List<Segment> LS = sc.GroupLines(5, 3);
            //MessageBox.Show("LS.Count()=" + LS.Count);

            string s = ""; string s2 = "";
            for (int k = 0; k < LS.Count; k++)
            {
                double x1 = LS[k].a; double y1 = 0;
                double x2 = 0; double y2 = LS[k].b;

                if (Math.Abs(Math.PI / 2f - LS[k].az) > 0.0001)
                {
                    CvInvoke.Line(src, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), new MCvScalar(0, 0, 255), 1);
                }
                else
                {
                    CvInvoke.Line(src, new Point((int)x1, 0), new Point((int)x1, 1024), new MCvScalar(255, 0, 255), 1);
                }

                //s+="\r\n"+k +":az="+ LS[k].az+",ro="+ LS[k].ro;
                s += "\r\n" + string.Format("A={0:0.##}, B={1:0.##}, C={2:0.##}, ro={3:0.##}, az={4:0.##}, a={5:0.##}, b={6:0.##}",
                                    LS[k].A, LS[k].B, LS[k].C, LS[k].ro, LS[k].az, LS[k].a, LS[k].b);

                s2 += string.Format("  A{0}={1:0.##}", k, 57.3 * LS[k].az);

            }
            //输出矩形,长方形,与相关文本: 
            if (sc.Corner.X != -65536)
            {
                CvInvoke.Rectangle(src, new Rectangle((int)sc.Corner.X - 2, (int)sc.Corner.Y - 2, 4, 4), new MCvScalar(255, 255, 255), 1);
                CvInvoke.PutText(src, string.Format("x={0:0.##}  y={1:0.##}", sc.Corner.X, sc.Corner.Y), new Point((int)sc.Corner.X-60, (int)sc.Corner.Y + 20), Emgu.CV.CvEnum.FontFace.HersheyPlain, .75, new MCvScalar(0, 255, 255), 1);
                CvInvoke.PutText(src, s2, new Point((int)sc.Corner.X-80, (int)sc.Corner.Y + 40), Emgu.CV.CvEnum.FontFace.HersheyPlain, .75, new MCvScalar(0, 255, 255), 1);
            }

            pictureBox1.Image = src.ToBitmap();
            //MessageBox.Show("====-====\r\n" + s);

#endregion
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;


using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.CV.UI;


namespace HoughCramerCornerTest01  //引入到不同工程时,请修改这命名空间的名称
{
    /// <summary>
    /// 线段处理类
    /// </summary>
    class SegmentsClass
    {
        public PointF Corner = new PointF(-65536f, -65536f);//角点生成无效时,返回此值

        public int num = 0;//输入线段数目
        List<Segment> ss = new List<Segment>();//线段集合1
        List<Segment> mySegment = new List<Segment>();//线段集合2

        /// <summary>
        /// 线段处理类--构造函数
        /// </summary>
        /// <param name="lines">传入:线段数组</param>
        public SegmentsClass(LineSegment2D[] lines)
        {
            num = lines.Count();

            double dx;
            double dy;

            double az;
            double ro;

            double A;
            double B;
            double C;

            double a;
            double b;

            for (int i = 0; i < num; i++)
            {
                double x2 = lines[i].P2.X;
                double y2 = lines[i].P2.Y;
                double x1 = lines[i].P1.X;
                double y1 = lines[i].P1.Y;

                dx = x2 - x1;
                dy = y2 - y1;

                A = dy;
                B = -dx;
                C = dx * y1 - dy * x1;

                //double q = Math.Atan2(dy, dx);
                //az = q >= 0 ? q : (Math.PI + q);//归结到 0--PI 范围内
                az = dy / dx;
                ro = Math.Abs(C) / (Math.Sqrt(A * A + B * B));

                a = 65536;
                if (Math.Abs(dy) > 0.00001)
                {
                    a = -C / dy;
                }
                b = 65536;
                if (Math.Abs(dx) > 0.00001)
                {
                    b = C / dx;//-C/dx
                }

                Segment st = new Segment(dx, dy, az, ro, A, B, C, a, b);
                ss.Add(st);
            }

        }

        /// <summary>
        /// 对线段分组
        /// </summary>
        /// <param name="Distance2origin">分组参数1:按线段到原点的距离(如:5像素)</param>
        /// <param name="AngleTolerance">分组参数2:按线段的方位角(如:3度)</param>
        /// <returns>分组后的线段集,仅等于二条时,继续计算出角点</returns>
        public List<Segment> GroupLines(int Distance2origin, int AngleTolerance)
        {
            List<Segment> s0 = ss.OrderBy(o => o.az).OrderBy(o => o.ro).ToList();//按照方位角--极径排序

            List<Segment> LS = new List<Segment>();
            List<List<Segment>> LLS = new List<List<Segment>>();

            Segment n0 = s0[0];
            LS.Add(n0);

            for (int i = 1; i < s0.Count; i++)
            {
                Segment n1 = s0[i - 1];
                Segment n2 = s0[i];
                if (Math.Abs(n1.ro - n2.ro) < Distance2origin &&Math.Abs(n1.az - n2.az) < AngleTolerance / 57.3f)
                {
                    LS.Add(n2);
                }
                else
                {
                    LLS.Add(LS);
                    LS = new List<Segment>();
                    LS.Add(n2);
                }
            }
            LLS.Add(LS);



            for (int j = 0; j < LLS.Count; j++)
            {
                Segment gSegment = new Segment();
                gSegment.az = LLS[j].Average(o => o.az);
                gSegment.ro = LLS[j].Average(o => o.ro);
                gSegment.A = LLS[j].Average(o => o.A);
                gSegment.B = LLS[j].Average(o => o.B);
                gSegment.C = LLS[j].Average(o => o.C);
                gSegment.a = LLS[j].Average(o => o.a);
                gSegment.b = LLS[j].Average(o => o.b);
                mySegment.Add(gSegment);
            }

            if (mySegment.Count == 2)
            {
                getCorner();
            }

            return mySegment;
        }

        /// <summary>
        /// 用克莱姆法则解二元一次方程组,计算角点
        /// </summary>
        public void getCorner()
        {
            double A1 = mySegment[0].A;
            double B1 = mySegment[0].B;
            double C1 = mySegment[0].C;

            double A2 = mySegment[1].A;
            double B2 = mySegment[1].B;
            double C2 = mySegment[1].C;

            double D = A1 * B2 - A2 * B1;
            double Dx = B1 * C2 - B2 * C1;
            double Dy = A2 * C1 - A1 * C2;

            if (D != 0)
            {
                Corner.X = (float)(Dx / D);
                Corner.Y = (float)(Dy / D);
            }
        }

    }

    /// <summary>
    /// 自定义线段类
    /// </summary>
    class Segment
    {
        public double dx;
        public double dy;

        public double az;//方位角
        public double ro;//极径

        public double A;
        public double B;
        public double C;

        public double a;//直线1截距
        public double b;

        /// <summary>
        /// 自定义线段类--构造函数1
        /// </summary>
        public Segment()
        {
        }

        /// <summary>
        /// 自定义线段类--构造函数2
        /// </summary>
        /// <param name="_dx">两点间水平平移</param>
        /// <param name="_dy">两点间竖向平移</param>
        /// <param name="_az">线段方位角(0--PI)</param>
        /// <param name="_ro">原点到线段的距离</param>
        /// <param name="_A">直线一般式方程:(Ax+By+C=0)系数A</param>
        /// <param name="_B">直线一般式方程:(Ax+By+C=0)系数B</param>
        /// <param name="_C">直线一般式方程:(Ax+By+C=0)系数C</param>
        /// <param name="_a">直线在X轴的截距,返回65536时,为计算溢出</param>
        /// <param name="_b">直线在Y轴的截距,返回65536时,为计算溢出</param>
        public Segment(double _dx,double _dy,double _az,double _ro,double _A,double _B,double _C,double _a,double _b)
        {
            dx = _dx;dy = _dy;az = _az;ro = _ro;A = _A;B = _B;C = _C;a = _a;b = _b;
        }
    }
}
