using imageStacker.Core.ByteImage;
using imageStacker.Core.Readers;
using imageStacker.Core.Test.Unit.ByteImage;
using System.IO;

namespace imageStacker.Core.Test.Unit.Readers
{
    public class ImageMultiFileReaderOrderedOrderingTest : OrderedReaderTestBase
    {
        public ImageMultiFileReaderOrderedOrderingTest() : base(
            new MutableByteImageProvider(8, 8),
            new MutableByteImageFactory(new Logger(TextWriter.Null)),
            255)
        {

        }

        protected override IImageReader<MutableByteImage> Reader =>
            new ImageMutliFileOrderedReader<MutableByteImage>(
                new Logger(TextWriter.Null),
                factory,
                new ReaderOptions
                {
                    FolderName = base.tempPath,
                    Filter = "*.png"
                });
    }
}