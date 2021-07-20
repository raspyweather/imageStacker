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

        public override byte[] GetBytes()
            => (byte[])Data.Clone();

        public MutableByteImage Clone()
            => new MutableByteImage(
                Width: Width,
                Height: Height,
                format: PixelFormat,
                data: (byte[])Data.Clone());
    }
}
