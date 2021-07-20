using imageStacker.Core.Abstraction;
using imageStacker.Core.UshortImage;
using System.Drawing.Imaging;

namespace imageStacker.Magick
{
    public class MagickUshortImageFactory : MutableUshortImageFactory
    {
        public MagickUshortImageFactory(ILogger logger) : base(logger)
        { }

        public override MutableUshortImage FromFile(string filename)
        {
            var img = new ImageMagick.MagickImage(filename);
            var height = img.Height;
            var width = img.Width;

            using var pixels = img.GetPixelsUnsafe();
            var data = pixels.GetArea(0, 0, width, height);
            return FromData(img.Width, img.Height, data, PixelFormat.Format48bppRgb);
        }
    }
}
