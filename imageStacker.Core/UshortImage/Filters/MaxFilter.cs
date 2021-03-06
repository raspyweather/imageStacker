﻿using imageStacker.Core.Abstraction;

namespace imageStacker.Core.UshortImage.Filters
{
    public class MaxFilter : IFilter<MutableUshortImage>
    {
        public MaxFilter(IMaxFilterOptions options = null)
        {
            this.Name = options?.Name ?? nameof(MaxFilter);
        }

        public string Name { get; }

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
}
