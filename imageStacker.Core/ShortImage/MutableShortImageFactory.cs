using imageStacker.Core.Abstraction;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace imageStacker.Core.ShortImage
{
    public class MutableShortImageFactory : IMutableImageFactory<MutableShortImage>
    {
        private readonly ILogger logger;
        public MutableShortImageFactory(ILogger logger)
        {
            this.logger = logger;
        }

        public MutableShortImage Clone(MutableShortImage image)
            => new MutableShortImage(image.Width, image.Height, image.PixelFormat, image.Data.ToArray());

        public MutableShortImage FromBytes(int width, int height, byte[] data, PixelFormat pixelFormat = PixelFormat.Format48bppRgb)
        {
            try
            {
                short[] sdata = new short[(int)Math.Ceiling(data.Length / 2d)];
                Buffer.BlockCopy(data, 0, sdata, 0, data.Length);
                return new MutableShortImage(width, height, pixelFormat, sdata);
            }
            catch (Exception e) { logger.LogException(e); throw; }
        }

        public MutableShortImage FromFile(string filename)
            => FromImage(Image.FromFile(filename));

        public MutableShortImage FromImage(Image image)
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

                return FromBytes(width, height, bmp1Bytes, pixelFormat);
            }
            catch (Exception e) { logger.LogException(e); throw; }
        }

        public byte[] ToBytes(MutableShortImage image)
        {
            byte[] data = new byte[image.BytesPerPixel * image.Data.Length];
            Buffer.BlockCopy(image.Data, 0, data, 0, data.Length);
            return data;
        }
        // todo: return image meta as well


        public Image ToImage(MutableShortImage image)
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
