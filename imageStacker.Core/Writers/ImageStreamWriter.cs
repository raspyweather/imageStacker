using imageStacker.Core.Abstraction;
using System;
using System.IO;
using System.Threading.Tasks;

namespace imageStacker.Core.Writers

{
    /// <summary>
    /// Outputs raw RGB Byte Stream
    /// </summary>
    public class ImageStreamWriter<T> : ImageWriter<T>, IDisposable where T : IProcessableImage
    {
        private readonly Stream outputStream;
        public ImageStreamWriter(ILogger logger, IMutableImageFactory<T> factory, Stream outputStream)
            : base(logger, factory)
        {
            this.outputStream = outputStream;
        }

        public void Dispose()
        {
            outputStream?.Close();
        }

        public override Task WriteFile(T image, ISaveInfo info)
        {
            var imageAsBytes = factory.ToBytes(image);
            outputStream.Write(imageAsBytes, 0, imageAsBytes.Length);
            image = default(T);
            return Task.CompletedTask;
        }

        ~ImageStreamWriter()
        {
            outputStream?.Close();
        }
    }
}
