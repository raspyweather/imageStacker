using imageStacker.Core.Abstraction;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Writers

{
    public class TestImageWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        private readonly ILogger logger;
        private readonly IMutableImageFactory<T> factory;
        private readonly ActionBlock<(T image, ISaveInfo saveInfo)> target;

        public TestImageWriter(ILogger logger, IMutableImageFactory<T> factory)
        {
            this.logger = logger;
            this.factory = factory;
            this.target = new ActionBlock<(T image, ISaveInfo saveInfo)>(x => { });
        }

        public ITargetBlock<(T image, ISaveInfo saveInfo)> GetTarget()
        {
            return this.target;
        }

        public async Task Work()
        {
            await this.target.Completion;
        }
    }
}
