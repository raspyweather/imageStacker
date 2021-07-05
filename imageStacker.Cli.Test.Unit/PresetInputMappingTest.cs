using Xunit;
using FluentAssertions;
using imageStacker.ffmpeg;

namespace imageStacker.Cli.Test.Unit
{
    public class PresetInputMappingTest
    {
        [Theory]
        [InlineData("fhd", FfmpegVideoEncoderPreset.FullHD)]
        [InlineData("fullhd", FfmpegVideoEncoderPreset.FullHD)]
        [InlineData("FHD", FfmpegVideoEncoderPreset.FullHD)]
        [InlineData("fullHD", FfmpegVideoEncoderPreset.FullHD)]

        [InlineData("4k", FfmpegVideoEncoderPreset.FourK)]
        [InlineData("fourk", FfmpegVideoEncoderPreset.FourK)]
        [InlineData("4K", FfmpegVideoEncoderPreset.FourK)]

        [InlineData("archive", FfmpegVideoEncoderPreset.Archive)]

        [InlineData("s", FfmpegVideoEncoderPreset.None)]
        public void PresetInputMappingTest___TestParse___Works(string input, FfmpegVideoEncoderPreset expected)
        {
            StackingEnvironmentBuilderExtensions.MapInputToPreset(input).Should().Be(expected);
        }
    }
}
