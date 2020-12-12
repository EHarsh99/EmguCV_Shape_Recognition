using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelloEmgu6
{
    public partial class Form1 : Form
    {
        VideoCapture _capture;
        Thread _captureThread;
        public Form1()
        {
            InitializeComponent();
        }

        private void ProcessImage()
        {

            while (_capture.IsOpened)
            {
                Mat sourceFrame = _capture.QueryFrame();
                int shapeNum = 0;
                int contourNum = 0;
                int triangles = 0;
                int squares = 0;



                //Resize to PictureBox aspect ratio
                int newHeight = (sourceFrame.Size.Height * sourcePictureBox.Size.Width) / sourceFrame.Size.Width;
                Size newSize = new Size(sourcePictureBox.Size.Width, newHeight);
                CvInvoke.Resize(sourceFrame, sourceFrame, newSize);



                var binaryImage = sourceFrame.ToImage<Gray, byte>().ThresholdBinary(new Gray(125), new Gray(255)).Mat;

                var blurredImage = new Mat();
                //var cannyImage = new Mat();
                var decoratedImage = new Mat();
                //CvInvoke.GaussianBlur(sourceFrame, blurredImage, new Size(9, 9), 0);
                //CvInvoke.CvtColor(blurredImage, blurredImage, typeof(Bgr), typeof(Gray));
                //CvInvoke.Canny(blurredImage, cannyImage, 150, 255);

                CvInvoke.CvtColor(binaryImage, decoratedImage, typeof(Gray), typeof(Bgr));

                using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                {
                    CvInvoke.FindContours(binaryImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                    for (int i = 0; i < contours.Size; i++)
                    {
                        double areas = CvInvoke.ContourArea(contours[i]);
                        Rectangle boundingBox = CvInvoke.BoundingRectangle(contours[i]);

                        if (areas > 250 && areas < 5000)
                        {
                            shapeNum++;
                        }

                        if (areas > 250 && areas < 700)
                        {   //detect triangles
                            VectorOfPoint contour = contours[i];
                            CvInvoke.Polylines(decoratedImage, contour, true, new Bgr(Color.Red).MCvScalar);
                            triangles++;
                            MarkDetectedObjectsTri(sourceFrame, contour, boundingBox, areas);
                        }

                        else if (areas > 1000 && areas < 5000)
                        {   //detect squares
                            VectorOfPoint contour = contours[i];
                            CvInvoke.Polylines(decoratedImage, contour, true, new Bgr(Color.Blue).MCvScalar);
                            squares++;
                            MarkDetectedObjectsTri(sourceFrame, contour, boundingBox, areas);
                        }

                        
                            contourNum = i;

                    }
                }

                sourcePictureBox.Image = sourceFrame.Bitmap;
                roiPictureBox.Image = decoratedImage.Bitmap;

                //displays output data(contours, shapes, triangles, squares)
                Invoke(new Action(() =>
                {
                    MessageBox.Text = $"Number of contours: {contourNum}";
                    MessageBox2.Text = $"Number of shapes: {shapeNum}";
                    TriangleBox.Text = $"Number of triangles: {triangles}";
                    SquareBox.Text = $"Number of squares: {squares}";
                }));

            }

        }
        private static void MarkDetectedObject(Mat frame, VectorOfPoint contour, Rectangle boundingBox, double area)
        {   // Drawing contour and box around it    
            CvInvoke.Polylines(frame, contour, true, new Bgr(Color.Red).MCvScalar);
            CvInvoke.Rectangle(frame, boundingBox, new Bgr(Color.Red).MCvScalar);

            // Write information next to marked object  
            Point center = new Point(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
            CvInvoke.Circle(frame, center, 2, new Bgr(Color.Red).MCvScalar);
            var info = new string[] { $"Area: {area}", $"Position: {center.X}, {center.Y}" };
            WriteMultilineText(frame, info, new Point(center.X, boundingBox.Bottom + 12));
        }
        private static void WriteMultilineText(Mat frame, string[] lines, Point origin)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int y = i * 10 + origin.Y;
                // Moving down on each line  
                CvInvoke.PutText(frame, lines[i], new Point(origin.X, y),
                FontFace.HersheyPlain, 0.8, new Bgr(Color.Red).MCvScalar);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _captureThread.Abort();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            _capture = new VideoCapture(1);
            _captureThread = new Thread(ProcessImage);
            _captureThread.Start();
        }

        private void MarkDetectedObjectsTri(Mat frame, VectorOfPoint contour, Rectangle boundingBox, double area)
        {
            //Draw contour & box around it
            CvInvoke.Polylines(frame, contour, true, new Bgr(Color.Red).MCvScalar);
            CvInvoke.Rectangle(frame, boundingBox, new Bgr(Color.Cyan).MCvScalar);

            //information written next to object
            Point center = new Point(boundingBox.X + boundingBox.Width / 2, boundingBox.Y + boundingBox.Height / 2);
        }
    }
}
