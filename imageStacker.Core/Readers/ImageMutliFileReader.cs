using imageStacker.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace imageStacker.Core.Readers
{
    public class ImageMutliFileReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        public ImageMutliFileReader(ILogger logger, IMutableImageFactory<T> factory, IImageReaderOptions options)
      : base(logger, factory)
        {
            if (options?.Files != null && options.Files.Any())
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

        private const int readQueueLength = 8;

        private readonly Queue<string> filenames;

        private readonly IBoundedQueue<MemoryStream> dataToParse = BoundedQueueFactory.Get<MemoryStream>(readQueueLength,"In-ParQ");

        private async Task ReadFromDisk()
        {
            int i = 0;
            foreach (var filename in filenames)
            {
                try
                {
                    logger.NotifyFillstate(dataToParse.Count, "ReadBuffer");
                    logger.NotifyFillstate(i, "FilesRead");
                    await dataToParse.Enqueue(new MemoryStream(File.ReadAllBytes(filename), false));
                    i++;
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            dataToParse.CompleteAdding();
        }

        private async Task DecodeImage(IBoundedQueue<T> queue)
        {
            MemoryStream data;
            while ((data = await dataToParse.DequeueOrDefault()) != null)
            {
                var bmp = new Bitmap(data);
                var width = bmp.Width;
                var height = bmp.Height;
                var pixelFormat = bmp.PixelFormat;

                var bmp1Data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);

                await queue.Enqueue(factory.FromBytes(width, height, bmp1Bytes));

                bmp.UnlockBits(bmp1Data);
                bmp.Dispose();
                data.Dispose();
                logger.NotifyFillstate(dataToParse.Count, "ParseBuffer");
            }
        }

        public override async Task Produce(IBoundedQueue<T> queue)
        {
            var readingTask = Task.Run(() => ReadFromDisk());
            var decodingTasks = Task.WhenAll(Enumerable.Range(0, 6).Select(x => Task.Run(() => DecodeImage(queue)))).ContinueWith(t => queue.CompleteAdding());

            await Task.WhenAll(decodingTasks, readingTask);

            if (decodingTasks.IsFaulted || readingTask.IsFaulted)
            {
                throw decodingTasks.Exception
                      ?? readingTask.Exception
                      ?? new Exception("unkown stuff happened");
            }
        }
    }

}
