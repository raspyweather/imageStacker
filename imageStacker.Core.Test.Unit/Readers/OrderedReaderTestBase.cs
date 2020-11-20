using FluentAssertions;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Test.Unit.ByteImage;
using System;
using System.Threading.Tasks;
using Xunit;

namespace imageStacker.Core.Test.Unit.Readers
{
    public abstract class OrderedReaderTestBase : ReaderTestBase<MutableByteImage>, IDisposable
    {
        protected OrderedReaderTestBase(
            IImageProvider<MutableByteImage> imageProvider,
            IMutableImageFactory<MutableByteImage> factory,
            int imagesCount) :
            base(imageProvider, factory, imagesCount)
        {
        }

        [Fact]
        public async Task CheckOrderedProduction()
        {
            Prepare();
            var reader = Reader;
            var t = Task.Run(() => reader.Produce(queue));
            int i = 0;
            MutableByteImage data;
            while ((data = await queue.DequeueOrNull()) != null)
            {
                data.Data[0].Should().Be((byte)(i));
                i++;
                await Task.Yield();
            }

            i.Should().Be(ImagesCount);
        }
    }
}
