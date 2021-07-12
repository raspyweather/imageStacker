using imageStacker.Core.Abstraction;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace imageStacker.Core.ShortImage.Filters
{
    public class MaxVecFilter : IFilter<MutableShortImage>
    {
        public MaxVecFilter(IMaxFilterOptions options = null)
        {
            this.Name = options?.Name ?? nameof(MaxVecFilter);
        }

        public string Name { get; }

        public bool IsSupported => Sse2.IsSupported;

        public unsafe void Process(MutableShortImage currentPicture, MutableShortImage nextPicture)
        {
            int simdSize = Vector128<short>.Count;
            int length = currentPicture.Data.Length;

            fixed (short* dataPtr1 = currentPicture.Data)
            fixed (short* dataPtr2 = nextPicture.Data)
            {
                int i = 0;
                for (; i < length; i += simdSize)
                {
                    Sse2.Store(dataPtr1 + i, Sse41.Max(
                        Sse2.LoadVector128(dataPtr1 + i),
                        Sse2.LoadVector128(dataPtr2 + i)));
                }

                for (; i < length; i++)
                {
                    var nextData = *(dataPtr2 + i);
                    if (*(dataPtr1 + i) < nextData)
                    {
                        *(dataPtr1 + i) = nextData;
                    }
                }
            }
        }
    }
}
