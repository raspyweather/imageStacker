using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Readers
{
    public class ImageFileReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        private readonly Queue<string> filenames;
        private readonly BufferBlock<T> queue;

        public ImageFileReader(ILogger logger, IMutableImageFactory<T> factory, IImageReaderOptions options)
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

            var opts = new DataflowBlockOptions
            {
                BoundedCapacity = 16,
                EnsureOrdered = true
            };
            this.queue = new BufferBlock<T>(opts).WithLogging("ReadFile");
        }

        public async override Task Work()
        {
            foreach (var filename in filenames)
            {
                try
                {
                    using var bmp1 = new Bitmap(filename);
                    var height = bmp1.Height;
                    var width = bmp1.Width;
                    var pixelFormat = bmp1.PixelFormat;

                    var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                    var length = bmp1Data.Stride * bmp1Data.Height;

                    byte[] bmp1Bytes = new byte[length];
                    Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                    var image = factory.FromBytes(width, height, bmp1Bytes);
                    await queue.SendAsync(image);
                    bmp1.UnlockBits(bmp1Data);
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            queue.Complete();
        }

        public override ISourceBlock<T> GetSource()
        {
            return queue;
        }
    }
}