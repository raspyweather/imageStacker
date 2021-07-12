using imageStacker.Core.Abstraction;
using System;

namespace imageStacker.Core.ShortImage.Filters
{
    public class CopyFilter : IFilter<MutableShortImage>
    {
        public CopyFilter(ICopyFilterOptions options = null)
        {
            this.Name = options?.Name ?? nameof(CopyFilter);
        }
        public string Name { get; init; }

        public bool IsSupported => true;

        public unsafe void Process(MutableShortImage currentImage, MutableShortImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            Buffer.BlockCopy(nextPicture.Data, 0, currentImage.Data, 0, 2 * length);
        }
    }
}
