using imageStacker.Core.ByteImage;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace imageStacker.Core
{
    public class MutableByteImageFactory : IMutableImageFactory<MutableByteImage>
    {
        private readonly ILogger logger;
        public MutableByteImageFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public MutableByteImage Clone(MutableByteImage image)
        {
            return new MutableByteImage(image.Width, image.Height, image.PixelFormat, image.Data.ToArray());
        }

        public MutableByteImage FromBytes(int width, int height, byte[] data, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
        {
            try
            {
                return new MutableByteImage(width, height, pixelFormat, data);
            }
            catch (Exception e) { logger.LogException(e); throw; }
        }

        public MutableByteImage FromFile(string filename)
        {
            return FromImage(Image.FromFile(filename));
        }

        public MutableByteImage FromImage(Image image)
        {
            try
            {
                var bmp1 = (image as Bitmap) ?? new Bitmap(image);
                var height = bmp1.Height;
                var width = bmp1.Width;
                var pixelFormat = bmp1.PixelFormat;

                var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                bmp1.UnlockBits(bmp1Data);
                bmp1.Dispose();

                return new MutableByteImage(width, height, pixelFormat, bmp1Bytes);
            }
            catch (Exception e) { logger.LogException(e); throw; }
        }

        public byte[] ToBytes(MutableByteImage image)
        {
            // todo: return image meta as well
            return image.Data;
        }

        public Image ToImage(MutableByteImage image)
        {
            var height = image.Height;
            var width = image.Width;
            var pixelFormat = image.PixelFormat;
            var newPicture = new Bitmap(width, height, pixelFormat);
            var bmp1Data = newPicture.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
            var length = bmp1Data.Stride * bmp1Data.Height;
            Marshal.Copy(image.Data, 0, bmp1Data.Scan0, image.Data.Length);
            newPicture.UnlockBits(bmp1Data);
            return newPicture;
        }
    }
}