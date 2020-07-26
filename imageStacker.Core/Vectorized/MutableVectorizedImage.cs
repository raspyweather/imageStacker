using System.Drawing.Imaging;
using System.Numerics;

namespace imageStacker.Core.Vectorized
{
    public class MutableVectorizedImage : MutableImage
    {
        public MutableVectorizedImage(int Width, int Height,int Offset, PixelFormat format, Vector<byte>[] data) : base(Width, Height, format)
        {
            this.Offset = Offset;
            this.Data = data;
        }

        public Vector<byte>[] Data { get; }
        public int Offset { get;  }
    }
}