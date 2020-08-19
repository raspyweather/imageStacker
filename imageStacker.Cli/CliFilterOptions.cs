using CommandLine;
using imageStacker.Core.Abstraction;

namespace imageStacker.Cli
{
    public abstract class CommonFilterOptions
    {
        [Option(longName: "Name", Required = false)]
        public string Name { get; set; }
    }

    [Verb("MaxFilter")]
    public class MaxFilterOptions : CommonFilterOptions, IMaxFilterOptions
    {
    }


    [Verb("MinFilter")]
    public class MinFilterOptions : CommonFilterOptions, IMinFilterOptions
    {
    }

    [Verb("AttackDecayFilter")]
    public class AttackDecayFilterOptions : CommonFilterOptions, IAttackDecayOptions
    {
        [Option(longName: "Decay", Required = true)]
        public float Decay { get; set; } = 0.2f;

        [Option(longName: "Attack", Required = true)]
        public float Attack { get; set; } = 1.0f;
    }
}
