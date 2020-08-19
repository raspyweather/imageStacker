using imageStacker.Core.Abstraction;

namespace imageStacker.Core.ByteImage.Filters
{
    /// <summary>
    /// Algorithm generously contributed by @patagonaa
    /// </summary>
    public class AttackDecayFilter : IFilter<MutableByteImage>
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

        public unsafe void Process(MutableByteImage currentPicture, MutableByteImage nextPicture)
        {
            const float Attack = 1, Decay = 0.2f;

            int length = nextPicture.Data.Length;
            fixed (byte* currentPicPtr = currentPicture.Data)
            {
                fixed (byte* nextPicPtr = nextPicture.Data)
                {
                    byte* currentPxPtr = currentPicPtr;
                    byte* nextPxPtr = nextPicPtr;

                    for (int i = 0; i < length; i++)
                    {
                        var currentColor = *nextPxPtr;
                        var workingDataColor = *currentPxPtr;

                        var newPixelFactor = workingDataColor < currentColor ? Attack : Decay;

                        var newPixelValue = (byte)((currentColor * newPixelFactor) + (workingDataColor * (1 - newPixelFactor)));

                        *currentPxPtr = newPixelValue;
                        currentPxPtr++;
                        nextPxPtr++;
                    }
                }
            }
        }
    }
}
