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

namespace KinectColorViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinect = null; 
        private ColorFrameReader colorFrame = null;  
        private WriteableBitmap colorBitmap = null; 
        private FrameDescription colorFrameDescription = null; 

        public MainWindow()
        {
            this.kinect = KinectSensor.GetDefault(); 
            this.colorFrame = kinect.ColorFrameSource.OpenReader();  
            this.colorFrame.FrameArrived += colorFrame_FrameArrived; 
            this.colorFrameDescription = this.kinect.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.kinect.Open(); 
            this.DataContext = this;
            InitializeComponent();
        }
        void colorFrame_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame frame = e.FrameReference.AcquireFrame()) 
            {
                if (frame != null)
                {
                    this.colorBitmap.Lock(); 
                    frame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)(this.colorFrameDescription.Width * this.colorFrameDescription.Height * 4), ColorImageFormat.Bgra);
                    this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));
                   
                    this.colorBitmap.Unlock(); 
                }
            }
        }
        public ImageSource ColorSource
        {
            get
            {
                return this.colorBitmap;
            }
        }
    }
}
