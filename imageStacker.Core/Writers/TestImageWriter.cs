using imageStacker.Core.Abstraction;
using System.Threading.Tasks;

namespace imageStacker.Core.Writers

{
    public class TestImageWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        private readonly ILogger logger;
        private readonly IMutableImageFactory<T> factory;
        private IBoundedQueue<(T image, ISaveInfo info)> queue;

        public TestImageWriter(ILogger logger, IMutableImageFactory<T> factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public void SetQueue(IBoundedQueue<(T image, ISaveInfo info)> queue)
        {
            this.queue = queue;
        }

        public async Task WaitForCompletion()
        {
            while (true)
            {
                var (image, info) = await queue.DequeueOrDefault();
                if (image == null || info == null)
                {
                    break;
                }
            }
        }
    }
}
