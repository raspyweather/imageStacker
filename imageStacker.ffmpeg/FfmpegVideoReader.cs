using FFMpegCore;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoReader : IImageReader<MutableByteImage>
    {
        private readonly FfmpegVideoReaderArguments _arguments;
        private readonly ILogger _logger;
        private readonly IMutableImageFactory<MutableByteImage> _factory;
        private readonly TransformBlock<(int width, int height, byte[] data), MutableByteImage> transformBlock;

        public FfmpegVideoReader(FfmpegVideoReaderArguments arguments, IMutableImageFactory<MutableByteImage> factory, ILogger logger)
        {
            _arguments = arguments;
            _logger = logger;
            _factory = factory;

            var flowOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 16,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 8
            };

            transformBlock = new TransformBlock<(int width, int height, byte[] data), MutableByteImage>(x => GetImageFromBytes(x.width, x.height, x.data), flowOptions);
        }

        public ISourceBlock<MutableByteImage> GetSource()
        {
            return transformBlock;
        }

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

                var result = await FFProbe.AnalyseAsync(_arguments.InputFile).ConfigureAwait(false);

                var width = result.PrimaryVideoStream.Width;
                var height = result.PrimaryVideoStream.Height;

                _logger.WriteLine("Input from ffmpeg currently only supports rgb24-convertable input", Verbosity.Warning);

                var bpp = Image.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format24bppRgb) / 8;
                var pixelsPerFrame = width * height;

                var frameSizeInBytes = pixelsPerFrame * bpp;

                var sink = new RawImagePipeSink(frameSizeInBytes, async bytes => await transformBlock.SendAsync((width, height, bytes)));
                var args = FFMpegArguments
                    .FromFileInput(_arguments.InputFile)
                    .OutputToPipe(sink, options =>
                        options.DisableChannel(FFMpegCore.Enums.Channel.Audio)
                            .UsingMultithreading(true)
                            .ForceFormat("rawvideo")
                            .WithCustomArgument(_arguments.CustomArgs ?? string.Empty)
                            .ForcePixelFormat("bgr24"))
                    .NotifyOnProgress(
                        percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "InputVideoParsing"),
                        TimeSpan.FromSeconds(1));

                await args.ProcessAsynchronously();

                await transformBlock.Completion;

                _logger.WriteLine("finished reading", Verbosity.Info);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                _logger.WriteLine("Couldn't find ffmpeg", Verbosity.Error);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
        }

        private MutableByteImage GetImageFromBytes(int width, int height, byte[] bytes)
        {
            return _factory.FromBytes(width, height, bytes);
        }
    }
}