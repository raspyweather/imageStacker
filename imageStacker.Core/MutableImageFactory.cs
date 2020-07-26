using System.Drawing;
using System.Drawing.Imaging;

namespace imageStacker.Core
{
    public interface IMutableImageFactory<T> where T : IProcessableImage
    {
        T FromFile(string filename);
        T FromImage(Image image);
        T FromBytes(int width, int height, byte[] data, PixelFormat pixelFormat = PixelFormat.Format24bppRgb);

        Image ToImage(T image);

        byte[] ToBytes(T image);

        T Clone(T image);
    }
}