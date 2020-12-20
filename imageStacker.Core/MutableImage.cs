using System.Drawing;
using System.Drawing.Imaging;

namespace imageStacker.Core
{
    public interface IProcessableImage
    {
        public int Width { get; }
        public int Height { get; }

        public int BytesPerPixel { get; }

        public PixelFormat PixelFormat { get; }
    }

    public abstract class MutableImage : IProcessableImage
    {
        internal MutableImage(int Width, int Height, PixelFormat format)
        {
            this.Width = Width;
            this.Height = Height;
            this.PixelFormat = format;
        }

        public int Width { get; }
        public int Height { get; }

        public int BytesPerPixel => Image.GetPixelFormatSize(PixelFormat);

        public PixelFormat PixelFormat { get; }
    }

}
