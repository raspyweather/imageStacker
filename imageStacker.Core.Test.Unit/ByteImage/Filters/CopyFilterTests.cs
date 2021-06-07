using FluentAssertions;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using System;
using System.Drawing.Imaging;
using System.Linq;
using Xunit;

namespace imageStacker.Core.Test.Unit.ByteImage.Filters
{
    public abstract class CopyFilterTestBase
    {
        private readonly MutableByteImageProvider provider = new MutableByteImageProvider(8, 8, PixelFormat.Format24bppRgb);

        protected abstract IFilter<MutableByteImage> Filter { get; }

        [Fact]
        public void FilterShouldProvideNextValue()
        {
            var emptyImage = provider.PrepareEmptyImage();
            var noisyImage = provider.PrepareNoisyImage();

            var copyFilter = Filter;

            copyFilter.Process(emptyImage, noisyImage);

            emptyImage.Data.SequenceEqual(noisyImage.Data).Should().BeTrue();
        }
    }

    public class CopyFilterTest : CopyFilterTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new CopyFilter();
    }
}
