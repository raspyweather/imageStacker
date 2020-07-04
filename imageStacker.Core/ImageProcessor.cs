using Microsoft.VisualBasic;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public interface IImageProcessingStrategy
    {
        public Task Process(IImageReader reader, List<IFilter> filters, IImageWriter writer);
    }

    public abstract class BasicProcessingStrategy : IImageProcessingStrategy
    {
        protected List<(IFilter filter, MutableImage startImage)> Data { get; }

        public abstract Task Process(IImageReader reader, List<IFilter> filters, IImageWriter writer);
    }

    public abstract class ThreadProcessingStrategy : BasicProcessingStrategy
    {
        protected readonly ConcurrentQueue<IProcessableImage> inputQueue = new ConcurrentQueue<IProcessableImage>();
        protected readonly ConcurrentQueue<(IProcessableImage image, ISaveInfo saveInfo)> outputQueue = new ConcurrentQueue<(IProcessableImage image, ISaveInfo saveInfo)>();
        protected readonly CancellationTokenSource
            inputFinishedToken = new CancellationTokenSource(),
            outputFinishedToken = new CancellationTokenSource();


        protected async Task ReadingThread(IImageReader reader)
        {
            await reader.Produce(inputQueue);
            inputFinishedToken.Cancel();
            Console.WriteLine("input finished");
        }
        protected async Task WritingThread(IImageWriter writer)
        {
            while (true)
            {
                Logger.loggerInstance.NotifyFillstate(outputQueue.Count, "WriteBuffer");
                if (!outputQueue.TryDequeue(out var imageInfo))
                {
                    if (outputFinishedToken.Token.IsCancellationRequested)
                    {
                        // finished
                        break;
                    }
                    await Task.Delay(100);
                    continue;
                }
                writer.Writefile(imageInfo.image, imageInfo.saveInfo);
            }
        }
        protected async Task<IProcessableImage> GetFirstImage()
        {
            while (true)
            {
                if (!inputQueue.TryDequeue(out var firstData))
                {
                    if (inputFinishedToken.IsCancellationRequested)
                    {
                        break;
                    }
                    //await Task.Delay(100);
                    continue;
                }
                return firstData;
            }
            throw new InvalidOperationException("No Image could be found");
        }

        protected abstract Task ProcessingThread(List<IFilter> filter);

        public override async Task Process(IImageReader reader, List<IFilter> filters, IImageWriter writer)
        {
            var readThread = ReadingThread(reader);
            readThread.ConfigureAwait(false);
            //     var readThread = Task.Factory.StartNew(() => ReadingThread(reader));
            var processThread = Task.Factory.StartNew(() => ProcessingThread(filters).ContinueWith(_ => outputFinishedToken.Cancel()));
            var writeThread = Task.Factory.StartNew(() => WritingThread(writer));

            Console.WriteLine("threads started");

            await Task.WhenAll(readThread, processThread, writeThread);
        }
    }

    public class StackAllStrategy : ThreadProcessingStrategy, IImageProcessingStrategy
    {
        protected override async Task ProcessingThread(List<IFilter> filters)
        {
            IProcessableImage firstData = await GetFirstImage();
            var firstMutableImage = MutableImage.FromProcessableImage(firstData);
            var baseImages = filters.Select((filter, index) => (filter, image: firstMutableImage.Clone(), index)).ToList();

            var tasks = new ConcurrentQueue<Task>();

            while (true)
            {
                while (tasks.Count > 10)
                {
                    await Task.WhenAny(tasks);
                }
                if (!inputQueue.TryDequeue(out var nextImage))
                {
                    if (inputFinishedToken.IsCancellationRequested)
                    {
                        Console.WriteLine("cancellation requested");
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
            Console.WriteLine("all tasks done");
            baseImages.ForEach(data => outputQueue.Enqueue((data.image, new SaveInfo(null, data.filter.Name))));
        }
    }

    public class StackAllMergeStrategy : ThreadProcessingStrategy, IImageProcessingStrategy
    {
        protected override async Task ProcessingThread(List<IFilter> filters)
        {
            IProcessableImage firstData = await GetFirstImage();
            var firstMutableImage = MutableImage.FromProcessableImage(firstData);

            const int threadsToUtilize = 8;

            var jobs = new ConcurrentQueue<(IFilter filter, MutableImage image)[]>();

            for (int i = 0; i < threadsToUtilize; i++)
            {
                jobs.Enqueue(filters.Select((filter, index) => (filter, image: firstMutableImage.Clone())).ToArray());
            }

            var tasks = new ConcurrentQueue<Task>();

            while (true)
            {
                Logger.loggerInstance.NotifyFillstate(tasks.Count, "ProcessBuffer");
                while (tasks.Count >= 8)
                {
                    await Task.WhenAny(tasks);
                    tasks.Filter(task => !task.IsCompleted);
                }
                if (!inputQueue.TryDequeue(out var nextImage))
                {
                    if (inputFinishedToken.IsCancellationRequested)
                    {
                        Console.WriteLine("cancellation requested");
                        break;
                    }
                 //   Console.WriteLine("nothing to process");
                    await Task.Delay(100);
                    await Task.Yield();
                    continue;
                }

                jobs.TryDequeue(out var currentJob);
                var task = Task.Factory.StartNew(() =>
               {
                   foreach (var (filter, image) in currentJob)
                   {
                       filter.Process(image, nextImage);
                   }
               });
                jobs.Enqueue(currentJob);
                tasks.Enqueue(task);

                Logger.loggerInstance.NotifyFillstate(tasks.Count, "ProcessBuffer");

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
            Console.WriteLine("done");

        }
    }

    public class StackContinousStrategy : ThreadProcessingStrategy, IImageProcessingStrategy
    {
        private readonly int StackCount;

        public StackContinousStrategy(int stackCount)
        {
            StackCount = stackCount;
        }

        protected override async Task ProcessingThread(List<IFilter> filters)
        {
            var bufferQueue = new Queue<(IFilter filter, MutableImage image)>();
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

                bufferQueue.AsParallel().ForAll(data => data.filter.Process(data.image, nextImage));

                if (bufferQueue.Count == StackCount)
                {
                    var (filter, image) = bufferQueue.Dequeue();
                    outputQueue.Enqueue((image, new SaveInfo(i, filter.Name)));
                }

                filters.ForEach(filter =>
                {
                    bufferQueue.Enqueue((filter, image: MutableImage.FromProcessableImage(nextImage)));
                });
            }
        }
    }

    public class StackProgressiveStrategy : ThreadProcessingStrategy, IImageProcessingStrategy
    {
        protected override async Task ProcessingThread(List<IFilter> filters)
        {
            IProcessableImage firstData = await GetFirstImage();
            var baseImages = filters.Select((filter, index) => (filter, image: MutableImage.FromProcessableImage(firstData), index)).ToList();

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

                baseImages.AsParallel()
                    .ForAll(data =>
                    {
                        data.filter.Process(data.image, nextImage);
                        outputQueue.Enqueue((data.image.Clone(), new SaveInfo(i, data.filter.Name)));
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
