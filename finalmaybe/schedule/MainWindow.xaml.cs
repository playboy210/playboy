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
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.IO;
namespace eventrecord
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        DateTime selectedDate = DateTime.Today;

        int record = 0;

        private KinectSensorChooser sensorChooser;

        

        public MainWindow()
        {
            InitializeComponent();
            
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
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
            var newbutton = new KinectTileButton
            {
                Content = String.Format("添加新事件"),
                Height = 300
            };
            newbutton.Click +=
                 (o, args) => AddNewEvent();
            scrollContent.Children.Add(newbutton);
            string ID = "zyx";
            string ScheduleFileName;
            ScheduleFileName = System.IO.Directory.GetCurrentDirectory();
            ScheduleFileName = @ScheduleFileName + "\\" + ID + ".txt";
            if (File.Exists(ScheduleFileName))
            {  
                string tempstring;
                StreamReader sr = new StreamReader(ScheduleFileName);
                for (record = 0; record <= 365; record++)
                {
                    tempstring = sr.ReadLine();
                    //MessageBox.Show(selectedDate.ToShortDateString());
                    if (tempstring == selectedDate.ToShortDateString())
                    {
                        tempstring = sr.ReadLine();
                        if (tempstring != "" && tempstring != null)
                        {
                            var button = new KinectTileButton
                        {
                            Content = String.Format(tempstring),
                            Height = 300
                        };
                            button.Click +=
                                (o, args) => AddNewEvent();
                            scrollContent.Children.Add(button);
                        }
                    }
                }
                sr.Close();
            }
        }


        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            //MessageBox.Show(calendar.SelectedDate.ToString());
            scrollContent.Children.Clear();
            var newbutton = new KinectTileButton
            {
                Content = String.Format("添加新事件"),
                Height = 300
            };
            selectedDate = calendar.SelectedDate.GetValueOrDefault();
            newbutton.Click +=
                (o, args) => AddNewEvent();
            scrollContent.Children.Add(newbutton);
            string ID = "zyx";
            string ScheduleFileName;
            ScheduleFileName = System.IO.Directory.GetCurrentDirectory();
            ScheduleFileName = @ScheduleFileName + "\\" + ID + ".txt";
            if (File.Exists(ScheduleFileName))
            {
                string tempstring;
                StreamReader sr = new StreamReader(ScheduleFileName);
                for (record = 0; record <= 365; record++)
                {
                    tempstring = sr.ReadLine();
                    //MessageBox.Show(selectedDate.ToShortDateString());
                    if (tempstring == selectedDate.ToShortDateString())
                    {
                        tempstring = sr.ReadLine();
                        if (tempstring != "" && tempstring != null)
                        {
                            var button = new KinectTileButton
                        {
                            Content = String.Format(tempstring),
                            Height = 300
                        };
                            button.Click +=
                                (o, args) => MessageBox.Show(tempstring);
                            scrollContent.Children.Add(button);
                        }
                    }
                }
                sr.Close();
            }
        }
        private void AddNewEvent()
        {
            string ID = "zyx";
            string ScheduleFileName;
            ScheduleFileName = System.IO.Directory.GetCurrentDirectory();
            ScheduleFileName = @ScheduleFileName + "\\" + ID + ".txt";
            StreamWriter sw = File.AppendText(ScheduleFileName);
            sw.WriteLine(selectedDate.ToShortDateString());
            sw.Close();
            new newEvent().Show();
            this.Close();
        }

        private void MetroWindow_Closed_1(object sender, EventArgs e)
        {
            sensorChooser.Stop();
        }

    }
}
