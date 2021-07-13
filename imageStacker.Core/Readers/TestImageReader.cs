
using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Readers
{
    public class TestImageReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        private readonly int width, height, count;
        private readonly PixelFormat format;
        private readonly byte[] data1, data2;
        private readonly BufferBlock<T> queue;

        public TestImageReader(int count, int width, int height, PixelFormat format, ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {
            this.width = width;
            this.height = height;
            this.format = format;
            this.count = count;

            var random = new Random();
            long length = width * 1L * height * Image.GetPixelFormatSize(format) / 8L;
            data1 = new byte[length];
            data2 = new byte[length];
            random.NextBytes(data1);
            random.NextBytes(data2);

            var opts = new DataflowBlockOptions
            {
                BoundedCapacity = 16,
                EnsureOrdered = true
            };
            this.queue = new BufferBlock<T>(opts).WithLogging("ReadFile");
        }

        public override ISourceBlock<T> GetSource()
        {
            return queue;
        }

        public override async Task Work()
        {
            for (int i = 0; i < count; i++)
            {
                logger.NotifyFillstate(i, "TestData");
                var data = new byte[data1.Length];
                ((i % 2 == 0) ? data1 : data2).CopyTo(data, 0);
                await queue.SendAsync(factory.FromBytes(width, height, data, format));
            }
            queue.Complete();
        }
    }
}