namespace imageStacker.ffmpeg
{
    public class FfmpegVideoReaderArguments
    {
        public double Framerate { get; set; }

        public string PathToFfmpeg { get; set; }

        public string[] InputFiles { get; set; }
    }
}