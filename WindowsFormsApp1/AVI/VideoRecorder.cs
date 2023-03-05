using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPV.AVI
{
    internal class VideoRecorder: IDisposable
    {
        FileStream aviFile;
        H264Writer writer;
        OpenH264Lib.Encoder encoder;
        DateTime startTime;
        bool isEncoding;

        public VideoRecorder(string aviPath, int width = 320, int height = 240, float fps = 24, int bitsPerSecond = 500000, float keyFrameInterval = 2)
        {
            aviFile = System.IO.File.OpenWrite(aviPath);
            writer = new H264Writer(aviFile, width, height, fps);

            encoder = new OpenH264Lib.Encoder("openh264-2.3.1.dll");

            OpenH264Lib.Encoder.OnEncodeCallback onEncode = (data, length, frameType) =>
            {
                if (writer!=null)
                lock (writer)
                {
                    var keyFrame = (frameType == OpenH264Lib.Encoder.FrameType.IDR) || (frameType == OpenH264Lib.Encoder.FrameType.I);
                    writer.AddImage(data, keyFrame);
                    //Console.WriteLine("Encord {0} bytes, KeyFrame:{1}", length, keyFrame);
                    isEncoding = false;
                }
            };

            encoder.Setup(width, height, bitsPerSecond, fps, keyFrameInterval, /*OpenH264Lib.Encoder.rcMODES.RC_OFF_MODE, */onEncode);
            startTime = DateTime.Now;
        }

        public void AddFrame(Bitmap bmp)
        {
            if (encoder != null && isEncoding == false)
                lock (encoder)
                {
                    isEncoding = true;
                    encoder.Encode(bmp, (float)DateTime.Now.Subtract(startTime).TotalSeconds);
                }
        }

        public void Dispose()
        {
            lock(encoder)
            {
                encoder.Dispose();
                encoder = null;
            }
            lock(writer)
            {
                writer.Close();
                writer = null;
            }
        }
    }
}
