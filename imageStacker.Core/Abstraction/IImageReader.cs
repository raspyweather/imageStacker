using imageStacker.Core.Abstraction;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public interface IImageReader<T> where T : IProcessableImage
    {
        public Task Produce(IBoundedQueue<T> queue);
    }
}
