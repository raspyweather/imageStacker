using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace imageStacker.Core.ByteImage.Filters
{
    public class MaxVecFilter : IFilter<MutableByteImage>
    {
        public string Name => nameof(MaxVecFilter);
        public unsafe void Process(MutableByteImage currentPicture, MutableByteImage nextPicture)
        {
            if (!Sse2.IsSupported)
            {
                throw new NotSupportedException("SSE2 not supported 😒");
            }

            int simdSize = Vector128<byte>.Count;
            int length = currentPicture.Data.Length;

            fixed (byte* dataPtr1 = currentPicture.Data)
            fixed (byte* dataPtr2 = nextPicture.Data)
            {
                int i = 0;
                for (; i < length; i += simdSize)
                {
                    Sse2.Store(dataPtr1 + i, Sse2.Max(
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
