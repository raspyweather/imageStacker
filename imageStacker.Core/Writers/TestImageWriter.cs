using imageStacker.Core.Abstraction;
using System.Threading.Tasks;

namespace imageStacker.Core.Writers

{
    public class TestImageWriter<T> : ImageWriter<T> where T : IProcessableImage
    {
        public TestImageWriter(ILogger logger, IMutableImageFactory<T> factory)
         : base(logger, factory)
        {
        }
        public override Task WriteFile(T image, ISaveInfo info)
        {
            return Task.CompletedTask;
        }
    }
}
