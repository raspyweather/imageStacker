using FFMpegCore;
using FFMpegCore.Pipes;
using imageStacker.Core;
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

        public async Task Produce(ConcurrentQueue<MutableByteImage> queue)
        {
            try
            {
                FFMpegOptions.Configure(new FFMpegOptions
                {
                    RootDirectory = @"C:\Users\armbe\Downloads\ffmpeg-4.3.1-full_build\bin"
                });

                var result = await FFProbe.AnalyseAsync(_arguments.InputFiles.First()).ConfigureAwait(false);

                var width = result.PrimaryVideoStream.Width;
                var height = result.PrimaryVideoStream.Height;
                var bpp = Image.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format24bppRgb) / 8;
                var pixelsPerFrame = width * height;

                var frameSizeInBytes = pixelsPerFrame * bpp;

                _logger.WriteLine("Input from ffmpeg currently only supports rgb24-convertable input", Verbosity.Warning);

                var chunksQueue = new ConcurrentQueue<byte[]>();
                using var memoryStream = new ChunkedMemoryStream(frameSizeInBytes, chunksQueue, _logger); // new MemoryStream(frameSizeInBytes);
                StreamPipeSink sink = new StreamPipeSink(memoryStream);
                var args = FFMpegArguments
                    .FromInputFiles(_arguments.InputFiles)
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
            catch (Exception e)
            {

            }

            async Task ParseInputStream(ConcurrentQueue<MutableByteImage> queue, ConcurrentQueue<byte[]> chunksQueue, int width, int height, int frameSizeInBytes, ChunkedMemoryStream memoryStream)
            {
                int count = 0;

                while (true)
                //while ((memoryStream.HasUnwrittenData || chunksQueue.Count > 0) && !parsingFinished.IsCancellationRequested)
                {
                    try
                    {
                        var foo = await chunksQueue.TryDequeueOrWait(parsingFinished);
                        if (foo.cancelled) { break; }
                        _logger.NotifyFillstate(++count, "ParsedImages");
                        _logger.NotifyFillstate(chunksQueue.Count, "ChunkedQueue");
                        queue.Enqueue(_factory.FromBytes(width, height, foo.item));

                    }
                    catch (Exception e) { _logger.LogException(e); }

                    await queue.WaitForBufferSpace(24);
                }
                Console.WriteLine(memoryStream.HasUnwrittenData);
            }
        }
    }

    public class FfmpegVideoReaderArguments
    {
        public double Framerate { get; set; }

        public string[] InputFiles { get; set; }
    }
}
