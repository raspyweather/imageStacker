using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Core
{

    public class StackAllMergeStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public StackAllMergeStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        { }
        protected override async Task ProcessingThread(List<IFilter<T>> filters)
        {
            T firstMutableImage = await GetFirstImage();

            const int threadsToUtilize = 8;

            var jobs = new ConcurrentQueue<(IFilter<T> filter, T image)[]>();

            for (int i = 0; i < threadsToUtilize; i++)
            {
                jobs.Enqueue(filters.Select((filter, index) => (filter, image: factory.Clone(firstMutableImage))).ToArray());
            }

            var tasks = new ConcurrentQueue<Task>();

            while (true)
            {
                logger.NotifyFillstate(tasks.Count, "ProcessBuffer");
                while (tasks.Count >= 8)
                {
                    await Task.WhenAny(tasks);
                    tasks.Filter(task => !task.IsCompleted);
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

                if (jobs.TryDequeue(out var currentJob))
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        foreach (var (filter, image) in currentJob)
                        {
                            filter.Process(image, nextImage);
                        }
                    });
                    jobs.Enqueue(currentJob);
                    tasks.Enqueue(task);
                }

                logger.NotifyFillstate(tasks.Count, "ProcessBuffer");
            }

            await Task.WhenAll(tasks);

            if (!jobs.TryDequeue(out var previousJob)) { throw new Exception("illegal state?"); }
            while (jobs.TryDequeue(out var job))
            {
                previousJob.Zip(job, (first, second) => { first.filter.Process(second.image, first.image); return 0; }).ToArray();
                previousJob = job;
            }

            previousJob.ToList().ForEach(data =>
            {
                outputQueue.Enqueue((data.image, new SaveInfo(null, data.filter.Name)));
            });
        }
    }

    public class StackContinousStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        private readonly int StackCount;

        public StackContinousStrategy(int stackCount, ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {
            StackCount = stackCount;
        }

        protected override async Task ProcessingThread(List<IFilter<T>> filters)
        {
            var bufferQueue = new Queue<(IFilter<T> filter, T image, Task task, int appliedImages, int startIndex)>();
            int maxSize = filters.Count * StackCount;
            for (int i = 0; true; i++)
            {
                if (outputQueue.Count > 16)
                {
                    await Task.Delay(100);
                    await Task.Yield();
                    continue;
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

                for (int ii = 0; ii < bufferQueue.Count; ii++)
                {
                    var data = bufferQueue.Dequeue();
                    data.task = Task.Run(() => data.filter.Process(data.image, nextImage));
                    data.appliedImages++;
                    bufferQueue.Enqueue(data);
                }

                while (maxSize <= bufferQueue.Count)
                {
                    var (filter, image, t, appliedImages, startIndex) = bufferQueue.Dequeue();
                    await t;
                    outputQueue.Enqueue((image, new SaveInfo(startIndex, filter.Name)));
                }

                filters.ForEach(filter =>
                {
                    bufferQueue.Enqueue((filter, image: nextImage, Task.CompletedTask, 0, i));
                });
            }

            while (0 <= bufferQueue.Count)
            {
                var (filter, image, t, appliedImages, startindex) = bufferQueue.Dequeue();
                await t;
                outputQueue.Enqueue((image, new SaveInfo(startindex, filter.Name)));
            }
        }
    }

    public class StackProgressiveStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public StackProgressiveStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {

        }
        protected override async Task ProcessingThread(List<IFilter<T>> filters)
        {
            T firstData = await GetFirstImage();
            var baseImages = filters.Select((filter, index) => (filter, image: firstData, index)).ToList();

            for (int i = 0; true; i++)
            {
                if (!inputQueue.TryDequeue(out var nextImage))
                {
                    if (inputFinishedToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(100);

                    continue;
                }

                while (outputQueue.Count > 64)
                {
                    await Task.Yield();
                    await Task.Delay(100);
                }

                baseImages.AsParallel()
                    .ForAll(data =>
                    {
                        data.filter.Process(data.image, nextImage);
                        outputQueue.Enqueue((factory.Clone(data.image), new SaveInfo(i, data.filter.Name)));
                    });
            }
        }
    }

    public static class ConcurrentQueueExtension
    {
        public static void Filter<T>(this ConcurrentQueue<T> me, Func<T, bool> filterFunc)
        {
            for (int i = 0; i < me.Count; i++)
            {
                me.TryDequeue(out T item);
                if (filterFunc(item))
                {
                    me.Enqueue(item);
                }
            }
        }
    }
}
