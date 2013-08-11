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
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using iFlyDotNet;
using Microsoft.Kinect.Toolkit;
namespace eventrecord
{
    /// <summary>
    /// 记事.xaml 的交互逻辑
    /// </summary>
    public partial class newEvent : Window
    {

        KinectSensor sensor;

        private KinectSensorChooser sensorChooser;

        bool translating = false;
        KinectAudioSource audioSource;


        //public List<string> CreateWav = new List<string>();
        private struct WAVEFORMATEX
        {
            public ushort FormatTag;
            public ushort Channels;
            public uint SamplesPerSec;
            public uint AvgBytesPerSec;
            public ushort BlockAlign;
            public ushort BitsPerSample;
            public ushort Size;
        }
        private bool isRecoding = false;

        private const int RiffHeaderSize = 20;

        private const string RiffHeaderTag = "RIFF";

        private const int WaveformatExSize = 18; // native sizeof(WAVEFORMATEX)

        private const int DataHeaderSize = 8;

        private const string DataHeaderTag = "data";

        private const int FullHeaderSize = RiffHeaderSize + WaveformatExSize + DataHeaderSize;
        public newEvent()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.WindowState = System.Windows.WindowState.Normal;
            //this.WindowStyle = System.Windows.WindowStyle.None;
            //this.ResizeMode = System.Windows.ResizeMode.NoResize;
            //this.Topmost = true;

