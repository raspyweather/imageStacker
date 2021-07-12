using imageStacker.Core.Abstraction;

namespace imageStacker.Core.ShortImage.Filters
{
    public class MinFilter : IFilter<MutableShortImage>
    {
        public MinFilter(IMinFilterOptions options = null)
        {
            this.Name = options?.Name ?? nameof(MinFilter);
        }

        public string Name { get; }

        public bool IsSupported => true;

        public unsafe void Process(MutableShortImage currentPicture, MutableShortImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            fixed (short* currentPicPtr = currentPicture.Data)
            fixed (short* nextPicPtr = nextPicture.Data)
            {
                short* currentPxPtr = currentPicPtr;
                short* nextPxPtr = nextPicPtr;

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
