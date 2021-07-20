using System.Drawing.Imaging;

namespace imageStacker.Core.UshortImage
{
    public class MutableUshortImage : MutableImage, IProcessableImage
    {
        internal MutableUshortImage(int Width, int Height, PixelFormat format, ushort[] data)
            : base(Width, Height, format)
        {
            this.Data = data;
        }

        public ushort[] Data { get; }

        public override byte[] GetBytes()
        {
            byte[] data = new byte[BytesPerPixel * Data.Length];
            System.Buffer.BlockCopy(Data, 0, data, 0, data.Length);
            return data;
        }

        public MutableUshortImage Clone() => new MutableUshortImage(
                Width: Width,
                Height: Height,
                format: PixelFormat,
                data: (ushort[])Data.Clone());
    }
}
