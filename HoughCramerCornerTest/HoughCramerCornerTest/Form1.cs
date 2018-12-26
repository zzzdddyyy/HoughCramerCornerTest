using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.Util;
using Emgu.CV.UI;
using System.Threading;

namespace HoughCramerCornerTest
{
    public partial class Form1 : Form
    {
        DetectCornerAndK detectCorner = new DetectCornerAndK();
        CornerPointK cornerPointK = new CornerPointK();

        private LineSegment2D[] lines;
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

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 获取图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCaptureImg_Click(object sender, EventArgs e)
        {
            btnMethodCal.Enabled = false;
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                myImg = new Image<Bgr, Byte>(openFile.FileName);
                //TODO:处理图像
                GetLinesByHough(myImg.Bitmap);
                DealWithLines(lines);
            }
            else
            {
                return;
            }
        }

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
                10, //Distance resolution in pixel-related units
                Math.PI / 180.0, //Angle resolution measured in radians.
                300, //threshold
                100, //min Line width
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

        private void DealWithLines(LineSegment2D[] lines)
        {
            foreach (LineSegment2D line in lines)
            {
                //以不同颜色绘制两大类线段:
                int dltX = line.P2.X - line.P1.X;
                int dltY = line.P2.Y - line.P1.Y;
                double Ang = 57.3 * Math.Atan2(dltY, dltX);
                CvInvoke.Line(myImg, line.P1, line.P2, new MCvScalar((byte)(127 + Ang % 128), (byte)(3 * Ang % 255), (byte)(2 * Ang % 255)), 5);
                //CvInvoke.Line(remapImg, line.P1, line.P2, new MCvScalar(0), 5);
            }
            SegmentsClass sc = new SegmentsClass(lines);
            List<Segment> LS = sc.GroupLines(5, 3);

            string s = ""; string s2 = "";
            for (int k = 0; k < LS.Count; k++)
            {
                double x1 = LS[k].a;
                double y1 = 0;
                double x2 = 0;
                double y2 = LS[k].b;

                if (Math.Abs(Math.PI / 2f - LS[k].az) > 0.0001)
                {
                    CvInvoke.Line(myImg, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), new MCvScalar(0, 0, 255), 3);
                    CvInvoke.Line(remapImg, new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), new MCvScalar(0), 3);
                }
                else
                {
                    CvInvoke.Line(myImg, new Point((int)x1, 0), new Point((int)x1, 1024), new MCvScalar(0, 0, 255), 3);
                    CvInvoke.Line(remapImg, new Point((int)x1, 0), new Point((int)x1, 1024), new MCvScalar(0), 3);
                }
                //s+="\r\n"+k +":az="+ LS[k].az+",ro="+ LS[k].ro;
                s += "\r\n" + string.Format("A={0:0.##}, B={1:0.##}, C={2:0.##}, ro={3:0.##}, az={4:0.##}, a={5:0.##}, b={6:0.##}",
                         LS[k].A, LS[k].B, LS[k].C, LS[k].ro, LS[k].az, LS[k].a, LS[k].b);

