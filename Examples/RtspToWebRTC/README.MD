This project receives an RTSP stream and sends it through WebRTC to a browser client without transcoding.

The RTSP stream address is set using the RtspClient.Setup function in the main program.
Once you start the program, open the WebRTCClient.html on a browser to see your camera feed.

This example uses an internal WebSocketServer for WebRTC Signaling. This example will not work if published online since the signaling server only works for localhost. In that case, the WebRTC Signaling can be achieved through an MQTT server. Below project may provide some insight on this:
https://github.com/cjb/serverless-webrtc

The IP camera used sends video frames encoded with H264 (RTP packet type 96) and audio frames encoded with PCMA (RTP packet type 8). H264 is supported by WebRTC but PCMA is not.

If your camera uses different encoders, make sure they are supported by the WebRTC standard and change the RtspClient.GetVideoFormat() and 
RtspClient.GetAudioFormat() functions accordingly.

You can check out your camera's RTP payload types from the messages on the console screen and find out what they are at the website below:

https://www.iana.org/assignments/rtp-parameters/rtp-parameters.xhtml#rtp-parameters-1

You can check the codecs supported by WebRTC below:

https://developer.mozilla.org/en-US/docs/Web/Media/Formats/WebRTC_codecs
