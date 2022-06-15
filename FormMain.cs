using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Tesseract;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LicensePlateRecognition_v0
{
    public partial class frmMain : Form
    {
        #region variable define
        private Image<Bgr, Byte> inputImage;
        //Plate characters images
        private List<Image<Bgr, Byte>> plateCharsImg = new List<Image<Bgr, byte>>();
        private List<String> plateCharsText = new List<String>();

        //Plate detect area
        private Rectangle[] rects;
        PictureBox[] pictureBox = new PictureBox[12];
        static readonly CascadeClassifier motobikeCascadeClassifier = new CascadeClassifier(@"D:\CodeCSharp\LicensePlateRecognition_v0\output-hv-33-x25.xml");


        //

        #endregion

        public frmMain()
        {
            InitializeComponent();
            Console.WriteLine(Application.StartupPath + "\\output-hv-33-x25.xml");
        }

        //Open Image file
        private void btnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                inputImage = new Image<Bgr, Byte>(ofd.FileName);
                
            }
            else
            {
                return;
            }
            
            Image<Gray, Byte> grayImage = inputImage.Convert<Gray, Byte>();
            rects = motobikeCascadeClassifier.DetectMultiScale(grayImage, 1.05, 8, new Size(1,1) , new Size(0,0));
            if(rects.Length ==1)
            {
                Graphics g = Graphics.FromImage(inputImage.Bitmap);
                Pen p = new Pen(Color.Green, 3);
                foreach(Rectangle rect in rects)
                {
                    //Rectangle box = new Rectangle(rect.Location, new Size(rect.Size.Height, rect.Size.Width));

                    g.DrawRectangle(p, rect);
                    //Set crop region
                    grayImage.ROI = rect;
                }
                picInput.Image = inputImage.ToBitmap();
            }
            else
            {
                MessageBox.Show("Please move close to plate");
            }

            //Crop image
            Image<Gray, Byte> cropImage = grayImage.CopyBlank();
            grayImage.CopyTo(cropImage);
            grayImage.ROI = Rectangle.Empty;



            //Grayscale
            //Edge
            Image<Gray, Byte> edge = cropImage.CopyBlank();
            CvInvoke.Canny(cropImage, edge, 30, 200);

            edge = edge.Resize(400,266, Emgu.CV.CvEnum.Inter.Linear);
            //edge = edge.SmoothGaussian(5, 5, 0, 0);
            List<Rectangle> boxes = new List<Rectangle>();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(edge.Copy(), contours, new Mat(), Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            for (int i = 0; i < contours.Size; i++)
            {
                Rectangle box = CvInvoke.BoundingRectangle(contours[i]);
                double ratio = (double)box.Width / box.Height;
                //Console.WriteLine("R.Width = " + box.Width.ToString() + "  R.Height = " + box.Height.ToString());
                if (box.Width > 20 && box.Width < 150
                    && box.Height > 80 && box.Height < 180
                    && ratio > 0.2 && ratio < 1.1)
                {
                    boxes.Add(box);
                }
            }

            //merge box nearby
            
            
            for(int i = 0; i < boxes.Count; i++)
            {
                for(int j = i+1; j < boxes.Count; j++)
                {
                    if((boxes[j].X < (boxes[i].X + boxes[i].Width) && boxes[j].X > boxes[i].X)
                                && (boxes[j].Y < (boxes[i].Y + boxes[i].Width) && boxes[j].Y > boxes[i].Y))
                    {
                        boxes.Remove(boxes[j]);

                    }
                    else if ((boxes[i].X < (boxes[j].X + boxes[j].Width) && boxes[i].X > boxes[j].X)
                                && (boxes[i].Y < (boxes[j].Y + boxes[j].Width) && boxes[i].Y > boxes[j].Y))
                    {
                        boxes.Remove(boxes[i]);
                        break;
                    }
                }
            }
            
            
            //Show detected plates to output
            Console.WriteLine("RECT = " + boxes.Count.ToString());
            Image<Bgr, Byte> outImg = edge.Convert<Bgr, Byte>();
            foreach (Rectangle box in boxes)
            {
                Graphics g = Graphics.FromImage(outImg.Bitmap);
                Pen p = new Pen(Color.Yellow, 2);
                g.DrawRectangle(p, box);
                Console.WriteLine("POS: " + box.X.ToString() + " *** " + box.Y.ToString());
            }

            //Crop image to plate number list
            List<Image<Gray, Byte>> plateNumbers = new List<Image<Gray, byte>>();
            cropImage = cropImage.Resize(400, 266, Emgu.CV.CvEnum.Inter.Linear);
            //
            //CvInvoke.Threshold(cropImage, cropImage, 127, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

            //cropImage.Erode(7);
            //cropImage.Dilate(50);

            for (int i = 0; i < boxes.Count; i++)
            {
                cropImage.ROI = boxes[i];
                Image<Gray, Byte> temp = cropImage.CopyBlank();
                cropImage.CopyTo(temp);
                cropImage.ROI = Rectangle.Empty;
                plateNumbers.Add(temp);

                //save image test
                
            }






            //Using tesseract to detect text
            String outputString = "";
            
            for(int i = 0; i < plateNumbers.Count; i++)
            {
                Image<Gray, Byte> imgProcess = plateNumbers[i].Copy();
                TesseractEngine engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
                Page page = engine.Process(imgProcess.Convert<Bgr, Byte>().Bitmap, PageSegMode.SingleChar);
                outputString += page.GetText();
            }
            outputString =  Regex.Replace(outputString, @"\s", "");
            lblOutput.Text = outputString;
            
            picOutput.Image = outImg.ToBitmap();
            if (plateNumbers.Count > 7)
            {
                FrmStep frmStep = new FrmStep(plateNumbers);
                frmStep.Show();
            }
            //MessageBox.Show("SIZE = " + edge.Size.ToString());
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        #region program functions
        
        #endregion
    }
}
