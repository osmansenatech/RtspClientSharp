using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SIPSorcery.Media;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using WebSocketSharp.Server;

namespace RTSPToWebRTC
{
    class WebRTCClient
    {
        private const int WEBSOCKET_PORT = 80;
        
        public static void Connect()
        {
            Console.WriteLine("WebRTC Get Started");

            // Start web socket.
            Console.WriteLine("Starting web socket server...");
            var webSocketServer = new WebSocketServer(IPAddress.Any, WEBSOCKET_PORT);
            webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/", (peer) => peer.CreatePeerConnection = () => CreatePeerConnection());
            webSocketServer.Start();

            Console.WriteLine($"Waiting for web socket connections on {webSocketServer.Address}:{webSocketServer.Port}...");

            Console.WriteLine("Press any key exit.");
            Console.ReadKey();
        }

        private static Task<RTCPeerConnection> CreatePeerConnection()
        {
            var pc = new RTCPeerConnection(null);
            MediaStreamTrack videoTrack = new MediaStreamTrack(RtspClient.GetVideoFormat(), MediaStreamStatusEnum.SendOnly);
            MediaStreamTrack audioTrack = new MediaStreamTrack(RtspClient.GetAudioFormat(), MediaStreamStatusEnum.SendOnly);

            pc.addTrack(videoTrack);
            pc.addTrack(audioTrack);

            RtspClient.OnReceivedRTPFrame += pc.SendRtpRaw;

            pc.onconnectionstatechange += async (state) =>
            {
                Console.WriteLine($"Peer connection state change to {state}.");

                switch (state)
                {
                    case RTCPeerConnectionState.connected:
                        RtspClient.StartVideo();
                        break;
                    case RTCPeerConnectionState.failed:
                        pc.Close("ice disconnection");
                        break;
                    case RTCPeerConnectionState.closed:
                        RtspClient.StopVideo();
                        break;
                }
            };

            return Task.FromResult(pc);
        }
    }
}