using System;

namespace FPV.RawFramesDecoding.DecodedFrames
{
    public interface IDecodedAudioFrame
    {
        DateTime Timestamp { get; }
        ArraySegment<byte> DecodedBytes { get; }
        AudioFrameFormat Format { get; }
    }
}