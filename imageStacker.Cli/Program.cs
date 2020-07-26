﻿using CommandLine;
using imageStacker.Core;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using imageStacker.Core.Readers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
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
            //  args = @"stackImage --inputFolder=L:\Canada\timelapses\202006242150 --outputFile=202006242150 --outputFolder=.".Split(' ');
            // args = @"stackImage --inputFolder=F:\2020 --outputFile=2020F --outputFolder=.".Split(' ');
            //  args = @"stackContinuous --inputFolder=F:\2020 --outputFile=2020 --StackCount=5 --outputFolder=.".Split(' ');
            //args = @"stackContinuous --StackCount=5 --inputFolder=L:\Canada\timelapses\202007100014 --outputToPipe".Split(' ');
            //args = @"stackImage --inputFolder=C:\202007192249 --outputFile=2020F --outputFolder=.".Split(' ');
            args = @"stackProgressive --inputFolder=C:\202007192249 --outputFile=2020F --outputFolder=.".Split(' ');
            //   args = @"test".Split(' ');
            Stopwatch st = new Stopwatch();
            st.Start();

            IMutableImageFactory<MutableByteImage> factory = null;
            IImageReader<MutableByteImage> inputMode = null;
            IImageWriter<MutableByteImage> outputMode = null;
            ILogger logger = null;

            List<IFilter<MutableByteImage>> filters = new List<IFilter<MutableByteImage>> { new MaxFilter() };
            IImageProcessingStrategy<MutableByteImage> processingStrategy = null;
            bool throwMe = false;

            void setupCommons(CommonOptions info)
            {
                logger = CreateLogger(info);
                factory = new MutableByteImageFactory(logger);

                logger?.WriteLine(info.ToString().Replace(",", Environment.NewLine), Verbosity.Info);
                bool throwMeHere = false;

                (inputMode, throwMeHere) = SetInput(info, factory, logger);
                throwMe |= throwMeHere;

                (outputMode, throwMeHere) = SetOutput(info, logger, factory);
                throwMe |= throwMeHere;
            }

            var result = Parser.Default
                .ParseArguments<InfoOptions,
                                StackAllOptions,
                                StackProgressiveOptions,
                                StackContinuousOptions,
                                TestOptions>(args)
                .WithParsed<InfoOptions>(info =>
                {
                    GetInfo(info);
                    return;
                })
                .WithParsed<StackAllOptions>(info =>
                {
                    setupCommons(info);

                    // todo switch between stackall and stackallmerge, if supported by filter
                    // processingStrategy = new StackAllStrategy();
                    // might be unsafe

                    processingStrategy = new StackAllMergeStrategy<MutableByteImage>(logger, factory);

                })
                .WithParsed<StackProgressiveOptions>(info =>
                {
                    setupCommons(info);
                    processingStrategy = new StackProgressiveStrategy<MutableByteImage>(logger, factory);
                })
                .WithParsed<StackContinuousOptions>(info =>
                {
                    setupCommons(info);

                    if (info.Count == 0)
                    {
                        logger?.WriteLine("You have to define --stackCount for continuous stacking", Verbosity.Error);
                        throwMe = true;
                    }

                    processingStrategy = new StackContinousStrategy<MutableByteImage>(info.Count, logger, factory);
                })
                .WithParsed<TestOptions>(async info =>
              {
                  info.UseOutputPipe = false;
                  info.OutputFile = ".";
                  info.OutputFolder = ".";

                  logger = new Logger(Console.Out);

                  factory = new MutableByteImageFactory(logger);

                  inputMode = new TestImageReadeer<MutableByteImage>
                     (info.Count,
                     info.Width,
                     info.Height,
                     PixelFormat.Format24bppRgb,
                     logger,
                     factory);

                  outputMode = new TestImageWriter<MutableByteImage>(logger, factory);

                  processingStrategy = new StackAllMergeStrategy<MutableByteImage>(logger, factory);

                  await RunBenchmark(info, logger, new MutableByteImageFactory(logger));

                  Process.GetCurrentProcess().Close();
              })
                .WithNotParsed(x =>
                    logger?.WriteLine(String.Join(Environment.NewLine, x.Select(y => y.ToString()).ToArray()), Verbosity.Error));

            if (throwMe)
            {
                logger?.WriteLine("Invalid Configuration, see issues above.", Verbosity.Error);
                return;
            }

            if (inputMode == null || outputMode == null)
            {
                logger?.WriteLine($"Input: {inputMode}, Output:{outputMode}", Verbosity.Error);
                logger?.WriteLine("IO undefined", Verbosity.Error);
                return;
            }
            if (processingStrategy == null)
            {
                logger?.WriteLine("Not processing strategy defined", Verbosity.Error);
            }

            logger?.WriteLine($"{inputMode} {outputMode} {processingStrategy}", Verbosity.Info);

            await processingStrategy.Process(inputMode, filters, outputMode);
            st.Stop();
            logger?.WriteLine($"Processing took {st.ElapsedMilliseconds / 1000d}", Verbosity.Info);

            logger?.Dispose();
        }

        private static (IImageWriter<MutableByteImage>, bool throwMe) SetOutput(CommonOptions commonOptions, ILogger logger, IMutableImageFactory<MutableByteImage> factory)
        {
            if (commonOptions.UseOutputPipe)
            {
                return (new ImageStreamWriter<MutableByteImage>(logger, factory, Console.OpenStandardOutput()), false);
            }

            if (!string.IsNullOrWhiteSpace(commonOptions.OutputFolder) &&
                !string.IsNullOrWhiteSpace(commonOptions.OutputFile))
            {
                return (new ImageFileWriter<MutableByteImage>(commonOptions.OutputFile, commonOptions.OutputFolder, factory), false);
            }

            logger?.WriteLine("No Output Mode defined", Verbosity.Error);
            logger?.WriteLine("Consider specifying --UseOutputPipe or --OutputFolder and --OutputFile", Verbosity.Error);
            logger?.WriteLine("", Verbosity.Error);
            return (null, true);
        }

        private static (IImageReader<MutableByteImage>, bool throwMe) SetInput(CommonOptions commonOptions, IMutableImageFactory<MutableByteImage> factory, ILogger logger)
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

                return (new ImageStreamReader<MutableByteImage>(
                    logger,
                    factory,
                    Console.OpenStandardOutput(width * height * 3),
                    width,
                    height,
                    PixelFormat.Format24bppRgb), false);
            }
            if (commonOptions.InputFiles != null && commonOptions.InputFiles.Count() > 0)
            {
                return (new ImageFileReader<MutableByteImage>(logger, factory, commonOptions.InputFiles.ToArray()), false);
            }
            if (!string.IsNullOrWhiteSpace(commonOptions.InputFolder))
            {
                logger?.WriteLine("Currently only *.jpg input supported", Verbosity.Warning);
                if (!Directory.Exists(commonOptions.InputFolder))
                {
                    logger?.WriteLine($"InputFolder does not exist {commonOptions.InputFolder}", Verbosity.Warning);
                }
                return (new ImageFileReader<MutableByteImage>(logger, factory, commonOptions.InputFolder), false);
            }

            logger?.WriteLine("No Input Mode defined", Verbosity.Error);
            return (null, true);
        }

        private static ILogger CreateLogger(CommonOptions commonOptions)
        {
            if (commonOptions.UseOutputPipe)
            {
                return new Logger(Console.Error);
            }
            return new Logger(Console.Out);
        }

        private static async Task RunBenchmark(TestOptions options, ILogger logger, IMutableImageFactory<MutableByteImage> factory)
        {
            try
            {
                logger.WriteLine("Running Benchmarks", Verbosity.Info);

                logger.WriteLine("Testing Filters", Verbosity.Info);

                foreach (var filter in new List<IFilter<MutableByteImage>> { new MinFilter(), new MinVecFilter(), new MaxFilter(), new MaxVecFilter(),
                    new MinFilter(), new MinVecFilter(), new MaxFilter(), new MaxVecFilter() })
                {
                    Stopwatch stopwatch = new Stopwatch();
                    var imageReadeer = new TestImageReadeer<MutableByteImage>
                        (options.Count,
                        options.Width,
                        options.Height,
                        PixelFormat.Format24bppRgb,
                        logger,
                        factory);
                    var imageWriter = new ImageFileWriter<MutableByteImage>("name", ".", factory);
                    var strategy = new StackAllMergeStrategy<MutableByteImage>(logger, factory);
                    stopwatch.Start();
                    await strategy.Process(imageReadeer, new List<IFilter<MutableByteImage>> { filter }, imageWriter);
                    stopwatch.Stop();
                    logger.WriteLine($"Fitler {filter.Name} took {stopwatch.ElapsedMilliseconds}", Verbosity.Info);
                }

            }
            catch (Exception e)
            {
                logger.LogException(e);
                throw;
            }

        }
        private static void GetInfo(InfoOptions info)
        {
            Console.Error.WriteLine(info.ToString());

            static void printSize(Size sz) { Console.Error.WriteLine($"{sz.Width} {sz.Height}"); }
            static void printPixelFormat(PixelFormat pixelFormat) { Console.Error.WriteLine(Enum.GetName(typeof(PixelFormat), pixelFormat)); }

            if (info.UseInputPipe) { Console.Error.WriteLine("UseInputPipe Not supported for options"); }

            if (Directory.Exists(info.InputFolder))
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
            Console.Error.WriteLine("No Input Files could be found");
        }
    }
}
