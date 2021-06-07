using imageStacker.Core;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Test.Unit.ByteImage;
using imageStacker.Core.Test.Unit.Readers;
using System.IO;

namespace imageStacker.ffmpeg.Test.Unit
{
    public abstract class VideoReaderTestBase : OrderedReaderTestBase
    {
        public VideoReaderTestBase(IImageProvider<MutableByteImage> imageProvider, IMutableImageFactory<MutableByteImage> factory, int imagesCount)
            : base(imageProvider, factory, imagesCount)
        {
        }

        protected string TempFile = "";

        protected override void Prepare()
        {
            base.Prepare();

            TempFile = Path.Combine(tempPath, Path.GetRandomFileName() + ".apng");

            var args = FFMpegCore.FFMpegArguments
               .FromFileInput(Path.Combine(tempPath, "%05d.png"), false, args => args.ForcePixelFormat("rgb24"))
                .OutputToFile(TempFile, true, args =>
                    args.WithSpeedPreset(FFMpegCore.Enums.Speed.VerySlow)
                        .ForcePixelFormat("rgb24"));

            args.ProcessSynchronously(true);
        }
    }
}
