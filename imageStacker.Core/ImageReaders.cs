using System.IO;

namespace imageStacker.Core
{
    internal static class StreamHelper
    {
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            using var ms = new MemoryStream(count);
            stream.CopyTo(ms, count);
            return ms.ToArray();
        }
    }
}
