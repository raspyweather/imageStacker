using CommandLine;

namespace imageStacker.Ez.Cli
{
    partial class Program
    {
        [Verb("bird")]
        public class BirdArguments : GenericArguments
        {
            [Option("filter", Required = false, Separator = ',', HelpText = "List of Filters with respective parameters; Example: 'MaxFilter Name=Max,AttackDecayFilter Attack=1.0 Decay=0.2 '")]
            public string Filter { get; set; }

            [Option("gapSize", Required = true, HelpText = "How many pictures to skip between frames")]
            public int GapSize { get; set; }

            [Option("outputVideoFile", HelpText = "Video output filename; ignores outputFolder parameter")]
            public string OutputVideoFile { get; set; }
        }
    }
}
