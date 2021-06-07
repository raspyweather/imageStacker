using imageStacker.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core
{
    public abstract class ThreadProcessingStrategy<T> : ProcessingStrategy<T> where T : IProcessableImage
    {
        public ThreadProcessingStrategy(ILogger logger, IMutableImageFactory<T> factory)
            : base(logger, factory)
        {
        }

        protected async Task ReadingThread(IImageReader<T> reader)
        {
            try
            {
                await reader.Work();
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
                await writer.Work();
                logger.WriteLine("Processing Output Ended", Verbosity.Warning, true);
            }
            catch (Exception e)
            {
                logger.LogException(e);
                throw;
            }
        }

        protected abstract Task ProcessingThread(List<IFilter<T>> filter, ISourceBlock<T> inputQueue, ITargetBlock<(T image, ISaveInfo saveInfo)> outputQueue);

        public override async Task Process(IImageReader<T> reader, List<IFilter<T>> filters, IImageWriter<T> writer)
        {
            try
            {
                var readThread = Task.Run(() => ReadingThread(reader));
                var processThread = Task.Run(() => ProcessingThread(filters, reader.GetSource(), writer.GetTarget()));
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
