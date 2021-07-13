using System;
using System.Drawing.Imaging;
using System.Linq;
using FluentAssertions;
using imageStacker.Core.UshortImage;
using imageStacker.Core.UshortImage.Filters;
using Xunit;


namespace imageStacker.Core.Test.Unit.UshortImage.Filters
{
    public abstract class MinFilterTestBase
    {

        private readonly MutableUshortImageProvider provider = new MutableUshortImageProvider(8, 8, PixelFormat.Format48bppRgb);

        protected abstract IFilter<MutableUshortImage> Filter { get; }

        [Fact]
        public void FilterShouldSelectMinValues()
        {
            var whiteImage = provider.PreparePrefilledImage(ushort.MaxValue);
            var noisyImage = provider.PrepareNoisyImage();

            var minFilter = Filter;

            minFilter.Process(whiteImage, noisyImage);

            whiteImage.Data.SequenceEqual(noisyImage.Data).Should().BeTrue();
        }
    }

    public class MinFilterTest : MinFilterTestBase
    {
        protected override IFilter<MutableUshortImage> Filter => new MinFilter();
    }
    public class MinVecFilterTest : MinFilterTestBase
    {
        protected override IFilter<MutableUshortImage> Filter => new MinVecFilter();
    }
}
