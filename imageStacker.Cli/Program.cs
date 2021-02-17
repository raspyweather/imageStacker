using CommandLine;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using imageStacker.Core.ByteImage.Filters;
using imageStacker.Core.Readers;
using imageStacker.Core.Writers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Cli
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            var env = GetBasicEnvironment();

            StaticLogger.Instance = new Logger(Console.Out, Verbosity.Info);

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
                    env.ConfigureCommonEnvironment(info);

                    // todo switch between stackall and stackallmerge, if supported by filter
                    // processingStrategy = new StackAllStrategy();
                    // might be unsafe

                    env.ProcessingStrategy = new StackAllMergeStrategy<MutableByteImage>(env.Logger, env.Factory);

                })
                .WithParsed<StackProgressiveOptions>(info =>
                {
                    env.ConfigureCommonEnvironment(info);
                    env.ProcessingStrategy = new StackProgressiveStrategy<MutableByteImage>(env.Logger, env.Factory);
                })
                .WithParsed<StackContinuousOptions>(info =>
                {
                    env.ConfigureCommonEnvironment(info);

                    if (info.Count == 0)
                    {
                        env.Logger?.WriteLine("You have to define --stackCount for continuous stacking", Verbosity.Error);
                        env.ThrowMe = true;
                    }

                    env.ProcessingStrategy = new StackContinousStrategy<MutableByteImage>(info.Count, env.Logger, env.Factory);
                })
                .WithParsed<TestOptions>(info =>
                {
                    info.UseOutputPipe = false;
                    info.OutputFolder = ".";

                    env.Logger = new Logger(Console.Out, Verbosity.Info);

                    env.Factory = new MutableByteImageFactory(env.Logger);

                    env.InputMode = new TestImageReader<MutableByteImage>
                        (info.Count,
                        info.Width,
                        info.Height,
                        PixelFormat.Format24bppRgb,
                        env.Logger,
                        env.Factory);

                    env.OutputMode = new TestImageWriter<MutableByteImage>(env.Logger, env.Factory);

                    env.ProcessingStrategy = new StackAllSimpleStrategy<MutableByteImage>(env.Logger, env.Factory);

                    RunBenchmark(info, env.Logger, new MutableByteImageFactory(env.Logger)).Wait();

                    Process.GetCurrentProcess().Close();
                }).WithNotParsed(x =>
                       env.Logger?.WriteLine(String.Join(Environment.NewLine, x.Select(y => y.ToString()).ToArray()), Verbosity.Error));

            if (env.ThrowMe)
            {
                env.Logger?.WriteLine("Invalid Configuration, see issues above.", Verbosity.Error);
                return;
            }

            if (env.InputMode == null || env.OutputMode == null)
            {
                env.Logger?.WriteLine($"Input: {env.InputMode}, Output:{env.OutputMode}", Verbosity.Error);
                env.Logger?.WriteLine("IO undefined", Verbosity.Error);
                return;
            }

            if (env.ProcessingStrategy == null)
            {
                env.Logger?.WriteLine("Not processing strategy defined", Verbosity.Error);
            }

            env.Logger?.WriteLine($"{env.InputMode} {env.OutputMode} {env.ProcessingStrategy}", Verbosity.Info);

            env.CheckConstraints();

            await env.ProcessingStrategy.Process(env.InputMode, env.Filters, env.OutputMode);
            st.Stop();
            env.Logger?.WriteLine($"Processing took {st.ElapsedMilliseconds / 1000d}", Verbosity.Info);

            env.Logger?.Dispose();
        }

        private static async Task RunBenchmark(TestOptions options, ILogger logger, IMutableImageFactory<MutableByteImage> factory)
        {
            try
            {
                logger.WriteLine("Running Benchmarks", Verbosity.Info);

                logger.WriteLine("Testing Filters", Verbosity.Info);

                foreach (var filter in new List<IFilter<MutableByteImage>> {
                    new MinFilter(new MinFilterOptions{ }),
                    new MinVecFilter(new MinFilterOptions{ }),
                    new MaxFilter(new MaxFilterOptions{ }),
                    new MaxVecFilter(new MaxFilterOptions{ }),
                    new AttackDecayFilter( new AttackDecayFilterOptions{ }),
                    new AttackDecayVecFilter(new AttackDecayFilterOptions{ }) })
                {
                    Stopwatch stopwatch = new Stopwatch();


                    var imageReader = new TestImageReader<MutableByteImage>
                                          (options.Count,
                                          options.Width,
                                          options.Height,
                                          PixelFormat.Format24bppRgb,
                                          logger,
                                          factory);
                    var imageWriter = new ImageStreamWriter<MutableByteImage>(logger, factory, Stream.Null);
                    var strategy = new StackAllMergeStrategy<MutableByteImage>(logger, factory);

                    stopwatch.Start();
                    await strategy.Process(imageReader, new List<IFilter<MutableByteImage>> { filter }, imageWriter);
                    stopwatch.Stop();
                    logger.WriteLine($"Filter {filter.Name} took {stopwatch.ElapsedMilliseconds}", Verbosity.Info);
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
        private static IStackingEnvironment GetBasicEnvironment() => new StackingEnvironment();
    }
}
