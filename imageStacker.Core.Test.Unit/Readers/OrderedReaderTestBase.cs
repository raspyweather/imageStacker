using FluentAssertions;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Test.Unit.ByteImage;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
            reader.GetSource().LinkTo(queue, new DataflowLinkOptions { PropagateCompletion = true });
            var t = Task.Run(() => reader.Work());
            int i = 0;
            MutableByteImage data;
            while (true)
            {
                try
                {
                    data = await queue.ReceiveAsync();
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                data.Data[0].Should().Be((byte)(i));
                i++;
                await Task.Yield();
            }

            i.Should().Be(ImagesCount);

            // for capturing exceptions
            await t;
        }
    }
}
