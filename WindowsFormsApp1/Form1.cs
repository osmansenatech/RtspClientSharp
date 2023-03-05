using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using FPV.RawFramesDecoding;
using FPV.RawFramesDecoding.DecodedFrames;
using FPV.RawFramesDecoding.FFmpeg;
using FPV.Streaming;

namespace FPV
{
    public partial class Form1 : Form
    {
        public class BrowserConnection
        {
            public int width { get; set; }
            public int height { get; set; }
            public byte[] bitmapData { get; set; }
            public void FpvVideoFailed(string src)
            {
                Console.WriteLine(src);
            }
            public void StartVideo()
            {

            }
            public void RestartVideo()
            {
            }
        }

        private readonly ChromiumWebBrowser browser;
        private readonly BrowserConnection browserConnection;
        private readonly RTSPManager rtspMAnager;
        public Form1()
        {
            InitializeComponent();

            var settings = new CefSettings();
            settings.CachePath = Application.StartupPath + "\\Cache";
            settings.Locale = "en-US";
            CefSharp.Cef.Initialize(settings);
            CefSharpSettings.WcfEnabled = true;

            var applicationFile = "file:///" + Application.StartupPath + "/fpv.html";

            browser = new ChromiumWebBrowser(applicationFile)
            {
                Dock = DockStyle.Fill
            };
            browser.BrowserSettings.FileAccessFromFileUrls = CefSharp.CefState.Enabled;
            browser.BrowserSettings.UniversalAccessFromFileUrls = CefSharp.CefState.Enabled;
            browser.BrowserSettings.LocalStorage = CefSharp.CefState.Disabled;
            browser.BrowserSettings.ApplicationCache = CefSharp.CefState.Disabled;

            browserConnection = new BrowserConnection();
            browser.JavascriptObjectRepository.Register("csharp", browserConnection, false);
            
            Controls.Add(browser);

            browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;

            rtspMAnager = new RTSPManager();
            rtspMAnager.VideoSource.FrameReceived += VideoSource_FrameReceived;
            rtspMAnager.Start("rtsp://192.168.3.12:554/1", "admin", "admin");
            server = new ImageStreamingServer(Snapshots());
            server.Start(80);
        }

        private void Browser_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            //browser.ShowDevTools();
        }

        ImageStreamingServer server;

        private IEnumerable<Image> Snapshots()
        {
            while (true)
            {
                if (_writeableBitmap != null)
                    lock (_writeableBitmap)
                    {
                        yield return _writeableBitmap;
                    }
                System.Threading.Thread.Sleep(50);
            }
            yield break;
        }

        private void VideoSource_FrameReceived(object sender, IDecodedVideoFrame frame)
        {
            if (_transformParameters==null)
            {
                ReinitializeBitmap(frame.FrameParameters.Width,frame.FrameParameters.Height,frame.FrameParameters.PixelFormat);
            }
            //Console.WriteLine($"{frame.FrameParameters.Width}x{frame.FrameParameters.Height} {frame.FrameParameters.PixelFormat}");

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

        TransformParameters _transformParameters;
        Bitmap _writeableBitmap;
        private void ReinitializeBitmap(int width, int height, FFmpegPixelFormat pixelFormat)
        {
            _transformParameters = new TransformParameters(RectangleF.Empty,
                    new System.Drawing.Size(width, height),
                    ScalingPolicy.Stretch, PixelFormat.Bgra32, ScalingQuality.FastBilinear);

            browserConnection.width = width;
            browserConnection.height = height;
            _writeableBitmap = new Bitmap(width, height+1, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        }

        void ExecuteScript(string script)
        {
            //Console.WriteLine(script);
            if (browser.IsBrowserInitialized)
                CefSharp.WebBrowserExtensions.ExecuteScriptAsync(browser, script);
        }

    }
}
