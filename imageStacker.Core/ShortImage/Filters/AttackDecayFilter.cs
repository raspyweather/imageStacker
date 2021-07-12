using imageStacker.Core.Abstraction;

namespace imageStacker.Core.ShortImage.Filters
{
    /// <summary>
    /// Algorithm generously contributed by @patagonaa
    /// </summary>
    public class AttackDecayFilter : IFilter<MutableShortImage>
    {
        public AttackDecayFilter(IAttackDecayOptions options)
        {
            this.Attack = options.Attack;
            this.Decay = options.Decay;
            this.Name = options.Name ?? nameof(AttackDecayFilter);
        }
        public string Name { get; }

        public float Attack { get; }
        public float Decay { get; }

        public bool IsSupported => true;

        public unsafe void Process(MutableShortImage currentPicture, MutableShortImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            fixed (short* currentPicPtr = currentPicture.Data)
            {
                fixed (short* nextPicPtr = nextPicture.Data)
                {
                    short* currentPxPtr = currentPicPtr;
                    short* nextPxPtr = nextPicPtr;

                    for (int i = 0; i < length; i++)
                    {
                        var currentColor = *nextPxPtr;
                        var workingDataColor = *currentPxPtr;

                        var newPixelFactor = workingDataColor < currentColor ? Attack : Decay;

                        var newPixelValue = (short)((currentColor * newPixelFactor) + (workingDataColor * (1 - newPixelFactor)));

                        *currentPxPtr = newPixelValue;
                        currentPxPtr++;
                        nextPxPtr++;
                    }
                }
            }
        }
    }
}
