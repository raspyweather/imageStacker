using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace imageStacker.Core
{
    [Obsolete]
    public class FileReaderHelpers
    {
        public static Size GetDimensions(string name) => Image.FromFile(name).Size;

        public static PixelFormat GetPixelFormat(string name) => Image.FromFile(name).PixelFormat;
    }
}
