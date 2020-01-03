using imageStacker.ImageCommand;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
namespace imageStacker
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            const string str1 = @"H:\Canada\timelapses\201909222021";
            const string str2 = @"./sample1";
            var files = System.IO.Directory.GetFiles(str1, "*.jpg", System.IO.SearchOption.TopDirectoryOnly);
            var command = new Average();
            var firstFile = files.First();
            var imageType = Image.FromFile(firstFile);
            var firstImage = new Bitmap(firstFile);

            Action<byte[], byte[]> ov1 = command.OverlayBrighter;

            Stopwatch st = new Stopwatch();
            void f(string name, Action<byte[], byte[]> action)
            {
                st.Restart();
                foreach (var file in files.Skip(1))
                {
                    Console.WriteLine(file);
                    var image = new Bitmap(file);
                    firstImage = command.Run(firstImage, image, ov1);
                    image.Dispose();
                }
                firstImage.Save(name + ".jpg", imageType.RawFormat);
                Console.WriteLine(st.ElapsedMilliseconds);
            }

            f("res3", ov1);

        }

    }
}
