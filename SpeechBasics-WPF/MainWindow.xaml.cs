//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Ports;
    using System.Data;
    using System.Text;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;
    using Microsoft.Speech.Recognition;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "In a full-fledged application, the SpeechRecognitionEngine object should be properly disposed. For the sake of simplicity, we're omitting that code in this sample.")]
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Resource key for medium-gray-colored brush.
        /// </summary>
        #region Member Variables

       SerialPort sp = null;
       char command = 's';

        private const string MediumGreyBrushKey = "MediumGreyBrush";
        MediaPlayer player = new MediaPlayer();

        private static readonly Dictionary<Direction, Direction> TurnRight = new Dictionary<Direction, Direction>
            {
                { Direction.Up, Direction.Right },
                { Direction.Right, Direction.Down },
                { Direction.Down, Direction.Left },
                { Direction.Left, Direction.Up }
            };

        private static readonly Dictionary<Direction, Direction> TurnLeft = new Dictionary<Direction, Direction>
            {
                { Direction.Up, Direction.Left },
                { Direction.Right, Direction.Up },
                { Direction.Down, Direction.Right },
                { Direction.Left, Direction.Down }
            };

        private static readonly Dictionary<Direction, Point> Displacements = new Dictionary<Direction, Point>
            {
                { Direction.Up, new Point { X = 0, Y = -1 } },
                { Direction.Right, new Point { X = 1, Y = 0 } },
                { Direction.Down, new Point { X = 0, Y = 1 } },
                { Direction.Left, new Point { X = -1, Y = 0 } }
            };

        /// <summary>
        /// Active Kinect sensor.
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Speech recognition engine using audio data from Kinect.
        /// </summary>
        private SpeechRecognitionEngine speechEngine;

        /// <summary>
        /// Current direction where turtle is facing.
        /// </summary>
        private Direction curDirection = Direction.Up;

        /// <summary>
        /// List of all UI span elements used to select recognized text.
        /// </summary>
        private List<Span> recognitionSpans;
        #endregion Member Variables

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

        }

 
        private enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
        
        private static RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }
            
            return null;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
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

            if (null != this.sensor)
            {
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
                return;
            }

            RecognizerInfo ri = GetKinectRecognizer();

            if (null != ri)
            {
                recognitionSpans = new List<Span> { forwardSpan, backSpan, rightSpan, leftSpan };

                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                /****************************************************************
                * 
                * Use this code to create grammar programmatically rather than from
                * a grammar file.
                * 
                * var directions = new Choices();
                * directions.Add(new SemanticResultValue("forward", "FORWARD"));
                * directions.Add(new SemanticResultValue("forwards", "FORWARD"));
                * directions.Add(new SemanticResultValue("straight", "FORWARD"));
                * directions.Add(new SemanticResultValue("backward", "BACKWARD"));
                * directions.Add(new SemanticResultValue("backwards", "BACKWARD"));
                * directions.Add(new SemanticResultValue("back", "BACKWARD"));
                * directions.Add(new SemanticResultValue("turn left", "LEFT"));
                * directions.Add(new SemanticResultValue("turn right", "RIGHT"));
                *
                * var gb = new GrammarBuilder { Culture = ri.Culture };
                * gb.Append(directions);
                *
                * var g = new Grammar(gb);
                * 
                ****************************************************************/

                // Create a grammar from grammar definition XML file.
                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    speechEngine.LoadGrammar(g);
                }

                speechEngine.SpeechRecognized += SpeechRecognized;
                speechEngine.SpeechRecognitionRejected += SpeechRejected;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            else
            {
                this.statusBarText.Text = Properties.Resources.NoSpeechRecognizer;
            }
        }


        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.AudioSource.Stop();

                this.sensor.Stop();
                this.sensor = null;
            }

            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= SpeechRecognized;
                this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }


        private void ClearRecognitionHighlights()
        {
            foreach (Span span in recognitionSpans)
            {
                span.Foreground = (Brush)this.Resources[MediumGreyBrushKey];
                span.FontWeight = FontWeights.Normal;
            }
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.3;

            // Number of degrees in a right angle.
            const int DegreesInRightAngle = 90;

            // Number of pixels turtle should move forwards or backwards each time.
            const int DisplacementAmount = 60;

            ClearRecognitionHighlights();

            if (e.Result.Confidence >= ConfidenceThreshold)
            {
               textBox1.Text = e.Result.Semantics.Value.ToString();
                switch (e.Result.Semantics.Value.ToString())
                {
                    case "FORWARD":
                        command = '1';
                        forwardSpan.Foreground = Brushes.DeepSkyBlue;
                        forwardSpan.FontWeight = FontWeights.Bold;
                        turtleTranslation.X = (playArea.Width + turtleTranslation.X + (DisplacementAmount * Displacements[curDirection].X)) % playArea.Width;
                        turtleTranslation.Y = (playArea.Height + turtleTranslation.Y + (DisplacementAmount * Displacements[curDirection].Y)) % playArea.Height;
                        break;

                    case "BACKWARD":
                        command = '2';
                        backSpan.Foreground = Brushes.DeepSkyBlue;
                        backSpan.FontWeight = FontWeights.Bold;
                        turtleTranslation.X = (playArea.Width + turtleTranslation.X - (DisplacementAmount * Displacements[curDirection].X)) % playArea.Width;
                        turtleTranslation.Y = (playArea.Height + turtleTranslation.Y - (DisplacementAmount * Displacements[curDirection].Y)) % playArea.Height;
                        break;

                    case "LEFT":
                        command = '3';
                        leftSpan.Foreground = Brushes.DeepSkyBlue;
                        leftSpan.FontWeight = FontWeights.Bold;
                        curDirection = TurnLeft[curDirection];

                        // We take a left turn to mean a counter-clockwise right angle rotation for the displayed turtle.
                        turtleRotation.Angle -= DegreesInRightAngle;
                        break;

                    case "RIGHT":
                        command = '4';
                        rightSpan.Foreground = Brushes.DeepSkyBlue;
                        rightSpan.FontWeight = FontWeights.Bold;
                        curDirection = TurnRight[curDirection];

                        // We take a right turn to mean a clockwise right angle rotation for the displayed turtle.
                        turtleRotation.Angle += DegreesInRightAngle;
                        break;

                   case "HELLO":
                        //MessageBox.Show("Hi,master!","HELLO");
                        mediaElement1.Play();
                        //mediaElement1.Stop();
                        //player.Open(new Uri("MediaElement/voice.wma",UriKind.Relative));
                        //player.Play();
                        break;
                   case "ROTATE":
                        command = 'a';
                        break;
                   case "STOP":
                        command = 's';
                        break;
                }
                this.setProperty();
            }
        }

        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            ClearRecognitionHighlights();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
           mediaElement1.Play();
        }

        private void mediaElement1_MediaEnded(object sender, RoutedEventArgs e)
        {
           mediaElement1.Stop();
        }
        private void setProperty()
        {
           sp = new SerialPort();
           sp.PortName = "COM2";
           sp.BaudRate = 9600;
           sp.StopBits = StopBits.One;
           sp.DataBits = Convert.ToInt16("8");
           sp.Parity = Parity.Odd;
           sp.ReadTimeout = -1;
           try
           {
              sp.Open();
              this.sendData();
              sp.Close();
           }
           catch(Exception)
           { 
              ///
           }
        }

        private void sendData()
        {
           switch (command)
           {
              case '1': sp.Write("1");
                 break;
              case '2': sp.Write("2");
                 break;
              case '3': sp.Write("3");
                 break;
              case '4': sp.Write("4");
                 break;
              case 'a': sp.Write("a");
                 break;
              case 's': sp.Write("s");
                 break;
              default:
                 break;
           }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}