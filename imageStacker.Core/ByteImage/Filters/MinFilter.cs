using System;

namespace imageStacker.Core.ByteImage.Filters
{
    public class MinFilter : IFilter<MutableByteImage>
    {
        public string Name => nameof(MinFilter);
        public unsafe void Process(MutableByteImage currentPicture, MutableByteImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            fixed (byte* currentPicPtr = currentPicture.Data)
            fixed (byte* nextPicPtr = nextPicture.Data)
            {
                byte* currentPxPtr = currentPicPtr;
                byte* nextPxPtr = nextPicPtr;

                for (int i = 0; i < length; i++)
                {
                    var nextData = *nextPxPtr;
                    if (*currentPxPtr > nextData)
                    {
                        *currentPxPtr = nextData;
                    }

                    currentPxPtr++;
                    nextPxPtr++;
                }
            }
        }
    }
}
