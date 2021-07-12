using FluentAssertions;
using imageStacker.Core.ShortImage;
using imageStacker.Core.ShortImage.Filters;
using System;
using System.Drawing.Imaging;
using System.Linq;
using Xunit;

namespace imageStacker.Core.Test.Unit.ShortImage.Filters
{
    public abstract class MaxFilterTestBase
    {

        private readonly MutableShortImageProvider provider = new MutableShortImageProvider(8, 8, PixelFormat.Format24bppRgb);

        protected abstract IFilter<MutableShortImage> Filter { get; }

        [Fact]
        public void FilterShouldSelectMaxValues()
        {
            var emptyImage = provider.PrepareEmptyImage();
            var noisyImage = provider.PrepareNoisyImage();

            var maxFilter = Filter;

            maxFilter.Process(emptyImage, noisyImage);

            emptyImage.Data.SequenceEqual(noisyImage.Data).Should().BeTrue();
        }
    }

    public class MaxFilterTest : MaxFilterTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MaxFilter();
    }
    public class MaxVecFilterTest : MaxFilterTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MaxVecFilter();
    }
}
