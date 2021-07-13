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

        public MutableUshortImage Clone() => new MutableUshortImage(
                Width: Width,
                Height: Height,
                format: PixelFormat,
                data: (ushort[])Data.Clone());
    }
}
