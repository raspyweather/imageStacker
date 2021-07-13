using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Ez.Batch.Cli
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            BatchArguments arguments = null;
            var parser = Parser.Default.ParseArguments<BatchArguments>(args)
                .WithParsed(args => arguments = args);

            if (arguments == null)
            {
                Console.WriteLine("No args given");
                return;
            }

            if (!System.IO.Directory.Exists(arguments.OutputFolder))
            {
                Console.WriteLine("Output Folders don't exist");
                return;
            }

            bool useInputFoldersFolder = System.IO.Directory.Exists(arguments.InputFoldersFolder);
            bool useInputVideosFolder = System.IO.Directory.Exists(arguments.InputVideosFolder);

            if (!useInputFoldersFolder || !useInputVideosFolder)
            {
                Console.WriteLine("No existing input defined");
                return;
            }

            List<string> inputFolderFolders = (useInputFoldersFolder ?
                System.IO.Directory.GetDirectories(arguments.InputFoldersFolder).ToList() :
                new List<string>()).ToList();

            List<string> inputVideoFolders = useInputVideosFolder ?
                System.IO.Directory.GetDirectories(arguments.InputVideosFolder).ToList() :
                new List<string>();

            string[] videoExtensions = { "mov", "mp4", "avi" };

            List<string> inputVideos = inputVideoFolders.SelectMany(x =>
                System.IO.Directory.GetFiles(x).Where(x =>
                    videoExtensions.Any(y => x.EndsWith(y, StringComparison.InvariantCultureIgnoreCase)
            ))).ToList();

            Console.WriteLine($"Found {inputFolderFolders.Count} Image Folders, {inputVideos.Count} Videos");

            foreach (var inputFolder in inputFolderFolders)
            {
                try
                {
                    await Ez.Cli.Program.Main($"all --inputFolder={inputFolder} --inputFilter=*.JPG --outputFolder={arguments.OutputFolder}".Split());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            foreach (var videoFile in inputVideos)
            {
                try
                {
                    string folderName = System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(videoFile));
                    await Ez.Cli.Program.Main($"all --inputVideoFile={videoFile} --outputFolder={System.IO.Path.Combine(arguments.OutputFolder, folderName)}".Split());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }

    [Verb("batch")]
    internal class BatchArguments
    {
        [Option("inputFoldersFolder")]
        public string InputFoldersFolder { get; set; }

        [Option("inputVideosFolder")]
        public string InputVideosFolder { get; set; }

        [Option("outputFolder")]
        public string OutputFolder { get; set; }
    }
}
