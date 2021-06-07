using CommandLine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace imageStacker.Ez.Cli
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            static async Task birdMode(BirdArguments args)
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
            };

            BirdArguments birdArgs = null;
            Parser.Default.ParseArguments<BirdArguments>(args)
                .WithParsed(bArgs => birdArgs = bArgs);

            if (birdArgs == null)
            {
                Console.WriteLine("No args given");
                return;
            }

            await birdMode(birdArgs);

            // bird mode:
            /*
            1. Specify list of images or video
                1a) if video, extract frames to temp folder 
                // 1b) if images via folder, order them by date & name

            2. Stackall for gapsize, with i+gapsize while images available to tempfolder2
            3. Combine into Video with copyfilter
             */

            /* star mode:
             1. Make normal timelapse
             2. Make Max-stack Timelapse
             3. Make Max-Stack Image
             4. Make Attack-Decay- (HS) Timelapse    
             */

            /*
             
             
             
             */
        }

        [Verb("bird")]
        public class BirdArguments
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

            [Option("gapSize", Required = true, HelpText = "How many pictures to skip between frames")]
            public int GapSize { get; set; }

            [Option("filter", Required = false, Separator = ',', HelpText = "List of Filters with respective parameters; Example: 'MaxFilter Name=Max,AttackDecayFilter Attack=1.0 Decay=0.2 '")]
            public string Filter { get; set; }

            [Option("temp", HelpText = "Tempfolder; specify to use something different than your default temp folder for storing intermediary data")]

            public string TempFolder { get; set; }

            [Option("outputVideoFile", HelpText = "Video output filename; ignores outputFolder parameter")]
            public string OutputVideoFile { get; set; }
        }
    }
}
