using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using imageStacker.Core.Readers;
using imageStacker.ffmpeg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.Piping.Cli
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            Console.WriteLine(args[0]);

            var filterFactory = new MutableByteImageFilterFactory(true);
            var filters = new List<IFilter<MutableByteImage>>
                {
                  filterFactory.CreateMaxFilter(new MaxOptions
                          {
                              Name ="max",
                          }),
                   filterFactory.CreateAttackDecayFilter(new AttackOptions
                    {
                        Attack = 1f,
                        Decay= 0.2f,
                        Name ="attackHF",
                    }),
                    filterFactory.CreateAttackDecayFilter(new AttackOptions
                    {
                        Attack = 1f,
                        Decay= 0.01f,
                        Name ="attackHS",
                    }),
                         filterFactory.CreateAttackDecayFilter(new AttackOptions
                      {
                          Attack = 0.2f,
                          Decay= 1f,
                          Name ="attackLF",
                      }),
                     filterFactory.CreateAttackDecayFilter(new AttackOptions
                      {
                          Attack = 0.01f,
                          Decay= 1f,
                          Name ="attackLS",
                      }),
                      filterFactory.CreateMinFilter(new MinOptions
                      {
                          Name ="min",
                      }),
                };

            var str = args[0];
            foreach (var filter in filters)
            {
                using var logger = new Logger(Console.Out);
                StaticLogger.Instance = logger;
                Console.WriteLine(str);
                var filename = Path.GetFileNameWithoutExtension(str);
                var factory = new MutableByteImageFactory(logger);
             /*   var reader = new FfmpegVideoReader(new FfmpegVideoReaderArguments
                {
                    InputFile = str
                }, factory, logger);*/

                var directoryName = (File.Exists(args[0])) ? Path.GetDirectoryName(args[0]) : args[0];

                var commonExtension = Directory.GetFiles(directoryName)
                    .Select(x => Path.GetExtension(x))
                    .GroupBy(x => x)
                    .OrderBy(x => x.Count()).First();

                var reader = new ImageMutliFileReader<MutableByteImage>(logger, factory, new ReaderOptions
                    {
                        FolderName = args[0],
                        Filter = $"*{commonExtension.Key}",
                    },
                    true);

                var processingStrategy = new StackProgressiveStrategy<MutableByteImage>(logger, factory);


                System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
                st.Start();

                //var writer = new ImageFileWriter<MutableByteImage>(filename, ".", factory);

                var resultingFilename =/* @"H:\timelapses\stackedVideo\" +*/ Path.GetRandomFileName() + filename + "-" + filter.Name + ".mp4";
                if (File.Exists(resultingFilename))
                {
                    st.Stop();
                    Console.WriteLine($"Skipped {resultingFilename}");
                    continue;
                }

                var writer = new FfmpegVideoWriter(new FfmpegVideoWriterArguments
                {
                    Framerate = 60,
                    OutputFile = resultingFilename
                }, logger);

                await processingStrategy.Process(reader, new List<IFilter<MutableByteImage>> { filter }, writer);

                st.Stop();
                Console.WriteLine($"it took {st.ElapsedMilliseconds}ms");
            }
        }
    }
}
