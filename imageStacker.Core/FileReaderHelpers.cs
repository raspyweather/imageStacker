using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.IO;
using System.Reflection.Metadata;

namespace imageStacker.Core
{
    [Obsolete]
    public class FileReaderHelpers
    {
        public static void Produce(ITargetBlock<IProcessableImage> target, string[] filenames)
        {
            foreach (var filename in filenames)
            {
                // Console.WriteLine($"Idx: {i++} File: {filename}");
                try
                {
                    target.Post(MutableImage.FromFile(filename));
                }
                catch (Exception e) { Console.WriteLine(e); }
            }
            target.Complete();
        }
        public static async Task Produce(ConcurrentQueue<IProcessableImage> queue, string[] filenames)
        {
            foreach (var filename in filenames)
            {
                //   Console.WriteLine($"Idx: {i++} File: {filename}");
                try
                {
                    var bmp1 = new Bitmap(filename);
                    var height = bmp1.Height;
                    var width = bmp1.Width;
                    var pixelFormat = bmp1.PixelFormat;

                    var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                    var length = bmp1Data.Stride * bmp1Data.Height;

                    byte[] bmp1Bytes = new byte[length];
                    Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                    var image = MutableImage.FromBytes(width, height, bmp1Bytes);
                    queue.Enqueue(image);
                    bmp1.UnlockBits(bmp1Data);
                    bmp1.Dispose();
                }
                catch (Exception e) { Console.WriteLine(e); }
                while (queue.Count > 10) await Task.Delay(10);
            }
        }
        public static byte[] Read(string name)
        {
            try
            {
                var bmp1 = new Bitmap(name);
                var height = bmp1.Height;
                var width = bmp1.Width;
                var pixelFormat = bmp1.PixelFormat;

                var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                bmp1.UnlockBits(bmp1Data);
                bmp1.Dispose();
                return bmp1Bytes;
            }
            catch (Exception e) { Console.WriteLine(e); return new byte[0]; }
        }

        public static MutableImage ReadWMeta(string name)
        {
            try
            {
                using var bmp1 = new Bitmap(name);
                using var stream = new MemoryStream();
                bmp1.Save(stream, ImageFormat.Jpeg);
                return MutableImage.FromBytes(bmp1.Width, bmp1.Height, stream.ToArray());
            }
            catch (Exception e) { Console.WriteLine(e); throw; }
        }

        public static Image FromData(Stream stream, int width, int height, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
        {
            var newPicture = new Bitmap(width, height, pixelFormat);
            var bmp1Data = newPicture.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
            var length = bmp1Data.Stride * bmp1Data.Height;
            var buffer = new byte[width * height * 3];
            stream.Read(buffer, 0, width * height * 3);
            Marshal.Copy(buffer, 0, bmp1Data.Scan0, length);
            newPicture.UnlockBits(bmp1Data);
            return newPicture;
        }

        public static Stream ReadWMetaAsStream(string name, ImageFormat fm)
        {
            try
            {
                using var bmp1 = new Bitmap(name);
                var stream = new MemoryStream();
                bmp1.Save(stream, fm);
                return stream;
            }
            catch (Exception e) { Console.WriteLine(e); throw e; }
        }

        public static Size GetDimensions(string name) => Image.FromFile(name).Size;

        public static PixelFormat GetPixelFormat(string name) => Image.FromFile(name).PixelFormat;
    }
}
