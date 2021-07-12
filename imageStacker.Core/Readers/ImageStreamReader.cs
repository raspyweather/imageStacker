using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Readers
{
    public class ImageStreamReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        private readonly int Width, Height;
        private readonly PixelFormat Format;
        private readonly Stream InputStream;
        private readonly BufferBlock<T> queue;

        public ImageStreamReader(ILogger logger, IMutableImageFactory<T> factory, Stream inputStream, int width, int height, PixelFormat format = PixelFormat.Format24bppRgb)
            : base(logger, factory)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.InputStream = inputStream;

            var opts = new DataflowBlockOptions
            {
                BoundedCapacity = 16,
                EnsureOrdered = true
            };
            this.queue = new BufferBlock<T>(opts).WithLogging("ReadStream");
        }

        public override async Task Work()
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

                    await queue.SendAsync(factory.FromImage(bm));
                }
                catch (Exception e) { logger.LogException(e); }
            }
            queue.Complete();
        }

        public override ISourceBlock<T> GetSource()
        {
            return queue;
        }
    }
}
