// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TexturedFaceMeshViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FaceTracking3D
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
        #region standarddeviation
        public  float HeadHeightStandardDeviation = 0;
        public  float NoseHeightStandardDeviation = 0;
        public  float LeftEyeWidthStandardDeviation = 0;
        public  float RightEyeHeightStandardDeviation = 0;
        public  float RightEyeWidthStandardDeviation = 0;
        public  float LeftEyeHeightStandardDeviation = 0;
        public  float MouthWidthStandardDeviation = 0;
        public  float HeadWidthStandardDeviation = 0;
        public  float ChinWidthStandardDeviation = 0;
        public  float OutEyeWidthStandardDeviation = 0;
        public  float LeftCheekMouthLengthStandardDeviation = 0;
        public  float RightCheekMouthLengthStandardDeviation = 0;
        public  float ChinHeightStandardDeviation = 0;
        public  float InEyeWidthStandardDeviation = 0;
        public  float GoldenTriangleDegreeStandardDeviation = 0;
        public  float SilverTriangleDegreeStandardDeviation = 0;
        public  float BronzeTriangleDegreeStandardDeviation = 0;
        #endregion standarddeviation
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

        //public float[,,] data;
        public static float[,,] data=new float[1000,121, 3];
        public int dataindex=0;
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

                        if (faceTrackFrame.TrackSuccessful && button1.Content.ToString()=="Collecting")
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

        public float GetDistance(int i,int p1,int p2)
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

        public float GetStandardDeviation(float mean,int p1,int p2)
        {
            float sum = 0;
            float deviation = 0;
            for (int i = 0; i <= dataindex; i++)
            {
                sum += (GetDistance(i,p1,p2) - mean) * (GetDistance(i,p1,p2) - mean);
            }
            deviation = (float)Math.Sqrt(sum / (dataindex + 1));
            return(deviation);
        }
        public float GetStandardDeviation(float mean, int p1, int p2, int p3, int p4)
        {
            float sum = 0;
            float deviation = 0;
            for (int i = 0; i <= dataindex; i++)
            {
                sum +=(float)Math.Pow((GetVectorDegree(data[i, p1, 0] - data[i, p2, 0], data[i, p1, 1] - data[i, p2, 1], data[i, p1, 2] - data[i, p2, 2], data[i, p3, 0] - data[i, p4, 0], data[i, p3, 1] - data[i, p4, 1], data[i, p3, 2] - data[i, p4, 2]))-mean,2);
            }
            deviation = (float)Math.Sqrt(sum / (dataindex + 1));
            return (deviation);
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
                data[dataindex,pointIndex, 0] = point.X;
                data[dataindex,pointIndex, 1] = point.Y;
                data[dataindex,pointIndex, 2] = point.Z;
                this.theGeometry.TextureCoordinates[pointIndex] =
                    new Point(
                        projected.X / (double)this.colorImageWritableBitmap.PixelWidth,
                        projected.Y / (double)this.colorImageWritableBitmap.PixelHeight);
            }
            distance.Text = "Distance is" + data[dataindex, 4, 2].ToString();
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
            if (button1.Content.ToString() == "START COLLECTION")
            {
                button1.Content = "Collecting";
                textBox1.Text = "0";
                textBox2.Text = "0";
                textBox3.Text = "0";
                textBox4.Text = "0";
                textBox5.Text = "0";
                textBox6.Text = "0";
                textBox7.Text = "0";
                textBox8.Text = "0";
                textBox9.Text = "0";
                textBox10.Text = "0";
                textBox11.Text = "0";
                textBox12.Text = "0";
                textBox13.Text = "0";
                textBox14.Text = "0";
                textBox15.Text = "0";
                textBox16.Text = "0";
                textBox18.Text = "0";
            }
            else if (button1.Content.ToString() == "Collecting")
            {
                button1.Content = "Collected";
            }
            else if (button1.Content.ToString() == "Collected")
            {
                button1.Content = "START COLLECTION";
                dataindex = 0;
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (button1.Content.ToString() == "Collected")
            {
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
                    LeftEyeWidth += GetDistance(i, 56, 53); 
                    RightEyeHeight += GetDistance(i, 22, 21); 
                    RightEyeWidth += GetDistance(i, 20, 23); 
                    LeftEyeHeight += GetDistance(i, 54, 55); 
                    MouthWidth += GetDistance(i, 88, 89); 
                    HeadWidth += GetDistance(i, 117, 113); 
                    ChinWidth += GetDistance(i, 30, 63); 
                    OutEyeWidth += GetDistance(i, 20, 53); 
                    LeftCheekMouthLength += GetDistance(i, 88, 91);
                    RightCheekMouthLength += GetDistance(i, 89, 90); 
                    ChinHeight += GetDistance(i, 9, 10); 
                    InEyeWidth += GetDistance(i, 23, 56);
                    GoldenTriangleDegree += GetVectorDegree(data[i, 26, 0] - data[i, 39, 0], data[i, 26, 1] - data[i, 39, 1], data[i, 26, 2] - data[i, 39, 2], data[i, 59, 0] - data[i, 39, 0], data[i, 59, 1] - data[i, 39, 1], data[i, 59, 2] - data[i, 39, 2]);
                    SilverTriangleDegree += GetVectorDegree(data[i, 20, 0] - data[i, 23, 0], data[i, 20, 1] - data[i, 23, 1], data[i, 20, 2] - data[i, 23, 2], data[i, 53, 0] - data[i, 56, 0], data[i, 53, 1] - data[i, 56, 1], data[i, 53, 2] - data[i, 56, 2]);
                    BronzeTriangleDegree += GetVectorDegree(data[i, 10, 0] - data[i, 32, 0], data[i, 10, 1] - data[i, 32, 1], data[i, 10, 2] - data[i, 32, 2], data[i, 65, 0] - data[i, 10, 0], data[i, 65, 1] - data[i, 10, 1], data[i, 65, 2] - data[i, 10, 2]);
                    //string fileName = "e:\\rawdata" + m +".txt";
                    //StreamWriter uw = File.AppendText(fileName);
                    //uw.Write(
                    //    "dataindex=" + dataindex + ";" +
                    //    "HeadHeight:" +
                    //         HeadHeight + "                            " +
                    //    "HeadWidth:" +
                    //         HeadWidth + "                            " +
                    //    "InEyeWidth:" +
                    //         InEyeWidth + "                            " +
                    //         "OutEyeWidth:" +
                    //         OutEyeWidth + "                            " +
                    //    "NoseHeight:" +
                    //         NoseHeight + "                            " +
                    //    "LeftEyeWidth:" +
                    //         LeftEyeWidth + "                            " +
                    //    "LeftEyeHeight:" +
                    //         LeftEyeHeight + "                            " +
                    //    "RightEyeWidth:" +
                    //         RightEyeWidth + "                            " +
                    //    "RightEyeHeight:" +
                    //         RightEyeHeight + "                            " +
                    //    "LeftCheekMouthLength:" +
                    //         LeftCheekMouthLength + "                            " +
                    //         "RightCheekMouthLength:" +
                    //         RightCheekMouthLength + "                            " +
                    //         "ChinHeight:" +
                    //         ChinHeight + "                            " +
                    //    "ChinWidth:" +
                    //         ChinWidth + "                            " +
                    //         "GoldenTriangleDegree:" +
                    //         GoldenTriangleDegree + "                          " +
                    //         "SilverTriangleDegree:" +
                    //         SilverTriangleDegree + "                            ");
                    //uw.Close();
                    //StreamWriter fw = File.AppendText(fileName);
                    //fw.Write("and & and");
                    //fw.Close();
                    
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

                 HeadHeightStandardDeviation = GetStandardDeviation(HeadHeightMeanValue,0,10);
                 NoseHeightStandardDeviation = GetStandardDeviation(NoseHeightMeanValue,94,39);
                 LeftEyeWidthStandardDeviation = GetStandardDeviation(LeftEyeWidthMeanValue,56,53 );
                 RightEyeHeightStandardDeviation = GetStandardDeviation(RightEyeHeightMeanValue,22,21);
                 RightEyeWidthStandardDeviation = GetStandardDeviation(RightEyeWidthMeanValue,20,23);
                 LeftEyeHeightStandardDeviation = GetStandardDeviation(LeftEyeHeightMeanValue,54,55);
                 MouthWidthStandardDeviation = GetStandardDeviation(MouthWidthMeanValue,88,89);
                 HeadWidthStandardDeviation = GetStandardDeviation(HeadWidthMeanValue,117,113);
                 ChinWidthStandardDeviation = GetStandardDeviation(ChinWidthMeanValue,30,63);
                 OutEyeWidthStandardDeviation = GetStandardDeviation(OutEyeWidthMeanValue,20,53);
                 LeftCheekMouthLengthStandardDeviation = GetStandardDeviation(LeftCheekMouthLengthMeanValue,88,91);
                 RightCheekMouthLengthStandardDeviation = GetStandardDeviation(RightCheekMouthLengthMeanValue,89,90);
                 ChinHeightStandardDeviation = GetStandardDeviation(ChinHeightMeanValue,9,10);
                 InEyeWidthStandardDeviation = GetStandardDeviation(InEyeWidthMeanValue,23,56);
                 GoldenTriangleDegreeStandardDeviation = GetStandardDeviation(GoldenTriangleDegreeMeanValue,26,39,59,39);
                 SilverTriangleDegreeStandardDeviation = GetStandardDeviation(SilverTriangleDegreeMeanValue,20,23,53,56);
                 BronzeTriangleDegreeStandardDeviation = GetStandardDeviation(BronzeTriangleDegreeMeanValue,10,32,65,10);

                textBox1.Text = (HeadHeightMeanValue).ToString() +"+-" + HeadHeightStandardDeviation.ToString();
                textBox2.Text = (NoseHeightMeanValue).ToString() + "+-" + NoseHeightStandardDeviation.ToString();
                textBox3.Text = (LeftEyeWidthMeanValue).ToString() + "+-" + LeftEyeWidthStandardDeviation.ToString();
                textBox4.Text = (RightEyeHeightMeanValue).ToString() + "+-" + RightEyeHeightStandardDeviation.ToString();
                textBox5.Text = (RightEyeWidthMeanValue).ToString() + "+-" + RightEyeWidthStandardDeviation.ToString();
                textBox6.Text = (LeftEyeHeightMeanValue).ToString() + "+-" + LeftEyeHeightStandardDeviation.ToString();
                textBox7.Text = (MouthWidthMeanValue).ToString() + "+-" + MouthWidthStandardDeviation.ToString();
                textBox8.Text = (HeadWidthMeanValue).ToString() + "+-" + HeadWidthStandardDeviation.ToString();
                textBox9.Text = (ChinWidthMeanValue).ToString() + "+-" + ChinWidthStandardDeviation.ToString();
                textBox10.Text = (OutEyeWidthMeanValue).ToString() + "+-" + OutEyeWidthStandardDeviation.ToString();
                textBox11.Text = (LeftCheekMouthLengthMeanValue).ToString() + "+-" + LeftCheekMouthLengthStandardDeviation.ToString();
                textBox12.Text = (RightCheekMouthLengthMeanValue).ToString() + "+-" + RightCheekMouthLengthStandardDeviation.ToString();
                textBox13.Text = (ChinHeightMeanValue).ToString() + "+-" + ChinHeightStandardDeviation.ToString();
                textBox14.Text = (InEyeWidthMeanValue).ToString() + "+-" + InEyeWidthStandardDeviation.ToString();
                textBox15.Text = (GoldenTriangleDegreeMeanValue).ToString() + "+-" + GoldenTriangleDegreeStandardDeviation.ToString();
                textBox16.Text = (SilverTriangleDegreeMeanValue).ToString() + "+-" + SilverTriangleDegreeStandardDeviation.ToString();
                textBox18.Text = (BronzeTriangleDegreeMeanValue).ToString() + "+-" + BronzeTriangleDegreeStandardDeviation.ToString();
                textBox17.Visibility = Visibility.Visible;
                textBlock17.Visibility = Visibility.Visible;
                button3.Visibility = Visibility.Visible;



            }
            button2.Content = "Calculated";
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            string dataName = textBox17.Text.ToString();
            string fileName = "e:\\Faceof"+dataName+".txt";
            StreamWriter uw = File.AppendText(fileName);   
            //uw.WriteLine("Name:{0}",dataName);
            uw.WriteLine("{0},{1}", (HeadHeightMeanValue - 3 * HeadHeightStandardDeviation).ToString("0.00000000"), (HeadHeightMeanValue + 3 * HeadHeightStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (NoseHeightMeanValue - 3 * NoseHeightStandardDeviation).ToString("0.00000000"), (NoseHeightMeanValue + 3 * NoseHeightStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (LeftEyeWidthMeanValue - 3 * LeftEyeWidthStandardDeviation).ToString("0.00000000"), (LeftEyeWidthMeanValue + 3 * LeftEyeWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (RightEyeHeightMeanValue - 3 * RightEyeHeightStandardDeviation).ToString("0.00000000"), (RightEyeHeightMeanValue + 3 * RightEyeHeightStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (RightEyeWidthMeanValue - 3 * RightEyeWidthStandardDeviation).ToString("0.00000000"), (RightEyeWidthMeanValue + 3 * RightEyeWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (LeftEyeHeightMeanValue - 3 * LeftEyeHeightStandardDeviation).ToString("0.00000000"), (LeftEyeHeightMeanValue + 3 * LeftEyeHeightStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (MouthWidthMeanValue - 3 * MouthWidthStandardDeviation).ToString("0.00000000"), (MouthWidthMeanValue + 3 * MouthWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (HeadWidthMeanValue - 3 * HeadWidthStandardDeviation).ToString("0.00000000"), (HeadWidthMeanValue + 3 * HeadWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (ChinWidthMeanValue - 3 * ChinWidthStandardDeviation).ToString("0.00000000"), (ChinWidthMeanValue + 3 * ChinWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (OutEyeWidthMeanValue - 3 * OutEyeWidthStandardDeviation).ToString("0.00000000"), (OutEyeWidthMeanValue + 3 * OutEyeWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (LeftCheekMouthLengthMeanValue - 3 * LeftCheekMouthLengthStandardDeviation).ToString("0.00000000"), (LeftCheekMouthLengthMeanValue + 3 * LeftCheekMouthLengthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (RightCheekMouthLengthMeanValue - 3 * RightCheekMouthLengthStandardDeviation).ToString("0.00000000"), (RightCheekMouthLengthMeanValue + 3 * RightCheekMouthLengthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (ChinHeightMeanValue - 3 * ChinHeightStandardDeviation).ToString("0.00000000"), (ChinHeightMeanValue + 3 * ChinHeightStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (InEyeWidthMeanValue - 3 * InEyeWidthStandardDeviation).ToString("0.00000000"), (InEyeWidthMeanValue + 3 * InEyeWidthStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (GoldenTriangleDegreeMeanValue - 3 * GoldenTriangleDegreeStandardDeviation).ToString("0.00000000"), (GoldenTriangleDegreeMeanValue + 3 * GoldenTriangleDegreeStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (SilverTriangleDegreeMeanValue - 3 * SilverTriangleDegreeStandardDeviation).ToString("0.00000000"), (SilverTriangleDegreeMeanValue + 3 * SilverTriangleDegreeStandardDeviation).ToString("0.00000000"));
            uw.WriteLine("{0},{1}", (BronzeTriangleDegreeMeanValue - 3 * BronzeTriangleDegreeStandardDeviation).ToString("0.00000000"), (BronzeTriangleDegreeMeanValue + 3 * BronzeTriangleDegreeStandardDeviation).ToString("0.00000000"));  

            uw.Close();
            MessageBox.Show("Successfully saved!","Button Message");
            button1.Content = "START COLLECTION";
            button2.Content = "Calculation";
            textBox17.Visibility = Visibility.Hidden;
            textBlock17.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
            dataindex = 0;
            m++;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            button1.Content = "START COLLECTION";
            button2.Content = "Calculation";
            dataindex = 0;
            textBox17.Visibility = Visibility.Hidden;
            textBlock17.Visibility = Visibility.Hidden;
            button3.Visibility = Visibility.Hidden;
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox12.Text = "";
            textBox13.Text = "";
            textBox14.Text = "";
            textBox15.Text = "";
            textBox16.Text = "";
            textBox18.Text = ""; 
            Array.Clear(data, 0, data.Length);
        }
        //public void MailSender()
        //{
        //    string mailSender, mailReciever;
        //    string mailContent, senderPassword;
        //    string mailSubject;
        //    string server = "202.38.64.8";//USTC server
        //    mailSender = "gjc1994@mail.ustc.edu.cn";
        //    mailReciever =  "gjc1994@mail.ustc.edu.cn";
        //    mailContent =   "Help!";
        //    senderPassword = "zyq1994";
        //    mailSubject = "HELP SOS";

        //    MailAddress mailFrom = new MailAddress(mailSender);
        //    MailAddress mailTo = new MailAddress(mailReciever);
        //    MailMessage myMail = new MailMessage(mailFrom, mailTo);

        //    myMail.Subject = mailSubject;
        //    myMail.Body = mailContent;
        //    myMail.BodyEncoding = System.Text.Encoding.UTF8;
        //    myMail.IsBodyHtml = true;
        //    myMail.Priority = MailPriority.High;
        //    string file = "";
        //    bool isSended = true;
        //    Attachment data = new Attachment(file, MediaTypeNames.Image.Jpeg);
        //    myMail.Attachments.Add(data);
        //    SmtpClient client = new SmtpClient(server);
        //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
        //    client.Credentials = new System.Net.NetworkCredential(mailSender, senderPassword);
        //    try
        //    {
        //        client.Send(myMail);
        //    }
        //    catch
        //    {
        //        isSended = false;
        //    }

        //    if (isSended)
        //    {
        //        MessageBox.Show("Success!","Message");
        //    }
        //    else
        //    {
        //        MessageBox.Show("Fail To Send Mail!", "Message");
        //    }
        //}
       

    }
}