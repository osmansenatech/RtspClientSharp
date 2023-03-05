using System;

namespace FPV.RawFramesDecoding.DecodedFrames
{
    public interface IDecodedVideoFrame
    {
        DecodedVideoFrameParameters FrameParameters { get; }
        void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParameters);
    }
}