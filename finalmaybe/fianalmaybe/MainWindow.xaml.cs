using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.Drawing;
using MahApps.Metro;
using MahApps.Metro.Controls;
namespace fianalmaybe
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private KinectSensorChooser sensorChooser;
        public MainWindow()
        {
            this.InitializeComponent();
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
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
        }
        private void Button1_Click_1(object sender, RoutedEventArgs e)
        {
            var FaceTrackWindow = new FaceTrackMainWindow();
            FaceTrackWindow.Show();
            this.Close();
        }

        private void KinectTileButton_Click(object sender, RoutedEventArgs e)
        {
            var SpeechWindow = new Microsoft.Samples.Kinect.SpeechBasics.MainWindow();
            SpeechWindow.Show();
            this.Close();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
                try
                {
                    if (!error)
                        kinectRegion1.KinectSensor = args.NewSensor;
                }
                catch (Exception)
                {

                    throw;
                }

            }
        }

        private void KinectCircleButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MetroWindow_Closed_1(object sender, EventArgs e)
        {
            sensorChooser.Stop();
        }

        private void KinectTileButton_Click_1(object sender, RoutedEventArgs e)
        {
            new eventrecord.MainWindow().Show();
            this.Close();
        }

        private void KinectTileButton_Click_2(object sender, RoutedEventArgs e)
        {
            new objection.MainWindow().Show();
            this.Close();
        }

        //private void button1_MouseEnter(object sender, MouseEventArgs e)
        //{
        //   ImageBrush imageBrush = new ImageBrush();
        //   imageBrush.ImageSource = new BitmapImage(new Uri("voicecontroller_icon1.png", UriKind.Relative));
        //   button1.Background = imageBrush;

        //}

     
    }
}
