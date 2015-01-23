using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.Diagnostics;
using System.Media;

namespace KinectPiano
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor sensor;
        private byte[] colorPixels;
        private WriteableBitmap colorBitmap;
        private Skeleton[] skeletonData;
        private Rect[] rect;
        private Boolean[] teclasFlag = new Boolean[7];
        private SoundPlayer[] player = new SoundPlayer[7];


        public MainWindow()
        {
            InitializeComponent();

            player[0] = new SoundPlayer();
            player[0].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\C4.wav";
            player[1] = new SoundPlayer();
            player[1].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\D4.wav";
            player[2] = new SoundPlayer();
            player[2].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\E4.wav";
            player[3] = new SoundPlayer();
            player[3].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\F4.wav";
            player[4] = new SoundPlayer();
            player[4].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\G4.wav";
            player[5] = new SoundPlayer();
            player[5].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\A4.wav";
            player[6] = new SoundPlayer();
            player[6].SoundLocation = @"C:\Users\Jose Gabriel\Documents\Visual Studio 2013\Projects\KinectPiano\KinectPiano\sounds\B4.wav";

            player[0].Load();
            player[1].Load();
            player[2].Load();
            player[3].Load();
            player[4].Load();
            player[5].Load();
            player[6].Load();

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Closed += new EventHandler(MainWindow_Closed);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (null != this.sensor)
                {
                    this.sensor.Stop();
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            if (this.sensor != null)
            {
                //Habilitar y configurar el stream de la camara
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                //Habilitar y configurar el stream de datos de profundidad
                //this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                //Habilitar y configurar el stream de la cámara infraroja
                //this.sensor.ColorStream.Enable(ColorImageFormat.InfraredResolution640x480Fps30);

                //Habilitar y configurar el stream del skeleton
                this.sensor.SkeletonStream.Enable();
                // Allocate ST data
                skeletonData = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
                // Get Ready for Skeleton Ready Events
                sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinect_SkeletonFrameReady);

                //Obtener rectángulos
                rect = new Rect[7];
                rect[0] = new Rect(Canvas.GetLeft(tecla1), Canvas.GetTop(tecla1), tecla1.Width, tecla1.Height);
                rect[1] = new Rect(Canvas.GetLeft(tecla2), Canvas.GetTop(tecla2), tecla2.Width, tecla2.Height);
                rect[2] = new Rect(Canvas.GetLeft(tecla3), Canvas.GetTop(tecla3), tecla3.Width, tecla3.Height);
                rect[3] = new Rect(Canvas.GetLeft(tecla4), Canvas.GetTop(tecla4), tecla4.Width, tecla4.Height);
                rect[4] = new Rect(Canvas.GetLeft(tecla5), Canvas.GetTop(tecla5), tecla5.Width, tecla5.Height);
                rect[5] = new Rect(Canvas.GetLeft(tecla6), Canvas.GetTop(tecla6), tecla6.Width, tecla6.Height);
                rect[6] = new Rect(Canvas.GetLeft(tecla7), Canvas.GetTop(tecla7), tecla7.Width, tecla7.Height);

                //Iniciar el sensor
                this.sensor.Start();
                
            }
            if (null == this.sensor)
            {
                //Handle failed connection
            }
        }

        private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            {
                if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
                {
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
                    foreach (Skeleton skeleton in skeletonData)
                    {
                        // Get RightHand
                        //Joint rightHand = skeleton.Joints[JointType.HandRight];
                        // Get LeftHand
                        //Joint leftHand = skeleton.Joints[JointType.HandLeft];

                        // 3D coordinates in meters
                        SkeletonPoint skeletonPointR = skeleton.Joints[JointType.HandRight].Position;
                        SkeletonPoint skeletonPointL = skeleton.Joints[JointType.HandLeft].Position;

                        ColorImagePoint rightHand = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPointR, ColorImageFormat.RgbResolution640x480Fps30);
                        ColorImagePoint leftHand = sensor.CoordinateMapper.MapSkeletonPointToColorPoint(skeletonPointL, ColorImageFormat.RgbResolution640x480Fps30);

                        if (skeleton.Joints[JointType.HandRight].TrackingState == JointTrackingState.Tracked 
                            && skeleton.Joints[JointType.HandLeft].TrackingState == JointTrackingState.Tracked)
                        {
                            //Console.WriteLine("Right hand: " + rightHand.Position.X + ", " + rightHand.Position.Y + ", " + rightHand.Position.Z);
                            Console.WriteLine("Left hand: " + leftHand.X + ", " + leftHand.Y);


                            if (rect[0].Contains(rightHand.X, rightHand.Y)
                                || rect[0].Contains(leftHand.X, leftHand.Y))
                            {
                                tecla1.Fill = Brushes.Blue;
                                Console.WriteLine("------------------ 1--------------------");
                                if (!teclasFlag[0])
                                {
                                    teclasFlag[0] = true;
                                    player[0].Play();
                                }
                            }
                            else
                            {
                                tecla1.Fill = Brushes.MidnightBlue;
                                teclasFlag[0] = false;
                                //player.Stop();
                            }
                            
                            if (rect[1].Contains(rightHand.X, rightHand.Y)
                                || rect[1].Contains(leftHand.X, leftHand.Y))
                            {

                                tecla2.Fill = Brushes.Pink; 
                                Console.WriteLine("------------------ 2 --------------------");
                                if (!teclasFlag[1])
                                {
                                    teclasFlag[1] = true;
                                    player[1].Play();
                                }
                            }
                            else
                            {
                                tecla2.Fill = Brushes.DeepPink;
                                teclasFlag[1] = false;
                            }

                            if (rect[2].Contains(rightHand.X, rightHand.Y)
                                || rect[2].Contains(leftHand.X, leftHand.Y))
                            {
                                tecla3.Fill = Brushes.Red;
                                Console.WriteLine("------------------ 3 --------------------");
                                if (!teclasFlag[2])
                                {
                                    teclasFlag[2] = true;
                                    player[2].Play();
                                }
                            }
                            else
                            {
                                tecla3.Fill = Brushes.DarkRed;
                                teclasFlag[2] = false;
                            }

                            if (rect[3].Contains(rightHand.X, rightHand.Y)
                                || rect[3].Contains(leftHand.X, leftHand.Y))
                            {
                                tecla4.Fill = Brushes.Yellow;
                                Console.WriteLine("------------------ 4 --------------------");
                                if (!teclasFlag[3])
                                {
                                    teclasFlag[3] = true;
                                    player[3].Play();
                                }
                            }
                            else
                            {
                                tecla4.Fill = Brushes.YellowGreen;
                                teclasFlag[3] = false;
                            }

                            if (rect[4].Contains(rightHand.X, rightHand.Y)
                                || rect[4].Contains(leftHand.X, leftHand.Y))
                            {
                                tecla5.Fill = Brushes.Green;
                                Console.WriteLine("------------------ 5 --------------------");
                                if (!teclasFlag[4])
                                {
                                    teclasFlag[4] = true;
                                    player[4].Play();
                                }
                            }
                            else
                            {
                                tecla5.Fill = Brushes.DarkGreen;
                                teclasFlag[4] = false;
                            }

                            if (rect[5].Contains(rightHand.X, rightHand.Y)
                                || rect[5].Contains(leftHand.X, leftHand.Y))
                            {
                                tecla6.Fill = Brushes.Cyan;
                                Console.WriteLine("------------------ 6 --------------------");
                                if (!teclasFlag[5])
                                {
                                    teclasFlag[5] = true;
                                    player[5].Play();
                                }
                            }
                            else
                            {
                                tecla6.Fill = Brushes.DarkCyan;
                                teclasFlag[5] = false;
                            }

                            if (rect[6].Contains(rightHand.X, rightHand.Y)
                                || rect[6].Contains(leftHand.X, leftHand.Y))
                            {
                                tecla7.Fill = Brushes.WhiteSmoke;
                                Console.WriteLine("------------------ 7 --------------------");
                                if (!teclasFlag[6])
                                {
                                    teclasFlag[6] = true;
                                    player[6].Play();
                                }
                            }
                            else
                            {
                                tecla7.Fill = Brushes.White;
                                teclasFlag[6] = false;
                            }
                        }
                    }
                }
            }
        }

        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                
                if (colorFrame != null)
                {
                    colorFrame.CopyPixelDataTo(this.colorPixels);
                    this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                }
                if (colorFrame != null)
                {
                    
                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.colorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);

                    this.ColorImage.Source = this.colorBitmap;
                    //this.SkeletalImage
                }
            }
        }


        /*
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.tracked(skel);
                        }
                    }
                }
            }
        }

        public void tracked(Skeleton skeleton)
        {
            Joint jHandRight = skeleton.Joints[JointType.HandRight];
            Joint jHipCenter = skeleton.Joints[JointType.HipCenter];
            if ((jHipCenter.Position.Z - jHandRight.Position.Z) > 0.4)
            {
                //Consider hand raised in front of them
                System.Diagnostics.Debug.WriteLine("Hand: Raised");
            }
            else
            {
                //Hand is lowered by the users side
                System.Diagnostics.Debug.WriteLine("Hand: Lowered");
            }
        }*/
    }
}
