using imageStacker.Core.ShortImage;
using imageStacker.Core.Test.Unit.GenericImage;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace imageStacker.Core.Test.Unit.ShortImage
{
    public class MutableShortImageProvider : IImageProvider<MutableShortImage>
    {
        private readonly int width, height;
        private readonly PixelFormat pixelFormat;
        public MutableShortImageProvider(int width, int height, PixelFormat pixelFormat = PixelFormat.Format48bppRgb)
        {
            this.width = width;
            this.height = height;
            this.pixelFormat = pixelFormat;
        }
        public MutableShortImage PrepareEmptyImage()
        {
            int necessaryShorts = width * height * Image.GetPixelFormatSize(pixelFormat) / 16;
            var data = new short[necessaryShorts];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = short.MinValue;
            }

            return new MutableShortImage(width, height, pixelFormat, data);
        }

        public MutableShortImage PrepareNoisyImage()
        {
            int necessaryShorts = width * height * Image.GetPixelFormatSize(pixelFormat) / 16;

            var random = new Random();
            var data = new short[necessaryShorts];

            var randomBytes = new byte[necessaryShorts * 2];

            random.NextBytes(randomBytes);
            Buffer.BlockCopy(randomBytes, 0, data, 0, randomBytes.Length);

            return new MutableShortImage(width, height, pixelFormat, data);
        }

        public MutableShortImage PreparePrefilledImage(int value)
        {
            int necessaryShorts = width * height * Image.GetPixelFormatSize(pixelFormat) / 16;

            var data = new short[necessaryShorts];

            for (int i = 0; i < necessaryShorts; i++)
            {
                data[i] = (short)(value % (short.MaxValue + 1));
            }

            return new MutableShortImage(width, height, pixelFormat, data);
        }
    }
}
