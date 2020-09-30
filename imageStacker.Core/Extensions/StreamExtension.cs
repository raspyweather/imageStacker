using System;
using System.IO;

namespace imageStacker.Core
{
    public static class StreamExtension
    {
        public static byte[] ReadBytes(this Stream stream, int count)
        {
            using var ms = new MemoryStream(count);
            stream.CopyStream(ms, count);
            return ms.ToArray();
        }

        public static void CopyStream(this Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}