//2016-03-14 byLSS

namespace KinectPPT_byLSS
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Windows.Forms;
    //for SendKeys and DoEvents

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 
        /// </summary>
        bool COLORFRAME = false;
        const float LIFT = 0.15f;
        const int LOCKED = 0;
        const int CONTROLLING = 1;
        const int SELECTING = 2;
        const int TOESC = 3;
        const int TOPLAY = 4;
        const int TOSTART = 5;
        const int TOHIDE = 6;
        string[] mode = { "locked", "controlling", "selecting", "toESC", "toPlay", "toStart", "toHide" };
        bool nextpage = false;
        bool prevpage = false;
        bool hiding = false;
        bool outside = false;
        int State = LOCKED;
        int itab = 0;
        #region joints definition
        public const int Jointnum = 25;
        public const int Spinebase = 0;
        public const int Spinemid = 1;
        public const int Neck = 2;
        public const int Head = 3;
        public const int Shoulderleft = 4;
        public const int Elbowleft = 5;
        public const int Wristleft = 6;
        public const int Handleft = 7;
        public const int Shoulderright = 8;
        public const int Elbowright = 9;
        public const int Wristright = 10;
        public const int Handright = 11;
        public const int Hipleft = 12;
        public const int Kneeleft = 13;
        public const int Ankleleft = 14;
        public const int Footleft = 15;
        public const int Hipright = 16;
        public const int Kneeright = 17;
        public const int Ankleright = 18;
        public const int Footright = 19;
        public const int Spineshoulder = 20;
        public const int Handtipleft = 21;
        public const int Thumbleft = 22;
        public const int Handtipright = 23;
        public const int Thumbright = 24;
        #endregion

        #region original varies
        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 15;
        private const double HeadSize = 20;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;
        private ColorFrameReader colorFrameReader = null;///////////////////////////////////////////////////////////////////////////////        

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        private List<Pen> bodyColors;
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            // open the reader for the body frames
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            if(COLORFRAME)this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();///////////////////////////////////////////////////

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            // Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // populate body colors, one for each BodyIndex
            this.bodyColors = new List<Pen>();

            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));

            // open the sensor
            this.kinectSensor.Open();

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            InitializeComponent();
        }

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.imageSource;
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
            if (COLORFRAME)if (this.colorFrameReader != null)
            {
                colorFrameReader.FrameArrived += colorFrameReader_FrameArrived;///////////////////////////////////////////////////////
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
            if (COLORFRAME) if (this.colorFrameReader != null)/////////////////////////////////////////////////////////////////////////////////////
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null) this.bodies = new Body[bodyFrame.BodyCount];
                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                try
                {
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        // Draw a transparent background to set the render size
                        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                        int penIndex = 0;
                        var body = bodies[0];
                        foreach (Body thebody in this.bodies)
                        {
                            Pen drawPen = this.bodyColors[penIndex++];
                            if (thebody.IsTracked)
                            {
                                this.DrawBody(thebody, dc, drawPen);
                                if (body.Joints[JointType.SpineBase].Position.Z == 0) body = thebody;
                                else if (body.Joints[JointType.SpineBase].Position.Z > thebody.Joints[JointType.SpineBase].Position.Z) body = thebody;
                            }
                        }

                        Pen drawPenInside = new Pen(Brushes.LightBlue, 11);
                        Pen drawPenOutside = new Pen(Brushes.DarkBlue, 11);
                        this.DrawClippedEdges(body, dc);
                        if (outside) this.DrawBody(body, dc, drawPenOutside);
                        else
                        {
                            this.DrawBody(body, dc, drawPenInside);
                            #region PPT control
                            CameraSpacePoint head = getCameraSpacePoint(body, JointType.Head);
                            CameraSpacePoint handL = getCameraSpacePoint(body, JointType.HandLeft);
                            CameraSpacePoint handR = getCameraSpacePoint(body, JointType.HandRight);
                            CameraSpacePoint spineBase = getCameraSpacePoint(body, JointType.SpineBase);
                            CameraSpacePoint shoulderR = getCameraSpacePoint(body, JointType.ShoulderRight);
                            CameraSpacePoint shoulderL = getCameraSpacePoint(body, JointType.ShoulderLeft);

                            if (hiding)
                            {
                                state.Content = "";
                                state.Background = Brushes.Transparent;
                            }
                            else
                            {
                                state.Content = mode[State];
                                state.Background = Brushes.Black;
                            }
                            if (handL.Y < head.Y) State = LOCKED;
                            if (State == LOCKED) { }
                            else if (spineBase.Z - handL.Z > LIFT && spineBase.Z - handR.Z < LIFT && handR.X - spineBase.X > 150) exit();
                            else if (spineBase.Z - handL.Z > LIFT && spineBase.Z - handR.Z > LIFT)
                            {
                                if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Open) State = TOESC;
                                if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Closed) { State = SELECTING; itab = 0; }
                                if (body.HandRightState == HandState.Closed && body.HandLeftState == HandState.Open) State = TOPLAY;
                                if (body.HandRightState == HandState.Closed && body.HandLeftState == HandState.Closed) State = TOSTART;
                                if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Lasso) State = TOHIDE;
                            }
                            switch (State)
                            {
                                case LOCKED:
                                    if (handR.Y < head.Y) State = CONTROLLING;
                                    break;
                                case TOHIDE:
                                    if (spineBase.Z - handL.Z < LIFT && spineBase.Z - handR.Z < LIFT)
                                    {
                                        hiding = !hiding;
                                        State = CONTROLLING;
                                    }
                                    break;
                                case TOSTART:
                                    if (spineBase.Z - handL.Z < LIFT && spineBase.Z - handR.Z < LIFT)
                                    {
                                        SendKeys.SendWait("{F5}");
                                        State = CONTROLLING;
                                    }
                                    break;
                                case TOPLAY:
                                    if (spineBase.Z - handL.Z < LIFT && spineBase.Z - handR.Z < LIFT)
                                    {
                                        SendKeys.SendWait("{Tab}");
                                        SendKeys.SendWait("{Enter}");
                                        State = LOCKED;
                                    }
                                    break;
                                case TOESC:
                                    if (spineBase.Z - handL.Z < LIFT && spineBase.Z - handR.Z < LIFT)
                                    {
                                        SendKeys.SendWait("{Esc}");
                                        State = LOCKED;
                                    }
                                    break;
                                case CONTROLLING:
                                    if (spineBase.Z - handL.Z > LIFT && spineBase.Z - handR.Z < LIFT) // if left hand lift forward only
                                    {
                                        if (handL.X < shoulderL.X) prevpage = true;
                                        else
                                        {
                                            if (prevpage)
                                            {
                                                SendKeys.SendWait("{Left}");
                                                prevpage = false;
                                            }
                                        }
                                    }
                                    else if (spineBase.Z - handR.Z > LIFT && spineBase.Z - handL.Z < LIFT) // if right hand lift forward only
                                    {
                                        if (handR.X > shoulderR.X) nextpage = true;
                                        else
                                        {
                                            if (nextpage)
                                            {
                                                SendKeys.SendWait("{Right}");
                                                nextpage = false;
                                            }
                                        }
                                    }
                                    break;
                                case SELECTING:
                                    if (itab != 0) state.Content += "(" + itab.ToString() + ")";
                                    if (spineBase.Z - handL.Z < LIFT && spineBase.Z - handR.Z < LIFT)
                                    {
                                        SendKeys.SendWait("{Enter}");
                                        State = CONTROLLING;
                                    }
                                    else if (spineBase.Z - handL.Z > LIFT && spineBase.Z - handR.Z < LIFT)
                                    {
                                        if (handL.X < shoulderL.X) prevpage = true;
                                        else
                                        {
                                            if (prevpage)
                                            {
                                                SendKeys.SendWait("+{Tab}");
                                                itab--;
                                                prevpage = false;
                                            }
                                        }
                                    }
                                    else if (spineBase.Z - handR.Z > LIFT && spineBase.Z - handL.Z < LIFT)
                                    {
                                        if (handR.X > shoulderR.X) nextpage = true;
                                        else
                                        {
                                            if (nextpage)
                                            {
                                                SendKeys.SendWait("{Tab}");
                                                itab++;
                                                nextpage = false;
                                            }
                                        }
                                    }
                                    break;
                                default: break;
                            }
                            #endregion
                        }

                        // prevent drawing outside of our render area
                        this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    }
                }
                catch { return; }
            }
        }
        private CameraSpacePoint getCameraSpacePoint(Body body, JointType jointType)
        {
            CameraSpacePoint cameraSpacePoint;
            CameraSpacePoint position = body.Joints[jointType].Position;
            if (position.Z < 0) position.Z = InferredZPositionClamp;
            cameraSpacePoint.Z = position.Z;
            DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
            cameraSpacePoint.X = depthSpacePoint.X;
            cameraSpacePoint.Y = depthSpacePoint.Y;
            return cameraSpacePoint;
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(Body thebody, DrawingContext drawingContext, Pen drawingPen)
        {
            if (hiding) return;
            IReadOnlyDictionary<JointType, Joint> joints = thebody.Joints;
            // convert the joint points to depth (display) space
            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
            foreach (JointType jointType in joints.Keys)
            {
                // sometimes the depth(Z) of an inferred joint may show as negative
                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                CameraSpacePoint position = joints[jointType].Position;
                if (position.Z < 0) position.Z = InferredZPositionClamp;
                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
            }

            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }

            // Draw the hands and head
            Brush drawbrush = Brushes.LightBlue;
            drawingContext.DrawEllipse(drawbrush, null, jointPoints[JointType.Head], HeadSize, HeadSize);
            switch (thebody.HandLeftState)
            {
                case HandState.Closed:drawbrush = this.handClosedBrush; break;
                case HandState.Open:drawbrush = this.handOpenBrush; break;
                case HandState.Lasso:drawbrush = this.handLassoBrush; break;
                case HandState.Unknown:drawbrush = Brushes.Black; break;
                case HandState.NotTracked: drawbrush = Brushes.Yellow; break;
            }
            drawingContext.DrawEllipse(drawbrush, null, jointPoints[JointType.HandLeft], HandSize, HandSize);
            switch (thebody.HandRightState)
            {
                case HandState.Closed: drawbrush = this.handClosedBrush; break;
                case HandState.Open: drawbrush = this.handOpenBrush; break;
                case HandState.Lasso: drawbrush = this.handLassoBrush; break;
                case HandState.Unknown: drawbrush = Brushes.Black; break;
                case HandState.NotTracked: drawbrush = Brushes.Yellow; break;
            }
            drawingContext.DrawEllipse(drawbrush, null, jointPoints[JointType.HandRight], HandSize, HandSize);
        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            outside = false;
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                if (!hiding) drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                if (!hiding) drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
                outside = true;
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                if (!hiding) drawingContext.DrawRectangle(Brushes.Red, null, new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
                outside = true;
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                if (!hiding) drawingContext.DrawRectangle(Brushes.Red, null, new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
                outside = true;
            }
        }

        private void colorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                // Defensive programming: Just in case the sensor skips a frame, exit the function
                if (colorFrame == null) return;

                // Setup an array that can hold all of the bytes of the image
                var colorFrameDescription = colorFrame.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
                var frameSize = colorFrameDescription.Width * colorFrameDescription.Height * colorFrameDescription.BytesPerPixel;
                var colorData = new byte[frameSize];

                // Fill in the array with the data from the camera
                colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Bgra);

                // Use the byte array to make an image and put it on the screen
                CameraImage.Source = BitmapSource.Create(
                    colorFrame.ColorFrameSource.FrameDescription.Width,
                    colorFrame.ColorFrameSource.FrameDescription.Height,
                    96, 96, PixelFormats.Bgr32, null, colorData, colorFrameDescription.Width * 4);
            }
        }

        private void exit()
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }
            if (COLORFRAME) if (this.colorFrameReader != null)///////////////////////////////////////////////////////////////////////////////////////
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }
            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
            for (int i = 0; i < 3; i++)
            {
                state.Content = "Kinect_PPT is closing......(" + (3 - i).ToString() + ")";
                System.Threading.Thread t = new System.Threading.Thread(o => System.Threading.Thread.Sleep(1000));
                t.Start(this);
                while (t.IsAlive) System.Windows.Forms.Application.DoEvents();
            }
            //System.Threading.Thread.Sleep(3000);
            System.Environment.Exit(System.Environment.ExitCode);
        }
    }
}
