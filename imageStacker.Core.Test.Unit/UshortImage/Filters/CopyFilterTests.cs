using System;
using System.Drawing.Imaging;
using System.Linq;
using FluentAssertions;
using imageStacker.Core.UshortImage;
using imageStacker.Core.UshortImage.Filters;
using Xunit;

namespace imageStacker.Core.Test.Unit.UshortImage.Filters
{
    public abstract class CopyFilterTestBase
    {
        private readonly MutableUshortImageProvider provider = new MutableUshortImageProvider(8, 8, PixelFormat.Format24bppRgb);

        protected abstract IFilter<MutableUshortImage> Filter { get; }

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
        protected override IFilter<MutableUshortImage> Filter => new CopyFilter();
    }
}
