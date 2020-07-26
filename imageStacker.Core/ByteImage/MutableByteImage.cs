using System.Drawing.Imaging;

namespace imageStacker.Core.ByteImage
{
    public class MutableByteImage : MutableImage, IProcessableImage
    {
        internal MutableByteImage(int Width, int Height, PixelFormat format, byte[] data)
            : base(Width, Height, format)
        {
            this.Data = data;
        }

        public byte[] Data { get; }

        public int Brightness(int x, int y) =>
            this.Data[x + y * Width] * this.Data[x + y * Width] +
            this.Data[x + y * Width + 1] * this.Data[x + y * Width + 1] +
            this.Data[x + y * Width + 2] * this.Data[x + y * Width + 2];

        public MutableByteImage Clone() => new MutableByteImage(
                Width: Width,
                Height: Height,
                format: PixelFormat,
                data: (byte[])Data.Clone());
    }
}
