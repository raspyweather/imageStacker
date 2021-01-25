using imageStacker.Core.Abstraction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace imageStacker.Core.Readers
{
    public class ImageMutliFileOrderedReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        public ImageMutliFileOrderedReader(ILogger logger, IMutableImageFactory<T> factory, IImageReaderOptions options)
            : base(logger, factory)
        {
            if (options?.Files != null && options.Files.Count() > 0)
            {
                this.filenames = new Queue<string>(options.Files);
            }
            else
            {
                this.filenames = new Queue<string>(Directory.GetFiles(options.FolderName, options.Filter, new EnumerationOptions
                {
                    AttributesToSkip = FileAttributes.System,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                }));
            }

            this.logger.WriteLine($"Items found: {filenames.Count}", Verbosity.Info);
        }

        private const int publishQueueLength = 8;
        private const int readQueueLength = 24;

        private readonly Queue<string> filenames;

        private readonly IBoundedQueue<(MemoryStream, int)> dataToParse = BoundedQueueFactory.Get<(MemoryStream, int)>(readQueueLength);
        private readonly IBoundedQueue<(T, int)> dataToPublish = BoundedQueueFactory.Get<(T, int)>(publishQueueLength);
        private async Task ReadFromDisk()
        {
            int i = 0;
            foreach (var filename in filenames)
            {
                try
                {
                    logger.NotifyFillstate(dataToParse.Count, "ReadBuffer");
                    logger.NotifyFillstate(i, "FilesRead");
                    await dataToParse.Enqueue((new MemoryStream(File.ReadAllBytes(filename), false), i));
                    i++;
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            dataToParse.CompleteAdding();
        }

        private async Task DecodeImage(IBoundedQueue<(T, int)> queue)
        {
            (MemoryStream, int) data;
            while ((data = await dataToParse.DequeueOrDefault()).Item1 != null)
            {
                try
                {
                    using var bmp = new Bitmap(data.Item1);
                    var width = bmp.Width;
                    var height = bmp.Height;
                    var pixelFormat = bmp.PixelFormat;

                    var bmp1Data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                    var length = bmp1Data.Stride * bmp1Data.Height;

                    byte[] bmp1Bytes = new byte[length];
                    Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);

                    await queue.Enqueue((factory.FromBytes(width, height, bmp1Bytes), data.Item2));

                    bmp.UnlockBits(bmp1Data);
                    data.Item1.Dispose();
                    logger.NotifyFillstate(dataToParse.Count, "ReadBuffer");
                }
                catch (Exception e) { logger.LogException(e); continue; }
            }
            queue.CompleteAdding();
        }

        private async Task Publish(IBoundedQueue<T> queue)
        {
            int nextImageIndex = 0;

            var reassembleDict = new ConcurrentDictionary<int, T>();

            (T, int) result;
            while ((result = await dataToPublish.DequeueOrDefault()).Item1 != null)
            {
                logger.NotifyFillstate(dataToPublish.Count, "ParsedBuffer");
                logger.NotifyFillstate(reassembleDict.Count, "ReassembleBuffer");

                reassembleDict[result.Item2] = result.Item1;

                while (reassembleDict.TryRemove(nextImageIndex, out var image))
                {
                    nextImageIndex++;
                    await queue.Enqueue(image);
                }
            }
            while (reassembleDict.TryRemove(nextImageIndex, out var image))
            {
                nextImageIndex++;
                await queue.Enqueue(image);
            }
            logger.NotifyFillstate(reassembleDict.Count, "ReassembleBuffer");
            queue.CompleteAdding();
        }

        public async override Task Produce(IBoundedQueue<T> queue)
        {
            var readingTask = Task.Run(() => ReadFromDisk());
            var decodingTasks = Task.WhenAll(Enumerable.Range(0, 6).Select(x => Task.Run(() => DecodeImage(dataToPublish)))).ContinueWith((t) => dataToPublish.CompleteAdding());
            var orderingTask = Task.Run(() => Publish(queue));

            await Task.WhenAll(decodingTasks, orderingTask, readingTask);

            if (decodingTasks.IsFaulted || readingTask.IsFaulted || orderingTask.IsFaulted)
            {
                throw decodingTasks.Exception
                      ?? readingTask.Exception
                      ?? orderingTask.Exception
                      ?? new Exception("unkown stuff happened");
            }

        }
    }

}
