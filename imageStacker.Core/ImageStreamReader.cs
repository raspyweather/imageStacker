using imageStacker.Core.Abstraction;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public class ImageStreamReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        private readonly int Width, Height;
        private readonly PixelFormat Format;
        private readonly Stream InputStream;

        public ImageStreamReader(ILogger logger, IMutableImageFactory<T> factory, Stream inputStream, int width, int height, PixelFormat format = PixelFormat.Format24bppRgb)
            : base(logger, factory)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.InputStream = inputStream;
        }

        public async override Task Produce(IBoundedQueue<T> queue)
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

                    await queue.Enqueue(factory.FromImage(bm));
                }
                catch (Exception e) { logger.LogException(e); }
            }
            queue.CompleteAdding();
        }
    }
}
