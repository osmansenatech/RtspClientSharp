using System;
using System.Collections.Generic;
using System.Drawing;
using FPV.AVI;
using FPV.RawFramesDecoding;
using FPV.RawFramesDecoding.DecodedFrames;
using FPV.RawFramesDecoding.FFmpeg;
using FPV.Streaming;

namespace FPV
{
    public partial class FPVManager
    {
        public const int FrameRate = 20;
        private const int SleepMS = 1000 / FrameRate;
        private RTSPManager rtspMAnager;
        private ImageStreamingServer server;
        TransformParameters _transformParameters;
        Bitmap _writeableBitmap;
        VideoRecorder videoRecorder;

        public void StartFPVStream(string recordPath, string url, string username=null, string password=null, int width=640, int height=480)
        {
            if (rtspMAnager != null) return;

            _transformParameters = new TransformParameters(RectangleF.Empty,
                        new System.Drawing.Size(width, height),
                        ScalingPolicy.Stretch, PixelFormat.Bgra32, ScalingQuality.FastBilinear);

            _writeableBitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            videoRecorder = new VideoRecorder(recordPath, width, height, FrameRate);

            rtspMAnager = new RTSPManager();
            rtspMAnager.VideoSource.FrameReceived += VideoSource_FrameReceived;
            rtspMAnager.Start(url,username,password);
            
            server = new ImageStreamingServer(Snapshots());
            server.Start(80);
        }

        public void StopFPVStream()
        {
            if (rtspMAnager == null) return;
            rtspMAnager.VideoSource.FrameReceived -= VideoSource_FrameReceived;
            rtspMAnager.Stop();
            rtspMAnager = null;

            videoRecorder.Dispose();
            videoRecorder = null;

            server.Stop();
            server.Dispose();
            server = null;

            _writeableBitmap.Dispose();
            _writeableBitmap = null;
        }

        private IEnumerable<Image> Snapshots()
        {
            while (rtspMAnager != null)
            {
                var sleepMS = SleepMS;
                var time = DateTime.Now;
                lock (_writeableBitmap)
                {
                    yield return _writeableBitmap;
                    videoRecorder.AddFrame(_writeableBitmap);
                }
                var timeEllapsed = (DateTime.Now - time).TotalMilliseconds;
                sleepMS -= (int)timeEllapsed;

                System.Threading.Thread.Sleep(sleepMS);
            }
            yield break;
        }

        private void VideoSource_FrameReceived(object sender, IDecodedVideoFrame frame)
        {
            lock (_writeableBitmap)
            {
                System.Drawing.Imaging.BitmapData bd = _writeableBitmap.LockBits(new Rectangle(0, 0, _writeableBitmap.Width, _writeableBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                try
                {
                    IntPtr pval = bd.Scan0;
                    frame.TransformTo(pval, bd.Stride, _transformParameters);
                }
                finally
                {
                    _writeableBitmap.UnlockBits(bd);
                }
            }
        }

    }
}
