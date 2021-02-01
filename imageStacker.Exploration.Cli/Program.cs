using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace imageStacker.Exploration.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var folders = System.IO.Directory.GetDirectories(@"H:\timelapses\timelapses").ToArray();
            foreach (var item in folders)
            {
                try
                {
                    var filename = Path.GetFileNameWithoutExtension(item);
                    var resultingFilename = $"H:\\timelapses\\stacki\\{filename}";
                    if (!(File.Exists(resultingFilename + "-MaxVecFilter-.jpg") || File.Exists(resultingFilename + "-MaxVecFilter-.png")))
                    {
                        if (Directory.GetFiles(item).Any(x => x.EndsWith(".tif") || x.EndsWith(".tiff")))
                        {
                            Console.WriteLine("Skipping unsupported tifs");
                            continue;
                        }
                        // ,AttackDecayFilter Attack=1 Decay=0.05 Name=attackH, AttackDecayFilter Attack=0.05 Decay=1 Name=attackL
                        var zargs = $"stackAll --inputFolder={item} --outputFilePrefix={filename} --outputFolder=H:\\timelapses\\stacki --inputFilter=*.* --filters=MaxFilter,MinFilter".Split(' ');
                        await imageStacker.Cli.Program.Main(zargs);
                    }
                    else
                    {
                        Console.WriteLine($"Skipped {filename + "-MaxFilter-.jpg"}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            foreach (var item in folders)
            {
                try
                {
                    // ,AttackDecayFilter Attack=1 Decay=0.05 Name=attackH, AttackDecayFilter Attack=0.05 Decay=1 Name=attackL
                    await imageStacker.Piping.Cli.Program.Main(new string[] { item });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            // var folders = System.IO.Directory.GetDirectories(@"H:\timelapses\timelapses").ToArray();


            /*  var files = System.IO.Directory.GetFiles(@"H:\timelapses\sourceVideos", "*.MOV", SearchOption.AllDirectories).ToArray();
              foreach (var item in files)
              {
                  try
                  {
                      var filename = Path.GetFileNameWithoutExtension(item);
                      var resultingFilename = $"{filename}";
                      //    if (!(File.Exists(resultingFilename + "-max-.jpg") || File.Exists(resultingFilename + "-max-.png")))
                      //    {
                      Console.WriteLine(resultingFilename);
                      // var prc = Process.Start(@"E:\Users\armbe\OneDrive\Dokumente\PlatformIO\Projects\imageStacker\imageStacker.Piping.Cli\bin\x64\Release\net5.0\imageStacker.Piping.Cli.exe", item);
                      // prc.WaitForExit();
                      await imageStacker.Piping.Cli.Program.Main(new string[] { item });
                      //    }
                      //    else
                      //    {
                      //        Console.WriteLine($"Skipped {filename + "-max-.jpg"}");
                      //    }
                  }
                  catch (System.Exception e)
                  {
                      System.Console.WriteLine(e);
                      throw;
                  }
              }*/
        }
    }
}