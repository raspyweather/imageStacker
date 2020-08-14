using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace imageStacker.Core.ByteImage.Filters
{
    /// <summary>
    /// Algorithm generously contributed by @patagonaa
    /// </summary>
    public class AttackDecayVecFilter : IFilter<MutableByteImage>
    {
        public string Name => nameof(AttackDecayVecFilter);

        public unsafe void Process(MutableByteImage currentPicture, MutableByteImage nextPicture)
        {
            float MaxFactor = 1;
            float Attack = 1, Decay = 0.2f;

            float[] attackAr = new float[] { Attack, Attack, Attack, Attack };
            float[] decayAr = new float[] { Decay, Decay, Decay, Decay };

            int length = nextPicture.Data.Length;

            float* MaxFactorPtr = &MaxFactor;
            fixed (float* AttackPtr = attackAr)
            fixed (float* DecayPtr = decayAr)
            fixed (byte* currentPicPtr = currentPicture.Data)
            fixed (byte* nextPicPtr = nextPicture.Data)
            {
                byte* currentPxPtr = currentPicPtr;
                byte* nextPxPtr = nextPicPtr;


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
                    *currentPxPtr = (byte)Avx.Extract(result, 0);
                    currentPxPtr++;
                    *currentPxPtr = (byte)Avx.Extract(result, 1);
                    currentPxPtr++;
                    *currentPxPtr = (byte)Avx.Extract(result, 2);
                    currentPxPtr++;
                    *currentPxPtr = (byte)Avx.Extract(result, 3);
                    currentPxPtr++;
                    
                    nextPxPtr += 4;

                }

                for (int i = 0; i < remainingLength; i++)
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
