using FFMpegCore;
using FFMpegCore.Pipes;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoWriter : IImageWriter<MutableByteImage>
    {
        public FfmpegVideoWriter(FfmpegVideoWriterArguments arguments, IBoundedQueue<MutableByteImage> queue, Logger logger)
        {
            _arguments = arguments;
            _logger = logger;
            boundedQueue = queue;
            this.source = new RawVideoPipeSource(new MutableByteImageBoundedQueueEnumerator(queue));
        }

        private readonly RawVideoPipeSource source;

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
            var args = FFMpegArguments
                   .FromPipeInput(source, args =>
                   {
                       //args.UsingMultithreading(true);
                   })
                   .OutputToFile(_arguments.OutputFile, true, options => options.WithFramerate(_arguments.Framerate)
                   .ForcePixelFormat("yuv420p")
                   .WithVideoCodec("libx264")
                   .WithConstantRateFactor(25)
                   .UsingMultithreading(true)
                   .UsingThreads(8)
                   .WithCustomArgument("-profile:v baseline -level 3.0"))
                   .NotifyOnProgress(
                       percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "OutputVideoEncoding"),
                       TimeSpan.FromSeconds(1));

            await args.ProcessAsynchronously(true);
            _logger.WriteLine("finished writing", Verbosity.Info);
        }
    }

    public class MutableByteImageBoundedQueueEnumerator : IEnumerator<IVideoFrame>
    {
        public MutableByteImageBoundedQueueEnumerator(IBoundedQueue<MutableByteImage> queue)
        {
            this.queue = queue;
        }

        private IBoundedQueue<MutableByteImage> queue;
        public IVideoFrame Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            var item = this.queue.DequeueOrDefault();
            item.Wait();
            var frame = item.Result;
            if (frame == default)
            {
                return false;
            }
            this.Current = new FfmpegVideoFrame(frame);
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
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
