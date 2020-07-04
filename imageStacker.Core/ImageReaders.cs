using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace imageStacker.Core
{

    public interface IImageReader
    {
        public Task Produce(BlockingCollection<IProcessableImage> queue);
    }

    public class ImageStreamReader : IImageReader
    {
        private readonly int Width, Height;
        private readonly PixelFormat Format;
        private readonly Stream InputStream;

        public ImageStreamReader(Stream inputStream, int width, int height, PixelFormat format = PixelFormat.Format24bppRgb)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.InputStream = inputStream;
        }
        public async Task Produce(BlockingCollection<IProcessableImage> queue)
        {
            var bytesToRead = Width * Height * Image.GetPixelFormatSize(Format);
            while (this.InputStream.CanRead)
            {
                try
                {
                    var bm = new Bitmap(
                        Width,
                        Height,
                        Width,
                        Format,
                        Marshal.UnsafeAddrOfPinnedArrayElement(this.InputStream.ReadBytes(bytesToRead), 0));

                    queue.Add(MutableImage.FromImage(bm));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

    internal static class StreamHelper
    {
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            using var ms = new MemoryStream(count);
            stream.CopyTo(ms, count);
            return ms.ToArray();
        }
    }

    public class ImageFileReader : IImageReader
    {
        public ImageFileReader(string folderName, string filter = "*.JPG")
        {
            this.filenames = new Queue<string>(Directory.GetFiles(folderName, filter, new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.System,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive
            }));
            Console.Error.WriteLine($"Items found: {filenames.Count}");
        }
        public ImageFileReader(string[] files)
        {
            this.filenames = new Queue<string>(files);
        }

        private readonly Queue<string> filenames;

        public async Task Produce(BlockingCollection<IProcessableImage> queue)
        {
            int i = 0;
            foreach (var filename in filenames)
            {
                try
                {
                    Logger.loggerInstance.NotifyFillstate(queue.Count, this.GetType().Name);
                    var bmp1 = new Bitmap(filename);
                    var height = bmp1.Height;
                    var width = bmp1.Width;
                    var pixelFormat = bmp1.PixelFormat;

                    var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                    var length = bmp1Data.Stride * bmp1Data.Height;

                    byte[] bmp1Bytes = new byte[length];
                    Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                    var image = MutableImage.FromBytes(width, height, bmp1Bytes);
                    queue.Add(image);
                    bmp1.UnlockBits(bmp1Data);
                    bmp1.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }

    public class ImageMutliFileReader : IImageReader
    {
        public ImageMutliFileReader(string folderName, string filter = "*.JPG")
        {
            this.filenames = new Queue<string>(Directory.GetFiles(folderName, filter, new EnumerationOptions
            {
                AttributesToSkip = FileAttributes.System,
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive
            }));
            Console.Error.WriteLine($"Items found: {filenames.Count}");
        }
        public ImageMutliFileReader(string[] files)
        {
            this.filenames = new Queue<string>(files);
        }

        private const int readQueueLength = 8;
        private readonly Queue<string> filenames;

        private readonly BlockingCollection<MemoryStream> rawImageQueue = new BlockingCollection<MemoryStream>(readQueueLength);

        private void ReadFromDisk()
        {
            int i = 0;
            Parallel.ForEach(filenames, filename =>
            {
                try
                {
                    Logger.loggerInstance.NotifyFillstate(rawImageQueue.Count, "ReadBuffer");
                    Logger.loggerInstance.NotifyFillstate(i, "FilesRead");
                    rawImageQueue.Add(new MemoryStream(File.ReadAllBytes(filename), false));
                    i++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
            rawImageQueue.CompleteAdding();
            Console.WriteLine("Finished reading" + i.ToString());
        }

        private void DecodeImage(BlockingCollection<IProcessableImage> decodedImageQueue)
        {
            foreach(var data in rawImageQueue.GetConsumingEnumerable())
            {
                var bmp = new Bitmap(data);
                var width = bmp.Width;
                var height = bmp.Height;
                var pixelFormat = bmp.PixelFormat;

                var bmp1Data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);

                decodedImageQueue.Add(MutableImage.FromBytes(width, height, bmp1Bytes));

                bmp.UnlockBits(bmp1Data);
                bmp.Dispose();
                data.Dispose();
                Logger.loggerInstance.NotifyFillstate(decodedImageQueue.Count, "ParseBuffer");
            }
            Console.WriteLine("finished decoding");
        }

        public async Task Produce(BlockingCollection<IProcessableImage> decodedQueue)
        {
            var readingTask = Task.Factory.StartNew(() => ReadFromDisk(), TaskCreationOptions.LongRunning);
            var decodingTasks = Enumerable.Range(0, 6).Select(x => Task.Factory.StartNew(() => DecodeImage(decodedQueue), TaskCreationOptions.LongRunning));

            await Task.WhenAll(new[] { readingTask }.Concat(decodingTasks));
            decodedQueue.CompleteAdding();
        }
    }

}
