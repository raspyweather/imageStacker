using System;
using System.Collections.Generic;
using System.Text;

namespace imageStacker.Core.ByteImage.Filters
{
    /// <summary>
    /// Algorithm generously contributed by @patagonaa
    /// </summary>
    public class AttackDecayFilter : IFilter<MutableByteImage>
    {
        public string Name => nameof(AttackDecayFilter);

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
