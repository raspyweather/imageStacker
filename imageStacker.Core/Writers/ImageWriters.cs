using imageStacker.Core.Abstraction;
using System.Threading.Tasks;

namespace imageStacker.Core.Writers

{

    public abstract class ImageWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        protected readonly ILogger logger;
        protected readonly IMutableImageFactory<T> factory;
        public ImageWriter(ILogger logger, IMutableImageFactory<T> factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public Task WaitForCompletion() => Task.CompletedTask;

        public abstract Task WriteFile(T image, ISaveInfo info);
    }
}
