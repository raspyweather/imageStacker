using FluentAssertions;
using imageStacker.Core;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Readers;
using System;
using System.IO;
using Xunit;

namespace imageStacker.Cli.Test.Unit
{
    public class InputModeBuilderExtensionTest
    {
        [Fact]
        public void InputFileConfiguration_FileReader_IsCreated()
        {
            var parameters = new StackAllOptions
            {
                InputFiles = new string[] { "a.jpg", "b.jpg", "c.jpg" },
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);

            basicEnvironment.InputMode.Should().NotBeNull();
            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.InputMode.Should().BeOfType<ImageMutliFileOrderedReader<MutableByteImage>>();
        }

        [Fact]
        public void InputFolderConfiguration_FolderDoesNotExist_ThrowMeFlagSet()
        {
            var parameters = new StackAllOptions
            {
                InputFolder = "test",
            };
            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);

            basicEnvironment.ThrowMe.Should().BeTrue();
        }

        [Fact]
        public void InputConfiguration_NotSpecified_ThrowMeFlagSet()
        {
            var parameters = new StackAllOptions();
            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);

            basicEnvironment.ThrowMe.Should().BeTrue();
        }

        [Fact]
        public void InputStreamConfiguration_SizeNotSpecified_ThrowMeFlagSet()
        {
            var parameters = new StackAllOptions
            {
                UseInputPipe = true,
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);

            basicEnvironment.ThrowMe.Should().BeTrue();
        }

        [Fact]
        public void InputStreamConfiguration_SizeNotParseable_ThrowMeFlagSet()
        {
            var parameters = new StackAllOptions
            {
                UseInputPipe = true,
                InputSize = "120:-"
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);

            basicEnvironment.ThrowMe.Should().BeTrue();
        }

        [Fact]
        public void InputStreamConfiguration_SizeParseable_InputReaderCreated()
        {
            var parameters = new StackAllOptions
            {
                UseInputPipe = true,
                InputSize = "1x20:1080"
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);

            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.InputMode.Should().NotBeNull();
        }
    }
}
