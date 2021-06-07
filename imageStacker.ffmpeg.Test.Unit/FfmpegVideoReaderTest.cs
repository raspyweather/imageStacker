using imageStacker.Core;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Test.Unit.ByteImage;
using System.IO;

namespace imageStacker.ffmpeg.Test.Unit
{
    public class FfmpegVideoReaderTest : VideoReaderTestBase
    {
        public FfmpegVideoReaderTest() : base(
            new MutableByteImageProvider(8, 8),
            new MutableByteImageFactory(new Logger(TextWriter.Null)),
            255)
        {
        }
        protected override IImageReader<MutableByteImage> Reader =>
            new FfmpegVideoReader(
                new FfmpegVideoReaderArguments
                {
                    InputFile = TempFile,
                    CustomArgs = "-crf 0"
                },
                factory,
                new Logger(TextWriter.Null));

    }
}
