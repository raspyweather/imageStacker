using System.Collections.Generic;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public abstract class ProcessingStrategy<T> : IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public ProcessingStrategy(ILogger logger, IMutableImageFactory<T> factory)
        {
            this.factory = factory;
            this.logger = logger;
        }

        protected readonly ILogger logger;
        protected readonly IMutableImageFactory<T> factory;
        protected List<(IFilter<T> filter, T startImage)> Data { get; }

        public abstract Task Process(IImageReader<T> reader, List<IFilter<T>> filters, IImageWriter<T> writer);
    }
}
