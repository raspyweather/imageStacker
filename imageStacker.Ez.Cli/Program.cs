using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Ez.Cli
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Task executionTask = Task.CompletedTask;

            Parser.Default.ParseArguments<BirdArguments, StarArguments, CarArguments, EverythingArguments>(args)
                    .WithParsed<BirdArguments>(bArgs => executionTask = ProcessData(bArgs))
                    .WithParsed<StarArguments>(bArgs => executionTask = ProcessData(bArgs))
                    .WithParsed<EverythingArguments>(bArgs => executionTask = ProcessData(bArgs))
                    .WithParsed<CarArguments>(bArgs => executionTask = ProcessData(bArgs));

            await executionTask;

            /* bird mode:
            1. Specify list of images or video
            2. Stackall for gapsize, with i+gapsize while images available to tempfolder2
            3. Combine into Video with copyfilter */

            /* star mode:
             1. Make normal timelapse
             2. Make Max-stack Timelapse
             3. Make Max-Stack Image
             4. Make Attack-Decay- (HS) Timelapse */

            /* car mode (rqual with star mode)
              1. Make Max-stack Image
              2. Make Max-stack Video
              3. Make Attack-Decay (HS) Timelapse
              4. Make normal timelapse
             */

            /* Everything 
             1. Make normal timelapse
             2. Make Min,Max,AT-HF,AT-HS,AT-LF,AT-LS stack Timelapse
             3. Make Min,Max-Stack Image
             */
        }

        private static async Task ProcessData(EverythingArguments args)
        {
            string outputSubfolder = System.IO.Path.GetFileName(args.InputFolder) ??
                                     System.IO.Path.GetFileNameWithoutExtension(args.InputVideoFile) ??
                                     System.IO.Path.GetFileNameWithoutExtension(args.InputFiles.FirstOrDefault());

            string outputFolder = System.IO.Path.GetFullPath(args.OutputFolder);

            var topDirectoryInfo = System.IO.Directory.CreateDirectory(outputFolder);

            var dirInfo = topDirectoryInfo.CreateSubdirectory(outputSubfolder);

            var sourceArgs = GetSourceParameter(args);

            { // Create Videos
                (string filterName, string filterParam)[] filterArgs = {
                    ("timelapse","CopyFilter"),
                    ("MinFilter","MinFilter"),
                    ("MaxFilter","MaxFilter"),
                    ("AttackDecay-HF","AttackDecayFilter Attack=1.0,Decay=0.2"),
                    ("AttackDecay-HS","AttackDecayFilter Attack=1.0,Decay=0.01"),
                    ("AttackDecay-LF","AttackDecayFilter Attack=0.2,Decay=1"),
                    ("AttackDecay-LS","AttackDecayFilter Attack=0.01,Decay=1"),
                    ("AttackDecay-Hav","AttackDecayFilter Attack=0.5,Decay=0.5"),
                    ("AttackDecay-Lav","AttackDecayFilter Attack=0.01,Decay=0.01")
                };

                Console.WriteLine("Creating Vidoes");

                foreach (var (filterName, filterParam) in filterArgs)
                {
                    string filename = System.IO.Path.Combine(dirInfo.FullName, $"i-{filterName}.mp4");
                    string command = $"stackProgressive {sourceArgs} --filters={filterParam} --outputVideoFile={filename}";
                    await imageStacker.Cli.Program.Main(command.Split(' '));
                }
            }
            { // Create Stacked Images
                Console.WriteLine("Creating Stacked Images");
                string filters = "MinFilter, MaxFilter";
                string command = $"stackAll {sourceArgs} --filters={filters} --outputFilePrefix=i --outputFolder={dirInfo.FullName}";
                await imageStacker.Cli.Program.Main(command.Split(' '));
            }

            Console.WriteLine("Done. Thanks :) ");
        }
        private static async Task ProcessData(BirdArguments args)
        {
            try
            {
                string initialFramesFolder, intermediaryFramesFolder;

                if (!string.IsNullOrWhiteSpace(args.TempFolder))
                {
                    if (!System.IO.Directory.Exists(args.TempFolder))
                    {
                        Console.WriteLine($"Folder \"{args.TempFolder}\" seems not to exist. Please provide a valid destination.");
                        return;
                    }
                }

                string tempFolderToUse = args.TempFolder ?? System.IO.Path.GetTempPath();

                initialFramesFolder = System.IO.Path.Combine(tempFolderToUse,
                    System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName()) + "-extractedFrames");
                intermediaryFramesFolder = System.IO.Path.Combine(tempFolderToUse,
                    System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName()) + "-intermediaryFrames");

                try
                {
                    System.IO.Directory.CreateDirectory(initialFramesFolder);
                    System.IO.Directory.CreateDirectory(intermediaryFramesFolder);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Could not create temporary folders - exiting.");
                    Console.WriteLine(e.ToString());
                    return;
                }


                if (!string.IsNullOrWhiteSpace(args.InputVideoFile))
                {
                    string fullPathToVideo = System.IO.Path.GetFullPath(args.InputVideoFile);
                    string useArgs = string.IsNullOrWhiteSpace(args.InputVideoArguments) ? "" : $"--inputVideoArguments={args.InputVideoArguments}";
                    // extract frames
                    string command = $"stackProgressive {useArgs} --inputVideoFile={fullPathToVideo} --filters=CopyFilter --outputFilePrefix=i --outputFolder={initialFramesFolder}";
                    await imageStacker.Cli.Program.Main(command.Split(' '));
                }
                else
                {
                    initialFramesFolder = args.InputFolder;
                }

                string[] files = Array.Empty<string>();

                if (!string.IsNullOrWhiteSpace(initialFramesFolder))
                {
                    string searchPattern = args.InputFilter ?? "*.*";
                    files = System.IO.Directory.GetFiles(initialFramesFolder, searchPattern);
                    Console.WriteLine($"\n{files.Length} initial frames found");
                }


                /*
                 Gap size 5; 20 frames
                1,6,11,16
                2,7,12,17
                3,8,13,18,
                4,9,14,19
                5,10,15,20
                 */
                for (int i = 0; i < args.GapSize; i++)
                {
                    Console.Write($"Stack Nr {i} -");
                    var intermediaryStack = new List<string>();
                    for (int ii = i; ii < files.Length; ii += args.GapSize)
                    {
                        intermediaryStack.Add(files[ii]);
                        Console.Write($"{ii}-");
                    }
                    Console.WriteLine();

                    string command = $"stackAll --inputFiles={string.Join(" ", intermediaryStack)} --filters={args.Filter} --outputFilePrefix=i-{i:d6} --outputFolder={intermediaryFramesFolder}";
                    await imageStacker.Cli.Program.Main(command.Split(' '));
                }
                {
                    var intermediaryFiles = System.IO.Directory.GetFiles(intermediaryFramesFolder);
                    if (intermediaryFiles.Length == 0)
                    {
                        Console.WriteLine("No intermediaries to combine!");
                        return;
                    }
                    // Create final Video
                    string command = $"stackProgressive --inputFiles={string.Join(" ", intermediaryFiles)} --filters=CopyFilter --outputVideoFile={args.OutputVideoFile}";
                    await imageStacker.Cli.Program.Main(command.Split(' '));
                }


                Console.Write("Cleaning up");
                System.IO.Directory.Delete(initialFramesFolder, true);
                System.IO.Directory.Delete(intermediaryFramesFolder, true);
                Console.WriteLine(" ...Done!");

                System.Diagnostics.Process.Start("explorer", args.OutputVideoFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static string GetSourceParameter(ISourceArguments args)
            => !string.IsNullOrWhiteSpace(args.InputVideoFile) ?
                   $"--inputVideoFile={args.InputVideoFile} " + (!string.IsNullOrWhiteSpace(args.InputVideoArguments) ? $"--inputVideoOptions={args.InputVideoArguments}" : "") :
                   (args.InputFiles == null || !args.InputFiles.Any()) ?
                        $"--inputFolder={args.InputFolder} --inputFilter={args.InputFilter}" :
                        $"--inputFiles={string.Join(" ", args.InputFiles)}";


        private static async Task ProcessData(StarArguments args)
        {
            var sourceArg = GetSourceParameter(args);

            var outputPrefix = !string.IsNullOrWhiteSpace(args.OutputPrefix) ? args.OutputPrefix : args.InputFolder ?? System.IO.Path.GetDirectoryName(args.InputFiles.First());

            {
                Console.WriteLine("Creating normal timelapse");
                string command = $"stackProgressive {sourceArg} --filters=CopyFilter --outputVideoFile={outputPrefix}-timelapse.mp4";
                await imageStacker.Cli.Program.Main(command.Split(' '));
            }
            {
                Console.WriteLine("Creating max-stack image");
                string command = $"stackAll {sourceArg} --filters=MaxFilter --outputFolder={args.OutputFolder} --outputFilePrefix={outputPrefix}";
                await imageStacker.Cli.Program.Main(command.Split(' '));
            }
            {
                Console.WriteLine("Creating max timelapse");
                string command = $"stackProgressive {sourceArg} --filters=MaxFilter --outputVideoFile={outputPrefix}-max.mp4";
                await imageStacker.Cli.Program.Main(command.Split(' '));
            }
            {
                Console.WriteLine("Creating High-Attack-Fast-Decay timelapse");
                string command = $"stackProgressive {sourceArg} --filters=AttackDecayFilter Attack=1.0,Decay=0.5 --outputVideoFile={outputPrefix}-at-HF.mp4";
                await imageStacker.Cli.Program.Main(command.Split(' '));
            }
        }

        public interface ISourceArguments
        {
            string InputVideoFile { get; }
            IEnumerable<string> InputFiles { get; }
            string InputFolder { get; }
            string InputFilter { get; }
            string InputVideoArguments { get; }
        }

        [Verb("stars")]
        public class StarArguments : GenericArguments
        {
            [Option("outputPrefix", Required = false, HelpText = "Partial name of files produced")]
            public string OutputPrefix { get; set; }

            [Option("outputFolder", Default = ".", HelpText = "Where output files shall be created")]
            public string OutputFolder { get; set; }
        }

        [Verb("cars")]
        public class CarArguments : StarArguments { }

        [Verb("bird")]
        public class BirdArguments : GenericArguments
        {

            [Option("gapSize", Required = true, HelpText = "How many pictures to skip between frames")]
            public int GapSize { get; set; }

            [Option("temp", HelpText = "Tempfolder; specify to use something different than your default temp folder for storing intermediary data")]

            public string TempFolder { get; set; }

            [Option("outputVideoFile", HelpText = "Video output filename; ignores outputFolder parameter")]
            public string OutputVideoFile { get; set; }
        }

        [Verb("all")]
        public class EverythingArguments : GenericArguments
        {
            [Option("outputFolder", Default = ".", HelpText = "Where output files shall be created")]
            public string OutputFolder { get; set; }
        }

        public abstract class GenericArguments : ISourceArguments
        {
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

            [Option("filter", Required = false, Separator = ',', HelpText = "List of Filters with respective parameters; Example: 'MaxFilter Name=Max,AttackDecayFilter Attack=1.0 Decay=0.2 '")]
            public string Filter { get; set; }
        }
    }
}
