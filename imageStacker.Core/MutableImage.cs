using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace imageStacker.Core
{
    public interface IProcessableImage
    {
        public int Width { get; }
        public int Height { get; }

        public int BytesPerPixel { get; }

        public PixelFormat PixelFormat { get; }

        public byte[] Data { get; }
    }

    public class MutableImage : IProcessableImage
    {

        public static MutableImage FromProcessableImage(IProcessableImage image)
         => new MutableImage
            (
                Width: image.Width,
                Height: image.Height,
                format: image.PixelFormat,
                data: image.Data.ToArray());

        internal MutableImage(int Width, int Height, PixelFormat format, byte[] data)
        {
            this.Width = Width;
            this.Height = Height;
            this.Data = data;
            this.PixelFormat = format;
        }

        public static MutableImage FromFile(string fileName)
        {
            return FromImage(Image.FromFile(fileName));
        }
        public static MutableImage FromImage(Image image)
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

                return new MutableImage(width, height, pixelFormat, bmp1Bytes);
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        public static MutableImage FromBytes(int width, int height, byte[] data, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
        {
            try
            {
                return new MutableImage(width, height, pixelFormat, data);
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        public static Image ToImage(MutableImage image)
        {
            //  var newPicture = new Bitmap(previousData.Width, previousData.Height, previousData.PixelFormat);
            var height = image.Height;
            var width = image.Width;
            var pixelFormat = image.PixelFormat;
            var newPicture = new Bitmap(width, height, pixelFormat);
            var bmp1Data = newPicture.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
            var length = bmp1Data.Stride * bmp1Data.Height;
            Marshal.Copy(image.Data, 0, bmp1Data.Scan0, length);
            newPicture.UnlockBits(bmp1Data);

            return newPicture;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] Data { get; }

        public int BytesPerPixel => Image.GetPixelFormatSize(PixelFormat);


        public PixelFormat PixelFormat { get; }

        public int Brightness(int x, int y) =>
            this.Data[x + y * Width] * this.Data[x + y * Width] +
            this.Data[x + y * Width + 1] * this.Data[x + y * Width + 1] +
            this.Data[x + y * Width + 2] * this.Data[x + y * Width + 2];

        public MutableImage Clone() => new MutableImage(
                Width: Width,
                Height: Height,
                format: PixelFormat,
                data: (byte[])Data.Clone());
    }

}
