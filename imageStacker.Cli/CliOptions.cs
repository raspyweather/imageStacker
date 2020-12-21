using CommandLine;
using System.Collections.Generic;

namespace imageStacker.Cli
{
    public interface ICommonOptions
    {
        bool UseOutputPipe { get; set; }

        string OutputFolder { get; set; }

        bool UseInputPipe { get; set; }
    }
    public enum InputOption
    {
        Files,
        Stream
    }
    public enum OutputOption
    {
        File,
        Stream
    }

    [Verb("test")]
    public class TestOptions : CommonOptions
    {
        [Option("Count")]
        public int Count { get; set; } = 50;

        [Option("Width")]
        public int Width { get; set; } = 4 * 1920;

        [Option("Height")]
        public int Height { get; set; } = 4 * 1080;

    }

    public abstract class CommonOptions : ICommonOptions
    {
        [Option("outputToPipe", Required = false)]
        public virtual bool UseOutputPipe { get; set; }
        [Option("outputFolder", Required = false)]
        public virtual string OutputFolder { get; set; }
        [Option("inputFromPipe", Required = false)]
        public virtual bool UseInputPipe { get; set; }

        [Option("inputSize", HelpText = "Format: 1920x1080", Required = false)]
        public virtual string InputSize { get; set; }

        [Option("inputFiles", Required = false)]
        public virtual IEnumerable<string> InputFiles { get; set; }

        [Option("outputFile", HelpText = "Name Prefix of the output file written to the disk", Required = false)]
        public virtual string OutputFile { get; set; }

        [Option("inputFolder", Required = false)]
        public virtual string InputFolder { get; set; }

        [Option("inputFilter", HelpText = "Filter for enumerating files of specified inputFolder, e.g. *.jpg")]
        public virtual string InputFilter { get; set; }

        [Option("filters", Required = false, Separator = ',', HelpText = "List of Filters with respective parameters; Example: 'MaxFilter Name=Max,AttackDecayFilter Attack=1.0 Decay=0.2 '")]
        public virtual IEnumerable<string> Filters { get; set; }

        public override string ToString()
        {
            return $"{GetType()} " +
                   $"{nameof(UseOutputPipe)}={UseOutputPipe}," +
                   $"{nameof(OutputFolder)}={OutputFolder}," +
                   $"{nameof(UseInputPipe)}={UseInputPipe}," +
                   $"{nameof(InputFiles)}={string.Join(";", InputFiles)}," +
                   $"{nameof(InputFolder)}={InputFolder}";
        }
    }

    [Verb("stackImage")]
    public class StackAllOptions : CommonOptions
    { }

    [Verb("info", HelpText = "[Dev Functionality] can be used to retrieve dimensions of pictures")]
    public class InfoOptions : CommonOptions
    { }

    [Verb("stackProgressive")]
    public class StackProgressiveOptions : CommonOptions
    {
        public int StartCount { get; set; }
        public int EndCount { get; set; }
    }

    [Verb("stackContinuous")]
    public class StackContinuousOptions : CommonOptions
    {
        [Option("StackCount")]
        public int Count { get; set; }
    }
}
