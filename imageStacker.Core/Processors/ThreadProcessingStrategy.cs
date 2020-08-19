using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                    await Task.Run(() => writer.Writefile(imageInfo.image, imageInfo.saveInfo));
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
                if (!inputQueue.TryDequeue(out var firstData))
                {
                    if (inputFinishedToken.IsCancellationRequested)
                    {
                        break;
                    }
                    await Task.Delay(100);
                    continue;
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

                await Task.WhenAll(readThread, processThread, writeThread);

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
