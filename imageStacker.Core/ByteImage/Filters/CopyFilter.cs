using imageStacker.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace imageStacker.Core.ByteImage.Filters
{
    public class CopyFilter : IFilter<MutableByteImage>
    {
        public CopyFilter(ICopyFilterOptions options = null)
        {
            this.Name = options?.Name ?? nameof(CopyFilter);
        }
        public string Name { get; init; }

        public bool IsSupported => true;

        public unsafe void Process(MutableByteImage currentImage, MutableByteImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            Buffer.BlockCopy(nextPicture.Data, 0, currentImage.Data, 0, length);
        }
    }
}
