using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public interface IImageReader<T> where T : IProcessableImage
    {
        public Task Produce(ConcurrentQueue<T> queue);
    }
}
