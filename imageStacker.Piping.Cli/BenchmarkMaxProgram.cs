using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace imageStacker.Piping.Cli
{
    class BenchmarkMaxProgram
    {
        private static void TestFilterModes()
        {
            byte[] data1 = new byte[4986 * 3264 * 3 * 10];
            byte[] data2 = new byte[4986 * 3264 * 3 * 10];
            Random r = new Random();
            r.NextBytes(data1);
            r.NextBytes(data2);

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            FillVector(data1, data2);
            stopwatch.Stop();
            Console.WriteLine($"NumericVector {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            FillVectors128(data1, data2);
            stopwatch.Stop();
            Console.WriteLine($"Vector128 {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            FillVectors128Aligned(data1, data2);
            stopwatch.Stop();
            Console.WriteLine($"Vector128Aligned {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            FillVectors256(data1, data2);
            stopwatch.Stop();
            Console.WriteLine($"Vector256 {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            PtrMax(data1, data2);
            stopwatch.Stop();
            Console.WriteLine($"PtrMax {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            ClassicMax(data1, data2);
            stopwatch.Stop();
            Console.WriteLine($"ClassicMax {stopwatch.ElapsedMilliseconds}");
        }

        static unsafe void FillVectors256(byte[] data1, byte[] data2)
        {
            if (!Avx2.IsSupported)
            {
                Console.WriteLine("AVX2 not supported 😒");
                return;
            }
            byte[] result = new byte[data1.Length];
            int simdSize = Vector256<byte>.Count;
            int length = data1.Length / simdSize;
            fixed (byte* resultPtr = result)
            fixed (byte* dataPtr1 = data1)
            fixed (byte* dataPtr2 = data2)
            {
                for (int i = 0; i < length; i++)
                {
                    int subIdx = simdSize * i;
                    Avx.Store(resultPtr + subIdx, Avx2.Max(
                        Avx.LoadVector256(dataPtr1 + subIdx),
                        Avx.LoadVector256(dataPtr2 + subIdx)));
                }
            }
        }

        static unsafe void FillVectors128Aligned(byte[] data1, byte[] data2)
        {
            if (!Sse2.IsSupported)
            {
                Console.WriteLine("SSE2 not supported 😒");
                return;
            }
            byte[] result = new byte[data1.Length];
            int simdSize = Vector128<byte>.Count;
            int length = data1.Length / simdSize;
            fixed (byte* resultPtr = result)
            fixed (byte* dataPtr1 = data1)
            fixed (byte* dataPtr2 = data2)
            {
                for (int i = 0; i < length; i++)
                {
                    int subIdx = simdSize * i;
                    Sse2.StoreAligned(resultPtr + subIdx, Sse2.Max(
                        Sse2.LoadAlignedVector128(dataPtr1 + subIdx),
                        Sse2.LoadAlignedVector128(dataPtr2 + subIdx)));
                }
            }
        }

        static unsafe void FillVectors128(byte[] data1, byte[] data2)
        {
            if (!Sse2.IsSupported)
            {
                Console.WriteLine("SSE2 not supported 😒");
                return;
            }
            byte[] result = new byte[data1.Length];
            int simdSize = Vector128<byte>.Count;
            int length = data1.Length / simdSize;
            fixed (byte* resultPtr = result)
            fixed (byte* dataPtr1 = data1)
            fixed (byte* dataPtr2 = data2)
            {
                for (int i = 0; i < length; i++)
                {
                    int subIdx = simdSize * i;
                    Sse2.Store(resultPtr, Sse2.Max(
                        Sse2.LoadVector128(dataPtr1 + subIdx),
                        Sse2.LoadVector128(dataPtr2 + subIdx)));
                }
            }
        }

        static unsafe void FillVector(byte[] data1, byte[] data2)
        {
            if (!Vector.IsHardwareAccelerated)
            {
                Console.WriteLine("Vector HW Acceleration not supported 😒");
            }
            byte[] result = new byte[data1.Length];
            int simdSize = Vector<byte>.Count;
            int length = data1.Length;

            for (int i = 0; i < length; i += simdSize)
            {
                Vector.Max(
                    new Vector<byte>(data1, i),
                    new Vector<byte>(data2, i)).CopyTo(result, i);
            }
        }

        static unsafe void PtrMax(byte[] data1, byte[] data2)
        {
            byte[] result = new byte[data1.Length];
            int length = data1.Length;
            fixed (byte* currentPicPtr = data1)
            fixed (byte* nextPicPtr = data2)
            {
                byte* currentPxPtr = currentPicPtr;
                byte* nextPxPtr = nextPicPtr;

                for (int i = 0; i < length; i++)
                {
                    var nextData = *nextPxPtr;
                    if (*currentPxPtr < nextData)
                    {
                        *currentPxPtr = nextData;
                    }

                    currentPxPtr++;
                    nextPxPtr++;
                }
            }
        }

        static unsafe void ClassicMax(byte[] data1, byte[] data2)
        {
            int length = data1.Length;
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                var data1V = data1[i];
                var data2V = data2[i];

                if (data1V > data2V)
                {
                    result[i] = data1V;
                }
                result[i] = data2V;
            }
        }
    }

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


//  using var st = new System.IO.FileStream(item, System.IO.FileMode.Open);
//  using BufferedStream bufferedStream = new BufferedStream(st, 1024 * 1024 * 1024);
//  bufferedStream.CopyTo(stream);
//  var bytes = FileReader.ReadWMeta(item);
//   StreamReader str = new StreamReader(bufferedStream);
