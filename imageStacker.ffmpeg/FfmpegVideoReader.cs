using FFMpegCore;
using FFMpegCore.Pipes;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    public class FfmpegVideoReader : IImageReader<MutableByteImage>
    {
        private readonly FfmpegVideoReaderArguments _arguments;
        private readonly Logger _logger;
        private readonly IMutableImageFactory<MutableByteImage> _factory;
        private readonly CancellationTokenSource parsingFinished = new CancellationTokenSource();
        private bool isFinished = false;

        public FfmpegVideoReader(FfmpegVideoReaderArguments arguments, IMutableImageFactory<MutableByteImage> factory, Logger logger)
        {
            _arguments = arguments;
            _logger = logger;
            _factory = factory;
        }

        public async Task Produce(IBoundedQueue<MutableByteImage> queue)
        {
            try
            {
                FFMpegOptions.Configure(new FFMpegOptions
                {
                    //       RootDirectory = _arguments.PathToFfmpeg
                });

                var result = await FFProbe.AnalyseAsync(_arguments.InputFiles.First()).ConfigureAwait(false);

                var width = result.PrimaryVideoStream.Width;
                var height = result.PrimaryVideoStream.Height;
                var bpp = Image.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format24bppRgb) / 8;
                var pixelsPerFrame = width * height;

                var frameSizeInBytes = pixelsPerFrame * bpp;

                _logger.WriteLine("Input from ffmpeg currently only supports rgb24-convertable input", Verbosity.Warning);

                var chunksQueue = new ConcurrentQueue<byte[]>();
                using var memoryStream = new ChunkedSimpleMemoryStream(frameSizeInBytes, chunksQueue); // new MemoryStream(frameSizeInBytes);
                StreamPipeSink sink = new StreamPipeSink(memoryStream);
                var args = FFMpegArguments
                    .FromInputFiles(_arguments.InputFiles)
                    .DisableChannel(FFMpegCore.Enums.Channel.Audio)
                    .WithArgument(new FfmpegFpsArgument(_arguments.Framerate))
                    .UsingMultithreading(true)
                    .ForceFormat("rawvideo")
                    .ForcePixelFormat("bgr24")
                    .OutputToPipe(sink)
                    .NotifyOnProgress(
                        percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "InputVideoParsing"),
                        TimeSpan.FromSeconds(1));

                var produceTask = args.ProcessAsynchronously(true).ContinueWith((_) => parsingFinished.Cancel());
                var consumeTask = ParseInputStream(queue, chunksQueue, width, height, frameSizeInBytes, memoryStream);

                await Task.WhenAll(produceTask, consumeTask);

                //    await Task.WhenAny(produceTask, consumeTask).ConfigureAwait(false);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                _logger.WriteLine("Couldn't find ffmpeg", Verbosity.Error);
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }
        }

        private async Task ParseInputStream(IBoundedQueue<MutableByteImage> queue, ConcurrentQueue<byte[]> chunksQueue, int width, int height, int frameSizeInBytes, ChunkedSimpleMemoryStream memoryStream)
        {
            int count = 0;

            while (true)
            {
                try
                {
                    var (cancelled, item) = await chunksQueue.TryDequeueOrWait(parsingFinished);
                    if (cancelled) { break; }
                    _logger.NotifyFillstate(++count, "ParsedImages");
                    _logger.NotifyFillstate(chunksQueue.Count, "ChunkedQueue");
                    await queue.Enqueue(_factory.FromBytes(width, height, item));
                }
                catch (Exception e) { _logger.LogException(e); }
            }
            if (memoryStream.HasUnwrittenData)
            {
                _logger.WriteLine("Unwritten data exists after finishing parsings." +
                    " This indicates a severe issue with frame splitting", Verbosity.Warning);
            }
        }
    }
}