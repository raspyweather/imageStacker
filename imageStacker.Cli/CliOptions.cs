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
        #region InputFromPipe
        [Option("inputFromPipe", Required = false)]
        public virtual bool UseInputPipe { get; set; }

        [Option("inputSize", HelpText = "Format: 1920x1080", Required = false)]
        public virtual string InputSize { get; set; }
        #endregion

        #region InputFromFiles
        [Option("inputFiles", Required = false)]
        public virtual IEnumerable<string> InputFiles { get; set; }
        #endregion

        #region InputFromFolder
        [Option("inputFolder", Required = false)]
        public virtual string InputFolder { get; set; }

        [Option("inputFilter", HelpText = "Filter for enumerating files of specified inputFolder, e.g. *.jpg")]
        public virtual string InputFilter { get; set; }
        #endregion

        #region OutputToImages
        [Option("outputFolder", Required = false)]
        public virtual string OutputFolder { get; set; }

        [Option("outputFilePrefix", HelpText = "Name Prefix of the output file written to the disk", Required = false)]
        public virtual string OutputFilePrefix { get; set; }
        #endregion

        #region OutputToPipe
        [Option("outputToPipe", Required = false)]
        public virtual bool UseOutputPipe { get; set; }
        #endregion

        #region OutputToVideo
        [Option("outputFile")]
        public virtual string OuputVideoFile { get; set; }

        [Option("outputVideoOptions", HelpText = "ffmpeg Options")]
        public virtual string OutputVideoOptions { get; set; }

        [Option("outputPreset", HelpText = "Presets for output encoding e.g. FHD, 4K")]
        public virtual string OutputPreset { get; set; }
        #endregion

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

    [Verb("stackAll")]
    public class StackAllOptions : CommonOptions
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
        [Option("stackCount")]
        public int Count { get; set; }
    }

    [Verb("info", HelpText = "[Dev Functionality] can be used to retrieve dimensions of pictures")]
    public class InfoOptions : CommonOptions
    { }
}