            //this.Left = 0.0;
            //this.Top = 0.0;
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
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
                    sensor = args.NewSensor;

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

            }
        }
        public string OutputFileName = System.IO.Directory.GetCurrentDirectory();
        public void AudioRecord()
        { 
            var buffer = new byte[5000];
            int recordingLength = 0;
            //string OutputFileName = System.IO.Directory.GetCurrentDirectory();
            OutputFileName = OutputFileName + "\\" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav";
            //CreateWav.Add(OutputFileName);
            using (var fileStream = new FileStream(OutputFileName, FileMode.OpenOrCreate))
            {
                FileStream logStream = null;
                string logname = System.IO.Directory.GetCurrentDirectory();
                logname = logname + "\\" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
                logStream = new FileStream(logname, FileMode.OpenOrCreate);
                using (var dreamEventStream = new StreamWriter(logStream))
                {
                    logStream = null;
                    WriteWavHeader(fileStream);

                    dreamEventStream.WriteLine(System.DateTime.Now.ToString("yyyyMMddHHmmss") + "开始录音");


                    // Start capturing audio                               
                    using (var audioStream = audioSource.Start())
                    {
                        // Simply copy the data from the stream down to the file
                        int count;
                        bool readStream = true;
                        while (readStream && ((count = audioStream.Read(buffer, 0, buffer.Length)) > 0))
                        {
                            //当听到话时才录音
                            if (audioSource.SoundSourceAngleConfidence > 0.5)
                            {
                                dreamEventStream.WriteLine(System.DateTime.Now.ToString("yyyyMMddHHmmss"));
                                fileStream.Write(buffer, 0, count);
                            }

                            recordingLength += count;

                            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new EmptyDelegate(delegate { }));
                            if (!isRecoding)
                                break;
                        }
                    }

                    dreamEventStream.WriteLine(System.DateTime.Now.ToString("yyyyMMddHHmmss") + "停止录音");
                   
                    UpdateDataLength(fileStream, recordingLength);
                }
                if (logStream != null)
                {
                    logStream.Dispose();
                }
                TransSpeech(OutputFileName);
                OutputFileName = System.IO.Directory.GetCurrentDirectory();
            }
        }

        private delegate void EmptyDelegate();

        private static void UpdateDataLength(Stream stream, int dataLength)
        {
            using (var bw = new BinaryWriter(stream))
            {
                // Write file size - 8 to riff header
                bw.Seek(RiffHeaderTag.Length, SeekOrigin.Begin);
                bw.Write(dataLength + FullHeaderSize - 8);

                // Write data size to data header
                bw.Seek(FullHeaderSize - 4, SeekOrigin.Begin);
                bw.Write(dataLength);
            }
        }

        private static void WriteHeaderString(Stream stream, string s)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteWavHeader(Stream stream)
        {
            // Data length to be fixed up later
            int dataLength = 0;

            // We need to use a memory stream because the BinaryWriter will close the underlying stream when it is closed
            MemoryStream memStream = null;
            BinaryWriter bw = null;

            try
            {
                memStream = new MemoryStream(64);

                WAVEFORMATEX format = new WAVEFORMATEX
                {
                    FormatTag = 1,
                    Channels = 1,
                    SamplesPerSec = 16000,
                    AvgBytesPerSec = 32000,
                    BlockAlign = 2,
                    BitsPerSample = 16,
                    Size = 0
                };

                bw = new BinaryWriter(memStream);

                // RIFF header
                WriteHeaderString(memStream, RiffHeaderTag);
                bw.Write(dataLength + FullHeaderSize - 8); // File size - 8
                WriteHeaderString(memStream, "WAVE");
                WriteHeaderString(memStream, "fmt ");
                bw.Write(WaveformatExSize);

                // WAVEFORMATEX
                bw.Write(format.FormatTag);
                bw.Write(format.Channels);
                bw.Write(format.SamplesPerSec);
                bw.Write(format.AvgBytesPerSec);
                bw.Write(format.BlockAlign);
                bw.Write(format.BitsPerSample);
                bw.Write(format.Size);

                // data header
                WriteHeaderString(memStream, DataHeaderTag);
                bw.Write(dataLength);
                memStream.WriteTo(stream);
            }
            finally
            {
                if (bw != null)
                {
                    memStream = null;
                    bw.Dispose();
                }

                if (memStream != null)
                {
                    memStream.Dispose();
                }
            }
        }

        public void TransSpeech(string speech)
        {
            button.IsEnabled = false;
            string c1 = "server_url=dev.voicecloud.cn,appid=51fc7487,timeout=10000";
            string c2 = "sub=iat,ssm=1,auf=audio/L16;rate=16000,aue=speex,ent=sms16k,rst=plain";
            iFlyISR isr = new iFlyISR(c1, c2);
            isr.DataArrived += new EventHandler<iFlyISR.DataArrivedEventArgs>(asr_DataAvailable);
            isr.ISREnd += new EventHandler(Isr_ISREnd);
            isr.Audio2TxtAsync(speech, null);
        }

        void Isr_ISREnd(object sender, EventArgs e)
        {
            button.Dispatcher.Invoke(new Action(() => { button.IsEnabled = true; }));
            //button.IsEnabled = true;
        }
        void asr_DataAvailable(object sender, iFlyISR.DataArrivedEventArgs e)
        {
            result.Dispatcher.Invoke(new Action(() => { result.AppendText(e.result);  }));
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            //foreach (var wav in CreateWav)
            //{
            //    string c1 = "server_url=dev.voicecloud.cn,appid=4e7bf06d,timeout=10000";
            //    string c2 = "sub=iat,ssm=1,auf=audio/L16;rate=16000,aue=speex,ent=sms16k,rst=plain";
            //    iFlyISR isr = new iFlyISR(c1, c2);
            //    //using(iFlyISR isr = new iFlyISR(c1, c2))
            //    //{
            //    //    isr.DataArrived += new EventHandler<iFlyISR.DataArrivedEventArgs>(asr_DataAvailable);
            //    //    isr.ISREnd += new EventHandler(Isr_ISREnd);
            //    //    isr.Audio2TxtAsync(wav, null);
            //    //}
            //    isr.DataArrived += new EventHandler<iFlyISR.DataArrivedEventArgs>(asr_DataAvailable);
            //    isr.ISREnd += new EventHandler(Isr_ISREnd);
            //    isr.Audio2TxtAsync("wav", null);
            //    isr.DataArrived -= new EventHandler<iFlyISR.DataArrivedEventArgs>(asr_DataAvailable);
            //    isr.ISREnd -= new EventHandler(Isr_ISREnd);
            //}
            string c1 = "server_url=dev.voicecloud.cn,appid=51fc7487,timeout=10000";
            string c2 = "sub=iat,ssm=1,auf=audio/L16;rate=16000,aue=speex,ent=sms16k,rst=plain";
            iFlyISR isr = new iFlyISR(c1, c2);
            isr.DataArrived += new EventHandler<iFlyISR.DataArrivedEventArgs>(asr_DataAvailable);
            isr.ISREnd += new EventHandler(Isr_ISREnd);
            isr.Audio2TxtAsync(OutputFileName, null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string ID = "zyx";
            string ScheduleFileName;
            ScheduleFileName = System.IO.Directory.GetCurrentDirectory();
            ScheduleFileName = @ScheduleFileName + "\\" + ID + ".txt";
            StreamWriter sw = File.AppendText(ScheduleFileName);
            sw.WriteLine(result.Text);
            sw.WriteLine("");
            sw.Close();
            new eventrecord.MainWindow().Show();
            this.Close();
          
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            isRecoding = !isRecoding;

            if (isRecoding)
                buttonStart.Content = "Stop Recording";
            else
                buttonStart.Content = "Resume Recording";

            if (isRecoding)
            {
                audioSource = sensor.AudioSource;
                AudioRecord();
            }
           
        }
    }
}
