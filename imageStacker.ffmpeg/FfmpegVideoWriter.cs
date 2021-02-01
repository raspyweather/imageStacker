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
        public FfmpegVideoWriter(FfmpegVideoWriterArguments arguments, ILogger logger)
        {
            _arguments = arguments;
            _logger = logger;
        }

        private RawVideoPipeSource source;

        private readonly ILogger _logger;

        private IBoundedQueue<(MutableByteImage image, ISaveInfo info)> boundedQueue;

        private readonly FfmpegVideoWriterArguments _arguments;

        public async Task WriteFile(MutableByteImage image, ISaveInfo info)
        {
            await boundedQueue.Enqueue((image, info));
        }

        public async Task WaitForCompletion()
        {
            if (!string.IsNullOrWhiteSpace(_arguments.PathToFfmpeg))
            {
                FFMpegOptions.Configure(new FFMpegOptions
                {
                    RootDirectory = _arguments.PathToFfmpeg
                });
            }

            var args = FFMpegArguments
                   .FromPipeInput(source, args =>
                   {
                   })
                   .OutputToFile(_arguments.OutputFile, true, options => options.WithFramerate(_arguments.Framerate)
                   .UsingMultithreading(true)
                   .UsingThreads(Environment.ProcessorCount)
                   .ForcePixelFormat("yuv420p")
                   .WithVideoCodec("libx264")
                   .OverwriteExisting()
                   .WithConstantRateFactor(25)
                   .WithCustomArgument(_arguments.CustomArgs)
                   .WithCustomArgument("-profile:v baseline -level 3.0"))
                   .NotifyOnProgress(
                       percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "OutputVideoEncoding"),
                       TimeSpan.FromSeconds(1));

            await args.ProcessAsynchronously(true);
            _logger.WriteLine("finished writing", Verbosity.Info);
            boundedQueue.CompleteAdding();
        }

        public void SetQueue(IBoundedQueue<(MutableByteImage image, ISaveInfo info)> queue)
        {
            this.boundedQueue = queue;
            this.source = new RawVideoPipeSource(new MutableByteImageBoundedQueueEnumerator(queue));
        }
    }

    public static class FfmpegVideoWriterPresets
    {
        public static FFMpegArgumentOptions UseFHDPreset(this FFMpegArgumentOptions args)
        {
            return args.ForcePixelFormat("yuv420p")
                   .WithVideoCodec("libx264")
                   .WithConstantRateFactor(25)
                   .WithCustomArgument("-profile:v baseline -level 3.0 -vf scale=-1:1080");
        }

        public static FFMpegArgumentOptions Use4KPreset(this FFMpegArgumentOptions args)
        {
            return args.ForcePixelFormat("yuv420p")
                   .WithVideoCodec("libx264")
                   .WithConstantRateFactor(25)
                   .WithCustomArgument("-profile:v baseline -level 3.0 -vf scale=-1:2160");
        }
    }

    public class MutableByteImageBoundedQueueEnumerator : IEnumerator<IVideoFrame>
    {
        public MutableByteImageBoundedQueueEnumerator(IBoundedQueue<(MutableByteImage image, ISaveInfo info)> queue)
        {
            this.queue = queue;
        }

        private readonly IBoundedQueue<(MutableByteImage image, ISaveInfo info)> queue;
        public IVideoFrame Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            try
            {
                var item = this.queue.DequeueOrDefault();
                item.Wait();
                var frame = item.Result;
                if (frame == default)
                {
                    return false;
                }
                this.Current = new FfmpegVideoFrame(frame.image);
                return true;
            }
            catch
            {
                return false;
            }
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

        public string CustomArgs { get; set; }

        public string OutputFile { get; set; }
        public string PathToFfmpeg { get; set; }
    }
}
