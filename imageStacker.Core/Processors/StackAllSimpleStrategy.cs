using imageStacker.Core.Abstraction;
using imageStacker.Core.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core
{
    public class StackAllSimpleStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public StackAllSimpleStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {
        }

        protected override async Task ProcessingThread(List<IFilter<T>> filters, ISourceBlock<T> inputQueue, ITargetBlock<(T image, ISaveInfo saveInfo)> outputQueue)
        {
            T firstMutableImage = await inputQueue.ReceiveAsync();
            var baseImages = filters.Select((filter, index) => (filter, image: factory.Clone(firstMutableImage), index)).ToList();

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

                foreach (var item in baseImages)
                {
                    foreach (var filter in filters)
                    {
                        filter.Process(item.image, nextImage);
                    }
                }
            }

            foreach (var (filter, image, index) in baseImages)
            {
                await outputQueue.SendAsync((image, new SaveInfo(index, filter.Name)));
            }
            outputQueue.Complete();
        }
    }
}
