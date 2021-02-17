using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core
{
    public interface IImageReader<T> where T : IProcessableImage
    {
        public ISourceBlock<T> GetSource();
        public Task Work();
    }
}
