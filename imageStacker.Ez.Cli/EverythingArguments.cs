using CommandLine;

namespace imageStacker.Ez.Cli
{
    [Verb("all")]
    public class EverythingArguments : GenericArguments
    {
        [Option("outputFolder", Default = ".", HelpText = "Where output files shall be created")]
        public string OutputFolder { get; set; }
    }
}
