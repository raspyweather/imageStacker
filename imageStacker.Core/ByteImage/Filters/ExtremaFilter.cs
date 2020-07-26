namespace imageStacker.Core.ByteImage.Filters
{

    public class ExtremaFilter : IFilter<MutableByteImage>
    {
        public string Name => nameof(ExtremaFilter);

        private readonly int Sigma = 20;

        public unsafe void Process(MutableByteImage currentPicture, MutableByteImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            fixed (byte* currentPicPtr = currentPicture.Data)
            {
                fixed (byte* nextPicPtr = nextPicture.Data)
                {
                    byte* currentPxPtr = currentPicPtr;
                    byte* nextPxPtr = nextPicPtr;

                    for (int i = 0; i < length; i++)
                    {
                        var currentData = *currentPxPtr;
                        var nextData = *nextPxPtr;
                        if (currentData - nextData > Sigma || nextData - currentData > Sigma)
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
}