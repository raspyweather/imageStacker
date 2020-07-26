using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace imageStacker.Core.Vectorized
{
    internal static class VectorExtensions
    {
        public static Vector<byte> Vectorize(this byte[] ar, int start)
        {
            return new Vector<byte>(ar, start);
        }
    }
}