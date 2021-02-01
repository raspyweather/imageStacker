using imageStacker.Core.Abstraction;
using System;
using System.IO;
using System.Threading.Tasks;

namespace imageStacker.Core.Writers

{
    /// <summary>
    /// Outputs raw RGB Byte Stream
    /// </summary>
    public class ImageStreamWriter<T> : IImageWriter<T>, IDisposable where T : IProcessableImage
    {
        private readonly Stream outputStream;
        private IBoundedQueue<(T image, ISaveInfo info)> queue;
        private readonly ILogger logger;
        private readonly IMutableImageFactory<T> factory;

        public ImageStreamWriter(ILogger logger, IMutableImageFactory<T> factory, Stream outputStream)
        {
            this.outputStream = outputStream;
            this.logger = logger;
            this.factory = factory;
        }

        public void Dispose()
        {
            outputStream?.Close();
        }

        public async Task WaitForCompletion()
        {
            while (true)
            {
                var (image, info) = await queue.DequeueOrDefault();
                if (image == null || info == null)
                {
                    break;
                }

                var imageAsBytes = factory.ToBytes(image);
                await outputStream.WriteAsync(imageAsBytes.AsMemory(0, imageAsBytes.Length));
            }
        }

        public void SetQueue(IBoundedQueue<(T image, ISaveInfo info)> queue)
        {
            this.queue = queue;
        }

        ~ImageStreamWriter()
        {
            outputStream?.Close();
        }
    }
}
