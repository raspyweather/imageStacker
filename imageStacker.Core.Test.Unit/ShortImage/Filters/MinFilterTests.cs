using FluentAssertions;
using imageStacker.Core.ShortImage;
using imageStacker.Core.ShortImage.Filters;
using System;
using System.Drawing.Imaging;
using System.Linq;
using Xunit;


namespace imageStacker.Core.Test.Unit.ShortImage.Filters
{
    public abstract class MinFilterTestBase
    {

        private readonly MutableShortImageProvider provider = new MutableShortImageProvider(8, 8, PixelFormat.Format48bppRgb);

        protected abstract IFilter<MutableShortImage> Filter { get; }

        [Fact]
        public void FilterShouldSelectMinValues()
        {
            var whiteImage = provider.PreparePrefilledImage(short.MaxValue);
            var noisyImage = provider.PrepareNoisyImage();

            var minFilter = Filter;

            minFilter.Process(whiteImage, noisyImage);

            whiteImage.Data.SequenceEqual(noisyImage.Data).Should().BeTrue();
        }
    }

    public class MinFilterTest : MinFilterTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MinFilter();
    }
    public class MinVecFilterTest : MinFilterTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MinVecFilter();
    }
}
