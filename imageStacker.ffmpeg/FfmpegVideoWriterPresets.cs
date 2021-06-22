using FFMpegCore;

namespace imageStacker.ffmpeg
{
    public static class FfmpegVideoWriterPresets
    {
        public static FFMpegArgumentOptions UseFHDPreset(this FFMpegArgumentOptions args)
        {
            return args.ForcePixelFormat("yuv420p")
                   .WithVideoCodec("libx264")
                   .WithConstantRateFactor(25)
                   .WithCustomArgument("-profile:v baseline -level 3.0 -vf scale=-1:1080");
        }

        public static FFMpegArgumentOptions Use4KPreset(this FFMpegArgumentOptions args)
        {
            return args.ForcePixelFormat("yuv420p")
                   .WithVideoCodec("libx264")
                   .WithConstantRateFactor(25)
                   .WithCustomArgument("-profile:v baseline -level 4.1 -vf scale=-1:2160");
        }
        public static FFMpegArgumentOptions UseArchivePreset(this FFMpegArgumentOptions args)
        {
            return args
                    .WithVideoCodec("ffv1")
                    .WithConstantRateFactor(0);
        }

        public static FFMpegArgumentOptions UsePreset(this FFMpegArgumentOptions args, FfmpegVideoEncoderPreset preset)
        => preset switch
        {
            FfmpegVideoEncoderPreset.FullHD => args.UseFHDPreset(),
            FfmpegVideoEncoderPreset.FourK => args.Use4KPreset(),
            FfmpegVideoEncoderPreset.Archive => args.UseArchivePreset(),
            _ => args,
        };
    }
}
