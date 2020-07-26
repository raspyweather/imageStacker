
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace imageStacker.Core.Readers
{
    public class TestImageReadeer<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        private readonly int width, height, count;
        private readonly PixelFormat format;
        private readonly byte[] data1, data2;
        public TestImageReadeer(int count, int width, int height, PixelFormat format, ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {
            this.width = width;
            this.height = height;
            this.format = format;
            this.count = count;

            var random = new Random();
            long length = width * 1L * height * Image.GetPixelFormatSize(format) / 8l;
            data1 = new byte[length];
            data2 = new byte[length];
            random.NextBytes(data1);
            random.NextBytes(data2);
        }

        public async override Task Produce(ConcurrentQueue<T> queue)
        {
            for (int i = 0; i < count; i++)
            {
                while (queue.Count > 8)
                {
                    await Task.Delay(50);
                }
                logger.NotifyFillstate(i, "TestData");
                var data = new byte[data1.Length];
                ((i % 2 == 0) ? data1 : data2).CopyTo(data, 0);
                queue.Enqueue(factory.FromBytes(width, height, data, format));
            }
        }
    }
}