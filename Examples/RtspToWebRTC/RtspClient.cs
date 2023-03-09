using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.Rtsp;
using SIPSorcery.Net;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia;

namespace RTSPToWebRTC
{
    internal class RtspClient
    {
        private static string Url, Username, Password;
        public static void Setup(string url, string username, string password)
        {
            Url = url;
            Username = username;
            Password = password;
        }

        public static VideoFormat GetVideoFormat()
        {
            return new VideoFormat(VideoCodecsEnum.H264, 96);
        }
        public static AudioFormat GetAudioFormat()
        {
            return new AudioFormat(AudioCodecsEnum.PCMA, 8);
        }

        public delegate void ReceivedRTPFrameHandler(SDPMediaTypesEnum mediaType, byte[] payload, uint timestamp, int markerBit, int payloadTypeID);
        public static  ReceivedRTPFrameHandler OnReceivedRTPFrame;

        private static CancellationTokenSource cancellationTokenSource;
        private static Task connectTask;
        public static void StartVideo()
        {
            var serverUri = new Uri(Url);
            var credentials = new NetworkCredential(Username, Password);

            var connectionParameters = new ConnectionParameters(serverUri, credentials);
            connectionParameters.RequiredTracks = RequiredTracks.Video;
            cancellationTokenSource = new CancellationTokenSource();

            connectTask = ConnectAsync(connectionParameters, cancellationTokenSource.Token);
        }

        internal static void SetVideoSourceFormat(VideoFormat videoFormat)
        {
            Console.WriteLine("Requested format: " + videoFormat.FormatName);
        }

        public static void StopVideo()
        {
            cancellationTokenSource.Cancel();
            Console.WriteLine("Canceling");
            connectTask.Wait(CancellationToken.None);
        }

        private static async Task ConnectAsync(ConnectionParameters connectionParameters, CancellationToken token)
        {
            try
            {
                TimeSpan delay = TimeSpan.FromSeconds(5);
                using (var rtspClient = new RtspClientSharp.RtspClient(connectionParameters))
                {

                    rtspClient.RtpPacketReceived += (sender, rtpPacket) =>
                    {
                        Console.WriteLine($"New frame {rtpPacket.Timestamp} \tPayloadType: {rtpPacket.PayloadType}");
                        var data = rtpPacket.PayloadSegment.ToArray();
                        var type = (rtpPacket.PayloadType == 8) ? SDPMediaTypesEnum.audio : SDPMediaTypesEnum.video;

                        OnReceivedRTPFrame?.Invoke(type, data, rtpPacket.Timestamp, rtpPacket.MarkerBit ? 1 : 0, rtpPacket.PayloadType);
                    };

                    while (true)
                    {
                        Console.WriteLine("Connecting...");

                        try
                        {
                            await rtspClient.ConnectAsync(token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (RtspClientException e)
                        {
                            Console.WriteLine(e.ToString());
                            await Task.Delay(delay, token);
                            continue;
                        }

                        Console.WriteLine("Connected.");

                        try
                        {
                            await rtspClient.ReceiveAsync(token);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (RtspClientException e)
                        {
                            Console.WriteLine(e.ToString());
                            await Task.Delay(delay, token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
