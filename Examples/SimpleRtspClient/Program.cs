using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.Rtsp;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;

namespace RTSPToWebRTC
{
    class Program
    {
        static void Main()
        {
            RTSPClient.Setup("rtsp://192.168.3.12:554/1", "admin", "admin");
            WebRTCClient.Connect();
        }
    }
}