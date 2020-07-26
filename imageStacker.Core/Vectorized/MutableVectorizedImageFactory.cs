using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace imageStacker.Core.Vectorized
{
    public unsafe class MutableVectorizedImageFactory : IMutableImageFactory<MutableVectorizedImage>
    {
        public MutableVectorizedImage Clone(MutableVectorizedImage image)
        {
            throw new NotImplementedException();
        }

        public MutableVectorizedImage FromBytes(int width, int height, byte[] data, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
        {
            throw new NotImplementedException();
           /* var simdLength = Vector<byte>.Count;
            var remainingBytes = data.Length % simdLength;
            var vectorCount = data.Length / simdLength;
            fixed (byte* dataPtr = data)
            {
                Avx.LoadVector256(dataPtr)
            }
            new Vector<byte>(data.AsSpan())

            return new MutableVectorizedImage(width, height, pixelFormat,)*/
        }

        public MutableVectorizedImage FromFile(string filename)
        {
            throw new NotImplementedException();
        }

        public MutableVectorizedImage FromImage(Image image)
        {
            throw new NotImplementedException();
        }

        public byte[] ToBytes(MutableVectorizedImage image)
        {
            throw new NotImplementedException();
        }

        public Image ToImage(MutableVectorizedImage image)
        {
            throw new NotImplementedException();
        }
    }
}
