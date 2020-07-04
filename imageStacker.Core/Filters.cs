namespace imageStacker.Core
{
    public interface IFilter
    {
        public string Name { get; }
        public void Process(MutableImage currentIamge, IProcessableImage nextPicture);
    }

    public class MinFilter : IFilter
    {
        public string Name => nameof(MinFilter);
        public unsafe void Process(MutableImage currentPicture, IProcessableImage nextPicture)
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

    public class MaxFilter : IFilter
    {
        public string Name => nameof(MaxFilter);

        public unsafe void Process(MutableImage currentPicture, IProcessableImage nextPicture)
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
                        var nextData = *nextPxPtr;
                        if (*currentPxPtr < nextData)
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

    public class ExtremaFilter : IFilter
    {
        public string Name => nameof(ExtremaFilter);

        private readonly int Sigma = 20;

        public unsafe void Process(MutableImage currentPicture, IProcessableImage nextPicture)
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
