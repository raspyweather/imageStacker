using FluentAssertions;
using imageStacker.Core.ShortImage;
using imageStacker.Core.ShortImage.Filters;
using System;
using System.Drawing.Imaging;
using System.Linq;
using Xunit;

namespace imageStacker.Core.Test.Unit.ShortImage.Filters
{
    public abstract class FilterCommutativeTestBase
    {
        private readonly MutableShortImageProvider provider = new MutableShortImageProvider(8, 8, PixelFormat.Format24bppRgb);
        protected abstract IFilter<MutableShortImage> Filter { get; }

        [Fact]
        public void FilterShouldBeACommutative()
        {
            var emptyImage1 = provider.PreparePrefilledImage(0);
            var noisyImage1 = provider.PreparePrefilledImage(255);

            var emptyImage2 = provider.PreparePrefilledImage(255);
            var noisyImage2 = provider.PreparePrefilledImage(0);

            var filter = Filter;
            filter.Process(emptyImage1, noisyImage1);
            filter.Process(noisyImage2, emptyImage2);

            emptyImage1.Data.SequenceEqual(noisyImage2.Data).Should().BeTrue();
        }
    }
    public class MaxFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MaxFilter();
    }

    public class MaxVectorFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MaxVecFilter();
    }

    public class MinFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MinFilter();
    }

    public class MinVectorFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableShortImage> Filter => new MinVecFilter();
    }
}
