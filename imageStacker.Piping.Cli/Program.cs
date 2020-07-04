using imageStacker.Core;
using imageStacker.Core.Gpu;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace imageStacker.Piping.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var z = new GpuMaxFilter();
            z.Fill(null, 0);
            /* var files = System.IO.Directory.GetFiles("/mnt/h/DCIM/109_FUJI");
             using (var stream = Console.OpenStandardOutput())
             {
                 foreach (var item in files)
                 {
                     var bytes = File.ReadAllBytes(item);
                     stream.Write(bytes, 0, bytes.Length);
                     stream.WriteByte(0);
                 }
             }*/
        }
        /*    static async Task Main(string[] args)
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                int length = 4896 * 3264 * 3;
                using var stream = Console.OpenStandardInput(length);
                try
                {
                    int ctr = 0;
                    int bytesCtr = 0;
                    byte[] buffer = new byte[4896 * 3264 * 3];
                    while (stream.CanRead)
                    {
                        ctr++;
                        int bytesRead = await stream.ReadAsync(buffer, bytesCtr, length - bytesCtr);
                        bytesCtr += bytesRead;
                        if (bytesCtr == length)
                        {
                            bytesCtr = 0;
                            Console.WriteLine("\nNext picture " + st.ElapsedMilliseconds);
                        }
                        if (bytesRead == 0) { return; } 
                        Console.Write($"\rLength: {length} {bytesRead} {buffer[bytesCtr]:x6} {ctr} ");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                /*    var files = Directory.GetFiles("/mnt/f/fuji/lapse2");
                    using (var stream = Console.OpenStandardOutput())
                    {
                        foreach (var item in files)
                        {
                            FileReader.ReadWMetaAsStream(item, ImageFormat.Bmp).CopyTo(stream);
                            stream.WriteByte(0);
                    }*/
        /*  }
          public static byte[] ReadAllBytes(Stream stream)
          {
              using (var ms = new MemoryStream(4896 * 3264 * 3))
              {
                  stream.CopyTo(ms, 4896 * 3264 * 3);
                  return ms.ToArray();
              }
          }*/
    }

    //  using var st = new System.IO.FileStream(item, System.IO.FileMode.Open);
    //  using BufferedStream bufferedStream = new BufferedStream(st, 1024 * 1024 * 1024);
    //  bufferedStream.CopyTo(stream);
    //  var bytes = FileReader.ReadWMeta(item);
    //   StreamReader str = new StreamReader(bufferedStream);
}