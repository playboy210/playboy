// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TexturedFaceMeshViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Monitor
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.IO;
    using System.Text;
    using Point = System.Windows.Point;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Globalization;
    /// <summary>
    /// Interaction logic for TexturedFaceMeshViewer.xaml
    /// </summary>
    public partial class TexturedFaceMeshViewer : UserControl, IDisposable
    {
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect",
            typeof(KinectSensor),
            typeof(TexturedFaceMeshViewer),
            new UIPropertyMetadata(
                null,
                (o, args) =>
                ((TexturedFaceMeshViewer)o).OnKinectChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));
        #region meanvalue
        float HeadHeightMeanValue = 0;
        float NoseHeightMeanValue = 0;
        float LeftEyeWidthMeanValue = 0;
        float RightEyeHeightMeanValue = 0;
        float RightEyeWidthMeanValue = 0;
        float LeftEyeHeightMeanValue = 0;
        float MouthWidthMeanValue = 0;
        float HeadWidthMeanValue = 0;
        float ChinWidthMeanValue = 0;
        float OutEyeWidthMeanValue = 0;
        float LeftCheekMouthLengthMeanValue = 0;
        float RightCheekMouthLengthMeanValue = 0;
        float ChinHeightMeanValue = 0;
        float InEyeWidthMeanValue = 0;
        float GoldenTriangleDegreeMeanValue = 0;
        float SilverTriangleDegreeMeanValue = 0;
        float BronzeTriangleDegreeMeanValue = 0;
        #endregion meanvalue
        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        private byte[] colorImage;

        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;

        private WriteableBitmap colorImageWritableBitmap;

        private short[] depthImage;

        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        private FaceTracker faceTracker;

        private Skeleton[] skeletonData;

        private int trackingId = -1;
        public int m = 0;
        private FaceTriangle[] triangleIndices;
        public int SCORE = 0;
        //public float[,,] data;
        public static float[, ,] data = new float[1000, 121, 3];
        public int dataindex = 0;
        public string path;
        public TexturedFaceMeshViewer()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public KinectSensor Kinect
        {
            get
            {
                return (KinectSensor)this.GetValue(KinectProperty);
            }

            set
            {
                this.SetValue(KinectProperty, value);
            }
        }

        public void Dispose()
        {
            this.DestroyFaceTracker();
        }

        private void AllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;


            try
            {
                colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
                depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
                skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for changes in any of the data this function is receiving
                // and reset things appropriately.
                if (this.depthImageFormat != depthImageFrame.Format)
                {
                    this.DestroyFaceTracker();
                    this.depthImage = null;
                    this.depthImageFormat = depthImageFrame.Format;
                }

                if (this.colorImageFormat != colorImageFrame.Format)
                {
                    this.DestroyFaceTracker();
                    this.colorImage = null;
                    this.colorImageFormat = colorImageFrame.Format;
                    this.colorImageWritableBitmap = null;
                    this.ColorImage.Source = null;
                    this.theMaterial.Brush = null;
                }

                if (this.skeletonData != null && this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    this.skeletonData = null;
                }

                // Create any buffers to store copies of the data we work with
                if (this.depthImage == null)
                {
                    this.depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (this.colorImage == null)
                {
                    this.colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                if (this.colorImageWritableBitmap == null)
                {
                    this.colorImageWritableBitmap = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    this.ColorImage.Source = this.colorImageWritableBitmap;
                    this.theMaterial.Brush = new ImageBrush(this.colorImageWritableBitmap)
                        {
                            ViewportUnits = BrushMappingMode.Absolute
                        };
                }

                if (this.skeletonData == null)
                {
                    this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                // Copy data received in this event to our buffers.
                colorImageFrame.CopyPixelDataTo(this.colorImage);
                depthImageFrame.CopyPixelDataTo(this.depthImage);
                skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                this.colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImage,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);

                // Find a skeleton to track.
                // First see if our old one is good.
                // When a skeleton is in PositionOnly tracking state, don't pick a new one
                // as it may become fully tracked again.
                Skeleton skeletonOfInterest =
                    this.skeletonData.FirstOrDefault(
                        skeleton =>
                        skeleton.TrackingId == this.trackingId
                        && skeleton.TrackingState != SkeletonTrackingState.NotTracked);

                if (skeletonOfInterest == null)
                {
                    // Old one wasn't around.  Find any skeleton that is being tracked and use it.
                    skeletonOfInterest =
                        this.skeletonData.FirstOrDefault(
                            skeleton => skeleton.TrackingState == SkeletonTrackingState.Tracked);

                    if (skeletonOfInterest != null)
                    {
                        // This may be a different person so reset the tracker which
                        // could have tuned itself to the previous person.
                        if (this.faceTracker != null)
                        {
                            this.faceTracker.ResetTracking();
                        }

                        this.trackingId = skeletonOfInterest.TrackingId;
                    }
                }

                bool displayFaceMesh = false;

                if (skeletonOfInterest != null && skeletonOfInterest.TrackingState == SkeletonTrackingState.Tracked)
                {
                    if (this.faceTracker == null)
                    {
                        try
                        {
                            this.faceTracker = new FaceTracker(this.Kinect);
                        }
                        catch (InvalidOperationException)
                        {
                            // During some shutdown scenarios the FaceTracker
                            // is unable to be instantiated.  Catch that exception
                            // and don't track a face.
                            Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                            this.faceTracker = null;
                        }
                    }

                    if (this.faceTracker != null)
                    {
                        FaceTrackFrame faceTrackFrame = this.faceTracker.Track(
                            this.colorImageFormat,
                            this.colorImage,
                            this.depthImageFormat,
                            this.depthImage,
                            skeletonOfInterest);

                        if (faceTrackFrame.TrackSuccessful && status.Text.ToString() == "STATUS:MONITORING")
                        {
                            this.UpdateMesh(faceTrackFrame);

                            // Only display the face mesh if there was a successful track.
                            displayFaceMesh = true;
                        }
                    }
                }
                else
                {
                    this.trackingId = -1;
                }

                this.viewport3d.Visibility = displayFaceMesh ? Visibility.Visible : Visibility.Hidden;
            }
            finally
            {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }

                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }

            try
            {
                    kinectRegion.KinectSensor = Kinect;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DestroyFaceTracker()
        {
            if (this.faceTracker != null)
            {
                this.faceTracker.Dispose();
                this.faceTracker = null;
            }
        }

        private void OnKinectChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                try
                {
                    oldSensor.AllFramesReady -= this.AllFramesReady;

                    this.DestroyFaceTracker();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (newSensor != null)
            {
                try
                {
                    this.faceTracker = new FaceTracker(this.Kinect);

                    newSensor.AllFramesReady += this.AllFramesReady;
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }
        public float GetVectorDegree(float ax, float ay, float az, float bx, float by, float bz)
        {
            float cosDegree = 0;
            cosDegree = Math.Abs(ax * bx + ay * by + az * bz) / (((float)Math.Sqrt(ax * ax + ay * ay + az * az) * (float)Math.Sqrt(bx * bx + by * by + bz * bz)));
            return (cosDegree);
        }

        public float GetDistance(int i, int p1, int p2)
        {
            float distance;
            distance = (float)Math.Pow(Math.Pow(data[i, p1, 0] - data[i, p2, 0], 2) + Math.Pow(data[i, p1, 1] - data[i, p2, 1], 2) + Math.Pow(data[i, p1, 2] - data[i, p2, 2], 2), 0.5);
            return (distance);
        }
        public float MeanValue(float a, float b)
        {
            float MeanValue = a / b;
            return (MeanValue);
        }

        void UpdateMesh(FaceTrackFrame faceTrackingFrame)
        {
            EnumIndexableCollection<FeaturePoint, Vector3DF> shapePoints = faceTrackingFrame.Get3DShape();
            EnumIndexableCollection<FeaturePoint, PointF> projectedShapePoints = faceTrackingFrame.GetProjected3DShape();

            if (this.triangleIndices == null)
            {
                // Update stuff that doesn't change from frame to frame
                this.triangleIndices = faceTrackingFrame.GetTriangles();
                var indices = new Int32Collection(this.triangleIndices.Length * 3);
                foreach (FaceTriangle triangle in this.triangleIndices)
                {
                    indices.Add(triangle.Third);
                    indices.Add(triangle.Second);
                    indices.Add(triangle.First);
                }

                this.theGeometry.TriangleIndices = indices;
                this.theGeometry.Normals = null; // Let WPF3D calculate these.

                this.theGeometry.Positions = new Point3DCollection(shapePoints.Count);
                this.theGeometry.TextureCoordinates = new PointCollection(projectedShapePoints.Count);
                for (int pointIndex = 0; pointIndex < shapePoints.Count; pointIndex++)
                {
                    this.theGeometry.Positions.Add(new Point3D());
                    this.theGeometry.TextureCoordinates.Add(new Point());
                }
            }
            //Vector3DF point=shapePoints[3];
            // Update the 3D model's vertices and texture coordinates

            for (int pointIndex = 0; pointIndex < shapePoints.Count; pointIndex++)
            {
                Vector3DF point = shapePoints[pointIndex];
                this.theGeometry.Positions[pointIndex] = new Point3D(point.X, point.Y, -point.Z);

                PointF projected = projectedShapePoints[pointIndex];
                data[dataindex, pointIndex, 0] = point.X;
                data[dataindex, pointIndex, 1] = point.Y;
                data[dataindex, pointIndex, 2] = point.Z;
                this.theGeometry.TextureCoordinates[pointIndex] =
                    new Point(
                        projected.X / (double)this.colorImageWritableBitmap.PixelWidth,
                        projected.Y / (double)this.colorImageWritableBitmap.PixelHeight);
            }
            textBlock1.Text = data[dataindex, 4, 2].ToString();
            if (data[dataindex, 4, 2] > 1.2 && data[dataindex, 4, 2] < 1.4)
            {
                dataindex++;
                //StreamWriter fw = File.AppendText("e:\\newnewstart4.txt");
                //fw.Write("z=" + );
                //fw.Close();
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (status.Text.ToString() == "STATUS:")
                status.Text = "STATUS:MONITORING";
            else if (status.Text.ToString() == "STATUS:MONITORING")
            {
                status.Text = "STATUS:";
                float HeadHeight = 0;
                float NoseHeight = 0;
                float LeftEyeWidth = 0;
                float RightEyeHeight = 0;
                float RightEyeWidth = 0;
                float LeftEyeHeight = 0;
                float MouthWidth = 0;
                float HeadWidth = 0;
                float ChinWidth = 0;
                float OutEyeWidth = 0;
                float LeftCheekMouthLength = 0;
                float RightCheekMouthLength = 0;
                float ChinHeight = 0;
                float InEyeWidth = 0;
                float GoldenTriangleDegree = 0;
                float SilverTriangleDegree = 0;
                float BronzeTriangleDegree = 0;

                dataindex--;

                for (int i = 0; i < this.dataindex; i++)
                {
                    HeadHeight += GetDistance(i, 0, 10);
                    NoseHeight += GetDistance(i, 94, 39);
                    LeftEyeWidth += GetDistance(i, 56, 53); ;
                    RightEyeHeight += GetDistance(i, 22, 21); ;
                    RightEyeWidth += GetDistance(i, 20, 23); ;
                    LeftEyeHeight += GetDistance(i, 54, 55); ;
                    MouthWidth += GetDistance(i, 88, 89); ;
                    HeadWidth += GetDistance(i, 117, 113); ;
                    ChinWidth += GetDistance(i, 30, 63); ;
                    OutEyeWidth += GetDistance(i, 20, 53); ;
                    LeftCheekMouthLength += GetDistance(i, 88, 91); ;
                    RightCheekMouthLength += GetDistance(i, 89, 90); ;
                    ChinHeight += GetDistance(i, 9, 10); ;
                    InEyeWidth += GetDistance(i, 23, 56); ;
                    GoldenTriangleDegree += GetVectorDegree(data[i, 26, 0] - data[i, 39, 0], data[i, 26, 1] - data[i, 39, 1], data[i, 26, 2] - data[i, 39, 2], data[i, 59, 0] - data[i, 39, 0], data[i, 59, 1] - data[i, 39, 1], data[i, 59, 2] - data[i, 39, 2]);
                    SilverTriangleDegree += GetVectorDegree(data[i, 20, 0] - data[i, 23, 0], data[i, 20, 1] - data[i, 23, 1], data[i, 20, 2] - data[i, 23, 2], data[i, 53, 0] - data[i, 56, 0], data[i, 53, 1] - data[i, 56, 1], data[i, 53, 2] - data[i, 56, 2]);
                    BronzeTriangleDegree += GetVectorDegree(data[i, 10, 0] - data[i, 32, 0], data[i, 10, 1] - data[i, 32, 1], data[i, 10, 2] - data[i, 32, 2], data[i, 65, 0] - data[i, 10, 0], data[i, 65, 1] - data[i, 10, 1], data[i, 65, 2] - data[i, 10, 2]);
                }
                string fileName = "e:\\Faceofzxg.txt";
                double[,] facebase = new double[17, 2];
                using (StreamReader sr = new StreamReader(fileName))
                {

                    for (int j = 0; j < 17; j++)
                    {
                        string line, st1, st2;
                        line = sr.ReadLine();
                        st1 = line.Remove(10, 11);
                        st2 = line.Remove(0, 11);
                        facebase[j, 0] = System.Convert.ToDouble(st1);
                        facebase[j, 1] = System.Convert.ToDouble(st2);
                    }
                }

                HeadHeightMeanValue = MeanValue(HeadHeight, dataindex);
                NoseHeightMeanValue = MeanValue(NoseHeight, dataindex);
                LeftEyeWidthMeanValue = MeanValue(LeftEyeWidth, dataindex);
                RightEyeHeightMeanValue = MeanValue(RightEyeHeight, dataindex);
                RightEyeWidthMeanValue = MeanValue(RightEyeWidth, dataindex);
                LeftEyeHeightMeanValue = MeanValue(LeftEyeHeight, dataindex);
                MouthWidthMeanValue = MeanValue(MouthWidth, dataindex);
                HeadWidthMeanValue = MeanValue(HeadWidth, dataindex);
                ChinWidthMeanValue = MeanValue(ChinWidth, dataindex);
                OutEyeWidthMeanValue = MeanValue(OutEyeWidth, dataindex);
                LeftCheekMouthLengthMeanValue = MeanValue(LeftCheekMouthLength, dataindex);
                RightCheekMouthLengthMeanValue = MeanValue(RightCheekMouthLength, dataindex);
                ChinHeightMeanValue = MeanValue(ChinHeight, dataindex);
                InEyeWidthMeanValue = MeanValue(InEyeWidth, dataindex);
                GoldenTriangleDegreeMeanValue = MeanValue(GoldenTriangleDegree, dataindex);
                SilverTriangleDegreeMeanValue = MeanValue(SilverTriangleDegree, dataindex);
                BronzeTriangleDegreeMeanValue = MeanValue(BronzeTriangleDegree, dataindex);
                #region 评分
                string score = "";
                if (HeadHeightMeanValue > facebase[0, 0] && HeadHeightMeanValue < facebase[0, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (NoseHeightMeanValue > facebase[1, 0] && NoseHeightMeanValue < facebase[1, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (LeftEyeWidthMeanValue > facebase[2, 0] && NoseHeightMeanValue < facebase[2, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (RightEyeHeightMeanValue > facebase[3, 0] && NoseHeightMeanValue < facebase[3, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (RightEyeWidthMeanValue > facebase[4, 0] && NoseHeightMeanValue < facebase[4, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (LeftEyeHeightMeanValue > facebase[5, 0] && NoseHeightMeanValue < facebase[5, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (MouthWidthMeanValue > facebase[6, 0] && NoseHeightMeanValue < facebase[6, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (HeadWidthMeanValue > facebase[7, 0] && NoseHeightMeanValue < facebase[7, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (ChinWidthMeanValue > facebase[8, 0] && NoseHeightMeanValue < facebase[8, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (OutEyeWidthMeanValue > facebase[9, 0] && NoseHeightMeanValue < facebase[9, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (LeftCheekMouthLengthMeanValue > facebase[10, 0] && NoseHeightMeanValue < facebase[10, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }

                if (RightCheekMouthLengthMeanValue > facebase[11, 0] && NoseHeightMeanValue < facebase[11, 1])
                {
                    SCORE++;
                    score += "1";

                }
                else { score += "0"; }
                if (ChinHeightMeanValue > facebase[12, 0] && NoseHeightMeanValue < facebase[12, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (InEyeWidthMeanValue > facebase[13, 0] && NoseHeightMeanValue < facebase[13, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (GoldenTriangleDegreeMeanValue > facebase[14, 0] && NoseHeightMeanValue < facebase[14, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (SilverTriangleDegreeMeanValue > facebase[15, 0] && NoseHeightMeanValue < facebase[15, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (BronzeTriangleDegreeMeanValue > facebase[16, 0] && NoseHeightMeanValue < facebase[16, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                textBlock5.Text = score.ToString();
                if (SCORE >= 13)
                {
                    textBlock6.Text = "PASS";
                }
                else
                {
                    textBlock6.Text = "STRANGER";
                    ImageCapture();
                    MailSender();

                }
                SCORE = 0;
                #endregion 评分
            }
        }
        public void MailSender()
        {
            string mailSender, mailReciever;
            string mailContent, senderPassword;
            string mailSubject;
            string server = "202.38.64.8";//USTC server
            mailSender = "gjc1994@mail.ustc.edu.cn";
            mailReciever = "gjc1994@mail.ustc.edu.cn";
            mailContent = "Help!";
            senderPassword = "zyq1994";
            mailSubject = "HELP SOS";

            MailAddress mailFrom = new MailAddress(mailSender);
            MailAddress mailTo = new MailAddress(mailReciever);
            MailMessage myMail = new MailMessage(mailFrom, mailTo);

            myMail.Subject = mailSubject;
            myMail.Body = mailContent;
            myMail.BodyEncoding = System.Text.Encoding.UTF8;
            myMail.IsBodyHtml = true;
            myMail.Priority = MailPriority.High;
            string file = path;
            bool isSended = true;
            Attachment data = new Attachment(file, MediaTypeNames.Image.Jpeg);
            myMail.Attachments.Add(data);
            SmtpClient client = new SmtpClient(server);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new System.Net.NetworkCredential(mailSender, senderPassword);
            try
            {
                client.Send(myMail);
            }
            catch
            {
                isSended = false;
            }

            if (isSended)
            {
                MessageBox.Show("Success!", "Message");
            }
            else
            {
                MessageBox.Show("Fail To Send Mail!", "Message");
            }
        }
        public void ImageCapture()
        { 
        BitmapEncoder encoder = new JpegBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorImageWritableBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            path = Path.Combine(myPhotos, "Stranger-" + time + ".jpeg");

            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
            }
            catch (IOException)
            {   
            }
        }

        private void KinectTileButton_Click(object sender, RoutedEventArgs e)
        {
            if (status.Text.ToString() == "STATUS:")
                status.Text = "STATUS:MONITORING";
            else if (status.Text.ToString() == "STATUS:MONITORING")
            {
                status.Text = "STATUS:";
                float HeadHeight = 0;
                float NoseHeight = 0;
                float LeftEyeWidth = 0;
                float RightEyeHeight = 0;
                float RightEyeWidth = 0;
                float LeftEyeHeight = 0;
                float MouthWidth = 0;
                float HeadWidth = 0;
                float ChinWidth = 0;
                float OutEyeWidth = 0;
                float LeftCheekMouthLength = 0;
                float RightCheekMouthLength = 0;
                float ChinHeight = 0;
                float InEyeWidth = 0;
                float GoldenTriangleDegree = 0;
                float SilverTriangleDegree = 0;
                float BronzeTriangleDegree = 0;

                dataindex--;

                for (int i = 0; i < this.dataindex; i++)
                {
                    HeadHeight += GetDistance(i, 0, 10);
                    NoseHeight += GetDistance(i, 94, 39);
                    LeftEyeWidth += GetDistance(i, 56, 53); ;
                    RightEyeHeight += GetDistance(i, 22, 21); ;
                    RightEyeWidth += GetDistance(i, 20, 23); ;
                    LeftEyeHeight += GetDistance(i, 54, 55); ;
                    MouthWidth += GetDistance(i, 88, 89); ;
                    HeadWidth += GetDistance(i, 117, 113); ;
                    ChinWidth += GetDistance(i, 30, 63); ;
                    OutEyeWidth += GetDistance(i, 20, 53); ;
                    LeftCheekMouthLength += GetDistance(i, 88, 91); ;
                    RightCheekMouthLength += GetDistance(i, 89, 90); ;
                    ChinHeight += GetDistance(i, 9, 10); ;
                    InEyeWidth += GetDistance(i, 23, 56); ;
                    GoldenTriangleDegree += GetVectorDegree(data[i, 26, 0] - data[i, 39, 0], data[i, 26, 1] - data[i, 39, 1], data[i, 26, 2] - data[i, 39, 2], data[i, 59, 0] - data[i, 39, 0], data[i, 59, 1] - data[i, 39, 1], data[i, 59, 2] - data[i, 39, 2]);
                    SilverTriangleDegree += GetVectorDegree(data[i, 20, 0] - data[i, 23, 0], data[i, 20, 1] - data[i, 23, 1], data[i, 20, 2] - data[i, 23, 2], data[i, 53, 0] - data[i, 56, 0], data[i, 53, 1] - data[i, 56, 1], data[i, 53, 2] - data[i, 56, 2]);
                    BronzeTriangleDegree += GetVectorDegree(data[i, 10, 0] - data[i, 32, 0], data[i, 10, 1] - data[i, 32, 1], data[i, 10, 2] - data[i, 32, 2], data[i, 65, 0] - data[i, 10, 0], data[i, 65, 1] - data[i, 10, 1], data[i, 65, 2] - data[i, 10, 2]);
                }
                string fileName = "e:\\Faceofzxg.txt";
                double[,] facebase = new double[17, 2];
                using (StreamReader sr = new StreamReader(fileName))
                {

                    for (int j = 0; j < 17; j++)
                    {
                        string line, st1, st2;
                        line = sr.ReadLine();
                        st1 = line.Remove(10, 11);
                        st2 = line.Remove(0, 11);
                        facebase[j, 0] = System.Convert.ToDouble(st1);
                        facebase[j, 1] = System.Convert.ToDouble(st2);
                    }
                }

                HeadHeightMeanValue = MeanValue(HeadHeight, dataindex);
                NoseHeightMeanValue = MeanValue(NoseHeight, dataindex);
                LeftEyeWidthMeanValue = MeanValue(LeftEyeWidth, dataindex);
                RightEyeHeightMeanValue = MeanValue(RightEyeHeight, dataindex);
                RightEyeWidthMeanValue = MeanValue(RightEyeWidth, dataindex);
                LeftEyeHeightMeanValue = MeanValue(LeftEyeHeight, dataindex);
                MouthWidthMeanValue = MeanValue(MouthWidth, dataindex);
                HeadWidthMeanValue = MeanValue(HeadWidth, dataindex);
                ChinWidthMeanValue = MeanValue(ChinWidth, dataindex);
                OutEyeWidthMeanValue = MeanValue(OutEyeWidth, dataindex);
                LeftCheekMouthLengthMeanValue = MeanValue(LeftCheekMouthLength, dataindex);
                RightCheekMouthLengthMeanValue = MeanValue(RightCheekMouthLength, dataindex);
                ChinHeightMeanValue = MeanValue(ChinHeight, dataindex);
                InEyeWidthMeanValue = MeanValue(InEyeWidth, dataindex);
                GoldenTriangleDegreeMeanValue = MeanValue(GoldenTriangleDegree, dataindex);
                SilverTriangleDegreeMeanValue = MeanValue(SilverTriangleDegree, dataindex);
                BronzeTriangleDegreeMeanValue = MeanValue(BronzeTriangleDegree, dataindex);
                #region 评分
                string score = "";
                if (HeadHeightMeanValue > facebase[0, 0] && HeadHeightMeanValue < facebase[0, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (NoseHeightMeanValue > facebase[1, 0] && NoseHeightMeanValue < facebase[1, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (LeftEyeWidthMeanValue > facebase[2, 0] && NoseHeightMeanValue < facebase[2, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (RightEyeHeightMeanValue > facebase[3, 0] && NoseHeightMeanValue < facebase[3, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (RightEyeWidthMeanValue > facebase[4, 0] && NoseHeightMeanValue < facebase[4, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (LeftEyeHeightMeanValue > facebase[5, 0] && NoseHeightMeanValue < facebase[5, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (MouthWidthMeanValue > facebase[6, 0] && NoseHeightMeanValue < facebase[6, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (HeadWidthMeanValue > facebase[7, 0] && NoseHeightMeanValue < facebase[7, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (ChinWidthMeanValue > facebase[8, 0] && NoseHeightMeanValue < facebase[8, 1])
                {
                    score += "1";
                    SCORE++;
                }
                else { score += "0"; }
                if (OutEyeWidthMeanValue > facebase[9, 0] && NoseHeightMeanValue < facebase[9, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (LeftCheekMouthLengthMeanValue > facebase[10, 0] && NoseHeightMeanValue < facebase[10, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }

                if (RightCheekMouthLengthMeanValue > facebase[11, 0] && NoseHeightMeanValue < facebase[11, 1])
                {
                    SCORE++;
                    score += "1";

                }
                else { score += "0"; }
                if (ChinHeightMeanValue > facebase[12, 0] && NoseHeightMeanValue < facebase[12, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (InEyeWidthMeanValue > facebase[13, 0] && NoseHeightMeanValue < facebase[13, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (GoldenTriangleDegreeMeanValue > facebase[14, 0] && NoseHeightMeanValue < facebase[14, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (SilverTriangleDegreeMeanValue > facebase[15, 0] && NoseHeightMeanValue < facebase[15, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                if (BronzeTriangleDegreeMeanValue > facebase[16, 0] && NoseHeightMeanValue < facebase[16, 1])
                {
                    score += "1";
                    SCORE++;

                }
                else { score += "0"; }
                textBlock5.Text = score.ToString();
                if (SCORE >= 13)
                {
                    textBlock6.Text = "PASS";
                }
                else
                {
                    textBlock6.Text = "STRANGER";
                    ImageCapture();
                    MailSender();

                }
                SCORE = 0;
                #endregion 评分
            }
        }
        
        
    }
}