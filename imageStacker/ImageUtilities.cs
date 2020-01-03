using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace imageStacker
{
    public static class ImageUtilities
    {
        public static bool isCompatibleType(this Bitmap bmp1, Bitmap bmp2)
        {
            var equalSize = bmp1.PhysicalDimension == bmp2.PhysicalDimension;
            var equalDepth = bmp1.PixelFormat == bmp2.PixelFormat;
            return equalDepth && equalSize;
        }
    }
}
