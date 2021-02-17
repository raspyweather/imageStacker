using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Abstraction
{
    public interface IImageWriter<T> where T : IProcessableImage
    {
        public Task Work();
        ITargetBlock<(T image, ISaveInfo saveInfo)> GetTarget();
    }
}
