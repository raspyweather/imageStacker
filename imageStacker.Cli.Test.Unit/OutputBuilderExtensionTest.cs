using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace imageStacker.Cli.Test.Unit
{
    public class OutputBuilderExtensionTest
    {
        [Fact]
        public void OutputVideoFileConfiguration_VideoWriter_IsCreated()
        {
            var parameters = new StackProgressiveOptions
            {
                OutputVideoFile = "a.mp4"
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureOuptutMode(parameters);

            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.OutputMode.Should().NotBeNull();
            basicEnvironment.OutputMode.Should().BeOfType<ffmpeg.FfmpegVideoWriter>();
        }

        [Fact]
        public void OutputVideoFileConfiguration_VideoWriter_UsesCustomArguments()
        {
            var parameters = new StackProgressiveOptions
            {
                OutputVideoFile = "a.mp4",
                OutputVideoOptions = "-crf 24 -vf scale=-1:1080"
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureOuptutMode(parameters);

            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.OutputMode.Should().NotBeNull();
            basicEnvironment.OutputMode.Should().BeOfType<ffmpeg.FfmpegVideoWriter>();
        }

        [Fact]
        public void OutputFileConfiguration_FileWriter_IsCreated()
        {
            var parameters = new StackAllOptions
            {
                OutputFolder = ".",
                OutputFilePrefix = "timelapse"
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureOuptutMode(parameters);

            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.OutputMode.Should().NotBeNull();
        }

        [Fact]
        public void OutputStreamConfiguration_StreamWriter_IsCreated()
        {
            var parameters = new StackAllOptions
            {
                UseOutputPipe = true
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureOuptutMode(parameters);

            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.OutputMode.Should().NotBeNull();
        }

        [Fact]
        public void OutputFileConfiguration_EmptyConfiguration_SetFlagThrowMe()
        {
            var parameters = new StackAllOptions
            { };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureOuptutMode(parameters);

            basicEnvironment.ThrowMe.Should().BeTrue();
        }

    }
}