                s2 += string.Format("  Theta{0}={1:0.####}", k+1, 57.3 * Math.Atan(LS[k].az));
            }
            //
            //输出矩形,长方形:  

            //if (sc.Corner.X != -65536)
            //{
            //    //CvInvoke.Circle(remapImg,new Point((int)sc.Corner.X,(int)sc.Corner.Y),5,new MCvScalar(0),5);
            //    CvInvoke.Rectangle(myImg, new Rectangle((int)sc.Corner.X -3, (int)sc.Corner.Y-3 ,6, 6), new MCvScalar(25, 255, 55),5 );
            //    CvInvoke.PutText(myImg, string.Format("x0={0:0.##}  y0={1:0.##}", sc.Corner.X, sc.Corner.Y), new Point((int)sc.Corner.X +20, (int)sc.Corner.Y - 10), FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 1);
            //    //CvInvoke.PutText(remapImg, string.Format("x={0:0.##}  y=`{1:0.##}", sc.Corner.X, sc.Corner.Y), new Point((int)sc.Corner.X - 60, (int)sc.Corner.Y + 20), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 1);
            //    CvInvoke.PutText(myImg, s2, new Point((int)sc.Corner.X, (int)sc.Corner.Y - 40), FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 255), 2);
            //    //CvInvoke.PutText(remapImg, s2, new Point((int)sc.Corner.X - 80, (int)sc.Corner.Y + 40), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(0, 0, 255), 1);
            //}

            pictureBox1.Image = myImg.ToBitmap();
            pictureBox2.Image = remapImg.ToBitmap();
            btnMethodCal.Enabled = true;
        }

        /// <summary>
        /// 调用函数计算
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMethodCal_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            pictureBox2.Image = null;

            btnCaptureImg.Enabled = false;
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                myImg = new Image<Bgr, Byte>(openFile.FileName);
              
            }
            else
            {
                return;
            }
            ////TODO:处理图像
            //cornerPointK = detectCorner.GetCornerAndK(myImg.Bitmap);
            ////if (cornerPointK.Corner.X==-1||cornerPointK.Corner.Y==-1)
            ////{
            ////    MessageBox.Show("未找到角点");
            ////}
            //CvInvoke.Circle(myImg,new Point((int)cornerPointK.Corner.X,(int)cornerPointK.Corner.Y),4,new MCvScalar(0,255,0),3 );
            ////CvInvoke.Circle(myImg,new Point((int)cornerPointK.Corner.X,(int)cornerPointK.Corner.Y),4,new MCvScalar(0,255,0),3 );
            //CvInvoke.PutText(myImg, string.Format("x={0:0.##}  y={1:0.##}", cornerPointK.Corner.X, cornerPointK.Corner.Y), new Point((int)cornerPointK.Corner.X - 60, (int)cornerPointK.Corner.Y + 20), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 1);
            ////CvInvoke.PutText(myImg, string.Format("x={0:0.##}  y={1:0.##}", cornerPointK.Corner.X, cornerPointK.Corner.Y), new Point((int)cornerPointK.Corner.X - 60, (int)cornerPointK.Corner.Y + 20), Emgu.CV.CvEnum.FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 1);
            //txtK1.Text = "LineK1 = "+cornerPointK.LineK1.ToString()+"====="+"角点X:"+cornerPointK.Corner.X;
            //txtK2.Text = "LineK2 = " + cornerPointK.LineK2.ToString() + "=====" + "角点Y:" + cornerPointK.Corner.Y;

            //TODO:处理图像
            cornerPointK = detectCorner.GetCornerAndK(myImg.ToBitmap());

            LineSegment2D[] lines = detectCorner.GetLinesByHough(myImg.Bitmap);
            foreach (LineSegment2D line in lines)
            {
                myImg.Draw(line, new Bgr(Color.Red), 3);
                detectCorner.binaryImg.Draw(line, new Gray(125), 3);
            }
            for (int c = 0; c < cornerPointK.Corner.Count; c++)
            {
                CvInvoke.Circle(myImg, new Point((int)cornerPointK.Corner[c].X, (int)cornerPointK.Corner[c].Y), 4, new MCvScalar(0, 255, 0), 3);
                CvInvoke.Circle(myImg, new Point((int)cornerPointK.Center.X, (int)cornerPointK.Center.Y), 4, new MCvScalar(255, 255, 0), 3);
                //CvInvoke.Circle(myImg,new Point((int)cornerPointK.Corner.X,(int)cornerPointK.Corner.Y),4,new MCvScalar(0,255,0),3 );
                CvInvoke.PutText(myImg, string.Format("x={0:0.##}  y={1:0.##}", cornerPointK.Corner[c].X, cornerPointK.Corner[c].Y),
                    new Point((int)cornerPointK.Corner[c].X - 60, (int)cornerPointK.Corner[c].Y + 20), FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 1);

                CvInvoke.PutText(myImg, string.Format("x={0:0.##}  y={1:0.##}", cornerPointK.Center.X, cornerPointK.Center.Y),
                    new Point((int)cornerPointK.Center.X - 60, (int)cornerPointK.Center.Y + 80), FontFace.HersheyPlain, 2, new MCvScalar(0, 255, 0), 1);
            }
            pictureBox1.Image = myImg.ToBitmap();
            pictureBox2.Image = detectCorner.binaryImg.ToBitmap();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}
