using FFMpegCore.Arguments;

namespace imageStacker.ffmpeg
{
    internal class FfmpegFpsArgument : IArgument
    {
        public FfmpegFpsArgument(double fps)
        {
            Fps = fps;
        }

        public double Fps { get; set; }

        public string Text => $"-vf fps=\"{Fps}\"";
    }
}