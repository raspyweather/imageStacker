using System.Threading.Tasks;

namespace imageStacker.Core.Abstraction
{
    public interface IImageWriter<T> where T : IProcessableImage
    {
        public Task WriteFile(T image, ISaveInfo info);

        public Task WaitForCompletion();
    }
}
