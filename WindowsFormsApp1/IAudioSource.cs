using System;
using FPV.RawFramesDecoding.DecodedFrames;

namespace FPV
{
    interface IAudioSource
    {
        event EventHandler<IDecodedAudioFrame> FrameReceived;
    }
}
