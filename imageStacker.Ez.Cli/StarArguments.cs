using CommandLine;

namespace imageStacker.Ez.Cli
{
    [Verb("stars")]
    public class StarArguments : GenericArguments
    {
        [Option("filter", Required = false, Separator = ',', HelpText = "List of Filters with respective parameters; Example: 'MaxFilter Name=Max,AttackDecayFilter Attack=1.0 Decay=0.2 '")]
        public string Filter { get; set; }

        [Option("outputPrefix", Required = false, HelpText = "Partial name of files produced")]
        public string OutputPrefix { get; set; }

        [Option("outputFolder", Default = ".", HelpText = "Where output files shall be created")]
        public string OutputFolder { get; set; }
    }
}
