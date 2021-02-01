using imageStacker.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public abstract class ThreadProcessingStrategy<T> : ProcessingStrategy<T> where T : IProcessableImage
    {
        public ThreadProcessingStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        { }

        protected readonly IBoundedQueue<T> inputQueue = BoundedQueueFactory.Get<T>(4, "Th-InQ");
        protected readonly IBoundedQueue<(T image, ISaveInfo saveInfo)> outputQueue = BoundedQueueFactory.Get<(T image, ISaveInfo saveInfo)>(2, "TH-OutQ");

        protected async Task ReadingThread(IImageReader<T> reader)
        {
            try
            {
                await reader.Produce(inputQueue);
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
                writer.SetQueue(outputQueue);
                await writer.WaitForCompletion();
                logger.NotifyFillstate(outputQueue.Count, "WriteBuffer");
                logger.WriteLine("Processing Output Ended", Verbosity.Warning, true);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                throw;
            }
        }

        protected async Task<T> GetFirstImage()
        {
            var image = await inputQueue.DequeueOrDefault();
            if (image != null)
                return image;

            throw new InvalidOperationException("No Image could be found");
        }

        protected abstract Task ProcessingThread(List<IFilter<T>> filter);

        public override async Task Process(IImageReader<T> reader, List<IFilter<T>> filters, IImageWriter<T> writer)
        {
            try
            {
                var readThread = Task.Run(() => ReadingThread(reader));
                var processThread = Task.Run(() => ProcessingThread(filters));
                var writeThread = Task.Run(() => WritingThread(writer));

                await Task.WhenAll(readThread, processThread, writeThread);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
