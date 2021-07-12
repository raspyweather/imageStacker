using CommandLine;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Readers;
using imageStacker.Core.Writers;
using imageStacker.ffmpeg;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace imageStacker.Cli
{
    public static class StackingEnvironmentBuilderExtensions
    {
        private static ILogger CreateLogger(CommonOptions commonOptions)
        {
            if (commonOptions.UseOutputPipe)
            {
                return new Logger(Console.Error);
            }
            return new Logger(Console.Out);
        }

        public static IStackingEnvironment CheckConstraints(this IStackingEnvironment environment)
        {
            if (environment.Filters.Count > 1 && environment.OutputMode.GetType() == typeof(FfmpegVideoWriter))
            {
                environment.Logger?.WriteLine("Multiple filters and therefore video outputs are currently not supported.", Verbosity.Error, true);
                environment.ThrowMe = true;
                return environment;
            }

            return environment;
        }

        public static IStackingEnvironment ConfigureCommonEnvironment(this IStackingEnvironment env, CommonOptions info)
        {
            env.Logger = CreateLogger(info);
            StaticLogger.Instance = env.Logger;

            env.Factory = new MutableByteImageFactory(env.Logger);

            env.ConfigureFilters(info.Filters);

            env.Logger?.WriteLine(info.ToString().Replace(",", Environment.NewLine), Verbosity.Info);

            env.ConfigureInputMode(info);

            env.ConfigureOuptutMode(info);

            return env;
        }

        public static IStackingEnvironment ConfigureFilters(this IStackingEnvironment environment, IEnumerable<string> optionArgs)
        {
            environment.Filters = new List<IFilter<MutableByteImage>>();

            if (optionArgs == null || !optionArgs.Any())
            {
                environment.ThrowMe = true;
                return environment;
            }

            var filteredOptionArgs = string.Join(' ', optionArgs)
                .Replace("--filters=", "")
                .Replace("--filters", "")
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(x => x.Split(',')).ToArray();

            var parameterGroups = new List<List<string>>();
            var previousList = new List<string> { filteredOptionArgs.First() };
            foreach (var item in filteredOptionArgs.Skip(1))
            {
                if (item.Contains("="))
                {
                    previousList.Add("--" + item.Trim(','));
                    continue;
                }
                parameterGroups.Add(previousList);
                previousList = new List<string> { item.Trim(',') };
            }

            parameterGroups.Add(previousList);

            var factory = new MutableByteImageFilterFactory(true);

            foreach (var group in parameterGroups)
            {
                var result = Parser.Default
                        .ParseArguments<MaxFilterOptions,
                                        MinFilterOptions,
                                        CopyFilterOptions,
                                        AttackDecayFilterOptions>(group)
                                        .WithParsed<MaxFilterOptions>(options => environment.Filters.Add(factory.CreateMaxFilter(options)))
                                        .WithParsed<MinFilterOptions>(options => environment.Filters.Add(factory.CreateMinFilter(options)))
                                        .WithParsed<CopyFilterOptions>(options => environment.Filters.Add(factory.CreateCopyFilter(options)))
                                        .WithParsed<AttackDecayFilterOptions>(options => environment.Filters.Add(factory.CreateAttackDecayFilter(options)))
                                        .WithNotParsed(e => Console.Write(e.ToString()));
            }

            return environment;
        }

        public static FfmpegVideoEncoderPreset MapInputToPreset(string input)
        {
            string lowerInput = input.ToLowerInvariant().Trim();
            if (new string[] { "fhd", "fullhd" }.Contains(lowerInput))
            {
                return FfmpegVideoEncoderPreset.FullHD;
            }
            if (new string[] { "4k", "fourk" }.Contains(lowerInput))
            {
                return FfmpegVideoEncoderPreset.FourK;
            }
            if (new string[] { "archive" }.Contains(lowerInput))
            {
                return FfmpegVideoEncoderPreset.Archive;
            }
            if (new string[] { "halfsize" }.Contains(lowerInput))
            {
                return FfmpegVideoEncoderPreset.HalfSize;
            }
            return FfmpegVideoEncoderPreset.None;
        }

        public static IStackingEnvironment ConfigureOuptutMode(this IStackingEnvironment env, CommonOptions commonOptions)
        {
            if (commonOptions.UseOutputPipe)
            {
                env.OutputMode = new ImageStreamWriter<MutableByteImage>(env.Logger, env.Factory, Console.OpenStandardOutput());
                return env;
            }

            if (!string.IsNullOrWhiteSpace(commonOptions.OutputVideoFile))
            {
                if (!string.IsNullOrWhiteSpace(commonOptions.OutputFolder))
                {
                    env.Logger?.WriteLine("OutputFolder Option set, but is being ignored when using Video as output", Verbosity.Warning, true);
                }

                env.OutputMode = new FfmpegVideoWriter(
                    new FfmpegVideoWriterArguments
                    {
                        OutputFile = commonOptions.OutputVideoFile,
                        CustomArgs = commonOptions.OutputVideoOptions,
                        PathToFfmpeg = commonOptions.FfmpegLocation,
                        Preset = MapInputToPreset(commonOptions.OutputPreset)
                    },
                    env.Logger);
                return env;
            }

            if (!string.IsNullOrWhiteSpace(commonOptions.OutputFolder))
            {
                env.OutputMode = new ImageFileWriter<MutableByteImage>(commonOptions.OutputFilePrefix, commonOptions.OutputFolder, env.Factory);
                return env;
            }

            env.Logger?.WriteLine("No Output Mode defined", Verbosity.Error);
            env.Logger?.WriteLine("Consider specifying --UseOutputPipe or --OutputFolder", Verbosity.Error);
            env.Logger?.WriteLine("", Verbosity.Error);
            env.ThrowMe = true;
            return env;
        }

        public static IStackingEnvironment ConfigureInputMode(this IStackingEnvironment env, CommonOptions commonOptions)
        {
            if (!string.IsNullOrWhiteSpace(commonOptions.InputVideoFile))
            {
                env.Logger?.WriteLine("Using Video Input - you might occur some bugs.", Verbosity.Warning);

                env.InputMode = new FfmpegVideoReader(new FfmpegVideoReaderArguments
                {
                    InputFile = commonOptions.InputVideoFile,
                    PathToFfmpeg = commonOptions.FfmpegLocation,
                    CustomArgs = commonOptions.InputVideoArguments
                }, env.Factory, env.Logger);
                return env;
            }
            if (commonOptions.UseInputPipe)
            {
                env.Logger?.WriteLine("Currently only BGR24 input supported", Verbosity.Warning);
                var inputSize = commonOptions.InputSize;
                if (string.IsNullOrWhiteSpace(inputSize))
                {
                    env.ThrowMe = true;
                    env.Logger?.WriteLine($"InputSize is not defined but necessary for using inputpipes", Verbosity.Error);
                }
                var wh = Regex.Split(inputSize ?? string.Empty, "[^0-9]");

                if (int.TryParse(wh[0], out int width) && int.TryParse(wh[1], out int height))
                {
                    env.InputMode = new ImageStreamReader<MutableByteImage>(
                        env.Logger,
                        env.Factory,
                        Console.OpenStandardOutput(width * height * 3),
                        width,
                        height,
                        PixelFormat.Format24bppRgb);
                }
                else
                {
                    env.Logger?.WriteLine($"InputSize is not parseable {commonOptions.InputSize}", Verbosity.Error);
                    env.ThrowMe = true;
                }


                return env;
            }
            if (commonOptions.InputFiles != null && commonOptions.InputFiles.Any())
            {
                env.InputMode = new ImageMutliFileReader<MutableByteImage>(
                    env.Logger,
                    env.Factory,
                    new ReaderOptions { Files = commonOptions.InputFiles.ToArray() },
                    true);

                return env;
            }
            if (!string.IsNullOrWhiteSpace(commonOptions.InputFolder))
            {
                env.Logger?.WriteLine("Currently only *.jpg input supported", Verbosity.Warning);
                if (!Directory.Exists(commonOptions.InputFolder))
                {
                    env.Logger?.WriteLine($"InputFolder does not exist {commonOptions.InputFolder}", Verbosity.Error);
                    env.ThrowMe = true;
                }
                else
                {
                    env.InputMode = new ImageMutliFileReader<MutableByteImage>(
                        env.Logger,
                        env.Factory,
                        new ReaderOptions
                        {
                            FolderName = commonOptions.InputFolder,
                            Filter = commonOptions.InputFilter
                        },
                        true);
                }
                return env;
            }

            env.Logger?.WriteLine("No Input Mode defined", Verbosity.Error);
            env.ThrowMe = true;
            return env;
        }
    }
}
