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
using System.IO;
using System.Globalization;
using System.ComponentModel;

namespace KinectDepthviewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MapDepthToByte = 8000 / 256;
        private KinectSensor kinect=null;
        private byte[] pixelData;
        private DepthFrameReader reader=null;
        private FrameDescription depthframdescrioption = null;
        private WriteableBitmap depthmap = null;
        public MainWindow()
        {
            this.kinect = KinectSensor.GetDefault();
            this.reader = this.kinect.DepthFrameSource.OpenReader();
            this.reader.FrameArrived += this.Reader_FrameArrived;
            this.depthframdescrioption = this.kinect.DepthFrameSource.FrameDescription;
            this.pixelData = new byte[this.depthframdescrioption.Width * this.depthframdescrioption.Height];
            this.depthmap = new WriteableBitmap(depthframdescrioption.Width, depthframdescrioption.Height, 96.0, 96.0, PixelFormats.Gray8, null);
            this.kinect.Open();
            this.DataContext = this;
            InitializeComponent();
        }
        public ImageSource ImageSource
        {
            get
            {
                return this.depthmap;
            }
        }
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;
            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        if (((this.depthframdescrioption.Width * this.depthframdescrioption.Height) == (depthBuffer.Size / this.depthframdescrioption.BytesPerPixel)) &&
                            (this.depthframdescrioption.Width == this.depthmap.PixelWidth) && (this.depthframdescrioption.Height == this.depthmap.PixelHeight))
                        {
                            ushort maxDepth = ushort.MaxValue;
                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            ushort* frameData = (ushort*)depthFrameData;
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthframdescrioption.BytesPerPixel); ++i)
            {
                ushort depth = frameData[i];
                this.pixelData[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }
        private void RenderDepthPixels()
        {
            this.depthmap.WritePixels(
                new Int32Rect(0, 0, this.depthmap.PixelWidth, this.depthmap.PixelHeight),
                this.pixelData,
                this.depthmap.PixelWidth,
                0);
        }
        public event PropertyChangedEventHandler PropertyChanged;
       
    }
}
