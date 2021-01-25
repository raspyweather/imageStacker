using FluentAssertions;
using imageStacker.Core;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace imageStacker.Cli.Test.Unit
{
    public class FilterBuilderExtensionTest
    {
        [Fact]
        public void FilterConfiguration_NoConfiguration_SetsThrowMeFlag()
        {
            var parameters = new StackAllOptions { };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            basicEnvironment.ThrowMe.Should().BeTrue();
        }

        [Theory]
        [InlineData("--filters MaxFilter")]
        [InlineData("--filters MinFilter")]
        public void FilterConfiguration_FilterConfig_CreatesRequestedFilter(string input)
        {
            var parameters = new StackAllOptions
            {
                Filters = input.Split(' ')
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            basicEnvironment.Filters.Count.Should().Be(1);
            basicEnvironment.ThrowMe.Should().BeFalse();
        }

        [Fact]
        public void FilterConfiguration_AttackDecayFilter_CreatesAttackDecayFilter()
        {
            var parameters = new StackAllOptions
            {
                Filters = "--filters AttackDecayFilter Attack=1.0 Decay=0.2".Split(' ')
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            basicEnvironment.Filters.Count.Should().Be(1);
            basicEnvironment.ThrowMe.Should().BeFalse();
        }

        [Theory]
        [InlineData("--filters AttackDecayFilter Attack=1.0 Decay=0.2", 1.0, 0.2)]
        [InlineData("--filters AttackDecayFilter Attack=-1.0 Decay=0.2", -1.0, 0.2)]
        [InlineData("--filters AttackDecayFilter Attack=0.2 Decay=-1.0", 0.2, -1.0)]
        [InlineData("--filters AttackDecayFilter Attack=0.2 Decay=1.0", 0.2, 1.0)]
        public void FilterConfiguration_AttackDecayFilter_ParametersSetCorrectly(string input, float expectedAttack, float expectedDecay)
        {
            var parameters = new StackAllOptions
            {
                Filters = input.Split(' ')
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            var createdFilter = basicEnvironment.Filters.Single().As<IAttackDecayFilter<MutableByteImage>>();
            createdFilter.Attack.Should().Be(expectedAttack);
            createdFilter.Decay.Should().Be(expectedDecay);
        }

        [Fact]
        public void FilterConfiguration_DefaultFilterName_NameIsSet()
        {
            var parameters = new StackAllOptions
            {
                Filters = "--filters AttackDecayFilter Attack=0.2 Decay=1.0, MaxFilter, MinFilter".Split(' ')
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            var createdFilters = basicEnvironment.Filters.Cast<IFilter<MutableByteImage>>().ToList();

            createdFilters.Count.Should().Be(3);
            createdFilters.Select(x => x.Name).Distinct().Count().Should().Be(3);
        }

        [Theory]
        [InlineData("--filters AttackDecayFilter Attack=0.2 Decay=1.0, MaxFilter, MinFilter", 3)]
        [InlineData("--filters AttackDecayFilter Attack=0.2 Decay=1.0,MaxFilter,MinFilter", 3)]
        [InlineData("--filters=AttackDecayFilter Attack=0.2 Decay=1.0,MaxFilter,MinFilter", 3)]
        [InlineData("--filters=AttackDecayFilter Attack=0.2 Decay=1.0 , MaxFilter , MinFilter", 3)]
        [InlineData("--filters=MaxFilter Name=max1,MaxFilter Name=max2,MaxFilter Name=max3", 3)]
        public void FilterConfiguration_AllowsFriendlyInput_InputsParseCorrectly(string input, int expectedFilters)
        {
            var parameters = new StackAllOptions
            {
                Filters = input.Split(' ')
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            basicEnvironment.Filters.Cast<IFilter<MutableByteImage>>().Should().HaveCount(expectedFilters);

        }

        [Theory]
        [InlineData("--filters AttackDecayFilter Name=cookie Attack=1.0 Decay=0.2", "cookie")]
        [InlineData("--filters MaxFilter Name=cookie", "cookie")]
        [InlineData("--filters MinFilter Name=cookie", "cookie")]
        public void FilterConfiguration_AttackiDecayFilter_ParametersSetCorrectly(string input, string expectedName)
        {
            var parameters = new StackAllOptions
            {
                Filters = input.Split(' ')
            };

            var basicEnvironment = new StackingEnvironment();

            basicEnvironment.ConfigureFilters(parameters.Filters);

            var createdFilter = basicEnvironment.Filters.Single().As<IFilter<MutableByteImage>>();
            createdFilter.Name.Should().Be(expectedName);
        }


    }
}
