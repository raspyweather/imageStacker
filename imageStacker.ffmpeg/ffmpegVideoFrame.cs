using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FFMpegCore.Pipes;
using imageStacker.Core;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoFrame : IVideoFrame
    {
        private readonly MutableImage image;

        public FfmpegVideoFrame(MutableImage byteImage)
        {
            this.image = byteImage;
        }

        public int Width => image.Width;

        public int Height => image.Height;

        public string Format => image.BytesPerPixel == 3 ? "bgr24" : "bgr48le";

        public void Serialize(Stream pipe)
        {
            var data = image.GetBytes();
            pipe.Write(data, 0, data.Length);
        }

        public Task SerializeAsync(Stream pipe, CancellationToken token)
        {
            var data = image.GetBytes();
            return pipe.WriteAsync(data, 0, data.Length, token);
        }
    }
}
