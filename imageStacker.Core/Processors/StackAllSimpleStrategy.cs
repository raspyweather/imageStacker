using imageStacker.Core.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public class StackAllSimpleStrategy<T> : ThreadProcessingStrategy<T>, IImageProcessingStrategy<T> where T : IProcessableImage
    {
        public StackAllSimpleStrategy(ILogger logger, IMutableImageFactory<T> factory) : base(logger, factory)
        {
        }

        protected override async Task ProcessingThread(List<IFilter<T>> filters)
        {
            T firstMutableImage = await GetFirstImage();
            var baseImages = filters.Select((filter, index) => (filter, image: factory.Clone(firstMutableImage), index)).ToList();

            T nextImage;
            while ((nextImage = await inputQueue.DequeueOrDefault()) != null)
            {
                foreach (var item in baseImages)
                {
                    foreach (var filter in filters)
                    {
                        filter.Process(item.image, nextImage);
                    }
                }
            }

            foreach (var data in baseImages)
            {
                await outputQueue.Enqueue((data.image, new SaveInfo(null, data.filter.Name)));
            }
            outputQueue.CompleteAdding();
        }

    }
}
