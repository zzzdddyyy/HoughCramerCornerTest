using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.Util;

namespace ImageProcessing
{
    /// <summary>
    /// 处理图像获取角点和斜率类
    /// </summary>
    public class DetectCenterAndSlop
    {
        //字段
        public PointF Center { get; set; }
        public List<PointF> Corner { get; set; }
        public double LineK1 { get; set; }
        public double LineK2 { get; set; }
        public double RotatedAngle { get; set; }

        #region 全局变量
        private LineSegment2D[] lines;

        public const int width = 5472;      //相机分辨率 2000W像素
        public const int height = 3648;
        private Size imageSize = new Size(width, height);//图像的大小

        private Matrix<double> cameraMatrix = new Matrix<double>(3, 3);//相机内部参数
        private Matrix<double> distCoeffs = new Matrix<double>(5, 1);//畸变参数

        private Matrix<float> mapx = new Matrix<float>(height, width); //x坐标对应的映射矩阵
        private Matrix<float> mapy = new Matrix<float>(height, width);
        private MCvTermCriteria criteria = new MCvTermCriteria(100, 1e-5);//求角点迭代的终止条件（精度）

        private Matrix<double> frontCameraTrans = new Matrix<double>(3, 3);
        private Matrix<double> rightCameraTrans = new Matrix<double>(3, 3);


        double cannyThresholdLinking = 60.0;
        double cannyThreshold = 100.0;

        private Image<Bgr, byte> myImg;
        private Image<Gray, byte> grayImg;//灰度图
        private Image<Gray, byte> remapImg;
        private Image<Gray, byte> binaryImg;//二值化图
        private Image<Gray, byte> edageImg;//边缘图

        Rectangle ROI = new Rectangle(new Point(900, 0), new Size(3400, 3648));


