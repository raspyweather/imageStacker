using FluentAssertions;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using System;
using System.Drawing.Imaging;
using System.Linq;
using Xunit;

namespace imageStacker.Core.Test.Unit.ByteImage.Filters
{
    public abstract class MinFilterTestBase
    {

        private readonly MutableByteImageProvider provider = new MutableByteImageProvider(8, 8, PixelFormat.Format24bppRgb);

        protected abstract IFilter<MutableByteImage> Filter { get; }

        [Fact]
        public void FilterShouldSelectMinValues()
        {
            var whiteImage = provider.PreparePrefilledImage(255);
            var noisyImage = provider.PrepareNoisyImage();

            var minFilter = Filter;

            minFilter.Process(whiteImage, noisyImage);

            whiteImage.Data.SequenceEqual(noisyImage.Data).Should().BeTrue();
        }
    }

    public class MinFilterTest : MinFilterTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new MinFilter();
    }
    public class MinVecFilterTest : MinFilterTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new MinVecFilter();
    }
}
