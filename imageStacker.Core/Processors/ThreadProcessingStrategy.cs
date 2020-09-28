using imageStacker.Core.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public abstract class ThreadProcessingStrategy<T> : ProcessingStrategy<T> where T : IProcessableImage
    {
        public ThreadProcessingStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        { }

        protected readonly ConcurrentQueue<T> inputQueue = new ConcurrentQueue<T>();
        protected readonly ConcurrentQueue<(T image, ISaveInfo saveInfo)> outputQueue = new ConcurrentQueue<(T image, ISaveInfo saveInfo)>();
        protected readonly CancellationTokenSource
            inputFinishedToken = new CancellationTokenSource(),
            outputFinishedToken = new CancellationTokenSource();

        protected async Task ReadingThread(IImageReader<T> reader)
        {
            try
            {
                await reader.Produce(inputQueue);
                inputFinishedToken.Cancel();
            }
            catch (Exception e)
            {
                logger.LogException(e);
                throw;
            }
        }
        protected async Task WritingThread(IImageWriter<T> writer)
        {
            try
            {
                var tasks = new List<Task>();
                while (true)
                {
                    logger.NotifyFillstate(outputQueue.Count, "WriteBuffer");

                    var (cancelled, imageInfo) = await outputQueue.TryDequeueOrWait(outputFinishedToken);

                    if (cancelled)
                    {
                        break;
                    }

                    tasks.Add(Task.Run(() => writer.Writefile(imageInfo.image, imageInfo.saveInfo)));

                    await tasks.WaitForFinishingTasks(128);

                    tasks = tasks.Where(x => !x.IsCompleted).ToList();
                }
                await Task.WhenAll(tasks);
                logger.NotifyFillstate(outputQueue.Count, "WriteBuffer");
                logger.WriteLine("Processing Ended", Verbosity.Warning, true);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                throw;
            }
        }

        protected async Task<T> GetFirstImage()
        {
            while (true)
            {
                var (cancelled, firstData) = await inputQueue.TryDequeueOrWait(inputFinishedToken);

                if (cancelled)
                {
                    break;
                }

                return firstData;
            }

            throw new InvalidOperationException("No Image could be found");
        }

        protected abstract Task ProcessingThread(List<IFilter<T>> filter);

        public override async Task Process(IImageReader<T> reader, List<IFilter<T>> filters, IImageWriter<T> writer)
        {
            try
            {
                var readThread = Task.Run(() => ReadingThread(reader)).ContinueWith(_ => inputFinishedToken.Cancel());
                var processThread = Task.Run(() => ProcessingThread(filters).ContinueWith(_ => outputFinishedToken.Cancel()));
                var writeThread = Task.Run(() => WritingThread(writer));

                var tasks = new List<Task> { readThread, processThread, writeThread };
                await tasks.WaitForFinishingTasks(3);
                tasks = tasks.Where(x => !x.IsCompleted).ToList();

                while (tasks.Count > 3)
                {
                    await Task.WhenAny(tasks);
                }

                //  await Task.WhenAll(readThread, processThread, writeThread);

                await readThread;
                await processThread;
                await writeThread;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
