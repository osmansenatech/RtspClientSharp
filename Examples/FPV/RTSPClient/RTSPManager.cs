using FPV.RawFramesReceiving;
using RtspClientSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FPV
{
    internal class RTSPManager
    {
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";

        private readonly RealtimeVideoSource _realtimeVideoSource = new RealtimeVideoSource();
        private readonly RealtimeAudioSource _realtimeAudioSource = new RealtimeAudioSource();

        private IRawFramesSource _rawFramesSource;

        public event EventHandler<string> StatusChanged;

        public IVideoSource VideoSource => _realtimeVideoSource;

        public void Start(string address, string username, string password)
        {
            if (_rawFramesSource != null)
                return;

            if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                address = RtspPrefix + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
            {
                Console.WriteLine("Error: Invalid device address: " + address);
                return;
            }

            var credential = new NetworkCredential(username, password);

            var connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) :
                new ConnectionParameters(deviceUri, credential);

            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            _rawFramesSource = new RawFramesSource(connectionParameters);
            _rawFramesSource.ConnectionStatusChanged += ConnectionStatusChanged;

            _realtimeVideoSource.SetRawFramesSource(_rawFramesSource);
            _realtimeAudioSource.SetRawFramesSource(_rawFramesSource);

            _rawFramesSource.Start();
        }

        public void Stop()
        {
            if (_rawFramesSource == null)
                return;

            _rawFramesSource.Stop();
            _realtimeVideoSource.SetRawFramesSource(null);
            _rawFramesSource = null;
        }

        private void ConnectionStatusChanged(object sender, string s)
        {
            StatusChanged?.Invoke(this, s);
        }
    }
}
