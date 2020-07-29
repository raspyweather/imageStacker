using FluentAssertions;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using imageStacker.Core.Test.Unit.ByteImage;
using imageStacker.Core.Test.Unit.ByteImage.Filters;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Xunit;

namespace imageStacker.Core.Test.Unit
{
    public abstract class FilterCommutativeTestBase
    {
        private readonly MutableByteImageProvider provider = new MutableByteImageProvider(8, 8, PixelFormat.Format24bppRgb);
        protected abstract IFilter<MutableByteImage> Filter { get; }

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
        protected override IFilter<MutableByteImage> Filter => new MaxFilter();
    }

    public class MaxVectorFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new MaxVecFilter();
    }

    public class MinFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new MinFilter();
    }

    public class MinVectorFilterCommutativeTest : FilterCommutativeTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new MinVecFilter();
    }
}
