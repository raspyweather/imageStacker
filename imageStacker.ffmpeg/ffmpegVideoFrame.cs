using FFMpegCore.Pipes;
using imageStacker.Core.ByteImage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoFrame : IVideoFrame
    {
        private readonly MutableByteImage byteImage;
        public FfmpegVideoFrame(MutableByteImage byteImage)
        {
            this.byteImage = byteImage;
        }

        public int Width => byteImage.Width;

        public int Height => byteImage.Height;

        public string Format => "bgr24";

        public void Serialize(Stream pipe)
            => pipe.Write(byteImage.Data, 0, byteImage.Data.Length);

        public Task SerializeAsync(Stream pipe, CancellationToken token)
            => pipe.WriteAsync(byteImage.Data, 0, byteImage.Data.Length, token);
    }
}
