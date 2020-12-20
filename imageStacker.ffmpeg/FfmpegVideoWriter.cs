using FFMpegCore;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using System;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoWriterPiped : IImageWriter<MutableByteImage>
    {
        public FfmpegVideoWriterPiped(FfmpegVideoWriterArguments arguments, IBoundedQueue<MutableByteImage> queue, Logger logger)
        {
            _arguments = arguments;
            _logger = logger;
            boundedQueue = queue;
            this.source = new RawMutableByteFramePipeSourceAsync(queue, logger);
        }

        private readonly RawMutableByteFramePipeSourceAsync source;

        private readonly Logger _logger;

        private readonly IBoundedQueue<MutableByteImage> boundedQueue;

        private readonly FfmpegVideoWriterArguments _arguments;

        public async Task WriteFile(MutableByteImage image, ISaveInfo info)
        {
            await boundedQueue.Enqueue(image);
        }

        public async Task WaitForCompletion()
        {
            FFMpegOptions.Configure(new FFMpegOptions
            {
                RootDirectory = _arguments.PathToFfmpeg
            });
            var args = FFMpegCore.FFMpegArguments
                   .FromPipe(source)
                   .WithFramerate(_arguments.Framerate)
                   .OutputToFile(_arguments.OutputFile)
                   .NotifyOnProgress(
                       percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "OutputVideoEncoding"),
                       TimeSpan.FromSeconds(1));

            await args.ProcessAsynchronously(true);
            _logger.WriteLine("finished writing", Verbosity.Info);
        }
    }

    public class FfmpegVideoWriterArguments
    {
        public string Format { get; set; } = "mp4";
        public double Framerate { get; set; } = 60;

        public string OutputFile { get; set; }
        public string PathToFfmpeg { get; set; }
    }
}
