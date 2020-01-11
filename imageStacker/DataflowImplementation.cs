using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;

namespace imageStacker
{
    class DataflowImplementation
    {
        public static void Produce(ITargetBlock<byte[]> target, string[] filenames)
        {

            foreach (var filename in filenames)
            {
                Console.WriteLine(filename);
                var bmp1 = new Bitmap(filename);
                var height = bmp1.Height;
                var width = bmp1.Width;
                var pixelFormat = bmp1.PixelFormat;

                var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                target.Post(bmp1Bytes);
                bmp1.UnlockBits(bmp1Data);
                bmp1.Dispose();
            }
            target.Complete();
        }

        public static void Produce(ITargetBlock<byte[]> target, Bitmap[] images)
        {

            foreach (var bmp1 in images)
            {
                var height = bmp1.Height;
                var width = bmp1.Width;
                var pixelFormat = bmp1.PixelFormat;

                var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;

                byte[] bmp1Bytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);
                target.Post(bmp1Bytes);
                bmp1.UnlockBits(bmp1Data);
                bmp1.Dispose();
            }
            target.Complete();
        }


        // Demonstrates the consumption end of the producer and consumer pattern.
        public static async Task<Bitmap> ConsumeAsync(ISourceBlock<byte[]> source, string firstFile)
        {
            var bmp1 = new Bitmap(firstFile);
            var height = bmp1.Height;
            var width = bmp1.Width;
            var pixelFormat = bmp1.PixelFormat;

            var bmp1Data = bmp1.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, pixelFormat);
            var length = bmp1Data.Stride * bmp1Data.Height;

            byte[] bmp1Bytes = new byte[length];
            Marshal.Copy(bmp1Data.Scan0, bmp1Bytes, 0, length);

            // Initialize a counter to track the number of bytes that are processed.
            long bytesProcessed = 0;

            // Read from the source buffer until the source buffer has no 
            // available output data.
             
            while (await source.OutputAvailableAsync()&& !source.Completion.IsCompleted)
            {
                byte[] data = source.Receive();
                if (data.Length != bmp1Bytes.Length)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Data Length mismatch {data.Length}/{bmp1Bytes.Length}");
                    Console.ForegroundColor = color;
                }
                for (int i = 0; i < data.Length; i++)
                {
                    if (bmp1Bytes[i] < data[i])
                    {
                        bmp1Bytes[i] = data[i];
                    }
                }
                Console.WriteLine(bmp1Bytes.Count(x => x > 0));
                // Increment the count of bytes received.
                bytesProcessed += data.Length;
            }
            Marshal.Copy(bmp1Bytes, 0, bmp1Data.Scan0, length);
            bmp1.UnlockBits(bmp1Data);
            return bmp1;
        }
   
    }
}
