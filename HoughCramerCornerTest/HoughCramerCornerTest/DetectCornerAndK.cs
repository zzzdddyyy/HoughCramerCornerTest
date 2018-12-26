using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace HoughCramerCornerTest
{
    /// <summary>
    /// 处理图像获取角点和斜率类
    /// </summary>
    public class DetectCornerAndK
    {
        #region 全局变量
        private LineSegment2D[] lines;
        CornerPointK cornerPointK = new CornerPointK();

        private const int width = 1280;      //相机分辨率
        private const int height = 1024;
        private Size imageSize = new Size(width, height);//图像的大小

        private Matrix<double> cameraMatrix = new Matrix<double>(3, 3);//相机内部参数
        private Matrix<double> distCoeffs = new Matrix<double>(5, 1);//畸变参数

        private Matrix<float> mapx = new Matrix<float>(height, width); //x坐标对应的映射矩阵
        private Matrix<float> mapy = new Matrix<float>(height, width);
        private MCvTermCriteria criteria = new MCvTermCriteria(100, 1e-5);//求角点迭代的终止条件（精度）

        double cannyThresholdLinking = 60.0;
        double cannyThreshold = 100.0;

        private Image<Bgr, byte> myImg;
        private Image<Gray, byte> grayImg;//灰度图
        private Image<Gray, byte> remapImg;
        private Image<Gray, byte> binaryImg;//二值化图
        private Image<Gray, byte> edageImg;//边缘图

        readonly Mat kernelClosing = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(3, 3));//运算核
        #endregion

        /// <summary>
        /// 获取直线集
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private LineSegment2D[] GetLinesByHough(Bitmap img)
        {
            //获取畸变矩阵
            GetCamParams();
            #region 灰度处理
            //灰度化
            grayImg = new Image<Gray, byte>(img).PyrDown().PyrUp();
            remapImg = grayImg.CopyBlank();//映射后图像
            //获取畸变参数
            GetCamParams();
            //畸变校正
            try
            {
                CvInvoke.InitUndistortRectifyMap(cameraMatrix, distCoeffs, null, cameraMatrix, imageSize, DepthType.Cv32F, mapx, mapy);
                CvInvoke.Remap(grayImg, remapImg, mapx, mapy, Inter.Linear, BorderType.Reflect101, new MCvScalar(0));
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            //二值化
            binaryImg = remapImg.CopyBlank();//创建一张和灰度图一样大小的画布
            CvInvoke.Threshold(remapImg, binaryImg, 250, 255, ThresholdType.Binary);
            //Closing
            Image<Gray, byte> closingImg = binaryImg.CopyBlank();//闭运算后图像
            CvInvoke.MorphologyEx(binaryImg, closingImg, MorphOp.Close, kernelClosing, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 0, 0, 255));
            #endregion
            #region 边缘检测
            edageImg = closingImg.CopyBlank();
            CvInvoke.Canny(closingImg, edageImg, cannyThreshold, cannyThresholdLinking);
            edageImg.SmoothMedian(9);
            #endregion
            #region HoughLinesP
            //HoughLineP
            lines = CvInvoke.HoughLinesP(
                edageImg,
                1, //Distance resolution in pixel-related units
                Math.PI / 180.0, //Angle resolution measured in radians.
                80, //threshold
                50, //min Line width
                30); //gap between lines);
            #endregion
            return lines;
        }

        /// <summary>
        /// 获取相机矩阵和畸变矩阵
        /// </summary>
        private void GetCamParams()
        {
            //填充相机矩阵
            cameraMatrix[0, 0] = 1240.8572220055391;
            cameraMatrix[0, 1] = 0;
            cameraMatrix[0, 2] = 646.38527349489539;
            cameraMatrix[1, 0] = 0;
            cameraMatrix[1, 1] = 1239.2998782377838;
            cameraMatrix[1, 2] = 486.14441543170562;
            cameraMatrix[2, 0] = 0;
            cameraMatrix[2, 1] = 0;
            cameraMatrix[2, 2] = 1;
            //填充畸变矩阵
            distCoeffs[0, 0] = -0.18403250333760257;
            distCoeffs[1, 0] = 0.17726308285500975;
            distCoeffs[2, 0] = -0.00029903650191503278;
            distCoeffs[3, 0] = -0.0017477041393902419;
            distCoeffs[4, 0] = 0;
        }

        /// <summary>
        /// 获取直线角点和斜率
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public CornerPointK GetCornerAndK(Bitmap img,double wight=0,double hight=0)
        {
            GetLinesByHough(img);
            SegmentsClass sc = new SegmentsClass(lines);
            
            List<Segment> LS = sc.GroupLines(50, 3);

            
            if (LS.Count==2)
            {
                cornerPointK.Corner = sc.Corner;
                cornerPointK.LineK1 = LS[0].az;
                cornerPointK.LineK2 = LS[1].az;
            }
            else
            {
                cornerPointK.Corner = new PointF(-1,-1);
                cornerPointK.LineK1 = Double.NaN;
                cornerPointK.LineK2 = Double.NaN;
            }

            //=======以下算法为小海绵使用=======
            double minK = Math.Abs(cornerPointK.LineK1) < Math.Abs(cornerPointK.LineK2)
                ? cornerPointK.LineK1
                : cornerPointK.LineK2;
            double theta = Math.Atan(minK) * 57.3;
            cornerPointK.Center =new PointF((float)(cornerPointK.Corner.X + 0.5 * wight * Math.Cos(theta) - 0.5 * hight * Math.Sin(theta)),
                (float)(cornerPointK.Corner.Y + 0.5 * wight * Math.Sin(theta) +0.5 * hight * Math.Cos(theta)));
            return cornerPointK;
        }
    }

    /// <summary>
    /// 记录角点坐标和直线斜率类
    /// </summary>
    public class CornerPointK
    {
        public PointF Center { get; set; }
        public PointF Corner { get; set; }
        public double LineK1 { get; set; }
        public double LineK2 { get; set; }
    }

    /// <summary>
    /// 线段处理类
    /// CopyRight:XDD
    /// </summary>
    class SegmentsClass
    {
        public PointF Corner = new PointF(-1f, -1f);//角点生成无效时,返回此值

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
                if (Math.Abs(n1.ro - n2.ro) < Distance2origin &&Math.Abs(Math.Atan(n1.az)- Math.Atan(n2.az))*57.3 < AngleTolerance )
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
        public Segment(double _dx, double _dy, double _az, double _ro, double _A, double _B, double _C, double _a, double _b)
        {
            dx = _dx; dy = _dy; az = _az; ro = _ro; A = _A; B = _B; C = _C; a = _a; b = _b;
        }
    }
}
