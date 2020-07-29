using FluentAssertions;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Xunit;

namespace imageStacker.Core.Test.Unit.ByteImage.Filters
{
    public abstract class MaxFilterTestBase
    {

        private readonly MutableByteImageProvider provider = new MutableByteImageProvider(8, 8, PixelFormat.Format24bppRgb);

        protected abstract IFilter<MutableByteImage> Filter { get; }

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
        protected override IFilter<MutableByteImage> Filter => new MaxFilter();
    }
    public class MaxVecFilterTest : MaxFilterTestBase
    {
        protected override IFilter<MutableByteImage> Filter => new MaxVecFilter();
    }
}
