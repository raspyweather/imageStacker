using System.Collections.Generic;

namespace imageStacker.Ez.Cli
{
    partial class Program
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
}
