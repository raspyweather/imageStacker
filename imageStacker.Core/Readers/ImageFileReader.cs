using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
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
    public class ImageFileReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        public ImageFileReader(ILogger logger, IMutableImageFactory<T> factory, IImageReaderOptions options)
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

        private readonly Queue<string> filenames;

        public async override Task Produce(IBoundedQueue<T> queue)
        {
            foreach (var filename in filenames)
            {
                try
                {
                    logger.NotifyFillstate(queue.Count, this.GetType().Name);
                    var bmp1 = new Bitmap(filename);
                    var height = bmp1.Height;
                    var width = bmp1.Width;
                    var pixelFormat = bmp1.PixelFormat;

                    var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                    var length = bmp1Data.Stride * bmp1Data.Height;

                    byte[] bmp1Bytes = new byte[length];
                    Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                    var image = factory.FromBytes(width, height, bmp1Bytes);
                    await queue.Enqueue(image);
                    bmp1.UnlockBits(bmp1Data);
                    bmp1.Dispose();
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            queue.CompleteAdding();
        }
    }
}