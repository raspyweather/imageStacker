using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public interface IImageProcessingStrategy
    {
        public Task Process(IImageReader reader, List<IFilter> filters, IImageWriter writer);
    }

    public class ThreadOrderedProcessingStrategy : IImageProcessingStrategy
    {
        public async Task Process(IImageReader reader, List<IFilter> filters, IImageWriter writer)
        {
            var readQueue = new BlockingCollection<IProcessableImage>(16);
            var produceTask = Task.Run(() => reader.Produce(readQueue));

            var filterWorkImages = new List<MutableImage>();

            var firstImage = readQueue.Take();
            for (int i = 0; i < filters.Count; i++)
            {
                filterWorkImages.Add(MutableImage.FromProcessableImage(firstImage));
            }

            while (!readQueue.IsCompleted)
            {
                var image = readQueue.Take();

                for (int i = 0; i < filters.Count; i++)
                {
                    var filter = filters[i];
                    var workImage = filterWorkImages[i];
                    filter.Process(workImage, image);
                }
            }

            await produceTask;

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var workImage = filterWorkImages[i];
                writer.WriteFile(workImage, new SaveInfo(null, filter.Name));
            }
        }
    }

    public class ThreadUnorderedProcessingStrategy : IImageProcessingStrategy
    {
        public async Task Process(IImageReader reader, List<IFilter> filters, IImageWriter writer)
        {
            var readQueue = new BlockingCollection<IProcessableImage>(16);
            var produceTask = Task.Run(() => reader.Produce(readQueue));

            var taskPerThread = new List<Task<List<IProcessableImage>>>();

            for (int threadIndex = 0; threadIndex < 8; threadIndex++)
            {
                taskPerThread.Add(Task.Factory.StartNew(() => Process(filters, readQueue)));
            }
            await Task.WhenAll(new[] { produceTask }.Concat(taskPerThread));

            for (int filterIndex = 0; filterIndex < filters.Count; filterIndex++)
            {
                var filter = filters[filterIndex];
                var workImage = MutableImage.FromProcessableImage(taskPerThread[0].Result[filterIndex]);

                for(int threadIndex = 1; threadIndex < taskPerThread.Count; threadIndex++)
                {
                    filter.Process(workImage, taskPerThread[threadIndex].Result[filterIndex]);
                }

                writer.WriteFile(workImage, new SaveInfo(null, filter.Name));
            }
        }

        private List<IProcessableImage> Process(IList<IFilter> filters, BlockingCollection<IProcessableImage> readQueue)
        {
            var filterWorkImages = new List<MutableImage>();

            var firstImage = readQueue.Take();
            for (int filterIndex = 0; filterIndex < filters.Count; filterIndex++)
            {
                filterWorkImages.Add(MutableImage.FromProcessableImage(firstImage));
            }

            while (!readQueue.IsCompleted)
            {
                var image = readQueue.Take();

                for (int i = 0; i < filters.Count; i++)
                {
                    var filter = filters[i];
                    var workImage = filterWorkImages[i];
                    filter.Process(workImage, image);
                }
            }

            return filterWorkImages.ToList<IProcessableImage>();
        }
    }
}
