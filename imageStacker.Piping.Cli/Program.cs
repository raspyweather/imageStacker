using imageStacker.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Effects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace imageStacker.Piping.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data1 = new byte[4896 * 3264 * 3];
            byte[] data2 = new byte[4896 * 3264 * 3];
            Random r = new Random();
            r.NextBytes(data1);
            r.NextBytes(data2);

            Stopwatch stopwatch = new Stopwatch();

            MutableByteImageFactory factory = new MutableByteImageFactory(null);
            Bitmap bitmap = new Bitmap(4896, 3264, PixelFormat.Format24bppRgb);

            var fs = new System.IO.FileStream("1.jpg", FileMode.Open);
            MemoryStream memoryStream = new MemoryStream();
            fs.CopyTo(memoryStream);
            bitmap.Save(memoryStream, ImageFormat.Jpeg);
            memoryStream.Seek(0, SeekOrigin.Begin);

            stopwatch.Start();
            JpegDecode(memoryStream);
            stopwatch.Stop();
            Console.WriteLine($"jpegdecode {stopwatch.ElapsedMilliseconds}");

            stopwatch.Reset();
            memoryStream.Seek(0, SeekOrigin.Begin);
            stopwatch.Start();
            SysdrawingDecode(memoryStream);
            stopwatch.Stop();
            Console.WriteLine($"sysdecode {stopwatch.ElapsedMilliseconds}");

            stopwatch.Reset();
            memoryStream.Seek(0, SeekOrigin.Begin);
            stopwatch.Start();
            SysdrawingDecode2(memoryStream);
            stopwatch.Stop();
            Console.WriteLine($"sysdecode2 {stopwatch.ElapsedMilliseconds}");

            stopwatch.Reset();
            memoryStream.Seek(0, SeekOrigin.Begin);
            stopwatch.Start();
            JpegDecode2(memoryStream);
            stopwatch.Stop();
            Console.WriteLine($"libjpg {stopwatch.ElapsedMilliseconds}");
        }

        private static MemoryStream JpegDecode(MemoryStream jpegData)
        {
            var byteStream = new MemoryStream();
            JpegDecoder d = new JpegDecoder();
            var img = d.Decode(new Configuration(new JpegConfigurationModule()), jpegData);
            img.SaveAsBmp(byteStream, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
            //using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(jpegData, new JpegDecoder());
            // image.SaveAsBmp(byteStream);
            return byteStream;
        }


        private static MemoryStream JpegDecode2(MemoryStream jpegData)
        {
            var byteStream = new MemoryStream();
            var i = new BitMiracle.LibJpeg.JpegImage(jpegData);
            i.WriteBitmap(byteStream);
            //using SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(jpegData, new JpegDecoder());
            // image.SaveAsBmp(byteStream);
            return byteStream;
        }

        private static MemoryStream SysdrawingDecode(MemoryStream jpegData)
        {
            var byteStream = new MemoryStream();
            using System.Drawing.Image image = System.Drawing.Image.FromStream(jpegData);
            image.Save(byteStream, ImageFormat.Bmp);
            return byteStream;
        }

        private static MemoryStream SysdrawingDecode2(MemoryStream jpegData)
        {
            var byteStream = new MemoryStream();
            using System.Drawing.Image image = System.Drawing.Image.FromStream(jpegData);
            using var img = new Bitmap(image);
            img.Save(jpegData, ImageFormat.MemoryBmp);
            return byteStream;
        }
    }
}
