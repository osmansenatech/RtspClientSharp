using System;

namespace FPV.RawFramesDecoding.DecodedFrames
{
    class DecodedVideoFrame : IDecodedVideoFrame
    {
        private readonly Action<IntPtr, int, TransformParameters> _transformAction;
        
        public DecodedVideoFrameParameters FrameParameters { get; private set; }
        public DecodedVideoFrame(Action<IntPtr, int, TransformParameters> transformAction, DecodedVideoFrameParameters frameParameters)
        {
            _transformAction = transformAction;
            FrameParameters = frameParameters;
        }

        public void TransformTo(IntPtr buffer, int bufferStride, TransformParameters transformParameters)
        {
            _transformAction(buffer, bufferStride, transformParameters);
        }
    }
}