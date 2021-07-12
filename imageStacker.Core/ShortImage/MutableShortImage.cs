using System.Drawing.Imaging;

namespace imageStacker.Core.ShortImage
{
    public class MutableShortImage : MutableImage, IProcessableImage
    {
        internal MutableShortImage(int Width, int Height, PixelFormat format, short[] data)
            : base(Width, Height, format)
        {
            this.Data = data;
        }

        public short[] Data { get; }

        public MutableShortImage Clone() => new MutableShortImage(
                Width: Width,
                Height: Height,
                format: PixelFormat,
                data: (short[])Data.Clone());
    }
}
