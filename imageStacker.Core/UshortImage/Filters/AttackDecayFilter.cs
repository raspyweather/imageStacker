using imageStacker.Core.Abstraction;

namespace imageStacker.Core.UshortImage.Filters
{
    /// <summary>
    /// Algorithm generously contributed by @patagonaa
    /// </summary>
    public class AttackDecayFilter : IFilter<MutableUshortImage>
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

        public unsafe void Process(MutableUshortImage currentPicture, MutableUshortImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            fixed (ushort* currentPicPtr = currentPicture.Data)
            {
                fixed (ushort* nextPicPtr = nextPicture.Data)
                {
                    ushort* currentPxPtr = currentPicPtr;
                    ushort* nextPxPtr = nextPicPtr;

                    for (int i = 0; i < length; i++)
                    {
                        var currentColor = *nextPxPtr;
                        var workingDataColor = *currentPxPtr;

                        var newPixelFactor = workingDataColor < currentColor ? Attack : Decay;

                        var newPixelValue = (ushort)((currentColor * newPixelFactor) + (workingDataColor * (1 - newPixelFactor)));

                        *currentPxPtr = newPixelValue;
                        currentPxPtr++;
                        nextPxPtr++;
                    }
                }
            }
        }
    }
}
