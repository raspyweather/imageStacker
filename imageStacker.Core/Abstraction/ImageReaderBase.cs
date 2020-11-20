using imageStacker.Core.Abstraction;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public abstract class ImageReaderBase<T> : IImageReader<T> where T : IProcessableImage
    {
        protected readonly ILogger logger;
        protected readonly IMutableImageFactory<T> factory;
        public ImageReaderBase(ILogger logger, IMutableImageFactory<T> factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public abstract Task Produce(IBoundedQueue<T> queue);
    }

}
