using System;
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
            var produceTask = reader.Produce(readQueue);

            var taskPerThread = new List<Task<List<IProcessableImage>>>();

            for (int threadIndex = 0; threadIndex < 8; threadIndex++)
            {
                taskPerThread.Add(Task.Factory.StartNew(() => Process(filters, readQueue), TaskCreationOptions.LongRunning));
            }
            await Task.WhenAll(new[] { produceTask }.Concat(taskPerThread));
            Console.WriteLine("Multi-Processing done");

            Parallel.For(0, filters.Count, filterIndex =>
            {
                var filter = filters[filterIndex];
                var workImage = taskPerThread[0].Result[filterIndex] as MutableImage;

                for (int threadIndex = 1; threadIndex < taskPerThread.Count; threadIndex++)
                {
                    filter.Process(workImage, taskPerThread[threadIndex].Result[filterIndex]);
                }

                writer.WriteFile(workImage, new SaveInfo(null, filter.Name));
            });

            Console.WriteLine("Single-Processing done");
        }

        private List<IProcessableImage> Process(IList<IFilter> filters, BlockingCollection<IProcessableImage> readQueue)
        {
            var filterArray = filters.ToArray();
            var filterCount = filterArray.Length;
            var filterWorkImages = new MutableImage[filterCount];

            var firstImage = readQueue.Take();
            for (int filterIndex = 0; filterIndex < filterCount; filterIndex++)
            {
                filterWorkImages[filterIndex] = MutableImage.FromProcessableImage(firstImage);
            }

            foreach (var image in readQueue.GetConsumingEnumerable())
            {
                for (int i = 0; i < filterCount; i++)
                {
                    var filter = filterArray[i];
                    var workImage = filterWorkImages[i];
                    filter.Process(workImage, image);
                }
            }

            return filterWorkImages.ToList<IProcessableImage>();
        }
    }
}
