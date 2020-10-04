using imageStacker.Core;
using imageStacker.Core.ByteImage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoWriter : IImageWriter<MutableByteImage>
    {
        public FfmpegVideoWriter(FfmpegVideoWriterArguments arguments, Logger logger)
        {
            _arguments = arguments;
            _logger = logger;
        }

        private readonly Logger _logger;

        private readonly MemoryStream inputStream = new MemoryStream(1024 * 1024 * 128);

        private readonly FfmpegVideoWriterArguments _arguments;

        public async Task Writefile(MutableByteImage image, ISaveInfo info)
        {
            var inputPipe = new FFMpegCore.Pipes.StreamPipeSource(inputStream);
            var args = FFMpegCore.FFMpegArguments
                .FromPipe(inputPipe)
                .WithFramerate(_arguments.Framerate)
                .OutputToFile(_arguments.OutputFile)
                .NotifyOnProgress(
                    percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "OutputVideoEncoding"),
                    TimeSpan.FromSeconds(1));
            await args.ProcessAsynchronously(true);
        }
    }

    public class FfmpegVideoWriterArguments
    {
        public string Format { get; set; } = "mp4";
        public double Framerate { get; set; } = 60;

        public string OutputFile { get; set; }
    }
}
