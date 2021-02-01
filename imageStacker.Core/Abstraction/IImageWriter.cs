using System;
using System.Threading.Tasks;

namespace imageStacker.Core.Abstraction
{
    public interface IImageWriter<T> where T : IProcessableImage
    {
        public void SetQueue(IBoundedQueue<(T image, ISaveInfo info)> queue);

        public Task WaitForCompletion();
    }
}
