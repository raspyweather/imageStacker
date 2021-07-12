using System;
using System.Drawing;
using System.Drawing.Imaging;
using imageStacker.Core.Test.Unit.GenericImage;
using imageStacker.Core.UshortImage;

namespace imageStacker.Core.Test.Unit.UshortImage
{
    public class MutableUshortImageProvider : IImageProvider<MutableUshortImage>
    {
        private readonly int width, height;
        private readonly PixelFormat pixelFormat;
        public MutableUshortImageProvider(int width, int height, PixelFormat pixelFormat = PixelFormat.Format48bppRgb)
        {
            this.width = width;
            this.height = height;
            this.pixelFormat = pixelFormat;
        }
        public MutableUshortImage PrepareEmptyImage()
        {
            int necessaryUshorts = width * height * Image.GetPixelFormatSize(pixelFormat) / 16;
            var data = new ushort[necessaryUshorts];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = ushort.MinValue;
            }

            return new MutableUshortImage(width, height, pixelFormat, data);
        }

        public MutableUshortImage PrepareNoisyImage()
        {
            int necessaryUshorts = width * height * Image.GetPixelFormatSize(pixelFormat) / 16;

            var random = new Random();
            var data = new ushort[necessaryUshorts];

            var randomBytes = new byte[necessaryUshorts * 2];

            random.NextBytes(randomBytes);
            Buffer.BlockCopy(randomBytes, 0, data, 0, randomBytes.Length);

            return new MutableUshortImage(width, height, pixelFormat, data);
        }

        public MutableUshortImage PreparePrefilledImage(int value)
        {
            int necessaryUshorts = width * height * Image.GetPixelFormatSize(pixelFormat) / 16;

            var data = new ushort[necessaryUshorts];

            for (int i = 0; i < necessaryUshorts; i++)
            {
                data[i] = (ushort)(value % (ushort.MaxValue + 1));
            }

            return new MutableUshortImage(width, height, pixelFormat, data);
        }
    }
}
