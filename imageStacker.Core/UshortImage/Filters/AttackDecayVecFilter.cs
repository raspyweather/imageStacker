using System.Runtime.Intrinsics.X86;
using imageStacker.Core.Abstraction;

namespace imageStacker.Core.UshortImage.Filters
{
    /// <summary>
    /// Algorithm generously contributed by @patagonaa
    /// </summary>
    public class AttackDecayVecFilter : IFilter<MutableUshortImage>
    {
        public AttackDecayVecFilter(IAttackDecayOptions options)
        {
            this.Attack = options.Attack;
            this.Decay = options.Decay;
            this.Name = options.Name ?? nameof(AttackDecayVecFilter);
        }
        public string Name { get; }

        public float Attack { get; }
        public float Decay { get; }

        public bool IsSupported => Avx.IsSupported;

        public unsafe void Process(MutableUshortImage currentPicture, MutableUshortImage nextPicture)
        {
            float MaxFactor = 1;

            float[] attackAr = new float[] { Attack, Attack, Attack, Attack };
            float[] decayAr = new float[] { Decay, Decay, Decay, Decay };

            int length = nextPicture.Data.Length;

            float* MaxFactorPtr = &MaxFactor;
            fixed (float* AttackPtr = attackAr)
            fixed (float* DecayPtr = decayAr)
            fixed (ushort* currentPicPtr = currentPicture.Data)
            fixed (ushort* nextPicPtr = nextPicture.Data)
            {
                ushort* currentPxPtr = currentPicPtr;
                ushort* nextPxPtr = nextPicPtr;


                int remainingLength = length % 4;
                for (int i = 0; i < length; i += 4)
                {
                    var currentColor = *nextPxPtr;
                    var workingDataColor = *currentPxPtr;

                    var currentColorPtr = nextPxPtr;
                    var workingDataColorPtr = currentPxPtr;

                    var cmpResult = Avx.ConvertToVector128Single(
                                        Sse2.CompareGreaterThan(
                                            Sse41.ConvertToVector128Int32(currentColorPtr),
                                            Sse41.ConvertToVector128Int32(workingDataColorPtr)
                                         ));

                    var pixelFactor = Avx.Add(
                        Avx.And(cmpResult, Avx.BroadcastScalarToVector128(AttackPtr)),
                         Avx.AndNot(cmpResult, Avx.BroadcastScalarToVector128(DecayPtr))
                        );

                    var result = Avx.Add(
                        Avx.Multiply(
                           Avx.Subtract(
                                Avx.BroadcastScalarToVector128(MaxFactorPtr),
                                pixelFactor),
                           Sse41.ConvertToVector128Single(
                                Sse41.ConvertToVector128Int32(workingDataColorPtr))
                            ),
                        Avx.Multiply(
                            pixelFactor,
                            Sse41.ConvertToVector128Single(
                                    Sse41.ConvertToVector128Int32(currentColorPtr))));

                    // TODO improve Store
                    *currentPxPtr = (ushort)Avx.Extract(result, 0);
                    currentPxPtr++;
                    *currentPxPtr = (ushort)Avx.Extract(result, 1);
                    currentPxPtr++;
                    *currentPxPtr = (ushort)Avx.Extract(result, 2);
                    currentPxPtr++;
                    *currentPxPtr = (ushort)Avx.Extract(result, 3);
                    currentPxPtr++;

                    nextPxPtr += 4;

                }

                for (int i = 0; i < remainingLength; i++)
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
