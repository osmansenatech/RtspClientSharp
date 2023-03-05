using System;
using FPV.RawFramesDecoding;
using FPV.RawFramesDecoding.DecodedFrames;

namespace FPV
{
    public interface IVideoSource
    {
        event EventHandler<IDecodedVideoFrame> FrameReceived;
    }
}