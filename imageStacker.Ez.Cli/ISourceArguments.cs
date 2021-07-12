using System.Collections.Generic;

namespace imageStacker.Ez.Cli
{
    public interface ISourceArguments
    {
        string InputVideoFile { get; }
        IEnumerable<string> InputFiles { get; }
        string InputFolder { get; }
        string InputFilter { get; }
        string InputVideoArguments { get; }
    }
}
