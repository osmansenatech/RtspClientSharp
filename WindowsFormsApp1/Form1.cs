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
            private FPVManager _manager = new FPVManager();
            public byte[] bitmapData { get; set; }
            public void FpvVideoFailed(string src)
            {
                Console.WriteLine(src);
            }
            public void StartVideo(int width, int height)
            {
                _manager.StartFPVStream("fpv.avi", "rtsp://admin:admin@192.168.3.12:554/1",null,null,width,height);
            }
            public void RestartVideo()
            {
            }
            public void StopVideo()
            {
                _manager.StopFPVStream();
            }
        }

        private readonly ChromiumWebBrowser browser;
        private readonly BrowserConnection browserConnection;
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
        }

        private void Browser_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            //browser.ShowDevTools();
        }


        void ExecuteScript(string script)
        {
            //Console.WriteLine(script);
            if (browser.IsBrowserInitialized)
                CefSharp.WebBrowserExtensions.ExecuteScriptAsync(browser, script);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            browserConnection.StopVideo();
        }
    }
}
