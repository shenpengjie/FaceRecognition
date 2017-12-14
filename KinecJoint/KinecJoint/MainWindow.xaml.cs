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

namespace KinecJoint
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private int displayWidth;
        private int displayHeight;
        private bool outside = false;
        private KinectSensor kinectSensor = null;
        private DrawingImage drawing;
        private CoordinateMapper coordinateMapper = null;
        private DrawingGroup drawingGroup;
        private Brush HandCloseBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
        private Brush HandOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
        private Brush HandLessoBruh = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
        private Brush TrackedJoint = new SolidColorBrush(Color.FromArgb(128, 66, 128, 60));
        private Brush InferredJoint = Brushes.Yellow;
        private Pen Bone = new Pen(Brushes.Gray, 1);
        private BodyFrameReader bodyFrameReader = null;
        private ColorFrameReader colorFrameReader = null;
        private FrameDescription frameDescription = null;
        private Body[] bodies = null;
        private List<Tuple<JointType, JointType>> bones;
        private List<Pen> bodycolors;
        public MainWindow()
        {
            this.kinectSensor = KinectSensor.GetDefault();

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            this.frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
            this.bones = new List<Tuple<JointType, JointType>>();
            //人体躯干
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));
            //右边手臂
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));
            //左边手臂
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));
            //右腿
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));
            //左腿
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
            this.bodycolors = new List<Pen>();
            this.bodycolors.Add(new Pen(Brushes.Red, 6));
            this.bodycolors.Add(new Pen(Brushes.Orange, 6));
            this.bodycolors.Add(new Pen(Brushes.Green, 6));
            this.bodycolors.Add(new Pen(Brushes.Blue, 6));
            this.bodycolors.Add(new Pen(Brushes.Indigo, 6));
            this.bodycolors.Add(new Pen(Brushes.Violet, 6));
            kinectSensor.Open();
            this.drawingGroup = new DrawingGroup();
            this.drawing = new DrawingImage(this.drawingGroup);
            InitializeComponent();
        }
        public ImageSource ImageSource
        {
            get
            {
                return this.drawing;
            }
        }
       /* private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool Datareceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrameReader != null)
                {
                    if (this.bodies == null) this.bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    Datareceived = true;
                }
                if(Datareceived)
                {
                    try
                    {
                        using (DrawingContext dc = this.drawingGroup.Open())
                        {
                            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                            int penIndex = 0;
                            var body = bodies[0];
                            foreach (Body thebody in this.bodies)
                            {
                                Pen drawPen = this.bodycolors[penIndex++];
                                if (thebody.IsTracked)
                                {
                                    this.DrawBody(thebody, dc, drawPen);
                                    if (body.Joints[JointType.SpineBase].Position.Z == 0) body = thebody;
                                    else if (body.Joints[JointType.SpineBase].Position.Z > thebody.Joints[JointType.SpineBase].Position.Z) body = thebody;
                                }
                            }
                            Pen drawPenInside = new Pen(Brushes.LightBlue, 11);
                            Pen drawPenOutside = new Pen(Brushes.DarkBlue, 11);
                            if (outside) this.DrawBody(body, dc, drawPenOutside);
                            else
                            {
                                this.DrawBody(body, dc, drawPenInside);
                            }
                            this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                        }
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }
        
        private void DrawBody(Body thebody, DrawingContext drawingContext,Pen drawingpen)
        {
            IReadOnlyDictionary<JointType, Joint> joints = thebody.Joints;
            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
            foreach (JointType jointType in joints.Keys)
            {
                
                CameraSpacePoint position = joints[jointType].Position;
                if (position.Z < 0) position.Z = 0.1f;
                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
            }

            
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingpen);
            }

            
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.TrackedJoint;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.InferredJoint;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], 3, 3);
                }
            }

           
            Brush drawbrush = Brushes.LightBlue;
            drawingContext.DrawEllipse(drawbrush, null, jointPoints[JointType.Head], 20, 20);
            switch (thebody.HandLeftState)
            {
                case HandState.Closed: drawbrush = this.HandCloseBrush; break;
                case HandState.Open: drawbrush = this.HandOpenBrush; break;
                case HandState.Lasso: drawbrush = this.HandOpenBrush; break;
                case HandState.Unknown: drawbrush = Brushes.Black; break;
                case HandState.NotTracked: drawbrush = Brushes.Yellow; break;
            }
            drawingContext.DrawEllipse(drawbrush, null, jointPoints[JointType.HandLeft], 15, 15);
            switch (thebody.HandRightState)
            {
                case HandState.Closed: drawbrush = this.HandCloseBrush; break;
                case HandState.Open: drawbrush = this.HandOpenBrush; break;
                case HandState.Lasso: drawbrush = this.HandLessoBruh; break;
                case HandState.Unknown: drawbrush = Brushes.Black; break;
                case HandState.NotTracked: drawbrush = Brushes.Yellow; break;
            }
            drawingContext.DrawEllipse(drawbrush, null, jointPoints[JointType.HandRight], 15, 15);
        }
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

          
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            
            Pen drawPen = this.Bone;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += Reader_FrameArrived;
            }
        }*/

    }
}

        
