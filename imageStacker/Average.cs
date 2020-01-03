using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace imageStacker.ImageCommand
{
    class Average
    {
        public Bitmap Run(Bitmap bmp1, Bitmap bmp2, Action<byte[], byte[]> operation)
        {
            if (!bmp1.isCompatibleType(bmp2))
            {
                throw new ArgumentException("Operations on different size or pixel formats not supported");
            }

            var height = bmp1.Height;
            var width = bmp1.Width;
            var pixelFormat = bmp1.PixelFormat;


            var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
            var bmp2Data = bmp2.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);

            var length = bmp1Data.Stride * bmp1Data.Height;

            byte[] bmp1Bytes = new byte[length];
            byte[] bmp2Bytes = new byte[length];

            // Copy bitmap to byte[]
            Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
            Marshal.Copy(bmp2Data.Scan0, bmp2Bytes, 0, length);

            operation(bmp1Bytes, bmp2Bytes);

            bmp2.UnlockBits(bmp2Data);
            Marshal.Copy(bmp1Bytes, 0, bmp1Data.Scan0, length);
            bmp1.UnlockBits(bmp1Data);
            return bmp1;
        }

        public void Averaging(byte[] ar1, byte[] ar2)
        {
            for (int idx = 0; idx < ar1.Length; idx++)
            {
                ar1[idx] = (byte)((ar1[idx] + ar2[idx]) / 2);
            }
        }

        public void OverlayBrighter(byte[] ar1, byte[] ar2)
        {
            for (int idx = 0; idx < ar1.Length; idx++)
            {
                if (ar1[idx] < ar2[idx])
                {
                    ar1[idx] =  ar2[idx];
                }
            }
        }
    }
}
