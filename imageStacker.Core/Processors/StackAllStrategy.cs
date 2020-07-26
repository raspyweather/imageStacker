using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public class StackAllStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        private const int tasksCount = 4;

        public StackAllStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        { }

        protected override async Task ProcessingThread(List<IFilter<T>> filters)
        {
            T firstMutableImage = await GetFirstImage();
            var baseImages = filters.Select((filter, index) => (filter, image: factory.Clone(firstMutableImage), index)).ToList();

            var tasks = new ConcurrentQueue<Task>();

            while (true)
            {
                while (tasks.Count > tasksCount)
                {
                    await Task.WhenAny(tasks);
                }
                if (!inputQueue.TryDequeue(out var nextImage))
                {
                    if (inputFinishedToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(100);
                    await Task.Yield();
                    continue;
                }

                baseImages.ForEach(data =>
                {
                    var task = Task.Factory.StartNew(() => data.filter.Process(data.image, nextImage));
                    tasks.Enqueue(task);
                    task.Start();
                });
            }

            await Task.WhenAll(tasks);
            baseImages.ForEach(data => outputQueue.Enqueue((data.image, new SaveInfo(null, data.filter.Name))));
        }
    }
}
