using imageStacker.Core.ByteImage;
using imageStacker.Core.Readers;
using imageStacker.Core.Test.Unit.ByteImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace imageStacker.Core.Test.Unit.Readers
{
    public class ImageFileReaderTest : OrderedReaderTestBase
    {
        public ImageFileReaderTest() : base(
            new MutableByteImageProvider(8, 8),
            new MutableByteImageFactory(new Logger(TextWriter.Null)),
            255)
        {

        }

        protected override IImageReader<MutableByteImage> Reader =>
            new ImageFileReader<MutableByteImage>(new Logger(TextWriter.Null), factory, base.tempPath);
    }
}
