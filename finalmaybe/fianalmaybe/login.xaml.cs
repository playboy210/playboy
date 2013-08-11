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
namespace fianalmaybe
{
    public partial class login : Window
    {
        public login()
        {
            InitializeComponent();
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;

            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }
        #region Member Variables
        private KinectSensor sensor;
        private SpeechRecognitionEngine speechEngine;
        #endregion Member Variables
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
            InitializeComponent();
            label1.Content = "loaded";
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

            RecognizerInfo ri = GetKinectRecognizer();
            if (null != ri)
            {
                //label1.Content = "loaded222";
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                //using (var memoryStream = new MemoryStream(128))
                {
                    label1.Content = "loaded4448888888888";
                    var g = new Grammar(memoryStream);
                    speechEngine.LoadGrammar(g);
                }
                speechEngine.SetInputToAudioStream(
                    sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
                label1.Content = "loaded4455554";
                speechEngine.SpeechRecognized += SpeechRecognized;
                //speechEngine.SpeechRecognitionRejected += SpeechRejected;

                label1.Content = "loaded444";
            }
            else
            {
                label1.Content = "shabigjc";
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
                //this.speechEngine.SpeechRecognitionRejected -= SpeechRejected;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            const double ConfidenceThreshold = 0.3;

            label1.Content = "loaded3333";
            if (e.Result.Confidence >= ConfidenceThreshold)
            {
                if (e.Result.Semantics.Value.ToString() == "PLAY")
                {
                    label1.Content = "Got!";
                    var facetrack = new FaceTracking3D.MainWindow();
                    facetrack.Show();
                    this.Close();
                }
            }
        }
    }
}

