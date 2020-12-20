using FFMpegCore;
using FFMpegCore.Pipes;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using System;
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
                    RootDirectory = _arguments.PathToFfmpeg
                });

                var result = await FFProbe.AnalyseAsync(_arguments.InputFiles.First()).ConfigureAwait(false);

                var width = result.PrimaryVideoStream.Width;
                var height = result.PrimaryVideoStream.Height;
                var bpp = Image.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format24bppRgb) / 8;
                var pixelsPerFrame = width * height;

                var frameSizeInBytes = pixelsPerFrame * bpp;

                _logger.WriteLine("Input from ffmpeg currently only supports rgb24-convertable input", Verbosity.Warning);

                var chunksQueue = BoundedQueueFactory.Get<byte[]>(4);
                using var memoryStream = new ChunkedSimpleMemoryStream(frameSizeInBytes, chunksQueue); // new MemoryStream(frameSizeInBytes);
                StreamPipeSink sink = new StreamPipeSink(memoryStream);
                var args = FFMpegArguments
                    .FromInputFiles(_arguments.InputFiles)
                    .DisableChannel(FFMpegCore.Enums.Channel.Audio)
                    .UsingMultithreading(true)
                    .ForceFormat("rawvideo")
                    .ForcePixelFormat("bgr24")
                    .OutputToPipe(sink)
                    .NotifyOnProgress(
                        percent => _logger.NotifyFillstate(Convert.ToInt32(percent), "InputVideoParsing"),
                        TimeSpan.FromSeconds(1));

                var produceTask = args.ProcessAsynchronously(true).ContinueWith((_) =>
                {
                    chunksQueue.CompleteAdding();
                    queue.CompleteAdding();
                });
                var consumeTask = ParseInputStream(queue, chunksQueue, width, height, frameSizeInBytes, memoryStream)
                    .ContinueWith((_) => _logger.WriteLine("finished reading", Verbosity.Info));

                await Task.WhenAll(produceTask, consumeTask);

                _logger.WriteLine("finished reading", Verbosity.Info);
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

        private async Task ParseInputStream(IBoundedQueue<MutableByteImage> queue, IBoundedQueue<byte[]> chunksQueue, int width, int height, int frameSizeInBytes, ChunkedSimpleMemoryStream memoryStream)
        {
            int count = 0;

            while (true)
            {
                try
                {
                    var item = await chunksQueue.DequeueOrDefault();
                    if (item == default) { break; }

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