using CommandLine;
using imageStacker.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace imageStacker.Cli
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Proposed Syntax - huge todo :D
            // imagestacker.exe single --folder ./cookies --output 1.png

            // Features:
            // Verbs
            //  - linear, single, ramp
            // File input
            //  - --files --folder --except --reverse --skip
            //
            //            args = new string[] { @"stackImage", @"--inputFolder=C:\Users\armbe\OneDrive\Dokumente\PlatformIO\Projects\imageStacker\imageStacker.Piping.Cli\data", "--outputFolder=.", "--outputFile=stacked" };
            //args = @"stackImage --inputFolder=L:\Canada\timelapses\202006242150 --outputFile=202006242150 --outputFolder=.".Split(' ');
            args = @"stackContinuous --inputFolder=L:\Canada\timelapses\202003032204 --outputFile=202003032204 --StackCount=5 --outputFolder=.".Split(' ');
            Stopwatch st = new Stopwatch();
            st.Start();

            IImageReader inputMode = null;
            IImageWriter outputMode = null;
            List<IFilter> filters = new List<IFilter> { new MaxFilter() };
            IImageProcessingStrategy processingStrategy = null;
            bool throwMe = false;

            IImageReader setInput(CommonOptions commonOptions)
            {
                if (commonOptions.UseInputPipe)
                {
                    var inputSize = commonOptions.InputSize;
                    if (string.IsNullOrWhiteSpace(inputSize))
                    {
                        throw new ArgumentException("Inputsize must be defined for inputpipes ");
                    }

                    var wh = Regex.Split(inputSize, "[^0-9]");
                    int.TryParse(wh[0], out int width);
                    int.TryParse(wh[1], out int height);

                    return new ImageStreamReader(
                        Process.GetCurrentProcess().StandardInput.BaseStream,
                        width,
                        height,
                        PixelFormat.Format24bppRgb);
                }
                if (commonOptions.InputFiles != null && commonOptions.InputFiles.Count() > 0)
                {
                    return new ImageMutliFileReader(commonOptions.InputFiles.ToArray());
                }
                if (!string.IsNullOrWhiteSpace(commonOptions.InputFolder))
                {
                    Console.Error.WriteLineColored("Currently only *.jpg input supported", ConsoleColor.Yellow, ConsoleColor.Black);
                    if (!Directory.Exists(commonOptions.InputFolder))
                    {
                        Console.Error.WriteLineColored($"InputFolder does not exist {commonOptions.InputFolder}", ConsoleColor.Yellow, ConsoleColor.Black);
                    }
                    return new ImageMutliFileReader(commonOptions.InputFolder);
                }
                Console.Error.WriteLineColored("No Input Mode defined", ConsoleColor.Red, ConsoleColor.Black);
                throwMe = true;
                return null;
            }

            IImageWriter setOutput(CommonOptions commonOptions)
            {
                if (commonOptions.UseOutputPipe)
                {
                    return new ImageStreamWriter(Console.OpenStandardOutput());
                }

                if (!string.IsNullOrWhiteSpace(commonOptions.OutputFolder) &&
                    !string.IsNullOrWhiteSpace(commonOptions.OutputFile))
                {
                    return new ImageFileWriter(commonOptions.OutputFile, commonOptions.OutputFolder);
                }

                Console.Error.WriteLineColored("No Output Mode defined", ConsoleColor.Red, ConsoleColor.Black);
                Console.Error.WriteLine("Consider specifying --UseOutputPipe or --OutputFolder and --OutputFile");
                Console.Error.WriteLine();
                throwMe = true;
                return null;
            }

            var result = Parser.Default
                .ParseArguments<InfoOptions,
                                StackAllOptions,
                                StackProgressiveOptions,
                                StackContinuousOptions>(args)
                .WithParsed<InfoOptions>(info =>
                {
                    Console.WriteLine("[INFO]", ConsoleColor.White, ConsoleColor.Black);
                    GetInfo(info);
                    return;
                })
                .WithParsed<StackAllOptions>(info =>
                {
                    Console.WriteLine(info.ToString().Replace(",", Environment.NewLine));

                    // processingStrategy = new StackAllStrategy();
                    // might be unsafe
                    processingStrategy = new StackAllMergeStrategy();
                    inputMode = setInput(info);
                    outputMode = setOutput(info);

                })
                .WithParsed<StackProgressiveOptions>(info =>
                {
                    Console.WriteLine(info.ToString().Replace(",", Environment.NewLine));

                    processingStrategy = new StackProgressiveStrategy();
                    inputMode = setInput(info);
                    outputMode = setOutput(info);
                })
                .WithParsed<StackContinuousOptions>(info =>
                {
                    Console.WriteLine(info.ToString().Replace(",", Environment.NewLine));
                    if (info.Count == 0)
                    {
                        Console.WriteLine("You have to define --stackCount for continuous stacking");
                        throwMe = true;
                    }
                    processingStrategy = new StackContinousStrategy(info.Count);
                    inputMode = setInput(info);
                    outputMode = setOutput(info);
                })
                .WithNotParsed(x =>
                {
                    Console.Error.WriteLineColored(String.Join(Environment.NewLine, x.Select(y => y.ToString()).ToArray()), ConsoleColor.Red);
                });

            if (throwMe)
            {
                Console.Error.WriteLineColored("Invalid Configuration, see issues above.", ConsoleColor.Red, ConsoleColor.Black);
                return;
            }

            if (inputMode == null || outputMode == null)
            {
                Console.Error.WriteLine($"Input: {inputMode}, Output:{outputMode}");
                Console.Error.WriteLineColored("IO undefined", ConsoleColor.Red, ConsoleColor.Black);
                return;
            }
            if (processingStrategy == null)
            {
                Console.Error.WriteLineColored("Not processing strategy defined", ConsoleColor.Red, ConsoleColor.Black);
            }

            Console.WriteLine($"{inputMode} {outputMode} {processingStrategy}");
            /*
                        var queue = new ConcurrentQueue<IProcessableImage>();
                        var t = Task.Run(() => inputMode.Produce(queue));
                        int i = 0;
                        while (true)
                        {
                            if (queue.TryDequeue(out var z))
                            {
                                Console.WriteLine($"i:{i++:d2}");
                            }
                        }*/


            await processingStrategy.Process(inputMode, filters, outputMode);
            st.Stop();
            Console.WriteLine($"Processing took ${st.ElapsedMilliseconds / 1000d}");
        }

        static void GetInfo(InfoOptions info)
        {
            Console.WriteLine(info.ToString());

            static void printSize(Size sz) { Console.WriteLine($"{sz.Width} {sz.Height}"); }
            static void printPixelFormat(PixelFormat pixelFormat) { Console.WriteLine(Enum.GetName(typeof(PixelFormat), pixelFormat)); }

            if (info.UseInputPipe) { Console.Error.WriteLine("UseInputPipe Not supported for options", ConsoleColor.Red, ConsoleColor.Black); }

            if (System.IO.Directory.Exists(info.InputFolder))
            {
                // TODO Adjust search pattern
                var files = Directory.GetFiles(info.InputFolder, "*.jpg", new EnumerationOptions
                {
                    MatchCasing = MatchCasing.CaseInsensitive,
                    IgnoreInaccessible = true
                });
                if (files.Length != 0)
                {
                    printSize(FileReaderHelpers.GetDimensions(files.First()));
                    printPixelFormat(FileReaderHelpers.GetPixelFormat(files.First()));
                    return;
                }
            }

            if (info.InputFiles?.Count() != 0)
            {
                printSize(FileReaderHelpers.GetDimensions(info.InputFiles.First()));
                printPixelFormat(FileReaderHelpers.GetPixelFormat(info.InputFiles.First()));
                return;
            }
            Console.Error.WriteLine("No Input Files could be found", ConsoleColor.Red, ConsoleColor.Black);

        }

    }

    internal static class ConsoleExtensions
    {
        public static void WriteLineColored(this TextWriter consoleOut, string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            var foregroundColor = Console.ForegroundColor;
            var backgroundColor = Console.BackgroundColor;
            Console.ForegroundColor = foreground ?? foregroundColor;
            Console.BackgroundColor = background ?? backgroundColor;

            consoleOut.WriteLine(text);

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }
    }
}
