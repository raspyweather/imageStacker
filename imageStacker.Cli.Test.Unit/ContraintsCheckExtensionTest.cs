using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace imageStacker.Cli.Test.Unit
{
    public class ContraintsCheckExtensionTest
    {
        [Fact]
        public void InputVideoDecodeConfiguration_MultipleFilters_InvalidConfigDetected()
        {
            var parameters = new StackAllOptions
            {
                InputVideoFile = "a.mp4",
                InputVideoArguments = "-vf scale=-1:1080",
                Filters = new List<string> { "MaxFilter", "MinFilter" },
                OutputVideoFile = "a2.mp4"
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureInputMode(parameters);
            basicEnvironment.ConfigureFilters(parameters.Filters);
            basicEnvironment.ConfigureOuptutMode(parameters);

            basicEnvironment.ThrowMe.Should().BeFalse();
            basicEnvironment.Filters.Should().HaveCount(2);

            basicEnvironment.CheckConstraints();

            basicEnvironment.ThrowMe.Should().BeTrue();
        }
    }
}
