using CommandLine;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using System.Collections.Generic;

namespace imageStacker.Cli
{
    public interface IStackingEnvironment
    {
        public IMutableImageFactory<MutableByteImage> Factory { get; set; }
        public List<IFilter<MutableByteImage>> Filters { get; set; }
        public IImageReader<MutableByteImage> InputMode { get; set; }
        public IImageWriter<MutableByteImage> OutputMode { get; set; }
        public ILogger Logger { get; set; }
        public IImageProcessingStrategy<MutableByteImage> ProcessingStrategy { get; set; }
        public bool ThrowMe { get; set; }

        public bool IsOrderRelevant { get; set; }
    }

    public class StackingEnvironment : IStackingEnvironment
    {
        public IMutableImageFactory<MutableByteImage> Factory { get; set; }
        public List<IFilter<MutableByteImage>> Filters { get; set; }
        public IImageReader<MutableByteImage> InputMode { get; set; }
        public IImageWriter<MutableByteImage> OutputMode { get; set; }
        public ILogger Logger { get; set; }
        public IImageProcessingStrategy<MutableByteImage> ProcessingStrategy { get; set; }
        public bool ThrowMe { get; set; }
        public bool IsOrderRelevant { get; set; }
    }

    public interface ICommonOptions
    {
        bool UseOutputPipe { get; set; }

        string OutputFolder { get; set; }

        bool UseInputPipe { get; set; }
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
        [Option("ffmpegLocation", HelpText = "Path to ffmpeg executable")]
        public virtual string FfmpegLocation { get; set; }

        #region InputFromPipe
        [Option("inputFromPipe", Required = false, HelpText = "Used to use stdin as input, required inputSize to be specified. Only bgr24 is currently supported.")]
        public virtual bool UseInputPipe { get; set; }

        [Option("inputSize", HelpText = "Necessary for using stdin Format: 1920x1080", Required = false)]
        public virtual string InputSize { get; set; }
        #endregion

        #region InputFromFiles
        [Option("inputFiles", Required = false, HelpText = "List of image files to be used as stacking input")]
        public virtual IEnumerable<string> InputFiles { get; set; }
        #endregion

        #region InputFromFolder
        [Option("inputFolder", Required = false, HelpText = "Folder to select input files from.")]
        public virtual string InputFolder { get; set; }

        [Option("inputFilter", HelpText = "Filter for enumerating files of specified inputFolder, e.g. *.jpg")]
        public virtual string InputFilter { get; set; }
        #endregion

        #region InputFromVideo

        [Option("inputVideoFile", Required = false, HelpText = "Video file to extract frames from")]
        public virtual string InputVideoFile { get; set; }

        [Option("inputVideoArguments", Required = false, HelpText = "Arguments passed to ffmpeg for decoding. See ffmpeg -help for details")]
        public virtual string InputVideoArguments { get; set; }
        #endregion

        #region OutputToImages
        [Option("outputFolder", Required = false, HelpText = "Folder in which to create files. Necessary parameter for image series or single frame output.")]
        public virtual string OutputFolder { get; set; }

        [Option("outputFilePrefix", HelpText = "Name Prefix of the output file written to the disk. Naming scheme: $outputFolder/$prefix-$filterName-$counter.png", Required = false)]
        public virtual string OutputFilePrefix { get; set; }
        #endregion

        #region OutputToPipe
        [Option("outputToPipe", Required = false, HelpText = "Used to forward output to stdout (bgr24)")]
        public virtual bool UseOutputPipe { get; set; }
        #endregion

        #region OutputToVideo
        [Option("outputVideoFile", HelpText = "Video output filename; ignores outputFolder parameter")]
        public virtual string OutputVideoFile { get; set; }

        [Option("outputVideoOptions", HelpText = "ffmpeg Options")]
        public virtual string OutputVideoOptions { get; set; }

        [Option("outputPreset", Required = false, HelpText = "Presets for output encoding e.g. fhd, 4k, archive")]
        public virtual string OutputPreset { get; set; } = "fhd";
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
