using imageStacker.Core;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Test.Unit.ByteImage;
using imageStacker.Core.Test.Unit.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
               .FromFileInput(Path.Combine(tempPath, "%05d.png"), false, args =>
                args.WithFramerate(1)
                     .ForcePixelFormat("rgb24")
                     .WithFramerate(1))
                .OutputToFile(TempFile, true, args =>
                    args.WithSpeedPreset(FFMpegCore.Enums.Speed.VerySlow)
                        //.WithFramerate(1.0)
                        //.WithConstantRateFactor(0)
                        .ForcePixelFormat("rgb24")
                );
            try
            {
                args.ProcessSynchronously(true);
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