        readonly Mat kernelClosing = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));//运算核
        #endregion

        /// <summary>
        ///  获取直线集
        /// </summary>
        /// <param name="img"></param>
        /// <param name="cameraID">0=右侧，1=前方</param>
        /// <returns>Hough直线集</returns>
        private LineSegment2D[] GetLinesByHough(Bitmap img, int cameraID)
        {
            #region 灰度处理
            //灰度化
            grayImg = new Image<Gray, byte>(img).PyrDown().PyrUp();
            remapImg = grayImg.CopyBlank();//映射后图像
            //获取畸变参数
            if (cameraID == 0)
            {
                GetRightCamParams();
            }
            else
            {
                GetFrontCamParams();
            }

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

            //Image<Gray, byte> roiBinary = GetROI(grayImg, ROI);//控制是否需要畸变校正

            //二值化
            binaryImg = grayImg.CopyBlank();//创建一张和灰度图一样大小的画布
            CvInvoke.Threshold(remapImg, binaryImg, 240, 255, ThresholdType.Binary);//控制是否需要畸变校正
            //Closing
            Image<Gray, byte> closingImg = binaryImg.CopyBlank();//闭运算后图像
            CvInvoke.MorphologyEx(binaryImg, closingImg, MorphOp.Open, kernelClosing, new Point(-1, -1), 5, BorderType.Default, new MCvScalar(255, 0, 0, 255));
            #endregion

            #region 去除白色不相干区域块
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();//区块集合
            Image<Gray, byte> dnc = new Image<Gray, byte>(closingImg.Width, closingImg.Height);
            CvInvoke.FindContours(closingImg, contours, dnc, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);//轮廓集合
            for (int k = 0; k < contours.Size; k++)
            {
                double area = CvInvoke.ContourArea(contours[k]);//获取各连通域的面积 
                if (area < 2500000)//根据面积作筛选(指定最小面积,最大面积):
                {
                    CvInvoke.FillConvexPoly(closingImg, contours[k], new MCvScalar(0));
                }
            }
            #endregion

            #region 边缘检测
            edageImg = closingImg.CopyBlank();
            CvInvoke.Canny(closingImg, edageImg, cannyThreshold, cannyThresholdLinking);
            edageImg.SmoothMedian(5);
            #endregion
            #region HoughLinesP
            //HoughLineP
            lines = CvInvoke.HoughLinesP(
                edageImg,
                1, //Distance resolution in pixel-related units
                Math.PI / 180.0, //Angle resolution measured in radians.
                100, //threshold
                100, //min Line width
                10); //gap between lines);
            #endregion
            return lines;
        }

        /// <summary>
        /// 获取ROI
        /// </summary>
        /// <param name="image">需裁剪的原图</param>
        /// <param name="rect">裁剪留下的ROI大小</param>
        /// <returns>ROI</returns>
        private Image<Gray, byte> GetROI(Image<Gray, byte> image, Rectangle rect)
        {
            //程序中image是原始图像，类型Image<Gray, byte>，rectangle是矩形，CropImage是截得的图像。
            Image<Gray, byte> Sub = image.GetSubRect(rect);//若计算校正后的，需把grayImg==>remapImg
            Image<Gray, byte> CropImage = new Image<Gray, byte>(Sub.Size);
            CvInvoke.cvCopy(Sub, CropImage, IntPtr.Zero);
            return CropImage;
        }

        /// <summary>
        /// 获取高架（右侧）相机矩阵和畸变矩阵，cameraID==0
        /// </summary>
        private void GetRightCamParams()
        {
            //填充相机矩阵
            cameraMatrix[0, 0] = 4930.84899683025;
            cameraMatrix[0, 1] = 0;
            cameraMatrix[0, 2] = 2792.76945597925;
            cameraMatrix[1, 0] = 0;
            cameraMatrix[1, 1] = 4930.27079004687;
            cameraMatrix[1, 2] = 1821.74996840516;
            cameraMatrix[2, 0] = 0;
            cameraMatrix[2, 1] = 0;
            cameraMatrix[2, 2] = 1;
            //填充畸变矩阵
            distCoeffs[0, 0] = -0.0490372460438003;
            distCoeffs[1, 0] = 0.116466805705153;
            distCoeffs[2, 0] = 0;
            distCoeffs[3, 0] = 0;
            distCoeffs[4, 0] = 0;
            //填充坐标变换矩阵
            rightCameraTrans[0, 0] = -1.15017394e-02;
            rightCameraTrans[0, 1] = 6.49269479e-01;
            rightCameraTrans[0, 2] = -1.08420217e-19;
            rightCameraTrans[1, 0] = 6.46775302e-01;
            rightCameraTrans[1, 1] = 9.85949589e-03;
            rightCameraTrans[1, 2] = -2.03287907e-20;
            rightCameraTrans[2, 0] = -1.13760054e+03;
            rightCameraTrans[2, 1] = 8.13282071e+02;
            rightCameraTrans[2, 2] = 1.00000000e+00;
        }

        /// <summary>
        /// 获取前方相机矩阵和畸变矩阵，cameraID==1
        /// </summary>
        private void GetFrontCamParams()
        {
            //填充相机矩阵
            cameraMatrix[0, 0] = 4982.97189998836;
            cameraMatrix[0, 1] = 0;
            cameraMatrix[0, 2] = 2714.21936749570;
            cameraMatrix[1, 0] = 0;
            cameraMatrix[1, 1] = 4981.22950673207;
            cameraMatrix[1, 2] = 1806.92732111017;
            cameraMatrix[2, 0] = 0;
            cameraMatrix[2, 1] = 0;
            cameraMatrix[2, 2] = 1;
            //填充畸变矩阵,先径向再切向
            distCoeffs[0, 0] = -0.0692608998666217;
            distCoeffs[1, 0] = 0.135826745142162;
            distCoeffs[2, 0] = 0;
            distCoeffs[3, 0] = 0;
            distCoeffs[4, 0] = 0;
            //填充坐标变换矩阵
            //填充坐标变换矩阵
            frontCameraTrans[0, 0] = -2.60766882e-03;
            frontCameraTrans[0, 1] = -6.51649339e-01;
            frontCameraTrans[0, 2] = 0.00000000e+00;
            frontCameraTrans[1, 0] = -6.53976411e-01;
            frontCameraTrans[1, 1] = 3.61057195e-03;
            frontCameraTrans[1, 2] = -5.75982404e-20;
            frontCameraTrans[2, 0] = 3.72215718e+03;
            frontCameraTrans[2, 1] = 1.68562888e+03;
            frontCameraTrans[2, 2] = 1.00000000e+00;
        }

        /// <summary>
        /// 获取直线角点和斜率
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public void GetCornerAndSlope(Bitmap img, int cameraID)
        {
            GetLinesByHough(img, cameraID);
            SegmentsClass sc = new SegmentsClass(lines);

            List<Segment> LS = sc.GroupLines(80, 3, 170);

            if (LS.Count != 0)
            {
                Corner = sc.Corner;
                LineK1 = LS[0].az;
                LineK2 = LS[1].az == LS[0].az ? LS[2].az : LS[1].az;
                double tempAngle = LineK1 <= LineK2 ? LineK1 : LineK2;
                if (tempAngle * 180f / Math.PI <= 45)//顺时针
                {
                    RotatedAngle = tempAngle * 180f / Math.PI;
                }
                else//逆时针
                {
                    RotatedAngle = -(90-tempAngle * 180f / Math.PI);
                }
                //cornerPointK.Center = new PointF(cornerPointK.Corner.Average(a => a.X), cornerPointK.Corner.Average(a => a.Y));
                Matrix<double> camerCenter = new Matrix<double>(1, 3)
                {
                    [0, 0] = Corner.Average(a => a.X),
                    [0, 1] = Corner.Average(a => a.Y),
                    [0, 2] = 1
                };
                if (cameraID == 0)
                {
                    Center = new PointF((float.Parse(((camerCenter * rightCameraTrans)[0, 0]).ToString())), (float.Parse(((camerCenter * rightCameraTrans)[0, 1]).ToString())));
                }
                else
                {
                    Center = new PointF((float.Parse(((camerCenter * frontCameraTrans)[0, 0]).ToString())), (float.Parse(((camerCenter * frontCameraTrans)[0, 1]).ToString())));
                }
            }
            else
            {
                Corner = new List<PointF>() { new PointF(-1f, -1f) };
                LineK1 = Double.NaN;
                LineK2 = Double.NaN;
                Center = new PointF(-1, -1);
            }
        }
    }

    /// <summary>
    /// 记录角点坐标和直线斜率类
    /// </summary>
     class CornerPointK
    {
        public PointF Center { get; set; }
        public List<PointF> Corner { get; set; }
        public double LineK1 { get; set; }
        public double LineK2 { get; set; }
    }

    /// <summary>
    /// 线段处理类
    /// CopyRight:ZDY
    /// </summary>
    class SegmentsClass
    {
        public List<PointF> Corner = new List<PointF>();//角点生成无效时,返回此值

        public int num = 0;//输入线段数目
        List<Segment> ss = new List<Segment>();//线段集合1
        List<Segment> mySegment = new List<Segment>();//线段集合2
        Dictionary<Segment, int> mySegmentDic = new Dictionary<Segment, int>();//分组线段
        int k = 0;//记录线段种类
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

                double q = Math.Atan2(dy, dx);
                az = q >= 0 ? q : (Math.PI + q);//归结到 0--PI 范围内
                //az = dy / dx;
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
            List<Segment> s0 = ss.OrderBy(o => o.az).OrderBy(o => o.ro).ToList();//按照方位角--极径排序
        }

        /// <summary>
        /// 对线段分组
        /// </summary>
        /// <param name="Distance2origin">分组参数1:按线段到原点的距离(如:5像素)</param>
        /// <param name="AngleTolerance">分组参数2:按线段的方位角(如:3度)</param>
        /// <returns>分组后的线段集,仅等于二条时,继续计算出角点</returns>
        public List<Segment> GroupLines(int Distance2origin, int AngleToleranceMin, int AngleToleranceMax)
        {
            #region N线相交分组
            foreach (var line in ss)
            {
                mySegmentDic.Add(line, -1);//全部标记为-1
            }


            while (mySegmentDic.ContainsValue(-1))
            {
                Segment pinSegment = new Segment();
                int count = 0;
                int num = mySegmentDic.Count(a => a.Value == -1);//记录字典中标记为-1的线段数量
                Segment[] keySegments = new Segment[num];//储存标记为-1的线段的数组
                var plst = mySegmentDic.Where(a => a.Value == -1).Select(a => a.Key);//筛选出字典中标记为-1的线段
                keySegments = plst.ToArray();//把所有-1的Segment转为数组
                Segment n1 = keySegments[0];
                mySegmentDic[n1] = k;//把数组中第一个线段标记为k
                for (int i = 1; i < num; i++)//遍历剩下的标记为-1的线段
                {
                    if (mySegmentDic[keySegments[i]] == -1)
                    {
                        Segment n2 = keySegments[i];
                        if (Math.Abs(n1.ro - n2.ro) < Distance2origin && (Math.Abs(n1.az - n2.az) * 180f / Math.PI < AngleToleranceMin || Math.Abs(n1.az - n2.az) * 180f / Math.PI > AngleToleranceMax))
                        {
                            mySegmentDic[keySegments[i]] = k;
                        }
                        count++;
                    }
                }
                k++;
            }
            for (int j = 0; j < k; j++)
            {
                Segment gSegment = new Segment();
                gSegment.az = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(a => a.az);
                gSegment.ro = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(o => o.ro);
                gSegment.A = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(o => o.A);
                gSegment.B = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(o => o.B);
                gSegment.C = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(o => o.C);
                gSegment.a = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(o => o.a);
                gSegment.b = mySegmentDic.Where(a => a.Value == j).Select(a => a.Key).Average(o => o.b);

                mySegment.Add(gSegment);
            }
            #endregion
            if (Math.Abs(mySegment[0].az - mySegment[1].az) < 0.5)
            {
                Segment temp = mySegment[1];
                mySegment[1] = mySegment[2];
                mySegment[2] = temp;
            }
            if (Math.Abs(mySegment[1].az - mySegment[2].az) < 0.5)
            {
                Segment temp1 = mySegment[0];
                mySegment[0] = mySegment[1];
                mySegment[1] = temp1;
            }
            //if (mySegment.Count == 4)
            //{
            //    getCorner();
            //}
            getCorner(1.5, 5472, 3648);
            return mySegment;
        }

        /// <summary>
        /// 用克莱姆法则解二元一次方程组,计算角点
        /// </summary>
        /// <param name="factor">溢出因子</param>
        /// <param name="wight">图像宽</param>
        /// <param name="hight">图像高</param>
        private void getCorner(double factor, int wight = 5472, int hight = 3648)
        {
            for (int seg = 0; seg < mySegment.Count; seg++)
            {
                double A1 = mySegment[seg].A;
                double B1 = mySegment[seg].B;
                double C1 = mySegment[seg].C;

                double A2 = mySegment[(seg + 1) % mySegment.Count].A;
                double B2 = mySegment[(seg + 1) % mySegment.Count].B;
                double C2 = mySegment[(seg + 1) % mySegment.Count].C;

                double D = A1 * B2 - A2 * B1;
                double Dx = B1 * C2 - B2 * C1;
                double Dy = A2 * C1 - A1 * C2;
                if (D != 0)
                {
                    PointF tempCorner = new PointF((float)(Dx / D), (float)(Dy / D));
                    //判断是否溢出*1.5
                    if (tempCorner.X > 0 && tempCorner.X < wight * factor && tempCorner.Y > 0 && tempCorner.Y < hight * factor)
                    {
                        Corner.Add(tempCorner);
                    }
                }
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
