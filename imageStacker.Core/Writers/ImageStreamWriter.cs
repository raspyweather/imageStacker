using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Writers
{
    /// <summary>
    /// Outputs raw RGB Byte Stream
    /// </summary>
    public class ImageStreamWriter<T> : IImageWriter<T>, IDisposable where T : IProcessableImage
    {
        private readonly Stream outputStream;
        private readonly IMutableImageFactory<T> factory;
        private readonly ActionBlock<(T image, ISaveInfo info)> actionBlock;

        public ImageStreamWriter(ILogger logger, IMutableImageFactory<T> factory, Stream outputStream)
        {
            this.outputStream = outputStream;
            this.factory = factory;
            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 16,
                EnsureOrdered = true,
                MaxDegreeOfParallelism = 1
            };
            this.actionBlock = new ActionBlock<(T image, ISaveInfo info)>(WriteImage, options).WithLogging("WriteStream");
        }

        public void Dispose()
        {
            outputStream?.Close();
        }

        public async Task Work()
        {
            await this.actionBlock.Completion;
        }

        private async Task WriteImage((T image, ISaveInfo info) imageItem)
        {
            var imageAsBytes = factory.ToBytes(imageItem.image); //TODO: do this in a BufferBlock once ToBytes does anything performance-critical
            await outputStream.WriteAsync(imageAsBytes);
        }

        public ITargetBlock<(T image, ISaveInfo saveInfo)> GetTarget()
        {
            return actionBlock;
        }

        ~ImageStreamWriter()
        {
            outputStream?.Close();
        }
    }
}
