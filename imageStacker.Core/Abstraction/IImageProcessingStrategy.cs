using imageStacker.Core.Abstraction;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public interface IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public Task Process(IImageReader<T> reader, List<IFilter<T>> filters, IImageWriter<T> writer);
    }
}
