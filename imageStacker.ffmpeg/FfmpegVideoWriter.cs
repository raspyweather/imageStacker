using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FFMpegCore;
using FFMpegCore.Pipes;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoWriter : IImageWriter<MutableImage>
    {
        public FfmpegVideoWriter(FfmpegVideoWriterArguments arguments, ILogger logger)
        {
            _arguments = arguments;
            _logger = logger;

            var opts = new DataflowBlockOptions
            {
                BoundedCapacity = 16,
                EnsureOrdered = true
            };
            this.queue = new BufferBlock<(MutableImage image, ISaveInfo info)>(opts).WithLogging("WriteVideo");
        }

        private readonly ILogger _logger;

        private readonly BufferBlock<(MutableImage image, ISaveInfo info)> queue;

        private readonly FfmpegVideoWriterArguments _arguments;

        public async Task Work()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_arguments.PathToFfmpeg))
                {
                    FFMpegOptions.Configure(new FFMpegOptions
                    {
                        RootDirectory = _arguments.PathToFfmpeg
                    });
                }

                var source = new RawVideoPipeSource(new MutableByteImageBoundedQueueEnumerator(queue));

                var args = FFMpegArguments
                .FromPipeInput(source, args =>
                {
                })
                .OutputToFile(_arguments.OutputFile, true,
                options => options.WithFramerate(_arguments.Framerate)
                    .UsingMultithreading(true)
                    .UsingThreads(Environment.ProcessorCount)
                    .UsePreset(_arguments.Preset)
                    .OverwriteExisting()
                    .WithCustomArgument(_arguments.CustomArgs))
                .NotifyOnProgress(
                    percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "OutputVideoEncoding"),
                    TimeSpan.FromSeconds(1));

                await args.ProcessAsynchronously(true);
                _logger.WriteLine("finished writing", Verbosity.Info);
                queue.Complete();
            }
            catch (Exception)
            {

            }
        }

        public ITargetBlock<(MutableImage image, ISaveInfo saveInfo)> GetTarget()
        {
            return queue;
        }
    }

    public class MutableByteImageBoundedQueueEnumerator : IEnumerator<IVideoFrame>
    {
        public MutableByteImageBoundedQueueEnumerator(ISourceBlock<(MutableImage image, ISaveInfo info)> queue)
        {
            this.queue = queue;
        }

        private readonly ISourceBlock<(MutableImage image, ISaveInfo info)> queue;
        public IVideoFrame Current { get; private set; }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            try
            {
                var (image, _) = this.queue.Receive();
                this.Current = new FfmpegVideoFrame(image);
                return true;
            }
            catch (InvalidOperationException)
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

        public FfmpegVideoEncoderPreset Preset { get; set; }
    }

    public enum FfmpegVideoEncoderPreset
    {
        None = 0,
        FullHD,
        HalfSize,
        FourK,
        Archive,
    }
}
