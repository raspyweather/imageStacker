using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.Core.Readers
{
    public class ImageMutliFileReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        public ImageMutliFileReader(ILogger ilogger, IMutableImageFactory<T> factory, string folderName, string filter = "*.JPG")
            : base(ilogger, factory)
        {
            this.filenames = new Queue<string>(Directory.GetFiles(folderName, filter, new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.System,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive
            }));
            Console.Error.WriteLine($"Items found: {filenames.Count}");
        }
        public ImageMutliFileReader(ILogger ilogger, IMutableImageFactory<T> factory, string[] files)
            : base(ilogger, factory)
        {
            this.filenames = new Queue<string>(files);
        }

        private const int processQueueLength = 16;
        private const int readQueueLength = 8;

        private readonly Queue<string> filenames;

        private readonly CancellationTokenSource readingFinished = new CancellationTokenSource();

        private readonly ConcurrentQueue<MemoryStream> dataToParse = new ConcurrentQueue<MemoryStream>();

        private async Task ReadFromDisk()
        {
            int i = 0;
            foreach (var filename in filenames)
            {
                try
                {
                    logger.NotifyFillstate(dataToParse.Count, "ReadBuffer");
                    logger.NotifyFillstate(i, "FilesRead");
                    dataToParse.Enqueue(new MemoryStream(File.ReadAllBytes(filename), false));
                    i++;
                }
                catch (Exception e) { Console.WriteLine(e); }
                while (dataToParse.Count > readQueueLength)
                {
                    await Task.Delay(100);
                    await Task.Yield();
                }
            }
            readingFinished.Cancel();
        }

        private async Task DecodeImage(ConcurrentQueue<T> queue)
        {
            while (!readingFinished.Token.IsCancellationRequested)
            {
                if (!dataToParse.TryDequeue(out var data))
                {
                    await Task.Delay(100);
                    await Task.Yield();
                    continue;
                }
                var bmp = new Bitmap(data);
                var width = bmp.Width;
                var height = bmp.Height;
                var pixelFormat = bmp.PixelFormat;

                var bmp1Data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);

                queue.Enqueue(factory.FromBytes(width, height, bmp1Bytes));

                bmp.UnlockBits(bmp1Data);
                bmp.Dispose();
                data.Dispose();
                logger.NotifyFillstate(dataToParse.Count, "ParseBuffer");

                while (queue.Count > processQueueLength)
                {
                    await Task.Delay(100);
                    await Task.Yield();
                }
            }
        }

        public override async Task Produce(ConcurrentQueue<T> queue)
        {
            var readingTask = Task.Run(() => ReadFromDisk());
            var decodingTask = Task.Run(() => DecodeImage(queue));

            var decodingTasks = Enumerable.Range(0, 4).Select(x => (Task)Task.Run(() => DecodeImage(queue)));

            await readingTask;
            await Task.WhenAll(decodingTasks);
            //await decodingTask;

            if (decodingTask.IsFaulted || readingTask.IsFaulted)
            {
                throw new Exception();
            }

        }
    }

}
