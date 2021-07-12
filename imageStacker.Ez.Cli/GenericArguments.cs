using CommandLine;
using System.Collections.Generic;

namespace imageStacker.Ez.Cli
{
    public abstract class GenericArguments : ISourceArguments
    {

        [Option("temp", HelpText = "Tempfolder; specify to use something different than your default temp folder for storing intermediary data")]
        public string TempFolder { get; set; }

        #region InputFromFiles
        [Option("inputFiles", Required = false, HelpText = "List of image files to be used as stacking input")]
        public virtual IEnumerable<string> InputFiles { get; set; }
        #endregion

        #region InputFromFolder
        [Option("inputFolder", Required = false, HelpText = "Folder to select input files from.")]
        public string InputFolder { get; set; }

        [Option("inputFilter", HelpText = "Filter for enumerating files of specified inputFolder, e.g. *.jpg")]
        public string InputFilter { get; set; }
        #endregion

        #region InputFromVideo
        [Option("inputVideoFile", Required = false, HelpText = "Video file to extract frames from")]
        public string InputVideoFile { get; set; }

        [Option("inputVideoArguments", Required = false, HelpText = "Arguments passed to ffmpeg for decoding. See ffmpeg -help for details")]
        public string InputVideoArguments { get; set; }
        #endregion
    }
}
