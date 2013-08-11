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
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Emgu.CV.Features2D;
using Emgu.CV.Reflection;
using Emgu.CV.Structure;
using Microsoft.Speech.Synthesis;
using Emgu.CV;  
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Windows.Forms;
using Drawing = System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using ImageManipulationExtensionMethods;
using System.IO.Ports;
namespace objection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string KinectName = "playboy";

        //public KinectSensor sensor;

        KinectSensor sensor;

        public KinectSensorChooser sensorChooser;

        public bool isBottleFound = false;

        public bool isBootleAttained = false;

        public Drawing.Bitmap colorBitmap2;

        public struct Bottle
        {
            public double bottleHeight;
            public SkeletonPoint bottleCoordinate;
            public double bottleWidth;
        }

        SerialPort sp = null;

        public bool isListening;

        public SpeechRecognitionEngine speechRecognizer;

        public DispatcherTimer sr_ReadyTimer;

        public DispatcherTimer kinectListningTimer;

        int countDown2 = 20;

        String[] speechCommands = new string[19];

        SkeletonPoint bottleCoordinate;

        //DepthImagePoint

        public DepthImagePixel[] depthPixels;


        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        private byte[] colorImage;

        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;

        private WriteableBitmap colorImageWritableBitmap;

        private short[] depthImage;

        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        private Skeleton[] skeletonData;

        bool toFindBottle = false;





        public MainWindow()
        {
            this.Closed += this.WindowClosed;
            this.InitializeComponent();
        }

        private void WindowClosed(object sender, EventArgs e)
        {

            this.sensorChooser.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.Start();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
           
        }

        //private void AllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        //{
        //    ColorImageFrame colorImageFrame = null;
        //    DepthImageFrame depthImageFrame = null;
        //    SkeletonFrame skeletonFrame = null;

        //    try
        //    {
        //        colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
        //        depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
        //        skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

        //        if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
        //        {
        //            return;
        //        }

        //        // Check for changes in any of the data this function is receiving
        //        // and reset things appropriately.
        //        if (this.depthImageFormat != depthImageFrame.Format)
        //        {
        //            this.depthImage = null;
        //            this.depthImageFormat = depthImageFrame.Format;
        //        }

        //        if (this.colorImageFormat != colorImageFrame.Format)
        //        {
        //            this.colorImage = null;
        //            this.colorImageFormat = colorImageFrame.Format;
        //            this.colorImageWritableBitmap = null;
        //            this.CurrentRGBImage.Source = null;
        //        }

        //        if (this.skeletonData != null && this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
        //        {
        //            this.skeletonData = null;
        //        }

        //        // Create any buffers to store copies of the data we work with
        //        if (this.depthImage == null)
        //        {
        //            this.depthImage = new short[depthImageFrame.PixelDataLength];
        //        }

        //        if (this.colorImage == null)
        //        {
        //            this.colorImage = new byte[colorImageFrame.PixelDataLength];
        //        }

        //        if (this.colorImageWritableBitmap == null)
        //        {
        //            this.colorImageWritableBitmap = new WriteableBitmap(
        //                colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
        //            this.CurrentRGBImage.Source = this.colorImageWritableBitmap;
        //        }

        //        if (this.skeletonData == null)
        //        {
        //            this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
        //        }

        //        // Copy data received in this event to our buffers.
        //        colorImageFrame.CopyPixelDataTo(this.colorImage);
        //        depthImageFrame.CopyPixelDataTo(this.depthImage);
        //        skeletonFrame.CopySkeletonDataTo(this.skeletonData);
        //        this.colorImageWritableBitmap.WritePixels(
        //            new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
        //            this.colorImage,
        //            colorImageFrame.Width * Bgr32BytesPerPixel,
        //            0);

        //        // Find a skeleton to track.
        //        // First see if our old one is good.
        //        // When a skeleton is in PositionOnly tracking state, don't pick a new one
        //        // as it may become fully tracked again.
        //    }
        //    finally
        //    {
        //        if (colorImageFrame != null)
        //        {
        //            colorImageFrame.Dispose();
        //        }

        //        if (depthImageFrame != null)
        //        {
        //            depthImageFrame.Dispose();
        //        }

        //        if (skeletonFrame != null)
        //        {
        //            skeletonFrame.Dispose();
        //        }
        //    }
        //}

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            sensor = kinectChangedEventArgs.NewSensor;

            if (oldSensor != null)
            {
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (sensor != null)
            {
                try
                {
                    sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    try
                    {
                        //This will throw on non Kinect For Windows devices.
                        sensor.DepthStream.Range = DepthRange.Near;
                        sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        sensor.DepthStream.Range = DepthRange.Default;
                        sensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    sensor.SkeletonStream.Enable();
                    //newSensor.AllFramesReady += this.AllFramesReady;
                    //sensor.ColorFrameReady += this.SensorColorFrameReady;
                    //sensor.AllFramesReady += FindBottle;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    if (this.colorImage == null)
                    {
                        this.colorImage = new byte[colorFrame.PixelDataLength];
                    }
                        this.colorImageWritableBitmap = new WriteableBitmap(
                                 colorFrame.Width, colorFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorImage);

                    // Write the pixel data into our bitmap
                    this.colorImageWritableBitmap.WritePixels(
                        new Int32Rect(0, 0, colorFrame.Width, colorFrame.Height),
                        this.colorImage,
                         colorFrame.Width * Bgr32BytesPerPixel,
                        0);
                    CurrentRGBImage.Source = colorImageWritableBitmap;
                }
            }
           
        }
        #region 语音识别
        //public bool IsKinectCalled()
        //{
        //    bool IsKienctCalled = false;
        //    return (IsKienctCalled);
        //}
        public double beamAngle;

        public double BeamAngle
        {
            get { return beamAngle; }
            set
            {
                beamAngle = value;
            }
        }

        public void loadDefaultCommands()
        {
            speechCommands = System.IO.File.ReadAllLines("kinectMediaController_speechCommands.txt");
        }


        public void initializeSpeechCommandsArray()
        {
            try
            {
                speechCommands[0] = KinectName;
                speechCommands[1] = "bottle";
                speechCommands[2] = "me";
            }
            catch (NullReferenceException)
            {

            }
        }

        public void sr_ReadyCountDown()
        {
            sr_ReadyTimer = new DispatcherTimer();
            sr_ReadyTimer.Tick += new EventHandler(this.srInitializing);

            sr_ReadyTimer.Interval = new TimeSpan(0, 0, 1, 0, 0); // 1000 Milliseconds 
            sr_ReadyTimer.Start();
        }
        int countDown1 = 4;
        public void srInitializing(object o, EventArgs sender)
        {
            --countDown1;
            if (countDown1 == 3)
            {
                myKinectReply.Text = "preparing media controller.";
            }
            if (countDown1 == 2)
            {
                myKinectReply.Text = "preparing media controller..";
            }
            if (countDown1 == 1)
            {
                myKinectReply.Text = "preparing media controller...";
            }
            if (countDown1 == 0)
            {
                myKinectReply.Text = "Ready";
            }
            if (countDown1 == -1)
            {
                myKinectReply.Text = "";

                sr_ReadyTimer.Stop();
                sr_ReadyTimer = null;

                countDown1 = 4;
            }

        }

        private void Start()
        {
            //Kinect = KinectSensor.KinectSensors
            //                      .FirstOrDefault(s => s.Status == KinectStatus.Connected);
            
            //set sensor audio source to variable
            var audioSource = sensorChooser.Kinect.AudioSource;
            //var audioSource = Kinect.AudioSource;
            //Set the beam angle mode - the direction the audio beam is pointing
            //we want it to be set to adaptive
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            // audioSource.BeamAngle = Math.PI / 180.0 * 10.0; //angle in radians
            //start the audiosource 
            audioSource.BeamAngleChanged += audioSource_BeamAngleChanged;

            var kinectStream = audioSource.Start();
            //configure incoming audio stream
            speechRecognizer.SetInputToAudioStream(
                kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            //make sure the recognizer does not stop after completing 	
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
            //reduce background and ambient noise for better accuracy
            sensorChooser.Kinect.AudioSource.EchoCancellationMode = EchoCancellationMode.None;
            sensorChooser.Kinect.AudioSource.AutomaticGainControlEnabled = false;
        }
        public void audioSource_BeamAngleChanged(object sender, BeamAngleChangedEventArgs e)
        {
            BeamAngle = -1 * e.Angle;
        }
        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            //set recognizer info
            RecognizerInfo ri = GetKinectRecognizer();
            //create instance of SRE
             SpeechRecognitionEngine sre = new SpeechRecognitionEngine(ri.Id);

            //Add the words we want our program to recognise
            var grammar = new Choices();
            grammar.Add(KinectName);
            grammar.Add("bottle");
            grammar.Add("me");
            //set culture - language, country/region
            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            //set up the grammar builder
            var g = new Grammar(gb);
            sre.LoadGrammar(g);

            //Set events for recognizing, hypothesising and rejecting speech
            sre.SpeechRecognized += SreSpeechRecognized;
            sre.SpeechHypothesized += SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;
            return sre;
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < 0.5)
            {
                //RejectSpeech(e.Result);
                myKinectReply.Text =string.Format("{0}?",e.Result.Text);
            }
            else
            {
                if (e.Result.Text == KinectName)
                {
                    myKinectReply.Text = "playboy";
                    kinectListening();
                    isListening = true;
                    kinectListening_ReadyCountDown();
                }
                if (e.Result.Text == "bottle")
                {
                    myKinectReply.Text = "bottle";
                    toFindBottle = true;
                    sensor.AllFramesReady += FindBottle;
                     //FindBottle();

                }
                if (e.Result.Text == "me")
                {
                    myKinectReply.Text  = "me";
                    FindHuman();
                }
            }
        }

        private void RejectSpeech(RecognitionResult result)
        {
            if (isListening)
            {
                myKinectReply.Text = "Pardon Moi?";
            }
        }

        public void kinectListening()
        {
            backcolor.Source = new BitmapImage(new Uri("Green.png", UriKind.Relative));

        }
        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            RejectSpeech(e.Result);
        }

        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            if (e.Result.Confidence > 0)
            {
                //feedback
            }

        }

        public void kinectListening_ReadyCountDown()
        {
            if (this.kinectListningTimer != null)
            {
                this.kinectListningTimer.Stop();
                this.kinectListningTimer = null;
                countDown2 = 20;
            }

            this.kinectListningTimer = new DispatcherTimer();
            this.kinectListningTimer.Tick += new EventHandler(this.kinectListeningTimerInitialized);

            this.kinectListningTimer.Interval = new TimeSpan(0, 0, 0, 1, 0); // 100 Milliseconds 
            this.kinectListningTimer.Start();
        }
        public void kinectNotListening()
        {
            isListening = false;
            backcolor.Source = new BitmapImage(new Uri("Red.png", UriKind.Relative));
        }
        public void kinectListeningTimerInitialized(object o, EventArgs sender)
        {
            --countDown2;

            if (countDown2 == 16)
            {
                myKinectReply.Text = "";
            }

            if (countDown2 == 15)
            {
                myKinectReply.Text = "I am listening.";
            }

            if (countDown2 == 14)
            {
                myKinectReply.Text = "I am listening..";
            }
            if (countDown2 == 13)
            {
                myKinectReply.Text = "I am listening...";
            }

            if (countDown2 == 11)
            {
                myKinectReply.Text = "OK.";
            }
            if (countDown2 == 10)
            {
                myKinectReply.Text = "OK..";
            }
            if (countDown2 == 9)
            {
                myKinectReply.Text = "OK...";
            }

            if (countDown2 == 8)
            {
                myKinectReply.Text = "Entering standby.";
            }
            if (countDown2 == 7)
            {
                myKinectReply.Text = "Entering standby..";
            }
            if (countDown2 == 6)
            {
                myKinectReply.Text = "Entering standby...";
            }

            if (countDown2 <= 5)
            {
                myKinectReply.Text = "";
                kinectNotListening();
            }

            if (countDown2 <= 0)
            {
                this.kinectListningTimer.Stop();
                this.kinectListningTimer = null;
                countDown2 = 20;
            }
        }
        #endregion 语音识别

        #region 物体识别
        public void FindBottle(object sender,AllFramesReadyEventArgs e)
        {
            //contour Hu矩不靠谱！！！！
            //if (isBottleFound == false)
            //{
            //    SkeletonPoint bottleCoordinate;
            //    var colorframe = sensor.ColorStream.OpenNextFrame(100);
            //    var colorimage = colorframe.ToOpenCVImage<Bgr, Byte>();
            //    var depthframe = sensor.DepthStream.OpenNextFrame(100);
            //    //var depthimage = depthframe.ToOpenCVImage<Gray, Byte>();
            //    using (var depthimage = depthframe.ToOpenCVImage<Gray, Byte>())
            //    {
            //        double recognitionResult = 0;
            //        Image<Gray, Byte> imageThreshold = depthimage.ThresholdBinary(new Gray(100d), new Gray(255d));
            //        Image<Bgr, Byte> imageBottle = new Image<Bgr, byte>("spirit.jpg");
            //        Image<Gray, Byte> imageBottleGray = imageBottle.Convert<Gray, Byte>();
            //        Image<Gray, Byte> imageBottleThreshold = imageBottleGray.ThresholdBinary(new Gray(100d), new Gray(255d));
            //        Contour<Drawing.Point> contour1 = imageThreshold.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);
            //        Contour<Drawing.Point> contour2 = imageBottleThreshold.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL);
            //        recognitionResult = MatchShapes(contour1, contour2);
            //        if (recognitionResult < 10) //maybe!!!!!!!!
            //        {
            ////            bottleCoordinate = DetermineCoordinate(contour1);
            ////        }
            ////        MoveRobotToDestination();
            //    }
            //}
            if (isBottleFound == false)
            {
                DepthImageFrame depthframe = e.OpenDepthImageFrame();
                ColorImageFrame colorframe = e.OpenColorImageFrame();
                SkeletonPoint bottleCoordinate;
                ColorImagePoint[] colorpoint = new ColorImagePoint[sensor.ColorStream.FramePixelDataLength];
                //ColorImagePoint colorImagePoint = new ColorImagePoint();
                DepthImagePoint depthImagePoint = new DepthImagePoint();
                DepthImagePoint[] depthPoints = new DepthImagePoint[sensor.DepthStream.FramePixelDataLength];
                //DepthImagePoint[] depthPoint = new DepthImagePoint[this.sensor.DepthStream.FramePixelDataLength];
                //SkeletonPoint[] skeletonPoint = new SkeletonPoint[this.sensor.SkeletonStream.FrameSkeletonArrayLength];
                SkeletonPoint[] skeletonPoint = new SkeletonPoint[this.sensor.DepthStream.FramePixelDataLength];
                //var depthFrame = e.OpenDepthImageFrame();
                //               var colorframe = e.OpenColorImageFrame();
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];
                if(depthframe !=null && colorframe != null)
                {
                    depthframe.CopyDepthImagePixelDataTo(this.depthPixels);
                    var colorimage = colorframe.ToOpenCVImage<Bgr, Byte>();
                    Drawing.Point middlePoint = new Drawing.Point(0, 0);
                    //var depthframe = sensor.DepthStream.OpenNextFrame(100);
                    long matchTime;
                    MKeyPoint[] keyPoints;
                    double number;
                    List<int> index;
                    using (Image<Gray, Byte> modelImage = new Image<Gray, byte>("spirit.png"))
                    using (Image<Gray, Byte> observedImage = colorframe.ToOpenCVImage<Gray, Byte>())
                    {
                        Image<Bgr, byte> result = DrawMatches.Draw(modelImage, observedImage, out matchTime, out number, out keyPoints, out index);
                        resultImage.Source = result.ToBitmapSource();
                    }
                    if (number >= 6)
                    {
                        //MoveRobotToDestination();
                        //假设
                        sensor.AllFramesReady -= FindBottle;
                        #region GetbottleCoordinate
                        CoordinateMapper changeCoordinate = new CoordinateMapper(sensorChooser.Kinect);
                        //changeCoordinate.MapColorFrameToSkeletonFrame(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30
                        //    ,depthPixels, skeletonPoint);
                       // changeCoordinate.MapColorFrameToDepthFrame(ColorImageFormat.RgbResolution1280x960Fps12, DepthImageFormat.Resolution640x480Fps30, depthPixels,depthPoint);
                        foreach (var _index in index)
                        {
                            middlePoint.X += (int)keyPoints[_index].Point.X;
                            middlePoint.Y += (int)keyPoints[_index].Point.Y;
                        }
                        middlePoint.X = middlePoint.X / index.Count;
                        middlePoint.Y = middlePoint.Y / index.Count;
                        //暂定如此！！！！
                        //changeCoordinate.MapColorFrameToDepthFrame(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30, depthPixels, depthPoints);
                        changeCoordinate.MapColorFrameToSkeletonFrame(ColorImageFormat.RgbResolution640x480Fps30, DepthImageFormat.Resolution640x480Fps30, depthPixels,skeletonPoint);
                        //depthImagePoint.X = middlePoint.X;
                        //depthImagePoint.Y = middlePoint.Y;
                        //depthImagePoint.Depth = depthPoints[depthImagePoint.Y * 640 + depthImagePoint.X].Depth;
                        //bottleCoordinate = changeCoordinate.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, depthImagePoint);
                        #endregion GetbottleCoordinate
                        long Skeletonindex = 640 * middlePoint.Y + middlePoint.X;
                        bottleCoordinate = skeletonPoint[Skeletonindex];
                        //bottleCoordinate.X = 10; 
                        //bottleCoordinate.Y = 10;
                        coordinate.Text = string.Format("{0:F4},{1:F4},{2:F4}", bottleCoordinate.X, bottleCoordinate.Y, bottleCoordinate.Z);
                        MoveRobotToDestination();
                    }
                }
            }
        }

        private double MatchShapes(Contour<Drawing.Point> contour1, Contour<Drawing.Point> contour2)
        {
            //匹配方法
            CONTOURS_MATCH_TYPE matchType = CONTOURS_MATCH_TYPE.CV_CONTOUR_MATCH_I1;
            double matchValue = contour1.MatchShapes(contour2, matchType);
            return (matchValue);
        }

        public SkeletonPoint DetermineCoordinate(Contour<Drawing.Point> contour)
        {
            SkeletonPoint kinectCoordinate;
            DepthImagePoint featurePoint = new DepthImagePoint();
            CoordinateMapper changeCoordinate = new CoordinateMapper(sensorChooser.Kinect);
            Drawing.Point middlePoint = new Drawing.Point(0, 0);
            for (int i = 0; i < contour.Total; i++)
            {
                middlePoint.X += contour[i].X;
                middlePoint.Y += contour[i].Y;
            }
            featurePoint.X = middlePoint.X / contour.Total;
            featurePoint.Y = middlePoint.Y / contour.Total;
            kinectCoordinate = changeCoordinate.MapDepthPointToSkeletonPoint(DepthImageFormat.Resolution640x480Fps30, featurePoint);
            return (kinectCoordinate);
        }
        #endregion 物体识别
        public void MoveRobotToDestination()
        {
            ///手坐标
            ///x=bootlecoordinate.x
            ///z=rightvalue
            ///y=bottlecoordiante.y
            isBootleAttained = true;
        }

        public void PassTheBottle()
        {
            if (isBootleAttained)
            {
                //turn 180 degree
                //says:'For whom'
                //
                myKinectReply.Text = "Who?";
                Start();
                initializeSpeechCommandsArray();
                sr_ReadyCountDown();
                //语音识别。辨认方向
                //turn to that way
                FindHuman();
                //go
                //pass
                isBootleAttained = false;
                isBottleFound = false;
            }
        }

        public void FindHuman()
        {
            //turn beamangle =》degree;
            sensorChooser.Kinect.SkeletonFrameReady += SkeletonFrameReady;
            this.SetProperty();
            //已获取右手坐标，go!
        }

        SkeletonPoint rightHandCoordinate;

        public void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            CoordinateMapper coordinateChanger = new CoordinateMapper(sensorChooser.Kinect);
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;

                var skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                skeletonFrame.CopySkeletonDataTo(skeletons);
                foreach (Skeleton skeletonData in skeletons)
                {
                    if (skeletonData.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        rightHandCoordinate = skeletonData.Joints[JointType.HandRight].Position;
                    }
                }
            }

        }
        private void SetProperty()
        {
            sp = new SerialPort();
            sp.PortName = "com2";
            sp.BaudRate = 9600;
            sp.StopBits = StopBits.One;
            sp.DataBits = Convert.ToInt16("8");
            sp.Parity = Parity.Odd;
            sp.ReadTimeout = -1;
            try
            {
                sp.Open();
                //
                sp.Close();
            }
            catch (Exception)
            {
                ///
            }
        }

        private void robotgo_Click(object sender, RoutedEventArgs e)
        {
            speechRecognizer = CreateSpeechRecognizer();
            loadDefaultCommands();
            myKinectReply.Text = "media controller ready";
            Start();
            initializeSpeechCommandsArray();
            sr_ReadyCountDown();
        }
    }
}
