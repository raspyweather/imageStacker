using System;
using imageStacker.Core.Abstraction;

namespace imageStacker.Core.UshortImage.Filters
{
    public class CopyFilter : IFilter<MutableUshortImage>
    {
        public CopyFilter(ICopyFilterOptions options = null)
        {
            this.Name = options?.Name ?? nameof(CopyFilter);
        }
        public string Name { get; init; }

        public bool IsSupported => true;

        public unsafe void Process(MutableUshortImage currentImage, MutableUshortImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            Buffer.BlockCopy(nextPicture.Data, 0, currentImage.Data, 0, 2 * length);
        }
    }
}
