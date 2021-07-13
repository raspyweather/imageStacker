﻿using imageStacker.Core.Abstraction;
using imageStacker.Core.Writers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core
{
    public class StackAllMergeStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public StackAllMergeStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        { }
        protected override async Task ProcessingThread(List<IFilter<T>> filters, ISourceBlock<T> inputQueue, ITargetBlock<(T image, ISaveInfo saveInfo)> outputQueue)
        {
            T firstMutableImage = await inputQueue.ReceiveAsync();

            const int threadsToUtilize = 16;

            var jobs = new ConcurrentQueue<(IFilter<T> filter, T image)[]>();

            for (int i = 0; i < threadsToUtilize; i++)
            {
                jobs.Enqueue(filters.Select((filter, index) => (filter, image: factory.Clone(firstMutableImage))).ToArray());
            }

            var tasks = new ConcurrentQueue<Task>();

            while (true)
            {
                T nextImage;
                try
                {
                    nextImage = await inputQueue.ReceiveAsync();
                }
                catch (InvalidOperationException)
                {
                    break;
                }
                logger.NotifyFillstate(tasks.Count, "ProcessBuffer");
                while (tasks.Count >= 8)
                {
                    await Task.WhenAny(tasks);
                    tasks.Filter(task => !task.IsCompleted);
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

            foreach (var (filter, image) in previousJob)
            {
                await outputQueue.SendAsync((image, new SaveInfo(null, filter.Name)));
            }

            outputQueue.Complete();
        }
    }

    public class CopyStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public CopyStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        { }

        protected override async Task ProcessingThread(List<IFilter<T>> filter, ISourceBlock<T> inputQueue, ITargetBlock<(T image, ISaveInfo saveInfo)> outputQueue)
        {
            int idx = 0;
            while (true)
            {
                T image;
                try
                {
                    image = await inputQueue.ReceiveAsync();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                await outputQueue.SendAsync((image, new SaveInfo(idx++, "foo")));
            }
            outputQueue.Complete();
            logger.WriteLine("finished copying", Verbosity.Info);
        }
    }

    public class StackContinousStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        private readonly int StackCount;

        public StackContinousStrategy(int stackCount, ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {
            StackCount = stackCount;
        }

        protected override async Task ProcessingThread(List<IFilter<T>> filters, ISourceBlock<T> inputQueue, ITargetBlock<(T image, ISaveInfo saveInfo)> outputQueue)
        {
            var bufferQueue = new Queue<(IFilter<T> filter, T image, Task task, int appliedImages, int startIndex)>();
            int maxSize = filters.Count * StackCount;

            int i = 0;
            while (true)
            {
                T nextImage;
                try
                {
                    nextImage = await inputQueue.ReceiveAsync();
                }
                catch (InvalidOperationException)
                {
                    break;
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
                    await outputQueue.SendAsync((image, new SaveInfo(startIndex, filter.Name)));
                }

                filters.ForEach(filter =>
                {
                    bufferQueue.Enqueue((filter, image: nextImage, Task.CompletedTask, 0, i));
                });

                i++;
            }

            while (0 < bufferQueue.Count)
            {
                var (filter, image, t, appliedImages, startindex) = bufferQueue.Dequeue();
                await t;
                await outputQueue.SendAsync((image, new SaveInfo(startindex, filter.Name)));
            }

            outputQueue.Complete();
        }
    }

    public class StackProgressiveStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public StackProgressiveStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {

        }
        protected override async Task ProcessingThread(List<IFilter<T>> filters, ISourceBlock<T> inputQueue, ITargetBlock<(T image, ISaveInfo saveInfo)> outputQueue)
        {
            T firstData = await inputQueue.ReceiveAsync();
            var baseImages = filters.Select((filter) => (filter, image: factory.Clone(firstData), index: 0)).ToList();

            int i = 0;
            while (true)
            {
                T nextImage;
                try
                {
                    nextImage = await inputQueue.ReceiveAsync();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                var tasks = baseImages.Select(data => Task.Run(async () =>
                    {
                        data.index = i;
                        data.filter.Process(data.image, nextImage);
                        await outputQueue.SendAsync((factory.Clone(data.image), new SaveInfo(data.index, data.filter.Name)));
                        return data;
                    }));

                await Task.WhenAll(tasks);

                i++;
            }
            outputQueue.Complete();
        }
    }
}
