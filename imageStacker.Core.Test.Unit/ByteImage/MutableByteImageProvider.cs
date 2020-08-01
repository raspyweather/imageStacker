using imageStacker.Core.ByteImage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace imageStacker.Core.Test.Unit.ByteImage
{
    public class MutableByteImageProvider : IImageProvider<MutableByteImage>
    {
        private readonly int width, height;
        private readonly PixelFormat pixelFormat = PixelFormat.Format24bppRgb;
        public MutableByteImageProvider(int width, int height, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
        {
            this.width = width;
            this.height = height;
            this.pixelFormat = pixelFormat;
        }
        public MutableByteImage PrepareEmptyImage()
        {
            int necessaryBytes = width * height * Image.GetPixelFormatSize(pixelFormat) / 8;
            var data = new byte[necessaryBytes];

            return new MutableByteImage(width, height, PixelFormat.Format24bppRgb, data);
        }

        public MutableByteImage PrepareNoisyImage()
        {
            int necessaryBytes = width * height * Image.GetPixelFormatSize(pixelFormat) / 8;

            var random = new Random();
            var data = new byte[necessaryBytes];

            random.NextBytes(data);

            return new MutableByteImage(width, height, PixelFormat.Format24bppRgb, data);
        }

        public MutableByteImage PreparePrefilledImage(int value)
        {
            int necessaryBytes = width * height * Image.GetPixelFormatSize(pixelFormat) / 8;

            var data = new byte[necessaryBytes];

            for (int i = 0; i < necessaryBytes; i++)
            {
                data[i] = (byte)(value % 256);
            }

            return new MutableByteImage(width, height, PixelFormat.Format24bppRgb, data);
        }
    }
}
